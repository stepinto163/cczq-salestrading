using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>
    /// 识别候选报价（H40 应答 quotes[]）。只返回候选不生成 quoteId；编码语义与 QuoteRecord 一致
    /// （side "1"/"2"、quoteType "1"/"2"/"3"、yieldType "1"/"2"），转换复用 GatewayModelMapper。
    /// </summary>
    public class RecognizeQuote
    {
        [JsonProperty("candidateId")]
        public string CandidateId { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        [JsonProperty("yieldType")]
        public string YieldType { get; set; }

        /// <summary>十进制字符串。</summary>
        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("quantityUnit")]
        public string QuantityUnit { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>yyyy-MM-dd。</summary>
        [JsonProperty("tradeDate")]
        public string TradeDate { get; set; }

        [JsonProperty("settlType")]
        public string SettlType { get; set; }
    }
}
