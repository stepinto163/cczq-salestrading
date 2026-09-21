using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ficc.SalesTrading;

namespace Ficc.FrontendLearning
{
    // 所有方法在 UI 线程调用。DPS 回调通过 BeginInvoke 转到此线程。
    // 仅缓存当前页；报价、行情和提醒各用独立 stream/topic/model。
    public sealed class SubscribedPage
    {
        private readonly DpsClient client;
        private readonly QuoteExamples api;
        private readonly List<JObject> buffered = new List<JObject>();
        private readonly List<string> ids = new List<string>();
        private readonly Dictionary<string, JObject> records = new Dictionary<string, JObject>();
        private string domain;
        private string collection;
        private string idField;
        private string streamId;
        private string viewId;
        private long sequence;
        private bool loading;
        private bool stale;
        private bool overflow;
        public string Topic { get; private set; }
        public string Domain => domain;
        public int PageNumber { get; private set; } = 1;
        public int TotalCount { get; private set; }
        public const int PageSize = 20;
        public event Action Changed;
        public event Action<string> Failed;
        public IEnumerable<JObject> Rows => ids.Select(id => records[id]);

        public SubscribedPage(DpsClient client, QuoteExamples api)
        {
            this.client = client;
            this.api = api;
        }

        public Task Open(string selectedDomain, string securityId)
        {
            return Open(selectedDomain, QuoteExamples.BondFilter(securityId));
        }

        public async Task Open(string selectedDomain, JObject filter)
        {
            await Close();
            domain = selectedDomain;
            collection = domain == "quote" ? "quotes" : domain == "price" ? "books" : "notifications";
            idField = domain == "quote" ? "quoteId" : domain == "price" ? "securityId" : "notificationId";
            var stream = await api.Request("ficc." + domain + ".stream.open", new JObject());
            streamId = (string)stream["streamId"];
            Topic = (string)stream["pushTopic"]; // 使用服务端返回值，不拼接 sessionId。
            viewId = "LEARN-" + Guid.NewGuid().ToString("N");
            var request = new JObject
            {
                ["viewId"] = viewId,
                ["pageSize"] = PageSize,
                ["filter"] = filter.DeepClone()
            };
            if (domain == "quote")
            {
                request["ownerScope"] = "MINE";
                request["status"] = new JArray("PENDING_CONFIRM", "ACTIVE", "REFERRED", "DELETED");
            }
            if (domain == "price") request["mode"] = "BEST";
            if (domain == "notification") request["readState"] = "ALL";

            await Load(async () =>
            {
                // 先订阅，再请求初始页；期间缓存 PUSH，消除初始化窗口中的丢消息风险。
                await Task.Run(() => client.Subscribe(Topic, false));
                return await api.Request("ficc." + domain + ".view.open", request);
            });
        }

        public Task QueryPage(int pageNumber)
        {
            if (viewId == null || pageNumber < 1 || loading)
                throw new InvalidOperationException("请先打开视图，等待请求完成。");
            return Load(() => api.Request("ficc." + domain + ".view.query", new JObject
            {
                ["viewId"] = viewId,
                ["pageNumber"] = pageNumber,
                ["pageSize"] = PageSize
            }));
        }

        private async Task Load(Func<Task<JObject>> request)
        {
            loading = true;
            overflow = false;
            buffered.Clear();
            try
            {
                Install(await request());
                if (overflow) throw new InvalidOperationException("初始化期间消息过多，请重新打开视图。");
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
            }
            var pending = buffered.ToArray();
            buffered.Clear();
            foreach (var message in pending) Receive(Topic, message);
            Changed?.Invoke();
        }

        private void Install(JObject reply)
        {
            var view = (JObject)reply["view"];
            if ((string)reply["streamId"] != streamId || (string)view["viewId"] != viewId)
                throw new InvalidOperationException("视图回复与当前请求不匹配。");
            records.Clear();
            foreach (JObject row in (JArray)reply[collection]) records[(string)row[idField]] = row;
            ids.Clear();
            ids.AddRange(((JArray)view["ids"]).Values<string>());
            PageNumber = (int)view["pageNumber"];
            TotalCount = (int)view["totalCount"];
            sequence = (long)reply["sequence"];
        }

        public void Receive(string topic, JObject message)
        {
            if (topic != Topic || (string)message["streamId"] != streamId) return;
            if (loading)
            {
                if (buffered.Count < 200) buffered.Add(message);
                else overflow = true;
                return;
            }
            if (stale) return;
            long next = (long)message["sequence"];
            if (next <= sequence) return; // 已包含在初始页中的消息，不重复应用。
            if (next != sequence + 1 || (string)message["kind"] == "RESET_REQUIRED")
            {
                stale = true;
                Failed?.Invoke("推送序号不连续；停止应用更新，请重新打开视图。");
                return;
            }
            sequence = next; // HEARTBEAT 也占序号，只是不更新表格。
            if ((string)message["kind"] == "HEARTBEAT") return;
            try
            {
                bool refill = ClientViewPage.Apply(message, viewId, collection, idField,
                    PageNumber, PageSize, ids, records);
                foreach (JObject view in (JArray)message["views"] ?? new JArray())
                    if ((string)view["viewId"] == viewId) TotalCount = (int)view["totalCount"];
                Changed?.Invoke();
                if (refill) RefillFirstPage();
            }
            catch (Exception error)
            {
                stale = true;
                Failed?.Invoke(error.ToString());
            }
        }

        private async void RefillFirstPage()
        {
            // 当前页删除或退出筛选后补齐第一页；已有请求时不并发启动。
            if (loading) return;
            try { await QueryPage(1); }
            catch (Exception error) { Failed?.Invoke(error.ToString()); }
        }

        public async Task Close()
        {
            if (loading) throw new InvalidOperationException("视图正在加载，请稍后重试。");
            string oldTopic = Topic;
            string oldViewId = viewId;
            Topic = null;
            viewId = null;
            stale = true;
            if (oldTopic != null) await Task.Run(() => client.Unsubscribe(oldTopic));
            if (oldViewId != null)
                await api.Request("ficc." + domain + ".view.close", new JObject { ["viewId"] = oldViewId });
        }
    }
}
