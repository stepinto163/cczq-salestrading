using System.Windows;
using System.Windows.Controls;
using STC.SalesTrading.Plugin.Shared.Services;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>合并筛选面板（原型 filters-merged，报价提醒页使用）。</summary>
    public partial class AlertFilterPanel : UserControl
    {
        public AlertFilterPanel()
        {
            InitializeComponent();
        }

        // 合并侧栏同时管理信用、利率分组，入口显式选择市场。
        private void OnFavBondAddClick(object sender, RoutedEventArgs e) => ShowMarketMenu((Button)sender, true);
        private void OnFilterGroupAddClick(object sender, RoutedEventArgs e) => ShowMarketMenu((Button)sender, false);
        private void ShowMarketMenu(Button button, bool favorites)
        {
            var menu = new ContextMenu { PlacementTarget = button };
            foreach (var market in new[] { FilterProfileService.MarketKind.Credit, FilterProfileService.MarketKind.Rate })
            {
                var selected = market;
                var item = new MenuItem
                {
                    Header = market == FilterProfileService.MarketKind.Credit ? "信用分组" : "利率分组",
                    Style = TryFindResource("StcMenuItem") as Style
                };
                item.Click += (sender, args) =>
                {
                    if (favorites) FavBondManagerWindow.Show(Window.GetWindow(this), selected, FilterProfileService.LookupService);
                    else FilterGroupManagerWindow.Show(Window.GetWindow(this), selected);
                };
                menu.Items.Add(item);
            }
            menu.IsOpen = true;
        }
    }
}
