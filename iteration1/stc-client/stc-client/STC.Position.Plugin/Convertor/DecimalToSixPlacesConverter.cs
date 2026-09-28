using System;
using System.Globalization;
using System.Windows.Data;

namespace STC.Position.Plugin.Convertor
{
    public class DecimalToSixPlacesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal decimalValue = 0;
            if (value != null)
            {
                
                if (value.GetType() == typeof(decimal))
                {
                    decimalValue = (decimal)value;

                    return decimalValue == 0 ? "0" : decimalValue.ToString("N0", culture);
                }

                if (value.GetType() == typeof(double))
                {
                    decimalValue = Common.Utils.CommonUtils.ConvertToDecimal(Common.Utils.CommonUtils.ConvertToDouble(value.ToString()).ToString());
                    return decimalValue == 0 ? "0" : decimalValue.ToString("N0", culture);
                }

                if (value.GetType() == typeof(string))
                {
                    decimalValue = Common.Utils.CommonUtils.ConvertToDecimal(value.ToString());
                    return decimalValue == 0 ? "0" : decimalValue.ToString("N0", culture);
                }
            }

            return decimalValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
