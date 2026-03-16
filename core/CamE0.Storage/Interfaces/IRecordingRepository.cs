using CamE0.Storage.Models;

namespace CamE0.Storage.Interfaces;

public interface IRecordingRepository
{
    Task<Recording?> GetByIdAsync(string id);
    Task<IReadOnlyList<Recording>> GetByCameraIdAsync(string cameraId, DateTime? from = null, DateTime? to = null);
    Task<IReadOnlyList<Recording>> GetAllAsync(DateTime? from = null, DateTime? to = null);
    Task<Recording> AddAsync(Recording recording);
    Task<Recording> UpdateAsync(Recording recording);
    Task DeleteAsync(string id);
}
