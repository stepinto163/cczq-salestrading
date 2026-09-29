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
                    // Retain successful IDs even when another row in the batch failed.
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
                    // A previous submit may have confirmed this row before a later row failed.
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
