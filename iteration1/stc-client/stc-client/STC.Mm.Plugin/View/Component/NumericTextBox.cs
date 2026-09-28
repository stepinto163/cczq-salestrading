using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using TextBox = System.Windows.Controls.TextBox;

namespace STC.Mm.Plugin.View.Component
{
    /// <summary>
    /// 数字文本框
    /// </summary>
    public class NumericTextBox : TextBox
    {
        /// <summary>
        /// ValueChangedEvent.
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericTextBox));

        /// <summary>
        /// MaxValueProperty.
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
                                                                    "MaxValue",
                                                                    typeof(decimal),
                                                                    typeof(NumericTextBox),
                                                                    new FrameworkPropertyMetadata(decimal.MaxValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMaxValueChanged));

        /// <summary>
        /// MinValueProperty.
        /// </summary>
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
                                                                    "MinValue",
                                                                    typeof(decimal),
                                                                    typeof(NumericTextBox),
                                                                    new FrameworkPropertyMetadata(decimal.MinValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMinValueChanged));

        /// <summary>
        /// DecimalLengthProperty.
        /// </summary>
        public static readonly DependencyProperty DecimalLengthProperty = DependencyProperty.Register(
                                                                            "DecimalLength",
                                                                            typeof(int),
                                                                            typeof(NumericTextBox),
                                                                            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDecimalLengthChanged));

        /// <summary>
        /// OriginalFormatProperty.
        /// </summary>
        public static readonly DependencyProperty OriginalFormatProperty = DependencyProperty.Register("OriginalFormat", typeof(string), typeof(NumericTextBox));

        /// <summary>
        /// NumericValueProperty.
        /// </summary>
        public static readonly DependencyProperty NumericValueProperty = DependencyProperty.Register(
                                                                        "NumericValue",
                                                                        typeof(decimal?),
                                                                        typeof(NumericTextBox),
                                                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        /// <summary>
        /// IsCommaSeparatedValueProperty.
        /// </summary>
        public static readonly DependencyProperty IsCommaSeparatedValueProperty = DependencyProperty.Register(
                                                                        "IsCommaSeparatedValue",
                                                                        typeof(bool),
                                                                        typeof(NumericTextBox),
                                                                        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCommaChanged));

        /// <summary>
        /// IsGotFocusSelectAllProperty.
        /// </summary>
        public static readonly DependencyProperty IsGotFocusSelectAllProperty = DependencyProperty.Register(
                                                                        "IsGotFocusSelectAll",
                                                                        typeof(bool),
                                                                        typeof(NumericTextBox),
                                                                        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectAllChanged));

        /// <summary>
        /// TextBackgroundProperty.
        /// </summary>
        public static readonly DependencyProperty TextBackgroundProperty = DependencyProperty.Register(
                                                                        "TextBackground",
                                                                        typeof(Brush),
                                                                        typeof(NumericTextBox),
                                                                        new FrameworkPropertyMetadata(BackgroundProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextBackgroundChanged));

        /// <summary>
        /// TextForegroundProperty.
        /// </summary>
        public static readonly DependencyProperty TextForegroundProperty = DependencyProperty.Register(
                                                                        "TextForeground",
                                                                        typeof(Brush),
                                                                        typeof(NumericTextBox),
                                                                        new FrameworkPropertyMetadata(ForegroundProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextForegroundChanged));

        /// <summary>
        /// AdjustDecimalLengthProperty.
        /// </summary>
        public static readonly DependencyProperty AdjustDecimalLengthProperty = DependencyProperty.Register(
                                                                        "AdjustDecimalLength",
                                                                        typeof(bool),
                                                                        typeof(NumericTextBox),
                                                                        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAdjustDecimalLengthChanged));

        /// <summary>
        /// InputNumberTypeProperty.
        /// </summary>
        public static readonly DependencyProperty InputNumberTypeProperty = DependencyProperty.Register(
                                                                        "InputNumberType",
                                                                        typeof(InputNumberTypeEnum),
                                                                        typeof(NumericTextBox),
                                                                        new FrameworkPropertyMetadata(InputNumberTypeEnum.DecimalWithLength, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnInputNumberTypeChanged));


        /// <summary>
        /// TextHorizontalAlignmentProperty.
        /// </summary>
        public static readonly DependencyProperty TextContentAlignmentProperty = DependencyProperty.Register(
                                                                        "TextContentAlignment",
                                                                        typeof(TextAlignment),
                                                                        typeof(NumericTextBox),
                                                                        new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TextContentAlignmentChanged));

        private static NumericTextBoxValidationRule validationRuleDecimal = InitializeValidationRule();

        private string preText;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericTextBox"/> class.
        /// </summary>
        public NumericTextBox()
        {
            // 禁用粘贴的键盘快捷键
            this.InputBindings.Add(new KeyBinding(ApplicationCommands.NotACommand, Key.V, ModifierKeys.Control));
            this.InputBindings.Add(new KeyBinding(ApplicationCommands.NotACommand, Key.Insert, ModifierKeys.Shift));

            this.TextAlignment = TextContentAlignment;
            this.VerticalContentAlignment = VerticalAlignment.Center;

            this.TextChanged += this.Change;

            this.ContextMenu = null;
        }

        /// <summary>
        /// Gets or sets TextContentAlignment
        /// </summary>
        public TextAlignment TextContentAlignment
        {
            get { return (TextAlignment)this.GetValue(TextContentAlignmentProperty); }
            set 
            { 
                this.SetValue(TextContentAlignmentProperty, value);
                this.TextAlignment = (TextAlignment)value;
            }
        }

        /// <summary>
        /// TextContentAlignmentValueChanged
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void TextContentAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                control.TextAlignment = (TextAlignment)e.NewValue;
            }
        }

        /// ValueChanged
        /// </summary>
        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }

        /// <summary>
        /// Gets or sets 输入数值类型
        /// </summary>
        public InputNumberTypeEnum InputNumberType
        {
            get { return (InputNumberTypeEnum)this.GetValue(InputNumberTypeProperty); }
            set { this.SetValue(InputNumberTypeProperty, value); }
        }

        /// <summary>
        /// Gets or sets 值
        /// </summary>
        public decimal? NumericValue
        {
            get
            {
                return (decimal?)this.GetValue(NumericValueProperty);
            }

            set
            {
                if (value != null && this.NumericValue != value)
                {
                    this.SetValue(NumericValueProperty, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets 小数部分长度
        /// </summary>
        public int DecimalLength
        {
            get
            {
                return (int)this.GetValue(DecimalLengthProperty);
            }

            set
            {
                if (this.NumericValue != null)
                {
                    this.SetValue(DecimalLengthProperty, value);
                    this.OnFormatChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets 原始格式
        /// </summary>
        public string OriginalFormat
        {
            get { return (string)this.GetValue(OriginalFormatProperty); }
            set { this.SetValue(OriginalFormatProperty, value); }
        }

        /// <summary>
        /// Gets or sets 最大值
        /// </summary>
        public decimal MaxValue
        {
            get { return (decimal)this.GetValue(MaxValueProperty); }
            set { this.SetValue(MaxValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets 最小值
        /// </summary>
        public decimal MinValue
        {
            get { return (decimal)this.GetValue(MinValueProperty); }
            set { this.SetValue(MinValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets 是否要逗号分隔
        /// </summary>
        public bool IsCommaSeparatedValue
        {
            get { return (bool)this.GetValue(IsCommaSeparatedValueProperty); }
            set { this.SetValue(IsCommaSeparatedValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets 聚焦时是否全选
        /// </summary>
        public bool IsGotFocusSelectAll
        {
            get { return (bool)this.GetValue(IsGotFocusSelectAllProperty); }
            set { this.SetValue(IsGotFocusSelectAllProperty, value); }
        }

        /// <summary>
        /// Gets or sets 背景色
        /// </summary>
        public Brush TextBackground
        {
            get { return (Brush)this.GetValue(TextBackgroundProperty); }
            set { this.SetValue(TextBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets 前景色
        /// </summary>
        public Brush TextForeground
        {
            get { return (Brush)this.GetValue(TextForegroundProperty); }
            set { this.SetValue(TextForegroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets 小数部分长度能否调整
        /// </summary>
        public bool AdjustDecimalLength
        {
            get { return (bool)this.GetValue(AdjustDecimalLengthProperty); }
            set { this.SetValue(AdjustDecimalLengthProperty, value); }
        }

        /// <inheritdoc/>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            System.Windows.Data.Binding textBinding = BindingOperations.GetBinding(this, TextBox.TextProperty);

            if (textBinding != null)
            {
                textBinding.ValidationRules.Add(validationRuleDecimal);
            }
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (this.NumericValue.HasValue)
            {
                this.OnFormatChanged();
            }

            base.OnGotFocus(e);

            if (this.IsGotFocusSelectAll)
            {
                using (Timer timer = new Timer())
                {
                    timer.Interval = 100;
                    timer.Enabled = true;
                    timer.Tick += (sender, args) =>
                    {
                        this.SelectAll();
                        timer.Dispose();
                    };
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (this.NumericValue.HasValue)
            {
                this.TextChanged -= this.Change;
                if (!this.AdjustDecimalLength)
                {
                    this.Text = this.NumericValue.Value.ToString(GetFormat(this));
                }
                else
                {
                    this.Text = this.NumericValue.Value.ToString();
                }

                this.TextChanged += this.Change;
            }

            base.OnLostFocus(e);
        }

        private static NumericTextBoxValidationRule InitializeValidationRule()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericTextBox), new FrameworkPropertyMetadata(typeof(NumericTextBox)));
            InputMethod.IsInputMethodEnabledProperty.OverrideMetadata(typeof(NumericTextBox), new FrameworkPropertyMetadata(false));

            // Validation的执行时机
            validationRuleDecimal = new NumericTextBoxValidationRule
            {
                ValidationStep = ValidationStep.ConvertedProposedValue,
            };
            return validationRuleDecimal;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericTextBox)d;

            if (e.NewValue == null)
            {
                control.Text = null;
            }
            else
            {
                var newValue = (decimal)e.NewValue;

                var length = control.SelectionLength;
                var start = control.SelectionStart;

                string format = string.Empty;

                if (!control.IsFocused)
                {
                    format = GetFormat(control);
                }

                var newText = newValue.ToString(format);
                if (!newText.Equals(control.Text, StringComparison.CurrentCulture))
                {
                    if (!IsAdjustDecimalLength(control))
                    {
                        control.Text = newValue.ToString(format);
                    }
                    else
                    {
                        control.Text = newValue.ToString();
                    }

                    control.SelectionLength = length;
                    control.SelectionStart = start;
                }
            }

            control.RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
        }

        private static void OnCommaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                if (control.NumericValue.HasValue && !control.IsFocused)
                {
                    control.Text = control.NumericValue.Value.ToString(GetFormat(control));
                }
            }
        }

        private static void OnIsSelectAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                control.IsGotFocusSelectAll = (bool)e.NewValue;
            }
        }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                control.MaxValue = (decimal)e.NewValue;
            }
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                control.MinValue = (decimal)e.NewValue;
            }
        }

        private static void OnInputNumberTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                control.InputNumberType = (InputNumberTypeEnum)e.NewValue;
            }

            switch (control.InputNumberType)
            {
                case InputNumberTypeEnum.ShortType:
                    control.MaxValue = short.MaxValue;
                    control.MinValue = short.MinValue;
                    break;
                case InputNumberTypeEnum.IntType:
                    control.MaxValue = int.MaxValue;
                    control.MinValue = int.MinValue;
                    break;
                case InputNumberTypeEnum.LongType:
                    control.MaxValue = long.MaxValue;
                    control.MinValue = long.MinValue;
                    break;
                default:
                    break;
            }
        }

        private static void OnDecimalLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                control.DecimalLength = (int)e.NewValue;
            }
        }

        private static void OnTextBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                control.TextBackground = (Brush)e.NewValue;
                control.Background = (Brush)e.NewValue;
            }
        }

        private static void OnTextForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                control.TextForeground = (Brush)e.NewValue;
                control.Foreground = (Brush)e.NewValue;
            }
        }

        private static void OnAdjustDecimalLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericTextBox;
            if (control != null)
            {
                control.AdjustDecimalLength = (bool)e.NewValue;
                control.Text = control.Text;
            }
        }

        private static string GetFormat(NumericTextBox textBox)
        {
            if (!string.IsNullOrEmpty(textBox.OriginalFormat))
            {
                return textBox.OriginalFormat;
            }

            if (textBox.InputNumberType == InputNumberTypeEnum.DecimalType)
            {
                return textBox.OriginalFormat;
            }

            int decimalLength = textBox.DecimalLength;

            if (textBox.IsCommaSeparatedValue)
            {
                return string.Format("N{0}", decimalLength);
            }

            return string.Format("#0.".PadRight("#0.".Length + decimalLength, '0'));
        }

        private static bool IsAdjustDecimalLength(NumericTextBox textBox)
        {
            return textBox.AdjustDecimalLength;
        }

        private void Change(object sender, TextChangedEventArgs e)
        {
            var txtValue = this.Text;

            if (string.IsNullOrEmpty(txtValue) ||
                (string.Equals("-", txtValue, StringComparison.CurrentCulture) && this.MinValue < 0))
            {
                this.NumericValue = null;
                this.preText = this.Text;
                return;
            }

            decimal value;
            if (decimal.TryParse(this.Text, out value))
            {
                var decimals = this.Text.Split('.');

                if ((this.InputNumberType != InputNumberTypeEnum.DecimalType && decimals.Length >= 2 && (this.DecimalLength == 0 || this.DecimalLength < decimals[1].Length)) ||
                    txtValue.IndexOf('+') >= 0)
                {
                    this.Text = this.preText;
                    this.SelectionStart = this.Text.Length;
                    return;
                }

                if (this.MinValue <= value && this.MaxValue >= value)
                {
                    this.NumericValue = value;
                    int intPreLenght = (this.preText != null) ? this.preText.Length : 0;
                    this.preText = this.Text;

                    foreach (var change in e.Changes)
                    {
                        // 格式化增加部分（不考虑删除部分）
                        int intDiffCnt = (this.Text.Length - intPreLenght) + change.RemovedLength;

                        int intOffset = change.Offset + intDiffCnt;

                        if (intOffset > 0)
                        {
                            this.SelectionStart = intOffset;
                        }
                    }

                    return;
                }
            }

            this.Text = this.preText;
            this.SelectionStart = this.Text.Length;
        }

        private void OnFormatChanged()
        {
            this.TextChanged -= this.Change;
            if (!this.AdjustDecimalLength)
            {
                this.Text = this.NumericValue.Value.ToString(GetFormat(this));
            }
            else
            {
                this.Text = this.NumericValue.Value.ToString();
            }

            this.TextChanged += this.Change;
        }

        private class NumericTextBoxValidationRule : ValidationRule
        {
            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                decimal validdecimal;
                ValidationResult result;

                string strTarget = value.ToString();

                if (decimal.TryParse(strTarget, out validdecimal))
                {
                    result = new ValidationResult(true, string.Empty);
                }
                else
                {
                    result = new ValidationResult(false, string.Empty);
                }

                return result;
            }
        }
    }

    /// <summary>
    /// 输入数字类型枚举
    /// </summary>
    public enum InputNumberTypeEnum
    {
        /// <summary>浮点数字类型</summary>
        DecimalType,

        /// <summary>整数型（2字节）</summary>
        ShortType,

        /// <summary>整数型（4字节）</summary>
        IntType,

        /// <summary>整数型（8字节）</summary>
        LongType,

        /// <summary>浮点数字类型（指定小数点位数）</summary>
        DecimalWithLength,
    }
}
