using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>三域 view.close 请求体（H18/H53/H28）。重复关闭成功。</summary>
    public class ViewCloseRequest
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }
    }

}
