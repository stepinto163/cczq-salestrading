using Newtonsoft.Json;
using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
namespace STC.XBond.Plugin.Convertor
{
    public class ClockVisibilityConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string ispoint = values[0].ToString();
            string validUntilTime = values[1].ToString().Replace("{DependencyProperty.UnsetValue}","");
            string remainingTime = values[2].ToString().Replace("{DependencyProperty.UnsetValue}", "");
            string info1 = values[3].ToString().Replace("{DependencyProperty.UnsetValue}", "");
            if (ispoint == "1")
            {
                return Visibility.Collapsed;
            }
            else
            {
                if (string.IsNullOrEmpty(validUntilTime))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    if (JsonSplit.IsJson(info1))
                    {
                        OrderTimeModel otm = JsonConvert.DeserializeObject<OrderTimeModel>(info1);
                        if (!otm.ValidTimeType.StartsWith("0|"))
                        {
                            return Visibility.Visible;
                        }
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
