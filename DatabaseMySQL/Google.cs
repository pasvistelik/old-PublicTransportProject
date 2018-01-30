using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
//using GeoCoords = TransportClasses.OptimalRoute.GeoCoords;

namespace TransportClasses
{
    public static class GoogleApi
    {
        public class ResponceCoords
        {

        }
        public static GeoCoords GetCoords(string adress, GeoCoords crds = null)
        {
            string request = "https://maps.googleapis.com/maps/api/geocode/json?address=" + adress.Replace("&", "");
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

            Regex regexX = new Regex("(?<=\"location\"(\\s)*:(\\s)*{(\\s)*\"lat\"(\\s)*:(\\s)*)([0-9]+.[0-9]+)(?=(\\s)*,(\\s)*\"lng\"(\\s)*:(\\s)*[0-9]+.[0-9]+(\\s)*}(\\s)*,(\\s)*)");
            Regex regexY = new Regex("(?<=\"location\"(\\s)*:(\\s)*{(\\s)*\"lat\"(\\s)*:(\\s)*[0-9]+.[0-9]+(\\s)*,(\\s)*\"lng\"(\\s)*:(\\s)*)([0-9]+.[0-9]+)(?=(\\s)*}(\\s)*,(\\s)*)");

            double xCoord = double.Parse(regexX.Match(result).Value, System.Globalization.CultureInfo.InvariantCulture);
            double yCoord = double.Parse(regexY.Match(result).Value, System.Globalization.CultureInfo.InvariantCulture);

            return new GeoCoords(Math.Round(xCoord, 4), Math.Round(yCoord, 4));

        }


        public static int GetWalkingDistance(GeoCoords from, GeoCoords to)
        {
            string request = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + from.xCoord.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + from.yCoord.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&destinations=" + to.xCoord.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + to.yCoord.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&mode=walking";

            WebRequest req = WebRequest.Create(request);
            WebResponse resp = req.GetResponse();

            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string result = sr.ReadToEnd();
            sr.Close();

            Regex regex = new Regex("(?<=(\\s)*{(\\s)*\"distance\"(\\s)*:(\\s)*{(\\s)*\"text\"(\\s)*:(\\s)*\"[0-9]+(,)?(\\s)*([0-9]+)?(\\s)*(\\w)+\"(\\s)*,(\\s)*\"value\"(\\s)*:(\\s)*)([0-9]+)(?=(\\s)*}(\\s)*,(\\s)*\"duration\")");

            return int.Parse(regex.Match(result).Value);

        }
        public static List<int> GetWalkingDistances(GeoCoords from, IEnumerable<GeoCoords> toArray)
        {
            if (toArray.Count() > 25) throw new Exception();

            StringBuilder sb = new StringBuilder();
            foreach (GeoCoords crds in toArray)
            {
                sb.Append(Math.Round(crds.xCoord, 4).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + Math.Round(crds.yCoord, 4).ToString(System.Globalization.CultureInfo.InvariantCulture) + "|");
            }
            sb.Remove(sb.Length - 1, 1);
            
            string request = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + from.xCoord.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + from.yCoord.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&destinations=" + sb.ToString() + "&mode=walking";

            WebRequest req = WebRequest.Create(request);
            WebResponse resp = req.GetResponse();

            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string result = sr.ReadToEnd();
            sr.Close();

            Regex regex = new Regex("(?<=(\\s)*{(\\s)*\"distance\"(\\s)*:(\\s)*{(\\s)*\"text\"(\\s)*:(\\s)*\"[0-9]+(,)?(\\s)*([0-9]+)?(\\s)*(\\w)+\"(\\s)*,(\\s)*\"value\"(\\s)*:(\\s)*)([0-9]+)(?=(\\s)*}(\\s)*,(\\s)*\"duration\")");

            List<int> distances = new List<int>();
            MatchCollection mc = regex.Matches(result);
            foreach (Match m in mc)
            {
                distances.Add(int.Parse(m.Value));
            }

            if (distances.Count != toArray.Count()) throw new Exception();

            return distances;

        }
    }
}