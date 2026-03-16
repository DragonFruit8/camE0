using CamE0.Video.Interfaces;
using CamE0.Video.Models;
using CamE0.Video.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CamE0.Video;

public static class VideoServiceExtensions
{
    public static IServiceCollection AddCamE0Video(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FFmpegSettings>(configuration.GetSection("FFmpeg"));
        services.AddSingleton<IFFmpegService, FFmpegService>();
        services.AddSingleton<IStreamManager, StreamManager>();
        services.AddSingleton<IRecordingService, RecordingService>();
        services.AddSingleton<IMotionDetector, FrameDifferenceMotionDetector>();
        return services;
    }
}
