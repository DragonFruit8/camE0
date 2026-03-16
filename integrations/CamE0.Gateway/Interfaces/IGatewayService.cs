namespace CamE0.Gateway.Interfaces;

/// <summary>
/// Gateway service for managing external integrations.
/// </summary>
public interface IGatewayService
{
    Task<bool> RegisterIntegrationAsync(IntegrationConfig config);
    Task<bool> RemoveIntegrationAsync(string integrationId);
    Task<IReadOnlyList<IntegrationConfig>> GetIntegrationsAsync();
    Task<IntegrationStatus> GetStatusAsync(string integrationId);
    Task SendEventAsync(string integrationId, IntegrationEvent eventData);
}

public sealed class IntegrationConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public Dictionary<string, string> Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class IntegrationEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum IntegrationStatus
{
    Connected,
    Disconnected,
    Error,
    Disabled
}
