using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ReadingTransportFromOsmXML
{
    class OsmRoute
    {
        public string /*hashcode,*/routeNumOSM, routeNameOsm, routeNameByOSM, routeNameRuOSM, routeNameEnOSM, routeFromOsm, routeToOsm, routeTypeOsm, routeIdOsm, routeVersionOSM, routeChangesetOSM, routeTimestampOSM, routeLastUserNameOSM, routeLastUserIdOSM;
        public List<string> StationsIdsOsm;

        public OsmRoute(List<string> StationsIdsOsm, string routeNameOsm, string routeNameByOSM, string routeNameRuOSM, string routeNameEnOSM, string routeFromOsm, string routeToOsm, string routeTypeOsm, string routeIdOsm, string routeVersionOSM, string routeChangesetOSM, string routeTimestampOSM, string routeLastUserNameOSM, string routeLastUserIdOSM, string routeNumOSM)
        {
            //Thread.Sleep(1);
            //hashcode = "R" + ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString("X");
            this.StationsIdsOsm = StationsIdsOsm;
            this.routeNameOsm = routeNameOsm.Replace("'", @"\'").Replace("`", @"\'");
            this.routeNameByOSM = routeNameByOSM.Replace("'", @"\'").Replace("`", @"\'");
            this.routeNameRuOSM = routeNameRuOSM.Replace("'", @"\'").Replace("`", @"\'");
            this.routeNameEnOSM = routeNameEnOSM.Replace("'", @"\'").Replace("`", @"\'");
            this.routeFromOsm = routeFromOsm.Replace("'", @"\'").Replace("`", @"\'");
            this.routeToOsm = routeToOsm.Replace("'", @"\'").Replace("`", @"\'");
            this.routeTypeOsm = routeTypeOsm;
            this.routeIdOsm = routeIdOsm;
            this.routeVersionOSM = routeVersionOSM;
            this.routeChangesetOSM = routeChangesetOSM;
            this.routeTimestampOSM = routeTimestampOSM;
            this.routeLastUserNameOSM = routeLastUserNameOSM.Replace("'", @"\'").Replace("`", @"\'");
            this.routeLastUserIdOSM = routeLastUserIdOSM;
            this.routeNumOSM = routeNumOSM;
        }

        public static List<string>[] GetGroupsArray(XmlNodeList nodeList, List<OsmRoute> osmRoutes)
        {
            int n = nodeList.Count;
            List<string>[] routesGroupOsm = new List<string>[n/2];

            List<string> allRoutesGroupOsm = new List<string>();
            foreach (XmlNode xnd in nodeList) allRoutesGroupOsm.Add(xnd.Attributes["ref"].Value);

            List<int> indexes = new List<int>();
            bool ok = true;
            for (int i = 0, j=0; allRoutesGroupOsm.Count > 0 && i < n/2; i++,j++)
            {
                ok = true;
                foreach (OsmRoute r in osmRoutes)
                {
                    if (!ok) break;
                    while (j >= allRoutesGroupOsm.Count && j>0) j--;
                    if (allRoutesGroupOsm.Count == 0) break;
                    if (r.routeIdOsm == allRoutesGroupOsm[j])
                    {
                        
                        allRoutesGroupOsm.Remove(r.routeIdOsm);
                        if (allRoutesGroupOsm.Count == 0) break;
                        foreach (OsmRoute s in osmRoutes)
                        {
                            if (!ok) break;
                            if (s.routeVersionOSM == r.routeVersionOSM && s.routeIdOsm != r.routeIdOsm && s.routeIdOsm == allRoutesGroupOsm[0])
                            {
                                routesGroupOsm[i] = new List<string>();
                                routesGroupOsm[i].Add(r.routeIdOsm);
                                routesGroupOsm[i].Add(s.routeIdOsm);
                                
                                allRoutesGroupOsm.Remove(s.routeIdOsm);
                                j = 0;
                                ok = false;
                                break;
                            }
                        }
                    }
                }
            }

            return routesGroupOsm;
        }
    }
}
