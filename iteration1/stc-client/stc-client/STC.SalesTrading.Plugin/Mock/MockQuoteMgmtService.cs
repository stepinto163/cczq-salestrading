using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;

namespace STC.SalesTrading.Plugin.Mock
{
    /// <summary>报价管理服务 Mock 实现（开发期样例数据，G7 接网关时替换）。</summary>
    public class MockQuoteMgmtService : IQuoteMgmtService
    {
        private readonly MockSalesTradingStore _store;

        public MockQuoteMgmtService()
            : this(new MockRequestOptions(), MockSalesTradingStore.Shared)
        {
        }

        public MockQuoteMgmtService(MockRequestOptions options)
            : this(options, MockSalesTradingStore.Shared)
        {
        }

        public MockQuoteMgmtService(MockRequestOptions options, MockSalesTradingStore store)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _store.EnsureToday(BuildToday);
            _store.EnsureHistory(() => BuildHistory(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-1)));
        }

        public MockRequestOptions Options { get; }

        public Task<IReadOnlyList<MgmtQuoteRow>> LoadTodayRowsAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<MgmtQuoteRow>>(_store.TodaySnapshot());
        }

        public Task<IReadOnlyList<MgmtQuoteRow>> LoadHistoryRowsAsync(DateTime start, DateTime end, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<MgmtQuoteRow>>(_store.HistorySnapshot(start, end));
        }

        public async Task<PagedResult<MgmtQuoteRow>> QueryTodayRowsAsync(QuoteMgmtQuery query, CancellationToken ct)
        {
            await BeforeRequestAsync(ct);
            return Query(Options.ReturnEmptyResults ? new List<MgmtQuoteRow>() : _store.TodaySnapshot(), query);
        }

        public async Task<PagedResult<MgmtQuoteRow>> QueryHistoryRowsAsync(QuoteMgmtQuery query, CancellationToken ct)
        {
            await BeforeRequestAsync(ct);
            var start = query != null && query.StartDate.HasValue ? query.StartDate.Value : DateTime.Today.AddDays(-7);
            var end = query != null && query.EndDate.HasValue ? query.EndDate.Value : DateTime.Today;
            return Query(Options.ReturnEmptyResults ? new List<MgmtQuoteRow>() : _store.HistorySnapshot(start, end), query);
        }

        public async Task DeleteQuotesAsync(IReadOnlyCollection<string> quoteIds, bool history, CancellationToken ct)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();
            _store.DeleteManagedQuotes(quoteIds, history);
        }

        private async Task BeforeRequestAsync(CancellationToken ct)
        {
            if (Options.Delay > TimeSpan.Zero) await Task.Delay(Options.Delay, ct);
            ct.ThrowIfCancellationRequested();
            if (!Options.FailNextRequest) return;
            Options.FailNextRequest = false;
            throw new InvalidOperationException(Options.FailureMessage);
        }

        private static PagedResult<MgmtQuoteRow> Query(IReadOnlyList<MgmtQuoteRow> source, QuoteMgmtQuery query)
        {
            var rows = source == null ? new List<MgmtQuoteRow>() : new List<MgmtQuoteRow>(source);
            if (query != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Mode) && query.Mode != "全部")
                    rows = rows.Where(r => ModeLabel(r.Mode) == query.Mode).ToList();
                if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "全部")
                    rows = rows.Where(r => (r.Status == QuoteStatus.Revoked ? "撤销" : "有效") == query.Status).ToList();
                rows = rows.Where(r => Contains(r.Code, query.BondSearchText) || Contains(r.Name, query.BondSearchText)).ToList();
                rows = rows.Where(r => Contains(r.Institution, query.InstitutionSearchText)).ToList();
                rows = rows.Where(r => Contains(r.WallTrader, query.TraderSearchText)).ToList();
            }

            var total = rows.Count;
            var page = query == null ? 1 : Math.Max(1, query.Page);
            var pageSize = query == null ? 200 : Math.Max(1, query.PageSize);
            return new PagedResult<MgmtQuoteRow>
            {
                Items = rows.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = total,
                QueryId = "mock-quote-mgmt",
            };
        }

        private static bool Contains(string value, string search)
        {
            return string.IsNullOrWhiteSpace(search)
                || (!string.IsNullOrEmpty(value) && value.IndexOf(search.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string ModeLabel(QuoteMode mode)
        {
            switch (mode)
            {
                case QuoteMode.Qt: return "QT";
                case QuoteMode.Ideal: return "ideal";
                case QuoteMode.Manual: return "手工录入";
                default: return string.Empty;
            }
        }

        private static readonly string[] Codes = { "240210.IB", "240011.IB", "230026.IB", "220208.IB", "240205.IB", "230210.IB", "240006.IB", "220315.IB" };
        private static readonly string[] Names = { "24国开10", "24附息国债11", "23附息国债26", "22国开08", "24国开05", "23国开10", "24附息国债06", "22国开15" };
        private static readonly string[] Institutions = { "国利", "国际", "平安", "中诚", "上田", "信唐" };
        private static readonly string[] Traders = { "张伟", "李娜", "王强", "刘洋", "陈晨" };
        private static readonly string[] Maturities = { "1.2Y", "2.5Y", "3.1Y", "4.8Y", "0.5Y", "5.6Y", "2.0Y", "7.3Y" };
        private static readonly string[] Ratings = { "AAA", "AA+", "AAA", "AA", "AAA", "AA+", "AAA", "AAA" };

        private static List<MgmtQuoteRow> BuildToday()
        {
            var now = DateTime.Now;
            var list = new List<MgmtQuoteRow>();
            for (var i = 0; i < 15; i++)
            {
                var r = New(i);
                r.QuoteTime = now.AddMinutes(-i * 7);
                r.WallTrader = "长城" + Traders[i % Traders.Length];
                list.Add(r);
            }
            return list;
        }

        private static List<MgmtQuoteRow> BuildHistory(DateTime start, DateTime end)
        {
            var list = new List<MgmtQuoteRow>();
            var count = 0;
            for (var d = end.Date; d >= start.Date; d = d.AddDays(-1))
            {
                var perDay = 3 + (count % 4);
                for (var j = 0; j < perDay && count < 24; j++)
                {
                    var r = New(count);
                    r.QuoteTime = d.AddHours(9 + j).AddMinutes(30);
                    r.WallTrader = "长城" + Traders[count % Traders.Length];
                    list.Add(r);
                    count++;
                }
            }
            return list;
        }

        private static MgmtQuoteRow New(int i)
        {
            var bid = i % 2 == 0;
            var mode = i % 3 == 0 ? QuoteMode.Qt : i % 3 == 1 ? QuoteMode.Ideal : QuoteMode.Manual;
            var revoked = i % 5 == 4;
            var flags = QuoteFlag.None;
            if (i % 6 == 0) flags |= QuoteFlag.Urgent;
            if (i % 7 == 0) flags |= QuoteFlag.Recommend;
            if (i % 9 == 0) flags |= QuoteFlag.Whole;
            if (i % 11 == 0) flags |= QuoteFlag.Packing;
            if (i % 13 == 0) flags |= QuoteFlag.Oco;

            return new MgmtQuoteRow
            {
                QuoteId = "Q" + (30000 + i),
                Mode = mode,
                Status = revoked ? QuoteStatus.Revoked : QuoteStatus.Active,
                Maturity = Maturities[i % Maturities.Length],
                Code = Codes[i % Codes.Length],
                Name = Names[i % Names.Length],
                Direction = bid ? QuoteDirection.Bid : QuoteDirection.Ofr,
                Price = Math.Round(2.1000m + i * 0.0137m, 4),
                Volume = 1000 + i * 500,
                Flags = flags,
                Institution = Institutions[i % Institutions.Length],
                Trader = Traders[i % Traders.Length],
                WallTrader = string.Empty,
                LastPrice = Math.Round(2.0500m + i * 0.0101m, 4),
                BondLibrary = i % 2 == 0 ? "银行间" : "交易所",
                IssuerRating = Ratings[i % Ratings.Length],
                BondRating = Ratings[(i + 1) % Ratings.Length],
                NzNetPrice = Math.Round(101.2000m + i * 0.031m, 4),
                NzDeviation = Math.Round(-0.50m + i * 0.07m, 2),
                NzYtm = Math.Round(2.0800m + i * 0.011m, 4),
                CsNetPrice = Math.Round(101.0000m + i * 0.029m, 4),
                CsFullPrice = Math.Round(103.0000m + i * 0.041m, 4),
                CsYtm = Math.Round(1.9900m + i * 0.012m, 4),
                QuoteTime = DateTime.Now,
            };
        }
    }
}
