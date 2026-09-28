using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Broker.Plugin.Convertor
{

    [ValueConversion(typeof(string), typeof(Brush))]
    public class OperateTypeColorConverter : IValueConverter
    {
        static Brush White = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        static Brush Black = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            string data = value.ToString()+"";
            switch (data)
            {
                case "新意向":
                    return Application.Current.Resources["ColorBrush.Yellow"];
                case "部分确认":
                    return Application.Current.Resources["ColorBrush.Violet"];
                case "确认":
                case "全部确认":
                    return Application.Current.Resources["ColorBrush.Green2"];
                case "拒绝":

                case "交易撤销":
                    return Application.Current.Resources["ColorBrush.Red3"];
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


