using STC.SalesTrading.Plugin.Shared.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>报价提醒弹窗窗口（屏幕右下角，AlertPopupService 管理显示/隐藏与定位）。</summary>
    public partial class AlertPopupWindow : Window
    {
        public AlertPopupWindow()
        {
            InitializeComponent();
        }

        /// <summary>「查看」：标记该弹窗 alertKey 已读 + 跳转定位，然后关闭弹窗（原型 viewAlertPopup）。</summary>
        private void OnGroupViewClick(object sender, RoutedEventArgs e)
        {
            var group = (sender as FrameworkElement)?.DataContext as AlertPopupGroup;
            if (group != null) group.View();
        }

        /// <summary>「关闭」：只关闭 UI，不标记已读（原型 closeAlertPopup）。</summary>
        private void OnGroupCloseClick(object sender, RoutedEventArgs e)
        {
            var group = (sender as FrameworkElement)?.DataContext as AlertPopupGroup;
            if (group != null) group.Close();
        }
    }
}
