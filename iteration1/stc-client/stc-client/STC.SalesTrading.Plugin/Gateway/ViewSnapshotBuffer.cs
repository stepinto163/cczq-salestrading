using System;
using System.Collections.Generic;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>由视图锁保护，在快照请求期间缓冲推送并按 sequence 回放。</summary>
    internal sealed class ViewSnapshotBuffer<TBatch> where TBatch : StreamMessage
    {
        private readonly List<TBatch> pending = new List<TBatch>();
        private bool loading;
        private long sequence = -1;
        private int generation;

        public int Begin()
        {
            if (loading) throw new InvalidOperationException("视图已有在途请求");
            loading = true;
            return generation;
        }

        public void Reset()
        {
            generation++;
            loading = false;
            sequence = -1;
            pending.Clear();
        }

        public bool BufferOrSkip(TBatch batch)
        {
            if (loading)
            {
                pending.Add(batch);
                return true;
            }
            var incoming = ParseSequence(batch.Sequence);
            if (incoming <= sequence) return true;
            sequence = incoming;
            return false;
        }

        public IReadOnlyList<TBatch> Complete(int epoch, string snapshotSequence)
        {
            if (epoch != generation) throw new OperationCanceledException("视图已关闭或重建");
            var incoming = ParseSequence(snapshotSequence);
            if (incoming < sequence) throw new InvalidOperationException("快照早于已应用推送，请重新加载");
            sequence = incoming;
            loading = false;
            var replay = pending.ToArray();
            pending.Clear();
            return replay;
        }

        private static long ParseSequence(string text)
        {
            long value;
            if (!long.TryParse(text, out value)) throw new InvalidOperationException("推送 sequence 无效");
            return value;
        }
    }
}
