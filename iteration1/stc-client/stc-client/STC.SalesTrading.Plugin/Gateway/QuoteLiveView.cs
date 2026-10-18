using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Gateway.Contracts.Quote;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// quote 域实时视图（规范 H16/H17/H18 + S01 views 语义）：
    /// open 登记条件并取第一页 → 推送 upsertIds/removeIds 维护本地行集合。
    /// 第一页由客户端按「updateTime DESC, quoteId ASC」排序并裁剪至 pageSize，
    /// 移除出现缺口时合并请求一次第一页补位；后续页只更新/移除已有行，忽略未加载 Id 的 upsert。
    /// viewId 由客户端生成且永不复用；改条件 = close 旧 view + 新 viewId 重新 open。
    /// </summary>
    internal sealed class QuoteLiveView : IDisposable
    {
        /// <summary>行集合发生变化（推送消化/补位完成），UI 层据此重查。</summary>
        public event Action Changed;

        private readonly GatewayRequester requester;
        private readonly GatewayReportStream<QuoteStreamBatch> stream;
        private readonly Dictionary<string, QuoteRecord> rowsById = new Dictionary<string, QuoteRecord>(StringComparer.Ordinal);
        private List<QuoteRecord> ordered = new List<QuoteRecord>();
        private readonly object sync = new object();

        private string filterSignature;
        private int pageSize = 200;
        private bool refillQueued;
        private bool disposed;
        private readonly SemaphoreSlim requests = new SemaphoreSlim(1, 1);
        private readonly ViewSnapshotBuffer<QuoteStreamBatch> snapshots = new ViewSnapshotBuffer<QuoteStreamBatch>();


        /// <summary>视图 Id：条件变化重建视图时换新（服务端禁止复用已关闭的 viewId）；
        /// 推送按 ViewId 匹配，旧视图推送对新 Id 自动失效。</summary>
        public string ViewId { get; private set; }
        public string StreamId { get; private set; }
        public int TotalCount { get; private set; }

        public QuoteLiveView(GatewayRequester requester, GatewayReportStream<QuoteStreamBatch> stream)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            ViewId = "stc-qv-" + Guid.NewGuid().ToString("N").Substring(0, 12);
            stream.Register(HandleBatch);
            stream.Failed += OnStreamFailed;
        }

        /// <summary>当前已加载行（排序后）。只读快照引用，消化推送时会整体替换。</summary>
        public IReadOnlyList<QuoteRecord> Rows
        {
            get { lock (sync) return ordered; }
        }

        /// <summary>
        /// 最近一次应答/推送附带的债券资料（决策 #9 已落地）。securityId → 资料；
        /// 服务层映射行时按 securityId 取用，缺失部分再走 base.get 兜底。
        /// </summary>
        public IReadOnlyDictionary<string, QuoteReference> LastReference
        {
            get { lock (sync) return lastReference; }
        }

        private Dictionary<string, QuoteReference> lastReference = new Dictionary<string, QuoteReference>();

        /// <summary>
        /// 打开（或按新条件重建）视图并取第一页。已打开且筛选签名未变时为空操作。
        /// 返回第一页行。
        /// </summary>
        public async Task<IReadOnlyList<QuoteRecord>> OpenAsync(QuoteFilter filter, List<string> status, string ownerScope, int size, CancellationToken ct)
        {
            await requests.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (disposed) throw new ObjectDisposedException(GetType().Name);
                return await OpenAsyncCore(filter, status, ownerScope, size, ct).ConfigureAwait(false);
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

        private async Task<IReadOnlyList<QuoteRecord>> OpenAsyncCore(QuoteFilter filter, List<string> status, string ownerScope, int size, CancellationToken ct)
        {
            var signature = BuildSignature(filter, status, ownerScope, size);
            lock (sync)
            {
                if (filterSignature == signature && StreamId != null && stream.IsActive(StreamId))
                {
                    return ordered;
                }
                pageSize = size;
            }

            // 换条件：close 旧 view（旧 viewId 永不复用），以新 viewId open
            await CloseAsync().ConfigureAwait(false);
            lock (sync)
            {
                // 新条件 = 新视图 Id（后端 1001: viewId cannot be reused）
                ViewId = ViewId.Substring(0, 7) + Guid.NewGuid().ToString("N").Substring(0, 12);
            }

            // 与 price 域同款降级：只等订阅完成（stream.open 应答），不等心跳首帧——
            // 重进场景重复订阅可能收不到 HEARTBEAT，等它只会 10s 超时循环；增量随后台握手自愈
            await stream.EnsureSubscribedAsync(ct).ConfigureAwait(false);

            var request = new ViewOpenRequest
            {
                ViewId = ViewId,
                Filter = filter,
                PageSize = size,
                OwnerScope = ownerScope,
                Status = status,
            };
            int epoch;
            lock (sync) epoch = snapshots.Begin();
            var data = await requester.RequestAsync<ViewOpenRequest, QuoteViewData>(
                GatewayTopics.QuoteViewOpen, request, ct).ConfigureAwait(false);

            lock (sync)
            {
                var replay = snapshots.Complete(epoch, data.Sequence);
                StreamId = data.StreamId;
                if (data.Reference != null) lastReference = new Dictionary<string, QuoteReference>(data.Reference);
                rowsById.Clear();
                if (data.Quotes != null)
                {
                    foreach (var q in data.Quotes) rowsById[q.QuoteId] = q;
                }
                RebuildOrdered();
                TotalCount = data.View != null && data.View.TotalCount.HasValue ? data.View.TotalCount.Value : rowsById.Count;
                filterSignature = signature;
                foreach (var batch in replay) HandleBatch(batch);
            }
            return ordered;
        }

        /// <summary>查询指定页（滚动加载）。返回该页行；不改变第一页缓存语义。</summary>
        public async Task<IReadOnlyList<QuoteRecord>> QueryPageAsync(int pageNumber, CancellationToken ct)
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

        private async Task<IReadOnlyList<QuoteRecord>> QueryPageAsyncCore(int pageNumber, CancellationToken ct)
        {
            if (StreamId == null) throw new InvalidOperationException("view 未打开");
            var request = new ViewQueryRequest { ViewId = ViewId, PageNumber = pageNumber, PageSize = pageSize };
            int epoch;
            lock (sync) epoch = snapshots.Begin();
            var data = await requester.RequestAsync<ViewQueryRequest, QuoteViewData>(
                GatewayTopics.QuoteViewQuery, request, ct).ConfigureAwait(false);

            lock (sync)
            {
                var replay = snapshots.Complete(epoch, data.Sequence);
                if (data.Reference != null) lastReference = new Dictionary<string, QuoteReference>(data.Reference);
                if (data.Quotes != null)
                {
                    foreach (var q in data.Quotes) rowsById[q.QuoteId] = q;
                }
                RebuildOrdered();
                TotalCount = data.View != null && data.View.TotalCount.HasValue ? data.View.TotalCount.Value : TotalCount;
                foreach (var batch in replay) HandleBatch(batch);
            }
            return data.Quotes ?? new List<QuoteRecord>();
        }

        /// <summary>关闭视图（幂等）。已关闭 view 的在途消息由服务端忽略。</summary>
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
                RebuildOrdered();
            }
            if (closingView == null) return;
            try
            {
                await requester.RequestAsync<ViewCloseRequest, object>(
                    GatewayTopics.QuoteViewClose, new ViewCloseRequest { ViewId = closingView }, CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // 关闭失败不阻断重建；服务端按连接释放
            }
        }

        /// <summary>推送消化（GatewayReportStream<QuoteStreamBatch> 分发）。只处理引用本 viewId 的 views 项。</summary>
        internal void HandleBatch(QuoteStreamBatch batch)
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
                if (StreamId == null) return;   // 未打开/已关闭：在途消息忽略
                if (batch.Reference != null && batch.Reference.Count > 0)
                {
                    lastReference = new Dictionary<string, QuoteReference>(batch.Reference);
                }

                var isFirstPage = ordered.Count <= pageSize;
                if (removeIds != null)
                {
                    foreach (var id in removeIds)
                    {
                        if (rowsById.Remove(id)) removedFromLoaded = true;
                    }
                }
                if (batch.Quotes != null && upsertIds != null)
                {
                    // GroupBy 防御：ToDictionary 遇重复/空 QuoteId 会抛（被 Dispatch 吞掉整批静默丢弃）
                    var byId = batch.Quotes
                        .Where(q => q != null && q.QuoteId != null)
                        .GroupBy(q => q.QuoteId, StringComparer.Ordinal)
                        .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
                    foreach (var id in upsertIds)
                    {
                        QuoteRecord q;
                        if (!byId.TryGetValue(id, out q)) continue;     // 规范：upsert 必须在本批 quotes 中
                        if (!isFirstPage && !rowsById.ContainsKey(id) && (removeIds == null || !removeIds.Contains(id)))
                        {
                            continue;                                    // 后续页：忽略未加载 Id 的 upsert
                        }
                        rowsById[id] = q;
                        firstPageChanged = true;
                    }
                }
                if (totalCount >= 0) TotalCount = totalCount;
                RebuildOrdered();
            }

            // 第一页缺口补位（规范 3.3：合并请求一次第一页）
            MaybeRefillFirstPage(removedFromLoaded);

            if (firstPageChanged || removedFromLoaded)
            {
                var handler = Changed;
                if (handler != null) handler();
            }
        }

        /// <summary>第一页因移除出现缺口时，请求一次第一页补位（串行化，避免风暴）。</summary>
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

        /// <summary>updateTime DESC + quoteId ASC（ordinal）重建有序快照。</summary>
        private void RebuildOrdered()
        {
            ordered = rowsById.Values
                .OrderByDescending(q => q.UpdateTime ?? DateTime.MinValue)
                .ThenBy(q => q.QuoteId, StringComparer.Ordinal)
                .ToList();
        }


        private static string BuildSignature(QuoteFilter filter, List<string> status, string ownerScope, int size)
        {
            // 筛选签名：条件变化需要重建 view。用序列化后的 JSON 足够稳定且实现最短。
            var payload = new ViewOpenRequest { Filter = filter, Status = status, OwnerScope = ownerScope, PageSize = size };
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
            _ = CloseAsync();   // 释放服务端视图（tab 关闭后残留会导致重建时行为异常）
        }
    }
}
