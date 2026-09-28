using com.gateway.log;
using STC.Common.Handler.Base;
using STC.Common.Handler.MarketExec;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Data
{
    /// <summary>
    /// Ems数据处理类
    /// </summary>
    public class EmsOrdersHandler
    {
        /// <summary>
        /// key: securityID ;  value:回调函数
        /// </summary>
        private ConcurrentDictionary<string, Action<ExecutionReport>> _UpdateEmsOrderCallbackDic = new ConcurrentDictionary<string, Action<ExecutionReport>>();

        /// <summary>
        /// key: securityID+Action<ExecutionReport>， value：注册数量
        /// </summary>
        private ConcurrentDictionary<string, int> registerCountDic = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 缓存Ems订单(订阅)
        /// key: VenueTypeGW#orderID; value: ems消息
        /// </summary>
        private ConcurrentDictionary<string, ExecutionReport> _CacheEmsOrders = new ConcurrentDictionary<string, ExecutionReport>();

        /// <summary>
        /// key: Venues+StrategyOrderId, value: blocknum 
        /// </summary>
        private ConcurrentDictionary<string, int> blocknumDic = new ConcurrentDictionary<string, int>();

        //// key: strategyOrderID;  value: orderIDList
        //private ConcurrentDictionary<string, List<string>> dataDic = new ConcurrentDictionary<string, List<string>>();

        private EmsOrdersHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        private volatile bool isInit = false;

        public void init()
        {
            if (isInit == false)
            {
                bool isAllow = Common.Handler.Admin.AdminHandler.Instance.CheckPermission("1003", null);
                if (!isAllow)
                {
                    isInit = true;
                    return;
                }

                MarketExecHandler.Instance.SubscribeOrder(Instance_OnEmsOrder);
              
                isInit = true;
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("EmsOrdersHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                if (_CacheEmsOrders != null)
                {
                    _CacheEmsOrders.Clear();
                }
                
                if (blocknumDic != null)
                {
                    blocknumDic.Clear();
                }

                // 隔夜重连， 把注册过的债券再请求一下
                // 复制
                Dictionary<string, Action<ExecutionReport>> copyDic = _UpdateEmsOrderCallbackDic.ToDictionary(x => x.Key, x => x.Value);
                // 清空_UpdateEmsOrderCallbackDic
                _UpdateEmsOrderCallbackDic.Clear();
                // 重新注册
                if (copyDic != null && copyDic.Count > 0)
                {
                    foreach (KeyValuePair<string, Action<ExecutionReport>> pair in copyDic)
                    {
                        RegistEMSOrdersBySecurityID(pair.Key, pair.Value);
                    }
                }

                isInit = false;

                init();
            }
        }

        private void Instance_OnEmsOrder(Common.Model.Fix.Message.ExecutionReport obj)
        {
            // 没有注册， 无需处理
            if (_UpdateEmsOrderCallbackDic.ContainsKey(obj.SecurityID) == false 
                && obj.UserName != LocalEnv.UserInfo.UserName)
            {
                return;
            }

            // 缓存EmsOrder消息
            bool needEliminate = false;
            bool isUpdate = true;
            string blocknumDicKey = string.Format("{0}-{1}", obj.Venues, obj.StrategyOrderId);// obj.StrategyOrderId;
            if (!(obj.OrdStatus == "N" || obj.OrdStatus == "0" || obj.OrdStatus == "1" || obj.OrdStatus == "S" || obj.OrdStatus == "6"))
            {
                if (obj.UserName != LocalEnv.UserInfo.UserName)
                {
                    needEliminate = true;
                    isUpdate = false;
                }
                else
                {
                    int newBlocknum = obj.BlockNum.ConvertToInt();
                    if (blocknumDic.ContainsKey(blocknumDicKey))
                    {
                        if (blocknumDic[blocknumDicKey].CompareTo(newBlocknum) < 0)
                        {
                            blocknumDic[blocknumDicKey] = newBlocknum;
                            needEliminate = true;
                        }
                        else if (blocknumDic[blocknumDicKey].CompareTo(newBlocknum) > 0)
                        {
                            // 过时消息，无需处理
                            return;
                        }
                    }
                    else
                    {
                        blocknumDic[blocknumDicKey] = newBlocknum;
                    }
                }
            }
           

            lock (string.Intern(obj.OrderID))
            {
                // 剔除过时数据
                if (needEliminate)
                {
                    IEnumerable<KeyValuePair<string, ExecutionReport>> removeItems = _CacheEmsOrders.Where(
                        item => item.Value.StrategyOrderId == obj.StrategyOrderId 
                        && item.Value.Venues == obj.Venues);
                    foreach (KeyValuePair<string, ExecutionReport> item in removeItems)
                    {
                        ExecutionReport exReport = null;
                        _CacheEmsOrders.TryRemove(item.Key, out exReport);
                    }
                }

                if (isUpdate)
                {
                    // 加入新数据
                    string primarykey = $"{obj.VenueTypeGW}#{obj.OrderID}";

                    if (_CacheEmsOrders.ContainsKey(primarykey) == false 
                        || (_CacheEmsOrders.ContainsKey(primarykey) && _CacheEmsOrders[primarykey].UpdateTime.CompareTo(obj.UpdateTime) <= 0))
                    {
                        _CacheEmsOrders.AddOrUpdate(primarykey, obj, (key, value) => obj);
                    }
                    else
                    {
                        isUpdate = false;
                    }
                }
            }

            if (_UpdateEmsOrderCallbackDic.ContainsKey(obj.SecurityID) && isUpdate)
                _UpdateEmsOrderCallbackDic[obj.SecurityID]?.Invoke(obj);
        }

        private static EmsOrdersHandler server = new EmsOrdersHandler();
        public static EmsOrdersHandler Instance { get { return server; } }

        //// 缓存已订阅过securityID
        //private ConcurrentBag<string> _cacheBag = new ConcurrentBag<string>();

        private void RequestEmsOrders(string securityId)
        {

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.dealTimeBegin = today;
                    request.dealTimeEnd = today;
                    request.securityid = securityId;
                    //request.userName = LocalEnv.UserInfo.UserName;
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    request.orderStatus = "N,0,1,S,6";
                    int index = 0;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = MarketExecHandler.Instance.QueryOrderForEMS(request);

                        if (reply == null || reply.datas == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            foreach (Common.Model.Fix.Message.ExecutionReport obj in reply.datas)
                            {
                                obj.Vs = obj.Vs == null || obj.Vs == "Y" || obj.Vs == "" ? "Y" : "N";

                                Instance_OnEmsOrder(obj);
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
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        public void RegistEMSOrdersBySecurityID(string securityID, Action<ExecutionReport> callBack)
        {
            /*if (string.IsNullOrEmpty(securityID) == false && callBack != null)
            {
                string registerKey = $"{securityID}#{callBack.Target}#{callBack.Method.Name}";
                if (registerCountDic.ContainsKey(registerKey) == false)
                {
                    registerCountDic[registerKey] = 0;
                }
                registerCountDic[registerKey]++;

                lock (string.Intern(securityID))
                {
                    if (_UpdateEmsOrderCallbackDic.ContainsKey(securityID) == false)
                    {
                        _UpdateEmsOrderCallbackDic[securityID] = callBack;

                        IEnumerable<KeyValuePair<string, ExecutionReport>> cacheItems = _CacheEmsOrders.Where(item => item.Value.SecurityID == securityID);
                        if (cacheItems != null)
                        {
                            foreach (KeyValuePair<string, ExecutionReport> item in cacheItems)
                            {
                                callBack?.Invoke(item.Value);
                            }
                        }
                        bool isAllow = Common.Handler.Admin.AdminHandler.Instance.CheckPermission("1003", null);
                        if (!isAllow)
                        {
                            return;
                        }
                        ThreadPool.QueueUserWorkItem(o => {

                            // 还没有注册， 先注册数据
                            RequestEmsOrders(securityID);
                        });

                        //ThreadPool.QueueUserWorkItem(o =>
                        //{
                        //    try
                        //    {
                        //        MarketExecHandler.Instance.SubscribeOrderBySecurtityIDs(new List<string> { securityID }, Instance_OnEmsOrder);

                        //    }
                        //    catch (Exception e)
                        //    {
                        //        LogHelper.Error("RegistEMSOrdersBySecurityID", e);
                        //    }
                        //});
                    }
                    else
                    {
                        int index = -1;
                        if (_UpdateEmsOrderCallbackDic[securityID] != null)
                        {
                            index = Array.IndexOf(_UpdateEmsOrderCallbackDic[securityID].GetInvocationList(), (Action<ExecutionReport>)callBack);
                        }
                        
                        if (index < 0)
                        {
                            _UpdateEmsOrderCallbackDic[securityID] += callBack;

                            // 已经注册， 使用缓存数据
                            IEnumerable<KeyValuePair<string, ExecutionReport>> cacheItems = _CacheEmsOrders.Where(item => item.Value.SecurityID == securityID);
                            if (cacheItems != null)
                            {
                                foreach (KeyValuePair<string, ExecutionReport> item in cacheItems)
                                {
                                    callBack?.Invoke(item.Value);
                                }
                            }
                        }
                    }
                }
            }*/
        }

        public void UnregistEMSOrderBySecurityID(string securtityID, Action<ExecutionReport> callBack)
        {
            /*if (string.IsNullOrEmpty(securtityID) == false && callBack != null)
            {
                string registerKey = $"{securtityID}#{callBack.Target}#{callBack.Method.Name}";
                if (registerCountDic.ContainsKey(registerKey) == false)
                {
                    return;
                }
                registerCountDic[registerKey]--;

                if(registerCountDic[registerKey] > 0)
                {
                    return;
                }
                else
                {
                    int count = 0;
                    registerCountDic.TryRemove(registerKey, out count);
                }


                // 去掉回调函数
                if (_UpdateEmsOrderCallbackDic.ContainsKey(securtityID))
                {
                    _UpdateEmsOrderCallbackDic[securtityID] -= callBack;
                }

                if (_UpdateEmsOrderCallbackDic[securtityID] == null || _UpdateEmsOrderCallbackDic[securtityID].GetInvocationList().Count() == 0)
                {
                    //Action<ExecutionReport> erCallBack = null;
                    //_UpdateEmsOrderCallbackDic.TryRemove(securtityID, out erCallBack);

                    //    // 清空缓存
                    //    List<string> removeSecurityIDList = new List<string>();
                    //    IEnumerable<KeyValuePair<string, ExecutionReport>> removeItems =  _CacheEmsOrders.Where(item => item.Value.SecurityID == securtityID);
                    //     foreach(KeyValuePair<string, ExecutionReport> item in removeItems)
                    //    {
                    //        ExecutionReport exReport = null;
                    //        _CacheEmsOrders.TryRemove(item.Key, out exReport);
                    //        if (exReport != null)
                    //        {
                    //            if (removeSecurityIDList.Contains(exReport.StrategyOrderId) == false)
                    //            {
                    //                removeSecurityIDList.Add(exReport.StrategyOrderId);
                    //            }
                    //        }
                    //    }

                    //    // 清理blocknumDic
                    //    foreach (string removeStrateygOrderID in removeSecurityIDList)
                    //    {
                    //        int removeBlockNum = 0;
                    //        blocknumDic.TryRemove(removeStrateygOrderID, out removeBlockNum);
                    //    }


                    //    ThreadPool.QueueUserWorkItem(o =>
                    //    {
                    //        try
                    //        {
                    //            MarketExecHandler.Instance.UnsubscribeOrderBySecurtityIDs(new List<string> { securtityID });
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            LogHelper.Error("UnregistEMSOrderBySecurityID", e);
                    //        }
                    //    });
                }
            }*/
        }

        /// <summary>
        /// 移除指定StrategyOrderID的记录
        /// </summary>
        /// <param name="strategyOrderID">订单号</param>
        public void RemoveCacheDataByStrategyOrderId(string strategyOrderID)
        {
            // 删除_CacheEmsOrders中缓存
            IEnumerable<KeyValuePair<string, ExecutionReport>> removeItems = _CacheEmsOrders.Where(item => item.Value.StrategyOrderId == strategyOrderID);
            ExecutionReport exReport = null;
            foreach (KeyValuePair<string, ExecutionReport> item in removeItems)
            {
                _CacheEmsOrders.TryRemove(item.Key, out exReport);
            }

            // 删除blocknumDic中缓存
            List<string> filterKey = blocknumDic.Keys.Where(delegate (string key)
            {
                return key.Contains(strategyOrderID);
            }).ToList();

            if (filterKey != null && filterKey.Count > 0)
            {
                int removeBlocknum;
                foreach (string key in filterKey)
                    blocknumDic.TryRemove(key, out removeBlocknum);
            }

        }
    }
}
