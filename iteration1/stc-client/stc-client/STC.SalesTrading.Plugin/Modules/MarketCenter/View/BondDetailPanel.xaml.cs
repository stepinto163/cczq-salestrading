using STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;
using STC.SalesTrading.Plugin.Shared.Services;
using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>右侧债券详情面板（7 区块）。DataContext 为 BondDetailViewModel。</summary>
    public partial class BondDetailPanel : UserControl
    {
        public BondDetailPanel()
        {
            InitializeComponent();
        }

        private void OnExplainClick(object sender, RoutedEventArgs e)
        {
            ToastService.Show("推荐说明待接 /api/recommendations/rules");
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            ToastService.Show("发送报价待接 /api/recommendations/quote-send");
        }
    }
}
