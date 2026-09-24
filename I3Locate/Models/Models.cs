namespace I3Locate.Models;

/// <summary>A single Wi-Fi access point observed during a scan.</summary>
public sealed record WifiReading(string Bssid, string Ssid, int SignalStrengthDbm);

/// <summary>A resolved geographic position.</summary>
public sealed record GeoLocation(
    double Latitude,
    double Longitude,
    double? AccuracyMeters,
    string Source)
{
    public string GoogleMapsUrl =>
        $"https://www.google.com/maps/search/?api=1&query={Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
        $"{Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
}

/// <summary>Public-facing IP details and network ownership (from ipinfo.io).</summary>
public sealed record PublicNetworkInfo(
    string? Ip,
    string? City,
    string? Region,
    string? Country,
    string? Org,
    string? Postal,
    string? Timezone,
    string? Loc);

/// <summary>A local network adapter's addressing.</summary>
public sealed record NetworkAdapterInfo(
    string Name,
    string Description,
    string MacAddress,
    IReadOnlyList<string> IpAddresses,
    IReadOnlyList<string> Gateways,
    IReadOnlyList<string> DnsServers);

/// <summary>Everything gathered in a single run, serialised into the JSON report.</summary>
public sealed record LocationReport
{
    public DateTimeOffset StartedAtUtc { get; init; }
    public DateTimeOffset CompletedAtUtc { get; init; }
    public string MachineName { get; init; } = "";
    public string UserName { get; init; } = "";
    public string OperatingSystem { get; init; } = "";
    public TimeSpan Uptime { get; init; }

    public GeoLocation? Location { get; init; }
    public PublicNetworkInfo? PublicNetwork { get; init; }
    public IReadOnlyList<WifiReading> WifiReadings { get; init; } = Array.Empty<WifiReading>();
    public IReadOnlyList<NetworkAdapterInfo> Adapters { get; init; } = Array.Empty<NetworkAdapterInfo>();

    public string? ScreenshotPath { get; init; }
    public string? WebcamPath { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
