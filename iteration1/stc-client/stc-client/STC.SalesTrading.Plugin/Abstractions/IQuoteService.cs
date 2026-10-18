using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>
    /// 报价录入域服务（数据接缝）。
    /// 本接口只锁 UI 层需要的语义，不锁传输协议；
    /// 网关实现见 Gateway/GatewayQuoteService，ViewModel 零改动。
    /// </summary>
    public interface IQuoteService
    {
        /// <summary>
        /// 服务端报价集合发生变化（推送到达/断线复位）。UI 在无用户选中态时据此重查；
        /// 回调线程不定，UI 层需自行调度。
        /// </summary>
        event Action QuotesChanged;

        /// <summary>拉取待确认报价行（原型：GET /quote-confirm/batches?status=pending，客户端按 批次|机构|来源 分组，2.5s 轮询增量）。</summary>
        Task<IReadOnlyList<QuoteRow>> LoadPendingRowsAsync(CancellationToken ct);

        /// <summary>按 Id 拉取完整报价（编辑弹窗先拉全量再预填，§3.1.4 专项 1）。</summary>
        Task<QuoteRow> GetQuoteAsync(string quoteId, CancellationToken ct);

        /// <summary>报价变更历史（ficc.quote.history.query，H10；按 revision 升序，看板行右键"报价日志"）。</summary>
        Task<IReadOnlyList<QuoteLogEntry>> LoadQuoteLogAsync(string quoteId, CancellationToken ct);

        /// <summary>精确报价文本识别（原型：POST /api/parse → 结构化行 + 估值回填）。</summary>
        Task<QuoteParseResult> ParsePreciseTextAsync(string text, CancellationToken ct);

        /// <summary>模糊报价按条件查询候选债券（原型：POST /api/bond-filter/query）。</summary>
        Task<IReadOnlyList<FuzzyCandidate>> QueryFuzzyCandidatesAsync(FuzzyQuery query, CancellationToken ct);

        /// <summary>
        /// 确认报价（ficc.quote.confirm，"确认并修正"）：行上的编辑值与 CRM Id 随确认一起提交，
        /// 带 expectedRevision 乐观锁；未匹配 CRM 的行由 UI 前置拦截（UNRESOLVED_COUNTERPARTY 语义）。
        /// </summary>
        Task ConfirmQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct);

        /// <summary>提交精确识别页产生的新报价行（create，自动 PENDING_CONFIRM）。</summary>
        Task SubmitQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct);

        /// <summary>
        /// 修改报价（ficc.quote.update，带 expectedRevision 乐观锁）。
        /// 行上 QuoteId/Revision 缺失时由实现抛出，不静默转创建。
        /// </summary>
        Task UpdateQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct);

        /// <summary>删除待确认报价（ficc.quote.delete；expectedRevision 必填，故传行而非 Id）。</summary>
        Task DeleteQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct);

        /// <summary>将模糊识别结果转为模糊报价（原型 0910 由「意向报价」改名）。</summary>
        Task SubmitIntentQuotesAsync(IReadOnlyCollection<BoardIntentRow> rows, CancellationToken ct);
    }
}
