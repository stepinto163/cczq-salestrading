using STC.Mmshe.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Mmshe.Plugin.Convertor
{
    public class FormatDigitConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "--";
            if ("1".Equals(parameter))
            {
                return MMsheCommonUtils.Format3Digit(value.ToString());
            }
            else 
            {
                return MMsheCommonUtils.Format4Digit(value.ToString());
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
