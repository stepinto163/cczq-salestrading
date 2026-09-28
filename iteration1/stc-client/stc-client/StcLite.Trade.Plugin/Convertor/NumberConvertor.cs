using StcLite.Trade.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StcLite.Trade.Plugin.Convertor
{
    public class NumberConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is decimal)
                {
                    decimal temp = CommonUtils.ConvertToDecimal(value.ToString());
                    if (parameter == null)
                    {
                        if (temp != decimal.Zero)
                        {
                            //return temp.ToString("N4");
                            return CommonUtils.FormatNum(temp.ToString());
                        }
                    }
                    else if (parameter.ToString() == "1")
                    {
                        if (temp != 0)
                            return temp.ToString("N4");
                    }
                    else if (parameter.ToString() == "3")
                    {
                        //string ss = CommonUtils.FormatPriceZeroEqualsBar(value.ToString());
                        return CommonUtils.FormatPriceZeroEqualsBar(value.ToString());
                    }
                    else
                    {
                        return CommonUtils.FormatNum(temp.ToString());
                    }
                }
                else if (value is long)
                {
                    long temp = CommonUtils.ConvertToLong(value.ToString());
                    if (temp != 0)
                    {
                        return temp.ToString();
                    }
                }
                else if (value is int)
                {
                    int temp = CommonUtils.ConvertToInt(value.ToString());
                    if (temp != 0)
                    {
                        return temp.ToString();
                    }
                }
                else if (value is string)
                {
                    decimal temp = CommonUtils.ConvertToDecimal(value.ToString());
                    if (temp != decimal.Zero)
                    {
                        //return temp.ToString("N4");
                        return CommonUtils.FormatNum(temp.ToString());
                    }
                }
            }
            return "- -";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
