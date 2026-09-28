using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class LastImageVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || !(values[0] is string) || !(values[1] is string))
            {
                return System.Windows.Visibility.Collapsed; // 隐藏 Image
            }

            string value1 = (string)values[0];
            string value2 = (string)values[1];

            if ((value1 == "N" || value2 == "Y"))
            {
                return System.Windows.Visibility.Visible; // 显示 Image
            }
            else
            {
                return System.Windows.Visibility.Collapsed; // 隐藏 Image
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
