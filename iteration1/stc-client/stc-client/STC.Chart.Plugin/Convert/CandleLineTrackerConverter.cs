using OxyPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using OxyPlot.Series;
using STC.Common.Model.Chart;
using STC.Chart.Plugin.ViewModel;

namespace STC.Chart.Plugin.Converts
{
    public class CandleLineTrackerConverter : IMultiValueConverter
    {
        //正向修改
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string res = string.Empty;

            object tag = ((System.Windows.FrameworkElement)((OxyPlot.PlotModel)values[1]).PlotView).Tag;
            OxyPlot.Wpf.PlotView x = (System.Windows.FrameworkElement)((OxyPlot.PlotModel)values[1]).PlotView as OxyPlot.Wpf.PlotView;
            OxyPlot.PlotModel pm = x.ActualModel as OxyPlot.PlotModel;
            var jlSeries = (OxyPlot.Series.LineSeries)pm.Series[0];

            List<StockInfo> fulldata = (List<StockInfo>)tag;
            if (values[0] is ScreenPoint)
            {
                ScreenPoint sp = (ScreenPoint)values[0];
                var datapoint = jlSeries.InverseTransform(sp);
                string d = OxyPlot.Axes.DateTimeAxis.ToDateTime(datapoint.X).ToString();
                var info = fulldata.FirstOrDefault(o => o.Time.ToString() == d);
                if (info != null)
                {
                    // res = $"Date:{d}\nOpen:{info.O}\nHigh:{info.H}\nLow:{info.L}\nClose:{info.C}\nVolume:{info.V}";
                    res = $"Time:{d}\nHigh:{info.H}\nVolume:{info.V}";
                }
                else
                {
                    for (int i = 0; i < pm.Series.Count; i++)
                    {
                        var vcan = pm.Series[i];
                        if (vcan.GetType() == typeof(OxyPlot.Series.RectangleBarSeries))
                        {
                            OxyPlot.Series.RectangleBarSeries rb = (OxyPlot.Series.RectangleBarSeries)pm.Series[i];
                            TrackerHitResult ts = rb.GetNearestPoint(sp, false);
                            if (ts != null)
                            {
                                OxyPlot.Series.RectangleBarItem rbitem = (OxyPlot.Series.RectangleBarItem)ts.Item;
                                string dd = OxyPlot.Axes.DateTimeAxis.ToDateTime(rbitem.Y0).ToString();
                                var info0 = fulldata.FirstOrDefault(o => o.Time.ToString() == dd);
                                if (info0 != null)
                                {
                                    res = $"Date:{dd}\nOpen:{info0.O}\nHigh:{info0.H}\nLow:{info0.L}\nClose:{info0.C}\nVolume:{info0.V}";
                                }

                                return res;
                            }
                        }
                    }
                }
            }
            return res;
        }
        //反向修改
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            //返回空，标记不可双向转换
            return null;
        }
    }
}
