using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.XBond.Plugin.Convertor
{
    /// <summary>
    /// 数字格式转化器
    /// </summary>
    public class FormatPriceConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if ((value.ToString().Replace(" ","") != "--") && (!string.IsNullOrWhiteSpace(value.ToString())))
                    return CommonUtils.FormatNum(value.ToString());
                else
                    return value.ToString();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
