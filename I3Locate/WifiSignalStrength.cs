using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class WifiSignalStrength : IComparable<WifiSignalStrength>
    {
        /// <summary>
        /// The SSID of the access point (AP).
        /// </summary>
        private string ssid;
        /// <summary>
        /// The MAC address of the access point (AP).
        /// </summary>
        private string macAddress;
        /// <summary>
        /// The received signal strength in dBm.
        /// </summary>
        private int rssi;

        /// <summary>
        /// Create a new instance of WifiSignalStrength from given parameters.
        /// </summary>
        /// <param name="ssid">The SSID of the access point.</param>
        /// <param name="macAddress">The MAC address of the access point.</param>
        /// <param name="rssi">The received signal strength in dBm.</param>
        public WifiSignalStrength(string ssid, string macAddress, int rssi)
        {
            this.ssid = String.IsNullOrEmpty(ssid) ? "" : ssid.Trim();
            this.macAddress = String.IsNullOrEmpty(macAddress) ? "" : macAddress;
            this.rssi = rssi;
        }

        /// <summary>
        /// Gets  the SSID of the access point.
        /// </summary>
        public string SSID
        {
            get { return this.ssid; }
        }

        /// <summary>
        /// Gets the MAC address of the access point.
        /// </summary>
        public string MacAddress
        {
            get { return this.macAddress; }
        }

        /// <summary>
        /// Gets the received signal strength in dBm of the access point.
        /// </summary>
        public int RSSI
        {
            get { return this.rssi; }

        }
        public override bool Equals(object obj)
        {
            bool equals = false;
            if (obj != null && (obj is WifiSignalStrength))
            {
                WifiSignalStrength comparedObject = (WifiSignalStrength)obj;
                equals = (this.SSID == comparedObject.SSID)
                          && (this.MacAddress == comparedObject.MacAddress)
                          && (this.RSSI == comparedObject.RSSI);
            }
            return equals;
        }

        public override int GetHashCode()
        {
            int hashCode = 37;
            hashCode = (83 * hashCode) + this.ssid.GetHashCode();
            hashCode = (83 * hashCode) + this.macAddress.GetHashCode();
            hashCode = (83 * hashCode) + this.rssi;
            return hashCode;
        }


        public override string ToString()
        {
            return ToJSON();
        }

        public string ToJSON()
        {
            String formatTemplate = "{{\"mac\": \"{0}\", \"ssid\": \"{1}\", \"rssi\": {2}}}";
            return String.Format(formatTemplate, this.MacAddress, this.SSID, this.RSSI);
        }

        public string ToXMLElement()
        {
            return String.Format("<signalStrength mac=\"{0}\" ssid=\"{1}\" rssi=\"{2}\" />\r\n", this.MacAddress, this.SSID, this.RSSI);
        }

        #region IComparable<WifiSignalStrength> Members

        public int CompareTo(WifiSignalStrength other)
        {
            int result = Int32.MaxValue;
            if (other != null)
            {
                result = this.RSSI.CompareTo(other.RSSI);
                if (result != 0)
                {
                    return (-1) * (result);
                }

                result = this.SSID.CompareTo(other.SSID);
                if (result != 0)
                {
                    return result;
                }

                result = this.MacAddress.CompareTo(other.MacAddress);
            }
            return result;
        }

        #endregion
    }
}
