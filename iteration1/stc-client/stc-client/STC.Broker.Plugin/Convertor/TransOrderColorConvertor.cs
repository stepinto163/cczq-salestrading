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
  public  class TransOrderColorConvertor : IValueConverter
    {
        /// <summary>
        /// 确认颜色
        /// </summary>
        private Brush BrushOk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#41E390"));
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
        private Thickness paddingOne = new Thickness(5, 4, 0, 5);
        private Thickness paddingTwo = new Thickness(0, 2, 0, 0);
        private FontFamily fontOne = new FontFamily("楷体");
        private FontFamily fontTwo = new FontFamily("宋体");
        private FontFamily fontThree = new FontFamily("Microsoft YaHei Semibold");


        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            if (value == null)
            {
                return BrushOk;
            }
            else
            {
                switch (value.ToString())
                {
                    case "1":
                    case "2":
                    case "I":
                    case "E":
                        return BrushOne;
                    case "F":
                    case "H":
                        return BrushTwo;
                    case "G":
                        if (param == null)
                        {
                            return BrushOk;
                        }
                        else
                        {
                            if (param.ToString().ToUpper() == "TRUE")
                            {
                                return Brushtre;
                            }
                            else
                            {
                                return BrushOk;
                            }
                        }
                    default:
                        return BrushOk;

                }
            }
        }


        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return "";
        }
    }
}
