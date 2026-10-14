using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private string RealtimeRoot => Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROOT");

        private static TestStep Step(string action, params string[] values)
        {
            var result = new TestStep { Action = action, Name = action };
            for (int index = 0; index < values.Length; index += 2)
            {
                result.Parameters[values[index]] = values[index + 1];
            }
            return result;
        }

        private void Signal(string name, JObject value)
        {
            string target = Path.Combine(RealtimeRoot, name + ".json");
            string temporary = target + ".tmp";
            File.WriteAllText(temporary, value.ToString());
            File.Move(temporary, target);
        }

        private async Task<JObject> WaitSignalAsync(string name, int seconds = 60)
        {
            string target = Path.Combine(RealtimeRoot, name + ".json");
            var playback = new PlaybackControl(RealtimeRoot);
            long initialPause = playback.PausedMilliseconds;
            var elapsed = Stopwatch.StartNew();
            while (!File.Exists(target))
            {
                ThrowIfCancelled();
                string peerReport = Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROLE") == "observer"
                    ? Path.Combine(RealtimeRoot, "result.json") : Path.Combine(RealtimeRoot, "observer", "result.json");
                if (File.Exists(peerReport))
                {
                    JObject peer = JObject.Parse(File.ReadAllText(peerReport));
                    Require((string)peer["status"] == "PASSED", "另一账号执行失败，请查看独立报告：" + peerReport);
                }
                if (elapsed.ElapsedMilliseconds - (playback.PausedMilliseconds - initialPause) > seconds * 1000L)
                {
                    throw new TimeoutException("等待双账号同步：" + name);
                }
                await Task.Delay(100);
            }
            return JObject.Parse(File.ReadAllText(target));
        }

        private async Task CheckRealtimeLifecycleAsync(TestStep step)
        {
            Require(!string.IsNullOrEmpty(RealtimeRoot), "双账号用例须由高级测试管理器启动");
            if (Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROLE") == "observer")
            {
                await ObserveRealtimeAsync(step);
                return;
            }
            string security = Parameter(step, "securityId");
            Signal("input", new JObject { ["securityId"] = security, ["actor"] = STC.Common.Utils.LocalEnv.UserInfo.UserName });
            await WaitSignalAsync("observer-ready");
            // 新增只走正常精确报价页面；确认前必须没有服务端待确认记录。
            await ExecuteAsync(Step("parse", "text", security + " " + Parameter(step, "price") + " 1 bid +1", "expectedRows", "1"));
            string clientId = model.QuotePage.Precise.Rows.Single().Row.ClientQuoteId;
            await RealtimeStageAsync("preview", security, clientId);
            await ExecuteAsync(Step("parse.select", "row", "1", "selected", "true", "expectedCount", "1"));
            await ExecuteAsync(Step("parse.confirm", "expectedCount", "1"));
            await RealtimeStageAsync("active", security, clientId);
            int operationIndex = 0;
            foreach (string operation in new[] { "Refer", "Unrefer", "Refer", "删除" })
            {
                await ExecuteAsync(Step("management.operate", "button", operation));
                await RealtimeStageAsync(operation == "Refer" ? (operationIndex++ == 0 ? "referred" : "referred-again") : operation == "Unrefer" ? "restored" : "deleted", security, clientId);
            }
        }

        private async Task RealtimeStageAsync(string phase, string security, string clientId)
        {
            Signal(phase, new JObject
            {
                ["quoteId"] = quoteId, ["clientQuoteId"] = clientId, ["securityId"] = security,
                ["time"] = DateTime.Now.ToString("O"), ["phase"] = phase
            });
            // 观察方需展示验证结果；最大可配置展示间隔不占用原35秒业务等待预算。
            captions?.Suspend();
            highlight?.Clear();
            await WaitSignalAsync(phase + "-observed", 35 + OperationInterval.MaximumMilliseconds / 1000);
            Emit("双账号 " + phase, "PASSED", "观察账号已在原页面核对，未点击刷新");
        }

        private async Task ObserveRealtimeAsync(TestStep step)
        {
            JObject input = await WaitSignalAsync("input");
            string security = (string)input["securityId"];
            Require((string)input["actor"] != STC.Common.Utils.LocalEnv.UserInfo.UserName, "操作和观察账号不能相同");
            string tab = Parameter(step, "tab");
            if (tab == "alert") await PrepareNotificationFixtureAsync(security);
            await CheckPageAsync(step);
            string localText = "双账号隔离检查：未提交文本";
            TextBox previewInput = null;
            if (tab == "quote-precise" || tab == "quote-fuzzy")
            {
                var entryView = Descendants<UserControl>(view).Single(control => control.IsVisible
                    && control.GetType().Name == Parameter(step, "view"));
                previewInput = BoundTextBox(entryView, "InputText");
                await SetTextAsync(previewInput, localText);
            }
            var initialAlerts = new HashSet<string>(model.MarketCenterPage.Alert.Rows.Select(row => row.AlertKey));
            var observedAlerts = new HashSet<string>();
            Signal("observer-ready", new JObject { ["user"] = STC.Common.Utils.LocalEnv.UserInfo.UserName, ["tab"] = tab });
            foreach (string phase in new[] { "preview", "active", "referred", "restored", "referred-again", "deleted" })
            {
                JObject change = await WaitSignalAsync(phase, 90);
                string id = (string)change["quoteId"];
                string clientId = (string)change["clientQuoteId"];
                bool active = phase == "active" || phase == "restored";
                await BeginVerificationAsync("观察账号 · " + definition.Feature + " · " + RealtimePhaseLabel(phase));
                Func<bool> matches;
                if (tab == "quote-confirm" || previewInput != null || tab == "history")
                {
                    matches = () => !model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                        .Any(row => row.Row.ClientQuoteId == clientId || (id != null && row.RecognitionId == id))
                        && (previewInput == null || previewInput.Text == localText)
                        && (tab != "history" || !model.QuoteMgmtPage.History.Rows.Any(row => row.QuoteId == id));
                }
                else if (tab == "alert")
                {
                    matches = () =>
                    {
                        var found = model.MarketCenterPage.Alert.Rows.Where(row => row.Code == security && !initialAlerts.Contains(row.AlertKey)).ToList();
                        if (active)
                        {
                            foreach (var row in found) observedAlerts.Add(row.AlertKey);
                            return found.Count > 0;
                        }
                        return phase == "preview" ? found.Count == 0
                            : !model.MarketCenterPage.Alert.Rows.Any(row => observedAlerts.Contains(row.AlertKey));
                    };
                }
                else if (tab == "today")
                {
                    matches = () =>
                    {
                        var row = model.QuoteMgmtPage.Today.Rows.FirstOrDefault(item => item.QuoteId == id);
                        // 普通其他用户只可见非QT的有效报价；撤销后退回所有者私有范围。
                        return active ? row != null && row.Status.ToString() == "Active" : row == null;
                    };
                }
                else
                {
                    matches = () => VisibleRealtimeRows().Any(row => RowIncludesQuote(row, id)) == active;
                }
                if (!active && tab != "today")
                {
                    // 否定断言保留观察窗口，避免事件尚未到达时提前判定通过。
                    await Task.Delay(1000);
                }
                await Until(matches, "观察账号页面未实时更新：" + tab + "/" + phase, 20);
                view.UpdateLayout();
                Require(matches(), "渲染后页面状态不一致");
                DateTime observedAt = DateTime.Now;
                string expectation = tab == "today"
                    ? (active ? "目标报价已显示为有效报价" : "目标报价未出现在当前可见记录中")
                    : "当前页面已符合本阶段的更新或隔离规则";
                var grid = Descendants<DataGrid>(view).FirstOrDefault(control => control.IsVisible);
                FrameworkElement verificationTarget = previewInput ?? (FrameworkElement)grid ?? view;
                if (grid != null && active)
                {
                    object matchingRow = grid.Items.Cast<object>().FirstOrDefault(row => RowIncludesQuote(row, id));
                    if (matchingRow != null)
                    {
                        grid.ScrollIntoView(matchingRow);
                        grid.UpdateLayout();
                        verificationTarget = grid.ItemContainerGenerator.ContainerFromItem(matchingRow) as FrameworkElement ?? grid;
                    }
                }
                await HoldVerificationAsync(verificationTarget, "观察账号 · " + definition.Feature + " · " + RealtimePhaseLabel(phase) + " · " + expectation);
                Require(matches(), "展示验证结果期间页面状态发生异常变化");
                Capture("realtime-" + phase);
                File.WriteAllText(Path.Combine(reportDirectory, "realtime-" + phase + ".json"), new JObject
                {
                    ["tab"] = tab, ["phase"] = phase, ["quoteId"] = id,
                    ["manualRefresh"] = false, ["observedAt"] = observedAt.ToString("O"),
                    ["displayCompletedAt"] = DateTime.Now.ToString("O")
                }.ToString());
                Signal(phase + "-observed", new JObject { ["tab"] = tab, ["phase"] = phase });
                Emit("实时观察 " + phase, "PASSED", tab);
                captions?.Suspend();
                highlight?.Clear();
            }
        }

        private static string RealtimePhaseLabel(string phase)
        {
            switch (phase)
            {
                case "preview": return "仅识别，尚未确认";
                case "active": return "确认后";
                case "referred": return "Refer 后";
                case "restored": return "Unrefer 后";
                case "referred-again": return "再次 Refer 后";
                case "deleted": return "删除后";
                default: throw new ArgumentOutOfRangeException(nameof(phase));
            }
        }

        private object[] VisibleRealtimeRows()
        {
            return Descendants<DataGrid>(view).Where(grid => grid.IsVisible)
                .SelectMany(grid => grid.Items.Cast<object>()).ToArray();
        }

        private static bool RowIncludesQuote(object row, string id)
        {
            if (id == null) return false;
            if ((string)row.GetType().GetProperty("QuoteId")?.GetValue(row) == id) return true;
            var sources = row.GetType().GetProperty("SourceQuoteIds")?.GetValue(row) as string[];
            return sources != null && sources.Contains(id);
        }
    }
}
