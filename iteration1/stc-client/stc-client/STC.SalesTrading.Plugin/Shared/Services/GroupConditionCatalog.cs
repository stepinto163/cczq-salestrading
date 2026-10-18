using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Shared.Components;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using UiCondition = STC.SalesTrading.Plugin.Shared.ViewModel.FilterCondition;
using MarketKind = STC.SalesTrading.Plugin.Shared.Services.FilterProfileService.MarketKind;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 筛选分组维护窗的条件目录（对齐 reviewed-market-detail 的原型语义）：
    /// 面板结构（分区标题/条目顺序/控件形态 = 原型侧栏）× 服务端字典（label/候选/置灰）的合成。
    /// 同时承担条件编解码：<see cref="Compile"/> 把 UI chip 编译成结构化协议条件（保存透传）、
    /// <see cref="Display"/> 把协议条件翻译回中文 chip（读回直接显示）——
    /// 文本 CONTAINS / 区间 BETWEEN(LT/LTE/GT/GTE，M/D 折算年) / 布尔 EQ / 枚举 IN。
    /// 目录按市场缓存（弹窗低频，面板 Measure 一次）。
    /// </summary>
    public sealed class GroupConditionCatalog
    {
        private static readonly Dictionary<MarketKind, GroupConditionCatalog> cache =
            new Dictionary<MarketKind, GroupConditionCatalog>();

        /// <summary>最近一次构建失败原因（诊断用；成功后清空）。</summary>
        public static string LastFailReason { get; private set; }

        private readonly Dictionary<string, FilterFieldEntry> fields;
        private readonly Dictionary<MarketKind, List<CatalogSection>> catalogs = new Dictionary<MarketKind, List<CatalogSection>>();

        private GroupConditionCatalog(IReadOnlyList<FilterFieldEntry> entries)
        {
            fields = entries.ToDictionary(field => field.Field);
        }

        /// <summary>取（或懒建）某市场的目录；字典不可用/无字段时返回 null（调用方回退本地目录）。</summary>
        public static GroupConditionCatalog TryGet(MarketKind market, IGroupDirectory directory)
        {
            lock (cache)
            {
                if (cache.TryGetValue(market, out var cached)) return cached;
            }
            IReadOnlyList<FilterFieldEntry> entries;
            try
            {
                entries = directory.GetFilterFields(market);
            }
            catch (Exception ex)
            {
                LastFailReason = "字典加载失败：" + ex.Message;
                return null;
            }
            if (entries == null || entries.Count == 0)
            {
                LastFailReason = "字典未返回筛选字段";
                return null;
            }

            var catalog = new GroupConditionCatalog(entries);
            try
            {
                catalog.Build(market);
            }
            catch (Exception ex)
            {
                LastFailReason = "目录构建失败：" + ex.Message;
                return null;
            }
            LastFailReason = null;
            lock (cache)
            {
                cache[market] = catalog;
            }
            return catalog;
        }

        /// <summary>字典/面板结构变化后失效（登录切换等）。</summary>
        public static void Invalidate()
        {
            lock (cache)
            {
                cache.Clear();
            }
        }

        public List<CatalogSection> ForMarket(MarketKind market) { return catalogs[market]; }

        /// <summary>利率市场不提供的字段（后端无数据，目录与保存一并拦截）。</summary>
        public static bool Prohibited(MarketKind market, string field)
        {
            return market == MarketKind.Rate
                && new[] { "localGovernmentRegion", "issuerRating", "hotTags", "marketMakerTags", "perpetualType", "isPerpetual" }.Contains(field);
        }

        // ============ 目录构建：面板结构 × 字典数据 ============

        private void Build(MarketKind market)
        {
            // 市场 ↔ 面板一一对应（远端同款）：信用目录取信用面板结构、利率目录取利率面板结构
            Build(market, market == MarketKind.Credit ? (FrameworkElement)new CreditFilterPanel() : new RateFilterPanel());
        }

        private void Build(MarketKind market, FrameworkElement panel)
        {
            panel.Measure(new Size(384, 10000));
            panel.Arrange(new Rect(0, 0, 384, 10000));
            SidebarFilter.Configure(panel, fields.Values.ToList());
            panel.UpdateLayout();
            var sections = new List<CatalogSection>();
            CatalogSection section = null;
            foreach (var control in SidebarFilter.Descendants<FrameworkElement>(panel))
            {
                // 分区标题 = 面板分区头（原型侧栏分区）；「报价筛选」不进条件目录（原型语义）
                if (control is FilterSectionHeader header)
                {
                    section = header.Title == "报价筛选" ? null : new CatalogSection(header.Title);
                    if (section != null) sections.Add(section);
                }
                string key = SidebarFilter.GetField(control);
                if (section == null || key == null || !fields.TryGetValue(key, out var definition) || Prohibited(market, key)) continue;

                var kind = control is FilterSearchRow || control is TextBox ? CatalogConditionGroup.ConditionKind.Text
                    : control is FilterComboRow || control is ComboBox ? CatalogConditionGroup.ConditionKind.Select
                    : control is FilterRangeRow ? CatalogConditionGroup.ConditionKind.Range
                    : CatalogConditionGroup.ConditionKind.Tags;
                var group = new CatalogConditionGroup(definition.Label, key, kind)
                {
                    Enabled = definition.Enabled,
                    HasRange = key == "remainingYears" || control is FilterRangeRow,
                    HasUnitSelect = key == "remainingYears",
                };
                if (control is FilterRangeRow range && range.IsDisabled) group.Enabled = false;
                foreach (var toggle in SidebarFilter.Descendants<ToggleButton>(control))
                {
                    string label = toggle.Content as string;
                    if (string.IsNullOrEmpty(label)) continue;
                    // 面板静态 tag 带上字典代码（H14 enumValues）：勾选/落库/读回全程用代码对齐，
                    // 中文仅作显示——否则服务端 canonical 校验拒绝、行值匹配不上（实测踩坑）
                    var option = definition.EnumValues?.FirstOrDefault(o => o.Label == label);
                    group.Items.Add(new CatalogItem(label, option?.Value, toggle.IsEnabled));
                }
                if (kind == CatalogConditionGroup.ConditionKind.Select)
                {
                    foreach (var option in definition.EnumValues ?? new List<FilterEnumOption>())
                    {
                        group.Items.Add(new CatalogItem(option.Label, option.Value));
                    }
                }
                section.Groups.Add(group);
            }
            catalogs[market] = sections;
        }

        // ============ 条件编译（保存） ============

        /// <summary>UI chip → 协议条件。已带载荷的直接透传（仍校验选项未置灰）；否则从显示值编译。</summary>
        public FilterConditionPayload Compile(MarketKind market, UiCondition condition)
        {
            var group = ForMarket(market).SelectMany(item => item.Groups).FirstOrDefault(item => item.Key == condition.Key);
            if (group == null || !group.Enabled) throw new InvalidOperationException("筛选条件不可用：" + condition.Label);

            if (condition.Protocol is FilterConditionPayload stored)
            {
                // 已保存条件保留结构化值；仍检查当前目录中禁用的枚举选项
                foreach (var token in Tokenize(stored.Value))
                {
                    string label = fields[condition.Key].EnumValues?
                        .FirstOrDefault(option => option.Value == token)?.Label;
                    if (label != null && group.Items.Any(item => item.Label == label && !item.Enabled))
                    {
                        throw new InvalidOperationException("筛选选项不可用：" + label);
                    }
                }
                return stored;
            }

            var definition = fields[condition.Key];
            string value = condition.Value;
            var result = new FilterConditionPayload { Field = condition.Key, Operator = "EQ" };
            if (group.Kind == CatalogConditionGroup.ConditionKind.Text)
            {
                result.Operator = "CONTAINS";
                result.Value = value;
            }
            else if (group.HasRange)
            {
                decimal scale = value.EndsWith("M") ? 1m / 12 : value.EndsWith("D") ? 1m / 365 : 1m;
                value = value.TrimEnd('Y', 'M', 'D');
                if (value.StartsWith("<") || value.StartsWith("＞") || value.StartsWith(">"))
                {
                    result.Operator = value.StartsWith("<") ? "LT" : "GT";
                    result.Value = decimal.Parse(value.Substring(1), CultureInfo.InvariantCulture) * scale;
                }
                else
                {
                    var bounds = value.Split('-');
                    if (bounds.Length != 2) throw new InvalidOperationException("无效范围：" + value);
                    decimal? lower = bounds[0].Length == 0 ? (decimal?)null : decimal.Parse(bounds[0], CultureInfo.InvariantCulture) * scale;
                    decimal? upper = bounds[1].Length == 0 ? (decimal?)null : decimal.Parse(bounds[1], CultureInfo.InvariantCulture) * scale;
                    if (lower == null && upper == null || lower > upper) throw new InvalidOperationException("无效范围：" + value);
                    result.Operator = lower == null ? "LTE" : upper == null ? "GTE" : "BETWEEN";
                    result.Value = lower != null && upper != null ? (object)new[] { lower.Value, upper.Value } : lower ?? upper;
                }
            }
            else
            {
                // 显示值/代码双向定位目录项；落库取目录项代码（无字典字段 Value==Label 原值）
                var choice = group.Items.FirstOrDefault(item => item.Label == value || item.Value == value);
                if (choice == null || !choice.Enabled) throw new InvalidOperationException("筛选选项不可用：" + value);
                if (definition.Type == "boolean")
                {
                    string positive = condition.Key == "isTaxExempt" ? "免税" : condition.Key == "isMunicipal" ? "城投" : "次级";
                    result.Value = value == positive;
                }
                else if (condition.Key == "issueYear")
                {
                    result.Value = decimal.Parse(value, CultureInfo.InvariantCulture);
                }
                else
                {
                    result.Operator = "IN";
                    result.Value = new[] { choice.Value };
                }
            }
            return result;
        }

        // ============ 条件显示（读回） ============

        /// <summary>协议条件 → 中文 chip（Value=显示文本、Protocol=载荷透传）；字段不在字典时返回 null（调用方回退）。</summary>
        public UiCondition Display(FilterConditionPayload condition)
        {
            fields.TryGetValue(condition.Field, out var field);
            var tokens = Tokenize(condition.Value);
            Func<string, string> label = token => field?.EnumValues?.FirstOrDefault(option => option.Value == token)?.Label ?? token;
            string text;
            if (condition.Operator == "BETWEEN") text = tokens[0] + "-" + tokens[1];
            else if (tokens.Length > 1) text = string.Join(" / ", tokens.Select(label));
            else if (condition.Value is bool)
            {
                bool positive = (bool)condition.Value;
                text = condition.Field == "isTaxExempt" ? (positive ? "免税" : "非免税")
                    : condition.Field == "isMunicipal" ? (positive ? "城投" : "非城投") : (positive ? "次级" : "非次级");
            }
            else text = label(tokens[0]);
            if (condition.Operator == "GT") text = ">" + text;
            if (condition.Operator == "GTE") text = "≥" + text;
            if (condition.Operator == "LT") text = "<" + text;
            if (condition.Operator == "LTE") text = "≤" + text;
            if (condition.Field == "remainingYears") text += "Y";
            // Value 统一存显示文本（与点选目录的 chip 同语义，Identity 一致）；协议值在 Protocol
            return new UiCondition(condition.Field, field != null ? field.Label : condition.Field, text)
            {
                Protocol = condition,
            };
        }

        // ============ 辅助 ============

        /// <summary>载荷值展开为字符串 token（标量/数组/布尔统一）；空值返回空串占位。</summary>
        private static string[] Tokenize(object value)
        {
            if (value == null) return new[] { string.Empty };
            var enumerable = value as System.Collections.IEnumerable;
            if (enumerable is string || enumerable == null) return new[] { Convert.ToString(value, CultureInfo.InvariantCulture) };
            return enumerable.Cast<object>().Select(item => item == null ? string.Empty : Convert.ToString(item, CultureInfo.InvariantCulture)).ToArray();
        }
    }
}
