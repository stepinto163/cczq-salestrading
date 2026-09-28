using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using STC.Chart.Plugin.Converts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// ChartControl.xaml 的交互逻辑
    /// </summary>
    public partial class ChartControl : UserControl
    {
        public Collection<Item> Items { get; set; }

        public class Item
        {
            public string Label { get; set; }
            public double Value { get; set; }
        }
        private DateTimeAxis axisX;
        private CategoryAxis axisX2;
        private LinearAxis axisY;
        private List<double> dateTimeValue = new List<double>();
        private Dictionary<string, Series> seriesDict = new Dictionary<string, Series>();
        private List<ChartNamePo> updateDict = new List<ChartNamePo>();

        public PlotModel PlotModel { get; set; }

        public ChartControl()
        {
            InitializeComponent();

            string title = "";
            OxyColor cBorder = OxyColor.FromUInt32(0xFF193D37);
            dateConverter = new DateConverter();
            TenorRangeList = TenorRange.Sample();
            CurrenctDateTime = DateTime.Now;

            PlotModel = new PlotModel
            {
                //PlotAreaBorderThickness = new OxyThickness(0),
                PlotAreaBorderColor = cBorder,
                Title = title,
                TitleToolTip = title
            };
            axisX = new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "yyyy-MM-dd" };
            axisX2 = new CategoryAxis { Position = AxisPosition.Bottom};
            axisY = new LinearAxis();
            //PlotModel.Axes.Add(axisX);
            PlotModel.Axes.Add(axisX2);
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

            //Legend legend = new Legend
            //{
            //    LegendPosition = LegendPosition.TopLeft,
            //    LegendTextColor = OxyColor.Parse("#FFEBC8"),
            //    LegendFontSize = 12,
            //    LegendOrientation = LegendOrientation.Horizontal,
            //    LegendMargin = -5,
            //    LegendItemSpacing = 10,
            //    LegendPlacement = LegendPlacement.Inside,
            //};
            //PlotModel.Legends.Add(legend);

            this.Items = new Collection<Item>
            {
                new Item {Label="200", Value=37},
                new Item {Label = "400", Value=7},
                new Item {Label="600",Value=23},
                new Item {Label="800",Value=23},
                new Item {Label="1000",Value=23}
            };
            // PlotModel.Series.Add(new LineSeries { Title = "2009", ItemsSource = this.Items });
            var controller = new PlotController();
            controller.UnbindAll();
            controller.BindMouseDown(OxyMouseButton.Left, PlotCommands.SnapTrack);
            controller.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control, PlotCommands.Track);
            controller.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift, PlotCommands.PointsOnlyTrack);
            plotView.Controller = controller;

            rangeSlider.Minimum = 0;
            rangeSlider.Maximum = 0;
            rangeSlider.LowerValue = rangeSlider.Minimum;
            rangeSlider.UpperValue = rangeSlider.Maximum;
            rangeSlider.RangeMargin = 20;

            this.DataContext = this;

            Task.Factory.StartNew(() =>
            {
                UpdatePoints();
            });
        }

        private string axisXFmt = "yyyy-MM-dd";
        public string AxisXFormat
        {
            get { return axisXFmt; }
            set
            {
                axisXFmt = value;
                axisX.StringFormat = value;
            }
        }

        private string axisYFmt = "0.00";
        public string AxisYFormat
        {
            get { return axisYFmt; }
            set
            {
                axisYFmt = value;
                axisY.StringFormat = value;
            }
        }

        private DateConverter dateConverter;

        private List<TenorRange> tenorRangeList;
        public List<TenorRange> TenorRangeList
        {
            get { return tenorRangeList; }
            set
            {
                tenorRangeList = value;
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


        public void SetAxisXFormat(string fmt)
        {
            axisX.StringFormat = fmt;
        }

        public void SetAxisYFormat(string fmt)
        {
            axisY.StringFormat = fmt;
        }

        public void AddLineSeries(string name, List<ChartDataPo> list)
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

        public void changeSource(List<ChartNamePo> chartNamePos)
        {

            foreach (var item in chartNamePos)
            {
                string name = item.name;
                List<ChartDataPo> list = item.datas;
                AddLineSeries(name);
                AddPoints(name, list);
            }

            InvalidChart();
        }

        public void Update(string name, List<ChartDataPo> list)
        {
            updateDict.Add(new ChartNamePo()
            {
                datas = list,
                name = name
            });
        }

        object locker = new object();
        public void UpdatePoints()
        {
            Random random = new Random();
            while (true)
            {
                try
                {
                    if (updateDict.Count > 0)
                    {
                        lock (locker)
                        {
                            if (updateDict.Count > 0)
                            {
                                var demo = updateDict[0];
                                string name = demo.name;
                                updateDict.RemoveAt(0);
                                foreach (var item in demo.datas)
                                {
                                    item.Value = double.Parse(item.Value.ToString(AxisYFormat));
                                    Series series = GetSeries(name);
                                    if (series != null)
                                    {
                                        if (series is LineSeries)
                                        {
                                            LineSeries lineSeries = series as LineSeries;
                                            double dtAxisVal = DateTimeAxis.ToDouble(item.DateTime);
                                            AddXValue(dtAxisVal);
                                            var point = lineSeries.Points.Find(_ => _.X == dtAxisVal);
                                            lineSeries.Points.Remove(point);
                                            lineSeries.Points.Add(new DataPoint(dtAxisVal, point.Y + random.Next(-1, 1) * 0.01));
                                        }
                                        else if (series is ScatterSeries)
                                        {
                                            ScatterSeries scatterSeries = series as ScatterSeries;
                                            double dtAxisVal = DateTimeAxis.ToDouble(item.DateTime);
                                            AddXValue(dtAxisVal);
                                            var point = scatterSeries.Points.Find(_ => _.X == dtAxisVal);
                                            scatterSeries.Points.Remove(point);
                                            scatterSeries.Points.Add(new ScatterPoint(dtAxisVal, item.Value));
                                        }
                                    }
                                }

                                InvalidChart();
                            }
                        }


                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                Thread.Sleep(10);

            }
        }



        public void AddPoints(string name, List<TenorPoint> list)
        {
            Series series = GetSeries(name);

            if (series == null)
            {
                series = AddLineSeries(name);
            }

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
            InvalidChart();
        }

        public void AddPoint(string name, ChartDataPo item)
        {
            item.Value = double.Parse(item.Value.ToString(AxisYFormat));
            Series series = GetSeries(name);
            if (series != null)
            {
                if (series is LineSeries)
                {
                    LineSeries lineSeries = series as LineSeries;
                    double dtAxisVal = DateTimeAxis.ToDouble(item.DateTime);
                    AddXValue(dtAxisVal);
                    lineSeries.Points.Add(new DataPoint(dtAxisVal, item.Value));
                }
                else if (series is ScatterSeries)
                {
                    ScatterSeries scatterSeries = series as ScatterSeries;
                    double dtAxisVal = DateTimeAxis.ToDouble(item.DateTime);
                    AddXValue(dtAxisVal);
                    scatterSeries.Points.Add(new ScatterPoint(dtAxisVal, item.Value));

                }
            }
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

        public void AddPoints(string name, List<ChartDataPo> list)
        {
            Series series = GetSeries(name);
            if (series != null)
            {
                if (series is LineSeries)
                {
                    LineSeries lineSeries = series as LineSeries;
                    foreach (ChartDataPo item in list)
                    {
                        item.Value = double.Parse(item.Value.ToString(AxisYFormat));
                        double dtAxisVal = DateTimeAxis.ToDouble(item.DateTime);
                        AddXValue(dtAxisVal);
                        lineSeries.Points.Add(new DataPoint(dtAxisVal, item.Value));
                    }
                }
                else if (series is ScatterSeries)
                {
                    ScatterSeries scatterSeries = series as ScatterSeries;
                    foreach (ChartDataPo item in list)
                    {
                        item.Value = double.Parse(item.Value.ToString(AxisYFormat));
                        double dtAxisVal = DateTimeAxis.ToDouble(item.DateTime);
                        AddXValue(dtAxisVal);
                        scatterSeries.Points.Add(new ScatterPoint(dtAxisVal, item.Value));
                    }
                }
            }
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

        public void AddScatterSeries(string name, OxyColor color, MarkerType markerType, List<ChartDataPo> list)
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


        public void ClearSeries()
        {
            PlotModel.Series.Clear();
            seriesDict.Clear();
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

        public void SetSeriesVisible(string name, bool visible)
        {
            Series series = GetSeries(name);
            if (series != null)
            {
                series.IsVisible = visible;
                series.RenderInLegend = visible;
                //if (visible)
                //{
                //    PlotModel.Series.Add(series);
                //}
                //else
                //{
                //    PlotModel.Series.Remove(series);
                //}
                InvalidChart();
            }
        }

        public void InvalidChart()
        {
            Dispatcher.Invoke(() =>
            {
                if (xValueChanged)
                {
                    //rangeSlider.Minimum = 0;
                    //if (rangeSlider.Maximum <= 0)
                    //{
                    //    rangeSlider.Maximum = dateTimeValue.Count - 1;
                    //}
                    //else
                    //{
                    //    rangeSlider.Maximum = rangeSlider.Maximum;
                    //}
                    rangeSlider.Maximum = dateTimeValue.Count - 1;
                    //rangeSlider.LowerValue = rangeSlider.Minimum;
                    //if (rangeSlider.UpperValue < 0)
                    //{
                    //    rangeSlider.UpperValue = dateTimeValue.Count - 1;
                    //}
                    //else
                    //{
                    //    rangeSlider.UpperValue = rangeSlider.UpperValue + xValueChangedCount;
                    //}
                    rangeSlider.UpperValue = rangeSlider.UpperValue + xValueChangedCount;
                    xValueChanged = false;
                    xValueChangedCount = 0;
                    RangeSlider_ValueChanged(rangeSlider.LowerValue, rangeSlider.UpperValue);
                }
                else
                {
                    plotView.InvalidatePlot();
                }

            });

            //this.PlotModel.InvalidatePlot(true);
            //plotView.InvalidatePlot();
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

    public class ChartDataPo
    {
        public DateTime DateTime { get; set; }
        public double Value { get; set; }

        public ChartDataPo()
        {

        }

        public ChartDataPo(DateTime dt, double val)
        {
            DateTime = dt;
            Value = val;
        }
    }

    public class ChartNamePo
    {
        public List<ChartDataPo> datas { get; set; }
        public string name { get; set; }
    }
}
