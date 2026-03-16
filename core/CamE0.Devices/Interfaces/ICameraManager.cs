using CamE0.Devices.Models;

namespace CamE0.Devices.Interfaces;

public interface ICameraManager
{
    Task<Camera> AddCameraAsync(Camera camera);
    Task<Camera?> GetCameraAsync(string id);
    Task<IReadOnlyList<Camera>> GetAllCamerasAsync();
    Task<Camera> UpdateCameraAsync(Camera camera);
    Task RemoveCameraAsync(string id);
    Task<CameraStatus> GetCameraStatusAsync(string id);
    Task<bool> TestConnectionAsync(string rtspUrl, string? username = null, string? password = null);
}
