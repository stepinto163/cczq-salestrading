using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        // 使用精确报价和真实 CRM 下拉建立数据；预期值来自服务端报价及基础资料，
        // 断言读取最终 TextBlock.Text，不以 ViewModel 自身值作为预期值。
        private async Task CheckNotificationFieldsAsync(TestStep step)
        {
            string security = Parameter(step, "securityId");
            await PrepareNotificationFixtureAsync(security);
            await ShowReminderPageAsync();
            var created = new List<string>();
            foreach (string side in new[] { "bid", "ofr" })
            {
                await ExecuteAsync(Step("parse", "text", security + " " + side + " 2.431 1234 +1", "expectedRows", "1"));
                await CheckPreciseCrmAsync(Parameter(step, "trader"));
                var selected = model.QuotePage.Precise.Rows.Single().Row;
                var existing = await api.RequestAsync(SalesTrading.UiTests.Contracts.SalesTradingApi.QuoteQuery,
                    new SalesTrading.UiTests.Contracts.QuoteQueryRequest
                    {
                        Limit = 100,
                        OwnerScope = "MINE",
                        Status = new List<string> { "ACTIVE" },
                        Filter = new SalesTrading.UiTests.Contracts.Filter
                        {
                            Conditions = new List<SalesTrading.UiTests.Contracts.FilterCondition>
                            {
                                new SalesTrading.UiTests.Contracts.FilterCondition { Field = "securityId", Operator = "EQ", Value = security },
                                new SalesTrading.UiTests.Contracts.FilterCondition { Field = "side", Operator = "EQ", Value = side == "bid" ? "1" : "2" },
                                new SalesTrading.UiTests.Contracts.FilterCondition { Field = "settlType", Operator = "EQ", Value = "2" }
                            }
                        }
                    });
                Require(existing.TotalCount <= 100 && existing.Quotes
                    .Where(item => item.InstitutionId == selected.InstitutionId && item.ContactId == selected.ContactId)
                    .All(item => ownedQuotes.Contains(item.QuoteId)),
                    "业务键已有非本例的有效报价；请换测试债券，不能覆盖业务数据");
                await ExecuteAsync(Step("parse.select", "row", "1", "selected", "true", "expectedCount", "1"));
                await ExecuteAsync(Step("parse.confirm", "expectedCount", "1"));
                created.Add(quoteId);
                await WatchTriggerAsync(quoteId);
                await CloseReminderPopupsAsync();
                await ShowReminderPageAsync();
                await VerifyNotificationRowFieldsAsync(quoteId, side);
            }

            quoteId = created[0];
            await ExecuteQuoteWorkflowAsync(Step("management.edit", "securityId", security,
                "price", "2.432", "quantity", "2345", "side", "1", "settlType", "2"));
            await ExecuteQuoteWorkflowAsync(Step("management.save"));
            await CloseEditorAsync();
            await CloseReminderPopupsAsync();
            await ShowReminderPageAsync();
            await VerifyNotificationRowFieldsAsync(quoteId, "edited-bid");
        }

        private async Task VerifyNotificationRowFieldsAsync(string changedQuoteId, string phase)
        {
            JObject quote = await api.GetQuoteAsync(changedQuoteId);
            string security = (string)quote["securityId"];
            string side = (string)quote["side"] == "1" ? "Bid" : "Ofr";
            decimal price = (decimal)quote["price"];
            await Until(() => model.MarketCenterPage.Alert.Rows.Any(candidate => candidate.SecurityId == security
                && candidate.Triggers.Any(trigger => trigger.QuoteIds.Contains(changedQuoteId))
                && (side == "Bid" ? candidate.Bid : candidate.Ofr) == price), "提醒行显示本次变更的价格");
            AlertRow row = model.MarketCenterPage.Alert.Rows.First(candidate => candidate.SecurityId == security
                && candidate.Triggers.Any(trigger => trigger.QuoteIds.Contains(changedQuoteId))
                && (side == "Bid" ? candidate.Bid : candidate.Ofr) == price);
            var panel = Descendants<BoardAlertView>(view).Single(control => control.IsVisible);
            var grid = Descendants<DataGrid>(panel).Single(control => control.Items.Contains(row));
            var evidence = new JArray();

            await VerifyReminderCellAsync(grid, row, "代码", security, evidence);
            Require(!string.IsNullOrWhiteSpace((string)quote["institutionName"]), "测试报价缺少实际机构，不能用空值验证回填");
            Require(!string.IsNullOrWhiteSpace((string)quote["contactName"]), "测试报价缺少实际联系人");
            await VerifyReminderCellAsync(grid, row, "报价机构." + side, (string)quote["institutionName"], evidence);
            await VerifyReminderCellAsync(grid, row, "报价交易员." + side, (string)quote["contactName"], evidence);
            var owner = STC.Common.Utils.LocalEnv.UserInfo;
            Require((string)quote["ownerUserId"] == owner.UserId, "测试报价归属人必须是实际操作账号");
            string ownerName = string.IsNullOrWhiteSpace(owner.FullName) ? owner.UserName : owner.FullName;
            await VerifyReminderCellAsync(grid, row, "长城交易员." + side, ownerName, evidence);
            await VerifyReminderCellAsync(grid, row, side, price.ToString("0.0000", CultureInfo.InvariantCulture), evidence);
            decimal quantity = (decimal)quote["quantity"] * ((string)quote["quantityUnit"] == "YI" ? 10000m : 1m);
            await VerifyReminderCellAsync(grid, row, "Vol." + side, quantity.ToString(CultureInfo.InvariantCulture), evidence, true);
            DateTime time = (DateTime)(quote["quoteTime"] ?? quote["updateTime"]);
            await VerifyReminderCellAsync(grid, row, "报价时间", time.ToString("HH:mm:ss"), evidence);

            JObject notification = (await QueryNotificationsAsync()).OfType<JObject>()
                .Single(record => (string)record["notificationId"] == row.NotificationId);
            string[] types = ((JArray)notification["reasons"]).OfType<JObject>().Select(reason => (string)reason["type"]).Distinct().ToArray();
            string reasonText = string.Join("/", types.Select(type => type == "MY_BEST" ? "我的最优"
                : ReminderLabels[Array.IndexOf(ReminderTypes, type)]));
            await VerifyReminderCellAsync(grid, row, "提醒类型", reasonText, evidence);

            JObject referenceData = await api.RequestProjectionAsync(SalesTrading.UiTests.Contracts.SalesTradingApi.BaseGet,
                new SalesTrading.UiTests.Contracts.BaseGetRequest
                { Kind = "SECURITY", Ids = new List<string> { security } });
            JObject reference = ((JArray)referenceData["records"]).OfType<JObject>().Single(item => (string)item["securityId"] == security);
            foreach (var field in new[]
            {
                new[] { "简称", "securityName" }, new[] { "剩余期限", "remainingYears" },
                new[] { "债券库", "bondSubtype" }, new[] { "主体评级", "issuerRating" }, new[] { "债券评级", "bondRating" },
                new[] { "中债YTM(%)", "valuationYield" }, new[] { "中债净价", "valuationCleanPrice" },
                new[] { "中证YTM(%)", "csiValuationYield" }, new[] { "中证净价", "csiValuationCleanPrice" }
            })
            {
                string value = (string)reference[field[1]];
                bool numeric = field[1].Contains("Yield") || field[1].Contains("Price");
                await VerifyReminderCellAsync(grid, row, field[0], value ?? string.Empty, evidence, numeric);
            }
            File.WriteAllText(Path.Combine(reportDirectory, "notification-fields-" + phase + ".json"), evidence.ToString());
            Capture("notification-fields-" + phase);
        }

        private async Task VerifyReminderCellAsync(DataGrid grid, AlertRow row, string header, string expected,
            JArray evidence, bool numeric = false)
        {
            var column = grid.Columns.Single(item => Convert.ToString(item.Header) == header);
            await BeforeUiActionAsync(grid, "滚动到提醒字段 · " + header);
            grid.ScrollIntoView(row, column);
            grid.UpdateLayout();
            FrameworkElement content = column.GetCellContent(row);
            Require(content != null, "提醒字段未生成：" + header);
            TextBlock text = content as TextBlock ?? Descendants<TextBlock>(content).Single();
            await VerifyAsync(text, "提醒字段 · " + header + " · 预期 " + (expected.Length == 0 ? "空（源数据未提供）" : expected), () =>
            {
                Require(text.IsVisible, "提醒字段不可见：" + header);
                bool equal = text.Text == expected;
                if (numeric && expected.Length > 0)
                {
                    decimal actualNumber;
                    equal = decimal.TryParse(text.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out actualNumber)
                        && actualNumber == decimal.Parse(expected, CultureInfo.InvariantCulture);
                }
                Require(equal, header + "：预期 [" + expected + "]，实际 [" + text.Text + "]");
            });
            evidence.Add(new JObject { ["field"] = header, ["expected"] = expected, ["rendered"] = text.Text });
        }
    }
}
