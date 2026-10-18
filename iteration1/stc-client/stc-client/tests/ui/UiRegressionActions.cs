using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json.Linq;
using SalesTrading.UiTests.Contracts;
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

        // 在正式 STC 宿主中检查最终字号，捕获共享样式覆盖局部字号的回归。
        private async Task CheckRecommendationTypographyAsync()
        {
            var detail = model.MarketCenterPage.Detail;
            var recommendation = detail.Recommendations.Single(row => row.Evidence.Any(item => item.QuoteId == quoteId));
            var grid = RecommendationGrid();
            await ShowRecommendationRowAsync(true);
            await Until(() => grid.ItemContainerGenerator.ContainerFromItem(recommendation) is DataGridRow,
                "推荐行渲染完成");
            var container = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(recommendation);
            var feedbackButtons = Descendants<Button>(container)
                .Where(button => new[] { "有用", "无用", "不适用" }.Contains(Convert.ToString(button.Content))).ToArray();
            Require(feedbackButtons.Length == 3, "反馈按钮未完整渲染");
            foreach (var button in feedbackButtons)
            {
                await VerifyAsync(button, "推荐反馈 · " + button.Content + " · 字号预期 11，实际 " + button.FontSize,
                    () => Require(button.FontSize == 11, "反馈按钮字号应为11"));
            }
            var ownerTexts = Descendants<TextBlock>(container).Where(text => text.Text == recommendation.OwnerTrader).ToArray();
            Require(ownerTexts.Length > 0, "验证目标缺失：长城交易员");
            foreach (var text in ownerTexts)
            {
                await VerifyAsync(text, "长城交易员 · 字号预期 11，实际 " + text.FontSize,
                    () => Require(text.FontSize == 11, "长城交易员字号应为11"));
            }
            var feedbackTexts = Descendants<TextBlock>(container).Where(text => BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path?.Path == "FeedbackText").ToArray();
            Require(feedbackTexts.Length > 0, "验证目标缺失：反馈状态");
            foreach (var text in feedbackTexts)
            {
                await VerifyAsync(text, "反馈状态 · 字号预期 10，实际 " + text.FontSize,
                    () => Require(text.FontSize == 10, "反馈状态字号应为10"));
            }
            var panel = Descendants<BondDetailPanel>(view).Single(item => Descendants<DataGrid>(item).Contains(grid));
            var refresh = Descendants<Button>(panel).Single(button => Convert.ToString(button.Content) == "刷新");
            await VerifyAsync(refresh, "刷新按钮 · 字号预期 11，实际 " + refresh.FontSize, () => Require(refresh.FontSize == 11, "刷新按钮字号应为11"));
            var title = Descendants<TextBlock>(panel).Single(text => text.Text == "智能推荐");
            await VerifyAsync(title, "智能推荐标题 · 字号预期 12，实际 " + title.FontSize, () => Require(title.FontSize == 12, "推荐标题字号应为12"));
            File.WriteAllText(Path.Combine(reportDirectory, "recommendation-typography.json"), JObject.FromObject(new
            {
                panelWidth = panel.ActualWidth,
                titleFont = title.FontSize,
                refreshFont = refresh.FontSize,
                ownerFont = ownerTexts[0].FontSize,
                feedbackFont = feedbackButtons[0].FontSize,
                feedbackStatusFont = feedbackTexts[0].FontSize
            }).ToString());
        }

        // 小样本通过真实精确报价流程生成；接口跨页由 history.assert 单独检查。
        private async Task CheckQuoteHistoryDialogAsync()
        {
            var quoteRow = model.MarketCenterPage.RateAll.Rows.Single(row => row.QuoteId == quoteId);
            var rowControl = FindVerificationRow(quoteRow);
            Require(rowControl != null && rowControl.IsVisible, "行情中的本例报价行不可见");
            var command = model.MarketCenterPage.RateAll.QuoteLogCommand;
            Require(command.CanExecute(quoteRow), "报价日志命令不可用");
            await BeforeUiActionAsync(rowControl, "打开本例报价日志");
            // ShowDialog 使用嵌套消息循环，必须排入 Dispatcher，不能同步阻塞测试步骤。
            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() => command.Execute(quoteRow)));
            await Until(() => Application.Current.Windows.OfType<QuoteLogWindow>().Any(window => window.IsVisible),
                "报价日志弹窗打开");
            var historyWindow = Application.Current.Windows.OfType<QuoteLogWindow>().Single(window => window.IsVisible);
            try
            {
                var historyModel = (QuoteLogViewModel)historyWindow.DataContext;
                await Until(() => !historyModel.IsLoading && (historyModel.TotalCount > 0 || !string.IsNullOrEmpty(historyModel.ErrorMessage)),
                    "报价日志加载完成");
                Require(string.IsNullOrEmpty(historyModel.ErrorMessage), historyModel.ErrorMessage);
                var expected = JObject.Parse(File.ReadAllText(Path.Combine(reportDirectory, "history.json")));
                int expectedCount = (int)expected["totalCount"];
                Require(expectedCount >= 2 && expectedCount <= QuoteLogViewModel.PageSize, "本用例要求 2 至 20 条历史记录");
                var grid = (DataGrid)historyWindow.FindName("LogGrid");
                var pager = Descendants<PaginationBar>(historyWindow).Single();
                var previous = (Button)pager.FindName("PrevButton");
                var next = (Button)pager.FindName("NextButton");
                await VerifyAsync(grid, "报价日志 · 记录数及最新版本与服务端一致", () =>
                {
                    Require(grid.Items.Count == expectedCount && historyModel.TotalCount == expectedCount, "日志记录数与服务端不一致");
                    Require(Convert.ToString(historyModel.Records[0].Revision) == (string)expected["records"][0]["revision"], "日志最新版本不一致");
                });
                await VerifyAsync(pager, "单页边界 · 第 1/1 页，上一页及下一页禁用", () =>
                {
                    Require(pager.CurrentPage == 1 && pager.TotalPages == 1 && pager.TotalCount == expectedCount, "分页摘要错误");
                    Require(!previous.IsEnabled && !next.IsEnabled, "单页的翻页按钮应禁用");
                });
                var currentUser = STC.Common.Utils.LocalEnv.UserInfo;
                var operatorColumn = grid.Columns.OfType<DataGridTextColumn>()
                    .Single(column => Convert.ToString(column.Header) == "操作人");
                grid.UpdateLayout();
                var operatorCell = operatorColumn.GetCellContent(historyModel.Records[0]) as TextBlock;
                Require(operatorCell != null && operatorCell.IsVisible, "操作人单元格未渲染");
                await VerifyAsync(operatorCell, "操作人 · 显示本例操作账号的登录名 " + currentUser.UserName, () =>
                {
                    Require(((Binding)operatorColumn.Binding).Path.Path == "OperatorUserName", "操作人列仍绑定主键");
                    Require(operatorCell.Text == currentUser.UserName, "操作人单元格未显示登录名");
                    Require(historyModel.Records.All(record => record.OperatorUserId == currentUser.UserId), "本例历史操作人主键不符");
                    Require(historyModel.Records.All(record => record.OperatorUserName == currentUser.UserName), "操作人未显示登录名");
                });
                var oldRecords = historyModel.Records;
                var refresh = Descendants<Button>(historyWindow).Single(button => button.Command == historyModel.RefreshCommand);
                await ClickAsync(refresh);
                await Until(() => !historyModel.IsLoading && !ReferenceEquals(oldRecords, historyModel.Records), "刷新重新加载历史记录");
                await VerifyAsync(grid, "刷新后仍为第一页，记录完整且没有错误", () =>
                {
                    Require(string.IsNullOrEmpty(historyModel.ErrorMessage), historyModel.ErrorMessage);
                    Require(historyModel.Page == 1 && grid.Items.Count == expectedCount, "刷新后页码或记录错误");
                });
                File.WriteAllText(Path.Combine(reportDirectory, "history-dialog.json"), JObject.FromObject(new
                {
                    quoteId, totalCount = historyModel.TotalCount, pageNumber = pager.CurrentPage,
                    totalPages = pager.TotalPages, previousEnabled = previous.IsEnabled,
                    nextEnabled = next.IsEnabled, operatorUserName = historyModel.Records[0].OperatorUserName, refreshReloaded = !ReferenceEquals(oldRecords, historyModel.Records)
                }).ToString());
                var close = Descendants<Button>(historyWindow).Single(button => button.Command == null && !(button.Parent is StackPanel));
                await ClickAsync(close);
                await Until(() => !historyWindow.IsVisible, "报价日志已关闭");
            }
            finally
            {
                if (historyWindow.IsVisible) historyWindow.Close();
            }
        }

        private async Task ExecuteRegressionAsync(TestStep step)
        {
            switch (step.Action)
            {
                case "notification.restore-delete":
                    await CheckNotificationRestoreDeleteAsync(Parameter(step, "securityId"));
                    break;
                case "notification.read-survives-removal":
                    await CheckNotificationReadSurvivesRemovalAsync(Parameter(step, "securityId"));
                    break;
                case "realtime.notification-switch":
                    await CheckNotificationSwitchAsync(step);
                    return;
                case "notification.fields":
                    await CheckNotificationFieldsAsync(step);
                    return;
                case "notification.lifecycle":
                    await CheckNotificationLifecycleAsync(Parameter(step, "securityId"));
                    break;
                case "history.dialog":
                    await CheckQuoteHistoryDialogAsync();
                    break;
                case "filter.prepare":
                    await PrepareFilterCombinationAsync(step);
                    break;
                case "filter.combination":
                    await CheckFilterCombinationAsync(step);
                    break;
                case "page.open":
                    await CheckPageAsync(step);
                    break;
                case "fuzzy.preview":
                    await CheckFuzzyPreviewAsync();
                    break;
                case "precise.crm":
                    await CheckPreciseCrmAsync(Parameter(step, "trader"));
                    break;
                case "precise.input":
                    await CheckPreciseInputAsync();
                    break;
                case "realtime.lifecycle":
                    await CheckRealtimeLifecycleAsync(step);
                    break;
                case "qtrade.publish":
                    await PublishQTradeAsync(step);
                    break;
                case "qtrade.confirmPublished":
                    await ConfirmPublishedQTradeAsync(step);
                    break;
                case "management.edit":
                case "management.save":
                case "management.reject":
                case "management.operate":
                    await ExecuteQuoteWorkflowAsync(step);
                    break;
                case "groups.manage":
                    await CheckGroupManagementAsync();
                    break;
                case "pending.toolbar":
                    await CheckPendingToolbarAsync();
                    break;
                case "recommendation.typography":
                    await CheckRecommendationTypographyAsync();
                    break;
                case "recommendation.assert":
                    var market = model.MarketCenterPage;
                    var target = market.RateAll.Rows.Single(row => row.QuoteId == quoteId);
                    var marketGrid = Descendants<DataGrid>(view).First(grid => grid.IsVisible && grid.Items.Contains(target));
                    await SelectItemAsync(marketGrid, target);
                    marketGrid.ScrollIntoView(target);
                    await Until(() => market.Detail.Code == target.SecurityId
                        && market.Detail.Recommendations.Any(row => row.Evidence.Any(item => item.QuoteId == quoteId)),
                        "选中行情行自动加载真实推荐证据");
                    var recommendation = market.Detail.Recommendations.Single(row => row.Evidence.Any(item => item.QuoteId == quoteId));
                    Require(!recommendation.IsMock && recommendation.QuoteCount > 0 && recommendation.Score > 0,
                        "推荐未采用真实正式报价证据");
                    Require(!string.IsNullOrWhiteSpace(recommendation.OwnerTrader), "内部归属交易员未显示");
                    var recommendationGrid = RecommendationGrid();
                    var recommendationContainer = await ShowRecommendationRowAsync(false);
                    await ClickAsync(Button(recommendationContainer, "展开"));
                    await Until(() => recommendationContainer.DetailsVisibility == Visibility.Visible, "展开真实证据");
                    market.Detail.SelectedRecSide = STC.SalesTrading.Plugin.Abstractions.Models.QuoteDirection.Ofr;
                    Require(market.Detail.RecPageItems.All(row => row.Side == STC.SalesTrading.Plugin.Abstractions.Models.QuoteDirection.Ofr), "Ofr过滤错误");
                    market.Detail.SelectedRecSide = STC.SalesTrading.Plugin.Abstractions.Models.QuoteDirection.Bid;
                    market.Detail.SelectedRecWindow = "7d";
                    await Until(() => market.Detail.Recommendations.Any(row => row.Evidence.Any(item => item.QuoteId == quoteId)), "日期切换后重新计算推荐");
                    recommendation = market.Detail.Recommendations.Single(row => row.Evidence.Any(item => item.QuoteId == quoteId));
                    recommendationContainer = await ShowRecommendationRowAsync(true);
                    if (recommendation.CanFeedback)
                    {
                        await ClickAsync(Descendants<Button>(recommendationContainer).Single(button => Convert.ToString(button.Content) == "不适用"));
                        await Until(() => !recommendation.CanFeedback, "真实反馈保存并锁定按钮");
                    }
                    string savedFeedback = recommendation.Feedback;
                    await ReloadRecommendationsAsync(market.Detail);
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
                    await SelectItemAsync(managementGrid, model.QuoteMgmtPage.Today.Rows.Single(row => row.QuoteId == quoteId));
                    Require(model.QuoteMgmtPage.Today.SelectedRow.Count == 1, "实际表格选中报价未同步到操作目标");
                    break;
                case "filter.latency":
                    await MeasureFilterLatencyAsync(step);
                    break;
                case "qtrade.negotiation":
                    await CheckQTradeNegotiationAsync();
                    break;
                case "qtrade.business-key":
                    await CheckQTradeNegotiationAsync(true);
                    break;
                case "quote.fields":
                    JObject stored = await api.GetQuoteAsync(quoteId);
                    foreach (string field in new[] { "side", "settlType" })
                    {
                        Require((string)stored[field] == Parameter(step, field), field + " 不匹配：" + stored[field]);
                    }
                    Require((decimal)stored["quantity"] == decimal.Parse(Parameter(step, "quantity"), CultureInfo.InvariantCulture), "数量不匹配");
                    File.WriteAllText(Path.Combine(reportDirectory, stepNumber.ToString("D2") + "-fields.json"), stored.ToString());
                    break;
                case "sidebar.range":
                    FrameworkElement rangePanel = await OpenSidebarAsync(Parameter(step, "page"));
                    FilterRangeRow range = Descendants<FilterRangeRow>(rangePanel)
                        .Single(row => row.IsVisible && SidebarFilter.GetField(row) == Parameter(step, "field"));
                    TextBox[] inputs = Descendants<TextBox>(range).ToArray();
                    await SetTextAsync(inputs[0], Parameter(step, "min"));
                    await SetTextAsync(inputs[1], Parameter(step, "max"));
                    await AssertSidebarResultAsync(Parameter(step, "page"), bool.Parse(Parameter(step, "visible")));
                    break;
                case "sidebar.search":
                    FrameworkElement searchPanel = await OpenSidebarAsync(Parameter(step, "page"));
                    TextBox search = Descendants<TextBox>(searchPanel).Single(input =>
                        BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path?.Path == "FilterSearchText");
                    await SetTextAsync(search, Parameter(step, "text"));
                    await AssertSidebarResultAsync(Parameter(step, "page"), bool.Parse(Parameter(step, "visible")));
                    break;
                case "sidebar.reset":
                    FrameworkElement resetPanel = await OpenSidebarAsync(Parameter(step, "page"));
                    await ClickAsync(Button(resetPanel, "清除过滤"));
                    await AssertSidebarResultAsync(Parameter(step, "page"), true);
                    Require(Descendants<FilterRangeRow>(resetPanel).Where(row => row.IsVisible)
                        .All(row => string.IsNullOrEmpty(row.MinText) && string.IsNullOrEmpty(row.MaxText)), "清除过滤后区间控件仍有值");
                    break;
                case "parse.row":
                    var row = model.QuotePage.Precise.Rows[int.Parse(Parameter(step, "row")) - 1];
                    await VerifyCellAsync(row, "代码", Parameter(step, "securityId"), () => row.Code);
                    await VerifyCellAsync(row, "方向", Parameter(step, "side"), () => row.Direction);
                    await VerifyCellAsync(row, "报价", decimal.Parse(Parameter(step, "price"), CultureInfo.InvariantCulture),
                        () => decimal.Parse(row.Price, CultureInfo.InvariantCulture));
                    await VerifyCellAsync(row, "报量(万)", decimal.Parse(Parameter(step, "quantity"), CultureInfo.InvariantCulture),
                        () => decimal.Parse(row.Volume, CultureInfo.InvariantCulture));
                    break;
                case "parse.count":
                    var countPage = Descendants<PreciseQuoteView>(view).Single(control => control.IsVisible);
                    var totalCount = Descendants<TextBlock>(countPage).Single(text => text.IsVisible
                        && text.DataContext is STC.SalesTrading.Plugin.Shared.ViewModel.StatusBarSection
                        && text.Text.StartsWith("共 "));
                    await VerifyAsync(totalCount, "预览总条数 · 预期 " + Parameter(step, "rows") + "，实际 " + model.QuotePage.Precise.Rows.Count,
                        () => Require(model.QuotePage.Precise.Rows.Count == int.Parse(Parameter(step, "rows"))
                            && totalCount.Text == "共 " + Parameter(step, "rows") + " 条", "剩余预览条数不匹配"));
                    var selectionCount = Descendants<TextBlock>(Descendants<PreciseQuoteView>(view).Single(control => control.IsVisible))
                        .Single(text => text.Inlines.OfType<System.Windows.Documents.Run>().Any(run =>
                            BindingOperations.GetBinding(run, System.Windows.Documents.Run.TextProperty)?.Path?.Path == "SelectedCount"));
                    await VerifyAsync(selectionCount, "已选条数 · 预期 " + Parameter(step, "selected") + "，实际 " + model.QuotePage.Precise.SelectedCount,
                        () => Require(model.QuotePage.Precise.SelectedCount == int.Parse(Parameter(step, "selected")), "预览选中数量不匹配"));
                    await VerifyAsync(null, "模型校验 · 未确认预览的 revision 必须为 0",
                        () => Require(model.QuotePage.Precise.Rows.All(preview => string.IsNullOrEmpty(preview.Row.Revision)), "未确认预览不应已持久化"));
                    break;
                case "parse.toolbar":
                    var toolbarView = Descendants<PreciseQuoteView>(view).Single(control => control.IsVisible);
                    string binding = Parameter(step, "control");
                    Require(new[] { "ToolbarPriceType", "ToolbarSettlement", "AmountInput", "BpInput" }.Contains(binding), "不支持的工具栏字段");
                    if (binding.StartsWith("Toolbar"))
                    {
                        ComboBox choice = Descendants<ComboBox>(toolbarView).Single(control =>
                            BindingOperations.GetBinding(control, ComboBox.SelectedItemProperty)?.Path?.Path == binding);
                        await SelectItemAsync(choice, Parameter(step, "value"));
                    }
                    else
                    {
                        TextBox input = Descendants<TextBox>(toolbarView).Single(control =>
                            BindingOperations.GetBinding(control, TextBox.TextProperty)?.Path?.Path == binding);
                        await SetTextAsync(input, Parameter(step, "value"));
                    }
                    break;
                case "parse.warning":
                    var warning = Descendants<TextBlock>(view).FirstOrDefault(text => text.IsVisible
                        && text.Text.Contains(Parameter(step, "message")));
                    Require(warning != null, "验证目标缺失：识别警告 " + Parameter(step, "message"));
                    await VerifyAsync(warning, "识别警告 · " + Parameter(step, "message"), () =>
                        Require(warning.Text.Contains(Parameter(step, "message")), "识别警告未显示在界面"));
                    break;
                case "parse.settlement":
                    var datedRow = model.QuotePage.Precise.Rows[int.Parse(Parameter(step, "row")) - 1];
                    await VerifyCellAsync(datedRow, "清算速度", Parameter(step, "settlement"), () => datedRow.Settlement);
                    await VerifyCellAsync(datedRow, "交易日期", (DateTime?)DateTime.Today.AddDays(int.Parse(Parameter(step, "dayOffset"))),
                        () => datedRow.Row.TradeDate?.Date);
                    await VerifyCellAsync(datedRow, "价格类型", Parameter(step, "priceType"), () => datedRow.PriceType);
                    break;
                case "parse.click":
                    var precise = Descendants<PreciseQuoteView>(view).Single(control => control.IsVisible);
                    await ClickAsync(Button(precise, Parameter(step, "button")));
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

        private async Task SetTextAsync(TextBox input, string value)
        {
            await BeforeUiActionAsync(input, "输入文本");
            SetTextImmediately(input, value);
        }

        private static void SetTextImmediately(TextBox input, string value)
        {
            input.BringIntoView();
            Require(input.IsVisible && input.IsEnabled, "输入框不可操作");
            input.Focus();
            highlight?.Show(input);
            // 保留 Text 的双向绑定，与用户键入保持一致。直接赋值会移除绑定。
            input.SetCurrentValue(TextBox.TextProperty, value);
        }

        private async Task<DataGridRow> ShowRecommendationRowAsync(bool feedbackColumn)
        {
            var detail = model.MarketCenterPage.Detail;
            var grid = RecommendationGrid();
            var panel = Descendants<BondDetailPanel>(view).Single(control => control.IsVisible);
            // 历史证据增加后目标可能不在首页；按用户可见分页导航，不关闭虚拟化或改写分页模型。
            while (detail.CanPreviousRecPage)
            {
                await ClickAsync(Descendants<Button>(panel).Single(button => Convert.ToString(button.Content) == "上一页"));
            }
            int pageBudget = detail.RecTotalPages;
            while (!detail.RecPageItems.Any(row => row.Evidence.Any(item => item.QuoteId == quoteId)))
            {
                Require(--pageBudget > 0 && detail.CanNextRecPage, "推荐分页中没有本例报价证据");
                await ClickAsync(Descendants<Button>(panel).Single(button => Convert.ToString(button.Content) == "下一页"));
            }
            var recommendation = detail.RecPageItems.Single(row => row.Evidence.Any(item => item.QuoteId == quoteId));
            grid.BringIntoView();
            grid.ScrollIntoView(recommendation, feedbackColumn ? grid.Columns.Last() : grid.Columns.First());
            DataGridRow container = null;
            await Until(() =>
            {
                grid.UpdateLayout();
                container = grid.ItemContainerGenerator.ContainerFromItem(recommendation) as DataGridRow;
                return container != null && container.IsVisible;
            }, "推荐目标行完成渲染");
            highlight?.Show(container);
            return container;
        }

        private async Task<FrameworkElement> OpenSidebarAsync(string page)
        {
            Require(page == "mgmt" || page == "market", "侧栏 page 必须为 mgmt/market");
            await CloseEditorAsync();
            await NavigateAsync(page);
            if (page == "mgmt")
            {
                return Descendants<MgmtFilterPanel>(view).Single(panel => panel.IsVisible);
            }
            object tab = model.MarketCenterPage.SubTabs.Single(item => item.Key == "rateAll");
            await SelectItemAsync(Descendants<ListBox>(view).First(list => list.Items.Contains(tab)), tab);
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

        private async Task<string> WatchTriggerAsync(string id, bool present = true, string previousKey = null)
        {
            string key = null;
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < deadline)
            {
                ThrowIfCancelled();
                key = (await QueryNotificationsAsync()).OfType<JObject>()
                    .SelectMany(row => (row["triggers"] as JArray ?? new JArray()).OfType<JObject>())
                    .Where(trigger => (string)trigger["type"] == "WATCHLIST"
                        && ((JArray)trigger["quoteIds"]).Values<string>().Contains(id))
                    .Select(trigger => (string)trigger["alertKey"]).FirstOrDefault();
                if ((!present && key == null) || (present && key != null && key != previousKey)) return key;
                await Task.Delay(100);
            }
            throw new InvalidOperationException("等待关注触发实例超时：" + id);
        }

        private bool HasPopup(string key)
        {
            return STC.SalesTrading.Plugin.Shared.Services.AlertPopupService.Groups
                .Any(group => group.Items.Any(item => item.AlertKey == key));
        }

        private async Task CloseReminderPopupsAsync()
        {
            // 分组入队先于 ItemsControl 容器生成；不能因 Close 按钮暂未渲染就跳过。
            foreach (var group in STC.SalesTrading.Plugin.Shared.Services.AlertPopupService.Groups.ToArray())
            {
                Button close = null;
                await Until(() =>
                {
                    if (!STC.SalesTrading.Plugin.Shared.Services.AlertPopupService.Groups.Contains(group)) return true;
                    var window = Application.Current.Windows.OfType<AlertPopupWindow>().FirstOrDefault(item => item.IsVisible);
                    if (window == null) return false;
                    window.UpdateLayout();
                    close = Descendants<Button>(window).FirstOrDefault(button => button.IsVisible
                        && ReferenceEquals(button.DataContext, group) && Convert.ToString(button.Content) == "关闭");
                    return close != null;
                }, "等待当前提醒分组的关闭按钮渲染");
                if (close != null) await ClickAsync(close);
                await Until(() => !STC.SalesTrading.Plugin.Shared.Services.AlertPopupService.Groups.Contains(group),
                    "点击关闭后当前提醒分组确实移除");
            }
        }

        private async Task CreateNotificationQuoteAsync(string securityId, string price, string side = "bid")
        {
            await ExecuteAsync(new TestStep { Action = "parse", Parameters = new JObject
                { ["text"] = securityId + " " + price + " 1 " + side + " +1", ["expectedRows"] = "1" } });
            await ExecuteAsync(new TestStep { Action = "parse.select", Parameters = new JObject
                { ["row"] = "1", ["selected"] = "true", ["expectedCount"] = "1" } });
            await ExecuteAsync(new TestStep { Action = "parse.confirm", Parameters = new JObject { ["expectedCount"] = "1" } });
        }

        private async Task CheckNotificationLifecycleAsync(string securityId)
        {
            await CloseReminderPopupsAsync();
            await CreateNotificationQuoteAsync(securityId, "2.531");
            string firstQuote = quoteId;
            string firstKey = await WatchTriggerAsync(firstQuote);
            await Until(() => HasPopup(firstKey), "停留报价录入时收到第一笔新提醒");
            await CloseReminderPopupsAsync();
            await CreateNotificationQuoteAsync(securityId, "2.532");
            string secondQuote = quoteId;
            string secondKey = await WatchTriggerAsync(secondQuote);
            await Until(() => HasPopup(secondKey), "同券第二笔新提醒");
            Require(await WatchTriggerAsync(firstQuote) == firstKey, "新增同券报价错误改变第一笔触发键");
            Require(!HasPopup(firstKey), "新增第二笔报价不应重弹第一笔");
            await CloseReminderPopupsAsync();

            quoteId = firstQuote;
            await ExecuteQuoteWorkflowAsync(new TestStep { Action = "management.operate", Parameters = new JObject { ["button"] = "Refer" } });
            await WatchTriggerAsync(firstQuote, false);
            await Until(() => model.MarketCenterPage.Alert.Rows.SelectMany(row => row.Triggers ?? new STC.SalesTrading.Plugin.Abstractions.Models.AlertTrigger[0])
                .All(trigger => trigger.AlertKey != firstKey), "撤销后提醒列表移除第一笔实例");
            await VerifyAsync(null, "撤销 A：保留 B 的触发键，不对 B 重新弹窗", () => Require(!HasPopup(secondKey), "撤销 A 导致未变 B 重弹"));
            Require(await WatchTriggerAsync(secondQuote) == secondKey, "撤销 A 改变了未变 B 的触发键");

            quoteId = secondQuote;
            await ExecuteQuoteWorkflowAsync(new TestStep { Action = "management.edit", Parameters = new JObject
                { ["securityId"] = securityId, ["price"] = "2.533", ["quantity"] = "1", ["side"] = "1", ["settlType"] = "2" } });
            await ExecuteQuoteWorkflowAsync(new TestStep { Action = "management.save" });
            await CloseEditorAsync();
            string editedKey = await WatchTriggerAsync(secondQuote, true, secondKey);
            await Until(() => HasPopup(editedKey), "修改 B：新版本产生一次提醒");
            var group = STC.SalesTrading.Plugin.Shared.Services.AlertPopupService.Groups.Single(item => item.Items.Any(popup => popup.AlertKey == editedKey));
            Require(group.Items.Count(item => item.AlertKey == editedKey) == 1 && group.TotalCount == 1, "修改 B 的提醒重复弹出");
            var editedPopup = group.Items.Single(item => item.AlertKey == editedKey);
            Require(editedPopup.BidPrice == "2.5330" && editedPopup.BidVolume == "1", "弹窗未显示触发报价的新价格及数量");
            await CloseReminderPopupsAsync();
            await NavigateAsync("market");
            var tab = model.MarketCenterPage.SubTabs.Single(item => item.Key == "alert");
            await SelectItemAsync(Descendants<ListBox>(view).First(list => list.Items.Contains(tab)), tab);
            await Until(() => !model.MarketCenterPage.Alert.IsLoading, "提醒页可用");
            await BeforeUiActionAsync(Descendants<BoardAlertView>(view).Single(item => item.IsVisible), "刷新提醒列表");
            await model.MarketCenterPage.Alert.RefreshCommand.ExecuteAsync(null);
            await VerifyAsync(Descendants<BoardAlertView>(view).Single(item => item.IsVisible), "手动刷新不弹存量提醒", () => Require(!HasPopup(editedKey), "手动刷新错误弹出历史提醒"));
            File.WriteAllText(Path.Combine(reportDirectory, "notification-lifecycle.json"), JObject.FromObject(new
                { firstQuote, secondQuote, firstKey, secondKey, editedKey, unchangedQuoteRepopped = false }).ToString());
        }

        private async Task AssertNoReminderPopupAsync(string key, string message)
        {
            // 否定断言持续观察，防止在服务端推送尚未到达时提前通过。
            DateTime deadline = DateTime.UtcNow.AddSeconds(1);
            while (DateTime.UtcNow < deadline)
            {
                ThrowIfCancelled();
                Require(!HasPopup(key), message);
                await Task.Delay(50);
            }
        }

        private async Task CheckNotificationRestoreDeleteAsync(string securityId)
        {
            await CloseReminderPopupsAsync();
            await CreateNotificationQuoteAsync(securityId, "2.541");
            string originalKey = await WatchTriggerAsync(quoteId);
            await Until(() => HasPopup(originalKey), "确认报价弹出提醒");
            await CloseReminderPopupsAsync();
            await ExecuteQuoteWorkflowAsync(Step("management.operate", "button", "Refer"));
            await WatchTriggerAsync(quoteId, false);
            await AssertNoReminderPopupAsync(originalKey, "Refer 不应重弹旧版本");
            await ExecuteQuoteWorkflowAsync(Step("management.operate", "button", "Unrefer"));
            string restoredKey = await WatchTriggerAsync(quoteId, true, originalKey);
            await Until(() => HasPopup(restoredKey), "Unrefer 新版本弹出一次提醒");
            Require(STC.SalesTrading.Plugin.Shared.Services.AlertPopupService.Groups
                .SelectMany(group => group.Items).Count(item => item.AlertKey == restoredKey) == 1,
                "Unrefer 重复弹窗");
            await CloseReminderPopupsAsync();
            await ExecuteQuoteWorkflowAsync(Step("management.operate", "button", "Refer"));
            await WatchTriggerAsync(quoteId, false);
            await ExecuteQuoteWorkflowAsync(Step("management.operate", "button", "删除"));
            await WatchTriggerAsync(quoteId, false);
            await AssertNoReminderPopupAsync(restoredKey, "删除后旧提醒不能复活");
            File.WriteAllText(Path.Combine(reportDirectory, "notification-restore-delete.json"),
                JObject.FromObject(new { quoteId, originalKey, restoredKey }).ToString());
        }

        private async Task CheckNotificationReadSurvivesRemovalAsync(string securityId)
        {
            await CloseReminderPopupsAsync();
            await CreateNotificationQuoteAsync(securityId, "2.551");
            string first = quoteId;
            await WatchTriggerAsync(first);
            await CreateNotificationQuoteAsync(securityId, "2.552");
            string second = quoteId;
            string secondKey = await WatchTriggerAsync(second);
            await ReadNotificationThroughUiAsync();
            await CloseReminderPopupsAsync();
            quoteId = first;
            await ExecuteQuoteWorkflowAsync(Step("management.operate", "button", "Refer"));
            await WatchTriggerAsync(first, false);
            await Until(() => model.MarketCenterPage.Alert.Rows.Any(row => row.IsRead
                && row.Triggers.Any(trigger => trigger.AlertKey == secondKey)), "移除 A 后 B 的已读状态保留");
            JObject remaining = (await QueryNotificationsAsync()).OfType<JObject>().Single(row =>
                ((JArray)row["triggers"]).OfType<JObject>().Any(trigger => (string)trigger["alertKey"] == secondKey));
            Require(remaining["readTime"] != null && remaining["readTime"].Type != JTokenType.Null,
                "B 的已读状态没有持久化");
            await AssertNoReminderPopupAsync(secondKey, "已读 B 因删除同券成员而重弹");
            quoteId = second;
            await ExecuteQuoteWorkflowAsync(Step("management.edit", "securityId", securityId,
                "price", "2.553", "quantity", "1", "side", "1", "settlType", "2"));
            await ExecuteQuoteWorkflowAsync(Step("management.save"));
            await CloseEditorAsync();
            string changedKey = await WatchTriggerAsync(second, true, secondKey);
            await Until(() => HasPopup(changedKey), "已读报价修改后，新版本不能被旧已读状态吞掉");
            await CloseReminderPopupsAsync();
            File.WriteAllText(Path.Combine(reportDirectory, "notification-read-removal.json"),
                JObject.FromObject(new { first, second, secondKey, changedKey }).ToString());
        }

        private async Task PrepareNotificationFixtureAsync(string securityId, bool enableWatchlist = true)
        {
            Require(originalNotificationPreference == null, "每例只准备一次提醒夹具");
            originalNotificationPreference = (JObject)(await api.RequestProjectionAsync(
                SalesTradingApi.NotificationPreferenceGet, new Request()))["preference"];
            File.WriteAllText(Path.Combine(reportDirectory, "original-notification-preference.json"), originalNotificationPreference.ToString());
            if (enableWatchlist)
            {
                await api.RequestProjectionAsync(SalesTradingApi.NotificationPreferenceSave,
                    new NotificationPreferenceSaveRequest
                    {
                        ExpectedRevision = (string)originalNotificationPreference["revision"],
                        WatchlistEnabled = true,
                        FilterGroupEnabled = (bool)originalNotificationPreference["filterGroupEnabled"],
                        CounterQuoteEnabled = (bool)originalNotificationPreference["counterQuoteEnabled"],
                        MyBestEnabled = (bool)originalNotificationPreference["myBestEnabled"]
                    });
            }
            testWatchlist = (JObject)(await api.RequestProjectionAsync(SalesTradingApi.WatchlistSave,
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
            JObject data = await api.RequestProjectionAsync(SalesTradingApi.NotificationQuery,
                new NotificationQueryRequest { Limit = 100 });
            return (JArray)data["notifications"];
        }

        private async Task ReadNotificationThroughUiAsync()
        {
            Require(quoteId != null, "请先创建并确认本例报价");
            await CloseEditorAsync();
            await NavigateAsync("market");
            object tab = model.MarketCenterPage.SubTabs.Single(item => item.Key == "alert");
            await SelectItemAsync(Descendants<ListBox>(view).First(list => list.Items.Contains(tab)), tab);
            view.UpdateLayout();
            JObject notification = null;
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (notification == null)
            {
                ThrowIfCancelled();
                notification = (await QueryNotificationsAsync()).OfType<JObject>().FirstOrDefault(item =>
                    (item["triggers"] as JArray ?? new JArray()).OfType<JObject>().Any(trigger =>
                        (string)trigger["type"] == "WATCHLIST"
                        && ((JArray)trigger["quoteIds"]).Values<string>().Contains(quoteId)));
                if (notification != null)
                {
                    break;
                }
                Require(DateTime.UtcNow < deadline, "通知服务未返回本例报价的提醒");
                await Task.Delay(200);
            }
            string notificationId = (string)notification["notificationId"];
            Require(notification["readTime"] == null || notification["readTime"].Type == JTokenType.Null, "新建提醒应为未读");
            await Until(() => !model.MarketCenterPage.Alert.IsLoading
                && model.MarketCenterPage.Alert.Rows.Any(row => row.NotificationId == notificationId), "本例提醒推送到 UI");
            BoardAlertView alert = Descendants<BoardAlertView>(view).Single(control => control.IsVisible);
            DataGrid grid = (DataGrid)alert.FindName("Grid");
            // 操作间隔内推送可能替换同 ID 的行对象；按 ID 在实际选择时重新定位。
            await BeforeUiActionAsync(grid, "选择当前关注提醒");
            object target = grid.Items.Cast<object>().First(item => (string)item.GetType().GetProperty("NotificationId")?.GetValue(item) == notificationId);
            grid.ScrollIntoView(target);
            grid.SelectedItem = target;
            grid.UpdateLayout();
            HighlightRow(target);
            Require(grid.SelectedItems.Contains(target), "当前关注提醒没有选中");
            var markRead = Button(alert, "标记已读");
            await BeforeUiActionAsync(markRead, "点击标记已读");
            Require(grid.SelectedItems.Cast<object>().Any(item =>
                (string)item.GetType().GetProperty("NotificationId")?.GetValue(item) == notificationId),
                "点击已读前提醒选择因刷新丢失");
            ClickImmediately(markRead);
            await Until(() => model.MarketCenterPage.Alert.Rows.Any(row => row.NotificationId == notificationId && row.IsRead), "界面已读状态更新");
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
                    JObject deleted = await api.RequestProjectionAsync(SalesTradingApi.WatchlistDelete,
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
                    JObject current = (JObject)(await api.RequestProjectionAsync(
                        SalesTradingApi.NotificationPreferenceGet, new Request()))["preference"];
                    JObject restored = (JObject)(await api.RequestProjectionAsync(SalesTradingApi.NotificationPreferenceSave,
                        new NotificationPreferenceSaveRequest
                        {
                            ExpectedRevision = (string)current["revision"],
                            WatchlistEnabled = (bool)originalNotificationPreference["watchlistEnabled"],
                            FilterGroupEnabled = (bool)originalNotificationPreference["filterGroupEnabled"],
                            CounterQuoteEnabled = (bool)originalNotificationPreference["counterQuoteEnabled"],
                            MyBestEnabled = (bool)originalNotificationPreference["myBestEnabled"]
                        }))["preference"];
                    foreach (string field in ReminderFields)
                    {
                        Require(JToken.DeepEquals(restored[field], originalNotificationPreference[field]), "提醒偏好恢复未确认：" + field);
                    }
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
