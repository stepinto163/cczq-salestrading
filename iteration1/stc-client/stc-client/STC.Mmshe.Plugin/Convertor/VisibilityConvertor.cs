using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Mmshe.Plugin.Convertor
{
    /// <summary>
    /// 隐藏显示转换器
    /// </summary>
    public class VisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            if (value.ToString() == "Y")
            {
                return Visibility.Collapsed;
            }

            switch (value.ToString())
            {
                case "0":
                case "True":
                case "true":
                    return Visibility.Visible;
                case "1":
                case "False":
                case "false":
                    return Visibility.Collapsed;
                case "2":
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
