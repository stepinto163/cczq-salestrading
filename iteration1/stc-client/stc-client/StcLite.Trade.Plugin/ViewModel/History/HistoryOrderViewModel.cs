using BDB.Trade.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Data;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using System.Threading;
using BDB.Trade.Plugin.CustomeUI;
using System.Windows.Input;
using Microsoft.Win32;
using BDB.Trade.Plugin.View;
using System.IO;
using com.gateway.log;
using STC.Common.Model;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.Model.Fix.Message;
using STC.Common.Handler.TBook;
using STC.Common.Handler.MarketExec;
using TimeUtils = BDB.Trade.Plugin.Utils.TimeUtils;
using STC.Common.View.UI;
using STC.Common.Model.Export;
using STC.Frame.UI;

namespace BDB.Trade.Plugin.ViewModel
{
    public class HistoryOrderViewModel : BaseViewModel
    {
        private HistoryOrderView historyOrderView;

        public HistoryOrderView HistoryOrderView
        {
            get
            {
                return historyOrderView;
            }

            set
            {
                historyOrderView = value;
            }
        }
        public ICommand ExportCommand { get { return new RelayCommand(OnExportClick); } }
        public ICommand EMSExportCommand { get { return new RelayCommand(EMSOnExportClick); } }

        private bool stcCodeStatus = false;
        private string selectSTCCode;

        public string SelectSTCCode
        {
            get
            {
                return selectSTCCode;
            }

            set
            {
                if (selectSTCCode != value)
                {
                    selectSTCCode = value;
                    EMSRequest.strategyorderid = SelectSTCCode;
                    this.OnPropertyChanged();
                    if (!stcCodeStatus)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(o =>
                        {
                            clearEMS();
                            EMSIndex = 0;
                            historyOrderView.ShowEMSMask(true);
                            RequestEMSOrders();
                        });
                    }
                }
            }
        }
        public string stringFormat = "yyyy-MM-dd";
        private int index = 0;
        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                index = value;
            }
        }
        private int emsindex = 0;
        public int EMSIndex
        {
            get
            {
                return emsindex;
            }

            set
            {
                emsindex = value;
            }
        }
        private QueryOrderReq request = new QueryOrderReq();
        public QueryOrderReq Request
        {
            get
            {
                return request;
            }

            set
            {
                request = value;
                this.OnPropertyChanged();
            }
        }
        private QueryOrderReq emsrequest = new QueryOrderReq();
        public QueryOrderReq EMSRequest
        {
            get
            {
                return emsrequest;
            }

            set
            {
                emsrequest = value;
                this.OnPropertyChanged();
            }
        }
        private PageControl _pageControl;
        public PageControl PageControl
        {
            get
            {
                return _pageControl;
            }

            set
            {
                _pageControl = value;
                this.OnPropertyChanged();
            }
        }
        private PageControl _emsPageControl;
        public PageControl EMSPageControl
        {
            get
            {
                return _emsPageControl;
            }

            set
            {
                _emsPageControl = value;
                this.OnPropertyChanged();
            }
        }
        private object lockObj_Orders = new object();
        public object LockObj_Orders
        {
            get
            {
                return lockObj_Orders;
            }
        }

        #region '我的订单'表格字段
        public const string ISCHECKED_ORDER_FIELD = "IsChecked";
        public const string KEY_ORDER_FIELD = "Key";
        public const string SIDE_ORDER_FIELD = "Side";
        public const string SECURITYID_ORDER_FIELD = "SecurityID";
        public const string SYMBOL_ORDER_FIELD = "Symbol";
        public const string NETPRICE_ORDER_FIELD = "NetPrice";
        public const string NETPRICE1_ORDER_FIELD = "NetPrice1";
        public const string PRICE_ORDER_FIELD = "Price";
        public const string QTY_ORDER_FIELD = "Qty";
        public const string DEALINGQTY_ORDER_FIELD = "DealingQty";
        public const string EXECQTY_ORDER_FIELD = "ExecQty";
        public const string MYDEALER_ORDER_FIELD = "MyDealer";
        public const string SETTLETYPE_ORDER_FIELD = "SettlType";
        public const string ORDTYPE_ORDER_FIELD = "OrdType";
        public const string STCCODE_ORDER_FIELD = "STCCode";
        public const string VENEUES_ORDER_FIELD = "Veneues";
        public const string ORDERSTATUS_ORDER_FIELD = "OrderStatus";
        public const string EXECTYPE_FIELD = "ExecType";
        public const string TRANSACTTIME_ORDER_FIELD = "TransactTime";
        public const string ALGONAME_ORDER_FIELD = "AlgoName";
        public const string FIRMFLAG_ORDER_FIELD = "FirmFlag";
        public const string NONFIRM_TIME_FIELD = "NonfirmTime";
        public const string REMAINING_TIME_FIELD = "RemainingTime";
        public const string SHOWDELETEBTN_ORDER_FIELD = "ShowDeleteBtn";
        public const string WHOLE_FLAG_FIELD = "WholeFlag";
        public const string CROSS_VENUES_FIELD = "CrossVenues";
        public const string VENUES_SELECT_DISPLAY_FIELD = "VenuesSelectDisplay";

        public const string CREATE_DATE_FIELD = "createDate";// 创建日期

        public const string TIMEINFORCE_FIELD = "TimeInForce";
        public const string UPDATETIME_FIELD = "UpdateTime";
        public const string IS_EXPANDED_FIELD = "IsExpanded";
        public const string ISPOINT_FILED = "IsPoint";
        #endregion

        private DataView _OrdersDV;
        public DataView OrdersDV
        {
            get
            {
                lock (LockObj_Orders)
                {
                    if (_OrdersDV == null)
                    {
                        _OrdersDV = new DataTable().DefaultView;
                        _OrdersDV.Table.Columns.Add(ISCHECKED_ORDER_FIELD, typeof(bool));
                        _OrdersDV.Table.Columns.Add(KEY_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(SIDE_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(SYMBOL_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(SECURITYID_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(NETPRICE_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(NETPRICE1_ORDER_FIELD, typeof(decimal));
                        _OrdersDV.Table.Columns.Add(PRICE_ORDER_FIELD, typeof(decimal));
                        _OrdersDV.Table.Columns.Add(QTY_ORDER_FIELD, typeof(long));
                        _OrdersDV.Table.Columns.Add(DEALINGQTY_ORDER_FIELD, typeof(long));
                        _OrdersDV.Table.Columns.Add(EXECQTY_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(MYDEALER_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(SETTLETYPE_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(ORDTYPE_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(STCCODE_ORDER_FIELD, typeof(string));

                        _OrdersDV.Table.Columns.Add(CREATE_DATE_FIELD, typeof(string));

                        _OrdersDV.Table.Columns.Add(VENEUES_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(ORDERSTATUS_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(EXECTYPE_FIELD,typeof(string));
                        _OrdersDV.Table.Columns.Add(TRANSACTTIME_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(ALGONAME_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(FIRMFLAG_ORDER_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(NONFIRM_TIME_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(REMAINING_TIME_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(SHOWDELETEBTN_ORDER_FIELD, typeof(Visibility));
                        _OrdersDV.Table.Columns.Add(WHOLE_FLAG_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(CROSS_VENUES_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(TIMEINFORCE_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(VENUES_SELECT_DISPLAY_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(UPDATETIME_FIELD, typeof(string));
                        _OrdersDV.Table.Columns.Add(IS_EXPANDED_FIELD, typeof(bool));
                        _OrdersDV.Table.Columns.Add(ISPOINT_FILED, typeof(string));
                        _OrdersDV.Sort = $"{TRANSACTTIME_ORDER_FIELD} desc";
                        _OrdersDV.Table.PrimaryKey = new DataColumn[] { _OrdersDV.Table.Columns[KEY_ORDER_FIELD] };

                    }
                    return this._OrdersDV;
                }
            }
        }
        private object lockObj_EMSOrders = new object();
        public object LockObj_EMSOrders
        {
            get
            {
                return lockObj_EMSOrders;
            }
        }
        #region 'ems订单'表格字段
        public const string EMSISCHECKED_ORDER_FIELD = "IsChecked";
        public const string EMSKEY_ORDER_FIELD = "Key";
        public const string EMS_EXEC_ID_FIELD = "ExecID";
        public const string EMS_CLORD_ID_FIELD = "ClOrdID";
        public const string EMS_STRATEGY_ORDER_ID_FIELD = "StrategyOrderId";
        public const string EMSSIDE_ORDER_FIELD = "Side";
        public const string EMSSECURITYID_ORDER_FIELD = "SecurityID";
        public const string EMSSYMBOL_ORDER_FIELD = "Symbol";
        public const string EMSNETPRICE_ORDER_FIELD = "NetPrice";
        public const string EMSNETPRICE1_ORDER_FIELD = "NetPrice1";
        public const string EMSPRICE_ORDER_FIELD = "Price";
        public const string EMSQTY_ORDER_FIELD = "Qty";
        public const string EMSDEALINGQTY_ORDER_FIELD = "DealingQty";
        public const string EMSEXECQTY_ORDER_FIELD = "ExecQty";
        public const string EMSMYDEALER_ORDER_FIELD = "MyDealer";
        public const string EMSSETTLETYPE_ORDER_FIELD = "SettlType";
        public const string EMSORDTYPE_ORDER_FIELD = "OrdType";
        public const string EMSSTCCODE_ORDER_FIELD = "STCCode";
        public const string EMSVENEUES_ORDER_FIELD = "Veneues";
        public const string EMSORDERSTATUS_ORDER_FIELD = "OrderStatus";
        public const string EMSEXECTYPE_FIELD = "ExecType";
        public const string EMSTRANSACTTIME_ORDER_FIELD = "TransactTime";
        public const string EMSALGONAME_ORDER_FIELD = "AlgoName";
        public const string EMSFIRMFLAG_ORDER_FIELD = "FirmFlag";
        public const string EMSNONFIRM_TIME_FIELD = "NonfirmTime";
        public const string EMSREMAINING_TIME_FIELD = "RemainingTime";
        public const string EMSSHOWDELETEBTN_ORDER_FIELD = "ShowDeleteBtn";
        public const string EMSWHOLE_FLAG_FIELD = "WholeFlag";
        public const string EMSCROSS_VENUES_FIELD = "CrossVenues";
        public const string EMSVENUES_SELECT_DISPLAY_FIELD = "VenuesSelectDisplay";
        public const string EMSTIMEINFORCE_FIELD = "TimeInForce";
        public const string EMSUPDATETIME_FIELD = "UpdateTime";
        public const string EMSSTRATEGYORDERVENEUES_ORDER_FIELD = "Strategyordervenues";
        public const string EMS_CREATE_DATE_FIELD = "createDate";// 创建日期
        #endregion
        private DataView _EMSOrdersDV;
        public DataView EMSOrdersDV
        {
            get
            {
                lock (LockObj_EMSOrders)
                {
                    if (_EMSOrdersDV == null)
                    {
                        _EMSOrdersDV = new DataTable().DefaultView;
                        _EMSOrdersDV.Table.Columns.Add(EMSISCHECKED_ORDER_FIELD, typeof(bool));
                        _EMSOrdersDV.Table.Columns.Add(EMSKEY_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMS_EXEC_ID_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMS_CLORD_ID_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMS_STRATEGY_ORDER_ID_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSSIDE_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSSYMBOL_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSSECURITYID_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSNETPRICE_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSNETPRICE1_ORDER_FIELD, typeof(decimal));
                        _EMSOrdersDV.Table.Columns.Add(EMSPRICE_ORDER_FIELD, typeof(decimal));
                        _EMSOrdersDV.Table.Columns.Add(EMSQTY_ORDER_FIELD, typeof(long));
                        _EMSOrdersDV.Table.Columns.Add(EMSDEALINGQTY_ORDER_FIELD, typeof(long));
                        _EMSOrdersDV.Table.Columns.Add(EMSEXECQTY_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSMYDEALER_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSSETTLETYPE_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSORDTYPE_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSSTCCODE_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSVENEUES_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSORDERSTATUS_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSEXECTYPE_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSTRANSACTTIME_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSALGONAME_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSFIRMFLAG_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSNONFIRM_TIME_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSREMAINING_TIME_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSSHOWDELETEBTN_ORDER_FIELD, typeof(Visibility));
                        _EMSOrdersDV.Table.Columns.Add(EMSWHOLE_FLAG_FIELD, typeof(string));

                        _EMSOrdersDV.Table.Columns.Add(EMS_CREATE_DATE_FIELD, typeof(string));

                        _EMSOrdersDV.Table.Columns.Add(EMSCROSS_VENUES_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSVENUES_SELECT_DISPLAY_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSTIMEINFORCE_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSUPDATETIME_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(EMSSTRATEGYORDERVENEUES_ORDER_FIELD, typeof(string));
                        _EMSOrdersDV.Table.Columns.Add(ISPOINT_FILED, typeof(string));
                        _EMSOrdersDV.Sort = $"{EMSTRANSACTTIME_ORDER_FIELD} desc";
                        _EMSOrdersDV.Table.PrimaryKey = new DataColumn[] { _EMSOrdersDV.Table.Columns[EMSKEY_ORDER_FIELD] };

                    }
                    return this._EMSOrdersDV;
                }
            }
        }
        private DataRowView _selectedItem_Orders;

        /// <summary>
        /// 我的订单选中行记录
        /// </summary>
        public DataRowView SelectedItem_Orders
        {
            get
            {
                return _selectedItem_Orders;
            }

            set
            {
                _selectedItem_Orders = value;
            }
        }
        private string _searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged();

                    //FilterOrderData();

                }
            }
        }
        private string pageVisible;

        public string PageVisible
        {
            get
            {
                return pageVisible;
            }

            set
            {
                pageVisible = value;
                this.OnPropertyChanged();
            }
        }
        private string emsPageVisible;

        public string EMSPageVisible
        {
            get
            {
                return emsPageVisible;
            }

            set
            {
                emsPageVisible = value;
                this.OnPropertyChanged();
            }
        }

        private int totalCount;
        public int TotalCount
        {
            get
            {
                return totalCount;
            }

            set
            {
                totalCount = value;
                this.OnPropertyChanged();
            }
        }
        private int emsTotalCount;
        public int EMSTotalCount
        {
            get
            {
                return emsTotalCount;
            }

            set
            {
                emsTotalCount = value;
                this.OnPropertyChanged();
            }
        }
        public static List<KeyValuePair<string, string>> SideItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Side");
            }
        }

        public static List<KeyValuePair<string, string>> SettleTypeItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("SettleType");
            }
        }

        public static List<KeyValuePair<string, string>> VenuesItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Venues");
            }
        }

        private DataRowView lastOrder;
        public DataRowView LastOrder
        {
            get
            {
                return lastOrder;
            }

            set
            {
                lastOrder = value;
                this.OnPropertyChanged();
                if (lastOrder != null)
                {
                    LastOrderid = lastOrder[KEY_ORDER_FIELD] as string;
                }
                else
                {
                    LastOrderid = "";
                }
                //LastIsExpander = lastOrder[IS_EXPANDED_FIELD] as string;
            }
        }
        public string ClickNum
        {
            get
            {
                return clickNum;
            }

            set
            {
                clickNum = value;
            }
        }

        public bool StcCodeStatus
        {
            get
            {
                return stcCodeStatus;
            }

            set
            {
                stcCodeStatus = value;
            }
        }

        private string lastOrderid;
        public string LastOrderid
        {
            get
            {
                return lastOrderid;
            }

            set
            {
                lastOrderid = value;
            }
        }

        private string lastIsExpander;
        public string LastIsExpander
        {
            get
            {
                return lastIsExpander;
            }

            set
            {
                lastIsExpander = value;
            }
        }

        public HistoryOrderViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(OrdersDV, LockObj_Orders);
            BindingOperations.EnableCollectionSynchronization(EMSOrdersDV, LockObj_EMSOrders);
            _ProcessOrdersThreadQueue = new Utils.ThreadQueue<ExecutionReport>(UpdateOrders);
            _ProcessEMSOrdersThreadQueue = new Utils.ThreadQueue<ExecutionReport>(UpdateEMSOrders);
            day = DateTimeEx.ServerTime.AddDays(-1).ToString("yyyy-MM-dd");
            today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            request.dealTimeBegin = day;
            request.dealTimeEnd = today;
            emsrequest.dealTimeBegin = day;
            emsrequest.dealTimeEnd = today;
        }
        private Utils.ThreadQueue<ExecutionReport> _ProcessOrdersThreadQueue;
        private Utils.ThreadQueue<ExecutionReport> _ProcessEMSOrdersThreadQueue;
        private string day = "";
        private string today = "";
        public void Init()
        {
            Index = 0;
            EMSIndex = 0;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                historyOrderView.IsLoad = false;
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                clear();
                clearEMS();
                historyOrderView.ShowMask(true);
                RequestOrders();

                historyOrderView.ShowEMSMask(true);
                RequestEMSOrders();
                //}));
            });
        }
        public void clear()
        {
            try
            {
                lock (OrdersDV)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        //for (int i = 0; i < this.OrdersDV.Table.Rows.Count; i++)
                        //{
                        //    this.OrdersDV.Table.Rows.RemoveAt(i);
                        //}
                        for (int drcount = this.OrdersDV.Table.Rows.Count - 1; drcount >= 0; drcount--)
                        {
                            DataRow dr = this.OrdersDV.Table.Rows[drcount];
                            dr.BeginEdit();
                            dr.Delete();
                            dr.EndEdit();
                        }
                        //this.OrdersDV.Table.Rows.Clear();
                        this.OrdersDV.Table.AcceptChanges();
                    }));
                }
            }
            catch (Exception e)
            {

            }
        }
        public void clearEMS()
        {
            try
            {
                lock (EMSOrdersDV)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        for (int drcount = this.EMSOrdersDV.Table.Rows.Count - 1; drcount >= 0; drcount--)
                        {
                            DataRow dr = this.EMSOrdersDV.Table.Rows[drcount];
                            dr.BeginEdit();
                            dr.Delete();
                            dr.EndEdit();
                        }
                        this.EMSOrdersDV.Table.AcceptChanges();
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        public void RequestOrders()
        {
            try
            {
                //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                //request.orderStatus = "N,0,1";

                long start = DateTime.Now.Ticks;
                request.userName = LocalEnv.UserInfo.UserName;
                request.pageSize = CommonConstants.pageCount;
                request.pageNum = Index;
                ///获取订单 
                var reply = TBookHandler.Instance.QueryOrderForTBook(request);

                if (reply != null && reply.datas != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        TotalCount = reply.totalSize;
                        PageControl.RecordCount = reply.totalSize;
                        if (TotalCount > CommonConstants.pageCount)
                        {
                            PageVisible = "0";
                        }
                        else
                        {
                            PageVisible = "";
                        }
                        foreach (ExecutionReport obj in reply.datas)
                        {
                            lock (string.Intern(obj.OrderID))
                            {
                                UpdateOrders(obj);
                                //this._ProcessOrdersThreadQueue.Enqueue(obj);
                            }
                        }
                    }));

                    long end = DateTime.Now.Ticks;
                    Console.WriteLine("RequestOrders time :" + (end - start) / 10000000.0);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestOrders", e);
            }
            finally
            {
                historyOrderView.ShowMask(false);
            }
        }
        public void RequestEMSOrders()
        {
            try
            {
                //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                //request.orderStatus = "N,0,1";
                long start = DateTime.Now.Ticks;
                emsrequest.userName = LocalEnv.UserInfo.UserName;
                //EMSOrdersDV = new DataVie
                emsrequest.pageSize = CommonConstants.pageCount;
                emsrequest.pageNum = EMSIndex;
                ///获取订单 
                var reply = MarketExecHandler.Instance.QueryOrderForEMS(emsrequest);

                if (reply != null && reply.datas != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        EMSTotalCount = reply.totalSize;
                        EMSPageControl.RecordCount = reply.totalSize;
                        if (EMSTotalCount > CommonConstants.pageCount)
                        {
                            EMSPageVisible = "0";
                        }
                        else
                        {
                            EMSPageVisible = "";
                        }
                        foreach (ExecutionReport obj in reply.datas)
                        {
                            lock (string.Intern(obj.ClOrdID))
                            {
                                UpdateEMSOrders(obj);
                                //this._ProcessEMSOrdersThreadQueue.Enqueue(obj);
                            }
                        }
                        historyOrderView.IsDouble = "false";
                    }));
                }

                long end = DateTime.Now.Ticks;
                Console.WriteLine("RequestEMSOrders time :" + (end - start) / 10000000.0);
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestEMSOrders", e);
            }
            finally
            {
                historyOrderView.ShowEMSMask(false);
                historyOrderView.IsLoad = true;
            }
        }
        private void UpdateOrders(ExecutionReport obj)
        {
            try
            {
                lock (OrdersDV)
                {
                    long start = DateTime.Now.Ticks;
                    DataRow dr = this.OrdersDV.Table.Rows.Find(obj.OrderID);

                    if (dr != null)
                    {
                    }
                    else
                    {
                        dr = this.OrdersDV.Table.NewRow();
                        dr.BeginEdit();
                        dr[KEY_ORDER_FIELD] = obj.OrderID;
                        dr[ISCHECKED_ORDER_FIELD] = false;

                        this.OrdersDV.Table.Rows.Add(dr);
                    }

                    if (dr != null)
                    {
                        dr[SIDE_ORDER_FIELD] = obj.Side;
                        dr[SYMBOL_ORDER_FIELD] = obj.Symbol; // 没有简称
                        dr[SECURITYID_ORDER_FIELD] = obj.SecurityID;// obj.SecurityID;
                        dr[NETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice) == 0 ? "--" : string.Format("{0:N4}", CommonUtils.ConvertToDecimal(obj.NetPrice));
                        dr[NETPRICE1_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        dr[PRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.Price);
                        dr[QTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty);
                        dr[DEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                        dr[EXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty) == 0 ? "" : CommonUtils.ConvertToLong(obj.CumQty).ToString();// CommonUtils.ConvertToLong(obj.CumQty);
                        dr[MYDEALER_ORDER_FIELD] = obj.PartySubName; // 本方交易员
                        dr[SETTLETYPE_ORDER_FIELD] = obj.SettlType; // 清算速度 
                        dr[ORDTYPE_ORDER_FIELD] = obj.AlgoName; // 报价类型, 目前仅有此一种类别X-Bond做市，三阶段将增加：Tradebook，IDB，双边做市
                        dr[STCCODE_ORDER_FIELD] = obj.OrderID; // BDB单号
                        dr[VENEUES_ORDER_FIELD] = obj.Venues; // 场所
                        dr[ORDERSTATUS_ORDER_FIELD] = obj.OrdStatus;// 订单状态
                        dr[EXECTYPE_FIELD] = obj.ExecType;
                        dr[TRANSACTTIME_ORDER_FIELD] = obj.TransactTime; // 发送时间

                        dr[CREATE_DATE_FIELD] = obj.TransactTime;

                        dr[ALGONAME_ORDER_FIELD] = obj.AlgoName;// 交易策略，NONO，STP,XBOND,CROSS
                        dr[FIRMFLAG_ORDER_FIELD] = obj.FirmFlag; // firm，nonfirm
                        string firmToNonTime = obj.FirmToNonTime == null ? string.Empty : obj.FirmToNonTime;
                        dr[NONFIRM_TIME_FIELD] = firmToNonTime; // nofirmtime
                        dr[REMAINING_TIME_FIELD] = firmToNonTime;
                        dr[WHOLE_FLAG_FIELD] = obj.WholeFlag; // 是否整量
                        dr[CROSS_VENUES_FIELD] = obj.CrossVenues; // 撮合渠道
                        dr[ISPOINT_FILED] = obj.TimeInForce == "0" ? "0" : "1";  //是否是点价意向
                        //dr[TIMEINFORCE_FIELD] = obj.TimeInForce;
                        if (obj.TimeInForce == "0")
                        {
                            dr[TIMEINFORCE_FIELD] = "限价LMT单";
                        }
                        else if (obj.TimeInForce == "3")
                        {
                            if (string.IsNullOrEmpty(obj.IocTimeOut))
                            {
                                dr[TIMEINFORCE_FIELD] = "点价IOC单";
                            }
                            else
                            {
                                dr[TIMEINFORCE_FIELD] = "点价FAK单";
                            }
                        }

                        dr[UPDATETIME_FIELD] = obj.UpdateTime;
                        //myOrder[VENUES_SELECT_F_FIELD] = obj.VenuesSelectF; //渠道类型  不填写1,默认2，指定3 
                        //myOrder[VENUES_RDM_NUM_FIELD] = obj.VenuesRdmNum; //渠道类型默认，填写随机几家渠道
                        //myOrder[SELF_SELECT_VENUES_FIELD] = obj.SelfSelectVenues; // 指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC 

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
                        dr[VENUES_SELECT_DISPLAY_FIELD] = venuesSelectDisplay;
                        if (lastOrder != null && LastOrderid.Equals(dr[KEY_ORDER_FIELD]))
                            dr[IS_EXPANDED_FIELD] = true;
                        else
                            dr[IS_EXPANDED_FIELD] = false;
                    }

                    dr.EndEdit();
                    this.OrdersDV.Table.AcceptChanges();
                    long end = DateTime.Now.Ticks;
                    Console.WriteLine("UpdateOrders time :" + (end - start) / 10000000.0);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateMyOrders", e);
            }
        }

        private void UpdateEMSOrders(ExecutionReport obj)
        {
            try
            {
                lock (EMSOrdersDV)
                {
                    long start = DateTime.Now.Ticks;
                    Console.WriteLine(DateTime.Now.Second);
                    DataRow myOrder = this.EMSOrdersDV.Table.Rows.Find(obj.OrderID);

                    if (myOrder != null)
                    {
                    }
                    else
                    {
                        myOrder = this.EMSOrdersDV.Table.NewRow();
                        myOrder.BeginEdit();
                        myOrder[EMSKEY_ORDER_FIELD] = obj.ClOrdID;
                        myOrder[EMSISCHECKED_ORDER_FIELD] = false;

                        // 请求ems消息
                        //EmsOrdersHandler.Instance.RequestEmsOrders(obj.SecurityID);

                        this.EMSOrdersDV.Table.Rows.Add(myOrder);
                    }

                    if (myOrder != null)
                    {
                        myOrder[EMSSIDE_ORDER_FIELD] = obj.Side;
                        myOrder[EMSSYMBOL_ORDER_FIELD] = obj.Symbol; // 没有简称
                        myOrder[EMSSECURITYID_ORDER_FIELD] = obj.SecurityID;// obj.SecurityID;
                        myOrder[EMSNETPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice) == 0 ? "--" : string.Format("{0:N4}", CommonUtils.ConvertToDecimal(obj.NetPrice));
                        myOrder[EMSNETPRICE1_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        myOrder[EMSPRICE_ORDER_FIELD] = CommonUtils.ConvertToDecimal(obj.Price);
                        myOrder[EMSQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty);
                        myOrder[EMSDEALINGQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.OpenQty); // 已处理
                        myOrder[EMSEXECQTY_ORDER_FIELD] = CommonUtils.ConvertToLong(obj.CumQty) == 0 ? "" : CommonUtils.ConvertToLong(obj.CumQty).ToString();
                        myOrder[EMSMYDEALER_ORDER_FIELD] = obj.PartySubName; // 本方交易员
                        myOrder[EMSSETTLETYPE_ORDER_FIELD] = obj.SettlType; // 清算速度 
                        myOrder[EMSORDTYPE_ORDER_FIELD] = obj.AlgoName; // 报价类型, 目前仅有此一种类别X-Bond做市，三阶段将增加：Tradebook，IDB，双边做市
                        myOrder[EMSSTCCODE_ORDER_FIELD] = obj.ClOrdID; // BDB单号
                        myOrder[EMSVENEUES_ORDER_FIELD] = obj.Venues; // 场所
                        myOrder[EMSORDERSTATUS_ORDER_FIELD] = obj.OrdStatus;// 订单状态
                        myOrder[EMSEXECTYPE_FIELD] = obj.ExecType;
                        myOrder[EMSTRANSACTTIME_ORDER_FIELD] = obj.TransactTime; // 发送时间

                        myOrder[CREATE_DATE_FIELD] = obj.TransactTime;

                        myOrder[EMSALGONAME_ORDER_FIELD] = obj.AlgoName;// 交易策略，NONO，STP,XBOND,CROSS
                        myOrder[EMSFIRMFLAG_ORDER_FIELD] = obj.FirmFlag; // firm，nonfirm
                        string firmToNonTime = obj.FirmToNonTime == null ? string.Empty : obj.FirmToNonTime;
                        myOrder[EMSNONFIRM_TIME_FIELD] = firmToNonTime; // nofirmtime
                        myOrder[EMSREMAINING_TIME_FIELD] = firmToNonTime;
                        myOrder[EMSWHOLE_FLAG_FIELD] = obj.WholeFlag; // 是否整量

                        myOrder[EMS_CREATE_DATE_FIELD] = obj.TransactTime;

                        myOrder[EMSCROSS_VENUES_FIELD] = obj.CrossVenues; // 撮合渠道
                        myOrder[EMSTIMEINFORCE_FIELD] = obj.TimeInForce;
                        myOrder[ISPOINT_FILED] = obj.TimeInForce == "0" ? "0" : "1";  //是否是点价意向
                        //if ("0".Equals(obj.TimeInForce) && "1".Equals(obj.OCOFlag))
                        //{
                        //    myOrder[EMSTIMEINFORCE_FIELD] = "限价单(oco)";
                        //}
                        //else
                        //{
                        //    myOrder[EMSTIMEINFORCE_FIELD] = ComboSourceUtil.Instance.FindEnum(obj.TimeInForce, "timeinforce");
                        //}
                        //po.TimeInForce = ComboSourceUtil.Instance.FindEnum(obj.TimeInForce, "timeinforce");
                        if (obj.TimeInForce == "0")
                        {
                            myOrder[EMSTIMEINFORCE_FIELD] = "限价LMT单";
                        }
                        else if (obj.TimeInForce == "3")
                        {
                            if (string.IsNullOrEmpty(obj.IocTimeOut))
                            {
                                myOrder[EMSTIMEINFORCE_FIELD] = "点价IOC单";
                            }
                            else
                            {
                                myOrder[EMSTIMEINFORCE_FIELD] = "点价FAK单";
                            }
                        }
                        myOrder[EMSUPDATETIME_FIELD] = obj.UpdateTime;
                        //myOrder[VENUES_SELECT_F_FIELD] = obj.VenuesSelectF; //渠道类型  不填写1,默认2，指定3 
                        //myOrder[VENUES_RDM_NUM_FIELD] = obj.VenuesRdmNum; //渠道类型默认，填写随机几家渠道
                        //myOrder[SELF_SELECT_VENUES_FIELD] = obj.SelfSelectVenues; // 指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC 

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
                        myOrder[EMSVENUES_SELECT_DISPLAY_FIELD] = venuesSelectDisplay;
                    }
                    myOrder[EMS_EXEC_ID_FIELD] = obj.OrderID;
                    myOrder[EMS_STRATEGY_ORDER_ID_FIELD] = obj.StrategyOrderId;
                    myOrder[EMS_CLORD_ID_FIELD] = obj.ClOrdID;
                    myOrder[EMSSTRATEGYORDERVENEUES_ORDER_FIELD] = obj.StrategyOrderVenues;
                    myOrder.EndEdit();
                    this.EMSOrdersDV.Table.AcceptChanges();
                    long end = DateTime.Now.Ticks;
                    Console.WriteLine("UpdateEMSOrders time :" + (end - start) / 10000000.0);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateEMSOrders", e);
            }
        }

        ~HistoryOrderViewModel()
        {
            this.Dispose();
        }
        new public void Dispose()
        {
            if (_ProcessOrdersThreadQueue != null)
            {
                _ProcessOrdersThreadQueue.Dispose();
            }

            if (_ProcessEMSOrdersThreadQueue != null)
            {
                _ProcessEMSOrdersThreadQueue.Dispose();
            }
            // 释放我的订单数据
            lock (LockObj_Orders)
            {
                if (_OrdersDV != null)
                {
                    _OrdersDV.Dispose();
                    _OrdersDV = null;
                }
            }
            lock (LockObj_EMSOrders)
            {
                if (_EMSOrdersDV != null)
                {
                    _EMSOrdersDV.Dispose();
                    _EMSOrdersDV = null;
                }
            }
            base.Dispose();
        }
        /// <summary>
        /// 处理我的订单的过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessOrdersFilterCondition(Dictionary<string, IList> filterDic)
        {
            if (filterDic.ContainsKey("TransactDate"))
            {
                IList dateList = filterDic["TransactDate"];
                if (dateList.Count > 0)
                {
                    DateTime begin = Convert.ToDateTime(dateList[0].ToString());
                    DateTime end = Convert.ToDateTime(dateList[1].ToString());
                    bool status = TimeUtils.Instance.TimeLengthMoreTwoYear(begin, end);
                    if (status)
                    {
                        showMessage("界面不支持2年以上订单筛选，请修改日期区间");
                        return;
                    }
                    Request.dealTimeBegin = dateList[0].ToString();
                    Request.dealTimeEnd = dateList[1].ToString();
                }
                else
                {
                    Request.dealTimeBegin = day;
                    Request.dealTimeEnd = today;
                }
            }
            // 人工，IDB， 量化
            if (filterDic.ContainsKey("Venues"))
            {
                IList venuesList = filterDic["Venues"];
                StringBuilder inFilterSB_venues = new StringBuilder();
                if (venuesList.Count > 0)
                {
                    foreach (ListBoxItem item in venuesList)
                    {
                        if (inFilterSB_venues.Length > 0)
                        {
                            inFilterSB_venues.Append(",");
                        }

                        if (item.Content.ToString() == CommonConstants.MANUAL)
                        {
                            inFilterSB_venues.Append("MANUAL");
                        }
                        else if (item.Content.ToString() == CommonConstants.IDB)
                        {
                            inFilterSB_venues.Append("IDB");
                        }
                        else if (item.Content.ToString() == CommonConstants.QUANT)
                        {
                            inFilterSB_venues.Append("QUANT");
                        }
                    }
                }

                if (inFilterSB_venues.Length > 0)
                {
                    Request.venues = inFilterSB_venues.ToString();
                }
                else
                {
                    Request.venues = "";
                }
            }

            // firm，nonfirm
            if (filterDic.ContainsKey("FirmFlag"))
            {
                IList list = filterDic["FirmFlag"];
                StringBuilder inFilterSB_FirmFlag = new StringBuilder();
                if (list.Count > 0)
                {
                    foreach (ListBoxItem item in list)
                    {
                        if (inFilterSB_FirmFlag.Length > 0)
                        {
                            inFilterSB_FirmFlag.Append(",");
                        }

                        if (item.Content.ToString() == CommonConstants.FIRM)
                        {
                            inFilterSB_FirmFlag.Append("1");
                        }
                        else if (item.Content.ToString() == CommonConstants.NONFIRM)
                        {
                            inFilterSB_FirmFlag.Append("0");
                        }
                    }
                }

                if (inFilterSB_FirmFlag.Length > 0)
                {
                    Request.firmflag = inFilterSB_FirmFlag.ToString();
                }
                else
                {
                    Request.firmflag = "";
                }
            }
            if (filterDic.ContainsKey("OrderStatus"))
            {
                IList statusList = filterDic["OrderStatus"];
                StringBuilder inFilterSB_Status = new StringBuilder();
                if (statusList.Count > 0)
                {
                    foreach (ListBoxItem item in statusList)
                    {
                        if (item.Content.ToString() != CommonConstants.ALL)
                        {
                            if (inFilterSB_Status.Length > 0)
                            {
                                inFilterSB_Status.Append(",");
                            }

                            if (item.Content.ToString() == CommonConstants.VALID)
                            {
                                inFilterSB_Status.Append("N,0,1,S,6");
                            }
                            else if (item.Content.ToString() == CommonConstants.INVALID)
                            {
                                inFilterSB_Status.Append("4,8,2,C");
                            }
                        }
                    }
                }

                if (inFilterSB_Status.Length > 0)
                {
                    Request.orderStatus = inFilterSB_Status.ToString();
                }
                else
                {
                    Request.orderStatus = "";
                }
            }

            if (filterDic.ContainsKey("IsPoint"))
            {
                IList statusList = filterDic["IsPoint"];
                StringBuilder inFilterSB_Status = new StringBuilder();
                if (statusList.Count > 0)
                {
                    foreach (ListBoxItem item in statusList)
                    {
                        if (item.Content.ToString() != CommonConstants.ALL)
                        {
                            if (inFilterSB_Status.Length > 0)
                            {
                                inFilterSB_Status.Append(",");
                            }

                            if (item.Content.ToString() == CommonConstants.PointOrder)
                            {
                                inFilterSB_Status.Append("0");
                            }
                            else if (item.Content.ToString() == CommonConstants.PriceOrder)
                            {
                                inFilterSB_Status.Append("3");
                            }
                        }
                    }
                }

                if (inFilterSB_Status.Length > 0)
                {
                    Request.timeinforce = inFilterSB_Status.ToString();
                }
                else
                {
                    Request.timeinforce = "";
                }
            }


            Request.symboltype = "";
            lock (OrdersDV)
            {
                for (int drcount = this.OrdersDV.Table.Rows.Count - 1; drcount >= 0; drcount--)
                {
                    DataRow dr = this.OrdersDV.Table.Rows[drcount];
                    dr.BeginEdit();
                    dr.Delete();
                    dr.EndEdit();
                }
                this.OrdersDV.Table.AcceptChanges();

            }
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    historyOrderView.ShowMask(true);
                    Index = 0;
                    RequestOrders();
                    LastOrder = null;
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            });
        }

        AutoResetEvent autoEvent1 = new AutoResetEvent(false);
        /// <summary>
        /// 处理我的订单的过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessEMSOrdersFilterCondition(Dictionary<string, IList> filterDic)
        {
            if (filterDic.ContainsKey("TransactDate"))
            {
                IList dateList = filterDic["TransactDate"];
                if (dateList.Count > 0)
                {
                    DateTime begin = Convert.ToDateTime(dateList[0].ToString());
                    DateTime end = Convert.ToDateTime(dateList[1].ToString());
                    bool status = TimeUtils.Instance.TimeLengthMoreTwoYear(begin, end);
                    if (status)
                    {
                        showMessage("界面不支持2年以上订单筛选，请修改日期区间");
                        return;
                    }
                    EMSRequest.dealTimeBegin = dateList[0].ToString();
                    EMSRequest.dealTimeEnd = dateList[1].ToString();
                }
                else
                {
                    EMSRequest.dealTimeBegin = day;
                    EMSRequest.dealTimeEnd = today;
                }
            }
            // 人工，IDB， 量化
            if (filterDic.ContainsKey("Venues"))
            {
                IList venuesList = filterDic["Venues"];
                StringBuilder inFilterSB_venues = new StringBuilder();
                if (venuesList.Count > 0)
                {
                    foreach (ListBoxItem item in venuesList)
                    {
                        if (inFilterSB_venues.Length > 0)
                        {
                            inFilterSB_venues.Append(",");
                        }

                        if (item.Content.ToString() == CommonConstants.MANUAL)
                        {
                            inFilterSB_venues.Append("MANUAL");
                        }
                        else if (item.Content.ToString() == CommonConstants.IDB)
                        {
                            inFilterSB_venues.Append("IDB");
                        }
                        else if (item.Content.ToString() == CommonConstants.QUANT)
                        {
                            inFilterSB_venues.Append("QUANT");
                        }
                    }
                }

                if (inFilterSB_venues.Length > 0)
                {
                    EMSRequest.strategyordervenues = inFilterSB_venues.ToString();
                }
                else
                {
                    EMSRequest.strategyordervenues = "";
                }
            }

            // firm，nonfirm
            if (filterDic.ContainsKey("FirmFlag"))
            {
                IList list = filterDic["FirmFlag"];
                StringBuilder inFilterSB_FirmFlag = new StringBuilder();
                if (list.Count > 0)
                {
                    foreach (ListBoxItem item in list)
                    {
                        if (inFilterSB_FirmFlag.Length > 0)
                        {
                            inFilterSB_FirmFlag.Append(",");
                        }

                        if (item.Content.ToString() == CommonConstants.FIRM)
                        {
                            inFilterSB_FirmFlag.Append("1");
                        }
                        else if (item.Content.ToString() == CommonConstants.NONFIRM)
                        {
                            inFilterSB_FirmFlag.Append("0");
                        }
                    }
                }

                if (inFilterSB_FirmFlag.Length > 0)
                {
                    EMSRequest.firmflag = inFilterSB_FirmFlag.ToString();
                }
                else
                {
                    EMSRequest.firmflag = "";
                }
            }
            if (filterDic.ContainsKey("OrderStatus"))
            {
                IList statusList = filterDic["OrderStatus"];
                StringBuilder inFilterSB_Status = new StringBuilder();
                if (statusList.Count > 0)
                {
                    foreach (ListBoxItem item in statusList)
                    {
                        if (item.Content.ToString() != CommonConstants.ALL)
                        {
                            if (inFilterSB_Status.Length > 0)
                            {
                                inFilterSB_Status.Append(",");
                            }

                            if (item.Content.ToString() == CommonConstants.VALID)
                            {
                                inFilterSB_Status.Append("N,0,1,S,6");
                            }
                            else if (item.Content.ToString() == CommonConstants.INVALID)
                            {
                                inFilterSB_Status.Append("4,8,2,C");
                            }
                        }
                    }
                }

                if (inFilterSB_Status.Length > 0)
                {
                    EMSRequest.orderStatus = inFilterSB_Status.ToString();
                }
                else
                {
                    EMSRequest.orderStatus = "";
                }
            }

            if (filterDic.ContainsKey("IsPoint"))
            {
                IList statusList = filterDic["IsPoint"];
                StringBuilder inFilterSB_Status = new StringBuilder();
                if (statusList.Count > 0)
                {
                    foreach (ListBoxItem item in statusList)
                    {
                        if (item.Content.ToString() != CommonConstants.ALL)
                        {
                            if (inFilterSB_Status.Length > 0)
                            {
                                inFilterSB_Status.Append(",");
                            }

                            if (item.Content.ToString() == CommonConstants.PointOrder)
                            {
                                inFilterSB_Status.Append("0");
                            }
                            else if (item.Content.ToString() == CommonConstants.PriceOrder)
                            {
                                inFilterSB_Status.Append("3");
                            }
                        }
                    }
                }

                if (inFilterSB_Status.Length > 0)
                {
                    EMSRequest.timeinforce = inFilterSB_Status.ToString();
                }
                else
                {
                    EMSRequest.timeinforce = "";
                }
            }

            EMSRequest.symboltype = "";

            try
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(clearEMSData), autoEvent1);

                ThreadPool.QueueUserWorkItem(new WaitCallback(RequestEMSData), autoEvent1);
                
                //System.Threading.ThreadPool.QueueUserWorkItem(o =>
                //{
                //    EMSIndex = 0;
                //    historyOrderView.ShowEMSMask(true);
                //    RequestEMSOrders();
                //});
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void RequestEMSData(object state)
        {
            autoEvent1.WaitOne();
            EMSIndex = 0;
            RequestEMSOrders();
        }

        private void clearEMSData(object state)
        {
            historyOrderView.ShowEMSMask(true);
            clearEMS();
            autoEvent1.Set();
        }

        private string clickNum = "0";
        public void selectEMSOrderBySTCOrderID(bool isDouble)
        {
            try
            {
                if (SelectedItem_Orders != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (LastOrder != null && (LastOrderid) != SelectedItem_Orders[KEY_ORDER_FIELD] as string)
                        {
                            DataRow dr = this.OrdersDV.Table.Rows.Find(LastOrderid);
                            if (dr != null)
                            {
                                dr[IS_EXPANDED_FIELD] = false;
                            }
                            SelectedItem_Orders[IS_EXPANDED_FIELD] = true;
                            EMSRequest.strategyorderid = SelectedItem_Orders[KEY_ORDER_FIELD] as string;
                            SelectSTCCode = EMSRequest.strategyorderid;
                            Console.WriteLine("SelectedItem_Orders true" + EMSRequest.strategyorderid);

                            LastOrder = SelectedItem_Orders;
                        }
                        else
                        {
                            if (isDouble)
                            {
                                if (LastOrder != null && !"".Equals(LastOrderid))
                                {
                                    DataRow dr = this.OrdersDV.Table.Rows.Find(LastOrderid);
                                    //if (dr != null)
                                    //{
                                    //    dr[IS_EXPANDED_FIELD] = false;
                                    //}
                                    if (dr != null)
                                    {
                                        string statu = dr[IS_EXPANDED_FIELD].ToString();
                                        if (statu != null && (statu).Equals("True"))
                                        {
                                            dr[IS_EXPANDED_FIELD] = false;

                                            EMSRequest.strategyorderid = "";
                                            SelectSTCCode = EMSRequest.strategyorderid;
                                            LastOrderid = "";
                                            lastOrder = null;
                                            Console.WriteLine("LastOrder false" + EMSRequest.strategyorderid);
                                        }
                                        else
                                        {
                                            dr[IS_EXPANDED_FIELD] = true;

                                            EMSRequest.strategyorderid = SelectedItem_Orders[KEY_ORDER_FIELD] as string;
                                            SelectSTCCode = EMSRequest.strategyorderid;

                                            Console.WriteLine("LastOrder true" + EMSRequest.strategyorderid);

                                            LastOrder = SelectedItem_Orders;
                                        }
                                    }
                                }
                                else
                                {
                                    string statu = SelectedItem_Orders[IS_EXPANDED_FIELD].ToString();
                                    if (statu != null && (statu).Equals("True"))
                                    {
                                        SelectedItem_Orders[IS_EXPANDED_FIELD] = false;
                                        EMSRequest.strategyorderid = "";
                                        SelectSTCCode = EMSRequest.strategyorderid;
                                        LastOrderid = "";
                                        lastOrder = null;
                                        Console.WriteLine("SelectedItem_Orders false" + EMSRequest.strategyorderid);
                                    }
                                    else
                                    {
                                        SelectedItem_Orders[IS_EXPANDED_FIELD] = true;
                                        EMSRequest.strategyorderid = SelectedItem_Orders[KEY_ORDER_FIELD] as string;
                                        SelectSTCCode = EMSRequest.strategyorderid;
                                        Console.WriteLine("SelectedItem_Orders true" + EMSRequest.strategyorderid);

                                        LastOrder = SelectedItem_Orders;
                                    }
                                }
                            }
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message)
        {
            MessageBoxGUI22 me = new MessageBoxGUI22();
            me.setWarnMsg(message);
            me.ShowDialog();
        }
        #region 导出母订单
        private DataTable ExportDV;
        private string fileName = "";
        AutoResetEvent autoEvent = new AutoResetEvent(false);
        private void OnExportClick()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_历史母订单记录";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            if ((bool)(saveFileDialog.ShowDialog()))
            {
                fileName = saveFileDialog.FileName;
                ExportDV = OrdersDV.Table.Clone();
                ThreadPool.QueueUserWorkItem(new WaitCallback(RequestExportMethod), autoEvent);
            }
        }
        private void RequestExportMethod(object state)
        {
            lock (ExportDV.Rows.SyncRoot)
            {
                RequestExportOrders();
            }
            autoEvent.Set();
        }
        public void RequestExportOrders()
        {
            StreamWriter sw = null;
            try
            {
                sw = ExcelHelper.Instance.createStream(fileName);
                if (sw != null)
                {
                    LogHelper.Info("fileName:" + fileName);
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    //request.orderStatus = "N,0,1";
                    QueryOrderReq req = new QueryOrderReq();
                    req.userName = LocalEnv.UserInfo.UserName;
                    req.pageSize = 1;
                    req.pageNum = 1;
                    req.dealTimeBegin = Request.dealTimeBegin;
                    req.dealTimeEnd = Request.dealTimeEnd;
                    req.orderid = Request.orderid;
                    req.securityid = Request.securityid;
                    req.symbol = Request.symbol;
                    req.orderStatus = Request.orderStatus;
                    req.firmflag = Request.firmflag;
                    req.venues = Request.venues;
                    req.symboltype = Request.symboltype;
                    req.timeinforce = Request.timeinforce;
                    ///获取订单 
                    var reply = TBookHandler.Instance.QueryOrderForTBook(req);

                    if (reply != null && reply.datas != null)
                    {
                        int maxCount = reply.totalSize;
                        int pageNum = 0;
                        if (maxCount > 0)
                        {
                            pageNum = maxCount % CommonConstants.pageCount == 0 ? (maxCount / CommonConstants.pageCount) : (maxCount / CommonConstants.pageCount + 1);
                        }
                        else
                            pageNum = 0;
                        int count = 0;
                        bool status = true;
                        for (int i = 0; i < pageNum; i++)
                        {
                            req.pageNum = i;
                            req.pageSize = CommonConstants.pageCount;
                            var reply1 = TBookHandler.Instance.QueryOrderForTBook(req);
                            if (reply1 != null && reply1.datas != null && reply1.datas.Count > 0)
                            {
                                foreach (ExecutionReport obj in reply1.datas)
                                {
                                    lock (string.Intern(obj.OrderID))
                                    {
                                        //UpdateExportOrders(obj);                                    
                                        ExportPo po = toExportOrder(obj);
                                        status = ExcelHelper.Instance.SaveDataToCSVFile(po, fileName, sw, count, CommonConstants.HISOrders);
                                        count++;
                                    }
                                }
                            }
                        }
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (count == maxCount)
                            {
                                showMessage("导出成功");
                            }
                            else
                            {
                                showMessage("导出失败");
                            }
                        }));
                    }
                    if (sw != null)
                    {
                        sw.Dispose();
                    }
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        showMessage("导出失败");
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestExportOrders", e);
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    showMessage("导出失败");
                }));
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
            }
        }
        private ExportPo toExportOrder(ExecutionReport obj)
        {
            ExportPo po = new ExportPo();
            po.STCCode = obj.OrderID;
            po.Qty = obj.OrderQty;
            po.Side = ComboSourceUtil.Instance.FindEnum(obj.Side, "side");
            po.SecurityID = obj.SecurityID;
            po.Symbol = obj.Symbol;
            po.SettlType = ComboSourceUtil.Instance.FindEnum(obj.SettlType, "settletype_myorder");
            po.FirmFlag = ComboSourceUtil.Instance.FindEnum(obj.FirmFlag, "firmflag");
            //po.TimeInForce = ComboSourceUtil.Instance.FindEnum(obj.TimeInForce, "timeinforce");
            if (obj.TimeInForce == "0")
            {
                po.TimeInForce = "限价LMT单";
            }
            else if (obj.TimeInForce == "3")
            {
                if (string.IsNullOrEmpty(obj.IocTimeOut))
                {
                    po.TimeInForce = "点价IOC单";
                }
                else
                {
                    po.TimeInForce = "点价FAK单";
                }
            }
            po.OrderStatus = ComboSourceUtil.Instance.FindEnum(obj.ExecType, "ordstatus"); 
            po.Price = string.Format("{0:N4}", decimal.Parse(obj.Price));
            po.NetPrice = CommonUtils.ConvertToDecimal(obj.NetPrice) == 0 ? "--" : string.Format("{0:N4}", CommonUtils.ConvertToDecimal(obj.NetPrice));// string.Format("{0:N4}", decimal.Parse(obj.NetPrice));
            po.ExecQty = CommonUtils.ConvertToLong(obj.CumQty) == 0 ? "" : CommonUtils.ConvertToLong(obj.CumQty).ToString();
            po.MyDealer = obj.PartySubName;
            po.Veneues = ComboSourceUtil.Instance.FindEnum(obj.Venues, "venuesorder");
            po.CrossVenues = ComboSourceUtil.Instance.FindEnum(obj.CrossVenues, "crossvenues");
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
            po.VenuesSelectDisplay = venuesSelectDisplay;
            po.UpdateTime = TimeUtils.Instance.ToTime(obj.UpdateTime);
            po.UpdateDate = TimeUtils.Instance.ToDate(obj.UpdateTime);
            po.Createdate = TimeUtils.Instance.ToDate(obj.TransactTime);
            po.IsPoint = ComboSourceUtil.Instance.FindEnum(obj.TimeInForce == "0" ? "0" : "1", "IsPoint");
            return po;
        }
        #endregion
        #region 导出子订单

        private DataTable EMSExportDV;
        private string EMSfileName = "";
        AutoResetEvent EMSautoEvent = new AutoResetEvent(false);
        private void EMSOnExportClick()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_历史子订单记录";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            if ((bool)(saveFileDialog.ShowDialog()))
            {
                EMSfileName = saveFileDialog.FileName;
                EMSExportDV = EMSOrdersDV.Table.Clone();
                ThreadPool.QueueUserWorkItem(new WaitCallback(RequestEMSExportMethod), EMSautoEvent);
            }
        }

        private void RequestEMSExportMethod(object state)
        {
            lock (EMSExportDV.Rows.SyncRoot)
            {
                RequestEMSExportOrders();
            }
            EMSautoEvent.Set();
        }

        private void RequestEMSExportOrders()
        {
            StreamWriter sw = null;
            try
            {
                sw = ExcelHelper.Instance.createStream(EMSfileName);
                if (sw != null)
                {
                    LogHelper.Info("EMSfileName:" + EMSfileName);
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    //request.orderStatus = "N,0,1";
                    QueryOrderReq req = new QueryOrderReq();
                    req.userName = LocalEnv.UserInfo.UserName;
                    req.pageSize = 1;
                    req.pageNum = 1;
                    req.dealTimeBegin = EMSRequest.dealTimeBegin;
                    req.dealTimeEnd = EMSRequest.dealTimeEnd;
                    req.strategyorderid = EMSRequest.strategyorderid;
                    req.securityid = EMSRequest.securityid;
                    req.symbol = EMSRequest.symbol;
                    req.orderStatus = EMSRequest.orderStatus;
                    req.firmflag = EMSRequest.firmflag;
                    req.strategyordervenues = EMSRequest.strategyordervenues;
                    req.symboltype = EMSRequest.symboltype;
                    req.timeinforce = EMSRequest.timeinforce;
                    ///获取订单 
                    var reply = MarketExecHandler.Instance.QueryOrderForEMS(req);

                    if (reply != null && reply.datas != null)
                    {
                        int maxCount = reply.totalSize;
                        int pageNum = 0;
                        if (maxCount > 0)
                        {
                            pageNum = maxCount % CommonConstants.pageCount == 0 ? (maxCount / CommonConstants.pageCount) : (maxCount / CommonConstants.pageCount + 1);
                        }
                        else
                            pageNum = 0;
                        int count = 0;
                        bool status = true;
                        for (int i = 0; i < pageNum; i++)
                        {
                            req.pageNum = i;
                            req.pageSize = CommonConstants.pageCount;
                            var reply1 = MarketExecHandler.Instance.QueryOrderForEMS(req);
                            if (reply1 != null && reply1.datas != null && reply1.datas.Count > 0)
                            {
                                foreach (ExecutionReport obj in reply1.datas)
                                {
                                    lock (string.Intern(obj.OrderID))
                                    {
                                        //UpdateExportOrders(obj);                                    
                                        ExportPo po = toEMSExportOrder(obj);
                                        status = ExcelHelper.Instance.SaveDataToCSVFile(po, fileName, sw, count, CommonConstants.HISEMSOrders);
                                        count++;
                                    }
                                }
                            }
                        }
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (count == maxCount)
                            {
                                showMessage("导出成功");
                            }
                            else
                            {
                                showMessage("导出失败");
                            }
                        }));
                    }
                    if (sw != null)
                    {
                        sw.Dispose();
                    }
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        showMessage("导出失败");
                    }));
                }
            }
            catch (Exception e)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    showMessage("导出失败");
                }));
                LogHelper.Error("RequestEMSExportOrders", e);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
            }
        }
        private ExportPo toEMSExportOrder(ExecutionReport obj)
        {
            ExportPo po = new ExportPo();
            po.STCCode = obj.ClOrdID;
            po.StrategyOrderId = obj.StrategyOrderId;
            po.Qty = obj.OrderQty;
            po.Side = ComboSourceUtil.Instance.FindEnum(obj.Side, "side");
            po.SecurityID = obj.SecurityID;
            po.Symbol = obj.Symbol;
            po.SettlType = ComboSourceUtil.Instance.FindEnum(obj.SettlType, "settletype_myorder");
            po.FirmFlag = ComboSourceUtil.Instance.FindEnum(obj.FirmFlag, "firmflag");
            //if ("0".Equals(obj.TimeInForce) && "1".Equals(obj.OCOFlag))
            //{
            //    po.TimeInForce = "限价单(oco)";
            //}
            //else
            //{
            //    po.TimeInForce = ComboSourceUtil.Instance.FindEnum(obj.TimeInForce, "timeinforce");
            //}
            if (obj.TimeInForce == "0")
            {
                po.TimeInForce = "限价LMT单";
            }
            else if (obj.TimeInForce == "3")
            {
                if (string.IsNullOrEmpty(obj.IocTimeOut))
                {
                    po.TimeInForce = "点价IOC单";
                }
                else
                {
                    po.TimeInForce = "点价FAK单";
                }
            }
            po.OrderStatus = ComboSourceUtil.Instance.FindEnum(obj.ExecType, "ordstatus");
            po.Price = string.Format("{0:N4}", decimal.Parse(obj.Price));
            po.NetPrice = CommonUtils.ConvertToDecimal(obj.NetPrice) == 0 ? "--" : string.Format("{0:N4}", CommonUtils.ConvertToDecimal(obj.NetPrice)); //string.Format("{0:N4}", decimal.Parse(obj.NetPrice));
            po.ExecQty = CommonUtils.ConvertToLong(obj.CumQty) == 0 ? "" : CommonUtils.ConvertToLong(obj.CumQty).ToString();
            po.MyDealer = obj.PartySubName;
            po.Veneues = ComboSourceUtil.Instance.FindEnum(obj.Venues, "venuesorder");
            po.VenuesSelectDisplay = ComboSourceUtil.Instance.FindEnum(obj.Venues, "venues_exe");
            po.UpdateTime = TimeUtils.Instance.ToTime(obj.UpdateTime);
            po.UpdateDate = TimeUtils.Instance.ToDate(obj.UpdateTime);
            po.Createdate = TimeUtils.Instance.ToDate(obj.TransactTime);
            po.IsPoint = ComboSourceUtil.Instance.FindEnum(obj.TimeInForce == "0" ? "0" : "1", "IsPoint");
            return po;
        }
        #endregion
    }
}
