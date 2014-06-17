using NativeWifi;
using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class WifiScanner
    {
        /// <summary>
        /// A wireless lan client.
        /// </summary>
        private static readonly WlanClient WLAN_CLIENT = new WlanClient();

        private WifiScanner() { }

        private static WlanClient.WlanInterface GetNetworkInterfaceFromId(String id)
        {
            WlanClient.WlanInterface networkInterface = null;
            if (!String.IsNullOrEmpty(id))
            {
                foreach (WlanClient.WlanInterface wlanIface in WLAN_CLIENT.Interfaces)
                {
                    if (id == wlanIface.NetworkInterface.Id)
                    {
                        networkInterface = wlanIface;
                    }
                }
            }
            return networkInterface;
        }

        private static string ConvertAddressBytesToString(byte[] macAddr)
        {
            string mac = "";
            if (macAddr != null)
            {
                var macAddrLen = (uint)macAddr.Length;
                var str = new string[(int)macAddrLen];
                for (int i = 0; i < macAddrLen; i++)
                {
                    str[i] = macAddr[i].ToString("x2");
                }
                mac = string.Join(":", str).ToUpper();
            }
            return mac;
        }

        static public List<WifiInterface> GetAvailableWLANInterfaces()
        {
            List<WifiInterface> interfaceList = new List<WifiInterface>();
            foreach (WlanClient.WlanInterface wlanIface in WLAN_CLIENT.Interfaces)
            {
                string id = wlanIface.NetworkInterface.Id;
                string description = wlanIface.NetworkInterface.Description;
                string macAddress = ConvertAddressBytesToString(wlanIface.NetworkInterface.GetPhysicalAddress().GetAddressBytes());
                interfaceList.Add(new WifiInterface(id, description, macAddress));
            }
            return interfaceList;
        }

        static public Scan ScanWifiSignals(WifiInterface wifiInterface)
        {
            List<Reading> readingList = new List<Reading>();
            Scan scan = new Scan(DateTime.UtcNow, readingList);
            if (wifiInterface != null && !String.IsNullOrEmpty(wifiInterface.ID))
            {
                try
                {
                    WlanClient.WlanInterface wlanIface = GetNetworkInterfaceFromId(wifiInterface.ID);
                    Wlan.WlanBssEntry[] wlanBssEntries = wlanIface.GetNetworkBssList();
                    foreach (Wlan.WlanBssEntry wlanBssEntry in wlanBssEntries)
                    {
                        string mac = ConvertAddressBytesToString(wlanBssEntry.dot11Bssid);
                        string ssid = Encoding.ASCII.GetString(wlanBssEntry.dot11Ssid.SSID, 0, (int)wlanBssEntry.dot11Ssid.SSIDLength);
                        int rssi = wlanBssEntry.rssi;
                        if (rssi > 0)
                        {
                            rssi -= 255;
                        }
                        Reading reading = new Reading(mac, ssid, rssi);

                        readingList.Add(reading);
                    }
                    readingList.Sort();
                }
                catch
                {
                    // Do nothing.
                }
            }
            return scan;
        }

        static public List<WifiSignalStrength> ScanForSignalStrengths(WifiInterface wifiInterface)
        {
            List<WifiSignalStrength> signalStrengthList = new List<WifiSignalStrength>();
            if (wifiInterface != null && !String.IsNullOrEmpty(wifiInterface.ID))
            {
                try
                {
                    WlanClient.WlanInterface wlanIface = GetNetworkInterfaceFromId(wifiInterface.ID);
                    Wlan.WlanBssEntry[] wlanBssEntries = wlanIface.GetNetworkBssList();
                    foreach (Wlan.WlanBssEntry wlanBssEntry in wlanBssEntries)
                    {
                        string mac = ConvertAddressBytesToString(wlanBssEntry.dot11Bssid);
                        string ssid = Encoding.ASCII.GetString(wlanBssEntry.dot11Ssid.SSID, 0, (int)wlanBssEntry.dot11Ssid.SSIDLength);
                        int rssi = wlanBssEntry.rssi;
                        if (rssi > 0)
                        {
                            rssi -= 255;
                        }
                        WifiSignalStrength signalStrength = new WifiSignalStrength(ssid, mac, rssi);
                        signalStrengthList.Add(signalStrength);
                    }
                    signalStrengthList.Sort();
                }
                catch
                {
                    // Do nothing.
                }
            }
            return signalStrengthList;
        }
    }
}
