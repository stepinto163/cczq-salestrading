using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Newtonsoft.Json.Linq;
using SalesTrading.UiTests.Contracts;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Modules.QuoteMgmt.View;
using STC.SalesTrading.Plugin.Shared.Components;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private readonly Dictionary<string, JObject> filterFixtures = new Dictionary<string, JObject>();

        // 仅六笔功能夹具，通过真实识别、选择、确认流程生成；不走性能测试批量写入路径。
        private async Task PrepareFilterCombinationAsync(TestStep step)
        {
            Require(filterFixtures.Count == 0, "组合筛选夹具只能准备一次");
            string security = Parameter(step, "securityId");
            var existing = await api.RequestAsync(SalesTradingApi.QuoteQuery, new QuoteQueryRequest
            {
                Limit = 1,
                Status = new List<string> { "ACTIVE", "REFERRED" },
                Filter = new Filter { Conditions = new List<FilterCondition>
                {
                    new FilterCondition { Field = "securityId", Operator = "EQ", Value = security },
                    new FilterCondition { Field = "price", Operator = "BETWEEN", Value = new JArray(6.1m, 6.7m) }
                } }
            });
            Require(existing.TotalCount == 0, "功能测试价格区间 6.1–6.7 已有报价；请换测试债券，不覆盖现有数据");
            string[] aliases = { "B1", "B2", "B3", "O1", "O2", "O3" };
            string[] prices = { "6.101", "6.202", "6.303", "6.404", "6.505", "6.606" };
            string[] quantities = { "100", "300", "500", "200", "400", "600" };
            string[] settlement = { "1", "2", "1", "1", "2", "2" };
            string text = string.Join("\n", Enumerable.Range(0, 6).Select(index => security + " " + prices[index]
                + " " + quantities[index] + (index < 3 ? " bid " : " ofr ") + (settlement[index] == "1" ? "+0" : "+1")));
            await ExecuteActionAsync(new TestStep { Action = "parse", Parameters = new JObject
            {
                ["text"] = text, ["expectedRows"] = "6"
            } });
            for (int index = 0; index < 6; index++)
            {
                await ExecuteActionAsync(new TestStep { Action = "parse.select", Parameters = new JObject
                {
                    ["row"] = (index + 1).ToString(), ["selected"] = "true", ["expectedCount"] = (index + 1).ToString()
                } });
            }
            await ExecuteActionAsync(new TestStep { Action = "parse.confirm", Parameters = new JObject { ["expectedCount"] = "6" } });
            Require(ownedQuotes.Count == 6, "必须创建六笔独立报价，不能用空列表验证过滤");
            foreach (string id in ownedQuotes)
            {
                JObject quote = await api.GetQuoteAsync(id);
                int index = Array.FindIndex(prices, price => decimal.Parse(price, CultureInfo.InvariantCulture) == (decimal)quote["price"]);
                Require(index >= 0 && !filterFixtures.ContainsKey(aliases[index]), "夹具价格重复或识别错误");
                Require((string)quote["securityId"] == security && (string)quote["side"] == (index < 3 ? "1" : "2")
                    && (decimal)quote["quantity"] == decimal.Parse(quantities[index], CultureInfo.InvariantCulture)
                    && (string)quote["settlType"] == settlement[index] && (string)quote["quoteType"] == "1",
                    "组合夹具方向、数量、清算或计价方式不匹配：" + aliases[index]);
                filterFixtures.Add(aliases[index], quote);
            }
            File.WriteAllText(Path.Combine(reportDirectory, "filter-fixtures.json"), JObject.FromObject(filterFixtures).ToString());
        }

        private async Task CheckFilterCombinationAsync(TestStep step)
        {
            Require(filterFixtures.Count == 6, "请先准备组合筛选夹具");
            string page = Parameter(step, "page");
            string tab = Parameter(step, "tab");
            Require((page == "mgmt" && tab == "today") || (page == "market" && (tab == "rateAll" || tab == "rateBest")),
                "组合筛选仅支持今日报价、利率全部和利率最优");
            await NavigateAsync(page);
            if (page == "market")
            {
                await SelectMarketTabAsync(tab);
            }
            FrameworkElement panel = page == "mgmt"
                ? (FrameworkElement)Descendants<MgmtFilterPanel>(view).Single(item => item.IsVisible)
                : Descendants<MarketFilterPanel>(view).Single(item => item.IsVisible);
            if (bool.Parse(Parameter(step, "reset")))
            {
                await ClickAsync(Button(panel, "清除过滤"));
            }
            JObject ranges = JObject.Parse(Parameter(step, "ranges"));
            JObject tags = JObject.Parse(Parameter(step, "tags"));
            foreach (var range in ranges.Properties())
            {
                Require(new[] { "bidPrice", "offerPrice", "bidQuantity", "offerQuantity" }.Contains(range.Name),
                    "本组只使用已接通的报价价格和数量字段：" + range.Name);
                var editor = Descendants<FilterRangeRow>(panel).Single(item => item.IsVisible && SidebarFilter.GetField(item) == range.Name);
                string[] bounds = ((JArray)range.Value).Values<string>().ToArray();
                Require(bounds.Length == 2, "区间必须提供最小值、最大值；空字符串表示不限制");
                var inputs = Descendants<TextBox>(editor).ToArray();
                await SetTextAsync(inputs[0], bounds[0]);
                if (editor.ShowMax)
                {
                    await SetTextAsync(inputs[1], bounds[1]);
                }
                else
                {
                    Require(string.IsNullOrEmpty(bounds[1]), "数量控件仅支持下限");
                }
            }
            foreach (var tag in tags.Properties())
            {
                Require(new[] { "settlType", "quoteType" }.Contains(tag.Name), "本组不使用未核实的资料枚举：" + tag.Name);
                var editor = Descendants<FrameworkElement>(panel).Single(item => item.IsVisible && SidebarFilter.GetField(item) == tag.Name);
                string[] labels = ((JArray)tag.Value).Values<string>().ToArray();
                Require(tag.Name != "quoteType" || labels.Length <= 1, "计价方式是单选；多选 OR 用清算速度验证");
                var buttons = Descendants<ToggleButton>(editor).Where(button => button.IsVisible).ToArray();
                Require(labels.All(label => buttons.Any(button => Convert.ToString(button.Content) == label)), "缺少枚举选项：" + tag.Name);
                foreach (var button in buttons)
                {
                    bool selected = labels.Contains(Convert.ToString(button.Content));
                    if ((button.IsChecked == true) != selected)
                    {
                        await ClickAsync(button);
                    }
                }
            }
            var search = Descendants<TextBox>(panel).SingleOrDefault(input => input.IsVisible
                && BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path?.Path == "FilterSearchText");
            Require(search != null, "当前侧栏缺少已绑定 FilterSearchText 的搜索输入：" + page);
            string searchText = Parameter(step, "search");
            await SetTextAsync(search, searchText == "<empty>" ? "" : searchText);
            // 等待文本控件自身的 300ms 防抖；随后显式点击筛选并等待本次查询完成。
            await Task.Delay(400);
            var pageView = Descendants<FrameworkElement>(view).Single(item => item.IsVisible && item.GetType().Name == Parameter(step, "view"));
            var state = pageView.DataContext;
            var changed = state as INotifyPropertyChanged;
            Require(changed != null, "页面未提供查询状态通知");
            Func<bool> loading = () => (bool)state.GetType().GetProperty("IsLoading").GetValue(state);
            bool sawLoading = false;
            PropertyChangedEventHandler listener = (sender, args) =>
            {
                if (args.PropertyName == "LoadState" || args.PropertyName == "IsLoading") sawLoading |= loading();
            };
            changed.PropertyChanged += listener;
            try
            {
                await ClickAsync(Descendants<Button>(panel).First(button => button.IsVisible && Convert.ToString(button.Content) == "筛选"));
                try
                {
                    await Until(() => sawLoading && !loading(), "本次组合筛选查询完成");
                }
                catch
                {
                    File.WriteAllText(Path.Combine(reportDirectory, "filter-query-failure.json"), JObject.FromObject(new
                    {
                        sawLoading, loading = loading(), stateType = state.GetType().FullName,
                        error = state.GetType().GetProperty("ErrorMessage")?.GetValue(state),
                        messages = STC.SalesTrading.Plugin.Shared.Services.ToastService.Messages.Select(message => message.Text).ToArray()
                    }).ToString());
                    throw;
                }
            }
            finally
            {
                changed.PropertyChanged -= listener;
            }
            string error = Convert.ToString(state.GetType().GetProperty("ErrorMessage")?.GetValue(state));
            Require(string.IsNullOrEmpty(error), "组合筛选查询失败：" + error);
            DataGrid grid = Descendants<DataGrid>(pageView).Single(item => item.IsVisible);
            string[] aliases = Parameter(step, "expected") == "NONE" ? new string[0] : Parameter(step, "expected").Split(',');
            Require(aliases.All(filterFixtures.ContainsKey), "未知预期报价别名");
            var expected = new HashSet<string>(aliases.Select(alias => (string)filterFixtures[alias]["quoteId"]));
            Func<HashSet<string>> actual = () => new HashSet<string>(grid.Items.Cast<object>()
                .SelectMany(item => tab == "rateBest"
                    ? (string[])item.GetType().GetProperty("SourceQuoteIds").GetValue(item)
                    : new[] { (string)item.GetType().GetProperty("QuoteId").GetValue(item) })
                .Where(ownedQuotes.Contains));
            var scope = Descendants<FrameworkElement>(panel).Single(item => item.IsVisible && SidebarFilter.GetScope(item) == (page == "mgmt" ? "Combined" : "Rate"));
            JObject evidence = new JObject
            {
                ["step"] = step.Name, ["tab"] = tab, ["filter"] = SidebarFilter.Read(scope).ToJson(),
                ["search"] = searchText, ["expectedAliases"] = new JArray(aliases),
                ["expectedQuoteIds"] = new JArray(expected), ["actualQuoteIds"] = new JArray(actual()),
                ["queryId"] = Convert.ToString(state.GetType().GetProperty("QueryId")?.GetValue(state)),
                ["queryCycleObserved"] = sawLoading
            };
            File.WriteAllText(Path.Combine(reportDirectory, stepNumber.ToString("D2") + "-combination.json"), evidence.ToString());
            await VerifyAsync(grid, step.Name + " · 预期 " + (aliases.Length == 0 ? "无本例报价" : string.Join(",", aliases)),
                () => Require(expected.SetEquals(actual()), "组合筛选结果不符：" + evidence));
            if (expected.Count > 0)
            {
                object row = grid.Items.Cast<object>().First(item => expected.Any(id => RowIncludesQuote(item, id)));
                await VerifyAsync(FindVerificationRow(row), "组合筛选命中 · " + string.Join(",", aliases), () =>
                {
                    Require(expected.SetEquals(actual()), "展示时筛选结果发生变化");
                    if (tab == "rateBest")
                    {
                        foreach (string side in new[] { "B", "O" })
                        {
                            var selected = aliases.Where(alias => alias.StartsWith(side)).Select(alias => filterFixtures[alias]).ToArray();
                            decimal? expectedPrice = selected.Length == 0 ? (decimal?)null : (decimal)selected[0]["price"];
                            decimal? expectedQuantity = selected.Length == 0 ? (decimal?)null : selected.Sum(item => (decimal)item["quantity"]);
                            string priceProperty = side == "B" ? "Bid" : "Ofr";
                            Require((decimal?)row.GetType().GetProperty(priceProperty).GetValue(row) == expectedPrice,
                                "最优价格错误：" + priceProperty + "，预期 " + expectedPrice);
                            Require((decimal?)row.GetType().GetProperty(priceProperty + "Volume").GetValue(row) == expectedQuantity,
                                "最优报量错误：" + priceProperty + "，预期 " + expectedQuantity);
                        }
                    }
                });
            }
            Require(grid.EnableRowVirtualization, "组合筛选必须保留行虚拟化");
        }
    }
}
