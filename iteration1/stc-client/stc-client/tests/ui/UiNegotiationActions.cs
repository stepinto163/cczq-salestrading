using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        // Test fixture enters at the intake API through the real GW. No external FIX session
        // or QTrade publication is simulated as a successful delivery. Confirmations use UI controls.
        private async Task CheckQTradeNegotiationAsync()
        {
            Require(quoteId != null, "先通过手工报价创建本例所属用户的待确认夹具");
            string token = Environment.GetEnvironmentVariable("STC_UI_INTAKE_TOKEN");
            Require(!string.IsNullOrEmpty(token), "设置本机测试环境 STC_UI_INTAKE_TOKEN");
            JObject manual = await model.Backend.Api.GetQuoteAsync(quoteId);
            string owner = (string)manual["ownerUserId"];
            string security = (string)manual["securityId"];
            string intention = "UI-INTENTION-" + Guid.NewGuid().ToString("N");
            string account = "UI-QT-" + owner;
            CloseEditor();

            Func<string, string, Task<string>> receive = async (conversation, price) =>
            {
                string eventId = "UI-QT-" + Guid.NewGuid().ToString("N");
                var request = new QuoteCreateRequest
                {
                    RequestId = eventId,
                    Intake = new IntakeIdentity
                    {
                        Token = token, OwnerUserId = owner, SourceUserId = account,
                        ExternalMessageId = eventId, ExternalQuoteId = intention,
                        NegotiationId = intention + "-" + conversation,
                        SourceTime = DateTime.Now.ToString("yyyyMMdd-HH:mm:ss.fff"), TransType = "N",
                        Text = "UI negotiation test " + conversation
                    },
                    Quotes = new System.Collections.Generic.List<QuoteCreateItem>
                    {
                        new QuoteCreateItem
                        {
                            ClientQuoteId = eventId, SecurityId = security, Side = "2", QuoteType = "1",
                            YieldType = "1", Price = price, Quantity = "1", QuantityUnit = "WAN",
                            Currency = "CNY", SettlType = "2", TradeDate = DateTime.Today.ToString("yyyy-MM-dd")
                        }
                    }
                };
                QuoteWriteData result = await model.Backend.Api.RequestAsync(SalesTradingApi.QuoteCreate, request);
                FiccApiClient.RequireQuoteResults(result);
                string id = result.Results.Single().QuoteId;
                Track(id);
                await Until(() => model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                    .Any(row => row.RecognitionId == id && decimal.Parse(row.Price, CultureInfo.InvariantCulture)
                        == decimal.Parse(price, CultureInfo.InvariantCulture)),
                    "真实待确认推送及价格 " + conversation);
                Require(!model.MarketCenterPage.RateAll.Rows.Any(row => row.QuoteId == id), "未确认提案进入行情");
                Require(!model.QuoteMgmtPage.Today.Rows.Any(row => row.QuoteId == id), "未确认提案进入正式报价管理");
                Emit("QT 接入 " + conversation, "PASSED", "GW 返回成功；待确认已推送；正式价池未进入 " + id);
                return id;
            };

            string first = await receive("A-B", "2.1234");
            string second = await receive("A-C", "2.2345");
            Require(first != second, "同一意向的两个会话被错误合并");
            await ConfirmNegotiationOnUiAsync(first);
            await ConfirmNegotiationOnUiAsync(second);
            await AssertNegotiationPriceAsync(first, 2.1234m);
            await AssertNegotiationPriceAsync(second, 2.2345m);
            Capture("qt-two-confirmed-conversations");

            string replacement = await receive("A-C", "2.3456");
            Require(replacement != second, "新提案覆盖了已确认记录");
            await AssertNegotiationPriceAsync(second, 2.2345m);
            await AssertNegotiationPriceAsync(first, 2.1234m);
            Capture("qt-pending-keeps-confirmed-price");
            string refreshed = await receive("A-C", "2.4567");
            Require(refreshed == replacement, "同会话待确认更新新建了重复提案");
            await ConfirmNegotiationOnUiAsync(replacement);
            await AssertNegotiationPriceAsync(replacement, 2.4567m);
            await AssertNegotiationPriceAsync(first, 2.1234m);
            await Until(() => !model.MarketCenterPage.RateAll.Rows.Any(row => row.QuoteId == second),
                "旧 A-C 价格退出真实行情表");
            JObject retired = await model.Backend.Api.GetQuoteAsync(second);
            Require((string)retired["status"] == "REFERRED", "旧报价未撤出正式价池");
            Capture("qt-confirmed-replacement");
            Emit("QT 双边更新", "PASSED", "A-C 新提案经 UI 确认后替换旧价；A-B 价格保持不变");
        }

        private async Task ConfirmNegotiationOnUiAsync(string id)
        {
            await NavigateAsync("entry");
            var rows = model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows).ToList();
            foreach (var row in rows) row.IsSelected = row.RecognitionId == id;
            var confirmation = Descendants<QuoteConfirmView>(view).Single(control => control.IsVisible);
            Click(Button(confirmation, "确认报价"));
            await AwaitCommand(model.QuotePage.Confirm.ConfirmSelectedCommand.ExecutionTask);
            JObject stored = await model.Backend.Api.GetQuoteAsync(id);
            Require((string)stored["status"] == "ACTIVE", "界面确认未生效：" + id);
            await Until(() => !model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                .Any(row => row.RecognitionId == id), "已确认提案退出报价确认");
            Emit("UI 确认", "PASSED", id);
        }

        private async Task AssertNegotiationPriceAsync(string id, decimal price)
        {
            await NavigateAsync("market");
            object rateTab = model.MarketCenterPage.SubTabs.Single(tab => tab.Key == "rateAll");
            Descendants<ListBox>(view).First(list => list.Items.Contains(rateTab)).SelectedItem = rateTab;
            view.UpdateLayout();
            await Until(() => model.MarketCenterPage.RateAll.Rows.Any(row => row.QuoteId == id && row.Price == price),
                "行情推送显示已确认价格 " + price);
            Require(Descendants<DataGrid>(view).Any(grid => grid.IsVisible && grid.Items.Cast<object>()
                .Any(row => (string)row.GetType().GetProperty("QuoteId")?.GetValue(row) == id)),
                "真实行情 DataGrid 未显示报价");
        }
    }
}
