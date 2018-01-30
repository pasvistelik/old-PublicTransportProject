using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace TransportClasses
{
    public static class Database
    {
        private static bool initialized = false;
        public static readonly Regex hashcodePattrern = new Regex("^[SRTV][0-9A-F]{1,15}$");
        
            
        
        //private static List<VehiclesProvider> vehiclesProviders = null;
        public static bool TryInitialize()
        {
            if (!initialized)
            {
                //DateTime startTime = DateTime.Now;
                try
                {


                    Database.Connect();
                    var s = Database.GetAllStations(Database.conn);
                    var r = Database.GetAllRoutes(Database.conn);
                    var t = Database.GetAllTimetablesDictinary(Database.conn);

                    List<Station> tmpUsedStations = new List<Station>();
                    foreach (Route rr in r)
                    {
                        if (rr.stationsJSON != null)
                        {
                            try
                            {
                                if (rr.stationsJSON[rr.stationsJSON.Length - 1] != ']') continue;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                                List<string>[] codes = JsonConvert.DeserializeObject<List<string>[]>(rr.stationsJSON);
                                List<Station> tmp1 = new List<Station>();
                                List<Station> tmp2 = new List<Station>();

                                List<Timetable> tab1 = new List<Timetable>();
                                List<Timetable> tab2 = new List<Timetable>();



                                foreach (string ss in codes[0])
                                {
                                    bool tmpUsed = false;
                                    foreach (Station sss in tmpUsedStations)
                                    {
                                        if (sss != null && sss.hashcode == ss)
                                        {
                                            if (sss.routes == null) sss.routes = new List<Route>();
                                            if (!sss.routes.Contains(rr)) sss.routes.Add(rr);
                                            tmp1.Add(sss);

                                            Timetable tmpTab = /*GetTimetable(sss.hashcode, rr.hashcode, canReadDataFromLocalCopy: true); */null;
                                            if (!t.TryGetValue(sss.hashcode + rr.hashcode, out tmpTab))
                                            {
                                            }
                                            tab1.Add(tmpTab);
                                            tmpUsed = true;
                                            break;
                                        }
                                    }

                                    if (!tmpUsed) foreach (Station sss in s)
                                        {
                                            if (sss != null && sss.hashcode == ss)
                                            {
                                                if (sss.routes == null) sss.routes = new List<Route>();
                                                if (!sss.routes.Contains(rr)) sss.routes.Add(rr);
                                                tmp1.Add(sss);

                                                Timetable tmpTab = /*GetTimetable(sss.hashcode, rr.hashcode, canReadDataFromLocalCopy: true); */null;
                                                if (!t.TryGetValue(sss.hashcode + rr.hashcode, out tmpTab))
                                                {
                                                }
                                                tab1.Add(tmpTab);
                                                if (!tmpUsedStations.Contains(sss)) tmpUsedStations.Add(sss);
                                                break;
                                            }
                                        }
                                }
                                foreach (string ss in codes[1])
                                {
                                    bool tmpUsed = false;
                                    foreach (Station sss in tmpUsedStations)
                                    {
                                        if (sss != null && sss.hashcode == ss)
                                        {
                                            if (sss.routes == null) sss.routes = new List<Route>();
                                            if (!sss.routes.Contains(rr)) sss.routes.Add(rr);
                                            tmp2.Add(sss);

                                            Timetable tmpTab = /*GetTimetable(sss.hashcode, rr.hashcode, canReadDataFromLocalCopy: true); */null;
                                            if (!t.TryGetValue(sss.hashcode + rr.hashcode, out tmpTab))
                                            {
                                            }
                                            tab2.Add(tmpTab);
                                            tmpUsed = true;
                                            break;
                                        }
                                    }

                                    if (!tmpUsed) foreach (Station sss in s)
                                        {
                                            if (sss != null && sss.hashcode == ss)
                                            {
                                                if (sss.routes == null) sss.routes = new List<Route>();
                                                if (!sss.routes.Contains(rr)) sss.routes.Add(rr);
                                                tmp2.Add(sss);

                                                Timetable tmpTab = /*GetTimetable(sss.hashcode, rr.hashcode, canReadDataFromLocalCopy: true); */null;
                                                if (!t.TryGetValue(sss.hashcode + rr.hashcode, out tmpTab))
                                                {
                                                }
                                                tab2.Add(tmpTab);
                                                if (!tmpUsedStations.Contains(sss)) tmpUsedStations.Add(sss);
                                                break;
                                            }
                                        }
                                }

                                rr.stations = new List<Station>[] { tmp1, tmp2 };
                                rr.timetables = new List<Timetable>[] { tab1, tab2 };
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }
                    }
                    /*foreach (Station ss in s)
                    {
                        if (ss.routes == null) ss.routes = new List<Route>();
                        foreach (Route rr in r)
                        {
                            if (stationsJSON != null)
                            {
                                List<string>[] tmp = JsonConvert.DeserializeObject<List<string>[]>(stationsJSON);
                                stations = Station.FromList(tmp);
                            }
                        }
                    }*/


                    //GlobalVehiclesProvider.Load(new UmniyTransportRfVehiclesProvider());

                    allStationsList = s;
                    allRoutesList = r;

                    Database.Disconnect();


                    initialized = true;

                    
                }
                catch (Exception ex)
                {
                    return false;
                }
                //finally
                //{
                //    TimeSpan tmp = (DateTime.Now - startTime);
                //    MessageBox.Show(tmp.TotalSeconds.ToString() + "   " + tmp.TotalMilliseconds.ToString());
                //}
            }
            return true;
        }

        private static List<Station> allStationsList = null;
        private static List<Route> allRoutesList = null;
        private static SortedDictionary<string, Timetable> timetablesDictionary = new SortedDictionary<string, Timetable>();
        private static SortedDictionary<string, Station> stationsDictionary = new SortedDictionary<string, Station>();
        private static SortedDictionary<string, Route> routesDictionary = new SortedDictionary<string, Route>();
        private static SortedDictionary<string, List<string>> routesCodesOnStationsDictionary = new SortedDictionary<string, List<string>>();
        private static SortedDictionary<string, List<string>[]> stationsCodesOnRouteDictionary = new SortedDictionary<string, List<string>[]>();

        public static MySqlConnection conn = null;

        private static string serverName = "localhost"; // Адрес сервера (для локальной базы пишите "localhost")
        private static string userName = "a"; // Имя пользователя
        private static string dbName = "transport"; //Имя базы данных
        private static string port = "3306"; // Порт для подключения
        private static string password = "a"; // Пароль для подключения*/
        

            



        private static string connStr = "server=" + serverName +
            ";user=" + userName +
            ";database=" + dbName +
            ";port=" + port +
            ";password=" + password + ";";

        public static void Connect()
        {
            if (conn == null)
            {
                conn = new MySqlConnection(connStr);//MySqlConnection 
                conn.Open();
            }
            if(conn.State == ConnectionState.Broken || conn.State == ConnectionState.Closed) conn.Open();
            
        }
        public static void Disconnect()
        {
            //conn.Close();
        }
        public static List<Station> GetAllStations(MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            if (canReadDataFromLocalCopy && allStationsList != null)
                return allStationsList;

            string read_books = "SELECT * FROM stations ORDER BY hashcode";// name";//_rus
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlCommand sqlCom = new MySqlCommand(read_books, callingConn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();
            object[] myDataCurrentItemArray = null;
            //List<Station> tmp
            allStationsList = new List<Station>();
            for (int i = 0; i < myData.Length; i++)
            {
                // Do something...
                /*try
                {
                    MessageBox.Show((string)myData[i].ItemArray[0] + ", " + (string)myData[i].ItemArray[1] + ", " + (string)myData[i].ItemArray[2] + ", " + (string)myData[i].ItemArray[3] + ", " + myData[i].ItemArray[4].ToString() + ", " + myData[i].ItemArray[5].ToString() + ", " + (string)myData[i].ItemArray[6]);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }*/
                myDataCurrentItemArray = myData[i].ItemArray;
                allStationsList.Add(new Station((string)myDataCurrentItemArray[0], (string)myDataCurrentItemArray[1], (string)myDataCurrentItemArray[2], (string)myDataCurrentItemArray[3], (int)myDataCurrentItemArray[4], (int)myDataCurrentItemArray[5], null/*(string)myData[i].ItemArray[6]*/, (string)myDataCurrentItemArray[7], (string)myDataCurrentItemArray[8], (string)myDataCurrentItemArray[9], (string)myDataCurrentItemArray[10], (string)myDataCurrentItemArray[11], (string)myDataCurrentItemArray[12], (string)myDataCurrentItemArray[13]));
            }

            if (needClose) callingConn.Close();
            return allStationsList;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static List<Station> GetStationsAround(OptimalRoute.GeoCoords coords, int radius, MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            List<Station> tmp = null;
            if (canReadDataFromLocalCopy)
            {
                tmp = GetAllStations(canReadDataFromLocalCopy: true);
            }
            else
            { 
                bool needClose = false;
                if (callingConn == null)
                {
                    callingConn = (MySqlConnection)(conn.Clone());
                    callingConn.Open();
                    needClose = true;
                }
                //Math.Atan2(Math.Sqrt(Math.Pow(Math.Cos(b.xCoord * Math.PI / 180) * Math.Sin((b.yCoord - a.yCoord) * Math.PI / 180), 2) + Math.Pow(Math.Cos(a.xCoord * Math.PI / 180) * Math.Sin(b.xCoord * Math.PI / 180) - Math.Sin(a.xCoord * Math.PI / 180) * Math.Cos(b.xCoord * Math.PI / 180) * Math.Cos((b.yCoord - a.yCoord) * Math.PI / 180), 2)), Math.Sin(a.xCoord * Math.PI / 180) * Math.Sin(b.xCoord * Math.PI / 180) + Math.Cos(a.xCoord * Math.PI / 180) * Math.Cos(b.xCoord * Math.PI / 180) * Math.Cos((b.yCoord - a.yCoord) * Math.PI / 180)) * 6372795

                //double pi180 = Math.PI / 180;

                //double xp = coords.xCoord * pi180;

                //string sss = "(ATAN2(SQRT(POW(" + (Math.Cos(xp)).ToString().Replace(',', '.') + " * SIN((" + (coords.yCoord).ToString().Replace(',', '.') + " - 0.0001 * y_coord) * " + pi180.ToString().Replace(',', '.') + "), 2) + POW(COS(x_coord * " + (0.0001 * pi180).ToString().Replace(',', '.') + ") * " + (Math.Sin(xp)).ToString().Replace(',', '.') + " - SIN(x_coord * " + (0.0001 * pi180).ToString().Replace(',', '.') + ") * " + (Math.Cos(xp)).ToString().Replace(',', '.') + " * COS((" + (coords.yCoord).ToString().Replace(',', '.') + " - 0.0001 * y_coord) * " + pi180.ToString().Replace(',', '.') + "), 2)), COS(x_coord * " + (0.0001 * pi180).ToString().Replace(',', '.') + ") * " + (Math.Sin(xp)).ToString().Replace(',', '.') + " + COS(x_coord * " + (0.0001 * pi180).ToString().Replace(',', '.') + ") * " + (Math.Cos(xp)).ToString().Replace(',', '.') + " * COS((" + (coords.yCoord).ToString().Replace(',', '.') + " - 0.0001 * y_coord) * " + pi180.ToString().Replace(',', '.') + ")) * 6372795)  < " + (radius * 1.2).ToString();


                //sss = "(ATAN2(SQRT(POW(COS("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) * SIN(("+coords.yCoord.ToString().Replace(',', '.') + " - y_coord) * PI() / 180), 2) + POW(COS(x_coord * PI() / 180) * SIN("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) - SIN(x_coord * PI() / 180) * COS("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) * COS(("+coords.yCoord.ToString().Replace(',', '.') + " - y_coord) * PI() / 180), 2)), SIN(x_coord * PI() / 180) * SIN("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) + COS(x_coord * PI() / 180) * COS("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) * COS(("+coords.yCoord.ToString().Replace(',', '.') + " - y_coord) * PI() / 180)) * 6372795) < " + radius.ToString();



                //MessageBox.Show(sss);

                //string read_books = "SELECT * FROM `stations`";// WHERE "+sss;
                /*MySqlCommand sqlCom = new MySqlCommand(read_books, conn);
                sqlCom.ExecuteNonQuery();
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
                DataTable dt = new DataTable();
                dataAdapter.Fill(dt);
                var myData = dt.Select();

                List<Station> tmp = new List<Station>();
                for (int i = 0; i < myData.Length; i++)
                {
                    tmp.Add(new Station((string)myData[i].ItemArray[0], (string)myData[i].ItemArray[1], (string)myData[i].ItemArray[2], (string)myData[i].ItemArray[3], (int)myData[i].ItemArray[4], (int)myData[i].ItemArray[5], (string)myData[i].ItemArray[6]));
                }*/
                tmp = GetAllStations(callingConn, canReadDataFromLocalCopy: false);
                if (needClose) callingConn.Close();
            }
            
            List<Station> result = new List<Station>();
            foreach (Station s in tmp) if (s!=null && OptimalRoute.GeoCoords.Distance(s.Coords, coords) < radius) result.Add(s);
            
            return result;

            //return tmp;
        }

        public static List<Station> GetStationsAround2(OptimalRoute.GeoCoords coords, int count, MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }

            List<Station> tmp = GetAllStations(callingConn, canReadDataFromLocalCopy: canReadDataFromLocalCopy);
            List<Station> result = new List<Station>();

            throw new NotImplementedException();



            //foreach (Station s in tmp) if (s != null && OptimalRoute.GeoCoords.Distance(new OptimalRoute.GeoCoords(s.xCoord, s.yCoord), coords) < radius) result.Add(s);

            if (needClose) callingConn.Close();
            return result;
        }

        public static List<Station> GetStationsInRectangle(OptimalRoute.GeoCoords leftTopCoords, OptimalRoute.GeoCoords rightBottomCoords, MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }

            List<Station> tmp = GetAllStations(callingConn, canReadDataFromLocalCopy: canReadDataFromLocalCopy);
            List<Station> result = new List<Station>();

            double x0 = leftTopCoords.lat;
            double y0 = leftTopCoords.lng;
            double x1 = rightBottomCoords.lat;
            double y1 = rightBottomCoords.lng;

            foreach (Station s in tmp)
            {
                double x = s.lat;
                double y = s.lng;
                if (x >= x0 && x <= x1 && y >= y0 && y <= y1) result.Add(s);
            }

            if (needClose) callingConn.Close();
            return result;
        }

        //


        public static Route GetRouteByHashcode(string code, MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            try
            {
                Route tmpRoute = null;
                bool ok = true;
                if (routesDictionary.TryGetValue(code, out tmpRoute))
                {
                    ok = false;
                    if (canReadDataFromLocalCopy) return tmpRoute;
                }

                if (!hashcodePattrern.IsMatch(code)) return null; // Anti-SQL injection
                string read = "SELECT * FROM routes WHERE hashcode='" + code + "' LIMIT 1";
                bool needClose = false;
                if (callingConn == null)
                {
                    callingConn = (MySqlConnection)(conn.Clone());
                    callingConn.OpenAsync();
                    needClose = true;
                }
                MySqlCommand sqlCom = new MySqlCommand(read, callingConn);
                sqlCom.ExecuteNonQuery();
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
                DataTable dt = new DataTable();
                dataAdapter.Fill(dt);
                var myData = dt.Select();

                //foreach (object a in myData[0].ItemArray) MessageBox.Show(a.ToString());
                if (needClose) callingConn.CloseAsync();

                if (myData != null && myData.Length == 0) return null;

                tmpRoute = new Route((string)myData[0].ItemArray[0], (string)myData[0].ItemArray[1], (string)myData[0].ItemArray[2], (string)myData[0].ItemArray[3], (string)myData[0].ItemArray[4], /*null*/(string)myData[0].ItemArray[5], ((int)myData[0].ItemArray[6]).ToString(), (string)myData[0].ItemArray[7], (string)myData[0].ItemArray[8], (string)myData[0].ItemArray[9], (string)myData[0].ItemArray[10], (string)myData[0].ItemArray[11], (string)myData[0].ItemArray[12], (string)myData[0].ItemArray[13], (string)myData[0].ItemArray[14], (string)myData[0].ItemArray[15]);
                if (ok) routesDictionary.Add(code, tmpRoute);
                return tmpRoute;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("");
                return null;
            }
        }
        public static Route GetRouteByOsmCode(int code, MySqlConnection callingConn = null)
        {
            string read = "SELECT * FROM routes WHERE osm_id LIKE '%" + code + "%' LIMIT 1";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlCommand sqlCom = new MySqlCommand(read, callingConn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            if (needClose) callingConn.Close();

            if (myData!=null && myData.Length == 0) return null;

            return new Route((string)myData[0].ItemArray[0], (string)myData[0].ItemArray[1], (string)myData[0].ItemArray[2], (string)myData[0].ItemArray[3], (string)myData[0].ItemArray[4], /*null*/(string)myData[0].ItemArray[5], ((int)myData[0].ItemArray[6]).ToString(), (string)myData[0].ItemArray[7], (string)myData[0].ItemArray[8], (string)myData[0].ItemArray[9], (string)myData[0].ItemArray[10], (string)myData[0].ItemArray[11], (string)myData[0].ItemArray[12], (string)myData[0].ItemArray[13], (string)myData[0].ItemArray[14], (string)myData[0].ItemArray[15]);
        }







        public static List<Route> GetRoutesOnStation(string hashcode, MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            try
            {
                bool needClose = false;
                if (callingConn == null)
                {
                    callingConn = (MySqlConnection)(conn.Clone());
                    callingConn.Open();
                    needClose = true;
                }
                List<Route> result = new List<Route>();
                List<string> tmpRoutesCodes = GetRoutesCodesOnStation(hashcode, callingConn, canReadDataFromLocalCopy);

                if (tmpRoutesCodes != null)
                {
                    foreach (string routeCode in tmpRoutesCodes)
                    {
                        Route tmpRoute = Database.GetRouteByHashcode(routeCode, callingConn, canReadDataFromLocalCopy);
                        if (tmpRoute != null)
                        {
                            tmpRoute.stations = null;
                            result.Add(tmpRoute);
                        }
                    }
                }
                if (needClose) callingConn.Close();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static List<string> GetRoutesCodesOnStation(string stationCode, MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            List<string> tmpRoutesCodesOnStation = null;
            bool ok = true;
            if (routesCodesOnStationsDictionary.TryGetValue(stationCode, out tmpRoutesCodesOnStation))
            {
                ok = false;
                if (canReadDataFromLocalCopy) return tmpRoutesCodesOnStation;
            }

            if (!hashcodePattrern.IsMatch(stationCode)) return null; // Anti-SQL injection
            string read = "SELECT routes FROM stations WHERE hashcode='" + stationCode + "' LIMIT 1";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlCommand sqlCom = new MySqlCommand(read, callingConn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            if (needClose) callingConn.Close();

            if (myData != null && myData.Length == 0) return null;

            string routesJSON = (string)myData[0].ItemArray[0];
            tmpRoutesCodesOnStation = JsonConvert.DeserializeObject<List<string>>(routesJSON);

            if(ok) routesCodesOnStationsDictionary.Add(stationCode, tmpRoutesCodesOnStation);
            return tmpRoutesCodesOnStation;
        }


        public static List<Station>[] GetStationsOnRoute(string hashcode, MySqlConnection callingConn = null)
        {
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            List<Station>[] result = new List<Station>[2];
            List<string>[] tmpStationsCodes = GetStationsCodesOnRoute(hashcode, callingConn);

            for (int i = 0; i <= 1; i++)
            {
                result[i] = new List<Station>();
                foreach (string stationCode in tmpStationsCodes[i])
                {
                    Station tmpStation = GetStationByHashcode(stationCode, callingConn);
                    tmpStation.routes = null;
                    result[i].Add(tmpStation);
                }
            }
            if (needClose) callingConn.Close();
            return result;
        }
        public static List<string>[] GetStationsCodesOnRoute(string routeCode, MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            List<string>[] tmpStationsCodesOnRoute = null;
            bool ok = true;
            if (stationsCodesOnRouteDictionary.TryGetValue(routeCode, out tmpStationsCodesOnRoute))
            {
                ok = false;
                if (canReadDataFromLocalCopy) return tmpStationsCodesOnRoute;
            }

            if (!hashcodePattrern.IsMatch(routeCode)) return null; // Anti-SQL injection
            string read = "SELECT stations FROM routes WHERE hashcode='" + routeCode + "' LIMIT 1";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlCommand sqlCom = new MySqlCommand(read, callingConn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            if (needClose) callingConn.Close();

            if (myData != null && myData.Length == 0) return null;

            string stationsJSON = (string)myData[0].ItemArray[0];
            tmpStationsCodesOnRoute = JsonConvert.DeserializeObject<List<string>[]>(stationsJSON);
            
            if(ok) stationsCodesOnRouteDictionary.Add(routeCode, tmpStationsCodesOnRoute);
            return tmpStationsCodesOnRoute;
        }


        public static List<Route> GetAllRoutes(MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            if (canReadDataFromLocalCopy && allRoutesList != null) return allRoutesList;

            string read = "SELECT * FROM routes ORDER BY type, number, osm_num";//
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlCommand sqlCom = new MySqlCommand(read, callingConn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            allRoutesList = new List<Route>();
            object[] myDataCurrentItemArray = null;
            for (int i = 0; i < myData.Length; i++)
            {
                // Do something...
                /*try
                {
                    MessageBox.Show((string)myData[i].ItemArray[0] + ", " + (string)myData[i].ItemArray[1] + ", " + (string)myData[i].ItemArray[2] + ", " + (string)myData[i].ItemArray[3] + ", " + myData[i].ItemArray[4].ToString() + ", " + myData[i].ItemArray[5].ToString() + ", " + (string)myData[i].ItemArray[6]);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }*/
                myDataCurrentItemArray = myData[i].ItemArray;
                allRoutesList.Add(new Route((string)myDataCurrentItemArray[0], (string)myDataCurrentItemArray[1], (string)myDataCurrentItemArray[2], (string)myDataCurrentItemArray[3], (string)myDataCurrentItemArray[4], /*null*/(string)myDataCurrentItemArray[5], ((int)myDataCurrentItemArray[6]).ToString(), (string)myDataCurrentItemArray[7], (string)myDataCurrentItemArray[8], (string)myDataCurrentItemArray[9], (string)myDataCurrentItemArray[10], (string)myDataCurrentItemArray[11], (string)myDataCurrentItemArray[12], (string)myDataCurrentItemArray[13], (string)myDataCurrentItemArray[14], (string)myDataCurrentItemArray[15]));
            }

            if (needClose) callingConn.Close();
            return allRoutesList;
        }
        public static Station GetStationByHashcode(string code, MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            Station tmpStation = null;
            bool ok = true;
            if (stationsDictionary.TryGetValue(code, out tmpStation))
            {
                ok = false;
                if(canReadDataFromLocalCopy) return tmpStation;
            }

            if (!hashcodePattrern.IsMatch(code)) return null; // Anti-SQL injection
            string read = "SELECT * FROM stations WHERE hashcode='" + code + "' LIMIT 1";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlCommand sqlCom = new MySqlCommand(read, callingConn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            if (needClose) callingConn.Close();

            if (myData != null && myData.Length == 0) return null;

            tmpStation = new Station((string)myData[0].ItemArray[0], (string)myData[0].ItemArray[1], (string)myData[0].ItemArray[2], (string)myData[0].ItemArray[3], (int)myData[0].ItemArray[4], (int)myData[0].ItemArray[5], null, (string)myData[0].ItemArray[7], (string)myData[0].ItemArray[8], (string)myData[0].ItemArray[9], (string)myData[0].ItemArray[10], (string)myData[0].ItemArray[11], (string)myData[0].ItemArray[12], (string)myData[0].ItemArray[13]/*(string)myData[0].ItemArray[6]*/);
            if(ok) stationsDictionary.Add(code, tmpStation);
            return tmpStation;
        }

        public static Station GetStationByOsmCode(int code, MySqlConnection callingConn = null)
        {
            string read = "SELECT * FROM stations WHERE osm_id='" + code + "' LIMIT 1";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlCommand sqlCom = new MySqlCommand(read, callingConn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            if (needClose) callingConn.Close();

            if (myData != null && myData.Length == 0) return null;

            return new Station((string)myData[0].ItemArray[0], (string)myData[0].ItemArray[1], (string)myData[0].ItemArray[2], (string)myData[0].ItemArray[3], (int)myData[0].ItemArray[4], (int)myData[0].ItemArray[5], null, (string)myData[0].ItemArray[7], (string)myData[0].ItemArray[8], (string)myData[0].ItemArray[9], (string)myData[0].ItemArray[10], (string)myData[0].ItemArray[11], (string)myData[0].ItemArray[12], (string)myData[0].ItemArray[13]/*(string)myData[0].ItemArray[6]*/);
        }

        public static string CreateHash()
        {
            Thread.Sleep(10);
            return ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString("X");
        }
        public static Station CreateStation(MySqlConnection callingConn = null)
        {
            string code = "S" + CreateHash();

            string sql = "INSERT INTO `"+ dbName + "`.`stations` (`hashcode`) VALUES ('" + code + "');";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();

            if (needClose) callingConn.Close();
            return GetStationByHashcode(code);
        }
        public static void UpdateStation(string hashcode, string nameRus, string nameBy, string nameEn, double xCoord, double yCoord, /*string strCodes,*/ MySqlConnection callingConn = null)
        {
            int x = (int)(xCoord * 10000), y = (int)(yCoord * 10000);

            string sql = "UPDATE  `" + dbName + "`.`stations` SET  `name_rus` = '" + nameRus + "',`name_by` = '" + nameBy + "',`name_en` = '" + nameEn + "',`x_coord` = '" + x + "',`y_coord` = '" + y + /*"',`routes` = '" + strCodes +*/ "' WHERE  `stations`.`hashcode` = '" + hashcode + "';";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();
            if (needClose) callingConn.Close();
        }
        public static void DeleteStation(Station s, MySqlConnection callingConn = null)
        {
            string sql = "DELETE FROM `" + dbName + "`.`stations` WHERE `stations`.`hashcode` = '" + s.hashcode + "' LIMIT 1";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();

            // удалить из маршрутов...



            // удалить из расписания...
            string sql2 = "DELETE FROM `" + dbName + "`.`timetable` WHERE `timetable`.`station` = '" + s.hashcode + "'";
            MySqlScript script2 = new MySqlScript(callingConn, sql2);
            script2.Execute();
            if (needClose) callingConn.Close();
        }



        public static Route CreateRoute(MySqlConnection callingConn = null)
        {
            string code = "R" + CreateHash();

            string sql = "INSERT INTO `" + dbName + "`.`routes` (`hashcode`) VALUES ('" + code + "');";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();

            if (needClose) callingConn.Close();
            return GetRouteByHashcode(code);
        }
        public static void UpdateRoute(string hashcode, string rus, string en, string by, string stationsJSON, string number, string type, MySqlConnection callingConn = null)
        {
            // отредактировать остановки...
            List<string>[] oldStationsCodes = GetStationsCodesOnRoute(hashcode);

            List<string> ttt = new List<string>();
            for (int i = 0; i <= 1; i++) foreach (string s in oldStationsCodes[i]) if (!(ttt.Contains(s))) ttt.Add(s);
            foreach (string code in ttt)
            {
                List<string> routesCodes = GetRoutesCodesOnStation(code);
                if (!(routesCodes.Contains(hashcode)))
                {
                    routesCodes.Add(hashcode);

                    string strRouteCodes = JsonConvert.SerializeObject(routesCodes);

                    string new_sql = "UPDATE  `" + dbName + "`.`stations` SET `routes` = '" + strRouteCodes + "' WHERE  `stations`.`hashcode` = '" + code + "';";
                    bool needClose1 = false;
                    if (callingConn == null)
                    {
                        callingConn = (MySqlConnection)(conn.Clone());
                        callingConn.Open();
                        needClose1 = true;
                    }
                    MySqlScript script1 = new MySqlScript(callingConn, new_sql);
                    script1.Execute();
                    if (needClose1) callingConn.Close();
                    
                }
            }




            List<string>[] newStationsCodes = JsonConvert.DeserializeObject<List<string>[]>(stationsJSON);

            List<string> tmp = new List<string>();
            for (int i = 0; i <= 1; i++) foreach (string s in oldStationsCodes[i]) if (!(tmp.Contains(s)) && !(newStationsCodes[0].Contains(s)) && !(newStationsCodes[1].Contains(s))) tmp.Add(s);
            //MessageBox.Show("removed "+tmp.Count+ " elements " + oldStationsCodes[0].Count + " " + newStationsCodes[0].Count + " " + oldStationsCodes[1].Count + " " + newStationsCodes[1].Count + " ");
            foreach (string s in tmp) RemoveRouteFromStation(s, hashcode, callingConn);

            tmp = new List<string>();
            for (int i = 0; i <= 1; i++) foreach (string s in newStationsCodes[i]) if (!(tmp.Contains(s)) && !(oldStationsCodes[0].Contains(s)) && !(oldStationsCodes[1].Contains(s))) tmp.Add(s);
            //MessageBox.Show("added " + tmp.Count + " elements");
            foreach (string s in tmp) AddRouteToStation(s, hashcode, callingConn);



            //////

            string sql = "UPDATE  `" + dbName + "`.`routes` SET  `name_rus` = '" + rus + "',`name_by` = '" + by + "',`name_en` = '" + en + "',`stations` = '" + stationsJSON + "',`number` = '" + int.Parse(number) + "',`type` = '" + type + "' WHERE  `routes`.`hashcode` = '" + hashcode + "';";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();
            if (needClose) callingConn.Close();

            /*
            Route tmpRoute = null;
            if (routesDictionary.TryGetValue(hashcode, out tmpRoute)) routesDictionary[hashcode] = ;*/
        }
        public static void RemoveRouteFromStation(string stationCode, string routeCode, MySqlConnection callingConn = null)
        {
            List<string> tmp = GetRoutesCodesOnStation(stationCode);
            tmp.Remove(routeCode);
            string strCodes = JsonConvert.SerializeObject(tmp);

            string sql = "UPDATE  `" + dbName + "`.`stations` SET  `routes` = '" + strCodes + "' WHERE  `stations`.`hashcode` = '" + stationCode + "';";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();

            //MessageBox.Show("Remove " + routeCode + " from " + stationCode);
            if (needClose) callingConn.Close();
        }
        public static void AddRouteToStation(string stationCode, string routeCode, MySqlConnection callingConn = null)
        {
            List<string> tmp = GetRoutesCodesOnStation(stationCode, callingConn);
            tmp.Add(routeCode);
            string strCodes = JsonConvert.SerializeObject(tmp);

            string sql = "UPDATE  `" + dbName + "`.`stations` SET  `routes` = '" + strCodes + "' WHERE  `stations`.`hashcode` = '" + stationCode + "';";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();

            //MessageBox.Show("Add "+routeCode+" to " +stationCode);
            if (needClose) callingConn.Close();
        }
        public static void DeleteRoute(Route s, MySqlConnection callingConn = null)
        {
            string sql = "DELETE FROM `" + dbName + "`.`routes` WHERE `routes`.`hashcode` = '" + s.hashcode + "' LIMIT 1";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();

            // удалить из остановок...



            // удалить из расписания...
            string sql2 = "DELETE FROM `" + dbName + "`.`timetable` WHERE `timetable`.`route` = '" + s.hashcode + "'";
            MySqlScript script2 = new MySqlScript(callingConn, sql2);
            script2.Execute();
            if (needClose) callingConn.Close();
        }






        public static void SetFullTimetableForRoute(string filePath, string routeCode, MySqlConnection callingConn = null)
        {
            List<string>[] stationsCodes = GetStationsCodesOnRoute(routeCode, callingConn);
            List<Timetable>[] fullTimetable;
            try
            {
                fullTimetable = Timetable.DeserializeFullTable(File.ReadAllText(filePath));
            }
            catch
            {
                throw new Exception("Не удалось распознать расписание.");
            }
            if (stationsCodes[0].Count != fullTimetable[0].Count || stationsCodes[1].Count != fullTimetable[1].Count) throw new Exception("Количество остановок не совпадает: должно быть "+ fullTimetable[0].Count + " и "+ fullTimetable[1].Count);
            for (int i = 0; i <= 1; i++)
            {
                for (int j = 0; j < stationsCodes[i].Count; j++)
                {
                    try
                    {
                        GetTimetable(stationsCodes[i][j], routeCode, callingConn);
                    }
                    catch
                    {
                        throw new Exception("Ошибка во время проверки существования записи о расписании.");
                    }
                    try
                    {
                        UpdateTimetable(stationsCodes[i][j], routeCode, fullTimetable[i][j], callingConn);
                        //Timetable tmpResult = null;
                        //if (timetablesDictionary.TryGetValue(stationsCodes[i][j] + routeCode, out tmpResult)) timetablesDictionary[stationsCodes[i][j] + routeCode] = fullTimetable[i][j];
                    }
                    catch
                    {
                        throw new Exception("Ошибка во время записи расписания.");
                    }
                }
            }
        }
        public static void SetTimetable(string filePath, string stationCode, string routeCode, MySqlConnection callingConn = null)
        {
            try
            {
                string jsonObject = File.ReadAllText(filePath);

                Timetable tmp = Timetable.Deserialize(jsonObject);
                //MessageBox.Show(tmp.table[0].times[0].ToString());

                //Timetable tmpTimetable = Database.GetTimetable(stationCode, routeCode);
                //string TEST = .SerializeObject(tmpTimetable);
                //MessageBox.Show(TEST);


                //string code = "T" + CreateHash();


                //object tmpObj = JsonConvert.DeserializeObject(jsonObject);
                //Timetable tmp = (Timetable)tmpObj;
                //Timetable tmp = JsonConvert.DeserializeObject<Timetable>(jsonObject);
                //MessageBox.Show(tmp.type.ToString());
                //            UPDATE `"+ dbName + "`.`timetable` SET `table` = '[]',`type` = '0',`spetial` = '[]' WHERE station='S157A8E1A8A2' AND route='R158542FE936' LIMIT 1
                string sql = "UPDATE `" + dbName + "`.`timetable` SET `table` = '" + JsonConvert.SerializeObject(tmp.table) + "',`spetial` = '" + JsonConvert.SerializeObject(tmp.spetial) + "',`type` = '" + JsonConvert.SerializeObject(tmp.type) + "' WHERE station = '" + stationCode + "' AND route = '" + routeCode + "' LIMIT 1";
                //MessageBox.Show(sql);
                bool needClose = false;
                if (callingConn == null)
                {
                    callingConn = (MySqlConnection)(conn.Clone());
                    callingConn.Open();
                    needClose = true;
                }
                MySqlScript script = new MySqlScript(callingConn, sql);
                script.Execute();
                if (needClose) callingConn.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }
        public static Timetable CreateTimetable(string StationCode, string RouteCode, MySqlConnection callingConn = null)
        {
            string code = "T" + CreateHash();

            //Timetable tmp = Timetable.CreateStandart();
            string type = "1";//JsonConvert.SerializeObject(tmp.type);
            string emptyTable = "[{\"days\":[1,2,3,4,5],\"times\":[]},{\"days\":[6,0],\"times\":[]}]";//JsonConvert.SerializeObject(tmp.table);
            string emptySpetial = "[]";//JsonConvert.SerializeObject(tmp.spetial);

            string sql = "INSERT INTO `" + dbName + "`.`timetable` (`hashcode`,`station`,`route`,`table`,`spetial`,`type`) VALUES ('" + code + "','" + StationCode + "','" + RouteCode + "','" + emptyTable + "','" + emptySpetial + "','" + type + "');";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();

            if (needClose) callingConn.Close();
            return GetTimetable(StationCode, RouteCode);
        }
        public static void UpdateTimetable(string StationCode, string RouteCode, Timetable tmp, MySqlConnection callingConn = null)
        {
            string type = JsonConvert.SerializeObject(tmp.type);
            string myTable = JsonConvert.SerializeObject(tmp.table);
            string mySpetial = JsonConvert.SerializeObject(tmp.spetial);

            string sql = "UPDATE `" + dbName + "`.`timetable` SET `table` = '" + myTable + "',`type` = '" + type + "',`spetial` = '" + mySpetial + "' WHERE station='" + StationCode + "' AND route='" + RouteCode + "' LIMIT 1";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlScript script = new MySqlScript(callingConn, sql);
            script.Execute();
            if (needClose) callingConn.Close();
        }
        public static Timetable GetTimetable(string StationCode, string RouteCode, MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            Timetable tmpResult = null;
            bool ok = true;
            if (timetablesDictionary.TryGetValue(StationCode + RouteCode, out tmpResult))
            {
                ok = false;
                if(canReadDataFromLocalCopy) return tmpResult;
            }

            if (!hashcodePattrern.IsMatch(StationCode) || !hashcodePattrern.IsMatch(RouteCode)) return null; // Anti-SQL injection
            string read = "SELECT * FROM `" + dbName + "`.`timetable` WHERE station='" + StationCode + "' AND route='" + RouteCode + "' LIMIT 1";
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlCommand sqlCom = new MySqlCommand(read, callingConn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            if (myData.Length == 0)
            {
                try
                {
                    //MessageBox.Show("not");
                    return CreateTimetable(StationCode, RouteCode);
                }
                catch
                {
                    throw new Exception("Ошибка во время создании новой записи расписания.");
                }
            }
            /*MessageBox.Show((string)myData[0].ItemArray[3]);
            MessageBox.Show((string)myData[0].ItemArray[4]);
            MessageBox.Show((string)myData[0].ItemArray[5]);*/

            TableType type = JsonConvert.DeserializeObject<TableType>((string)myData[0].ItemArray[5]);
            ObservableCollection<Table> table = JsonConvert.DeserializeObject<ObservableCollection<Table>>((string)myData[0].ItemArray[3]);
            ObservableCollection<SpetialTableForDay> spetial = JsonConvert.DeserializeObject<ObservableCollection<SpetialTableForDay>>((string)myData[0].ItemArray[4]);


            tmpResult = new Timetable(StationCode, RouteCode, type, table, spetial);
            //Timetable tmp = JsonConvert.DeserializeObject<Timetable>((string)myData[0].ItemArray[3]);// new Timetable(new List<Table>());
            //MessageBox.Show(tmp.type.ToString());
            if (needClose) callingConn.Close();

            if(ok) timetablesDictionary.Add(StationCode + RouteCode, tmpResult);
            return tmpResult;
        }


        public static SortedDictionary<string, Timetable> GetAllTimetablesDictinary(MySqlConnection callingConn = null, bool canReadDataFromLocalCopy = false)
        {
            if (canReadDataFromLocalCopy && timetablesDictionary != null && timetablesDictionary.Count != 0) return timetablesDictionary;

            string read = "SELECT * FROM timetable ORDER BY station, route";//
            bool needClose = false;
            if (callingConn == null)
            {
                callingConn = (MySqlConnection)(conn.Clone());
                callingConn.Open();
                needClose = true;
            }
            MySqlCommand sqlCom = new MySqlCommand(read, callingConn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            object[] myDataCurrentItemArray = null;
            //allTimetablesDictinary = new List<Timetable>();
            for (int i = 0; i < myData.Length; i++)
            {
                myDataCurrentItemArray = myData[i].ItemArray;
                string stationCode = (string)myDataCurrentItemArray[1];
                string routeCode = (string)myDataCurrentItemArray[2];

                Timetable tmpResult = null;
                string dictCode = stationCode + routeCode;
                if (!timetablesDictionary.TryGetValue(dictCode, out tmpResult))
                {
                    TableType type = JsonConvert.DeserializeObject<TableType>((string)myDataCurrentItemArray[5]);
                    ObservableCollection<Table> table = JsonConvert.DeserializeObject<ObservableCollection<Table>>((string)myDataCurrentItemArray[3]);
                    ObservableCollection<SpetialTableForDay> spetial = JsonConvert.DeserializeObject<ObservableCollection<SpetialTableForDay>>((string)myDataCurrentItemArray[4]);

                    tmpResult = new Timetable(stationCode, routeCode, type, table, spetial);

                    timetablesDictionary.Add(dictCode, tmpResult);
                }
            }

            if (needClose) callingConn.Close();
            return timetablesDictionary;
        }

    }
}
