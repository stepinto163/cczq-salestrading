using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Notification
{

    /// <summary>通知原因（NotificationRecord.reasons[]）。</summary>
    public class NotificationReason
    {
        /// <summary>WATCHLIST / FILTER_GROUP / COUNTER_QUOTE / MY_BEST。</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>命中的分组 Id 集合。</summary>
        [JsonProperty("groupIds")]
        public List<string> GroupIds { get; set; }

        /// <summary>原因说明文本。</summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>对价提醒：本人报价 Id（后端新增）。</summary>
        [JsonProperty("ownQuoteId")]
        public string OwnQuoteId { get; set; }

        /// <summary>对价提醒：对手报价 Id（后端新增）。</summary>
        [JsonProperty("otherQuoteId")]
        public string OtherQuoteId { get; set; }

        /// <summary>关联报价集合（后端新增，同 record.quoteIds 的 reason 级细分）。</summary>
        [JsonProperty("quoteIds")]
        public List<string> QuoteIds { get; set; }
    }

}
