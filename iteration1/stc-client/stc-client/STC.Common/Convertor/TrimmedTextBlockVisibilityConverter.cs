using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Common.Convertor
{
    public class TrimmedTextBlockVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            TextBlock textBlock = (TextBlock)value;
            bool isTrim = IsTextTrimmed(textBlock, culture);

            if (isTrim)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 判断当前显示的内容是否显示不全被截断
        /// </summary>
        /// <param name="textBlock"></param>
        /// <returns></returns>
        private bool IsTextTrimmed(TextBlock textBlock, System.Globalization.CultureInfo culture)
        {
            Typeface typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            FormattedText formattedText = new FormattedText(
                textBlock.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground);
            bool isTrimmed = Math.Round(formattedText.Width, 7) > Math.Round(textBlock.ActualWidth, 7);
            return isTrimmed;
        }

    }
}
