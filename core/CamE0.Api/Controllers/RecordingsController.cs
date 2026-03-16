using CamE0.Devices.Interfaces;
using CamE0.Storage.Interfaces;
using CamE0.Storage.Models;
using CamE0.Video.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CamE0.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class RecordingsController : ControllerBase
{
    private readonly IRecordingService _recordingService;
    private readonly IRecordingRepository _recordingRepository;
    private readonly ICameraManager _cameraManager;

    public RecordingsController(
        IRecordingService recordingService,
        IRecordingRepository recordingRepository,
        ICameraManager cameraManager)
    {
        _recordingService = recordingService;
        _recordingRepository = recordingRepository;
        _cameraManager = cameraManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var recordings = await _recordingRepository.GetAllAsync(from, to);
        return Ok(recordings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var recording = await _recordingRepository.GetByIdAsync(id);
        if (recording == null)
        {
            return NotFound(new { error = $"Recording with ID {id} not found." });
        }
        return Ok(recording);
    }

    [HttpGet("camera/{cameraId}")]
    public async Task<IActionResult> GetByCameraId(string cameraId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var recordings = await _recordingRepository.GetByCameraIdAsync(cameraId, from, to);
        return Ok(recordings);
    }

    [HttpPost("{cameraId}/start")]
    [Authorize(Policy = "OperatorOrAdmin")]
    public async Task<IActionResult> StartRecording(string cameraId, [FromQuery] RecordingType type = RecordingType.Continuous)
    {
        var camera = await _cameraManager.GetCameraAsync(cameraId);
        if (camera == null)
        {
            return NotFound(new { error = $"Camera with ID {cameraId} not found." });
        }

        var recording = await _recordingService.StartRecordingAsync(cameraId, camera.RtspUrl, type);
        return Ok(recording);
    }

    [HttpPost("{cameraId}/stop")]
    [Authorize(Policy = "OperatorOrAdmin")]
    public async Task<IActionResult> StopRecording(string cameraId)
    {
        var recording = await _recordingService.StopRecordingAsync(cameraId);
        if (recording == null)
        {
            return NotFound(new { error = $"No active recording for camera {cameraId}." });
        }
        return Ok(recording);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveRecordings()
    {
        var recordings = await _recordingService.GetActiveRecordingsAsync();
        return Ok(recordings);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(string id)
    {
        await _recordingRepository.DeleteAsync(id);
        return NoContent();
    }
}
