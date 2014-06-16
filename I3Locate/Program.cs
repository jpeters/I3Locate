using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NativeWifi;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;

namespace I3Locate
{
    class Program
    {
        static private List<Scan> WirelessScans;
        static private string publicIP = "";
        static private string hostname;
        static private readonly string webcamPicturePath = GetTempFolder() + "webcam.jpg";
        static private readonly string screenshotPicturePath = GetTempFolder() + "screenshot.jpg";

        static void Main(string[] args)
        {
            WirelessScans = GetLocalSSIDs();
            List<string> coordinates = GetGeoLocation(WirelessScans);

            TakeScreenshot();
            TakeWebCamPicture();
            publicIP = GetIPPublicAddress();
            List<ComputerIPAddress> address = GetLocalIPAddresses();
            hostname = GetHostName();
        }

        static private List<string> GetGeoLocation(List<Scan> scans)
        {
            //https://maps.googleapis.com/maps/api/browserlocation/json?browser=firefox&sensor=true&wifi=mac:[mac_address]|ssid:[ssid_name]|ss:[rssi]&wifi=mac:[mac_address]|ssid:[ssid_name]|ss:[rssi]

            //GET /maps/api/browserlocation/json?browser=firefox&sensor=true&wifi=mac:01-24-7c-bc-51-46%7Cssid:3x2x%7Css:-37&wifi=mac:09-86-3b-31-97-b2%7Cssid:belkin.7b2%7Css:-47&wifi=mac:28-cf-da-ba-be-13%7Cssid:HERESIARCH%20NETWORK%7Css:-49&wifi=mac:2b-cf-da-ba-be-10%7Cssid: ARCH%20GUESTS%7Css:-52&wifi=mac:08-56-3b-2b-e1-a8%7Cssid:belkin.1a8%7Css:-59&wifi=mac:02-1e-64-fd-df-67%7Cssid:Brown%20Cow%7Css:-59&wifi=mac:2a-cf-df-ba-be-10%7Cssid: ARCH%20GUESTS%7Css:-59 HTTP/1.1
            try
            {
                foreach (Scan myScan in scans)
                {
                    foreach (Reading myReading in myScan.Readings)
                    {
                        string url = @"https://maps.googleapis.com/maps/api/browserlocation/json?browser=firefox&sensor=true&wifi=mac:" + myReading.MAC.Replace(":", "-");

                        WebRequest request = HttpWebRequest.Create(url);

                        WebResponse response = request.GetResponse();

                        StreamReader reader = new StreamReader(response.GetResponseStream());

                        string urlText = reader.ReadToEnd();
                    }
                }
                return null;
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        static private string GetIPPublicAddress()
        {
            try
            {
                string url = "http://checkip.dyndns.org/";

                WebRequest request = HttpWebRequest.Create(url);

                WebResponse response = request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());

                string urlText = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(urlText))
                {
                    return Regex.Split(Regex.Split(urlText, @"<html><head><title>Current IP Check</title></head><body>Current IP Address: ")[1], @"</body></html>")[0];
                }
                else
                {
                    return "";
                }
            }
            catch (Exception exc)
            {
                return "";
            }
        }

        static private string GetHostName()
        {
            return Dns.GetHostName();
        }

        static private string GetDnsAdress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;

                    foreach (IPAddress dnsAdress in dnsAddresses)
                    {
                        return dnsAdress.ToString();
                    }
                }
            }

            throw new InvalidOperationException("Unable to find DNS Address");
        }

        static private List<ComputerIPAddress> GetLocalIPAddresses()
        {
            
            List<ComputerIPAddress> address = new List<ComputerIPAddress>();

            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    address.Add(new ComputerIPAddress(ip.ToString(), ReturnSubnetmask(ip.ToString()), "", GetDnsAdress()));
                }
            }

            return address;
        }

        static public string ReturnSubnetmask(String ipaddress)
        {
            uint firstOctet = ReturnFirtsOctet(ipaddress);
            if (firstOctet >= 0 && firstOctet <= 127)
                return "255.0.0.0";
            else if (firstOctet >= 128 && firstOctet <= 191)
                return "255.255.0.0";
            else if (firstOctet >= 192 && firstOctet <= 223)
                return "255.255.255.0";
            else return "0.0.0.0";
        }

        static public uint ReturnFirtsOctet(string ipAddress)
        {
            System.Net.IPAddress iPAddress = System.Net.IPAddress.Parse(ipAddress);
            byte[] byteIP = iPAddress.GetAddressBytes();
            uint ipInUint = (uint)byteIP[0];
            return ipInUint;
        }

        static private string GetExecutionPath()
        {
            string path;
            path = System.IO.Path.GetDirectoryName(
               System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            return path;
        }

        static private string GetTempFolder()
        {
            return System.Environment.GetEnvironmentVariable("WINDIR") + "\\temp\\";
        }

        static private bool TakeScreenshot()
        {
            //string outputPath = GetTempFolder() + "screenshot.jpg";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = GetExecutionPath() + "\\Screenshot\\preyshot.exe";
            startInfo.Arguments = screenshotPicturePath;
            Process.Start(startInfo);

            return false;
        }

        static private bool TakeWebCamPicture()
        {
            //string outputPath = GetTempFolder() + "webcam.jpg";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = GetExecutionPath() + "\\Webcam\\prey-webcam.exe";
            startInfo.Arguments = "-invalid youcam,cyberlink,google -frame 10 -outfile " + webcamPicturePath;
            Process.Start(startInfo);

            return false;
        }

        static private List<Scan> GetLocalSSIDs()
        {
            List<Scan> myScans = new List<Scan>();
            List<WifiInterface> interfaces = WifiScanner.GetAvailableWLANInterfaces();

            foreach (WifiInterface wifiInterface in interfaces)
            {
                myScans.Add(WifiScanner.ScanWifiSignals(wifiInterface));
            }

            return myScans;
        }

    }

    class ComputerIPAddress
    {
        private string _IPAddress = "";
        private string _Mask = "";
        private string _Gateway = "";
        private string _DNS = "";

        public ComputerIPAddress(string IPAddress, string Mask, string Gateway, string DNS)
        {
            this.IPAddress = IPAddress;
            this.Mask = Mask;
            this.Gateway = Gateway;
            this.DNS = DNS;
        }

        public string IPAddress
        {
            get {return _IPAddress;}
            set {_IPAddress = value;}
        }

        public string Mask
        {
            get {return _Mask;}
            set {_Mask = value;}
        }

        public string Gateway
        {
            get {return _Gateway;}
            set {_Gateway = value;}
        }

        public string DNS
        {
            get {return _DNS;}
            set {_DNS = value;}
        }
    }
    
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
                catch { 
                    // Do nothing.
                }
            }
            return signalStrengthList;
        }
    }
}
