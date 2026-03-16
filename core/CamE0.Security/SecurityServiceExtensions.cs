using CamE0.Security.Interfaces;
using CamE0.Security.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CamE0.Security;

/// <summary>
/// DI registration for security services.
/// </summary>
public static class SecurityServiceExtensions
{
    public static IServiceCollection AddCamE0Security(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        services.AddSingleton<IHashService, HashService>();
        services.AddSingleton<IAuditLogger, InMemoryAuditLogger>();
        services.AddSingleton<IRateLimiter>(new SlidingWindowRateLimiter(maxRequests: 100, windowDuration: TimeSpan.FromMinutes(1)));
        return services;
    }
}
