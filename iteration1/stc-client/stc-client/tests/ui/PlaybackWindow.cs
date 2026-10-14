using System;
using System.Drawing;
using System.Windows.Forms;

namespace SalesTrading.UiTests
{
    // 模型无关的浮动播放控制窗；管理器把事件传给当前套件，工作进程共享播放文件。
    internal sealed class PlaybackWindow : Form
    {
        private static readonly int[] Delays = { 8000, 4000, 2000, 1000, 500, 0 };
        private static readonly string[] Speeds = { "0.25×", "0.5×", "1×", "2×", "4×", "最快" };
        private readonly TrackBar speed = new TrackBar { Minimum = 0, Maximum = 5, Value = 2,
            TickFrequency = 1, AutoSize = false, Height = 32, Dock = DockStyle.Fill };
        private readonly Label speedText = new Label { AutoSize = true };
        private readonly Button pause = new Button { Text = "暂停", AutoSize = true, Enabled = false };
        private readonly Button next = new Button { Text = "下一动作", AutoSize = true, Enabled = false };
        private readonly Label state = new Label { Dock = DockStyle.Fill, AutoEllipsis = true, Text = "就绪 · 可提前调整播放速度" };
        private bool paused;
        private bool running;
        public event Action<int> SpeedChanged;
        public event Action<bool> PauseChanged;
        public event Action NextRequested;
        public int DelayMilliseconds => Delays[speed.Value];

        public PlaybackWindow()
        {
            Text = "自动测试 · 播放控制";
            Font = new Font("Microsoft YaHei UI", 9);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ClientSize = new Size(430, 140);
            TopMost = true;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - Width - 24,
                Screen.PrimaryScreen.WorkingArea.Top + 60);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                Padding = new Padding(8) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var slider = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            slider.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
            slider.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            slider.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
            slider.Controls.Add(new Label { Text = "慢", AutoSize = true }, 0, 0);
            slider.Controls.Add(speed, 1, 0);
            slider.Controls.Add(new Label { Text = "快", AutoSize = true }, 2, 0);
            layout.Controls.Add(slider, 0, 0);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            buttons.Controls.Add(pause);
            buttons.Controls.Add(next);
            buttons.Controls.Add(speedText);
            layout.Controls.Add(buttons, 0, 1);
            layout.Controls.Add(state, 0, 2);
            Controls.Add(layout);
            speed.ValueChanged += (sender, args) =>
            {
                UpdateSpeedText();
                SpeedChanged?.Invoke(DelayMilliseconds);
            };
            pause.Click += (sender, args) =>
            {
                paused = !paused;
                pause.Text = paused ? "继续" : "暂停";
                next.Enabled = paused && running;
                state.Text = paused ? "暂停请求：当前请求结束后，在下一动作处暂停" : "正在播放";
                PauseChanged?.Invoke(paused);
            };
            next.Click += (sender, args) =>
            {
                state.Text = "单步放行一个动作或验证阶段，然后保持暂停";
                NextRequested?.Invoke();
            };
            FormClosing += (sender, args) =>
            {
                if (args.CloseReason == CloseReason.UserClosing)
                {
                    args.Cancel = true;
                    Hide();
                }
            };
            UpdateSpeedText();
        }

        private void UpdateSpeedText()
        {
            speedText.Text = Speeds[speed.Value] + " · " + (DelayMilliseconds == 0 ? "无额外等待" : DelayMilliseconds / 1000.0 + " 秒");
        }

        public void SetRunning(bool value)
        {
            running = value;
            paused = false;
            pause.Enabled = value;
            pause.Text = "暂停";
            next.Enabled = false;
            state.Text = value ? "正在播放" : "就绪 · 可提前调整播放速度";
        }

        public void ShowProgress(string text)
        {
            state.Text = text;
        }
    }
}
