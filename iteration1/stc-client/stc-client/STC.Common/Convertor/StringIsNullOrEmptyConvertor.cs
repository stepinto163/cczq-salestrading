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
    /// 字符串是否为空转换器
    /// </summary>
    public class StringIsNullOrEmptyConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                if (value == null)
                {
                    return true;
                }

                return string.IsNullOrEmpty(value.ToString());
            }
            else
            {
                string res = string.IsNullOrEmpty((value?? "") as string) ? "True" : "False";
                return res;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
