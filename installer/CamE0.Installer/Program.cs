using CamE0.Authentication;
using CamE0.Devices;
using CamE0.Security;
using CamE0.Storage;
using CamE0.Video;

namespace CamE0.Installer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure as Windows Service when running on Windows
        if (OperatingSystem.IsWindows())
        {
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "CamE0 NVR Service";
            });
        }

        // Register all CamE0 modules
        builder.Services.AddCamE0Security();
        builder.Services.AddCamE0Authentication(builder.Configuration);
        builder.Services.AddCamE0Devices();
        builder.Services.AddCamE0Storage(builder.Configuration);
        builder.Services.AddCamE0Video(builder.Configuration);

        builder.Services.AddHostedService<NvrHostedService>();

        var host = builder.Build();
        host.Run();
    }
}
