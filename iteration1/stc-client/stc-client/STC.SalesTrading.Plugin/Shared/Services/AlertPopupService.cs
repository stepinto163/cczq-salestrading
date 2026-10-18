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
    /// 独立无边框窗口定位在【主窗口所在屏幕】的工作区右下角，精确停靠任务栏上方、多屏跟随；
    /// 队列空时自动隐藏；任意线程可 Show，内部调度到 UI 线程。
    /// </summary>
    public static class AlertPopupService
    {

        /// <summary>当前弹窗分组队列（AlertPopupWindow 绑定）。</summary>
        public static ObservableCollection<AlertPopupGroup> Groups { get; } = new ObservableCollection<AlertPopupGroup>();

        private static Window _window;

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
                if (!w.IsVisible) w.Show();
            }
            else if (w.IsVisible)
            {
                w.Hide();
            }
        }

        /// <summary>
        /// 定位到【主窗口所在屏幕】的工作区右下角（精确停靠任务栏上方，多屏跟随）。
        /// 对齐 lite 参考实现 QuoteRemindWindow.SetWindowPositionAndSize：
        /// 用 WPF 自身 Left/Top（DIP）直接设置、不做物理像素换算，取 Screen.FromHandle(主窗口) 的 WorkingArea
        /// （已剔除任务栏），配合渲染后真实的 ActualWidth/ActualHeight —— 这样 WPF 不会把位置拉回，
        /// 且高分屏/DPI 虚拟化下也与 WorkingArea 同坐标系，窗口底边 = 任务栏上沿，绝不压住任务栏。
        /// </summary>
        private static void Reposition()
        {
            var w = Window;
            if (w == null || w.ActualWidth <= 0 || w.ActualHeight <= 0) return;

            try
            {
                var wa = GetTargetWorkingArea();
                // 贴死工作区右下角、零间距：窗口底边正好落在任务栏上沿（对齐 lite 参考实现）
                w.Left = wa.Right - w.ActualWidth;
                w.Top = wa.Bottom - w.ActualHeight;
            }
            catch (Exception)
            {
                // 定位失败回退系统默认位置
            }
        }

        /// <summary>取定位目标工作区：优先主窗口所在屏，主窗口不可用时回退鼠标所在屏。</summary>
        private static System.Drawing.Rectangle GetTargetWorkingArea()
        {
            var main = Application.Current?.MainWindow;
            if (main != null)
            {
                try
                {
                    var h = new WindowInteropHelper(main).Handle;
                    if (h != IntPtr.Zero)
                    {
                        return System.Windows.Forms.Screen.FromHandle(h).WorkingArea;
                    }
                }
                catch (Exception)
                {
                    // 主窗口句柄不可用则回退鼠标屏
                }
            }

            return System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position).WorkingArea;
        }
    }
}
