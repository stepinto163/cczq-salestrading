using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>IntakeIdentity</summary>
    public class IntakeIdentity : QuoteSource
    {
        [JsonProperty("token")]
        public string Token { get; set; }

    }
}
