using netapi.Api;
using STC.SalesTrading.Plugin.Abstractions.Gateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// STC ApiManager 适配器。业务层只看到 IGatewayClient，后续可替换为其他传输实现。
    /// ApiManager 的 Request 是同步 API，因此异步入口在线程池执行并响应取消。
    /// </summary>
    public sealed class ApiManagerGatewayClient : IGatewayClient
    {
        private readonly IApiManager _apiManager;
        private readonly Func<IApi> _apiFactory;

        public ApiManagerGatewayClient()
            : this(ApiManager.Instance)
        {
        }

        public ApiManagerGatewayClient(IApiManager apiManager)
        {
            _apiManager = apiManager ?? throw new ArgumentNullException(nameof(apiManager));
            _apiFactory = () => _apiManager.GetDefaultApi()
                ?? throw new InvalidOperationException("STC 默认网关尚未初始化");
        }

        public string CurrentUserName
        {
            get
            {
                try { return _apiFactory().UserName; }
                catch { return string.Empty; }
            }
        }

        public GatewayResponse<T> Request<T>(GatewayRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            try
            {
                var api = _apiFactory();
                var data = api.Request<T>(request.ServiceId, request.Topic, request.Payload,
                    request.Timeout.HasValue ? (int)request.Timeout.Value.TotalMilliseconds : api.TimeOut);
                return GatewayResponse<T>.Ok(data);
            }
            catch (Exception ex)
            {
                return GatewayResponse<T>.Fail("gateway.request_failed", ex.Message, ex);
            }
        }

        public async Task<GatewayResponse<T>> RequestAsync<T>(
            GatewayRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            cancellationToken.ThrowIfCancellationRequested();
            var task = Task.Run(() => Request<T>(request), cancellationToken);
            return await task.ConfigureAwait(false);
        }

        public IDisposable Subscribe(GatewaySubscription subscription, Action<GatewayMessage> handler)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var api = _apiFactory();
            var topics = (subscription.Topics ?? Array.Empty<string>()).ToArray();
            var serviceId = subscription.ServiceId;
            // netapi（GWApi）按 topic 记账：同 topic 重复订阅不挂新回调、UnSubScribe 直接按 topic 撤——
            // 多 tab 各自 stream.open 会拿到同一 pushTopic，直接透传会互踩（一个 tab 退订撤掉另一个的推送；
            // 第二个 tab 的回调甚至不会被挂上）。故在客户端做按 topic 的引用计数 + 分发：
            // 首个订阅者执行 netapi 订阅并挂单一分发回调，后续订阅者只挂 handler，最后一个退出才真正退订。
            var key = serviceId + "\u0001" + string.Join("\u0001", topics);

            TopicDispatch dispatch;
            bool needSubscribe;
            lock (TopicGate)
            {
                if (!TopicDispatches.TryGetValue(key, out dispatch))
                {
                    dispatch = new TopicDispatch { ServiceId = serviceId, Topics = topics };
                    TopicDispatches[key] = dispatch;
                }
                dispatch.Handlers.Add(handler);
                needSubscribe = !dispatch.NetSubscribed;
                dispatch.NetSubscribed = true;
            }

            if (needSubscribe)
            {
                Action<object> callback = payload =>
                {
                    List<Action<GatewayMessage>> snapshot;
                    lock (TopicGate) snapshot = dispatch.Handlers.ToList();
                    var message = new GatewayMessage
                    {
                        ServiceId = dispatch.ServiceId,
                        Topics = dispatch.Topics,
                        Payload = payload,
                        ReceivedAt = DateTime.Now,
                    };
                    foreach (var target in snapshot)
                    {
                        try { target(message); }
                        catch { /* 单个订阅方异常不影响其余 */ }
                    }
                };
                try
                {
                    if (subscription.IncludeImage)
                        api.SubScribeWithImage(callback, topics, serviceId, subscription.SignalId ?? string.Empty);
                    else
                        api.SubScribe(callback, topics, serviceId, subscription.SignalId ?? string.Empty);
                }
                catch
                {
                    lock (TopicGate)
                    {
                        dispatch.NetSubscribed = false;
                        dispatch.Handlers.Remove(handler);
                        if (dispatch.Handlers.Count == 0) TopicDispatches.Remove(key);
                    }
                    throw;
                }
            }

            return new DelegateSubscription(() =>
            {
                bool needUnsubscribe;
                lock (TopicGate)
                {
                    dispatch.Handlers.Remove(handler);
                    needUnsubscribe = dispatch.Handlers.Count == 0;
                    if (needUnsubscribe)
                    {
                        dispatch.NetSubscribed = false;
                        TopicDispatches.Remove(key);
                    }
                }
                // 真正的 netapi 退订放锁外（网络调用不持静态锁）；失败仅意味着服务端按连接回收
                if (needUnsubscribe)
                {
                    try { api.UnSubScribe(dispatch.ServiceId, dispatch.Topics); }
                    catch { }
                }
            });
        }

        /// <summary>按 (serviceId, topics) 的进程级订阅登记（多 tab 共享同一 pushTopic 的引用计数分发）。</summary>
        private static readonly object TopicGate = new object();

        private static readonly Dictionary<string, TopicDispatch> TopicDispatches =
            new Dictionary<string, TopicDispatch>();

        private sealed class TopicDispatch
        {
            public string ServiceId;
            public string[] Topics;
            public bool NetSubscribed;
            public readonly List<Action<GatewayMessage>> Handlers = new List<Action<GatewayMessage>>();
        }

        public IDisposable SubscribeConnectionState(Action<GatewayConnectionChanged> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var api = _apiFactory();
            Action<object> callback = state => handler(new GatewayConnectionChanged
            {
                RawState = state,
                ReceivedAt = DateTime.Now,
            });
            api.SubScribeConStatus(callback);
            return new DelegateSubscription(() =>
            {
                try { api.UnSubScribeConStatus(callback); }
                catch { }
            });
        }

        private sealed class DelegateSubscription : IDisposable
        {
            private Action _dispose;
            public DelegateSubscription(Action dispose) { _dispose = dispose; }
            public void Dispose()
            {
                var action = Interlocked.Exchange(ref _dispose, null);
                if (action != null) action();
            }
        }
    }
}
