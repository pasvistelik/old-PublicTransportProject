using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransportClasses
{
    public class SimpleRoute
    {
        public readonly List<string> vehicles;
        public readonly string hashcode;
        //public readonly string rusFrom, rusTo, enFrom, enTo, byFrom, byTo;
        public readonly string number, type;
        public readonly string from, to;
        //public readonly string[] osm_id, osm_version, osm_changeset, osm_timestamp, osm_last_user_name, osm_last_user_id;
        //public readonly string osm_num;
        public readonly string owner;
        public readonly List<string>[] stationsCodes;
        public SimpleRoute(Route route)
        {
            hashcode = route.hashcode;
            //rusFrom = route.rusFrom;
            //rusTo = route.rusTo;
            //enFrom = route.enFrom;
            //enTo = route.enTo;
            //byFrom = route.byFrom;
            //byTo = route.byTo;
            from = route.from;
            to = route.to;
            number = route.number;
            type = route.type;
            //osm_id = route.osm_id;
            //osm_version = route.osm_version;
            //osm_changeset = route.osm_changeset;
            //osm_timestamp = route.osm_timestamp;
            //osm_last_user_name = route.osm_last_user_name;
            //osm_last_user_id = route.osm_last_user_id;
            //osm_num = route.osm_num;
            owner = route.owner;
            if (route.vehicles == null) vehicles = null;
            else
            {
                List<string> tmp = new List<string>();
                List<Vehicle> rVehicles = route.vehicles;
                foreach (Vehicle v in rVehicles) if (v != null) tmp.Add(v.Hashcode);
                vehicles = tmp;
            }
            if (route.stations == null) stationsCodes = null;
            else
            {
                List<string>[] tmp = new List<string>[2];
                tmp[0] = new List<string>();
                List<Station> rStations0 = route.stations[0];
                if (rStations0 != null) foreach (Station s in rStations0) if (s != null) tmp[0].Add(s.hashcode);
                tmp[1] = new List<string>();
                List<Station> rStations1 = route.stations[1];
                if (rStations1 != null) foreach (Station s in rStations1) if (s != null) tmp[1].Add(s.hashcode);
                stationsCodes = tmp;
            }
        }
    }

    public class Route
    {
        public List<Vehicle> vehicles = null;
        public List<OptimalRoute.GeoCoords> gpsTrack = null;
        public string hashcode;
        public string rusFrom, rusTo, enFrom, enTo, byFrom, byTo, number, type;
        public string from, to;
        public string[] osm_id, osm_version, osm_changeset, osm_timestamp, osm_last_user_name, osm_last_user_id;
        public string osm_num, owner;
        public List<Station>[] stations = null;
        public List<Timetable>[] timetables = null;
        public string stationsJSON;
        public Route(string hashcode, string name, string name_rus, string name_en, string name_by, string stationsJSON, string number, string type, string owner = "", string osm_id = "[\"\",\"\"]", string osm_version = "[\"\",\"\"]", string osm_changeset = "[\"\",\"\"]", string osm_timestamp = "[\"\",\"\"]", string osm_last_user_name = "[\"\",\"\"]", string osm_last_user_id = "[\"\",\"\"]", string osm_num = "")
        {
            try
            {
                vehicles = new List<Vehicle>();

                this.hashcode = hashcode;

                string[] rus = JsonConvert.DeserializeObject<string[]>(name_rus);
                rusFrom = rus[0];
                rusTo = rus[1];

                string[] nameOSM = JsonConvert.DeserializeObject<string[]>(name);
                from = nameOSM[0];
                to = nameOSM[1];

                string[] en = JsonConvert.DeserializeObject<string[]>(name_en);
                enFrom = en[0];
                enTo = en[1];

                string[] by = JsonConvert.DeserializeObject<string[]>(name_by);
                byFrom = by[0];
                byTo = by[1];

                this.osm_id = JsonConvert.DeserializeObject<string[]>(osm_id);
                this.osm_version = JsonConvert.DeserializeObject<string[]>(osm_version);
                this.osm_changeset = JsonConvert.DeserializeObject<string[]>(osm_changeset);
                this.osm_timestamp = JsonConvert.DeserializeObject<string[]>(osm_timestamp);
                this.osm_last_user_name = JsonConvert.DeserializeObject<string[]>(osm_last_user_name);
                this.osm_last_user_id = JsonConvert.DeserializeObject<string[]>(osm_last_user_id);

                //MessageBox.Show(stationsJSON);
                this.stationsJSON = stationsJSON;
                if (stationsJSON != null)
                {
                    //111111List<string>[] tmp = JsonConvert.DeserializeObject<List<string>[]>(stationsJSON);
                    //111stations = Station.FromList(tmp);
                }

                this.number = number;
                this.type = type;
                this.osm_num = osm_num;
                this.owner = owner;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        public static List<Route> FromList(List<string> codes)
        {
            List<Route> tmp = new List<Route>();
            foreach (string s in codes)
            {
                Route r = Database.GetRouteByHashcode(s);
                //r.stations = null;
                tmp.Add(r);/////////////////////////////////////////////////////
            }
            return tmp;
        }
        public override string ToString()
        {
            string nm = osm_num == string.Empty ? number : osm_num;
            return "(" + type + " " + nm + ") " + rusFrom + " - " + rusTo;
        }
        public string GetShortName()
        {
            return type + " " + number;
        }
        public string GetName(bool invert = false)
        {
            return ((invert == false) ? (rusFrom + " - " + rusTo + " (" + type + " " + number + ")") : (rusTo + " - " + rusFrom + " (" + type + " " + number + ")"));
        }


        public string getNextStationCodeAfter(string stationCode, bool canReadDataFromLocalCopy = false)
        {
            List<string>[] codes = Database.GetStationsCodesOnRoute(this.hashcode, canReadDataFromLocalCopy: canReadDataFromLocalCopy);
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
        public Station getNextStation(Station currentStation)
        {
            for (int j = 0; j <= 1; j++)
            {
                for (int i = 0, n = stations[j].Count; i < n; i++)
                {
                    if (stations[j][i] == currentStation)
                    {
                        if (i + 1 != n) return stations[j][i + 1];
                        else return null;
                    }
                }
            }
            return null;
        }
        public Station getPreviousStation(Station currentStation)
        {
            for (int j = 0; j <= 1; j++)
            {
                for (int i = 0, n = stations[j].Count; i < n; i++)
                {
                    if (stations[j][i] == currentStation)
                    {
                        if (i != 0) return stations[j][i - 1];
                        else return null;
                    }
                }
            }
            return null;
        }
        public Timetable GetTimetable(Station station)
        {
            for (int j = 0; j <= 1; j++)
            {
                for (int i = 0, n = stations[j].Count; i < n; i++)
                {
                    if (stations[j][i] == station)
                    {
                        return timetables[j][i];
                    }
                }
            }
            return null;
        }
    }
}
