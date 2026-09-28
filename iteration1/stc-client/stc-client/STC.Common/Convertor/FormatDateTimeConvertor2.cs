using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class FormatDateTimeConvertor2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                {
                    return string.Empty;
                }
                string d = value.ToString();

                if (string.IsNullOrEmpty(d))
                {
                    return d;
                }
                if (d.Equals("--"))
                {
                    return d;
                }
                DateTime formatDateTime = CommonUtils.ConvertToDateTime(d);
                if (parameter == null || formatDateTime == DateTime.MinValue || formatDateTime == DateTime.MaxValue)
                {
                    return d;
                }
                string format = parameter.ToString();

                return formatDateTime.ToString(format);
            }
            catch (Exception ex)
            {

            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
