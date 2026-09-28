using com.gateway.log;
using BDB.Trade.Plugin.Handler.DataProcesser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STC.Common.Model.Fix.Message;
using STC.Common.Handler.Base;
using STC.Common.Handler.MarketExec;
using STC.Common.Utils;
using STC.Common.Model.Base;
using STC.Common.Api;
using STC.Common.Model.TBook;

namespace BDB.Trade.Plugin.Handler
{
    public class ExecutionsHandler
    {
        // 订阅我的成交
        private readonly Object m_OnOwnExecutionsEventLock = new Object();
        private Action<ExecutionReport> _OnOwnExecutions;
        public event Action<ExecutionReport> OnOwnExecutions
        {
            add
            {
                lock (m_OnOwnExecutionsEventLock)
                {
                    if (_OnOwnExecutions == null || !_OnOwnExecutions.GetInvocationList().Contains(value))
                        _OnOwnExecutions += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheOwnExecutions)
                    {
                        _OnOwnExecutions?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnOwnExecutionsEventLock)
                {
                    _OnOwnExecutions -= value;
                }
            }
        }

        // 订阅市场成交
        private readonly Object m_OnMarketExecutionsEventLock = new Object();
        private Action<ExecutionReport> _OnMarketExecutions;
        public event Action<ExecutionReport> OnMarketExecutions
        {
            add
            {
                lock (m_OnMarketExecutionsEventLock)
                {
                    if(_OnMarketExecutions == null || !_OnMarketExecutions.GetInvocationList().Contains(value))
                    {
                        _OnMarketExecutions += value;
                    }
                }
            }
            remove
            {
                lock (m_OnMarketExecutionsEventLock)
                {
                    _OnMarketExecutions -= value;
                }
            }
        }

        /// <summary>
        /// 缓存市场成交
        /// </summary>
        //private ConcurrentDictionary<string, ExecutionReport> _CacheMarketExecutions = new ConcurrentDictionary<string, ExecutionReport>();

        /// <summary>
        /// 缓存我的成交
        /// </summary>
        private ConcurrentDictionary<string, ExecutionReport> _CacheOwnExecutions = new ConcurrentDictionary<string, ExecutionReport>();

        /// <summary>
        /// 构造函数
        /// </summary>
        private ExecutionsHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        private volatile bool isInit = false;

        public bool EndInit()
        {
            return isInit;
        }

        public void init()
        {
            if (isInit == false)
            {
                MarketExecHandler.Instance.InitByBroker(Instance_OnExecOrder, LocalEnv.UserInfo.UserName, PermisionHandler.Instance.GetBondPermission());

                RequestOwnExecutions();
                //RequestMarketExecutions();

                isInit = true;
            }
        }


        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("ExecutionsHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                //_CacheMarketExecutions.Clear();
                _CacheOwnExecutions.Clear();
                isInit = false;

                //MyExecutionProcesser.Processer.ResetClear();//todo_lily:
                LastExecutionProcesser.Processer.ResetClear();

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        init();
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("RequestOrders", e);
                    }
                });
            }
        }

        public ExecutionReport GetOrderById(string orderId)
        {
            //if (_CacheMarketExecutions.ContainsKey(orderId))
            //    return _CacheMarketExecutions[orderId];

            //return MarketExecHandler.Instance.GetmarketExecByExecID(orderId);

            return MarketExecCache.Instance.GetmarketExecByExecID(orderId);
        }

        /// <summary>
        /// 当收到市场撤销单时，重新推送内存里面最新的一笔成交单
        /// </summary>
        /// <param name="obj"></param>
        private void DealCancelStatus(ExecutionReport obj)
        {
            if (obj.OrdStatus == "4")
            {
                LogHelper.Info(string.Format("DealCancelStatus ordId:{0}  securityid:{1}, transacTime:{2}", obj.OrderID, obj.SecurityID, obj.TransactTime)); 
                //IEnumerable<ExecutionReport> list = MarketExecHandler.Instance.GetMarketExecByCondition(obj.SecurityID, string.Empty);
                IEnumerable<ExecutionReport> list = MarketExecCache.Instance.GetMarketExecByCondition(obj.SecurityID, string.Empty);
                //_CacheMarketExecutions.Values.Where((o) => { return o.SecurityID == obj.SecurityID; });
                //IEnumerable <ExecutionReport> sortList = list.OrderByDescending(num => num, new SpecialComparer());

                ExecutionReport repeatOrder = null;
                int count = list.Count();
                if (count > 0)
                {
                    for (int i = count - 1; i >= 0; i--)
                    {
                        ExecutionReport order = list.ElementAt(i);

                        if (order.OrdStatus != "4")
                        {
                            repeatOrder = order;
                            break;
                        }
                    }
                }
                if (repeatOrder != null)
                {
                    repeatOrder.Info1 = obj.Info1;
                    //LogHelper.Info(string.Format("DealCancelStatus ordId:{0}, info1:{1}", obj.OrderID, repeatOrder.Info1));
                    _OnMarketExecutions?.Invoke(repeatOrder);
                }
                else
                {
                    LogHelper.Info("RepeatOrde is null.");
                }
            }
        }

        public void Instance_OnExecOrder(ExecutionReport obj)
        {
            if (obj.VenueTypeGW != null && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SDN.ToLower()))
            {
                lock (string.Intern(obj.OrderID))
                {
                    _CacheOwnExecutions.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                }

                _OnOwnExecutions?.Invoke(obj);
            }
            else
            {
                //// 过滤掉 4：成交无效（删除/撤销等）
                //if (obj.OrdStatus == "4")
                //{
                //    return;
                //}
                lock (string.Intern(obj.ExecID))
                {
                    _OnMarketExecutions?.Invoke(obj);

                    DealCancelStatus(obj);
                }
                
            }
        }

        public void TestOnExecOrder(ExecutionReport obj)
        {
            this.Instance_OnExecOrder(obj);
        }

        private static ExecutionsHandler server = new ExecutionsHandler();
        public static ExecutionsHandler Instance { get { return server; } }

        public void RequestOwnExecutions()
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
                    // request.venuetypegw = "Connect2";
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    request.orderStatus = "1,2";
                    int index = 0;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取成交
                        var reply = MarketExecHandler.Instance.QueryExecOrderForEMS(request);

                        if (reply == null || reply.datas == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            foreach (ExecutionReport obj in reply.datas)
                            {
                                if (obj.VenueTypeGW != null && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()))
                                {
                                    lock (string.Intern(obj.OrderID))
                                    {
                                        _CacheOwnExecutions.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                                    }

                                    _OnOwnExecutions?.Invoke(obj);
                                }
                                else
                                {
                                    //// 过滤掉 4：成交无效（删除/撤销等）
                                    //if (obj.OrdStatus == "4")
                                    //{
                                    //    continue;
                                    //}
                                    //lock (string.Intern(obj.ExecID))
                                    //{
                                    //    if (_CacheMarketExecutions.ContainsKey(obj.ExecID)
                                    //        && _CacheMarketExecutions[obj.ExecID].TransactTime.CompareTo(obj.TransactTime) > 0)
                                    //    {
                                    //        continue;
                                    //    }
                                    //    _CacheMarketExecutions.AddOrUpdate(obj.ExecID, obj, (key, value) => obj);
                                    //}
                                    _OnMarketExecutions?.Invoke(obj);
                                }
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
                    LogHelper.Error("RequestExecutions", e);
                }
            });
        }
    }
}
