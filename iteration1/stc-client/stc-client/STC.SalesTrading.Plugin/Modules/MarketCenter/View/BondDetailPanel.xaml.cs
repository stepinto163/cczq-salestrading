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
            ToastService.Show("基于所选债券所有用户的正式报价，按方向和归属分组；每条报价计一次，30天半衰期。得分=100×(1-exp(-衰减次数/5))。本人机构和交易员可见，其他人证据脱敏；不包含成交，不自动发送报价。");
        }

        private void OnEvidenceClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = button == null ? null : ItemsControl.ContainerFromElement(RecommendationGrid, button) as DataGridRow;
            if (row == null) return;
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            button.Content = row.DetailsVisibility == Visibility.Visible ? "收起" : "展开";
        }

        private async void OnFeedbackClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = button?.DataContext as STC.SalesTrading.Plugin.Abstractions.Models.RecommendationRow;
            var model = DataContext as BondDetailViewModel;
            if (model != null && row != null)
                await model.SubmitRecommendationFeedbackAsync(row, button.Tag as string);
        }
    }
}
