using OxyPlot;
using STC.Chart.Plugin.Converts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// ChartDemo.xaml 的交互逻辑
    /// </summary>
    public partial class ChartDemo : UserControl
    {
        private Random r = new Random(13);

        int numberOfPoints = 40;
        DateTime dtBase = new DateTime(2020, 1, 1);
        int seriesCount = 4;
        double[] trend = { 4.5, 4, 3.5, 3 };
        double[] upDownRatio = { 0.45, 0.44, 0.5, 0.54 };
        Dictionary<string, List<ChartDataPo>> keyValuePairs = new Dictionary<string, List<ChartDataPo>>();
        public ChartDemo()
        {
            InitializeComponent();

            chartControl.AxisYFormat = "0.00";

            for (int j = 0; j < seriesCount; j++)
            {
                List<ChartDataPo> list = new List<ChartDataPo>();
                for (int i = 0; i < numberOfPoints; i++)
                {
                    int sign = 1;
                    double delta = r.NextDouble();
                    sign = delta < upDownRatio[j] ? 1 : -1;
                    trend[j] += sign * r.Next(0, 10) * 0.001;
                    DateTime dt = dtBase.AddDays(i);
                    list.Add(new ChartDataPo(dt, trend[j]));
                }
                chartControl.AddLineSeries("Series " + (j + 1), list);
                keyValuePairs.Add("Series " + (j + 1), list);
            }

            List<ChartDataPo> bidList = new List<ChartDataPo>();
            List<ChartDataPo> ofrList = new List<ChartDataPo>();
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = dtBase.AddDays(i * 4);
                bidList.Add(new ChartDataPo(dt, 3 + r.NextDouble() - 0.5));
                ofrList.Add(new ChartDataPo(dt, 4 + r.NextDouble() - 0.5));
            }
            chartControl.AddScatterSeries("Bid", OxyColors.Blue, MarkerType.Triangle, bidList);
            chartControl.AddScatterSeries("Ofr", OxyColors.Orange, MarkerType.Triangle, ofrList);

            Loaded += ChartDemo_Loaded;
        }

        private void ChartDemo_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    int num = 2000;
                    Dispatcher.Invoke(() =>
                    {
                        num = !int.TryParse(count.Text, out int temp) ? 2000 : int.Parse(count.Text);
                    });
                    Thread.Sleep(num);
                    SimulateData();
                }
            });
            //ThreadPool.QueueUserWorkItem(new WaitCallback(RtDataThread));
        }

        /// <summary>
        /// 模拟数据
        /// </summary>
        private void SimulateData()
        {

            //Dispatcher.Invoke(() =>
            //{
            //    chartControl.ClearSeries();
            //});
            dtBase = new DateTime(2020, 1, 1);
            trend = new double[] { 4.5, 4, 3.5, 3 };
            upDownRatio = new double[] { 0.45, 0.44, 0.5, 0.54 };
            Random random = new Random();
            for (int j = 0; j < seriesCount; j++)
            {
                List<ChartDataPo> list = new List<ChartDataPo>();
                for (int i = 0; i < numberOfPoints; i++)
                {
                    int sign = 1;
                    double delta = r.NextDouble();
                    sign = delta < upDownRatio[j] ? 1 : -1;
                    trend[j] += sign * r.Next(0, 10) * 0.001;
                    DateTime dt = dtBase.AddDays(i);
                    list.Add(new ChartDataPo(dt, trend[j]));
                }
                chartControl.Update("Series " + (j + 1), list);
            }

            //for (int j = 0; j < keyValuePairs.Count; j++)
            //{
            //    List<ChartDataPo> list = new List<ChartDataPo>();
            //    var data = keyValuePairs.ElementAt(j);
            //    for (int i = 0; i < data.Value.Count; i++)
            //    {
            //        var value = data.Value[i].Value + random.Next(-1, 1) * 0.01;
            //        list.Add(new ChartDataPo(data.Value[i].DateTime, value));
            //    }
            //    chartControl.Update("Series " + (j + 1), list);

            //}
            //List<ChartNamePo> pos = new List<ChartNamePo>();
            //for (int j = 0; j < keyValuePairs.Count; j++)
            //{
            //    List<ChartDataPo> list = new List<ChartDataPo>();
            //    var data = keyValuePairs.ElementAt(j);
            //    for (int i = 0; i < data.Value.Count; i++)
            //    {
            //        var value = data.Value[i].Value+ random.Next(-1,1)*0.01;
            //        list.Add(new ChartDataPo(data.Value[i].DateTime, value));
            //    }
            //    chartControl.Update("Series " + (j + 1), list);
            //    //pos.Add(new ChartNamePo()
            //    //{
            //    //    datas = list,
            //    //    name = "Series " + (j + 1)
            //    //});
            //}
            //chartControl.changeSource(pos);
            List<ChartDataPo> bidList = new List<ChartDataPo>();
            List<ChartDataPo> ofrList = new List<ChartDataPo>();
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = dtBase.AddDays(i * 4);
                bidList.Add(new ChartDataPo(dt, 3 + r.NextDouble() - 0.5));
                ofrList.Add(new ChartDataPo(dt, 4 + r.NextDouble() - 0.5));
            }
            chartControl.Update("Bid", bidList);
            chartControl.Update("Ofr", ofrList);
        }


        private void RtDataThread(object obj)
        {
            DateTime dt = dtBase.AddDays(numberOfPoints);
            while (true)
            {
                Thread.Sleep(1000);

                dt = dt.AddDays(1);
                Console.WriteLine("RtDataThread add one point: {0}", dt.ToString("yyyyMMdd"));
                for (int j = 0; j < seriesCount; j++)
                {
                    int sign = 1;
                    double delta = r.NextDouble();
                    sign = delta < upDownRatio[j] ? 1 : -1;
                    trend[j] += sign * r.Next(0, 10) * 0.001;
                    chartControl.AddPoint("Series " + (j + 1), new ChartDataPo(dt, trend[j]));
                }
                this.Dispatcher.Invoke((Action)(() =>
                {
                    chartControl.InvalidChart();
                }));
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null && chartControl != null)
            {
                string name = cb.Content as string;
                chartControl.SetSeriesVisible(name, cb.IsChecked.Value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
