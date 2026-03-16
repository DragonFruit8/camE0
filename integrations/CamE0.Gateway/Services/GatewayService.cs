using System.Collections.Concurrent;
using CamE0.Gateway.Interfaces;
using Microsoft.Extensions.Logging;

namespace CamE0.Gateway.Services;

/// <summary>
/// Local gateway service for managing integrations.
/// All integrations are local-only — no cloud dependencies.
/// </summary>
public sealed class GatewayService : IGatewayService
{
    private readonly ConcurrentDictionary<string, IntegrationConfig> _integrations = new();
    private readonly ConcurrentDictionary<string, IntegrationStatus> _statuses = new();
    private readonly ILogger<GatewayService> _logger;

    public GatewayService(ILogger<GatewayService> logger)
    {
        _logger = logger;
    }

    public Task<bool> RegisterIntegrationAsync(IntegrationConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var added = _integrations.TryAdd(config.Id, config);
        if (added)
        {
            _statuses[config.Id] = config.Enabled ? IntegrationStatus.Connected : IntegrationStatus.Disabled;
            _logger.LogInformation("Integration registered: {Name} ({Type})", config.Name, config.Type);
        }

        return Task.FromResult(added);
    }

    public Task<bool> RemoveIntegrationAsync(string integrationId)
    {
        var removed = _integrations.TryRemove(integrationId, out _);
        if (removed)
        {
            _statuses.TryRemove(integrationId, out _);
            _logger.LogInformation("Integration removed: {IntegrationId}", integrationId);
        }

        return Task.FromResult(removed);
    }

    public Task<IReadOnlyList<IntegrationConfig>> GetIntegrationsAsync()
    {
        IReadOnlyList<IntegrationConfig> result = _integrations.Values.ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IntegrationStatus> GetStatusAsync(string integrationId)
    {
        var status = _statuses.GetValueOrDefault(integrationId, IntegrationStatus.Disconnected);
        return Task.FromResult(status);
    }

    public Task SendEventAsync(string integrationId, IntegrationEvent eventData)
    {
        if (!_integrations.TryGetValue(integrationId, out var config))
        {
            _logger.LogWarning("Integration {IntegrationId} not found", integrationId);
            return Task.CompletedTask;
        }

        if (!config.Enabled)
        {
            _logger.LogDebug("Integration {IntegrationId} is disabled, skipping event", integrationId);
            return Task.CompletedTask;
        }

        _logger.LogInformation("Event sent to integration {Name}: {EventType}",
            config.Name, eventData.EventType);

        return Task.CompletedTask;
    }
}
