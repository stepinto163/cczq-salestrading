using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 产品侧栏筛选面板的字段标注与条件收集（远端 SidebarFilter 同构）。
    /// XAML 控件以 <c>SidebarFilter.Field</c> 显式标注协议字段（不通过中文标签猜字段）、
    /// 根节点以 <c>SidebarFilter.Scope</c> 标注市场范围（挂载收集器）；
    /// 枚举候选取服务端字典（Configure 填充），控件值变化经 300ms 防抖编译成
    /// <see cref="ProductFilter"/> 通知宿主（<see cref="IProductFilterHost"/>）下推重查。
    /// 新增控件需同时配置 Field/Scope 并核对后端数据映射，不能仅添加一个可点击的标签。
    /// </summary>
    public sealed class SidebarFilter
    {
        public static readonly DependencyProperty FieldProperty = DependencyProperty.RegisterAttached(
            "Field", typeof(string), typeof(SidebarFilter), new PropertyMetadata(null));
        public static string GetField(DependencyObject element) { return (string)element.GetValue(FieldProperty); }
        public static void SetField(DependencyObject element, string value) { element.SetValue(FieldProperty, value); }

        public static readonly DependencyProperty ScopeProperty = DependencyProperty.RegisterAttached(
            "Scope", typeof(string), typeof(SidebarFilter), new PropertyMetadata(null, OnScopeChanged));
        public static string GetScope(DependencyObject element) { return (string)element.GetValue(ScopeProperty); }
        public static void SetScope(DependencyObject element, string value) { element.SetValue(ScopeProperty, value); }

        private static readonly DependencyProperty EditorProperty = DependencyProperty.RegisterAttached(
            "Editor", typeof(SidebarFilter), typeof(SidebarFilter));

        private readonly FrameworkElement root;
        private readonly DispatcherTimer debounce;
        private Dictionary<string, FilterFieldEntry> definitions;
        private FrameworkElement[] fields;
        private bool initializing;
        private bool ready;

        private SidebarFilter(FrameworkElement root)
        {
            this.root = root;
            debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            debounce.Tick += (sender, args) => Apply();
            root.Loaded += OnLoaded;
            root.IsVisibleChanged += (sender, args) =>
            {
                if (root.IsVisible) OnLoaded(sender, new RoutedEventArgs());
            };
            root.Unloaded += (sender, args) => debounce.Stop();
            root.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnChanged), true);
            root.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(OnChanged), true);
            root.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnTextChanged), true);
            root.AddHandler(Selector.SelectionChangedEvent, new SelectionChangedEventHandler(OnSelectionChanged), true);
            root.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClick), true);
        }

        private static void OnScopeChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var root = element as FrameworkElement;
            if (root != null && root.GetValue(EditorProperty) == null)
            {
                root.SetValue(EditorProperty, new SidebarFilter(root));
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (ready || initializing || !root.IsLoaded || !root.IsVisible) return;
            var host = root.DataContext as IProductFilterHost;
            if (host == null) return;
            initializing = true;
            try
            {
                var entries = await host.GetFilterDictionaryAsync().ConfigureAwait(true);
                if (entries == null || entries.Count == 0) throw new InvalidOperationException("服务未返回筛选字段字典。");
                definitions = entries.ToDictionary(field => field.Field);
                fields = Descendants<FrameworkElement>(root)
                    .Where(control => !string.IsNullOrEmpty(GetField(control))).ToArray();
                Configure(root, entries);
                ready = true;
            }
            catch (Exception)
            {
                // 字典不可用时保持未就绪：点「筛选」按钮会重试（OnButtonClick），面板候选维持产品文案
            }
            finally
            {
                initializing = false;
            }
        }

        /// <summary>用服务端字典填充面板候选（枚举候选回退业务字典已在网关层做掉）：
        /// 标签行 Items 替换为候选 label（settlType 保留产品文案；细分券种/热门标签按面板原 Items 过滤，
        /// 信用/利率各自暴露不同分类）；发行年份取近 4 年；下拉行前置「不限」。</summary>
        public static void Configure(FrameworkElement root, IReadOnlyList<FilterFieldEntry> definitions)
        {
            if (definitions == null || definitions.Count == 0)
            {
                throw new InvalidOperationException("服务未返回筛选字段字典。");
            }
            var map = definitions.ToDictionary(field => field.Field);
            foreach (FrameworkElement control in Descendants<FrameworkElement>(root))
            {
                string field = GetField(control);
                if (string.IsNullOrEmpty(field) || !map.TryGetValue(field, out var definition)) continue;

                IReadOnlyList<FilterEnumOption> options = definition.EnumValues ?? new List<FilterEnumOption>();
                if (control is FilterTagRow tags && options.Count > 0 && field != "settlType")
                {
                    IEnumerable<FilterEnumOption> choices = options;
                    // 信用/利率面板有意暴露不同的债券分类：按面板原 Items 文案过滤字典候选
                    if (field == "bondSubtype" || field == "hotTags")
                    {
                        string[] captions = (tags.Items ?? string.Empty).Split(',');
                        choices = options.Where(option => captions.Contains(option.Label));
                    }
                    tags.Items = string.Join(",", choices.Select(option => option.Label));
                }
                if (control is FilterTagRow years && field == "issueYear")
                {
                    years.Items = string.Join(",", Enumerable.Range(DateTime.Today.Year - 3, 4).Reverse());
                }
                if (control is FilterComboRow comboRow)
                {
                    // 无候选时保留面板原 Items（不阻断整树构建）
                    if (options.Count > 0)
                    {
                        comboRow.Items = "不限," + string.Join(",", options.Select(option => option.Label));
                    }
                }
                else if (control is ComboBox combo && options.Count > 0)
                {
                    combo.Items.Clear();
                    combo.Items.Add("不限");
                    foreach (var option in options) combo.Items.Add(option.Label);
                    combo.SelectedIndex = 0;
                }
            }
        }

        private IEnumerable<FrameworkElement> Fields() { return fields ?? Enumerable.Empty<FrameworkElement>(); }

        /// <summary>视觉树深度优先遍历（目录构建/候选填充/条件收集共用）。</summary>
        public static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                var child = VisualTreeHelper.GetChild(parent, index);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        private FrameworkElement FindField(object source)
        {
            DependencyObject current = source as DependencyObject;
            while (current != null && current != root)
            {
                if (!string.IsNullOrEmpty(GetField(current))) return current as FrameworkElement;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void OnChanged(object sender, RoutedEventArgs args)
        {
            FrameworkElement fieldControl = FindField(args.OriginalSource);
            if (initializing || !ready || fieldControl == null) return;
            string field = GetField(fieldControl);
            // quoteType（净价/全价）与 boolean 字段单选互斥：选中即清同组其它项
            if (args.OriginalSource is ToggleButton selected && selected.IsChecked == true &&
                (field == "quoteType" || (definitions.ContainsKey(field) && definitions[field].Type == "boolean")))
            {
                initializing = true;
                foreach (ToggleButton toggle in Descendants<ToggleButton>(fieldControl).Where(toggle => toggle != selected))
                {
                    toggle.IsChecked = false;
                }
                initializing = false;
            }
            if (field == "remainingYears" && args.OriginalSource is ToggleButton)
            {
                initializing = true;
                foreach (TextBox input in Descendants<TextBox>(fieldControl)) input.Clear();
                initializing = false;
            }
            if (field == "quoteType") UpdatePriceUnits();
            Schedule();
        }

        private bool UsesCleanPrice()
        {
            return Fields().Where(item => GetField(item) == "quoteType")
                .SelectMany(item => Descendants<ToggleButton>(item))
                .Any(toggle => toggle.IsChecked == true && toggle.Content != null && toggle.Content.ToString() == "净价");
        }

        /// <summary>净价/收益率口径联动：价格/估值差区间行的单位（元/% 与 元/BP）。</summary>
        private void UpdatePriceUnits()
        {
            bool cleanPrice = UsesCleanPrice();
            foreach (FilterRangeRow range in Fields().OfType<FilterRangeRow>())
            {
                string field = GetField(range);
                if (field == "bidPrice" || field == "offerPrice") range.Unit = cleanPrice ? "元" : "%";
                if (field == "bidValuationSpread" || field == "offerValuationSpread") range.Unit = cleanPrice ? "元" : "BP";
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs args)
        {
            FrameworkElement control = FindField(args.OriginalSource);
            if (initializing || !ready || control == null) return;
            if (GetField(control) == "remainingYears")
            {
                initializing = true;
                foreach (ToggleButton toggle in Descendants<ToggleButton>(control)) toggle.IsChecked = false;
                initializing = false;
            }
            Schedule();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (!initializing && ready && FindField(args.OriginalSource) != null) Schedule();
        }

        private void OnButtonClick(object sender, RoutedEventArgs args)
        {
            if ((args.OriginalSource as Button) == null) return;
            if ((args.OriginalSource as Button).Content == null) return;
            if ((args.OriginalSource as Button).Content.ToString() != "筛选") return;
            if (!ready) OnLoaded(sender, args);
            else Apply();
        }

        private void Schedule()
        {
            debounce.Stop();
            debounce.Start();
        }

        private void Apply()
        {
            debounce.Stop();
            if (!ready || initializing || !root.IsLoaded) return;
            try
            {
                ProductFilter filter = Read(root);
                var host = root.DataContext as IProductFilterHost;
                if (host != null) host.ApplyProductFilter(GetScope(root), filter);
            }
            catch (Exception error)
            {
                ToastService.Show("筛选条件无效：" + error.Message, ToastKind.Error);
            }
        }

        /// <summary>遍历带 Field 的控件编译条件（面板态 → 协议载荷）。</summary>
        public static ProductFilter Read(FrameworkElement root)
        {
            var editor = (SidebarFilter)root.GetValue(EditorProperty);
            if (editor == null || !editor.ready) throw new InvalidOperationException("筛选字典尚未加载。");
            var filter = new ProductFilter();
            bool cleanPrice = editor.UsesCleanPrice();
            foreach (FrameworkElement control in editor.Fields())
            {
                editor.ReadField(control, filter.Conditions, cleanPrice);
            }
            return filter;
        }

        private void ReadField(FrameworkElement control, List<FilterConditionPayload> conditions, bool cleanPrice)
        {
            string field = GetField(control);
            if (control is FilterRangeRow range)
            {
                // 净价口径下估值差字段切换为净价差协议字段
                if (cleanPrice && field == "bidValuationSpread") field = "bidCleanPriceSpread";
                if (cleanPrice && field == "offerValuationSpread") field = "offerCleanPriceSpread";
                if (!range.IsDisabled) AddRange(conditions, field, range.MinText, range.ShowMax ? range.MaxText : null, 1);
                return;
            }
            if (control is FilterSearchRow || control is TextBox)
            {
                var textBox = control as TextBox ?? Descendants<TextBox>(control).FirstOrDefault();
                string text = textBox != null ? (textBox.Text ?? string.Empty).Trim() : string.Empty;
                if (!string.IsNullOrEmpty(text)) Add(conditions, field, "CONTAINS", text);
                return;
            }
            if (field == "remainingYears")
            {
                ReadMaturity(control, conditions);
                return;
            }
            FilterFieldEntry definition;
            if (!definitions.TryGetValue(field, out definition)) return;

            IEnumerable<string> labels;
            var combo = control as ComboBox ?? Descendants<ComboBox>(control).FirstOrDefault();
            if (combo != null)
            {
                if (combo.SelectedIndex <= 0) return;
                var item = combo.SelectedItem as ComboBoxItem;
                labels = new[] { item != null ? (item.Content ?? string.Empty).ToString() : (combo.SelectedItem != null ? combo.SelectedItem.ToString() : string.Empty) };
            }
            else
            {
                labels = Descendants<ToggleButton>(control).Where(toggle => toggle.IsChecked == true)
                    .Select(toggle => toggle.Content != null ? toggle.Content.ToString() : null);
            }
            string[] selected = labels.Where(label => !string.IsNullOrEmpty(label)).ToArray();
            if (selected.Length == 0) return;

            if (definition.Type == "boolean")
            {
                string positive = field == "isTaxExempt" ? "免税" : field == "isMunicipal" ? "城投"
                    : field == "isPerpetual" ? "永续" : "次级";
                Add(conditions, field, "EQ", selected[0] == positive);
            }
            else if (field == "settlType")
            {
                // 产品文案 → 协议值：T+0/非远期=1、T+1/非远期=2、远期=其余全部值
                var values = new HashSet<string>();
                foreach (string label in selected)
                {
                    if (label == "T+0" || label == "非远期") values.Add("1");
                    if (label == "T+1" || label == "非远期") values.Add("2");
                    if (label == "远期")
                    {
                        foreach (var option in definition.EnumValues ?? new List<FilterEnumOption>())
                        {
                            if (option.Value != "1" && option.Value != "2") values.Add(option.Value);
                        }
                    }
                }
                Add(conditions, field, "IN", values.ToArray());
            }
            else if (field == "issueYear")
            {
                Add(conditions, field, "IN", selected.Select(value => decimal.Parse(value, CultureInfo.InvariantCulture)).ToArray());
            }
            else
            {
                // label → 协议值（字典候选反查；不存在即面板与字典失配，暴露给调用方 toast）
                var values = new List<string>();
                foreach (string label in selected)
                {
                    string value = null;
                    if (definition.EnumValues != null)
                    {
                        foreach (var option in definition.EnumValues)
                        {
                            if (option.Label == label) { value = option.Value; break; }
                        }
                    }
                    if (value == null) throw new InvalidOperationException("字典中不存在 " + field + "：" + label);
                    values.Add(value);
                }
                Add(conditions, field, "IN", values.ToArray());
            }
        }

        /// <summary>剩余期限：自定义区间输入优先（Y/M/D 折算年），否则选中档位标签（&lt;3M / ＞20Y / 1-3Y…）。</summary>
        private static void ReadMaturity(FrameworkElement control, List<FilterConditionPayload> conditions)
        {
            TextBox[] inputs = Descendants<TextBox>(control).ToArray();
            if (inputs.Any(input => !string.IsNullOrWhiteSpace(input.Text)))
            {
                ComboBox units = Descendants<ComboBox>(control).FirstOrDefault();
                var unitItem = units != null ? units.SelectedItem as ComboBoxItem : null;
                string unit = unitItem != null && unitItem.Content != null ? unitItem.Content.ToString() : "Y";
                AddRange(conditions, "remainingYears", inputs[0].Text, inputs.Length > 1 ? inputs[1].Text : null,
                    unit == "M" ? 1m / 12 : unit == "D" ? 1m / 365 : 1m);
                return;
            }
            foreach (string label in Descendants<ToggleButton>(control).Where(toggle => toggle.IsChecked == true)
                .Select(toggle => toggle.Content != null ? toggle.Content.ToString() : string.Empty)
                .Where(label => !string.IsNullOrEmpty(label)).ToList())
            {
                if (label == "<3M")
                {
                    Add(conditions, "remainingYears", "LT", 0.25m);
                }
                else if (label == "＞20Y" || label == ">20Y")
                {
                    Add(conditions, "remainingYears", "GT", 20m);
                }
                else
                {
                    decimal scale = label.EndsWith("M") ? 1m / 12 : 1m;
                    string[] bounds = label.TrimEnd('M', 'Y').Split('-');
                    AddRange(conditions, "remainingYears", bounds[0], bounds.Length > 1 ? bounds[1] : null, scale);
                }
            }
        }

        /// <summary>同字段条件在协议里是 OR，有界区间必须合成一条 BETWEEN。</summary>
        private static void AddRange(List<FilterConditionPayload> conditions, string field, string minimum, string maximum, decimal scale)
        {
            decimal? lower = Number(minimum, field);
            decimal? upper = Number(maximum, field);
            if (lower.HasValue && upper.HasValue && lower > upper)
            {
                throw new InvalidOperationException(field + " 最小值不能大于最大值。");
            }
            if (lower.HasValue && upper.HasValue)
            {
                Add(conditions, field, "BETWEEN", new[] { lower.Value * scale, upper.Value * scale });
            }
            else if (lower.HasValue)
            {
                Add(conditions, field, "GTE", lower.Value * scale);
            }
            else if (upper.HasValue)
            {
                Add(conditions, field, "LTE", upper.Value * scale);
            }
        }

        private static decimal? Number(string text, string field)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            decimal value;
            if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return value;
            throw new InvalidOperationException(field + " 请输入有效数字。");
        }

        private static void Add(List<FilterConditionPayload> conditions, string field, string operation, object value)
        {
            conditions.Add(new FilterConditionPayload { Field = field, Operator = operation, Value = value });
        }

        /// <summary>清除过滤的视觉重置（原型 resetBondFilterPanel）：清控件选中态并立即应用一次空条件。</summary>
        public static void Reset(FrameworkElement root)
        {
            var editor = (SidebarFilter)root.GetValue(EditorProperty);
            if (editor == null) return;
            editor.initializing = true;
            foreach (FrameworkElement field in editor.Fields())
            {
                foreach (ToggleButton toggle in Descendants<ToggleButton>(field)) toggle.IsChecked = false;
                foreach (TextBox input in Descendants<TextBox>(field)) input.Clear();
                var textBox = field as TextBox;
                if (textBox != null) textBox.Clear();
                foreach (ComboBox combo in Descendants<ComboBox>(field)) combo.SelectedIndex = 0;
                var select = field as ComboBox;
                if (select != null) select.SelectedIndex = 0;
            }
            editor.initializing = false;
            editor.UpdatePriceUnits();
            editor.Apply();
        }
    }
}
