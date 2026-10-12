using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>单条报价缺少可比较收益率，不影响其他行情展示。</summary>
    public class PriceCalculationIssue : ProtocolModel
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
