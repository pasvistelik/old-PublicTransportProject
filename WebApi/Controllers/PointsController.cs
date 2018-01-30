using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Windows.Forms;
using TransportClasses;

namespace WebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]//http in origins
    public class PointsController : ApiController
    {
        /*[HttpGet, AllowAnonymous, Route("api/Points")]
        public List<string> Get(string coords)
        {
            try
            {
                Database.TryInitialize();
                //MessageBox.Show(coords.Split(new char[] { ',' })[0]);
                string[] tmp = coords.Split(',');
                if (tmp.Length != 4) throw new Exception();

                double my_x0 = double.Parse(tmp[0], System.Globalization.CultureInfo.InvariantCulture);
                double my_y0 = double.Parse(tmp[1], System.Globalization.CultureInfo.InvariantCulture);
                double my_x1 = double.Parse(tmp[2], System.Globalization.CultureInfo.InvariantCulture);
                double my_y1 = double.Parse(tmp[3], System.Globalization.CultureInfo.InvariantCulture);

                OptimalRoute.GeoCoords from = new OptimalRoute.GeoCoords(my_x0, my_y0);
                OptimalRoute.GeoCoords to = new OptimalRoute.GeoCoords(my_x1, my_y1);

                Dictionary<OptimalRoute.Priority, double> priorities = new Dictionary<OptimalRoute.Priority, double>();
                priorities.Add(OptimalRoute.Priority.MinimalTime, 1);

                Database.Connect();
                OptimalRoute res = OptimalRoute.GetBestOptimalRoute(from, to, DateTime.Now, priorities);//new OptimalRoute(from, to, DateTime.Now);
                return res.points;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                Database.Disconnect();
            }
        }*/

        [HttpGet, AllowAnonymous, Route("api/Points")]
        public List<string> Get(string from, string to, string startTime = null, string priorities = null, string customize = null, string transportTypes = null, string goingSpeed = null, string dopTimeMinutes = null)
        {
            try
            {
                Database.TryInitialize();

                double my_x0;
                double my_y0;
                OptimalRoute.GeoCoords fromPosition = null;
                double my_x1;
                double my_y1;
                OptimalRoute.GeoCoords toPosition = null;

                string[] tmp = from.Split(',');
                if (tmp.Length != 2 || !double.TryParse(tmp[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_x0) || !double.TryParse(tmp[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_y0))
                {
                    tmp = to.Split(',');
                    if (tmp.Length == 2 && double.TryParse(tmp[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_x1) && double.TryParse(tmp[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_y1))
                    {
                        toPosition = new OptimalRoute.GeoCoords(my_x1, my_y1);
                    }
                    fromPosition = GoogleApi.GetCoords(from, toPosition);
                }
                else
                {
                    fromPosition = new OptimalRoute.GeoCoords(my_x0, my_y0);
                }


                if (toPosition == null)
                {
                    tmp = to.Split(',');
                    if (tmp.Length != 2 || !double.TryParse(tmp[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_x1) || !double.TryParse(tmp[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_y1))
                    {
                        toPosition = GoogleApi.GetCoords(to, fromPosition);
                    }
                    else
                    {
                        toPosition = new OptimalRoute.GeoCoords(my_x1, my_y1);
                    }
                }

                Dictionary<OptimalRoute.Priority, double> myPriorities = new Dictionary<OptimalRoute.Priority, double>();
                myPriorities.Add(OptimalRoute.Priority.MinimalTime, 1);

                DateTime myStartTime;
                //MessageBox.Show(DateTime.Now.ToShortTimeString());
                if (startTime == null) myStartTime = DateTime.Now;
                else
                {
                    myStartTime = DateTime.Parse(startTime);
                    if (myStartTime < DateTime.Now) myStartTime += TimeSpan.FromDays(1);
                }

                double percentTotalTime = 1;
                double percentTotalGoingTime = 1;
                double percentTotalTransportChangingCount = 1;

                List<OptimalRoute.RouteType> types = null;
                if (transportTypes != null)
                {
                    try
                    {
                        string[] rTypes = transportTypes.Split(',');
                        types = new List<OptimalRoute.RouteType>();
                        foreach (string s in rTypes)
                        {
                            try
                            {
                                types.Add(OptimalRoute.RouteTypeConverter.FromString(s));
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                    catch
                    {
                        types = null;
                    }
                }

                if (customize != null)
                {
                    string[] tmpCustomize = customize.Split('|');
                    foreach (string s in tmpCustomize)
                    {
                        string[] ss = s.Split(',');
                        if (ss.Length != 2) throw new FormatException();
                        if (ss[0].Equals("totalTime", StringComparison.InvariantCultureIgnoreCase)) percentTotalTime = double.Parse(ss[1], System.Globalization.CultureInfo.InvariantCulture);
                        else if (ss[0].Equals("totalGoingTime", StringComparison.InvariantCultureIgnoreCase)) percentTotalGoingTime = double.Parse(ss[1], System.Globalization.CultureInfo.InvariantCulture);
                        else if (ss[0].Equals("totalTransportChangingCount", StringComparison.InvariantCultureIgnoreCase)) percentTotalTransportChangingCount = double.Parse(ss[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                double my_speed = 5;
                if (goingSpeed != null)
                {
                    if (!double.TryParse(goingSpeed, out my_speed) || my_speed <= 0) my_speed = 5;
                }
                double my_dopTimeMinutes = 2;
                if (dopTimeMinutes != null)
                {
                    if (!double.TryParse(dopTimeMinutes, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_dopTimeMinutes) || my_dopTimeMinutes < 0)
                        my_dopTimeMinutes = 2;
                }

                //Database.Connect();!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                OptimalRoute res = OptimalRoute.GetBestOptimalRoute(fromPosition, toPosition, myStartTime, myPriorities, types, my_speed, my_dopTimeMinutes, percentTotalTime, percentTotalGoingTime, percentTotalTransportChangingCount);//new OptimalRoute(from, to, DateTime.Now);
                //Database.Disconnect();
                return res.points;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("111111"+ex.Message);
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
        }
        [HttpGet, AllowAnonymous, Route("api/OptimalRoute")]
        public List<OptimalWay> GetFullOptimalRoute(string from, string to, string startTime = null, string priorities = null, string customize = null, string transportTypes = null, string goingSpeed = null, string dopTimeMinutes = null)
        {
            try
            {
                Database.TryInitialize();

                double my_x0;
                double my_y0;
                OptimalRoute.GeoCoords fromPosition = null;
                double my_x1;
                double my_y1;
                OptimalRoute.GeoCoords toPosition = null;

                string[] tmp = from.Split(',');
                if (tmp.Length != 2 || !double.TryParse(tmp[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_x0) || !double.TryParse(tmp[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_y0))
                {
                    tmp = to.Split(',');
                    if (tmp.Length == 2 && double.TryParse(tmp[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_x1) && double.TryParse(tmp[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_y1))
                    {
                        toPosition = new OptimalRoute.GeoCoords(my_x1, my_y1);
                    }
                    fromPosition = GoogleApi.GetCoords(from, toPosition);
                }
                else
                {
                    fromPosition = new OptimalRoute.GeoCoords(my_x0, my_y0);
                }


                if (toPosition == null)
                {
                    tmp = to.Split(',');
                    if (tmp.Length != 2 || !double.TryParse(tmp[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_x1) || !double.TryParse(tmp[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_y1))
                    {
                        toPosition = GoogleApi.GetCoords(to, fromPosition);
                    }
                    else
                    {
                        toPosition = new OptimalRoute.GeoCoords(my_x1, my_y1);
                    }
                }

                Dictionary<OptimalRoute.Priority, double> myPriorities = new Dictionary<OptimalRoute.Priority, double>();
                myPriorities.Add(OptimalRoute.Priority.MinimalTime, 1);

                DateTime myStartTime;
                //MessageBox.Show(DateTime.Now.ToShortTimeString());
                if (startTime == null) myStartTime = DateTime.Now;
                else
                {
                    myStartTime = DateTime.Parse(startTime);
                    if (myStartTime < DateTime.Now) myStartTime += TimeSpan.FromDays(1);
                }
                

                double percentTotalTime = 1;
                double percentTotalGoingTime = 1;
                double percentTotalTransportChangingCount = 1;

                List<OptimalRoute.RouteType> types = null;
                if (transportTypes != null)
                {
                    try
                    {
                        string[] rTypes = transportTypes.Split(',');
                        types = new List<OptimalRoute.RouteType>();
                        foreach (string s in rTypes)
                        {
                            try
                            {
                                types.Add(OptimalRoute.RouteTypeConverter.FromString(s));
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                    catch
                    {
                        types = null;
                    }
                }

                if (customize != null)
                {
                    string[] tmpCustomize = customize.Split('|');
                    foreach (string s in tmpCustomize)
                    {
                        string[] ss = s.Split(',');
                        if (ss.Length != 2) throw new FormatException();
                        if (ss[0].Equals("totalTime", StringComparison.InvariantCultureIgnoreCase)) percentTotalTime = double.Parse(ss[1], System.Globalization.CultureInfo.InvariantCulture);
                        else if (ss[0].Equals("totalGoingTime", StringComparison.InvariantCultureIgnoreCase)) percentTotalGoingTime = double.Parse(ss[1], System.Globalization.CultureInfo.InvariantCulture);
                        else if (ss[0].Equals("totalTransportChangingCount", StringComparison.InvariantCultureIgnoreCase)) percentTotalTransportChangingCount = double.Parse(ss[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                double my_speed = 5;
                if (goingSpeed != null)
                {
                    if (!double.TryParse(goingSpeed, out my_speed) || my_speed <= 0) my_speed = 5;
                }
                double my_dopTimeMinutes = 2;
                if (dopTimeMinutes != null)
                {
                    if (!double.TryParse(dopTimeMinutes, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out my_dopTimeMinutes) || my_dopTimeMinutes < 0)
                        my_dopTimeMinutes = 2;
                }

                //Database.Connect();!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                OptimalRoutesCollection res = OptimalRoute.FindOptimalRoutes(fromPosition, toPosition, myStartTime, myPriorities, types, my_speed, my_dopTimeMinutes, percentTotalTime, percentTotalGoingTime, percentTotalTransportChangingCount);//new OptimalRoute(from, to, DateTime.Now);
                //Database.Disconnect();
                return res.GetOptimalWays();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("111111"+ex.Message);
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
        }
    }
}
