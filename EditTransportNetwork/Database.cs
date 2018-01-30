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

namespace EditTransportNetwork
{
    public static class Database
    {
        public static MySqlConnection conn;

        private static string serverName = "localhost"; // Адрес сервера (для локальной базы пишите "localhost")
        private static string userName = "a"; // Имя пользователя
        private static string dbName = "transport"; //Имя базы данных
        private static string port = "3306"; // Порт для подключения
        private static string password = "a"; // Пароль для подключения
        private static string connStr = "server=" + serverName +
            ";user=" + userName +
            ";database=" + dbName +
            ";port=" + port +
            ";password=" + password + ";";

        public static void Connect()
        {
            conn = new MySqlConnection(connStr);//MySqlConnection 
            conn.Open();
        }
        public static void Disconnect()
        {
            conn.Close();
        }
        public static List<Station> GetAllStations()
        {
            
            string read_books = "SELECT * FROM stations ORDER BY name_rus";
            MySqlCommand sqlCom = new MySqlCommand(read_books, conn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            List<Station> tmp = new List<Station>();
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
                tmp.Add(new Station((string)myData[i].ItemArray[0], (string)myData[i].ItemArray[1], (string)myData[i].ItemArray[2], (string)myData[i].ItemArray[3], (int)myData[i].ItemArray[4], (int)myData[i].ItemArray[5], (string)myData[i].ItemArray[6]));
            }
            
            return tmp;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static List<Station> GetStationsAround(OptimalRoute.GeoCoords coords, int radius)
        {
            //Math.Atan2(Math.Sqrt(Math.Pow(Math.Cos(b.xCoord * Math.PI / 180) * Math.Sin((b.yCoord - a.yCoord) * Math.PI / 180), 2) + Math.Pow(Math.Cos(a.xCoord * Math.PI / 180) * Math.Sin(b.xCoord * Math.PI / 180) - Math.Sin(a.xCoord * Math.PI / 180) * Math.Cos(b.xCoord * Math.PI / 180) * Math.Cos((b.yCoord - a.yCoord) * Math.PI / 180), 2)), Math.Sin(a.xCoord * Math.PI / 180) * Math.Sin(b.xCoord * Math.PI / 180) + Math.Cos(a.xCoord * Math.PI / 180) * Math.Cos(b.xCoord * Math.PI / 180) * Math.Cos((b.yCoord - a.yCoord) * Math.PI / 180)) * 6372795

            double pi180 = Math.PI / 180;

            double xp = coords.xCoord * pi180;

            string sss = "(ATAN2(SQRT(POW(" + (Math.Cos(xp)).ToString().Replace(',','.') + " * SIN(("+(coords.yCoord).ToString().Replace(',', '.') + " - 0.0001 * y_coord) * "+pi180.ToString().Replace(',', '.') + "), 2) + POW(COS(x_coord * "+(0.0001 * pi180).ToString().Replace(',', '.') + ") * "+(Math.Sin(xp)).ToString().Replace(',', '.') + " - SIN(x_coord * "+(0.0001 * pi180).ToString().Replace(',', '.') + ") * "+(Math.Cos(xp)).ToString().Replace(',', '.') + " * COS(("+(coords.yCoord).ToString().Replace(',', '.') + " - 0.0001 * y_coord) * "+pi180.ToString().Replace(',', '.') + "), 2)), COS(x_coord * " + (0.0001 * pi180).ToString().Replace(',', '.') + ") * " + (Math.Sin(xp)).ToString().Replace(',', '.') + " + COS(x_coord * " + (0.0001 * pi180).ToString().Replace(',', '.') + ") * " + (Math.Cos(xp)).ToString().Replace(',', '.') + " * COS((" + (coords.yCoord).ToString().Replace(',', '.') + " - 0.0001 * y_coord) * " + pi180.ToString().Replace(',', '.') + ")) * 6372795)  < " + (radius * 1.2).ToString();


            //sss = "(ATAN2(SQRT(POW(COS("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) * SIN(("+coords.yCoord.ToString().Replace(',', '.') + " - y_coord) * PI() / 180), 2) + POW(COS(x_coord * PI() / 180) * SIN("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) - SIN(x_coord * PI() / 180) * COS("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) * COS(("+coords.yCoord.ToString().Replace(',', '.') + " - y_coord) * PI() / 180), 2)), SIN(x_coord * PI() / 180) * SIN("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) + COS(x_coord * PI() / 180) * COS("+coords.xCoord.ToString().Replace(',', '.') + " * PI() / 180) * COS(("+coords.yCoord.ToString().Replace(',', '.') + " - y_coord) * PI() / 180)) * 6372795) < " + radius.ToString();



            //MessageBox.Show(sss);

            string read_books = "SELECT * FROM `stations`";// WHERE "+sss;
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

            List<Station> tmp = GetAllStations();
            List<Station> result = new List<Station>();
            foreach (Station s in tmp) if (OptimalRoute.GeoCoords.Distance(new OptimalRoute.GeoCoords(s.xCoord, s.yCoord), coords) < radius) result.Add(s);
            return result;

            return tmp;
        }

        //


        public static Route GetRouteByHashcode(string code)
        {
            string read = "SELECT * FROM routes WHERE hashcode='" + code + "' LIMIT 1";
            MySqlCommand sqlCom = new MySqlCommand(read, conn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();
            
            return new Route((string)myData[0].ItemArray[0], (string)myData[0].ItemArray[1], (string)myData[0].ItemArray[2], (string)myData[0].ItemArray[3], null/*(string)myData[0].ItemArray[4]*/, ((int)myData[0].ItemArray[5]).ToString(), (string)myData[0].ItemArray[6]);
        }


        public static List<string> GetRoutesCodesOnStation(string stationCode)
        {
            string read = "SELECT routes FROM stations WHERE hashcode='" + stationCode + "' LIMIT 1";
            MySqlCommand sqlCom = new MySqlCommand(read, conn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            string routesJSON = (string)myData[0].ItemArray[0];
            List<string> tmp = JsonConvert.DeserializeObject<List<string>>(routesJSON);

            return tmp;
        }
        public static List<string>[] GetStationsCodesOnRoute(string routeCode)
        {
            string read = "SELECT stations FROM routes WHERE hashcode='" + routeCode + "' LIMIT 1";
            MySqlCommand sqlCom = new MySqlCommand(read, conn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            string stationsJSON = (string)myData[0].ItemArray[0];
            List<string>[] tmp = JsonConvert.DeserializeObject<List<string>[]>(stationsJSON);

            return tmp;
        }


        public static List<Route> GetAllRoutes()
        {

            string read = "SELECT * FROM routes ORDER BY number";//
            MySqlCommand sqlCom = new MySqlCommand(read, conn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            List<Route> tmp = new List<Route>();
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
                tmp.Add(new Route((string)myData[i].ItemArray[0], (string)myData[i].ItemArray[1], (string)myData[i].ItemArray[2], (string)myData[i].ItemArray[3], (string)myData[i].ItemArray[4], ((int)myData[i].ItemArray[5]).ToString(), (string)myData[i].ItemArray[6]));
            }

            return tmp;
        }
        public static Station GetStationByHashcode(string code)
        {
            string read = "SELECT * FROM stations WHERE hashcode='" + code + "' LIMIT 1";
            MySqlCommand sqlCom = new MySqlCommand(read, conn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            var myData = dt.Select();

            return new Station((string)myData[0].ItemArray[0], (string)myData[0].ItemArray[1], (string)myData[0].ItemArray[2], (string)myData[0].ItemArray[3], (int)myData[0].ItemArray[4], (int)myData[0].ItemArray[5], null/*(string)myData[0].ItemArray[6]*/);
        }

        public static string CreateHash()
        {
            return ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString("X");
        }
        public static Station CreateStation()
        {
            string code = "S" + CreateHash();

            string sql = "INSERT INTO `transport`.`stations` (`hashcode`) VALUES ('"+code+"');";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();

            return GetStationByHashcode(code);
        }
        public static void UpdateStation(string hashcode, string nameRus, string nameBy, string nameEn, double xCoord, double yCoord, string strCodes)
        {
            int x = (int)(xCoord * 10000), y = (int)(yCoord * 10000);

            string sql = "UPDATE  `transport`.`stations` SET  `name_rus` = '"+nameRus+ "',`name_by` = '" + nameBy+ "',`name_en` = '" + nameEn + "',`x_coord` = '"+x+"',`y_coord` = '"+y+"',`routes` = '" + strCodes+"' WHERE  `stations`.`hashcode` = '" + hashcode+"';";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();
        }
        public static void DeleteStation(Station s)
        {
            string sql = "DELETE FROM `transport`.`stations` WHERE `stations`.`hashcode` = '" + s.hashcode + "' LIMIT 1";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();

            // удалить из маршрутов...



            // удалить из расписания...
            string sql2 = "DELETE FROM `transport`.`timetable` WHERE `timetable`.`station` = '" + s.hashcode + "'";
            MySqlScript script2 = new MySqlScript(conn, sql2);
            script2.Execute();
        }
        


        public static Route CreateRoute()
        {
            string code = "R" + CreateHash();

            string sql = "INSERT INTO `transport`.`routes` (`hashcode`) VALUES ('" + code + "');";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();

            return GetRouteByHashcode(code);
        }
        public static void UpdateRoute(string hashcode, string rus, string en, string by, string stationsJSON, string number, string type)
        {
            // отредактировать остановки...
            List<string>[] oldStationsCodes = GetStationsCodesOnRoute(hashcode);
            List<string>[] newStationsCodes = JsonConvert.DeserializeObject<List<string>[]>(stationsJSON);

            List<string> tmp = new List<string>();
            for (int i = 0; i <= 1; i++) foreach (string s in oldStationsCodes[i]) if (!(tmp.Contains(s)) && !(newStationsCodes[0].Contains(s)) && !(newStationsCodes[1].Contains(s))) tmp.Add(s);
            //MessageBox.Show("removed "+tmp.Count+ " elements " + oldStationsCodes[0].Count + " " + newStationsCodes[0].Count + " " + oldStationsCodes[1].Count + " " + newStationsCodes[1].Count + " ");
            foreach (string s in tmp) RemoveRouteFromStation(s, hashcode);

            tmp = new List<string>();
            for (int i = 0; i <= 1; i++) foreach (string s in newStationsCodes[i]) if (!(tmp.Contains(s)) && !(oldStationsCodes[0].Contains(s)) && !(oldStationsCodes[1].Contains(s))) tmp.Add(s);
            //MessageBox.Show("added " + tmp.Count + " elements");
            foreach (string s in tmp) AddRouteToStation(s, hashcode);



            //////

            string sql = "UPDATE  `transport`.`routes` SET  `name_rus` = '" + rus + "',`name_by` = '" + by + "',`name_en` = '" + en + "',`stations` = '" + stationsJSON + "',`number` = '" + int.Parse(number) + "',`type` = '" + type + "' WHERE  `routes`.`hashcode` = '" + hashcode + "';";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();
        }
        public static void RemoveRouteFromStation(string stationCode, string routeCode)
        {
            List<string> tmp = GetRoutesCodesOnStation(stationCode);
            tmp.Remove(routeCode);
            string strCodes = JsonConvert.SerializeObject(tmp);

            string sql = "UPDATE  `transport`.`stations` SET  `routes` = '" + strCodes + "' WHERE  `stations`.`hashcode` = '" + stationCode + "';";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();

            //MessageBox.Show("Remove " + routeCode + " from " + stationCode);
        }
        public static void AddRouteToStation(string stationCode, string routeCode)
        {
            List<string> tmp = GetRoutesCodesOnStation(stationCode);
            tmp.Add(routeCode);
            string strCodes = JsonConvert.SerializeObject(tmp);

            string sql = "UPDATE  `transport`.`stations` SET  `routes` = '" + strCodes + "' WHERE  `stations`.`hashcode` = '" + stationCode + "';";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();

            //MessageBox.Show("Add "+routeCode+" to " +stationCode);
        }
        public static void DeleteRoute(Route s)
        {
            string sql = "DELETE FROM `transport`.`routes` WHERE `routes`.`hashcode` = '" + s.hashcode + "' LIMIT 1";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();

            // удалить из остановок...



            // удалить из расписания...
            string sql2 = "DELETE FROM `transport`.`timetable` WHERE `timetable`.`route` = '" + s.hashcode + "'";
            MySqlScript script2 = new MySqlScript(conn, sql2);
            script2.Execute();
        }




        public static Timetable CreateTimetable(string StationCode, string RouteCode)
        {
            string code = "T" + CreateHash();

            //Timetable tmp = Timetable.CreateStandart();
            string type = "1";//JsonConvert.SerializeObject(tmp.type);
            string emptyTable = "[{\"days\":[1,2,3,4,5],\"times\":[]},{\"days\":[6,0],\"times\":[]}]";//JsonConvert.SerializeObject(tmp.table);
            string emptySpetial = "[]";//JsonConvert.SerializeObject(tmp.spetial);

            string sql = "INSERT INTO `transport`.`timetable` (`hashcode`,`station`,`route`,`table`,`spetial`,`type`) VALUES ('" + code + "','" + StationCode + "','" + RouteCode + "','" + emptyTable + "','" + emptySpetial + "','" + type + "');";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();

            return GetTimetable(StationCode, RouteCode);
        }
        public static void UpdateTimetable(string StationCode, string RouteCode, Timetable tmp)
        {
            string type = JsonConvert.SerializeObject(tmp.type);
            string myTable = JsonConvert.SerializeObject(tmp.table);
            string mySpetial = JsonConvert.SerializeObject(tmp.spetial);

            string sql = "UPDATE  `transport`.`timetable` SET  `table` = '" + myTable + "',`type` = '" + type + "',`spetial` = '" + mySpetial + "' WHERE station='" + StationCode + "' AND route='" + RouteCode + "' LIMIT 1";
            MySqlScript script = new MySqlScript(conn, sql);
            script.Execute();
        }
        public static Timetable GetTimetable(string StationCode, string RouteCode)
        {
            string read = "SELECT * FROM `transport`.`timetable` WHERE station='" + StationCode + "' AND route='" + RouteCode + "' LIMIT 1";
            MySqlCommand sqlCom = new MySqlCommand(read, conn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable(); 
            dataAdapter.Fill(dt); 
            var myData = dt.Select();

            if (myData.Length == 0)
            {
                //MessageBox.Show("not");
                return CreateTimetable(StationCode, RouteCode);
            }
            /*MessageBox.Show((string)myData[0].ItemArray[3]);
            MessageBox.Show((string)myData[0].ItemArray[4]);
            MessageBox.Show((string)myData[0].ItemArray[5]);*/

            Timetable.TableType type = JsonConvert.DeserializeObject<Timetable.TableType>((string)myData[0].ItemArray[5]);
            ObservableCollection<Timetable.Table> table = JsonConvert.DeserializeObject<ObservableCollection<Timetable.Table>>((string)myData[0].ItemArray[3]);
            ObservableCollection<Timetable.SpetialTableForDay> spetial = JsonConvert.DeserializeObject<ObservableCollection<Timetable.SpetialTableForDay>>((string)myData[0].ItemArray[4]);

            Timetable tmp = new Timetable(type, table, spetial);
            //Timetable tmp = JsonConvert.DeserializeObject<Timetable>((string)myData[0].ItemArray[3]);// new Timetable(new List<Timetable.Table>());
            //MessageBox.Show(tmp.type.ToString());
            return tmp;
        }



    }
}
