using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteChangeRequest</summary>
    public class QuoteChangeRequest : Request
    {
        [JsonProperty("quotes")]
        public List<QuoteChangeItem> Quotes { get; set; }

    }
}
