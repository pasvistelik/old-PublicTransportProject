using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransportClasses;

namespace EditTransportNetwork
{
    public partial class Form2 : Form
    {
        OptimalRoute.GeoCoords nowPos, needPos;
        DateTime time;
        public Form2(OptimalRoute.GeoCoords nowPos, OptimalRoute.GeoCoords needPos, DateTime time)
        {
            InitializeComponent();
            this.nowPos = nowPos;
            this.needPos = needPos;
            this.time = time;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Dictionary<OptimalRoute.Priority, double> priorities = new Dictionary<OptimalRoute.Priority, double>();
            priorities.Add(OptimalRoute.Priority.MinimalTime, 1);

            Database.Connect();
            OptimalRoute route = /*new OptimalRoute*/OptimalRoute.GetBestOptimalRoute(nowPos, needPos, time, priorities);

            listBox1.DataSource = route.points;

            //MessageBox.Show(Database.GetStationsAround(nowPos, 644).Count.ToString());
            //MessageBox.Show(Database.GetStationsAround(nowPos, 655).Count.ToString());
            //MessageBox.Show(Database.GetStationsAround(nowPos, 1770).Count.ToString());
            //MessageBox.Show(Database.GetStationsAround(nowPos, 815).Count.ToString());
            //MessageBox.Show(Database.GetStationsAround(nowPos, 950).Count.ToString());
            //MessageBox.Show(Database.GetStationsAround(nowPos, 2127).Count.ToString());
            //MessageBox.Show(Database.GetStationsAround(nowPos, OptimalRoute.GeoCoords.Distance(nowPos, needPos)).Count.ToString());
            //MessageBox.Show(time.ToString());
            //MessageBox.Show(OptimalRoute.GeoCoords.Distance(a,b).ToString());
        }
    }
}
