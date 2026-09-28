using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace STC.PT.Plugin.View.Component
{
    public class WatermarkTextBox: TextBox
    {
        static WatermarkTextBox()
        {
            //这句话就是告诉应用程序可以在Themes/Generic文件夹下找到样式，
            //否则，另一个程序集使用则需要添加类似如何的
            //<!--<ResourceDictionary Source="pack://application:,,,/MyWPFControlsLib;component/Themes/Generic.xaml"/>-->引用
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WatermarkTextBox), new FrameworkPropertyMetadata(typeof(WatermarkTextBox)));
        }

        public static readonly DependencyProperty WaterMarkProperty = DependencyProperty.Register("WaterMark", typeof(string), typeof(WatermarkTextBox), new PropertyMetadata("请输入文本"));

        public string WaterMark
        {
            get { return (string)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }
    }
}

