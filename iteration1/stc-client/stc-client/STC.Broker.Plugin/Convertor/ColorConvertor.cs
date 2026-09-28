using STC.Broker.Plugin.Model;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Broker.Plugin.Convertor
{
    /// <summary>
    /// 内部看板颜色转换器
    /// </summary>
    public class ColorConvertor : IValueConverter
    {
        Brush WHITE_ColorBrush;
        Brush GRAY_ColorBrush;
        Brush Yellow_ColorBrush;

        public ColorConvertor()
        {
            var converter = new System.Windows.Media.BrushConverter();

            WHITE_ColorBrush = new SolidColorBrush(Colors.White);
            GRAY_ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A4D4C"));
            Yellow_ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDA9A"));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string checkData = string.Empty;
            string securityID = string.Empty;
            if (value != null)
            {
                if (value is TransactionOrdermModel && parameter != null)
                {
                    TransactionOrdermModel data = value as TransactionOrdermModel;
                    PropertyInfo property = typeof(TransactionOrdermModel).GetProperty(parameter.ToString());
                    checkData = property.GetValue(data)?.ToString();
                    PropertyInfo securityIDproperty = typeof(TransactionOrdermModel).GetProperty("SecurityID");
                    securityID = securityIDproperty.GetValue(data)?.ToString();
                }
                else
                {
                    checkData = value.ToString();
                }
            }

            if (string.IsNullOrEmpty(checkData) || checkData == "--" || checkData == "0")
            {
                return GRAY_ColorBrush;
            }
            else
            {
                if (securityID == "99999")
                {
                    return Yellow_ColorBrush;
                }
                else
                {
                    return WHITE_ColorBrush;
                }
            } 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
