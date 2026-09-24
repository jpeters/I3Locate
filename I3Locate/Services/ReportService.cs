using System.Text;
using System.Text.Json;
using I3Locate.Configuration;
using I3Locate.Models;

namespace I3Locate.Services;

/// <summary>
/// Writes the gathered evidence to disk for the RMM to collect: a human-readable
/// <c>Report.txt</c> and a machine-readable <c>report.json</c>. Optionally POSTs the
/// JSON to a configured webhook.
/// </summary>
public sealed class ReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly HttpClient _http;
    private readonly AppConfig _config;

    public ReportService(HttpClient http, AppConfig config)
    {
        _http = http;
        _config = config;
    }

    public async Task WriteAsync(LocationReport report, string outputDirectory, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputDirectory);

        var json = JsonSerializer.Serialize(report, JsonOptions);
        await File.WriteAllTextAsync(Path.Combine(outputDirectory, "report.json"), json, ct).ConfigureAwait(false);

        var text = BuildTextReport(report);
        await File.WriteAllTextAsync(Path.Combine(outputDirectory, "Report.txt"), text, ct).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(_config.WebhookUrl))
        {
            await TryPostWebhookAsync(json, ct).ConfigureAwait(false);
        }
    }

    private async Task TryPostWebhookAsync(string json, CancellationToken ct)
    {
        try
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _http.PostAsync(_config.WebhookUrl, content, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // The on-disk report is the primary channel; webhook is best-effort.
        }
    }

    private static string BuildTextReport(LocationReport r)
    {
        var sb = new StringBuilder();
        void Header(string title)
        {
            sb.AppendLine("########################################################");
            sb.AppendLine("# " + title);
            sb.AppendLine("########################################################");
            sb.AppendLine();
        }

        sb.AppendLine("This computer came online and reported in.");
        sb.AppendLine();
        sb.AppendLine($"Scan started: {r.StartedAtUtc:u}");
        sb.AppendLine($"Scan ended:   {r.CompletedAtUtc:u}");
        sb.AppendLine();

        if (r.Location is not null)
        {
            Header("Geographic location");
            sb.AppendLine($"Source:    {r.Location.Source}");
            if (r.Location.AccuracyMeters is { } acc)
            {
                sb.AppendLine($"Accuracy:  ~{acc:0} meters");
            }
            sb.AppendLine($"Latitude:  {r.Location.Latitude}");
            sb.AppendLine($"Longitude: {r.Location.Longitude}");
            sb.AppendLine($"Map:       {r.Location.GoogleMapsUrl}");
            sb.AppendLine();
        }

        if (r.PublicNetwork is not null)
        {
            Header("Public network");
            var p = r.PublicNetwork;
            if (!string.IsNullOrEmpty(p.Ip)) sb.AppendLine($"Public IP: {p.Ip}");
            if (!string.IsNullOrEmpty(p.Org)) sb.AppendLine($"Network:   {p.Org}");
            var place = string.Join(", ", new[] { p.City, p.Region, p.Country, p.Postal }
                .Where(s => !string.IsNullOrEmpty(s)));
            if (!string.IsNullOrEmpty(place)) sb.AppendLine($"Location:  {place}");
            if (!string.IsNullOrEmpty(p.Timezone)) sb.AppendLine($"Timezone:  {p.Timezone}");
            sb.AppendLine();
        }

        if (r.Adapters.Count > 0)
        {
            Header("Local network adapters");
            foreach (var a in r.Adapters)
            {
                sb.AppendLine($"{a.Name} ({a.Description})");
                sb.AppendLine($"  MAC:      {a.MacAddress}");
                if (a.IpAddresses.Count > 0) sb.AppendLine($"  IP:       {string.Join(", ", a.IpAddresses)}");
                if (a.Gateways.Count > 0) sb.AppendLine($"  Gateway:  {string.Join(", ", a.Gateways)}");
                if (a.DnsServers.Count > 0) sb.AppendLine($"  DNS:      {string.Join(", ", a.DnsServers)}");
                sb.AppendLine();
            }
        }

        if (r.WifiReadings.Count > 0)
        {
            Header($"Nearby Wi-Fi access points ({r.WifiReadings.Count})");
            foreach (var w in r.WifiReadings)
            {
                var ssid = string.IsNullOrEmpty(w.Ssid) ? "<hidden>" : w.Ssid;
                sb.AppendLine($"  {w.Bssid}  {w.SignalStrengthDbm,4} dBm  {ssid}");
            }
            sb.AppendLine();
        }

        Header("System");
        sb.AppendLine($"Hostname:  {r.MachineName}");
        sb.AppendLine($"User:      {r.UserName}");
        sb.AppendLine($"OS:        {r.OperatingSystem}");
        sb.AppendLine($"Uptime:    {r.Uptime:d\\.hh\\:mm\\:ss}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(r.ScreenshotPath)) sb.AppendLine($"Screenshot saved: {r.ScreenshotPath}");
        if (!string.IsNullOrEmpty(r.WebcamPath)) sb.AppendLine($"Webcam image saved: {r.WebcamPath}");

        if (r.Errors.Count > 0)
        {
            sb.AppendLine();
            Header("Warnings");
            foreach (var e in r.Errors)
            {
                sb.AppendLine("  - " + e);
            }
        }

        return sb.ToString();
    }
}
