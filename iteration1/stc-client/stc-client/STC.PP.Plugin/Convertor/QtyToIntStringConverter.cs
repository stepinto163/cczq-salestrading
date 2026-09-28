using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.PP.Plugin.Convertor
{
    public class QtyToIntStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is decimal)
                {
                    return ((int)(decimal)value).ToString("F0"); // 若要千位分隔使用 "N0"
                }
                if (value is double)
                {
                    return ((int)(double)value).ToString("F0");
                }
            }
            return value != null ? value.ToString() : "";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
