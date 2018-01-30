using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using TransportClasses;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace PublicTransportWebService
{/*
    public static class MyData
    {
        public class Crds
        {
            public readonly double lat, lng;
            public readonly string hashcode;
            public Crds(double lat, double lng, string hashcode)
            {
                this.lat = lat;
                this.lng = lng;
                this.hashcode = hashcode;
            }
        }
        public static string GetAllStationsMarks()
        {
            Database.Connect();
            List<Station> tmp = Database.GetAllStations();
            Database.Disconnect();
            List<Crds> tmpCrds = new List<Crds>();
            foreach (Station s in tmp) tmpCrds.Add(new Crds(s.xCoord, s.yCoord, s.hashcode));
            string ttt = JsonConvert.SerializeObject(tmpCrds);
            //MessageBox.Show(ttt.Contains("&quot;").ToString());

            return ttt;//.Replace("\"","\\\"");
        }
        public static Station GetStation(string code)
        {
            Database.Connect();
            var result = Database.GetStationByHashcode(code);
            Database.Disconnect();

            return result;
        }
    }*/
}