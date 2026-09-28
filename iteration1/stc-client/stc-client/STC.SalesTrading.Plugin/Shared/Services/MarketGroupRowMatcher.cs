using System;
using System.Collections.Generic;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 筛选分组条件对看板行的客户端求值（原型 marketGroupFilterPayload → POST /bond-filter/query 的本地近似）。
    /// 语义：分组内所有子分组条件按 key 分桶（key 内任一值命中即可、key 之间 AND）。
    /// 行上可求值的 key：rt（剩余期限档/自定义区间）、mkt（代码后缀→上市场所）；
    /// 其余 key（tag/ent/type/city…）看板行没有对应属性，**忽略**——后端分组查询契约到位后
    /// 整体替换为服务端命中集比对（原型 marketGroupMatchIds 方案）。
    /// </summary>
    public static class MarketGroupRowMatcher
    {
        /// <summary>rawCode 为带后缀原始代码（场所判断依赖后缀）；maturity 形如 "7.83Y"。</summary>
        public static bool Matches(string rawCode, string maturity, FilterNamedGroup group)
        {
            if (group == null) return true;

            // 展平全部子分组条件，按 key 分桶去重（原型 flattenMarketSubgroups + normalizedMarketCondition）
            var buckets = new Dictionary<string, HashSet<string>>();
            foreach (var subgroup in group.Subgroups)
            {
                foreach (var condition in subgroup.Conditions)
                {
                    if (string.IsNullOrEmpty(condition.Key) || string.IsNullOrEmpty(condition.Value)) continue;
                    HashSet<string> values;
                    if (!buckets.TryGetValue(condition.Key, out values))
                    {
                        values = new HashSet<string>();
                        buckets[condition.Key] = values;
                    }
                    values.Add(condition.Value);
                }
            }
            if (buckets.Count == 0) return true;

            foreach (var bucket in buckets)
            {
                if (!BucketMatches(bucket.Key, bucket.Value, rawCode, maturity))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool BucketMatches(string key, HashSet<string> values, string rawCode, string maturity)
        {
            switch (key)
            {
                case "rt":
                    var years = ParseYears(maturity);
                    if (!years.HasValue) return false;
                    foreach (var value in values)
                    {
                        if (TenorValueMatches(value, years.Value)) return true;
                    }
                    return false;

                case "mkt":
                    var venue = VenueOfCode(rawCode);
                    if (venue == null) return false;
                    return values.Contains(venue);

                default:
                    // 行上无该属性的 key：忽略（视为通过），见类注释
                    return true;
            }
        }

        /// <summary>"7.83Y" / "0.92Y" → 年；解析失败返回 null。</summary>
        internal static decimal? ParseYears(string maturity)
        {
            if (string.IsNullOrEmpty(maturity)) return null;
            maturity = maturity.Trim();
            if (maturity.EndsWith("Y", StringComparison.OrdinalIgnoreCase))
            {
                maturity = maturity.Substring(0, maturity.Length - 1);
            }
            else if (maturity.EndsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                maturity = maturity.Substring(0, maturity.Length - 1);
                decimal months;
                if (decimal.TryParse(maturity, out months)) return months / 12m;
                return null;
            }
            else if (maturity.EndsWith("D", StringComparison.OrdinalIgnoreCase))
            {
                maturity = maturity.Substring(0, maturity.Length - 1);
                decimal days;
                if (decimal.TryParse(maturity, out days)) return days / 365m;
                return null;
            }

            decimal result;
            return decimal.TryParse(maturity, out result) ? result : (decimal?)null;
        }

        /// <summary>期限条件值匹配：11 档标签（&lt;3M…&gt;20Y）或自定义区间 "min-max"（年）。</summary>
        internal static bool TenorValueMatches(string value, decimal years)
        {
            value = (value ?? string.Empty).Trim();
            if (value.Length == 0) return false;

            if (value.StartsWith("<"))
            {
                decimal bound;
                return TryParseTenorBound(value.Substring(1), out bound) && years < bound;
            }
            if (value.StartsWith(">"))
            {
                decimal bound;
                return TryParseTenorBound(value.Substring(1), out bound) && years > bound;
            }

            var dash = value.IndexOf('-');
            if (dash > 0)
            {
                // "3-6M"（档位）或 "1-3"（自定义区间，年；AddCustomRange 已折算）
                var left = value.Substring(0, dash);
                var right = value.Substring(dash + 1);
                decimal min, max;
                var hasMin = TryParseTenorBound(left, out min);
                var hasMax = TryParseTenorBound(right, out max);
                if (!hasMin && !hasMax) return false;
                if (hasMin && years < min) return false;
                if (hasMax && years > max) return false;
                return true;
            }

            return false;
        }

        /// <summary>"3M"→0.25、"6Y"→6、"3"→3（无单位视为年）；空串视为开区间端（返回 false 表示无界）。</summary>
        private static bool TryParseTenorBound(string text, out decimal value)
        {
            text = (text ?? string.Empty).Trim();
            value = 0m;
            if (text.Length == 0) return false;
            var unit = 1m;
            var last = text[text.Length - 1];
            if (last == 'M' || last == 'm') { unit = 1m / 12m; text = text.Substring(0, text.Length - 1); }
            else if (last == 'D' || last == 'd') { unit = 1m / 365m; text = text.Substring(0, text.Length - 1); }
            else if (last == 'Y' || last == 'y') { text = text.Substring(0, text.Length - 1); }

            decimal number;
            if (text.Length == 0 || !decimal.TryParse(text, out number)) return false;
            value = number * unit;
            return true;
        }

        /// <summary>带后缀原始代码 → 上市场所（.IB 银行间 / .SH 上交所 / .SZ 深交所 / .BJ 北交所）。</summary>
        internal static string VenueOfCode(string rawCode)
        {
            var code = (rawCode ?? string.Empty).Trim().ToUpperInvariant();
            if (code.EndsWith(".IB")) return "银行间";
            if (code.EndsWith(".SH")) return "上交所";
            if (code.EndsWith(".SZ")) return "深交所";
            if (code.EndsWith(".BJ")) return "北交所";
            return null;
        }
    }
}
