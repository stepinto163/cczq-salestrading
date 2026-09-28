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
        /// <summary>加载今日报价（原型：mgmtToday 全量 + 增量）。</summary>
        Task<IReadOnlyList<MgmtQuoteRow>> LoadTodayRowsAsync(CancellationToken ct);

        /// <summary>加载历史报价（原型：mgmtHistory，按 eod-YYYY-MM-DD 日终分组）。</summary>
        Task<IReadOnlyList<MgmtQuoteRow>> LoadHistoryRowsAsync(DateTime start, DateTime end, CancellationToken ct);

        /// <summary>按服务端查询语义加载今日报价，返回当前页和服务端总数。</summary>
        Task<PagedResult<MgmtQuoteRow>> QueryTodayRowsAsync(QuoteMgmtQuery query, CancellationToken ct);

        /// <summary>按服务端查询语义加载历史报价，返回当前页和服务端总数。</summary>
        Task<PagedResult<MgmtQuoteRow>> QueryHistoryRowsAsync(QuoteMgmtQuery query, CancellationToken ct);

        /// <summary>删除今日或历史报价；Mock 阶段用于验证刷新后一致性。</summary>
        Task DeleteQuotesAsync(IReadOnlyCollection<string> quoteIds, bool history, CancellationToken ct);
    }
}
