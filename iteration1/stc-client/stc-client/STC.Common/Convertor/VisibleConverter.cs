using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class VisibleConverter : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            object obj = Visibility.Collapsed;
            if (value != null)
            {
                string strValue = value.ToString();
                if (strValue == "1")
                {
                    obj = Visibility.Visible;
                }
                else if (strValue == "2")
                {
                    obj = Visibility.Hidden;
                }
                else
                {
                    obj = Visibility.Collapsed;
                }
            }
            return obj;
        }
        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return "";
        }
    }
}