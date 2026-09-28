using STC.Common.Handler.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.DataProcesser
{
    /// <summary>
    ///  Ems订单数据
    /// </summary>
    public class EmsOrderProcesser
    {
        /// <summary>
        /// Ems订单处理器实例
        /// </summary>
        private static EmsOrderProcesser proc = new EmsOrderProcesser();

        /// <summary>
        /// 实例
        /// </summary>
        public static EmsOrderProcesser Processer
        {
            get
            {
                return proc;
            }
        }

        /// <summary>
        /// 是否初始化
        /// </summary>
        private volatile bool isInit = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        private EmsOrderProcesser()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~EmsOrderProcesser()
        {
            Dispose();
        }

        /// <summary>
        /// 本机构订单数量统计
        /// key: SecurityID
        /// value: List(MyBidCount， MyPartyBidCount， MyOfrCount, MyPartyOfrCount)
        /// </summary>
        private ConcurrentDictionary<string, List<int>> _MyPartyCountDicBySecurityID = new ConcurrentDictionary<string, List<int>>();
        /// <summary>
        /// 本机构订单数量统计
        /// key: securityID#settlType#venues#venueTypeGw#entryPx#side
        /// value: key维度上的总数量
        /// </summary>
        private ConcurrentDictionary<string, int> _MyPartyCountDic = new ConcurrentDictionary<string, int>();
        ///// <summary>
        ///// 缓存最新的成交
        ///// key：SecurrityId
        ///// value: ExecutionReport
        ///// </summary>
        //private Dictionary<string, ExecutionReport> _LastExecutionDic = new Dictionary<string, ExecutionReport>();

        /// <summary>
        /// detaildv资源锁
        /// </summary>
        private object detailDvLockObj = new object();
        public object DetailDvLockObj
        {
            get
            {
                return detailDvLockObj;
            }
        }



        #region 表格字段
        public const string KEY_FIELD = "Key";// 可能是OrderID
        public const string ORDERID_FIELD = "OrderID";
        public const string SECURITY_ID_FIELD = "SecurityID";
        public const string SIDE_FIELD = "Side";
        public const string PX_FIELD = "Px";
        public const string NET_PRICE_FIELD = "NetPrice";
        public const string QTY_FIELD = "Qty";
        public const string SETTLE_TYPE_FIELD = "SettleType";
        public const string USER_NAME_FIELD = "UserName";//本方交易员
        public const string ORDER_STATUS_FIELD = "OrdStatus";
        public const string VENUES_FIELD = "Venues";
        public const string VENUES_TYPE_GW_FIELD = "VenuesTypeGW";
        public const string IS_ME_FIELD = "IsMe"; // 是否是自己的单子
        public const string STRATEGYORDERID_FIELD = "StrategyOrderId"; // 
        public const string CLORDID_FIELD = "ClOrdID";
        public const string TRANSACTTIME_FIELD = "TransactTime";
        #endregion

        private DataTable _DetailDT;
        public DataTable DetailDT
        {
            get
            {
                lock (EmsOrderProcesser.Processer.DetailDvLockObj)
                {
                    if (_DetailDT == null)
                    {
                        _DetailDT = new DataTable();
                        _DetailDT.Columns.Add(KEY_FIELD, typeof(string));
                        _DetailDT.Columns.Add(ORDERID_FIELD, typeof(string));
                        _DetailDT.Columns.Add(SECURITY_ID_FIELD, typeof(string));
                        _DetailDT.Columns.Add(SIDE_FIELD, typeof(string));
                        _DetailDT.Columns.Add(PX_FIELD, typeof(decimal));
                        _DetailDT.Columns.Add(NET_PRICE_FIELD, typeof(decimal));
                        _DetailDT.Columns.Add(QTY_FIELD, typeof(long));
                        _DetailDT.Columns.Add(SETTLE_TYPE_FIELD, typeof(string));
                        _DetailDT.Columns.Add(USER_NAME_FIELD, typeof(string));
                        _DetailDT.Columns.Add(ORDER_STATUS_FIELD, typeof(string));
                        _DetailDT.Columns.Add(VENUES_FIELD, typeof(string));
                        _DetailDT.Columns.Add(VENUES_TYPE_GW_FIELD, typeof(string));
                        _DetailDT.Columns.Add(IS_ME_FIELD, typeof(Visibility));
                        _DetailDT.Columns.Add(STRATEGYORDERID_FIELD, typeof(string));
                        _DetailDT.Columns.Add(CLORDID_FIELD, typeof(string));
                        _DetailDT.Columns.Add(TRANSACTTIME_FIELD, typeof(string));

                        _DetailDT.PrimaryKey = new DataColumn[] { _DetailDT.Columns[KEY_FIELD] };
                    }
                }
                return _DetailDT;
            }
        }

        AsyncThreadQueue<ExecutionReport> _ProcessMarketExecutioinsThreadQueue;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (isInit == false)
            {
                if (_ProcessMarketExecutioinsThreadQueue == null)
                {
                    _ProcessMarketExecutioinsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateEmsOrders);
                }

                //#region 创建测试数据
                //this.GenerateTestData();
                //return;
                //#endregion


                //#region 收Ems订单消息
                //EmsOrdersHandler.Instance.OnEmsOrders += Instance_OnEmsOrders;
                //#endregion

                isInit = true;
            }
        }

        /// <summary>
        /// 清楚数据
        /// </summary>
        public void Clear()
        {
            //EmsOrdersHandler.Instance.OnEmsOrders -= Instance_OnEmsOrders;

            lock (this.detailDvLockObj)
            {
                if (_DetailDT != null)
                {
                    _DetailDT.Rows.Clear();
                }
            }

            if (_MyPartyCountDic != null)
            {
                _MyPartyCountDic.Clear();
            }

            if (_MyPartyCountDicBySecurityID != null)
            {
                foreach(List<int> list in _MyPartyCountDicBySecurityID.Values)
                {
                    list.Clear();
                }
                _MyPartyCountDicBySecurityID.Clear();
            }

            if (_ProcessMarketExecutioinsThreadQueue != null)
            {
                _ProcessMarketExecutioinsThreadQueue.Clear();
            }

            isInit = false;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.Clear();

            lock (this.detailDvLockObj)
            {
                if (_DetailDT != null)
                {
                    _DetailDT.Clear();
                    _DetailDT.Dispose();
                    _DetailDT = null;
                }
            }

            if (_MyPartyCountDic != null)
            {
                _MyPartyCountDic.Clear();
                _MyPartyCountDic = null;
            }

            if (_MyPartyCountDicBySecurityID != null)
            {
                foreach (List<int> list in _MyPartyCountDicBySecurityID.Values)
                {
                    list.Clear();
                }
                _MyPartyCountDicBySecurityID.Clear();
                _MyPartyCountDicBySecurityID = null;
            }

            if (_ProcessMarketExecutioinsThreadQueue != null)
            {
                _ProcessMarketExecutioinsThreadQueue.Clear();
                _ProcessMarketExecutioinsThreadQueue.Dispose();
                _ProcessMarketExecutioinsThreadQueue = null;
            }

            isInit = false;
        }

        /// <summary>
        /// 清零数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("EmsOrderProcesser Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                    this.Clear();
                    this.Init();
                }));
            }
        }

        /// <summary>
        /// Ems订单消息回调接口
        /// </summary>
        /// <param name="obj"></param>
        public void Instance_OnEmsOrders(ExecutionReport obj)
        {
            if (_ProcessMarketExecutioinsThreadQueue != null)
            {
                _ProcessMarketExecutioinsThreadQueue.Enqueue(obj);
            }
        }

        /// <summary>
        /// 接收Ems订单消息
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateEmsOrders(ExecutionReport obj)
        {
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            lock (this.detailDvLockObj)
            {
                try
                {
                    Console.WriteLine(string.Format("EmsOrderProcessor------------OrderId:{0}, netPrice:{1}, netPrice2:{2}", obj.OrderID, obj.NetPrice, obj.NetPrice2));
                    this.Sub_UpdateEmsOrders(obj);
                    if (!string.IsNullOrEmpty(obj.OrderQty2))
                    {
                        ExecutionReport er = obj.Clone();
                        CopyOrder(er);
                        Console.WriteLine(string.Format("EmsOrderProcessor-------er-----OrderId:{0}, netPrice:{1}, netPrice2:{2}", er.OrderID, er.NetPrice, er.NetPrice2));
                        this.Sub_UpdateEmsOrders(er);
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            }
            //}));
        }

        private void CopyOrder(ExecutionReport copyOrder)
        {
            if (!string.IsNullOrEmpty(copyOrder.OrderQty2))
            {
                copyOrder.Price = copyOrder.Price2;
                copyOrder.NetPrice = copyOrder.NetPrice2;
                copyOrder.OrderQty = copyOrder.OrderQty2;
                copyOrder.LeavesQty = copyOrder.LeavesQty2;
                copyOrder.CumQty = copyOrder.CumQty2;
                copyOrder.LastPx = copyOrder.LastPx2;
                copyOrder.LastQty = copyOrder.LastQty2;
                copyOrder.ExecType = copyOrder.ExecType2;
                copyOrder.AvgPx = copyOrder.AvgPx2;
                copyOrder.Yield = copyOrder.Yield2;
                copyOrder.YieldToExecution = copyOrder.YieldToExecution2;

                copyOrder.OrderID = copyOrder.OrderID + CommonConstants.OrderEnd;
                copyOrder.Side = copyOrder.Side == "1" ? "2" : "1";
            }
        }

        /// <summary>
        /// 处理Ems订单消息
        /// </summary>
        /// <param name="obj"></param>
        private void Sub_UpdateEmsOrders(ExecutionReport obj)
        {
            // securityID#settlType#venues#venueTypeGw#entryPx#side
            // string conditionKey = $"{obj.SecurityID}#{obj.SettlType}#{obj.Venues}#{obj.VenueTypeGW}#{CommonUtils.ConvertToDecimal(obj.Price).ToString("#.########")}#{obj.Side}";
            string conditionKey = $"{obj.SecurityID}#{obj.SettlType}#{obj.Venues}#{CommonUtils.ConvertToDecimal(obj.Price).ToString("#.########")}#{obj.Side}#{obj.PartyName}";

            string primaryKey = $"{obj.VenueTypeGW}#{obj.OrderID}";
            DataRow findDr = this.DetailDT.Rows.Find(primaryKey);

            if (findDr != null)
            {
                //if (CommonUtils.ConvertToString(findDr[TRANSACTTIME_FIELD]).CompareTo(obj.TransactTime) > 0)
                //{
                //    // 如果时间早， 就不更新了。
                //    return;
                //}
                if (obj.OrdStatus != "0" && obj.OrdStatus != "1")
                {
                    if (_MyPartyCountDic.ContainsKey(conditionKey))
                    {
                        _MyPartyCountDic[conditionKey]--;

                        if (_MyPartyCountDic[conditionKey] == 0)
                        {
                            int removeCount = 0;
                            _MyPartyCountDic.TryRemove(conditionKey, out removeCount);
                        }
                    }

                    if (_MyPartyCountDicBySecurityID.ContainsKey(obj.SecurityID))
                    {
                        if (obj.Side == "1") // bid
                        {
                            if (obj.UserName == LocalEnv.UserInfo.UserName)
                            {
                                _MyPartyCountDicBySecurityID[obj.SecurityID][0]--;
                            }
                            else
                            {
                                _MyPartyCountDicBySecurityID[obj.SecurityID][1]--;
                            }
                        }
                        else if (obj.Side == "2") // ofr
                        {
                            if (obj.UserName == LocalEnv.UserInfo.UserName)
                            {
                                _MyPartyCountDicBySecurityID[obj.SecurityID][2]--;
                            }
                            else
                            {
                                _MyPartyCountDicBySecurityID[obj.SecurityID][3]--;
                            }
                        }

                        if (_MyPartyCountDicBySecurityID[obj.SecurityID][0] == 0
                            && _MyPartyCountDicBySecurityID[obj.SecurityID][1] == 0
                            && _MyPartyCountDicBySecurityID[obj.SecurityID][2] == 0
                            && _MyPartyCountDicBySecurityID[obj.SecurityID][3] == 0)
                        {
                            List<int> removeCountList = null;
                            _MyPartyCountDicBySecurityID.TryRemove(obj.SecurityID, out removeCountList);
                        }
                    }

                    this.DetailDT.Rows.Remove(findDr);
                    return;
                }
            }

            if (findDr == null)
            {
                if (obj.OrdStatus != "0" && obj.OrdStatus != "1")
                {
                    return;
                }

                // 新建计数器
                if (_MyPartyCountDic.ContainsKey(conditionKey) == false)
                {
                    _MyPartyCountDic[conditionKey] = 0;
                }

                // securtityID
                if (_MyPartyCountDicBySecurityID.ContainsKey(obj.SecurityID) == false)
                {
                    _MyPartyCountDicBySecurityID[obj.SecurityID] = new List<int>(new int[] { 0, 0, 0, 0 });
                }


                // 统计记录
                _MyPartyCountDic[conditionKey]++;
                if (obj.Side == "1") // bid
                {
                    if (obj.UserName == LocalEnv.UserInfo.UserName)
                    {
                        _MyPartyCountDicBySecurityID[obj.SecurityID][0]++;
                    }
                    else
                    {
                        _MyPartyCountDicBySecurityID[obj.SecurityID][1]++;
                    }
                }
                else if (obj.Side == "2") // ofr
                {
                    if (obj.UserName == LocalEnv.UserInfo.UserName)
                    {
                        _MyPartyCountDicBySecurityID[obj.SecurityID][2]++;
                    }
                    else
                    {
                        _MyPartyCountDicBySecurityID[obj.SecurityID][3]++;
                    }
                }

                // 添加记录
                findDr = this.DetailDT.NewRow();
                findDr[KEY_FIELD] = primaryKey;
                findDr[ORDERID_FIELD] = obj.OrderID;
                findDr[IS_ME_FIELD] = (obj.UserName == LocalEnv.UserInfo.UserName) == true ? Visibility.Visible : Visibility.Hidden;
                findDr[SECURITY_ID_FIELD] = obj.SecurityID;
                findDr[SIDE_FIELD] = obj.Side; //这里要注意， 1:买 2：卖； 深度行情是 0：bid； 1：Ofr
                findDr[PX_FIELD] = CommonUtils.ConvertToDecimal(obj.Price); // 收益率
                findDr[NET_PRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价
                findDr[QTY_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty); // 数量
                findDr[SETTLE_TYPE_FIELD] = obj.SettlType;
                findDr[USER_NAME_FIELD] = obj.PartySubName;
                findDr[ORDER_STATUS_FIELD] = obj.OrdStatus;
                findDr[VENUES_FIELD] = obj.Venues;
                findDr[VENUES_TYPE_GW_FIELD] = obj.VenueTypeGW;
                findDr[STRATEGYORDERID_FIELD] = obj.StrategyOrderId;
                findDr[CLORDID_FIELD] = obj.ClOrdID;
                this.DetailDT.Rows.Add(findDr);
            }
            else
            {
                //if (CommonUtils.ConvertToString(findDr[TRANSACTTIME_FIELD]).CompareTo(obj.TransactTime) > 0)
                //{
                //    // 如果时间早， 就不更新了。
                //    return;
                //}
                findDr[IS_ME_FIELD] = (obj.UserName == LocalEnv.UserInfo.UserName) == true ? Visibility.Visible : Visibility.Hidden;
                findDr[SECURITY_ID_FIELD] = obj.SecurityID;
                findDr[SIDE_FIELD] = obj.Side; //这里要注意， 1:买 2：卖； 深度行情是 0：bid； 1：Ofr
                findDr[PX_FIELD] = CommonUtils.ConvertToDecimal(obj.Price); // 收益率
                findDr[NET_PRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价
                findDr[QTY_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty); // 数量
                findDr[SETTLE_TYPE_FIELD] = obj.SettlType;
                findDr[USER_NAME_FIELD] = obj.PartySubName;
                findDr[ORDER_STATUS_FIELD] = obj.OrdStatus;
                findDr[VENUES_FIELD] = obj.Venues;
                findDr[VENUES_TYPE_GW_FIELD] = obj.VenueTypeGW;
                findDr[STRATEGYORDERID_FIELD] = obj.StrategyOrderId;
                findDr[CLORDID_FIELD] = obj.ClOrdID;
            }
        }

        /// <summary>
        /// 根据输入条件， 计算符合该条件的Ems数据条数
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="settlType"></param>
        /// <param name="venues"></param>
        /// <param name="venueTypeGw"></param>
        /// <param name="entryPx"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public int GetCacheEmsOrders(string securityID, string settlType, string venues, string venueTypeGw, decimal entryPx, string side, string pubsubname = null, decimal yieldExe = decimal.Zero)
        {

            int sumCount = 0;
            string[] arr = settlType.Split(',');
            foreach (string subSettlType in arr)
            {
                if (string.IsNullOrEmpty(subSettlType) == false)
                {
                    //string conditionKey = $"{securityID}#{settlType}#{venues}#{venueTypeGw}#{entryPx.ToString("#.########")}#{side}";
                    string conditionKey = $"{securityID}#{settlType}#{venues}#{entryPx.ToString("#.########")}#{side}#{pubsubname}";
                    if (_MyPartyCountDic.ContainsKey(conditionKey))
                    {
                        sumCount += _MyPartyCountDic[conditionKey];
                    }
                    if (venues.Equals("QDMESP") && sumCount == 0)
                    {
                        conditionKey = $"{securityID}#{settlType}#{venues}#{yieldExe.ToString("#.########")}#{side}#{pubsubname}";
                        if (_MyPartyCountDic.ContainsKey(conditionKey))
                        {
                            sumCount += _MyPartyCountDic[conditionKey];
                        }
                    }
                }
            }
            return sumCount;
        }


        /// <summary>
        /// 根据securityID , 获得本机构订单数量统计
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns>List(MyBidCount， MyPartyBidCount， MyOfrCount, MyPartyOfrCount)</returns>
        public List<int> GetMyPartyCountBySecurityID(string securityID)
        {
            if (_MyPartyCountDicBySecurityID.ContainsKey(securityID))
            {
                return _MyPartyCountDicBySecurityID[securityID];
            }
            return null;
        }
    }
}
