using System.Windows;
using System.Windows.Controls;
using STC.SalesTrading.Plugin.Shared.Services;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>信用筛选面板（原型 filters-credit）。</summary>
    public partial class CreditFilterPanel : UserControl
    {
        public CreditFilterPanel()
        {
            InitializeComponent();
        }

        // 顶部「关注债券 / 筛选分组」区：管理弹窗已按原型实现（分组经 IGroupDirectory 同步后端）
        // 面板 DataContext = MarketCenterViewModel，经其 MarketCenterService 提供识别回填取数
        private void OnFavBondAddClick(object sender, RoutedEventArgs e)
            => FavBondManagerWindow.Show(Window.GetWindow(this), FilterProfileService.MarketKind.Credit,
                (DataContext as ViewModel.MarketCenterViewModel)?.MarketCenterService);

        private void OnFilterGroupAddClick(object sender, RoutedEventArgs e)
            => FilterGroupManagerWindow.Show(Window.GetWindow(this), FilterProfileService.MarketKind.Credit);
    }
}
