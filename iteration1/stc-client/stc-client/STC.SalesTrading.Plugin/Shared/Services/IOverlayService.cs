using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.Shell;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 浮窗/对话框展示接缝：VM 层只依赖此接口（可单测、可换 UI 形态），
    /// 具体弹窗实现在 View 层（<see cref="STC.SalesTrading.Plugin.Shell.View.OverlayService"/>），由组合根注入。
    /// </summary>
    public interface IOverlayService
    {
        /// <summary>打开报价录入浮窗（独立新建的报价录入实例）。</summary>
        void ShowQuoteEntryOverlay(QuoteViewModel viewModel);

        /// <summary>打开用户中心弹窗。</summary>
        void ShowUserCenter(UserProfile profile);

        /// <summary>打开设置弹窗。</summary>
        void ShowSettings();
    }
}
