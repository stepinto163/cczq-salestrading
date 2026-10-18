using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>stream.open 应答 Data（H15/H50/H25）：私有推送 topic 与流标识。</summary>
    public class StreamOpenData
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        /// <summary>服务端下发的订阅 topic，禁止客户端拼接。</summary>
        [JsonProperty("pushTopic")]
        public string PushTopic { get; set; }

        [JsonProperty("streamId")]
        public string StreamId { get; set; }
    }

}
