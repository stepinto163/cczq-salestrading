using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QTR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Handler.QTRS
{
    public class QtrsHandler : IQtrsHandler
    {
        private static IQtrsHandler instance = new QtrsHandler();
        public static IQtrsHandler Instance { get { return instance; } }

        public QtrsHandler()
        {
            //Base.BaseHandler.Instance.Init(Instance_OnClear);
        }

        ~QtrsHandler()
        {
            Dispose();
        }


        public void Dispose()
        {
            this.Clear();
            isInit = false;
        }


        private void Clear()
        {
            lock (lockobj)
            {
                CacheQtrOrderModel.Clear();
                CacheQtrTradeModel.Clear();

                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.STCLITESVR, new string[] { ApiConsts.Topic_dps_qtr_order + "*" });
                isSubscribeQtrOrder = false;

                ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.STCLITESVR, new string[] { ApiConsts.Topic_dps_qtr_trade + "*" });
                isSubscribeQtrTrade = false;
            }
        }

        public void Init()
        {
            //重新订阅
            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQtrOrderMsg, new string[] { ApiConsts.Topic_dps_qtr_order + "*" }, ApiConsts.STCLITESVR, "");
            isSubscribeQtrOrder = true;

            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQtrTradeMsg, new string[] { ApiConsts.Topic_dps_qtr_trade + "*" }, ApiConsts.STCLITESVR, "");
            isSubscribeQtrTrade = true;

            isInit = true;
        }


        public void InitTestData()
        {
            // 测试数据
            GenerateTestData();
        }


        public void Test()
        {
            // 动态改变测试数据
            // DyChgTestData();
        }

        #region 经纪商需求意向 构造
        public Dictionary<string, Action<List<QtrOrder>>> DictQtrOrderCallBack = new Dictionary<string, Action<List<QtrOrder>>>();
        private event Action<List<QtrOrder>> OnQtrOrder;
        /// <summary>
        /// 缓存数据
        /// </summary>
        private ObservableCollection<QtrOrder> CacheQtrOrderModel = new ObservableCollection<QtrOrder>();

        private bool isSubscribeQtrOrder = false;
        #endregion


        #region 经纪商已确认 构造
        public Dictionary<string, Action<List<QtrTrade>>> DictQtrTradeCallBack = new Dictionary<string, Action<List<QtrTrade>>>();
        private event Action<List<QtrTrade>> OnQtrTrade;
        /// <summary>
        /// 缓存数据
        /// </summary>
        private ObservableCollection<QtrTrade> CacheQtrTradeModel = new ObservableCollection<QtrTrade>();

        private bool isSubscribeQtrTrade = false;

        public object lockobj = new object();

        private volatile bool isInit = false;
        #endregion

        #region 清零信号
        private void Instance_OnClear(ClearPo obj)
        {

            if (obj != null && "Y".Equals(obj.IsClear))
            {
                try
                {
                    ResetHandler();
                }
                catch (Exception ex)
                {

                }
            }
        
        }

        public async void ResetHandler()
        {
            await Task.Run(() =>
            {
                this.Clear();
                Thread.Sleep(5000);
                this.Init();
            });
        }

        #endregion
        
        #region QTR 成交 
            /// <summary>
            /// 订阅 QTR 成交 
            /// </summary>
            /// <param name="onMsgCallBack"></param>
            public async void SubscribeQtrTrade(Action<List<QtrTrade>> callBack)
        {
            await Task.Run(() =>
            {
                if (callBack != null)
                {
                    //BDB.Position.Plugin.ViewModel.MainViewModel.OnPostionDetail
                    string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                    if (!DictQtrTradeCallBack.ContainsKey(callbackkey))
                    {
                        OnQtrTrade += callBack;
                        DictQtrTradeCallBack.Add(callbackkey, callBack);

                        if (isSubscribeQtrTrade)
                        {
                            callBack?.Invoke(CacheQtrTradeModel.ToList());
                            return;
                        }
                    }
                }

                lock (this)
                {
                    if (!isSubscribeQtrTrade)
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQtrTradeMsg, new string[] { ApiConsts.Topic_dps_qtr_trade + "*" }, ApiConsts.STCLITESVR, "");
                        isSubscribeQtrTrade = true;
                    }
                }
            });
        }

        /// <summary>
        /// 取消订阅 QTR 成交 
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public async void UnSubscribeQtrTrade(Action<List<QtrTrade>> callBack)
        {
            await Task.Run(() =>
            {
                if (callBack != null && OnQtrTrade != null)
                {
                    var funclist = OnQtrTrade.GetInvocationList();
                    {
                        foreach (var f in funclist)
                        {
                            if (f.Target.ToString() == callBack.Target.ToString())
                            {
                                OnQtrTrade -= callBack;
                                string key = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                                if (DictQtrTradeCallBack.ContainsKey(key))
                                    DictQtrTradeCallBack.Remove(key);
                            }
                        }

                    }
                }

                if (OnQtrTrade == null)
                {
                    ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.STCLITESVR, new string[] { ApiConsts.Topic_dps_qtr_trade + "*" });
                    isSubscribeQtrTrade = false;
                }
            });
        }

        /// <summary>
        /// Qtr 成交 回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnQtrTradeMsg(object obj)
        {
            NetMessage netMsg = obj as NetMessage;
            if (netMsg != null)
            {
                if (netMsg.Topic.Contains(ApiConsts.Topic_dps_qtr_trade))
                {

                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<QtrTrade> mmSysStatus = BaseHelper.ConvertStcMessage<List<QtrTrade>>(new string[] { netMsg.Topic }, obj);
                        CacheQtrTradeModel = new ObservableCollection<QtrTrade>(mmSysStatus);
                        OnQtrTrade?.Invoke(mmSysStatus);
                    }
                    else
                    {
                        lock (lockobj)
                        {
                            QtrTrade s = BaseHelper.ConvertStcMessage<QtrTrade>(new string[] { netMsg.Topic }, obj);

                            //更新原始订单编号
                            QtrOrder srcorder = CacheQtrOrderModel.FirstOrDefault(o => o.execID == s.dealID);
                            if (srcorder != null)
                            {
                                s.orderID = srcorder.execID;
                                //s.deliverToSubID = srcorder.deliverToSubID;
                            }

                            //更新缓存
                            QtrTrade pm = CacheQtrTradeModel.FirstOrDefault(o => o.dealID == s.dealID);
                            if (pm != null)
                            {
                                pm = s;
                            }
                            else
                            {
                                CacheQtrTradeModel.Add(s);
                            }

                            OnQtrTrade?.Invoke(new List<QtrTrade>() { s });
                        }
                    }
                }
            }
        }
        #endregion

        #region QTR 需求意向

        /// <summary>
        /// 订阅经纪商需求意向
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public async void SubscribeQtrOrder(Action<List<QtrOrder>> callBack)
        {
            await Task.Run(() =>
            {
                if (callBack != null)
                {
                    //BDB.Position.Plugin.ViewModel.MainViewModel.OnPostionDetail
                    string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                    if (!DictQtrOrderCallBack.ContainsKey(callbackkey))
                    {
                        OnQtrOrder += callBack;
                        DictQtrOrderCallBack.Add(callbackkey, callBack);

                        if (isSubscribeQtrOrder)
                        {
                            callBack?.Invoke(CacheQtrOrderModel.ToList());
                            return;
                        }
                    }
                }

                lock (this)
                {
                    if (!isSubscribeQtrOrder)
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQtrOrderMsg, new string[] { ApiConsts.Topic_dps_qtr_order + "*" }, ApiConsts.STCLITESVR, "");
                        isSubscribeQtrOrder = true;
                    }
                }

            });
        }



        /// <summary>
        /// 取消订阅经纪商需求意向
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public async void UnSubscribeQtrOrder(Action<List<QtrOrder>> callBack)
        {
            await Task.Run(() =>
            {
                if (callBack != null && OnQtrOrder != null)
                {
                    var funclist = OnQtrOrder.GetInvocationList();
                    {
                        foreach (var f in funclist)
                        {
                            if (f.Target.ToString() == callBack.Target.ToString())
                            {
                                OnQtrOrder -= callBack;
                                string key = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                                if (DictQtrOrderCallBack.ContainsKey(key))
                                    DictQtrOrderCallBack.Remove(key);
                            }
                        }

                    }
                }

                if (OnQtrOrder == null)
                {
                    ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.STCLITESVR, new string[] { ApiConsts.Topic_dps_qtr_order + "*" });
                    isSubscribeQtrOrder = false;
                }
            });
        }

        private Regex reg = new Regex(ApiConsts.Topic_stc_qtrs_di);

        /// <summary>
        /// 经纪商需求意向回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnQtrOrderMsg(object obj)
        {
            NetMessage netMsg = obj as NetMessage;
            if (netMsg != null)
            {
                if (netMsg.Topic.Contains(ApiConsts.Topic_dps_qtr_order))
                {

                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<QtrOrder> mmSysStatus = BaseHelper.ConvertStcMessage<List<QtrOrder>>(new string[] { netMsg.Topic }, obj);
                        CacheQtrOrderModel = new ObservableCollection<QtrOrder>(mmSysStatus);
                        OnQtrOrder?.Invoke(mmSysStatus);
                    }
                    else
                    {
                        lock (lockobj)
                        {
                            QtrOrder s = BaseHelper.ConvertStcMessage<QtrOrder>(new string[] { netMsg.Topic }, obj);

                            //更新缓存
                            QtrOrder pm = CacheQtrOrderModel.FirstOrDefault(o => o.orderID == s.orderID);
                            if (pm != null)
                            {
                                pm = s;
                            }
                            else
                            {
                                CacheQtrOrderModel.Add(s);
                            }

                            //更新Trade的 OrderID
                            QtrTrade qt = CacheQtrTradeModel.FirstOrDefault(o => o.dealID == s.execID);
                            if (qt != null)
                            {
                                qt.orderID = s.orderID;
                                //qt.deliverToSubID = s.deliverToSubID;
                                OnQtrTrade?.Invoke(new List<QtrTrade>() { qt });
                            }

                            OnQtrOrder?.Invoke(new List<QtrOrder>() { s });
                        }
                    }
                }
            }
        }
        #endregion


        #region Qtr 连接状态
        public void SubscribeQtrStatusMsg(Action<SessionStatus> callBack)
        {
            //连接状态
            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQtrStatusMsg,
                          new string[] { ApiConsts.Topic_dps_qtr_netStatus + "*" },
                          ApiConsts.STCLITESVR, "");
        }

        public void UnSubscribeQtrStatusMsg(Action<SessionStatus> callBack)
        {
            //连接状态
            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQtrStatusMsg,
                          new string[] { ApiConsts.Topic_dps_qtr_netStatus + "*" },
                          ApiConsts.STCLITESVR, "");
        }

        private void Instance_OnQtrStatusMsg(object obj)
        {
            SessionStatus data = BaseHelper.ConvertStcMessage<SessionStatus>(new string[] { $"{ ApiConsts.Topic_dps_qtr_netStatus}*" }, obj);
            if (data != null)
            {
                //to do...
            }
        }
        #endregion


        #region Qtr 行情 
        public void SubscribeQtrQuoteMsg(Action<QtrQuote> callBack)
        {
            //行情
            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQtrQuoteMsg,
                       new string[] { ApiConsts.Topic_dps_qtr_quote + "*" },
                       ApiConsts.STCLITESVR, "");
        }

        public void UnSubscribeQtrQuoteMsg(Action<QtrQuote> callBack)
        {
            //行情
            ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQtrQuoteMsg,
                       new string[] { ApiConsts.Topic_dps_qtr_quote + "*" },
                       ApiConsts.STCLITESVR, "");
        }


        private void Instance_OnQtrQuoteMsg(object obj)
        {
            QtrQuote data = BaseHelper.ConvertStcMessage<QtrQuote>(new string[] { $"{ ApiConsts.Topic_dps_qtr_quote}*" }, obj);
            if (data != null)
            {
                //to do...
            }
        }

        #endregion

        #region 测试数据
        private void GenerateTestData()
        {
            // -------- 创建测试数据 -------------
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                List<QtrOrder> lstdi = new List<QtrOrder>();
                for (int i = 0; i < 100; i++)
                {
                    QtrOrder er = new QtrOrder();
                    string orderid = "000" + i.ToString();
                    orderid = orderid.Substring(orderid.Length - 3, 3);
                    er.orderID = orderid;
                    er.comment = $"T{orderid}";
                    er.deliverToCompID = "1";
                    er.execID = null;
                    er.execType = "F";
                    //er.firmflag = "1";
                    //er.lastPx = ComCommonUtils.ConvertToDecimal(ComCommonUtils.FormatNum( (3 + i*100/1000.0) .ToString("N4")));
                    //er.cleanPrice = 100+ CommonUtils.ConvertToDecimal(CommonUtils.FormatNum((3 + i * 100 / 1000.0).ToString("N4")))*10;
                    er.yield = er.lastPx;
                    er.lastQty = 0;
                    er.leavesQty = 0;
                    er.orderQty = 100 + i;
                    ///0 = 有效 New
                    ///1 = 部分成交Partially filled
                    ///2 = 全部成交 Filled
                    ///4 = 撤销 Canceled
                    ///8 = 拒绝 Reject
                    ///C = 过期 Expired"
                    switch (i % 6)
                    {
                        case 0:
                            er.ordStatus = "0";
                            /// 1 = TPSC 国利
                            /// 2 = CNEX 国际
                            /// 3 = CCTB 中诚
                            /// 4 = PTCN 平安
                            /// 5 = CTTJ 信唐
                            er.senderCompID = "1";
                            er.venues = "TPSC";

                            break;
                        case 1:
                            er.ordStatus = "1";
                            er.senderCompID = "2";
                            er.venues = "CNEX";
                            er.cumQty = 100 - i > 0 ? 100 - i : 10 ;// 总成交量
                            break;
                        case 2:
                            er.ordStatus = "2";
                            er.senderCompID = "3";
                            er.venues = "CCTB";
                            er.cumQty = 100 + i;
                            break;
                        case 3:
                            er.ordStatus = "4";
                            er.senderCompID = "4";
                            er.venues = "PTCN";
                            break;
                        case 4:
                            er.ordStatus = "8";
                            er.senderCompID = "5";
                            er.venues = "CTTJ";
                            break;
                        default:
                            er.ordStatus = "C";
                            er.senderCompID = "1";
                            er.venues = "TPSC";
                            break;
                    }
                    er.ordType = "2";
                    er.price = er.lastPx;

                    er.securityID = $"21021{(i % 10).ToString()}.IB";
                    er.symbol = $"21国开{(i % 10).ToString()}.IB";

                    switch (i % 3)
                    {
                        case 0:
                            //er.securityType = "BNC";
                            er.settlType = "1";
                            break;
                        case 1:
                            //er.securityType = "BCO";
                            er.settlType = "2";
                            break;
                        default:
                            //er.securityType = "NCD";
                            er.settlType = "3";
                            break;
                    }

                    er.sendingTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    er.transactTime = er.sendingTime;
                    er.settlDate = DateTime.Now.ToString("yyyy-MM-dd");
                    switch (i % 9)
                    {
                        case 0:
                            /// Bid：买入 
                            /// Ofr：卖出
                            /// 1=bid
                            /// 2=ofr
                            er.side = 1;
                            break;
                        case 1:
                            er.side = 2;
                            break;
                        default:
                            er.side = 2;
                            break;
                    }

                    er.tradeDate = er.settlDate;
                    er.deliverToSubID = $"张三{i.ToString()}";
                    er.marketIndicator = 4;
                    er.yieldType = "MATURITY";
                    lstdi.Add(er);
                }

                CacheQtrOrderModel = new ObservableCollection<QtrOrder>(lstdi);
                OnQtrOrder?.Invoke(lstdi);
            });

            // -------- 创建测试数据 -------------
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                List<QtrTrade> lstdi = new List<QtrTrade>();
                for (int i = 0; i < 100; i++)
                {
                    QtrTrade er = new QtrTrade();
                    string orderid = "000" + i.ToString();
                    orderid = orderid.Substring(orderid.Length - 3, 3);
                    er.dealID = $"{orderid}";
                    er.comment = $"T{orderid}";
                    er.bondShortName = $"21021{(i % 10).ToString()}.IB";
                    er.bondCode = $"21国开{(i % 10).ToString()}.IB";
                    //er.price = ComCommonUtils.ConvertToDecimal(ComCommonUtils.FormatNum((3 + i * 100 / 1000.0).ToString("N4")));
                    er.cleanPrice = 100 + er.price;
                    er.tradedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    switch (i % 9)
                    {
                        case 0:
                            /// Bid：买入 
                            /// Ofr：卖出
                            /// 1=bid
                            /// 2=ofr  GVN=given、TKN=taken、TRD=traded
                            er.direction = "GVN";
                            er.method = 1;
                            break;
                        case 1:
                            er.direction = "TKN";
                            er.method = 2;
                            break;
                        default:
                            er.direction = "TRD";
                            er.method = 3;
                            break;
                    }

                    switch (i % 6)
                    {
                        case 0:
                            /// 1 = TPSC 国利
                            /// 2 = CNEX 国际
                            /// 3 = CCTB 中诚
                            /// 4 = PTCN 平安
                            /// 5 = CTTJ 信唐
                            er.companyID = 1;
                            break;
                        case 1:
                            er.companyID = 2;
                            break;
                        case 2:
                            er.companyID = 3;
                            break;
                        case 3:
                            er.companyID = 4;
                            break;
                        case 4:
                            er.companyID = 5;
                            break;
                        default:
                            er.companyID = 1;
                            break;
                    }

                    lstdi.Add(er);
                }

                CacheQtrTradeModel = new ObservableCollection<QtrTrade>(lstdi);
                OnQtrTrade?.Invoke(lstdi);

            });

        }

        private void DyChgTestData()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                int i = 0;
                foreach (QtrOrder er in CacheQtrOrderModel)
                {
                    ///0 = 有效 New
                    ///1 = 部分成交Partially filled
                    ///2 = 全部成交 Filled
                    ///4 = 撤销 Canceled
                    ///8 = 拒绝 Reject
                    ///C = 过期 Expired"
                    //if (i % 2 == 0)
                    {
                        switch (er.ordStatus)
                        {
                            //0 = 有效 New
                            case "0":
                                er.ordStatus = "1";
                                //er.updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                er.cumQty = er.orderQty - 10;
                                //er.leavesQt = er.orderQty - er.cumQty;
                                er.lastPx = er.price - 0.005M;
                                //er.transactTime = er.updatetime;
                                
                                //er.comment = er.updatetime;
                                break;
                            case "1":
                                er.ordStatus = "2";
                                //er.updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                er.cumQty = er.orderQty;
                                //er.leavesQt = 0;
                                er.lastPx = er.price - 0.005M;
                                //er.transactTime = er.updatetime;
                                //er.comment = er.updatetime;
                                break;

                            case "4":
                                er.ordStatus = "0";
                                //er.updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                er.cumQty = er.orderQty;
                                //er.leavesQt = 0;
                                er.lastPx = er.price - 0.005M;
                                //er.transactTime = er.updatetime;
                                //er.comment = er.updatetime;
                                break;
                        }

                        List<QtrOrder> lstdi = new List<QtrOrder>();
                        lstdi.Add(er);
                        OnQtrOrder?.Invoke(lstdi);

                        //List<QtrTrade> lstddo = JsonConvert.DeserializeObject<List<QtrTrade>>(JsonConvert.SerializeObject(lstdi));
                        //OnDealDoneOrder?.Invoke(lstddo);

                        //Thread.Sleep(10);
                    }

                    if (i % 6 == 0)
                    {

                    }

                    if (i % 8 == 0)
                    {

                    }

                    i++;
                }
            });
        }

        #endregion
    }
}
