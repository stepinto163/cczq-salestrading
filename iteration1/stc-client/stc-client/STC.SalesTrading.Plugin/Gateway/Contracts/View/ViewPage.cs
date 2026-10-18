using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>view 元数据（view.open / view.query 应答内的 view 对象）。</summary>
    public class ViewPage
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("pageNumber")]
        public int? PageNumber { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        /// <summary>完整过滤结果数，不是当前页条数。</summary>
        [JsonProperty("totalCount")]
        public int? TotalCount { get; set; }

        /// <summary>本页行标识（quoteId / securityId / notificationId）。</summary>
        [JsonProperty("ids")]
        public List<string> Ids { get; set; }

        /// <summary>还有下一页（后端新增便捷位；null 视为按 totalCount 判断）。</summary>
        [JsonProperty("hasMore")]
        public bool? HasMore { get; set; }
    }

}
