using Microsoft.Extensions.Logging;

namespace CamE0.PluginSdk;

/// <summary>
/// Context provided to plugins for accessing system services.
/// </summary>
public interface IPluginContext
{
    ILogger Logger { get; }
    IServiceProvider Services { get; }
    string DataDirectory { get; }
    IPluginConfiguration Configuration { get; }
}
