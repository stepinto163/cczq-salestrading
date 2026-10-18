using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>ficc.quote.recognize 应答 Data（H40）。不能识别的字段返回 issues，不用估值替用户输入价格。</summary>
    public class RecognizeData
    {
        [JsonProperty("quotes")]
        public List<RecognizeQuote> Quotes { get; set; }

        [JsonProperty("issues")]
        public List<RecognizeIssue> Issues { get; set; }

        [JsonProperty("referenceVersion")]
        public string ReferenceVersion { get; set; }
    }
}
