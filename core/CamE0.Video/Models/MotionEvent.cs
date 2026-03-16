namespace CamE0.Video.Models;

public sealed class MotionEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CameraId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double MotionScore { get; set; }
    public int Threshold { get; set; }
    public string? SnapshotPath { get; set; }
    public MotionRegion? Region { get; set; }
}

public sealed class MotionRegion
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
