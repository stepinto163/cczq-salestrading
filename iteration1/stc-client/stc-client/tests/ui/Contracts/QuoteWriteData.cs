using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteWriteData</summary>
    public class QuoteWriteData : ProtocolModel
    {
        [JsonProperty("results")]
        public List<QuoteOperationResult> Results { get; set; }

    }
}
