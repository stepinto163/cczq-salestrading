using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace STC.Common.View.UI
{
    public partial class BalloonWindow : Window
    {
        public BalloonWindow(string title, string message, int durationMs = 3000)
        {
            InitializeComponent();

            TitleBlock.Text = title;
            MessageBlock.Text = message;

            Loaded += (s, e) =>
            {
                PositionNearTray(); // 定位到托盘旁边
                StartAutoClose(durationMs); // 自动关闭
            };
        }

        private void StartAutoClose(int durationMs)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                Close();
            };
            timer.Start();
        }

        /// <summary>
        /// 将窗口定位到任务栏托盘的右下角
        /// </summary>
        private void PositionNearTray()
        {
            var workArea = SystemParameters.WorkArea; // 可用工作区
            Left = workArea.Right - Width - 10;
            Top = workArea.Bottom - Height - 10;
        }
    }
}
