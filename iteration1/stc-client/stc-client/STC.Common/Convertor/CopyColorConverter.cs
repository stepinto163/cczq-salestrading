using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.Windows.Media;

namespace STC.Common.Convertor
{
    /// <summary>
    /// 用于明细的显示配置，根据不同的状态和角色设置明细不同的显示效果
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class CopyColorConverter : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            object obj = "";
            if (value != null)
            {
                string strValue = value.ToString();
                if (strValue.IndexOf('-') > 0)
                {
                    String[] ss = strValue.Split('-');
                    switch (ss[0])
                    {
                        case "IsShow":
                            if (!ss[1].Equals("") && ss[2].Equals("0"))
                                obj = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C5585A"));//字体红色 需要加桥但尚未加桥的成交单
                            else if (ss[1].Equals("") && ss[2].Equals("2"))//头颜色
                                obj = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EAB854"));//字体黄色 header头统一默认颜色
                            else if (ss[1].Equals("") && ss[2].Equals("3"))//汇总颜色
                                obj = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00"));//字体橙色 汇总默认颜色
                            else
                                obj = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D7D7D7"));//字体白色//("#000000"));//字体黑色不需要加桥的成交单，或者需要加桥但已经加完桥的成交单
                            break;
                        case "Isbroker":
                            if (!ss[1].Equals("") && ss[2].Equals("0"))
                                obj = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#452A19"));//背景黄色：该笔成交有修改且未被查看
                            else
                                obj = Brushes.Transparent;
                            break;
                        case "Isliq":
                            if (!ss[1].Equals("") && ss[2].Equals("1"))
                                obj = FontWeights.Bold;
                            if (ss[1].Equals("") && ss[2].Equals("3"))//头加粗
                                obj = FontWeights.Bold;
                            else
                                obj = FontWeights.Normal;
                            break;
                        case "Padding":
                            if (ss[1].Equals("") && ss[2].Equals("0"))
                                obj = new Thickness(0, 0, 0, 0);
                            else
                                obj = new Thickness(0, 0, 0, 0);
                            break;
                        case "Family":
                            if (ss[1].Equals(""))
                                switch (ss[2])
                                {
                                    case "0":
                                        obj = new FontFamily("Microsoft YaHei Semibold");
                                        break;
                                    case "2":
                                        obj = new FontFamily("MicrosoftYaHei");
                                        break;
                                    default:
                                        obj = new FontFamily("Microsoft YaHei Semibold");
                                        break;
                                }
                            break;
                        case "Size":
                            if (ss[1].Equals("") && ss[2].Equals("0"))
                                obj = 14;
                            else
                                obj = 14;
                            break;
                        // Header==0 隐藏
                        case "Header":
                            if (ss[1].Equals("") && ss[2].Equals("0"))
                                obj = "Hidden";
                            else
                                obj = "Visible";
                            break;
                        case "Width":
                            if (ss[1].Equals("") && ss[2].Equals("0"))
                                obj = 0;
                            else
                                obj = 35;
                            break;
                        case "Margin":
                            if (ss[1].Equals("") && ss[2].Equals("0"))
                                obj = new Thickness(0, 0, 0, 0);
                            else
                                obj = new Thickness(0, 0, 1, 0);
                            break;
                    }
                }
                switch (strValue)
                {
                    ///发送按钮宽度 Visible取64 Hidden取0
                    case "Visible":
                        obj = 64;
                        break;
                    case "Hidden":
                        obj = 0;
                        break;
                    ///删除线显示转换
                    case "True":
                        //预定义的静态的装饰线
                        //obj = TextDecorations.Strikethrough;
                        //自定义装饰线  删除线
                        SolidColorBrush red = new SolidColorBrush(Colors.Black) { Opacity = 0.5 };//红色画刷
                        Pen pen = new Pen(red, 1);
                        TextDecoration myStrikethrough = new TextDecoration(TextDecorationLocation.Strikethrough, pen, 0, TextDecorationUnit.Pixel, TextDecorationUnit.Pixel);
                        TextDecorationCollection myCollection = new TextDecorationCollection
                        {
                            myStrikethrough
                        };
                        obj = myCollection;
                        break;
                    case "False":
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