using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using STC.Rfq.Plugin.Utils;

namespace STC.Rfq.Plugin.Convertor
{
    public class CirculSizeToQtyConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return "--";
            }
            
            decimal d;

            if (parameter.ToString() == "kw")
            {
                if (decimal.TryParse(value.ToString(), out d))
                {
                    //d * 100000000 * 0.01m / 10000
                    //decimal qty = d * 100;
                    return (d / 1000).ToString("n2");
                }
            }
            else if (parameter.ToString() == "yi")
            {
                if (decimal.TryParse(value.ToString(), out d))
                {
                    return d.ToString("n2");
                }
            }

                return "--";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
