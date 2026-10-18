using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Parser
{
    /// <summary>
    /// 识别结果行（ParserSvr / stc.parser.bond）。
    /// 编码为 ParserSvr 私有口径，与 FICC 不同：
    /// side "1"=Bid/"2"=Ofr（同 FICC）；priceType "1"=净价 "2"=全价 "3"=到期 "7"=行权；
    /// settlType 3/4 = 明日交易 T+0/T+1（STC 明+0/明+1 老口径，非 T+2/T+3）。
    /// </summary>
    public class ParserQuote
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        /// <summary>债券简称。</summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("priceType")]
        public string PriceType { get; set; }

        /// <summary>价格/收益率，字符串传输。</summary>
        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("qty")]
        public string Qty { get; set; }

        [JsonProperty("settlType")]
        public string SettlType { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("traderId")]
        public string TraderId { get; set; }

        [JsonProperty("traderName")]
        public string TraderName { get; set; }

        /// <summary>识别来源原文。</summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>剩余期限。</summary>
        [JsonProperty("timeToMaturity")]
        public string TimeToMaturity { get; set; }

        /// <summary>中债估值。</summary>
        [JsonProperty("chinaBondValuation")]
        public string ChinaBondValuation { get; set; }
    }
}
