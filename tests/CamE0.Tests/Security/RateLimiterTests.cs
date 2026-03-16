using CamE0.Security.Interfaces;
using CamE0.Security.Services;

namespace CamE0.Tests.Security;

public class RateLimiterTests
{
    [Fact]
    public void IsAllowed_UnderLimit_ReturnsTrue()
    {
        var limiter = new SlidingWindowRateLimiter(maxRequests: 5, windowDuration: TimeSpan.FromMinutes(1));
        Assert.True(limiter.IsAllowed("client1", "/api/test"));
    }

    [Fact]
    public void IsAllowed_OverLimit_ReturnsFalse()
    {
        var limiter = new SlidingWindowRateLimiter(maxRequests: 3, windowDuration: TimeSpan.FromMinutes(1));

        for (int i = 0; i < 3; i++)
        {
            limiter.RecordRequest("client1", "/api/test");
        }

        Assert.False(limiter.IsAllowed("client1", "/api/test"));
    }

    [Fact]
    public void Reset_ClearsClientRequests()
    {
        var limiter = new SlidingWindowRateLimiter(maxRequests: 2, windowDuration: TimeSpan.FromMinutes(1));

        limiter.RecordRequest("client1", "/api/test");
        limiter.RecordRequest("client1", "/api/test");
        Assert.False(limiter.IsAllowed("client1", "/api/test"));

        limiter.Reset("client1");
        Assert.True(limiter.IsAllowed("client1", "/api/test"));
    }

    [Fact]
    public void GetStatus_ReturnsCorrectInfo()
    {
        var limiter = new SlidingWindowRateLimiter(maxRequests: 10, windowDuration: TimeSpan.FromMinutes(1));

        limiter.RecordRequest("client1", "/api/test");
        limiter.RecordRequest("client1", "/api/test");

        var status = limiter.GetStatus("client1", "/api/test");
        Assert.Equal(2, status.RequestCount);
        Assert.Equal(10, status.MaxRequests);
        Assert.False(status.IsBlocked);
    }

    [Fact]
    public void DifferentEndpoints_TrackSeparately()
    {
        var limiter = new SlidingWindowRateLimiter(maxRequests: 1, windowDuration: TimeSpan.FromMinutes(1));

        limiter.RecordRequest("client1", "/api/endpoint1");
        Assert.False(limiter.IsAllowed("client1", "/api/endpoint1"));
        Assert.True(limiter.IsAllowed("client1", "/api/endpoint2"));
    }
}
