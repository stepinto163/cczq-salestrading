using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ServerApi.MD
{
    public interface IMDCEDAServer
    {
        event Action<MarketDataSnapshotFullRefresh> OnPrice;

        void UnsubscribAll(List<string> synbols);

        void SubscribeMDPrice(string symbol);

        
    }
}
