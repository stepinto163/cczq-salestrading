using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Mmshe.Plugin.Convertor
{
    public class BooleanConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            switch(value.ToString())
            {
                case "1":
                case "True":
                    return true;
                case "0":
                case "Flase":
                    return false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "1";
            }

            switch ((bool)value)
            {
                case true:
                    return "1";
                case false:
                    return "0";
            }

            return "0";
        }
    }
}
