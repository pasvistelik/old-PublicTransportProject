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
    public class TimetablesController : ApiController
    {
        private static List<Timetable> allTimetables = null;
        private static List<SimpleTimetable> allSimpleTimetables = null;
        // GET: api/Stations
        [HttpGet, AllowAnonymous, Route("api/Timetables")]
        public IEnumerable<SimpleTimetable> Get()
        {
            if (allSimpleTimetables != null) return allSimpleTimetables;
            try
            {
                Database.TryInitialize();
                //Database.Connect();!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (allSimpleTimetables == null || allTimetables.Count != allSimpleTimetables.Count)
                {
                    if (allTimetables == null) allTimetables = Database.GetAllTimetablesDictinary(canReadDataFromLocalCopy: true).Values.ToList();
                    allSimpleTimetables = new List<SimpleTimetable>();
                    foreach (Timetable s in allTimetables) if (s != null) allSimpleTimetables.Add(new SimpleTimetable(s));
                }
                return allSimpleTimetables;
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
    }
}
