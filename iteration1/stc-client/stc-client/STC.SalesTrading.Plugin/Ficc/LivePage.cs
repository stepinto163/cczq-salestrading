using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;

namespace STC.SalesTrading.Plugin.Ficc
{
    // UI-thread owned; each panel has its own membership and query boundary.
    public sealed class LivePage : IDisposable
    {
        private readonly FiccApiClient api;
        private readonly ViewProtocolClient protocol;
        private readonly IFiccTransport transport;
        private readonly string domain;
        private readonly string collection;
        private readonly string idField;
        private readonly List<JObject> pending = new List<JObject>();
        private IDisposable subscription;
        private string streamId;
        private string viewId;
        private long sequence;
        private bool loading;
        private TaskCompletionSource<bool> loadCompletion;
        private bool stale = true;
        private bool disposed;
        private bool overflow;
        public ObservableCollection<JObject> Rows { get; } = new ObservableCollection<JObject>();
        public int PageNumber { get; private set; } = 1;
        public int PageSize { get; set; } = 30;
        public string OwnerScope { get; set; } = "MINE";
        public int TotalCount { get; private set; }
        public bool IsReady => !stale && !loading && !disposed;
        public string Topic { get; private set; }
        public string PriceMode { get; set; } = "BEST";
        public JArray QuoteStatuses { get; set; } = new JArray("ACTIVE", "REFERRED");
        public bool IsBrowsing { get; private set; }
        public int PendingUpdateCount => changedIds.Count;
        public int Generation { get; private set; }
        private readonly HashSet<string> changedIds = new HashSet<string>();
        public event Action Changed;
        public event Action Updated;
        public event Action<string[]> Removed;
        public event Action<string> Failed;

        public LivePage(FiccApiClient api, IFiccTransport transport, string domain)
        {
            this.api = api;
            protocol = new ViewProtocolClient(api, domain);
            this.transport = transport;
            this.domain = domain;
            collection = domain == "quote" ? "quotes" : domain == "price" ? "books" : "notifications";
            idField = domain == "quote" ? "quoteId" : domain == "price" ? "securityId" : "notificationId";
            transport.ConnectionChanged += ConnectionChanged;
        }

        public async Task WaitForIdleAsync()
        {
            while (loading)
            {
                await loadCompletion.Task;
            }
        }

        public async Task OpenAsync(JObject filter, bool pendingOnly = false)
        {
            if (loading || disposed)
            {
                throw new InvalidOperationException("视图忙碌或已关闭。");
            }
            await CloseAsync();
            Generation++;
            IsBrowsing = false;
            changedIds.Clear();
            StreamOpenData stream = await protocol.OpenStreamAsync();
            streamId = stream.StreamId;
            Topic = stream.PushTopic;
            viewId = "NATIVE-" + Guid.NewGuid().ToString("N");
            await LoadAsync(async () =>
            {
                subscription = transport.Subscribe(FiccApiClient.ServiceFor("ficc." + domain + ".stream.open"),
                    Topic, Receive, domain == "price");
                return await protocol.OpenAsync(viewId, PageSize, ProtocolCodec.Decode<Filter>(filter),
                    OwnerScope, pendingOnly ? new List<string> { "PENDING_CONFIRM" } : QuoteStatuses.Values<string>().ToList(), PriceMode);
            });
        }

        public Task QueryAsync(int pageNumber)
        {
            if (viewId == null || pageNumber < 1 || loading || disposed)
            {
                throw new InvalidOperationException("请先打开视图并等待当前请求结束。");
            }
            return LoadAsync(() => protocol.QueryAsync(viewId, pageNumber, PageSize));
        }

        public void BeginBrowsing()
        {
            if (IsBrowsing)
            {
                return;
            }
            IsBrowsing = true;
            Changed?.Invoke();
        }

        public Task ResumeLatestAsync()
        {
            IsBrowsing = false;
            changedIds.Clear();
            Generation++;
            return QueryAsync(1);
        }

        private async Task LoadAsync(Func<Task<JObject>> query)
        {
            loadCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            loading = true;
            overflow = false;
            pending.Clear();
            try
            {
                JObject reply = await query();
                if (disposed)
                {
                    return;
                }
                ViewPage view = ProtocolCodec.Decode<ViewPage>((JObject)reply["view"]);
                if (overflow || (string)reply["streamId"] != streamId || view.ViewId != viewId)
                {
                    throw new InvalidOperationException("视图边界失效，请重新查询。");
                }
                var records = ((JArray)reply[collection]).OfType<JObject>().ToDictionary(row => (string)row[idField]);
                SetRows(view.Ids.Select(id => records[id]).ToArray());
                PageNumber = view.PageNumber.Value;
                TotalCount = view.TotalCount.Value;
                sequence = (long)reply["sequence"];
                stale = false;
            }
            catch
            {
                stale = true;
                throw;
            }
            finally
            {
                loading = false;
                loadCompletion.TrySetResult(true);
            }
            JObject[] buffered = pending.ToArray();
            pending.Clear();
            foreach (JObject message in buffered)
            {
                Receive(message);
            }
            Changed?.Invoke();
        }

        private void Receive(JObject message)
        {
            if (disposed || (string)message["streamId"] != streamId)
            {
                return;
            }
            if (loading)
            {
                if (pending.Count < 200)
                {
                    pending.Add(message);
                }
                else
                {
                    overflow = true;
                }
                return;
            }
            if (stale)
            {
                return;
            }
            StreamMessage batch = protocol.ReadPush(message, out List<ViewDelta> viewChanges, out List<JObject> changedRecords);
            long next = batch.Sequence == null ? -1 : long.Parse(batch.Sequence, CultureInfo.InvariantCulture);
            if (batch.Kind == "RESET_REQUIRED")
            {
                Invalidate("服务要求重建视图，请重新查询。");
                return;
            }
            if (next <= sequence)
            {
                return;
            }
            if (next != sequence + 1)
            {
                Invalidate("推送序号中断，请重新查询。");
                return;
            }
            sequence = next;
            if (batch.Kind == "HEARTBEAT")
            {
                return;
            }
            try
            {
                ViewDelta delta = viewChanges.FirstOrDefault(view => view.ViewId == viewId);
                if (delta == null)
                {
                    return;
                }
                TotalCount = delta.TotalCount.Value;
                var incoming = changedRecords
                    .Where(row => domain != "price" || (string)row["viewId"] == viewId)
                    .ToDictionary(row => (string)row[idField]);
                List<JObject> rows = Rows.ToList();
                var removals = new HashSet<string>(delta.RemoveIds ?? new List<string>());
                if (removals.Count > 0)
                {
                    Removed?.Invoke(removals.ToArray());
                }
                if (IsBrowsing)
                {
                    changedIds.UnionWith(removals);
                    changedIds.UnionWith(delta.UpsertIds ?? new List<string>());
                }
                bool refill = rows.RemoveAll(row => removals.Contains((string)row[idField])) > 0;
                foreach (string id in delta.UpsertIds ?? new List<string>())
                {
                    int index = rows.FindIndex(existing => (string)existing[idField] == id);
                    if ((PageNumber != 1 || IsBrowsing) && index < 0)
                    {
                        continue;
                    }
                    JObject row = incoming[id];
                    if (index >= 0)
                    {
                        refill |= Timestamp(row) < Timestamp(rows[index]);
                        rows[index] = row;
                    }
                    else
                    {
                        rows.Add(row);
                    }
                }
                if (PageNumber == 1 && !IsBrowsing)
                {
                    rows = rows.OrderByDescending(Timestamp).ThenBy(row => (string)row[idField], StringComparer.Ordinal)
                        .Take(PageSize).ToList();
                }
                SetRows(rows);
                Changed?.Invoke();
                if (refill && PageNumber == 1 && !IsBrowsing)
                {
                    RefillAsync();
                }
                else
                {
                    Updated?.Invoke();
                }
            }
            catch (Exception error)
            {
                Invalidate(error.Message);
            }
        }

        private async void RefillAsync()
        {
            try
            {
                if (!loading && !disposed)
                {
                    await QueryAsync(1);
                    Updated?.Invoke();
                }
            }
            catch (Exception error)
            {
                Invalidate(error.Message);
            }
        }

        private static DateTimeOffset Timestamp(JObject row)
        {
            return DateTimeOffset.Parse(row["updateTime"].ToString(), CultureInfo.InvariantCulture);
        }

        private void SetRows(IEnumerable<JObject> rows)
        {
            JObject[] result = rows.ToArray();
            for (int index = 0; index < result.Length; index++)
            {
                if (index < Rows.Count)
                {
                    Rows[index] = result[index];
                }
                else
                {
                    Rows.Add(result[index]);
                }
            }
            while (Rows.Count > result.Length)
            {
                Rows.RemoveAt(Rows.Count - 1);
            }
        }

        private void ConnectionChanged()
        {
            Invalidate("连接状态已变化，请重新查询以恢复订阅。");
        }

        private void Invalidate(string message)
        {
            stale = true;
            Failed?.Invoke(message);
        }

        public async Task CloseAsync()
        {
            subscription?.Dispose();
            subscription = null;
            string previousId = viewId;
            viewId = null;
            stale = true;
            if (previousId != null)
            {
                await protocol.CloseAsync(previousId);
            }
        }

        public void Dispose()
        {
            disposed = true;
            stale = true;
            subscription?.Dispose();
            subscription = null;
            transport.ConnectionChanged -= ConnectionChanged;
        }
    }
}
