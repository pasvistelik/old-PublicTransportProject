using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransportClasses;

namespace EditTransportNetwork
{
    static class Program
    {
        /*public struct S
        {
            public object a, b;
        }*/
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            while (true)
            {

                try
                {
                    Database.Connect();

                    //RefreshData();

                    //List<Station> tmp = Database.GetAllStations();

                    /*List<string> t = new List<string>();
                    t.Add("first"); t.Add("second");
                    Group g = new Group(t, t);

                    S s = new S();
                    s.a = t;
                    s.b = t;

                    object[] o = new object[] { t,t };*/

                    //Database.GetTimetable("asg", "sa");


                    


                    /*List<DayOfWeek> workDays = new List<DayOfWeek>();
                    List<DayOfWeek> weekDays = new List<DayOfWeek>();
                    List<Timetable.SimpleTime> times = new List<Timetable.SimpleTime>();
                    workDays.Add(DayOfWeek.Monday);
                    //workDays.Add(DayOfWeek.Sunday);
                    weekDays.Add(DayOfWeek.Saturday);
                    weekDays.Add(DayOfWeek.Sunday);
                    Timetable.Table tr = new Timetable.Table(workDays, times);
                    Timetable.Table tr2 = new Timetable.Table(weekDays, times);
                    List<Timetable.Table> tt = new List<Timetable.Table>();
                    tt.Add(tr);
                    tt.Add(tr2);

                    List<Timetable.SpetialTableForDay> spetial = new List<Timetable.SpetialTableForDay>();
                    List<string> dates = new List<string>();
                    dates.Add("09.09.2017");
                    dates.Add("09.01.2016");
                    spetial.Add(new Timetable.SpetialTableForDay(dates, times));*/

                    //Timetable t = Timetable.CreateStandart();

                    //string str = JsonConvert.SerializeObject(t);
                    //MessageBox.Show(str);

                    /*List<string> s = new List<string>();
                    List<string> s2 = new List<string>();
                    s.Add("first");
                    s.Add("asd");
                    List<string>[] o = new List<string>[]{ s, s2};
                    string str = JsonConvert.SerializeObject(o);
                    MessageBox.Show(str);
                    List<string>[] tmp = JsonConvert.DeserializeObject<List<string>[]>(str);
                    foreach (string x in tmp[1]) MessageBox.Show(x);*/

                    // // UPDATE `transport`.`routes` SET `stations` = REPLACE( `stations`, '{', '[');

                    try
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new Form1());
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message,"Fatal error!");
                    }


                    Database.Disconnect();


                    break;
                }
                catch
                {
                    DialogResult dialogResult = MessageBox.Show("Не удается подключиться к БД. Выполнить повторную попытку?", "Ошибка подключения к БД", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //MessageBox.Show("Сейчас будет повторная попытка...");
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        Application.Exit();
                        break;
                    }

                    continue;
                }
            }






            //MessageBox.Show("123");
        }


    }
}
