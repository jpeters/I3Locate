using System.Runtime.Versioning;
using I3Locate.Configuration;
using I3Locate.Models;
using I3Locate.Services;

namespace I3Locate;

/// <summary>
/// I3Locate — a device-recovery locator. Intended to be launched by an RMM on a
/// company-owned machine that has been reported lost or stolen: it gathers as much
/// location evidence as the hardware allows (Wi-Fi triangulation, public IP and
/// network ownership, local addressing, a screenshot and a webcam still) and writes
/// a report the RMM can collect.
/// </summary>
[SupportedOSPlatform("windows")]
public static class Program
{
    public static async Task<int> Main()
    {
        var startedAt = DateTimeOffset.UtcNow;
        var config = AppConfig.Load();
        var outputDir = config.ResolveOutputDirectory();
        var errors = new List<string>();

        using var http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Clamp(config.HttpTimeoutSeconds, 5, 120))
        };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("I3Locate/2.0");

        // --- Gather Wi-Fi scan (needed for both geolocation and the report) ---
        IReadOnlyList<WifiReading> wifi = Array.Empty<WifiReading>();
        try
        {
            wifi = await new WifiScanService().ScanAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            errors.Add("Wi-Fi scan failed: " + ex.Message);
        }

        // --- Public network info (also drives the IP-based location fallback) ---
        PublicNetworkInfo? publicNetwork = null;
        try
        {
            publicNetwork = await new PublicNetworkService(http, config).LookupAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            errors.Add("Public network lookup failed: " + ex.Message);
        }

        // --- Resolve geographic position ---
        GeoLocation? location = null;
        try
        {
            location = await new GeolocationService(http, config)
                .ResolveAsync(wifi, publicNetwork).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            errors.Add("Geolocation failed: " + ex.Message);
        }

        // --- Local adapters ---
        IReadOnlyList<NetworkAdapterInfo> adapters = Array.Empty<NetworkAdapterInfo>();
        try
        {
            adapters = new NetworkInfoService().GetAdapters();
        }
        catch (Exception ex)
        {
            errors.Add("Adapter enumeration failed: " + ex.Message);
        }

        Directory.CreateDirectory(outputDir);

        // --- Screenshot ---
        string? screenshotPath = null;
        if (config.CaptureScreenshot)
        {
            screenshotPath = new ScreenshotService().Capture(Path.Combine(outputDir, "screenshot.jpg"));
            if (screenshotPath is null)
            {
                errors.Add("Screenshot unavailable (no interactive desktop?).");
            }
        }

        // --- Webcam ---
        string? webcamPath = null;
        if (config.CaptureWebcam)
        {
            webcamPath = new WebcamService().Capture(Path.Combine(outputDir, "webcam.jpg"));
            if (webcamPath is null)
            {
                errors.Add("Webcam unavailable (no camera or in use).");
            }
        }

        var report = new LocationReport
        {
            StartedAtUtc = startedAt,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            MachineName = Environment.MachineName,
            UserName = Environment.UserName,
            OperatingSystem = Environment.OSVersion.VersionString,
            Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64),
            Location = location,
            PublicNetwork = publicNetwork,
            WifiReadings = wifi,
            Adapters = adapters,
            ScreenshotPath = screenshotPath,
            WebcamPath = webcamPath,
            Errors = errors
        };

        await new ReportService(http, config).WriteAsync(report, outputDir).ConfigureAwait(false);

        Console.WriteLine($"I3Locate report written to: {outputDir}");
        return 0;
    }
}
