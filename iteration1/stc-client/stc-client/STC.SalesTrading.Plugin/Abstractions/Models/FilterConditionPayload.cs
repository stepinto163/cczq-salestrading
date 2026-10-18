using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 结构化筛选条件载荷（UI 条件与后端协议之间的编解码产物：field + operator + value）。
    /// value 按契约是标量或数组（EQ/CONTAINS/IN/BETWEEN/LT/LTE/GT/GTE…见规范过滤库）。
    /// 由 <see cref="STC.SalesTrading.Plugin.Shared.Services.GroupConditionCatalog"/> 生成并随 UI 条件缓存
    /// （保存直接透传、读回直接显示），网关层仅做映射。
    /// </summary>
    public class FilterConditionPayload
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }
    }
}
