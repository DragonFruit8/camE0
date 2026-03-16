using CamE0.Devices.Interfaces;
using CamE0.Devices.Models;
using CamE0.Devices.Services;

namespace CamE0.Tests.Devices;

public class CameraRepositoryTests
{
    private readonly ICameraRepository _repository = new InMemoryCameraRepository();

    [Fact]
    public async Task AddAsync_NewCamera_CanBeRetrieved()
    {
        var camera = new Camera { Name = "Test Camera", IpAddress = "10.0.0.1" };
        await _repository.AddAsync(camera);

        var result = await _repository.GetByIdAsync(camera.Id);
        Assert.NotNull(result);
        Assert.Equal("Test Camera", result.Name);
    }

    [Fact]
    public async Task GetAllAsync_MultipleItems_ReturnsAll()
    {
        await _repository.AddAsync(new Camera { Name = "C1" });
        await _repository.AddAsync(new Camera { Name = "C2" });

        var all = await _repository.GetAllAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCamera()
    {
        var camera = new Camera { Name = "Delete Me" };
        await _repository.AddAsync(camera);
        await _repository.DeleteAsync(camera.Id);

        Assert.Null(await _repository.GetByIdAsync(camera.Id));
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        await _repository.AddAsync(new Camera { Name = "C1" });
        await _repository.AddAsync(new Camera { Name = "C2" });

        Assert.Equal(2, await _repository.GetCountAsync());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCamera()
    {
        var camera = new Camera { Name = "Original" };
        await _repository.AddAsync(camera);

        camera.Name = "Updated";
        await _repository.UpdateAsync(camera);

        var result = await _repository.GetByIdAsync(camera.Id);
        Assert.Equal("Updated", result?.Name);
    }
}
