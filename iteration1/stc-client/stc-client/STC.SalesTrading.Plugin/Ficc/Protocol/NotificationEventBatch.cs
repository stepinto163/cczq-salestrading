using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationEventBatch</summary>
    public class NotificationEventBatch : ProtocolModel
    {
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("epoch")]
        public string Epoch { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("currentSequence")]
        public string CurrentSequence { get; set; }

        [JsonProperty("fromSequence")]
        public string FromSequence { get; set; }

        [JsonProperty("toSequence")]
        public string ToSequence { get; set; }

        [JsonProperty("nextSequence")]
        public string NextSequence { get; set; }

        [JsonProperty("seenSequence")]
        public string SeenSequence { get; set; }

        [JsonProperty("hasMore")]
        public bool? HasMore { get; set; }

        [JsonProperty("events")]
        public List<NotificationEvent> Events { get; set; }

    }
}
