using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Data;
using STC.Frame.Common;
using STC.IRS.Plugin.View;
using STC.IRS.Plugin.View.Component;
using STC.IRS.Plugin.ViewModel.Comparer;
using STC.IRS.Plugin.ViewModel.Data;
using STC.InnerBoard.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.IRS.Plugin.ViewModel
{
    public class InnerBoardViewModel : ViewModelBase
    {
        bool isPermission_WorkTable = AdminHandler.Instance.CheckPermisionById("4000", "4008");

        public Visibility RecognitionBtnVisibility
        {
            get
            {
                return isPermission_WorkTable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

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
                this.ViewSource.View?.Refresh();
            }
        }

        private string _best_DisplayMode;

        public string Best_DisplayMode
        {
            get
            {
                return _best_DisplayMode;
            }
            set
            {
                _best_DisplayMode = value;
                this.OnPropertyChanged();
                this.ViewSource_Best.View?.Refresh();
            }
        }

        private string _SearchText;

        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                this.OnPropertyChanged();
                this.ViewSource.View?.Refresh();
            }
        }

        private string _best_SearchText;

        public string Best_SearchText
        {
            get
            {
                return _best_SearchText;
            }
            set
            {
                _best_SearchText = value;
                this.OnPropertyChanged();
                this.ViewSource_Best.View?.Refresh();
            }
        }



        private bool _autoEnable = true;

        public bool AutoEnable
        {
            get { return _autoEnable; }
            set 
            { 
                _autoEnable = value;
                this.OnPropertyChanged();

                if (auteRefreshTimer != null)
                {
                    auteRefreshTimer.Enabled = _autoEnable;
                }
            }
        }

        private InnerOrderData _selectedItem_MyInnerOrders;

        public InnerOrderData SelectedItem_MyInnerOrders
        {
            get
            {
                return _selectedItem_MyInnerOrders;
            }
            set
            {
                _selectedItem_MyInnerOrders = value;
                this.OnPropertyChanged();
            }
        }

        private InnerOrderData _selectedItem_MyInnerOrders_Best;

        public InnerOrderData SelectedItem_MyInnerOrders_Best
        {
            get
            {
                return _selectedItem_MyInnerOrders_Best;
            }
            set
            {
                _selectedItem_MyInnerOrders_Best = value;
                this.OnPropertyChanged();
            }
        }

        private ConcurrentDictionary<string, CCDCValuaModel> _symbolCountDic = new ConcurrentDictionary<string, CCDCValuaModel>();

        private ObservableCollection<FilterListBoxItemData> _interestRateDebtList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "利率债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "国债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "央票", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "地方政府债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "金融债", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> InterestRateDebtList { get { return _interestRateDebtList; } }

        private ObservableCollection<FilterListBoxItemData> _creditBondList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "信用债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "短融", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "中票", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "企业债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "公司债", itemIsSelected = false, Itemmargin="0 0 50 0"},
            new FilterListBoxItemData() {itemText = "PPN", itemIsSelected = false, Itemmargin="50 0 0 0"},
            new FilterListBoxItemData() {itemText = "NCD", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "ABS", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "CRM", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> CreditBondList { get { return _creditBondList; } }

        private ObservableCollection<FilterListBoxItemData> _remaingingTimeList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "0~1Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "1~3Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "3~5Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "5~7Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "7~10Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = ">10Y", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RemaingingTimeList { get { return _remaingingTimeList; } }

        private ObservableCollection<FilterListBoxItemData> _ratingList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "AAA+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AAA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA-", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "A+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RatingList { get { return _ratingList; } }

        //private ObservableCollection<FilterListBoxItemData> _roleList = new ObservableCollection<FilterListBoxItemData>();

        //public ObservableCollection<FilterListBoxItemData> RoleList { get { return _roleList; } }

        private ObservableCollection<FilterListBoxItemData> _interestRateDebtList_Best = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "利率债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "国债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "央票", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "地方政府债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "金融债", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> InterestRateDebtList_Best { get { return _interestRateDebtList_Best; } }

        private ObservableCollection<FilterListBoxItemData> _creditBondList_Best = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "信用债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "短融", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "中票", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "企业债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "公司债", itemIsSelected = false, Itemmargin="0 0 50 0"},
            new FilterListBoxItemData() {itemText = "PPN", itemIsSelected = false, Itemmargin="50 0 0 0"},
            new FilterListBoxItemData() {itemText = "NCD", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "ABS", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "CRM", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> CreditBondList_Best { get { return _creditBondList_Best; } }

        private ObservableCollection<FilterListBoxItemData> _remaingingTimeList_Best = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "0~1Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "1~3Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "3~5Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "5~7Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "7~10Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = ">10Y", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RemaingingTimeList_Best { get { return _remaingingTimeList_Best; } }

        private ObservableCollection<FilterListBoxItemData> _ratingList_Best = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "AAA+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AAA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA-", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "A+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RatingList_Best { get { return _ratingList_Best; } }

        ////**************** 逐笔 **********************
        internal object _dataLock = new object();

        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        private CustomCollection<InnerOrderData> _innerOrders = new CustomCollection<InnerOrderData>();

        public CustomCollection<InnerOrderData> InnerOrders
        {
            get { return _innerOrders; }
            set { _innerOrders = value; }
        }
        //********************************************

        //**************** 最优 **********************
        internal object _dataLock_best = new object();
        private CollectionViewSource _viewSource_best = new CollectionViewSource();

        public CollectionViewSource ViewSource_Best
        {
            get { return _viewSource_best; }
            set { _viewSource_best = value; }
        }

        private CustomCollection<InnerOrderData> _innerOrders_best = new CustomCollection<InnerOrderData>();
        public CustomCollection<InnerOrderData> InnerOrders_Best
        {
            get { return _innerOrders_best; }
            set { _innerOrders_best = value; }
        }
        //********************************************

        public ICommand RefreshCommand { get; set; }

        private Timer auteRefreshTimer;

        private readonly string _ModelId = "5000";
        private readonly string _Code = "5002";
        private bool isPermission = false; //查看机构内部权限

        private static string INTENTION_FLAG = "(需求)";

        //***************浮板报价************************
        private InnerDepthPopupViewModel _innerDepthPopupViewModel;

        public InnerDepthPopupViewModel InnerDepthPopupViewModel
        {
            get 
            {
                if(_innerDepthPopupViewModel == null)
                {
                    _innerDepthPopupViewModel = new InnerDepthPopupViewModel();
                }
                return _innerDepthPopupViewModel;
            }
            set { _innerDepthPopupViewModel = value; }
        }
        //********************************************

        private object _dataLock_tag = new object();
        private CollectionViewSource _viewSource_tag = new CollectionViewSource();
        public CollectionViewSource ViewSource_Tag
        {
            get { return _viewSource_tag; }
            set { _viewSource_tag = value; }
        }

        private FilterGroupItem _selectedItem_tag;

        public FilterGroupItem SelectedItem_Tag
        {
            get { return _selectedItem_tag; }
            set
            {
                if (_selectedItem_tag != value)
                {
                    _selectedItem_tag = value;

                    if (value != null)
                    {
                        this.FocusTags = _selectedItem_tag.FocusTags;

                        this.StartPrice = _selectedItem_tag.StartPrice;
                        this.EndPrice = _selectedItem_tag.EndPrice;
                        this.IsPrice = _selectedItem_tag.IsPrice;
                    }
                    else
                    {
                        //this.FocusTags = InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS;
                    }

                    this.OnPropertyChanged();
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
                this.ViewSource?.View.Refresh();
            }
        }

        private string _startTermDY;

        public string StartTermDY
        {
            get { return _startTermDY; }
            set
            {
                _startTermDY = value;
                this.OnPropertyChanged();
            }
        }

        private string _endTermDY;

        public string EndTermDY
        {
            get { return _endTermDY; }
            set
            {
                _endTermDY = value;
                this.OnPropertyChanged();
                this.ViewSource.View?.Refresh();
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

            }
        }

        private string _focusTags;

        public string FocusTags
        {
            get { return _focusTags; }
            set {
                if (_focusTags != value)
                {
                    _focusTags = value;

                    try
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            _focusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);
                        }
                        else
                        {
                            _focusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS);
                        }
                    }
                    catch (Exception)
                    {
                        _focusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS);
                    }

                    if (this.SelectedItem_Tag != null && this.SelectedItem_Tag.FocusTags != value)
                    {
                        this.SelectedItem_Tag = null;
                    }

                    this.OnPropertyChanged();
                    this.ViewSource.View?.Refresh();
                }
            }
        }

        private Dictionary<string, List<string>> _focusTagDic;

        private bool _isMe;

        public bool IsMe
        {
            get { return _isMe; }
            set {
                _isMe = value;
                this.OnPropertyChanged();
                this.ViewSource.View?.Refresh();
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
                this.ViewSource.View?.Refresh();

               

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
                this.ViewSource.View?.Refresh();

               

            }
        }

        private FilterGroupItem _selectedItem_tag_best;

        public FilterGroupItem SelectedItem_Tag_Best
        {
            get { return _selectedItem_tag_best; }
            set
            {
                if (_selectedItem_tag_best != value)
                {
                    _selectedItem_tag_best = value;
                    
                    if (value != null)
                    {
                        this.FocusTags_Best = _selectedItem_tag_best.FocusTags;

                        this.StartPrice_Best = _selectedItem_tag_best.StartPrice;
                        this.EndPrice_Best = _selectedItem_tag_best.EndPrice;
                        this.IsPrice_Best = _selectedItem_tag_best.IsPrice;
                    }
                    else
                    {
                        //this.FocusTags_Best = InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS;
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        private bool _IsAppTerm_Best;

        public bool IsAppTerm_Best
        {
            get { return _IsAppTerm_Best; }
            set
            {
                _IsAppTerm_Best = value;

                if (this.SelectedItem_Tag_Best != null && this.SelectedItem_Tag_Best.IsAppTerm != value)
                {
                    this.SelectedItem_Tag_Best = null;
                }

                this.OnPropertyChanged();
                this.ViewSource_Best?.View.Refresh();
            }
        }

        private string _startTermDY_Best;

        public string StartTermDY_Best
        {
            get { return _startTermDY_Best; }
            set
            {
                _startTermDY_Best = value;
                this.OnPropertyChanged();
            }
        }

        private string _endTermDY_Best;

        public string EndTermDY_Best
        {
            get { return _endTermDY_Best; }
            set
            {
                _endTermDY_Best = value;
                this.OnPropertyChanged();
                this.ViewSource_Best.View?.Refresh();
            }
        }

        private string _startCoupon_Best;

        public string StartCoupon_Best
        {
            get { return _startCoupon_Best; }
            set
            {
                _startCoupon_Best = value;
                this.OnPropertyChanged();
            }
        }

        private string _endCoupon_Best;

        public string EndCoupon_Best
        {
            get { return _endCoupon_Best; }
            set
            {
                _endCoupon_Best = value;
                this.OnPropertyChanged();
                this.ViewSource_Best.View?.Refresh();
            }
        }

        private string _startPrice_Best;

        public string StartPrice_Best
        {
            get { return _startPrice_Best; }
            set
            {
                _startPrice_Best = value;
                this.OnPropertyChanged();
            }
        }

        private string _endPrice_Best;

        public string EndPrice_Best
        {
            get { return _endPrice_Best; }
            set
            {
                _endPrice_Best = value;
                this.OnPropertyChanged();
                this.ViewSource_Best.View?.Refresh();
            }
        }

        private string _focusTags_Best;

        public string FocusTags_Best
        {
            get { return _focusTags_Best; }
            set
            {
                 if (_focusTags_Best != value)
                {
                    _focusTags_Best = value;
                    try
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            _focusTagDic_Best = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);
                        }
                        else
                        {
                            _focusTagDic_Best = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS);
                        }
                    }
                    catch (Exception)
                    {
                        _focusTagDic_Best = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS);
                    }

                    if (this.SelectedItem_Tag_Best != null && this.SelectedItem_Tag_Best.FocusTags != value)
                    {
                        this.SelectedItem_Tag_Best = null;
                    }

                    this.OnPropertyChanged();
                    this.ViewSource_Best.View?.Refresh();
                }
            }
        }
        private Dictionary<string, List<string>> _focusTagDic_Best;

        private bool _hasPrce_best;

        public bool HasPrice_Best
        {
            get { return _hasPrce_best; }
            set
            {
                if (_hasPrce_best != value)
                {
                    _hasPrce_best = value;
                    this.OnPropertyChanged();
                    this.ViewSource_Best.View?.Refresh();

                    IniFileHelper.WriteValue($"IRS_HasPrice-{LocalEnv.UserInfo.UserName}", "HasPrice", value ? "1" : "0");
                }
            }
        }

        private bool _isMe_best;

        public bool IsMe_Best
        {
            get { return _isMe_best; }
            set
            {
                _isMe_best = value;
                this.OnPropertyChanged();
                this.ViewSource_Best.View?.Refresh();
            }
        }

        private bool _isCoupon_best;

        public bool IsCoupon_Best
        {
            get { return _isCoupon_best; }
            set
            {
                _isCoupon_best = value;

                if (this.SelectedItem_Tag_Best != null && this.SelectedItem_Tag_Best.IsCoupon != value)
                {
                    this.SelectedItem_Tag_Best = null;
                }

                this.OnPropertyChanged();
                this.ViewSource.View?.Refresh();
            }
        }

        private bool _isPrice_best;

        public bool IsPrice_Best
        {
            get { return _isPrice_best; }
            set
            {
                _isPrice_best = value;

                if (this.SelectedItem_Tag_Best != null && this.SelectedItem_Tag_Best.IsPrice != value)
                {
                    this.SelectedItem_Tag_Best = null;
                }

                this.OnPropertyChanged();
                this.ViewSource.View?.Refresh();
            }
        }

        /// <summary>
        /// 1:做市 2:销交
        /// </summary>
        private string _innerType = "1";

        /// <summary>
        /// 我的报单个数
        /// </summary>
        private int _MyOrderCount = 0;

        public int MyOrderCount
        {
            get
            {
                return _MyOrderCount;
            }
            set
            {
                _MyOrderCount = value;
                HasMyOrders = value > 0;
                this.OnPropertyChanged();
            }
        }

        private bool _HasMyOrders = false;

        public bool HasMyOrders
        {
            get
            {
                return _HasMyOrders;
            }
            set
            {
                _HasMyOrders = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否显示内部看板报废区,1:显示
        /// </summary>
        private Visibility isshowihr = Visibility.Collapsed;
        public Visibility IsShowIHR
        {
            get { return isshowihr; }
            set
            {
                isshowihr = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否显示内部看板模糊需求,1:显示
        /// </summary>
        private Visibility isshowfuzzy = Visibility.Collapsed;
        public Visibility IsShowFuzzy
        {
            get { return isshowfuzzy; }
            set
            {
                isshowfuzzy = value;
                this.OnPropertyChanged();
            }
        }

        bool type1_flag;
        bool type2_flag;
        bool type3_flag;

        public InnerBoardViewModel()
        {
            if (IntentionIRSHandler.Instance.isSALER())
            {
                _innerType = "2";
            }

            string hasPriceStr = IniFileHelper.ReadValue($"IRS_HasPrice-{LocalEnv.UserInfo.UserName}", "HasPrice");
            if (string.IsNullOrEmpty(hasPriceStr) || hasPriceStr == "1")
            {
                HasPrice_Best = true;
            }

            type1_flag = AdminHandler.Instance.CheckPermission(MODULEID_CLIENT, TYPE1);
            type2_flag = AdminHandler.Instance.CheckPermission(MODULEID_CLIENT, TYPE2);
            type3_flag = AdminHandler.Instance.CheckPermission(MODULEID_CLIENT, TYPE3);

            if (RefreshUIThread == null)
            {
                RefreshUIThread = new System.Threading.Thread(ProcessInnerOrder_RealTime);
                RefreshUIThread.IsBackground = true;
                RefreshUIThread.Start();
            }

            BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);

            BaseHandler.Instance.Init(Instance_OnClear);

            //_lastUpdateTime = $"{DateTimeEx.ServerTime.ToString(dateDateFormatPattern)} 00:00:00.000";

            isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);

            //this.ViewSource.IsLiveFilteringRequested = true;
            //this.ViewSource.IsLiveSortingRequested = true;
            //this.ViewSource.Source = InnerOrders;
            //SetFilterDatas();
            //BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _dataLock);

            //this.ViewSource_Best.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
            //this.ViewSource_Best.IsLiveFilteringRequested = true;
            //this.ViewSource_Best.IsLiveSortingRequested = true;
            //this.ViewSource_Best.Source = InnerOrders_Best;
            //SetFilterDatas_Best();
            //BindingOperations.EnableCollectionSynchronization(this.ViewSource_Best.View, _dataLock_best);

            RefreshCommand = new ComRelayCommand(InitEmsOrder1);

            IntentionIRSHandler.Instance.OnQor += Instance_OnQor;
            LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;

            ViewSource_Tag.Source = IntentionOldHandler.Instance.GroupList;

            InitEmsOrder1();

            //作废区查询权限
            bool rootRight = AdminHandler.Instance.CheckPermisionById("5000", "5004");
            bool noRight = AdminHandler.Instance.CheckPermission("5004", "500401");
            IsShowIHR = rootRight ? noRight ? Visibility.Collapsed : Visibility.Visible : Visibility.Collapsed;

            //IRS中模糊需求先关掉
            //IsShowFuzzy = LocalEnv.ConfInfo.IsShowInnerFuzzyRequire == "1" ? Visibility.Visible : Visibility.Collapsed;
        }

        private string[] irsCodes = new string[] {
            "FR007_1M","FR007_3M","FR007_6M", "FR007_9M", "FR007_1Y", "FR007_2Y", "FR007_3Y", "FR007_4Y", "FR007_5Y", "FR007_7Y", "FR007_10Y",
            "Shibor3M_6M","Shibor3M_9M","Shibor3M_1Y", "Shibor3M_2Y", "Shibor3M_3Y", "Shibor3M_4Y", "Shibor3M_5Y","Shibor3M_7Y", "Shibor3M_10Y",
            "LPR1Y_6M","LPR1Y_9M","LPR1Y_1Y", "LPR1Y_2Y", "LPR1Y_3Y", "LPR1Y_4Y", "LPR1Y_5Y","LPR1Y_7Y", "LPR1Y_10Y",
            "LPR5Y_6M","LPR5Y_9M","LPR5Y_1Y", "LPR5Y_2Y", "LPR5Y_3Y", "LPR5Y_4Y", "LPR5Y_5Y","LPR5Y_7Y", "LPR5Y_10Y",
            "FDR007_3M","FDR007_6M","FDR007_9M","FDR007_1Y", "FDR007_2Y", "FDR007_3Y", "FDR007_4Y", "FDR007_5Y",
            "GB10_1M","GB10_3M","GB10_6M", "GB10_9M", "GB10_1Y", "GB10_5Y",
            "CDB10_1M","CDB10_3M","CDB10_6M", "CDB10_9M", "CDB10_1Y", "CDB10_5Y",
            "D10/G10_1M","D10/G10_3M","D10/G10_6M", "D10/G10_9M", "D10/G10_1Y", "D10/G10_5Y",
            "AAA3/D3_1M","AAA3/D3_3M","AAA3/D3_6M", "AAA3/D3_9M", "AAA3/D3_1Y", "AAA3/D3_5Y",
            "ShiborO/N_1M","ShiborO/N_3M","ShiborO/N_6M", "ShiborO/N_9M", "ShiborO/N_1Y", "ShiborO/N_2Y", "ShiborO/N_3Y",
            "FDR001_1M","FDR001_3M","FDR001_6M", "FDR001_9M", "FDR001_1Y", "FDR001_2Y", "FDR001_3Y",
            "FR007_3M*6M","FR007_6M*9M","FR007_6M*1Y","FR007_9M*1Y","FR007_1Y*2Y","FR007_1Y*5Y","FR007_2Y*5Y",
            "Shibor3M_6M*9M","Shibor3M_9M*1Y","Shibor3M_1Y*2Y","Shibor3M_1Y*5Y","Shibor3M_2Y*5Y",
            "S3M/R07_1Y","S3M/R07_5Y"};

        private void InitIRSBest()
        {
            foreach(string code in irsCodes)
            {
                EmsOrderReport er = new EmsOrderReport();
                er.securityid = code;

                UpdateInnerOrder_Best(er);
            }
        }

        private void onloadData_Guarantor(bool obj)
        {
            //throw new NotImplementedException();
        }

        ~InnerBoardViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            base.Dispose();
            isInit = false;
            IntentionIRSHandler.Instance.OnQor -= Instance_OnQor;
            LastExecutionProcesser.Processer.OnLastExecutions -= Processer_OnLastExecutions;

            if (auteRefreshTimer != null)
            {
                auteRefreshTimer.Dispose();
            }

            if (RefreshUIThread != null)
            {
                RefreshUIThread.Abort();
                RefreshUIThread = null;
            }

            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (InnerOrders != null)
                    {
                        MyOrderCount = 0;
                        InnerOrders.Clear();
                        InnerOrders = null;
                        InnerOrders = new CustomCollection<InnerOrderData>();
                    }

                    if (_innerOrders_best != null)
                    {
                        _innerOrders_best.Clear();
                        _innerOrders_best = null;
                        _innerOrders_best = new CustomCollection<InnerOrderData>();
                    }

                    InterestRateDebtList.Clear();
                    CreditBondList.Clear();
                    RemaingingTimeList.Clear();
                    RatingList.Clear();
                }));
            }
        }

        private void Processer_OnLastExecutions(ExecutionReport obj)
        {
            // 市场最新成交
            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                lock (_dataLock)
                {
                    IEnumerable<InnerOrderData> innerDatas = this.InnerOrders.Where(item => item.SecurityID == obj.SecurityID);
                    if (innerDatas != null && innerDatas.Count() > 0)
                    {
                        InnerOrderData innerOrderData = innerDatas.First();
                        UpdateLastExecutions(innerOrderData);
                    }
                }

                lock (_dataLock_best)
                {
                    IEnumerable<InnerOrderData> innerDatas = this.InnerOrders_Best.Where(item => item.SecurityID == obj.SecurityID);
                    if (innerDatas != null && innerDatas.Count() > 0)
                    {
                        InnerOrderData innerOrderData = innerDatas.First();
                        UpdateLastExecutions(innerOrderData);
                    }
                }
            }));
        }

        private void UpdateLastExecutions(InnerOrderData innerOrderData)
        {
            try
            {
                ExecutionReport lastExeItem = LastExecutionProcesser.Processer.GetExecutionReportDic(innerOrderData.SecurityID);
                
                if (innerOrderData != null && lastExeItem != null)
                {
                    if (lastExeItem.SecurityID.EndsWith(".SH") || lastExeItem.SecurityID.EndsWith(".SZ"))
                    {
                        innerOrderData.LastMarketPx = CommonUtils.ConvertToDecimal(lastExeItem.NetPrice); // 成交价
                    }
                    else
                    {
                        innerOrderData.LastMarketPx = CommonUtils.ConvertToDecimal(lastExeItem.Yield); // 成交价
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        private void onloadData(bool obj)
        {
            lock(_dataLock)
            {
                var itemList = InnerOrders.Where(item => string.IsNullOrEmpty(item.SecurityType));
                foreach (InnerOrderData data in itemList)
                {
                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(data.SecurityID);
                    if (bondsInfoPo != null)
                    {
                        //Symbol = bondsInfoPo.Short_Name;
                        //Security = bondsInfoPo.Bond_ID;
                        //SymbolName = bondsInfoPo.Short_Name;
                        data.RemainingTime = bondsInfoPo.RemaingingTime;
                        data.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        data.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        data.BondSubType = bondsInfoPo.Bond_Subtype;
                        //data.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                        //data.Rating = GenRating(data.SignDic1, data.SignDic3, data.SecurityID);
                        //data.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //data.Issuer = bondsInfoPo.issuer;
                        data.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        data.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    }
                }
            }    
        }

        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                isInit = false;
                InitEmsOrder1();
            }
        }

        private void AuteRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RefreshEmsOrder();
        }

        private void InitEmsOrder()
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                QueryEmsOrderReq request = new QueryEmsOrderReq();
                //request.dealTimeBegin = $"{DateTimeEx.ServerTime.ToString(dateDateFormatPattern)} 00:00:00.000";
                if (!isPermission)
                    request.userName = LocalEnv.UserInfo.UserName;
                QueryEmsOrderReplyPo reply = TableOldHandler.Instance.QueryEmsOrder_Irs(request);
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MyOrderCount = 0;
                        InnerOrders.Clear();
                        //_lastUpdateTime = string.Empty;
                        if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
                        {
                            foreach (EmsOrderReport er in reply.Datas)
                            {
                                try
                                {
                                    UpdateInnerOrder(er);
                                }
                                catch(Exception e)
                                {
                                    LogHandler.Error(e);
                                }
                            }
                        }

                        if (auteRefreshTimer == null)
                        {
                            auteRefreshTimer = new Timer();
                            auteRefreshTimer.Elapsed += AuteRefreshTimer_Elapsed;
                            auteRefreshTimer.Interval = 5000;
                            auteRefreshTimer.Enabled = AutoEnable;
                        }
                    }));
                }
            });
        }

        private void InitEmsOrder1()
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (InnerOrders != null)
                    {
                        MyOrderCount = 0;
                        InnerOrders.Clear();
                        InnerOrders = null;
                        InnerOrders = new CustomCollection<InnerOrderData>();
                    }

                    if (InnerOrders_Best != null)
                    {
                        InnerOrders_Best.Clear();
                        InnerOrders_Best = null;
                        InnerOrders_Best = new CustomCollection<InnerOrderData>();
                    }


                    // 初始化全部Irs代码
                    InitIRSBest();

                    IntentionIRSHandler.Instance.RequestQor();
                }));
            }
        }

        //private string _lastUpdateTime = string.Empty;
        //private const string dateDateFormatPattern = "yyyy-MM-dd";
        //private const string dateTimeFormatPattern = "yyyy-MM-dd HH:mm:ss.fff";
        private void RefreshEmsOrder()
        {
            ThreadPoolHelper.InvokeThreadPool((o) => {
                QueryEmsOrderReq request = new QueryEmsOrderReq();
                //request.dealTimeBegin = string.IsNullOrEmpty(_lastUpdateTime) ? $"{DateTimeEx.ServerTime.ToString(dateDateFormatPattern)} 00:00:00.000" : _lastUpdateTime;
                if (!isPermission)
                    request.userName = LocalEnv.UserInfo.UserName;
                QueryEmsOrderReplyPo reply = TableOldHandler.Instance.QueryEmsOrder_Irs(request);
                if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
                {
                    if (System.Windows.Application.Current != null)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            foreach (EmsOrderReport er in reply.Datas)
                            {
                                UpdateInnerOrder(er);
                            }
                        }));
                    }
                }
            });
        }

        private void OnCtPrice(MarketDataSnapshotFullRefresh price)
        {

        }

        private void UpdateCtPrice(MarketDataSnapshotFullRefresh price)
        {
            GetBestPrice(price);
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CCDCValuaModel srcccdc = null;
                    if (_symbolCountDic.ContainsKey(price.SecurityID)
                        && (_symbolCountDic[price.SecurityID] == null
                            || _symbolCountDic[price.SecurityID].CCDCValuation != price.CCDCValuation))
                    {
                        try
                        {
                            srcccdc = CommonUtils.GetCCDCValua(price.SecurityID);
                            if (srcccdc == null)
                            {
                                srcccdc = CommonUtils.GenerateCCDCValue(price);

                                _symbolCountDic[price.SecurityID] = srcccdc;

                                CommonUtils.SetCCDCValua(price.SecurityID, srcccdc);
                            }
                        }
                        catch (Exception e)
                        {
                            LogHandler.Error(e);
                        }
                    }
                       
                    lock(_dataLock)
                    {
                        IEnumerable<InnerOrderData> findDatas = InnerOrders.Where(item => item.SecurityID == price.SecurityID);
                        foreach (InnerOrderData findData in findDatas)
                        {
                            findData.Best_Bid = _bestPriceDic[price.SecurityID][0];
                            findData.Best_Ofr = _bestPriceDic[price.SecurityID][1];
                            RefreshCCDCValue(findData, price, srcccdc);
                        }
                    }
                    
                    lock(_dataLock_best)
                    {
                        IEnumerable<InnerOrderData> findDatas_best = InnerOrders_Best.Where(item => item.SecurityID == price.SecurityID);
                        foreach (InnerOrderData findData in findDatas_best)
                        {
                            findData.Best_Bid = _bestPriceDic[price.SecurityID][0];
                            if (findData.Price_InnerBid.HasValue && (findData.Best_Bid == null || findData.Best_Bid >= findData.Price_InnerBid) && findData.Count_InnerBid > 0)
                            {
                                findData.IsMarketBest_InnerBid = "1"; // 市场最优，大拇指
                            }
                            else
                            {
                                findData.IsMarketBest_InnerBid = "0";
                            }
                            
                            findData.Best_Ofr = _bestPriceDic[price.SecurityID][1];
                            if (findData.Price_InnerOfr.HasValue && (findData.Best_Ofr == null || findData.Best_Ofr <= findData.Price_InnerOfr) && findData.Count_InnerOfr > 0)
                            {
                                findData.IsMarketBest_InnerOfr = "1"; // 市场最优，大拇指
                            }
                            else
                            {
                                findData.IsMarketBest_InnerOfr = "0";
                            }
                            RefreshCCDCValue(findData, price, srcccdc);
                        }
                    }
                    
                }));
            }
        }

        private void RefreshCCDCValue(InnerOrderData findData, MarketDataSnapshotFullRefresh price, CCDCValuaModel srcccdc)
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
        /// key:securityid; value:bestPrices 0:bestBid 1:bestOfr 2:sencodBestBid 3:sencondBestOfr
        /// </summary>
        ConcurrentDictionary<string, decimal?[]> _bestPriceDic = new ConcurrentDictionary<string, decimal?[]>();

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
            //foreach (GNoMDEntries entry in bestPrices)
            //{
            //    if (entry.MDEntryType == "0") // bid
            //    {
            //        _bestPriceDic[price.SecurityID][0] = entry.MDEntryPx.ConvertToDecimal();
            //    }
            //    else if (entry.MDEntryType == "1") // ofr
            //    {
            //        _bestPriceDic[price.SecurityID][1] = entry.MDEntryPx.ConvertToDecimal();
            //    }
            //}
        }

        internal bool isInit = false;
        List<EmsOrderReport> realTimeList = new List<EmsOrderReport>();
        private void Instance_OnQor(EmsOrderReport er, bool isNew)
        {
            if (er != null && er.marketindicator != "2")
            {
                return;
            }

            if (isInit == false && isNew)
            {
                lock(realTimeList)
                {
                    realTimeList.Add(er);
                }
            }

            if (isNew == false)
            {
                if (er == null)
                {
                    lock(realTimeList)
                    {
                        foreach (EmsOrderReport er1 in realTimeList)
                        {
							//LogHandler.Info("1----" + System.Threading.Thread.CurrentThread.Name);
                            UpdateInnerOrder(er1, true);
                        }
                    }

                    System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        this.ViewSource.Source = InnerOrders;
                        this.ViewSource.SortDescriptions.Clear();
                        this.ViewSource.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
                        this.ViewSource.IsLiveFilteringRequested = true;
                        this.ViewSource.IsLiveSortingRequested = true;
                        this.ViewSource.View.CollectionChanged += View_CollectionChanged;
                        //SetFilterDatas();
                        SetFilterDatas1();
                        BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _dataLock);
                        this.ViewSource.View?.Refresh();

                        this.ViewSource_Best.Source = InnerOrders_Best;
                        this.ViewSource_Best.SortDescriptions.Clear();
                        this.ViewSource_Best.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
                        this.ViewSource_Best.IsLiveSortingRequested = true;
                        this.ViewSource_Best.LiveFilteringProperties.Add("FirmFlag_InnerBid");
                        this.ViewSource_Best.LiveFilteringProperties.Add("FirmFlag_InnerOfr");
                        this.ViewSource_Best.LiveFilteringProperties.Add("SettleType_InnerBid");
                        this.ViewSource_Best.LiveFilteringProperties.Add("SettleType_InnerOfr");
                        this.ViewSource_Best.LiveFilteringProperties.Add("PartySubName_InnerBid");
                        this.ViewSource_Best.LiveFilteringProperties.Add("PartySubName_InnerOfr");
                        this.ViewSource_Best.LiveFilteringProperties.Add("IsMe_InnerBid");
                        this.ViewSource_Best.LiveFilteringProperties.Add("IsMe_InnerOfr");
                        this.ViewSource_Best.LiveFilteringProperties.Add("Client_InnerBid");
                        this.ViewSource_Best.LiveFilteringProperties.Add("Client_InnerOfr");
                        this.ViewSource_Best.LiveFilteringProperties.Add("DQType_InnerBid");
                        this.ViewSource_Best.LiveFilteringProperties.Add("DQType_InnerOfr");
                        this.ViewSource_Best.IsLiveFilteringRequested = true;
                        this.ViewSource_Best.View.CollectionChanged += BestView_CollectionChanged;
                        //SetFilterDatas_Best();
                        SetFilterDatas_Best1();
                        BindingOperations.EnableCollectionSynchronization(this.ViewSource_Best.View, _dataLock_best);
                        this.ViewSource_Best.View?.Refresh();

                        isInit = true;
                    }));
                }
                else
                {
                    //LogHandler.Info("2----" + System.Threading.Thread.CurrentThread.Name);
                    UpdateInnerOrder(er, isNew);
                }
            }

            else
            {
                lock(_tempRealTimeOrderListLock)
                {
                    tempRealTimeOrderList.Add(er);
                }
                _autoResetEvent.Set();
            }
        }

        #region 逐笔
        public int _rowCount;
        public int RowCount
        {
            get
            {
                return _rowCount;
            }
            set
            {
                _rowCount = value;
                this.OnPropertyChanged();
            }
        }

        public int _bidRowCount;
        public int BidRowCount
        {
            get
            {
                return _bidRowCount;
            }
            set
            {
                _bidRowCount = value;
                this.OnPropertyChanged();
            }
        }

        public int _ofrRowCount;
        public int OfrRowCount
        {
            get
            {
                return _ofrRowCount;
            }
            set
            {
                _ofrRowCount = value;
                this.OnPropertyChanged();
            }
        }
        private void View_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _rowCount = 0;
            _bidRowCount = 0;
            _ofrRowCount = 0;
            lock(_dataLock)
            {
                foreach (InnerOrderData d in this.ViewSource.View)
                {
                    RowCount++;
                    if (d.Side == "1")
                    {
                        BidRowCount++;
                    }
                    if (d.Side == "2")
                    {
                        OfrRowCount++;
                    }
                }
            }
            
            RowCount = _rowCount;
            BidRowCount = _bidRowCount;
            OfrRowCount = _ofrRowCount;
        }
        #endregion

        #region 最优
        public int _rowCount_best;
        public int RowCount_Best
        {
            get
            {
                return _rowCount_best;
            }
            set
            {
                _rowCount_best = value;
                this.OnPropertyChanged();
            }
        }

        private int _bidRowCount_best;
        public int BidRowCount_Best
        {
            get
            {
                return _bidRowCount_best;
            }
            set
            {
                _bidRowCount_best = value;
                this.OnPropertyChanged();
            }
        }

        public int _ofrRowCount_best;
        public int OfrRowCount_Best
        {
            get
            {
                return _ofrRowCount_best;
            }
            set
            {
                _ofrRowCount_best = value;
                this.OnPropertyChanged();
            }
        }


        private void BestView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _rowCount_best = 0;
            _bidRowCount_best = 0;
            _ofrRowCount_best = 0;
            lock(_dataLock_best)
            {
                foreach (InnerOrderData d in this.ViewSource_Best.View)
                {
                    _rowCount_best++;
                    if (d.Count_InnerBid > 0)
                    {
                        _bidRowCount_best++;
                    }
                    if (d.Count_InnerOfr > 0)
                    {
                        _ofrRowCount_best++;
                    }
                }
            }
            RowCount_Best = _rowCount_best;
            BidRowCount_Best = _bidRowCount_best;
            OfrRowCount_Best = _ofrRowCount_best;
        }
        #endregion

        private System.Threading.Thread RefreshUIThread = null;//刷新ui线程
        private object _tempRealTimeOrderListLock = new object();
        private List<EmsOrderReport> tempRealTimeOrderList = new List<EmsOrderReport>();
        private System.Threading.AutoResetEvent _autoResetEvent = new System.Threading.AutoResetEvent(false);

        private void ProcessInnerOrder_RealTime()
        {
            while (true)
            {
                List<EmsOrderReport> processList = new List<EmsOrderReport>();
                lock (_tempRealTimeOrderListLock)
                {
                    processList.AddRange(tempRealTimeOrderList);
                    tempRealTimeOrderList.Clear();
                }

                if (processList.Count > 0)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind, new Action(() =>
                    {
                        foreach (EmsOrderReport er in processList)
                        {
                            UpdateInnerOrder(er, true);
                        }
                    }));
                }
                else
                {
                    _autoResetEvent.WaitOne();
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        private async void UpdateInnerOrder(EmsOrderReport er, bool isNew = false)
        {
            //if (_lastUpdateTime.CompareTo(er.updatetime) < 0)
            //{
            //    _lastUpdateTime = er.updatetime;
            //}

            InnerOrderData findData = null;
            IEnumerable<InnerOrderData> findDatas = null;
            int dataCount = 0;
            lock (_dataLock)
            {
                findDatas = InnerOrders.Where(item => item.OrderID == er.orderid);
                if (findDatas != null)
                {
                    dataCount = findDatas.Count();

                    if (dataCount > 0)
                    {
                        findData = findDatas.First();
                    }
                }
            }
             

            if (dataCount > 0)
            {
                //if (!(er.exectype == "N" || er.exectype == "0" || er.exectype == "1" || er.exectype == "S" || er.exectype == "6"))
                if ((string.IsNullOrEmpty(er.firmflag) == false && (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C"))
                    || (string.IsNullOrEmpty(er.firmflag) == true && (er.orderstatus == "0" || er.orderstatus == "2" || er.orderstatus == "3" || er.orderstatus == "4"))
                    || (string.IsNullOrEmpty(er.firmflag) == true && er.orderstatus == "1" && er.price.HasValue && er.orderqty.HasValue))
                {

                    lock (_dataLock)
                    {
                        if (findData.IsMe)
                        {
                            MyOrderCount--;
                        }
                        this.InnerOrders.Remove(findData);
                        if (_symbolCountDic.ContainsKey(er.securityid) && !InnerOrders.Any(item => item.SecurityID == er.securityid))
                        {
                            PriceHandler.Instance.UnregisterCTPrice(er.securityid, UpdateCtPrice);
                            CCDCValuaModel removeCcdcValueModel;
                            _symbolCountDic.TryRemove(er.securityid, out removeCcdcValueModel);
                        }
                    }
                    UpdateInnerOrder_Best(er);
                    if (isNew)
                    {
                        try
                        {
                            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
                            {
                                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }
                    }    
                    return;
                }
                findData.OrderID = er.orderid;
                findData.SecurityID = er.securityid;
                findData.IsSpreadOrBasis = CommonUtils.IsSpreadOrBasic(er.securityid);
                findData.Symbol = er.symbol;
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityid);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    findData.RemainingTime = bondsInfoPo.RemaingingTime;
                    findData.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                    findData.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    findData.BondSubType = bondsInfoPo.Bond_Subtype;
                    //findData.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                    //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    //findData.Issuer = bondsInfoPo.issuer;
                    findData.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }
                findData.Side = er.side;
                findData.FirmFlag = string.IsNullOrEmpty(er.firmflag) ? INTENTION_FLAG : er.firmflag;
                //findData.RemaingingTime = 
                findData.SettleType = er.settltype;
                findData.SettlDate = $"{er.settlDate}*{er.settltype}"; 
                findData.RoleId = er.roleid;
                findData.StrategyOrderId = er.strategyorderid;
                findData.RoleName = er.rolename;
                findData.Price = er.price;
                if (!er.price.HasValue)
                {
                    if (er.side == "1")
                    {
                        findData.Price_Display = "Bid";
                    }
                    else if (er.side == "2")
                    {
                        findData.Price_Display = "Ofr";
                    }
                    else
                    {
                        findData.Price_Display = "--";
                    }
                }
                else
                {
                    findData.Price_Display = er.price.Value.ToString("0.00##");
                }
                findData.NetPrice = er.netprice;
                findData.Qty = er.orderqty?.ConvertToLong() - er.executionqty?.ConvertToLong();
                findData.Qty = findData.Qty / 100;
                findData.CopyInfo = GetCopyInfo(findData.Qty.ToString(), er);
                findData.PartySubId = er.partysubid;
                findData.PartySubName = er.partysubname;
                findData.UpdateTime = er.updatetime.Length > 11 ? er.updatetime.Substring(11, 8) : (er.updatetime.Length > 8 ? er.updatetime.Substring(0, 8) : er.updatetime);
                findData.CreateTime = er.createtime.Length > 11 ? er.createtime.Substring(11, 8) : (er.createtime.Length > 8 ? er.createtime.Substring(0, 8) : er.createtime);
                findData.WholeFlag = er.wholeflag;
                findData.IsMe = er.partysubid == LocalEnv.UserInfo.UserId;
                findData.IntentionID = er.inf4;
                //findData.IsNeedFlash = isNew;
				findData.FlashCount = isNew ?  4 : 0;
                findData.Sign = er.sign;
                findData.Sign1 = er.sign1;
                findData.Sign2 = er.sign2;
                findData.Sign3 = er.sign3;
                findData.Client = GenClient(findData.SignDic, er, findData.IsMe);
                findData.Rating = GenRating(findData.SignDic1, findData.SignDic3, findData.SecurityID);
                findData.BondCategory = GenBondCategory(findData.SignDic1);
                findData.Level = GenLevel(findData.SignDic3);
                findData.RegionType = GenRegionType(findData.SignDic2);
                findData.EnterpriseType = GenEnterpriseType(findData.SignDic1);
                findData.If_Guarantee = GenIfGuarantee(findData.SignDic1);
                findData.Term = GenTerm(findData.SignDic3, er.securityid);
                findData.Term_Convert = Utils.CommonUtils.ConvertRemainingTime(GenTerm(findData.SignDic3, string.Empty));
                findData.Product = GenProduct(findData.SignDic3);
                findData.Type = GenType(findData.SignDic3);
                findData.IssueType = GenIssueType(findData.SignDic1);
                findData.DQType = GenDQType(findData.SignDic, findData.IsMe);
                findData.TermDays = er.termDays;
                findData.CouponRateCurrent = er.couponRateCurrent;
                findData.IssuerCode = er.issuerCode;
                findData.IssuerName = string.IsNullOrEmpty(er.issuerName) ? "--" : er.issuerName;
                findData.WarranterCode = er.warranterCode;
                findData.WarranterName = string.IsNullOrEmpty(er.warranterName) ? "--" : er.warranterName;
                //findData.RemainingTime_New = STCTenorHelper.SetShowTenor(er.termDays, er.optionDate, er.optiontype);
                findData.ChinaBondValuation = er.chinaBondValuation;
                findData.ModifiedDuration = er.modifiedDuration;
                findData.DayOffStr = er.dayOffStr;
            }
            else
            {
                //if (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C")
                if ((string.IsNullOrEmpty(er.firmflag) == false && (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C"))
                    || (string.IsNullOrEmpty(er.firmflag) == true && (er.orderstatus == "0" || er.orderstatus == "2" || er.orderstatus == "3" || er.orderstatus == "4"))
                    || (string.IsNullOrEmpty(er.firmflag) == true && er.orderstatus == "1" && er.price.HasValue && er.orderqty.HasValue))
                {
                    return;
                }

                if (string.IsNullOrEmpty(er.securityid))
                {
                    return;
                }

                findData = new InnerOrderData();
                findData.OrderID = er.orderid;
                findData.SecurityID = er.securityid;
                findData.IsSpreadOrBasis = CommonUtils.IsSpreadOrBasic(er.securityid);
                findData.Symbol = er.symbol;
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityid);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    findData.RemainingTime = bondsInfoPo.RemaingingTime;
                    findData.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                    findData.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    findData.BondSubType = bondsInfoPo.Bond_Subtype;
                    //findData.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                    //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    //findData.Issuer = bondsInfoPo.issuer;
                    findData.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }
                findData.Side = er.side;
                findData.FirmFlag = string.IsNullOrEmpty(er.firmflag) ? INTENTION_FLAG : er.firmflag;
                //findData.RemaingingTime = 
                findData.SettleType = er.settltype;
                findData.SettlDate = $"{er.settlDate}*{er.settltype}";
                findData.RoleId = er.roleid;
                findData.RoleName = er.rolename;
                findData.Price = er.price;
                if (!er.price.HasValue)
                {
                    if (er.side == "1")
                    {
                        findData.Price_Display = "Bid";
                    }
                    else if (er.side == "2")
                    {
                        findData.Price_Display = "Ofr";
                    }
                    else
                    {
                        findData.Price_Display = "--";
                    }
                }
                else
                {
                    findData.Price_Display = er.price.Value.ToString("0.00##");
                }
                findData.NetPrice = er.netprice;
                findData.Qty = er.orderqty?.ConvertToLong() - er.executionqty?.ConvertToLong();
                findData.Qty = findData.Qty / 100;
                findData.CopyInfo = GetCopyInfo(findData.Qty.ToString(), er);
                findData.PartySubId = er.partysubid;
                findData.PartySubName = er.partysubname;
                findData.UpdateTime = er.updatetime.Length > 11 ? er.updatetime.Substring(11, 8) : (er.updatetime.Length > 8 ? er.updatetime.Substring(0, 8) : er.updatetime);
                findData.CreateTime = er.createtime.Length > 11 ? er.createtime.Substring(11, 8) : (er.createtime.Length > 8 ? er.createtime.Substring(0, 8) : er.createtime);
                findData.WholeFlag = er.wholeflag;
                findData.IsMe = er.partysubid == LocalEnv.UserInfo.UserId;
                findData.IntentionID = er.inf4;
                findData.StrategyOrderId = er.strategyorderid;
                //findData.IsNeedFlash = isNew;
				findData.FlashCount = isNew ? 4 : 0;
                findData.Sign = er.sign;
                findData.Sign1 = er.sign1;
                findData.Sign2 = er.sign2;
                findData.Sign3 = er.sign3;
                findData.Client = GenClient(findData.SignDic, er, findData.IsMe);
                findData.Rating = GenRating(findData.SignDic1, findData.SignDic3, findData.SecurityID);
                findData.BondCategory = GenBondCategory(findData.SignDic1);
                findData.Level = GenLevel(findData.SignDic3);
                findData.RegionType = GenRegionType(findData.SignDic2);
                findData.EnterpriseType = GenEnterpriseType(findData.SignDic1);
                findData.If_Guarantee = GenIfGuarantee(findData.SignDic1);
                findData.Term = GenTerm(findData.SignDic3, er.securityid);
                findData.Term_Convert = Utils.CommonUtils.ConvertRemainingTime(GenTerm(findData.SignDic3, string.Empty));
                findData.Product = GenProduct(findData.SignDic3);
                findData.Type = GenType(findData.SignDic3);
                findData.IssueType = GenIssueType(findData.SignDic1);
                findData.DQType = GenDQType(findData.SignDic, findData.IsMe);
                findData.TermDays = er.termDays;
                findData.CouponRateCurrent = er.couponRateCurrent;
                findData.IssuerCode = er.issuerCode; 
                findData.IssuerName = string.IsNullOrEmpty(er.issuerName) ? "--" : er.issuerName;
                findData.WarranterCode = er.warranterCode;
                findData.WarranterName = string.IsNullOrEmpty(er.warranterName) ? "--" : er.warranterName;
                //findData.RemainingTime_New = STCTenorHelper.SetShowTenor(er.termDays, er.optionDate, er.optiontype);
                findData.ChinaBondValuation = er.chinaBondValuation;
                findData.ModifiedDuration = er.modifiedDuration;
                findData.DayOffStr = er.dayOffStr;

                lock (_dataLock)
                {
                    if (findData.IsMe)
                    {
                        MyOrderCount++;
                    }
                    InnerOrders.Insert(0, findData);
                }

                UpdateLastExecutions(findData);

                try
                {
                    if (!_symbolCountDic.ContainsKey(er.securityid))
                    {
                        _symbolCountDic[er.securityid] = null;
                        LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{er.securityid}.");
                        PriceHandler.Instance.RegisterCTPrice(er.securityid, UpdateCtPrice);
                    }

                    CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(findData.SecurityID);
                    if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                    {
                        findData.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                        findData.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                        findData.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }

                if (findData.CCDCValuation.HasValue && findData.Price.HasValue)
                {
                    if (findData.Side == "1")
                        findData.CCDCValuation_BP = (findData.Price - findData.CCDCValuation) * 100;
                    else if (findData.Side == "2")
                        findData.CCDCValuation_BP = (findData.CCDCValuation - findData.Price) * 100;
                }
                
                if (_bestPriceDic.ContainsKey(er.securityid))
                {
                    findData.Best_Bid = _bestPriceDic[er.securityid][0];
                    findData.Best_Ofr = _bestPriceDic[er.securityid][1];
                }
            }
           
            UpdateInnerOrder_Best(er);
            if (isNew)
            {
                try
                {
                    if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
                    {
                        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                    }
                }
                catch(Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
            
        }

        internal PriceComparer bidPriceComparer = new PriceComparer("0");
        internal PriceComparer ofrPriceComparer = new PriceComparer("1");
        private void UpdateInnerOrder_Best(EmsOrderReport er)
        {
            //if (_lastUpdateTime.CompareTo(er.updatetime) < 0)
            //{
            //    _lastUpdateTime = er.updatetime;
            //}
            string updateTime = string.Empty;

            InnerOrderData findData_best;
            int bestCount = 0;
            IEnumerable<InnerOrderData> findDatas_Best = null;
            IEnumerable<InnerOrderData> bid_InnerOrdersList = null;
            IEnumerable<InnerOrderData> ofr_InnerOrdersList = null;
            int bidCount = 0;
            int ofrCount = 0;
            InnerOrderData myFirstBidInnerData = null;
            InnerOrderData myFirstOfrInnerData = null;
            lock (_dataLock)
            {
                IEnumerable<InnerOrderData> findDatas_securityid = InnerOrders.Where(item => item.SecurityID == er.securityid).OrderByDescending(item => item.UpdateTime);
                if (findDatas_securityid.Count() > 0)
                {
                    updateTime = findDatas_securityid.First().UpdateTime;
                }
                bid_InnerOrdersList = InnerOrders.Where(item => item.SecurityID == er.securityid && item.Side == "1").OrderBy(item => item, ofrPriceComparer).ToList();
                ofr_InnerOrdersList = InnerOrders.Where(item => item.SecurityID == er.securityid && item.Side == "2").OrderBy(item => item, bidPriceComparer).ToList();

                bidCount = bid_InnerOrdersList.Count();
                ofrCount = ofr_InnerOrdersList.Count();


                IEnumerable<InnerOrderData> myBidList = bid_InnerOrdersList.Where(item => item.IsMe);
                if (myBidList != null && myBidList.Count() > 0)
                {
                    myFirstBidInnerData = myBidList.First();
                }

                IEnumerable<InnerOrderData> myOfrList = ofr_InnerOrdersList.Where(item => item.IsMe);
                if (myOfrList != null && myOfrList.Count() > 0)
                {
                    myFirstOfrInnerData = myOfrList.First();
                }
            }

            lock (_dataLock_best)
            {
                findDatas_Best = InnerOrders_Best.Where(item => item.SecurityID == er.securityid);
                bestCount = findDatas_Best.Count();
            }

            this.InnerDepthPopupViewModel.OnInnerDepthCallback(er.securityid, bid_InnerOrdersList, ofr_InnerOrdersList);           
            

            if (er.side == "1")
            {
                if (bid_InnerOrdersList != null && bidCount > 0)
                {
                    InnerOrderData bestbid_inner = bid_InnerOrdersList.First();
                    foreach (InnerOrderData data in bid_InnerOrdersList)
                    {
                        if (data.IsMe && bestbid_inner.Price == data.Price)
                        {
                            data.IsInnerBest = true;
                        }
                        else
                        {
                            data.IsInnerBest = false;
                        }
                    }
                }
            }
            else if (er.side == "2")
            {
                if (ofr_InnerOrdersList != null && ofrCount > 0)
                {
                    InnerOrderData bestofr_inner = ofr_InnerOrdersList.First();
                    foreach (InnerOrderData data in ofr_InnerOrdersList)
                    {
                        if (data.IsMe && bestofr_inner.Price == data.Price)
                        {
                            data.IsInnerBest = true;
                        }
                        else
                        {
                            data.IsInnerBest = false;
                        }
                    }
                }
            }

            if (findDatas_Best != null && bestCount > 0)
            {
                findData_best = findDatas_Best.First();

                //if (!(er.exectype == "N" || er.exectype == "0" || er.exectype == "1" || er.exectype == "S" || er.exectype == "6"))
                if ((string.IsNullOrEmpty(er.firmflag) == false && (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C"))
                    || (string.IsNullOrEmpty(er.firmflag) == true && (er.orderstatus == "0" || er.orderstatus == "2" || er.orderstatus == "3" || er.orderstatus == "4"))
                    || (string.IsNullOrEmpty(er.firmflag) == true && er.orderstatus == "1" && er.price.HasValue && er.orderqty.HasValue))
                {
                    
                    // 删除Bid单边
                    if (bidCount == 0)
                    {
                        findData_best.PartySubName_InnerBid = string.Empty;
                        findData_best.Qty_InnerBid = null;
                        findData_best.CopyInfo_InnerBid = null;
                        findData_best.Count_InnerBid = 0;
                        findData_best.Price_InnerBid = null;
                        findData_best.FirmFlag_InnerBid = string.Empty;
                        findData_best.SettleType_InnerBid = string.Empty;
                        findData_best.RoleId_InnerBid = string.Empty;
                        findData_best.RoleName_InnerBid = string.Empty;
                        findData_best.WholeFlag_InnerBid = string.Empty;
                        findData_best.IsMe_InnerBid = false;
                        findData_best.NetPrice_InnerBid = null;
                        findData_best.IntentionId_InnerBid = string.Empty;
                        findData_best.StrategyOrderId_innerBid = string.Empty;
                        findData_best.IsBest_InnerBid = string.Empty;
                        findData_best.PartySubName_Tooltip_InnerBid = string.Empty;
                        findData_best.Qty_Tooltip_InnerBid = string.Empty;
                        findData_best.Sign_InnerBid = 0;
                        findData_best.Sign_InnerBid1 = 0;
                        findData_best.Sign_InnerBid2 = 0;
                        string x = $"000000000000000000{findData_best.GetIrsTerm(findData_best.Term)}{findData_best.GetIrsProduct(findData_best.Product, findData_best.Type)}{findData_best.GetIrsType(findData_best.Type)}00000000000000000000000000000000";
                        findData_best.Sign_InnerBid3 = Utils.CommonUtils.ToLong(x);
                        findData_best.Client_InnerBid = string.Empty;
                        findData_best.DQType_InnerBid = string.Empty;
                        findData_best.IsMarketBest_InnerBid = string.Empty;
                    }

                    // 删除Ofr单边
                    if (ofrCount == 0)
                    {
                        findData_best.PartySubName_InnerOfr = string.Empty;
                        findData_best.Qty_InnerOfr = null;
                        findData_best.CopyInfo_InnerOfr = null;
                        findData_best.Count_InnerOfr = 0;
                        findData_best.Price_InnerOfr = null;
                        findData_best.FirmFlag_InnerOfr = string.Empty;
                        findData_best.SettleType_InnerOfr = string.Empty;
                        findData_best.RoleId_InnerOfr = string.Empty;
                        findData_best.RoleName_InnerOfr = string.Empty;
                        findData_best.WholeFlag_InnerOfr = string.Empty;
                        findData_best.IsMe_InnerOfr = false;
                        findData_best.NetPrice_InnerOfr = null;
                        findData_best.IntentionId_InnerOfr = string.Empty;
                        findData_best.StrategyOrderId_InnerOfr = string.Empty;
                        findData_best.IsBest_InnerOfr = string.Empty;
                        findData_best.PartySubName_Tooltip_InnerOfr = string.Empty;
                        findData_best.Qty_Tooltip_InnerOfr = string.Empty;
                        findData_best.Sign_InnerOfr = 0;
                        findData_best.Sign_InnerOfr1 = 0;
                        findData_best.Sign_InnerOfr2 = 0;
                        string x = $"000000000000000000{findData_best.GetIrsTerm(findData_best.Term)}{findData_best.GetIrsProduct(findData_best.Product, findData_best.Type)}{findData_best.GetIrsType(findData_best.Type)}00000000000000000000000000000000";
                        findData_best.Sign_InnerOfr3 = Utils.CommonUtils.ToLong(x);
                        findData_best.Client_InnerOfr = string.Empty;
                        findData_best.DQType_InnerOfr = string.Empty;
                        findData_best.IsMarketBest_InnerOfr = string.Empty;
                    }
                    findData_best.UpdateTime = updateTime;

                    // 删除全部
                    if (bidCount == 0 && ofrCount == 0)
                    {
                        //lock(_dataLock_best)
                        //{
                        //    this.InnerOrders_Best.Remove(findData_best);
                        //}
                        return;
                    }
                    //await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                    //return;
                }
                
                findData_best.SecurityID = er.securityid;
                findData_best.Symbol = er.symbol;
                findData_best.UpdateTime = updateTime;
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityid);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    findData_best.RemainingTime = bondsInfoPo.RemaingingTime;
                    findData_best.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                    findData_best.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    findData_best.BondSubType = bondsInfoPo.Bond_Subtype;
                    //findData_best.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                    //findData_best.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    //findData_best.Issuer = bondsInfoPo.issuer;
                    findData_best.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    findData_best.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }

                if (bidCount > 0)
                {
                    InnerOrderData bestBidInnerOrder = bid_InnerOrdersList.First();
                    List<string> partySubNameList = new List<string>();
                    string partysubname = string.Empty;
                    long? qty = 0;
                    StringBuilder partySubNameTooltip = new StringBuilder();
                    StringBuilder qtyTooltip = new StringBuilder();
                    StringBuilder copyInfo = new StringBuilder();
                    foreach (InnerOrderData item in bid_InnerOrdersList)
                    {
                        if (item.Price == bestBidInnerOrder.Price)
                        {
                            qty += item.Qty.HasValue ? item.Qty.Value : 0;
                            if (partySubNameTooltip.Length > 0)
                            {
                                partySubNameTooltip.Append(" + ");
                            }
                            partySubNameTooltip.Append(item.PartySubName);

                            if(!partySubNameList.Contains(item.PartySubName))
                            {
                                partySubNameList.Add(item.PartySubName);
                            }

                            if (qtyTooltip.Length > 0)
                            {
                                qtyTooltip.Append(" + ");
                            }
                            qtyTooltip.Append(item.Qty.HasValue ? item.Qty.Value.ToString() : "Bid");

                            if (copyInfo.Length > 0)
                            {
                                copyInfo.Append("+");
                            }

                            copyInfo.Append(item.CopyInfo);
                        }
                        else
                        {
                            break;
                        }
                    }

                    findData_best.TermDays = bestBidInnerOrder.TermDays;
                    findData_best.CouponRateCurrent = bestBidInnerOrder.CouponRateCurrent;
                    findData_best.IssuerCode = bestBidInnerOrder.IssuerCode;
                    findData_best.IssuerName = bestBidInnerOrder.IssuerName;
                    findData_best.WarranterCode = bestBidInnerOrder.WarranterCode;
                    findData_best.WarranterName = bestBidInnerOrder.WarranterName;
                    //findData_best.RemainingTime_New = bestBidInnerOrder.RemainingTime_New;
                    findData_best.ChinaBondValuation = bestBidInnerOrder.ChinaBondValuation;
                    findData_best.ModifiedDuration = bestBidInnerOrder.ModifiedDuration;
                    findData_best.DayOffStr = bestBidInnerOrder.DayOffStr;
                    findData_best.Rating = bestBidInnerOrder.Rating;
                    findData_best.BondCategory = bestBidInnerOrder.BondCategory;
                    findData_best.Level = bestBidInnerOrder.Level;
                    findData_best.RegionType = bestBidInnerOrder.RegionType;
                    findData_best.EnterpriseType = bestBidInnerOrder.EnterpriseType;
                    findData_best.If_Guarantee = bestBidInnerOrder.If_Guarantee;
                    findData_best.IssueType = bestBidInnerOrder.IssueType;
                    if (!string.IsNullOrEmpty(bestBidInnerOrder.Term))
                        findData_best.Term = bestBidInnerOrder.Term;
                    findData_best.Term_Convert = bestBidInnerOrder.Term_Convert;
                    if (!string.IsNullOrEmpty(bestBidInnerOrder.Product))
                        findData_best.Product = bestBidInnerOrder.Product;
                    findData_best.Type = bestBidInnerOrder.Type;
                    findData_best.PartySubName_InnerBid = partySubNameList.Count > 1 ? "多" : bestBidInnerOrder.PartySubName;
                    findData_best.PartySubName_Tooltip_InnerBid = partySubNameTooltip.ToString();
                    findData_best.Qty_InnerBid = qty == 0 ? null : qty;
                    findData_best.CopyInfo_InnerBid = copyInfo.ToString();
                    findData_best.Qty_Tooltip_InnerBid = qtyTooltip.ToString();
                    
                    //int count = bid_InnerOrdersList.Count(item => item.Price == bestBidInnerOrder.Price);
                    findData_best.Count_InnerBid = bidCount;
                    findData_best.Price_InnerBid = bestBidInnerOrder.Price;
                    findData_best.FirmFlag_InnerBid = bestBidInnerOrder.FirmFlag;
                    findData_best.SettleType_InnerBid = bestBidInnerOrder.SettleType;
                    findData_best.RoleId_InnerBid = bestBidInnerOrder.RoleId;
                    findData_best.RoleName_InnerBid = bestBidInnerOrder.RoleName;
                    findData_best.WholeFlag_InnerBid = bestBidInnerOrder.WholeFlag;
                    findData_best.IsMe_InnerBid = bestBidInnerOrder.PartySubId == LocalEnv.UserInfo.UserId;
                    findData_best.IntentionId_InnerBid = bestBidInnerOrder.IntentionID;
                    findData_best.NetPrice_InnerBid = bestBidInnerOrder.NetPrice;
                    findData_best.StrategyOrderId_innerBid = bestBidInnerOrder.StrategyOrderId;
                    findData_best.Sign_InnerBid = bestBidInnerOrder.Sign;
                    findData_best.Sign_InnerBid1 = bestBidInnerOrder.Sign1;
                    findData_best.Sign_InnerBid2 = bestBidInnerOrder.Sign2;
                    findData_best.Sign_InnerBid3 = bestBidInnerOrder.Sign3;
                    findData_best.Client_InnerBid = bestBidInnerOrder.Client;
                    findData_best.DQType_InnerBid = bestBidInnerOrder.DQType;
                    findData_best.IsSpreadOrBasis = bestBidInnerOrder.IsSpreadOrBasis;


                    if (myFirstBidInnerData != null)
                    {
                        if (myFirstBidInnerData.Price == findData_best.Price_InnerBid)
                        {
                            findData_best.IsBest_InnerBid = "2";// 内部最优，底色
                        }
                        else
                        {
                            findData_best.IsBest_InnerBid = "3";// 非内部最优，边框
                        }
                    }
                    else
                    {
                        findData_best.IsBest_InnerBid = string.Empty;
                    }
                }
                
                if (ofrCount > 0)
                {
                    InnerOrderData bestOfrInnerOrder = ofr_InnerOrdersList.First();
                    List<string> partySubNameList = new List<string>();
                    string partysubname = string.Empty;
                    long? qty = 0;
                    StringBuilder partySubNameTooltip = new StringBuilder();
                    StringBuilder qtyTooltip = new StringBuilder();
                    StringBuilder copyInfo = new StringBuilder();
                    foreach (InnerOrderData item in ofr_InnerOrdersList)
                    {
                        if (item.Price == bestOfrInnerOrder.Price)
                        {
                            qty += item.Qty.HasValue ? item.Qty.Value : 0;
                            if (partySubNameTooltip.Length > 0)
                            {
                                partySubNameTooltip.Append(" + ");
                            }
                            partySubNameTooltip.Append(item.PartySubName);

                            if (!partySubNameList.Contains(item.PartySubName))
                            {
                                partySubNameList.Add(item.PartySubName);
                            }

                            if (qtyTooltip.Length > 0)
                            {
                                qtyTooltip.Append(" + ");
                            }
                            qtyTooltip.Append(item.Qty.HasValue ? item.Qty.Value.ToString() : "Ofr");

                            if (copyInfo.Length > 0)
                            {
                                copyInfo.Append("+");
                            }

                            copyInfo.Append(item.CopyInfo);
                        }
                        else
                        {
                            break;
                        }
                    }

                    
                    findData_best.PartySubName_InnerOfr = partySubNameList.Count > 1 ? "多" : bestOfrInnerOrder.PartySubName;
                    findData_best.PartySubName_Tooltip_InnerOfr = partySubNameTooltip.ToString();
                    findData_best.CopyInfo_InnerOfr = copyInfo.ToString();
                    findData_best.Qty_InnerOfr = qty == 0 ? null : qty;
                    findData_best.Qty_Tooltip_InnerOfr = qtyTooltip.ToString();

                    findData_best.TermDays = bestOfrInnerOrder.TermDays;
                    findData_best.CouponRateCurrent = bestOfrInnerOrder.CouponRateCurrent;
                    findData_best.IssuerCode = bestOfrInnerOrder.IssuerCode;
                    findData_best.IssuerName = bestOfrInnerOrder.IssuerName;
                    findData_best.WarranterCode = bestOfrInnerOrder.WarranterCode;
                    findData_best.WarranterName = bestOfrInnerOrder.WarranterName;
                    //findData_best.RemainingTime_New = bestOfrInnerOrder.RemainingTime_New;
                    findData_best.ChinaBondValuation = bestOfrInnerOrder.ChinaBondValuation;
                    findData_best.ModifiedDuration = bestOfrInnerOrder.ModifiedDuration;
                    findData_best.DayOffStr = bestOfrInnerOrder.DayOffStr;
                    findData_best.Rating = bestOfrInnerOrder.Rating;
                    findData_best.BondCategory = bestOfrInnerOrder.BondCategory;
                    findData_best.Level = bestOfrInnerOrder.Level;
                    findData_best.RegionType = bestOfrInnerOrder.RegionType;
                    findData_best.EnterpriseType = bestOfrInnerOrder.EnterpriseType;
                    findData_best.If_Guarantee = bestOfrInnerOrder.If_Guarantee;
                    findData_best.IssueType = bestOfrInnerOrder.IssueType;
                    if (!string.IsNullOrEmpty(bestOfrInnerOrder.Term))
                        findData_best.Term = bestOfrInnerOrder.Term;
                    findData_best.Term_Convert = bestOfrInnerOrder.Term_Convert;
                    if (!string.IsNullOrEmpty(bestOfrInnerOrder.Product))
                        findData_best.Product = bestOfrInnerOrder.Product;
                    findData_best.Type = bestOfrInnerOrder.Type;
                    findData_best.Price_InnerOfr = bestOfrInnerOrder.Price;
                    //int count = ofr_InnerOrdersList.Count(item => item.Price == bestOfrInnerOrder.Price);
                    findData_best.Count_InnerOfr = ofrCount;
                    findData_best.FirmFlag_InnerOfr = bestOfrInnerOrder.FirmFlag;
                    findData_best.SettleType_InnerOfr = bestOfrInnerOrder.SettleType;
                    findData_best.RoleId_InnerOfr = bestOfrInnerOrder.RoleId;
                    findData_best.RoleName_InnerOfr = bestOfrInnerOrder.RoleName;
                    findData_best.WholeFlag_InnerOfr = bestOfrInnerOrder.WholeFlag;
                    findData_best.IsMe_InnerOfr = bestOfrInnerOrder.PartySubId == LocalEnv.UserInfo.UserId;
                    findData_best.IntentionId_InnerOfr = bestOfrInnerOrder.IntentionID;
                    findData_best.NetPrice_InnerOfr = bestOfrInnerOrder.NetPrice;
                    findData_best.StrategyOrderId_InnerOfr = bestOfrInnerOrder.StrategyOrderId;
                    findData_best.Sign_InnerOfr = bestOfrInnerOrder.Sign;
                    findData_best.Sign_InnerOfr1 = bestOfrInnerOrder.Sign1;
                    findData_best.Sign_InnerOfr2 = bestOfrInnerOrder.Sign2;
                    findData_best.Sign_InnerOfr3 = bestOfrInnerOrder.Sign3;
                    findData_best.Client_InnerOfr = bestOfrInnerOrder.Client;
                    findData_best.DQType_InnerOfr = bestOfrInnerOrder.DQType;
                    findData_best.IsSpreadOrBasis = bestOfrInnerOrder.IsSpreadOrBasis;

                    if (myFirstOfrInnerData != null)
                    {
                        if (myFirstOfrInnerData.Price == findData_best.Price_InnerOfr)
                        {
                            findData_best.IsBest_InnerOfr = "2";// 内部最优，底色
                        }
                        else
                        {
                            findData_best.IsBest_InnerOfr = "3";// 非内部最优，边框
                        }
                    }
                    else
                    {
                        findData_best.IsBest_InnerOfr = string.Empty;
                    }
                }
            }
            else
            {
                //if (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C")
                if ((string.IsNullOrEmpty(er.firmflag) == false && (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C"))
                    || (string.IsNullOrEmpty(er.firmflag) == true && (er.orderstatus == "0" || er.orderstatus == "2" || er.orderstatus == "3" || er.orderstatus == "4"))
                    || (string.IsNullOrEmpty(er.firmflag) == true && er.orderstatus == "1" && er.price.HasValue && er.orderqty.HasValue))
                {
                    return;
                }

                if (string.IsNullOrEmpty(er.securityid))
                {
                    return;
                }

                findData_best = new InnerOrderData();
                
                findData_best.SecurityID = er.securityid;
                findData_best.Symbol = er.symbol;
                findData_best.UpdateTime = updateTime;

                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityid);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    findData_best.RemainingTime = bondsInfoPo.RemaingingTime;
                    findData_best.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                    findData_best.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    findData_best.BondSubType = bondsInfoPo.Bond_Subtype;
                    //findData_best.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                    //findData_best.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    //findData_best.Issuer = bondsInfoPo.issuer;
                    findData_best.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    findData_best.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }

                if (bidCount > 0)
                {
                    InnerOrderData bestBidInnerOrder = bid_InnerOrdersList.First();
                    List<string> partySubNameList = new List<string>();
                    string partysubname = string.Empty;
                    long? qty = 0;
                    StringBuilder partySubNameTooltip = new StringBuilder();
                    StringBuilder qtyTooltip = new StringBuilder();
                    StringBuilder copyInfo = new StringBuilder();
                    foreach (InnerOrderData item in bid_InnerOrdersList)
                    {
                        if (item.Price == bestBidInnerOrder.Price)
                        {
                            qty += item.Qty.HasValue ? item.Qty.Value : 0;
                            if (partySubNameTooltip.Length > 0)
                            {
                                partySubNameTooltip.Append(" + ");
                            }
                            partySubNameTooltip.Append(item.PartySubName);

                            if (!partySubNameList.Contains(item.PartySubName))
                            {
                                partySubNameList.Add(item.PartySubName);
                            }

                            if (qtyTooltip.Length > 0)
                            {
                                qtyTooltip.Append(" + ");
                            }
                            qtyTooltip.Append(item.Qty.HasValue ? item.Qty.Value.ToString() : "Bid");

                            if (copyInfo.Length > 0)
                            {
                                copyInfo.Append("+");
                            }

                            copyInfo.Append(item.CopyInfo);
                        }
                        else
                        {
                            break;
                        }
                    }

                    findData_best.TermDays = bestBidInnerOrder.TermDays;
                    findData_best.CouponRateCurrent = bestBidInnerOrder.CouponRateCurrent;
                    findData_best.IssuerCode = bestBidInnerOrder.IssuerCode;
                    findData_best.IssuerName = bestBidInnerOrder.IssuerName;
                    findData_best.WarranterCode = bestBidInnerOrder.WarranterCode;
                    findData_best.WarranterName = bestBidInnerOrder.WarranterName;
                    //findData_best.RemainingTime_New = bestBidInnerOrder.RemainingTime_New;
                    findData_best.ChinaBondValuation = bestBidInnerOrder.ChinaBondValuation;
                    findData_best.ModifiedDuration = bestBidInnerOrder.ModifiedDuration;
                    findData_best.DayOffStr = bestBidInnerOrder.DayOffStr;
                    findData_best.Rating = bestBidInnerOrder.Rating;
                    findData_best.BondCategory = bestBidInnerOrder.BondCategory;
                    findData_best.Level = bestBidInnerOrder.Level;
                    findData_best.RegionType = bestBidInnerOrder.RegionType;
                    findData_best.EnterpriseType = bestBidInnerOrder.EnterpriseType;
                    findData_best.If_Guarantee = bestBidInnerOrder.If_Guarantee;
                    findData_best.IssueType = bestBidInnerOrder.IssueType;
                    if (!string.IsNullOrEmpty(bestBidInnerOrder.Term))
                        findData_best.Term = bestBidInnerOrder.Term;
                    findData_best.Term_Convert = bestBidInnerOrder.Term_Convert;
                    if (!string.IsNullOrEmpty(bestBidInnerOrder.Product))
                        findData_best.Product = bestBidInnerOrder.Product;
                    findData_best.Type = bestBidInnerOrder.Type;
                    findData_best.PartySubName_InnerBid = partySubNameList.Count > 1 ? "多" : bestBidInnerOrder.PartySubName;
                    findData_best.PartySubName_Tooltip_InnerBid = partySubNameTooltip.ToString();
                    findData_best.Qty_InnerBid = qty == 0 ? null : qty;
                    findData_best.CopyInfo_InnerBid = copyInfo.ToString();
                    findData_best.Qty_Tooltip_InnerBid = qtyTooltip.ToString();
                    //int count = bid_InnerOrdersList.Count(item => item.Price == bestBidInnerOrder.Price);
                    findData_best.Count_InnerBid = bidCount;
                    findData_best.Price_InnerBid = bestBidInnerOrder.Price;
                    findData_best.FirmFlag_InnerBid = bestBidInnerOrder.FirmFlag;
                    findData_best.SettleType_InnerBid = bestBidInnerOrder.SettleType;
                    findData_best.RoleId_InnerBid = bestBidInnerOrder.RoleId;
                    findData_best.RoleName_InnerBid = bestBidInnerOrder.RoleName;
                    findData_best.WholeFlag_InnerBid = bestBidInnerOrder.WholeFlag;
                    findData_best.IsMe_InnerBid = bestBidInnerOrder.PartySubId == LocalEnv.UserInfo.UserId;
                    findData_best.IntentionId_InnerBid = bestBidInnerOrder.IntentionID;
                    findData_best.NetPrice_InnerBid = bestBidInnerOrder.NetPrice;
                    findData_best.StrategyOrderId_innerBid = bestBidInnerOrder.StrategyOrderId;
                    findData_best.Sign_InnerBid = bestBidInnerOrder.Sign;
                    findData_best.Sign_InnerBid1 = bestBidInnerOrder.Sign1;
                    findData_best.Sign_InnerBid2 = bestBidInnerOrder.Sign2;
                    findData_best.Sign_InnerBid3 = bestBidInnerOrder.Sign3;
                    findData_best.Client_InnerBid = bestBidInnerOrder.Client;
                    findData_best.DQType_InnerBid = bestBidInnerOrder.DQType;
                    findData_best.IsSpreadOrBasis = bestBidInnerOrder.IsSpreadOrBasis;

                    if (myFirstBidInnerData != null)
                    {
                        if (myFirstBidInnerData.Price == findData_best.Price_InnerBid)
                        {
                            findData_best.IsBest_InnerBid = "2";// 内部最优，底色
                        }
                        else
                        {
                            findData_best.IsBest_InnerBid = "3";// 非内部最优，边框
                        }
                    }
                    else
                    {
                        findData_best.IsBest_InnerBid = string.Empty;
                    }
                }

                if (ofrCount > 0)
                {
                    InnerOrderData bestOfrInnerOrder = ofr_InnerOrdersList.First();
                    List<string> partySubNameList = new List<string>();
                    string partysubname = string.Empty;
                    long? qty = 0;
                    StringBuilder partySubNameTooltip = new StringBuilder();
                    StringBuilder qtyTooltip = new StringBuilder();
                    StringBuilder copyInfo = new StringBuilder();
                    foreach (InnerOrderData item in ofr_InnerOrdersList)
                    {
                        if (item.Price == bestOfrInnerOrder.Price)
                        {
                            qty += item.Qty.HasValue ? item.Qty.Value : 0;
                            if (partySubNameTooltip.Length > 0)
                            {
                                partySubNameTooltip.Append(" + ");
                            }
                            partySubNameTooltip.Append(item.PartySubName);

                            if (!partySubNameList.Contains(item.PartySubName))
                            {
                                partySubNameList.Add(item.PartySubName);
                            }

                            if (qtyTooltip.Length > 0)
                            {
                                qtyTooltip.Append(" + ");
                            }
                            qtyTooltip.Append(item.Qty.HasValue ? item.Qty.Value.ToString() : "Ofr");

                            if (copyInfo.Length > 0)
                            {
                                copyInfo.Append("+");
                            }

                            copyInfo.Append(item.CopyInfo);
                        }
                        else
                        {
                            break;
                        }
                    }

                    findData_best.PartySubName_InnerOfr = partySubNameList.Count > 1 ? "多" : bestOfrInnerOrder.PartySubName;
                    findData_best.PartySubName_Tooltip_InnerOfr = partySubNameTooltip.ToString();
                    findData_best.Qty_InnerOfr = qty == 0 ? null : qty;
                    findData_best.CopyInfo_InnerOfr = copyInfo.ToString();
                    findData_best.Qty_Tooltip_InnerOfr = qtyTooltip.ToString();


                    findData_best.TermDays = bestOfrInnerOrder.TermDays;
                    findData_best.CouponRateCurrent = bestOfrInnerOrder.CouponRateCurrent;
                    findData_best.IssuerCode = bestOfrInnerOrder.IssuerCode;
                    findData_best.IssuerName = bestOfrInnerOrder.IssuerName;
                    findData_best.WarranterCode = bestOfrInnerOrder.WarranterCode;
                    findData_best.WarranterName = bestOfrInnerOrder.WarranterName;
                    //findData_best.RemainingTime_New = bestOfrInnerOrder.RemainingTime_New;
                    findData_best.ChinaBondValuation = bestOfrInnerOrder.ChinaBondValuation;
                    findData_best.ModifiedDuration = bestOfrInnerOrder.ModifiedDuration;
                    findData_best.DayOffStr = bestOfrInnerOrder.DayOffStr;
                    findData_best.Rating = bestOfrInnerOrder.Rating;
                    findData_best.BondCategory = bestOfrInnerOrder.BondCategory;
                    findData_best.Level = bestOfrInnerOrder.Level;
                    findData_best.RegionType = bestOfrInnerOrder.RegionType;
                    findData_best.EnterpriseType = bestOfrInnerOrder.EnterpriseType;
                    findData_best.If_Guarantee = bestOfrInnerOrder.If_Guarantee;
                    findData_best.IssueType = bestOfrInnerOrder.IssueType;
                    if (!string.IsNullOrEmpty(bestOfrInnerOrder.Term))
                        findData_best.Term = bestOfrInnerOrder.Term;
                    findData_best.Term_Convert = bestOfrInnerOrder.Term_Convert;
                    if (!string.IsNullOrEmpty(bestOfrInnerOrder.Product))
                        findData_best.Product = bestOfrInnerOrder.Product;
                    findData_best.Type = bestOfrInnerOrder.Type;
                    findData_best.Price_InnerOfr = bestOfrInnerOrder.Price;
                    //int count = ofr_InnerOrdersList.Count(item => item.Price == bestOfrInnerOrder.Price);
                    findData_best.Count_InnerOfr = ofrCount;
                    findData_best.FirmFlag_InnerOfr = bestOfrInnerOrder.FirmFlag;
                    findData_best.SettleType_InnerOfr = bestOfrInnerOrder.SettleType;
                    findData_best.RoleId_InnerOfr = bestOfrInnerOrder.RoleId;
                    findData_best.RoleName_InnerOfr = bestOfrInnerOrder.RoleName;
                    findData_best.WholeFlag_InnerOfr = bestOfrInnerOrder.WholeFlag;
                    findData_best.IsMe_InnerOfr = bestOfrInnerOrder.PartySubId == LocalEnv.UserInfo.UserId;
                    findData_best.IntentionId_InnerOfr = bestOfrInnerOrder.IntentionID;
                    findData_best.NetPrice_InnerOfr = bestOfrInnerOrder.NetPrice;
                    findData_best.StrategyOrderId_InnerOfr = bestOfrInnerOrder.StrategyOrderId;
                    findData_best.Sign_InnerOfr = bestOfrInnerOrder.Sign;
                    findData_best.Sign_InnerOfr1 = bestOfrInnerOrder.Sign1;
                    findData_best.Sign_InnerOfr2 = bestOfrInnerOrder.Sign2;
                    findData_best.Sign_InnerOfr3 = bestOfrInnerOrder.Sign3;
                    findData_best.Client_InnerOfr = bestOfrInnerOrder.Client;
                    findData_best.DQType_InnerOfr = bestOfrInnerOrder.DQType;
                    findData_best.IsSpreadOrBasis = bestOfrInnerOrder.IsSpreadOrBasis;

                    if (myFirstOfrInnerData != null)
                    {
                        if (myFirstOfrInnerData.Price == findData_best.Price_InnerOfr)
                        {
                            findData_best.IsBest_InnerOfr = "2";// 内部最优，底色
                        }
                        else
                        {
                            findData_best.IsBest_InnerOfr = "3";// 非内部最优，边框
                        }
                    }
                    else
                    {
                        findData_best.IsBest_InnerOfr = string.Empty;
                    }
                }

                UpdateLastExecutions(findData_best);

                lock (_dataLock_best)
                {
                    InnerOrders_Best.Insert(0, findData_best);
                }

                try
                {
                    if (!_symbolCountDic.ContainsKey(er.securityid))
                    {
                        _symbolCountDic[er.securityid] = null;
                        LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{er.securityid}.");
                        PriceHandler.Instance.RegisterCTPrice(er.securityid, UpdateCtPrice);
                    }

                    CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(findData_best.SecurityID);
                    if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                    {
                        findData_best.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                        findData_best.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                        findData_best.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }

                if (findData_best.CCDCValuation.HasValue && findData_best.Price.HasValue)
                {
                    if (findData_best.Side == "1")
                        findData_best.CCDCValuation_BP = (findData_best.Price - findData_best.CCDCValuation) * 100;
                    else if (findData_best.Side == "2")
                        findData_best.CCDCValuation_BP = (findData_best.CCDCValuation - findData_best.Price) * 100;
                }
            }
            if (_bestPriceDic.ContainsKey(er.securityid))
            {
                findData_best.Best_Bid = _bestPriceDic[er.securityid][0];
                if (findData_best.Price_InnerBid.HasValue && (findData_best.Best_Bid == null || findData_best.Best_Bid >= findData_best.Price_InnerBid) && findData_best.Count_InnerBid > 0)
                {
                    findData_best.IsMarketBest_InnerBid = "1"; // 市场最优，大拇指
                }
                else
                {
                    findData_best.IsMarketBest_InnerBid = "0";
                }

                findData_best.Best_Ofr = _bestPriceDic[er.securityid][1];
                if (findData_best.Price_InnerOfr.HasValue && (findData_best.Best_Ofr == null || findData_best.Best_Ofr <= findData_best.Price_InnerOfr) && findData_best.Count_InnerOfr > 0)
                {
                    findData_best.IsMarketBest_InnerOfr = "1"; // 市场最优，大拇指
                }
                else
                {
                    findData_best.IsMarketBest_InnerOfr = "0";
                }
            }
           

        }

        #region 逐笔过滤
        private List<string> firmFlagFilterList = new List<string>();
        private List<string> settleTypeFilterList = new List<string>();
        private List<string> sideFilterList = new List<string>();
        private List<string> roleFilterList = new List<string>();
        private string isMeFilter = "0";

        internal void FilterData(Dictionary<string, IList> filterDic)
        {
            firmFlagFilterList.Clear();
            if (filterDic != null && filterDic.ContainsKey("FirmFlag"))
            {
                IList firmTypeList = filterDic["FirmFlag"];
                if (firmTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem firmType in firmTypeList)
                    {
                        string firmContent = firmType.Content.ToString();
                        string firmflag = string.Empty;
                        if (firmContent == "firm")
                        {
                            firmflag = "1";
                        }
                        else if (firmContent == "nonfirm")
                        {
                            firmflag = "0";
                        }
                        else
                        {
                            firmflag = INTENTION_FLAG;
                        }
                        if (!firmFlagFilterList.Contains(firmflag))
                        {
                            firmFlagFilterList.Add(firmflag);
                        }
                        
                    }
                }
            }

            settleTypeFilterList.Clear();
            if (filterDic != null && filterDic.ContainsKey("SettlType"))
            {

                IList settleTypeList = filterDic["SettlType"];
                if (settleTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem settleTypeItem in settleTypeList)
                    {
                        string settleTypeContent = settleTypeItem.Content.ToString();
                        string settleTypeflag = string.Empty;
                        if (settleTypeContent == "T+0")
                        {
                            settleTypeflag = "1";
                        }
                        else if (settleTypeContent == "T+1")
                        {
                            if (!settleTypeFilterList.Contains("2"))
                            {
                                settleTypeFilterList.Add("2");
                            }
                            if (!settleTypeFilterList.Contains("3"))
                            {
                                settleTypeFilterList.Add("3");
                            }
                        }
                        else if (settleTypeContent == "远期")
                        {
                            settleTypeflag = "4";
                        }
                        if (!settleTypeFilterList.Contains(settleTypeflag))
                        {
                            settleTypeFilterList.Add(settleTypeflag);
                        }

                    }
                }
            }

            sideFilterList.Clear();
            if (filterDic != null && filterDic.ContainsKey("Side"))
            {
                IList sideList = filterDic["Side"];
                if (sideList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem sideItem in sideList)
                    {
                        string sideContent = sideItem.Content.ToString();
                        string side = string.Empty;
                        if (sideContent == "Bid")
                        {
                            side = "1";
                        }
                        else if (sideContent == "Ofr")
                        {
                            side = "2";
                        }
                        if (!sideFilterList.Contains(side))
                        {
                            sideFilterList.Add(side);
                        }

                    }
                }
            }

            roleFilterList.Clear();
            if (filterDic != null && filterDic.ContainsKey("Role"))
            {
                IList roleList = filterDic["Role"];
                if (roleList.Count > 0)
                {
                    foreach (string roleItem in roleList)
                    {

                        if (roleItem == "All")
                        {
                            break;
                        }

                        if (!roleFilterList.Contains(roleItem))
                        {
                            roleFilterList.Add(roleItem);
                        }

                    }
                }
            }

            if (filterDic != null && filterDic.ContainsKey("IsMe"))
            {
                IList isMeList = filterDic["IsMe"];
                if (isMeList.Count > 0)
                {
                    foreach (string isMeItem in isMeList)
                    {
                        if (isMeItem == "1")
                        {
                            isMeFilter = "1";
                        }
                        else
                        {
                            isMeFilter = "0";
                        }
                    }
                }
            }

            this.ViewSource.View?.Refresh();
        }

        string interest = string.Empty;
        List<string> bondCategoryConditions_Interest = new List<string>();
        /// <summary>
        /// 生成债券类型过滤条件
        /// 利率债 BNC
        /// </summary>
        public void FilterBondType(System.Collections.IList selectedItems)
        {
            interest = string.Empty;
            bondCategoryConditions_Interest.Clear();
            if (InterestRateDebtList != null)
            {
                foreach (FilterListBoxItemData lbi in InterestRateDebtList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "利率债")
                    {
                        bondCategoryConditions_Interest.Clear();
                        interest = "1";
                        break;
                    }
                    bondCategoryConditions_Interest.Add(lbi.itemText);
                }
            }
            this.ViewSource.View?.Refresh();
        }

        string credit = string.Empty;
        List<string> bondCategoryConditions_Credit = new List<string>();
        /// <summary>
        /// 生成信用债过滤条件
        /// 不是利率债BNC的都归为信用债（信用债:BCO，ABS，NCD）
        /// </summary>
        public void FilterCreditBondType()
        {
            credit = string.Empty;
            bondCategoryConditions_Credit.Clear();
            if (CreditBondList != null)
            {
                foreach (FilterListBoxItemData lbi in CreditBondList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "信用债")
                    {
                        bondCategoryConditions_Credit.Clear();
                        credit = "1";
                        break;
                    }

                    if (lbi.itemText.ToString() == "其他")
                    {
                        bondCategoryConditions_Credit.Add("");
                    }
                    else
                    {
                        bondCategoryConditions_Credit.Add(lbi.itemText);
                    }
                }
            }
            this.ViewSource.View?.Refresh();
        }

        List<int[]> RemainingTimeConditions = new List<int[]>();
        /// <summary>
        /// 生成剩余时间过滤条件
        /// </summary>
        public void FilterRemainingTime()
        {
            RemainingTimeConditions.Clear();
            if (RemaingingTimeList != null)
            {
                foreach (FilterListBoxItemData lbi in RemaingingTimeList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        RemainingTimeConditions.Clear();
                        break;
                    }


                    if (lbi.itemText == "0~1Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 0, 1 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= 0 and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 1})");
                    }
                    else if (lbi.itemText == "1~3Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 1, 3 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 1} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 3})");
                    }
                    else if (lbi.itemText == "3~5Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 3, 5 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 3} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 5})");
                    }
                    else if (lbi.itemText == "5~7Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 5, 7 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 5} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 7})");
                    }
                    else if (lbi.itemText == "7~10Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 7, 10 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 7} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 10})");
                    }
                    else if (lbi.itemText == ">10Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 10 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 10})");
                    }
                }
            }
            this.ViewSource.View?.Refresh();
        }

        List<string> rateOptions = new List<string>() { "AAA+", "AAA", "AA+", "AA", "AA-", "A+" };
        List<string> ratingConditions = new List<string>();
        /// <summary>
        /// 主体评级过滤条件
        /// </summary>
        public void FilterRating()
        {
            ratingConditions.Clear();
            if (RatingList != null)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;
                foreach (FilterListBoxItemData lbi in RatingList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        ratingConditions.Clear();
                        break;
                    }

                    ratingConditions.Add(lbi.itemText);
                    //switch (lbi.itemText)
                    //{
                    //    case "AAA+":
                    //    case "AAA":
                    //    case "AA+":
                    //    case "AA":
                    //    case "AA-":
                    //    case "A+":
                    //        AddListFilter(inFilterSB, $"'{lbi.itemText}'");
                    //        break;
                    //    case "其他":
                    //        notInFilter = $"{MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} not in ('AAA+','AAA','AA+','AA','AA-','A+') or {MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} is null";
                    //        break;
                    //}
                }
            }
            this.ViewSource.View?.Refresh();
        }

        private void SetFilterDatas()
        {
            this.ViewSource.View.Filter = x => {
                InnerOrderData data = x as InnerOrderData;

                if (string.IsNullOrEmpty(this.SearchText) == false 
                    && (data.SecurityID == null || !data.SecurityID.Contains(this.SearchText)) 
                    && (data.Symbol == null || !data.Symbol.Contains(this.SearchText))
                    && (data.RoleName == null || !data.RoleName.Contains(this.SearchText))
                    && (data.PartySubName == null || !data.PartySubName.Contains(this.SearchText)))
                    return false;

                if (firmFlagFilterList != null && firmFlagFilterList.Count > 0 && !firmFlagFilterList.Contains(data.FirmFlag))
                    return false;

                if (settleTypeFilterList != null && settleTypeFilterList.Count > 0 && !(settleTypeFilterList.Contains(data.SettleType) || (settleTypeFilterList.Contains("4") && data.SettleType.Contains("+"))))
                    return false;

                if (sideFilterList != null && sideFilterList.Count > 0 && !sideFilterList.Contains(data.Side))
                    return false;

                if (roleFilterList.Count == 0 || (roleFilterList != null && data.RoleId != null && roleFilterList.Count > 0 && !roleFilterList.Contains(data.RoleId)))
                    return false;

                if (!((interest == "1" && data.SecurityType == "BNC")
                   || (bondCategoryConditions_Interest.Count > 0 && bondCategoryConditions_Interest.Contains(data.BondCategory) && data.SecurityType == "BNC")
                   || (credit == "1" && data.SecurityType != "BNC")
                   || (bondCategoryConditions_Credit.Count > 0 && bondCategoryConditions_Credit.Contains(data.BondCategory) && data.SecurityType != "BNC")
                   || (bondCategoryConditions_Interest.Count == 0 && bondCategoryConditions_Credit.Count == 0 && string.IsNullOrEmpty(interest) && string.IsNullOrEmpty(credit))))
                    return false;

                if (RemainingTimeConditions.Count > 0)
                {
                    bool flag = false;
                    foreach (int[] range in RemainingTimeConditions)
                    {
                        if (range.Length == 2)
                        {
                            if (data.RemaingingTime_convert >= range[0] && data.RemaingingTime_convert < range[1])
                            {
                                flag = true;
                                break;
                            }
                        }
                        else if (range.Length == 1)
                        {
                            if (data.RemaingingTime_convert >= range[0])
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                        return false;
                }
                if (ratingConditions.Count > 0)
                {
                    bool flag = false;
                    if (ratingConditions.Contains(data.IssuerRatingCurrentNPY))
                    {
                        flag = true;
                    }
                    else if (ratingConditions.Contains("其他") && (string.IsNullOrEmpty(data.IssuerRatingCurrentNPY) || !rateOptions.Contains(data.IssuerRatingCurrentNPY)))
                    {
                        flag = true;
                    }
                    if (!flag)
                        return false;
                }

                if (isMeFilter == "1" && data.IsMe == false)
                {
                    return false;
                }

                return true;
            };
        }

        private void SetFilterDatas1()
        {
            this.ViewSource.View.Filter = x => {
                InnerOrderData data = x as InnerOrderData;

                if ((this.DisplayMode == "BNC" && data.SecurityType != "BNC" && data.BondSubType != "PSB")
                    || (string.IsNullOrEmpty(this.DisplayMode) == false && this.DisplayMode != "BNC" && data.SecurityType == "BNC" && data.BondSubType != "PSB")
                    || (this.DisplayMode == "BNC" && data.BondSubType == "PSB"))
                    return false;

                if (string.IsNullOrEmpty(this.SearchText) == false
                    && (data.SecurityID == null || !data.SecurityID.Contains(this.SearchText))
                    && (data.Symbol == null || !data.Symbol.Contains(this.SearchText))
                    && (data.RoleName == null || !data.RoleName.Contains(this.SearchText))
                    && (data.PartySubName == null || !data.PartySubName.Contains(this.SearchText))
                    && (data.IssuerName == null || !data.IssuerName.Contains(this.SearchText)))
                    return false;

                if (IsMe == true && data.IsMe == false)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(this.StartTermDY) && data.TermDays < CommonUtils.GetDays(this.StartTermDY))
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(this.EndTermDY) && data.TermDays > CommonUtils.GetDays(this.EndTermDY))
                {
                    return false;
                }

                if (this.IsPrice)
                {
                    if (!data.Price.HasValue)
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(this.StartPrice) && data.Price < this.StartPrice.ConvertToDecimal())
                    {
                        return false;
                    }
                    if (!string.IsNullOrEmpty(this.EndPrice) && data.Price > this.EndPrice.ConvertToDecimal())
                    {
                        return false;
                    }
                }

                bool result = true;

                if (data.Sign != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic)
                    {
                        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }

                if (data.Sign1 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic1)
                    {
                        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }

                if (data.Sign2 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic2)
                    {
                        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }

                if (data.Sign3 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic3)
                    {
                        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
                }


                if (result)
                {
                    if (BaseHandler.Instance.GetQuoteTagDic() != null)
                    {
                        foreach (QuoteTag qt in BaseHandler.Instance.GetQuoteTagDic().Values)
                        {
                            if (this._focusTagDic != null && this._focusTagDic.ContainsKey(qt.tagType) && this._focusTagDic[qt.tagType].Count != 0)
                            {
                                if (qt.signField == "sign" && data.Sign == 0)
                                {
                                    return false;
                                }
                                else if (qt.signField == "sign1" && data.Sign1 == 0)
                                {
                                    return false;
                                }
                                else if (qt.signField == "sign2" && data.Sign2 == 0)
                                {
                                    return false;
                                }
                                else if (qt.signField == "sign3" && data.Sign3 == 0)
                                {
                                    return false;
                                }
                            }

                        }
                    }
                }
                else
                {
                    return false;
                }

                return result;
            };
        }

        #endregion

        #region 最优过滤--清算速度，整，意向
        private List<string> firmFlagFilterList_Best = new List<string>();
        private List<string> settleTypeFilterList_Best = new List<string>();
        private List<string> sideFilterList_Best = new List<string>();
        private List<string> roleFilterList_Best = new List<string>();
        private string isMeFilter_Best = "0";

        internal void FilterData_Best(Dictionary<string, IList> filterDic)
        {
            firmFlagFilterList_Best.Clear();
            if (filterDic != null && filterDic.ContainsKey("FirmFlag"))
            {
                IList firmTypeList = filterDic["FirmFlag"];
                if (firmTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem firmType in firmTypeList)
                    {
                        string firmContent = firmType.Content.ToString();
                        string firmflag = string.Empty;
                        if (firmContent == "firm")
                        {
                            firmflag = "1";
                        }
                        else if (firmContent == "nonfirm")
                        {
                            firmflag = "0";
                        }
                        else
                        {
                            firmflag = INTENTION_FLAG;
                        }
                        if (!firmFlagFilterList_Best.Contains(firmflag))
                        {
                            firmFlagFilterList_Best.Add(firmflag);
                        }

                    }
                }
            }

            settleTypeFilterList_Best.Clear();
            if (filterDic != null && filterDic.ContainsKey("SettlType"))
            {

                IList settleTypeList = filterDic["SettlType"];
                if (settleTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem settleTypeItem in settleTypeList)
                    {
                        string settleTypeContent = settleTypeItem.Content.ToString();
                        string settleTypeflag = string.Empty;
                        if (settleTypeContent == "T+0")
                        {
                            settleTypeflag = "1";
                        }
                        else if (settleTypeContent == "T+1")
                        {
                            if (!settleTypeFilterList_Best.Contains("2"))
                            {
                                settleTypeFilterList_Best.Add("2");
                            }
                            if (!settleTypeFilterList_Best.Contains("3"))
                            {
                                settleTypeFilterList_Best.Add("3");
                            }
                        }
                        else if (settleTypeContent == "远期")
                        {
                            settleTypeflag = "4";
                        }
                        if (!settleTypeFilterList_Best.Contains(settleTypeflag))
                        {
                            settleTypeFilterList_Best.Add(settleTypeflag);
                        }

                    }
                }
            }

            sideFilterList_Best.Clear();
            if (filterDic != null && filterDic.ContainsKey("Side"))
            {
                IList sideList = filterDic["Side"];
                if (sideList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem sideItem in sideList)
                    {
                        string sideContent = sideItem.Content.ToString();
                        string side = string.Empty;
                        if (sideContent == "Bid")
                        {
                            side = "1";
                        }
                        else if (sideContent == "Ofr")
                        {
                            side = "2";
                        }
                        if (!sideFilterList_Best.Contains(side))
                        {
                            sideFilterList_Best.Add(side);
                        }

                    }
                }
            }

            roleFilterList_Best.Clear();
            if (filterDic != null && filterDic.ContainsKey("Role"))
            {
                IList roleList = filterDic["Role"];
                if (roleList.Count > 0)
                {
                    foreach (string roleItem in roleList)
                    {

                        if (roleItem == "All")
                        {
                            break;
                        }

                        if (!roleFilterList_Best.Contains(roleItem))
                        {
                            roleFilterList_Best.Add(roleItem);
                        }

                    }
                }
            }

            if (filterDic != null && filterDic.ContainsKey("IsMe"))
            {
                IList isMeList = filterDic["IsMe"];
                if (isMeList.Count > 0)
                {
                    foreach (string isMeItem in isMeList)
                    {
                        if (isMeItem == "1")
                        {
                            isMeFilter_Best = "1";
                        }
                        else
                        {
                            isMeFilter_Best = "0";
                        }
                    }
                }
            }
            
            this.ViewSource_Best.View?.Refresh();
        }

        string interest_best = string.Empty;
        List<string> bondCategoryConditions_Interest_Best = new List<string>();
        /// <summary>
        /// 生成债券类型过滤条件
        /// 利率债 BNC
        /// </summary>
        public void FilterBondType_Best(System.Collections.IList selectedItems)
        {
            interest_best = string.Empty;
            bondCategoryConditions_Interest_Best.Clear();
            if (InterestRateDebtList_Best != null)
            {
                foreach (FilterListBoxItemData lbi in InterestRateDebtList_Best)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "利率债")
                    {
                        bondCategoryConditions_Interest_Best.Clear();
                        interest_best = "1";
                        break;
                    }
                    bondCategoryConditions_Interest_Best.Add(lbi.itemText);
                }
            }
            this.ViewSource_Best.View?.Refresh();
        }

        string credit_best = string.Empty;
        List<string> bondCategoryConditions_Credit_Best = new List<string>();
        /// <summary>
        /// 生成信用债过滤条件
        /// 不是利率债BNC的都归为信用债（信用债:BCO，ABS，NCD）
        /// </summary>
        public void FilterCreditBondType_Best()
        {
            credit_best = string.Empty;
            bondCategoryConditions_Credit_Best.Clear();
            if (CreditBondList_Best != null)
            {
                foreach (FilterListBoxItemData lbi in CreditBondList_Best)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "信用债")
                    {
                        bondCategoryConditions_Credit_Best.Clear();
                        credit_best = "1";
                        break;
                    }

                    if (lbi.itemText.ToString() == "其他")
                    {
                        bondCategoryConditions_Credit_Best.Add("");
                    }
                    else
                    {
                        bondCategoryConditions_Credit_Best.Add(lbi.itemText);
                    }
                }
            }
            this.ViewSource_Best.View?.Refresh();
        }

        List<int[]> RemainingTimeConditions_Best = new List<int[]>();
        /// <summary>
        /// 生成剩余时间过滤条件
        /// </summary>
        public void FilterRemainingTime_Best()
        {
            RemainingTimeConditions_Best.Clear();
            if (RemaingingTimeList_Best != null)
            {
                foreach (FilterListBoxItemData lbi in RemaingingTimeList_Best)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        RemainingTimeConditions_Best.Clear();
                        break;
                    }


                    if (lbi.itemText == "0~1Y")
                    {
                        RemainingTimeConditions_Best.Add(new int[] { 0, 1 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= 0 and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 1})");
                    }
                    else if (lbi.itemText == "1~3Y")
                    {
                        RemainingTimeConditions_Best.Add(new int[] { 1, 3 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 1} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 3})");
                    }
                    else if (lbi.itemText == "3~5Y")
                    {
                        RemainingTimeConditions_Best.Add(new int[] { 3, 5 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 3} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 5})");
                    }
                    else if (lbi.itemText == "5~7Y")
                    {
                        RemainingTimeConditions_Best.Add(new int[] { 5, 7 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 5} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 7})");
                    }
                    else if (lbi.itemText == "7~10Y")
                    {
                        RemainingTimeConditions_Best.Add(new int[] { 7, 10 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 7} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 10})");
                    }
                    else if (lbi.itemText == ">10Y")
                    {
                        RemainingTimeConditions_Best.Add(new int[] { 10 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 10})");
                    }
                }
            }
            this.ViewSource_Best.View?.Refresh();
        }

        List<string> rateOptions_best = new List<string>() { "AAA+", "AAA", "AA+", "AA", "AA-", "A+" };
        List<string> ratingConditions_best = new List<string>();
        /// <summary>
        /// 主体评级过滤条件
        /// </summary>
        public void FilterRating_Best()
        {
            ratingConditions_best.Clear();
            if (RatingList_Best != null)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;
                foreach (FilterListBoxItemData lbi in RatingList_Best)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        ratingConditions_best.Clear();
                        break;
                    }

                    ratingConditions_best.Add(lbi.itemText);
                    //switch (lbi.itemText)
                    //{
                    //    case "AAA+":
                    //    case "AAA":
                    //    case "AA+":
                    //    case "AA":
                    //    case "AA-":
                    //    case "A+":
                    //        AddListFilter(inFilterSB, $"'{lbi.itemText}'");
                    //        break;
                    //    case "其他":
                    //        notInFilter = $"{MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} not in ('AAA+','AAA','AA+','AA','AA-','A+') or {MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} is null";
                    //        break;
                    //}
                }
            }
            this.ViewSource_Best.View?.Refresh();
        }


        private void SetFilterDatas_Best()
        {
            this.ViewSource_Best.View.Filter = x => {
                InnerOrderData data = x as InnerOrderData;

                if (string.IsNullOrEmpty(this.Best_SearchText) == false
                    && (data.SecurityID == null || !data.SecurityID.Contains(this.Best_SearchText))
                    && (data.Symbol == null || !data.Symbol.Contains(this.Best_SearchText))
                    && (data.RoleName_InnerBid == null || !data.RoleName_InnerBid.Contains(this.Best_SearchText))
                    && (data.RoleName_InnerOfr == null || !data.RoleName_InnerOfr.Contains(this.Best_SearchText))
                    && (data.PartySubName_InnerBid == null || !data.PartySubName_InnerBid.Contains(this.Best_SearchText))
                    && (data.PartySubName_InnerOfr == null || !data.PartySubName_InnerOfr.Contains(this.Best_SearchText)))
                    return false;

                if (firmFlagFilterList_Best != null && firmFlagFilterList_Best.Count > 0 && !firmFlagFilterList_Best.Contains(data.FirmFlag_InnerBid) && !firmFlagFilterList_Best.Contains(data.FirmFlag_InnerOfr))
                    return false;

                if (settleTypeFilterList_Best != null && settleTypeFilterList_Best.Count > 0
                    && !(settleTypeFilterList_Best.Contains(data.SettleType_InnerBid) || (settleTypeFilterList_Best.Contains("4") && data.SettleType_InnerBid != null && data.SettleType_InnerBid.Contains("+")))
                    && !(settleTypeFilterList_Best.Contains(data.SettleType_InnerOfr) || (settleTypeFilterList_Best.Contains("4") && data.SettleType_InnerOfr != null && data.SettleType_InnerOfr.Contains("+"))))
                    return false;

                if (sideFilterList_Best != null && sideFilterList_Best.Count > 0 &&
                    !((sideFilterList_Best.Contains("1") && data.Count_InnerBid > 0) || (sideFilterList_Best.Contains("2") && data.Count_InnerOfr > 0)))
                    return false;

                if (roleFilterList_Best.Count == 0 || (roleFilterList_Best != null && roleFilterList_Best.Count > 0 && !roleFilterList_Best.Contains(data.RoleId_InnerBid) && !roleFilterList_Best.Contains(data.RoleId_InnerOfr)))
                    return false;

                if (!((interest_best == "1" && data.SecurityType == "BNC")
                   || (bondCategoryConditions_Interest_Best.Count > 0 && bondCategoryConditions_Interest_Best.Contains(data.BondCategory) && data.SecurityType == "BNC")
                   || (credit_best == "1" && data.SecurityType != "BNC")
                   || (bondCategoryConditions_Credit_Best.Count > 0 && bondCategoryConditions_Credit_Best.Contains(data.BondCategory) && data.SecurityType != "BNC")
                   || (bondCategoryConditions_Interest_Best.Count == 0 && bondCategoryConditions_Credit_Best.Count == 0 && string.IsNullOrEmpty(interest_best) && string.IsNullOrEmpty(credit_best))))
                    return false;

                if (RemainingTimeConditions_Best.Count > 0)
                {
                    bool flag = false;
                    foreach (int[] range in RemainingTimeConditions_Best)
                    {
                        if (range.Length == 2)
                        {
                            if (data.RemaingingTime_convert >= range[0] && data.RemaingingTime_convert < range[1])
                            {
                                flag = true;
                                break;
                            }
                        }
                        else if (range.Length == 1)
                        {
                            if (data.RemaingingTime_convert >= range[0])
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                        return false;
                }
                if (ratingConditions_best.Count > 0)
                {
                    bool flag = false;
                    if (ratingConditions_best.Contains(data.IssuerRatingCurrentNPY))
                    {
                        flag = true;
                    }
                    else if (ratingConditions_best.Contains("其他") && (string.IsNullOrEmpty(data.IssuerRatingCurrentNPY) || !rateOptions.Contains(data.IssuerRatingCurrentNPY)))
                    {
                        flag = true;
                    }
                    if (!flag)
                        return false;
                }

                if (isMeFilter_Best == "1" && data.IsMe_InnerBid == false && data.IsMe_InnerOfr == false)
                    return false;

                return true;
            };
        }

        private void SetFilterDatas_Best1()
        {
            this.ViewSource_Best.View.Filter = x => {
                InnerOrderData data = x as InnerOrderData;

                if ((this.Best_DisplayMode == "BNC" && data.SecurityType != "BNC" && data.BondSubType != "PSB")
                    || (string.IsNullOrEmpty(this.Best_DisplayMode) == false && this.Best_DisplayMode != "BNC" && data.SecurityType == "BNC" && data.BondSubType != "PSB")
                    || (this.Best_DisplayMode == "BNC" && data.BondSubType == "PSB"))
                    return false;

                if (string.IsNullOrEmpty(this.Best_SearchText) == false
                    && (data.SecurityID == null || !data.SecurityID.Contains(this.Best_SearchText))
                    && (data.Symbol == null || !data.Symbol.Contains(this.Best_SearchText))
                    && (data.RoleName_InnerBid == null || !data.RoleName_InnerBid.Contains(this.Best_SearchText))
                    && (data.RoleName_InnerOfr == null || !data.RoleName_InnerOfr.Contains(this.Best_SearchText))
                    && (data.PartySubName_InnerBid == null || !data.PartySubName_InnerBid.Contains(this.Best_SearchText))
                    && (data.PartySubName_InnerOfr == null || !data.PartySubName_InnerOfr.Contains(this.Best_SearchText))
                    && (data.IssuerName == null || !data.IssuerName.Contains(this.Best_SearchText)))
                    return false;

                if (IsMe_Best == true && data.IsMe_InnerBid == false && data.IsMe_InnerOfr == false)
                    return false;

                if (IsMe_Best == true && _focusTagDic_Best.ContainsKey(Utils.Constants.SIDE)
                    && _focusTagDic_Best[Utils.Constants.SIDE] != null && _focusTagDic_Best[Utils.Constants.SIDE].Count > 0
                    && !((_focusTagDic_Best[Utils.Constants.SIDE].Contains("0001") && data.IsMe_InnerBid == true)
                        || (_focusTagDic_Best[Utils.Constants.SIDE].Contains("0010") && data.IsMe_InnerOfr == true)))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(this.StartTermDY_Best) && data.TermDays < CommonUtils.GetDays(this.StartTermDY_Best))
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(this.EndTermDY_Best) && data.TermDays > CommonUtils.GetDays(this.EndTermDY_Best))
                {
                    return false;
                }

                bool result_InnerBid = true;
                bool result_InnerOfr = true;
                if (HasPrice_Best)
                {
                    result_InnerBid = data.Count_InnerBid > 0;
                    result_InnerOfr = data.Count_InnerOfr > 0;
                }
                else
                {
                    if (data.Count_InnerBid > 0 || data.Count_InnerOfr > 0)
                    {
                        result_InnerBid = data.Count_InnerBid > 0;
                        result_InnerOfr = data.Count_InnerOfr > 0;
                    }
                }

                if (this.IsPrice_Best)
                {
                    result_InnerBid = result_InnerBid
                    && ((data.Price_InnerBid.HasValue == false && string.IsNullOrEmpty(this.StartPrice_Best) && string.IsNullOrEmpty(this.EndPrice_Best))
                    || (data.Price_InnerBid.HasValue 
                    && (string.IsNullOrEmpty(this.StartPrice_Best) || (!string.IsNullOrEmpty(this.StartPrice_Best) && data.Price_InnerBid >= this.StartPrice_Best.ConvertToDecimal()))
                    && (string.IsNullOrEmpty(this.EndPrice_Best) || (!string.IsNullOrEmpty(this.EndPrice_Best) && data.Price_InnerBid <= this.EndPrice_Best.ConvertToDecimal()))));

                    result_InnerOfr = result_InnerOfr
                    && ((data.Price_InnerOfr.HasValue == false && string.IsNullOrEmpty(this.StartPrice_Best) && string.IsNullOrEmpty(this.EndPrice_Best))
                    || (data.Price_InnerOfr.HasValue 
                    && (string.IsNullOrEmpty(this.StartPrice_Best) || (!string.IsNullOrEmpty(this.StartPrice_Best) && data.Price_InnerOfr >= this.StartPrice_Best.ConvertToDecimal()))
                    && (string.IsNullOrEmpty(this.EndPrice_Best) || (!string.IsNullOrEmpty(this.EndPrice_Best) && data.Price_InnerOfr <= this.EndPrice_Best.ConvertToDecimal()))));


                    if (!(result_InnerBid || result_InnerOfr))
                        return false;
                }

                
                if ((data.Sign_InnerBid != 0 || data.Sign_InnerOfr != 0 || data.Sign_InnerBid1 != 0 || data.Sign_InnerOfr1 != 0 || data.Sign_InnerBid2 != 0 || data.Sign_InnerOfr2 != 0 || data.Sign_InnerBid3 != 0 || data.Sign_InnerOfr3 != 0) && this._focusTagDic_Best != null)
                {
                    if (result_InnerBid)
                    {
                        if (data.SignDic_InnerBid != null)
                        {
                            foreach (KeyValuePair<string, string> pair in data.SignDic_InnerBid)
                            {
                                CheckSign(data, pair.Key, pair.Value, ref result_InnerBid, this._focusTagDic_Best, "1");

                                if (!result_InnerBid)
                                    break;
                            }
                        }
                        else
                        {
                            //result_InnerBid = false;
                        }
                    }

                    if (result_InnerBid)
                    {
                        if (data.SignDic_InnerBid1 != null)
                        {
                            foreach (KeyValuePair<string, string> pair in data.SignDic_InnerBid1)
                            {
                                CheckSign(data, pair.Key, pair.Value, ref result_InnerBid, this._focusTagDic_Best, "1");

                                if (!result_InnerBid)
                                    break;
                            }
                        }
                        else
                        {
                            //result_InnerBid = false;
                        }
                    }

                    if (result_InnerBid)
                    {
                        if (data.SignDic_InnerBid2 != null)
                        {
                            foreach (KeyValuePair<string, string> pair in data.SignDic_InnerBid2)
                            {
                                CheckSign(data, pair.Key, pair.Value, ref result_InnerBid, this._focusTagDic_Best, "1");

                                if (!result_InnerBid)
                                    break;
                            }
                        }
                        else
                        {
                            //result_InnerBid = false;
                        }
                    }

                    if (result_InnerBid)
                    {
                        if (data.SignDic_InnerBid3 != null)
                        {
                            foreach (KeyValuePair<string, string> pair in data.SignDic_InnerBid3)
                            {
                                CheckSign(data, pair.Key, pair.Value, ref result_InnerBid, this._focusTagDic_Best, "1");

                                if (!result_InnerBid)
                                    break;
                            }
                        }
                        else
                        {
                            //result_InnerBid = false;
                        }
                    }

                    if (result_InnerOfr)
                    {
                        if (data.SignDic_InnerOfr != null)
                        {
                            foreach (KeyValuePair<string, string> pair in data.SignDic_InnerOfr)
                            {
                                CheckSign(data, pair.Key, pair.Value, ref result_InnerOfr, this._focusTagDic_Best, "2");

                                if (!result_InnerOfr)
                                    break;
                            }
                        }
                        else
                        {
                            //result_InnerOfr = false;
                        }
                    }

                    if (result_InnerOfr)
                    {
                        if (data.SignDic_InnerOfr1 != null)
                        {
                            foreach (KeyValuePair<string, string> pair in data.SignDic_InnerOfr1)
                            {
                                CheckSign(data, pair.Key, pair.Value, ref result_InnerOfr, this._focusTagDic_Best, "2");

                                if (!result_InnerOfr)
                                    break;
                            }
                        }
                        else
                        {
                            //result_InnerOfr = false;
                        }
                    }

                    if (result_InnerOfr)
                    {
                        if (data.SignDic_InnerOfr2 != null)
                        {
                            foreach (KeyValuePair<string, string> pair in data.SignDic_InnerOfr2)
                            {
                                CheckSign(data, pair.Key, pair.Value, ref result_InnerOfr, this._focusTagDic_Best, "2");

                                if (!result_InnerOfr)
                                    break;
                            }
                        }
                        else
                        {
                            //result_InnerOfr = false;
                        }
                    }

                    if (result_InnerOfr)
                    {
                        if (data.SignDic_InnerOfr3 != null)
                        {
                            foreach (KeyValuePair<string, string> pair in data.SignDic_InnerOfr3)
                            {
                                CheckSign(data, pair.Key, pair.Value, ref result_InnerOfr, this._focusTagDic_Best, "2");

                                if (!result_InnerOfr)
                                    break;
                            }
                        }
                        else
                        {
                            //result_InnerOfr = false;
                        }
                    }

                    //return result_InnerBid || result_InnerOfr;
                }

                if (result_InnerBid || result_InnerOfr)
                {
                    if (BaseHandler.Instance.GetQuoteTagDic() != null)
                    {
                        foreach (QuoteTag qt in BaseHandler.Instance.GetQuoteTagDic().Values)
                        {
                            if (this._focusTagDic_Best != null && this._focusTagDic_Best.ContainsKey(qt.tagType) && this._focusTagDic_Best[qt.tagType].Count != 0)
                            {
                                if (qt.signField == "sign" && data.Sign_InnerBid == 0 && data.Sign_InnerOfr == 0)
                                {
                                    return false;
                                }
                                else if (qt.signField == "sign1" && data.Sign_InnerBid1 == 0 && data.Sign_InnerOfr1 == 0)
                                {
                                    return false;
                                }
                                else if (qt.signField == "sign2" && data.Sign_InnerBid2 == 0 && data.Sign_InnerOfr2 == 0)
                                {
                                    return false;
                                }
                                else if (qt.signField == "sign3" && data.Sign_InnerBid3 == 0 && data.Sign_InnerOfr3 == 0)
                                {
                                    return false;
                                }
                            }
                            
                        }
                    }
                }
                else
                {
                    return false;
                }
                return true;
            };
        }

        private void CheckSign(InnerOrderData data, string key, string value, ref bool result, Dictionary<string,List<string>> focusTagDic, string type = null)
        {
            if (key == Utils.Constants.DQTYPE)
            {
                string tempDqtype = value;

                bool isme = false;
                if (type == "1")
                {
                    isme = data.IsMe_InnerBid;
                }
                else if (type == "2")
                {
                    isme = data.IsMe_InnerOfr;
                }
                else
                {
                    isme = data.IsMe;
                }

                if (_innerType == "2" && isme == false)
                {
                    tempDqtype = "0000";
                }
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(tempDqtype));
            }
            else if (key == Utils.Constants.ORGTYPE)
            {
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value) || (focusTagDic[key].Contains("0000000000000001") && value == "0000000000000000"));
            }
            else if (key == Utils.Constants.BOND_SUB_TYPE)
            {
                int bondSubTypeCount = 0;
                if (focusTagDic.ContainsKey(key))
                {
                    List<string> subBondTypeList = focusTagDic[key].ToList();
                    subBondTypeList.RemoveAll(item => item == "1011" || item == "1100" || item == "1101" || item == "1110" || item == "1111");
                    bondSubTypeCount = subBondTypeList.Count;
                }

                result = result
                    && (!focusTagDic.ContainsKey(key) || bondSubTypeCount == 0 || bondSubTypeCount == focusTagDic[key].Count)
                    && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value))
                    && (!focusTagDic.ContainsKey(Utils.Constants.BONDTYPE) || focusTagDic[Utils.Constants.BONDTYPE].Count == 0 || !focusTagDic.ContainsKey(key) || focusTagDic[Utils.Constants.BONDTYPE].Count == focusTagDic[key].Count);
            }
            else
            {
                //result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(InnerBoardFilterPanel_New.ALL) || focusTagDic[key].Contains(value));
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value));
            }
        }
        #endregion 




        /// <summary>
        /// 打开tradebook界面
        /// </summary>
        public void OpenTradeBook(InnerOrderData item)
        {
            if (item != null)
            {
                // TradeBookView view = new TradeBookView();
                // QuotationListViewModel viewModel = new QuotationListViewModel();
                // viewModel.SecurityID = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
                // viewModel.Symbol = CommonUtils.ConvertToString(this.SelectedItem[SYMBOL_FIELD]);
                // viewModel.FullName = CommonUtils.ConvertToString(this.SelectedItem[SYMBOL_FIELD]);//TODO： 这个还没有
                // view.DataContext = viewModel;

                //TabPageHelper.OpenTabPage(viewModel.SecurityID, view, -1);

                // 使用插件形式打开深度行情页面
                string securityID = CommonUtils.ConvertToString(item.SecurityID);
                int tradeBookPluginKey = (int)PluginKeyEnum.TradeBookPlugin;

                IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(tradeBookPluginKey);
                if (plugin != null)
                {
                    plugin.PluginData = securityID;
                    AppUtils.Application.PluginManager.EnablePlugin(tradeBookPluginKey);
                    plugin.PluginData = null;
                    // pluginKey是22，表示TradeBook插件， 可以多次打开
                    // 到load方法中判断 是否需要新建页面
                    if (plugin.PluginKey == tradeBookPluginKey)
                    {
                        plugin.Enabled = false;
                    }
                }
            }
        }

        internal IEnumerable<InnerOrderData> FindInnerOrderDatas()
        {
            IEnumerable<InnerOrderData> flashDatas = this.InnerOrders.Where(item => item.FlashCount > 0 || item.bestbidFlashCount > 0 || item.bestofrFlashCount > 0);
            return flashDatas;
        }

        internal IEnumerable<InnerOrderData> FindInnerOrderDatas_Best()
        {
            IEnumerable<InnerOrderData> flashDatas = this.InnerOrders_Best.Where(item => item.bestofrFlashCount > 0 || item.bestofrFlashCount > 0);
            return flashDatas;
        }

        public const string MODULEID_CLIENT = "5003";
        public const string TYPE1 = "500301";
        public const string TYPE2 = "500302";
        public const string TYPE3 = "500303";

        private string GenClient(Dictionary<string, string> dic, EmsOrderReport er, bool isMe)
        {
            string client = string.Empty;
            if (type3_flag || isMe)
            {
                if (!string.IsNullOrEmpty(er.institutionname)
                    || !string.IsNullOrEmpty(er.tradername))
                {
                    client = $"{er.institutionname}-{er.tradername}";
                }
            }
            else if (type1_flag)
            {
                if (dic != null)
                {
                    if (dic.ContainsKey(Utils.Constants.ORGTYPE))
                    {
                        client = CommonUtils.GetTagNameByCode(dic[Utils.Constants.ORGTYPE], Utils.Constants.ORGTYPE);
                    }
                    if (dic.ContainsKey(Utils.Constants.VIP))
                    {
                        if (dic[Utils.Constants.VIP] == "0001")
                        {
                            client = client + "(vip)";
                        }
                    }
                }
            }
            else if (type2_flag)
            {
                client = er.institutionname;
            }
            
            return client;
        }

        private string GenBondCategory(Dictionary<string, string> singDic1)
        {
            string client = string.Empty;
            string localGovBondTypeDefault = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Utils.Constants.BOND_SUB_TYPE))
                {
                    client = CommonUtils.GetTagNameByCode(singDic1[Utils.Constants.BOND_SUB_TYPE], Utils.Constants.BOND_SUB_TYPE);
                }

                if (singDic1.ContainsKey(Utils.Constants.LOCAL_GOV_BOND_TYPE))
                {
                    localGovBondTypeDefault = CommonUtils.GetTagNameByCode(singDic1[Utils.Constants.LOCAL_GOV_BOND_TYPE], Utils.Constants.LOCAL_GOV_BOND_TYPE).Replace("债","");
                }

                if (!string.IsNullOrEmpty(localGovBondTypeDefault))
                {
                    client = $"{client}({localGovBondTypeDefault})";
                }
            }

            return client;
        }

        private string GenDQType(Dictionary<string, string> dic, bool isMe)
        {
            string dqtype = string.Empty;

            if (_innerType == "1" || (_innerType == "2" && isMe))
            {
                if (dic != null)
                {
                    if (dic.ContainsKey(Utils.Constants.DQTYPE))
                    {
                        if (dic[Utils.Constants.DQTYPE] == "0010")
                        {
                            dqtype = "!";
                        }
                    }
                }
            }

            return dqtype;
        }

        private string GenRating(Dictionary<string, string> singDic1, Dictionary<string, string> signDic3, string securityID)
        {
            string rating_current = string.Empty;
            if (signDic3 != null)
            {
                if (signDic3.ContainsKey(Utils.Constants.RATING_CURRENT))
                {
                    rating_current = CommonUtils.GetTagNameByCode(signDic3[Utils.Constants.RATING_CURRENT], Utils.Constants.RATING_CURRENT);
                }
            }
            rating_current = string.IsNullOrEmpty(rating_current) ? "--" : rating_current;

            if (rating_current.StartsWith("其他"))
            {
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Rating_Current))
                {
                    rating_current = bondsInfoPo.Rating_Current;
                }
                else
                {
                    rating_current = "--";
                }
            }

            string ranking_type = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Utils.Constants.RANKING_TYPE))
                {
                    ranking_type = CommonUtils.GetTagNameByCode(singDic1[Utils.Constants.RANKING_TYPE], Utils.Constants.RANKING_TYPE);
                }
            }
            ranking_type = string.IsNullOrEmpty(ranking_type) ? "--" : ranking_type;

            if (ranking_type.StartsWith("其他"))
            {
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Issuer_Rating_Current))
                {
                    ranking_type = bondsInfoPo.Issuer_Rating_Current;
                }
                else
                {
                    ranking_type = "--";
                }
            }

            return $"{ranking_type}/{rating_current}";
        }

        private string GenIssuer(Dictionary<string, string> singDic2)
        {
            string issuer = string.Empty;
            if (singDic2 != null)
            {
                if (singDic2.ContainsKey(Utils.Constants.ISSUER))
                {
                    ChildTag ct = CommonUtils.GetIssuer(singDic2[Utils.Constants.ISSUER]);
                    if (ct != null)
                        issuer = ct.childTagName;
                }
            }
            issuer = string.IsNullOrEmpty(issuer) ? string.Empty : issuer;

            return issuer;
        }

        private string GenLevel(Dictionary<string, string> singDic3)
        {
            string level = string.Empty;
            if (singDic3 != null)
            {
                if (singDic3.ContainsKey(Utils.Constants.ADMIN_LEVEL))
                {
                    level = CommonUtils.GetTagNameByCode(singDic3[Utils.Constants.ADMIN_LEVEL], Utils.Constants.ADMIN_LEVEL);
                }
            }

            return level;
        }

        private string GenRegionType(Dictionary<string, string> singDic2)
        {
            string regionType = string.Empty;
            if (singDic2 != null)
            {
                if (singDic2.ContainsKey(Utils.Constants.REGION_TYPE))
                {
                    regionType = CommonUtils.GetTagNameByCode(singDic2[Utils.Constants.REGION_TYPE], Utils.Constants.REGION_TYPE);
                }
            }

            return regionType;
        }

        private string GenEnterpriseType(Dictionary<string, string> singDic1)
        {
            string enterpriseType = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Utils.Constants.ENTERPRISE_TYPE))
                {
                    enterpriseType = CommonUtils.GetTagNameByCode(singDic1[Utils.Constants.ENTERPRISE_TYPE], Utils.Constants.ENTERPRISE_TYPE);
                }
            }

            return enterpriseType;
        }

        private string GenIssueType(Dictionary<string, string> singDic1)
        {
            string issueType = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Utils.Constants.ISSUE_TYPE))
                {
                    issueType = CommonUtils.GetTagNameByCode(singDic1[Utils.Constants.ISSUE_TYPE], Utils.Constants.ISSUE_TYPE);
                }
            }

            return issueType;
        }

        private bool GenIfGuarantee(Dictionary<string, string> signDic1)
        {
            string IfGuarantee = string.Empty;
            if (signDic1 != null)
            {
                if (signDic1.ContainsKey(Utils.Constants.IF_GUARANTEE))
                {
                    IfGuarantee = CommonUtils.GetTagNameByCode(signDic1[Utils.Constants.IF_GUARANTEE], Utils.Constants.IF_GUARANTEE);
                }
            }

            return IfGuarantee == "0001";
        }

        private string GenProduct(Dictionary<string, string> signDic3)
        {
            string product = string.Empty;
            if (signDic3 != null)
            {
                if (signDic3.ContainsKey(Utils.Constants.IRS_PRODUCT_TYPE))
                {
                    product = CommonUtils.GetTagNameByCode(signDic3[Utils.Constants.IRS_PRODUCT_TYPE], Utils.Constants.IRS_PRODUCT_TYPE);
                }
            }

            return product;
        }

        private string GenTerm(Dictionary<string, string> signDic3, string securityID)
        {
            string term = string.Empty;
            int index = securityID.IndexOf("_");
            if (!string.IsNullOrEmpty(securityID) && index >= 0)
            {
                term = securityID.Substring(index + 1);
            }
            else
            {
                if (signDic3 != null)
                {
                    if (signDic3.ContainsKey(Utils.Constants.IRS_TERM))
                    {
                        term = CommonUtils.GetTagNameByCode(signDic3[Utils.Constants.IRS_TERM], Utils.Constants.IRS_TERM);
                    }
                }
            }
            

            return term;
        }

        private string GenType(Dictionary<string, string> signDic3)
        {
            string type = string.Empty;
            if (signDic3 != null)
            {
                if (signDic3.ContainsKey(Utils.Constants.IRS_TYPE))
                {
                    type = CommonUtils.GetTagNameByCode(signDic3[Utils.Constants.IRS_TYPE], Utils.Constants.IRS_TYPE);
                }
            }

            return type;
        }

        private string GetCopyInfo(string qty, EmsOrderReport er)
        {
            string nonfirmStar = string.Empty;
            if (er.firmflag == null || er.firmflag == "0")
            {
                nonfirmStar = "*";
            }
            StringBuilder copyInfo = new StringBuilder();
            string settlTypeDisplay = ComboSourceUtil.Instance.FindEnum(er.settltype, "settletype");
            if (er.settltype != "1" && er.settltype != "2" && er.settltype != "2,3")
            {
                if (copyInfo.Length > 0)
                {
                    copyInfo.Append(",");
                }
                copyInfo.Append(settlTypeDisplay);
            }
            if (er.wholeflag == "1")
            {
                if (copyInfo.Length > 0)
                {
                    copyInfo.Append(",");
                }
                copyInfo.Append("整量");
            }

            string copyText = string.IsNullOrEmpty(qty) ? "--" : qty;
            if(nonfirmStar.Length > 0)
            {
                copyText = $"{copyText}{nonfirmStar}";
            }
            if (copyInfo.Length > 0)
            {
                copyText = $"{copyText}({copyInfo})";
            }
            return copyText;
        }

        internal List<InnerOrderData> GetInnerOrderDatas(Func<InnerOrderData, bool> predicate, IComparer<InnerOrderData> comparer = null)
        {
            IEnumerable<InnerOrderData> dataList = null;
            if (predicate != null)
            {
                lock(_dataLock)
                {
                    dataList = this.InnerOrders.Where(predicate);

                    if (dataList != null && comparer != null)
                    {
                        dataList = dataList.OrderBy(item => item, comparer);
                    }
                }
            }
            return dataList?.ToList();
        }

        internal InnerOrderData SelectedRowBySecurtiyID_Single_Copy(string groupid, string orderid)
        {
            try
            {
                if (InnerOrders.Count > 0)
                {
                    InnerOrderData record = InnerOrders.First(item => item.OrderID == orderid);
                    if (!string.IsNullOrEmpty(groupid))
                    {
                        if (IntentionOldHandler.Instance.GroupList.Count > 0)
                        {
                            this.SelectedItem_MyInnerOrders = record;

                            FilterGroupItem group = IntentionOldHandler.Instance.GroupList.First(item => item.GroupID == groupid);
                            if (this.SelectedItem_Tag != group)
                            {
                                this.SelectedItem_Tag = group;
                            }
                            else
                            {
                                ViewSource.View?.Refresh();
                            }
                        }
                    }
                    else
                    {
                        this.SelectedItem_MyInnerOrders = record;
                        this.SelectedItem_Tag = null;
                    }

                    return record;
                }
            }
            catch (System.InvalidOperationException ex)
            {
                CommonUtils.Prompt2Second("订阅处理", "无可处理数据");
            }
            return null;
        }


        internal InnerOrderData SelectedRowBySecurtiyID_Best_Copy(string groupid, string securityid)
        {
            try
            {
                if (InnerOrders_Best.Count > 0)
                {
                    InnerOrderData record = InnerOrders_Best.First(item => item.SecurityID == securityid);
                    if (!string.IsNullOrEmpty(groupid))
                    {
                        if (IntentionOldHandler.Instance.GroupList.Count > 0)
                        {
                            this.SelectedItem_MyInnerOrders_Best = record;

                            FilterGroupItem group = IntentionOldHandler.Instance.GroupList.First(item => item.GroupID == groupid);
                            if (this.SelectedItem_Tag_Best != group)
                            {
                                this.SelectedItem_Tag_Best = group;
                            }
                            else
                            {
                                ViewSource_Best.View?.Refresh();
                            }
                        }
                    }
                    else
                    {
                        this.SelectedItem_MyInnerOrders_Best = record;
                        this.SelectedItem_Tag_Best = null;
                    }

                    return record;
                }
            }
            catch (System.InvalidOperationException ex)
            {
                CommonUtils.Prompt2Second("订阅处理", "无可处理数据");
            }
            return null;
        }


        internal InnerOrderData SelectedRowBySecurtiyID_Best(string securityid)
        {
            try
            {
                if (InnerOrders_Best.Count > 0)
                {
                    
                    InnerOrderData record = InnerOrders_Best.First(item => item.SecurityID == securityid);
                    this.SelectedItem_MyInnerOrders_Best = record;
                    this.SelectedItem_Tag_Best = null;
                    return record;
                }
            }
            catch(System.InvalidOperationException ex)
            {
                CommonUtils.Prompt2Second("订阅处理", "无可处理数据");
            }
            return null;
        }

        internal void SelectedGroup(string groupid)
        {
            try
            {
                if (IntentionOldHandler.Instance.GroupList.Count > 0)
                {
                    this.SelectedItem_MyInnerOrders = null;
                    this.SelectedItem_MyInnerOrders_Best = null;
                    if (this.ViewSource.View != null)
                    {
                        (this.ViewSource.View as CustomCollectionView<InnerOrderData>).ClearOldSelectedItem();
                    }
                    
                    if (this.ViewSource_Best.View != null)
                    {
                        (this.ViewSource_Best.View as CustomCollectionView<InnerOrderData>).ClearOldSelectedItem();
                    }
                   
                    FilterGroupItem group = IntentionOldHandler.Instance.GroupList.First(item => item.GroupID == groupid);
                    if (this.SelectedItem_Tag != group)
                    {
                        this.SelectedItem_Tag = group;
                    }
                    else
                    {
                        ViewSource.View?.Refresh();
                    }

                    if (this.SelectedItem_Tag_Best != group)
                    {
                        this.SelectedItem_Tag_Best = group;
                    }
                    else
                    {
                        ViewSource_Best.View?.Refresh();
                    }
                }
            }
            catch (System.InvalidOperationException ex)
            {
                CommonUtils.Prompt2Second("订阅处理", "无可处理Tag组");
            }
        }

        internal async void UpdateGroupOne(FilterGroupItem updateItem, bool ifSubscribe)
        {
            // 更新
            QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
            req.action = "2";
            req.id = updateItem.GroupID;
            req.username = req.closeby = LocalEnv.UserInfo.UserName;
            req.groupname = updateItem.GroupName;
            req.ifSubscribe = ifSubscribe ? "1" : "0";
            req.focusType = updateItem.Inf1;
            req.marketindicator = "2";
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
            }

            if (updateItem.IsPrice_Edit && (!string.IsNullOrEmpty(updateItem.StartPrice_Edit) || !string.IsNullOrEmpty(updateItem.EndPrice_Edit)))
            {
                req.priceRangeStr = $"{updateItem.StartPrice_Edit},{updateItem.EndPrice_Edit}";
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

    }
}
