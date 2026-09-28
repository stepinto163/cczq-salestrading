using com.gateway.connector.tcp;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.dps.plugin.Common;
using STC.dps.plugin.QM.po;
using STC.dps.plugin.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.dps.plugin.Handler
{
    public class OrdersHandler : IOrdersHandler
    {
        private static OrdersHandler ordersHandler = new OrdersHandler();
        public static IOrdersHandler Instance { get { return ordersHandler; } }
        public GateWayApi GateWayApi { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// 缓存我的订单(订阅)
        /// </summary>
        private ConcurrentDictionary<string, ExecutionReport> _CacheOwnOrders = new ConcurrentDictionary<string, ExecutionReport>();
        //判断修改
        private ConcurrentDictionary<string, ExecutionReport> _CacheKeyOrders = new ConcurrentDictionary<string, ExecutionReport>();
        private ConcurrentDictionary<string, List<string>> _CacheKeyID = new ConcurrentDictionary<string, List<string>>();
        //编号，每个人、每个经纪商、每天 从1开始
        private ConcurrentDictionary<string, int> _CacheBrokerVersion = new ConcurrentDictionary<string, int>();
        private OrdersHandler()
        {
           
        }

        public void Init(GateWayApi GateWayApi, string userName)
        {
            this.GateWayApi = GateWayApi;
            this.UserName = userName;
            RequestOrders();
        }

        public void OnMessage(ExecutionReport obj)
        {
            //ThreadPool.QueueUserWorkItem(o =>
            //{
                try
                {
                    Instance_OnOrder(obj);
                }
                catch (Exception e)
                {
                    LogHelper.Error("ExecutionReport", e);
                }
            //});
        }

        //public void RequestOrders()
        //{
        //    System.Threading.ThreadPool.QueueUserWorkItem(o =>
        //    {
        //        try
        //        {
        //            QueryOrderReq request = new QueryOrderReq();
        //            string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
        //            request.dealTimeBegin = today;
        //            request.dealTimeEnd = today;
        //            request.userName = UserName ;//LocalEnv.UserInfo.UserName;
        //            //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
        //            //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
        //            //request.orderStatus = "N,0,1";
        //            int index = 0;
        //            request.pageSize = 2000;

        //            bool flag = true;
        //            while (flag)
        //            {
        //                request.pageNum = index;
        //                ///获取订单 
        //                string content = JsonConvert.SerializeObject(request);
        //                TBookOrder reply = GateWayApi.requestSync<TBookOrder>(Constants.TBookSvr, Constants.Topic_stc_tbook_orderQuery, content);
        //                LogHelper.Info("reply：" + reply);
        //                if (reply == null || reply.datas == null)
        //                {
        //                    flag = false;
        //                    break;
        //                }

        //                if (reply != null && reply.datas != null)
        //                {
        //                    foreach (ExecutionReport obj in reply.datas)
        //                    {
        //                        Instance_OnOrder(obj);
        //                    }
        //                    if (reply.datas.Count < request.pageSize)
        //                    {
        //                        flag = false;
        //                    }
        //                    else
        //                    {
        //                        index++;
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            LogHelper.Error("RequestOrders", e);
        //        }
        //    });
        //}

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
                    request.userName = UserName;
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
                        string content = JsonConvert.SerializeObject(request);
                        var reply = GateWayApi.requestSync<TBookOrder>(Constants.TBookSvr, Constants.Topic_stc_ems_orderQuery, content);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            foreach (ExecutionReport obj in reply.datas)
                            {
                                Instance_OnOrder(obj);
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


        private void Instance_OnOrder(ExecutionReport obj)
        {
            lock (string.Intern(obj.OrderID))
            {
                if (Constants.VenueTypeGW.Equals(obj.VenueTypeGW))
                    return;
                string orderKey = obj.SecurityID + obj.Side + obj.Venues;
                //if (!(obj.ExecType == "N" || obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "S" || obj.ExecType == "6"))
                if (obj.ExecType == "4" || obj.ExecType == "6")
                {
                    ExecutionReport outEx;
                    string orderID = obj.StrategyOrderId != null ? obj.StrategyOrderId : obj.OrderID;
                    if (_CacheKeyID.ContainsKey(orderKey))
                    {
                        List<string> idList = _CacheKeyID[orderKey];
                        idList.Remove(orderID);
                        if (idList.Count == 0 && _CacheKeyOrders.ContainsKey(orderKey))
                        {
                            _CacheKeyOrders.TryRemove(orderKey, out outEx);
                        }
                    }
                    if (_CacheOwnOrders.ContainsKey(orderID))
                    {
                        _CacheOwnOrders.TryRemove(orderID, out outEx);
                    }
                    if (_CacheBrokerVersion.ContainsKey(obj.Venues))
                    {
                        int oldId = _CacheBrokerVersion[obj.Venues];
                        int id = CommonUtil.ConvertToInt(obj.Info2);
                        if (id > oldId)
                            _CacheBrokerVersion.AddOrUpdate(obj.Venues, id, (key, value) => id);
                    }
                    else
                    {
                        int id = CommonUtil.ConvertToInt(obj.Info2);
                        _CacheBrokerVersion.AddOrUpdate(obj.Venues, id, (key, value) => id);
                    }
                }
                else
                {
                    string orderID = obj.StrategyOrderId != null ? obj.StrategyOrderId : obj.OrderID;
                    _CacheOwnOrders.AddOrUpdate(orderID, obj, (key, value) => obj);
                    _CacheKeyOrders.AddOrUpdate(orderKey, obj, (key, value) => obj);
                    List<string> idList = new List<string>();
                    if (!_CacheKeyID.ContainsKey(orderKey))
                    {
                        idList.Add(orderID);
                    }
                    else
                    {
                        idList = _CacheKeyID[orderKey];
                        if (!idList.Contains(orderID))
                            idList.Add(orderID);
                    }
                    _CacheKeyID.AddOrUpdate(orderKey, idList, (key, value) => idList);
                    if (_CacheBrokerVersion.ContainsKey(obj.Venues))
                    {
                        int oldId = _CacheBrokerVersion[obj.Venues];
                        int id = CommonUtil.ConvertToInt(obj.Info2);
                        if (id > oldId)
                            _CacheBrokerVersion.AddOrUpdate(obj.Venues, id, (key, value) => id);
                    }
                    else
                    {
                        int id = CommonUtil.ConvertToInt(obj.Info2);
                        _CacheBrokerVersion.AddOrUpdate(obj.Venues, id, (key, value) => id);
                    }
                }
            }
        }

        //获取每个经纪商的编号
        public int getVersionByVenue(string venue)
        {
            int version = 0;
            string[] temp = new string[2];
            if (venue.Contains("-"))
                temp = venue.Split('-');
            if (!string.IsNullOrEmpty(temp[1]))
            {
                if (_CacheBrokerVersion.ContainsKey(temp[1]))
                    version = _CacheBrokerVersion[temp[1]];
            }
            return version;
        }

        //判断报价修改
        public QMPo IsContain(string key, NewOrderSingle order)
        {
            lock (_CacheKeyOrders)
            {
                QMPo po = new QMPo();
                //0-新增;1-修改;2-删除
                if (_CacheKeyOrders.ContainsKey(key))
                {
                    po.action = 1;
                    po.ExecutionReport = _CacheKeyOrders[key];
                }
                else
                    po.action = 0;
                return po;
            }
        }

        public QMPo IsContainEMS(string key, ExecutionReport order)
        {
            lock (_CacheKeyOrders)
            {
                QMPo po = new QMPo();
                //0-新增;1-修改;2-删除
                if (_CacheKeyOrders.ContainsKey(key))
                {
                    po.action = 1;
                    po.ExecutionReport = _CacheKeyOrders[key];
                }
                else
                    po.action = 0;
                return po;
            }
        }

        //判断订单是否存在，做撤销
        public ExecutionReport IsContainID(string Id)
        {
            lock (_CacheOwnOrders)
            {
                if (_CacheOwnOrders.ContainsKey(Id))
                    return _CacheOwnOrders[Id];
                else
                    return null;
            }
        }

        public void Clear()
        {
            _CacheOwnOrders.Clear();
            _CacheKeyID.Clear();
            _CacheKeyOrders.Clear();
            _CacheBrokerVersion.Clear();
            LogHelper.Info("OrdersHandler clear！");
        }
       
    }
   
    //class QMMessage : IMessage
    //{
    //    private Proto proto;
    //    public QMMessage(Proto proto)
    //    {
    //        this.proto = proto;
    //    }

    //    ~QMMessage()
    //    {
    //    }

    //    public void onMessage(string topic, string t)
    //    {
    //        try
    //        {
    //            ExecutionReport ex = JsonConvert.DeserializeObject<ExecutionReport>(t);
    //            OrdersHandler.Instance.OnMessage(ex);
    //        }
    //        catch (Exception e)
    //        {
    //            LogHelper.Error(e);
    //        }
    //    }
    //}

}