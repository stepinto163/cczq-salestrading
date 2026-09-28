using OxyPlot;
using STC.Chart.Plugin.Converts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Chart.Plugin.OxyChart
{
    /// <summary>
    /// TenorDemo.xaml 的交互逻辑
    /// </summary>
    public partial class TenorDemo : UserControl
    {
        private Random r = new Random(13);
        DateTime dtBase = new DateTime(2020, 1, 20);
        public Window myWindow { get; set; }

        public TenorDemo()
        {
            InitializeComponent();

            tenorChart.CurrenctDateTime = dtBase;

            //init x
            List<TenorPoint> list = new List<TenorPoint>();
            List<TenorPoint> bidList = new List<TenorPoint>();
            List<TenorPoint> ofrList = new List<TenorPoint>();
            int[] months = { 0, 1, 2, 3, 6, 9, 12 };
            foreach (int item in months)
            {
                list.Add(new TenorPoint(item, TenorUnitEnum.Month));
            }
            int[] years = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30, 40, 50 };
            foreach (int item in years)
            {
                list.Add(new TenorPoint(item, TenorUnitEnum.Year));
            }

            bidList.AddRange(list.ToArray());
            ofrList.AddRange(list.ToArray());

            //init y
            double trend = 4.5;
            for (int j = 0; j < 4; j++)
            {
                trend = 4.5;
                for (int i = 0; i < list.Count; i++)
                {
                    int sign = 1;
                    trend += sign * r.Next(0, 10) * 0.001;
                    list[i].Value = trend;
                    if (j == 2)
                    {
                        bidList[i].Value = list[i].Value - sign * r.Next(0, 10) * 0.0005;
                    }
                }

                tenorChart.AddLineSeries("Series " + (j + 1), list);
                if (j == 2)
                {
                    tenorChart.AddScatterSeries("Bid", OxyColors.Orange, MarkerType.Triangle, bidList);
                }

            }

            trend = 4.5;
            for (int i = 0; i < list.Count; i++)
            {
                int sign = 1;
                ofrList[i].Value = list[i].Value - sign * r.Next(0, 10) * 0.0005;
            }


            tenorChart.AddScatterSeries("Ofr", OxyColors.Blue, MarkerType.Triangle, ofrList);
        }

        public void SetMyWindow(object mywin)
        {
            this.myWindow = (Window)mywin;
        }

        private void btnShutDown_Click(object sender, RoutedEventArgs e)
        {
            if (myWindow != null)
            {
                myWindow.Close();
                GC.Collect();
            }
        }
    }
}
