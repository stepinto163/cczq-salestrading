using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.Cust;
using STC.Common.Handler.FinQ;
using STC.Common.Handler.OwnExec;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Handler.TBook;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Common;
using STC.Common.Model.CRM;
using STC.Common.Model.Cust;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Order.InnerPanel;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Common.ViewModel.WorkTable;
using STC.Frame.Common;
using STC.Frame.CustomUI.Component;
using STC.Frame.Plugin;
using STC.Frame.UI;
using STC.Frame.Utils;
using STC.Frame.Utils.ExcelHelper;
using STC.WorkBench.Plugin.Utils;
using STC.WorkBench.Plugin.Utils.Data;
using STC.WorkBench.Plugin.View;
using STC.WorkBench.Plugin.View.WorkTable;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static STC.WorkBench.Plugin.View.Component.AutoCompleteComboBox;
using WtUtils = STC.WorkBench.Plugin.Utils.CommonUtils;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public enum TBookStatusEnum
    {
        ///// <summary>
        ///// 待处理
        ///// </summary>
        //Working,

        ///// <summary>
        ///// 已处理
        ///// </summary>
        //Worked,

        /// <summary>
        /// 我的需求
        /// </summary>
        MyIntention,

        /// <summary>
        /// 已忽略
        /// </summary>
        Ignore,
    }

    public class MyOrderDefaultViewModel : ViewModelBase
    {
        /// <summary>
        /// 1:做市 2:销交
        /// </summary>
        private string _innerType = "1";

        private bool _ConfirmEnable = true;
        public bool ConfirmEnable
        {
            get
            {
                return _ConfirmEnable;
            }
            set
            {
                _ConfirmEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _QuickConfirmEnable = true;
        public bool QuickConfirmEnable
        {
            get
            {
                return _QuickConfirmEnable;
            }
            set
            {
                _QuickConfirmEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _TGEnable = true;
        public bool TGEnable
        {
            get
            {
                return _TGEnable;
            }
            set
            {
                _TGEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IgnoreEnable = true;
        public bool IgnoreEnable
        {
            get
            {
                return _IgnoreEnable;
            }
            set
            {
                _IgnoreEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _RecoverEnable = true;
        public bool RecoverEnable
        {
            get
            {
                return _RecoverEnable;
            }
            set
            {
                _RecoverEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _RefEnable = true;

        public bool RefEnable
        {
            get
            {
                return _RefEnable;
            }
            set
            {
                _RefEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _ToNewEnable = true;

        public bool ToNewEnable
        {
            get { return _ToNewEnable; }
            set
            {
                _ToNewEnable = value;
                this.OnPropertyChanged();
            }
        }


        private IntentionQuoteItem _selectedItem_Working;
        public IntentionQuoteItem SelectedItem_Working
        {
            get
            {
                return this._selectedItem_Working;
            }

            set
            {
                this._selectedItem_Working = value;
                this.OnPropertyChanged();
                Price_Edit = null;
                Qty_Edit = null;
                
            }
        }

        private IList _selectedItems_Working;
        private List<IntentionQuoteItem> workingSelectedItems;
        private object _workingSelectedItemsLock = new object();
        public IList SelectedItems_Working
        {
            get
            {
                return this._selectedItems_Working;
            }

            set
            {
                this._selectedItems_Working = value;
                this.OnPropertyChanged();
                lock (_workingSelectedItemsLock)
                {
                    if (value != null)
                    {
                        workingSelectedItems = value.Cast<IntentionQuoteItem>().ToList();
                    }
                    else
                    {
                        if (workingSelectedItems != null)
                        {
                            workingSelectedItems.Clear();
                            workingSelectedItems = null;
                        }
                    }
                }

                if (value != null && value.Count == 1)
                {
                    InquiryBatch_IsEnabled = true;
                }
                else
                {
                    InquiryBatch_IsEnabled = false;
                }

                NotifyFilter();
                SetSearchDes();
                this.InquiryViewSource.View?.Refresh();
            }
        }

        private bool _InquiryBatch_IsEnabled = false;

        public bool InquiryBatch_IsEnabled
        {
            get { return _InquiryBatch_IsEnabled; }
            set
            {
                _InquiryBatch_IsEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsEditPrice = false;

        public bool IsEditPrice
        {
            get { return _IsEditPrice; }
            set
            {
                _IsEditPrice = value;
                this.OnPropertyChanged();
            }
        }

        private void SetSearchDes()
        {
            StringBuilder sb = new StringBuilder();
            //if (!string.IsNullOrEmpty(this.FilterText))
            //{
            //    sb.Append(this.FilterText);
            //}
            if (this.SelectedItems_Working != null && this.SelectedItems_Working.Count > 0)
            {
                foreach (IntentionQuoteItem item in SelectedItems_Working)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("/");
                    }
                    sb.Append(item.SecurityId);
                }
            }
            string text = string.Empty;
            if (sb.Length > 0)
            {
                text = $"搜索:{sb}";
            }
            SetDesSource(this.DesSource, "搜索", text);
        }

        //private IntentionQuoteItem _selectedItem_Worked;
        //public IntentionQuoteItem SelectedItem_Worked
        //{
        //    get
        //    {
        //        return this._selectedItem_Worked;
        //    }

        //    set
        //    {
        //        this._selectedItem_Worked = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private IntentionQuoteItem _selectedItem_Ignore;
        //public IntentionQuoteItem SelectedItem_Ignore
        //{
        //    get
        //    {
        //        return this._selectedItem_Ignore;
        //    }

        //    set
        //    {
        //        this._selectedItem_Ignore = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //// 机构号
        //private ObservableCollection<KeyValueData> _bankList = new ObservableCollection<KeyValueData>();
        //public ObservableCollection<KeyValueData> BankList
        //{
        //    get
        //    {
        //        if (this._bankList.Count == 0)
        //        {
        //            this._bankList.Add(new KeyValueData() { Key = "BOC", Value = "中国银行" });
        //            this._bankList.Add(new KeyValueData() { Key = "CCB", Value = "建设银行" });
        //            this._bankList.Add(new KeyValueData() { Key = "ICBC", Value = "工商银行" });
        //            this._bankList.Add(new KeyValueData() { Key = "ABOC", Value = "农业银行" });
        //            this._bankList.Add(new KeyValueData() { Key = "JTYH", Value = "交通银行" });
        //        }

        //        return this._bankList;
        //    }

        //    set
        //    {
        //        this._bankList = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private object _bankListLock = new object();
        //private CollectionViewSource _BankListViewSource = new CollectionViewSource();
        //public CollectionViewSource BankListViewSource
        //{
        //    get { return _BankListViewSource; }
        //    set { _BankListViewSource = value; }
        //}

        //// 交易员
        //private ObservableCollection<KeyValueData> _userList = new ObservableCollection<KeyValueData>();
        //public ObservableCollection<KeyValueData> UserList
        //{
        //    get
        //    {
        //        if (this._userList.Count == 0)
        //        {
        //            this._userList.Add(new KeyValueData() { Key = "001", Value = "李客户" });
        //            this._userList.Add(new KeyValueData() { Key = "002", Value = "刘客户" });
        //            this._userList.Add(new KeyValueData() { Key = "003", Value = "张客户" });
        //            this._userList.Add(new KeyValueData() { Key = "004", Value = "王客户" });
        //            this._userList.Add(new KeyValueData() { Key = "005", Value = "沈客户" });
        //        }
        //        return this._userList;
        //    }
        //    set
        //    {
        //        this._userList = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private object _userListLock = new object();
        //private CollectionViewSource _userListViewSource = new CollectionViewSource();
        //public CollectionViewSource UserListViewSource
        //{
        //    get { return _userListViewSource; }
        //    set { _userListViewSource = value; }
        //}

        ////经纪商
        //private ObservableCollection<KeyValueData> _brokerList = new ObservableCollection<KeyValueData>();
        //public ObservableCollection<KeyValueData> BrokerList
        //{
        //    get
        //    {
        //        if (this._brokerList.Count == 0)
        //        {
        //            this._brokerList.Add(new KeyValueData() { Key = "001", Value = "张岱君" });
        //            this._brokerList.Add(new KeyValueData() { Key = "002", Value = "王阳明" });
        //            this._brokerList.Add(new KeyValueData() { Key = "003", Value = "林炜凯" });
        //            this._brokerList.Add(new KeyValueData() { Key = "004", Value = "李国思" });
        //            this._brokerList.Add(new KeyValueData() { Key = "005", Value = "刘方舟" });
        //        }
        //        return this._brokerList;
        //    }

        //    set
        //    {
        //        this._brokerList = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private object _brokerListLock = new object();
        //private CollectionViewSource _brokerListViewSource = new CollectionViewSource();
        //public CollectionViewSource BrokerListViewSource
        //{
        //    get { return _brokerListViewSource; }
        //    set { _brokerListViewSource = value; }
        //}

        public ICommand ConfirmCommand { get; set; }

        public ICommand QuickConfirmCommand { get; set; }

        public ICommand TGCommand { get; set; }

        public ICommand IgnoreCommand { get; set; }

        public ICommand RefCommand { get; set; }

        public ICommand ToNewCommand { get; set; }

        public ICommand ReorderCommand { get; set; }

        public ICommand ReorderOrderCommand { get; set; }

        public ICommand QuoteIdentifyCommand { get; set; }

        public ICommand ToReferenceCommand { get; set; }

        public ICommand ToFinishCommand { get; set; }

        public ICommand AllConfirmCommand { get; set; }

        public ICommand AllQuickConfirmCommand { get; set; }

        public ICommand AllTGCommand { get; set; }

        public ICommand AllIgnoreCommand { get; set; }

        public ICommand AllRecoverCommand { get; set; }

        public ICommand AllRefCommand { get; set; }

        public ICommand AllToNewCommand { get; set; }

        public ICommand AllToReferenceCommand { get; set; }

        public ICommand AllToFinishCommand { get; set; }

        public ICommand AllToRefInquiryCommand { get; set; }

        public ICommand AllToCancelPublishCommand { get; set; }
        /// <summary>
        /// 编辑行
        /// </summary>
        public ICommand EditRowCommand { get; set; }

        /// <summary>
        /// 确认行
        /// </summary>
        public ICommand SaveRowCommand { get; set; }

        /// <summary>
        // 取消行
        /// </summary>
        public ICommand CancelRowCommand { get; set; }

        /// <summary>
        /// 导出
        /// </summary>
        public ICommand ExportCommand { get; set; }

        /// <summary>
        /// 导入
        /// </summary>
        public ICommand ImportCommand { get; set; }

        /// <summary>
        /// 加入发布名单
        /// </summary>
        public ICommand AddBrokerCommand { get; set; }

        /// <summary>
        /// 取消发布名单
        /// </summary>
        public ICommand CancelBrokerCommand { get; set; }

        /// <summary>
        /// 调用起QQ
        /// </summary>
        public ICommand StartQQCommand { get; set; }

        /// <summary>
        /// 发送报价到QQ单聊对话框
        /// </summary>
        public ICommand StartQQ2Command { get; set; }

        /// <summary>
        /// 刷新数据
        /// </summary>
        public ICommand RefreshCommand { get; set; }

        public ICommand EditPriceCommand { get; set; }

        public ICommand EditQtyCommand { get; set; }

        public ICommand EditInstitutionIDCommand { get; set; }

        public ICommand EditTraderIDCommand { get; set; }

        public ICommand EditVipCommand { get; set; }

        public ICommand EditClientTypeCommand { get; set; }

        public ICommand SearchIntentionDateCommand { get; set; }

        public ICommand SearchDemendDateCommand { get; set; }

        public ICommand AllFinishCommand { get; set; }

        public ICommand ToEditInquiryCommand { get; set; }

        public ICommand ToSaveInquiryCommand { get; set; }

        public ICommand ToCancelEditInquiryCommand { get; set; }

        public ICommand ToRefInquiryCommand { get; set; }

        public ICommand ToRenewInquiryCommand { get; set; }

        private bool _ExportEnabled = true;

        public bool ExportEnabled
        {
            get
            {
                return _ExportEnabled;
            }
            set
            {
                this._ExportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _ImportEnabled = true;

        public bool ImportEnabled
        {
            get
            {
                return _ImportEnabled;
            }
            set
            {
                this._ImportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _RefreshBtnEnable = true;

        public bool RefreshBtnEnable
        {
            get { return _RefreshBtnEnable; }
            set
            {
                _RefreshBtnEnable = value;
                this.OnPropertyChanged();
            }
        }

        protected object _workingLock = new object();
        private CollectionViewSource workingViewSource = new CollectionViewSource();
        public CollectionViewSource WorkingViewSource
        {
            get { return workingViewSource; }
            set { workingViewSource = value; }
        }

        //private object _workedLock = new object();
        //private CollectionViewSource workedViewSource = new CollectionViewSource();
        //public CollectionViewSource WorkedViewSource
        //{
        //    get { return workedViewSource; }
        //    set { workedViewSource = value; }
        //}

        //private object _ignoreLock = new object();
        //private CollectionViewSource ignoreViewSource = new CollectionViewSource();
        //public CollectionViewSource IgnoreViewSource
        //{
        //    get { return ignoreViewSource; }
        //    set { ignoreViewSource = value; }
        //}

        private CustomCollection<IntentionQuoteItem> intentionQuoteInfo = new CustomCollection<IntentionQuoteItem>();

        public CustomCollection<IntentionQuoteItem> IntentionQuoteInfo
        {
            get { return intentionQuoteInfo; }
            set { intentionQuoteInfo = value; }
        }

        ////**************** 参考 **********************
        public const string KEY_FIELD = "Key";
        public const string SECURITYID_FIELD = "securityId";
        public const string UPDATETIME_FIELD = "UpdateTime";
        public const string ITEM_FIELD = "Item";
        public const string ISDISPLAY_FIELD = "IsDisplay";
        public const string REMAININGTIME_FIELD = "RemainingTime";
        public const string SIDE_FIELD = "Side";

        internal object _dataLock_demand = new object();

        // 按照updatetime排序后的数据
        private DataTable _DemandDT;

        public DataTable DemandDT
        {
            get
            {
                lock (_dataLock_demand)
                {
                    if (_DemandDT == null)
                    {
                        _DemandDT = new DataTable();
                        _DemandDT.Columns.Add(KEY_FIELD, typeof(string));
                        _DemandDT.Columns.Add(SECURITYID_FIELD, typeof(string));
                        _DemandDT.Columns.Add(UPDATETIME_FIELD, typeof(string));
                        _DemandDT.Columns.Add(ITEM_FIELD, typeof(IntentionQuoteItem));
                        _DemandDT.Columns.Add(ISDISPLAY_FIELD, typeof(bool));
                        _DemandDT.Columns.Add(REMAININGTIME_FIELD, typeof(string));
                        _DemandDT.Columns.Add(SIDE_FIELD, typeof(string));

                        _DemandDT.PrimaryKey = new DataColumn[] { _DemandDT.Columns[KEY_FIELD] };
                    }
                    return _DemandDT;
                }
            }
        }

        private DataView _DemandDV;

        public DataView DemandDV
        {
            get
            {
                lock (_dataLock_demand)
                {
                    if (_DemandDV == null)
                    {
                        _DemandDV = DemandDT.DefaultView;
                        _DemandDV.RowFilter = $"{ISDISPLAY_FIELD} = true";
                        _DemandDV.Sort = $"{UPDATETIME_FIELD} desc";
                    }
                }
                return _DemandDV;
            }
        }

        /// <summary>
        /// key：securityid 
        /// </summary>
        private ConcurrentDictionary<string, List<DataRow>> _demand_Dic = new ConcurrentDictionary<string, List<DataRow>>(); 


        //********************************************

        ////**************** 显示 **********************
        internal object _dataLock_display = new object();

        private CollectionViewSource _viewSource_display = new CollectionViewSource();
        public CollectionViewSource ViewSource_display
        {
            get { return _viewSource_display; }
            set { _viewSource_display = value; }
        }

        private ObservableCollection<IntentionQuoteItem> _innerOrders_display = new ObservableCollection<IntentionQuoteItem>();

        public ObservableCollection<IntentionQuoteItem> InnerOrders_display
        {
            get { return _innerOrders_display; }
            set { _innerOrders_display = value; }
        }

        // securityid, orderid, IntentionQuoteItem
        internal ConcurrentDictionary<string, IntentionQuoteItem> _Cache_display_Dic = new ConcurrentDictionary<string, IntentionQuoteItem>();

        private int _DemandCount = 0;

        private IntentionQuoteItem _selectedItem_Reference;
        public IntentionQuoteItem SelectedItem_Reference
        {
            get
            {
                return this._selectedItem_Reference;
            }

            set
            {
                this._selectedItem_Reference = value;
                this.OnPropertyChanged();
            }
        }
        //********************************************

        private Predicate<IntentionQuoteItem> pStatus0 { get; } = new Predicate<IntentionQuoteItem>(x => (x.Status == "0" || x.Status == "1" || x.Status == "2" || x.Status == "3" || x.Status == "4") && (x.CloseBy == null || x.CloseBy == LocalEnv.UserInfo.UserName));
        private Predicate<IntentionQuoteItem> pStatus1_2 { get; } = new Predicate<IntentionQuoteItem>(x => (x.Status == "1" || x.Status == "2") && x.CloseBy == LocalEnv.UserInfo.UserName);
        private Predicate<IntentionQuoteItem> pStatus3 { get; } = new Predicate<IntentionQuoteItem>(x => x.Status == "3" && x.CloseBy == LocalEnv.UserInfo.UserName);

        private const string filterProperty = nameof(IntentionQuoteItem.Status);

        //AsyncThreadQueue<ConfirmIntentionQuoteReplyPo> replyUpdateThread;

        private Dictionary<string, string> _sideDic = new Dictionary<string, string>();
        public Dictionary<string, string> SideDic
        {
            get
            {
                if (_sideDic.Count == 0)
                {
                    _sideDic["1"] = "Bid";
                    _sideDic["2"] = "Ofr";
                }
                return _sideDic;
            }
        }

        private int _TotalCount4Working = 0;
        public int TotalCount4Working
        {
            get
            {
                return _TotalCount4Working;
            }
            set
            {
                _TotalCount4Working = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4BidWorking = 0;

        public int TotalCount4BidWorking
        {
            get
            {
                return _TotalCount4BidWorking;
            }
            set
            {
                _TotalCount4BidWorking = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4OfrWorking = 0;

        public int TotalCount4OfrWorking
        {
            get
            {
                return _TotalCount4OfrWorking;
            }
            set
            {
                _TotalCount4OfrWorking = value;
                this.OnPropertyChanged();
            }
        }

        private int _BidC = 0;

        public int BidC
        {
            get
            {
                return _BidC;
            }
            set
            {
                _BidC = value;
                if (value > 0)
                {
                    this.BidCV = Visibility.Visible;
                }
                else
                {
                    this.BidCV = Visibility.Collapsed;
                }
                if (value > 0 || OfrC > 0)
                {
                    this.RedPointV = Visibility.Visible;
                }
                else
                {
                    this.RedPointV = Visibility.Collapsed;
                }

                this.OnPropertyChanged();
            }
        }

        private int _OfrC = 0;

        public int OfrC
        {
            get
            {
                return _OfrC;
            }
            set
            {
                _OfrC = value;
                if (value > 0)
                {
                    this.OfrCV = Visibility.Visible;
                }
                else
                {
                    this.OfrCV = Visibility.Collapsed;
                }
                if (value > 0 || _BidC > 0)
                {
                    this.RedPointV = Visibility.Visible;
                }
                else
                {
                    this.RedPointV = Visibility.Collapsed;
                }
                this.OnPropertyChanged();
            }
        }

        private Visibility _redPointV = Visibility.Collapsed;

        public Visibility RedPointV
        {
            get
            {
                return _redPointV;
            }
            set
            {
                _redPointV = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _bidCV = Visibility.Collapsed;

        public Visibility BidCV
        {
            get
            {
                return _bidCV;
            }
            set
            {
                _bidCV = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _ofrCV = Visibility.Collapsed;

        public Visibility OfrCV
        {
            get
            {
                return _ofrCV;
            }
            set
            {
                _ofrCV = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4Demand = 0;
        public int TotalCount4Demand
        {
            get
            {
                return _TotalCount4Demand;
            }
            set
            {
                _TotalCount4Demand = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4BidDemand = 0;

        public int TotalCount4BidDemand
        {
            get
            {
                return _TotalCount4BidDemand;
            }
            set
            {
                _TotalCount4BidDemand = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4OfrDemand = 0;

        public int TotalCount4OfrDemand
        {
            get
            {
                return _TotalCount4OfrDemand;
            }
            set
            {
                _TotalCount4OfrDemand = value;
                this.OnPropertyChanged();
            }
        }

        private int _PublishBtnH = 0;//发布
        public int PublishBtnH
        {
            get
            {
                return _PublishBtnH;
            }
            set
            {
                _PublishBtnH = value;
                this.OnPropertyChanged();
            }
        }

        private int _RefBtnH = 0;
        public int RefBtnH
        {
            get
            {
                return _RefBtnH;
            }
            set
            {
                _RefBtnH = value;
                this.OnPropertyChanged();
            }
        }

        private int _HiddenBtnH = 0;
        public int HiddenBtnH
        {
            get
            {
                return _HiddenBtnH;
            }
            set
            {
                _HiddenBtnH = value;
                this.OnPropertyChanged();
            }
        }

        private int _RecoverBtnH = 0;
        public int RecoverBtnH
        {
            get
            {
                return _RecoverBtnH;
            }
            set
            {
                _RecoverBtnH = value;
                this.OnPropertyChanged();
            }
        }

        private int _AddBrokerBtnH = 0;
        public int AddBrokerBtnH
        {
            get
            {
                return _AddBrokerBtnH;
            }
            set
            {
                _AddBrokerBtnH = value;
                this.OnPropertyChanged();
            }
        }

        private int _CancelBrokerBtnH = 0;
        public int CancelBrokerBtnH
        {
            get
            {
                return _CancelBrokerBtnH;
            }
            set
            {
                _CancelBrokerBtnH = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsEnable_StartQQBtn = false;

        public bool IsEnable_StartQQBtn
        {
            get
            {
                return _IsEnable_StartQQBtn;
            }
            set
            {
                _IsEnable_StartQQBtn = value;
                this.OnPropertyChanged();
            }
        }

        #region 过滤

        private string _DisplayMode;

        public string DisplayMode
        {
            get
            {
                return _DisplayMode;
            }
            set
            {
                _DisplayMode = value;
                this.OnPropertyChanged();
                ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                this.WorkingViewSource.View?.Refresh();
                NotifyFilter();
            }
        }

        private ObservableCollection<DescriptionItem> _desSource = new ObservableCollection<DescriptionItem>();

        public ObservableCollection<DescriptionItem> DesSource
        {
            get { return _desSource; }
            set { _desSource = value; this.OnPropertyChanged(); }
        }

        private void SetDesSource(ObservableCollection<DescriptionItem> datas, string key, string value)
        {
            if (key == Consts.BONDTYPE || key == Consts.BOND_SUB_TYPE2)
                return;//key = Consts.BOND_SUB_TYPE;

            if (string.IsNullOrEmpty(value))
            {
                if (datas.Any(item => item.Key == key))
                {
                    DescriptionItem removeItem = datas.First(item => item.Key == key);
                    datas.Remove(removeItem);
                }
            }
            else
            {
                if (datas.Any(item => item.Key == key))
                {
                    DescriptionItem updateItem = datas.First(item => item.Key == key);
                    updateItem.Title = value;
                }
                else
                {
                    datas.Add(new DescriptionItem(value, key));
                }
            }
        }


        private bool _IsAppTerm;

        public bool IsAppTerm
        {
            get { return _IsAppTerm; }
            set
            {
                _IsAppTerm = value;

                if (this.SelectedItem_Tag != null && this.SelectedItem_Tag.IsAppTerm != value)
                {
                    this.SelectedItem_Tag = null;
                }

                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();
            }
        }

        private string _startTermDY = string.Empty;

        public string StartTermDY
        {
            get { return _startTermDY; }
            set
            {
                if (_startTermDY != value)
                {
                    _startTermDY = value;
                    this.OnPropertyChanged();

                    if (_focusTagDic == null
                        || (_focusTagDic.ContainsKey(Consts.TERM) && _focusTagDic[Consts.TERM].Count == 0))
                    {
                        string text = string.Empty;
                        if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                        {
                            text = $"剩余期限:{StartTermDY}-{EndTermDY}";
                        }
                        SetDesSource(this.DesSource, Consts.TERM, text);
                    }
                    if (!string.IsNullOrEmpty(value))
                    {
                        this.SelectedItem_Working = null;
                    }

                    WriteDBLogHelper.Instance.GenRecord("personal", "027", "剩余期限-开始");
                }
            }
        }

        private string _endTermDY = string.Empty;

        public string EndTermDY
        {
            get { return _endTermDY; }
            set
            {
                if (_endTermDY != value)
                {
                    _endTermDY = value;
                    this.OnPropertyChanged();
                    //this.WorkingViewSource.View?.Refresh();
                    NotifyFilter();

                    if (_focusTagDic == null
                        || (_focusTagDic.ContainsKey(Consts.TERM) && _focusTagDic[Consts.TERM].Count == 0))
                    {
                        string text = string.Empty;
                        if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                        {
                            text = $"剩余期限:{StartTermDY}-{EndTermDY}";
                        }
                        SetDesSource(this.DesSource, Consts.TERM, text);
                    }
                    if (!string.IsNullOrEmpty(value))
                    {
                        this.SelectedItem_Working = null;
                    }

                    WriteDBLogHelper.Instance.GenRecord("personal", "028", "剩余期限-结束");
                }
                
            }
        }

        private bool _isCoupon;

        public bool IsCoupon
        {
            get { return _isCoupon; }
            set
            {
                _isCoupon = value;

                if (this.SelectedItem_Tag != null && this.SelectedItem_Tag.IsCoupon != value)
                {
                    this.SelectedItem_Tag = null;
                }

                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();
            }
        }

        private string _startCoupon;

        public string StartCoupon
        {
            get { return _startCoupon; }
            set
            {
                _startCoupon = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartCoupon) || !string.IsNullOrEmpty(EndCoupon))
                {
                    text = $"票面:{StartCoupon}-{EndCoupon}";
                }
                SetDesSource(this.DesSource, "Coupon", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        private string _endCoupon;

        public string EndCoupon
        {
            get { return _endCoupon; }
            set
            {
                _endCoupon = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartCoupon) || !string.IsNullOrEmpty(EndCoupon))
                {
                    text = $"票面:{StartCoupon}-{EndCoupon}";
                }
                SetDesSource(this.DesSource, "Coupon", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        private string _startChinaBondValue;

        public string StartChinaBondValue
        {
            get { return _startChinaBondValue; }
            set
            {
                _startChinaBondValue = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartChinaBondValue) || !string.IsNullOrEmpty(EndChinaBondValue))
                {
                    text = $"中债估值:{StartChinaBondValue}-{EndChinaBondValue}";
                }
                SetDesSource(this.DesSource, "ChinaBondValue", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        private string _endChinaBondValue;

        public string EndChinaBondValue
        {
            get { return _endChinaBondValue; }
            set
            {
                _endChinaBondValue = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartChinaBondValue) || !string.IsNullOrEmpty(EndChinaBondValue))
                {
                    text = $"中债估值:{StartChinaBondValue}-{EndChinaBondValue}";
                }
                SetDesSource(this.DesSource, "ChinaBondValue", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        private bool _isPrice;

        public bool IsPrice
        {
            get { return _isPrice; }
            set
            {
                _isPrice = value;

                if (this.SelectedItem_Tag != null && this.SelectedItem_Tag.IsPrice != value)
                {
                    this.SelectedItem_Tag = null;
                }

                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();
            }
        }

        private string _startPrice;

        public string StartPrice
        {
            get { return _startPrice; }
            set
            {
                _startPrice = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartPrice) || !string.IsNullOrEmpty(EndPrice))
                {
                    text = $"报价:{StartPrice}-{EndPrice}";
                }
                SetDesSource(this.DesSource, "Price", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        private string _endPrice;

        public string EndPrice
        {
            get { return _endPrice; }
            set
            {
                _endPrice = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartPrice) || !string.IsNullOrEmpty(EndPrice))
                {
                    text = $"报价:{StartPrice}-{EndPrice}";
                }
                SetDesSource(this.DesSource, "Price", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }


        private string _StartBidBp;

        public string StartBidBp
        {
            get { return _StartBidBp; }
            set
            {
                _StartBidBp = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartBidBp) || !string.IsNullOrEmpty(EndBidBp))
                {
                    text = $"bid-中债:{StartBidBp}-{EndBidBp}";
                }
                SetDesSource(this.DesSource, "BidBp", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        private string _EndBidBp;

        public string EndBidBp
        {
            get { return _EndBidBp; }
            set
            {
                _EndBidBp = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartBidBp) || !string.IsNullOrEmpty(EndBidBp))
                {
                    text = $"bid-中债:{StartBidBp}-{EndBidBp}";
                }
                SetDesSource(this.DesSource, "BidBp", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        private string _StartOfrBp;

        public string StartOfrBp
        {
            get { return _StartOfrBp; }
            set
            {
                _StartOfrBp = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartOfrBp) || !string.IsNullOrEmpty(EndOfrBp))
                {
                    text = $"中债-ofr:{StartOfrBp}-{EndOfrBp}";
                }
                SetDesSource(this.DesSource, "OfrBp", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        private string _EndOfrBp;

        public string EndOfrBp
        {
            get { return _EndOfrBp; }
            set
            {
                _EndOfrBp = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartOfrBp) || !string.IsNullOrEmpty(EndOfrBp))
                {
                    text = $"中债-ofr:{StartOfrBp}-{EndOfrBp}";
                }
                SetDesSource(this.DesSource, "OfrBp", text);
                if (!string.IsNullOrEmpty(value))
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        private string _focusTags;

        public string FocusTags
        {
            get { return _focusTags; }
            set
            {
                if (_focusTags != value)
                {
                    _focusTags = value;

                    try
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            _focusTagDic = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(value);
                        }
                        else
                        {
                            _focusTagDic = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(string.Empty);
                        }
                    }
                    catch (Exception)
                    {
                        _focusTagDic = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(string.Empty);
                    }

                    if (this.SelectedItem_Tag != null && this.SelectedItem_Tag.FocusTags != value)
                    {
                        this.SelectedItem_Tag = null;
                    }

                    this.OnPropertyChanged();
                    //this.WorkingViewSource.View?.Refresh();
                    NotifyFilter();
                }

                if (_focusTagDic != null)
                {
                    foreach (KeyValuePair<string, List<string>> pair in _focusTagDic)
                    {
                        StringBuilder text = new StringBuilder();
                        if (pair.Value != null && pair.Value.Count > 0)
                        {
                            if (pair.Key == Consts.RANKING_TYPE)
                            {
                                text.Append("主体:");
                            }
                            if (pair.Key == Consts.RATING_CURRENT)
                            {
                                text.Append("债项:");
                            }
                            else if (pair.Key == Consts.ISSUER_RATING_CURRENT_CBR)
                            {
                                text.Append("中债资信:");
                            }
                            else if (pair.Key == Consts.CDC_IMPLITED_RATING)
                            {
                                text.Append("中债隐含:");
                            }
                            else if (pair.Key == Consts.CSI_IMPLIED_RATING)
                            {
                                text.Append("中证隐含:");
                            }
                        }
                        foreach (string filterValue in pair.Value)
                        {
                            string temp = string.Empty;
                            if (pair.Key == Consts.BOND_SUB_TYPE && filterValue.StartsWith("x"))
                            {
                                temp = STC.Common.Utils.CommonUtils.GetTagNameByCode(filterValue.Substring(1), Consts.BOND_SUB_TYPE2);
                            }
                            else if (filterValue != "xx")
                            {
                                temp = STC.Common.Utils.CommonUtils.GetTagNameByCode(filterValue, pair.Key);
                            }
                            else
                            {
                                temp = "其他";
                            }
                            text.Append(STC.Common.Utils.CommonUtils.ConvertDes(pair.Key, temp));
                            text.Append("/");
                        }

                        if (text.Length > 0)
                        {
                            text.Remove(text.Length - 1, 1);
                        }

                        if (pair.Key == Consts.TERM && text.Length == 0)
                        {
                            if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                            {
                                text.Append($"剩余期限:{StartTermDY}-{EndTermDY}");
                            }
                        }

                        SetDesSource(this.DesSource, pair.Key, text.ToString());
                        
                    }
                    this.SelectedItem_Working = null;
                }
                else
                {
                    foreach(string key in  BaseHandler.Instance.GetQuoteTagDic().Keys)
                    {
                        SetDesSource(this.DesSource, key, string.Empty);
                    }
                }
            }
        }

        private ConcurrentDictionary<string, List<string>> _focusTagDic;

        private bool _isMe;

        public bool IsMe
        {
            get { return _isMe; }
            set
            {
                _isMe = value;
                this.OnPropertyChanged();
                //this.WorkingViewSource.View?.Refresh();
                NotifyFilter();

                string text = value ? "我" : string.Empty;
                SetDesSource(this.DesSource, "我", text);
                if (value == true)
                {
                    this.SelectedItem_Working = null;
                }
            }
        }

        string[] rankingtype_arr = new string[] { "00001", "00010", "00011", "00100", "00101", "00110" };
        string[] ratingCurrent_arr = new string[] { "00001", "00010", "00011", "00100", "00101", "00110" };
        string[] cdcImplitedRating_arr = new string[] { "00001", "00010", "00011", "00100", "00101", "00110", "01000" };

        private void CheckSign(IntentionQuoteItem data, string key, string value, ref bool result, ConcurrentDictionary<string, List<string>> focusTagDic, string type = null)
        {
            if (focusTagDic == null)
            {
                result = true;
            }
            else if (key == Consts.SIDE)
            {
                result = true;
            }
            else if (key == Consts.ISSUER)
            {
                List<string> binaryList = new List<string>();
                string[] arr = null;
                if (!string.IsNullOrEmpty(data.IssuerCode))
                {
                    arr = data.IssuerCode.Split(CommonConstants.BAR_CHAR, StringSplitOptions.RemoveEmptyEntries);
                }

                if (focusTagDic.ContainsKey(key))
                {
                    foreach (string ic in focusTagDic[key])
                    {
                        ChildTag ct = STC.Common.Utils.CommonUtils.GetIssuer(ic);
                        if (ct != null)
                        {
                            binaryList.Add(ct.childTag);
                        }

                    }
                }

                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || binaryList.Any(item => arr != null && arr.Contains(item)));
            }
            else if (key == Consts.GUARANTOR)
            {
                string[] arr = null;
                List<string> binaryList = new List<string>();
                if (!string.IsNullOrEmpty(data.WarranterCode))
                {
                    arr = data.WarranterCode.Split(CommonConstants.BAR_CHAR, StringSplitOptions.RemoveEmptyEntries);
                }

                if (focusTagDic.ContainsKey(key))
                {
                    foreach (string ic in focusTagDic[key])
                    {
                        ChildTag ct = STC.Common.Utils.CommonUtils.GetGuarantor(ic);
                        if (ct != null)
                        {
                            binaryList.Add(ct.childTag);
                        }
                    }
                }

                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || binaryList.Any(item => arr != null && arr.Contains(item)));
            }
            else if (key == Consts.DQTYPE)
            {
                string tempDqtype = value;

                bool isme = data.IsMe;

                if (_innerType == "2" && isme == false)
                {
                    tempDqtype = "0000";
                }
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(tempDqtype));
            }
            else if (key == Consts.CUM_TYPE)
            {
                string cumType = "00";

                if (data.SignDic1 != null && data.SignDic1.ContainsKey(Consts.CUM_TYPE))
                    cumType = data.SignDic1[Consts.CUM_TYPE];


                string ifExe = "00";

                if (data.SignDic3 != null && data.SignDic3.ContainsKey(Consts.IF_EXERCISE))
                    ifExe = data.SignDic3[Consts.IF_EXERCISE];

                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(cumType) || (focusTagDic[key].Contains("11") && ifExe == "10"));
            }
            else if (key == Consts.ORGTYPE)
            {
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value) || (focusTagDic[key].Contains("0000000000000001") && value == "0000000000000000"));
            }
            else if (key == Consts.BOND_SUB_TYPE)
            {
                List<string> oldSubBondTypeList = focusTagDic[key].ToList();
                oldSubBondTypeList.RemoveAll(item => item == "x0001" || item == "x0010");
                int bondSubTypeCount = 0;
                if (focusTagDic.ContainsKey(key))
                {
                    List<string> subBondTypeList = focusTagDic[key].ToList();
                    subBondTypeList.RemoveAll(item => item == "1011" || item == "1100" || item == "1101" || item == "1110" || item == "1111" || item == "x0001" || item == "x0010");
                    bondSubTypeCount = subBondTypeList.Count;
                }

                result = result
                    && (!focusTagDic.ContainsKey(key) || bondSubTypeCount == 0 || bondSubTypeCount == oldSubBondTypeList.Count)
                    && (!focusTagDic.ContainsKey(key) || oldSubBondTypeList.Count == 0 || oldSubBondTypeList.Contains(value))
                    && (!focusTagDic.ContainsKey(Consts.BONDTYPE) || focusTagDic[Consts.BONDTYPE].Count == 0 || !focusTagDic.ContainsKey(key) || focusTagDic[Consts.BONDTYPE].Count == oldSubBondTypeList.Count);
            }
            else if (key == Consts.RANKING_TYPE)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("xx") && !rankingtype_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            }
            else if (key == Consts.RATING_CURRENT)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("xx") && !ratingCurrent_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            }
            else if (key == Consts.CDC_IMPLITED_RATING)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("xx") && !cdcImplitedRating_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            }
            else if (key == Consts.ISSUER_RATING_CURRENT_CBR
                || key == Consts.CSI_IMPLIED_RATING)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("01000") && value == "00000" && !"BNC".Equals(data.SecurityType)); // 00000也算其他
            }
            else
            {
                //result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(InnerBoardFilterPanel_New.ALL) || focusTagDic[key].Contains(value));
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value));
            }
        }
        #endregion

        private string _IsInquiry;

        public string IsInquiry
        {
            get { return _IsInquiry; }
            set 
            { 
                _IsInquiry = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 分组排序中使用，记录组中最值
        /// key:groupkey  value:当前排序字段
        /// </summary>
        protected ConcurrentDictionary<string, object> _Summary_Dic = new ConcurrentDictionary<string, object>();
        protected string _currentSortColumnName = "UpdateTime";
        protected ListSortDirection _currentListSortDirection = ListSortDirection.Descending;

        /// <summary>
        /// 询价
        /// 分组排序使用， 记录组中最值
        /// key: groupkey   value:当前排序字段
        /// </summary>
        protected ConcurrentDictionary<string, object> _Summary_Dic_Inquiry = new ConcurrentDictionary<string, object>();
        protected string _currentSortColumnName_Inquiry = "UpdateTime";
        protected ListSortDirection _currentListSortDirection_Inquiry = ListSortDirection.Descending;

        public MyOrderDefaultViewModel()
        {
            try
            {
                //for (int i = 0; i < 4; i++)
                //{
                //    IntentionQuoteItem item = new IntentionQuoteItem();
                //    item.IntentionId = i.ConvertToString();
                //    item.InstitutionId = "BOC";
                //    item.InstitutionName = "中国银行";
                //    item.TraderId = "001";
                //    item.TraderName = "李客户";
                //    item.QqNumber = "123456";
                //    item.Side = "1";
                //    item.SecurityId = "180210.IB";
                //    item.Symbol = "18国开10";
                //    item.CleanPrice = 101.6m;
                //    item.FullPrice = 3.186m;
                //    item.Price = 3.186m;
                //    item.Qty = 2000;
                //    item.SettlType = "1";
                //    string tradeDate = CommonUtils.CalTradeDate("1");
                //    item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
                //    DateTime settlDateTime;
                //    int selectIndex;
                //    CommonUtils.SetSettleType("1", tradeDate, out settlDateTime, out selectIndex);
                //    item.edit_settletype = selectIndex;
                //    item.BrokerId = "001";
                //    item.BrokerName = "张岱君";
                //    item.Text = "bid 3.186 AAA 180210 18国开10 2000";
                //    item.Status = i + "";
                //    item.CreateTime = "2021-12-12 12:12:12";
                //    item.UpdateTime = "2021-12-12 12:12:12";
                //    item.CloseBy = "yxl";


                //    this.IntentionQuoteInfo.Add(item);
                //}

                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && !STC.Common.Utils.CommonUtils.HasInquiry())
                {
                    IsInquiry = "0";
                }
                else
                {
                    IsInquiry = LocalEnv.ConfInfo.IsInquiry ? "1" : "0";
                }

                statusFilterList.Clear();
                statusFilterList.Add("0");
                statusFilterList.Add("1");

                if (LocalEnv.UserInfo.RoleType == RoleType.SELLER_ROLE)
                {
                    this.dealTimeBegin = DateTime.Now;
                    this.dealTimeEnd = DateTime.Now;
                }
                //isProgSetTime = true;
                this.dealTimeBegin_demand = DateTime.Now;
                this.dealTimeEnd_demand = DateTime.Now;
                //isProgSetTime = false;
                this.timelimitselectedindex = 0;
                //OnSearchDateCommand();

                if (IntentionOldHandler.Instance.isSALER())
                {
                    _innerType = "2";
                }

                // 参考数据过滤刷新
                if (FilterUIThread == null)
                {
                    FilterUIThread = new Thread(ProcessFilterThread);
                    FilterUIThread.IsBackground = true;
                    FilterUIThread.Start();
                }

                // 延迟刷新
                if (SuspendThread == null)
                {
                    SuspendThread = new Thread(ProcessSuspendThread);
                    SuspendThread.IsBackground = true;
                    SuspendThread.Start();
                }

                ViewSource_Tag.Source = IntentionOldHandler.Instance.GroupList;

                BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);

                BaseHandler.Instance.Init(Instance_OnClear);

                DesignHandler.Instance.OnHotKey += Instance_OnHotKey;

                //if (replyUpdateThread == null)
                //{
                //    replyUpdateThread = new AsyncThreadQueue<ConfirmIntentionQuoteReplyPo>(UpdateConfirmIntentionQuoteRequest);
                //}

                //Thread refreshCheckBoxThread = new Thread(RefreshCheckBox);
                //refreshCheckBoxThread.IsBackground = true;
                //refreshCheckBoxThread.Start();
                IntentionOldHandler.Instance.OnMarketQuotes += Instance_OnMarketQuotes;
                IntentionOldHandler.Instance.OnHistoryMarketQuotes += Instance_OnHistoryMarketQuotes;
                IntentionOldHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;
                IntentionOldHandler.Instance.OnHistoryIntentionQuotes += Instance_OnHistoryIntentionQuotes;

                if (STC.Common.Utils.CommonUtils.IsBDBAppName())
                {
                    LastExecutionProcesser.Processer.OnLastExecutions_BDB -= Processer_OnLastExecutions_BDB;
                    LastExecutionProcesser.Processer.OnLastExecutions_BDB += Processer_OnLastExecutions_BDB;
                }
                else
                {
                    //订阅最后成交处理完成事件，当获取历史记录完成后更新界面的最后成交
                    BaseHandler.Instance.SubscribeProctMarketExecutionsStatus(OnLoadtMarketExecutionsStatus);
                }

                //市场最优
                //QueryBrokerDatas();

                //this.BankListViewSource.IsLiveFilteringRequested = true;
                //this.BankListViewSource.IsLiveSortingRequested = true;
                //this.BankListViewSource.Source = BankList;
                //BindingOperations.EnableCollectionSynchronization(this.BankListViewSource.View, _bankListLock);

                //this.UserListViewSource.IsLiveFilteringRequested = true;
                //this.UserListViewSource.IsLiveSortingRequested = true;
                //this.UserListViewSource.Source = UserList;
                //BindingOperations.EnableCollectionSynchronization(this.UserListViewSource.View, _userListLock);

                //this.BrokerListViewSource.IsLiveFilteringRequested = true;
                //this.BrokerListViewSource.IsLiveSortingRequested = true;
                //this.BrokerListViewSource.Source = BrokerList;
                //BindingOperations.EnableCollectionSynchronization(this.BrokerListViewSource.View, _brokerListLock);

                //this.ViewSource_Broker.SortDescriptions.Clear();
                //this.ViewSource_Broker.SortDescriptions.Add(new SortDescription("CreateTime", ListSortDirection.Descending));
                //this.ViewSource_Broker.IsLiveFilteringRequested = true;
                //this.ViewSource_Broker.IsLiveSortingRequested = true;
                //this.ViewSource_Broker.Source = this.InnerOrders_Broker;
                //BindingOperations.EnableCollectionSynchronization(this.ViewSource_Broker.View, this._dataLock_broker);

                
                this.WorkingViewSource.IsLiveFilteringRequested = true;
                this.WorkingViewSource.IsLiveSortingRequested = true;
                this.WorkingViewSource.IsLiveGroupingRequested = true;
                this.WorkingViewSource.LiveFilteringProperties.Add("Status");
                
                
                this.WorkingViewSource.Source = IntentionQuoteInfo;
                this.WorkingViewSource.View.CollectionChanged += WorkingView_CollectionChanged;
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        /*ListCollectionView viewsource = (ListCollectionView)this.WorkingViewSource.View;
                        viewsource.GroupDescriptions.Clear();
                        viewsource.GroupDescriptions.Add(new PropertyGroupDescription("SortKey"));

                        viewsource.SortDescriptions.Clear();
                        viewsource.SortDescriptions.Add(new SortDescription("TempSort", ListSortDirection.Descending));
                        viewsource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));*/

                        this.WorkingViewSource.SortDescriptions.Clear();
                        this.WorkingViewSource.SortDescriptions.Add(new SortDescription("TempSort", ListSortDirection.Descending));
                        this.WorkingViewSource.SortDescriptions.Add(new SortDescription("SortKey", ListSortDirection.Ascending));
                        this.WorkingViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));
                        this.WorkingViewSource.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));

                        /*this.WorkingViewSource.GroupDescriptions.Clear();
                        this.WorkingViewSource.GroupDescriptions.Add(new PropertyGroupDescription("SortKey"));

                        viewsource.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
                        viewsource.CustomSort = new MyCustomComparer("UpdateTime", ListSortDirection.Descending, _Summary_Dic);*/
                    }
                    else
                    {
                        this.WorkingViewSource.SortDescriptions.Clear();
                        this.WorkingViewSource.SortDescriptions.Add(new SortDescription("CreateTime", ListSortDirection.Descending));
                    }

                    (this as IntentionControl_Buy_ViewModel).FilterData();
                }
                else
                {
                    this.WorkingViewSource.SortDescriptions.Clear();
                    this.WorkingViewSource.SortDescriptions.Add(new SortDescription("CreateTime", ListSortDirection.Descending));
                    FilterData();
                }
                BindingOperations.EnableCollectionSynchronization(this.WorkingViewSource.View, _workingLock);


                this.ViewSource_display.IsLiveFilteringRequested = true;
                this.ViewSource_display.IsLiveSortingRequested = true;
                this.ViewSource_display.SortDescriptions.Clear();
                this.ViewSource_display.SortDescriptions.Add(new SortDescription("CreateTime", ListSortDirection.Descending));
                this.ViewSource_display.Source = InnerOrders_display;
                this.ViewSource_display.View.CollectionChanged += View_CollectionChanged;
                BindingOperations.EnableCollectionSynchronization(this.ViewSource_display.View, _dataLock_display);

                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {

                        this.InquiryViewSource.IsLiveFilteringRequested = true;
                        this.InquiryViewSource.IsLiveSortingRequested = true;
                        this.InquiryViewSource.SortDescriptions.Clear();
                        this.InquiryViewSource.LiveFilteringProperties.Add("TempSort");

                        this.InquiryViewSource.SortDescriptions.Clear();
                        this.InquiryViewSource.SortDescriptions.Add(new SortDescription("TempSort", ListSortDirection.Descending));
                        this.InquiryViewSource.SortDescriptions.Add(new SortDescription("IntentionId", ListSortDirection.Ascending));
                        this.InquiryViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));
                        this.InquiryViewSource.SortDescriptions.Add(new SortDescription("Sn", ListSortDirection.Ascending));
                        this.InquiryViewSource.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));

                        this.InquiryViewSource.Source = InquiryQuoteInfo;
                    }
                    else
                    {
                        this.InquiryViewSource.IsLiveFilteringRequested = true;
                        this.InquiryViewSource.IsLiveSortingRequested = true;
                        this.InquiryViewSource.SortDescriptions.Clear();
                        this.InquiryViewSource.LiveFilteringProperties.Add("TempSort");
                        this.InquiryViewSource.SortDescriptions.Add(new SortDescription("Sn", ListSortDirection.Ascending));
                        this.InquiryViewSource.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
                        this.InquiryViewSource.Source = InquiryQuoteInfo;
                    }
                }
                else
                {
                    this.InquiryViewSource.IsLiveFilteringRequested = true;
                    this.InquiryViewSource.IsLiveSortingRequested = true;
                    this.InquiryViewSource.SortDescriptions.Clear();
                    this.InquiryViewSource.LiveFilteringProperties.Add("TempSort");
                    this.InquiryViewSource.SortDescriptions.Add(new SortDescription("Sn", ListSortDirection.Ascending));
                    this.InquiryViewSource.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
                    this.InquiryViewSource.Source = InquiryQuoteInfo;
                }
                FilterInquiryData();
                BindingOperations.EnableCollectionSynchronization(this.InquiryViewSource.View, _inquiryLock);


                this.StartQQCommand = new ComRelayCommand(this.DoStartQQCommand);
                this.StartQQ2Command = new ComRelayCommand(this.DoStartQQ2Command);

                this.ConfirmCommand = new ComRelayCommand<Button>(this.DoConfirmCommand);
                this.QuickConfirmCommand = new ComRelayCommand<Button>(this.DoQuickConfirmCommand);//无用
                this.TGCommand = new ComRelayCommand<Button>(this.DoTGCommand);//无用
                this.IgnoreCommand = new ComRelayCommand<Button>(this.DoIgnoreCommand);//REF
                this.ReorderCommand = new ComRelayCommand<Button>(this.DoReorderCommand);//恢复
                this.RefCommand = new ComRelayCommand<Button>(this.DoRefCommand);//REF(同)
                this.ToNewCommand = new ComRelayCommand<Button>(this.DoNewCommand);//隐藏（无用）
                this.ReorderOrderCommand = new ComRelayCommand<Button>(this.DoRecordOrderCommand);//恢复(同)
                this.ToReferenceCommand = new ComRelayCommand<Button>(this.DoToReferenceCommand);
                this.ToFinishCommand = new ComRelayCommand<Button>(this.DoToFinishCommand);

                this.QuoteIdentifyCommand = new ComRelayCommand(this.DoQuoteIdentifyCommand);//无用
                this.AllConfirmCommand = new ComRelayCommand(this.DoAllConfirmCommand);//发布
                this.AllQuickConfirmCommand = new ComRelayCommand(this.DoAllQuickConfirmCommand);//无用
                this.AllTGCommand = new ComRelayCommand(this.DoAllTGCommand);//无用
                this.AllIgnoreCommand = new ComRelayCommand(this.DoAllIgnoreCommand);//REF
                this.AllRecoverCommand = new ComRelayCommand(this.DoAllRecoverCommand);//恢复
                this.AllRefCommand = new ComRelayCommand(this.DoAllRefCommand);//无用
                this.AllToNewCommand = new ComRelayCommand(this.DoAllNewCommand);//隐藏（无用）
                this.AllToReferenceCommand = new ComRelayCommand(this.DoAllToReferenceCommand);//发个需
                this.AllToFinishCommand = new ComRelayCommand(this.DoAllToFinishCommand);//完成
                this.AllToCancelPublishCommand = new ComRelayCommand(this.DoCancelPublishCommand);//取消发布

                EditRowCommand = new ComRelayCommand<Button>(EditRow);
                SaveRowCommand = new ComRelayCommand<Button>(SaveRow);
                CancelRowCommand = new ComRelayCommand<Button>(CancelRow);

                ExportCommand = new ComRelayCommand(ExportCSV);
                ImportCommand = new ComRelayCommand(ImportCSV);

                AddBrokerCommand = new ComRelayCommand(DoAddBrokerCommand);
                CancelBrokerCommand = new ComRelayCommand(DoCancelBrokerCommand);

                RefreshCommand = new ComRelayCommand(DoRefreshIntentionData);

                EditPriceCommand = new ComRelayCommand<string>(EditPrice);

                EditQtyCommand = new ComRelayCommand<string>(EditQty);

                this.SearchIntentionDateCommand = new ComRelayCommand(OnSearchDateCommand);// （无用）

                this.SearchDemendDateCommand = new ComRelayCommand(OnSearchDateCommand);//（无用）

                this.ToRenewInquiryCommand = new ComRelayCommand<Button>(OnRenewInquiryCommand);// 询价恢复

                this.ToEditInquiryCommand = new ComRelayCommand<Button>(OnEditInquiryCommand);// 询价编辑

                this.ToRefInquiryCommand = new ComRelayCommand<Button>(OnRefInquiryCommand);// 询价REF

                this.ToSaveInquiryCommand = new ComRelayCommand<Button>(OnSaveInquiryCommand);//保存编辑询价

                this.ToCancelEditInquiryCommand = new ComRelayCommand<Button>(OnCancelEditInquiryCommand);//取消编辑询价

                isAdjustAllChkStatus = true;
                DesignHandler.Instance.OnBrokerSetting += Instance_OnBrokerSetting;
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void DoCancelPublishCommand()
        {
            WriteDBLogHelper.Instance.GenRecord("personal", "005", "批量-取消发布");

            Sub_DoCancelPublishCommand(this.SelectedItems_Working);
        }

        internal void Sub_DoCancelPublishCommand(IList selectedDatas)
        {

            if (selectedDatas == null || selectedDatas.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "取消发内部");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量取消发内部操作？", "取消发内部"))
            //{
            //    return;
            //}

            SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                foreach (IntentionQuoteItem item in selectedDatas)
                {
                    if ((item.Status != "1" && item.Status != "0") || item.UserName != LocalEnv.UserInfo.UserName) continue;
                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.IntentionId] = item;
                    //if (item.Status == "0")
                    //{
                    //    GenIntentionInquiryRequest(item);
                    //}
                    //else
                    //{
                    //    item.InquiryRequest = null;
                    //}
                }

                //if (exceptionReplyDic_local.Count > 1)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.IntentionId))
                        {
                            continue;
                        }
                        STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                        request.intentionId = item.IntentionId;
                        request.actionType = "10";// 取消发布
                        request.closeBy = LocalEnv.UserInfo.UserName;
                        request.closeByName = LocalEnv.UserInfo.FullName;
                        request.userName = item.BrokerId;
                        request.updateTime = item.UpdateTime;
                        if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                        {
                            request.quoteType = "BUYER_INTENTION";
                        }
                        else
                        {
                            request.quoteType = "INTENTION";
                        }
                        orderDic[item.IntentionId].Request = request;

                        //if (item.InquiryRequest != null)
                        //{
                        //    GetErrorInfo_Inquiry(item.InquiryRequest,
                        //        ref failCount,
                        //        ref totalCount,
                        //        warnReplyDic,
                        //        prohibitReplyDic,
                        //        exceptionReplyDic);
                        //}
                        //else
                        //{
                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        //}

                        //System.Threading.ThreadPool.QueueUserWorkItem(x =>
                        //{
                        //    Thread.Sleep(2000);

                        //    DateTime intentionDT_Start = this.DealTimeBegin ?? DateTime.Now;
                        //    DateTime intentionDT_End = (this.DealTimeEnd ?? DateTime.Now).AddDays(1);
                        //    IntentionOldHandler.Instance.RequestMarketQuotes_IntentionId(item.IntentionId, intentionDT_Start.ToString("yyyy-MM-dd"), intentionDT_End.ToString("yyyy-MM-dd"));
                        //});

                        try
                        {
                            Thread.Sleep(20);
                        }
                        catch (Exception ex)
                        { }
                    }

                    while ((totalCount + failCount_local) != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("Sub_DoCancelPublishCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        /// <summary>
        /// 生成询价删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="groupKey"></param>
        /// <param name="inquiryNotRefList"></param>
        /// <param name="currentInquiryItem"></param>
        /// <returns></returns>
        protected IntentionInquiryRequest GenIntentionInquiryRequest(string key, string groupKey, List<string> inquiryNotRefList, IntentionMarketItem currentInquiryItem = null)
        {
            if (!string.IsNullOrEmpty(key) && _Cache_InquiryQuote_Dic.ContainsKey(key) && _Cache_InquiryQuote_Dic[key].Count > 0)
            {
                IntentionInquiryRequest req = new IntentionInquiryRequest();
                req.action = "3";
                req.biztypeid = LocalEnv.UserInfo.Termids;
                req.inquiryKey = key;
                req.intentionId = key;
                req.userName = LocalEnv.UserInfo.UserName;
                List<TableIntentionInquiry> inquiryList = new List<TableIntentionInquiry>();
                if (!string.IsNullOrEmpty(key) && _Cache_InquiryQuote_Dic.ContainsKey(key))
                {
                    foreach (IntentionMarketItem inquiryItem in _Cache_InquiryQuote_Dic[key].Values)
                    {
                        if (inquiryItem.Id.EndsWith("_内部") || inquiryItem.SendType == 3 || inquiryItem.OptionType == "sendIntentQQGroups")
                        {
                            continue;
                        }

                        if (currentInquiryItem != null && currentInquiryItem.Id != inquiryItem.Id)
                        {
                            continue;
                        }

                        if (inquiryItem.Status != "1")
                        {
                            continue;
                        }

                        TableIntentionInquiry tableIntentionInquiry = new TableIntentionInquiry();
                        tableIntentionInquiry.id = inquiryItem.Id;
                        tableIntentionInquiry.status = inquiryItem.Status;
                        if (inquiryNotRefList != null && inquiryNotRefList.Contains(inquiryItem.Id))
                        {
                            tableIntentionInquiry.action = "6";
                        }
                        else
                        {
                            tableIntentionInquiry.action = "5";
                        }
                        tableIntentionInquiry.biztypeid = LocalEnv.UserInfo.Termids;
                        tableIntentionInquiry.securityId = inquiryItem.SecurityId;
                        tableIntentionInquiry.institutionId = string.Empty;//????
                        tableIntentionInquiry.institutionName = string.Empty;//????
                        tableIntentionInquiry.traderId = string.Empty;//????
                        tableIntentionInquiry.traderName = string.Empty;//????
                        tableIntentionInquiry.intentionId = key;
                        tableIntentionInquiry.inquiryKey = key;
                        string side = inquiryItem.Side;
                        if (inquiryItem.Side == "3")
                        {
                            side = "1";
                        }
                        else if (inquiryItem.Side == "4")
                        {
                            side = "2";
                        }

                        tableIntentionInquiry.side = side;
                        tableIntentionInquiry.price = inquiryItem.Price?.ToString("0.####").ConvertToDecimal();
                        tableIntentionInquiry.qty = inquiryItem.Qty?.ToString("0").ConvertToDecimal();
                        tableIntentionInquiry.settlType = inquiryItem.SettlType;
                        //string groupKey = $"{item.QuoteType}_{item.Trader_Buyer}_{item.SecurityId}_{item.Side}";
                        tableIntentionInquiry.strategyOrderId = groupKey;
                        tableIntentionInquiry.clOrdID = inquiryItem.ClOrdID;
                        tableIntentionInquiry.execOrderId = inquiryItem.ExecOrderId;
                        tableIntentionInquiry.venue = inquiryItem.Venue;
                        tableIntentionInquiry.venueTypeGW = inquiryItem.VenueTypeGW;
                        if (inquiryItem.Receiver_Edit != null && inquiryItem.Receiver_Edit.OptionType == "sendIntentBrokers")
                        {
                            tableIntentionInquiry.receiverNumber = inquiryItem.Receiver_Edit.Broker.qq; //"3004330557";// todo: 
                            tableIntentionInquiry.receiverName = inquiryItem.Receiver_Edit.Broker.name;
                            tableIntentionInquiry.info5 = JsonConvert.SerializeObject(inquiryItem.Receiver_Edit);
                            tableIntentionInquiry.sendType = 0;
                        }
                        else if (inquiryItem.Receiver_Edit != null && inquiryItem.Receiver_Edit.OptionType == "sendIntentContacts")
                        {
                            tableIntentionInquiry.receiverNumber = inquiryItem.Receiver_Edit.Sender.qq;
                            tableIntentionInquiry.receiverName = inquiryItem.Receiver_Edit.Sender.name;
                            tableIntentionInquiry.batchReceiverNumberMap = new Dictionary<string, string>();
                            tableIntentionInquiry.batchReceiverNumberMap[inquiryItem.Receiver_Edit.Sender.qq] = inquiryItem.Receiver_Edit.Sender.name;
                            tableIntentionInquiry.info5 = JsonConvert.SerializeObject(inquiryItem.Receiver_Edit);
                            tableIntentionInquiry.sendType = 0;
                        }
                        else if (inquiryItem.Receiver_Edit != null && inquiryItem.Receiver_Edit.OptionType == "sendIntentCounterparties")
                        {
                            tableIntentionInquiry.receiverNumber = inquiryItem.Receiver_Edit.Sender.qq;
                            tableIntentionInquiry.receiverName = inquiryItem.Receiver_Edit.Sender.name;
                            tableIntentionInquiry.batchReceiverNumberMap = new Dictionary<string, string>();
                            tableIntentionInquiry.batchReceiverNumberMap[inquiryItem.Receiver_Edit.Sender.qq] = inquiryItem.Receiver_Edit.Sender.name;
                            tableIntentionInquiry.info5 = JsonConvert.SerializeObject(inquiryItem.Receiver_Edit);
                            tableIntentionInquiry.sendType = 0;
                        }
                        else if (inquiryItem.Receiver_Edit != null && inquiryItem.Receiver_Edit.OptionType == "sendIntentGroups")
                        {
                            if (inquiryItem.Receiver_Edit.Groups != null
                                && inquiryItem.Receiver_Edit.Groups.members != null
                                && inquiryItem.Receiver_Edit.Groups.members.Count > 0)
                            {
                                tableIntentionInquiry.batchReceiverNumberMap = new Dictionary<string, string>();
                                foreach (UserConfig uc in inquiryItem.Receiver_Edit.Groups.members)
                                {
                                    tableIntentionInquiry.batchReceiverNumberMap[uc.qq] = uc.name;
                                }
                            }
                            tableIntentionInquiry.info5 = JsonConvert.SerializeObject(inquiryItem.Receiver_Edit);
                            tableIntentionInquiry.sendType = 2;
                        }
                        else if (inquiryItem.Receiver_Edit != null && inquiryItem.Receiver_Edit.OptionType == "sendIntentQQGroups")
                        {
                            tableIntentionInquiry.receiverGroupOpenId = inquiryItem.Receiver_Edit.QQGroups.groupOpenID;
                            tableIntentionInquiry.receiverGroupName = inquiryItem.Receiver_Edit.QQGroups.groupName;
                            tableIntentionInquiry.info5 = JsonConvert.SerializeObject(inquiryItem.Receiver_Edit);
                            tableIntentionInquiry.sendType = 1;
                            string price = tableIntentionInquiry.price.HasValue ? tableIntentionInquiry.price?.ToString("0.00##") : string.Empty;
                            string qty = tableIntentionInquiry.qty.HasValue ? tableIntentionInquiry.qty?.ToString("0") + "w" : string.Empty;
                            tableIntentionInquiry.text = "REF";
                        }
                        tableIntentionInquiry.senderNumber = Common.Utils.CommonUtils.GetQQNumber(LocalEnv.UserInfo.UserName); //"3003945772";//
                        tableIntentionInquiry.bargainFlag = "0";
                        tableIntentionInquiry.quoteDividedFlag = "N";
                        if (inquiryItem.PFlag_Edit == "1")
                        {
                            tableIntentionInquiry.bargainFlag = "1";
                        }
                        else if (inquiryItem.PFlag_Edit == "2")
                        {
                            tableIntentionInquiry.quoteDividedFlag = "Y";
                        }
                        tableIntentionInquiry.ocoFlag = inquiryItem.OCOFlag;
                        tableIntentionInquiry.userName = LocalEnv.UserInfo.UserName;
                        tableIntentionInquiry.info1 = inquiryItem.Remark;
                        tableIntentionInquiry.createTime = inquiryItem.CreateTime;
                        InquiryInfo3 info3 = new InquiryInfo3();
                        info3.PriceType = inquiryItem.PriceType;
                        tableIntentionInquiry.info3 = JsonConvert.SerializeObject(info3);
                        inquiryList.Add(tableIntentionInquiry);
                    }
                }

                req.dataList = inquiryList;
                return req;
            }
            return null;
        }


        private void Instance_OnMarketQuotes(TableIntentionInquiry obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                {
                    (this as IntentionControl_Buy_ViewModel).OnMarketQuotes(obj);
                }
                else
                {
                    OnMarketQuotes(obj);
                }
            }));
        }


        private void Instance_OnHistoryMarketQuotes(List<TableIntentionInquiry> datas)
        {
            if (datas.Count == 0)
                return;
            LogHelper.Info("OnHistoryMarketQuotes");

            int batchSize = 500;
            var syncLock = new object(); // 新增集合访问锁

            Task.Run(() =>
            {
                try
                {
                    // 获取数据快照
                    List<TableIntentionInquiry> snapshot;
                    lock (datas)
                    {
                        snapshot = datas.ToList(); // 线程安全拷贝
                    }

                    for (int batchIndex = 0; batchIndex < snapshot.Count; batchIndex += batchSize)
                    {
                        var currentBatch = snapshot.Skip(batchIndex).Take(batchSize).ToList();

                        // 异步更新UI（非阻塞）
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var data in currentBatch)
                            {
                                //if (startDate_intention.CompareTo(data.updateTime) <= 0 && endDate_intention.CompareTo(data.updateTime) >= 0)
                                //{
                                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                                    {
                                        (this as IntentionControl_Buy_ViewModel).OnMarketQuotes(data);
                                    }
                                    else
                                    {
                                        OnMarketQuotes(data);
                                    }
                                //}
                            }
                        }, System.Windows.Threading.DispatcherPriority.Send); // 调整优先级

                        // 优化CPU释放策略
                        if (batchIndex % (batchSize * 2) == 0 && batchIndex != 0)
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    // 处理窗口关闭时的正常终止
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex); // UI线程记录日志
                }
            });
        }

        /// <summary>
        /// 询价汇总行， key：groupkey ; value:汇总行
        /// </summary>
        internal ConcurrentDictionary<string, IntentionMarketItem> _Cache_GroupInquiryQuote_Dic = new ConcurrentDictionary<string, IntentionMarketItem>();
        /// <summary>
        /// 询价明细行， key:groupkey;  value:所有明细行
        /// </summary>
        internal ConcurrentDictionary<string, ConcurrentDictionary<string, IntentionMarketItem>> _Cache_InquiryQuote_Dic = new ConcurrentDictionary<string, ConcurrentDictionary<string, IntentionMarketItem>>();

        private CustomCollection<IntentionMarketItem> inquiryQuoteInfo = new CustomCollection<IntentionMarketItem>();

        public CustomCollection<IntentionMarketItem> InquiryQuoteInfo
        {
            get { return inquiryQuoteInfo; }
            set { inquiryQuoteInfo = value; }
        }

        protected object _inquiryLock = new object();
        private CollectionViewSource inquiryViewSource = new CollectionViewSource();
        public CollectionViewSource InquiryViewSource
        {
            get { return inquiryViewSource; }
            set { inquiryViewSource = value; }
        }

        internal void OnMarketQuotes(TableIntentionInquiry obj)
        {
            lock (_inquiryLock)
            {
                IntentionMarketItem item = null;

                if (_Cache_InquiryQuote_Dic.ContainsKey(obj.intentionId) && _Cache_InquiryQuote_Dic[obj.intentionId] != null
                    && _Cache_InquiryQuote_Dic[obj.intentionId].ContainsKey(obj.id) && _Cache_InquiryQuote_Dic[obj.intentionId][obj.id] != null)
                {
                    item = _Cache_InquiryQuote_Dic[obj.intentionId][obj.id];
                }
                if (item != null)
                {
                    if ((obj.receiverName == "内部" && obj.status != "1")
                        || (obj.receiverName != "内部" && obj.status != "1" && obj.status != "4"))
                    {
                        IntentionMarketItem removeItem = null;
                        _Cache_InquiryQuote_Dic[obj.intentionId].TryRemove(obj.id, out removeItem);

                        InquiryQuoteInfo.Remove(removeItem);

                        return;
                    }

                    item.Id = obj.id;
                    item.QuoteId = obj.quoteId;

                    if (JsonSplit.IsJson(obj.info5))
                    {
                        ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);
                        item.Receiver_Edit = toOption;
                        if (toOption != null)
                        {
                            item.ReceiverNumber = toOption.DispayName;
                            item.ReceiverNumber_Edit = toOption.DispayName;
                        }
                    }
                    else
                    {
                        item.ReceiverNumber = obj.receiverName;
                    }
                    item.IntentionId = obj.intentionId;
                    item.SecurityId = obj.securityId;
                    item.SendType = obj.sendType;
                    if (JsonSplit.IsJson(obj.info5))
                    {
                        ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);

                        if (toOption != null)
                        {
                            item.OptionType = toOption.OptionType;
                        }

                    }
                    if (obj.sendType == 3)
                    {
                        item.Side = obj.side == "1" ? "3" : "4";
                    }
                    else
                    {
                        item.Side = obj.side;
                    }
                    
                    item.Price = obj.price;
                    item.Price_Edit = obj.price;
                    item.Qty = obj.qty;
                    item.Qty_Edit = obj.qty;
                    item.Name = obj.name;
                    item.UserName = obj.userName;
                    item.UserId = obj.userId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.SettlType = string.IsNullOrEmpty(obj.settlType) ? "2" : obj.settlType;

                    item.Status = obj.status;
                    item.Action = obj.action;

                    item.PFlag = item.PFlag_Edit = "0";
                    if (obj.bargainFlag == "1")
                    {
                        item.PFlag = item.PFlag_Edit = "1";
                    }
                    else if (obj.quoteDividedFlag == "Y")
                    {
                        item.PFlag = item.PFlag_Edit = "2";
                    }

                    item.OCOFlag = item.OCOFlag_Edit = obj.ocoFlag;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    //item.SenderNumber = obj.senderNumber;
                    //item.ReceiverNumber = obj.receiverNumber;
                    item.Remark = obj.info1;
                    if (obj.receiverName == "内部")
                        item.Sn = 0;
                    else if (obj.status == "1")
                        item.Sn = 1;
                    else
                        item.Sn = 2;
                    item.ClOrdID = obj.clOrdID;
                    item.ExecOrderId = obj.execOrderId;
                    item.Venue = obj.venue;
                    item.VenueTypeGW = obj.venueTypeGW;
                    if (JsonSplit.IsJson(obj.info3))
                    {
                        InquiryInfo3 ii3 = JsonConvert.DeserializeObject<InquiryInfo3>(obj.info3);
                        if (ii3 != null)
                        {
                            item.PriceType = ii3.PriceType;
                        }
                    }
                }
                else
                {
                    if ((obj.receiverName == "内部" && obj.status != "1")
                        || (obj.receiverName != "内部" && obj.status != "1" && obj.status != "4"))
                    {
                        return;
                    }

                    item = new IntentionMarketItem();
                    item.Id = obj.id;
                    item.QuoteId = obj.quoteId;

                    if (JsonSplit.IsJson(obj.info5))
                    {
                        ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);
                        item.Receiver_Edit = toOption;
                        if (toOption != null)
                        {
                            item.ReceiverNumber = toOption.DispayName;
                            item.ReceiverNumber_Edit = toOption.DispayName;
                        }
                    }
                    else
                    {
                        item.ReceiverNumber = obj.receiverName;
                    }

                    item.IntentionId = obj.intentionId;
                    item.SecurityId = obj.securityId;
                    item.SendType = obj.sendType;

                    if (JsonSplit.IsJson(obj.info5))
                    {
                        ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);

                        if (toOption != null)
                        {
                            item.OptionType = toOption.OptionType;
                        }
                    }

                    if (obj.sendType == 3)
                    {
                        item.Side = obj.side == "1" ? "3" : "4";
                    }
                    else
                    {
                        item.Side = obj.side;
                    }

                    item.Price = obj.price;
                    item.Price_Edit = obj.price;
                    item.Qty = obj.qty;
                    item.Qty_Edit = obj.qty;
                    item.Name = obj.name;
                    item.UserName = obj.userName;
                    item.UserId = obj.userId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.SettlType = string.IsNullOrEmpty(obj.settlType) ? "2" : obj.settlType;

                    item.Status = obj.status;
                    item.Action = obj.action;
                    item.PFlag = item.PFlag_Edit = "0";
                    if (obj.bargainFlag == "1")
                    {
                        item.PFlag = item.PFlag_Edit = "1";
                    }
                    else if (obj.quoteDividedFlag == "Y")
                    {
                        item.PFlag = item.PFlag_Edit = "2";
                    }
                    item.OCOFlag = item.OCOFlag_Edit = obj.ocoFlag;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    //item.SenderNumber = obj.senderNumber;
                    //item.ReceiverNumber = obj.receiverNumber;
                    item.Remark = obj.info1;
                    if (obj.receiverName == "内部")
                        item.Sn = 0;
                    else if (obj.status == "1")
                        item.Sn = 1;
                    else
                        item.Sn = 2;
                    item.ClOrdID = obj.clOrdID;
                    item.ExecOrderId = obj.execOrderId;
                    item.Venue = obj.venue;
                    item.VenueTypeGW = obj.venueTypeGW;
                    if (JsonSplit.IsJson(obj.info3))
                    {
                        InquiryInfo3 ii3 = JsonConvert.DeserializeObject<InquiryInfo3>(obj.info3);
                        if (ii3 != null)
                        {
                            item.PriceType = ii3.PriceType;
                        }
                    }

                    InquiryQuoteInfo.Add(item);

                    if (!_Cache_InquiryQuote_Dic.ContainsKey(obj.intentionId))
                    {
                        _Cache_InquiryQuote_Dic[obj.intentionId] = new ConcurrentDictionary<string, IntentionMarketItem>();
                    }
                    if (!_Cache_InquiryQuote_Dic[obj.intentionId].ContainsKey(obj.id))
                    {
                        _Cache_InquiryQuote_Dic[obj.intentionId][obj.id] = item;
                    }
                }

                if (_Cache_InnerOrders_Dic.ContainsKey(obj.securityId) && _Cache_InnerOrders_Dic[obj.securityId] != null
                     && _Cache_InnerOrders_Dic[obj.securityId].ContainsKey(obj.intentionId) && _Cache_InnerOrders_Dic[obj.securityId][obj.intentionId] != null)
                {
                    IntentionQuoteItem intentionItem = _Cache_InnerOrders_Dic[obj.securityId][obj.intentionId];
                    GenStatusDisplay(intentionItem);
                }
            }
        }

        protected void GenStatusDisplay(IntentionQuoteItem intentionItem)
        {
            StringBuilder inquirySB = new StringBuilder();
            StringBuilder inquiryToolTipSB = new StringBuilder();
            int count = 0;

            string key = intentionItem.IntentionId;
            if (LocalEnv.ConfInfo.IsPAYL && !STC.Common.Utils.CommonUtils.IsInvestor())
            {
                key = intentionItem.GroupKey;
            }

            if (!string.IsNullOrEmpty(key) && _Cache_InquiryQuote_Dic.ContainsKey(key) && _Cache_InquiryQuote_Dic[key] != null)
            {
                foreach (IntentionMarketItem marketItem in _Cache_InquiryQuote_Dic[key].Values)
                {
                    if (marketItem.Id.EndsWith("_内部") || marketItem.Status != "1" || marketItem.SendType == 3)
                    {
                        continue;
                    }
                    if (count < 3)
                    {
                        if (inquirySB.Length > 0)
                        {
                            inquirySB.Append("、");
                        }
                        inquirySB.Append(marketItem.ReceiverNumber);
                    }

                    if (inquiryToolTipSB.Length > 0)
                    {
                        inquiryToolTipSB.Append("、");
                    }
                    inquiryToolTipSB.Append(marketItem.ReceiverNumber);
                    count++;
                }
            }

            intentionItem.Status_Display = $"{inquirySB}";
            intentionItem.Status_Tooltip_Display = $"{inquiryToolTipSB}";

            if (inquirySB.Length > 0)
            {
                intentionItem.InquiryStatus = "1";
            }
            else
            {
                intentionItem.InquiryStatus = "0";
            }

            if (!string.IsNullOrEmpty(key) && _Cache_Group_InnerOrders_Dic.ContainsKey(key) && _Cache_Group_InnerOrders_Dic[key] != null)
            {
                // 汇总行
                if (!string.IsNullOrEmpty(key) && _Cache_InquiryQuote_Dic.ContainsKey(key) && _Cache_InquiryQuote_Dic[key] != null)
                {
                    _Cache_Group_InnerOrders_Dic[key].HasInquiry = _Cache_InquiryQuote_Dic[key].Count > 0;
                }
                else
                {
                    _Cache_Group_InnerOrders_Dic[key].HasInquiry = false;
                }

                // 明细行
                if (_Cache_Buy_InnerOrders_Dic.ContainsKey(key) && _Cache_Buy_InnerOrders_Dic[key] != null)
                {
                    if (_Cache_Buy_InnerOrders_Dic[key].Values.Count > 1)
                    {
                        foreach (IntentionQuoteItem item in _Cache_Buy_InnerOrders_Dic[key].Values)
                        {
                            item.HasInquiry = true;
                        }
                    }
                    else
                    {
                        foreach (IntentionQuoteItem item in _Cache_Buy_InnerOrders_Dic[key].Values)
                        {
                            if (!string.IsNullOrEmpty(key) && _Cache_InquiryQuote_Dic.ContainsKey(key) && _Cache_InquiryQuote_Dic[key] != null)
                            {
                                item.HasInquiry = _Cache_InquiryQuote_Dic[key].Count > 0;
                            }
                            else
                            {
                                item.HasInquiry = false;
                            }
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(key) && _Cache_InquiryQuote_Dic.ContainsKey(key) && _Cache_InquiryQuote_Dic[key] != null)
                {
                    intentionItem.HasInquiry = _Cache_InquiryQuote_Dic[key].Count > 0;
                }
                else
                {
                    if (intentionItem.Status == "80" || intentionItem.Status == "81")
                    {
                        intentionItem.HasInquiry = false;
                    }
                    else
                    {
                        intentionItem.HasInquiry = true;
                    }
                }
            }
            
        }



        private void OnRefInquiryCommand(Button btn)
        {
            IntentionMarketItem currentItem = btn.DataContext as IntentionMarketItem;
            if (currentItem != null)
            {
                List<string> inquiryNotRefList = new List<string>();
                int result = RefInquiryMessageBox.Show();
                if (result == 0)
                {
                    inquiryNotRefList.Add(currentItem.Id);
                }
                if (result == -1)
                {
                    return;
                }
                if (currentItem.ReceiverNumber == "内部")
                {
                    DoHideOne(this.SelectedItem_Working);
                }
                else
                {
                    Sub_OnRefAllInquiryCommand(currentItem.IntentionId, currentItem.GroupKey, currentItem, inquiryNotRefList);
                }
            }
        }

        private void OnRenewInquiryCommand(Button btn)
        {
            IntentionMarketItem currentItem = btn.DataContext as IntentionMarketItem;
            if (currentItem != null)
            {
                Sub_ToEditInquiryCommand(null, currentItem);
            }
        }

        private void OnEditInquiryCommand(Button btn)
        {
            IntentionMarketItem currentItem = btn.DataContext as IntentionMarketItem;
            if (currentItem != null)
            {
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                {
                    currentItem.IsEdit = true;

                    string securityID = currentItem.SecurityId;
                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                    if (bondsInfoPo != null)
                    {
                        // 含权
                        if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                        ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                        (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                        {

                            if (currentItem.PriceTypeDic.Count == 2)
                            {
                                currentItem.PriceTypeDic.Add(new KeyValuePair<string, string>("7", "行权"));
                            }
                        }
                        else
                        {
                            if (currentItem.PriceTypeDic.Count == 3)
                            {
                                currentItem.PriceTypeDic.RemoveAt(2);
                            }
                        }
                    }
                   
                    currentItem.Price_Edit = currentItem.Price;
                    currentItem.Qty_Edit = currentItem.Qty;
                    currentItem.SettlType = currentItem.SettlType;
                    currentItem.PriceType_Edit = currentItem.PriceType;
                    currentItem.PFlag_Edit = currentItem.PFlag;
                    currentItem.OCOFlag_Edit = currentItem.OCOFlag;
                    currentItem.Remark_Edit = currentItem.Remark;
                }
                else
                {
                    Sub_ToEditInquiryCommand(this.SelectedItem_Working, currentItem);
                }
            }
        }

        internal void Sub_ToEditInquiryCommand(IntentionQuoteItem item, IntentionMarketItem inquiryItem)
        {
            if (LocalEnv.ConfInfo.IsPAYL)
            {
                PublishQuoteView_PA publishQuoteView_PA = new PublishQuoteView_PA();
                PublishQuoteViewModel_PA vm_pa = new PublishQuoteViewModel_PA();
                vm_pa.InitRenew(inquiryItem, publishQuoteView_PA);
                publishQuoteView_PA.DataContext = vm_pa;
                publishQuoteView_PA.Show();
            }
            else
            {
                if (item == null)
                {
                    STC.Common.View.Component.CustomMessageBox.Info("请先选择个需", "提示");
                    return;
                }
                string winKey = $"PublishQuoteView_{item.IntentionId}";
                if (inquiryItem != null)
                {
                    winKey = $"{winKey}_{inquiryItem.Id}";
                }

                if (!WindowManager.Instance.IsExist(winKey))
                {
                    PublishQuoteView win = new PublishQuoteView();
                    PublishQuoteViewModel vm = win.DataContext as PublishQuoteViewModel;
                    vm.CurrentItem = item;
                    vm.CurrentMarketItem = inquiryItem;
                    vm.InitData(win);
                    WindowManager.Instance.OpenOnlyOneWindow($"winKey", win);
                }
            }
        }

        private void OnSaveInquiryCommand(Button btn)
        {
            IntentionMarketItem currentItem = btn.DataContext as IntentionMarketItem;
            if (currentItem != null)
            {
                currentItem.IsEdit = false;
                Sub_DoSendQuoteCommand(currentItem);
            }
        }

        private void Sub_DoSendQuoteCommand(IntentionMarketItem item)
        {
            try
            {
                int failCount_local = 0;
                List<TableIntentionInquiry> refList = new List<TableIntentionInquiry>();
                List<TableIntentionInquiry> inquiryList = new List<TableIntentionInquiry>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                TableIntentionInquiry updateData = new TableIntentionInquiry();
                TableIntentionInquiry oldInquiry = IntentionOldHandler.Instance.GetInquiryById(item.IntentionId, item.Id);
                updateData.Clone(oldInquiry);
                updateData.status = null;
                updateData.price = item.Price_Edit.HasValue ? item.Price_Edit?.ToString("0.####").ConvertToDecimal() : oldInquiry.price;
                updateData.qty = item.Qty_Edit.HasValue ? item.Qty_Edit?.ToString("0").ConvertToDecimal() : oldInquiry.qty;
                InquiryInfo3 info3 = new InquiryInfo3();
                info3.PriceType = item.PriceType_Edit;
                updateData.info3 = JsonConvert.SerializeObject(info3);
                updateData.settlType = item.SettlType_Edit;
                updateData.bargainFlag = "0";
                updateData.quoteDividedFlag = "N";
                if (item.PFlag_Edit == "1")
                {
                    updateData.bargainFlag = "1";
                }
                else if (item.PFlag_Edit == "2")
                {
                    updateData.quoteDividedFlag = "Y";
                }
                updateData.ocoFlag = item.OCOFlag_Edit;
                updateData.info1 = item.Remark_Edit;

                inquiryList.Add(updateData);

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                    IntentionInquiryRequest request = new IntentionInquiryRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.biztypeid = LocalEnv.UserInfo.Termids;
                    request.action = "1";
                    request.dataList = inquiryList;
                    //TableOldHandler.Instance.IntentionInquiry(request);

                    //orderDic[item.IntentionId].Request = request;
                    //RemenberInstAndTrade(item);

                    GetErrorInfo_Inquiry(request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                Dictionary<string, IntentionQuoteItem> tempDic = new Dictionary<string, IntentionQuoteItem>();
                                foreach (TableIntentionInquiry inquiry in inquiryList)
                                {
                                    tempDic[Guid.NewGuid().ToString()] = null;
                                }
                                BatchResultWindow batchResultWindow = new BatchResultWindow(tempDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                //batchResultWindow.Owner = _owner;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));

                    //System.Threading.ThreadPool.QueueUserWorkItem(x =>
                    //{
                    //    Thread.Sleep(2000);
                    //    Query();
                    //});
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("BatchUpdateInquiry", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void OnCancelEditInquiryCommand(Button btn)
        {
            IntentionMarketItem currentItem = btn.DataContext as IntentionMarketItem;
            if (currentItem != null)
            {
                currentItem.IsEdit = false;
            }
        }

        internal void Sub_OnRefAllInquiryCommand(string key, string groupKey, IntentionMarketItem inqueryItem = null, List<string> inquiryNotRefList = null)
        {
            List<TableIntentionInquiry> inquiryList = new List<TableIntentionInquiry>();

            IntentionInquiryRequest request = null;
            if (!string.IsNullOrEmpty(key) && _Cache_InquiryQuote_Dic.ContainsKey(key))
            {
                ConcurrentDictionary<string, IntentionMarketItem> list = null;
                if (inqueryItem != null)
                {
                    list = new ConcurrentDictionary<string, IntentionMarketItem>();
                    list[inqueryItem.Id] = inqueryItem;
                }
                else
                {
                    if (_Cache_InquiryQuote_Dic.ContainsKey(key))
                    {
                        list = _Cache_InquiryQuote_Dic[key];
                    }
                }
                
                if (list == null || list.Count == 0)
                {
                    return;
                }

                request = GenIntentionInquiryRequest(key, groupKey, inquiryNotRefList, inqueryItem);
            }

            if (request != null)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int failCount_local = 0;
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                    //TableOldHandler.Instance.IntentionInquiry(request);

                    //orderDic[item.IntentionId].Request = request;
                    //RemenberInstAndTrade(item);

                    GetErrorInfo_Inquiry(request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    //System.Threading.ThreadPool.QueueUserWorkItem(x =>
                    //{
                    //    Thread.Sleep(2000);

                    //    DateTime intentionDT_Start = this.DealTimeBegin ?? DateTime.Now;
                    //    DateTime intentionDT_End = (this.DealTimeEnd ?? DateTime.Now).AddDays(1);
                    //    IntentionOldHandler.Instance.RequestMarketQuotes_IntentionId(item.IntentionId, intentionDT_Start.ToString("yyyy-MM-dd"), intentionDT_End.ToString("yyyy-MM-dd"));
                    //});

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                Dictionary<string, IntentionQuoteItem> tempDic = new Dictionary<string, IntentionQuoteItem>();
                                foreach (TableIntentionInquiry inquiry in inquiryList)
                                {
                                    tempDic[Guid.NewGuid().ToString()] = null;
                                }
                                BatchResultWindow batchResultWindow = new BatchResultWindow(tempDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));
                });
            }
            else
            {
                Common.View.Component.CustomMessageBox.Info($"无可撤询价", "RefAll");
            }
        }

        protected void GetErrorInfo_Inquiry(IntentionInquiryRequest request,
                ref int failCount,
                ref int totalCount,
                Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
                Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    request.quoteType = "BUYER_INTENTION";
                }
                else
                {
                    request.quoteType = "INTENTION";
                }

                if (request.dataList != null)
                {
                    foreach (TableIntentionInquiry inquiryItem in request.dataList)
                    {
                        if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                        {
                            inquiryItem.quoteType = "BUYER_INTENTION";
                        }
                        else
                        {
                            inquiryItem.quoteType = "INTENTION";
                        }
                    }
                }

                IntentionInquiryPo replyPo = TableOldHandler.Instance.IntentionInquiry(request);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10000")
                        {
                            bool isProhibitory = false;
                            List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
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
                                prohibitReplyDic[request.intentionId] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[request.intentionId] = orderRisks;
                            }
                        }
                        else
                        {
                            if (replyPo.Datas != null)
                            {
                                foreach (TableIntentionInquiry error in replyPo.Datas)
                                {
                                    if (!string.IsNullOrEmpty(error.pubFQMsg))
                                    {
                                        List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                                        TPlaceOrderResult result = new TPlaceOrderResult();
                                        result.Code = "10003";
                                        result.DoubleWarn = false;
                                        result.Reason = error.pubFQMsg;
                                        result.SecurityId = error.securityId;
                                        result.Side = error.side;
                                        result.Price = error.price;
                                        result.Qty = error.qty;
                                        exceptionResult.Add(result);
                                        string key = string.IsNullOrEmpty(error.id) ? Guid.NewGuid().ToString() : error.id;
                                        exceptionReplyDic[key] = exceptionResult;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    bool hasError = false;
                    if (replyPo.Datas != null && replyPo.Datas.Count > 0)
                    {
                        foreach (TableIntentionInquiry error in replyPo.Datas)
                        {
                            if (!string.IsNullOrEmpty(error.pubFQMsg))
                            {
                                List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                                TPlaceOrderResult result = new TPlaceOrderResult();
                                result.Code = "10003";
                                result.DoubleWarn = false;
                                result.Reason = error.pubFQMsg;
                                result.SecurityId = error.securityId;
                                result.Side = error.side;
                                result.Price = error.price;
                                result.Qty = error.qty;
                                exceptionResult.Add(result);
                                string key = string.IsNullOrEmpty(error.id) ? Guid.NewGuid().ToString() : error.id;
                                exceptionReplyDic[key] = exceptionResult;
                                hasError = true;
                            }
                        }
                    }

                    if (hasError)
                    {
                        failCount++;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }


        internal void OnRefAllInquiry()
        {
            IntentionQuoteItem currentItem = null;
            ObservableCollection<IntentionMarketItem> datas = null;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
            {
                foreach (IntentionMarketItem inquiryItem in this.inquiryViewSource.View)
                {
                    if (inquiryItem.Type == "G")
                    {
                        this.Sub_OnRefAllInquiryCommand(inquiryItem.IntentionId, inquiryItem.IntentionId);
                    }
                }
            }
            else
            {
                if (this.SelectedItem_Working != null)
                {
                    if (LocalEnv.ConfInfo.IsInquiry && this.SelectedItems_Working != null && this.SelectedItems_Working.Count == 1)
                    {
                        if (this.SelectedItem_Working != null)
                        {
                            string key = this.SelectedItem_Working.IntentionId;
                            if (LocalEnv.ConfInfo.IsPAYL)
                            {
                                key = this.SelectedItem_Working.GroupKey;
                            }

                            currentItem = this.SelectedItem_Working;
                            if (!string.IsNullOrEmpty(key)
                               && this._Cache_InquiryQuote_Dic.ContainsKey(key)
                               && this._Cache_InquiryQuote_Dic[key].Count > 0)
                            {
                                datas = new ObservableCollection<IntentionMarketItem>(this._Cache_InquiryQuote_Dic[key].Values.Where<IntentionMarketItem>(a => a.Status == "1" && a.SendType != 3 && a.OptionType != "sendIntentQQGroups").OrderByDescending(a => a.UpdateTime));
                            }

                            if (datas != null && datas.Count > 0)
                            {
                                ModifyRefView x = new ModifyRefView(currentItem, datas);
                                if (x.ShowDialog() == true)
                                {
                                    this.Sub_OnRefAllInquiryCommand(key, this.SelectedItem_Working.GroupKey, null, x.InquiryNotRefList);
                                }
                            }
                            else
                            {
                                this.Sub_OnRefAllInquiryCommand(key, this.SelectedItem_Working.GroupKey);
                            }
                        }
                    }

                }
                else
                {
                    Common.View.Component.CustomMessageBox.Info($"请先选择一条个需", "撤销全部");
                }
            }
        }

        private void DoToFinishCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    ModifyPersonalRemarksView x = new ModifyPersonalRemarksView();
                    x.memo.Text = currentItem.PersonalRemarks;
                    if (x.ShowDialog() == true)
                    {
                        string text = x.memo.Text;
                        DoToFinishOne(currentItem, text);
                    }
                }
            }

            WriteDBLogHelper.Instance.GenRecord("personal", "010", "完成");
        }

        private void DoToFinishOne(IntentionQuoteItem item, string text)
        {
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = item.IntentionId;
            request.actionType = "9";// 已完成
            request.userName = item.BrokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.updateTime = item.UpdateTime;
            request.personalRemarks = text;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);


                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共恢复{totalCount}条, 其中失败{failCount}条", "恢复结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共恢复{totalCount}条, 全部成功", "恢复结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoReorderCommand", e);
                }
            });
        }

        private void View_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                List<IntentionQuoteItem> tempDatas = this.ViewSource_display.View.Cast<IntentionQuoteItem>().ToList();
                TotalCount4BidDemand = tempDatas.Count(p => p.Side == "1");
                TotalCount4OfrDemand = tempDatas.Count(p => p.Side == "2");
                TotalCount4Demand = TotalCount4BidDemand + TotalCount4OfrDemand;
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems.Count > 0)
                {
                    foreach (IntentionQuoteItem data in e.NewItems)
                    {
                        TotalCount4Demand++;
                        if (data.Side == "1")
                        {
                            TotalCount4BidDemand++;
                        }
                        if (data.Side == "2")
                        {
                            TotalCount4OfrDemand++;
                        }
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Count > 0)
                {
                    foreach (IntentionQuoteItem data in e.OldItems)
                    {
                        TotalCount4Demand--;
                        if (data.Side == "1")
                        {
                            TotalCount4BidDemand--;
                        }
                        if (data.Side == "2")
                        {
                            TotalCount4OfrDemand--;
                        }
                    }
                }
            }

            
        }

        internal void OnSearchIntentionDataCommand()
        {
            TotalCount4BidWorking = 0;
            TotalCount4OfrWorking = 0;
            TotalCount4Working = 0;
            BidC = 0;
            OfrC = 0;

            lock(_workingLock)
            {
                IntentionQuoteInfo.Clear();
            }
            _Cache_InnerOrders_Dic.Clear();
            _Cache_Buy_InnerOrders_Dic.Clear();
            _Cache_Group_InnerOrders_Dic.Clear();
            _Summary_Dic.Clear();

            if (this.DealTimeBegin == null && this.DealTimeEnd == null)
            {
                IntentionOldHandler.Instance.RefreshIntentionData(null, null);
                IntentionOldHandler.Instance.RefreshIntentionData(null, DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), "80,81,82");
            }
            else
            {
                DateTime intentionDT_Start = this.DealTimeBegin ?? DateTime.Now;
                DateTime intentionDT_End = (this.DealTimeEnd ?? DateTime.Now).AddDays(1);
                IntentionOldHandler.Instance.RefreshIntentionData(intentionDT_Start.ToString("yyyy-MM-dd"), intentionDT_End.ToString("yyyy-MM-dd"));
            }
        }

        private void OnSearchDemandDataCommand()
        {
            TotalCount4BidDemand = 0;
            TotalCount4OfrDemand = 0;
            TotalCount4Demand = 0;

            _demand_Dic.Clear();
            lock (_dataLock_demand)
            {
                DemandDT.Rows.Clear();
            }
            lock(_dataLock_display)
            {
                InnerOrders_display.Clear();
                _DemandCount = 0;
                _Cache_display_Dic.Clear();
            }

            DateTime demandDT_Start = this.DealTimeBegin_Demand ?? DateTime.Now;
            DateTime demandDT_End = (this.DealTimeEnd_Demand ?? DateTime.Now).AddDays(1);

            IntentionOldHandler.Instance.RefreshDemandData(demandDT_Start.ToString("yyyy-MM-dd"), demandDT_End.ToString("yyyy-MM-dd"));
        }

        private void OnSearchDateCommand()
        {
            TotalCount4BidWorking = 0;
            TotalCount4OfrWorking = 0;
            TotalCount4Working = 0;
            BidC = 0;
            OfrC = 0;

            TotalCount4BidDemand = 0;
            TotalCount4OfrDemand = 0;
            TotalCount4Demand = 0;

            lock (_workingLock)
            {
                IntentionQuoteInfo.Clear();
            }
            _Cache_InnerOrders_Dic.Clear();
            _Cache_Buy_InnerOrders_Dic.Clear();
            _Cache_Group_InnerOrders_Dic.Clear();
            _Summary_Dic.Clear();

            _demand_Dic.Clear();
            lock (_dataLock_demand)
            {
                DemandDT.Rows.Clear();
            }
            lock (_dataLock_display)
            {
                InnerOrders_display.Clear();
                _DemandCount = 0;
                _Cache_display_Dic.Clear();
            }

            DateTime intentionDT_Start = this.DealTimeBegin ?? DateTime.Now;
            DateTime intentionDT_End = (this.DealTimeEnd ?? DateTime.Now).AddDays(1);
            DateTime demandDT_Start = this.DealTimeBegin_Demand ?? DateTime.Now;
            DateTime demandDT_End = (this.DealTimeEnd_Demand ?? DateTime.Now).AddDays(1);

            //string startDate = intentionDT_Start.CompareTo(demandDT_Start) <= 0 ? intentionDT_Start.ToString("yyyy-MM-dd") : demandDT_Start.ToString("yyyy-MM-dd");
            //string endDate = intentionDT_End.CompareTo(demandDT_End) >= 0 ? intentionDT_End.ToString("yyyy-MM-dd") : demandDT_End.ToString("yyyy-MM-dd");

            if (this.DealTimeBegin == null && this.DealTimeEnd == null)
            {
                IntentionOldHandler.Instance.RefreshIntentionData(null, null);
                IntentionOldHandler.Instance.RefreshIntentionData(null, DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), "80,81,82");
            }
            else
            {
                IntentionOldHandler.Instance.RefreshIntentionData(intentionDT_Start.ToString("yyyy-MM-dd"), intentionDT_End.ToString("yyyy-MM-dd"));
            }
            IntentionOldHandler.Instance.RefreshDemandData(demandDT_Start.ToString("yyyy-MM-dd"), demandDT_End.ToString("yyyy-MM-dd"));
        }

        private void OnSearchMarketDataCommand()
        {
            //市场最优
            //QueryBrokerDatas();
        }

        private void DoToReferenceCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    //if (!Common.View.Component.CustomMessageBox.Question("是否执行发个需操作？", "发个需"))
                    //{
                    //    return;
                    //}
                    DoToReferenceOne(currentItem);
                }
            }

            WriteDBLogHelper.Instance.GenRecord("personal", "012", "发个需");
        }

        private void DoToReferenceOne(IntentionQuoteItem item)
        {
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.actionType = "8";// 个需和参考转换
            request.userName = item.BrokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.accountID = item.AccountID;
            request.accountName = item.AccountName;
            request.ifSubBond = item.ifSubBond;
            request.vipCode = item.vipCode;
            request.orgType = item.orgType;
            request.validUntilTime = GetValidUntilTime(item.Info1);
            request.inf1 = item.Info1;
            request.info2 = item.Info2;
            request.channel = item.Channel;
            request.priceType = item.PriceType;
            request.updateTime = item.UpdateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);


                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共恢复{totalCount}条, 其中失败{failCount}条", "恢复结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共恢复{totalCount}条, 全部成功", "恢复结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoReorderCommand", e);
                }
            });
        }

        private void Instance_OnHistoryIntentionQuotes(List<Common.Model.Table.WorkBrench.IntentionQuoteInfo> datas)
        {
            Sub_Instance_OnHistoryIntentionQuotes(datas, false);
        }

        protected bool isLoadHistory = false;
        private void Sub_Instance_OnHistoryIntentionQuotes(List<Common.Model.Table.WorkBrench.IntentionQuoteInfo> datas, bool isSuspend = true)
        {
            isLoadHistory = true;
            LogHelper.Info("OnHistoryIntentionQuotes");

            string startDate_intention = (this.DealTimeBegin ?? DateTime.Now).ToString("yyyy-MM-dd") + " 00:00:00";
            string endDate_intention = (this.DealTimeEnd ?? DateTime.Now).AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00";

            string startDate_demand = (this.DealTimeBegin_Demand?? DateTime.Now).ToString("yyyy-MM-dd") + " 00:00:00";
            string endDate_demand = (this.DealTimeEnd_Demand ?? DateTime.Now).AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00";


            int batchSize = 500;
            var syncLock = new object(); // 新增集合访问锁

            Task.Run(() =>
            {
                try
                {
                    // 获取数据快照
                    List<Common.Model.Table.WorkBrench.IntentionQuoteInfo> snapshot;
                    lock (datas)
                    {
                        snapshot = datas.ToList(); // 线程安全拷贝
                    }

                    for (int batchIndex = 0; batchIndex < snapshot.Count; batchIndex += batchSize)
                    {
                        var currentBatch = snapshot.Skip(batchIndex).Take(batchSize).ToList();

                        // 异步更新UI（非阻塞）
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var data in currentBatch)
                            {
                                if (data.quotetype != "DEMAND")
                                {
                                    if ((this.DealTimeBegin == null && this.DealTimeEnd == null)
                                    || (startDate_intention.CompareTo(data.updateTime) <= 0 && endDate_intention.CompareTo(data.updateTime) >= 0))
                                    {
                                        if (statusFilterList != null && statusFilterList.Count > 0 && !statusFilterList.Contains(data.status) && !isSuspend)
                                        {
                                            suspendDic[data.intentionId] = data;
                                            //_Suspend_ManualResetEvent.Set();
                                        }
                                        else
                                        {
                                            STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo removeObj = null;
                                            suspendDic.TryRemove(data.intentionId, out removeObj);
                                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL && !STC.Common.Utils.CommonUtils.IsInvestor())
                                            {
                                                (this as IntentionControl_Buy_ViewModel)?.OnIntentionQuotes(data);
                                            }
                                            else
                                            {
                                                OnIntentionQuotes(data);
                                            }
                                            SetDefaultSelectedItem();
                                        }
                                    }
                                }
                                else
                                {
                                    if (startDate_demand.CompareTo(data.updateTime) <= 0 && endDate_demand.CompareTo(data.updateTime) >= 0)
                                    {
                                        OnIntentionQuotes_demand(data);
                                    }
                                }
                            }
                        }, System.Windows.Threading.DispatcherPriority.Send); // 调整优先级

                        // 优化CPU释放策略
                        if (batchIndex % (batchSize * 2) == 0 && batchIndex != 0)
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    // 处理窗口关闭时的正常终止
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex); // UI线程记录日志
                }
                finally
                {
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        isLoadHistory = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                            this.WorkingViewSource.View?.Refresh();
                        }, System.Windows.Threading.DispatcherPriority.Send); // 调整优先级
                    }
                }
            });
        }

        private void DoRefreshIntentionData()
        {
            RefreshBtnEnable = false;

            OnSearchDateCommand();

            OnSearchMarketDataCommand();

            RefreshBtnEnable = true;
        }

        private void DoStartQQCommand()
        {
            Sub_DoStartQQCommand(false, this.SelectedItem_Working);
        }

        private void DoStartQQ2Command()
        {
            Sub_DoStartQQCommand(true, this.SelectedItem_Working);
        }

        internal void Sub_DoStartQQCommand(bool is_send_direct, IntentionQuoteItem item)
        {
            if (!LocalEnv.ConfInfo.IsQQ)
            {
                return;
            }
            string receiver = item.IM;
            string remainingTime = item.RemainingTime;
            string symbol = item.Symbol;
            string securityid = item.SecurityId;
            string side = item.Side == "1" ? "Bid" : "Ofr";
            string price = item.Price == null ? string.Empty : $" {item.Price.Value.ToString("#.####")}";
            string qty = item.Qty == null ? string.Empty : $" {item.Qty.Value.ToString("#")}w";
            string settlType = string.Empty;
            if (item.SettlType != "2"
                && item.SettlType != "3"
                 && item.SettlType != "2,3")
            {
                settlType = " " + ComboSourceUtil.Instance.FindEnum(item.SettlType, "settletype");
            }
            string message = $"{remainingTime} {symbol} {securityid} {side}{price}{qty}{settlType}";

            if (!string.IsNullOrEmpty(receiver) && !string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
            {
                ThreadPoolHelper.InvokeThreadPool(o =>
                {
                    SendDesktopFinQReq request = new SendDesktopFinQReq();
                    request.Username = LocalEnv.UserInfo.UserName;
                    request.sender = LocalEnv.UserInfo.QQNumber;
                    request.receiver = receiver;
                    request.send_direct = is_send_direct;
                    request.clear_edit = true;
                    request.channel = "0";
                    request.message = is_send_direct ? $"您的报价【{message}】，还能再好吗？" : message;
                    PResult result = FinQHandler.Instance.SendDesktopFinQ(request);

                    if (result != null && !result.IsSuccess())
                    {
                        string errorMsg = "失败";
                        if (!string.IsNullOrEmpty(result.Msg))
                            errorMsg = result.Msg;
                        Utils.CommonUtils.Prompt2Second("发起客户单聊QQ", errorMsg);
                    }
                });
                ClipboardHelper.CopyToClipboard(message);
            }
            else
            {
                if (string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
                    Utils.CommonUtils.Prompt2Second("发起客户单聊QQ", "请联系管理员设置本用户对应QQ号码!");
                else if (string.IsNullOrEmpty(receiver))
                    Utils.CommonUtils.Prompt2Second("发起客户单聊QQ", "找不到客户QQ号");
            }
        }

        private void DoRecordOrderCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    //if (!Common.View.Component.CustomMessageBox.Question("是否执行恢复操作？", "恢复"))
                    //{
                    //    return;
                    //}

                    DoReorderOne(currentItem);
                }
            }
        }

        private void onloadData(bool obj)
        {
            var itemList = IntentionQuoteInfo.Where(item => string.IsNullOrEmpty(item.RemainingTime));
            foreach (IntentionQuoteItem data in itemList)
            {
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(data.SecurityId);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    data.RemainingTime = bondsInfoPo.RemaingingTime;
                    data.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                    data.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    data.BondSubType = bondsInfoPo.Bond_Subtype;
                    //data.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    //data.Issuer = bondsInfoPo.issuer;
                    data.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    //data.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }
            }
        }

        public async void ImportCSV()
        {

            try
            {
                this.ImportEnabled = false;
                // 1.弹框
                //创建一个保存文件式的对话框
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                //设置这个对话框的起始保存路径
                ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.RestoreDirectory = true;
                //设置保存的文件的类型，注意过滤器的语法
                ofd.Filter = "(文本文件*.csv)|*.csv";
                ofd.DefaultExt = ".csv";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ofd.FileName.EndsWith(".csv", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = "当前只支持csv";
                        prompt.Show();
                        return;
                    }

                    // 3.导入操作
                    StringBuilder sb = new StringBuilder();

                    DataTable dt = CSVHelper.Instance.ReadCSV(ofd.FileName);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            StringBuilder lineSB = new StringBuilder();
                            foreach (DataColumn dc in dt.Columns)
                            {
                                string value = dr[dc.ColumnName].ConvertToString();
                                value = value.Trim('\"');

                                if (!string.IsNullOrEmpty(value))
                                {
                                    if (lineSB.Length > 0)
                                    {
                                        lineSB.Append(" ");
                                    }
                                    lineSB.Append(value);
                                }
                            }
                            sb.AppendLine(lineSB.ToString());
                        }
                    }
                    else
                    {
                        promptTxt = "无数据或文件有误或文件被占用";
                    }


                    if (sb.Length > 0)
                    {
                        await Task.Run(() =>
                        {
                            IntentionOldHandler.Instance.ImportIntentionQuote(sb.ToString(), dt);
                        });
                    }


                    //if (errorList != null && errorList.Count > 0)
                    //{
                    //    StringBuilder errors = new StringBuilder();
                    //    foreach (string error in errorList)
                    //    {
                    //        if (errors.Length > 0)
                    //        {
                    //            errors.AppendLine();
                    //        }
                    //        errors.Append(error);
                    //    }
                    //    promptTxt = errors.ToString();
                    //}
                    //else
                    //{
                    //    promptTxt = "导入成功";
                    //}
                }
                else
                {
                    promptTxt = "取消导入";
                }

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导入";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                }
            }
            catch (Exception e)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导入";
                prompt.PromptContext = e.Message;
                prompt.Show();

                LogHelper.Error(e);
            }
            finally
            {
                this.ImportEnabled = true;
            }
        }

        private async void ExportCSV()
        {
            int totalSize = 0;
            int successCount = 0;
            int failCount = 0;

            this.ExportEnabled = false;
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            try
            {
                saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                saveFileDialog.FileName = DateTimeEx.ServerTime.ToString("yyyyMMddHHmmss") + "_待发布";
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    using (StreamWriter writer = CSVHelper.Instance.createStream(fileName))
                    {
                        // 输出表头
                        writer.WriteLine(GetHeaderLine(mainExportDic));


                        await Task.Run(() =>
                        {
                            foreach (IntentionQuoteItem item in IntentionQuoteInfo)
                            {
                                if (item.Status == "0" && (item.CloseBy == null || item.CloseBy == LocalEnv.UserInfo.UserName))
                                {
                                    StringBuilder lineSB = new StringBuilder();
                                    try
                                    {
                                        foreach (KeyValuePair<string, string> pair in mainExportDic)
                                        {
                                            string field = pair.Key;

                                            object fieldValue = typeof(IntentionQuoteItem).GetProperty(field).GetValue(item);

                                            string value = Common.Utils.CommonUtils.ConvertToString(fieldValue);

                                            if (field.Equals("Side", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = ComboSourceUtil.Instance.FindEnum(value, "side");
                                            }
                                            else if (field.Equals("SettlType", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = ComboSourceUtil.Instance.FindEnum(value, "settletype");
                                            }
                                            else if (field.Equals("Price", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = fieldValue != null ? value.ConvertToDecimal().ToString("0.00##") : string.Empty;
                                            }
                                            else if (field.Equals("Qty", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = fieldValue != null ? value.ConvertToDecimal().ToString("0") : string.Empty;
                                            }
                                            else if (field.Equals("WholeFlag", StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (value == "1")
                                                {
                                                    value = "整";
                                                }
                                                else
                                                {
                                                    value = string.Empty;
                                                }
                                            }
                                            if (lineSB.Length > 0)
                                            {
                                                lineSB.Append(",");
                                            }

                                            lineSB.Append($"\"{value}\"");
                                        }
                                        writer.WriteLine(lineSB);
                                        successCount++;
                                    }
                                    catch (Exception e)
                                    {
                                        LogHelper.Error(e);
                                        failCount++;
                                    }
                                    totalSize++;
                                }

                            }
                        });
                        // 释放资源
                        writer.Flush();
                        writer.Close();
                    }
                    Common.View.Component.CustomMessageBox.Info($"《{saveFileDialog.FileName}》已保存。总条数：{totalSize}   成功：{successCount}  失败：{failCount}");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                this.ExportEnabled = true;
            }


        }


        private string GetHeaderLine(List<KeyValuePair<string, string>> list)
        {
            StringBuilder headerSb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in list)
            {
                if (headerSb.Length > 0)
                {
                    headerSb.Append(",");
                }
                headerSb.Append(pair.Value);
            }

            return headerSb.ToString();
        }

        List<KeyValuePair<string, string>> mainExportDic = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("InstitutionName", "来源方机构"),
            new KeyValuePair<string, string>("TraderName","来源方"),
            new KeyValuePair<string, string>("Side","方向" ),
            new KeyValuePair<string, string>("SecurityId","代码" ),
            new KeyValuePair<string, string>("Symbol","简称" ),
            new KeyValuePair<string, string>("Price","报价" ),
            new KeyValuePair<string, string>("Qty","报量" ),
            new KeyValuePair<string, string>("WholeFlag","是否整量"),
            new KeyValuePair<string, string>("SettlType","清算速度" ),
        };

        private void WorkingView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                SetDefaultSelectedItem();
                List<IntentionQuoteItem> tempDatas = null;
                lock (_workingLock)
                {
                    tempDatas = this.WorkingViewSource.View.Cast<IntentionQuoteItem>().ToList();
                }

                if (tempDatas != null)
                {
                    TotalCount4BidWorking = tempDatas.Count(p => p.Side == "1");
                    TotalCount4OfrWorking = tempDatas.Count(p => p.Side == "2");
                    BidC = tempDatas.Count(p => p.Side == "1" && p.Status == "80");
                    OfrC = tempDatas.Count(p => p.Side == "2" && p.Status == "80");
                    TotalCount4Working = TotalCount4BidWorking + TotalCount4OfrWorking;
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems.Count > 0)
                {
                    foreach (IntentionQuoteItem data in e.NewItems)
                    {
                        TotalCount4Working++;
                        if (data.Side == "1")
                        {
                            TotalCount4BidWorking++;
                            if (data.Status == "80")
                            {
                                BidC++;
                            }
                        }
                        if (data.Side == "2")
                        {
                            TotalCount4OfrWorking++;
                        }
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Count > 0)
                {
                    foreach (IntentionQuoteItem data in e.OldItems)
                    {
                        TotalCount4Working--;
                        if (data.Side == "1")
                        {
                            TotalCount4BidWorking--;
                        }
                        if (data.Side == "2")
                        {
                            TotalCount4OfrWorking--;
                        }
                    }
                }
            }
        }

        //private void IgnoreView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
        //         || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
        //         || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        //    {
        //        Ignore_Calculate();
        //    }
        //}

        ///// <summary>
        ///// 计算待处理数量
        ///// </summary>
        //private void Working_Calculate()
        //{
        //    int bidCount = 0;
        //    int ofrCount = 0;
        //    int totalCount = 0;

        //    lock (_workingLock)
        //    {
        //        foreach (IntentionQuoteItem item in this.WorkingViewSource.View)
        //        {
        //            totalCount++;
        //            if (item.Side == "1")
        //            {
        //                bidCount++;
        //            }
        //            else if (item.Side == "2")
        //            {
        //                ofrCount++;
        //            }
        //        }
        //    }

        //    this.TotalCount4Working = totalCount;
        //    this.TotalCount4BidWorking = bidCount;
        //    this.TotalCount4OfrWorking = ofrCount;
        //}

        /// <summary>
        /// 计算待处理数量
        /// </summary>
        //private void Ignore_Calculate()
        //{
        //    int bidCount = 0;
        //    int ofrCount = 0;
        //    int totalCount = 0;

        //    lock (_ignoreLock)
        //    {
        //        foreach (IntentionQuoteItem item in this.IgnoreViewSource.View)
        //        {
        //            totalCount++;
        //            if (item.Side == "1")
        //            {
        //                bidCount++;
        //            }
        //            else if (item.Side == "2")
        //            {
        //                ofrCount++;
        //            }
        //        }
        //    }

        //    this.TotalCount4Ignore = totalCount;
        //    this.TotalCount4BidIgnore = bidCount;
        //    this.TotalCount4OfrIgnore = ofrCount;
        //}
        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lock(_workingLock)
                        {
                            if (this.IntentionQuoteInfo != null)
                            {
                                this.IntentionQuoteInfo.Clear();
                            }
                        }

                        _demand_Dic.Clear();
                        lock (_dataLock_demand)
                        {
                            DemandDT.Rows.Clear();
                        }
                        lock (_dataLock_display)
                        {
                            InnerOrders_display.Clear();
                            _DemandCount = 0;
                            _Cache_display_Dic.Clear();
                        }
                        this.InnerOrders_Broker.Clear();

                        _Cache_InnerOrders_Dic.Clear();
                        _Cache_Buy_InnerOrders_Dic.Clear();
                        _Cache_Group_InnerOrders_Dic.Clear();
                        _Summary_Dic.Clear();
                        this.TotalCount4Working = 0;
                        this.TotalCount4BidWorking = 0;
                        this.TotalCount4OfrWorking = 0;
                        this.BidC = 0;
                        this.OfrC = 0;
                        this.TotalCount4Demand = 0;
                        this.TotalCount4BidDemand = 0;
                        this.TotalCount4OfrDemand = 0;
                        IntentionOldHandler.Instance.OnMarketQuotes -= Instance_OnMarketQuotes;
                        IntentionOldHandler.Instance.OnMarketQuotes += Instance_OnMarketQuotes;
                        IntentionOldHandler.Instance.OnHistoryMarketQuotes -= Instance_OnHistoryMarketQuotes;
                        IntentionOldHandler.Instance.OnHistoryMarketQuotes += Instance_OnHistoryMarketQuotes;
                        IntentionOldHandler.Instance.OnIntentionQuotes -= Instance_OnIntentionQuotes;
                        IntentionOldHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;
                        IntentionOldHandler.Instance.OnHistoryIntentionQuotes -= Instance_OnHistoryIntentionQuotes;
                        IntentionOldHandler.Instance.OnHistoryIntentionQuotes += Instance_OnHistoryIntentionQuotes;

                        if (STC.Common.Utils.CommonUtils.IsBDBAppName())
                        {
                            LastExecutionProcesser.Processer.OnLastExecutions_BDB -= Processer_OnLastExecutions_BDB;
                            LastExecutionProcesser.Processer.OnLastExecutions_BDB += Processer_OnLastExecutions_BDB;
                        }
                        else
                        {
                            LastExecutionProcesser.Processer.OnLastExecutions -= Processer_OnLastExecutions;
                            LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;
                        }
                        
                    }));
                }
            }
        }

        private void Instance_OnHotKey(EHotKeySetting obj)
        {
            if (LocalEnv.UserInfo.RoleType != RoleType.BUYER_ROLE)
            {
                if (obj == EHotKeySetting.需求工作台普通确认)
                {
                    DoAllConfirmCommand();
                }
                else if (obj == EHotKeySetting.需求工作台闪电确认)
                {
                    DoAllQuickConfirmCommand();
                }
                else if (obj == EHotKeySetting.需求工作台TG)
                {
                    DoAllTGCommand();
                }
                else if (obj == EHotKeySetting.需求工作台忽略)
                {
                    DoAllIgnoreCommand();
                }
            }
        }


        //private void UpdateConfirmIntentionQuoteRequest(ConfirmIntentionQuoteReplyPo reply)
        //{
        //    Application.Current.Dispatcher.BeginInvoke((Action)(() => {
        //       var findList = IntentionQuoteInfo.Where(x => x.IntentionId == reply.intentionId);
        //        if (findList != null && findList.Count() > 0)
        //        {
        //            IntentionQuoteItem item = findList.First();
        //            item.Status = reply.status;

        //            isAdjustAllChkStatus = true;

        //            if (reply.status == "3")
        //            {
        //                isAdjustAllChkStatus_Ignore = true;
        //            }
        //            item.OrderID = reply.orderId;
        //        }
        //    }));
        //}

        private void Instance_OnIntentionQuotes(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string startDate_intention = (this.DealTimeBegin ?? DateTime.Now).ToString("yyyy-MM-dd") + " 00:00:00";
                string endDate_intention = (this.DealTimeEnd ?? DateTime.Now).AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00";

                string startDate_demand = (this.DealTimeBegin_Demand ?? DateTime.Now).ToString("yyyy-MM-dd") + " 00:00:00";
                string endDate_demand = (this.DealTimeEnd_Demand ?? DateTime.Now).AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00";

                if (obj.quotetype != "DEMAND")
                {
                    if ((this.DealTimeBegin == null && this.DealTimeEnd == null)
                    || (startDate_intention.CompareTo(obj.updateTime) <= 0 && endDate_intention.CompareTo(obj.updateTime) >= 0))
                    {
                        if (statusFilterList != null && statusFilterList.Count > 0 && !statusFilterList.Contains(obj.status))
                        {
                            suspendDic[obj.intentionId] = obj;
                            _Suspend_ManualResetEvent.Set();
                        }
                        else
                        {
                            STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo removeObj = null;
                            suspendDic.TryRemove(obj.intentionId, out removeObj);

                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL && !STC.Common.Utils.CommonUtils.IsInvestor())
                            {
                                (this as IntentionControl_Buy_ViewModel).OnIntentionQuotes(obj);
                            }
                            else
                            {
                                OnIntentionQuotes(obj);
                            }
                            SetDefaultSelectedItem();
                        }
                    }
                }
                else
                {
                    if (startDate_demand.CompareTo(obj.updateTime) <= 0 && endDate_demand.CompareTo(obj.updateTime) >= 0)
                    {
                        OnIntentionQuotes_demand(obj);
                    }
                }
            }));
        }

        protected ConcurrentDictionary<string, STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> suspendDic = new System.Collections.Concurrent.ConcurrentDictionary<string, Common.Model.Table.WorkBrench.IntentionQuoteInfo>();
        internal void OnIntentionQuotes(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            //TableIntentionInquiry inquiryInfo = ConvertToInquiryData(obj);
            //OnMarketQuotes(inquiryInfo);

            lock (_workingLock)
            {
                IntentionQuoteItem item = null;

                if (_Cache_InnerOrders_Dic.ContainsKey(obj.securityId) && _Cache_InnerOrders_Dic[obj.securityId] != null
                    && _Cache_InnerOrders_Dic[obj.securityId].ContainsKey(obj.intentionId) && _Cache_InnerOrders_Dic[obj.securityId][obj.intentionId] != null)
                {
                    item = _Cache_InnerOrders_Dic[obj.securityId][obj.intentionId];
                }

                bool displayFlag = true;
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    //if (STC.Common.Utils.CommonUtils.IsOwn())
                    //{
                    //    if (STC.Common.Utils.CommonUtils.IsInvestor())
                    //    {
                    //        displayFlag = obj.closeBy == null || obj.closeBy == LocalEnv.UserInfo.UserName || obj.partyManager == LocalEnv.UserInfo.UserName;
                    //    }
                    //    else
                    //    {
                    //        displayFlag = obj.closeBy == null || obj.closeBy == LocalEnv.UserInfo.UserName || obj.partyTrader == LocalEnv.UserInfo.UserName;
                    //    }
                    //}
                }
                else
                {
                    displayFlag = obj.closeBy == null || obj.closeBy == LocalEnv.UserInfo.UserName;
                }
                if (item != null)
                {
                    //(!string.IsNullOrEmpty(obj.closeBy) && obj.closeBy != LocalEnv.UserInfo.UserName && obj.status != "0") ||
                    if (displayFlag == false || obj.status == "00" || !(obj.quotetype == "INTENTION" || obj.quotetype == "BUYER_INTENTION") || obj.delflag == "1")
                    {
                        this.IntentionQuoteInfo.Remove(item);
                        if (_Cache_InnerOrders_Dic.ContainsKey(obj.securityId))
                        {
                            ConcurrentDictionary<string, IntentionQuoteItem> removeDic = _Cache_InnerOrders_Dic[obj.securityId];

                            if (removeDic != null && removeDic.ContainsKey(obj.intentionId))
                            {
                                IntentionQuoteItem removeData;
                                removeDic.TryRemove(obj.intentionId, out removeData);
                            }

                            if (_Cache_InnerOrders_Dic[obj.securityId].Count == 0)
                            {
                                _Cache_InnerOrders_Dic.TryRemove(obj.securityId, out removeDic);
                            }
                        }
                        return;
                    }

                    

                    isAdjustAllChkStatus = true;

                    //if ((obj.status == "3" && item.Status == "0")
                    //    || (obj.status == "0" && item.Status == "3"))
                    //{
                    //    isAdjustAllChkStatus_Ignore = true;
                    //}

                    //item.Type = obj.type;
                    item.Status = obj.status;

                    if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    {
                        if (string.IsNullOrEmpty(obj.orderId) == false)
                        {

                            // 更新我的订单/我的撤单
                            MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                               );
                            // 更新我的成交
                            MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                                );
                        }
                    }
                    item.TraderName_Edit = obj.traderName;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.InstitutionName_Edit = obj.institutionName;
                    item.BookId = obj.bookId;
                    item.BookName = obj.bookName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.BrokerId = obj.userName;
                    item.BrokerName = obj.name;
                    item.BrokerName_Edit = obj.name;
                    item.RoleId = obj.roleid;
                    item.RoleName = obj.rolename;

                    item.Side = obj.side;
                    item.CleanPrice = obj.cleanPrice;
                    item.FullPrice = obj.fullPrice;
                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    item.ExecutionQty = obj.executionqty;
                    item.SettlType = obj.settlType;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;
                    item.CloseBy = obj.closeBy;
                    item.CloseByName = obj.closeByName;
                    item.OrderID = obj.orderId;
                    item.QqNumber = obj.qqNumber;
                    item.WholeFlag = obj.wholeFlag;
                    item.AccountID = obj.accountID;
                    item.AccountName = obj.accountName;
                    item.ifSubBond = obj.ifSubBond;
                    item.vipCode = obj.vipCode;
                    item.validUntilTime = obj.validuntitime;
                    item.Info1 = obj.inf1;
                    item.Info2 = obj.info2;
                    item.Info5 = obj.info5;
                    item.orgType = obj.orgType;
                    if (obj.status == "2" || (obj.status == "1" && obj.userName != LocalEnv.UserInfo.UserName))
                    {
                        item.IsShow = false;
                    }
                    else
                    {
                        item.IsShow = true;
                    }
                    item.PriceType = obj.priceType;
                    item.FirmFlag = obj.firmFlag;

                    item.CustomerPrice = obj.customerPrice;
                    item.BP = obj.customerSpread;
                    item.HurryFlag = obj.hurryFlag;
                    item.SuggestFlag = obj.suggestFlag;
                    item.FireFlag = obj.fireFlag;
                    item.FloorPrice = obj.floorPrice;
                    item.OCOFlag = obj.OcoFlag;

                    item.HurryFlag_Edit = item.HurryFlag;
                    item.SuggestFlag_Edit = item.SuggestFlag;
                    item.FireFlag_Edit = item.FireFlag;
                    item.FloorPrice_Edit = item.FloorPrice;
                    item.OCOFlag_Edit = item.OCOFlag;
                    item.Channel = item.Channel;
                    item.InquiryStatus = "0";
                    if (!string.IsNullOrEmpty(obj.info2) && JsonSplit.IsJson(obj.info2))
                    {
                        item.FQInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TagData>(obj.info2);
                        if (item.FQInfo != null)
                        {
                            item.InquiryStatus = string.IsNullOrEmpty(item.FQInfo.inquiryStatus) ? "0" : item.FQInfo.inquiryStatus;
                            item.MatchedQty = item.FQInfo.MATCHED_QUANTITY.ConvertToLong();
                            item.IM = item.FQInfo.fromQQ;
                            item.ToGroupName = item.FQInfo.toGroupName;
                            //info2里面如果有ADD_WHITE_LIST=1,+号就显示
                            //只有私聊显示 
                            setAddVisible(item);
                            item.StrategyOrderId = item.FQInfo.StrategyOrderId;
                        }
                    }

                    item.IsMe = obj.userName == LocalEnv.UserInfo.UserName;
                    item.Sign = obj.sign;
                    item.Sign1 = obj.sign1;
                    item.Sign2 = obj.sign2;
                    item.Sign3 = obj.sign3;
                    item.Sign4 = obj.sign4;
                    item.TermDays = obj.termDays;
                    item.CouponRateCurrent = obj.couponRateCurrent;
                    item.IssuerCode = obj.issuerCode;
                    item.IssuerName = string.IsNullOrEmpty(obj.issuerName) ? "--" : obj.issuerName;
                    item.WarranterCode = obj.warranterCode;
                    item.WarranterName = string.IsNullOrEmpty(obj.warranterName) ? "--" : obj.warranterName;
                    item.ChinaBondValuation = obj.chinaBondValuation;
                    item.ChinaBondValuationFull = obj.chinaBondValuationFull;
                    item.CsiBondValuation = obj.csiBondValuation;
                    item.CsiBondValuationFull = obj.csiBondValuationFull;
                    if (SignParser.IsCumType(item.SignDic1))
                    {
                        if (string.IsNullOrEmpty(obj.chinaBondValuationFull) == false)
                        {
                            int speIndex = obj.chinaBondValuationFull.IndexOf('|');
                            if (speIndex > -1)
                            {
                                item.ChinaBondExercise = obj.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                                item.ChinaBondDue = obj.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                            }
                            else
                            {
                                item.ChinaBondDue = obj.chinaBondValuationFull.ConvertToDecimal();
                            }
                        }
                        
                    }
                    else
                    {
                        item.ChinaBondDue = obj.chinaBondValuation;
                    }
                    if (string.IsNullOrEmpty(obj.csiBondValuationFull) == false)
                    {
                        int speIndex = obj.csiBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            item.CsiBondValuationExercise = obj.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            item.CsiBondValuationDue = obj.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            item.CsiBondValuationDue = obj.csiBondValuationFull.ConvertToDecimal();
                        }
                    }
                    else
                    {
                        item.CsiBondValuationDue = obj.csiBondValuation;
                    }

                    if (item.ChinaBondExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondExercise) * 100;
                        else if (item.Side == "2")
                            item.CCDCValuation_BP_InnerOfr = (item.ChinaBondExercise - item.Price) * 100;
                    }
                    else if (item.ChinaBondDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondDue) * 100;
                        else if (item.Side == "2")
                            item.CCDCValuation_BP_InnerOfr = (item.ChinaBondDue - item.Price) * 100;
                    }
                    if (item.CsiBondValuationExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationExercise) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationExercise - item.Price) * 100;
                    }
                    else if (item.CsiBondValuationDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationDue) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationDue - item.Price) * 100;
                    }
                    item.ModifiedDuration = obj.modifiedDuration;
                    if (!string.IsNullOrEmpty(obj.inf3) && obj.inf3 != "0")
                    {
                        JObject jsonObj = JObject.Parse(obj.inf3);
                        if (jsonObj.ContainsKey("Val_Modified_Duration"))
                        {
                            //string Issue_Amount = jsonObj["Issue_Amount"].ToString();
                            string Val_Modified_Duration = jsonObj["Val_Modified_Duration"].ToString();
                            //string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                            //string valuation_date = jsonObj["valuation_date"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Val_Modified_Duration))
                            {
                                item.ModifiedDuration = Val_Modified_Duration.ConvertToDecimal();
                            }
                        }

                        if (jsonObj.ContainsKey("Outstanding_Amount"))
                        {
                            string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Outstanding_Amount))
                            {
                                item.Outstanding_Amount = Outstanding_Amount.ConvertToDecimal() / 10000;
                            }
                        }
                    }
                    item.DayOffStr = obj.dayOffStr;
                    item.CopyInfo = SignParser.GetCopyInfo(item.Qty?.ToString("#"), obj);
                    item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                    item.Rating = SignParser.GenRating(item.SignDic1, item.SignDic3, item.SecurityId);
                    item.Cdc_Implited_Rating = SignParser.GetCDCImplitedRating(item.SignDic3);
                    item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                    item.Level = SignParser.GenLevel(item.SignDic3);
                    item.RegionType = SignParser.GenRegionType(item.SignDic2);
                    item.EnterpriseType = SignParser.GenEnterpriseType(item.SignDic1);
                    item.If_Guarantee = SignParser.GenIfGuarantee(item.SignDic1);
                    item.If_Repay = SignParser.GenIfRepay(item.SignDic3);
                    item.IssueType = SignParser.GenIssueType(item.SignDic1);
                    item.IssueAmount = obj.issueAmount / 10000;
                    if (STC.Common.Utils.CommonUtils.IsNumeric(obj.outstandingAmount))
                    {
                        item.Outstanding_Amount = obj.outstandingAmount.ConvertToDecimal() / 10000;
                    }
                    item.QuoteType = obj.quotetype;
                    item.Tax_Free = SignParser.GetTaxFree(item.SignDic4);

                    GenStatusDisplay(item);

                    item.CounterPartyID = obj.counterPartyID;
                    item.CounterPartyName = obj.counterPartyName;
                    item.RoleType = obj.roletype;
                    item.Investor = obj.partyManager;
                    item.InvestorName = obj.partyManagerName;
                    item.Trader_Buyer = obj.partyTrader;
                    item.TraderName_Buyer = obj.partyTraderName;


                    if (JsonSplit.IsJson(obj.readFlag))
                    {
                        JObject jsonObj = JObject.Parse(obj.readFlag);
                        if (jsonObj != null && jsonObj.ContainsKey(LocalEnv.UserInfo.UserName))
                        {
                            item.ReadFlag = jsonObj[LocalEnv.UserInfo.UserName].ToString();
                        }
                    }

                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                    {
                        if (JsonSplit.IsJson(obj.info5))
                        {
                            NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(obj.info5);
                            item.PublicRemarks = info5tag.PUBLIC_REMARKS;
                            item.CommandNote = info5tag.COMMAND_NOTES;
                            item.RiskNote = info5tag.RISK_CHECK_RESULT;
                            if (info5tag.PERSONAL_REMARKS != null && info5tag.PERSONAL_REMARKS.ContainsKey(LocalEnv.UserInfo.UserName))
                            {
                                item.PersonalRemarks = info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName];
                            }
                            //if (!string.IsNullOrEmpty(info5tag.REF_NOTES))
                            //{
                            //    if (!string.IsNullOrEmpty(item.PersonalRemarks))
                            //    {
                            //        item.PersonalRemarks += "\r\n";
                            //    }
                            //    item.PersonalRemarks += $"撤销备注:{info5tag.REF_NOTES}";
                            //}
                        }
                        else
                        {
                            item.PublicRemarks = string.Empty;
                            item.CommandNote = string.Empty;
                            item.RiskNote = string.Empty;
                            item.PersonalRemarks = string.Empty;
                        }
                    }
                    else
                    {
                        item.PublicRemarks = obj.publicRemarks;
                        item.PersonalRemarks = obj.personalRemarks;
                    }
                    string matchStatus = IntentionOldHandler.Instance.ExtractByKey(obj.subStatus, "MATCH_STATUS");
                    item.MatchStatus = matchStatus;
                    string execStatus = IntentionOldHandler.Instance.ExtractByKey(obj.subStatus, "EXEC_STATUS");
                    item.ExecStatus = execStatus;
                    item.RelatedUsers = obj.relatedUsers;
                    item.RelatedUsers_Display = GetRelatedUsersDisplay(obj.relatedUsers);
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        if (!string.IsNullOrEmpty(obj.inf1) && JsonSplit.IsJson(obj.inf1))
                        {
                            JObject jsonObj = JObject.Parse(obj.inf1);
                            if (jsonObj.ContainsKey("HsProductId"))
                            {
                                item.HsProductId = jsonObj["HsProductId"] == null ? string.Empty : jsonObj["HsProductId"].ToString();
                            }
                        }
                        item.GroupKey = $"{obj.quotetype}_{obj.partyTrader}_{obj.securityId}_{obj.side}";
                    }
                    else
                    {
                        item.GroupKey = item.IntentionId;
                    }

                    if (obj.priceType == "1")
                    {
                        item.Price_Display = obj.cleanPrice;
                    }
                    else if (obj.priceType == "2")
                    {
                        item.Price_Display = obj.fullPrice;
                    }
                    else
                    {
                        item.Price_Display = obj.price;
                    }

                }
                else
                {
                    //(!string.IsNullOrEmpty(obj.closeBy) && obj.closeBy != LocalEnv.UserInfo.UserName && obj.status != "0") ||
                    if (displayFlag == false || obj.status == "00" || !(obj.quotetype == "INTENTION" || obj.quotetype == "BUYER_INTENTION") || obj.delflag == "1")
                    {
                        return;
                    }

                    item = new IntentionQuoteItem();
                    if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    {
                        if (string.IsNullOrEmpty(obj.orderId) == false)
                        {

                            // 更新我的订单/我的撤单
                            MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                               );
                            // 更新我的成交
                            MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                                );
                        }
                    }
                    item.IntentionId = obj.intentionId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.BookId = obj.bookId;
                    item.BookName = obj.bookName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.BrokerId = obj.userName;
                    item.BrokerName = obj.name;
                    item.BrokerName_Edit = obj.name;
                    item.RoleId = obj.roleid;
                    item.RoleName = obj.rolename;
                    item.QqNumber = obj.qqNumber;
                    item.Side = obj.side;
                    item.SecurityId = obj.securityId;
                    item.Symbol = obj.symbol;
                    item.CleanPrice = obj.cleanPrice;
                    item.FullPrice = obj.fullPrice;
                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    item.ExecutionQty = obj.executionqty;
                    item.SettlType = obj.settlType;
                    //string tradeDate = CommonUtils.CalTradeDate(obj.settlType);
                    //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
                    //DateTime settlDateTime;
                    //int selectIndex;
                    //CommonUtils.SetSettleType(obj.settlType, tradeDate, out settlDateTime, out selectIndex);
                    //item.edit_settletype = selectIndex;

                    //item.BrokerId = obj.brokerId;
                    //item.BrokerName = obj.brokerName;
                    item.Text = obj.text;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;
                    item.CloseBy = obj.closeBy;
                    item.CloseByName = obj.closeByName;
                    //item.Type = obj.type;
                    item.Status = obj.status;
                    item.WholeFlag = obj.wholeFlag;
                    item.AccountID = obj.accountID;
                    item.AccountName = obj.accountName;
                    item.ifSubBond = obj.ifSubBond;
                    item.vipCode = obj.vipCode;
                    item.validUntilTime = obj.validuntitime;
                    item.Info1 = obj.inf1;
                    item.Info2 = obj.info2;
                    item.Info5 = obj.info5;
                    item.orgType = obj.orgType;


                    if (obj.status == "2" || (obj.status == "1" && obj.userName != LocalEnv.UserInfo.UserName))
                    {
                        item.IsShow = false;
                    }
                    else
                    {
                        item.IsShow = true;
                    }

                    isAdjustAllChkStatus = true;

                    //if (obj.status == "3")
                    //{
                    //    isAdjustAllChkStatus_Ignore = true;
                    //}
                    item.OrderID = obj.orderId;

                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(obj.securityId);
                    string marketindicator = "";
                    if (bondsInfoPo != null)
                    {
                        //Symbol = bondsInfoPo.Short_Name;
                        //Security = bondsInfoPo.Bond_ID;
                        //SymbolName = bondsInfoPo.Short_Name;
                        item.RemainingTime = bondsInfoPo.RemaingingTime;
                        item.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        item.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        item.BondSubType = bondsInfoPo.Bond_Subtype;
                        //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //findData.Issuer = bondsInfoPo.issuer;
                        item.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        //findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                        marketindicator = bondsInfoPo.MarketIndicator;
                    }

                    

                    if (!_Cache_InnerOrders_Dic.ContainsKey(obj.securityId))
                    {
                        _Cache_InnerOrders_Dic[obj.securityId] = new ConcurrentDictionary<string, IntentionQuoteItem>();
                    }
                    _Cache_InnerOrders_Dic[obj.securityId][obj.intentionId] = item;

                    if (marketindicator == "4")
                    {
                        try
                        {
                            if (!_symbolCountDic.ContainsKey(obj.securityId))
                            {
                                _symbolCountDic[obj.securityId] = null;
                                LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{obj.securityId}.");
                                PriceHandler.Instance.RegisterCTPrice(obj.securityId, UpdateCtPrice);
                            }

                            //CCDCValuaModel ccdcValueModel = Common.Utils.CommonUtils.GetCCDCValua(item.SecurityId);
                            //if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                            //{
                            //    item.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                            //    item.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                            //    item.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                            //}
                        }
                        catch (Exception e)
                        {
                            LogHandler.Error(e);
                        }

                        //if (item.CCDCValuation.HasValue && item.Price.HasValue)
                        //{
                        //    if (item.Side == "1")
                        //        item.CCDCValuation_BP = (item.Price - item.CCDCValuation) * 100;
                        //    else if (item.Side == "2")
                        //        item.CCDCValuation_BP = (item.CCDCValuation - item.Price) * 100;
                        //}
                    }

                    if (_bestPriceDic.ContainsKey(obj.securityId))
                    {
                        item.Best_Bid = _bestPriceDic[obj.securityId][0];
                        item.Best_Ofr = _bestPriceDic[obj.securityId][1];
                    }

                    item.PriceType = obj.priceType;
                    item.FirmFlag = obj.firmFlag;

                    item.CustomerPrice = obj.customerPrice;
                    item.BP = obj.customerSpread;
                    item.HurryFlag = obj.hurryFlag;
                    item.SuggestFlag = obj.suggestFlag;
                    item.FireFlag = obj.fireFlag;
                    item.FloorPrice = obj.floorPrice;
                    item.OCOFlag = obj.OcoFlag;

                    item.HurryFlag_Edit = item.HurryFlag;
                    item.SuggestFlag_Edit = item.SuggestFlag;
                    item.FireFlag_Edit = item.FireFlag;
                    item.FloorPrice_Edit = item.FloorPrice;
                    item.OCOFlag_Edit = item.OCOFlag;

                    item.Channel = obj.channel;
                    item.InquiryStatus = "0";
                    if (!string.IsNullOrEmpty(obj.info2) && JsonSplit.IsJson(obj.info2))
                    {
                        item.FQInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TagData>(obj.info2);
                        if (item.FQInfo != null)
                        {
                            item.MatchedQty = item.FQInfo.MATCHED_QUANTITY.ConvertToLong();
                            item.InquiryStatus = string.IsNullOrEmpty(item.FQInfo.inquiryStatus) ? "0" : item.FQInfo.inquiryStatus;
                            item.IM = item.FQInfo.fromQQ;
                            item.ToGroupName = item.FQInfo.toGroupName;
                            //info2里面如果有ADD_WHITE_LIST=1,+号就显示
                            //只有私聊显示 
                            setAddVisible(item);
                            item.StrategyOrderId = item.FQInfo.StrategyOrderId;
                        }
                    }

                    item.IsMe = obj.userName == LocalEnv.UserInfo.UserName;
                    item.Sign = obj.sign;
                    item.Sign1 = obj.sign1;
                    item.Sign2 = obj.sign2;
                    item.Sign3 = obj.sign3;
                    item.Sign4 = obj.sign4;
                    item.TermDays = obj.termDays;
                    item.CouponRateCurrent = obj.couponRateCurrent;
                    item.IssuerCode = obj.issuerCode;
                    item.IssuerName = string.IsNullOrEmpty(obj.issuerName) ? "--" : obj.issuerName;
                    item.WarranterCode = obj.warranterCode;
                    item.WarranterName = string.IsNullOrEmpty(obj.warranterName) ? "--" : obj.warranterName;
                    item.ChinaBondValuation = obj.chinaBondValuation;
                    item.ChinaBondValuationFull = obj.chinaBondValuationFull;
                    item.CsiBondValuation = obj.csiBondValuation;
                    item.CsiBondValuationFull = obj.csiBondValuationFull;
                    if (SignParser.IsCumType(item.SignDic1))
                    {
                        if (string.IsNullOrEmpty(obj.chinaBondValuationFull) == false)
                        {
                            int speIndex = obj.chinaBondValuationFull.IndexOf('|');
                            if (speIndex > -1)
                            {
                                item.ChinaBondExercise = obj.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                                item.ChinaBondDue = obj.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                            }
                            else
                            {
                                item.ChinaBondDue = obj.chinaBondValuationFull.ConvertToDecimal();
                            }
                        }
                    }
                    else
                    {
                        item.ChinaBondDue = obj.chinaBondValuation;
                        
                    }
                    if (string.IsNullOrEmpty(obj.csiBondValuationFull) == false)
                    {
                        int speIndex = obj.csiBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            item.CsiBondValuationExercise = obj.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            item.CsiBondValuationDue = obj.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            item.CsiBondValuationDue = obj.csiBondValuationFull.ConvertToDecimal();
                        }
                    }
                    else
                    {
                        item.CsiBondValuationDue = obj.csiBondValuation;
                    }
                    if (item.ChinaBondExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondExercise) * 100;
                        else if (item.Side == "2")
                            item.CCDCValuation_BP_InnerOfr = (item.ChinaBondExercise - item.Price) * 100;
                    }
                    else if (item.ChinaBondDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondDue) * 100;
                        else if (item.Side == "2")
                            item.CCDCValuation_BP_InnerOfr = (item.ChinaBondDue - item.Price) * 100;
                    }
                    if (item.CsiBondValuationExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationExercise) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationExercise - item.Price) * 100;
                    }
                    else if (item.CsiBondValuationDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationDue) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationDue - item.Price) * 100;
                    }

                    item.ModifiedDuration = obj.modifiedDuration;
                    if (!string.IsNullOrEmpty(obj.inf3) && obj.inf3 != "0")
                    {
                        JObject jsonObj = JObject.Parse(obj.inf3);
                        if (jsonObj.ContainsKey("Val_Modified_Duration"))
                        {
                            //string Issue_Amount = jsonObj["Issue_Amount"].ToString();
                            string Val_Modified_Duration = jsonObj["Val_Modified_Duration"].ToString();
                            //string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                            //string valuation_date = jsonObj["valuation_date"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Val_Modified_Duration))
                            {
                                item.ModifiedDuration = Val_Modified_Duration.ConvertToDecimal();
                            }
                        }
                        if (jsonObj.ContainsKey("Outstanding_Amount"))
                        {
                            string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Outstanding_Amount))
                            {
                                item.Outstanding_Amount = Outstanding_Amount.ConvertToDecimal() / 10000;
                            }
                        }
                    }
                    item.DayOffStr = obj.dayOffStr;
                    item.CopyInfo = SignParser.GetCopyInfo(item.Qty?.ToString("#"), obj);
                    item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                    item.Rating = SignParser.GenRating(item.SignDic1, item.SignDic3, item.SecurityId);
                    item.Cdc_Implited_Rating = SignParser.GetCDCImplitedRating(item.SignDic3);
                    item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                    item.Level = SignParser.GenLevel(item.SignDic3);
                    item.RegionType = SignParser.GenRegionType(item.SignDic2);
                    item.EnterpriseType = SignParser.GenEnterpriseType(item.SignDic1);
                    item.If_Guarantee = SignParser.GenIfGuarantee(item.SignDic1);
                    item.If_Repay = SignParser.GenIfRepay(item.SignDic3);
                    item.IssueType = SignParser.GenIssueType(item.SignDic1);
                    item.IssueAmount = obj.issueAmount / 10000;
                    if (STC.Common.Utils.CommonUtils.IsNumeric(obj.outstandingAmount))
                    {
                        item.Outstanding_Amount = obj.outstandingAmount.ConvertToDecimal() / 10000;
                    }
                    item.QuoteType = obj.quotetype;
                    item.Tax_Free = SignParser.GetTaxFree(item.SignDic4);

                    GenStatusDisplay(item);

                    item.CounterPartyID = obj.counterPartyID;
                    item.CounterPartyName = obj.counterPartyName;
                    item.RoleType = obj.roletype;
                    item.Investor = obj.partyManager;
                    item.InvestorName = obj.partyManagerName;
                    item.Trader_Buyer = obj.partyTrader;
                    item.TraderName_Buyer = obj.partyTraderName;

                    if (JsonSplit.IsJson(obj.readFlag))
                    {
                        JObject jsonObj = JObject.Parse(obj.readFlag);
                        if (jsonObj != null && jsonObj.ContainsKey(LocalEnv.UserInfo.UserName))
                        {
                            item.ReadFlag = jsonObj[LocalEnv.UserInfo.UserName].ToString();
                        }
                    }

                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                    {
                        if (JsonSplit.IsJson(obj.info5))
                        {
                            NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(obj.info5);
                            item.PublicRemarks = info5tag.PUBLIC_REMARKS;
                            item.CommandNote = info5tag.COMMAND_NOTES;
                            item.RiskNote = info5tag.RISK_CHECK_RESULT;
                            if (info5tag.PERSONAL_REMARKS != null && info5tag.PERSONAL_REMARKS.ContainsKey(LocalEnv.UserInfo.UserName))
                            {
                                item.PersonalRemarks = info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName];
                            }
                            //if (!string.IsNullOrEmpty(info5tag.REF_NOTES))
                            //{
                            //    if (!string.IsNullOrEmpty(item.PersonalRemarks))
                            //    {
                            //        item.PersonalRemarks += "\r\n";
                            //    }
                            //    item.PersonalRemarks += $"撤销备注:{info5tag.REF_NOTES}";
                            //}
                        }
                        else
                        {
                            item.PublicRemarks = string.Empty;
                            item.CommandNote = string.Empty;
                            item.RiskNote = string.Empty;
                            item.PersonalRemarks = string.Empty;
                        }
                    }
                    else
                    {
                        item.PublicRemarks = obj.publicRemarks;
                        item.PersonalRemarks = obj.personalRemarks;
                    }

                    string matchStatus = IntentionOldHandler.Instance.ExtractByKey(obj.subStatus, "MATCH_STATUS");
                    item.MatchStatus = matchStatus;
                    string execStatus = IntentionOldHandler.Instance.ExtractByKey(obj.subStatus, "EXEC_STATUS");
                    item.ExecStatus = execStatus;
                    item.RelatedUsers = obj.relatedUsers;
                    item.RelatedUsers_Display = GetRelatedUsersDisplay(obj.relatedUsers);
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        if (!string.IsNullOrEmpty(obj.inf1) && JsonSplit.IsJson(obj.inf1))
                        {
                            JObject jsonObj = JObject.Parse(obj.inf1);
                            if (jsonObj.ContainsKey("HsProductId"))
                            {
                                item.HsProductId = jsonObj["HsProductId"] == null ? string.Empty : jsonObj["HsProductId"].ToString();
                            }
                        }
                        item.GroupKey = $"{obj.quotetype}_{obj.partyTrader}_{obj.securityId}_{obj.side}";
                    }
                    else
                    {
                        item.GroupKey = item.IntentionId;
                    }

                    if (obj.priceType == "1")
                    {
                        item.Price_Display = obj.cleanPrice;
                    }
                    else if (obj.priceType == "2")
                    {
                        item.Price_Display = obj.fullPrice;
                    }
                    else
                    {
                        item.Price_Display = obj.price;
                    }


                    this.IntentionQuoteInfo.Add(item);

                    if (Common.Utils.CommonUtils.IsBDBAppName())
                    {
                        UpdateLastExecutions_BDB(item);
                    }
                    else
                    {
                        UpdateLastExecutions(item);
                    }
                }
            }
            //((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
        }

        protected string GetRelatedUsersDisplay(string relatedUsers)
        {
            StringBuilder displayTxt = new StringBuilder();
            if (!string.IsNullOrEmpty(relatedUsers))
            {
               
                string[] arr = relatedUsers.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stcUser in arr)
                {
                    StcTbookUsers user = STC.Common.Utils.CommonUtils.GetUsersInfoByUserName(stcUser);
                    if (user != null)
                    {
                        if (displayTxt.Length > 0)
                        {
                            displayTxt.Append(CommonConstants.COMMA_CHAR);
                        }
                        displayTxt.Append(user.Name);
                    }
                }
            }
            return displayTxt.ToString();
        }

        private void SetDefaultSelectedItem()
        {
            //if (this.SelectedItem_Working == null && !this.WorkingViewSource.View.IsEmpty)
            //{
            //    this.WorkingViewSource.View.MoveCurrentToFirst();
            //    this.SelectedItem_Working = this.WorkingViewSource.View.CurrentItem as IntentionQuoteItem;
            //}
        }

        private bool isDisplay(IntentionQuoteItem item)
        {
            bool result = true;
            try
            {
                bool flag = true;
                if (item == null)
                    return flag;


                lock (_workingSelectedItemsLock)
                {
                    if (this.workingSelectedItems != null && this.workingSelectedItems.Count > 0
                    && (item.SecurityId == null || !this.workingSelectedItems.Any(entry => entry.SecurityId == item.SecurityId)))
                    {
                        return false;
                    }
                }

                lock (_GroupBondsLock)
                {
                    if (this.SelectedGroupBondsItems != null && this.SelectedGroupBondsItems.Count > 0
                    && (item.SecurityId == null || !this.SelectedGroupBondsItems.Contains(item.SecurityId)))
                    {
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(this.FilterText) == false
                    && (item.SecurityId == null || !item.SecurityId.Contains(this.FilterText))
                    && (item.Symbol == null || !item.Symbol.Contains(this.FilterText))
                    //&& (item.RoleName == null || !item.RoleName.Contains(this._FilterText))
                    && (item.BrokerName == null || !item.BrokerName.Contains(this.FilterText))
                    && (item.IssuerName == null || !item.IssuerName.Contains(this.FilterText))
                    && (item.WarranterName == null || !item.WarranterName.Contains(this.FilterText))
                    && (item.TraderName == null || !item.TraderName.Contains(this.FilterText))
                    && (item.InstitutionName == null || !item.InstitutionName.Contains(this.FilterText)))
                    return false;

                if (this._focusTagDic != null && this._focusTagDic.ContainsKey(Consts.SIDE))
                {
                    List<string> sidefilter = this._focusTagDic[Consts.SIDE];
                    if (sidefilter != null && sidefilter.Count > 0)
                    {

                        string otherside = item.Side == "1" ? "2" : "1";
                        //DataRow[] drs = DemandDT.Select($"{SECURITYID_FIELD}='{item.SecurityId}' and {SIDE_FIELD} = '{otherside}'");
                        bool isBidAndOfr = false;
                        if (_demand_Dic != null && _demand_Dic.ContainsKey(item.SecurityId))
                        {
                            isBidAndOfr = _demand_Dic[item.SecurityId].Any(dr => dr[SIDE_FIELD].ToString() == otherside);
                        }
                        if ((sidefilter.Contains("0001") && item.Side == "1")
                        || (sidefilter.Contains("0010") && item.Side == "2")
                        || (sidefilter.Contains("0011") && isBidAndOfr))
                        {
                            // ignore
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (LocalEnv.UserInfo.RoleType != RoleType.BUYER_ROLE)
                {
                    if (((statusFilterList == null || statusFilterList.Count == 0
                   || (statusFilterList != null && statusFilterList.Count > 0 && statusFilterList.Contains(item.Status)))
                   && (item.Status == "0" || item.Status == "1" || item.Status == "2" || item.Status == "3" || item.Status == "4"
                   || item.Status == "80" || item.Status == "81" || item.Status == "82" || item.Status == "83" || item.Status == "84")
                   && (item.CloseBy == null || item.CloseBy == LocalEnv.UserInfo.UserName)) == false)
                        return false;
                }

                string bondSubType_tag = item.SignDic1[Consts.BOND_SUB_TYPE];
                string securityType = STC.Common.Utils.CommonUtils.GetQQSecurityType(bondSubType_tag, item.BondSubType);
                if ((this.DisplayMode == "BNC" && securityType != "2")
                    || (this.DisplayMode == "BCO" && securityType != "1")
                    || (this.DisplayMode == "LOCAL" && securityType != "21")
                    || (this.DisplayMode == "NCD" && securityType != "3"))
                {
                    return false;
                }

                //if ((this.DisplayMode == "BNC" && item.SecurityType != "BNC" && item.BondSubType != "PSB")
                //    || (string.IsNullOrEmpty(this.DisplayMode) == false && this.DisplayMode != "BNC" && item.SecurityType == "BNC" && item.BondSubType != "PSB")
                //    || (this.DisplayMode == "BNC" && item.BondSubType == "PSB"))
                //    return false;

                if (!string.IsNullOrEmpty(this.StartTermDY) && item.TermDays < STC.Common.Utils.CommonUtils.GetDays(this.StartTermDY))
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(this.EndTermDY) && item.TermDays > STC.Common.Utils.CommonUtils.GetDays(this.EndTermDY))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(this.StartCoupon) && (item.CouponRateCurrent == null || item.CouponRateCurrent < this.StartCoupon.ConvertToDecimal()))
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(this.EndCoupon) && (item.CouponRateCurrent == null || item.CouponRateCurrent > this.EndCoupon.ConvertToDecimal()))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(this.StartPrice) && !string.IsNullOrEmpty(this.EndPrice))
                {
                    decimal startPrice = this.StartPrice.ConvertToDecimal();
                    decimal endPrice = this.EndPrice.ConvertToDecimal();
                    if (!(item.Price >= startPrice && item.Price <= endPrice))
                    {
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(this.StartPrice) && string.IsNullOrEmpty(this.EndPrice))
                {
                    decimal startPrice = this.StartPrice.ConvertToDecimal();
                    if (!(item.Price >= startPrice))
                    {
                        return false;
                    }
                }
                else if (string.IsNullOrEmpty(this.StartPrice) && !string.IsNullOrEmpty(this.EndPrice))
                {
                    decimal endPrice = this.EndPrice.ConvertToDecimal();
                    if (!(item.Price <= endPrice))
                    {
                        return false;
                    }
                }


                decimal? chinaBondValue = item.ChinaBondValuation;
                if (item.ChinaBondExercise.HasValue)
                {
                    chinaBondValue = item.ChinaBondExercise;
                }

                if (!string.IsNullOrEmpty(this.StartChinaBondValue) && !string.IsNullOrEmpty(this.EndChinaBondValue))
                {
                    decimal startChinaBondValue = this.StartChinaBondValue.ConvertToDecimal();
                    decimal endChinaBondValue = this.EndChinaBondValue.ConvertToDecimal();
                    if (!(chinaBondValue >= startChinaBondValue && chinaBondValue <= endChinaBondValue))
                    {
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(this.StartChinaBondValue) && string.IsNullOrEmpty(this.EndChinaBondValue))
                {
                    decimal startChinaBondValue = this.StartChinaBondValue.ConvertToDecimal();
                    if (!(chinaBondValue >= startChinaBondValue))
                    {
                        return false;
                    }
                }
                else if (string.IsNullOrEmpty(this.StartChinaBondValue) && !string.IsNullOrEmpty(this.EndChinaBondValue))
                {
                    decimal endChinaBondValue = this.EndChinaBondValue.ConvertToDecimal();
                    if (!(chinaBondValue <= endChinaBondValue))
                    {
                        return false;
                    }
                }

                // bid-中债
                if (item.Side == "1")
                {
                    if (item.Price.HasValue && item.ChinaBondValuation.HasValue)
                    {
                        decimal value = (item.Price.Value - item.ChinaBondValuation.Value) * 100;
                        if (!string.IsNullOrEmpty(this.StartBidBp) && !string.IsNullOrEmpty(this.EndBidBp))
                        {

                            decimal startBp = this.StartBidBp.ConvertToDecimal();
                            decimal endBp = this.EndBidBp.ConvertToDecimal();
                            if (!(value >= startBp && value <= endBp))
                            {
                                return false;
                            }
                        }
                        else if (!string.IsNullOrEmpty(this.StartBidBp) && string.IsNullOrEmpty(this.EndBidBp))
                        {
                            decimal startPrice = this.StartBidBp.ConvertToDecimal();
                            if (!(value >= startPrice))
                            {
                                return false;
                            }
                        }
                        else if (string.IsNullOrEmpty(this.StartBidBp) && !string.IsNullOrEmpty(this.EndBidBp))
                        {
                            decimal endPrice = this.EndBidBp.ConvertToDecimal();
                            if (!(value <= endPrice))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.StartBidBp) || !string.IsNullOrEmpty(this.EndBidBp))
                        {
                            return false;
                        }
                    }
                }

                // 中债-ofr
                if (item.Side == "2")
                {
                    if (item.Price.HasValue && item.ChinaBondValuation.HasValue)
                    {
                        decimal value = (item.ChinaBondValuation.Value - item.Price.Value) * 100;
                        if (!string.IsNullOrEmpty(this.StartOfrBp) && !string.IsNullOrEmpty(this.EndOfrBp))
                        {
                            decimal startPrice = this.StartOfrBp.ConvertToDecimal();
                            decimal endPrice = this.EndOfrBp.ConvertToDecimal();
                            if (!(value >= startPrice && value <= endPrice))
                            {
                                return false;
                            }
                        }
                        else if (!string.IsNullOrEmpty(this.StartOfrBp) && string.IsNullOrEmpty(this.EndOfrBp))
                        {
                            decimal startPrice = this.StartOfrBp.ConvertToDecimal();
                            if (!(value >= startPrice))
                            {
                                return false;
                            }
                        }
                        else if (string.IsNullOrEmpty(this.StartOfrBp) && !string.IsNullOrEmpty(this.EndOfrBp))
                        {
                            decimal endPrice = this.EndOfrBp.ConvertToDecimal();
                            if (!(value <= endPrice))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.StartOfrBp) || !string.IsNullOrEmpty(this.EndOfrBp))
                        {
                            return false;
                        }
                    }
                }



                if (item.Sign != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in item.SignDic)
                    {
                        CheckSign(item, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }

                if (item.Sign1 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in item.SignDic1)
                    {
                        CheckSign(item, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }

                if (item.Sign2 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in item.SignDic2)
                    {
                        CheckSign(item, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }

                if (item.Sign3 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in item.SignDic3)
                    {
                        CheckSign(item, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }
                else if (result && item.Sign3 != 0 && "BNC".Equals(item.SecurityType) && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in item.SignDic3)
                    {
                        string key = pair.Key;
                        string value = pair.Value;
                        if (key == Consts.RANKING_TYPE)
                        {
                            result = result
                               && (!this._focusTagDic.ContainsKey(key) || this._focusTagDic[key].Count == 0) || this._focusTagDic[key].Contains(value)
                               || (this._focusTagDic[key].Contains("xx") && !rankingtype_arr.Contains(value) && !"BNC".Equals(item.SecurityType));
                        }
                        else if (key == Consts.RATING_CURRENT)
                        {
                            result = result
                               && (!this._focusTagDic.ContainsKey(key) || this._focusTagDic[key].Count == 0) || this._focusTagDic[key].Contains(value)
                               || (this._focusTagDic[key].Contains("xx") && !ratingCurrent_arr.Contains(value) && !"BNC".Equals(item.SecurityType));
                        }
                        else if (key == Consts.CDC_IMPLITED_RATING)
                        {
                            result = result
                               && (!this._focusTagDic.ContainsKey(key) || this._focusTagDic[key].Count == 0) || this._focusTagDic[key].Contains(value)
                               || (this._focusTagDic[key].Contains("xx") && !cdcImplitedRating_arr.Contains(value) && !"BNC".Equals(item.SecurityType));
                        }
                        else if (key == Consts.ISSUER_RATING_CURRENT_CBR
                            || key == Consts.CSI_IMPLIED_RATING)
                        {
                            result = result
                               && (!this._focusTagDic.ContainsKey(key) || this._focusTagDic[key].Count == 0) || this._focusTagDic[key].Contains(value)
                               || (this._focusTagDic[key].Contains("01000") && value == "00000" && !"BNC".Equals(item.SecurityType)); // 00000也算其他
                        }
                    }
                }
                if (item.Sign4 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in item.SignDic4)
                    {
                        CheckSign(item, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }
                
            }
            catch(Exception e)
            {
                LogHelper.Error(e);
            }

            return result;
        }

        public bool IsValidDemand(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            return obj.status == "N" || obj.status == "0" || obj.status == "1" || obj.status == "80" || obj.status == "81";
        }

        private void OnIntentionQuotes_demand(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            //lock (_dataLock_demand)
            {
                IntentionQuoteItem item = null;
                string primaryKey = obj.intentionId;

                DataRow findRow = null;
                lock (_dataLock_demand)
                {
                    findRow = DemandDT.Rows.Find(primaryKey);

                    if (findRow != null)
                    {
                        item = (IntentionQuoteItem)findRow[ITEM_FIELD];
                    }
                }

                if (item != null)
                {
                    if ((!string.IsNullOrEmpty(obj.closeBy) && obj.closeBy != LocalEnv.UserInfo.UserName
                        && obj.status != "0") || obj.status == "00" || !IsValidDemand(obj) || obj.quotetype != "DEMAND")
                    {
                        // 删除参考缓存数据
                        if (_demand_Dic.ContainsKey(obj.securityId))
                        {
                            _demand_Dic[obj.securityId].Remove(findRow);
                        }
                        lock (_dataLock_demand)
                        {
                            this.DemandDT.Rows.Remove(findRow);
                        }
                        //删除展示
                        if (_Cache_display_Dic.ContainsKey(primaryKey))
                        {
                            IntentionQuoteItem removeDisplayData = _Cache_display_Dic[primaryKey];

                            IntentionQuoteItem removeItem_dic = null;
                            _Cache_display_Dic.TryRemove(primaryKey, out removeItem_dic);
                            
                            lock (_dataLock_display)
                            {
                                InnerOrders_display.Remove(removeDisplayData);
                                _DemandCount--;
                            }

                            // 需要补充一条，补的是过滤后的第200条
                            DataRowView nthRow = null;
                            int index = 0;
                            foreach (DataRowView drv in DemandDV)
                            {
                                if (index == 200)
                                {
                                    break;
                                }
                                string id = drv[KEY_FIELD].ToString();
                                if (!_Cache_display_Dic.ContainsKey(id))
                                {
                                    nthRow = drv;
                                    break;
                                }
                                index++;
                            }
                            //var query = from row in DemandDV.Cast<DataRowView>()
                            //            select row;
                            //var nthRow = query.Skip(199).FirstOrDefault();
                            if (nthRow != null)
                            {
                                IntentionQuoteItem makeupItem = (IntentionQuoteItem)nthRow[ITEM_FIELD];
                                if (!_Cache_display_Dic.ContainsKey(makeupItem.IntentionId))
                                {
                                    _Cache_display_Dic[makeupItem.IntentionId] = makeupItem;
                                    lock (_dataLock_display)
                                    {
                                        InnerOrders_display.Add(makeupItem);
                                        _DemandCount++;
                                    }
                                }
                                else
                                {
                                    // ignore
                                }
                            }
                            else
                            {
                                // ignore
                            }
                        }
                        if (_Cache_display_Dic.Count != InnerOrders_display.Count)
                        {
                            LogHelper.Error("私域池数据不匹配1");
                        }
                        return;
                    }

                    //item.Type = obj.type;
                    item.Status = obj.status;
                    item.TraderName_Edit = obj.traderName;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.InstitutionName_Edit = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.BrokerId = obj.userName;
                    item.BrokerName = obj.name;
                    item.BrokerName_Edit = obj.name;
                    item.RoleId = obj.roleid;
                    item.RoleName = obj.rolename;

                    item.Side = obj.side;
                    item.Price = obj.price;
                    item.CleanPrice = obj.cleanPrice;
                    item.FullPrice = obj.fullPrice;
                    item.Qty = obj.qty;
                    item.ExecutionQty = obj.executionqty;
                    item.SettlType = obj.settlType;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;
                    item.CloseBy = obj.closeBy;
                    item.CloseByName = obj.closeByName;
                    item.OrderID = obj.orderId;
                    item.QqNumber = obj.qqNumber;
                    item.WholeFlag = obj.wholeFlag;
                    item.AccountID = obj.accountID;
                    item.AccountName = obj.accountName;
                    item.ifSubBond = obj.ifSubBond;
                    item.vipCode = obj.vipCode;
                    item.validUntilTime = obj.validuntitime;
                    item.Info1 = obj.inf1;
                    item.Info2 = obj.info2;
                    item.orgType = obj.orgType;
                    if (obj.status == "2" || (obj.status == "1" && obj.userName != LocalEnv.UserInfo.UserName))
                    {
                        item.IsShow = false;
                    }
                    else
                    {
                        item.IsShow = true;
                    }
                    item.PriceType = obj.priceType;
                    item.FirmFlag = obj.firmFlag;

                    item.CustomerPrice = obj.customerPrice;
                    item.BP = obj.customerSpread;
                    item.HurryFlag = obj.hurryFlag;
                    item.SuggestFlag = obj.suggestFlag;
                    item.FireFlag = obj.fireFlag;
                    item.FloorPrice = obj.floorPrice;
                    item.OCOFlag = obj.OcoFlag;
                    item.PublicRemarks = obj.publicRemarks;
                    item.PersonalRemarks = obj.personalRemarks;

                    item.HurryFlag_Edit = item.HurryFlag;
                    item.SuggestFlag_Edit = item.SuggestFlag;
                    item.FireFlag_Edit = item.FireFlag;
                    item.FloorPrice_Edit = item.FloorPrice;
                    item.OCOFlag_Edit = item.OCOFlag;
                    item.Channel = item.Channel;
                    if (!string.IsNullOrEmpty(obj.info2) && JsonSplit.IsJson(obj.info2))
                    {
                        item.FQInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TagData>(obj.info2);
                        if (item.FQInfo != null)
                        {
                            item.IM = item.FQInfo.fromQQ;
                            item.ToGroupName = item.FQInfo.toGroupName;
                        }
                    }

                    item.IsMe = obj.userName == LocalEnv.UserInfo.UserName;
                    item.Sign = obj.sign;
                    item.Sign1 = obj.sign1;
                    item.Sign2 = obj.sign2;
                    item.Sign3 = obj.sign3;
                    item.Sign4 = obj.sign4;
                    item.TermDays = obj.termDays;
                    item.CouponRateCurrent = obj.couponRateCurrent;
                    item.IssuerCode = obj.issuerCode;
                    item.IssuerName = string.IsNullOrEmpty(obj.issuerName) ? "--" : obj.issuerName;
                    item.WarranterCode = obj.warranterCode;
                    item.WarranterName = string.IsNullOrEmpty(obj.warranterName) ? "--" : obj.warranterName;
                    item.ChinaBondValuation = obj.chinaBondValuation;
                    item.ChinaBondValuationFull = obj.chinaBondValuationFull;
                    item.CsiBondValuation = obj.csiBondValuation;
                    item.CsiBondValuationFull = obj.csiBondValuationFull;
                    if (SignParser.IsCumType(item.SignDic1))
                    {
                        if (string.IsNullOrEmpty(obj.chinaBondValuationFull) == false)
                        {
                            int speIndex = obj.chinaBondValuationFull.IndexOf('|');
                            if (speIndex > -1)
                            {
                                item.ChinaBondExercise = obj.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                                item.ChinaBondDue = obj.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                            }
                            else
                            {
                                item.ChinaBondDue = obj.chinaBondValuationFull.ConvertToDecimal();
                            }
                        }
                        
                    }
                    else
                    {
                        item.ChinaBondDue = obj.chinaBondValuation;
                    }
                    if (string.IsNullOrEmpty(obj.csiBondValuationFull) == false)
                    {
                        int speIndex = obj.csiBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            item.CsiBondValuationExercise = obj.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            item.CsiBondValuationDue = obj.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            item.CsiBondValuationDue = obj.csiBondValuationFull.ConvertToDecimal();
                        }
                    }
                    else
                    {
                        item.CsiBondValuationDue = obj.csiBondValuation;
                    }

                    if (item.ChinaBondExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondExercise) * 100;
                        else if (item.Side == "2")
                            item.CCDCValuation_BP_InnerOfr = (item.ChinaBondExercise - item.Price) * 100;
                    }
                    else if (item.ChinaBondDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondDue) * 100;
                        else if (item.Side == "2")
                            item.CCDCValuation_BP_InnerOfr = (item.ChinaBondDue - item.Price) * 100;
                    }
                    if (item.CsiBondValuationExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationExercise) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationExercise - item.Price) * 100;
                    }
                    else if (item.CsiBondValuationDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationDue) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationDue - item.Price) * 100;
                    }
                    item.ModifiedDuration = obj.modifiedDuration;
                    if (!string.IsNullOrEmpty(obj.inf3) && obj.inf3 != "0")
                    {
                        JObject jsonObj = JObject.Parse(obj.inf3);
                        if (jsonObj.ContainsKey("Val_Modified_Duration"))
                        {
                            //string Issue_Amount = jsonObj["Issue_Amount"].ToString();
                            string Val_Modified_Duration = jsonObj["Val_Modified_Duration"].ToString();
                            //string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                            //string valuation_date = jsonObj["valuation_date"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Val_Modified_Duration))
                            {
                                item.ModifiedDuration = Val_Modified_Duration.ConvertToDecimal();
                            }
                        }

                        if (jsonObj.ContainsKey("Outstanding_Amount"))
                        {
                            string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Outstanding_Amount))
                            {
                                item.Outstanding_Amount = Outstanding_Amount.ConvertToDecimal() / 10000;
                            }
                        }
                    }
                    item.DayOffStr = obj.dayOffStr;
                    item.CopyInfo = SignParser.GetCopyInfo(item.Qty?.ToString("#"), obj);
                    item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                    item.Rating = SignParser.GenRating(item.SignDic1, item.SignDic3, item.SecurityId);
                    item.Cdc_Implited_Rating = SignParser.GetCDCImplitedRating(item.SignDic3);
                    item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                    item.Level = SignParser.GenLevel(item.SignDic3);
                    item.RegionType = SignParser.GenRegionType(item.SignDic2);
                    item.EnterpriseType = SignParser.GenEnterpriseType(item.SignDic1);
                    item.If_Guarantee = SignParser.GenIfGuarantee(item.SignDic1);
                    item.If_Repay = SignParser.GenIfRepay(item.SignDic3);
                    item.IssueType = SignParser.GenIssueType(item.SignDic1);
                    item.IssueAmount = obj.issueAmount / 10000;
                    if (STC.Common.Utils.CommonUtils.IsNumeric(obj.outstandingAmount))
                    {
                        item.Outstanding_Amount = obj.outstandingAmount.ConvertToDecimal() / 10000;
                    }
                    item.QuoteType = obj.quotetype;
                    item.Tax_Free = SignParser.GetTaxFree(item.SignDic4);

                    if (obj.priceType == "1")
                    {
                        item.Price_Display = obj.cleanPrice;
                    }
                    else if (obj.priceType == "2")
                    {
                        item.Price_Display = obj.fullPrice;
                    }
                    else
                    {
                        item.Price_Display = obj.price;
                    }

                    bool isDisplayFlag = isDisplay(item);
                    // 更新缓存
                    lock (_dataLock_demand)
                    {
                        findRow[KEY_FIELD] = primaryKey;
                        findRow[SECURITYID_FIELD] = item.SecurityId;
                        findRow[UPDATETIME_FIELD] = item.UpdateTime;
                        findRow[ITEM_FIELD] = item;
                        findRow[ISDISPLAY_FIELD] = isDisplayFlag;
                        findRow[REMAININGTIME_FIELD] = item.RemainingTime;
                        findRow[SIDE_FIELD] = item.Side;
                    }

                    if (isDisplayFlag)
                    {
                        if (!_Cache_display_Dic.ContainsKey(primaryKey))
                        {
                            lock (_dataLock_display)
                            {
                                _Cache_display_Dic[primaryKey] = item;
                                InnerOrders_display.Insert(0, item);
                                _DemandCount++;
                                // 去掉第200条数据
                                if (_DemandCount > 200)
                                {
                                    IntentionQuoteItem removeItem = InnerOrders_display.ElementAt(200);
                                    IntentionQuoteItem removeItem_dic = null;
                                    _Cache_display_Dic.TryRemove(removeItem.IntentionId, out removeItem_dic);
                                    InnerOrders_display.Remove(removeItem);
                                    _DemandCount--;
                                }
                            }
                        }
                    }
                    else
                    {
                        // 删除展示
                        if (_Cache_display_Dic.ContainsKey(primaryKey))
                        {
                            IntentionQuoteItem removeDisplayData = _Cache_display_Dic[primaryKey];

                            IntentionQuoteItem removeItem_dic = null;
                            _Cache_display_Dic.TryRemove(primaryKey, out removeItem_dic);

                            lock (_dataLock_display)
                            {
                                InnerOrders_display.Remove(removeDisplayData);
                                _DemandCount--;
                            }

                            // 需要补充一条，补的是过滤后的第200条
                            DataRowView nthRow = null;
                            int index = 0;
                            foreach (DataRowView drv in DemandDV)
                            {
                                if (index == 200)
                                {
                                   break;
                                }
                                string id = drv[KEY_FIELD].ToString();
                                if (!_Cache_display_Dic.ContainsKey(id))
                                {
                                    nthRow = drv;
                                    break;
                                }
                                index++;
                            }
                            //var query = from row in DemandDV.Cast<DataRowView>()
                            //            select row;
                            //var nthRow = query.Skip(199).FirstOrDefault();
                            if (nthRow != null)
                            {
                                IntentionQuoteItem makeupItem = (IntentionQuoteItem)nthRow[ITEM_FIELD];
                                if (!_Cache_display_Dic.ContainsKey(makeupItem.IntentionId))
                                {
                                    _Cache_display_Dic[makeupItem.IntentionId] = makeupItem;
                                    lock (_dataLock_display)
                                    {
                                        InnerOrders_display.Add(makeupItem);
                                        _DemandCount++;
                                    }
                                }
                                else
                                {
                                    // ignore
                                }
                            }
                            else
                            {
                                // ignore
                            }
                        }
                    }
                    if (_Cache_display_Dic.Count != InnerOrders_display.Count)
                    {
                        LogHelper.Error("私域池数据不匹配2");
                    }
                }
                else
                {
                    if ((!string.IsNullOrEmpty(obj.closeBy) && obj.closeBy != LocalEnv.UserInfo.UserName
                        && obj.status != "0") || obj.status == "00" || !IsValidDemand(obj) || obj.quotetype != "DEMAND")
                    {
                        return;
                    }

                    item = new IntentionQuoteItem();
                    item.IntentionId = obj.intentionId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.BrokerId = obj.userName;
                    item.BrokerName = obj.name;
                    item.BrokerName_Edit = obj.name;
                    item.RoleId = obj.roleid;
                    item.RoleName = obj.rolename;
                    item.QqNumber = obj.qqNumber;
                    item.Side = obj.side;
                    item.SecurityId = obj.securityId;
                    item.Symbol = obj.symbol;
                    item.CleanPrice = obj.cleanPrice;
                    item.FullPrice = obj.fullPrice;
                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    item.ExecutionQty = obj.executionqty;
                    item.SettlType = obj.settlType;
                    //string tradeDate = CommonUtils.CalTradeDate(obj.settlType);
                    //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
                    //DateTime settlDateTime;
                    //int selectIndex;
                    //CommonUtils.SetSettleType(obj.settlType, tradeDate, out settlDateTime, out selectIndex);
                    //item.edit_settletype = selectIndex;

                    //item.BrokerId = obj.brokerId;
                    //item.BrokerName = obj.brokerName;
                    item.Text = obj.text;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;
                    item.CloseBy = obj.closeBy;
                    item.CloseByName = obj.closeByName;
                    //item.Type = obj.type;
                    item.Status = obj.status;
                    item.WholeFlag = obj.wholeFlag;
                    item.AccountID = obj.accountID;
                    item.AccountName = obj.accountName;
                    item.ifSubBond = obj.ifSubBond;
                    item.vipCode = obj.vipCode;
                    item.validUntilTime = obj.validuntitime;
                    item.Info1 = obj.inf1;
                    item.Info2 = obj.info2;
                    item.orgType = obj.orgType;


                    if (obj.status == "2" || (obj.status == "1" && obj.userName != LocalEnv.UserInfo.UserName))
                    {
                        item.IsShow = false;
                    }
                    else
                    {
                        item.IsShow = true;
                    }

                    //if (obj.status == "3")
                    //{
                    //    isAdjustAllChkStatus_Ignore = true;
                    //}
                    item.OrderID = obj.orderId;

                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(obj.securityId);
                    string marketindicator = "";
                    if (bondsInfoPo != null)
                    {
                        //Symbol = bondsInfoPo.Short_Name;
                        //Security = bondsInfoPo.Bond_ID;
                        //SymbolName = bondsInfoPo.Short_Name;
                        item.RemainingTime = bondsInfoPo.RemaingingTime;
                        item.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        item.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        item.BondSubType = bondsInfoPo.Bond_Subtype;
                        //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //findData.Issuer = bondsInfoPo.issuer;
                        item.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        //findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                        marketindicator = bondsInfoPo.MarketIndicator;
                    }

                    if (marketindicator == "4")
                    {
                        try
                        {
                            if (!_symbolCountDic.ContainsKey(obj.securityId))
                            {
                                _symbolCountDic[obj.securityId] = null;
                                LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{obj.securityId}.");
                                PriceHandler.Instance.RegisterCTPrice(obj.securityId, UpdateCtPrice);
                            }
                        }
                        catch (Exception e)
                        {
                            LogHandler.Error(e);
                        }
                    }

                    if (_bestPriceDic.ContainsKey(obj.securityId))
                    {
                        item.Best_Bid = _bestPriceDic[obj.securityId][0];
                        item.Best_Ofr = _bestPriceDic[obj.securityId][1];
                    }

                    item.PriceType = obj.priceType;
                    item.FirmFlag = obj.firmFlag;

                    item.CustomerPrice = obj.customerPrice;
                    item.BP = obj.customerSpread;
                    item.HurryFlag = obj.hurryFlag;
                    item.SuggestFlag = obj.suggestFlag;
                    item.FireFlag = obj.fireFlag;
                    item.FloorPrice = obj.floorPrice;
                    item.OCOFlag = obj.OcoFlag;
                    item.PublicRemarks = obj.publicRemarks;
                    item.PersonalRemarks = obj.personalRemarks;

                    item.HurryFlag_Edit = item.HurryFlag;
                    item.SuggestFlag_Edit = item.SuggestFlag;
                    item.FireFlag_Edit = item.FireFlag;
                    item.FloorPrice_Edit = item.FloorPrice;
                    item.OCOFlag_Edit = item.OCOFlag;

                    item.Channel = obj.channel;
                    if (!string.IsNullOrEmpty(obj.info2) && JsonSplit.IsJson(obj.info2))
                    {
                        item.FQInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TagData>(obj.info2);
                        if (item.FQInfo != null)
                        {
                            item.IM = item.FQInfo.fromQQ;
                            item.ToGroupName = item.FQInfo.toGroupName;
                        }
                    }

                    item.IsMe = obj.userName == LocalEnv.UserInfo.UserName;
                    item.Sign = obj.sign;
                    item.Sign1 = obj.sign1;
                    item.Sign2 = obj.sign2;
                    item.Sign3 = obj.sign3;
                    item.Sign4 = obj.sign4;
                    item.TermDays = obj.termDays;
                    item.CouponRateCurrent = obj.couponRateCurrent;
                    item.IssuerCode = obj.issuerCode;
                    item.IssuerName = string.IsNullOrEmpty(obj.issuerName) ? "--" : obj.issuerName;
                    item.WarranterCode = obj.warranterCode;
                    item.WarranterName = string.IsNullOrEmpty(obj.warranterName) ? "--" : obj.warranterName;
                    item.ChinaBondValuation = obj.chinaBondValuation;
                    item.ChinaBondValuationFull = obj.chinaBondValuationFull;
                    item.CsiBondValuation = obj.csiBondValuation;
                    item.CsiBondValuationFull = obj.csiBondValuationFull;
                    if (SignParser.IsCumType(item.SignDic1))
                    {
                        if (string.IsNullOrEmpty(obj.chinaBondValuationFull) == false)
                        {
                            int speIndex = obj.chinaBondValuationFull.IndexOf('|');
                            if (speIndex > -1)
                            {
                                item.ChinaBondExercise = obj.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                                item.ChinaBondDue = obj.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                            }
                            else
                            {
                                item.ChinaBondDue = obj.chinaBondValuationFull.ConvertToDecimal();
                            }
                        }
                        
                    }
                    else
                    {
                        item.ChinaBondDue = obj.chinaBondValuation;
                        
                    }
                    if (string.IsNullOrEmpty(obj.csiBondValuationFull) == false)
                    {
                        int speIndex = obj.csiBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            item.CsiBondValuationExercise = obj.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            item.CsiBondValuationDue = obj.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            item.CsiBondValuationDue = obj.csiBondValuationFull.ConvertToDecimal();
                        }
                    }
                    else
                    {
                        item.CsiBondValuationDue = obj.csiBondValuation;
                    }
                    if (item.ChinaBondExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondExercise) * 100;
                        else if (item.Side == "2")
                            item.CCDCValuation_BP_InnerOfr = (item.ChinaBondExercise - item.Price) * 100;
                    }
                    else if (item.ChinaBondDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondDue) * 100;
                        else if (item.Side == "2")
                            item.CCDCValuation_BP_InnerOfr = (item.ChinaBondDue - item.Price) * 100;
                    }
                    if (item.CsiBondValuationExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationExercise) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationExercise - item.Price) * 100;
                    }
                    else if (item.CsiBondValuationDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationDue) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationDue - item.Price) * 100;
                    }
                    item.ModifiedDuration = obj.modifiedDuration;
                    if (!string.IsNullOrEmpty(obj.inf3) && obj.inf3 != "0")
                    {
                        JObject jsonObj = JObject.Parse(obj.inf3);
                        if (jsonObj.ContainsKey("Val_Modified_Duration"))
                        {
                            //string Issue_Amount = jsonObj["Issue_Amount"].ToString();
                            string Val_Modified_Duration = jsonObj["Val_Modified_Duration"].ToString();
                            //string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                            //string valuation_date = jsonObj["valuation_date"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Val_Modified_Duration))
                            {
                                item.ModifiedDuration = Val_Modified_Duration.ConvertToDecimal();
                            }
                        }
                        if (jsonObj.ContainsKey("Outstanding_Amount"))
                        {
                            string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Outstanding_Amount))
                            {
                                item.Outstanding_Amount = Outstanding_Amount.ConvertToDecimal() / 10000;
                            }
                        }
                    }
                    item.DayOffStr = obj.dayOffStr;
                    item.CopyInfo = SignParser.GetCopyInfo(item.Qty?.ToString("#"), obj);
                    item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                    item.Rating = SignParser.GenRating(item.SignDic1, item.SignDic3, item.SecurityId);
                    item.Cdc_Implited_Rating = SignParser.GetCDCImplitedRating(item.SignDic3);
                    item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                    item.Level = SignParser.GenLevel(item.SignDic3);
                    item.RegionType = SignParser.GenRegionType(item.SignDic2);
                    item.EnterpriseType = SignParser.GenEnterpriseType(item.SignDic1);
                    item.If_Guarantee = SignParser.GenIfGuarantee(item.SignDic1);
                    item.If_Repay = SignParser.GenIfRepay(item.SignDic3);
                    item.IssueType = SignParser.GenIssueType(item.SignDic1);
                    item.IssueAmount = obj.issueAmount / 10000;
                    if (STC.Common.Utils.CommonUtils.IsNumeric(obj.outstandingAmount))
                    {
                        item.Outstanding_Amount = obj.outstandingAmount.ConvertToDecimal() / 10000;
                    }
                    item.QuoteType = obj.quotetype;
                    item.Tax_Free = SignParser.GetTaxFree(item.SignDic4);

                    if (obj.priceType == "1")
                    {
                        item.Price_Display = obj.cleanPrice;
                    }
                    else if (obj.priceType == "2")
                    {
                        item.Price_Display = obj.fullPrice;
                    }
                    else
                    {
                        item.Price_Display = obj.price;
                    }

                    bool isDisplayFlag = isDisplay(item);
                    // 添加缓存
                    lock (_dataLock_demand)
                    {
                        DataRow newRow = DemandDT.NewRow();
                        newRow[KEY_FIELD] = primaryKey;
                        newRow[SECURITYID_FIELD] = item.SecurityId;
                        newRow[UPDATETIME_FIELD] = item.UpdateTime;
                        newRow[ITEM_FIELD] = item;
                        newRow[ISDISPLAY_FIELD] = isDisplayFlag;
                        newRow[REMAININGTIME_FIELD] = item.RemainingTime;
                        newRow[SIDE_FIELD] = item.Side;
                        DemandDT.Rows.Add(newRow);

                        if (!_demand_Dic.ContainsKey(obj.securityId))
                        {
                            _demand_Dic[obj.securityId] = new List<DataRow>();
                        }
                        _demand_Dic[obj.securityId].Add(newRow);
                    }


                    if (Common.Utils.CommonUtils.IsBDBAppName())
                    {
                        UpdateLastExecutions_BDB(item);
                    }
                    else
                    {
                        UpdateLastExecutions(item);
                    }

                    if (isDisplayFlag)
                    {
                        // 添加展示
                        lock (_dataLock_display)
                        {
                            _Cache_display_Dic[primaryKey] = item;

                            InnerOrders_display.Insert(0, item);
                            _DemandCount++;
                            // 去掉第200条数据
                            if (_DemandCount > 200)
                            {
                                IntentionQuoteItem removeItem = InnerOrders_display.ElementAt(200);
                                IntentionQuoteItem removeItem_dic = null;
                                _Cache_display_Dic.TryRemove(removeItem.IntentionId, out removeItem_dic);
                                InnerOrders_display.Remove(removeItem);
                                _DemandCount--;
                            }
                        }
                    }
                    if (_Cache_display_Dic.Count != InnerOrders_display.Count)
                    {
                        LogHelper.Error("私域池数据不匹配3");
                    }
                }
            }
            //((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
        }

        public void setAddVisible(IntentionQuoteItem item)
        {
            if (item.FQInfo != null && string.IsNullOrEmpty(item.FQInfo.toGroupOpenID)
                 && !string.IsNullOrEmpty(item.IM) && STC.Common.Utils.CommonUtils.BrokerDic != null && !STC.Common.Utils.CommonUtils.BrokerDic.ContainsKey(item.IM))
                item.AddVisible = "1";
            else
                item.AddVisible = "0";//不显示+号
        }

        /// <summary>
        /// 普通确认
        /// </summary>
        private void DoConfirmCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    //if (!Common.View.Component.CustomMessageBox.Question("是否执行发布确认操作？", "发布确认"))
                    //{
                    //    return;
                    //}

                    CheckandTips("1", currentItem);
                }
            }

            WriteDBLogHelper.Instance.GenRecord("personal", "008", "发内部");
        }

        private void DoConfirmOne(IntentionQuoteItem item)
        {
            item.IsConfirmed = true;
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.corssVenues = "2";
            request.actionType = "1";// NF发布确认
            request.userName = item.BrokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.accountID = item.AccountID;
            request.accountName = item.AccountName;
            request.ifSubBond = item.ifSubBond;
            request.vipCode = item.vipCode;
            request.orgType = item.orgType;
            request.validUntilTime = GetValidUntilTime(item.Info1);
            request.inf1 = item.Info1;
            request.info2 = item.Info2;
            request.channel = item.Channel;
            request.priceType = item.PriceType;
            request.updateTime = item.UpdateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }

            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;
                    RemenberInstAndTrade(item);

                    GetErrorInfo(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoConfirmCommand", e);
                }
                finally
                {
                    item.IsConfirmed = false;
                }
            });
        }

        private void DoQuickConfirmCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    //if (!Common.View.Component.CustomMessageBox.Question("是否执行firm发布确认操作？", "firm发布确认"))
                    //{
                    //    return;
                    //}

                    CheckandTips("2", currentItem);
                }
            }
        }

        private void DoQuickConfirmOne(IntentionQuoteItem item)
        {
            item.IsConfirmed = true;
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.corssVenues = "2";
            request.actionType = "2";// firm发布确认
            request.userName = item.BrokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.accountID = item.AccountID;
            request.accountName = item.AccountName;
            request.ifSubBond = item.ifSubBond;
            request.vipCode = item.vipCode;
            request.orgType = item.orgType;
            request.validUntilTime = GetValidUntilTime(item.Info1);
            request.inf1 = item.Info1;
            request.info2 = item.Info2;
            request.channel = item.Channel;
            request.priceType = item.PriceType;
            request.updateTime = item.UpdateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }

            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;
                    RemenberInstAndTrade(item);

                    GetErrorInfo(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共闪电确认{totalCount}条, 其中失败{failCount}条", "闪电确认结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共闪电确认{totalCount}条, 全部成功", "闪电确认结果");
                        //}
                    }));

                }
                catch (Exception e)
                {
                    LogHelper.Error("DoQuickConfirmCommand", e);
                }
                finally
                {
                    item.IsConfirmed = false;
                }
            });
        }

        private void DoTGCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    //if (!Common.View.Component.CustomMessageBox.Question("是否执行T/G操作？", "T/G"))
                    //{
                    //    return;
                    //}
                    CheckandTips("3", currentItem);
                }
            }
        }

        private void DoTGOne(IntentionQuoteItem item)
        {
            item.IsConfirmed = true;
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.corssVenues = "2";
            request.actionType = "3";// T/G确认
            request.userName = item.BrokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.accountID = item.AccountID;
            request.accountName = item.AccountName;
            request.ifSubBond = item.ifSubBond;
            request.vipCode = item.vipCode;
            request.orgType = item.orgType;
            request.validUntilTime = GetValidUntilTime(item.Info1);
            request.inf1 = item.Info1;
            request.info2 = item.Info2;
            request.channel = item.Channel;
            request.priceType = item.PriceType;
            request.updateTime = item.UpdateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;
                    RemenberInstAndTrade(item);

                    GetErrorInfo(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共T/G确认{totalCount}条, 其中失败{failCount}条", "T/G确认结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共T/G确认{totalCount}条, 全部成功", "T/G确认结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoQuickConfirmCommand", e);
                }
                finally
                {
                    item.IsConfirmed = false;
                }
            });
        }

        private void DoIgnoreCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    //if (!Common.View.Component.CustomMessageBox.Question("是否执行REF操作？", "REF"))
                    //{
                    //    return;
                    //}
                    ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                    DoIgnoreOne(currentItem);
                }
            }

            WriteDBLogHelper.Instance.GenRecord("personal", "009", "撤销");
        }

        private void DoIgnoreOne(IntentionQuoteItem item)
        {
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.actionType = "4";// 删除
            request.userName = item.BrokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.accountID = item.AccountID;
            request.accountName = item.AccountName;
            request.ifSubBond = item.ifSubBond;
            request.vipCode = item.vipCode;
            request.orgType = item.orgType;
            request.validUntilTime = GetValidUntilTime(item.Info1);
            request.inf1 = item.Info1;
            request.info2 = item.Info2;
            request.channel = item.Channel;
            request.priceType = item.PriceType;
            request.updateTime = item.UpdateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共忽略{totalCount}条, 其中失败{failCount}条", "忽略结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共忽略{totalCount}条, 全部成功", "忽略结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoQuickConfirmCommand", e);
                }
            });
        }

        protected void DoRefCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    //if (!Common.View.Component.CustomMessageBox.Question("是否执行REF报价？", "REF"))
                    //{
                    //    return;
                    //}
                    ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                    DoIgnoreOne(currentItem);
                }
            }
        }

        private async void DoRefOne(IntentionQuoteItem item)
        {
            DataRow drv = MyOrderProcesser.Processer.GetOrderById(item.OrderID);

            if (drv != null)
            {
                STC.Common.Model.TBook.OrderCancelRequest cancelPo = new STC.Common.Model.TBook.OrderCancelRequest();
                cancelPo.AlgoName = Common.Utils.CommonUtils.ConvertToString(drv[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                cancelPo.OrderID = Common.Utils.CommonUtils.ConvertToString(drv[MyOrderProcesser.STCCODE_ORDER_FIELD]);
                STC.Common.Model.TBook.TBookCancelReplyPo reply = await CancelOrder(cancelPo);
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

        public async Task<TBookCancelReplyPo> CancelOrder(STC.Common.Model.TBook.OrderCancelRequest cancelPo)
        {
            TBookCancelReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = OrdersHandler.Instance.CancelOrder(cancelPo);

            });
            return reply;
        }

        private void DoNewCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    //if (!Common.View.Component.CustomMessageBox.Question("是否取消发内部报价？", "取消发内部"))
                    //{
                    //    return;
                    //}
                    ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                    DoHideOne(currentItem);
                }
            }

            WriteDBLogHelper.Instance.GenRecord("personal", "029", "取消发内部");
        }

        private void DoHideOne(IntentionQuoteItem item)
        {
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.actionType = "7";// 隐藏
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.userName = item.BrokerId;
            request.updateTime = item.UpdateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共忽略{totalCount}条, 其中失败{failCount}条", "忽略结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共忽略{totalCount}条, 全部成功", "忽略结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoQuickConfirmCommand", e);
                }
            });
        }
        
        private STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo GetIntentionQuoteInfo(IntentionQuoteItem item)
        {
            STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo record = new STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo();
            record.intentionId = item.IntentionId;
            record.institutionId = item.InstitutionId;
            record.institutionName = item.InstitutionName;
            record.userName = item.UserName;
            record.traderId = item.TraderId;
            record.traderName = item.TraderName;
            record.side = item.Side;
            record.securityId = item.SecurityId;
            record.symbol = item.Symbol;
            record.price = item.Price;
            record.qty = item.Qty;
            record.settlType = item.SettlType;
            record.text = item.Text;
            //record.userName = item.UserName;
            record.closeBy = LocalEnv.UserInfo.UserName;// item.CloseBy;
            record.status = "0";
            record.brokerId = item.UserName;
            record.brokerName = item.UserName;
            record.qqNumber = item.QqNumber;
            record.createTime = item.CreateTime;
            record.updateTime = item.UpdateTime;
            record.fullPrice = item.FullPrice;
            record.cleanPrice = item.CleanPrice;
            record.wholeFlag = item.WholeFlag;
            record.priceType = item.PriceType;
            record.accountID = item.AccountID;
            record.accountName = item.AccountName;
            record.vipCode = item.vipCode;
            record.ifSubBond = item.ifSubBond;
            record.orgType = item.orgType;
            record.validuntitime = item.validUntilTime;
            record.inf1 = item.Info1;
            record.hurryFlag = item.HurryFlag;
            record.suggestFlag = item.SuggestFlag;
            record.fireFlag = item.FireFlag;
            record.floorPrice = item.FloorPrice;
            record.OcoFlag = item.OCOFlag;
            record.publicRemarks = item.PublicRemarks;
            record.personalRemarks = item.PersonalRemarks;
            record.customerPrice = item.CustomerPrice;
            record.customerSpread = item.BP;
            record.info2 = item.Info2;
            return record;
        }

        private void Instance_OnBrokerSetting(object obj)
        {
            //拉取设置中发布名单
            STC.Common.Utils.CommonUtils.loadInnerSetingBroker();
            foreach (IntentionQuoteItem item in this.workingViewSource.View)
            {
                setAddVisible(item);
            }
        }

        private async void DoAddBrokerCommand()
        {
            if (this.SelectedItem_Working != null)
            {
                string info = string.Format("是否将以下来源加入自动发布名单?\r\n个人QQ {0}({1})", this.SelectedItem_Working.FQInfo.fromQQ, this.SelectedItem_Working.FQInfo.fromName);
                if (!Common.View.Component.CustomMessageBox.Question(info, "加入"))
                {
                    return;
                }
                else
                {
                    ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                    BrokerWhiteList req = new BrokerWhiteList();
                    req.brokerId = this.SelectedItem_Working.FQInfo.fromQQ;
                    req.brokerName = this.SelectedItem_Working.FQInfo.fromName;
                    req.whiteListEnable = 1;
                    req.username = LocalEnv.UserInfo.UserName;
                    req.userId = LocalEnv.UserInfo.UserId;
                    req.whiteListType = "FINQ";
                    req.updateType = "";//
                    req.action = 1;
                    BrokerWhiteReplyPo reply = await UpdateBrokerWhite(req);
                    if (reply != null)
                    {
                        if (reply.Code == "0000")
                        {
                            STC.Common.View.Component.CustomMessageBox.Info($"加入成功!\r\n{reply.Msg}", "提示", null);
                            //拉取设置中发布名单
                            STC.Common.Utils.CommonUtils.loadInnerSetingBroker();
                        }
                        else
                        {
                            STC.Common.View.Component.CustomMessageBox.Error($"加入失败!\r\n{reply.Msg}", "提示", null);
                        }
                        foreach (IntentionQuoteItem item in this.workingViewSource.View)
                        {
                            setAddVisible(item);
                        }
                    }
                }
            }
        }

        public async Task<BrokerWhiteReplyPo> UpdateBrokerWhite(BrokerWhiteList req)
        {
            BrokerWhiteReplyPo reply = null;

            await Task.Run(() =>
            {
                reply = TableHandler.Instance.brokerWhite(req);
            });
            return reply;
        }

        private async void DoCancelBrokerCommand()
        {
            if (this.SelectedItem_Working != null)
            {
                string qq = this.SelectedItem_Working.FQInfo.fromQQ;
                string info = string.Format("是否将以下来源取消自动发布名单?\r\n个人QQ {0}({1})", qq, this.SelectedItem_Working.FQInfo.fromName);
                if (!Common.View.Component.CustomMessageBox.Question(info, "加入"))
                {
                    return;
                }
                else
                {
                    BrokerWhiteList req = new BrokerWhiteList();
                    if (STC.Common.Utils.CommonUtils.BrokerDic.ContainsKey(qq))
                    {
                        BrokerWhiteList bw = STC.Common.Utils.CommonUtils.BrokerDic[qq];
                        req.id = bw.id;
                        req.brokerId = bw.brokerId;
                        req.brokerName = bw.brokerName;
                        req.whiteListType = bw.whiteListType;
                        req.updateType = bw.updateType;
                        req.username = LocalEnv.UserInfo.UserName;
                        req.userId = LocalEnv.UserInfo.UserId;
                    }
                    req.action = 3;
                    BrokerWhiteReplyPo reply = await UpdateBrokerWhite(req);
                    if (reply != null)
                    {
                        if (reply.Code == "0000")
                        {
                            STC.Common.View.Component.CustomMessageBox.Info($"取消成功!\r\n{reply.Msg}", "提示", null);
                            //拉取设置中发布名单
                            STC.Common.Utils.CommonUtils.loadInnerSetingBroker();
                        }
                        else
                        {

                            STC.Common.View.Component.CustomMessageBox.Error($"取消失败!\r\n{reply.Msg}", "提示", null);
                        }
                        foreach (IntentionQuoteItem item in this.workingViewSource.View)
                        {
                            setAddVisible(item);
                        }
                    }
                }
            }
        }

        private void DoReorderCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    //if (!Common.View.Component.CustomMessageBox.Question("是否执行恢复操作？", "恢复"))
                    //{
                    //    return;
                    //}
                    ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                    DoReorderOne(currentItem);
                }
            }
            WriteDBLogHelper.Instance.GenRecord("personal", "032", "恢复");
        }

        private void DoReorderOne(IntentionQuoteItem item)
        {
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.actionType = "5";// 恢复
            request.userName = item.BrokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.accountID = item.AccountID;
            request.accountName = item.AccountName;
            request.ifSubBond = item.ifSubBond;
            request.vipCode = item.vipCode;
            request.orgType = item.orgType;
            request.validUntilTime = GetValidUntilTime(item.Info1);
            request.inf1 = item.Info1;
            request.info2 = item.Info2;
            request.channel = item.Channel;
            request.priceType = item.PriceType;
            request.updateTime = item.UpdateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);


                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共恢复{totalCount}条, 其中失败{failCount}条", "恢复结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共恢复{totalCount}条, 全部成功", "恢复结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoReorderCommand", e);
                }
            });
        }

        private void DoQuoteIdentifyCommand()
        {
            if (!WindowManager.Instance.IsExist(WindowKeyHelper.RECOGENIZEQUOTE))
            {
                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    RecognizeIntentionQuoteWin_PA win = new RecognizeIntentionQuoteWin_PA(null, null);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.RECOGENIZEQUOTE, win);
                }
                else
                {
                    RecognizeIntentionQuoteWin win = new RecognizeIntentionQuoteWin(null);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.RECOGENIZEQUOTE, win);
                }
            }
        }

        private ConfirmIntentionQuoteReplyPo CheckParam(IntentionQuoteItem item)
        {
            ConfirmIntentionQuoteReplyPo reply = new ConfirmIntentionQuoteReplyPo();
            reply.intentionId = item.IntentionId;
            reply.orderId = item.OrderID;
            reply.securityId = item.SecurityId;
            reply.status = item.Status;
            reply.Code = "0000";
            decimal d = 0;
            string errorMsg = string.Empty;
            //if (item.Price == null || item.Price == 0)
            //    errorMsg = "未填写价格或价格为0";
            //else if (item.Qty == null || item.Qty == 0)
            //    errorMsg = "未填写量或量为0";
            //else if (item.Qty % 100 > 0)
            //    errorMsg = string.Format("量必须是100的整数倍，当前输入值：{0}", item.Qty.Value.ToString("0"));
            //else 
            if (string.IsNullOrEmpty(item.SettlType))
                errorMsg = "清算速度未填写";
            else if (string.IsNullOrEmpty(item.Side))
                errorMsg = "方向未填写";
            //else if (item.Qty != null && item.Qty > 0 && decimal.TryParse(item.Qty.ToString(), out d) && (d % 100) > 0)
            //    errorMsg = string.Format("量必须是100的整数倍，当前输入值：{0}", item.Qty);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                reply.Code = "10001";

                List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();
                TPlaceOrderResult por = new TPlaceOrderResult();
                por.Code = "10003";
                por.DoubleWarn = false;
                por.Reason = errorMsg;
                result.Add(por);

                reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);
            }
            return reply;
        }

        private void GetLocalCheckErrorInfo(IntentionQuoteItem item,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                BaseReplyPo replyPo = CheckParam(item);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10001")
                        {
                            exceptionReplyDic[item.IntentionId] = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                        }
                    }
                }
                else
                {
                    //replyUpdateThread.Enqueue(replyPo);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }

        protected void GetErrorInfo(STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo replyPo = IntentionOldHandler.Instance.ConfirmIntentionQuote(request);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10000")
                        {
                            if (JsonSplit.IsJson(replyPo.Msg))
                            {
                                List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                                bool isProhibitory = false;
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
                                    prohibitReplyDic[request.intentionId] = orderRisks;
                                }
                                else
                                {
                                    warnReplyDic[request.intentionId] = orderRisks;
                                }
                            }
                            else
                            {
                                BaseReplyPo reply = new BaseReplyPo();
                                reply.Code = "10000";
                                List<TPlaceOrderResult> resultList = new List<TPlaceOrderResult>();
                                TPlaceOrderResult por = new TPlaceOrderResult();
                                if (STC.Common.Utils.CommonUtils.IsInvestor())
                                {
                                    por.Code = "10001";
                                }
                                else
                                {
                                    if (replyPo.Msg.Contains("强风控"))
                                    {
                                        por.Code = "10002";
                                    }
                                    else if (replyPo.Msg.Contains("弱风控"))
                                    {
                                        por.Code = "10001";
                                    }
                                }
                                por.DoubleWarn = false;
                                por.Reason = replyPo.Msg;
                                resultList.Add(por);
                                reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(resultList);

                                if (por.Code == "10002")
                                {
                                    prohibitReplyDic[request.intentionId] = resultList;
                                }
                                else if (por.Code == "10001")
                                {
                                    warnReplyDic[request.intentionId] = resultList;
                                }
                            }
                        }
                        else
                        {
                            List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                            TPlaceOrderResult result = new TPlaceOrderResult();
                            result.Code = "10003";
                            result.DoubleWarn = false;
                            result.Reason = replyPo.Msg;
                            exceptionResult.Add(result);
                            exceptionReplyDic[request.intentionId] = exceptionResult;
                        }
                    }
                }
                else
                {
                    //replyUpdateThread.Enqueue(replyPo);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }

        protected void SetDoAllBtnEnable(bool isEnable)
        {
            this.ConfirmEnable = isEnable;
            this.QuickConfirmEnable = isEnable;
            this.TGEnable = isEnable;
            this.RefEnable = isEnable;
            this.RecoverEnable = isEnable;
            this.ToNewEnable = isEnable;
            this.IgnoreEnable = isEnable;
        }

        internal void DoAllConfirmCommand()
        {
            Sub_DoAllConfirmCommand(this.SelectedItems_Working);
        }

        internal void Sub_DoAllConfirmCommand(IList selectedItems)
        {
            WriteDBLogHelper.Instance.GenRecord("personal", "004", "批量-发内部");

            if (selectedItems == null || selectedItems.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量发布确认");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量发布确认操作？", "批量发布确认"))
            //{
            //    return;
            //}

            SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                foreach (IntentionQuoteItem item in selectedItems)
                {
                    if (item.Status != "0") continue;
                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.IntentionId] = item;
                }

                //if (exceptionReplyDic_local.Count > 1)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.IntentionId))
                        {
                            continue;
                        }
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                            request.intentionId = item.IntentionId;
                            request.corssVenues = "2";
                            request.actionType = "1";// NF发布确认
                            request.userName = item.BrokerId;
                            request.closeBy = LocalEnv.UserInfo.UserName;
                            request.closeByName = LocalEnv.UserInfo.FullName;
                            request.clOrdId = DataTimeSequence.Instance.GetId();
                            request.accountID = item.AccountID;
                            request.accountName = item.AccountName;
                            request.ifSubBond = item.ifSubBond;
                            request.vipCode = item.vipCode;
                            request.orgType = item.orgType;
                            request.validUntilTime = GetValidUntilTime(item.Info1);
                            request.inf1 = item.Info1;
                            request.priceType = item.PriceType;
                            request.info2 = item.Info2;
                            request.channel = item.Channel;
                            request.updateTime = item.UpdateTime;
                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                            {
                                request.quoteType = "BUYER_INTENTION";
                            }
                            else
                            {
                                request.quoteType = "INTENTION";
                            }
                            orderDic[item.IntentionId].Request = request;
                            RemenberInstAndTrade(item);

                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                        try
                        {
                            Thread.Sleep(20);
                        }
                        catch (Exception ex)
                        { }
                    }

                    while ((totalCount + failCount_local) != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllConfirmCommand1", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void DoAllQuickConfirmCommand()
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量firm发布确认");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量firm发布确认操作？", "批量firm发布确认"))
            //{
            //    return;
            //}

            SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                foreach (IntentionQuoteItem item in this.SelectedItems_Working)
                {
                    if (item.Status != "0" || !item.Price.HasValue || !item.Qty.HasValue) continue;
                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.IntentionId] = item;
                }
                //if (exceptionReplyDic_local.Count > 0)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.IntentionId))
                        {
                            continue;
                        }
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                            request.intentionId = item.IntentionId;
                            request.corssVenues = "2";
                            request.actionType = "2";// 闪电下单
                            request.userName = item.BrokerId;
                            request.closeBy = LocalEnv.UserInfo.UserName;
                            request.closeByName = LocalEnv.UserInfo.FullName;
                            request.clOrdId = DataTimeSequence.Instance.GetId();
                            request.accountID = item.AccountID;
                            request.accountName = item.AccountName;
                            request.ifSubBond = item.ifSubBond;
                            request.vipCode = item.vipCode;
                            request.orgType = item.orgType;
                            request.validUntilTime = GetValidUntilTime(item.Info1);
                            request.inf1 = item.Info1;
                            request.info2 = item.Info2;
                            request.channel = item.Channel;
                            request.priceType = item.PriceType;
                            request.updateTime = item.UpdateTime;
                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                            {
                                request.quoteType = "BUYER_INTENTION";
                            }
                            else
                            {
                                request.quoteType = "INTENTION";
                            }
                            orderDic[item.IntentionId].Request = request;
                            RemenberInstAndTrade(item);

                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                        try
                        {
                            Thread.Sleep(20);
                        }
                        catch (Exception ex)
                        { }
                    }

                    while ((totalCount + failCount_local) != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共闪电确认{totalCount}条, 其中失败{failCount}条", "闪电确认结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共闪电确认{totalCount}条, 全部成功", "闪电确认结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllQuickConfirmCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void DoAllTGCommand()
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量T/G");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量T/G操作？", "批量T/G"))
            //{
            //    return;
            //}

            SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                foreach (IntentionQuoteItem item in this.SelectedItems_Working)
                {
                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.IntentionId] = item;
                }

                //if (exceptionReplyDic_local.Count > 0)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.IntentionId))
                        {
                            continue;
                        }
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                            request.intentionId = item.IntentionId;
                            request.corssVenues = "2";
                            request.actionType = "3";// TG确认
                            request.userName = item.BrokerId;
                            request.closeBy = LocalEnv.UserInfo.UserName;
                            request.closeByName = LocalEnv.UserInfo.FullName;
                            request.clOrdId = DataTimeSequence.Instance.GetId();
                            request.accountID = item.AccountID;
                            request.accountName = item.AccountName;
                            request.ifSubBond = item.ifSubBond;
                            request.vipCode = item.vipCode;
                            request.orgType = item.orgType;
                            request.validUntilTime = GetValidUntilTime(item.Info1);
                            request.inf1 = item.Info1;
                            request.info2 = item.Info2;
                            request.channel = item.Channel;
                            request.priceType = item.PriceType;
                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                            {
                                request.quoteType = "BUYER_INTENTION";
                            }
                            else
                            {
                                request.quoteType = "INTENTION";
                            }
                            item.Request = request;
                            RemenberInstAndTrade(item);

                            GetErrorInfo(request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                        try
                        {
                            Thread.Sleep(20);
                        }
                        catch (Exception ex)
                        { }
                    }


                    while ((totalCount + failCount_local) != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共T/G确认{totalCount}条, 其中失败{failCount}条", "T/G确认结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共T/G确认{totalCount}条, 全部成功", "T/G确认结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllTGCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void DoAllIgnoreCommand()
        {
            WriteDBLogHelper.Instance.GenRecord("personal", "006", "批量-REF");

            Sub_DoAllIgnoreCommand(this.SelectedItems_Working);
        }

        internal void Sub_DoAllIgnoreCommand(IList selectedItems, string quoteType = null)
        {
            if (selectedItems == null || selectedItems.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量REF");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量REF操作？", "批量REF"))
            //{
            //    return;
            //}

            IntentionQuoteItem currentItem = null;
            ObservableCollection<IntentionMarketItem> datas = null;
            List<string> InquiryNotRefList = null;
            if (LocalEnv.ConfInfo.IsInquiry && selectedItems.Count == 1)
            {
                if (this.SelectedItem_Working != null)
                {
                    currentItem = this.SelectedItem_Working;
                    if (this._Cache_InquiryQuote_Dic.ContainsKey(this.SelectedItem_Working.IntentionId)
                       && this._Cache_InquiryQuote_Dic[this.SelectedItem_Working.IntentionId].Count > 0)
                    {
                        datas = new ObservableCollection<IntentionMarketItem>(this._Cache_InquiryQuote_Dic[this.SelectedItem_Working.IntentionId].Values.Where<IntentionMarketItem>(a => a.Status == "1" && a.SendType != 3 && a.OptionType != "sendIntentQQGroups").OrderByDescending(a => a.UpdateTime));
                    }
                }
                if (datas != null && datas.Count > 0)
                {
                    ModifyRefView x = new ModifyRefView(currentItem, datas);
                    if (x.ShowDialog() == false)
                    {
                        return;
                    }
                    InquiryNotRefList = x.InquiryNotRefList;
                }
            }

            SetDoAllBtnEnable(false);
            try
            {
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                foreach (IntentionQuoteItem item in selectedItems)
                {
                    if ((item.Status != "0" && item.Status != "1") || item.UserName != LocalEnv.UserInfo.UserName) continue;
                    if (item.Status == "0" || item.Status == "1")
                    {
                        STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                        request.intentionId = item.IntentionId;
                        request.actionType = "4";// 删除
                        request.userName = item.BrokerId;
                        request.closeBy = LocalEnv.UserInfo.UserName;
                        request.closeByName = LocalEnv.UserInfo.FullName;
                        request.clOrdId = DataTimeSequence.Instance.GetId();
                        request.accountID = item.AccountID;
                        request.accountName = item.AccountName;
                        request.ifSubBond = item.ifSubBond;
                        request.vipCode = item.vipCode;
                        request.orgType = item.orgType;
                        request.validUntilTime = GetValidUntilTime(item.Info1);
                        request.inf1 = item.Info1;
                        request.info2 = item.Info2;
                        request.channel = item.Channel;
                        request.priceType = item.PriceType;
                        request.updateTime = item.UpdateTime;
                        if (string.IsNullOrEmpty(quoteType))
                        {
                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                            {
                                request.quoteType = "BUYER_INTENTION";
                            }
                            else
                            {
                                request.quoteType = "INTENTION";
                            }
                        }
                        else
                        {
                            request.quoteType = quoteType;
                        }
                        item.Request = request;
                        orderDic[item.IntentionId] = item;

                        IntentionInquiryRequest inquiryRequest = GenIntentionInquiryRequest(item.IntentionId, item.GroupKey, InquiryNotRefList);
                        item.InquiryRequest = inquiryRequest;
                    }
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    // 删除部分
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (item.Status == "0" || item.Status == "1")
                        {
                            System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                            {
                                int inquiryFail = failCount;
                                if (item.InquiryRequest != null)
                                {
                                    GetErrorInfo_Inquiry(item.InquiryRequest,
                                        ref failCount,
                                        ref totalCount,
                                        warnReplyDic,
                                        prohibitReplyDic,
                                        exceptionReplyDic);
                                }
                                else
                                {
                                    totalCount++;
                                }

                                if (inquiryFail - failCount < 1)
                                {
                                    GetErrorInfo(item.Request,
                                            ref failCount,
                                            ref totalCount,
                                            warnReplyDic,
                                            prohibitReplyDic,
                                            exceptionReplyDic);
                                }
                            });
                            try
                            {
                                Thread.Sleep(20);
                            }
                            catch (Exception ex)
                            { }
                        }
                    }

                    //// 撤销部分
                    //List<STC.Common.Model.TBook.OrderCancelRequest> cancelOrdersList = new List<STC.Common.Model.TBook.OrderCancelRequest>();
                    //lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                    //{
                    //    foreach (IntentionQuoteItem item in orderDic.Values)
                    //    {
                    //        if (item.Status == "1")
                    //        {
                    //            DataRow drv = MyOrderProcesser.Processer.GetOrderById(item.OrderID);

                    //            if (drv != null)
                    //            {
                    //                STC.Common.Model.TBook.OrderCancelRequest cancelPo = new STC.Common.Model.TBook.OrderCancelRequest();
                    //                cancelPo.AlgoName = Common.Utils.CommonUtils.ConvertToString(drv[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                    //                cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                    //                cancelPo.OrderID = Common.Utils.CommonUtils.ConvertToString(drv[MyOrderProcesser.STCCODE_ORDER_FIELD]);
                    //                cancelOrdersList.Add(cancelPo);

                    //            }
                    //        }
                    //    }
                    //}
                    //if (cancelOrdersList != null)
                    //{
                    //    foreach (STC.Common.Model.TBook.OrderCancelRequest request in cancelOrdersList)
                    //    {
                    //        ThreadPool.QueueUserWorkItem(o3 =>
                    //        {
                    //            STC.Common.Model.TBook.OrderCancelRequest cancelPo = request as STC.Common.Model.TBook.OrderCancelRequest;
                    //            TBookCancelReplyPo reply = OrdersHandler.Instance.CancelOrder(cancelPo);
                    //            if (reply == null || !reply.IsSuccess())
                    //            {
                    //                List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                    //                TPlaceOrderResult result = new TPlaceOrderResult();
                    //                result.Code = "10003";
                    //                result.DoubleWarn = false;
                    //                result.Reason = (reply == null || string.IsNullOrEmpty(reply.Msg)) ? $"订单号{cancelPo.OrderID}:撤销失败" : $"订单号{cancelPo.OrderID}:{reply.Msg}";
                    //                exceptionResult.Add(result);
                    //                exceptionReplyDic[cancelPo.OrderID] = exceptionResult;
                    //            }

                    //            try
                    //            {
                    //                Thread.Sleep(20);
                    //            }
                    //            catch (Exception ex)
                    //            { }
                    //            totalCount++;
                    //        });
                    //    }
                    //}

                    while (totalCount != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 其中失败{failCount}条", "忽略结果");
                        }
                        //else
                        //{
                        //   Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 全部成功", "忽略结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllIgnoreCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void DoAllRecoverCommand()
        {
            Sub_DoAllRecoverCommand(this.SelectedItems_Working);
        }

        internal void Sub_DoAllRecoverCommand(IList selectedDatas, string quoteType = null)
        {
            if (selectedDatas == null || selectedDatas.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量恢复");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量恢复操作？", "批量恢复"))
            //{
            //    return;
            //}

            SetDoAllBtnEnable(false);
            try
            {
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                foreach (IntentionQuoteItem item in selectedDatas)
                {
                    if ((item.Status != "3" && item.Status != "4")) continue;
                    STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                    request.intentionId = item.IntentionId;
                    request.actionType = "5";// 恢复
                    request.userName = item.BrokerId;
                    request.closeBy = LocalEnv.UserInfo.UserName;
                    request.closeByName = LocalEnv.UserInfo.FullName;
                    request.clOrdId = DataTimeSequence.Instance.GetId();
                    request.accountID = item.AccountID;
                    request.accountName = item.AccountName;
                    request.ifSubBond = item.ifSubBond;
                    request.vipCode = item.vipCode;
                    request.orgType = item.orgType;
                    request.validUntilTime = GetValidUntilTime(item.Info1);
                    request.inf1 = item.Info1;
                    request.info2 = item.Info2;
                    request.channel = item.Channel;
                    request.priceType = item.PriceType;
                    request.updateTime = item.UpdateTime;
                    if (string.IsNullOrEmpty(quoteType))
                    {
                        if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                        {
                            request.quoteType = "BUYER_INTENTION";
                        }
                        else
                        {
                            request.quoteType = "INTENTION";
                        }
                    }
                    else
                    {
                        request.quoteType = quoteType;
                    }
                    item.Request = request;
                    orderDic[item.IntentionId] = item;
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                    }

                    while (totalCount != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //Common.View.Component.CustomMessageBox.Info($"共恢复{totalCount}条, 其中失败{failCount}条", "恢复结果");
                        }
                        //else
                        //{
                        //    Common.View.Component.CustomMessageBox.Info($"共恢复{totalCount}条, 全部成功", "恢复结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllRecoverCommand", e);
            }
            finally
            {
                this.SetDoAllBtnEnable(true);
            }

        }

        private async void DoAllRefCommand()
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量撤销");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量撤销操作？", "批量撤销"))
            //{
            //    return;
            //}

            SetDoAllBtnEnable(false);


            try
            {

                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                foreach (IntentionQuoteItem item in this.SelectedItems_Working)
                {
                    if (item.Status != "1" || item.UserName != LocalEnv.UserInfo.UserName) continue;
                    orderDic[item.IntentionId] = item;
                }

                StringBuilder error = null;
                error = await SubDeleteAllOrders(orderDic);
                if (error != null && error.Length > 0)
                {
                    Common.View.Component.CustomMessageBox.Error(string.Format($"撤销订单失败, 原因:{error}"));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllRefCommand1", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private async Task<StringBuilder> SubDeleteAllOrders(Dictionary<string, IntentionQuoteItem> orderDic)
        {
            StringBuilder error = new StringBuilder();

            await Task.Run(() =>
            {
                List<STC.Common.Model.TBook.OrderCancelRequest> cancelOrdersList = new List<STC.Common.Model.TBook.OrderCancelRequest>();
                int count = 0;
                ConcurrentDictionary<string, TBookCancelReplyPo> dic = new ConcurrentDictionary<string, TBookCancelReplyPo>();

                lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        DataRow drv = MyOrderProcesser.Processer.GetOrderById(item.OrderID);

                        if (drv != null)
                        {
                            STC.Common.Model.TBook.OrderCancelRequest cancelPo = new STC.Common.Model.TBook.OrderCancelRequest();
                            cancelPo.AlgoName = Common.Utils.CommonUtils.ConvertToString(drv[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                            cancelPo.OrderID = Common.Utils.CommonUtils.ConvertToString(drv[MyOrderProcesser.STCCODE_ORDER_FIELD]);
                            cancelOrdersList.Add(cancelPo);
                            count++;
                        }
                    }
                }

                if (cancelOrdersList != null)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        foreach (STC.Common.Model.TBook.OrderCancelRequest request in cancelOrdersList)
                        {

                            if (request is STC.Common.Model.TBook.OrderCancelRequest)
                            {
                                STC.Common.Model.TBook.OrderCancelRequest cancelPo = request as STC.Common.Model.TBook.OrderCancelRequest;
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

        internal void DoAllNewCommand()
        {
            Sub_DoAllNewCommand(this.SelectedItems_Working);
        }

        internal void Sub_DoAllNewCommand(IList selectedDatas)
        {
            WriteDBLogHelper.Instance.GenRecord("personal", "005", "批量-取消发内部");

            if (selectedDatas == null || selectedDatas.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "取消发内部");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量取消发内部操作？", "取消发内部"))
            //{
            //    return;
            //}

            SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                foreach (IntentionQuoteItem item in selectedDatas)
                {
                    if (item.Status != "1" || item.UserName != LocalEnv.UserInfo.UserName) continue;
                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.IntentionId] = item;
                }

                //if (exceptionReplyDic_local.Count > 1)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.IntentionId))
                        {
                            continue;
                        }
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                            request.intentionId = item.IntentionId;
                            request.actionType = "7";// 隐藏
                            request.closeBy = LocalEnv.UserInfo.UserName;
                            request.closeByName = LocalEnv.UserInfo.FullName;
                            request.userName = item.BrokerId;
                            request.updateTime = item.UpdateTime;
                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                            {
                                request.quoteType = "BUYER_INTENTION";
                            }
                            else
                            {
                                request.quoteType = "INTENTION";
                            }
                            orderDic[item.IntentionId].Request = request;

                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                        try
                        {
                            Thread.Sleep(20);
                        }
                        catch (Exception ex)
                        { }
                    }

                    while ((totalCount + failCount_local) != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllNewCommand1", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void DoAllToReferenceCommand()
        {
            Sub_DoAllToReferenceCommand(this.SelectedItems_Working);
        }

        internal void Sub_DoAllToReferenceCommand(IList selectedDatas)
        {
            if (selectedDatas == null || selectedDatas.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量转参考");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量转参考操作？", "批量转参考"))
            //{
            //    return;
            //}


            SetDoAllBtnEnable(false);
            try
            {
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                foreach (IntentionQuoteItem item in selectedDatas)
                {
                    if ((item.Status != "0") || item.UserName != LocalEnv.UserInfo.UserName) continue;
                    if (item.Status == "0")
                    {
                        STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                        request.intentionId = item.IntentionId;
                        request.actionType = "8";// 转参考
                        request.userName = item.BrokerId;
                        request.closeBy = LocalEnv.UserInfo.UserName;
                        request.closeByName = LocalEnv.UserInfo.FullName;
                        request.clOrdId = DataTimeSequence.Instance.GetId();
                        request.accountID = item.AccountID;
                        request.accountName = item.AccountName;
                        request.ifSubBond = item.ifSubBond;
                        request.vipCode = item.vipCode;
                        request.orgType = item.orgType;
                        request.validUntilTime = GetValidUntilTime(item.Info1);
                        request.inf1 = item.Info1;
                        request.info2 = item.Info2;
                        request.channel = item.Channel;
                        request.priceType = item.PriceType;
                        request.updateTime = item.UpdateTime;
                        item.Request = request;
                        if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                        {
                            request.quoteType = "BUYER_INTENTION";
                        }
                        else
                        {
                            request.quoteType = "INTENTION";
                        }
                        orderDic[item.IntentionId] = item;
                    }
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    // 转参考部分
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (item.Status == "0")
                        {
                            System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                            {
                                GetErrorInfo(item.Request,
                                        ref failCount,
                                        ref totalCount,
                                        warnReplyDic,
                                        prohibitReplyDic,
                                        exceptionReplyDic);
                            });
                            try
                            {
                                Thread.Sleep(20);
                            }
                            catch (Exception ex)
                            { }
                        }
                    }

                    while (totalCount != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 其中失败{failCount}条", "忽略结果");
                        }
                        //else
                        //{
                        //   Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 全部成功", "忽略结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllToReferenceCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void DoAllToFinishCommand()
        {
            WriteDBLogHelper.Instance.GenRecord("personal", "007", "批量-完成");

            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                return;
            }

            List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
            if (this.SelectedItems_Working != null && this.SelectedItems_Working.Count > 0)
            {
                foreach (IntentionQuoteItem item in this.SelectedItems_Working)
                {
                    if (item.Status == "0" || item.Status == "1")
                    {
                        modifyList.Add(item);
                    }
                }
            }

            IntentionQuoteItem currentItem = null;
            ObservableCollection<IntentionMarketItem> datas = null;
            if (LocalEnv.ConfInfo.IsInquiry && this.SelectedItems_Working != null && this.SelectedItems_Working.Count == 1)
            {
                if (this.SelectedItem_Working != null)
                {
                    
                    if (this._Cache_InquiryQuote_Dic.ContainsKey(this.SelectedItem_Working.IntentionId)
                       && this._Cache_InquiryQuote_Dic[this.SelectedItem_Working.IntentionId].Count > 0)
                    {
                        datas = new ObservableCollection<IntentionMarketItem>(this._Cache_InquiryQuote_Dic[this.SelectedItem_Working.IntentionId].Values.Where<IntentionMarketItem>(a => a.Status == "1" && a.SendType != 3 && a.OptionType != "sendIntentQQGroups").OrderByDescending(a => a.UpdateTime));
                        if (datas != null && datas.Count > 0)
                        {
                            currentItem = this.SelectedItem_Working;
                        }
                    }
                }
            }

            if (modifyList.Count > 0)
            {
                ModifyPersonalRemarksView x = new ModifyPersonalRemarksView(currentItem, datas);
                if (this.SelectedItems_Working != null && this.SelectedItems_Working.Count == 1)
                {
                    x.memo.Text = this.SelectedItem_Working.PersonalRemarks;
                }
                if (x.ShowDialog() == true)
                {
                    EditPersonalMarks(modifyList, x.InquiryNotRefList, x.memo.Text, true);
                    //Sub_DoAllToFinishCommand(this.SelectedItems_Working);
                }
            }
        }

        internal void Sub_DoAllToFinishCommand(IList selectedDatas)
        {
            if (selectedDatas == null || selectedDatas.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量转参考");
                return;
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量转参考操作？", "批量转参考"))
            //{
            //    return;
            //}


            SetDoAllBtnEnable(false);
            try
            {
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                foreach (IntentionQuoteItem item in selectedDatas)
                {
                    STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                    request.intentionId = item.IntentionId;
                    request.actionType = "9";// 已完成
                    request.userName = item.BrokerId;
                    request.closeBy = LocalEnv.UserInfo.UserName;
                    request.closeByName = LocalEnv.UserInfo.FullName;
                    request.updateTime = item.UpdateTime;
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                    {
                        request.quoteType = "BUYER_INTENTION";
                    }
                    else
                    {
                        request.quoteType = "INTENTION";
                    }
                    item.Request = request;
                    orderDic[item.IntentionId] = item;
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    // 转参考部分
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                        try
                        {
                            Thread.Sleep(20);
                        }
                        catch (Exception ex)
                        { }
                    }

                    while (totalCount != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 其中失败{failCount}条", "忽略结果");
                        }
                        //else
                        //{
                        //   Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 全部成功", "忽略结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllToReferenceCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        internal void Sub_OnMarketCommand(IntentionQuoteItem item)
        {
            if (item == null)
            {
                STC.Common.View.Component.CustomMessageBox.Info("请先选择个需", "提示");
                return;
            }

            if (LocalEnv.ConfInfo.IsPAYL)
            {
                PublishQuoteView_PA publishQuoteView_PA = new PublishQuoteView_PA();
                PublishQuoteViewModel_PA vm_pa = new PublishQuoteViewModel_PA();
                List<IntentionQuoteItem> list = new List<IntentionQuoteItem>();
                list.Add(item);
                vm_pa.Init(list, publishQuoteView_PA);
                publishQuoteView_PA.DataContext = vm_pa;
                publishQuoteView_PA.Show();
            }
            else
            {
                if (!WindowManager.Instance.IsExist($"PublishQuoteView_{item.IntentionId}"))
                {
                    PublishQuoteView win = new PublishQuoteView();
                    PublishQuoteViewModel vm = win.DataContext as PublishQuoteViewModel;
                    if (vm != null)
                    {
                        vm.CurrentItem = item;
                        vm.InitData(win);
                    }
                    WindowManager.Instance.OpenOnlyOneWindow($"PublishQuoteView_{item.IntentionId}", win);
                }
            }
            
        }


        private void RefreshCheckBox()
        {
            try
            {
                while (true)
                {
                    //if (isAdjustAllChkStatus_Ignore)
                    //{
                    //    isAdjustAllChkStatus_Ignore = false;
                    //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //    {
                    //        this.AdjustAllChkStatus_Ignore();
                    //    }));
                    //}

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
            catch (ThreadAbortException)
            {
                // ignore.
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        #region 待处理
        private bool _hasSelected = false;
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

        protected volatile bool isAdjustAllChkStatus = false;
        public void CheckBoxAllValueChange(bool isAllChecked)
        {
            int workingDataCount = 0;
            lock (_workingLock)
            {
                foreach (IntentionQuoteItem item in this.workingViewSource.View)
                {
                    if ((item.Status == "0" || item.Status == "1" || item.Status == "3" || item.Status == "4") && (item.CloseBy == null || item.CloseBy == LocalEnv.UserInfo.UserName))
                    {
                        item.IsChecked = isAllChecked;
                        workingDataCount++;
                    }

                }
            }


            if (workingDataCount == 0)
            {
                IsAllChecked = false;
                _hasSelected = false;
            }
            else
            {
                IsAllChecked = isAllChecked;
                _hasSelected = isAllChecked;
            }
        }

        public void AdjustAllChkStatus()
        {
            bool isCheckAll = true;

            int workingDataCount = 0;
            lock (_workingLock)
            {
                this._hasSelected = false;
                foreach (IntentionQuoteItem item in this.workingViewSource.View)
                {
                    if ((item.Status == "0" || item.Status == "1" || item.Status == "3" || item.Status == "4") && (item.CloseBy == null || item.CloseBy == LocalEnv.UserInfo.UserName))
                    {
                        workingDataCount++;
                        if (item.IsChecked == true)
                        {
                            this._hasSelected = true;
                        }
                        if (item.IsChecked == false)
                        {
                            isCheckAll = false;
                        }
                        if (this._hasSelected == true && isCheckAll == false)
                        {
                            break;
                        }
                    }
                }
                IsAllChecked = isCheckAll && workingDataCount != 0;
            }

        }

        public void CheckBoxSingleClick()
        {
            isAdjustAllChkStatus = true;
        }
        #endregion

        #region 已忽略
        //private bool _hasSelected_Ignore = false;
        //// 是否选中全选多选框
        //private bool _IsAllChecked_Ignore = false;
        //public bool IsAllChecked_Ignore
        //{
        //    get
        //    {
        //        return _IsAllChecked_Ignore;
        //    }

        //    set
        //    {
        //        if (_IsAllChecked_Ignore != value)
        //        {
        //            _IsAllChecked_Ignore = value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        //private volatile bool isAdjustAllChkStatus_Ignore = false;
        //public void CheckBoxAllValueChange_Ignore(bool isAllChecked)
        //{

        //    lock (_ignoreLock)
        //    {
        //        foreach (IntentionQuoteItem item in this.IgnoreViewSource.View)
        //        {

        //            item.IsChecked = isAllChecked;
        //        }
        //    }

        //    if (this.IgnoreViewSource.View.IsEmpty == true)
        //    {
        //        IsAllChecked_Ignore = false;
        //        _hasSelected_Ignore = false;
        //    }
        //    else
        //    {
        //        IsAllChecked_Ignore = isAllChecked;
        //        _hasSelected_Ignore = isAllChecked;
        //    }
        //}

        //public void AdjustAllChkStatus_Ignore()
        //{
        //    bool isCheckAll = true;

        //    lock (_workingLock)
        //    {
        //        foreach (IntentionQuoteItem item in this.IgnoreViewSource.View)
        //        {
        //            if (item.IsChecked == true)
        //            {
        //                _hasSelected_Ignore = true;
        //            }
        //            if (item.IsChecked == false)
        //            {
        //                isCheckAll = false;
        //            }
        //            if (_hasSelected_Ignore == true && isCheckAll == false)
        //            {
        //                break;
        //            }
        //        }
        //        IsAllChecked_Ignore = isCheckAll && this.IgnoreViewSource.View.IsEmpty == false;
        //    }

        //}

        //public void CheckBoxSingleClick_Ignore()
        //{
        //    isAdjustAllChkStatus_Ignore = true;
        //}
        #endregion

        internal void FilterClientType(AutoCompleteArgs args)
        {
            List<CrmInst> result = GetCrmInst(); ;
            if (this._selectedInstitution_Edit != null && !string.IsNullOrEmpty(this._selectedInstitution_Edit.insttypevalue))
            {
                result = result.Where(item => item.shortnamecn == this._selectedInstitution_Edit.insttypevalue).ToList();
            }
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                result = result.Where(item => item.shortnamecn.Contains(args.Pattern)).ToList();
            }

            args.DataSource = result;
        }
        private List<CrmInst> GetCrmInst()
        {
            List<CrmInst> result = new List<CrmInst>();
            Dictionary<string, QuoteTag> dic = BaseHandler.Instance.GetQuoteTagDic();

            if (dic != null && dic.ContainsKey("ORG_TYPE") && dic["ORG_TYPE"] != null
                && dic["ORG_TYPE"].childTags != null && dic["ORG_TYPE"].childTags.Count > 0)
            {
                foreach (ChildTag ct in dic["ORG_TYPE"].childTags)
                {
                    CrmInst bankCrm = new CrmInst();
                    bankCrm.shortnamecn = ct.childTagName;
                    bankCrm.instcode = ct.childTagValue;
                    result.Add(bankCrm);
                }
            }
            return result;
        }

        internal void FilterInstitution(AutoCompleteArgs args)
        {

            //BankListViewSource.View.Filter = x =>
            //{
            //    if (string.IsNullOrEmpty(args.Pattern))
            //        return true;
            //    else
            //        return ((KeyValueData)x).Key.Contains(args.Pattern) || ((KeyValueData)x).Value.Contains(args.Pattern);
            //};
            //args.DataSource = BankListViewSource.View;
            string currentTraderID = string.Empty;
            string currentTraderName = string.Empty;
            if (this.SelectedTrader_Edit != null)
            {
                currentTraderID = this.SelectedTrader_Edit.idbkey;
                currentTraderName = this.SelectedTrader_Edit.namecn;
            }

            List<string> instIdList = null;
            if (!string.IsNullOrEmpty(currentTraderID))
            {
                List<CrmContact> crList = null;
                if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                {
                    crList = WtUtils.GetContactInfoPo(currentTraderName, null);
                }
                else
                {
                    crList = WtUtils.GetDBContactInfoPo(currentTraderName, null);
                }

                if (crList != null && crList.Count > 0)
                {
                    instIdList = new List<string>();
                    foreach (CrmContact crItem in crList)
                    {
                        if (crItem.idbkey == currentTraderID)
                        {
                            if (!instIdList.Contains(crItem.instid))
                            {
                                instIdList.Add(crItem.instid);
                            }
                        }
                    }
                }
            }

            List<CrmInst> result = new List<CrmInst>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {

                        List<CrmInst> selectDatas = WtUtils.GetInstitutionsInfoPo(str, 2000, instIdList);
                        result.AddRange(selectDatas);
                    }
                }
                else
                {
                    result = WtUtils.GetInstitutionsInfoPo(args.Pattern.Trim(), 2000, instIdList);
                }
            }
            else
            {
                List<CrmInst> myInstList = Common.Utils.CommonUtils.GetMyInstList();
                if ((instIdList == null || instIdList.Count == 0) && myInstList.Count > 0)
                {
                    result.AddRange(myInstList);
                }
                else
                {
                    result.AddRange(WtUtils.GetInstitutionsInfoPo(instIdList));
                }
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new InstitutionComparer(args.Pattern)).ToList();
                }
            }

            args.DataSource = result;
        }

        internal void FilterTrader(AutoCompleteArgs args)
        {

            //UserListViewSource.View.Filter = x =>
            //{
            //    if (string.IsNullOrEmpty(args.Pattern))
            //        return true;
            //    else
            //        return ((KeyValueData)x).Key.Contains(args.Pattern) || ((KeyValueData)x).Value.Contains(args.Pattern);
            //};
            //args.DataSource = UserListViewSource.View;
            string currentInstID = null;
            if (this.SelectedItem_Working != null)
            {
                currentInstID = this.SelectedItem_Working.InstitutionId;
            }

            currentInstID = string.IsNullOrEmpty(currentInstID) ? null : currentInstID;

            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = WtUtils.GetDBContactInfoPo(str, currentInstID);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = WtUtils.GetDBContactInfoPo(args.Pattern.Trim(), currentInstID);
            }
            else
            {
                List<CrmContact> myTraderList = Common.Utils.CommonUtils.GetMyContract();
                if (string.IsNullOrEmpty(currentInstID) && myTraderList.Count > 0)
                {
                    result = myTraderList;
                }
                else
                {
                    result = WtUtils.GetDBContactInfoPo(null, currentInstID);
                }
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new TraderComparer(args.Pattern)).ToList();
                }
            }

            args.DataSource = result;
        }

        internal void FilterTraderLocal(AutoCompleteArgs args)
        {
            string currentInstID = null;
            if (this.SelectedInstintuion_Edit != null)
            {
                currentInstID = this.SelectedInstintuion_Edit.idbkey;
            }

            currentInstID = string.IsNullOrEmpty(currentInstID) ? null : currentInstID;

            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = WtUtils.GetContactInfoPo(str, currentInstID);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = WtUtils.GetContactInfoPo(args.Pattern.Trim(), currentInstID);
            }
            else
            {
                List<CrmContact> myTraderList = Common.Utils.CommonUtils.GetMyContract();
                if (string.IsNullOrEmpty(currentInstID) && myTraderList.Count > 0)
                {
                    result = myTraderList;
                }
                else
                {
                    result.AddRange(WtUtils.GetContactInfoPo(null, currentInstID));
                }
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new TraderComparer(args.Pattern)).ToList();
                }
            }

            args.DataSource = result;
        }


        internal void FilterBroker(AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            STCUserReplyPo result = AdminHandler.Instance.GetCachestcUserQuery();

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                list = result.Datas.Where(d => d.ActionIds.Contains("600102")
                && ((d.Username != null && d.Username.Contains(args.Pattern)) || (d.Name != null && d.Name.Contains(args.Pattern)))).ToList();//多条件筛选，新功能增加
            }
            else
            {
                list = result.Datas.Where(d => d.ActionIds.Contains("600102")).ToList();
            }

            args.DataSource = list;
        }

        private string _selectFilterType;

        public string SelectFilterType
        {
            get
            {
                return _selectFilterType;
            }
            set
            {
                _selectFilterType = value;
            }
        }

        private string _FilterText;

        public string FilterText
        {
            get { return _FilterText; }
            set
            {
                if (_FilterText != value)
                {
                    _FilterText = value;
                    this.OnPropertyChanged();
                    ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                    this.workingViewSource.View?.Refresh();
                    SetSearchDes();
                    NotifyFilter();

                    if (!string.IsNullOrEmpty(value))
                    {
                        WriteDBLogHelper.Instance.GenRecord("personal", "011", "搜索框");
                    }
                }
            }
        }

        internal void RefreshView()
        {
            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            this.workingViewSource.View?.Refresh();
            NotifyFilter();
        }

        internal void FilterData()
        {
            this.WorkingViewSource.View.Filter = x =>
            {
                IntentionQuoteItem item = (IntentionQuoteItem)x;

                bool flag = true;
                if (item == null)
                    return flag;

                //if (((quoteTypeFilterList == null || quoteTypeFilterList.Count == 0
                //   || (quoteTypeFilterList != null && quoteTypeFilterList.Count > 0 && quoteTypeFilterList.Contains(item.QuoteType)))) == false)
                //    return false;

                //if (string.IsNullOrEmpty(this.ClientFilterText) == false
                //    && (item.TraderName == null || !item.TraderName.Contains(this.ClientFilterText))
                //    && (item.InstitutionName == null || !item.InstitutionName.Contains(this.ClientFilterText)))
                //{
                //    return false;
                //}

                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    if (string.IsNullOrEmpty(this.FilterText) == false
                    && (item.SecurityId == null || !item.SecurityId.Contains(this.FilterText))
                    && (item.Symbol == null || !item.Symbol.Contains(this.FilterText))
                    && (item.IssuerName == null || !item.IssuerName.Contains(this.FilterText))
                    && (item.WarranterName == null || !item.WarranterName.Contains(this.FilterText))
                    && (item.TraderName_Buyer == null || !item.TraderName_Buyer.Contains(this.FilterText))
                    && (item.InvestorName == null || !item.InvestorName.Contains(this.FilterText)))
                        return false;
                }
                else
                {
                    if (string.IsNullOrEmpty(this.FilterText) == false
                    && (item.SecurityId == null || !item.SecurityId.Contains(this.FilterText))
                    && (item.Symbol == null || !item.Symbol.Contains(this.FilterText))
                    //&& (item.RoleName == null || !item.RoleName.Contains(this.FilterText))
                    && (item.BrokerName == null || !item.BrokerName.Contains(this.FilterText))
                    && (item.IssuerName == null || !item.IssuerName.Contains(this.FilterText))
                    && (item.WarranterName == null || !item.WarranterName.Contains(this.FilterText))
                    && (item.TraderName == null || !item.TraderName.Contains(this.FilterText))
                    && (item.InstitutionName == null || !item.InstitutionName.Contains(this.FilterText)))
                        return false;
                }
                

                //if (this._focusTagDic != null && this._focusTagDic.ContainsKey(Consts.SIDE))
                //{
                //    List<string> sidefilter = this._focusTagDic[Consts.SIDE];
                //    if (sidefilter.Count > 0)
                //    {
                //        string otherside = item.Side == "1" ? "2" : "1";
                //        if ((sidefilter.Contains("0001") && item.Side == "1")
                //        || (sidefilter.Contains("0010") && item.Side == "2")
                //        || (sidefilter.Contains("0011") && _Cache_InnerOrders_Dic[item.SecurityId].Values.Any(
                //            i => i.Side == otherside 
                //            && (statusFilterList == null || statusFilterList.Count == 0 || (statusFilterList != null && statusFilterList.Count > 0 && statusFilterList.Contains(i.Status))))))
                //        {
                //            // ignore
                //        }
                //        else
                //        {
                //            return false;
                //        }
                //    }
                //}

                bool displayFlag = true;
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    //if (STC.Common.Utils.CommonUtils.IsOwn())
                    //{
                    //    if (STC.Common.Utils.CommonUtils.IsInvestor())
                    //    {
                    //        displayFlag = item.CloseBy == null || item.CloseBy == LocalEnv.UserInfo.UserName || item.Investor == LocalEnv.UserInfo.UserName;
                    //    }
                    //    else
                    //    {
                    //        displayFlag = item.CloseBy == null || item.CloseBy == LocalEnv.UserInfo.UserName || item.Trader_Buyer == LocalEnv.UserInfo.UserName;
                    //    }
                    //}
                }
                else
                {
                    displayFlag = item.CloseBy == null || item.CloseBy == LocalEnv.UserInfo.UserName;
                }

                if (displayFlag == false)
                    return false;

                //&& (item.CloseBy == null || item.CloseBy == LocalEnv.UserInfo.UserName)
                if (((statusFilterList == null || statusFilterList.Count == 0
                    || (statusFilterList != null && statusFilterList.Count > 0 && statusFilterList.Contains(item.Status)))
                    && (item.Status == "0" || item.Status == "1" || item.Status == "2" || item.Status == "3" || item.Status == "4"
                    || item.Status == "80" || item.Status == "81" || item.Status == "82" || item.Status == "83" || item.Status == "84")) == false)
                    return false;


                string bondSubType_tag = item.SignDic1[Consts.BOND_SUB_TYPE];
                string securityType = STC.Common.Utils.CommonUtils.GetQQSecurityType(bondSubType_tag, item.BondSubType);
                if ((this.DisplayMode == "BNC" && securityType != "2")
                    || (this.DisplayMode == "BCO" && securityType != "1")
                    || (this.DisplayMode == "LOCAL" && securityType != "21")
                    || (this.DisplayMode == "NCD" && securityType != "3"))
                {
                    return false;
                }

                //if ((this.DisplayMode == "BNC" && item.SecurityType != "BNC" && item.BondSubType != "PSB")
                //    || (string.IsNullOrEmpty(this.DisplayMode) == false && this.DisplayMode != "BNC" && item.SecurityType == "BNC" && item.BondSubType != "PSB")
                //    || (this.DisplayMode == "BNC" && item.BondSubType == "PSB"))
                //    return false;

                //if (!string.IsNullOrEmpty(this.StartTermDY) && item.TermDays < STC.Common.Utils.CommonUtils.GetDays(this.StartTermDY))
                //{
                //    return false;
                //}
                //if (!string.IsNullOrEmpty(this.EndTermDY) && item.TermDays > STC.Common.Utils.CommonUtils.GetDays(this.EndTermDY))
                //{
                //    return false;
                //}

                //if (!string.IsNullOrEmpty(this.StartCoupon) && (item.CouponRateCurrent == null || item.CouponRateCurrent < this.StartCoupon.ConvertToDecimal()))
                //{
                //    return false;
                //}
                //if (!string.IsNullOrEmpty(this.EndCoupon) && (item.CouponRateCurrent == null || item.CouponRateCurrent > this.EndCoupon.ConvertToDecimal()))
                //{
                //    return false;
                //}

                //if (!string.IsNullOrEmpty(this.StartPrice) && !string.IsNullOrEmpty(this.EndPrice))
                //{
                //    decimal startPrice = this.StartPrice.ConvertToDecimal();
                //    decimal endPrice = this.EndPrice.ConvertToDecimal();
                //    if (!(item.Price >= startPrice && item.Price <= endPrice))
                //    {
                //        return false;
                //    }
                //}
                //else if (!string.IsNullOrEmpty(this.StartPrice) && string.IsNullOrEmpty(this.EndPrice))
                //{
                //    decimal startPrice = this.StartPrice.ConvertToDecimal();
                //    if (!(item.Price >= startPrice))
                //    {
                //        return false;
                //    }
                //}
                //else if (string.IsNullOrEmpty(this.StartPrice) && !string.IsNullOrEmpty(this.EndPrice))
                //{
                //    decimal endPrice = this.EndPrice.ConvertToDecimal();
                //    if (!(item.Price <= endPrice))
                //    {
                //        return false;
                //    }
                //}


                //decimal? chinaBondValue = item.ChinaBondValuation;
                //if (item.ChinaBondExercise.HasValue)
                //{
                //    chinaBondValue = item.ChinaBondExercise;
                //}

                //if (!string.IsNullOrEmpty(this.StartChinaBondValue) && !string.IsNullOrEmpty(this.EndChinaBondValue))
                //{
                //    decimal startChinaBondValue = this.StartChinaBondValue.ConvertToDecimal();
                //    decimal endChinaBondValue = this.EndChinaBondValue.ConvertToDecimal();
                //    if (!(chinaBondValue >= startChinaBondValue && chinaBondValue <= endChinaBondValue))
                //    {
                //        return false;
                //    }
                //}
                //else if (!string.IsNullOrEmpty(this.StartChinaBondValue) && string.IsNullOrEmpty(this.EndChinaBondValue))
                //{
                //    decimal startChinaBondValue = this.StartChinaBondValue.ConvertToDecimal();
                //    if (!(chinaBondValue >= startChinaBondValue))
                //    {
                //        return false;
                //    }
                //}
                //else if (string.IsNullOrEmpty(this.StartChinaBondValue) && !string.IsNullOrEmpty(this.EndChinaBondValue))
                //{
                //    decimal endChinaBondValue = this.EndChinaBondValue.ConvertToDecimal();
                //    if (!(chinaBondValue <= endChinaBondValue))
                //    {
                //        return false;
                //    }
                //}

                //// bid-中债
                //if (item.Side == "1")
                //{
                //    if (item.Price.HasValue && item.ChinaBondValuation.HasValue)
                //    {
                //        decimal value = (item.Price.Value - item.ChinaBondValuation.Value) * 100;
                //        if (!string.IsNullOrEmpty(this.StartBidBp) && !string.IsNullOrEmpty(this.EndBidBp))
                //        {

                //            decimal startBp = this.StartBidBp.ConvertToDecimal();
                //            decimal endBp = this.EndBidBp.ConvertToDecimal();
                //            if (!(value >= startBp && value <= endBp))
                //            {
                //                return false;
                //            }
                //        }
                //        else if (!string.IsNullOrEmpty(this.StartBidBp) && string.IsNullOrEmpty(this.EndBidBp))
                //        {
                //            decimal startPrice = this.StartBidBp.ConvertToDecimal();
                //            if (!(value >= startPrice))
                //            {
                //                return false;
                //            }
                //        }
                //        else if (string.IsNullOrEmpty(this.StartBidBp) && !string.IsNullOrEmpty(this.EndBidBp))
                //        {
                //            decimal endPrice = this.EndBidBp.ConvertToDecimal();
                //            if (!(value <= endPrice))
                //            {
                //                return false;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        if (!string.IsNullOrEmpty(this.StartBidBp) || !string.IsNullOrEmpty(this.EndBidBp))
                //        {
                //            return false;
                //        }
                //    }
                //}

                //// 中债-ofr
                //if (item.Side == "2")
                //{
                //    if (item.Price.HasValue && item.ChinaBondValuation.HasValue)
                //    {
                //        decimal value = (item.ChinaBondValuation.Value - item.Price.Value) * 100;
                //        if (!string.IsNullOrEmpty(this.StartOfrBp) && !string.IsNullOrEmpty(this.EndOfrBp))
                //        {
                //            decimal startPrice = this.StartOfrBp.ConvertToDecimal();
                //            decimal endPrice = this.EndOfrBp.ConvertToDecimal();
                //            if (!(value >= startPrice && value <= endPrice))
                //            {
                //                return false;
                //            }
                //        }
                //        else if (!string.IsNullOrEmpty(this.StartOfrBp) && string.IsNullOrEmpty(this.EndOfrBp))
                //        {
                //            decimal startPrice = this.StartOfrBp.ConvertToDecimal();
                //            if (!(value >= startPrice))
                //            {
                //                return false;
                //            }
                //        }
                //        else if (string.IsNullOrEmpty(this.StartOfrBp) && !string.IsNullOrEmpty(this.EndOfrBp))
                //        {
                //            decimal endPrice = this.EndOfrBp.ConvertToDecimal();
                //            if (!(value <= endPrice))
                //            {
                //                return false;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        if (!string.IsNullOrEmpty(this.StartOfrBp) || !string.IsNullOrEmpty(this.EndOfrBp))
                //        {
                //            return false;
                //        }
                //    }
                //}

                bool result = true;

                //if (item.Sign != 0 && this._focusTagDic != null)
                //{
                //    foreach (KeyValuePair<string, string> pair in item.SignDic)
                //    {
                //        CheckSign(item, pair.Key, pair.Value, ref result, this._focusTagDic);

                //        if (!result)
                //            return result;
                //    }
                //}

                //if (item.Sign1 != 0 && this._focusTagDic != null)
                //{
                //    foreach (KeyValuePair<string, string> pair in item.SignDic1)
                //    {
                //        CheckSign(item, pair.Key, pair.Value, ref result, this._focusTagDic);

                //        if (!result)
                //            return result;
                //    }
                //}

                //if (item.Sign2 != 0 && this._focusTagDic != null)
                //{
                //    foreach (KeyValuePair<string, string> pair in item.SignDic2)
                //    {
                //        CheckSign(item, pair.Key, pair.Value, ref result, this._focusTagDic);

                //        if (!result)
                //            return result;
                //    }
                //}

                //if (item.Sign3 != 0 && this._focusTagDic != null)
                //{
                //    foreach (KeyValuePair<string, string> pair in item.SignDic3)
                //    {
                //        CheckSign(item, pair.Key, pair.Value, ref result, this._focusTagDic);

                //        if (!result)
                //            return result;
                //    }
                //}
                //else if (result && item.Sign3 != 0 && "BNC".Equals(item.SecurityType) && this._focusTagDic != null)
                //{
                //    foreach (KeyValuePair<string, string> pair in item.SignDic3)
                //    {
                //        string key = pair.Key;
                //        string value = pair.Value;
                //        if (key == Consts.RANKING_TYPE)
                //        {
                //            result = result
                //               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
                //               || (_focusTagDic[key].Contains("xx") && !rankingtype_arr.Contains(value) && !"BNC".Equals(item.SecurityType));
                //        }
                //        else if (key == Consts.RATING_CURRENT)
                //        {
                //            result = result
                //               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
                //               || (_focusTagDic[key].Contains("xx") && !ratingCurrent_arr.Contains(value) && !"BNC".Equals(item.SecurityType));
                //        }
                //        else if (key == Consts.CDC_IMPLITED_RATING)
                //        {
                //            result = result
                //               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
                //               || (_focusTagDic[key].Contains("xx") && !cdcImplitedRating_arr.Contains(value) && !"BNC".Equals(item.SecurityType));
                //        }
                //        else if (key == Consts.ISSUER_RATING_CURRENT_CBR
                //            || key == Consts.CSI_IMPLIED_RATING)
                //        {
                //            result = result
                //               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
                //               || (_focusTagDic[key].Contains("01000") && value == "00000" && !"BNC".Equals(item.SecurityType)); // 00000也算其他
                //        }
                //    }
                //}
                return result;
            };
        }

        protected List<string> statusFilterList = new List<string>();

        internal void ProcessFilterData(string[] statusArr)
        {
            statusFilterList.Clear();

            if (statusArr != null && statusArr.Length > 0)
            {
                foreach (string statusType in statusArr)
                {
                    if (statusType == "有效")
                    {
                        statusFilterList.Clear();
                        statusFilterList.Add("0");
                        statusFilterList.Add("1");
                    }

                    if (statusType == "待发布")//隐藏
                    {
                        statusFilterList.Add("0");
                    }
                    else if (statusType == "已发布")//正常
                    {
                        statusFilterList.Add("1");
                    }
                    else if (statusType == "已完成")
                    {
                        statusFilterList.Add("2");
                    }
                    //else if (statusType.Content.ToString() == "已删除")
                    //{
                    //    statusFilterList.Add("3");
                    //}
                    else if (statusType == "已撤销")
                    {
                        statusFilterList.Add("3");
                        statusFilterList.Add("4");
                    }
                }
            }
            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            this.WorkingViewSource.View?.Refresh();
            NotifyFilter();
            DealSuspendDatas();
        }

        internal void ProcessFilterData_Buyer(string[] statusArr)
        {
            statusFilterList.Clear();

            if (statusArr != null && statusArr.Length > 0)
            {
                foreach (string statusType in statusArr)
                {
                    if (statusType == "待发布")//隐藏
                    {
                        statusFilterList.Add("0");
                    }
                    else if (statusType == "已发布")//正常
                    {
                        statusFilterList.Add("1");
                    }
                    else if (statusType == "待处理")
                    {
                        statusFilterList.Add("80");
                    }
                    else if (statusType == "处理中")
                    {
                        statusFilterList.Add("81");
                    }
                    else if (statusType == "撤销中")
                    {
                        statusFilterList.Add("82");
                    }
                    else if (statusType == "已撤销")
                    {
                        statusFilterList.Add("83");
                        statusFilterList.Add("3");
                        statusFilterList.Add("4");
                    }
                    else if (statusType == "已完成")
                    {
                        statusFilterList.Add("84");
                        statusFilterList.Add("2");
                    }
                }
            }
            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            this.WorkingViewSource.View?.Refresh();
            NotifyFilter();
            DealSuspendDatas();
        }


        protected List<string> matchStatusFilterList = new List<string>();

        internal void ProcessMatchStatusFilterData(string[] statusArr)
        {
            matchStatusFilterList.Clear();

            if (statusArr != null && statusArr.Length > 0)
            {
                matchStatusFilterList.AddRange(statusArr);
            }
            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            this.WorkingViewSource.View?.Refresh();
        }

        //private List<string> quoteTypeFilterList = new List<string>();


        //internal void ProcessQuoteTypeData(List<string> statusArr)
        //{
        //    quoteTypeFilterList = statusArr;

        //    this.WorkingViewSource.View?.Refresh();
        //    NotifyFilter();
        //}

        /// <summary>
        /// 编辑记录
        /// </summary>
        /// <param name="editBtn"></param>
        private void EditRow(Button editBtn)
        {
            if (editBtn != null && editBtn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem item = editBtn.DataContext as IntentionQuoteItem;
                EditRow(item);
            }
        }

        internal void EditRow(IntentionQuoteItem item)
        {
            item.IsEdit = "0";
            item.InstitutionName_Edit = item.InstitutionName;
            //item.InstitutionName_Text = item.InstitutionName;
            item.TraderName_Edit = item.TraderName;
            //item.TraderName_Text = item.TraderName;
            item.Side_Edit = item.Side;
            item.Price_Edit = item.Price_Display;
            item.Qty_Edit = item.Qty;
            item.SettlType_Edit = item.SettlType;
            item.WholeFlag_Edit = item.WholeFlag;
            //string tradeDate = CommonUtils.CalTradeDate(item.SettlType);
            //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
            //DateTime settlDateTime;
            //int selectIndex;
            //CommonUtils.SetSettleType(item.SettlType, tradeDate, out settlDateTime, out selectIndex);
            //item.edit_settletype = selectIndex;

            item.CustomerPrice_Edit = item.CustomerPrice;
            item.BP_Edit = item.BP;
            item.HurryFlag_Edit = item.HurryFlag;
            item.SuggestFlag_Edit = item.SuggestFlag;
            item.FireFlag_Edit = item.FireFlag;
            item.FloorPrice_Edit = item.FloorPrice;
            item.OCOFlag_Edit = item.OCOFlag;
            item.PublicRemarks_Edit = item.PublicRemarks;
            item.PersonalRemarks_Edit = item.PersonalRemarks;
            item.PriceType_Edit= item.PriceType;
        }

        private static string COLUMNNAME_INSTITUTION = "来源方机构";
        private static string COLUMNNAME_TRADER = "来源方";
        private static string COLUMNNAME_SIDE = "方向";
        private static string COLUMNNAME_PRICE = "定价";
        private static string COLUMNNAME_QTY = "报量";
        private static string COLUMNNAME_SETTLETYPE = "清算速度";
        private static string COLUMNNAME_BROKER = "Trader";

        private static string COLUMNNAME_CUSTOMERPRICE = "来源方报价";
        private static string COLUMNNAME_BP = "点差";
        private static string COLUMNNAME_TAG = "标签";
        private static string COLUMNNAME_MARK = "公告";
        private static string COLUMNNAME_MEMO = "个人笔记";

        /// <summary>
        /// 保存记录修改
        /// </summary>
        /// <param name="saveBtn"></param>
        private void SaveRow(Button saveBtn)
        {
            if (saveBtn != null && saveBtn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem item = saveBtn.DataContext as IntentionQuoteItem;
                SaveRow(item);
            }
        }

        internal async void SaveRow(IntentionQuoteItem item, string columnHeader = null)
        {
            item.IsEdit = "2";
            // 价格
            decimal? price = null;
            // 报量
            decimal? qty = null;
            // 整量
            string wholeFlag = null;
            // 清算速度
            string settlType = null;
            // 交易员编号
            string traderId = null;
            // 交易员名称
            string traderName = null;
            // 经纪商编号
            string brokerId = null;
            // 经纪商名称
            string brokerName = null;
            // 机构编号
            string institutionId = null;
            // 机构名称
            string institutionName = null;
            // 方向
            string side = null;
            // 客户报价
            decimal? customerPrice = null;
            // 点差
            decimal? bp = null;
            // 急
            string hurryFlag = null;
            // 荐
            string suggestFlag = null;
            // 火
            string fireFlag = null;
            // 底价
            string floorPrice = null;
            // oco, 打包
            string ocoFlag = null;
            // 公开备注
            string publicRemarks = null;
            // 个人备注
            string personalRemarks = null;

            // 客户机构
            if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION || columnHeader == COLUMNNAME_TRADER)
            {
                if (item.SelectedInstintuion_Edit != null)
                {
                    institutionName = item.SelectedInstintuion_Edit.shortnamecn;
                    institutionId = item.SelectedInstintuion_Edit.idbkey;
                }
                else
                {
                    institutionId = item.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                    institutionName = item.InstitutionName_Edit;
                }


                //// 联动检查客户名
                //if (!string.IsNullOrEmpty(institutionId) 
                //        && !string.IsNullOrEmpty(item.TraderId))
                //{
                //    List<CrmContact> CrmContactList = CommonUtils.GetContactidInfoPo(item.InstitutionId);
                //    if (!CrmContactList.Any(m=>m.ContactId == item.TraderId))
                //    {
                //        traderName = string.Empty;
                //        traderId = string.Empty;
                //    }
                //}
                //}

                //// 客户名
                //if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
                //{
                if (item.SelectedTrader_Edit != null)
                {
                    traderName = item.SelectedTrader_Edit.namecn;
                    traderId = item.SelectedTrader_Edit.idbkey;
                    //item.QqNumber = item.SelectedTrader_Edit.QQ;
                }
                else
                {
                    traderId = item.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                    traderName = item.TraderName_Edit;
                }
                //if(traderId == item.TraderId && traderName == item.TraderName)
                //{
                //    return;
                //}

                price = item.Price;
                qty = item.Qty;

                if (institutionId == item.InstitutionId && institutionName == item.InstitutionName
                    && traderId == item.TraderId && traderName == item.TraderName)
                {
                    return;
                }
                else
                {
                    RemenberInstAndTrade(item);
                }
            }

            // 方向
            if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
            {
                side = item.Side_Edit;

                if (item.BP.HasValue && item.CustomerPrice.HasValue)
                {
                    if (item.Side == "1")
                        price = item.CustomerPrice + item.BP / 100;
                    else
                        price = item.CustomerPrice - item.BP / 100;
                }
                else if (item.CustomerPrice.HasValue && item.Price.HasValue)
                {
                    if (item.Side == "1")
                    {
                        bp = (item.Price - item.CustomerPrice) * 100;
                    }
                    else
                    {
                        bp = (item.CustomerPrice - item.Price) * 100;
                    }
                }
                else
                {
                    price = item.Price;
                }

                qty = item.Qty;

                if (side == item.Side)
                    return;
            }
            // 价格
            if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
            {
                price = item.Price_Edit;
                qty = item.Qty;

                if (price.HasValue && item.CustomerPrice.HasValue)
                {
                    if (item.Side == "1")
                    {
                        bp = (price - item.CustomerPrice) * 100;
                    }
                    else
                    {
                        bp = (item.CustomerPrice - price) * 100;
                    }
                }

                if (price == item.Price)
                    return;
            }
            // 数量
            if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            {
                if (item.Qty == item.Qty_Edit
                    && item.WholeFlag == item.WholeFlag_Edit)
                    return;
                qty = item.Qty_Edit;
                //qty = qty - qty % 100;
                if (qty < 1)
                {
                    qty = 1;
                }
                else if (qty > 100000000)
                {
                    qty = 100000000;
                }

                price = item.Price;
                wholeFlag = item.WholeFlag_Edit;
            }
            // 清算速度
            if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
            {
                //DateTime createtime = CommonUtils.ConvertToDateTime(item.CreateTime);
                //settlType = CommonUtils.IsSpotSettle(createtime, item.edit_settltype_date, item.edit_settletype);
                //if (string.IsNullOrEmpty(settlType))
                //{
                //    settlType = item.edit_settltype_date.ToString("MM.dd") + (item.edit_settletype == 0 ? "+0" : "+1");
                //}
                settlType = item.SettlType_Edit;

                price = item.Price;
                qty = item.Qty;

                if (settlType == item.SettlType)
                    return;
            }

            // trader
            if (columnHeader == null || columnHeader == COLUMNNAME_BROKER)
            {
                if (item.SelectedBroker_Edit != null)
                {
                    brokerName = item.SelectedBroker_Edit.Name;
                    brokerId = item.SelectedBroker_Edit.Username;
                    //item.QqNumber = item.SelectedTrader_Edit.QQ;
                }
                else
                {
                    brokerName = item.BrokerName_Edit == item.BrokerName ? item.BrokerName : string.Empty;
                    brokerId = item.BrokerName_Edit == item.BrokerName ? item.BrokerId : string.Empty;
                }
                //if(traderId == item.TraderId && traderName == item.TraderName)
                //{
                //    return;
                //}

                price = item.Price;
                qty = item.Qty;

                if (brokerId == item.BrokerId && brokerName == item.BrokerName)
                {
                    return;
                }
            }

            // 客户报价
            if (columnHeader == null || columnHeader == COLUMNNAME_CUSTOMERPRICE)
            {
                customerPrice = item.CustomerPrice_Edit;

                price = item.Price;
                qty = item.Qty;

                if (item.BP.HasValue && customerPrice.HasValue)
                {
                    if (item.Side == "1")
                        price = customerPrice + item.BP / 100;
                    else
                        price = customerPrice - item.BP / 100;
                }

                if (customerPrice == item.CustomerPrice)
                    return;
            }

            // 价差
            if (columnHeader == null || columnHeader == COLUMNNAME_BP)
            {
                bp = item.BP_Edit;

                price = item.Price;
                qty = item.Qty;

                if (item.CustomerPrice.HasValue && bp.HasValue)
                {
                    if (item.Side == "1")
                        price = item.CustomerPrice + bp / 100;
                    else
                        price = item.CustomerPrice - bp / 100;
                }

                if (bp == item.BP)
                    return;
            }

            // 标签
            if (columnHeader == null || columnHeader == COLUMNNAME_TAG)
            {
                hurryFlag = item.HurryFlag_Edit;
                suggestFlag = item.SuggestFlag_Edit;
                fireFlag = item.FireFlag_Edit;
                floorPrice = item.FloorPrice_Edit;
                ocoFlag = item.OCOFlag_Edit;

                price = item.Price;
                qty = item.Qty;

                if (hurryFlag == item.HurryFlag
                    && suggestFlag == item.SuggestFlag
                    && fireFlag == item.FireFlag
                    && floorPrice == item.FloorPrice
                    && ocoFlag == item.OCOFlag)
                {
                    return;
                }
            }

            // 备注
            if (columnHeader == null || columnHeader == COLUMNNAME_MARK)
            {
                publicRemarks = item.PublicRemarks_Edit;

                price = item.Price;
                qty = item.Qty;

                if (publicRemarks == item.PublicRemarks)
                    return;
            }

            // Memo
            if (columnHeader == null || columnHeader == COLUMNNAME_MEMO)
            {
                personalRemarks = item.PersonalRemarks_Edit;

                price = item.Price;
                qty = item.Qty;

                if (personalRemarks == item.PersonalRemarks)
                    return;
            }

            if (!string.IsNullOrEmpty(item.Status) && item.Status != "0")
            {
                if (!string.IsNullOrEmpty(institutionId))
                {
                    Common.Utils.CommonUtils.AddContract(item.TraderId);
                    Common.Utils.CommonUtils.SaveContract();
                }
                if (!string.IsNullOrEmpty(traderId))
                {
                    Common.Utils.CommonUtils.AddInst(item.InstitutionId);
                    Common.Utils.CommonUtils.SaveInst();
                }
            }


            // 新更新
            STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest();
            request.intentionId = item.IntentionId;
            request.institutionId = institutionId;
            request.institutionName = institutionName;
            request.traderId = traderId;
            request.traderName = traderName;
            request.side = side;
            request.price = price;
            request.qty = qty == 0 ? null : qty;
            request.settlType = settlType;
            request.wholeFlag = wholeFlag;
            //request.brokerId = brokerId;
            //request.brokerName = brokerName;
            request.name = brokerName;
            request.userName = brokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;

            request.customerPrice = customerPrice;
            request.customerSpread = bp;
            request.hurryFlag = hurryFlag;
            request.suggestFlag = suggestFlag;
            request.fireFlag = fireFlag;
            request.floorPrice = floorPrice;
            request.OcoFlag = ocoFlag;
            request.publicRemarks = publicRemarks;
            request.personalRemarks = personalRemarks;
            Common.ViewModel.Order.InnerPanel.TagData info2tag = new Common.ViewModel.Order.InnerPanel.TagData()
            {
                HURRY_FLAG = item.HurryFlag_Edit,
                SUGGEST_FLAG = item.SuggestFlag_Edit,
                FIRE_FLAG = item.FireFlag_Edit,
                FLOOR_PRICE = item.FloorPrice_Edit,
                CUSTOMER_PRICE = item.CustomerPrice_Edit.HasValue ? item.CustomerPrice_Edit.ToString() : string.Empty,
                CUSTOMER_SPREAD = item.BP_Edit.HasValue ? item.BP_Edit.ToString() : string.Empty,
                PUBLIC_REMARKS = item.PublicRemarks_Edit,
                PERSONAL_REMARKS = item.PersonalRemarks_Edit,
                OCO_FLAG = ocoFlag
            };
            if (item.FQInfo != null)
            {
                info2tag.fromCorpName = item.FQInfo.fromCorpName;
                info2tag.fromName = item.FQInfo.fromName;
                info2tag.fromQQ = item.FQInfo.fromQQ;
                info2tag.source = item.FQInfo.source;
                info2tag.toCorpName = item.FQInfo.toCorpName;
                info2tag.toGroupOpenID = item.FQInfo.toGroupOpenID;
                info2tag.toName = item.FQInfo.toName;
                info2tag.toQQ = item.FQInfo.toQQ;
                info2tag.ADD_WHITE_LIST = item.FQInfo.ADD_WHITE_LIST;
                info2tag.toGroupName = item.FQInfo.toGroupName;
                info2tag.toQQGroup = item.FQInfo.toQQGroup;
            }
            request.VipCode = info2tag.VIP;
            request.OrgType = info2tag.ORG_TYPE;
            request.IfSubBond = info2tag.IF_SUB_BOND;
            request.info2 = Newtonsoft.Json.JsonConvert.SerializeObject(info2tag);
            bool isSuccess = await IntentionOldHandler.Instance.UpdateIntentionQuote(request);
            //if (isSuccess)
            //{
            //    // 客户机构
            //    if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            //    {
            //        item.InstitutionName = institutionName;
            //        item.InstitutionId = institutionId;
            //    }
            //    // 客户名
            //    if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            //    {
            //        item.TraderName = traderName;
            //        item.TraderId = traderId;
            //    }
            //    // 方向
            //    if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
            //        item.Side = side;
            //    // 价格
            //    if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
            //        item.Price = price;
            //    // 数量
            //    if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            //        item.Qty = qty;
            //    // 清算速度
            //    if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
            //        item.SettlType = settlType;

            //    if (string.IsNullOrEmpty(item.OrderID) == false)
            //    {
            //        // 更新我的订单/我的撤单
            //        MyOrderProcesser.Processer.UpdateCustomInfo(item.IntentionId,
            //           item.InstitutionId, item.InstitutionName,
            //           item.TraderId, item.TraderName
            //           );
            //        // 更新我的成交
            //        MyExecutionProcesser.Processer.UpdateCustomInfo(item.IntentionId,
            //            item.InstitutionId, item.InstitutionName,
            //            item.TraderId, item.TraderName
            //            );
            //    }
            //}
        }

        /// <summary>
        /// 取消编辑修改
        /// </summary>
        /// <param name="cancelBtn"></param>
        private void CancelRow(Button cancelBtn)
        {
            if (cancelBtn != null && cancelBtn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem item = cancelBtn.DataContext as IntentionQuoteItem;
                item.IsEdit = "2";
            }
        }

        private void CheckandTips(string type, IntentionQuoteItem currentItem)
        {
            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic_local = new Dictionary<string, List<TPlaceOrderResult>>();
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic_local = new Dictionary<string, List<TPlaceOrderResult>>();
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic_local = new Dictionary<string, List<TPlaceOrderResult>>();
            BaseReplyPo replyPo = CheckParam(currentItem);
            if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
            {


                if (replyPo != null)
                {
                    if (replyPo.Code == "10001")
                    {
                        exceptionReplyDic_local[currentItem.IntentionId] = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);

                        orderDic[currentItem.IntentionId] = currentItem;
                        BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                               warnReplyDic_local, prohibitReplyDic_local, 1, exceptionReplyDic_local);
                        batchResultWindow.Owner = Application.Current.MainWindow;
                        batchResultWindow.ShowDialog();
                        return;

                    }
                }

            }
            else
            {

                switch (type)
                {
                    case "1"://NF发布确认
                        DoConfirmOne(currentItem);
                        break;
                    case "2"://firm发布确认
                        DoQuickConfirmOne(currentItem);
                        break;
                    case "3"://t/g确认
                        DoTGOne(currentItem);
                        break;
                    case "4":
                        break;
                    case "5":
                        break;
                }

            }
        }

        #region 更新MD
        protected ConcurrentDictionary<string, CCDCValuaModel> _symbolCountDic = new ConcurrentDictionary<string, CCDCValuaModel>();

        protected void UpdateCtPrice(MarketDataSnapshotFullRefresh price)
        {
            GetBestPrice(price);
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    //CCDCValuaModel srcccdc = null;
                    //if (_symbolCountDic.ContainsKey(price.SecurityID)
                    //    && (_symbolCountDic[price.SecurityID] == null
                    //        || _symbolCountDic[price.SecurityID].CCDCValuation != price.CCDCValuation))
                    //{
                    //    try
                    //    {
                    //        srcccdc = Common.Utils.CommonUtils.GetCCDCValua(price.SecurityID);
                    //        if (srcccdc == null)
                    //        {
                    //            srcccdc = Common.Utils.CommonUtils.GenerateCCDCValue(price);

                    //            _symbolCountDic[price.SecurityID] = srcccdc;

                    //            Common.Utils.CommonUtils.SetCCDCValua(price.SecurityID, srcccdc);
                    //        }
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        LogHandler.Error(e);
                    //    }
                    //}

                    IEnumerable<IntentionQuoteItem> findDatas = IntentionQuoteInfo.Where(item => item.SecurityId == price.SecurityID);
                    foreach (IntentionQuoteItem findData in findDatas)
                    {
                        findData.Best_Bid = _bestPriceDic[price.SecurityID][0];
                        findData.Best_Ofr = _bestPriceDic[price.SecurityID][1];
                        //RefreshCCDCValue(findData, price, srcccdc);
                    }

                    // DataRow[] findRows = DemandDT.Select($"{SECURITYID_FIELD} = '{price.SecurityID}'");
                    //foreach (DataRowView drv in DemandDv_Filter)
                    //{
                    //    (drv.Row[ITEM_FIELD] as IntentionQuoteItem).Best_Bid = _bestPriceDic[price.SecurityID][0];
                    //    (drv.Row[ITEM_FIELD] as IntentionQuoteItem).Best_Ofr = _bestPriceDic[price.SecurityID][1];
                    //    //RefreshCCDCValue(findData, price, srcccdc);
                    //}
                    if (_demand_Dic.ContainsKey(price.SecurityID))
                    {
                        List<DataRow> list = _demand_Dic[price.SecurityID];
                        if (list != null && list.Count > 0)
                        {
                            foreach (DataRow dr in list)
                            {
                                (dr[ITEM_FIELD] as IntentionQuoteItem).Best_Bid = _bestPriceDic[price.SecurityID][0];
                                (dr[ITEM_FIELD] as IntentionQuoteItem).Best_Ofr = _bestPriceDic[price.SecurityID][1];
                            }
                        }
                    }
                }));
            }
        }

        private void RefreshCCDCValue(IntentionQuoteItem findData, MarketDataSnapshotFullRefresh price, CCDCValuaModel srcccdc)
        {
            if (!string.IsNullOrEmpty(price.CCDCValuation) && srcccdc != null)
            {
                findData.CCDCValuation = srcccdc.CCDCValuation.ConvertToDecimal();
                findData.CCDCValuation_Date = srcccdc.CCDCValuaDate;
                findData.ShowLastWorkDay = srcccdc.ShowLastWorkDay;
                if (findData.CCDCValuation.HasValue && findData.Price.HasValue)
                {
                    if (findData.Side == "1")
                        findData.CCDCValuation_BP = (findData.Price - findData.CCDCValuation) * 100;
                    else if (findData.Side == "2")
                        findData.CCDCValuation_BP = (findData.CCDCValuation - findData.Price) * 100;
                }
            }
        }


        /// <summary>
        /// key:securityid; value:bestPrices 0:bestBid 1:bestOfr
        /// </summary>
       protected ConcurrentDictionary<string, decimal?[]> _bestPriceDic = new ConcurrentDictionary<string, decimal?[]>();

        private void GetBestPrice(MarketDataSnapshotFullRefresh price)
        {
            if (!_bestPriceDic.ContainsKey(price.SecurityID))
            {
                _bestPriceDic[price.SecurityID] = new decimal?[2];
            }
            else
            {
                _bestPriceDic[price.SecurityID][0] = null;
                _bestPriceDic[price.SecurityID][1] = null;


            }

            // 剔除
            var bestPrices_best_bid = price.NoMDEntries?.Where(entry => entry.VenueTypeGW != "INNER" && entry.MDEntryType == "0").OrderBy(entry => entry.MDPriceLevel.ConvertToLong());
            if (bestPrices_best_bid != null && bestPrices_best_bid.Count() > 0)
            {
                _bestPriceDic[price.SecurityID][0] = bestPrices_best_bid.First().MDEntryPx.ConvertToDecimal();
            }

            var bestPrices_best_ofr = price.NoMDEntries?.Where(entry => entry.VenueTypeGW != "INNER" && entry.MDEntryType == "1").OrderBy(entry => entry.MDPriceLevel.ConvertToLong());
            if (bestPrices_best_ofr != null && bestPrices_best_ofr.Count() > 0)
            {
                _bestPriceDic[price.SecurityID][1] = bestPrices_best_ofr.First().MDEntryPx.ConvertToDecimal();
            }

            //var bestPrices = price.NoMDEntries?.Where(item => item.MDPriceLevel == "1");

            //if (bestPrices != null)
            //{
            //    foreach (GNoMDEntries entry in bestPrices)
            //    {
            //        if (entry.MDEntryType == "0") // bid
            //        {
            //            _bestPriceDic[price.SecurityID][0] = entry.MDEntryPx.ConvertToDecimal();
            //        }
            //        else if (entry.MDEntryType == "1") // ofr
            //        {
            //            _bestPriceDic[price.SecurityID][1] = entry.MDEntryPx.ConvertToDecimal();
            //        }
            //    }
            //}
        }
        #endregion


        private void RemenberInstAndTrade(IntentionQuoteItem item)
        {
            if (!string.IsNullOrEmpty(item.TraderId))
            {
                Common.Utils.CommonUtils.AddContract(item.TraderId);
                Common.Utils.CommonUtils.SaveContract();
            }
            if (!string.IsNullOrEmpty(item.InstitutionId))
            {
                Common.Utils.CommonUtils.AddInst(item.InstitutionId);
                Common.Utils.CommonUtils.SaveInst();
            }
        }

        internal void EditOrder(bool isUpdate = true)
        {
            if (this.SelectedItem_Working == null)
            {
                return;
            }

            string intentionID = this.SelectedItem_Working.IntentionId;
            string securityID = this.SelectedItem_Working.SecurityId;

            if (WindowManager.Instance.IsExist($"EditOrder-{intentionID}"))
            {
                return;
            }
            PriceMatchParam priceMatchParam = new PriceMatchParam();

            priceMatchParam.SendOrderType = "OCO";//??

            priceMatchParam.CustomFirmAmount = this.SelectedItem_Working.Qty?.ToString(); // OrderQty 订单量
            if (isUpdate)
            {
                priceMatchParam.OrderId = this.SelectedItem_Working.OrderID;// 订单号
            }
            priceMatchParam.RandomNum = string.Empty; // 渠道类型默认，填写随机几家渠道 ??
            priceMatchParam.Side = this.SelectedItem_Working.Side; // 方向
            priceMatchParam.Symbol = this.SelectedItem_Working.SecurityId; // 代码
            priceMatchParam.SymbolName = this.SelectedItem_Working.Symbol; // 名称
            priceMatchParam.WholeFlag = this.SelectedItem_Working.WholeFlag; // 整量标志
            
            
            priceMatchParam.PriceType = this.SelectedItem_Working.PriceType;
            priceMatchParam.NetPrice = this.SelectedItem_Working.CleanPrice?.ToString("N4"); // 净价??
            priceMatchParam.Price = this.SelectedItem_Working.Price?.ToString("N4"); // 收益率
            priceMatchParam.SettlType = this.SelectedItem_Working.SettlType; // 清算速度
            priceMatchParam.NonfirmTime = string.Empty; // nonfirmTime;
            priceMatchParam.CrossVenues = string.Empty;// 撮合渠道
            priceMatchParam.VenuesSelectF = string.Empty;//渠道类型  不填写1,默认2，指定3 
            priceMatchParam.SelfSelectVenues = string.Empty;//指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC

            priceMatchParam.InstitutionID = this.SelectedItem_Working.InstitutionId; //客盘机构id
            priceMatchParam.InstitutionName = this.SelectedItem_Working.InstitutionName;//客盘机构名字
            priceMatchParam.TraderID = this.SelectedItem_Working.TraderId; // 客盘交易员id
            priceMatchParam.TraderName = this.SelectedItem_Working.TraderName;//客盘交易员名字
            
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if (isUpdate)
                {
                    priceMatchParam.roletype = this.SelectedItem_Working.RoleType;
                    priceMatchParam.buyerTraderId = this.SelectedItem_Working.Trader_Buyer;
                    priceMatchParam.buyerTraderName = this.SelectedItem_Working.TraderName_Buyer;
                    priceMatchParam.managerId = this.SelectedItem_Working.Investor;
                    priceMatchParam.managerName = this.SelectedItem_Working.InvestorName;
                    priceMatchParam.MatchedQty = this.SelectedItem_Working.MatchedQty;
                }
                else
                {
                    if (STC.Common.Utils.CommonUtils.IsInvestor())
                    {
                        priceMatchParam.roletype = this.SelectedItem_Working.RoleType;
                        //priceMatchParam.buyerTraderId = this.SelectedItem_Working.Trader_Buyer;
                        //priceMatchParam.buyerTraderName = this.SelectedItem_Working.TraderName_Buyer;
                        priceMatchParam.managerId = LocalEnv.UserInfo.UserName;
                        priceMatchParam.managerName = LocalEnv.UserInfo.FullName;
                        //priceMatchParam.MatchedQty = this.SelectedItem_Working.MatchedQty;
                    }
                    else
                    {
                        priceMatchParam.buyerTraderId = LocalEnv.UserInfo.UserName;
                        priceMatchParam.buyerTraderName = LocalEnv.UserInfo.FullName;
                        string type = STC.Common.Utils.CommonUtils.ConvertToPAType(this.SelectedItem_Working.SecurityId);
                        List<string> managerList = AdminHandler.Instance.GetManagerList(LocalEnv.UserInfo.UserId, type);
                        List<StcTbookUsers> investorsList = Common.Utils.CommonUtils.GetInvestors();
                        var invalidManagerList = investorsList.Where(x => managerList.Contains(x.Userid));
                        foreach(StcTbookUsers stu in invalidManagerList)
                        {
                            if (stu.Username == this.SelectedItem_Working.Investor)
                            {
                                priceMatchParam.managerId = this.SelectedItem_Working.Investor;
                                priceMatchParam.managerName = this.SelectedItem_Working.InvestorName;
                                break;
                            }
                        }

                        //List<StcTbookUsers> investorsList = PTExecOrderProcesser.Instance.InvestorsList;
                        //List<string> managerList = AdminHandler.Instance.GetManagerList(LocalEnv.UserInfo.UserId);
                        //if (managerList != null)
                        //{
                        //    List<StcTbookUsers> list = investorsList.Where(d => managerList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                        //}
                    }
                }
            }



            priceMatchParam.Terminal = string.Empty; //??
            priceMatchParam.Info1 = this.SelectedItem_Working.Info1;
            priceMatchParam.UserName = this.SelectedItem_Working.BrokerId;
            priceMatchParam.Name = this.SelectedItem_Working.BrokerName;
            
            priceMatchParam.Info3 = string.Empty;//用于IDB单子??
            if (isUpdate)
            {
                priceMatchParam.Info4 = intentionID;//用于意向单的Intentionid
            }
            priceMatchParam.AccountID = this.SelectedItem_Working.AccountID;
            //priceMatchParam.Text = this.SelectedItem_Working.Text;
            priceMatchParam.PriceCondition = string.Empty;//??
            //priceMatchParam.QuoteType = singleOrder.QuoteType;
            priceMatchParam.IsFirm = false;// nonfirm

            priceMatchParam.IsIntention = true;
            priceMatchParam.status = this.SelectedItem_Working.Status;
            if (isUpdate)
            {
                priceMatchParam.IntentionID = intentionID;
            }
            priceMatchParam.channel = this.SelectedItem_Working.Channel;
            priceMatchParam.QuoteType = this.SelectedItem_Working.QuoteType;

            Common.ViewModel.Order.InnerPanel.TagData info2tag = null;
            if (priceMatchParam != null && JsonSplit.IsJson(this.SelectedItem_Working.Info2))
            {
                info2tag = JsonConvert.DeserializeObject<Common.ViewModel.Order.InnerPanel.TagData>(this.SelectedItem_Working.Info2);
            }
            else
            {
                info2tag = new Common.ViewModel.Order.InnerPanel.TagData();
            }

            info2tag.HURRY_FLAG = this.SelectedItem_Working.HurryFlag;
            info2tag.SUGGEST_FLAG = this.SelectedItem_Working.SuggestFlag;
            info2tag.FIRE_FLAG = this.SelectedItem_Working.FireFlag;
            info2tag.FLOOR_PRICE = this.SelectedItem_Working.FloorPrice;
            info2tag.CUSTOMER_PRICE = this.SelectedItem_Working.CustomerPrice.HasValue ? this.SelectedItem_Working.CustomerPrice.ToString() : string.Empty;
            info2tag.CUSTOMER_SPREAD = this.SelectedItem_Working.BP.HasValue ? this.SelectedItem_Working.BP.ToString() : string.Empty;
            info2tag.PUBLIC_REMARKS = this.SelectedItem_Working.PublicRemarks;
            info2tag.PERSONAL_REMARKS = this.SelectedItem_Working.PersonalRemarks;
            info2tag.OCO_FLAG = this.SelectedItem_Working.OCOFlag;
            if (this.SelectedItem_Working.FQInfo != null)
            {
                info2tag.fromCorpName = this.SelectedItem_Working.FQInfo.fromCorpName;
                info2tag.fromName = this.SelectedItem_Working.FQInfo.fromName;
                info2tag.fromQQ = this.SelectedItem_Working.FQInfo.fromQQ;
                info2tag.source = this.SelectedItem_Working.FQInfo.source;
                info2tag.toCorpName = this.SelectedItem_Working.FQInfo.toCorpName;
                info2tag.toGroupOpenID = this.SelectedItem_Working.FQInfo.toGroupOpenID;
                info2tag.toName = this.SelectedItem_Working.FQInfo.toName;
                info2tag.toQQ = this.SelectedItem_Working.FQInfo.toQQ;
                info2tag.ADD_WHITE_LIST = this.SelectedItem_Working.FQInfo.ADD_WHITE_LIST;
                info2tag.toGroupName = this.SelectedItem_Working.FQInfo.toGroupName;
                info2tag.toQQGroup = this.SelectedItem_Working.FQInfo.toQQGroup;
            }
            priceMatchParam.Info2 = Newtonsoft.Json.JsonConvert.SerializeObject(info2tag);

            priceMatchParam.Info5 = this.SelectedItem_Working.Info5;

            priceMatchParam.AccountID = string.IsNullOrWhiteSpace(this.SelectedItem_Working.AccountID) ? this.SelectedItem_Working.BookId : this.SelectedItem_Working.AccountID;
            priceMatchParam.relatedUsers = this.SelectedItem_Working.RelatedUsers;
            SendRequireView sendRequireView = new SendRequireView(priceMatchParam);
            WindowManager.Instance.OpenOnlyOneWindow($"EditOrder-{intentionID}", sendRequireView);
        }

        #region 订阅标签
        private object _dataLock_tag = new object();
        private CollectionViewSource _viewSource_tag = new CollectionViewSource();
        public CollectionViewSource ViewSource_Tag
        {
            get { return _viewSource_tag; }
            set { _viewSource_tag = value; }
        }

        private List<string> SelectedGroupBondsItems = new List<string>();
        private object _GroupBondsLock = new object();

        private FilterGroupItem _selectedItem_tag;

        public FilterGroupItem SelectedItem_Tag
        {
            get { return _selectedItem_tag; }
            set
            {
                //if (_selectedItem_tag != value)
                {
                    _selectedItem_tag = value;

                    if (value != null)
                    {
                        if (value.Inf1 != "GROUP")
                        {
                            // 筛选处理
                            this.StartTermDY = _selectedItem_tag.StartTermDY;
                            this.EndTermDY = _selectedItem_tag.EndTermDY;
                            if (!string.IsNullOrEmpty(_selectedItem_tag.StartTermDY) || !string.IsNullOrEmpty(_selectedItem_tag.EndTermDY))
                            {
                                this.IsAppTerm = _selectedItem_tag.IsAppTerm = true;
                            }
                            else
                            {
                                this.IsAppTerm = _selectedItem_tag.IsAppTerm = false;
                            }

                            this.FocusTags = _selectedItem_tag.FocusTags;

                            this.StartCoupon = _selectedItem_tag.StartCoupon;
                            this.EndCoupon = _selectedItem_tag.EndCoupon;
                            this.IsCoupon = _selectedItem_tag.IsCoupon;

                            this.StartPrice = _selectedItem_tag.StartPrice;
                            this.EndPrice = _selectedItem_tag.EndPrice;

                            this.StartChinaBondValue = _selectedItem_tag.StartChinaBond;
                            this.EndChinaBondValue = _selectedItem_tag.EndChinaBond;

                            this.StartBidBp = _selectedItem_tag.StartBidBp;
                            this.EndBidBp = _selectedItem_tag.EndBidBp;

                            this.StartOfrBp = _selectedItem_tag.StartOfrBp;
                            this.EndOfrBp = _selectedItem_tag.EndOfrBp;
                           
                        }
                        else
                        {
                            // 筛选处理
                            this.StartTermDY = string.Empty;
                            this.EndTermDY = string.Empty;
                            this.IsAppTerm = false;

                            this.StartCoupon = string.Empty;
                            this.EndCoupon = string.Empty;
                            this.IsCoupon = false;

                            this.FocusTags = string.Empty;

                            this.StartPrice = string.Empty;
                            this.EndPrice = string.Empty;
                            this.IsPrice = false;

                            this.StartChinaBondValue = string.Empty;
                            this.EndChinaBondValue = string.Empty;

                            this.StartBidBp = string.Empty;
                            this.EndBidBp = string.Empty;

                            this.StartOfrBp = string.Empty;
                            this.EndOfrBp = string.Empty;
                        }

                        ProcessGroupBonds(value.Inf1, value.FocusGroupId);
                    }
                    else
                    {
                        // 筛选处理
                        this.StartTermDY = string.Empty;
                        this.EndTermDY = string.Empty;
                        this.IsAppTerm = false;

                        this.StartCoupon = string.Empty;
                        this.EndCoupon = string.Empty;
                        this.IsCoupon = false;

                        this.FocusTags = string.Empty;

                        this.StartPrice = string.Empty;
                        this.EndPrice = string.Empty;
                        this.IsPrice = false;

                        this.StartChinaBondValue = string.Empty;
                        this.EndChinaBondValue = string.Empty;

                        this.StartBidBp = string.Empty;
                        this.EndBidBp = string.Empty;

                        this.StartOfrBp = string.Empty;
                        this.EndOfrBp = string.Empty;

                        //this.FocusTags = InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS;
                        this.DesSource.Clear();

                        // 在ListBoxItem_PreviewMouseLeftButtonDown里处理
                        ProcessGroupBonds(null, null);
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 券池处理
        /// </summary>
        /// <param name="inf1"></param>
        /// <param name="focusGroupId"></param>
        internal void ProcessGroupBonds(string inf1, string focusGroupId)
        {
            if (inf1 == "GROUP")
            {
                // 券池处理
                string[] groupidArr = focusGroupId.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                List<string> bonds = BondGroupHandler.Instance.GetSecuritys(new List<string>(groupidArr));
                lock (_GroupBondsLock)
                {
                    SelectedGroupBondsItems.Clear();
                    if (bonds != null)
                    {
                        SelectedGroupBondsItems.AddRange(bonds);
                    }
                }
                NotifyFilter();
                SetGroupBondDes();
            }
            else
            {
                if (SelectedGroupBondsItems == null || SelectedGroupBondsItems.Count > 0)
                {
                    lock (_GroupBondsLock)
                    {
                        SelectedGroupBondsItems.Clear();
                    }
                    NotifyFilter();
                    SetGroupBondDes();
                }
                
            }
        }

        private void SetGroupBondDes()
        {
            StringBuilder sb = new StringBuilder();
            //if (!string.IsNullOrEmpty(this.FilterText))
            //{
            //    sb.Append(this.FilterText);
            //}
            if (this.SelectedGroupBondsItems != null && this.SelectedGroupBondsItems.Count > 0)
            {
                foreach (string securityId in SelectedGroupBondsItems)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("/");
                    }
                    sb.Append(securityId);
                }
            }
            string text = string.Empty;
            if (sb.Length > 0)
            {
                text = $"券池:{sb}";
            }
            SetDesSource(this.DesSource, "券池", text);
        }

        internal async void UpdateGroupOne(FilterGroupItem updateItem, bool ifSubscribe)
        {
            // 更新
            QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
            //req.biztypeid = LocalEnv.UserInfo.biztypeid;
            req.action = "2";
            req.id = updateItem.GroupID;
            req.username = req.closeby = LocalEnv.UserInfo.UserName;
            req.groupname = updateItem.GroupName;
            req.ifSubscribe = ifSubscribe ? "1" : "0";
            req.focusType = updateItem.Inf1;
            req.marketindicator = "4";
            //try
            //{
            //    if (!string.IsNullOrEmpty(updateItem.FocusTags))
            //    {
            //        Dictionary<string, List<string>> dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(updateItem.FocusTags);
            //        if (dic.ContainsKey(Utils.Constants.BONDTYPE))
            //            req.bondTypes = dic[Utils.Constants.BONDTYPE];
            //        if (dic.ContainsKey(Utils.Constants.TERM))
            //            req.terms = dic[Utils.Constants.TERM];
            //        if (dic.ContainsKey(Utils.Constants.FIRM))
            //            req.firms = dic[Utils.Constants.FIRM];
            //        if (dic.ContainsKey(Utils.Constants.SETTLTYPE))
            //            req.settlTypes = dic[Utils.Constants.SETTLTYPE];
            //        if (dic.ContainsKey(Utils.Constants.SIDE))
            //            req.sides = dic[Utils.Constants.SIDE];
            //        if (dic.ContainsKey(Utils.Constants.VIP))
            //            req.ifVip = dic[Utils.Constants.VIP];
            //        else
            //        {
            //            req.ifVip = new List<string>();
            //            req.ifVip.Add("0010");
            //        }
            //        if (dic.ContainsKey(Utils.Constants.ROLETYPE))
            //            req.roleTypes = dic[Utils.Constants.ROLETYPE];
            //        if (dic.ContainsKey(Utils.Constants.ORGTYPE))
            //            req.orgTypes = dic[Utils.Constants.ORGTYPE];
            //        if (dic.ContainsKey(Utils.Constants.FINBONDTYPE))
            //            req.finBondTypes = dic[Utils.Constants.FINBONDTYPE];
            //        if (dic.ContainsKey(Utils.Constants.DQTYPE))
            //            req.dqTypes = dic[Utils.Constants.DQTYPE];


            //    }

            //}
            //catch (Exception e)
            //{
            //    LogHelper.Error(e);
            //}
            bool isDYNAMIC_PARAM = false;
            if ((!string.IsNullOrEmpty(updateItem.StartTermDY_Edit) || !string.IsNullOrEmpty(updateItem.EndTermDY_Edit)))
            {
                req.termRangeStr = $"{updateItem.StartTermDY_Edit},{updateItem.EndTermDY_Edit}";

                string startT = string.Empty;
                if (!string.IsNullOrEmpty(updateItem.StartTermDY_Edit))
                {
                    startT = STC.Common.Utils.CommonUtils.GetDays(updateItem.StartTermDY_Edit).ToString();
                }

                string endT = string.Empty;
                if (!string.IsNullOrEmpty(updateItem.EndTermDY_Edit))
                {
                    endT = STC.Common.Utils.CommonUtils.GetDays(updateItem.EndTermDY_Edit).ToString();
                }
                req.termDays = $"{startT},{endT}";
                isDYNAMIC_PARAM = true;
            }

            if (updateItem.IsCoupon_Edit && (!string.IsNullOrEmpty(updateItem.StartCoupon_Edit) || !string.IsNullOrEmpty(updateItem.EndCoupon_Edit)))
            {
                req.couponRangeStr = $"{updateItem.StartCoupon_Edit},{updateItem.EndCoupon_Edit}";
                isDYNAMIC_PARAM = true;
            }

            if (!string.IsNullOrEmpty(updateItem.StartPrice_Edit) || !string.IsNullOrEmpty(updateItem.EndPrice_Edit))
            {
                req.priceRangeStr = $"{updateItem.StartPrice_Edit},{updateItem.EndPrice_Edit}";
                isDYNAMIC_PARAM = true;
            }

            if (!string.IsNullOrEmpty(updateItem.StartChinaBond_Edit) || !string.IsNullOrEmpty(updateItem.EndChinaBond_Edit))
            {
                req.valuationRangeStr = $"{updateItem.StartChinaBond_Edit},{updateItem.EndChinaBond_Edit}";
                isDYNAMIC_PARAM = true;
            }

            if (!string.IsNullOrEmpty(updateItem.StartBidBp_Edit) || !string.IsNullOrEmpty(updateItem.EndBidBp_Edit))
            {
                req.bidSubValRangeStr = $"{updateItem.StartBidBp_Edit},{updateItem.EndBidBp_Edit}";
                isDYNAMIC_PARAM = true;
            }

            if (!string.IsNullOrEmpty(updateItem.StartOfrBp_Edit) || !string.IsNullOrEmpty(updateItem.EndOfrBp_Edit))
            {
                req.valSubOfrRangeStr = $"{updateItem.StartOfrBp_Edit},{updateItem.EndOfrBp_Edit}";
                isDYNAMIC_PARAM = true;
            }

            req.focusTags = updateItem.FocusTags;
            //req.ordersn = item.Ordersn;
            req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            req.ip = EngineHelper.GetIp();
            //req.remark
            req.createtime = req.updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            QueryQuoteTagGroupReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);
            });

            if (reply != null && reply.IsSuccess())
            {
                try
                {
                    //FilterGroupItem updateItem = this.GroupList.First(obj => obj.GroupID == item.GroupID);
                    updateItem.UserName = reply.Datas[0].username;
                    updateItem.GroupName = updateItem.GroupName_Edit = reply.Datas[0].groupname;
                    updateItem.IfSubscribe = updateItem.IfSubscribe_Edit = reply.Datas[0].ifSubscribe == "1" ? true : false;
                    updateItem.FocusTags = updateItem.FocusTags_Edit = reply.Datas[0].focusTags;
                    updateItem.Inf1 = reply.Datas[0].inf1;
                    updateItem.Inf2 = reply.Datas[0].inf2;
                    updateItem.Inf3 = reply.Datas[0].inf3;
                    updateItem.UpdateTime = reply.Datas[0].updatetime;
                    updateItem.CreateTime = reply.Datas[0].createtime;
                    updateItem.Ordersn = reply.Datas[0].ordersn;
                    updateItem.IP = reply.Datas[0].ip;
                    updateItem.Terminal = reply.Datas[0].terminal;
                    updateItem.Remark = reply.Datas[0].remark;
                    updateItem.CloseBy = reply.Datas[0].closeby;

                    Common.Utils.CommonUtils.Prompt2Second("保存", "成功");
                }
                catch (System.InvalidOperationException)
                {
                    // ignore
                }
            }
            else
            {
                Common.Utils.CommonUtils.Prompt2Second("保存", (reply == null || string.IsNullOrEmpty(reply.Msg)) ? "失败" : reply.Msg);
            }
        }
        #endregion

        private Visibility _isBestOperationGrid = Visibility.Collapsed;

        public Visibility IsBestOperationGrid
        {
            get { return _isBestOperationGrid; }
            set
            {
                _isBestOperationGrid = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// type不是INTENTION_GROUP 且 状态是“处理中” 的数据
        /// </summary>
        internal ConcurrentDictionary<string, ConcurrentDictionary<string, IntentionQuoteItem>> _Cache_Buy_InnerOrders_Dic = new ConcurrentDictionary<string, ConcurrentDictionary<string, IntentionQuoteItem>>();
        /// <summary>
        /// type=INTENTION_GROUP 的数据放入其中， 
        /// </summary>
        internal ConcurrentDictionary<string, IntentionQuoteItem> _Cache_Group_InnerOrders_Dic = new ConcurrentDictionary<string, IntentionQuoteItem>();
        /// <summary>
        /// 主数据
        /// </summary>
        internal ConcurrentDictionary<string, ConcurrentDictionary<string, IntentionQuoteItem>> _Cache_InnerOrders_Dic = new ConcurrentDictionary<string, ConcurrentDictionary<string, IntentionQuoteItem>>();
        internal IntentionQuoteItem SelectedRowBySecurtiyID_Single_Copy(string groupid, string orderid, string securityID)
        {
            try
            {
                if (_Cache_InnerOrders_Dic.ContainsKey(securityID) && _Cache_InnerOrders_Dic[securityID] != null && _Cache_InnerOrders_Dic[securityID].ContainsKey(orderid))
                {
                    IntentionQuoteItem record = _Cache_InnerOrders_Dic[securityID][orderid];
                    if (!string.IsNullOrEmpty(groupid))
                    {
                        if (IntentionOldHandler.Instance.GroupList.Count > 0)
                        {
                            this.SelectedItem_Working = record;

                            FilterGroupItem group = IntentionOldHandler.Instance.GroupList.First(item => item.GroupID == groupid);
                            if (this.SelectedItem_Tag != group)
                            {
                                this.SelectedItem_Tag = group;
                            }
                            else
                            {
                                ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                                this.WorkingViewSource.View?.Refresh();
                                NotifyFilter();
                            }
                        }
                    }
                    else
                    {
                        this.SelectedItem_Working = record;
                        this.SelectedItem_Tag = null;
                    }

                    return record;
                }
            }
            catch (System.InvalidOperationException ex)
            {
                STC.Common.Utils.CommonUtils.Prompt2Second("订阅处理", "无可处理数据");
            }
            return null;
        }

        internal IntentionQuoteItem SelectedRowBySecurtiyID_Best(string securityid, string orderID)
        {
            try
            {
                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    if (_Cache_InnerOrders_Dic.ContainsKey(securityid))
                    {
                        foreach (IntentionQuoteItem record in  _Cache_InnerOrders_Dic[securityid].Values)
                        {
                            if (record.GroupKey == orderID)
                            {
                                this.SelectedItem_Working = record;
                                this.SelectedItem_Tag = null;
                                return record;
                            }
                        }
                    }
                    else

                    { }
                }
                else

                {
                    if (_Cache_InnerOrders_Dic.ContainsKey(securityid) && _Cache_InnerOrders_Dic[securityid].ContainsKey(orderID))
                    {
                        IntentionQuoteItem record = _Cache_InnerOrders_Dic[securityid][orderID];
                        this.SelectedItem_Working = record;
                        this.SelectedItem_Tag = null;
                        return record;
                    }
                    else

                    { }
                }
                

            }
            catch (System.InvalidOperationException ex)
            {
                STC.Common.Utils.CommonUtils.Prompt2Second("订阅处理", "无可处理数据");
            }
            return null;
        }

        internal void SelectedGroup(string groupid)
        {
            try
            {
                if (IntentionOldHandler.Instance.GroupList.Count > 0)
                {
                    this.SelectedItem_Working = null;
                    if (this.WorkingViewSource.View != null)
                    {
                        (this.WorkingViewSource.View as CustomCollectionView<IntentionQuoteItem>).ClearOldSelectedItem();
                    }

                    FilterGroupItem group = IntentionOldHandler.Instance.GroupList.First(item => item.GroupID == groupid);
                    if (this.SelectedItem_Tag != group)
                    {
                        this.SelectedItem_Tag = group;
                    }
                    else
                    {
                        ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
                        this.WorkingViewSource.View?.Refresh();
                        NotifyFilter();
                    }
                }
            }
            catch (System.InvalidOperationException ex)
            {
                STC.Common.Utils.CommonUtils.Prompt2Second("订阅处理", "无可处理Tag组");
            }
        }

        #region 批量修复
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

        private CrmInst selectClientTypeItem;

        public CrmInst SelectClientTypeItem
        {
            get { return selectClientTypeItem; }
            set
            {
                if (selectClientTypeItem != value)
                {
                    selectClientTypeItem = value;
                    if (selectClientTypeItem != null)
                        ClientTypeEnum = selectClientTypeItem.shortnamecn;
                    this.OnPropertyChanged();
                }
            }
        }

        private string clientTypeEnum;
        public string ClientTypeEnum
        {
            get { return clientTypeEnum; }
            set { clientTypeEnum = value; this.OnPropertyChanged(); }
        }

        private bool isSysAuto_Institution = false;

        private CrmInst _selectedInstitution_Edit;

        public CrmInst SelectedInstintuion_Edit
        {
            get { return _selectedInstitution_Edit; }
            set
            {
                if (_selectedInstitution_Edit != value)
                {
                    _selectedInstitution_Edit = value;

                    if (value != null)
                    {
                        List<CrmInst> result = GetCrmInst();
                        if (this._selectedInstitution_Edit != null && !string.IsNullOrEmpty(this._selectedInstitution_Edit.insttypevalue))
                        {
                            result = result.Where(item => item.shortnamecn == this._selectedInstitution_Edit.insttypevalue).ToList();
                            if (result != null && result.Count > 0)
                            {
                                this.SelectClientTypeItem = result.First();
                            }

                        }
                    }


                    this.OnPropertyChanged();
                }
            }
        }

        private string _institutionName_Edit;

        private bool isSysAuto_Trader = false;

        public string InstitutionName_Edit
        {
            get { return _institutionName_Edit; }
            set
            {
                if (!(string.IsNullOrEmpty(_institutionName_Edit) && string.IsNullOrEmpty(value)) && _institutionName_Edit != value)
                {
                    //if (isSysAuto_Institution == false
                    //&& _institutionName_Edit != value)
                    //{
                    //    isSysAuto_Trader = true;
                    //    SelectedTrader_Edit = null;
                    //    TraderName_Edit = string.Empty;
                    //}
                    //isSysAuto_Institution = false;

                    _institutionName_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private CrmContact _selectedTrader_Edit;

        public CrmContact SelectedTrader_Edit
        {
            get { return _selectedTrader_Edit; }
            set
            {

                if (_selectedTrader_Edit != value)
                {
                    _selectedTrader_Edit = value;

                    // 只有crmContact中才有机构信息，可以联动显示
                    if (value != null)
                    {
                        if (SelectedInstintuion_Edit == null || SelectedInstintuion_Edit.idbkey != value.instid)
                        {
                            List<string> instList = new List<string>();
                            instList.Add(value.instid);
                            List<CrmInst> results = WtUtils.GetInstitutionsInfoPo(instList);
                            if (results != null && results.Count > 0)
                            {
                                isSysAuto_Institution = true;
                                isSysAuto_Trader = true;
                                SelectedInstintuion_Edit = results[0];
                                InstitutionName_Edit = results[0].shortnamecn;
                            }
                        }

                        if (SelectedInstintuion_Edit != null && SelectedInstintuion_Edit.idbkey == value.instid)
                        {
                            isSysAuto_Institution = false;
                            isSysAuto_Trader = true;
                            SelectedInstintuion_Edit = SelectedInstintuion_Edit;
                            InstitutionName_Edit = InstitutionName_Edit;
                        }
                    }

                    
                    this.OnPropertyChanged();
                }
            }
        }

        private string _traderName_Edit;

        public string TraderName_Edit
        {
            get { return _traderName_Edit; }
            set
            {
                if (!(string.IsNullOrEmpty(_traderName_Edit) && string.IsNullOrEmpty(value)) && _traderName_Edit != value)
                {
                    //if (isSysAuto_Trader == false
                    //    && _traderName_Edit != value)
                    //{
                    //    isSysAuto_Institution = true;
                    //    SelectedInstintuion_Edit = null;
                    //    InstitutionName_Edit = string.Empty;
                    //}
                    isSysAuto_Trader = false;

                    _traderName_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private bool _isVip = false;

        public bool IsVip
        {
            get
            {
                return _isVip;
            }
            set
            {
                if (_isVip != value)
                {
                    _isVip = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected void Sub_UpdateIntention(Dictionary<string, IntentionQuoteItem> orderDic)
        {
            int totalCount = 0;
            int failCount = 0;
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
            foreach (IntentionQuoteItem item in orderDic.Values)
            {
                if (item.UpdateIntentionRequest != null || item.Request != null)
                {
                    if (item.UpdateIntentionRequest != null)
                    {
                        GetErrorInfo_UpdateIntention(item.UpdateIntentionRequest,
                           ref failCount,
                           ref totalCount,
                           warnReplyDic,
                           prohibitReplyDic,
                           exceptionReplyDic);
                    }
                    else if (item.Request != null)
                    {
                        GetErrorInfo(item.Request,
                           ref failCount,
                           ref totalCount,
                           warnReplyDic,
                           prohibitReplyDic,
                           exceptionReplyDic);
                    }
                }
                else
                {
                    totalCount++;
                }
            }

            while (totalCount != orderDic.Count)
            {
                Thread.Sleep(100);
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (failCount > 0)
                {
                    if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                    {
                        BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                            warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                        batchResultWindow.Owner = Application.Current.MainWindow;
                        batchResultWindow.ShowDialog();
                    }
                }
            }));
        }

        protected void GetErrorInfo_UpdateIntention(STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                PResult replyPo = TableOldHandler.Instance.UpdateIntentionQuote(request);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;
                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10000")
                        {
                            if (JsonSplit.IsJson(replyPo.Msg))
                            {
                                List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                                bool isProhibitory = false;
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
                                    prohibitReplyDic[request.intentionId] = orderRisks;
                                }
                                else
                                {
                                    warnReplyDic[request.intentionId] = orderRisks;
                                }
                            }
                            else
                            {
                                BaseReplyPo reply = new BaseReplyPo();
                                reply.Code = "10000";
                                List<TPlaceOrderResult> resultList = new List<TPlaceOrderResult>();
                                TPlaceOrderResult por = new TPlaceOrderResult();
                                if (STC.Common.Utils.CommonUtils.IsInvestor())
                                {
                                    por.Code = "10001";
                                }
                                else
                                {
                                    if (replyPo.Msg.Contains("强风控"))
                                    {
                                        por.Code = "10002";
                                    }
                                    else if (replyPo.Msg.Contains("弱风控"))
                                    {
                                        por.Code = "10001";
                                    }
                                }
                                por.DoubleWarn = false;
                                por.Reason = replyPo.Msg;
                                resultList.Add(por);
                                reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(resultList);

                                if (por.Code == "10002")
                                {
                                    prohibitReplyDic[request.intentionId] = resultList;
                                }
                                else if (por.Code == "10001")
                                {
                                    warnReplyDic[request.intentionId] = resultList;
                                }
                            }
                        }
                        else
                        {
                            List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                            TPlaceOrderResult result = new TPlaceOrderResult();
                            result.Code = "10003";
                            result.DoubleWarn = false;
                            result.Reason = replyPo.Msg;
                            exceptionResult.Add(result);
                            exceptionReplyDic[request.intentionId] = exceptionResult;
                        }
                    }
                }
                else
                {
                    //replyUpdateThread.Enqueue(replyPo);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }

        protected Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic = new Dictionary<string, Common.Model.Fix.Message.NewOrderSingle>();
        protected Dictionary<string, STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest> newIntentionDic = new Dictionary<string, STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest>();

        internal void EditPrice(string parameter)
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                return;
            }

            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            //newIntentionDic.Clear();

            
            Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in this.SelectedItems_Working)
                {
                    if (drv.Type == "INTENTION_GROUP")
                    {
                        continue;
                    }

                    // 批量修改点差操作，对报价标准为“净价(1)”“全价(2)”的记录不生效
                    if (drv.PriceType == "1" || drv.PriceType == "2")
                    {
                        continue;
                    }

                    if ((parameter == "中债估值" || parameter == "中证估值" || parameter == "市场最优") && (drv.PriceType == "1" || drv.PriceType == "2"))
                    {
                        continue;
                    }

                    if (drv.Status == "1")
                    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);

                        decimal? newPrice = null;
                        if (parameter == "中债估值")
                        {
                            if (drv.ChinaBondExercise != null && drv.ChinaBondExercise.HasValue)
                            {
                                newPrice = drv.ChinaBondExercise;
                            }
                            else
                            {
                                newPrice = drv.ChinaBondDue;
                            }
                        }
                        else if (parameter == "中证估值")
                        {
                            if (drv.CsiBondValuationExercise != null && drv.CsiBondValuationExercise.HasValue)
                            {
                                newPrice = drv.CsiBondValuationExercise;
                            }
                            else
                            {
                                newPrice = drv.CsiBondValuationDue;
                            }
                        }
                        else if(parameter == "市场最优")
                        {
                            if (drv.Side == "1")
                            {
                                newPrice = drv.Best_Bid;
                            }
                            else if (drv.Side == "2")
                            {
                                newPrice = drv.Best_Ofr;
                            }
                        }
                        else
                        {
                            if (drv.PriceType == "1")
                            {
                                if (STC.Common.Utils.CommonUtils.IsNumeric(newOrderSingle.NetPrice))
                                {
                                    newPrice = modifyPrice(newOrderSingle.NetPrice.ConvertToDecimal(), parameter);
                                }
                            }
                            else if (drv.PriceType == "2")
                            {
                                if (STC.Common.Utils.CommonUtils.IsNumeric(newOrderSingle.FullPrice))
                                {
                                    newPrice = modifyPrice(newOrderSingle.FullPrice.ConvertToDecimal(), parameter);
                                }
                            }
                            else
                            {
                                if (STC.Common.Utils.CommonUtils.IsNumeric(newOrderSingle.Price))
                                {
                                    newPrice = modifyPrice(newOrderSingle.Price.ConvertToDecimal(), parameter);
                                }
                            }
                        }
                        if (newPrice != null && newPrice.HasValue)
                        {
                            CalResult calResult = Common.Utils.CommonUtils.CalBond_PriceType(newOrderSingle.SecurityID, newOrderSingle.SettlType, newPrice.ToString(), newOrderSingle.PriceType);
                            if (calResult != null && calResult.Data != null)
                            {
                                if (newOrderSingle.PriceType == "7")
                                {
                                    newOrderSingle.Price = (calResult.Data.YieldToExecution * 100).ToString();
                                }
                                else
                                {
                                    newOrderSingle.Price = (calResult.Data.Yield * 100).ToString();
                                }
                                newOrderSingle.NetPrice = calResult.Data.CleanPrice.ToString();
                                newOrderSingle.FullPrice = calResult.Data.FullPrice.ToString();
                            }
                            if (newOrderSingle.PriceType != "1" && newOrderSingle.PriceType != "2")
                            {
                                TagData info2tag = null;
                                if (newOrderSingle != null && JsonSplit.IsJson(newOrderSingle.Info2))
                                {
                                    info2tag = JsonConvert.DeserializeObject<TagData>(newOrderSingle.Info2);
                                }
                                else
                                {
                                    info2tag = new TagData();
                                }
                                if (!string.IsNullOrEmpty(info2tag.CUSTOMER_PRICE))
                                {
                                    decimal customerPrice = info2tag.CUSTOMER_PRICE.ConvertToDecimal();
                                    if (newOrderSingle.Side == "1")
                                    {
                                        info2tag.CUSTOMER_SPREAD = ((newPrice - customerPrice) * 100).ToString();
                                    }
                                    else
                                    {
                                        info2tag.CUSTOMER_SPREAD = ((customerPrice - newPrice) * 100).ToString();
                                    }
                                }
                                newOrderSingle.Info2 = JsonConvert.SerializeObject(info2tag);
                            }
                            newOrderSingleDic[newOrderSingle.IntentionID] = newOrderSingle;
                        }
                        else
                        {
                            newOrderSingle.Price = null;
                            newOrderSingle.NetPrice = null;
                            newOrderSingle.FullPrice = null;
                        }
                    }
                    else if (drv.Status == "0" || drv.Status == "80" || drv.Status == "81")
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        newIntentionOrder.securityid = drv.SecurityId;
                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            if (STC.Common.Utils.CommonUtils.IsInvestor())
                            {
                                newIntentionOrder.actionFlag = "0";
                            }
                            else
                            {
                                newIntentionOrder.actionFlag = "2";
                            }

                            newIntentionOrder.riskManualCk = "1";
                        }

                        decimal? newPrice = null;
                        if (parameter == "中债估值")
                        {
                            if (drv.ChinaBondExercise != null && drv.ChinaBondExercise.HasValue)
                            {
                                newPrice = drv.ChinaBondExercise;
                            }
                            else
                            {
                                newPrice = drv.ChinaBondDue;
                            }
                        }
                        else if (parameter == "中证估值")
                        {
                            if (drv.CsiBondValuationExercise != null && drv.CsiBondValuationExercise.HasValue)
                            {
                                newPrice = drv.CsiBondValuationExercise;
                            }
                            else
                            {
                                newPrice = drv.CsiBondValuationDue;
                            }
                        }
                        else if (parameter == "市场最优")
                        {
                            if (drv.Side == "1")
                            {
                                newPrice = drv.Best_Bid;
                            }
                            else if (drv.Side == "2")
                            {
                                newPrice = drv.Best_Ofr;
                            }
                        }
                        else
                        {
                            if (newIntentionOrder.priceType == "1")
                            {
                                if (newIntentionOrder.cleanPrice.HasValue)
                                {
                                    newPrice = modifyPrice(newIntentionOrder.cleanPrice.Value, parameter);
                                }
                            }
                            else if (newIntentionOrder.priceType == "2")
                            {
                                if (newIntentionOrder.fullPrice.HasValue)
                                {
                                    newPrice = modifyPrice(newIntentionOrder.fullPrice.Value, parameter);
                                }
                            }
                            else
                            {
                                if (newIntentionOrder.price.HasValue)
                                {
                                    newPrice = modifyPrice(newIntentionOrder.price.Value, parameter);
                                }
                            }
                        }
                        if (newPrice != null && newPrice.HasValue)
                        {
                            if (LocalEnv.ConfInfo.IsPAYL && (parameter == "中债估值" || parameter == "中证估值" || parameter == "市场最优"))
                            {
                                if (newPrice.HasValue)
                                {
                                    if (newIntentionOrder.side == "1")
                                    {
                                        // bid
                                        newIntentionOrder.price = decimal.Ceiling(newPrice.Value * 100) / 100;
                                    }
                                    else
                                    {
                                        // ofr
                                        newIntentionOrder.price = decimal.Floor(newPrice.Value * 100) / 100;
                                    }
                                }
                                else
                                {
                                    newIntentionOrder.price = newPrice;
                                }
                            }
                            else
                            {
                                CalResult calResult = Common.Utils.CommonUtils.CalBond_PriceType(newIntentionOrder.securityid, newIntentionOrder.settlType, newPrice?.ToString(), newIntentionOrder.priceType);
                                if (calResult != null && calResult.Data != null)
                                {
                                    if (newIntentionOrder.priceType == "7")
                                    {
                                        newIntentionOrder.price = calResult.Data.YieldToExecution * 100;
                                    }
                                    else
                                    {
                                        newIntentionOrder.price = calResult.Data.Yield * 100;
                                    }
                                    newIntentionOrder.cleanPrice = calResult.Data.CleanPrice;
                                    newIntentionOrder.fullPrice = calResult.Data.FullPrice;
                                }
                            }

                            if (newIntentionOrder.customerPrice.HasValue)
                            {
                                if (newIntentionOrder.priceType != "1" && newIntentionOrder.priceType != "2")
                                {
                                    TagData info2tag = null;
                                    if (newIntentionOrder != null && JsonSplit.IsJson(newIntentionOrder.info2))
                                    {
                                        info2tag = JsonConvert.DeserializeObject<TagData>(newIntentionOrder.info2);
                                    }
                                    else
                                    {
                                        info2tag = new TagData();
                                    }
                                    if (!string.IsNullOrEmpty(info2tag.CUSTOMER_PRICE))
                                    {
                                        decimal customerPrice = info2tag.CUSTOMER_PRICE.ConvertToDecimal();
                                        if (newIntentionOrder.side == "1")
                                        {
                                            newIntentionOrder.customerSpread = (newIntentionOrder.price - newIntentionOrder.customerPrice) * 100;
                                        }
                                        else
                                        {
                                            newIntentionOrder.customerSpread = (newIntentionOrder.customerPrice - newIntentionOrder.price) * 100;
                                        }
                                        info2tag.CUSTOMER_SPREAD = newIntentionOrder.customerSpread.ToString();
                                    }
                                    newIntentionOrder.info2 = JsonConvert.SerializeObject(info2tag);
                                }
                            }
                            //newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;
                            drv.UpdateIntentionRequest = newIntentionOrder;
                            drv.Request = null;
                            drv.InquiryRequest = null;
                            orderDic[newIntentionOrder.intentionId] = drv;
                        }
                        else
                        {
                            newIntentionOrder.price = null;
                            newIntentionOrder.cleanPrice = null;
                            newIntentionOrder.fullPrice = null;
                        }
                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && orderDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = orderDic.Count + newOrderSingleDic.Count;
                    
                    
                    // 修改意向单
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        Sub_UpdateIntention(orderDic);
                    }
                    else
                    {
                        foreach (IntentionQuoteItem item in orderDic.Values)
                        {
                            PResult result = TableOldHandler.Instance.UpdateIntentionQuote(item.UpdateIntentionRequest);
                            if (result == null || !result.IsSuccess())
                            {
                                failCount++;
                            }
                        }
                    }


                    // 修改订单
                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        sendPricePo.HandlInst = "2";
                        RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    string text = string.Empty;
                    if (failCount > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑报价", text);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
            });
        }

        private decimal modifyPrice(decimal price, string parameter)
        {
            if (parameter == "+5")
            {
                price += 0.05m;
            }
            else if (parameter == "+1")
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
            else if (parameter == "-5")
            {
                price -= 0.05m;
            }
            return price;
        }

        internal void EditQty(string parameter)
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                return;
            }

            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            //newIntentionDic.Clear();
            Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();

            int matchExefail = 0;
            
            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in this.SelectedItems_Working)
                {
                    if (drv.Type == "INTENTION_GROUP")
                    {
                        continue;
                    }

                    if (drv.Status == "1")
                    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);

                        decimal qty = 0;
                        if (STC.Common.Utils.CommonUtils.IsNumeric(newOrderSingle.OrderQty))
                        {
                            qty = modifyQty(newOrderSingle.OrderQty.ConvertToDecimal(), parameter);
                        }
                        else
                        {
                            qty = modifyQty(decimal.Zero, parameter);
                        }

                        if (qty == 0)
                        {
                            newOrderSingle.OrderQty = null;
                        }
                        else
                        {
                            newOrderSingle.OrderQty = qty.ToString();
                        }

                        //个需的量不能修改为小于已匹配成交量
                        if (drv.MatchedQty.HasValue && drv.MatchedQty.Value > qty) 
                        {
                            if (this.SelectedItems_Working.Count == 1)
                            {
                                Common.View.Component.CustomMessageBox.Info("个需的量不能修改为小于已匹配成交量", "修改报量");
                            }
                            matchExefail++;
                            continue;
                        }

                        newOrderSingleDic[newOrderSingle.IntentionID] = newOrderSingle;
                    }
                    else if (drv.Status == "0" || drv.Status == "80" || drv.Status == "81")
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        newIntentionOrder.securityid = drv.SecurityId;
                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            if (STC.Common.Utils.CommonUtils.IsInvestor())
                            {
                                newIntentionOrder.actionFlag = "0";
                            }
                            else
                            {
                                newIntentionOrder.actionFlag = "2";
                            }

                            newIntentionOrder.riskManualCk = "1";
                        }

                        decimal qty = 0;
                        if (newIntentionOrder.qty.HasValue)
                        {
                            qty = modifyQty(newIntentionOrder.qty.Value, parameter);
                        }
                        else
                        {
                            qty = modifyQty(decimal.Zero, parameter);
                        }

                        if (qty == 0)
                        {
                            newIntentionOrder.qty = null;
                        }
                        else
                        {
                            newIntentionOrder.qty = qty;
                        }

                        //个需的量不能修改为小于已匹配成交量
                        if (drv.MatchedQty.HasValue && drv.MatchedQty.Value > qty)
                        {
                            continue;
                        }

                        //newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;
                        drv.UpdateIntentionRequest = newIntentionOrder;
                        drv.Request = null;
                        drv.InquiryRequest = null;
                        orderDic[newIntentionOrder.intentionId] = drv;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && orderDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }


            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = orderDic.Count + newOrderSingleDic.Count + matchExefail;

                    // 修改意向单
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        Sub_UpdateIntention(orderDic);
                    }
                    else
                    {
                        foreach (IntentionQuoteItem item in orderDic.Values)
                        {
                            PResult result = TableOldHandler.Instance.UpdateIntentionQuote(item.UpdateIntentionRequest);
                            if (result == null || !result.IsSuccess())
                            {
                                failCount++;
                            }
                        }
                    }

                    // 修改订单
                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    string text = string.Empty;
                    if (failCount + matchExefail > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount + matchExefail} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑报量", text);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
            });

        }

        private decimal modifyQty(decimal qty, string parameter)
        {
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

            if (qty < 0)
            {
                qty = 0;
            }
            else if (qty > 1000000)
            {
                qty = 1000000;
            }

            return qty;
        }

        internal void EditClientInfo()
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                return;
            }

            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            newIntentionDic.Clear();

            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in this.SelectedItems_Working)
                {
                    if (drv.Status == "1")
                    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);

                        // 机构编号
                        string institutionId = null;
                        // 机构名称
                        string institutionName = null;
                        if (this.SelectedInstintuion_Edit != null)
                        {
                            institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                            institutionId = this.SelectedInstintuion_Edit.idbkey;
                        }
                        else
                        {
                            institutionId = this.InstitutionName_Edit == newOrderSingle.InstitutionName ? newOrderSingle.InstitutionID : string.Empty;
                            institutionName = this.InstitutionName_Edit == null ? string.Empty : this.InstitutionName_Edit;
                        }

                        // 交易员编号
                        string traderId = null;
                        // 交易员名称
                        string traderName = null;
                        if (this.SelectedTrader_Edit != null)
                        {
                            traderName = this.SelectedTrader_Edit.namecn;
                            traderId = this.SelectedTrader_Edit.idbkey;
                            //item.QqNumber = item.SelectedTrader_Edit.QQ;
                        }
                        else
                        {
                            traderName = this.TraderName_Edit == null ? string.Empty : this.TraderName_Edit;
                            traderId = this.TraderName_Edit == newOrderSingle.TraderName ? newOrderSingle.TraderID : string.Empty;
                        }

                        if (!string.IsNullOrEmpty(institutionName))
                        {
                            newOrderSingle.InstitutionID = institutionId;
                            newOrderSingle.InstitutionName = institutionName;
                        }
                        if (!string.IsNullOrEmpty(traderName))
                        {
                            newOrderSingle.TraderID = traderId;
                            newOrderSingle.TraderName = traderName;
                        }
                        if (string.IsNullOrEmpty(institutionName) && string.IsNullOrEmpty(traderName))
                        {
                            newOrderSingle.InstitutionID = institutionId;
                            newOrderSingle.InstitutionName = institutionName;
                            newOrderSingle.TraderID = traderId;
                            newOrderSingle.TraderName = traderName;
                        }

                        //string clientType = SelectClientTypeItem != null ? selectClientTypeItem.instcode : string.Empty;
                        //if (!string.IsNullOrEmpty(clientType))
                        //{
                        //    newOrderSingle.OrgType = clientType;
                        //}

                        //newOrderSingle.VipCode = IsVip ? "0001" : "0010";

                        //TagData info2tag = null;
                        //if (JsonSplit.IsJson(drv.Info2))
                        //{
                        //    info2tag = JsonConvert.DeserializeObject<TagData>(drv.Info2);
                        //}
                        //else
                        //{
                        //    info2tag = new TagData();
                        //}
                        //info2tag.HURRY_FLAG = this.HurryFlag;
                        //info2tag.SUGGEST_FLAG = this.SuggestFlag;
                        //info2tag.FIRE_FLAG = this.FireFlag;
                        //info2tag.FLOOR_PRICE = this.FloorPrice;
                        //info2tag.OCO_FLAG = this.OCOFlag;

                        //newOrderSingle.Info2 = JsonConvert.SerializeObject(info2tag);
                        newOrderSingleDic[newOrderSingle.IntentionID] = newOrderSingle;
                    }
                    else if (drv.Status == "0")
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        //TagData info2tag = null;
                        //if (JsonSplit.IsJson(drv.Info2))
                        //{
                        //    info2tag = JsonConvert.DeserializeObject<TagData>(drv.Info2);
                        //}
                        //else
                        //{
                        //    info2tag = new TagData();
                        //}
                        // 机构编号
                        string institutionId = null;
                        // 机构名称
                        string institutionName = null;
                        if (this.SelectedInstintuion_Edit != null)
                        {
                            institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                            institutionId = this.SelectedInstintuion_Edit.idbkey;
                        }
                        else
                        {
                            institutionId = this.InstitutionName_Edit == newIntentionOrder.institutionName ? newIntentionOrder.institutionId : string.Empty;
                            institutionName = this.InstitutionName_Edit == null ? string.Empty : this.InstitutionName_Edit;
                        }
                       

                        // 交易员编号
                        string traderId = null;
                        // 交易员名称
                        string traderName = null;
                        if (this.SelectedTrader_Edit != null)
                        {
                            traderName = this.SelectedTrader_Edit.namecn;
                            traderId = this.SelectedTrader_Edit.idbkey;
                            //item.QqNumber = item.SelectedTrader_Edit.QQ;
                        }
                        else
                        {
                            traderName = this.TraderName_Edit == null ? string.Empty : this.TraderName_Edit;
                            traderId = this.TraderName_Edit == newIntentionOrder.traderName ? newIntentionOrder.traderId : string.Empty;
                        }

                        if (!string.IsNullOrEmpty(institutionName))
                        {
                            newIntentionOrder.institutionId = institutionId;
                            newIntentionOrder.institutionName = institutionName;
                        }
                        if (!string.IsNullOrEmpty(traderName))
                        {
                            newIntentionOrder.traderId = traderId;
                            newIntentionOrder.traderName = traderName;
                        }
                        if (string.IsNullOrEmpty(institutionName) && string.IsNullOrEmpty(traderName))
                        {
                            newIntentionOrder.institutionId = institutionId;
                            newIntentionOrder.institutionName = institutionName;
                            newIntentionOrder.traderId = traderId;
                            newIntentionOrder.traderName = traderName;
                        }

                        //string clientType = SelectClientTypeItem != null ? selectClientTypeItem.instcode : string.Empty;
                        //if (!string.IsNullOrEmpty(clientType))
                        //{
                        //    info2tag.ORG_TYPE = clientType;
                        //    newIntentionOrder.OrgType = clientType;
                        //}

                        //info2tag.VIP = IsVip ? "0001" : "0010";
                        //newIntentionOrder.VipCode = info2tag.VIP;

                        //info2tag.HURRY_FLAG = this.HurryFlag;
                        //info2tag.SUGGEST_FLAG = this.SuggestFlag;
                        //info2tag.FIRE_FLAG = this.FireFlag;
                        //info2tag.FLOOR_PRICE = this.FloorPrice;
                        //info2tag.OCO_FLAG = this.OCOFlag;

                        //newIntentionOrder.info2 = JsonConvert.SerializeObject(info2tag);
                        newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && newIntentionDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = newIntentionDic.Count + newOrderSingleDic.Count;
                    // 修改意向单
                    foreach (STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request in newIntentionDic.Values)
                    {
                        PResult result = TableOldHandler.Instance.UpdateIntentionQuote(request);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    // 修改订单
                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        sendPricePo.HandlInst = "2";
                        RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    string text = string.Empty;
                    if (failCount > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑", text);
                    }
                    //else
                    //{
                    //    text = "订单已更新";
                    //}
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
            });
        }

        internal void EditTags()
        {
            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            newIntentionDic.Clear();

            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in this.SelectedItems_Working)
                {
                    if (drv.Status == "1")
                    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);
                        TagData info2tag = null;
                        if (JsonSplit.IsJson(drv.Info2))
                        {
                            info2tag = JsonConvert.DeserializeObject<TagData>(drv.Info2);
                        }
                        else
                        {
                            info2tag = new TagData();
                        }
                        info2tag.HURRY_FLAG = this.HurryFlag;
                        info2tag.SUGGEST_FLAG = this.SuggestFlag;
                        info2tag.FIRE_FLAG = this.FireFlag;
                        info2tag.FLOOR_PRICE = this.FloorPrice;
                        info2tag.OCO_FLAG = this.OCOFlag;

                        newOrderSingle.Info2 = JsonConvert.SerializeObject(info2tag);
                        newOrderSingleDic[newOrderSingle.IntentionID] = newOrderSingle;
                    }
                    else if (drv.Status == "0")
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        TagData info2tag = null;
                        if (JsonSplit.IsJson(drv.Info2))
                        {
                            info2tag = JsonConvert.DeserializeObject<TagData>(drv.Info2);
                        }
                        else
                        {
                            info2tag = new TagData();
                        }

                        info2tag.HURRY_FLAG = this.HurryFlag;
                        info2tag.SUGGEST_FLAG = this.SuggestFlag;
                        info2tag.FIRE_FLAG = this.FireFlag;
                        info2tag.FLOOR_PRICE = this.FloorPrice;
                        info2tag.OCO_FLAG = this.OCOFlag;

                        newIntentionOrder.info2 = JsonConvert.SerializeObject(info2tag);
                        newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && newIntentionDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = newIntentionDic.Count + newOrderSingleDic.Count;
                    // 修改意向单
                    foreach (STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request in newIntentionDic.Values)
                    {
                        PResult result = TableOldHandler.Instance.UpdateIntentionQuote(request);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    // 修改订单
                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        sendPricePo.HandlInst = "2";
                        RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    string text = string.Empty;
                    if (failCount > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑", text);
                    }
                    //else
                    //{
                    //    text = "订单已更新";
                    //}
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
            });
        }


        internal void EditPersonalMarks(List<IntentionQuoteItem> modifyList, List<string> inquiryNotRefList, string remarks, bool isFinish = false)
        {
            BatchOperateBtnEnable = false;
            
            if (modifyList.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    // 修改意向单
                    foreach (IntentionQuoteItem requestItem in modifyList)
                    {
                        if (isFinish && (requestItem.Status == "0" || requestItem.Status == "1"))
                        {
                            orderDic[requestItem.IntentionId] = requestItem;

                            IntentionInquiryRequest inquiryRequest =GenIntentionInquiryRequest(requestItem.IntentionId, requestItem.GroupKey, inquiryNotRefList);
                            requestItem.InquiryRequest = inquiryRequest;

                            // --------------- confirmRequest -------------------
                            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest finishReq = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                            finishReq.intentionId = requestItem.IntentionId;
                            finishReq.actionType = "9";// 已完成
                            finishReq.userName = requestItem.BrokerId;
                            finishReq.closeBy = LocalEnv.UserInfo.UserName;
                            finishReq.closeByName = LocalEnv.UserInfo.FullName;
                            finishReq.updateTime = requestItem.UpdateTime;
                            finishReq.personalRemarks = remarks;

                            NoteData info5tag = null;
                            if (requestItem != null && JsonSplit.IsJson(requestItem.Info5))
                            {
                                info5tag = JsonConvert.DeserializeObject<NoteData>(requestItem.Info5);
                            }
                            else
                            {
                                info5tag = new NoteData();
                            }
                            if (info5tag.PERSONAL_REMARKS == null)
                            {
                                info5tag.PERSONAL_REMARKS = new Dictionary<string, string>();
                            }
                            info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName] = remarks;
                            finishReq.info5 = JsonConvert.SerializeObject(info5tag);

                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                            {
                                finishReq.quoteType = "BUYER_INTENTION";
                            }
                            else
                            {
                                finishReq.quoteType = "INTENTION";
                            }
                            requestItem.Request = finishReq;
                            // ---------------------------------------
                            requestItem.UpdateIntentionRequest = null;
                            
                        }
                        else
                        {
                            STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request = GetIntentionOrder(requestItem);
                            // info2
                            TagData info2tag = null;
                            if (JsonSplit.IsJson(requestItem.Info2))
                            {
                                info2tag = JsonConvert.DeserializeObject<TagData>(requestItem.Info2);
                            }
                            else
                            {
                                info2tag = new TagData();
                            }
                            info2tag.PERSONAL_REMARKS = remarks;
                            request.info2 = JsonConvert.SerializeObject(info2tag);
                            // info5
                            NoteData info5tag = null;
                            if (requestItem != null && JsonSplit.IsJson(requestItem.Info5))
                            {
                                info5tag = JsonConvert.DeserializeObject<NoteData>(requestItem.Info5);
                            }
                            else
                            {
                                info5tag = new NoteData();
                            }
                            if (info5tag.PERSONAL_REMARKS == null)
                            {
                                info5tag.PERSONAL_REMARKS = new Dictionary<string, string>();
                            }
                            info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName] = remarks;
                            request.info5 = JsonConvert.SerializeObject(info5tag);

                            orderDic[requestItem.IntentionId] = requestItem;
                            requestItem.Request = null;
                            requestItem.UpdateIntentionRequest = request;
                            requestItem.InquiryRequest = null;
                        }
                    }
                
                
                    if (orderDic != null && orderDic.Count > 0)
                    {
                        foreach(IntentionQuoteItem requestItem in orderDic.Values)
                        {
                            int inquiryFail = failCount;
                            if (requestItem.InquiryRequest != null)
                            {
                                GetErrorInfo_Inquiry(requestItem.InquiryRequest,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                            }
                            else
                            {
                                totalCount++;
                            }

                            if (inquiryFail - failCount < 1)
                            {
                                if (isFinish)
                                {
                                    totalCount--;
                                    GetErrorInfo(requestItem.Request,
                                       ref failCount,
                                       ref totalCount,
                                       warnReplyDic,
                                       prohibitReplyDic,
                                       exceptionReplyDic);
                                }
                                else
                                {
                                    totalCount--;
                                    GetErrorInfo_UpdateIntention(requestItem.UpdateIntentionRequest,
                                       ref failCount,
                                       ref totalCount,
                                       warnReplyDic,
                                       prohibitReplyDic,
                                       exceptionReplyDic);
                                }
                            }
                        }

                        while (totalCount != orderDic.Count)
                        {
                            Thread.Sleep(100);
                        }

                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (failCount > 0)
                            {
                                if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                                {
                                    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                        warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                    batchResultWindow.Owner = Application.Current.MainWindow;
                                    batchResultWindow.ShowDialog();
                                }
                            }
                        }));
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
            });
        }

        protected Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(IntentionQuoteItem item)
        {
            Common.Model.Fix.Message.NewOrderSingle sendPricePo = new Common.Model.Fix.Message.NewOrderSingle();

            // BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            sendPricePo.SecurityType = item.SecurityType;
            sendPricePo.Symbol = item.Symbol;
            sendPricePo.StrategyOrderId = item.OrderID;

            sendPricePo.AlgoName = "CROSS";
            sendPricePo.VenueTypeGW = "INNER";
            sendPricePo.Venues = "MANUAL";
            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            sendPricePo.MarketIndicator = "4";
            sendPricePo.OrdType = "2";
            sendPricePo.OrderQty = item.Qty?.ToString();
            sendPricePo.Price = item.Price?.ToString();
            sendPricePo.NetPrice = item.CleanPrice?.ToString();
            sendPricePo.FullPrice = item.FullPrice?.ToString();
            sendPricePo.SecurityID = item.SecurityId;
            sendPricePo.PriceType = item.PriceType; //收益率
            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            sendPricePo.SettlType = item.SettlType;
            sendPricePo.Side = item.Side;

            sendPricePo.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = "0";
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;
            sendPricePo.Name = LocalEnv.UserInfo.UserName; //放当前操作员

            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = item.FirmFlag;
            sendPricePo.WholeFlag = item.WholeFlag;

            //sendPricePo.QuantID = quantID;

            //sendPricePo.VenuesRdmNum = randomNum;

            //sendPricePo.VenuesSelectF = venuesSelectF;

            //sendPricePo.SelfSelectVenues = selfSelectVenues;

            sendPricePo.InstitutionID = item.InstitutionId; //客盘机构id
            sendPricePo.InstitutionName = item.InstitutionName;//客盘机构名字
            sendPricePo.TraderID = item.TraderId; // 客盘交易员id
            sendPricePo.TraderName = item.TraderName;//客盘交易员名字
            //sendPricePo.ShowTrader = showTrader;// 是否显示经济商

            //sendPricePo.ValidUntilTime = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");
            sendPricePo.Terminal = string.Format("STClient_{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.Info1 = item.Info1;
            sendPricePo.Info2 = item.Info2;
            sendPricePo.ValidUntilTime = GetValidUntilTime(item.Info1);

            sendPricePo.Info4 = item.IntentionId;
            //sendPricePo.AccountID = accountid;
            //sendPricePo.AccountName = accountName;
            sendPricePo.VipCode = item.vipCode;
            if (!string.IsNullOrEmpty(item.Info2) && JsonSplit.IsJson(item.Info2))
            {
                TagData info2tag = JsonConvert.DeserializeObject<TagData>(item.Info2);
                sendPricePo.IfSubBond = info2tag.IF_SUB_BOND;
                sendPricePo.OCOFlag = info2tag.OCO_FLAG;
            }

            //sendPricePo.PriceCondition = priceCondition;
            sendPricePo.IntentionID = item.IntentionId;
            sendPricePo.Channel = string.IsNullOrEmpty(item.Channel) ? "CLIENT" : item.Channel;
            sendPricePo.QuoteType = item.QuoteType;
            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();

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

        protected STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest GetIntentionOrder(IntentionQuoteItem item)
        {
            STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest();
            request.intentionId = item.IntentionId;
            request.institutionId = item.InstitutionId;
            request.institutionName = item.InstitutionName;
            request.traderId = item.TraderId;
            request.traderName = item.TraderName;
            request.side = item.Side;
            request.price = item.Price;
            request.cleanPrice = item.CleanPrice;
            request.fullPrice = item.FullPrice;
            request.priceType = item.PriceType;
            request.qty = item.Qty;
            request.settlType = item.SettlType;
            request.wholeFlag = item.WholeFlag;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.userName = string.IsNullOrEmpty(item.UserName) ? LocalEnv.UserInfo.UserName : item.UserName;
            request.hurryFlag = item.HurryFlag;
            request.suggestFlag = item.SuggestFlag;
            request.fireFlag = item.FireFlag;
            request.floorPrice = item.FloorPrice;
            request.OcoFlag = item.OCOFlag;
            request.publicRemarks = item.PublicRemarks;
            request.personalRemarks = item.PersonalRemarks;
            request.customerPrice = item.CustomerPrice;
            request.customerSpread = item.BP;
            request.info2 = item.Info2;
            request.channel = string.IsNullOrEmpty(item.Channel) ? "CLIENT" : item.Channel;
            request.QuoteType = item.QuoteType;
            return request;
        }

        protected string GetValidUntilTime(string info1)
        {
            OrderTimeModel otm = null;
            if (JsonSplit.IsJson(info1))
            {
                otm = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderTimeModel>(info1);
            }
            string ValidUntilTime = string.Empty;
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
            else if (otm != null && !string.IsNullOrEmpty(otm.ValidTimeType) && otm.ValidTimeType.StartsWith("1|"))
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
            return ValidUntilTime;
        }

        #endregion


        #region 市场最优
        private Visibility loadmaskvisible_Broker = Visibility.Collapsed;

        public Visibility BrokerLoadMaskVisible
        {
            get
            {
                return loadmaskvisible_Broker;
            }

            set
            {
                loadmaskvisible_Broker = value;
                this.OnPropertyChanged();
            }
        }

        private InnerOrderData _selectedItem_MyInnerOrders_Broker;

        public InnerOrderData SelectedItem_MyInnerOrders_Broker
        {
            get
            {
                return _selectedItem_MyInnerOrders_Broker;
            }
            set
            {
                _selectedItem_MyInnerOrders_Broker = value;
                this.OnPropertyChanged();
            }
        }

        //**************** 市场行情 **********************
        internal object _dataLock_broker = new object();
        private CollectionViewSource _viewSource_broker = new CollectionViewSource();

        public CollectionViewSource ViewSource_Broker
        {
            get { return _viewSource_broker; }
            set { _viewSource_broker = value; }
        }

        private CustomCollection<InnerOrderData> _innerOrders_broker = new CustomCollection<InnerOrderData>();
        public CustomCollection<InnerOrderData> InnerOrders_Broker
        {
            get { return _innerOrders_broker; }
            set { _innerOrders_broker = value; }
        }

        internal ConcurrentDictionary<string, InnerOrderData> _flashDic_broker = new ConcurrentDictionary<string, InnerOrderData>();
        internal ConcurrentDictionary<string, InnerOrderData> _Cache_InnerOrders_Dic_Broker = new ConcurrentDictionary<string, InnerOrderData>();
        //********************************************

        QueryCustMdReq queryCustMdReq = new QueryCustMdReq() { pageNum = 1, pageSize = 200, userName = LocalEnv.UserInfo.UserName };
        private void QueryBrokerDatas()
        {
            Task.Run(() =>
            {
                // 请求CustSvr，stc.cust.md.query 数据
                queryCustMdReq.reqTags = this._focusTagDic;
                queryCustMdReq.reqParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(this.FilterText))
                {
                    queryCustMdReq.reqParams["SecurityID"] = this.FilterText;
                    //queryCustMdReq.reqParams["Symbol"] = this.FilterText;
                }

                queryCustMdReq.extParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(this.StartPrice) || !string.IsNullOrEmpty(this.EndPrice))
                {
                    queryCustMdReq.extParams["YieldRange"] = $"{this.StartPrice},{this.EndPrice}";
                }
                if (!string.IsNullOrEmpty(this.StartCoupon) || !string.IsNullOrEmpty(this.EndCoupon))
                {
                    queryCustMdReq.extParams["CouponRateCurrentRange"] = $"{this.StartCoupon},{this.EndCoupon}";
                }
                if (!string.IsNullOrEmpty(this.StartChinaBondValue) || !string.IsNullOrEmpty(this.EndChinaBondValue))
                {
                    queryCustMdReq.extParams["CouponRateCurrentRange"] = $"{this.StartChinaBondValue},{this.EndChinaBondValue}";
                }
                if (!string.IsNullOrEmpty(this.StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                {
                    long start = STC.Common.Utils.CommonUtils.GetDays(this.StartTermDY);
                    long end = STC.Common.Utils.CommonUtils.GetDays(this.EndTermDY);
                    string s = start == 0 ? string.Empty : start.ToString();
                    string e = end == 0 ? string.Empty : end.ToString();
                    queryCustMdReq.extParams["TermDaysRange"] = $"{s},{e}";
                    queryCustMdReq.couponRangeStr = $"{s},{e}";
                }

                QueryCustMdReplyPo result = CustHandler.Instance.QueryCustMd(queryCustMdReq);

                if (result != null && result.datas != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.InnerOrders_Broker.Clear();
                        foreach (BrokerMarketDataSnapshot item in result.datas)
                        {
                            UpdateBrokerMd(item);
                        }
                    }));
                }

                // 关闭滚动
                this.BrokerLoadMaskVisible = Visibility.Collapsed;
            });
        }

        private void UpdateBrokerMd(BrokerMarketDataSnapshot brokerData)
        {
            InnerOrderData data = new InnerOrderData();
            data.SecurityID = brokerData.SecurityID;
            data.Symbol = brokerData.Symbol;
            data.UpdateTime = brokerData.SendingTime;
            data.CCDCValuation_Date = brokerData.CCDCValuaDate;
            data.LastMarketPx = brokerData.LastPrice.ConvertToDecimal();
            if (!string.IsNullOrEmpty(brokerData.LastWorkDay))
            {
                if (brokerData.LastWorkDay.Replace("-", "").Replace("/", "") != brokerData.CCDCValuaDate.Replace("-", "").Replace("/", ""))
                {
                    data.ShowLastWorkDay = "!";
                }
            }
            BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(brokerData.SecurityID);
            if (bondsInfoPo != null)
            {
                //Symbol = bondsInfoPo.Short_Name;
                //Security = bondsInfoPo.Bond_ID;
                //SymbolName = bondsInfoPo.Short_Name;
                //findData_best.RemainingTime = bondsInfoPo.RemaingingTime;
                data.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                data.SecurityType = bondsInfoPo.SECURITY_TYPE;
                data.BondSubType = bondsInfoPo.Bond_Subtype;
                //findData_best.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                //findData_best.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                //findData_best.Issuer = bondsInfoPo.issuer;
                //data.RemainingTime = bondsInfoPo.RemaingingTime;
                //data.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                data.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
            }

            data.Sign = brokerData.Sign.ConvertToLong();
            data.Sign1 = brokerData.Sign1.ConvertToLong();
            data.Sign2 = brokerData.Sign2.ConvertToLong();
            data.Sign3 = brokerData.Sign3.ConvertToLong();
            data.Rating = SignParser.GenRating(data.SignDic1, data.SignDic3, data.SecurityID);// 评级信息
            data.Cdc_Implited_Rating = SignParser.GetCDCImplitedRating(data.SignDic3);//中债隐含
            data.BondCategory = SignParser.GenBondCategory(data.SignDic, data.SignDic1);//类型
            data.Level = SignParser.GenLevel(data.SignDic3);
            data.RegionType = SignParser.GenRegionType(data.SignDic2);
            data.EnterpriseType = SignParser.GenEnterpriseType(data.SignDic1);
            data.If_Guarantee = SignParser.GenIfGuarantee(data.SignDic1);
            data.If_Repay = SignParser.GenIfRepay(data.SignDic3);
            data.IssueType = SignParser.GenIssueType(data.SignDic1);
            data.DQType = SignParser.GenDQType(data.SignDic, false, _innerType);


            if (string.IsNullOrEmpty(brokerData.Info1) == false)
            {
                Info1 inf1 = JsonConvert.DeserializeObject<Info1>(brokerData.Info1);
                data.TermDays = inf1.termDays.ConvertToDecimal();
                data.CouponRateCurrent = inf1.couponRateCurrent.ConvertToDecimal();
                data.IssuerCode = inf1.issuerCode;
                ChildTag ct = STC.Common.Utils.CommonUtils.GetIssuer(inf1.issuerCode);
                data.IssuerName = ct != null ? ct.childTagName : string.Empty;
                //data.WarranterCode = bestBidInnerOrder.WarranterCode;
                //data.WarranterName = bestBidInnerOrder.WarranterName;
                ////findData_best.RemainingTime_New = bestBidInnerOrder.RemainingTime_New;
                data.ChinaBondValuation = inf1.chinaBondValuation.ConvertToDecimal();
                data.ChinaBondValuationFull = inf1.chinaBondValuationFull;
                //data.ChinaBondExercise = bestBidInnerOrder.ChinaBondExercise;
                //data.ChinaBondDue = bestBidInnerOrder.ChinaBondDue;
                //data.CCDCValuation_BP = bestBidInnerOrder.CCDCValuation_BP;
                data.CsiBondValuation = inf1.csiBondValuation.ConvertToDecimal();
                data.CsiBondValuationFull = inf1.csiBondValuationFull;
                //data.CsiBondValuationExercise = bestBidInnerOrder.CsiBondValuationExercise;
                //data.CsiBondValuationDue = bestBidInnerOrder.CsiBondValuationDue;
                //data.ModifiedDuration = bestBidInnerOrder.ModifiedDuration;
                data.DayOffStr = inf1.dayOffStr;
                //data.Rating = bestBidInnerOrder.Rating;
                //data.Cdc_Implited_Rating = bestBidInnerOrder.Cdc_Implited_Rating;
                //data.BondCategory = bestBidInnerOrder.BondCategory;
                //data.Level = bestBidInnerOrder.Level;
                //data.RegionType = bestBidInnerOrder.RegionType;
                //data.EnterpriseType = bestBidInnerOrder.EnterpriseType;
                //data.If_Guarantee = bestBidInnerOrder.If_Guarantee;
                //data.If_Repay = bestBidInnerOrder.If_Repay;
                //data.IssueType = bestBidInnerOrder.IssueType;
                data.IssueAmount = inf1.issueAmount.ConvertToDecimal();
                data.Outstanding_Amount = inf1.Outstanding_Amount.ConvertToDecimal();
                data.RemainingTime = inf1.tenor;
                data.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(inf1.tenor);
                data.IssuerCode = inf1.issuerCode;
                data.IssuerName = inf1.issuerName;


                if (SignParser.IsCumType(data.SignDic1))
                {
                    if (string.IsNullOrEmpty(inf1.chinaBondValuationFull) == false)
                    {
                        int speIndex = inf1.chinaBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            data.ChinaBondExercise = inf1.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            data.ChinaBondDue = inf1.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            data.ChinaBondDue = inf1.chinaBondValuationFull.ConvertToDecimal();
                        }
                    }

                    if (string.IsNullOrEmpty(inf1.csiBondValuationFull) == false)
                    {
                        int speIndex = inf1.csiBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            data.CsiBondValuationExercise = inf1.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            data.CsiBondValuationDue = inf1.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            data.CsiBondValuationDue = inf1.csiBondValuationFull.ConvertToDecimal();
                        }
                    }
                }
                else
                {
                    data.ChinaBondDue = inf1.chinaBondValuation.ConvertToDecimal();

                    data.CsiBondValuationDue = inf1.csiBondValuation.ConvertToDecimal();
                }
            }

            var bidList = brokerData.NoMDEntries?.Where(entry => entry.MDEntryType == "0");
            if (bidList != null)
            {
                //this.BidRowCount++;
                int bidCount = 0;
                string partysubname = string.Empty;
                long? qty = 0;
                string bestPrice = string.Empty;
                StringBuilder partySubNameTooltip = new StringBuilder();
                StringBuilder qtyTooltip = new StringBuilder();
                StringBuilder copyInfo = new StringBuilder();
                foreach (MDEntries entry in bidList)
                {

                    string result = CalCornerMarket(data.Marks_InnerBid, entry.MDEntrySize.ConvertToLong(), entry.Text);
                    data.Marks_InnerBid = result;
                    if (entry.MDPriceLevel == "1")
                    {
                        bestPrice = entry.MDEntryPx;
                    }

                    if (entry.MDEntryPx == bestPrice)
                    {
                        bidCount++;
                        long size = entry.MDEntrySize.ConvertToLong();
                        qty += size;

                        if (string.IsNullOrEmpty(partysubname))
                        {
                            partysubname = entry.Venues;
                        }

                        if (partySubNameTooltip.Length > 0)
                        {
                            partySubNameTooltip.Append(" + ");
                        }
                        partySubNameTooltip.Append(ComboSourceUtil.Instance.FindEnum(entry.Venues, "venues"));

                        if (qtyTooltip.Length > 0)
                        {
                            qtyTooltip.Append(" + ");
                        }
                        qtyTooltip.Append(!string.IsNullOrEmpty(entry.MDEntrySize) ? entry.MDEntrySize : "Bid");

                        if (copyInfo.Length > 0)
                        {
                            copyInfo.Append("+");
                        }

                        copyInfo.Append(GetCopyInfo(entry.MDEntrySize, entry));
                    }
                }

                data.PartySubName_InnerBid = partySubNameTooltip.ToString().Contains("+") ? "多" : partysubname;
                data.PartySubName_Tooltip_InnerBid = partySubNameTooltip.ToString();
                data.Qty_InnerBid = qty == 0 ? null : qty;
                data.CopyInfo_InnerBid = copyInfo.ToString();
                data.Qty_Tooltip_InnerBid = qtyTooltip.ToString();
                ////int count = bid_InnerOrdersList.Count(item => item.Price == bestBidInnerOrder.Price);
                data.Count_InnerBid = bidCount;
                data.Price_InnerBid = bestPrice.ConvertToDecimal();
                if (data.ChinaBondValuation.HasValue && data.ChinaBondValuation != decimal.Zero && data.Price_InnerBid.HasValue)
                {
                    data.CCDCValuation_BP_InnerBid = (data.Price_InnerBid - data.ChinaBondValuation) * 100;
                }
                if (data.ChinaBondExercise.HasValue && data.ChinaBondExercise != decimal.Zero && data.Price_InnerBid.HasValue)
                {
                    data.CCDCValuation_BP_InnerBid = (data.Price_InnerBid - data.ChinaBondExercise) * 100;
                }

                //data.FirmFlag_InnerBid = bestBidInnerOrder.FirmFlag;
                //data.SettleType_InnerBid = bestBidInnerOrder.SettleType;
                //data.RoleId_InnerBid = bestBidInnerOrder.RoleId;
                //data.RoleName_InnerBid = bestBidInnerOrder.RoleName;
                //data.WholeFlag_InnerBid = bestBidInnerOrder.WholeFlag;
                //data.IsMe_InnerBid = bestBidInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                //data.IntentionId_InnerBid = bestBidInnerOrder.IntentionID;
                //data.NetPrice_InnerBid = bestBidInnerOrder.NetPrice;
                //data.OrderID_InnerBid = bestBidInnerOrder.OrderID;
                //data.StrategyOrderId_innerBid = bestBidInnerOrder.StrategyOrderId;
                //data.Client_InnerBid = GenClient(data.SignDic, b, false); // 客户（可能不需要）
                //data.TraderID_InnerBid = bestBidInnerOrder.TraderID;
                //data.TraderName_InnerBid = bestBidInnerOrder.TraderName;
                //data.InstitutionID_InnerBid = bestBidInnerOrder.InstitutionID;
                //data.InstitutionName_InnerBid = bestBidInnerOrder.InstitutionName;
                //data.DQType_InnerBid = bestBidInnerOrder.DQType;
                //data.HurryFlag_InnerBid = bestBidInnerOrder.HurryFlag;
                //data.SuggestFlag_InnerBid = bestBidInnerOrder.SuggestFlag;
                //data.FireFlag_InnerBid = bestBidInnerOrder.FireFlag;
                //data.FloorPrice_InnerBid = bestBidInnerOrder.FloorPrice;
                //data.OCOFlag_InnerBid = bestBidInnerOrder.OCOFlag;
                //data.PublicRemarks_InnerBid = bestBidInnerOrder.PublicRemarks;
                //data.PersonalRemarks_InnerBid = findData_best.IsMe_InnerBid ? bestBidInnerOrder.PersonalRemarks : string.Empty;
            }

            var ofrList = brokerData.NoMDEntries?.Where(entry => entry.MDEntryType == "1");
            if (ofrList != null)
            {
                //this.OfrRowCount++;
                int ofrCount = 0;
                string partysubname = string.Empty;
                long? qty = 0;
                string bestPrice = string.Empty;
                StringBuilder partySubNameTooltip = new StringBuilder();
                StringBuilder qtyTooltip = new StringBuilder();
                StringBuilder copyInfo = new StringBuilder();

                foreach (MDEntries entry in ofrList)
                {
                    string result = CalCornerMarket(data.Marks_InnerOfr, entry.MDEntrySize.ConvertToLong(), entry.Text);
                    data.Marks_InnerOfr = result;

                    if (entry.MDPriceLevel == "1")
                    {
                        bestPrice = entry.MDEntryPx;
                    }
                    if (entry.MDEntryPx == bestPrice)
                    {
                        ofrCount++;
                        long size = entry.MDEntrySize.ConvertToLong();
                        qty += size;

                        if (string.IsNullOrEmpty(partysubname))
                        {
                            partysubname = entry.Venues;
                        }

                        if (partySubNameTooltip.Length > 0)
                        {
                            partySubNameTooltip.Append(" + ");
                        }
                        partySubNameTooltip.Append(ComboSourceUtil.Instance.FindEnum(entry.Venues, "venues"));

                        if (qtyTooltip.Length > 0)
                        {
                            qtyTooltip.Append(" + ");
                        }
                        qtyTooltip.Append(!string.IsNullOrEmpty(entry.MDEntrySize) ? entry.MDEntrySize : "Ofr");

                        if (copyInfo.Length > 0)
                        {
                            copyInfo.Append("+");
                        }

                        copyInfo.Append(GetCopyInfo(entry.MDEntrySize, entry));
                    }
                    else
                    {
                        break;
                    }
                }

                data.PartySubName_InnerOfr = partySubNameTooltip.ToString().Contains("+") ? "多" : partysubname;
                data.PartySubName_Tooltip_InnerOfr = partySubNameTooltip.ToString();
                data.Qty_InnerOfr = qty == 0 ? null : qty;
                data.CopyInfo_InnerOfr = copyInfo.ToString();
                data.Qty_Tooltip_InnerOfr = qtyTooltip.ToString();


                //data.TermDays = bestOfrInnerOrder.TermDays;
                //data.CouponRateCurrent = bestOfrInnerOrder.CouponRateCurrent;

                //data.WarranterCode = bestOfrInnerOrder.WarranterCode;
                //data.WarranterName = bestOfrInnerOrder.WarranterName;
                ////data.RemainingTime_New = bestOfrInnerOrder.RemainingTime_New;
                //data.ChinaBondValuation = bestOfrInnerOrder.ChinaBondValuation;
                //data.ChinaBondValuationFull = bestOfrInnerOrder.ChinaBondValuationFull;
                //data.ChinaBondExercise = bestOfrInnerOrder.ChinaBondExercise;
                //data.ChinaBondDue = bestOfrInnerOrder.ChinaBondDue;
                //data.CCDCValuation_BP = bestOfrInnerOrder.CCDCValuation_BP;
                //data.CsiBondValuation = bestOfrInnerOrder.CsiBondValuation;
                //data.CsiBondValuationFull = bestOfrInnerOrder.CsiBondValuationFull;
                //data.CsiBondValuationExercise = bestOfrInnerOrder.CsiBondValuationExercise;
                //data.CsiBondValuationDue = bestOfrInnerOrder.CsiBondValuationDue;
                //data.ModifiedDuration = bestOfrInnerOrder.ModifiedDuration;
                //data.DayOffStr = bestOfrInnerOrder.DayOffStr;
                //data.Rating = bestOfrInnerOrder.Rating;
                //data.Cdc_Implited_Rating = bestOfrInnerOrder.Cdc_Implited_Rating;
                //data.BondCategory = bestOfrInnerOrder.BondCategory;
                //data.Level = bestOfrInnerOrder.Level;
                //data.RegionType = bestOfrInnerOrder.RegionType;
                //data.EnterpriseType = bestOfrInnerOrder.EnterpriseType;
                //data.If_Guarantee = bestOfrInnerOrder.If_Guarantee;
                //data.If_Repay = bestOfrInnerOrder.If_Repay;
                //data.IssueType = bestOfrInnerOrder.IssueType;
                //data.IssueAmount = bestOfrInnerOrder.IssueAmount;
                //data.Outstanding_Amount = bestOfrInnerOrder.Outstanding_Amount;
                data.Price_InnerOfr = bestPrice.ConvertToDecimal();
                if (data.ChinaBondValuation.HasValue && data.ChinaBondValuation != decimal.Zero && data.Price_InnerOfr.HasValue)
                {
                    data.CCDCValuation_BP_InnerOfr = (data.ChinaBondValuation - data.Price_InnerOfr) * 100;
                }
                if (data.ChinaBondExercise.HasValue && data.ChinaBondExercise != decimal.Zero && data.Price_InnerOfr.HasValue)
                {
                    data.CCDCValuation_BP_InnerOfr = (data.ChinaBondExercise - data.Price_InnerOfr) * 100;
                }
                ////int count = ofr_InnerOrdersList.Count(item => item.Price == bestOfrInnerOrder.Price);
                data.Count_InnerOfr = ofrCount;
                //data.FirmFlag_InnerOfr = bestOfrInnerOrder.FirmFlag;
                //data.SettleType_InnerOfr = bestOfrInnerOrder.SettleType;
                //data.RoleId_InnerOfr = bestOfrInnerOrder.RoleId;
                //data.RoleName_InnerOfr = bestOfrInnerOrder.RoleName;
                //data.WholeFlag_InnerOfr = bestOfrInnerOrder.WholeFlag;
                //data.IsMe_InnerOfr = bestOfrInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                //data.IntentionId_InnerOfr = bestOfrInnerOrder.IntentionID;
                //data.NetPrice_InnerOfr = bestOfrInnerOrder.NetPrice;
                //data.OrderID_InnerOfr = bestOfrInnerOrder.OrderID;
                //data.StrategyOrderId_InnerOfr = bestOfrInnerOrder.StrategyOrderId;

                //data.Client_InnerOfr = GenClient(data.SignDic, b, false); //  客户（可能不需要）
                //data.TraderID_InnerOfr = bestOfrInnerOrder.TraderID;
                //data.TraderName_InnerOfr = bestOfrInnerOrder.TraderName;
                //data.InstitutionID_InnerOfr = bestOfrInnerOrder.InstitutionID;
                //data.InstitutionName_InnerOfr = bestOfrInnerOrder.InstitutionName;
                //data.DQType_InnerOfr = bestOfrInnerOrder.DQType;
                //data.HurryFlag_InnerOfr = bestOfrInnerOrder.HurryFlag;
                //data.SuggestFlag_InnerOfr = bestOfrInnerOrder.SuggestFlag;
                //data.FireFlag_InnerOfr = bestOfrInnerOrder.FireFlag;
                //data.FloorPrice_InnerOfr = bestOfrInnerOrder.FloorPrice;
                //data.OCOFlag_InnerOfr = bestOfrInnerOrder.OCOFlag;
                //data.PublicRemarks_InnerOfr = bestOfrInnerOrder.PublicRemarks;
                //data.PersonalRemarks_InnerOfr = findData_best.IsMe_InnerOfr ? bestOfrInnerOrder.PersonalRemarks : string.Empty;
            }




            lock (this._dataLock_broker)
            {
                if (isDisplay_Md(data))
                    this.InnerOrders_Broker.Insert(0, data);
                //this.RowCount++;
            }
        }

        private string CalCornerMarket(string oldTxt, decimal qty, string text)
        {
            string newText = qty.ToString();
            if (!string.IsNullOrEmpty(text))
                newText = string.Format("{0}", text);//string.Format("{0}({1})", newText, text);

            string oldText = oldTxt;

            if (!string.IsNullOrEmpty(oldText))
            {
                if (!string.IsNullOrEmpty(newText))
                    newText = string.Format("{0}+{1}", oldText, newText);
                else
                    newText = oldText;
            }

            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(oldText))
                return string.Empty;

            return newText;

        }

        private bool isDisplay_Md(InnerOrderData data)
        {
            bool result_InnerBid = true;
            bool result_InnerOfr = true;
            // bid-中债
            decimal? value = null;
            if (data.Price_InnerBid.HasValue && data.ChinaBondValuation.HasValue)
            {
                value = (data.Price_InnerBid - data.ChinaBondValuation) * 100;
            }
            result_InnerBid = result_InnerBid
               && ((value.HasValue == false && string.IsNullOrEmpty(this.StartBidBp) && string.IsNullOrEmpty(this.EndBidBp))
               || (value.HasValue
               && (string.IsNullOrEmpty(this.StartBidBp) || (!string.IsNullOrEmpty(this.StartBidBp) && value >= this.StartBidBp.ConvertToDecimal()))
               && (string.IsNullOrEmpty(this.EndBidBp) || (!string.IsNullOrEmpty(this.EndBidBp) && value <= this.EndBidBp.ConvertToDecimal()))));

            // 中债-ofr
            value = null;
            if (data.Price_InnerOfr.HasValue && data.ChinaBondValuation.HasValue)
            {
                value = (data.ChinaBondValuation - data.Price_InnerOfr) * 100;
            }
            result_InnerOfr = result_InnerOfr
                && ((value.HasValue == false && string.IsNullOrEmpty(this.StartOfrBp) && string.IsNullOrEmpty(this.EndOfrBp))
                || (value.HasValue
                && (string.IsNullOrEmpty(this.StartOfrBp) || (!string.IsNullOrEmpty(this.StartOfrBp) && value >= this.StartOfrBp.ConvertToDecimal()))
                && (string.IsNullOrEmpty(this.EndOfrBp) || (!string.IsNullOrEmpty(this.EndOfrBp) && value <= this.EndOfrBp.ConvertToDecimal()))));

            if (!(result_InnerBid && result_InnerOfr))
                return false;
            else
                return true;
        }

        private string GetCopyInfo(string qty, MDEntries er)
        {
            string nonfirmStar = string.Empty;
            if (er.FirmFlag == "0")
            {
                nonfirmStar = "*";
            }
            StringBuilder copyInfo = new StringBuilder();
            string settlTypeDisplay = ComboSourceUtil.Instance.FindEnum(er.SettlType, "settletype");
            if (settlTypeDisplay != "T+1")
            {
                if (copyInfo.Length > 0)
                {
                    copyInfo.Append(",");
                }
                copyInfo.Append(settlTypeDisplay);
            }

            string copyText = string.IsNullOrEmpty(qty) ? "--" : qty;
            if (nonfirmStar.Length > 0)
            {
                copyText = $"{copyText}{nonfirmStar}";
            }
            if (copyInfo.Length > 0)
            {
                copyText = $"{copyText}({copyInfo})";
            }
            return copyText;
        }
        #endregion

        #region 暂缓刷线

        private void ProcessSuspendThread()
        {
            while (true)
            {
                if (suspendDic.Count == 0)
                {
                    _Suspend_ManualResetEvent.Reset();
                    _Suspend_ManualResetEvent.WaitOne();
                }

                Thread.Sleep(500);

                DealSuspendDatas();
            }
        }

        private void DealSuspendDatas()
        {
            List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> historyList = suspendDic.Values.ToList();
            suspendDic.Clear();
            Sub_Instance_OnHistoryIntentionQuotes(historyList);
        }

        #endregion

        #region 刷新参考筛选
        private bool isFiltering = false;
        public static ManualResetEvent _Filter_ManualResetEvent = new ManualResetEvent(false);
        List<object> filterTaskList = new List<object>();
        private System.Threading.Thread FilterUIThread = null;//刷新ui线程
        public static ManualResetEvent _Suspend_ManualResetEvent = new ManualResetEvent(false);
        private Thread SuspendThread = null;//延迟刷新

        public void NotifyFilter()
        {
            lock (filterTaskList)
            {
                filterTaskList.Add(string.Empty);
                _Filter_ManualResetEvent.Set();
            }
        }


        private void ProcessFilterThread()
        {
            while (true)
            {
                if (filterTaskList.Count == 0)
                {
                    _Filter_ManualResetEvent.Reset();
                    _Filter_ManualResetEvent.WaitOne();
                }

                if (!isFiltering)
                {
                    lock (filterTaskList)
                    {
                        filterTaskList.Clear();
                    }

                    ProcessFilterData();

                    OnSearchMarketDataCommand();

                    Thread.Sleep(500);
                }
            }
        }

        private void ProcessFilterData()
        {
            isFiltering = true;
            List<IntentionQuoteItem> demandList = new List<IntentionQuoteItem>();
            lock (_dataLock_demand)
            {
                int index = 0;
                foreach (DataRow drv in DemandDT.Rows)
                {
                    IntentionQuoteItem item = drv[ITEM_FIELD] as IntentionQuoteItem;
                    bool flag = isDisplay(item);
                    drv[ISDISPLAY_FIELD] = flag;
                    index++;
                }

                foreach (DataRowView drv in DemandDV)
                {
                    IntentionQuoteItem item = drv[ITEM_FIELD] as IntentionQuoteItem;
                    if (demandList.Count >= 200)
                    {
                        break;
                    }
                    else
                    {
                        demandList.Add(item);
                    }
                }
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                lock (_dataLock_display)
                {
                    _Cache_display_Dic.Clear();
                    InnerOrders_display.Clear();
                    _DemandCount = 0;
                    foreach (IntentionQuoteItem item in demandList)
                    {
                        _Cache_display_Dic[item.IntentionId] = item;
                        InnerOrders_display.Add(item);
                        _DemandCount++;
                    }
                    demandList.Clear();
                }
                if (_Cache_display_Dic.Count != InnerOrders_display.Count)
                {
                    LogHelper.Error("私域池数据不匹配4");
                }
                isFiltering = false;
            }));
        }
        #endregion

        #region 根据历史查询
        private DateTime? dealTimeBegin;
        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? DealTimeBegin
        {
            get { return dealTimeBegin; }
            set
            {
                if ((dealTimeBegin == null && value != null)
                    || (dealTimeBegin != null && value == null)
                    || (dealTimeBegin != null && value != null && dealTimeBegin?.ToString("yyyy-MM-dd") != value?.ToString("yyyy-MM-dd")))
                {
                    dealTimeBegin = value;
                    this.OnPropertyChanged();

                    if (LocalEnv.UserInfo.RoleType == RoleType.SELLER_ROLE)
                    {
                        OnSearchIntentionDataCommand();
                    }
                }
            }
        }

        private DateTime? dealTimeEnd;
        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? DealTimeEnd
        {
            get { return dealTimeEnd; }
            set
            {
                if ((dealTimeEnd == null && value != null)
                    || (dealTimeEnd != null && value == null)
                    || (dealTimeEnd != null && value != null && dealTimeEnd?.ToString("yyyy-MM-dd") != value?.ToString("yyyy-MM-dd")))
                {
                    dealTimeEnd = value;
                    this.OnPropertyChanged();

                    if (LocalEnv.UserInfo.RoleType == RoleType.SELLER_ROLE)
                    {
                        OnSearchIntentionDataCommand();
                    }
                }
            }
        }

        private DateTime? dealTimeBegin_demand;
        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? DealTimeBegin_Demand
        {
            get { return dealTimeBegin_demand; }
            set
            {
                if ((dealTimeBegin_demand == null && value != null)
                    || (dealTimeBegin_demand != null && value == null)
                    || (dealTimeBegin_demand != null && value != null && dealTimeBegin_demand?.ToString("yyyy-MM-dd") != value?.ToString("yyyy-MM-dd")))
                {
                    dealTimeBegin_demand = value;
                    this.OnPropertyChanged();
                    CheckTimeLimitSelected();
                    OnSearchDemandDataCommand();

                    if (!isProgSetTime)
                    {
                        WriteDBLogHelper.Instance.GenRecord("personal", "017", "开始日期");
                    }
                }
            }
        }

        private DateTime? dealTimeEnd_demand;
        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? DealTimeEnd_Demand
        {
            get { return dealTimeEnd_demand; }
            set
            {
                if ((dealTimeEnd_demand == null && value != null)
                    || (dealTimeEnd_demand != null && value == null)
                    || (dealTimeEnd_demand != null && value != null && dealTimeEnd_demand?.ToString("yyyy-MM-dd") != value?.ToString("yyyy-MM-dd")))
                {
                    dealTimeEnd_demand = value;
                    this.OnPropertyChanged();
                    CheckTimeLimitSelected();
                    OnSearchDemandDataCommand();

                    if (!isProgSetTime)
                    {
                        WriteDBLogHelper.Instance.GenRecord("personal", "018", "结束日期");
                    }
                }
            }
        }

        private void CheckTimeLimitSelected()
        {
            switch (TimeLimitSelectedIndex.ToString())
            {
                case "0":
                case "today":
                    if (!isProgSetTime)
                    {
                        if (this.DealTimeBegin_Demand != DateTime.Now || this.DealTimeEnd_Demand != DateTime.Now)
                        {
                            TimeLimitSelectedIndex = -1;
                        }
                    }
                    break;
                case "1":
                case "yesterday":
                    if (!isProgSetTime)
                    {
                        if (this.DealTimeBegin_Demand != DateTime.Now.AddDays(-1) || this.DealTimeEnd_Demand != DateTime.Now.AddDays(-1))
                        {
                            TimeLimitSelectedIndex = -1;
                        }
                    }
                    break;
                case "2":
                case "last7days":
                    if (!isProgSetTime)
                    {
                        if (this.DealTimeBegin_Demand != DateTime.Now.AddDays(-6) || this.DealTimeEnd_Demand != DateTime.Now)
                        {
                            TimeLimitSelectedIndex = -1;
                        }
                    }
                    break;
                case "3":
                case "last30days":
                    if (!isProgSetTime)
                    {
                        if (this.DealTimeBegin_Demand != DateTime.Now.AddDays(-29) || this.DealTimeEnd_Demand != DateTime.Now)
                        {
                            TimeLimitSelectedIndex = -1;
                        }
                    }
                    break;
                case "4":
                case "unlimited":
                    //如果不限标志亮则灰掉
                    if (!isProgSetTime)
                    {
                        if (this.DealTimeBegin_Demand != null || this.DealTimeEnd_Demand != null)
                        {
                            TimeLimitSelectedIndex = -1;
                        }
                    }
                    break;
            }
        }

        private int timelimitselectedindex = 0;
        private bool isProgSetTime = false;
        public int TimeLimitSelectedIndex
        {
            get { return timelimitselectedindex; }
            set
            {
                if (timelimitselectedindex != value)
                {
                    isProgSetTime = true;
                    switch (value.ToString())
                    {
                        case "0":
                        case "today":
                            this.DealTimeBegin_Demand = DateTime.Now;
                            this.DealTimeEnd_Demand = DateTime.Now;
                            WriteDBLogHelper.Instance.GenRecord("personal", "013", "今天");
                            break;
                        case "1":
                        case "yesterday":
                            this.DealTimeBegin_Demand = DateTime.Now.AddDays(-1);
                            this.DealTimeEnd_Demand = DateTime.Now.AddDays(-1);
                            WriteDBLogHelper.Instance.GenRecord("personal", "014", "昨天");
                            break;
                        case "2":
                        case "last7days":
                            this.DealTimeBegin_Demand = DateTime.Now.AddDays(-6);
                            this.DealTimeEnd_Demand = DateTime.Now;
                            WriteDBLogHelper.Instance.GenRecord("personal", "015", "最近7天");
                            break;
                        case "3":
                        case "last30days":
                            this.DealTimeBegin_Demand = DateTime.Now.AddDays(-29);
                            this.DealTimeEnd_Demand = DateTime.Now;
                            WriteDBLogHelper.Instance.GenRecord("personal", "016", "最近30天");
                            break;
                        case "4":
                        case "unlimited":
                            this.DealTimeBegin_Demand = null;
                            this.DealTimeEnd_Demand = null;
                            break;
                    }

                    timelimitselectedindex = value;
                    isProgSetTime = false;

                    this.OnPropertyChanged();

                }
            }
        }


        #endregion

        #region 标签

        // 急
        private string _HurryFlag;

        public string HurryFlag
        {
            get
            {
                return _HurryFlag;
            }
            set
            {
                _HurryFlag = value;
                this.OnPropertyChanged();
            }
        }

        // 荐
        private string _SuggestFlag;

        public string SuggestFlag
        {
            get
            {
                return _SuggestFlag;
            }
            set
            {
                _SuggestFlag = value;
                this.OnPropertyChanged();
            }
        }

        // 火
        private string _FireFlag;

        public string FireFlag
        {
            get
            {
                return _FireFlag;
            }
            set
            {
                _FireFlag = value;
                this.OnPropertyChanged();
            }
        }

        // 底价
        private string _FloorPrice;

        public string FloorPrice
        {
            get
            {
                return _FloorPrice;
            }
            set
            {
                _FloorPrice = value;
                this.OnPropertyChanged();
            }
        }

        // oco, 打包
        private string _OCOFlag;

        public string OCOFlag
        {
            get
            {
                return _OCOFlag;
            }
            set
            {
                _OCOFlag = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region 更新成交

        private void OnLoadtMarketExecutionsStatus(bool isok)
        {
            if (isok)
            {
                LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;
                //获取最优价
                foreach (string securityid in _Cache_InnerOrders_Dic.Keys.ToList())
                {
                    LastExecutionProcesser.Processer.AddGetLastExecutionQueue(securityid);
                }
            }
        }

        private void Processer_OnLastExecutions_BDB(ExecutionReport obj)
        {
            if (obj == null)
                return;

            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    if (_Cache_InnerOrders_Dic.ContainsKey(obj.SecurityID) || _demand_Dic.ContainsKey(obj.SecurityID))
                    {
                        MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem_BDB(obj.SecurityID);

                        if (lastExeItem == null)
                            return;
                        // 市场最新成交
                        if (_Cache_InnerOrders_Dic.ContainsKey(obj.SecurityID))
                        {
                            foreach (IntentionQuoteItem item in _Cache_InnerOrders_Dic[obj.SecurityID].Values)
                            {
                                item.LastMarketPx = lastExeItem.ExecutePricePercent;
                            }
                        }

                        //DataRow[] findRows = DemandDT.Select($"{SECURITYID_FIELD}='{obj.SecurityID}'");
                        //foreach (DataRowView drv in DemandDv_Filter)
                        //{
                        //    UpdateLastExecutions(drv[ITEM_FIELD] as IntentionQuoteItem);
                        //}
                        if (_demand_Dic.ContainsKey(obj.SecurityID))
                        {
                            List<DataRow> list = _demand_Dic[obj.SecurityID];
                            if (list != null && list.Count > 0)
                            {
                                foreach (DataRow dr in list)
                                {
                                    (dr[ITEM_FIELD] as IntentionQuoteItem).LastMarketPx = lastExeItem.ExecutePricePercent;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        private void Processer_OnLastExecutions(ExecutionReport obj)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    if (_Cache_InnerOrders_Dic.ContainsKey(obj.SecurityID) || _demand_Dic.ContainsKey(obj.SecurityID))
                    {
                        decimal? lastMarketPx = null;
                        ExecutionReport lastExeItem = LastExecutionProcesser.Processer.GetExecutionReportDic(obj.SecurityID);

                        if (lastExeItem != null)
                        {
                            if (LocalEnv.ConfInfo.IsPAYL)
                            {
                                lastMarketPx = lastExeItem.LastPx.ConvertToDecimal(); // 成交价
                            }
                            else
                            {
                                if (lastExeItem.SecurityID.EndsWith(".SH") || lastExeItem.SecurityID.EndsWith(".SZ"))
                                {
                                    lastMarketPx = lastExeItem.NetPrice.ConvertToDecimal(); // 成交价
                                }
                                else
                                {
                                    lastMarketPx = lastExeItem.Yield.ConvertToDecimal(); // 成交价
                                }
                            }
                        }


                        // 市场最新成交
                        if (_Cache_InnerOrders_Dic.ContainsKey(obj.SecurityID))
                        {
                            foreach (IntentionQuoteItem item in _Cache_InnerOrders_Dic[obj.SecurityID].Values)
                            {
                                item.LastMarketPx = lastMarketPx;
                            }
                        }

                        //DataRow[] findRows = DemandDT.Select($"{SECURITYID_FIELD}='{obj.SecurityID}'");
                        //foreach (DataRowView drv in DemandDv_Filter)
                        //{
                        //    UpdateLastExecutions(drv[ITEM_FIELD] as IntentionQuoteItem);
                        //}
                        if (_demand_Dic.ContainsKey(obj.SecurityID))
                        {
                            List<DataRow> list = _demand_Dic[obj.SecurityID];
                            if (list != null && list.Count > 0)
                            {
                                foreach (DataRow dr in list)
                                {
                                    (dr[ITEM_FIELD] as IntentionQuoteItem).LastMarketPx = lastMarketPx;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        protected void UpdateLastExecutions_BDB(IntentionQuoteItem item)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    if (item != null)
                    {
                        MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem_BDB(item.SecurityId);

                        if (lastExeItem != null && lastExeItem != null)
                        {
                            item.LastMarketPx = lastExeItem.ExecutePricePercent;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        protected void UpdateLastExecutions(IntentionQuoteItem item)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    if (item != null)
                    {
                        ExecutionReport lastExeItem = LastExecutionProcesser.Processer.GetExecutionReportDic(item.SecurityId);

                        if (lastExeItem != null)
                        {
                            if (LocalEnv.ConfInfo.IsPAYL)
                            {
                                item.LastMarketPx = lastExeItem.LastPx.ConvertToDecimal(); // 成交价
                            }
                            else
                            {
                                if (lastExeItem.SecurityID.EndsWith(".SH") || lastExeItem.SecurityID.EndsWith(".SZ"))
                                {
                                    item.LastMarketPx = lastExeItem.NetPrice.ConvertToDecimal(); // 成交价
                                }
                                else
                                {
                                    item.LastMarketPx = lastExeItem.Yield.ConvertToDecimal(); // 成交价
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }
        #endregion

        private void FilterInquiryData()
        {
            this.InquiryViewSource.View.Filter = x =>
            {
                IntentionMarketItem item = (IntentionMarketItem)x;

                string key = string.Empty;
                if (this.SelectedItem_Working != null)
                {
                    key = this.SelectedItem_Working.IntentionId;
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        key = this.SelectedItem_Working.GroupKey;
                    }

                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(this.SelectedItem_Working.SecurityId);
                    if (bondsInfoPo != null)
                    {
                        // 含权
                        if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                        ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                        (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                        {

                            if (PriceTypeEmptyDic.Count == 2)
                            {
                                PriceTypeEmptyDic.Add(new KeyValuePair<string, string>("7", "行权"));
                            }
                            this.PriceType_Edit = string.IsNullOrEmpty(this.SelectedItem_Working.PriceType) ? "7" : this.SelectedItem_Working.PriceType;
                        }
                        else
                        {
                            this.PriceType_Edit = string.IsNullOrEmpty(this.SelectedItem_Working.PriceType) ? "3" : this.SelectedItem_Working.PriceType;
                            if (PriceTypeEmptyDic.Count == 3)
                            {
                                PriceTypeEmptyDic.RemoveAt(2);
                            }
                        }
                    }

                    if (!Price_Edit.HasValue && this.SelectedItem_Working.Price.HasValue)
                    {
                        if (this.SelectedItem_Working.PriceType == "1")
                        {
                            Price_Edit = this.SelectedItem_Working.CleanPrice;
                        }
                        else if (this.SelectedItem_Working.PriceType == "2")
                        {
                            Price_Edit = this.SelectedItem_Working.FullPrice;
                        }
                        else
                        {
                            Price_Edit = this.SelectedItem_Working.Price;
                        }
                    }

                    if (!Qty_Edit.HasValue && this.SelectedItem_Working.Qty.HasValue)
                    {
                        Qty_Edit = this.SelectedItem_Working.Qty;
                    }
                }

                if (item.Type == "G")
                {
                    string groupkey = item.IntentionId;
                    if (_Cache_InquiryQuote_Dic.ContainsKey(groupkey))
                    {
                        foreach (IntentionMarketItem subItem in _Cache_InquiryQuote_Dic[groupkey].Values)
                        {
                            if (IsDisplay_Inquiry(subItem)) return true;
                        }
                    }
                    return false;
                }
                else
                {
                    return IsDisplay_Inquiry(item);
                }
            };
        }

        private bool IsDisplay_Inquiry(IntentionMarketItem item)
        {
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
            {
                if (this.SelectedItems_Working != null && this.SelectedItems_Working.Count > 0 && !string.IsNullOrEmpty(item.IntentionId))
                {
                    if (!this.workingSelectedItems.Any(x => x.StrategyOrderId == item.IntentionId))
                    {
                        return false;
                    }
                }

                if (myTradersList != null && myTradersList.Count > 0 && !myTradersList.Contains(item.Trader_Buyer))
                    return false;
            }
            else
            {
                string key = string.Empty;
                if (this.SelectedItem_Working != null)
                {
                    key = this.SelectedItem_Working.IntentionId;
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        key = this.SelectedItem_Working.GroupKey;
                    }
                }

                if (this.SelectedItem_Working == null || (this.SelectedItem_Working != null && item.IntentionId != key))
                {
                    return false;
                }
            }

            if (_inquiryStatusList != null && _inquiryStatusList.Count > 0 && !_inquiryStatusList.Contains(item.Status))
            {
                return false;
            }

            return true;
        }

        List<string> _inquiryStatusList = null;
        internal void FilterInquiryStatus(List<string> statusList)
        {
            _inquiryStatusList = statusList;
            this.InquiryViewSource.View?.Refresh();
        }

        protected List<string> myTradersList = new List<string>();
        internal void ProcessFilterMyTraders(string[] arr)
        {
            myTradersList.Clear();

            if (arr != null)
            {
                myTradersList.AddRange(arr);
            }

            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            this.WorkingViewSource.View?.Refresh();
            this.InquiryViewSource.View?.Refresh();
        }

        private TableIntentionInquiry ConvertToInquiryData(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            TableIntentionInquiry newInquiry = new TableIntentionInquiry();
            newInquiry.id = $"{obj.intentionId}_内部";
            newInquiry.biztypeid = LocalEnv.UserInfo.Termids;
            newInquiry.intentionId = obj.intentionId;
            newInquiry.securityId = obj.securityId;
            newInquiry.side = obj.side;
            newInquiry.price = obj.price;
            newInquiry.qty = obj.qty;
            newInquiry.userName = obj.userName;
            newInquiry.settlType = obj.settlType;
            newInquiry.status = obj.status == "1" ? "1" : "0";
            newInquiry.createTime = obj.createTime;
            newInquiry.updateTime = obj.updateTime;
            newInquiry.receiverName = "内部";

            return newInquiry;
        }

        #region 询价批量修改报价和报量
        private decimal? _price_Edit;

        public decimal? Price_Edit
        {
            get { return _price_Edit; }
            set
            {
                _price_Edit = value;
                this.OnPropertyChanged();
            }
        }


        private decimal? _qty_Edit;

        public decimal? Qty_Edit
        {
            get { return _qty_Edit; }
            set
            {
                _qty_Edit = value;
                this.OnPropertyChanged();
            }
        }

        #region 清算速度
        private ObservableCollection<string> _SettlTypeList = new ObservableCollection<string>();

        public ObservableCollection<string> SettlTypeList
        {
            get
            {
                if (_SettlTypeList.Count == 0)
                {
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        _SettlTypeList.Add("不限");
                        _SettlTypeList.Add("T+1");
                        _SettlTypeList.Add("明+0");
                    }
                    else
                    {
                        _SettlTypeList.Add("+1");
                        _SettlTypeList.Add("今+1");
                        _SettlTypeList.Add("明+0");
                    }
                }
                return _SettlTypeList;
            }
        }
        #endregion

        #region 报价标准
        /// <summary>
        /// 用于需求工作台行内价格标准的筛选项
        /// </summary>
        private ObservableCollection<KeyValuePair<string, string>> _priceType_table_Dic = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> PriceTypeTableDic
        {
            get
            {
                if (_priceType_table_Dic.Count == 0)
                {
                    _priceType_table_Dic.Add(new KeyValuePair<string, string>("1", "净价"));//["1"] = "净价";
                    //_priceType_table_Dic.Add(new KeyValuePair<string, string>("2", "全价"));//["2"] = "全价";
                    _priceType_table_Dic.Add(new KeyValuePair<string, string>("3", "到期"));//["3"] = "到期收益率";
                    _priceType_table_Dic.Add(new KeyValuePair<string, string>("7", "行权"));//["7"] = "行权收益率";
                }
                return _priceType_table_Dic;
            }
        }

        private ObservableCollection<KeyValuePair<string, string>> _priceType_empty_Dic = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> PriceTypeEmptyDic
        {
            get
            {
                if (_priceType_empty_Dic.Count == 0)
                {
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("1", "净价"));//["1"] = "净价";
                    //_priceType_empty_Dic.Add(new KeyValuePair<string, string>("2", "全价"));//["2"] = "全价";
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("3", "到期"));//["3"] = "到期收益率";
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("7", "行权"));//["7"] = "行权收益率";
                }
                return _priceType_empty_Dic;
            }
        }

        public string _PriceType_Edit = "3";

        public string PriceType_Edit
        {
            get { return _PriceType_Edit; }
            set
            {
                _PriceType_Edit = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        internal void BatchUpdateInquiry(string groupKey)
        {
            try
            {
                if (string.IsNullOrEmpty(groupKey) 
                    || !_Cache_InquiryQuote_Dic.ContainsKey(groupKey)
                    || _Cache_InquiryQuote_Dic[groupKey].Count == 0)
                {
                    return;
                }

                List<IntentionMarketItem> updateList = new List<IntentionMarketItem>();
                foreach (IntentionMarketItem inquiryItem in _Cache_InquiryQuote_Dic[groupKey].Values)
                {
                    if (inquiryItem.Status == "1" && inquiryItem.SendType != 3)
                    {
                        updateList.Add(inquiryItem);
                    }
                }

                if (updateList == null || updateList.Count == 0)
                {
                    return;
                }


                int failCount_local = 0;
                List<TableIntentionInquiry> refList = new List<TableIntentionInquiry>();
                List<TableIntentionInquiry> inquiryList = new List<TableIntentionInquiry>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                string key = string.Empty;
                foreach (IntentionMarketItem item in updateList)
                {
                    TableIntentionInquiry oldInquiry = IntentionOldHandler.Instance.GetInquiryById(item.IntentionId, item.Id);

                    TableIntentionInquiry updateData = new TableIntentionInquiry();
                    updateData.Clone(oldInquiry);
                    updateData.status = null;
                    updateData.price = this.Price_Edit.HasValue ? this.Price_Edit?.ToString("0.####").ConvertToDecimal() : oldInquiry.price;
                    updateData.qty = this.Qty_Edit.HasValue ? this.Qty_Edit?.ToString("0").ConvertToDecimal() : oldInquiry.qty;
                    InquiryInfo3 info3 = new InquiryInfo3();
                    info3.PriceType = this.PriceType_Edit;
                    updateData.info3 = JsonConvert.SerializeObject(info3);
                    inquiryList.Add(updateData);
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                    IntentionInquiryRequest request = new IntentionInquiryRequest();
                    request.intentionId = key;
                    request.inquiryKey = key;
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.biztypeid = LocalEnv.UserInfo.Termids;
                    request.action = "1";
                    request.dataList = inquiryList;
                    //TableOldHandler.Instance.IntentionInquiry(request);

                    //orderDic[item.IntentionId].Request = request;
                    //RemenberInstAndTrade(item);

                    GetErrorInfo_Inquiry(request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                Dictionary<string, IntentionQuoteItem> tempDic = new Dictionary<string, IntentionQuoteItem>();
                                foreach (TableIntentionInquiry inquiry in inquiryList)
                                {
                                    tempDic[Guid.NewGuid().ToString()] = null;
                                }
                                BatchResultWindow batchResultWindow = new BatchResultWindow(tempDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                //batchResultWindow.Owner = _owner;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));

                    //System.Threading.ThreadPool.QueueUserWorkItem(x =>
                    //{
                    //    Thread.Sleep(2000);
                    //    Query();
                    //});
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("BatchUpdateInquiry", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }
        #endregion

        internal IntentionMarketItem GetIntentionMarketItemById(string id)
        {
            return InquiryQuoteInfo.FirstOrDefault(x => x.Id == id);
        }

        internal void EditPrice_Custom(decimal? price, string priceType, Action ATEFunc = null)
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                return;
            }

            IEnumerable<IntentionQuoteItem> list = this.SelectedItems_Working.Cast<IntentionQuoteItem>();

            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            //newIntentionDic.Clear();

            Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in list)
                {
                    if (drv.Type == "INTENTION_GROUP")
                    {
                        continue;
                    }

                    string savePriceType = priceType;
                    if (string.IsNullOrEmpty(savePriceType))
                    {
                        BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(drv.SecurityId);
                        if (bondsInfoPo != null)
                        {
                            // 含权
                            if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                            {
                                savePriceType = string.IsNullOrEmpty(priceType) ? "7" : priceType;
                            }
                            else
                            {
                                savePriceType = string.IsNullOrEmpty(priceType) ? "3" : priceType;
                            }
                        }
                    }

                    if (drv.Status == "1")
                    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);

                        decimal? newPrice = null;
                        if (price.HasValue == false)
                        {
                            newPrice = null;
                        }
                        else
                        {
                            newPrice = price.Value;
                        }
                        if (newPrice != null && newPrice.HasValue)
                        {
                            CalResult calResult = Common.Utils.CommonUtils.CalBond_PriceType(newOrderSingle.SecurityID, newOrderSingle.SettlType, newPrice.ToString(), savePriceType);
                            if (calResult != null && calResult.Data != null)
                            {
                                if (savePriceType == "7")
                                {
                                    newOrderSingle.Price = (calResult.Data.YieldToExecution * 100).ToString();
                                }
                                else
                                {
                                    newOrderSingle.Price = (calResult.Data.Yield * 100).ToString();
                                }
                                newOrderSingle.NetPrice = calResult.Data.CleanPrice.ToString();
                                newOrderSingle.FullPrice = calResult.Data.FullPrice.ToString();
                            }
                            
                            newOrderSingle.PriceType = savePriceType;
                            if (priceType != "1" && priceType != "2")
                            {
                                TagData info2tag = null;
                                if (newOrderSingle != null && JsonSplit.IsJson(newOrderSingle.Info2))
                                {
                                    info2tag = JsonConvert.DeserializeObject<TagData>(newOrderSingle.Info2);
                                }
                                else
                                {
                                    info2tag = new TagData();
                                }
                                if (!string.IsNullOrEmpty(info2tag.CUSTOMER_PRICE))
                                {
                                    decimal customerPrice = info2tag.CUSTOMER_PRICE.ConvertToDecimal();
                                    if (newOrderSingle.Side == "1")
                                    {
                                        info2tag.CUSTOMER_SPREAD = ((newPrice - customerPrice) * 100).ToString();
                                    }
                                    else
                                    {
                                        info2tag.CUSTOMER_SPREAD = ((customerPrice - newPrice) * 100).ToString();
                                    }
                                }
                                newOrderSingle.Info2 = JsonConvert.SerializeObject(info2tag);
                            }

                            
                            
                            newOrderSingleDic[newOrderSingle.IntentionID] = newOrderSingle;
                        }
                        else
                        {
                            newOrderSingle.Price = null;
                            newOrderSingle.NetPrice = null;
                            newOrderSingle.FullPrice = null;
                        }
                    }
                    if (drv.Status == "0" || drv.Status == "80" || drv.Status == "81")
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        newIntentionOrder.securityid = drv.SecurityId;
                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            if (STC.Common.Utils.CommonUtils.IsInvestor())
                            {
                                newIntentionOrder.actionFlag = "0";
                            }
                            else
                            {
                                newIntentionOrder.actionFlag = "2";
                            }

                            newIntentionOrder.riskManualCk = "1";
                        }

                        if (price.HasValue == false)
                        {
                            newIntentionOrder.price = null;
                            newIntentionOrder.cleanPrice = null;
                            newIntentionOrder.fullPrice = null;
                        }
                        else
                        {
                            CalResult calResult = Common.Utils.CommonUtils.CalBond_PriceType(newIntentionOrder.securityid, newIntentionOrder.settlType, price?.ToString(), savePriceType);
                            if (calResult != null && calResult.Data != null)
                            {
                                if (savePriceType == "7")
                                {
                                    newIntentionOrder.price = calResult.Data.YieldToExecution * 100;
                                }
                                else
                                {
                                    newIntentionOrder.price = calResult.Data.Yield * 100;
                                }
                                newIntentionOrder.cleanPrice = calResult.Data.CleanPrice;
                                newIntentionOrder.fullPrice = calResult.Data.FullPrice;
                            }
                        }
                        newIntentionOrder.priceType = savePriceType;
                        //newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;
                        drv.UpdateIntentionRequest = newIntentionOrder;
                        drv.Request = null;
                        drv.InquiryRequest = null;
                        orderDic[newIntentionOrder.intentionId] = drv;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && orderDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                ATEFunc?.Invoke();
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = orderDic.Count + newOrderSingleDic.Count;
                    // 修改意向单
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        Sub_UpdateIntention(orderDic);
                    }
                    else
                    {
                        foreach (IntentionQuoteItem item in orderDic.Values)
                        {
                            PResult result = TableOldHandler.Instance.UpdateIntentionQuote(item.UpdateIntentionRequest);
                            if (result == null || !result.IsSuccess())
                            {
                                failCount++;
                            }
                        }
                    }

                    // 修改订单
                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        sendPricePo.HandlInst = "2";
                        RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    string text = string.Empty;
                    if (failCount > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑报价", text);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
                ATEFunc?.Invoke();
            });
        }

        internal void EditQty_Custom(decimal? qty, Action ATEFunc = null)
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                return;
            }

            IEnumerable<IntentionQuoteItem> list = this.SelectedItems_Working.Cast<IntentionQuoteItem>();

            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            //newIntentionDic.Clear();
            int matchExefail = 0;

            Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in list)
                {
                    if (drv.Type == "INTENTION_GROUP")
                    {
                        continue;
                    }

                    if (drv.Status == "1")
                    {
                        Common.Model.Fix.Message.NewOrderSingle newOrderSingle = GetNewOrderSingle(drv);

                        if (qty.HasValue == false || qty.Value == decimal.Zero)
                        {
                            newOrderSingle.OrderQty = null;
                        }
                        else
                        {
                            newOrderSingle.OrderQty = qty.Value.ToString();
                        }

                        //个需的量不能修改为小于已匹配成交量
                        if (drv.MatchedQty.HasValue && drv.MatchedQty.Value > qty)
                        {
                            if (this.SelectedItems_Working.Count == 1)
                            {
                                Common.View.Component.CustomMessageBox.Info("个需的量不能修改为小于已匹配成交量", "修改报量");
                            }
                            matchExefail++;
                            continue;
                        }

                        newOrderSingleDic[newOrderSingle.IntentionID] = newOrderSingle;
                    }
                    else if (drv.Status == "0" || drv.Status == "80" || drv.Status == "81")
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        newIntentionOrder.securityid = drv.SecurityId;

                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            if (STC.Common.Utils.CommonUtils.IsInvestor())
                            {
                                newIntentionOrder.actionFlag = "0";
                            }
                            else
                            {
                                newIntentionOrder.actionFlag = "2";
                            }

                            newIntentionOrder.riskManualCk = "1";
                        }

                        if (qty.HasValue == false || qty.Value == decimal.Zero)
                        {
                            newIntentionOrder.qty = null;
                        }
                        else
                        {
                            newIntentionOrder.qty = qty.Value;
                        }

                        //个需的量不能修改为小于已匹配成交量
                        if (drv.MatchedQty.HasValue && drv.MatchedQty.Value > qty)
                        {
                            continue;
                        }

                        //newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;
                        drv.UpdateIntentionRequest = newIntentionOrder;
                        drv.Request = null;
                        drv.InquiryRequest = null;
                        orderDic[newIntentionOrder.intentionId] = drv;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && orderDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                ATEFunc?.Invoke();
                return;
            }


            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = orderDic.Count + newOrderSingleDic.Count + matchExefail;
                    // 修改意向单
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        Sub_UpdateIntention(orderDic);
                    }
                    else
                    {
                        foreach (IntentionQuoteItem item in orderDic.Values)
                        {
                            PResult result = TableOldHandler.Instance.UpdateIntentionQuote(item.UpdateIntentionRequest);
                            if (result == null || !result.IsSuccess())
                            {
                                failCount++;
                            }
                        }
                    }

                    // 修改订单
                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    string text = string.Empty;
                    if (failCount + matchExefail > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount + matchExefail} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑报量", text);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
                ATEFunc?.Invoke();
            });
        }
    }

    public class MyCustomComparer : IComparer
    {
        private readonly string _sortPropertyName;
        private readonly ListSortDirection? _direction;
        private ConcurrentDictionary<string, object> _dic;
        private static readonly Regex _numberRegex = new Regex(
                @"(\d+)|([^\d]+)",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );

        public MyCustomComparer(string column, ListSortDirection? direction, ConcurrentDictionary<string, object> summaryDic)
        {
            _sortPropertyName = column;
            _direction = direction;
            _dic = summaryDic;
        }

        public int Compare(object x, object y)
        {
            // 处理null情况
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int result = 0;
            IntentionQuoteItem xItem = (IntentionQuoteItem)x;
            IntentionQuoteItem yItem = (IntentionQuoteItem)y;
            try
            {
                string xSortKey = xItem.SortKey;
                string ySortKey = yItem.SortKey;
                string xType = xItem.Type;
                string yType = yItem.Type;

                PropertyInfo propertyInfo = typeof(IntentionQuoteItem).GetProperty(_sortPropertyName);
                if (propertyInfo != null)
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {

                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {

                    }

                    string xValue = propertyInfo.GetValue(x) == null ? null : propertyInfo.GetValue(x).ToString();
                    string yValue = propertyInfo.GetValue(y) == null ? null : propertyInfo.GetValue(y).ToString();

                    if (!string.IsNullOrEmpty(xSortKey) && !string.IsNullOrEmpty(ySortKey) && xSortKey == ySortKey)
                    {
                        result = -xType.CompareTo(yType);
                        if (result == 0)
                        {
                            result = CommonCompare(xValue, yValue);
                        }
                        else
                        {
                            return result;
                        }
                    }
                    else
                    {
                        string newValue1 = xValue;
                        if (_dic != null && !string.IsNullOrEmpty(xSortKey) && _dic.ContainsKey(xSortKey))
                        {
                            newValue1 = _dic[xSortKey] == null ? null : _dic[xSortKey].ToString();
                        }
                        string newValue2 = yValue;
                        if (_dic != null && !string.IsNullOrEmpty(ySortKey) && _dic.ContainsKey(ySortKey))
                        {
                            newValue2 = _dic[ySortKey] == null ? null : _dic[ySortKey].ToString();
                        }
                        result = CommonCompare(newValue1, newValue2);
                    }
                }

               
            }
            catch(Exception e)
            {
                LogHandler.Info($"xItem:{xItem.ToString()}");
                
                LogHandler.Error(e);
            }
            if (_direction == ListSortDirection.Descending)
            {
                return -result;
            }
            else
            {
                return result;
            }
        }

        public int CommonCompare(string x, string y)
        {
            // 处理null情况
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var xMatches = _numberRegex.Matches(x);
            var yMatches = _numberRegex.Matches(y);
            int minLength = Math.Min(xMatches.Count, yMatches.Count);

            for (int i = 0; i < minLength; i++)
            {
                var xMatch = xMatches[i];
                var yMatch = yMatches[i];
                int result;

                // 比较数字段
                int xNum;
                int yNum;
                if (int.TryParse(xMatch.Value, out xNum) &&
                    int.TryParse(yMatch.Value, out yNum))
                {
                    result = xNum.CompareTo(yNum);
                }
                // 比较文本段
                else
                {
                    result = CultureInfo.CurrentCulture.CompareInfo.Compare(
                        xMatch.Value, yMatch.Value,
                        CompareOptions.IgnoreCase
                    );
                }

                if (result != 0) return result;
            }

            // 较短字符串排在前面
            return xMatches.Count.CompareTo(yMatches.Count);
        }


    }
}
