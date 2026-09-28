namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>模糊报价候选债券（原型右栏表格，5 列）。</summary>
    public class FuzzyCandidate
    {
        /// <summary>剩余期限（m）。</summary>
        public string Maturity { get; set; }

        /// <summary>债券代码（c）。</summary>
        public string Code { get; set; }

        /// <summary>债券简称（n）。</summary>
        public string Name { get; set; }

        /// <summary>债券库。</summary>
        public string BondLibrary { get; set; }

        /// <summary>中债估值。</summary>
        public decimal? NzValuation { get; set; }
    }
}
