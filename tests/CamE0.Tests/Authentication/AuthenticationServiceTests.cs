using CamE0.Authentication.Interfaces;
using CamE0.Authentication.Models;
using CamE0.Authentication.Services;
using CamE0.Security.Interfaces;
using CamE0.Security.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CamE0.Tests.Authentication;

public class AuthenticationServiceTests
{
    private readonly IAuthenticationService _authService;
    private readonly IUserRepository _userRepository;

    public AuthenticationServiceTests()
    {
        _userRepository = new InMemoryUserRepository();
        var jwtSettings = new JwtSettings
        {
            SecretKey = "CamE0_Test_Secret_Key_For_Unit_Tests_32chars!!",
            Issuer = "CamE0Test",
            Audience = "CamE0TestUsers",
            ExpirationMinutes = 60
        };
        var tokenService = new JwtTokenService(Options.Create(jwtSettings));
        var auditLogger = new InMemoryAuditLogger(new Mock<ILogger<InMemoryAuditLogger>>().Object);
        var rateLimiter = new SlidingWindowRateLimiter(maxRequests: 100);
        var logger = new Mock<ILogger<AuthenticationService>>().Object;

        _authService = new AuthenticationService(_userRepository, tokenService, auditLogger, rateLimiter, logger);
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsToken()
    {
        var request = new LoginRequest { Username = "admin", Password = "admin" };
        var response = await _authService.AuthenticateAsync(request);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrEmpty(response.Token));
        Assert.Equal("admin", response.Username);
        Assert.Equal(UserRoles.Admin, response.Role);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidPassword_ReturnsNull()
    {
        var request = new LoginRequest { Username = "admin", Password = "wrongpassword" };
        var response = await _authService.AuthenticateAsync(request);

        Assert.Null(response);
    }

    [Fact]
    public async Task AuthenticateAsync_NonExistentUser_ReturnsNull()
    {
        var request = new LoginRequest { Username = "nonexistent", Password = "password" };
        var response = await _authService.AuthenticateAsync(request);

        Assert.Null(response);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ExistingUser_ReturnsUser()
    {
        var user = await _authService.GetUserByUsernameAsync("admin");
        Assert.NotNull(user);
        Assert.Equal("admin", user.Username);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_NonExistentUser_ReturnsNull()
    {
        var user = await _authService.GetUserByUsernameAsync("nonexistent");
        Assert.Null(user);
    }
}
