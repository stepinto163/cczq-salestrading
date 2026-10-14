using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.Shared.Components;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private async Task CheckPendingToolbarAsync()
        {
            await CloseEditorAsync();
            await NavigateAsync("entry");
            var confirm = model.QuotePage.Confirm;
            Require(ownedQuotes.Count == 2, "工具栏用例必须创建且仅创建两条报价");
            await Until(() => ownedQuotes.All(id => confirm.Batches.SelectMany(batch => batch.Rows)
                .Any(row => row.RecognitionId == id)), "两条待确认报价到达");
            var page = Descendants<QuoteConfirmView>(view).Single(item => item.IsVisible);
            var toolbar = (Border)page.FindName("BatchToolbar");
            var selected = confirm.Batches.SelectMany(batch => batch.Rows).Single(row => row.RecognitionId == quoteId);
            var untouched = confirm.Batches.SelectMany(batch => batch.Rows).Single(row => ownedQuotes.Contains(row.RecognitionId) && row != selected);
            var original = await model.Backend.Api.GetQuoteAsync(untouched.RecognitionId);
            var checks = new JArray();
            Action<bool, string> check = (ok, message) =>
            {
                Require(ok, message);
                checks.Add(message);
                File.WriteAllText(Path.Combine(reportDirectory, "pending-toolbar-checks.json"), checks.ToString());
            };
            foreach (var row in confirm.Batches.SelectMany(batch => batch.Rows).Where(row => row.IsSelected).ToArray())
            {
                await SelectPendingRowAsync(page, row, false);
            }
            await ClickAsync(Button(toolbar, "Ofr"));
            check(selected.Direction == "Bid", "未勾选时方向按钮不修改报价");
            await SelectPendingRowAsync(page, selected, true);
            check(confirm.SelectedCount == 1, "实际行复选框同步已选数量");
            await ClickAsync(Button(toolbar, "Ofr"));
            check(selected.Direction == "Ofr" && untouched.Direction == "Bid", "Ofr仅应用勾选行");
            await ClickAsync(Button(toolbar, "Bid"));
            check(selected.Direction == "Bid", "Bid按钮有效");

            TextBox bp = ToolbarInput(toolbar, "BpInput");
            await ClickAsync(Button(toolbar, "+"));
            check(Number(selected.Price) == 2.51m, "空BP输入按加号增加1BP");
            await ClickAsync(Button(toolbar, "+"));
            check(Number(selected.Price) == 2.52m && bp.Text == "2", "重复加号仍只增加1BP并显示累计调整");
            await ClickAsync(Button(toolbar, "−"));
            check(Number(selected.Price) == 2.51m, "减号减少1BP");
            await EnterAdjustmentAsync(bp, "5");
            check(Number(selected.Price) == 2.56m, "Enter应用输入的5BP增量");
            await EnterAdjustmentAsync(bp, "-5");
            check(Number(selected.Price) == 2.51m, "Enter支持负BP调整");
            await EnterAdjustmentAsync(bp, "invalid");
            check(Number(selected.Price) == 2.51m, "无效BP不修改报价");

            var priceType = ToolbarChoice(toolbar, "ToolbarPriceType");
            foreach (string value in new[] { "到期收益率", "行权收益率", "净价", "全价" })
            {
                await SelectItemAsync(priceType, value);
                check(selected.PriceType == value, "价格类型可选：" + value);
            }
            await SetPendingPriceAsync(page, selected, "100");
            await ClickAsync(Button(toolbar, "+"));
            check(Number(selected.Price) == 101m, "全价使用价格单位调整，符合产品原型");
            await SelectItemAsync(priceType, "到期收益率");
            await SetPendingPriceAsync(page, selected, "2.5");
            var settlement = ToolbarChoice(toolbar, "ToolbarSettlement");
            foreach (string value in new[] { "T+1", "T+0", "T+2", "T+3", "远期" })
            {
                await SelectItemAsync(settlement, value);
                check(selected.Settlement == value, "清算类型可选：" + value);
            }
            // 换勾选目标后，刚才使用过的选项也必须能再次应用。
            await SelectPendingRowAsync(page, selected, false);
            await SelectPendingRowAsync(page, untouched, true);
            await SelectItemAsync(settlement, "远期");
            check(untouched.Settlement == "远期", "更换勾选行后可重复选择相同清算类型");
            await SelectItemAsync(settlement, "T+1");
            await SelectPendingRowAsync(page, untouched, false);
            await SelectPendingRowAsync(page, selected, true);
            await SelectItemAsync(settlement, "T+1");
            check(selected.Settlement == "T+1", "远期可通过默认T+1选项改回");

            TextBox amount = ToolbarInput(toolbar, "AmountInput");
            await ClickAsync(Button(toolbar, "+1k"));
            check(Number(selected.Volume) == 2000m, "空报量输入的+1k增加1000万");
            await ClickAsync(Button(toolbar, "-1k"));
            check(Number(selected.Volume) == 1000m, "-1k减少1000万");
            await EnterAdjustmentAsync(amount, "250");
            check(Number(selected.Volume) == 1250m, "报量框Enter应用250万增量");
            await EnterAdjustmentAsync(amount, "-2000");
            check(Number(selected.Volume) == 0m, "报量扣减不会变为负数");
            await ClickAsync(Button(toolbar, "+1k"));
            check(Number(selected.Volume) == 1000m, "扣减后+1k恢复1000万");

            foreach (string caption in new[] { "急", "荐", "整", "OCO", "打包" })
            {
                var toggle = Descendants<ToggleButton>(toolbar).Single(button => Convert.ToString(button.Content) == caption);
                await ClickAsync(toggle);
                check(toggle.IsChecked == true, "旗标可开启：" + caption);
            }
            check(selected.IsPacking && !selected.IsOco, "打包开启后清除OCO");
            await ClickAsync(Descendants<ToggleButton>(toolbar).Single(button => Convert.ToString(button.Content) == "OCO"));
            check(selected.IsOco && !selected.IsPacking, "OCO开启后清除打包");
            await ClickAsync(Descendants<ToggleButton>(toolbar).Single(button => Convert.ToString(button.Content) == "OCO"));
            check(!selected.IsOco, "OCO再次点击可关闭");
            await SelectPendingRowAsync(page, untouched, true);
            await ClickAsync(Descendants<ToggleButton>(toolbar).Single(button => Convert.ToString(button.Content) == "急"));
            check(selected.IsUrgent && untouched.IsUrgent, "混合旗标统一开启，不逐行反转");
            await ClickAsync(Descendants<ToggleButton>(toolbar).Single(button => Convert.ToString(button.Content) == "急"));
            check(!selected.IsUrgent && !untouched.IsUrgent, "全开旗标统一关闭");
            await SelectPendingRowAsync(page, untouched, false);
            await ClickAsync(Descendants<ToggleButton>(toolbar).Single(button => Convert.ToString(button.Content) == "急"));
            await ClickAsync(Descendants<ToggleButton>(toolbar).Single(button => Convert.ToString(button.Content) == "打包"));

            var contact = Descendants<CrmLookup>(toolbar).Single(input => input.Kind == "CONTACT");
            var institution = Descendants<CrmLookup>(toolbar).Single(input => input.Kind == "INSTITUTION");
            var choice = await SelectToolbarCrmAsync(contact, "王");
            check(selected.Row.ContactId == choice.ContactId && selected.InstitutionId == choice.InstitutionId
                && institution.Text == choice.InstitutionName, "跨机构交易员搜索选择后回填机构和两个ID");
            check(string.IsNullOrEmpty(untouched.Row.ContactId), "CRM工具栏不修改未勾选行");
            await SelectToolbarCrmAsync(contact, choice.ContactName);
            check(selected.Row.ContactId != null, "已选机构下交易员搜索可选择");
            var changedInstitution = await SelectToolbarCrmAsync(institution, "长城");
            check(selected.InstitutionId == changedInstitution.InstitutionId, "机构输入框可搜索并应用选择");
            if (changedInstitution.InstitutionId != choice.InstitutionId)
            {
                check(string.IsNullOrEmpty(selected.Row.ContactId) && string.IsNullOrEmpty(selected.Trader), "更换机构清除不相容交易员");
            }
            await SetTextAsync(institution, "");
            await SelectToolbarCrmAsync(contact, choice.ContactName);
            check(Number(untouched.Price) == 2.5m && Number(untouched.Volume) == 1000m
                && !untouched.IsRec && !untouched.IsWhole && !untouched.IsOco && !untouched.IsPacking,
                "未勾选报价保留原价格、数量和旗标");

            string expectedContact = selected.Row.ContactId;
            string expectedInstitution = selected.Row.InstitutionId;
            await ClickAsync(Button(toolbar, "确认报价"));
            await AwaitCommand(confirm.ConfirmSelectedCommand.ExecutionTask);
            var saved = await model.Backend.Api.GetQuoteAsync(selected.RecognitionId);
            check((string)saved["status"] == "ACTIVE" && (string)saved["side"] == "1"
                && (decimal)saved["quantity"] == 1000 && (string)saved["settlType"] == "2", "工具栏修改经确认写入服务端");
            check((bool)saved["urgent"] && (bool)saved["recommended"] && (bool)saved["whole"]
                && (bool)saved["packing"] && !(bool)saved["oco"], "五个旗标正确持久化");
            check((string)saved["contactId"] == expectedContact && (string)saved["institutionId"] == expectedInstitution,
                "CRM机构和交易员ID持久化");
            var unmodified = await model.Backend.Api.GetQuoteAsync(untouched.RecognitionId);
            check(JToken.DeepEquals(original, unmodified), "未勾选报价的服务端记录完整保持不变");
            await Until(() => !confirm.Batches.SelectMany(batch => batch.Rows).Any(row => row.RecognitionId == selected.RecognitionId), "已确认行退出待确认列表");
            untouched = confirm.Batches.SelectMany(batch => batch.Rows).Single(row => row.RecognitionId == untouched.RecognitionId);
            await SelectPendingRowAsync(page, untouched, true);
            await ClickAsync(Button(toolbar, "删除"));
            await AwaitCommand(confirm.DeleteSelectedCommand.ExecutionTask);
            var deleted = await model.Backend.Api.GetQuoteAsync(untouched.RecognitionId);
            check((string)deleted["status"] == "DELETED", "删除按钮仅删除勾选的测试报价");
            check(confirm.SelectedCount == 0 && !confirm.ToolbarUrgent && !confirm.ToolbarPacking,
                "移除选择后工具栏清除已选数量和旗标状态");
            Track(selected.RecognitionId);
            await VerifyManagementEditorAsync(saved);
            check(true, "产品编辑表单保留CRM身份与报价旗标，变更历史可查看");
        }

        private async Task SetPendingPriceAsync(QuoteConfirmView page, QuoteConfirmRowViewModel row, string value)
        {
            var input = Descendants<TextBox>(page).Single(control => ReferenceEquals(control.DataContext, row)
                && BindingOperations.GetBinding(control, TextBox.TextProperty)?.Path?.Path == "Price");
            await SetTextAsync(input, value);
            input.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        private static decimal Number(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

        private static TextBox ToolbarInput(DependencyObject toolbar, string name)
        {
            return Descendants<TextBox>(toolbar).Single(input => BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path?.Path == name);
        }

        private static ComboBox ToolbarChoice(DependencyObject toolbar, string name)
        {
            return Descendants<ComboBox>(toolbar).Single(input => BindingOperations.GetBinding(input, ComboBox.SelectedItemProperty)?.Path?.Path == name);
        }

        private async Task EnterAdjustmentAsync(TextBox input, string value)
        {
            input.Focus();
            await SetTextAsync(input, value);
            await BeforeUiActionAsync(input, "键盘/鼠标输入");
            input.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(input), Environment.TickCount, Key.Enter)
                { RoutedEvent = Keyboard.PreviewKeyDownEvent });
        }

        private async Task SelectPendingRowAsync(QuoteConfirmView page, QuoteConfirmRowViewModel row, bool selected)
        {
            var master = (DataGrid)page.FindName("MasterGrid");
            var batch = ((QuoteConfirmViewModel)page.DataContext).Batches.Single(item => item.Rows.Contains(row));
            master.ScrollIntoView(batch);
            master.UpdateLayout();
            batch.IsExpanded = true;
            var container = (DataGridRow)master.ItemContainerGenerator.ContainerFromItem(batch);
            container.DetailsVisibility = Visibility.Visible;
            master.UpdateLayout();
            await Until(() => Descendants<DataGrid>(container).Any(grid => grid.Items.Contains(row)), "展开组内报价表");
            var details = Descendants<DataGrid>(container).First(grid => grid.Items.Contains(row));
            details.ScrollIntoView(row, details.Columns[0]);
            details.UpdateLayout();
            await Until(() => Descendants<CheckBox>(details).Any(control => ReferenceEquals(control.DataContext, row)), "呈现报价行复选框");
            var checkbox = Descendants<CheckBox>(details).First(control => ReferenceEquals(control.DataContext, row));
            checkbox.BringIntoView();
            if (checkbox.IsChecked != selected) await ClickAsync(checkbox);
        }

        private async Task<CrmChoice> SelectToolbarCrmAsync(CrmLookup input, string text)
        {
            shell.Activate();
            input.BringIntoView();
            input.Focus();
            Keyboard.Focus(input);
            await Task.Delay(100);
            await SetTextAsync(input, text);
            input.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            // 已选文字未变化时，按下箭头重新打开候选（与控件的用户操作一致）。
            await BeforeUiActionAsync(input, "键盘/鼠标输入");
            input.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(input), Environment.TickCount, Key.Down)
                { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            var suggestions = (ListBox)typeof(CrmLookup).GetField("suggestions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(input);
            try
            {
                await Until(() => suggestions.Items.Count > 0, "CRM下拉返回：" + text);
            }
            catch
            {
                var status = (TextBlock)typeof(CrmLookup).GetField("status", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(input);
                File.WriteAllText(Path.Combine(reportDirectory, "crm-search-diagnostic.txt"),
                    "focus=" + input.IsKeyboardFocusWithin + "; text=" + input.Text + "; status=" + status.Text);
                throw;
            }
            var selected = (CrmChoice)suggestions.Items[0];
            Require(!string.IsNullOrWhiteSpace(selected.InstitutionName) && selected.DisplayName.Contains(selected.InstitutionName), "CRM候选缺少机构信息");
            await SelectItemAsync(suggestions, selected);
            await BeforeUiActionAsync(input, "键盘/鼠标输入");
            input.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(input), Environment.TickCount, Key.Enter)
                { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            return selected;
        }
    }
}
