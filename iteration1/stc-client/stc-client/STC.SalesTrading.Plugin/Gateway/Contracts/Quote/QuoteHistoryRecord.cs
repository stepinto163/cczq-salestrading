using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>历史报价行（ficc.quote.history.search，H11）。行粒度见规范 R1 评审项。</summary>
    public class QuoteHistoryRecord
    {
        /// <summary>服务端分配的行键。</summary>
        [JsonProperty("rowKey")]
        public string RowKey { get; set; }

        /// <summary>业务日期 yyyy-MM-dd，UI 日终分组键。</summary>
        [JsonProperty("businessDate")]
        public string BusinessDate { get; set; }

        /// <summary>CREATE / UPDATE / CONFIRM / REFER / UNREFER / DELETE。</summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("quote")]
        public QuoteRecord Quote { get; set; }

        /// <summary>交易日期（后端新形状；与 BusinessDate 二选一容错）。</summary>
        [JsonProperty("tradeDate")]
        public string TradeDate { get; set; }

        /// <summary>行级债券资料（后端新形状自带估值）。</summary>
        [JsonProperty("reference")]
        public View.QuoteReference Reference { get; set; }
    }

}
