using Ats.Foundation.Message;
using STC.Common.Model.Fix.Message;
using STC.XBond.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ServerApi.TBook
{
    public interface ITBookCEDAServer
    {
        event Action<ExecutionReport> OnOrder;

        void UnsubscribAll();

        void SubscribeOrder();

        List<Bond> GetBond();

        TBookCancelReplyPo CancelOrder(TBookCancelPo cancelPo);

        PResult PlaceOrder(NewOrderSingle newOrder);
    }
}
