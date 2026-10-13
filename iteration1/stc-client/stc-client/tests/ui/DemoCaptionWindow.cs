using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace SalesTrading.UiTests
{
    // 自动化进程专用字幕：不激活窗口、不接收鼠标，避免影响输入和点击目标。
    internal sealed class DemoCaptionWindow : Window
    {
        private readonly TextBlock heading;
        private readonly TextBlock operation;
        private readonly TextBlock next;
        private readonly DispatcherTimer positionTimer;
        private Window anchor;

        public DemoCaptionWindow()
        {
            Title = "自动化演示字幕";
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            ShowActivated = false;
            Focusable = false;
            IsHitTestVisible = false;
            Topmost = true;
            Width = 860;
            SizeToContent = SizeToContent.Height;
            heading = new TextBlock { FontSize = 13, Foreground = Brushes.LightGray };
            operation = new TextBlock
            {
                FontSize = 24, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4)
            };
            next = new TextBlock { FontSize = 14, Foreground = Brushes.LightGray, TextWrapping = TextWrapping.Wrap };
            StackPanel lines = new StackPanel();
            lines.Children.Add(heading);
            lines.Children.Add(operation);
            lines.Children.Add(next);
            Content = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(235, 15, 23, 34)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(73, 169, 231)),
                BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20, 10, 20, 10), Child = lines
            };
            SourceInitialized += (sender, args) =>
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                int style = GetWindowLong(handle, -20);
                SetWindowLong(handle, -20, style | 0x20 | 0x08000000 | 0x80);
            };
            SizeChanged += (sender, args) => Position();
            positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            positionTimer.Tick += (sender, args) => Position();
            Closed += (sender, args) => positionTimer.Stop();
        }

        public string OperationText => operation.Text;
        public string NextText => next.Text;

        public void Present(Window target, string caseName, int index, int count,
            string phase, string action, string upcoming)
        {
            anchor = target;
            heading.Text = caseName + (index > 0 ? "  ·  " + index + " / " + count : "");
            operation.Text = phase + "：" + action;
            next.Text = string.IsNullOrEmpty(upcoming) ? "" : "接下来：" + upcoming;
            next.Visibility = string.IsNullOrEmpty(upcoming) ? Visibility.Collapsed : Visibility.Visible;
            Position();
            if (!IsVisible) Show();
            positionTimer.Start();
        }

        public void Suspend()
        {
            positionTimer.Stop();
            Hide();
        }

        private void Position()
        {
            Rect area = SystemParameters.WorkArea;
            if (anchor != null && anchor.IsVisible && anchor.WindowState == WindowState.Normal)
            {
                area = new Rect(anchor.Left, anchor.Top, anchor.ActualWidth, anchor.ActualHeight);
            }
            Width = Math.Max(240, Math.Min(860, area.Width - 32));
            Left = area.Left + Math.Max(0, (area.Width - Width) / 2);
            Top = Math.Max(area.Top, area.Bottom - Math.Max(ActualHeight, 115) - 24);
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr handle, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr handle, int index, int value);
    }
}
