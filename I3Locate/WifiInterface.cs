using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class WifiInterface
    {
        //WifiInterface(id, description, macAddress)

        private string _id;
        private string _description;
        private string _macAddress;

        public WifiInterface(string ID, string Description, string MacAddress)
        {
            this.ID = ID;
            this.Description = Description;
            this.MacAddress = MacAddress;
        }

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string MacAddress
        {
            get { return _macAddress; }
            set { _macAddress = value; }
        }
    }
}
