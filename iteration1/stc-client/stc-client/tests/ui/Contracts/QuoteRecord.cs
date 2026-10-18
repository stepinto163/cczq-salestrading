using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteRecord</summary>
    public class QuoteRecord : QuoteInput
    {
        [JsonProperty("clonedFromQuoteId")]
        public string ClonedFromQuoteId { get; set; }

        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

        [JsonProperty("sourceMessageId")]
        public string SourceMessageId { get; set; }

        [JsonProperty("source")]
        public QuoteSource Source { get; set; }

        [JsonProperty("ownerUserId")]
        public string OwnerUserId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        [JsonProperty("confirmedBy")]
        public string ConfirmedBy { get; set; }

        [JsonProperty("createTime")]
        public string CreateTime { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }

        [JsonProperty("quoteTime")]
        public string QuoteTime { get; set; }

        [JsonProperty("confirmTime")]
        public string ConfirmTime { get; set; }

    }
}
