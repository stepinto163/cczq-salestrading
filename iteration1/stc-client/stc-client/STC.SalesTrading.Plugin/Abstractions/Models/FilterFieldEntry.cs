using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>筛选字段目录项（ficc.quote.dictionary.query 的 filterFields[]，H14）。</summary>
    public class FilterFieldEntry
    {
        /// <summary>字段名（条件落库的 field，唯一事实源）。</summary>
        public string Field { get; set; }

        /// <summary>中文显示名。</summary>
        public string Label { get; set; }

        /// <summary>值类型（enum/classification/text/number/date 等，契约 §filterFields）。</summary>
        public string Type { get; set; }

        /// <summary>该字段允许的运算符（EQ/IN/CONTAINS/BETWEEN/EXISTS…）。</summary>
        public IReadOnlyList<string> Operators { get; set; }

        /// <summary>分类/枚举字段的候选值（显示 label、发送 value）。</summary>
        public IReadOnlyList<FilterEnumOption> EnumValues { get; set; }

        /// <summary>分组名（报价/债券/流动性/信用/条款/区域 → 目录分区标题）。</summary>
        public string Group { get; set; }

        /// <summary>是否启用（后端下发的不可用字段不进目录）。</summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>枚举候选（显示 label、发送 value）。</summary>
    public class FilterEnumOption
    {
        public string Value { get; set; }

        public string Label { get; set; }
    }
}
