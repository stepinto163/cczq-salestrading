using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
//using OxyPlot.Legends;
using OxyPlot.Series;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STC.Chart.Plugin.ViewModel
{
    public class CandleLineViewModel: ViewModelBase
    {
        #region 构造
        public PlotModel model { get; set; }
        public PlotModel model2 { get; set; }

        public CandleStickSeries candle { get; set; }
        public CandleStickSeries rb { get; set; }
        DateTimeAxis timeSPanAxis1;
        DateTimeAxis timeSPanAxis2;

        public Dictionary<double, StockInfo> DictPoints = new Dictionary<double, StockInfo>();

        public List<StockInfo> Data { get; set; }
        public ICommand ResetCommand { get { return new ComRelayCommand(OnResetCommand); } }
        public ICommand ChangeSampleCommand { get { return new ComRelayCommand(OnChangeSampleCommand); } }
        

        private string currentTrackerOther = string.Empty;
        public string CurrentTrackerOther
        {
            get
            {
                return currentTrackerOther;
            }

            set
            {
                currentTrackerOther = value;
                OnPropertyChanged(nameof(CurrentTrackerOther));
            }
        }

        private object _CurrentTag = string.Empty;
        public object CurrentTag
        {
            get
            {
                return _CurrentTag;
            }

            set
            {
                _CurrentTag = value;
                OnPropertyChanged(nameof(CurrentTag));
            }
        }
        #endregion

        private void OnResetCommand()
        {
            model.ResetAllAxes();
            model.InvalidatePlot(true);
            model2.ResetAllAxes();
            model2.InvalidatePlot(true);
        }

        private void OnChangeSampleCommand()
        {
            model.ResetAllAxes();
            model.InvalidatePlot(true);
            model2.ResetAllAxes();
            model2.InvalidatePlot(true);
        }


        [Obsolete]
        public CandleLineViewModel()
        {
            Init1();
            Init2();
        }

        /// <summary>
        /// 设置坐标轴
        /// </summary>
        [Obsolete]
        private void Init1()
        {
            InitData();

            #region Prg

            #region Model1 顶图
            model = new PlotModel
            {
                PlotType = PlotType.XY,
                Title = "",

                IsLegendVisible = true,
                PlotAreaBackground = OxyColor.Parse("#121A19"),
                PlotAreaBorderColor = OxyColor.Parse("#193D37"),
                PlotAreaBorderThickness = new OxyThickness(1, 1, 1, 1),

                TitleColor = OxyColors.Yellow,
                TitlePadding = 0,

                // PlotMargins = new OxyThickness(26, 10, 0, 20), //调节上边距
                PlotMargins = new OxyThickness(26, 10, 0, 10), //调节上边距
                Background = OxyColor.Parse("#0A0F0E"),
                //LegendBackground = OxyColor.Parse("#121A19"),
                // LegendTitleColor = OxyColors.White,
                //LegendTitleFontSize = 9,

                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopRight,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 1,
                LegendItemAlignment = OxyPlot.HorizontalAlignment.Right,
                LegendTextColor = OxyColors.White,
                LegendMargin = -24,
                LegendItemSpacing = 4,
                LegendFontSize = 10,
            };

            #endregion

            #region Model2 底图
            model2 = new PlotModel
            {
                PlotType = PlotType.XY,
                Title = "",
                IsLegendVisible = false,
                PlotAreaBackground = OxyColor.Parse("#121A19"),
                PlotAreaBorderColor = OxyColor.Parse("#193D37"),
                //PlotAreaBorderThickness = new OxyThickness(1, 1, 1, 1),
                TitleColor = OxyColors.Yellow,
                TitlePadding = 0,
                PlotMargins = new OxyThickness(26, -30, 0, 20), //调节上边距
                Background = OxyColor.Parse("#0A0F0E"),
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0,
                LegendItemAlignment = OxyPlot.HorizontalAlignment.Right,
            };

            #endregion

            string datestr = DateTime.Now.ToString("yyyy-MM-dd");

            #region 坐标轴设置

            #region 顶图 xy 轴设置

            //Model x 轴 日期时间  TimspanKey
            timeSPanAxis1 = new DateTimeAxis()
            {
                Key = "TimspanKey",
                AxislineStyle = LineStyle.Dot,
                AxislineColor = OxyColor.Parse("#9C9C9C"),
                AxislineThickness = 0,
                //缩放
                IsZoomEnabled = true,
                //平移（按住鼠标右键平移）
                IsPanEnabled = true,

                Position = AxisPosition.Bottom,
                //刻度
                TickStyle = TickStyle.None,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.Parse("#193D37"),
                MinorGridlineColor = OxyColor.Parse("#193D37"),
                MajorGridlineThickness = 0.5,
                MinorGridlineThickness = 0.5,
                StringFormat = "HH:mm",
                TitleFontSize = 9,
                AxisDistance = 0,
                AxisTickToLabelDistance = 5,
                AxisTitleDistance = 50,
                IsAxisVisible = true,
                // 坐标轴名称
                Title = "",
                TitleColor = OxyColors.Blue,
                FontSize = 9,
                Angle = 0,
                // TextColor = model2.Background,
                //TextColor = OxyColors.LightBlue,
                // TextColor = model2.Background,
                TextColor = OxyColor.Parse("#788187"),
                //MajorStep = 0.05,
                //MinorStep = 0.05,
                //MajorTickSize = 6,
                //MinorTickSize = 6,
                //IntervalLength = 60,
                //MinorIntervalType = DateTimeIntervalType.Days,
                MajorStep = 1 / 12.0, // 上面的设置会失效
                MinorStep = 1 / 24.0, // 上面的设置会失效
            };
            model.Axes.Add(timeSPanAxis1);


            LinearAxis linearAxis1 = new LinearAxis()
            {
                Key = "klineY",
                TextColor = OxyColor.Parse("#9C9C9C"),
                Position = AxisPosition.Left,
                IsZoomEnabled = true,//坐标轴缩放关闭
                IsPanEnabled = true,//图表缩放功能关闭
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                MajorGridlineColor = OxyColor.Parse("#193D37"),
                TicklineColor = OxyColor.Parse("#193D37"),
                AxisDistance = 2,
                AxislineStyle = OxyPlot.LineStyle.Solid,
                AxislineThickness = 5,
                AxisTickToLabelDistance = 3,
                AxisTitleDistance = 0,
                AxislineColor = OxyColors.Transparent,
                TickStyle = TickStyle.None,
                //Minimum = 0,
                //Maximum = 30,
                FontSize = 9,
                MajorStep = 5.0050,
                MinorStep = 0.0005,
                //StringFormat = "f4",
                AbsoluteMinimum = 0,
                //AbsoluteMaximum = 10,
                UseSuperExponentialFormat = false,
            };
            model.Axes.Add(linearAxis1);
            #endregion

            #region 底图 xy 轴设置
            //Model2 x 轴 日期时间 TimspanKey2
            timeSPanAxis2 = new DateTimeAxis()
            {
                Key = "TimspanKey2",
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.Parse("#9C9C9C"),
                AxislineThickness = 1,
                //缩放
                IsZoomEnabled = true,
                //平移（按住鼠标右键平移）
                IsPanEnabled = true,
                //刻度
                TickStyle = TickStyle.None,

                Position = AxisPosition.Bottom,

                //MinorIntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.Parse("#193D37"),
                MinorGridlineColor = OxyColor.Parse("#193D37"),
                MajorGridlineThickness = 0.5,
                MinorGridlineThickness = 0.5,
                StringFormat = "HH:mm",
                TitleFontSize = 8,
                AxisDistance = 0,
                AxisTickToLabelDistance = 5,
                AxisTitleDistance = 5,
                IsAxisVisible = true,
                // 坐标轴名称
                Title = "",
                TitleColor = OxyColors.Blue,
                FontSize = 9,
                Angle = 1,
                TextColor = OxyColor.Parse("#788187"),
                //MajorStep = 0.05,
                //MinorStep = 0.05,
                //MajorTickSize = 6,
                //MinorTickSize = 6,
                //IntervalLength = 60,
                //MinorIntervalType = DateTimeIntervalType.Days,
                MajorStep = 1 / 12.0, // 上面的设置会失效
                MinorStep = 1 / 24.0, // 上面的设置会失效
            };
            model2.Axes.Add(timeSPanAxis2);


            //定义y2轴
            double maxBootomAxisY = 10;
            LinearAxis VolumnAxesY = new LinearAxis()
            {
                Key = "VolumnY",
                Position = AxisPosition.Left,
                Title = "",//显示标题内容
                TitleColor = OxyColor.Parse("#ff0000"),//显示标题位置
                IsZoomEnabled = true,//坐标轴缩放关闭
                IsPanEnabled = true,//图表缩放功能关闭
                MajorGridlineStyle = LineStyle.None,//主刻度设置格网
                MajorGridlineThickness = 0.2,
                MajorGridlineColor = OxyColor.Parse("#7379a0"),
                MinorGridlineStyle = LineStyle.None,//子刻度设置格网样式
                // MinorGridlineColor = OxyColor.Parse("#00ff00"),
                MinorGridlineThickness = 0.1,
                AxisDistance = 0,
                AxislineStyle = LineStyle.Solid,
                AxislineThickness = 0.5,
                AxisTickToLabelDistance = 5,
                AxisTitleDistance = 5,
                FontSize = 9,
                AxislineColor = OxyColors.Transparent,
                TextColor = OxyColor.Parse("#788187"),
                TickStyle = TickStyle.Inside,
                Angle = 0,
                //Minimum = 0,
                //Maximum = 10,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = maxBootomAxisY,
                UseSuperExponentialFormat = false,
            };

            model2.Axes.Add(VolumnAxesY);
            #endregion

            #endregion


            #endregion

            // Bid 线
            #region 成交量 线
            LineSeries volumnSerial = new LineSeries() { Title = "Bid", LineStyle = LineStyle.Solid, YAxisKey = "klineY", XAxisKey = "TimspanKey" };
            volumnSerial.BrokenLineThickness = 0.1;
            volumnSerial.LineStyle = LineStyle.Solid;
            volumnSerial.StrokeThickness = 1;
            volumnSerial.Color = OxyColor.Parse("#FB943C");
            volumnSerial.MarkerType = MarkerType.Circle;
            volumnSerial.MarkerSize = 2;
            volumnSerial.MarkerFill = OxyColor.Parse("#FB943C");
            volumnSerial.Title = "Tkn";
            model.Series.Add(volumnSerial);

            // ask 线
            LineSeries Askerial = new LineSeries() { Title = "Ask", LineStyle = LineStyle.Solid, YAxisKey = "klineY", XAxisKey = "TimspanKey" };
            Askerial.BrokenLineThickness = 1;
            Askerial.LineStyle = LineStyle.Solid;
            Askerial.StrokeThickness = 0.5;
            Askerial.Color = OxyColor.Parse("#58B3FF");
            Askerial.MarkerFill = OxyColor.Parse("#58B3FF");
            Askerial.Title = "Gvn";
            // Askerial.TrackerFormatString = "Date: {2}\nOpen: {5:0.00000}\nHigh: {3:0.00000}\nLow: {4:0.00000}\nClose: {6:0.00000}";
            model.Series.Add(Askerial);

            //成交量 aver 线
            LineSeries AskBidAvererial = new LineSeries() { Title = "Aver", YAxisKey = "klineY", XAxisKey = "TimspanKey" };
            AskBidAvererial.LineStyle = LineStyle.Solid;
            AskBidAvererial.StrokeThickness = 1;
            AskBidAvererial.BrokenLineStyle = LineStyle.None;
            AskBidAvererial.BrokenLineThickness = 0;
            AskBidAvererial.Color = OxyColor.Parse("#FFFFFF");
            AskBidAvererial.MarkerFill = OxyColor.Parse("#FFFFFF");
            //AskBidAvererial.MarkerType = MarkerType.Circle;
            AskBidAvererial.MarkerType = MarkerType.None;
            Askerial.Title = "Trd";
            model.Series.Add(AskBidAvererial);


            // K 线
            #region 老版
            candle = new CandleStickSeries()
            {
                Color = OxyColor.FromRgb(0xFF, 0xEB, 0xC8),
                TextColor = OxyColors.White,
                IncreasingColor = OxyColor.FromRgb(255, 95, 95),
                DecreasingColor = OxyColor.FromRgb(0, 197, 49),
                //IncreasingFill = OxyColor.FromRgb(255, 95, 95),
                //DecreasingFill = OxyColor.FromRgb(0, 197, 49),
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid,
                //CandleWidth = 5,
                CandleWidth = 0.003,
                DataFieldX = "Time",
                DataFieldHigh = "H",
                DataFieldLow = "L",
                DataFieldClose = "C",
                DataFieldOpen = "O",
                // ItemsSource = Data,
                YAxisKey = "klineY",
                XAxisKey = "TimspanKey",
                // TrackerFormatString = "Date: {2}\nOpen: {5:0.00000}\nHigh: {3:0.00000}\nLow: {4:0.00000}\nClose: {6:0.00000}",
            };

            model.Series.Add(candle);
            #endregion


            //彩色坐标
            var axis1 = new LinearColorAxis();
            axis1.Key = "ColorAxis";
            axis1.Minimum = 0.1;
            axis1.Maximum = 0.9;
            axis1.HighColor = OxyColors.Red;
            axis1.LowColor = OxyColors.GreenYellow;
            axis1.Position = AxisPosition.Top;
            axis1.StringFormat = "";
            axis1.IsAxisVisible = false;

            model.Axes.Add(axis1);

            // Bid 散点图
            ScatterSeries BidScatterSeries = new ScatterSeries();
            BidScatterSeries.Title = "Bid";
            BidScatterSeries.MarkerSize = 2;
            BidScatterSeries.MarkerType = MarkerType.Triangle;
            BidScatterSeries.MarkerStroke = OxyColors.Orange;
            BidScatterSeries.MarkerFill = OxyColors.Orange;
            BidScatterSeries.TrackerFormatString = "Bid";
            // scatterSeries.LabelFormatString = "{5}";
            model.Series.Add(BidScatterSeries);


            // Ofr 散点图
            ScatterSeries OfrScatterSeries = new ScatterSeries();
            OfrScatterSeries.Title = "Ofr";
            OfrScatterSeries.MarkerSize = 3;
            OfrScatterSeries.MarkerType = MarkerType.Triangle;
            OfrScatterSeries.MarkerStroke = OxyColors.Blue;
            OfrScatterSeries.MarkerFill = OxyColors.Blue;
            OfrScatterSeries.TrackerFormatString = "Ofr";
            // scatterSeries.LabelFormatString = "{5}";
            model.Series.Add(OfrScatterSeries);
            #endregion

            #region 底图线图设置
            // 底图设置
            // 柱形图成交量
            RectangleBarSeries rb = new RectangleBarSeries();
            rb.YAxisKey = "VolumnY";
            rb.XAxisKey = "TimspanKey2";
            model2.Series.Add(rb);

            // 底图面积图
            AreaSeries areaSeries = new AreaSeries()
            {
                StrokeThickness = 0,
                LineStyle = OxyPlot.LineStyle.Solid,
                MarkerType = MarkerType.None,
                MarkerStroke = OxyColors.Red,
                Color = OxyColors.Green,
                Color2 = OxyColor.Parse("#193D37"),
                Fill = OxyColor.Parse("#30193D37"),
                DataFieldX2 = "X",
                YAxisKey = "VolumnY",
                XAxisKey = "TimspanKey2",
                ConstantY2 = 0
            };
            model2.Series.Add(areaSeries);

            // 底图成交量线
            LineSeries vol = new LineSeries() { Title = "Bid", LineStyle = LineStyle.Solid };
            vol.BrokenLineThickness = 0.1;
            vol.LineStyle = LineStyle.Dot;
            vol.StrokeThickness = 1;
            vol.Color = OxyColors.White;
            model2.Series.Add(vol);

            // 底图 K 线 == 成交量图
            CandleStickSeries volcandle = new CandleStickSeries()
            {
                Color = OxyColors.Transparent,
                TextColor = OxyColors.White,
                IncreasingColor = OxyColor.FromRgb(255, 95, 95),
                DecreasingColor = OxyColor.FromRgb(0, 197, 49),
                //IncreasingFill = OxyColor.FromRgb(255, 95, 95),
                //DecreasingFill = OxyColor.FromRgb(0, 197, 49),
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid,
                // CandleWidth = 5,
                CandleWidth = 0.003,
                DataFieldX = "Time",
                DataFieldHigh = "H",
                DataFieldLow = "L",
                DataFieldClose = "C",
                DataFieldOpen = "O",
                // ItemsSource = Data,
                YAxisKey = "VolumnY",
                XAxisKey = "TimspanKey2",
            };

            model2.Series.Add(volcandle);



            #endregion

            #region 填充背景

            #region 设置坐标轴和事件
            string curdate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime sdt = Convert.ToDateTime($"{curdate} 07:00:00");
            DateTime edt = Convert.ToDateTime($"{curdate} 20:00:00");
            timeSPanAxis1.AbsoluteMinimum = DateTimeAxis.ToDouble(sdt);
            timeSPanAxis1.AbsoluteMaximum = DateTimeAxis.ToDouble(edt);
            //timeSPanAxis1.Minimum = DateTimeAxis.ToDouble(Convert.ToDateTime($"{curdate} 07:00:00"));
            //timeSPanAxis1.Maximum = DateTimeAxis.ToDouble(Convert.ToDateTime($"{curdate} 20:00:00"));

            timeSPanAxis2.AbsoluteMinimum = timeSPanAxis1.AbsoluteMinimum;
            timeSPanAxis2.AbsoluteMaximum = timeSPanAxis1.AbsoluteMaximum;
            //timeSPanAxis2.Minimum = timeSPanAxis1.Minimum;
            //timeSPanAxis2.Maximum = timeSPanAxis1.Maximum;

            timeSPanAxis1.AxisChanged += TimeSPanAxis1_AxisChanged;
            timeSPanAxis2.AxisChanged += TimeSPanAxis2_AxisChanged;
            #endregion

            #region 成交价画背景
            Task.Factory.StartNew(() =>
            {
                List<int> lstDraw = new List<int>();
                for (int i = 0; i < Data.Count; i++)
                {
                    double vx = DateTimeAxis.ToDouble(Data[i].Time.AddMinutes(-15));
                    double evx = DateTimeAxis.ToDouble(Data[i].Time.AddMinutes(45));
                    int jk = int.Parse(Math.Floor(i / 4.0).ToString());
                    if (jk % 2 == 1)
                    {
                        if (!lstDraw.Contains(jk))
                        {
                            RectangleAnnotation a1 = new RectangleAnnotation() { MinimumX = vx, MaximumX = evx, MinimumY = 0, MaximumY = 60 };
                            a1.Fill = OxyColor.Parse("#30193D37");

                            model.Annotations.Add(a1);
                            lstDraw.Add(jk);
                        }
                    }
                }
            });
            #endregion

            // 方法2 下图动态添加面积图
            Task.Factory.StartNew(() =>
            {
                //areaSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Data[0].Time.AddMinutes(-15)), maxBootomAxisY));
                //areaSeries.Points2.Add(new DataPoint(DateTimeAxis.ToDouble(Data[0].Time.AddMinutes(45)), 0));
                List<int> lstDraw2 = new List<int>();
                for (int i = 0; i < Data.Count; i++)
                {
                    int jk = int.Parse(Math.Floor(i / 4.0).ToString());
                    if (jk % 2 == 1)
                    {
                        if (!lstDraw2.Contains(jk))
                        {
                            double vx = DateTimeAxis.ToDouble(Data[i].Time.AddMinutes(-15));
                            double evx = DateTimeAxis.ToDouble(Data[i].Time.AddMinutes(45));

                            areaSeries.Points.Add(new DataPoint(vx, 0));
                            areaSeries.Points2.Add(new DataPoint(vx, 0));

                            areaSeries.Points.Add(new DataPoint(vx, maxBootomAxisY));
                            areaSeries.Points2.Add(new DataPoint(vx, 0));
                            areaSeries.Points.Add(new DataPoint(evx, maxBootomAxisY));
                            areaSeries.Points2.Add(new DataPoint(evx, 0));

                            areaSeries.Points.Add(new DataPoint(evx, 0));
                            areaSeries.Points2.Add(new DataPoint(evx, 0));

                            lstDraw2.Add(jk);
                        }
                    }
                }

                model2.InvalidatePlot(true);
            });


            #endregion


            var rd = new Random();

            for (int i = 0; i < Data.Count; i++)
            {
                StockInfo si = Data[i];
                bool isEven = si.O < si.C ? true : false;
                double vx = DateTimeAxis.ToDouble(si.Time);

                //HighLowItem hli = new HighLowItem() { X = vx, Open = si.O, Close = si.C, High = si.H, Low = si.L };
                //candle.Items.Add(hli);

                DictPoints.Add(vx, si);


                if (!isEven)
                {
                    HighLowItem lowvol = new HighLowItem() { X = vx, Open = si.V, Close = 0, High = si.V, Low = 0 };
                    volcandle.Items.Add(lowvol);
                }
                else
                {
                    HighLowItem lowvol = new HighLowItem() { X = vx, Open = 0, Close = si.V, High = si.V, Low = 0 };
                    volcandle.Items.Add(lowvol);
                }

                volumnSerial.Points.Add(new DataPoint(vx, si.H * 1.0));
                Askerial.Points.Add(new DataPoint(vx, si.L * 1.3));
                AskBidAvererial.Points.Add(new DataPoint(vx, si.L * 1.3 + (si.H * 1.3 - si.L * 1.3) / 2));

                //BidScatterSeries.Points.Add(new ScatterPoint(vx, si.H * 1.3, 3, 200));
                //OfrScatterSeries.Points.Add(new ScatterPoint(vx, si.L * 1.0, 4, 200));
            }



            model.InvalidatePlot(false);
            // model2.ResetAllAxes();
            model2.InvalidatePlot(false);
        }

        /// <summary>
        /// 设置初始数据
        /// </summary>
        private void Init2()
        {

        }

        /// <summary>
        /// 改变显示范围
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void RangeSliderChange(double min, double max)
        {
            try
            { 
                int minPos = 0;
                int maxPos = 0;

                minPos = (int)Math.Round(Data.Count * min);
                maxPos = (int)Math.Round(Data.Count * max) - 1 ;
                if (min == 0 && max == 1)
                {
                    model.ResetAllAxes();
                    model.InvalidatePlot(true);
                    model2.ResetAllAxes();
                    model2.InvalidatePlot(true);
                }
                else
                { 
                    StockInfo startStock = Data[minPos];
                    StockInfo endStock = Data[maxPos];
                    double bx = DateTimeAxis.ToDouble(startStock.Time);
                    double ex = DateTimeAxis.ToDouble(endStock.Time);

                    timeSPanAxis2.Zoom(bx, ex);
                    model2.InvalidatePlot(false);
                }
            }
            catch 
            {
            }
        }

        /// <summary>
        /// 时间间隔，单位分钟
        /// </summary>
        private int timeDiff = 15;
        private void InitData()
        {
            var rd = new Random();
            string curdate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime sd = Convert.ToDateTime($"{curdate} 07:00:00");
            DateTime ed = Convert.ToDateTime($"{curdate} 19:45:00");
            Data = new List<StockInfo>();

            while (sd < ed)
            {
                double vv = rd.Next(1000 * 1000, 1000 * 1000 * 10) / 1000000.0;
                double vv2 = rd.Next(1000 * 1000, 1000 * 1000 * 10) / 1000000.0;
                double oo = rd.Next(1000 * 1000, 1000 * 1000 * 10) / 1000000.0;
                double cc = rd.Next(1000 * 1000, 1000 * 1000 * 10) / 1000000.0;
                double v = rd.Next(2, 9);
                StockInfo si = new StockInfo();
                sd = sd.AddMinutes(timeDiff);
                Data.Add(new StockInfo() { Time = sd, V = v, H = vv, L = vv2, O = oo, C = cc });
            }

            CurrentTag = Data;
        }

        private int axid = 0;


        private void TimeSPanAxis1_AxisChanged(object sender, AxisChangedEventArgs e)
        {
            if ((e.ChangeType == AxisChangeTypes.Zoom) || (e.ChangeType == AxisChangeTypes.Pan))
            {
                if (axid == 0)
                {
                    axid = 2;
                    timeSPanAxis2.Zoom(timeSPanAxis1.ActualMinimum, timeSPanAxis1.ActualMaximum);
                    model2.InvalidatePlot(false);
                }

                if (axid == 1)
                    axid = 0;
            }
            else if (e.ChangeType == AxisChangeTypes.Reset)
            {
                model2.ResetAllAxes();
                model2.InvalidatePlot(true);
            }
            else
            {
                timeSPanAxis2.Zoom(timeSPanAxis1.ActualMinimum, timeSPanAxis1.ActualMaximum);
            }
        }

        private void TimeSPanAxis2_AxisChanged(object sender, AxisChangedEventArgs e)
        {
            if ((e.ChangeType == AxisChangeTypes.Zoom) || (e.ChangeType == AxisChangeTypes.Pan))
            {
                if (axid != 2)
                {
                    axid = 1;
                    timeSPanAxis1.Zoom(timeSPanAxis2.ActualMinimum, timeSPanAxis2.ActualMaximum);

                    model.InvalidatePlot(false);
                }

                if (axid == 2)
                    axid = 0;
            }
            else
            {
                timeSPanAxis1.Zoom(timeSPanAxis2.ActualMinimum, timeSPanAxis2.ActualMaximum);
            }
        }
    }


    #region StockInfo ,FullData,  CustomDataPoint Class
    public class StockInfo
    {
        public DateTime Time { get; set; }
        public TimeSpan QTime { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double O { get; set; }
        public double C { get; set; }

        public double V { get; set; }
    }


    public class FullData
    {
        public List<CustomDataPoint> Points1 { get; set; }
        public List<CustomDataPoint> Points2 { get; set; }
    }

    public class CustomDataPoint : IDataPointProvider
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Description { get; set; }
        public DataPoint GetDataPoint() => new DataPoint(X, Y);

        public CustomDataPoint(double x, double y, string description)
        {
            X = x;
            Y = y;
            Description = description;
        }
    }

    #endregion
}
