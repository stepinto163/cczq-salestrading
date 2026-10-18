using Newtonsoft.Json;
using System;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>
    /// 报价变更历史记录（ficc.quote.history.query，H10 records[]，按 revision 升序）。
    /// before/after 全量快照不进 UI（日志弹窗只列 时间/动作/版本/操作人/原因）。
    /// </summary>
    public class QuoteLogRecord
    {
        [JsonProperty("eventId")]
        public string EventId { get; set; }

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

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
