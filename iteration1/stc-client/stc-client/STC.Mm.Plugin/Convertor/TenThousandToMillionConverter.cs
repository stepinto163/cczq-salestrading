using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using STC.Mm.Plugin.Utils;

namespace STC.Mm.Plugin.Convertor
{
    public class TenThousandToMillionConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            decimal d;
            if (decimal.TryParse(value.ToString(),out d))
            {
                return d / 1000;
            }

            return string.Empty;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
