using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.Modules.QuoteMgmt.View
{
    /// <summary>报价管理左侧筛选面板宿主：顶部清除/搜索 + 合并筛选面板（复用 filters-merged）。</summary>
    public partial class MgmtFilterPanel : UserControl
    {
        public MgmtFilterPanel()
        {
            InitializeComponent();
        }

        /// <summary>清除过滤：重置面板内全部筛选标签与输入框（视觉走树，Mock 阶段不驱动数据过滤）。</summary>
        private void OnClearFilters(object sender, RoutedEventArgs e)
        {
            STC.SalesTrading.Plugin.Shared.Services.MarketGroupSelectionService.ClearActive();
            foreach (var toggle in FindChildren<ToggleButton>(this))
            {
                toggle.IsChecked = false;
            }
            foreach (var box in FindChildren<TextBox>(this))
            {
                box.Clear();
            }
        }

        private static System.Collections.Generic.IEnumerable<T> FindChildren<T>(DependencyObject root) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T t) yield return t;
                foreach (var inner in FindChildren<T>(child)) yield return inner;
            }
        }
    }
}
