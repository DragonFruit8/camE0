using CamE0.Storage.Interfaces;
using CamE0.Storage.Models;
using CamE0.Storage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CamE0.Storage;

public static class StorageServiceExtensions
{
    public static IServiceCollection AddCamE0Storage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));
        services.AddSingleton<IStorageService, LocalStorageService>();
        services.AddSingleton<IRecordingRepository, InMemoryRecordingRepository>();
        return services;
    }
}
