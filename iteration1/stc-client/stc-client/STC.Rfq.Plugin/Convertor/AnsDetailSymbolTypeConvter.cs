using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Rfq.Plugin.Convertor
{
    public class AnsDetailSymbolTypeConvter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return string.Empty; }

            return STC.Rfq.Plugin.ViewModel.MmStatisticsViewModel.ConverSymbolTypeText(value.ToString());
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
