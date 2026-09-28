using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Common.Convertor
{
    public class ComboBoxDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as List<string>;
            if (list == null)
                return "";

            string fullText = string.Join(",", list);

            string param = parameter != null ? parameter.ToString() : null;

            if (param == "Trimmed")
            {
                return fullText.Length > 240 ? fullText.Substring(0, 240) + "..." : fullText;
            }

            if (param == "PA")
            {
                if (string.IsNullOrWhiteSpace(fullText))
                    return "";
                else
                {
                    return $"({fullText})";
                }
            }

            return fullText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
