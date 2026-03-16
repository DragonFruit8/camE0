using CamE0.Devices.Interfaces;
using CamE0.Devices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CamE0.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CamerasController : ControllerBase
{
    private readonly ICameraManager _cameraManager;
    private readonly IOnvifDiscoveryService _discoveryService;

    public CamerasController(ICameraManager cameraManager, IOnvifDiscoveryService discoveryService)
    {
        _cameraManager = cameraManager;
        _discoveryService = discoveryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cameras = await _cameraManager.GetAllCamerasAsync();
        return Ok(cameras);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var camera = await _cameraManager.GetCameraAsync(id);
        if (camera == null)
        {
            return NotFound(new { error = $"Camera with ID {id} not found." });
        }
        return Ok(camera);
    }

    [HttpPost]
    [Authorize(Policy = "OperatorOrAdmin")]
    public async Task<IActionResult> Add([FromBody] Camera camera)
    {
        if (string.IsNullOrWhiteSpace(camera.Name))
        {
            return BadRequest(new { error = "Camera name is required." });
        }

        try
        {
            var result = await _cameraManager.AddCameraAsync(camera);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "OperatorOrAdmin")]
    public async Task<IActionResult> Update(string id, [FromBody] Camera camera)
    {
        camera.Id = id;
        try
        {
            var result = await _cameraManager.UpdateCameraAsync(camera);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(string id)
    {
        await _cameraManager.RemoveCameraAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(string id)
    {
        var status = await _cameraManager.GetCameraStatusAsync(id);
        return Ok(new { cameraId = id, status = status.ToString() });
    }

    [HttpPost("test-connection")]
    [Authorize(Policy = "OperatorOrAdmin")]
    public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest request)
    {
        var isConnected = await _cameraManager.TestConnectionAsync(request.RtspUrl, request.Username, request.Password);
        return Ok(new { connected = isConnected });
    }

    [HttpPost("discover")]
    [Authorize(Policy = "OperatorOrAdmin")]
    public async Task<IActionResult> DiscoverDevices([FromQuery] int timeoutSeconds = 5)
    {
        var devices = await _discoveryService.DiscoverDevicesAsync(TimeSpan.FromSeconds(timeoutSeconds));
        return Ok(devices);
    }
}

public sealed class TestConnectionRequest
{
    public string RtspUrl { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
}
