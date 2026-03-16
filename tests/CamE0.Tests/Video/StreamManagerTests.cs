using CamE0.Video.Interfaces;
using CamE0.Video.Models;
using CamE0.Video.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CamE0.Tests.Video;

public class StreamManagerTests
{
    private readonly IStreamManager _streamManager;
    private readonly Mock<IFFmpegService> _ffmpegMock;

    public StreamManagerTests()
    {
        _ffmpegMock = new Mock<IFFmpegService>();
        _ffmpegMock.Setup(f => f.IsFFmpegAvailableAsync()).ReturnsAsync(true);
        var logger = new Mock<ILogger<StreamManager>>().Object;
        _streamManager = new StreamManager(_ffmpegMock.Object, logger);
    }

    [Fact]
    public async Task StartStreamAsync_ReturnsStreamingStatus()
    {
        var stream = await _streamManager.StartStreamAsync("cam1", "rtsp://test/stream");
        Assert.Equal(StreamStatus.Streaming, stream.Status);
        Assert.Equal("cam1", stream.CameraId);
    }

    [Fact]
    public async Task StopStreamAsync_RemovesStream()
    {
        await _streamManager.StartStreamAsync("cam1", "rtsp://test/stream");
        await _streamManager.StopStreamAsync("cam1");

        var stream = await _streamManager.GetStreamAsync("cam1");
        Assert.Null(stream);
    }

    [Fact]
    public async Task GetAllStreamsAsync_ReturnsActiveStreams()
    {
        await _streamManager.StartStreamAsync("cam1", "rtsp://test/stream1");
        await _streamManager.StartStreamAsync("cam2", "rtsp://test/stream2");

        var streams = await _streamManager.GetAllStreamsAsync();
        Assert.Equal(2, streams.Count);
    }

    [Fact]
    public async Task IsStreamingAsync_ActiveStream_ReturnsTrue()
    {
        await _streamManager.StartStreamAsync("cam1", "rtsp://test/stream");
        Assert.True(await _streamManager.IsStreamingAsync("cam1"));
    }

    [Fact]
    public async Task IsStreamingAsync_NoStream_ReturnsFalse()
    {
        Assert.False(await _streamManager.IsStreamingAsync("nonexistent"));
    }

    [Fact]
    public async Task StartStreamAsync_FFmpegUnavailable_ReturnsError()
    {
        _ffmpegMock.Setup(f => f.IsFFmpegAvailableAsync()).ReturnsAsync(false);
        var logger = new Mock<ILogger<StreamManager>>().Object;
        var manager = new StreamManager(_ffmpegMock.Object, logger);

        var stream = await manager.StartStreamAsync("cam1", "rtsp://test/stream");
        Assert.Equal(StreamStatus.Error, stream.Status);
    }
}
