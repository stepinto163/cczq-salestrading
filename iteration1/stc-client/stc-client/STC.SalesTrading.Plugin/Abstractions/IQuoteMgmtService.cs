using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>
    /// 报价管理域服务（数据接缝）。今日报价 + 历史报价（按日终分组语义由上层实现）。
    /// </summary>
    public interface IQuoteMgmtService
    {
        /// <summary>
        /// 今日报价集合发生变化（订阅推送到达）。今日页据此重查；
        /// 回调线程不定，UI 层需自行调度。
        /// </summary>
        event Action QuotesChanged;

        /// <summary>加载今日报价（原型：mgmtToday 全量 + 增量）。</summary>
        Task<IReadOnlyList<QuoteRow>> LoadTodayRowsAsync(CancellationToken ct);

        /// <summary>加载历史报价（原型：mgmtHistory，按 eod-YYYY-MM-DD 日终分组）。</summary>
        Task<IReadOnlyList<QuoteRow>> LoadHistoryRowsAsync(DateTime start, DateTime end, CancellationToken ct);

        /// <summary>按服务端查询语义加载今日报价，返回当前页和服务端总数。</summary>
        Task<PagedResult<QuoteRow>> QueryTodayRowsAsync(QuoteMgmtQuery query, CancellationToken ct);

        /// <summary>按服务端查询语义加载历史报价，返回当前页和服务端总数。</summary>
        Task<PagedResult<QuoteRow>> QueryHistoryRowsAsync(QuoteMgmtQuery query, CancellationToken ct);

        /// <summary>删除今日或历史报价（ficc.quote.delete；expectedRevision 必填，故传行而非 Id）。</summary>
        Task DeleteQuotesAsync(IReadOnlyCollection<QuoteRow> rows, bool history, CancellationToken ct);

        /// <summary>Refer 勾选行（ficc.quote.refer，H04；仅 ACTIVE 可撤销，带 expectedRevision）。</summary>
        Task ReferQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct);

        /// <summary>Unrefer 勾选行（ficc.quote.unrefer，H05；仅 REFERRED 可恢复）。</summary>
        Task UnreferQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct);
    }
}
