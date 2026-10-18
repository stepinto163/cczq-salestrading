using System;
using System.Diagnostics;
using System.Threading;

namespace SalesTrading.UiTests
{
    /// <summary>
    /// 可选的客户端阶段计时。默认关闭，不写磁盘、不记录业务数据。
    /// 测试端提供线程安全的内存接收器；起止时间使用同一单调时钟。
    /// 异步阶段包含等待，嵌套阶段不可直接相加。
    /// </summary>
    public static class ClientPerformanceTrace
    {
        public static Action<string, long, long, int> Sink { get; set; }

        public static long Start()
        {
            return Sink == null ? 0 : Stopwatch.GetTimestamp();
        }

        public static void End(string stage, long started)
        {
            if (started == 0) return;
            var sink = Sink;
            sink?.Invoke(stage, started, Stopwatch.GetTimestamp(), Thread.CurrentThread.ManagedThreadId);
        }
    }
}
