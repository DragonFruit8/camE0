using CamE0.Gateway.Interfaces;
using CamE0.Gateway.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CamE0.Gateway;

public static class GatewayServiceExtensions
{
    public static IServiceCollection AddCamE0Gateway(this IServiceCollection services)
    {
        services.AddSingleton<IGatewayService, GatewayService>();
        return services;
    }
}
