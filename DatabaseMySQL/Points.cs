using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransportClasses
{
    public partial class OptimalRoute
    {
        public class Points : ICollection<Points.Point>, ICloneable
        {
            public class Point : IComparable<Point>, ICloneable
            {
                internal Points currentGraph;
                public Points CurrentGraph
                {
                    get
                    {
                        return currentGraph;
                    }
                }
                //private LinkedList<Point> updatesHistory = new LinkedList<Point>();
                /*public LinkedList<Point> UpdatesHistory
                {
                    get
                    {
                        return updatesHistory;
                    }
                }*/
                private Route myRoute = null;
                public Route Route
                {
                    get
                    {
                        return myRoute;
                    }
                    internal set
                    {
                        myRoute = value;
                    }
                }
                public string RouteCode
                {
                    get
                    {
                        return ((myRoute == null) ? null : myRoute.hashcode);
                    }
                }
                private Point prev = null;
                public Point Previous
                {
                    get
                    {
                        return prev;
                    }
                    internal set
                    {
                        prev = value;
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
                private Station fromWhere = null, station = null;
                public Station Station
                {
                    get
                    {
                        return station;
                    }
                }
                public string StationCode
                {
                    get
                    {
                        if (station == null) return null;
                        return station.hashcode;
                    }
                }
                public string PreviousStationCode
                {
                    get
                    {
                        if (fromWhere == null) return null;
                        return fromWhere.hashcode;
                    }
                }
                private bool visited = false;
                public bool tryUpdate(TimeSpan totalTime, Point previousPoint, Station fromWhere = null, Route myRoute = null)
                {
                    if (totalTime < this.totalTime)
                    {
                        //updatesHistory.AddLast(new Point(this));

                        this.myRoute = myRoute;
                        this.prev = previousPoint;
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
                public Point(TimeSpan totalTime, Station station, Station fromWhere = null, Route r = null)
                {
                    this.station = station;
                    this.coords = station.Coords;
                    this.totalTime = totalTime;
                    this.fromWhere = fromWhere;
                    this.myRoute = r;
                    station.Point = this;
                }
                public Point(TimeSpan totalTime, GeoCoords crds, Station fromWhere = null, Route r = null)
                {
                    this.station = null;
                    this.coords = crds;
                    this.totalTime = totalTime;
                    this.fromWhere = fromWhere;
                    this.myRoute = r;
                }
                private Point(Point point/*, IgnoringFragments ignoringFragments = null*/)
                {
                    if (point == null) throw new ArgumentNullException();
                    ////if (ignoringFragments == null)
                    ////{
                    //this.updatesHistory = point.updatesHistory;
                    this.myRoute = point.myRoute;
                    this.prev = point.prev;// == null ? null : new Point(point.prev);
                    this.coords = point.coords;
                    this.fromWhere = point.fromWhere;
                    this.station = point.station;

                    this.totalTime = TimeSpan.FromSeconds(point.totalTime.TotalSeconds);
                    this.visited = point.visited;
                    ////}
                    ////else
                    ////{
                    ////    /*if (point.updatesHistory.First != null) point = point.updatesHistory.First.Value;
                    ////    this.updatesHistory = point.updatesHistory;*/
                    ////    this.myRoute = point.myRoute;
                    ////    this.prev = point.prev;// == null ? null : new Point(point.prev);
                    ////    this.coords = point.coords;
                    ////    this.fromWhere = point.fromWhere;
                    ////    this.station = point.station;

                    ////    this.totalTime = point.totalTime;
                    ////    visited = false;

                    //if (IgnoringFragment.Contains(ignoringFragments, point.StationCode, point.RouteCode, point.PreviousStationCode))
                    //{
                    //    if (point.updatesHistory.Last == null)
                    //        throw new Exception();
                    //    if (point.updatesHistory.Last.Previous == null)
                    //    {
                    //        /*if (point.updatesHistory.Count <= 1) this.updatesHistory = point.updatesHistory;
                    //        else this.updatesHistory = point.updatesHistory.Last.Previous.Value.updatesHistory;*/
                    //        this.updatesHistory = point.updatesHistory;
                    //        this.myRoute = null;
                    //        this.prev = null;
                    //        this.coords = point.coords;
                    //        this.fromWhere = null;
                    //        this.station = null;
                    //        visited = true;
                    //        totalTime = TimeSpan.FromDays(25001);
                    //    }
                    //    else
                    //    {
                    //        Point previousInHistory = point.updatesHistory.First.Value;
                    //        //Point previousInHistory = point.updatesHistory.Last.Previous.Value;
                    //        Point newPnt = new Point(previousInHistory, ignoringFragments);
                    //        point = newPnt;

                    //        /*if (point.updatesHistory.Count <= 1) this.updatesHistory = point.updatesHistory;
                    //        else*/ this.updatesHistory = point.updatesHistory.Last.Previous.Value.updatesHistory;
                    //        this.myRoute = point.myRoute;
                    //        this.prev = point.prev == null ? null : new Point(point.prev);
                    //        this.coords = point.coords;
                    //        this.fromWhere = point.fromWhere;
                    //        this.station = point.station;

                    //        this.totalTime = point.totalTime;
                    //        visited = point.visited;
                    //    }
                    //}
                    //else
                    //{
                    //    //if (point.updatesHistory.Count <= 1)
                    //    this.updatesHistory = point.updatesHistory;
                    //    //else this.updatesHistory = point.updatesHistory.Last.Previous.Value.updatesHistory;
                    //    this.myRoute = point.myRoute;
                    //    this.prev = point.prev == null ? null : new Point(point.prev);
                    //    this.coords = point.coords;
                    //    this.fromWhere = point.fromWhere;
                    //    this.station = point.station;

                    //    this.totalTime = point.totalTime;
                    //    visited = point.visited;
                    //}


                    //////    bool ok = true;
                    //////metka2:
                    //////    if (ok && ignoringFragments != null)
                    //////    {
                    //////    metka:
                    //////        foreach (Point p in point.updatesHistory)
                    //////        {
                    //////            if (IgnoringFragment.Contains(ignoringFragments, p.StationCode, p.RouteCode, p.PreviousStationCode)) 
                    //////            {
                    //////                if (p.updatesHistory.Last != null && p.updatesHistory.Last.Previous == null)
                    //////                {
                    //////                    ok = false;
                    //////                    this.updatesHistory = p.updatesHistory;
                    //////                    this.myRoute = p.myRoute;
                    //////                    this.prev = p.prev == null ? null : new Point(point.prev);
                    //////                    this.coords = p.coords;
                    //////                    this.fromWhere = p.fromWhere;
                    //////                    this.station = p.station;
                    //////                    visited = true;
                    //////                    totalTime = TimeSpan.FromDays(25001);
                    //////                    goto metka2;
                    //////                }
                    //////                point = new Point(p.UpdatesHistory.Last.Previous.Value, /*needUnvisit,*/ ignoringFragments);
                    //////                goto metka;
                    //////            }
                    //////        }
                    //////    }
                    //////    if (ok)
                    //////    {
                    //////        this.updatesHistory = point.updatesHistory;
                    //////        this.myRoute = point.myRoute;
                    //////        this.prev = point.prev == null ? null : new Point(point.prev);
                    //////        this.coords = point.coords;
                    //////        this.fromWhere = point.fromWhere;
                    //////        this.station = point.station;

                    //////        this.totalTime = point.totalTime;
                    //////        /*if (needUnvisit) visited = false;
                    //////        else*/ visited = point.visited;
                    //////    }
                    //}
                }
                public override string ToString()
                {
                    string from, to, tr, p;
                    if (fromWhere != null) from = fromWhere.ToString();
                    else from = "null";
                    if (station != null) to = station.ToString();
                    else to = "null";
                    if (myRoute != null) tr = myRoute.GetShortName();
                    else tr = "пешком";
                    if (prev != null) p = prev.ToString();
                    else p = "null";
                    return /*p+" -->> */"(" + totalTime.ToString() + ") " + to + " (" + tr + ")"; // from " + from + " to
                }
                /*
                public static Point SelectPointWithMinimalMark(List<Point> points)
                {
                    Point p = null;
                    foreach (Point t in points) if (!(t.IsVisited))
                        {
                            p = t;
                            break;
                        }
                    if (p == null) return p;
                    foreach (Point t in points) if (!(t.IsVisited) && t.totalTime < p.totalTime) p = t;
                    return p;
                }
                */
                public int CompareTo(Point other)
                {
                    if (other == null) throw new ArgumentNullException();

                    if (visited != other.visited)
                    {
                        if (visited) return 1;
                        return -1;
                    }

                    if (totalTime == other.totalTime) return 0;
                    if (totalTime > other.totalTime) return 1;
                    return -1;
                }

                public object Clone()
                {
                    return new Point(this);
                }
                /*
                /// <summary>
                /// Откатывает вершину до момента, когда она еще не содержала блокируемые фрагменты пути.
                /// </summary>
                /// <param name="ignoringFragments">Блокируемые фрагменты пути.</param>
                /// <returns>Возвращает ближайшую версию, не содержащую блокируемые фрагменты пути.</returns>
                public Point Downgrade(IgnoringFragments ignoringFragments)
                {
                    //Point tmp = new Point(TimeSpan.FromDays(25000), coords, null, null); 
                    //tmp
                    ////Point tmp;
                    ////if (updatesHistory.Count == 0) tmp = this;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    ////else tmp = this.updatesHistory.First.Next == null ? updatesHistory.First.Value : updatesHistory.First.Next.Value;

                    //Point tmpResult = new Point(this, ignoringFragments);

                    //return tmpResult;
                    if (this.updatesHistory.Count == 0) return null;
                    return new Point(this, ignoringFragments);
                }
                */
                public TimeSpan TotalGoingTime
                {
                    get
                    {
                        TimeSpan goingTime = new TimeSpan();
                        Point tmpP = this;
                        //this.points.Add(tmpP.ToString());
                        while (tmpP.Previous != null)
                        {
                            if (tmpP.RouteCode == null) goingTime += tmpP.TotalTime - tmpP.Previous.TotalTime;
                            tmpP = tmpP.Previous;
                        }
                        return goingTime;
                    }
                }
                public int TotalTransportChangingCount
                {
                    get
                    {
                        int result = 0;
                        Point tmpP = this;
                        //this.points.Add(tmpP.ToString());
                        while (tmpP.Previous != null)
                        {
                            if (tmpP.RouteCode != null && tmpP.RouteCode != tmpP.Previous.RouteCode) result++;
                            tmpP = tmpP.Previous;
                        }
                        return result;
                    }
                }
            }

            public static TimeSpan DEBUG_timeToCreateNext = new TimeSpan();
            public Point startPoint;
            public Point finalPoint;
            private List<Point> points;
            private Point currentSelectedPoint = null;
            public Points(Point startPoint, Point finalPoint)
            {
                this.startPoint = startPoint;
                this.finalPoint = finalPoint;
                points = new List<Point>();
            }
            public void Fill(IEnumerable<Station> stationsList, double goingSpeed, TimeSpan reservedTime, IgnoringFragments myIgnoringFragments = null)
            {
                foreach (Station st in stationsList)
                {
                    if (myIgnoringFragments != null && myIgnoringFragments.Contains(st.hashcode, null, null)) continue;

                    Point add = new Point(TimeSpan.FromDays(25000), st);
                    add.tryUpdate(GetTimeForGoingTo(this.startPoint.Coords, st.Coords, goingSpeed) + reservedTime, this.startPoint);
                    this.Add(add);
                }
            }
            private Point Next()
            {
                DateTime t0 = DateTime.Now;
                if (currentSelectedPoint != null) currentSelectedPoint.Visited();

                //points.Sort();
                

                //currentSelectedPoint = points[0];
                currentSelectedPoint = SelectPointWithMinimalMark();
                
                //currentSelectedPoint.currentGraph = (Points)this.Clone();
                DEBUG_timeToCreateNext += DateTime.Now - t0;
                return currentSelectedPoint;
                //return currentSelectedPoint.IsVisited ? currentSelectedPoint = null : currentSelectedPoint;
            }
            public Point SelectPointWithMinimalMark()
            {
                Point p = null;
                foreach (Point t in points) if (!(t.IsVisited))
                    {
                        p = t;
                        break;
                    }
                if (p == null) return p;
                foreach (Point t in points) if (!(t.IsVisited) && t.TotalTime < p.TotalTime) p = t;
                return p;
            }
            /// <summary>
            /// Пытатся найти вершину по станции и, в случае отсутствия таковой, создает новую.
            /// </summary>
            /// <param name="station"></param>
            /// <returns></returns>
            public Point Find(Station station)
            {
                //foreach (Point p in points) if (p.Station.hashcode == station.hashcode) return p;
                if (station.Point != null) return station.Point;
                Point newCreatdPoint = new Point(TimeSpan.FromDays(25000), station);
                points.Add(newCreatdPoint);
                return newCreatdPoint;
            }
            public Point Find(Point point)
            {
                foreach (Point p in points) if (p.Coords == point.Coords && p.StationCode == point.StationCode) return p;
                return null;
            }
            public int Count
            {
                get
                {
                    return points.Count;
                }
            }
            public int VisitedCount
            {
                get
                {
                    int i = 0;
                    foreach (Point p in this) if (p.IsVisited) i++;
                    return i;
                }
            }
            public bool IsReadOnly
            {
                get
                {
                    return false;
                    //throw new NotImplementedException();
                }
            }
            public void Add(Point item)
            {
                if (item != null) points.Add(item);
            }
            public void Clear()
            {
                points.Clear();
            }
            public bool Contains(Point item)
            {
                return points.Contains(item);
            }
            public void CopyTo(Point[] array, int arrayIndex)
            {
                points.CopyTo(array, arrayIndex);
            }
            public IEnumerator<Point> GetEnumerator()
            {
                return points.GetEnumerator();
            }
            public bool Remove(Point item)
            {
                return points.Remove(item);
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return points.GetEnumerator();
            }
            /*
            /// <summary>
            /// Откатывает вершины до момента, когда они еще не содержали блокируемые фрагменты пути.
            /// </summary>
            /// <param name="ignoringFragments">Блокируемые фрагменты пути.</param>
            /// <returns>Возвращает ближайшую версию, не содержащую блокируемые фрагменты пути.</returns>
            public Points Downgrade(IgnoringFragments ignoringFragments)
            {
                Points newP = new Points(startPoint, finalPoint);
                foreach (Point p in this) newP.Add(p.Downgrade(ignoringFragments));
                return newP;
            }*/
            /// <summary>
            /// Находит кратчайший путь.
            /// </summary>
            /// <param name="myStartPoint"></param>
            /// <param name="myFinishPoint"></param>
            /// <param name="myIgnoringFragments"></param>
            /// <param name="time"></param>
            /// <param name="types"></param>
            /// <param name="speed"></param>
            /// <param name="reservedTime"></param>
            /// <param name="databaseMysqlConnection"></param>
            public void CountShortWay(/*Point myStartPoint, Point myFinishPoint,*/List<Route> ignoringRoutes, IgnoringFragments myIgnoringFragments, DateTime time, IEnumerable<RouteType> types, double speed, TimeSpan reservedTime, MySql.Data.MySqlClient.MySqlConnection databaseMysqlConnection)
            {
                //TimeSpan overLimitResedvedTime = TimeSpan.FromMinutes(20);

                DEBUG_timeToCreateNext = new TimeSpan();
                DateTime t0 = DateTime.Now, t1;
                TimeSpan /*t_total = new TimeSpan(),*/ t_giversin = new TimeSpan(), t_finding_time = new TimeSpan()/*, t_upd_in_stations = new TimeSpan()*/;
                TimeSpan /*t_updating_total = new TimeSpan(),*/ t_going_check_total = new TimeSpan(), t_stations = new TimeSpan(), t_without_finding_marks = new TimeSpan();
                for (Point selectedPoint = Next(); selectedPoint != null; selectedPoint = Next())
                {
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    if (selectedPoint.TotalTime > finalPoint.TotalTime/* + overLimitResedvedTime*/) //... Пропускаем и удаляем, если значение метки превышает минимальное время до пункта назначения.
                    {
                        //points.Remove(selectedPoint);//
                        break;
                        //continue;//
                    }
                    DateTime t4 = DateTime.Now;
                    DateTime t3 = DateTime.Now;
                    Station selectedPointStation = selectedPoint.Station;
                    if (selectedPointStation != null)
                    {
                        // Момент, когда мы прибудем на остановку:
                        DateTime momentWhenComingToStation = time + selectedPoint.TotalTime;

                        //t1 = DateTime.Now;
                        // Загружаем маршруты, проходящие через остановку:
                        List<Route> routesOnStation;// = routesOnStation = Database.GetRoutesOnStation(selectedPointStation.hashcode, canReadDataFromLocalCopy: true);
                        if (selectedPointStation.routes != null) routesOnStation = selectedPointStation.routes;
                        else continue;

                        //t_total += DateTime.Now - t1;
                        foreach (Route selectedRoute in routesOnStation)
                        {
                            if (ignoringRoutes != null && ignoringRoutes.Contains(selectedRoute)) continue;
                            if (types.Contains(RouteTypeConverter.FromString(selectedRoute.type)))
                            {
                                //t1 = DateTime.Now;
                                // Следующая остановка у данного транспорта:
                                Station nextStation = selectedRoute.getNextStation(selectedPointStation);

                                /*// Код остановки, на которую попадем на данном транспорте:
                                string nextCode = selectedRoute.getNextStationCodeAfter(selectedPointStation.hashcode, canReadDataFromLocalCopy: true);*/
                                //t_total += DateTime.Now - t1;
                                if (nextStation/*nextCode*/ != null) // Если остановка не является конечной, то:
                                {
                                    //t1 = DateTime.Now;
                                    // Загружаем расписание:
                                    Timetable table = selectedRoute.GetTimetable(selectedPointStation);//Database.GetTimetable(selectedPointStation.hashcode, selectedRoute.hashcode, databaseMysqlConnection, canReadDataFromLocalCopy: true);
                                    //t_total += DateTime.Now - t1;
                                    // Блокируем попытку попасть указанным транспортом на указанную остановку:
                                    if (myIgnoringFragments.Contains(nextStation.hashcode/*nextCode*/, selectedRoute.hashcode, selectedPointStation.hashcode)) continue;

                                    if (table.type == TableType.table) // Если это точное расписание, то:
                                    {
                                        // Минимальный начальный момент, с который можно начинать ожидать посадку:
                                        DateTime momentWhenAskingForGoing = momentWhenComingToStation;

                                        // Резервируем дополнительное время, если будем пересаживаться на другой маршрут:
                                        //if (selectedPoint.RouteCode == null || selectedPoint.RouteCode != selectedRoute.hashcode) momentWhenAskingForGoing += reservedTime;
                                        if (selectedPoint.Route != null && selectedPoint.Route != selectedRoute) momentWhenAskingForGoing += reservedTime;

                                        t1 = DateTime.Now;
                                        // Подсчитываем, сколько будем ожидать этот транспорт на остановке:
                                        TimeSpan waitingTime = table.FindTimeAfter(momentWhenAskingForGoing);
                                        t_finding_time += DateTime.Now - t1;

                                        // Момент, когда мы сядем в транспорт:
                                        DateTime momentWhenSitInTransport = momentWhenAskingForGoing + waitingTime;
                                        //t1 = DateTime.Now;
                                        /*// Следующая остановка у данного транспорта:
                                        Station nextStation = Database.GetStationByHashcode(nextCode, databaseMysqlConnection, canReadDataFromLocalCopy: true);*/

                                        // И соответствующее расписание на этой остановке:
                                        Timetable tbl = selectedRoute.GetTimetable(nextStation);//Database.GetTimetable(nextStation.hashcode, selectedRoute.hashcode, databaseMysqlConnection, canReadDataFromLocalCopy: true);
                                        //t_total += DateTime.Now - t1;
                                        t1 = DateTime.Now;
                                        // (сколько будем ехать до следующей остановки):
                                        TimeSpan goingOnTransportTime = tbl.FindTimeAfter(momentWhenSitInTransport);
                                        t_finding_time += DateTime.Now - t1;

                                        // Метка времени:
                                        TimeSpan onNextPointTotalTime = momentWhenSitInTransport - momentWhenComingToStation + goingOnTransportTime + selectedPoint.TotalTime;
                                        //t1 = DateTime.Now;
                                        if (Find(nextStation).tryUpdate(onNextPointTotalTime, selectedPoint, selectedPointStation, selectedRoute))
                                        {

                                        }
                                        //t_updating_total += DateTime.Now - t1;
                                    }
                                    else if (table.type == TableType.periodic)
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                            }
                        }
                    }
                    //t_upd_in_stations = TimeSpan.FromMilliseconds(t_updating_total.TotalMilliseconds);
                    t_stations += DateTime.Now - t3;
                    GeoCoords selectedPointCoords = selectedPoint.Coords;
                    t_without_finding_marks += DateTime.Now - t4;
                    // Нет смысла идти пешком "транзитом" через остановку:
                    /*//11111111111111111111111111!!!!!!!!!!!!!!!!!*/
                    if (selectedPoint.Route == null) continue;
                    t4 = DateTime.Now;
                    DateTime t2 = DateTime.Now;
                    // Попробуем пройти пешком до других "вершин":
                    foreach (Point p in this/*!!!!!!!!!!!!points*/)
                        if (!p.IsVisited && p != selectedPoint)
                        {
                            // Блокируем попытку дойти пешком до указанной остановки:
                            if (myIgnoringFragments.Contains(p.StationCode, null, selectedPointStation.hashcode)) continue;

                            t1 = DateTime.Now;
                            int distanceToSelectedPoint = GeoCoords.Distance(selectedPointCoords, p.Coords);
                            t_giversin += DateTime.Now - t1;

                            TimeSpan goingTime = GetTimeForGoingTo(distanceToSelectedPoint, speed/*, true, sp*/);

                            TimeSpan newTime = selectedPoint.TotalTime + goingTime + reservedTime;
                            /*if (p != myFinishPoint)*/ // newTime += reservedTime;
                            //t1 = DateTime.Now;
                            if (p.tryUpdate(newTime, selectedPoint, selectedPointStation))
                            {

                            }
                            //t_updating_total += DateTime.Now - t1;
                        }
                    t_going_check_total += DateTime.Now - t2;

                    t_without_finding_marks += DateTime.Now - t4;
                    if (myIgnoringFragments.Contains(null, null, selectedPointStation.hashcode)) continue;
                    //t1 = DateTime.Now;
                    if (finalPoint.tryUpdate(selectedPoint.TotalTime + GetTimeForGoingTo(selectedPointCoords, finalPoint.Coords, speed), selectedPoint, selectedPointStation))
                    {

                    }
                    //t_updating_total += DateTime.Now - t1;
                }

                t1 = DateTime.Now;
                // Сокращаем время ходьбы пешком до минимума и избавляемся от "бессмысленных" пересадок, сохраняя общее время неизменным:
                Point currentPoint = finalPoint.Previous;
                while (currentPoint != startPoint)
                {
                    Route r = currentPoint.Route;
                    if (r != null)
                    {
                        Point previousPoint = currentPoint.Previous;
                        if (previousPoint != startPoint && previousPoint.Route != r) // Если на предыдущую остановку мы добрались другим транспортом, то:
                        {
                            Station previousRouteStation = r.getPreviousStation(previousPoint.Station);
                            if (previousRouteStation != null)
                            {
                                Point point = previousRouteStation.Point;
                                if (point != null && point.IsVisited)
                                {
                                    Timetable ttt = r.GetTimetable(previousRouteStation);
                                    if (ttt != null)
                                    {
                                        DateTime ddd = time + previousPoint.TotalTime;
                                        TimeSpan moment = r.GetTimetable(currentPoint.Station).FindTimeAfter(ddd);
                                        TimeSpan tmp_time = ttt.FindTimeBefore(ddd + moment);

                                        TimeSpan momentArriveOnCurrent = previousPoint.TotalTime + moment;
                                        TimeSpan momentSittingOnPrevious = momentArriveOnCurrent + tmp_time;
                                        /*bool bbb = point.Route != null && point.Route.GetTimetable(point.Station) != null && point.Route.GetTimetable(point.Station).FindTimeAfter(time + point.TotalTime) <= previousPoint.TotalTime + moment + tmp_time;
                                        if (bbb)
                                        {
                                            previousPoint.Route = r;
                                            previousPoint.Previous = point;////!bbb && point.TotalTime <= momentSittingOnPrevious &&
                                    }
                                    else */
                                        if (/*point.TotalGoingTime>=previousPoint.TotalGoingTime || */point.TotalTime <= previousPoint.TotalTime/* && point.TotalGoingTime <= previousPoint.TotalGoingTime*/)
                                        {
                                            previousPoint.Route = r;
                                            previousPoint.Previous = point;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    currentPoint = currentPoint.Previous;
                }

                //MessageBox.Show("Total: " + (DateTime.Now - t0).TotalMilliseconds
                ///**///+ "\n\tWithout finding minimal marks: " + t_without_finding_marks.TotalMilliseconds
                //+ "\n\tFinding minimal marks: " + DEBUG_timeToCreateNext.TotalMilliseconds
                //+ "\n\tStations: " + t_stations.TotalMilliseconds
                //+ "\n\t\tFinding time: " + t_finding_time.TotalMilliseconds
                //+ "\n\tWalking checks: " + t_going_check_total.TotalMilliseconds
                //+ "\n\t\tGaversin: " + t_giversin.TotalMilliseconds/**/
                //+ "\n\tRecount: " + (DateTime.Now - t1).TotalMilliseconds
                //);
            }

            public object Clone()
            {
                Point clonedStartPoint = (Point)startPoint.Clone();
                Point clonedFinalPoint = (Point)finalPoint.Clone();
                Points clonedPoints = new Points(clonedStartPoint, clonedFinalPoint);

                foreach (Point p in points) clonedPoints.Add((Point)p.Clone());

                foreach (Point p in points)
                {
                    if (p.Previous == null) continue;
                    if (p.Previous.Coords == startPoint.Coords) clonedPoints.Find(p).Previous = clonedStartPoint;
                    else clonedPoints.Find(p).Previous = clonedPoints.Find(p.Previous);
                }

                if (clonedFinalPoint.Previous != null)
                {
                    if (clonedFinalPoint.Previous.Coords == startPoint.Coords) clonedFinalPoint.Previous = startPoint;
                    else clonedFinalPoint.Previous = clonedPoints.Find(clonedFinalPoint.Previous);
                }
                return clonedPoints;
            }
        }
    }
}
