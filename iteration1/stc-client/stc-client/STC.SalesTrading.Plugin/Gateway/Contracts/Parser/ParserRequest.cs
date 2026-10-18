using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Parser
{
    /// <summary>
    /// STC 老识别服务请求（ParserSvr / stc.parser.bond）。
    /// 注意：非 FICC 信封——应答为 Code/Msg/Datas 平铺、请求无 requestId。
    /// </summary>
    public class ParserRequest
    {
        /// <summary>STC 登录用户名（ParserSvr 必填）。</summary>
        [JsonProperty("userName")]
        public string UserName { get; set; }

        /// <summary>待识别的报价原文。</summary>
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
