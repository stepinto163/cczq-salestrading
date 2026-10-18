using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Parser
{
    /// <summary>
    /// STC 老识别服务应答（ParserSvr / stc.parser.bond）：Code/Msg/Datas 平铺，
    /// 非标准 FICC 信封（无 requestId/Data 壳）。Code 大写开头，"0000" 为成功。
    /// </summary>
    public class ParserReply
    {
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("Warnings")]
        public List<string> Warnings { get; set; }

        [JsonProperty("Datas")]
        public List<ParserQuote> Datas { get; set; }
    }
}
