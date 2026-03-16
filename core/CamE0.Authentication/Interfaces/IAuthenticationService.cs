using CamE0.Authentication.Models;

namespace CamE0.Authentication.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request);
    Task<bool> ValidateTokenAsync(string token);
    Task<User?> GetUserByUsernameAsync(string username);
}
