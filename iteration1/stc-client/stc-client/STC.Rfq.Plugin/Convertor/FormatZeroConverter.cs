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
    public class FormatZeroConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return "--"; }

            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    return "--";
                }
            }
            else if (parameter.ToString().ToLower() == "s")
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    return "--";
                }
            }

            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return "--"; }

            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    return "--";
                }
            }
            else if (parameter.ToString().ToLower() == "s")
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    return "--";
                }
            }

            return value;
        }
    }
}
