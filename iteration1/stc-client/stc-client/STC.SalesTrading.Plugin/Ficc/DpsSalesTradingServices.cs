using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using QuoteEvent = STC.SalesTrading.Plugin.Abstractions.Models.QuoteEvent;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Ficc
{
    public interface ISalesTradingUpdates
    {
        event Action QuotesChanged;
        event Action NotificationsChanged;
    }

    // Composition boundary: the product ViewModels keep their typed contracts; DPS stays here.
    public sealed partial class DpsSalesTradingServices : IQuoteService, IQuoteMgmtService,
        IMarketCenterService, IProductFilterCatalog, ISalesTradingUpdates, IDisposable
    {
        private Task<DictionaryData> filterDictionaryTask;

        public Task<DictionaryData> GetFilterDictionaryAsync()
        {
            if (filterDictionaryTask == null || filterDictionaryTask.IsFaulted || filterDictionaryTask.IsCanceled)
            {
                filterDictionaryTask = Api.RequestAsync(SalesTradingApi.QuoteDictionaryQuery, new DictionaryRequest());
            }
            return filterDictionaryTask;
        }

        private readonly StcFiccTransport transport;
        private readonly Dictionary<string, QuerySession> sessions = new Dictionary<string, QuerySession>();
        private readonly Dictionary<string, JObject> securities = new Dictionary<string, JObject>();
        private readonly Dictionary<string, JObject> displayedQuotes = new Dictionary<string, JObject>();
        private event Action<QuoteEvent> priceChanged;
        private long priceChangeNumber;
        private bool disposed;
        public event Action QuotesChanged;
        public event Action NotificationsChanged;
        public FiccApiClient Api { get; }
        public string UserName { get; }

        public DpsSalesTradingServices()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            LogComponent("STARTING", stopwatch.ElapsedMilliseconds);
            try
            {
                IApi hostApi = ApiManager.Instance.GetDefaultApi();
                if (hostApi == null)
                {
                    throw new InvalidOperationException("STC登录连接尚未建立。");
                }
                UserName = hostApi.UserName;
                transport = new StcFiccTransport(hostApi, SynchronizationContext.Current
                    ?? new DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                Api = new FiccApiClient(transport);
                LogComponent("INITIALIZED", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception error)
            {
                LogComponent("FAILED", stopwatch.ElapsedMilliseconds);
                LogHelper.Error(error);
                throw;
            }
        }

        private static void LogComponent(string state, long elapsedMillis)
        {
            string message = "COMPONENT " + new JObject
            {
                ["service"] = "STCClient",
                ["component"] = "SALES_TRADING_ADAPTER",
                ["state"] = state,
                ["mode"] = "dps",
                ["implementation"] = nameof(DpsSalesTradingServices),
                ["target"] = "QuoteSvr/NotificationSvr via authenticated GW",
                ["elapsedMillis"] = elapsedMillis
            }.ToString(Formatting.None);
            if (state == "FAILED")
            {
                LogHelper.Error(message);
            }
            else
            {
                LogHelper.Info(message);
            }
        }

        private async Task<LivePage> QueryAsync(string key, string domain, JObject filter, CancellationToken cancellation,
            int pageNumber = 1, int pageSize = 100, string mode = "BEST", string market = null, JArray statuses = null)
        {
            cancellation.ThrowIfCancellationRequested();
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(DpsSalesTradingServices));
            }
            if (!sessions.TryGetValue(key, out QuerySession session))
            {
                LivePage page = new LivePage(Api, transport, domain);
                session = new QuerySession(page);
                sessions.Add(key, session);
                page.Updated += () => NotifyDomain(domain);
                page.Failed += error => LogHelper.Error("Sales trading stream unavailable: " + domain + ": " + error);
            }
            await session.Gate.WaitAsync(cancellation);
            try
            {
                cancellation.ThrowIfCancellationRequested();
                int boundedSize = Math.Max(1, Math.Min(pageSize, 100));
                string signature = new JObject
                {
                    ["filter"] = filter, ["mode"] = mode, ["market"] = market,
                    ["statuses"] = statuses, ["pageSize"] = boundedSize
                }.ToString(Formatting.None);
                LivePage page = session.Page;
                await page.WaitForIdleAsync();
                cancellation.ThrowIfCancellationRequested();
                if (session.Signature != signature || !page.IsReady)
                {
                    page.PriceMode = mode;
                    page.PageSize = boundedSize;
                    page.QuoteStatuses = statuses ?? new JArray("ACTIVE", "REFERRED");
                    await page.OpenAsync(filter, key == "pending");
                    session.Signature = signature;
                }
                if (page.PageNumber != pageNumber)
                {
                    await page.QueryAsync(pageNumber);
                }
                cancellation.ThrowIfCancellationRequested();
                foreach (JObject quote in page.Rows.Where(row => row["quoteId"] != null))
                {
                    displayedQuotes[(string)quote["quoteId"]] = (JObject)quote.DeepClone();
                }
                await LoadSecuritiesAsync(page.Rows.Select(row => (string)row["securityId"]), cancellation);
                return page;
            }
            finally
            {
                session.Gate.Release();
            }
        }

        private void NotifyDomain(string domain)
        {
            if (disposed)
            {
                return;
            }
            if (domain == "quote")
            {
                QuotesChanged?.Invoke();
            }
            else if (domain == "notification")
            {
                NotificationsChanged?.Invoke();
            }
            else
            {
                priceChanged?.Invoke(new QuoteEvent { Kind = QuoteEventKind.Edit, StateRevision = ++priceChangeNumber });
            }
        }

        private async Task LoadSecuritiesAsync(IEnumerable<string> ids, CancellationToken cancellation)
        {
            string[] missing = ids.Where(id => !string.IsNullOrEmpty(id) && !securities.ContainsKey(id)).Distinct().Take(100).ToArray();
            if (missing.Length == 0)
            {
                return;
            }
            BaseData result = await Api.RequestAsync(SalesTradingApi.BaseGet, new BaseGetRequest
            {
                Kind = "SECURITY",
                Ids = missing.ToList()
            });
            cancellation.ThrowIfCancellationRequested();
            foreach (ReferenceRecord security in result.Records)
            {
                securities[security.SecurityId] = security.ToJson();
            }
        }

        private JObject WithReference(JObject quote)
        {
            JObject result = (JObject)quote.DeepClone();
            if (securities.TryGetValue((string)quote["securityId"] ?? "", out JObject security))
            {
                foreach (JProperty property in security.Properties())
                {
                    if (result[property.Name] == null)
                    {
                        result[property.Name] = property.Value.DeepClone();
                    }
                }
            }
            return result;
        }

        public void SubscribeQuoteEvents(Action<QuoteEvent> handler)
        {
            priceChanged += handler;
        }

        public void UnsubscribeQuoteEvents(Action<QuoteEvent> handler)
        {
            priceChanged -= handler;
        }

        public void Dispose()
        {
            disposed = true;
            foreach (QuerySession session in sessions.Values)
            {
                session.Page.Dispose();
            }
            transport.Dispose();
            sessions.Clear();
            displayedQuotes.Clear();
            securities.Clear();
        }

        private sealed class QuerySession
        {
            public LivePage Page { get; }
            public SemaphoreSlim Gate { get; } = new SemaphoreSlim(1, 1);
            public string Signature { get; set; }

            public QuerySession(LivePage page)
            {
                Page = page;
            }
        }
    }
}
