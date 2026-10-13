using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;

namespace STC.SalesTrading.Plugin.Ficc
{
    public sealed partial class DpsSalesTradingServices
    {
        private static JObject EmptyFilter()
        {
            return new JObject { ["conditions"] = new JArray() };
        }

        private static void AddCondition(JObject filter, string field, string operation, JToken value)
        {
            ((JArray)filter["conditions"]).Add(new JObject { ["field"] = field, ["operator"] = operation, ["value"] = value });
        }

        private static JObject ManagementFilter(QuoteMgmtQuery query)
        {
            if (!string.IsNullOrEmpty(query.Mode) && query.Mode != "全部")
            {
                throw new NotSupportedException("当前QuoteSvr尚未提供报价来源筛选接口。");
            }
            if (!string.IsNullOrWhiteSpace(query.TraderSearchText))
            {
                throw new NotSupportedException("当前QuoteSvr尚未提供长城交易员专属筛选接口。");
            }
            JObject filter = query.ProductFilter?.ToJson() ?? EmptyFilter();
            foreach (string text in new[] { query.BondSearchText, query.InstitutionSearchText })
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    AddCondition(filter, "searchText", "CONTAINS", text.Trim());
                }
            }
            return filter;
        }

        public async Task<IReadOnlyList<QuoteConfirmRow>> LoadPendingRowsAsync(CancellationToken ct)
        {
            LivePage page = await QueryAsync("pending", "quote", EmptyFilter(), ct);
            return page.Rows.Select(row => DpsModelMapper.Display(WithReference(row)).ToObject<QuoteConfirmRow>()).ToArray();
        }

        public Task<QuoteParseResult> ParsePreciseTextAsync(string text, CancellationToken ct)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(DpsSalesTradingServices));
            }
            return new StcQuoteParserClient(transport, UserName).ParseAsync(text, ct);
        }

        public Task<IReadOnlyList<FuzzyCandidate>> QueryFuzzyCandidatesAsync(FuzzyQuery query, CancellationToken ct)
        {
            throw new NotSupportedException("模糊报价服务尚未接入。");
        }

        public Task SubmitIntentQuotesAsync(IReadOnlyCollection<BoardIntentRow> rows, CancellationToken ct)
        {
            throw new NotSupportedException("模糊报价服务尚未接入，未提交任何记录。");
        }

        public Task SubmitQuotesAsync(IReadOnlyCollection<QuoteConfirmRow> rows, CancellationToken ct)
        {
            return new ParsedQuoteSubmitter(Api).SubmitAsync(rows, ct);
        }

        public async Task ConfirmRowsAsync(IReadOnlyCollection<QuoteConfirmRow> rows, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var quotes = new List<QuoteChangeItem>();
            foreach (QuoteConfirmRow row in rows)
            {
                QuoteChangeItem input = ProtocolCodec.Decode<QuoteChangeItem>(DpsModelMapper.Input(row));
                input.QuoteId = row.RecognitionId;
                input.ExpectedRevision = row.Revision.ToString(System.Globalization.CultureInfo.InvariantCulture);
                quotes.Add(input);
            }
            FiccApiClient.RequireQuoteResults(await Api.ChangeQuotesAsync(SalesTradingApi.QuoteConfirm, quotes));
        }

        public async Task DeletePendingAsync(IEnumerable<QuoteConfirmRow> rows, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            JObject[] quotes = rows.Select(row => new JObject
            {
                ["quoteId"] = row.RecognitionId,
                ["revision"] = row.Revision
            }).ToArray();
            FiccApiClient.RequireQuoteResults(await Api.OperateAsync("delete", quotes));
        }

        public Task ConfirmQuotesAsync(IReadOnlyCollection<string> recognitionIds, CancellationToken ct)
        {
            return OperateDisplayedAsync("confirm", recognitionIds, ct);
        }

        public Task DeleteQuotesAsync(IReadOnlyCollection<string> recognitionIds, CancellationToken ct)
        {
            return OperateDisplayedAsync("delete", recognitionIds, ct);
        }

        public Task DeleteQuotesAsync(IReadOnlyCollection<string> quoteIds, bool history, CancellationToken ct)
        {
            if (history)
            {
                throw new InvalidOperationException("历史报价为审计记录，不允许删除。");
            }
            return OperateDisplayedAsync("delete", quoteIds, ct);
        }

        private async Task OperateDisplayedAsync(string operation, IEnumerable<string> ids, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            JObject[] quotes = ids.Select(id => displayedQuotes.TryGetValue(id, out JObject quote) ? quote
                : throw new InvalidOperationException("请先读取报价：" + id)).ToArray();
            FiccApiClient.RequireQuoteResults(await Api.OperateAsync(operation, quotes));
        }

        public async Task OperateAsync(string operation, IEnumerable<MgmtQuoteRow> rows, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            JObject[] quotes = rows.Select(row => new JObject { ["quoteId"] = row.QuoteId, ["revision"] = row.Revision }).ToArray();
            FiccApiClient.RequireQuoteResults(await Api.OperateAsync(operation, quotes));
        }

        public async Task<PagedResult<MgmtQuoteRow>> QueryTodayRowsAsync(QuoteMgmtQuery query, CancellationToken ct)
        {
            query = query ?? new QuoteMgmtQuery();
            JArray statuses = query.Status == "有效" ? new JArray("ACTIVE") : query.Status == "撤销" ? new JArray("REFERRED")
                : new JArray("ACTIVE", "REFERRED");
            LivePage page = await QueryAsync("management", "quote", ManagementFilter(query), ct, query.Page, query.PageSize, statuses: statuses);
            long mappingStarted = ClientPerformanceTrace.Start();
            MgmtQuoteRow[] mappedRows = page.Rows.Select(row => DpsModelMapper.Display(WithReference(row)).ToObject<MgmtQuoteRow>()).ToArray();
            ClientPerformanceTrace.End("today.mapping", mappingStarted);
            return new PagedResult<MgmtQuoteRow>
            {
                Items = mappedRows,
                TotalCount = page.TotalCount, QueryId = page.Topic, IsPartial = page.TotalCount > page.Rows.Count
            };
        }

        public async Task<IReadOnlyList<MgmtQuoteRow>> LoadTodayRowsAsync(CancellationToken ct)
        {
            return (await QueryTodayRowsAsync(new QuoteMgmtQuery(), ct)).Items;
        }

        public async Task<PagedResult<MgmtQuoteRow>> QueryHistoryRowsAsync(QuoteMgmtQuery query, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            await Shared.Services.FilterProfileService.EnsureLoadedAsync();
            QuoteHistorySearchData result = await Api.RequestAsync(SalesTradingApi.QuoteHistorySearch, new QuoteHistorySearchRequest
            {
                FromDate = (query.StartDate ?? DateTime.Today.AddDays(-7)).ToString("yyyy-MM-dd"),
                ToDate = (query.EndDate ?? DateTime.Today).ToString("yyyy-MM-dd"),
                Limit = Math.Max(1, Math.Min(query.PageSize, 100)),
                Filter = ProtocolCodec.Decode<Filter>(Shared.Services.MarketGroupSelectionService.ApplyTo(ManagementFilter(query), null))
            });
            ct.ThrowIfCancellationRequested();
            long mappingStarted = ClientPerformanceTrace.Start();
            List<MgmtQuoteRow> rows = new List<MgmtQuoteRow>();
            foreach (QuoteHistoryRow item in result.Rows)
            {
                JObject quote = item.Quote.ToJson();
                if (item.Reference != null)
                {
                    securities[item.Quote.SecurityId] = item.Reference.ToJson();
                }
                rows.Add(DpsModelMapper.Display(WithReference(quote)).ToObject<MgmtQuoteRow>());
            }
            ClientPerformanceTrace.End("history.mapping", mappingStarted);
            return new PagedResult<MgmtQuoteRow>
            {
                Items = rows, TotalCount = result.TotalCount ?? rows.Count,
                IsPartial = result.HasMore ?? result.NextCursor != null
            };
        }

        public async Task<IReadOnlyList<MgmtQuoteRow>> LoadHistoryRowsAsync(DateTime start, DateTime end, CancellationToken ct)
        {
            return (await QueryHistoryRowsAsync(new QuoteMgmtQuery { StartDate = start, EndDate = end }, ct)).Items;
        }
    }
}
