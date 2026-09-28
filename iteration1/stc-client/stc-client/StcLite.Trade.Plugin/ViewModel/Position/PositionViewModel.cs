using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.Handler.DataProcesser;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using com.gateway.log;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.ViewModel.Adapter;
using BDB.Trade.Plugin.View.Order;
using Newtonsoft.Json;
using BDB.Trade.Plugin.View.Position;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Base;
using STC.Common.View.Component;
using STC.Common.Model.TBook;
using OrderCancelRequest = STC.Common.Model.TBook.OrderCancelRequest;
using STC.Frame.Common;
using STC.Common.Data;
using OrdersHandler = BDB.Trade.Plugin.Handler.OrdersHandler;
using ExecutionsHandler = BDB.Trade.Plugin.Handler.ExecutionsHandler;

namespace BDB.Trade.Plugin.ViewModel
{
    public class PositionViewModel : BaseViewModel
    {
        WindowManager m_windowManager = WindowManager.Instance;//new WindowManager();
        
        #region  构造
        /// <summary>
        /// 0: 显示 1：不显示
        /// </summary>
        private Visibility isHiddenMyOrdersOperationGrid =  Visibility.Collapsed;

        public Visibility IsHiddenMyOrdersOperationGrid
        {
            get
            {
                return isHiddenMyOrdersOperationGrid;
            }

            set
            {
                if (isHiddenMyOrdersOperationGrid != value)
                {
                    isHiddenMyOrdersOperationGrid = value;
                    this.OnPropertyChanged(nameof(IsHiddenMyOrdersOperationGrid));
                }
            }
        }

        private string myOrdersHeight = "262";
        public string MyOrdersHeight
        {
            get
            {
                return myOrdersHeight;
            }

            set
            {
                if (myOrdersHeight != value)
                {
                    myOrdersHeight = value;
                    this.OnPropertyChanged(nameof(MyOrdersHeight));
                }
            }
        }

        private int myOrdersOperationGridHeight = 0;
        public int MyOrdersOperationGridHeight
        {
            get
            {
                return myOrdersOperationGridHeight;
            }

            set
            {
                if (myOrdersOperationGridHeight != value)
                {
                    myOrdersOperationGridHeight = value;
                    this.OnPropertyChanged(nameof(MyOrdersOperationGridHeight));
                }
            }
        }

       
        /// <summary>
        /// 我的订单所有选中个数
        /// </summary>
        private int _checkedCount4MyOrders = 0;
        public int CheckedCount4MyOrders
        {
            get
            {
                return _checkedCount4MyOrders;
            }
            set
            {
                if (_checkedCount4MyOrders != value)
                {
                    _checkedCount4MyOrders = value;
                    this.OnPropertyChanged();
                }
            }
        }

        
        #endregion

        #region  定义 selectedItem

        /// <summary>
        /// 我的订单数据源
        /// </summary>
        private OrderModel _selectedItem_MyOrders;

        /// <summary>
        /// 我的订单选中行记录
        /// </summary>
        public OrderModel SelectedItem_MyOrders
        {
            get
            {
                return _selectedItem_MyOrders;
            }

            set
            {
                _selectedItem_MyOrders = value;
            }
        }

       

        private ExecutionsOrderModel _selectedItem_MyExecutions;

        /// <summary>
        /// 我的成交选中行记录
        /// </summary>
        public ExecutionsOrderModel SelectedItem_MyExecutions
        {
            get
            {
                return _selectedItem_MyExecutions;
            }

            set
            {
                _selectedItem_MyExecutions = value;
            }
        }
        
        #endregion

        #region  other
        private OrderModel _selectedItem_MyCancelOrders;

        /// <summary>
        /// 已撤订单选中行记录
        /// </summary>
        public OrderModel SelectedItem_MyCancelOrders
        {
            get
            {
                return _selectedItem_MyCancelOrders;
            }

            set
            {
                _selectedItem_MyCancelOrders = value;
            }
        }


        // 搜索
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

                    FilterOrderData();
                    FilterCancelOrderData();
                }
            }
        }

        private int selectedPageIndex = 0;
        public int SelectedPageIndex
        {
            get
            {
                return selectedPageIndex;
            }
            set
            {
                selectedPageIndex = value;
                this.OnPropertyChanged(nameof(SelectedPageIndex));
            }
        }

        #region CheckBox
        /// <summary>
        /// 我的需求 搜索条件 - 全部
        /// </summary>
        private bool searchAllCheck = true;

        public bool SearchAllCheck
        {
            get
            {
                return searchAllCheck;
            }

            set
            {
                if (!(SearchRequireCheck ^ SearchOrderCheck))
                {
                    return;
                }

                searchAllCheck = value;
                if (searchAllCheck)
                {
                    SearchRequireCheck = SearchOrderCheck = false;
                    FilterOrderData();
                }

                this.OnPropertyChanged(nameof(SearchAllCheck));
            }
        }

        /// <summary>
        /// 我的需求  搜索条件 - 需求
        /// </summary>
        private bool searchRequireCheck = false;
        public bool SearchRequireCheck
        {
            get
            {
                return searchRequireCheck;
            }

            set
            {
                if (!(SearchAllCheck ^ SearchOrderCheck))
                {
                    return;
                }
                searchRequireCheck = value;
                if (searchRequireCheck)
                {
                    SearchAllCheck = SearchOrderCheck = false;
                    FilterOrderData();
                }
                this.OnPropertyChanged(nameof(SearchRequireCheck));

            }
        }
        /// <summary>
        /// 我的需求  搜索条件 - 意向
        /// </summary>
        private bool searchOrderCheck = false;
        public bool SearchOrderCheck
        {
            get
            {
                return searchOrderCheck;
            }

            set
            {
                if (!(SearchAllCheck ^ SearchRequireCheck))
                {
                    return;
                }

                searchOrderCheck = value;
                if (searchOrderCheck)
                {
                    SearchAllCheck = SearchRequireCheck = false;
                    FilterOrderData();
                }

                this.OnPropertyChanged(nameof(SearchOrderCheck));

            }
        }


        /// <summary>
        /// 已撤 搜索条件 - 全部
        /// </summary>
        private bool searchCancelAllCheck = true;

        public bool SearchCancelAllCheck
        {
            get
            {
                return searchCancelAllCheck;
            }

            set
            {
                if (!(SearchCancelRequireCheck ^ SearchCancelOrderCheck))
                {
                    return;
                }

                searchCancelAllCheck = value;
                if (searchCancelAllCheck)
                {
                    SearchCancelRequireCheck = SearchCancelOrderCheck = false;
                    FilterCancelOrderData();
                }

                this.OnPropertyChanged(nameof(SearchCancelAllCheck));
            }
        }

        /// <summary>
        /// 已撤  搜索条件 - 需求
        /// </summary>
        private bool searchCancelRequireCheck = false;
        public bool SearchCancelRequireCheck
        {
            get
            {
                return searchCancelRequireCheck;
            }

            set
            {
                if (!(SearchCancelAllCheck ^ SearchCancelOrderCheck))
                {
                    return;
                }
                searchCancelRequireCheck = value;
                if (searchCancelRequireCheck)
                {
                    SearchCancelAllCheck = SearchCancelOrderCheck = false;
                    FilterCancelOrderData();
                }
                this.OnPropertyChanged(nameof(SearchCancelRequireCheck));

            }
        }
        /// <summary>
        /// 已撤  搜索条件 - 意向
        /// </summary>
        private bool searchCancelOrderCheck = false;
        public bool SearchCancelOrderCheck
        {
            get
            {
                return searchCancelOrderCheck;
            }

            set
            {
                if (!(SearchCancelAllCheck ^ SearchCancelRequireCheck))
                {
                    return;
                }

                searchCancelOrderCheck = value;
                if (searchCancelOrderCheck)
                {
                    SearchCancelAllCheck = SearchCancelRequireCheck = false;
                    FilterCancelOrderData();
                }

                this.OnPropertyChanged(nameof(SearchCancelOrderCheck));
            }
        }
        #endregion

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

        public ICommand DelOrderCommand { get; set; }

        public ICommand CancelSelectedOrdersCommand { get; set; }

        public ICommand UncheckAllCommand { get; set; }

        public ICommand EditOrderCommand { get; set; }

        public ICommand ResumeCommand { get; set; }

        private bool _isEnabled_Resend = true;
        public bool IsEnabled_Resend
        {
            get
            {
                return _isEnabled_Resend;
            }
            set
            {
                if (_isEnabled_Resend != value)
                {
                    _isEnabled_Resend = value;
                    this.OnPropertyChanged();
                }
            }
        }
        #endregion

        #region 数据绑定修改为 ObservableCollection
        /// <summary>
        /// Multi Thread Lock
        /// </summary>
        private object objlock = new object();
        
        private ObservableCollection<OrderModel> _DictAllMyReqOrders = new ObservableCollection<OrderModel>();
        private ObservableCollection<ExecutionsOrderModel> _DictAllMyExecutions = new ObservableCollection<ExecutionsOrderModel>();
        private ObservableCollection<OrderModel> _DictAllMyCancelOrders = new ObservableCollection<OrderModel>();

        public ObservableCollection<OrderModel> DictAllMyReqOrders
        {
            get { return _DictAllMyReqOrders; }
            set
            {
                _DictAllMyReqOrders = value;
                this.OnPropertyChanged(nameof(DictAllMyReqOrders));
            }
        }
        public ObservableCollection<ExecutionsOrderModel> DictAllMyExecutions
        {
            get { return _DictAllMyExecutions; }
            set
            {
                _DictAllMyExecutions = value;
                this.OnPropertyChanged(nameof(DictAllMyExecutions));
            }
        }
        public ObservableCollection<OrderModel> DictAllMyCancelOrders
        {
            get { return _DictAllMyCancelOrders; }
            set
            {
                _DictAllMyCancelOrders = value;
                this.OnPropertyChanged(nameof(DictAllMyCancelOrders));
            }
        }

        /// <summary>
        /// 我的需求
        /// </summary>
        private ObservableCollection<OrderModel> m_ObsReqMyOrders = new ObservableCollection<OrderModel>();
        public ObservableCollection<OrderModel> ObsMyReqOrders
        {
            get { return m_ObsReqMyOrders; }
            set
            {
                m_ObsReqMyOrders = value;
                this.OnPropertyChanged(nameof(ObsMyReqOrders));
            }
        }

        /// <summary>
        /// 已确认
        /// </summary>
        private ObservableCollection<ExecutionsOrderModel> m_ObsMyExecutions = new ObservableCollection<ExecutionsOrderModel>();
        public ObservableCollection<ExecutionsOrderModel> ObsMyExecutions
        {
            get { return m_ObsMyExecutions; }
            set
            {
                m_ObsMyExecutions = value;
                this.OnPropertyChanged(nameof(ObsMyExecutions));
            }
        }

        /// <summary>
        /// 已撤订单
        /// </summary>
        private ObservableCollection<OrderModel> m_ObsMyCancelOrders = new ObservableCollection<OrderModel>();
        public ObservableCollection<OrderModel> ObsMyCancelOrders
        {
            get { return m_ObsMyCancelOrders; }
            set
            {
                m_ObsMyCancelOrders = value;
                this.OnPropertyChanged(nameof(ObsMyCancelOrders));
            }
        }
        #endregion

        //Modi:cyc Date:2021-04-12 Func:增加倒计时
        DispatcherTimer descTimer = new DispatcherTimer();

        private AsyncThreadQueue<ExecutionReport> _ProcessMyExecutionThreadQueue;

        private object objOrderLock = new object();
        private object objExecutionLock = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        public PositionViewModel()
        {
            try
            {
                DelOrderCommand = new RelayCommand(DeleteOrder);
                
                CancelSelectedOrdersCommand = new RelayCommand(CancelSelectedOrders);

                UncheckAllCommand = new RelayCommand(UncheckAll);

                EditOrderCommand = new RelayCommand(EditOrder);

                ResumeCommand = new RelayCommand(ResumeOrder);

                #region 收TBook消息
                //---------------收TBook消息--------------
                // 我的订单
                OrdersHandler.Instance.OnOwnOrders += OrderProcesser_OnUpdate;
                //---------------------------------------
                #endregion
                //---------------收EMSSvr消息--------------
                //我的成交
                ExecutionsHandler.Instance.OnOwnExecutions += ExecutionProcesser_OnUpdate;
                //---------------------------------------

                StartUpdateThread();
                // 订阅QQ消息
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    Common.Handler.Base.BaseHandler.Instance.Init(Instance_OnClear);
                //}), System.Windows.Threading.DispatcherPriority.Background);

                this.descTimer = new DispatcherTimer();
                this.descTimer.Interval = TimeSpan.FromSeconds(1);
                this.descTimer.Tick += DescTimer_Tick;
                this.descTimer.Start();
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
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

        #region 清零信号
        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info($"PostionViewModel Received Clear Message：{obj.IsClear}");

            return;

            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        lock (objlock)
                        {
                            m_toProcData.Clear();
                            ObsMyExecutions = new ObservableCollection<ExecutionsOrderModel>();
                            ObsMyReqOrders = new ObservableCollection<OrderModel>();
                            ObsMyCancelOrders = new ObservableCollection<OrderModel>();
                            DictAllMyCancelOrders = new ObservableCollection<OrderModel>();
                            DictAllMyExecutions = new ObservableCollection<ExecutionsOrderModel>();
                            DictAllMyReqOrders = new ObservableCollection<OrderModel>();

                            OrdersHandler.Instance.RequestOrders();
                            LogHelper.Info($"PostionViewModel RequestOrders ...");
                            ExecutionsHandler.Instance.RequestOwnExecutions();
                            LogHelper.Info($"PostionViewModel RequestOwnExecutions ...");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"PostionViewModel OnClear Error:{ex.Message}");
                    }
                }));
            }
        }
        #endregion


        private Thread updateOrderDataThread;
        private Queue<object> m_toProcData = new Queue<object>();
        private static ManualResetEvent myResetEvent = new ManualResetEvent(false);
        private void StartUpdateThread()
        {
            updateOrderDataThread = new Thread(threadUpdateData);
            updateOrderDataThread.IsBackground = true;
            updateOrderDataThread.Priority = ThreadPriority.AboveNormal;
            updateOrderDataThread.Start();
        }

        private void threadUpdateData()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        while (this.m_toProcData.Count > 0)
                        {
                            object obj;
                            lock (objlock)
                            {
                               obj =  this.m_toProcData.Dequeue();
                            }

                            // 已确认
                            if ((obj as ExecutionReport).Info7 == "Execution")
                            {
                                Proc_ExecutionProcesser(obj);
                            }

                            // 我的需求  // 已撤订单 
                            if ((obj as ExecutionReport).Info7 == "Order")
                            {
                                Proc_OrderProcesser(obj);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }

                    myResetEvent.WaitOne();
                }
            }
            catch (ThreadAbortException e)
            {
                Thread.ResetAbort();
            }
            catch (System.OutOfMemoryException ex1)
            {
                LogHelper.Error(ex1);
            }
            catch (System.Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
        private void ExecutionProcesser_OnUpdate(object obj)
        {
            if ((obj as ExecutionReport).UserName == LocalEnv.UserInfo.UserName)
            {
                lock (objlock)
                {
                    (obj as ExecutionReport).Info7 = "Execution";
                    m_toProcData.Enqueue(obj);
                    myResetEvent.Set();
                }
            }
        }

        private void OrderProcesser_OnUpdate(object obj)
        {
            lock (objlock)
            {
                (obj as ExecutionReport).Info7 = "Order";
                m_toProcData.Enqueue(obj);
                myResetEvent.Set();
            }
        }

        /// <summary>
        ///  已成交订单
        /// </summary>
        /// <param name="obj"></param>
        private void Proc_ExecutionProcesser(object obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (objExecutionLock)
                {
                    ExecutionReport rs = (ExecutionReport)obj;
                    string s = JsonConvert.SerializeObject(obj);
                    // LogHelper.Error("ExecutionProcesser_OnUpdate:" + s);
                    ExecutionsOrderModel ls = JsonConvert.DeserializeObject<ExecutionsOrderModel>(s);
                    ls.LastPx_Display = CommonUtils.FormatPrice(rs.Yield);// 用于显示的成交价
                    ls.LastPx = CommonUtils.ConvertToDecimal(rs.Yield);// 成交价；
                    ls.SyncStatus = rs.SynchFlag; // 同步状态
                    ls.ExecutionTime = rs.TransactTime;// 成交时间
                    ls.StrategyOrderID = rs.StrategyOrderId;//我的订单号 
                    ls.RefOrderID = rs.CounterPartyOrdID;//对手订单号

                    decimal dirtyPrice = CommonUtils.ConvertToDecimal(rs.DirtyPrice);
                    ls.DirtyPrice = dirtyPrice;// 全价
                    ls.CumQty = CommonUtils.ConvertToLong(rs.CumQty);//总成交量
                    long lastQty = CommonUtils.ConvertToLong(rs.LastQty);
                    ls.LastQty = lastQty; // 成交量
                    ls.DeliveryAmount = dirtyPrice * lastQty;//结算金额,结算金额=全价 *成交量
                    ls.TradeDate = rs.TradeDate;

                    var m = DictAllMyExecutions.FirstOrDefault(o => o.OrderID == rs.OrderID);
                    if (m != null)
                    {
                        m = ls;
                    }
                    else
                    {
                        DictAllMyExecutions.Insert(0, ls);
                    }

                    var m2 = DictAllMyReqOrders.FirstOrDefault(o => o.OrderID == rs.OrderID);
                    if (m2 != null)
                    {
                        DictAllMyReqOrders.Remove(m2);
                        // ObsMyReqOrders = new ObservableCollection<OrderModel>(DictAllMyReqOrders.OrderByDescending(o => o.UpdateTime));
                        FilterOrderData();
                    }

                    FilterExecutionOrder();
                    // ObsMyExecutions = new ObservableCollection<ExecutionsOrderModel>(DictAllMyExecutions.OrderByDescending(o => o.ExecutionTime));
                }
            }));
        }

        // 我的订单  DictMyOrders  // 已撤订单   DictMyCancelOrders  //已确认 DictMyExecutions
        private void Proc_OrderProcesser(object obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (objOrderLock)
                {
                    ExecutionReport rs = (ExecutionReport)obj;
                    string s = JsonConvert.SerializeObject(rs);
                    // LogHelper.Error("OrderProcesser_OnUpdate:" + s);
                    OrderModel ls = JsonConvert.DeserializeObject<OrderModel>(s);

                    ls.IsPoint = rs.TimeInForce == "0" ? "0" : "1";  //是否是点价意向
                    ls.MyDealer = rs.UserName;  // 本方交易员
                    ls.OrdType = rs.AlgoName;   // 报价类型, 目前仅有此一种类别X-Bond做市，三阶段将增加：Tradebook，IDB，双边做市
                    ls.STCCode = rs.OrderID;    // BDB单号
                    ls.NonfirmTime = rs.FirmToNonTime == null ? string.Empty : rs.FirmToNonTime;  // nofirmtime
                    ls.RemainingTime = ls.NonfirmTime;
                    ls.Qty = rs.OrderQty;
                    ls.SettleType = rs.SettlType;
                    ls.ExecType = rs.ExecType;
                    ls.AlgoName = rs.AlgoName;
                    ls.Venues = rs.Venues;
                    ls.ExecQty = rs.CumQty;
                    ls.OrderStatus = rs.OrdStatus;

                    //if (string.IsNullOrEmpty(rs.Symbol))
                    //{
                    //    BondsInfoPo symn = CommonUtils.GetBondsInfoPoBySecurityID(rs.SecurityID);
                    //    if (symn !=null)
                    //    {
                    //        rs.Symbol = symn.Short_Name;
                    //        ls.Symbol = rs.Symbol;
                    //    }
                    //}

                    if (ls.IsPoint == "0")
                        ls.EnabledBtn = (ls.IsPoint == "0") && (rs.ExecType == "6") ? false : true;
                    else
                        ls.EnabledBtn = false;

                    if ((rs.ExecType == "0" || rs.ExecType == "1" || rs.ExecType == "N" || rs.ExecType == "6") && rs.AlgoName != "XBOND" && rs.Venues != "QUANT" && string.IsNullOrEmpty(rs.IocTimeOut))
                    {
                        ls.ShowDeleteBtn = true;
                    }
                    else
                    {
                        ls.ShowDeleteBtn = false;
                    }

                    // 判断订单类别  0 我的需求  1 已确认 -1 已撤销 
                    //N-新增, 0-正常,4-已撤销,8-已拒绝,1-部分成交,2-完全成交,S-报价中,6-撤销中,C-异常
                    int OrderType = -1;
                    OrderType = (rs.OrdStatus == "N") || (rs.OrdStatus == "0") || (rs.OrdStatus == "1") || (rs.OrdStatus == "S") || (rs.OrdStatus == "6") ? 0 : -1;
                    if (OrderType == -1)
                    {
                        if (rs.OrdStatus == "2")
                        {
                            OrderType = 1;
                        }
                    }

                    if (OrderType == 0)   // 0 我的需求
                    {
                        var m = DictAllMyReqOrders.FirstOrDefault(o => o.OrderID == rs.OrderID);
                        if (ls.IsPoint == "0")
                            ls.EnabledBtn = (rs.ExecType == "6") ? false : true;
                        if (m != null)
                        {
                            DictAllMyReqOrders.Remove(m);
                        }

                        DictAllMyReqOrders.Insert(0, ls);
                        FilterOrderData();
                    }
                    else if (OrderType == 1)    //1 已确认
                    {
                        var m = DictAllMyReqOrders.FirstOrDefault(o => o.OrderID == rs.OrderID);
                        if (ls.IsPoint == "0")
                            ls.EnabledBtn = (rs.ExecType == "6") ? false : true;
                        if (m != null)
                        {
                            DictAllMyReqOrders.Remove(m);
                        }

                        FilterOrderData();
                    }
                    else if (OrderType == -1)  // -1 已撤销
                    {
                        var m = DictAllMyCancelOrders.FirstOrDefault(o => o.OrderID == rs.OrderID);
                        if (m != null)
                        {
                            DictAllMyCancelOrders.Remove(m);
                        }

                        DictAllMyCancelOrders.Insert(0, ls);
                        FilterCancelOrderData();

                        //N-新增, 0-正常,4-已撤销,8-已拒绝,1-部分成交,2-完全成交,S-报价中,6-撤销中,C-异常
                        if ((rs.OrdStatus == "4") || (rs.OrdStatus == "2") || (rs.OrdStatus == "8") || (rs.OrdStatus == "C"))
                        {
                            var m2 = DictAllMyReqOrders.FirstOrDefault(o => o.OrderID == rs.OrderID);
                            if (m2 != null)
                            {
                                DictAllMyReqOrders.Remove(m2);
                                FilterOrderData();
                            }
                        }
                    }
                }
            }));
        }

        #region Dispose
        ~PositionViewModel()
        {
            updateOrderDataThread.Abort();
            this.Dispose();
        }

        new public void Dispose()
        {
            base.Dispose();
        }

        #endregion

        #region 已确认
        private string filterSB = "'CNEX','TPSC','CTTJ','PTCN','CCTB'";
        public string FilterSB
        {
            get
            {
                return filterSB;
            }

            set
            {
                if (filterSB != value)
                {
                    filterSB = value;
                    FilterExecutionOrder();
                    this.OnPropertyChanged(nameof(FilterSB));
                }
            }
        }

        /// <summary>
        /// 处理我的成交的过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessMyExecutionsFilterConditon(Dictionary<string, IList> filterDic)
        {
            // venues
            IList venuesList = filterDic["Broker"];
            StringBuilder inFilterSB_venues = new StringBuilder();
            if (venuesList.Count > 0)
            {
                foreach (ListBoxItem item in venuesList)
                {
                    if (item.Name == "All")
                    {
                        inFilterSB_venues.Append("'CNEX','TPSC','CTTJ','PTCN','CCTB'");
                        break;
                    }

                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }
                    else if (item.Content.ToString() == CommonConstants.CNEX)
                    {
                        inFilterSB_venues.Append("'CNEX'");
                    }
                    else if (item.Content.ToString() == CommonConstants.TPSC)
                    {
                        inFilterSB_venues.Append("'TPSC'");
                    }
                    else if (item.Content.ToString() == CommonConstants.CTTJ)
                    {
                        inFilterSB_venues.Append("'CTTJ'");
                    }
                    else if (item.Content.ToString() == CommonConstants.PTCN)
                    {
                        inFilterSB_venues.Append("'PTCN'");
                    }
                    else if (item.Content.ToString() == CommonConstants.CCTB)
                    {
                        inFilterSB_venues.Append("'CCTB'");
                    }
                }
            }
            
            FilterSB = inFilterSB_venues.ToString();
        }

        private void FilterExecutionOrder()
        {
            // ObservableCollection<Model> collection = new ObservableCollection<Model>();
            // ObservableCollection<Model> filteredCollection = new ObservableCollection<Model>(from item in collection where item.ID > 0 && item.ID < 10 orderby item.ID select item);

            // ObsMyExecutions = new ObservableCollection<ExecutionsOrderModel>(DictAllMyExecutions.Where(p => p.Venues.Contains(filter_executions.ToString())));


            //ICollectionView cv = (ICollectionView)CollectionViewSource.GetDefaultView(DictAllMyExecutions);
            //cv.Filter = o => {
            //    ExecutionsOrderModel order = o as ExecutionsOrderModel;
            //    return inFilterSB_venues.ToString().Contains(order.Venues);
            //};
            // ObsMyExecutions = new ObservableCollection<ExecutionsOrderModel>(cv.Cast<ExecutionsOrderModel>().ToList());
            ObsMyExecutions = new ObservableCollection<ExecutionsOrderModel>(DictAllMyExecutions.Where(p => FilterSB.Contains(p.Venues)).OrderByDescending(o => o.TradeDate));
        }

        #endregion

        #region 查询过滤
        /// <summary>
        /// 过滤我的需求
        /// </summary>
        private void FilterOrderData()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (objlock)
                {
                    if (SearchAllCheck)
                    {
                        if (string.IsNullOrEmpty(this.SearchText))
                        {
                            this.ObsMyReqOrders = new ObservableCollection<OrderModel>(DictAllMyReqOrders.OrderByDescending(o => o.UpdateTime));
                            // ObsMyReqOrders = new ObservableCollection<OrderModel>(DictAllMyReqOrders.OrderByDescending(o => o.UpdateTime));
                        }
                        else
                        {
                            this.ObsMyReqOrders = new ObservableCollection<OrderModel>(DictAllMyReqOrders.Where(p => p.SecurityID.Contains(this.SearchText)).OrderByDescending(o => o.UpdateTime));
                        }
                    }
                    else if (SearchRequireCheck)
                    {
                        if (string.IsNullOrEmpty(this.SearchText))
                        {
                            this.ObsMyReqOrders = new ObservableCollection<OrderModel>(DictAllMyReqOrders.Where(p => p.IsPoint == "0").OrderByDescending(o => o.UpdateTime));
                        }
                        else
                        {
                            this.ObsMyReqOrders = new ObservableCollection<OrderModel>(DictAllMyReqOrders.Where(p => p.SecurityID.Contains(this.SearchText) && p.IsPoint == "0").OrderByDescending(o => o.UpdateTime));
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(this.SearchText))
                        {
                            this.ObsMyReqOrders = new ObservableCollection<OrderModel>(DictAllMyReqOrders.Where(p => p.IsPoint == "1").OrderByDescending(o => o.UpdateTime));
                        }
                        else
                        {
                            this.ObsMyReqOrders = new ObservableCollection<OrderModel>(DictAllMyReqOrders.Where(p => p.SecurityID.Contains(this.SearchText) && p.IsPoint == "1").OrderByDescending(o => o.UpdateTime));
                        }
                    }
                }
            }));
        }

        /// <summary>
        /// 过滤已撤销订单数据
        /// </summary>
        private void FilterCancelOrderData()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (objlock)
                {
                    if (SearchCancelAllCheck)
                    {
                        if (string.IsNullOrEmpty(this.SearchText))
                        {
                            this.ObsMyCancelOrders = new ObservableCollection<OrderModel>(DictAllMyCancelOrders.OrderByDescending(o => o.UpdateTime));
                        }
                        else
                        {
                            this.ObsMyCancelOrders = new ObservableCollection<OrderModel>(DictAllMyCancelOrders.Where(p => p.SecurityID.Contains(this.SearchText)).OrderByDescending(o => o.UpdateTime));
                        }
                    }
                    else if (SearchCancelRequireCheck)
                    {
                        if (string.IsNullOrEmpty(this.SearchText))
                        {
                            this.ObsMyCancelOrders = new ObservableCollection<OrderModel>(DictAllMyCancelOrders.Where(p => p.IsPoint == "0").OrderByDescending(o => o.UpdateTime));
                        }
                        else
                        {
                            this.ObsMyCancelOrders = new ObservableCollection<OrderModel>(DictAllMyCancelOrders.Where(p => p.SecurityID.Contains(this.SearchText) && p.IsPoint == "0").OrderByDescending(o => o.UpdateTime));
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(this.SearchText))
                        {
                            this.ObsMyCancelOrders = new ObservableCollection<OrderModel>(DictAllMyCancelOrders.Where(p => p.IsPoint == "1").OrderByDescending(o => o.UpdateTime));
                        }
                        else
                        {
                            this.ObsMyCancelOrders = new ObservableCollection<OrderModel>(DictAllMyCancelOrders.Where(p => p.SecurityID.Contains(this.SearchText) && p.IsPoint == "1").OrderByDescending(o => o.UpdateTime));
                        }
                    }
                }
            }));
        }

        #endregion

        #region 删除订单
        public async void DeleteOrder()
        {
            if (this.SelectedItem_MyOrders == null)
            {
                return;
            }

            OrderModel obj = (OrderModel)this.SelectedItem_MyOrders;

            bool result = CustomMessageBox.Question("是否撤销该笔报价？", "删除订单");
            if (result == true)
            {
                // modi:cyc date:20210406 Memo:临时注释掉
                ////if (BDB.Common.Api.ClientApi.Instance.EnabledMsgLog == "1")
                ////{
                ////    LogHandler.Info("点击撤单按钮");
                ////}
                OrderCancelRequest cancelPo = new OrderCancelRequest();
                cancelPo.AlgoName = CommonUtils.ConvertToString(obj.AlgoName);
                cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                cancelPo.OrderID = CommonUtils.ConvertToString(obj.OrderID);
                if (string.IsNullOrEmpty(cancelPo.OrderID))
                {
                    cancelPo.OrderID = CommonUtils.ConvertToString(obj.STCCode);
                }
                TBookCancelReplyPo reply = await CancelOrder(cancelPo);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        //this.SelectedItem_MyOrders[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                    }
                    else
                    {
                        CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
                    }
                }
            }
        }

        public async Task<TBookCancelReplyPo> CancelOrder(OrderCancelRequest cancelPo)
        {
            TBookCancelReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = OrdersHandler.Instance.CancelOrder(cancelPo);

            });
           return reply;
        }

        private async void CancelSelectedOrders()
        {
            bool result = CustomMessageBox.Question($"是否撤销选中{this.CheckedCount4MyOrders}笔报价？", "删除订单");
            if (result == true)
            {
                // modi:cyc date:20210406 Memo:临时注释掉
                ////if (BDB.Common.Api.ClientApi.Instance.EnabledMsgLog == "1")
                ////{
                ////    LogHandler.Info("选中订单撤单");
                ////}
                StringBuilder error = null;

                error = await CancelSelectedOrdersAsync();
                if (error.Length > 0)
                {
                    CustomMessageBox.Error(string.Format($"删除订单失败, 原因:{error}"));
                }

            }
            
            IsHiddenMyOrdersOperationGrid = Visibility.Collapsed;
            IsAllChecked = false;
        }

        private async Task<StringBuilder> CancelSelectedOrdersAsync()
        {
            StringBuilder error = new StringBuilder();
            await Task.Run(() =>
            {
                List<OrderCancelRequest> cancelOrdersList = new List<OrderCancelRequest>();
                int count = 0;
                ConcurrentDictionary<string, TBookCancelReplyPo> dic = new ConcurrentDictionary<string, TBookCancelReplyPo>();

                lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    foreach (var item in ObsMyReqOrders)
                    {
                        if (item.IsChecked && item.EnabledBtn && item.ShowDeleteBtn)
                        {
                            OrderCancelRequest cancelPo = new OrderCancelRequest();
                            cancelPo.AlgoName = CommonUtils.ConvertToString(item.AlgoName);
                            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                            cancelPo.OrderID = CommonUtils.ConvertToString(item.STCCode);
                            cancelOrdersList.Add(cancelPo);
                            count++;
                        }
                    }
                }
                if (cancelOrdersList != null)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        foreach (OrderCancelRequest cancelPo in cancelOrdersList)
                        {
                            try
                            {
                                TBookCancelReplyPo reply = OrdersHandler.Instance.CancelOrder(cancelPo);
                                dic[cancelPo.OrderID] = reply;

                                Thread.Sleep(200);
                            }
                            catch (Exception e)
                            {
                                LogHandler.Error(e);
                            }
                        }
                    });
                }
                while (true)
                {
                    if (count == dic.Count)
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }

                foreach (KeyValuePair<string, TBookCancelReplyPo> pair in dic)
                {
                    string orderID = pair.Key;
                    TBookCancelReplyPo reply = pair.Value;
                    if (reply != null)
                    {
                        if (reply.IsSuccess() == false)
                        {
                            error.AppendLine($"订单号{orderID}:{reply.Msg}");
                        }
                        //else
                        //{
                        //    item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                        //}
                    }
                }
            });

            return error;
        }


        public void SetAllCancelRecordSelect(bool issel)
        {
            foreach (var x in ObsMyCancelOrders)
            {
                bool isvalid = x.IsPoint == "0";
                if (isvalid)
                {
                    x.IsChecked = issel;
                }
            }
        }
        #endregion

        #region 我的订单全选逻辑
        // 是否选中全选多选框
        private bool _IsAllChecked = false;
        public bool IsAllChecked
        {
            get
            {
                return _IsAllChecked;
            }

            set
            {
                if (_IsAllChecked != value)
                {
                    _IsAllChecked = value;
                    this.OnPropertyChanged(nameof(IsAllChecked));
                }
            }
        }

        public void CalcRecordCount()
        {
            CheckedCount4MyOrders = ObsMyReqOrders.Where(a => a.IsChecked).ToList().Count;
            IsHiddenMyOrdersOperationGrid = CheckedCount4MyOrders > 0?  Visibility.Visible : Visibility.Collapsed;
            this.OnPropertyChanged(nameof(CheckedCount4MyOrders));
        }

        public void SetAllReqRecordSelect(bool issel)
        {
            foreach (var x in ObsMyReqOrders)
            {
                bool isvalid = (x.IsPoint == "0") && (x.ExecType != "6");
                if (isvalid)
                {
                    x.IsChecked = issel;
                }
            }
        }

        private void UncheckAll()
        {
            IsAllChecked = false;
            lock(MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (var item in ObsMyReqOrders)
                {
                    item.IsChecked = false;
                }
            }

            IsHiddenMyOrdersOperationGrid = Visibility.Collapsed;
        }
        #endregion

        #region 编辑订单
        private void EditOrder()
        {
            if (this.SelectedItem_MyOrders == null)
            {
                return;
            }
            OrderModel obj = (OrderModel)this.SelectedItem_MyOrders;


            string strategyCodeID = CommonUtils.ConvertToString(obj.OrderID);
            if (m_windowManager.IsExist($"EditOrder-{strategyCodeID}"))
            {
                return;
            }

            // to do...

            NewOrderSingle singleOrder = GetNewOrderSingle(obj);
            PriceMatchParam priceMatchParam = new PriceMatchParam();


            priceMatchParam.CustomFirmAmount = CommonUtils.ConvertToLong(singleOrder.OrderQty).ToString(); // OrderQty 订单量
            priceMatchParam.OrderId = singleOrder.StrategyOrderId;// 订单号
            priceMatchParam.RandomNum = singleOrder.VenuesRdmNum; // 渠道类型默认，填写随机几家渠道
            priceMatchParam.Side = singleOrder.Side; // 方向
            priceMatchParam.Symbol = singleOrder.SecurityID; // 代码
            priceMatchParam.SymbolName = singleOrder.Symbol; // 名称
            priceMatchParam.WholeFlag = singleOrder.WholeFlag; // 整量标志
            priceMatchParam.NetPrice = singleOrder.NetPrice; // 净价
            priceMatchParam.Price = singleOrder.Price; // 收益率
            priceMatchParam.SettlType = singleOrder.SettlType; // 清算速度
            priceMatchParam.NonfirmTime = singleOrder.FirmToNonTime; // nonfirmTime;
            priceMatchParam.CrossVenues = singleOrder.CrossVenues;// 撮合渠道
            priceMatchParam.VenuesSelectF = singleOrder.VenuesSelectF;//渠道类型  不填写1,默认2，指定3 
            priceMatchParam.SelfSelectVenues = singleOrder.SelfSelectVenues;//指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC

            priceMatchParam.InstitutionID = singleOrder.InstitutionID; //客盘机构id
            priceMatchParam.InstitutionName = singleOrder.InstitutionName;//客盘机构名字
            priceMatchParam.TraderID = singleOrder.TraderID; // 客盘交易员id
            priceMatchParam.TraderName = singleOrder.TraderName;//客盘交易员名字
            priceMatchParam.Terminal = singleOrder.Terminal;
            priceMatchParam.Info3 = singleOrder.Info3;//用于IDB单子
            priceMatchParam.Info1 = singleOrder.Info1;//用于记忆报价有效时长
            priceMatchParam.Venues = singleOrder.Venues; // 渠道
            priceMatchParam.OldOrder = singleOrder;
            
            SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);
            // bid;
            if (singleOrder.Side == "1")
            {
                sendPriceView.OfrBtn.IsEnabled = false;
            }
            // ofr
            else if (singleOrder.Side == "2")
            {
                sendPriceView.BidBtn.IsEnabled = false;
            }

            if (singleOrder.FirmFlag == "1")
            {
                //sendPriceView.AmountTxt.IsEnabled = false;
                //sendPriceView.btnOne.IsEnabled = sendPriceView.btnThree.IsEnabled = sendPriceView.btnFive.IsEnabled = false;
            }

            sendPriceView.Search_TextBox.IsEnabled = false;
            sendPriceView.lbDirect.IsEnabled = false;
            //Modi:cyc Date:20210406 Memo:临时注释掉 ？
            ////sendPriceView.btnXBond.IsEnabled = false;

            //sendPriceView.datePicker.IsEnabled = false;
            //sendPriceView.cbSettle.IsEnabled = false;
            //sendPriceView.lbOne.IsEnabled = sendPriceView.lbOther.IsEnabled = false;
            //sendPriceView.lbSpotOne.IsEnabled = sendPriceView.lbSpotOther.IsEnabled = false;

            //sendPriceView.lbAddOneNotLimit.IsEnabled = sendPriceView.lbAddOneToday.IsEnabled = sendPriceView.lbAddOneTomorrow.IsEnabled = false;

            //sendPriceView.Show();
            m_windowManager.OpenOnlyOneWindow($"EditOrder-{strategyCodeID}", sendPriceView);
        }

        private NewOrderSingle GetNewOrderSingle(OrderModel drv)
        {
            string strategyCodeID = CommonUtils.ConvertToString(drv.OrderID);
            if (string.IsNullOrEmpty(strategyCodeID))
            {
                strategyCodeID = CommonUtils.ConvertToString(drv.STCCode);
            }
            string securityID = CommonUtils.ConvertToString(drv.SecurityID);
            string qty = CommonUtils.ConvertToString(drv.Qty);
            string exeqty = CommonUtils.ConvertToString(drv.ExecQty);
            string price = CommonUtils.ConvertToString(drv.Price);
            string netPrice = CommonUtils.ConvertToString(drv.NetPrice);
            string settlType = CommonUtils.ConvertToString(drv.SettleType);
            string side = CommonUtils.ConvertToString(drv.Side);
            string wholeFlag = CommonUtils.ConvertToString(drv.WholeFlag);
            string firmFlag = CommonUtils.ConvertToString(drv.FirmFlag);
            string crossVenues = CommonUtils.ConvertToString(drv.CrossVenues);
            string firmToNonTime = CommonUtils.ConvertToString(drv.NonfirmTime);
            string randomNum = CommonUtils.ConvertToString(drv.VenuesRdmNum);
            string venuesSelectF = CommonUtils.ConvertToString(drv.VenuesSelectF);
            string selfSelectVenues = CommonUtils.ConvertToString(drv.SelfSelectVenues);
            string venues = CommonUtils.ConvertToString(drv.Venues);
            string venueTypeGw = CommonUtils.ConvertToString(drv.VenueTypeGW);
            string algoName = CommonUtils.ConvertToString(drv.AlgoName);
            string quantID = CommonUtils.ConvertToString(drv.QuantID);
            string timeInForce = CommonUtils.ConvertToString(drv.TimeInForce);
            string ordStatus = CommonUtils.ConvertToString(drv.OrderStatus);
            string bizType = CommonUtils.ConvertToString(drv.BizType);
            string bizTypeID = CommonUtils.ConvertToString(drv.BizTypeID);
            string quoteID = CommonUtils.ConvertToString(drv.QuoteID);
            string roleID = CommonUtils.ConvertToString(drv.RoleID);
            string roleName = CommonUtils.ConvertToString(drv.RoleName);
            string symbol = CommonUtils.ConvertToString(drv.Symbol);
            string userId = CommonUtils.ConvertToString(drv.UserID);
            string institutionID = CommonUtils.ConvertToString(drv.InstitutionID);
            string institutionName = CommonUtils.ConvertToString(drv.InstitutionName);
            string traderID = CommonUtils.ConvertToString(drv.TraderID);
            string traderName = CommonUtils.ConvertToString(drv.TraderName);
            string showTrader = CommonUtils.ConvertToString(drv.ShowTrader);
            string ternimal = CommonUtils.ConvertToString(drv.Terminal);
            string info3 = CommonUtils.ConvertToString(drv.Info3);
            string info1 = CommonUtils.ConvertToString(drv.Info1);
            NewOrderSingle sendPricePo = new NewOrderSingle();

            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            if (bondsInfoPo != null)
            {
                sendPricePo.SecurityType = bondsInfoPo.SECURITY_TYPE;
            }
            sendPricePo.BizType = bizType;
            sendPricePo.BizTypeID = bizTypeID;
            sendPricePo.QuoteID = quoteID;
            sendPricePo.RoleID = roleID;
            sendPricePo.RoleName = roleName;
            sendPricePo.Symbol = symbol;
            sendPricePo.UserID = userId;
            sendPricePo.OrdStatus = ordStatus;
            sendPricePo.StrategyOrderId = strategyCodeID;

            sendPricePo.AlgoName = algoName;
            sendPricePo.VenueTypeGW = venueTypeGw;
            sendPricePo.Venues = venues;
            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            sendPricePo.MarketIndicator = "4";
            sendPricePo.OrdType = "2";
            sendPricePo.OrderQty = (CommonUtils.ConvertToDecimal(qty) - CommonUtils.ConvertToDecimal(exeqty)).ToString();
            sendPricePo.Price = price;
            sendPricePo.NetPrice = netPrice;
            sendPricePo.SecurityID = securityID;

            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            sendPricePo.SettlType = settlType;

            sendPricePo.Side = side;

            sendPricePo.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = timeInForce;
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;

            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = firmFlag;

            sendPricePo.CrossVenues = crossVenues;

            sendPricePo.WholeFlag = wholeFlag;

            sendPricePo.QuantID = quantID;

            sendPricePo.FirmToNonTime = firmToNonTime;

            sendPricePo.VenuesRdmNum = randomNum;

            sendPricePo.VenuesSelectF = venuesSelectF;

            sendPricePo.SelfSelectVenues = selfSelectVenues;

            sendPricePo.InstitutionID = institutionID; //客盘机构id
            sendPricePo.InstitutionName = institutionName;//客盘机构名字
            sendPricePo.TraderID = traderID; // 客盘交易员id
            sendPricePo.TraderName = traderName;//客盘交易员名字
            sendPricePo.ShowTrader = showTrader;// 是否显示经济商

            //sendPricePo.ValidUntilTime = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");
            sendPricePo.Terminal = ternimal;
            sendPricePo.Info3 = info3;
            sendPricePo.Info1 = info1;

            return sendPricePo;
        }


        #endregion

        

        #region 产生新订单
       
        private void ResumeOrder()
        {
            if (this.SelectedItem_MyCancelOrders == null)
            {
                return;
            }

            OrderModel om = (OrderModel)this.SelectedItem_MyCancelOrders;

            string strategyCodeID = CommonUtils.ConvertToString(om.STCCode);
            if (m_windowManager.IsExist($"ResumeOrder-{strategyCodeID}"))
            {
                return;
            }

            NewOrderSingle singleOrder = GetNewOrderSingle(this.SelectedItem_MyCancelOrders);
            PriceMatchParam priceMatchParam = new PriceMatchParam();
            priceMatchParam.CustomFirmAmount = priceMatchParam.CustomFirmAmount = CommonUtils.ConvertToLong(singleOrder.OrderQty).ToString(); // OrderQty 订单量; // OrderQty 订单量
            priceMatchParam.OrderId = singleOrder.StrategyOrderId;// 订单号
            priceMatchParam.RandomNum = singleOrder.VenuesRdmNum; // 渠道类型默认，填写随机几家渠道
            priceMatchParam.Side = singleOrder.Side; // 方向
            priceMatchParam.Symbol = singleOrder.SecurityID; // 代码
            priceMatchParam.SymbolName = singleOrder.Symbol; // 名称
            priceMatchParam.WholeFlag = singleOrder.WholeFlag; // 整量标志
            priceMatchParam.NetPrice = singleOrder.NetPrice; // 净价
            priceMatchParam.Price = singleOrder.Price; // 收益率
            priceMatchParam.SettlType = singleOrder.SettlType; // 清算速度
            priceMatchParam.NonfirmTime = singleOrder.FirmToNonTime; // nonfirmTime;
            priceMatchParam.CrossVenues = singleOrder.CrossVenues;// 撮合渠道
            priceMatchParam.VenuesSelectF = singleOrder.VenuesSelectF;//渠道类型  不填写1,默认2，指定3 
            priceMatchParam.SelfSelectVenues = singleOrder.SelfSelectVenues;//指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC

            priceMatchParam.InstitutionID = singleOrder.InstitutionID; //客盘机构id
            priceMatchParam.InstitutionName = singleOrder.InstitutionName;//客盘机构名字
            priceMatchParam.TraderID = singleOrder.TraderID; // 客盘交易员id
            priceMatchParam.TraderName = singleOrder.TraderName;//客盘交易员名字

            priceMatchParam.Terminal = singleOrder.Terminal;
            priceMatchParam.Info3 = singleOrder.Info3;
            priceMatchParam.Info1 = singleOrder.Info1;//用于记忆报价有效时长
            priceMatchParam.Venues = singleOrder.Venues; // 渠道
            priceMatchParam.OldOrder = singleOrder;
            if (singleOrder.FirmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.UpdataPrice = false;
            SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);

            //// bid;
            //if (singleOrder.Side == "1")
            //{
            //    sendPriceView.OfrBtn.IsEnabled = false;
            //}
            //// ofr
            //else if (singleOrder.Side == "2")
            //{
            //    sendPriceView.BidBtn.IsEnabled = false;
            //}
            // 禁止点击tabpage
            sendPriceView.BidBtn.IsEnabled = false;
            sendPriceView.OfrBtn.IsEnabled = false;

            if (singleOrder.FirmFlag == "1")
            {
                sendPriceView.Search_TextBox.IsEnabled = false;
            }
            else
            {
                sendPriceView.Search_TextBox.IsEnabled = false;
            }

            sendPriceView.lbDirect.IsEnabled = false;

            //sendPriceView.lbOne.IsEnabled = sendPriceView.lbOther.IsEnabled = false;
            //sendPriceView.lbSpotOne.IsEnabled = sendPriceView.lbSpotOther.IsEnabled = false;

            //sendPriceView.Show();
            m_windowManager.OpenOnlyOneWindow($"ResumeOrder-{strategyCodeID}", sendPriceView);
        }

                
        #endregion

        #region 历史查询
        /// <summary>
        /// 打开历史查询界面
        /// </summary>
        public void OpenHistoryPage(bool isExecution)
        {
            // 使用插件形式打开历史页面
            int tradeBookPluginKey = (int)PluginKeyEnum.HistoryPlugin;

            IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(tradeBookPluginKey);
            if (plugin != null)
            {
                bool isExist = TabPageHelper.IsExistTabItem(plugin.Description);
                if (isExist == false)
                {
                    AppUtils.Application.PluginManager.EnablePlugin(tradeBookPluginKey);
                }
            }
        }

        #endregion

        #region 复制处理

        public string CopySTCCodeText(IList SelectedItemsList,string tabName)
        {
            StringBuilder sb = new StringBuilder();
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (var v in SelectedItemsList)
                {
                    if (tabName.Equals("Orders"))
                    {
                        OrderModel dgr = (OrderModel)v;
                        string stcCodeText = dgr.STCCode as string;
                        sb.Append(stcCodeText + "\n");
                    }
                    else if (tabName.Equals("Deals"))
                    {
                        ExecutionsOrderModel dgr = (ExecutionsOrderModel)v;
                        string stcCodeText = dgr.StrategyOrderID as string;
                        sb.Append(stcCodeText + "\n");
                    }
                    else if (tabName.Equals("Cancels"))
                    {
                        OrderModel dgr = (OrderModel)v;
                        string stcCodeText = dgr.STCCode as string;
                        sb.Append(stcCodeText + "\n");
                    }
                }
            }
            return sb.ToString();
        }

        public string CopyRowText(IList SelectedItemsList, string tabName)
        {
            StringBuilder sb = new StringBuilder();
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (var v in SelectedItemsList)
                {
                    if (tabName.Equals("Orders"))
                    {
                        OrderModel dgr = (OrderModel)v;
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr.Side as string, "side") + "\t");
                        sb.Append(dgr.SecurityID as string + "\t");
                        sb.Append(dgr.Symbol as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string , "settletype") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.FIRMFLAG_ORDER_FIELD] as string, "firmflag") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.ORDERSTATUS_ORDER_FIELD] as string, "ordstatus") + "\t");
                        //sb.Append(dgr[MyOrderProcesser.ORDERTYPE_FIELD] as string + "\t");
                        sb.Append(string.Format("{0:N4}", dgr.Price) + "%\t");
                        //sb.Append(this.Covert(dgr[MyOrderProcesser.WHOLE_FLAG_FIELD] , "wholeflag") + "\t");
                        sb.Append(dgr.Qty + "w\t");
                        //sb.Append(this.NumCovert(dgr[MyOrderProcesser.EXECQTY_ORDER_FIELD]) + "\t");
                        //sb.Append(CommonConstants.SHANXIZHENGQUAN + "\t");//dgr[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venues") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.CROSS_VENUES_FIELD] as string, "CrossVenues") + "\t");
                        //sb.Append(dgr[MyOrderProcesser.VENUES_SELECT_DISPLAY_FIELD] as string + "\t");
                        //sb.Append(dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string + "\t");
                        //sb.Append(TimeUtils.Instance.ToTime(dgr[MyOrderProcesser.UPDATETIME_ORDER_FIELD] as string) + "\t");

                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr.SettleType as string, "settletype_myorder"));
                        /*sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venuesorder")+ "\t");
                        sb.Append(dgr[MyOrderProcesser.STCCODE_ORDER_FIELD]);*/
                        sb.Append("\n");
                    }
                    else if (tabName.Equals("Deals"))
                    {
                        ExecutionsOrderModel dgr = (ExecutionsOrderModel)v;
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr.Side as string, "side") + "\t");
                        sb.Append(dgr.SecurityID as string + "\t");
                        sb.Append(dgr.Symbol as string + "\t");
                        sb.Append(string.Format("{0:N4}%", dgr.LastPx_Display) + "\t");
                        /*sb.Append(dgr[MyExecutionProcesser.CUMQTY_FIELD] + "w\t");*/
                        sb.Append(this.NumCovert(dgr.LastQty) + "w\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr.SettlType as string, "settletype") + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr.Venues as string, "venues_exe") + "\t");
                        sb.Append(dgr.ExecID as string + "\t");
                        /*sb.Append(TimeUtils.Instance.ToTime(dgr[MyExecutionProcesser.EXECUTIONTIME_FIELD] as string) + "\t");*/
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SYNC_STATUS_FIELD] as string, "synchflag") + "\t");

                        //sb.Append(this.NumCovert(dgr[MyExecutionProcesser.NETPRICE_FIELD]) + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SETTLTYPE_FIELD] as string, "settletype") + "\t");


                        //string side = dgr[MyExecutionProcesser.SIDE_FIELD] as string;// ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SIDE_FIELD] as string, "side");

                        //if (side.Equals("1"))
                        //{
                        //    sb.Append(this.StringConvert(dgr[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]) + "\tto\t");
                        //    sb.Append(CommonConstants.SHANXIZHENGQUAN + "\t");
                        //    //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.PARTYNAME_FIELD]) + "\t");
                        //}
                        //else
                        //{
                        //    //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.PARTYNAME_FIELD]) + "\tto\t");
                        //    sb.Append(CommonConstants.SHANXIZHENGQUAN + "\tto\t");
                        //    sb.Append(this.StringConvert(dgr[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]) + "\t");
                        //}

                        //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.COUNTERPARTYSUBNAME_FIELD]) + "\t");
                        //sb.Append(dgr[MyExecutionProcesser.EXECID_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.VENUES_FIELD] as string, "venues_exe") + "\t");
                        //sb.Append(dgr[MyExecutionProcesser.PARTYSUBNAME_FIELD] as string + "\t");
                        //sb.Append(TimeUtils.Instance.ToTime(dgr[MyExecutionProcesser.EXECUTIONTIME_FIELD] as string) + "\t");
                        //sb.Append(dgr[MyExecutionProcesser.STRATEGY_ORDER_ID_FIELD] as string + "\t");
                        //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.REFORDERID_FIELD]) + "\t");     

                        sb.Append("\n");
                    }
                    else if (tabName.Equals("Cancels"))
                    {
                        OrderModel dgr = (OrderModel)v;
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr.Side as string, "side") + "\t");
                        sb.Append(dgr.SecurityID as string + "\t");
                        sb.Append(dgr.Symbol as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.FIRMFLAG_ORDER_FIELD] as string, "firmflag") + "\t");
                        sb.Append(string.Format("{0:N4}%", dgr.Price) + "\t");
                        //sb.Append(this.Covert(dgr[MyOrderProcesser.WHOLE_FLAG_FIELD], "wholeflag") + "\t");
                        sb.Append(dgr.Qty+ "w\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr.SettleType as string, "settletype_myOrder") + "\t");
                        /*sb.Append(this.NumCovert(dgr[MyOrderProcesser.EXECQTY_ORDER_FIELD] )+ "\t");*/
                        // sb.Append(dgr[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string + "\t");
                        /*sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venues") + "\t");*/
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.CROSS_VENUES_FIELD] as string, "CrossVenues") + "\t");
                        //sb.Append(dgr[MyOrderProcesser.VENUES_SELECT_DISPLAY_FIELD] as string + "\t");
                        /*sb.Append(dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string + "\t");*/
                       /* sb.Append(TimeUtils.Instance.ToTime(dgr[MyOrderProcesser.UPDATETIME_ORDER_FIELD] as string) + "\t");*/
                        sb.Append("\n");
                    }
                }
            }
            return sb.ToString();
        }

        
        private string Covert(object value,string parameter)
        {
            if (value != null)
            {
                string key = value.ToString();
                string[] keyArr = key.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                string param = parameter.ToString();

                StringBuilder sb = new StringBuilder();
                foreach (string item in keyArr)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(CommonConstants.COMMA_CHAR);
                    }
                    sb.Append(ComboSourceUtil.Instance.FindEnum(item.Trim(), param));
                }
                return sb.ToString();

            }
            return "";
        }
        private string NumCovert(object value)
        {
            if (value != null)
            {
                if (value is decimal)
                {
                    decimal temp = CommonUtils.ConvertToDecimal(value.ToString());
                    if (temp != decimal.Zero)
                    {
                        return temp.ToString("N4");
                    }
                }
                else if (value is long)
                {
                    long temp = CommonUtils.ConvertToLong(value.ToString());
                    if (temp != 0)
                    {
                        return temp.ToString();
                    }
                }
                else if (value is int)
                {
                    int temp = CommonUtils.ConvertToInt(value.ToString());
                    if (temp != 0)
                    {
                        return temp.ToString();
                    }
                }
            }
            return "--";
        }
        public object StringConvert(object value)
        {
            if (value == null || value.ToString() == string.Empty)
            {
                return "--";
            }
            return value;
        }

        #endregion

        #region 点价倒计时

        /// <summary>
        /// 更新我的订单剩余时间
        /// </summary>
        public void UpdatMyOrdersTimeRemains()
        {
            // var m = ObsMyReqOrders.FirstOrDefault(o => o.OrderID == rs.OrderID);
            foreach (var x in ObsMyReqOrders)
            {
                if (x.IsPoint == "1")
                {
                    int icotimeout = 0;
                    int res = 0;
                    int.TryParse(x.IocTimeOut.ToString(), out icotimeout);

                    // 保存剩余时间
                    DateTime transactTime = CommonUtils.ConvertToDateTime(x.TransactTime.ToString()).AddSeconds(icotimeout);
                    TimeSpan diffTime = transactTime - DateTimeEx.ServerTime;
                    res = Convert.ToInt32(diffTime.TotalSeconds);
                    x.RemainSeconds = res < 0 ? "0" : res.ToString();
                }
            }
        }
        #endregion
    }


}
