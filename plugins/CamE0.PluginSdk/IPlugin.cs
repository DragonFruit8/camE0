namespace CamE0.PluginSdk;

/// <summary>
/// Base interface for all CamE0 plugins.
/// </summary>
public interface IPlugin
{
    string Id { get; }
    string Name { get; }
    string Version { get; }
    string Description { get; }
    Task InitializeAsync(IPluginContext context);
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
