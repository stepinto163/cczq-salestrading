using STC.SalesTrading.Plugin.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 精确报价文本本地简析（方向/代码/价格/报量/清算的极简行解析）。
    /// 仅作 ficc.quote.recognize（H40）上线前的过渡兜底：识别请求失败时由服务实现回退到此处，
    /// 并在结果 Warnings 注明；后端识别上线后删除回退调用即可。
    /// </summary>
    public static class PreciseTextParser
    {
        /// <summary>逐行解析；返回解析出的行与失败行数（无法提取代码与价格的行计失败）。</summary>
        public static QuoteParseResult Parse(string text)
        {
            var rows = new List<QuoteRow>();
            var warnings = new List<string>();
            var lineNumber = 0;
            if (!string.IsNullOrWhiteSpace(text))
            {
                foreach (var line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    lineNumber++;
                    var row = TryParseLine(line.Trim());
                    if (row != null) rows.Add(row);
                    else warnings.Add("第 " + lineNumber + " 行未能识别（需含 代码+价格）：" + line.Trim());
                }
            }

            return new QuoteParseResult
            {
                Rows = rows,
                Warnings = warnings,
                SucceededCount = rows.Count,
                FailedCount = warnings.Count,
                Elapsed = TimeSpan.FromMilliseconds(1),
            };
        }

        /// <summary>极简行解析：Bid/Ofr 关键词 + 债券代码（含 .IB/.SH）+ 价格 + 报量，可选 净价/全价/T+N。</summary>
        private static QuoteRow TryParseLine(string line)
        {
            var upper = line.ToUpperInvariant();
            var direction = upper.Contains("OFR") || upper.StartsWith("出") || upper.StartsWith("收") && upper.Contains("OFR")
                ? QuoteDirection.Ofr
                : upper.Contains("BID") || upper.StartsWith("收") || upper.StartsWith("REF") && upper.Contains("BID")
                    ? QuoteDirection.Bid
                    : QuoteDirection.Ofr;
            if (upper.StartsWith("REF") || upper.StartsWith("REFER")) direction = upper.Contains("BID") ? QuoteDirection.Bid : QuoteDirection.Ofr;

            var code = Regex.Match(line, @"\d{6,9}(\.IB|\.SH)?").Value;
            if (string.IsNullOrEmpty(code)) return null;

            var priceType = upper.Contains("净价") ? PriceType.CleanPrice
                : upper.Contains("全价") ? PriceType.DirtyPrice
                : PriceType.YieldToMaturity;

            var priceMatch = Regex.Match(line, @"(?<!\d)(\d{1,3}\.\d{2,4})(?!\d)");
            if (!priceMatch.Success) return null;
            var price = decimal.Parse(priceMatch.Value, CultureInfo.InvariantCulture);

            var volMatch = Regex.Match(line, @"(?<!\d)(\d{3,6})(?!\d)");
            decimal? volume = volMatch.Success ? decimal.Parse(volMatch.Value) : (decimal?)null;

            var settlement = upper.Contains("T+0") ? SettlementType.T0
                : upper.Contains("T+2") ? SettlementType.T2
                : upper.Contains("T+3") ? SettlementType.T3
                : SettlementType.T1;

            var nameMatch = Regex.Match(line, @"\d{2}[国附开进出]\S{2,4}");

            return new QuoteRow
            {
                QuoteId = "qc-precise-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                SourceType = SourceType.Manual,
                Status = QuoteStatus.PendingConfirm,
                CreateTime = DateTime.Now,
                SecurityId = code,
                SecurityName = nameMatch.Success ? nameMatch.Value : code,
                Side = direction,
                Price = price,
                QuoteType = priceType,
                Quantity = volume,
                SettlType = settlement,
                TradeDate = DateTime.Now.Date,
                Remark = line,
            };
        }
    }
}
