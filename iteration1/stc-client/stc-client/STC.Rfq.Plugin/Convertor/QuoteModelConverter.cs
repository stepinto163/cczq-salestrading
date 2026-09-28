using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Rfq.Plugin.Convertor
{
    /// <summary>
    /// 报价模式
    /// </summary>
    public enum QuoteModelEnum
    {
        /// <summary>
        /// 手动
        /// </summary>
        Manual = 1,

        /// <summary>
        /// 自动
        /// </summary>
        Auto = 2,
    }

    public class QuoteModelConverter : IValueConverter
    {
        //报价模式1:手动、2:自动
        //          false   true

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            QuoteModelEnum s = (QuoteModelEnum)CommonUtils.ConvertToInt(value.ToString());
            return s == (QuoteModelEnum)int.Parse(parameter.ToString());
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            if (!isChecked)
            {
                return null;
            }
            return (QuoteModelEnum)int.Parse(parameter.ToString());
        }
    }
}
