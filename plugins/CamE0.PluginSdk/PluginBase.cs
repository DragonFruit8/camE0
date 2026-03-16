namespace CamE0.PluginSdk;

/// <summary>
/// Base class for plugins with default implementations.
/// </summary>
public abstract class PluginBase : IPlugin
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Version { get; }
    public virtual string Description => string.Empty;

    protected IPluginContext Context { get; private set; } = null!;

    public virtual Task InitializeAsync(IPluginContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        return Task.CompletedTask;
    }

    public virtual Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
