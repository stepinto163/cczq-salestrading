using STC.Common.Handler.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Model.Base;
using System.Data;
using STC.Common.Model.Fix.Message;
using System.Threading;
using System.Windows;
using System.Collections.Concurrent;
using STC.Common.Utils;
using STC.Common.Data;

namespace STC.Common.DataProcesser
{
    /// <summary>
    /// Ems数据处理器（包含Ems订单和Ems成交单）
    /// </summary>
    public class CrossOrdersProcesser
    {
        /// <summary>
        /// Ems数据处理器实例
        /// </summary>
        private static CrossOrdersProcesser proc = new CrossOrdersProcesser();

        /// <summary>
        /// 单例
        /// </summary>
        public static CrossOrdersProcesser Processer
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

        // 订阅Ems订单
        private readonly Object m_OnWarningOrdersEventLock = new Object();
        // string: strategyOrderID
        private Action<string> _OnWarningOrders;
        public event Action<string> OnWarningOrders
        {
            add
            {
                lock (m_OnWarningOrdersEventLock)
                {
                    _OnWarningOrders += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in warningStatusDic)
                    {
                        _OnWarningOrders?.Invoke(obj.Key);
                    }
                });
            }
            remove
            {
                lock (m_OnWarningOrdersEventLock)
                {
                    _OnWarningOrders -= value;
                }
            }
        }

        //key: strategyOrderID； value: 0:正常 1:不正常 
        private ConcurrentDictionary<string, int> warningStatusDic = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 通过strategyOrderID获取是否处于警告状态
        /// 0： 正常  1：警告
        /// </summary>
        /// <param name="strategyOrderID"></param>
        /// <returns></returns>
        public int GetWarningStatusByStrategyOrderID(string strategyOrderID)
        {
            int flag = 0;
            if (warningStatusDic.ContainsKey(strategyOrderID))
            {
                flag = warningStatusDic[strategyOrderID] > 0 ? 1 : 0;
            }
            return flag;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        private CrossOrdersProcesser()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~CrossOrdersProcesser()
        {
            Dispose();
        }

        /// <summary>
        /// key: Venues+StrategyOrderId, value: blocknum 
        /// </summary>
        private Dictionary<string, int> blocknumDic = new Dictionary<string, int>();

        /// <summary>
        /// key: canceled strategyID,  value : null 无用
        /// </summary>
        private Dictionary<string, string> cancelStcIDDic = new Dictionary<string, string>();

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
        public const string KEY_FIELD = "Key";
        public const string STATUS_FIELD = "Status";
        public const string ORDERID_FIELD = "OrderID";
        public const string VENUES_FIELD = "Venues";
        public const string PRICE_FIELD = "Price";
        public const string DISPLAY_PRICE_FIELD = "Price_Display";
        public const string NETPRICE_FIELD = "NetPrice";
        public const string QTY_FIELD = "Qty";
        public const string EDIT_NETPRICE_FIELD = "Edit_NetPrice";
        public const string DISPLAYBTN_FIELD = "DisplayBtn";
        public const string CLORDID_FIELD = "ClOrdID";
        public const string STRATEGYORDERID_FIELD = "StrategyOrderId";
        public const string TRANSACTTIME_FIELD = "TransactTime";
        public const string BLOCK_NUM = "blocknum";
        public const string SIDE = "Side";
        public const string MARKETINDICATOR_FIELD = "MarketIndicator";
        public const string BOOKID_FIELD = "BookID"; 
        //public const string TIMEINFORCE_FIELD = "timeinforce";
        #endregion

        private DataTable _CrossDt;
        public DataTable CrossDt
        {
            get
            {
                if (this._CrossDt == null)
                {
                    _CrossDt = new DataTable();
                    _CrossDt.Columns.Add(KEY_FIELD, typeof(string));
                    _CrossDt.Columns.Add(STATUS_FIELD, typeof(string));
                    _CrossDt.Columns.Add(ORDERID_FIELD, typeof(string));
                    _CrossDt.Columns.Add(VENUES_FIELD, typeof(string));
                    _CrossDt.Columns.Add(DISPLAY_PRICE_FIELD, typeof(string));
                    _CrossDt.Columns.Add(PRICE_FIELD, typeof(string));
                    _CrossDt.Columns.Add(NETPRICE_FIELD, typeof(decimal));
                    _CrossDt.Columns.Add(QTY_FIELD, typeof(long));
                    _CrossDt.Columns.Add(EDIT_NETPRICE_FIELD, typeof(decimal));
                    _CrossDt.Columns.Add(DISPLAYBTN_FIELD, typeof(string));
                    _CrossDt.Columns.Add(CLORDID_FIELD, typeof(string));
                    _CrossDt.Columns.Add(STRATEGYORDERID_FIELD, typeof(string));
                    _CrossDt.Columns.Add(TRANSACTTIME_FIELD, typeof(string));
                    _CrossDt.Columns.Add(BLOCK_NUM, typeof(string));
                    _CrossDt.Columns.Add(SIDE, typeof(string));
                    //_CrossDt.Columns.Add(TIMEINFORCE_FIELD, typeof(string));
                    _CrossDt.Columns.Add(MARKETINDICATOR_FIELD, typeof(string));
                    _CrossDt.Columns.Add(BOOKID_FIELD, typeof(string));

                    _CrossDt.PrimaryKey = new DataColumn[] { _CrossDt.Columns[KEY_FIELD] };
                    //this.OnPropertyChanged();
                }

                return _CrossDt;
            }
        }

        AsyncThreadQueue<ExecutionReport> _ProcessEmsOrdersThreadQueue;
        AsyncThreadQueue<ExecutionReport> _ProcessMyExecutionThreadQueue;

        /// <summary>
        /// 资源锁
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (isInit == false)
            {
                if (_ProcessMyExecutionThreadQueue == null)
                {
                    _ProcessMyExecutionThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateMyExecutions);
                }

                if (_ProcessEmsOrdersThreadQueue == null)
                {
                    _ProcessEmsOrdersThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateEmsOrders);
                }

                //#region 创建测试数据
                //this.GenerateTestData();
                //return;
                //#endregion

                #region 收Exe订单消息
                ExecutionsHandler.Instance.OnOwnExecutions += Instance_OnOwnExecutions;
                #endregion

                //#region 收Ems订单消息
                //EmsOrdersHandler.Instance.OnEmsOrders += Instance_OnEmsOrders;
                //#endregion

                isInit = true;
            }
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            ExecutionsHandler.Instance.OnOwnExecutions -= Instance_OnOwnExecutions;
            //EmsOrdersHandler.Instance.OnEmsOrders -= Instance_OnEmsOrders;

            lock (this.detailDvLockObj)
            {
                if (_CrossDt != null)
                {
                    _CrossDt.Rows.Clear();
                }
            }

            if (_ProcessMyExecutionThreadQueue != null)
            {
                _ProcessMyExecutionThreadQueue.Clear();
            }

            if (_ProcessEmsOrdersThreadQueue != null)
            {
                _ProcessEmsOrdersThreadQueue.Clear();
            }

            isInit = false;
        }

        /// <summary>
        /// 释放数据
        /// </summary>
        public void Dispose()
        {
            this.Clear();

            lock (this.detailDvLockObj)
            {
                if (_CrossDt != null)
                {
                    _CrossDt.Clear();
                    _CrossDt.Dispose();
                    _CrossDt = null;
                }
            }

            if (_ProcessMyExecutionThreadQueue != null)
            {
                _ProcessMyExecutionThreadQueue.Clear();
                _ProcessMyExecutionThreadQueue.Dispose();
                _ProcessMyExecutionThreadQueue = null;
            }

            if (_ProcessEmsOrdersThreadQueue != null)
            {
                _ProcessEmsOrdersThreadQueue.Clear();
                _ProcessEmsOrdersThreadQueue.Dispose();
                _ProcessEmsOrdersThreadQueue = null;
            }
        }

        /// <summary>
        /// 清零消息回调
        /// </summary>
        /// <param name="obj">清零消息</param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("CrossOrderProcesser Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    this.Init();
                }));
            }
        }

        /// <summary>
        /// 我的成交数据回调方法
        /// </summary>
        /// <param name="obj">Ems成交数据</param>
        private void Instance_OnOwnExecutions(ExecutionReport obj)
        {
            if (this._ProcessMyExecutionThreadQueue != null)
            {
                if (obj.UserName == LocalEnv.UserInfo.UserName)
                {
                    this._ProcessMyExecutionThreadQueue.Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// 更新我的成交
        /// </summary>
        /// <param name="obj">Ems成交数据</param>
        private void UpdateMyExecutions(ExecutionReport obj)
        {
            lock (this.detailDvLockObj)
            {
                try
                {
                    //this.Sub_UpdateMyExecutions(obj);

                    this.Sub_UpdateMyExecutions2(obj);

                }
                catch (Exception e)
                {
                    LogHandler.Error("CrossOrdersProcesser UpdateMyExecutions error", e);
                }
            }
        }

        /// <summary>
        /// 更新我的成交数据
        /// </summary>
        /// <param name="obj">Ems成交数据</param>
        private void Sub_UpdateMyExecutions2(Common.Model.Fix.Message.ExecutionReport obj)
        {
            string primaryKey = $"{obj.VenueTypeGW}#{obj.OrderID}"; 
            DataRow[] dataRows = this.CrossDt.Select($"{KEY_FIELD} = '{primaryKey}'");
            if (dataRows != null && dataRows.Length > 0)
            {
                foreach (DataRow myExecution in dataRows)
                {
                    if (CommonUtils.ConvertToString(myExecution[TRANSACTTIME_FIELD]).CompareTo(obj.TransactTime) > 0)
                    {
                        // 如果时间早， 就不更新了。
                        continue;
                    }


                    myExecution[VENUES_FIELD] = obj.Venues;
                    myExecution[BLOCK_NUM] = $"{obj.BlockNum}:{obj.OrdStatus}:{obj.ExecType}:{obj.TimeInForce}";
                    //myExecution[TIMEINFORCE_FIELD] = obj.TimeInForce;

                    myExecution[DISPLAY_PRICE_FIELD] = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
                    myExecution[PRICE_FIELD] = obj.Yield;//CommonUtils.ConvertToDecimal(obj.Yield);// 成交价
                    myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);  // 净价
                    myExecution[EDIT_NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价（编辑）
                    myExecution[QTY_FIELD] = CommonUtils.ConvertToDecimal(obj.LastQty); // 成交量
                    myExecution[CLORDID_FIELD] = obj.ClOrdID;

                    myExecution[SIDE] = obj.Side;
                    myExecution[DISPLAYBTN_FIELD] = Visibility.Collapsed;
                    myExecution[MARKETINDICATOR_FIELD] = obj.MarketIndicator;
                    if (Consts.MARKETINDICATOR_FUTURE.Equals(obj.MarketIndicator))
                    {
                        //国债期货 成交价(%) 不显示
                        myExecution[DISPLAY_PRICE_FIELD] = DBNull.Value;
                        //国债期货 净价显示成交价
                        myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);
                    }
                }
            }
            else
            {
                // 添加记录
                DataRow myExecution = this.CrossDt.NewRow();
                myExecution[ORDERID_FIELD] = obj.OrderID;
                myExecution[STATUS_FIELD] = "已成交";
                myExecution[STRATEGYORDERID_FIELD] = obj.StrategyOrderId;
                myExecution[KEY_FIELD] = primaryKey;

                this.CrossDt.Rows.Add(myExecution);


                myExecution[VENUES_FIELD] = obj.Venues;
                myExecution[BLOCK_NUM] = $"{obj.BlockNum}:{obj.OrdStatus}:{obj.ExecType}:{obj.TimeInForce}";
                //myExecution[TIMEINFORCE_FIELD] = obj.TimeInForce;

                myExecution[DISPLAY_PRICE_FIELD] = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
                myExecution[PRICE_FIELD] = obj.Yield;//CommonUtils.ConvertToDecimal(obj.Yield);// 成交价
                myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);  // 净价
                myExecution[EDIT_NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价（编辑）
                myExecution[QTY_FIELD] = CommonUtils.ConvertToDecimal(obj.LastQty); // 成交量
                myExecution[CLORDID_FIELD] = obj.ClOrdID;

                myExecution[SIDE] = obj.Side;
                myExecution[DISPLAYBTN_FIELD] = Visibility.Collapsed;
                myExecution[MARKETINDICATOR_FIELD] = obj.MarketIndicator;
                if (Consts.MARKETINDICATOR_FUTURE.Equals(obj.MarketIndicator))
                {
                    //国债期货 成交价(%) 不显示
                    myExecution[DISPLAY_PRICE_FIELD] = DBNull.Value;
                    //国债期货 净价显示成交价
                    myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);
                }
            }
        }


        /// <summary>
        /// 更新我的成交数据
        /// </summary>
        /// <param name="obj">Ems成交数据</param>
        private void Sub_UpdateMyExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            string primaryKey = $"{obj.VenueTypeGW}#{obj.OrderID}";
            DataRow myExecution = this.CrossDt.Rows.Find(primaryKey);
            if (myExecution != null)
            {
                if (CommonUtils.ConvertToString(myExecution[TRANSACTTIME_FIELD]).CompareTo(obj.TransactTime) > 0)
                {
                    // 如果时间早， 就不更新了。
                    return;
                }
            }
            else
            {
                // 添加记录
                myExecution = this.CrossDt.NewRow();
                myExecution[ORDERID_FIELD] = obj.OrderID;
                myExecution[STATUS_FIELD] = "已成交";
                myExecution[STRATEGYORDERID_FIELD] = obj.StrategyOrderId;

                this.CrossDt.Rows.Add(myExecution);
            }



            myExecution[VENUES_FIELD] = obj.Venues;
            myExecution[BLOCK_NUM] = $"{obj.BlockNum}:{obj.OrdStatus}:{obj.ExecType}:{obj.TimeInForce}";
            //myExecution[TIMEINFORCE_FIELD] = obj.TimeInForce;

            myExecution[DISPLAY_PRICE_FIELD] = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
            myExecution[PRICE_FIELD] = obj.Yield;//CommonUtils.ConvertToDecimal(obj.Yield);// 成交价
            myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);  // 净价
            myExecution[EDIT_NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价（编辑）
            myExecution[QTY_FIELD] = CommonUtils.ConvertToDecimal(obj.LastQty); // 成交量
            myExecution[CLORDID_FIELD] = obj.ClOrdID;

            myExecution[SIDE] = obj.Side;
            myExecution[DISPLAYBTN_FIELD] = Visibility.Collapsed;
            myExecution[MARKETINDICATOR_FIELD] = obj.MarketIndicator;
            if (Consts.MARKETINDICATOR_FUTURE.Equals(obj.MarketIndicator))
            {
                //国债期货 成交价(%) 不显示
                myExecution[DISPLAY_PRICE_FIELD] = DBNull.Value;
                //国债期货 净价显示成交价
                myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);
            }
        }

        /// <summary>
        /// Ems订单数据回调方法
        /// </summary>
        /// <param name="obj">Ems订单数据</param>
        public void Instance_OnEmsOrders(ExecutionReport obj)
        {
            if (_ProcessEmsOrdersThreadQueue != null)
            {
                if (obj.UserName == LocalEnv.UserInfo.UserName)
                {
                    _ProcessEmsOrdersThreadQueue.Enqueue(obj);
                }
            }
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
        /// 更新Ems订单
        /// </summary>
        /// <param name="obj">Ems订单数据</param>
        private void UpdateEmsOrders(ExecutionReport obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Console.WriteLine(string.Format("------------OrderId:{0}, netPrice:{1}, netPrice2:{2}", obj.OrderID, obj.NetPrice, obj.NetPrice2));
                this.Sub_UpdateEmsOrders(obj);

                if (!string.IsNullOrEmpty(obj.OrderQty2))
                {
                    ExecutionReport er = obj.Clone();
                    CopyOrder(er);
                    Console.WriteLine(string.Format("-------er-----OrderId:{0}, netPrice:{1}, netPrice2:{2}", er.OrderID, er.NetPrice, er.NetPrice2));
                    this.Sub_UpdateEmsOrders(er);
                }
            }));
        }

        /// <summary>
        /// 更新Ems订单数据
        /// </summary>
        /// <param name="obj">Ems订单数据</param>
        private void Sub_UpdateEmsOrders(ExecutionReport obj)
        {
            try
            {
                lock (lockObj)
                {
                    if (cancelStcIDDic.ContainsKey(obj.StrategyOrderId))
                    {
                        return;
                    }

                    ///  判断是否要剔除所有过时数据，使用blocknum比较
                    ///  TODO: 如果母单不是有效订单了，这个需要移除
                    string blocknumDicKey = string.Format("{0}-{1}", obj.Venues, obj.StrategyOrderId);// obj.StrategyOrderId;
                    string strategyOrderID = obj.StrategyOrderId;
                    string currentBlocknum = obj.BlockNum;
                    bool needEliminate = false;

                    int newBlockNum = CommonUtils.ConvertToInt(currentBlocknum);
                    int oldBlockNum = 0;
                    lock (blocknumDic)
                    {
                        if (blocknumDic.ContainsKey(blocknumDicKey))
                        {
                            oldBlockNum = blocknumDic[blocknumDicKey];
                            if (newBlockNum > oldBlockNum)
                            {
                                needEliminate = true;
                                blocknumDic[blocknumDicKey] = newBlockNum;
                            }
                        }
                        else
                        {
                            blocknumDic[blocknumDicKey] = newBlockNum;
                        }
                    }
                    


                    lock (this.detailDvLockObj)
                    {
                        string primaryKey = $"{obj.VenueTypeGW}#{obj.OrderID}";
                        DataRow findDr = this.CrossDt.Rows.Find(primaryKey);
                        if (!(obj.OrdStatus == "N" || obj.OrdStatus == "0" || obj.OrdStatus == "1")
                            && !((obj.OrdStatus == "4" || obj.OrdStatus == "8" || obj.OrdStatus == "C") && obj.TimeInForce == "0" && newBlockNum >= oldBlockNum)
                            && findDr == null)
                        {
                            return;
                        }

                        if (needEliminate)
                        {

                            DataRow[] drs = this.CrossDt.Select($"{STRATEGYORDERID_FIELD} = '{strategyOrderID}' and {VENUES_FIELD} = '{obj.Venues}'");
                            List<DataRow> removeDrsList = new List<DataRow>();
                            foreach (DataRow dr in drs)
                            {
                                string ordStatus = CommonUtils.ConvertToString(dr[STATUS_FIELD]);

                                if (!(ordStatus == "新增" || ordStatus == "正常" || ordStatus == "部分成交" || ordStatus == "已成交"))
                                {
                                    removeDrsList.Add(dr);
                                    continue;
                                }
                            }

                            foreach (DataRow removeDr in removeDrsList)
                            {
                                string ordStatus = CommonUtils.ConvertToString(removeDr[STATUS_FIELD]);
                                string key = CommonUtils.ConvertToString(removeDr[KEY_FIELD]);
                                string ordID = CommonUtils.ConvertToString(removeDr[ORDERID_FIELD]);

                                this.CrossDt.Rows.Remove(removeDr);

                                warningStatusDic[strategyOrderID] -= ConvertWarningStatus(ordStatus, key, ordID);
                            }
                            this.CrossDt.AcceptChanges();
                        }




                        if (findDr != null)
                        {
                            //if (CommonUtils.ConvertToString(findDr[TRANSACTTIME_FIELD]).CompareTo(obj.TransactTime) > 0)
                            //{
                            //    // 如果时间早， 就不更新了。
                            //    return;
                            //}
                            if (!(obj.OrdStatus == "N" || obj.OrdStatus == "0" || obj.OrdStatus == "1")
                                && !((obj.OrdStatus == "4" || obj.OrdStatus == "8" || obj.OrdStatus == "C") && obj.TimeInForce == "0" && newBlockNum >= oldBlockNum))
                            {
                                string ordStatus = CommonUtils.ConvertToString(findDr[STATUS_FIELD]);
                                string key = CommonUtils.ConvertToString(findDr[KEY_FIELD]);
                                string ordID = CommonUtils.ConvertToString(findDr[ORDERID_FIELD]);

                                this.CrossDt.Rows.Remove(findDr);

                                warningStatusDic[strategyOrderID] -= ConvertWarningStatus(ordStatus, key, ordID);
                                this.CrossDt.AcceptChanges();
                                return;
                            }
                        }

                        string oldKey = string.Empty;
                        string oldOrdID = string.Empty;
                        string oldOrdStatus = string.Empty;
                        if (findDr == null)
                        {
                            // 添加记录
                            findDr = this.CrossDt.NewRow();
                            findDr[ORDERID_FIELD] = obj.OrderID.EndsWith(CommonConstants.OrderEnd) ? obj.OrderID.Replace(CommonConstants.OrderEnd, "") : obj.OrderID; //obj.OrderID;
                            findDr[STRATEGYORDERID_FIELD] = obj.StrategyOrderId;
                            findDr[KEY_FIELD] = primaryKey;

                            this.CrossDt.Rows.Add(findDr);
                        }
                        else
                        {
                            //if (CommonUtils.ConvertToString(findDr[TRANSACTTIME_FIELD]).CompareTo(obj.TransactTime) > 0)
                            //{
                            //    // 如果时间早， 就不更新了。
                            //    return;
                            //}
                            //if (obj.OrdStatus != "0" && obj.OrdStatus != "1")
                            //{
                            //    this.CrossDt.Rows.Remove(findDr);
                            //}
                        }
                        oldKey = CommonUtils.ConvertToString(findDr[KEY_FIELD]);
                        oldOrdID = CommonUtils.ConvertToString(findDr[ORDERID_FIELD]);
                        oldOrdStatus = CommonUtils.ConvertToString(findDr[STATUS_FIELD]);

                        findDr[VENUES_FIELD] = obj.Venues;
                        findDr[BLOCK_NUM] = $"{obj.BlockNum}:{obj.OrdStatus}:{obj.ExecType}:{obj.TimeInForce}";
                        //findDr[TIMEINFORCE_FIELD] = obj.TimeInForce;

                        findDr[STATUS_FIELD] = ComboSourceUtil.Instance.FindEnum(obj.OrdStatus, "ordstatus");
                        if ((obj.OrdStatus == "0" || obj.OrdStatus == "1") && obj.ExecType == "6")
                        {
                            findDr[STATUS_FIELD] = "撤销中";
                        }

                        findDr[DISPLAY_PRICE_FIELD] = CommonUtils.FormatPrice(obj.Price);// 用于显示的收益率
                        findDr[PRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Price);// 收益率
                        findDr[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);  // 净价
                        findDr[EDIT_NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价（编辑）
                        findDr[QTY_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty); // 数量
                        findDr[CLORDID_FIELD] = obj.ClOrdID;
                        findDr[SIDE] = obj.Side;
                        findDr[MARKETINDICATOR_FIELD] = obj.MarketIndicator;
                        findDr[BOOKID_FIELD] = obj.BookID;
                        if (Consts.MARKETINDICATOR_FUTURE.Equals(obj.MarketIndicator))
                        {
                            //国债期货 成交价(%) 不显示
                            findDr[DISPLAY_PRICE_FIELD] = DBNull.Value;
                            //国债期货 净价显示成交价
                            findDr[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Price);
                        }

                        if (obj.Venues == "ODM" && (obj.OrdStatus == "0" || obj.OrdStatus == "1"))
                        {
                            findDr[DISPLAYBTN_FIELD] = Visibility.Visible;
                        }
                        else
                        {
                            findDr[DISPLAYBTN_FIELD] = Visibility.Collapsed;
                        }
                        this.CrossDt.AcceptChanges();

                        if (warningStatusDic.ContainsKey(strategyOrderID) == false)
                        {
                            warningStatusDic[strategyOrderID] = ConvertWarningStatus(obj.OrdStatus, obj.OrderID, CommonUtils.ConvertToString(findDr[ORDERID_FIELD]));
                        }
                        else
                        {
                            warningStatusDic[strategyOrderID] -= ConvertWarningStatus(oldOrdStatus, oldKey, oldOrdID);
                            string ordStatus = CommonUtils.ConvertToString(findDr[STATUS_FIELD]);
                            string key = CommonUtils.ConvertToString(findDr[KEY_FIELD]);
                            string ordID = CommonUtils.ConvertToString(findDr[ORDERID_FIELD]);
                            warningStatusDic[strategyOrderID] += ConvertWarningStatus(ordStatus, key, ordID);
                        }
                        _OnWarningOrders?.Invoke(strategyOrderID);
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error("CrossOrdersProcesser UpdateEmsOrders error", e);
            }
        }


        /// <summary>
        /// 转换警告状态
        /// </summary>
        /// <param name="status">ems订单状态</param>
        /// <returns>警告状态 0：正常 1：警告</returns>
        private int ConvertWarningStatus(string status, string key, string orderid)
        {
            int flag = 0;
            if (key == orderid)
            {
                if (status == "C" || status == "8" || status == "4"
                || status == "异常" || status == "已撤销" || status == "已拒绝")
                {
                    flag = 1;
                }
            }
            return flag;
        }

        /// <summary>
        /// 移除指定StrategyOrderID的记录
        /// </summary>
        /// <param name="strategyOrderID">订单号</param>
        public void RemoveDataRowByStrategyOrderId(string strategyOrderID)
        {
            lock(cancelStcIDDic)
            {
                cancelStcIDDic[strategyOrderID] = null;
            }

            lock (DetailDvLockObj)
            {
                DataRow[] removeDrs = this.CrossDt.Select($"{STRATEGYORDERID_FIELD} = '{strategyOrderID}'");
                foreach (DataRow removeDr in removeDrs)
                {
                    this.CrossDt.Rows.Remove(removeDr);
                }
            }

            if (warningStatusDic.ContainsKey(strategyOrderID))
            {
                int value = 0;
                warningStatusDic.TryRemove(strategyOrderID, out value);
            }

            lock(blocknumDic)
            {
                List<string> filterKey = blocknumDic.Keys.Where(delegate (string key)
                    {
                        return key.Contains(strategyOrderID);
                    }).ToList();

                if (filterKey != null && filterKey.Count > 0)
                {
                    foreach (string key in filterKey)
                        blocknumDic.Remove(key);
                }

                //if (blocknumDic.ContainsKey(strategyOrderID))
                //{
                //    blocknumDic.Remove(strategyOrderID);
                //}
            }
        }
    }
}
