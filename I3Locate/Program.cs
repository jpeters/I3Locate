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
        static private DateTime scanStart;
        static private DateTime scanEnd;
        static private string publicIP = "";
        static private string hostname = "";
        static private string whoIsInfo = "";
        static private readonly string webcamPicturePath = GetTempFolder() + "webcam.jpg";
        static private readonly string screenshotPicturePath = GetTempFolder() + "screenshot.jpg";
        static private readonly string reportPath = GetTempFolder() + "Report.txt";
        static private GoogleLocationResult googleResult;
        static private List<ComputerIPInterface> addresses = new List<ComputerIPInterface>();
        static private string modifiedFiles = "";
        
        static void Main(string[] args)
        {
            scanStart = DateTime.Now;
            WirelessScans = GetLocalSSIDs();
            googleResult = GetGeoLocation(WirelessScans);
            TakeScreenshot();
            TakeWebCamPicture();
            publicIP = GetIPPublicAddress();
            addresses = GetMachineIPInformation();
            if (!string.IsNullOrEmpty(publicIP))
            {
                whoIsInfo = GetIPInfo(publicIP);
            }
            hostname = GetHostName();

            scanEnd = DateTime.Now;
            ProduceReport();
        }

        private static string GetIPInfo(string ip)
        {

            WebClient wc = new WebClient();

            StringBuilder sb = new StringBuilder();

            wc.Headers.Set("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.0.19) Gecko/2010031422 Firefox/3.0.19 ( .NET CLR 3.5.30729; .NET4.0E)");

            string html = wc.DownloadString("http://www.networksolutions.com/whois/results.jsp?ip=" + ip);

            string info = GetStringInBetween("<pre style=\"width:550px;overflow:hidden;\">", "</pre>", html);
            info = Regex.Replace(info, "&nbsp;", " ");

            string[] lines = info.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string obj in lines)
            {
                sb.AppendLine(obj);
            }
            return sb.ToString();
        }

        private static string GetStringInBetween(string strBegin, string strEnd, string strSource)
        {
            string result = "";
            int iIndexOfBegin = strSource.IndexOf(strBegin);

            if (iIndexOfBegin != -1)
            {
                strSource = strSource.Substring(iIndexOfBegin + strBegin.Length);

                int iEnd = strSource.IndexOf(strEnd);

                if (iEnd != -1) result = strSource.Substring(0, iEnd);
            }

            return result;
        }

        public static string FindModifiedFiles()
        {
            //Not implemented
            if (Directory.Exists(@"c:\Users"))
            {
                DirectoryInfo directory = new DirectoryInfo(@"c:\Users");
                DateTime from_date = DateTime.Now.AddDays(-3);
                DateTime to_date = DateTime.Now;
                //var files = directory.GetFiles()
                //  .Where(file => file.LastWriteTime >= from_date && file.LastWriteTime <= to_date);
                return "";
            }

            return "";
        }

        public static TimeSpan GetUptime()
        {
            System.DateTime SystemStartTime = DateTime.Now.AddMilliseconds(-Environment.TickCount);
            return DateTime.Now - SystemStartTime;
        }

        private static void ProduceReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Your Computer came online and reported in.");
            sb.AppendLine("");
            sb.AppendLine("Scan initiated at: " + scanStart.ToString() + " and ended at: " + scanEnd.ToString());
            sb.AppendLine("");
            if (googleResult != null)
            {
                if (!string.IsNullOrEmpty(googleResult.Latitude) && !string.IsNullOrEmpty(googleResult.Longitude))
                {
                    sb.AppendLine("########################################################");
                    sb.AppendLine("# Geo Location information");
                    sb.AppendLine("########################################################");
                    sb.AppendLine("");
                    sb.AppendLine("Accuracy of this information within meters: " + googleResult.Accuracy);
                    sb.AppendLine("");
                    sb.AppendLine("Latitude: " + googleResult.Latitude);
                    sb.AppendLine("");
                    sb.AppendLine("Longitude: " + googleResult.Longitude);
                    sb.AppendLine("");
                    sb.AppendLine("Google Maps URL: " + googleResult.GetGoogleMap());
                    sb.AppendLine("");
                }
            }
            
            if (addresses.Count > 0 || !string.IsNullOrEmpty(publicIP) || !string.IsNullOrEmpty(hostname))
            {
                sb.AppendLine("########################################################");
                sb.AppendLine("# Current Network Information");
                sb.AppendLine("########################################################");
                sb.AppendLine("");
                if (!string.IsNullOrEmpty(hostname))
                {
                    sb.AppendLine("Hostname: " + hostname);
                    sb.AppendLine("");
                }
                if (!string.IsNullOrEmpty(publicIP))
                {
                    sb.AppendLine("Public IP Address: " + publicIP);
                    sb.AppendLine("");
                }
                if(addresses.Count > 0)
                {
                    foreach(ComputerIPInterface myInterface in addresses)
                    {
                        foreach (string item in myInterface.IPAddress)
	                    {
                            sb.AppendLine("Local IP Address: " + item);
                            sb.AppendLine("");
	                    }

                        foreach (string item in myInterface.Mask)
                        {
                            sb.AppendLine("Subnet Mask: " + item);
                            sb.AppendLine("");
                        }

                        foreach (string item in myInterface.Gateway)
                        {
                            sb.AppendLine("Gateway: " + item);
                            sb.AppendLine("");
                        }

                        foreach (string item in myInterface.DNS)
                        {
                            sb.AppendLine("DNS: " + item);
                            sb.AppendLine("");
                        }
                    }
                }

                if(!string.IsNullOrEmpty(whoIsInfo))
                {
                    sb.Append(whoIsInfo);
                    sb.AppendLine("");
                }
            }

            sb.AppendLine("########################################################");
            sb.AppendLine("# System information");
            sb.AppendLine("########################################################");
            sb.AppendLine("");
            sb.AppendLine("Start up time: " + DateTime.Now.AddMilliseconds(-Environment.TickCount).ToString());
            sb.AppendLine("");

            StreamWriter file = new StreamWriter(reportPath); 
            file.WriteLine(sb.ToString());
            file.Flush();
            file.Close();
        }

        static private GoogleLocationResult GetGeoLocation(List<Scan> scans)
        {
            //https://maps.googleapis.com/maps/api/browserlocation/json?browser=firefox&sensor=true&wifi=mac:[mac_address]|ssid:[ssid_name]|ss:[rssi]&wifi=mac:[mac_address]|ssid:[ssid_name]|ss:[rssi]

            //GET /maps/api/browserlocation/json?browser=firefox&sensor=true&wifi=mac:01-24-7c-bc-51-46%7Cssid:3x2x%7Css:-37&wifi=mac:09-86-3b-31-97-b2%7Cssid:belkin.7b2%7Css:-47&wifi=mac:28-cf-da-ba-be-13%7Cssid:HERESIARCH%20NETWORK%7Css:-49&wifi=mac:2b-cf-da-ba-be-10%7Cssid: ARCH%20GUESTS%7Css:-52&wifi=mac:08-56-3b-2b-e1-a8%7Cssid:belkin.1a8%7Css:-59&wifi=mac:02-1e-64-fd-df-67%7Cssid:Brown%20Cow%7Css:-59&wifi=mac:2a-cf-df-ba-be-10%7Cssid: ARCH%20GUESTS%7Css:-59 HTTP/1.1
            string attributes = "";
            foreach (Scan myScan in scans)
            {
                foreach (Reading myReading in myScan.ReadingList)
                {
                    if (!string.IsNullOrEmpty(myReading.SSID) && !string.IsNullOrEmpty(myReading.MAC))
                    {
                        string addThis = "&wifi=mac:" + myReading.MAC.Replace(":", "-") + "|ssid:" + myReading.SSID + "|ss:" + myReading.RSSI.ToString();
                        if ((83 + attributes.Length + addThis.Length) < 512)
                        {
                            attributes += attributes + addThis;
                        }
                        //attributes += attributes + "&wifi=mac:" + myReading.MAC.Replace(":", "-") + "|ssid:" + myReading.SSID + "|ss:" + myReading.RSSI.ToString();
                    }
                }
            }
            
            try
            {
                //this is 83 characters
                string url = @"https://maps.googleapis.com/maps/api/browserlocation/json?browser=true&sensor=true$" + attributes;

                string length = url.Length.ToString();

                WebRequest request = HttpWebRequest.Create(url);

                WebResponse response = request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());

                string urlText = reader.ReadToEnd();

                if (urlText.Contains("OK"))
                {
                    return new GoogleLocationResult(urlText);
                }
                else
                {
                    return null;
                }
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

        static private List<ComputerIPInterface> GetMachineIPInformation()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            List<ComputerIPInterface> colAddresses = new List<ComputerIPInterface>();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up && (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                {
                    List<string> unicastIP = new List<string>();
                    List<string> dnsIPs = new List<string>();
                    List<string> subnetMask = new List<string>();
                    List<string> gatewayIP = new List<string>();

                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;
                    GatewayIPAddressInformationCollection gatewayAddresses = ipProperties.GatewayAddresses;
                    UnicastIPAddressInformationCollection ipAddresses = ipProperties.UnicastAddresses;


                    foreach (UnicastIPAddressInformation info in ipAddresses) 
                    {
                        if(info.PrefixOrigin == PrefixOrigin.Dhcp || info.PrefixOrigin == PrefixOrigin.Manual)
                        {
                            unicastIP.Add(info.Address.ToString());
                            subnetMask.Add(info.IPv4Mask.ToString());
                        }
                    }

                    foreach (IPAddress address in dnsAddresses)
                    {
                        dnsIPs.Add(address.ToString());
                    }

                    foreach (GatewayIPAddressInformation gateway in gatewayAddresses)
                    {
                        gatewayIP.Add(gateway.Address.ToString());
                    }

                    colAddresses.Add(new ComputerIPInterface(unicastIP, subnetMask, gatewayIP, dnsIPs));
                }
            }

            return colAddresses;

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

}
