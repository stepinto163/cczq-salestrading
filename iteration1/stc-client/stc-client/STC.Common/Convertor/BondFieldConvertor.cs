using STC.Common.Utils;
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
    /// 债券价格 显示%或者--
    /// </summary>
    public class BondFieldConvertor : IMultiValueConverter
    {
        public const string MARKETINDICATOR_FUTURE = "5";
        public const string TYPE_AMOUNT= "Amount";
        public const string TYPE_PRICE= "Price";
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //多参数绑定
            if (values == null || values.Length < 3)
                return "--";
            object priceOrAmount = values[0];
            object marketindicator = values[1];
            object type = values[2];
            if (priceOrAmount == null || priceOrAmount == System.Windows.DependencyProperty.UnsetValue || string.IsNullOrEmpty(priceOrAmount.ToString()))
            {
                return "--";
            }

            if (TYPE_AMOUNT.Equals(type))
            {
                if (MARKETINDICATOR_FUTURE.Equals(marketindicator))
                {
                    return "--";
                }
                else
                {
                    decimal amount = priceOrAmount.ToString().ConvertToDecimal();
                    decimal amount2 = Math.Round(amount);
                    return amount2 == 0 ? "--" : amount2.ToString();
                }
                
            }
            else if (TYPE_PRICE.Equals(type))
            {
                if (MARKETINDICATOR_FUTURE.Equals(marketindicator))
                {
                    return "--";
                }
                else //债券的价格
                {
                    decimal price;
                    if (decimal.TryParse(priceOrAmount.ToString(), out price))
                    {
                        return price.ToString("N4");
                    }
                    return priceOrAmount.ToString();
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
