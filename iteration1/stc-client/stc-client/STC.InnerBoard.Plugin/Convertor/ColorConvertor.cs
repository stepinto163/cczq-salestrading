using STC.InnerBoard.Plugin.ViewModel.Data;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.InnerBoard.Plugin.Convertor
{
    public class EmptyColorConvertor : IValueConverter
    {
        Brush WHITE_ColorBrush;
        Brush GRAY_ColorBrush;
        Brush Yellow_ColorBrush;

        public EmptyColorConvertor()
        {
            var converter = new System.Windows.Media.BrushConverter();

            WHITE_ColorBrush = new SolidColorBrush(Colors.White);
            GRAY_ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A4D4C"));
            Yellow_ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDA9A"));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string checkData = string.Empty;
            if (value != null)
            {
                checkData = value.ToString();
            }

            if (string.IsNullOrEmpty(checkData) || checkData == "--" || checkData == "0")
            {
                return GRAY_ColorBrush;
            }
            else
            {
                return WHITE_ColorBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 内部看板颜色转换器
    /// </summary>
    public class ColorConvertor : IMultiValueConverter
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

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string checkData = string.Empty;
            string securityID = string.Empty;
            if (values != null && values.Length == 2)
            {
                //if (value is InnerOrderData && parameter != null)
                //{
                //    InnerOrderData data = value as InnerOrderData;
                //    PropertyInfo property = typeof(InnerOrderData).GetProperty(parameter.ToString());
                //    checkData = property.GetValue(data)?.ToString();
                //    PropertyInfo securityIDproperty = typeof(InnerOrderData).GetProperty("SecurityID");
                //    securityID = securityIDproperty.GetValue(data)?.ToString();
                //}
                //else
                //{
                //    checkData = value.ToString();
                //}
                if (values[0] != null)
                    checkData = values[0].ToString();
                securityID = values[1].ToString();
            }
            else if (values != null && values.Length == 1)
            {
                if (values[0] != null)
                    checkData = values[0].ToString();
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
