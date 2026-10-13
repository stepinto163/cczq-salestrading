using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;
using STC.SalesTrading.Plugin.Modules.QuoteMgmt.View;
using STC.SalesTrading.Plugin.Shared.Components;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private JObject originalNotificationPreference;
        private JObject testWatchlist;

        private async Task ExecuteRegressionAsync(TestStep step)
        {
            switch (step.Action)
            {
                case "recommendation.assert":
                    var market = model.MarketCenterPage;
                    var target = market.RateAll.Rows.Single(row => row.QuoteId == quoteId);
                    var marketGrid = Descendants<DataGrid>(view).First(grid => grid.IsVisible && grid.Items.Contains(target));
                    marketGrid.SelectedItem = target;
                    marketGrid.ScrollIntoView(target);
                    await Until(() => market.Detail.Code == target.Code && !market.Detail.RecommendationLoading
                        && market.Detail.Recommendations.Any(row => row.Evidence.Any(item => item.QuoteId == quoteId)),
                        "选中行情行自动加载真实推荐证据");
                    var recommendation = market.Detail.Recommendations.Single(row => row.Evidence.Any(item => item.QuoteId == quoteId));
                    Require(!recommendation.IsMock && recommendation.QuoteCount > 0 && recommendation.Score > 0,
                        "推荐未采用真实正式报价证据");
                    Require(!string.IsNullOrWhiteSpace(recommendation.OwnerTrader), "内部归属交易员未显示");
                    var recommendationGrid = Descendants<DataGrid>(view).Single(grid => grid.Name == "RecommendationGrid");
                    recommendationGrid.ScrollIntoView(recommendation);
                    recommendationGrid.UpdateLayout();
                    var recommendationContainer = (DataGridRow)recommendationGrid.ItemContainerGenerator.ContainerFromItem(recommendation);
                    Click(Descendants<Button>(recommendationContainer).Single(button => Convert.ToString(button.Content) == "展开"));
                    await Until(() => recommendationContainer.DetailsVisibility == Visibility.Visible, "展开真实证据");
                    market.Detail.SelectedRecSide = STC.SalesTrading.Plugin.Abstractions.Models.QuoteDirection.Ofr;
                    Require(market.Detail.RecPageItems.All(row => row.Side == STC.SalesTrading.Plugin.Abstractions.Models.QuoteDirection.Ofr), "Ofr过滤错误");
                    market.Detail.SelectedRecSide = STC.SalesTrading.Plugin.Abstractions.Models.QuoteDirection.Bid;
                    market.Detail.SelectedRecWindow = "7d";
                    await Until(() => !market.Detail.RecommendationLoading && market.Detail.Recommendations.Any(row => row.Evidence.Any(item => item.QuoteId == quoteId)), "日期切换后重新计算推荐");
                    recommendation = market.Detail.Recommendations.Single(row => row.Evidence.Any(item => item.QuoteId == quoteId));
                    recommendationGrid.ScrollIntoView(recommendation, recommendationGrid.Columns.Last());
                    recommendationGrid.UpdateLayout();
                    recommendationContainer = (DataGridRow)recommendationGrid.ItemContainerGenerator.ContainerFromItem(recommendation);
                    if (recommendation.CanFeedback)
                    {
                        Click(Descendants<Button>(recommendationContainer).Single(button => Convert.ToString(button.Content) == "不适用"));
                        await Until(() => !recommendation.CanFeedback, "真实反馈保存并锁定按钮");
                    }
                    string savedFeedback = recommendation.Feedback;
                    await market.Detail.ReloadRecommendationsAsync();
                    Require(market.Detail.Recommendations.Single(row => row.RecommendationId == recommendation.RecommendationId).Feedback == savedFeedback,
                        "刷新后反馈状态丢失");
                    File.WriteAllText(Path.Combine(reportDirectory, "recommendation.json"), JObject.FromObject(new
                    {
                        quoteId, recommendation.RecommendationId, recommendation.QuoteCount, recommendation.Score,
                        recommendation.OwnerTrader, market.Detail.Code, savedFeedback
                    }).ToString());
                    break;
                case "management.select":
                    await OpenSidebarAsync("mgmt");
                    await Until(() => !model.QuoteMgmtPage.Today.IsLoading, "管理页查询完成");
                    DataGrid managementGrid = Descendants<DataGrid>(view)
                        .Single(grid => grid.IsVisible && grid.Name == "TodayGrid");
                    managementGrid.SelectedItem = model.QuoteMgmtPage.Today.Rows.Single(row => row.QuoteId == quoteId);
                    Require(model.QuoteMgmtPage.Today.SelectedRow.Count == 1, "实际表格选中报价未同步到操作目标");
                    break;
                case "filter.latency":
                    await MeasureFilterLatencyAsync(step);
                    break;
                case "qtrade.negotiation":
                    await CheckQTradeNegotiationAsync();
                    break;
                case "quote.fields":
                    JObject stored = await model.Backend.Api.GetQuoteAsync(quoteId);
                    foreach (string field in new[] { "side", "settlType" })
                    {
                        Require((string)stored[field] == Parameter(step, field), field + " 不匹配：" + stored[field]);
                    }
                    Require((decimal)stored["quantity"] == decimal.Parse(Parameter(step, "quantity"), CultureInfo.InvariantCulture), "数量不匹配");
                    File.WriteAllText(Path.Combine(reportDirectory, stepNumber.ToString("D2") + "-fields.json"), stored.ToString());
                    break;
                case "manual.reject":
                    Require(editor != null && editor.IsReady, "请先打开手工报价");
                    JObject before = editor.CurrentQuote == null ? null : (JObject)editor.CurrentQuote.DeepClone();
                    Click(Button(editor, Parameter(step, "button")));
                    await Until(() => editor.IsReady && Descendants<TextBlock>(editor)
                        .Any(text => text.Text.Contains(Parameter(step, "message"))), "明确的拒绝提示：" + Parameter(step, "message"));
                    Require(JToken.DeepEquals(before, editor.CurrentQuote), "拒绝操作改变了报价");
                    if (before != null)
                    {
                        JObject after = await model.Backend.Api.GetQuoteAsync((string)before["quoteId"]);
                        Require(JToken.DeepEquals(before["revision"], after["revision"]), "拒绝操作改变了服务端版本");
                    }
                    break;
                case "sidebar.range":
                    FrameworkElement rangePanel = await OpenSidebarAsync(Parameter(step, "page"));
                    FilterRangeRow range = Descendants<FilterRangeRow>(rangePanel)
                        .Single(row => row.IsVisible && SidebarFilter.GetField(row) == Parameter(step, "field"));
                    TextBox[] inputs = Descendants<TextBox>(range).ToArray();
                    SetText(inputs[0], Parameter(step, "min"));
                    SetText(inputs[1], Parameter(step, "max"));
                    await AssertSidebarResultAsync(Parameter(step, "page"), bool.Parse(Parameter(step, "visible")));
                    break;
                case "sidebar.search":
                    FrameworkElement searchPanel = await OpenSidebarAsync(Parameter(step, "page"));
                    TextBox search = Descendants<TextBox>(searchPanel).Single(input =>
                        BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path?.Path == "FilterSearchText");
                    SetText(search, Parameter(step, "text"));
                    await AssertSidebarResultAsync(Parameter(step, "page"), bool.Parse(Parameter(step, "visible")));
                    break;
                case "sidebar.reset":
                    FrameworkElement resetPanel = await OpenSidebarAsync(Parameter(step, "page"));
                    Click(Button(resetPanel, "清除过滤"));
                    await AssertSidebarResultAsync(Parameter(step, "page"), true);
                    Require(Descendants<FilterRangeRow>(resetPanel).Where(row => row.IsVisible)
                        .All(row => string.IsNullOrEmpty(row.MinText) && string.IsNullOrEmpty(row.MaxText)), "清除过滤后区间控件仍有值");
                    break;
                case "parse.row":
                    var row = model.QuotePage.Precise.Rows[int.Parse(Parameter(step, "row")) - 1];
                    Require(row.Code == Parameter(step, "securityId"), "识别债券不匹配：" + row.Code);
                    Require(row.Direction == Parameter(step, "side"), "识别方向不匹配：" + row.Direction);
                    Require(decimal.Parse(row.Price, CultureInfo.InvariantCulture) == decimal.Parse(Parameter(step, "price"), CultureInfo.InvariantCulture), "识别价格不匹配：" + row.Price);
                    Require(decimal.Parse(row.Volume, CultureInfo.InvariantCulture) == decimal.Parse(Parameter(step, "quantity"), CultureInfo.InvariantCulture), "识别数量不匹配：" + row.Volume);
                    break;
                case "parse.count":
                    Require(model.QuotePage.Precise.Rows.Count == int.Parse(Parameter(step, "rows")), "剩余预览条数不匹配");
                    Require(model.QuotePage.Precise.SelectedCount == int.Parse(Parameter(step, "selected")), "预览选中数量不匹配");
                    Require(model.QuotePage.Precise.Rows.All(preview => preview.Row.Revision == 0), "未确认预览不应已持久化");
                    break;
                case "parse.toolbar":
                    var toolbarView = Descendants<PreciseQuoteView>(view).Single(control => control.IsVisible);
                    string binding = Parameter(step, "control");
                    Require(new[] { "ToolbarPriceType", "ToolbarSettlement", "AmountInput", "BpInput" }.Contains(binding), "不支持的工具栏字段");
                    if (binding.StartsWith("Toolbar"))
                    {
                        ComboBox choice = Descendants<ComboBox>(toolbarView).Single(control =>
                            BindingOperations.GetBinding(control, ComboBox.SelectedItemProperty)?.Path?.Path == binding);
                        choice.SelectedItem = Parameter(step, "value");
                    }
                    else
                    {
                        TextBox input = Descendants<TextBox>(toolbarView).Single(control =>
                            BindingOperations.GetBinding(control, TextBox.TextProperty)?.Path?.Path == binding);
                        SetText(input, Parameter(step, "value"));
                    }
                    break;
                case "parse.warning":
                    Require(model.QuotePage.Precise.StatusMessage.Contains(Parameter(step, "message")),
                        "缺少识别警告：" + model.QuotePage.Precise.StatusMessage);
                    Require(Descendants<TextBlock>(view).Any(text => text.IsVisible && text.Text.Contains(Parameter(step, "message"))),
                        "识别警告未显示在界面");
                    break;
                case "parse.settlement":
                    var datedRow = model.QuotePage.Precise.Rows[int.Parse(Parameter(step, "row")) - 1];
                    Require(datedRow.Settlement == Parameter(step, "settlement"), "清算速度不匹配：" + datedRow.Settlement);
                    Require(datedRow.Row.TradeDate?.Date == DateTime.Today.AddDays(int.Parse(Parameter(step, "dayOffset"))), "交易日期不匹配：" + datedRow.Row.TradeDate);
                    Require(datedRow.PriceType == Parameter(step, "priceType"), "计价方式不匹配：" + datedRow.PriceType);
                    break;
                case "parse.click":
                    var precise = Descendants<PreciseQuoteView>(view).Single(control => control.IsVisible);
                    Click(Button(precise, Parameter(step, "button")));
                    if (Parameter(step, "button") == "确认报价")
                    {
                        await AwaitCommand(model.QuotePage.Precise.ConfirmSelectedCommand.ExecutionTask);
                    }
                    await Application.Current.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "notification.prepare":
                    await PrepareNotificationFixtureAsync(Parameter(step, "securityId"));
                    break;
                case "notification.read":
                    await ReadNotificationThroughUiAsync();
                    break;
                default:
                    throw new InvalidOperationException("不支持的动作：" + step.Action);
            }
        }

        private static void SetText(TextBox input, string value)
        {
            input.BringIntoView();
            Require(input.IsVisible && input.IsEnabled, "输入框不可操作");
            input.Focus();
            input.Text = value;
        }

        private async Task<FrameworkElement> OpenSidebarAsync(string page)
        {
            Require(page == "mgmt" || page == "market", "侧栏 page 必须为 mgmt/market");
            CloseEditor();
            await NavigateAsync(page);
            if (page == "mgmt")
            {
                return Descendants<MgmtFilterPanel>(view).Single(panel => panel.IsVisible);
            }
            object tab = model.MarketCenterPage.SubTabs.Single(item => item.Key == "rateAll");
            Descendants<ListBox>(view).First(list => list.Items.Contains(tab)).SelectedItem = tab;
            await Application.Current.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            view.UpdateLayout();
            return Descendants<MarketFilterPanel>(view).Single(panel => panel.IsVisible);
        }

        private async Task AssertSidebarResultAsync(string page, bool visible)
        {
            Require(ownedQuotes.Count > 0, "筛选必须先创建本例报价，不能用空列表冒充通过");
            JObject lastState = null;
            try
            {
                await Until(() =>
                {
                    bool loading = page == "mgmt" ? model.QuoteMgmtPage.Today.IsLoading : model.MarketCenterPage.RateAll.IsLoading;
                    string error = page == "mgmt" ? model.QuoteMgmtPage.Today.ErrorMessage : model.MarketCenterPage.RateAll.ErrorMessage;
                    string[] ids = page == "mgmt" ? model.QuoteMgmtPage.Today.Rows.Select(row => row.QuoteId).ToArray()
                        : model.MarketCenterPage.RateAll.Rows.Select(row => row.QuoteId).ToArray();
                    lastState = new JObject
                    {
                        ["page"] = page, ["expectedVisible"] = visible, ["loading"] = loading,
                        ["error"] = error, ["ownedQuoteIds"] = new JArray(ownedQuotes), ["actualQuoteIds"] = new JArray(ids)
                    };
                    return !loading && string.IsNullOrEmpty(error) && ownedQuotes.All(id => ids.Contains(id) == visible);
                }, page + " 筛选后本例报价应" + (visible ? "显示" : "消失"));
            }
            finally
            {
                File.WriteAllText(Path.Combine(reportDirectory, stepNumber.ToString("D2") + "-sidebar.json"), lastState?.ToString() ?? "{}");
            }
            var displayedIds = Descendants<DataGrid>(view).Where(grid => grid.IsVisible)
                .SelectMany(grid => grid.Items.Cast<object>()).Select(item => (string)item.GetType().GetProperty("QuoteId")?.GetValue(item)).ToArray();
            Require(ownedQuotes.All(id => displayedIds.Contains(id) == visible), "视图模型与实际表格不一致");
            if (page == "mgmt")
            {
                DataGrid grid = Descendants<DataGrid>(view).Single(item => item.IsVisible && item.Name == "TodayGrid");
                Require(grid.SelectedItems.Count == 0 && model.QuoteMgmtPage.Today.SelectedRow.Count == 0,
                    "筛选刷新后不得保留旧报价的操作选择");
            }
        }

        private async Task PrepareNotificationFixtureAsync(string securityId)
        {
            Require(originalNotificationPreference == null, "每例只准备一次提醒夹具");
            originalNotificationPreference = (JObject)(await model.Backend.Api.RequestProjectionAsync(
                SalesTradingApi.NotificationPreferenceGet, new Request()))["preference"];
            File.WriteAllText(Path.Combine(reportDirectory, "original-notification-preference.json"), originalNotificationPreference.ToString());
            await model.Backend.Api.RequestProjectionAsync(SalesTradingApi.NotificationPreferenceSave,
                new NotificationPreferenceSaveRequest
                {
                    ExpectedRevision = (string)originalNotificationPreference["revision"],
                    WatchlistEnabled = true,
                    FilterGroupEnabled = (bool)originalNotificationPreference["filterGroupEnabled"],
                    CounterQuoteEnabled = (bool)originalNotificationPreference["counterQuoteEnabled"],
                    MyBestEnabled = (bool)originalNotificationPreference["myBestEnabled"]
                });
            testWatchlist = (JObject)(await model.Backend.Api.RequestProjectionAsync(SalesTradingApi.WatchlistSave,
                new WatchlistSaveRequest
                {
                    Name = "UI-" + Guid.NewGuid().ToString("N"),
                    Market = "RATE",
                    Scope = "MARKET",
                    Securities = new System.Collections.Generic.List<WatchlistSecurity>
                    {
                        new WatchlistSecurity { SecurityId = securityId }
                    }
                }))["watchlist"];
            Require(testWatchlist != null, "未返回测试关注分组");
            File.WriteAllText(Path.Combine(reportDirectory, "owned-watchlist.json"), testWatchlist.ToString());
        }

        private async Task<JArray> QueryNotificationsAsync()
        {
            JObject data = await model.Backend.Api.RequestProjectionAsync(SalesTradingApi.NotificationQuery,
                new NotificationQueryRequest { Limit = 100 });
            return (JArray)data["notifications"];
        }

        private async Task ReadNotificationThroughUiAsync()
        {
            Require(quoteId != null, "请先创建并确认本例报价");
            CloseEditor();
            await NavigateAsync("market");
            object tab = model.MarketCenterPage.SubTabs.Single(item => item.Key == "alert");
            Descendants<ListBox>(view).First(list => list.Items.Contains(tab)).SelectedItem = tab;
            view.UpdateLayout();
            JObject notification = null;
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (notification == null)
            {
                ThrowIfCancelled();
                notification = (await QueryNotificationsAsync()).OfType<JObject>().FirstOrDefault(item =>
                    item["quoteIds"] is JArray quoteIds && quoteIds.Values<string>().Contains(quoteId));
                if (notification != null)
                {
                    break;
                }
                Require(DateTime.UtcNow < deadline, "通知服务未返回本例报价的提醒");
                await Task.Delay(200);
            }
            string notificationId = (string)notification["notificationId"];
            Require(notification["readTime"] == null || notification["readTime"].Type == JTokenType.Null, "新建提醒应为未读");
            await Until(() => model.MarketCenterPage.Alert.Rows.Any(row => row.AlertKey == notificationId), "本例提醒推送到 UI");
            BoardAlertView alert = Descendants<BoardAlertView>(view).Single(control => control.IsVisible);
            DataGrid grid = (DataGrid)alert.FindName("Grid");
            object target = grid.Items.Cast<object>().First(item => (string)item.GetType().GetProperty("AlertKey")?.GetValue(item) == notificationId);
            grid.ScrollIntoView(target);
            grid.SelectedItem = target;
            Click(Button(alert, "标记已读"));
            await AwaitCommand(model.MarketCenterPage.Alert.MarkReadCommand.ExecutionTask);
            await Until(() => model.MarketCenterPage.Alert.Rows.Any(row => row.AlertKey == notificationId && row.IsRead), "界面已读状态更新");
            JObject persisted = (await QueryNotificationsAsync()).OfType<JObject>().Single(item => (string)item["notificationId"] == notificationId);
            Require(persisted["readTime"] != null && persisted["readTime"].Type != JTokenType.Null, "已读状态未持久化");
            File.WriteAllText(Path.Combine(reportDirectory, "notification-read.json"), persisted.ToString());
        }

        private async Task CleanupNotificationFixtureAsync()
        {
            var failures = new System.Collections.Generic.List<string>();
            if (testWatchlist != null)
            {
                try
                {
                    JObject deleted = await model.Backend.Api.RequestProjectionAsync(SalesTradingApi.WatchlistDelete,
                        new WatchlistDeleteRequest
                        {
                            WatchlistId = (string)testWatchlist["watchlistId"],
                            ExpectedRevision = (string)testWatchlist["revision"]
                        });
                    Require((bool?)deleted["deleted"] == true, "关注分组删除未确认");
                }
                catch (Exception error)
                {
                    failures.Add(error.ToString());
                }
            }
            if (originalNotificationPreference != null)
            {
                try
                {
                    JObject current = (JObject)(await model.Backend.Api.RequestProjectionAsync(
                        SalesTradingApi.NotificationPreferenceGet, new Request()))["preference"];
                    JObject restored = (JObject)(await model.Backend.Api.RequestProjectionAsync(SalesTradingApi.NotificationPreferenceSave,
                        new NotificationPreferenceSaveRequest
                        {
                            ExpectedRevision = (string)current["revision"],
                            WatchlistEnabled = (bool)originalNotificationPreference["watchlistEnabled"],
                            FilterGroupEnabled = (bool)originalNotificationPreference["filterGroupEnabled"],
                            CounterQuoteEnabled = (bool)originalNotificationPreference["counterQuoteEnabled"],
                            MyBestEnabled = (bool)originalNotificationPreference["myBestEnabled"]
                        }))["preference"];
                    Require(JToken.DeepEquals(restored["watchlistEnabled"], originalNotificationPreference["watchlistEnabled"]), "提醒偏好恢复未确认");
                    File.WriteAllText(Path.Combine(reportDirectory, "notification-cleanup.json"), restored.ToString());
                }
                catch (Exception error)
                {
                    failures.Add(error.ToString());
                }
            }
            Require(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }
    }
}
