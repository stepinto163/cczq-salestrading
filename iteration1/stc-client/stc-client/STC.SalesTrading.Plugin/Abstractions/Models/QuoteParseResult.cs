using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>精确报价识别结果（原型 POST /api/parse 出参概要）。</summary>
    public class QuoteParseResult
    {
        /// <summary>识别出的结构化报价行。</summary>
        public IReadOnlyList<QuoteConfirmRow> Rows { get; set; }

        /// <summary>识别警告（字段缺失、候选不确定等）。</summary>
        public IReadOnlyList<string> Warnings { get; set; }

        /// <summary>识别耗时。</summary>
        public TimeSpan Elapsed { get; set; }

        /// <summary>成功条数。</summary>
        public int SucceededCount { get; set; }

        /// <summary>失败条数。</summary>
        public int FailedCount { get; set; }
    }
}
