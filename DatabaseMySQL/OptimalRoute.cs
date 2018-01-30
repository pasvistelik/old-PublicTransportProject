using Newtonsoft.Json;
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
        public enum Priority
        {
            MinimalTime,
            MinimalGoingTime,
            MinimalTransportChanging
        };
        public enum RouteType
        {
            bus, trolleybus, express_bus, marsh, tram, metro
        }
        public static class RouteTypeConverter
        {
            public static RouteType FromString(string type)
            {
                if (type == "bus") return RouteType.bus;
                if (type == "trolleybus") return RouteType.trolleybus;
                if (type == "express_bus") return RouteType.express_bus;
                if (type == "marsh") return RouteType.marsh;
                if (type == "tram") return RouteType.tram;
                if (type == "metro") return RouteType.metro;
                throw new FormatException("Указан неизвестный тип транспорта.");
            }
        }


        private Points myPoints = null;
        public Points MyPoints
        {
            get
            {
                return myPoints;
            }
        }
        private bool visited = false;
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
        public void CancelVisitedMark()
        {
            visited = false;
        }
        private readonly TimeSpan totalGoingTime;
        public TimeSpan TotalGoingTime
        {
            get
            {
                return totalGoingTime;
            }
        }
        private readonly TimeSpan totalTime;
        public TimeSpan TotalTime
        {
            get
            {
                return totalTime;
            }
        }
        private readonly int totalTransportChangingCount;
        public int TotalTransportChangingCount
        {
            get
            {
                return totalTransportChangingCount;
            }
        }
        public static OptimalRoute SelectOptimalRouteWithMinimalMark(IEnumerable<OptimalRoute> points, Priority priority)
        {
            OptimalRoute p = null;
            foreach (OptimalRoute t in points) if (!(t.IsVisited))
                {
                    p = t;
                    break;
                }
            if (p == null) return p;
            if (priority == Priority.MinimalTime)
            {
                foreach (OptimalRoute t in points) if (!(t.IsVisited) && t.TotalTime < p.TotalTime) p = t;
            }
            else if (priority == Priority.MinimalGoingTime)
            {
                foreach (OptimalRoute t in points) if (!(t.IsVisited) && t.TotalGoingTime < p.TotalGoingTime) p = t;
            }
            else if (priority == Priority.MinimalTransportChanging)
            {
                foreach (OptimalRoute t in points) if (!(t.IsVisited) && t.TotalTransportChangingCount < p.TotalTransportChangingCount) p = t;
            }
            return p;
        }
        public readonly double goingSpeed;
        public readonly GeoCoords nowPos;
        public readonly GeoCoords needPos;
        public readonly DateTime time;
        public readonly IEnumerable<RouteType> types;
        public readonly IgnoringFragments myIgnoringFragments;
        //public readonly Point finalPoint, startPoint;
        public List<string> points = new List<string>();
        
        List<Priority> priorityList = new List<Priority>(new []{Priority.MinimalTime, Priority.MinimalTime, Priority.MinimalTransportChanging});
        
        public readonly List<Route> ignoringRoutes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nowPos"></param>
        /// <param name="needPos"></param>
        /// <param name="time">Начальный момент времени.</param>
        /// <param name="types">Допустимые типы транспорта.</param>
        /// <param name="goingSpeed">Скорость движения пешком.</param>
        /// <param name="dopTimeMinutes">Резерв времени для ожидания транспорта.</param>
        public OptimalRoute(GeoCoords nowPos, GeoCoords needPos, DateTime time, IEnumerable<RouteType> types = null, double goingSpeed = 5, double dopTimeMinutes = 2, List<Route> ignoringRoutesAdd = null, IEnumerable<IgnoringFragment> ignoringList = null, /*Point myStartPoint = null,*/ Points myPoints = null/*, Point myFinishPoint = null*/)
        {
            if (ignoringRoutesAdd != null) this.ignoringRoutes = ignoringRoutesAdd;
            else this.ignoringRoutes = new List<Route>();
            Database.TryInitialize();
            MySql.Data.MySqlClient.MySqlConnection databaseMysqlConnection = Database.conn;
            
            this.needPos = needPos;
            this.nowPos = nowPos;
            this.goingSpeed = goingSpeed;
            this.time = time;
            TimeSpan reservedTime = TimeSpan.FromMinutes(dopTimeMinutes);
            if (types == null) types = new RouteType[] { RouteType.bus, RouteType.trolleybus, /*RouteType.tram, RouteType.metro, RouteType.express_bus, RouteType.marsh*/ };
            this.types = types;//{ RouteType.bus };//
            
            if (ignoringList != null) myIgnoringFragments = new IgnoringFragments(ignoringList);
            else myIgnoringFragments = new IgnoringFragments();
           
            //if (myPoints == null)
            //{
            Points.Point myStartPoint = new Points.Point(TimeSpan.FromSeconds(0), nowPos);
            Points.Point myFinishPoint = new Points.Point(TimeSpan.FromDays(25000), needPos);
            myFinishPoint.tryUpdate(GetTimeForGoingTo(nowPos, needPos, goingSpeed) + TimeSpan.FromMinutes(20), myStartPoint);//!!!!!!!!!!!!!!!!!
            //myFinishPoint.tryUpdate(GetTimeForGoingTo(GoogleApi.GetWalkingDistance(nowPos, needPos), goingSpeed), myStartPoint);
            myPoints = new Points(myStartPoint, myFinishPoint);
            // Получим "начальный" список станций:
            List<Station> stationsList = Database.GetStationsAround(myPoints.startPoint.Coords, GeoCoords.Distance(myPoints.startPoint.Coords, myPoints.finalPoint.Coords), canReadDataFromLocalCopy: true);
            myPoints.Fill(stationsList, goingSpeed, reservedTime, myIgnoringFragments);
            
            //}

            //else
            //{
            //    if (myFinishPoint == null || myStartPoint == null)
            //        throw new Exception();

            //}

            //DateTime ttt1 = DateTime.Now;
            // Находим кратчайшие пути до всех вершин:
            myPoints.CountShortWay(/*myStartPoint, myFinishPoint,*/ignoringRoutes, myIgnoringFragments, time, types, goingSpeed, reservedTime, databaseMysqlConnection);

            //myFinishPoint.tryUpdate(GetTimeForGoingTo(nowPos, needPos, goingSpeed), myStartPoint);//!!!!!!!!!!!!!!!!!

            //MessageBox.Show((DateTime.Now - ttt1).TotalMilliseconds.ToString());

            Points.Point tmpP = myPoints.finalPoint;
            this.points.Add(tmpP.ToString());////
            while (tmpP.Previous != null)
            {
                tmpP = tmpP.Previous;//
                this.points.Add(tmpP.ToString());
                if (tmpP.Previous == null && tmpP.Coords != myPoints.startPoint.Coords)
                    throw new Exception("Где-то удалилась часть маршрута...");   
            }

            this.totalTime = myPoints.finalPoint.TotalTime;
            this.totalGoingTime = myPoints.finalPoint.TotalGoingTime;
            this.totalTransportChangingCount = myPoints.finalPoint.TotalTransportChangingCount;
            /*this.finalPoint = myFinishPoint;
            this.startPoint = myStartPoint;*/
            this.myPoints = myPoints;
            
        }
        public static TimeSpan GetTimeForGoingTo(double distance, double speed/*, bool betweenPoints = false, double sp = 0*/)///////////////////
        {
            int seconds = (int)(distance / (speed / 3.6));
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts;
        }
        public static TimeSpan GetTimeForGoingTo(GeoCoords fromPos, GeoCoords toPos, double speed/*, bool betweenPoints = false*/)
        {
            //int distance = GoogleApi.GetWalkingDistance(fromPos, toPos);
            int distance = GeoCoords.Distance(fromPos, toPos);
            return GetTimeForGoingTo(distance, speed/*, betweenPoints*/);
        }





       
        public class IgnoringFragment
        {
            public readonly string stationCode, routeCode, fromStationCode;
            public IgnoringFragment(string stationCode, string routeCode, string fromStationCode)
            {
                this.stationCode = stationCode;
                this.routeCode = routeCode;
                this.fromStationCode = fromStationCode;
            }
            /*public static bool Contains(IEnumerable<IgnoringFragment> collection, string stationCode, string routeCode, string fromStationCode)
            {
                if (collection == null) return false;
                foreach (IgnoringFragment r in collection) if (r.routeCode == routeCode && r.stationCode == stationCode && r.fromStationCode == fromStationCode) return true;
                return false;
            }*/
        }
        public class IgnoringFragments : ICollection<IgnoringFragment>
        {
            private List<IgnoringFragment> fragments;
            public IgnoringFragments()
            {
                fragments = new List<IgnoringFragment>();
            }
            public IgnoringFragments(IEnumerable<IgnoringFragment> collection)
            {
                fragments = new List<IgnoringFragment>(collection);
            }
            public int Count
            {
                get
                {
                    return fragments.Count;
                }
            }
            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }
            public void Add(IgnoringFragment item)
            {
                fragments.Add(item);
            }
            public void Clear()
            {
                fragments.Clear();
            }
            public bool Contains(IgnoringFragment item)
            {
                return Contains(item.stationCode, item.routeCode, item.fromStationCode);
            }
            public bool Contains(string stationCode, string routeCode, string fromStationCode)
            {
                foreach (IgnoringFragment r in fragments) if (r.routeCode == routeCode && r.stationCode == stationCode && r.fromStationCode == fromStationCode) return true;
                return false;
            }
            public void CopyTo(IgnoringFragment[] array, int arrayIndex)
            {
                fragments.CopyTo(array, arrayIndex);
            }
            public IEnumerator<IgnoringFragment> GetEnumerator()
            {
                return fragments.GetEnumerator();
            }
            public bool Remove(IgnoringFragment item)
            {
                return fragments.Remove(item);
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        public static OptimalRoute GetBestOptimalRoute(GeoCoords nowPos, GeoCoords needPos, DateTime time, Dictionary<Priority, double> priorities, IEnumerable<RouteType> types = null, double speed = 5, double dopTimeMinutes = 2, double percentTotalTime = 1, double percentTotalGoingTime = 1, double percentTotalTransportChangingCount = 1)
        {
            OptimalRoutesCollection tmp = FindOptimalRoutes(nowPos, needPos, time, priorities, types, speed, dopTimeMinutes , percentTotalTime, percentTotalGoingTime, percentTotalTransportChangingCount);
            return tmp.Customize(percentTotalTime, percentTotalGoingTime, percentTotalTransportChangingCount);
        }
        public static OptimalRoutesCollection FindOptimalRoutes(GeoCoords nowPos, GeoCoords needPos, DateTime time, Dictionary<Priority, double> priorities, IEnumerable<RouteType> types = null, double speed = 5, double dopTimeMinutes = 2, double percentTotalTime = 1, double percentTotalGoingTime = 1, double percentTotalTransportChangingCount = 1)
        {
            int n1 = 1, n2 = 1;
            if (priorities == null || priorities.Count == 0) throw new Exception("Не заданы приоритеты.");
            
            OptimalRoutesCollection findedOptimalRoutes = new OptimalRoutesCollection();
            //DateTime ttt1 = DateTime.Now;
            findedOptimalRoutes.Add(new OptimalRoute(nowPos, needPos, time, types, speed, dopTimeMinutes));
            //MessageBox.Show((DateTime.Now - ttt1).TotalMilliseconds.ToString());

            //return findedOptimalRoutes;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1

            List<Route> ignoringRoutes = new List<Route>();
            

            Stack<Priority> myPriorities = new Stack<Priority>(priorities.Keys);
            //for (Priority currentPriority = myPriorities.Pop(); myPriorities.Count != 0; currentPriority = myPriorities.Pop())
            //{
            Priority currentPriority = myPriorities.Pop();
            //double ddd = priorities[currentPriority];
            double ddd = 0.25;// 0.95;// 0.85; // Порог эффективности маршрута по выбранному критерию.
            if (ddd > 1 || ddd < 0.25) throw new Exception("Priority must been from [0.25; 1]");
            //List<IgnoringFragment> globalIgnoringFragments = new List<IgnoringFragment>();
            IgnoringFragments ignoringFragments = new IgnoringFragments();//!!!!!!!!!!!!!!
            //List<IgnoringFragment> currentIgnoringFragments;

            //Point myStartPoint = findedOptimalRoutes[0].startPoint;

            for (OptimalRoute selectedOptimalRoute = findedOptimalRoutes[0]; selectedOptimalRoute != null; selectedOptimalRoute.Visited(), selectedOptimalRoute = SelectOptimalRouteWithMinimalMark(findedOptimalRoutes, Priority.MinimalTime))
            {
                ignoringRoutes = new List<Route>();//!!!!!!!!!!!1111111111
                // Проходим по всем ребрам выбранного пути и строим новые маршруты при удалении ребер:
                for (Points.Point tmpP = selectedOptimalRoute.myPoints.finalPoint; tmpP.Previous != null; tmpP = tmpP.Previous)
                {
                    if (tmpP.Route != null && !ignoringRoutes.Contains(tmpP.Route)) ignoringRoutes.Add(tmpP.Route);


                    ////// Игнорируемое "ребро":
                    ////IgnoringFragment tmpIgnFragm = new IgnoringFragment(tmpP.StationCode, tmpP.RouteCode, tmpP.PreviousStationCode);

                    //////!!!!!!!!!!!!!!!!!//
                    ////ignoringFragments = new IgnoringFragments(selectedOptimalRoute.myIgnoringFragments);//!!!!

                    ////if (ignoringFragments.Contains(tmpIgnFragm)) continue;
                    ////ignoringFragments.Add(tmpIgnFragm);

                    ////Points myPoints = null;
                    //////Point myFinishPoint = null;
                    ////OptimalRoute tmpOptimalRoute = null;

                    //////myPoints = selectedOptimalRoute.MyPoints.Downgrade(ignoringFragments);
                    //////myFinishPoint = selectedOptimalRoute.finalPoint.Downgrade(ignoringFragments);

                    //////myPoints = tmpP.Previous.CurrentGraph;

                    ////// Строим новый маршрут, избегая указанные ребра:
                    ////tmpOptimalRoute = new OptimalRoute(nowPos, needPos, time, types, speed, dopTimeMinutes, ignoringList: ignoringFragments/*,/*myStartPoint,*/ /*myPoints*//*, myFinishPoint*/);


                    ////n2++;
                    ////if (tmpOptimalRoute.TotalTime.TotalSeconds <= findedOptimalRoutes[0].TotalTime.TotalSeconds / ddd)
                    ////{
                    ////    string tmpJSON = JsonConvert.SerializeObject(tmpOptimalRoute.points);
                    ////    bool ok = false;
                    ////    foreach (OptimalRoute opt in findedOptimalRoutes)
                    ////    {
                    ////        if (JsonConvert.SerializeObject(opt.points) == tmpJSON)
                    ////        {
                    ////            ok = true;
                    ////            break;
                    ////        }
                    ////    }
                    ////    if (ok) continue;
                    ////    findedOptimalRoutes.Add(tmpOptimalRoute);
                    ////    n1++;
                    ////}
                }
                foreach (Route r in ignoringRoutes)
                {
                    if (selectedOptimalRoute.ignoringRoutes.Contains(r)) continue;
                    List<Route> ignoringRoutesAdd = new List<Route>(selectedOptimalRoute.ignoringRoutes);
                    ignoringRoutesAdd.Add(r);
                    OptimalRoute tmpOptimalRoute = new OptimalRoute(nowPos, needPos, time, types, speed, dopTimeMinutes, ignoringRoutesAdd: ignoringRoutesAdd);
                    
                    n2++;
                    if (tmpOptimalRoute.TotalTime.TotalSeconds <= findedOptimalRoutes[0].TotalTime.TotalSeconds / ddd)
                    {
                        string tmpJSON = JsonConvert.SerializeObject(tmpOptimalRoute.points);
                        bool ok = false;
                        foreach (OptimalRoute opt in findedOptimalRoutes)
                        {
                            if (JsonConvert.SerializeObject(opt.points) == tmpJSON)
                            {
                                ok = true;
                                break;
                            }
                        }
                        if (ok) continue;
                        findedOptimalRoutes.Add(tmpOptimalRoute);
                        n1++;
                    }
                }
            }
            //MessageBox.Show(n1 + " from " + n2);
            return findedOptimalRoutes;
        }
        public static void CancelVisitedMarksInOptimalRoutes(OptimalRoutesCollection optimalRoutes)
        {
            foreach (OptimalRoute r in optimalRoutes)
            {
                r.CancelVisitedMark();
            }
        }
    }


    public class OptimalRoutesCollection : ICollection<OptimalRoute>
    {
        public List<OptimalWay> GetOptimalWays()
        {
            List<OptimalWay> result = new List<OptimalWay>();
            foreach (OptimalRoute r in this)
                result.Add(new OptimalWay(r));
            return result;
        }
        public OptimalRoute Customize(double percentTotalTime = 1, double percentTotalGoingTime = 1, double percentTotalTransportChangingCount = 1)
        {
            TimeSpan minimalTime = this[0].TotalTime;
            TimeSpan minimalGoingTime = this[0].TotalGoingTime;
            int minimalTransportChangingCount = this[0].TotalTransportChangingCount;

            foreach (OptimalRoute r in this)
            {
                if (r.TotalTime < minimalTime) minimalTime = r.TotalTime;
                if (r.TotalGoingTime < minimalGoingTime) minimalGoingTime = r.TotalGoingTime;
                if (r.TotalTransportChangingCount < minimalTransportChangingCount) minimalTransportChangingCount = r.TotalTransportChangingCount;
            }
            if (minimalTransportChangingCount == 0) minimalTransportChangingCount = 1;
            
            OptimalRoute optRouteWithMaximalRank = this[0];
            double tmpTransportChangingCountEffictivity = this[0].TotalTransportChangingCount == 0 ? 100 : Math.Round(100 * (double)minimalTransportChangingCount / this[0].TotalTransportChangingCount);
            double rank = percentTotalTime * minimalTime.TotalSeconds / this[0].TotalTime.TotalSeconds
                + percentTotalGoingTime * minimalGoingTime.TotalSeconds / this[0].TotalGoingTime.TotalSeconds
                + percentTotalTransportChangingCount * tmpTransportChangingCountEffictivity;

            foreach (OptimalRoute r in this)
            {
                tmpTransportChangingCountEffictivity = r.TotalTransportChangingCount == 0 ? 100 : Math.Round(100 * (double)minimalTransportChangingCount / r.TotalTransportChangingCount);

                double tmp_rank = percentTotalTime * minimalTime.TotalSeconds / r.TotalTime.TotalSeconds
                + percentTotalGoingTime * minimalGoingTime.TotalSeconds / r.TotalGoingTime.TotalSeconds
                + percentTotalTransportChangingCount * tmpTransportChangingCountEffictivity;
                if (tmp_rank > rank)
                {
                    rank = tmp_rank;
                    optRouteWithMaximalRank = r;
                }
            }

            double tmpTransportChangingCount = optRouteWithMaximalRank.TotalTransportChangingCount;
            double transportChangingCountEffictivity = tmpTransportChangingCount == 0 ? 100 : Math.Round(100 * minimalTransportChangingCount / tmpTransportChangingCount);
            /*MessageBox.Show("Total time effictivity: " + Math.Round(100 * minimalTime.TotalSeconds / optRouteWithMaximalRank.TotalTime.TotalSeconds, 1) + "%.\n"
                + "Total going time effictivity: " + Math.Round(100 * minimalGoingTime.TotalSeconds / optRouteWithMaximalRank.TotalGoingTime.TotalSeconds, 1) + "%.\n"
                + "Transport changing count effictivity: " + transportChangingCountEffictivity + " %.");*/
            return optRouteWithMaximalRank;// findedOptimalRoutes[0];
                                           //CancelVisitedMarksInOptimalRoutes(findedOptimalRoutes);
                                           //}
        }
        private List<OptimalRoute> optimalRoutes;
        public OptimalRoutesCollection()
        {
            optimalRoutes = new List<OptimalRoute>();
        }
        public int Count
        {
            get
            {
                return optimalRoutes.Count;
            }
        }
        public OptimalRoute this[int index]
        {
            get
            {
                return optimalRoutes[index];
            }
        }
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        public void Add(OptimalRoute item)
        {
            optimalRoutes.Add(item);
        }
        public void Clear()
        {
            optimalRoutes.Clear();
        }
        public bool Contains(OptimalRoute item)
        {
            return optimalRoutes.Contains(item);
        }
        public void CopyTo(OptimalRoute[] array, int arrayIndex)
        {
            optimalRoutes.CopyTo(array, arrayIndex);
        }
        public IEnumerator<OptimalRoute> GetEnumerator()
        {
            return optimalRoutes.GetEnumerator();
        }
        public bool Remove(OptimalRoute item)
        {
            return optimalRoutes.Remove(item);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return optimalRoutes.GetEnumerator();
        }
    }
    public class OptimalWay
    {
        public readonly int totalTimeSeconds;
        public readonly int totalGoingTimeSeconds;
        public readonly int totalTransportChangingCount;
        public List<WayPoint> points;
        public OptimalWay(OptimalRoute optimalRoute)
        {
            totalTimeSeconds = (int)optimalRoute.TotalTime.TotalSeconds;
            totalGoingTimeSeconds = (int)optimalRoute.TotalGoingTime.TotalSeconds;
            totalTransportChangingCount = optimalRoute.TotalTransportChangingCount;
            points = new List<WayPoint>();
            OptimalRoute.Points optRoutePoints = optimalRoute.MyPoints;
            points.Add(new WayPoint(optRoutePoints.startPoint.TotalTime, optRoutePoints.startPoint.Station, optRoutePoints.startPoint.Route, optRoutePoints.startPoint.Coords));

            List<WayPoint> tmp = new List<WayPoint>();
            for(OptimalRoute.Points.Point tmpP = optimalRoute.MyPoints.finalPoint; tmpP.Previous != null; tmpP = tmpP.Previous)
            {
                tmp.Add(new WayPoint(tmpP.TotalTime, tmpP.Station, tmpP.Route, tmpP.Coords));
            }
            tmp.Reverse();
            points.AddRange(tmp);

            /*foreach (OptimalRoute.Points.Point p in optRoutePoints)
            {
                points.Add(new WayPoint(p.Station, p.Route, p.Coords));
            }
            points.Add(new WayPoint(optRoutePoints.finalPoint.Station, optRoutePoints.finalPoint.Route, optRoutePoints.finalPoint.Coords));*/
        }
        public class WayPoint
        {
            public readonly Station station;
            public readonly Route route;
            public readonly OptimalRoute.GeoCoords coords;
            public readonly string time;
            public WayPoint(TimeSpan time, Station station, Route route, OptimalRoute.GeoCoords coords)
            {
                /*try
                {*/
                    this.time = time.ToString();
                    this.station = station == null ? null : new Station(station.hashcode, station.nameRus, station.nameEn, station.nameBy, (int)(10000 * station.lat), (int)(10000 * station.lng), null, station.name); //station;
                    this.route = route == null ? null : new Route(route.hashcode, JsonConvert.SerializeObject(new string[] { route.from, route.to }), JsonConvert.SerializeObject(new string[] { route.from, route.to }), JsonConvert.SerializeObject(new string[] { route.from, route.to }), JsonConvert.SerializeObject(new string[] { route.from, route.to }), null, route.number, route.type, route.owner); //route;
                    this.coords = coords;
                /*}
                catch (Exception ex)
                {

                }*/
            }
        }
    }
}


/*
//////points = new List<Point>();
            //////myStartPoint = new Point(TimeSpan.FromSeconds(0), nowPos);

            //////List<Stack<Point>> TMP_MY_LIST = new List<Stack<Point>>();
            //////Stack<Point> TMP_QUEUE = new Stack<Point>();
            //////TMP_QUEUE.Push(myStartPoint);
            //////TMP_MY_LIST.Add(TMP_QUEUE);
            
            //////foreach (Station st in stationsList)
            //////{
            //////    Point add = new Point(TimeSpan.FromDays(25000), st);
            //////    add.tryUpdate(GetTimeForGoingTo(nowPos, st.Coords), myStartPoint);
            //////    points.Add(add);

//////    TMP_QUEUE = new Stack<Point>();
//////    TMP_QUEUE.Push(add);
//////    TMP_MY_LIST.Add(TMP_QUEUE);
//////}
//////myFinishPoint = new Point(TimeSpan.FromDays(25000), needPos);
//////myFinishPoint.tryUpdate(GetTimeForGoingTo(nowPos, needPos), myStartPoint);
//////points.Add(myFinishPoint);

//////for (;;)//...
//////{
//////    List<Stack<Point>> NEW_ITERATION_LIST_STACK = new List<Stack<Point>>();
//////    foreach (Stack<Point> qq in TMP_MY_LIST)
//////    {
//////        Point lastPointInStack = qq.Peek();

//////        // Момент, когда мы прибудем на остановку:
//////        DateTime momentWhenComingToStation = time + lastPointInStack.TotalTime;

//////        GeoCoords selectedPointCoords = lastPointInStack.Coords;

//////        Station s = lastPointInStack.Station;
//////        if (s != null)
//////        {
//////            List<Route> rr = Database.GetRoutesOnStation(s.hashcode, canReadDataFromLocalCopy: true);

//////            foreach (Route r in rr)
//////            {
//////                // Загружаем расписание:
//////                Timetable table = Database.GetTimetable(s.hashcode, r.hashcode, databaseMysqlConnection, canReadDataFromLocalCopy: true);

//////                // Код остановки, на которую попадем на данном транспорте:
//////                string nextCode = r.getNextStationCodeAfter(s.hashcode);

//////                if (nextCode != null) // Если остановка не является конечной, то:
//////                {
//////                    if (table.type == TableType.table) // Если это точное расписание, то:
//////                    {
//////                        // Минимальный начальный момент, с который можно начинать ожидать посадку:
//////                        DateTime momentWhenAskingForGoing = momentWhenComingToStation;

//////                        // Резервируем дополнительное время, если будем пересаживаться на другой маршрут:
//////                        if (lastPointInStack.RouteCode == null || lastPointInStack.RouteCode != r.hashcode) momentWhenAskingForGoing += reservedTime;

//////                        // Подсчитываем, сколько будем ожидать этот транспорт на остановке:
//////                        TimeSpan waitingTime = table.FindTimeAfter(momentWhenAskingForGoing);

//////                        // Момент, когда мы сядем в транспорт:
//////                        DateTime momentWhenSitInTransport = momentWhenAskingForGoing + waitingTime;

//////                        // Следующая остановка у данного транспорта:
//////                        Station next = Database.GetStationByHashcode(nextCode, databaseMysqlConnection, canReadDataFromLocalCopy: true);

//////                        // И соответствующее расписание на этой остановке:
//////                        Timetable tbl = Database.GetTimetable(next.hashcode, r.hashcode, databaseMysqlConnection, canReadDataFromLocalCopy: true);

//////                        // (сколько будем ехать до следующей остановки):
//////                        TimeSpan goingOnTransportTime = tbl.FindTimeAfter(momentWhenSitInTransport);

//////                        // Метка времени:
//////                        TimeSpan onNextPointTotalTime = momentWhenSitInTransport - momentWhenComingToStation + goingOnTransportTime + lastPointInStack.TotalTime;

//////                        //TimeSpan timeForGoingFromNextPointToFinal = GetTimeForGoingTo(next.Coords, myFinishPoint.Coords);

//////                        if (onNextPointTotalTime < myFinishPoint.TotalTime + reservedTime + reservedTime + reservedTime + reservedTime + reservedTime)//...
//////                        {
//////                            Stack<Point> NEW_TMP_STACK = new Stack<Point>(new Stack<Point>(qq));
//////                            Point addingPoint = new Point(onNextPointTotalTime, selectedPointCoords, s, r);
//////                            NEW_TMP_STACK.Push(addingPoint);
//////                            NEW_ITERATION_LIST_STACK.Add(NEW_TMP_STACK);
//////                        }

//////                        //////////bool contains = false;
//////                        //////////foreach (Point ppp in points) if (ppp.Coords == next.Coords)
//////                        //////////    {
//////                        //////////        ppp.tryUpdate(onNextPointTotalTime, lastPointInStack, s, r);
//////                        //////////        contains = true;
//////                        //////////        break;
//////                        //////////    }
//////                        //////////if (!contains) // (если вершина "неизвестна")
//////                        //////////    points.Add(new Point(onNextPointTotalTime, next, s, r));


//////                    }
//////                    else if (table.type == TableType.periodic)
//////                    {
//////                        // ...........................
//////                    }
//////                }
//////            }
//////        }

//////        // Попробуем пройти пешком до других "вершин":
//////        foreach (Point p in points) if (p != lastPointInStack)
//////            {
//////                int distanceToSelectedPoint = GeoCoords.Distance(selectedPointCoords, p.Coords);

//////                TimeSpan goingTime = GetTimeForGoingTo(distanceToSelectedPoint);
//////                TimeSpan newTime = lastPointInStack.TotalTime + goingTime;
//////                if (p != myFinishPoint) newTime += reservedTime;

//////                //TimeSpan timeForGoingFromNextPointToFinal = GetTimeForGoingTo(p.Coords, myFinishPoint.Coords);
//////                                                        //////////p.tryUpdate(newTime, lastPointInStack, s);
//////                if (newTime < myFinishPoint.TotalTime + reservedTime + reservedTime + reservedTime + reservedTime + reservedTime)//...
//////                {
//////                    Stack<Point> NEW_TMP_STACK = new Stack<Point>(new Stack<Point>(qq));
//////                    Point addingPoint = new Point(newTime, selectedPointCoords, s, null);
//////                    NEW_TMP_STACK.Push(addingPoint);
//////                    NEW_ITERATION_LIST_STACK.Add(NEW_TMP_STACK);
//////                }

//////            }



//////        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//////    }
//////    TMP_MY_LIST = NEW_ITERATION_LIST_STACK;
//////}

//}


////        }
////    }
////} 
*/