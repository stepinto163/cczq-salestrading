using com.gateway.log;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using STC.Common.Handler.Position;
using STC.Common.Model.Hedge;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Position.Plugin.View.Chart;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Position.Plugin.ViewModel.Chart
{
    public class RiskExposureChartViewModel : ViewModelBase
    {
        public PlotModel plotModel1 { get; set; }

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

        // 最后更新时间
        private string _lastupdatetime = string.Empty;
        public string LastUpdateTime
        {
            get
            {
                return _lastupdatetime;
            }

            set
            {
                if (_lastupdatetime != value)
                {
                    if (string.IsNullOrEmpty(_lastupdatetime))
                    {
                        _lastupdatetime = value;
                    }
                    else
                    {
                        DateTime dt1 =  CommonUtils.ConvertToDateTime(_lastupdatetime);
                        DateTime dt2 = CommonUtils.ConvertToDateTime(value);

                        if (dt2 > dt1)
                        {
                            _lastupdatetime = value;
                        }
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        public Dictionary<string, DV01DataModel> DV01RealData = new Dictionary<string, DV01DataModel>();
        public Dictionary<string, DV01DataModel> DV01RealSimData = new Dictionary<string, DV01DataModel>();
        public BarSeries barSeries1;
        public BarSeries barSeries2;
        public BarSeries barSeries3;
        public CategoryAxis categoryAxis1;
        public LinearAxis linearAxis1;

        //绘图X坐标轴 字典
        private Dictionary<string, string> XLabelDict = new Dictionary<string, string>();

        public RiskExposureChartViewModel()
        {
            InitXLabelDict();

            plotModel1 = new PlotModel
            {
                //PlotType = PlotType.XY,
                Title = "",
                Subtitle = "",
                TitleFontSize = 16,
                IsLegendVisible = true,
                PlotAreaBackground = OxyColor.Parse("#141414"),
                PlotAreaBorderColor = OxyColor.Parse("#193D37"),
                PlotAreaBorderThickness = new OxyThickness(1, 1, 1, 1),
                //LegendBorderThickness = 0,
                TitleColor = OxyColors.Wheat,
                //LegendItemAlignment = OxyPlot.HorizontalAlignment.Right,
                PlotMargins = new OxyThickness(80, 0, 0, 50), //调节上边距
            };

            plotModel1.Background = OxyColor.Parse("#0A0F0E");
            plotModel1.TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinView;
            plotModel1.TitlePadding = 4;
            
            //Legend legend1 = new Legend();
            //legend1.LegendBackground = OxyColor.FromArgb(200, 255, 255, 255);
            //legend1.LegendBorder = OxyColors.Black;
            //plotModel1.Legends.Add(legend1);

            categoryAxis1 = new CategoryAxis();
            categoryAxis1.Key = "y";
            categoryAxis1.TextColor = OxyColor.Parse("#D7D7D7");
            categoryAxis1.MajorGridlineColor = OxyColor.Parse("#193D37");
            categoryAxis1.MajorGridlineThickness = 1;
            categoryAxis1.MajorGridlineStyle = LineStyle.Solid;
            categoryAxis1.MinorGridlineStyle = LineStyle.Dot;
            categoryAxis1.GapWidth = 0.4;


            plotModel1.Axes.Add(categoryAxis1);

            linearAxis1 = new LinearAxis();
            linearAxis1.Key = "x";
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.Title = "X";
            linearAxis1.TextColor = OxyColor.Parse("#D7D7D7");
            //linearAxis1.StringFormat = "f2";
            linearAxis1.MajorGridlineColor = OxyColor.Parse("#193D37");
            linearAxis1.MajorGridlineThickness = 1;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.MinorGridlineThickness = 0.5;
            linearAxis1.StringFormat = "0.##";
            //linearAxis1.MajorStep = 0.5;
            //linearAxis1.MinorStep = 0.1;
            ////StringFormat = "f4",
            //linearAxis1.Minimum = 0;
            //linearAxis1.AbsoluteMinimum = 0;
            linearAxis1.IsZoomEnabled = false;
            plotModel1.Axes.Add(linearAxis1);

            barSeries1 = new BarSeries();
            barSeries1.XAxisKey = "x";
            barSeries1.YAxisKey = "y";
            barSeries1.Title = "实况";
            barSeries1.FillColor = OxyColor.Parse("#A08C00");
            barSeries1.BarWidth = 0.2;
            barSeries1.Tag = "Real-0";
            plotModel1.Series.Add(barSeries1);

            barSeries3 = new BarSeries();
            barSeries3.XAxisKey = "x";
            barSeries3.YAxisKey = "y";
            barSeries3.Title = "gap";
            barSeries3.FillColor = OxyColor.Parse("#000000");
            barSeries3.BarWidth = 0.1;
            barSeries3.Tag = "gap";
            plotModel1.Series.Add(barSeries3);

            barSeries2 = new BarSeries();
            barSeries2.XAxisKey = "x";
            barSeries2.YAxisKey = "y";
            barSeries2.Title = "实况+模拟";
            barSeries2.FillColor = OxyColor.Parse("#005DBB");
            barSeries2.BarWidth = 0.2;
            barSeries2.Tag = "RealSim-0";
            plotModel1.Series.Add(barSeries2);

            //plotModel1.InvalidatePlot(true);
            //请求数据并画图
            //LoadDataAndDraw();

            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

            //初始化持仓敞口绘图 X 坐标轴
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "InitRiskExposureXAxisKey", Info1 = CommonUtils.SerializeObject(XLabelDict) });

        }

        public void TryDispose()
        {
            
        }

        /// <summary>
        /// 初始化绘图X坐标轴
        /// </summary>
        private void InitXLabelDict()
        {
            XLabelDict.Clear();
            XLabelDict.Add("0.5", "6M");
            XLabelDict.Add("1", "1Y");
            XLabelDict.Add("2", "2Y");
            XLabelDict.Add("3", "3Y");
            XLabelDict.Add("5", "5Y");
            XLabelDict.Add("7", "7Y");
            XLabelDict.Add("10", "10Y");
            XLabelDict.Add("15", "15Y");
            XLabelDict.Add("20", "20Y");
            XLabelDict.Add("30", "30Y");
            XLabelDict.Add("40", "40Y");
            XLabelDict.Add("50", "50Y");
        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;
            //初始化 X 坐标轴 Lable
            if (obj.Code == "InitRiskExposureXAxisKey" && !string.IsNullOrEmpty(obj.Info1))
            {

                if (JsonSplit.IsJson(obj.Info1))
                {
                    //XLabelDict = CommonUtils.DeserializeObject<Dictionary<string, string>>(obj.Info1);
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
                    {
                        try
                        {

                            if (categoryAxis1.Labels.Count == 0)
                            {
                                //categoryAxis1.Labels.Clear();
                                //categoryAxis1.ActualLabels.Clear();
                                //barSeries3.Items.Clear();
                                List<string> lst = XLabelDict.Values.ToList();
                                for (int i = 0; i < lst.Count; i++)
                                {
                                    categoryAxis1.Labels.Add(lst[i]);
                                }


                                //画间隔
                                for (int i = 0; i < 12; i++)
                                {
                                    barSeries3.Items.Add(new BarItem() { CategoryIndex = i, Color = OxyColors.Automatic, Value = 0 });
                                }
                            }

                            categoryAxis1.IsZoomEnabled = false;

                            //plotModel1.InvalidatePlot(true);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex.Message);
                        }
                    }));
                }
            }

            if (obj.Code == "DrawDV01" && !string.IsNullOrEmpty(obj.Info1))
            {
                //画图
                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        if (JsonSplit.IsJson(obj.Info1))
                        {
                            ObservableCollection<KrdPositionDataModel> dic = CommonUtils.DeserializeObject<ObservableCollection<KrdPositionDataModel>>(obj.Info1);

                            //sortkey 3实况，2实况+模拟，1差值, 0数据区 - 模拟数据
                            //实况
                            KrdPositionDataModel dr = dic.Where(p => p.sortkey == "3").FirstOrDefault();
                            if (dr != null)
                            {
                                DV01RealData["6M"] = new DV01DataModel() { x = "6M", y = double.Parse(dr.F6M.ToString()) };
                                DV01RealData["1Y"] = new DV01DataModel() { x = "1Y", y = double.Parse(dr.F1Y.ToString()) };
                                DV01RealData["2Y"] = new DV01DataModel() { x = "2Y", y = double.Parse(dr.F2Y.ToString()) };
                                DV01RealData["3Y"] = new DV01DataModel() { x = "3Y", y = double.Parse(dr.F3Y.ToString()) };
                                DV01RealData["5Y"] = new DV01DataModel() { x = "5Y", y = double.Parse(dr.F5Y.ToString()) };
                                DV01RealData["7Y"] = new DV01DataModel() { x = "7Y", y = double.Parse(dr.F7Y.ToString()) };
                                DV01RealData["10"] = new DV01DataModel() { x = "10", y = double.Parse(dr.F10Y.ToString()) };
                                DV01RealData["15Y"] = new DV01DataModel() { x = "15Y", y = double.Parse(dr.F15Y.ToString()) };
                                DV01RealData["20Y"] = new DV01DataModel() { x = "20Y", y = double.Parse(dr.F20Y.ToString()) };
                                DV01RealData["30Y"] = new DV01DataModel() { x = "30Y", y = double.Parse(dr.F30Y.ToString()) };
                                DV01RealData["40Y"] = new DV01DataModel() { x = "40Y", y = double.Parse(dr.F40Y.ToString()) };
                                DV01RealData["50Y"] = new DV01DataModel() { x = "50Y", y = double.Parse(dr.F50Y.ToString()) };
                                barSeries1.Tag = "Real-1";
                                //DrawRealLine();
                            }
                            else
                            {
                                DV01RealData["6M"] = new DV01DataModel() { x = "6M", y = 0 };
                                DV01RealData["1Y"] = new DV01DataModel() { x = "1Y", y = 0 };
                                DV01RealData["2Y"] = new DV01DataModel() { x = "2Y", y = 0 };
                                DV01RealData["3Y"] = new DV01DataModel() { x = "3Y", y = 0 };
                                DV01RealData["5Y"] = new DV01DataModel() { x = "5Y", y = 0 };
                                DV01RealData["7Y"] = new DV01DataModel() { x = "7Y", y = 0 };
                                DV01RealData["10"] = new DV01DataModel() { x = "10", y = 0 };
                                DV01RealData["15Y"] = new DV01DataModel() { x = "15Y", y = 0 };
                                DV01RealData["20Y"] = new DV01DataModel() { x = "20Y", y = 0 };
                                DV01RealData["30Y"] = new DV01DataModel() { x = "30Y", y = 0 };
                                DV01RealData["40Y"] = new DV01DataModel() { x = "40Y", y = 0 };
                                DV01RealData["50Y"] = new DV01DataModel() { x = "50Y", y = 0 };
                                barSeries1.Tag = "Real-0";
                                //DrawRealLine();
                            }

                            //sortkey 3实况，2实况+模拟，1差值, 0数据区 - 模拟数据
                            //实况+模拟
                            KrdPositionDataModel drrealsim = dic.Where(p => p.sortkey == "2").FirstOrDefault();
                            if (drrealsim != null)
                            {
                                DV01RealSimData["6M"] = new DV01DataModel() { x = "6M", y = double.Parse(drrealsim.F6M.ToString()) };
                                DV01RealSimData["1Y"] = new DV01DataModel() { x = "1Y", y = double.Parse(drrealsim.F1Y.ToString()) };
                                DV01RealSimData["2Y"] = new DV01DataModel() { x = "2Y", y = double.Parse(drrealsim.F2Y.ToString()) };
                                DV01RealSimData["3Y"] = new DV01DataModel() { x = "3Y", y = double.Parse(drrealsim.F3Y.ToString()) };
                                DV01RealSimData["5Y"] = new DV01DataModel() { x = "5Y", y = double.Parse(drrealsim.F5Y.ToString()) };
                                DV01RealSimData["7Y"] = new DV01DataModel() { x = "7Y", y = double.Parse(drrealsim.F7Y.ToString()) };
                                DV01RealSimData["10"] = new DV01DataModel() { x = "10", y = double.Parse(drrealsim.F10Y.ToString()) };
                                DV01RealSimData["15Y"] = new DV01DataModel() { x = "15Y", y = double.Parse(drrealsim.F15Y.ToString()) };
                                DV01RealSimData["20Y"] = new DV01DataModel() { x = "20Y", y = double.Parse(drrealsim.F20Y.ToString()) };
                                DV01RealSimData["30Y"] = new DV01DataModel() { x = "30Y", y = double.Parse(drrealsim.F30Y.ToString()) };
                                DV01RealSimData["40Y"] = new DV01DataModel() { x = "40Y", y = double.Parse(drrealsim.F40Y.ToString()) };
                                DV01RealSimData["50Y"] = new DV01DataModel() { x = "50Y", y = double.Parse(drrealsim.F50Y.ToString()) };
                                barSeries2.Tag = "RealSim-1";
                                //DrawRealSimLine();
                            }
                            else
                            {
                                DV01RealSimData["6M"] = new DV01DataModel() { x = "6M", y = 0 };
                                DV01RealSimData["1Y"] = new DV01DataModel() { x = "1Y", y = 0 };
                                DV01RealSimData["2Y"] = new DV01DataModel() { x = "2Y", y = 0 };
                                DV01RealSimData["3Y"] = new DV01DataModel() { x = "3Y", y = 0 };
                                DV01RealSimData["5Y"] = new DV01DataModel() { x = "5Y", y = 0 };
                                DV01RealSimData["7Y"] = new DV01DataModel() { x = "7Y", y = 0 };
                                DV01RealSimData["10"] = new DV01DataModel() { x = "10", y = 0 };
                                DV01RealSimData["15Y"] = new DV01DataModel() { x = "15Y", y = 0 };
                                DV01RealSimData["20Y"] = new DV01DataModel() { x = "20Y", y = 0 };
                                DV01RealSimData["30Y"] = new DV01DataModel() { x = "30Y", y = 0 };
                                DV01RealSimData["40Y"] = new DV01DataModel() { x = "40Y", y = 0 };
                                DV01RealSimData["50Y"] = new DV01DataModel() { x = "50Y", y = 0 };
                                barSeries2.Tag = "RealSim-0";
                                //DrawRealSimLine();

                            }

                            DrawLine();
                        }
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
            }

            //更新 最后更新时间
            if (obj.Code == "UpdateDV01LastUpdateTime" && !string.IsNullOrEmpty(obj.Info1))
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, (Action)(() =>
                {
                    LastUpdateTime = obj.Info1;
                }));
            }

        }

        double max;
        double min;
        public void DrawLine()
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, (Action)(() =>
            {
                DrawRealLine();

                DrawRealSimLine();

                min = double.MaxValue;
                max = double.MinValue;
                bool isAdjust = false;
                List<DV01DataModel> lstdata = DV01RealData.Values.ToList();
                //CommonUtils.WriteConvertObjectToJsonToText(lstdata, "aa.txt");
                if (lstdata.Count > 0)
                {
                    double maxvalue = lstdata.Max(p => p.y);
                    double minvalue = lstdata.Min(p => p.y);

                    if (minvalue < min)
                    {
                        min = minvalue;
                        isAdjust = true;
                    }

                    if (maxvalue > max)
                    {
                        max = maxvalue;
                        isAdjust = true;
                    }
                }
                lstdata = DV01RealSimData.Values.ToList();
                //CommonUtils.WriteConvertObjectToJsonToText(lstdata, "bb.txt");
                if (lstdata.Count > 0)
                {
                    double maxvalue = lstdata.Max(p => p.y);
                    double minvalue = lstdata.Min(p => p.y);


                    if (minvalue < min)
                    {
                        min = minvalue;
                        isAdjust = true;
                    }

                    if (maxvalue > max)
                    {
                        max = maxvalue;
                        isAdjust = true;
                    }
                }

                if (isAdjust && min != max && min != double.MaxValue && max != double.MinValue)
                {
                    double delta = Math.Ceiling((max - min) / 6);
                    int n = delta.ToString().Length - 1;
                    delta = delta - delta % (Math.Pow(10, n));

                    linearAxis1.Minimum = min - delta * 0.2;
                    linearAxis1.Maximum = max + delta * 0.2;
                    linearAxis1.MajorStep = delta;

                    plotModel1?.InvalidatePlot(true);

                    plotModel1?.ResetAllAxes();
                }


            }));
        }

        /// <summary>
        /// 实况图
        /// </summary>
        public void DrawRealLine()
        {
            try
            {
                List<DV01DataModel> lstdata = DV01RealData.Values.ToList();
                // 数值范围
                //if (lstdata.Count > 0)
                //{
                //    double maxvalue = lstdata.Max(p => p.y);
                //    double minvalue = lstdata.Min(p => p.y);

                //    if (linearAxis1.Minimum > minvalue - minvalue * 0.2)
                //        linearAxis1.Minimum = minvalue - minvalue * 0.2;

                //    if (linearAxis1.ActualMaximum < maxvalue + maxvalue * 0.2)
                //        linearAxis1.Maximum = maxvalue + maxvalue * 0.2;
                //}

                // 画曲线
                if (DV01RealData.Count > 0)
                {
                    for (int i = 0; i < barSeries1.Items.Count; i++)
                    {
                        barSeries1.Items.Remove(barSeries1.Items[i]);
                        i--;
                    }

                    for (int i = 0; i < lstdata.Count; i++)
                    {
                        barSeries1.Items.Add(new BarItem() { CategoryIndex = i, Color = OxyColors.Automatic, Value = lstdata[i].y });
                    }
                }

                //plotModel1?.InvalidatePlot(true);

                //plotModel1?.ResetAllAxes();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        /// <summary>
        /// 模拟图
        /// </summary>
        public void DrawRealSimLine()
        {
            try
            {
                List<DV01DataModel> lstdata = DV01RealSimData.Values.ToList();
                //// 数值范围
                //if (lstdata.Count > 0)
                //{
                //    double maxvalue = lstdata.Max(p => p.y);
                //    double minvalue = lstdata.Min(p => p.y);

                //    if (linearAxis1.Minimum > minvalue - minvalue * 0.2)
                //        linearAxis1.Minimum = minvalue - minvalue * 0.2;

                //    if (linearAxis1.ActualMaximum < maxvalue + maxvalue * 0.2)
                //        linearAxis1.Maximum = maxvalue + maxvalue * 0.2;

                //        //linearAxis1.AbsoluteMinimum = linearAxis1.Minimum;
                //        //linearAxis1.AbsoluteMaximum = linearAxis1.Maximum;

                //}


                // 画曲线
                if (DV01RealSimData.Count > 0)
                {
                    for (int i = 0; i < barSeries2.Items.Count; i++)
                    {
                        barSeries2.Items.Remove(barSeries2.Items[i]);
                        i--;
                    }

                    for (int i = 0; i < lstdata.Count; i++)
                    {
                        barSeries2.Items.Add(new BarItem() { CategoryIndex = i, Color = OxyColors.Automatic, Value = lstdata[i].y });
                    }
                }



                //plotModel1?.InvalidatePlot(true);
                //plotModel1?.ResetAllAxes();

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

    }

    public class DV01DataModel
    {
        public string x { get; set; }
        public double y { get; set; }
    }
}
