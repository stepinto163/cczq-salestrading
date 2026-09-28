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

        // 顶部「关注债券 / 筛选分组」区：管理弹窗已按原型实现（数据会话内保存，后端同步待接）。
        // 提醒合并页（filters-merged）在原型中无独立存储字段，暂按信用口径（待确认）
        private void OnFavBondAddClick(object sender, RoutedEventArgs e)
            => FavBondManagerWindow.Show(Window.GetWindow(this), FilterProfileService.MarketKind.Credit,
                (DataContext as ViewModel.MarketCenterViewModel)?.MarketCenterService);

        private void OnFilterGroupAddClick(object sender, RoutedEventArgs e)
            => FilterGroupManagerWindow.Show(Window.GetWindow(this), FilterProfileService.MarketKind.Credit);
    }
}
