using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class Reading : IComparable<Reading>
    {
        /// <summary>
        /// The MAC address of the access point (AP).
        /// </summary>
        private string mac;
        /// <summary>
        /// The SSID of the access point (AP).
        /// </summary>
        private string ssid;
        /// <summary>
        /// The received signal strength in dBm.
        /// </summary>
        private int rssi;


        /// <summary>
        /// Create a new instance of Reading from given parameters.
        /// </summary>
        /// <param name="ssid">The SSID of the access point.</param>
        /// <param name="macAddress">The MAC address of the access point.</param>
        /// <param name="rssi">The received signal strength in dBm.</param>
        public Reading(string mac, string ssid, int rssi)
        {
            this.mac = String.IsNullOrEmpty(mac) ? "" : mac;
            this.ssid = String.IsNullOrEmpty(ssid) ? "" : ssid;
            this.rssi = rssi;
        }

        /// <summary>
        /// Gets the MAC address of the access point.
        /// </summary>
        public string MAC
        {
            get { return this.mac; }
        }

        /// <summary>
        /// Gets  the SSID of the access point.
        /// </summary>
        public string SSID
        {
            get { return this.ssid; }
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
            if (obj != null && (obj is Reading))
            {
                Reading comparedObject = (Reading)obj;
                equals = (this.SSID == comparedObject.SSID)
                          && (this.MAC == comparedObject.MAC)
                          && (this.RSSI == comparedObject.RSSI);
            }
            return equals;
        }

        public override int GetHashCode()
        {
            int hashCode = 37;
            hashCode = (83 * hashCode) + this.ssid.GetHashCode();
            hashCode = (83 * hashCode) + this.mac.GetHashCode();
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
            return String.Format(formatTemplate, this.MAC, this.SSID, this.RSSI);
        }

        public string ToXMLElement()
        {
            return String.Format("<reading mac=\"{0}\" ssid=\"{1}\" rssi=\"{2}\" />\r\n", this.MAC, this.SSID, this.RSSI);
        }

        #region IComparable<Reading> Members

        public int CompareTo(Reading other)
        {
            int result = Int32.MaxValue;
            if (other != null)
            {
                result = this.RSSI.CompareTo(other.RSSI);
                if (result != 0)
                {
                    return (-1) * (result);
                }


                result = this.MAC.CompareTo(other.MAC);
                if (result != 0)
                {
                    return result;
                }

                result = this.SSID.CompareTo(other.SSID);
            }
            return result;
        }

        #endregion
    }
}
