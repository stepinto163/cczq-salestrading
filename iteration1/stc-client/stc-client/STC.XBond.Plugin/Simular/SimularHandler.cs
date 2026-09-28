using STC.Common.Handler.TBook;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Simular
{
    /// <summary>
    /// 测试时使用， 模拟下单
    /// </summary>
    public class SimularHandler
    {
        public static void SimularPlaceOrder()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(5000);

                        NewOrderSingle newOrder = new NewOrderSingle();
                        newOrder.AlgoName = "STP";
                        newOrder.VenueTypeGW = "SOR";
                        newOrder.Venues = "TPSC";
                        newOrder.MarketIndicator = "4";
                        newOrder.UserName = "rjq";

                        newOrder.ClOrdID = "c" + DateTime.Now.Ticks;
                        newOrder.SecurityID = "180210.IB";
                        //newOrder.ValidUntilTime = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");
                        newOrder.Side = "1";
                        newOrder.Price = "4.2";
                        newOrder.OrdType = "2";
                        newOrder.OrderQty = "1000";
                        newOrder.TimeInForce = "0";
                        newOrder.SettlType = "2";
                        newOrder.Terminal = "client demo";

                        TBookHandler.Instance.PlaceOrder(newOrder);

                        Console.WriteLine("SimularPlaceOrder data:{0}", newOrder);
                    }
                    catch (Exception e)
                    {

                    }
                }

            });
        }
    }
}
