using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.IRS.Plugin.Convertor
{
    /// <summary>
    /// 内部看板颜色转换器
    /// </summary>
    public class ColorConvertor : IValueConverter
    {
        Brush WHITE_ColorBrush;
        Brush GRAY_ColorBrush;

        public ColorConvertor()
        {
            var converter = new System.Windows.Media.BrushConverter();

            WHITE_ColorBrush = new SolidColorBrush(Colors.White);
            GRAY_ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A4D4C"));

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value != null)
            {
                string side = value.ToString();
                switch (side)
                {
                    case "--":
                    case "":
                        return GRAY_ColorBrush;
                    default:
                        return WHITE_ColorBrush;
                }
            }
            return GRAY_ColorBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
