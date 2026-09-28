using STC.Frame.Utils;
using STC.Mm.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Mm.Plugin.Convertor
{
    /// <summary>
    /// 
    /// </summary>
    public class TradeDateAndSettlTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[1] != DependencyProperty.UnsetValue)
            {
                string tradedate = values[0] == DependencyProperty.UnsetValue ? DateTimeEx.ServerTime.ToString("yyyyMMdd") : CommonUtils.ConvertToString(values[0]);
                string fastsettltype = CommonUtils.ConvertToString(values[1]);
                return CommonUtils.FormatSettlType(tradedate, fastsettltype);
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
