using CamE0.Devices.Interfaces;
using CamE0.Devices.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CamE0.Devices;

public static class DeviceServiceExtensions
{
    public static IServiceCollection AddCamE0Devices(this IServiceCollection services)
    {
        services.AddSingleton<ICameraRepository, InMemoryCameraRepository>();
        services.AddSingleton<IOnvifDiscoveryService, OnvifDiscoveryService>();
        services.AddSingleton<ICameraManager, CameraManager>();
        return services;
    }
}
