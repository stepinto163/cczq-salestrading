using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    /// <summary>
    /// 为空字符串转换成
    /// </summary>
    public class StringConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == string.Empty)
            {
                if (parameter != null)
                {
                    return parameter;
                }
                else
                {
                    return "--";

                }
            }
            //else if (value is string)
            //{
            //    return CommonUtils.FormatNum(value.ToString());
            //}
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
