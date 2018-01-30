using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditTransportNetwork
{
    public class Route
    {
        public string hashcode;
        public string rusFrom, rusTo, enFrom, enTo, byFrom, byTo, number, type;
        public List<Station>[] stations;

        public Route(string hashcode, string name_rus, string name_en, string name_by, string stationsJSON, string number, string type)
        {
            this.hashcode = hashcode;

            string[] rus = JsonConvert.DeserializeObject<string[]>(name_rus);
            rusFrom = rus[0];
            rusTo = rus[1];

            string[] en = JsonConvert.DeserializeObject<string[]>(name_en);
            enFrom = en[0];
            enTo = en[1];

            string[] by = JsonConvert.DeserializeObject<string[]>(name_by);
            byFrom = by[0];
            byTo = by[1];

            //MessageBox.Show(stationsJSON);
            if (stationsJSON != null)
            {
                List<string>[] tmp = JsonConvert.DeserializeObject<List<string>[]>(stationsJSON);
                stations = Station.FromList(tmp);
            }

            this.number = number;
            this.type = type;
        }
        public static List<Route> FromList(List<string> codes)
        {
            List<Route> tmp = new List<Route>();
            foreach(string s in codes)
            {
                tmp.Add(Database.GetRouteByHashcode(s));
            }
            return tmp;
        }
        public override string ToString()
        {
            return "(" + type + " " + number + ") "+rusFrom + " - " + rusTo;
        }
        public string GetShortName()
        {
            return type + " " + number;
        }
        public string GetName(bool invert = false)
        {
            return ((invert == false) ? (rusFrom + " - " + rusTo + " (" + type + " " + number + ")") : (rusTo + " - " + rusFrom + " (" + type + " " + number + ")"));
        }


        public string getNextStationCodeAfter(string stationCode)
        {
            List<string>[] codes = Database.GetStationsCodesOnRoute(this.hashcode);
            if (codes != null)
            {
                for (int j = 0; j <= 1; j++)
                {
                    for (int i = 0; i < codes[j].Count; i++)
                    {
                        if (codes[j][i] == stationCode)
                        {
                            if (i + 1 < codes[j].Count) return codes[j][i + 1];
                            else return null;
                        }
                    }
                }
            }
            //MessageBox.Show("нет остановок");
            return null;
        }
    }
}
