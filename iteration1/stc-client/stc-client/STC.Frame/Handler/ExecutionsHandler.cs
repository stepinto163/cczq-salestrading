using com.gateway.log;
using STC.Common.Api;
using STC.Common.Handler.Base;
using STC.Common.Handler.MarketExec;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Utils
{
    /// <summary>
    /// 成交数据工具类
    /// </summary>
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

        public event Action<ExecutionReport> OnOwnExecutionsOnlyNew
        {
            add
            {
                lock (m_OnOwnExecutionsEventLock)
                {
                    if (_OnOwnExecutions == null || !_OnOwnExecutions.GetInvocationList().Contains(value))
                        _OnOwnExecutions += value;
                }
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
                    _OnMarketExecutions += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    List<ExecutionReport> tmp = _CacheMarketExecutions.Values.OrderBy(p => p.UpdateTime).ToList();
                    // string s = Newtonsoft.Json.JsonConvert.SerializeObject(tmp);
                    foreach (var obj in tmp)
                    {
                        _OnMarketExecutions?.Invoke(obj);
                    }
                });
            }
            remove
            {
                lock (m_OnMarketExecutionsEventLock)
                {
                    _OnMarketExecutions -= value;
                }
            }
        }

        public event Action<ExecutionReport> OnMarketExecutionsOnlyNew
        {
            add
            {
                lock (m_OnMarketExecutionsEventLock)
                {
                    _OnMarketExecutions += value;
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
        private ConcurrentDictionary<string, ExecutionReport> _CacheMarketExecutions = new ConcurrentDictionary<string, ExecutionReport>();

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

        /// <summary>
        /// 初始化
        /// </summary>
        public void init()
        {
            if (isInit == false)
            {
                MarketExecHandler.Instance.Init(Instance_OnExecOrder);
                
               // RequestOwnExecutions();
                RequestMarketExecutions();

                isInit = true;
            }
        }

        /// <summary>
        /// 清零数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("ExecutionsHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                _CacheMarketExecutions.Clear();
                _CacheOwnExecutions.Clear();
                isInit = false;

                //MyExecutionProcesser.Processer.ResetClear();
                //LastExecutionProcesser.Processer.ResetClear();

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

        /// <summary>
        /// 所有成交数据回调方法（本方所有ems成交数据+cdh数据）
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnExecOrder(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj.VenueTypeGW != null && (!obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CMDS.ToLower())))
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
                    if (_CacheMarketExecutions.ContainsKey(obj.ExecID)
                         && _CacheMarketExecutions[obj.ExecID].TransactTime.CompareTo(obj.TransactTime) > 0)
                    {
                        return;
                    }
                    _CacheMarketExecutions.AddOrUpdate(obj.ExecID, obj, (key, value) => obj);
                }
                _OnMarketExecutions?.Invoke(obj);
            }
        }

        /// <summary>
        /// 测试方法
        /// </summary>
        /// <param name="obj"></param>
        public void TestOnExecOrder(Common.Model.Fix.Message.ExecutionReport obj)
        {
            this.Instance_OnExecOrder(obj);
        }

        /// <summary>
        /// ExecutionsHandler实例
        /// </summary>
        private static ExecutionsHandler server = new ExecutionsHandler();
        public static ExecutionsHandler Instance { get { return server; } }

        /// <summary>
        /// 请求当日历史成交数据（所有本方ems成交数据 + cdh数据）
        /// </summary>
        public void RequestMarketExecutions()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.dealTimeBegin = today;
                    request.dealTimeEnd = today;
                    //request.venuetypegw = "CDH";
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

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            foreach (Common.Model.Fix.Message.ExecutionReport obj in reply.datas)
                            {
                                if (obj.VenueTypeGW != null && (!obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CMDS.ToLower())))
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
                                    lock (string.Intern(obj.ExecID))
                                    {
                                        if (_CacheMarketExecutions.ContainsKey(obj.ExecID)
                                            && _CacheMarketExecutions[obj.ExecID].TransactTime.CompareTo(obj.TransactTime) > 0)
                                        {
                                            continue;
                                        }
                                        _CacheMarketExecutions.AddOrUpdate(obj.ExecID, obj, (key, value) => obj);
                                    }
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
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestExecutions", e);
                }
            });
        }

        /// <summary>
        /// 请求当日历史我的成交数据
        /// </summary>
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

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            foreach (Common.Model.Fix.Message.ExecutionReport obj in reply.datas)
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
                                    lock (string.Intern(obj.ExecID))
                                    {
                                        if (_CacheMarketExecutions.ContainsKey(obj.ExecID)
                                            && _CacheMarketExecutions[obj.ExecID].TransactTime.CompareTo(obj.TransactTime) > 0)
                                        {
                                            continue;
                                        }
                                        _CacheMarketExecutions.AddOrUpdate(obj.ExecID, obj, (key, value) => obj);
                                    }
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
