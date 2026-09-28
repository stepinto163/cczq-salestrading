using STC.Repo.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Repo.Plugin.Converter
{
    public class DetailTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is TotalBondItem)
            {
                TotalBondItem totalBondItem = value as TotalBondItem;
                return $"{totalBondItem.SecurityID}/{totalBondItem.Symbol}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
