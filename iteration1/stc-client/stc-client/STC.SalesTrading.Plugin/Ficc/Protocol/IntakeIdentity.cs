using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>IntakeIdentity</summary>
    public class IntakeIdentity : QuoteSource
    {
        [JsonProperty("token")]
        public string Token { get; set; }

    }
}
