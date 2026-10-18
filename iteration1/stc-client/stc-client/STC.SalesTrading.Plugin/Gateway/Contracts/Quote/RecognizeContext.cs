using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>识别上下文（RecognizeRequest.context）。</summary>
    public class RecognizeContext
    {
        /// <summary>yyyy-MM-dd。</summary>
        [JsonProperty("tradeDate")]
        public string TradeDate { get; set; }
    }
}
