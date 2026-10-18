using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Gateway.Contracts.Notification;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// notification 域实时视图（规范 H26/H27/H28 + S02 views 语义，行维护规则与 QuoteLiveView 相同）：
    /// 一行 = 一条通知（NotificationRecord），行标识 notificationId；排序 updateTime DESC + notificationId ASC。
    /// </summary>
    internal sealed class NotificationLiveView : IDisposable
    {
        /// <summary>行集合发生变化（推送消化/补位完成），UI 层据此重查。</summary>
        public event Action Changed;

        private readonly GatewayRequester requester;
        private readonly GatewayReportStream<NotificationStreamBatch> stream;
        private readonly Dictionary<string, NotificationRecord> rowsById = new Dictionary<string, NotificationRecord>(StringComparer.Ordinal);
        private List<NotificationRecord> ordered = new List<NotificationRecord>();
        private readonly object sync = new object();

        private string filterSignature;
        private int pageSize = 200;
        private bool refillQueued;
        private bool disposed;
        private readonly SemaphoreSlim requests = new SemaphoreSlim(1, 1);
        private readonly ViewSnapshotBuffer<NotificationStreamBatch> snapshots = new ViewSnapshotBuffer<NotificationStreamBatch>();

        /// <summary>视图 Id：条件变化重建视图时换新（服务端禁止复用已关闭的 viewId）；
        /// 推送按 ViewId 匹配，旧视图推送对新 Id 自动失效。</summary>
        public string ViewId { get; private set; }
        public string StreamId { get; private set; }
        public int TotalCount { get; private set; }

        public NotificationLiveView(GatewayRequester requester, GatewayReportStream<NotificationStreamBatch> stream)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            ViewId = "stc-nv-" + Guid.NewGuid().ToString("N").Substring(0, 12);
            stream.Register(HandleBatch);
            stream.Failed += OnStreamFailed;
        }

        /// <summary>当前已加载行（排序后）。</summary>
        public IReadOnlyList<NotificationRecord> Rows
        {
            get { lock (sync) return ordered; }
        }

        /// <summary>打开（或按新条件重建）视图并取第一页；条件未变时为空操作。</summary>
        public async Task<IReadOnlyList<NotificationRecord>> OpenAsync(QuoteFilter filter, string readState, int size, CancellationToken ct)
        {
            await requests.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (disposed) throw new ObjectDisposedException(GetType().Name);
                return await OpenAsyncCore(filter, readState, size, ct).ConfigureAwait(false);
            }
            catch
            {
                await CloseAsync().ConfigureAwait(false);
                throw;
            }
            finally
            {
                requests.Release();
            }
        }

        private async Task<IReadOnlyList<NotificationRecord>> OpenAsyncCore(QuoteFilter filter, string readState, int size, CancellationToken ct)
        {
            var signature = BuildSignature(filter, readState, size);
            lock (sync)
            {
                if (filterSignature == signature && StreamId != null && stream.IsActive(StreamId))
                {
                    return ordered;
                }
                pageSize = size;
            }

            await CloseAsync().ConfigureAwait(false);
            lock (sync)
            {
                // 新条件 = 新视图 Id（后端 1001: viewId cannot be reused）
                ViewId = ViewId.Substring(0, 7) + Guid.NewGuid().ToString("N").Substring(0, 12);
            }
            // 与 price 域同款降级：只等订阅完成，不等心跳首帧（重进场景重复订阅收不到 HEARTBEAT）
            await stream.EnsureSubscribedAsync(ct).ConfigureAwait(false);

            var request = new ViewOpenRequest { ViewId = ViewId, Filter = filter, PageSize = size, ReadState = readState };
            int epoch;
            lock (sync) epoch = snapshots.Begin();
            var data = await requester.RequestAsync<ViewOpenRequest, NotificationViewData>(
                GatewayTopics.NotificationViewOpen, request, ct).ConfigureAwait(false);

            lock (sync)
            {
                var replay = snapshots.Complete(epoch, data.Sequence);
                StreamId = data.StreamId;
                rowsById.Clear();
                if (data.Notifications != null)
                {
                    foreach (var n in data.Notifications)
                    {
                        if (n != null && n.NotificationId != null) rowsById[n.NotificationId] = n;
                    }
                }
                firstPageIds = new HashSet<string>(rowsById.Keys);
                RebuildOrdered();
                TotalCount = data.View != null && data.View.TotalCount.HasValue ? data.View.TotalCount.Value : rowsById.Count;
                filterSignature = signature;
                foreach (var batch in replay) HandleBatch(batch);
            }
            return ordered;
        }

        /// <summary>第一块行 Id（OpenAsync 时点）；行缓存淘汰时永不触碰。</summary>
        private HashSet<string> firstPageIds = new HashSet<string>();

        /// <summary>行缓存硬上限：超限淘汰深页行（非第一页），再由调用方合并新块；
        /// 被淘汰页往回翻会重新触发一次 view.query（以请求换内存封顶，峰值约 3000 行）。</summary>
        private void EnforceCacheLimit()
        {
            if (rowsById.Count <= 3000) return;
            foreach (var id in rowsById.Keys.Where(id => !firstPageIds.Contains(id)).ToList())
            {
                rowsById.Remove(id);
            }
        }

        public async Task<IReadOnlyList<NotificationRecord>> QueryPageAsync(int pageNumber, CancellationToken ct)
        {
            await requests.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (disposed) throw new ObjectDisposedException(GetType().Name);
                return await QueryPageAsyncCore(pageNumber, ct).ConfigureAwait(false);
            }
            catch
            {
                await CloseAsync().ConfigureAwait(false);
                throw;
            }
            finally
            {
                requests.Release();
            }
        }

        private async Task<IReadOnlyList<NotificationRecord>> QueryPageAsyncCore(int pageNumber, CancellationToken ct)
        {
            if (StreamId == null) throw new InvalidOperationException("view 未打开");
            var request = new ViewQueryRequest { ViewId = ViewId, PageNumber = pageNumber, PageSize = pageSize };
            int epoch;
            lock (sync) epoch = snapshots.Begin();
            var data = await requester.RequestAsync<ViewQueryRequest, NotificationViewData>(
                GatewayTopics.NotificationViewQuery, request, ct).ConfigureAwait(false);

            lock (sync)
            {
                var replay = snapshots.Complete(epoch, data.Sequence);
                EnforceCacheLimit();
                if (data.Notifications != null)
                {
                    foreach (var n in data.Notifications)
                    {
                        if (n != null && n.NotificationId != null) rowsById[n.NotificationId] = n;
                    }
                }
                RebuildOrdered();
                TotalCount = data.View != null && data.View.TotalCount.HasValue ? data.View.TotalCount.Value : TotalCount;
                foreach (var batch in replay) HandleBatch(batch);
            }
            return data.Notifications ?? new List<NotificationRecord>();
        }

        /// <summary>标记已读后本地同步置位（readTime 推送稍后也会到达，双保险让 UI 立即反映）。</summary>
        public void MarkReadLocally(string notificationId)
        {
            lock (sync)
            {
                NotificationRecord row;
                if (notificationId != null && rowsById.TryGetValue(notificationId, out row) && row != null && !row.ReadTime.HasValue)
                {
                    row.ReadTime = DateTime.Now;
                    RebuildOrdered();
                }
            }
            var handler = Changed;
            if (handler != null) handler();
        }

        public async Task CloseAsync()
        {
            string current;
            string closingView;
            lock (sync)
            {
                current = StreamId;
                closingView = ViewId;
                snapshots.Reset();
                StreamId = null;
                filterSignature = null;
                rowsById.Clear();
                firstPageIds.Clear();
                RebuildOrdered();
            }
            if (closingView == null) return;
            try
            {
                await requester.RequestAsync<ViewCloseRequest, object>(
                    GatewayTopics.NotificationViewClose, new ViewCloseRequest { ViewId = closingView }, CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // 关闭失败不阻断重建；服务端按连接释放
            }
        }

        internal void HandleBatch(NotificationStreamBatch batch)
        {
            List<string> upsertIds = null;
            List<string> removeIds = null;
            int totalCount = -1;
            if (batch.Views != null)
            {
                foreach (var v in batch.Views)
                {
                    if (v == null || v.ViewId != ViewId) continue;
                    upsertIds = v.UpsertIds;
                    removeIds = v.RemoveIds;
                    totalCount = v.TotalCount ?? -1;
                }
            }

            if (upsertIds == null && removeIds == null && totalCount < 0) return;
            bool firstPageChanged = false;
            bool removedFromLoaded = false;
            lock (sync)
            {
                if (disposed) return;
                if (StreamId != null && batch.StreamId != StreamId) return;
                if (snapshots.BufferOrSkip(batch)) return;
                if (StreamId == null) return;
                var isFirstPage = ordered.Count <= pageSize;
                if (removeIds != null)
                {
                    foreach (var id in removeIds)
                    {
                        if (rowsById.Remove(id)) removedFromLoaded = true;
                    }
                }
                if (batch.Notifications != null && upsertIds != null)
                {
                    var byId = batch.Notifications
                        .Where(n => n != null && n.NotificationId != null)
                        .GroupBy(n => n.NotificationId, StringComparer.Ordinal)
                        .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
                    foreach (var id in upsertIds)
                    {
                        NotificationRecord row;
                        if (!byId.TryGetValue(id, out row)) continue;
                        if (!isFirstPage && !rowsById.ContainsKey(id) && (removeIds == null || !removeIds.Contains(id)))
                        {
                            continue;
                        }
                        rowsById[id] = row;
                        firstPageChanged = true;
                    }
                }
                if (batch.Preference != null)
                {
                    // 偏好变化暂无 UI 消费；进推送即视为变化信号由上层处理
                }
                if (totalCount >= 0) TotalCount = totalCount;
                RebuildOrdered();
            }

            MaybeRefillFirstPage(removedFromLoaded);

            if (firstPageChanged || removedFromLoaded)
            {
                var handler = Changed;
                if (handler != null) handler();
            }
        }

        private void MaybeRefillFirstPage(bool removed)
        {
            if (!removed) return;
            lock (sync)
            {
                if (refillQueued) return;
                if (ordered.Count >= pageSize || ordered.Count >= TotalCount) return;
                refillQueued = true;
            }
            QueryPageAsync(1, CancellationToken.None).ContinueWith(_ =>
            {
                lock (sync) refillQueued = false;
                var handler = Changed;
                if (handler != null) handler();
            });
        }

        private void RebuildOrdered()
        {
            ordered = rowsById.Values
                .OrderByDescending(n => n.UpdateTime ?? DateTime.MinValue)
                .ThenBy(n => n.NotificationId, StringComparer.Ordinal)
                .ToList();
        }

        private static string BuildSignature(QuoteFilter filter, string readState, int size)
        {
            var payload = new ViewOpenRequest { Filter = filter, ReadState = readState, PageSize = size };
            return Newtonsoft.Json.JsonConvert.SerializeObject(payload, GatewayRequester.JsonSettings);
        }

        private void OnStreamFailed(string reason)
        {
            lock (sync)
            {
                StreamId = null;
                filterSignature = null;
                snapshots.Reset();
            }
        }

        public void Dispose()
        {
            disposed = true;
            stream.Failed -= OnStreamFailed;
            stream.Unregister(HandleBatch);
            _ = CloseAsync();
        }
    }
}
