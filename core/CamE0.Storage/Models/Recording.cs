namespace CamE0.Storage.Models;

public sealed class Recording
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CameraId { get; set; } = string.Empty;
    public string CameraName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long FileSizeBytes { get; set; }
    public RecordingType Type { get; set; } = RecordingType.Continuous;
    public RecordingStatus Status { get; set; } = RecordingStatus.Recording;
    public string? TriggerReason { get; set; }
}

public enum RecordingType
{
    Continuous,
    MotionTriggered,
    Manual,
    Scheduled
}

public enum RecordingStatus
{
    Recording,
    Completed,
    Failed,
    Deleted
}
