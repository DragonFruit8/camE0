using CamE0.Video.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CamE0.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MotionController : ControllerBase
{
    private readonly IMotionDetector _motionDetector;

    public MotionController(IMotionDetector motionDetector)
    {
        _motionDetector = motionDetector;
    }

    [HttpGet("{cameraId}/events")]
    public async Task<IActionResult> GetEvents(string cameraId, [FromQuery] int count = 50)
    {
        var events = await _motionDetector.GetRecentEventsAsync(cameraId, count);
        return Ok(events);
    }

    [HttpPost("{cameraId}/sensitivity")]
    [Authorize(Policy = "OperatorOrAdmin")]
    public IActionResult SetSensitivity(string cameraId, [FromQuery] int sensitivity)
    {
        if (sensitivity < 1 || sensitivity > 100)
        {
            return BadRequest(new { error = "Sensitivity must be between 1 and 100." });
        }

        _motionDetector.SetSensitivity(cameraId, sensitivity);
        return Ok(new { cameraId, sensitivity });
    }
}
