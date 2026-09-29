using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Ficc
{
    /// <summary>STC ParserSvr protocol adapter. Recognition never creates a QuoteSvr record.</summary>
    public sealed class StcQuoteParserClient
    {
        private readonly IFiccTransport transport;
        private readonly string userName;

        public StcQuoteParserClient(IFiccTransport transport, string userName)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.userName = userName;
        }

        public async Task<QuoteParseResult> ParseAsync(string text, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("请先输入报价文本。", nameof(text));
            }
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new InvalidOperationException("STC 登录用户尚未就绪。");
            }

            Stopwatch timer = Stopwatch.StartNew();
            // ParserSvr uses Code/Msg/Datas, not the FICC Data/requestId envelope.
            ParserRequest request = new ParserRequest { UserName = userName, Content = text };
            JObject reply = await transport.RequestAsync("ParserSvr", "stc.parser.bond", request.ToJson());
            cancellation.ThrowIfCancellationRequested();
            if (reply == null)
            {
                throw new InvalidOperationException("ParserSvr 未返回应答。");
            }
            if ((string)reply["Code"] != "0000")
            {
                throw new InvalidOperationException("ParserSvr " + (string)reply["Code"] + ": " + (string)reply["Msg"]);
            }
            ParserReply parsedReply = ProtocolCodec.Decode<ParserReply>(reply);
            List<ParserQuote> data = parsedReply.Datas;
            if (data == null)
            {
                throw new InvalidOperationException("ParserSvr 应答缺少 Datas 数组。");
            }

            List<QuoteConfirmRow> rows = new List<QuoteConfirmRow>();
            List<string> warnings = new List<string>();
            foreach (ParserQuote item in data)
            {
                if (item == null)
                {
                    throw new InvalidOperationException("ParserSvr 返回了无效的报价行。");
                }
                QuoteConfirmRow row = Map(item.ToJson());
                rows.Add(row);
                if (string.IsNullOrWhiteSpace(row.Code) || !row.Price.HasValue || !row.Volume.HasValue)
                {
                    warnings.Add("第 " + rows.Count + " 条报价字段不完整，请核对后提交。");
                }
                if ((!string.IsNullOrWhiteSpace(row.Institution) && string.IsNullOrWhiteSpace(row.InstitutionId))
                    || (!string.IsNullOrWhiteSpace(row.Trader) && string.IsNullOrWhiteSpace(row.ContactId)))
                {
                    warnings.Add("第 " + rows.Count + " 条机构或联系人尚未匹配 CRM，确认前须选择对应记录。");
                }
            }
            if (rows.Count == 0)
            {
                warnings.Add("未识别到报价，请检查原文后重试。");
            }
            return new QuoteParseResult
            {
                Rows = rows,
                Warnings = warnings,
                Elapsed = timer.Elapsed,
                SucceededCount = rows.Count,
                // ParserSvr does not provide a per-input-line failure count.
                FailedCount = 0
            };
        }

        private static QuoteConfirmRow Map(JObject quote)
        {
            string side = (string)quote["side"];
            if (side != "1" && side != "2")
            {
                throw new InvalidOperationException("ParserSvr 返回了不支持的报价方向：" + side);
            }
            PriceType priceType;
            switch ((string)quote["priceType"])
            {
                case "1":
                    priceType = PriceType.CleanPrice;
                    break;
                case "2":
                    priceType = PriceType.DirtyPrice;
                    break;
                case "3":
                    priceType = PriceType.YieldToMaturity;
                    break;
                case "7":
                    priceType = PriceType.YieldToExercise;
                    break;
                default:
                    throw new InvalidOperationException("ParserSvr 返回了不支持的计价类型：" + (string)quote["priceType"]);
            }
            string settlement = (string)quote["settlType"];
            QuoteConfirmRow row = new QuoteConfirmRow
            {
                Code = (string)quote["securityId"],
                Name = (string)quote["symbol"],
                Direction = side == "1" ? QuoteDirection.Bid : QuoteDirection.Ofr,
                PriceType = priceType,
                Price = (decimal?)quote["price"],
                Volume = (decimal?)quote["qty"],
                InstitutionId = (string)quote["institutionId"],
                Institution = (string)quote["institutionName"],
                ContactId = (string)quote["traderId"],
                Trader = (string)quote["traderName"],
                SettlementCode = settlement,
                Settlement = settlement == "1" ? SettlementType.T0
                    : settlement == "2" ? SettlementType.T1
                    : settlement == "3" ? SettlementType.T2
                    : settlement == "4" ? SettlementType.T3 : SettlementType.Forward,
                TradeDate = DateTime.Today,
                TextContent = (string)quote["text"],
                Maturity = (string)quote["timeToMaturity"],
                NzValuation = (decimal?)quote["chinaBondValuation"],
                SourceType = "MANUAL",
                BatchStatus = BatchStatus.PendingReview
            };
            // Parser clordid/status are not persisted FICC quote IDs or lifecycle states.
            row.RecognitionId = "PARSE-" + row.ClientQuoteId;
            return row;
        }
    }
}
