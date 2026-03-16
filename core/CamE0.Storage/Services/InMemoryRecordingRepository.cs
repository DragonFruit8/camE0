using System.Collections.Concurrent;
using CamE0.Storage.Interfaces;
using CamE0.Storage.Models;

namespace CamE0.Storage.Services;

public sealed class InMemoryRecordingRepository : IRecordingRepository
{
    private readonly ConcurrentDictionary<string, Recording> _recordings = new();

    public Task<Recording?> GetByIdAsync(string id)
    {
        _recordings.TryGetValue(id, out var recording);
        return Task.FromResult(recording);
    }

    public Task<IReadOnlyList<Recording>> GetByCameraIdAsync(string cameraId, DateTime? from = null, DateTime? to = null)
    {
        var query = _recordings.Values.Where(r => r.CameraId == cameraId);
        if (from.HasValue) query = query.Where(r => r.StartTime >= from.Value);
        if (to.HasValue) query = query.Where(r => r.StartTime <= to.Value);
        IReadOnlyList<Recording> result = query.OrderByDescending(r => r.StartTime).ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Recording>> GetAllAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _recordings.Values.AsEnumerable();
        if (from.HasValue) query = query.Where(r => r.StartTime >= from.Value);
        if (to.HasValue) query = query.Where(r => r.StartTime <= to.Value);
        IReadOnlyList<Recording> result = query.OrderByDescending(r => r.StartTime).ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<Recording> AddAsync(Recording recording)
    {
        if (!_recordings.TryAdd(recording.Id, recording))
        {
            throw new InvalidOperationException($"Recording with ID {recording.Id} already exists.");
        }
        return Task.FromResult(recording);
    }

    public Task<Recording> UpdateAsync(Recording recording)
    {
        _recordings[recording.Id] = recording;
        return Task.FromResult(recording);
    }

    public Task DeleteAsync(string id)
    {
        _recordings.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
