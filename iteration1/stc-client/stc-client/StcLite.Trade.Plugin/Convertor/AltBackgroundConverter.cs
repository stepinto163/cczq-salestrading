using System;
using System.Collections.Generic;

using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StcLite.Trade.Plugin.Convertor
{
    public class AltBackgroundConverter : IValueConverter
    {
        Brush odd_ColorBrush;
        Brush even_ColorBrush;

        public AltBackgroundConverter()
        {
            var converter = new System.Windows.Media.BrushConverter();

            odd_ColorBrush = (Brush)converter.ConvertFromString("#0F0F0F");
            even_ColorBrush = (Brush)converter.ConvertFromString("#141414");
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            // Get the index of a ListViewItem
            int index = listView.ItemContainerGenerator.IndexFromContainer(item);

            if (index % 2 == 0)
            {
                return odd_ColorBrush;
            }
            else
            {
                return even_ColorBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
