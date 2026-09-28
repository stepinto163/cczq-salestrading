using STC.Broker.Plugin.Model.Transaction;
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
    public class HisRecordColorConverter : IValueConverter
    {
        static readonly Brush White = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        static readonly Brush Black = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        static readonly Brush Yellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9C152"));

        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            string para = param.ToString() + "";
            string data = value.ToString() + "";
            if ((para).Equals("fore"))
            {
                if (data == "")
                {
                    return White;
                }
                int index = data.IndexOf(HisRecordPo.Flag);
                string current = data.Substring(0, index);
                string last = data.Substring(index + 1, data.Count() - index - 1);
                if (!(current + "").Equals(last + ""))
                    return Application.Current.Resources["ColorBrush.Black"];
                else
                    return White;
            }
            if ((para).Equals("back"))
            {
                if (data == "")
                {
                    return null;
                }
                int index = data.IndexOf(HisRecordPo.Flag);
                string current = data.Substring(0, index);
                string last = data.Substring(index + 1, data.Count() - index - 1);
                if (!(current + "").Equals(last) && !string.IsNullOrEmpty(current))
                    return Yellow;
                else
                    return null;
            }
            return null;

        }

        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return "";
        }
    }

}
