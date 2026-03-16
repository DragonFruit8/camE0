using CamE0.Authentication.Interfaces;
using CamE0.Authentication.Models;
using CamE0.Security.Interfaces;
using Microsoft.Extensions.Logging;

namespace CamE0.Authentication.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditLogger _auditLogger;
    private readonly IRateLimiter _rateLimiter;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IAuditLogger auditLogger,
        IRateLimiter rateLimiter,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _auditLogger = auditLogger;
        _rateLimiter = rateLimiter;
        _logger = logger;
    }

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
    {
        // Rate limiting check
        if (!_rateLimiter.IsAllowed(request.Username, "login"))
        {
            _logger.LogWarning("Rate limit exceeded for user {Username}", request.Username);
            await _auditLogger.LogAsync(new AuditEntry(
                DateTime.UtcNow, "Authentication", "LoginRateLimited",
                request.Username, "Rate limit exceeded for login attempts"));
            return null;
        }

        _rateLimiter.RecordRequest(request.Username, "login");

        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null || !user.IsActive)
        {
            await _auditLogger.LogAsync(new AuditEntry(
                DateTime.UtcNow, "Authentication", "LoginFailed",
                request.Username, "Invalid username or account inactive"));
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await _auditLogger.LogAsync(new AuditEntry(
                DateTime.UtcNow, "Authentication", "LoginFailed",
                request.Username, "Invalid password"));
            return null;
        }

        var token = _jwtTokenService.GenerateToken(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        await _auditLogger.LogAsync(new AuditEntry(
            DateTime.UtcNow, "Authentication", "LoginSuccess",
            user.Username, "User logged in successfully"));

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(480)
        };
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        return Task.FromResult(_jwtTokenService.ValidateToken(token));
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }
}
