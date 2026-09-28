using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign.Reserve
{
    [ValueConversion(typeof(string), typeof(Brush))]
    public class BridgeStatusColorConverter : IValueConverter
    {
        static Brush White = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4C6A7"));
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            string data = value.ToString() + "";
            switch (data)
            {
                case "已确认":
                    return Application.Current.Resources["ColorBrush.Green2"];
                case "已取消":
                    return White;
                case "待回复":
                    return Application.Current.Resources["ColorBrush.Yellow"];
                case "已成交":
                    return Application.Current.Resources["ColorBrush.Violet"];    
                default:
                    return White;
            }
        }
        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return "";
        }
    }
}
