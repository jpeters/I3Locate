using System.Text.Json;
using I3Locate.Configuration;
using I3Locate.Models;

namespace I3Locate.Services;

/// <summary>
/// Looks up the machine's public IP and its network ownership / coarse location
/// from ipinfo.io, replacing the scraped <c>checkip.dyndns.org</c> and
/// <c>networksolutions.com/whois</c> pages (both defunct/fragile).
/// </summary>
public sealed class PublicNetworkService
{
    private readonly HttpClient _http;
    private readonly AppConfig _config;

    public PublicNetworkService(HttpClient http, AppConfig config)
    {
        _http = http;
        _config = config;
    }

    public async Task<PublicNetworkInfo?> LookupAsync(CancellationToken ct = default)
    {
        try
        {
            var url = "https://ipinfo.io/json";
            if (!string.IsNullOrWhiteSpace(_config.IpInfoToken))
            {
                url += "?token=" + Uri.EscapeDataString(_config.IpInfoToken!);
            }

            using var response = await _http.GetAsync(url, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return await FallbackIpOnlyAsync(ct).ConfigureAwait(false);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
            var root = doc.RootElement;

            return new PublicNetworkInfo(
                Ip: GetString(root, "ip"),
                City: GetString(root, "city"),
                Region: GetString(root, "region"),
                Country: GetString(root, "country"),
                Org: GetString(root, "org"),
                Postal: GetString(root, "postal"),
                Timezone: GetString(root, "timezone"),
                Loc: GetString(root, "loc"));
        }
        catch
        {
            return await FallbackIpOnlyAsync(ct).ConfigureAwait(false);
        }
    }

    /// <summary>If ipinfo is unreachable, at least resolve the bare public IP via ipify.</summary>
    private async Task<PublicNetworkInfo?> FallbackIpOnlyAsync(CancellationToken ct)
    {
        try
        {
            var ip = (await _http.GetStringAsync("https://api.ipify.org", ct).ConfigureAwait(false)).Trim();
            return string.IsNullOrEmpty(ip)
                ? null
                : new PublicNetworkInfo(ip, null, null, null, null, null, null, null);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;
}
