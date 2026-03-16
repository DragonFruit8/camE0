using System.Collections.Concurrent;
using CamE0.Video.Interfaces;
using CamE0.Video.Models;
using Microsoft.Extensions.Logging;

namespace CamE0.Video.Services;

public sealed class StreamManager : IStreamManager
{
    private readonly ConcurrentDictionary<string, VideoStream> _streams = new();
    private readonly IFFmpegService _ffmpegService;
    private readonly ILogger<StreamManager> _logger;

    public StreamManager(IFFmpegService ffmpegService, ILogger<StreamManager> logger)
    {
        _ffmpegService = ffmpegService;
        _logger = logger;
    }

    public async Task<VideoStream> StartStreamAsync(string cameraId, string rtspUrl)
    {
        if (_streams.TryGetValue(cameraId, out var existing) && existing.Status == StreamStatus.Streaming)
        {
            _logger.LogWarning("Stream already active for camera {CameraId}", cameraId);
            return existing;
        }

        var stream = new VideoStream
        {
            CameraId = cameraId,
            RtspUrl = rtspUrl,
            Status = StreamStatus.Connecting,
            StartedAt = DateTime.UtcNow
        };

        _streams[cameraId] = stream;

        // Verify FFmpeg availability
        var isAvailable = await _ffmpegService.IsFFmpegAvailableAsync();
        if (!isAvailable)
        {
            stream.Status = StreamStatus.Error;
            _logger.LogError("FFmpeg is not available. Cannot start stream for camera {CameraId}", cameraId);
            return stream;
        }

        stream.Status = StreamStatus.Streaming;
        _logger.LogInformation("Stream started for camera {CameraId} from {RtspUrl}", cameraId, rtspUrl);

        return stream;
    }

    public async Task StopStreamAsync(string cameraId)
    {
        if (_streams.TryRemove(cameraId, out var stream))
        {
            if (stream.ProcessId.HasValue)
            {
                await _ffmpegService.StopProcessAsync(stream.ProcessId.Value);
            }
            _logger.LogInformation("Stream stopped for camera {CameraId}", cameraId);
        }
    }

    public Task<VideoStream?> GetStreamAsync(string cameraId)
    {
        _streams.TryGetValue(cameraId, out var stream);
        return Task.FromResult(stream);
    }

    public Task<IReadOnlyList<VideoStream>> GetAllStreamsAsync()
    {
        IReadOnlyList<VideoStream> streams = _streams.Values.ToList().AsReadOnly();
        return Task.FromResult(streams);
    }

    public Task<bool> IsStreamingAsync(string cameraId)
    {
        var isStreaming = _streams.TryGetValue(cameraId, out var stream)
            && stream.Status == StreamStatus.Streaming;
        return Task.FromResult(isStreaming);
    }
}
