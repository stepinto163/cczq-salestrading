using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Threading;
using System.Windows;
using System.Collections.Concurrent;
using STC.Common.Handler.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Model.Base;
using STC.Common.Data;

namespace BDB.Trade.Plugin.Handler.DataProcesser
{
    public class CrossOrdersProcesser
    {
        private static CrossOrdersProcesser proc = new CrossOrdersProcesser();

        public static CrossOrdersProcesser Processer
        {
            get
            {
                return proc;
            }
        }

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

        public int GetWarningStatusByStrategyOrderID(string strategyOrderID)
        {
            int flag = 0;
            if (warningStatusDic.ContainsKey(strategyOrderID))
            {
                flag = warningStatusDic[strategyOrderID];
            }
            return flag;
        }

        private CrossOrdersProcesser()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

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


        private object detailDvLockObj = new object();
        public object DetailDvLockObj
        {
            get
            {
                return detailDvLockObj;
            }
        }

        #region 表格字段
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
                    _CrossDt.Columns.Add(STATUS_FIELD, typeof(string));
                    _CrossDt.Columns.Add(ORDERID_FIELD, typeof(string));
                    _CrossDt.Columns.Add(VENUES_FIELD, typeof(string));
                    _CrossDt.Columns.Add(DISPLAY_PRICE_FIELD, typeof(string));
                    _CrossDt.Columns.Add(PRICE_FIELD, typeof(decimal));
                    _CrossDt.Columns.Add(NETPRICE_FIELD, typeof(decimal));
                    _CrossDt.Columns.Add(QTY_FIELD, typeof(long));
                    _CrossDt.Columns.Add(EDIT_NETPRICE_FIELD, typeof(decimal));
                    _CrossDt.Columns.Add(DISPLAYBTN_FIELD, typeof(string));
                    _CrossDt.Columns.Add(CLORDID_FIELD, typeof(string));
                    _CrossDt.Columns.Add(STRATEGYORDERID_FIELD, typeof(string));
                    _CrossDt.Columns.Add(TRANSACTTIME_FIELD, typeof(string));
                    _CrossDt.Columns.Add(BLOCK_NUM, typeof(string));
                    //_CrossDt.Columns.Add(TIMEINFORCE_FIELD, typeof(string));

                    _CrossDt.PrimaryKey = new DataColumn[] { _CrossDt.Columns[ORDERID_FIELD] };
                    //this.OnPropertyChanged();
                }

                return _CrossDt;
            }
        }

        AsyncThreadQueue<ExecutionReport> _ProcessEmsOrdersThreadQueue;
        AsyncThreadQueue<ExecutionReport> _ProcessMyExecutionThreadQueue;

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
                // this.GenerateTestData();
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

        private void Instance_OnClear(ClearPo obj)
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

        private void UpdateMyExecutions(ExecutionReport obj)
        {
            lock (this.detailDvLockObj)
            {
                try
                {
                    this.Sub_UpdateMyExecutions(obj);
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
        /// <param name="obj"></param>
        private void Sub_UpdateMyExecutions(ExecutionReport obj)
        {
            DataRow myExecution = this.CrossDt.Rows.Find(obj.OrderID);
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
            myExecution[PRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);// 成交价
            myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);  // 净价
            myExecution[EDIT_NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价（编辑）
            myExecution[QTY_FIELD] = CommonUtils.ConvertToLong(obj.LastQty); // 成交量
            myExecution[CLORDID_FIELD] = obj.ClOrdID;

            myExecution[DISPLAYBTN_FIELD] = Visibility.Collapsed;
        }

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

        private void UpdateEmsOrders(ExecutionReport obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Sub_UpdateEmsOrders(obj);
            }));
        }

        private void Sub_UpdateEmsOrders(ExecutionReport obj)
        {
            try
            {
                lock(cancelStcIDDic)
                {
                    if (cancelStcIDDic.ContainsKey(obj.StrategyOrderId))
                    {
                        return;
                    }
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
                DataRow findDr = this.CrossDt.Rows.Find(obj.OrderID);
                if (!(obj.OrdStatus == "N" || obj.OrdStatus == "0" || obj.OrdStatus == "1")
                    && !((obj.OrdStatus == "4" || obj.OrdStatus == "8" || obj.OrdStatus == "C") && obj.TimeInForce == "0" && newBlockNum >= oldBlockNum)
                    && findDr == null)
                {
                    return;
                }

                lock (this.detailDvLockObj)
                {
                    if (needEliminate)
                    {
                        warningStatusDic[strategyOrderID] = 0;
                        DataRow[] drs = this.CrossDt.Select($"{STRATEGYORDERID_FIELD} = '{strategyOrderID}'");
                        List<DataRow> removeDrsList = new List<DataRow>();
                        foreach (DataRow dr in drs)
                        {
                            string ordStatus = CommonUtils.ConvertToString(dr[STATUS_FIELD]);

                            if (!(ordStatus == "新增" || ordStatus == "正常" || ordStatus == "部分成交"|| ordStatus == "已成交"))
                            {
                                removeDrsList.Add(dr);
                                continue;
                            }

                            int currentFlag = warningStatusDic[strategyOrderID];
                            warningStatusDic[strategyOrderID] = currentFlag | ConvertWarningStatus(ordStatus);
                        }

                        foreach (DataRow removeDr in removeDrsList)
                        {
                            this.CrossDt.Rows.Remove(removeDr);
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
                            this.CrossDt.Rows.Remove(findDr);
                            warningStatusDic[strategyOrderID] = 0;
                            DataRow[] drs = this.CrossDt.Select($"{STRATEGYORDERID_FIELD} = '{strategyOrderID}'");
                            foreach (DataRow dr in drs)
                            {
                                string ordStatus = CommonUtils.ConvertToString(dr[STATUS_FIELD]);
                                int currentFlag = warningStatusDic[strategyOrderID];
                                warningStatusDic[strategyOrderID] = currentFlag | ConvertWarningStatus(ordStatus);
                            }
                            this.CrossDt.AcceptChanges();
                            return;
                        }
                    }

                    if (findDr == null)
                    {
                        // 添加记录
                        findDr = this.CrossDt.NewRow();
                        findDr[ORDERID_FIELD] = obj.OrderID;
                        findDr[STRATEGYORDERID_FIELD] = obj.StrategyOrderId;

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
                        warningStatusDic[strategyOrderID] = ConvertWarningStatus(obj.OrdStatus);
                    }
                    else
                    {
                        int currentFlag = warningStatusDic[strategyOrderID];
                        warningStatusDic[strategyOrderID] = currentFlag | ConvertWarningStatus(obj.OrdStatus);
                    }
                    _OnWarningOrders?.Invoke(strategyOrderID);
                }
            }
            catch (Exception e)
            {
                LogHandler.Error("CrossOrdersProcesser UpdateEmsOrders error", e);
            }


        }

        private int ConvertWarningStatus(string status)
        {
            int flag = 0;
            if (status == "C" || status == "8" || status == "4"
                || status == "异常" || status == "已撤销" || status == "已拒绝")
            {
                flag = 1;
            }
            return flag;
        }

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
