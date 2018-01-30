using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadingTransportFromOsmXML
{
    class OsmStation
    {
        public string hashcode, stationLat, stationLon, stationNameOSM, stationNameByOSM, stationNameRuOSM, stationNameEnOSM, stationIdOSM, stationVersionOSM, stationChangesetOSM, stationTimestampOSM, stationLastUserNameOSM, stationLastUserIdOSM;
        public OsmStation(string stationLat, string stationLon, string stationNameOSM, string stationNameByOSM, string stationNameRuOSM, string stationNameEnOSM, string stationIdOSM, string stationVersionOSM, string stationChangesetOSM, string stationTimestampOSM, string stationLastUserNameOSM, string stationLastUserIdOSM)
        {
            Thread.Sleep(1);
            hashcode = "S" + ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString("X");
            this.stationLat = stationLat;
            this.stationLon = stationLon;
            this.stationNameOSM = stationNameOSM.Replace("'", @"\'").Replace("`", @"\'");
            this.stationNameByOSM = stationNameByOSM.Replace("'",@"\'").Replace("`", @"\'");
            this.stationNameRuOSM = stationNameRuOSM.Replace("'", @"\'").Replace("`", @"\'");
            this.stationNameEnOSM = stationNameEnOSM.Replace("'", @"\'").Replace("`", @"\'");
            this.stationIdOSM = stationIdOSM;
            this.stationVersionOSM = stationVersionOSM;
            this.stationChangesetOSM = stationChangesetOSM;
            this.stationTimestampOSM = stationTimestampOSM;
            this.stationLastUserNameOSM = stationLastUserNameOSM.Replace("'", @"\'").Replace("`", @"\'");
            this.stationLastUserIdOSM = stationLastUserIdOSM;
        }
        public string ToMySqlFragment()
        {
            return "('" + hashcode + "', '" + ((int)(double.Parse(stationLat, System.Globalization.CultureInfo.InvariantCulture) * 10000)).ToString() + "', '" + ((int)(double.Parse(stationLon, System.Globalization.CultureInfo.InvariantCulture) * 10000)).ToString() +"', '"+stationNameOSM+"', '"+stationNameByOSM + "', '" + stationNameRuOSM+"', '"+stationNameEnOSM+"', '"+stationIdOSM + "', '" + stationVersionOSM+"', '"+stationChangesetOSM+"', '"+stationTimestampOSM + "', '" + stationLastUserNameOSM+"', '"+stationLastUserIdOSM+"'),";
        }
    }
}
