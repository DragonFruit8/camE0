using CamE0.Video.Interfaces;
using CamE0.Video.Models;
using CamE0.Video.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CamE0.Tests.Video;

public class MotionDetectorTests
{
    private readonly IMotionDetector _detector;

    public MotionDetectorTests()
    {
        var logger = new Mock<ILogger<FrameDifferenceMotionDetector>>().Object;
        _detector = new FrameDifferenceMotionDetector(logger);
    }

    [Fact]
    public async Task DetectMotionAsync_IdenticalFrames_ReturnsNull()
    {
        var frame = new byte[1000];
        Array.Fill(frame, (byte)128);

        var result = await _detector.DetectMotionAsync("cam1", frame, frame, 50);
        Assert.Null(result);
    }

    [Fact]
    public async Task DetectMotionAsync_DifferentFrames_DetectsMotion()
    {
        var frame1 = new byte[1000];
        var frame2 = new byte[1000];
        Array.Fill(frame1, (byte)0);
        Array.Fill(frame2, (byte)255);

        var result = await _detector.DetectMotionAsync("cam1", frame1, frame2, 50);
        Assert.NotNull(result);
        Assert.Equal("cam1", result.CameraId);
        Assert.True(result.MotionScore > 0);
    }

    [Fact]
    public async Task GetRecentEventsAsync_NoEvents_ReturnsEmpty()
    {
        var events = await _detector.GetRecentEventsAsync("cam-none");
        Assert.Empty(events);
    }

    [Fact]
    public async Task DetectMotionAsync_RecordsEvents()
    {
        var frame1 = new byte[100];
        var frame2 = new byte[100];
        Array.Fill(frame2, (byte)200);

        await _detector.DetectMotionAsync("cam2", frame1, frame2, 50);
        var events = await _detector.GetRecentEventsAsync("cam2");
        Assert.NotEmpty(events);
    }

    [Fact]
    public void SetSensitivity_DoesNotThrow()
    {
        _detector.SetSensitivity("cam1", 75);
        // No exception means success
    }

    [Fact]
    public async Task MotionDetected_EventFires()
    {
        var fired = false;
        _detector.MotionDetected += (_, _) => fired = true;

        var frame1 = new byte[100];
        var frame2 = new byte[100];
        Array.Fill(frame2, (byte)255);

        await _detector.DetectMotionAsync("cam-event", frame1, frame2, 50);
        Assert.True(fired);
    }
}
