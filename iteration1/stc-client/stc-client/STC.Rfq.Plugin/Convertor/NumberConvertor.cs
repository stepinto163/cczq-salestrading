using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Rfq.Plugin.Convertor
{
    /// <summary>
    /// 数字格式转化器
    /// </summary>
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
                            return temp.ToString("N4");
                        }
                    }
                    else
                        return temp.ToString("N4");

                }
                else if (value is long)
                {
                    long temp = CommonUtils.ConvertToLong(value.ToString());
                    if (parameter == null)
                    {
                        if (temp != 0)
                        {
                            return temp.ToString();
                        }
                    }
                    else
                    {
                        if (parameter.ToString() == "1")
                        {
                            if (temp >=100000)
                            {
                                return (temp / 1000).ToString() + " K";
                            }
                            else
                            {
                                return temp.ToString();
                            }
                        }
                        else
                        {
                            return temp.ToString();
                        }
                    }
                }
                else if (value is int)
                {
                    int temp = CommonUtils.ConvertToInt(value.ToString());
                    if (parameter == null)
                    {
                        if (temp != 0)
                        {
                            return temp.ToString();
                        }
                    }
                    else
                        return temp.ToString();
                }
                else if (value is string)
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
                    else if (parameter.ToString() == "2")
                    {
                        if (temp == decimal.Zero)
                        {
                            return "+0 bp";
                        }
                        else if (temp > 0)
                        {
                            return $"+{temp} bp";
                        }
                        else
                        {
                            return $"{temp} bp";
                        }
                    }
                    else
                    {
                        return CommonUtils.FormatNum(temp.ToString());
                    }
                }
            }
            return "--";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
