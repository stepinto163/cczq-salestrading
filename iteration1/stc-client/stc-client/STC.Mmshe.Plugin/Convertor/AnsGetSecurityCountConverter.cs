using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Mmshe.Plugin.Convertor
{
    public class AnsGetSecurityCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return value;

            if (parameter.ToString() == "security")
            {
                return GetValue(value.ToString(), parameter.ToString());
            }
            else if (parameter.ToString() == "standard")
            {
                return GetValue(value.ToString(), parameter.ToString());
            }
            else if (parameter.ToString().Equals("securityFore"))
            {
                decimal d_sec;
                decimal d_sta;
                string sec = GetValue(value.ToString(), "security");
                string sta = GetValue(value.ToString(), "standard");
                if (decimal.TryParse(sec,out d_sec) && decimal.TryParse(sta,out d_sta) )
                {
                    if (d_sec >= d_sta)
                    {
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    }
                    else
                    {
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    }
                }
                else
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetValue(string str,string mode)
        {
            string text = string.Empty;

            if (string.IsNullOrEmpty(str)) return "--";

            switch (mode)
            {
                case "security":
                    text = str.Substring(0, str.IndexOf("/"));
                    break;
                case "standard":
                    text = str.Substring(str.IndexOf("/") + 1, str.Length - str.IndexOf("/") - 1);
                    break;
            }

            return text;
        }
    }
}
