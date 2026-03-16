using CamE0.Storage.Models;

namespace CamE0.Video.Interfaces;

public interface IRecordingService
{
    Task<Recording> StartRecordingAsync(string cameraId, string rtspUrl, RecordingType type = RecordingType.Continuous);
    Task<Recording?> StopRecordingAsync(string cameraId);
    Task<bool> IsRecordingAsync(string cameraId);
    Task<IReadOnlyList<Recording>> GetActiveRecordingsAsync();
}
