using System;
using System.Threading;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using LineSeries = OxyPlot.Series.LineSeries;
using System.Collections.ObjectModel;
using OxyPlot.Series;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using OxyPlot.Annotations;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Drawing;
using System.Reflection;
using System.IO;
using OxyPlot.Legends;
using STC.Common.Model;

namespace BDB.Trade.Plugin.ViewModel.Chart
{
    public class TransactionLadderViewModel : BaseViewModel
    {
        public PlotModel model { get; set; }

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

        public List<CustomDataPoint> Points { get; set; }
        public List<CustomDataPoint> Points2 { get; set; }
        public List<CustomDataPoint> Points3 { get; set; }

        public TransactionLadderViewModel()
        {

            #region Ok Prg

            model = new PlotModel
            {
                PlotType = PlotType.XY,
                Title = "",
                
                
                IsLegendVisible = false,
                PlotAreaBackground = OxyColor.Parse("#121A19"),
                PlotAreaBorderColor = OxyColor.Parse("#193D37"),
                PlotAreaBorderThickness = new OxyThickness(1, 1, 1, 0),
                TitleColor = OxyColors.Yellow,
                TitlePadding = 0,
                PlotMargins = new OxyThickness(10, 0, 0, 20), //调节上边距
            };

            model.Legends.Add(new Legend()
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0,
                LegendItemAlignment = OxyPlot.HorizontalAlignment.Right,
            });

            model.Background = OxyColor.Parse("#0A0F0E");



            string datestr = DateTime.Now.ToString("yyyy-MM-dd");
            var startDate = Convert.ToDateTime($"{datestr} 09:00");
            var endDate = Convert.ToDateTime($"{datestr} 15:00");

            var minValue = DateTimeAxis.ToDouble(startDate);
            var maxValue = DateTimeAxis.ToDouble(endDate);

            // X 轴
            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                // 数值范围
                AbsoluteMinimum = minValue,
                AbsoluteMaximum = maxValue,
                Minimum = minValue,
                Maximum = maxValue,

                // 坐标轴线设置(需要把图表边框设为0才能生效）
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.Parse("#9C9C9C"),
                AxislineThickness = 1,

                //缩放
                IsZoomEnabled = true,
                //平移（按住鼠标右键平移）
                IsPanEnabled = true,

                //刻度
                TickStyle = TickStyle.Crossing,

                //小刻度线
                MinorTickSize = 5,
                MinorIntervalType = DateTimeIntervalType.Minutes,
                MinorGridlineColor = OxyColor.Parse("#FF0000"),
                MinorGridlineThickness = 0.5,

                // 主网格线
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.Parse("#193D37"),
                MajorGridlineThickness = 1,

                StringFormat = "HH:mm",
                IntervalType = DateTimeIntervalType.Manual,
                IntervalLength = 30,
                // 坐标轴名称
                Title = "",
                TitleColor = OxyColors.Blue,
                FontSize = 11,
                Angle = 0,
                TextColor = OxyColor.Parse("#788187"),
            });

            // Y 轴
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                // 数值范围
                Minimum = 0,
                Maximum = 20,
                MajorStep = 5,
                MinorStep = 1,

                // 坐标轴线设置(需要把图表边框设为0才能生效）
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColor.Parse("#9C9C9C"),
                AxislineThickness = 1,

                //缩放
                IsZoomEnabled = true,
                //平移（按住鼠标右键平移）
                IsPanEnabled = true,

                //刻度
                TickStyle = TickStyle.None,

                //小刻度线
                MinorTickSize = 20,
                MinorGridlineColor = OxyColor.Parse("#FF0000"),
                MinorGridlineThickness = 0.5,

                // 主网格线
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.Parse("#193D37"),
                MajorGridlineThickness = 0.5,

                // 坐标轴名称
                Title = "",
                TitleColor = OxyColors.Blue,
                FontSize = 11,
                Angle = 0,
                TextColor = OxyColor.Parse("#788187"),
                // ExtraGridlineColor = OxyColors.Orange,
            });


            #region Data
            this.Points = new List<CustomDataPoint>
            {
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:00")), 3,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:15")), 3,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:30")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:45")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:00")), 5,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:15")), 5,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:30")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:45")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:00")), 1,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:15")), 1,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:30")), 2,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:45")), 2,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:00")), 1,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:15")), 1,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:30")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:45")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:00")), 2,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:15")), 2,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:30")), 1,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:45")), 1,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:00")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:15")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:30")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:45")), 4,"demo0"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 15:00")), 5,"demo0"),
            };


            this.Points2 = new List<CustomDataPoint>
            {
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:00")), 4,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:15")), 4,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:30")), 6,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:45")), 6,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:00")), 8,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:15")), 8,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:30")), 5,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:45")), 5,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:00")), 10,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:15")), 10,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:30")), 8,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:45")), 8,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:00")), 3,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:15")), 3,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:30")), 7,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:45")), 7,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:00")), 3,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:15")), 3,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:30")), 2,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:45")), 2,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:00")), 6,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:15")), 6,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:30")), 8,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:45")), 8,"demo1"),
                new CustomDataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 15:00")), 9,"demo1"),
            };



            // 全部数据
            this.CurrentTag = new FullData() { Points1 = this.Points, Points2 = Points2 };

            #endregion



            #region 加背景,会影响表格线颜色和折线图，舍弃
            //OxyImage image;
            //var assembly = Assembly.GetExecutingAssembly();
            //string path = "e:\\Ns1yn.png";
            //using (FileStream stream = new FileStream(path, FileMode.Open))
            //{
            //    image = new OxyImage(stream);
            //}

            //// Centered in plot area, filling width
            //model.Annotations.Add(new ImageAnnotation
            //{
            //    ImageSource = image,
            //    Opacity = 0.1,
            //    Interpolate = false,
            //    X = new PlotLength(0.5, PlotLengthUnit.RelativeToPlotArea),
            //    Y = new PlotLength(0.5, PlotLengthUnit.RelativeToPlotArea),
            //    Width = new PlotLength(1, PlotLengthUnit.RelativeToPlotArea),
            //    Height = new PlotLength(1, PlotLengthUnit.RelativeToPlotArea),

            //    HorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
            //    VerticalAlignment = OxyPlot.VerticalAlignment.Middle,
            //});

            //// Centered in plot area, filling width
            //model.Annotations.Add(new ImageAnnotation
            //{
            //    ImageSource = image,
            //    Opacity = 0.2,
            //    Interpolate = false,
            //    X = new PlotLength(20, PlotLengthUnit.RelativeToPlotArea),
            //    Y = new PlotLength(20, PlotLengthUnit.RelativeToPlotArea),
            //    Width = new PlotLength(1, PlotLengthUnit.RelativeToPlotArea),
            //    HorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
            //    VerticalAlignment = OxyPlot.VerticalAlignment.Middle
            //});

            //// Relative to plot area, inside top/right corner, 120pt wide
            //model.Annotations.Add(new ImageAnnotation
            //{
            //    ImageSource = image,
            //    X = new PlotLength(1, PlotLengthUnit.RelativeToPlotArea),
            //    Y = new PlotLength(0, PlotLengthUnit.RelativeToPlotArea),
            //    Width = new PlotLength(120, PlotLengthUnit.ScreenUnits),
            //    HorizontalAlignment = OxyPlot.HorizontalAlignment.Right,
            //    VerticalAlignment = OxyPlot.VerticalAlignment.Top
            //});

            //// Relative to plot area, above top/left corner, 20pt high
            //model.Annotations.Add(new ImageAnnotation
            //{
            //    ImageSource = image,
            //    X = new PlotLength(0, PlotLengthUnit.RelativeToPlotArea),
            //    Y = new PlotLength(0, PlotLengthUnit.RelativeToPlotArea),
            //    OffsetY = new PlotLength(-5, PlotLengthUnit.ScreenUnits),
            //    Height = new PlotLength(20, PlotLengthUnit.ScreenUnits),
            //    HorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
            //    VerticalAlignment = OxyPlot.VerticalAlignment.Bottom
            //});

            //// At the point (50,50), 200pt wide
            //model.Annotations.Add(new ImageAnnotation
            //{
            //    ImageSource = image,
            //    X = new PlotLength(50, PlotLengthUnit.Data),
            //    Y = new PlotLength(50, PlotLengthUnit.Data),
            //    Width = new PlotLength(200, PlotLengthUnit.ScreenUnits),
            //    HorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
            //    VerticalAlignment = OxyPlot.VerticalAlignment.Top
            //});

            //// At the point (50,20), 50 x units wide
            //model.Annotations.Add(new ImageAnnotation
            //{
            //    ImageSource = image,
            //    X = new PlotLength(50, PlotLengthUnit.Data),
            //    Y = new PlotLength(20, PlotLengthUnit.Data),
            //    Width = new PlotLength(50, PlotLengthUnit.Data),
            //    HorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
            //    VerticalAlignment = OxyPlot.VerticalAlignment.Top
            //});

            //// Relative to the viewport, centered at the bottom, with offset (could also use bottom vertical alignment)
            //model.Annotations.Add(new ImageAnnotation
            //{
            //    ImageSource = image,
            //    X = new PlotLength(0.5, PlotLengthUnit.RelativeToViewport),
            //    Y = new PlotLength(1, PlotLengthUnit.RelativeToViewport),
            //    OffsetY = new PlotLength(-35, PlotLengthUnit.ScreenUnits),
            //    Height = new PlotLength(30, PlotLengthUnit.ScreenUnits),
            //    HorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
            //    VerticalAlignment = OxyPlot.VerticalAlignment.Top
            //});


            #endregion

            #region 手工划线，因为手工添加背景时会覆盖掉原来的表格线，所以必须手工画

            ////for (int i = 0; i < Points.Count; i++)
            ////{
            ////    int j = int.Parse(Math.Floor(i / 5.0).ToString());
            ////    if (j % 2 == 0)
            ////    {
            ////        CustomDataPoint dp0 = Points[0];
            ////        if (i > 0)
            ////        {
            ////            dp0 = Points[i - 1];
            ////        }
            ////        CustomDataPoint dp1 = Points[i];
            ////        var a = new OxyPlot.Annotations.RectangleAnnotation();
            ////        a.Stroke = OxyColor.Parse("#193D37");
            ////        a.StrokeThickness = 0;
            ////        //a.X = dp1.X;
            ////        //a.Y = dp0.X;
            ////        //a.Width = dp1.X - dp0.X;
            ////        //a.Height = 20;
            ////        a.MinimumX = dp1.X;
            ////        a.MaximumX = dp0.X;
            ////        a.Fill = OxyColor.Parse("#172422");
            ////        model.Annotations.Add(a);
            ////    }
            ////    else if (int.Parse(Math.Floor(i / 5.0).ToString()) % 2 == 1)
            ////    {
            ////        CustomDataPoint dp0 = Points[i - 1];
            ////        CustomDataPoint dp1 = Points[i];
            ////        var a = new OxyPlot.Annotations.RectangleAnnotation();
            ////        // a.Stroke = OxyColor.Parse("#FF3D37");
            ////        a.Stroke = OxyColor.Parse("#193D37");
            ////        a.MinimumX = dp1.X;
            ////        a.MaximumX = dp0.X;
            ////        a.Fill = OxyColor.Parse("#121A19");

            ////        a.StrokeThickness = 0;
            ////        model.Annotations.Add(a);
            ////    }
            ////}

            ////for (int i = 0; i < Points.Count; i++)
            ////{
            ////    //画是垂直线
            ////    if (Math.Abs(i) % 5 == 0)
            ////    {
            ////        var a = new OxyPlot.Annotations.LineAnnotation();
            ////        a.LineStyle = LineStyle.Solid;
            ////        a.Type = LineAnnotationType.Vertical;
            ////        a.StrokeThickness = 0.5;
            ////        a.Color = OxyColor.Parse("#193D37");
            ////        a.X = 0;
            ////        a.Y = Points[i].Y;
            ////        model.Annotations.Add(a);
            ////    }
            ////}

            ////for (int i = 0; i < 20; i++)
            ////{
            ////    //画是水平线
            ////    if (i % 5 == 0)
            ////    {
            ////        var a = new OxyPlot.Annotations.LineAnnotation();
            ////        a.LineStyle = LineStyle.Solid;
            ////        a.Type = LineAnnotationType.Horizontal;
            ////        a.StrokeThickness = 0.5;
            ////        a.Color = OxyColor.Parse("#193D37");
            ////        a.X = 0;
            ////        a.Y = i;
            ////        model.Annotations.Add(a);
            ////    }
            ////}
            #endregion

            #region 手工画线2，利用面积图

            //画背景
            var dpseries1 = new AreaSeries
            {
                //ItemsSource = DP0,
                DataFieldX = "X",
                DataFieldY = "Y",
                Color = OxyColor.Parse("#193D37"),
                Fill = OxyColor.Parse("#172422"),
                MarkerType = MarkerType.None,
                StrokeThickness = 1,
                TrackerKey = "InvisibleTracker",
                TrackerFormatString = "",
            };
            dpseries1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:00")), 20));
            dpseries1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:00")), 20));

            model.Series.Add(dpseries1);

            var dpseries2 = new AreaSeries
            {
                //ItemsSource = DP1,
                DataFieldX = "X",
                DataFieldY = "Y",
                Color = OxyColor.Parse("#193D37"),
                Fill = OxyColor.Parse("#121A19"),
                MarkerType = MarkerType.None,
                StrokeThickness = 1,
                TrackerKey = "InvisibleTracker",
                TrackerFormatString = "",
            };
            dpseries2.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 10:00")), 20));
            dpseries2.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:00")), 20));

            model.Series.Add(dpseries2);

            var dpseries3 = new AreaSeries
            {
                //ItemsSource = DP0,
                DataFieldX = "X",
                DataFieldY = "Y",
                Color = OxyColor.Parse("#193D37"),
                Fill = OxyColor.Parse("#172422"),
                MarkerType = MarkerType.None,
                StrokeThickness = 1,
                TrackerKey = "InvisibleTracker",
                TrackerFormatString = "",
            };
            dpseries3.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 11:00")), 20));
            dpseries3.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:00")), 20));

            model.Series.Add(dpseries3);

            var dpseries4 = new AreaSeries
            {
                //ItemsSource = DP1,
                DataFieldX = "X",
                DataFieldY = "Y",
                Color = OxyColor.Parse("#193D37"),
                Fill = OxyColor.Parse("#121A19"),
                MarkerType = MarkerType.None,
                StrokeThickness = 1,
                TrackerKey = "InvisibleTracker",
                TrackerFormatString = "",
            };
            dpseries4.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 12:00")), 20));
            dpseries4.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:00")), 20));

            model.Series.Add(dpseries4);

            var dpseries5 = new AreaSeries
            {
                //ItemsSource = DP0,
                DataFieldX = "X",
                DataFieldY = "Y",
                Color = OxyColor.Parse("#193D37"),
                Fill = OxyColor.Parse("#172422"),
                MarkerType = MarkerType.None,
                StrokeThickness = 1,
                TrackerKey = "InvisibleTracker",
                TrackerFormatString = "",
            };
            dpseries5.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 13:00")), 20));
            dpseries5.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:00")), 20));

            model.Series.Add(dpseries5);

            var dpseries6 = new AreaSeries
            {
                //ItemsSource = DP1,
                DataFieldX = "X",
                DataFieldY = "Y",
                Color = OxyColor.Parse("#193D37"),
                Fill = OxyColor.Parse("#121A19"),
                MarkerType = MarkerType.None,
                TrackerKey = "InvisibleTracker",
                StrokeThickness = 1,
            };
            dpseries6.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 14:00")), 20));
            dpseries6.Points.Add(new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 15:00")), 20));

            model.Series.Add(dpseries6);

            //画是垂直线
            for (int i = 1; i <= 20; i++)
            {
                if (i % 4 == 0)
                {
                    var c = new OxyPlot.Annotations.LineAnnotation();
                    c.LineStyle = LineStyle.Solid;
                    c.Type = LineAnnotationType.Vertical;
                    c.StrokeThickness = 0.5;
                    c.Color = OxyColor.Parse("#193D37");
                    c.X = Points[i].X;
                    c.Y = 20;
                    model.Annotations.Add(c);
                }
            }

            //画是水平线
            for (int i = 0; i <= 20; i++)
            {
                if (i % 5 == 0)
                {
                    var f = new OxyPlot.Annotations.LineAnnotation()
                    {
                        LineStyle = LineStyle.Solid,
                        Type = LineAnnotationType.Horizontal,
                        StrokeThickness = 0.5,
                        Color = OxyColor.Parse("#193D37"),
                        X = 0,
                        Y = i
                    };
                    model.Annotations.Add(f);
                }
            }
            #endregion

            //彩色坐标轴，用于绘制散点图
            var axis1 = new LinearColorAxis();
            axis1.Key = "ColorAxis";
            axis1.Minimum = 0.1;
            axis1.Maximum = 0.9;
            axis1.HighColor = OxyColors.OrangeRed;
            axis1.LowColor = OxyColors.CornflowerBlue;
            axis1.Position = AxisPosition.Top;
            axis1.StringFormat = "";
            axis1.IsAxisVisible = false;
            model.Axes.Add(axis1);

            // 散点图
            ScatterSeries scatterSeries = new ScatterSeries();
            scatterSeries.Title = "交易";
            scatterSeries.ColorAxisKey = "ColorAxis";
            scatterSeries.MarkerSize = 8;
            scatterSeries.MarkerType = MarkerType.Circle;
            //scatterSeries.TrackerFormatString = "有交易";
            //scatterSeries.LabelFormatString = "{5}";
            model.Series.Add(scatterSeries);



            ////成交量 aver 线
            //LineSeries AskBidAvererial = new LineSeries() { Title = "Aver" };
            //AskBidAvererial.BrokenLineThickness = 1;
            //AskBidAvererial.LineStyle = LineStyle.Solid;
            //AskBidAvererial.StrokeThickness = 1;
            //AskBidAvererial.Color = OxyColors.Yellow;
            //AskBidAvererial.CanTrackerInterpolatePoints = false;
            //AskBidAvererial.TrackerFormatString = "X={2},\nY={4},\nAdditionalInfo={Description}";
            ////AskBidAvererial.MarkerFill = OxyColors.Green;
            ////AskBidAvererial.MarkerType = MarkerType.Circle;
            //model.Series.Add(AskBidAvererial);
            var rd = new Random();


            //Task.Factory.StartNew(() =>
            //{
            //    for (int i = 0; i < Points.Count; i++)
            //    {
            //        CustomDataPoint dp0 = Points[i];
            //        CustomDataPoint dp1 = Points2[i];
            //        var aa = new ArrowAnnotation();
            //        if (i == 0)
            //        {
            //            aa.StartPoint = new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:00")), dp0.Y + dp1.Y);
            //            aa.EndPoint = new DataPoint(Points[i].X, dp0.Y + dp1.Y);
            //        }
            //        else
            //        {
            //            aa.StartPoint = new DataPoint(Points[i - 1].X, dp0.Y + dp1.Y);
            //            aa.EndPoint = new DataPoint(Points[i].X, dp0.Y + dp1.Y);
            //        }
            //        aa.HeadWidth = 1;
            //        aa.LineStyle = LineStyle.Solid;
            //        aa.HeadLength = 1;
            //        aa.Veeness = 1;
            //        aa.Color = OxyColors.DarkGray;
            //        aa.Tag = Points[i].X.ToString();
            //        model.Annotations.Add(aa);

            //    }
            //});

            #region 画线
            for (int i = 0; i < Points.Count; i++)
            {
                if (i % 2 == 0)
                {
                    var aa = new ArrowAnnotation();
                    aa.ToolTip = "高线";
                    if (i == 0)
                    {
                        aa.StartPoint = new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:00")), Points[i].Y);
                        aa.EndPoint = new DataPoint(Points[i].X, Points[i].Y);
                    }
                    else
                    {
                        aa.StartPoint = new DataPoint(Points[i - 1].X, Points[i - 1].Y);
                        aa.EndPoint = new DataPoint(Points[i].X, Points[i].Y);
                    }

                    aa.HeadWidth = 1;
                    aa.LineStyle = LineStyle.Dot;
                    aa.HeadLength = 1;
                    aa.Veeness = 1;
                    aa.Color = OxyColors.DarkGray;
                    aa.Tag = Points[i].X.ToString();
                    model.Annotations.Add(aa);

                    //// 线上加文本注释
                    //var a = new OxyPlot.Annotations.TextAnnotation();
                    //a.Text = $"Vol:{Points[i].Y.ToString()}";
                    //a.TextColor = OxyColors.Yellow;
                    //a.Background = OxyColor.Parse("#193D37");
                    //a.StrokeThickness = 1;
                    //a.Stroke = OxyColor.Parse("#308376");
                    //a.TextPosition = new DataPoint(Points[i].X, Points[i].Y + 0.5);
                    //model.Annotations.Add(a);

                }
                else
                {
                    var aa = new ArrowAnnotation();
                    aa.StartPoint = new DataPoint(Points[i - 1].X, Points[i - 1].Y);
                    aa.EndPoint = new DataPoint(Points[i].X, Points[i].Y);
                    aa.HeadWidth = 1;
                    aa.LineStyle = LineStyle.Solid;
                    aa.HeadLength = 1;
                    aa.Veeness = 1;
                    aa.Color = OxyColors.CornflowerBlue;
                    aa.Tag = Points[i].X.ToString();
                    model.Annotations.Add(aa);
                }
                double newx = DateTimeAxis.ToDouble(DateTimeAxis.ToDateTime(Points[i].X));
                scatterSeries.Points.Add(new ScatterPoint(Points[i].X, Points[i].Y, 1, 1, 200) { Value = 0, Tag = newx });

            }
            #endregion

            #region 画线
            for (int i = 0; i < Points2.Count; i++)
            {
                if (i % 2 == 0)
                {
                    var aa = new ArrowAnnotation();
                    aa.ToolTip = "低线";
                    if (i == 0)
                    {
                        aa.StartPoint = new DataPoint(DateTimeAxis.ToDouble(Convert.ToDateTime($"{datestr} 09:00")), Points2[i].Y);
                        aa.EndPoint = new DataPoint(Points2[i].X, Points2[i].Y);
                    }
                    else
                    {
                        aa.StartPoint = new DataPoint(Points2[i - 1].X, Points2[i - 1].Y);
                        aa.EndPoint = new DataPoint(Points2[i].X, Points2[i].Y);
                    }

                    aa.HeadWidth = 1;
                    aa.LineStyle = LineStyle.Dot;
                    aa.HeadLength = 1;
                    aa.Veeness = 1;
                    aa.Color = OxyColors.DarkGray;
                    aa.ToolTip = Points[i].X.ToString();
                    model.Annotations.Add(aa);
                }
                else
                {
                    var aa = new ArrowAnnotation();
                    aa.StartPoint = new DataPoint(Points2[i - 1].X, Points2[i - 1].Y);
                    aa.EndPoint = new DataPoint(Points2[i].X, Points2[i].Y);
                    aa.HeadWidth = 1;
                    aa.LineStyle = LineStyle.Solid;
                    aa.HeadLength = 1;
                    aa.Veeness = 1;
                    aa.Color = OxyColors.OrangeRed;
                    aa.ToolTip = Points[i].X.ToString();
                    model.Annotations.Add(aa);
                }

                double newx = DateTimeAxis.ToDouble(DateTimeAxis.ToDateTime(Points2[i].X));
                scatterSeries.Points.Add(new ScatterPoint(Points2[i].X, Points2[i].Y, 1, 1, 200) { Value = 1, Tag = newx });

            }
            #endregion


            #endregion


        }

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
}