using CamE0.Devices.Models;

namespace CamE0.Devices.Interfaces;

public interface ICameraRepository
{
    Task<Camera?> GetByIdAsync(string id);
    Task<IReadOnlyList<Camera>> GetAllAsync();
    Task<Camera> AddAsync(Camera camera);
    Task<Camera> UpdateAsync(Camera camera);
    Task DeleteAsync(string id);
    Task<int> GetCountAsync();
}
