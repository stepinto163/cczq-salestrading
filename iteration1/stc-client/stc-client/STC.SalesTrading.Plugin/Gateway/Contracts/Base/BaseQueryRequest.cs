using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Base
{

    /// <summary>
    /// ficc.base.query 请求体（H43）。SECURITY/ISSUER 用 text+cursor 续取；
    /// CRM（INSTITUTION/CONTACT）用 text+offset 分页，CONTACT 可带 institutionId 限定。
    /// </summary>
    public class BaseQueryRequest
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary>包含搜索（不区分大小写）；空串浏览列表。</summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>仅 kind=CONTACT：限定联系人所属机构。</summary>
        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        /// <summary>仅 CRM 查询；默认 0。</summary>
        [JsonProperty("offset")]
        public int? Offset { get; set; }

        /// <summary>默认 200，范围 1—200（界面建议 50）。</summary>
        [JsonProperty("limit")]
        public int? Limit { get; set; }

        /// <summary>仅 SECURITY/ISSUER 续取。</summary>
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }

}
