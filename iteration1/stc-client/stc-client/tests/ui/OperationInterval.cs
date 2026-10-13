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
        private int lastMilliseconds;

        public OperationInterval(string directory)
        {
            path = Path.Combine(directory, "operation-interval-ms.txt");
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

        public async Task WaitAsync(Action checkCancellation, Action<bool, long> report)
        {
            checkCancellation();
            if (Read() == 0)
            {
                return;
            }
            Stopwatch waiting = Stopwatch.StartNew();
            report(true, 0);
            try
            {
                while (true)
                {
                    checkCancellation();
                    long remaining = Read() - waiting.ElapsedMilliseconds;
                    if (remaining <= 0)
                    {
                        return;
                    }
                    // 异步等待保持 WPF 消息循环响应；调小间隔或停止最多约 100ms 被观察到。
                    await Task.Delay((int)Math.Min(100, remaining));
                }
            }
            finally
            {
                report(false, waiting.ElapsedMilliseconds);
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
