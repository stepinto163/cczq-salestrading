using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteEvent</summary>
    public class QuoteEvent : ProtocolModel
    {
        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("eventId")]
        public string EventId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("quote")]
        public QuoteRecord Quote { get; set; }

        [JsonProperty("record")]
        public SettingRecord Record { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

    }
}
