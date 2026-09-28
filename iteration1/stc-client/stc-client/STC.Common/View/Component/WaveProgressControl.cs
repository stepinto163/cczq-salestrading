using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Common.View.Component
{
    public class WaveProgressControl : UserControl
    {
        public WaveProgressControl()
        {
            //InitializeComponent();
            //this.DataContext = this;

            //这句话就是告诉应用程序可以在Themes/Generic文件夹下找到样式，
            //否则，另一个程序集使用则需要添加类似如何的
            //<!--<ResourceDictionary Source="pack://application:,,,/MyWPFControlsLib;component/Themes/Generic.xaml"/>-->引用
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WaveProgressControl), new FrameworkPropertyMetadata(typeof(WaveProgressControl)));
        }

        private static SolidColorBrush defaultWaveProgressBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
        public static readonly DependencyProperty WaveProgressBackgroundProperty = DependencyProperty.Register(
            "WaveProgressBackground", typeof(SolidColorBrush), typeof(WaveProgressControl), new PropertyMetadata(defaultWaveProgressBackground));

        /// <summary>
        /// 进度条背景色
        /// </summary>
        public SolidColorBrush WaveProgressBackground
        {
            get { return (SolidColorBrush)GetValue(WaveProgressBackgroundProperty); }
            set { SetValue(WaveProgressBackgroundProperty, value); }
        }

        private static SolidColorBrush defaultWaveBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
        public static readonly DependencyProperty WaveBorderBrushProperty = DependencyProperty.Register(
            "WaveBorderBrush", typeof(SolidColorBrush), typeof(WaveProgressControl), new PropertyMetadata(defaultWaveBorderBrush));
        /// <summary>
        /// 边框颜色
        /// </summary>
        public SolidColorBrush WaveBorderBrush
        {
            get { return (SolidColorBrush)GetValue(WaveBorderBrushProperty); }
            set { SetValue(WaveBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty WaveBorderThicknessProperty = DependencyProperty.Register(
            "WaveBorderThickness", typeof(double), typeof(WaveProgressControl), new PropertyMetadata(2.0));

        /// <summary>
        /// 边框粗细
        /// </summary>
        public double WaveBorderThickness
        {
            get { return (double)GetValue(WaveBorderThicknessProperty); }
            set { SetValue(WaveBorderThicknessProperty, value); }
        }

        private static SolidColorBrush defaultWavePorgressBarColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282828"));
        public static readonly DependencyProperty WavePorgressBarColorProperty = DependencyProperty.Register(
            "WavePorgressBarColor", typeof(SolidColorBrush), typeof(WaveProgressControl), new PropertyMetadata(defaultWavePorgressBarColor));
        /// <summary>
        /// 进度条颜色
        /// </summary>
        public SolidColorBrush WavePorgressBarColor
        {
            get { return (SolidColorBrush)GetValue(WavePorgressBarColorProperty); }
            set { SetValue(WavePorgressBarColorProperty, value); }
        }

        private static SolidColorBrush defaultTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(
            "TextColor", typeof(SolidColorBrush), typeof(WaveProgressControl), new PropertyMetadata(defaultTextColor));
        /// <summary>
        /// 文字颜色
        /// </summary>
        public SolidColorBrush TextColor
        {
            get { return (SolidColorBrush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double), typeof(WaveProgressControl), new PropertyMetadata(default(double)));

        /// <summary>
        /// 当前进度
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WaveProgressControl));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            "MaxValue", typeof(double), typeof(WaveProgressControl), new PropertyMetadata(default(double)));

        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty DisPlayValueProperty = DependencyProperty.Register(
            "DisPlayValue", typeof(string), typeof(WaveProgressControl), new PropertyMetadata("0"));

        public string DisPlayValue
        {
            get { return (string)GetValue(DisPlayValueProperty); }
            set { SetValue(DisPlayValueProperty, value); }
        }

        public static readonly DependencyProperty WaveProgressHeightProperty = DependencyProperty.Register(
            "WaveProgressHeight", typeof(double), typeof(WaveProgressControl), new PropertyMetadata(default(double)));

        /// <summary>
        /// 次属性不要手动设置
        /// </summary>
        public double WaveProgressHeight
        {
            get { return (double)GetValue(WaveProgressHeightProperty); }
            set { SetValue(WaveProgressHeightProperty, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ValueProperty)
            {
                RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
                if (Value >= 0 && MaxValue > 0)
                {
                    double bl = Value / MaxValue;
                    WaveProgressHeight = 140 * bl;
                    DisPlayValue = (bl * 100).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    WaveProgressHeight = 0;
                    DisPlayValue = "0";
                }
            }

        }

    }
}


