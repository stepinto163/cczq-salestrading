using System;
using System.Threading;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    public sealed class LatestRequestCoordinator : IDisposable
    {
        private readonly object _gate = new object();
        private CancellationTokenSource _current;
        private long _generation;
        private bool _disposed;

        public Lease Begin()
        {
            lock (_gate)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(LatestRequestCoordinator));
                _current?.Cancel();
                _current?.Dispose();
                _current = new CancellationTokenSource();
                return new Lease(this, _current, ++_generation);
            }
        }

        public void CancelCurrent()
        {
            lock (_gate)
            {
                _current?.Cancel();
            }
        }

        private bool IsCurrent(CancellationTokenSource source, long generation)
        {
            lock (_gate)
            {
                return !_disposed && ReferenceEquals(_current, source) && _generation == generation
                    && !source.IsCancellationRequested;
            }
        }

        private void Complete(CancellationTokenSource source, long generation)
        {
            lock (_gate)
            {
                if (_disposed || !ReferenceEquals(_current, source) || _generation != generation) return;
                _current = null;
                source.Dispose();
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (_disposed) return;
                _disposed = true;
                _current?.Cancel();
                _current?.Dispose();
                _current = null;
            }
        }

        public sealed class Lease : IDisposable
        {
            private readonly LatestRequestCoordinator _owner;
            private readonly CancellationTokenSource _source;
            private readonly long _generation;
            private int _disposed;

            internal Lease(LatestRequestCoordinator owner, CancellationTokenSource source, long generation)
            {
                _owner = owner;
                _source = source;
                _generation = generation;
            }

            public CancellationToken Token => _source.Token;
            public bool IsCurrent => _owner.IsCurrent(_source, _generation);
            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                    _owner.Complete(_source, _generation);
            }
        }
    }
}
