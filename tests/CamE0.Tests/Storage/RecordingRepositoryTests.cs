using CamE0.Storage.Interfaces;
using CamE0.Storage.Models;
using CamE0.Storage.Services;

namespace CamE0.Tests.Storage;

public class RecordingRepositoryTests
{
    private readonly IRecordingRepository _repository = new InMemoryRecordingRepository();

    [Fact]
    public async Task AddAsync_NewRecording_CanBeRetrieved()
    {
        var recording = new Recording
        {
            CameraId = "cam1",
            StartTime = DateTime.UtcNow,
            Type = RecordingType.Continuous,
            Status = RecordingStatus.Recording
        };

        await _repository.AddAsync(recording);
        var result = await _repository.GetByIdAsync(recording.Id);

        Assert.NotNull(result);
        Assert.Equal("cam1", result.CameraId);
    }

    [Fact]
    public async Task GetByCameraIdAsync_FiltersByCameraId()
    {
        await _repository.AddAsync(new Recording { CameraId = "cam1", StartTime = DateTime.UtcNow });
        await _repository.AddAsync(new Recording { CameraId = "cam2", StartTime = DateTime.UtcNow });
        await _repository.AddAsync(new Recording { CameraId = "cam1", StartTime = DateTime.UtcNow });

        var cam1Recordings = await _repository.GetByCameraIdAsync("cam1");
        Assert.Equal(2, cam1Recordings.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRecordings()
    {
        await _repository.AddAsync(new Recording { CameraId = "cam1", StartTime = DateTime.UtcNow });
        await _repository.AddAsync(new Recording { CameraId = "cam2", StartTime = DateTime.UtcNow });

        var all = await _repository.GetAllAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRecording()
    {
        var recording = new Recording { CameraId = "cam1", StartTime = DateTime.UtcNow };
        await _repository.AddAsync(recording);
        await _repository.DeleteAsync(recording.Id);

        Assert.Null(await _repository.GetByIdAsync(recording.Id));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRecording()
    {
        var recording = new Recording { CameraId = "cam1", StartTime = DateTime.UtcNow, Status = RecordingStatus.Recording };
        await _repository.AddAsync(recording);

        recording.Status = RecordingStatus.Completed;
        recording.EndTime = DateTime.UtcNow;
        await _repository.UpdateAsync(recording);

        var result = await _repository.GetByIdAsync(recording.Id);
        Assert.Equal(RecordingStatus.Completed, result?.Status);
    }
}
