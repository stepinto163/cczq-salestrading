using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

using SalesTrading.UiTests.Contracts;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;
using STC.SalesTrading.Plugin.Shared.Components;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        // Test fixture enters at the intake API through the real GW. No external FIX session
        // or QTrade publication is simulated as a successful delivery. Confirmations use UI controls.
        private async Task CheckQTradeNegotiationAsync(bool mergeDifferentConversations = false)
        {
            Require(quoteId != null, "先通过精确报价确认本例归属报价");
            string token = Environment.GetEnvironmentVariable("STC_UI_INTAKE_TOKEN");
            Require(!string.IsNullOrEmpty(token), "设置本机测试环境 STC_UI_INTAKE_TOKEN");
            JObject manual = await api.GetQuoteAsync(quoteId);
            string owner = (string)manual["ownerUserId"];
            string security = (string)manual["securityId"];
            string intention = "UI-INTENTION-" + Guid.NewGuid().ToString("N");
            string account = "UI-QT-" + owner;
            await CloseEditorAsync();

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
                            Currency = "CNY", SettlType = conversation == "A-B" && !mergeDifferentConversations ? "1" : "2",
                            TradeDate = DateTime.Today.ToString("yyyy-MM-dd")
                        }
                    }
                };
                QuoteWriteData result = await api.RequestAsync(SalesTradingApi.QuoteCreate, request);
                TestApiClient.RequireQuoteResults(result);
                string id = result.Results.Single().QuoteId;
                Track(id);
                await Until(() => model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                    .Any(row => row.QuoteId == id && decimal.Parse(row.Price, CultureInfo.InvariantCulture)
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
            await ConfirmNegotiationOnUiAsync(first, true);
            await ConfirmNegotiationOnUiAsync(second, true);
            if (mergeDifferentConversations)
            {
                await AssertRetiredNegotiationAsync(first);
            }
            else
            {
                await AssertNegotiationPriceAsync(first, 2.1234m);
            }
            await AssertNegotiationPriceAsync(second, 2.2345m);
            Capture("qt-two-confirmed-conversations");

            string replacementConversation = mergeDifferentConversations ? "A-D" : "A-C";
            string replacement = await receive(replacementConversation, "2.3456");
            Require(replacement != second, "新提案覆盖了已确认记录");
            await AssertNegotiationPriceAsync(second, 2.2345m);
            if (!mergeDifferentConversations)
            {
                await AssertNegotiationPriceAsync(first, 2.1234m);
            }
            Capture("qt-pending-keeps-confirmed-price");
            string refreshed = await receive(replacementConversation, "2.4567");
            Require(refreshed == replacement, "同会话待确认更新新建了重复提案");
            await ConfirmNegotiationOnUiAsync(replacement, true);
            await AssertNegotiationPriceAsync(replacement, 2.4567m);
            if (!mergeDifferentConversations)
            {
                await AssertNegotiationPriceAsync(first, 2.1234m);
            }
            await Until(() => !model.MarketCenterPage.RateAll.Rows.Any(row => row.QuoteId == second),
                "旧 A-C 价格退出真实行情表");
            JObject retired = await api.GetQuoteAsync(second);
            Require((string)retired["status"] == "REFERRED", "旧报价未撤出正式价池");
            Capture("qt-confirmed-replacement");
            Emit("QT 业务键替换", "PASSED", mergeDifferentConversations
                ? "不同 NegotiationID、相同完整业务键：仅确认后替换旧价"
                : "同业务键新提案仅确认后替换旧价；不同清算速度报价保持不变");
        }

        private async Task AssertRetiredNegotiationAsync(string id)
        {
            await NavigateAsync("market");
            await Until(() => !model.MarketCenterPage.RateAll.Rows.Any(row => row.QuoteId == id),
                "被替换报价退出行情 " + id);
            JObject retired = await api.GetQuoteAsync(id);
            Require((string)retired["status"] == "REFERRED", "被替换报价应保留为 REFERRED");
        }

        private async Task ConfirmNegotiationOnUiAsync(string id, bool completeCounterparty = false)
        {
            await NavigateAsync("entry");
            await SelectTabAsync("quote-confirm");
            await Until(() => Descendants<QuoteConfirmView>(view).Any(control => control.IsVisible), "报价确认页显示");
            var rows = model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows).ToList();
            var confirmation = Descendants<QuoteConfirmView>(view).Single(control => control.IsVisible);
            foreach (var row in rows.Where(row => row.IsSelected || row.QuoteId == id))
            {
                await SelectPendingRowAsync(confirmation, row, row.QuoteId == id);
            }
            if (completeCounterparty)
            {
                var toolbar = (Border)confirmation.FindName("BatchToolbar");
                var institution = Descendants<LazySearchComboBox>(toolbar).Single(input => input.SearchCommand == STC.SalesTrading.Plugin.Shared.Services.CrmLazySearch.Institution);
                var contact = Descendants<LazySearchComboBox>(toolbar).Single(input => input.SearchCommand == STC.SalesTrading.Plugin.Shared.Services.CrmLazySearch.Contact);
                await SetTextAsync(institution, "");
                var selected = await SelectToolbarCrmAsync(contact, "李继存");
                Require(selected.ContactName == "李继存" && !string.IsNullOrEmpty(selected.InstitutionId),
                    "测试联系人必须有已匹配的机构");
                JObject incoming = await api.GetQuoteAsync(id);
                // 不能为了验证去重而撤掉测试开始前已有的业务报价。
                QuoteQueryData existing = await api.RequestAsync(SalesTradingApi.QuoteQuery,
                    new QuoteQueryRequest
                    {
                        Limit = 100,
                        OwnerScope = "MINE",
                        Status = new System.Collections.Generic.List<string> { "ACTIVE" },
                        Filter = new Filter { Conditions = new System.Collections.Generic.List<FilterCondition>
                        {
                            new FilterCondition { Field = "securityId", Operator = "EQ", Value = (string)incoming["securityId"] },
                            new FilterCondition { Field = "side", Operator = "EQ", Value = (string)incoming["side"] },
                            new FilterCondition { Field = "settlType", Operator = "EQ", Value = (string)incoming["settlType"] }
                        } }
                    });
                Require(existing.TotalCount <= 100 && existing.Quotes
                    .Where(item => item.InstitutionId == selected.InstitutionId && item.ContactId == selected.ContactId)
                    .All(item => ownedQuotes.Contains(item.QuoteId)),
                    "业务键已有非本例的有效报价；请换测试债券，不能覆盖业务数据");
            }
            await ClickAsync(Button(confirmation, "确认报价"));
            await AwaitCommand(model.QuotePage.Confirm.ConfirmSelectedCommand.ExecutionTask);
            JObject stored = await api.GetQuoteAsync(id);
            Require((string)stored["status"] == "ACTIVE", "界面确认未生效：" + id);
            await Until(() => !model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                .Any(row => row.QuoteId == id), "已确认提案退出报价确认");
            Emit("UI 确认", "PASSED", id);
        }

        private async Task AssertNegotiationPriceAsync(string id, decimal price)
        {
            await NavigateAsync("market");
            object rateTab = model.MarketCenterPage.SubTabs.Single(tab => tab.Key == "rateAll");
            await SelectItemAsync(Descendants<ListBox>(view).First(list => list.Items.Contains(rateTab)), rateTab);
            view.UpdateLayout();
            await Until(() => model.MarketCenterPage.RateAll.Rows.Any(row => row.QuoteId == id && row.Price == price),
                "行情推送显示已确认价格 " + price);
            Require(Descendants<DataGrid>(view).Any(grid => grid.IsVisible && grid.Items.Cast<object>()
                .Any(row => (string)row.GetType().GetProperty("QuoteId")?.GetValue(row) == id)),
                "真实行情 DataGrid 未显示报价");
        }
    }
}
