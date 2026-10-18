using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Base
{

    /// <summary>
    /// ficc.base.query / ficc.base.get 应答 Data（H42/H43）。
    /// SECURITY/ISSUER 返回 totalCount+nextCursor；CRM 返回 hasMore+nextOffset——两组字段按 kind 二选一。
    /// </summary>
    public class BaseQueryData
    {
        [JsonProperty("records")]
        public List<BaseRecord> Records { get; set; }

        [JsonProperty("totalCount")]
        public int? TotalCount { get; set; }

        [JsonProperty("nextCursor")]
        public string NextCursor { get; set; }

        [JsonProperty("hasMore")]
        public bool? HasMore { get; set; }

        [JsonProperty("nextOffset")]
        public int? NextOffset { get; set; }
    }

}
