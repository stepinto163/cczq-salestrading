using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>ficc.price.report 推送批次（S03 独立 price 域）。books 为变化券的完整价格簿。</summary>
    public class PriceStreamBatch : StreamMessage
    {
        [JsonProperty("books")]
        public List<PriceBookRow> Books { get; set; }
    }

}
