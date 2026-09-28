using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
namespace STC.Common.Convertor
{
    public class VisibleConverterTwo : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            object obj = Visibility.Collapsed;
            if (value != null)
            {
                string strValue = value.ToString();
                if (strValue == "3")
                {
                    obj = Visibility.Visible;
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
