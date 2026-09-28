using BDB.Common.Model.OHLC;
using com.gateway.log;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using STC.Common.Model.Fix.Message;

using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace BDB.Price.Plugin.Chart.Deal
{
    /// <summary>
    /// ExecChart.xaml 的交互逻辑
    /// </summary>
    public partial class ExecChart : UserControl
    {
        public PlotController CustomPlotController { get; set; }

        private object _lock = new object();

        public string guid = Guid.NewGuid().ToString();

        public ExecChart()
        {
            ChartExecCache.Instance.BatchUpdateChartDataAction += Instance_BatchUpdateChartDataAction;

            InitializeComponent();

            try
            {
                InitChart();

                _areaSeries.ItemsSource = seriesList;
                _areaSeries2.ItemsSource = seriesList;
                _areaSeries3.ItemsSource = seriesList;

                //画直线
                this.Plot.ActualController.UnbindMouseDown(OxyMouseButton.Left);
                this.Plot.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack);

                var pc = Plot.ActualController;
                this.Plot.MouseDoubleClick += Plot_MouseDoubleClick;

                var customPlotController = new PlotController();
                customPlotController.BindMouseEnter(new DelegatePlotCommand<OxyMouseEventArgs>(
                    (view, controller, args) =>
                    {
                        try
                        {
                            if (_areaSeries.ItemsSource != null)
                            {
                                controller.AddHoverManipulator(view, new TrackerManipulator(view)
                                { LockToInitialSeries = false, Snap = false, PointsOnly = true }, args);
                            }
                            //if (_areaSeries.ItemsSource != null)
                            //{
                            //    controller.AddHoverManipulator(view, new TrackerManipulator(view)
                            //    { LockToInitialSeries = false, Snap = false, PointsOnly = true,
                            //        FiresDistance = 2.0,
                            //        CheckDistanceBetweenPoints = true,
                            //    }, args);
                            //}
                            //if (_areaSeries2.ItemsSource != null)
                            //{
                            //    controller.AddHoverManipulator(view, new TrackerManipulator(view)
                            //    {
                            //        LockToInitialSeries = false, Snap = false, PointsOnly = true,
                            //        FiresDistance = 2.0,
                            //        CheckDistanceBetweenPoints = true,
                            //    }, args);
                            //}
                            //if (_areaSeries3.ItemsSource != null)
                            //{
                            //    controller.AddHoverManipulator(view, new TrackerManipulator(view)
                            //    { LockToInitialSeries = false, Snap = false, PointsOnly = true,
                            //        FiresDistance = 2.0,
                            //        CheckDistanceBetweenPoints = true,
                            //    }, args);
                            //}
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }
                    }));

                CustomPlotController = customPlotController;

                this.Plot.Controller = CustomPlotController;


                //左键按住不放，移动图形
                this.Plot.Controller.UnbindMouseDown(OxyMouseButton.Right);
                this.Plot.Controller.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);
            }
            catch (Exception ex)
            {

            }
        }


        ~ExecChart()
        {

            Dispose();
        }

        private void Plot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Plot.Model.ResetAllAxes();
            Plot.InvalidatePlot(true);
        }

        #region UI部分

        public PlotModel model { get; set; }
        private DealDateTimeAxis _dateAxis;
        private DealDateTimeAxis _dateAxis_future2;
        private DealDateTimeAxis _dateAxis_future3;

        private CustomLinearAxis _valueAxis;

        private DealAreaSeries _areaSeries;
        private DealAreaSeries _areaSeries2;
        private DealAreaSeries _areaSeries3;

        private ScatterSeries _scatterSeries;
        private ScatterSeries _scatterSeries2;
        private ScatterSeries _scatterSeries3;

        private void InitChart()
        {
            OxyColor cBackground = OxyColor.FromUInt32(0xFF172422);
            OxyColor cBorder = OxyColor.FromUInt32(0xFF193D37);

            this.Plot.Background = new SolidColorBrush(Color.FromArgb(cBackground.A, cBackground.R, cBackground.G, cBackground.B));
            model = new PlotModel
            {
                DefaultFontSize = 10,
                EdgeRenderingMode = EdgeRenderingMode.PreferSharpness,
                Background = cBackground,
                PlotType = PlotType.XY,
                IsLegendVisible = false,
                PlotAreaBackground = cBackground,
                PlotAreaBorderColor = cBackground,
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotMargins = new OxyThickness(35, 5, 0, 10), //调节上边距
            };
            this.Plot.Model = model;

            //X轴
            // 初始化债券
            _dateAxis = new DealDateTimeAxis()
            {
                Key = "x1",
                MajorTickSize = 0,
                //ExtraGridlineThickness = 20,
                IntervalType = DateTimeIntervalType.Minutes,
                MajorStep = 3.0 / 24, // 1/24 = 1 hour, 1/24/2 = 30 minutes
                StringFormat = "HH:mm",

                Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 7, 0, 0)),
                Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 10, 0)),
                StartPosition = 0,
                EndPosition = 1
            };
            model.Axes.Add(_dateAxis);

            //初始化国债期货
            //_dateAxis_future = new DealDateTimeAxis()
            //{
            //    IsAxisVisible = false,
            //    MajorTickSize = 0,
            //    //ExtraGridlineThickness = 20,
            //    IntervalType = DateTimeIntervalType.Minutes,
            //    MajorStep = 1.0 / 24, // 1/24 = 1 hour, 1/24/2 = 30 minutes
            //    StringFormat = "HH:mm",

            //    Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 30, 0)),
            //    Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 11, 30, 0)),
            //    StartPosition = 0,
            //    EndPosition = 0.45
            //};
            //model.Axes.Add(_dateAxis_future);
            _dateAxis_future2 = new DealDateTimeAxis()
            {
                Key = "x2",
                FirstLabelIsEmpty = true,
                IsAxisVisible = false,
                MajorTickSize = 0,
                //ExtraGridlineThickness = 20,
                IntervalType = DateTimeIntervalType.Minutes,
                MajorStep = 1.0 / 24, // 1/24 = 1 hour, 1/24/2 = 30 minutes
                StringFormat = "HH:mm",

                Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 00, 0)),
                Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 00, 0)),
                StartPosition = 0.47,
                EndPosition = 0.94
            };
            model.Axes.Add(_dateAxis_future2);
            _dateAxis_future3 = new DealDateTimeAxis()
            {
                Key = "x3",
                FirstLabelIsEmpty = true,
                IsAxisVisible = false,
                MajorTickSize = 0,
                //ExtraGridlineThickness = 20,
                IntervalType = DateTimeIntervalType.Minutes,
                MajorStep = 1.0 / 24 / 4.0, // 1/24 = 1 hour, 1/24/2 = 30 minutes
                StringFormat = "HH:mm",

                Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 00, 0)),
                Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 16, 0)),
                StartPosition = 0.94,
                EndPosition = 1
            };
            model.Axes.Add(_dateAxis_future3);

            //Y轴
            _valueAxis = new CustomLinearAxis()
            {
                MajorTickSize = 30,
                StringFormat = "N4",

                //Angle = 60,
                Maximum = 10,
                Minimum = 0,
            };
            model.Axes.Add(_valueAxis);

            foreach (Axis item in model.Axes)
            {
                item.IsZoomEnabled = false;
                item.IsPanEnabled = false;

                item.TextColor = OxyColors.LightGray;
                item.ExtraGridlineColor = OxyColors.White;
                item.ExtraGridlineStyle = LineStyle.Solid;

                item.MinorGridlineColor = cBorder;
                item.MinorGridlineStyle = LineStyle.None;
                item.MinorTicklineColor = cBorder;
                item.MinorTickSize = 0;

                item.MajorGridlineColor = cBorder;
                item.MajorGridlineStyle = LineStyle.Solid;
                item.TicklineColor = cBorder;
                item.MajorTickSize = 0;

                //item.MinorGridlineColor = cBorder;
                //item.MinorGridlineStyle = LineStyle.Dot,
            }

            // 渐变色区域
            // 债券部分 
            _areaSeries = new DealAreaSeries()
            {
                //MarkerType = MarkerType.Circle,
                //MarkerSize = 1,
                //MarkerFill = OxyColors.Transparent,// OxyColor.Parse("#000000"),
                XAxisKey = "x1",
                StrokeThickness = 1,
                Color2 = OxyColors.Transparent,
                Color = OxyColor.Parse("#FF7D00"),
                //ConstantY2 = 0,
                DataFieldX = "X",
                DataFieldY = "Y",
            };
            model.Series.Add(_areaSeries);
            // 国债期货部分
            _areaSeries2 = new DealAreaSeries()
            {
                //MarkerType = MarkerType.Circle,
                //MarkerSize = 1,
                //MarkerFill = OxyColors.Transparent,// OxyColor.Parse("#000000"),
                IsVisible = false,
                XAxisKey = "x2",
                StrokeThickness = 1,
                Color2 = OxyColors.Transparent,
                Color = OxyColor.Parse("#FF7D00"),
                //ConstantY2 = 0,
                DataFieldX = "X",
                DataFieldY = "Y",
            };
            model.Series.Add(_areaSeries2);

            _areaSeries3 = new DealAreaSeries()
            {
                //MarkerType = MarkerType.Circle,
                //MarkerSize = 1,
                //MarkerFill = OxyColors.Transparent,// OxyColor.Parse("#000000"),
                IsVisible = false,
                XAxisKey = "x3",
                StrokeThickness = 1,
                Color2 = OxyColors.Transparent,
                Color = OxyColor.Parse("#FF7D00"),
                //ConstantY2 = 0,
                DataFieldX = "X",
                DataFieldY = "Y",
            };
            model.Series.Add(_areaSeries3);

            _scatterSeries = new ScatterSeries()
            {
                IsVisible = true,
                XAxisKey = "x1",
                MarkerFill = OxyColor.Parse("#FF7D00"),
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                DataFieldX = "X",
                DataFieldY = "Y",
            };
            model.Series.Add(_scatterSeries);

            _scatterSeries2 = new ScatterSeries()
            {
                IsVisible = false,
                XAxisKey = "x2",
                MarkerFill = OxyColor.Parse("#FF7D00"),
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                DataFieldX = "X",
                DataFieldY = "Y",
            };
            model.Series.Add(_scatterSeries2);

            _scatterSeries3 = new ScatterSeries()
            {
                IsVisible = false,
                XAxisKey = "x3",
                MarkerFill = OxyColor.Parse("#FF7D00"),
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                DataFieldX = "X",
                DataFieldY = "Y",
            };
            model.Series.Add(_scatterSeries3);
        }

        public void SetValuation(string securityId)
        {
            // 在Y轴坐标50%和80%处显示网格线
            string valuation = PriceHandler.Instance.GetValuation(securityId, string.Empty);
            if (string.IsNullOrEmpty(valuation) == false)
            {
                _valueAxis.ExtraGridlines = new double[1] { valuation.ConvertToDouble() };
                _valueAxis.ExtraGridlineStyle = LineStyle.DashDashDot; // 网格线样式
            }
            else
            {
                _valueAxis.ExtraGridlines = null;
            }
        }

        private decimal min = 0;
        private decimal max = 10;

        public void SetDataMinMax(decimal high, decimal low)
        {
            if (high != 0 && low != 0)
            {
                min = low;
                max = high;
            }
            else
            {
                min = 0;
                max = 10;
            }

            UpdateChartMinMax();

            _areaSeries.ConstantY2 = _areaSeries2.ConstantY2 = _areaSeries3.ConstantY2 = min.ConvertToDouble();

            model.PlotView?.InvalidatePlot();
        }

        public void InitDateAxis()
        {
            _dateAxis.Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 7, 0, 0));
            _dateAxis.Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 10, 0));

            _dateAxis_future2.Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 00, 0));
            _dateAxis_future2.Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 00, 0));

            _dateAxis_future3.Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 00, 0));
            _dateAxis_future3.Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 16, 0));
        }

        public void ClearData()
        {
            bool isChange = false;
            if (min != 0 || max != 10)
            {
                min = 0;
                max = 10;
                if (model != null)
                {
                    //DealDateTimeAxis xAxis = model.Axes[0] as DealDateTimeAxis;
                    //xAxis.Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 7, 0, 0));
                    //xAxis.Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 10, 0));

                    //LinearAxis _valueAxis = model.Axes[1] as LinearAxis;
                    decimal delta = (max - min) / 5;
                    if (this.ActualHeight <= 200)
                    {
                        delta = (max - min) / 2;
                    }
                    _valueAxis.Minimum = 0;
                    _valueAxis.Maximum = 10;
                    _valueAxis.MajorStep = delta.ConvertToDouble();
                    //AreaSeries _areaSeries = model.Series[0] as AreaSeries;
                    isChange = true;
                }
            }


            lock (cacheExecList)
            {
                if (cacheExecList != null && cacheExecList.Count > 0)
                {
                    cacheExecList.Clear();
                    isChange = true;
                }
            }

            lock (_lock)
            {
                if (seriesList.Count > 0 && seriesList.Count > 0)
                {
                    seriesList.Clear();
                    seriesDic.Clear();
                    isChange = true;
                }
            }

            if (isChange)
            {
                if (model != null)
                {
                    model.PlotView?.InvalidatePlot();
                }
            }
        }

        private ObservableCollection<DealDataPoint> seriesList = new ObservableCollection<DealDataPoint>();

        // 为查询速度
        private ConcurrentDictionary<string, DealDataPoint> seriesDic = new ConcurrentDictionary<string, DealDataPoint>();
        public void Update(object obj)
        {
            bool flag = false;
            if (obj is ExecutionReport)
            {
                flag = (obj as ExecutionReport).SecurityID == this.SecurityID;
            }
            else if (obj is OhlcMsg)
            {
                flag = (obj as OhlcMsg).securityid == this.SecurityID;
            }
            if (flag)
            {
                lock (cacheExecList)
                {
                    cacheExecList.Add(obj);
                }
            }
        }

        public void Dispose()
        {
            lock (cacheExecList)
            {
                if (cacheExecList != null)
                {
                    cacheExecList.Clear();
                }
            }

            lock (_lock)
            {
                if (seriesList != null)
                {
                    seriesList.Clear();
                    seriesDic.Clear();
                }
            }

            ChartExecCache.Instance.BatchUpdateChartDataAction -= Instance_BatchUpdateChartDataAction;
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Plot.SizeChanged -= Plot_SizeChanged;
                this.Plot.Controller.UnbindMouseEnter();
                this.Plot.Controller.UnbindMouseDown(OxyMouseButton.Left);
                this.Plot.Controller = null;
                this.Plot.Model = null;
                this.Plot.Dispose();

            }));
            GC.Collect();
        }

        #endregion

        private void Plot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (min == 0 && max == 10)
            {
                decimal delta = (max - min) / 5;
                if (this.ActualHeight <= 200)
                {
                    delta = (max - min) / 2;
                }
                _valueAxis.Minimum = min.ConvertToDouble();
                _valueAxis.Maximum = max.ConvertToDouble();
                _valueAxis.MajorStep = delta.ConvertToDouble();
            }
            else
            {
                UpdateChartMinMax();
            }


            model.PlotView?.InvalidatePlot();
        }

        private void UpdateChartMinMax()
        {
            if ((this.Parent as Grid).ActualHeight <= 200)
            {
                decimal delta = (max - min) / 2;

                if (delta <= 0)
                {
                    delta = 0.0025M;
                }

                _valueAxis.Minimum = (min - delta).ConvertToDouble();
                _valueAxis.Maximum = (max + delta / 2).ConvertToDouble();
                _valueAxis.MajorStep = delta.ConvertToDouble();
            }
            else
            {
                if ((max - min) / 0.0025M % 2 == 0)
                {
                    decimal delta = (max - min) / 5;
                    if (delta <= 0)
                    {
                        delta = 0.0025M;
                    }
                    _valueAxis.Minimum = (min - delta).ConvertToDouble();
                    _valueAxis.Maximum = (max + delta).ConvertToDouble();
                    _valueAxis.MajorStep = delta.ConvertToDouble();
                }
                else
                {
                    decimal delta = (max - min) / 13;
                    if (delta <= 0)
                    {
                        delta = 0.00125M;
                    }
                    _valueAxis.Minimum = (min - delta * 2).ConvertToDouble();
                    _valueAxis.Maximum = (max + delta).ConvertToDouble();
                    _valueAxis.MajorStep = (delta * 2).ConvertToDouble();
                }
            }
        }


        #region 更新数据


        private string _securityID;

        public string SecurityID
        {
            get { return _securityID; }
            set 
            { 
                if (!string.IsNullOrEmpty(value))
                {
                    _securityID = value;

                    if (CommonUtils.IsFutureBond(value))
                    {
                        _dateAxis.MajorTickSize = 0;
                        _dateAxis.MajorTickSize = 0;
                        //ExtraGridlineThickness = 20,
                        _dateAxis.IntervalType = DateTimeIntervalType.Minutes;
                        _dateAxis.MajorStep = 1.0 / 24; // 1/24 = 1 hour, 1/24/2 = 30 minutes
                        _dateAxis.StringFormat = "HH:mm";

                        _dateAxis.Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 30, 0));
                        _dateAxis.Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 11, 30, 0));
                        _dateAxis.StartPosition = 0;
                        _dateAxis.EndPosition = 0.47;

                        //_dateAxis.IsAxisVisible = true;
                        //_dateAxis_future.IsAxisVisible = true;
                        _dateAxis_future2.IsAxisVisible = true;
                        _dateAxis_future3.IsAxisVisible = false;
                        _areaSeries2.IsVisible = true;
                        _areaSeries3.IsVisible = true;

                        _scatterSeries2.IsVisible = true;
                        _scatterSeries3.IsVisible = true;

                        _valueAxis.StringFormat = "N3";

                        model.TitleToolTip = value;
                    }
                    else
                    {
                        _dateAxis.MajorTickSize = 0;
                        _dateAxis.MajorTickSize = 0;
                        //ExtraGridlineThickness = 20,
                        _dateAxis.IntervalType = DateTimeIntervalType.Minutes;
                        _dateAxis.MajorStep = 3.0 / 24; // 1/24 = 1 hour, 1/24/2 = 30 minutes
                        _dateAxis.StringFormat = "HH:mm";

                        _dateAxis.Minimum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 7, 0, 0));
                        _dateAxis.Maximum = DateTimeAxis.ToDouble(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 30, 0));

                        _dateAxis.StartPosition = 0;
                        _dateAxis.EndPosition = 1;

                        //_dateAxis.IsAxisVisible = true;
                        //_dateAxis_future.IsAxisVisible = false;
                        _dateAxis_future2.IsAxisVisible = false;
                        _dateAxis_future3.IsAxisVisible = false;
                        _areaSeries2.IsVisible = false;
                        _areaSeries3.IsVisible = false;

                        _scatterSeries2.IsVisible = false;
                        _scatterSeries3.IsVisible = false;

                        _valueAxis.StringFormat = "N4";

                        model.TitleToolTip = value;
                    }

                    model.InvalidatePlot(true);
                }
            }
        }

        private List<object> cacheExecList = new List<object>();

        private delegate void AsyncUpdateData(List<object> a);


        private void Instance_BatchUpdateChartDataAction()
        {
            List<object> newList = null;
            lock (cacheExecList)
            {
                if (cacheExecList != null && cacheExecList.Count > 0)
                {
                    newList = cacheExecList.ToList();
                    cacheExecList.Clear();
                }
            }

            if (newList != null && newList.Count > 0)
            {
                ThreadPoolHelper.InvokeThreadPool((o) =>
                {
                    BindCacheSource(newList);
                });
            }
        }

        public void BindCacheSource(List<object> newList)
        {
            AsyncUpdateData del = UpdateData;
            if (newList != null)
            {
                List<object> updateList = new List<object>();
                foreach (object obj in newList.ToList())
                {
                    if (obj is ExecutionReport)
                    {
                        ExecutionReport er = obj as ExecutionReport;
                        if (er.SecurityID != this.SecurityID)
                        {
                            continue;
                        }
                    }
                    else if (obj is OhlcMsg)
                    {
                        OhlcMsg ohlc = obj as OhlcMsg;
                        if (ohlc.securityid != this.SecurityID)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }


                    updateList.Add(obj);
                    if (updateList.Count == 30)
                    {
                        this.Dispatcher.BeginInvoke(del, updateList.ToList());
                        updateList.Clear();
                    }
                }
                if (updateList.Count > 0)
                {
                    this.Dispatcher.BeginInvoke(del, updateList.ToList());
                    updateList.Clear();
                }

                this.Dispatcher.BeginInvoke((Action)(() => {
                    if (_areaSeries.ItemsSource != null)
                        model.PlotView?.InvalidatePlot();
                    //if (_areaSeries2.ItemsSource != null)
                    //    model.PlotView?.InvalidatePlot();
                    //if (_areaSeries3.ItemsSource != null)
                    //    model.PlotView?.InvalidatePlot();
                }));
            }
        }

        private async void UpdateData(List<object> list)
        {
            foreach (object er in list)
            {
                BindData(er);
            }
            await System.Windows.Threading.Dispatcher.Yield();
        }


        public void BindData(object obj)
        {
            if (obj is ExecutionReport)
            {
                BindData(obj as ExecutionReport);
            }
            else if (obj is OhlcMsg)
            {
                BindData(obj as OhlcMsg);
            }
        }

        public void BindData(OhlcMsg obj)
        {
            if (obj.securityid != this.SecurityID)
            {
                return;
            }

            // x值
            string closetime = $"{obj.closetime.Substring(0, 17)}00";
            DateTime clostTimeDt = DateTime.Parse(closetime);
            double x = DateTimeAxis.ToDouble(clostTimeDt);
            
            // y值
            decimal currentExePrice = obj.close.ConvertToDecimal();

            // 圆点
            _scatterSeries.Points.Clear();
            _scatterSeries2.Points.Clear();
            _scatterSeries3.Points.Clear();

            if (clostTimeDt.ToString("HH:mm:ss").CompareTo("15:00:00") >= 0)
            {
                _scatterSeries3.Points.Add(new ScatterPoint(x, currentExePrice.ConvertToDouble()));
            }
            else if (clostTimeDt.ToString("HH:mm:ss").CompareTo("13:00:00") >= 0)
            {
                _scatterSeries2.Points.Add(new ScatterPoint(x, currentExePrice.ConvertToDouble()));
            }
            else
            {
                if (clostTimeDt.ToString("HH:mm:ss").CompareTo("11:30:00") == 0)
                {
                    LogHelper.Debug($"{obj.securityid}原始{closetime}值:{currentExePrice}");
                }
                _scatterSeries.Points.Add(new ScatterPoint(x, currentExePrice.ConvertToDouble()));
            }

            // 加点
            UpdateOhlcPoint(closetime, currentExePrice);
            if (clostTimeDt.ToString("HH:mm:ss").CompareTo("13:00:00") == 0)
            {
                // 拟一个11: 30的点， 把13：00的点值赋值给这个11: 30的点值
                UpdateOhlcPoint($"{clostTimeDt.ToString("yyyy-MM-dd ")}11:30:00", currentExePrice);
            }
        }

        public void UpdateOhlcPoint(string closetime, decimal currentExePrice)
        {
            int i = 0;

            DateTime clostTimeDt = DateTime.Parse(closetime);
            double x = DateTimeAxis.ToDouble(clostTimeDt);

            if (!seriesDic.ContainsKey(closetime))
            {
                DealDataPoint point = new DealDataPoint(x, currentExePrice.ConvertToDouble());
                point.Id = closetime;
                //point.Side = obj.Side;
                lock (_lock)
                {
                    if (seriesList.Count == 0)
                    {
                        seriesList.Add(point);
                        seriesDic[closetime] = point;
                    }
                    else
                    {
                        if (x > seriesList[seriesList.Count - 1].X)
                        {
                            seriesList.Add(point);
                            seriesDic[closetime] = point;
                        }
                        else
                        {
                            foreach (DealDataPoint dp in seriesList)
                            {
                                if (x < dp.X)
                                {
                                    break;
                                }
                                i++;
                            }
                            seriesList.Insert(i, point);
                            seriesDic[closetime] = point;
                        }
                    }
                }
            }
            else
            {
                DealDataPoint findItem = seriesDic[closetime];
                findItem.X = x;
                findItem.Y = currentExePrice.ConvertToDouble();
                findItem.Id = closetime;
                //findItem.Side = obj.Side;
            }
        }


        public void BindData(ExecutionReport obj)
        {
            if (obj.SecurityID != this.SecurityID)
            {
                return;
            }

            int i = 0;
            double x = DateTimeAxis.ToDouble(DateTime.Parse(obj.TransactTime));
            if (obj.OrdStatus == "4")
            {

                if (seriesList != null)
                {
                    try
                    {
                        lock (_lock)
                        {
                            if (seriesDic.ContainsKey(obj.ExecID))
                            {
                                DealDataPoint removeItem = seriesDic[obj.ExecID];
                                seriesList.Remove(removeItem);
                                seriesDic.TryRemove(obj.ExecID, out removeItem);
                            }

                        }
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                decimal currentExePrice = obj.LastPx.ConvertToDecimal();
                if (!seriesDic.ContainsKey(obj.ExecID))
                {
                    DealDataPoint point = new DealDataPoint(x, currentExePrice.ConvertToDouble());
                    point.Id = obj.ExecID;
                    point.Side = obj.Side;
                    lock (_lock)
                    {
                        if (seriesList.Count == 0)
                        {
                            seriesList.Add(point);
                            seriesDic[obj.ExecID] = point;
                        }
                        else
                        {
                            if (x > seriesList[seriesList.Count - 1].X)
                            {
                                seriesList.Add(point);
                                seriesDic[obj.ExecID] = point;
                            }
                            else
                            {
                                foreach (DealDataPoint dp in seriesList)
                                {
                                    if (x < dp.X)
                                    {
                                        break;
                                    }
                                    i++;
                                }
                                seriesList.Insert(i, point);
                                seriesDic[obj.ExecID] = point;
                            }
                        }
                    }
                }
                else
                {
                    DealDataPoint findItem = seriesDic[obj.ExecID];
                    findItem.X = x;
                    findItem.Y = currentExePrice.ConvertToDouble();
                    findItem.Id = obj.ExecID;
                    findItem.Side = obj.Side;
                }
            }


            #endregion
        }
    }
}
