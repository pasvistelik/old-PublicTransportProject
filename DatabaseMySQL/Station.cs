using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistanceObj = TransportClasses.OptimalRoute.GeoCoords.DistanceObj;

namespace TransportClasses
{
    public class SimpleStation
    {
        public string hashcode;//string
        //public string nameRus;//, nameEn, nameBy;
        public string name;
        //public double xCoord, yCoord;
        public OptimalRoute.GeoCoords coords;
        public List<string> routesCodes;
        public SimpleStation(Station s, int intId)
        {
            hashcode = s.hashcode;
            //nameRus = s.nameRus;
            //nameBy = s.nameBy;
            //nameEn = s.nameEn;
            name = s.name;
            //xCoord = s.lat;
            //yCoord = s.lng;
            coords = s.Coords;
            routesCodes = new List<string>();
            if (s.routes != null) foreach (Route r in s.routes) if (r != null) routesCodes.Add(r.hashcode);
        }
    }
    public class Station
    {
        public string hashcode;
        public string nameRus, nameEn, nameBy;
        public string name, osm_id, osm_version, osm_changeset, osm_timestamp, osm_last_user_name, osm_last_user_id;
        public double lat, lng;
        public List<Route> routes;
        public List<DistanceObj> distances;
        private OptimalRoute.Points.Point point = null;
        public OptimalRoute.Points.Point Point
        {
            get
            {
                return point;
            }
            set
            {
                point = value;
            }
        }
        public OptimalRoute.GeoCoords Coords
        {
            get
            {
                return new OptimalRoute.GeoCoords(lat, lng);
            }
        }

        public static List<string> ConvertJSON_toList(string routesJSON)
        {
            //List<string> tmp = new List<string>();

            //DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(List<string>));


            /*
            List<string> t = new List<string>();
            t.Add("first"); t.Add("second");

            MessageBox.Show(JsonConvert.SerializeObject(t));*/

            //json.WriteObject()


            //List<string> tmp = (List<string>)json.ReadObject(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(routesJSON)));

            //...
            /*List<string> tmp = JsonConvert.DeserializeObject<List<string>>(routesJSON);
            foreach (string s in tmp)
            {
                MessageBox.Show(s.ToString());
            }*/
            return JsonConvert.DeserializeObject<List<string>>(routesJSON);
        }
        public Station(string hash, string rusName, string enName, string byName, int x, int y, string routesJSON, string name = "", string osm_id = "", string osm_version = "", string osm_changeset = "", string osm_timestamp = "", string osm_last_user_name = "", string osm_last_user_id = "")
        {
            hashcode = hash;
            nameRus = rusName;
            nameEn = enName;
            nameBy = byName;
            lat = (double)x / 10000;
            lng = (double)y / 10000;
            if (routesJSON != null) routes = Route.FromList(ConvertJSON_toList(routesJSON));
            this.name = name;
            this.osm_id = osm_id;
            this.osm_version = osm_version;
            this.osm_changeset = osm_changeset;
            this.osm_timestamp = osm_timestamp;
            this.osm_last_user_id = osm_last_user_id;
            this.osm_last_user_name = osm_last_user_name;
        }
        public override string ToString()
        {
            if (nameRus != string.Empty) return nameRus;// Rus;
            else if (name != string.Empty) return name;
            else return nameEn;
        }

        public static List<Station>[] FromList(List<string>[] codes)
        {
            List<Station> tmp1 = new List<Station>();
            List<Station> tmp2 = new List<Station>();

            foreach (string s in codes[0])
            {
                tmp1.Add(Database.GetStationByHashcode(s));
            }
            foreach (string s in codes[1])
            {
                tmp2.Add(Database.GetStationByHashcode(s));
            }

            List<Station>[] tmp = new List<Station>[] { tmp1, tmp2 };
            return tmp;
        }
    }
}
