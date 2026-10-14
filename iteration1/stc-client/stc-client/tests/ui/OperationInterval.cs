using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SalesTrading.UiTests
{
    // 管理窗口与独立工作进程共用的小型控制文件；运行中读取，不缓存启动参数。
    internal sealed class OperationInterval
    {
        public const int MaximumMilliseconds = 60000;
        private readonly string path;
        private readonly PlaybackControl playback;
        private int lastMilliseconds;

        public OperationInterval(string directory)
        {
            path = Path.Combine(directory, "operation-interval-ms.txt");
            playback = new PlaybackControl(directory);
        }

        public void Save(int milliseconds)
        {
            if (milliseconds < 0 || milliseconds > MaximumMilliseconds)
            {
                throw new ArgumentOutOfRangeException(nameof(milliseconds), "操作间隔须为 0 至 60000 毫秒。");
            }
            string temporary = path + ".new";
            File.WriteAllText(temporary, milliseconds.ToString(CultureInfo.InvariantCulture));
            if (File.Exists(path))
            {
                File.Replace(temporary, path, null);
            }
            else
            {
                File.Move(temporary, path);
            }
            lastMilliseconds = milliseconds;
        }

        private int Read()
        {
            try
            {
                int value;
                string text;
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete))
                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                    && value >= 0 && value <= MaximumMilliseconds)
                {
                    lastMilliseconds = value;
                }
            }
            catch (IOException)
            {
                // 原子替换时短暂不可读，沿用上一次有效值，下一轮重读。
            }
            return lastMilliseconds;
        }

        public async Task<bool> WaitAsync(Action checkCancellation, Action<bool, long> report,
            Action<bool> reportPause = null, bool gateOnly = false, bool singleStepGranted = false)
        {
            bool reportingWait = false;
            bool reportingPause = false;
            Stopwatch segment = new Stopwatch();
            long activeMilliseconds = 0;
            try
            {
                while (true)
                {
                    checkCancellation();
                    if (singleStepGranted)
                    {
                        return true;
                    }
                    if (playback.IsPaused)
                    {
                        if (reportingWait)
                        {
                            activeMilliseconds += segment.ElapsedMilliseconds;
                            report(false, segment.ElapsedMilliseconds);
                            reportingWait = false;
                            segment.Reset();
                        }
                        if (!reportingPause)
                        {
                            reportPause?.Invoke(true);
                            reportingPause = true;
                        }
                        // 全局一个许可只被一个工作进程领取，双账号不会同时单步放行。
                        if (playback.TryTakeStep())
                        {
                            return true;
                        }
                        await Task.Delay(100);
                        continue;
                    }
                    if (reportingPause)
                    {
                        reportPause?.Invoke(false);
                        reportingPause = false;
                    }
                    if (gateOnly)
                    {
                        return false;
                    }
                    long remaining = Read() - activeMilliseconds - segment.ElapsedMilliseconds;
                    if (remaining <= 0)
                    {
                        return false;
                    }
                    if (!reportingWait)
                    {
                        segment.Restart();
                        report(true, 0);
                        reportingWait = true;
                    }
                    await Task.Delay((int)Math.Min(100, remaining));
                }
            }
            finally
            {
                if (reportingWait)
                {
                    report(false, segment.ElapsedMilliseconds);
                }
                if (reportingPause)
                {
                    reportPause?.Invoke(false);
                }
            }
        }
    }

    // 两个客户端共享播放状态；暂停时间单独记账，业务请求超时不改变。
    internal sealed class PlaybackControl
    {
        private readonly string path;
        private readonly string lockPath;

        public PlaybackControl(string directory)
        {
            path = Path.Combine(directory, "playback-control.txt");
            lockPath = path + ".lock";
        }

        private sealed class State
        {
            public bool Paused;
            public int StepPermit;
            public long PausedSinceTicks;
            public long CompletedPauseTicks;
        }

        private State ReadState()
        {
            if (!File.Exists(path))
            {
                return new State();
            }
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete))
            using (var reader = new StreamReader(stream))
            {
                string[] fields = reader.ReadToEnd().Split('|');
                return new State
                {
                    Paused = bool.Parse(fields[0]),
                    StepPermit = int.Parse(fields[1], CultureInfo.InvariantCulture),
                    PausedSinceTicks = long.Parse(fields[2], CultureInfo.InvariantCulture),
                    CompletedPauseTicks = long.Parse(fields[3], CultureInfo.InvariantCulture)
                };
            }
        }

        private void WriteState(State state)
        {
            string temporary = path + ".new";
            File.WriteAllText(temporary, string.Join("|", state.Paused.ToString(),
                state.StepPermit.ToString(CultureInfo.InvariantCulture),
                state.PausedSinceTicks.ToString(CultureInfo.InvariantCulture),
                state.CompletedPauseTicks.ToString(CultureInfo.InvariantCulture)));
            if (File.Exists(path))
            {
                File.Replace(temporary, path, null);
            }
            else
            {
                File.Move(temporary, path);
            }
        }

        private FileStream Lock()
        {
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException) when (attempt < 50)
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        public bool IsPaused => ReadState().Paused;

        public long PausedMilliseconds
        {
            get
            {
                State state = ReadState();
                long ticks = state.CompletedPauseTicks;
                if (state.Paused)
                {
                    ticks += Math.Max(0, DateTime.UtcNow.Ticks - state.PausedSinceTicks);
                }
                return ticks / TimeSpan.TicksPerMillisecond;
            }
        }

        public void SetPaused(bool paused)
        {
            using (Lock())
            {
                State state = ReadState();
                if (paused && !state.Paused)
                {
                    state.PausedSinceTicks = DateTime.UtcNow.Ticks;
                }
                else if (!paused && state.Paused)
                {
                    state.CompletedPauseTicks += Math.Max(0, DateTime.UtcNow.Ticks - state.PausedSinceTicks);
                }
                state.Paused = paused;
                state.StepPermit = 0;
                WriteState(state);
            }
        }

        public void NextAction()
        {
            using (Lock())
            {
                State state = ReadState();
                if (!state.Paused)
                {
                    throw new InvalidOperationException("请先暂停，再执行下一动作。");
                }
                state.StepPermit = 1;
                WriteState(state);
            }
        }

        public bool TryTakeStep()
        {
            using (Lock())
            {
                State state = ReadState();
                if (!state.Paused || state.StepPermit == 0)
                {
                    return false;
                }
                state.StepPermit = 0;
                WriteState(state);
                return true;
            }
        }
    }

    // 只扣除显式操作间隔；服务请求、UI 就绪等待和停止宽限期仍受原看门狗约束。
    internal sealed class OperationWaitBudget
    {
        private readonly Stopwatch current = new Stopwatch();
        private long completedMilliseconds;

        public void Begin()
        {
            if (!current.IsRunning)
            {
                current.Restart();
            }
        }

        public void Complete(long milliseconds)
        {
            if (!current.IsRunning)
            {
                return;
            }
            completedMilliseconds += Math.Max(0, Math.Min(OperationInterval.MaximumMilliseconds + 100, milliseconds));
            current.Reset();
        }

        public long ExcludedMilliseconds => completedMilliseconds
            + Math.Min(OperationInterval.MaximumMilliseconds, current.ElapsedMilliseconds);
    }
}
