using Ats.Foundation.Message;
using com.gateway.log;
using Microsoft.Expression.Interactivity.Core;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.OwnExec;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.TBook;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.TBook;
using STC.Common.Model.WorkTable;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Component;
using STC.Common.View.MyTrading;
using STC.Common.View.Order;
using STC.Common.View.WorkTable;
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

namespace STC.Common.ViewModel.MyTrading
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
    public class MyOrdersViewModel : ViewModelBase
    {
        WindowManager m_windowManager = WindowManager.Instance;//new WindowManager();

        public UserControl myWindow;
        /// <summary>
        /// 定时刷新改变checkbox之后， 界面的变化情况
        /// </summary>
        Thread refreshCheckBoxThread = null;

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
                lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    if (_MyOrdersDV == null)
                    {
                        _MyOrdersDV = new DataView(MyOrderProcesser.Processer.MyOrdersDT);
                        //_MyOrdersDV.RowFilter = $"({MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'N' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '0' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '1' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'S' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '6')";
                        _MyOrdersDV.Sort = $"{MyOrderProcesser.UPDATETIME_ORDER_FIELD} desc";
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
                this.OnPropertyChanged();
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
        private string totalCountOrderHeader = "我的订单";
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


        #region 渠道权限和交易权限
        private readonly string XBOND = "XBOND";
        /// <summary>
        /// 渠道权限
        /// </summary>
        private readonly string MODULEID = "1002";
        /// <summary>
        /// 所有权限
        /// </summary>
        private readonly string CHECK_ALL_ACTIONID = "100203";
        /// <summary>
        /// 仅内部
        /// </summary>
        private readonly string CHECK_INNER_ACTIONID = "100201";
        /// <summary>
        /// 内部，X-Bond
        /// </summary>
        private readonly string CHECK_INNER_XBOND_ACTIONID = "100202";

        /// 是否有内部撮合权限
        public bool IsHaveVenueInnerPermission = false;

        /// <summary>
        /// 渠道权限-仅内部
        /// </summary>
        private bool IsHaveInnerPermission = false;

        /// <summary>
        /// 渠道权限-内部，X-Bond
        /// </summary>
        private bool IsHaveInnerXbondPermission = false;

        /// <summary>
        /// 渠道权限-所有权限
        /// </summary>
        private bool IsHaveAllPermission = false;

        /// <summary>
        /// 仅内部渠道按钮是否可见
        /// </summary>
        private Visibility _Visibility_InnerBtn = Visibility.Visible;

        public Visibility Visibility_InnerBtn
        {
            get
            {
                return _Visibility_InnerBtn;
            }
            set
            {
                if (_Visibility_InnerBtn != value)
                {
                    _Visibility_InnerBtn = value;
                    this.OnPropertyChanged(nameof(Visibility_InnerBtn));
                }
            }
        }

        #endregion

        public string TradeTabSelectedItem = "";
        /// <summary>
        /// 构造函数
        /// </summary>
        public MyOrdersViewModel()
        {
            try
            {
                Thread refreshCheckBoxThread = new Thread(RefreshCheckBox);
                refreshCheckBoxThread.IsBackground = true;
                refreshCheckBoxThread.Start();

                DelOrderCommand = new ComRelayCommand(DeleteOrder);

                CancelAllOrdersCommand = new ActionCommand(DeleteAllOrders);

                CancelSelectedOrdersCommand = new ComRelayCommand(CancelSelectedOrders);

                UncheckAllCommand = new ComRelayCommand(UncheckAll);

                EditOrderCommand = new ComRelayCommand(EditOrder);

                ToNonfirmCommand = new ComRelayCommand(OnToNonFirm);

                ToFirmCommand = new ComRelayCommand(OnToFirm);

                EditPriceCommand = new ComRelayCommand<string>(EditPrice);

                EditQtyCommand = new ComRelayCommand<string>(EditQty);

                EditVenuesCommand = new ComRelayCommand<string>(EditVenues);

                AssignVenuesCommand = new ComRelayCommand(AssignVenues);

                // 我的订单
                BindingOperations.EnableCollectionSynchronization(MyOrdersDV, MyOrderProcesser.Processer.LockObj_MyOrders);
                this.MyOrdersDV.ListChanged += MyOrdersDV_ListChanged;
                

                // 是否有内部交易权限
                //IsHaveVenueInnerPermission = GetIsHaveInner(goodsType);
                // 获取渠道权限
                IsHaveInnerPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);
                IsHaveInnerXbondPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);
                IsHaveAllPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
                //IsHaveVenueXbondPermission = CheckVenuePermission(XBOND);
                //bool isShowXBond = (IsHaveAllPermission || IsHaveInnerXbondPermission) && IsHaveVenueXbondPermission;

                isAdjustAllChkStatus = true;

                List<Venue> bncFirstList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BNC, "1");
                List<Venue> bncSecondList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BNC, "2");
                List<Venue> bncDirectList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BNC, string.Empty);
                bncFirstCount = bncFirstList != null ? bncFirstList.Count : 0;
                bncSecondCount = bncSecondList != null ? bncSecondList.Count : 0;
                bncDirectCount = bncDirectList != null ? bncDirectList.Count : 0;
                if (bncDirectList != null)
                {
                    if (IsHaveInnerXbondPermission || IsHaveAllPermission)
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
                }

                List<Venue> bcoFirstList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BCO, "1");
                List<Venue> bcoSecondList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BCO, "2");
                List<Venue> bcoDirectList = SystemParamHandler.Instance.GetListVenueByCondition(Consts.BCO, string.Empty);
                bcoFirstCount = bcoFirstList != null ? bcoFirstList.Count : 0;
                bcoSecondCount = bcoSecondList != null ? bcoSecondList.Count : 0;
                bcoDirectCount = bcoDirectList != null ? bcoDirectList.Count : 0;
                if (bncDirectList != null)
                {
                    if (IsHaveInnerXbondPermission || IsHaveAllPermission)
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

                // 没有任何权限，仅内部不可见
                if ((!IsHaveInnerPermission) && (!IsHaveInnerXbondPermission) && (!IsHaveAllPermission))
                {
                    Visibility_InnerBtn = Visibility.Collapsed;
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }


        private void RefreshCheckBox()
        {
            try
            {
                while (true)
                {
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
            catch (ThreadAbortException threadAbortException)
            {
                // ignore.
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        ~MyOrdersViewModel()
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
            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                if (_MyOrdersDV != null)
                {
                    _MyOrdersDV.Dispose();
                    _MyOrdersDV = null;
                }
            }
            base.Dispose();
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

                    if (item.Content.ToString() == CommonConstants.MANUAL)
                    {
                        inFilterSB_venues.Append("'MANUAL'");
                    }
                    else if (item.Content.ToString() == CommonConstants.IDB)
                    {
                        inFilterSB_venues.Append("'IDB'");
                    }
                    else if (item.Content.ToString() == CommonConstants.QUANT)
                    {
                        inFilterSB_venues.Append("'QUANT'");
                    }
                    else if (item.Content.ToString() == CommonConstants.INNER)
                    {
                        inFilterSB_venues.Append("'MANUAL'");
                    }
                    else if (item.Content.ToString() == CommonConstants.QDMESP)
                    {
                        //algoNameFiter = $"{MyOrderProcesser.ALGONAME_ORDER_FIELD} in ('MM')";
                        inFilterSB_venues.Append("'QDMESP'");
                    }
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                filter_orders.Append($"{MyOrderProcesser.VENEUES_ORDER_FIELD} in ({inFilterSB_venues})");
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

                    if (item.Content.ToString() == CommonConstants.FIRM)
                    {
                        inFilterSB_FirmFlag.Append("'1'");
                    }
                    else if (item.Content.ToString() == CommonConstants.NONFIRM)
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

                filter_orders.Append($"{MyOrderProcesser.FIRMFLAG_ORDER_FIELD} in ({inFilterSB_FirmFlag})");
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

                    if (item.Content.ToString() == CommonConstants.RATE)
                    {
                        inFilterSB_Code.Append("'BNC'");
                    }
                    else if (item.Content.ToString() == CommonConstants.CREDIT)
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

                filter_orders.Append($"{MyOrderProcesser.SECURITY_TYPE} in ({inFilterSB_Code})");
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
                tempFilter.Append($"({MyOrderProcesser.SECURITYID_ORDER_FIELD} like '%{this.SearchText}%' or {MyOrderProcesser.SYMBOL_ORDER_FIELD} like '%{this.SearchText}%')");
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

            //tempFilter.Append($"({MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'N' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '0' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '1' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'S' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '6')");

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                LogHandler.Info($"----------this.MyOrdersDV.RowFilter-------{tempFilter}");
                this.MyOrdersDV.RowFilter = tempFilter.ToString();
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
                    cancelPo.AlgoName = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                    cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                    cancelPo.OrderID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.STCCODE_ORDER_FIELD]);
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
        }

        public async Task<TBookCancelReplyPo> CancelOrder(STC.Common.Model.TBook.OrderCancelRequest cancelPo)
        {
            TBookCancelReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = OrdersHandler.Instance.CancelOrder(cancelPo);

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

                lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    foreach (DataRow item in MyOrdersDV.Table.Rows)
                    {
                        if (((Visibility)item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] == Visibility.Visible && (bool)item[MyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true && selected != "一键撤量化订单")
                           || (CommonUtils.ConvertToString(item[MyOrderProcesser.VENEUES_ORDER_FIELD]) == "QUANT" && selected == "一键撤量化订单"))
                        {
                            OrderCancelRequest cancelPo = new OrderCancelRequest();
                            cancelPo.AlgoName = CommonUtils.ConvertToString(item[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                            cancelPo.OrderID = CommonUtils.ConvertToString(item[MyOrderProcesser.STCCODE_ORDER_FIELD]);
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
                                TBookCancelReplyPo reply = OrdersHandler.Instance.CancelOrder(cancelPo);
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

        private async void CancelSelectedOrders()
        {
            BatchOperateBtnEnable = false;
            StringBuilder error = await CancelSelectedOrdersAsync();
            Prompt2Seconds prompt = new Prompt2Seconds();
            if (error.Length > 0)
            {
                prompt.tbTitle.Text = "撤单失败";
                prompt.tbRefer.Text = error.ToString(); ;
            }
            else
            {
                prompt.tbTitle.Text = "撤单成功";
                prompt.tbRefer.Text = "撤单成功";
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

                lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    foreach (DataRowView item in this.MyOrdersDV)
                    {
                        if ((bool)item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true
                            && (Visibility)item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] == Visibility.Visible && (bool)item[MyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true)
                        {
                            OrderCancelRequest cancelPo = new OrderCancelRequest();
                            cancelPo.AlgoName = CommonUtils.ConvertToString(item[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                            cancelPo.OrderID = CommonUtils.ConvertToString(item[MyOrderProcesser.STCCODE_ORDER_FIELD]);
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
                                TBookCancelReplyPo reply = OrdersHandler.Instance.CancelOrder(cancelPo);
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
                        //    item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
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

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView item in this.MyOrdersDV)
                {
                    if (!(item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] is System.DBNull)
                        && (Visibility)(item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD]) == Visibility.Visible && (bool)item[MyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true)
                    {
                        item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = isAllChecked;
                        if (isAllChecked)
                        {
                            checkedCount++;
                        }
                    }
                    else
                    {
                        item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
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

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView item in MyOrdersDV)
                {
                    string side = CommonUtils.ConvertToString(item[MyOrderProcesser.SIDE_ORDER_FIELD]);
                    totalCount++;
                    if (side == "1")
                    {
                        bidCount++;
                    }
                    else if (side == "2")
                    {
                        ofrCount++;
                    }
                    if (!(item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] is System.DBNull)
                       && (Visibility)item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] == Visibility.Visible && (bool)item[MyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true)
                    {
                        checkableCount++;
                        if (!(item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                        {
                            checkedCount++;

                            if (CommonUtils.ConvertToString(item[MyOrderProcesser.FIRMFLAG_ORDER_FIELD]) == "1")
                            {
                                hasFirm = true;
                            }
                            else
                            {
                                hasNonfirm = true;
                            }

                            string securityType = CommonUtils.ConvertToString(item[MyOrderProcesser.SECURITY_TYPE]);
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
                (e.PropertyDescriptor.Name == MyOrderProcesser.SIDE_ORDER_FIELD
                || e.PropertyDescriptor.Name == MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD
                || e.PropertyDescriptor.Name == MyOrderProcesser.ISCHECKED_ORDER_FIELD
                || e.PropertyDescriptor.Name == MyOrderProcesser.FIRMFLAG_ORDER_FIELD)))
            {
                isAdjustAllChkStatus = true;
                //AdjustAllChkStatus();
            }
        }

        private void UncheckAll()
        {
            IsAllChecked = false;
            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView item in MyOrdersDV)
                {
                    item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
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

            string strategyCodeID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.STCCODE_ORDER_FIELD]);
            if (m_windowManager.IsExist($"EditOrder-{strategyCodeID}"))
            {
                return;
            }


            Common.Model.Fix.Message.NewOrderSingle singleOrder = GetNewOrderSingle(this.SelectedItem_MyOrders);
            PriceMatchParam priceMatchParam = new PriceMatchParam();


            //string firmFlag = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
            //string orderQty = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.QTY_ORDER_FIELD]);
            //string ordersID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.STCCODE_ORDER_FIELD]);
            //string randomNum = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.VENUES_RDM_NUM_FIELD]);
            //string side = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.SIDE_ORDER_FIELD]);
            //string securityID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
            //string symbolName = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.SYMBOL_ORDER_FIELD]);
            //string wholeFlag = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.WHOLE_FLAG_FIELD]);
            //string netPrice = CommonUtils.ConvertToDecimal(this.SelectedItem_MyOrders[MyOrderProcesser.NETPRICE_ORDER_FIELD].ToString()).ToString("#.####");
            //string price = CommonUtils.ConvertToDecimal(this.SelectedItem_MyOrders[MyOrderProcesser.PRICE_ORDER_FIELD].ToString()).ToString("#.####");
            //string settleType = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
            //string nonfirmTime = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.NONFIRM_TIME_FIELD]);
            //string crossVenues = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.CROSS_VENUES_FIELD]);
            //string venuesSelectF = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.VENUES_SELECT_F_FIELD]);
            //string selfSelectVenues = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.SELF_SELECT_VENUES_FIELD]);
            //string institutionID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.INSTITUTIONID_FIELD]);
            //string institutionName = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.INSTITUTIONNAME_FIELD]);
            //string traderID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.TRADERID_FIELD]);
            //string traderName = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.TRADERNAME_FIELD]);

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
            priceMatchParam.Info1 = singleOrder.Info1;
            priceMatchParam.Info3 = singleOrder.Info3;//用于IDB单子
            priceMatchParam.Info4 = singleOrder.Info4;//用于意向单的Intentionid


            if (singleOrder.FirmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.OldOrder = singleOrder;

            SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);
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
            sendPriceView.SetButtonEnable("btnXBond", true);

            //sendPriceView.datePicker.IsEnabled = false;
            //sendPriceView.cbSettle.IsEnabled = false;
            //sendPriceView.lbOne.IsEnabled = sendPriceView.lbOther.IsEnabled = false;
            //sendPriceView.lbSpotOne.IsEnabled = sendPriceView.lbSpotOther.IsEnabled = false;

            //sendPriceView.lbAddOneNotLimit.IsEnabled = sendPriceView.lbAddOneToday.IsEnabled = sendPriceView.lbAddOneTomorrow.IsEnabled = false;

            //sendPriceView.Show();
            m_windowManager.OpenOnlyOneWindow($"EditOrder-{strategyCodeID}", sendPriceView);
        }

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(DataRowView drv)
        {
            string strategyCodeID = CommonUtils.ConvertToString(drv[MyOrderProcesser.STCCODE_ORDER_FIELD]);
            string securityID = CommonUtils.ConvertToString(drv[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
            string qty = CommonUtils.ConvertToString(drv[MyOrderProcesser.QTY_ORDER_FIELD]);
            string exeqty = CommonUtils.ConvertToString(drv[MyOrderProcesser.EXECQTY_ORDER_FIELD]);
            string price = CommonUtils.ConvertToString(drv[MyOrderProcesser.PRICE_ORDER_FIELD]);
            string netPrice = CommonUtils.ConvertToString(drv[MyOrderProcesser.NETPRICE_ORDER_FIELD]);
            string settlType = CommonUtils.ConvertToString(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
            string side = CommonUtils.ConvertToString(drv[MyOrderProcesser.SIDE_ORDER_FIELD]);
            string wholeFlag = CommonUtils.ConvertToString(drv[MyOrderProcesser.WHOLE_FLAG_FIELD]);
            string firmFlag = CommonUtils.ConvertToString(drv[MyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
            string crossVenues = CommonUtils.ConvertToString(drv[MyOrderProcesser.CROSS_VENUES_FIELD]);
            string firmToNonTime = CommonUtils.ConvertToString(drv[MyOrderProcesser.NONFIRM_TIME_FIELD]);
            string randomNum = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENUES_RDM_NUM_FIELD]);
            string venuesSelectF = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENUES_SELECT_F_FIELD]);
            string selfSelectVenues = CommonUtils.ConvertToString(drv[MyOrderProcesser.SELF_SELECT_VENUES_FIELD]);


            string venues = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENEUES_ORDER_FIELD]);
            string venueTypeGw = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENUETYPEGW_FIELD]);
            string algoName = CommonUtils.ConvertToString(drv[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
            string quantID = CommonUtils.ConvertToString(drv[MyOrderProcesser.QUANTID_FIELD]);
            string timeInForce = CommonUtils.ConvertToString(drv[MyOrderProcesser.TIMEINFORCE_FIELD]);
            string ordStatus = CommonUtils.ConvertToString(drv[MyOrderProcesser.ORDERSTATUS_ORDER_FIELD]);
            string bizType = CommonUtils.ConvertToString(drv[MyOrderProcesser.BIZTYPE_FIELD]);
            string bizTypeID = CommonUtils.ConvertToString(drv[MyOrderProcesser.BIZTYPEID_FIELD]);
            string quoteID = CommonUtils.ConvertToString(drv[MyOrderProcesser.QUOTEID_FIELD]);
            string roleID = CommonUtils.ConvertToString(drv[MyOrderProcesser.ROLEID_FIELD]);
            string roleName = CommonUtils.ConvertToString(drv[MyOrderProcesser.ROLENAME_FIELD]);
            string symbol = CommonUtils.ConvertToString(drv[MyOrderProcesser.SYMBOL_ORDER_FIELD]);
            string userId = CommonUtils.ConvertToString(drv[MyOrderProcesser.USERID_FIELD]);
            string institutionID = CommonUtils.ConvertToString(drv[MyOrderProcesser.INSTITUTIONID_FIELD]);
            string institutionName = CommonUtils.ConvertToString(drv[MyOrderProcesser.INSTITUTIONNAME_FIELD]);
            string traderID = CommonUtils.ConvertToString(drv[MyOrderProcesser.TRADERID_FIELD]);
            string traderName = CommonUtils.ConvertToString(drv[MyOrderProcesser.TRADERNAME_FIELD]);
            string showTrader = CommonUtils.ConvertToString(drv[MyOrderProcesser.SHOW_TRADER_FIELD]);
            string ternimal = CommonUtils.ConvertToString(drv[MyOrderProcesser.TERMINAL_FIELD]);
            string info1 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO1_FIELD]);
            string info3 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO3_FIELD]);
            string validUntiltime = CommonUtils.ConvertToString(drv[MyOrderProcesser.VALIDUNTILTIME_FIELD]);
            string intentionid = CommonUtils.ConvertToString(drv[MyOrderProcesser.INTENTIONID_FIELD]);
            string accountid = CommonUtils.ConvertToString(drv[MyOrderProcesser.AccountID_Field]);
            string accountName = CommonUtils.ConvertToString(drv[MyOrderProcesser.AccountName_Field]);

            string priceCondition = CommonUtils.ConvertToString(drv[MyOrderProcesser.PriceCondition_Field]);

            Common.Model.Fix.Message.NewOrderSingle sendPricePo = new Common.Model.Fix.Message.NewOrderSingle();

            // BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            sendPricePo.SecurityType = CommonUtils.ConvertToString(drv[MyOrderProcesser.SECURITY_TYPE]);

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
            sendPricePo.Info1 = info1;
            sendPricePo.Info3 = info3;
            sendPricePo.ValidUntilTime = validUntiltime;
            sendPricePo.Info4 = intentionid;
            sendPricePo.AccountID = accountid;
            sendPricePo.AccountName = accountName;

            sendPricePo.PriceCondition = priceCondition;

            return sendPricePo;
        }

        private void OnToFirm()
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.ToFirm;

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {
                        string firmFlag = CommonUtils.ConvertToString(drv[MyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
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
                            drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
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
                        OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
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

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {
                        string firmFlag = CommonUtils.ConvertToString(drv[MyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
                        if (firmFlag == "1")
                        {
                            Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);
                            newOrderSingle.FirmFlag = "0";
                            if (newOrderSingle.Venues == "IDB")
                            {
                                newOrderSingle.Venues = "MANUAL";
                            }
                            newOrderSingle.VenuesSelectF = "1";
                            newOrderSingle.CrossVenues = "2";
                            newOrderSingle.SelfSelectVenues = string.Empty;
                            newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                        }
                        else
                        {
                            drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
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
                        OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
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
            priceMatchParam.Info4 = singleOrder.Info4;

            if (singleOrder.FirmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.OldOrder = singleOrder;

            priceMatchParam.UpdataPrice = false;
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

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
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
                            OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
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

        private void UpdatePriceCallBack(ModifyOrdersReply reply)
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
                        BatchResultWindow batchResultWindow = new BatchResultWindow(newOrderSingleDic,
                            warnReplyDic, prohibitReplyDic, failCount);
                        batchResultWindow.Owner = Application.Current.MainWindow;
                        batchResultWindow.ShowDialog();
                    }
                    else
                    {
                        string text = $"共执行 {totalCount} 条, 其中 {totalCount - failCount} 条成功";

                        Prompt2Seconds prompt = new Prompt2Seconds();
                        if (batchOpsTypeEnum == BatchOperateTypeEnum.EditQty)
                        {
                            if (failCount == 0)
                            {
                                prompt.tbTitle.Text = prompt.tbRefer.Text = "订单已更新";
                            }
                            else
                            {
                                prompt.tbTitle.Text = "批量编辑报量";
                                prompt.tbRefer.Text = text;
                            }

                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.EditPrice)
                        {
                            if (failCount == 0)
                            {
                                prompt.tbTitle.Text = prompt.tbRefer.Text = "订单已更新";
                            }
                            else
                            {
                                prompt.tbTitle.Text = "批量编辑报价";
                                prompt.tbRefer.Text = text;
                            }
                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.ToFirm)
                        {
                            if (failCount == 0)
                            {
                                prompt.tbTitle.Text = prompt.tbRefer.Text = "更新为firm订单";
                            }
                            else
                            {
                                prompt.tbTitle.Text = "批量转Firm";
                                prompt.tbRefer.Text = text;
                            }
                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.ToNonFirm)
                        {
                            if (failCount == 0)
                            {
                                prompt.tbTitle.Text = prompt.tbRefer.Text = "更新为nonfirm订单";
                            }
                            else
                            {
                                prompt.tbTitle.Text = "批量转NonFirm";
                                prompt.tbRefer.Text = text;
                            }
                        }
                        else if (batchOpsTypeEnum == BatchOperateTypeEnum.EditVenues)
                        {
                            if (failCount == 0)
                            {
                                prompt.tbTitle.Text = prompt.tbRefer.Text = "渠道已更新";
                            }
                            else
                            {
                                prompt.tbTitle.Text = "批量编辑渠道";
                                prompt.tbRefer.Text = text;
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

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);
                        modifyQty(newOrderSingle, parameter);
                        int orderQty = CommonUtils.ConvertToInt(newOrderSingle.OrderQty);
                        if (orderQty > 1000000)
                        {
                            drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
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
                            TBookCancelReplyPo result = OrdersHandler.Instance.CancelOrder(cancelPo);

                            ModifyOrdersReply reply = new ModifyOrdersReply();
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
                            ModifyOrdersReply reply = new ModifyOrdersReply();
                            reply.Code = "0001";
                            reply.Msg = "报量超出最大上限1000000万";
                            reply.OrderID = sendPricePo.StrategyOrderId;
                            UpdatePriceCallBack(reply);
                        }
                        else
                        {
                            OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
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

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {
                        if (parameter == "XBOND" || parameter == "默认" || parameter == "默认+")
                        {
                            if (drv[MyOrderProcesser.WHOLE_FLAG_FIELD].ConvertToString() == "1"
                                || drv[MyOrderProcesser.NONFIRM_TIME_FIELD].ConvertToString() == "0")
                            {
                                drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                                continue;
                            }
                        }

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
                        OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
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

        private void AssignVenues()
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.EditVenues;

            bool hasBNC = false;
            bool hasBCO = false;

            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView drv in this.MyOrdersDV)
                {
                    if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                        && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true)
                    {
                        if (drv[MyOrderProcesser.WHOLE_FLAG_FIELD].ConvertToString() == "1"
                            || drv[MyOrderProcesser.FIRMFLAG_ORDER_FIELD].ConvertToString() == "0")
                        {
                            drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                            continue;
                        }

                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);

                        if (CommonUtils.IsRate(newOrderSingle.SecurityType))
                        {
                            hasBNC = true;
                        }
                        else
                        {
                            hasBCO = true;
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

            AssignVenuesWindow win = new AssignVenuesWindow();
            AssignVenuesWindowViewModel viewModel = win.DataContext as AssignVenuesWindowViewModel;
            if (hasBNC) viewModel.IsShowInterestRateDebt = Visibility.Visible;
            if (hasBCO) viewModel.IsShowCreditDebt = Visibility.Visible;
            bool? result = win.ShowDialog();

            string bncSelfSelectVenues = win.InterestRateDebtECV.SelfSelectVenues;
            string bcoSelfSelectsVenues = win.CreditDebtECV.SelfSelectVenues;

            if (result == false)
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
                        sendPricePo.CrossVenues = "1";
                        sendPricePo.VenuesSelectF = "3";
                        sendPricePo.VenuesRdmNum = null;
                        if (CommonUtils.IsRate(sendPricePo.SecurityType))
                        {
                            sendPricePo.SelfSelectVenues = bncSelfSelectVenues;
                        }
                        else
                        {
                            sendPricePo.SelfSelectVenues = bcoSelfSelectsVenues;
                        }

                        OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });


        }


        public string CopySTCCodeText(IList SelectedItemsList, string tabName)
        {
            StringBuilder sb = new StringBuilder();
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (DataRowView dgr in SelectedItemsList)
                {
                    if (tabName.Equals("Orders"))
                    {
                        string stcCodeText = dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string;
                        sb.Append(stcCodeText + "\n");
                    }
                    else if (tabName.Equals("Deals"))
                    {
                        string stcCodeText = dgr[MyExecutionProcesser.STRATEGY_ORDER_ID_FIELD] as string;
                        sb.Append(stcCodeText + "\n");
                    }
                    else if (tabName.Equals("Cancels"))
                    {
                        string stcCodeText = dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string;
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
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SIDE_ORDER_FIELD] as string, "side") + "\t");
                        sb.Append(dgr[MyOrderProcesser.SECURITYID_ORDER_FIELD] as string + "\t");
                        sb.Append(dgr[MyOrderProcesser.SYMBOL_ORDER_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string , "settletype") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.FIRMFLAG_ORDER_FIELD] as string, "firmflag") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.ORDERSTATUS_ORDER_FIELD] as string, "ordstatus") + "\t");
                        //sb.Append(dgr[MyOrderProcesser.ORDERTYPE_FIELD] as string + "\t");
                        sb.Append(string.Format("{0:N4}", dgr[MyOrderProcesser.PRICE_ORDER_FIELD]) + "%\t");
                        //sb.Append(this.Covert(dgr[MyOrderProcesser.WHOLE_FLAG_FIELD] , "wholeflag") + "\t");
                        sb.Append(dgr[MyOrderProcesser.QTY_ORDER_FIELD] + "w\t");
                        //sb.Append(this.NumCovert(dgr[MyOrderProcesser.EXECQTY_ORDER_FIELD]) + "\t");
                        //sb.Append(Constants.SHANXIZHENGQUAN + "\t");//dgr[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venues") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.CROSS_VENUES_FIELD] as string, "CrossVenues") + "\t");
                        //sb.Append(dgr[MyOrderProcesser.VENUES_SELECT_DISPLAY_FIELD] as string + "\t");
                        //sb.Append(dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string + "\t");
                        //sb.Append(TimeUtils.Instance.ToTime(dgr[MyOrderProcesser.UPDATETIME_ORDER_FIELD] as string) + "\t");

                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string, "settletype_myorder"));

                        sb.Append("\n");
                    }
                    else if (tabName.Equals("Deals"))
                    {
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SIDE_FIELD] as string, "side") + "\t");
                        sb.Append(dgr[MyExecutionProcesser.SECURITYID_FIELD] as string + "\t");
                        sb.Append(dgr[MyExecutionProcesser.SYMBOL_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SYNC_STATUS_FIELD] as string, "synchflag") + "\t");
                        sb.Append(string.Format("{0:N4}%", dgr[MyExecutionProcesser.DISPLAY_LASTPX_FIELD]) + "\t");
                        //sb.Append(this.NumCovert(dgr[MyExecutionProcesser.NETPRICE_FIELD]) + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SETTLTYPE_FIELD] as string, "settletype") + "\t");
                        sb.Append(dgr[MyExecutionProcesser.LASTQTY_FIELD] + "w\t");

                        string side = dgr[MyExecutionProcesser.SIDE_FIELD] as string;// ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SIDE_FIELD] as string, "side");

                        if (side.Equals("1"))
                        {
                            sb.Append(this.StringConvert(dgr[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]) + "\tto\t");
                            //sb.Append(Constants.SHANXIZHENGQUAN + "\t");
                            //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.PARTYNAME_FIELD]) + "\t");
                            sb.Append(this.StringConvert(dgr[MyExecutionProcesser.INSTITUTIONNAME_FIELD]) + "\t");
                        }
                        else
                        {
                            sb.Append(this.StringConvert(dgr[MyExecutionProcesser.INSTITUTIONNAME_FIELD]) + "\tto\t");
                            //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.PARTYNAME_FIELD]) + "\tto\t");
                            //sb.Append(Constants.SHANXIZHENGQUAN + "\tto\t");
                            sb.Append(this.StringConvert(dgr[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]) + "\t");
                        }

                        //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.COUNTERPARTYSUBNAME_FIELD]) + "\t");
                        //sb.Append(dgr[MyExecutionProcesser.EXECID_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.VENUES_FIELD] as string, "venues_exe") + "\t");
                        //sb.Append(dgr[MyExecutionProcesser.PARTYSUBNAME_FIELD] as string + "\t");
                        //sb.Append(TimeUtils.Instance.ToTime(dgr[MyExecutionProcesser.EXECUTIONTIME_FIELD] as string) + "\t");
                        //sb.Append(dgr[MyExecutionProcesser.STRATEGY_ORDER_ID_FIELD] as string + "\t");
                        //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.REFORDERID_FIELD]) + "\t");     
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SETTLTYPE_FIELD] as string, "settletype") + "\t");
                        sb.Append("\n");
                    }
                    else if (tabName.Equals("Cancels"))
                    {
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SIDE_ORDER_FIELD] as string, "side") + "\t");
                        sb.Append(dgr[MyOrderProcesser.SECURITYID_ORDER_FIELD] as string + "\t");
                        sb.Append(dgr[MyOrderProcesser.SYMBOL_ORDER_FIELD] as string + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string, "settletype") + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.FIRMFLAG_ORDER_FIELD] as string, "firmflag") + "\t");
                        sb.Append(string.Format("{0:N4}", dgr[MyOrderProcesser.PRICE_ORDER_FIELD]) + "\t");
                        sb.Append(this.Covert(dgr[MyOrderProcesser.WHOLE_FLAG_FIELD], "wholeflag") + "\t");
                        sb.Append(dgr[MyOrderProcesser.QTY_ORDER_FIELD] + "\t");
                        sb.Append(this.NumCovert(dgr[MyOrderProcesser.EXECQTY_ORDER_FIELD]) + "\t");
                        sb.Append(dgr[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venues") + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.CROSS_VENUES_FIELD] as string, "CrossVenues") + "\t");
                        sb.Append(dgr[MyOrderProcesser.VENUES_SELECT_DISPLAY_FIELD] as string + "\t");
                        sb.Append(dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string + "\t");
                        sb.Append(TimeUtils.Instance.ToTime(dgr[MyOrderProcesser.UPDATETIME_ORDER_FIELD] as string) + "\t");
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
                    dgr[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = dgr[MyOrderProcesser.ISCHECKED_ORDER_FIELD].ToString().ToLower() == "true" ? false : true;
                }
            }
        }

        private string Covert(object value, string parameter)
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

        public DataRowView HighlightRow_MyOrder(string intentionId)
        {
            foreach (DataRowView drv in this.MyOrdersDV)
            {
                if (CommonUtils.ConvertToString(drv[MyOrderProcesser.INTENTIONID_FIELD]) == intentionId)
                {
                    this.SelectedItem_MyOrders = drv;
                    return drv;
                }
            }
            this.SelectedItem_MyOrders = null;
            return null;
        }
    }
}
