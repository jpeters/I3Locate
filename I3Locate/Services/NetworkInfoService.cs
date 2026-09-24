using System.Net.NetworkInformation;
using I3Locate.Models;

namespace I3Locate.Services;

/// <summary>Reads local network adapter addressing (IP, gateway, DNS, MAC).</summary>
public sealed class NetworkInfoService
{
    public IReadOnlyList<NetworkAdapterInfo> GetAdapters()
    {
        var result = new List<NetworkAdapterInfo>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            if (nic.NetworkInterfaceType is not (NetworkInterfaceType.Wireless80211
                or NetworkInterfaceType.Ethernet
                or NetworkInterfaceType.GigabitEthernet))
            {
                continue;
            }

            var props = nic.GetIPProperties();

            var ips = props.UnicastAddresses
                .Where(a => a.PrefixOrigin is PrefixOrigin.Dhcp or PrefixOrigin.Manual)
                .Select(a => a.Address.ToString())
                .ToList();

            var gateways = props.GatewayAddresses.Select(g => g.Address.ToString()).ToList();
            var dns = props.DnsAddresses.Select(d => d.ToString()).ToList();

            var mac = FormatMac(nic.GetPhysicalAddress());

            result.Add(new NetworkAdapterInfo(nic.Name, nic.Description, mac, ips, gateways, dns));
        }

        return result;
    }

    private static string FormatMac(PhysicalAddress address)
    {
        var bytes = address.GetAddressBytes();
        return bytes.Length == 0
            ? string.Empty
            : string.Join(':', bytes.Select(b => b.ToString("x2")));
    }
}
