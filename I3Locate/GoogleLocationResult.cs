using System;
using System.Collections.Generic;
using System.Text;

namespace I3Locate
{
    public class GoogleLocationResult
    {
        //What to parse
        //{\n   \"accuracy\" : 48,\n   \"location\" : 
        //{\n      \"lat\" : 00.00000,\n      \"lng\" : -00.0000000\n   },\n   \"status\" : \"OK\"\n}\n"

        private string _accuracy;
        private string _lat;
        private string _lng;
        private string _status;

        public GoogleLocationResult(string input)
        {

            char[] delim = { '\n', ' ', '\n', ':', '{', '}', '\"', ',' };

            string[] words = input.Split(delim);
            List<string> newWords = new List<string>();
            foreach (string word in words)
            {
                if (!string.IsNullOrEmpty(word) && word != "location")
                    newWords.Add(word);
            }

            if (newWords.Count == 8)
            {
                _accuracy = newWords[1];
                _lat = newWords[3];
                _lng = newWords[5];
                _status = newWords[7];
            }
            
        }

        public string GetGoogleMap()
        {

            //http://maps.google.com/maps?z=12&t=m&q=loc:38.9419+-78.3020
            if (!string.IsNullOrEmpty(Latitude) && !string.IsNullOrEmpty(Longitude))
            {

                return "http://maps.google.com/?daddr=" + Latitude + "," + Longitude;
                //return "http://maps.google.com/maps?z=12&t=m&q=loc:" + Latitude + "," + Longitude;
            }
            else
            {
                return "";
            }
        }

        public string Accuracy
        {
            get { return _accuracy; }
        }

        public string Latitude
        {
            get { return _lat; }
        }

        public string Longitude
        {
            get { return _lng; }
        }

        public string Status
        {
            get { return _status; }
        }

    }
}
