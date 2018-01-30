using Newtonsoft.Json;
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
    public class RoutesController : ApiController
    {
        [AllowAnonymous]
        public IEnumerable<SimpleRoute> Get()
        {
            try
            {
                Database.TryInitialize();
                //Database.Connect();!!!!!!!!!
                List<Route> tmp = Database.GetAllRoutes(canReadDataFromLocalCopy: true);
                List<SimpleRoute> result = new List<SimpleRoute>();
                foreach (Route r in tmp) result.Add(new SimpleRoute(r));
                return result;
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }

        }


        [HttpGet, AllowAnonymous, Route("api/Routes/{hashcode}")]
        public SimpleRoute Get(string hashcode)
        {
            try
            {
                //////////////Database.Connect();!!!!!!!!!!!!!!!!!
                return new SimpleRoute(Database.GetRouteByHashcode(hashcode, canReadDataFromLocalCopy: true));
            }
            catch (Exception ex)
            {
                //MessageBox.Show(hashcode + "   "+ex.Message);
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
        }

        [HttpGet, AllowAnonymous, Route("api/Routes/{hashcode}/GpsTrackNodes")]
        public IEnumerable<OptimalRoute.GeoCoords> GetGpsTrackNodes(string hashcode)
        {
            try
            {
                //Database.Connect();
                Route route = Database.GetRouteByHashcode(hashcode, canReadDataFromLocalCopy: true);
                throw new NotImplementedException();
                //return ;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(hashcode + "   "+ex.Message);
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
        }
        [HttpGet, AllowAnonymous, Route("api/Routes/{hashcode}/Vehicles")]
        public IEnumerable<SimpleVehicle> GetVehicles(string hashcode)
        {
            try
            {
                //Database.Connect();
                List<Vehicle> routeVehicles = Database.GetRouteByHashcode(hashcode, canReadDataFromLocalCopy: true).vehicles;
                List<SimpleVehicle> result = new List<SimpleVehicle>();
                foreach (Vehicle v in routeVehicles) result.Add(new SimpleVehicle(v));
                return result;
                //throw new NotImplementedException();
                //return ;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(hashcode + "   "+ex.Message);
                return null;
            }
            finally
            {
                //Database.Disconnect();
            }
        }

        // GET: api/Stations/5
        [HttpGet, AllowAnonymous, Route("api/Routes2/{hashcode}")]
        public string Get2(string hashcode)
        {
            try
            {
                //Database.Connect();
                return JsonConvert.SerializeObject(Database.GetRouteByHashcode(hashcode, canReadDataFromLocalCopy: true));
            }
            catch (Exception ex)
            {
                //MessageBox.Show(hashcode + "   "+ex.Message);
                return ex.Message;
            }
            finally
            {
                //Database.Disconnect();
            }
        }

        [AllowAnonymous]
        [Route("api/Routes/{hashcode}/Stations")]
        public List<string>[] GetStationsForRoute(string hashcode)
        {
            try
            {
                //Database.Connect();
                return Database.GetStationsCodesOnRoute(hashcode, canReadDataFromLocalCopy: true);
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

        [AllowAnonymous]
        [Route("api/Routes/Osm/{osmcode}/Stations")]
        public List<string>[] GetStationsByRouteOsmCode(int osmcode)
        {
            try
            {
                //Database.Connect();
                Route tmp = Database.GetRouteByOsmCode(osmcode);
                Database.Disconnect();
                return GetStationsForRoute(tmp.hashcode);
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
        [AllowAnonymous]
        [Route("api/Routes/Osm/{id}")]
        public Route Get(int id)
        {
            try
            {
                //Database.Connect();
                return Database.GetRouteByOsmCode(id);
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

        // POST: api/Routes
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Routes/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Routes/5
        public void Delete(int id)
        {
        }
    }
}
