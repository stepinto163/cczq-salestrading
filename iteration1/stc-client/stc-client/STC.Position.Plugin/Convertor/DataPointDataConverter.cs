using OxyPlot.Series;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Position.Plugin.Convertor
{
    public class DataPointDataConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return "";

            string res = string.Empty;

            if (values != null && values.Length == 1 && values[0] is OxyPlot.TrackerHitResult)
            {
                
                OxyPlot.TrackerHitResult THR = values[0] as OxyPlot.TrackerHitResult;
                
                int indx = 0;
                List<string> lablist = (THR.YAxis as OxyPlot.Axes.CategoryAxis).Labels;

                if (lablist.Count == 0)
                    return res;
                
                if (THR.Item is OxyPlot.Series.BarItem)
                {
                    BarItem hli = THR.Item as OxyPlot.Series.BarItem;
                    indx = hli.CategoryIndex;
                    
                    res = $"DV01 {lablist[indx]} (元)";

                    string currentDate = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");

                    string val = hli.Value.ToString("N0");
                    string title = (THR.Series as OxyPlot.Series.Series).Title;
                    res = res + $"\n{currentDate}\n{title}: {val}";
                }

                ////for (int i = 0; i < THR.PlotModel.Series.Count; i++)
                ////{
                ////    var item = THR.PlotModel.Series[i];
                ////    if (item.GetType() == typeof(OxyPlot.Series.BarSeries))
                ////    {
                ////        OxyPlot.Series.BarSeries BS = item as OxyPlot.Series.BarSeries;
                ////        if (BS.Tag.ToString() == "Real-1")
                ////        {
                ////            string v = BS.Items[indx].Value.ToString("N0");
                ////            res = res + $"\n{BS.Title}: {v}";
                ////        }

                ////        if (BS.Tag.ToString() == "RealSim-1")
                ////        {
                ////            string v = BS.Items[indx].Value.ToString("N0");
                ////            res = res + $"\n{BS.Title}: {v}";
                ////        }
                ////    }
                ////}

                return res;
            }

            return res;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
