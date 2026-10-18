using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>price 域 view.open / view.query 应答 Data（H51/H52）。禁止 quotes/reports 字段。</summary>
    public class PriceViewData
    {
        [JsonProperty("streamId")]
        public string StreamId { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("view")]
        public ViewPage View { get; set; }

        [JsonProperty("books")]
        public List<PriceBookRow> Books { get; set; }
    }

}
