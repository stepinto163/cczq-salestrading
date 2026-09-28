using Ats.Foundation.Message;
using com.gateway.log;
using Microsoft.Expression.Interactivity.Core;
using STC.Common.Command;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.OwnExec;
using STC.Common.Handler.Position;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.TBook;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Position;
using STC.Common.Model.TBook;
using STC.Common.Model.WorkTable;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Order;
using STC.Common.View.Order.InnerPanel;
using STC.Common.View.WorkTable;
using STC.Common.ViewModel.Order;
using STC.Frame.Common;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.View.Batch;
using STC.XBond.Plugin.View.Component;
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

namespace STC.XBond.Plugin.ViewModel
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
        /// 0: 显示 1：不显示     国债期货 订单操作底部菜单
        /// </summary>
        private Visibility isHiddenMyOrdersOperationGridTF;
        public Visibility IsHiddenMyOrdersOperationGridTF
        {
            get
            {
                return isHiddenMyOrdersOperationGridTF;
            }

            set
            {
                if (isHiddenMyOrdersOperationGridTF != value)
                {
                    isHiddenMyOrdersOperationGridTF = value;
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
                        _MyOrdersDV.Sort = $"{MyOrderProcesser.UPDATETIME_ORDER_FIELD} desc";
                        this.OnPropertyChanged();
                    }

                    return this._MyOrdersDV;
                }
            }
            set
            {
                _MyOrdersDV = value;
                OnPropertyChanged("MyOrdersDV");
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

        // 我的成交总数
        private int _totalCount4MyExes = 0;
        public int TotalCount4MyExes
        {
            get
            {
                return _totalCount4MyExes;
            }
            set
            {
                if (_totalCount4MyExes != value)
                {
                    _totalCount4MyExes = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 我的成交中bid的总数
        /// </summary>
        private int _totalCount4MyBidExes = 0;
        public int TotalCount4MyBidExes
        {
            get
            {
                return _totalCount4MyBidExes;
            }
            set
            {
                if (_totalCount4MyBidExes != value)
                {
                    _totalCount4MyBidExes = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 我的成交中ofr的总数
        /// </summary>
        private int _totalCount4MyOfrExes = 0;
        public int TotalCount4MyOfrExes
        {
            get
            {
                return _totalCount4MyOfrExes;
            }
            set
            {
                if (_totalCount4MyOfrExes != value)
                {
                    _totalCount4MyOfrExes = value;
                    this.OnPropertyChanged();
                }
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
                lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
                {
                    if (_MyExecutionsDV == null)
                    {
                        _MyExecutionsDV = new DataView(MyExecutionProcesser.Processer.MyExecutionsDT);
                        this.OnPropertyChanged(nameof(MyExecutionsDV));
                        MyExecutionsDV_ListChanged(null, null);
                    }
                    return this._MyExecutionsDV;
                }
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
                this.OnPropertyChanged();
            }
        }

        // 已撤订单总数
        private int _totalCount4MyCancelOrders = 0;
        public int TotalCount4MyCancelOrders
        {
            get
            {
                return _totalCount4MyCancelOrders;
            }
            set
            {
                if (_totalCount4MyCancelOrders != value)
                {
                    _totalCount4MyCancelOrders = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 已撤订单中bid的总数
        /// </summary>
        private int _totalCount4MyBidCancelOrders = 0;
        public int TotalCount4MyBidCancelOrders
        {
            get
            {
                return _totalCount4MyBidCancelOrders;
            }
            set
            {
                if (_totalCount4MyBidCancelOrders != value)
                {
                    _totalCount4MyBidCancelOrders = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 已撤订单中ofr的总数
        /// </summary>
        private int _totalCount4MyOfrCancelOrders = 0;
        public int TotalCount4MyOfrCancelOrders
        {
            get
            {
                return _totalCount4MyOfrCancelOrders;
            }
            set
            {
                if (_totalCount4MyOfrCancelOrders != value)
                {
                    _totalCount4MyOfrCancelOrders = value;
                    this.OnPropertyChanged();
                }
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
                lock (MyOrderProcesser.Processer.LockObj_MyCancelOrders)
                {
                    if (_MyCancelOrdersDV == null)
                    {
                        _MyCancelOrdersDV = new DataView(MyOrderProcesser.Processer.MyCancelOrdersDT);
                        //_MyCancelOrdersDV.RowFilter = $"{MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '4'";
                        _MyCancelOrdersDV.Sort = $"{MyOrderProcesser.UPDATETIME_ORDER_FIELD} desc";
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


        private bool _BatchOCOOperateBtnEnanble = true;

        public bool BatchOCOOperateBtnEnanble
        {
            get
            {
                return _BatchOCOOperateBtnEnanble;
            }
            set
            {
                if (_BatchOCOOperateBtnEnanble != value)
                {
                    _BatchOCOOperateBtnEnanble = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private bool _BatchXBONDOperateBtnEnanble = true;

        public bool BatchXBONDOperateBtnEnanble
        {
            get
            {
                return _BatchXBONDOperateBtnEnanble;
            }
            set
            {
                if (_BatchXBONDOperateBtnEnanble != value)
                {
                    _BatchXBONDOperateBtnEnanble = value;
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
        private string totalCountOrderHeader = CommonUtils.IsBDBAppName() ? "我的需求" : "我的订单";
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
        private string _TotalCountExecutionsHeader = "我的成交";
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

        #region Rfq
        /// <summary>
        /// RFQ
        /// </summary>
        private string rfqheader = "RFQ列表";
        public string RfqHeader
        {
            get
            {
                return rfqheader;
            }
            set
            {
                rfqheader = value;
                this.OnPropertyChanged(nameof(RfqHeader));
            }
        }


        /// <summary>
        /// RFQ 是否显示小红点 +0 显示，""不显示
        /// </summary>
        private string isrfqshowreddot = "";
        public string IsRfqShowRedDot
        {
            get
            {
                return isrfqshowreddot;
            }
            set
            {
                isrfqshowreddot = value;
                this.OnPropertyChanged(nameof(IsRfqShowRedDot));
            }
        }
        #endregion

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

        public ICommand OpenDrpProcDiagCommand { get; set; }

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
        public MyTraderViewModel()
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

            AssignVenuesCommand = new RelayCommand(AssignVenues);

            OpenDrpProcDiagCommand = new DelegateCommand(new Action<object>(OnOpenDrpProcDiagCommand));

            Task.Run(() =>
            {
                try
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {

                        // 我的订单
                        BindingOperations.EnableCollectionSynchronization(MyOrdersDV, MyOrderProcesser.Processer.LockObj_MyOrders);
                        this.MyOrdersDV.ListChanged += MyOrdersDV_ListChanged;
                        MyOrderProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions1;

                        // 我的成交
                        BindingOperations.EnableCollectionSynchronization(MyExecutionsDV, MyExecutionProcesser.Processer.LockObj_MyExecutions);
                        this.MyExecutionsDV.ListChanged += MyExecutionsDV_ListChanged;
                        MyExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;

                        // 已撤订单
                        BindingOperations.EnableCollectionSynchronization(MyCancelOrdersDV, MyOrderProcesser.Processer.LockObj_MyCancelOrders);
                        this.MyCancelOrdersDV.ListChanged += MyCancelOrdersDV_ListChanged;


                        // 是否有内部交易权限
                        //IsHaveVenueInnerPermission = GetIsHaveInner(goodsType);
                        // 获取渠道权限
                        IsHaveInnerPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);
                        IsHaveInnerXbondPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);
                        IsHaveAllPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
                        //IsHaveVenueXbondPermission = CheckVenuePermission(XBOND);
                        //bool isShowXBond = (IsHaveAllPermission || IsHaveInnerXbondPermission) && IsHaveVenueXbondPermission;

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

                        //RFQ订阅
                        BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

                        BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);
                    });
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void onloadData(bool obj)
        {
            if (obj)
            {
                lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    foreach (DataRow myOrder in MyOrderProcesser.Processer.MyOrdersDT.Rows)
                    {
                        BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(myOrder[MyOrderProcesser.SECURITYID_ORDER_FIELD] + "");
                        if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                    ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                    (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                        {
                            if (myOrder[MyOrderProcesser.YieldToExecutionBAK_FIELD] != null && CommonUtils.IsNumeric(myOrder[MyOrderProcesser.YieldToExecutionBAK_FIELD] + ""))
                                myOrder[MyOrderProcesser.YieldToExecution_FIELD] = CommonUtils.ConvertToDecimal(myOrder[MyOrderProcesser.YieldToExecutionBAK_FIELD] + "");
                            else
                                myOrder[MyOrderProcesser.YieldToExecution_FIELD] = null;
                        }
                    }
                }
                
                lock (MyOrderProcesser.Processer.LockObj_MyCancelOrders)
                {
                    foreach (DataRow myOrder in MyOrderProcesser.Processer.MyCancelOrdersDT.Rows)
                    {
                        BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(myOrder[MyOrderProcesser.SECURITYID_ORDER_FIELD] + "");
                        if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                    ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                    (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                        {
                            if (myOrder[MyOrderProcesser.YieldToExecutionBAK_FIELD] != null && CommonUtils.IsNumeric(myOrder[MyOrderProcesser.YieldToExecutionBAK_FIELD] + ""))
                                myOrder[MyOrderProcesser.YieldToExecution_FIELD] = CommonUtils.ConvertToDecimal(myOrder[MyOrderProcesser.YieldToExecutionBAK_FIELD] + "");
                            else
                                myOrder[MyOrderProcesser.YieldToExecution_FIELD] = null;
                        }
                    }
                }

                lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
                {
                    foreach (DataRow myOrder in MyExecutionProcesser.Processer.MyExecutionsDT.Rows)
                    {
                        BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(myOrder[MyExecutionProcesser.SECURITYID_FIELD] + "");
                        if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                    ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                    (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                        {
                            if (myOrder[MyExecutionProcesser.YieldToExecutionBAK_FIELD] != null && CommonUtils.IsNumeric(myOrder[MyExecutionProcesser.YieldToExecutionBAK_FIELD] + ""))
                                myOrder[MyExecutionProcesser.YieldToExecution_FIELD] = CommonUtils.ConvertToDecimal(myOrder[MyExecutionProcesser.YieldToExecutionBAK_FIELD] + "");
                            else
                                myOrder[MyExecutionProcesser.YieldToExecution_FIELD] = null;
                        }
                    }
                }
            }
        }
            private void MyExecutionsDV_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e == null || e.PropertyDescriptor == null || e.PropertyDescriptor.Name == MyExecutionProcesser.SIDE_FIELD)
            {
                int totalCount = 0;
                int totalBidCount = 0;
                int totalOfrCount = 0;
                lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
                {
                    foreach (DataRowView item in this.MyExecutionsDV)
                    {
                        totalCount++;
                        string side = CommonUtils.ConvertToString(item[MyExecutionProcesser.SIDE_FIELD]);

                        if (side == "1")
                        {
                            totalBidCount++;
                        }
                        else if (side == "2")
                        {
                            totalOfrCount++;
                        }
                    }
                }

                TotalCount4MyExes = totalCount;
                TotalCount4MyBidExes = totalBidCount;
                TotalCount4MyOfrExes = totalOfrCount;
            }
        }

        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;
            if (obj.Code == "RFQHeader")
            {
                if ((obj.Msg) == "0")
                {
                    this.RfqHeader = "RFQ列表";
                }
                else
                {
                    this.RfqHeader = $"RFQ列表({obj.Msg})";
                }


                if (TradeTabSelectedItem == "MyRfq_TabItem")
                {
                    IsRfqShowRedDot = "";
                }
                else
                {
                    if (obj.Msg != "0")
                        IsRfqShowRedDot = "+0";
                    else
                        IsRfqShowRedDot = "";
                }
            }
            else if (obj.Code == "ChatMsg")
            {
                string id = obj.Msg;
                DataRow dr = MyOrderProcesser.Processer.GetOrderById(id);
                if (dr != null)
                {
                    Common.Model.Fix.Message.NewOrderSingle order = GetNewOrderSingle(dr);
                    OpenOneSendPriceView(order, order.StrategyOrderId);
                }
            }
        }

        private void Processer_OnLastExecutions1(object obj)
        {
            int cnt = int.Parse(obj.ToString());
            if (cnt == 0)
            {
                IsMyOrdersShowRedDot = "";
                TotalCountOrderHeader = CommonUtils.IsBDBAppName() ? "我的需求" : "我的订单";
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

                string header = CommonUtils.IsBDBAppName() ? "我的需求" : "我的订单";
                TotalCountOrderHeader = $"{header}({cnt})";
            }
        }

        private void Processer_OnLastExecutions(object obj)
        {
            //if (Application.Current == null || Application.Current.Dispatcher.CheckAccess() == false)
            //{
            //    return;
            //}
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (obj.GetType() == typeof(int))
                {
                    int cnt = int.Parse(obj.ToString());
                    if (cnt == 0)
                    {
                        IsMyExecutionsShowRedDot = "";
                        TotalCountExecutionsHeader = "我的成交";
                    }
                    else
                    {
                        try
                        {
                            lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
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

                        TotalCountExecutionsHeader = $"我的成交({cnt})";
                    }
                }
            });
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
            catch (ThreadAbortException threadAbortException)
            {
                // ignore.
            }
            catch (Exception e)
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
                || (e.PropertyDescriptor != null && e.PropertyDescriptor.Name == MyOrderProcesser.ISCHECKED_ORDER_FIELD))
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
            BaseHandler.Instance.UnSubscribeAppMsg(OnAppMsg);

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

            // 释放我的成交数据
            lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
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
                foreach (string item in venuesList)
                {
                    if (item == "All")
                    {
                        inFilterSB_venues.Remove(0, inFilterSB_venues.Length);
                        break;
                    }

                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }

                    if (item == Constants.TBook)
                    {
                        inFilterSB_venues.Append("'MANUAL','QUANT','IDB'");
                    }
                    else if (item == Constants.XBond)
                    {
                        inFilterSB_venues.Append("'ODM'");
                    }
                    else if (item == CommonConstants.ALL_EN && item == "AllBroker")
                    {
                        inFilterSB_venues.Append("'CNEX','TPSC','CTTJ','PTCN','CCTB','UEDA'");
                    }
                    else if (item == CommonConstants.CNEX)
                    {
                        inFilterSB_venues.Append("'CNEX'");
                    }
                    else if (item == CommonConstants.TPSC)
                    {
                        inFilterSB_venues.Append("'TPSC'");
                    }
                    else if (item == CommonConstants.CTTJ)
                    {
                        inFilterSB_venues.Append("'CTTJ'");
                    }
                    else if (item == CommonConstants.PTCN)
                    {
                        inFilterSB_venues.Append("'PTCN'");
                    }
                    else if (item == CommonConstants.CCTB)
                    {
                        inFilterSB_venues.Append("'CCTB'");
                    }
                    else if (item == CommonConstants.UEDA)
                    {
                        inFilterSB_venues.Append("'UEDA'");
                    }
                    else if (item.Equals("双边"))
                    {
                        inFilterSB_venues.Append("'QDMESP'");
                    }
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                filter_executions.Append($"{MyExecutionProcesser.VENUES_FIELD} in ({inFilterSB_venues})");
            }

            // syncStatus
            IList syncStatusList = filterDic["SyncStatus"];
            StringBuilder inFilterSB_syncStatus = new StringBuilder();
            if (syncStatusList.Count > 0)
            {
                foreach (string item in syncStatusList)
                {
                    if (item == CommonConstants.ALL)
                    {
                        continue;
                    }

                    if (inFilterSB_syncStatus.Length > 0)
                    {
                        inFilterSB_syncStatus.Append(" or ");
                    }

                    if (item == Constants.SEND)
                    {
                        inFilterSB_syncStatus.Append($"{ MyExecutionProcesser.SYNC_STATUS_FIELD} = 'Y'");
                    }
                    else if (item == Constants.SOLVE)
                    {
                        inFilterSB_syncStatus.Append($"{MyExecutionProcesser.SYNC_STATUS_FIELD} = 'S'");
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
                    if (item.Content.ToString() == CommonConstants.ALL_EN)
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

                filter_executions.Append($"{MyOrderProcesser.SECURITY_TYPE} in ({inFilterSB_Code})");
            }

            // 债券/期货/IRS
            IList contractTypeList = filterDic["ContractType"];
            StringBuilder inFilterSB_ContractType = new StringBuilder();
            if (contractTypeList.Count > 0)
            {
                foreach (ListBoxItem item in contractTypeList)
                {
                    if (item.Content.ToString() == CommonConstants.ALL_EN)
                    {
                        continue;
                    }

                    if (inFilterSB_ContractType.Length > 0)
                    {
                        inFilterSB_ContractType.Append(",");
                    }

                    if (item.Content.ToString() == Constants.BOND)
                    {
                        inFilterSB_ContractType.Append("'4'");
                    }
                    else if (item.Content.ToString() == Constants.FUTURE)
                    {
                        inFilterSB_ContractType.Append("'5'");
                    }
                    else if (item.Content.ToString() == Constants.IRS)
                    {
                        inFilterSB_ContractType.Append("'2'");
                    }
                }
            }
            if (inFilterSB_ContractType.Length > 0)
            {
                if (filter_executions.Length > 0)
                {
                    filter_executions.Append(" and ");
                }

                filter_executions.Append($"{MyExecutionProcesser.MARKETINDICATOR_FIELD} in ({inFilterSB_ContractType})");
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

            lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
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
                foreach (string item in venuesList)
                {
                    if (item == CommonConstants.ALL_EN)
                    {
                        continue;
                    }

                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }

                    if (item == Constants.MANUAL)
                    {
                        inFilterSB_venues.Append("'MANUAL'");
                    }
                    else if (item == Constants.IDB)
                    {
                        inFilterSB_venues.Append("'IDB'");
                    }
                    else if (item == Constants.QUANT)
                    {
                        inFilterSB_venues.Append("'QUANT'");
                    }
                    else if (item == Constants.INNER)
                    {
                        inFilterSB_venues.Append("'MANUAL'");

                        //List<string> allVenues = SystemParamHandler.Instance.GetAllVenue();
                        //foreach (string venue in allVenues)
                        //{
                        //    inFilterSB_venues.Append(",'" + venue + "'");
                        //}
                    }
                    else if (item == Constants.QDMESP)
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
                    if (item.Tag.ToString() == CommonConstants.ALL_EN)
                    {
                        continue;
                    }

                    if (inFilterSB_FirmFlag.Length > 0)
                    {
                        inFilterSB_FirmFlag.Append(",");
                    }

                    if (item.Tag.ToString() == Constants.FIRM)
                    {
                        inFilterSB_FirmFlag.Append("'1'");
                    }
                    else if (item.Tag.ToString() == Constants.NONFIRM)
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
                    if (item.Content.ToString() == CommonConstants.ALL_EN)
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

                filter_orders.Append($"{MyOrderProcesser.SECURITY_TYPE} in ({inFilterSB_Code})");
            }

            // 债券/期货/IRS
            IList contractTypeList = filterDic["ContractType"];
            StringBuilder inFilterSB_ContractType = new StringBuilder();
            if (contractTypeList.Count > 0)
            {
                foreach (ListBoxItem item in contractTypeList)
                {
                    if (item.Content.ToString() == CommonConstants.ALL_EN)
                    {
                        continue;
                    }

                    if (inFilterSB_ContractType.Length > 0)
                    {
                        inFilterSB_ContractType.Append(",");
                    }

                    if (item.Content.ToString() == Constants.BOND)
                    {
                        inFilterSB_ContractType.Append("'4'");
                    }
                    else if (item.Content.ToString() == Constants.FUTURE)
                    {
                        inFilterSB_ContractType.Append("'5'");
                    }
                    else if (item.Content.ToString() == Constants.IRS)
                    {
                        inFilterSB_ContractType.Append("'2'");
                    }
                }
            }

            if (inFilterSB_ContractType.Length > 0)
            {
                if (filter_orders.Length > 0)
                {
                    filter_orders.Append(" and ");
                }

                filter_orders.Append($"{MyOrderProcesser.MARKETINDICATOR_FIELD} in ({inFilterSB_ContractType})");
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
                    if (item.Content.ToString() == CommonConstants.ALL_EN)
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

                        //List<string> allVenues = SystemParamHandler.Instance.GetAllVenue();
                        //foreach (string venue in allVenues)
                        //{
                        //    inFilterSB_venues.Append(",'" + venue + "'");
                        //}
                    }
                    else if (item.Content.ToString() == Constants.QDMESP)
                    {
                        //algoNameFiter = $"{MyOrderProcesser.ALGONAME_ORDER_FIELD} in ('MM')";
                        inFilterSB_venues.Append("'QDMESP'");
                    }
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                filter_cancelOrders.Append($"{MyOrderProcesser.VENEUES_ORDER_FIELD} in ({inFilterSB_venues})");
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
                    if (item.Tag.ToString() == CommonConstants.ALL_EN)
                    {
                        continue;
                    }

                    if (inFilterSB_FirmFlag.Length > 0)
                    {
                        inFilterSB_FirmFlag.Append(",");
                    }

                    if (item.Tag.ToString() == Constants.FIRM)
                    {
                        inFilterSB_FirmFlag.Append("'1'");
                    }
                    else if (item.Tag.ToString() == Constants.NONFIRM)
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

                filter_cancelOrders.Append($"{MyOrderProcesser.FIRMFLAG_ORDER_FIELD} in ({inFilterSB_FirmFlag})");
            }

            // 利率/信用
            IList codelist = filterDic["CodeType"];
            StringBuilder inFilterSB_Code = new StringBuilder();
            if (codelist.Count > 0)
            {
                foreach (ListBoxItem item in codelist)
                {
                    if (item.Content.ToString() == CommonConstants.ALL_EN)
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

                filter_cancelOrders.Append($"{MyOrderProcesser.SECURITY_TYPE} in ({inFilterSB_Code})");
            }

            // 债券/期货
            IList contractTypeList = filterDic["ContractType"];
            StringBuilder inFilterSB_ContractType = new StringBuilder();
            if (contractTypeList.Count > 0)
            {
                foreach (ListBoxItem item in contractTypeList)
                {
                    if (item.Content.ToString() == CommonConstants.ALL_EN)
                    {
                        continue;
                    }

                    if (inFilterSB_ContractType.Length > 0)
                    {
                        inFilterSB_ContractType.Append(",");
                    }

                    if (item.Content.ToString() == Constants.BOND)
                    {
                        inFilterSB_ContractType.Append("'4'");
                    }
                    else if (item.Content.ToString() == Constants.FUTURE)
                    {
                        inFilterSB_ContractType.Append("'5'");
                    }
                    else if (item.Content.ToString() == Constants.IRS)
                    {
                        inFilterSB_ContractType.Append("'2'");
                    }
                }
            }

            if (inFilterSB_ContractType.Length > 0)
            {
                if (filter_cancelOrders.Length > 0)
                {
                    filter_cancelOrders.Append(" and ");
                }

                filter_cancelOrders.Append($"{MyOrderProcesser.MARKETINDICATOR_FIELD} in ({inFilterSB_ContractType})");
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
                tempFilter.Append($"({MyOrderProcesser.SECURITYID_ORDER_FIELD} like '%{this.SearchText}%' or {MyOrderProcesser.SYMBOL_ORDER_FIELD} like '%{this.SearchText}%')");
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

            //tempFilter.Append($"{MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '4'");

            lock (MyOrderProcesser.Processer.LockObj_MyCancelOrders)
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
                bool result = Common.View.Component.CustomMessageBox.Question("是否撤销该笔报价？", "删除订单");
                if (result == true)
                {
                    string algoname = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                    if (algoname == "DQ")
                    {
                        //定向报价撤单, 如果是自己撤自己的DQ 则Info == 4
                        STC.Common.Model.Fix.Message.OrderCancelRequest rs = Common.ViewModel.Drp.Utils.DRPTools.GetCancelOrder(this.SelectedItem_MyOrders);
                        rs.Info1 = "4";
                        STC.Common.Model.Fix.Message.PResult res = await Common.ViewModel.Drp.Utils.DRPTools.CancelOrder(rs);
                        if (res != null)
                        {
                            if (res.Code == "0000")
                            {
                            }
                            else
                            {
                                Common.View.Component.CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{res.Msg}."), "删除订单");
                            }
                        }
                    }
                    else
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
                                Common.View.Component.CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
                            }
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

            bool result = result = Common.View.Component.CustomMessageBox.Question("是否撤销全部报价（不包括点价FAK订单和量化订单）？", "删除订单");

            if (result == true)
            {
                StringBuilder error = null;
                error = await SubDeleteAllOrders(selected);
                if (error != null && error.Length > 0)
                {
                    Common.View.Component.CustomMessageBox.Error(string.Format($"删除订单失败, 原因:{error}"));
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
                        if (((Visibility)item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] == Visibility.Visible && (bool)item[MyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true  && selected != "一键撤量化订单")
                           || (!string.IsNullOrEmpty(CommonUtils.ConvertToString(item[MyOrderProcesser.QUANTID_FIELD])) && selected == "一键撤量化订单"))
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
            BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = false;
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
            BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;
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
            //是否选中国债期货订单
            bool isTreasuryFutureChecked = false;
            lock (MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView item in this.MyOrdersDV)
                {
                    if (!(item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] is System.DBNull)
                        && (Visibility)(item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD]) == Visibility.Visible && (bool)item[MyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true && item[MyOrderProcesser.ALGONAME_ORDER_FIELD].ToString() != "DQ" && item[MyOrderProcesser.QuoteType_Field].ToString() != "virtual" && item[MyOrderProcesser.QuoteType_Field].ToString() != "irs")//&& item[MyOrderProcesser.QuoteType_Field].ToString() != "require")
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
                    if (Consts.MARKETINDICATOR_FUTURE.Equals(item[MyOrderProcesser.MARKETINDICATOR_FIELD]))
                    {
                        isTreasuryFutureChecked = true;
                    }
                }
            }

            CheckedCount4MyOrders = checkedCount;
            if (checkedCount > 0)
            {
                IsAllChecked = true;
                if (isTreasuryFutureChecked)
                {
                    IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
                    IsHiddenMyOrdersOperationGridTF = Visibility.Visible;
                }
                else
                {
                    IsHiddenMyOrdersOperationGrid = Visibility.Visible;
                    IsHiddenMyOrdersOperationGridTF = Visibility.Hidden;
                }
            }
            else
            {
                IsAllChecked = false;
                IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
                IsHiddenMyOrdersOperationGridTF = Visibility.Hidden;
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

            bool hasSTP = false;
            bool hasFwd = false;

            int bidCount = 0;
            int ofrCount = 0;
            int totalCount = 0;
            int requireCount = 0;
            //是否选中国债期货订单
            bool isTreasuryFutureChecked = false;
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

                    string quoteType = CommonUtils.ConvertToString(item[MyOrderProcesser.QuoteType_Field]);
                    if (!(item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] is System.DBNull) && (Visibility)item[MyOrderProcesser.SHOWDELETEBTN_ORDER_FIELD] == Visibility.Visible && (bool)item[MyOrderProcesser.ENABLEDBTN_ORDER_FIELD] == true && item[MyOrderProcesser.ALGONAME_ORDER_FIELD].ToString() != "DQ" && item[MyOrderProcesser.QuoteType_Field].ToString() != "virtual")// && item[MyOrderProcesser.QuoteType_Field].ToString() != "require")
                    {
                        checkableCount++;
                        if (quoteType == "require")
                        {
                            requireCount++;
                        }
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
                            if (Consts.MARKETINDICATOR_FUTURE.Equals(item[MyOrderProcesser.MARKETINDICATOR_FIELD]))
                            {
                                isTreasuryFutureChecked = true;
                            }

                            string execType = CommonUtils.ConvertToString(item[MyOrderProcesser.SEND_ORDER_TYPE_FIELD]);
                            if (execType == "STP" || execType == "直通")
                            {
                                hasSTP = true;
                            }

                            string settle = CommonUtils.ConvertToString(item[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
                            if (!(settle == "1" || settle == "2" || settle == "2,3" || settle == "T+0" || settle == "T+1" || settle == "T+1(不限)"))
                            {
                                hasFwd = true;
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
                if (isTreasuryFutureChecked)
                {
                    IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
                    IsHiddenMyOrdersOperationGridTF = Visibility.Visible;
                }
                else
                {
                    IsHiddenMyOrdersOperationGrid = Visibility.Visible;
                    IsHiddenMyOrdersOperationGridTF = Visibility.Hidden;
                }
            }
            else
            {
                IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
                IsHiddenMyOrdersOperationGridTF = Visibility.Hidden;
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


            if (CommonUtils.IsBDBAppName())
            {
                Visibility_XBondBtn = Visibility.Collapsed;
                Visibility_Direct = Visibility.Collapsed;
            }
            else
            {
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

                if (directCount > 0 && requireCount < checkedCount)
                {
                    Visibility_Direct = Visibility.Visible;
                }
                else
                {
                    Visibility_Direct = Visibility.Collapsed;
                }
            }

            if (hasSTP && !(PermisionHelper.Instance.isOCO() || PermisionHelper.Instance.isAllExec()))
            {
                BatchOCOOperateBtnEnanble = false;
            }
            else
            {
                BatchOCOOperateBtnEnanble = true;
            }


            if (hasFwd)
            {
                BatchXBONDOperateBtnEnanble = false;
            }
            else
            {
                BatchXBONDOperateBtnEnanble = true;

                if (hasSTP && !(PermisionHelper.Instance.isOCO() || PermisionHelper.Instance.isAllExec()))
                {
                    BatchXBONDOperateBtnEnanble = false;
                }
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
            lock (MyOrderProcesser.Processer.LockObj_MyCancelOrders)
            {
                foreach (DataRowView item in this.MyCancelOrdersDV)
                {
                    if (CommonUtils.ConvertToString(item[MyOrderProcesser.VENEUES_ORDER_FIELD]) == "QUANT"
                        || CommonUtils.ConvertToString(item[MyOrderProcesser.VENEUES_ORDER_FIELD]) == "QDMESP"
                        || item[MyOrderProcesser.QuoteType_Field].ToString() == "virtual"
                        || item[MyOrderProcesser.QuoteType_Field].ToString() == "irs")
                        //|| item[MyOrderProcesser.QuoteType_Field].ToString() == "require")
                    {
                        continue;
                    }
                    if (isAllChecked)
                    {
                        checkTrueCount++;
                    }
                    item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = isAllChecked;
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

            int bidCount = 0;
            int ofrCount = 0;
            int totalCount = 0;

            lock (MyOrderProcesser.Processer.LockObj_MyCancelOrders)
            {
                foreach (DataRowView item in MyCancelOrdersDV)
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

                    if (CommonUtils.ConvertToString(item[MyOrderProcesser.VENEUES_ORDER_FIELD]) == "QUANT"
                        || CommonUtils.ConvertToString(item[MyOrderProcesser.VENEUES_ORDER_FIELD]) == "QDMESP"
                        || item[MyOrderProcesser.QuoteType_Field].ToString() == "virtual")
                        //|| item[MyOrderProcesser.QuoteType_Field].ToString() == "require")
                    {
                        filterCount++;
                    }
                    else
                    {
                        if ((item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull
                        || (bool)item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == false))
                        {
                            isCheckAll = false;
                            //break;
                        }
                    }
                    
                }
                IsAllChecked_CancelOrders = isCheckAll && MyCancelOrdersDV.Count > filterCount;
            }

            TotalCount4MyCancelOrders = totalCount;
            TotalCount4MyOfrCancelOrders = ofrCount;
            TotalCount4MyBidCancelOrders = bidCount;
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
            lock(MyOrderProcesser.Processer.LockObj_MyOrders)
            {
                foreach (DataRowView item in MyOrdersDV)
                {
                    item[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                }
            }
            IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
            IsHiddenMyOrdersOperationGridTF = Visibility.Hidden;
        }

        private void EditOrder()
        {
            if (this.SelectedItem_MyOrders == null)
            {
                return;
            }

            string algoname = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
            string strategyCodeID = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.STCCODE_ORDER_FIELD]);
            string securityID = CommonUtils.ConvertToString(SelectedItem_MyOrders[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
            
            //如果是修改定向报价
            if (algoname == "DQ")
            {
                string id = $"EditDrpOrder-{strategyCodeID}";
                if (!WindowManager.Instance.IsExist(id))
                {
                    SendPointPriceView sendPointPriceView = new SendPointPriceView(this.SelectedItem_MyOrders);
                    WindowManager.Instance.OpenOnlyOneWindow(id, sendPointPriceView);
                }

                return;
            }

            if (m_windowManager.IsExist($"EditOrder-{strategyCodeID}"))
            {
                return;
            }

            Common.Model.Fix.Message.NewOrderSingle singleOrder = GetNewOrderSingle(this.SelectedItem_MyOrders);
            PriceMatchParam priceMatchParam = new PriceMatchParam();

            priceMatchParam.SendOrderType = singleOrder.AlgoName.Equals("CROSS") ? "OCO" : "直通";

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
            priceMatchParam.Info2 = singleOrder.Info2;
            priceMatchParam.Info3 = singleOrder.Info3;//用于IDB单子
            priceMatchParam.Info4 = singleOrder.Info4;//用于意向单的Intentionid
            priceMatchParam.AccountID = singleOrder.AccountID;
            priceMatchParam.Text = singleOrder.Text;
            priceMatchParam.PriceCondition = singleOrder.PriceCondition;
            priceMatchParam.channel = singleOrder.Channel;
            //priceMatchParam.QuoteType = singleOrder.QuoteType;
            if (singleOrder.FirmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.OldOrder = singleOrder;

            //如果是模糊需求
            if (securityID == "99999")
            {
                OrderTimeModel otm = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderTimeModel>(singleOrder.Info1);
                if (!string.IsNullOrEmpty(otm.QuoteType))
                {
                    if (otm.QuoteType == "virtual")
                    {
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "EditFuzzyOrder", RequestID = $"EditFuzzyOrder-{priceMatchParam.OrderId}", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(priceMatchParam) });
                        return;
                    }
                }
            }

            string marketindicator = singleOrder.MarketIndicator;
            if (marketindicator == "2")
            {
                //IRS报价
                SendIRSView sendPriceView = new SendIRSView(priceMatchParam);
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetPointWinKey(singleOrder.StrategyOrderId), sendPriceView);
            }
            else
            {
                //如果是需求
                if (singleOrder.VenueTypeGW == "INNER" && !string.IsNullOrEmpty(singleOrder.IntentionID))
                {
                    priceMatchParam.Info2 = singleOrder.Info2;
                    priceMatchParam.IntentionID = singleOrder.IntentionID;
                    priceMatchParam.OldOrder = null;
                    SendRequireView sendRequireView = new SendRequireView(priceMatchParam);
                    m_windowManager.OpenOnlyOneWindow($"EditOrder-{priceMatchParam.OrderId}", sendRequireView);
                }
                else
                {

                    //SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);
                    SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
                    // bid;
                    if (singleOrder.Side == "1")
                    {
                        sendPriceView.SetButtonEnable("OfrBtn", false);
                    }
                    // ofr
                    else if (singleOrder.Side == "2")
                    {
                        sendPriceView.SetButtonEnable("BidBtn", false);
                    }

                    sendPriceView.SetButtonEnable("Search_TextBox", false);
                    m_windowManager.OpenOnlyOneWindow($"EditOrder-{strategyCodeID}", sendPriceView);
                }
            }
        }

        /// <summary>
        /// 处理idb数据
        /// </summary>
        private void ProcessIDBData()
        {
            string exeID = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.EXECID_FIELD]);
            if (m_windowManager.IsExist($"ProcessIDBData-{exeID}"))
            {
                return;
            }

            // 当前行转成ExecutionReport
            Common.Model.Fix.Message.ExecutionReport er = new Common.Model.Fix.Message.ExecutionReport();
            er.SecurityID = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.SECURITYID_FIELD]);
            er.Symbol = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.SYMBOL_FIELD]);
            er.Price = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.LASTPX_FIELD]);
            er.Side = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.SIDE_FIELD]);
            er.OrderQty = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.LASTQTY_FIELD]);
            er.TransactTime = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.EXECUTIONTIME_FIELD]);
            er.ExecID = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.EXECID_FIELD]);
            er.Venues = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.VENUES_FIELD]);
            er.CounterPartyName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]);
            er.CounterPartySubName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.COUNTERPARTYSUBNAME_FIELD]);
            er.OrderID = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.ORDERID_FIELD]);
            er.SettlType = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.SETTLTYPE_FIELD]); 
            er.TradeDate = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.TRADEDATE_FIELD]);

            er.TransactTime = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.EXECUTIONTIME_FIELD]);
            er.TraderName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.TRADENAME_FIELD]);
            er.InstitutionName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.INSTITUTIONNAME_FIELD]);
            
            er.Info6 = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.INFO6_FIELD]);

            er.UserName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.USERNAME_FIELD]);
            er.PartyName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.PARTYNAME_FIELD]);
            er.PartySubName = CommonUtils.ConvertToString(this.SelectedItem_MyExecutions[MyExecutionProcesser.PARTYSUBNAME_FIELD]);

            ExecuteReplyView erv = new ExecuteReplyView(er);
            //erv.Owner = Application.Current.MainWindow;
            //erv.Show();
            m_windowManager.OpenOnlyOneWindow($"ProcessIDBData-{exeID}", erv);
        }

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(DataRow dr)
        {
            string strategyCodeID = CommonUtils.ConvertToString(dr[MyOrderProcesser.STCCODE_ORDER_FIELD]);
            string securityID = CommonUtils.ConvertToString(dr[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
            string qty = CommonUtils.ConvertToString(dr[MyOrderProcesser.QTY_ORDER_FIELD]);
            string exeqty = CommonUtils.ConvertToString(dr[MyOrderProcesser.EXECQTY_ORDER_FIELD]);
            string price = CommonUtils.ConvertToString(dr[MyOrderProcesser.PRICE_ORDER_FIELD]);
            string netPrice = CommonUtils.ConvertToString(dr[MyOrderProcesser.NETPRICE_ORDER_FIELD]);
            string settlType = CommonUtils.ConvertToString(dr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
            string side = CommonUtils.ConvertToString(dr[MyOrderProcesser.SIDE_ORDER_FIELD]);
            string wholeFlag = CommonUtils.ConvertToString(dr[MyOrderProcesser.WHOLE_FLAG_FIELD]);
            string firmFlag = CommonUtils.ConvertToString(dr[MyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
            string crossVenues = CommonUtils.ConvertToString(dr[MyOrderProcesser.CROSS_VENUES_FIELD]);
            string firmToNonTime = CommonUtils.ConvertToString(dr[MyOrderProcesser.NONFIRM_TIME_FIELD]);
            string randomNum = CommonUtils.ConvertToString(dr[MyOrderProcesser.VENUES_RDM_NUM_FIELD]);
            string venuesSelectF = CommonUtils.ConvertToString(dr[MyOrderProcesser.VENUES_SELECT_F_FIELD]);
            string selfSelectVenues = CommonUtils.ConvertToString(dr[MyOrderProcesser.SELF_SELECT_VENUES_FIELD]);


            string venues = CommonUtils.ConvertToString(dr[MyOrderProcesser.VENEUES_ORDER_FIELD]);
            string venueTypeGw = CommonUtils.ConvertToString(dr[MyOrderProcesser.VENUETYPEGW_FIELD]);
            string algoName = CommonUtils.ConvertToString(dr[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
            string quantID = CommonUtils.ConvertToString(dr[MyOrderProcesser.QUANTID_FIELD]);
            string timeInForce = CommonUtils.ConvertToString(dr[MyOrderProcesser.TIMEINFORCE_FIELD]);
            string ordStatus = CommonUtils.ConvertToString(dr[MyOrderProcesser.ORDERSTATUS_ORDER_FIELD]);
            string bizType = CommonUtils.ConvertToString(dr[MyOrderProcesser.BIZTYPE_FIELD]);
            string bizTypeID = CommonUtils.ConvertToString(dr[MyOrderProcesser.BIZTYPEID_FIELD]);
            string quoteID = CommonUtils.ConvertToString(dr[MyOrderProcesser.QUOTEID_FIELD]);
            string roleID = CommonUtils.ConvertToString(dr[MyOrderProcesser.ROLEID_FIELD]);
            string roleName = CommonUtils.ConvertToString(dr[MyOrderProcesser.ROLENAME_FIELD]);
            string symbol = CommonUtils.ConvertToString(dr[MyOrderProcesser.SYMBOL_ORDER_FIELD]);
            string userId = CommonUtils.ConvertToString(dr[MyOrderProcesser.USERID_FIELD]);
            string institutionID = CommonUtils.ConvertToString(dr[MyOrderProcesser.INSTITUTIONID_FIELD]);
            string institutionName = CommonUtils.ConvertToString(dr[MyOrderProcesser.INSTITUTIONNAME_FIELD]);
            string traderID = CommonUtils.ConvertToString(dr[MyOrderProcesser.TRADERID_FIELD]);
            string traderName = CommonUtils.ConvertToString(dr[MyOrderProcesser.TRADERNAME_FIELD]);
            string showTrader = CommonUtils.ConvertToString(dr[MyOrderProcesser.SHOW_TRADER_FIELD]);
            string ternimal = CommonUtils.ConvertToString(dr[MyOrderProcesser.TERMINAL_FIELD]);
            string info1 = CommonUtils.ConvertToString(dr[MyOrderProcesser.INFO1_FIELD]);
            string info2 = CommonUtils.ConvertToString(dr[MyOrderProcesser.INFO2_FIELD]);
            string info3 = CommonUtils.ConvertToString(dr[MyOrderProcesser.INFO3_FIELD]);
            string validUntiltime = CommonUtils.ConvertToString(dr[MyOrderProcesser.VALIDUNTILTIME_FIELD]);


            string marketindicator = CommonUtils.ConvertToString(dr[MyOrderProcesser.MARKETINDICATOR_FIELD]);
            string accountid = CommonUtils.ConvertToString(dr[MyOrderProcesser.AccountID_Field]);
            string accountName = CommonUtils.ConvertToString(dr[MyOrderProcesser.AccountName_Field]);

            string priceCondition = CommonUtils.ConvertToString(dr[MyOrderProcesser.PriceCondition_Field]);

            string intentionID = CommonUtils.ConvertToString(dr[MyOrderProcesser.INTENTIONID_FIELD]);

            Common.Model.Fix.Message.NewOrderSingle sendPricePo = new Common.Model.Fix.Message.NewOrderSingle();

            // BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            sendPricePo.SecurityType = CommonUtils.ConvertToString(dr[MyOrderProcesser.SECURITY_TYPE]);
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
            sendPricePo.Info2 = info2;
            sendPricePo.Info3 = info3;
            OrderTimeModel otm = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderTimeModel>(info1);
            string ValidUntilTime = validUntiltime;
            if (otm != null && otm.ValidTimeType == "0|")
            {
                // 默认
                int hour = DateTimeEx.ServerTimeNow.Hour;
                int min = DateTimeEx.ServerTimeNow.Minute;
                if (hour > 0 && hour < 6)
                {
                    ValidUntilTime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (hour >= 6 && hour < 12)
                {
                    ValidUntilTime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    ValidUntilTime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00").ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            else if (otm != null && otm.ValidTimeType.StartsWith("1|"))
            {
                //时长
                int totalsecond = 0;
                string temp = otm.ValidTimeType.Replace("1|", string.Empty);
                string[] tempArr = temp.Split(CommonConstants.COLON_CHAR);
                if (tempArr != null && tempArr.Length == 2)
                {
                    string ValidMinute = tempArr[0];
                    string ValidSecond = tempArr[1];
                    if (!string.IsNullOrEmpty(ValidMinute))
                    {
                        totalsecond = int.Parse(ValidMinute) * 60;
                    }

                    if (!string.IsNullOrEmpty(ValidSecond))
                    {
                        totalsecond += int.Parse(ValidSecond);
                    }
                }


                if (totalsecond == 0)
                {
                    ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(30).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }


            // 用户设置的到期时间,需要大于用户自定义的时间
            string selfUntilTime = ValidUntilTime;
            UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
            if (userSetting != null)
            {
                // 个人设置时间
                string validUntiTimeSetting = userSetting.validUntiTimeSetting;

                // 如果撤单时间和个人设置时间不为空
                if (!string.IsNullOrEmpty(selfUntilTime) && (!string.IsNullOrEmpty(validUntiTimeSetting)))
                {
                    validUntiTimeSetting = string.Format("{0} {1}", DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString(), validUntiTimeSetting);
                    // 默认，时长，时点
                    if (!(otm != null && otm.ValidTimeType == "3|"))
                    {
                        ValidUntilTime = GetMinDate(selfUntilTime, validUntiTimeSetting, otm != null && otm.ValidTimeType == "0|");
                    }
                    else
                    {
                        //选择了无
                        if (DateTimeEx.ServerTimeNow.CompareTo(DateTime.Parse(validUntiTimeSetting)) <= 0)
                        {
                            ValidUntilTime = validUntiTimeSetting;
                        }
                        else
                        {
                            ValidUntilTime = string.Empty;
                        }
                    }
                }
                else
                {
                    // 默认，时长，时点
                    if (!(otm != null && otm.ValidTimeType == "3|"))
                    {
                        // 默认
                        if (otm != null && otm.ValidTimeType == "0|")
                        {
                            ValidUntilTime = GetMinDate(selfUntilTime, "", true);
                        }
                    }
                    else
                    {
                        ValidUntilTime = string.Empty;
                    }
                }
            }
            sendPricePo.ValidUntilTime = ValidUntilTime;

            sendPricePo.Info4 = intentionID;
            sendPricePo.MarketIndicator = marketindicator;
            sendPricePo.AccountID = accountid;
            sendPricePo.AccountName = accountName;

            sendPricePo.PriceCondition = priceCondition;

            sendPricePo.IntentionID = intentionID;

            return sendPricePo;
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
            string info2 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO2_FIELD]);
            string info3 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO3_FIELD]);
            string validUntiltime = CommonUtils.ConvertToString(drv[MyOrderProcesser.VALIDUNTILTIME_FIELD]);


            string marketindicator = CommonUtils.ConvertToString(drv[MyOrderProcesser.MARKETINDICATOR_FIELD]);
            string accountid = CommonUtils.ConvertToString(drv[MyOrderProcesser.AccountID_Field]);
            string accountName = CommonUtils.ConvertToString(drv[MyOrderProcesser.AccountName_Field]);

            string priceCondition = CommonUtils.ConvertToString(drv[MyOrderProcesser.PriceCondition_Field]);

            string intentionID = CommonUtils.ConvertToString(drv[MyOrderProcesser.INTENTIONID_FIELD]);
            
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
            sendPricePo.Info2 = info2;
            sendPricePo.Info3 = info3;
            OrderTimeModel otm = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderTimeModel>(info1);
            string ValidUntilTime = validUntiltime;
            if (otm != null && otm.ValidTimeType == "0|")
            {
                // 默认
                int hour = DateTimeEx.ServerTimeNow.Hour;
                int min = DateTimeEx.ServerTimeNow.Minute;
                if (hour > 0 && hour < 6)
                {
                    ValidUntilTime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (hour >= 6 && hour < 12)
                {
                    ValidUntilTime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00").ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    ValidUntilTime = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00").ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            else if (otm != null && otm.ValidTimeType.StartsWith("1|"))
            {
                //时长
                int totalsecond = 0;
                string temp = otm.ValidTimeType.Replace("1|", string.Empty);
                string[] tempArr = temp.Split(CommonConstants.COLON_CHAR);
                if (tempArr != null && tempArr.Length == 2)
                {
                    string ValidMinute = tempArr[0];
                    string ValidSecond = tempArr[1];
                    if (!string.IsNullOrEmpty(ValidMinute))
                    {
                        totalsecond = int.Parse(ValidMinute) * 60;
                    }

                    if (!string.IsNullOrEmpty(ValidSecond))
                    {
                        totalsecond += int.Parse(ValidSecond);
                    }
                }
                

                if (totalsecond == 0)
                {
                    ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(30).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            

            // 用户设置的到期时间,需要大于用户自定义的时间
            string selfUntilTime = ValidUntilTime;
            UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
            if (userSetting != null)
            {
                // 个人设置时间
                string validUntiTimeSetting = userSetting.validUntiTimeSetting;

                // 如果撤单时间和个人设置时间不为空
                if (!string.IsNullOrEmpty(selfUntilTime) && (!string.IsNullOrEmpty(validUntiTimeSetting)))
                {
                    validUntiTimeSetting = string.Format("{0} {1}", DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString(), validUntiTimeSetting);
                    // 默认，时长，时点
                    if (!(otm != null && otm.ValidTimeType == "3|"))
                    {
                        ValidUntilTime = GetMinDate(selfUntilTime, validUntiTimeSetting, otm != null && otm.ValidTimeType == "0|");
                    }
                    else
                    {
                        //选择了无
                        if (DateTimeEx.ServerTimeNow.CompareTo(DateTime.Parse(validUntiTimeSetting)) <= 0)
                        {
                            ValidUntilTime = validUntiTimeSetting;
                        }
                        else
                        {
                            ValidUntilTime = string.Empty;
                        }
                    }
                }
                else
                {
                    // 默认，时长，时点
                    if (!(otm != null && otm.ValidTimeType == "3|"))
                    {
                        // 默认
                        if (otm != null && otm.ValidTimeType == "0|")
                        {
                            ValidUntilTime = GetMinDate(selfUntilTime, "", true);
                        }
                    }
                    else
                    {
                        ValidUntilTime = string.Empty;
                    }
                }
            }
            sendPricePo.ValidUntilTime = ValidUntilTime;

            sendPricePo.Info4 = intentionID;
            sendPricePo.MarketIndicator = marketindicator;
            sendPricePo.AccountID = accountid;
            sendPricePo.AccountName = accountName;

            sendPricePo.PriceCondition = priceCondition;

            sendPricePo.IntentionID = intentionID;

            return sendPricePo;
        }

        /// <summary>
        /// 取2个日期的最小值,当为默认时
        /// </summary>
        /// <param name="selfUntilTime">撤单时间</param>
        /// <param name="validUntiTimeSetting">个人设置</param>
        /// <returns></returns>
        private string GetMinDate(string selfUntilTime, string validUntiTimeSetting = "", bool isDefault = true)
        {
            List<DateTime> LstTime = new List<DateTime>();
            DateTime cudatetime = DateTimeEx.ServerTimeNow;
            LogHandler.Info($"GetMinDate:selfUntilTime:{selfUntilTime} validUntiTimeSetting:{validUntiTimeSetting} isDefault:{isDefault.ToString()}" + " ServerTime:" + cudatetime.ToString("yyyy-MM-dd HH:mm:ss"));
            // 如果是时点, 失效时间<当前时间，则直接失效
            if (!isDefault)
            {
                if (cudatetime.CompareTo(DateTime.Parse(selfUntilTime)) >= 0)
                {
                    return selfUntilTime;
                }
            }

            // DateTime cudatetime = DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 08:35:00");
            if (isDefault)
            {
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00"));
            }

            LstTime.Add(cudatetime);
            LstTime.Add(DateTime.Parse(selfUntilTime));
            if (!string.IsNullOrEmpty(validUntiTimeSetting))
            {
                LstTime.Add(DateTime.Parse(validUntiTimeSetting));
            }

            List<DateTime> NewLstTime = LstTime.Distinct().ToList();
            NewLstTime.Sort();
            int i = NewLstTime.IndexOf(cudatetime);
            NewLstTime.RemoveRange(0, i);

            if (NewLstTime.Count == 1)
            {
                return string.Empty;
            }
            else
            {
                return NewLstTime[1].ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        private void OnToFirm()
        {
            BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = false;
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
                BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;
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
            BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = false;
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


                            //直通单变OCO
                            if (newOrderSingle.AlgoName == "STP")
                            {
                                newOrderSingle.AlgoName = "CROSS";
                                newOrderSingle.VenueTypeGW = "INNER";
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
                BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;
                return;
            }

            //bool checkResult = PlaceOrderAdapter.Instance.CheckNonFirmOrder(newOrderSingleDic.Values.ToList());
            //if (!checkResult)
            //{
            //    BatchOperateBtnEnable = true;
            //    return;
            //}

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
            priceMatchParam.RandomNum = singleOrder.VenuesRdmNum; // 渠道类型默认，填写随机几家渠道
            priceMatchParam.Side = singleOrder.Side; // 方向
            priceMatchParam.Price = singleOrder.Price; // 收益率
            priceMatchParam.UpdataPrice = false;
            priceMatchParam.CustomFirmAmount = singleOrder.OrderQty; // OrderQty 订单量
            if (singleOrder.FirmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.OrderId = singleOrder.StrategyOrderId;// 订单号
            priceMatchParam.Symbol = singleOrder.SecurityID; // 代码
            priceMatchParam.SymbolName = singleOrder.Symbol; // 名称
            priceMatchParam.WholeFlag = singleOrder.WholeFlag; // 整量标志
            priceMatchParam.NetPrice = singleOrder.NetPrice; // 净价

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
            priceMatchParam.Info2 = singleOrder.Info2;
            priceMatchParam.Info3 = singleOrder.Info3;
            priceMatchParam.Info4 = singleOrder.Info4;//用于意向单的Intentionid
            priceMatchParam.AccountID = singleOrder.AccountID;

            priceMatchParam.PriceCondition = singleOrder.PriceCondition;
            priceMatchParam.channel = singleOrder.Channel;

            //priceMatchParam.SendOrderType = CommonUtils.ConvertToString(this.SelectedItem_MyCancelOrders[MyOrderProcesser.SEND_ORDER_TYPE_FIELD]);
            priceMatchParam.SendOrderType = singleOrder.AlgoName.Equals("CROSS") ? "OCO" : "直通";


            if (singleOrder.FirmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.OldOrder = singleOrder;
            priceMatchParam.Text = singleOrder.Text;
            string marketindicator = singleOrder.MarketIndicator.ToString();
            if (marketindicator == "2")
            {
                //IRS报价
                SendIRSView sendPriceView = new SendIRSView(priceMatchParam);
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetPointWinKey(singleOrder.StrategyOrderId), sendPriceView);
            }
            else
            {
                //如果是需求
                if (singleOrder.VenueTypeGW == "INNER" && !string.IsNullOrEmpty(singleOrder.IntentionID))
                {
                    priceMatchParam.IntentionID = singleOrder.IntentionID;
                    SendRequireView sendRequireView = new SendRequireView(priceMatchParam);
                    m_windowManager.OpenOnlyOneWindow($"ResumeOrder-{priceMatchParam.OrderId}", sendRequireView);
                }
                else
                {
                    if (!WindowManager.Instance.IsExist($"ResumeOrder-{strategyCodeID}"))
                    {
                        SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
                        // bid;
                        if (singleOrder.Side == "1")
                        {
                            sendPriceView.SetButtonEnable("OfrBtn", false);
                        }
                        // ofr
                        else if (singleOrder.Side == "2")
                        {
                            sendPriceView.SetButtonEnable("BidBtn", false);
                        }
                        sendPriceView.SetButtonEnable("Search_TextBox", false);

                        //如果是模糊需求
                        if (singleOrder.SecurityID == "99999")
                        {
                            OrderTimeModel otm = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderTimeModel>(singleOrder.Info1);
                            if (!string.IsNullOrEmpty(otm.QuoteType))
                            {
                                if (otm.QuoteType == "virtual")
                                {
                                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "EditFuzzyOrder", RequestID = $"EditFuzzyOrder-{priceMatchParam.OrderId}", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(priceMatchParam) });
                                    return;
                                }
                            }
                        }

                        WindowManager.Instance.OpenOnlyOneWindow($"ResumeOrder-{strategyCodeID}", sendPriceView);
                    }
                }
            }
        }

        private void ResumeOrder()
        {
            if (this.SelectedItem_MyCancelOrders == null)
            {
                return;
            }

            string strategyCodeID = CommonUtils.ConvertToString(this.SelectedItem_MyCancelOrders[MyOrderProcesser.STCCODE_ORDER_FIELD]);
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
            if (newOrderSingleDic.Count > 0)
            {
                bool treasuryFutureSelected = false;
                //检查是否选中了国债期货
                foreach(var item in newOrderSingleDic.Values)
                {
                    if(Consts.MARKETINDICATOR_FUTURE.Equals(item.MarketIndicator))
                    {
                        treasuryFutureSelected = true;
                        break;
                    }
                }
                //国债期货不能执行恢复操作
                if(treasuryFutureSelected)
                {
                    Common.View.Component.CustomMessageBox.Info("国债期货订单不支持恢复操作!", "恢复报价");
                    IsEnabled_Resend = true;
                    return;
                }
            }
            if (newOrderSingleDic.Count == 0)
            {
                Common.View.Component.CustomMessageBox.Info("请先选中需恢复报价的记录!", "恢复报价");
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

            if (CommonUtils.IsBDBAppName() == false)
            {
                BatchVenueView batchVenueView = new BatchVenueView();
                batchVenueView.Init(newOrderSingleDic);
                batchVenueView.Owner = Application.Current.MainWindow;
                batchVenueView.ShowDialog();
            }
            else
            {
                await Task.Run(() =>
                {
                    int successCount = 0;
                    foreach (Common.Model.Fix.Message.NewOrderSingle neworder in newOrderSingleDic.Values)
                    {
                        Common.Model.Fix.Message.PResult reply = PlaceOrder(neworder);
                        if (reply != null && reply.IsSuccess())
                        {
                            successCount++;
                        }
                    }
                    CommonUtils.Prompt2Second("批量恢复报价", $"共执行{newOrderSingleDic.Values.Count}条，其中{successCount}条成功");
                });
            }


            IsEnabled_Resend = true;
        }

        private Common.Model.Fix.Message.PResult PlaceOrder(Common.Model.Fix.Message.NewOrderSingle sendPricePo)
        {
            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();
            Common.Model.Fix.Message.PResult reply = OrdersHandler.Instance.PlaceOrder(sendPricePo);
            return reply;
        }

        private async Task<Dictionary<string, Common.Model.Fix.Message.NewOrderSingle>> Sub_ResendOrders()
        {
            Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic = new Dictionary<string, Common.Model.Fix.Message.NewOrderSingle>();
            await Task.Run(() =>
            {
                
                lock (MyOrderProcesser.Processer.LockObj_MyCancelOrders)
                {
                    foreach (DataRowView drv in this.MyCancelOrdersDV)
                    {
                        if (!(drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] is System.DBNull)
                            && (bool)drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] == true && CommonUtils.ConvertToString(drv[MyOrderProcesser.VENEUES_ORDER_FIELD]) != "QUANT" && CommonUtils.ConvertToString(drv[MyOrderProcesser.VENEUES_ORDER_FIELD]) != "QDMESP" && drv[MyOrderProcesser.QuoteType_Field].ToString() != "virtual")
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
            BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = false;
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
                BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;
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
                    BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;
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
            if (CommonUtils.IsNumeric(newOrderSingle.Price))
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
           
        }

        private void EditQty(string parameter)
        {
            BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = false;
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
                BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;
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
            BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = false;
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
                                || drv[MyOrderProcesser.FIRMFLAG_ORDER_FIELD].ConvertToString() == "0")
                            {
                                drv[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                                continue;
                            }
                        }

                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);

                        modifyVenues(newOrderSingle, parameter);

                        if (parameter == "XBOND" && newOrderSingle.SelfSelectVenues.Contains("ODM") && newOrderSingle.SettlType == "2,3")
                        {
                            newOrderSingle.SettlType = "2";
                        }


                        if (newOrderSingle.Venues == "IDB")
                        {
                            newOrderSingle.Venues = "MANUAL";
                        }

                        //直通单变OCO
                        //if (newOrderSingle.AlgoName == "STP")
                        {
                            newOrderSingle.AlgoName = "CROSS";
                            newOrderSingle.VenueTypeGW = "INNER";
                            newOrderSingle.Venues = "MANUAL";

                            //newOrderSingle.CrossVenues = "1";
                            //newOrderSingle.SelfSelectVenues = string.Empty;
                        }


                        newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;
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

                if (newOrderSingle.AlgoName == "STP")
                {
                    newOrderSingle.AlgoName = "CROSS";
                }

            }
            if (parameter == "XBOND")
            {
                newOrderSingle.CrossVenues = "3";
                newOrderSingle.VenuesSelectF = "3";
                newOrderSingle.SelfSelectVenues = "ODM-ODM";
                newOrderSingle.VenuesRdmNum = null;

                if (newOrderSingle.AlgoName == "STP")
                {
                    newOrderSingle.AlgoName = "CROSS";
                }

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
            BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            warnReplyDic.Clear();
            prohibitReplyDic.Clear();
            batchOpsTypeEnum = BatchOperateTypeEnum.EditVenues;

            bool hasBNC = false;
            bool hasBCO = false;
            bool hasFwd = false;

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

                        string settle = CommonUtils.ConvertToString(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
                        if (!(settle == "1" || settle == "2" || settle == "2,3" || settle == "T+0" || settle == "T+1" || settle == "T+1(不限)"))
                        {
                            hasFwd = true;
                        }


                        newOrderSingleDic[newOrderSingle.StrategyOrderId] = newOrderSingle;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0)
            {
                BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;
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
                BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;
                return;
            }

            if (hasFwd && ((!string.IsNullOrEmpty(bncSelfSelectVenues) && bncSelfSelectVenues.Contains("ODM"))
                    || (!string.IsNullOrEmpty(bcoSelfSelectsVenues) && bcoSelfSelectsVenues.Contains("ODM"))))
            {
                Common.View.Component.CustomMessageBox.Warning("批量操作包含XBOND渠道，XBOND不支持远期报价");

                BatchXBONDOperateBtnEnanble = BatchOCOOperateBtnEnanble = BatchOperateBtnEnable = true;

                if (hasFwd)
                {
                    BatchXBONDOperateBtnEnanble = false;
                }

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

                        sendPricePo.AlgoName = "CROSS";
                        sendPricePo.VenueTypeGW = "INNER";
                        sendPricePo.Venues = "MANUAL";


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


                if (AppUtils.Application.Panel.SelectedItem != null
                && AppUtils.Application.Panel.SelectedItem is TabItem)
                {
                    TabItem tabItem = AppUtils.Application.Panel.SelectedItem as TabItem;
                    if (tabItem.Content is HistoryView)
                    {

                        HistoryView historyView = tabItem.Content as HistoryView;
                        if (isExecution)
                        {
                            historyView.SelectDealView();
                        }
                        else
                        {
                            historyView.SelectOrderView();
                        }
                    }
                }
            }
        }

        public string CopySTCCodeText(IList SelectedItemsList,string tabName)
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
            Dictionary<CustomExecDataGrid.ExeKey, long> exeDic = new Dictionary<CustomExecDataGrid.ExeKey, long>();
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
                    else if (tabName.Equals("Deals_Institution"))
                    {
                        string securityId = CommonUtils.ConvertToString(dgr[MyExecutionProcesser.SECURITYID_FIELD]);
                        string symbolName = CommonUtils.ConvertToString(dgr[MyExecutionProcesser.SYMBOL_FIELD]);

                        string px = CommonUtils.ConvertToString(dgr[MyExecutionProcesser.LASTPX_FIELD]);
                        //decimal d = 0;
                        //if (decimal.TryParse(px, out d))
                        //    px = string.Format("{0:N4}%", d);

                        string qty = CommonUtils.ConvertToString(dgr[MyExecutionProcesser.LASTQTY_FIELD]);

                        string side = CommonUtils.ConvertToString(dgr[MyExecutionProcesser.SIDE_FIELD]);

                        string partySubName = CommonUtils.ConvertToString(dgr[MyExecutionProcesser.PARTYSUBNAME_FIELD]);
                        //string counterPartySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]);

                        string settle = CommonUtils.ConvertToString(dgr[MyExecutionProcesser.SETTLTYPE_FIELD]);

                        settle = ComboSourceUtil.Instance.FindEnum(settle, "settletype");

                        //string color = side == "2" ? "#58B3FF" : "#FB943C";

                        //side = ComboSourceUtil.Instance.FindEnum(drv[MyExecutionProcesser.SIDE_FIELD] as string, "side");
                        string institutionName = CommonUtils.ConvertToString(dgr[MyExecutionProcesser.INSTITUTIONNAME_FIELD], "--");
                        string tradeName = CommonUtils.ConvertToString(dgr[MyExecutionProcesser.TRADENAME_FIELD], "--");

                        CustomExecDataGrid.ExeKey key = new CustomExecDataGrid.ExeKey()
                        {
                            securityId = securityId,
                            symbolName = symbolName,
                            px = CommonUtils.ConvertToDecimal(px),
                            settle = settle,
                            side = side,
                            partySubName = partySubName,
                            InstitutionName = institutionName,
                            TradeName = tradeName
                        };

                        if (exeDic.ContainsKey(key))
                        {
                            exeDic[key] += CommonUtils.ConvertToLong(qty);
                        }
                        else
                        {
                            exeDic[key] = CommonUtils.ConvertToLong(qty);
                        }
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
                        sb.Append(dgr[MyOrderProcesser.PARTYSUBNAME_FIELD] as string + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venues") + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.CROSS_VENUES_FIELD] as string, "CrossVenues") + "\t");
                        sb.Append(dgr[MyOrderProcesser.VENUES_SELECT_DISPLAY_FIELD] as string + "\t");
                        sb.Append(dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string + "\t");
                        sb.Append(TimeUtils.Instance.ToTime(dgr[MyOrderProcesser.UPDATETIME_ORDER_FIELD] as string) + "\t");
                        sb.Append("\n");
                    }
                }
            }

            foreach (KeyValuePair<CustomExecDataGrid.ExeKey, long> pair in exeDic)
            {
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }

                string txt = string.Empty;
                if (pair.Key.side == "1")
                {
                    txt = $"\t{pair.Key.partySubName}\tto\t{pair.Key.InstitutionName}:{pair.Key.TradeName}";
                }
                else
                {
                    txt = $"\t{pair.Key.InstitutionName}:{pair.Key.TradeName}\tto\t{pair.Key.partySubName}";
                }
                sb.Append($"{pair.Key.securityId}\t{pair.Key.symbolName}\t{string.Format("{0:N4}%", pair.Key.px)}\t{pair.Value}w{txt}\t{pair.Key.settle}");
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

        public DataRowView HighlightRow_MyCancelOrder(string intentionId)
        {
            foreach (DataRowView drv in this.MyCancelOrdersDV)
            {
                if (CommonUtils.ConvertToString(drv[MyOrderProcesser.INTENTIONID_FIELD]) == intentionId)
                {
                    this.SelectedItem_MyCancelOrders = drv;
                    return drv;
                }
            }
            this.SelectedItem_MyCancelOrders = null;
            return null;
        }

        public DataRowView HighlightRow_MyExe(string intentionId)
        {
            foreach (DataRowView drv in this.MyExecutionsDV)
            {
                if (CommonUtils.ConvertToString(drv[MyExecutionProcesser.INTENTIONID_FIELD]).Contains(intentionId))
                {
                    this.SelectedItem_MyExecutions = drv;
                    return drv;
                }
            }
            this.SelectedItem_MyExecutions = null;
            return null;
        }

        #region 我的成交 标签
        private Dictionary<string, string> _tagDic = new Dictionary<string, string>();

        public Dictionary<string, string> TagDic
        {
            get
            {
                if (_tagDic.Count == 0)
                {
                    _tagDic["1"] = "平盘";
                    _tagDic["2"] = "对冲";
                    _tagDic["3"] = "未平盘";
                }
                return _tagDic;
            }
        }

        internal void UpdateExe()
        {
            ThreadPoolHelper.InvokeThreadPool(o => {
                if (this.SelectedItem_MyExecutions == null)
                    return;

                string exeid = this.SelectedItem_MyExecutions[MyExecutionProcesser.ORDERID_FIELD].ConvertToString();
                string tag1 = this.SelectedItem_MyExecutions[MyExecutionProcesser.TAG1_FIELD].ConvertToString();
                Common.Model.EMS.UpdateOrderReq req = new Common.Model.EMS.UpdateOrderReq();
                req.OrderId = exeid;
                req.tag1 = tag1;

                Common.Model.Fix.Message.PResult result = Common.Handler.EMS.EMSHandler.Instance.updateOrder(req);

                if (result == null)
                {
                    CommonUtils.Prompt2Second("警告", $"成交编号:[{exeid}]修改失败");
                }
                else
                {
                    if (!result.IsSuccess())
                    {
                        CommonUtils.Prompt2Second("警告", result.Msg);
                    }
                }
            });
        }

        #endregion

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="inparam"></param>
        private void OnOpenDrpProcDiagCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                TMessageContent detail = Common.Handler.Drp.DrpHandler.Instance.GetContent((string)inparam);
                if (detail != null)
                {
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "DRPMsgPo", Info1 = "MessageContent", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(detail) });
                }

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }
    }
}
