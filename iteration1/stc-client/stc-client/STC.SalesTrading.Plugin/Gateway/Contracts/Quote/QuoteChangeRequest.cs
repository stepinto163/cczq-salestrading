using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>ficc.quote.confirm / update 的字段变更请求体。</summary>
    public class QuoteChangeRequest
    {
        [JsonProperty("quotes")]
        public List<QuoteChangeItem> Quotes { get; set; }
    }

    /// <summary>Refer、Unrefer、删除仅携带身份与版本，不能混入显式清空的编辑字段。</summary>
    public class QuoteLifecycleRequest
    {
        [JsonProperty("quotes")]
        public List<QuoteLifecycleItem> Quotes { get; set; }
    }

    public class QuoteLifecycleItem
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
