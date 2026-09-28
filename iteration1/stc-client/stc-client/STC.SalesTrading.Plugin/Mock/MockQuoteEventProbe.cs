using System.Collections.Concurrent;
using System.Collections.Generic;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Mock
{
    /// <summary>供 PowerShell/Smoke 验收使用的线程安全推送采集器。</summary>
    public sealed class MockQuoteEventProbe
    {
        private readonly ConcurrentQueue<QuoteEvent> _events = new ConcurrentQueue<QuoteEvent>();

        public MockQuoteEventProbe()
        {
            Handler = Handle;
        }

        public System.Action<QuoteEvent> Handler { get; }

        public void Handle(QuoteEvent message)
        {
            if (message != null) _events.Enqueue(message);
        }

        public IReadOnlyList<QuoteEvent> Snapshot()
        {
            return _events.ToArray();
        }
    }
}
