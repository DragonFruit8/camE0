using CamE0.Authentication;
using CamE0.Devices;
using CamE0.Security;
using CamE0.Storage;
using CamE0.Video;

namespace CamE0.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register all CamE0 modules
        builder.Services.AddCamE0Security();
        builder.Services.AddCamE0Authentication(builder.Configuration);
        builder.Services.AddCamE0Devices();
        builder.Services.AddCamE0Storage(builder.Configuration);
        builder.Services.AddCamE0Video(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddRazorPages();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.Run();
    }
}
