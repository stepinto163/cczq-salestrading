using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Mmshe.Plugin.Convertor
{
    /// <summary>
    /// 是否包含字符串转换器
    /// </summary>
    public class StringContainsConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                string str = value.ToString();
                string[] splitStr = str.Split(',');

                if (splitStr.Length > 1)
                {
                    if ((!splitStr.Contains(".")) && (!splitStr.Contains("4")))
                    {
                        if (splitStr.Contains("1"))
                        {
                            return true;
                        }
                    }
                }
                string subStr = parameter.ToString();
                return splitStr.Contains(subStr);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
