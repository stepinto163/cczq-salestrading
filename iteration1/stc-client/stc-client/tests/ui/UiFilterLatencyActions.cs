using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Shared.Components;

namespace SalesTrading.UiTests
{
    internal sealed partial class UiScenarioWorker
    {
        private readonly Dictionary<string, JObject> latencyQuotes = new Dictionary<string, JObject>();
        private readonly HashSet<string> latencyActive = new HashSet<string>();

        // 仅显式性能用例调用；500 条上限，不修改虚拟化，不改变生产查询实现。
        private async Task MeasureFilterLatencyAsync(TestStep step)
        {
            int count = int.Parse(Parameter(step, "quotes"), CultureInfo.InvariantCulture);
            int samples = int.Parse(Parameter(step, "samples"), CultureInfo.InvariantCulture);
            Require(count > 0 && count <= 500 && samples >= 2 && samples <= 20, "限制 500 报价、每种交互最多 20 次");
            FrameworkElement panel = await OpenSidebarAsync("mgmt");
            await Until(() => !model.QuoteMgmtPage.Today.IsLoading, "管理页就绪");
            int originalTotal = model.QuoteMgmtPage.Today.TotalCount;
            await CloseLatencyViewsAsync();
            string recovery = Environment.GetEnvironmentVariable("STC_UI_LATENCY_RECOVERY");
            if (!string.IsNullOrWhiteSpace(recovery)) {
                foreach (string id in JArray.Parse(File.ReadAllText(recovery)).Values<string>()) {
                    JObject quote = await model.Backend.Api.GetQuoteAsync(id);
                    Require(((string)quote["remark"] ?? "").StartsWith("FILTER-LATENCY-"), "拒绝清理非本测试报价");
                    if ((string)quote["status"] == "DELETED") continue;
                    Track(id);
                    latencyQuotes[id] = quote;
                    if ((string)quote["status"] == "ACTIVE") latencyActive.Add(id);
                }
                await CleanupLatencyQuotesAsync();
                File.WriteAllText(recovery, "[]");
                Emit("上次夹具清理", "PASSED", "恢复未完成清理的测试报价");
            }
            var broadFilter = new Filter { Conditions = new List<FilterCondition> {
                new FilterCondition { Field = "bidPrice", Operator = "BETWEEN", Value = new JArray(12.001m, 12.010m) }
            }};
            // 价格区间用于隔离本轮数据；有既存记录则停止，不能混入其他人的数据。
            QuoteQueryData baseline = await model.Backend.Api.RequestAsync(SalesTradingApi.QuoteQuery,
                new QuoteQueryRequest { Filter = broadFilter, Limit = 1, Status = new List<string> { "ACTIVE", "REFERRED" } });
            Require(baseline.TotalCount == 0, "测试价格区间已有报价，请先检查，禁止覆盖");
            string prefix = "FILTER-LATENCY-" + Guid.NewGuid().ToString("N");
            var fixtures = new List<Tuple<string, int>>();
            for (int offset = 0; offset < count; offset += 25)
            {
                ThrowIfCancelled();
                var seedTimer = Stopwatch.StartNew();
                var inputs = new List<QuoteCreateItem>();
                for (int index = offset; index < Math.Min(count, offset + 25); index++)
                {
                    inputs.Add(new QuoteCreateItem {
                        ClientQuoteId = prefix + "-" + index, SecurityId = Parameter(step, "securityId"),
                        Side = "1", QuoteType = "1", YieldType = "1",
                        Price = (12.001m + (index % 10) * 0.001m).ToString(CultureInfo.InvariantCulture),
                        Quantity = (((index / 10) % 5 + 1) * 100).ToString(CultureInfo.InvariantCulture),
                        QuantityUnit = "WAN", Currency = "CNY", SettlType = index % 2 == 0 ? "1" : "2",
                        TradeDate = DateTime.Today.ToString("yyyy-MM-dd"), Remark = prefix
                    });
                }
                QuoteWriteData created = await model.Backend.Api.CreateQuotesAsync(inputs);
                foreach (QuoteOperationResult result in created.Results.Where(item => item.Code == "0000"))
                {
                    Track(result.QuoteId);
                    latencyQuotes[result.QuoteId] = new JObject { ["quoteId"] = result.QuoteId, ["revision"] = result.Revision };
                    int index = int.Parse(result.ClientQuoteId.Substring(prefix.Length + 1), CultureInfo.InvariantCulture);
                    fixtures.Add(Tuple.Create(result.QuoteId, index));
                }
                FiccApiClient.RequireQuoteResults(created);
                await ChangeLatencyQuotesAsync("confirm", created.Results.Select(item => item.QuoteId).ToArray());
                Emit("准备过滤夹具", "RUNNING", fixtures.Count + "/" + count + "; batch=" + seedTimer.ElapsedMilliseconds + "ms");
                Require(seedTimer.Elapsed.TotalSeconds < 3, "单批准备超过3秒，停止继续加压并清理");
            }
            await Until(() => !model.QuoteMgmtPage.Today.IsLoading, "夹具推送完成");
            DataGrid grid = Descendants<DataGrid>(view).Single(item => item.IsVisible && item.Name == "TodayGrid");
            Require(grid.EnableRowVirtualization && grid.EnableColumnVirtualization
                && VirtualizingPanel.GetIsVirtualizing(grid), "必须启用行列虚拟化");
            var measurements = new JArray();
            var report = new JObject {
                ["fixtureQuotes"] = count, ["existingManagementQuotes"] = originalTotal,
                ["singleClient"] = true, ["transport"] = "STC desktop -> GW -> QuoteSvr -> WPF",
                ["renderBoundary"] = "WPF layout and next CompositionTarget.Rendering callback; not physical display presentation",
                ["virtualization"] = true, ["samples"] = measurements
            };
            string output = Path.Combine(reportDirectory, "filter-latency.json");
            var traceGate = new object();
            var traces = new List<Tuple<string, long, long, int>>();
            bool profiling = Environment.GetEnvironmentVariable("STC_UI_PROFILE") == "1";
            long traceOrigin = Stopwatch.GetTimestamp();
            var dispatcherOperations = new Dictionary<DispatcherOperation, long>();
            DispatcherHookEventHandler operationStarted = (sender, args) =>
            {
                dispatcherOperations[args.Operation] = Stopwatch.GetTimestamp();
            };
            DispatcherHookEventHandler operationCompleted = (sender, args) =>
            {
                if (dispatcherOperations.TryGetValue(args.Operation, out long started))
                {
                    dispatcherOperations.Remove(args.Operation);
                    ClientPerformanceTrace.End("dispatcher:" + args.Operation.Priority, started);
                }
            };
            if (profiling)
            {
                grid.Dispatcher.Hooks.OperationStarted += operationStarted;
                grid.Dispatcher.Hooks.OperationCompleted += operationCompleted;
                ClientPerformanceTrace.Sink = (stage, start, end, thread) =>
                {
                    lock (traceGate)
                    {
                        traces.Add(Tuple.Create(stage, start, end, thread));
                    }
                };
            }
            try
            {
                // 第一次单列，不混入预热后的分位数。相邻样本必须切换条件，禁止复用相同查询结果。
                for (int iteration = -1; iteration < samples * 2; iteration++)
                {
                    bool automatic = iteration >= samples;
                    bool even = (iteration + 1) % 2 == 0;
                    string settlement = even ? "T+0" : "T+1";
                    HashSet<string> expected = new HashSet<string>(fixtures.Where(item =>
                        (item.Item2 % 2 == 0) == even && item.Item2 % 10 >= 4 && item.Item2 % 10 <= 8
                        && (item.Item2 / 10) % 5 >= 3).Select(item => item.Item1));
                    Require(expected.Count > 0 && expected.Count <= 100, "筛选结果应在单页内，核对完整 ID 集合");
                    long sampleStart = Stopwatch.GetTimestamp();
                    var timer = new Stopwatch();
                    double queryStart = -1;
                    double rowsReady = -1;
                    var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    PropertyChangedEventHandler handler = (sender, args) => {
                        if (args.PropertyName != "LoadState") return;
                        if (model.QuoteMgmtPage.Today.IsLoading) {
                            if (queryStart < 0) queryStart = timer.Elapsed.TotalMilliseconds;
                            return;
                        }
                        // QueryId 是订阅主题，切换筛选时可以不变；以本次 Loading 边界及完整结果集为准。
                        if (queryStart < 0) return;
                        if (!string.IsNullOrEmpty(model.QuoteMgmtPage.Today.ErrorMessage)) {
                            completion.TrySetException(new InvalidOperationException(model.QuoteMgmtPage.Today.ErrorMessage));
                            return;
                        }
                        if (expected.SetEquals(model.QuoteMgmtPage.Today.Rows.Select(row => row.QuoteId))
                            && model.QuoteMgmtPage.Today.TotalCount == expected.Count) {
                            rowsReady = timer.Elapsed.TotalMilliseconds;
                            completion.TrySetResult(true);
                        }
                    };
                    model.QuoteMgmtPage.Today.PropertyChanged += handler;
                    try
                    {
                        if (automatic)
                        {
                            timer.Start();
                        }
                        SetLatencyTag(panel, "settlType", settlement);
                        SetLatencyTag(panel, "quoteType", "收益率");
                        SetLatencyRange(panel, "bidPrice", "12.005", "12.009");
                        SetLatencyRange(panel, "bidQuantity", "400", "");
                        if (!automatic) {
                            timer.Start();
                            Click(Button(panel, "筛选"));
                        }
                        if (await Task.WhenAny(completion.Task, Task.Delay(5000)) != completion.Task)
                            throw new TimeoutException("组合筛选结果未在 5 秒内就绪；不计为成功样本");
                        await completion.Task;
                        await WaitForLatencyRenderAsync(grid);
                        double total = timer.Elapsed.TotalMilliseconds;
                        Require(expected.SetEquals(grid.Items.Cast<object>().Select(item =>
                            (string)item.GetType().GetProperty("QuoteId").GetValue(item))), "实际表格与预期 ID 集合不符");
                        int realized = Descendants<DataGridRow>(grid).Count();
                        measurements.Add(new JObject {
                            ["mode"] = iteration < 0 ? "cold-click" : automatic ? "automatic" : "click",
                            ["traceStartMs"] = (sampleStart - traceOrigin) * 1000.0 / Stopwatch.Frequency,
                            ["traceEndMs"] = (Stopwatch.GetTimestamp() - traceOrigin) * 1000.0 / Stopwatch.Frequency,
                            ["settlement"] = settlement, ["expectedRows"] = expected.Count,
                            ["gridItems"] = grid.Items.Count, ["realizedRows"] = realized,
                            ["inputToQueryMs"] = queryStart, ["queryToRowsMs"] = rowsReady - queryStart,
                            ["rowsToRenderMs"] = total - rowsReady, ["endToEndMs"] = total,
                            ["filter"] = SidebarFilter.Read(Descendants<FrameworkElement>(panel)
                                .First(item => SidebarFilter.GetScope(item) == "Combined")).ToJson()
                        });
                        File.WriteAllText(output, report.ToString());
                        Emit("过滤样本", "PASSED", iteration + ": " + total.ToString("F2") + " ms; " + expected.Count + " rows");
                    }
                    finally
                    {
                        model.QuoteMgmtPage.Today.PropertyChanged -= handler;
                    }
                    await Task.Delay(100);
                }
                Capture("filter-latency-result");
                report["status"] = "PASSED";
                report["correctness"] = "PASSED";
                report["referenceTargetMs"] = 100;
                var statistics = new JObject();
                foreach (string mode in new[] { "click", "automatic" }) {
                    double[] values = measurements.OfType<JObject>().Where(item => (string)item["mode"] == mode)
                        .Select(item => (double)item["endToEndMs"]).OrderBy(value => value).ToArray();
                    statistics[mode] = new JObject {
                        ["samples"] = values.Length,
                        ["p50Ms"] = values[(int)Math.Ceiling(values.Length * 0.50) - 1],
                        ["p95Ms"] = values[(int)Math.Ceiling(values.Length * 0.95) - 1],
                        ["maxMs"] = values.Last()
                    };
                }
                report["summary"] = statistics;
                report["referenceTargetMet"] = statistics.Properties().All(item => (double)item.Value["p95Ms"] < 100);
            }
            finally
            {
                grid.Dispatcher.Hooks.OperationStarted -= operationStarted;
                grid.Dispatcher.Hooks.OperationCompleted -= operationCompleted;
                ClientPerformanceTrace.Sink = null;
                if (profiling)
                {
                    lock (traceGate)
                    {
                        report["profile"] = new JArray(traces.Select(item => new JObject {
                            ["stage"] = item.Item1,
                            ["startMs"] = (item.Item2 - traceOrigin) * 1000.0 / Stopwatch.Frequency,
                            ["durationMs"] = (item.Item3 - item.Item2) * 1000.0 / Stopwatch.Frequency,
                            ["thread"] = item.Item4
                        }));
                    }
                }
                File.WriteAllText(output, report.ToString());
            }
        }

        private static void SetLatencyTag(FrameworkElement panel, string field, string label)
        {
            FrameworkElement control = Descendants<FrameworkElement>(panel).Single(item => SidebarFilter.GetField(item) == field);
            ToggleButton[] buttons = Descendants<ToggleButton>(control).ToArray();
            Require(buttons.Any(button => Convert.ToString(button.Content) == label), "缺少筛选项 " + label);
            foreach (ToggleButton button in buttons) button.IsChecked = Convert.ToString(button.Content) == label;
        }

        private static void SetLatencyRange(FrameworkElement panel, string field, string min, string max)
        {
            FilterRangeRow control = Descendants<FilterRangeRow>(panel).Single(item => SidebarFilter.GetField(item) == field);
            TextBox[] inputs = Descendants<TextBox>(control).ToArray();
            SetText(inputs[0], min);
            if (control.ShowMax) SetText(inputs[1], max);
        }

        private static async Task WaitForLatencyRenderAsync(DataGrid grid)
        {
            long queued = ClientPerformanceTrace.Start();
            await grid.Dispatcher.InvokeAsync(() =>
            {
                ClientPerformanceTrace.End("render.dispatch-wait", queued);
                long layoutStarted = ClientPerformanceTrace.Start();
                grid.UpdateLayout();
                ClientPerformanceTrace.End("render.update-layout", layoutStarted);
            }, DispatcherPriority.Render);
            long renderingStarted = ClientPerformanceTrace.Start();
            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler handler = (sender, args) => completion.TrySetResult(true);
            CompositionTarget.Rendering += handler;
            try {
                grid.InvalidateVisual();
                if (await Task.WhenAny(completion.Task, Task.Delay(3000)) != completion.Task)
                    throw new TimeoutException("WPF 未产生渲染回调");
                await completion.Task;
            }
            finally
            {
                CompositionTarget.Rendering -= handler;
                ClientPerformanceTrace.End("render.next-callback", renderingStarted);
            }
        }

        private async Task ChangeLatencyQuotesAsync(string operation, string[] ids)
        {
            if (ids.Length == 0) return;
            JObject reply = await model.Backend.Api.OperateAsync(operation, ids.Select(id => latencyQuotes[id]));
            foreach (JObject result in ((JArray)reply["results"]).OfType<JObject>().Where(item => (string)item["Code"] == "0000"))
            {
                string id = (string)result["quoteId"];
                latencyQuotes[id]["revision"] = result["revision"];
                if (operation == "confirm")
                {
                    latencyActive.Add(id);
                }
                if (operation == "refer")
                {
                    latencyActive.Remove(id);
                }
                if (operation == "delete")
                {
                    ownedQuotes.Remove(id);
                    latencyQuotes.Remove(id);
                }
            }
            FiccApiClient.RequireQuoteResults(reply);
            File.WriteAllText(Path.Combine(reportDirectory, "owned-quotes.json"), new JArray(ownedQuotes).ToString());
        }

        private async Task CloseLatencyViewsAsync()
        {
            // 测量前后隔离夹具写入与 UI 推送；测量本身由正式筛选控件重新打开真实服务视图。
            // 反射仅存在于测试，不为生产适配器增加测试 API。
            var sessions = (IDictionary)typeof(DpsSalesTradingServices).GetField("sessions",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(model.Backend);
            foreach (object session in sessions.Values.Cast<object>().ToArray()) {
                var page = (LivePage)session.GetType().GetProperty("Page").GetValue(session);
                await page.WaitForIdleAsync();
                await page.CloseAsync();
            }
        }

        private async Task CleanupLatencyQuotesAsync()
        {
            if (latencyQuotes.Count > 0) await CloseLatencyViewsAsync();
            while (latencyQuotes.Count > 0)
            {
                string[] ids = latencyQuotes.Keys.Take(25).ToArray();
                await ChangeLatencyQuotesAsync("refer", ids.Where(latencyActive.Contains).ToArray());
                await ChangeLatencyQuotesAsync("delete", ids);
            }
        }
    }
}
