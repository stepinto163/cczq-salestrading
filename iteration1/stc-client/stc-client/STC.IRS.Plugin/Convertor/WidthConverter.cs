using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.IRS.Plugin.Convertor
{
    public class WidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double totalWidth = (double)values[0];
            if (totalWidth > 0)
            {
                int i = 0;
                foreach(double width in values)
                {
                    if (i == 0)
                        totalWidth -= 16;
                    else
                        totalWidth -= width;
                    i++;
                }
            }
            return totalWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
