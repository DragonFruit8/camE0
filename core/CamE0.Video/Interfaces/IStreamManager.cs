using CamE0.Video.Models;

namespace CamE0.Video.Interfaces;

public interface IStreamManager
{
    Task<VideoStream> StartStreamAsync(string cameraId, string rtspUrl);
    Task StopStreamAsync(string cameraId);
    Task<VideoStream?> GetStreamAsync(string cameraId);
    Task<IReadOnlyList<VideoStream>> GetAllStreamsAsync();
    Task<bool> IsStreamingAsync(string cameraId);
}
