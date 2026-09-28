using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 全局 Toast 服务（对应原型 toast-container，右侧滑入，3.2s 自动消失）。
    /// 任意线程可调用 Show，内部自动调度到 UI 线程。
    /// </summary>
    public static class ToastService
    {
        /// <summary>显示时长（毫秒），原型 toast 约 3s。</summary>
        private const int DurationMs = 3200;

        /// <summary>当前消息队列（ToastHost 直接绑定该集合）。</summary>
        public static ObservableCollection<ToastMessage> Messages { get; } = new ObservableCollection<ToastMessage>();

        /// <summary>弹出一条 Toast。</summary>
        public static void Show(string text, ToastKind kind = ToastKind.Info)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null) return;

            if (!dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => Show(text, kind)));
                return;
            }

            var message = new ToastMessage { Text = text, Kind = kind };
            Messages.Insert(0, message);
            message.RemoveTimer = new Timer(OnExpired, message, DurationMs, Timeout.Infinite);
        }

        private static void OnExpired(object state)
        {
            var message = (ToastMessage)state;
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null) return;

            dispatcher.BeginInvoke(new Action(() =>
            {
                message.RemoveTimer?.Dispose();
                Messages.Remove(message);
            }));
        }
    }
}
