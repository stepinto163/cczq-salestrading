using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;

namespace STC.SalesTrading.Plugin.View
{
    // Product layout is explicit. The shared dictionary supplies protocol values.
    public sealed class ProductFilterPanel : UserControl
    {
        private readonly FiccApiClient api;
        private readonly Func<Func<Task>, Task> execute;
        private readonly Func<JObject, Task> apply;
        private readonly StackPanel fields = new StackPanel();
        private readonly TextBox search = new TextBox { Margin = new Thickness(4), ToolTip = "搜索债券、报价机构或联系人" };
        private readonly Dictionary<string, Func<IEnumerable<JObject>>> readers = new Dictionary<string, Func<IEnumerable<JObject>>>();
        private readonly List<Action> clear = new List<Action>();
        private readonly Dictionary<string, TextBlock> priceUnits = new Dictionary<string, TextBlock>();
        private readonly WrapPanel watchlists = new WrapPanel();
        private readonly WrapPanel groups = new WrapPanel();
        private string selectedWatchlist;
        private string selectedGroup;
        public int FieldCount => readers.Count;

        public ProductFilterPanel(FiccApiClient api, Func<Func<Task>, Task> execute, Func<JObject, Task> apply)
        {
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/STC.SalesTrading.Plugin;component/View/ProductTheme.xaml", UriKind.Relative)
            });
            this.api = api;
            this.execute = execute;
            this.apply = apply;
            Style sidebarScrollbar = new Style(typeof(ScrollBar));
            sidebarScrollbar.Setters.Add(new Setter(WidthProperty, 6.0));
            Resources.Add(typeof(ScrollBar), sidebarScrollbar);
            MinWidth = 384;
            Background = (Brush)new BrushConverter().ConvertFromString("#0D1117");
            DockPanel root = new DockPanel { Margin = new Thickness(10, 0, 10, 0) };
            DockPanel top = new DockPanel { Margin = new Thickness(0, 10, 0, 8) };
            Button reset = UiElements.Button("清除过滤", async () => await execute(async () =>
            {
                Reset();
                await apply(Read());
            }));
            reset.Margin = new Thickness(0, 0, 6, 0);
            reset.Padding = new Thickness(8, 2, 8, 2);
            DockPanel.SetDock(reset, Dock.Left);
            top.Children.Add(reset);
            search.Tag = "搜索债券、机构、交易员、发行人";
            search.Style = (Style)FindResource("ProductFilterInput");
            search.Height = 26;
            search.Margin = new Thickness(0);
            search.KeyDown += async (sender, args) =>
            {
                if (args.Key == System.Windows.Input.Key.Enter)
                {
                    await execute(() => apply(Read()));
                }
            };
            top.Children.Add(search);
            DockPanel.SetDock(top, Dock.Top);
            root.Children.Add(top);
            StackPanel content = new StackPanel();
            content.Children.Add(new Border { BorderBrush = (Brush)new BrushConverter().ConvertFromString("#21262D"),
                BorderThickness = new Thickness(0, 1, 0, 0), Padding = new Thickness(0, 12, 0, 0),
                Child = LabelledRow("关注债券", watchlists) });
            content.Children.Add(Section(null, LabelledRow("筛选分组", groups)));
            content.Children.Add(fields);
            root.Children.Add(new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
            Content = root;
        }

        private static FrameworkElement Section(string title, UIElement content)
        {
            StackPanel panel = new StackPanel();
            if (title != null)
            {
                panel.Children.Add(new TextBlock { Text = title, FontSize = 13, FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.SlateGray, Margin = new Thickness(0, 0, 0, 8) });
            }
            panel.Children.Add(content);
            return new Border { Child = panel, BorderBrush = (Brush)new BrushConverter().ConvertFromString("#21262D"),
                BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(0, 0, 0, 8), Margin = new Thickness(0, 0, 0, 12) };
        }

        private static DockPanel LabelledRow(string label, UIElement content)
        {
            DockPanel row = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
            TextBlock title = new TextBlock { Text = label, Width = 73, FontSize = 13, Foreground = Brushes.SlateGray,
                Padding = new Thickness(0, 2, 0, 0) };
            DockPanel.SetDock(title, Dock.Left);
            row.Children.Add(title);
            row.Children.Add(content);
            return row;
        }

        public void SetDictionary(JObject dictionary)
        {
            fields.Children.Clear();
            readers.Clear();
            clear.Clear();
            priceUnits.Clear();
            JArray catalog = dictionary["filterFields"] as JArray;
            if (catalog == null || catalog.Count == 0)
            {
                throw new InvalidOperationException("服务未返回产品筛选字段目录。");
            }
            var definitions = catalog.OfType<JObject>().ToDictionary(item => (string)item["field"]);
            readers["searchText"] = () => string.IsNullOrWhiteSpace(search.Text) ? Enumerable.Empty<JObject>()
                : new[] { Condition("searchText", "CONTAINS", search.Text.Trim()) };
            JObject dictionaries = dictionary["dictionaries"] as JObject;
            StackPanel quote = new StackPanel();
            quote.Children.Add(BuildField(definitions["settlType"], dictionaries));
            DockPanel priceType = new DockPanel();
            Button filterButton = UiElements.Button("筛选", async () => await execute(() => apply(Read())));
            filterButton.Padding = new Thickness(8, 2, 8, 2);
            filterButton.Margin = new Thickness(4, 0, 0, 8);
            DockPanel.SetDock(filterButton, Dock.Right);
            priceType.Children.Add(filterButton);
            priceType.Children.Add(BuildField(definitions["quoteType"], dictionaries));
            quote.Children.Add(priceType);
            UniformGrid quoteRanges = new UniformGrid { Columns = 2 };
            foreach (string name in new[] { "bidPrice", "offerPrice", "bidValuationSpread", "offerValuationSpread", "bidOfferSpread",
                "valuationYield", "csiValuationYield", "couponRate", "bidQuantity", "offerQuantity" })
            {
                quoteRanges.Children.Add(BuildField(definitions[name], dictionaries, true));
            }
            quote.Children.Add(quoteRanges);
            fields.Children.Add(Section("报价筛选", quote));
            // The product controls determine layout; the API dictionary supplies values only.
            string[] sections =
            {
                "债券筛选|productType,bondSubtype,hotTags,isTaxExempt,localGovernmentType,localGovernmentUse,localGovernmentRegion,ncdType,remainingYears,issuerName,issuerType,listingStatus,issueMethod",
                "流动性筛选|liquidityGrade,marketMakerTags,listedMarket",
                "信用筛选|bankType,industry,ownershipType,guaranteeType,guarantorName,issuerRating,issuerCbrRating,ratingOutlook,bondRating,bondCbrRating,yyRating,csiRating",
                "条款筛选|optionType,repaymentType,couponFrequency,perpetualType,isSubordinated,rateIndex,issueYear",
                "区域筛选|isMunicipal,keyProvince,municipalLevel,regionalRating,regionCategory,region"
            };
            foreach (string section in sections)
            {
                string[] parts = section.Split('|');
                StackPanel controls = new StackPanel();
                foreach (string name in parts[1].Split(','))
                {
                    controls.Children.Add(BuildField(definitions[name], dictionaries));
                }
                fields.Children.Add(Section(parts[0], controls));
            }
        }

        private FrameworkElement BuildField(JObject definition, JObject dictionaries, bool compact = false)
        {
            string name = (string)definition["field"];
            string type = (string)definition["type"];
            string label = (string)definition["label"];
            if (name == "settlType")
            {
                label = "清算速度";
            }
            Dictionary<string, string> labels = new Dictionary<string, string>
            {
                ["listedMarket"] = "上市场所", ["optionType"] = "含权属性", ["perpetualType"] = "永续债",
                ["rateIndex"] = "利率方式", ["regionalRating"] = "地区等级", ["regionCategory"] = "地区",
                ["listingStatus"] = "上市状态", ["localGovernmentUse"] = "地方债专项"
            };
            if (labels.TryGetValue(name, out string productLabel))
            {
                label = productLabel;
            }
            DockPanel row = new DockPanel { Margin = new Thickness(0, 0, compact ? 4 : 0, compact ? 2 : 5) };
            TextBlock fieldCaption = new TextBlock { Text = label, Width = compact ? 52 : 73, FontSize = compact ? 12 : 13,
                Foreground = compact ? Brushes.Gainsboro : Brushes.SlateGray, Padding = new Thickness(0, 2, 0, 0), ToolTip = label };
            DockPanel.SetDock(fieldCaption, Dock.Left);
            row.Children.Add(fieldCaption);
            JArray options = definition["enumValues"] as JArray ?? dictionaries?[name] as JArray;
            if (name == "settlType" && options != null)
            {
                WrapPanel choices = new WrapPanel();
                var checks = new List<CheckBox>();
                foreach (string caption in new[] { "T+0", "T+1", "远期", "非远期" })
                {
                    string[] values = caption == "T+0" ? new[] { "1" } : caption == "T+1" ? new[] { "2" }
                        : caption == "非远期" ? new[] { "1", "2" }
                        : options.OfType<JObject>().Select(option => (string)option["value"]).Where(value => value != "1" && value != "2").ToArray();
                    CheckBox check = new CheckBox { Content = caption, Tag = values };
                    check.Style = (Style)FindResource("ProductFilterTag");
                    AutomationProperties.SetAutomationId(check, "filter.settlement." + caption);
                    checks.Add(check);
                    choices.Children.Add(check);
                    clear.Add(() => check.IsChecked = false);
                }
                readers[name] = () =>
                {
                    JArray values = new JArray(checks.Where(check => check.IsChecked == true).SelectMany(check => (string[])check.Tag).Distinct());
                    return values.Count == 0 ? Enumerable.Empty<JObject>() : new[] { Condition(name, "IN", values) };
                };
                row.Children.Add(choices);
            }
            else if (name == "localGovernmentUse" && options != null)
            {
                ComboBox select = new ComboBox { Style = (Style)FindResource("ProductFilterSelect"),
                    SelectedValuePath = "Tag" };
                select.Items.Add(new ComboBoxItem { Content = "不限", Tag = "" });
                foreach (JObject option in options.OfType<JObject>())
                {
                    select.Items.Add(new ComboBoxItem { Content = (string)option["label"], Tag = (string)option["value"] });
                }
                select.SelectedIndex = 0;
                row.Children.Add(select);
                clear.Add(() => select.SelectedIndex = 0);
                readers[name] = () => select.SelectedIndex <= 0 ? Enumerable.Empty<JObject>()
                    : new[] { Condition(name, "EQ", (string)select.SelectedValue) };
            }
            else if (options != null && options.Count > 0)
            {
                WrapPanel choices = new WrapPanel();
                var checks = new List<CheckBox>();
                foreach (JObject option in options.OfType<JObject>().Where(option => name != "quoteType" || (string)option["value"] != "3"))
                {
                    CheckBox check = new CheckBox { Content = (string)option["label"], Tag = option["value"].DeepClone() };
                    check.Style = (Style)FindResource("ProductFilterTag");
                    if (name == "quoteType")
                    {
                        check.Width = (string)option["value"] == "1" ? 92 : 80;
                        check.Checked += (sender, args) =>
                        {
                            foreach (CheckBox other in checks.Where(other => other != check))
                            {
                                other.IsChecked = false;
                            }
                            UpdatePriceUnits(check.Tag.ToString() == "2");
                        };
                        check.Unchecked += (sender, args) => UpdatePriceUnits(checks.Any(other => other.IsChecked == true && other.Tag.ToString() == "2"));
                    }
                    AutomationProperties.SetAutomationId(check, "filter." + name + "." + option["value"]);
                    checks.Add(check);
                    choices.Children.Add(check);
                    clear.Add(() => check.IsChecked = false);
                }
                row.Children.Add(choices);
                if (name == "localGovernmentRegion" || name == "keyProvince")
                {
                    string[] preferred = { "广东", "山东", "浙江", "江苏" };
                    CheckBox[] primary = preferred.Select(prefix => checks.FirstOrDefault(check =>
                        ((string)check.Content).StartsWith(prefix, StringComparison.Ordinal))).Where(check => check != null).ToArray();
                    if (primary.Length == 0)
                    {
                        primary = checks.Take(4).ToArray();
                    }
                    foreach (CheckBox check in checks)
                    {
                        check.Visibility = primary.Contains(check) ? Visibility.Visible : Visibility.Collapsed;
                    }
                    Button more = new Button { Content = "更多", Padding = new Thickness(8, 2, 8, 2), Margin = new Thickness(0, 0, 3, 3) };
                    more.Click += (sender, args) =>
                    {
                        bool expand = (string)more.Content == "更多";
                        foreach (CheckBox check in checks.Where(check => !primary.Contains(check)))
                        {
                            check.Visibility = expand || check.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                        }
                        more.Content = expand ? "收起" : "更多";
                    };
                    choices.Children.Add(more);
                }
                readers[name] = () =>
                {
                    JArray values = new JArray(checks.Where(check => check.IsChecked == true).Select(check => ((JToken)check.Tag).DeepClone()));
                    return values.Count == 0 ? Enumerable.Empty<JObject>() : new[] { Condition(name, "IN", values) };
                };
            }
            else if (type == "boolean")
            {
                WrapPanel choices = new WrapPanel();
                string positive = name == "isTaxExempt" ? "免税" : name == "isMunicipal" ? "城投" : "是";
                string negative = name == "isTaxExempt" ? "非免税" : name == "isMunicipal" ? "非城投" : "否";
                CheckBox yes = new CheckBox { Content = positive, Style = (Style)FindResource("ProductFilterTag") };
                CheckBox no = new CheckBox { Content = negative, Style = (Style)FindResource("ProductFilterTag") };
                yes.Checked += (sender, args) => no.IsChecked = false;
                no.Checked += (sender, args) => yes.IsChecked = false;
                choices.Children.Add(yes);
                choices.Children.Add(no);
                row.Children.Add(choices);
                clear.Add(() => { yes.IsChecked = false; no.IsChecked = false; });
                readers[name] = () => yes.IsChecked != true && no.IsChecked != true ? Enumerable.Empty<JObject>()
                    : new[] { Condition(name, "EQ", yes.IsChecked == true) };
            }
            else if (type == "decimal")
            {
                Grid range = new Grid();
                range.ColumnDefinitions.Add(new ColumnDefinition());
                range.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
                range.ColumnDefinitions.Add(new ColumnDefinition());
                TextBox lower = new TextBox { ToolTip = "最小值", Tag = "最小", Style = (Style)FindResource("ProductFilterInput") };
                TextBox upper = new TextBox { ToolTip = "最大值", Tag = "最大", Style = (Style)FindResource("ProductFilterInput") };
                AutomationProperties.SetAutomationId(lower, "filter." + name + ".min");
                AutomationProperties.SetAutomationId(upper, "filter." + name + ".max");
                TextBlock separator = new TextBlock { Text = "—", VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(separator, 1);
                Grid.SetColumn(upper, 2);
                range.Children.Add(lower);
                range.Children.Add(separator);
                range.Children.Add(upper);
                if (compact)
                {
                    string unit = name.EndsWith("Quantity") ? "万" : name.EndsWith("Spread") ? "BP" : "%";
                    TextBlock suffix = new TextBlock { Text = unit, Width = 20, FontSize = 11, Foreground = Brushes.SlateGray };
                    priceUnits[name] = suffix;
                    DockPanel.SetDock(suffix, Dock.Right);
                    row.Children.Add(suffix);
                    if (name.EndsWith("Quantity"))
                    {
                        range.HorizontalAlignment = HorizontalAlignment.Left;
                        range.Width = 43;
                        upper.Visibility = Visibility.Collapsed;
                        separator.Visibility = Visibility.Collapsed;
                        range.ColumnDefinitions[1].Width = new GridLength(0);
                        range.ColumnDefinitions[2].Width = new GridLength(0);
                        // Keep the unit next to the single minimum-volume box.
                        row.Children.Remove(suffix);
                        range.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
                        Grid.SetColumn(suffix, 3);
                        range.Children.Add(suffix);
                        range.Width = 63;
                    }
                }
                row.Children.Add(range);
                clear.Add(() => { lower.Clear(); upper.Clear(); });
                readers[name] = () => ReadRange(name, label, lower.Text, upper.Text);
                if (name == "remainingYears")
                {
                    WrapPanel presets = new WrapPanel();
                    string[] periods = { "<3M|0|0.25", "3-6M|0.25|0.5", "6-9M|0.5|0.75", "9-12M|0.75|1", "1-3Y|1|3",
                        "3-5Y|3|5", "5-7Y|5|7", "7-10Y|7|10", "10-15Y|10|15", "15-20Y|15|20", ">20Y|20|" };
                    string selectedPeriod = null;
                    foreach (string period in periods)
                    {
                        string[] values = period.Split('|');
                        CheckBox choice = new CheckBox { Content = values[0], Style = (Style)FindResource("ProductFilterTag") };
                        choice.Checked += (sender, args) =>
                        {
                            foreach (CheckBox other in presets.Children.OfType<CheckBox>().Where(other => other != choice))
                            {
                                other.IsChecked = false;
                            }
                            lower.Text = values[1];
                            upper.Text = values[2];
                            selectedPeriod = values[0];
                        };
                        choice.Unchecked += (sender, args) =>
                        {
                            if (selectedPeriod == values[0])
                            {
                                lower.Clear();
                                upper.Clear();
                                selectedPeriod = null;
                            }
                        };
                        presets.Children.Add(choice);
                        clear.Add(() => choice.IsChecked = false);
                    }
                    row.Children.Remove(range);
                    StackPanel tenor = new StackPanel();
                    tenor.Children.Add(presets);
                    tenor.Children.Add(range);
                    row.Children.Add(tenor);
                    readers[name] = () => selectedPeriod == "<3M" && lower.Text == "0" && upper.Text == "0.25"
                        ? new[] { Condition(name, "LT", 0.25m) }
                        : selectedPeriod == ">20Y" && lower.Text == "20" && upper.Text == ""
                            ? new[] { Condition(name, "GT", 20m) }
                            : ReadRange(name, label, lower.Text, upper.Text);
                }
            }
            else
            {
                TextBox text = new TextBox { Style = (Style)FindResource("ProductFilterInput"), Tag = "搜索" + label + "..." };
                row.Children.Add(text);
                clear.Add(text.Clear);
                bool contains = ((JArray)definition["operators"]).Values<string>().Contains("CONTAINS") && !name.EndsWith("Id");
                readers[name] = () => string.IsNullOrWhiteSpace(text.Text) ? Enumerable.Empty<JObject>()
                    : new[] { Condition(name, contains ? "CONTAINS" : "EQ", text.Text.Trim()) };
            }
            return row;
        }

        private static IEnumerable<JObject> ReadRange(string name, string label, string lower, string upper)
        {
            decimal? minimum = ParseNumber(label, lower);
            decimal? maximum = ParseNumber(label, upper);
            if (minimum.HasValue && maximum.HasValue)
            {
                if (minimum > maximum)
                {
                    throw new InvalidOperationException(label + "：最小值不能大于最大值。");
                }
                yield return Condition(name, "BETWEEN", new JArray(minimum.Value, maximum.Value));
            }
            else if (minimum.HasValue)
            {
                yield return Condition(name, "GTE", minimum.Value);
            }
            else if (maximum.HasValue)
            {
                yield return Condition(name, "LTE", maximum.Value);
            }
        }

        private void UpdatePriceUnits(bool cleanPrice)
        {
            foreach (string name in new[] { "bidPrice", "offerPrice", "bidValuationSpread", "offerValuationSpread" })
            {
                if (priceUnits.TryGetValue(name, out TextBlock suffix))
                {
                    suffix.Text = cleanPrice ? "元" : name.EndsWith("Spread") ? "BP" : "%";
                }
            }
        }

        private static decimal? ParseNumber(string label, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            if (!decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
            {
                throw new InvalidOperationException(label + "：请输入有效数值。");
            }
            return value;
        }

        private static JObject Condition(string field, string operation, JToken value)
        {
            return new JObject { ["field"] = field, ["operator"] = operation, ["value"] = value };
        }

        public JObject Read()
        {
            JObject filter = new JObject { ["conditions"] = new JArray(readers.Values.SelectMany(read => read())) };
            JArray conditions = (JArray)filter["conditions"];
            bool cleanPrice = conditions.OfType<JObject>().Any(condition => (string)condition["field"] == "quoteType"
                && ((JArray)condition["value"]).Values<string>().Contains("2"));
            if (cleanPrice)
            {
                foreach (JObject condition in conditions.OfType<JObject>())
                {
                    string field = (string)condition["field"];
                    if (field == "bidValuationSpread" || field == "offerValuationSpread")
                    {
                        condition["field"] = field == "bidValuationSpread" ? "bidCleanPriceSpread" : "offerCleanPriceSpread";
                    }
                }
            }
            if (selectedWatchlist != null)
            {
                filter["watchlistIds"] = new JArray(selectedWatchlist);
            }
            if (selectedGroup != null)
            {
                filter["filterGroupIds"] = new JArray(selectedGroup);
            }
            return filter;
        }

        public void Reset()
        {
            search.Clear();
            selectedWatchlist = null;
            selectedGroup = null;
            foreach (Action action in clear)
            {
                action();
            }
        }

        public void SetSavedGroups(JArray lists, JArray filters)
        {
            FillGroups(watchlists, lists, "watchlistId", id => selectedWatchlist = id);
            FillGroups(groups, filters, "filterGroupId", id => selectedGroup = id);
        }

        private void FillGroups(Panel panel, JArray records, string idField, Action<string> select)
        {
            panel.Children.Clear();
            foreach (JObject record in records.OfType<JObject>())
            {
                Button button = UiElements.Button((string)record["name"], async () => await execute(async () =>
                {
                    select((string)record[idField]);
                    await apply(Read());
                }));
                button.Padding = new Thickness(8, 2, 8, 2);
                button.Margin = new Thickness(0, 0, 3, 3);
                button.FontSize = 13;
                panel.Children.Add(button);
            }
            if (records.Count == 0)
            {
                panel.Children.Add(new TextBlock { Text = "暂无保存的分组", Margin = new Thickness(4), Opacity = 0.6 });
            }
        }
    }
}
