using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class Reading
    {
        private string _SSID;
        private string _MAC;
        private int _RSSI;

        public Reading(string MAC, string SSID, int RSSI)
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
