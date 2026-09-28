using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class RadioButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null && parameter != null)
                {
                    string str = value.ToString();
                    string subStr = parameter.ToString();
                    return str.Contains(subStr);
                }
            }
            catch
            {
            }
            
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (parameter != null)
                {
                    if ((bool)value == true)
                    {
                        return parameter.ToString();
                    }
                }
            }
            catch
            {
            }
            
            return string.Empty;
        }
    }
}
