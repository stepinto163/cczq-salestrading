using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>
    /// 识别问题项（H40 应答 issues[]）。元素形状后端待实现（H40 未定稿），字段容错全可空；
    /// UI 只做状态栏文本提示，不做行级推断。
    /// </summary>
    public class RecognizeIssue
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
