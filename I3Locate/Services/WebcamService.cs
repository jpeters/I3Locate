using System.Runtime.Versioning;
using OpenCvSharp;

namespace I3Locate.Services;

/// <summary>
/// Grabs a still frame from the first available camera via OpenCV, replacing the
/// bundled DirectShow <c>prey-webcam.exe</c> / <c>snapshot.exe</c> helpers.
/// A few frames are discarded first so the sensor can auto-expose and the image
/// isn't black.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WebcamService
{
    private const int WarmupFrames = 8;
    private const int MaxDeviceIndex = 3;

    public string? Capture(string outputPath)
    {
        for (var device = 0; device <= MaxDeviceIndex; device++)
        {
            try
            {
                using var capture = new VideoCapture(device);
                if (!capture.IsOpened())
                {
                    continue;
                }

                using var frame = new Mat();
                for (var i = 0; i < WarmupFrames; i++)
                {
                    capture.Read(frame);
                }

                if (frame.Empty())
                {
                    continue;
                }

                return Cv2.ImWrite(outputPath, frame) ? outputPath : null;
            }
            catch
            {
                // Device busy / not present: try the next index.
            }
        }

        return null;
    }
}
