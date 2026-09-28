using STC.Chrome.Plugin.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Chrome.Plugin.Handler
{
    public class TokenCountdownHandler
    {
        private static readonly TokenCountdownHandler instance = new TokenCountdownHandler();
        public static TokenCountdownHandler Instance { get { return instance; } }

        private readonly object _lock = new object();

        private Thread _workerThread;
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);

        public bool IsValidServerToken { get; private set; }

        // Token 即将失效（剩余 <120分钟）
        public event EventHandler TokenInvalidated;

        // 每次回调剩余秒数
        public event EventHandler<int> Tick;

        //是否已经订阅倒计时事件
        public bool IsTickSubscribed = false;


        public void Start(int seconds)
        {
            lock (_lock)
            {
                StopInternal();

                IsValidServerToken = true;
                _stopEvent = new ManualResetEvent(false);

                _workerThread = new Thread(() => RunCountdown(seconds));
                _workerThread.IsBackground = true;
                _workerThread.Start();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                StopInternal();
            }
        }

        private void StopInternal()
        {
            try
            {
                if (_stopEvent != null)
                    _stopEvent.Set();

                if (_workerThread != null && _workerThread.IsAlive)
                    _workerThread.Join(50);
            }
            catch { }

            _workerThread = null;
        }


        private void RunCountdown(int seconds)
        {
            try
            {
                int remain = seconds;
                while (remain > 0)
                {
                    // 每10秒 => 可提前停止
                    if (_stopEvent.WaitOne(10000))
                        return;
                    remain -= 10;
                    try
                    {
                        Tick?.Invoke(this, remain);
                    }
                    catch
                    {
                        // 回调异常不能影响计数线程
                    }

                    // <120分钟 => Token无效触发
                    if (remain < 120 * 60 && IsValidServerToken)
                    {
                        IsValidServerToken = false;
                        try
                        {
                            TokenInvalidated?.Invoke(this, EventArgs.Empty);
                        }
                        catch
                        {
                            // 同样不能让外部事件导致线程崩溃
                        }
                        return;
                    }
                }
            }
            catch
            {
                // 兜底，确保任何异常都不会向外抛
                // 线程绝不能让异常逃逸，否则最终 TaskScheduler 会捕获
            }
        }

        /// <summary>
        /// 重置 Token 倒计时
        /// </summary>
        public void Reset(int seconds)
        {
            Start(seconds);
        }

        /// <summary>
        /// 将 Unix 时间戳（秒）转换为本地时间
        /// </summary>
        /// <param name="timestamp">Unix 时间戳（秒）</param>
        /// <returns>本地时间</returns>
        public DateTime FromUnixTimeSeconds(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().DateTime;
        }

        /// <summary>
        /// 将本地时间转换为 Unix 时间戳（秒）
        /// </summary>
        /// <param name="time">本地时间</param>
        /// <returns>Unix 时间戳（秒）</returns>
        public long ToUnixTimeSeconds(DateTime time)
        {
            return new DateTimeOffset(time).ToUnixTimeSeconds();
        }

        /// <summary>
        /// 将秒数转换为 HH:mm:ss 格式
        /// </summary>
        public string SecondsToTimeString(int seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                (int)ts.TotalHours, ts.Minutes, ts.Seconds);
        }
    }
}
