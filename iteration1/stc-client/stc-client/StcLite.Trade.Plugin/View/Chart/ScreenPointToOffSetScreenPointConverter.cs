using OxyPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BDB.Trade.Plugin.View.Chart
{
    public class ScreenPointToOffSetScreenPointConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ScreenPoint)
            {
                ScreenPoint sp = (ScreenPoint)value;
                return new ScreenPoint(sp.X, sp.Y + 10);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }
    }
}
