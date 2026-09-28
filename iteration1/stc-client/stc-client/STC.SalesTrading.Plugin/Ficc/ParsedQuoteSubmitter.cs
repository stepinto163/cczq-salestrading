using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
                JObject[] inputs = drafts.Select(row =>
                {
                    JObject input = DpsModelMapper.Input(row);
                    input["clientQuoteId"] = row.ClientQuoteId;
                    return input;
                }).ToArray();
                JObject response = await api.CreateAsync(inputs);
                JArray results = response["results"] as JArray;
                if (results != null)
                {
                    // Retain successful IDs even when another row in the batch failed.
                    foreach (JObject result in results.OfType<JObject>())
                    {
                        QuoteConfirmRow row = drafts.FirstOrDefault(candidate =>
                            candidate.ClientQuoteId == (string)result["clientQuoteId"]);
                        if (row != null && (string)result["Code"] == "0000"
                            && !string.IsNullOrWhiteSpace((string)result["quoteId"]))
                        {
                            row.RecognitionId = (string)result["quoteId"];
                            row.Revision = (long?)result["revision"] ?? 1;
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
                JObject quote = await api.GetQuoteAsync(row.RecognitionId);
                if (quote == null)
                {
                    throw new InvalidOperationException("无法读取已创建报价：" + row.RecognitionId);
                }
                row.Revision = (long)quote["revision"];
                string status = (string)quote["status"];
                if (status == "ACTIVE")
                {
                    // A previous submit may have confirmed this row before a later row failed.
                    continue;
                }
                if (status != "PENDING_CONFIRM")
                {
                    throw new InvalidOperationException("报价状态已改变，请在报价管理中处理：" + row.RecognitionId);
                }
                JObject response = await api.OperateAsync("confirm", new[] { quote }, DpsModelMapper.Input(row));
                FiccApiClient.RequireQuoteResults(response);
            }
        }
    }
}
