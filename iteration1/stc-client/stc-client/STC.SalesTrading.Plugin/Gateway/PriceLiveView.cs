using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// price 域实时视图（规范 H51/H52/H53 + S03 views 语义，行维护规则与 QuoteLiveView 相同）：
    /// 一行 = 一券的完整价格簿（PriceBookRow），行标识 securityId；
    /// 排序 updateTime DESC + securityId ASC；第一页裁剪、移除缺口合并一次补位、后续页忽略未加载 upsert。
    /// </summary>
    internal sealed class PriceLiveView : IDisposable
    {
        /// <summary>行集合发生变化（推送消化/补位完成），UI 层据此重查。</summary>
        public event Action Changed;

        private readonly GatewayRequester requester;
        private readonly GatewayReportStream<PriceStreamBatch> stream;
        private readonly Dictionary<string, PriceBookRow> rowsById = new Dictionary<string, PriceBookRow>(StringComparer.Ordinal);
        private List<PriceBookRow> ordered = new List<PriceBookRow>();
        private readonly object sync = new object();

        private string filterSignature;
        private int pageSize = 200;
        private bool refillQueued;
        private bool disposed;
        private readonly SemaphoreSlim requests = new SemaphoreSlim(1, 1);
        private readonly ViewSnapshotBuffer<PriceStreamBatch> snapshots = new ViewSnapshotBuffer<PriceStreamBatch>();

        /// <summary>视图 Id：条件变化重建视图时换新（服务端禁止复用已关闭的 viewId）；
        /// 推送按 ViewId 匹配，旧视图推送对新 Id 自动失效。</summary>
        public string ViewId { get; private set; }
        public int TotalCount { get; private set; }

        public PriceLiveView(GatewayRequester requester, GatewayReportStream<PriceStreamBatch> stream)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            ViewId = "stc-pv-" + Guid.NewGuid().ToString("N").Substring(0, 12);
            stream.Register(HandleBatch);
            stream.Failed += OnStreamFailed;
        }

        /// <summary>当前已加载行（排序后）。</summary>
        public IReadOnlyList<PriceBookRow> Rows
        {
            get { lock (sync) return ordered; }
        }

        /// <summary>打开（或按新条件重建）视图并取第一页；条件未变时为空操作。</summary>
        public async Task<IReadOnlyList<PriceBookRow>> OpenAsync(QuoteFilter filter, string mode, int size, CancellationToken ct)
        {
            await requests.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (disposed) throw new ObjectDisposedException(GetType().Name);
                return await OpenAsyncCore(filter, mode, size, ct).ConfigureAwait(false);
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

        private async Task<IReadOnlyList<PriceBookRow>> OpenAsyncCore(QuoteFilter filter, string mode, int size, CancellationToken ct)
        {
            var signature = BuildSignature(filter, mode, size);
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
                ViewId = "stc-pv-" + Guid.NewGuid().ToString("N").Substring(0, 12);
            }
            // 「先建流再 view.open」（规范顺序），但只等到订阅完成（stream.open 应答，秒级）——
            // 不等 Image 首帧：重进场景服务端按会话记账不重推初始 image，等握手只会白耗超时；
            // 增量订阅此刻已注册，后台握手自愈，首屏数据来自下面的 view.open 应答。
            await stream.EnsureSubscribedAsync(ct).ConfigureAwait(false);

            var request = new ViewOpenRequest { ViewId = ViewId, Filter = filter, PageSize = size, Mode = mode };
            int epoch;
            lock (sync) epoch = snapshots.Begin();
            var data = await requester.RequestAsync<ViewOpenRequest, PriceViewData>(
                GatewayTopics.PriceViewOpen, request, ct).ConfigureAwait(false);

            lock (sync)
            {
                var replay = snapshots.Complete(epoch, data.Sequence);
                StreamId = data.StreamId;
                rowsById.Clear();
                if (data.Books != null)
                {
                    foreach (var b in data.Books)
                    {
                        if (b != null && b.SecurityId != null) rowsById[b.SecurityId] = b;
                    }
                }
                RebuildOrdered();
                TotalCount = data.View != null && data.View.TotalCount.HasValue ? data.View.TotalCount.Value : rowsById.Count;
                firstPageIds = new HashSet<string>(rowsById.Keys);
                filterSignature = signature;
                foreach (var batch in replay) HandleBatch(batch);
            }
            return ordered;
        }

        /// <summary>查询指定页（滚动加载/第一页补位）。</summary>
        /// <summary>第一块行 Id（OpenAsync 时点）；行缓存淘汰时永不触碰。</summary>
        private HashSet<string> firstPageIds = new HashSet<string>();

        /// <summary>最近一轮推送实际变更的报价/券（UI 增量闪烁判定；refill 触发的 Changed 不清空——递补行不算新报价）。</summary>
        private readonly HashSet<string> lastTouchedQuoteIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> lastTouchedSecurityIds = new HashSet<string>(StringComparer.Ordinal);

        public IReadOnlyList<string> SnapshotTouchedQuoteIds()
        {
            lock (sync) return lastTouchedQuoteIds.ToList();
        }

        public IReadOnlyList<string> SnapshotTouchedSecurityIds()
        {
            lock (sync) return lastTouchedSecurityIds.ToList();
        }

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

        public async Task<IReadOnlyList<PriceBookRow>> QueryPageAsync(int pageNumber, CancellationToken ct)
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

        private async Task<IReadOnlyList<PriceBookRow>> QueryPageAsyncCore(int pageNumber, CancellationToken ct)
        {
            if (StreamId == null) throw new InvalidOperationException("view 未打开");
            var request = new ViewQueryRequest { ViewId = ViewId, PageNumber = pageNumber, PageSize = pageSize };
            int epoch;
            lock (sync) epoch = snapshots.Begin();
            var data = await requester.RequestAsync<ViewQueryRequest, PriceViewData>(
                GatewayTopics.PriceViewQuery, request, ct).ConfigureAwait(false);

            lock (sync)
            {
                var replay = snapshots.Complete(epoch, data.Sequence);
                EnforceCacheLimit();
                if (data.Books != null)
                {
                    foreach (var b in data.Books)
                    {
                        if (b != null && b.SecurityId != null) rowsById[b.SecurityId] = b;
                    }
                }
                RebuildOrdered();
                TotalCount = data.View != null && data.View.TotalCount.HasValue ? data.View.TotalCount.Value : TotalCount;
                foreach (var batch in replay) HandleBatch(batch);
            }
            return data.Books ?? new List<PriceBookRow>();
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
                    GatewayTopics.PriceViewClose, new ViewCloseRequest { ViewId = closingView }, CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // 关闭失败不阻断重建；服务端按连接释放
            }
        }

        internal void HandleBatch(PriceStreamBatch batch)
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
                if (removeIds != null)
                {
                    foreach (var id in removeIds)
                    {
                        if (rowsById.Remove(id)) removedFromLoaded = true;
                    }
                }
                if (batch.Books != null && upsertIds != null)
                {
                    // books[] 是同一推送流上「所有视图」的并集（每本书自带 viewId）：必须只认本视图的书。
                    // 否则 FULL 视图会拿到 BEST 视图对同券的截断簿（每侧仅最优档）覆盖完整簿，
                    // 该券条目从 N 条掉到 1-2 条——实测改一笔报价后利率全部 57 行 → 38 行。
                    var byId = batch.Books
                        .Where(b => b != null && b.SecurityId != null
                            && (string.IsNullOrEmpty(b.ViewId) || string.Equals(b.ViewId, ViewId, StringComparison.Ordinal)))
                        .GroupBy(b => b.SecurityId, StringComparer.Ordinal)
                        .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
                    lastTouchedQuoteIds.Clear();
                    lastTouchedSecurityIds.Clear();
                    foreach (var id in upsertIds)
                    {
                        PriceBookRow book;
                        if (!byId.TryGetValue(id, out book)) continue;
                        // 无条件入缓存：新报价（缓存外 securityId）updateTime 最新、排序必落第一页头部；
                        // 第 2 页已加载券的更新照收（数据保真）。缓存总量由 3000 行上限淘汰兜底。
                        // 旧逻辑「缓存外 upsert 跳过」会让新报价永远进不来、缓存只减不增（实测数据越刷越少）
                        PriceBookRow previous;
                        rowsById.TryGetValue(id, out previous);
                        rowsById[id] = book;
                        firstPageChanged = true;
                        lastTouchedSecurityIds.Add(id);
                        if (book.MarketData != null && book.MarketData.NoMDEntries != null)
                        {
                            // 触及集精确化：整券簿推送会带上同券全部条目，未改动的报价只是被重排
                            // （quoteRevision 不变）。逐条比对 revision，只有变更/新增的报价进触及集——
                            // 否则改 1 笔整券 N 笔全闪（探针实证：改后其他条目时间/版本均原样）
                            foreach (var entry in book.MarketData.NoMDEntries)
                            {
                                if (entry == null || entry.MDEntryID == null) continue;
                                QuoteRef quoteRef = null;
                                var hasRef = book.QuoteRefs != null && book.QuoteRefs.TryGetValue(entry.MDEntryID, out quoteRef);
                                var quoteId = hasRef && quoteRef != null && quoteRef.QuoteId != null
                                    ? quoteRef.QuoteId
                                    : entry.MDEntryID.StartsWith("FICC:", StringComparison.Ordinal) ? entry.MDEntryID.Substring(5) : entry.MDEntryID;
                                if (quoteId == null) continue;

                                QuoteRef prevRef = null;
                                var hadPrev = previous != null && previous.QuoteRefs != null
                                    && previous.QuoteRefs.TryGetValue(entry.MDEntryID, out prevRef);
                                var newRevision = quoteRef != null ? quoteRef.QuoteRevision : null;
                                var oldRevision = prevRef != null ? prevRef.QuoteRevision : null;
                                var changed = !hadPrev
                                    || !string.Equals(newRevision ?? string.Empty, oldRevision ?? string.Empty, StringComparison.Ordinal);
                                if (changed) lastTouchedQuoteIds.Add(quoteId);
                            }
                        }
                    }
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

        /// <summary>updateTime DESC + securityId ASC（ordinal）重建有序快照。</summary>
        private void RebuildOrdered()
        {
            ordered = rowsById.Values
                .OrderByDescending(b => b.UpdateTime ?? DateTime.MinValue)
                .ThenBy(b => b.SecurityId, StringComparer.Ordinal)
                .ToList();
        }

        public string StreamId { get; private set; }

        private static string BuildSignature(QuoteFilter filter, string mode, int size)
        {
            var payload = new ViewOpenRequest { Filter = filter, Mode = mode, PageSize = size };
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
            _ = CloseAsync();   // 释放服务端视图
        }
    }
}
