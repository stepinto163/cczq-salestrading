using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>三域 view.query 请求体（H17/H52/H27）。不创建新订阅，返回该时点指定页。</summary>
    public class ViewQueryRequest
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        /// <summary>从 1 开始。</summary>
        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
    }

}
