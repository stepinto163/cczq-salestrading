using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.WorkTable.Plugin.Convertor
{
    class MultiAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 0)
            {
                bool flag = true;
                foreach (object v in values)
                {
                    if (v == System.Windows.DependencyProperty.UnsetValue)
                    {
                        return false;
                    }
                    else
                    {
                        flag = flag && !(bool)v;
                    }
                }
                return flag;
            }
            
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
