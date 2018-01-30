using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace TransportClasses
{
    public class SimpleTimetable
    {
        public readonly string stationCode, routeCode;
        public readonly TableType type;
        public readonly ObservableCollection<SimpleTable> table;
        public readonly ObservableCollection<SimpleSpetialTableForDay> spetial;
        public SimpleTimetable(Timetable timetable)
        {
            //throw new NotImplementedException();
            stationCode = timetable.stationCode;
            routeCode = timetable.routeCode;
            //this.timetable = timetable;
            type = timetable.type;


            ObservableCollection<Table> oldTable = timetable.table;
            if (oldTable == null) table = null;
            else
            {
                table = new ObservableCollection<SimpleTable>();

                foreach (Table t in oldTable)
                {
                    table.Add(new SimpleTable(t));
                }
            }


            ObservableCollection<SpetialTableForDay> oldSpetialTableTable = timetable.spetial;
            if (oldTable == null) spetial = null;
            else
            {
                spetial = new ObservableCollection<SimpleSpetialTableForDay>();

                foreach (SpetialTableForDay t in oldSpetialTableTable)
                {
                    spetial.Add(new SimpleSpetialTableForDay(t));
                }
            }
        }
    }



    [Serializable]
    public class SimpleTable
    {
        public int[] days;
        public List<int> times;
        public SimpleTable(Table table)
        {
            days = new int[7];
            days.Initialize();
            foreach (DayOfWeek day in table.days) days[(int)day] = 1;
            times = new List<int>();
            int previousSeconds = -1;
            bool needAddDay = false;
            foreach (SimpleTime st in table.times)
            {
                int seconds = st.hour * 3600 + st.minute * 60;
                if (seconds < previousSeconds) needAddDay = true;
                if (needAddDay) seconds += 86400;
                else previousSeconds = seconds;
                times.Add(seconds);

            }
        }
    }
    [Serializable]
    public class SimpleSpetialTableForDay
    {
        public List<string> days; //example:  01.01.1970
        public List<int> times;
        public SimpleSpetialTableForDay(SpetialTableForDay spetialTable)
        {
            days = spetialTable.days;
            times = new List<int>();
            int previousSeconds = -1;
            bool needAddDay = false;
            foreach (SimpleTime st in spetialTable.times)
            {
                int seconds = st.hour * 3600 + st.minute * 60;
                if (seconds < previousSeconds) needAddDay = true;
                if (needAddDay) seconds += 86400;
                else previousSeconds = seconds;
                times.Add(seconds);
            }
        }
    }
    [Serializable]
    public enum TableType { table = 1, periodic = 2 }
    [Serializable]
    public class SimpleTime
    {
        public int hour, minute;
        public const int DAY_BEGIN_HOUR = 4;
        public SimpleTime(int h, int m)
        {
            if (h >= 0 && h <= 23 && m >= 0 && m <= 59)
            {
                hour = h;
                minute = m;
            }
            else throw new FormatException("Некорректное время");
        }
        public override string ToString()
        {
            return hour + ":" + minute;
        }
        public static bool operator ==(SimpleTime a, SimpleTime b)
        {
            return (a.hour == b.hour && a.minute == b.minute);
        }
        public static bool operator !=(SimpleTime a, SimpleTime b)
        {
            return !(a == b);
        }
        public static bool operator >(SimpleTime a, SimpleTime b)
        {
            int ah = a.hour < DAY_BEGIN_HOUR ? a.hour + 24 : a.hour;
            int bh = b.hour < DAY_BEGIN_HOUR ? b.hour + 24 : b.hour;
            return (ah > bh || ah == bh && a.minute > b.minute);
        }
        public static bool operator <(SimpleTime a, SimpleTime b)
        {
            return b > a;
        }
        public static bool operator >=(SimpleTime a, SimpleTime b)
        {
            return (a == b || a > b);
        }
        public static bool operator <=(SimpleTime a, SimpleTime b)
        {
            return b >= a;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    [Serializable]
    public class Table
    {
        public List<DayOfWeek> days;
        public ObservableCollection<SimpleTime> times;
        public Table(List<DayOfWeek> days, ObservableCollection<SimpleTime> times)
        {
            this.days = days;
            this.times = times;
        }
        public Table()
        {
            days = new List<DayOfWeek>();
            times = new ObservableCollection<SimpleTime>();
        }
        public override string ToString()
        {
            if (days.Count == 0) return "(нет элементов в коллекции)";
            StringBuilder tmp = new StringBuilder();
            foreach (DayOfWeek d in days) tmp.Append(d.ToString() + ", ");
            tmp.Remove(tmp.Length - 2, 2);
            return tmp.ToString();
        }
    }
    [Serializable]
    public class SpetialTableForDay
    {
        public List<string> days; //example:  01.01.1970
        public ObservableCollection<SimpleTime> times;
        public SpetialTableForDay(List<string> days, ObservableCollection<SimpleTime> times)
        {
            DateTime tmp;
            foreach (string s in days) if (DateTime.TryParse(s, out tmp) == false) throw new FormatException("Некорректная дата.");
            this.days = days;
            this.times = times;
        }
        public SpetialTableForDay()
        {
            days = new List<string>();
            times = new ObservableCollection<SimpleTime>();
        }
        public override string ToString()
        {
            if (days.Count == 0) return "(нет элементов в коллекции)";
            StringBuilder tmp = new StringBuilder();
            foreach (string d in days) tmp.Append(d + ", ");
            tmp.Remove(tmp.Length - 2, 2);
            return tmp.ToString();
        }
    }

    [Serializable]
    public class Timetable
    {
        /*
        public enum TableType { table = 1, periodic = 2 }
        public class SimpleTime
        {
            public int hour, minute;
            public const int DAY_BEGIN_HOUR = 4;
            public SimpleTime(int h, int m)
            {
                if (h >= 0 && h <= 23 && m >= 0 && m <= 59)
                {
                    hour = h;
                    minute = m;
                }
                else throw new FormatException("Некорректное время");
            }
            public override string ToString()
            {
                return hour + ":" + minute;
            }
            public static bool operator ==(SimpleTime a, SimpleTime b)
            {
                return (a.hour == b.hour && a.minute == b.minute);
            }
            public static bool operator !=(SimpleTime a, SimpleTime b)
            {
                return !(a == b);
            }
            public static bool operator >(SimpleTime a, SimpleTime b)
            {
                int ah = a.hour < DAY_BEGIN_HOUR ? a.hour + 24 : a.hour;
                int bh = b.hour < DAY_BEGIN_HOUR ? b.hour + 24 : b.hour;
                return (ah > bh || ah == bh && a.minute > b.minute);
            }
            public static bool operator <(SimpleTime a, SimpleTime b)
            {
                return b > a;
            }
            public static bool operator >=(SimpleTime a, SimpleTime b)
            {
                return (a == b || a > b);
            }
            public static bool operator <=(SimpleTime a, SimpleTime b)
            {
                return b >= a;
            }
        }
        public class Table
        {
            public List<DayOfWeek> days;
            public ObservableCollection<SimpleTime> times;
            public Table(List<DayOfWeek> days, ObservableCollection<SimpleTime> times)
            {
                this.days = days;
                this.times = times;
            }
            public Table()
            {
                days = new List<DayOfWeek>();
                times = new ObservableCollection<SimpleTime>();
            }
            public override string ToString()
            {
                if (days.Count == 0) return "(нет элементов в коллекции)";
                StringBuilder tmp = new StringBuilder();
                foreach (DayOfWeek d in days) tmp.Append(d.ToString() + ", ");
                tmp.Remove(tmp.Length - 2, 2);
                return tmp.ToString();
            }
        }
        public class SpetialTableForDay
        {
            public List<string> days; //example:  01.01.1970
            public ObservableCollection<SimpleTime> times;
            public SpetialTableForDay(List<string> days, ObservableCollection<SimpleTime> times)
            {
                DateTime tmp;
                foreach (string s in days) if (DateTime.TryParse(s, out tmp) == false) throw new FormatException("Некорректная дата.");
                this.days = days;
                this.times = times;
            }
            public SpetialTableForDay()
            {
                days = new List<string>();
                times = new ObservableCollection<SimpleTime>();
            }
            public override string ToString()
            {
                if (days.Count == 0) return "(нет элементов в коллекции)";
                StringBuilder tmp = new StringBuilder();
                foreach (string d in days) tmp.Append(d + ", ");
                tmp.Remove(tmp.Length - 2, 2);
                return tmp.ToString();
            }
        }

        */
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public string stationCode = null, routeCode = null;


        public /*readonly*/ TableType type;
        public /*readonly*/ ObservableCollection<Table> table;
        public /*readonly*/ ObservableCollection<SpetialTableForDay> spetial;
        public Timetable(string stationCode, string routeCode, TableType myType, ObservableCollection<Table> t, ObservableCollection<SpetialTableForDay> spetial = null)
        {
            this.stationCode = stationCode;
            this.routeCode = routeCode;
            if (spetial != null)
            {
            metka2:
                foreach (SpetialTableForDay s in spetial)
                {
                metka1: //MessageBox.Show("del");
                    foreach (string str in s.days) if (DateTime.Parse(str) < DateTime.Now)
                        {
                            //MessageBox.Show("del");
                            s.days.Remove(str);
                            goto metka1;
                        }
                    if (s.days.Count == 0)
                    {
                        spetial.Remove(s);
                        goto metka2;
                    }
                }

                this.spetial = spetial;
            }
            bool[] valid = new bool[7];
            valid.Initialize();
            if (t != null)
            {
                foreach (Table tr in t)
                {
                    foreach (DayOfWeek d in tr.days)
                    {
                        if (!valid[(int)d])
                        {
                            valid[(int)d] = true;
                        }
                        else throw new FormatException("В расписании повторяются дни.");
                    }
                }

                type = myType;
                table = t;
            }
            else throw new Exception();
        }


        public ObservableCollection<DayOfWeek> GetFreeDays()
        {
            ObservableCollection<DayOfWeek> ttt = new ObservableCollection<DayOfWeek>();
            bool[] valid = new bool[7];
            valid.Initialize();
            if (table != null)
            {
                foreach (Table tr in table)
                {
                    foreach (DayOfWeek d in tr.days)
                    {
                        if (!valid[(int)d])
                        {
                            valid[(int)d] = true;
                        }
                    }
                }
            }

            for (int i = 0; i < 7; i++) if (!valid[i]) ttt.Add((DayOfWeek)i);
            return ttt;
        }




        public static Timetable CreateStandart()
        {
            List<DayOfWeek> workDays = new List<DayOfWeek>();
            List<DayOfWeek> weekDays = new List<DayOfWeek>();
            ObservableCollection<SimpleTime> times = new ObservableCollection<SimpleTime>();
            times.Add(new SimpleTime(6, 0));
            workDays.Add(DayOfWeek.Monday);
            //workDays.Add(DayOfWeek.Sunday);
            weekDays.Add(DayOfWeek.Saturday);
            weekDays.Add(DayOfWeek.Sunday);
            Table tr = new Table(workDays, times);
            Table tr2 = new Table(weekDays, times);
            ObservableCollection<Table> tt = new ObservableCollection<Table>();
            tt.Add(tr);
            tt.Add(tr2);

            ObservableCollection<SpetialTableForDay> spetial = new ObservableCollection<SpetialTableForDay>();
            List<string> dates = new List<string>();
            dates.Add("09.09.2017");
            dates.Add("09.01.2016");
            spetial.Add(new SpetialTableForDay(dates, times));

            TableType myType = TableType.table;

            Timetable t = new Timetable(null, null, myType, tt, spetial);
            return t;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">Время, когда мы прибудем на станцию.</param>
        /// <returns></returns>
        public TimeSpan FindTimeAfter(DateTime time)
        {
            foreach (Table t in table)
            {
                if (t.days.Contains(time.DayOfWeek))
                {

                    foreach (SimpleTime st in t.times)
                    {
                        //MessageBox.Show("Проверяем: прибытие в " + TimeSpan.FromMinutes(st.hour * 60 + st.minute).ToString()+", мы в "+ TimeSpan.FromSeconds(time.Second + time.Minute * 60 + time.Hour * 3600).ToString());
                        //
                        if (TimeSpan.FromMinutes(st.hour * 60 + st.minute) >= TimeSpan.FromSeconds(time.Second + time.Minute * 60 + time.Hour * 3600)/*time.Hour >= st.hour && time.Minute >= st.minute*/)
                        {
                            //MessageBox.Show("Ближайшее время: " + st.ToString());
                            return TimeSpan.FromMinutes(st.hour * 60 + st.minute) - TimeSpan.FromSeconds(time.Second + time.Minute * 60 + time.Hour * 3600);
                        }
                    }
                    if (t.times.Count != 0) return TimeSpan.FromMinutes(t.times[0].hour * 60 + t.times[0].minute) - TimeSpan.FromSeconds(time.Second + time.Minute * 60 + time.Hour * 3600) + TimeSpan.FromDays(1);
                    break;
                }
            }
            return TimeSpan.FromDays(25000);
            //throw new Exception();
        }
        public TimeSpan FindTimeBefore(DateTime time)
        {
            foreach (Table t in table)
            {
                if (t.days.Contains(time.DayOfWeek))
                {
                    bool ok = false;
                    SimpleTime st = null;
                    foreach (SimpleTime stt in t.times)
                    {
                        //MessageBox.Show("Проверяем: прибытие в " + TimeSpan.FromMinutes(st.hour * 60 + st.minute).ToString()+", мы в "+ TimeSpan.FromSeconds(time.Second + time.Minute * 60 + time.Hour * 3600).ToString());
                        //
                        if (TimeSpan.FromMinutes(stt.hour * 60 + stt.minute) <= TimeSpan.FromSeconds(time.Second + time.Minute * 60 + time.Hour * 3600)/*time.Hour >= st.hour && time.Minute >= st.minute*/)
                        {
                            ok = true;
                            st = stt;
                            //MessageBox.Show("Ближайшее время: " + st.ToString());
                        }
                        else break;
                    }
                    if (ok) return TimeSpan.FromMinutes(st.hour * 60 + st.minute) - TimeSpan.FromSeconds(time.Second + time.Minute * 60 + time.Hour * 3600);
                    if (t.times.Count != 0) return TimeSpan.FromMinutes(t.times[0].hour * 60 + t.times[0].minute) - TimeSpan.FromSeconds(time.Second + time.Minute * 60 + time.Hour * 3600) - TimeSpan.FromDays(1);
                    break;
                }
            }
            return TimeSpan.FromDays(0/*-25000*/);
            //throw new Exception();
        }



        public string Serialize()
        {
            string type = JsonConvert.SerializeObject(this.type);
            List<List<string>> group_days = new List<List<string>>();
            List<List<string>> group_times = new List<List<string>>();
            if (table != null)
            {
                foreach (Table t in this.table)
                {
                    List<string> days = new List<string>();
                    foreach (DayOfWeek d in t.days) days.Add(JsonConvert.SerializeObject(d));
                    group_days.Add(days);
                    List<string> times = new List<string>();
                    foreach (SimpleTime s in t.times) times.Add(JsonConvert.SerializeObject(s));
                    group_times.Add(times);
                }
            }
            List<List<string>> spetial_group_days = new List<List<string>>();
            List<List<string>> spetial_group_times = new List<List<string>>();
            if (spetial != null)
            {
                foreach (SpetialTableForDay t in this.spetial)
                {
                    List<string> spetial_days = new List<string>();
                    foreach (string d in t.days) spetial_days.Add(JsonConvert.SerializeObject(d));
                    spetial_group_days.Add(spetial_days);
                    List<string> spetial_times = new List<string>();
                    foreach (SimpleTime s in t.times) spetial_times.Add(JsonConvert.SerializeObject(s));
                    spetial_group_times.Add(spetial_times);
                }
            }
            return (JsonConvert.SerializeObject(new PrimitiveTimetable(type, group_days, group_times, spetial_group_days, spetial_group_times)));
        }


        public static Timetable Deserialize(string json_text)
        {
            PrimitiveTimetable pt = JsonConvert.DeserializeObject<PrimitiveTimetable>(json_text);
            TableType type = JsonConvert.DeserializeObject<TableType>(pt.type);
            ObservableCollection<Table> table = new ObservableCollection<Table>();
            ObservableCollection<SpetialTableForDay> spetial = new ObservableCollection<SpetialTableForDay>();


            for (int i = 0; i < pt.days.Count; i++)
            {
                List<DayOfWeek> days = new List<DayOfWeek>();
                foreach (string s in pt.days[i]) days.Add(JsonConvert.DeserializeObject<DayOfWeek>(s));

                ObservableCollection<SimpleTime> times = new ObservableCollection<SimpleTime>();
                foreach (string s in pt.times[i]) times.Add(JsonConvert.DeserializeObject<SimpleTime>(s));

                table.Add(new Table(days, times));
            }


            for (int i = 0; i < pt.spetial_days.Count; i++)
            {
                List<string> spetial_days = new List<string>();
                foreach (string s in pt.spetial_days[i]) spetial_days.Add(JsonConvert.DeserializeObject<string>(s));

                ObservableCollection<SimpleTime> spetial_times = new ObservableCollection<SimpleTime>();
                foreach (string s in pt.spetial_times[i]) spetial_times.Add(JsonConvert.DeserializeObject<SimpleTime>(s));

                spetial.Add(new SpetialTableForDay(spetial_days, spetial_times));
            }

            Timetable result = new Timetable(null, null, type, table, spetial);

            /*string test = "{\"type\":1,\"table\":[{\"days\":[1,2,3,4,5],\"times\":[]},{\"days\":[6,0],\"times\":[]}],\"spetial\":null}";
            Timetable test_tmp = JsonConvert.DeserializeObject<Timetable>(test);
            MessageBox.Show("ok!!! " + test_tmp.type.ToString());*/

            return result;
        }

        public static string SerializeFullTable(List<Timetable>[] fullTable)
        {
            List<string>[] resList = new List<string>[2];
            for (int i = 0; i <= 1; i++)
            {
                resList[i] = new List<string>();
                foreach (Timetable tab in fullTable[i]) resList[i].Add(tab.Serialize());
            }
            return JsonConvert.SerializeObject(resList);
        }
        public static List<Timetable>[] DeserializeFullTable(string fullTableJSON)
        {
            List<Timetable>[] fullTable = new List<Timetable>[2];
            List<string>[] resList = JsonConvert.DeserializeObject<List<string>[]>(fullTableJSON);
            for (int i = 0; i <= 1; i++)
            {
                fullTable[i] = new List<Timetable>();
                foreach (string s in resList[i]) fullTable[i].Add(Deserialize(s));
            }
            return fullTable;
        }

    }
    public class PrimitiveTimetable
    {
        public string type;
        public List<List<string>> days;
        public List<List<string>> times;
        public List<List<string>> spetial_days;
        public List<List<string>> spetial_times;
        public PrimitiveTimetable(string type, List<List<string>> days, List<List<string>> times, List<List<string>> spetial_days, List<List<string>> spetial_times)
        {
            this.days = days;
            this.spetial_days = spetial_days;
            this.spetial_times = spetial_times;
            this.times = times;
            this.type = type;
        }
    }
}
