using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransportClasses;

namespace EditTransportNetwork
{
    public partial class Form1 : Form
    {
        private bool isRouteAndStationSelected = false, isRouteSelected = false;
        private bool RouteChanged, StationChanged;
        public Form1()
        {
            InitializeComponent();
        }
        private List<Route> allRoutes;
        private List<Station> allStations;
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox6.SelectedIndex = 13;
            comboBox5.SelectedIndex = 1;

            //textBox21.Text = "53,6848";
            //textBox20.Text = "23,8402";

            ////////
            //LoadTimetable("asg", "sa");
            ////////

            RouteChanged = false;
            StationChanged = false;

            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
            listBox5.Items.Clear();

            
            allStations = Database.GetAllStations();

            listBox1.Items.AddRange(allStations.ToArray());
            listBox1.SelectedIndex = -1;


            allRoutes = Database.GetAllRoutes();

            listBox5.Items.AddRange(allRoutes.ToArray());
            listBox5.SelectedIndex = -1;

            if (textBox22.Text != string.Empty) textBox22_TextChanged(sender, e);
            if (textBox23.Text != string.Empty) textBox23_TextChanged(sender, e);

            webBrowser1.Url = new Uri("http://www.openstreetmap.org/#map=12/53.6733/23.8346");
            //webBrowser1.Url = new Uri("http://localhost:13425/");
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Station tmp = (listBox1.Items[listBox1.SelectedIndex] as Station);
                textBox1.Text = tmp.lat.ToString();
                textBox2.Text = tmp.lng.ToString();
                textBox3.Text = tmp.nameRus;
                textBox4.Text = tmp.nameBy;
                textBox5.Text = tmp.nameEn;
                textBox14.Text = tmp.hashcode;

                textBox30.Text = tmp.name;
                textBox25.Text = tmp.osm_id;
                textBox24.Text = tmp.osm_version;
                textBox26.Text = tmp.osm_timestamp;
                textBox27.Text = tmp.osm_changeset;
                textBox28.Text = tmp.osm_last_user_name;
                textBox29.Text = tmp.osm_last_user_id;

                
                webBrowser1.Url = new Uri("http://www.openstreetmap.org/#map=17/"+(tmp.lat+0.0001).ToString(System.Globalization.CultureInfo.InvariantCulture)+ "/" + (tmp.lng).ToString(System.Globalization.CultureInfo.InvariantCulture));

                /*HtmlDocument doc = webBrowser1.Document;
                HtmlElement head = doc.GetElementsByTagName("head")[0];
                HtmlElement s = doc.CreateElement("script");
                s.SetAttribute("text", "function sayHello() { var sss = document.getElementById('map'); sss.style.position = 'fixed'; sss.style.top = '0px';  sss.style['z-index'] = '0';  var aaa = document.getElementById('flash'); aaa.style.display = 'none';  var bbb = document.getElementById('sidebar'); bbb.style.display = 'none'; var ccc = document.getElementsByTagName('header')[0]; ccc.style.display = 'none'; }");
                head.AppendChild(s);
                webBrowser1.Document.InvokeScript("sayHello");*/

                listBox2.Items.Clear();
                //listBox2.Items.AddRange(tmp.routes.ToArray());
                listBox2.Items.AddRange(Database.GetRoutesOnStation(tmp.hashcode).ToArray());
            }
            catch { }
        }
        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (RouteChanged) MessageBox.Show("123");
            try
            {
                Route tmp = (listBox5.Items[listBox5.SelectedIndex] as Route);
                textBox10.Text = tmp.number == "0" && tmp.osm_num != string.Empty ? tmp.osm_num : tmp.number.ToString();
                textBox9.Text = tmp.type.ToString();
                textBox8.Text = tmp.rusFrom;
                textBox7.Text = tmp.rusTo;
                textBox11.Text = tmp.enFrom;
                textBox6.Text = tmp.enTo;
                textBox13.Text = tmp.byFrom;
                textBox12.Text = tmp.byTo;
                textBox15.Text = tmp.hashcode;

                textBox31.Text = tmp.osm_num;
                textBox33.Text = tmp.from;
                textBox32.Text = tmp.to;

                textBox38.Text = tmp.osm_id[0];
                textBox39.Text = tmp.osm_version[0];
                textBox36.Text = tmp.osm_timestamp[0];
                textBox37.Text = tmp.osm_changeset[0];
                textBox34.Text = tmp.osm_last_user_name[0];
                textBox35.Text = tmp.osm_last_user_id[0];

                textBox44.Text = tmp.osm_id[1];
                textBox45.Text = tmp.osm_version[1];
                textBox42.Text = tmp.osm_timestamp[1];
                textBox43.Text = tmp.osm_changeset[1];
                textBox40.Text = tmp.osm_last_user_name[1];
                textBox41.Text = tmp.osm_last_user_id[1];

                List<Station>[] stationsOnRoute = Database.GetStationsOnRoute(tmp.hashcode);

                label16.Text = tmp.rusFrom + " - " + tmp.rusTo + " (" + stationsOnRoute[0].Count + " остановок):";
                label17.Text = tmp.rusTo + " - " + tmp.rusFrom + " (" + stationsOnRoute[1].Count + " остановок):";

                
                listBox3.Items.Clear();
                //listBox3.Items.AddRange(tmp.stations[0].ToArray());
                listBox3.Items.AddRange(stationsOnRoute[0].ToArray());

                listBox4.Items.Clear();
                //listBox4.Items.AddRange(tmp.stations[1].ToArray());
                listBox4.Items.AddRange(stationsOnRoute[1].ToArray());

                isRouteSelected = true;
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Route tmp = (listBox2.Items[listBox2.SelectedIndex] as Route);
                Route s=null;
                foreach (Route r in listBox5.Items)
                {
                    if (r.hashcode == tmp.hashcode)
                    {
                        s = r;

                        label20.Text = tmp.GetName();
                        label21.Text = (listBox1.SelectedItem as Station).nameRus;

                        string StationCode = (listBox1.SelectedItem as Station).hashcode;
                        string RouteCode = r.hashcode;
                        LoadTimetable(StationCode, RouteCode);

                        break;
                    }
                }
                listBox5.SelectedItem = s;
                isRouteAndStationSelected = true;
            }
            catch { }
        }
        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Station tmp = (listBox3.Items[listBox3.SelectedIndex] as Station);
                Station s = null;
                foreach (Station r in listBox1.Items)
                {
                    if (r.hashcode == tmp.hashcode)
                    {
                        s = r;

                        label20.Text = (listBox5.SelectedItem as Route).GetName();
                        label21.Text = tmp.nameRus;

                        string StationCode = r.hashcode;
                        string RouteCode = (listBox5.SelectedItem as Route).hashcode;
                        LoadTimetable(StationCode, RouteCode);

                        break;
                    }
                }
                listBox1.SelectedItem = s;
                isRouteAndStationSelected = true;
            }
            catch { }
        }
        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Station tmp = (listBox4.Items[listBox4.SelectedIndex] as Station);
                Station s = null;
                foreach (Station r in listBox1.Items)
                {
                    if (r.hashcode == tmp.hashcode)
                    {
                        s = r;

                        label20.Text = (listBox5.SelectedItem as Route).GetName();
                        label21.Text = tmp.nameRus;

                        string StationCode = r.hashcode;
                        string RouteCode = (listBox5.SelectedItem as Route).hashcode;
                        LoadTimetable(StationCode, RouteCode);

                        break;
                    }
                }
                listBox1.SelectedItem = s;
                isRouteAndStationSelected = true;
            }
            catch { }
        }

        private DialogResult ConfirmDeletingElement(string param2)
        {
            return MessageBox.Show("Вы действительно хотите удалить элемент '" + param2 + "'?", "Подтвердите действие", MessageBoxButtons.YesNo);
        }
        private DialogResult ConfirmDeletingStation(string param2)
        {
            return MessageBox.Show("Вы действительно хотите удалить остановку '" + param2 + "'?", "Подтвердите действие", MessageBoxButtons.YesNo);
        }
        private DialogResult ConfirmDeletingRoute(string param2)
        {
            return MessageBox.Show("Вы действительно хотите удалить маршрут '" + param2 + "'?", "Подтвердите действие", MessageBoxButtons.YesNo);
        }
        private DialogResult ConfirmDeletingStationFromRoute(string param1, string param2)
        {
            return MessageBox.Show("Вы действительно хотите удалить остановку '" + param1 + "' из маршрута '" + param2 + "'?", "Подтвердите действие", MessageBoxButtons.YesNo);
        }
        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listBox1.SelectedIndex!=-1){
                Station tmp = (listBox1.Items[listBox1.SelectedIndex] as Station);
                DialogResult dialogResult = ConfirmDeletingStation(tmp.nameRus);
                if (dialogResult == DialogResult.Yes)
                {
                    //MessageBox.Show("Сейчас будет повторная попытка...");
                    Database.DeleteStation(tmp);
                    Form1_Load(sender, e);
                }
            }
        }
        private void listBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listBox5.SelectedIndex != -1)
            {
                Route tmp = (listBox5.Items[listBox5.SelectedIndex] as Route);
                DialogResult dialogResult = ConfirmDeletingRoute(tmp.GetName());
                if (dialogResult == DialogResult.Yes)
                {
                    //MessageBox.Show("Сейчас будет повторная попытка...");
                    Database.DeleteRoute(tmp);
                    Form1_Load(sender, e);
                }
            }

        }
        private void listBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listBox3.SelectedIndex != -1)
            {
                DialogResult dialogResult = ConfirmDeletingStationFromRoute((listBox3.Items[listBox3.SelectedIndex] as Station).nameRus, (listBox5.Items[listBox5.SelectedIndex] as Route).GetName());
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        listBox3.Items.Remove(listBox3.SelectedItem);
                        RouteChanged = true;
                        button6_Click(sender, e);
                    }
                    catch { }
                    //MessageBox.Show("Сейчас будет повторная попытка...");
                }
            }
            else if (e.KeyData == Keys.Add && listBox3.SelectedIndex > 0 && listBox3.SelectedIndex < listBox3.Items.Count)
            {
                try
                {
                    var tmp = listBox3.Items[listBox3.SelectedIndex];
                    int s = listBox3.SelectedIndex - 1;
                    listBox3.Items.Insert(s, tmp);
                    listBox3.Items.RemoveAt(listBox3.SelectedIndex);

                    //button6_Click(sender, e);
                    UpdateRoute(false);
                    listBox3.SelectedIndex = s;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.KeyData == Keys.Subtract && listBox3.SelectedIndex > -1 && listBox3.SelectedIndex < listBox3.Items.Count-1)
            {
                try
                {
                    var tmp = listBox3.Items[listBox3.SelectedIndex];
                    int s = listBox3.SelectedIndex + 2;
                    listBox3.Items.Insert(s, tmp);
                    listBox3.Items.RemoveAt(listBox3.SelectedIndex);

                    //button6_Click(sender, e);
                    UpdateRoute(false);
                    listBox3.SelectedIndex = s-1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void listBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listBox4.SelectedIndex != -1)
            {
                DialogResult dialogResult = ConfirmDeletingStationFromRoute((listBox4.Items[listBox4.SelectedIndex] as Station).nameRus, (listBox5.Items[listBox5.SelectedIndex] as Route).GetName(true));
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        listBox4.Items.Remove(listBox4.SelectedItem);
                        RouteChanged = true;
                        button6_Click(sender, e);
                    }
                    catch { }
                    //MessageBox.Show("Сейчас будет повторная попытка...");
                }
            }
            else if (e.KeyData == Keys.Add && listBox4.SelectedIndex > 0 && listBox4.SelectedIndex < listBox4.Items.Count)
            {
                try
                {
                    var tmp = listBox4.Items[listBox4.SelectedIndex];
                    int s = listBox4.SelectedIndex - 1;
                    listBox4.Items.Insert(s, tmp);
                    listBox4.Items.RemoveAt(listBox4.SelectedIndex);

                    //button6_Click(sender, e);
                    UpdateRoute(false);
                    listBox4.SelectedIndex = s;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.KeyData == Keys.Subtract && listBox4.SelectedIndex > -1 && listBox4.SelectedIndex < listBox4.Items.Count - 1)
            {
                try
                {
                    var tmp = listBox4.Items[listBox4.SelectedIndex];
                    int s = listBox4.SelectedIndex + 2;
                    listBox4.Items.Insert(s, tmp);
                    listBox4.Items.RemoveAt(listBox4.SelectedIndex);

                    //button6_Click(sender, e);
                    UpdateRoute(false);
                    listBox4.SelectedIndex = s - 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Station tmp = Database.CreateStation();
            Form1_Load(sender, e);
            foreach (Station s in (listBox1.Items))
            {
                if (s.hashcode == tmp.hashcode)
                {
                    listBox1.SelectedItem = s;
                    break;
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Route tmp = Database.CreateRoute();
            Form1_Load(sender, e);
            foreach (Route s in (listBox5.Items))
            {
                if (s.hashcode == tmp.hashcode)
                {
                    listBox5.SelectedItem = s;
                    break;
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                Station s = (listBox1.Items[listBox1.SelectedIndex] as Station);

                List<string> tmpCodes = new List<string>();
                if (s.routes != null) foreach (Route r in s.routes) tmpCodes.Add(r.hashcode);
                //string routesJSON = JsonConvert.SerializeObject(tmpCodes);

                Database.UpdateStation(s.hashcode, textBox3.Text, textBox4.Text, textBox5.Text, double.Parse(textBox1.Text), double.Parse(textBox2.Text)/*, routesJSON*/);

                Form1_Load(sender, e);
                foreach (Station t in (listBox1.Items))
                {
                    if (s.hashcode == t.hashcode)
                    {
                        listBox1.SelectedItem = t;
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void UpdateRoute(bool needUpdateForm = true)
        {
            Route s = (listBox5.Items[listBox5.SelectedIndex] as Route);

            List<string> tmpCodes1 = new List<string>();
            foreach (Station r in listBox3.Items) tmpCodes1.Add(r.hashcode);
            List<string> tmpCodes2 = new List<string>();
            foreach (Station r in listBox4.Items) tmpCodes2.Add(r.hashcode);
            List<string>[] tmpCodes = new List<string>[] { tmpCodes1, tmpCodes2 };

            string stationsJSON = JsonConvert.SerializeObject(tmpCodes);

            string rus = JsonConvert.SerializeObject(new string[] { textBox8.Text.Replace("\"",""), textBox7.Text.Replace("\"", "") });
            string en = JsonConvert.SerializeObject(new string[] { textBox11.Text.Replace("\"", ""), textBox6.Text.Replace("\"", "") });
            string by = JsonConvert.SerializeObject(new string[] { textBox13.Text.Replace("\"", ""), textBox12.Text.Replace("\"", "") });

            string number = textBox10.Text;
            string type = textBox9.Text;

            Database.UpdateRoute(s.hashcode, rus, en, by, stationsJSON, number, type);

            if(needUpdateForm) Form1_Load(null, null);
            foreach (Route t in (listBox5.Items))
            {
                if (s.hashcode == t.hashcode)
                {
                    listBox5.SelectedItem = t;
                    break;
                }
            }
        }
        private void ReverseStationsOnRoute(bool needUpdateForm = true)
        {
            Route s = (listBox5.Items[listBox5.SelectedIndex] as Route);

            List<string> tmpCodes1 = new List<string>();
            foreach (Station r in listBox3.Items) tmpCodes1.Add(r.hashcode);
            List<string> tmpCodes2 = new List<string>();
            foreach (Station r in listBox4.Items) tmpCodes2.Add(r.hashcode);
            List<string>[] tmpCodes = new List<string>[] { tmpCodes2, tmpCodes1 };

            string stationsJSON = JsonConvert.SerializeObject(tmpCodes);

            string rus = JsonConvert.SerializeObject(new string[] { textBox7.Text, textBox8.Text });
            string en = JsonConvert.SerializeObject(new string[] { textBox6.Text, textBox11.Text });
            string by = JsonConvert.SerializeObject(new string[] { textBox12.Text, textBox13.Text });

            string number = textBox10.Text;
            string type = textBox9.Text;

            Database.UpdateRoute(s.hashcode, rus, en, by, stationsJSON, number, type);

            if (needUpdateForm) Form1_Load(null, null);
            foreach (Route t in (listBox5.Items))
            {
                if (s.hashcode == t.hashcode)
                {
                    listBox5.SelectedItem = t;
                    break;
                }
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateRoute(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            RouteChanged = true;
            //DialogResult dialogResult = MessageBox.Show()
            try
            {
                if (/*listBox1.SelectedIndex!=-1 && listBox3.SelectedIndex!=-1*/ !listBox3.Items.Contains(listBox1.SelectedItem))
                {
                    listBox3.Items.Add(listBox1.SelectedItem);
                    button6_Click(sender, e);
                }
            }
            catch { }
        }
        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            RouteChanged = true;
        }
        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
        private void button4_Click(object sender, EventArgs e)
        {
            RouteChanged = true;
            try
            {
                if (/*listBox1.SelectedIndex!=-1 && listBox3.SelectedIndex!=-1*/ !listBox4.Items.Contains(listBox1.SelectedItem))
                {
                    listBox4.Items.Add(listBox1.SelectedItem);
                    button6_Click(sender, e);
                }
            }
            catch { }
        }





        Timetable tmpTimetable = null;
        ObservableCollection<SimpleTime> tmpList = null;
        private void listBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = listBox6.SelectedIndex;
            if (i != -1) try
                {
                    tmpList = tmpTimetable.table[i].times;

                    //listBox8.Items.Clear();
                    //foreach (DayOfWeek d in tmpTimetable.table[i].days) listBox8.Items.Add(d);
                    listBox8.DataSource = null;
                    listBox8.DataSource = tmpTimetable.table[i].days;

                    listBox7.DataSource = null;
                    listBox7.DataSource = tmpList;
                    //listBox7.Items.Clear();
                    //foreach (Timetable.SimpleTime d in tmpTimetable.table[i].times) listBox7.Items.Add(d);

                    label22.Text = "Time (обычные)";
                }
                catch { }
        }
        private void listBox6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listBox6.SelectedIndex != -1)
            {
                //Station tmp = (listBox1.Items[listBox1.SelectedIndex] as Station);
                DialogResult dialogResult = ConfirmDeletingElement(listBox6.Items[listBox6.SelectedIndex].ToString());
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        //listBox6.Items.RemoveAt(listBox6.SelectedIndex);
                        //tmpTimetable.table.Remove(listBox6.Items[listBox6.SelectedIndex] as Timetable.Table);
                        tmpTimetable.table.RemoveAt(listBox6.SelectedIndex);
                        listBox6.DataSource = null;
                        listBox6.DataSource = tmpTimetable.table;
                        comboBox2.DataSource = null;
                        comboBox2.DataSource = tmpTimetable.GetFreeDays();
                        listBox8.DataSource = null;
                        //listBox8.DataSource = tmpTimetable.table[listBox6.SelectedIndex].days;

                        tmpList = null;
                        listBox7.DataSource = null;

                        //MessageBox.Show("Сейчас будет повторная попытка...");
                        //Database.DeleteStation(tmp);
                        //Form1_Load(sender, e);
                    }
                    catch(Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tmpTimetable!=null) try
                {
                    tmpTimetable.type = (TableType)(comboBox1.SelectedIndex+1);
                }
                catch { }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (isRouteAndStationSelected) SaveTimetable();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //listBox6.Items.Add(new Table());
            tmpTimetable.table.Add(new Table());
            listBox6.DataSource = null;
            listBox6.DataSource = tmpTimetable.table;
        }

        private void listBox8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listBox6.SelectedIndex != -1 && listBox8.SelectedIndex != -1)
            {
                //Station tmp = (listBox1.Items[listBox1.SelectedIndex] as Station);
                DialogResult dialogResult = ConfirmDeletingElement(listBox8.Items[listBox8.SelectedIndex].ToString());
                if (dialogResult == DialogResult.Yes)
                {
                    //listBox8.Items.RemoveAt(listBox8.SelectedIndex);
                    tmpTimetable.table[listBox6.SelectedIndex].days.RemoveAt(listBox8.SelectedIndex);
                    listBox8.DataSource = null;
                    listBox8.DataSource = tmpTimetable.table[listBox6.SelectedIndex].days;
                    listBox6.DataSource = null;
                    listBox6.DataSource = tmpTimetable.table;
                    comboBox2.DataSource = null;
                    comboBox2.DataSource = tmpTimetable.GetFreeDays();

                    //MessageBox.Show("Сейчас будет повторная попытка...");
                    //Database.DeleteStation(tmp);
                    //Form1_Load(sender, e);
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex != -1 && listBox6.SelectedIndex!=-1)
            {
                tmpTimetable.table[listBox6.SelectedIndex].days.Add((DayOfWeek)comboBox2.SelectedItem);
                comboBox2.DataSource = null;
                comboBox2.DataSource = tmpTimetable.GetFreeDays();
                listBox6.DataSource = null;
                listBox6.DataSource = tmpTimetable.table;
                listBox8.Update();
            }
        }

        private void listBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = listBox9.SelectedIndex;
            if (i != -1) try
                {
                    tmpList = tmpTimetable.spetial[i].times;

                    listBox10.DataSource = null;
                    listBox10.DataSource = tmpTimetable.spetial[i].days;

                    listBox7.DataSource = null;
                    listBox7.DataSource = tmpList;
                }
                catch { }


            label22.Text = "Time (специальные)";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //MessageBox.Show((tmpList != null).ToString());
            //MessageBox.Show(tmpList.Count.ToString());
            if (comboBox3.SelectedIndex != -1 && comboBox4.SelectedIndex != -1) try
                {
                    //MessageBox.Show(int.Parse((string)comboBox3.SelectedItem).ToString());

                    SimpleTime time = new SimpleTime(int.Parse((string)comboBox3.SelectedItem), int.Parse((string)comboBox4.SelectedItem));

                    foreach (SimpleTime s in tmpList) if (s.hour == time.hour && s.minute == time.minute) throw new Exception("Повторяется время!");
                    
                    if (tmpTimetable.type == TableType.table) tmpList.Add(time);
                    else if (tmpTimetable.type == TableType.periodic && time.minute>0)
                    {
                        foreach (SimpleTime s in tmpList) if (s.hour == time.hour) throw new Exception("Повторяется время!");
                        tmpList.Add(time);
                    }
                    
                    listBox7.DataSource = null;
                    listBox7.DataSource = tmpList;
                }
                catch (Exception ex){
                    MessageBox.Show(ex.Message);
                }
        }

        private void listBox7_Enter(object sender, EventArgs e)
        {
            //MessageBox.Show("1232redf");
        }

        private void listBox7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listBox7.SelectedIndex != -1)
            {
                //Station tmp = (listBox1.Items[listBox1.SelectedIndex] as Station);
                DialogResult dialogResult = ConfirmDeletingElement(tmpList[listBox7.SelectedIndex].ToString());
                if (dialogResult == DialogResult.Yes)
                {
                    tmpList.RemoveAt(listBox7.SelectedIndex);

                    listBox7.DataSource = null;
                    listBox7.DataSource = tmpList;
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            tmpTimetable.spetial.Add(new SpetialTableForDay());
            listBox9.DataSource = null;
            listBox9.DataSource = tmpTimetable.spetial;
        }

        private void listBox9_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listBox9.SelectedIndex != -1)
            {
                DialogResult dialogResult = ConfirmDeletingElement(listBox9.Items[listBox9.SelectedIndex].ToString());
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        tmpTimetable.spetial.RemoveAt(listBox9.SelectedIndex);
                        listBox9.DataSource = null;
                        listBox9.DataSource = tmpTimetable.spetial;
                        listBox10.DataSource = null;

                        tmpList = null;
                        listBox7.DataSource = null;
                        
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (/*comboBox2.SelectedIndex != -1 && */listBox9.SelectedIndex != -1)
            {
                try
                {
                    int i = listBox9.SelectedIndex;
                    string date = dateTimePicker1.Value.Date.ToShortDateString();

                    foreach(SpetialTableForDay ts in tmpTimetable.spetial) foreach(string s in ts.days) if(s==date) throw new Exception("Повторяется дата!");

                    tmpTimetable.spetial[i].days.Add(date);
                    listBox9.DataSource = null;
                    listBox9.DataSource = tmpTimetable.spetial;
                    listBox10.Update();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void listBox10_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listBox9.SelectedIndex != -1 && listBox10.SelectedIndex != -1)
            {
                //Station tmp = (listBox1.Items[listBox1.SelectedIndex] as Station);
                DialogResult dialogResult = ConfirmDeletingElement(listBox10.Items[listBox10.SelectedIndex].ToString());
                if (dialogResult == DialogResult.Yes)
                {
                    //listBox8.Items.RemoveAt(listBox8.SelectedIndex);
                    tmpTimetable.spetial[listBox9.SelectedIndex].days.RemoveAt(listBox8.SelectedIndex);
                    listBox10.DataSource = null;
                    listBox10.DataSource = tmpTimetable.spetial[listBox9.SelectedIndex].days;
                    listBox9.DataSource = null;
                    listBox9.DataSource = tmpTimetable.spetial;

                    //MessageBox.Show("Сейчас будет повторная попытка...");
                    //Database.DeleteStation(tmp);
                    //Form1_Load(sender, e);
                }
            }
        }

        private void LoadTimetable(string StationCode, string RouteCode, bool isNeedSavePrevious = true)
        {
            if (tmpTimetable != null)
            {
                if (isNeedSavePrevious) SaveTimetable();

                tmpTimetable = null;
                tmpList = null;
            }

            //MessageBox.Show("first");

            textBox17.Text = StationCode;
            textBox16.Text = RouteCode;

            tmpTimetable = Database.GetTimetable(StationCode, RouteCode);

            //textBox46.Text = tmpTimetable.

            //MessageBox.Show("loaded");

            //comboBox2.Items.Clear();
            //MessageBox.Show(listBox7.Items.Count.ToString());
            listBox6.DataSource = null;
            listBox7.DataSource = null;
            listBox8.DataSource = null;
            listBox9.DataSource = null;
            listBox10.DataSource = null;

            listBox7.DataSource = tmpList;
            //MessageBox.Show(listBox7.Items.Count.ToString());
            listBox6.Items.Clear();
            listBox7.Items.Clear();
            listBox8.Items.Clear();
            listBox9.Items.Clear();
            listBox10.Items.Clear();

            //MessageBox.Show(listBox7.Items.Count.ToString());

            comboBox2.DataSource = null;
            comboBox2.DataSource = tmpTimetable.GetFreeDays();
            
            comboBox1.SelectedIndex = (int)tmpTimetable.type - 1;
                        
            listBox6.DataSource = tmpTimetable.table;
            listBox9.DataSource = tmpTimetable.spetial;

            listBox6.Update();
            listBox7.Update();
            listBox8.Update();
            listBox9.Update();
            listBox10.Update();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //DateTime t = new DateTime(0, 0, 0, comboBox6.SelectedIndex, comboBox5.SelectedIndex, 0);
            DateTime t = dateTimePicker2.Value;
            //mt (comboBox6.SelectedIndex);
            //mt.AddMinutes(comboBox5.SelectedIndex);
            DateTime mt = new DateTime(t.Year, t.Month, t.Day, comboBox6.SelectedIndex, comboBox5.SelectedIndex, 0);
            Form2 f = new Form2(new OptimalRoute.GeoCoords(double.Parse(textBox21.Text), double.Parse(textBox20.Text)), new OptimalRoute.GeoCoords(double.Parse(textBox19.Text), double.Parse(textBox18.Text)), mt);
            f.ShowDialog();
        }

        private string initialDirectory = @"D:\Files\Other\Projects\PublicTransport\Converters\GrodnoBusTimetable\GrodnoBusTimetableConverterResults";//AppDomain.CurrentDomain.BaseDirectory;
        private void button14_Click(object sender, EventArgs e)
        {
            if (isRouteAndStationSelected)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Выберите файл с расписанием";
                ofd.Filter = "Расписание в формате json|*.json";
                ofd.InitialDirectory = initialDirectory;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    initialDirectory = ofd.FileName.Replace(@"\"+ofd.SafeFileName,"");
                    Database.SetTimetable(ofd.FileName, textBox17.Text, textBox16.Text);
                    //MessageBox.Show("sended...");
                    LoadTimetable(textBox17.Text, textBox16.Text, false);
                }
            }
        }

        private void button15_Click(object sender, EventArgs e) // Добавить полное расписание маршрута
        {
            if (isRouteSelected)
            {
                try
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Title = "Выберите файл с полным расписанием маршрута";
                    ofd.Filter = "Расписание в формате json|*.json";
                    ofd.InitialDirectory = initialDirectory;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        initialDirectory = ofd.FileName.Replace(@"\" + ofd.SafeFileName, "");
                        Database.SetFullTimetableForRoute(ofd.FileName, textBox15.Text);
                        MessageBox.Show("Расписание успешно загружено.");
                        //LoadTimetable(textBox17.Text, textBox16.Text, false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void textBox23_TextChanged(object sender, EventArgs e) // Быстрый поиск маршрута
        {
            List<Route> findedRoutes = new List<Route>();
            foreach (Route r in allRoutes) if (Regex.IsMatch(r.ToString(), textBox23.Text, RegexOptions.IgnoreCase)) findedRoutes.Add(r);
            listBox5.Items.Clear();
            listBox5.Items.AddRange(findedRoutes.ToArray());
            listBox5.SelectedIndex = -1;
        }

        private void textBox22_TextChanged(object sender, EventArgs e) // Быстрый поиск остановки
        {
            List<Station> findedStations = new List<Station>();
            foreach (Station s in allStations) if (Regex.IsMatch(s.name, textBox22.Text, RegexOptions.IgnoreCase) || Regex.IsMatch(s.nameRus, textBox22.Text, RegexOptions.IgnoreCase)) findedStations.Add(s);
            listBox1.Items.Clear();
            listBox1.Items.AddRange(findedStations.ToArray());
            listBox1.SelectedIndex = -1;
        }

        bool firstLoadedBrowser = true;

        private void button16_Click(object sender, EventArgs e)
        {
            if (isRouteSelected)
            {
                ReverseStationsOnRoute(true);
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (firstLoadedBrowser)
            {
                HtmlDocument doc = webBrowser1.Document;
                HtmlElement head = doc.GetElementsByTagName("head")[0];
                HtmlElement s = doc.CreateElement("script");
                //s.SetAttribute("text", "function sayHello() { var sss = document.getElementById('map'); sss.style.position = 'fixed'; sss.style.top = '0px'; /*var aaa = document.getElementById('flash'); aaa.style.display = 'none';  var bbb = document.getElementById('sidebar'); bbb.style.display = 'none';*/ var ccc = document.getElementsByTagName('header')[0]; ccc.style.display = 'none'; }");
                s.SetAttribute("text", "function sayHello() { var sss = document.getElementById('map'); sss.style.position = 'fixed'; sss.style.top = '0px'; sss.style.width = '100vw'; sss.style.height = '100vh'; /*var aaa = document.getElementById('flash'); aaa.style.display = 'none';*/  var bbb = document.getElementById('sidebar'); bbb.style.display = 'none'; var ccc = document.getElementsByTagName('header')[0]; ccc.style.display = 'none';}");
                head.AppendChild(s);
                //webBrowser1.Document.InvokeScript("sayHello");

                firstLoadedBrowser = false;
            }
        }

        private void SaveTimetable()
        {
            Database.UpdateTimetable(textBox17.Text, textBox16.Text, tmpTimetable /*new Timetable(tmpTimetable.type, tmpTimetable.table, tmpTimetable.spetial)*/);
        }


    }
}
