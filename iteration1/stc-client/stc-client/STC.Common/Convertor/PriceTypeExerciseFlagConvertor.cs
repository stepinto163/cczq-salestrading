using System;
using System.Globalization;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class PriceTypeExerciseFlagConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            // priceType    0-意向;1-净价;2- 全价;3或空-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
            // exerciseFlag 0=行权收益率,1或空=到期收益率
            // calFlag 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价

            string pricetype = values[0].ToString().Replace("{DependencyProperty.UnsetValue}","");
            string exerciseFlag = values[1].ToString().Replace("{DependencyProperty.UnsetValue}", "");

            if (exerciseFlag == "0")
            {
                return "权";
            }
            else
            {
                if (string.IsNullOrEmpty(pricetype) || pricetype == "3")
                    return "到";
                else if (pricetype == "1")
                    return "净";
                else if (pricetype == "0")
                    return "意";
                else if (pricetype == "2")
                    return "全";
                else if (pricetype == "4")
                    return "利";
                else if (pricetype == "8")
                    return "未";
            }

            return "净";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
