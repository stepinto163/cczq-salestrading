
using com.gateway.log;
using BDB.Trade.Plugin.Handler.DataProcesser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STC.Common.Model.Fix.Message;
using STC.Common.Handler.Base;
using STC.Common.Handler.TBook;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.Model;
using STC.Common.Model.TBook;
using OrderCancelRequest = STC.Common.Model.TBook.OrderCancelRequest;

namespace BDB.Trade.Plugin.Handler
{
    public class OrdersHandler
    {
        // 订阅我的订单
        private readonly Object m_OnOwnOrdersEventLock = new Object();
        private Action<ExecutionReport> _OnOwnOrders;
        public event Action<ExecutionReport> OnOwnOrders
        {
            add
            {
                lock (m_OnOwnOrdersEventLock)
                {
                    _OnOwnOrders += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheOwnOrders)
                    {
                        _OnOwnOrders?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnOwnOrdersEventLock)
                {
                    _OnOwnOrders -= value;
                }
            }
        }

        /// <summary>
        /// 缓存我的订单(订阅)
        /// </summary>
        private ConcurrentDictionary<string, ExecutionReport> _CacheOwnOrders = new ConcurrentDictionary<string, ExecutionReport>();

        private OrdersHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        private volatile bool isInit = false;
        
        public void init()
        {
            if (isInit ==  false)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        TBookHandler.Instance.Init(Instance_OnOrder, null);
                        // IMHandler.Instance.Init(Instance_OnQQ);
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("RegisterCTPrice", e);
                    }
                });
                RequestOrders();

                isInit = true;
            }
        }
        //private void Instance_OnQQ(List<QQUserConfigPo> obj)
        //{
        //    LogHelper.Info("RegisterCTPrice");
        //}

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info("OrdersHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                _CacheOwnOrders.Clear();
                isInit = false;

                MyOrderProcesser.Processer.ResetClear();

                init();
            }
        }

        private void Instance_OnOrder(ExecutionReport obj)
        {
            lock (string.Intern(obj.OrderID))
            {
                _CacheOwnOrders.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                // 6: 撤销中； 如果是撤销中，那就暂不处理 
                // 如果是 TimeInForce = 3 IOC Point 则回掉
                //if (obj.TimeInForce.ToString() != "3")
                //{
                    if (obj.ExecType != "6" && obj.ExecType != "0" && obj.ExecType != "N" && obj.ExecType != "S" && _modifyOrderReplyDic.ContainsKey(obj.OrderID))
                    {
                        Func<ExecutionReport, ExecutionReport> func = TakeAWhile;
                        func.BeginInvoke(obj, TakesAWhileCallBack, obj);
                    }
                //}
                //else
                //{
                //    Func<ExecutionReport, ExecutionReport> func = TakeAWhile;
                //    func.BeginInvoke(obj, TakesAWhileCallBack, obj);
                //}
            }

            //UpdateLstOrderDic(obj);

            _OnOwnOrders?.Invoke(obj);
        }

        public void TestOnOrder(ExecutionReport obj)
        {
            this.Instance_OnOrder(obj);
        }

        private static ExecutionReport TakeAWhile(ExecutionReport obj)
        {
            return obj;
        }

        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="fun">调用的委托</param>
         private void TakesAWhileCallBack(IAsyncResult fun)
        {
            try
            {
                if (fun == null)
                {
                    LogHelper.Info("fun is null");
                    return;
                }

                AsyncResult ar = (AsyncResult)fun;
                Func<ExecutionReport, ExecutionReport> dl = (Func<ExecutionReport, ExecutionReport>)ar.AsyncDelegate;
                //等待函数执行完毕
                ExecutionReport result = dl.EndInvoke(fun);

                if (result.ExecType == "4")
                {
                    NewOrderSingle modifyOrderSingle1;
                    _modifyOrderDic.TryRemove(result.OrderID, out modifyOrderSingle1);
                    modifyOrderSingle1.MacID = EngineHelper.GetMacByNetworkInterface();
                    modifyOrderSingle1.InsideIP = EngineHelper.GetIp();
                    modifyOrderSingle1.HardDiskID = EngineHelper.Getharddiskid2();
                    NewOrderSingle copyOrder = modifyOrderSingle1.Clone();
                    copyOrder.StrategyOrderId = string.Empty;
                    PResult newOrderResult = PlaceOrder(copyOrder);
                    if (newOrderResult == null || !newOrderResult.IsSuccess())
                    {
                        Action<ModifyOrdersReply> cbAction;
                        if (_modifyOrderReplyDic.TryRemove(result.OrderID, out cbAction))
                        {
                            ModifyOrdersReply modifyOrderReply = new ModifyOrdersReply();
                            modifyOrderReply.Code = newOrderResult == null ? "0002" : newOrderResult.Code;
                            modifyOrderReply.Msg = newOrderResult == null ? "修改订单失败， 并且原订单已撤" : newOrderResult.Msg;
                            modifyOrderReply.OrderID = result.OrderID;
                            cbAction?.Invoke(modifyOrderReply);
                        }
                    }
                    else
                    {
                        Action<ModifyOrdersReply> cbAction;
                        if (_modifyOrderReplyDic.TryGetValue(result.OrderID, out cbAction))
                        {
                            ModifyOrdersReply modifyOrderReply = new ModifyOrdersReply();
                            modifyOrderReply.Code = newOrderResult.Code;//"AAAA";
                            modifyOrderReply.Msg = "";
                            modifyOrderReply.OrderID = result.OrderID;
                            cbAction?.Invoke(modifyOrderReply);
                        }
                    }
                }
                else
                {
                    Action<ModifyOrdersReply> cbAction;
                    if (_modifyOrderReplyDic.TryRemove(result.OrderID, out cbAction))
                    {
                        ModifyOrdersReply modifyOrderReply = new ModifyOrdersReply();
                        modifyOrderReply.Code = "0003";
                        switch (result.ExecType)
                        {
                            case "1":
                                modifyOrderReply.Msg = "订单状态为部分成交，无法修改订单";
                                break;
                            case "2":
                                modifyOrderReply.Msg = "订单状态为完全成交，无法修改订单";
                                break;
                            default:
                                modifyOrderReply.Msg = $"订单状态为{result.ExecType}，无法修改订单";
                                break;
                        }

                        modifyOrderReply.OrderID = result.OrderID;
                        cbAction?.Invoke(modifyOrderReply);
                    }
                    NewOrderSingle modifyOrderSingle1;
                    _modifyOrderDic.TryRemove(result.OrderID, out modifyOrderSingle1);
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        public ExecutionReport GetCacheOrders(string securityID, string settlType, string venues, string venueTypeGw)
        {
            var searchResult = _CacheOwnOrders.Values.Where(p => p.SecurityID == securityID
            && p.SettlType == settlType
            && p.Venues == venues
            && p.VenueTypeGW == venueTypeGw);
            if (searchResult.Count() > 0)
            {
                return searchResult.First();
            }
            return null;
        }

        private static OrdersHandler server = new OrdersHandler();
        public static OrdersHandler Instance { get { return server; } }

        public void RequestOrders()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.dealTimeBegin = today;
                    request.dealTimeEnd = today;
                    request.userName = LocalEnv.UserInfo.UserName;
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    //request.orderStatus = "N,0,1";
                    int index = 0;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = TBookHandler.Instance.QueryOrderForTBook(request);

                        if (reply == null || reply.datas == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            foreach (ExecutionReport obj in reply.datas)
                            {
                                lock (string.Intern(obj.OrderID))
                                {
                                    _CacheOwnOrders.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                                }

                                //UpdateLstOrderDic(obj);

                                _OnOwnOrders?.Invoke(obj);

                            }
                            if (reply.datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                        Thread.Sleep(100);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        private ConcurrentDictionary<string, Action<ModifyOrdersReply>> _modifyOrderReplyDic = new ConcurrentDictionary<string, Action<ModifyOrdersReply>>();
        private ConcurrentDictionary<string, NewOrderSingle> _modifyOrderDic = new ConcurrentDictionary<string, NewOrderSingle>();
        /// <summary>
        /// 先取消订单， 等待订单回报。
        /// 若订单回报状态是 “撤单”，再发下单
        /// 若订单回报状态非 "撤单"， 直接返回报错信息。
        /// </summary>
        /// <param name="modifyOrderSingle">修改新单的参数</param>
        /// <param name="orderID">原笔订单的OrderID</param>
        /// <param name="callbackAction">消息结果回调</param>
        public void ModifyOrders(NewOrderSingle modifyOrderSingle, string orderID, Action<ModifyOrdersReply> callbackAction)
        {
            lock(string.Intern(orderID))
            {
                _modifyOrderReplyDic.AddOrUpdate(orderID, callbackAction, (key, value) => callbackAction);
                _modifyOrderDic.AddOrUpdate(orderID, modifyOrderSingle, (key, value) => modifyOrderSingle);
            }
            
            OrderCancelRequest cancelPo = new OrderCancelRequest();
            cancelPo.AlgoName = modifyOrderSingle.AlgoName;
            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
            cancelPo.OrderID = orderID;
            TBookCancelReplyPo reply = CancelOrder(cancelPo);
            if (reply != null)
            {

                if (!reply.IsSuccess())
                {
                    Action<ModifyOrdersReply> cbAction;
                    if(_modifyOrderReplyDic.TryRemove(orderID, out cbAction))
                    {
                        ModifyOrdersReply modifyOrderReply = new ModifyOrdersReply();
                        modifyOrderReply.Code = "0001";
                        modifyOrderReply.Msg = "撤单失败";
                        modifyOrderReply.OrderID = orderID;
                        cbAction?.Invoke(modifyOrderReply);
                        NewOrderSingle modifyOrderSingle1;
                        _modifyOrderDic.TryRemove(orderID, out modifyOrderSingle1);
                    }
                    return;
                }
            }
        }

        public PResult PlaceOrder(NewOrderSingle order)
        {
            order.QuoteID = string.Empty;
            PResult result = TBookHandler.Instance.PlaceOrder(order);
            return result;
        }

        public TBookCancelReplyPo CancelOrder(OrderCancelRequest cancelOrder)
        {
            TBookCancelReplyPo result = TBookHandler.Instance.CancelOrder(cancelOrder);
            return result;
        }
    }

    public class ModifyOrdersReply : BaseReplyPo
    {
        public string OrderID;
    }
}
