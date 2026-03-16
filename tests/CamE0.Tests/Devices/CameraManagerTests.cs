using CamE0.Devices.Interfaces;
using CamE0.Devices.Models;
using CamE0.Devices.Services;
using CamE0.Security.Interfaces;
using CamE0.Security.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CamE0.Tests.Devices;

public class CameraManagerTests
{
    private readonly ICameraManager _cameraManager;
    private readonly ICameraRepository _repository;

    public CameraManagerTests()
    {
        _repository = new InMemoryCameraRepository();
        var auditLogger = new InMemoryAuditLogger(new Mock<ILogger<InMemoryAuditLogger>>().Object);
        var logger = new Mock<ILogger<CameraManager>>().Object;
        _cameraManager = new CameraManager(_repository, auditLogger, logger);
    }

    [Fact]
    public async Task AddCameraAsync_ValidCamera_ReturnsCamera()
    {
        var camera = new Camera { Name = "Test Camera", IpAddress = "192.168.1.100", Port = 554 };
        var result = await _cameraManager.AddCameraAsync(camera);

        Assert.NotNull(result);
        Assert.Equal("Test Camera", result.Name);
        Assert.False(string.IsNullOrEmpty(result.RtspUrl));
    }

    [Fact]
    public async Task GetAllCamerasAsync_ReturnsAddedCameras()
    {
        await _cameraManager.AddCameraAsync(new Camera { Name = "Camera 1", IpAddress = "192.168.1.1" });
        await _cameraManager.AddCameraAsync(new Camera { Name = "Camera 2", IpAddress = "192.168.1.2" });

        var cameras = await _cameraManager.GetAllCamerasAsync();
        Assert.Equal(2, cameras.Count);
    }

    [Fact]
    public async Task GetCameraAsync_ExistingCamera_ReturnsCamera()
    {
        var camera = await _cameraManager.AddCameraAsync(new Camera { Name = "Test", IpAddress = "192.168.1.1" });
        var result = await _cameraManager.GetCameraAsync(camera.Id);

        Assert.NotNull(result);
        Assert.Equal(camera.Id, result.Id);
    }

    [Fact]
    public async Task RemoveCameraAsync_RemovesCamera()
    {
        var camera = await _cameraManager.AddCameraAsync(new Camera { Name = "Test", IpAddress = "192.168.1.1" });
        await _cameraManager.RemoveCameraAsync(camera.Id);

        var result = await _cameraManager.GetCameraAsync(camera.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task TestConnectionAsync_ValidRtspUrl_ReturnsTrue()
    {
        var result = await _cameraManager.TestConnectionAsync("rtsp://192.168.1.1:554/stream1");
        Assert.True(result);
    }

    [Fact]
    public async Task TestConnectionAsync_InvalidUrl_ReturnsFalse()
    {
        var result = await _cameraManager.TestConnectionAsync("http://invalid");
        Assert.False(result);
    }

    [Fact]
    public async Task TestConnectionAsync_EmptyUrl_ReturnsFalse()
    {
        var result = await _cameraManager.TestConnectionAsync("");
        Assert.False(result);
    }
}
