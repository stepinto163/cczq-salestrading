using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class MultiCompareConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2) // 确保有两个值被传递进来进行比较
            {
                if (values[0] == null && values[1] == null)
                    return true;
                else if (values[0] == null && values[1] != null)
                    return false;
                else
                    return values[0].Equals(values[1]); // 比较两个值是否相等，可以根据需要调整比较逻辑
            }
            return false; // 如果值不匹配或数量不正确，返回false或其他适当值
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // 不需要反向转换
        }
    }
}
