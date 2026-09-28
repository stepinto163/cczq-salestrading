using System;
using System.Threading;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Abstractions.Gateway
{
    public interface IGatewayClient
    {
        GatewayResponse<T> Request<T>(GatewayRequest request);
        Task<GatewayResponse<T>> RequestAsync<T>(GatewayRequest request, CancellationToken cancellationToken);
        IDisposable Subscribe(GatewaySubscription subscription, Action<GatewayMessage> handler);
        IDisposable SubscribeConnectionState(Action<GatewayConnectionChanged> handler);
        string CurrentUserName { get; }
    }
}
