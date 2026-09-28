namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>模糊报价找券查询条件（原型 POST /api/bond-filter/query 概要）。</summary>
    public class FuzzyQuery
    {
        /// <summary>原始报价文本。</summary>
        public string Text { get; set; }

        /// <summary>机构。</summary>
        public string Institution { get; set; }

        /// <summary>交易员。</summary>
        public string Trader { get; set; }

        /// <summary>分页偏移。</summary>
        public int Offset { get; set; }

        /// <summary>每页条数（原型 50 条/页）。</summary>
        public int Limit { get; set; }
    }
}
