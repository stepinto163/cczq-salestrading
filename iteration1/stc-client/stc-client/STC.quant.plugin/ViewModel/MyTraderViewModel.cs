using Ats.Foundation.Message;
using com.gateway.log;
using Microsoft.Expression.Interactivity.Core;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Cal;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Cal;
using STC.Common.Model.TBook;
using STC.Common.Model.WorkTable;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.Common.ViewModel;
using STC.Frame.Common;
using STC.quant.plugin.Utils;
using STC.quant.plugin.Utils.Data;
using STC.quant.plugin.View.Component;
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace STC.quant.plugin.ViewModel
{

    /// <summary>
    /// 批量操作类型
    /// </summary>
    public enum BatchOperateTypeEnum
    {
        /// <summary>
        /// 无操作
        /// </summary>
        None,

        /// <summary>
        /// 转firm
        /// </summary>
        ToFirm,

        /// <summary>
        /// 转Nonfirm
        /// </summary>
        ToNonFirm,

        /// <summary>
        /// 编辑价格
        /// </summary>
        EditPrice,

        /// <summary>
        /// 编辑数量
        /// </summary>
        EditQty,

        /// <summary>
        /// 编辑渠道
        /// </summary>
        EditVenues,
    }


    /// <summary>
    /// 我的订单/我的成交/已撤订单MyTraderView的数据内容
    /// </summary>
    public class MyTraderViewModel : ViewModelBase
    {
        WindowManager m_windowManager = WindowManager.Instance;//new WindowManager();

        public UserControl myWindow;
        /// <summary>
        /// 定时刷新改变checkbox之后， 界面的变化情况
        /// </summary>
        Thread refreshCheckBoxThread = null;

        /// <summary>
        /// 是否刷新已撤订单状态变化
        /// </summary>
        private volatile bool isAdjustAllChkStatus4CancelOrders = false;

        /// <summary>
        /// 是否刷新我的订单状态变化
        /// </summary>
        private volatile bool isAdjustAllChkStatus = false;

        /// <summary>
        /// 0: 显示 1：不显示
        /// </summary>
        private Visibility isHiddenMyOrdersOperationGrid;

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
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 转firm按钮是否可用
        /// </summary>
        private bool _ToFirmBtn_IsEnabled = true;

        public bool ToFirmBtn_IsEnabled
        {
            get
            {
                return _ToFirmBtn_IsEnabled;
            }
            set
            {
                if (_ToFirmBtn_IsEnabled != value)
                {
                    _ToFirmBtn_IsEnabled = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 转Nonfirm按钮是否可用
        /// </summary>
        private bool _ToNonfirmBtn_IsEnabled = true;

        public bool ToNonfirmBtn_IsEnabled
        {
            get
            {
                return _ToNonfirmBtn_IsEnabled;
            }
            set
            {
                if (ToNonfirmBtn_IsEnabled != value)
                {
                    _ToNonfirmBtn_IsEnabled = value;
                    this.OnPropertyChanged();
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

        // 我的订单总数
        private int _totalCount4MyOrders = 0;
        public int TotalCount4MyOrders
        {
            get
            {
                return _totalCount4MyOrders;
            }
            set
            {
                if (_totalCount4MyOrders != value)
                {
                    _totalCount4MyOrders = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 我的订单中bid的总数
        /// </summary>
        private int _totalCount4MyBidOrders = 0;
        public int TotalCount4MyBidOrders
        {
            get
            {
                return _totalCount4MyBidOrders;
            }
            set
            {
                if (_totalCount4MyBidOrders != value)
                {
                    _totalCount4MyBidOrders = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 我的订单中ofr的总数
        /// </summary>
        private int _totalCount4MyOfrOrders = 0;
        public int TotalCount4MyOfrOrders
        {
            get
            {
                return _totalCount4MyOfrOrders;
            }
            set
            {
                if (_totalCount4MyOfrOrders != value)
                {
                    _totalCount4MyOfrOrders = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private DataView _MyOrdersDV;

        /// <summary>
        /// 我的订单数据源
        /// </summary>
        public DataView MyOrdersDV
        {
            get
            {
                lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    if (_MyOrdersDV == null)
                    {
                        _MyOrdersDV = new DataView(QuantMyOrderProcesser.Processer.MyOrdersDT);
                        //_MyOrdersDV.RowFilter = $"({QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'N' or {QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '0' or {QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '1' or {QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'S' or {QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '6')";
                        _MyOrdersDV.RowFilter = $"{QuantMyOrderProcesser.QUANTID_FIELD} =  null ";
                        _MyOrdersDV.Sort = $"{QuantMyOrderProcesser.UPDATETIME_ORDER_FIELD} desc";
                        this.OnPropertyChanged();
                    }
                    return this._MyOrdersDV;
                }
            }
        }

        private DataRowView _selectedItem_MyOrders;

        /// <summary>
        /// 我的订单选中行记录
        /// </summary>
        public DataRowView SelectedItem_MyOrders
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

        private DataView _MyExecutionsDV;

        /// <summary>
        /// 我的成交数据源
        /// </summary>
        public DataView MyExecutionsDV
        {
            get
            {
                lock (QuantMyExecutionProcesser.Processer.LockObj_MyExecutions)
                {
                    if (_MyExecutionsDV == null)
                    {
                        _MyExecutionsDV = new DataView(QuantMyExecutionProcesser.Processer.MyExecutionsDT);
                        _MyExecutionsDV.RowFilter = $"{QuantMyOrderProcesser.QUANTID_FIELD} =  null ";
                        _MyExecutionsDV.Sort = $"{QuantMyExecutionProcesser.EXECUTIONTIME_FIELD} desc";
                        this.OnPropertyChanged(nameof(MyExecutionsDV));
                    }
                    return this._MyExecutionsDV;
                }
            }
        }

        public void ChangeQuantID(string quantid)
        {
            try
            {
                TotalCountOrderHeader = "当日订单-" + quantid;
                TotalCountExecutionsHeader = "当日成交-" + quantid;
                string today = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
                _MyExecutionsDV.RowFilter = $"{QuantMyExecutionProcesser.QUANTID_FIELD} =  '{quantid}' and {QuantMyExecutionProcesser.EXECUTIONTIME_FIELD} > '{today}'";
                _MyOrdersDV.RowFilter = $"{QuantMyOrderProcesser.QUANTID_FIELD} =  '{quantid}' and {QuantMyOrderProcesser.UPDATETIME_ORDER_FIELD} > '{today}'";
            }
            catch (Exception e)
            {
            }
        }

        private DataRowView _selectedItem_MyExecutions;

        /// <summary>
        /// 我的成交选中行记录
        /// </summary>
        public DataRowView SelectedItem_MyExecutions
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

        private DataView _MyCancelOrdersDV;

        /// <summary>
        /// 已撤订单数据源
        /// </summary>
        public DataView MyCancelOrdersDV
        {
            get
            {
                lock (QuantMyOrderProcesser.Processer.LockObj_MyCancelOrders)
                {
                    if (_MyCancelOrdersDV == null)
                    {
                        _MyCancelOrdersDV = new DataView(QuantMyOrderProcesser.Processer.MyCancelOrdersDT);
                        //_MyCancelOrdersDV.RowFilter = $"{QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '4'";
                        _MyCancelOrdersDV.Sort = $"{QuantMyOrderProcesser.UPDATETIME_ORDER_FIELD} desc";
                        this.OnPropertyChanged();
                    }
                    return this._MyCancelOrdersDV;
                }
            }
        }

        private DataRowView _selectedItem_MyCancelOrders;

        /// <summary>
        /// 已撤订单选中行记录
        /// </summary>
        public DataRowView SelectedItem_MyCancelOrders
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
        /// <summary>
        /// 批量操作按钮是否可用
        /// </summary>
        private bool _BatchOperateBtnEnanble = true;

        public bool BatchOperateBtnEnable
        {
            get
            {
                return _BatchOperateBtnEnanble;
            }
            set
            {
                if (_BatchOperateBtnEnanble != value)
                {
                    _BatchOperateBtnEnanble = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// XBond渠道按钮是否可见
        /// </summary>
        private Visibility _Visibility_XBondBtn = Visibility.Visible;

        public Visibility Visibility_XBondBtn
        {
            get
            {
                return _Visibility_XBondBtn;
            }
            set
            {
                if (_Visibility_XBondBtn != value)
                {
                    _Visibility_XBondBtn = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 默认渠道按钮是否可见
        /// </summary>
        private Visibility _Visibility_DefaultBtn = Visibility.Collapsed;

        public Visibility Visibility_DefaultBtn
        {
            get
            {
                return _Visibility_DefaultBtn;
            }
            set
            {
                if (_Visibility_DefaultBtn != value)
                {
                    _Visibility_DefaultBtn = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 指定渠道按钮是否可见
        /// </summary>
        private Visibility _Visibility_Direct = Visibility.Visible;

        public Visibility Visibility_Direct
        {
            get
            {
                return _Visibility_Direct;
            }
            set
            {
                if (_Visibility_Direct != value)
                {
                    this._Visibility_Direct = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 我的订单Tab标题
        /// </summary>
        private string totalCountOrderHeader = "当日订单";
        public string TotalCountOrderHeader
        {
            get
            {
                return totalCountOrderHeader;
            }
            set
            {
                totalCountOrderHeader = value;
                this.OnPropertyChanged(nameof(TotalCountOrderHeader));
            }
        }
        /// <summary>
        /// 我的订单 是否显示小红点 +0 显示，""不显示
        /// </summary>
        private string isMyOrdersShowRedDot = "";
        public string IsMyOrdersShowRedDot
        {
            get
            {
                return isMyOrdersShowRedDot;
            }
            set
            {
                isMyOrdersShowRedDot = value;
                this.OnPropertyChanged(nameof(IsMyOrdersShowRedDot));
            }
        }
        

        /// <summary>
        /// 我的成交Tab标题
        /// </summary>
        private string _TotalCountExecutionsHeader = "当日成交";
        public string TotalCountExecutionsHeader
        {
            get
            {
                return _TotalCountExecutionsHeader;
            }
            set
            {
                _TotalCountExecutionsHeader = value;
                this.OnPropertyChanged(nameof(TotalCountExecutionsHeader));
            }
        }

        /// <summary>
        /// 我的成交是否显示小红点 +0 显示，""不显示
        /// </summary>
        private string isMyExecutionsShowRedDot = "";
        public string IsMyExecutionsShowRedDot
        {
            get
            {
                return isMyExecutionsShowRedDot;
            }
            set
            {
                isMyExecutionsShowRedDot = value;
                this.OnPropertyChanged(nameof(IsMyExecutionsShowRedDot));
            }
        }

        private ObservableCollection<RandomBtnModel> _RandomBtns = new ObservableCollection<RandomBtnModel>();
        public ObservableCollection<RandomBtnModel> RandomBtns
        {
            get
            {
                return _RandomBtns;
            }
        }

        private int bncFirstCount = 0;
        private int bncSecondCount = 0;
        private int bcoFirstCount = 0;
        private int bcoSecondCount = 0;
        private int bncDirectCount = 0;
        private int bcoDirectCount = 0;
        private bool bncHasXBond = false;
        private bool bcoHasXBond = false;

        Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic = new Dictionary<string, Common.Model.Fix.Message.NewOrderSingle>();
        Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
        Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
        public ICommand DelOrderCommand { get; set; }

        public ICommand CancelAllOrdersCommand { get; set; }

        public ICommand CancelSelectedOrdersCommand { get; set; }

        public ICommand UncheckAllCommand { get; set; }

        public ICommand EditOrderCommand { get; set; }

        public ICommand ToNonfirmCommand { get; set; }

        public ICommand ToFirmCommand { get; set; }

        public ICommand ProcessIDBDataCommand { get; set; }

        public ICommand ResumeCommand { get; set; }

        public ICommand ResendCommand { get; set; }

        public ICommand EditPriceCommand { get; set; }

        public ICommand EditQtyCommand { get; set; }

        public ICommand EditVenuesCommand { get; set; }

        public ICommand AssignVenuesCommand { get; set; }
        

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

        public string TradeTabSelectedItem = "";
        /// <summary>
        /// 构造函数
        /// </summary>
        public MyTraderViewModel()
        {
            try
            {
                Thread refreshCheckBoxThread = new Thread(RefreshCheckBox);
                refreshCheckBoxThread.IsBackground = true;
                refreshCheckBoxThread.Start();

                DelOrderCommand = new RelayCommand(DeleteOrder);

                CancelAllOrdersCommand = new ActionCommand(DeleteAllOrders);

                CancelSelectedOrdersCommand = new RelayCommand(CancelSelectedOrders);

                UncheckAllCommand = new RelayCommand(UncheckAll);

                EditOrderCommand = new RelayCommand(EditOrder);

                ToNonfirmCommand = new RelayCommand(OnToNonFirm);

                ToFirmCommand = new RelayCommand(OnToFirm);

                ProcessIDBDataCommand = new RelayCommand(ProcessIDBData);

                ResumeCommand = new RelayCommand(ResumeOrder);

                ResendCommand = new RelayCommand(ResendOrders);

                EditPriceCommand = new RelayCommand<string>(EditPrice);

                EditQtyCommand = new RelayCommand<string>(EditQty);

                EditVenuesCommand = new RelayCommand<string>(EditVenues);

                //AssignVenuesCommand = new RelayCommand(AssignVenues);

                // 我的订单
                BindingOperations.EnableCollectionSynchronization(MyOrdersDV, QuantMyOrderProcesser.Processer.LockObj_MyOrders);
                this.MyOrdersDV.ListChanged += MyOrdersDV_ListChanged;
                QuantMyOrderProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions1;

                // 我的成交
                BindingOperations.EnableCollectionSynchronization(MyExecutionsDV, QuantMyExecutionProcesser.Processer.LockObj_MyExecutions);
                QuantMyExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;

               // 已撤订单
               BindingOperations.EnableCollectionSynchronization(MyCancelOrdersDV, QuantMyOrderProcesser.Processer.LockObj_MyCancelOrders);
                this.MyCancelOrdersDV.ListChanged += MyCancelOrdersDV_ListChanged;

                isAdjustAllChkStatus4CancelOrders = true;
                isAdjustAllChkStatus = true;

                List<Venue> bncFirstList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BNC, "1");
                List<Venue> bncSecondList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BNC, "2");
                List<Venue> bncDirectList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BNC, string.Empty);
                bncFirstCount = bncFirstList != null ? bncFirstList.Count : 0;
                bncSecondCount = bncSecondList != null ? bncSecondList.Count : 0;
                bncDirectCount = bncDirectList != null ? bncDirectList.Count : 0;
                if (bncDirectList != null)
                {
                    foreach (Venue v in bncDirectList)
                    {
                        if ("XBOND".Equals(v.name))
                        {
                            bncHasXBond = true;
                            break;
                        }
                    }
                }

                List<Venue> bcoFirstList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BCO, "1");
                List<Venue> bcoSecondList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BCO, "2");
                List<Venue> bcoDirectList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BCO, string.Empty);
                bcoFirstCount = bcoFirstList != null ? bcoFirstList.Count : 0;
                bcoSecondCount = bcoSecondList != null ? bcoSecondList.Count : 0;
                bcoDirectCount = bcoDirectList != null ? bcoDirectList.Count : 0;
                if (bncDirectList != null)
                {
                    foreach (Venue v in bcoDirectList)
                    {
                        if ("XBOND".Equals(v.name))
                        {
                            bcoHasXBond = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void Processer_OnLastExecutions1(object obj)
        {
            //TotalCountOrderHeader = "当日订单";
            int cnt = int.Parse(obj.ToString());
            if (cnt == 0)
            {
                IsMyOrdersShowRedDot = "";
               // TotalCountOrderHeader = "当日订单";
            }
            else
            {
                try
                {
                    if (TradeTabSelectedItem == "MyOrders_TabItem")
                    {
                        IsMyOrdersShowRedDot = "";
                    }
                    else
                    {
                        IsMyOrdersShowRedDot = "+0";
                    }
                }
                catch
                {
                    IsMyOrdersShowRedDot = "+0";
                }
                
                //TotalCountOrderHeader = $"当日订单({cnt})";
            }
        }

        private void Processer_OnLastExecutions(object obj)
        {
            TotalCountExecutionsHeader = "当日成交";
            int cnt = int.Parse(obj.ToString());
            if (cnt == 0)
            {
                IsMyExecutionsShowRedDot = "";
               // TotalCountExecutionsHeader = "当日成交";
            }
            else
            {
                try
                {
                    lock (QuantMyExecutionProcesser.Processer.LockObj_MyExecutions)
                    {
                        int recc = this.MyExecutionsDV.Table.Rows.Count;
                        foreach (DataRow myOrder in this.MyExecutionsDV.Table.Rows)
                        {
                            myOrder.BeginEdit();
                            myOrder["ID"] = recc;
                            myOrder.EndEdit();
                            myOrder.AcceptChanges();
                            recc--;
                        }
                    }

                    if (TradeTabSelectedItem == "MyExecutions_TabItem")
                    {
                        IsMyExecutionsShowRedDot = "";
                    }
                    else
                    {
                        IsMyExecutionsShowRedDot = "+0";
                    }

                }
                catch 
                {
                    IsMyExecutionsShowRedDot = "+0";
                }
                
               // TotalCountExecutionsHeader = $"当日成交({cnt})";
            }
        }

        private void RefreshCheckBox()
        {
            try
            {
                while (true)
                {
                    if (isAdjustAllChkStatus4CancelOrders)
                    {
                        isAdjustAllChkStatus4CancelOrders = false;
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this.AdjustAllChkStatus4CancelOrders();
                        }));
                        
                    }

                    if (isAdjustAllChkStatus)
                    {
                        isAdjustAllChkStatus = false;
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this.AdjustAllChkStatus();
                        }));
                    }
                    Thread.Sleep(500);
                }
            }
            catch(ThreadAbortException threadAbortException)
            {
                // ignore.
            }
            catch(Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void MyCancelOrdersDV_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (_isCheckBoxAllClick_CancelOrders == true)
            {
                return;
            }

            if (e.PropertyDescriptor == null 
                || (e.PropertyDescriptor != null && e.PropertyDescriptor.Name == QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD))
            {
                isAdjustAllChkStatus4CancelOrders = true;
                //AdjustAllChkStatus4CancelOrders();
            }
        }

        ~MyTraderViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            if (refreshCheckBoxThread != null)
            {
                refreshCheckBoxThread.Abort();
                refreshCheckBoxThread = null;
            }

            // 释放我的订单数据
            lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            {
                if (_MyOrdersDV != null)
                {
                    _MyOrdersDV.Dispose();
                    _MyOrdersDV = null;
                }
            }

            // 释放我的成交数据
            lock (QuantMyExecutionProcesser.Processer.LockObj_MyExecutions)
            {
                if (_MyExecutionsDV != null)
                {
                    _MyExecutionsDV.Dispose();
                    _MyExecutionsDV = null;
                }
            }
            base.Dispose();
        }

        StringBuilder filter_executions = new StringBuilder();
        /// <summary>
        /// 处理我的成交的过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessMyExecutionsFilterConditon(Dictionary<string, IList> filterDic)
        {
            filter_executions.Remove(0, filter_executions.Length);
            // venues
            IList venuesList = filterDic["Venues"];
            StringBuilder inFilterSB_venues = new StringBuilder();
            if (venuesList.Count > 0)
            {
                foreach (ListBoxItem item in venuesList)
                {
                    if (item.Name == "All")
                    {
                        inFilterSB_venues.Remove(0, inFilterSB_venues.Length);
                        break;
                    }

                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }

                    if (item.Content.ToString() == Constants.TBook)
                    {
                        inFilterSB_venues.Append("'MANUAL','QUANT','IDB'");
                    }
                    else if (item.Content.ToString() == Constants.XBond)
                    {
                        inFilterSB_venues.Append("'ODM'");
                    }
                    else if (item.Content.ToString() == CommonConstants.AllBroker && item.Name == "AllBroker")
                    {
                        inFilterSB_venues.Append("'CNEX','TPSC','CTTJ','PTCN','CCTB','UEDA'");
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
                    else if (item.Content.ToString() == CommonConstants.UEDA)
                    {
                        inFilterSB_venues.Append("'UEDA'");
                    }
                    else if (item.Content.ToString() == Constants.QDMESP)
                    {
                        inFilterSB_venues.Append("'QDMESP'");
                    }
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                filter_executions.Append($"{QuantMyExecutionProcesser.VENUES_FIELD} in ({inFilterSB_venues})");
            }

            // syncStatus
            IList syncStatusList = filterDic["SyncStatus"];
            StringBuilder inFilterSB_syncStatus = new StringBuilder();
            if (syncStatusList.Count > 0)
            {
                foreach (ListBoxItem item in syncStatusList)
                {
                    if (item.Content.ToString() == CommonConstants.ALL)
                    {
                        continue;
                    }

                    if (inFilterSB_syncStatus.Length > 0)
                    {
                        inFilterSB_syncStatus.Append(" or ");
                    }

                    if (item.Content.ToString() == Constants.SEND)
                    {
                        inFilterSB_syncStatus.Append($"{ QuantMyExecutionProcesser.SYNC_STATUS_FIELD} = 'Y'");
                    }
                    else if (item.Content.ToString() == Constants.SOLVE)
                    {
                        inFilterSB_syncStatus.Append($"{QuantMyExecutionProcesser.SYNC_STATUS_FIELD} = 'S'");
                    }
                }
            }

            if (inFilterSB_syncStatus.Length > 0)
            {
                if (filter_executions.Length > 0)
                {
                    filter_executions.Append(" and ");
                }
                filter_executions.Append($"({inFilterSB_syncStatus})");
            }

            // 利率/信用
            IList codelist = filterDic["CodeType"];
            StringBuilder inFilterSB_Code = new StringBuilder();
            if (codelist.Count > 0)
            {
                foreach (ListBoxItem item in codelist)
                {
                    if (item.Content.ToString() == CommonConstants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_Code.Length > 0)
                    {
                        inFilterSB_Code.Append(",");
                    }

                    if (item.Content.ToString() == Constants.RATE)
                    {
                        inFilterSB_Code.Append("'BNC'");
                    }
                    else if (item.Content.ToString() == Constants.CREDIT)
                    {
                        inFilterSB_Code.Append("'BCO'");
                    }
                }
            }

            if (inFilterSB_Code.Length > 0)
            {
                if (filter_executions.Length > 0)
                {
                    filter_executions.Append(" and ");
                }

                filter_executions.Append($"{QuantMyOrderProcesser.SECURITY_TYPE} in ({inFilterSB_Code})");
            }

            FilterExeData();
        }

        /// <summary>
        /// 过滤我的成交数据
        /// </summary>
        private void FilterExeData()
        {
            StringBuilder tempFilter = new StringBuilder();
            //if (this.SearchText.Length > 0)
            //{
            //    tempFilter.Append($"{SECURITYID_FIELD} like '%{this.SearchText}%'");
            //}

            if (filter_executions.Length > 0)
            {
                if (tempFilter.Length > 0)
                {
                    tempFilter.Append(" and ");
                }

                tempFilter.Append(filter_executions);
            }

            lock (QuantMyExecutionProcesser.Processer.LockObj_MyExecutions)
            {
                LogHandler.Info($"----------this.MyExecutionsDV.RowFilter-------{tempFilter}");
                this.MyExecutionsDV.RowFilter = tempFilter.ToString();
            }
        }

        StringBuilder filter_orders = new StringBuilder();
        /// <summary>
        /// 处理我的订单的过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessMyOrdersFilterCondition(Dictionary<string, IList> filterDic)
        {
            filter_orders.Remove(0, filter_orders.Length);
            // 人工，IDB， 量化
            IList venuesList = filterDic["Venues"];
            StringBuilder inFilterSB_venues = new StringBuilder();
            //string algoNameFiter = string.Empty;
            if (venuesList.Count > 0)
            {
                foreach (ListBoxItem item in venuesList)
                {
                    if (item.Content.ToString() == CommonConstants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }

                    if (item.Content.ToString() == Constants.MANUAL)
                    {
                        inFilterSB_venues.Append("'MANUAL'");
                    }
                    else if (item.Content.ToString() == Constants.IDB)
                    {
                        inFilterSB_venues.Append("'IDB'");
                    }
                    else if (item.Content.ToString() == Constants.QUANT)
                    {
                        inFilterSB_venues.Append("'QUANT'");
                    }
                    else if (item.Content.ToString() == Constants.INNER)
                    {
                        inFilterSB_venues.Append("'MANUAL'");
                    }
                    else if (item.Content.ToString() == Constants.QDMESP)
                    {
                        //algoNameFiter = $"{QuantMyOrderProcesser.ALGONAME_ORDER_FIELD} in ('MM')";
                        inFilterSB_venues.Append("'QDMESP'");
                    }
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                filter_orders.Append($"{QuantMyOrderProcesser.VENEUES_ORDER_FIELD} in ({inFilterSB_venues})");
            }

            //if (!string.IsNullOrEmpty(algoNameFiter))
            //{
            //    if (filter_orders.Length > 0)
            //    {
            //        filter_orders.Append(" and ");
            //    }

            //    filter_orders.Append(algoNameFiter);
            //}


            // firm，nonfirm
            IList list = filterDic["FirmFlag"];
            StringBuilder inFilterSB_FirmFlag = new StringBuilder();
            if (list.Count > 0)
            {
                foreach (ListBoxItem item in list)
                {
                    if (item.Content.ToString() == CommonConstants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_FirmFlag.Length > 0)
                    {
                        inFilterSB_FirmFlag.Append(",");
                    }

                    if (item.Content.ToString() == Constants.FIRM)
                    {
                        inFilterSB_FirmFlag.Append("'1'");
                    }
                    else if (item.Content.ToString() == Constants.NONFIRM)
                    {
                        inFilterSB_FirmFlag.Append("'0'");
                    }
                }
            }

            if (inFilterSB_FirmFlag.Length > 0)
            {
                if (filter_orders.Length > 0)
                {
                    filter_orders.Append(" and ");
                }

                filter_orders.Append($"{QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD} in ({inFilterSB_FirmFlag})");
            }


            // 利率/信用
            IList codelist = filterDic["CodeType"];
            StringBuilder inFilterSB_Code = new StringBuilder();
            if (codelist.Count > 0)
            {
                foreach (ListBoxItem item in codelist)
                {
                    if (item.Content.ToString() == CommonConstants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_Code.Length > 0)
                    {
                        inFilterSB_Code.Append(",");
                    }

                    if (item.Content.ToString() == Constants.RATE)
                    {
                        inFilterSB_Code.Append("'BNC'");
                    }
                    else if (item.Content.ToString() == Constants.CREDIT)
                    {
                        inFilterSB_Code.Append("'BCO'");
                    }
                }
            }

            if (inFilterSB_Code.Length > 0)
            {
                if (filter_orders.Length > 0)
                {
                    filter_orders.Append(" and ");
                }

                filter_orders.Append($"{QuantMyOrderProcesser.SECURITY_TYPE} in ({inFilterSB_Code})");
            }


            FilterOrderData();
        }

        /// <summary>
        /// 过滤我的订单数据
        /// </summary>
        private void FilterOrderData()
        {
            StringBuilder tempFilter = new StringBuilder();
            if (this.SearchText.Length > 0)
            {
                tempFilter.Append($"{QuantMyOrderProcesser.SECURITYID_ORDER_FIELD} like '%{this.SearchText}%'");
            }

            if (filter_orders.Length > 0)
            {
                if (tempFilter.Length > 0)
                {
                    tempFilter.Append(" and ");
                }

                tempFilter.Append(filter_orders);
            }

            //if (tempFilter.Length > 0)
            //{
            //    tempFilter.Append(" and ");
            //}

            //tempFilter.Append($"({QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'N' or {QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '0' or {QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '1' or {QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'S' or {QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '6')");

            lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            {
                LogHandler.Info($"----------this.MyOrdersDV.RowFilter-------{tempFilter}");
                this.MyOrdersDV.RowFilter = tempFilter.ToString();
            }
        }

        StringBuilder filter_cancelOrders = new StringBuilder();
        /// <summary>
        /// 处理已撤订单的过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessMyCancelOrdersFilterCondition(Dictionary<string, IList> filterDic)
        {
            filter_cancelOrders.Remove(0, filter_cancelOrders.Length);
            // 人工，IDB， 量化
            IList venuesList = filterDic["Venues"];
            StringBuilder inFilterSB_venues = new StringBuilder();
            //string algoNameFiter = string.Empty;
            if (venuesList.Count > 0)
            {
                foreach (ListBoxItem item in venuesList)
                {
                    if (item.Content.ToString() == CommonConstants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }

                    if (item.Content.ToString() == Constants.MANUAL)
                    {
                        inFilterSB_venues.Append("'MANUAL'");
                    }
                    else if (item.Content.ToString() == Constants.IDB)
                    {
                        inFilterSB_venues.Append("'IDB'");
                    }
                    else if (item.Content.ToString() == Constants.QUANT)
                    {
                        inFilterSB_venues.Append("'QUANT'");
                    }
                    else if (item.Content.ToString() == Constants.INNER)
                    {
                        inFilterSB_venues.Append("'MANUAL'");
                    }
                    else if (item.Content.ToString() == Constants.QDMESP)
                    {
                        //algoNameFiter = $"{QuantMyOrderProcesser.ALGONAME_ORDER_FIELD} in ('MM')";
                        inFilterSB_venues.Append("'QDMESP'");
                    }
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                filter_cancelOrders.Append($"{QuantMyOrderProcesser.VENEUES_ORDER_FIELD} in ({inFilterSB_venues})");
            }

            //if (!string.IsNullOrEmpty(algoNameFiter))
            //{
            //    if (filter_cancelOrders.Length > 0)
            //    {
            //        filter_cancelOrders.Append(" and ");
            //    }

            //    filter_cancelOrders.Append(algoNameFiter);
            //}

            // firm，nonfirm
            IList list = filterDic["FirmFlag"];
            StringBuilder inFilterSB_FirmFlag = new StringBuilder();
            if (list.Count > 0)
            {
                foreach (ListBoxItem item in list)
                {
                    if (item.Content.ToString() == CommonConstants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_FirmFlag.Length > 0)
                    {
                        inFilterSB_FirmFlag.Append(",");
                    }

                    if (item.Content.ToString() == Constants.FIRM)
                    {
                        inFilterSB_FirmFlag.Append("'1'");
                    }
                    else if (item.Content.ToString() == Constants.NONFIRM)
                    {
                        inFilterSB_FirmFlag.Append("'0'");
                    }
                }
            }

            if (inFilterSB_FirmFlag.Length > 0)
            {
                if (filter_cancelOrders.Length > 0)
                {
                    filter_cancelOrders.Append(" and ");
                }

                filter_cancelOrders.Append($"{QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD} in ({inFilterSB_FirmFlag})");
            }

            // 利率/信用
            IList codelist = filterDic["CodeType"];
            StringBuilder inFilterSB_Code = new StringBuilder();
            if (codelist.Count > 0)
            {
                foreach (ListBoxItem item in codelist)
                {
                    if (item.Content.ToString() == CommonConstants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_Code.Length > 0)
                    {
                        inFilterSB_Code.Append(",");
                    }

                    if (item.Content.ToString() == Constants.RATE)
                    {
                        inFilterSB_Code.Append("'BNC'");
                    }
                    else if (item.Content.ToString() == Constants.CREDIT)
                    {
                        inFilterSB_Code.Append("'BCO'");
                    }
                }
            }

            if (inFilterSB_Code.Length > 0)
            {
                if (filter_cancelOrders.Length > 0)
                {
                    filter_cancelOrders.Append(" and ");
                }

                filter_cancelOrders.Append($"{QuantMyOrderProcesser.SECURITY_TYPE} in ({inFilterSB_Code})");
            }

            FilterCancelOrderData();
        }

        /// <summary>
        /// 过滤已撤订单数据
        /// </summary>
        private void FilterCancelOrderData()
        {
            StringBuilder tempFilter = new StringBuilder();
            if (this.SearchText.Length > 0)
            {
                tempFilter.Append($"{QuantMyOrderProcesser.SECURITYID_ORDER_FIELD} like '%{this.SearchText}%'");
            }

            if (filter_cancelOrders.Length > 0)
            {
                if (tempFilter.Length > 0)
                {
                    tempFilter.Append(" and ");
                }

                tempFilter.Append(filter_cancelOrders);
            }

            //if (tempFilter.Length > 0)
            //{
            //    tempFilter.Append(" and ");
            //}

            //tempFilter.Append($"{QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '4'");

            lock (QuantMyOrderProcesser.Processer.LockObj_MyCancelOrders)
            {
                LogHandler.Info($"----------this.MyCancelOrdersDV.RowFilter-------{tempFilter}");
                this.MyCancelOrdersDV.RowFilter = tempFilter.ToString();
            }
        }

        public async void DeleteOrder()
        {
            if (this.SelectedItem_MyOrders != null 
                && this.SelectedItem_MyOrders.Row.RowState != DataRowState.Detached)
            {
                bool result = CustomMessageBox.Question("是否撤销该笔报价？", "删除订单");
                if (result == true)
                {
                    OrderCancelRequest cancelPo = new OrderCancelRequest();
                    cancelPo.AlgoName = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.ALGONAME_ORDER_FIELD]);
                    cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                    cancelPo.OrderID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.STCCODE_ORDER_FIELD]);
                    TBookCancelReplyPo reply = await CancelOrder(cancelPo);
                    if (reply != null)
                    {
                        if (reply.Code == "0000")
                        {
                            //this.SelectedItem_MyOrders[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                        }
                        else
                        {
                            CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
                        }
                    }
                }
            }
        }

        public async Task<TBookCancelReplyPo> CancelOrder(STC.Common.Model.TBook.OrderCancelRequest cancelPo)
        {
            TBookCancelReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = QuantOrdersHandler.Instance.CancelOrder(cancelPo);

            });
            return reply;
        }

        public async void DeleteAllOrders(object arg)
        {
            string selected = arg as string;

            bool result = result = CustomMessageBox.Question("是否撤销全部报价（不包括点价FAK订单和量化订单）？", "删除订单");

            if (result == true)
            {
                StringBuilder error = null;
                error = await SubDeleteAllOrders(selected);
                if (error != null && error.Length > 0)
                {
                    CustomMessageBox.Error(string.Format($"删除订单失败, 原因:{error}"));
                }
            }
        }

        private async Task<StringBuilder> SubDeleteAllOrders(string selected)
        {
            StringBuilder error = new StringBuilder();
            
            await Task.Run(() =>
            {
                List<OrderCancelRequest> cancelOrdersList = new List<OrderCancelRequest>();
                int count = 0;
                ConcurrentDictionary<string, TBookCancelReplyPo> dic = new ConcurrentDictionary<string, TBookCancelReplyPo>();

                lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    foreach (DataRow item in MyOrdersDV.Table.Rows)
                    {
                        if (((Visibility)item[QuantMyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] == Visibility.Visible && (bool)item[QuantMyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true && selected != "一键撤量化订单")
                           || (!string.IsNullOrEmpty(CommonUtils.ConvertToString(item[MyOrderProcesser.QUANTID_FIELD])) && selected == "一键撤量化订单"))
                        {
                            OrderCancelRequest cancelPo = new OrderCancelRequest();
                            cancelPo.AlgoName = CommonUtils.ConvertToString(item[QuantMyOrderProcesser.ALGONAME_ORDER_FIELD]);
                            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                            cancelPo.OrderID = CommonUtils.ConvertToString(item[QuantMyOrderProcesser.STCCODE_ORDER_FIELD]);
                            cancelOrdersList.Add(cancelPo);
                            count++;
                        }
                    }
                }

                if (cancelOrdersList != null)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        foreach (OrderCancelRequest request in cancelOrdersList)
                        {

                            if (request is OrderCancelRequest)
                            {
                                OrderCancelRequest cancelPo = request as OrderCancelRequest;
                                TBookCancelReplyPo reply = QuantOrdersHandler.Instance.CancelOrder(cancelPo);
                                dic[cancelPo.OrderID] = reply;
                            }

                            try
                            {
                                Thread.Sleep(200);
                            }
                            catch (Exception ex)
                            { }
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

                foreach(KeyValuePair<string, TBookCancelReplyPo> pair in dic)
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
                        //    item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                        //}
                    }
                }
               
            });
            return error;
        }

        private async void CancelSelectedOrders()
        {
            BatchOperateBtnEnable = false;
            StringBuilder error = await CancelSelectedOrdersAsync();
            Prompt2Seconds prompt = new Prompt2Seconds();
            if (error.Length > 0)
            {
                prompt.PromptTitle = "撤单失败";
                prompt.PromptContext = error.ToString(); ;
            }
            else
            {
                prompt.PromptTitle = "撤单成功";
                prompt.PromptContext = "撤单成功";
            }
            prompt.Show();
            prompt.Activate();
        }

        private async Task<StringBuilder> CancelSelectedOrdersAsync()
        {
            StringBuilder error = new StringBuilder();

            await Task.Run(() =>
            {
                List<OrderCancelRequest> cancelOrdersList = new List<OrderCancelRequest>();
                int count = 0;
                ConcurrentDictionary<string, TBookCancelReplyPo> dic = new ConcurrentDictionary<string, TBookCancelReplyPo>();

                lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    foreach (DataRowView item in this.MyOrdersDV)
                    {
                        if ((bool)item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == true
                            && (Visibility)item[QuantMyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] == Visibility.Visible && (bool)item[QuantMyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true)
                        {
                            OrderCancelRequest cancelPo = new OrderCancelRequest();
                            cancelPo.AlgoName = CommonUtils.ConvertToString(item[QuantMyOrderProcesser.ALGONAME_ORDER_FIELD]);
                            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                            cancelPo.OrderID = CommonUtils.ConvertToString(item[QuantMyOrderProcesser.STCCODE_ORDER_FIELD]);
                            cancelOrdersList.Add(cancelPo);
                            count++;
                        }
                    }
                }
                if (cancelOrdersList != null)
                {
                    foreach (OrderCancelRequest cancelPo in cancelOrdersList)
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                TBookCancelReplyPo reply = QuantOrdersHandler.Instance.CancelOrder(cancelPo);
                                dic[cancelPo.OrderID] = reply;

                                //Thread.Sleep(200);
                            }
                            catch (Exception e)
                            {
                                LogHandler.Error(e);
                            }
                        });
                    }
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
                        //    item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                        //}
                    }
                }
            });
            BatchOperateBtnEnable = true;
            return error;
        }

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
                    this.OnPropertyChanged();
                }
            }
        }

        private volatile bool _isCheckBoxAllClick = false; 
        public void CheckBoxAllValueChange()
        {
            bool isAllChecked = !IsAllChecked;
            _isCheckBoxAllClick = true;
            int checkedCount = 0;

            lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView item in this.MyOrdersDV)
                {
                    if (!(item[QuantMyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] is System.DBNull)
                        && (Visibility)(item[QuantMyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD]) == Visibility.Visible && (bool)item[QuantMyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true)
                    {
                        item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = isAllChecked;
                        if (isAllChecked)
                        {
                            checkedCount++;
                        }
                    }
                    else
                    {
                        item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                    }
                }
            }
            
            CheckedCount4MyOrders = checkedCount;
            if (checkedCount > 0)
            {
                IsAllChecked = true;
                IsHiddenMyOrdersOperationGrid = Visibility.Visible;
            }
            else
            {
                IsAllChecked = false;
                IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
            }
            _isCheckBoxAllClick = false;
            AdjustAllChkStatus();
        }

        public void AdjustAllChkStatus()
        {
            int checkableCount = 0;
            int checkedCount = 0;

            bool hasFirm = false;
            bool hasNonfirm = false;

            bool hasBNC = false;
            bool hasBCO = false;

            int bidCount = 0;
            int ofrCount = 0;
            int totalCount = 0;

            lock(QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView item in MyOrdersDV)
                {
                    string side = CommonUtils.ConvertToString(item[QuantMyOrderProcesser.SIDE_ORDER_FIELD]);
                    totalCount++;
                    if (side == "1")
                    {
                        bidCount++;
                    }
                    else if (side == "2")
                    {
                        ofrCount++;
                    }
                    if (!(item[QuantMyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] is System.DBNull)
                       && (Visibility)item[QuantMyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] == Visibility.Visible && (bool)item[QuantMyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true)
                    {
                        checkableCount++;
                        if (!(item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                        {
                            checkedCount++;

                            if (CommonUtils.ConvertToString(item[QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD]) == "1")
                            {
                                hasFirm = true;
                            }
                            else
                            {
                                hasNonfirm = true;
                            }

                            string securityType = CommonUtils.ConvertToString(item[QuantMyOrderProcesser.SECURITY_TYPE]);
                            if (CommonUtils.IsRate(securityType))
                            {
                                hasBNC = true;
                            }
                            else if (securityType == Consts.BCO)
                            {
                                hasBCO = true;
                            }
                        }
                    }
                }
            }
            
            this.TotalCount4MyOrders = totalCount;
            this.TotalCount4MyBidOrders = bidCount;
            this.TotalCount4MyOfrOrders = ofrCount;

            IsAllChecked = checkableCount == checkedCount && checkableCount > 0;
            CheckedCount4MyOrders = checkedCount;
            if (checkedCount > 0)
            {
                IsHiddenMyOrdersOperationGrid = Visibility.Visible;
            }
            else
            {
                IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
            }

            if (hasFirm)
            {
                ToNonfirmBtn_IsEnabled = true;
            }
            else
            {
                ToNonfirmBtn_IsEnabled = false;
            }
            if (hasNonfirm)
            {
                ToFirmBtn_IsEnabled = true;
            }
            else
            {
                ToFirmBtn_IsEnabled = false;
            }

         

            int firstCount = 0;
            int secondCount = 0;
            int directCount = 0;
            if (hasBNC && hasBCO)
            {
                firstCount = Math.Min(bncFirstCount, bcoFirstCount);
                secondCount = Math.Min(bncSecondCount, bcoSecondCount);
                directCount = Math.Min(bncDirectCount, bcoDirectCount);

                if (bncHasXBond && bcoHasXBond)
                {
                    Visibility_XBondBtn = Visibility.Visible;
                }
                else
                {
                    Visibility_XBondBtn = Visibility.Collapsed;
                }
            }
            else if (hasBNC)
            {
                firstCount = bncFirstCount;
                secondCount = bncSecondCount;
                directCount = bncDirectCount;

                if (bncHasXBond)
                {
                    Visibility_XBondBtn = Visibility.Visible;
                }
                else
                {
                    Visibility_XBondBtn = Visibility.Collapsed;
                }
            }
            else if (hasBCO)
            {
                firstCount = bcoFirstCount;
                secondCount = bcoSecondCount;
                directCount = bcoDirectCount;

                if (bcoHasXBond)
                {
                    Visibility_XBondBtn = Visibility.Visible;
                }
                else
                {
                    Visibility_XBondBtn = Visibility.Collapsed;
                }
            }

            //if (firstCount > 0)
            //{
            //    Visibility_DefaultBtn = Visibility.Visible;
            //}
            //else
            //{
            //    Visibility_DefaultBtn = Visibility.Collapsed;
            //}

            if (directCount > 0)
            {
                Visibility_Direct = Visibility.Visible;
            }
            else
            {
                Visibility_Direct = Visibility.Collapsed;
            }


            //// 添加
            //if (secondCount > RandomBtns.Count)
            //{
            //    int diff = secondCount - RandomBtns.Count;
            //    for (int i = 0; i < diff; i++)
            //    {
            //        RandomBtns.Add(new RandomBtnModel() { Content = $"默认+{RandomBtns.Count + 1}"});
            //    }
            //}
            //// 删除
            //else if (secondCount < RandomBtns.Count)
            //{
            //    int diff = RandomBtns.Count - secondCount;
            //    for (int i = 0; i < diff; i++)
            //    {
            //        RandomBtns.RemoveAt(RandomBtns.Count - 1);
            //    }
            //}
        }
        #endregion

        #region 已撤订单全选逻辑
        private bool _IsAllChecked_CancelOrders = false;
        public bool IsAllChecked_CancelOrders
        {
            get
            {
                return _IsAllChecked_CancelOrders;
            }

            set
            {
                if (_IsAllChecked_CancelOrders != value)
                {
                    _IsAllChecked_CancelOrders = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private volatile bool _isCheckBoxAllClick_CancelOrders = false;
        public void CheckBoxAllValueChange4CancelOrders()
        {
            bool isAllChecked = !IsAllChecked_CancelOrders;
            _isCheckBoxAllClick_CancelOrders = true;
            int checkTrueCount = 0;
            lock (QuantMyOrderProcesser.Processer.LockObj_MyCancelOrders)
            {
                foreach (DataRowView item in this.MyCancelOrdersDV)
                {
                    if (CommonUtils.ConvertToString(item[QuantMyOrderProcesser.VENEUES_ORDER_FIELD]) == "QUANT"
                        || CommonUtils.ConvertToString(item[QuantMyOrderProcesser.VENEUES_ORDER_FIELD]) == "QDMESP")
                    {
                        continue;
                    }
                    if (isAllChecked)
                    {
                        checkTrueCount++;
                    }
                    item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = isAllChecked;
                }
            }
            if (checkTrueCount > 0)
            {
                IsAllChecked_CancelOrders = true;
            }
            else
            {
                IsAllChecked_CancelOrders = false;
            }
            _isCheckBoxAllClick_CancelOrders = false;
            AdjustAllChkStatus4CancelOrders();
        }

        public void AdjustAllChkStatus4CancelOrders()
        {
            bool isCheckAll = true;
            int filterCount = 0;

            lock (QuantMyOrderProcesser.Processer.LockObj_MyCancelOrders)
            {
                foreach (DataRowView item in MyCancelOrdersDV)
                {
                    if (CommonUtils.ConvertToString(item[QuantMyOrderProcesser.VENEUES_ORDER_FIELD]) == "QUANT"
                        || CommonUtils.ConvertToString(item[QuantMyOrderProcesser.VENEUES_ORDER_FIELD]) == "QDMESP")
                    {
                        filterCount++;
                    }
                    else
                    {
                        if ((item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull
                        || (bool)item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == false))
                        {
                            isCheckAll = false;
                            break;
                        }
                    }
                    
                }
                IsAllChecked_CancelOrders = isCheckAll && MyCancelOrdersDV.Count > filterCount;
            }
        }
        #endregion

        private void MyOrdersDV_ListChanged(object sender, ListChangedEventArgs e)
        {
            //if (e.ListChangedType == ListChangedType.ItemAdded)
            //{
            //    this.IsAllChecked = false;
            //    this.IsAllChecked_CancelOrders = false;
            //}
            if (_isCheckBoxAllClick == true)
            {
                return;
            }

            if (e.PropertyDescriptor == null
                || (e.PropertyDescriptor != null && 
                (e.PropertyDescriptor.Name == QuantMyOrderProcesser.SIDE_ORDER_FIELD
                || e.PropertyDescriptor.Name == QuantMyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD
                || e.PropertyDescriptor.Name == QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD
                || e.PropertyDescriptor.Name == QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD)))
            {
                isAdjustAllChkStatus = true;
                //AdjustAllChkStatus();
            }
        }

        private void UncheckAll()
        {
            IsAllChecked = false;
            lock(QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView item in MyOrdersDV)
                {
                    item[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                }
            }
            IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
        }

        private void EditOrder()
        {
            if (this.SelectedItem_MyOrders == null)
            {
                return;
            }

            string strategyCodeID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.STCCODE_ORDER_FIELD]);
            if (m_windowManager.IsExist($"EditOrder-{strategyCodeID}"))
            {
                return;
            }


            Common.Model.Fix.Message.NewOrderSingle singleOrder = GetNewOrderSingle(this.SelectedItem_MyOrders);
            PriceMatchParam priceMatchParam = new PriceMatchParam();


            //string firmFlag = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
            //string orderQty = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.QTY_ORDER_FIELD]);
            //string ordersID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.STCCODE_ORDER_FIELD]);
            //string randomNum = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.VENUES_RDM_NUM_FIELD]);
            //string side = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.SIDE_ORDER_FIELD]);
            //string securityID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.SECURITYID_ORDER_FIELD]);
            //string symbolName = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.SYMBOL_ORDER_FIELD]);
            //string wholeFlag = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.WHOLE_FLAG_FIELD]);
            //string netPrice = CommonUtils.ConvertToDecimal(this.SelectedItem_MyOrders[QuantMyOrderProcesser.NETPRICE_ORDER_FIELD].ToString()).ToString("#.####");
            //string price = CommonUtils.ConvertToDecimal(this.SelectedItem_MyOrders[QuantMyOrderProcesser.PRICE_ORDER_FIELD].ToString()).ToString("#.####");
            //string settleType = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
            //string nonfirmTime = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.NONFIRM_TIME_FIELD]);
            //string crossVenues = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.CROSS_VENUES_FIELD]);
            //string venuesSelectF = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.VENUES_SELECT_F_FIELD]);
            //string selfSelectVenues = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.SELF_SELECT_VENUES_FIELD]);
            //string institutionID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.INSTITUTIONID_FIELD]);
            //string institutionName = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.INSTITUTIONNAME_FIELD]);
            //string traderID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.TRADERID_FIELD]);
            //string traderName = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[QuantMyOrderProcesser.TRADERNAME_FIELD]);

            priceMatchParam.CustomFirmAmount = singleOrder.OrderQty; // OrderQty 订单量
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

            if (singleOrder.FirmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.OldOrder = singleOrder;

            //SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);
            SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
            // bid;
            if (singleOrder.Side == "1")
            {
                //sendPriceView.OfrBtn.IsEnabled = false;
                sendPriceView.SetButtonEnable("OfrBtn", false);
            }
            // ofr
            else if (singleOrder.Side == "2")
            {
                // sendPriceView.BidBtn.IsEnabled = false;
                sendPriceView.SetButtonEnable("BidBtn", false);
            }

            if (singleOrder.FirmFlag == "1")
            {
                //sendPriceView.AmountTxt.IsEnabled = false;
                //sendPriceView.btnOne.IsEnabled = sendPriceView.btnThree.IsEnabled = sendPriceView.btnFive.IsEnabled = false;
            }

            sendPriceView.SetButtonEnable("Search_TextBox", false);
            sendPriceView.SetButtonEnable("btnXBond", false);

            //sendPriceView.datePicker.IsEnabled = false;
            //sendPriceView.cbSettle.IsEnabled = false;
            //sendPriceView.lbOne.IsEnabled = sendPriceView.lbOther.IsEnabled = false;
            //sendPriceView.lbSpotOne.IsEnabled = sendPriceView.lbSpotOther.IsEnabled = false;

            //sendPriceView.lbAddOneNotLimit.IsEnabled = sendPriceView.lbAddOneToday.IsEnabled = sendPriceView.lbAddOneTomorrow.IsEnabled = false;

            //sendPriceView.Show();
            m_windowManager.OpenOnlyOneWindow($"EditOrder-{strategyCodeID}", sendPriceView);
        }

        /// <summary>
        /// 处理idb数据
        /// </summary>
        private void ProcessIDBData()
        {
            string exeID = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.EXECID_FIELD]);
            if (m_windowManager.IsExist($"ProcessIDBData-{exeID}"))
            {
                return;
            }

            // 当前行转成ExecutionReport
            Common.Model.Fix.Message.ExecutionReport er = new Common.Model.Fix.Message.ExecutionReport();
            er.SecurityID = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.SECURITYID_FIELD]);
            er.Symbol = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.SYMBOL_FIELD]);
            er.Price = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.LASTPX_FIELD]);
            er.Side = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.SIDE_FIELD]);
            er.OrderQty = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.LASTQTY_FIELD]);
            er.TransactTime = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.EXECUTIONTIME_FIELD]);
            er.ExecID = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.EXECID_FIELD]);
            er.Venues = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.VENUES_FIELD]);
            er.CounterPartyName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.COUNTERPARTYNAME_FIELD]);
            er.CounterPartySubName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.COUNTERPARTYSUBNAME_FIELD]);
            er.OrderID = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.ORDERID_FIELD]);
            er.SettlType = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.SETTLTYPE_FIELD]); 
            er.TradeDate = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.TRADEDATE_FIELD]);

            er.TransactTime = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.EXECUTIONTIME_FIELD]);
            er.TraderName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.TRADENAME_FIELD]);
            er.InstitutionName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.INSTITUTIONNAME_FIELD]);
            
            er.Info6 = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.INFO6_FIELD]);

            er.UserName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.USERNAME_FIELD]);
            er.PartyName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.PARTYNAME_FIELD]);
            er.PartySubName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[QuantMyExecutionProcesser.PARTYSUBNAME_FIELD]);

            //ExecuteReplyView erv = new ExecuteReplyView(er);
            //erv.Owner = Application.Current.MainWindow;
            //erv.Show();
           // m_windowManager.OpenOnlyOneWindow($"ProcessIDBData-{exeID}", erv);
        }

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(DataRowView drv)
        {
            string strategyCodeID = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.STCCODE_ORDER_FIELD]);
            string securityID = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.SECURITYID_ORDER_FIELD]);
            string qty = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.QTY_ORDER_FIELD]);
            string exeqty = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.EXECQTY_ORDER_FIELD]);
            string price = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.PRICE_ORDER_FIELD]);
            string netPrice = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.NETPRICE_ORDER_FIELD]);
            string settlType = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
            string side = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.SIDE_ORDER_FIELD]);
            string wholeFlag = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.WHOLE_FLAG_FIELD]);
            string firmFlag = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
            string crossVenues = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.CROSS_VENUES_FIELD]);
            string firmToNonTime = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.NONFIRM_TIME_FIELD]);
            string randomNum = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.VENUES_RDM_NUM_FIELD]);
            string venuesSelectF = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.VENUES_SELECT_F_FIELD]);
            string selfSelectVenues = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.SELF_SELECT_VENUES_FIELD]);


            string venues = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.VENEUES_ORDER_FIELD]);
            string venueTypeGw = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.VENUETYPEGW_FIELD]);
            string algoName = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.ALGONAME_ORDER_FIELD]);
            string quantID = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.QUANTID_FIELD]);
            string timeInForce = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.TIMEINFORCE_FIELD]);
            string ordStatus = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD]);
            string bizType = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.BIZTYPE_FIELD]);
            string bizTypeID = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.BIZTYPEID_FIELD]);
            string quoteID = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.QUOTEID_FIELD]);
            string roleID = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.ROLEID_FIELD]);
            string roleName = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.ROLENAME_FIELD]);
            string symbol = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.SYMBOL_ORDER_FIELD]);
            string userId = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.USERID_FIELD]);
            string institutionID = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.INSTITUTIONID_FIELD]);
            string institutionName = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.INSTITUTIONNAME_FIELD]);
            string traderID = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.TRADERID_FIELD]);
            string traderName = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.TRADERNAME_FIELD]);
            string showTrader = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.SHOW_TRADER_FIELD]);
            string ternimal = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.TERMINAL_FIELD]);
            string info3 = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.INFO3_FIELD]);

            Common.Model.Fix.Message.NewOrderSingle sendPricePo = new Common.Model.Fix.Message.NewOrderSingle();

            // BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            sendPricePo.SecurityType = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.SECURITY_TYPE]);

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
            sendPricePo.PriceType = "3"; //收益率
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

            return sendPricePo;
        }

        private void OnToFirm()
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.ToFirm;

            lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {
                        string firmFlag = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
                        if (firmFlag == "0")
                        {
                            Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);
                            newOrderSingle.FirmFlag = "1";
                            if (newOrderSingle.Venues == "IDB")
                            {
                                newOrderSingle.Venues = "MANUAL";
                            }
                            newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                        }
                        else
                        {
                            drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                        }
                    }
                }
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    InitStatistic();

                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        QuantOrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void OnToNonFirm()
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.ToNonFirm;

            lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            { 
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {
                        string firmFlag = CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
                        if (firmFlag == "1")
                        {
                            Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);
                            newOrderSingle.FirmFlag = "0";
                            if (newOrderSingle.Venues == "IDB")
                            {
                                newOrderSingle.Venues = "MANUAL";
                            }
                            newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                        }
                        else
                        {
                            drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                        }
                    }
                }
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    InitStatistic();

                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        QuantOrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void OpenOneSendPriceView(Common.Model.Fix.Message.NewOrderSingle singleOrder, string strategyCodeID)
        {
            PriceMatchParam priceMatchParam = new PriceMatchParam();

            priceMatchParam.CustomFirmAmount = singleOrder.OrderQty; // OrderQty 订单量
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

            if (singleOrder.FirmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.UpdataPrice = false;
            //SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);
            SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
            // bid;
            if (singleOrder.Side == "1")
            {
                //sendPriceView.OfrBtn.IsEnabled = false;
                sendPriceView.SetButtonEnable("OfrBtn", false);
            }
            // ofr
            else if (singleOrder.Side == "2")
            {
                //sendPriceView.BidBtn.IsEnabled = false;
                sendPriceView.SetButtonEnable("BidBtn", false);
            }

            if (singleOrder.FirmFlag == "1")
            {
                //sendPriceView.Search_TextBox.IsEnabled = false;
                sendPriceView.SetButtonEnable("Search_TextBox", false);
                //sendPriceView.AmountTxt.IsEnabled = false;
                //sendPriceView.SettlTypeListBox.IsEnabled = false;
            }
            else
            {
                //sendPriceView.Search_TextBox.IsEnabled = false;
                sendPriceView.SetButtonEnable("Search_TextBox", false);
                //sendPriceView.SettlTypeListBox.IsEnabled = false;
            }

            //sendPriceView.lbOne.IsEnabled = sendPriceView.lbOther.IsEnabled = false;
            //sendPriceView.lbSpotOne.IsEnabled = sendPriceView.lbSpotOther.IsEnabled = false;

            //sendPriceView.Show();
            m_windowManager.OpenOnlyOneWindow($"ResumeOrder-{strategyCodeID}", sendPriceView);
        }

        private void ResumeOrder()
        {
            if (this.SelectedItem_MyCancelOrders == null)
            {
                return;
            }

            string strategyCodeID = CommonUtils.ConvertToString(this.SelectedItem_MyCancelOrders[QuantMyOrderProcesser.STCCODE_ORDER_FIELD]);
            if (m_windowManager.IsExist($"ResumeOrder-{strategyCodeID}"))
            {
                return;
            }

            Common.Model.Fix.Message.NewOrderSingle singleOrder = GetNewOrderSingle(this.SelectedItem_MyCancelOrders);


            this.OpenOneSendPriceView(singleOrder, strategyCodeID);
        }

        private async void ResendOrders()
        {
            //if (m_windowManager.IsExist("ResendOrders"))
            //{
            //    return;
            //}

            IsEnabled_Resend = false;
            Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic = await Sub_ResendOrders();
            if (newOrderSingleDic.Count == 0)
            {
                CustomMessageBox.Info("请先选中需恢复报价的记录!", "恢复报价");
                IsEnabled_Resend = true;
                return;
            }
            else if (newOrderSingleDic.Count == 1)
            {
                Common.Model.Fix.Message.NewOrderSingle newOrderSingle = newOrderSingleDic.Values.ToList()[0];
                this.OpenOneSendPriceView(newOrderSingle, newOrderSingle.StrategyOrderId);
                IsEnabled_Resend = true;
                return;
            }

            //BatchResendView convertFirmOrNonFirmView = new BatchResendView();
            //convertFirmOrNonFirmView.Init(newOrderSingleDic);
            ////convertFirmOrNonFirmView.Show();
            ////m_windowManager.OpenOnlyOneWindow("ResendOrders", convertFirmOrNonFirmView);
            //convertFirmOrNonFirmView.Owner = Application.Current.MainWindow;
            //convertFirmOrNonFirmView.ShowDialog();
            IsEnabled_Resend = true;
        }

        private async Task<Dictionary<string, Common.Model.Fix.Message.NewOrderSingle>> Sub_ResendOrders()
        {
            Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic = new Dictionary<string, Common.Model.Fix.Message.NewOrderSingle>();
            await Task.Run(() =>
            {
                
                lock (QuantMyOrderProcesser.Processer.LockObj_MyCancelOrders)
                {
                    foreach (DataRowView drv in this.MyCancelOrdersDV)
                    {
                        if (!(drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                            && (bool)drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == true
                             && CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.VENEUES_ORDER_FIELD]) != "QUANT"
                        && CommonUtils.ConvertToString(drv[QuantMyOrderProcesser.VENEUES_ORDER_FIELD]) != "QDMESP")
                        {
                            Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);
                            if (newOrderSingle.Venues == "IDB")
                            {
                                newOrderSingle.Venues = "MANUAL";
                            }
                            newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                        }
                    }
                }
            });
            return newOrderSingleDic;
        }

        private long opsCount = 0;
        private volatile int failCount = 0;
        private BatchOperateTypeEnum batchOpsTypeEnum;
        private void EditPrice(string parameter)
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.EditPrice;

            lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {

                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);
                        modifyPrice(newOrderSingle, parameter);

                        if (newOrderSingle.Venues == "IDB")
                        {
                            newOrderSingle.Venues = "MANUAL";
                        }
                        newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    InitStatistic();

                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        ThreadPool.QueueUserWorkItem(o => {
                            string securityID = sendPricePo.SecurityID;
                            string settlType = sendPricePo.SettlType;
                            string price = sendPricePo.Price;
                            decimal netPrice = CalBond(securityID, "4", settlType, NumChange(price));
                            sendPricePo.NetPrice = netPrice.ToString();
                            QuantOrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                        });
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        /// <summary>
        /// 初始化统计值
        /// </summary>
        private void InitStatistic()
        {
            Interlocked.Exchange(ref opsCount, newOrderSingleDic.Count);
            failCount = 0;
        }

        private void UpdatePriceCallBack(QuantModifyOrdersReply reply)
        {
            Interlocked.Decrement(ref opsCount);
            if (newOrderSingleDic != null)
            {
                if (reply.IsSuccess() == false)
                {
                    failCount++;
                    if (reply.Code == "10000")
                    {
                        

                        try
                        {
                            bool isProhibitory = false;
                            List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(reply.Msg);
                            foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                            {
                                if (placeOrderResult.Code == "10002")
                                {
                                    isProhibitory = true;
                                    break;
                                }
                            }

                            if (isProhibitory)
                            {
                                prohibitReplyDic[reply.OrderID] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[reply.OrderID] = orderRisks;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }
                    }
                }
            }

            if (Interlocked.Read(ref opsCount) == 0)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    BatchOperateBtnEnable = true;
                    int totalCount = newOrderSingleDic.Count;
  
                    if (newOrderSingleDic.Count > 0 && (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0))
                    {
                        //BatchResultWindow batchResultWindow = new BatchResultWindow(newOrderSingleDic,
                        //    warnReplyDic, prohibitReplyDic, failCount);
                        //batchResultWindow.Owner = Application.Current.MainWindow;
                        //batchResultWindow.ShowDialog();
                    }
                    else
                    {
                        string text = $"共执行 {totalCount} 条, 其中 {totalCount - failCount} 条成功";

                        Prompt2Seconds prompt = new Prompt2Seconds();
                        if (batchOpsTypeEnum == BatchOperateTypeEnum.EditQty)
                        {
                            if (failCount == 0)
                            {
                                prompt.PromptTitle = prompt.PromptContext = "订单已更新";
                            }
                            else
                            {
                                prompt.PromptTitle = "批量编辑报量";
                                prompt.PromptContext = text;
                            }
                            
                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.EditPrice)
                        {
                            if (failCount == 0)
                            {
                                prompt.PromptTitle = prompt.PromptContext = "订单已更新";
                            }
                            else
                            {
                                prompt.PromptTitle = "批量编辑报价";
                                prompt.PromptContext = text;
                            }
                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.ToFirm)
                        {
                            if (failCount == 0)
                            {
                                prompt.PromptTitle = prompt.PromptContext = "更新为firm订单";
                            }
                            else
                            {
                                prompt.PromptTitle = "批量转Firm";
                                prompt.PromptContext = text;
                            }
                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.ToNonFirm)
                        {
                            if (failCount == 0)
                            {
                                prompt.PromptTitle = prompt.PromptContext = "更新为nonfirm订单";
                            }
                            else
                            {
                                prompt.PromptTitle = "批量转NonFirm";
                                prompt.PromptContext = text;
                            }
                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.EditVenues)
                        {
                            if (failCount == 0)
                            {
                                prompt.PromptTitle = prompt.PromptContext = "渠道已更新";
                            }
                            else
                            {
                                prompt.PromptTitle = "批量编辑渠道";
                                prompt.PromptContext = text;
                            }
                        }

                        prompt.Owner = Application.Current.MainWindow;
                        prompt.Show();
                    }
                }));
            }
        }


        private string NumChange(string price)
        {
            decimal d = 0;
            if (decimal.TryParse(price, out d))
            {
                d = d / 100;
                price = d.ToString();
            }

            return price;
        }

        public decimal CalBond(string securityID, string calType, string settleType, string val)
        {
            try
            {
                CalReq requst = new CalReq();
                requst.SecurityID = securityID;
                requst.CalType = calType;//"4";
                requst.SettlType = settleType;// "1";
                requst.Value = val;// customPrice;

                CalResult result = CalHandler.Instance.CalBond(requst);
                if (requst != null && result.Data != null)
                {
                    if (calType.Equals("4"))
                    {
                        //NetPrice = result.Data.CleanPrice.ToString("N4");
                        return result.Data.CleanPrice;
                    }
                    else if (calType.Equals("2"))
                    {
                        //CustomPrice = (result.Data.Yield * 100).ToString("N4");
                        return result.Data.Yield;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return decimal.Zero;
        }


        private void modifyPrice(Common.Model.Fix.Message.NewOrderSingle newOrderSingle, string parameter)
        {
            decimal price = CommonUtils.ConvertToDecimal(newOrderSingle.Price);
            if (parameter == "+1")
            {
                price += 0.01m;
            }
            else if (parameter == "+0.5")
            {
                price += 0.005m;
            }
            else if (parameter == "+0.25")
            {
                price += 0.0025m;
            }
            else if (parameter == "-0.25")
            {
                price -= 0.0025m;
            }
            else if (parameter == "-0.5")
            {
                price -= 0.005m;
            }
            else if (parameter == "-1")
            {
                price -= 0.01m;
            }
            newOrderSingle.Price = price.ToString();
        }

        private void EditQty(string parameter)
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.EditQty;

            lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);
                        modifyQty(newOrderSingle, parameter);
                        int orderQty = CommonUtils.ConvertToInt(newOrderSingle.OrderQty);
                        if (orderQty > 1000000)
                        {
                            drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                        }
                        if (newOrderSingle.Venues == "IDB")
                        {
                            newOrderSingle.Venues = "MANUAL";
                        }
                        newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    InitStatistic();

                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        int orderQty = CommonUtils.ConvertToInt(sendPricePo.OrderQty);
                        if (orderQty <= 0)
                        {
                            OrderCancelRequest cancelPo = new OrderCancelRequest();
                            cancelPo.AlgoName = sendPricePo.AlgoName;
                            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                            cancelPo.OrderID = sendPricePo.StrategyOrderId;
                            TBookCancelReplyPo result = QuantOrdersHandler.Instance.CancelOrder(cancelPo);

                            QuantModifyOrdersReply reply = new QuantModifyOrdersReply();
                            if (result.IsSuccess())
                            {
                                reply.Code = "0000";
                                reply.Msg = "撤单成功";
                            }
                            else
                            {
                                reply.Code = "0001";
                                reply.Msg = "撤单失败";
                            }
                            
                            reply.OrderID = sendPricePo.StrategyOrderId;
                            UpdatePriceCallBack(reply);
                        }
                        else if (orderQty > 1000000)
                        {
                            QuantModifyOrdersReply reply = new QuantModifyOrdersReply();
                            reply.Code = "0001";
                            reply.Msg = "报量超出最大上限1000000万";
                            reply.OrderID = sendPricePo.StrategyOrderId;
                            UpdatePriceCallBack(reply);
                        }
                        else
                        {
                            QuantOrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                        }
                        
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });

        }

        private void modifyQty(Common.Model.Fix.Message.NewOrderSingle newOrderSingle, string parameter)
        {
            int qty = CommonUtils.ConvertToInt(newOrderSingle.OrderQty);
            if (parameter == "+5000")
            {
                qty += 5000;
            }
            else if (parameter == "+1000")
            {
                qty += 1000;
            }
            else if (parameter == "-5000")
            {
                qty -= 5000;
            }
            else if (parameter == "-1000")
            {
                qty -= 1000;
            }
            else if (parameter == "5K")
            {
                qty = 5000;
            }
            else if (parameter == "1E")
            {
                qty = 10000;
            }

            newOrderSingle.OrderQty = qty.ToString();
        }
    
        private void EditVenues(string parameter)
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.EditQty;

            lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);

                        modifyVenues(newOrderSingle, parameter);


                        if (newOrderSingle.Venues == "IDB")
                        {
                            newOrderSingle.Venues = "MANUAL";
                        }
                        newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    InitStatistic();

                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        QuantOrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void modifyVenues(Common.Model.Fix.Message.NewOrderSingle newOrderSingle, string parameter)
        {
            if (parameter == "仅内部")
            {
                newOrderSingle.CrossVenues = "2";
                newOrderSingle.VenuesSelectF = string.Empty;
                newOrderSingle.SelfSelectVenues = null;
                newOrderSingle.VenuesRdmNum = null;
            }
            if (parameter == "XBOND")
            {
                newOrderSingle.CrossVenues = "3";
                newOrderSingle.VenuesSelectF = "3";
                newOrderSingle.SelfSelectVenues = "ODM-ODM";
                newOrderSingle.VenuesRdmNum = null;
            }
            else if (parameter == "默认")
            {
                newOrderSingle.CrossVenues = "1";
                newOrderSingle.VenuesSelectF = "2";
                newOrderSingle.SelfSelectVenues = string.Empty;
                newOrderSingle.VenuesRdmNum = null;
            }
            else if (parameter.Contains("默认+"))
            {

                newOrderSingle.CrossVenues = "1";
                newOrderSingle.VenuesSelectF = "2";
                newOrderSingle.SelfSelectVenues = string.Empty;
                newOrderSingle.VenuesRdmNum = parameter.Substring(3);
            }
        }

        //private void AssignVenues()
        //{
        //    BatchOperateBtnEnable = false;
        //    newOrderSingleDic.Clear();
        //    warnReplyDic.Clear();
        //    prohibitReplyDic.Clear();
        //    batchOpsTypeEnum = BatchOperateTypeEnum.EditVenues;

        //    bool hasBNC = false;
        //    bool hasBCO = false;

        //    lock (QuantMyOrderProcesser.Processer.LockObj_MyOrders)
        //    {
        //        foreach (DataRowView drv in this.MyOrdersDV)
        //        {
        //            if (!(drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
        //                && (bool)drv[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
        //            {
        //                Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);

        //                if (CommonUtils.IsRate(newOrderSingle.SecurityType))
        //                {
        //                    hasBNC = true;
        //                }
        //                else
        //                {
        //                    hasBCO = true;
        //                }
                        
        //                if (newOrderSingle.Venues == "IDB")
        //                {
        //                    newOrderSingle.Venues = "MANUAL";
        //                }
        //                newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
        //            }
        //        }
        //    }

        //    if (newOrderSingleDic.Count == 0)
        //    {
        //        BatchOperateBtnEnable = true;
        //        return;
        //    }

        //    AssignVenuesWindow win = new AssignVenuesWindow();
        //    AssignVenuesWindowViewModel viewModel = win.DataContext as AssignVenuesWindowViewModel;
        //    if (hasBNC) viewModel.IsShowInterestRateDebt = Visibility.Visible;
        //    if (hasBCO) viewModel.IsShowCreditDebt = Visibility.Visible;
        //    bool? result = win.ShowDialog();

        //    string bncSelfSelectVenues = win.InterestRateDebt.SelfSelectVenues;
        //    string bcoSelfSelectsVenues = win.CreditDebt.SelfSelectVenues;

        //    if (result == false)
        //    {
        //        BatchOperateBtnEnable = true;
        //        return;
        //    }

        //    Task.Run(() =>
        //    {
        //        try
        //        {
        //            InitStatistic();

        //            foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
        //            {
        //                sendPricePo.CrossVenues = "1";
        //                sendPricePo.VenuesSelectF = "3";
        //                sendPricePo.VenuesRdmNum = null;
        //                if (CommonUtils.IsRate(sendPricePo.SecurityType))
        //                {
        //                    sendPricePo.SelfSelectVenues = bncSelfSelectVenues;
        //                }
        //                else
        //                {
        //                    sendPricePo.SelfSelectVenues = bcoSelfSelectsVenues;
        //                }
                        
        //                QuantOrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            LogHelper.Error(e);
        //        }
        //    });

            
        //}

        /// <summary>
        /// 打开历史查询界面
        /// </summary>
        //public void OpenHistoryPage(bool isExecution)
        //{
        //    // 使用插件形式打开历史页面
        //    int tradeBookPluginKey = (int)PluginKeyEnum.HistoryPlugin;

        //    IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(tradeBookPluginKey);
        //    if (plugin != null)
        //    {
        //        bool isExist = TabPageHelper.IsExistTabItem(plugin.Description);
        //        if (isExist == false)
        //        {
        //            AppUtils.Application.PluginManager.EnablePlugin(tradeBookPluginKey);
        //        }


        //        if (AppUtils.Application.Panel.SelectedItem != null
        //        && AppUtils.Application.Panel.SelectedItem is TabItem)
        //        {
        //            TabItem tabItem = AppUtils.Application.Panel.SelectedItem as TabItem;
        //            if (tabItem.Content is HistoryView)
        //            {

        //                HistoryView historyView = tabItem.Content as HistoryView;
        //                if (isExecution)
        //                {
        //                    historyView.SelectDealView();
        //                }
        //                else
        //                {
        //                    historyView.SelectOrderView();
        //                }
        //            }
        //        }
        //    }
        //}

        public string CopySTCCodeText(IList SelectedItemsList,string tabName)
        {
            StringBuilder sb = new StringBuilder();
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (DataRowView dgr in SelectedItemsList)
                {
                    if (tabName.Equals("Orders"))
                    {
                        string stcCodeText = dgr[QuantMyOrderProcesser.STCCODE_ORDER_FIELD] as string;
                        sb.Append(stcCodeText + "\n");
                    }
                    else if (tabName.Equals("Deals"))
                    {
                        string stcCodeText = dgr[QuantMyExecutionProcesser.STRATEGY_ORDER_ID_FIELD] as string;
                        sb.Append(stcCodeText + "\n");
                    }
                    else if (tabName.Equals("Cancels"))
                    {
                        string stcCodeText = dgr[QuantMyOrderProcesser.STCCODE_ORDER_FIELD] as string;
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
                foreach (DataRowView dgr in SelectedItemsList)
                {
                    if (tabName.Equals("Orders"))
                    {
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.SIDE_ORDER_FIELD] as string, "side") + "\t");
                        sb.Append(dgr[QuantMyOrderProcesser.SECURITYID_ORDER_FIELD] as string + "\t");
                        sb.Append(dgr[QuantMyOrderProcesser.SYMBOL_ORDER_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string , "settletype") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD] as string, "firmflag") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.ORDERSTATUS_ORDER_FIELD] as string, "ordstatus") + "\t");
                        //sb.Append(dgr[QuantMyOrderProcesser.ORDERTYPE_FIELD] as string + "\t");
                        sb.Append(string.Format("{0:N4}", dgr[QuantMyOrderProcesser.PRICE_ORDER_FIELD]) + "%\t");
                        //sb.Append(this.Covert(dgr[QuantMyOrderProcesser.WHOLE_FLAG_FIELD] , "wholeflag") + "\t");
                        sb.Append(dgr[QuantMyOrderProcesser.QTY_ORDER_FIELD] + "w\t");
                        //sb.Append(this.NumCovert(dgr[QuantMyOrderProcesser.EXECQTY_ORDER_FIELD]) + "\t");
                        //sb.Append(Constants.SHANXIZHENGQUAN + "\t");//dgr[QuantMyOrderProcesser.MYDEALER_ORDER_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venues") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.CROSS_VENUES_FIELD] as string, "CrossVenues") + "\t");
                        //sb.Append(dgr[QuantMyOrderProcesser.VENUES_SELECT_DISPLAY_FIELD] as string + "\t");
                        //sb.Append(dgr[QuantMyOrderProcesser.STCCODE_ORDER_FIELD] as string + "\t");
                        //sb.Append(TimeUtils.Instance.ToTime(dgr[QuantMyOrderProcesser.UPDATETIME_ORDER_FIELD] as string) + "\t");

                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string, "settletype_myorder"));

                        sb.Append("\n");
                    }
                    else if (tabName.Equals("Deals"))
                    {
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyExecutionProcesser.SIDE_FIELD] as string, "side") + "\t");
                        sb.Append(dgr[QuantMyExecutionProcesser.SECURITYID_FIELD] as string + "\t");
                        sb.Append(dgr[QuantMyExecutionProcesser.SYMBOL_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyExecutionProcesser.SYNC_STATUS_FIELD] as string, "synchflag") + "\t");
                        sb.Append(string.Format("{0:N4}%", dgr[QuantMyExecutionProcesser.DISPLAY_LASTPX_FIELD]) + "\t");
                        //sb.Append(this.NumCovert(dgr[QuantMyExecutionProcesser.NETPRICE_FIELD]) + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyExecutionProcesser.SETTLTYPE_FIELD] as string, "settletype") + "\t");
                        sb.Append(dgr[QuantMyExecutionProcesser.LASTQTY_FIELD] + "w\t");

                        string side = dgr[QuantMyExecutionProcesser.SIDE_FIELD] as string;// ComboSourceUtil.Instance.FindEnum(dgr[QuantMyExecutionProcesser.SIDE_FIELD] as string, "side");

                        if (side.Equals("1")) 
                        {
                            sb.Append(this.StringConvert(dgr[QuantMyExecutionProcesser.COUNTERPARTYNAME_FIELD]) + "\tto\t");
                            //sb.Append(Constants.SHANXIZHENGQUAN + "\t");
                            //sb.Append(this.StringConvert(dgr[QuantMyExecutionProcesser.PARTYNAME_FIELD]) + "\t");
                        }
                        else
                        {
                            //sb.Append(this.StringConvert(dgr[QuantMyExecutionProcesser.PARTYNAME_FIELD]) + "\tto\t");
                            //sb.Append(Constants.SHANXIZHENGQUAN + "\tto\t");
                            sb.Append(this.StringConvert(dgr[QuantMyExecutionProcesser.COUNTERPARTYNAME_FIELD]) + "\t");
                        }

                        //sb.Append(this.StringConvert(dgr[QuantMyExecutionProcesser.COUNTERPARTYSUBNAME_FIELD]) + "\t");
                        //sb.Append(dgr[QuantMyExecutionProcesser.EXECID_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyExecutionProcesser.VENUES_FIELD] as string, "venues_exe") + "\t");
                        //sb.Append(dgr[QuantMyExecutionProcesser.PARTYSUBNAME_FIELD] as string + "\t");
                        //sb.Append(TimeUtils.Instance.ToTime(dgr[QuantMyExecutionProcesser.EXECUTIONTIME_FIELD] as string) + "\t");
                        //sb.Append(dgr[QuantMyExecutionProcesser.STRATEGY_ORDER_ID_FIELD] as string + "\t");
                        //sb.Append(this.StringConvert(dgr[QuantMyExecutionProcesser.REFORDERID_FIELD]) + "\t");     
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyExecutionProcesser.SETTLTYPE_FIELD] as string, "settletype") + "\t");
                        sb.Append("\n");
                    }
                    else if (tabName.Equals("Cancels"))
                    {
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.SIDE_ORDER_FIELD] as string, "side") + "\t");
                        sb.Append(dgr[QuantMyOrderProcesser.SECURITYID_ORDER_FIELD] as string + "\t");
                        sb.Append(dgr[QuantMyOrderProcesser.SYMBOL_ORDER_FIELD] as string + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string, "settletype") + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.FIRMFLAG_ORDER_FIELD] as string, "firmflag") + "\t");
                        sb.Append(string.Format("{0:N4}", dgr[QuantMyOrderProcesser.PRICE_ORDER_FIELD]) + "\t");
                        sb.Append(this.Covert(dgr[QuantMyOrderProcesser.WHOLE_FLAG_FIELD], "wholeflag") + "\t");
                        sb.Append(dgr[QuantMyOrderProcesser.QTY_ORDER_FIELD] + "\t");
                        sb.Append(this.NumCovert(dgr[QuantMyOrderProcesser.EXECQTY_ORDER_FIELD] )+ "\t");
                        sb.Append(dgr[QuantMyOrderProcesser.PARTYSUBNAME_FIELD] as string + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venues") + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[QuantMyOrderProcesser.CROSS_VENUES_FIELD] as string, "CrossVenues") + "\t");
                        sb.Append(dgr[QuantMyOrderProcesser.VENUES_SELECT_DISPLAY_FIELD] as string + "\t");
                        sb.Append(dgr[QuantMyOrderProcesser.STCCODE_ORDER_FIELD] as string + "\t");
                        sb.Append(TimeUtils.Instance.ToTime(dgr[QuantMyOrderProcesser.UPDATETIME_ORDER_FIELD] as string) + "\t");
                        sb.Append("\n");
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 双击选中当前行
        /// </summary>
        /// <param name="SelectedItemsList"></param>
        public void SetCurrentSelected(IList SelectedItemsList)
        {
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (DataRowView dgr in SelectedItemsList)
                {
                    dgr[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD] = dgr[QuantMyOrderProcesser.ISCHECKED_ORDER_FIELD].ToString().ToLower() == "true" ? false : true;
                }
            }
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
    }
}
