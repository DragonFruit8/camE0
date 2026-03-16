namespace CamE0.PluginSdk;

/// <summary>
/// Interface for handling system events in plugins.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : SystemEvent
{
    Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for all system events.
/// </summary>
public abstract class SystemEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;
}

public sealed class CameraConnectedEvent : SystemEvent
{
    public string CameraId { get; set; } = string.Empty;
    public string CameraName { get; set; } = string.Empty;
}

public sealed class MotionDetectedEvent : SystemEvent
{
    public string CameraId { get; set; } = string.Empty;
    public double MotionScore { get; set; }
}

public sealed class RecordingStartedEvent : SystemEvent
{
    public string CameraId { get; set; } = string.Empty;
    public string RecordingId { get; set; } = string.Empty;
}

public sealed class RecordingStoppedEvent : SystemEvent
{
    public string CameraId { get; set; } = string.Empty;
    public string RecordingId { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
