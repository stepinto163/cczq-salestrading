using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Base
{

    /// <summary>ficc.base.get 请求体（H42）：按 ID 批量读取基础资料/CRM 记录。</summary>
    public class BaseGetRequest
    {
        /// <summary>SECURITY / ISSUER / INSTITUTION / CONTACT。</summary>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("ids")]
        public List<string> Ids { get; set; }
    }

}
