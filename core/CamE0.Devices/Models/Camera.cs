namespace CamE0.Devices.Models;

public sealed class Camera
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 554;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RtspUrl { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = string.Empty;
    public CameraStatus Status { get; set; } = CameraStatus.Offline;
    public bool MotionDetectionEnabled { get; set; } = true;
    public int MotionSensitivity { get; set; } = 50;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeenAt { get; set; }
    public CameraCapabilities Capabilities { get; set; } = new();
}

public enum CameraStatus
{
    Online,
    Offline,
    Recording,
    Error
}

public sealed class CameraCapabilities
{
    public bool SupportsPtz { get; set; }
    public bool SupportsAudio { get; set; }
    public bool SupportsMotionDetection { get; set; } = true;
    public List<string> SupportedResolutions { get; set; } = new() { "1920x1080", "1280x720" };
    public List<string> SupportedCodecs { get; set; } = new() { "H.264", "H.265" };
}
