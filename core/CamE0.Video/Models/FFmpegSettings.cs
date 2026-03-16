namespace CamE0.Video.Models;

public sealed class FFmpegSettings
{
    public string FFmpegPath { get; set; } = "ffmpeg";
    public string FFprobePath { get; set; } = "ffprobe";
    public string OutputFormat { get; set; } = "mp4";
    public string VideoCodec { get; set; } = "copy";
    public string AudioCodec { get; set; } = "aac";
    public int SegmentDurationSeconds { get; set; } = 300;
    public int ReconnectDelaySeconds { get; set; } = 5;
    public int MaxReconnectAttempts { get; set; } = 10;
    public string RtspTransport { get; set; } = "tcp";
    public Dictionary<string, string> ExtraArgs { get; set; } = new();
}
