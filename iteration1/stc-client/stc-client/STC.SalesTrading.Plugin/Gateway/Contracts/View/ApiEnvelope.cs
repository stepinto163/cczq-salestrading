using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{
    /// <summary>
    /// FICC 统一应答壳（规范 §2.2）：{"Code","Msg","requestId","Data"}。
    /// 注意：请求体没有此壳——requestId 与业务字段平铺在顶层（见 GatewayRequester.RequestAsync）。
    /// 序列化类型取自 gateway-api 内联的 Newtonsoft.Json（见 GatewayRequester 说明）。
    /// </summary>
    public sealed class ApiEnvelope<T>
    {
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("Data")]
        public T Data { get; set; }
    }
}
