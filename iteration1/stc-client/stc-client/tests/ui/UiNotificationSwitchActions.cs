using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using SalesTrading.UiTests.Contracts;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Shared.Components;
using STC.SalesTrading.Plugin.Shared.Services;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private static readonly string[] ReminderTypes = { "WATCHLIST", "FILTER_GROUP", "COUNTER_QUOTE", "MY_BEST" };
        private static readonly string[] ReminderLabels = { "关注债券", "筛选分组", "对价提醒", "我的最优报价" };
        private static readonly string[] ReminderFields = { "watchlistEnabled", "filterGroupEnabled", "counterQuoteEnabled", "myBestEnabled" };

        private Task<JObject> WaitReminderPeerAsync(string phase)
        {
            // 对端可能按最慢速度展示多个动作。业务卡死仍由 SuiteRunner 独立看门狗处理；
            // 此同步预算只避免把人工观看间隔误判为服务超时，暂停仍由 WaitSignalAsync 扣除。
            return WaitSignalAsync(phase, 120 + 40 * OperationInterval.MaximumMilliseconds / 1000);
        }

        private async Task CheckNotificationSwitchAsync(TestStep step)
        {
            Require(!string.IsNullOrEmpty(RealtimeRoot), "提醒开关用例需要高级双账号模式");
            string type = Parameter(step, "type");
            string security = Parameter(step, "securityId");
            Require(ReminderTypes.Contains(type), "未知提醒类型：" + type);
            if (Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROLE") != "observer")
            {
                // 操作账号通过实际精确报价输入；不向通知服务写入伪造提醒。
                await WaitReminderPeerAsync("switch-ready");
                if (type == "MY_BEST")
                {
                    await CreateNotificationQuoteAsync(security, "2.9900");
                    Signal("competitor-ready", new JObject { ["quoteId"] = quoteId });
                }
                else
                {
                    foreach (string phase in new[] { "disabled", "enabled" })
                    {
                        await WaitReminderPeerAsync(phase + "-ready");
                        await CreateNotificationQuoteAsync(security, phase == "disabled" ? "2.3100" : "2.3200",
                            type == "COUNTER_QUOTE" ? "ofr" : "bid");
                        Signal(phase + "-quote", new JObject { ["quoteId"] = quoteId });
                        captions?.Suspend();
                        highlight?.Clear();
                        await WaitReminderPeerAsync(phase + "-checked");
                    }
                }
                await WaitReminderPeerAsync("switch-complete");
                return;
            }

            await PrepareNotificationFixtureAsync(security, false);
            if (type == "FILTER_GROUP") await PrepareReminderFilterGroupAsync();
            if (type == "COUNTER_QUOTE") await CreateNotificationQuoteAsync(security, "1.1100");
            await ShowReminderPageAsync();
            foreach (string reminderType in ReminderTypes) await SetReminderSwitchAsync(reminderType, false);
            await CloseReminderPopupsAsync();
            Signal("switch-ready", new JObject());
            if (type == "MY_BEST") await WaitReminderPeerAsync("competitor-ready");

            foreach (string phase in new[] { "disabled", "enabled" })
            {
                bool enabled = phase == "enabled";
                if (enabled)
                {
                    await SetReminderSwitchAsync(type, true);
                    // 点击真实刷新按钮，重新读取服务端设置仍保留勾选。
                    await ClickAsync(Descendants<Button>(view).Single(button => button.IsVisible
                        && ReferenceEquals(button.Command, model.MarketCenterPage.Alert.RefreshCommand)));
                    await Until(() => model.MarketCenterPage.Alert.CanChangePreferences, "重新读取提醒设置");
                    Require(ReminderSwitch(type).IsChecked == true, "重读后开关未保持启用");
                    Require(!AlertPopupService.Groups.Any(group => group.AlertType == ReminderAlertType(type)), "仅重新启用不能弹出存量报价");
                }
                string changedQuote;
                if (type == "MY_BEST")
                {
                    decimal best = decimal.Parse(Parameter(step, "price"), System.Globalization.CultureInfo.InvariantCulture);
                    await CreateNotificationQuoteAsync(security, (enabled ? best : best + 0.01m).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    changedQuote = quoteId;
                    await ShowReminderPageAsync();
                }
                else
                {
                    Signal(phase + "-ready", new JObject());
                    changedQuote = (string)(await WaitReminderPeerAsync(phase + "-quote"))["quoteId"];
                }
                // 通过同一个服务器已确认的报价版本检查关闭/启用行为，避免空场景假通过。
                Require((string)(await api.GetQuoteAsync(changedQuote))["status"] == "ACTIVE", "测试报价未实际确认");
                if (enabled) await VerifyVisibleReminderAsync(type, changedQuote, security);
                else await VerifyDisabledReminderAsync(type, changedQuote);
                if (type != "MY_BEST") Signal(phase + "-checked", new JObject());
            }
            // 弹窗出现后再关闭开关，确认当前投影及该类弹窗也被撤去。
            await SetReminderSwitchAsync(type, false);
            await Until(() => !model.MarketCenterPage.Alert.Rows.Any(row => row.Types.Contains(ReminderAlertType(type)))
                && !AlertPopupService.Groups.Any(group => group.AlertType == ReminderAlertType(type)), "关闭后移除该类当前提醒及弹窗");
            await VerifyAsync(ReminderSwitch(type), "关闭开关 · 当前提醒与现存弹窗均已移除",
                () => Require(!AlertPopupService.Groups.Any(group => group.AlertType == ReminderAlertType(type)), "关闭开关后仍残留弹窗"));
            Capture("notification-switch-final");
            Signal("switch-complete", new JObject());
        }

        private async Task ShowReminderPageAsync()
        {
            await NavigateAsync("market");
            await SelectMarketTabAsync("alert");
            await Until(() => model.MarketCenterPage.Alert.CanChangePreferences && !model.MarketCenterPage.Alert.IsLoading,
                "提醒页及账户开关加载完成");
        }

        private CheckBox ReminderSwitch(string type)
        {
            string label = ReminderLabels[Array.IndexOf(ReminderTypes, type)];
            var panel = Descendants<BoardAlertView>(view).Single(item => item.IsVisible);
            return Descendants<CheckBox>(panel).Single(box => Convert.ToString(box.Content) == label);
        }

        private async Task SetReminderSwitchAsync(string type, bool enabled)
        {
            CheckBox control = ReminderSwitch(type);
            await Until(() => control.IsEnabled, "提醒开关可操作");
            if (control.IsChecked != enabled) await ClickAsync(control);
            await Until(() => model.MarketCenterPage.Alert.CanChangePreferences, "提醒开关保存完成");
            Require(string.IsNullOrEmpty(model.MarketCenterPage.Alert.PreferenceError), model.MarketCenterPage.Alert.PreferenceError);
            JObject saved = (JObject)(await api.RequestProjectionAsync(SalesTradingApi.NotificationPreferenceGet, new Request()))["preference"];
            string field = ReminderFields[Array.IndexOf(ReminderTypes, type)];
            await VerifyAsync(control, ReminderLabels[Array.IndexOf(ReminderTypes, type)] + " · " + (enabled ? "已开启" : "已关闭") + "，核对服务端保存值",
                () => Require(control.IsChecked == enabled && (bool)saved[field] == enabled, "界面开关与服务端设置不一致"));
            File.WriteAllText(Path.Combine(reportDirectory, type + "-" + enabled + "-preference.json"), saved.ToString());
        }

        private async Task VerifyDisabledReminderAsync(string type, string changedQuote)
        {
            // 等待投影完成；不仅在发布前或第一帧检查一次“没有弹窗”。
            DateTime deadline = DateTime.UtcNow.AddSeconds(3);
            do
            {
                Require(!FindReminderTriggers(await QueryNotificationsAsync(), type, changedQuote).Any(), "关闭后服务端仍生成该类提醒");
                Require(!AlertPopupService.Groups.Any(group => group.AlertType == ReminderAlertType(type)), "关闭后仍显示该类弹窗");
                await Task.Delay(100);
            } while (DateTime.UtcNow < deadline);
            await VerifyAsync(ReminderSwitch(type), "关闭状态 · 已确认匹配报价，无该类提醒或弹窗",
                () => Require(!AlertPopupService.Groups.Any(group => group.AlertType == ReminderAlertType(type)), "关闭后延迟弹窗"));
            Capture(type + "-disabled");
        }

        private static System.Collections.Generic.IEnumerable<JObject> FindReminderTriggers(JArray rows, string type, string id)
        {
            return rows.OfType<JObject>().SelectMany(row => (row["triggers"] as JArray ?? new JArray()).OfType<JObject>())
                .Where(trigger => (string)trigger["type"] == type && ((JArray)trigger["quoteIds"]).Values<string>().Contains(id));
        }

        private static AlertType ReminderAlertType(string type)
        {
            switch (type)
            {
                case "WATCHLIST": return AlertType.FavoriteBond;
                case "FILTER_GROUP": return AlertType.FilterGroup;
                case "COUNTER_QUOTE": return AlertType.PriceMatch;
                default: return AlertType.MyBestQuote;
            }
        }

        private async Task VerifyVisibleReminderAsync(string type, string changedQuote, string security)
        {
            JObject trigger = null;
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (trigger == null)
            {
                trigger = FindReminderTriggers(await QueryNotificationsAsync(), type, changedQuote).FirstOrDefault();
                Require(DateTime.UtcNow < deadline, "启用后未生成匹配提醒：" + type + "，请核查测试价格是否为最优");
                if (trigger == null) await Task.Delay(100);
            }
            string key = (string)trigger["alertKey"];
            await Until(() => HasPopup(key), "收到本笔报价的新版本弹窗");
            AlertPopupWindow popup = null;
            FrameworkElement itemControl = null;
            await Until(() =>
            {
                popup = Application.Current.Windows.OfType<AlertPopupWindow>().FirstOrDefault(window => window.IsVisible);
                if (popup == null) return false;
                popup.UpdateLayout();
                itemControl = Descendants<Border>(popup).FirstOrDefault(element => element.IsVisible
                    && element.DataContext is STC.SalesTrading.Plugin.Shared.ViewModel.AlertPopupItem item && item.AlertKey == key);
                return itemControl != null;
            }, "真实提醒窗口及本条内容已渲染");
            DesktopPresentation.PresentTarget(shell, itemControl);
            await Application.Current.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            string screenBounds = DesktopPresentation.VerifyOnScreen(popup);
            DesktopPresentation.VerifyUncovered(popup);
            Emit("提醒弹窗桌面可见性", "VERIFIED", screenBounds);
            var content = (STC.SalesTrading.Plugin.Shared.ViewModel.AlertPopupItem)itemControl.DataContext;
            await VerifyAsync(itemControl, "实际弹窗 · " + type + " · 债券 " + security + " · 触发报价 " + changedQuote,
                () => Require(content.Code == security && content.AlertKey == key && popup.IsVisible, "弹窗债券/触发版本不符"));
            JObject changed = await api.GetQuoteAsync(changedQuote);
            string side = (string)changed["side"] == "1" ? "Bid" : "Ofr";
            string priceBinding = side + "Price";
            var priceControl = Descendants<TextBlock>(itemControl).Single(text =>
                System.Windows.Data.BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path?.Path == priceBinding);
            decimal expectedPrice = (decimal)changed["price"];
            await VerifyAsync(priceControl, side + " 价格 · 预期 " + expectedPrice + "，实际 " + priceControl.Text,
                () => Require(decimal.Parse(priceControl.Text, System.Globalization.CultureInfo.InvariantCulture) == expectedPrice,
                    "弹窗未显示本次变更报价的价格"));
            string volume = side == "Bid" ? content.BidVolume : content.OfrVolume;
            string trader = side == "Bid" ? content.BidTrader : content.OfrTrader;
            Require(decimal.Parse(volume, System.Globalization.CultureInfo.InvariantCulture) == (decimal)changed["quantity"], "弹窗报量不符");
            Require(!string.IsNullOrWhiteSpace(trader), "弹窗长城交易员缺失");
            DesktopPresentation.VerifyOnScreen(popup);
            DesktopPresentation.VerifyUncovered(popup);
            Require(captions.IsVisible, "弹窗验证期间字幕未显示");
            Emit("演示字幕桌面位置", "VERIFIED", DesktopPresentation.VerifyOnScreen(captions));
            CaptureDesktop(type + "-desktop-popup");
            Capture(type + "-visible-popup", popup);
            File.WriteAllText(Path.Combine(reportDirectory, type + "-trigger.json"), trigger.ToString());
            // 65 保留真实窗口，随后通过关闭开关验证已有弹窗也被撤去；其余例验证关闭按钮。
            if (type == "WATCHLIST") return;
            await CloseReminderPopupsAsync();
            await VerifyAsync(ReminderSwitch(type), "点击关闭后，弹窗消失但提醒开关仍开启",
                () => Require(!HasPopup(key) && ReminderSwitch(type).IsChecked == true, "关闭弹窗错误改变开关或未关闭"));
        }

        private async Task PrepareReminderFilterGroupAsync()
        {
            groupTestPrefix = "UI" + Guid.NewGuid().ToString("N").Substring(0, 6);
            await NavigateAsync("market");
            await SelectMarketTabAsync("rateAll");
            await ReloadGroupsAsync();
            var panel = Descendants<RateFilterPanel>(view).Single(item => item.IsVisible);
            await OpenModalAsync(Descendants<Button>(panel).Single(button => Convert.ToString(button.ToolTip) == "管理筛选分组"));
            await Until(() => Application.Current.Windows.OfType<FilterGroupManagerWindow>().Any(), "筛选分组编辑窗口");
            var editor = Application.Current.Windows.OfType<FilterGroupManagerWindow>().Single();
            await CreateGroupThroughUiAsync(editor, groupTestPrefix);
            await ClickAsync(Button(editor, "国开"));
            await ClickAsync(Button(editor, "保存"));
            await Until(() => !editor.IsVisible, "提醒筛选组保存（不激活侧栏）");
        }
    }
}
