using com.gateway.log;
using Newtonsoft.Json;
using STC.Chart.Plugin.Converts;
using STC.Chart.Plugin.Data;
using STC.Chart.Plugin.FuncEx;
using STC.Common.Model.Chart;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace STC.Chart.Plugin.Chart
{
    /// <summary>
    /// LinePage.xaml 的交互逻辑
    /// </summary>
    public partial class LinePage : UserControl
    {
        /// <summary>
        /// 列宽
        /// </summary>
        private double colWidth;
        /// <summary>
        /// 柱状图集合
        /// </summary>
        List<Grid> bars = new List<Grid>();
        /// <summary>
        /// 点集合
        /// </summary>
        List<Ellipse> ellipses = new List<Ellipse>();
        /// <summary>
        /// 折线数据集合
        /// </summary>
        private List<LineModel> lineModels = new List<LineModel>();
        /// <summary>
        /// 图标显示时间集合
        /// </summary>
        List<DateTime> times = new List<DateTime>();

        /// <summary>
        /// 本地图表数据源
        /// </summary>
        private ObservableCollection<ChartModel> chartModelsLocal = new ObservableCollection<ChartModel>();

        /// <summary>
        /// 索引和折线椭圆的数据集合
        /// </summary>
        private Dictionary<int, Tuple<Ellipse, Line>> lineDatas = new Dictionary<int, Tuple<Ellipse, Line>>();

        List<BarModel> barData;

        //上下间距预留系数
        private decimal scale = 0.005M;



        public IEnumerable<DealtPo> LineSource
        {
            get { return (IEnumerable<DealtPo>)GetValue(LineSourceProperty); }
            set
            {
                SetValue(LineSourceProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineSourceProperty =
            DependencyProperty.Register("LineSource", typeof(IEnumerable<DealtPo>), typeof(LinePage), new FrameworkPropertyMetadata((o, e) =>
             {
                 var c = o as LinePage;
                 if (e.OldValue != null)
                 {
                     var notifyCollectionChanged = e.OldValue as INotifyCollectionChanged;
                     if (notifyCollectionChanged != null)
                     {
                         notifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(c.LineSource_CollectionChanged);
                     }
                 }

                 if (e.NewValue != null)
                 {
                     var notifyCollectionChanged = e.NewValue as INotifyCollectionChanged;
                     if (notifyCollectionChanged != null)
                     {
                         notifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(c.LineSource_CollectionChanged);
                     }
                 }

                 if (e.NewValue != null)
                 {
                     var lintPage = o as LinePage;
                     var source = e.NewValue as IEnumerable<DealtPo>;
                     ObservableCollection<ChartModel> chartModels = new ObservableCollection<ChartModel>();
                     source?.ToList()?.ForEach(_ => chartModels.Add(_.Change2ChartModel()));
                     lintPage.ChangeDataSource(chartModels, lintPage.BondCode);
                 }
             }));



        public string BondCode
        {
            get { return (string)GetValue(BondCodeProperty); }
            set { SetValue(BondCodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BondCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BondCodeProperty =
            DependencyProperty.Register("BondCode", typeof(string), typeof(LinePage), new PropertyMetadata(string.Empty, (o, e) =>
             {
                 var lintPage = o as LinePage;
                 lintPage.BondCodeText.Text = e.NewValue?.ToString();
             }));


        ///// <summary>
        ///// 柱状图最大值
        ///// </summary>
        //private int maxBarData
        //{
        //    get
        //    {
        //        if (chartModelsLocal == null || chartModelsLocal.Count == 0) return 0;
        //        return chartModelsLocal.Max(_ => _.Volumn);
        //    }
        //}
        /// <summary>
        /// 曲线 最小值
        /// </summary>
        private decimal minLineData
        {
            get
            {
                if (chartModelsLocal == null || chartModelsLocal.Count == 0) return 0;

                return chartModelsLocal.Min(_ => _.Price) * (1 - scale);
            }
        }
        /// <summary>
        /// 曲线 最大值
        /// </summary>
        private decimal maxLineData
        {
            get
            {
                if (chartModelsLocal == null || chartModelsLocal.Count == 0) return 0;
                return chartModelsLocal.Max(_ => _.Price) * (1 + scale);
            }
        }

        /// <summary>
        /// 时间间隔，单位分钟
        /// </summary>
        private int timeDiff = 15;

        static LinePage Instance = null;
        public LinePage()
        {
            InitializeComponent();
            Init();
            ChangeDataSource(new ObservableCollection<ChartModel>(), "");
            Instance = this;
        }

        private void LineSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateItem(this);
        }

        void UpdateItem(LinePage linePage)
        {
            linePage.Clear();
            ObservableCollection<ChartModel> chartModels = new ObservableCollection<ChartModel>();
            LineSource?.ToList()?.ForEach(_ => chartModels.Add(_.Change2ChartModel()));
            linePage.ChangeDataSource(chartModels, linePage.BondCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chartModels">图表数据源</param>
        public LinePage(ObservableCollection<ChartModel> chartModels, string bondCode)
        {
            InitializeComponent();
            Init();
            ChangeDataSource(chartModels, bondCode);
        }

        /// <summary>
        /// 更新图表
        /// </summary>
        /// <param name="chartModel"></param>
        public void UpdateChart(ChartModel chartModel)
        {
            if (chartModelsLocal == null) return;

            chartModelsLocal.Add(chartModel);
            if (chartModelsLocal.Count == 1)
            {
                ChangeDataSource(chartModelsLocal, "");
            }
            else
            {
                UpdateBar(chartModel);
                UpdateLine(chartModel);
            }
        }

        /// <summary>
        /// 改变数据源
        /// </summary>
        public void ChangeDataSource(ObservableCollection<ChartModel> chartModels, string bondCode)
        {
            if (chartModels == null || chartModels.Count == 0)
            {
                Clear();
                return;
            }

            this.BondCodeText.Text = bondCode;

            chartModelsLocal = chartModels;
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,new Action(() =>
             {
                 SetBarY(0);
                 SetLineY();
                 SetBar();
                 SetLineData();
             }));

        }
        ///// <summary>
        ///// 构造函数
        ///// </summary>
        ///// <param name="maxBarData">柱状图 最大值</param>
        ///// <param name="minLineData">折线图 y坐标  最小值</param>
        ///// <param name="maxLineData">最大值</param>
        //public LinePage(double maxBarData, decimal minLineData, decimal maxLineData, string strBondCode)
        //{
        //    InitializeComponent();
        //    Init();

        //    SetBarY(maxBarData);
        //    SetLineY(minLineData, maxLineData);

        //    BondCodeText.Text = strBondCode;
        //}


        /// <summary>
        /// 设置柱状图 最大值
        /// </summary>
        /// <param name="maxBarData"></param>
        public void SetBarY(decimal maxBarData)
        {
            try
            {

                if (maxBarData == 0)
                {
                    BarY00.Text = "";
                    BarY01.Text = "";
                    BarY02.Text = "";
                    BarY03.Text = "";
                }
                else
                {
                    maxBarData = maxBarData + (3 - maxBarData % 3);
                    decimal tempData = maxBarData / 3;
                    BarY00.Text = "0";
                    BarY01.Text = Math.Ceiling(tempData).ToString();
                    BarY02.Text = Math.Ceiling(tempData * 2).ToString();
                    BarY03.Text = maxBarData.ToString();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
        /// <summary>
        /// 设置折线图 y坐标 最大值 最小值
        /// </summary>
        /// <param name="minLineData"></param>
        /// <param name="maxLineData"></param>
        public void SetLineY()
        {
            if (maxLineData == 0)
            {
                LineY01.Text = "";
                LineY02.Text = "";
                LineY03.Text = "";
                LineY04.Text = "";
                LineY05.Text = "";
                LineY06.Text = "";
                LineY07.Text = "";

                return;
            }

            var tempData = (maxLineData - minLineData) / 6;

            PriceConverter priceConverter = new PriceConverter();

            LineY01.Text = priceConverter.Convert(minLineData, null, null, null).ToString();
            LineY02.Text = priceConverter.Convert(minLineData + tempData, null, null, null).ToString();
            LineY03.Text = priceConverter.Convert(minLineData + tempData * 2, null, null, null).ToString();
            LineY04.Text = priceConverter.Convert(minLineData + tempData * 3, null, null, null).ToString();
            LineY05.Text = priceConverter.Convert(minLineData + tempData * 4, null, null, null).ToString();
            LineY06.Text = priceConverter.Convert(minLineData + tempData * 5, null, null, null).ToString();
            LineY07.Text = priceConverter.Convert(maxLineData, null, null, null).ToString();
            if (maxLineData >= 10)
            {
                LineY01.FontSize = LineY02.FontSize = LineY03.FontSize = LineY04.FontSize = LineY05.FontSize = LineY06.FontSize = LineY07.FontSize = 8;
            }

            if (maxLineData >= 100)
            {
                LineY01.FontSize = LineY02.FontSize = LineY03.FontSize = LineY04.FontSize = LineY05.FontSize = LineY06.FontSize = LineY07.FontSize = 7;
            }
        }

        /// <summary>
        /// 更新Bar
        /// </summary>
        private void UpdateBar(ChartModel chartModel)
        {
            if (chartModel == null) return;

            var i = GetIndexByTime(chartModel.Time);
            var bar = GetBarDataByIndex(i);
            UpdateBarByIndex(i, bar);
        }

        /// <summary>
        /// 更新Line
        /// </summary>
        private void UpdateLine(ChartModel chartModel)
        {
            if (chartModel == null) return;

            var i = GetIndexByTime(chartModel.Time);
            var lineModel = GetLineDataByIndex(i);
            if (lineModel.data < LineY01.Text.ToDecimal() || lineModel.data > LineY07.Text.ToDecimal())
            {
                SetLineY();
                SetLineData();
                return;
            }
            if (lineDatas.ContainsKey(i))
            {
                double lastX = -1, lastY = 0;
                
                lastX = lineDatas[i].Item2.X2;
                lastY = lineDatas[i].Item2.Y2;
                
                var height = 6 * (this.ActualHeight - 20 - 16 - 20) / 9;
                var ellipse = lineDatas[i].Item1;
                var tempY = ((lineModel.data - minLineData) * (decimal)height / (maxLineData - minLineData));
                Canvas.SetBottom(ellipse, (double)tempY - ellipse.Width / 2);

                var line = lineDatas[i].Item2;
                line.Y2 = height - (double)tempY;
            }
            else
            {
                double lastX = -1, lastY = -1;
                if (i > 0)
                {
                    for (int j = i; j > 0; j--)
                    {
                        if (lineDatas.ContainsKey(j))
                        {
                            lastX = lineDatas[j].Item2.X2;
                            lastY = lineDatas[j].Item2.Y2;
                            break;
                        }
                    }
                }
                var height = 6 * (this.ActualHeight - 20 - 16 - 20) / 9;
                colWidth = (this.ActualWidth - 20 - 35) / 52;

                Ellipse ellipse = new Ellipse();
                ellipse.Width = 4;
                ellipse.Height = 4;
                ellipse.Tag = lineModel.Index;
                ellipse.Fill = (SolidColorBrush)this.FindResource("ColorBrush01");
                ellipse.MouseEnter += Ellipse_MouseEnter;
                ellipse.MouseLeave += Ellipse_MouseLeave;
                DataCanvas.Children.Add(ellipse);

                var tempY = ((lineModel.data - minLineData) * (decimal)height / (maxLineData - minLineData));
                var tempX = colWidth * lineModel.Index + colWidth / 2;

                Canvas.SetBottom(ellipse, (double)tempY - ellipse.Width / 2);
                Canvas.SetLeft(ellipse, tempX - ellipse.Width / 2);

                Line line = new Line();
                line.X2 = tempX;
                line.Y2 = height - (double)tempY;
                if (lastX != -1)
                {
                    line.Stroke = (SolidColorBrush)this.FindResource("ColorBrush01");
                    line.StrokeThickness = 1;
                    line.X1 = lastX;
                    line.Y1 = lastY;
                    DataCanvas.Children.Add(line);
                }

                lineDatas[i] = new Tuple<Ellipse, Line>(ellipse, line);

            }
        }


        /// <summary>
        /// 通过索引更新bar
        /// </summary>
        private void UpdateBarByIndex(int i, BarModel bar, bool isInit = false)
        {
            var height = this.ActualHeight;
            if (height == 0)
            {
                height = 365;
            }
            double temp;

            if (!isInit)
            {
                if (!string.IsNullOrEmpty(BarY03.Text) && (double.TryParse(BarY03.Text, out temp) && bar.Data > double.Parse(BarY03.Text)))
                {
                    //当前计算的bar数据超过量面板最大值，则重绘bar数据
                    SetBarY((decimal)bar.Data);
                    SetBar();
                }
                else
                {
                    if (string.IsNullOrEmpty(BarY03.Text))
                    {
                        SetBarY((decimal)bar.Data);
                    }
                    bars[i].Height = bar.Data / double.Parse(BarY03.Text.ToDecimal().ToString()) * ((height - 20 - 20 - 20) / 9) * 3;
                    if (bar.Type)
                    {
                        bars[i].Background = (SolidColorBrush)this.FindResource("ColorBrush04");
                    }
                    else
                    {
                        bars[i].Background = (SolidColorBrush)this.FindResource("ColorBrush05");
                    }

                    barData[i].Data = bar.Data;
                    barData[i].Type = bar.Type;
                    bars[i].Tag = bar.Data;
                }
            }
            else
            {
                double minh = bar.Data / double.Parse(BarY03.Text.ToDecimal().ToString()) * ((height - 20 - 20 - 20) / 9) * 3;
                if (minh >= 0)
                {
                    bars[i].Height = minh;
                    if (bar.Type)
                    {
                        bars[i].Background = (SolidColorBrush)this.FindResource("ColorBrush04");
                    }
                    else
                    {
                        bars[i].Background = (SolidColorBrush)this.FindResource("ColorBrush05");
                    }

                    barData[i].Data = bar.Data;
                    barData[i].Type = bar.Type;
                    bars[i].Tag = bar.Data;
                }
            }
        }
        /// <summary>
        /// 设置柱状图数据
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="bar">数据</param>
        public void SetBar()
        {
            if (chartModelsLocal == null || chartModelsLocal.Count == 0) return;
            List<BarModel> barDatas = new List<BarModel>();
            for (int index = 0; index < 52; index++)
            {
                var bar = GetBarDataByIndex(index);
                barDatas.Add(bar);
            }
            if (barDatas.Count > 0)
            {
                SetBarY(barDatas.Max(_ => (decimal)_.Data));
            }
            for (int index = 0; index < barDatas.Count; index++)
            {
                UpdateBarByIndex(index, barDatas[index], true);
            }
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public void Clear()
        {
            this.chartModelsLocal?.Clear();
            SetBarY(0);
            SetLineY();
            DataCanvas.Children?.Clear();
            bars.ForEach(_ => _.Height = 0);
            this.lineModels = null;
            BondCodeText.Text = "";
        }

        public void ClearData()
        {
            this.Clear();
            ChangeDataSource(new ObservableCollection<ChartModel>(), "");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            barData = new List<BarModel>();
            for (int i = 0; i < 52; i++)
            {
                Grid bar = new Grid();
                bar.Style = (Style)this.FindResource("BarStyle");
                Grid.SetColumn(bar, i + 1);
                bar.Height = 0;
                MainGrid.Children.Add(bar);
                bars.Add(bar);
                bar.Tag = 0;
                barData.Add(new BarModel());
            }

            DateTime dt = Convert.ToDateTime("07:00");
            while (dt <= Convert.ToDateTime("20:00"))
            {
                times.Add(dt);
                dt = dt.AddMinutes(timeDiff);
            }
        }
        /// <summary>
        /// 设置折线数据
        /// </summary>
        /// <param name="lineModels"></param>
        public void SetLineData()
        {
            if (chartModelsLocal == null) return;

            DataCanvas.Children.Clear();
            lineDatas.Clear();
            var height = 6 * (this.ActualHeight - 20 - 16 - 20) / 9;
            colWidth = (this.ActualWidth - 20 - 35) / 52;
            double lastX = -1, lastY = 0;

            for (int i = 0; i < 52; i++)
            {
                var lineModel = GetLineDataByIndex(i);
                if (lineModel.data == 0)
                {
                    continue;
                }
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 4;
                ellipse.Height = 4;
                ellipse.Tag = lineModel.Index;
                ellipse.Fill = (SolidColorBrush)this.FindResource("ColorBrush01");
                ellipse.MouseEnter += Ellipse_MouseEnter;
                ellipse.MouseLeave += Ellipse_MouseLeave;
                DataCanvas.Children.Add(ellipse);

                var tempY = ((lineModel.data - minLineData) * (decimal)height / (maxLineData - minLineData));
                var tempX = colWidth * lineModel.Index + colWidth / 2;

                Canvas.SetBottom(ellipse, (double)tempY - ellipse.Width / 2);
                Canvas.SetLeft(ellipse, tempX - ellipse.Width / 2);

                Line line = new Line();
                if (lastX != -1)
                {
                    line.Stroke = (SolidColorBrush)this.FindResource("ColorBrush01");
                    line.StrokeThickness = 1;
                    line.X1 = lastX;
                    line.Y1 = height - lastY;
                    line.X2 = tempX;
                    line.Y2 = height - (double)tempY;
                    DataCanvas.Children.Add(line);
                }
                lineDatas[i] = new Tuple<Ellipse, Line>(ellipse, line);
                lastX = tempX;
                lastY = (double)tempY;

            }
        }

        private void Ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup1.IsOpen = false;
            BorderDotted.Visibility = Visibility.Hidden;
        }

        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            Popup1.HorizontalOffset = 20;
            Popup1.PlacementTarget = ellipse;
            Popup1.IsOpen = true;
            var index = int.Parse(ellipse.Tag.ToString());
            PopTime.Text = "时间： " + GetTimeByIndex(index);
            PopCount.Text = "成交笔数： " + bars[index].Tag.ToString();
            PopPrice.Text = "平均成交： " + GetLineDataByIndex(index).data.ToString("0.0000");
            Grid.SetColumn(BorderDotted, index + 1);
            BorderDotted.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 自适应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinePage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataCanvas.Children?.Clear();
            bars.ForEach(_ => _.Height = 0);
            SetBarY(BarY03.Text.ToDecimal());
            SetLineY();
            SetBar();
            SetLineData();
        }

        string GetTimeByIndex(int index)
        {
            var minute = index * 15;
            DateTime dateTime = new DateTime(2019, 1, 1, 7, 0, 0);
            var result = dateTime.AddMinutes(minute);
            return result.ToString("HH:mm");
        }
        /// <summary>
        /// 根据时间获取索引
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        int GetIndexByTime(string time)
        {
            if (string.IsNullOrEmpty(time)) return 0;
            int index = 0;
            DateTime timeTemp;
            if (DateTime.TryParse(time, out timeTemp))
            {
                for (int i = 0; i < times.Count - 1; i++)
                {
                    if (timeTemp >= times[i] && timeTemp < times[i + 1])
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// 根据时间获取bar数据
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        BarModel GetBarDataByIndex(int index)
        {
            BarModel bar = new BarModel();
            var timeStart = times[index];
            var timeEnd = times[index + 1];
            var listBar = new List<ChartModel>();
            DateTime timeTemp;
            for (int i = 0; i < chartModelsLocal.Count; i++)
            {
                if (DateTime.TryParse(chartModelsLocal[i].Time, out timeTemp))
                {
                    if (timeTemp >= timeStart && timeTemp < timeEnd)
                    {
                        listBar.Add(chartModelsLocal[i]);
                    }
                }
            }
            if (listBar.Count > 0)
            {
                bar.Data = listBar.Sum(_ => _.Volumn);
                listBar.Sort((x, y) =>
                {
                    return x.Time.CompareTo(y.Time);
                });
                var sType = Convert.ToDecimal(listBar[0].Price) - Convert.ToDecimal(listBar[listBar.Count - 1].Price) >= 0 ? true : false;
                bar.Type = sType;
            }
            return bar;
        }

        /// <summary>
        /// 根据时间获取line数据
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        LineModel GetLineDataByIndex(int index)
        {
            LineModel line = new LineModel();
            var timeStart = times[index];
            var timeEnd = times[index + 1];
            var listBar = new List<ChartModel>();
            DateTime timeTemp;
            for (int i = 0; i < chartModelsLocal.Count; i++)
            {
                if (DateTime.TryParse(chartModelsLocal[i].Time, out timeTemp))
                {
                    if (timeTemp >= timeStart && timeTemp < timeEnd)
                    {
                        listBar.Add(chartModelsLocal[i]);
                    }
                }
            }
            if (listBar.Count > 0)
            {
                line.data = listBar.Average(_ => _.Price);
                line.Index = index;
            }
            return line;
        }
    }
}
