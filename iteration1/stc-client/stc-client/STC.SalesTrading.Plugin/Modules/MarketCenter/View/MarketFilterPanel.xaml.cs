using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using STC.SalesTrading.Plugin.Modules.MarketCenter.Model;
using STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>行情中心左侧筛选面板宿主：顶部清除/搜索 + 按子页切换的四套筛选面板。</summary>
    public partial class MarketFilterPanel : UserControl
    {
        public MarketFilterPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 清除过滤的视觉重置（原型 resetBondFilterPanel 的 tags/input/select 部分）：
        /// 只重置当前子页面板——取消全部选中 tag、清空全部输入框、下拉回到首项（「不限」/占位），
        /// 意向页同时收起互斥提示横幅。分组激活集与看板重查由 ClearFiltersCommand（VM）负责，
        /// 面板隐藏的其它子页不受影响（对齐原型每个面板各自的清除范围）。
        /// </summary>
        private void OnClearFilters(object sender, RoutedEventArgs e)
        {
            var scope = ResolveActivePanel();
            if (scope == null) return;
            if (scope != IntentHost)
            {
                STC.SalesTrading.Plugin.Shared.Components.SidebarFilter.Reset((FrameworkElement)scope);
                return;
            }

            foreach (var toggle in FindChildren<ToggleButton>(scope))
            {
                toggle.IsChecked = false;
            }
            foreach (var box in FindChildren<TextBox>(scope))
            {
                box.Clear();
            }
            foreach (var combo in FindChildren<ComboBox>(scope))
            {
                combo.SelectedIndex = combo.Items.Count > 0 ? 0 : -1;
            }

            if (scope == IntentHost)
            {
                // 互斥横幅：tag 全部取消后提示失效，直接收起（下次选中互斥 tag 时 EvaluateMutex 会重新评估）
                foreach (var child in FindChildren<FrameworkElement>(scope))
                {
                    if (child.Name == "MutexBanner")
                    {
                        child.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>当前子页对应的筛选面板（四套常驻，按 VM.FilterPanel 选择）。</summary>
        private UIElement ResolveActivePanel()
        {
            var vm = DataContext as MarketCenterViewModel;
            if (vm == null) return null;
            switch (vm.FilterPanel)
            {
                case MarketFilterPanelType.Credit: return CreditHost;
                case MarketFilterPanelType.Rate: return RateHost;
                case MarketFilterPanelType.Intent: return IntentHost;
                case MarketFilterPanelType.Alert: return AlertHost;
                default: return null;
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
