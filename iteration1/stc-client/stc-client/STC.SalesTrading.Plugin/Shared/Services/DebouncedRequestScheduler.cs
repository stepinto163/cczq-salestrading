using System;
using System.Threading;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>可取消的轻量查询防抖调度器，避免文本条件变化时连续触发请求。</summary>
    public sealed class DebouncedRequestScheduler : IDisposable
    {
        private readonly object _gate = new object();
        private CancellationTokenSource _pending;
        private bool _disposed;

        public void Schedule(TimeSpan delay, Func<CancellationToken, Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            CancellationTokenSource source;
            lock (_gate)
            {
                if (_disposed) return;
                _pending?.Cancel();
                source = new CancellationTokenSource();
                _pending = source;
            }

            _ = RunAsync(source, delay, action);
        }

        public void Cancel()
        {
            lock (_gate)
            {
                _pending?.Cancel();
            }
        }

        private async Task RunAsync(CancellationTokenSource source, TimeSpan delay, Func<CancellationToken, Task> action)
        {
            try
            {
                CancellationToken token = source.Token;
                // The caller is a WPF view model. Resume on its dispatcher so query
                // completion can update bound collections and the loading state.
                await Task.Delay(delay, token);
                token.ThrowIfCancellationRequested();
                await action(token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                lock (_gate)
                {
                    if (ReferenceEquals(_pending, source)) _pending = null;
                }
                source.Dispose();
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (_disposed) return;
                _disposed = true;
                _pending?.Cancel();
                // RunAsync owns disposal, including cancellation during the delay.
                _pending = null;
            }
        }
    }
}
