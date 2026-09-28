using System;
using System.Collections.Generic;
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

namespace STC.Mmshe.Plugin.View.Component
{
    /// <summary>
    /// HoverButton.xaml 的交互逻辑
    /// </summary>
    public partial class HoverButton : Button
    {
        private Brush _defaultBackground;
        private Brush _defaultForegroud;
        private Brush _defaultBorderBrush;
        private static Brush _defaultOutBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C000000"));

        /// <summary>
        /// button的圆弧度
        /// </summary>
        public string CornerRadius
        {
            get { return (string)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(string), typeof(HoverButton), new PropertyMetadata("1"));



        /// <summary>
        /// 按压背景颜色
        /// </summary>
        public Brush PressBackground
        {
            get { return (Brush)GetValue(PressBackgroundProperty); }
            set { SetValue(PressBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PressBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressBackgroundProperty =
            DependencyProperty.Register("PressBackground", typeof(Brush), typeof(HoverButton));


        /// <summary>
        /// 悬浮背景颜色
        /// </summary>
        public Brush HoverBackground
        {
            get { return (Brush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HoverBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(HoverButton));

        /// <summary>
        /// 按压前景颜色
        /// </summary>
        public Brush PressForeground
        {
            get { return (Brush)GetValue(PressForegroundProperty); }
            set { SetValue(PressForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PressForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressForegroundProperty =
            DependencyProperty.Register("PressForeground", typeof(Brush), typeof(HoverButton));


        /// <summary>
        /// 悬浮前景颜色
        /// </summary>
        public Brush HoverForeground
        {
            get { return (Brush)GetValue(HoverForegroundProperty); }
            set { SetValue(HoverForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HoverForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverForegroundProperty =
            DependencyProperty.Register("HoverForeground", typeof(Brush), typeof(HoverButton));

        /// <summary>
        /// 按压边框颜色
        /// </summary>
        public Brush PressBorderBrush
        {
            get { return (Brush)GetValue(PressBorderBrushProperty); }
            set { SetValue(PressBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PressBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressBorderBrushProperty =
            DependencyProperty.Register("PressBorderBrush", typeof(Brush), typeof(HoverButton));


        /// <summary>
        /// 悬浮边框颜色
        /// </summary>
        public Brush HoverBorderBrush
        {
            get { return (Brush)GetValue(HoverBorderBrushProperty); }
            set { SetValue(HoverBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HoverBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverBorderBrushProperty =
            DependencyProperty.Register("HoverBorderBrush", typeof(Brush), typeof(HoverButton));



        /// <summary>
        /// 外边框颜色
        /// </summary>
        public Brush OutBorderBrush
        {
            get { return (Brush)GetValue(OutBorderBrushProperty); }
            set { SetValue(OutBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OutBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OutBorderBrushProperty =
            DependencyProperty.Register("OutBorderBrush", typeof(Brush), typeof(HoverButton), new PropertyMetadata(_defaultOutBorderBrush));



        public HoverButton()
        {
            InitializeComponent();
            
        }

        //protected override void OnMouseMove(MouseEventArgs e)
        //{
        //    base.OnMouseMove(e);
        //    if (this.HoverBackground != null) this.Background = this.HoverBackground;
        //    if (this.HoverForeground != null) this.Foreground = this.HoverForeground;
        //    if (this.HoverBorderBrush != null) this.BorderBrush = this.HoverBorderBrush;
        //}

        //protected override void OnMouseEnter(MouseEventArgs e)
        //{
        //    _defaultBackground = this.Background;
        //    _defaultForegroud = this.Foreground;
        //    _defaultBorderBrush = this.BorderBrush;
        //    base.OnMouseEnter(e);
        //    if (this.HoverBackground != null) this.Background = this.HoverBackground;
        //    if (this.HoverForeground != null) this.Foreground = this.HoverForeground;
        //    if (this.HoverBorderBrush != null) this.BorderBrush = this.HoverBorderBrush;

        //}

        //protected override void OnMouseLeave(MouseEventArgs e)
        //{
        //    //base.OnMouseLeave(e);
        //    this.Background = _defaultBackground;
        //    this.Foreground = _defaultForegroud;
        //    this.BorderBrush = _defaultBorderBrush;
        //}

        //protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        //{
        //    base.OnIsPressedChanged(e);
        //    if ((bool)e.NewValue == true)
        //    {
        //        if (this.PressBackground != null) this.Background = this.PressBackground;
        //        if (this.PressForeground != null) this.Foreground = this.PressForeground;
        //        if (this.PressBorderBrush != null) this.BorderBrush = this.PressBorderBrush;
        //    }
        //    else
        //    {
        //        if (IsMouseOver)
        //        {
        //            if (this.HoverBackground != null) this.Background = this.HoverBackground;
        //            if (this.HoverForeground != null) this.Foreground = this.HoverForeground;
        //            if (this.HoverBorderBrush != null) this.BorderBrush = this.HoverBorderBrush;
        //        }
        //        else
        //        {
        //            this.Background = _defaultBackground;
        //            this.Foreground = _defaultForegroud;
        //            this.BorderBrush = _defaultBorderBrush;
        //        }    
        //    }
        //}
    }
}
