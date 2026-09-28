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
    /// <summary>
    /// 加桥成交单的显示配置
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class BridgeColorConverter : IValueConverter
    {
        private Brush BrushOne = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBC8"));
        private Brush BrushTwo = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#172422"));
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
            object obj = "Hidden";
            if (value != null)
            {
                string strValue = value.ToString();
                if (strValue.IndexOf('-') > 0)
                {
                    String[] ss = strValue.Split('-');
                    switch (ss[0])
                    {
                        case "IsShow"://字体颜色
                            if (ss.Length > 2 && !ss[1].Equals("") && ss[2].Equals("0"))
                                obj = isShowBrush;
                            else
                                obj = BrushOne;
                            break;
                        case "Isbroker": //背景色
                            obj = BrushTransparent;
                            break;
                        case "Isliq":
                            if (ss.Length > 2 && !ss[1].Equals("") && ss[2].Equals("1"))
                                obj = FontWeights.Bold;
                            else
                                obj = FontWeights.Normal;
                            break;
                        case "Padding":

                            if (ss.Length > 2)
                            {
                                if (ss[1].Equals("") && ss[2].Equals("0"))
                                    obj = paddingThickness;
                                else if (ss[2].Equals("f"))
                                    obj = new Thickness(0, 0, 0, 8);
                                else
                                    obj = paddingThickness;
                            }
                            else
                                obj = paddingThickness;
                            break;
                        case "Family":
                            if (ss.Length > 2 &&  ss[1].Equals(""))
                            {
                                switch (ss[2])
                                {
                                    case "0":
                                        obj = fontOne;
                                        break;
                                    case "2":
                                        obj = fontTwo;
                                        break;
                                    default:
                                        obj = fontThree;
                                        break;
                                }
                            }
                            else
                                obj = fontThree;
                            break;
                        case "Size":
                            if (ss.Length > 2 && ss[1].Equals("") && ss[2].Equals("0"))
                                obj = 18;
                            else
                                obj = 14;
                            break;
                        case "Header":
                            if (ss.Length > 2 && ss[1].Equals("") && ss[2].Equals("0"))
                                obj = "Hidden";
                            else
                                obj = "Visible";
                            break;
                        case "Width":
                            if (ss.Length > 2 && ss[1].Equals("") && ss[2].Equals("0"))
                                obj = 0;
                            else
                                obj = 26;
                            break;
                        case "Margin":
                            //if (ss.Length > 2)
                            //{
                            //    if (ss[1].Equals("") && ss[2].Equals("0"))
                            //        obj = paddingThickness;
                            //    else
                            //        obj = paddingThickness;
                            //} else
                                obj = paddingThickness;
                            break;
                    }
                }
                switch (strValue)
                {
                    case "Visible":
                        obj = 45;
                        break;
                    case "Hidden":
                        obj = 0;
                        break;
                }
            }
            return obj;
        }
        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return "";
        }
    }
}
