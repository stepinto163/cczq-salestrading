using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Handler.Position;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
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
using System.Windows.Threading;

namespace STC.Common.DataProcesser
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
        //Modi:cyc Date:2021-12-23 Func:增加倒计时
        System.Threading.Timer stTimer;

        private bool isdispose = false;
        /// <summary>
        /// 构造函数
        /// </summary>
        private MyOrderProcesser()
        {
            ////m_UpdateDTDelegate = AsyncUpdateData;

            OrdersHandler.Instance.OnClear += Instance_OnClear;


            //dueTime: 定时器延时多久开始调用。单位 毫秒
            //period: 定时器每隔多久调用一次。单位 毫秒
            stTimer = new System.Threading.Timer(new TimerCallback(timer_OnTime), null, 100, 300);
            //获取组合列表
            GetBookUser();

            // 非堵塞订单处理线程
            // 启动 ExecutionReport 处理程序
            
            if (_updateErOrdersDataThread == null)
            {
                _updateErOrdersDataThread = new Thread(ExecutionReportDataProcessor);
                _updateErOrdersDataThread.Priority = ThreadPriority.AboveNormal;
                _updateErOrdersDataThread.Start();
            }
        }


        #region 查询组合名称字典表
        /// 组合和组合ID对照表
        public ConcurrentDictionary<string, string> BookDict = new ConcurrentDictionary<string, string>();
        public void GetBookUser()
        {
            QueryPositionBookResult reply = PositionHandler.Instance.QueryBookUser();

            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    foreach (PostionBookModel pbm in reply.Datas)
                    {
                        BookDict[pbm.id] = pbm.bookname;
                    }
                }
            }
        }
        #endregion

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
        public const string INFO1_FIELD = "Info1";
        public const string INFO2_FIELD = "Info2";
        public const string INFO3_FIELD = "Info3";
        public const string EXECTYPE_FIELD = "ExecType";
        public const string WARINGSTATUS_FILED = "warningStatus";
        public const string VALIDUNTILTIME_FIELD = "ValidUntilTime";
        public const string IOCTIMEOUT_FIELD = "ioctimeout";
        public const string ORDERTYPE_FIELD = "ordertype";
        public const string RETURN_FIELD = "ReturnTime";
        public const string INTENTIONID_FIELD = "IntentionId";
        public const string MAXFLOOR_FIELD = "MaxFloor";
        public const string PARTYSUBNAME_FIELD = "PartySubName";
        public const string MARKETINDICATOR_FIELD = "MarketIndicator";
        public const string OPENCLOSE_FIELD = "OffsetFlag"; //开平仓标志 0=开仓，1=平仓
        public const string SPEC_HEDGE_FIELD = "HedgeFlag"; //投机套保套利 1=投机，2=套利，3=套保
        public const string CAP_ACCOUNT_FIELD = "CapAccount";//资金账号
        public const string BOOKID_FIELD = "BookID";
        public const string BOOKNAME_FIELD = "BookName";
        public const string MINCHANGE_FIELD = "MinChange";

        public const string SEND_ORDER_TYPE_FIELD = "SendOrderType";//执行类型

        public const string AccountID_Field = "AccountId";
        public const string AccountName_Field = "AccountName";
        public const string PriceCondition_Field = "PriceCondition";
        
        //允许撤改定向回价
        public const string EnableDRPBtn_Field = "EnableDRPBtn";
        public const string DRPShowInfo_Field = "DRPShowInfo";
        public const string IsShowDRPProcBtn_Field = "IsShowDRPProcBtn";
        public const string Text_Field = "Text";//原文
        public const string QuoteType_Field = "QuoteType";
        public const string Yield_FIELD = "Yield";
        public const string YieldToExecution_FIELD = "YieldToExecution";
        public const string YieldToExecutionBAK_FIELD = "YieldToExecutionBAK";

        // 来源
        public const string CHANNEL_FIELD = "Channel";

        #endregion

        /// <summary>
        /// 我的订单数据源
        /// </summary>
        private DataTable _MyOrdersDT;
        public DataTable MyOrdersDT
        {
            get
            {
                lock (lockObj_MyOrders)
                {
                    if (_MyOrdersDT == null)
                    {
                        _MyOrdersDT = new DataTable();
                        _MyOrdersDT.Columns.Add(ISCHECKED_ORDER_FIELD, typeof(bool));
                        _MyOrdersDT.Columns.Add(KEY_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SIDE_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SYMBOL_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SECURITYID_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(NETPRICE_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(PRICE_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(QTY_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(DEALINGQTY_ORDER_FIELD, typeof(long));
                        _MyOrdersDT.Columns.Add(EXECQTY_ORDER_FIELD, typeof(long));
                        _MyOrdersDT.Columns.Add(MYDEALER_ORDER_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(PARTYSUBNAME_FIELD, typeof(string));
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
                        _MyOrdersDT.Columns.Add(INSTITUTIONNAME_FIELD, typeof(string));
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
                        _MyOrdersDT.Columns.Add(INFO1_FIELD, typeof(string)); 
                        _MyOrdersDT.Columns.Add(INFO2_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(INFO3_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(EXECTYPE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(WARINGSTATUS_FILED, typeof(int));
                        _MyOrdersDT.Columns.Add(IOCTIMEOUT_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(ORDERTYPE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SEND_ORDER_TYPE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(VALIDUNTILTIME_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add("IsPoint", typeof(string));
                        _MyOrdersDT.Columns.Add("RemainSeconds", typeof(string));
                        _MyOrdersDT.Columns.Add(RETURN_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(INTENTIONID_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(MAXFLOOR_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(MARKETINDICATOR_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(OPENCLOSE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(SPEC_HEDGE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(CAP_ACCOUNT_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(BOOKID_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(BOOKNAME_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(MINCHANGE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(AccountID_Field, typeof(string));
                        _MyOrdersDT.Columns.Add(AccountName_Field, typeof(string));
                        _MyOrdersDT.Columns.Add(PriceCondition_Field, typeof(string));

                        _MyOrdersDT.Columns.Add(EnableDRPBtn_Field, typeof(bool));
                        _MyOrdersDT.Columns.Add(DRPShowInfo_Field, typeof(string));
                        _MyOrdersDT.Columns.Add(Text_Field, typeof(string));
                        _MyOrdersDT.Columns.Add(QuoteType_Field, typeof(string));
                        _MyOrdersDT.Columns.Add(Yield_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(YieldToExecution_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(YieldToExecutionBAK_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(CHANNEL_FIELD, typeof(string));

                        DataColumn col = new DataColumn();
                        col.ColumnName = IsShowDRPProcBtn_Field;
                        col.DataType = typeof(bool);
                        col.DefaultValue = false;
                        _MyOrdersDT.Columns.Add(col); 

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
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (isInit == false)
            {
                #region 收TBook消息
                // 我的订单
                OrdersHandler.Instance.OnOwnOrders += Instance_OnOrder;

                #endregion

                CrossOrdersProcesser.Processer.OnWarningOrders += Processer_OnWarningOrders;

                //定向回价订阅
                BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

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

            lock (lockObj_MyOrders)
            {
                if (_MyOrdersDT != null)
                {
                    _OnLastExecutions?.Invoke("0");
                    _MyOrdersDT.Rows.Clear();
                }
            }

            lock (this.lockObj_MyCancelOrders)
            {
                if (_MyCancelOrdersDT != null)
                {
                    _MyCancelOrdersDT.Rows.Clear();
                }
            }

            //lock (lockObj)
            //{
            //    //if (!dataQueue.IsEmpty)
            //    //{
            //    //    while (dataQueue.TryDequeue(out _))
            //    //    {
            //    //        // 不关心元素，只是移除所有元素
            //    //    }
            //    //}
            //}
            
            isInit = false;
        }

        /// <summary>
        /// 警告数据回调方法
        /// </summary>
        /// <param name="strategyOrderID"></param>
        private void Processer_OnWarningOrders(string strategyOrderID)
        {
            lock (LockObj_MyOrders)
            {
                DataRow findDr = this.MyOrdersDT.Rows.Find(strategyOrderID);
                if (findDr != null)
                {
                    findDr[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(strategyOrderID);
                    findDr.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// 释放数据
        /// </summary>
        public void Dispose()
        {
            isdispose = true;

            BaseHandler.Instance.UnSubscribeAppMsg(OnAppMsg);

            this.Clear();

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
        private void Instance_OnOrder(ExecutionReport obj)
        {
            // 将数据放入队列
            dataQueue.Enqueue(obj);
            // 设置事件，通知消费者有新数据
            dataAvailable.Set();
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
                            this.MyCancelOrdersDT.Rows.RemoveAt(_CancelMaxCapacity - 1);

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
                        if (CommonUtils.IsNumeric(obj.NetPrice))
                            myCancelOrder[NETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        else
                            myCancelOrder[NETPRICE_ORDER_FIELD] = null;
                        myCancelOrder[PRICE_ORDER_FIELD] = obj.Price;//CommonUtils.ConvertToDecimal(obj.Price);
                        myCancelOrder[QTY_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.OrderQty);
                        myCancelOrder[DEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                        myCancelOrder[EXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);
                        myCancelOrder[MYDEALER_ORDER_FIELD] = obj.UserName;
                        myCancelOrder[PARTYSUBNAME_FIELD] = obj.PartySubName; // 本方交易员
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
                        myCancelOrder[INFO1_FIELD] = obj.Info1;
                        myCancelOrder[INFO2_FIELD] = obj.Info2;
                        myCancelOrder[INFO3_FIELD] = obj.Info3;
                        myCancelOrder[EXECTYPE_FIELD] = obj.ExecType;
                        myCancelOrder[IOCTIMEOUT_FIELD] = obj.IocTimeOut;
                        myCancelOrder[VALIDUNTILTIME_FIELD] = obj.ValidUntilTime;


                        if (obj.AlgoName.ToUpper().Equals("STP"))
                        {
                            myCancelOrder[SEND_ORDER_TYPE_FIELD] = "直通";
                        }
                        else if (obj.AlgoName.ToUpper().Equals("CROSS"))
                        {
                            myCancelOrder[SEND_ORDER_TYPE_FIELD] = "OCO";
                        }
                        else if (obj.AlgoName.ToUpper().Equals("MM"))
                        {
                            myCancelOrder[SEND_ORDER_TYPE_FIELD] = "双边";
                        }


                        if (obj.AlgoName.ToUpper() == "DQ")
                        {
                            myCancelOrder[ORDERTYPE_FIELD] = "定向回价单";
                        }
                        else
                        {
                            if (obj.TimeInForce == "0")
                            {
                                myCancelOrder[ORDERTYPE_FIELD] = obj.SecurityID == "99999" ? "模糊需求" : "限价LMT单";
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
                            if (!string.IsNullOrEmpty(obj.SelfSelectVenues))
                            {
                                string[] arr = obj.SelfSelectVenues.Split(',');
                                foreach (string v in arr)
                                {
                                    if (string.IsNullOrEmpty(v) == false)
                                    {
                                        string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                                        displaySB.Append($"<{v_str}>");
                                    }
                                }
                            }
                            venuesSelectDisplay = $"指定{displaySB}";
                        }
                        //国债期货直接显示渠道=TradeBook
                        if (Consts.MARKETINDICATOR_FUTURE.Equals(obj.MarketIndicator))
                        {
                            venuesSelectDisplay = "TradeBook";
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

                        if ((obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "N" || obj.ExecType == "6") && obj.AlgoName != "XBOND" && obj.AlgoName != "MMSHE" && obj.Venues != "QUANT" && string.IsNullOrEmpty(obj.IocTimeOut))
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
                        myCancelOrder[INTENTIONID_FIELD] = obj.IntentionID;

                        //string newSettleType = string.Empty;
                        //string settleDate = CommonUtils.CalSettlDate(DateTimeEx.ServerTime.ToString("yyyyMMdd"), obj.SettlType, out newSettleType);
                        //myCancelOrder[SETTLDATE_ORDER_FIELD] = settleDate;
                        myCancelOrder[MARKETINDICATOR_FIELD] = obj.MarketIndicator;
                        //开平仓标志
                        myCancelOrder[OPENCLOSE_FIELD] = CommonUtils.GetOffsetFlagDisplay(obj.OffsetFlag);
                        //投机套保套利 1=投机，2=套利，3=套保
                        switch (obj.HedgeFlag)
                        {
                            case Consts.SPECULATE_CODE:
                                myCancelOrder[SPEC_HEDGE_FIELD] = Consts.SPECULATE;
                                break;
                            case Consts.ARBITRAGE_CODE:
                                myCancelOrder[SPEC_HEDGE_FIELD] = Consts.ARBITRAGE;
                                break;
                            case Consts.HEDGE_CODE:
                                myCancelOrder[SPEC_HEDGE_FIELD] = Consts.HEDGE;
                                break;
                        }
                        //资金账号暂时没有
                        //myCancelOrder[CAP_ACCOUNT_FIELD] = obj.HedgeFlag;
                        myCancelOrder[BOOKID_FIELD] = obj.BookID;
                        myCancelOrder[BOOKNAME_FIELD] = (obj.BookID != null && BookDict.ContainsKey(obj.BookID)) ? BookDict[obj.BookID] : obj.BookID;
                        //默认4位小数,最小价格变动单位
                        if ("1".Equals(obj.PriceType) && CommonUtils.IsSheBond(obj.SecurityID))
                        {
                            myCancelOrder[MINCHANGE_FIELD] = Consts.PRICE_MIN_CHANGE_0001;
                        }
                        else
                        {
                            myCancelOrder[MINCHANGE_FIELD] = CommonUtils.GetMinChangeBySecurityID(obj.SecurityID);
                        }
                        // 账户组合
                        myCancelOrder[AccountID_Field] = string.IsNullOrEmpty(obj.AccountID) ? null : obj.AccountID;
                        myCancelOrder[AccountName_Field] = string.IsNullOrEmpty(obj.AccountName) ? null : obj.AccountName;

                        myCancelOrder[PriceCondition_Field] = string.IsNullOrEmpty(obj.PriceCondition) ? null : obj.PriceCondition;

                        Common.Model.OrderTimeModel otm = new OrderTimeModel();
                        if (JsonSplit.IsJson(obj.Info1))
                            otm = Newtonsoft.Json.JsonConvert.DeserializeObject<Common.Model.OrderTimeModel>(obj.Info1);

                        if (obj.SecurityID == "99999")
                        {
                            myCancelOrder["Text"] = otm.Text;

                            if (!string.IsNullOrEmpty(otm.QuoteType))
                            {
                                myCancelOrder[QuoteType_Field] = string.IsNullOrEmpty(otm.QuoteType) ? null : otm.QuoteType;
                            }
                        }
                        else if (obj.MarketIndicator == "2")
                        {
                            if (!string.IsNullOrEmpty(otm.IsFromTableSvrOrTradeBook))
                                myCancelOrder[QuoteType_Field] = otm.IsFromTableSvrOrTradeBook == "1" ? "irs" : "tbookirs";
                            else
                                myCancelOrder[QuoteType_Field] = string.IsNullOrEmpty(obj.QuoteType) ? null : obj.QuoteType;
                        }
                        else if (obj.VenueTypeGW == "INNER" && !string.IsNullOrEmpty(obj.IntentionID))
                        {
                            //需求
                            myCancelOrder[QuoteType_Field] = "require";
                            
                        }
                        else
                        {
                            myCancelOrder[QuoteType_Field] = "order";
                        }
                        if ("QDMESP".Equals(obj.VenueTypeGW) || "1".Equals(obj.PriceType))
                        {
                            if (CommonUtils.IsNumeric(obj.Yield))
                                myCancelOrder[Yield_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);
                            else
                                myCancelOrder[Yield_FIELD] = null;
                        }
                        else 
                        {
                            myCancelOrder[Yield_FIELD] = obj.Price;
                        }
                        if ("7".Equals(obj.PriceType))
                        {
                            if (CommonUtils.IsNumeric(obj.YieldToExecution))
                                myCancelOrder[YieldToExecution_FIELD] = CommonUtils.ConvertToDecimal(obj.YieldToExecution);
                            else
                                myCancelOrder[YieldToExecution_FIELD] = null;
                        }
                        else
                        {
                            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(obj.SecurityID);
                            if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                        ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                        (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                            {
                                if (CommonUtils.IsNumeric(obj.YieldToExecution))
                                    myCancelOrder[YieldToExecution_FIELD] = CommonUtils.ConvertToDecimal(obj.YieldToExecution);
                                else
                                    myCancelOrder[YieldToExecution_FIELD] = null;
                            }
                        }
                        if (CommonUtils.IsNumeric(obj.YieldToExecution))
                            myCancelOrder[YieldToExecutionBAK_FIELD] = CommonUtils.ConvertToDecimal(obj.YieldToExecution);
                        else
                            myCancelOrder[YieldToExecutionBAK_FIELD] = null;
                        if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                        {
                            LogHandler.Info($"已撤订单{obj.ClOrdID}");
                        }
                        myCancelOrder[CHANNEL_FIELD] = obj.Channel;
                    }
                    myCancelOrder.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateMyCancelOrders", e);
            }
        }


        /// <summary>
        /// 通过orderid获得我的订单记录
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public DataRow GetOrderById(string orderId)
        {
            lock (LockObj_MyOrders)
            {
                return this.MyOrdersDT.Rows.Find(orderId);
            }
        }

        private ConcurrentDictionary<string, DataRowView> _LstOrderDic = new ConcurrentDictionary<string, DataRowView>();
        private ConcurrentDictionary<string, DataRowView> _MMSHELstOrderDic = new ConcurrentDictionary<string, DataRowView>();
        public Action<string, string> OnOwnOrdersChange;
        public Action<string, string> OnOwnMMSHEOrdersChange;

        /// <summary>
        /// 更新最近一笔我的订单记录
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateLstOrderDic(string securityID, string side)
        {
            try
            {
                lock (LockObj_MyOrders)
                {
                    MyOrdersDT.DefaultView.RowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}'";
                    string lstOrderKey = $"{securityID}#{side}";
                    if (MyOrdersDT.DefaultView.Count > 0)
                    {
                        DataRowView latestRow = MyOrdersDT.DefaultView.Cast<DataRowView>().OrderByDescending(row => row["UpdateTime"]).FirstOrDefault();
                        if (latestRow != null)
                        {
                            _LstOrderDic[lstOrderKey] = latestRow;
                        }
                    }
                    else
                    {
                        DataRowView removeDRV;
                        _LstOrderDic.TryRemove(lstOrderKey, out removeDRV);
                    }
                }

                OnOwnOrdersChange?.Invoke(securityID, side);
            }
            catch (Exception ex)
            {
                LogHandler.Error($"UpdateLstOrderDic: securityID:{securityID}; side:{side}", ex);
            }
            
        }
        private void UpdateMMSHELstOrderDic(string securityID, string venue, string side)
        {
            try
            {
                lock (LockObj_MyOrders)
                {
                    MyOrdersDT.DefaultView.RowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}'and {VENEUES_ORDER_FIELD} = '{side}'";
                    string lstOrderKey = $"{securityID}#{venue}#{side}";
                    if (MyOrdersDT.DefaultView.Count > 0)
                    {
                        DataRowView latestRow = MyOrdersDT.DefaultView.Cast<DataRowView>().OrderByDescending(row => row["UpdateTime"]).FirstOrDefault();
                        if (latestRow != null)
                        {
                            _MMSHELstOrderDic[lstOrderKey] = latestRow;
                        }
                    }
                    else
                    {
                        DataRowView removeDRV;
                        _MMSHELstOrderDic.TryRemove(lstOrderKey, out removeDRV);
                    }
                }
                OnOwnMMSHEOrdersChange?.Invoke($"{securityID}#{venue}", side);
            }
            catch (Exception ex)
            {
                LogHandler.Error($"UpdateMMSHELstOrderDic; securityID:{securityID};venue:{venue};side{side}", ex);
            }
        }

        /// <summary>
        /// 通过securityid取得最新一笔我的MM订单记录
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public ExecutionReport GetLstMMOrderBySecurityID(string securityID, string side, bool isMM = true)
        {
            string orderID = string.Empty;
            try
            {
                string rowFilter = rowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}' and {ALGONAME_ORDER_FIELD} = 'MM'";
                if (!isMM)
                {
                    rowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}'";
                }
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
        /// 通过securityid取得最新一笔我的MMSHE订单记录
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public ExecutionReport GetLstMMSHEOrderBySecurityID(string securityID,string venue, string side, bool isMMSHE = true)
        {
            string orderID = string.Empty;
            try
            {
                string rowFilter = rowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}' and {ALGONAME_ORDER_FIELD} = 'MMSHE' and {VENEUES_ORDER_FIELD} = '{venue}'";
                if (!isMMSHE)
                {
                    rowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}'";
                }
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
                    if (exectype == "N" || exectype == "0" || exectype == "1" || exectype == "S" || exectype == "6")
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }

            return null;
        }

        public DataRow GetMyBestOrderBySecurityId(string securityId, string side, string isPoint, string settle)
        {
            DataRow result = null;
            try
            {
                lock (LockObj_MyOrders)
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

                    if (MyOrdersDT.DefaultView.Count > 0)
                    {

                        DataTable dt = MyOrdersDT.DefaultView.ToTable();
                        decimal price = 0;

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
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

            return result;
        }

        private long AddQty(string firstNum, string secondNum)
        {
            long result = 0;
            long firt = CommonUtils.ConvertToLong(firstNum);
            long second = CommonUtils.ConvertToLong(secondNum);

            return result;
        }

        #region 点价意向倒计时
        private void timer_OnTime(object state)   //System.Threading.Timer
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
        /// key:orderid; 
        /// </summary>
        ConcurrentDictionary<string, string> iocTimeOutDic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 更新我的订单剩余时间
        /// </summary>
        public void UpdatMyOrdersTimeRemains()
        {
            if (iocTimeOutDic.Count > 0 && Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // var m = ObsMyReqOrders.FirstOrDefault(o => o.OrderID == rs.OrderID);
                    lock (LockObj_MyOrders)
                    {
                        //foreach (DataRow myOrder in this.MyOrdersDT.Rows)
                        foreach(string orderid in iocTimeOutDic.Keys)
                        {
                            DataRow myOrder = this.MyOrdersDT.Rows.Find(orderid);

                            string icotimeout_txt = iocTimeOutDic[orderid];
                            int icotimeout = 0;
                            int res = 0;
                            int.TryParse(icotimeout_txt, out icotimeout);

                            if (string.IsNullOrEmpty(myOrder[RETURN_FIELD].ToString()))
                            {
                                continue;
                            }

                            // 保存剩余时间
                            DateTime transactTime = CommonUtils.ConvertToDateTime(myOrder[RETURN_FIELD].ToString()).AddSeconds(icotimeout);
                            TimeSpan diffTime = transactTime - DateTimeEx.GetNow();
                            res = Convert.ToInt32(diffTime.TotalSeconds);
                            myOrder["RemainSeconds"] = res < 0 ? "0" : res.ToString();

                            if (res > 0 && ((icotimeout - res) >= int.Parse(LocalEnv.ConfInfo.PendingCancelTime)))
                            {
                                if (myOrder["EnableDRPBtn"].ToString() == "False")
                                {
                                    myOrder["EnableDRPBtn"] = true;
                                }
                            }

                            if (myOrder["RemainSeconds"].ToString() == "0")
                                myOrder["EnableDRPBtn"] = false;

                            myOrder.AcceptChanges();
                        }
                    }
                });
            }
           
        }
        #endregion

        /// <summary>
        /// 更新客户信息
        /// </summary>
        public void UpdateCustomInfo(string intentionID, string institutionId, string institutionName,
            string traderID, string traderName)
        {
            lock (LockObj_MyOrders)
            {
                DataRow[] drs = this.MyOrdersDT.Select($"{INTENTIONID_FIELD} = '{intentionID}'");
                if (drs != null && drs.Length > 0)
                {
                    foreach (DataRow dr in drs)
                    {
                        dr[INSTITUTIONID_FIELD] = institutionId;
                        dr[INSTITUTIONNAME_FIELD] = institutionName;
                        dr[TRADERID_FIELD] = traderID;
                        dr[TRADERNAME_FIELD] = traderName;
                    }
                }
            }

            lock (this.LockObj_MyCancelOrders)
            {
                DataRow[] drs = this.MyCancelOrdersDT.Select($"{INTENTIONID_FIELD} = '{intentionID}'");
                if (drs != null && drs.Length > 0)
                {
                    foreach (DataRow dr in drs)
                    {
                        dr[INSTITUTIONID_FIELD] = institutionId;
                        dr[INSTITUTIONNAME_FIELD] = institutionName;
                        dr[TRADERID_FIELD] = traderID;
                        dr[TRADERNAME_FIELD] = traderName;
                    }
                }
            }
        }


        private AsyncThreadQueue<TMessageContent> _msgTask = null;
        private object _appMsg_lockObj = new object();
        private DRPDetail drd = null;
        /// <summary>
        /// 处理定向回价消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {

            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code != "DRPMsgPo")
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_appMsg_lockObj)
                {
                    TMessageContent detail = JsonConvert.DeserializeObject<TMessageContent>(obj.Msg);
                    if (detail.contentType == null)
                    {
                        return;
                    }
                    //报价方
                    if (detail.contentType.Equals("DQ") || detail.contentType.Equals("DQB"))
                    {
                        drd = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(detail.detail);

                        List<DRPValue> Tmp = new List<DRPValue>(drd.iocOrderDqList.Where(p => p.orderStatus == "New"));
                        lock (this.lockObj_MyOrders)
                        {
                            DataRow findDr = this.MyOrdersDT.Rows.Find(drd.order.orderId);
                            if (findDr != null)
                            {
                                findDr[DRPShowInfo_Field] = $"{Tmp.Count}笔";
                                findDr[IsShowDRPProcBtn_Field] = true;
                                this.MyOrdersDT.AcceptChanges();
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 更新定向详情 
        /// </summary>
        /// <param name="key"></param>
        public void UpdateDRP(string key)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                TMessageContent detail = Handler.Drp.DrpHandler.Instance.GetContent(key);
                if (detail != null)
                {
                    //报价方
                    if (detail.contentType.Equals("DQ") || detail.contentType.Equals("DQB"))
                    {
                        drd = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(detail.detail);

                        List<DRPValue> Tmp = new List<DRPValue>(drd.iocOrderDqList.Where(p => p.orderStatus == "New" || p.orderStatus == "Partially_Filled"));

                        lock (LockObj_MyOrders)
                        {
                            DataRow findDr = this.MyOrdersDT.Rows.Find(drd.order.orderId);
                            if (findDr != null)
                            {
                                findDr[DRPShowInfo_Field] = $"{Tmp.Count}笔";
                                findDr[IsShowDRPProcBtn_Field] = true;
                                findDr.AcceptChanges();
                            }
                        }
                    }
                }
                else
                {
                    lock (LockObj_MyOrders)
                    {
                        // DQ方
                        DataRow findDr = this.MyOrdersDT.Rows.Find(key);
                        if (findDr != null)
                        {
                            if (findDr[ALGONAME_ORDER_FIELD].ToString() == "DQ" || findDr[ALGONAME_ORDER_FIELD].ToString() == "DQB")
                            {
                                string tmp = findDr[INFO1_FIELD].ToString();
                                if (JsonSplit.IsJson(tmp))
                                {
                                    DqInfo1Model dim = JsonConvert.DeserializeObject<DqInfo1Model>(tmp);
                                    findDr[DRPShowInfo_Field] = $"{dim.CnPartySubName} {dim.CnBidOrOfr} {dim.Price}";
                                    findDr[IsShowDRPProcBtn_Field] = false;
                                    findDr.AcceptChanges();
                                }
                            }
                        }
                    }
                }
            }), DispatcherPriority.DataBind);
        }



        #region 订单处理线程  Author:chenhu Date:20240625 Func:解决堵塞等问题
        /// <summary>
        /// 更新我的订单线程
        /// </summary>
        private Thread _updateErOrdersDataThread;

        // 行情数据队列
        private ConcurrentQueue<ExecutionReport> dataQueue = new ConcurrentQueue<ExecutionReport>();
        private ManualResetEventSlim dataAvailable = new ManualResetEventSlim(false);

        private void ExecutionReportDataProcessor()
        {
            bool isPermission = Handler.Admin.AdminHandler.Instance.CheckPermission("5007", "500701");
            // 精简模式
            if (isPermission)
            {
                return;
            }

            while (!isdispose)
            {
                // 等待数据可用
                dataAvailable.Wait();

                ExecutionReport data;

                while (dataQueue.TryDequeue(out data))
                {
                    if (data == null) // 生产结束的标记
                        return;

                    // 处理行情数据
                    ProcessExecutionReportData(data);
                }

                // 重置事件
                dataAvailable.Reset();
            }
        }

        /// <summary>
        /// 处理订单数据 Author:chenhu Date:20240625 Func:解决堵塞等问题
        /// </summary>
        /// <param name="data"></param>
        private void ProcessExecutionReportData(ExecutionReport obj)
        {
            // 更新我的订单
            UpdateMyOrdersDT(obj);

            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!(obj.ExecType == "N" || obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "S" || obj.ExecType == "6"))
                    {
                        EmsOrdersHandler.Instance.RemoveCacheDataByStrategyOrderId(obj.OrderID);
                        CrossOrdersProcesser.Processer.RemoveDataRowByStrategyOrderId(obj.OrderID);
                    }

                    //<已撤>挂单的范围包括：人工撤销内部订单
                    //<已撤>挂单的范围不包括：已撤销的点价单，已撤量化订单，IDB发起的撤单
                    if (obj.ExecType == "4" && obj.TimeInForce == "0" && obj.Venues.ToUpper() != "QUANT" && obj.Venues.ToUpper() != "IDB")
                    {
                        Sub_UpdateMyCancelOrders(obj);
                    }
                    else if (obj.ExecType == "4" && obj.VenueTypeGW.ToUpper() == "INNER" && (obj.AlgoName.ToUpper() == "DQ" || obj.AlgoName.ToUpper() == "DQB"))
                    {
                        Sub_UpdateMyCancelOrders(obj);
                    }
                }, DispatcherPriority.Send);
            });
        }

        private void UpdateMyOrdersDT(ExecutionReport obj)
        {
            try
            {

                Application.Current.Dispatcher.Invoke(() =>
                {
                    lock (LockObj_MyOrders)
                    {
                        string orderID = obj.OrderID;
                        string execType = string.Empty;
                        string securityID = string.Empty;

                        DataRow myOrder = this.MyOrdersDT.Rows.Find(orderID);

                        //如果是撤单
                        if (myOrder != null)
                        {
                            execType = CommonUtils.ConvertToString(obj.ExecType);

                            //arg.Add(new KeyValuePair<string, string>("N", "新增"));   //arg.Add(new KeyValuePair<string, string>("0", "正常"));
                            //arg.Add(new KeyValuePair<string, string>("4", "已撤销")); //arg.Add(new KeyValuePair<string, string>("8", "已拒绝"));
                            //arg.Add(new KeyValuePair<string, string>("1", "部分成交"));  //arg.Add(new KeyValuePair<string, string>("2", "完全成交"));
                            //arg.Add(new KeyValuePair<string, string>("S", "报价中"));  //arg.Add(new KeyValuePair<string, string>("6", "撤销中")); //arg.Add(new KeyValuePair<string, string>("C", "异常"));

                            if (!(execType == "N" || execType == "0" || execType == "1" || execType == "S" || execType == "6"))
                            {
                                string icoTimeOut = string.Empty;
                                iocTimeOutDic.TryRemove(obj.OrderID, out icoTimeOut);
                                this.MyOrdersDT.Rows.Remove(myOrder);
                                MyOrdersDT.AcceptChanges();

                                _OnLastExecutions?.Invoke(this.MyOrdersDT.Rows.Count);

                                //高线程优先级去处理数据
                                Thread thread = new Thread(() =>
                                {
                                    UpdateLstOrderDic(obj.SecurityID, obj.Side);
                                    UpdateMMSHELstOrderDic(obj.SecurityID,obj.Venues, obj.Side);
                                // 注销ems消息
                                EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(securityID, CrossOrdersProcesser.Processer.Instance_OnEmsOrders);
                                });

                                thread.Priority = ThreadPriority.AboveNormal;
                                thread.Start();

                                return;
                            }
                        }
                        else
                        {
                            execType = CommonUtils.ConvertToString(obj.ExecType);
                            securityID = CommonUtils.ConvertToString(obj.SecurityID);

                            if (!(execType == "N" || execType == "0" || execType == "1" || execType == "S" || execType == "6"))
                            {
                                return;
                            }

                            Thread thread = new Thread(() =>
                            {
                            // 请求ems消息
                            EmsOrdersHandler.Instance.RegistEMSOrdersBySecurityID(securityID, CrossOrdersProcesser.Processer.Instance_OnEmsOrders);
                            });

                            thread.Priority = ThreadPriority.AboveNormal;
                            thread.Start();
                        }

                        if (myOrder == null)
                        {
                            myOrder = this.MyOrdersDT.NewRow();
                            myOrder[KEY_ORDER_FIELD] = orderID;
                            myOrder[ISCHECKED_ORDER_FIELD] = false;
                            myOrder[ENABLEDBTN_ORDER_FIELD] = true;
                            myOrder[RETURN_FIELD] = DateTimeEx.GetNow().ToString("yyyy-MM-dd HH:mm:ss.fff");


                            myOrder[TERMINAL_FIELD] = obj.Terminal;
                            myOrder[SIDE_ORDER_FIELD] = obj.Side;
                            myOrder[SYMBOL_ORDER_FIELD] = obj.Symbol; // 没有简称
                            myOrder[SECURITYID_ORDER_FIELD] = obj.SecurityID;// obj.SecurityID;
                            if (obj.NetPrice != null && (CommonUtils.IsNumeric(obj.NetPrice)))
                                myOrder[NETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                            else
                                myOrder[NETPRICE_ORDER_FIELD] = DBNull.Value;
                            myOrder[PRICE_ORDER_FIELD] = obj.Price;//CommonUtils.ConvertToDecimal(obj.Price);
                            myOrder[QTY_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.OrderQty);
                            myOrder[DEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                            myOrder[EXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);
                            myOrder[MYDEALER_ORDER_FIELD] = obj.UserName;
                            myOrder[PARTYSUBNAME_FIELD] = obj.PartySubName; // 本方交易员
                            myOrder[SETTLETYPE_ORDER_FIELD] = obj.SettlType; // 清算速度 
                            myOrder[SETTLDATE_ORDER_FIELD] = obj.SettlDate; //交割日
                            myOrder[ORDTYPE_ORDER_FIELD] = obj.AlgoName; // 报价类型, 目前仅有此一种类别X-Bond做市，三阶段将增加：Tradebook，IDB，双边做市
                            myOrder[STCCODE_ORDER_FIELD] = obj.OrderID.EndsWith(CommonConstants.OrderEnd) ? obj.OrderID.Replace(CommonConstants.OrderEnd, "") : obj.OrderID;//obj.OrderID; // STC单号
                            myOrder[VENEUES_ORDER_FIELD] = obj.Venues; // 场所
                            string originalOrderStatus = CommonUtils.ConvertToString(myOrder[ORDERSTATUS_ORDER_FIELD]);
                            this.MyOrdersDT.Rows.Add(myOrder);
                        }


                        if (myOrder != null)
                        {

                            if (string.IsNullOrEmpty(obj.UpdateTime) == false && CommonUtils.ConvertToString(myOrder[UPDATETIME_ORDER_FIELD]).CompareTo(obj.UpdateTime) > 0)
                            {
                                // 如果时间早， 就不更新了。
                                return;
                            }

                            //myOrder[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(orderID);

                            myOrder[TERMINAL_FIELD] = obj.Terminal;

                            if (obj.NetPrice != null && (CommonUtils.IsNumeric(obj.NetPrice)))
                                myOrder[NETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                            else
                                myOrder[NETPRICE_ORDER_FIELD] = DBNull.Value;

                            myOrder[PRICE_ORDER_FIELD] = obj.Price;//CommonUtils.ConvertToDecimal(obj.Price);
                            myOrder[QTY_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.OrderQty);
                            myOrder[DEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                            myOrder[EXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);
                            myOrder[MYDEALER_ORDER_FIELD] = obj.UserName;
                            myOrder[PARTYSUBNAME_FIELD] = obj.PartySubName; // 本方交易员
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
                            myOrder[INFO1_FIELD] = obj.Info1;
                            myOrder[INFO2_FIELD] = obj.Info2;
                            myOrder[INFO3_FIELD] = obj.Info3;
                            myOrder[EXECTYPE_FIELD] = obj.ExecType;
                            //myOrder[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(obj.OrderID);
                            myOrder[IOCTIMEOUT_FIELD] = obj.IocTimeOut;
                            if (obj.AlgoName.ToUpper().Equals("STP"))
                            {
                                myOrder[SEND_ORDER_TYPE_FIELD] = "直通";
                            }
                            else if (obj.AlgoName.ToUpper().Equals("CROSS"))
                            {
                                myOrder[SEND_ORDER_TYPE_FIELD] = "OCO";
                            }
                            else if (obj.AlgoName.ToUpper().Equals("MM"))
                            {
                                myOrder[SEND_ORDER_TYPE_FIELD] = "双边";
                            }


                            if (obj.AlgoName.ToUpper() == "DQ")
                            {
                                myOrder[ORDERTYPE_FIELD] = "定向回价单";
                                myOrder["IsPoint"] = "0";  //是否是点价意向
                                myOrder[EnableDRPBtn_Field] = false;
                            }
                            else
                            {
                                if (obj.TimeInForce == "0")
                                {
                                    myOrder[ORDERTYPE_FIELD] = obj.SecurityID == "99999" ? "模糊需求" : "限价LMT单";
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
                            }

                            if (myOrder["IsPoint"].ToString() == "1" || myOrder["AlgoName"].ToString() == "DQ")
                            {
                                iocTimeOutDic[obj.OrderID] = obj.IocTimeOut;
                            }
                            else
                            {
                                string icoTimeOut = string.Empty;
                                iocTimeOutDic.TryRemove(obj.OrderID, out icoTimeOut);
                            }

                            myOrder["RemainSeconds"] = obj.IocTimeOut == null ? "" : obj.IocTimeOut;  // nofirmtime

                            Common.Model.OrderTimeModel otm = new OrderTimeModel();
                            if (JsonSplit.IsJson(obj.Info1))
                                otm = Newtonsoft.Json.JsonConvert.DeserializeObject<Common.Model.OrderTimeModel>(obj.Info1);

                            // 如果是报价
                            if (obj.TimeInForce == "0")
                            {
                                if (!string.IsNullOrEmpty(obj.Info1))
                                {
                                    //原文
                                    myOrder["Text"] = otm.Text;

                                    myOrder[VALIDUNTILTIME_FIELD] = otm.ValidUntilTime;
                                    if (obj.Info1.StartsWith("{\"ValidTimeType\":\"0|\""))
                                    {
                                        myOrder[REMAINING_TIME_FIELD] = "";
                                    }
                                    else
                                    {
                                        myOrder[REMAINING_TIME_FIELD] = otm.ValidUntilTime;
                                    }
                                }
                            }
                            else
                            {
                                myOrder[VALIDUNTILTIME_FIELD] = "";
                            }

                            if (!string.IsNullOrEmpty(obj.ValidUntilTime))
                            {
                                myOrder[VALIDUNTILTIME_FIELD] = obj.ValidUntilTime;
                                myOrder[REMAINING_TIME_FIELD] = "";
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
                                if (!string.IsNullOrEmpty(obj.SelfSelectVenues))
                                {
                                    string[] arr = obj.SelfSelectVenues.Split(',');
                                    foreach (string v in arr)
                                    {
                                        if (string.IsNullOrEmpty(v) == false)
                                        {
                                            string v_str = ComboSourceUtil.Instance.FindEnum(v.Substring(v.IndexOf('-') + 1), "venues");
                                            displaySB.Append($"<{v_str}>");
                                        }
                                    }
                                }
                                venuesSelectDisplay = $"指定{displaySB}";
                            }
                            //国债期货直接显示渠道=TradeBook
                            if (Consts.MARKETINDICATOR_FUTURE.Equals(obj.MarketIndicator))
                            {
                                venuesSelectDisplay = "TradeBook";
                            }
                            myOrder[VENUES_SELECT_DISPLAY_FIELD] = venuesSelectDisplay;

                            //if (obj.ExecType == "N")
                            //{
                            //    myOrder[RETURN_FIELD] = DateTime.Now.ToString();
                            //    // 如果时间差异过大，并点价订单没有撤销则跳过
                            //    DateTime lasttime = CommonUtils.ConvertToDateTime(myOrder[UPDATETIME_ORDER_FIELD].ToString()).AddMinutes(6);
                            //    if ((lasttime - DateTime.Now).TotalSeconds < 0)
                            //    {
                            //        myOrder[RETURN_FIELD] = null;
                            //        myOrder["RemainSeconds"] = "0";
                            //    }
                            //}

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
                                obj.AlgoName != "XBOND" && obj.AlgoName != "MMSHE" && obj.AlgoName.ToUpper() != "MM" && obj.AlgoName.ToUpper() != "DQ" && obj.Venues != "QUANT" && string.IsNullOrEmpty(obj.IocTimeOut))
                            {
                                myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Visible;
                            }
                            else if (obj.AlgoName == "DQ")
                            {
                                // DQ 允许撤单改单
                                myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Visible;
                            }
                            else
                            {
                                myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Collapsed;
                            }

                            if (obj.AlgoName.ToUpper() == "MM")
                                myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Collapsed;

                            if (obj.AlgoName.ToUpper() == "MMSHE")
                                myOrder[SHOWDELETEBTN_ORDER_FIELD] = Visibility.Collapsed;

                            myOrder[INSTITUTIONID_FIELD] = obj.InstitutionID;
                            myOrder[INSTITUTIONNAME_FIELD] = obj.InstitutionName;
                            myOrder[TRADERID_FIELD] = obj.TraderID;
                            myOrder[TRADERNAME_FIELD] = obj.TraderName;
                            myOrder[SHOW_TRADER_FIELD] = obj.ShowTrader;
                            myOrder[INTENTIONID_FIELD] = obj.Info4;
                            myOrder[MAXFLOOR_FIELD] = obj.MaxFloor;
                            //string newSettleType = string.Empty;
                            //string settleDate = CommonUtils.CalSettlDate(DateTimeEx.ServerTime.ToString("yyyyMMdd"),obj.SettlType, out newSettleType);
                            //myOrder[SETTLDATE_ORDER_FIELD] = settleDate;
                            myOrder[MARKETINDICATOR_FIELD] = obj.MarketIndicator;
                            //开平仓标志
                            myOrder[OPENCLOSE_FIELD] = CommonUtils.GetOffsetFlagDisplay(obj.OffsetFlag);
                            //投机套保套利 1=投机，2=套利，3=套保
                            switch (obj.HedgeFlag)
                            {
                                case Consts.SPECULATE_CODE:
                                    myOrder[SPEC_HEDGE_FIELD] = Consts.SPECULATE;
                                    break;
                                case Consts.ARBITRAGE_CODE:
                                    myOrder[SPEC_HEDGE_FIELD] = Consts.ARBITRAGE;
                                    break;
                                case Consts.HEDGE_CODE:
                                    myOrder[SPEC_HEDGE_FIELD] = Consts.HEDGE;
                                    break;
                            }
                            //资金账号暂时没有
                            //myOrder[CAP_ACCOUNT_FIELD] = obj.HedgeFlag;
                            myOrder[BOOKID_FIELD] = obj.BookID;
                            myOrder[BOOKNAME_FIELD] = !string.IsNullOrEmpty(obj.BookID) && BookDict.ContainsKey(obj.BookID) ? BookDict[obj.BookID] : obj.BookID;
                            //默认4位小数,最小价格变动单位
                            if ("1".Equals(obj.PriceType) && CommonUtils.IsSheBond(obj.SecurityID)) 
                            {
                                myOrder[MINCHANGE_FIELD] = Consts.PRICE_MIN_CHANGE_0001;
                            }
                            else
                            {
                                myOrder[MINCHANGE_FIELD] = CommonUtils.GetMinChangeBySecurityID(obj.SecurityID);
                            }
                            // 账户组合
                            myOrder[AccountID_Field] = string.IsNullOrEmpty(obj.AccountID) ? null : obj.AccountID;
                            myOrder[AccountName_Field] = string.IsNullOrEmpty(obj.AccountName) ? null : obj.AccountName;

                            myOrder[PriceCondition_Field] = string.IsNullOrEmpty(obj.PriceCondition) ? null : obj.PriceCondition;



                            //模糊需求
                            if (obj.SecurityID == "99999")
                            {
                                if (!string.IsNullOrEmpty(otm.QuoteType))
                                {
                                    myOrder[QuoteType_Field] = string.IsNullOrEmpty(otm.QuoteType) ? null : otm.QuoteType;
                                    myOrder[Text_Field] = otm.Text;
                                }

                                myOrder[SYMBOL_ORDER_FIELD] = otm.Text;
                            }
                            else if (obj.MarketIndicator == "2")
                            {
                                if (!string.IsNullOrEmpty(otm.IsFromTableSvrOrTradeBook))
                                    myOrder[QuoteType_Field] = otm.IsFromTableSvrOrTradeBook == "1" ? "irs" : "tbookirs";
                                else
                                    myOrder[QuoteType_Field] = string.IsNullOrEmpty(obj.QuoteType) ? null : obj.QuoteType;
                            }
                            else if (obj.VenueTypeGW == "INNER" && !string.IsNullOrEmpty(obj.IntentionID))
                            {
                                myOrder[QuoteType_Field] = "require";
                            }
                            else
                            {
                                myOrder[QuoteType_Field] = "order";
                            }
                            if ("QDMESP".Equals(obj.VenueTypeGW) || "1".Equals(obj.PriceType))
                            {
                                if (CommonUtils.IsNumeric(obj.Yield))
                                    myOrder[Yield_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);
                                else
                                    myOrder[Yield_FIELD] = null;
                            }
                            else
                            {
                                myOrder[Yield_FIELD] = obj.Price;
                            }

                            if ("7".Equals(obj.PriceType))
                            {
                                if (CommonUtils.IsNumeric(obj.YieldToExecution))
                                    myOrder[YieldToExecution_FIELD] = CommonUtils.ConvertToDecimal(obj.YieldToExecution);
                                else
                                    myOrder[YieldToExecution_FIELD] = null;
                            }
                            else
                            {
                                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(obj.SecurityID);
                                if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                                {
                                    if (CommonUtils.IsNumeric(obj.YieldToExecution))
                                        myOrder[YieldToExecution_FIELD] = CommonUtils.ConvertToDecimal(obj.YieldToExecution);
                                    else
                                        myOrder[YieldToExecution_FIELD] = null;
                                }
                            }
                            if (CommonUtils.IsNumeric(obj.YieldToExecution))
                                myOrder[YieldToExecutionBAK_FIELD] = CommonUtils.ConvertToDecimal(obj.YieldToExecution);
                            else
                                myOrder[YieldToExecutionBAK_FIELD] = null;


                            string intentionid = obj.IntentionID;
                            if (!string.IsNullOrEmpty(intentionid))
                            {
                                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo intentionQuoteInfo = IntentionOldHandler.Instance.GetIntentionByID(intentionid);
                                if (intentionQuoteInfo != null)
                                {
                                    myOrder[INSTITUTIONID_FIELD] = intentionQuoteInfo.institutionId;
                                    myOrder[INSTITUTIONNAME_FIELD] = intentionQuoteInfo.institutionName;
                                    myOrder[TRADERID_FIELD] = intentionQuoteInfo.traderId;
                                    myOrder[TRADERNAME_FIELD] = intentionQuoteInfo.traderName;
                                }
                            }
                            myOrder[CHANNEL_FIELD] = obj.Channel;
                            myOrder.AcceptChanges();

                            _OnLastExecutions?.Invoke(this.MyOrdersDT.Rows.Count);

                            //高线程优先级去处理数据
                            Thread thread = new Thread(() =>
                            {
                                //todo...
                                UpdateLstOrderDic(obj.SecurityID, obj.Side);
                                UpdateMMSHELstOrderDic(obj.SecurityID, obj.Venues, obj.Side);
                                UpdateDRP(orderID);
                            });
                            thread.Priority = ThreadPriority.AboveNormal;
                            thread.Start();
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                LogHelper.Error("MyOrderDt", ex);
            }
        }

        #endregion

    }
}
