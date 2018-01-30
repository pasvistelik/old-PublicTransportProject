using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using TransportClasses;

namespace WebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]//http in origins
    public class VehiclesController : ApiController
    {
        [HttpGet, AllowAnonymous, Route("api/Vehicles/{hashcode}")]
        public SimpleVehicle GetVehicle(string hashcode)
        {
            try
            {
                Database.TryInitialize();

                IEnumerable<Vehicle> tmp = GlobalVehiclesProvider.Vehicles;
                foreach (Vehicle v in tmp)
                {
                    if (v.Hashcode == hashcode) return new SimpleVehicle(v);
                }
                return null;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(hashcode + "   "+ex.Message);
                return null;
            }
        }
        [HttpGet, AllowAnonymous, Route("api/Vehicles")]
        public IEnumerable<SimpleVehicle> GetAllVehicles()
        {
            try
            {
                Database.TryInitialize();

                List<SimpleVehicle> result = new List<SimpleVehicle>();
                IEnumerable<Vehicle> tmp = GlobalVehiclesProvider.Vehicles;
                foreach (Vehicle v in tmp)
                {
                    try
                    {
                        if (v != null) result.Add(new SimpleVehicle(v));
                    }
                    catch (Exception ex)
                    {

                    }
                }

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
                Database.Disconnect();
            }
        }
        [HttpGet, AllowAnonymous, Route("api/Vehicles/{hashcode}/Route")]
        public SimpleRoute GetVehicleRoute(string hashcode)
        {
            try
            {
                Database.TryInitialize();

                IEnumerable<Vehicle> tmp = GlobalVehiclesProvider.Vehicles;
                foreach (Vehicle v in tmp)
                {
                    if (v.Hashcode == hashcode) return new SimpleRoute(v.route);
                }
                return null;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(hashcode + "   "+ex.Message);
                return null;
            }
        }
        [HttpGet, AllowAnonymous, Route("api/Vehicles/{hashcode}/CurrentPosition")]
        public OptimalRoute.GeoCoords GetVehicleCurrentPosition(string hashcode)
        {
            try
            {
                SimpleVehicle v = GetVehicle(hashcode);
                return new OptimalRoute.GeoCoords(v.lat, v.lng); 
            }
            catch (Exception ex)
            {
                //MessageBox.Show(hashcode + "   "+ex.Message);
                return null;
            }
        }
    }
}
