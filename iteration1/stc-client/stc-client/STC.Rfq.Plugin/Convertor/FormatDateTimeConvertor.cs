using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Rfq.Plugin.Convertor
{
    /// <summary>
    /// 时间格式转换器
    /// </summary>
    public class FormatDateTimeConvertor : IValueConverter
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
