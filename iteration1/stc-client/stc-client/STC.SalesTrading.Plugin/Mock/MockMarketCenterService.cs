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
    /// <summary>
    /// 行情中心服务 Mock 实现。全部看板行与最优聚合行同源生成（原型 rebuildBoardProjections 语义：
    /// 活跃报价按市场分类 → 全部列表 / 按 代码 聚合双向最优），保证两表数据一致。
    /// </summary>
    public class MockMarketCenterService : IMarketCenterService
    {
        private static readonly List<BoardQuoteRow> CreditRows;
        private static readonly List<BoardQuoteRow> RateRows;
        private readonly MockSalesTradingStore _store;

        public MockMarketCenterService()
            : this(new MockRequestOptions())
        {
        }

        public MockMarketCenterService(MockRequestOptions options)
            : this(options, MockSalesTradingStore.Shared)
        {
        }

        public MockMarketCenterService(MockRequestOptions options, MockSalesTradingStore store)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _store.EnsureBoard(() => CreditRows.Concat(RateRows));
        }

        public MockRequestOptions Options { get; }

        static MockMarketCenterService()
        {
            var now = DateTime.Now;

            CreditRows = new List<BoardQuoteRow>
            {
                Row(BondMarket.Credit, "7.83Y", "102480210.IB", "24中铁建MTN001", QuoteDirection.Bid, 2.62m, 2000, "中信证券", "张伟", "李强", 2.6180m, "银行间", "AAA", "AAA", 99.8201m, -3.25m, 2.6001m, 99.5322m, 100.8821m, 2.5902m, now, QuoteFlag.None, false, "240210"),
                Row(BondMarket.Credit, "7.83Y", "102480210.IB", "24中铁建MTN001", QuoteDirection.Ofr, 2.60m, 3000, "光大证券", "刘明", "王芳", 2.6180m, "银行间", "AAA", "AAA", 99.8201m, -1.25m, 2.6001m, 99.5322m, 100.8821m, 2.5902m, now, QuoteFlag.Urgent, false, "240211"),
                Row(BondMarket.Credit, "2.15Y", "012580101.IB", "25电网SCP001", QuoteDirection.Bid, 2.05m, 5000, "平安证券", "陈晨", "赵岩", 2.0480m, "银行间", "AAA", "A-1", 100.5102m, 0.85m, 2.0412m, 100.2011m, 101.5203m, 2.0301m, now, QuoteFlag.Whole, true, "240212"),
                Row(BondMarket.Credit, "4.62Y", "102380301.IB", "23国电投MTN002", QuoteDirection.Ofr, 2.38m, 1500, "国泰君安", "周雪", "孙浩", 2.3720m, "银行间", "AAA", "AAA", 101.2503m, 2.10m, 2.3702m, 100.9620m, 102.3102m, 2.3601m, now, QuoteFlag.Recommend, false, "240213"),
                Row(BondMarket.Credit, "4.62Y", "102380301.IB", "23国电投MTN002", QuoteDirection.Bid, 2.40m, 1000, "中金公司", "吴迪", "郑爽", 2.3720m, "银行间", "AAA", "AAA", 101.2503m, 4.10m, 2.3702m, 100.9620m, 102.3102m, 2.3601m, now, QuoteFlag.None, false, "240214"),
                Row(BondMarket.Credit, "3.36Y", "2424002.IB", "24招商银行二级01", QuoteDirection.Bid, 2.25m, 2500, "中信建投", "马超", "刘洋", 2.2410m, "银行间", "AAA", "AAA", 102.1805m, 1.65m, 2.2402m, 101.8902m, 103.2104m, 2.2301m, now, QuoteFlag.Oco, false, "240215"),
                Row(BondMarket.Credit, "3.36Y", "2424002.IB", "24招商银行二级01", QuoteDirection.Ofr, 2.22m, 3500, "华泰证券", "郭静", "陈涛", 2.2410m, "银行间", "AAA", "AAA", 102.1805m, -1.35m, 2.2402m, 101.8902m, 103.2104m, 2.2301m, now, QuoteFlag.Packing, false, "240216"),
                Row(BondMarket.Credit, "1.85Y", "2480001.IB", "24铁道债01", QuoteDirection.Bid, 1.98m, 4000, "广发证券", "林峰", "罗宾", 1.9750m, "银行间", "AAA", "AAA", 101.0501m, 0.55m, 1.9712m, 100.7210m, 102.0102m, 1.9601m, now, QuoteFlag.None, false, "240217"),
                Row(BondMarket.Credit, "5.12Y", "250205.SH", "25京投02", QuoteDirection.Ofr, 2.95m, 800, "招商证券", "韩雪", "顾磊", 2.9410m, "上交所", "AAA", "AA+", 100.3202m, 1.90m, 2.9302m, 100.0521m, 101.3802m, 2.9201m, now, QuoteFlag.None, false, "240218"),
                Row(BondMarket.Credit, "5.12Y", "250205.SH", "25京投02", QuoteDirection.Bid, 3.00m, 1200, "海通证券", "曹阳", "陆俊", 2.9410m, "上交所", "AAA", "AA+", 100.3202m, 6.90m, 2.9302m, 100.0521m, 101.3802m, 2.9201m, now, QuoteFlag.None, false, "240219"),
                Row(BondMarket.Credit, "2.78Y", "102580152.IB", "25中石油MTN002", QuoteDirection.Bid, 2.12m, 3000, "申万宏源", "蒋丽", "魏斌", 2.1150m, "银行间", "AAA", "AAA", 101.4204m, 0.62m, 2.1102m, 101.1320m, 102.4602m, 2.1001m, now, QuoteFlag.Urgent, true, "240220"),
                Row(BondMarket.Credit, "2.78Y", "102580152.IB", "25中石油MTN002", QuoteDirection.Ofr, 2.10m, 2000, "东方证券", "秦蕾", "冯刚", 2.1150m, "银行间", "AAA", "AAA", 101.4204m, -1.38m, 2.1102m, 101.1320m, 102.4602m, 2.1001m, now, QuoteFlag.None, false, "240221"),
                Row(BondMarket.Credit, "6.44Y", "102380610.IB", "23城投控股MTN001", QuoteDirection.Bid, 3.42m, 600, "国信证券", "范冰", "袁帅", 3.4150m, "银行间", "AA+", "AA+", 97.8502m, 0.35m, 3.4012m, 97.5320m, 98.9102m, 3.3901m, now, QuoteFlag.None, false, "240222"),
                Row(BondMarket.Credit, "0.92Y", "012580331.IB", "25华能SCP002", QuoteDirection.Ofr, 1.92m, 5000, "银河证券", "许琳", "苏杰", 1.9180m, "银行间", "AAA", "A-1", 100.2102m, 0.28m, 1.9102m, 99.9321m, 100.6102m, 1.9001m, now, QuoteFlag.None, false, "240223"),
                Row(BondMarket.Credit, "0.92Y", "012580331.IB", "25华能SCP002", QuoteDirection.Bid, 1.95m, 2000, "兴业证券", "齐雪", "万勇", 1.9180m, "银行间", "AAA", "A-1", 100.2102m, 3.28m, 1.9102m, 99.9321m, 100.6102m, 1.9001m, now, QuoteFlag.Recommend, false, "240224"),
            };

            RateRows = new List<BoardQuoteRow>
            {
                // 实盘重点券（CFETS 真实存在，供宿主行情订阅通道联调）
                Row(BondMarket.Rate, "5.62Y", "220220.IB", "22国开20", QuoteDirection.Bid, 1.8800m, 2000, "中信证券", "张伟", "李强", 1.8700m, "银行间", "—", "—", 101.1204m, 1.25m, 1.8652m, 100.8902m, 102.2104m, 1.8601m, now, QuoteFlag.None, true, "240201"),
                Row(BondMarket.Rate, "5.62Y", "220220.IB", "22国开20", QuoteDirection.Ofr, 1.8600m, 3000, "光大证券", "刘明", "王芳", 1.8700m, "银行间", "—", "—", 101.1204m, -1.25m, 1.8652m, 100.8902m, 102.2104m, 1.8601m, now, QuoteFlag.Urgent, false, "240202"),
                Row(BondMarket.Rate, "0.82Y", "220210.IB", "22国开10", QuoteDirection.Ofr, 1.7000m, 2500, "平安证券", "陈晨", "赵岩", 1.7100m, "银行间", "—", "—", 100.5602m, 0.85m, 1.6952m, 100.3102m, 100.9102m, 1.6901m, now, QuoteFlag.None, false, "240203"),
                Row(BondMarket.Rate, "0.82Y", "220210.IB", "22国开10", QuoteDirection.Bid, 1.7200m, 1500, "国泰君安", "周雪", "孙浩", 1.7100m, "银行间", "—", "—", 100.5602m, 3.85m, 1.6952m, 100.3102m, 100.9102m, 1.6901m, now, QuoteFlag.None, false, "240204"),
                Row(BondMarket.Rate, "4.79Y", "210215.IB", "21国开15", QuoteDirection.Bid, 1.7900m, 3000, "中金公司", "吴迪", "郑爽", 1.7800m, "银行间", "—", "—", 100.8804m, 2.10m, 1.7752m, 100.5602m, 101.4102m, 1.7701m, now, QuoteFlag.Whole, true, "240205"),
                Row(BondMarket.Rate, "4.79Y", "210215.IB", "21国开15", QuoteDirection.Ofr, 1.7750m, 4000, "中信建投", "马超", "刘洋", 1.7800m, "银行间", "—", "—", 100.8804m, -2.05m, 1.7752m, 100.5602m, 101.4102m, 1.7701m, now, QuoteFlag.None, false, "240206"),
                Row(BondMarket.Rate, "6.56Y", "230210.IB", "23国开10", QuoteDirection.Bid, 1.9200m, 1800, "华泰证券", "郭静", "陈涛", 1.9100m, "银行间", "—", "—", 101.6502m, 0.95m, 1.9052m, 101.3102m, 102.5302m, 1.9001m, now, QuoteFlag.None, false, "240207"),
                Row(BondMarket.Rate, "6.56Y", "230210.IB", "23国开10", QuoteDirection.Ofr, 1.9050m, 2600, "广发证券", "林峰", "罗宾", 1.9100m, "银行间", "—", "—", 101.6502m, -1.75m, 1.9052m, 101.3102m, 102.5302m, 1.9001m, now, QuoteFlag.Packing, false, "240208"),
                Row(BondMarket.Rate, "0.45Y", "100029.IB", "10国开29", QuoteDirection.Bid, 1.6200m, 5000, "招商证券", "韩雪", "顾磊", 1.6100m, "银行间", "—", "—", 99.8801m, 1.35m, 1.6052m, 99.6102m, 100.1102m, 1.5951m, now, QuoteFlag.Recommend, false, "240209"),
                Row(BondMarket.Rate, "0.45Y", "100029.IB", "10国开29", QuoteDirection.Ofr, 1.6000m, 3200, "海通证券", "曹阳", "陆俊", 1.6100m, "银行间", "—", "—", 99.8801m, -1.15m, 1.6052m, 99.6102m, 100.1102m, 1.5951m, now, QuoteFlag.None, false, "240210"),
            };
        }

        private static BoardQuoteRow Row(
            BondMarket market, string maturity, string code, string name, QuoteDirection direction,
            decimal price, decimal volume, string institution, string trader, string wallTrader,
            decimal lastPrice, string bondLibrary, string issuerRating, string bondRating,
            decimal nzNet, decimal nzDev, decimal nzYtm, decimal csNet, decimal csFull, decimal csYtm,
            DateTime quoteTime, QuoteFlag flags, bool isMine, string ownerAccountId,
            PriceType priceType = PriceType.YieldToMaturity)
        {
            return new BoardQuoteRow
            {
                Market = market,
                QuoteId = "bq-" + code + "-" + direction,
                Maturity = maturity,
                Code = code,
                Name = name,
                Direction = direction,
                PriceType = priceType,
                Settlement = SettlementType.T1,
                Price = price,
                Volume = volume,
                Institution = institution,
                Trader = trader,
                WallTrader = wallTrader,
                LastPrice = lastPrice,
                BondLibrary = bondLibrary,
                IssuerRating = issuerRating,
                BondRating = bondRating,
                NzNetPrice = nzNet,
                NzDeviation = nzDev,
                NzYtm = nzYtm,
                CsNetPrice = csNet,
                CsFullPrice = csFull,
                CsYtm = csYtm,
                QuoteTime = quoteTime,
                Flags = flags,
                IsMine = isMine,
                OwnerAccountId = ownerAccountId,
            };
        }

        public Task<IReadOnlyList<BoardQuoteRow>> LoadBoardRowsAsync(BondMarket market, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var source = _store.BoardSnapshot(market);
            return Task.FromResult<IReadOnlyList<BoardQuoteRow>>(source.ToList());
        }

        public async Task<PagedResult<BoardQuoteRow>> QueryBoardRowsAsync(MarketBoardQuery query, CancellationToken ct)
        {
            await BeforeRequestAsync(ct);
            var source = Options.ReturnEmptyResults ? new List<BoardQuoteRow>() : FilterBoardRows(query);
            var total = source.Count;
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Max(1, query.PageSize);
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResult<BoardQuoteRow>
            {
                Items = items,
                TotalCount = total,
                QueryId = string.Format("mock-board-{0}-{1}", query.Market, page),
            };
        }

        private List<BoardQuoteRow> FilterBoardRows(MarketBoardQuery query)
        {
            return _store.BoardSnapshot(query.Market)
                .Where(r => query.IncludeBid || r.Direction != QuoteDirection.Bid)
                .Where(r => query.IncludeOfr || r.Direction != QuoteDirection.Ofr)
                .Where(r => !query.MineOnly || r.IsMine)
                .Where(r => (r.Flags & query.RequiredFlags) == query.RequiredFlags)
                .Where(r => !query.UnquotedOnly)
                .Where(r => string.IsNullOrWhiteSpace(query.SearchText)
                    || (r.Code ?? string.Empty).IndexOf(query.SearchText, StringComparison.OrdinalIgnoreCase) >= 0
                    || (r.Name ?? string.Empty).IndexOf(query.SearchText, StringComparison.OrdinalIgnoreCase) >= 0
                    || (r.Institution ?? string.Empty).IndexOf(query.SearchText, StringComparison.OrdinalIgnoreCase) >= 0
                    || (r.Trader ?? string.Empty).IndexOf(query.SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        public Task<IReadOnlyList<BoardBestRow>> LoadBestRowsAsync(BondMarket market, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var source = _store.BoardSnapshot(market);
            return Task.FromResult<IReadOnlyList<BoardBestRow>>(BuildBestRows(source));
        }

        public async Task<PagedResult<BoardBestRow>> QueryBestRowsAsync(MarketBoardQuery query, CancellationToken ct)
        {
            await BeforeRequestAsync(ct);
            var result = Options.ReturnEmptyResults
                ? new List<BoardBestRow>()
                : BuildBestRows(FilterBoardRows(query)).ToList();
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Max(1, query.PageSize);
            return new PagedResult<BoardBestRow>
            {
                Items = result.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = result.Count,
                QueryId = string.Format("mock-best-{0}-{1}", query.Market, page),
            };
        }

        private async Task BeforeRequestAsync(CancellationToken ct)
        {
            if (Options.Delay > TimeSpan.Zero) await Task.Delay(Options.Delay, ct);
            ct.ThrowIfCancellationRequested();
            if (!Options.FailNextRequest) return;
            Options.FailNextRequest = false;
            throw new InvalidOperationException(Options.FailureMessage);
        }

        private static IReadOnlyList<BoardBestRow> BuildBestRows(IEnumerable<BoardQuoteRow> source)
        {
            var result = new List<BoardBestRow>();
            foreach (var group in source.GroupBy(r => new { r.Code, r.PriceType, r.Settlement }))
            {
                var bids = group.Where(r => r.Direction == QuoteDirection.Bid && r.Price.HasValue).ToList();
                var ofrs = group.Where(r => r.Direction == QuoteDirection.Ofr && r.Price.HasValue).ToList();
                var isYield = group.Key.PriceType == PriceType.YieldToMaturity
                    || group.Key.PriceType == PriceType.YieldToExercise
                    || group.Key.PriceType == PriceType.Spread;
                var bestBid = bids.Count == 0 ? null : (isYield
                    ? bids.OrderBy(r => r.Price.Value).First()
                    : bids.OrderByDescending(r => r.Price.Value).First());
                var bestOfr = ofrs.Count == 0 ? null : (isYield
                    ? ofrs.OrderByDescending(r => r.Price.Value).First()
                    : ofrs.OrderBy(r => r.Price.Value).First());
                if (bestBid == null && bestOfr == null) continue;

                // 原型 0910：同档（同价格）可能有多笔 → 统计笔数并生成浮层明细
                var bidLevel = bestBid == null ? new List<BoardQuoteRow>() : bids.Where(r => r.Price == bestBid.Price).ToList();
                var ofrLevel = bestOfr == null ? new List<BoardQuoteRow>() : ofrs.Where(r => r.Price == bestOfr.Price).ToList();

                var first = bestBid ?? bestOfr;
                result.Add(new BoardBestRow
                {
                    Market = first.Market,
                    Maturity = first.Maturity,
                    Code = first.Code,
                    Name = first.Name,
                    PriceType = first.PriceType,
                    Settlement = first.Settlement,
                    InstBid = bestBid?.Institution,
                    TraderBid = bestBid?.Trader,
                    VolBid = bestBid?.Volume,
                    Bid = bestBid?.Price,
                    Ofr = bestOfr?.Price,
                    VolOfr = bestOfr?.Volume,
                    TraderOfr = bestOfr?.Trader,
                    InstOfr = bestOfr?.Institution,
                    LastPrice = first.LastPrice,
                    BondLibrary = first.BondLibrary,
                    IssuerRating = first.IssuerRating,
                    BondRating = first.BondRating,
                    NzNetPrice = first.NzNetPrice,
                    NzDeviation = first.NzDeviation,
                    NzYtm = first.NzYtm,
                    CsNetPrice = first.CsNetPrice,
                    CsFullPrice = first.CsFullPrice,
                    CsYtm = first.CsYtm,
                    QuoteTime = first.QuoteTime,
                    SourceQuoteIds = group.Select(r => r.QuoteId).ToArray(),
                    BidBestCount = bidLevel.Count,
                    OfrBestCount = ofrLevel.Count,
                    BidBestQuotes = BuildBestDetails(bidLevel, isYield, group.Key.PriceType),
                    OfrBestQuotes = BuildBestDetails(ofrLevel, isYield, group.Key.PriceType),
                });
            }
            return result;
        }

        /// <summary>构造同档报价浮层明细（原型 bestQuoteLayerHtml：按时间倒序，非本人报价的机构/交易员留空）。</summary>
        private static List<BestQuoteDetail> BuildBestDetails(List<BoardQuoteRow> level, bool isYield, PriceType priceType)
        {
            return level
                .OrderByDescending(r => r.QuoteTime ?? DateTime.MinValue)
                .ThenBy(r => r.QuoteId, StringComparer.Ordinal)
                .Select(r => new BestQuoteDetail
                {
                    Price = string.Format("{0} {1}", r.Price?.ToString("0.0000") ?? "--", PriceTypeShortLabel(priceType)),
                    Yield = isYield ? (r.Price?.ToString("0.0000") ?? "--") + "%" : "—",
                    Volume = r.Volume,
                    Institution = r.IsMine ? r.Institution : null,
                    Trader = r.IsMine ? r.Trader : null,
                    OwnerTrader = r.WallTrader,
                    Time = r.QuoteTime?.ToString("HH:mm:ss") ?? "--",
                    IsMine = r.IsMine,
                    QuoteId = r.QuoteId,
                })
                .ToList();
        }

        /// <summary>价格类型短标识（原型 bestQuotePriceText 的 YTM/YTC/净价/全价）。</summary>
        private static string PriceTypeShortLabel(PriceType priceType)
        {
            switch (priceType)
            {
                case PriceType.YieldToMaturity: return "YTM";
                case PriceType.YieldToExercise: return "YTC";
                case PriceType.CleanPrice: return "净价";
                case PriceType.DirtyPrice: return "全价";
                default: return priceType.ToString();
            }
        }

        public Task<IReadOnlyList<BoardIntentRow>> LoadIntentRowsAsync(DateTime start, DateTime end, CancellationToken ct)
        {
            var now = DateTime.Now;
            var rows = new List<BoardIntentRow>
            {
                new BoardIntentRow { IntentId = "bi-001", Content = "收 城投 AAA 3到5Y 上市 公募", RecognizedConditions = "方向 Bid · 城投 · 评级 AAA · 期限 3-5Y · 上市 · 公募", Institution = "国泰君安", Trader = "陈彦霖", WallTrader = "赵明", QuoteTime = now.AddMinutes(-12), Status = IntentStatus.Active, IsPendingSync = false, Recognition = IntentRecognitionStatus.Recognized },
                new BoardIntentRow { IntentId = "bi-002", Content = "bid 短融 中票 央企 有担保 煤炭", RecognizedConditions = "方向 Bid · 短融/中票 · 央企 · 有担保 · 煤炭", Institution = "中信证券", Trader = "张伟", WallTrader = "李强", QuoteTime = now.AddMinutes(-35), Status = IntentStatus.Active, IsPendingSync = true, Recognition = IntentRecognitionStatus.Recognized },
                new BoardIntentRow { IntentId = "bi-003", Content = "出 银行间 银行二级 商金债 2025", RecognizedConditions = "方向 Ofr · 银行间 · 银行二级 · 商金债 · 2025", Institution = "平安基金", Trader = "赵明", WallTrader = "孙浩", QuoteTime = now.AddHours(-1), Status = IntentStatus.Active, IsPendingSync = false, Recognition = IntentRecognitionStatus.Partial },
                new BoardIntentRow { IntentId = "bi-004", Content = "ofr 非上市 私募 含权 赎回权 化工", RecognizedConditions = "方向 Ofr · 非上市 · 私募 · 赎回权 · 化工", Institution = "光大证券", Trader = "刘明", WallTrader = "王芳", QuoteTime = now.AddHours(-2), Status = IntentStatus.Revoked, IsPendingSync = false, Recognition = IntentRecognitionStatus.Conflict },
                new BoardIntentRow { IntentId = "bi-005", Content = "收 大于20Y 高收益 投资级 AAA+ 省级", RecognizedConditions = "方向 Bid · 期限 >20Y · 高收益 · AAA+ · 省级", Institution = "中金公司", Trader = "吴迪", WallTrader = "郑爽", QuoteTime = now.AddHours(-3), Status = IntentStatus.Active, IsPendingSync = false },
                new BoardIntentRow { IntentId = "bi-006", Content = "ofr 中票 民企 煤炭", RecognizedConditions = "方向 Ofr · 中票 · 民企 · 煤炭", Institution = "银河证券", Trader = "许琳", WallTrader = "苏杰", QuoteTime = now.AddMinutes(-95), Status = IntentStatus.Active, IsPendingSync = false, Recognition = IntentRecognitionStatus.Failed },
            };
            _store.EnsureIntents(() => rows);
            ct.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<BoardIntentRow>>(_store.IntentSnapshot(start, end));
        }

        public async Task SetIntentStatusAsync(IReadOnlyCollection<string> intentIds, IntentStatus status, CancellationToken ct)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();
            _store.SetIntentStatus(intentIds, status);
        }

        public Task<IReadOnlyList<BoardAlertRow>> LoadAlertRowsAsync(CancellationToken ct)
        {
            var now = DateTime.Now;
            var rows = new List<BoardAlertRow>
            {
                new BoardAlertRow { AlertKey = "al-001", AlertType = AlertType.PriceMatch, Maturity = "5.62Y", Code = "220220.IB", Name = "22国开20", InstBid = "中信证券", TraderBid = "张伟", VolBid = 2000, Bid = 1.8800m, Ofr = 1.8600m, VolOfr = 3000, TraderOfr = "刘明", InstOfr = "光大证券", LastPrice = 1.8700m, BondLibrary = "银行间", IssuerRating = "—", BondRating = "—", NzNetPrice = 101.1204m, NzDeviation = 1.25m, NzYtm = 1.8652m, CsNetPrice = 100.8902m, CsFullPrice = 102.2104m, CsYtm = 1.8601m, QuoteTime = now.AddMinutes(-3) },
                new BoardAlertRow { AlertKey = "al-002", AlertType = AlertType.MyBestQuote, Maturity = "6.56Y", Code = "230210.IB", Name = "23国开10", InstBid = "华泰证券", TraderBid = "郭静", VolBid = 1800, Bid = 1.9200m, Ofr = 1.9050m, VolOfr = 2600, TraderOfr = "林峰", InstOfr = "广发证券", LastPrice = 1.9100m, BondLibrary = "银行间", IssuerRating = "—", BondRating = "—", NzNetPrice = 101.6502m, NzDeviation = 0.95m, NzYtm = 1.9052m, CsNetPrice = 101.3102m, CsFullPrice = 102.5302m, CsYtm = 1.9001m, QuoteTime = now.AddMinutes(-8) },
                new BoardAlertRow { AlertKey = "al-003", AlertType = AlertType.FavoriteBond, Maturity = "4.79Y", Code = "210215.IB", Name = "21国开15", InstBid = "中金公司", TraderBid = "吴迪", VolBid = 3000, Bid = 1.7900m, Ofr = 1.7750m, VolOfr = 4000, TraderOfr = "马超", InstOfr = "中信建投", LastPrice = 1.7800m, BondLibrary = "银行间", IssuerRating = "—", BondRating = "—", NzNetPrice = 100.8804m, NzDeviation = 2.10m, NzYtm = 1.7752m, CsNetPrice = 100.5602m, CsFullPrice = 101.4102m, CsYtm = 1.7701m, QuoteTime = now.AddMinutes(-15) },
            };
            _store.EnsureAlerts(() => rows);
            ct.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<BoardAlertRow>>(_store.AlertSnapshot());
        }


        /// <summary>
        /// 按代码查债券库（关注债券识别回填）。在进程内行情集合中查找，
        /// 支持全代码（102480210.IB）、去后缀（102480210）与尾部短码（480210）三种写法。
        /// </summary>
        public Task<BondLookupEntry> LookupBondAsync(string code, CancellationToken ct)
        {
            var token = (code ?? string.Empty).Trim();
            var entry = new BondLookupEntry { Code = token };
            if (token.Length == 0)
            {
                return Task.FromResult(entry);
            }

            var bare = token.EndsWith(".IB", StringComparison.OrdinalIgnoreCase)
                ? token.Substring(0, token.Length - 3)
                : token;
            var row = _store.BoardSnapshot(BondMarket.Credit)
                .Concat(_store.BoardSnapshot(BondMarket.Rate))
                .FirstOrDefault(r => string.Equals(r.Code, token, StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(r.Code, bare, StringComparison.OrdinalIgnoreCase)
                                     || (bare.Length >= 4 && r.Code != null && r.Code.EndsWith(bare, StringComparison.OrdinalIgnoreCase)));
            if (row == null)
            {
                return Task.FromResult(entry);
            }

            entry.Found = true;
            entry.Name = row.Name;
            entry.Tenor = row.Maturity;
            entry.BondLibrary = row.BondLibrary;
            entry.Valuation = row.NzYtm.HasValue ? row.NzYtm.Value.ToString("0.0000") + "%" : "—";
            entry.Market = row.Market;
            return Task.FromResult(entry);
        }

        /// <summary>生成智能推荐 Mock 行（Bid/Ofr 双侧 × 三种路径，形态对齐原型 recommendationMockRows）。</summary>
        private static List<RecommendationRow> BuildRecommendations()
        {
            var bonds = new[]
            {
                new[] { "220220.IB", "22国开20" },
                new[] { "220210.IB", "22国开10" },
                new[] { "210215.IB", "21国开15" },
                new[] { "230210.IB", "23国开10" },
                new[] { "100029.IB", "10国开29" },
            };
            var institutions = new[] { "浦发银行", "中信证券", "华泰证券", "招商银行", "国泰君安", "平安银行", "兴业银行", "中金公司" };
            var traders = new[] { "张伟", "李强", "刘明", "王芳", "陈晨", "赵岩", "周雪", "孙浩" };
            var wallTraders = new[] { "钱进", "孙立", "李昂", "郑爽", "吴迪", "冯刚", "顾磊", "陆俊" };
            var scopes = new[] { RecommendationScope.ExactBond, RecommendationScope.SimilarBond, RecommendationScope.SimilarInstitution };

            var rows = new List<RecommendationRow>();
            foreach (var side in new[] { QuoteDirection.Bid, QuoteDirection.Ofr })
            {
                int baseScore = side == QuoteDirection.Bid ? 88 : 86;
                for (int i = 0; i < 6; i++)
                {
                    var scope = scopes[i % scopes.Length];
                    var bond = bonds[i % bonds.Length];
                    bool exact = scope == RecommendationScope.ExactBond;
                    // Ofr 侧机构/交易员错位 3，避免与 Bid 侧完全一致
                    int off = side == QuoteDirection.Ofr ? 3 : 0;
                    int score = Math.Max(63, baseScore - i * 2);

                    rows.Add(new RecommendationRow
                    {
                        RecommendationId = "mock-" + side.ToString().ToLowerInvariant() + "-" + i,
                        IsMock = true,
                        Side = side,
                        Scope = scope,
                        Score = score,
                        Grade = score >= 85 ? "A" : score >= 72 ? "B" : "C",
                        InstitutionName = institutions[(i + off) % institutions.Length],
                        TraderName = traders[(i + off) % traders.Length],
                        RelatedCode = exact ? "220220.IB" : bond[0],
                        RelatedName = exact ? "22国开20" : bond[1],
                        QuoteCount = Math.Max(2, 9 - i),
                        LatestPrice = (side == QuoteDirection.Bid ? 2.01m : 2.03m) + (i % 5) * 0.002m,
                        LatestVolume = 500 + (i % 5) * 500,
                        OwnerTrader = wallTraders[(i + off) % wallTraders.Length],
                    });
                }
            }
            return rows;
        }

        public void SubscribeQuoteEvents(Action<QuoteEvent> handler)
        {
            _store.SubscribeQuoteEvents(handler);
        }

        public void UnsubscribeQuoteEvents(Action<QuoteEvent> handler)
        {
            _store.UnsubscribeQuoteEvents(handler);
        }
    }
}
