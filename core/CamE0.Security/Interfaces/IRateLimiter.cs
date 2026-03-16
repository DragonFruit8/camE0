namespace CamE0.Security.Interfaces;

/// <summary>
/// Provides rate limiting for API endpoints and authentication attempts.
/// </summary>
public interface IRateLimiter
{
    bool IsAllowed(string clientId, string endpoint);
    void RecordRequest(string clientId, string endpoint);
    void Reset(string clientId);
    RateLimitStatus GetStatus(string clientId, string endpoint);
}

public record RateLimitStatus(
    int RequestCount,
    int MaxRequests,
    TimeSpan Window,
    DateTime WindowResetTime,
    bool IsBlocked
);
