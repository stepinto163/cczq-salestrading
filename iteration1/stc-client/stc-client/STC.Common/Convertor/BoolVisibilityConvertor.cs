using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
namespace STC.Common.Convertor
{
    public class BoolVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = false;
            if (value != null && value is bool)
                b = (bool)value;

            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Visibility)
                return (Visibility)value == Visibility.Visible;

            return false;
        }
    }
}
