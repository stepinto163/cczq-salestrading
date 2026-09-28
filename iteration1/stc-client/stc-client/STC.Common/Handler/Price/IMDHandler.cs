using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Price
{
    public interface IMDHandler
    {
        void ClearCache();
        void Init(string symbol, Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback);
        void Init(List<string> symbol, List<string> hoSymbol, Action<string, MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback);
        void UnSubscribeAll(bool isSubscribe);

        void UnSubscribe(string symbol, Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback);

        void UnSubscribe(Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback);

        void UnSubscribe(Action<string, MarketDataSnapshotFullRefresh> onMDCTPriceCallback, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback);

        MarketDataSnapshotFullRefresh GetCachePriceByTopic(string topic);

        void SubscribeBest(Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback);

        void SubscribeFull(string symbol, Action<MarketDataSnapshotFullRefresh> onMDPriceCallback);

        MarketDataSnapshotFullRefresh GetCacheBestPriceBySecurity(string security);

        string GetCtTopic(string symbol);

        void SubscribeCT(string symbol, Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback);

        void UnsubscribeCT(string symbol, Action<MarketDataSnapshotFullRefresh> onMDCTPriceCallback);

        void SubscribeBest(string symbol, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback);

        void UnsubscribeBest(string symbol, Action<MarketDataSnapshotFullRefresh> onMDBestPriceCallback);

        void SubscribeFull(string symbol, string hoSymbol, Action<string, MarketDataSnapshotFullRefresh> onMDPriceCallback);

        void CefSubScribeWithImage(string[] topic);
    }
}
