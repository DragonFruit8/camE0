using CamE0.Authentication.Models;

namespace CamE0.Authentication.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    string? GetUserIdFromToken(string token);
}
