using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Rfq.Plugin.Convertor
{
    public class ANSAttentionForeConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = (Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");

            if (value == null) { return string.Empty; }

            int digit;

            if (int.TryParse(value.ToString(),out digit))
            {
                if (digit == 0)
                {
                    brush = (Brush)(new BrushConverter()).ConvertFromString("#A0A0A0");
                }
            }

            return brush;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
