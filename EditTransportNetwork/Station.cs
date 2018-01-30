using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditTransportNetwork
{
    public class Station
    {
        public string hashcode;
        public string nameRus, nameEn, nameBy;
        public double xCoord, yCoord;
        public List<Route> routes;

        public OptimalRoute.GeoCoords Coords
        {
            get
            {
                return new OptimalRoute.GeoCoords(xCoord, yCoord);
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
        public Station(string hash, string rusName, string enName, string byName, int x, int y, string routesJSON)
        {
            hashcode = hash;
            nameRus = rusName;
            nameEn = enName;
            nameBy = byName;
            xCoord = (double)x / 10000;
            yCoord = (double)y / 10000;
            if(routesJSON!=null) routes = Route.FromList(ConvertJSON_toList(routesJSON));
        }
        public override string ToString()
        {
            return nameRus;
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
