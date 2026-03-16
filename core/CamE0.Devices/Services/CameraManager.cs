using CamE0.Devices.Interfaces;
using CamE0.Devices.Models;
using CamE0.Security.Interfaces;
using Microsoft.Extensions.Logging;

namespace CamE0.Devices.Services;

public sealed class CameraManager : ICameraManager
{
    private readonly ICameraRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<CameraManager> _logger;
    private const int MaxCameras = 32;

    public CameraManager(
        ICameraRepository repository,
        IAuditLogger auditLogger,
        ILogger<CameraManager> logger)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Camera> AddCameraAsync(Camera camera)
    {
        var count = await _repository.GetCountAsync();
        if (count >= MaxCameras)
        {
            throw new InvalidOperationException($"Maximum number of cameras ({MaxCameras}) reached.");
        }

        if (string.IsNullOrEmpty(camera.RtspUrl) && !string.IsNullOrEmpty(camera.IpAddress))
        {
            camera.RtspUrl = BuildRtspUrl(camera);
        }

        camera.Status = CameraStatus.Offline;
        var result = await _repository.AddAsync(camera);

        await _auditLogger.LogAsync(new AuditEntry(
            DateTime.UtcNow, "Devices", "CameraAdded",
            "system", $"Camera '{camera.Name}' added at {camera.IpAddress}"));

        _logger.LogInformation("Camera {Name} added with ID {Id}", camera.Name, camera.Id);
        return result;
    }

    public async Task<Camera?> GetCameraAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<Camera>> GetAllCamerasAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Camera> UpdateCameraAsync(Camera camera)
    {
        var existing = await _repository.GetByIdAsync(camera.Id)
            ?? throw new InvalidOperationException($"Camera with ID {camera.Id} not found.");

        if (string.IsNullOrEmpty(camera.RtspUrl) && !string.IsNullOrEmpty(camera.IpAddress))
        {
            camera.RtspUrl = BuildRtspUrl(camera);
        }

        var result = await _repository.UpdateAsync(camera);

        await _auditLogger.LogAsync(new AuditEntry(
            DateTime.UtcNow, "Devices", "CameraUpdated",
            "system", $"Camera '{camera.Name}' updated"));

        return result;
    }

    public async Task RemoveCameraAsync(string id)
    {
        var camera = await _repository.GetByIdAsync(id);
        await _repository.DeleteAsync(id);

        await _auditLogger.LogAsync(new AuditEntry(
            DateTime.UtcNow, "Devices", "CameraRemoved",
            "system", $"Camera '{camera?.Name ?? id}' removed"));
    }

    public async Task<CameraStatus> GetCameraStatusAsync(string id)
    {
        var camera = await _repository.GetByIdAsync(id);
        return camera?.Status ?? CameraStatus.Offline;
    }

    public Task<bool> TestConnectionAsync(string rtspUrl, string? username = null, string? password = null)
    {
        // Basic validation of RTSP URL format
        if (string.IsNullOrEmpty(rtspUrl))
            return Task.FromResult(false);

        if (!rtspUrl.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(false);

        return Task.FromResult(true);
    }

    private static string BuildRtspUrl(Camera camera)
    {
        var auth = !string.IsNullOrEmpty(camera.Username)
            ? $"{camera.Username}:{camera.Password}@"
            : string.Empty;
        return $"rtsp://{auth}{camera.IpAddress}:{camera.Port}/stream1";
    }
}
