using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class QuoteForm {
        internal async Task RunScrollingSmoke(string resultFile) {
            var checks = new JArray();
            Action<bool, string> check = (condition, name) => {
                if (!condition) throw new Exception(name);
                checks.Add(name);
            };
            connected = true;
            automatedUi = true;
            quoteBatchSize = 20;
            streamId = "SCROLL-TEST";
            topic = "test.quote";
            int requests = 0;
            TaskCompletionSource<bool> release = null;
            var fixture = Enumerable.Range(0, 65).Select(index => new JObject {
                ["quoteId"] = "SCROLL-" + index, ["securityId"] = "250001.IB", ["side"] = "1",
                ["price"] = "2.5000", ["quantity"] = "1", ["settlType"] = "1", ["revision"] = "1",
                ["updateTime"] = DateTimeOffset.UtcNow.AddSeconds(-index).ToString("o")
            }).ToArray();
            quoteViewRequestOverride = async (requestTopic, request) => {
                if (requestTopic.EndsWith("close")) return new JObject();
                if (requestTopic.EndsWith("query")) {
                    requests++;
                    if (release != null) await release.Task;
                }
                int page = (int?)request["pageNumber"] ?? 1;
                int size = (int)request["pageSize"];
                var records = fixture.Skip((page - 1) * size).Take(size).ToArray();
                return new JObject {
                    ["streamId"] = streamId, ["sequence"] = 0,
                    ["quotes"] = new JArray(records.Select(row => row.DeepClone())),
                    ["view"] = new JObject { ["viewId"] = request["viewId"], ["pageNumber"] = page,
                        ["pageSize"] = size, ["totalCount"] = fixture.Length,
                        ["ids"] = new JArray(records.Select(row => (string)row["quoteId"])) }
                };
            };
            await OpenView();
            check(rows.Count == 20 && grid.VirtualMode, "Initial page uses virtual mode");
            release = new TaskCompletionSource<bool>();
            Task query = NextQuotePage();
            await NextQuotePage();
            check(requests == 1, "Overlapping requests are coalesced");
            release.SetResult(true);
            await query;
            release = null;
            check(rows.Count == 20 && quotePageNumber == 2 && ids[0] == "SCROLL-20", "Next page replaces rather than accumulates rows");
            grid.FirstDisplayedScrollingRowIndex = 19;
            await Task.Delay(60);
            check(quotePageNumber == 3 && rows.Count == 20, "Actual grid scroll requests one next page");
            await NextQuotePage();
            check(rows.Count == 5 && quotePageNumber == 4, "Final partial page");
            int before = requests;
            await NextQuotePage();
            check(requests == before, "End of data issues no request");
            await QueryQuotePage(1);
            check(rows.Count == 20 && quotePageNumber == 1, "Return to latest re-queries page one");
            foreach (string collection in new[] { "quotes", "books", "notifications" }) {
                string key = collection == "quotes" ? "quoteId" : collection == "books" ? "securityId" : "notificationId";
                var localIds = new List<string> { "A", "B" };
                var localRows = new Dictionary<string, JObject> {
                    ["A"] = new JObject { [key] = "A", ["updateTime"] = "2026-09-17T10:00:02Z" },
                    ["B"] = new JObject { [key] = "B", ["updateTime"] = "2026-09-17T10:00:01Z" }
                };
                var update = new JObject {
                    [collection] = new JArray(new JObject { [key] = "J", ["viewId"] = "V", ["updateTime"] = "2026-09-17T10:00:03Z" }),
                    ["views"] = new JArray(new JObject { ["viewId"] = "V", ["upsertIds"] = new JArray("J"), ["removeIds"] = new JArray() })
                };
                check(!ClientViewPage.Apply(update, "V", collection, key, 1, 2, localIds, localRows)
                    && localIds.SequenceEqual(new[] { "J", "A" }), collection + ": insert, sort and trim locally");
                update["views"][0]["upsertIds"] = new JArray();
                update["views"][0]["removeIds"] = new JArray("J");
                check(ClientViewPage.Apply(update, "V", collection, key, 1, 2, localIds, localRows), collection + ": first-page removal needs refill");
                check(!ClientViewPage.Apply(update, "V", collection, key, 2, 2, localIds, localRows), collection + ": later page does not refill");
            }
            quoteViewRequestOverride = (requestTopic, request) => throw new TimeoutException("Simulated timeout");
            await NextQuotePage();
            check(quoteViewStale && rows.Count == 20, "Failed query preserves rows and requires resynchronization");
            File.WriteAllText(resultFile, new JObject { ["result"] = "PASS", ["checks"] = checks,
                ["fixtureQuotes"] = fixture.Length, ["serverWrites"] = 0 }.ToString());
        }

        internal async Task RunScrollingPerformance(string configurationFile) {
            JObject configuration = JObject.Parse(File.ReadAllText(configurationFile));
            int count = (int)configuration["quoteCount"];
            int port = (int)configuration["gatewayPort"];
            if (port != 18700 || count < 1 || count > MaximumScrollingTestQuotes) throw new Exception("滚动测试仅允许独立 GW 18700 和最多 10,000 条。");
            string resultFile = (string)configuration["resultFile"];
            string heartbeatFile = (string)configuration["heartbeatFile"];
            var samples = new List<JObject>();
            var extensions = new JArray();
            var requests = new ConcurrentQueue<JObject>();
            var report = new JObject { ["result"] = "RUNNING", ["quoteCount"] = count,
                ["samples"] = new JArray(), ["extensions"] = extensions, ["scenarios"] = new JArray(),
                ["measurement"] = "Programmatic vertical scroll through the real grid; request/apply and WM_PAINT; not OS mouse input latency",
                ["scope"] = "One client, static quotes, isolated QuoteSvr/GW/schema; no live update load" };
            automatedUi = true;
            foreach (Button action in actions) action.Enabled = false;
            Text = "滚动性能测试 · " + count + " 条 · 单次有界测试";
            var clock = Stopwatch.StartNew();
            double lastTick = 0;
            double maximumUiGap = 0;
            var heartbeat = new System.Windows.Forms.Timer { Interval = 50 };
            heartbeat.Tick += (sender, args) => {
                double now = clock.Elapsed.TotalMilliseconds;
                maximumUiGap = Math.Max(maximumUiGap, now - lastTick);
                lastTick = now;
                File.WriteAllText(heartbeatFile, DateTime.UtcNow.ToString("o"));
            };
            Action<JObject> measure = request => requests.Enqueue(request);
            client.RequestMeasured += measure;
            quotePageMeasured = sample => extensions.Add(sample);
            heartbeat.Start();
            File.WriteAllText(heartbeatFile, DateTime.UtcNow.ToString("o"));
            try {
                await Task.Run(() => client.Connect("trader1", port));
                connected = true;
                JObject stream = await Request("ficc.quote.stream.open", new JObject());
                streamId = (string)stream["streamId"];
                topic = (string)stream["pushTopic"];
                await Task.Run(() => client.Subscribe(topic, false));
                statusFilter.SelectedIndex = 2;
                await OpenView();
                if (lastFilterTotalCount != count) throw new Exception("滚动测试数据数量不匹配。");
                grid.CurrentCell = grid[0, 0];
                string selected = (string)Selected()["quoteId"];
                var scrollingClock = Stopwatch.StartNew();
                for (int step = 0; step < 600; step++) {
                    int first = grid.FirstDisplayedScrollingRowIndex;
                    int visible = Math.Max(1, grid.DisplayedRowCount(false));
                    bool complete = (long)quotePageNumber * quoteBatchSize >= count && first + visible >= rows.Count;
                    if (complete) break;
                    var watch = Stopwatch.StartNew();
                    int next = Math.Min(rows.Count - 1, first + Math.Max(1, visible / 2));
                    grid.FirstDisplayedScrollingRowIndex = next;
                    await WaitForGridPaint();
                    double scrollToPaint = watch.Elapsed.TotalMilliseconds;
                    var sample = new JObject { ["step"] = step, ["loadedRows"] = rows.Count,
                        ["scrollToPaintMs"] = scrollToPaint, ["gridPaintMs"] = grid.LastPaintMilliseconds,
                        ["workingSetMB"] = Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0) };
                    samples.Add(sample);
                    while (loadingQuotePage && watch.ElapsedMilliseconds < 1500) await Task.Delay(20);
                    if (quoteViewStale || loadingQuotePage || scrollToPaint > 500 || maximumUiGap > 1000) throw new Exception("加载或 UI 响应超过保护阈值，停止加压。");
                    if (scrollingClock.Elapsed.TotalSeconds > 35) throw new Exception("本轮滚动达到 35 秒上限，停止加压。");
                    if (quotePageNumber == 1 && (string)Selected()?["quoteId"] != selected) throw new Exception("滚动意外改变选中报价。");
                    if (ids.Distinct().Count() != rows.Count) throw new Exception("滚动出现重复记录。");
                    report["samples"] = new JArray(samples);
                    File.WriteAllText(resultFile, report.ToString());
                    await Task.Delay(50);
                }
                if ((long)quotePageNumber * quoteBatchSize < count || grid.FirstDisplayedScrollingRowIndex + grid.DisplayedRowCount(false) < rows.Count) throw new Exception("未完成滚动到末尾。");
                report["result"] = "COMPLETED";
                report["loadedRows"] = rows.Count;
                report["selectionPreservedWithinPage"] = true;
                report["pageNumber"] = quotePageNumber;
                report["scrollP95Ms"] = Percentile(samples, "scrollToPaintMs", 0.95);
                report["scrollMaxMs"] = samples.Max(sample => (double)sample["scrollToPaintMs"]);
            } catch (Exception error) {
                report["result"] = "STOPPED";
                report["error"] = error.Message;
                throw;
            } finally {
                heartbeat.Stop();
                heartbeat.Dispose();
                client.RequestMeasured -= measure;
                quotePageMeasured = null;
                report["samples"] = new JArray(samples);
                report["requests"] = new JArray(requests);
                report["maximumUiTimerGapMs"] = maximumUiGap;
                report["peakWorkingSetMB"] = Process.GetCurrentProcess().PeakWorkingSet64 / (1024.0 * 1024.0);
                File.WriteAllText(resultFile, report.ToString());
            }
        }
    }
}
