using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteHistorySearchData</summary>
    public class QuoteHistorySearchData : PageData
    {
        [JsonProperty("rows")]
        public List<QuoteHistoryRow> Rows { get; set; }

    }
}
