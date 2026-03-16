using CamE0.Security.Interfaces;
using CamE0.Video.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CamE0.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SystemController : ControllerBase
{
    private readonly IAuditLogger _auditLogger;
    private readonly IFFmpegService _ffmpegService;

    public SystemController(IAuditLogger auditLogger, IFFmpegService ffmpegService)
    {
        _auditLogger = auditLogger;
        _ffmpegService = ffmpegService;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> Health()
    {
        var ffmpegAvailable = await _ffmpegService.IsFFmpegAvailableAsync();
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            ffmpegAvailable
        });
    }

    [HttpGet("audit-log")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAuditLog([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? category)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;
        var entries = await _auditLogger.GetEntriesAsync(fromDate, toDate, category);
        return Ok(entries);
    }
}
