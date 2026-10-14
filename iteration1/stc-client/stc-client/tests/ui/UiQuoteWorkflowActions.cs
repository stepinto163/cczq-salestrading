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
            if (step.Action == "management.edit")
            {
                if (editDialog == null || !editDialog.IsVisible)
                {
                    await SelectManagementQuoteAsync();
                    var page = Descendants<MgmtTodayView>(view).Single(control => control.IsVisible);
                    await OpenModalAsync(Button(page, "修改 / 历史"));
                    await Until(() => (editDialog = Application.Current.Windows.OfType<QuoteEntryDialog>().FirstOrDefault()) != null
                        && editDialog.IsReady, "产品报价修改表单加载");
                }
                Require(((TextBlock)editDialog.FindName("BondCode")).Text == Parameter(step, "securityId"), "编辑债券不匹配");
                await SetTextAsync((TextBox)editDialog.FindName("PriceBox"), Parameter(step, "price"));
                await SetTextAsync((TextBox)editDialog.FindName("VolumeBox"), Parameter(step, "quantity"));
                await ClickAsync((RadioButton)editDialog.FindName(Parameter(step, "side") == "1" ? "DirBid" : "DirOfr"));
                var settlement = (ComboBox)editDialog.FindName("SettlementBox");
                await BeforeUiActionAsync(settlement, "选择清算速度");
                settlement.SelectedValue = Parameter(step, "settlType");
                return;
            }
            if (step.Action == "management.save" || step.Action == "management.reject")
            {
                Require(editDialog != null && editDialog.IsReady, "先打开报价管理修改表单");
                JObject before = await model.Backend.Api.GetQuoteAsync(quoteId);
                await ClickAsync(Button(editDialog, "保存修改"));
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
            await ClickAsync(Button(Descendants<MgmtTodayView>(view).Single(control => control.IsVisible), button));
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
            await ClickAsync((CheckBox)editDialog.FindName("FlagOco"));
            await ClickAsync(Button(editDialog, "保存修改"));
            await Until(() => editDialog.ErrorMessage.Contains("OCO与打包"), "编辑表单OCO互斥提示");
            Require(JToken.DeepEquals((await model.Backend.Api.GetQuoteAsync(quoteId))["revision"], saved["revision"]), "OCO互斥拒绝不应保存");
            await ClickAsync((CheckBox)editDialog.FindName("FlagOco"));
            await OpenModalAsync(Button(editDialog, "变更历史"));
            Window history = null;
            await Until(() => (history = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window.Title == "报价变更历史")) != null, "真实报价历史窗口");
            Require(((TextBox)history.Content).Text.Contains(quoteId), "历史窗口未读取本例报价");
            await CloseWindowAsync(history);
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
