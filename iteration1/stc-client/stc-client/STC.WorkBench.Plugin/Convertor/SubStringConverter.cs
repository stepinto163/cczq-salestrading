using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.WorkBench.Plugin.Convertor
{
    public class SubStringConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            string text = value.ToString();
            int length = 0;
            if (parameter != null)
            {
                length = CommonUtils.ConvertToInt(parameter.ToString());
            }

            if (text.Length > 0 && length > 0 && text.Length > length)
            {
                return text.Substring(0, length);
            }
            else
            {
                return text;
            }
            
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
