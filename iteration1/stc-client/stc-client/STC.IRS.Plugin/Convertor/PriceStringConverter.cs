using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.IRS.Plugin.Convertor
{
    class PriceStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 1)
            {
                string price = values[0]?.ToString();
                string securityid = values[1]?.ToString();

                if (string.IsNullOrEmpty(price))
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

                if (!string.IsNullOrEmpty(price))
                {
                    if (CommonUtils.IsNumeric(price))
                    {
                        if (CommonUtils.IsSpreadOrBasic(securityid))
                        {
                            decimal price_bp = price.ConvertToDecimal() * 100;
                            return price_bp.ToString("00.00");
                        }
                        else
                        {

                            return price.ConvertToDecimal().ToString("0.00##"); ;
                        }
                    }
                    else
                    {
                        return price;
                    }
                }
            }
            return "--";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
