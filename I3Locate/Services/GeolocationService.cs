using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using I3Locate.Configuration;
using I3Locate.Models;

namespace I3Locate.Services;

/// <summary>
/// Resolves the machine's position. Prefers precise Wi-Fi triangulation via the
/// Google Geolocation API; falls back to coarse IP-based location so a position is
/// still reported when no API key is configured or no Wi-Fi is present.
///
/// Replaces the dead <c>maps.googleapis.com/maps/api/browserlocation/json</c>
/// endpoint (shut down in 2014).
/// </summary>
public sealed class GeolocationService
{
    private readonly HttpClient _http;
    private readonly AppConfig _config;

    public GeolocationService(HttpClient http, AppConfig config)
    {
        _http = http;
        _config = config;
    }

    public async Task<GeoLocation?> ResolveAsync(
        IReadOnlyList<WifiReading> wifiReadings,
        PublicNetworkInfo? publicNetwork,
        CancellationToken ct = default)
    {
        // The Google Geolocation API needs at least two access points to triangulate.
        if (!string.IsNullOrWhiteSpace(_config.GoogleGeolocationApiKey) && wifiReadings.Count >= 2)
        {
            var precise = await TryGoogleWifiAsync(wifiReadings, ct).ConfigureAwait(false);
            if (precise is not null)
            {
                return precise;
            }
        }

        // Fallback: derive an approximate position from the public IP (ipinfo.io "loc").
        return FromPublicIp(publicNetwork);
    }

    private async Task<GeoLocation?> TryGoogleWifiAsync(
        IReadOnlyList<WifiReading> wifiReadings,
        CancellationToken ct)
    {
        try
        {
            var url = "https://www.googleapis.com/geolocation/v1/geolocate?key=" +
                      Uri.EscapeDataString(_config.GoogleGeolocationApiKey!);

            var body = new
            {
                considerIp = false,
                wifiAccessPoints = wifiReadings.Select(r => new
                {
                    macAddress = r.Bssid,
                    signalStrength = r.SignalStrengthDbm
                }).ToArray()
            };

            using var response = await _http.PostAsJsonAsync(url, body, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);

            var root = doc.RootElement;
            if (!root.TryGetProperty("location", out var loc))
            {
                return null;
            }

            var lat = loc.GetProperty("lat").GetDouble();
            var lng = loc.GetProperty("lng").GetDouble();
            double? accuracy = root.TryGetProperty("accuracy", out var acc) ? acc.GetDouble() : null;

            return new GeoLocation(lat, lng, accuracy, "google-wifi");
        }
        catch
        {
            return null;
        }
    }

    private static GeoLocation? FromPublicIp(PublicNetworkInfo? publicNetwork)
    {
        if (string.IsNullOrWhiteSpace(publicNetwork?.Loc))
        {
            return null;
        }

        var parts = publicNetwork.Loc.Split(',');
        if (parts.Length == 2 &&
            double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
            double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
        {
            // IP geolocation is city-level at best; report a nominal accuracy.
            return new GeoLocation(lat, lng, 5000, "ip");
        }

        return null;
    }
}
