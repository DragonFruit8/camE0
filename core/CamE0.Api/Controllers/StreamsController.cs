using CamE0.Devices.Interfaces;
using CamE0.Video.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CamE0.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StreamsController : ControllerBase
{
    private readonly IStreamManager _streamManager;
    private readonly ICameraManager _cameraManager;

    public StreamsController(IStreamManager streamManager, ICameraManager cameraManager)
    {
        _streamManager = streamManager;
        _cameraManager = cameraManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStreams()
    {
        var streams = await _streamManager.GetAllStreamsAsync();
        return Ok(streams);
    }

    [HttpGet("{cameraId}")]
    public async Task<IActionResult> GetStream(string cameraId)
    {
        var stream = await _streamManager.GetStreamAsync(cameraId);
        if (stream == null)
        {
            return NotFound(new { error = $"No active stream for camera {cameraId}." });
        }
        return Ok(stream);
    }

    [HttpPost("{cameraId}/start")]
    [Authorize(Policy = "OperatorOrAdmin")]
    public async Task<IActionResult> StartStream(string cameraId)
    {
        var camera = await _cameraManager.GetCameraAsync(cameraId);
        if (camera == null)
        {
            return NotFound(new { error = $"Camera with ID {cameraId} not found." });
        }

        var stream = await _streamManager.StartStreamAsync(cameraId, camera.RtspUrl);
        return Ok(stream);
    }

    [HttpPost("{cameraId}/stop")]
    [Authorize(Policy = "OperatorOrAdmin")]
    public async Task<IActionResult> StopStream(string cameraId)
    {
        await _streamManager.StopStreamAsync(cameraId);
        return NoContent();
    }
}
