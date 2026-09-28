using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 债券代码查询结果（关注债券管理弹窗的识别回填）。
    /// Mock 实现在进程内行情集合中按代码查找；真实实现对应原型 POST /parse（mode=bond, lookup）。
    /// </summary>
    public class BondLookupEntry
    {
        /// <summary>查询的代码（原样回传）。</summary>
        public string Code { get; set; }

        /// <summary>债券库是否命中；未命中时行按半透明展示，仍可勾选入组。</summary>
        public bool Found { get; set; }

        /// <summary>债券简称。</summary>
        public string Name { get; set; }

        /// <summary>剩余期限（如 2.15Y）。</summary>
        public string Tenor { get; set; }

        /// <summary>债券库。</summary>
        public string BondLibrary { get; set; }

        /// <summary>中债估值（含 % 的展示文本）。</summary>
        public string Valuation { get; set; }

        /// <summary>所属市场；未命中时为 null（市场校验跳过）。</summary>
        public BondMarket? Market { get; set; }
    }
}
