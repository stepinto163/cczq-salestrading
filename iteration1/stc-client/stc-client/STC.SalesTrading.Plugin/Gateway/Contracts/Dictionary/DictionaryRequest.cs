using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Dictionary
{
    /// <summary>ficc.quote.dictionary.query 请求体（H14）。names 可空 = 返回全部字典与 filterFields。</summary>
    public class DictionaryRequest
    {
        [JsonProperty("names")]
        public System.Collections.Generic.List<string> Names { get; set; }
    }
}
