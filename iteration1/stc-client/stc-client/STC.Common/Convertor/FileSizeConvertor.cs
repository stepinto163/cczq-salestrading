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
    public class FileSizeConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }
            string txt = value.ToString();
            if (CommonUtils.IsNumeric(txt))
            {
                decimal num = txt.ConvertToDecimal();
                decimal num_KB = num / 1024M;

                if (num_KB > 1024)
                {
                    decimal num_MB = num_KB / 1024M;
                    if (num_MB > 1024)
                    {
                        decimal num_GB = num_MB / 1024M;
                        if (num_GB > 1024)
                        {
                            decimal num_TB = num_GB / 1024M;
                            return $"{num_TB.ToString("N2")}TB";
                        }
                        else
                        {
                            return $"{num_GB.ToString("N2")}GB";
                        }
                    }
                    else
                    {
                        return $"{num_MB.ToString("N2")}MB";
                    }
                }
                else
                {
                    return $"{num_KB.ToString("N2")}KB";
                }

            }
            return txt;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
