using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditTransportNetwork
{
    public class OptimalRoute
    {
        public readonly double speed;
        public readonly GeoCoords nowPos;
        public readonly GeoCoords needPos;
        public readonly DateTime time;
        public readonly string[] types;

        public List<string> points = new List<string>();

        public static string[] allTypes = new string[] { "bus", "trolleybus", "express_bus", "marsh", "tram", "metro" };



        public class GeoCoords
        {
            /// <summary>
            /// Широта.
            /// </summary>
            public readonly double xCoord;
            /// <summary>
            /// Долгота.
            /// </summary>
            public readonly double yCoord;
            public static int Distance(GeoCoords a, GeoCoords b)
            {
                {
                    int earthRadius = 6372795;

                    // перевести координаты в радианы
                    double lat1 = a.xCoord * Math.PI / 180;
                    double lat2 = b.xCoord * Math.PI / 180;
                    double long1 = a.yCoord * Math.PI / 180;
                    double long2 = b.yCoord * Math.PI / 180;

                    // косинусы и синусы широт и разницы долгот
                    double cl1 = Math.Cos(lat1);
                    double cl2 = Math.Cos(lat2);
                    double sl1 = Math.Sin(lat1);
                    double sl2 = Math.Sin(lat2);
                    double delta = long2 - long1;
                    double cdelta = Math.Cos(delta);
                    double sdelta = Math.Sin(delta);

                    // вычисления длины большого круга
                    double y = Math.Sqrt(Math.Pow(cl2 * sdelta, 2) + Math.Pow(cl1 * sl2 - sl1 * cl2 * cdelta, 2));
                    double x = sl1 * sl2 + cl1 * cl2 * cdelta;

                    //
                    double ad = Math.Atan2(y, x);
                    double dist = ad * earthRadius;

                    dist = Math.Atan2(Math.Sqrt(Math.Pow(Math.Cos(b.xCoord * Math.PI / 180) * Math.Sin((b.yCoord - a.yCoord) * Math.PI / 180), 2) + Math.Pow(Math.Cos(a.xCoord * Math.PI / 180) * Math.Sin(b.xCoord * Math.PI / 180) - Math.Sin(a.xCoord * Math.PI / 180) * Math.Cos(b.xCoord * Math.PI / 180) * Math.Cos((b.yCoord - a.yCoord) * Math.PI / 180), 2)), Math.Sin(a.xCoord * Math.PI / 180) * Math.Sin(b.xCoord * Math.PI / 180) + Math.Cos(a.xCoord * Math.PI / 180) * Math.Cos(b.xCoord * Math.PI / 180) * Math.Cos((b.yCoord - a.yCoord) * Math.PI / 180)) * 6372795;

                    return (int)Math.Round(dist, 0);
                }
            }
            /// <summary>
            /// Вычисляет расстояние в метрах до заданной остановки.
            /// </summary>
            /// <param name="station">Остановка, расстояние до которой необходимо вычислить.</param>
            /// <returns>Расстояние в метрах до остановки.</returns>
            /*public double DistanceTo(Station station)
            {
                return Distance(this, station.coords);
            }*/



            /// <summary>
            /// Задает координаты объекта: широту и долготу.
            /// </summary>
            /// <param name="xCoord">Широта в градусах (от -90 до 90).</param>
            /// <param name="yCoord">Долгота в градусах (от -180 до 180).</param>
            public GeoCoords(double xCoord, double yCoord)
            {
                if (xCoord >= -90 && xCoord <= 90 && yCoord >= -180 && yCoord <= 180)
                {
                    this.xCoord = xCoord;
                    this.yCoord = yCoord;
                }
                else throw new FormatException("Некорректные координаты.");
            }

            public static bool operator ==(GeoCoords a, GeoCoords b)
            {
                if (a.xCoord == b.xCoord && a.yCoord == b.yCoord) return true;
                return false;
            }
            public static bool operator !=(GeoCoords a, GeoCoords b)
            {
                if (a.xCoord != b.xCoord || a.yCoord != b.yCoord) return true;
                return false;
            }
            public override bool Equals(object obj)
            {
                if(xCoord == (obj as GeoCoords).xCoord && yCoord == (obj as GeoCoords).yCoord) return true;
                return false;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public class Point
        {
            private Route myRoute=null;
            public string RouteCode
            {
                get
                {
                    return ((myRoute == null) ? null : myRoute.hashcode);
                }
            }
            private Point prev=null;
            public Point Previous
            {
                get
                {
                    return prev;
                }
            }
            private GeoCoords coords;
            public GeoCoords Coords
            {
                get
                {
                    return coords;
                }
            }
            private TimeSpan totalTime;
            public TimeSpan TotalTime
            {
                get
                {
                    return totalTime;
                }
            }
            private Station fromWhere=null, station=null;
            public Station Station
            {
                get
                {
                    return station;
                }
            }
            private bool visited = false;
            public bool tryUpdate(TimeSpan totalTime, Point prev=null, Station fromWhere = null, Route myRoute=null)
            {
                if (totalTime < this.totalTime)
                {
                    this.myRoute = myRoute;
                    this.prev = prev;
                    this.totalTime = totalTime;
                    this.fromWhere = fromWhere;

                    return true;
                }
                return false;
            }
            public void Visited()
            {
                visited = true;
            }
            public bool IsVisited
            {
                get
                {
                    return visited;
                }
            }
            public Point(TimeSpan totalTime, Station station, Station fromWhere = null, Route r=null)
            {
                this.station = station;
                coords = station.Coords;
                this.totalTime = totalTime;
                this.fromWhere = fromWhere;
                this.myRoute = r;
            }
            public Point(TimeSpan totalTime, GeoCoords crds, Station fromWhere = null, Route r=null)
            {
                station = null;
                coords = crds;
                this.totalTime = totalTime;
                this.fromWhere = fromWhere;
                this.myRoute = r;
            }
            public override string ToString()
            {
                string from, to,tr, p;
                if (fromWhere != null) from = fromWhere.ToString();
                else from = "null";
                if (station != null) to = station.nameRus;
                else to = "null";
                if (myRoute != null) tr = myRoute.GetShortName();
                else tr = "пешком";
                if (prev != null) p = prev.ToString();
                else p = "null";
                return /*p+" -->> */"{{(" + totalTime.ToString() + ") " + to + " ("+tr+")}} "; // from " + from + " to
            }
            public static Point SelectPointWithMinimalMark(List<Point> points)
            {
                Point p = null;
                foreach (Point t in points) if (!(t.IsVisited))
                    {
                        p = t;
                        break;
                    }
                if (p == null) return p;
                //Point p = points[0];
                foreach (Point t in points) if (!(t.IsVisited) && t.totalTime < p.totalTime) p = t;
                return p;
            }
        }

        public OptimalRoute(GeoCoords nowPos, GeoCoords needPos, DateTime time, string[] types = null, double speed = 6, double dopTimeMinutes=1)
        {
            this.needPos = needPos;
            this.nowPos = nowPos;
            this.speed = speed;
            this.time = time;

            TimeSpan reservedTime = TimeSpan.FromMinutes(dopTimeMinutes);

            TimeSpan maxTimeForGoingBetweenStations = TimeSpan.FromMinutes(3);

            if (types == null) types = allTypes;
            this.types = types;

            int distance = GeoCoords.Distance(nowPos, needPos);
            TimeSpan maxTime = GetTimeForGoingTo(distance);
            List<Station> stationsList = Database.GetStationsAround(nowPos, distance);


            List<Point> points = new List<Point>();
            Point myStartPoint = new Point(TimeSpan.FromSeconds(0), nowPos);
            points.Add(myStartPoint);
            foreach (Station st in stationsList)
            {
                Point add = new Point(TimeSpan.FromDays(25000), st);
                add.tryUpdate(GetTimeForGoingTo(nowPos, st.Coords), myStartPoint);
                points.Add(add);
            }
            Point myFinishPoint = new Point(TimeSpan.FromDays(25000), needPos);
            myFinishPoint.tryUpdate(GetTimeForGoingTo(nowPos, needPos), myStartPoint);
            points.Add(myFinishPoint);

            while (true)
            {
                Point selectedPoint = Point.SelectPointWithMinimalMark(points);
                if (selectedPoint == null) break;

                int vis = 0;
                foreach (Point v in points) if (v.IsVisited) vis++;
                //MessageBox.Show("[all: "+points.Count+", visited: "+vis+"]"+selectedPoint.ToString());
                //
                Station s = selectedPoint.Station;
                if (s != null/* && s.routes!=null*/)
                {
                    //MessageBox.Show("Смотрим остановку '" + s.nameRus + "'");
                    List<Route> rr = new List<Route>();
                    foreach (string t in Database.GetRoutesCodesOnStation(s.hashcode)) rr.Add(Database.GetRouteByHashcode(t));

                    foreach (Route r in rr/*s.routes*/)
                    {
                        //MessageBox.Show("Смотрим маршрут '"+r.GetName()+"' на остановке '" + s.nameRus + "'");
                        Timetable table = Database.GetTimetable(s.hashcode, r.hashcode);
                        if (table.type == Timetable.TableType.table)
                        {
                            TimeSpan waitingTime = table.FindTimeAfter(time + selectedPoint.TotalTime); // (сколько будем ожидать транспорт, когда доберемся до остановки)
                            //MessageBox.Show("Сейчас затрачено "+selectedPoint.TotalTime+" ((now "+(time + selectedPoint.TotalTime).ToString() +"));  транспорт будет через: "+waitingTime.ToString());
                            string nextCode = r.getNextStationCodeAfter(s.hashcode);
                            //MessageBox.Show("После остановки '" + s.nameRus + "' на маршруте '"+r.GetName()+"' идет остановка " + nextCode);
                            if (nextCode != null) // (если остановка не является конечной)
                            {
                                Station next = Database.GetStationByHashcode(nextCode); // (следующая остановка у данного транспорта)

                                Timetable tbl = Database.GetTimetable(next.hashcode, r.hashcode);

                                DateTime tmp = time + waitingTime + selectedPoint.TotalTime; // (new Total time)
                                if (selectedPoint.RouteCode != r.hashcode) tmp += reservedTime;

                                //if (selectedPoint.Previous == null || selectedPoint.RouteCode != selectedPoint.Previous.RouteCode) tmp += reservedTime;
                                TimeSpan goingOnTransportTime = tbl.FindTimeAfter(tmp); // (сколько будем ехать до следующей остановки)

                                //TimeSpan goingTime =;

                                //MessageBox.Show("next: "+(selectedPoint.TotalTime + goingOnTransportTime).ToString());
                                bool contains = false;

                                
                                foreach (Point ppp in points)
                                {
                                    //MessageBox.Show(ppp.Coords + " vs " + next.Coords);
                                    if (ppp.Coords == next.Coords)
                                    {
                                        TimeSpan old = ppp.TotalTime;
                                        string oldPoint = ppp.ToString();
                                        //MessageBox.Show("Найдена остановка: " + ppp.Station.nameRus);

                                        TimeSpan newTime = selectedPoint.TotalTime + waitingTime + goingOnTransportTime; // (new Total time for next station)
                                        //if (ppp.Previous == null || ppp.RouteCode != ppp.Previous.RouteCode) newTime += dopTime;////////////////////

                                        if (ppp.tryUpdate(newTime, selectedPoint, s, r))
                                        {
                                            //MessageBox.Show("Total[["+(selectedPoint.TotalTime)+", "+(goingOnTransportTime + waitingTime) +"]]\n\n[[ '"+old.ToString()+"' replaced to '"+(ppp.TotalTime).ToString()+ "' ]] \n\nOld: \n[[ " + oldPoint + " ]]\n\nNew: \n[[ " + ppp.ToString() + " ]]");
                                        }
                                        contains = true;
                                        break;
                                    }
                                }
                                if (!contains) // (если вершина "неизвестна")
                                {

                                    TimeSpan newTime = selectedPoint.TotalTime/* + waitingTime*/ + goingOnTransportTime;

                                    points.Add(new Point(newTime, next, s,r));
                                    //MessageBox.Show("Остановка не найдена: " + next.nameRus);
                                }
                            }
                        }
                        else if (table.type == Timetable.TableType.periodic)
                        {
                            // ...........................
                        }
                    }
                }

                //// (попробовать пересесть на другой транспорт)

                foreach (Point p in points) if(p!= selectedPoint) // (попробуем пройти пешком до других "вершин")
                {

                    string oldPoint = p.ToString();
                    TimeSpan old = p.TotalTime;


                    GeoCoords tmp = selectedPoint.Coords;
                    int dist = GeoCoords.Distance(tmp, p.Coords);
                    int ddd = dist;
                    Point mytmpP2 = p;
                    while (mytmpP2.Previous != null)
                    {

                        if (mytmpP2.RouteCode == null) dist += GeoCoords.Distance(mytmpP2.Coords, mytmpP2.Previous.Coords);
                        else break;
                        mytmpP2 = mytmpP2.Previous;
                    }
                    double sp = 0.001*dist / GetTimeForGoingTo(dist, true).TotalHours;


                    TimeSpan goingTime = GetTimeForGoingTo(ddd, true, sp);

                    TimeSpan goingTimeTest = GetTimeForGoingTo(selectedPoint.Coords, p.Coords, true);

                    Point mytmpP = p;
                    TimeSpan sdf = p.TotalTime;
                    while (mytmpP.Previous != null)
                    {
                        mytmpP = mytmpP.Previous;
                        if (mytmpP.RouteCode != null)
                        {
                            goingTimeTest += sdf - mytmpP.TotalTime;
                            break;
                        }
                    }

                    //if (goingTimeTest < maxTimeForGoingBetweenStations)
                    //{
                    TimeSpan newTime = selectedPoint.TotalTime + goingTime;
                    if (p != myFinishPoint) newTime += reservedTime;
                        //if (p != myFinishPoint) newTime += dopTime;

                        if (p.tryUpdate(newTime, selectedPoint, s))
                        {
                            //MessageBox.Show("Total[["+(selectedPoint.TotalTime)+", "+(GetTimeForGoingTo(selectedPoint.Coords, p.Coords)) +"]]\n\n[[ '" + old.ToString() + "' replaced to '" + (p.TotalTime).ToString() + "' ]] \n\nOld: \n[[ " + oldPoint + " ]]\n\nNew: \n[[ " + p.ToString() + " ]]", "Пешком между остановками");
                        }
                    //}
                }


                if (selectedPoint.TotalTime > myFinishPoint.TotalTime) points.Remove(selectedPoint);
                else selectedPoint.Visited();
            }

            
            foreach (Point pr in points)
            {
                //MessageBox.Show("Найдена остановка: " + ppp.Station.nameRus);
                string oldPoint = pr.ToString();
                TimeSpan old = pr.TotalTime;
                if (pr.tryUpdate(pr.TotalTime + GetTimeForGoingTo(pr.Coords, needPos), pr, pr.Station))
                {
                    //MessageBox.Show("Total[[" + (pr.TotalTime) + ", " + (GetTimeForGoingTo(pr.Coords, needPos)) + "]]\n\n[[ '" + old.ToString() + "' replaced to '" + (pr.TotalTime).ToString() + "' ]] \n\nOld: \n[[ " + oldPoint + " ]]\n\nNew: \n[[ " + pr.ToString() + " ]]", "Пешком до места назначения");
                }
            }

            Point tmpP = myFinishPoint;
            this.points.Add(tmpP.ToString());
            while (tmpP.Previous != null)
            {
                tmpP = tmpP.Previous;
                this.points.Add(tmpP.ToString());
            }

            //MessageBox.Show(TotalGoingTime(myFinishPoint).ToString());
            

            //MessageBox.Show("end !!! :)");

            //foreach (Point p in points) this.points.Add(p.ToString());

            //MessageBox.Show(startList.Count.ToString());
            //MessageBox.Show(types.Length.ToString());
        }

        protected TimeSpan TotalGoingTime(Point p)
        {
            TimeSpan goingTimeTest = new TimeSpan();

            while (p.Previous != null)
            {
                if (p.RouteCode == null) goingTimeTest += p.TotalTime - p.Previous.TotalTime;
                p = p.Previous;
            }
            
            return goingTimeTest;
        }

        public TimeSpan GetTimeForGoingTo(double distance, bool betweenPoints=false, double sp=0)///////////////////
        {
            double tmp=0;
            /*if (betweenPoints) tmp = speed;
            else */tmp = (-Math.Atan(distance / 600 - 1.1) * 5 / Math.PI)+5.5;
            //MessageBox.Show(distance + " метров, speed = " + tmp);
            if (sp != 0)
                tmp = sp;
            int seconds = (int)(distance / ( (    tmp  ) * 5 / 18));  //(int)(distance / (speed * 5 / 18));
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts;
        }
        public TimeSpan GetTimeForGoingTo(GeoCoords fromPos, GeoCoords toPos, bool betweenPoints = false)
        {
            return GetTimeForGoingTo(GeoCoords.Distance(fromPos, toPos), betweenPoints);
        }

        public void GetOptimalRoute(GeoCoords a, GeoCoords b, DateTime time)
        {

        }
    }
}
