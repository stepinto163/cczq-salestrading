using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Handler.TBook
{
    public interface ITBookHandler
    {
        event Action<ExecutionReport> OnOrder;
        event Action<List<Bond>> OnBondChange;

        void Init();

        void Clear();

        /// <summary>
        /// 获取xbond的债券传ODM
        /// </summary>
        /// <param name="venue"></param>
        /// <returns></returns>
        //List<Bond> GetBonodByVenue(string venue);

        ExecutionReport GetOrderBySymbol(string symbol);

        TBookCancelReplyPo CancelOrder(TBookCancelPo cancelPo);

        PResult PlaceOrder(NewOrderSingle newOrder);

        PResult OpenCloseBond(BondOperationPo request);
    }
}
