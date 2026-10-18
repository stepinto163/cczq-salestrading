using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Notification
{

    /// <summary>通知（S02 推送、H26/H27/H29 应答）。同券非对价原因合并一条、对价独立。</summary>
    public class NotificationTrigger
    {
        [JsonProperty("alertKey")]
        public string AlertKey { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("quoteIds")]
        public List<string> QuoteIds { get; set; }
        [JsonProperty("quotes")]
        public List<STC.SalesTrading.Plugin.Gateway.Contracts.Quote.QuoteRecord> Quotes { get; set; }
    }

    public class NotificationRecord
    {
        [JsonProperty("notificationId")]
        public string NotificationId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("reasons")]
        public List<NotificationReason> Reasons { get; set; }

        /// <summary>服务端生成的提醒文案。</summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>文案令牌（content 变化标记，后端新增；暂无 UI 消费）。</summary>
        [JsonProperty("contentToken")]
        public string ContentToken { get; set; }

        /// <summary>提醒键（后端新增，显示/定位辅助键）。</summary>
        [JsonProperty("alertKey")]
        public string AlertKey { get; set; }

        /// <summary>是否有效（后端新增；false = 提醒已失效可过滤）。</summary>
        [JsonProperty("active")]
        public bool? Active { get; set; }

        /// <summary>通知版本（后端新增）。</summary>
        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("quoteIds")]
        public List<string> QuoteIds { get; set; }

        /// <summary>null 表示未读。</summary>
        [JsonProperty("triggers")]
        public List<NotificationTrigger> Triggers { get; set; }

        [JsonProperty("readTime")]
        public DateTime? ReadTime { get; set; }

        [JsonProperty("createTime")]
        public DateTime? CreateTime { get; set; }

        /// <summary>列表排序键（updateTime DESC, notificationId ASC）。</summary>
        [JsonProperty("updateTime")]
        public DateTime? UpdateTime { get; set; }
    }

}
