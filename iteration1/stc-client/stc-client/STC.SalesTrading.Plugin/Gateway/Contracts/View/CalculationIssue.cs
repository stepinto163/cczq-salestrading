using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{
    /// <summary>价格簿计算异常条目（R8：最优计算未就绪的字段级提示；calculationIssues[] 元素）。</summary>
    public class CalculationIssue
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
