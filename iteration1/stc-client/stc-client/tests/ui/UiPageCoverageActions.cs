using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private async Task CheckPageAsync(TestStep step)
        {
            string page = Parameter(step, "page");
            string tabKey = Parameter(step, "tab");
            await NavigateAsync(page);
            object tab;
            object target;
            if (page == "entry")
            {
                var entryTab = model.QuotePage.SubTabs.Single(item => item.Key == tabKey);
                tab = entryTab;
                target = entryTab.Target;
            }
            else if (page == "market")
            {
                var marketTab = model.MarketCenterPage.SubTabs.Single(item => item.Key == tabKey);
                tab = marketTab;
                target = marketTab.Target;
            }
            else
            {
                Require(page == "mgmt" && new[] { "today", "history" }.Contains(tabKey), "未知管理页签");
                target = tabKey == "today" ? (object)model.QuoteMgmtPage.Today : model.QuoteMgmtPage.History;
                tab = model.QuoteMgmtPage.SubTabs.Single(item => ReferenceEquals(item.Target, target));
            }
            await SelectItemAsync(Descendants<ListBox>(view).Single(list => list.IsVisible && list.Items.Contains(tab)), tab);
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            view.UpdateLayout();
            UserControl pageView = null;
            await Until(() => (pageView = Descendants<UserControl>(view).FirstOrDefault(control => control.IsVisible
                && control.GetType().Name == Parameter(step, "view") && ReferenceEquals(control.DataContext, target))) != null,
                "页签呈现 " + page + "/" + tabKey);
            var loadingProperty = target.GetType().GetProperty("IsLoading");
            highlight?.Show(Descendants<DataGrid>(pageView).FirstOrDefault(grid => grid.IsVisible) ?? (FrameworkElement)pageView);
            if (loadingProperty != null)
            {
                await Until(() => !(bool)loadingProperty.GetValue(target), "页面查询结束 " + tabKey);
            }
            string loadState = Convert.ToString(target.GetType().GetProperty("LoadState")?.GetValue(target));
            string error = Convert.ToString(target.GetType().GetProperty("ErrorMessage")?.GetValue(target));
            Require(loadState != "Error" && loadState != "Stale", "页面未就绪 " + tabKey + ": " + loadState + " " + error);
            Require(string.IsNullOrWhiteSpace(error), "页面查询失败 " + tabKey + ": " + error);
            Require(Descendants<DataGrid>(pageView).Any(grid => grid.IsVisible), "页面没有可见数据表 " + tabKey);
            File.WriteAllText(Path.Combine(reportDirectory, stepNumber.ToString("D2") + "-page.json"), new JObject
            {
                ["page"] = page, ["tab"] = tabKey, ["view"] = pageView.GetType().Name,
                ["loadState"] = loadState, ["error"] = error,
                ["scope"] = "页面呈现和查询状态；不代表全部业务功能通过"
            }.ToString());
        }

        private async Task CheckFuzzyPreviewAsync()
        {
            await NavigateAsync("entry");
            await SelectTabAsync("quote-fuzzy");
            await Until(() => Descendants<FuzzyQuoteView>(view).Any(control => control.IsVisible), "模糊报价页面");
            var fuzzyView = Descendants<FuzzyQuoteView>(view).Single(control => control.IsVisible);
            var fuzzy = model.QuotePage.Fuzzy;
            TextBox input = BoundTextBox(fuzzyView, "InputText");
            CheckBox keep = Descendants<CheckBox>(fuzzyView).Single(control =>
                BindingOperations.GetBinding(control, CheckBox.IsCheckedProperty)?.Path?.Path == "KeepTextAfterParse");
            if (keep.IsChecked != true) await ClickAsync(keep);
            const string original = "收 城投 AAA 3到5Y\n出 银行二级 5Y";
            await SetTextAsync(input, original);
            await ClickAsync(Button(fuzzyView, "识别报价"));
            Require(fuzzy.Rows.Count == 2 && fuzzy.Rows[0].Content == original.Split('\n')[0], "模糊原文拆行错误");
            Require(input.Text == original && fuzzy.SelectedCount == 0, "保留原文或默认未勾选错误");
            var first = fuzzy.Rows[0];
            var second = fuzzy.Rows[1];
            var grid = Descendants<DataGrid>(fuzzyView).Single(control => control.Items.Contains(first));
            grid.ScrollIntoView(first);
            grid.UpdateLayout();
            CheckBox rowCheck = Descendants<CheckBox>(grid).Single(control => ReferenceEquals(control.DataContext, first));
            await ClickAsync(rowCheck);
            try
            {
                await Until(() => first.IsSelected && fuzzy.SelectedCount == 1, "模糊报价勾选后已选计数更新");
            }
            finally
            {
                File.WriteAllText(Path.Combine(reportDirectory, "fuzzy-selection.json"), new JObject
                {
                    ["assembly"] = fuzzy.GetType().Assembly.Location,
                    ["hasSelectionObserver"] = fuzzy.GetType().GetField("_observedRows", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) != null,
                    ["rowSelected"] = first.IsSelected, ["checked"] = rowCheck.IsChecked,
                    ["selectedCount"] = fuzzy.SelectedCount,
                    ["bindingStatus"] = Convert.ToString(rowCheck.GetBindingExpression(CheckBox.IsCheckedProperty)?.Status)
                }.ToString());
            }
            await SetTextAsync(BoundTextBox(fuzzyView, "BatchInstitution"), "UI-预览机构");
            // 工具栏约定失焦提交；通过焦点移动触发正式绑定，不直接写ViewModel。
            input.Focus();
            await Until(() => first.Institution == "UI-预览机构", "批量机构失焦后应用");
            Require(first.Institution == "UI-预览机构" && second.Institution == "", "批量机构应只修改勾选行");
            await ClickAsync(Button(fuzzyView, "删除"));
            Require(fuzzy.Rows.Count == 1 && ReferenceEquals(fuzzy.Rows[0], second) && fuzzy.SelectedCount == 0,
                "删除范围或选择计数错误");
            Require(fuzzy.ActiveRow == null || fuzzy.Rows.Contains(fuzzy.ActiveRow), "活动候选仍引用已删除的模糊报价");
            await ClickAsync(Button(fuzzyView, "清空"));
            Require(input.Text == "" && fuzzy.Rows.Count == 1, "清空原文不应删除已有预览");
            await ClickAsync(keep);
            await SetTextAsync(input, "收 AAA 城投");
            await ClickAsync(Button(fuzzyView, "识别报价"));
            Require(input.Text == "" && fuzzy.Rows.Count == 1 && !ReferenceEquals(fuzzy.Rows[0], second),
                "新识别应替换旧预览并按设置清空原文");
            // 本例只检查客户端预览，不调用尚未完整落地的模糊报价提交接口。
        }

        private async Task CheckPreciseInputAsync()
        {
            await NavigateAsync("entry");
            await SelectTabAsync("quote-precise");
            await Until(() => Descendants<PreciseQuoteView>(view).Any(control => control.IsVisible), "精确报价页面");
            var preciseView = Descendants<PreciseQuoteView>(view).Single(control => control.IsVisible);
            var input = (TextBox)preciseView.FindName("InputBox");
            await ClickAsync(Button(preciseView, "示例"));
            highlight?.Show(input);
            Require(!string.IsNullOrWhiteSpace(input.Text), "示例按钮未填入原文");
            await ClickAsync(Button(preciseView, "清空"));
            highlight?.Show(input);
            Require(input.Text == "", "清空原文失败");
            var keep = Descendants<CheckBox>(preciseView).Single(control =>
                BindingOperations.GetBinding(control, CheckBox.IsCheckedProperty)?.Path?.Path == "KeepTextAfterParse");
            if (keep.IsChecked != true) await ClickAsync(keep);
            string text = Environment.GetEnvironmentVariable("STC_UI_SECURITY") + " 2.5 1 bid +1";
            await SetTextAsync(input, text);
            await ClickAsync(Button(preciseView, "识别报价"));
            await AwaitCommand(model.QuotePage.Precise.IdentifyCommand.ExecutionTask);
            highlight?.Show(Descendants<DataGrid>(preciseView).FirstOrDefault(grid => grid.IsVisible));
            Require(model.QuotePage.Precise.Rows.Count == 1 && input.Text == text, "识别失败或保留原文失效");
            var originalRow = model.QuotePage.Precise.Rows[0];
            HighlightRow(originalRow);
            await NavigateAsync("market");
            await NavigateAsync("entry");
            await SelectTabAsync("quote-precise");
            Require(model.QuotePage.Precise.Rows.Contains(originalRow) && input.Text == text, "切页丢失未确认预览或原文");
            await ClickAsync(Button(preciseView, "清空"));
            highlight?.Show(input);
            Require(input.Text == "" && model.QuotePage.Precise.Rows.Contains(originalRow), "清空应只影响输入框");
            Require(originalRow.Row.Revision == 0, "输入检查不得持久化未确认预览");
        }

        private static TextBox BoundTextBox(DependencyObject root, string property)
        {
            return Descendants<TextBox>(root).Single(control =>
                BindingOperations.GetBinding(control, TextBox.TextProperty)?.Path?.Path == property);
        }
    }
}
