using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>批量写入单项失败的错误明细（如 UNRESOLVED_COUNTERPARTY 及待补齐字段）。</summary>
    public class QuoteError
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("field")]
        public string Field { get; set; }
    }

}
