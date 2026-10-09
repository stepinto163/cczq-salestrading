using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Ficc
{
    /// <summary>
    /// 将识别预览提交为正式报价：先创建 Revision=0 的草稿，再读取服务器最新版本并逐条确认。
    /// 部分成功时保留服务器 quoteId/revision，后续重试复用已有记录，避免再次创建成功行。
    /// ACTIVE 表示此前已确认，可跳过；其他非待确认状态必须报错，不能擅自恢复或覆盖。
    /// </summary>
    public sealed class ParsedQuoteSubmitter
    {
        private readonly FiccApiClient api;

        public ParsedQuoteSubmitter(FiccApiClient api)
        {
            this.api = api;
        }

        public async Task SubmitAsync(IReadOnlyCollection<QuoteConfirmRow> rows, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            QuoteConfirmRow[] drafts = rows.Where(row => row.Revision == 0).ToArray();
            if (drafts.Length > 0)
            {
                QuoteCreateItem[] inputs = drafts.Select(row =>
                {
                    QuoteCreateItem input = ProtocolCodec.Decode<QuoteCreateItem>(DpsModelMapper.Input(row));
                    input.ClientQuoteId = row.ClientQuoteId;
                    return input;
                }).ToArray();
                QuoteWriteData response = await api.CreateQuotesAsync(inputs);
                List<QuoteOperationResult> results = response.Results;
                if (results != null)
                {
                    // 即使同批其他行失败，也先保存成功行的 ID；再抛异常才不会丢掉已入库结果。
                    foreach (QuoteOperationResult result in results)
                    {
                        QuoteConfirmRow row = drafts.FirstOrDefault(candidate =>
                            candidate.ClientQuoteId == result.ClientQuoteId);
                        if (row != null && result.Code == "0000"
                            && !string.IsNullOrWhiteSpace(result.QuoteId))
                        {
                            row.RecognitionId = result.QuoteId;
                            row.Revision = result.Revision == null ? 1 : long.Parse(result.Revision, System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }
                FiccApiClient.RequireQuoteResults(response);
                if (drafts.Any(row => row.Revision == 0))
                {
                    throw new InvalidOperationException("创建应答缺少报价 ID；请在报价确认页核查，勿重复提交。");
                }
            }

            foreach (QuoteConfirmRow row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                QuoteRecord quote = await api.GetQuoteModelAsync(row.RecognitionId);
                if (quote == null)
                {
                    throw new InvalidOperationException("无法读取已创建报价：" + row.RecognitionId);
                }
                row.Revision = long.Parse(quote.Revision, System.Globalization.CultureInfo.InvariantCulture);
                string status = quote.Status;
                if (status == "ACTIVE")
                {
                    // 前一次提交可能已确认本行，只是在后续行失败；再次提交不重复确认它。
                    continue;
                }
                if (status != "PENDING_CONFIRM")
                {
                    throw new InvalidOperationException("报价状态已改变，请在报价管理中处理：" + row.RecognitionId);
                }
                QuoteChangeItem changes = ProtocolCodec.Decode<QuoteChangeItem>(DpsModelMapper.Input(row));
                changes.QuoteId = quote.QuoteId;
                changes.ExpectedRevision = quote.Revision;
                QuoteWriteData response = await api.ChangeQuotesAsync(SalesTradingApi.QuoteConfirm, new[] { changes });
                FiccApiClient.RequireQuoteResults(response);
            }
        }
    }
}
