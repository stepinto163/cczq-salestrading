using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class MoreSettleContainsConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string str = value.ToString();
                string[] splitStr = str.Split(',');

                bool isContainT0 = false;
                bool isContainFwd = false;
                
                foreach (string ss in splitStr)
                {
                    if (ss == "1")
                        isContainT0 = true;
                    if ((ss.Contains("+0") || ss.Contains("+1")) && ss.Length > 3)
                        isContainFwd = true;
                }

                return isContainFwd && isContainT0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
