using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Mock
{
    /// <summary>
    /// 报价录入服务 Mock 实现（开发期样例数据，字段形态对齐原型 canonical）。
    /// 接网关后新增 Gateway 实现替换，ViewModel 零改动。
    /// </summary>
    public class MockQuoteService : IQuoteService
    {
        private readonly MockSalesTradingStore _store;

        public MockQuoteService()
            : this(MockSalesTradingStore.Shared)
        {
        }

        public MockQuoteService(MockSalesTradingStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _store.EnsurePending(BuildPendingRows);
        }

        public Task<IReadOnlyList<QuoteConfirmRow>> LoadPendingRowsAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<QuoteConfirmRow>>(_store.PendingSnapshot());
        }

        private static IEnumerable<QuoteConfirmRow> BuildPendingRows()
        {
            var now = DateTime.Now;
            var rows = new List<QuoteConfirmRow>
            {
                #region 批次 1：IDEAL 中信证券·张伟（待确认，4 条）

                new QuoteConfirmRow
                {
                    RecognitionId = "qc-001", BatchId = "B20260814001", SourceType = "ideal",
                    BatchStatus = BatchStatus.PendingReview, ReceivedAt = now.AddMinutes(-6),
                    BatchInstitution = "中信证券", BatchTrader = "张伟",
                    Maturity = "8.13Y", Code = "240210.IB", Name = "24国开10",
                    Direction = QuoteDirection.Bid, Price = 1.8600m, PriceType = PriceType.YieldToMaturity,
                    Volume = 2000, Institution = "中信证券", Trader = "张伟",
                    TradeDate = now.Date, Settlement = SettlementType.T1,
                    NzValuation = 102.1800m, NzNetPrice = 101.9205m, NzDeviation = -1.25m,
                    CsPriceBasis = "到期", TextContent = "bid 240210.IB 1.86 2000 T+1"
                },
                new QuoteConfirmRow
                {
                    RecognitionId = "qc-002", BatchId = "B20260814001", SourceType = "ideal",
                    BatchStatus = BatchStatus.PendingReview, ReceivedAt = now.AddMinutes(-6),
                    BatchInstitution = "中信证券", BatchTrader = "张伟",
                    Maturity = "11.27Y", Code = "240011.IB", Name = "24附息国债11",
                    Direction = QuoteDirection.Ofr, Price = 2.1200m, PriceType = PriceType.YieldToMaturity,
                    Volume = 3000, Institution = "中信证券", Trader = "张伟",
                    TradeDate = now.Date, Settlement = SettlementType.T1,
                    NzValuation = 105.5600m, NzNetPrice = 105.2011m, NzDeviation = 0.85m,
                    CsPriceBasis = "到期", TextContent = "ofr 240011.IB 2.12 3000"
                },
                new QuoteConfirmRow
                {
                    RecognitionId = "qc-003", BatchId = "B20260814001", SourceType = "ideal",
                    BatchStatus = BatchStatus.PendingReview, ReceivedAt = now.AddMinutes(-6),
                    BatchInstitution = "中信证券", BatchTrader = "张伟",
                    Maturity = "6.78Y", Code = "230026.IB", Name = "23附息国债26",
                    Direction = QuoteDirection.Bid, Price = 2.3500m, PriceType = PriceType.YieldToMaturity,
                    Volume = 1000, Institution = "中信证券", Trader = "张伟",
                    TradeDate = now.Date, Settlement = SettlementType.T0,
                    NzValuation = 100.2400m, NzNetPrice = 99.8742m, NzDeviation = 2.10m,
                    CsPriceBasis = "到期", TextContent = "bid 230026.IB 2.35 1000 T+0"
                },
                new QuoteConfirmRow
                {
                    RecognitionId = "qc-004", BatchId = "B20260814001", SourceType = "ideal",
                    BatchStatus = BatchStatus.PendingReview, ReceivedAt = now.AddMinutes(-6),
                    BatchInstitution = "中信证券", BatchTrader = "张伟",
                    Maturity = "2.15Y", Code = "210210.IB", Name = "21国开10",
                    Direction = QuoteDirection.Ofr, Price = 101.0200m, PriceType = PriceType.CleanPrice,
                    Volume = 2000, Institution = "中信证券", Trader = "张伟",
                    TradeDate = now.Date, Settlement = SettlementType.T1,
                    NzValuation = 101.5000m, NzNetPrice = 101.0125m, NzDeviation = -0.55m,
                    CsPriceBasis = "到期", TextContent = "ofr 210210.IB 净价 101.02 2000"
                },

                #endregion

                #region 批次 2：QT 光大证券·刘明（部分识别，3 条）

                new QuoteConfirmRow
                {
                    RecognitionId = "qc-005", BatchId = "B20260814002", SourceType = "qt",
                    BatchStatus = BatchStatus.PartialFailed, ReceivedAt = now.AddMinutes(-21),
                    BatchInstitution = "光大证券", BatchTrader = "刘明",
                    Maturity = "3.42Y", Code = "102580247.IB", Name = "25铁道MTN001",
                    Direction = QuoteDirection.Bid, Price = 3.8000m, PriceType = PriceType.YieldToMaturity,
                    Volume = 500, Institution = "光大证券", Trader = "刘明",
                    TradeDate = now.Date, Settlement = SettlementType.T1,
                    NzValuation = 103.1100m, NzNetPrice = 102.6681m, NzDeviation = 12.34m,
                    CsPriceBasis = "行权", TextContent = "bid 102580247.IB 3.80 500"
                },
                new QuoteConfirmRow
                {
                    RecognitionId = "qc-006", BatchId = "B20260814002", SourceType = "qt",
                    BatchStatus = BatchStatus.PartialFailed, ReceivedAt = now.AddMinutes(-21),
                    BatchInstitution = "光大证券", BatchTrader = "刘明",
                    Maturity = "3.42Y", Code = "102580247.IB", Name = "25铁道MTN001",
                    Direction = QuoteDirection.Ofr, Price = 3.8500m, PriceType = PriceType.YieldToMaturity,
                    Volume = 1000, Institution = "光大证券", Trader = "刘明",
                    TradeDate = now.Date, Settlement = SettlementType.T1,
                    NzValuation = 103.1100m, NzNetPrice = 102.6681m, NzDeviation = 17.25m,
                    CsPriceBasis = "行权", TextContent = "ofr 102580247.IB 3.85 1000"
                },
                new QuoteConfirmRow
                {
                    RecognitionId = "qc-007", BatchId = "B20260814002", SourceType = "qt",
                    BatchStatus = BatchStatus.PartialFailed, ReceivedAt = now.AddMinutes(-21),
                    BatchInstitution = "光大证券", BatchTrader = "刘明",
                    Maturity = "0.85Y", Code = "252980.SH", Name = "GC025",
                    Direction = QuoteDirection.Ofr, Price = 105.6800m, PriceType = PriceType.DirtyPrice,
                    Volume = 1500, Institution = "光大证券", Trader = "刘明",
                    TradeDate = now.Date, Settlement = SettlementType.T1,
                    NzValuation = 105.2000m, NzNetPrice = 104.8733m, NzDeviation = 0.45m,
                    CsPriceBasis = "到期", TextContent = "ofr 252980.SH 全价 105.68 1500"
                },

                #endregion

                #region 批次 3：IDEAL 平安基金·赵明（识别失败，2 条）

                new QuoteConfirmRow
                {
                    RecognitionId = "qc-008", BatchId = "B20260814003", SourceType = "ideal",
                    BatchStatus = BatchStatus.RecognitionFailed, ReceivedAt = now.AddMinutes(-45),
                    BatchInstitution = "平安基金", BatchTrader = "赵明",
                    Maturity = "4.62Y", Code = string.Empty, Name = "21柳发02",
                    Direction = QuoteDirection.Bid, Price = 3.5000m, PriceType = PriceType.YieldToMaturity,
                    Volume = 500, Institution = "平安基金", Trader = "赵明",
                    TradeDate = now.Date, Settlement = SettlementType.T1,
                    NzValuation = null, NzNetPrice = null, NzDeviation = null,
                    CsPriceBasis = "—", TextContent = "收 21柳发02 3.50 500"
                },
                new QuoteConfirmRow
                {
                    RecognitionId = "qc-009", BatchId = "B20260814003", SourceType = "ideal",
                    BatchStatus = BatchStatus.RecognitionFailed, ReceivedAt = now.AddMinutes(-45),
                    BatchInstitution = "平安基金", BatchTrader = "赵明",
                    Maturity = "1.12Y", Code = "230210.IB", Name = "23国开10",
                    Direction = QuoteDirection.Bid, Price = 2.9155m, PriceType = PriceType.YieldToMaturity,
                    Volume = 2000, Institution = "平安基金", Trader = "赵明",
                    TradeDate = now.Date, Settlement = SettlementType.T1,
                    NzValuation = 101.8300m, NzNetPrice = 101.4310m, NzDeviation = -1.05m,
                    CsPriceBasis = "到期", TextContent = "24国开10 bid 2.9155 出 ofr 2.92 2000"
                },

                #endregion
            };

            return rows;
        }

        public Task<QuoteParseResult> ParsePreciseTextAsync(string text, CancellationToken ct)
        {
            // 精确识别 Mock：极简行解析（方向 代码 价格 报量 [清算] 形态），供 UI 流程演示；
            // 真实解析引擎（含置信度/估值回填）在网关接入时替换实现。
            var rows = new List<QuoteConfirmRow>();
            if (!string.IsNullOrWhiteSpace(text))
            {
                foreach (var line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var row = TryParseLine(line.Trim());
                    if (row != null) rows.Add(row);
                }
            }

            return Task.FromResult(new QuoteParseResult
            {
                Rows = rows,
                Warnings = new string[0],
                SucceededCount = rows.Count,
                FailedCount = 0,
                Elapsed = TimeSpan.FromMilliseconds(120),
            });
        }

        /// <summary>极简行解析：Bid/Ofr 关键词 + 债券代码（含 .IB/.SH）+ 价格 + 报量，可选 净价/全价/T+N。</summary>
        private static QuoteConfirmRow TryParseLine(string line)
        {
            var upper = line.ToUpperInvariant();
            var direction = upper.Contains("OFR") || upper.StartsWith("出") || upper.StartsWith("收") && upper.Contains("OFR")
                ? QuoteDirection.Ofr
                : upper.Contains("BID") || upper.StartsWith("收") || upper.StartsWith("REF") && upper.Contains("BID")
                    ? QuoteDirection.Bid
                    : QuoteDirection.Ofr;
            if (upper.StartsWith("REF") || upper.StartsWith("REFER")) direction = upper.Contains("BID") ? QuoteDirection.Bid : QuoteDirection.Ofr;

            var code = System.Text.RegularExpressions.Regex.Match(line, @"\d{6,9}(\.IB|\.SH)?").Value;
            if (string.IsNullOrEmpty(code)) code = string.Empty;

            var nameMatch = System.Text.RegularExpressions.Regex.Match(line, @"\d{2}[国附开进出]\S{2,4}");
            var name = nameMatch.Success ? nameMatch.Value : code;

            var priceType = upper.Contains("净价") ? PriceType.CleanPrice
                : upper.Contains("全价") ? PriceType.DirtyPrice
                : PriceType.YieldToMaturity;

            var priceMatch = System.Text.RegularExpressions.Regex.Match(line, @"(?<!\d)(\d{1,3}\.\d{2,4})(?!\d)");
            decimal? price = priceMatch.Success ? decimal.Parse(priceMatch.Value, System.Globalization.CultureInfo.InvariantCulture) : (decimal?)null;

            var volMatch = System.Text.RegularExpressions.Regex.Match(line, @"(?<!\d)(\d{3,6})(?!\d)");
            decimal? volume = volMatch.Success ? decimal.Parse(volMatch.Value) : (decimal?)null;

            var settlement = upper.Contains("T+0") ? SettlementType.T0
                : upper.Contains("T+2") ? SettlementType.T2
                : upper.Contains("T+3") ? SettlementType.T3
                : SettlementType.T1;

            return new QuoteConfirmRow
            {
                RecognitionId = "qc-precise-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                SourceType = "ideal",
                BatchStatus = BatchStatus.PendingReview,
                ReceivedAt = DateTime.Now,
                Maturity = "—", Code = code, Name = name,
                Direction = direction, Price = price, PriceType = priceType,
                Volume = volume, Settlement = settlement,
                TradeDate = DateTime.Now.Date,
                NzValuation = null, NzNetPrice = null, NzDeviation = null, CsPriceBasis = "—",
                TextContent = line,
            };
        }

        public Task<IReadOnlyList<FuzzyCandidate>> QueryFuzzyCandidatesAsync(FuzzyQuery query, CancellationToken ct)
        {
            // 候选债券 Mock：55 条（50/页 → 演示第 2 页分页），形态对齐原型右栏（期限/代码/简称/债券库/中债估值）
            var candidates = new List<FuzzyCandidate>();
            var names = new[] { "24国开10", "24附息国债11", "25铁道MTN001", "23国开10", "22进出12", "24农发08", "25中票城投AAA", "24银行二级01", "25商金债02", "23国开08" };
            var libs = new[] { "银行间", "银行间", "银行间", "银行间", "银行间", "银行间", "上交所", "银行间", "银行间", "银行间" };
            for (var i = 0; i < 55; i++)
            {
                var t = i % names.Length;
                candidates.Add(new FuzzyCandidate
                {
                    Maturity = string.Format("{0}.{1:D2}Y", (i % 15) + 1, (i % 12)),
                    Code = string.Format("24{0:D4}.IB", (i % 40) * 7 + 101),
                    Name = names[t] + (i >= names.Length ? i.ToString("D2") : string.Empty),
                    BondLibrary = libs[t],
                    NzValuation = Math.Round(100m + (i % 90) * 0.0375m, 4),
                });
            }

            return Task.FromResult<IReadOnlyList<FuzzyCandidate>>(candidates);
        }

        public Task ConfirmQuotesAsync(IReadOnlyCollection<string> recognitionIds, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _store.ConfirmPending(recognitionIds);
            return Task.CompletedTask;
        }

        public Task SubmitQuotesAsync(IReadOnlyCollection<QuoteConfirmRow> rows, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _store.SubmitQuotes(rows);
            return Task.CompletedTask;
        }

        public Task DeleteQuotesAsync(IReadOnlyCollection<string> recognitionIds, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _store.DeletePending(recognitionIds);
            return Task.CompletedTask;
        }

        public Task SubmitIntentQuotesAsync(IReadOnlyCollection<BoardIntentRow> rows, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _store.AddIntents(rows);
            return Task.CompletedTask;
        }
    }
}
