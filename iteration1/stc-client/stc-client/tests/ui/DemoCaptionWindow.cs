using System;
using System.Runtime.InteropServices;
using System.IO;
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
                FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4)
            };
            StackPanel lines = new StackPanel();
            lines.Children.Add(heading);
            lines.Children.Add(operation);
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

        public void Present(Window target, string caseName, int index, int count,
            string phase, string action)
        {
            anchor = target;
            heading.Text = (DesktopPresentation.IsAdvanced ? DesktopPresentation.AccountLabel + " · " : "") + caseName + (index > 0 ? "  ·  " + index + " / " + count : "");
            operation.Text = phase + "：" + action;
            Position();
            if (OwnsPresentation && !IsVisible) Show();
            positionTimer.Start();
        }

        public void Suspend()
        {
            positionTimer.Stop();
            Hide();
        }

        private string PresentationFile => DesktopPresentation.IsAdvanced
            ? Path.Combine(Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROOT"), "presentation-owner.txt") : null;

        public void ClaimPresentation()
        {
            if (PresentationFile == null) return;
            string role = DesktopPresentation.IsObserver ? "observer" : "actor";
            string temporary = PresentationFile + "." + role;
            File.WriteAllText(temporary, role);
            if (!MoveFileEx(temporary, PresentationFile, 1))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        private bool OwnsPresentation
        {
            get
            {
                if (PresentationFile == null || !File.Exists(PresentationFile)) return true;
                try
                {
                    using (var stream = new FileStream(PresentationFile, FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite | FileShare.Delete))
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd() == (DesktopPresentation.IsObserver ? "observer" : "actor");
                    }
                }
                catch (IOException) { return false; }
            }
        }

        private void Position()
        {
            if (!OwnsPresentation)
            {
                if (IsVisible) Hide();
                return;
            }
            Rect area = DesktopPresentation.WorkArea(anchor ?? this);
            Rect bounds = DesktopPresentation.CaptionBounds(area);
            Width = Math.Min(860, bounds.Width);
            MaxHeight = bounds.Height;
            Left = bounds.Left;
            Top = bounds.Top;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool MoveFileEx(string source, string destination, uint flags);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr handle, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr handle, int index, int value);
    }
}
