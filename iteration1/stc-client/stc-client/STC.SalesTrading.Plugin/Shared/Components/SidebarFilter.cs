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
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Shared.Services;
using com.gateway.log;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 给产品 XAML 控件显式标注协议字段；不通过中文标签猜字段。
    /// 枚举值取服务端字典，文本/区间保留原有产品控件。
    /// 本类把控件值编译成 Filter 并通知对应页面，不在客户端重新判定全部报价是否匹配。
    /// 新增控件需同时配置 Field/Scope 并核对后端数据映射，不能仅添加一个可点击的标签。
    /// </summary>
    public sealed class SidebarFilter
    {
        public static readonly DependencyProperty FieldProperty = DependencyProperty.RegisterAttached(
            "Field", typeof(string), typeof(SidebarFilter), new PropertyMetadata(null));
        public static string GetField(DependencyObject element) => (string)element.GetValue(FieldProperty);
        public static void SetField(DependencyObject element, string value) => element.SetValue(FieldProperty, value);

        public static readonly DependencyProperty ScopeProperty = DependencyProperty.RegisterAttached(
            "Scope", typeof(string), typeof(SidebarFilter), new PropertyMetadata(null, OnScopeChanged));
        public static string GetScope(DependencyObject element) => (string)element.GetValue(ScopeProperty);
        public static void SetScope(DependencyObject element, string value) => element.SetValue(ScopeProperty, value);

        private static readonly DependencyProperty EditorProperty = DependencyProperty.RegisterAttached(
            "Editor", typeof(SidebarFilter), typeof(SidebarFilter));

        private readonly FrameworkElement root;
        private readonly DispatcherTimer debounce;
        private Dictionary<string, FilterField> definitions;
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
            if (element is FrameworkElement root && root.GetValue(EditorProperty) == null)
            {
                root.SetValue(EditorProperty, new SidebarFilter(root));
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (ready || initializing || !root.IsLoaded || !root.IsVisible || !(root.DataContext is IProductFilterHost host)) return;
            initializing = true;
            try
            {
                Configure(await host.GetFilterDictionaryAsync());
            }
            catch (Exception error)
            {
                LogHelper.Error(error);
                ToastService.Show("筛选字典加载失败：" + error.Message);
                root.ToolTip = "筛选字典加载失败，点击筛选可重试：" + error.Message;
            }
            finally
            {
                initializing = false;
            }
        }

        // Also used by the WPF integration checks, with the real server dictionary.
        public static void Configure(FrameworkElement root, DictionaryData dictionary)
        {
            ((SidebarFilter)root.GetValue(EditorProperty)).Configure(dictionary);
        }

        private void Configure(DictionaryData dictionary)
        {
            if (dictionary?.FilterFields == null || dictionary.FilterFields.Count == 0)
                throw new InvalidOperationException("服务未返回筛选字段字典。");
            initializing = true;
            definitions = dictionary.FilterFields.ToDictionary(field => field.Field);
            root.UpdateLayout();
            fields = Descendants<FrameworkElement>(root)
                .Where(control => !string.IsNullOrEmpty(GetField(control))).ToArray();
            foreach (FrameworkElement control in Fields())
            {
                string field = GetField(control);
                if (!definitions.TryGetValue(field, out FilterField definition))
                    throw new InvalidOperationException("服务不支持筛选字段：" + field);
                List<DictionaryOption> options = definition.EnumValues;
                if ((options == null || options.Count == 0) && dictionary.Dictionaries != null)
                    dictionary.Dictionaries.TryGetValue(field, out options);
                if (options != null && options.Count > 0)
                    definition.EnumValues = options;
                if (control is FilterTagRow tags && options != null && options.Count > 0 && field != "settlType")
                {
                    IEnumerable<DictionaryOption> choices = options;
                    // Credit/rate panels intentionally expose different bond categories.
                    if (field == "bondSubtype" || field == "marketMakerTags")
                    {
                        string[] captions = tags.Items.Split(',');
                        choices = options.Where(option => captions.Contains(option.Label));
                    }
                    tags.Items = string.Join(",", choices.Select(option => option.Label));
                }
                if (control is FilterTagRow years && field == "issueYear")
                    years.Items = string.Join(",", Enumerable.Range(DateTime.Today.Year - 3, 4).Reverse());
                ComboBox combo = control as ComboBox;
                if (control is FilterComboRow comboRow)
                {
                    if (options == null || options.Count == 0)
                        throw new InvalidOperationException("服务未返回枚举值：" + field);
                    comboRow.Items = "不限," + string.Join(",", options.Select(option => option.Label));
                }
                else if (combo != null && options != null)
                {
                    combo.Items.Clear();
                    combo.Items.Add("不限");
                    foreach (DictionaryOption option in options) combo.Items.Add(option.Label);
                    combo.SelectedIndex = 0;
                }
            }
            root.UpdateLayout();
            ready = true;
            initializing = false;
        }

        private IEnumerable<FrameworkElement> Fields() => fields ?? Enumerable.Empty<FrameworkElement>();

        public static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, index);
                if (child is T match) yield return match;
                foreach (T nested in Descendants<T>(child)) yield return nested;
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
            if (args.OriginalSource is ToggleButton selected && selected.IsChecked == true &&
                (field == "quoteType" || definitions[field].Type == "boolean"))
            {
                initializing = true;
                foreach (ToggleButton toggle in Descendants<ToggleButton>(fieldControl).Where(toggle => toggle != selected))
                    toggle.IsChecked = false;
                initializing = false;
            }
            if (field == "remainingYears" && args.OriginalSource is ToggleButton maturity && maturity.IsChecked == true)
            {
                initializing = true;
                foreach (TextBox input in Descendants<TextBox>(fieldControl)) input.Clear();
                initializing = false;
            }
            if (field == "quoteType") UpdatePriceUnits();
            Schedule();
        }

        private bool UsesCleanPrice() => Fields().Where(item => GetField(item) == "quoteType")
            .SelectMany(item => Descendants<ToggleButton>(item))
            .Any(toggle => toggle.IsChecked == true && toggle.Content?.ToString() == "净价");

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
            if ((args.OriginalSource as Button)?.Content?.ToString() != "筛选") return;
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
                Filter filter = Read(root);
                (root.DataContext as IProductFilterHost)?.ApplyProductFilter(GetScope(root), filter);
            }
            catch (Exception error)
            {
                LogHelper.Error(error);
                ToastService.Show("筛选条件无效：" + error.Message);
            }
        }

        public static Filter Read(FrameworkElement root)
        {
            SidebarFilter editor = (SidebarFilter)root.GetValue(EditorProperty);
            if (editor == null || !editor.ready) throw new InvalidOperationException("筛选字典尚未加载。");
            var filter = new Filter { Conditions = new List<FilterCondition>() };
            bool cleanPrice = editor.UsesCleanPrice();
            foreach (FrameworkElement control in editor.Fields()) editor.ReadField(control, filter.Conditions, cleanPrice);
            return filter;
        }

        private void ReadField(FrameworkElement control, List<FilterCondition> conditions, bool cleanPrice)
        {
            string field = GetField(control);
            if (control is FilterRangeRow range)
            {
                if (cleanPrice && field == "bidValuationSpread") field = "bidCleanPriceSpread";
                if (cleanPrice && field == "offerValuationSpread") field = "offerCleanPriceSpread";
                if (!range.IsDisabled) AddRange(conditions, field, range.MinText, range.ShowMax ? range.MaxText : null, 1);
                return;
            }
            if (control is FilterSearchRow || control is TextBox)
            {
                string text = (control as TextBox ?? Descendants<TextBox>(control).FirstOrDefault())?.Text?.Trim();
                if (!string.IsNullOrEmpty(text)) Add(conditions, field, "CONTAINS", text);
                return;
            }
            if (field == "remainingYears")
            {
                ReadMaturity(control, conditions);
                return;
            }
            IEnumerable<string> labels;
            ComboBox combo = control as ComboBox ?? Descendants<ComboBox>(control).FirstOrDefault();
            if (combo != null)
            {
                if (combo.SelectedIndex <= 0) return;
                labels = new[] { (combo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? combo.SelectedItem?.ToString() };
            }
            else labels = Descendants<ToggleButton>(control).Where(toggle => toggle.IsChecked == true)
                .Select(toggle => toggle.Content?.ToString());
            string[] selected = labels.Where(label => !string.IsNullOrEmpty(label)).ToArray();
            if (selected.Length == 0) return;
            if (definitions[field].Type == "boolean")
            {
                string positive = field == "isTaxExempt" ? "免税" : field == "isMunicipal" ? "城投"
                    : field == "isPerpetual" ? "永续" : "次级";
                Add(conditions, field, "EQ", selected[0] == positive);
            }
            else if (field == "settlType")
            {
                var values = new HashSet<string>();
                foreach (string label in selected)
                {
                    if (label == "T+0" || label == "非远期") values.Add("1");
                    if (label == "T+1" || label == "非远期") values.Add("2");
                    if (label == "远期")
                        foreach (DictionaryOption option in definitions[field].EnumValues.Where(option => option.Value != "1" && option.Value != "2"))
                            values.Add(option.Value);
                }
                Add(conditions, field, "IN", values.ToArray());
            }
            else if (field == "issueYear") Add(conditions, field, "IN", selected.Select(value => decimal.Parse(value, CultureInfo.InvariantCulture)).ToArray());
            else
            {
                string[] values = selected.Select(label => definitions[field].EnumValues?.FirstOrDefault(option => option.Label == label)?.Value
                    ?? throw new InvalidOperationException("字典中不存在 " + field + "：" + label)).ToArray();
                Add(conditions, field, "IN", values);
            }
        }

        private static void ReadMaturity(FrameworkElement control, List<FilterCondition> conditions)
        {
            TextBox[] inputs = Descendants<TextBox>(control).ToArray();
            if (inputs.Any(input => !string.IsNullOrWhiteSpace(input.Text)))
            {
                ComboBox units = Descendants<ComboBox>(control).FirstOrDefault();
                string unit = (units?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Y";
                AddRange(conditions, "remainingYears", inputs[0].Text, inputs.Length > 1 ? inputs[1].Text : null,
                    unit == "M" ? 1m / 12 : unit == "D" ? 1m / 365 : 1m);
                return;
            }
            foreach (string label in Descendants<ToggleButton>(control).Where(toggle => toggle.IsChecked == true)
                .Select(toggle => toggle.Content.ToString()))
            {
                if (label == "<3M") Add(conditions, "remainingYears", "LT", 0.25m);
                else if (label == "＞20Y" || label == ">20Y") Add(conditions, "remainingYears", "GT", 20m);
                else
                {
                    decimal scale = label.EndsWith("M") ? 1m / 12 : 1m;
                    string[] bounds = label.TrimEnd('M', 'Y').Split('-');
                    AddRange(conditions, "remainingYears", bounds[0], bounds[1], scale);
                }
            }
        }

        private static void AddRange(List<FilterCondition> conditions, string field, string minimum, string maximum, decimal scale)
        {
            decimal? lower = Number(minimum, field);
            decimal? upper = Number(maximum, field);
            if (lower.HasValue && upper.HasValue && lower > upper)
                throw new InvalidOperationException(field + " 最小值不能大于最大值。");
            // Same-field conditions are OR in the protocol, so a bounded range must be one BETWEEN.
            if (lower.HasValue && upper.HasValue)
                Add(conditions, field, "BETWEEN", new[] { lower.Value * scale, upper.Value * scale });
            else if (lower.HasValue) Add(conditions, field, "GTE", lower.Value * scale);
            else if (upper.HasValue) Add(conditions, field, "LTE", upper.Value * scale);
        }

        private static decimal? Number(string text, string field)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal value)) return value;
            throw new InvalidOperationException(field + " 请输入有效数字。");
        }

        private static void Add(List<FilterCondition> conditions, string field, string operation, object value)
        {
            conditions.Add(new FilterCondition { Field = field, Operator = operation, Value = value });
        }

        public static void Reset(FrameworkElement root)
        {
            SidebarFilter editor = (SidebarFilter)root.GetValue(EditorProperty);
            if (editor == null) return;
            editor.initializing = true;
            foreach (FrameworkElement field in editor.Fields())
            {
                foreach (ToggleButton toggle in Descendants<ToggleButton>(field)) toggle.IsChecked = false;
                foreach (TextBox input in Descendants<TextBox>(field)) input.Clear();
                if (field is TextBox text) text.Clear();
                foreach (ComboBox combo in Descendants<ComboBox>(field)) combo.SelectedIndex = 0;
                if (field is ComboBox select) select.SelectedIndex = 0;
            }
            editor.initializing = false;
            editor.UpdatePriceUnits();
            editor.Apply();
        }
    }
}
