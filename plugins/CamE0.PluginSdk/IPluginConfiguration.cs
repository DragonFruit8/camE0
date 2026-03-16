namespace CamE0.PluginSdk;

/// <summary>
/// Plugin configuration access.
/// </summary>
public interface IPluginConfiguration
{
    string? GetValue(string key);
    T? GetValue<T>(string key) where T : struct;
    void SetValue(string key, string value);
    IReadOnlyDictionary<string, string> GetAll();
}
