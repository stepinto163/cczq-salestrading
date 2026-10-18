using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SalesTrading.UiTests
{
    // 只管理自动化进程：保留系统缩放，不修改用户的桌面 DPI 或正式客户端配置。
    internal static class DesktopPresentation
    {
        internal const double FooterHeight = 172;
        private const double Gap = 12;

        internal static bool IsAdvanced => Environment.GetEnvironmentVariable("STC_UI_LAYOUT_ADVANCED") == "1";
        internal static bool IsObserver => Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROLE") == "observer";
        internal static string AccountLabel => (IsObserver ? "观察账号" : "操作账号") + " · "
            + Environment.GetEnvironmentVariable("STC_UI_USER");

        internal static Rect WorkArea(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).EnsureHandle();
            long anchor;
            IntPtr monitorAnchor = long.TryParse(Environment.GetEnvironmentVariable("STC_UI_LAYOUT_ANCHOR"), out anchor)
                ? new IntPtr(anchor) : handle;
            var information = new MonitorInformation { Size = Marshal.SizeOf(typeof(MonitorInformation)) };
            if (!GetMonitorInfo(MonitorFromWindow(monitorAnchor, 2), ref information))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
            // Win32 坐标为设备像素；WPF 布局必须使用当前窗口的 DIP 变换。
            Matrix fromDevice = HwndSource.FromHwnd(handle).CompositionTarget.TransformFromDevice;
            Point start = fromDevice.Transform(new Point(information.Work.Left, information.Work.Top));
            Point end = fromDevice.Transform(new Point(information.Work.Right, information.Work.Bottom));
            return new Rect(start, end);
        }

        internal static Rect ClientBounds(Rect workArea, bool advanced, bool observer)
        {
            double availableWidth = Math.Max(1, workArea.Width - Gap * 2);
            double availableHeight = Math.Max(1, workArea.Height - FooterHeight - Gap * 2);
            if (advanced && availableWidth >= 2212)
            {
                double width = (availableWidth - Gap) / 2;
                return new Rect(workArea.Left + Gap + (observer ? width + Gap : 0), workArea.Top + Gap,
                    width, availableHeight);
            }
            // 屏幕不足以并排时错开标题栏与左边缘；当前动作窗口前置，另一账号仍可辨认。
            double offset = advanced ? 36 : 0;
            return new Rect(workArea.Left + Gap + (observer ? offset : 0),
                workArea.Top + Gap + (observer ? offset : 0),
                Math.Max(1, availableWidth - offset), Math.Max(1, availableHeight - offset));
        }

        internal static void Arrange(Window shell)
        {
            shell.WindowState = WindowState.Normal;
            shell.WindowStartupLocation = WindowStartupLocation.Manual;
            Rect bounds = ClientBounds(WorkArea(shell), IsAdvanced, IsObserver);
            // 测试窗口可缩至可用工作区，内容保持正常字号与虚拟化，由原页面提供滚动。
            shell.MinWidth = Math.Min(shell.MinWidth, bounds.Width);
            shell.MinHeight = Math.Min(shell.MinHeight, bounds.Height);
            shell.Left = bounds.Left;
            shell.Top = bounds.Top;
            shell.Width = bounds.Width;
            shell.Height = bounds.Height;
            shell.Title = "STC 自动化 · " + AccountLabel;
            shell.UpdateLayout();
        }

        internal static void PresentTarget(Window shell, FrameworkElement target)
        {
            Window targetWindow = target == null ? shell : Window.GetWindow(target);
            if (targetWindow == null) return;
            if (targetWindow.WindowState == WindowState.Minimized) targetWindow.WindowState = WindowState.Normal;
            // 激活父客户端，再呈现目标弹窗，避免观察方始终被操作方盖住。
            if (shell != null)
            {
                RaiseWithoutMoving(shell);
                shell.Activate();
            }
            if (targetWindow != shell)
            {
                RaiseWithoutMoving(targetWindow);
                targetWindow.Activate();
            }
        }

        private static void RaiseWithoutMoving(Window window)
        {
            // 两个进程交替执行时 Activate 可能不改变非活动窗口的 Z 序。
            // 显式前置但不抢键盘焦点，不改变窗口位置、大小及置顶属性。
            IntPtr handle = new WindowInteropHelper(window).Handle;
            if (!SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 0x0013))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        internal static Rect CaptionBounds(Rect workArea)
        {
            // 右侧保留播放控制位置；底栏不遮住客户端内的表格与提醒弹窗。
            return new Rect(workArea.Left + Gap, workArea.Bottom - FooterHeight + Gap,
                Math.Max(200, workArea.Width - 480), FooterHeight - Gap * 2);
        }

        internal static string VerifyOnScreen(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            NativeRectangle bounds;
            if (!GetWindowRect(handle, out bounds)) throw new InvalidOperationException("无法获取真实窗口边界");
            var monitor = new MonitorInformation { Size = Marshal.SizeOf(typeof(MonitorInformation)) };
            GetMonitorInfo(MonitorFromWindow(handle, 2), ref monitor);
            if (bounds.Left < monitor.Work.Left || bounds.Top < monitor.Work.Top
                || bounds.Right > monitor.Work.Right || bounds.Bottom > monitor.Work.Bottom)
            {
                throw new InvalidOperationException("窗口超出显示器工作区：" + window.Title);
            }
            return string.Format(CultureInfo.InvariantCulture, "screenPixels=({0},{1})-({2},{3}); workArea=({4},{5})-({6},{7})",
                bounds.Left, bounds.Top, bounds.Right, bounds.Bottom,
                monitor.Work.Left, monitor.Work.Top, monitor.Work.Right, monitor.Work.Bottom);
        }

        internal static void VerifyUncovered(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            NativeRectangle bounds;
            GetWindowRect(handle, out bounds);
            foreach (double horizontal in new[] { 0.2, 0.5, 0.8 })
            foreach (double vertical in new[] { 0.2, 0.5, 0.8 })
            {
                var point = new NativePoint
                {
                    X = bounds.Left + (int)((bounds.Right - bounds.Left) * horizontal),
                    Y = bounds.Top + (int)((bounds.Bottom - bounds.Top) * vertical)
                };
                if (GetAncestor(WindowFromPoint(point), 2) != handle)
                {
                    throw new InvalidOperationException("提醒窗口被其他窗口遮挡，不能报告为可见");
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInformation
        {
            public int Size;
            public NativeRectangle Monitor;
            public NativeRectangle Work;
            public uint Flags;
        }
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
        [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInformation information);
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out NativeRectangle rectangle);
        [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(NativePoint point);
        [DllImport("user32.dll")] private static extern IntPtr GetAncestor(IntPtr window, uint flags);
    }
}
