using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class LastIsValidConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string fieldValue1 = values[0] as string; // 第一个字段的值
            string fieldValue2 = values[1] as string; // 第二个字段的值

            if (fieldValue1 == "N" || fieldValue2 == "Y")
            {
                return true; 
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
