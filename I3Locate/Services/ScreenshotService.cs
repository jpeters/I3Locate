using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace I3Locate.Services;

/// <summary>
/// Captures the primary desktop natively via GDI, replacing the bundled
/// <c>preyshot.exe</c>. Returns the file path on success, or null if no interactive
/// desktop is available (e.g. running as a service with no logged-in session).
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ScreenshotService
{
    public string? Capture(string outputPath)
    {
        try
        {
            var (width, height, left, top) = GetVirtualScreen();
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(left, top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }

            bitmap.Save(outputPath, ImageFormat.Jpeg);
            return outputPath;
        }
        catch
        {
            // No desktop / access denied: skip screenshot rather than fail the run.
            return null;
        }
    }

    // Fall back to primary-monitor metrics without pulling in WinForms.
    private static (int width, int height, int left, int top) GetVirtualScreen()
    {
        const int SM_XVIRTUALSCREEN = 76;
        const int SM_YVIRTUALSCREEN = 77;
        const int SM_CXVIRTUALSCREEN = 78;
        const int SM_CYVIRTUALSCREEN = 79;

        int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
        int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
        int width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
        int height = GetSystemMetrics(SM_CYVIRTUALSCREEN);
        return (width, height, left, top);
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
