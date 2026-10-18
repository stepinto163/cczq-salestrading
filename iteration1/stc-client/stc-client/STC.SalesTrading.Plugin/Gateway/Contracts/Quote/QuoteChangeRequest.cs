using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>ficc.quote.confirm / update / revoke / delete 共用请求体（H02—H07）。</summary>
    public class QuoteChangeRequest
    {
        [JsonProperty("quotes")]
        public List<QuoteChangeItem> Quotes { get; set; }
    }

}
