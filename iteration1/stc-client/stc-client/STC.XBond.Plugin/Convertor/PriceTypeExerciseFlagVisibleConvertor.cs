using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.XBond.Plugin.Convertor
{
    public class PriceTypeExerciseFlagVisibleConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // priceType    0-意向;1-净价;2- 全价;3或空-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
            // exerciseFlag 0=行权收益率,1或空=到期收益率
            // calFlag 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价
            string pricetype = "3";
            if (!(values[0] == null || values[0].ToString() == string.Empty))
                pricetype = values[0].ToString().Replace("{DependencyProperty.UnsetValue}", "3");

            string exerciseFlag = "1";
            if (!(values[1] == null || values[1].ToString() == string.Empty))
               exerciseFlag = values[1].ToString().Replace("{DependencyProperty.UnsetValue}", "1");

            if (exerciseFlag == "0")
            {
                //return "权";
                return Visibility.Visible;
            }
            else
            {
                if (string.IsNullOrEmpty(pricetype) || pricetype == "3")
                {
                    //return "到";
                    return Visibility.Collapsed;
                }
                else if (pricetype == "1")
                {
                    //return "净";
                    return Visibility.Visible;
                }
                else if (pricetype == "0")
                {
                    //return "意";
                    return Visibility.Collapsed;
                }
                else if (pricetype == "2")
                {
                    //return "全";
                    return Visibility.Visible;
                }
                else if (pricetype == "4")
                {
                    //return "利";
                    return Visibility.Visible;
                }
                else if (pricetype == "8")
                {
                    //return "未";
                    return Visibility.Collapsed;
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
