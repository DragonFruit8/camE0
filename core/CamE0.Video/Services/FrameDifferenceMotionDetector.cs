using System.Collections.Concurrent;
using CamE0.Video.Interfaces;
using CamE0.Video.Models;
using Microsoft.Extensions.Logging;

namespace CamE0.Video.Services;

/// <summary>
/// Frame-difference based motion detection.
/// Compares consecutive frames pixel-by-pixel to detect motion.
/// </summary>
public sealed class FrameDifferenceMotionDetector : IMotionDetector
{
    private readonly ConcurrentDictionary<string, List<MotionEvent>> _recentEvents = new();
    private readonly ConcurrentDictionary<string, int> _sensitivities = new();
    private readonly ILogger<FrameDifferenceMotionDetector> _logger;
    private const int MaxEventsPerCamera = 1000;

    public event EventHandler<MotionEvent>? MotionDetected;

    public FrameDifferenceMotionDetector(ILogger<FrameDifferenceMotionDetector> logger)
    {
        _logger = logger;
    }

    public Task<MotionEvent?> DetectMotionAsync(string cameraId, byte[] currentFrame, byte[] previousFrame, int sensitivity = 50)
    {
        ArgumentNullException.ThrowIfNull(currentFrame);
        ArgumentNullException.ThrowIfNull(previousFrame);

        var effectiveSensitivity = _sensitivities.GetValueOrDefault(cameraId, sensitivity);
        var motionScore = CalculateMotionScore(currentFrame, previousFrame);

        // Threshold is inverse of sensitivity (higher sensitivity = lower threshold)
        var threshold = 100.0 - effectiveSensitivity;
        var normalizedThreshold = threshold / 100.0 * 30.0; // Scale to pixel diff range

        if (motionScore > normalizedThreshold)
        {
            var motionEvent = new MotionEvent
            {
                CameraId = cameraId,
                Timestamp = DateTime.UtcNow,
                MotionScore = motionScore,
                Threshold = effectiveSensitivity
            };

            RecordEvent(cameraId, motionEvent);
            MotionDetected?.Invoke(this, motionEvent);

            _logger.LogInformation(
                "Motion detected on camera {CameraId}: score={Score:F2}, threshold={Threshold}",
                cameraId, motionScore, effectiveSensitivity);

            return Task.FromResult<MotionEvent?>(motionEvent);
        }

        return Task.FromResult<MotionEvent?>(null);
    }

    public Task<IReadOnlyList<MotionEvent>> GetRecentEventsAsync(string cameraId, int count = 50)
    {
        if (!_recentEvents.TryGetValue(cameraId, out var events))
        {
            return Task.FromResult<IReadOnlyList<MotionEvent>>(Array.Empty<MotionEvent>());
        }

        IReadOnlyList<MotionEvent> result = events
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToList()
            .AsReadOnly();

        return Task.FromResult(result);
    }

    public void SetSensitivity(string cameraId, int sensitivity)
    {
        _sensitivities[cameraId] = Math.Clamp(sensitivity, 1, 100);
    }

    private static double CalculateMotionScore(byte[] current, byte[] previous)
    {
        var minLength = Math.Min(current.Length, previous.Length);
        if (minLength == 0) return 0;

        long totalDiff = 0;
        var sampleSize = Math.Min(minLength, 10000); // Sample for performance
        var step = minLength / sampleSize;

        for (var i = 0; i < minLength; i += Math.Max(step, 1))
        {
            totalDiff += Math.Abs(current[i] - previous[i]);
        }

        var sampledPixels = minLength / Math.Max(step, 1);
        return sampledPixels > 0 ? (double)totalDiff / sampledPixels : 0;
    }

    private void RecordEvent(string cameraId, MotionEvent motionEvent)
    {
        var events = _recentEvents.GetOrAdd(cameraId, _ => new List<MotionEvent>());
        lock (events)
        {
            events.Add(motionEvent);
            if (events.Count > MaxEventsPerCamera)
            {
                events.RemoveRange(0, events.Count - MaxEventsPerCamera);
            }
        }
    }
}
