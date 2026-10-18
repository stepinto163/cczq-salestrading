using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Dictionary
{
    /// <summary>字典/枚举候选（H14：显示 label、发送 value）。</summary>
    public class DictionaryOption
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
