using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>
    /// 报价录入域服务（数据接缝）。
    /// 后端接口与出参未定，本接口只锁 UI 层需要的语义，不锁传输协议；
    /// 开发期由 Mock 实现，接网关时新增 Gateway 实现替换，ViewModel 零改动。
    /// </summary>
    public interface IQuoteService
    {
        /// <summary>拉取待确认报价行（原型：GET /quote-confirm/batches?status=pending，客户端按 批次|机构|来源 分组，2.5s 轮询增量）。</summary>
        Task<IReadOnlyList<QuoteConfirmRow>> LoadPendingRowsAsync(CancellationToken ct);

        /// <summary>精确报价文本识别（原型：POST /api/parse → 结构化行 + 估值回填）。</summary>
        Task<QuoteParseResult> ParsePreciseTextAsync(string text, CancellationToken ct);

        /// <summary>模糊报价按条件查询候选债券（原型：POST /api/bond-filter/query）。</summary>
        Task<IReadOnlyList<FuzzyCandidate>> QueryFuzzyCandidatesAsync(FuzzyQuery query, CancellationToken ct);

        /// <summary>确认报价（原型：POST /quote-confirm/confirm，带版本号乐观锁）。</summary>
        Task ConfirmQuotesAsync(IReadOnlyCollection<string> recognitionIds, CancellationToken ct);

        /// <summary>提交精确识别页产生的新报价行。</summary>
        Task SubmitQuotesAsync(IReadOnlyCollection<QuoteConfirmRow> rows, CancellationToken ct);

        /// <summary>删除待确认报价（原型：POST /quote-confirm/delete）。</summary>
        Task DeleteQuotesAsync(IReadOnlyCollection<string> recognitionIds, CancellationToken ct);

        /// <summary>将模糊识别结果转为模糊报价（原型 0910 由「意向报价」改名）。</summary>
        Task SubmitIntentQuotesAsync(IReadOnlyCollection<BoardIntentRow> rows, CancellationToken ct);
    }
}
