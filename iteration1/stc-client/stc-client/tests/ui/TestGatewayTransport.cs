using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions.Gateway;
using STC.SalesTrading.Plugin.Gateway;

namespace SalesTrading.UiTests
{
    // 断言使用独立协议模型，通过当前登录会话读取真实服务；不创建替代业务服务。
    internal sealed class TestGatewayTransport
    {
        private readonly ApiManagerGatewayClient gateway = new ApiManagerGatewayClient();

        public async Task<JObject> RequestAsync(string service, string topic, JObject request)
        {
            var response = await gateway.RequestAsync<string>(new GatewayRequest
            {
                ServiceId = service,
                Topic = topic,
                Payload = request.ToString(Newtonsoft.Json.Formatting.None),
                Timeout = TimeSpan.FromSeconds(10)
            }, CancellationToken.None);
            if (!response.Success)
            {
                throw new InvalidOperationException("测试请求失败：" + service + "/" + topic + " " + response.ErrorMessage);
            }
            return JObject.Parse(response.Data);
        }
    }
}
