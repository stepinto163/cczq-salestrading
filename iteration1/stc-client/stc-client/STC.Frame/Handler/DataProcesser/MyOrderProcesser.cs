using com.gateway.log;
using netapi.Api;
using STC.Common.Handler.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace STC.XBond.Plugin.Utils.DataProcesser
{
    /// <summary>
    /// 我的订单处理器
    /// </summary>
    public class MyOrderProcesser
    {
        private int _CancelMaxCapacity = 500;

        /// <summary>
        /// MyOrderProcesser实例
        /// </summary>
        private static MyOrderProcesser proc = new MyOrderProcesser();

        public static MyOrderProcesser Processer
        {
            get
            {
                return proc;
            }
        }

        /// <summary>
        /// 最新成交事件通知
        /// </summary>
        private Action<object> _OnLastExecutions;
        public event Action<object> OnLastExecutions
        {
            add
            {
                _OnLastExecutions += value;
            }
            remove
            {
                _OnLastExecutions -= value;
            }
        }


        private volatile bool isInit = false;

        private object lockObj_dummyDT = new object();
        /// <summary>
        /// 虚拟表格
        /// </summary>
        private DataTable _DummyDT;

        /// <summary>
        /// 更新我的订单线程
        /// </summary>
        private Thread _updateMyOrdersThread;

        /// <summary>
        /// 更新我的订单委托
        /// </summary>
        private UpdateDataTableDelegate m_UpdateDTDelegate;

        //Modi:cyc Date:2021-12-23 Func:增加倒计时
        DispatcherTimer descTimer = new DispatcherTimer();

        /// <summary>
        /// 构造函数
        /// </summary>
        private MyOrderProcesser()
        {
            m_UpdateDTDelegate = AsyncUpdateData;

            BaseHandler.Instance.Init(Instance_OnClear);

            lock(lockObj_dummyDT)
            {
                if (_DummyDT == null)
                {
                    _DummyDT = this.MyOrdersDT.Clone();
                }
            }

            // 定时检查 ctprice是否需要更新。
            if (_updateMyOrdersThread == null)
            {
                _updateMyOrdersThread = new Thread(UpdateMyOrdersUI);
                _updateMyOrdersThread.IsBackground = true;
                _updateMyOrdersThread.Start();
            }

            this.descTimer = new DispatcherTimer();
            this.descTimer.Interval = TimeSpan.FromSeconds(1);
            this.descTimer.Tick += DescTimer_Tick;
            this.descTimer.Start();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~MyOrderProcesser()
        {
            Dispose();
        }

        /// <summary>
        /// 我的订单资源锁
        /// </summary>
        private object lockObj_MyOrders = new object();
        public object LockObj_MyOrders
        {
            get
            {
                return lockObj_MyOrders;
            }
        }

        /// <summary>
        /// 我的撤单资源锁
        /// </summary>
        private object lockObj_MyCancelOrders = new object();
        public object LockObj_MyCancelOrders
        {
            get
            {
                return lockObj_MyCancelOrders;
            }
        }

        #region '我的订单'表格字段
        public const string ISCHECKED_ORDER_FIELD = "IsChecked";
        public const string KEY_ORDER_FIELD = "Key";
        public const string SIDE_ORDER_FIELD = "Side";
        public const string SECURITYID_ORDER_FIELD = "SecurityID";
        public const string SYMBOL_ORDER_FIELD = "Symbol";
        public const string NETPRICE_ORDER_FIELD = "NetPrice";
        public const string PRICE_ORDER_FIELD = "Price";
        public const string QTY_ORDER_FIELD = "Qty";
        public const string DEALINGQTY_ORDER_FIELD = "DealingQty";
        public const string EXECQTY_ORDER_FIELD = "ExecQty";
        public const string MYDEALER_ORDER_FIELD = "MyDealer";
        public const string SETTLETYPE_ORDER_FIELD = "SettleType";
        public const string SETTLDATE_ORDER_FIELD = "SettlDate";
        public const string ORDTYPE_ORDER_FIELD = "OrdType";
        public const string STCCODE_ORDER_FIELD = "STCCode";
        public const string VENEUES_ORDER_FIELD = "Veneues";
        public const string ORDERSTATUS_ORDER_FIELD = "OrderStatus";
        public const string TRANSACTTIME_ORDER_FIELD = "TransactTime";
        public const string UPDATETIME_ORDER_FIELD = "UpdateTime";
        public const string ALGONAME_ORDER_FIELD = "AlgoName";
        public const string FIRMFLAG_ORDER_FIELD = "FirmFlag";
        public const string SECURITY_TYPE = "SecurityType";
        public const string NONFIRM_TIME_FIELD = "NonfirmTime";
        public const string REMAINING_TIME_FIELD = "RemainingTime";
        public const string SHOWDELETEBTN_ORDER_FIELD = "ShowDeleteBtn";
        public const string ENABLEDBTN_ORDER_FIELD = "EnabledBtn";
        public const string WHOLE_FLAG_FIELD = "WholeFlag";
        public const string CROSS_VENUES_FIELD = "CrossVenues";
        public const string VENUES_SELECT_F_FIELD = "VenuesSelectF";
        public const string VENUES_RDM_NUM_FIELD = "VenuesRdmNum";
        public const string SELF_SELECT_VENUES_FIELD = "SelfSelectVenues";
        public const string VENUES_SELECT_DISPLAY_FIELD = "VenuesSelectDisplay";
        public const string INSTITUTIONID_FIELD = "InstitutionID";
        public const string INSTITUTIONNAME_FIELD = "InstitutionName";
        public const string TRADERID_FIELD = "TraderID";
        public const string TRADERNAME_FIELD = "TraderName";
        public const string VENUETYPEGW_FIELD = "VenueTypeGW";
        public const string QUANTID_FIELD = "QuantID";
        public const string TIMEINFORCE_FIELD = "TimeInForce";
        public const string BIZTYPE_FIELD = "BizType";
        public const string BIZTYPEID_FIELD = "BizTypeID";
        public const string QUOTEID_FIELD = "QuoteID";
        public const string ROLEID_FIELD = "RoleID";
        public const string ROLENAME_FIELD = "RoleName";
        public const string USERID_FIELD = "UserID";
        public const string SHOW_TRADER_FIELD = "ShowTrader";
        public const string TERMINAL_FIELD = "Terminal";
        public const string INFO3_FIELD = "Info3";
        public const string EXECTYPE_FIELD = "ExecType";
        public const string WARINGSTATUS_FILED = "warningStatus";
        public const string IOCTIMEOUT_FIELD = "ioctimeout";
        public const string ORDERTYPE_FIELD = "ordertype";
        #endregion

        /// <summary>
        /// 我的订单数据源
        /// </summary>
        private DataTable _MyOrdersDT;
        public DataTable MyOrdersDT
        {
            get
            {
                lock (LockObj_MyOrders)
                {
                    if (_MyOrdersDT == null)
                    {
                        _MyOrdersDT = new DataTable();
                        _MyOrdersDT.Columns.Add(ISCHECKED_ORDER_FIELD, typeof(bool));
                        _MyOrdersDT.Columns.Add(KEY_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SIDE_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SYMBOL_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SECURITYID_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(NETPRICE_ORDER_FIELD, typeof(decimal));
                        _MyOrdersDT.Columns.Add(PRICE_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(QTY_ORDER_FIELD, typeof(long));
                        _MyOrdersDT.Columns.Add(DEALINGQTY_ORDER_FIELD, typeof(long));
                        _MyOrdersDT.Columns.Add(EXECQTY_ORDER_FIELD, typeof(long));
                        _MyOrdersDT.Columns.Add(MYDEALER_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SETTLETYPE_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SETTLDATE_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(ORDTYPE_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(STCCODE_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(VENEUES_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(ORDERSTATUS_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(TRANSACTTIME_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(UPDATETIME_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(ALGONAME_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(FIRMFLAG_ORDER_FIELD, typeof(string)); 
                        _MyOrdersDT.Columns.Add(NONFIRM_TIME_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SECURITY_TYPE, typeof(string));
                        _MyOrdersDT.Columns.Add(REMAINING_TIME_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SHOWDELETEBTN_ORDER_FIELD, typeof(Visibility));
                        _MyOrdersDT.Columns.Add(ENABLEDBTN_ORDER_FIELD, typeof(bool));
                        _MyOrdersDT.Columns.Add(WHOLE_FLAG_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(CROSS_VENUES_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(VENUES_SELECT_F_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(VENUES_RDM_NUM_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SELF_SELECT_VENUES_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(VENUES_SELECT_DISPLAY_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(INSTITUTIONID_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(INSTITUTIONNAME_FIELD,typeof(string));
                        _MyOrdersDT.Columns.Add(TRADERID_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(TRADERNAME_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(VENUETYPEGW_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(QUANTID_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(TIMEINFORCE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(BIZTYPEID_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(BIZTYPE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(QUOTEID_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(ROLEID_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(ROLENAME_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(USERID_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SHOW_TRADER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(TERMINAL_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(INFO3_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(EXECTYPE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(WARINGSTATUS_FILED, typeof(int));
                        _MyOrdersDT.Columns.Add(IOCTIMEOUT_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(ORDERTYPE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add("IsPoint", typeof(string));
                        _MyOrdersDT.Columns.Add("RemainSeconds", typeof(string)); 
                        _MyOrdersDT.PrimaryKey = new DataColumn[] { _MyOrdersDT.Columns[KEY_ORDER_FIELD] };
                       
                        _MyOrdersDT.DefaultView.Sort = $"{UPDATETIME_ORDER_FIELD} desc";
                    }
                    return this._MyOrdersDT;
                }
            }
        }

        /// <summary>
        /// 我的撤单数据源
        /// </summary>
        private DataTable _MyCancelOrdersDT;
        public DataTable MyCancelOrdersDT
        {
            get
            {
                lock (LockObj_MyCancelOrders)
                {
                    if (_MyCancelOrdersDT == null)
                    {
                        _MyCancelOrdersDT = MyOrdersDT.Clone();

                        _MyCancelOrdersDT.PrimaryKey = new DataColumn[] { _MyCancelOrdersDT.Columns[KEY_ORDER_FIELD] };
                    }
                    return this._MyCancelOrdersDT;
                }
            }
        }

        /// <summary>
        /// 处理我的订单消息的线程队列
        /// </summary>
        private AsyncThreadQueue<ExecutionReport> _ProcessMyOrdersThreadQueue;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (isInit == false)
            {
                if (_ProcessMyOrdersThreadQueue == null)
                {
                    _ProcessMyOrdersThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateMyOrders);
                }

                //#region 创建测试数据
                //this.GenerateTestData();
                //return;
                //#endregion


                #region 收TBook消息
                //---------------收TBook消息--------------
                // 我的订单
                OrdersHandler.Instance.OnOwnOrders += Instance_OnOrder;
                //---------------------------------------
                #endregion

                CrossOrdersProcesser.Processer.OnWarningOrders += Processer_OnWarningOrders;

                isInit = true;
            }
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            OrdersHandler.Instance.OnOwnOrders -= Instance_OnOrder;
            CrossOrdersProcesser.Processer.OnWarningOrders -= Processer_OnWarningOrders;

            lock(this.lockObj_dummyDT)
            {
                if (_DummyDT != null)
                {
                    _DummyDT.Rows.Clear();
                }
            }

            lock (this.lockObj_MyOrders)
            {
                if (_MyOrdersDT != null)
                {
                    _OnLastExecutions?.Invoke("0");
                    _MyOrdersDT.Rows.Clear();
                }
            }

            lock(this.lockObj_MyCancelOrders)
            {
                if (_MyCancelOrdersDT != null)
                {
                    _MyCancelOrdersDT.Rows.Clear();
                }
            }

            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
            }

            isInit = false;
        }

        /// <summary>
        /// 警告数据回调方法
        /// </summary>
        /// <param name="strategyOrderID"></param>
        private void Processer_OnWarningOrders(string strategyOrderID)
        {
            lock(this.lockObj_MyOrders)
            {

                DataRow  findDr = this.MyOrdersDT.Rows.Find(strategyOrderID);
                if (findDr != null)
                {
                    findDr[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(strategyOrderID);
                }
            }
        }

        /// <summary>
        /// 释放数据
        /// </summary>
        public void Dispose()
        {
            if (_updateMyOrdersThread != null)
            {
                _updateMyOrdersThread.Abort();
                _updateMyOrdersThread = null;
            }

            this.Clear();

            lock(lockObj_dummyDT)
            {
                if (_DummyDT != null)
                {
                    _DummyDT.Clear();
                    _DummyDT.Dispose();
                    _DummyDT = null;
                }
            }

            lock (this.lockObj_MyOrders)
            {
                if (_MyOrdersDT != null)
                {
                    _MyOrdersDT.Clear();
                    _MyOrdersDT.Dispose();
                    _MyOrdersDT = null;
                }
            }

            lock (this.lockObj_MyCancelOrders)
            {
                if (_MyCancelOrdersDT != null)
                {
                    _MyCancelOrdersDT.Clear();
                    _MyCancelOrdersDT.Dispose();
                    _MyCancelOrdersDT = null;
                }
            }

            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
                _ProcessMyOrdersThreadQueue.Dispose();
                _ProcessMyOrdersThreadQueue = null;
            }

            isInit = false;
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void ResetClear()
        {
            LogHandler.Info("MyOrderProcesser ResetClear.");
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Clear();
                this.Init();
            }));
        }

        /// <summary>
        /// 清零消息回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("MyOrderProcesser Clear");
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
        /// 更新我的订单数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOrder(Common.Model.Fix.Message.ExecutionReport obj)
        {
            //UpdateMyOrders(obj);
            if (this._ProcessMyOrdersThreadQueue != null)
            {
                this._ProcessMyOrdersThreadQueue.Enqueue(obj);
            }
        }

        private volatile bool isUpdating = false;
        /// <summary>
        /// 更新我的订单
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMyOrdersUI()
        {
           
            while (true)
            {
                try
                {
                    if (isUpdating == true)
                    {
                        continue;
                    }
                    DataTable updateDt = null;
                    lock (lockObj_dummyDT)
                    {
                        if (updateDt == null && this._DummyDT != null && this._DummyDT.Rows.Count > 0)
                        {
                            updateDt = this._DummyDT.Clone();
                        }

                        if (updateDt != null)
                        {
                            updateDt.Rows.Clear();

                            // 每50条更新界面
                            int calCount = this._DummyDT.Rows.Count > 50 ? 50 : this._DummyDT.Rows.Count;
                            for(int i = 0; i< calCount; i++)
                            {
                                updateDt.Rows.Add(this._DummyDT.Rows[i].ItemArray);
                            }
                            for (int i = 0; i < calCount; i++)
                            {
                                this._DummyDT.Rows.RemoveAt(0);
                            }
                        }
                    }

                    if (Application.Current != null && updateDt != null && updateDt.Rows.Count > 0)
                    {
                        LogHandler.Info("-------------------UpdateMyOrdersUI-");
                        isUpdating = true;
                        Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind,
                            m_UpdateDTDelegate, updateDt);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                finally
                {
                    Thread.Sleep(200);
                }
            }
        }

        
        private async void AsyncUpdateData(DataTable updateDt)
        {
            try
            {
                if (updateDt != null && updateDt.Rows.Count > 0)
                {
                    int index = 0;
                    foreach (DataRow updateDr in updateDt.Rows)
                    {
                       
                        Sub_UpdateMyOrders2(updateDr);
                        if (index % 10 == 0)
                            await System.Windows.Threading.Dispatcher.Yield();
                        index++;
                    }
                }

                await System.Windows.Threading.Dispatcher.Yield();
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
            finally
            {
                isUpdating = false;
            }
        }

        /// <summary>
        /// 处理我的订单消息
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMyOrders(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj.ExecType == "6")
            {
                // 如果是存在，关闭编辑窗口
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    WindowManager.Instance.CloseWindow($"EditOrder-{obj.OrderID}");
                }));
            }



            // 更新我的订单虚拟数据
            UpdateDummyDt(obj);

            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{

                if (!(obj.ExecType == "N" || obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "S" || obj.ExecType == "6"))
                {
                    EmsOrdersHandler.Instance.RemoveCacheDataByStrategyOrderId(obj.OrderID);
                    CrossOrdersProcesser.Processer.RemoveDataRowByStrategyOrderId(obj.OrderID);
                
                }

                //<已撤>挂单的范围包括：人工撤销内部订单
                //< 已撤 > 挂单的范围不包括：已撤销的点价单，已撤量化订单，IDB发起的撤单
                if (obj.ExecType == "4" && obj.TimeInForce == "0"
                    && obj.Venues.ToUpper() != "QUANT" 
                    && obj.Venues.ToUpper() != "IDB")
                {
                    Sub_UpdateMyCancelOrders(obj);
                }
            //}));
        }

        /// <summary>
        /// 处理我的撤单数据
        /// </summary>
        /// <param name="obj"></param>
        private void Sub_UpdateMyCancelOrders(Common.Model.Fix.Message.ExecutionReport obj)
        {
            try
            {
                lock (this.lockObj_MyCancelOrders)
                {
                    DataRow myCancelOrder = this.MyCancelOrdersDT.Rows.Find(obj.OrderID);

                    if (myCancelOrder != null)
                    {

                        if (CommonUtils.ConvertToString(myCancelOrder[UPDATETIME_ORDER_FIELD]).CompareTo(obj.UpdateTime) > 0)
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }
                    }
                    else
                    {
                        if (this.MyCancelOrdersDT.Rows.Count >= _CancelMaxCapacity)
                        {
                            string removeOrderID = this.MyCancelOrdersDT.Rows[_CancelMaxCapacity - 1][KEY_ORDER_FIELD].ConvertToString();
                            OrdersHandler.Instance.RemoveCacheOrder(removeOrderID);
                            this.MyCancelOrdersDT.Rows.RemoveAt(_CancelMaxCapacity-1);

                        }

                        myCancelOrder = this.MyCancelOrdersDT.NewRow();
                        myCancelOrder[KEY_ORDER_FIELD] = obj.OrderID;
                        myCancelOrder[ISCHECKED_ORDER_FIELD] = false;
                        myCancelOrder[ENABLEDBTN_ORDER_FIELD] = true;


                        this.MyCancelOrdersDT.Rows.InsertAt(myCancelOrder, 0);
                    }

                    if (myCancelOrder != null)
                    {
                        myCancelOrder[TERMINAL_FIELD] = obj.Terminal;
                        myCancelOrder[SIDE_ORDER_FIELD] = obj.Side;
                        myCancelOrder[SYMBOL_ORDER_FIELD] = obj.Symbol; // 没有简称
                        myCancelOrder[SECURITYID_ORDER_FIELD] = obj.SecurityID;// obj.SecurityID;
                        myCancelOrder[NETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        myCancelOrder[PRICE_ORDER_FIELD] = obj.Price;//CommonUtils.ConvertToDecimal(obj.Price);
                        myCancelOrder[QTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty);
                        myCancelOrder[DEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                        myCancelOrder[EXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);
                        myCancelOrder[MYDEALER_ORDER_FIELD] = obj.UserName; // 本方交易员
                        myCancelOrder[SETTLETYPE_ORDER_FIELD] = obj.SettlType; // 清算速度 
                        myCancelOrder[SETTLDATE_ORDER_FIELD] = obj.SettlDate; //交割日
                        myCancelOrder[ORDTYPE_ORDER_FIELD] = obj.AlgoName; // 报价类型, 目前仅有此一种类别X-Bond做市，三阶段将增加：Tradebook，IDB，双边做市
                        myCancelOrder[STCCODE_ORDER_FIELD] = obj.OrderID.EndsWith(CommonConstants.OrderEnd) ? obj.OrderID.Replace(CommonConstants.OrderEnd, "") : obj.OrderID; // STC单号
                        myCancelOrder[VENEUES_ORDER_FIELD] = obj.Venues; // 场所
                        string originalOrderStatus = CommonUtils.ConvertToString(myCancelOrder[ORDERSTATUS_ORDER_FIELD]);
                        //if (originalOrderStatus != "4"
                        //    && obj.OrdStatus == "4")
                        //{
                        //    myOrder[ISCHECKED_ORDER_FIELD] = false;
                        //}
                        myCancelOrder[ORDERSTATUS_ORDER_FIELD] = obj.OrdStatus;// 订单状态
                        myCancelOrder[TRANSACTTIME_ORDER_FIELD] = obj.TransactTime;
                        myCancelOrder[UPDATETIME_ORDER_FIELD] = obj.UpdateTime; // 撤单发送时间使用updateTime
                        myCancelOrder[ALGONAME_ORDER_FIELD] = obj.AlgoName;// 交易策略，NONO，STP,XBOND,CROSS
                        myCancelOrder[FIRMFLAG_ORDER_FIELD] = obj.FirmFlag; // firm，nonfirm
                        string firmToNonTime = obj.FirmToNonTime == null ? string.Empty : obj.FirmToNonTime;
                        myCancelOrder[NONFIRM_TIME_FIELD] = firmToNonTime; // nofirmtime
                        myCancelOrder[SECURITY_TYPE] = obj.SecurityType;
                        myCancelOrder[REMAINING_TIME_FIELD] = firmToNonTime;
                        myCancelOrder[WHOLE_FLAG_FIELD] = obj.WholeFlag; // 是否整量
                        myCancelOrder[CROSS_VENUES_FIELD] = obj.CrossVenues; // 撮合渠道
                        myCancelOrder[VENUETYPEGW_FIELD] = obj.VenueTypeGW; // getway
                        myCancelOrder[QUANTID_FIELD] = obj.QuantID;// quantid
                        myCancelOrder[TIMEINFORCE_FIELD] = obj.TimeInForce;//有效时间 DAY("0"),IOC("3")
                        myCancelOrder[BIZTYPE_FIELD] = obj.BizType;
                        myCancelOrder[BIZTYPEID_FIELD] = obj.BizTypeID;
                        myCancelOrder[QUOTEID_FIELD] = obj.QuoteID;
                        myCancelOrder[ROLEID_FIELD] = obj.RoleID;
                        myCancelOrder[ROLENAME_FIELD] = obj.RoleName;
                        myCancelOrder[USERID_FIELD] = obj.UserID;

                        myCancelOrder[VENUES_SELECT_F_FIELD] = obj.VenuesSelectF; //渠道类型  不填写1,默认2，指定3 
                        myCancelOrder[VENUES_RDM_NUM_FIELD] = obj.VenuesRdmNum; //渠道类型默认，填写随机几家渠道
                        myCancelOrder[SELF_SELECT_VENUES_FIELD] = obj.SelfSelectVenues; // 指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC 
                        myCancelOrder[INFO3_FIELD] = obj.Info3;
                        myCancelOrder[EXECTYPE_FIELD] = obj.ExecType;
                        myCancelOrder[IOCTIMEOUT_FIELD] = obj.IocTimeOut;
                        if (obj.TimeInForce == "0")
                        {
                            myCancelOrder[ORDERTYPE_FIELD] = "限价LMT单";
                        }
                        else if (obj.TimeInForce == "3")
                        {
                            if (string.IsNullOrEmpty(obj.IocTimeOut))
                            {
                                myCancelOrder[ORDERTYPE_FIELD] = "点价IOC单";
                            }
                            else
                            {
                                myCancelOrder[ORDERTYPE_FIELD] = "点价FAK单";
                            }
                        }

                        // 渠道显示
                        string venuesSelectDisplay = "TradeBook";
                        if (obj.VenuesSelectF == "2")
                        {
                            if (string.IsNullOrEmpty(obj.VenuesRdmNum) == false)
                            {
                                venuesSelectDisplay = $"默认+{obj.VenuesRdmNum}家";
                            }
                            else
                            {
                                venuesSelectDisplay = "默认";
                            }
                        }
                        else if (obj.VenuesSelectF == "3")
                        {
                            StringBuilder displaySB = new StringBuilder();
                            string[] arr = obj.SelfSelectVenues.Split(',');
                            foreach (string v in arr)
                            {
                                if (string.IsNullOrEmpty(v) == false)
                                {
                                    string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                                    displaySB.Append($"<{v_str}>");
                                }
                            }
                            venuesSelectDisplay = $"指定{displaySB}";
                        }
                        myCancelOrder[VENUES_SELECT_DISPLAY_FIELD] = venuesSelectDisplay;

                        //if (obj.ExecType == "N" || obj.ExecType == "6")
                        if (obj.ExecType == "6")
                        {
                            myCancelOrder[ENABLEDBTN_ORDER_FIELD] = false;
                        }
                        else
                        {
                            myCancelOrder[ENABLEDBTN_ORDER_FIELD] = true;
                        }

                        if ((obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "N" || obj.ExecType == "6") &&
                            obj.AlgoName != "XBOND" && obj.Venues != "QUANT" && string.IsNullOrEmpty(obj.IocTimeOut))
                        {
                            myCancelOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Visible;
                        }
                        else
                        {
                            myCancelOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Collapsed;
                        }

                        myCancelOrder[INSTITUTIONID_FIELD] = obj.InstitutionID;
                        myCancelOrder[INSTITUTIONNAME_FIELD] = obj.InstitutionName;
                        myCancelOrder[TRADERID_FIELD] = obj.TraderID;
                        myCancelOrder[TRADERNAME_FIELD] = obj.TraderName;
                        myCancelOrder[SHOW_TRADER_FIELD] = obj.ShowTrader;
                        if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                        {
                            LogHandler.Info($"已撤订单{obj.ClOrdID}");
                        }
                    }
                    myCancelOrder.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateMyCancelOrders", e);
            }
        }

        private void UpdateDummyDt(Common.Model.Fix.Message.ExecutionReport obj)
        {
            try
            {
                lock (lockObj_dummyDT)
                {
                    DataRow myOrder = this._DummyDT.Rows.Find(obj.OrderID);
                    if (myOrder == null)
                    {
                        myOrder = this._DummyDT.NewRow();
                        myOrder[KEY_ORDER_FIELD] = obj.OrderID;
                        myOrder[ISCHECKED_ORDER_FIELD] = false;
                        myOrder[ENABLEDBTN_ORDER_FIELD] = true;

                        this._DummyDT.Rows.Add(myOrder);
                    }

                    if (myOrder != null)
                    {
                        if (string.IsNullOrEmpty(obj.UpdateTime) == false 
                            && CommonUtils.ConvertToString(myOrder[UPDATETIME_ORDER_FIELD]).CompareTo(obj.UpdateTime) > 0)
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }
                        myOrder[TERMINAL_FIELD] = obj.Terminal;
                        myOrder[SIDE_ORDER_FIELD] = obj.Side;
                        myOrder[SYMBOL_ORDER_FIELD] = obj.Symbol; // 没有简称
                        myOrder[SECURITYID_ORDER_FIELD] = obj.SecurityID;// obj.SecurityID;
                        myOrder[NETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        myOrder[PRICE_ORDER_FIELD] = obj.Price;//CommonUtils.ConvertToDecimal(obj.Price);
                        myOrder[QTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty);
                        myOrder[DEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                        myOrder[EXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);
                        myOrder[MYDEALER_ORDER_FIELD] = obj.UserName; // 本方交易员
                        myOrder[SETTLETYPE_ORDER_FIELD] = obj.SettlType; // 清算速度 
                        myOrder[SETTLDATE_ORDER_FIELD] = obj.SettlDate; //交割日
                        myOrder[ORDTYPE_ORDER_FIELD] = obj.AlgoName; // 报价类型, 目前仅有此一种类别X-Bond做市，三阶段将增加：Tradebook，IDB，双边做市
                        myOrder[STCCODE_ORDER_FIELD] = obj.OrderID.EndsWith(CommonConstants.OrderEnd) ? obj.OrderID.Replace(CommonConstants.OrderEnd, "") : obj.OrderID;//obj.OrderID; // STC单号
                        myOrder[VENEUES_ORDER_FIELD] = obj.Venues; // 场所
                        string originalOrderStatus = CommonUtils.ConvertToString(myOrder[ORDERSTATUS_ORDER_FIELD]);
                        //if (originalOrderStatus != "4"
                        //    && obj.OrdStatus == "4")
                        //{
                        //    myOrder[ISCHECKED_ORDER_FIELD] = false;
                        //}
                        myOrder[ORDERSTATUS_ORDER_FIELD] = obj.OrdStatus;// 订单状态
                        myOrder[TRANSACTTIME_ORDER_FIELD] = obj.TransactTime; // 发送时间
                        myOrder[UPDATETIME_ORDER_FIELD] = obj.UpdateTime;// 更新事件
                        myOrder[ALGONAME_ORDER_FIELD] = obj.AlgoName;// 交易策略，NONO，STP,XBOND,CROSS
                        myOrder[FIRMFLAG_ORDER_FIELD] = obj.FirmFlag; // firm，nonfirm
                        string firmToNonTime = obj.FirmToNonTime == null ? string.Empty : obj.FirmToNonTime;
                        myOrder[NONFIRM_TIME_FIELD] = firmToNonTime; // nofirmtime
                        myOrder[SECURITY_TYPE] = obj.SecurityType;
                        myOrder[REMAINING_TIME_FIELD] = firmToNonTime;
                        myOrder[WHOLE_FLAG_FIELD] = obj.WholeFlag; // 是否整量
                        myOrder[CROSS_VENUES_FIELD] = obj.CrossVenues; // 撮合渠道
                        myOrder[VENUETYPEGW_FIELD] = obj.VenueTypeGW; // getway
                        myOrder[QUANTID_FIELD] = obj.QuantID;// quantid
                        myOrder[TIMEINFORCE_FIELD] = obj.TimeInForce;//有效时间 DAY("0"),IOC("3")
                        myOrder[BIZTYPE_FIELD] = obj.BizType;
                        myOrder[BIZTYPEID_FIELD] = obj.BizTypeID;
                        myOrder[QUOTEID_FIELD] = obj.QuoteID;
                        myOrder[ROLEID_FIELD] = obj.RoleID;
                        myOrder[ROLENAME_FIELD] = obj.RoleName;
                        myOrder[USERID_FIELD] = obj.UserID;

                        myOrder[VENUES_SELECT_F_FIELD] = obj.VenuesSelectF; //渠道类型  不填写1,默认2，指定3 
                        myOrder[VENUES_RDM_NUM_FIELD] = obj.VenuesRdmNum; //渠道类型默认，填写随机几家渠道
                        myOrder[SELF_SELECT_VENUES_FIELD] = obj.SelfSelectVenues; // 指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC 
                        myOrder[INFO3_FIELD] = obj.Info3;
                        myOrder[EXECTYPE_FIELD] = obj.ExecType;
                        myOrder[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(obj.OrderID);
                        myOrder[IOCTIMEOUT_FIELD] = obj.IocTimeOut;
                        if (obj.TimeInForce == "0")
                        {
                            myOrder[ORDERTYPE_FIELD] = "限价LMT单";
                        }
                        else if (obj.TimeInForce == "3")
                        {
                            if (string.IsNullOrEmpty(obj.IocTimeOut))
                            {
                                myOrder[ORDERTYPE_FIELD] = "点价IOC单";
                            }
                            else
                            {
                                myOrder[ORDERTYPE_FIELD] = "点价FAK单";
                            }
                        }


                        myOrder["IsPoint"] = obj.TimeInForce == "0" ? "0" : "1";  //是否是点价意向
                        myOrder["RemainSeconds"] = obj.IocTimeOut == null ? string.Empty : obj.IocTimeOut;  // nofirmtime

                        // 渠道显示
                        string venuesSelectDisplay = "TradeBook";
                        if (obj.VenuesSelectF == "2")
                        {
                            if (string.IsNullOrEmpty(obj.VenuesRdmNum) == false)
                            {
                                venuesSelectDisplay = $"默认+{obj.VenuesRdmNum}家";
                            }
                            else
                            {
                                venuesSelectDisplay = "默认";
                            }
                        }
                        else if (obj.VenuesSelectF == "3")
                        {
                            StringBuilder displaySB = new StringBuilder();
                            string[] arr = obj.SelfSelectVenues.Split(',');
                            foreach(string v in arr)
                            {
                                if (string.IsNullOrEmpty(v) == false)
                                {
                                    string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                                    displaySB.Append($"<{v_str}>");
                                }
                            }
                            venuesSelectDisplay = $"指定{displaySB}";
                        }
                        myOrder[VENUES_SELECT_DISPLAY_FIELD] = venuesSelectDisplay;

                        //if (obj.ExecType == "N" || obj.ExecType == "6" || obj.AlgoName.ToUpper() == "MM")
                        if (obj.ExecType == "6" || obj.AlgoName.ToUpper() == "MM")
                        {
                            myOrder[ENABLEDBTN_ORDER_FIELD] = false;
                        }
                        else
                        {
                            myOrder[ENABLEDBTN_ORDER_FIELD] = true;
                        }
                        if ((obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "N" || obj.ExecType == "6") &&
                            obj.AlgoName != "XBOND" && obj.AlgoName.ToUpper() != "MM" && obj.Venues != "QUANT" && string.IsNullOrEmpty(obj.IocTimeOut))
                        {
                            myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Visible;
                        }
                        else
                        {
                            myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Collapsed;
                        }

                        if (obj.AlgoName.ToUpper() == "MM")
                            myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Collapsed;

                        myOrder[INSTITUTIONID_FIELD] = obj.InstitutionID;
                        myOrder[INSTITUTIONNAME_FIELD] = obj.InstitutionName;
                        myOrder[TRADERID_FIELD] = obj.TraderID;
                        myOrder[TRADERNAME_FIELD] = obj.TraderName;
                        myOrder[SHOW_TRADER_FIELD] = obj.ShowTrader;
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error("UpdateDummyDt", ex);
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dummyDr"></param>
        private void Sub_UpdateMyOrders2(DataRow dummyDr)
        {
            try
            {

                lock (this.lockObj_MyOrders)
                {
                    string orderID = CommonUtils.ConvertToString(dummyDr[MyOrderProcesser.KEY_ORDER_FIELD]);
                    string execType = CommonUtils.ConvertToString(dummyDr[MyOrderProcesser.EXECTYPE_FIELD]);
                    string securityID = CommonUtils.ConvertToString(dummyDr[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
                    string updateTime = CommonUtils.ConvertToString(dummyDr[MyOrderProcesser.UPDATETIME_ORDER_FIELD]);

                    DataRow myOrder = this.MyOrdersDT.Rows.Find(orderID);
                    LogHelper.Info($"---------------AsyncUpdateData-{orderID}:: {execType} :: {securityID} :: {updateTime}");
                    if (myOrder != null)
                    {
                        if (!(execType == "N" || execType == "0" || execType == "1" || execType == "S" || execType == "6"))
                        {

                            this.MyOrdersDT.Rows.Remove(myOrder);

                            // 注销ems消息
                            EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(securityID, CrossOrdersProcesser.Processer.Instance_OnEmsOrders);

                            //CrossOrdersProcesser.Processer.RemoveDataRowByStrategyOrderId(obj.OrderID);
                            return;
                        }

                        if (CommonUtils.ConvertToString(myOrder[UPDATETIME_ORDER_FIELD]).CompareTo(updateTime) > 0)
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }
                        else
                        {
                            myOrder.ItemArray = dummyDr.ItemArray;
                            myOrder[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(orderID);
                        }
                    }
                    else
                    {
                        if (!(execType == "N" || execType == "0" || execType == "1" || execType == "S" || execType == "6"))
                        {
                            return;
                        }

                        myOrder = this.MyOrdersDT.NewRow();
                        myOrder.ItemArray = dummyDr.ItemArray;
                        myOrder[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(orderID);

                        // 请求ems消息
                        EmsOrdersHandler.Instance.RegistEMSOrdersBySecurityID(securityID, CrossOrdersProcesser.Processer.Instance_OnEmsOrders);

                        this.MyOrdersDT.Rows.Add(myOrder);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateMyOrders", e);
            }
            finally
            {
                string securityID = CommonUtils.ConvertToString(dummyDr[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
                string side = CommonUtils.ConvertToString(dummyDr[MyOrderProcesser.SIDE_ORDER_FIELD]);
                UpdateLstOrderDic(securityID, side);

                _OnLastExecutions?.Invoke(this.MyOrdersDT.Rows.Count);
            }
        }

        /// <summary>
        /// 更新数据(废弃)
        /// </summary>
        /// <param name="obj"></param>
        private void Sub_UpdateMyOrders(Common.Model.Fix.Message.ExecutionReport obj)
        {
            try
            {
                lock (this.lockObj_MyOrders)
                {
                    DataRow myOrder = this.MyOrdersDT.Rows.Find(obj.OrderID);

                    if (myOrder != null)
                    {
                        if (!(obj.ExecType == "N" || obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "S" || obj.ExecType == "6"))
                        {
                            
                            this.MyOrdersDT.Rows.Remove(myOrder);

                            this.MyOrdersDT.AcceptChanges();

                            UpdateLstOrderDic(obj.SecurityID, obj.Side);

                            // 注销ems消息
                            EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(obj.SecurityID, CrossOrdersProcesser.Processer.Instance_OnEmsOrders);

                            //CrossOrdersProcesser.Processer.RemoveDataRowByStrategyOrderId(obj.OrderID);
                            return;
                        }

                        if (CommonUtils.ConvertToString(myOrder[UPDATETIME_ORDER_FIELD]).CompareTo(obj.UpdateTime) > 0)
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }
                    }
                    else
                    {
                        if (!(obj.ExecType == "N" || obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "S" || obj.ExecType == "6"))
                        {
                            return;
                        }

                        myOrder = this.MyOrdersDT.NewRow();
                        myOrder[KEY_ORDER_FIELD] = obj.OrderID;
                        myOrder[ISCHECKED_ORDER_FIELD] = false;
                        myOrder[ENABLEDBTN_ORDER_FIELD] = true;

                        // 请求ems消息
                        EmsOrdersHandler.Instance.RegistEMSOrdersBySecurityID(obj.SecurityID, CrossOrdersProcesser.Processer.Instance_OnEmsOrders);

                        this.MyOrdersDT.Rows.Add(myOrder);
                    }

                    if (myOrder != null)
                    {
                       

                        myOrder[TERMINAL_FIELD] = obj.Terminal;
                        myOrder[SIDE_ORDER_FIELD] = obj.Side;
                        myOrder[SYMBOL_ORDER_FIELD] = obj.Symbol; // 没有简称
                        myOrder[SECURITYID_ORDER_FIELD] = obj.SecurityID;// obj.SecurityID;
                        myOrder[NETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        myOrder[PRICE_ORDER_FIELD] = obj.Price;//CommonUtils.ConvertToDecimal(obj.Price);
                        myOrder[QTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty);
                        myOrder[DEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                        myOrder[EXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);
                        myOrder[MYDEALER_ORDER_FIELD] = obj.UserName; // 本方交易员
                        myOrder[SETTLETYPE_ORDER_FIELD] = obj.SettlType; // 清算速度 
                        myOrder[SETTLDATE_ORDER_FIELD] = obj.SettlDate; //交割日
                        myOrder[ORDTYPE_ORDER_FIELD] = obj.AlgoName; // 报价类型, 目前仅有此一种类别X-Bond做市，三阶段将增加：Tradebook，IDB，双边做市
                        myOrder[STCCODE_ORDER_FIELD] = obj.OrderID.EndsWith(CommonConstants.OrderEnd) ? obj.OrderID.Replace(CommonConstants.OrderEnd, "") : obj.OrderID;//obj.OrderID; // STC单号
                        myOrder[VENEUES_ORDER_FIELD] = obj.Venues; // 场所
                        string originalOrderStatus = CommonUtils.ConvertToString(myOrder[ORDERSTATUS_ORDER_FIELD]);
                        //if (originalOrderStatus != "4"
                        //    && obj.OrdStatus == "4")
                        //{
                        //    myOrder[ISCHECKED_ORDER_FIELD] = false;
                        //}
                        myOrder[ORDERSTATUS_ORDER_FIELD] = obj.OrdStatus;// 订单状态
                        myOrder[TRANSACTTIME_ORDER_FIELD] = obj.TransactTime; // 发送时间
                        myOrder[UPDATETIME_ORDER_FIELD] = obj.UpdateTime;// 更新事件
                        myOrder[ALGONAME_ORDER_FIELD] = obj.AlgoName;// 交易策略，NONO，STP,XBOND,CROSS
                        myOrder[FIRMFLAG_ORDER_FIELD] = obj.FirmFlag; // firm，nonfirm
                        string firmToNonTime = obj.FirmToNonTime == null ? string.Empty : obj.FirmToNonTime;
                        myOrder[NONFIRM_TIME_FIELD] = firmToNonTime; // nofirmtime
                        myOrder[SECURITY_TYPE] = obj.SecurityType;
                        myOrder[REMAINING_TIME_FIELD] = firmToNonTime;
                        myOrder[WHOLE_FLAG_FIELD] = obj.WholeFlag; // 是否整量
                        myOrder[CROSS_VENUES_FIELD] = obj.CrossVenues; // 撮合渠道
                        myOrder[VENUETYPEGW_FIELD] = obj.VenueTypeGW; // getway
                        myOrder[QUANTID_FIELD] = obj.QuantID;// quantid
                        myOrder[TIMEINFORCE_FIELD] = obj.TimeInForce;//有效时间 DAY("0"),IOC("3")
                        myOrder[BIZTYPE_FIELD] = obj.BizType;
                        myOrder[BIZTYPEID_FIELD] = obj.BizTypeID;
                        myOrder[QUOTEID_FIELD] = obj.QuoteID;
                        myOrder[ROLEID_FIELD] = obj.RoleID;
                        myOrder[ROLENAME_FIELD] = obj.RoleName;
                        myOrder[USERID_FIELD] = obj.UserID;

                        myOrder[VENUES_SELECT_F_FIELD] = obj.VenuesSelectF; //渠道类型  不填写1,默认2，指定3 
                        myOrder[VENUES_RDM_NUM_FIELD] = obj.VenuesRdmNum; //渠道类型默认，填写随机几家渠道
                        myOrder[SELF_SELECT_VENUES_FIELD] = obj.SelfSelectVenues; // 指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC 
                        myOrder[INFO3_FIELD] = obj.Info3;
                        myOrder[EXECTYPE_FIELD] = obj.ExecType;
                        myOrder[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(obj.OrderID);
                        myOrder[IOCTIMEOUT_FIELD] = obj.IocTimeOut;
                        if (obj.TimeInForce == "0")
                        {
                            myOrder[ORDERTYPE_FIELD] = "限价LMT单";
                        }
                        else if (obj.TimeInForce == "3")
                        {
                            if (string.IsNullOrEmpty(obj.IocTimeOut))
                            {
                                myOrder[ORDERTYPE_FIELD] = "点价IOC单";
                            }
                            else
                            {
                                myOrder[ORDERTYPE_FIELD] = "点价FAK单";
                            }
                        }

                        myOrder["IsPoint"] = obj.TimeInForce == "0" ? "0" : "1";  //是否是点价意向
                        myOrder["RemainSeconds"] = obj.IocTimeOut == null ? string.Empty : obj.IocTimeOut;  // nofirmtime

                        // 渠道显示
                        string venuesSelectDisplay = "TradeBook";
                        if (obj.VenuesSelectF == "2")
                        {
                            if (string.IsNullOrEmpty(obj.VenuesRdmNum) == false)
                            {
                                venuesSelectDisplay = $"默认+{obj.VenuesRdmNum}家";
                            }
                            else
                            {
                                venuesSelectDisplay = "默认";
                            }
                        }
                        else if (obj.VenuesSelectF == "3")
                        {
                            StringBuilder displaySB = new StringBuilder();
                            string[] arr = obj.SelfSelectVenues.Split(',');
                            foreach(string v in arr)
                            {
                                if (string.IsNullOrEmpty(v) == false)
                                {
                                    string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                                    displaySB.Append($"<{v_str}>");
                                }
                            }
                            venuesSelectDisplay = $"指定{displaySB}";
                        }
                        myOrder[VENUES_SELECT_DISPLAY_FIELD] = venuesSelectDisplay;

                        //if (obj.ExecType == "N" || obj.ExecType == "6")
                        if (obj.ExecType == "6")
                        {
                            myOrder[ENABLEDBTN_ORDER_FIELD] = false;
                        }
                        else
                        {
                            myOrder[ENABLEDBTN_ORDER_FIELD] = true;
                        }
                        if ((obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "N" || obj.ExecType == "6") &&
                            obj.AlgoName != "XBOND" && obj.Venues != "QUANT" && string.IsNullOrEmpty(obj.IocTimeOut))
                        {
                            myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Visible;
                        }
                        else
                        {
                            myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Collapsed;
                        }

                        if (obj.AlgoName.ToUpper() == "MM")
                            myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Collapsed;

                        myOrder[INSTITUTIONID_FIELD] = obj.InstitutionID;
                        myOrder[INSTITUTIONNAME_FIELD] = obj.InstitutionName;
                        myOrder[TRADERID_FIELD] = obj.TraderID;
                        myOrder[TRADERNAME_FIELD] = obj.TraderName;
                        myOrder[SHOW_TRADER_FIELD] = obj.ShowTrader;

                        if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                        {
                            LogHandler.Info($"我的订单{obj.ClOrdID}");
                        }
                        UpdateLstOrderDic(obj.SecurityID, obj.Side);
                    }
                    myOrder.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateMyOrders", e);
            }

        }

        /// <summary>
        /// 通过orderid获得我的订单记录
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public DataRow GetOrderById(string orderId)
        {
            lock(lockObj_MyOrders)
            {
                return this.MyOrdersDT.Rows.Find(orderId);
            }
        }

        private ConcurrentDictionary<string, DataRowView> _LstOrderDic = new ConcurrentDictionary<string, DataRowView>();
        public Action<string, string> OnOwnOrdersChange;

        /// <summary>
        /// 更新最近一笔我的订单记录
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateLstOrderDic(string securityID, string side)
        {
            lock(lockObj_MyOrders)
            {
                this.MyOrdersDT.DefaultView.RowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}'";

                string lstOrderKey = $"{securityID}#{side}";
                lock (string.Intern(lstOrderKey))
                {
                    if (this.MyOrdersDT.DefaultView.Count > 0)
                    {
                        _LstOrderDic[lstOrderKey] = this.MyOrdersDT.DefaultView[0];
                    }
                    else
                    {
                        DataRowView removeDRV;
                        _LstOrderDic.TryRemove(lstOrderKey, out removeDRV);
                    }
                }
            }
            OnOwnOrdersChange?.Invoke(securityID, side);
        }

        /// <summary>
        /// 通过securityid取得最新一笔我的MM订单记录
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public ExecutionReport GetLstMMOrderBySecurityID(string securityID, string side)
        {
            string orderID = string.Empty;
            try
            {
                string rowFilter = rowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}' and {ALGONAME_ORDER_FIELD} = 'MM'";


                lock (LockObj_MyOrders)
                {
                    this.MyOrdersDT.DefaultView.RowFilter = rowFilter;

                    if (this.MyOrdersDT.DefaultView.Count > 0)
                    {
                        DataRowView drv = this.MyOrdersDT.DefaultView[0];
                        orderID = CommonUtils.ConvertToString(drv[MyOrderProcesser.KEY_ORDER_FIELD]);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }

            return OrdersHandler.Instance.GetCacheValidOrders(orderID);
        }

        /// <summary>
        /// 通过securityid取得最新一笔我的订单记录
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public DataRowView GetLstOrderBySecurityID(string securityID, string side)
        {
            try
            {
                string lstOrderKey = $"{securityID}#{side}";
                if (_LstOrderDic.ContainsKey(lstOrderKey))
                {
                    DataRowView result = _LstOrderDic[lstOrderKey];
                    if (result.Row.RowState == DataRowState.Deleted)
                    {
                        return null;
                    }
                    string exectype = CommonUtils.ConvertToString(result[EXECTYPE_FIELD]);
                    if (exectype == "N" || exectype == "0" || exectype == "1" || exectype == "S" || exectype == "6" )
                    {
                        return result;
                    }
                }
            }
            catch(Exception ex)
            {
                LogHandler.Error(ex);
            }
            
            return null;
        }

        public DataRow GetMyBestOrderBySecurityId(string securityId, string side, string isPoint, string settle)
        {
            lock (this.lockObj_MyOrders)
            {
                //this.MyOrdersDT.DefaultView.RowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityId}' and {SIDE_ORDER_FIELD} = '{side}' and {SETTLETYPE_ORDER_FIELD} = '{settle}'";

                //this.MyOrdersDT.DefaultView.RowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityId}' and {SIDE_ORDER_FIELD} = '{side}'";

                string baseFilter = $"{SECURITYID_ORDER_FIELD} = '{securityId}' and {SIDE_ORDER_FIELD} = '{side}'";
                

                if (!string.IsNullOrEmpty(settle))
                {
                    baseFilter += $" and {settle}";
                }


                //if (!string.IsNullOrEmpty(settle))
                //    rowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityId}' and {SIDE_ORDER_FIELD} = '{side}' and {ISPOINT_FILED} = '0' and {settle}";
                //else
                //    rowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityId}' and {SIDE_ORDER_FIELD} = '{side}' and {ISPOINT_FILED} = '0'";

                this.MyOrdersDT.DefaultView.RowFilter = baseFilter;

                DataTable dt = MyOrdersDT.DefaultView.ToTable();
                decimal price = 0;
                DataRow result = null;

                foreach (DataRow dr in dt.Rows)
                {
                    string tem = CommonUtils.ConvertToString(dr[PRICE_ORDER_FIELD]);
                    decimal temPrice = CommonUtils.ConvertToDecimal(tem);

                    if (side == CommonConstants.SIDE_BID_ONE)
                    {
                        if (price == 0 || (price > temPrice))
                        {
                            result = dr;
                            price = temPrice;
                        }
                        else if (price != 0 && price == temPrice)
                        {
                            result[DEALINGQTY_ORDER_FIELD] = AddQty(result[DEALINGQTY_ORDER_FIELD].ToString(), dr[DEALINGQTY_ORDER_FIELD].ToString());
                            result[KEY_ORDER_FIELD] += "," + dr[KEY_ORDER_FIELD];
                        }
                    }
                    else if (side == CommonConstants.SIDE_OFR_TWO)
                    {
                        if (price == 0 || (price < temPrice))
                        {
                            result = dr;
                            price = temPrice;
                        }
                        else if (price != 0 && price == temPrice)
                        {
                            result[DEALINGQTY_ORDER_FIELD] = AddQty(result[DEALINGQTY_ORDER_FIELD].ToString(), dr[DEALINGQTY_ORDER_FIELD].ToString());
                            result[KEY_ORDER_FIELD] += "," + dr[KEY_ORDER_FIELD];
                        }
                    }
                }


                return result;
            }

        }

        private long AddQty(string firstNum, string secondNum)
        {
            long result = 0;
            long firt = CommonUtils.ConvertToLong(firstNum);
            long second = CommonUtils.ConvertToLong(secondNum);

            return result;
        }

        #region 点价意向倒计时
        private void DescTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                this.UpdatMyOrdersTimeRemains();
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region 点价倒计时

        /// <summary>
        /// 更新我的订单剩余时间
        /// </summary>
        public void UpdatMyOrdersTimeRemains()
        {
            // var m = ObsMyReqOrders.FirstOrDefault(o => o.OrderID == rs.OrderID);
            lock (LockObj_MyOrders)
            {
                foreach (DataRow myOrder in this.MyOrdersDT.Rows)
                {
                    if (myOrder["IsPoint"].ToString() == "1")
                    {
                        int icotimeout = 0;
                        int res = 0;
                        int.TryParse(myOrder["IocTimeOut"].ToString(), out icotimeout);

                        // 保存剩余时间
                        DateTime transactTime = CommonUtils.ConvertToDateTime(myOrder["TransactTime"].ToString()).AddSeconds(icotimeout);
                        TimeSpan diffTime = transactTime - DateTimeEx.ServerTime;
                        res = Convert.ToInt32(diffTime.TotalSeconds);
                        myOrder["RemainSeconds"] = res < 0 ? "0" : res.ToString();
                        myOrder.AcceptChanges();
                    }
                }
            }
            
        }
        #endregion

        /// <summary>
        /// 生成测试数据
        /// </summary>
        private void GenerateTestData()
        {
            // -------- 创建测试数据 -------------

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                for (int i = 0; i < 2; i++)
                {
                    Common.Model.Fix.Message.ExecutionReport er = new Common.Model.Fix.Message.ExecutionReport();
                    er.AvgPx = "0"; // 成交价格
                    er.ClOrdID = string.Format($"c{i}"); //客户订单编号
                    er.CumQty = "0";// 总成交量
                    er.LeavesQty = "1000"; //剩余量
                    er.OrdStatus = "0"; // 订单状态 :N,0( N,0(挂单成功 ), 1(部分成交 ),2( 完全成交),4( 撤销),8( 拒绝 ),C( 过期 )
                    er.OrdType = "2"; // 订单类型2limit
                    er.OrderID = string.Format($"s{i}"); // 订单号
                    er.OrderQty = "1000"; // （订单量：债券位为万 ）
                    er.Price = "4.2"; //价格: 债券指收益率
                    er.Side = "1"; // 方向 1:买入 2：卖
                    er.StandardHeader.MsgType = "8";
                    er.SecurityID = "180210.IB"; // 代码
                    er.Symbol = "180210.IB"; // 简称
                    er.TimeInForce = "0"; //有效时间 DAY("0"),IOC("3")
                    er.VenueTypeGW = "SOR"; //渠道
                    er.Venues = "TPSC"; // 经纪商
                    er.SettlType = "1"; // 清算速度？？？时这里取，还是从基本数据里取
                    er.TransactTime = "09:00:00"; // 发送时间？？
                    UpdateMyOrders(er);
                }

                for (int i = 0; i < 2; i++)
                {
                    Common.Model.Fix.Message.ExecutionReport er = new Common.Model.Fix.Message.ExecutionReport();
                    er.AvgPx = "0"; // 成交价格
                    er.ClOrdID = string.Format($"c{i}"); //客户订单编号
                    er.CumQty = "0";// 总成交量
                    er.LeavesQty = "1000"; //剩余量
                    er.OrdStatus = "0"; // 订单状态 :N,0( N,0(挂单成功 ), 1(部分成交 ),2( 完全成交),4( 撤销),8( 拒绝 ),C( 过期 )
                    er.OrdType = "2"; // 订单类型2limit
                    er.OrderID = string.Format($"s{i}"); // 订单号
                    er.OrderQty = "1000"; // （订单量：债券位为万 ）
                    er.Price = "5.2"; //价格: 债券指收益率
                    er.Side = "1"; // 方向 1:买入 2：卖
                    er.StandardHeader.MsgType = "8";
                    er.SecurityID = "180210.IB"; // 代码
                    er.Symbol = "180210.IB"; // 简称
                    er.TimeInForce = "0"; //有效时间 DAY("0"),IOC("3")
                    er.VenueTypeGW = "SOR"; //渠道
                    er.Venues = "TPSC"; // 经纪商
                    er.SettlType = "4"; // 清算速度
                    er.TransactTime = "09:00:00";
                    UpdateMyOrders(er);
                }

                for (int i = 3; i < 5; i++)
                {
                    Common.Model.Fix.Message.ExecutionReport er = new Common.Model.Fix.Message.ExecutionReport();
                    er.AvgPx = "0"; // 成交价格
                    er.ClOrdID = string.Format($"c{i}"); //客户订单编号
                    er.CumQty = "0";// 总成交量
                    er.LeavesQty = "1000"; //剩余量
                    er.OrdStatus = "0"; // 订单状态 :N,0( N,0(挂单成功 ), 1(部分成交 ),2( 完全成交),4( 撤销),8( 拒绝 ),C( 过期 )
                    er.OrdType = "2"; // 订单类型2limit
                    er.OrderID = string.Format($"s{i}"); // 订单号
                    er.OrderQty = "1000"; // （订单量：债券位为万 ）
                    er.Price = "5.2"; //价格: 债券指收益率
                    er.Side = "2"; // 方向 1:买入 2：卖
                    er.StandardHeader.MsgType = "8";
                    er.SecurityID = "180219.IB";// 代码
                    er.Symbol = "180219.IB"; // 简称
                    er.TimeInForce = "0"; //有效时间 DAY("0"),IOC("3")
                    er.SettlType = "2"; // 清算速度？？？
                    er.VenueTypeGW = "SOR"; //渠道
                    er.Venues = "TPSC"; // 经纪商
                    er.TransactTime = "09:00:00"; // 发送时间？？
                    UpdateMyOrders(er);
                }


            });

            //-------------------------------------
        }
    }
}
