using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>批量写入（create/confirm/update/revoke/delete）单项结果；Code 逐条判断，顶层 0000 只代表批量完成。</summary>
    public class QuoteOperationResult
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

        /// <summary>操作成功后的新版本。</summary>
        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("error")]
        public QuoteError Error { get; set; }
    }

}
