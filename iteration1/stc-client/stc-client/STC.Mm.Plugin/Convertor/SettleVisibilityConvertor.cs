using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Mm.Plugin.Convertor
{
    /// <summary>
    /// 隐藏显示转换器
    /// </summary>
    public class SettleVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }
            switch(value.ToString())
            {
                case "":
                case "+1":
                    return Visibility.Collapsed;
                case "+0":
                case "远":
                case "多":
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
