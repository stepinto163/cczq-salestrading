using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>自然语言识别请求（ficc.quote.recognize，H40）。text 必填，最大 20000 字符。</summary>
    public class RecognizeRequest
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>识别上下文（如交易日期），可空。</summary>
        [JsonProperty("context")]
        public RecognizeContext Context { get; set; }
    }
}
