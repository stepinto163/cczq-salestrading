using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using STC.Common.Model.Fix.Message;
using STC.Common.Api;

namespace STC.XBond.Plugin.Handler.Price
{
    public class MDHandler : IMDHandler
    {
        public event Action<MarketDataSnapshotFullRefresh> OnMDPrice;

        private ConcurrentDictionary<string, MarketDataSnapshotFullRefresh> dic = new ConcurrentDictionary<string, MarketDataSnapshotFullRefresh>();


        private MDHandler() { }
        private static IMDHandler server = new MDHandler();


        public static IMDHandler Instance { get { return server; } }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            ClientApi.Instance.OnMDPrice += Instance_OnPrice;
            
        }

        public void SubscribeMDPriceBySymbol(string symbol)
        {
            ClientApi.Instance.SubscribeMDPrice(symbol);
        }

        private void Instance_OnPrice(MarketDataSnapshotFullRefresh price)
        {
            string priceKey = string.Format("{0}-{1}", price.Symbol, price.VenueTypeGW);

            dic.AddOrUpdate(priceKey, price, (key, value) => price);
            
            OnMDPrice?.Invoke(price);
        }
    }
}
