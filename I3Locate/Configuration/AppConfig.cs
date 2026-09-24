using Microsoft.Extensions.Configuration;

namespace I3Locate.Configuration;

/// <summary>
/// Strongly-typed runtime settings, loaded from appsettings.json and overridable
/// by environment variables (prefix <c>I3Locate__</c>).
/// </summary>
public sealed class AppConfig
{
    public string? GoogleGeolocationApiKey { get; init; }
    public string? IpInfoToken { get; init; }
    public string? OutputDirectory { get; init; }
    public bool CaptureWebcam { get; init; } = true;
    public bool CaptureScreenshot { get; init; } = true;
    public string? WebhookUrl { get; init; }
    public int HttpTimeoutSeconds { get; init; } = 30;

    public static AppConfig Load()
    {
        var baseDir = AppContext.BaseDirectory;

        var config = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "I3Locate__")
            .Build();

        var section = config.GetSection("I3Locate");
        var app = section.Get<AppConfig>() ?? new AppConfig();

        // Environment variables without the section prefix are also honoured for
        // convenience when driving the tool from an RMM script.
        return app;
    }

    /// <summary>Resolved directory the report and images are written to.</summary>
    public string ResolveOutputDirectory()
    {
        if (!string.IsNullOrWhiteSpace(OutputDirectory))
        {
            return OutputDirectory!;
        }

        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        if (string.IsNullOrWhiteSpace(programData))
        {
            programData = Path.GetTempPath();
        }

        return Path.Combine(programData, "I3Locate");
    }
}
