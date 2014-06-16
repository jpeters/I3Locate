using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class Scan
    {
        private DateTime _timestamp;
        private List<Reading> _readings;

        public Scan(DateTime Timestamp, List<Reading> Readings)
        {
            _timestamp = Timestamp;
            _readings = Readings;
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        public List<Reading> Readings
        {
            get { return _readings; }
            set { _readings = value; }
        }
    }
}
