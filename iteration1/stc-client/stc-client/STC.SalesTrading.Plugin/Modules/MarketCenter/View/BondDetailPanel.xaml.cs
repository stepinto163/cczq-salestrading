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
            ToastService.Show("按同券、方向、报价归属人和对手分组，每条已确认报价计一次；按30天半衰期衰减。可展开查看证据，并提交反馈。");
        }

        private void OnEvidenceClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            DependencyObject current = button;
            while (current != null && !(current is DataGridRow))
            {
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            var row = current as DataGridRow;
            if (row == null) return;
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            button.Content = row.DetailsVisibility == Visibility.Visible ? "收起" : "展开";
        }

        private async void OnFeedbackClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var detail = DataContext as BondDetailViewModel;
            if (detail != null)
            {
                await detail.SubmitRecommendationFeedbackAsync(button.DataContext as Abstractions.Models.RecommendationRow, (string)button.Tag);
            }
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            ToastService.Show("发送报价待接 /api/recommendations/quote-send");
        }
    }
}
