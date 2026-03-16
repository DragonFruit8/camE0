namespace CamE0.Devices.Models;

public sealed class OnvifDeviceInfo
{
    public string EndpointAddress { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 80;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = string.Empty;
    public string HardwareId { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
}
