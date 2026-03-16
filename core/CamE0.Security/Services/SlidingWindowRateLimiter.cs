using System.Collections.Concurrent;
using CamE0.Security.Interfaces;

namespace CamE0.Security.Services;

/// <summary>
/// Sliding window rate limiter for API endpoint protection.
/// </summary>
public sealed class SlidingWindowRateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<string, RequestWindow> _windows = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _windowDuration;

    public SlidingWindowRateLimiter(int maxRequests = 100, TimeSpan? windowDuration = null)
    {
        _maxRequests = maxRequests;
        _windowDuration = windowDuration ?? TimeSpan.FromMinutes(1);
    }

    public bool IsAllowed(string clientId, string endpoint)
    {
        var key = BuildKey(clientId, endpoint);
        var window = _windows.GetOrAdd(key, _ => new RequestWindow(_windowDuration));
        window.CleanExpired();
        return window.Count < _maxRequests;
    }

    public void RecordRequest(string clientId, string endpoint)
    {
        var key = BuildKey(clientId, endpoint);
        var window = _windows.GetOrAdd(key, _ => new RequestWindow(_windowDuration));
        window.Add(DateTime.UtcNow);
    }

    public void Reset(string clientId)
    {
        var keysToRemove = _windows.Keys.Where(k => k.StartsWith(clientId + ":", StringComparison.Ordinal)).ToList();
        foreach (var key in keysToRemove)
        {
            _windows.TryRemove(key, out _);
        }
    }

    public RateLimitStatus GetStatus(string clientId, string endpoint)
    {
        var key = BuildKey(clientId, endpoint);
        if (!_windows.TryGetValue(key, out var window))
        {
            return new RateLimitStatus(0, _maxRequests, _windowDuration, DateTime.UtcNow.Add(_windowDuration), false);
        }
        window.CleanExpired();
        return new RateLimitStatus(
            window.Count,
            _maxRequests,
            _windowDuration,
            window.EarliestExpiry,
            window.Count >= _maxRequests);
    }

    private static string BuildKey(string clientId, string endpoint) => $"{clientId}:{endpoint}";

    private sealed class RequestWindow
    {
        private readonly TimeSpan _duration;
        private readonly ConcurrentBag<DateTime> _timestamps = new();
        private readonly object _cleanLock = new();

        public RequestWindow(TimeSpan duration)
        {
            _duration = duration;
        }

        public int Count
        {
            get
            {
                CleanExpired();
                return _timestamps.Count;
            }
        }

        public DateTime EarliestExpiry
        {
            get
            {
                var oldest = _timestamps.DefaultIfEmpty(DateTime.UtcNow).Min();
                return oldest.Add(_duration);
            }
        }

        public void Add(DateTime timestamp)
        {
            _timestamps.Add(timestamp);
        }

        public void CleanExpired()
        {
            // ConcurrentBag doesn't support removal, so we rebuild
            lock (_cleanLock)
            {
                var cutoff = DateTime.UtcNow - _duration;
                var valid = _timestamps.Where(t => t > cutoff).ToList();
                while (_timestamps.TryTake(out _)) { }
                foreach (var t in valid)
                {
                    _timestamps.Add(t);
                }
            }
        }
    }
}
