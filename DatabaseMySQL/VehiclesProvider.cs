using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransportClasses;

namespace TransportClasses
{

    public static class GlobalVehiclesProvider
    {
        private static int counter = 0;
        public static string NextHashcode
        {
            get
            {
                return "V" + Interlocked.Increment(ref counter);
            }
        }
        /// <summary>
        /// Все транспортные средства.
        /// </summary>
        public static IEnumerable<Vehicle> Vehicles
        {
            get
            {
                List<Vehicle> result = new List<Vehicle>();
                foreach (ExtendedVehiclesProvider p in vehiclesProviders) result.AddRange(p.Vehicles);
                return result;
            }
        }
        private static int updatingPeriodMilliseconds = 5000;
        public static int UpdatingPeriodMilliseconds
        {
            get
            {
                return updatingPeriodMilliseconds;
            }
        }
        public static void RemoveVehicle(Vehicle vehicle)
        {
            vehicle.vehiclesProvider.RemoveVehicle(vehicle);
        }
        private class ExtendedVehiclesProvider
        {
            public IEnumerable<Vehicle> Vehicles
            {
                get
                {
                    return provider.Vehicles;
                }
            }
            private VehiclesProvider provider;
            private Thread thread;
            private bool enabled = false;
            private void Enable()
            {
                if (!enabled)
                {
                    enabled = true;
                    thread = new Thread(delegate ()
                    {
                        while (true)
                        {
                            provider.Update();
                            Thread.Sleep(GlobalVehiclesProvider.updatingPeriodMilliseconds);
                        }
                    });
                    thread.Start();
                }
            }
            private void Disable()
            {
                if (enabled)
                {
                    enabled = false;
                    thread.Abort();
                    thread = null;
                    GC.Collect();
                }
            }
            public ExtendedVehiclesProvider(VehiclesProvider provider)
            {
                this.provider = provider;
                Enable();
            }
            public override bool Equals(object provider)
            {
                return provider is ExtendedVehiclesProvider && (provider as ExtendedVehiclesProvider).provider.GetType() == this.provider.GetType();
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }
        private static List<ExtendedVehiclesProvider> vehiclesProviders = new List<ExtendedVehiclesProvider>();
        public static void Load(VehiclesProvider provider)
        {
            if (provider == null) throw new ArgumentNullException();
            foreach (ExtendedVehiclesProvider p in vehiclesProviders) if (p.Equals(provider)) throw new ArgumentException();
            vehiclesProviders.Add(new ExtendedVehiclesProvider(provider));
        }
    }

    /// <summary>
    /// Поставщик GPS-координат транспорта.
    /// </summary>
    public abstract class VehiclesProvider
    {
        protected class Group
        {
            public readonly OptimalRoute.GeoCoords coords;
            public readonly DateTime moment;
            public Group(OptimalRoute.GeoCoords coords, DateTime moment)
            {
                this.coords = coords;
                this.moment = moment;
            }
        }
        protected ConcurrentDictionary<Vehicle, Group> vehiclesInfo;
        public void RemoveVehicle(Vehicle vehicle)
        {
            Route route = vehicle.route;
            if (route != null) route.vehicles.Remove(vehicle);
            Group tmpUnused;
            vehiclesInfo.TryRemove(vehicle, out tmpUnused);
        }
        /// <summary>
        /// Возвращает список транспортных средств поставщика GPS-координат.
        /// </summary>
        public IEnumerable<Vehicle> Vehicles
        {
            get
            {
                return vehiclesInfo.Keys;
            }
        }
        /// <summary>
        /// Возвращает список транспортных средств поставщика GPS-координат, следующих по указанному маршруту.
        /// </summary>
        /// <param name="route">Маршрут.</param>
        public IEnumerable<Vehicle> GetVehicles(Route route)
        {
            if (route == null) throw new NullReferenceException();
            IEnumerable<Vehicle> currentVehicles = GetVehicles(new Route[] { route });
            return currentVehicles;
        }
        /// <summary>
        /// Возвращает список транспортных средств поставщика GPS-координат, следующих по указанным маршрутам.
        /// </summary>
        /// <param name="route">Маршруты.</param>
        public IEnumerable<Vehicle> GetVehicles(IEnumerable<Route> routes)
        {
            List<Vehicle> result = new List<Vehicle>();
            foreach (Vehicle vehicle in Vehicles) if (routes.Contains(vehicle.route)) result.Add(vehicle);
            return result;
        }
        /// <summary>
        /// Возвращает последние известные GPS-координаты транспортного средства.
        /// </summary>
        /// <param name="vehicle">Транспортное средство.</param>
        /// <param name="lastUpdatingMoment">Переменная, в которую записывается последний известный момент времени.</param>
        public OptimalRoute.GeoCoords GetCurrentPosition(Vehicle vehicle, out DateTime lastUpdatingMoment)
        {
            Group gr = null;
            if (vehiclesInfo.TryGetValue(vehicle, out gr))
            {
                lastUpdatingMoment = gr.moment;
                return gr.coords;
            }
            lastUpdatingMoment = new DateTime(0);
            return null;
        }
        /// <summary>
        /// Возвращает маршруты, по которым следуют транспортные средства поставщика GPS-координат.
        /// </summary>
        public abstract IEnumerable<Route> GetRoutes();
        /// <summary>
        /// Возвращает станции, через которые следуют транспортные средства поставщика GPS-координат.
        /// </summary>
        public abstract IEnumerable<Station> GetStations();
        /// <summary>
        /// Обновляет информацию о каждом транспортном средстве.
        /// </summary>
        public abstract void Update();
        /// <summary>
        /// Возвращает GPS-трек маршрута.
        /// </summary>
        /// <param name="route">Маршрут.</param>
        public abstract IEnumerable<OptimalRoute.GeoCoords>[] GetRouteNodes(Route route);
        public bool Equals(VehiclesProvider provider)
        {
            return provider.GetType() == this.GetType();
        }
    }
    



    public class UmniyTransportRfVehiclesProvider : VehiclesProvider
    {
        private static ConcurrentBag<Station> stations = null;
        protected static List<ExtendedStation>[] extendedStations = null;
        private dynamic returnedStations = null;
        private static ConcurrentBag<Route> routes = null;
        protected static List<ExtendedRoute>[] extendedRoutes = null;
        private dynamic returnedRoutes = null;
        private static List<Station> allStations = null;
        private static List<Route> allRoutes = null;
        protected static ConcurrentDictionary<string, Vehicle> vehicles = null;//int
        protected static List<ExtendedVehicle>[] extendedVehicles = null;

        protected class ApiRequest
        {
            public readonly string routesRequest, stationsRequest, markersRequest, routesNodesRequest;
            public ApiRequest(string stationsRequest, string routesRequest, string markersRequest, string routesNodesRequest)
            {
                this.markersRequest = markersRequest;
                this.routesNodesRequest = routesNodesRequest;
                this.routesRequest = routesRequest;
                this.stationsRequest = stationsRequest;
            }
            public static readonly ApiRequest[] apiRequests = new ApiRequest[]
            {
                new ApiRequest(//ok
                    "http://bus62.ru/grodno/php/getStations.php?city=grodno",
                    "http://bus62.ru/grodno/php/getRoutes.php?city=grodno",
                    "http://bus62.ru/grodno/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=grodno&rids=",
                    "http://bus62.ru/grodno/php/getRouteNodes.php?city=grodno&type=0&rid="
                    ),
                new ApiRequest(//ok
                    "http://bus62.ru/php/getStations.php?city=ryazan",
                    "http://bus62.ru/php/getRoutes.php?city=ryazan",
                    "http://bus62.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=ryazan&rids=",
                    "http://bus62.ru/php/getRouteNodes.php?city=ryazan&type=0&rid="
                    ),
                new ApiRequest(//ok
                    "http://appp29.ru/php/getStations.php?city=arhangelsk",
                    "http://appp29.ru/php/getRoutes.php?city=arhangelsk",
                    "http://appp29.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=arhangelsk&rids=",
                    "http://appp29.ru/php/getRouteNodes.php?city=arhangelsk&type=0&rid="
                    ),
                new ApiRequest(//ok
                    "http://bus23.ru/php/getStations.php?city=sochi",
                    "http://bus23.ru/php/getRoutes.php?city=sochi",
                    "http://bus23.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=sochi&rids=",
                    "http://bus23.ru/php/getRouteNodes.php?city=sochi&type=0&rid="
                    ),
                //new ApiRequest(//ok
                //    "http://spopat-auto.ru/php/getStations.php?city=surgut",
                //    "http://spopat-auto.ru/php/getRoutes.php?city=surgut",
                //    "http://spopat-auto.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=surgut&rids=",
                //    "http://spopat-auto.ru/php/getRouteNodes.php?city=surgut&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://bus62.ru/karaganda/php/getStations.php?city=karaganda",
                //    "http://bus62.ru/karaganda/php/getRoutes.php?city=karaganda",
                //    "http://bus62.ru/karaganda/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=karaganda&rids=",
                //    "http://bus62.ru/karaganda/php/getRouteNodes.php?city=karaganda&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://bus68.ru/php/getStations.php?city=tambov",
                //    "http://bus68.ru/php/getRoutes.php?city=tambov",
                //    "http://bus68.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=tambov&rids=",
                //    "http://bus68.ru/php/getRouteNodes.php?city=tambov&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://bus125.ru/php/getStations.php?city=vladivostok",
                //    "http://bus125.ru/php/getRoutes.php?city=vladivostok",
                //    "http://bus125.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=vladivostok&rids=",
                //    "http://bus125.ru/php/getRouteNodes.php?city=vladivostok&type=0&rid="
                //    ),
                ///*new ApiRequest(
                //    "http://www.58bus.ru/php/getStations.php?city=penza",
                //    "http://www.58bus.ru/php/getRoutes.php?city=penza",
                //    "http://www.58bus.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=penza&rids=",
                //    "http://www.58bus.ru/php/getRouteNodes.php?city=penza&type=0&rid="
                //    ),*/
                //new ApiRequest(//ok
                //    "http://bus03.ru/php/getStations.php?city=ulanude",
                //    "http://bus03.ru/php/getRoutes.php?city=ulanude",
                //    "http://bus03.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=ulanude&rids=",
                //    "http://bus03.ru/php/getRouteNodes.php?city=ulanude&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://gis.irkobl.ru/ustilimskbus/php/getStations.php?city=ust_ilimsk",
                //    "http://gis.irkobl.ru/ustilimskbus/php/getRoutes.php?city=ust_ilimsk",
                //    "http://gis.irkobl.ru/ustilimskbus/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=ust_ilimsk&rids=",
                //    "http://gis.irkobl.ru/ustilimskbus/php/getRouteNodes.php?city=ust_ilimsk&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://bus33.su/php/getStations.php?city=vladimir",
                //    "http://bus33.su/php/getRoutes.php?city=vladimir",
                //    "http://bus33.su/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=vladimir&rids=",
                //    "http://bus33.su/php/getRouteNodes.php?city=vladimir&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://saratov.bus64.ru/php/getStations.php?city=saratov",
                //    "http://saratov.bus64.ru/php/getRoutes.php?city=saratov",
                //    "http://saratov.bus64.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=saratov&rids=",
                //    "http://saratov.bus64.ru/php/getRouteNodes.php?city=saratov&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://troll51.ru/php/getStations.php?city=murmansk",
                //    "http://troll51.ru/php/getRoutes.php?city=murmansk",
                //    "http://troll51.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=murmansk&rids=",
                //    "http://troll51.ru/php/getRouteNodes.php?city=murmansk&type=0&rid="
                //    ),
                ///*new ApiRequest(
                //    "http://buscheb.ru/php/getStations.php?city=cheboksari",
                //    "http://buscheb.ru/php/getRoutes.php?city=cheboksari",
                //    "http://buscheb.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=cheboksari&rids=",
                //    "http://buscheb.ru/php/getRouteNodes.php?city=cheboksari&type=0&rid="
                //    )*/
                //new ApiRequest(//ok
                //    "http://bus62.ru/tomsk/php/getStations.php?city=tomsk",
                //    "http://bus62.ru/tomsk/php/getRoutes.php?city=tomsk",
                //    "http://bus62.ru/tomsk/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=tomsk&rids=",
                //    "http://bus62.ru/tomsk/php/getRouteNodes.php?city=tomsk&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://traffic22.ru/php/getStations.php?city=barnaul",
                //    "http://traffic22.ru/php/getRoutes.php?city=barnaul",
                //    "http://traffic22.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=barnaul&rids=",
                //    "http://traffic22.ru/php/getRouteNodes.php?city=barnaul&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://bus27.ru/php/getStations.php?city=khabarovsk",
                //    "http://bus27.ru/php/getRoutes.php?city=khabarovsk",
                //    "http://bus27.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=khabarovsk&rids=",
                //    "http://bus27.ru/php/getRouteNodes.php?city=khabarovsk&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://bus36.ru/php/getStations.php?city=voronezh",
                //    "http://bus36.ru/php/getRoutes.php?city=voronezh",
                //    "http://bus36.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=voronezh&rids=",
                //    "http://bus36.ru/php/getRouteNodes.php?city=voronezh&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://gis.irkobl.ru/angarskbus/php/getStations.php?city=angarsk",
                //    "http://gis.irkobl.ru/angarskbus/php/getRoutes.php?city=angarsk",
                //    "http://gis.irkobl.ru/angarskbus/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=angarsk&rids=",
                //    "http://gis.irkobl.ru/angarskbus/php/getRouteNodes.php?city=angarsk&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://bus62.ru/biysk/php/getStations.php?city=biysk",
                //    "http://bus62.ru/biysk/php/getRoutes.php?city=biysk",
                //    "http://bus62.ru/biysk/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=biysk&rids=",
                //    "http://bus62.ru/biysk/php/getRouteNodes.php?city=biysk&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://gis.irkobl.ru/bratskbus/php/getStations.php?city=bratsk",
                //    "http://gis.irkobl.ru/bratskbus/php/getRoutes.php?city=bratsk",
                //    "http://gis.irkobl.ru/bratskbus/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=bratsk&rids=",
                //    "http://gis.irkobl.ru/bratskbus/php/getRouteNodes.php?city=bratsk&type=0&rid="
                //    ),
                ///*new ApiRequest(
                //    "http://bus62.ru/novgorod/php/getStations.php?city=novgorod",
                //    "http://bus62.ru/novgorod/php/getRoutes.php?city=novgorod",
                //    "http://bus62.ru/novgorod/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=novgorod&rids=",
                //    "http://bus62.ru/novgorod/php/getRouteNodes.php?city=novgorod&type=0&rid="
                //    )*/
                ///*new ApiRequest(
                //    "http://gis.irkobl.ru/irkbus/php/getStations.php?city=irkutsk",
                //    "http://gis.irkobl.ru/irkbus/php/getRoutes.php?city=irkutsk",
                //    "http://gis.irkobl.ru/irkbus/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=irkutsk&rids=",
                //    "http://gis.irkobl.ru/irkbus/php/getRouteNodes.php?city=irkutsk&type=0&rid="
                //    ),*/
                //new ApiRequest(
                //    "http://bus44.ru/php/getStations.php?city=kostroma",
                //    "http://bus44.ru/php/getRoutes.php?city=kostroma",
                //    "http://bus44.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=kostroma&rids=",
                //    "http://bus44.ru/php/getRouteNodes.php?city=kostroma&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://bus46.ru/php/getStations.php?city=kursk",
                //    "http://bus46.ru/php/getRoutes.php?city=kursk",
                //    "http://bus46.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=kursk&rids=",
                //    "http://bus46.ru/php/getRouteNodes.php?city=kursk&type=0&rid="
                //    ),
                //new ApiRequest(//ok
                //    "http://bus57.ru/php/getStations.php?city=orel",
                //    "http://bus57.ru/php/getRoutes.php?city=orel",
                //    "http://bus57.ru/php/getVehiclesMarkers.php?lat0=0&lng0=0&lat1=90&lng1=180&curk=0&city=orel&rids=",
                //    "http://bus57.ru/php/getRouteNodes.php?city=orel&type=0&rid="
                //    )
            };
        }
        protected class ExtendedStation
        {
            //{"id":546,"name":"220-е управление","descr":"Улица Репина","lat":53659093,"lng":23790064,"type":"0"}
            public readonly Station station;
            public readonly int id;
            public readonly double distanceError;
            public readonly string name;
            public readonly string descr;
            public readonly OptimalRoute.GeoCoords coords;
            public ExtendedStation(dynamic returnedStation)
            {
                id = (int)returnedStation.id;
                name = (string)returnedStation.name;
                descr = (string)returnedStation.descr;
                coords = new OptimalRoute.GeoCoords(Math.Round((double)returnedStation.lat / 1000000, 4), Math.Round((double)returnedStation.lng / 1000000, 4));
                double findedMinDistance = 1000000000;
                Station candidate = null;
                foreach (Station s in allStations)
                {
                    double tmpDist = OptimalRoute.GeoCoords.Distance(coords, s.Coords);
                    if (tmpDist < findedMinDistance)
                    {
                        findedMinDistance = tmpDist;
                        candidate = s;
                    }
                }
                if (candidate != null && findedMinDistance < 50)
                {
                    station = candidate;
                    distanceError = findedMinDistance;
                    if (!stations.Contains(candidate)) stations.Add(candidate);
                }
                else
                {
                    //...
                }
            }
        }
        protected class ExtendedRoute
        {
            //{"id":159,"name":"001т_537_532_528_693","type":"М","num":"1т","fromst":"КСМ","fromstid":537,"tost":"Магазин","tostid":639}
            public readonly Route route;
            public readonly int idForvard;
            public readonly string nameForvard;
            public readonly int idReverse;
            public readonly string nameReverse;
            public readonly string type;
            public readonly string num;
            public readonly string fromstForvard;
            public readonly int fromstidForvard;
            public readonly string tostForvard;
            public readonly int tostidForvard;
            public readonly string fromstReverse;
            public readonly int fromstidReverse;
            public readonly string tostReverse;
            public readonly int tostidReverse;
            public ExtendedRoute(dynamic returnedRouteForvard, dynamic returnedRouteReverse, int index)
            {
                idForvard = (int)returnedRouteForvard.id;
                nameForvard = (string)returnedRouteForvard.name;
                idReverse = (int)returnedRouteReverse.id;
                nameReverse = (string)returnedRouteReverse.name;
                type = (string)returnedRouteReverse.type;
                num = (string)returnedRouteReverse.num;
                fromstForvard = (string)returnedRouteForvard.fromst;
                fromstidForvard = (int)returnedRouteForvard.fromstid;
                tostForvard = (string)returnedRouteForvard.tost;
                tostidForvard = (int)returnedRouteForvard.tostid;
                fromstReverse = (string)returnedRouteReverse.fromst;
                fromstidReverse = (int)returnedRouteReverse.fromstid;
                tostReverse = (string)returnedRouteReverse.tost;
                tostidReverse = (int)returnedRouteReverse.tostid;

                List<Route> candidatesForRoute = new List<Route>();
                List<Route> candidatesForRoute1 = new List<Route>();
                List<Route> candidatesForRoute2 = new List<Route>();
                List<Route> candidatesForRoute3 = new List<Route>();
                List<Route> candidatesForRoute4 = new List<Route>();
                foreach (ExtendedStation s in extendedStations[index])
                {
                    if (s.station != null && s.station.routes != null)
                    {
                        if (s.id == (int)returnedRouteForvard.fromstid) foreach (Route r in s.station.routes) candidatesForRoute1.Add(r);
                        if (s.id == (int)returnedRouteForvard.tostid) foreach (Route r in s.station.routes) candidatesForRoute2.Add(r);
                        if (s.id == (int)returnedRouteReverse.fromstid) foreach (Route r in s.station.routes) candidatesForRoute3.Add(r);
                        if (s.id == (int)returnedRouteReverse.tostid) foreach (Route r in s.station.routes) candidatesForRoute4.Add(r);
                    }
                }
                foreach (Route r in candidatesForRoute1) if (candidatesForRoute2.Contains(r) && candidatesForRoute3.Contains(r) && candidatesForRoute4.Contains(r) && !candidatesForRoute.Contains(r)) candidatesForRoute.Add(r);
                foreach (Route r in candidatesForRoute2) if (candidatesForRoute1.Contains(r) && candidatesForRoute3.Contains(r) && candidatesForRoute4.Contains(r) && !candidatesForRoute.Contains(r)) candidatesForRoute.Add(r);
                foreach (Route r in candidatesForRoute3) if (candidatesForRoute2.Contains(r) && candidatesForRoute1.Contains(r) && candidatesForRoute4.Contains(r) && !candidatesForRoute.Contains(r)) candidatesForRoute.Add(r);
                foreach (Route r in candidatesForRoute4) if (candidatesForRoute2.Contains(r) && candidatesForRoute3.Contains(r) && candidatesForRoute1.Contains(r) && !candidatesForRoute.Contains(r)) candidatesForRoute.Add(r);

                try
                { 
                    if (candidatesForRoute.Count == 0) route = null; // throw new Exception();
                    else if (candidatesForRoute.Count == 1) route = candidatesForRoute[0];
                    else
                    {
                    metka:
                        foreach (Route r in candidatesForRoute)
                        {
                            if (r.type == "bus" && type != "A" && type != "А" && r.number != num || r.type == "marsh" && type != "T" && type != "Т" && r.number != num.Remove('т'))
                            {
                                candidatesForRoute.Remove(r);
                                goto metka;
                            }
                        }
                        if (candidatesForRoute.Count != 1) route = null; //throw new Exception();
                        else route = candidatesForRoute[0];
                        if (route != null && !routes.Contains(route)) routes.Add(route);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        protected class ExtendedVehicle
        {
            public readonly string id;
            public readonly Vehicle vehicle;
            private DateTime lastUpdatingMomentOnThisBase;
            public DateTime LastUpdatingMomentOnThisBase
            {
                get
                {
                    return lastUpdatingMomentOnThisBase;
                }
            }
            public void Updated()
            {
                lastUpdatingMomentOnThisBase = DateTime.Now;
            }
            public ExtendedVehicle(dynamic returnedVehicle, VehiclesProvider vehiclesProvider, int index)
            {
                lastUpdatingMomentOnThisBase = DateTime.Now;
                OptimalRoute.GeoCoords coords = new OptimalRoute.GeoCoords(Math.Round((double)returnedVehicle.lat / 1000000, 4), Math.Round((double)returnedVehicle.lon / 1000000, 4));
                DateTime lastTimeMoment = DateTime.Parse((string)returnedVehicle.lasttime);
                double speed = (double)returnedVehicle.speed;
                int routeId = (int)returnedVehicle.rid;
                //Route route = null;
                foreach (ExtendedRoute r in extendedRoutes[index])
                {
                    if (r.idForvard == routeId || r.idReverse == routeId)
                    {
                        id = (string)returnedVehicle.id;//!!!!!!!!!!!! было int
                        bool ok = true;
                        foreach (ExtendedVehicle v in extendedVehicles[index])
                        {
                            if (v.id == id)
                            {
                                ok = false;
                                break;
                            }
                        }
                        if (ok)
                        {
                            Vehicle addingVehicle = new Vehicle(r.route, vehiclesProvider);
                            vehicle = addingVehicle;

                            vehicles.TryAdd(id, addingVehicle);
                        }
                        break;
                    }
                }
            }
        }
        public UmniyTransportRfVehiclesProvider()
        {
            vehiclesInfo = new ConcurrentDictionary<Vehicle, Group>();
            GetStations();
            GetRoutes();
            vehicles = new ConcurrentDictionary<string, Vehicle>();//int
            extendedVehicles = new List<ExtendedVehicle>[ApiRequest.apiRequests.Length];
            for (int i = 0; i < ApiRequest.apiRequests.Length; i++) extendedVehicles[i] = new List<ExtendedVehicle>();
            

            Update();
            
        }

        
        private IEnumerable<OptimalRoute.GeoCoords> GetRouteNodes(string routeId, int index)
        {
            List<OptimalRoute.GeoCoords> result = new List<OptimalRoute.GeoCoords>();
            dynamic returnedObject = Request.SendRequest(ApiRequest.apiRequests[index].routesNodesRequest + routeId);
            foreach (dynamic crds in returnedObject) result.Add(new OptimalRoute.GeoCoords(Math.Round((double)crds.lat / 1000000, 4), Math.Round((double)crds.lng / 1000000, 4)));
            return result;
        }
        public override IEnumerable<OptimalRoute.GeoCoords>[] GetRouteNodes(Route route)
        {
            IEnumerable<OptimalRoute.GeoCoords> result1 = null, result2 = null;
            for (int i = 0; i < ApiRequest.apiRequests.Length; i++)
            {
                foreach (ExtendedRoute r in extendedRoutes[i])
                {
                    if (r.route == route)
                    {
                        result1 = GetRouteNodes(r.idForvard.ToString(), i);
                        result2 = GetRouteNodes(r.idReverse.ToString(), i);
                        return new IEnumerable<OptimalRoute.GeoCoords>[] { result1, result2 };
                    }
                }
            }
            return null;
        }

        
        public override IEnumerable<Route> GetRoutes()
        {
            if (routes != null && routes.Count != 0) return routes;


            try
            {
                extendedRoutes = new List<ExtendedRoute>[ApiRequest.apiRequests.Length];
                //Database.Connect();!!!!!!!!!!!!!!!!!!!!!!!!
                allRoutes = Database.GetAllRoutes(canReadDataFromLocalCopy: true);
                for (int i = 0; i < ApiRequest.apiRequests.Length; i++)
                {
                    dynamic routesJSON = Request.SendRequest(ApiRequest.apiRequests[i].routesRequest);
                    if (routesJSON != null)
                    {
                        returnedRoutes = routesJSON;
                        routes = new ConcurrentBag<Route>();
                        extendedRoutes[i] = new List<ExtendedRoute>();

                        foreach (dynamic route in routesJSON)
                        {
                            if (route != null)
                            {
                                try
                                {
                                    int id = (int)route.id;
                                    if (id % 2 != 0)
                                    {
                                        foreach (dynamic route2 in routesJSON)
                                        {
                                            if ((int)route2.id == id + 1)
                                            {
                                                ExtendedRoute newExtendedRoute = new ExtendedRoute(route, route2, i);
                                                if (!extendedRoutes[i].Contains(newExtendedRoute)) extendedRoutes[i].Add(newExtendedRoute);

                                                break;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
                return routes;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                Database.Disconnect();
            }
            return null;
        }

        
        public override IEnumerable<Station> GetStations()
        {
            if (stations != null && stations.Count != 0) return stations;

            extendedStations = new List<ExtendedStation>[ApiRequest.apiRequests.Length];
            stations = new ConcurrentBag<Station>();

            try
            {
                //Database.Connect();!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                allStations = Database.GetAllStations(canReadDataFromLocalCopy: true);
                for (int i = 0; i < ApiRequest.apiRequests.Length; i++)
                {
                    dynamic stationsJSON = Request.SendRequest(ApiRequest.apiRequests[i].stationsRequest);
                    if (stationsJSON != null)
                    {
                        returnedStations = stationsJSON;
                        extendedStations[i] = new List<ExtendedStation>();
                            foreach (dynamic station in stationsJSON)
                            {
                                if (station != null)
                                {
                                    try
                                    {
                                        ExtendedStation newExtendedStation = new ExtendedStation(station);
                                        if (!extendedStations[i].Contains(newExtendedStation)) extendedStations[i].Add(newExtendedStation);
                                    }
                                    catch (Exception ex)
                                    {
                                        continue;
                                    }
                                }
                            }
                        
                    
                    }
                }
                return stations;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                Database.Disconnect();
            }
            return null;
        }

        
        public override void Update()
        {
            if (routes == null) throw new NullReferenceException();
            Thread[] tmpThreads = new Thread[ApiRequest.apiRequests.Length];
            for (int i = 0; i < ApiRequest.apiRequests.Length; i++)
            {
                List<ExtendedRoute> extendedRoutesCurrentPart = extendedRoutes[i];


                metka:
                foreach (ExtendedVehicle ev in extendedVehicles[i])
                {
                    if (DateTime.Now - ev.LastUpdatingMomentOnThisBase > TimeSpan.FromMinutes(3))
                    {
                        RemoveVehicle(ev.vehicle);
                        extendedVehicles[i].Remove(ev);
                        goto metka;
                    }
                }
                List<ExtendedVehicle> extendedVehiclesCurrentPart = extendedVehicles[i];


                ApiRequest currentApiRequest = ApiRequest.apiRequests[i];
                int index = i;
                tmpThreads[i] = new Thread(delegate() { 
                    StringBuilder tmp = new StringBuilder();
                    foreach (ExtendedRoute r in extendedRoutesCurrentPart) tmp.Append(r.idForvard + "-0," + r.idReverse + "-0,");
                    tmp.Remove(tmp.Length - 1, 1);
                    string str = tmp.ToString();
                    //str = "159-0,210-0,208-0,209-0,161-0,160-0,163-0,164-0,211-0,162-0,1-0,2-0,4-0,3-0,140-0,139-0,142-0,141-0,6-0,5-0,8-0,7-0,10-0,9-0,11-0,12-0,14-0,13-0,144-0,143-0,16-0,15-0,18-0,17-0,61-0,62-0,19-0,20-0,73-0,74-0,184-0,183-0,165-0,167-0,166-0,168-0,169-0,170-0,22-0,21-0,146-0,145-0,28-0,27-0,26-0,25-0,147-0,148-0,30-0,29-0,32-0,31-0,136-0,135-0,133-0,134-0,138-0,137-0,34-0,33-0,214-0,173-0,212-0,171-0,213-0,172-0,215-0,175-0,176-0,174-0,97-0,95-0,94-0,99-0,96-0,107-0,106-0,150-0,149-0,189-0,188-0,38-0,37-0,119-0,120-0,121-0,122-0,40-0,39-0,178-0,207-0,204-0,205-0,206-0,182-0,181-0,42-0,41-0,93-0,90-0,89-0,91-0,92-0,124-0,123-0,44-0,43-0,100-0,102-0,101-0,108-0,109-0,151-0,152-0,126-0,125-0,46-0,45-0,128-0,127-0,48-0,47-0,104-0,105-0,103-0,110-0,111-0,50-0,49-0,52-0,51-0,153-0,154-0,54-0,53-0,156-0,155-0,55-0,56-0,157-0,158-0,58-0,57-0,130-0,129-0,60-0,59-0,66-0,65-0,67-0,68-0,132-0,131-0,186-0,185-0,69-0,70-0,72-0,71-0,76-0,75-0,77-0,187-0,78-0,80-0,79-0,82-0,81-0,84-0,83-0,86-0,85-0,87-0,88-0,309-0,308-0";
                    
                    dynamic returnedInfo = Request.SendRequest(currentApiRequest.markersRequest + str);
                    //if (returnedInfo == null) //throw new Exception();
                    //{
                    //    Thread.Sleep(5000);
                    //    returnedInfo = Request.SendRequest(currentApiRequest.markersRequest + str);
                    //    if (returnedInfo == null) throw new Exception();
                    //}
                    while (returnedInfo == null)
                    {
                        Thread.Sleep(GlobalVehiclesProvider.UpdatingPeriodMilliseconds);
                        returnedInfo = Request.SendRequest(currentApiRequest.markersRequest + str);
                    } 

                    dynamic objects = returnedInfo.anims;
                    //ExtendedVehicle extendedVehicle = null;
                    DateTime lastUpdatingMoment;
                    OptimalRoute.GeoCoords coords = null;

                    //MessageBox.Show((objects as IEnumerable<dynamic>).Count().ToString());
                
                    foreach (dynamic obj in objects)
                    {
                        ExtendedVehicle newExtendedVehicle = new ExtendedVehicle(obj, this, index);
                        bool ok = true;
                        lastUpdatingMoment = DateTime.Parse((string)obj.lasttime);
                        coords = new OptimalRoute.GeoCoords(Math.Round((double)obj.lat / 1000000, 4), Math.Round((double)obj.lon / 1000000, 4));
                        foreach (ExtendedVehicle v in extendedVehiclesCurrentPart)
                        {
                            if (v.id == newExtendedVehicle.id)
                            {
                                ok = false;
                                                                //foreach (ExtendedVehicle extendedVehicle in extendedVehiclesCurrentPart)
                                                                //{
                                                                //    if (extendedVehicle.id == int.Parse((string)obj.id))
                                                                //    {
                                        Vehicle veh = v.vehicle;// extendedVehicle.vehicle;
                                if (veh != null) vehiclesInfo[veh] = new Group(coords, lastUpdatingMoment);
                                v.Updated();
                                break;
                                                                //    }
                                                                //}
                                                                //break;
                            }
                        }
                        if (ok)
                        {
                            extendedVehiclesCurrentPart.Add(newExtendedVehicle);
                            newExtendedVehicle.Updated();
                            vehiclesInfo.TryAdd(newExtendedVehicle.vehicle, new Group(coords, lastUpdatingMoment));
                            //vehiclesInfo.Add(newExtendedVehicle.vehicle, new Group(coords, lastUpdatingMoment));
                        }
                    }
                });
                //tmpThreads[i].Start();
            }
            foreach (Thread t in tmpThreads) t.Start();
            foreach (Thread t in tmpThreads) t.Join();
        }
    }
}
