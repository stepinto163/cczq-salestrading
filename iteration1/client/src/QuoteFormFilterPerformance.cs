using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    public sealed partial class QuoteForm {
        JObject performanceFilter;
        Action<string> filterPhase;

        Task OpenFilterPerformanceTest() {
            return StartGridPerformanceTest(false);
        }

        Task OpenScrollingPerformanceTest() {
            return StartGridPerformanceTest(true);
        }

        Task StartGridPerformanceTest(bool scrolling) {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "QuotePrototype.csproj"))) {
                directory = directory.Parent;
            }
            if (directory == null) {
                throw new InvalidOperationException("无法定位性能测试脚本。");
            }
            string script = Path.Combine(directory.Parent.FullName, "scripts", "test-filter-performance.py");
            Process.Start(new ProcessStartInfo("python",
                "\"" + script + "\" --quotes 500 " + (scrolling ? "--scrolling " : "") + "--client \"" + Application.ExecutablePath + "\"") {
                WorkingDirectory = directory.Parent.FullName,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            state.Text = "已启动独立" + (scrolling ? "滚动" : "筛选") + "性能测试：500 条报价；结果在新窗口及 runtime/filter-performance 下。";
            return Task.CompletedTask;
        }

        internal async Task RunFilterPerformance(string configurationFile) {
            JObject configuration = JObject.Parse(File.ReadAllText(configurationFile));
            string resultFile = (string)configuration["resultFile"];
            string heartbeatFile = (string)configuration["heartbeatFile"];
            int repeats = (int)configuration["repeats"];
            int quoteCount = (int)configuration["quoteCount"];
            int gatewayPort = (int)configuration["gatewayPort"];
            if (gatewayPort != 18700 || repeats < 1 || repeats > 10 || quoteCount < 1 || quoteCount > 50000) {
                throw new InvalidOperationException("无效的独立性能测试配置。");
            }
            automatedUi = true;
            busy = true;
            foreach (Button button in actions) {
                button.Enabled = false;
            }
            Text = "筛选性能测试 · " + quoteCount + " 条服务端报价 / 最多 200 条加载";
            var samples = new JArray();
            var summaries = new JArray();
            var report = new JObject {
                ["result"] = "RUNNING",
                ["quoteCount"] = quoteCount,
                ["loadedRowLimit"] = 200,
                ["targetMs"] = 100,
                ["samples"] = samples,
                ["scenarios"] = summaries,
                ["messageLogDirectory"] = client.LogDirectory,
                ["measurement"] = "Filter change to UI continuation after grid WM_PAINT; includes close/open, JSON and full file logging",
                ["scope"] = "Single client, local GW, static quotes, warm in-memory backend; not concurrent load or streaming-update pressure"
            };
            var heartbeat = new System.Windows.Forms.Timer { Interval = 100 };
            heartbeat.Tick += (sender, args) => File.WriteAllText(heartbeatFile, DateTime.UtcNow.ToString("o"));
            heartbeat.Start();
            File.WriteAllText(heartbeatFile, DateTime.UtcNow.ToString("o"));
            JArray requestMeasurements = null;
            Action<JObject> measureRequest = measurement => requestMeasurements?.Add(measurement);
            client.RequestMeasured += measureRequest;
            try {
                await Task.Run(() => client.Connect("demo", gatewayPort));
                connected = true;
                user.Enabled = false;
                JObject stream = await Request("ficc.quote.stream.open", new JObject());
                streamId = (string)stream["streamId"];
                topic = (string)stream["pushTopic"];
                sequence = (long?)stream["sequence"] ?? 0;
                await Task.Run(() => client.Subscribe(topic, false));
                statusFilter.SelectedIndex = 2;
                foreach (JObject scenario in ((JArray)configuration["scenarios"]).OfType<JObject>()) {
                    var measured = new List<JObject>();
                    for (int iteration = 0; iteration <= repeats; iteration++) {
                        string name = (string)scenario["name"];
                        state.Text = name + " · " + (iteration == 0 ? "预热" : iteration + "/" + repeats);
                        performanceFilter = (JObject)scenario["filter"].DeepClone();
                        var watch = Stopwatch.StartNew();
                        var phases = new Dictionary<string, double>();
                        requestMeasurements = new JArray();
                        filterPhase = phase => phases[phase] = watch.Elapsed.TotalMilliseconds;
                        await OpenView();
                        await WaitForGridPaint();
                        double total = watch.Elapsed.TotalMilliseconds;
                        filterPhase = null;
                        if (!ids.SequenceEqual(((JArray)scenario["expectedIds"]).Select(value => (string)value))
                            || lastFilterTotalCount != (int)scenario["expectedCount"]) {
                            throw new InvalidOperationException("筛选结果或顺序与固定数据不一致：" + name);
                        }
                        var sample = new JObject {
                            ["scenario"] = name,
                            ["requests"] = requestMeasurements,
                            ["warmup"] = iteration == 0,
                            ["closeMs"] = phases["close.end"] - phases["close.start"],
                            ["openRequestMs"] = phases["open.worker.end"] - phases["open.worker.start"],
                            ["openDispatchMs"] = phases["open.end"] - phases["open.start"]
                                - (phases["open.worker.end"] - phases["open.worker.start"]),
                            ["applyMs"] = phases["apply.end"] - phases["open.end"],
                            ["paintAndDispatchMs"] = total - phases["apply.end"],
                            ["gridPaintMs"] = grid.LastPaintMilliseconds,
                            ["totalMs"] = total,
                            ["matchedQuotes"] = lastFilterTotalCount,
                            ["loadedRows"] = rows.Count
                        };
                        samples.Add(sample);
                        if (iteration > 0) {
                            measured.Add(sample);
                        }
                        File.WriteAllText(resultFile, report.ToString());
                        if (total > 1500) {
                            throw new TimeoutException("单次筛选超过 1500ms，已停止本轮，不继续加压。");
                        }
                        await Task.Delay(250);
                    }
                    var summary = new JObject {
                        ["name"] = scenario["name"],
                        ["samples"] = measured.Count,
                        ["p50Ms"] = Percentile(measured, "totalMs", 0.50),
                        ["p95Ms"] = Percentile(measured, "totalMs", 0.95),
                        ["maxMs"] = measured.Max(value => (double)value["totalMs"]),
                        ["openRequestP95Ms"] = Percentile(measured, "openRequestMs", 0.95)
                    };
                    summary["targetMet"] = (double)summary["p95Ms"] <= 100;
                    summaries.Add(summary);
                }
                report["result"] = "COMPLETED";
                report["targetMet"] = summaries.OfType<JObject>().All(value => (bool)value["targetMet"]);
            } catch (Exception error) {
                report["result"] = "STOPPED";
                report["error"] = error.Message;
                throw;
            } finally {
                client.RequestMeasured -= measureRequest;
                filterPhase = null;
                performanceFilter = null;
                heartbeat.Stop();
                heartbeat.Dispose();
                File.WriteAllText(resultFile, report.ToString());
                detail.Text = report.ToString();
            }
        }

        async Task WaitForGridPaint() {
            var painted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            PaintEventHandler handler = (sender, args) => painted.TrySetResult(true);
            grid.Paint += handler;
            try {
                grid.Invalidate();
                if (await Task.WhenAny(painted.Task, Task.Delay(1000)) != painted.Task) {
                    throw new TimeoutException("等待网格重绘超过 1 秒；请保持测试窗口可见。");
                }
                await painted.Task;
            } finally {
                grid.Paint -= handler;
            }
        }

        static double Percentile(List<JObject> samples, string field, double quantile) {
            double[] values = samples.Select(value => (double)value[field]).OrderBy(value => value).ToArray();
            return values[Math.Max(0, (int)Math.Ceiling(values.Length * quantile) - 1)];
        }
    }
}
