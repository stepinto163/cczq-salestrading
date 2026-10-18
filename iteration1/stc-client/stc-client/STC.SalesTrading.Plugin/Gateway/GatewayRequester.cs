using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions.Gateway;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// FICC 传输封装。职责边界：只做信封装配、requestId 注入、Code 校验与
    /// 「对象 ↔ JSON 字符串」转换，不含任何业务语义。
    /// <para>全程强类型、不出 JObject：请求 DTO 序列化为 string 直通 netapi（GWApi.Request
    /// 对 string payload 直接透传）；应答同样取 string，在本类内 <see cref="ReadReply{T}"/>
    /// 反序列化为强类型信封 <see cref="ApiEnvelope{T}"/>；推送由订阅侧拿到 JSON 字符串后
    /// 再由调用方反序列化。业务层永远只见 DTO。</para>
    /// <para>序列化策略：默认 <see cref="NullValueHandling.Ignore"/>（不赋值 =
    /// 不发字段 = 服务端保留原值）；可清空字段在 DTO 上逐字段标注 Include。</para>
    /// <para>关于 Newtonsoft 来源：本插件**不另引** client\bin 的 Newtonsoft.Json 程序集——
    /// gateway-api 已内联该类型，netapi 的 GWApi 用的就是那一份；再引宿主 bin 的强命名版会
    /// CS0433 二义性，且 requestSync 的泛型反序列化会落在我们看不到的那一份上。
    /// 后端自己的 C# 客户端同样只引 gateway-api。</para>
    /// </summary>
    internal sealed class GatewayRequester
    {
        /// <summary>
        /// 全局序列化设置。
        /// 默认 Ignore：与后端「未传字段保留」的写入语义对齐；
        /// 需要显式提交 null 以清空的字段，在 DTO 上用
        /// <c>[JsonProperty(NullValueHandling = NullValueHandling.Include)]</c> 逐字段覆盖。
        /// CamelCase resolver：DTO 属性 PascalCase ↔ 报文 lowerCamelCase 自动映射，
        /// DTO 只在 STC 风格字段（SecurityID/NoMDEntries 等）上显式标注。
        /// </summary>
        internal static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

        /// <summary>请求超时，与 netapi 现有 5 秒约定一致。</summary>
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);

        private readonly IGatewayClient gateway;

        /// <summary>网关登录身份显示名（顶栏/个人中心用；未登录为空串）。</summary>
        public string CurrentUserName
        {
            get { return gateway.CurrentUserName; }
        }

        internal GatewayRequester(IGatewayClient gateway)
        {
            this.gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        }

        /// <summary>
        /// 请求-应答。自动注入 requestId，并校验 Code 与应答内容。
        /// 批量接口的 <c>results[].Code</c> 逐条结果不在此层拦截，由调用方按需汇总——
        /// 规范约定顶层 0000 仅表示批量请求完成，单条成败必须逐项判断。
        /// </summary>
        internal async Task<TData> RequestAsync<TReq, TData>(string topic, TReq request, CancellationToken ct, string requestId = null)
            where TReq : class
            where TData : class
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 规范 §2.2：请求体 = requestId 与业务字段【平铺在顶层】（如 {"requestId","kind"}）；
            // 只有应答才有 {"Code","Msg","requestId","Data"} 壳（ApiEnvelope）。
            // 此处把 requestId 注入业务对象顶层后整体序列化；JObject 仅作传输层装配（不出本类）。
            var payload = JObject.FromObject(request, JsonSerializer.Create(JsonSettings));
            payload["requestId"] = requestId ?? "STC-" + Guid.NewGuid().ToString("N");

            // payload 用 string：netapi 的 GWApi.Request 对 string 直接透传（不序列化），
            // 应答同样取 string —— 反序列化完全在本类内发生，不经 netapi 的 requestSync<T>，
            // 也就不受它内部那份 Newtonsoft 影响。
            var reply = await gateway.RequestAsync<string>(new GatewayRequest
            {
                ServiceId = GatewayTopics.ServiceFor(topic),
                Topic = topic,
                Payload = payload.ToString(Newtonsoft.Json.Formatting.None),
                Timeout = RequestTimeout
            }, ct).ConfigureAwait(false);

            if (!reply.Success || string.IsNullOrWhiteSpace(reply.Data))
            {
                throw new GatewayApiException("gateway",
                    "网关请求失败（" + topic + "）："
                    + (reply.Success ? "服务未返回应答" : reply.ErrorCode + " " + reply.ErrorMessage));
            }

            var answer = ReadReply<ApiEnvelope<TData>>(topic, reply.Data);
            if (answer == null)
            {
                throw new GatewayApiException("gateway", "服务未返回应答：" + topic);
            }
            if (answer.Code != "0000")
            {
                throw BuildError(answer.Code, answer.Msg);
            }
            return answer.Data;
        }

        /// <summary>
        /// STC 老协议旁路（ParserSvr stc.parser.bond 等）：请求平铺序列化（不注入 requestId）、
        /// 应答不按 FICC 信封解包，直接反序列化为 TReply（Code/Msg 校验由调用方按老壳处理）。
        /// 路由仍走 ServiceFor（stc.parser.* → ParserSvr）。
        /// </summary>
        internal async Task<TReply> RequestLegacyAsync<TReq, TReply>(string topic, TReq request, CancellationToken ct)
            where TReq : class
            where TReply : class
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var payload = JObject.FromObject(request, JsonSerializer.Create(JsonSettings));
            var reply = await gateway.RequestAsync<string>(new GatewayRequest
            {
                ServiceId = GatewayTopics.ServiceFor(topic),
                Topic = topic,
                Payload = payload.ToString(Newtonsoft.Json.Formatting.None),
                Timeout = RequestTimeout
            }, ct).ConfigureAwait(false);

            if (!reply.Success || string.IsNullOrWhiteSpace(reply.Data))
            {
                throw new GatewayApiException("gateway",
                    "网关请求失败（" + topic + "）："
                    + (reply.Success ? "服务未返回应答" : reply.ErrorCode + " " + reply.ErrorMessage));
            }
            return ReadReply<TReply>(topic, reply.Data);
        }

        /// <summary>把后端应答 JSON 字符串反序列化为强类型；空应答抛异常而非返回 null 让上层踩空。</summary>
        internal static T ReadReply<T>(string topic, string json)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new GatewayApiException("gateway", "服务未返回应答：" + topic);
            }
            try
            {
                return JToken.Parse(json).ToObject<T>(JsonSerializer.Create(JsonSettings));
            }
            catch (Exception error)
            {
                throw new GatewayApiException("gateway", "应答解析失败（" + topic + "）：" + error.Message);
            }
        }

        /// <summary>订阅 DPS 推送。回调给出原始 JSON 字符串，由调用方反序列化后使用。</summary>
        internal IDisposable Subscribe(string topic, bool withImage, Action<string> onMessage)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (onMessage == null) throw new ArgumentNullException(nameof(onMessage));

            return gateway.Subscribe(new GatewaySubscription
            {
                ServiceId = GatewayTopics.ServiceFor(topic),
                Topics = new[] { topic },
                IncludeImage = withImage
            }, message =>
            {
                // netapi GWApi 的回调对象是 NetMessage{Topic, Data, SendTime}（Data 为 JSON 字符串）；
                // Payload 直为 string 的形态仅是兼容预留
                var payload = message.Payload as string;
                if (payload == null)
                {
                    var netMessage = message.Payload as netapi.Api.NetMessage;
                    payload = netMessage != null ? netMessage.Data : null;
                }
                onMessage(payload);
            });
        }

        /// <summary>把后端错误项转成带 Code 的异常；error.type / error.field 由调用方补充。</summary>
        internal static GatewayApiException BuildError(string code, string message, string errorType = null, string errorField = null)
        {
            return new GatewayApiException(code, message, errorType, errorField);
        }
    }
}
