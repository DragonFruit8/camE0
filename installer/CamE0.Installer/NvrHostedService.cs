using CamE0.Devices.Interfaces;
using CamE0.Security.Interfaces;
using CamE0.Video.Interfaces;

namespace CamE0.Installer;

/// <summary>
/// Background service that manages the NVR system lifecycle.
/// Runs as a Windows service or Linux daemon.
/// </summary>
public sealed class NvrHostedService : BackgroundService
{
    private readonly ICameraManager _cameraManager;
    private readonly IStreamManager _streamManager;
    private readonly IMotionDetector _motionDetector;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<NvrHostedService> _logger;

    public NvrHostedService(
        ICameraManager cameraManager,
        IStreamManager streamManager,
        IMotionDetector motionDetector,
        IAuditLogger auditLogger,
        ILogger<NvrHostedService> logger)
    {
        _cameraManager = cameraManager;
        _streamManager = streamManager;
        _motionDetector = motionDetector;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CamE0 NVR Service starting...");

        await _auditLogger.LogAsync(new CamE0.Security.Interfaces.AuditEntry(
            DateTime.UtcNow, "System", "ServiceStarted",
            "system", "CamE0 NVR Service started"));

        // Monitor cameras and restart streams as needed
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorCamerasAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NVR monitoring loop");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("CamE0 NVR Service stopping...");

        await _auditLogger.LogAsync(new CamE0.Security.Interfaces.AuditEntry(
            DateTime.UtcNow, "System", "ServiceStopped",
            "system", "CamE0 NVR Service stopped"));
    }

    private async Task MonitorCamerasAsync(CancellationToken stoppingToken)
    {
        var cameras = await _cameraManager.GetAllCamerasAsync();
        foreach (var camera in cameras)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var isStreaming = await _streamManager.IsStreamingAsync(camera.Id);
            if (!isStreaming && camera.Status != CamE0.Devices.Models.CameraStatus.Offline)
            {
                _logger.LogInformation("Camera {Name} is not streaming, attempting reconnect...", camera.Name);
                try
                {
                    await _streamManager.StartStreamAsync(camera.Id, camera.RtspUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to reconnect camera {Name}", camera.Name);
                }
            }
        }
    }
}
