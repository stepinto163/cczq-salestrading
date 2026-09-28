using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Chart.Plugin.Converts
{
    public class ForeGorundConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var ForeGround = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2786E4"));
            if (value == null) return ForeGround;
            if (value.ToString() == "TRD")
            {
                ForeGround = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00A74F"));
            }
            else if (value.ToString() == "GVN")
            {
                ForeGround = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D00 "));
            }
            else
            {
                ForeGround = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009AFF"));
            }
            return ForeGround;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BackGorundConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush color = new SolidColorBrush();


            switch ((int)value % 2)
            {
                case 0:
                    color.Color = (Color)ColorConverter.ConvertFromString("#172422 ");
                    break;
                case 1:
                    color.Color = (Color)ColorConverter.ConvertFromString("#121A19");
                    break;
                default:
                    break;
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConfirmWindowForeGorundConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush color = new SolidColorBrush();


            if (value.ToString() == "GVN")   //卖出
            {
                color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E66B00"));
            }
            else if (value.ToString() == "TKN")   //买入
            {
                color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0182EB "));
            }
            else
            {
                color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#454545 "));
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class BrokerBackGroundConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush color = new SolidColorBrush();

            switch (value.ToString())
            {
                case "平安":
                    color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F76920"));
                    break;
                case "国利":
                    color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D6F9B"));
                    break;
                case "中诚":
                    color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#708D20"));
                    break;
                case "信唐":
                    color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8C5AA5"));
                    break;
                case "国际":
                    color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BD5555"));
                    break;
                default:
                    break;
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class BrokerBackGorundConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush color = new SolidColorBrush();


            switch ((int)value % 2)
            {
                case 0:
                    color.Color = (Color)ColorConverter.ConvertFromString("#172422 ");
                    break;
                case 1:
                    color.Color = (Color)ColorConverter.ConvertFromString("#121A19");
                    break;
                default:
                    break;
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    

    /// <summary>
    /// 卡片展示高度赋值转换器
    /// </summary>
    public class HeightDiffConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            var num1 = double.Parse(value.ToString());
            var num2 = double.Parse(parameter.ToString());

            return num1 - num2 - 4 < 40 ? 40 : num1 - num2 - 4;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 高度宽度转换器
    /// </summary>
    public class HeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return 0;
            try
            {
                return double.Parse(Math.Floor(double.Parse(values[0].ToString()) / double.Parse(values[1].ToString())).ToString());
            }
            catch
            {
                return 0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BPForeGorundConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush color = new SolidColorBrush();

            if (value != null && value.ToString() != "--")
            {
                var sss = double.Parse(value.ToString());
                if (sss > 0)
                {
                    color.Color = (Color)ColorConverter.ConvertFromString("#21DF1F ");  //绿色
                }
                else if (sss < 0)
                {
                    color.Color = (Color)ColorConverter.ConvertFromString("#DF201F");  //红色
                }
                else
                {
                    color.Color = (Color)ColorConverter.ConvertFromString("#FFEBC8 ");  //米白色
                }
            }
            else
            {
                color.Color = (Color)ColorConverter.ConvertFromString("#FFEBC8 ");  //米白色
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class BPForeGorundConvert2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush color = new SolidColorBrush();

            if (value != null && value.ToString() != "--")
            {
                var sss = double.Parse(value.ToString());
                if (sss > 0)
                {
                    color.Color = (Color)ColorConverter.ConvertFromString("#21DF1F ");  //绿色
                }
                else if (sss < 0)
                {
                    color.Color = (Color)ColorConverter.ConvertFromString("#DF201F");  //红色
                }
                else
                {
                    color.Color = (Color)ColorConverter.ConvertFromString("#FFEBC8 ");  //米白色
                }
            }
            else
            {
                color.Color = (Color)ColorConverter.ConvertFromString("#FFEBC8 ");  //米白色
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

     public class String2NoValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return "--";
                var buyOrSell = value.ToString();
                if (string.IsNullOrWhiteSpace(buyOrSell))
                {
                    return "--";
                }
                return buyOrSell;
            }
            catch
            {
                return "--";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "--";
            if (value.ToString().ToLower() == "bid" || value.ToString().ToLower() == "ofr")
            {
                return value;
            }
            decimal result;
            if (decimal.TryParse(value.ToString(), out result))
            {
                int count = 0;
                if (value.ToString().IndexOf(".") > -1)
                {
                    count = value.ToString().Length - 1 - value.ToString().IndexOf("."); //小数位数
                }

                if (count == 0)
                {
                    return value.ToString() + ".00";
                }
                if (count == 1)
                {
                    return value.ToString() + "0";
                }
                if (count > 4)
                {
                    return Math.Round((double)result, 4);
                }
                return result.ToString();
            }
            return "--";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class String2NoValue2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return "已到期";
                var buyOrSell = value.ToString();
                if (string.IsNullOrWhiteSpace(buyOrSell))
                {
                    return "已到期";
                }
                return buyOrSell;
            }
            catch
            {
                return "已到期";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 空转不可见
    /// </summary>
    public class Null2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Visibility result = Visibility.Visible;
                if (value == null)
                {
                    result = Visibility.Hidden;
                }
                else
                {
                    var buyOrSell = value.ToString();
                    if (string.IsNullOrWhiteSpace(buyOrSell))
                    {
                        result = Visibility.Hidden;
                    }
                    else
                    {
                        result = Visibility.Visible;
                    }
                }
                if (parameter != null && parameter.ToString() == "1") //结果去反向
                {
                    result = result == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                }
                return result;
            }
            catch
            {
                return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// 远期小红点是否显示
    /// </summary>
    public class BDot2VisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Visibility.Hidden;
            Visibility visibilityDot = (Visibility)values[0];
            bool isForward = bool.Parse(values[1].ToString());
            if (isForward) return Visibility.Hidden;
            return visibilityDot;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
