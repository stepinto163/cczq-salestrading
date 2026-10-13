using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Shared.Components;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using UiCondition = STC.SalesTrading.Plugin.Shared.ViewModel.FilterCondition;
using ApiCondition = STC.SalesTrading.Plugin.Ficc.Protocol.FilterCondition;
using MarketKind = STC.SalesTrading.Plugin.Shared.Services.FilterProfileService.MarketKind;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>从产品侧栏与服务端字典构建分组目录；中文仅展示，协议保存稳定字段和结构化值。</summary>
    public sealed class GroupConditionCatalog
    {
        private readonly Dictionary<string, FilterField> fields;
        private readonly Dictionary<MarketKind, List<CatalogSection>> catalogs = new Dictionary<MarketKind, List<CatalogSection>>();
        public GroupConditionCatalog(DictionaryData dictionary)
        {
            fields = dictionary.FilterFields.ToDictionary(field => field.Field);
            foreach (var field in fields.Values)
                if ((field.EnumValues == null || field.EnumValues.Count == 0) && dictionary.Dictionaries != null
                    && dictionary.Dictionaries.TryGetValue(field.Field, out var options)) field.EnumValues = options;
            Build(MarketKind.Credit, new CreditFilterPanel(), dictionary);
            Build(MarketKind.Rate, new RateFilterPanel(), dictionary);
        }
        public static bool Prohibited(MarketKind market, string field) => market == MarketKind.Rate
            && new[] { "localGovernmentRegion", "issuerRating", "marketMakerTags", "perpetualType", "isPerpetual" }.Contains(field);
        public List<CatalogSection> ForMarket(MarketKind market) => catalogs[market];

        private void Build(MarketKind market, FrameworkElement panel, DictionaryData dictionary)
        {
            panel.Measure(new Size(384, 10000));
            panel.Arrange(new Rect(0, 0, 384, 10000));
            SidebarFilter.Configure(panel, dictionary);
            panel.UpdateLayout();
            var sections = new List<CatalogSection>();
            CatalogSection section = null;
            foreach (var control in SidebarFilter.Descendants<FrameworkElement>(panel))
            {
                if (control is FilterSectionHeader header)
                {
                    section = header.Title == "报价筛选" ? null : new CatalogSection(header.Title);
                    if (section != null) sections.Add(section);
                }
                string key = SidebarFilter.GetField(control);
                if (section == null || key == null || !fields.TryGetValue(key, out var definition) || Prohibited(market, key)) continue;
                var kind = control is FilterSearchRow || control is TextBox ? CatalogConditionGroup.ConditionKind.Text
                    : control is FilterComboRow || control is ComboBox ? CatalogConditionGroup.ConditionKind.Select
                    : control is FilterRangeRow ? CatalogConditionGroup.ConditionKind.Range : CatalogConditionGroup.ConditionKind.Tags;
                var group = new CatalogConditionGroup(definition.Label, key, kind)
                {
                    Enabled = control.IsEnabled && definition.Enabled != false,
                    HasRange = key == "remainingYears" || control is FilterRangeRow,
                    HasUnitSelect = key == "remainingYears"
                };
                if (control is FilterRangeRow range && range.IsDisabled) group.Enabled = false;
                foreach (var toggle in SidebarFilter.Descendants<ToggleButton>(control))
                {
                    string label = toggle.Content as string;
                    if (!string.IsNullOrEmpty(label)) group.Items.Add(new CatalogItem(label, toggle.IsEnabled));
                }
                if (kind == CatalogConditionGroup.ConditionKind.Select)
                    foreach (var option in definition.EnumValues ?? new List<DictionaryOption>())
                        group.Items.Add(new CatalogItem(option.Label));
                section.Groups.Add(group);
            }
            catalogs[market] = sections;
        }

        public ApiCondition Compile(MarketKind market, UiCondition condition)
        {
            var group = ForMarket(market).SelectMany(section => section.Groups).FirstOrDefault(item => item.Key == condition.Key);
            if (group == null || !group.Enabled) throw new InvalidOperationException("筛选条件不可用：" + condition.Label);
            if (condition.Protocol != null)
            {
                // 已保存条件保留结构化值；仍检查当前目录中禁用的枚举选项。
                var stored = condition.Protocol;
                var values = stored.Value == null ? JValue.CreateNull() : JToken.FromObject(stored.Value);
                foreach (var valueToken in values is JArray array ? array : new JArray(values))
                {
                    string label = fields[condition.Key].EnumValues?
                        .FirstOrDefault(option => option.Value == valueToken.ToString())?.Label;
                    if (label != null && group.Items.Any(item => item.Label == label && !item.Enabled))
                    {
                        throw new InvalidOperationException("筛选选项不可用：" + label);
                    }
                }
                return stored;
            }
            var definition = fields[condition.Key];
            string value = condition.Value;
            var result = new ApiCondition { Field = condition.Key, Operator = "EQ" };
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
                var choice = group.Items.FirstOrDefault(item => item.Label == value);
                if (choice == null || !choice.Enabled) throw new InvalidOperationException("筛选选项不可用：" + value);
                if (definition.Type == "boolean")
                {
                    string positive = condition.Key == "isTaxExempt" ? "免税" : condition.Key == "isMunicipal" ? "城投" : "次级";
                    result.Value = value == positive;
                }
                else if (condition.Key == "issueYear") result.Value = decimal.Parse(value, CultureInfo.InvariantCulture);
                else result.Value = definition.EnumValues?.FirstOrDefault(option => option.Label == value)?.Value
                    ?? throw new InvalidOperationException("字典中不存在：" + value);
            }
            return result;
        }

        public UiCondition Display(ApiCondition condition)
        {
            fields.TryGetValue(condition.Field, out var field);
            var token = condition.Value == null ? JValue.CreateNull() : JToken.FromObject(condition.Value);
            Func<JToken, string> label = value => field?.EnumValues?.FirstOrDefault(option => option.Value == value.ToString())?.Label ?? value.ToString();
            string text;
            if (condition.Operator == "BETWEEN") text = token[0] + "-" + token[1];
            else if (token is JArray array) text = string.Join(" / ", array.Select(label));
            else if (token.Type == JTokenType.Boolean)
            {
                bool positive = (bool)token;
                text = condition.Field == "isTaxExempt" ? (positive ? "免税" : "非免税")
                    : condition.Field == "isMunicipal" ? (positive ? "城投" : "非城投") : (positive ? "次级" : "非次级");
            }
            else text = label(token);
            if (condition.Operator == "GT") text = ">" + text;
            if (condition.Operator == "GTE") text = "≥" + text;
            if (condition.Operator == "LT") text = "<" + text;
            if (condition.Operator == "LTE") text = "≤" + text;
            if (condition.Field == "remainingYears") text += "Y";
            return new UiCondition(condition.Field, field?.Label ?? condition.Field, text) { Protocol = condition };
        }
    }
}
