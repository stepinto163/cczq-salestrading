using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>筛选面板分区标题（分隔线 + 标题）。</summary>
    public partial class FilterSectionHeader : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(FilterSectionHeader), new PropertyMetadata(null));

        public FilterSectionHeader()
        {
            InitializeComponent();
        }

        /// <summary>分区标题文案。</summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
    }
}
