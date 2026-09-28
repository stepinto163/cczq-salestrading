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
    [ValueConversion(typeof(string), typeof(string))]
    public class TransBorderStyleConvert : IValueConverter
    {
        /// <summary>
        /// 确认颜色
        /// </summary>
        private Brush BrushOk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F0F0F"));
        /// <summary>
        /// 待确认颜色
        /// </summary>
        private Brush BrushOne = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9D61"));
        /// <summary>
        /// 拒绝颜色
        /// </summary>
        private Brush BrushTwo = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5585A"));

        /// <summary>
        /// 部分成交颜色
        /// </summary>
        private Brush Brushtre = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#874EB9"));

        private Brush BrushTransparent = new SolidColorBrush(Colors.Transparent);
        private Brush isShowBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2040"));
        private Thickness paddingThickness = new Thickness(0, 0, 0, 0);
        private Thickness paddingOne = new Thickness(1);
        private Thickness paddingTwo = new Thickness(0);
        private FontFamily fontOne = new FontFamily("楷体");
        private FontFamily fontTwo = new FontFamily("宋体");
        private FontFamily fontThree = new FontFamily("Microsoft YaHei Semibold");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="typeTarget"></param>
        /// <param name="param">1:Thickness,2:Brush</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            if (param == null)
            {
                if (value == null)
                {
                    return BrushOk;
                }
                else
                {
                    if (value.ToString().ToUpper() == "TRUE")
                    {
                        return BrushOk;
                    }
                    return BrushTransparent;
                }
            }
            else
            {
                if (param.ToString() == "2")
                {
                    if (value == null)
                    {
                        return BrushOk;
                    }
                    else
                    {
                        if (value.ToString().ToUpper() == "TRUE")
                        {
                            return BrushOk;
                        }
                        return BrushTransparent;
                    }
                }
                else {
                    if (value == null)
                    {
                        return paddingOne;
                    }
                    else
                    {
                        if (value.ToString().ToUpper() == "TRUE")
                        {
                            return paddingOne;
                        }
                        return paddingTwo;
                    }
                }
            }
        }


        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return "";
        }
    }
}
