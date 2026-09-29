using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>ParserQuote</summary>
    public class ParserQuote : ProtocolModel
    {
        [JsonProperty("chinaBondValuation")]
        public string ChinaBondValuation { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("priceType")]
        public string PriceType { get; set; }

        [JsonProperty("qty")]
        public string Qty { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("settlType")]
        public string SettlType { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("timeToMaturity")]
        public string TimeToMaturity { get; set; }

        [JsonProperty("traderId")]
        public string TraderId { get; set; }

        [JsonProperty("traderName")]
        public string TraderName { get; set; }

    }
}
