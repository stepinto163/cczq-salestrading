using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.XBond.Plugin.Convertor
{
    /// <summary>
    /// 数字格式转化器 -新增国债期货返回3位小数
    /// </summary>
    public class NumberConvertor3 : IMultiValueConverter
    {
        public const string SOURCE_CTP = "5";
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null)
            {
                //多参数绑定
                if (values == null || values.Length < 2)
                    return "--";
                object entrypx = values[0];
                object source = values[1];
                int digit = 4;
                bool isFuture = false;
                if (SOURCE_CTP.Equals(source))
                {
                    isFuture = true;
                    //如果是期货，默认是国债期货3位小数
                    digit = 3;
                    //如果指定了第3参数MinChange
                    if (values.Length == 3)
                    {
                        digit = CommonUtils.GetDigit((string)values[2]);
                    }
                }
                if (entrypx != null)
                {
                    if (entrypx is string)
                    {
                        decimal outd = decimal.Zero;
                        bool isnumber = decimal.TryParse((entrypx as string), out outd);
                        if (isnumber)
                        {
                            decimal temp = CommonUtils.ConvertToDecimal(entrypx.ToString());
                            return isFuture ? temp.ToString("F" + digit) : CommonUtils.FormatNum(temp.ToString());
                        }
                        else
                            return (string)entrypx;
                    }
                    else if (entrypx is decimal)
                    {
                        decimal temp = CommonUtils.ConvertToDecimal(entrypx.ToString());
                        if (parameter == null)
                        {
                            if (temp != decimal.Zero)
                            {
                                return isFuture ? temp.ToString("F" + digit) : temp.ToString("F" + digit);
                            }
                        }
                        else
                            return isFuture ? temp.ToString("F" + digit) : temp.ToString("F" + digit);

                    }
                    else if (entrypx is long)
                    {
                        long temp = CommonUtils.ConvertToLong(entrypx.ToString());
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
                                if (temp >= 100000)
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
                    else if (entrypx is int)
                    {
                        int temp = CommonUtils.ConvertToInt(entrypx.ToString());
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
