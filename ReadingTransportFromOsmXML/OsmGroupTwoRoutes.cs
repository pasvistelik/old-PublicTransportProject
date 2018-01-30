using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadingTransportFromOsmXML
{
    class OsmGroupTwoRoutes
    {
        public readonly string hashcode;
        public string from;
        public string to;
        public string type, num;
        public List<string>[] stations_ids_osm;
        public string[] osm_id;
        public string[] osm_version;
        public string[] osm_changeset;
        public string[] osm_timestamp;
        public string[] osm_last_user_name;
        public string[] osm_last_user_id;


        public OsmGroupTwoRoutes(string type, string from, string to, List<string>[] stations_ids_osm, string[] osm_id, string[] osm_version, string[] osm_changeset, string[] osm_timestamp, string[] osm_last_user_name, string[] osm_last_user_id, string num)
        {
            Thread.Sleep(1);
            hashcode = "R" + ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString("X");

            this.from = from;
            this.to = to;
            this.stations_ids_osm = stations_ids_osm;
            this.osm_id = osm_id;
            this.osm_version = osm_version;
            this.osm_changeset = osm_changeset;
            this.osm_timestamp = osm_timestamp;
            this.osm_last_user_name = osm_last_user_name;
            this.osm_last_user_id = osm_last_user_id;
            this.type = type;
            this.num = num;
            //Console.WriteLine(routeGroupNameOsm);
        }


















        /*
         
         public readonly string hashcode;
        string ownerNameOsm="", ownerIdOsm="";
        public string OwnerNameOsm { get { return ownerNameOsm; } }
        public string OwnerIdOsm { get { return ownerIdOsm; }}
        public readonly string routeGroupNameOsm, routeGroupNameByOSM, routeGroupNameRuOSM, routeGroupNameEnOSM, routeGroupTypeOsm, routeGroupIdOsm, routeGroupVersionOSM, routeGroupChangesetOSM, routeGroupTimestampOSM, routeGroupLastUserNameOSM, routeGroupLastUserIdOSM;
        public List<string> RoutesGroupOsm;
        public void SetOwner(string ownerNameOsm, string ownerIdOsm)
        {
            this.ownerNameOsm = ownerNameOsm;
            this.ownerIdOsm = ownerIdOsm;
        }
        public OsmGroupTwoRoutes(List<string> RoutesGroupOsm, string routeGroupNameOsm, string routeGroupNameByOSM, string routeGroupNameRuOSM, string routeGroupNameEnOSM, string routeGroupTypeOsm, string routeGroupIdOsm, string routeGroupVersionOSM, string routeGroupChangesetOSM, string routeGroupTimestampOSM, string routeGroupLastUserNameOSM, string routeGroupLastUserIdOSM)
        {
            Thread.Sleep(1);
            hashcode = "R" + ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString("X");
            this.RoutesGroupOsm= RoutesGroupOsm;
            this.routeGroupNameOsm = routeGroupNameOsm;
            this.routeGroupNameByOSM = routeGroupNameByOSM;
            this.routeGroupNameRuOSM = routeGroupNameRuOSM;
            this.routeGroupNameEnOSM = routeGroupNameEnOSM;
            this.routeGroupTypeOsm = routeGroupTypeOsm;
            this.routeGroupIdOsm = routeGroupIdOsm;
            this.routeGroupVersionOSM = routeGroupVersionOSM;
            this.routeGroupChangesetOSM = routeGroupChangesetOSM;
            this.routeGroupTimestampOSM = routeGroupTimestampOSM;
            this.routeGroupLastUserNameOSM = routeGroupLastUserNameOSM;
            this.routeGroupLastUserIdOSM = routeGroupLastUserIdOSM;

            //Console.WriteLine(routeGroupNameOsm);
        }
         
         */
    }
}
