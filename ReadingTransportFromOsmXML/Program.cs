using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TransportClasses;

namespace ReadingTransportFromOsmXML
{

    class Program
    {
        public static readonly string OSM_Filepath = "A:\\test.xml";

        /*static string getXmlCode(string lat, string lon, string name)
        {
            string result = "<Placemark><name>"+name+"</name><open>1</open><LookAt><longitude>"+lon+"</longitude><latitude>"+lat+"</latitude><altitude>0</altitude><heading>0</heading><tilt>0</tilt><range>433.7415166391805</range><gx:altitudeMode>relativeToSeaFloor</gx:altitudeMode></LookAt><styleUrl >#m_ylw-pushpin</styleUrl><Point><gx:drawOrder>1</gx:drawOrder><coordinates>"+lon+","+lat+",0</coordinates></Point></Placemark>";
            return result;
        }
        static void SaveKML(List<string[]> tmp, string filename)
        {
            //StringBuilder str = new StringBuilder("");

            //str.Append("</Document></kml>");
            List<string> lines = new List<string>();
            //StreamWriter outputFile = new StreamWriter(filename, true);
            lines.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:gx=\"http://www.google.com/kml/ext/2.2\" xmlns:kml=\"http://www.opengis.net/kml/2.2\" xmlns:atom=\"http://www.w3.org/2005/Atom\"><Document><Folder><name>12345</name><open>1</open><StyleMap id=\"m_ylw-pushpin\"><Pair><key>normal</key><styleUrl>#s_ylw-pushpin</styleUrl></Pair><Pair><key>highlight</key><styleUrl>#s_ylw-pushpin_hl</styleUrl></Pair></StyleMap><Style id=\"s_ylw-pushpin_hl\"><IconStyle><scale>1.4</scale><Icon><href>http://maps.google.com/mapfiles/kml/shapes/caution.png</href></Icon><hotSpot x=\"0.5\" y=\"0\" xunits=\"fraction\" yunits=\"fraction\"/></IconStyle><ListStyle></ListStyle></Style><Style id=\"s_ylw-pushpin\"><IconStyle><scale>1.2</scale><Icon><href>http://maps.google.com/mapfiles/kml/shapes/caution.png</href></Icon><hotSpot x=\"0.5\" y=\"0\" xunits=\"fraction\" yunits=\"fraction\"/></IconStyle><ListStyle></ListStyle></Style>");
            foreach (string[] arr in tmp)
            {
                lines.Add(getXmlCode(arr[0], arr[1], arr[2]));
            }
            lines.Add("</Folder></Document></kml>");

            
            System.IO.File.WriteAllLines(filename, lines);
        }

        private static void SaveJsonCrds(List<Crds> rr, string filename)
        {
            System.IO.File.WriteAllLines(filename, new string[] { JsonConvert.SerializeObject(rr) });
        }*/

        static IEnumerable<XElement> StreamRootChildDoc(string filepath, string nodeName)//StringReader
        {
            using (XmlReader reader = XmlReader.Create(filepath))
            {
                reader.MoveToContent();
                // Parse the file and display each of the nodes.  
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == nodeName)
                            {
                                XElement el = XElement.ReadFrom(reader) as XElement;
                                if (el != null)
                                    yield return el;
                            }
                            break;
                    }
                }
            }
        }

        /*struct Crds
        {
            public double lat, lng;
            public Crds(string lat, string lon)
            {
                this.lat = double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture);
                this.lng = double.Parse(lon, System.Globalization.CultureInfo.InvariantCulture); ;
            }
        }*/



        static void InsertNewStationsFormOsmStationList(MySqlConnection conn, List<OsmStation> osmStations)
        {
            string tableName = "stations";
            int n = osmStations.Count;
            for (int i = 0; i < n; i += 100)
            {
                StringBuilder tmp = new StringBuilder("INSERT INTO `" + conn.Database + "`.`" + tableName + "` (`hashcode`, `x_coord`, `y_coord`, `name`, `name_by`, `name_rus`, `name_en`, `osm_id`, `osm_version`, `osm_changeset`, `osm_timestamp`, `osm_last_user_name`, `osm_last_user_id`) VALUES ");
                for (int j = i; j < n && j < i + 100; j++)
                {
                    tmp.Append(osmStations[j].ToMySqlFragment());
                }
                tmp.Remove(tmp.Length - 1, 1);
                tmp.Append(";");
                string sql = tmp.ToString();
                //Console.WriteLine(sql);
                MySqlScript script = new MySqlScript(conn, sql);
                script.Execute();
            }
        }
        static void SaveToMySQL(List<OsmStation> osmStations)
        {
            //Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            MySqlConnection conn = new MySqlConnection(JsonConvert.DeserializeObject<string>(File.ReadAllText("config.json")));//MySqlConnection 
            //Console.WriteLine(JsonConvert.SerializeObject(connStr));
            conn.Open();

            //stationLat, stationLon, stationNameOSM, stationNameByOSM, stationNameRuOSM, stationNameEnOSM, stationIdOSM, stationVersionOSM, stationChangesetOSM, stationTimestampOSM, stationLastUserNameOSM, stationLastUserIdOSM

            InsertNewStationsFormOsmStationList(conn, osmStations);

            conn.Close();
        }

        static List<OsmStation> ReadOsmStations()
        {
            List<OsmStation> osmStations = new List<OsmStation>();
            foreach (XElement e in StreamRootChildDoc(OSM_Filepath, "node"))//A:\\
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(e.ToString());
                XmlNode node = xDoc.FirstChild;

                if (node.SelectSingleNode("tag[@k=\"public_transport\" and @v=\"platform\"]") != null || node.SelectSingleNode("tag[@k=\"highway\" and @v=\"bus_stop\"]") != null)
                {
                    string stationLat, stationLon, stationNameOSM, stationNameByOSM, stationNameRuOSM, stationNameEnOSM, stationIdOSM, stationVersionOSM, stationChangesetOSM, stationTimestampOSM, stationLastUserNameOSM, stationLastUserIdOSM;
                    try
                    {
                        stationNameOSM = node.SelectSingleNode("tag[@k=\"name\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        stationNameOSM = string.Empty;
                    }
                    try
                    {
                        stationNameByOSM = node.SelectSingleNode("tag[@k=\"name:be\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        stationNameByOSM = string.Empty;
                    }
                    try
                    {
                        stationNameRuOSM = node.SelectSingleNode("tag[@k=\"name:ru\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        stationNameRuOSM = string.Empty;
                    }
                    try
                    {
                        stationNameEnOSM = node.SelectSingleNode("tag[@k=\"name:en\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        stationNameEnOSM = string.Empty;
                    }

                    stationLat = node.Attributes["lat"].Value;
                    stationLon = node.Attributes["lon"].Value;
                    stationIdOSM = node.Attributes["id"].Value;
                    stationVersionOSM = node.Attributes["version"].Value;
                    stationChangesetOSM = node.Attributes["changeset"].Value;
                    stationTimestampOSM = node.Attributes["timestamp"].Value;
                    stationLastUserNameOSM = node.Attributes["user"].Value;
                    stationLastUserIdOSM = node.Attributes["uid"].Value;

                    //Console.WriteLine("[" + stationLat + "; " + stationLon + "] " + stationNameOSM);

                    osmStations.Add(new OsmStation(stationLat, stationLon, stationNameOSM, stationNameByOSM, stationNameRuOSM, stationNameEnOSM, stationIdOSM, stationVersionOSM, stationChangesetOSM, stationTimestampOSM, stationLastUserNameOSM, stationLastUserIdOSM));

                    //count++;

                    //rr.Add(new Crds(stationLat, stationLon));////////////////////
                    //INFO.Add(new string[] { stationLat, stationLon, stationNameOSM });
                }
            }
            return osmStations;
        }





        static Dictionary<string, string> GetDictinaryOsmVsStations()
        {
            Database.Connect();
            List<Station> stations = Database.GetAllStations();
            Database.Disconnect();
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (Station st in stations) if (st.osm_id != null) result.Add(st.osm_id, st.hashcode);
            MessageBox.Show("Связано " + result.Count + " элементов.");
            return result;
        }
        static void InsertNewStationsFormOsmRouteList(MySqlConnection conn, List<OsmGroupTwoRoutes> osmGroupTwoRoutes)
        {
            string tableName = "routes";

            Dictionary<string, string> dictinaryOsmVsStations = GetDictinaryOsmVsStations();

            foreach (OsmGroupTwoRoutes gr in osmGroupTwoRoutes)
            {
                
                List<string> stationsIdsListFirst = new List<string>();
                foreach (string osmId in gr.stations_ids_osm[0]) if (dictinaryOsmVsStations.ContainsKey(osmId)) stationsIdsListFirst.Add(dictinaryOsmVsStations[osmId]);
                List<string> stationsIdsListSecond = new List<string>();
                foreach (string osmId in gr.stations_ids_osm[1]) if (dictinaryOsmVsStations.ContainsKey(osmId)) stationsIdsListSecond.Add(dictinaryOsmVsStations[osmId]);
                string stationsJSON = JsonConvert.SerializeObject(new List<string>[] { stationsIdsListFirst, stationsIdsListSecond });

                string tmp =
                    "INSERT INTO `"
                    + conn.Database
                    + "`.`" + tableName
                    + "` (`hashcode`, `stations`, `type`, `name`, `name_by`, `name_rus`, `name_en`, `osm_id`, `osm_version`, `osm_changeset`, `osm_timestamp`, `osm_last_user_name`, `osm_last_user_id`, `osm_num`) VALUES "
                    + "('" + gr.hashcode + "', '"
                    + stationsJSON + "', '"
                    + gr.type + "', '"
                    + "[\"" + gr.from + "\", \"" + gr.to + "\"]', '"
                    + "[\"" + gr.from + "\", \"" + gr.to + "\"]', '"
                    + "[\"" + gr.from + "\", \"" + gr.to + "\"]', '"
                    + "[\"" + gr.from + "\", \"" + gr.to + "\"]', '"
                    + "[\"" + gr.osm_id[0] + "\", \"" + gr.osm_id[1] + "\"]', '"
                    + "[\"" + gr.osm_version[0] + "\", \"" + gr.osm_version[1] + "\"]', '"
                    + "[\"" + gr.osm_changeset[0] + "\", \"" + gr.osm_changeset[1] + "\"]', '"
                    + "[\"" + gr.osm_timestamp[0] + "\", \"" + gr.osm_timestamp[1] + "\"]', '"
                    + "[\"" + gr.osm_last_user_name[0] + "\", \"" + gr.osm_last_user_name[1] + "\"]', '"
                    + "[\"" + gr.osm_last_user_id[0] + "\", \"" + gr.osm_last_user_id[1] + "\"]', '"
                    + gr.num
                    + "');";

                MySqlScript script = new MySqlScript(conn, tmp);
                script.Execute();

                Database.Connect();
                foreach (string code in stationsIdsListFirst) Database.AddRouteToStation(code, gr.hashcode, Database.conn);
                foreach (string code in stationsIdsListSecond) if(!stationsIdsListFirst.Contains(code)) Database.AddRouteToStation(code, gr.hashcode, Database.conn);
                Database.Disconnect();
            }

            
            /*int n = osmGroupTwoRoutes.Count;
            for (int i = 0; i < n; i += 100)
            {
                StringBuilder tmp = new StringBuilder("INSERT INTO `" + conn.Database + "`.`" + tableName + "` (`hashcode`, `stations`, `type`, `name`, `name_by`, `name_rus`, `name_en`, `osm_id`, `osm_version`, `osm_changeset`, `osm_timestamp`, `osm_last_user_name`, `osm_last_user_id`) VALUES ");
                for (int j = i; j < n && j < i + 100; j++)
                {
                    //tmp.Append(osmRoutes[j].ToMySqlFragment());
                }
                tmp.Remove(tmp.Length - 1, 1);
                tmp.Append(";");
                string sql = tmp.ToString();
                //Console.WriteLine(sql);
                MySqlScript script = new MySqlScript(conn, sql);
                script.Execute();
            }*/
        }
        static void SaveToMySQL(List<OsmGroupTwoRoutes> osmGroupTwoRoutes)
        {
            //Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            MySqlConnection conn = new MySqlConnection(JsonConvert.DeserializeObject<string>(File.ReadAllText("config.json")));//MySqlConnection 
            //Console.WriteLine(JsonConvert.SerializeObject(connStr));
            conn.Open();

            //stationLat, stationLon, stationNameOSM, stationNameByOSM, stationNameRuOSM, stationNameEnOSM, stationIdOSM, stationVersionOSM, stationChangesetOSM, stationTimestampOSM, stationLastUserNameOSM, stationLastUserIdOSM

            InsertNewStationsFormOsmRouteList(conn, osmGroupTwoRoutes);

            conn.Close();
        }

        static List<OsmRoute> ReadOsmRoutes()
        {
            //int count = 0;
            List<OsmRoute> osmRoutes = new List<OsmRoute>();
            foreach (XElement e in StreamRootChildDoc(OSM_Filepath, "relation"))//A:\\
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(e.ToString());
                XmlNode node = xDoc.FirstChild;

                if (node.SelectSingleNode("tag[@k=\"type\" and @v=\"route\"]") != null/* || node.SelectSingleNode("tag[@k=\"route\" and @v=\"bus\"]") != null*/)
                {
                    string routeNumOSM, routeNameOsm, routeNameByOSM, routeNameRuOSM, routeNameEnOSM, routeFromOsm, routeToOsm, routeTypeOsm, routeIdOsm, routeVersionOSM, routeChangesetOSM, routeTimestampOSM, routeLastUserNameOSM, routeLastUserIdOSM;
                    try
                    {
                        routeNameOsm = node.SelectSingleNode("tag[@k=\"name\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeNameOsm = string.Empty;
                    }
                    try
                    {
                        routeNumOSM = node.SelectSingleNode("tag[@k=\"ref\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeNumOSM = string.Empty;
                    }
                    try
                    {
                        routeFromOsm = node.SelectSingleNode("tag[@k=\"from\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeFromOsm = string.Empty;
                    }
                    try
                    {
                        routeToOsm = node.SelectSingleNode("tag[@k=\"to\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeToOsm = string.Empty;
                    }
                    try
                    {
                        routeTypeOsm = node.SelectSingleNode("tag[@k=\"route\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeTypeOsm = string.Empty;
                    }
                    try
                    {
                        routeNameByOSM = node.SelectSingleNode("tag[@k=\"name:be\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeNameByOSM = string.Empty;
                    }
                    try
                    {
                        routeNameRuOSM = node.SelectSingleNode("tag[@k=\"name:ru\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeNameRuOSM = string.Empty;
                    }
                    try
                    {
                        routeNameEnOSM = node.SelectSingleNode("tag[@k=\"name:en\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeNameEnOSM = string.Empty;
                    }

                    routeIdOsm = node.Attributes["id"].Value;
                    routeVersionOSM = node.Attributes["version"].Value;
                    routeChangesetOSM = node.Attributes["changeset"].Value;
                    routeTimestampOSM = node.Attributes["timestamp"].Value;
                    routeLastUserNameOSM = node.Attributes["user"].Value;
                    routeLastUserIdOSM = node.Attributes["uid"].Value;

                    XmlNodeList nodeList = node.SelectNodes("member[@type=\"node\"]");

                    List<string> StationsIdsOsm = new List<string>();
                    foreach (XmlNode xnd in nodeList) StationsIdsOsm.Add(xnd.Attributes["ref"].Value);

                    //Console.WriteLine(routeIdOsm + " : " + routeNameOsm);
                    
                    
                    //Console.WriteLine(StationsIdsOsm[0]);
                    //Console.WriteLine();

                    osmRoutes.Add(new OsmRoute(StationsIdsOsm, routeNameOsm, routeNameByOSM, routeNameRuOSM, routeNameEnOSM, routeFromOsm, routeToOsm, routeTypeOsm, routeIdOsm, routeVersionOSM, routeChangesetOSM, routeTimestampOSM, routeLastUserNameOSM, routeLastUserIdOSM, routeNumOSM));


                    //count++;

                    //rr.Add(new Crds(stationLat, stationLon));////////////////////
                    //INFO.Add(new string[] { stationLat, stationLon, stationNameOSM });
                }
            }
            //Console.WriteLine(count + " elements");
            return osmRoutes;
        }


        static List<OsmGroupTwoRoutes> ReadOsmGroupsTwoRoutes(List<OsmRoute> osmRoutes)
        {
            //int count = 0;
            List<OsmGroupTwoRoutes> osmGroupsTwoRoutes = new List<OsmGroupTwoRoutes>();
            LinkedList<OsmRoute> lst = new LinkedList<OsmRoute>(osmRoutes);
            while (lst.Count != 0)
            {
                OsmRoute first = lst.First.Value;
                lst.RemoveFirst();
                string from = first.routeFromOsm;
                string to = first.routeToOsm;
                if (to == string.Empty || from == string.Empty) continue;
                OsmRoute second;
                foreach (OsmRoute node in lst)
                {
                    if (to.Equals(node.routeFromOsm, StringComparison.InvariantCultureIgnoreCase) && from.Equals(node.routeToOsm, StringComparison.InvariantCultureIgnoreCase))
                    {
                        second = node;
                        lst.Remove(node);

                        string[] osm_id, osm_version, osm_changeset, osm_timestamp, osm_last_user_name, osm_last_user_id;
                        osm_id = new string[] { first.routeIdOsm, second.routeIdOsm };
                        osm_version = new string[] { first.routeVersionOSM, second.routeVersionOSM };
                        osm_changeset = new string[] { first.routeChangesetOSM, second.routeChangesetOSM };
                        osm_timestamp = new string[] { first.routeTimestampOSM, second.routeTimestampOSM };
                        osm_last_user_name = new string[] { first.routeLastUserNameOSM, second.routeLastUserNameOSM };
                        osm_last_user_id = new string[] { first.routeLastUserIdOSM, second.routeLastUserIdOSM };
                        List<string>[] stations_ids_osm = new List<string>[] { first.StationsIdsOsm, second.StationsIdsOsm };

                        osmGroupsTwoRoutes.Add(new OsmGroupTwoRoutes(first.routeTypeOsm, from, to, stations_ids_osm, osm_id, osm_version, osm_changeset, osm_timestamp, osm_last_user_name, osm_last_user_id, first.routeNumOSM));

                        //count++;
                        break;
                    }
                }
            }
            //Console.WriteLine("\n---------------------------------\nFinded " + count + " groups from routes.");

            /*LinkedList<OsmRoute> lst = new LinkedList<OsmRoute>(osmRoutes);
            while (lst.Count != 0)
            {
                metka:
                OsmRoute first = lst.First.Value;
                lst.RemoveFirst();
                OsmRoute second;
                foreach (OsmRoute node in lst)
                {
                    foreach (string codeSecond in node.StationsIdsOsm)
                    {
                        foreach (string codeFirst in first.StationsIdsOsm)
                        {
                            if (codeFirst == codeSecond)
                            {
                                second = node;
                                lst.Remove(node);
                                osmGroupsTwoRoutes.Add(new OsmGroupTwoRoutes())
                                goto metka;
                            }
                        }
                    }
                }
            }*/

            /*
            foreach (XElement e in StreamRootChildDoc(OSM_Filepath, "relation"))//A:\\
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(e.ToString());
                XmlNode node = xDoc.FirstChild;

                if (node.SelectSingleNode("tag[@k=\"type\" and @v=\"route_master\"]") != null)// || node.SelectSingleNode("tag[@k=\"route\" and @v=\"bus\"]") != null
            {
                string routeGroupNameOsm, routeGroupNameByOSM, routeGroupNameRuOSM, routeGroupNameEnOSM, routeGroupTypeOsm, routeGroupIdOsm, routeGroupVersionOSM, routeGroupChangesetOSM, routeGroupTimestampOSM, routeGroupLastUserNameOSM, routeGroupLastUserIdOSM;
                    try
                    {
                        routeGroupNameOsm = node.SelectSingleNode("tag[@k=\"name\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeGroupNameOsm = string.Empty;
                    }
                    try
                    {
                        routeGroupTypeOsm = node.SelectSingleNode("tag[@k=\"route_master\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeGroupTypeOsm = string.Empty;
                    }
                    try
                    {
                        routeGroupNameByOSM = node.SelectSingleNode("tag[@k=\"name:be\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeGroupNameByOSM = string.Empty;
                    }
                    try
                    {
                        routeGroupNameRuOSM = node.SelectSingleNode("tag[@k=\"name:ru\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeGroupNameRuOSM = string.Empty;
                    }
                    try
                    {
                        routeGroupNameEnOSM = node.SelectSingleNode("tag[@k=\"name:en\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        routeGroupNameEnOSM = string.Empty;
                    }

                    routeGroupIdOsm = node.Attributes["id"].Value;
                    routeGroupVersionOSM = node.Attributes["version"].Value;
                    routeGroupChangesetOSM = node.Attributes["changeset"].Value;
                    routeGroupTimestampOSM = node.Attributes["timestamp"].Value;
                    routeGroupLastUserNameOSM = node.Attributes["user"].Value;
                    routeGroupLastUserIdOSM = node.Attributes["uid"].Value;

                    XmlNodeList nodeList = node.SelectNodes("member[@type=\"relation\"]");

                    

                    List<string>[] RoutesGroupOsm = OsmRoute.GetGroupsArray(nodeList, osmRoutes);
                    //List<string> RoutesGroupOsm = new List<string>();


                    //foreach (XmlNode xnd in nodeList) RoutesGroupOsm.Add(xnd.Attributes["ref"].Value);



                    //Console.WriteLine(routeGroupIdOsm + " : " + routeGroupNameOsm);
                   
                    //Console.WriteLine(StationsIdsOsm[0]);
                    //Console.WriteLine();

                    foreach(List<string> lst in RoutesGroupOsm) osmGroupsTwoRoutes.Add(new OsmGroupTwoRoutes(lst, routeGroupNameOsm, routeGroupNameByOSM, routeGroupNameRuOSM, routeGroupNameEnOSM, routeGroupTypeOsm, routeGroupIdOsm, routeGroupVersionOSM, routeGroupChangesetOSM, routeGroupTimestampOSM, routeGroupLastUserNameOSM, routeGroupLastUserIdOSM));


                    //count++;

                    //rr.Add(new Crds(stationLat, stationLon));////////////////////
                    //INFO.Add(new string[] { stationLat, stationLon, stationNameOSM });
                }
            }
            */



            //Console.WriteLine(count + " elements");
            return osmGroupsTwoRoutes;
        }


        /*static void FindOwnersForRoutes(List<OsmGroupTwoRoutes> osmGroupsTwoRoutes)
        {
            int count = 0;

            foreach (XElement e in StreamRootChildDoc(OSM_Filepath, "relation"))//A:\\
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(e.ToString());
                XmlNode node = xDoc.FirstChild;

                if (node.SelectSingleNode("tag[@k=\"type\" and @v=\"network\"]") != null && node.SelectSingleNode("tag[@k=\"public_transport\" and @v=\"network\"]") != null)
                {
                    string ownerNameOsm, ownerIdOsm;
                    try
                    {
                        ownerNameOsm = node.SelectSingleNode("tag[@k=\"operator\"]").Attributes.GetNamedItem("v").Value.ToString();
                    }
                    catch
                    {
                        ownerNameOsm = string.Empty;
                    }

                    ownerIdOsm = node.Attributes["id"].Value;

                    XmlNodeList nodeList = node.SelectNodes("member[@type=\"relation\"]");
                    foreach (XmlNode xnd in nodeList) foreach (OsmGroupTwoRoutes gr in osmGroupsTwoRoutes) if (xnd.Attributes["ref"].Value == gr.routeGroupIdOsm) { gr.SetOwner(ownerNameOsm, ownerIdOsm); Console.WriteLine(gr.OwnerNameOsm+" : "+gr.routeGroupNameOsm); }
                    count++;
                }
            }
            //Console.WriteLine("\n---------------------------------\nAll "+count+" owners.\nAll "+osmGroupsTwoRoutes.Count+" routes.");
        }*/

        static void Main(string[] args)
        {
            DateTime t = new DateTime();
            t = DateTime.Now;
            //List<string[]> INFO = new List<string[]>();

            //List<Crds> rr = new List<Crds>();

            /*List<OsmStation> osmStations = ReadOsmStations();
            SaveToMySQL(osmStations);*/
            
            /*List<OsmRoute> osmRoutes = ReadOsmRoutes();
            List<OsmGroupTwoRoutes> osmFullRoutes = ReadOsmGroupsTwoRoutes(osmRoutes);
            Console.WriteLine("Total time: " + (DateTime.Now - t).ToString());
            Console.WriteLine("\n---------------------------------\nAll " + osmRoutes.Count + " routes.");
            //Console.WriteLine("\n---------------------------------\nAll " + osmFullRoutes.Count + " groups from routes.");
            //FindOwnersForRoutes(osmFullRoutes);


            SaveToMySQL(osmFullRoutes);*/

            /*StreamWriter SW = new StreamWriter(new FileStream(@"A:\routes_result.txt", FileMode.Create, FileAccess.Write));
            foreach (OsmRoute r in osmRoutes)
            {
                SW.Write("[" + r.routeNameOsm + "]: ");
                foreach (string stationCode in r.StationsIdsOsm) SW.Write(stationCode + ", ");
                SW.WriteLine();
            }
            SW.Close();*/

            /*StreamWriter SW2 = new StreamWriter(new FileStream(@"A:\routes_result2.txt", FileMode.Create, FileAccess.Write));
            foreach (OsmGroupTwoRoutes gr in osmFullRoutes)
            {
                gr.
                SW2.Write("[" + r.routeNameOsm + "]: ");
                foreach (string stationCode in r.StationsIdsOsm) SW2.Write(stationCode + ", ");
                SW2.WriteLine();
            }
            SW2.Close();*/



            //rr.Add(new Crds("25.556465", "45.454889"));
            //rr.Add(new Crds("95.556465", "-32.454889"));
            //Console.WriteLine(JsonConvert.SerializeObject(rr));


            //try
            // {
            /* IEnumerable<string> grandChildData =
             from el in StreamRootChildDoc("test.xml")//new StringReader() // D:\\Files\\Other\\Projects\\PublicTransport\\belarus-latest.osm\\
             where (el.Element("tag")!= null && el.Element("tag").Attribute("k")!=null && (string)(el.Element("tag").Attribute("k")) == "public_transport")
             select (string)el.Attribute("lat");*/

            //int count = 0;






            //Console.WriteLine(node.Value.ToString());

            /*if (e.Attribute("lat") != null && e.Element("tag") != null)
            {
                bool ok = false;
                foreach (XElement tag in e.Elements())
                {
                    if (tag.Name == "tag" && (string)(tag.Attribute("k")) == "public_transport" && (string)(tag.Attribute("v")) == "platform")
                    {
                        ok = true;
                        break;
                    }
                }
                // && (string)(e.Element("tag").Attribute("k")) == "public_transport"
                if (ok)
                {
                    Console.WriteLine("["+e.Attribute("lat")+"; "+ e.Attribute("lon") + "] "+(e));
                    count++;
                }

            }*/


            //SaveKML(INFO, "A:\\test.kml");
            //SaveJsonCrds(rr, "A:\\coords.txt");



            /*foreach (string str in grandChildData)
            {
                Console.WriteLine(str);
            }*/
            //}
            //catch { }





            // Load the document and set the root element.
            /* XmlDocument doc = new XmlDocument();
             try
             {
                 doc.Load("test.xml");//D:\\Files\\Other\\Projects\\PublicTransport\\belarus-latest.osm\\
                 //System.IO.StreamReader stream = new System.IO.StreamReader("D:\\Files\\Other\\Projects\\PublicTransport\\belarus-latest.osm\\test.xml");
                 //doc.Load(stream);
                 Console.WriteLine("Loaded.");
             }
             catch
             {
                 Console.WriteLine("Not loaded.");
             }

             XmlNode root = doc.DocumentElement;*/

            // Add the namespace.
            //XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            //nsmgr.AddNamespace("bk", "urn:newbooks-schema");

            // Select all nodes where the book price is greater than 10.00.
            //XmlNodeList nodeList = root.SelectNodes("descendant::bk:book[bk:price>10.00]", nsmgr);

            // XmlNodeList nodeList = root.SelectNodes("node[tag[@k=\"public_transport\" and @v=\"platform\"]]");// 


            /*foreach (XmlNode x in nodeList)
            {
                Console.WriteLine(x.Attributes["lat"].Value + "  " + x.Attributes["lon"].Value);
            }*/

            Console.WriteLine("Total time: "+(DateTime.Now-t).ToString());
            //Console.WriteLine(count + " elements");
            //Console.WriteLine(nodeList.Count + " elements");
            Console.ReadKey(true);
            Console.ReadKey(true);
            Console.ReadKey(true);
            Console.ReadKey(true);
            Console.ReadKey(true);

        }

        
    }
}
