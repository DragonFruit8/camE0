using System.Diagnostics;
using CamE0.Video.Interfaces;
using CamE0.Video.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CamE0.Video.Services;

/// <summary>
/// FFmpeg integration service for RTSP stream ingest and recording.
/// </summary>
public sealed class FFmpegService : IFFmpegService
{
    private readonly FFmpegSettings _settings;
    private readonly ILogger<FFmpegService> _logger;

    public FFmpegService(IOptions<FFmpegSettings> settings, ILogger<FFmpegService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<int> StartRtspIngestAsync(string rtspUrl, string outputPath, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var arguments = BuildIngestArguments(rtspUrl, outputPath);
        _logger.LogInformation("Starting FFmpeg ingest: {FFmpegPath} {Arguments}", _settings.FFmpegPath, arguments);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _settings.FFmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _logger.LogDebug("FFmpeg: {Data}", e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();

        _logger.LogInformation("FFmpeg process started with PID {ProcessId} for {RtspUrl}", process.Id, rtspUrl);

        // Wait briefly to check if the process started successfully
        await Task.Delay(1000, cancellationToken);

        if (process.HasExited)
        {
            _logger.LogError("FFmpeg process exited immediately with code {ExitCode}", process.ExitCode);
            return -1;
        }

        return process.Id;
    }

    public Task StopProcessAsync(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                _logger.LogInformation("FFmpeg process {ProcessId} stopped", processId);
            }
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("FFmpeg process {ProcessId} not found (already stopped)", processId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping FFmpeg process {ProcessId}", processId);
        }

        return Task.CompletedTask;
    }

    public async Task<byte[]?> CaptureSnapshotAsync(string rtspUrl)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"snapshot_{Guid.NewGuid()}.jpg");
        try
        {
            var arguments = $"-rtsp_transport {_settings.RtspTransport} -i \"{rtspUrl}\" -frames:v 1 -q:v 2 -y \"{tempPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _settings.FFmpegPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && File.Exists(tempPath))
            {
                return await File.ReadAllBytesAsync(tempPath);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing snapshot from {RtspUrl}", rtspUrl);
            return null;
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    public async Task<bool> IsFFmpegAvailableAsync()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _settings.FFmpegPath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetStreamInfoAsync(string rtspUrl)
    {
        try
        {
            var arguments = $"-rtsp_transport {_settings.RtspTransport} -i \"{rtspUrl}\" -show_streams -show_format -print_format json";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _settings.FFprobePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode == 0 ? output : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stream info for {RtspUrl}", rtspUrl);
            return null;
        }
    }

    private string BuildIngestArguments(string rtspUrl, string outputPath)
    {
        var args = new List<string>
        {
            $"-rtsp_transport {_settings.RtspTransport}",
            $"-i \"{rtspUrl}\"",
            $"-c:v {_settings.VideoCodec}",
            $"-c:a {_settings.AudioCodec}",
            "-movflags +frag_keyframe+empty_moov+faststart",
            $"-f segment",
            $"-segment_time {_settings.SegmentDurationSeconds}",
            "-segment_format mp4",
            "-reset_timestamps 1",
            "-strftime 1",
            $"\"{outputPath}\""
        };

        foreach (var (key, value) in _settings.ExtraArgs)
        {
            args.Add($"-{key} {value}");
        }

        return string.Join(" ", args);
    }
}
