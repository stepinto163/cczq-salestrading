using STC.WorkBench.Plugin.ViewModel.WorkTable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.WorkBench.Plugin.Convertor
{
    public class TBookStatusEnumToBoolConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TBookStatusEnum s = (TBookStatusEnum)value;
            TBookStatusEnum b = (TBookStatusEnum)int.Parse(parameter.ToString());
            return s == (TBookStatusEnum)int.Parse(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            if (!isChecked)
            {
                return null;
            }
            TBookStatusEnum b = (TBookStatusEnum)int.Parse(parameter.ToString());
            return (TBookStatusEnum)int.Parse(parameter.ToString());
        }
    }
}
