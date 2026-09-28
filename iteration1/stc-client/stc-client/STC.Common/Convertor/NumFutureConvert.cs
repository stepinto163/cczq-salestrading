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
    public class NumFutureConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is decimal)
                {
                    decimal temp = Utils.CommonUtils.ConvertToDecimal(value.ToString());
                    if (temp != decimal.Zero)
                    {
                        return temp.ToString("N4");
                    }
                }
                else if (value is long)
                {
                    long temp = Utils.CommonUtils.ConvertToLong(value.ToString());
                    if (temp != 0)
                    {
                        return temp.ToString();
                    }
                }
                else if (value is int)
                {
                    int temp = Utils.CommonUtils.ConvertToInt(value.ToString());
                    if (temp != 0)
                    {
                        return temp.ToString();
                    }
                }
                else if (value is string)
                {
                    decimal temp = CommonUtils.ConvertToDecimal(value.ToString());
                    if (parameter == null)
                    {
                        if (temp != decimal.Zero)
                        {
                            //return temp.ToString("N4");
                            return CommonUtils.FormatNumFuture(temp.ToString());
                        }
                    }
                    else
                    {
                        return CommonUtils.FormatNumFuture(temp.ToString());
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
