using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
    /// OxyPlotPlugin.xaml 的交互逻辑
    /// </summary>
    public partial class OxyPlotPlugin : UserControl
    {
        private Random r = new Random(13);
        private DateTimeAxis axisX;
        private LinearAxis axisY;
        private List<double> dateTimeValue = new List<double>();

        public PlotModel PlotModel { get; set; }
        //public List<PlotModel> PlotModels { get; set; }

        public OxyPlotPlugin()
        {
            InitializeComponent();

            //PlotModels = new List<PlotModel>();

            string title = "";
            OxyColor cBorder = OxyColor.FromUInt32(0xFF193D37);
            PlotModel = new PlotModel
            {
                //PlotAreaBorderThickness = new OxyThickness(0),
                PlotAreaBorderColor = cBorder,
                Title = title,
                TitleToolTip = title,
                LegendPosition = LegendPosition.TopLeft,
                LegendTextColor = OxyColor.Parse("#FFEBC8"),
                LegendFontSize = 12,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendMargin = -5,
                LegendItemSpacing = 10,
                LegendPlacement = LegendPlacement.Inside,
            };

            axisX = new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "yyyy-MM-dd" };
            axisY = new LinearAxis();
            PlotModel.Axes.Add(axisX);
            PlotModel.Axes.Add(axisY);

            foreach (Axis item in PlotModel.Axes)
            {
                item.TextColor = OxyColors.White;
                item.ExtraGridlineColor = OxyColors.White;
                item.MinorGridlineColor = OxyColors.White;
                item.MinorTicklineColor = OxyColors.White;

                item.MinorTickSize = 0;
                item.MajorTickSize = 0;
                item.TicklineColor = cBorder;

                item.MajorGridlineColor = cBorder;
                item.MajorGridlineStyle = LineStyle.Solid;
            }

            int dataCount = 3000;
            int seriesCount = 4;
            double[] trend = { 4.5, 4, 3.5, 3 };
            double[] upDownRatio = { 0.45, 0.44, 0.5, 0.54 };
            int[] signs = { -1, 1, -1, 1 };
            int numberOfPoints = dataCount;
            for (int j = 0; j < seriesCount; j++)
            {
                var lineSeries = new LineSeries
                {
                    Title = "Series " + (j + 1),
                    //MarkerSize = 3,
                    //MarkerType= MarkerType.Circle,
                };
                for (int i = 0; i < numberOfPoints; i++)
                {
                    int sign = 1;
                    double delta = r.NextDouble();
                    sign = delta < upDownRatio[j] ? 1 : -1;
                    trend[j] += sign * r.Next(0, 10) * 0.001;
                    DateTime dt = DateTime.Now.AddDays(i);
                    double dtAxisVal = DateTimeAxis.ToDouble(dt);
                    if (j == 0)
                    {
                        dateTimeValue.Add(dtAxisVal);
                    }
                    if (delta < 0.5)
                    {
                        lineSeries.Points.Add(new DataPoint(dtAxisVal, trend[j]));
                    }
                }
                if (j == 3)
                {
                    //lineSeries.IsVisible = false;
                }
                else
                {
                    PlotModel.Series.Add(lineSeries);
                }
            }

            ScatterSeries scatterSeries1 = new ScatterSeries
            {
                Title = "Ofr",
                MarkerType = MarkerType.Triangle,
                MarkerStroke = OxyColors.Blue,
                MarkerFill = OxyColors.Blue,

            };
            ScatterSeries scatterSeries2 = new ScatterSeries
            {
                Title = "Bid",
                MarkerType = MarkerType.Triangle,
                MarkerStroke = OxyColors.Orange,
                MarkerFill = OxyColors.Orange
            };
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = DateTime.Now.AddYears(i);
                double dtAxisVal = DateTimeAxis.ToDouble(dt);
                scatterSeries1.Points.Add(new ScatterPoint(dtAxisVal, 4 + r.NextDouble() - 0.5));
                scatterSeries2.Points.Add(new ScatterPoint(dtAxisVal, 3 + r.NextDouble() - 0.5));
            }
            PlotModel.Series.Add(scatterSeries2);
            PlotModel.Series.Add(scatterSeries1);
            


            rangeSlider.Minimum = 0;
            rangeSlider.Maximum = dateTimeValue.Count - 1;
            rangeSlider.LowerValue = rangeSlider.Minimum;
            rangeSlider.UpperValue = rangeSlider.Maximum;
            rangeSlider.RangeMargin = 20;

            var controller = new PlotController();
            controller.UnbindAll();
            controller.BindMouseDown(OxyMouseButton.Left, PlotCommands.SnapTrack);
            controller.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control, PlotCommands.Track);
            controller.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift, PlotCommands.PointsOnlyTrack);
            plotView.Controller = controller;

            this.DataContext = this;
        }

        private void RangeSlider_ValueChanged(double lower, double upper)
        {
            int min = (int)Math.Round(lower);
            int max = (int)Math.Round(upper);
            if (min >= 0 && min < dateTimeValue.Count
                && max >= 0 && max < dateTimeValue.Count)
            {
                double pointMin = dateTimeValue[min];
                double pointMax = dateTimeValue[max];
                axisX.Zoom(pointMin, pointMax);
                plotView.InvalidatePlot();

                //lastMin = min;
                //lastMax = max;
                //refreshRange = false;

                //DateTimePoint pointMin = mainSeries[min];
                //DateTimePoint pointMax = mainSeries[max];
                //axisX.MinValue = pointMin.DateTime.Ticks;
                //axisX.MaxValue = pointMax.DateTime.Ticks;
                //Console.WriteLine("date start={0},end={1}",
                //  DateTimeAxis.ToDateTime(pointMin).ToString("yyyyMMdd"), DateTimeAxis.ToDateTime(pointMax).ToString("yyyyMMdd"));
            }
        }
    }
}
