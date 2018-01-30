using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransportClasses
{
    public partial class OptimalRoute
    {
        public class GeoCoords
        {
            public class DistanceObj
            {
                public readonly Station station;
                public readonly int realDistance;
                public readonly int linearDistance;
                public DistanceObj(Station station, int realDistance, int linearDistance)
                {
                    if (station == null) throw new ArgumentNullException();
                    if (realDistance == 0 || linearDistance == 0) throw new ArgumentException();
                    this.station = station;
                    this.realDistance = realDistance;
                    this.linearDistance = linearDistance;
                }
                private static IEnumerable<DistanceObj> GetDistances(Station currentStation, List<Station> stations)
                {
                    List<DistanceObj> result = new List<DistanceObj>();
                    if (stations == null || currentStation == null) throw new ArgumentNullException();
                    if (stations.Count() > 50) throw new ArgumentOutOfRangeException();
                    if (stations.Count() == 0) return result; 
                    StringBuilder tmpReq = new StringBuilder();
                    foreach (Station s in stations)
                    {
                        if (s != null) tmpReq.Append("{\"lat\":" + s.Coords.lat + ",\"lon\":" + s.Coords.lng + "},");
                    }
                    tmpReq.Remove(tmpReq.Length - 1, 1);
                    string request = "http://matrix.mapzen.com/sources_to_targets?json="
                        + "{\"sources\":[{\"lat\":" + currentStation.Coords.lat + ",\"lon\":" + currentStation.Coords.lng + "}],"
                        + "\"targets\":[" + tmpReq.ToString() + "],"
                        + "\"costing\":\"pedestrian\"}"
                        + "&api_key=valhalla-7UikjOk";

                    try
                    {
                        dynamic returnedObject = Request.SendRequest(request);

                        List<dynamic> tmp = returnedObject.sources_to_targets[0];
                        for (int i = 0, n = tmp.Count(); i < n; i++) result.Add(new DistanceObj(stations[i], (int)((double)tmp[i].distance * 1000), Distance(currentStation.Coords, stations[i].Coords)));

                        return result;
                    }
                    catch
                    {
                        return null;
                    }
                }
                public static IEnumerable<DistanceObj> GetDistances(Station currentStation)
                {
                    List<Station> tmp = Database.GetStationsAround(currentStation.Coords, 2000, canReadDataFromLocalCopy: true);
                    IEnumerable<DistanceObj> result = GetDistances(currentStation, tmp);
                    if (result == null)
                    {
                        Thread.Sleep(1000);
                        result = GetDistances(currentStation, tmp);
                    }
                    return result;
                }
            }



            private const int earthRadius = 6372795;
            private const double pi180 = 0.017453;// 29251//Math.Round(Math.PI / 180, 5);
            /// <summary>
            /// Широта.
            /// </summary>
            public readonly double lat;
            /// <summary>
            /// Долгота.
            /// </summary>
            public readonly double lng;
            private static double zz = 1, yy = 1;
            public static double TaylorSin(double x)
            {
                yy = x * x;
                zz = x;
                return zz - (zz *= yy) / 6 + (zz *= yy) / 120;
            }
            public static double TaylorCos(double x)
            {
                yy = x * x;
                zz = yy;
                return 1 - (yy) / 2 + (zz *= yy) / 24;
            }
            public static double TaylorAtan(double x)
            {
                yy = x * x;
                zz = x;
                return zz - (zz *= yy) / 3 + (zz *= yy) / 5 - (zz *= yy) / 7 + (zz *= yy) / 9 - (zz *= yy) / 20;
            }
            public static int Distance(GeoCoords a, GeoCoords b)
            {
                {
                    // перевести координаты в радианы
                    double lat1 = a.lat * pi180;
                    double lat2 = b.lat * pi180;
                    double long1 = a.lng * pi180;
                    double long2 = b.lng * pi180;

                    // косинусы и синусы широт и разницы долгот
                    double cl1 = TaylorCos(lat1);
                    double cl2 = TaylorCos(lat2);
                    double sl1 = TaylorSin(lat1);
                    double sl2 = TaylorSin(lat2);
                    double delta = long2 - long1;
                    double cdelta = TaylorCos(delta);
                    double sdelta = TaylorSin(delta);

                    // вычисления длины большого круга
                    double tmp = cl2 * cdelta;
                    double y = Math.Sqrt(cl2 * cl2 * sdelta * sdelta + (cl1 * sl2 - sl1 * tmp) * (cl1 * sl2 - sl1 * tmp));
                    double x = sl1 * sl2 + cl1 * tmp;

                    //
                    double ad = Math.Atan2(y, x);//TaylorAtan(y/x);
                    int dist = (int)(ad * earthRadius);//(int)Math.Round(ad * earthRadius, 0);

                    //double dist2 = Math.Atan2(Math.Sqrt(Math.Pow(Math.Cos(b.xCoord * Math.PI / 180) * Math.Sin((b.yCoord - a.yCoord) * Math.PI / 180), 2) + Math.Pow(Math.Cos(a.xCoord * Math.PI / 180) * Math.Sin(b.xCoord * Math.PI / 180) - Math.Sin(a.xCoord * Math.PI / 180) * Math.Cos(b.xCoord * Math.PI / 180) * Math.Cos((b.yCoord - a.yCoord) * Math.PI / 180), 2)), Math.Sin(a.xCoord * Math.PI / 180) * Math.Sin(b.xCoord * Math.PI / 180) + Math.Cos(a.xCoord * Math.PI / 180) * Math.Cos(b.xCoord * Math.PI / 180) * Math.Cos((b.yCoord - a.yCoord) * Math.PI / 180)) * 6372795;
                    //dist2 = (int)Math.Round(dist2, 0);

                    return dist;
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
            /// <param name="lat">Широта в градусах (от -90 до 90).</param>
            /// <param name="lng">Долгота в градусах (от -180 до 180).</param>
            public GeoCoords(double lat, double lng)
            {
                if (lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180)
                {
                    this.lat = lat;
                    this.lng = lng;
                }
                else throw new FormatException("Некорректные координаты.");
            }

            public static bool operator ==(GeoCoords a, GeoCoords b)
            {
                if ((a as object) == null && (b as object) == null) return true;
                if ((a as object) == null || (b as object) == null) return false;
                if (a.lat == b.lat && a.lng == b.lng) return true;
                return false;
            }
            public static bool operator !=(GeoCoords a, GeoCoords b)
            {
                /*if ((a as object) == null && (b as object) == null) return false;
                if ((a as object) == null || (b as object) == null) return true;
                if (a.xCoord != b.xCoord || a.yCoord != b.yCoord) return true;
                return false;*/
                return !(a == b);
            }
            public override bool Equals(object obj)
            {
                return this == (obj as GeoCoords);
                /*if (xCoord == (obj as GeoCoords).xCoord && yCoord == (obj as GeoCoords).yCoord) return true;
                return false;*/
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
