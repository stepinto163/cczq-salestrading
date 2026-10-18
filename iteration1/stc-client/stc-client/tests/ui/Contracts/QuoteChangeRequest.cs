using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteChangeRequest</summary>
    public class QuoteChangeRequest : Request
    {
        [JsonProperty("quotes")]
        public List<QuoteChangeItem> Quotes { get; set; }

    }
}
