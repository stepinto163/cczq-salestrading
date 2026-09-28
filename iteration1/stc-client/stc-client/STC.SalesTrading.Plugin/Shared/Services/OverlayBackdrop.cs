using System.Windows;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 模态浮层窗口的遮罩辅助：把浮层窗口铺满宿主窗口并向外扩 <see cref="Bleed"/> 像素，
    /// 用于复现原型的整窗压暗遮罩：
    /// `.modal-overlay{background:rgba(0,0,0,.6)}`（编辑报价弹窗）、
    /// `.quote-entry-overlay{background:rgba(0,0,0,.68)}`（报价录入浮窗）。
    ///
    /// 说明：WPF 的独立 Window 无法给宿主窗口叠加半透明层，因此改为让浮层窗口覆盖宿主，
    /// 在浮层内部用一个半透明黑 Border 做遮罩；外扩 8px 用于掩盖最大化窗口的不可见缩放边框，
    /// 避免四周露出未被压暗的宿主体。
    /// </summary>
    internal static class OverlayBackdrop
    {
        /// <summary>覆盖宿主时的外扩量（DIP）。</summary>
        private const double Bleed = 8d;

        /// <summary>把 <paramref name="overlay"/> 的位置与尺寸对齐到宿主窗口（含外扩）。</summary>
        public static void CoverOwner(Window overlay)
        {
            if (overlay == null)
            {
                return;
            }

            var owner = overlay.Owner ?? Application.Current?.MainWindow;
            if (owner == null || owner.ActualWidth <= 0d || owner.ActualHeight <= 0d)
            {
                return;
            }

            overlay.Left = owner.Left - Bleed;
            overlay.Top = owner.Top - Bleed;
            overlay.Width = owner.ActualWidth + Bleed * 2d;
            overlay.Height = owner.ActualHeight + Bleed * 2d;
        }

        /// <summary>宿主客户区尺寸（DIP）；宿主不可用时回退到主屏幕工作区。</summary>
        public static Size OwnerSize(Window overlay)
        {
            var owner = overlay?.Owner ?? Application.Current?.MainWindow;
            if (owner != null && owner.ActualWidth > 0d && owner.ActualHeight > 0d)
            {
                return new Size(owner.ActualWidth, owner.ActualHeight);
            }

            var area = SystemParameters.WorkArea;
            return new Size(area.Width, area.Height);
        }
    }
}
