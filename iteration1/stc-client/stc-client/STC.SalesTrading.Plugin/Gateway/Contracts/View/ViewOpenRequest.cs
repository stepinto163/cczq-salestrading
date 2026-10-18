using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>
    /// 三域 view.open 请求体（H16/H51/H26）。viewId 由客户端生成且在本领域连接内永不复用。
    /// Mode/Depth 仅 price 域使用（quote/notification 域禁止，序列化 Ignore 不发）。
    /// </summary>
    public class ViewOpenRequest
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("filter")]
        public QuoteFilter Filter { get; set; }

        /// <summary>默认 200，范围 1—1000。</summary>
        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        /// <summary>仅 price 域：BEST / FULL / DEPTH。</summary>
        [JsonProperty("mode")]
        public string Mode { get; set; }

        /// <summary>仅 mode=DEPTH 时必填的正整数。</summary>
        [JsonProperty("depth")]
        public int? Depth { get; set; }

        /// <summary>仅 quote 域：状态约束（同 quote.query，如 ["ACTIVE"]）。行情/通知域禁止。</summary>
        [JsonProperty("status")]
        public List<string> Status { get; set; }

        /// <summary>仅 quote 域：MINE / VISIBLE（同 quote.query）。</summary>
        [JsonProperty("ownerScope")]
        public string OwnerScope { get; set; }

        /// <summary>仅 notification 域：ALL / READ / UNREAD（H26）。</summary>
        [JsonProperty("readState")]
        public string ReadState { get; set; }
    }

}
