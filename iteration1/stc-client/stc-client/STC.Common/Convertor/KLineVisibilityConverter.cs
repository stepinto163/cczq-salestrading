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
    public class KLineVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                string securityID = values[1] as string;
                if (!string.IsNullOrEmpty(securityID))
                {
                    if (CommonUtils.IsFutureBond(securityID))
                    {
                        if (LocalEnv.ConfInfo.IsKLine_F == false)
                        {
                            return Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        if (LocalEnv.ConfInfo.IsKLine == false)
                        {
                            return Visibility.Collapsed;
                        }
                    }
                }
            }
            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
