using STC.Common.Api;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class MultiVisibilityConverter : IMultiValueConverter
    {
        public const string MARKETINDICATOR_FUTURE = "5";
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null)
            {
                object marketindicator = values[0];
                object settletype = values[1];

                if (settletype == null)
                {
                    return Visibility.Collapsed;
                }
                string key = settletype.ToString();
                if (!"1".Equals(key))
                {
                    if ((!key.Contains("1") || key.Contains(".") || key.Contains("4")))
                    {
                        return Visibility.Collapsed;
                    }
                }
                string[] keyArr = key.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                //string param = parameter.ToString();
                //国债期货 清算速度不显示
                if (MARKETINDICATOR_FUTURE.Equals(marketindicator))
                {
                    return Visibility.Collapsed;
                }
                if (!MARKETINDICATOR_FUTURE.Equals(marketindicator) && values.Length > 3
                    && !CommonUtils.IsBrokerSecurity(values[2].ToString())
                    && !ApiConsts.CDH.Equals(values[3].ToString()) && !"INNER".Equals(values[3].ToString()))
                {
                    return Visibility.Collapsed;
                }   

                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
