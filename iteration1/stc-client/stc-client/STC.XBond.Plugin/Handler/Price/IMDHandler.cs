using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Handler.Price
{
    public interface IMDHandler
    {
        event Action<MarketDataSnapshotFullRefresh> OnMDPrice;

        void Init();

        void Clear();

        void SubscribeMDPriceBySymbol(string symbol);
    }
}
