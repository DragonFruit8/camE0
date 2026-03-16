namespace CamE0.Authentication.Models;

public sealed class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CamE0";
    public string Audience { get; set; } = "CamE0Users";
    public int ExpirationMinutes { get; set; } = 480; // 8 hours
}
