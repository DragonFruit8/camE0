using System.Collections.Concurrent;
using CamE0.Devices.Interfaces;
using CamE0.Devices.Models;

namespace CamE0.Devices.Services;

public sealed class InMemoryCameraRepository : ICameraRepository
{
    private readonly ConcurrentDictionary<string, Camera> _cameras = new();

    public Task<Camera?> GetByIdAsync(string id)
    {
        _cameras.TryGetValue(id, out var camera);
        return Task.FromResult(camera);
    }

    public Task<IReadOnlyList<Camera>> GetAllAsync()
    {
        IReadOnlyList<Camera> cameras = _cameras.Values.ToList().AsReadOnly();
        return Task.FromResult(cameras);
    }

    public Task<Camera> AddAsync(Camera camera)
    {
        if (!_cameras.TryAdd(camera.Id, camera))
        {
            throw new InvalidOperationException($"Camera with ID {camera.Id} already exists.");
        }
        return Task.FromResult(camera);
    }

    public Task<Camera> UpdateAsync(Camera camera)
    {
        _cameras[camera.Id] = camera;
        return Task.FromResult(camera);
    }

    public Task DeleteAsync(string id)
    {
        _cameras.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<int> GetCountAsync()
    {
        return Task.FromResult(_cameras.Count);
    }
}
