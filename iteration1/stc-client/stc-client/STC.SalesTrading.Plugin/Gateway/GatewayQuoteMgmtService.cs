using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;
using STC.SalesTrading.Plugin.Gateway.Contracts.Base;
using STC.SalesTrading.Plugin.Gateway.Contracts.Quote;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// 报价管理域服务（IQuoteMgmtService 网关实现，批次 2：查询路径）。
    /// 今日页走 quote 域 LiveView（ownerScope=VISIBLE，推送自动维护行集合）；
    /// 历史页走 ficc.quote.history.search（游标续取）；删除走 ficc.quote.delete 批量逐条校验。
    /// 债券资料（reference）依赖后端补齐（决策 #9）；补齐前用 EnsureReferenceAsync 过渡补取（决策 #10），
    /// 后端补齐后注释 QueryAsync 末尾那一行调用即可。
    /// </summary>
    public sealed class GatewayQuoteMgmtService : IQuoteMgmtService, IDisposable
    {
        private readonly GatewayRequester requester;
        private readonly GatewayReportStream<Contracts.View.QuoteStreamBatch> stream;
        private readonly QuoteLiveView todayView;

        /// <summary>今日页推送刷新事件（LiveView 行集合变化直接转发）。</summary>
        public event Action QuotesChanged
        {
            add { todayView.Changed += value; }
            remove { todayView.Changed -= value; }
        }

        internal GatewayQuoteMgmtService(GatewayRequester requester, GatewayReportStream<Contracts.View.QuoteStreamBatch> stream)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            todayView = new QuoteLiveView(requester, stream);
            stream.Failed += reason => STC.Common.Utils.Events.EventAggregator.Publish(
                new STC.SalesTrading.Plugin.Shared.Messages.DataSourceResetMessage());
        }

        public async Task<IReadOnlyList<QuoteRow>> LoadTodayRowsAsync(CancellationToken ct)
        {
            var result = await QueryTodayRowsAsync(new QuoteMgmtQuery { Today = true }, ct).ConfigureAwait(false);
            return result.Items;
        }

        /// <summary>
        /// 今日页查询：状态、权限范围和组合筛选统一下推服务端，避免分页后过滤造成漏行。
        /// </summary>
        public async Task<PagedResult<QuoteRow>> QueryTodayRowsAsync(QuoteMgmtQuery query, CancellationToken ct)
        {
            var status = ParseStatusFilter(query.Status);
            var viewRows = await todayView.OpenAsync(ToFilter(query), status, "VISIBLE", query.PageSize, ct).ConfigureAwait(false);
            var page = Math.Max(1, query.Page);
            if (query.ForceRefresh || page > 1)
            {
                // query 返回当前页，不对返回页再次 Skip；Open 缓存则包含已加载页。
                var pageRows = await todayView.QueryPageAsync(page, ct).ConfigureAwait(false);
                // 快照请求期间可能已收到更高版本推送；按本页 ID 读取回放后的最新缓存。
                var latestRows = todayView.Rows.ToDictionary(row => row.QuoteId);
                viewRows = pageRows.Where(row => latestRows.ContainsKey(row.QuoteId))
                    .Select(row => latestRows[row.QuoteId]).ToList();
            }
            var reference = todayView.LastReference;
            IEnumerable<QuoteRecord> source = viewRows;

            // 债券资料优先取视图/推送 reference（决策 #9 已落地），映射为 UI 行
            var mapped = source
                .Select(r => GatewayModelMapper.ToQuoteRow(r, RefAt(reference, r.SecurityId)))
                .ToList();

            // 缺失部分 base.get 兜底（决策 #10；视图/推送已带的不会再请求）
            await GatewayReferenceFill.EnsureAsync(requester, mapped, ct).ConfigureAwait(false);

            var items = mapped
                .Take(query.PageSize)
                .ToList();
            return new PagedResult<QuoteRow>
            {
                Items = items,
                TotalCount = todayView.TotalCount,
                QueryId = todayView.ViewId,
                IsPartial = false,
            };
        }

        public Task<PagedResult<QuoteRow>> QueryHistoryRowsAsync(QuoteMgmtQuery query, CancellationToken ct)
        {
            return QueryHistoryCoreAsync(query, null, ct);
        }

        /// <summary>按日期区间加载历史行（MgmtHistory 分组语义的上层兼容入口）。</summary>
        public Task<IReadOnlyList<QuoteRow>> LoadHistoryRowsAsync(DateTime start, DateTime end, CancellationToken ct)
        {
            return QueryHistoryCoreAsync(new QuoteMgmtQuery
            {
                StartDate = start,
                EndDate = end,
            }, null, ct).ContinueWith(t => (IReadOnlyList<QuoteRow>)t.Result.Items, ct);
        }

        /// <summary>
        /// 历史查询（ficc.quote.history.search）。nextCursor 经 PagedResult.QueryId 回传，
        /// VM 置入 QuoteMgmtQuery.Cursor 续取（游标绑定条件快照，条件变更必须置 null 重查）；
        /// H11 应答自带 reference，估值列直出不依赖决策 #9。
        /// </summary>
        private async Task<PagedResult<QuoteRow>> QueryHistoryCoreAsync(QuoteMgmtQuery query, string cursor, CancellationToken ct)
        {
            var request = new QuoteHistorySearchRequest
            {
                Filter = ToFilter(query),
                FromDate = (query.StartDate ?? DateTime.Today.AddDays(-30)).ToString("yyyy-MM-dd"),
                ToDate = (query.EndDate ?? DateTime.Today).ToString("yyyy-MM-dd"),
                Limit = query.PageSize,
                Cursor = cursor ?? query.Cursor,
            };
            var data = await requester.RequestAsync<QuoteHistorySearchRequest, QuoteHistoryData>(
                GatewayTopics.QuoteHistorySearch, request, ct).ConfigureAwait(false);

            var rows = (data.Rows ?? new List<QuoteHistoryRecord>())
                .Select(r => GatewayModelMapper.ToQuoteRow(r != null ? r.Quote : null, null))
                .Where(r => r != null)
                .ToList();

            // H11 应答自带 reference：按 securityId 回填（估值直出）
            if (data.Reference != null)
            {
                foreach (var row in rows)
                {
                    QuoteReference reference;
                    if (row.SecurityId != null && data.Reference.TryGetValue(row.SecurityId, out reference))
                    {
                        ApplyReference(row, reference);
                    }
                }
            }
            await GatewayReferenceFill.EnsureAsync(requester, rows, ct).ConfigureAwait(false);

            // 文本搜索（第一期客户端过滤，同今日页）
            var bond = (query.BondSearchText ?? string.Empty).Trim();
            var inst = (query.InstitutionSearchText ?? string.Empty).Trim();
            if (bond.Length > 0)
            {
                rows = rows.Where(r =>
                    (r.SecurityId ?? string.Empty).IndexOf(bond, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.SecurityName ?? string.Empty).IndexOf(bond, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (inst.Length > 0)
            {
                rows = rows.Where(r =>
                    (r.InstitutionName ?? string.Empty).IndexOf(inst, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.ContactName ?? string.Empty).IndexOf(inst, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            return new PagedResult<QuoteRow>
            {
                Items = rows,
                TotalCount = data.TotalCount ?? rows.Count,
                QueryId = data.NextCursor,
                IsPartial = false,
            };
        }

        /// <summary>H11 reference → 行（复用 EnsureAsync 的字段映射）。</summary>
        private static void ApplyReference(QuoteRow row, QuoteReference reference)
        {
            if (row.SecurityName == null) row.SecurityName = reference.SecurityName;
            if (row.Maturity == null) row.Maturity = reference.RemainingYears;
            if (row.BondLibrary == null) row.BondLibrary = reference.BondSubtype;
            if (row.IssuerRating == null) row.IssuerRating = reference.IssuerRating;
            if (row.BondRating == null) row.BondRating = reference.BondRating;
            row.NzYtm = row.NzYtm ?? GatewayModelMapper.ParseDecimal(reference.ValuationYield);
            row.NzNetPrice = row.NzNetPrice ?? GatewayModelMapper.ParseDecimal(reference.ValuationCleanPrice);
            row.CsYtm = row.CsYtm ?? GatewayModelMapper.ParseDecimal(reference.CsiValuationYield);
            row.CsNetPrice = row.CsNetPrice ?? GatewayModelMapper.ParseDecimal(reference.CsiValuationCleanPrice);
        }

        /// <summary>删除报价（ficc.quote.delete，H06）：expectedRevision 必填，传行集合。</summary>
        public Task DeleteQuotesAsync(IReadOnlyCollection<QuoteRow> rows, bool history, CancellationToken ct)
        {
            if (rows == null || rows.Count == 0) return Task.CompletedTask;
            return ExecuteLifecycleAsync(GatewayTopics.QuoteDelete, ToLifecycleRequest(rows), ct);
        }

        /// <summary>Refer（ficc.quote.refer，H04）：仅 ACTIVE 可撤销，带 expectedRevision。</summary>
        public Task ReferQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct)
        {
            return ExecuteLifecycleAsync(GatewayTopics.QuoteRefer, ToLifecycleRequest(rows), ct);
        }

        /// <summary>Unrefer（ficc.quote.unrefer，H05）：仅 REFERRED 可恢复。</summary>
        public Task UnreferQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct)
        {
            return ExecuteLifecycleAsync(GatewayTopics.QuoteUnrefer, ToLifecycleRequest(rows), ct);
        }

        private static QuoteLifecycleRequest ToLifecycleRequest(IReadOnlyCollection<QuoteRow> rows)
        {
            if (rows == null || rows.Count == 0) return null;
            return new QuoteLifecycleRequest
            {
                Quotes = rows.Select(r => new QuoteLifecycleItem
                {
                    QuoteId = r.QuoteId,
                    ExpectedRevision = r.Revision,
                    Reason = "用户操作",
                }).ToList(),
            };
        }

        private static QuoteReference RefAt(IReadOnlyDictionary<string, QuoteReference> reference, string securityId)
        {
            QuoteReference value;
            return securityId != null && reference != null && reference.TryGetValue(securityId, out value)
                ? value : null;
        }

        /// <summary>生命周期写操作公共路径：requestId 由 GatewayRequester 注入，批量结果逐条校验。</summary>
        private async Task ExecuteLifecycleAsync(string topic, QuoteLifecycleRequest request, CancellationToken ct)
        {
            if (request == null) return;
            var data = await requester.RequestAsync<QuoteLifecycleRequest, QuoteWriteData>(topic, request, ct).ConfigureAwait(false);
            var failures = CollectFailures(data);
            if (failures.Count > 0)
            {
                throw new GatewayApiException("gateway", string.Join("；", failures));
            }
        }

        private static List<string> CollectFailures(QuoteWriteData data)
        {
            return (data.Results ?? new List<QuoteOperationResult>())
                .Where(r => r != null && r.Code != "0000")
                .Select(r => (r.QuoteId ?? "?") + "：" + (r.Msg ?? r.Code))
                .ToList();
        }

        /// <summary>状态筛选映射："全部"=ACTIVE+REFERRED；今日视图不出现 PENDING_CONFIRM/DELETED。</summary>
        private static List<string> ParseStatusFilter(string status)
        {
            if (string.IsNullOrEmpty(status) || status == "全部") return new List<string> { "ACTIVE", "REFERRED" };
            if (status == "有效" || status == "ACTIVE") return new List<string> { "ACTIVE" };
            if (status == "撤销" || status == "已Refer" || status == "REFERRED") return new List<string> { "REFERRED" };
            return new List<string> { "ACTIVE", "REFERRED" };
        }

        /// <summary>查询语义 → 后端 filter，今日和历史页共用。</summary>
        private static QuoteFilter ToFilter(QuoteMgmtQuery query)
        {
            var credit = Shared.Services.MarketGroupSelectionService.ActiveGroupIds(Shared.Services.FilterProfileService.MarketKind.Credit);
            var rate = Shared.Services.MarketGroupSelectionService.ActiveGroupIds(Shared.Services.FilterProfileService.MarketKind.Rate);
            return new QuoteFilter
            {
                Conditions = BuildConditions(query),
                WatchlistIds = credit.favorites.Concat(rate.favorites).ToList(),
                FilterGroupIds = credit.filters.Concat(rate.filters).ToList(),
            };
        }

        private static List<FilterCondition> BuildConditions(QuoteMgmtQuery query)
        {
            var conditions = (query.ProductFilter?.Conditions ?? new List<FilterConditionPayload>())
                .Select(condition => new FilterCondition
                {
                    Field = condition.Field,
                    Operator = condition.Operator,
                    Value = condition.Value
                }).ToList();
            if (!string.IsNullOrWhiteSpace(query.SearchText))
                conditions.Add(new FilterCondition { Field = "searchText", Operator = "CONTAINS", Value = query.SearchText.Trim() });
            if (!string.IsNullOrWhiteSpace(query.BondSearchText))
                conditions.Add(new FilterCondition { Field = "searchText", Operator = "CONTAINS", Value = query.BondSearchText.Trim() });
            if (!string.IsNullOrWhiteSpace(query.InstitutionSearchText))
                conditions.Add(new FilterCondition { Field = "institutionName", Operator = "CONTAINS", Value = query.InstitutionSearchText.Trim() });
            return conditions;
        }

        public void Dispose()
        {
            todayView.Dispose();
            stream.Dispose();   // 与 Quote 共享，Dispose 幂等
        }
    }
}
