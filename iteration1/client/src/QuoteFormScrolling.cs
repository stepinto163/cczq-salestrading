using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    public sealed partial class QuoteForm {
        const int MaximumLoadedQuotes = 10000;
        readonly SemaphoreSlim quoteViewGate = new SemaphoreSlim(1, 1);
        int quoteBatchSize = 200;
        int quoteViewLimit;
        bool extendingQuotes;
        bool renderingQuotes;
        bool quoteViewStale;
        bool quoteBufferOverflow;
        string quoteLoadError;
        // Used only by the bounded functional test; normal calls go through DPS/GW.
        Func<string, JObject, Task<JObject>> quoteViewRequestOverride;
        Action<JObject> quoteExtensionMeasured;

        void InitializeQuoteScrolling(Control filterBar) {
            grid.Scroll += async (sender, args) => {
                if (args.ScrollOrientation == ScrollOrientation.VerticalScroll) {
                    await LoadQuotesNearBottom();
                }
            };
            grid.CurrentCellChanged += async (sender, args) => await LoadQuotesNearBottom();
            AddButton(filterBar, "加载更多 / 重试", () => ExtendQuoteView());
        }

        Task<JObject> RequestQuoteView(string requestTopic, JObject request) {
            return quoteViewRequestOverride != null
                ? quoteViewRequestOverride(requestTopic, request)
                : Request(requestTopic, request);
        }

        async Task LoadQuotesNearBottom() {
            if (renderingQuotes || opening || busy || extendingQuotes || !connected || quoteViewStale) return;
            if (rows.Count == 0 || rows.Count >= lastFilterTotalCount || quoteViewLimit >= MaximumLoadedQuotes) return;
            int first = grid.FirstDisplayedScrollingRowIndex;
            if (first < 0) return;
            int lastVisible = first + grid.DisplayedRowCount(false);
            int threshold = Math.Min(40, Math.Max(1, quoteBatchSize / 4));
            if (lastVisible < rows.Count - threshold) return;
            await ExtendQuoteView();
        }

        async Task OpenView() {
            if (!connected) throw new Exception("请先连接 GW。");
            await quoteViewGate.WaitAsync();
            opening = true;
            pending.Clear();
            quoteBufferOverflow = false;
            quoteViewStale = false;
            quoteLoadError = null;
            try {
                filterPhase?.Invoke("close.start");
                if (viewId != null) {
                    await RequestQuoteView("ficc.quote.view.close", new JObject { ["viewId"] = viewId });
                }
                filterPhase?.Invoke("close.end");
                viewId = "V-" + Guid.NewGuid().ToString("N");
                var statuses = statusFilter.SelectedIndex == 0
                    ? new JArray("PENDING_CONFIRM", "ACTIVE", "REFERRED", "DELETED")
                    : new JArray(new[] { "", "PENDING_CONFIRM", "ACTIVE", "REFERRED", "DELETED" }[statusFilter.SelectedIndex]);
                filterPhase?.Invoke("open.start");
                JObject reply = await RequestQuoteView("ficc.quote.view.open", new JObject {
                    ["viewId"] = viewId, ["ownerScope"] = "MINE", ["status"] = statuses,
                    ["limit"] = quoteBatchSize, ["filter"] = Filter()
                });
                filterPhase?.Invoke("open.end");
                ApplyQuoteViewReply(reply, false);
                filterPhase?.Invoke("apply.end");
            } catch {
                quoteViewStale = true;
                throw;
            } finally {
                FinishQuoteViewRequest();
                quoteViewGate.Release();
            }
        }

        async Task ExtendQuoteView() {
            if (!connected || viewId == null || opening || extendingQuotes || quoteViewStale) return;
            if (rows.Count >= lastFilterTotalCount || quoteViewLimit >= MaximumLoadedQuotes) return;
            // Do not queue requests behind a filter change, or issue one request per wheel event.
            if (!await quoteViewGate.WaitAsync(0)) return;
            extendingQuotes = true;
            opening = true;
            pending.Clear();
            quoteBufferOverflow = false;
            quoteLoadError = null;
            UpdateQuoteLoadState();
            try {
                int limit = Math.Min(MaximumLoadedQuotes, quoteViewLimit + quoteBatchSize);
                var watch = Stopwatch.StartNew();
                JObject reply = await RequestQuoteView("ficc.quote.view.extend", new JObject {
                    ["viewId"] = viewId, ["limit"] = limit
                });
                double requestMs = watch.Elapsed.TotalMilliseconds;
                ApplyQuoteViewReply(reply, true);
                quoteExtensionMeasured?.Invoke(new JObject {
                    ["limit"] = limit, ["loadedRows"] = rows.Count, ["requestMs"] = requestMs,
                    ["applyMs"] = watch.Elapsed.TotalMilliseconds - requestMs,
                    ["totalMs"] = watch.Elapsed.TotalMilliseconds
                });
            } catch (Exception error) {
                // A timeout may have advanced the server limit; resynchronize instead of blindly retrying.
                quoteViewStale = true;
                quoteLoadError = "加载失败，请点击筛选重新同步：" + error.Message;
                Log("QUOTE_SCROLL " + error.Message);
            } finally {
                extendingQuotes = false;
                FinishQuoteViewRequest();
                quoteViewGate.Release();
            }
        }

        void ApplyQuoteViewReply(JObject reply, bool preservePosition) {
            if ((string)reply["view"]?["viewId"] != viewId || (string)reply["streamId"] != streamId) {
                throw new Exception("报价视图回复不匹配，请重新筛选。");
            }
            string selected = preservePosition ? (string)Selected()?["quoteId"] : null;
            int first = preservePosition ? grid.FirstDisplayedScrollingRowIndex : 0;
            string anchor = first >= 0 && first < rows.Count ? (string)rows[first]["quoteId"] : null;
            cache.Clear();
            ids.Clear();
            foreach (JObject quote in (JArray)reply["quotes"]) cache[(string)quote["quoteId"]] = quote;
            foreach (JToken id in (JArray)reply["view"]["ids"]) ids.Add((string)id);
            quoteViewLimit = (int)reply["view"]["limit"];
            lastFilterTotalCount = (int)reply["view"]["totalCount"];
            sequence = (long)reply["sequence"];
            RenderQuoteRows(selected, anchor, first);
        }

        void FinishQuoteViewRequest() {
            opening = false;
            var buffered = pending.ToArray();
            pending.Clear();
            if (quoteBufferOverflow) quoteViewStale = true;
            foreach (var message in buffered) Receive(message.Item1, message.Item2);
            UpdateQuoteLoadState();
        }

        void ReceiveQuote(JObject message) {
            if ((string)message["streamId"] != streamId) return;
            if (opening) {
                if (pending.Count < 500) pending.Add(Tuple.Create(topic, message));
                else quoteBufferOverflow = true;
                return;
            }
            if (quoteViewStale) return;
            long next = (long?)message["sequence"] ?? sequence;
            if (next <= sequence) return;
            if (next != sequence + 1 || (string)message["kind"] == "RESET_REQUIRED") {
                quoteViewStale = true;
                UpdateQuoteLoadState();
                return;
            }
            sequence = next;
            if ((string)message["kind"] == "HEARTBEAT") return;
            foreach (JObject quote in (JArray)message["quotes"] ?? new JArray()) cache[(string)quote["quoteId"]] = quote;
            foreach (JObject view in (JArray)message["views"] ?? new JArray()) {
                if ((string)view["viewId"] != viewId) continue;
                foreach (JToken id in (JArray)view["removeIds"] ?? new JArray()) ids.Remove((string)id);
                foreach (JObject placement in (JArray)view["place"] ?? new JArray()) {
                    string id = (string)placement["id"];
                    ids.Remove(id);
                    ids.Insert(Math.Min((int)placement["index"], ids.Count), id);
                }
                lastFilterTotalCount = (int?)view["totalCount"] ?? lastFilterTotalCount;
            }
            var visible = new HashSet<string>(ids);
            foreach (string id in cache.Keys.Where(id => !visible.Contains(id)).ToArray()) cache.Remove(id);
            if (ids.Any(id => !cache.ContainsKey(id))) {
                quoteViewStale = true;
                UpdateQuoteLoadState();
                return;
            }
            Render();
        }

        void Render() {
            int first = grid.FirstDisplayedScrollingRowIndex;
            string anchor = first >= 0 && first < rows.Count ? (string)rows[first]["quoteId"] : null;
            RenderQuoteRows((string)Selected()?["quoteId"], anchor, first);
        }

        void RenderQuoteRows(string selected, string anchor, int first) {
            renderingQuotes = true;
            try {
                rows.Clear();
                foreach (string id in ids) if (cache.TryGetValue(id, out var quote)) rows.Add(quote);
                grid.RowCount = rows.Count;
                int selection = rows.FindIndex(quote => (string)quote["quoteId"] == selected);
                if (selection >= 0) grid.CurrentCell = grid[0, selection];
                int anchorIndex = rows.FindIndex(quote => (string)quote["quoteId"] == anchor);
                if (rows.Count > 0) grid.FirstDisplayedScrollingRowIndex = Math.Max(0, Math.Min(rows.Count - 1, anchorIndex >= 0 ? anchorIndex : first));
                grid.Invalidate();
            } finally {
                renderingQuotes = false;
                UpdateQuoteLoadState();
            }
        }

        void UpdateQuoteLoadState() {
            string suffix = extendingQuotes ? " · 正在加载更多" : "";
            if (quoteViewStale) suffix = " · " + (quoteLoadError ?? "更新不连续，请点击筛选重新同步");
            else if (quoteViewLimit >= MaximumLoadedQuotes && rows.Count < lastFilterTotalCount) suffix = " · 已达当前接口 10,000 条上限，请缩小筛选范围";
            state.Text = "已连接 GW · 已加载 " + rows.Count + " / " + lastFilterTotalCount + " 条 · 序号 " + sequence + suffix;
        }
    }
}
