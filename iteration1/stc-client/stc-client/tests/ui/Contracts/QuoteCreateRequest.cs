using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteCreateRequest</summary>
    public class QuoteCreateRequest : Request
    {
        [JsonProperty("quotes")]
        public List<QuoteCreateItem> Quotes { get; set; }

        [JsonProperty("intake")]
        public IntakeIdentity Intake { get; set; }

    }
}
