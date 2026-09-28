using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>筛选区间输入行（标签 + 最小/最大 + 单位），原型 filter-grid-item。</summary>
    public partial class FilterRangeRow : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(FilterRangeRow), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
            nameof(Unit), typeof(string), typeof(FilterRangeRow), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MinTextProperty = DependencyProperty.Register(
            nameof(MinText), typeof(string), typeof(FilterRangeRow),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MaxTextProperty = DependencyProperty.Register(
            nameof(MaxText), typeof(string), typeof(FilterRangeRow),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>是否渲染「最大」输入框。原型 Vol.Bid / Vol.Ofr 只有「最小」一个输入框。</summary>
        public static readonly DependencyProperty ShowMaxProperty = DependencyProperty.Register(
            nameof(ShowMax), typeof(bool), typeof(FilterRangeRow), new PropertyMetadata(true));

        /// <summary>整行置灰且不可编辑（原型 .filter-page-unavailable，如 Bid-Ofr 当前页不可用）。</summary>
        public static readonly DependencyProperty IsDisabledProperty = DependencyProperty.Register(
            nameof(IsDisabled), typeof(bool), typeof(FilterRangeRow), new PropertyMetadata(false));

        public FilterRangeRow()
        {
            InitializeComponent();
        }

        public string Label { get { return (string)GetValue(LabelProperty); } set { SetValue(LabelProperty, value); } }
        public string Unit { get { return (string)GetValue(UnitProperty); } set { SetValue(UnitProperty, value); } }
        public string MinText { get { return (string)GetValue(MinTextProperty); } set { SetValue(MinTextProperty, value); } }
        public string MaxText { get { return (string)GetValue(MaxTextProperty); } set { SetValue(MaxTextProperty, value); } }
        public bool ShowMax { get { return (bool)GetValue(ShowMaxProperty); } set { SetValue(ShowMaxProperty, value); } }
        public bool IsDisabled { get { return (bool)GetValue(IsDisabledProperty); } set { SetValue(IsDisabledProperty, value); } }
    }
}
