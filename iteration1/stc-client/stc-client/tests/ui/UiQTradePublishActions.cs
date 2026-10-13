using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;
using STC.SalesTrading.Plugin.Shared.Components;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private readonly List<string> publishedQuoteIds = new List<string>();

        // 唯一准备入口是 U41 发布 API，不调用 intake/create 注入接收报价。
        private async Task PublishQTradeAsync(TestStep step)
        {
            Require(Environment.GetEnvironmentVariable("STC_UI_QTRADE_PUBLISH") == "1",
                "本例会向外部 QTrade 发送测试意向。设置 STC_UI_QTRADE_PUBLISH=1 后单独执行。");
            string token = Environment.GetEnvironmentVariable("STC_UI_INTAKE_TOKEN");
            Require(!string.IsNullOrWhiteSpace(token), "内部发布测试需要已有 STC_UI_INTAKE_TOKEN");
            Require(string.Equals(Environment.GetEnvironmentVariable("STC_UI_USER"), Parameter(step, "receiverLogin"),
                StringComparison.OrdinalIgnoreCase), "请用用例 receiverLogin 指定的收件人账户登录；尚未发布消息");
            string appId = Parameter(step, "appId");
            Require(appId.Length == 9 && appId.All(char.IsDigit), "appId 应为 QTrade 分配的9位机构编号");
            string messageId = appId + DateTime.Now.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture)
                + Guid.NewGuid().ToString("N").Substring(0, 9);
            // 纯字母标记避免 QT NLP 将跟踪号中的数字误当作报价。
            string marker = "UITEST-" + new string(messageId.Select(character => char.IsDigit(character)
                ? (char)('A' + character - '0') : char.ToUpperInvariant(character)).ToArray());
            string mode = Parameter(step, "mode");
            string security = Parameter(step, "securityId");
            string price = Parameter(step, "price");
            if (price == "${uniquePrice}")
            {
                // QT 会复用相同券/方向/价格的既有意向，导致旧原文或不再下发。
                // 每次用四位小数的测试价格区分新输入，实际值写入回执关联的接收记录。
                int fraction = int.Parse(messageId.Substring(messageId.Length - 4), NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture) % 10000;
                price = (2m + fraction / 10000m).ToString("0.0000", CultureInfo.InvariantCulture);
            }
            int expected = int.Parse(Parameter(step, "expectedRows"), CultureInfo.InvariantCulture);
            var request = new QTradePublishRequest
            {
                MessageId = messageId,
                SenderQtAccount = Parameter(step, "senderQtAccount"),
                Token = token,
                Mode = mode,
                PostText = marker,
                Recipients = new List<QTradePublishRecipient>
                {
                    new QTradePublishRecipient
                    {
                        RoutingType = int.Parse(Parameter(step, "routingType"), CultureInfo.InvariantCulture),
                        RoutingId = Parameter(step, "routingId")
                    }
                }
            };
            if (mode == "TEXT")
            {
                request.PreText = "Bid " + security + " " + price + " 1w +1";
                if (expected == 2)
                {
                    request.PreText += "\nOfr " + security + " " + price + " 1w +1";
                }
            }
            else
            {
                Require(expected == 1, "结构化单券用例应期待1行");
                request.Quotes = new List<QTradePublishQuote>
                {
                    new QTradePublishQuote
                    {
                        ClientQuoteId = messageId, SecurityId = security.Split('.')[0], Exchange = "XCFE",
                        Symbol = Parameter(step, "symbol"), Side = "1", BidPriceType = 1,
                        BidPrice = price, BidQuantityYuan = "10000", BidSettlementType = "2",
                        BidTradeDate = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    }
                };
            }
            await NavigateAsync("entry");
            QTradePublishData result = await model.Backend.Api.RequestAsync(SalesTradingApi.QTradeQuotePublish, request);
            File.WriteAllText(Path.Combine(reportDirectory, "qtrade-publish-ack.json"), result.ToJson().ToString());
            Emit("QTrade 发布回执", result.Status, messageId);
            Require(result.MessageId == messageId && result.Status == "ACKNOWLEDGED" && result.ApplicationCode == "0",
                "QTrade 未确认全部发送成功；不可自动重发。回执：" + result.ToJson());
            await Until(() =>
            {
                var received = model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                    .Where(row => (row.TextContent ?? "").Contains(marker)).ToList();
                // 部分到达后超时，也必须清理已经收到的本例报价。
                foreach (var row in received)
                {
                    Track(row.RecognitionId);
                }
                return received.Count >= expected;
            },
                "QTrade 已受理，但未收到预期 FIX→QuoteSvr→STC 待确认行；检查 FIX 权限、inf1 及收件账号", 30);
            var rows = model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                .Where(row => (row.TextContent ?? "").Contains(marker)).ToList();
            Require(rows.Count == expected, "真实接收报价数量与预期不一致");
            var evidence = new JArray();
            foreach (var row in rows)
            {
                string id = row.RecognitionId;
                Track(id);
                publishedQuoteIds.Add(id);
                QuoteRecord quote = await model.Backend.Api.GetQuoteModelAsync(id);
                Require(quote.Status == "PENDING_CONFIRM" && quote.Source != null && quote.Source.Type == "QT", "未经过真实 QT 待确认通路");
                Require(quote.Source.SourceUserId == Parameter(step, "receiverQtAccount"), "收到的不是目标接收账号报价");
                Require(!string.IsNullOrWhiteSpace(quote.Source.NegotiationId), "接收报价缺少双边 NegotiationID");
                Require(quote.SecurityId == security && decimal.Parse(quote.Price, CultureInfo.InvariantCulture)
                    == decimal.Parse(price, CultureInfo.InvariantCulture), "债券或价格映射不正确");
                Require(decimal.Parse(quote.Quantity, CultureInfo.InvariantCulture) == 1 && quote.SettlType == "2", "数量单位或清算速度映射不正确");
                Require(!model.MarketCenterPage.RateAll.Rows.Any(item => item.QuoteId == id), "未确认报价污染行情");
                evidence.Add(quote.ToJson());
            }
            if (expected > 1)
            {
                Require(rows.Select(row => row.Row.BatchId).Distinct().Count() == 1,
                    "一条原始消息拆出的报价应同组");
            }
            File.WriteAllText(Path.Combine(reportDirectory, "qtrade-received-quotes.json"), evidence.ToString());
            var confirmationPage = Descendants<QuoteConfirmView>(view).Single(control => control.IsVisible);
            foreach (var row in rows)
            {
                await SelectPendingRowAsync(confirmationPage, row, false);
            }
            Capture("qtrade-real-pending");
        }

        private async Task ConfirmPublishedQTradeAsync(TestStep step)
        {
            Require(publishedQuoteIds.Count > 0, "本例未收到发布后的实际报价");
            await NavigateAsync("entry");
            var pendingRows = model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows).ToList();
            foreach (var row in pendingRows)
            {
                row.IsSelected = publishedQuoteIds.Contains(row.RecognitionId);
            }
            var page = Descendants<QuoteConfirmView>(view).Single(control => control.IsVisible);
            var toolbar = (Border)page.FindName("BatchToolbar");
            var institution = Descendants<CrmLookup>(toolbar).Single(input => input.Kind == "INSTITUTION");
            var contact = Descendants<CrmLookup>(toolbar).Single(input => input.Kind == "CONTACT");
            SetText(institution, "");
            string contactName = Parameter(step, "contactName");
            Require(!string.IsNullOrWhiteSpace(contactName), "请配置发送人的真实 CRM 联系人名称");
            var selected = await SelectToolbarCrmAsync(contact, contactName);
            Require(selected.ContactName == contactName && selected.InstitutionName == Parameter(step, "institutionName"),
                "CRM 候选不匹配指定发送人及机构，不能选择其他联系人代替");
            Require(pendingRows.Where(row => publishedQuoteIds.Contains(row.RecognitionId))
                .All(row => row.Row.ContactId == selected.ContactId && row.InstitutionId == selected.InstitutionId),
                "CRM 工具栏没有更新本例报价的联系人和机构 ID");
            Capture("qtrade-counterparty-selected");
            // 一次确认本批，避免逐条确认后服务端刷新覆盖其他行尚未提交的 CRM 编辑。
            Click(Button(page, "确认报价"));
            await AwaitCommand(model.QuotePage.Confirm.ConfirmSelectedCommand.ExecutionTask);
            foreach (string id in publishedQuoteIds)
            {
                QuoteRecord quote = await model.Backend.Api.GetQuoteModelAsync(id);
                Require(quote.Status == "ACTIVE", "界面确认未生效：" + id);
                await Until(() => !model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                    .Any(row => row.RecognitionId == id), "已确认报价退出待确认页面");
                await AssertNegotiationPriceAsync(id, decimal.Parse(quote.Price, CultureInfo.InvariantCulture));
                quoteId = id;
            }
            Capture("qtrade-real-confirmed-market");
        }
    }
}
