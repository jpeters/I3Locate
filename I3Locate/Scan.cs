using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class Scan : IComparable<Scan>
    {
        private DateTime capturedAt;
        private IList<Reading> readingList;

        public Scan(DateTime capturedAt, IList<Reading> readingList)
        {
            this.capturedAt = capturedAt;
            this.readingList = readingList;
        }

        public DateTime CapturedAt
        {
            get { return this.capturedAt; }
        }

        public IList<Reading> ReadingList
        {
            get { return this.readingList; }
        }

        public override bool Equals(object obj)
        {
            bool equals = false;
            if (obj != null && (obj is Scan))
            {
                Scan comparedObject = (Scan)obj;
                equals = (this.capturedAt == comparedObject.capturedAt)
                          && (this.readingList.Equals(comparedObject.readingList));
            }
            return equals;
        }

        public override int GetHashCode()
        {
            int hashCode = 37;
            hashCode = (83 * hashCode) + this.CapturedAt.GetHashCode();
            hashCode = (83 * hashCode) + this.ReadingList.GetHashCode();
            return hashCode;
        }


        public override string ToString()
        {
            return ToJSON();
        }

        public string ToJSON()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("{\r\n");
            buffer.Append("\"captured_at\": ");
            string strCapturedAt = capturedAt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            buffer.Append(String.Format("\"{0}\",\r\n", strCapturedAt));
            buffer.Append("\"reading_list\": [\r\n");
            if (this.readingList != null)
            {
                string delimiter = "";
                foreach (Reading r in readingList)
                {
                    if (r != null)
                    {
                        buffer.Append(delimiter);
                        buffer.Append(r.ToJSON());
                        delimiter = ",\r\n";
                    }
                }
            }
            buffer.Append("]\r\n");
            buffer.Append("}");
            return buffer.ToString();
        }

        #region IComparable<Scan> Members
        int IComparable<Scan>.CompareTo(Scan other)
        {
            int result = Int32.MaxValue;
            if (other != null)
            {
                result = this.capturedAt.CompareTo(other.capturedAt);
            }
            return result;
        }
        #endregion
    }
}
