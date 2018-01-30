using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoCoords = TransportClasses.OptimalRoute.GeoCoords;

namespace TransportClasses
{
    public class Vehicle
    {
        private readonly string hashcode;
        public string Hashcode
        {
            get
            {
                return hashcode;
            }
        }
        public readonly VehiclesProvider vehiclesProvider;
        private DateTime lastUpdatingMoment;
        private DateTime lastUpdatingMomentOnThisBase;
        public DateTime LastUpdatingMoment
        {
            get
            {
                return lastUpdatingMoment;
            }
        }
        public DateTime LastUpdatingMomentOnThisBase
        {
            get
            {
                return lastUpdatingMomentOnThisBase;
            }
        }
        public readonly Route route;
        public GeoCoords Coords
        {
            get
            {
                GeoCoords result = vehiclesProvider.GetCurrentPosition(this, out lastUpdatingMoment);
                if (result != null) lastUpdatingMomentOnThisBase = DateTime.Now;
                return result;
            }
        }
        public Vehicle(Route route, VehiclesProvider vehiclesProvider)
        {
            lastUpdatingMomentOnThisBase = DateTime.Now;
            this.route = route;
            this.vehiclesProvider = vehiclesProvider;

            if (route != null && !route.vehicles.Contains(this))
            {
                //bool ok = true;
                //foreach (Vehicle v in route.vehicles)
                //{
                //    //if(v.id)
                //}
                
                route.vehicles.Add(this);
            }
            hashcode = GlobalVehiclesProvider.NextHashcode;
        }
    }
    public class SimpleVehicle
    {
        public readonly string hashcode;
        private readonly Vehicle vehicle;
        public readonly DateTime lastUpdatingMoment;
        public readonly string route;
        //public readonly GeoCoords coords;
        public readonly double lat, lng;
        public SimpleVehicle(Vehicle vehicle)
        {
            hashcode = vehicle.Hashcode;
            this.vehicle = vehicle;
            lastUpdatingMoment = vehicle.LastUpdatingMoment;
            GeoCoords coords = vehicle.Coords;
            lat = coords.lat;
            lng = coords.lng;
            route = vehicle.route == null ? null : vehicle.route.hashcode;
        }
    }
}
