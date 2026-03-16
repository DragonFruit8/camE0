using System.Collections.Concurrent;
using CamE0.Storage.Interfaces;
using CamE0.Storage.Models;
using CamE0.Video.Interfaces;
using CamE0.Video.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CamE0.Video.Services;

public sealed class RecordingService : IRecordingService
{
    private readonly ConcurrentDictionary<string, Recording> _activeRecordings = new();
    private readonly IFFmpegService _ffmpegService;
    private readonly IRecordingRepository _recordingRepository;
    private readonly IStorageService _storageService;
    private readonly FFmpegSettings _ffmpegSettings;
    private readonly ILogger<RecordingService> _logger;

    public RecordingService(
        IFFmpegService ffmpegService,
        IRecordingRepository recordingRepository,
        IStorageService storageService,
        IOptions<FFmpegSettings> ffmpegSettings,
        ILogger<RecordingService> logger)
    {
        _ffmpegService = ffmpegService;
        _recordingRepository = recordingRepository;
        _storageService = storageService;
        _ffmpegSettings = ffmpegSettings.Value;
        _logger = logger;
    }

    public async Task<Recording> StartRecordingAsync(string cameraId, string rtspUrl, RecordingType type = RecordingType.Continuous)
    {
        if (_activeRecordings.ContainsKey(cameraId))
        {
            _logger.LogWarning("Recording already active for camera {CameraId}", cameraId);
            return _activeRecordings[cameraId];
        }

        var recordingPath = _storageService.GetStoragePath("recordings");
        var outputPath = Path.Combine(recordingPath, cameraId, $"%Y-%m-%d_%H-%M-%S.{_ffmpegSettings.OutputFormat}");

        var cameraDir = Path.Combine(recordingPath, cameraId);
        Directory.CreateDirectory(cameraDir);

        var recording = new Recording
        {
            CameraId = cameraId,
            StartTime = DateTime.UtcNow,
            FilePath = cameraDir,
            Type = type,
            Status = RecordingStatus.Recording
        };

        var processId = await _ffmpegService.StartRtspIngestAsync(rtspUrl, outputPath);
        if (processId <= 0)
        {
            recording.Status = RecordingStatus.Failed;
            _logger.LogError("Failed to start recording for camera {CameraId}", cameraId);
        }

        await _recordingRepository.AddAsync(recording);
        _activeRecordings[cameraId] = recording;

        _logger.LogInformation("Recording started for camera {CameraId}, type: {Type}", cameraId, type);
        return recording;
    }

    public async Task<Recording?> StopRecordingAsync(string cameraId)
    {
        if (!_activeRecordings.TryRemove(cameraId, out var recording))
        {
            return null;
        }

        recording.EndTime = DateTime.UtcNow;
        recording.Status = RecordingStatus.Completed;

        // Calculate total file size
        if (Directory.Exists(recording.FilePath))
        {
            recording.FileSizeBytes = Directory.GetFiles(recording.FilePath)
                .Sum(f => new FileInfo(f).Length);
        }

        await _recordingRepository.UpdateAsync(recording);

        _logger.LogInformation("Recording stopped for camera {CameraId}. Duration: {Duration}",
            cameraId, recording.EndTime - recording.StartTime);

        return recording;
    }

    public Task<bool> IsRecordingAsync(string cameraId)
    {
        return Task.FromResult(_activeRecordings.ContainsKey(cameraId));
    }

    public Task<IReadOnlyList<Recording>> GetActiveRecordingsAsync()
    {
        IReadOnlyList<Recording> recordings = _activeRecordings.Values.ToList().AsReadOnly();
        return Task.FromResult(recordings);
    }
}
