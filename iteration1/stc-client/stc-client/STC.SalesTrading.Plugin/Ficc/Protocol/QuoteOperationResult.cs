using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteOperationResult</summary>
    public class QuoteOperationResult : ProtocolModel
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("error")]
        public ErrorDetail Error { get; set; }

    }
}
