using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;

namespace STC.SalesTrading.Plugin.Gateway
{
    internal enum StreamHandshake
    {
        Heartbeat,
        Image
    }

    /// <summary>流负责序号校验；视图负责快照与推送的一致性。</summary>
    internal sealed class GatewayReportStream<TBatch> : IDisposable where TBatch : StreamMessage
    {
        public event Action<string> Failed;
        private readonly GatewayRequester requester;
        private readonly string streamOpenTopic;
        private readonly bool withImage;
        private readonly object sync = new object();
        private readonly object deliverySync = new object();
        private readonly Queue<TBatch> buffered = new Queue<TBatch>();
        private readonly List<Action<TBatch>> sinks = new List<Action<TBatch>>();
        private TaskCompletionSource<bool> opening;
        private IDisposable subscription;
        private string streamId;
        private long? lastSequence;
        private bool disposed;
        private int generation;

        public GatewayReportStream(GatewayRequester requester, string topic,
            StreamHandshake handshake = StreamHandshake.Heartbeat)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
            streamOpenTopic = topic ?? throw new ArgumentNullException(nameof(topic));
            withImage = handshake == StreamHandshake.Image;
        }

        public bool IsActive(string expectedStreamId)
        {
            lock (sync) return !disposed && subscription != null && streamId == expectedStreamId;
        }

        public async Task<bool> EnsureSubscribedAsync(CancellationToken ct)
        {
            TaskCompletionSource<bool> pending;
            bool start = false;
            int epoch;
            lock (sync)
            {
                if (disposed) throw new ObjectDisposedException(GetType().Name);
                if (subscription != null) return true;
                if (opening == null)
                {
                    opening = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    start = true;
                }
                pending = opening;
                epoch = generation;
            }
            if (start) _ = StartAsync(pending, epoch);
            var completed = await Task.WhenAny(pending.Task, Task.Delay(10000, ct)).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
            if (completed != pending.Task) throw new TimeoutException("实时订阅建立超时");
            return await pending.Task.ConfigureAwait(false);
        }

        private async Task StartAsync(TaskCompletionSource<bool> pending, int epoch)
        {
            try
            {
                var data = await requester.RequestAsync<object, StreamOpenData>(
                    streamOpenTopic, new object(), CancellationToken.None).ConfigureAwait(false);
                if (data == null || string.IsNullOrEmpty(data.PushTopic) || string.IsNullOrEmpty(data.StreamId))
                    throw new InvalidOperationException("stream.open 缺少 streamId/pushTopic");
                lock (sync)
                {
                    if (disposed || generation != epoch) return;
                    streamId = data.StreamId;
                }
                var created = requester.Subscribe(data.PushTopic, withImage, HandlePushJson);
                lock (sync)
                {
                    if (disposed || generation != epoch)
                    {
                        created.Dispose();
                        return;
                    }
                    subscription = created;
                }
                pending.TrySetResult(true);
            }
            catch (Exception error)
            {
                lock (sync)
                {
                    if (opening == pending) opening = null;
                }
                pending.TrySetException(error);
            }
        }

        public void Register(Action<TBatch> sink)
        {
            lock (deliverySync)
            {
                List<TBatch> replay;
                lock (sync)
                {
                    if (disposed) throw new ObjectDisposedException(GetType().Name);
                    sinks.Add(sink);
                    replay = new List<TBatch>(buffered);
                    buffered.Clear();
                }
                // 缓冲已完成序号校验，回放时直接交给视图。
                foreach (var batch in replay) Deliver(batch);
            }
        }

        public void Unregister(Action<TBatch> sink)
        {
            lock (sync) sinks.Remove(sink);
        }

        internal void HandlePushJson(string json)
        {
            lock (deliverySync)
            {
                try
                {
                    var batch = GatewayRequester.ReadReply<TBatch>("push", json);
                    if (batch == null) throw new InvalidOperationException("推送为空");
                    long sequence;
                    if (!long.TryParse(batch.Sequence, out sequence))
                        throw new InvalidOperationException("推送 sequence 无效");
                    lock (sync)
                    {
                        if (disposed) return;
                        // 忽略旧 topic 的在途推送。
                        if (batch.StreamId != streamId) return;
                        if (lastSequence.HasValue && sequence <= lastSequence.Value) return;
                        if (lastSequence.HasValue && sequence != lastSequence.Value + 1)
                            throw new InvalidOperationException("推送缺号：期望 " + (lastSequence.Value + 1) + "，实际 " + sequence);
                        lastSequence = sequence;
                        if (batch.Kind == StreamMessage.KindHeartbeat) return;
                        if (sinks.Count == 0)
                        {
                            buffered.Enqueue(batch);
                            return;
                        }
                    }
                    Deliver(batch);
                }
                catch (Exception error)
                {
                    Invalidate(error.Message);
                }
            }
        }

        private void Deliver(TBatch batch)
        {
            List<Action<TBatch>> handlers;
            lock (sync) handlers = new List<Action<TBatch>>(sinks);
            foreach (var handler in handlers) handler(batch);
        }

        private void Invalidate(string reason)
        {
            IDisposable previous;
            lock (sync)
            {
                if (disposed) return;
                generation++;
                previous = subscription;
                subscription = null;
                streamId = null;
                lastSequence = null;
                buffered.Clear();
                opening?.TrySetException(new InvalidOperationException(reason));
                opening = null;
            }
            previous?.Dispose();
            Failed?.Invoke(reason);
        }

        public void Dispose()
        {
            IDisposable previous;
            lock (sync)
            {
                if (disposed) return;
                disposed = true;
                generation++;
                previous = subscription;
                subscription = null;
                sinks.Clear();
                buffered.Clear();
                opening?.TrySetCanceled();
            }
            previous?.Dispose();
        }
    }
}
