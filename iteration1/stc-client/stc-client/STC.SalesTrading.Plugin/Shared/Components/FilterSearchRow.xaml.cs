using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>筛选面板「标签 + 搜索输入」行（原三面板手写同构行的公共件）。</summary>
    public partial class FilterSearchRow : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(FilterSearchRow), new PropertyMetadata(null));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder), typeof(string), typeof(FilterSearchRow), new PropertyMetadata(null));

        public static readonly DependencyProperty RowMarginProperty = DependencyProperty.Register(
            nameof(RowMargin), typeof(Thickness), typeof(FilterSearchRow),
            new PropertyMetadata(new Thickness(0, 0, 0, 8)));

        public FilterSearchRow()
        {
            InitializeComponent();
        }

        /// <summary>行首标签文案。</summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>输入框占位文案。</summary>
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        /// <summary>行外距（默认 "0,0,0,8"；区块末行传 "0"）。</summary>
        public Thickness RowMargin
        {
            get { return (Thickness)GetValue(RowMarginProperty); }
            set { SetValue(RowMarginProperty, value); }
        }
    }
}
