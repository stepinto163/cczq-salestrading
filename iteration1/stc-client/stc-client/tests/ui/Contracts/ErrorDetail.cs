using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>ErrorDetail</summary>
    public class ErrorDetail : ProtocolModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }

        [JsonProperty("currentRevision")]
        public string CurrentRevision { get; set; }

    }
}
