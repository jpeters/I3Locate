using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class ComputerIPInterface
    {
        private List<string> _IPAddress = new List<string>();
        private List<string> _Mask = new List<string>();
        private List<string> _Gateway = new List<string>();
        private List<string> _DNS = new List<string>();

        public ComputerIPInterface(List<string> IPAddress, List<string> Mask, List<string> Gateway, List<string> DNS)
        {
            this._IPAddress = IPAddress;
            this._Mask = Mask;
            this._Gateway = Gateway;
            this._DNS = DNS;
        }

        public List<string> IPAddress
        {
            get { return _IPAddress; }
            //set { _IPAddress = value; }
        }

        public List<string> Mask
        {
            get { return _Mask; }
            //set { _Mask = value; }
        }

        public List<string> Gateway
        {
            get { return _Gateway; }
            //set { _Gateway = value; }
        }

        public List<string> DNS
        {
            get { return _DNS; }
            //set { _DNS = value; }
        }
    }
}
