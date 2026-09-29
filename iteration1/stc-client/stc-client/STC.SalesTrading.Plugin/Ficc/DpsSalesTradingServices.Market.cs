using STC.SalesTrading.Plugin.Ficc.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;

namespace STC.SalesTrading.Plugin.Ficc
{
    public sealed partial class DpsSalesTradingServices
    {
        private static JObject MarketFilter(MarketBoardQuery query)
        {
            if (query.MineOnly || query.UnquotedOnly)
            {
                throw new NotSupportedException("当前Price接口尚未提供仅本人／未报价债券筛选。");
            }
            JObject filter = EmptyFilter();
            filter["market"] = query.Market == BondMarket.Credit ? "CREDIT" : "RATE";
            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                AddCondition(filter, "searchText", "CONTAINS", query.SearchText.Trim());
            }
            if (query.IncludeBid != query.IncludeOfr)
            {
                AddCondition(filter, "side", "EQ", query.IncludeBid ? "1" : "2");
            }
            if (!query.IncludeBid && !query.IncludeOfr)
            {
                throw new InvalidOperationException("请至少选择一个报价方向。");
            }
            var flags = new Dictionary<QuoteFlag, string>
            {
                [QuoteFlag.Urgent] = "urgent", [QuoteFlag.Recommend] = "recommended", [QuoteFlag.Whole] = "whole",
                [QuoteFlag.Oco] = "oco", [QuoteFlag.Packing] = "packing"
            };
            foreach (var flag in flags.Where(flag => query.RequiredFlags.HasFlag(flag.Key)))
            {
                AddCondition(filter, flag.Value, "EQ", true);
            }
            return filter;
        }

        private async Task<LivePage> MarketPageAsync(MarketBoardQuery query, string mode, CancellationToken ct)
        {
            string market = query.Market == BondMarket.Credit ? "CREDIT" : "RATE";
            return await QueryAsync(market + mode, "price", MarketFilter(query), ct, query.Page, query.PageSize, mode, market);
        }

        private IEnumerable<BoardQuoteRow> BookRows(JObject book, BondMarket market)
        {
            foreach (JObject entry in ((JArray)book["marketData"]?["NoMDEntries"] ?? new JArray()).OfType<JObject>())
            {
                JObject reference = book["quoteRefs"]?[(string)entry["MDEntryID"]] as JObject;
                JObject quote = new JObject
                {
                    ["securityId"] = book["securityId"]?.DeepClone(), ["quoteId"] = reference?["quoteId"]?.DeepClone(),
                    ["revision"] = reference?["quoteRevision"]?.DeepClone(), ["side"] = (string)entry["MDEntryType"] == "0" ? "1" : "2",
                    ["quoteType"] = "1", ["price"] = entry["MDEntryPx"]?.DeepClone(), ["quantity"] = entry["MDEntrySize"]?.DeepClone(),
                    ["settlType"] = entry["SettlType"]?.DeepClone(), ["updateTime"] = book["updateTime"]?.DeepClone()
                };
                BoardQuoteRow row = DpsModelMapper.Display(WithReference(quote)).ToObject<BoardQuoteRow>();
                row.Market = market;
                yield return row;
            }
        }

        public async Task<PagedResult<BoardQuoteRow>> QueryBoardRowsAsync(MarketBoardQuery query, CancellationToken ct)
        {
            LivePage page = await MarketPageAsync(query, "FULL", ct);
            BoardQuoteRow[] rows = page.Rows.SelectMany(book => BookRows(book, query.Market)).ToArray();
            return new PagedResult<BoardQuoteRow>
            {
                Items = rows, TotalCount = page.TotalCount, QueryId = page.Topic, IsPartial = page.TotalCount > page.Rows.Count
            };
        }

        public async Task<PagedResult<BoardBestRow>> QueryBestRowsAsync(MarketBoardQuery query, CancellationToken ct)
        {
            LivePage page = await MarketPageAsync(query, "BEST", ct);
            BoardBestRow[] rows = page.Rows.Select(book =>
            {
                BoardBestRow row = DpsModelMapper.Display(WithReference(book)).ToObject<BoardBestRow>();
                BoardQuoteRow[] entries = BookRows(book, query.Market).ToArray();
                BoardQuoteRow[] bids = entries.Where(entry => entry.Direction == QuoteDirection.Bid).ToArray();
                BoardQuoteRow[] offers = entries.Where(entry => entry.Direction == QuoteDirection.Ofr).ToArray();
                row.Market = query.Market;
                row.Bid = bids.FirstOrDefault()?.Price;
                row.Ofr = offers.FirstOrDefault()?.Price;
                row.VolBid = bids.Length == 0 ? (decimal?)null : bids.Sum(entry => entry.Volume);
                row.VolOfr = offers.Length == 0 ? (decimal?)null : offers.Sum(entry => entry.Volume);
                row.BidBestCount = bids.Length;
                row.OfrBestCount = offers.Length;
                row.SourceQuoteIds = entries.Select(entry => entry.QuoteId).Where(id => id != null).ToArray();
                row.BidBestQuotes = bids.Select(BestDetail).ToArray();
                row.OfrBestQuotes = offers.Select(BestDetail).ToArray();
                return row;
            }).ToArray();
            return new PagedResult<BoardBestRow>
            {
                Items = rows, TotalCount = page.TotalCount, QueryId = page.Topic, IsPartial = page.TotalCount > rows.Length
            };
        }

        private static BestQuoteDetail BestDetail(BoardQuoteRow row)
        {
            return new BestQuoteDetail
            {
                QuoteId = row.QuoteId, Price = row.Price?.ToString("0.0000"), Yield = row.Price?.ToString("0.0000"),
                Volume = row.Volume, Time = row.QuoteTime?.ToString("HH:mm:ss"), IsMine = row.IsMine
            };
        }

        public async Task<IReadOnlyList<BoardQuoteRow>> LoadBoardRowsAsync(BondMarket market, CancellationToken ct)
        {
            return (await QueryBoardRowsAsync(new MarketBoardQuery { Market = market }, ct)).Items;
        }

        public async Task<IReadOnlyList<BoardBestRow>> LoadBestRowsAsync(BondMarket market, CancellationToken ct)
        {
            return (await QueryBestRowsAsync(new MarketBoardQuery { Market = market }, ct)).Items;
        }

        public Task<IReadOnlyList<BoardIntentRow>> LoadIntentRowsAsync(DateTime start, DateTime end, CancellationToken ct)
        {
            throw new NotSupportedException("模糊报价服务尚未接入。");
        }

        public Task SetIntentStatusAsync(IReadOnlyCollection<string> intentIds, IntentStatus status, CancellationToken ct)
        {
            throw new NotSupportedException("模糊报价服务尚未接入，未修改任何记录。");
        }

        public async Task<IReadOnlyList<BoardAlertRow>> LoadAlertRowsAsync(CancellationToken ct)
        {
            LivePage page = await QueryAsync("notifications", "notification", EmptyFilter(), ct);
            return page.Rows.SelectMany(notification =>
            {
                JObject display = DpsModelMapper.Display(WithReference(notification));
                display["IsRead"] = notification["readTime"] != null && notification["readTime"].Type != JTokenType.Null;
                display["AlertKey"] = notification["notificationId"]?.DeepClone();
                return ((JArray)notification["reasons"] ?? new JArray()).OfType<JObject>().Select(reason =>
                {
                    BoardAlertRow row = display.ToObject<BoardAlertRow>();
                    string type = (string)reason["type"];
                    row.AlertType = type == "WATCHLIST" ? AlertType.FavoriteBond : type == "FILTER_GROUP" ? AlertType.FilterGroup
                        : type == "COUNTER_QUOTE" ? AlertType.PriceMatch : type == "MY_BEST" ? AlertType.MyBestQuote
                        : throw new InvalidOperationException("未知提醒类型：" + type);
                    return row;
                });
            }).ToArray();
        }

        public Task<JObject> MarkNotificationReadAsync(string notificationId)
        {
            return Api.RequestProjectionAsync(SalesTradingApi.NotificationRead, ProtocolCodec.Decode<NotificationReadRequest>(new JObject
            {
                ["notificationIds"] = new JArray(notificationId), ["read"] = true
            }));
        }

        public async Task<BondLookupEntry> LookupBondAsync(string code, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            JObject result = await Api.RequestProjectionAsync(SalesTradingApi.BaseQuery, ProtocolCodec.Decode<BaseQueryRequest>(new JObject { ["kind"] = "SECURITY", ["text"] = code, ["limit"] = 30 }));
            ct.ThrowIfCancellationRequested();
            JObject security = ((JArray)result["records"]).OfType<JObject>().FirstOrDefault(row =>
                string.Equals((string)row["securityId"], code, StringComparison.OrdinalIgnoreCase));
            return new BondLookupEntry
            {
                Code = code, Found = security != null, Name = (string)security?["securityName"],
                Tenor = security?["remainingYears"]?.ToString(), BondLibrary = (string)security?["bondSubtype"],
                Valuation = security?["valuationYield"]?.ToString()
            };
        }
    }
}
