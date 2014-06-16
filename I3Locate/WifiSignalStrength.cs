using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class WifiSignalStrength
    {
        private string _SSID;
        private string _MAC;
        private int _RSSI;

        public WifiSignalStrength(string SSID, string MAC, int RSSI)
        {
            this.SSID = SSID;
            this.MAC = MAC;
            this.RSSI = RSSI;
        }

        public string SSID
        {
            get { return _SSID; }
            set { _SSID = value; }
        }

        public string MAC
        {
            get { return _MAC; }
            set { _MAC = value; }
        }

        public int RSSI
        {
            get { return _RSSI; }
            set { _RSSI = value; }
        }
    }
}
