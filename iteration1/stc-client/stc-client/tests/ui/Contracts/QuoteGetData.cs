using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteGetData</summary>
    public class QuoteGetData : ProtocolModel
    {
        [JsonProperty("quote")]
        public QuoteRecord Quote { get; set; }

    }
}
