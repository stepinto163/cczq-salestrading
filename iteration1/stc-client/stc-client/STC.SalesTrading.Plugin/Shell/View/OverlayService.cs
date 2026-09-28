using System.Windows;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Shell;
using STC.SalesTrading.Plugin.Shared.Services;

namespace STC.SalesTrading.Plugin.Shell.View
{
    /// <summary>
    /// <see cref="IOverlayService"/> 的 View 层实现：集中持有「VM → 弹窗」的展示逻辑。
    /// VM 层不再直接引用任何 Window 类型；接网关后若弹窗改内嵌，仅替换本类。
    /// </summary>
    public sealed class OverlayService : IOverlayService
    {
        private static Window Owner => Application.Current?.MainWindow;

        public void ShowQuoteEntryOverlay(QuoteViewModel viewModel)
        {
            QuoteEntryOverlayWindow.Show(Owner, viewModel);
        }

        public void ShowUserCenter(UserProfile profile)
        {
            UserCenterWindow.Show(Owner, profile);
        }

        public void ShowSettings()
        {
            SettingsWindow.Show(Owner);
        }
    }
}
