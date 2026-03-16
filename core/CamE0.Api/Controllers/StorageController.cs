using CamE0.Storage.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CamE0.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StorageController : ControllerBase
{
    private readonly IStorageService _storageService;

    public StorageController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _storageService.GetStorageStatsAsync();
        return Ok(new
        {
            totalSpaceGb = stats.TotalSpaceBytes / (1024.0 * 1024 * 1024),
            usedSpaceGb = stats.UsedSpaceBytes / (1024.0 * 1024 * 1024),
            availableSpaceGb = stats.AvailableSpaceBytes / (1024.0 * 1024 * 1024),
            stats.TotalFiles
        });
    }

    [HttpGet("files/{category}")]
    public async Task<IActionResult> ListFiles(string category, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var files = await _storageService.ListFilesAsync(category, from, to);
        return Ok(files);
    }

    [HttpDelete("files")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return BadRequest(new { error = "File path is required." });
        }

        var deleted = await _storageService.DeleteFileAsync(filePath);
        return deleted ? NoContent() : NotFound();
    }
}
