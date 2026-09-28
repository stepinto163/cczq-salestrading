
using com.gateway.log;
using BDB.Trade.Plugin.ViewModel.Trade;
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
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Model.Base;
using STC.Common.Data;
using STC.Common.ViewModel;
using CTPriceControler = BDB.Trade.Plugin.ViewModel.Trade.CTPriceControler;

namespace BDB.Trade.Plugin.Handler.DataProcesser
{
    public class MyOrderProcesser
    {

        private static MyOrderProcesser proc = new MyOrderProcesser();

        public static MyOrderProcesser Processer
        {
            get
            {
                return proc;
            }
        }

        private volatile bool isInit = false;

        private MyOrderProcesser()
        {
            //BaseHandler.Instance.Init(Instance_OnClear);
        }

        ~MyOrderProcesser()
        {
            Dispose();
        }

        private object lockObj_MyOrders = new object();
        public object LockObj_MyOrders
        {
            get
            {
                return lockObj_MyOrders;
            }
        }

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
        public const string VENEUES_ORDER_FIELD = "Venues";
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
        public const string IOCTIMEOUT_FIELD = "IocTimeOut";
        public const string ORDERTYPE_FIELD = "ordertype";
        public const string VALIDUNTILTIME_FIELD = "ValidUntilTime";
        public const string ISPOINT_FILED = "IsPoint";
        public const string REMAINSECONDS_FIELD = "RemainSeconds";  //点价倒计时
        public const string PARTYSUBNAME_FIELD = "PartySubName";  //本方交易员
        public const string COUNTERPARTYNAME_FIELD = "CounterPartyName";  //对手交易员
        public const string INFO1_FIELD = "Info1"; //报价有效时长选项 0默认 1时长 2 时点
        public const string REJECTTEXT_FIELD = "RejectText"; //交易失败原因
        #endregion

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
                        _MyOrdersDT.Columns.Add(PRICE_ORDER_FIELD, typeof(decimal));
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
                        _MyOrdersDT.Columns.Add(INFO3_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(EXECTYPE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(WARINGSTATUS_FILED, typeof(int));
                        _MyOrdersDT.Columns.Add(IOCTIMEOUT_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(ORDERTYPE_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(VALIDUNTILTIME_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(ISPOINT_FILED, typeof(string));
                        _MyOrdersDT.Columns.Add(REMAINSECONDS_FIELD, typeof(int));
                        _MyOrdersDT.Columns.Add(PARTYSUBNAME_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(COUNTERPARTYNAME_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(INFO1_FIELD, typeof(string));
                        _MyOrdersDT.Columns.Add(REJECTTEXT_FIELD, typeof(string)); 
                        _MyOrdersDT.PrimaryKey = new DataColumn[] { _MyOrdersDT.Columns[KEY_ORDER_FIELD] };

                        _MyOrdersDT.DefaultView.Sort = $"{UPDATETIME_ORDER_FIELD} desc";
                    }
                    return this._MyOrdersDT;
                }
            }
        }

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

        private AsyncThreadQueue<ExecutionReport> _ProcessMyOrdersThreadQueue;

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

                //CrossOrdersProcesser.Processer.OnWarningOrders += Processer_OnWarningOrders;//todo_lily:

                isInit = true;
            }
        }


        public void Clear()
        {
            OrdersHandler.Instance.OnOwnOrders -= Instance_OnOrder;
            //CrossOrdersProcesser.Processer.OnWarningOrders -= Processer_OnWarningOrders;//todo_lily:

            lock (this.lockObj_MyOrders)
            {
                if (_MyOrdersDT != null)
                {
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

            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
            }

            isInit = false;
        }

        //todo_lily:
        //private void Processer_OnWarningOrders(string strategyOrderID)
        //{
        //    lock (this.lockObj_MyOrders)
        //    {

        //        DataRow findDr = this.MyOrdersDT.Rows.Find(strategyOrderID);
        //        if (findDr != null)
        //        {
        //            findDr[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(strategyOrderID);
        //        }
        //    }
        //}

        public void Dispose()
        {
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

            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
                _ProcessMyOrdersThreadQueue.Dispose();
                _ProcessMyOrdersThreadQueue = null;
            }

            isInit = false;
        }

        public void ResetClear()
        {
            LogHelper.Info("MyOrderProcesser ResetClear.");
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Clear();
                this.isInit = false;
                this.Init();
            }));
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info("MyOrderProcesser Clear");
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
            //UpdateMyOrders(obj);
            if (this._ProcessMyOrdersThreadQueue != null)
            {
                this._ProcessMyOrdersThreadQueue.Enqueue(obj);
            }
        }


        private void UpdateMyOrders(ExecutionReport obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (obj.ExecType == "6")
                {
                    // 如果是存在，关闭编辑窗口
                    //WindowManager.Instance.CloseWindow($"EditOrder-{obj.OrderID}");
                }

                string info = null;
                Sub_UpdateMyOrders(obj);

                //todo_lily:
                //if (!(obj.ExecType == "N" || obj.ExecType == "0" || obj.ExecType == "1" || obj.ExecType == "S" || obj.ExecType == "6"))
                //{
                //    CrossOrdersProcesser.Processer.RemoveDataRowByStrategyOrderId(obj.OrderID);
                //}

                //<已撤>挂单的范围包括：人工撤销内部订单
                //< 已撤 > 挂单的范围不包括：已撤销的点价单，已撤量化订单，IDB发起的撤单
                if (obj.ExecType == "4" && obj.TimeInForce == "0"
                    && obj.Venues.ToUpper() != "QUANT"
                    && obj.Venues.ToUpper() != "IDB")
                {
                    Sub_UpdateMyCancelOrders(obj);
                }

                // 更新撤单信息，应该保含点价
                if (obj.OrdStatus == "C" || obj.OrdStatus == "4" || obj.OrdStatus == "8")
                {
                    Sub_UpdateMyCancelOrders(obj);
                }
            }));
        }

        private void Sub_UpdateMyCancelOrders(ExecutionReport obj)
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
                        myCancelOrder = this.MyCancelOrdersDT.NewRow();
                        myCancelOrder[KEY_ORDER_FIELD] = obj.OrderID;
                        myCancelOrder[ISCHECKED_ORDER_FIELD] = false;
                        myCancelOrder[ENABLEDBTN_ORDER_FIELD] = true;
                        // 增加本方交易员，对手交易员
                        myCancelOrder[PARTYSUBNAME_FIELD] = obj.PartySubName;
                        myCancelOrder[COUNTERPARTYNAME_FIELD] = obj.CounterPartyName;

                        this.MyCancelOrdersDT.Rows.Add(myCancelOrder);
                    }

                    if (myCancelOrder != null)
                    {
                        myCancelOrder[TERMINAL_FIELD] = obj.Terminal;
                        myCancelOrder[SIDE_ORDER_FIELD] = obj.Side;
                        myCancelOrder[SYMBOL_ORDER_FIELD] = obj.Symbol; // 没有简称
                        myCancelOrder[SECURITYID_ORDER_FIELD] = obj.SecurityID;// obj.SecurityID;
                        myCancelOrder[NETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        myCancelOrder[PRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.Price);
                        myCancelOrder[QTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty);
                        myCancelOrder[DEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                        myCancelOrder[EXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);
                        myCancelOrder[MYDEALER_ORDER_FIELD] = obj.UserName; // 本方交易员
                        myCancelOrder[SETTLETYPE_ORDER_FIELD] = obj.SettlType; // 清算速度 
                        myCancelOrder[SETTLDATE_ORDER_FIELD] = obj.SettlDate; //交割日
                        myCancelOrder[ORDTYPE_ORDER_FIELD] = obj.AlgoName; // 报价类型, 目前仅有此一种类别X-Bond做市，三阶段将增加：Tradebook，IDB，双边做市
                        myCancelOrder[STCCODE_ORDER_FIELD] = obj.OrderID; // BDB单号
                        myCancelOrder[VENEUES_ORDER_FIELD] = obj.Venues; // 场所
                        string originalOrderStatus = CommonUtils.ConvertToString(myCancelOrder[ORDERSTATUS_ORDER_FIELD]);
                        //if (originalOrderStatus != "4"
                        //    && obj.OrdStatus == "4")
                        //{
                        //    myOrder[ISCHECKED_ORDER_FIELD] = false;
                        //}
                        myCancelOrder[ORDERSTATUS_ORDER_FIELD] = obj.OrdStatus;// 订单状态
                        // myCancelOrder[TRANSACTTIME_ORDER_FIELD] = obj.TransactTime;
                        myCancelOrder[UPDATETIME_ORDER_FIELD] = obj.UpdateTime; // 撤单发送时间使用updateTime
                        myCancelOrder[ALGONAME_ORDER_FIELD] = obj.AlgoName;// 交易策略，NONO，STP,XBOND,CROSS
                        myCancelOrder[FIRMFLAG_ORDER_FIELD] = obj.FirmFlag; // firm，nonfirm
                        string firmToNonTime = obj.FirmToNonTime == null ? string.Empty : obj.FirmToNonTime;
                        myCancelOrder[NONFIRM_TIME_FIELD] = firmToNonTime; // nofirmtime
                        //myCancelOrder[SECURITY_TYPE] = obj.SecurityType;
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
                        myCancelOrder[VALIDUNTILTIME_FIELD] = obj.ValidUntilTime;
                        myCancelOrder[ISPOINT_FILED] = obj.TimeInForce == "0" ? "0" : "1";  //是否是点价意向

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

                        if (obj.ExecType == "N" || obj.ExecType == "6")
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
                        // 增加本方交易员，对手交易员
                        myCancelOrder[PARTYSUBNAME_FIELD] = obj.PartySubName;
                        myCancelOrder[COUNTERPARTYNAME_FIELD] = obj.CounterPartyName;
                        myCancelOrder[INFO1_FIELD] = obj.Info1;
                        myCancelOrder[REJECTTEXT_FIELD] = obj.RejectText;
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
        /// 更新我的订单剩余时间
        /// </summary>
        public void UpdatMyOrdersTimeRemains()
        {
            for(int i = 0;i<this._MyOrdersDT.Rows.Count;i++)
            {
                DataRow dr =  this._MyOrdersDT.Rows[i];
                if (dr["IsPoint"].ToString() == "1")
                {
                    DataTable tmp = dr.Table.Clone(); // 复制DataRow的表结构
                    tmp.ImportRow(dr); // 将DataRow添加到DataTable中

                    int icotimeout = 0;
                    int res = 0;
                    int.TryParse(dr["IocTimeOut"].ToString(), out icotimeout);

                    // 保存剩余时间
                    DateTime transactTime = CommonUtils.ConvertToDateTime(dr["TransactTime"].ToString()).AddSeconds(icotimeout);
                    TimeSpan diffTime = transactTime - DateTimeEx.ServerTime;
                    res = Convert.ToInt32(diffTime.TotalSeconds);
                    dr.BeginEdit();
                    dr["RemainSeconds"] = res < 0 ? 0 : res;
                    dr.EndEdit();
                    dr.AcceptChanges();
                    
                }
            }
        }
        /// <summary>
        /// 更新数据， 好像没有删除数据？？还有很多字段不知道什么意思??key也不确定
        /// </summary>
        /// <param name="obj"></param>
        private void Sub_UpdateMyOrders(ExecutionReport obj)
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
                            //if (obj.TimeInForce.ToString() != "3")
                            //{
                                this.MyOrdersDT.Rows.Remove(myOrder);

                                UpdateLstOrderDic(obj);

                                //// 注销ems消息
                                //EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(obj.SecurityID, CrossOrdersProcesser.Processer.Instance_OnEmsOrders);//todo_lily:
                            //}
                            //else
                            //{
                            //    UpdateLstOrderDic(obj);
                            //}
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

                        if (obj.TimeInForce == "0")
                        {
                            if (!string.IsNullOrEmpty(myOrder["Info1"].ToString()))
                            {
                                if (myOrder["Info1"].ToString().Contains("|"))
                                {
                                    string info1 = myOrder["Info1"].ToString().Split('|')[0];
                                    myOrder[MyOrderProcesser.REMAINING_TIME_FIELD] = info1 == "0" ? "" : "1";
                                }
                            }
                        }
                        // 请求ems消息
                        //EmsOrdersHandler.Instance.RegistEMSOrdersBySecurityID(obj.SecurityID, CrossOrdersProcesser.Processer.Instance_OnEmsOrders);

                        this.MyOrdersDT.Rows.Add(myOrder);
                    }

                    if (myOrder != null)
                    {
                        myOrder[TERMINAL_FIELD] = obj.Terminal;
                        myOrder[SIDE_ORDER_FIELD] = obj.Side;
                        myOrder[SYMBOL_ORDER_FIELD] = obj.Symbol; // 没有简称
                        myOrder[SECURITYID_ORDER_FIELD] = obj.SecurityID;// obj.SecurityID;
                        myOrder[NETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        myOrder[PRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.Price);
                        myOrder[QTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty);
                        myOrder[DEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                        myOrder[EXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);
                        myOrder[MYDEALER_ORDER_FIELD] = obj.UserName; // 本方交易员
                        myOrder[SETTLETYPE_ORDER_FIELD] = obj.SettlType; // 清算速度 
                        myOrder[SETTLDATE_ORDER_FIELD] = obj.SettlDate; //交割日
                        myOrder[ORDTYPE_ORDER_FIELD] = obj.AlgoName; // 报价类型, 目前仅有此一种类别X-Bond做市，三阶段将增加：Tradebook，IDB，双边做市
                        myOrder[STCCODE_ORDER_FIELD] = obj.OrderID; // BDB单号
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
                        //myOrder[SECURITY_TYPE] = obj.SecurityType;
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
                        //myOrder[WARINGSTATUS_FILED] = CrossOrdersProcesser.Processer.GetWarningStatusByStrategyOrderID(obj.OrderID);//todo_lily:
                        myOrder[IOCTIMEOUT_FIELD] = obj.IocTimeOut;
                        myOrder[VALIDUNTILTIME_FIELD] = obj.ValidUntilTime;
                        // 增加本方交易员，对手交易员
                        myOrder[PARTYSUBNAME_FIELD] = obj.PartySubName;
                        myOrder[COUNTERPARTYNAME_FIELD] = obj.CounterPartyName;

                        if (obj.ExecType == "N")
                        {
                            myOrder[ENABLEDBTN_ORDER_FIELD] = true;
                        }
                        //////测试代码  to do...
                        ////obj.TimeInForce = "3";
                        ////obj.IocTimeOut = "30";
                        ////myOrder[IOCTIMEOUT_FIELD] = "30";
                        ////myOrder[ISPOINT_FILED] = "1";
                        ////// -------------------

                        myOrder[ISPOINT_FILED] = obj.TimeInForce == "0" ? "0":"1";  //是否是点价意向
                        if (obj.TimeInForce == "0")
                        {
                            if (!string.IsNullOrEmpty(myOrder["Info1"].ToString()))
                            {
                                if (myOrder["Info1"].ToString().Contains("|"))
                                {
                                    string info1 = myOrder["Info1"].ToString().Split('|')[0];
                                    myOrder[MyOrderProcesser.REMAINING_TIME_FIELD] = info1 == "0" ? "" : "1";
                                }
                            }
                        }
                        //// 当是点价意向，并且是新订单时 更新剩余时间
                        if (obj.TimeInForce == "3")
                        {
                            if (!string.IsNullOrEmpty(obj.IocTimeOut) && !string.IsNullOrEmpty(obj.TransactTime))
                            {
                                int icotimeout = 0;
                                int res = 0;
                                int.TryParse(obj["IocTimeOut"].ToString(), out icotimeout);

                                // 保存剩余时间
                                DateTime transactTime = CommonUtils.ConvertToDateTime(obj["TransactTime"].ToString()).AddSeconds(icotimeout);
                                TimeSpan diffTime = transactTime - DateTimeEx.ServerTime;
                                res = Convert.ToInt32(diffTime.TotalSeconds);
                                myOrder[REMAINSECONDS_FIELD] = res;
                            }
                        }

                        if (obj.TimeInForce == "0")
                        {
                            myOrder[REMAINSECONDS_FIELD] = 0;
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
                                // myOrder[VALIDUNTILTIME_FIELD] = string.Empty;
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

                        if (obj.ExecType == "N" || obj.ExecType == "6")
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

                        myOrder[INSTITUTIONID_FIELD] = obj.InstitutionID;
                        myOrder[INSTITUTIONNAME_FIELD] = obj.InstitutionName;
                        myOrder[TRADERID_FIELD] = obj.TraderID;
                        myOrder[TRADERNAME_FIELD] = obj.TraderName;
                        myOrder[SHOW_TRADER_FIELD] = obj.ShowTrader;
                        myOrder[INFO1_FIELD] = obj.Info1;
                        myOrder[REJECTTEXT_FIELD] = obj.RejectText; 

                        UpdateLstOrderDic(obj);
                    }
                    myOrder.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateMyOrders", e);
            }

        }

        public DataRow GetOrderById(string orderId)
        {
            return this.MyOrdersDT.Rows.Find(orderId);
        }

        private ConcurrentDictionary<string, DataRowView> _LstOrderDic = new ConcurrentDictionary<string, DataRowView>();
        public Action<ExecutionReport> OnOwnOrders;

        private void UpdateLstOrderDic(ExecutionReport obj)
        {
            this.MyOrdersDT.DefaultView.RowFilter = $"{SECURITYID_ORDER_FIELD} = '{obj.SecurityID}' and {SIDE_ORDER_FIELD} = '{obj.Side}'";

            string lstOrderKey = $"{obj.SecurityID}#{obj.Side}";
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

            OnOwnOrders?.Invoke(obj);
        }

        public DataRowView GetLstOrderBySecurityID(string securityID, string side)
        {
            string lstOrderKey = $"{securityID}#{side}";
            if (_LstOrderDic.ContainsKey(lstOrderKey))
            {
                DataRowView result = _LstOrderDic[lstOrderKey];
                string exectype = CommonUtils.ConvertToString(result[EXECTYPE_FIELD]);
                if (exectype == "0" || exectype == "1" || exectype == "6")
                {
                    return result;
                }
            }
            return null;
        }

        public DataRow GetMyOrderByCondition(string securityID, string side, string settle, string isPoint)
        {
            string rowFilter = string.Empty;
            if (!string.IsNullOrEmpty(settle))
                rowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}' and {settle}";
            else
                rowFilter = $"{SECURITYID_ORDER_FIELD} = '{securityID}' and {SIDE_ORDER_FIELD} = '{side}'";

            if (!string.IsNullOrEmpty(isPoint))
                rowFilter += $" and {ISPOINT_FILED} = '{isPoint}'";


            DataTable dt = CTPriceControler.getDataTable(this.MyOrdersDT, rowFilter);

            if (dt.Rows.Count > 0)
            {
                DataRow result = dt.Rows[0];
                string exectype = CommonUtils.ConvertToString(result[EXECTYPE_FIELD]);
                if (exectype == "0" || exectype == "1" || exectype == "6")
                {
                    return result;
                }
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

                if (!string.IsNullOrEmpty(isPoint))
                {
                    baseFilter += $" and {ISPOINT_FILED} = '{isPoint}'";
                }

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
                    ExecutionReport er = new ExecutionReport();
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
                    ExecutionReport er = new ExecutionReport();
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
                    ExecutionReport er = new ExecutionReport();
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
