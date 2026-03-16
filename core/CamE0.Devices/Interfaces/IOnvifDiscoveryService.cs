using CamE0.Devices.Models;

namespace CamE0.Devices.Interfaces;

public interface IOnvifDiscoveryService
{
    Task<IReadOnlyList<OnvifDeviceInfo>> DiscoverDevicesAsync(TimeSpan? timeout = null);
    Task<OnvifDeviceInfo?> GetDeviceInfoAsync(string ipAddress, int port = 80);
    Task<string?> GetRtspUrlAsync(string ipAddress, int port, string username, string password);
}
