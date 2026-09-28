using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using STC.Chart.Plugin.Converts;
using STC.Common.Utils;
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
    /// TenorChartControl.xaml 的交互逻辑
    /// </summary>
    public partial class TenorChartControl : UserControl
    {
        private Random r = new Random(13);
        private LinearAxis axisX;
        private LinearAxis axisY;
        private List<double> dateTimeValue = new List<double>();
        private Dictionary<string, Series> seriesDict = new Dictionary<string, Series>();
        private Dictionary<string, ScatterSeries> scatterDict = new Dictionary<string, ScatterSeries>();
        private DateConverter dateConverter;
        public ICommand ResetCommand { get { return new ComRelayCommand(OnResetCommand); } }
        public PlotModel PlotModel { get; set; }

        private List<TenorRange> tenorRangeList;
        public List<TenorRange> TenorRangeList
        {
            get { return tenorRangeList; }
            set
            {
                tenorRangeList=value;
                dateConverter.TenorRangeList = value;
            }
        }
        private DateTime curDateTime;
        public DateTime CurrenctDateTime
        {
            get { return curDateTime; }
            set
            {
                curDateTime = value;
                dateConverter.DtBase = curDateTime;
            }
        }

        private void OnResetCommand()
        {
            PlotModel.ResetAllAxes();
            PlotModel.InvalidatePlot(true);
        }

        public TenorChartControl()
        {
            InitializeComponent();

            dateConverter = this.Resources["dateConverter"] as DateConverter;
            TenorRangeList = TenorRange.Sample();
            CurrenctDateTime = DateTime.Now;

            string title = "";
            OxyColor cBorder = OxyColor.FromUInt32(0xFF193D37);
            PlotModel = new PlotModel
            {
                //PlotAreaBorderThickness = new OxyThickness(0),
                PlotAreaBorderColor = cBorder,
                Title = title,
                TitleToolTip = title,
                LegendPosition = LegendPosition.TopRight,
                LegendTextColor = OxyColor.Parse("#FFEBC8"),
                LegendFontSize = 12,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendMargin = 0,
                LegendItemSpacing = 10,
                LegendPlacement = LegendPlacement.Outside,
            };

            axisX = new LinearAxis();
            axisX.Position = AxisPosition.Bottom;
            axisX.MinorStep = 5;
            axisX.MajorStep = 60;
            axisX.LabelFormatter = a => TenorRange.ToTenorString(a, TenorRangeList);
            //axisX.LabelFormatter =new Func<double, string> (delegate (double d,string str)=>
            //{
            //});
            //axisX.MajorStep = 12;
            axisY = new LinearAxis();
            axisY.Position = AxisPosition.Left;
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

            //int dataCount = 3000;
            //int seriesCount = 1;
            //double[] trend = { 4.5, 4, 3.5, 3 };
            //double[] upDownRatio = { 0.45, 0.44, 0.5, 0.54 };
            //int[] signs = { -1, 1, -1, 1 };
            //int numberOfPoints = dataCount;
            //for (int j = 0; j < seriesCount; j++)
            //{
            //    var lineSeries = new LineSeries
            //    {
            //        Title = "Series " + (j + 1),
            //        MarkerSize = 3,
            //        MarkerType = MarkerType.Circle,
            //    };
            //    List<double> xs = TenorChartConst.TenorList;
            //    for (int i = 0; i < xs.Count; i++)
            //    {
            //        int sign = 1;
            //        double delta = r.NextDouble();
            //        //sign = delta < upDownRatio[j] ? 1 : -1;
            //        trend[j] += sign * r.Next(0, 10) * 0.001;
            //        lineSeries.Points.Add(new DataPoint(xs[i], trend[j]));

            //        //DateTime dt = DateTime.Now.AddDays(i);
            //        //double dtAxisVal = DateTimeAxis.ToDouble(dt);
            //        //if (j == 0)
            //        //{
            //        //    dateTimeValue.Add(dtAxisVal);
            //        //}
            //        //if (delta < 0.5)
            //        //{
            //        //    lineSeries.Points.Add(new DataPoint(dtAxisVal, trend[j]));
            //        //}
            //    }
            //    PlotModel.Series.Add(lineSeries);
            //}
            //dateTimeValue= TenorChartConst.TenorList;



            rangeSlider.Minimum = 0;
            rangeSlider.Maximum = dateTimeValue.Count - 1;
            rangeSlider.LowerValue = rangeSlider.Minimum;
            rangeSlider.UpperValue = rangeSlider.Maximum;
            rangeSlider.RangeMargin = 6;

            ////var controller = new PlotController();
            ////controller.UnbindAll();
            ////controller.BindMouseDown(OxyMouseButton.Left, PlotCommands.SnapTrack);
            ////controller.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control, PlotCommands.Track);
            ////controller.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift, PlotCommands.PointsOnlyTrack);
            ////plotView.Controller = controller;

            this.DataContext = this;
        }

        private LineSeries lineSeries;

        public void InitSeries(string name)
        {
            PlotModel.Series.Clear();
            lineSeries = new LineSeries
            {
                Title = name,
                MarkerSize = 3,
                MarkerType = MarkerType.None,
            };
            PlotModel.Series.Add(lineSeries);
            plotView.InvalidatePlot();
        }

        public void AddPoints(List<TenorPoint> list)
        {
            foreach(TenorPoint point in list)
            {
                TenorRange range = TenorRange.ActInside(point.UnitCount, point.Unit, TenorRangeList);
                if (range != null)
                {
                    int x = range.ActToInternal(point.UnitCount, point.Unit);
                    AddXValue(x);
                    lineSeries.Points.Add(new DataPoint(x, point.Value));
                }
            }
            InvalidChart();
        }

        public void AddPoints(string name, List<TenorPoint> list)
        {
            Series series = GetSeries(name);
            if (series != null)
            {
                if (series is LineSeries)
                {
                    foreach (TenorPoint point in list)
                    {
                        LineSeries lineSeries = series as LineSeries;
                        TenorRange range = TenorRange.ActInside(point.UnitCount, point.Unit, TenorRangeList);
                        if (range != null)
                        {
                            int x = range.ActToInternal(point.UnitCount, point.Unit);
                            AddXValue(x);
                            lineSeries.Points.Add(new DataPoint(x, point.Value));
                        }
                    }
                }
                else if (series is ScatterSeries)
                {
                    foreach (TenorPoint point in list)
                    {
                        ScatterSeries scatterSeries = series as ScatterSeries;
                        TenorRange range = TenorRange.ActInside(point.UnitCount, point.Unit, TenorRangeList);
                        if (range != null)
                        {
                            int x = range.ActToInternal(point.UnitCount, point.Unit);
                            AddXValue(x);
                            scatterSeries.Points.Add(new ScatterPoint(x, point.Value));
                        }
                    }
                }
            }
            
            InvalidChart();
        }

        public Series GetSeries(string name)
        {
            if (seriesDict.ContainsKey(name))
            {
                Series series = seriesDict[name];
                return series;
            }
            return null;
        }

        public void AddLineSeries(string name, List<TenorPoint> list)
        {
            Series series = AddLineSeries(name);
            AddPoints(name, list);
            InvalidChart();
            //LineSeries lineSeries = series as LineSeries;
            //foreach (ChartDataPo item in list)
            //{
            //    double dtAxisVal = DateTimeAxis.ToDouble(item.DateTime);
            //    lineSeries.Points.Add(new DataPoint(dtAxisVal, item.Value));
            //}
        }

        private Series AddLineSeries(string name)
        {
            var lineSeries = new LineSeries
            {
                Title = name,
                //MarkerSize = 3,
                //MarkerType= MarkerType.Circle,
            };
            PlotModel.Series.Add(lineSeries);
            seriesDict[name] = lineSeries;
            return lineSeries;
        }

        public void AddScatterSeries(string name, OxyColor color, MarkerType markerType, List<TenorPoint> list)
        {
            ScatterSeries scatterSeries = AddScatterSeries(name, color, markerType);
            AddPoints(name, list);
        }

        private ScatterSeries AddScatterSeries(string name, OxyColor color, MarkerType markerType)
        {
            ScatterSeries scatterSeries = new ScatterSeries
            {
                Title = name,
                MarkerType = markerType,
                MarkerStroke = color,
                MarkerFill = color,
            };
            PlotModel.Series.Add(scatterSeries);
            seriesDict[name] = scatterSeries;
            return scatterSeries;
        }


        private bool xValueChanged = false;
        private int xValueChangedCount = 0;

        private void AddXValue(double dtAxisVal)
        {
            if (!dateTimeValue.Contains(dtAxisVal))
            {
                dateTimeValue.Add(dtAxisVal);
                xValueChangedCount++;
                xValueChanged = true;
                //Console.WriteLine("AddXValue: {0}", DateTimeAxis.ToDateTime(dtAxisVal).ToLongDateString());
            }
        }

        public void InvalidChart()
        {
            if (xValueChanged)
            {
                rangeSlider.Maximum = dateTimeValue.Count - 1;
                rangeSlider.UpperValue = rangeSlider.UpperValue + xValueChangedCount;
                xValueChanged = false;
                xValueChangedCount = 0;
                RangeSlider_ValueChanged(rangeSlider.LowerValue, rangeSlider.UpperValue);
            }
            else
            {
                plotView.InvalidatePlot();
            }
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
                //Console.WriteLine("RangeSlider_ValueChanged: {0}={2},{1}={3}", min, max, pointMin, pointMax);
                axisX.Zoom(pointMin, pointMax);
                //this.PlotModel.InvalidatePlot(true);
                plotView.InvalidatePlot();
            }
        }
    }

    public class TenorPoint
    {
        public TenorUnitEnum Unit { get; set; }
        public int UnitCount { get; set; }
        public double Value { get; set; }

        public TenorPoint()
        {
        }

        public TenorPoint(int unitCount,TenorUnitEnum unit)
        {
            this.UnitCount = unitCount;
            this.Unit = unit;
        }

        public TenorPoint(int unitCount, TenorUnitEnum unit,double val):this(unitCount,unit)
        {
            this.Value = val;
        }
    }
}
