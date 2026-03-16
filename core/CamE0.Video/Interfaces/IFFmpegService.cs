namespace CamE0.Video.Interfaces;

public interface IFFmpegService
{
    Task<int> StartRtspIngestAsync(string rtspUrl, string outputPath, CancellationToken cancellationToken = default);
    Task StopProcessAsync(int processId);
    Task<byte[]?> CaptureSnapshotAsync(string rtspUrl);
    Task<bool> IsFFmpegAvailableAsync();
    Task<string?> GetStreamInfoAsync(string rtspUrl);
}
