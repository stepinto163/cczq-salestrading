using OxyPlot.Axes;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BDB.Price.Plugin.Chart.Deal
{
    public class DataPointDataConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter.ToString().ToUpper() == "X")
            {
                if (values != null && values.Length == 1 && values[0] is OxyPlot.TrackerHitResult)
                {
                    OxyPlot.TrackerHitResult thr = values[0] as OxyPlot.TrackerHitResult;
                    return DateTimeAxis.ToDateTime(thr.DataPoint.X).ToString("HH:mm:ss");
                }
            }
            if (parameter != null && parameter.ToString().ToUpper() == "Y")
            {
                if (values != null && values.Length == 1 && values[0] is OxyPlot.TrackerHitResult)
                {
                    OxyPlot.TrackerHitResult thr = values[0] as OxyPlot.TrackerHitResult;
                    if (CommonUtils.IsFutureBond(thr.PlotModel.TitleToolTip))
                    {
                        return thr.DataPoint.Y.ToString("N3");
                    }
                    else
                    {
                        return thr.DataPoint.Y.ToString("N4");
                    }
                }
            }
            else
            {
                if (values != null && values.Length == 1 && values[0] is OxyPlot.TrackerHitResult)
                {
                    OxyPlot.TrackerHitResult thr = values[0] as OxyPlot.TrackerHitResult;

                    if (thr.Series != null && thr.Series is OxyPlot.Series.AreaSeries)
                    {
                        OxyPlot.Series.AreaSeries ass = thr.Series as OxyPlot.Series.AreaSeries;
                        if (ass != null && ass.ItemsSource != null && ass.ItemsSource is System.Collections.ObjectModel.ObservableCollection<DealDataPoint>)
                        {
                            System.Collections.ObjectModel.ObservableCollection<DealDataPoint> seriesList = ass.ItemsSource as System.Collections.ObjectModel.ObservableCollection<DealDataPoint>;
                            var findItem = seriesList.FirstOrDefault(o => o.X == thr.DataPoint.X);
                            if (findItem != null)
                            {
                                return ComboSourceUtil.Instance.FindEnum(findItem.Side, "side");
                            }
                        }
                    }
                }
            }
            
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
