using STC.SalesTrading.Plugin.Shared.Components;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 右下角提醒弹窗服务（对应原型 alert-popup：**按提醒类型分组**，头部件显示「类型 (新增数)」与「查看/关闭」，
    /// 正文最多 3 条，由用户查看或关闭）。
    /// 独立无边框窗口跟随所属客户端右下角，按 DPI 换算并限制在屏幕工作区内；
    /// 队列空时自动隐藏；任意线程可 Show，内部调度到 UI 线程。
    /// </summary>
    public static class AlertPopupService
    {

        /// <summary>当前弹窗分组队列（AlertPopupWindow 绑定）。</summary>
        public static ObservableCollection<AlertPopupGroup> Groups { get; } = new ObservableCollection<AlertPopupGroup>();

        private static Window _window;
        private static Window _anchor;

        private static Window Window
        {
            get
            {
                if (_window == null)
                {
                    _window = new AlertPopupWindow();
                    // 尺寸定型（首次 Show 后）与内容增减长高时，都重新贴工作区右下角，避免压住任务栏
                    _window.Loaded += (s, e) => Reposition();
                    _window.SizeChanged += (s, e) => Reposition();
                }
                return _window;
            }
        }

        /// <summary>弹出一条提醒：并入同类型弹窗（不存在则新建一个，新弹窗置顶）。</summary>
        public static void Show(AlertPopupItem item)
        {
            if (item == null) return;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null) return;

            if (!dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => Show(item)));
                return;
            }

            var group = Groups.FirstOrDefault(g => !g.IsClosed && g.TypeLabel == item.TypeLabel);
            if (group == null)
            {
                group = new AlertPopupGroup(item.TypeLabel, item.AlertType);
                group.PropertyChanged += OnGroupChanged;
                Groups.Insert(0, group);
            }

            group.Append(item);
            UpdateWindow();
        }

        private static void OnGroupChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AlertPopupGroup.IsClosed)) return;
            var group = (AlertPopupGroup)sender;
            group.PropertyChanged -= OnGroupChanged;
            Groups.Remove(group);
            UpdateWindow();
        }

        private static void UpdateWindow()
        {
            var w = Window;
            if (Groups.Count > 0)
            {
                AttachAnchor();
                if (!w.IsVisible) w.Show();
                Reposition();
            }
            else if (w.IsVisible)
            {
                w.Hide();
            }
        }

        // 主窗口移动、缩放或换屏时，提醒跟随所属客户端，而不是落到其他账号后方。
        private static void AttachAnchor()
        {
            var main = Application.Current?.MainWindow;
            if (main == _window || main == _anchor) return;
            if (_anchor != null)
            {
                _anchor.LocationChanged -= OnAnchorChanged;
                _anchor.SizeChanged -= OnAnchorSizeChanged;
            }
            _anchor = main;
            if (_anchor != null)
            {
                _window.Owner = _anchor;
                _anchor.LocationChanged += OnAnchorChanged;
                _anchor.SizeChanged += OnAnchorSizeChanged;
            }
        }

        private static void OnAnchorChanged(object sender, EventArgs args) => Reposition();
        private static void OnAnchorSizeChanged(object sender, SizeChangedEventArgs args) => Reposition();

        /// <summary>设备像素先转为 WPF DIP，再限制在所属客户端与屏幕工作区内。</summary>
        private static void Reposition()
        {
            var popup = _window;
            if (popup == null || popup.ActualWidth <= 0 || popup.ActualHeight <= 0) return;
            var source = PresentationSource.FromVisual(popup);
            if (source?.CompositionTarget == null) return;
            var main = _anchor ?? Application.Current?.MainWindow;
            var handle = main == null ? IntPtr.Zero : new WindowInteropHelper(main).Handle;
            var pixels = System.Windows.Forms.Screen.FromHandle(handle).WorkingArea;
            var transform = source.CompositionTarget.TransformFromDevice;
            var area = new Rect(transform.Transform(new Point(pixels.Left, pixels.Top)),
                transform.Transform(new Point(pixels.Right, pixels.Bottom)));
            // 常态窗口停靠自身右下角；最大化窗口停靠工作区，不覆盖任务栏。
            if (main != null && main.IsVisible && main.WindowState == WindowState.Normal)
            {
                var clientArea = new Rect(main.Left, main.Top, main.ActualWidth, main.ActualHeight);
                clientArea.Intersect(area);
                if (!clientArea.IsEmpty) area = clientArea;
            }
            popup.Left = Math.Max(area.Left, area.Right - popup.ActualWidth);
            popup.Top = Math.Max(area.Top, area.Bottom - popup.ActualHeight);
        }
    }
}
