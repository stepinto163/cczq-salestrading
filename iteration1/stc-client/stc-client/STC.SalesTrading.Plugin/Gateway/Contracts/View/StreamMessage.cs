using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>
    /// 私有 report topic 推送公共壳（S01/S02/S03）。三域各自继承并携带业务数组；
    /// 首个 HEARTBEAT 只有序号无业务数组。sequence 为字符串、严格递增（含 HEARTBEAT）。
    /// </summary>
    public class StreamMessage
    {
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        /// <summary>UPDATE / HEARTBEAT 等。</summary>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("streamId")]
        public string StreamId { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("views")]
        public List<ViewDelta> Views { get; set; }

        public const string KindHeartbeat = "HEARTBEAT";
    }

}
