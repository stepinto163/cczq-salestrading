using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>ParserReply</summary>
    public class ParserReply : ProtocolModel
    {
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("Warnings")]
        public List<string> Warnings { get; set; }

        [JsonProperty("Datas")]
        public List<ParserQuote> Datas { get; set; }

    }
}
