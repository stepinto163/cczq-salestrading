using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    /// <summary>
    /// 显示转换器
    /// </summary>
    public class ReverseVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Visible;
            }

            if (value.ToString() == "Y")
            {
                return Visibility.Visible;
            }

            switch(value.ToString())
            {
                case "0":
                case "True":
                case "true":
                    return Visibility.Collapsed;
                case "1":
                case "False":
                case "false":
                    return Visibility.Visible;
                case "2":
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
