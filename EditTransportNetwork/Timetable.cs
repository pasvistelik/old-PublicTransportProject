using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditTransportNetwork
{
    class Timetable
    {
        public enum TableType { table = 1, periodic = 2}
        public class SimpleTime
        {
            public /*readonly*/ int hour, minute;
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
                return hour+":"+minute;
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
                if(days.Count == 0) return "(нет элементов в коллекции)";
                StringBuilder tmp = new StringBuilder();
                foreach (DayOfWeek d in days) tmp.Append(d.ToString()+", ");
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

        public /*readonly*/ TableType type;
        public /*readonly*/ ObservableCollection<Table> table;
        public /*readonly*/ ObservableCollection<SpetialTableForDay> spetial;
        public Timetable(TableType myType, ObservableCollection<Table> t, ObservableCollection<SpetialTableForDay> spetial = null)
        {
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
            ObservableCollection<Timetable.SimpleTime> times = new ObservableCollection<Timetable.SimpleTime>();
            times.Add(new SimpleTime(6, 0));
            workDays.Add(DayOfWeek.Monday);
            //workDays.Add(DayOfWeek.Sunday);
            weekDays.Add(DayOfWeek.Saturday);
            weekDays.Add(DayOfWeek.Sunday);
            Timetable.Table tr = new Timetable.Table(workDays, times);
            Timetable.Table tr2 = new Timetable.Table(weekDays, times);
            ObservableCollection<Timetable.Table> tt = new ObservableCollection<Timetable.Table>();
            tt.Add(tr);
            tt.Add(tr2);

            ObservableCollection<Timetable.SpetialTableForDay> spetial = new ObservableCollection<Timetable.SpetialTableForDay>();
            List<string> dates = new List<string>();
            dates.Add("09.09.2017");
            dates.Add("09.01.2016");
            spetial.Add(new Timetable.SpetialTableForDay(dates, times));

            TableType myType = TableType.table;

            Timetable t = new Timetable(myType, tt, spetial);
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
    }
}
