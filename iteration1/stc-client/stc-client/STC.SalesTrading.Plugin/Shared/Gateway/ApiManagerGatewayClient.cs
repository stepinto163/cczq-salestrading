using netapi.Api;
using STC.SalesTrading.Plugin.Abstractions.Gateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Shared.Gateway
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
            Action<object> callback = payload => handler(new GatewayMessage
            {
                ServiceId = subscription.ServiceId,
                Topics = topics,
                Payload = payload,
                ReceivedAt = DateTime.Now,
            });

            if (subscription.IncludeImage)
                api.SubScribeWithImage(callback, topics, subscription.ServiceId, subscription.SignalId ?? string.Empty);
            else
                api.SubScribe(callback, topics, subscription.ServiceId, subscription.SignalId ?? string.Empty);

            return new DelegateSubscription(() =>
            {
                try { api.UnSubScribe(subscription.ServiceId, topics); }
                catch { }
            });
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
