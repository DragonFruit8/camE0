using CamE0.Gateway.Interfaces;
using CamE0.Gateway.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CamE0.Tests.Gateway;

public class GatewayServiceTests
{
    private readonly IGatewayService _gateway;

    public GatewayServiceTests()
    {
        var logger = new Mock<ILogger<GatewayService>>().Object;
        _gateway = new GatewayService(logger);
    }

    [Fact]
    public async Task RegisterIntegrationAsync_NewIntegration_ReturnsTrue()
    {
        var config = new IntegrationConfig { Name = "Test", Type = "webhook" };
        Assert.True(await _gateway.RegisterIntegrationAsync(config));
    }

    [Fact]
    public async Task RegisterIntegrationAsync_Duplicate_ReturnsFalse()
    {
        var config = new IntegrationConfig { Id = "dup-1", Name = "Test", Type = "webhook" };
        await _gateway.RegisterIntegrationAsync(config);
        Assert.False(await _gateway.RegisterIntegrationAsync(config));
    }

    [Fact]
    public async Task RemoveIntegrationAsync_ExistingIntegration_ReturnsTrue()
    {
        var config = new IntegrationConfig { Name = "Test", Type = "webhook" };
        await _gateway.RegisterIntegrationAsync(config);
        Assert.True(await _gateway.RemoveIntegrationAsync(config.Id));
    }

    [Fact]
    public async Task GetIntegrationsAsync_ReturnsAll()
    {
        await _gateway.RegisterIntegrationAsync(new IntegrationConfig { Name = "I1", Type = "t1" });
        await _gateway.RegisterIntegrationAsync(new IntegrationConfig { Name = "I2", Type = "t2" });

        var all = await _gateway.GetIntegrationsAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetStatusAsync_EnabledIntegration_ReturnsConnected()
    {
        var config = new IntegrationConfig { Name = "Test", Type = "webhook", Enabled = true };
        await _gateway.RegisterIntegrationAsync(config);

        var status = await _gateway.GetStatusAsync(config.Id);
        Assert.Equal(IntegrationStatus.Connected, status);
    }

    [Fact]
    public async Task GetStatusAsync_UnknownIntegration_ReturnsDisconnected()
    {
        var status = await _gateway.GetStatusAsync("nonexistent");
        Assert.Equal(IntegrationStatus.Disconnected, status);
    }
}
