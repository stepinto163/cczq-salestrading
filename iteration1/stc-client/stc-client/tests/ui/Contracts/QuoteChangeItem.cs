using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>修改/确认/撤销等条目；未提供保留，显式 null 清除，false 关闭标记。</summary>
    public class QuoteChangeItem : QuoteInput
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

    }
}
