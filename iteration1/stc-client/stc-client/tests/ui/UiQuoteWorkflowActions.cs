using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Modules.QuoteMgmt.View;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private async Task ExecuteQuoteWorkflowAsync(TestStep step)
        {
            if (step.Action == "fixture.pending")
            {
                // 仅给待确认推送/工具栏用例准备数据，明确区分夹具与真实UI录入。
                var draft = new JObject
                {
                    ["securityId"] = Parameter(step, "securityId"), ["side"] = Parameter(step, "side"),
                    ["price"] = Parameter(step, "price"), ["quantity"] = Parameter(step, "quantity"),
                    ["settlType"] = Parameter(step, "settlType"), ["quoteType"] = "1", ["yieldType"] = "1",
                    ["quantityUnit"] = "WAN", ["currency"] = "CNY", ["tradeDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
                    ["clientQuoteId"] = "UI-FIXTURE-" + Guid.NewGuid().ToString("N"), ["remark"] = "自动化待确认夹具"
                };
                var results = FiccApiClient.RequireQuoteResults(await model.Backend.Api.CreateAsync(new[] { draft }));
                Track((string)results.Single()["quoteId"]);
                Require((string)(await model.Backend.Api.GetQuoteAsync(quoteId))["status"] == "PENDING_CONFIRM", "夹具必须待确认");
                return;
            }
            if (step.Action == "management.edit")
            {
                if (editDialog == null || !editDialog.IsVisible)
                {
                    await SelectManagementQuoteAsync();
                    var page = Descendants<MgmtTodayView>(view).Single(control => control.IsVisible);
                    _ = Application.Current.Dispatcher.BeginInvoke(new Action(() => Click(Button(page, "修改 / 历史"))));
                    await Until(() => (editDialog = Application.Current.Windows.OfType<QuoteEntryDialog>().FirstOrDefault()) != null
                        && editDialog.IsReady, "产品报价修改表单加载");
                }
                Require(((TextBlock)editDialog.FindName("BondCode")).Text == Parameter(step, "securityId"), "编辑债券不匹配");
                ((TextBox)editDialog.FindName("PriceBox")).Text = Parameter(step, "price");
                ((TextBox)editDialog.FindName("VolumeBox")).Text = Parameter(step, "quantity");
                ((RadioButton)editDialog.FindName(Parameter(step, "side") == "1" ? "DirBid" : "DirOfr")).IsChecked = true;
                ((ComboBox)editDialog.FindName("SettlementBox")).SelectedValue = Parameter(step, "settlType");
                return;
            }
            if (step.Action == "management.save" || step.Action == "management.reject")
            {
                Require(editDialog != null && editDialog.IsReady, "先打开报价管理修改表单");
                JObject before = await model.Backend.Api.GetQuoteAsync(quoteId);
                Click(Button(editDialog, "保存修改"));
                if (step.Action == "management.reject")
                {
                    await Until(() => editDialog.IsReady && editDialog.ErrorMessage.Contains(Parameter(step, "message")), "修改拒绝提示");
                    JObject after = await model.Backend.Api.GetQuoteAsync(quoteId);
                    Require(JToken.DeepEquals(before["revision"], after["revision"]), "拒绝操作修改了版本");
                }
                else
                {
                    await Until(() => !editDialog.IsVisible, "保存成功后编辑窗口关闭");
                    JObject after = await model.Backend.Api.GetQuoteAsync(quoteId);
                    Require((long)after["revision"] > (long)before["revision"], "保存未更新版本");
                    editDialog = null;
                }
                return;
            }
            await SelectManagementQuoteAsync();
            string button = Parameter(step, "button");
            Require(new[] { "Refer", "Unrefer", "删除" }.Contains(button), "不支持的报价管理操作");
            var management = model.QuoteMgmtPage.Today;
            Click(Button(Descendants<MgmtTodayView>(view).Single(control => control.IsVisible), button));
            await AwaitCommand(button == "Refer" ? management.ReferCommand.ExecutionTask
                : button == "Unrefer" ? management.UnreferCommand.ExecutionTask : management.DeleteCommand.ExecutionTask);
            string status = (string)(await model.Backend.Api.GetQuoteAsync(quoteId))["status"];
            Require(status == (button == "Refer" ? "REFERRED" : button == "Unrefer" ? "ACTIVE" : "DELETED"),
                "报价管理操作未生效：" + status);
        }

        private async Task VerifyManagementEditorAsync(JObject saved)
        {
            await ExecuteQuoteWorkflowAsync(new TestStep
            {
                Action = "management.edit", Parameters = new JObject
                {
                    ["securityId"] = (string)saved["securityId"], ["side"] = (string)saved["side"],
                    ["price"] = "2.55", ["quantity"] = (string)saved["quantity"], ["settlType"] = (string)saved["settlType"]
                }
            });
            Require(((TextBox)editDialog.FindName("InstitutionBox")).Text == (string)saved["institutionName"], "编辑表单机构未回填");
            Require(((TextBox)editDialog.FindName("TraderBox")).Text == (string)saved["contactName"], "编辑表单交易员未回填");
            ((CheckBox)editDialog.FindName("FlagOco")).IsChecked = true;
            Click(Button(editDialog, "保存修改"));
            await Until(() => editDialog.ErrorMessage.Contains("OCO与打包"), "编辑表单OCO互斥提示");
            Require(JToken.DeepEquals((await model.Backend.Api.GetQuoteAsync(quoteId))["revision"], saved["revision"]), "OCO互斥拒绝不应保存");
            ((CheckBox)editDialog.FindName("FlagOco")).IsChecked = false;
            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() => Click(Button(editDialog, "变更历史"))));
            Window history = null;
            await Until(() => (history = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window.Title == "报价变更历史")) != null, "真实报价历史窗口");
            Require(((TextBox)history.Content).Text.Contains(quoteId), "历史窗口未读取本例报价");
            history.Close();
            await ExecuteQuoteWorkflowAsync(new TestStep { Action = "management.save" });
            JObject after = await model.Backend.Api.GetQuoteAsync(quoteId);
            foreach (string field in new[] { "institutionId", "institutionName", "contactId", "contactName", "urgent", "recommended", "whole", "oco", "packing", "quantityUnit", "currency", "ownerUserId", "source" })
                Require(JToken.DeepEquals(saved[field], after[field]), "编辑错误修改字段：" + field);
            Require((decimal)after["price"] == 2.55m, "编辑价格未保存");
        }

        private async Task SelectManagementQuoteAsync()
        {
            await ExecuteRegressionAsync(new TestStep { Action = "management.select" });
            var grid = Descendants<DataGrid>(view).Single(control => control.IsVisible && control.Name == "TodayGrid");
            grid.ScrollIntoView(grid.SelectedItem);
            grid.UpdateLayout();
        }
    }
}
