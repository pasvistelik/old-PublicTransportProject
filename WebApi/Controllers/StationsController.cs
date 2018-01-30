using TransportClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Windows.Forms;
using System.Web.Http.Cors;

namespace WebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]//http in origins
    public class StationsController : ApiController
    {
        private static List<Station> allStations = null;
        private static List<SimpleStation> allSimpleStations = null;
        // GET: api/Stations
        [HttpGet, AllowAnonymous, Route("api/Stations")]
        public IEnumerable<SimpleStation> Get()
        {
            if (allSimpleStations != null) return allSimpleStations;
            try
            {
                Database.TryInitialize();
                //Database.Connect();!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (allSimpleStations == null || allStations.Count != allSimpleStations.Count)
                {
                    if (allStations == null) allStations = Database.GetAllStations(canReadDataFromLocalCopy: true);
                    allSimpleStations = new List<SimpleStation>();
                    int counter = 0;
                    foreach (Station s in allStations) if (s != null && s.routes != null && s.routes.Count != 0) allSimpleStations.Add(new SimpleStation(s, counter++));
                }
                return allSimpleStations; 
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
            
        }

        /*[HttpGet, AllowAnonymous, Route("api/Stations")]
        public IEnumerable<Station> Get(string coords)
        {
            try
            {
                //MessageBox.Show(coords.Split(new char[] { ',' })[0]);
                string[] tmp = coords.Split(',');
                if (tmp.Length != 4) throw new Exception();

                double my_x0 = double.Parse(tmp[0], System.Globalization.CultureInfo.InvariantCulture);
                double my_y0 = double.Parse(tmp[1], System.Globalization.CultureInfo.InvariantCulture);
                double my_x1 = double.Parse(tmp[2], System.Globalization.CultureInfo.InvariantCulture);
                double my_y1 = double.Parse(tmp[3], System.Globalization.CultureInfo.InvariantCulture);
                
                Database.Connect();
                return Database.GetStationsInRectangle(new OptimalRoute.GeoCoords(my_x0, my_y0), new OptimalRoute.GeoCoords(my_x1, my_y1), canReadDataFromLocalCopy:true);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                Database.Disconnect();
            }
        }*/

        // GET: api/Stations/5
        /*[HttpGet, AllowAnonymous, Route("api/Stations/{hashcode}")]
        public SimpleStation GetStations(string hashcode)
        {
            try
            {
                //Database.Connect();!!!!!!!!!!!!!!!!!!!!
                return new SimpleStation(Database.GetStationByHashcode(hashcode, canReadDataFromLocalCopy: true));
            }
            catch
            {
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
        }*/

        [HttpGet, AllowAnonymous, Route("api/Stations/{hashcode}/Routes")]
        public List<string> GetRoutesForStation(string hashcode)
        {
            try
            {
                //Database.Connect();!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                return Database.GetRoutesCodesOnStation(hashcode, canReadDataFromLocalCopy: true);
            }
            catch
            {
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
        }

        [HttpGet, AllowAnonymous, Route("api/Stations/Osm/{osmcode}/Routes")]
        public List<string> GetRoutesByStationOsmCode(int osmcode)
        {
            try
            {
                //Database.Connect();
                Station tmp = Database.GetStationByOsmCode(osmcode);
                Database.Disconnect();
                return GetRoutesForStation(tmp.hashcode);
            }
            catch
            {
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
        }

        // GET: api/Stations/5
        [HttpGet, AllowAnonymous, Route("api/Stations/Osm/{id}")]
        public Station Get(int id)
        {
            try
            {
                //Database.Connect();
                return Database.GetStationByOsmCode(id);
            }
            catch
            {
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
        }

        // POST: api/Stations
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Stations/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Stations/5
        public void Delete(int id)
        {
        }





        // GET: api/Stations/5
        [HttpPut, AllowAnonymous, Route("api/Stations/{hashcode}"), ]
        public string PutStation([FromBody]string value, string hashcode)
        {
            return "PUT request is success! ... Value: "+value;
            /*try
            {
                Database.Connect();
                return Database.GetStationByHashcode(hashcode);
            }
            catch
            {
                return null;
            }
            finally
            {
                Database.Disconnect();
            }*/
        }

    }
}
