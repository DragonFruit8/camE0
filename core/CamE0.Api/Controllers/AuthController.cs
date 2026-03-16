using CamE0.Authentication.Interfaces;
using CamE0.Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CamE0.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Username and password are required." });
        }

        var response = await _authService.AuthenticateAsync(request);
        if (response == null)
        {
            return Unauthorized(new { error = "Invalid credentials or account locked." });
        }

        return Ok(response);
    }

    [HttpPost("validate")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        var username = User.Identity?.Name;
        return Ok(new { valid = true, username });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt
        });
    }
}
