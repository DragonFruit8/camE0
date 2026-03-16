using CamE0.Authentication.Interfaces;
using CamE0.Authentication.Models;
using CamE0.Authentication.Services;
using Microsoft.Extensions.Options;

namespace CamE0.Tests.Authentication;

public class JwtTokenServiceTests
{
    private readonly IJwtTokenService _tokenService;
    private readonly JwtSettings _settings;

    public JwtTokenServiceTests()
    {
        _settings = new JwtSettings
        {
            SecretKey = "CamE0_Test_Secret_Key_For_Unit_Tests_32chars!!",
            Issuer = "CamE0Test",
            Audience = "CamE0TestUsers",
            ExpirationMinutes = 60
        };
        _tokenService = new JwtTokenService(Options.Create(_settings));
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var user = new User { Id = "1", Username = "testuser", Role = UserRoles.Admin };
        var token = _tokenService.GenerateToken(user);
        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        var user = new User { Id = "1", Username = "testuser", Role = UserRoles.Admin };
        var token = _tokenService.GenerateToken(user);
        Assert.True(_tokenService.ValidateToken(token));
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsFalse()
    {
        Assert.False(_tokenService.ValidateToken("invalid-token-string"));
    }

    [Fact]
    public void GetUserIdFromToken_ReturnsCorrectUserId()
    {
        var user = new User { Id = "42", Username = "testuser", Role = UserRoles.Viewer };
        var token = _tokenService.GenerateToken(user);
        var userId = _tokenService.GetUserIdFromToken(token);
        Assert.Equal("42", userId);
    }

    [Fact]
    public void GetUserIdFromToken_InvalidToken_ReturnsNull()
    {
        var result = _tokenService.GetUserIdFromToken("not-a-valid-jwt");
        Assert.Null(result);
    }
}
