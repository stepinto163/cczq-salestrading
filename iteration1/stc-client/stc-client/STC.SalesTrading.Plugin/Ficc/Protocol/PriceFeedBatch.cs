using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>PriceFeedBatch</summary>
    public class PriceFeedBatch : ProtocolModel
    {
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("epoch")]
        public string Epoch { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("currentSequence")]
        public string CurrentSequence { get; set; }

        [JsonProperty("streamId")]
        public string StreamId { get; set; }

        [JsonProperty("quotePosition")]
        public EventPosition QuotePosition { get; set; }

        [JsonProperty("prices")]
        public List<PriceBook> Prices { get; set; }

    }
}
