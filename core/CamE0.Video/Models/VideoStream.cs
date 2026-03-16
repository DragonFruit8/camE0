namespace CamE0.Video.Models;

public sealed class VideoStream
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CameraId { get; set; } = string.Empty;
    public string RtspUrl { get; set; } = string.Empty;
    public StreamStatus Status { get; set; } = StreamStatus.Stopped;
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public int Fps { get; set; } = 30;
    public string Codec { get; set; } = "h264";
    public int Bitrate { get; set; } = 4000;
    public DateTime? StartedAt { get; set; }
    public int? ProcessId { get; set; }
}

public enum StreamStatus
{
    Stopped,
    Connecting,
    Streaming,
    Recording,
    Error
}
