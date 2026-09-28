using STC.Common.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Common.View.Component
{
    ///<summary>
    /// 数字输入控件
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        /// <summary>
        /// 逗号分割属性
        /// </summary>
        public static readonly DependencyProperty IsCommaSeparatedValueProperty = DependencyProperty.Register("IsCommaSeparatedValue", typeof(bool), typeof(NumericUpDown));

        /// <summary>
        /// 更改值事件
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericUpDown));

        /// <summary>
        /// 增值事件
        /// </summary>
        public static readonly RoutedEvent ValueIncrementedEvent = EventManager.RegisterRoutedEvent("ValueIncremented", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericUpDown));

        /// <summary>
        /// 减值事件
        /// </summary>
        public static readonly RoutedEvent ValueDecrementedEvent = EventManager.RegisterRoutedEvent("ValueDecremented", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericUpDown));

        /// <summary>
        /// 最大值属性
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(decimal), typeof(NumericUpDown),
            new FrameworkPropertyMetadata(decimal.MaxValue, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnMaxValueChanged)));

        private static void OnMaxValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is NumericUpDown)
            {
                NumericUpDown control = obj as NumericUpDown;
                control.MaxValue = (decimal)e.NewValue;
            }
        }

        /// <summary>
        /// 最小值属性
        /// </summary>
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(decimal), typeof(NumericUpDown),
            new FrameworkPropertyMetadata(decimal.MinValue, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnMinValueChanged)));

        private static void OnMinValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is NumericUpDown)
            {
                NumericUpDown control = obj as NumericUpDown;
                control.MinValue = (decimal)e.NewValue;
            }
        }

        /// <summary>
        /// 小数点位数属性
        /// </summary>
        public static readonly DependencyProperty DecimalLengthProperty = DependencyProperty.Register("DecimalLength", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnDecimalLengthChanged)));

        /// <summary>
        /// 值属性
        /// </summary>
        public static readonly DependencyProperty NumericValueProperty = DependencyProperty.Register("NumericValue", typeof(decimal?), typeof(NumericUpDown));

        /// <summary>
        /// 区间值属性
        /// </summary>
        public static readonly DependencyProperty RangeValueProperty = DependencyProperty.Register("RangeValue", typeof(decimal?), typeof(NumericUpDown), new FrameworkPropertyMetadata(decimal.Zero, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnRangeValueChanged)));

        /// <summary>
        /// 上调区间值属性
        /// </summary>
        public static readonly DependencyProperty RangeValueUpProperty = DependencyProperty.Register("RangeValueUp", typeof(decimal?), typeof(NumericUpDown));

        /// <summary>
        /// 下调区间值属性
        /// </summary>
        public static readonly DependencyProperty RangeValueDownProperty = DependencyProperty.Register("RangeValueDown", typeof(decimal?), typeof(NumericUpDown));

        /// <summary>
        /// 鼠标滚轮可用属性
        /// </summary>
        public static readonly DependencyProperty CanMouseWheelProperty = DependencyProperty.Register("CanMouseWheel", typeof(bool), typeof(NumericUpDown));

        /// <summary>
        /// 零补充属性
        /// </summary>
        public static readonly DependencyProperty DotThreeZeroProperty = DependencyProperty.Register("DotThreeZero", typeof(bool), typeof(NumericUpDown));

        /// <summary>
        /// 聚焦全选属性
        /// </summary>
        public static readonly DependencyProperty IsGotFocusSelectAllProperty = DependencyProperty.Register("IsGotFocusSelectAll", typeof(bool), typeof(NumericUpDown));

        /// <summary>
        /// 文本背景颜色属性
        /// </summary>
        public static readonly DependencyProperty TextBackgroundProperty = DependencyProperty.Register("TextBackground", typeof(Brush), typeof(NumericUpDown), new FrameworkPropertyMetadata(BackgroundProperty.DefaultMetadata.DefaultValue));

        /// <summary>
        /// 文本前景色属性
        /// </summary>
        public static readonly DependencyProperty TextForegroundProperty = DependencyProperty.Register("TextForeground", typeof(Brush), typeof(NumericUpDown), new FrameworkPropertyMetadata(ForegroundProperty.DefaultMetadata.DefaultValue));

        /// <summary>
        /// 小数位数自动调整属性
        /// </summary>
        public static readonly DependencyProperty AdjustDecimalLengthProperty = DependencyProperty.Register("AdjustDecimalLength", typeof(bool), typeof(NumericUpDown));

        /// <summary>
        /// TextContentAlignment
        /// </summary>
        public static readonly DependencyProperty TextContentAlignmentProperty = DependencyProperty.Register("TextContentAlignment", typeof(TextAlignment), typeof(NumericUpDown));

        /// <summary>
        /// UpDownBackground属性
        /// </summary>
        public static readonly DependencyProperty UpDownBackgroundProperty = DependencyProperty.Register("UpDownBackground", typeof(Brush), typeof(NumericUpDown),new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x51,0x51,0x51))));

        /// <summary>
        /// BorderBrush
        /// </summary>
        public static readonly DependencyProperty UpDownBorderBrushProperty = DependencyProperty.Register("UpDownBorderBrush", typeof(Brush), typeof(NumericUpDown), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x51, 0x51, 0x51))));

        /// <summary>
        /// UpMargin
        /// </summary>
        public static readonly DependencyProperty UpMarginProperty = DependencyProperty.Register("UpMargin", typeof(Thickness), typeof(NumericUpDown), new FrameworkPropertyMetadata(new Thickness(0, 2, 2, 0)));

        /// <summary>
        /// DownMargin
        /// </summary>
        public static readonly DependencyProperty DownMarginProperty = DependencyProperty.Register("DownMargin", typeof(Thickness), typeof(NumericUpDown), new FrameworkPropertyMetadata(new Thickness(0, 1, 2, 2)));

        /// <summary>
        /// BorderThickness
        /// </summary>
        public static readonly DependencyProperty UpDownBorderThicknessProperty = DependencyProperty.Register("UpDownBorderThickness", typeof(Thickness), typeof(NumericUpDown), new FrameworkPropertyMetadata(new Thickness(1)));

        /// <summary>
        /// BorderBrush 136C5E
        /// </summary>
        public static readonly DependencyProperty NumBorderBrushProperty = DependencyProperty.Register("NumBorderBrush", typeof(Brush), typeof(NumericUpDown), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x13, 0x6C, 0x5E))));

        public bool AllowEmpty
        {
            get { return (bool)GetValue(AllowEmptyProperty); }
            set { SetValue(AllowEmptyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowEmpty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowEmptyProperty =
            DependencyProperty.Register("AllowEmpty", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(false));

        /// <summary>
        /// TextAlignment属性
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(NumericUpDown), new FrameworkPropertyMetadata(TextAlignment.Center));


        /// <summary>
        /// Gets or sets Text Foreground.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)this.GetValue(TextAlignmentProperty); }
            set { this.SetValue(TextAlignmentProperty, value); }
        }

        /// <summary>
        /// OriginalFormatProperty.
        /// </summary>
        public static readonly DependencyProperty OriginalFormatProperty = DependencyProperty.Register("OriginalFormat", typeof(string), typeof(NumericUpDown));

        /// <summary>
        /// Gets or sets 原始格式
        /// </summary>
        public string OriginalFormat
        {
            get { return (string)this.GetValue(OriginalFormatProperty); }
            set { this.SetValue(OriginalFormatProperty, value); }
        }

        public decimal? MultipleValue
        {
            get { return (decimal?)GetValue(MultipleValueProperty); }
            set { SetValue(MultipleValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MultipleValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MultipleValueProperty =
            DependencyProperty.Register("MultipleValue", typeof(decimal?), typeof(NumericUpDown), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnMultipleValueChanged)));

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericUpDown"/> class.
        /// </summary>
        public NumericUpDown()
        {
            this.InitializeComponent();

            //this.MaxValue = decimal.MaxValue;
            //this.MinValue = decimal.MinValue;
            //this.MaxValue = 999999999999;
            //this.MinValue = decimal.Zero;
            
            
            this.CanMouseWheel = true;
            this.RangeValueUp = decimal.Zero;
            this.RangeValueDown = decimal.Zero;

            this.NumericTextBox.ValueChanged += this.NumericTextBoxOnValueChanged;

            this.IsGotFocusSelectAll = true;
        }

        /// <summary>
        /// Add or remove value changed event.
        /// </summary>
        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }

        /// <summary>
        /// Add or remove value incremented event.
        /// </summary>
        public event RoutedEventHandler ValueIncremented
        {
            add { this.AddHandler(ValueIncrementedEvent, value); }
            remove { this.RemoveHandler(ValueIncrementedEvent, value); }
        }

        /// <summary>
        /// Add or remove value decremented event.
        /// </summary>
        public event RoutedEventHandler ValueDecremented
        {
            add { this.AddHandler(ValueDecrementedEvent, value); }
            remove { this.RemoveHandler(ValueDecrementedEvent, value); }
        }

        /// <summary>
        /// Gets or sets max value.
        /// </summary>
        public decimal MaxValue
        {
            get { return (decimal)this.GetValue(MaxValueProperty); }
            set
            {
                this.SetValue(MaxValueProperty, value);
                this.SetMaxValueEnabled();
            }
        }

        /// <summary>
        /// Gets or sets min value.
        /// </summary>
        public decimal MinValue
        {
            get { return (decimal)this.GetValue(MinValueProperty); }
            set {
                this.SetValue(MinValueProperty, value);
                this.SetMinValueEnabled();
            }
        }

        /// Gets or sets numeric value.
        /// </summary>
        public decimal? NumericValue
        {
            get
            {
                return (decimal?)this.GetValue(NumericValueProperty);
            }

            set
            {
                this.SetValue(NumericValueProperty, value);
                this.RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
            }
        }

        /// <summary>
        /// Gets or sets decimal length.
        /// </summary>
        public int DecimalLength
        {
            get { return (int)this.GetValue(DecimalLengthProperty); }
            set { this.SetValue(DecimalLengthProperty, value); }
        }

        /// <summary>
        /// Gets or sets range value.
        /// </summary>
        public decimal RangeValue
        {
            get { return (decimal)this.GetValue(RangeValueProperty); }
            set { this.SetValue(RangeValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets range value up.
        /// </summary>
        public decimal RangeValueUp
        {
            get { return (decimal)this.GetValue(RangeValueUpProperty); }
            set { this.SetValue(RangeValueUpProperty, value); }
        }

        /// <summary>
        /// Gets or sets range value down.
        /// </summary>
        public decimal RangeValueDown
        {
            get { return (decimal)this.GetValue(RangeValueDownProperty); }
            set { this.SetValue(RangeValueDownProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use comma to seperate the number.
        /// </summary>
        public bool IsCommaSeparatedValue
        {
            get { return (bool)this.GetValue(IsCommaSeparatedValueProperty); }
            set { this.SetValue(IsCommaSeparatedValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether allow mouse wheel.
        /// </summary>
        public bool CanMouseWheel
        {
            get { return (bool)this.GetValue(CanMouseWheelProperty); }
            set { this.SetValue(CanMouseWheelProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether add three zero.
        /// </summary>
        public bool DotThreeZero
        {
            get { return (bool)this.GetValue(DotThreeZeroProperty); }
            set { this.SetValue(DotThreeZeroProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether got focus select all.
        /// </summary>
        public bool IsGotFocusSelectAll
        {
            get { return (bool)this.GetValue(IsGotFocusSelectAllProperty); }
            set { this.SetValue(IsGotFocusSelectAllProperty, value); }
        }

        /// <summary>
        /// Gets or sets Text Background.
        /// </summary>
        public Brush TextBackground
        {
            get { return (Brush)this.GetValue(TextBackgroundProperty); }
            set { this.SetValue(TextBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets Text Foreground.
        /// </summary>
        public Brush TextForeground
        {
            get { return (Brush)this.GetValue(TextForegroundProperty); }
            set { this.SetValue(TextForegroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether adjust decimal length.
        /// </summary>
        public bool AdjustDecimalLength
        {
            get { return (bool)this.GetValue(AdjustDecimalLengthProperty); }
            set { this.SetValue(AdjustDecimalLengthProperty, value); }
        }

        /// <summary>
        /// 文字对齐方向，默认左对齐
        /// </summary>
        public TextAlignment TextContentAlignment
        {
            get { return (TextAlignment)this.GetValue(TextContentAlignmentProperty); }
            set 
            {
                this.SetValue(TextContentAlignmentProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets UpDownBackground value.
        /// </summary>
        public Brush UpDownBackground
        {
            get { return (Brush)this.GetValue(UpDownBackgroundProperty); }
            set { this.SetValue(UpDownBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets UpDownBorderBrush value.
        /// </summary>
        public Brush UpDownBorderBrush
        {
            get { return (Brush)this.GetValue(UpDownBorderBrushProperty); }
            set { this.SetValue(UpDownBorderBrushProperty, value); }
        }

        /// <summary>
        /// Border Brush
        /// </summary>
        public Brush NumBorderBrush
        {
            get { return (Brush)this.GetValue(NumBorderBrushProperty); }
            set { this.SetValue(NumBorderBrushProperty, value); }
        }
        /// <summary>
        /// Gets or sets a value UpDown Button Space Default:0,1,2,2 => 0,0,1,1
        /// </summary>
        public Thickness DownMargin
        {
            get { return (Thickness)this.GetValue(DownMarginProperty); }
            set { this.SetValue(DownMarginProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value UpDown Button Space Default: 0,2,2,0 => 0,1,1,0
        /// </summary>
        public Thickness UpMargin
        {
            get { return (Thickness)this.GetValue(UpMarginProperty); }
            set { this.SetValue(UpMarginProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value UpDown Button Space Default: 0,2,2,0 => 0,1,1,0
        /// </summary>
        public Thickness UpDownBorderThickness
        {
            get { return (Thickness)this.GetValue(UpDownBorderThicknessProperty); }
            set { this.SetValue(UpDownBorderThicknessProperty, value); }
        }

        public bool IsDecreaseToZero
        {
            get { return (bool)GetValue(IsDecreaseToZeroProperty); }
            set { SetValue(IsDecreaseToZeroProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDecreaseToZero.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDecreaseToZeroProperty =
            DependencyProperty.Register("IsDecreaseToZero", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(false));

        /// <summary>
        /// 设定焦点
        /// </summary>
        /// <returns>true:焦点设置成功 false:焦点设置失败</returns>
        public new bool Focus()
        {
            return this.NumericTextBox.Focus();
        }

        /// <summary>
        /// 上调值
        /// </summary>
        public void DoIncrement()
        {
            if (!this.NumericTextBox.NumericValue.HasValue)
            {
                if (MinValue <= 0 && MaxValue >= 0)
                {
                    this.NumericTextBox.NumericValue = decimal.Zero;

                    if (this.NumericTextBox.NumericValue != MaxValue)
                    {
                        if (this.RangeValueUp != decimal.Zero)
                        {
                            this.NumericTextBox.NumericValue += ParseDecimal(this.RangeValueUp);
                        }
                        else
                        {
                            this.NumericTextBox.NumericValue += ParseDecimal(this.RangeValue);
                        }
                    }
                }
                else
                {
                    this.NumericTextBox.NumericValue = this.MinValue;
                }
            }
            else
            {
                if (this.NumericTextBox.NumericValue != MaxValue)
                {
                    if (this.RangeValueUp != decimal.Zero)
                    {
                        this.NumericTextBox.NumericValue += ParseDecimal(this.RangeValueUp);
                    }
                    else
                    {
                        this.NumericTextBox.NumericValue += ParseDecimal(this.RangeValue);
                    }
                }
            }

            if (this.NumericTextBox.NumericValue > this.MaxValue)
            {
                this.NumericTextBox.NumericValue = this.MaxValue;
            }
            

            this.NumericValue = this.NumericTextBox.NumericValue;
            this.NumericTextBox.Focus();
            this.SelectFirst();
            this.RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueIncrementedEvent });
        }


        decimal remainder = 0;
        /// <summary>
        /// 下调值
        /// </summary>
        public void DoDecrement()
        {
            if (!this.NumericTextBox.NumericValue.HasValue)
            {
                if (MinValue <= 0 && MaxValue >= 0)
                {
                    this.NumericTextBox.NumericValue = decimal.Zero;
                }
                else
                {
                    this.NumericTextBox.NumericValue = MinValue;
                }
            }

            if (this.NumericTextBox.NumericValue != MinValue)
            {

                decimal oldValue = this.NumericTextBox.NumericValue.Value;

                if (remainder > 0 && this.NumericTextBox.NumericValue == 0 && IsDecreaseToZero)
                {
                    this.NumericTextBox.NumericValue -= remainder;
                    remainder = 0;
                }
                else
                {
                    if (this.RangeValueDown != decimal.Zero)
                    {
                        this.NumericTextBox.NumericValue -= ParseDecimal(this.RangeValueDown);
                    }
                    else
                    {
                        this.NumericTextBox.NumericValue -= ParseDecimal(this.RangeValue);
                    }

                    if (this.NumericTextBox.NumericValue < this.MinValue)
                    {
                        this.NumericTextBox.NumericValue = this.MinValue;
                    }

                    if (IsDecreaseToZero)
                    {
                        decimal newValue = this.NumericTextBox.NumericValue.Value;
                        if (oldValue > 0 && newValue < 0)
                        {
                            remainder = 0 - newValue;

                            this.NumericTextBox.NumericValue = 0;
                        }

                    }
                }
            }

            this.NumericValue = this.NumericTextBox.NumericValue;

            this.NumericTextBox.Focus();
            this.SelectFirst();
            this.RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueDecrementedEvent });
        }


        /// <summary>
        /// 内容全选
        /// </summary>
        public void SelectAll()
        {
            this.NumericTextBox.SelectAll();
        }

        /// <summary>
        /// 因为不能加0.50所以要调整位数（把后面的0去掉）。
        /// </summary>
        /// <param name="value">调整前的值</param>
        /// <returns>调整后的值</returns>
        private static decimal ParseDecimal(decimal value)
        {
            string strVal = "0";
            if (value.ToString().IndexOf('.') > 0)
            {
                // 小数部分对应
                strVal = value.ToString().TrimEnd('0');
                strVal = strVal.TrimEnd('.');
            }
            else
            {
                // 整数时无特别对应
                strVal = value.ToString();
            }

            if (string.IsNullOrEmpty(strVal))
            {
                return 0;
            }
            else
            {
                return decimal.Parse(strVal);
            }
        }

        private static void OnDecimalLengthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is NumericUpDown)
            {
                NumericUpDown control = obj as NumericUpDown;
                control.DecimalLength = (int)e.NewValue;
            }
        }

        private static void OnRangeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is NumericUpDown)
            {
                NumericUpDown control = obj as NumericUpDown;
                control.RangeValue = (decimal)e.NewValue;
            }
        }

        private static void OnMultipleValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is NumericUpDown)
            {
                NumericUpDown control = obj as NumericUpDown;
                control.MultipleValue = (decimal)e.NewValue;
            }
        }

        private void NumericTextBoxOnValueChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            this.NumericValue = this.NumericTextBox.NumericValue;

            this.SetMaxValueEnabled();
            this.SetMinValueEnabled();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.NumericTextBox.Padding = new Thickness(
                this.NumericTextBox.Padding.Left,
                this.NumericTextBox.Padding.Top,
                this._grid.ColumnDefinitions[1].ActualWidth,
                this.NumericTextBox.Padding.Bottom);
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            this.DoIncrement();
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            this.DoDecrement();
        }

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!this.CanMouseWheel)
            {
                return;
            }

            if (!this.NumericTextBox.IsFocused)
            {
                return;
            }

            if (e.Delta > 0)
            {
                this.DoIncrement();
            }
            else if (e.Delta < 0)
            {
                this.DoDecrement();
            }

            e.Handled = true;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    this.DoIncrement();
                    break;
                case Key.Down:
                    this.DoDecrement();
                    break;
                case Key.Decimal:
                    this.AddThreeZero();
                    break;
            }
        }

        private void SelectFirst()
        {
            this.NumericTextBox.Select(0, 0);
        }

        private void AddThreeZero()
        {
            decimal tmpVal;
            if (decimal.TryParse(this.NumericTextBox.NumericValue.ToString(), out tmpVal) && this.DotThreeZero)
            {
                if (tmpVal == 0)
                {
                    return;
                }

                var tmpStr = this.NumericTextBox.NumericValue.ToString() + "000";
                var over = tmpStr.Length - this.MaxValue.ToString().Length;
                for (int i = 0; i < over; i++)
                {
                    tmpStr = tmpStr.Remove(tmpStr.Length - 1, 1);
                }

                this.NumericValue = Convert.ToDecimal(tmpStr);
            }
        }

        /// <summary>
        /// 达到上限后不能按按钮。
        /// </summary>
        private void SetMaxValueEnabled()
        {
            if (this.NumericValue >= this.MaxValue)
            {
                this.UpButton.IsEnabled = false;
            }
            else
            {
                this.UpButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 达到下限后不能按按钮。
        /// </summary>
        private void SetMinValueEnabled()
        {
            if (this.NumericValue <= this.MinValue)
            {
                this.DownButton.IsEnabled = false;
            }
            else
            {
                this.DownButton.IsEnabled = true;
            }
        }

        public void EnterEdit()
        {
            this.NumericTextBox.Focus();
            this.NumericTextBox.SelectAll();
        }

        #region 全局搜索
        private bool isFromEnter = false;
        private void NumericTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AppUtils.IsInnerInput = false;
        }

        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppUtils.IsInnerInput = true;
            if (isFromEnter)
            {
                AppUtils.IsInnerInput = false;
            }
        }
        private void NumericTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            isFromEnter = false;
            AppUtils.IsInnerInput = true;
            if (e.Key == Key.Enter)
            {
                this.NumericTextBox.AdjustValue();
                isFromEnter = true;
            }
        }

        #endregion

        private void NumericTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.NumericTextBox.IsFocused)
            {
                this.EnterEdit();
                e.Handled = true;
            }
        }
    }
}
