using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class QuoteForm {
        const int MaximumScrollingTestQuotes = 10000;
        readonly SemaphoreSlim quoteViewGate = new SemaphoreSlim(1, 1);
        int quoteBatchSize = 200;
        int quoteViewLimit;
        int quotePageNumber = 1;
        bool quoteRefillPending;
        bool loadingQuotePage;
        bool renderingQuotes;
        bool quoteViewStale;
        bool quoteBufferOverflow;
        string quoteLoadError;
        // Used only by the bounded functional test; normal calls go through DPS/GW.
        Func<string, JObject, Task<JObject>> quoteViewRequestOverride;
        Action<JObject> quotePageMeasured;

        void InitializeQuoteScrolling(Control filterBar) {
            grid.Scroll += async (sender, args) => {
                if (args.ScrollOrientation == ScrollOrientation.VerticalScroll) {
                    await LoadQuotesNearBottom();
                }
            };
            AddButton(filterBar, "上一批", () => QueryQuotePage(quotePageNumber - 1));
            AddButton(filterBar, "返回最新", () => QueryQuotePage(1));
            AddButton(filterBar, "下一批", () => NextQuotePage());
        }

        Task<JObject> RequestQuoteView(string requestTopic, JObject request) {
            return quoteViewRequestOverride != null
                ? quoteViewRequestOverride(requestTopic, request)
                : Request(requestTopic, request);
        }

        async Task LoadQuotesNearBottom() {
            if (renderingQuotes || opening || busy || loadingQuotePage || !connected || quoteViewStale) return;
            if (rows.Count == 0 || (long)quotePageNumber * quoteBatchSize >= lastFilterTotalCount) return;
            int first = grid.FirstDisplayedScrollingRowIndex;
            if (first < 0) return;
            int lastVisible = first + grid.DisplayedRowCount(false);
            int threshold = Math.Min(40, Math.Max(1, quoteBatchSize / 4));
            if (lastVisible < rows.Count - threshold) return;
            await NextQuotePage();
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
                    ["pageSize"] = quoteBatchSize, ["filter"] = Filter()
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

        Task NextQuotePage() {
            return QueryQuotePage(quotePageNumber + 1);
        }

        async Task QueryQuotePage(int pageNumber) {
            if (!connected || viewId == null || opening || loadingQuotePage || quoteViewStale) return;
            if (pageNumber < 1 || (pageNumber > quotePageNumber && (long)(pageNumber - 1) * quoteBatchSize >= lastFilterTotalCount)) return;
            // Do not queue requests behind a filter change, or issue one request per wheel event.
            if (!await quoteViewGate.WaitAsync(0)) return;
            loadingQuotePage = true;
            opening = true;
            pending.Clear();
            quoteBufferOverflow = false;
            quoteLoadError = null;
            UpdateQuoteLoadState();
            try {
                int limit = quoteBatchSize;
                var watch = Stopwatch.StartNew();
                JObject reply = await RequestQuoteView("ficc.quote.view.query", new JObject {
                    ["viewId"] = viewId, ["pageNumber"] = pageNumber, ["pageSize"] = quoteBatchSize
                });
                double requestMs = watch.Elapsed.TotalMilliseconds;
                ApplyQuoteViewReply(reply, pageNumber == quotePageNumber);
                quotePageMeasured?.Invoke(new JObject {
                    ["limit"] = limit, ["loadedRows"] = rows.Count, ["requestMs"] = requestMs,
                    ["applyMs"] = watch.Elapsed.TotalMilliseconds - requestMs,
                    ["totalMs"] = watch.Elapsed.TotalMilliseconds
                });
            } catch (Exception error) {
                // Preserve the displayed page and require resynchronization after a failed query.
                quoteViewStale = true;
                quoteLoadError = "加载失败，请点击筛选重新同步：" + error.Message;
                Log("QUOTE_SCROLL " + error.Message);
            } finally {
                loadingQuotePage = false;
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
            quoteViewLimit = (int)reply["view"]["pageSize"];
            quotePageNumber = (int)reply["view"]["pageNumber"];
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
            bool refill;
            try {
                refill = ClientViewPage.Apply(message, viewId, "quotes", "quoteId",
                    quotePageNumber, quoteBatchSize, ids, cache);
            } catch (Exception error) {
                quoteViewStale = true;
                quoteLoadError = error.Message;
                UpdateQuoteLoadState();
                return;
            }
            foreach (JObject view in (JArray)message["views"] ?? new JArray()) {
                if ((string)view["viewId"] == viewId) lastFilterTotalCount = (int)view["totalCount"];
            }
            if (refill) ScheduleQuoteRefill();
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

        async void ScheduleQuoteRefill() {
            if (quoteRefillPending) return;
            quoteRefillPending = true;
            string requestedView = viewId;
            try {
                await Task.Delay(40);
                while (!IsDisposed && (opening || loadingQuotePage)) await Task.Delay(20);
                quoteRefillPending = false;
                if (!IsDisposed && viewId == requestedView && quotePageNumber == 1 && !quoteViewStale) {
                    await QueryQuotePage(1);
                }
            } finally {
                quoteRefillPending = false;
            }
        }

        void UpdateQuoteLoadState() {
            string suffix = loadingQuotePage ? " · 正在加载更多" : "";
            if (quoteViewStale) suffix = " · " + (quoteLoadError ?? "更新不连续，请点击筛选重新同步");

            state.Text = "已连接 GW · 本批 " + rows.Count + " 条 / 匹配 " + lastFilterTotalCount
                + " 条 · 第 " + quotePageNumber + " 批 · 序号 " + sequence + suffix;
        }
    }
}
