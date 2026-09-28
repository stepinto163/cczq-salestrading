using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BDB.Trade.Plugin.View.Chart
{
    public class TrackerConverter : IMultiValueConverter
    {
        //正向修改
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string res = string.Empty;

            object tag = ((System.Windows.FrameworkElement)((OxyPlot.PlotModel)values[1]).PlotView).Tag;
            OxyPlot.Wpf.PlotView x = (System.Windows.FrameworkElement)((OxyPlot.PlotModel)values[1]).PlotView as OxyPlot.Wpf.PlotView;
            OxyPlot.PlotModel pm = x.ActualModel as OxyPlot.PlotModel;
            var jlSeries = (OxyPlot.Series.LineSeries)pm.Series[0];

            BDB.Trade.Plugin.ViewModel.Chart.FullData fulldata = (BDB.Trade.Plugin.ViewModel.Chart.FullData)tag;
            if (values[0] is ScreenPoint)
            {
                ScreenPoint sp = (ScreenPoint)values[0];
                var datapoint = jlSeries.InverseTransform(sp);
                string d = OxyPlot.Axes.DateTimeAxis.ToDateTime(datapoint.X).ToString();
                var info = fulldata.Points1.FirstOrDefault(o => o.X == datapoint.X);
                var info2 = fulldata.Points2.FirstOrDefault(o => o.X == datapoint.X);
                if (info != null)
                {
                    res = $"{d}\nPoint1:{info.Y}\nPoint2:{info2.Y}";
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
