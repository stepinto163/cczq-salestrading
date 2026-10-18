using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>报价操作报告（S01 reports[]，仅向操作者本人发布）。</summary>
    public class StreamReportItem
    {
        [JsonProperty("eventId")]
        public string EventId { get; set; }

        /// <summary>触发本操作的请求 requestId，用于本地在途请求回执关联。</summary>
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        /// <summary>CREATE / UPDATE / CONFIRM / REFER / UNREFER / DELETE。</summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("operatorUserId")]
        public string OperatorUserId { get; set; }

        [JsonProperty("operationTime")]
        public DateTime? OperationTime { get; set; }
    }

}
