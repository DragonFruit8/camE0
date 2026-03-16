using CamE0.Video.Models;

namespace CamE0.Video.Interfaces;

public interface IMotionDetector
{
    Task<MotionEvent?> DetectMotionAsync(string cameraId, byte[] currentFrame, byte[] previousFrame, int sensitivity = 50);
    Task<IReadOnlyList<MotionEvent>> GetRecentEventsAsync(string cameraId, int count = 50);
    void SetSensitivity(string cameraId, int sensitivity);
    event EventHandler<MotionEvent>? MotionDetected;
}
