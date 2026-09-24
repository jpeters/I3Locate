using System.Runtime.Versioning;
using I3Locate.Models;
using ManagedNativeWifi;

namespace I3Locate.Services;

/// <summary>
/// Enumerates nearby Wi-Fi access points via the native WLAN API
/// (through the maintained <c>ManagedNativeWifi</c> package), replacing the old
/// hand-referenced <c>ManagedWifi.dll</c>.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WifiScanService
{
    /// <summary>
    /// Triggers a fresh scan on every wireless interface, then returns the
    /// discovered BSS networks ordered strongest signal first.
    /// </summary>
    public async Task<IReadOnlyList<WifiReading>> ScanAsync(CancellationToken ct = default)
    {
        // Ask the driver for a fresh scan so we get current neighbours rather than a
        // stale cached list. This covers all wireless interfaces; failure is non-fatal.
        try
        {
            await NativeWifi.ScanNetworksAsync(TimeSpan.FromSeconds(5), ct).ConfigureAwait(false);
        }
        catch
        {
            // No wireless hardware, driver refused, or timeout: fall through and read
            // whatever the cached BSS list holds.
        }

        var readings = new List<WifiReading>();
        try
        {
            foreach (var bss in NativeWifi.EnumerateBssNetworks())
            {
                var bssid = bss.Bssid.ToString();
                if (string.IsNullOrEmpty(bssid))
                {
                    continue;
                }

                var ssid = bss.Ssid.ToString() ?? string.Empty;

                // SignalStrength is already RSSI in dBm (negative values).
                readings.Add(new WifiReading(NormalizeMac(bssid), ssid, bss.SignalStrength));
            }
        }
        catch
        {
            // No wireless hardware / driver: return whatever we have.
        }

        return readings
            .GroupBy(r => r.Bssid)
            .Select(g => g.OrderByDescending(r => r.SignalStrengthDbm).First())
            .OrderByDescending(r => r.SignalStrengthDbm)
            .ToList();
    }

    /// <summary>Normalises a MAC to lower-case colon-separated form (aa:bb:cc:dd:ee:ff).</summary>
    private static string NormalizeMac(string raw)
    {
        var hex = new string(raw.Where(Uri.IsHexDigit).ToArray()).ToLowerInvariant();
        if (hex.Length != 12)
        {
            return raw.ToLowerInvariant();
        }

        return string.Join(':', Enumerable.Range(0, 6).Select(i => hex.Substring(i * 2, 2)));
    }
}
