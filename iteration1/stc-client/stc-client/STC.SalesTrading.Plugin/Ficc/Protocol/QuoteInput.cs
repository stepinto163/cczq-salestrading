using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>可编辑报价字段。代码使用 STC 字典值；金额、价格使用十进制字符串。</summary>
    public class QuoteInput : ProtocolModel
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("contactId")]
        public string ContactId { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        [JsonProperty("yieldType")]
        public string YieldType { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("quantityUnit")]
        public string QuantityUnit { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("tradeDate")]
        public string TradeDate { get; set; }

        [JsonProperty("settlType")]
        public string SettlType { get; set; }

        [JsonProperty("urgent")]
        public bool? Urgent { get; set; }

        [JsonProperty("recommended")]
        public bool? Recommended { get; set; }

        [JsonProperty("whole")]
        public bool? Whole { get; set; }

        [JsonProperty("oco")]
        public bool? Oco { get; set; }

        [JsonProperty("packing")]
        public bool? Packing { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

    }
}
