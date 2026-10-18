using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>FICC 应答信封，成功 Data 的类型由 Endpoint 确定。</summary>
    public class ApiReply<T> : Reply where T : ProtocolModel
    {
        [JsonProperty("Data")]
        public T Data { get; set; }
    }
}
