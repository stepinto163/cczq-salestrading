using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;

namespace STC.SalesTrading.Plugin.Shared.ViewModel
{
    /// <summary>Toast 级别。</summary>
    public enum ToastKind
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>单条 Toast 消息（由 ToastService 管理，ToastHost 渲染）。</summary>
    public class ToastMessage : ObservableObject
    {
        /// <summary>文案。</summary>
        public string Text { get; set; }

        /// <summary>级别。</summary>
        public ToastKind Kind { get; set; }

        /// <summary>自动移除定时器（持有引用防止被 GC 提前回收）。</summary>
        internal Timer RemoveTimer { get; set; }
    }
}
