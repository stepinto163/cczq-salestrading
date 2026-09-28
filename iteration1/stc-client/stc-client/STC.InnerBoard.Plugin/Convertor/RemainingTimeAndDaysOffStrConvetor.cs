using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.InnerBoard.Plugin.Convertor
{
    public class RemainingTimeAndDaysOffStrConvetor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            StringBuilder text = new StringBuilder();
            if (values != null && values.Length > 0)
            {
                if (values.Length > 0)
                {
                    text.Append(values[0]);

                    if (values.Length > 1 && values[1] != null && !string.IsNullOrEmpty(values[1].ToString()))
                    {
                        text.Append($"({values[1]})");
                    }
                }
            }
            return text.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
