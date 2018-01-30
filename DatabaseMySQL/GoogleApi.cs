using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TransportClasses.OptimalRoute;

namespace TransportClasses
{
    public static class Request
    {
        public static dynamic SendRequest(string request)
        {
            try
            {
                WebRequest req = WebRequest.Create(request);
                WebResponse resp = req.GetResponse();

                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                string result = sr.ReadToEnd();
                sr.Close();

                return JsonConvert.DeserializeObject<dynamic>(result);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
    public static class GoogleApi
    {
        public static GeoCoords GetCoords(string adress, GeoCoords crds = null)
        {
            /*string request = "https://maps.googleapis.com/maps/api/geocode/json?address=" + adress.Replace("&", "");
            string point = null;

            if (crds != null)
            {
                string x = crds.xCoord.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string y = crds.yCoord.ToString(System.Globalization.CultureInfo.InvariantCulture);
                point = ("&bounds=" + x + "," + y + "|" + x + "," + y).ToString();
                request += point;
            }

            WebRequest req = WebRequest.Create(request);
            WebResponse resp = req.GetResponse();

            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string result = sr.ReadToEnd();
            sr.Close();

            dynamic apiResultObject = JsonConvert.DeserializeObject<dynamic>(result);

            if ((string)apiResultObject.status != "OK") throw new Exception("Неудачный запрос к Google API.");

            double xCoord = Math.Round((double)apiResultObject.results[0].geometry.location.lat, 4);
            double yCoord = Math.Round((double)apiResultObject.results[0].geometry.location.lng, 4);*/

            string request = "http://nominatim.openstreetmap.org/search?q=" + adress.Replace("&", "") + "&format=json";

            WebRequest req = WebRequest.Create(request);
            WebResponse resp = req.GetResponse();

            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string result = sr.ReadToEnd();
            sr.Close();

            dynamic apiResultObject = JsonConvert.DeserializeObject<dynamic>(result);

            if (apiResultObject == null || ((IEnumerable<dynamic>)apiResultObject).Count() == 0) throw new Exception("Неудачный запрос к OSM API.");
            //    if (allPoints != null && allPoints.length != 0 && (type == "start" || type == "final"))
            //    {

            //        var coords = {
            //                lat: parseFloat(allPoints[0].lat),
            //                lng: parseFloat(allPoints[0].lon)
            //            };
            //    //var findedName = allPoints[0].display_name;//results[0].formatted_address;
            //    //$("input[name=" + inputName + "]").val(findedName);
            //    if (type == "start") setStartOptimalRoutePoint(coords);
            //    else setFinalOptimalRoutePoint(coords);
            //}

            double xCoord = Math.Round((double)apiResultObject[0].lat, 4);
            double yCoord = Math.Round((double)apiResultObject[0].lon, 4);

            return new GeoCoords(Math.Round(xCoord, 4), Math.Round(yCoord, 4));

        }
        public static int GetWalkingDistance(GeoCoords from, GeoCoords to)
        {
            return GetWalkingDistances(from, new GeoCoords[] { to })[0];
        }
        public static List<int> GetWalkingDistances(GeoCoords from, IEnumerable<GeoCoords> toArray)
        {
            StringBuilder sb = new StringBuilder();
            foreach (GeoCoords crds in toArray)
            {
                sb.Append(Math.Round(crds.lat, 4).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + Math.Round(crds.lng, 4).ToString(System.Globalization.CultureInfo.InvariantCulture) + "|");
            }
            sb.Remove(sb.Length - 1, 1);

            string request = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + from.lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + from.lng.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&destinations=" + sb.ToString() + "&mode=walking";

            WebRequest req = WebRequest.Create(request);
            WebResponse resp = req.GetResponse();

            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string result = sr.ReadToEnd();
            sr.Close();

            dynamic apiResultObject = JsonConvert.DeserializeObject<dynamic>(result);
            if ((string)apiResultObject.status != "OK") throw new Exception("Неудачный запрос к Google API.");

            dynamic apiResultObjectElements = apiResultObject.rows[0].elements;
            
            List<int> distances = new List<int>();
            foreach (dynamic element in apiResultObjectElements)
            {
                distances.Add((int)element.distance.value);
            }

            return distances;

        }
    }
}
