using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class WifiInterface
    {
        /// <summary>
        /// The ID of the wireless LAN network interface.
        /// </summary>
        private string id;
        /// <summary>
        /// The description of the wireless LAN network interface.
        /// </summary>
        private string description;
        /// <summary>
        /// The MAC address of the wireless LAN network interface.
        /// </summary>
        private string macAddress;

        /// <summary>
        /// Creates a new instance of WifiInterface from given parameters.
        /// </summary>
        /// <param name="id">The ID of the wireless LAN network interface.</param>
        /// <param name="Description">The description of the wireless LAN network interface.</param>
        /// <param name="macAddress">The MAC address of the wireless LAN network interface.</param>
        public WifiInterface(string id, string description, string macAddress)
        {
            this.id = (String.IsNullOrEmpty(id)) ? "" : id;
            this.description = (String.IsNullOrEmpty(description)) ? "" : description;
            this.macAddress = (String.IsNullOrEmpty(macAddress)) ? "" : macAddress;
        }

        /// <summary>
        /// Gets the ID of the wireless LAN network interface.
        /// </summary>
        public string ID
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the description of the wireless LAN network interface.
        /// </summary>
        public string Description
        {
            get { return this.description; }
        }

        /// <summary>
        /// Gets the MAC address  of the wireless LAN network interface.
        /// </summary>
        public string MACAddress
        {
            get { return this.macAddress; }
        }

        public override string ToString()
        {
            String formatTemplate = "[id: '{0}', description: '{1}', mac: {2}]";
            return String.Format(formatTemplate, this.ID, this.Description, this.MACAddress);
        }

        public override bool Equals(object obj)
        {
            bool equals = false;
            if (obj != null && (obj is WifiInterface))
            {
                WifiInterface comparedObject = (WifiInterface)obj;
                equals = (this.ID.Equals(comparedObject.ID)
                          && this.Description.Equals(comparedObject.Description)
                          && this.MACAddress.Equals(comparedObject.MACAddress));
            }
            return equals;
        }

        public override int GetHashCode()
        {
            int hashCode = 37;
            hashCode = (83 * hashCode) + this.ID.GetHashCode();
            hashCode = (83 * hashCode) + this.Description.GetHashCode();
            hashCode = (83 * hashCode) + this.MACAddress.GetHashCode();
            return hashCode;
        }
    }
}
