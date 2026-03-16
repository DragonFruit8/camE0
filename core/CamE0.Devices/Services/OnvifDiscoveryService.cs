using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using CamE0.Devices.Interfaces;
using CamE0.Devices.Models;
using Microsoft.Extensions.Logging;

namespace CamE0.Devices.Services;

/// <summary>
/// ONVIF WS-Discovery implementation for camera discovery on local network.
/// Uses UDP multicast to find ONVIF-compliant devices.
/// </summary>
public sealed class OnvifDiscoveryService : IOnvifDiscoveryService
{
    private readonly ILogger<OnvifDiscoveryService> _logger;

    private const string WsDiscoveryMulticast = "239.255.255.250";
    private const int WsDiscoveryPort = 3702;

    private static readonly string ProbeMessage = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<e:Envelope xmlns:e=""http://www.w3.org/2003/05/soap-envelope""
            xmlns:w=""http://schemas.xmlsoap.org/ws/2004/08/addressing""
            xmlns:d=""http://schemas.xmlsoap.org/ws/2005/04/discovery""
            xmlns:dn=""http://www.onvif.org/ver10/network/wsdl"">
    <e:Header>
        <w:MessageID>uuid:{0}</w:MessageID>
        <w:To>urn:schemas-xmlsoap-org:ws:2005:04:discovery</w:To>
        <w:Action>http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe</w:Action>
    </e:Header>
    <e:Body>
        <d:Probe>
            <d:Types>dn:NetworkVideoTransmitter</d:Types>
        </d:Probe>
    </e:Body>
</e:Envelope>";

    public OnvifDiscoveryService(ILogger<OnvifDiscoveryService> logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<OnvifDeviceInfo>> DiscoverDevicesAsync(TimeSpan? timeout = null)
    {
        var discoveryTimeout = timeout ?? TimeSpan.FromSeconds(5);
        var devices = new List<OnvifDeviceInfo>();

        try
        {
            using var udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            var multicastEndpoint = new IPEndPoint(IPAddress.Parse(WsDiscoveryMulticast), WsDiscoveryPort);
            var probeData = Encoding.UTF8.GetBytes(string.Format(ProbeMessage, Guid.NewGuid()));

            await udpClient.SendAsync(probeData, probeData.Length, multicastEndpoint);

            udpClient.Client.ReceiveTimeout = (int)discoveryTimeout.TotalMilliseconds;

            var endTime = DateTime.UtcNow.Add(discoveryTimeout);
            while (DateTime.UtcNow < endTime)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync()
                        .WaitAsync(endTime - DateTime.UtcNow);

                    var response = Encoding.UTF8.GetString(result.Buffer);
                    var device = ParseDiscoveryResponse(response, result.RemoteEndPoint);
                    if (device != null)
                    {
                        devices.Add(device);
                        _logger.LogInformation("Discovered ONVIF device at {IpAddress}:{Port}",
                            device.IpAddress, device.Port);
                    }
                }
                catch (TimeoutException)
                {
                    break;
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ONVIF device discovery");
        }

        return devices.AsReadOnly();
    }

    public Task<OnvifDeviceInfo?> GetDeviceInfoAsync(string ipAddress, int port = 80)
    {
        var device = new OnvifDeviceInfo
        {
            IpAddress = ipAddress,
            Port = port,
            EndpointAddress = $"http://{ipAddress}:{port}/onvif/device_service",
            DiscoveredAt = DateTime.UtcNow
        };
        return Task.FromResult<OnvifDeviceInfo?>(device);
    }

    public Task<string?> GetRtspUrlAsync(string ipAddress, int port, string username, string password)
    {
        // Standard ONVIF RTSP URL format
        var rtspUrl = $"rtsp://{username}:{password}@{ipAddress}:{port}/stream1";
        return Task.FromResult<string?>(rtspUrl);
    }

    private OnvifDeviceInfo? ParseDiscoveryResponse(string xml, IPEndPoint sender)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var ns = XNamespace.Get("http://schemas.xmlsoap.org/ws/2005/04/discovery");

            var xAddrs = doc.Descendants(ns + "XAddrs").FirstOrDefault()?.Value;
            var scopes = doc.Descendants(ns + "Scopes").FirstOrDefault()?.Value;

            var device = new OnvifDeviceInfo
            {
                IpAddress = sender.Address.ToString(),
                Port = sender.Port,
                EndpointAddress = xAddrs ?? $"http://{sender.Address}/onvif/device_service",
                DiscoveredAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(scopes))
            {
                device.Scopes = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var scope in device.Scopes)
                {
                    if (scope.Contains("hardware/", StringComparison.OrdinalIgnoreCase))
                        device.Model = Uri.UnescapeDataString(scope.Split("hardware/").Last());
                    else if (scope.Contains("name/", StringComparison.OrdinalIgnoreCase))
                        device.Manufacturer = Uri.UnescapeDataString(scope.Split("name/").Last());
                }
            }

            return device;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse ONVIF discovery response from {Sender}", sender);
            return null;
        }
    }
}
