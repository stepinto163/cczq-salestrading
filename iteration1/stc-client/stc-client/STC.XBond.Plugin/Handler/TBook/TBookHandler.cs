using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using STC.Common.Model.Fix.Message;
using STC.Common.Api;
using STC.Common.Model.TBook;

namespace STC.XBond.Plugin.Handler.TBook
{
    /// <summary>
    /// 我的订单
    /// </summary>
    public class TBookHandler : ITBookHandler
    {
        private ConcurrentDictionary<string, ExecutionReport> dic = new ConcurrentDictionary<string, ExecutionReport>();

        private TBookHandler() { }
        private static ITBookHandler server = new TBookHandler();


        public static ITBookHandler Instance { get { return server; } }

        public event Action<ExecutionReport> OnOrder;
        public event Action<List<Bond>> OnBondChange;

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            ClientApi.Instance.OnTBookOrder += Instance_OnOrder;

            ClientApi.Instance.OnBond += Instance_OnBond;

            //订阅我的订单
            ClientApi.Instance.SubscribeTBookOrder();

            ClientApi.Instance.SubsribeBond();
        }

        private void Instance_OnBond(List<Bond> bonds)
        {
            OnBondChange?.Invoke(bonds);
        }

        private void Instance_OnOrder(ExecutionReport order)
        {
            lock (string.Intern(order.OrderID))
            {
                if (order.OrdStatus.Equals("0"))
                    dic.AddOrUpdate(order.OrderID, order, (key, value) => order);
                else
                {
                    ExecutionReport tem = null;
                    dic.TryRemove(order.OrderID, out tem);
                }


                OnOrder?.Invoke(order);
            }
        }

        /// <summary>
        /// 获取xbond的债券传ODM
        /// </summary>
        /// <param name="venue"></param>
        /// <returns></returns>
        //public List<Bond> GetBonodByVenue(string venue)
        //{
        //    List<Bond> bonds = TBookCEDAServer.Instance.GetBond();

        //    return bonds.Where(delegate (Bond bond)
        //    {
        //        if (bond.venues.ToLower().Equals(venue.ToLower()))
        //            return true;
        //        else
        //            return false;
        //    }).ToList();
        //}

        public ExecutionReport GetOrderBySymbol(string symbol)
        {
            List<ExecutionReport> result = dic.Values.Where(delegate (ExecutionReport order)
            {
                if (order.Symbol.Equals(symbol))
                    return true;
                else
                    return false;
            }).ToList();


            return result != null && result.Count > 0 ? result[0] : null;
        }

        public TBookCancelReplyPo CancelOrder(TBookCancelPo cancelPo)
        {
            return ClientApi.Instance.CancelOrder(cancelPo);
        }

        public PResult PlaceOrder(NewOrderSingle newOrder)
        {
            return ClientApi.Instance.PlaceOrder(newOrder);
        }

        public PResult OpenCloseBond(BondOperationPo request)
        {
            return ClientApi.Instance.OpenCloseBond(request);
        }
    }
}
