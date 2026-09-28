using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cust;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cust;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Frame.Common;
using STC.InnerBoard.Plugin.View;
using STC.InnerBoard.Plugin.View.Component;
using STC.InnerBoard.Plugin.ViewModel.Comparer;
using STC.InnerBoard.Plugin.ViewModel.Data;
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.InnerBoard.Plugin.ViewModel
{
    public class InnerBoardViewModel : ViewModelBase
    {

        private Visibility loadmaskvisible_Single = Visibility.Collapsed;

        public Visibility SingleLoadMaskVisible
        {
            get
            {
                return loadmaskvisible_Single;
            }

            set
            {
                loadmaskvisible_Single = value;
                this.OnPropertyChanged();
            }
        }


        private Visibility loadmaskvisible_Best = Visibility.Collapsed;

        public Visibility BestLoadMaskVisible
        {
            get
            {
                return loadmaskvisible_Best;
            }

            set
            {
                loadmaskvisible_Best = value;
                this.OnPropertyChanged();
            }
        }


        private ObservableCollection<DescriptionItem> _desSource_Best = new ObservableCollection<DescriptionItem>();

        public ObservableCollection<DescriptionItem> DesSource_Best
        {
            get { return _desSource_Best; }
            set { _desSource_Best = value; this.OnPropertyChanged(); }
        }

        private ObservableCollection<DescriptionItem> _desSource = new ObservableCollection<DescriptionItem>();

        public ObservableCollection<DescriptionItem> DesSource
        {
            get { return _desSource; }
            set { _desSource = value; this.OnPropertyChanged(); }
        }


        bool isPermission_WorkTable = AdminHandler.Instance.CheckPermisionById("4000", "4008");

        public Visibility RecognitionBtnVisibility
        {
            get
            {
                return isPermission_WorkTable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string _IsInnerQtyDetail = "0";

        public string IsInnerQtyDetail
        {
            get
            {
                return _IsInnerQtyDetail;
            }
            set
            {
                _IsInnerQtyDetail = value;
                this.OnPropertyChanged();
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
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();
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

                if (!string.IsNullOrEmpty(value))
                {
                    WriteDBLogHelper.Instance.GenRecord("inner", "004", "搜索框");
                }
            }
        }

        private void SetDesSource(ObservableCollection<DescriptionItem> datas, string key, string value)
        {
            if (key == Utils.Constants.BONDTYPE || key == Utils.Constants.BOND_SUB_TYPE2)
                return;//key = Utils.Constants.BOND_SUB_TYPE;

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
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                if (!string.IsNullOrEmpty(value))
                {
                    WriteDBLogHelper.Instance.GenRecord("inner", "004", "搜索框");
                }
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

        //private ConcurrentDictionary<string, CCDCValuaModel> _symbolCountDic = new ConcurrentDictionary<string, CCDCValuaModel>();

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

        private ObservableCollection<InnerOrderData> _innerOrders = new ObservableCollection<InnerOrderData>();

        public ObservableCollection<InnerOrderData> InnerOrders
        {
            get { return _innerOrders; }
            set { _innerOrders = value; }
        }

        internal ConcurrentDictionary<string, InnerOrderData> _flashDic = new ConcurrentDictionary<string, InnerOrderData>();
        // securityid, orderid, innerOrderData
        internal ConcurrentDictionary<string, ConcurrentDictionary<string, InnerOrderData>> _Cache_InnerOrders_Dic = new ConcurrentDictionary<string, ConcurrentDictionary<string, InnerOrderData>>();
        //********************************************

        //**************** 最优 **********************
        internal object _dataLock_best = new object();
        private CollectionViewSource _viewSource_best = new CollectionViewSource();

        public CollectionViewSource ViewSource_Best
        {
            get { return _viewSource_best; }
            set { _viewSource_best = value; }
        }

        private ObservableCollection<InnerOrderData> _innerOrders_best = new ObservableCollection<InnerOrderData>();
        public ObservableCollection<InnerOrderData> InnerOrders_Best
        {
            get { return _innerOrders_best; }
            set { _innerOrders_best = value; }
        }

        internal ConcurrentDictionary<string, InnerOrderData> _flashDic_best = new ConcurrentDictionary<string, InnerOrderData>();
        internal ConcurrentDictionary<string, InnerOrderData> _Cache_InnerOrders_Dic_Best = new ConcurrentDictionary<string, InnerOrderData>();
        //********************************************


        public ICommand RefreshCommand { get; set; }

        private Timer auteRefreshTimer;//无用

        private bool isPermission = false; //查看机构内部权限

        private static string INTENTION_FLAG = "(需求)";

        //***************浮板报价************************
        private InnerDepthPopupViewModel _innerDepthPopupViewModel;

        public InnerDepthPopupViewModel InnerDepthPopupViewModel
        {
            get
            {
                if (_innerDepthPopupViewModel == null)
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

        private CollectionViewSource _viewSource_tag_best = new CollectionViewSource();
        public CollectionViewSource ViewSource_Tag_Best
        {
            get { return _viewSource_tag_best; }
            set { _viewSource_tag_best = value; }
        }

        private FilterGroupItem _selectedItem_tag;

        public FilterGroupItem SelectedItem_Tag
        {
            get { return _selectedItem_tag; }
            set
            {
                //if (_selectedItem_tag != value)
                {
                    _selectedItem_tag = value;
                    this.OnPropertyChanged();
                    if (value != null)
                    {
                        if (value.Inf1 != "GROUP")
                        {
                            this.StartTermDY = _selectedItem_tag.StartTermDY;
                            this.EndTermDY = _selectedItem_tag.EndTermDY;
                            if (!string.IsNullOrEmpty(_selectedItem_tag.StartTermDY) || !string.IsNullOrEmpty(_selectedItem_tag.EndTermDY))
                            {
                                this.IsAppTerm = _selectedItem_tag.IsAppTerm = true;
                            }
                            else
                            {
                                this.IsAppTerm= _selectedItem_tag.IsAppTerm = false;
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
                            this.IsAppTerm = _selectedItem_tag.IsAppTerm = false;

                            this.StartCoupon = string.Empty;
                            this.EndCoupon = string.Empty;
                            this.IsCoupon = false;

                            this.FocusTags = string.Empty;

                            this.StartPrice = string.Empty;
                            this.EndPrice = string.Empty;

                            this.StartChinaBondValue = string.Empty;
                            this.EndChinaBondValue = string.Empty;

                            this.StartBidBp = string.Empty;
                            this.EndBidBp = string.Empty;

                            this.StartOfrBp = string.Empty;
                            this.EndOfrBp = string.Empty;
                            this.DesSource.Clear();
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

                        this.StartChinaBondValue = string.Empty;
                        this.EndChinaBondValue = string.Empty;

                        this.StartBidBp = string.Empty;
                        this.EndBidBp = string.Empty;

                        this.StartOfrBp = string.Empty;
                        this.EndOfrBp = string.Empty;
                        this.DesSource.Clear();

                        ProcessGroupBonds(null, null);
                    }

                    
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
                this.ViewSource.View?.Refresh();
            }
        }

        private string _startTermDY;

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
                        || (_focusTagDic.ContainsKey(Utils.Constants.TERM) && _focusTagDic[Utils.Constants.TERM].Count == 0))
                    {
                        string text = string.Empty;
                        if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                        {
                            text = $"剩余期限:{StartTermDY}-{EndTermDY}";
                        }
                        SetDesSource(this.DesSource, Utils.Constants.TERM, text);
                    }

                    WriteDBLogHelper.Instance.GenRecord("inner", "018", "剩余期限-开始");
                }
            }
        }

        private string _endTermDY;

        public string EndTermDY
        {
            get { return _endTermDY; }
            set
            {
                if (_endTermDY != value)
                {
                    _endTermDY = value;
                    this.OnPropertyChanged();
                    this.ViewSource.View?.Refresh();

                    if (_focusTagDic == null
                        || (_focusTagDic.ContainsKey(Utils.Constants.TERM) && _focusTagDic[Utils.Constants.TERM].Count == 0))
                    {
                        string text = string.Empty;
                        if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                        {
                            text = $"剩余期限:{StartTermDY}-{EndTermDY}";
                        }
                        SetDesSource(this.DesSource, Utils.Constants.TERM, text);
                    }

                    WriteDBLogHelper.Instance.GenRecord("inner", "018", "剩余期限-结束");
                }
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartCoupon) || !string.IsNullOrEmpty(EndCoupon))
                {
                    text = $"票面:{StartCoupon}-{EndCoupon}";
                }
                SetDesSource(this.DesSource, "Coupon", text);
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartCoupon) || !string.IsNullOrEmpty(EndCoupon))
                {
                    text = $"票面:{StartCoupon}-{EndCoupon}";
                }
                SetDesSource(this.DesSource, "Coupon", text);
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartChinaBondValue) || !string.IsNullOrEmpty(EndChinaBondValue))
                {
                    text = $"中债估值:{StartChinaBondValue}-{EndChinaBondValue}";
                }
                SetDesSource(this.DesSource, "ChinaBondValue", text);
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartChinaBondValue) || !string.IsNullOrEmpty(EndChinaBondValue))
                {
                    text = $"中债估值:{StartChinaBondValue}-{EndChinaBondValue}";
                }
                SetDesSource(this.DesSource, "ChinaBondValue", text);
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartPrice) || !string.IsNullOrEmpty(EndPrice))
                {
                    text = $"报价:{StartPrice}-{EndPrice}";
                }
                SetDesSource(this.DesSource, "Price", text);
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartPrice) || !string.IsNullOrEmpty(EndPrice))
                {
                    text = $"报价:{StartPrice}-{EndPrice}";
                }
                SetDesSource(this.DesSource, "Price", text);
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartBidBp) || !string.IsNullOrEmpty(EndBidBp))
                {
                    text = $"bid-中债:{StartBidBp}-{EndBidBp}";
                }
                SetDesSource(this.DesSource, "BidBp", text);
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartBidBp) || !string.IsNullOrEmpty(EndBidBp))
                {
                    text = $"bid-中债:{StartBidBp}-{EndBidBp}";
                }
                SetDesSource(this.DesSource, "BidBp", text);
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartOfrBp) || !string.IsNullOrEmpty(EndOfrBp))
                {
                    text = $"中债-ofr:{StartOfrBp}-{EndOfrBp}";
                }
                SetDesSource(this.DesSource, "OfrBp", text);
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
                this.ViewSource.View?.Refresh();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartOfrBp) || !string.IsNullOrEmpty(EndOfrBp))
                {
                    text = $"中债-ofr:{StartOfrBp}-{EndOfrBp}";
                }
                SetDesSource(this.DesSource, "OfrBp", text);
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

                if (_focusTagDic != null)
                {
                    foreach (KeyValuePair<string, List<string>> pair in _focusTagDic)
                    {
                        StringBuilder text = new StringBuilder();
                        if (pair.Value != null && pair.Value.Count > 0)
                        {
                            if (pair.Key == Utils.Constants.RANKING_TYPE)
                            {
                                text.Append("主体:");
                            }
                            if (pair.Key == Utils.Constants.RATING_CURRENT)
                            {
                                text.Append("债项:");
                            }
                            else if (pair.Key == Utils.Constants.ISSUER_RATING_CURRENT_CBR)
                            {
                                text.Append("中债资信:");
                            }
                            else if (pair.Key == Utils.Constants.CDC_IMPLITED_RATING)
                            {
                                text.Append("中债隐含:");
                            }
                            else if (pair.Key == Utils.Constants.CSI_IMPLIED_RATING)
                            {
                                text.Append("中证隐含:");
                            }
                        }
                        foreach (string filterValue in pair.Value)
                        {
                            string temp = string.Empty;
                            if (pair.Key == Utils.Constants.BOND_SUB_TYPE && filterValue.StartsWith("x"))
                            {
                                temp = CommonUtils.GetTagNameByCode(filterValue.Substring(1), Utils.Constants.BOND_SUB_TYPE2);
                            }
                            else if (filterValue != "xx")
                            {
                                temp = CommonUtils.GetTagNameByCode(filterValue, pair.Key);
                            }
                            else
                            {
                                temp = "其他";
                            }
                            text.Append(CommonUtils.ConvertDes(pair.Key, temp));
                            text.Append("/");
                        }

                        if (text.Length > 0)
                        {
                            text.Remove(text.Length - 1, 1);
                        }

                        if (pair.Key == Utils.Constants.TERM && text.Length == 0)
                        {
                            if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                            {
                                text.Append($"剩余期限:{StartTermDY}-{EndTermDY}");
                            }
                        }

                        SetDesSource(this.DesSource, pair.Key, text.ToString());
                    }
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

                string text = value ? "我" : string.Empty;
                SetDesSource(this.DesSource, "我", text);
            }
        }

        private bool _isFuzzy;

        public bool IsFuzzy
        {
            get { return _isFuzzy; }
            set
            {
                _isFuzzy = value;
                this.OnPropertyChanged();
                this.ViewSource.View?.Refresh();

                string text = value ? "仅模糊" : string.Empty;
                SetDesSource(this.DesSource, "仅模糊", text);
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

        private FilterGroupItem _selectedItem_tag_best;

        public FilterGroupItem SelectedItem_Tag_Best
        {
            get { return _selectedItem_tag_best; }
            set
            {
                //if (_selectedItem_tag_best != value)
                {
                    _selectedItem_tag_best = value;
                    this.OnPropertyChanged();
                    if (value != null)
                    {
                        if (value.Inf1 != "GROUP")
                        {
                            this.StartTermDY_Best = _selectedItem_tag_best.StartTermDY;
                            this.EndTermDY_Best = _selectedItem_tag_best.EndTermDY;
                            if (!string.IsNullOrEmpty(_selectedItem_tag_best.StartTermDY) || !string.IsNullOrEmpty(_selectedItem_tag_best.EndTermDY))
                            {
                                this.IsAppTerm_Best = _selectedItem_tag_best.IsAppTerm = true;
                            }
                            else
                            {
                                this.IsAppTerm_Best = _selectedItem_tag_best.IsAppTerm = false;
                            }

                            this.FocusTags_Best = _selectedItem_tag_best.FocusTags;

                            this.StartCoupon_Best = _selectedItem_tag_best.StartCoupon;
                            this.EndCoupon_Best = _selectedItem_tag_best.EndCoupon;
                            this.IsCoupon_Best = _selectedItem_tag_best.IsCoupon;

                            this.StartPrice_Best = _selectedItem_tag_best.StartPrice;
                            this.EndPrice_Best = _selectedItem_tag_best.EndPrice;

                            this.StartChinaBondValue_Best = _selectedItem_tag_best.StartChinaBond;
                            this.EndChinaBondValue_Best = _selectedItem_tag_best.EndChinaBond;

                            this.StartBidBp_Best = _selectedItem_tag_best.StartBidBp;
                            this.EndBidBp_Best = _selectedItem_tag_best.EndBidBp;

                            this.StartOfrBp_Best = _selectedItem_tag_best.StartOfrBp;
                            this.EndOfrBp_Best = _selectedItem_tag_best.EndOfrBp;
                        }
                        else
                        {
                            this.StartTermDY_Best = string.Empty;
                            this.EndTermDY_Best = string.Empty;
                            this.IsAppTerm_Best = _selectedItem_tag_best.IsAppTerm = false;

                            this.FocusTags_Best = string.Empty;

                            this.StartCoupon_Best = string.Empty;
                            this.EndCoupon_Best = string.Empty;
                            this.IsCoupon_Best = false;

                            this.StartPrice_Best = string.Empty;
                            this.EndPrice_Best = string.Empty;

                            this.StartChinaBondValue_Best = string.Empty;
                            this.EndChinaBondValue_Best = string.Empty;

                            this.StartBidBp_Best = string.Empty;
                            this.EndBidBp_Best = string.Empty;

                            this.StartOfrBp_Best = string.Empty;
                            this.EndOfrBp_Best = string.Empty;
                        }

                        ProcessGroupBonds_Best(value.Inf1, value.FocusGroupId);
                    }
                    else
                    {
                        this.StartTermDY_Best = string.Empty;
                        this.EndTermDY_Best = string.Empty;
                        this.IsAppTerm_Best = false;

                        this.FocusTags_Best = string.Empty;

                        this.StartCoupon_Best = string.Empty;
                        this.EndCoupon_Best = string.Empty;
                        this.IsCoupon_Best = false;

                        this.StartPrice_Best = string.Empty;
                        this.EndPrice_Best = string.Empty;

                        this.StartChinaBondValue_Best = string.Empty;
                        this.EndChinaBondValue_Best = string.Empty;

                        this.StartBidBp_Best = string.Empty;
                        this.EndBidBp_Best = string.Empty;

                        this.StartOfrBp_Best = string.Empty;
                        this.EndOfrBp_Best = string.Empty;
                        this.DesSource_Best.Clear();
                        ProcessGroupBonds_Best(null, null);
                    }
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
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();
            }
        }

        private string _startTermDY_Best;

        public string StartTermDY_Best
        {
            get { return _startTermDY_Best; }
            set
            {
                if (_startTermDY_Best != value)
                {
                    _startTermDY_Best = value;
                    this.OnPropertyChanged();

                    if (_focusTagDic_Best == null
                        || (_focusTagDic_Best.ContainsKey(Utils.Constants.TERM) && _focusTagDic_Best[Utils.Constants.TERM].Count == 0))
                    {
                        string text = string.Empty;
                        if (!string.IsNullOrEmpty(StartTermDY_Best) || !string.IsNullOrEmpty(EndTermDY_Best))
                        {
                            text = $"剩余期限:{StartTermDY_Best}-{EndTermDY_Best}";
                        }
                        SetDesSource(this.DesSource_Best, Utils.Constants.TERM, text);
                    }

                    WriteDBLogHelper.Instance.GenRecord("inner", "018", "剩余期限-开始");
                }
            }
        }

        private string _endTermDY_Best;

        public string EndTermDY_Best
        {
            get { return _endTermDY_Best; }
            set
            {
                if (_endTermDY_Best != value)

                _endTermDY_Best = value;
                this.OnPropertyChanged();
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                if (_focusTagDic_Best == null
                    || (_focusTagDic_Best.ContainsKey(Utils.Constants.TERM) && _focusTagDic_Best[Utils.Constants.TERM].Count == 0))
                {
                    string text = string.Empty;
                    if (!string.IsNullOrEmpty(StartTermDY_Best) || !string.IsNullOrEmpty(EndTermDY_Best))
                    {
                        text = $"剩余期限:{StartTermDY_Best}-{EndTermDY_Best}";
                    }
                    SetDesSource(this.DesSource_Best, Utils.Constants.TERM, text);
                }
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

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartCoupon_Best) || !string.IsNullOrEmpty(EndCoupon_Best))
                {
                    text = $"票面:{StartCoupon_Best}-{EndCoupon_Best}";
                }
                SetDesSource(this.DesSource_Best, "Coupon", text);
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
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartCoupon_Best) || !string.IsNullOrEmpty(EndCoupon_Best))
                {
                    text = $"票面:{StartCoupon_Best}-{EndCoupon_Best}";
                }
                SetDesSource(this.DesSource_Best, "Coupon", text);
            }
        }

        private string _startChinaBondValue_Best;

        public string StartChinaBondValue_Best
        {
            get { return _startChinaBondValue_Best; }
            set
            {
                _startChinaBondValue_Best = value;
                this.OnPropertyChanged();
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartChinaBondValue_Best) || !string.IsNullOrEmpty(EndChinaBondValue_Best))
                {
                    text = $"中债估值:{StartChinaBondValue_Best}-{EndChinaBondValue_Best}";
                }
                SetDesSource(this.DesSource_Best, "ChinaBondValue", text);
            }
        }

        private string _endChinaBondValue_Best;

        public string EndChinaBondValue_Best
        {
            get { return _endChinaBondValue_Best; }
            set
            {
                _endChinaBondValue_Best = value;
                this.OnPropertyChanged();
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartChinaBondValue_Best) || !string.IsNullOrEmpty(EndChinaBondValue_Best))
                {
                    text = $"中债估值:{StartChinaBondValue_Best}-{EndChinaBondValue_Best}";
                }
                SetDesSource(this.DesSource_Best, "ChinaBondValue", text);
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
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartPrice_Best) || !string.IsNullOrEmpty(EndPrice_Best))
                {
                    text = $"报价:{StartPrice_Best}-{EndPrice_Best}";
                }
                SetDesSource(this.DesSource_Best, "Price", text);
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
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartPrice_Best) || !string.IsNullOrEmpty(EndPrice_Best))
                {
                    text = $"报价:{StartPrice_Best}-{EndPrice_Best}";
                }
                SetDesSource(this.DesSource_Best, "Price", text);
            }
        }


        private string _StartBidBp_Best;

        public string StartBidBp_Best
        {
            get { return _StartBidBp_Best; }
            set {
                _StartBidBp_Best = value;
                this.OnPropertyChanged();
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartBidBp_Best) || !string.IsNullOrEmpty(EndBidBp_Best))
                {
                    text = $"Bid-中债:{StartBidBp_Best}-{EndBidBp_Best}";
                }
                SetDesSource(this.DesSource_Best, "BidBp", text);
            }
        }

        private string _EndBidBp_Best;

        public string EndBidBp_Best
        {
            get { return _EndBidBp_Best; }
            set {
                _EndBidBp_Best = value;
                this.OnPropertyChanged();
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartBidBp_Best) || !string.IsNullOrEmpty(EndBidBp_Best))
                {
                    text = $"Bid-中债:{StartBidBp_Best}-{EndBidBp_Best}";
                }
                SetDesSource(this.DesSource_Best, "BidBp", text);
            }
        }

        private string _StartOfrBp_Best;

        public string StartOfrBp_Best
        {
            get { return _StartOfrBp_Best; }
            set {
                _StartOfrBp_Best = value;
                this.OnPropertyChanged();
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartOfrBp_Best) || !string.IsNullOrEmpty(EndOfrBp_Best))
                {
                    text = $"中债-Ofr:{StartOfrBp_Best}-{EndOfrBp_Best}";
                }
                SetDesSource(this.DesSource_Best, "OfrBp", text);
            }
        }

        private string _EndOfrBp_Best;

        public string EndOfrBp_Best
        {
            get { return _EndOfrBp_Best; }
            set
            {
                _EndOfrBp_Best = value;
                this.OnPropertyChanged();
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartOfrBp_Best) || !string.IsNullOrEmpty(EndOfrBp_Best))
                {
                    text = $"中债-Ofr:{StartOfrBp_Best}-{EndOfrBp_Best}";
                }
                SetDesSource(this.DesSource_Best, "OfrBp", text);
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
                    //this.ViewSource_Best.View?.Refresh();
                    this.FilterBestData();

                    if (_focusTagDic_Best != null)
                    {
                        foreach (KeyValuePair<string, List<string>> pair in _focusTagDic_Best)
                        {
                            StringBuilder text = new StringBuilder();
                            if (pair.Value != null && pair.Value.Count > 0)
                            {
                                if (pair.Key == Utils.Constants.RANKING_TYPE)
                                {
                                    text.Append("主体:");
                                }
                                if (pair.Key == Utils.Constants.RATING_CURRENT)
                                {
                                    text.Append("债项:");
                                }
                                else if (pair.Key == Utils.Constants.ISSUER_RATING_CURRENT_CBR)
                                {
                                    text.Append("中债资信:");
                                }
                                else if (pair.Key == Utils.Constants.CDC_IMPLITED_RATING)
                                {
                                    text.Append("中债隐含:");
                                }
                                else if (pair.Key == Utils.Constants.CSI_IMPLIED_RATING)
                                {
                                    text.Append("中证隐含:");
                                }
                            }

                            foreach (string filterValue in pair.Value)
                            {
                                string temp = string.Empty;
                                if (pair.Key == Utils.Constants.BOND_SUB_TYPE && filterValue.StartsWith("x"))
                                {
                                    temp = CommonUtils.GetTagNameByCode(filterValue.Substring(1), Utils.Constants.BOND_SUB_TYPE2);
                                }
                                else if (filterValue != "xx")
                                {
                                    temp = CommonUtils.GetTagNameByCode(filterValue, pair.Key);
                                }
                                else
                                {
                                    temp = "其他";
                                }
                                text.Append(CommonUtils.ConvertDes(pair.Key, temp));
                                text.Append("/");
                            }
                            if (text.Length > 0)
                            {
                                text.Remove(text.Length - 1, 1);
                            }

                            if (pair.Key == Utils.Constants.TERM && text.Length == 0)
                            {
                                if (!string.IsNullOrEmpty(StartTermDY_Best) || !string.IsNullOrEmpty(EndTermDY_Best))
                                {
                                    text.Append($"剩余期限:{StartTermDY_Best}-{EndTermDY_Best}");
                                }
                            }

                            SetDesSource(this.DesSource_Best, pair.Key, text.ToString());
                        }
                    }
                }
            }
        }
        private Dictionary<string, List<string>> _focusTagDic_Best;

        private bool _isMe_best;

        public bool IsMe_Best
        {
            get { return _isMe_best; }
            set
            {
                _isMe_best = value;
                this.OnPropertyChanged();
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = value ? "我" : string.Empty;
                SetDesSource(this.DesSource_Best, "我", text);
            }
        }

        private bool _isFuzzy_best;

        public bool IsFuzzy_Best
        {
            get { return _isFuzzy_best; }
            set
            {
                _isFuzzy_best = value;
                this.OnPropertyChanged();
                //this.ViewSource_Best.View?.Refresh();
                this.FilterBestData();

                string text = value ? "仅模糊" : string.Empty;
                SetDesSource(this.DesSource_Best, "仅模糊", text);
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

        private int _updateDataCount;

        public int UpdateDataCount
        {
            get { return _updateDataCount; }
            set { _updateDataCount = value; }
        }

        private int _updateDataCount_best;

        public int UpdateDataCount_Best
        {
            get { return _updateDataCount_best; }
            set { _updateDataCount_best = value; }
        }

        private double _verticalOffset = 0;

        public double VerticalOffset
        {
            get { return _verticalOffset; }
            set { _verticalOffset = value; }
        }

        private double _verticalOffset_best = 0;

        public double VerticalOffset_Best
        {
            get { return _verticalOffset_best; }
            set { _verticalOffset_best = value; }
        }

        bool type1_flag;

        public bool Type1_Flag
        {
            get { return type1_flag; }
            set
            {
                type1_flag = value;
                this.OnPropertyChanged();
            }
        }

        bool type2_flag;

        public bool Type2_Flag
        {
            get { return type2_flag; }
            set
            {
                type2_flag = value;
                this.OnPropertyChanged();
            }
        }

        bool type3_flag;

        public bool Type3_Flag
        {
            get { return type3_flag; }
            set
            {
                type3_flag = value;
                this.OnPropertyChanged();
            }
        }

        public InnerBoardView _InnerBoardView;

        public InnerBoardViewModel()
        {
            IsInnerQtyDetail = LocalEnv.ConfInfo.IsInnerQtyDetail;

            if (IntentionOldHandler.Instance.isSALER())
            {
                _innerType = "2";
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

            //if (TestUIThread == null)
            //{
            //    TestUIThread = new System.Threading.Thread(GenTestErOrder);
            //    TestUIThread.IsBackground = true;
            //    TestUIThread.Start();
            //}

            BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);

            BaseHandler.Instance.Init(Instance_OnClear);

            //_lastUpdateTime = $"{DateTimeEx.ServerTime.ToString(dateDateFormatPattern)} 00:00:00.000";

            isPermission = AdminHandler.Instance.CheckPermisionById(CommonConstants.MODELID_5000, CommonConstants.CODE_5002);

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


            //this.ViewSource_Best.View.CollectionChanged += BestView_CollectionChanged;
            //SetFilterDatas_Best();
            //SetFilterDatas_Best1();
            BindingOperations.EnableCollectionSynchronization(this.ViewSource_Best.View, _dataLock_best);
            //this.ViewSource_Best.View?.Refresh();


            BestLoadMaskVisible = Visibility.Collapsed;
            SingleLoadMaskVisible = Visibility.Collapsed;

            RefreshCommand = new ComRelayCommand(InitEmsOrder1);
            IntentionOldHandler.Instance.OnQor += Instance_OnQor;
            IntentionOldHandler.Instance.OnHistoryQor += Instance_OnHistoryQor;

            //LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;

            ViewSource_Tag.Source = IntentionOldHandler.Instance.GroupList;
            ViewSource_Tag_Best.Source = IntentionOldHandler.Instance.GroupList;

            IntentionOldHandler.Instance.RequestQor();

            //作废区查询权限
            bool rootRight = AdminHandler.Instance.CheckPermisionById("5000", "5004");
            bool noRight = AdminHandler.Instance.CheckPermission("5004", "500401");
            IsShowIHR = rootRight ? noRight ? Visibility.Collapsed : Visibility.Visible : Visibility.Collapsed;

            IsShowFuzzy = LocalEnv.ConfInfo.IsShowInnerFuzzyRequire == "1" ? Visibility.Visible : Visibility.Collapsed;

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
            IntentionOldHandler.Instance.OnQor -= Instance_OnQor;
            IntentionOldHandler.Instance.OnHistoryQor -= Instance_OnHistoryQor;

            if (STC.Common.Utils.CommonUtils.IsBDBAppName())
            {
                LastExecutionProcesser.Processer.OnLastExecutions_BDB -= Processer_OnLastExecutions_BDB;
            }
            else
            { 
                LastExecutionProcesser.Processer.OnLastExecutions -= Processer_OnLastExecutions;
            }


            if (auteRefreshTimer != null)
            {
                auteRefreshTimer.Dispose();
            }

            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock(_dataLock)
                    {
                        if (InnerOrders != null)
                        {
                            MyOrderCount = 0;
                            RowCount = 0;
                            BidRowCount = 0;
                            OfrRowCount = 0;
                            InnerOrders.Clear();
                            //InnerOrders = null;
                            //InnerOrders = new CustomCollection<InnerOrderData>();
                        }
                    }

                    lock(_dataLock_best)
                    {
                        if (InnerOrders_Best != null)
                        {
                            RowCount_Best = 0;
                            BidRowCount_Best = 0;
                            OfrRowCount_Best = 0;
                            InnerOrders_Best.Clear();
                            //_innerOrders_best = null;
                            //_innerOrders_best = new CustomCollection<InnerOrderData>();
                        }
                    }

                    _flashDic.Clear();
                    _flashDic_best.Clear();
                    _Cache_InnerOrders_Dic.Clear();
                    _Cache_InnerOrders_Dic_Best.Clear();

                    _lastUpdateTimeDic.Clear();
                    _Temp_Cache_InnerOrders_Dic.Clear();
                    _bestPriceDic.Clear();

                    InterestRateDebtList.Clear();
                    CreditBondList.Clear();
                    RemaingingTimeList.Clear();
                    RatingList.Clear();
                }));
            }
        }


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

        private void Processer_OnLastExecutions(ExecutionReport obj)
        {
            // 市场最新成交
            if (_Cache_InnerOrders_Dic.ContainsKey(obj.SecurityID))
            {
                foreach (InnerOrderData item in _Cache_InnerOrders_Dic[obj.SecurityID].Values)
                {
                    UpdateLastExecutions(item);
                }
            }

            if (_Cache_InnerOrders_Dic_Best.ContainsKey(obj.SecurityID))
            {
                UpdateLastExecutions(_Cache_InnerOrders_Dic_Best[obj.SecurityID]);
            }
        }

        private void UpdateLastExecutions(InnerOrderData innerOrderData)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    if (innerOrderData != null)
                    {
                        ExecutionReport lastExeItem = LastExecutionProcesser.Processer.GetExecutionReportDic(innerOrderData.SecurityID);

                        if (lastExeItem != null && lastExeItem != null)
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
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        private void Processer_OnLastExecutions_BDB(ExecutionReport obj)
        {
            if (obj == null)
                return;

            // 市场最新成交
            if (_Cache_InnerOrders_Dic.ContainsKey(obj.SecurityID))
            {
                foreach (InnerOrderData item in _Cache_InnerOrders_Dic[obj.SecurityID].Values)
                {
                    UpdateLastExecutions_BDB(item);
                }
            }

            if (_Cache_InnerOrders_Dic_Best.ContainsKey(obj.SecurityID))
            {
                UpdateLastExecutions_BDB(_Cache_InnerOrders_Dic_Best[obj.SecurityID]);
            }
        }

        private void UpdateLastExecutions_BDB(InnerOrderData innerOrderData)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    if (innerOrderData != null)
                    {
                        MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem_BDB(innerOrderData.SecurityID);

                        if (lastExeItem != null && lastExeItem != null)
                        {
                            innerOrderData.LastMarketPx = lastExeItem.ExecutePricePercent;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        private void onloadData(bool obj)
        {
            lock (_dataLock)
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
                        //data.RemainingTime = bondsInfoPo.RemaingingTime;
                        data.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        data.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        data.BondSubType = bondsInfoPo.Bond_Subtype;
                        //data.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                        //data.Rating = GenRating(data.SignDic1, data.SignDic3, data.SecurityID);
                        //data.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //data.Issuer = bondsInfoPo.issuer;
                        //data.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        data.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    }
                }
            }

            lock (_dataLock_best)
            {
                var itemList = InnerOrders_Best.Where(item => string.IsNullOrEmpty(item.SecurityType));
                foreach (InnerOrderData data in itemList)
                {
                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(data.SecurityID);
                    if (bondsInfoPo != null)
                    {
                        //Symbol = bondsInfoPo.Short_Name;
                        //Security = bondsInfoPo.Bond_ID;
                        //SymbolName = bondsInfoPo.Short_Name;
                        //data.RemainingTime = bondsInfoPo.RemaingingTime;
                        data.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        data.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        data.BondSubType = bondsInfoPo.Bond_Subtype;
                        //data.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                        //data.Rating = GenRating(data.SignDic1, data.SignDic3, data.SecurityID);
                        //data.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //data.Issuer = bondsInfoPo.issuer;
                        //data.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        data.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    }
                }
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("InnerBoardViewModel Clear");
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
                if (!string.IsNullOrEmpty(LocalEnv.UserInfo.Termids))
                {
                    request.reqParams = new Dictionary<string, string>();
                    request.reqParams["biztypeid"] = LocalEnv.UserInfo.Termids;
                }
               

                if (!isPermission)
                    request.userName = LocalEnv.UserInfo.UserName;
                QueryEmsOrderReplyPo reply = TableOldHandler.Instance.QueryEmsOrder(request);
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
                                catch (Exception e)
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


        private void InitEmsOrder1()
        {
            RefreshBtnEnable = false;
            BestLoadMaskVisible = Visibility.Visible;
            SingleLoadMaskVisible = Visibility.Visible;
            isInit = false;
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (_dataLock)
                    {
                        if (InnerOrders != null)
                        {
                            MyOrderCount = 0;
                            RowCount = 0;
                            BidRowCount = 0;
                            OfrRowCount = 0;
                            InnerOrders.Clear();
                            //InnerOrders = null;
                            //InnerOrders = new CustomCollection<InnerOrderData>();
                        }
                    }

                    lock(_dataLock_best)
                    {
                        if (InnerOrders_Best != null)
                        {
                            RowCount_Best = 0;
                            BidRowCount_Best = 0;
                            OfrRowCount_Best = 0;
                            InnerOrders_Best.Clear();
                            //InnerOrders_Best = null;
                            //InnerOrders_Best = new CustomCollection<InnerOrderData>();
                        }
                    }
                    

                    _flashDic.Clear();
                    _flashDic_best.Clear();
                    _Cache_InnerOrders_Dic.Clear();
                    _Cache_InnerOrders_Dic_Best.Clear();

                    _lastUpdateTimeDic.Clear();
                    _Temp_Cache_InnerOrders_Dic.Clear();
                    _bestPriceDic.Clear();

                    IntentionOldHandler.Instance.RequestQor();
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
                if (!string.IsNullOrEmpty(LocalEnv.UserInfo.Termids))
                {
                    request.reqParams = new Dictionary<string, string>();
                    request.reqParams["biztypeid"] = LocalEnv.UserInfo.Termids;
                }
                if (!isPermission)
                    request.userName = LocalEnv.UserInfo.UserName;
                QueryEmsOrderReplyPo reply = TableOldHandler.Instance.QueryEmsOrder(request);
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
                    //CCDCValuaModel srcccdc = null;
                    //if (_symbolCountDic.ContainsKey(price.SecurityID)
                    //    && (_symbolCountDic[price.SecurityID] == null
                    //        || _symbolCountDic[price.SecurityID].CCDCValuation != price.CCDCValuation))
                    //{
                    //    try
                    //    {
                    //        srcccdc = CommonUtils.GetCCDCValua(price.SecurityID);
                    //        if (srcccdc == null)
                    //        {
                    //            srcccdc = CommonUtils.GenerateCCDCValue(price);

                    //            _symbolCountDic[price.SecurityID] = srcccdc;

                    //            CommonUtils.SetCCDCValua(price.SecurityID, srcccdc);
                    //        }
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        LogHandler.Error(e);
                    //    }
                    //}


                    if (_Cache_InnerOrders_Dic.ContainsKey(price.SecurityID) && _Cache_InnerOrders_Dic[price.SecurityID] != null)
                    {
                        foreach (InnerOrderData findData in _Cache_InnerOrders_Dic[price.SecurityID].Values)
                        {
                            findData.Best_Bid = _bestPriceDic[price.SecurityID][0];
                            findData.Best_Ofr = _bestPriceDic[price.SecurityID][1];
                            //RefreshCCDCValue(findData, price, srcccdc);
                        }
                    }

                    if (_Cache_InnerOrders_Dic_Best.ContainsKey(price.SecurityID))
                    {
                        InnerOrderData findData = _Cache_InnerOrders_Dic_Best[price.SecurityID];

                        if (findData != null)
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
                            //RefreshCCDCValue(findData, price, srcccdc);
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
            }
        }


        /// <summary>
        /// key:securityid; value:bestPrices 0:bestBid 1:bestOfr 
        /// </summary>
        ConcurrentDictionary<string, decimal?[]> _bestPriceDic = new ConcurrentDictionary<string, decimal?[]>();

        ///// <summary>
        ///// key:securityid; value:bestPrices 0:isBestBidChange 1:isBestOfrChange 
        ///// </summary>
        //internal ConcurrentDictionary<string, bool[]>2 _priceChangeDic = new ConcurrentDictionary<string, bool[]>();

        private void GetBestPrice(MarketDataSnapshotFullRefresh price)
        {
            if (!_bestPriceDic.ContainsKey(price.SecurityID))
            {
                _bestPriceDic[price.SecurityID] = new decimal?[2];
                _bestPriceDic[price.SecurityID][0] = null;
                _bestPriceDic[price.SecurityID][1] = null;
            }

            //if (!_priceChangeDic.ContainsKey(price.SecurityID))
            //{
            //    _priceChangeDic[price.SecurityID] = new bool[2];
            //    _priceChangeDic[price.SecurityID][0] = false;
            //    _priceChangeDic[price.SecurityID][1] = false;
            //}

            // 剔除
            var bestPrices_best_bid = price.NoMDEntries?.Where(entry => entry.VenueTypeGW != "INNER" && entry.MDEntryType == "0").OrderBy(entry => entry.MDPriceLevel.ConvertToLong());
            if (bestPrices_best_bid != null && bestPrices_best_bid.Count() > 0)
            {
                GNoMDEntries entry = bestPrices_best_bid.First();
                decimal newBestBidPrice = entry.MDEntryPx.ConvertToDecimal();
                //if (!CommonUtils.IsBDBAppName())
                //{
                //    if (entry.PriceType == "1")
                //    {
                //        newBestBidPrice = entry.NetPrice.ConvertToDecimal();
                //    }
                //    else if (entry.PriceType == "3")
                //    {
                //        newBestBidPrice = entry.Yield.ConvertToDecimal();
                //    }
                //}
                
                if (_bestPriceDic[price.SecurityID][0] != newBestBidPrice)
                {
                    _bestPriceDic[price.SecurityID][0] = newBestBidPrice;
                    //_priceChangeDic[price.SecurityID][0] = true;
                }
            }
            else
            {
                _bestPriceDic[price.SecurityID][0] = null;
            }

            var bestPrices_best_ofr = price.NoMDEntries?.Where(entry => entry.VenueTypeGW != "INNER" && entry.MDEntryType == "1").OrderBy(entry => entry.MDPriceLevel.ConvertToLong());
            if (bestPrices_best_ofr != null && bestPrices_best_ofr.Count() > 0)
            {
                GNoMDEntries entry = bestPrices_best_ofr.First();
                decimal newBestOfrPrice = entry.MDEntryPx.ConvertToDecimal();
                //if (!CommonUtils.IsBDBAppName())
                //{
                //    if (entry.PriceType == "1")
                //    {
                //        newBestOfrPrice = entry.NetPrice.ConvertToDecimal();
                //    }
                //    else if (entry.PriceType == "3")
                //    {
                //        newBestOfrPrice = entry.Yield.ConvertToDecimal();
                //    }
                //}
                if (_bestPriceDic[price.SecurityID][1] != newBestOfrPrice)
                {
                    _bestPriceDic[price.SecurityID][1] = newBestOfrPrice;
                    //_priceChangeDic[price.SecurityID][1] = true;
                }
            }
            else
            {
                _bestPriceDic[price.SecurityID][1] = null;
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
        private void SetdeDefaultSelectedItem()
        {
            if (this.SelectedItem_MyInnerOrders == null && !this.ViewSource.View.IsEmpty)
            {
                this.ViewSource.View.MoveCurrentToFirst();
                this.SelectedItem_MyInnerOrders = this.ViewSource.View.CurrentItem as InnerOrderData;
            }
        }

        private void SetDefaultSelectedItem_Best()
        {
            if (this.SelectedItem_MyInnerOrders_Best == null && !this.ViewSource_Best.View.IsEmpty)
            {
                this.ViewSource_Best.View.MoveCurrentToFirst();
                this.SelectedItem_MyInnerOrders_Best = this.ViewSource_Best.View.CurrentItem as InnerOrderData;
            }
        }
        
        internal volatile bool isInit = false;
        List<EmsOrderReport> realTimeList = new List<EmsOrderReport>();
        private void Instance_OnQor(EmsOrderReport er)
        {
            if (er != null && er.marketindicator != "4")
            {
                return;
            }
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //Console.WriteLine($"realtime 1:{DateTime.Now.ToString("HH:mm:dd.fff")}");
                UpdateInnerOrder(er);
                SetdeDefaultSelectedItem();
                SetDefaultSelectedItem_Best();
                //Console.WriteLine($"realtime 2:{DateTime.Now.ToString("HH:mm:dd.fff")}");
            }));
        }

        private void Instance_OnHistoryQor(List<EmsOrderReport> orders)
        {
            if (orders != null && orders.Count > 0)
            {
                LogHelper.Info($"OnHistoryQor:{orders.Count},{orders[0].updatetime}");

                int batchSize = 500;
                var syncLock = new object(); // 新增集合访问锁

                Task.Run(() =>
                {
                    try
                    {
                        // 获取数据快照
                        List<EmsOrderReport> snapshot;
                        lock (orders)
                        {
                            snapshot = orders.ToList(); // 线程安全拷贝
                        }

                        for (int batchIndex = 0; batchIndex < snapshot.Count; batchIndex += batchSize)
                        {
                            var currentBatch = snapshot.Skip(batchIndex).Take(batchSize).ToList();

                            // 异步更新UI（非阻塞）
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (var data in currentBatch)
                                {
                                    UpdateInnerOrder(data);
                                }

                                SetdeDefaultSelectedItem();
                                SetDefaultSelectedItem_Best();
                            }, System.Windows.Threading.DispatcherPriority.Background); // 调整优先级

                            // 优化CPU释放策略
                            if (batchIndex % (batchSize * 1) == 0 && batchIndex != 0)
                            {
                                System.Threading.Thread.Sleep(10);
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
            else
            {
                RefreshBtnEnable = true;
                BestLoadMaskVisible = Visibility.Collapsed;
                SingleLoadMaskVisible = Visibility.Collapsed;
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
            try
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    SetdeDefaultSelectedItem();
                    lock (_dataLock)
                    {
                        ListCollectionView view = this.ViewSource.View as ListCollectionView;
                        if (view == null)
                        {
                            RowCount = 0;
                            BidRowCount = 0;
                            OfrRowCount = 0;
                        }
                        else
                        {
                            RowCount = view.Count;
                            IEnumerable<InnerOrderData> tempDatas = view.Cast<InnerOrderData>();
                            BidRowCount = tempDatas.Count(p => p.Side == "1");
                            OfrRowCount = tempDatas.Count(p => p.Side == "2");
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    lock (_dataLock)
                    {
                        if (e.NewItems.Count > 0)
                        {
                            foreach (InnerOrderData data in e.NewItems)
                            {
                                RowCount++;
                                if (data.Side == "1")
                                {
                                    BidRowCount++;
                                }
                                if (data.Side == "2")
                                {
                                    OfrRowCount++;
                                }
                            }
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    lock (_dataLock)
                    {
                        if (e.OldItems.Count > 0)
                        {
                            foreach (InnerOrderData data in e.OldItems)
                            {
                                RowCount--;
                                if (data.Side == "1")
                                {
                                    BidRowCount--;
                                }
                                if (data.Side == "2")
                                {
                                    OfrRowCount--;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
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
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                lock (_dataLock_best)
                {
                    ListCollectionView view = this.ViewSource_Best.View as ListCollectionView;
                    if (view == null)
                    {
                        RowCount_Best = 0;
                        BidRowCount_Best = 0;
                        OfrRowCount_Best = 0;
                    }
                    else
                    {
                        RowCount_Best = view.Count;
                        IEnumerable<InnerOrderData> tempDatas = view.Cast<InnerOrderData>();
                        BidRowCount_Best = tempDatas.Count(p => p.Count_InnerBid > 0);
                        OfrRowCount_Best = tempDatas.Count(p => p.Count_InnerOfr > 0);
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                lock (_dataLock_best)
                {
                    if (e.NewItems.Count > 0)
                    {
                        foreach (InnerOrderData data in e.NewItems)
                        {
                            RowCount_Best++;
                            if (data.Count_InnerBid > 0)
                            {
                                BidRowCount_Best++;
                            }
                            if (data.Count_InnerOfr > 0)
                            {
                                OfrRowCount_Best++;
                            }
                        }
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                lock (_dataLock_best)
                {
                    if (e.OldItems.Count > 0)
                    {
                        foreach (InnerOrderData data in e.OldItems)
                        {
                            RowCount_Best--;
                            if (data.Count_InnerBid > 0)
                            {
                                BidRowCount_Best--;
                            }
                            if (data.Count_InnerOfr > 0)
                            {
                                OfrRowCount_Best--;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        private System.Threading.Thread RefreshUIThread = null;//刷新ui线程
        private object _tempRealTimeOrderListLock = new object();
        private List<EmsOrderReport> tempRealTimeOrderList = new List<EmsOrderReport>();
        private System.Threading.AutoResetEvent _autoResetEvent = new System.Threading.AutoResetEvent(false);

        private System.Threading.Thread TestUIThread = null;

        private void GenTestErOrder()
        {
            while (true)
            {
                EmsOrderReport er = new EmsOrderReport();
                er.side = "1";
                er.orderid = Guid.NewGuid().ToString();
                er.securityid = er.orderid;
                er.orderstatus = "1";
                er.firmflag = "0";
                er.createtime = er.updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                lock (_tempRealTimeOrderListLock)
                {
                    tempRealTimeOrderList.Add(er);
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        

        private void ProcessInnerOrder_RealTime()
        {
            while(true)
            {
                if (isFilterBestData)
                {
                    isFilterBestData = false;
                    // 处理过滤
                    ProcessFilterBestData();
                }

                System.Threading.Thread.Sleep(500);
            }
        }

        private void UpdateInnerOrder(EmsOrderReport er)
        {
            //if (_lastUpdateTime.CompareTo(er.updatetime) < 0)
            //{
            //    _lastUpdateTime = er.updatetime;
            //}

            string updateTime_temp = er.updatetime.Length > 11 ? er.updatetime.Substring(11, 8) : (er.updatetime.Length > 8 ? er.updatetime.Substring(0, 8) : er.updatetime);
            string updateTime = string.Empty;
            if (!_lastUpdateTimeDic.ContainsKey(er.securityid))
            {
                _lastUpdateTimeDic[er.securityid] = string.Empty;
            }
            if (_lastUpdateTimeDic[er.securityid].CompareTo(updateTime_temp) < 0)
            {
                _lastUpdateTimeDic[er.securityid] = updateTime_temp;
            }

            InnerOrderData findData = null;
            if (_Cache_InnerOrders_Dic.ContainsKey(er.securityid) && _Cache_InnerOrders_Dic[er.securityid] != null
                && _Cache_InnerOrders_Dic[er.securityid].ContainsKey(er.orderid) && _Cache_InnerOrders_Dic[er.securityid][er.orderid] != null)
            {
                findData = _Cache_InnerOrders_Dic[er.securityid][er.orderid];
            }

            if (findData != null)
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
                        //if (_symbolCountDic.ContainsKey(er.securityid) && !InnerOrders.Any(item => item.SecurityID == er.securityid))
                        //{
                        //    PriceHandler.Instance.UnregisterCTPrice(er.securityid, UpdateCtPrice);
                        //    CCDCValuaModel removeCcdcValueModel;
                        //    _symbolCountDic.TryRemove(er.securityid, out removeCcdcValueModel);
                        //}

                        if (_Cache_InnerOrders_Dic.ContainsKey(er.securityid))
                        {
                            ConcurrentDictionary<string, InnerOrderData> removeDic = _Cache_InnerOrders_Dic[er.securityid];

                            if (removeDic != null && removeDic.ContainsKey(er.orderid))
                            {
                                InnerOrderData removeData;
                                removeDic.TryRemove(er.orderid, out removeData);
                            }

                            if (_Cache_InnerOrders_Dic[er.securityid].Count == 0)
                            {
                                _Cache_InnerOrders_Dic.TryRemove(er.securityid, out removeDic);
                            }
                        }
                        //if (Application.Current.CheckAccess() == false || this.ViewSource.View == null)
                        //{
                        //    RowCount--;
                        //    if (er.side == "1")
                        //    {
                        //        BidRowCount--;
                        //    }
                        //    else if (er.side == "2")
                        //    {
                        //        OfrRowCount--;
                        //    }
                        //}
                    }
                    Sub_FilterBestData(findData, true);
                    UpdateInnerOrder_Best(er.securityid, er.symbol, er.orderid, er.orderSource);

                    return;
                }
                findData.OrderID = er.orderid;
                findData.SecurityID = er.securityid;
                findData.Symbol = er.symbol;
                findData.OrderSource = er.orderSource;
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityid);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    //findData.RemainingTime = bondsInfoPo.RemaingingTime;
                    findData.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                    findData.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    findData.BondSubType = bondsInfoPo.Bond_Subtype;
                    //findData.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                    //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    //findData.Issuer = bondsInfoPo.issuer;
                    //findData.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }
                findData.Side = er.side;
                findData.FirmFlag = string.IsNullOrEmpty(er.firmflag) ? INTENTION_FLAG : er.firmflag;
                findData.RemainingTime = er.tenor;
                findData.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(er.tenor);
                findData.SettleType = er.settltype;
                findData.SettlDate = $"{er.settlDate}*{er.settltype}";
                findData.RoleId = er.roleid;
                findData.StrategyOrderId = er.strategyorderid;
                findData.RoleName = er.rolename;
                findData.Price = er.price;
                
                if (VerticalOffset > 0)
                {
                    UpdateDataCount++;
                }
                else
                {
                    UpdateDataCount = 0;
                }
                findData.NetPrice = er.netprice;
                findData.FullPrice = er.fullprice;
                findData.Qty = er.orderqty?.ConvertToLong() - er.executionqty?.ConvertToLong();
                findData.CopyInfo = GetCopyInfo(findData.Qty?.ToString("#"), er);
                findData.PartySubId = er.partysubid;
                findData.PartySubName = er.partysubname;
                findData.UserName = er.username;
                findData.UpdateTime = er.updatetime.Length > 11 ? er.updatetime.Substring(11, 8) : (er.updatetime.Length > 8 ? er.updatetime.Substring(0, 8) : er.updatetime);
                findData.CreateTime = er.createtime;//er.createtime.Length > 11 ? er.createtime.Substring(11, 8) : (er.createtime.Length > 8 ? er.createtime.Substring(0, 8) : er.createtime);
                findData.WholeFlag = er.wholeflag;
                findData.IsMe = er.username == LocalEnv.UserInfo.UserName;
                findData.IntentionID = er.inf4;
                //findData.IsNeedFlash = isNew;
                //findData.FlashCount = isNew ? 4 : 0;
                findData.Sign = er.sign;
                findData.Sign1 = er.sign1;
                findData.Sign2 = er.sign2;
                findData.Sign3 = er.sign3;
                findData.Sign4 = er.sign4;
                findData.InstitutionID = er.institutionid;
                findData.InstitutionName = er.institutionname;
                findData.TraderID = er.traderid;
                findData.TraderName = er.tradername;
                findData.Client = GenClient(findData.SignDic, er, findData.IsMe);
                findData.Rating = GenRating(findData.SignDic1, findData.SignDic3, findData.SecurityID);
                findData.Cdc_Implited_Rating = GetCDCImplitedRating(findData.SignDic3);
                findData.BondCategory = GenBondCategory(findData.SignDic, findData.SignDic1);
                findData.Level = GenLevel(findData.SignDic3);
                findData.RegionType = GenRegionType(findData.SignDic2);
                findData.EnterpriseType = GenEnterpriseType(findData.SignDic1);
                findData.If_Guarantee = GenIfGuarantee(findData.SignDic1);
                findData.If_Repay = GenIfRepay(findData.SignDic3);
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
                findData.ChinaBondValuationFull = er.chinaBondValuationFull;
                if (findData.ChinaBondValuation.HasValue && findData.ChinaBondValuation != decimal.Zero && findData.Price.HasValue)
                {
                    if (findData.Side == "1")
                        findData.CCDCValuation_BP = (findData.Price - findData.ChinaBondValuation) * 100;
                    else if (findData.Side == "2")
                        findData.CCDCValuation_BP = (findData.ChinaBondValuation - findData.Price) * 100;
                }
                findData.CsiBondValuation = er.csiBondValuation;
                findData.CsiBondValuationFull = er.csiBondValuationFull;
                if (findData.CsiBondValuation.HasValue && findData.CsiBondValuation != decimal.Zero && findData.Price.HasValue)
                {
                    if (findData.Side == "1")
                        findData.CsiValuation_BP = (findData.Price - findData.CsiBondValuation) * 100;
                    else if (findData.Side == "2")
                        findData.CsiValuation_BP = (findData.CsiBondValuation - findData.Price) * 100;
                }

                if (IsCumType(findData.SignDic1))
                {
                    if (string.IsNullOrEmpty(er.chinaBondValuationFull) == false)
                    {
                        int speIndex = er.chinaBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            findData.ChinaBondExercise = er.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            findData.ChinaBondDue = er.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            findData.ChinaBondDue = er.chinaBondValuationFull.ConvertToDecimal();
                        }
                    }

                    if (findData.ChinaBondExercise.HasValue && findData.ChinaBondExercise != decimal.Zero && findData.Price.HasValue)
                    {
                        if (findData.Side == "1")
                            findData.CCDCValuation_BP = (findData.Price - findData.ChinaBondExercise) * 100;
                        else if (findData.Side == "2")
                            findData.CCDCValuation_BP = (findData.ChinaBondExercise - findData.Price) * 100;
                    }

                    if (string.IsNullOrEmpty(er.csiBondValuationFull) == false)
                    {
                        int speIndex = er.csiBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            findData.CsiBondValuationExercise = er.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            findData.CsiBondValuationDue = er.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            findData.CsiBondValuationDue = er.csiBondValuationFull.ConvertToDecimal();
                        }
                    }

                    if (findData.CsiBondValuationExercise.HasValue && findData.CsiBondValuationExercise != decimal.Zero && findData.Price.HasValue)
                    {
                        if (findData.Side == "1")
                            findData.CsiValuation_BP = (findData.Price - findData.CsiBondValuationExercise) * 100;
                        else if (findData.Side == "2")
                            findData.CsiValuation_BP = (findData.CsiBondValuationExercise - findData.Price) * 100;
                    }
                }
                else
                {
                    findData.ChinaBondDue = er.chinaBondValuation;

                    findData.CsiBondValuationDue = er.csiBondValuation;
                }



                findData.ModifiedDuration = er.modifiedDuration;
                findData.DayOffStr = er.dayOffStr;
                findData.IssueAmount = er.issueAmount / 10000;
                if (CommonUtils.IsNumeric(er.outstandingAmount))
                {
                    findData.Outstanding_Amount = er.outstandingAmount.ConvertToDecimal() / 10000;
                }

                findData.OCOFlag = er.ocoflag;
                if (!string.IsNullOrEmpty(er.inf2) && er.inf2 != "0")
                {
                    TagData tg = JsonConvert.DeserializeObject<TagData>(er.inf2);
                    if (CommonUtils.IsNumeric(tg.CUSTOMER_PRICE))
                    {
                        findData.CustomerPrice = tg.CUSTOMER_PRICE.ConvertToDecimal();
                    }
                    if (CommonUtils.IsNumeric(tg.CUSTOMER_SPREAD))
                    {
                        findData.BP = tg.CUSTOMER_SPREAD.ConvertToDecimal();
                    }

                    findData.HurryFlag = tg.HURRY_FLAG;
                    findData.SuggestFlag = tg.SUGGEST_FLAG;
                    findData.FireFlag = tg.FIRE_FLAG;
                    findData.FloorPrice = tg.FLOOR_PRICE;
                    findData.PublicRemarks = tg.PUBLIC_REMARKS;
                    findData.PersonalRemarks = findData.IsMe ? tg.PERSONAL_REMARKS : string.Empty;
                    if (!string.IsNullOrEmpty(tg.OCO_FLAG))
                    {
                        findData.OCOFlag = tg.OCO_FLAG;
                    }

                    findData.FromQQ = tg.fromQQ;
                }

                if (!string.IsNullOrEmpty(er.inf3) && er.inf3 != "0")
                {
                    JObject jsonObj = JObject.Parse(er.inf3);
                    if (jsonObj.ContainsKey("Val_Modified_Duration"))
                    {
                        //string Issue_Amount = jsonObj["Issue_Amount"].ToString();
                        string Val_Modified_Duration = jsonObj["Val_Modified_Duration"].ToString();
                        //string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                        //string valuation_date = jsonObj["valuation_date"].ToString();
                        if (CommonUtils.IsNumeric(Val_Modified_Duration))
                        {
                            findData.ModifiedDuration = Val_Modified_Duration.ConvertToDecimal();
                        }
                    }
                }
                findData.Channel = er.channel;
                findData.Info2 = er.inf2;
                findData.Info1 = er.inf1;
                findData.PriceType = er.priceType;
                findData.Tax_Free = SignParser.GetTaxFree(findData.SignDic4);
                if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                {
                    findData.RiskInfo = er.riskMsg;
                    findData.IsRisk = string.IsNullOrEmpty(er.riskMsg) ? false : true;
                }

                decimal? displayPrice;
                if (er.priceType == "1")
                {
                    displayPrice = er.netprice;
                }
                else if (er.priceType == "2")
                {
                    displayPrice = er.fullprice;
                }
                else
                {
                    displayPrice = er.price;
                }

                if (!displayPrice.HasValue)
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
                    findData.Price_Display = displayPrice.Value.ToString("0.00##");
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

                if (VerticalOffset > 0)
                {
                    UpdateDataCount++;
                }
                else
                {
                    UpdateDataCount = 0;
                }
                findData = new InnerOrderData();
                findData.OrderID = er.orderid;
                findData.SecurityID = er.securityid;
                findData.Symbol = er.symbol;
                findData.OrderSource = er.orderSource;
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityid);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    //findData.RemainingTime = bondsInfoPo.RemaingingTime;
                    findData.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                    findData.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    findData.BondSubType = bondsInfoPo.Bond_Subtype;
                    //findData.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                    //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    //findData.Issuer = bondsInfoPo.issuer;
                    //findData.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }
                findData.Side = er.side;
                findData.FirmFlag = string.IsNullOrEmpty(er.firmflag) ? INTENTION_FLAG : er.firmflag;
                findData.RemainingTime = er.tenor;
                findData.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(er.tenor);
                findData.SettleType = er.settltype;
                findData.SettlDate = $"{er.settlDate}*{er.settltype}";
                findData.RoleId = er.roleid;
                findData.RoleName = er.rolename;
                findData.Price = er.price;
                findData.NetPrice = er.netprice;
                findData.FullPrice = er.fullprice;
                findData.Qty = er.orderqty?.ConvertToLong() - er.executionqty?.ConvertToLong();
                findData.CopyInfo = GetCopyInfo(findData.Qty?.ToString("#"), er);
                findData.PartySubId = er.partysubid;
                findData.PartySubName = er.partysubname;
                findData.UserName = er.username;
                findData.UpdateTime = er.updatetime.Length > 11 ? er.updatetime.Substring(11, 8) : (er.updatetime.Length > 8 ? er.updatetime.Substring(0, 8) : er.updatetime);
                findData.CreateTime = er.createtime;//er.createtime.Length > 11 ? er.createtime.Substring(11, 8) : (er.createtime.Length > 8 ? er.createtime.Substring(0, 8) : er.createtime);
                findData.WholeFlag = er.wholeflag;
                findData.IsMe = er.username == LocalEnv.UserInfo.UserName;
                findData.IntentionID = er.inf4;
                findData.StrategyOrderId = er.strategyorderid;
                //findData.IsNeedFlash = isNew;
                //findData.FlashCount = isNew ? 4 : 0;
                findData.Sign = er.sign;
                findData.Sign1 = er.sign1;
                findData.Sign2 = er.sign2;
                findData.Sign3 = er.sign3;
                findData.Sign4 = er.sign4;
                findData.InstitutionID = er.institutionid;
                findData.InstitutionName = er.institutionname;
                findData.TraderID = er.traderid;
                findData.TraderName = er.tradername;
                findData.Client = GenClient(findData.SignDic, er, findData.IsMe);
                findData.Rating = GenRating(findData.SignDic1, findData.SignDic3, findData.SecurityID);
                findData.Cdc_Implited_Rating = GetCDCImplitedRating(findData.SignDic3);
                findData.BondCategory = GenBondCategory(findData.SignDic, findData.SignDic1);
                findData.Level = GenLevel(findData.SignDic3);
                findData.RegionType = GenRegionType(findData.SignDic2);
                findData.EnterpriseType = GenEnterpriseType(findData.SignDic1);
                findData.If_Guarantee = GenIfGuarantee(findData.SignDic1);
                findData.If_Repay = GenIfRepay(findData.SignDic3);
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
                findData.ChinaBondValuationFull = er.chinaBondValuationFull;
                if (findData.ChinaBondValuation.HasValue && findData.ChinaBondValuation != decimal.Zero && findData.Price.HasValue)
                {
                    if (findData.Side == "1")
                        findData.CCDCValuation_BP = (findData.Price - findData.ChinaBondValuation) * 100;
                    else if (findData.Side == "2")
                        findData.CCDCValuation_BP = (findData.ChinaBondValuation - findData.Price) * 100;
                }
                findData.CsiBondValuation = er.csiBondValuation;
                findData.CsiBondValuationFull = er.csiBondValuationFull;
                if (findData.CsiBondValuation.HasValue && findData.CsiBondValuation != decimal.Zero && findData.Price.HasValue)
                {
                    if (findData.Side == "1")
                        findData.CsiValuation_BP = (findData.Price - findData.CsiBondValuation) * 100;
                    else if (findData.Side == "2")
                        findData.CsiValuation_BP = (findData.CsiBondValuation - findData.Price) * 100;
                }

                if (IsCumType(findData.SignDic1))
                {
                    if (string.IsNullOrEmpty(er.chinaBondValuationFull) == false)
                    {
                        int speIndex = er.chinaBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            findData.ChinaBondExercise = er.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            findData.ChinaBondDue = er.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            findData.ChinaBondDue = er.chinaBondValuationFull.ConvertToDecimal();
                        }
                    }

                    if (findData.ChinaBondExercise.HasValue && findData.ChinaBondExercise != decimal.Zero && findData.Price.HasValue)
                    {
                        if (findData.Side == "1")
                            findData.CCDCValuation_BP = (findData.Price - findData.ChinaBondExercise) * 100;
                        else if (findData.Side == "2")
                            findData.CCDCValuation_BP = (findData.ChinaBondExercise - findData.Price) * 100;
                    }

                    if (string.IsNullOrEmpty(er.csiBondValuationFull) == false)
                    {
                        int speIndex = er.csiBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            findData.CsiBondValuationExercise = er.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            findData.CsiBondValuationDue = er.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            findData.CsiBondValuationDue = er.csiBondValuationFull.ConvertToDecimal();
                        }
                    }

                    if (findData.CsiBondValuationExercise.HasValue && findData.CsiBondValuationExercise != decimal.Zero && findData.Price.HasValue)
                    {
                        if (findData.Side == "1")
                            findData.CCDCValuation_BP = (findData.Price - findData.CsiBondValuationExercise) * 100;
                        else if (findData.Side == "2")
                            findData.CCDCValuation_BP = (findData.CsiBondValuationExercise - findData.Price) * 100;
                    }
                }
                else
                {
                    findData.ChinaBondDue = er.chinaBondValuation;

                    findData.CsiBondValuationDue = er.csiBondValuation;
                }
                findData.ModifiedDuration = er.modifiedDuration;
                findData.DayOffStr = er.dayOffStr;
                findData.IssueAmount = er.issueAmount / 10000;
                if (CommonUtils.IsNumeric(er.outstandingAmount))
                {
                    findData.Outstanding_Amount = er.outstandingAmount.ConvertToDecimal() / 10000;
                }
                lock (_dataLock)
                {
                    if (findData.IsMe)
                    {
                        MyOrderCount++;
                    }
                    InnerOrders.Insert(0, findData);

                    //if (isNew)
                    {
                        _flashDic[er.orderid] = findData;
                    }

                    if (!_Cache_InnerOrders_Dic.ContainsKey(er.securityid))
                    {
                        _Cache_InnerOrders_Dic[er.securityid] = new ConcurrentDictionary<string, InnerOrderData>();
                    }
                    _Cache_InnerOrders_Dic[er.securityid][er.orderid] = findData;

                    //if (Application.Current.CheckAccess() == false || this.ViewSource.View == null)
                    //{
                    //    RowCount++;
                    //    if (er.side == "1")
                    //    {
                    //        BidRowCount++;
                    //    }
                    //    else if (er.side == "2")
                    //    {
                    //        OfrRowCount++;
                    //    }
                    //}
                }

                if (Common.Utils.CommonUtils.IsBDBAppName())
                {
                    UpdateLastExecutions_BDB(findData);
                }
                else
                {
                    UpdateLastExecutions(findData);
                }

                //try
                //{
                //    if (!_symbolCountDic.ContainsKey(er.securityid))
                //    {
                //        _symbolCountDic[er.securityid] = null;
                //        LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{er.securityid}.");
                //        PriceHandler.Instance.RegisterCTPrice(er.securityid, UpdateCtPrice);
                //    }

                //    CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(findData.SecurityID);
                //    if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                //    {
                //        findData.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                //        findData.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                //        findData.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                //    }
                //}
                //catch (Exception e)
                //{
                //    LogHandler.Error(e);
                //}

                //try
                //{
                //    if (!_bestPriceDic.ContainsKey(er.securityid))
                //    {
                //        if ((er.orderSource ?? "1") == "1")
                //        {
                //            _bestPriceDic[er.securityid] = new decimal?[2];
                //            LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{er.securityid}.");
                //            PriceHandler.Instance.RegisterCTPrice(er.securityid, UpdateCtPrice);
                //        }
                //    }
                //}
                //catch (Exception e)
                //{
                //    LogHandler.Error(e);
                //}

                if (_bestPriceDic.ContainsKey(er.securityid))
                {
                    findData.Best_Bid = _bestPriceDic[er.securityid][0];
                    findData.Best_Ofr = _bestPriceDic[er.securityid][1];
                }

                findData.OCOFlag = er.ocoflag;
                if (!string.IsNullOrEmpty(er.inf2) && er.inf2 != "0")
                {
                    TagData tg = JsonConvert.DeserializeObject<TagData>(er.inf2);
                    if (CommonUtils.IsNumeric(tg.CUSTOMER_PRICE))
                    {
                        findData.CustomerPrice = tg.CUSTOMER_PRICE.ConvertToDecimal();
                    }
                    if (CommonUtils.IsNumeric(tg.CUSTOMER_SPREAD))
                    {
                        findData.BP = tg.CUSTOMER_SPREAD.ConvertToDecimal();
                    }

                    findData.HurryFlag = tg.HURRY_FLAG;
                    findData.SuggestFlag = tg.SUGGEST_FLAG;
                    findData.FireFlag = tg.FIRE_FLAG;
                    findData.FloorPrice = tg.FLOOR_PRICE;
                    findData.PublicRemarks = tg.PUBLIC_REMARKS;
                    findData.PersonalRemarks = findData.IsMe ? tg.PERSONAL_REMARKS : string.Empty;
                    if (!string.IsNullOrEmpty(tg.OCO_FLAG))
                    {
                        findData.OCOFlag = tg.OCO_FLAG;
                    }

                    findData.FromQQ = tg.fromQQ;
                }

                if (!string.IsNullOrEmpty(er.inf3) && er.inf3 != "0")
                {
                    JObject jsonObj = JObject.Parse(er.inf3);
                    if (jsonObj.ContainsKey("Val_Modified_Duration"))
                    {
                        //string Issue_Amount = jsonObj["Issue_Amount"].ToString();
                        string Val_Modified_Duration = jsonObj["Val_Modified_Duration"].ToString();
                        //string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                        //string valuation_date = jsonObj["valuation_date"].ToString();
                        if (CommonUtils.IsNumeric(Val_Modified_Duration))
                        {
                            findData.ModifiedDuration = Val_Modified_Duration.ConvertToDecimal();
                        }
                    }

                }

                findData.Channel = er.channel;
                findData.Info2 = er.inf2;
                findData.Info1 = er.inf1;
                findData.PriceType = er.priceType;
                findData.Tax_Free = SignParser.GetTaxFree(findData.SignDic4);
                if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                {
                    findData.RiskInfo = er.riskMsg;
                    findData.IsRisk = string.IsNullOrEmpty(er.riskMsg) ? false : true;
                }

                decimal? displayPrice;
                if (er.priceType == "1")
                {
                    displayPrice = er.netprice;
                }
                else if (er.priceType == "2")
                {
                    displayPrice = er.fullprice;
                }
                else
                {
                    displayPrice = er.price;
                }

                if (!displayPrice.HasValue)
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
                    findData.Price_Display = displayPrice.Value.ToString("0.00##");
                }
            }
            Sub_FilterBestData(findData);
            UpdateInnerOrder_Best(er.securityid, er.symbol, er.orderid, er.orderSource);
        }

        /// <summary>
        /// securtityid, updatetime
        /// </summary>
        ConcurrentDictionary<string, string> _lastUpdateTimeDic = new ConcurrentDictionary<string, string>();
        internal PriceComparer bidPriceComparer = new PriceComparer("0");
        internal PriceComparer ofrPriceComparer = new PriceComparer("1");
        private void UpdateInnerOrder_Best(EmsOrderReport er, bool isNew = false)
        {
            try
            {
                string updateTime_temp = er.updatetime.Length > 11 ? er.updatetime.Substring(11, 8) : (er.updatetime.Length > 8 ? er.updatetime.Substring(0, 8) : er.updatetime);
                string updateTime = string.Empty;
                if (!_lastUpdateTimeDic.ContainsKey(er.securityid))
                {
                    _lastUpdateTimeDic[er.securityid] = string.Empty;
                }
                if (_lastUpdateTimeDic[er.securityid].CompareTo(updateTime_temp) < 0)
                {
                    _lastUpdateTimeDic[er.securityid] = updateTime_temp;
                }
                updateTime = _lastUpdateTimeDic[er.securityid];

                InnerOrderData findData_best = null;
                IEnumerable<InnerOrderData> bid_InnerOrdersList = null;
                IEnumerable<InnerOrderData> ofr_InnerOrdersList = null;
                int bidCount = 0;
                int ofrCount = 0;
                InnerOrderData myFirstBidInnerData = null;
                InnerOrderData myFirstOfrInnerData = null;

                string hurryFlag = string.Empty;
                string SuggestFlag = string.Empty;
                string fireflag = string.Empty;

                if (er.securityid == "99999")
                {
                    if (_Cache_InnerOrders_Dic.ContainsKey(er.securityid) && _Cache_InnerOrders_Dic[er.securityid] != null
                        && _Cache_InnerOrders_Dic[er.securityid].ContainsKey(er.orderid) && _Cache_InnerOrders_Dic[er.securityid][er.orderid] != null)
                    {
                        //updateTime = _Cache_InnerOrders_Dic[er.securityid][er.orderid].UpdateTime;

                        bid_InnerOrdersList = _Cache_InnerOrders_Dic[er.securityid].Values.Where(item => item.OrderID == er.orderid && item.Side == "1").OrderBy(item => item, bidPriceComparer).ToList();
                        ofr_InnerOrdersList = _Cache_InnerOrders_Dic[er.securityid].Values.Where(item => item.OrderID == er.orderid && item.Side == "2").OrderBy(item => item, ofrPriceComparer).ToList();
                    }
                }
                else
                {
                    IEnumerable<InnerOrderData> findDatas_securityid = null;
                    if (_Cache_InnerOrders_Dic.ContainsKey(er.securityid))
                    {
                        //findDatas_securityid = _Cache_InnerOrders_Dic[er.securityid].Values.OrderByDescending(item => item.UpdateTime);

                        bid_InnerOrdersList = _Cache_InnerOrders_Dic[er.securityid].Values.Where(item => item.Side == "1").OrderBy(item => item, bidPriceComparer).ToList();
                        ofr_InnerOrdersList = _Cache_InnerOrders_Dic[er.securityid].Values.Where(item => item.Side == "2").OrderBy(item => item, ofrPriceComparer).ToList();

                        hurryFlag = _Cache_InnerOrders_Dic[er.securityid].Values.Any(item => item.HurryFlag == "1") ? "1" : string.Empty;
                        SuggestFlag = _Cache_InnerOrders_Dic[er.securityid].Values.Any(item => item.SuggestFlag == "1") ? "1" : string.Empty;
                        fireflag = _Cache_InnerOrders_Dic[er.securityid].Values.Any(item => item.FireFlag == "1") ? "1" : string.Empty;
                    }

                    //if (findDatas_securityid != null && findDatas_securityid.Count() > 0)
                    //{
                    //    updateTime = findDatas_securityid.First().UpdateTime;
                    //}
                }

                if (bid_InnerOrdersList != null)
                {
                    bidCount = bid_InnerOrdersList.Count();

                    IEnumerable<InnerOrderData> myBidList = bid_InnerOrdersList.Where(item => item.IsMe);
                    if (myBidList != null && myBidList.Count() > 0)
                    {
                        myFirstBidInnerData = myBidList.First();
                    }
                }

                if (ofr_InnerOrdersList != null)
                {
                    ofrCount = ofr_InnerOrdersList.Count();

                    IEnumerable<InnerOrderData> myOfrList = ofr_InnerOrdersList.Where(item => item.IsMe);
                    if (myOfrList != null && myOfrList.Count() > 0)
                    {
                        myFirstOfrInnerData = myOfrList.First();
                    }
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

                string key = string.Empty;
                if (er.securityid == "99999")
                {
                    key = $"{er.securityid}_{er.orderid}";
                }
                else
                {
                    key = $"{er.securityid}";
                }

                if (_Cache_InnerOrders_Dic_Best.ContainsKey(key))
                {
                    findData_best = _Cache_InnerOrders_Dic_Best[key];
                }

                if (findData_best != null)
                {
                    //if (!(er.exectype == "N" || er.exectype == "0" || er.exectype == "1" || er.exectype == "S" || er.exectype == "6"))
                    if ((string.IsNullOrEmpty(er.firmflag) == false && (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C"))
                        || (string.IsNullOrEmpty(er.firmflag) == true && (er.orderstatus == "0" || er.orderstatus == "2" || er.orderstatus == "3" || er.orderstatus == "4"))
                        || (string.IsNullOrEmpty(er.firmflag) == true && er.orderstatus == "1" && er.price.HasValue && er.orderqty.HasValue))
                    {
                        // 删除全部
                        if (bidCount == 0 && ofrCount == 0)
                        {
                            lock (_dataLock_best)
                            {
                                this.InnerOrders_Best.Remove(findData_best);

                                // 不注销price订阅
                                //if (_bestPriceDic.ContainsKey(er.securityid) && !InnerOrders.Any(item => item.SecurityID == er.securityid))
                                //{
                                //    PriceHandler.Instance.UnregisterCTPrice(er.securityid, UpdateCtPrice);
                                //}

                                InnerOrderData removeData;
                                _Cache_InnerOrders_Dic_Best.TryRemove(key, out removeData);

                                if (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null)
                                {
                                    RowCount_Best--;
                                }
                            }
                            return;
                        }
                        else
                        {
                            // 删除Bid单边
                            if (bidCount == 0)
                            {
                                findData_best.PartySubName_InnerBid = string.Empty;
                                findData_best.PartySubId_InnerBid = string.Empty;
                                findData_best.UserName_InnerBid = string.Empty;
                                findData_best.Qty_InnerBid = null;
                                findData_best.CopyInfo_InnerBid = null;
                                lock (_dataLock_best)
                                {
                                    if (findData_best.Count_InnerBid > 0
                                        && (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null || this.ViewSource_Best.View.Contains(findData_best)))
                                    {
                                        BidRowCount_Best--;
                                    }
                                }
                                findData_best.Count_InnerBid = 0;
                                findData_best.Price_InnerBid = null;
                                findData_best.CCDCValuation_BP_InnerBid = null;
                                findData_best.CsiValuation_BP_InnerBid = null;
                                findData_best.FirmFlag_InnerBid = string.Empty;
                                findData_best.SettleType_InnerBid = string.Empty;
                                findData_best.RoleId_InnerBid = string.Empty;
                                findData_best.RoleName_InnerBid = string.Empty;
                                findData_best.WholeFlag_InnerBid = string.Empty;
                                findData_best.IsMe_InnerBid = false;
                                findData_best.NetPrice_InnerBid = null;
                                findData_best.FullPrice_InnerBid = null;
                                findData_best.IntentionId_InnerBid = string.Empty;
                                findData_best.StrategyOrderId_innerBid = string.Empty;
                                findData_best.IsBest_InnerBid = string.Empty;
                                findData_best.PartySubName_Tooltip_InnerBid = string.Empty;
                                findData_best.Qty_Tooltip_InnerBid = string.Empty;
                                findData_best.Sign_InnerBid = 0;
                                findData_best.Client_InnerBid = string.Empty;
                                findData_best.TraderID_InnerBid = string.Empty;
                                findData_best.TraderName_InnerBid = string.Empty;
                                findData_best.InstitutionID_InnerBid = string.Empty;
                                findData_best.InstitutionName_InnerBid = string.Empty;
                                findData_best.DQType_InnerBid = string.Empty;
                                findData_best.OrderID = string.Empty;
                                findData_best.HurryFlag_InnerBid = string.Empty;
                                findData_best.SuggestFlag_InnerBid = string.Empty;
                                findData_best.FireFlag_InnerBid = string.Empty;
                                findData_best.FloorPrice_InnerBid = string.Empty;
                                findData_best.OCOFlag_InnerBid = string.Empty;
                                findData_best.PublicRemarks_InnerBid = string.Empty;
                                findData_best.PersonalRemarks_InnerBid = string.Empty;
                                findData_best.FromQQ_Bid = string.Empty;
                                findData_best.Channel_InnerBid = string.Empty;
                                findData_best.Info2_InnerBid = string.Empty;
                                findData_best.Info1_InnerBid = string.Empty;
                                findData_best.PriceType_InnerBid = string.Empty;
                                findData_best.RiskInfo = string.Empty;
                                findData_best.IsRisk = false;
                                findData_best.Price_Display_InnerBid = null;
                            }

                            // 删除Ofr单边
                            if (ofrCount == 0)
                            {
                                findData_best.PartySubName_InnerOfr = string.Empty;
                                findData_best.PartySubId_InnerOfr = string.Empty;
                                findData_best.UserName_InnerOfr = string.Empty;
                                findData_best.Qty_InnerOfr = null;
                                findData_best.CopyInfo_InnerOfr = null;
                                findData_best.Count_InnerOfr = 0;
                                lock (_dataLock_best)
                                {
                                    if (findData_best.Count_InnerOfr > 0
                                       && (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null || this.ViewSource_Best.View.Contains(findData_best)))
                                    {
                                        OfrRowCount_Best--;
                                    }
                                }

                                findData_best.Price_InnerOfr = null;
                                findData_best.CCDCValuation_BP_InnerOfr = null;
                                findData_best.CsiValuation_BP_InnerOfr = null;
                                findData_best.FirmFlag_InnerOfr = string.Empty;
                                findData_best.SettleType_InnerOfr = string.Empty;
                                findData_best.RoleId_InnerOfr = string.Empty;
                                findData_best.RoleName_InnerOfr = string.Empty;
                                findData_best.WholeFlag_InnerOfr = string.Empty;
                                findData_best.IsMe_InnerOfr = false;
                                findData_best.NetPrice_InnerOfr = null;
                                findData_best.FullPrice_InnerOfr = null;
                                findData_best.IntentionId_InnerOfr = string.Empty;
                                findData_best.StrategyOrderId_InnerOfr = string.Empty;
                                findData_best.IsBest_InnerOfr = string.Empty;
                                findData_best.PartySubName_Tooltip_InnerOfr = string.Empty;
                                findData_best.Qty_Tooltip_InnerOfr = string.Empty;
                                findData_best.Sign_InnerOfr = 0;
                                findData_best.Client_InnerOfr = string.Empty;
                                findData_best.TraderID_InnerOfr = string.Empty;
                                findData_best.TraderName_InnerOfr = string.Empty;
                                findData_best.InstitutionID_InnerOfr = string.Empty;
                                findData_best.InstitutionName_InnerOfr = string.Empty;
                                findData_best.DQType_InnerOfr = string.Empty;
                                findData_best.OrderID = string.Empty;
                                findData_best.HurryFlag_InnerOfr = string.Empty;
                                findData_best.SuggestFlag_InnerOfr = string.Empty;
                                findData_best.FireFlag_InnerOfr = string.Empty;
                                findData_best.FloorPrice_InnerOfr = string.Empty;
                                findData_best.OCOFlag_InnerOfr = string.Empty;
                                findData_best.PublicRemarks_InnerOfr = string.Empty;
                                findData_best.PersonalRemarks_InnerOfr = string.Empty;
                                findData_best.FromQQ_Ofr = string.Empty;
                                findData_best.Channel_InnerOfr = string.Empty;
                                findData_best.Info2_InnerOfr = string.Empty;
                                findData_best.Info1_InnerOfr = string.Empty;
                                findData_best.PriceType_InnerOfr = string.Empty;
                                findData_best.Price_Display_InnerOfr = null;
                            }
                        }

                        findData_best.UpdateTime = updateTime;
                    }

                    if (VerticalOffset_Best > 0)
                    {
                        UpdateDataCount_Best++;
                    }
                    else
                    {
                        UpdateDataCount_Best = 0;
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
                        //findData_best.RemainingTime = bondsInfoPo.RemaingingTime;
                        findData_best.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        findData_best.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        findData_best.BondSubType = bondsInfoPo.Bond_Subtype;
                        //findData_best.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                        //findData_best.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //findData_best.Issuer = bondsInfoPo.issuer;
                        //findData_best.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        findData_best.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    }
                    findData_best.HurryFlag = hurryFlag;
                    findData_best.SuggestFlag = SuggestFlag;
                    findData_best.FireFlag = fireflag;
                    findData_best.RemainingTime = er.tenor;
                    findData_best.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(er.tenor);

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
                        findData_best.ChinaBondValuationFull = bestBidInnerOrder.ChinaBondValuationFull;
                        findData_best.ChinaBondExercise = bestBidInnerOrder.ChinaBondExercise;
                        findData_best.ChinaBondDue = bestBidInnerOrder.ChinaBondDue;
                        findData_best.CCDCValuation_BP = bestBidInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP = bestBidInnerOrder.CsiValuation_BP;
                        findData_best.CsiBondValuation = bestBidInnerOrder.CsiBondValuation;
                        findData_best.CsiBondValuationFull = bestBidInnerOrder.CsiBondValuationFull;
                        findData_best.CsiBondValuationExercise = bestBidInnerOrder.CsiBondValuationExercise;
                        findData_best.CsiBondValuationDue = bestBidInnerOrder.CsiBondValuationDue;
                        findData_best.ModifiedDuration = bestBidInnerOrder.ModifiedDuration;
                        findData_best.DayOffStr = bestBidInnerOrder.DayOffStr;
                        findData_best.Rating = bestBidInnerOrder.Rating;
                        findData_best.Cdc_Implited_Rating = bestBidInnerOrder.Cdc_Implited_Rating;
                        findData_best.BondCategory = bestBidInnerOrder.BondCategory;
                        findData_best.Level = bestBidInnerOrder.Level;
                        findData_best.RegionType = bestBidInnerOrder.RegionType;
                        findData_best.EnterpriseType = bestBidInnerOrder.EnterpriseType;
                        findData_best.If_Guarantee = bestBidInnerOrder.If_Guarantee;
                        findData_best.If_Repay = bestBidInnerOrder.If_Repay;
                        findData_best.IssueType = bestBidInnerOrder.IssueType;
                        findData_best.IssueAmount = bestBidInnerOrder.IssueAmount;
                        findData_best.Outstanding_Amount = bestBidInnerOrder.Outstanding_Amount;
                        findData_best.PartySubName_InnerBid = partySubNameList.Count > 1 ? "多" : bestBidInnerOrder.PartySubName;
                        findData_best.PartySubId_InnerBid = bestBidInnerOrder.PartySubId;
                        findData_best.UserName_InnerBid = bestBidInnerOrder.UserName;
                        findData_best.PartySubName_Tooltip_InnerBid = partySubNameTooltip.ToString();
                        findData_best.Qty_InnerBid = qty == 0 ? null : qty;
                        findData_best.CopyInfo_InnerBid = copyInfo.ToString();
                        findData_best.Qty_Tooltip_InnerBid = qtyTooltip.ToString();

                        //int count = bid_InnerOrdersList.Count(item => item.Price == bestBidInnerOrder.Price);
                        lock (_dataLock_best)
                        {
                            if (findData_best.Count_InnerBid == 0
                                && (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null || this.ViewSource_Best.View.Contains(findData_best)))
                            {
                                BidRowCount_Best++;
                            }
                        }
                        findData_best.Count_InnerBid = bidCount;
                        findData_best.Price_InnerBid = bestBidInnerOrder.Price;
                        findData_best.CCDCValuation_BP_InnerBid = bestBidInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP_InnerBid = bestBidInnerOrder.CsiValuation_BP;
                        findData_best.FirmFlag_InnerBid = bestBidInnerOrder.FirmFlag;
                        findData_best.SettleType_InnerBid = bestBidInnerOrder.SettleType;
                        findData_best.RoleId_InnerBid = bestBidInnerOrder.RoleId;
                        findData_best.RoleName_InnerBid = bestBidInnerOrder.RoleName;
                        findData_best.WholeFlag_InnerBid = bestBidInnerOrder.WholeFlag;
                        findData_best.IsMe_InnerBid = bestBidInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                        findData_best.IntentionId_InnerBid = bestBidInnerOrder.IntentionID;
                        findData_best.NetPrice_InnerBid = bestBidInnerOrder.NetPrice;
                        findData_best.FullPrice_InnerBid = bestBidInnerOrder.FullPrice;
                        findData_best.OrderID_InnerBid = bestBidInnerOrder.OrderID;
                        findData_best.StrategyOrderId_innerBid = bestBidInnerOrder.StrategyOrderId;
                        findData_best.Sign_InnerBid = bestBidInnerOrder.Sign;
                        findData_best.Sign_InnerBid1 = bestBidInnerOrder.Sign1;
                        findData_best.Sign_InnerBid2 = bestBidInnerOrder.Sign2;
                        findData_best.Sign_InnerBid3 = bestBidInnerOrder.Sign3;
                        findData_best.Sign_InnerBid4 = bestBidInnerOrder.Sign4;
                        findData_best.Client_InnerBid = bestBidInnerOrder.Client;
                        findData_best.TraderID_InnerBid = bestBidInnerOrder.TraderID;
                        findData_best.TraderName_InnerBid = bestBidInnerOrder.TraderName;
                        findData_best.InstitutionID_InnerBid = bestBidInnerOrder.InstitutionID;
                        findData_best.InstitutionName_InnerBid = bestBidInnerOrder.InstitutionName;
                        findData_best.DQType_InnerBid = bestBidInnerOrder.DQType;
                        findData_best.HurryFlag_InnerBid = bestBidInnerOrder.HurryFlag;
                        findData_best.SuggestFlag_InnerBid = bestBidInnerOrder.SuggestFlag;
                        findData_best.FireFlag_InnerBid = bestBidInnerOrder.FireFlag;
                        findData_best.FloorPrice_InnerBid = bestBidInnerOrder.FloorPrice;
                        findData_best.OCOFlag_InnerBid = bestBidInnerOrder.OCOFlag;
                        findData_best.PublicRemarks_InnerBid = bestBidInnerOrder.PublicRemarks;
                        findData_best.PersonalRemarks_InnerBid = findData_best.IsMe_InnerBid ? bestBidInnerOrder.PersonalRemarks : string.Empty;
                        findData_best.FromQQ_Bid = bestBidInnerOrder.FromQQ;
                        findData_best.Channel_InnerBid = bestBidInnerOrder.Channel;
                        findData_best.Info2_InnerBid = bestBidInnerOrder.Info2;
                        findData_best.Info1_InnerBid = bestBidInnerOrder.Info1;
                        findData_best.PriceType_InnerBid = bestBidInnerOrder.PriceType;

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

                        if (findData_best.PriceType_InnerBid == "1")
                        {
                            findData_best.Price_Display_InnerBid = findData_best.NetPrice_InnerBid;
                        }
                        else if (findData_best.PriceType_InnerBid == "2")
                        {
                            findData_best.Price_Display_InnerBid = findData_best.FullPrice_InnerBid;
                        }
                        else
                        {
                            findData_best.Price_Display_InnerBid = findData_best.Price_InnerBid;
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
                        findData_best.PartySubId_InnerOfr = bestOfrInnerOrder.PartySubId;
                        findData_best.UserName_InnerOfr = bestOfrInnerOrder.UserName;
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
                        findData_best.ChinaBondValuationFull = bestOfrInnerOrder.ChinaBondValuationFull;
                        findData_best.ChinaBondExercise = bestOfrInnerOrder.ChinaBondExercise;
                        findData_best.ChinaBondDue = bestOfrInnerOrder.ChinaBondDue;
                        findData_best.CCDCValuation_BP = bestOfrInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP = bestOfrInnerOrder.CsiValuation_BP;
                        findData_best.CsiBondValuation = bestOfrInnerOrder.CsiBondValuation;
                        findData_best.CsiBondValuationFull = bestOfrInnerOrder.CsiBondValuationFull;
                        findData_best.CsiBondValuationExercise = bestOfrInnerOrder.CsiBondValuationExercise;
                        findData_best.CsiBondValuationDue = bestOfrInnerOrder.CsiBondValuationDue;
                        findData_best.ModifiedDuration = bestOfrInnerOrder.ModifiedDuration;
                        findData_best.DayOffStr = bestOfrInnerOrder.DayOffStr;
                        findData_best.Rating = bestOfrInnerOrder.Rating;
                        findData_best.Cdc_Implited_Rating = bestOfrInnerOrder.Cdc_Implited_Rating;
                        findData_best.BondCategory = bestOfrInnerOrder.BondCategory;
                        findData_best.Level = bestOfrInnerOrder.Level;
                        findData_best.RegionType = bestOfrInnerOrder.RegionType;
                        findData_best.EnterpriseType = bestOfrInnerOrder.EnterpriseType;
                        findData_best.If_Guarantee = bestOfrInnerOrder.If_Guarantee;
                        findData_best.If_Repay = bestOfrInnerOrder.If_Repay;
                        findData_best.IssueType = bestOfrInnerOrder.IssueType;
                        findData_best.IssueAmount = bestOfrInnerOrder.IssueAmount;
                        findData_best.Outstanding_Amount = bestOfrInnerOrder.Outstanding_Amount;
                        findData_best.Price_InnerOfr = bestOfrInnerOrder.Price;
                        findData_best.CCDCValuation_BP_InnerOfr = bestOfrInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP_InnerOfr = bestOfrInnerOrder.CsiValuation_BP;
                        //int count = ofr_InnerOrdersList.Count(item => item.Price == bestOfrInnerOrder.Price);
                        lock (_dataLock_best)
                        {
                            if (findData_best.Count_InnerOfr == 0
                                && (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null || this.ViewSource_Best.View.Contains(findData_best)))
                            {
                                OfrRowCount_Best++;
                            }
                        }
                        findData_best.Count_InnerOfr = ofrCount;
                        findData_best.FirmFlag_InnerOfr = bestOfrInnerOrder.FirmFlag;
                        findData_best.SettleType_InnerOfr = bestOfrInnerOrder.SettleType;
                        findData_best.RoleId_InnerOfr = bestOfrInnerOrder.RoleId;
                        findData_best.RoleName_InnerOfr = bestOfrInnerOrder.RoleName;
                        findData_best.WholeFlag_InnerOfr = bestOfrInnerOrder.WholeFlag;
                        findData_best.IsMe_InnerOfr = bestOfrInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                        findData_best.IntentionId_InnerOfr = bestOfrInnerOrder.IntentionID;
                        findData_best.NetPrice_InnerOfr = bestOfrInnerOrder.NetPrice;
                        findData_best.FullPrice_InnerOfr = bestOfrInnerOrder.FullPrice;
                        findData_best.OrderID_InnerOfr = bestOfrInnerOrder.OrderID;
                        findData_best.StrategyOrderId_InnerOfr = bestOfrInnerOrder.StrategyOrderId;
                        findData_best.Sign_InnerOfr = bestOfrInnerOrder.Sign;
                        findData_best.Sign_InnerOfr1 = bestOfrInnerOrder.Sign1;
                        findData_best.Sign_InnerOfr2 = bestOfrInnerOrder.Sign2;
                        findData_best.Sign_InnerOfr3 = bestOfrInnerOrder.Sign3;
                        findData_best.Sign_InnerOfr4 = bestOfrInnerOrder.Sign4;
                        findData_best.Client_InnerOfr = bestOfrInnerOrder.Client;
                        findData_best.TraderID_InnerOfr = bestOfrInnerOrder.TraderID;
                        findData_best.TraderName_InnerOfr = bestOfrInnerOrder.TraderName;
                        findData_best.InstitutionID_InnerOfr = bestOfrInnerOrder.InstitutionID;
                        findData_best.InstitutionName_InnerOfr = bestOfrInnerOrder.InstitutionName;
                        findData_best.DQType_InnerOfr = bestOfrInnerOrder.DQType;
                        findData_best.HurryFlag_InnerOfr = bestOfrInnerOrder.HurryFlag;
                        findData_best.SuggestFlag_InnerOfr = bestOfrInnerOrder.SuggestFlag;
                        findData_best.FireFlag_InnerOfr = bestOfrInnerOrder.FireFlag;
                        findData_best.FloorPrice_InnerOfr = bestOfrInnerOrder.FloorPrice;
                        findData_best.OCOFlag_InnerOfr = bestOfrInnerOrder.OCOFlag;
                        findData_best.PublicRemarks_InnerOfr = bestOfrInnerOrder.PublicRemarks;
                        findData_best.PersonalRemarks_InnerOfr = findData_best.IsMe_InnerOfr ? bestOfrInnerOrder.PersonalRemarks : string.Empty;
                        findData_best.FromQQ_Ofr = bestOfrInnerOrder.FromQQ;
                        findData_best.Channel_InnerOfr = bestOfrInnerOrder.Channel;
                        findData_best.Info2_InnerOfr = bestOfrInnerOrder.Info2;
                        findData_best.Info1_InnerOfr = bestOfrInnerOrder.Info1;
                        findData_best.PriceType_InnerOfr = bestOfrInnerOrder.PriceType;

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
                        if (findData_best.PriceType_InnerOfr == "1")
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.NetPrice_InnerOfr;
                        }
                        else if (findData_best.PriceType_InnerOfr == "2")
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.FullPrice_InnerOfr;
                        }
                        else
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.Price_InnerOfr;
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

                    if (VerticalOffset_Best > 0)
                    {
                        UpdateDataCount_Best++;
                    }
                    else
                    {
                        UpdateDataCount_Best = 0;
                    }
                    findData_best = new InnerOrderData();

                    findData_best.SecurityID = er.securityid;
                    findData_best.Symbol = er.symbol;
                    findData_best.UpdateTime = updateTime;
                    findData_best.OrderSource = er.orderSource;
                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityid);
                    if (bondsInfoPo != null)
                    {
                        //Symbol = bondsInfoPo.Short_Name;
                        //Security = bondsInfoPo.Bond_ID;
                        //SymbolName = bondsInfoPo.Short_Name;
                        //findData_best.RemainingTime = bondsInfoPo.RemaingingTime;
                        findData_best.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        findData_best.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        findData_best.BondSubType = bondsInfoPo.Bond_Subtype;
                        //findData_best.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                        //findData_best.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //findData_best.Issuer = bondsInfoPo.issuer;
                        //findData_best.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        findData_best.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    }
                    findData_best.HurryFlag = hurryFlag;
                    findData_best.SuggestFlag = SuggestFlag;
                    findData_best.FireFlag = fireflag;
                    findData_best.RemainingTime = er.tenor;
                    findData_best.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(er.tenor);

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
                        findData_best.ChinaBondValuationFull = bestBidInnerOrder.ChinaBondValuationFull;
                        findData_best.ChinaBondExercise = bestBidInnerOrder.ChinaBondExercise;
                        findData_best.ChinaBondDue = bestBidInnerOrder.ChinaBondDue;
                        findData_best.CCDCValuation_BP = bestBidInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP = bestBidInnerOrder.CsiValuation_BP;
                        findData_best.CsiBondValuation = bestBidInnerOrder.CsiBondValuation;
                        findData_best.CsiBondValuationFull = bestBidInnerOrder.CsiBondValuationFull;
                        findData_best.CsiBondValuationExercise = bestBidInnerOrder.CsiBondValuationExercise;
                        findData_best.CsiBondValuationDue = bestBidInnerOrder.CsiBondValuationDue;
                        findData_best.ModifiedDuration = bestBidInnerOrder.ModifiedDuration;
                        findData_best.DayOffStr = bestBidInnerOrder.DayOffStr;
                        findData_best.Rating = bestBidInnerOrder.Rating;
                        findData_best.Cdc_Implited_Rating = bestBidInnerOrder.Cdc_Implited_Rating;
                        findData_best.BondCategory = bestBidInnerOrder.BondCategory;
                        findData_best.Level = bestBidInnerOrder.Level;
                        findData_best.RegionType = bestBidInnerOrder.RegionType;
                        findData_best.EnterpriseType = bestBidInnerOrder.EnterpriseType;
                        findData_best.If_Guarantee = bestBidInnerOrder.If_Guarantee;
                        findData_best.If_Repay = bestBidInnerOrder.If_Repay;
                        findData_best.IssueType = bestBidInnerOrder.IssueType;
                        findData_best.IssueAmount = bestBidInnerOrder.IssueAmount;
                        findData_best.Outstanding_Amount = bestBidInnerOrder.Outstanding_Amount;
                        findData_best.PartySubName_InnerBid = partySubNameList.Count > 1 ? "多" : bestBidInnerOrder.PartySubName;
                        findData_best.PartySubId_InnerBid = bestBidInnerOrder.PartySubId;
                        findData_best.UserName_InnerBid = bestBidInnerOrder.UserName;
                        findData_best.PartySubName_Tooltip_InnerBid = partySubNameTooltip.ToString();
                        findData_best.Qty_InnerBid = qty == 0 ? null : qty;
                        findData_best.CopyInfo_InnerBid = copyInfo.ToString();
                        findData_best.Qty_Tooltip_InnerBid = qtyTooltip.ToString();
                        //int count = bid_InnerOrdersList.Count(item => item.Price == bestBidInnerOrder.Price);
                        findData_best.Count_InnerBid = bidCount;
                        findData_best.Price_InnerBid = bestBidInnerOrder.Price;
                        findData_best.CCDCValuation_BP_InnerBid = bestBidInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP_InnerBid = bestBidInnerOrder.CsiValuation_BP;
                        findData_best.FirmFlag_InnerBid = bestBidInnerOrder.FirmFlag;
                        findData_best.SettleType_InnerBid = bestBidInnerOrder.SettleType;
                        findData_best.RoleId_InnerBid = bestBidInnerOrder.RoleId;
                        findData_best.RoleName_InnerBid = bestBidInnerOrder.RoleName;
                        findData_best.WholeFlag_InnerBid = bestBidInnerOrder.WholeFlag;
                        findData_best.IsMe_InnerBid = bestBidInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                        findData_best.IntentionId_InnerBid = bestBidInnerOrder.IntentionID;
                        findData_best.NetPrice_InnerBid = bestBidInnerOrder.NetPrice;
                        findData_best.FullPrice_InnerBid = bestBidInnerOrder.FullPrice;
                        findData_best.OrderID_InnerBid = bestBidInnerOrder.OrderID;
                        findData_best.StrategyOrderId_innerBid = bestBidInnerOrder.StrategyOrderId;
                        findData_best.Sign_InnerBid = bestBidInnerOrder.Sign;
                        findData_best.Sign_InnerBid1 = bestBidInnerOrder.Sign1;
                        findData_best.Sign_InnerBid2 = bestBidInnerOrder.Sign2;
                        findData_best.Sign_InnerBid3 = bestBidInnerOrder.Sign3;
                        findData_best.Sign_InnerBid4 = bestBidInnerOrder.Sign4;
                        findData_best.Client_InnerBid = bestBidInnerOrder.Client;
                        findData_best.TraderID_InnerBid = bestBidInnerOrder.TraderID;
                        findData_best.TraderName_InnerBid = bestBidInnerOrder.TraderName;
                        findData_best.InstitutionID_InnerBid = bestBidInnerOrder.InstitutionID;
                        findData_best.InstitutionName_InnerBid = bestBidInnerOrder.InstitutionName;
                        findData_best.DQType_InnerBid = bestBidInnerOrder.DQType;
                        findData_best.HurryFlag_InnerBid = bestBidInnerOrder.HurryFlag;
                        findData_best.SuggestFlag_InnerBid = bestBidInnerOrder.SuggestFlag;
                        findData_best.FireFlag_InnerBid = bestBidInnerOrder.FireFlag;
                        findData_best.FloorPrice_InnerBid = bestBidInnerOrder.FloorPrice;
                        findData_best.OCOFlag_InnerBid = bestBidInnerOrder.OCOFlag;
                        findData_best.PublicRemarks_InnerBid = bestBidInnerOrder.PublicRemarks;
                        findData_best.PersonalRemarks_InnerBid = findData_best.IsMe_InnerBid ? bestBidInnerOrder.PersonalRemarks : string.Empty;
                        findData_best.FromQQ_Bid = bestBidInnerOrder.FromQQ;
                        findData_best.Channel_InnerBid = bestBidInnerOrder.Channel;
                        findData_best.Info2_InnerBid = bestBidInnerOrder.Info2;
                        findData_best.Info1_InnerBid = bestBidInnerOrder.Info1;
                        findData_best.PriceType_InnerBid = bestBidInnerOrder.PriceType;

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

                        if (findData_best.PriceType_InnerBid == "1")
                        {
                            findData_best.Price_Display_InnerBid = findData_best.NetPrice_InnerBid;
                        }
                        else if (findData_best.PriceType_InnerBid == "2")
                        {
                            findData_best.Price_Display_InnerBid = findData_best.FullPrice_InnerBid;
                        }
                        else
                        {
                            findData_best.Price_Display_InnerBid = findData_best.Price_InnerBid;
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
                        findData_best.PartySubId_InnerOfr = bestOfrInnerOrder.PartySubId;
                        findData_best.UserName_InnerOfr = bestOfrInnerOrder.UserName;
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
                        findData_best.ChinaBondValuationFull = bestOfrInnerOrder.ChinaBondValuationFull;
                        findData_best.ChinaBondExercise = bestOfrInnerOrder.ChinaBondExercise;
                        findData_best.ChinaBondDue = bestOfrInnerOrder.ChinaBondDue;
                        findData_best.CCDCValuation_BP = bestOfrInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP = bestOfrInnerOrder.CsiValuation_BP;
                        findData_best.CsiBondValuation = bestOfrInnerOrder.CsiBondValuation;
                        findData_best.CsiBondValuationFull = bestOfrInnerOrder.CsiBondValuationFull;
                        findData_best.CsiBondValuationExercise = bestOfrInnerOrder.CsiBondValuationExercise;
                        findData_best.CsiBondValuationDue = bestOfrInnerOrder.CsiBondValuationDue;
                        findData_best.ModifiedDuration = bestOfrInnerOrder.ModifiedDuration;
                        findData_best.DayOffStr = bestOfrInnerOrder.DayOffStr;
                        findData_best.Rating = bestOfrInnerOrder.Rating;
                        findData_best.Cdc_Implited_Rating = bestOfrInnerOrder.Cdc_Implited_Rating;
                        findData_best.BondCategory = bestOfrInnerOrder.BondCategory;
                        findData_best.Level = bestOfrInnerOrder.Level;
                        findData_best.RegionType = bestOfrInnerOrder.RegionType;
                        findData_best.EnterpriseType = bestOfrInnerOrder.EnterpriseType;
                        findData_best.If_Guarantee = bestOfrInnerOrder.If_Guarantee;
                        findData_best.If_Repay = bestOfrInnerOrder.If_Repay;
                        findData_best.IssueType = bestOfrInnerOrder.IssueType;
                        findData_best.IssueAmount = bestOfrInnerOrder.IssueAmount;
                        findData_best.Outstanding_Amount = bestOfrInnerOrder.Outstanding_Amount;
                        findData_best.Price_InnerOfr = bestOfrInnerOrder.Price;
                        findData_best.CCDCValuation_BP_InnerOfr = bestOfrInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP_InnerOfr = bestOfrInnerOrder.CsiValuation_BP;
                        //int count = ofr_InnerOrdersList.Count(item => item.Price == bestOfrInnerOrder.Price);
                        findData_best.Count_InnerOfr = ofrCount;
                        findData_best.FirmFlag_InnerOfr = bestOfrInnerOrder.FirmFlag;
                        findData_best.SettleType_InnerOfr = bestOfrInnerOrder.SettleType;
                        findData_best.RoleId_InnerOfr = bestOfrInnerOrder.RoleId;
                        findData_best.RoleName_InnerOfr = bestOfrInnerOrder.RoleName;
                        findData_best.WholeFlag_InnerOfr = bestOfrInnerOrder.WholeFlag;
                        findData_best.IsMe_InnerOfr = bestOfrInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                        findData_best.IntentionId_InnerOfr = bestOfrInnerOrder.IntentionID;
                        findData_best.NetPrice_InnerOfr = bestOfrInnerOrder.NetPrice;
                        findData_best.FullPrice_InnerOfr = bestOfrInnerOrder.FullPrice;
                        findData_best.OrderID_InnerOfr = bestOfrInnerOrder.OrderID;
                        findData_best.StrategyOrderId_InnerOfr = bestOfrInnerOrder.StrategyOrderId;
                        findData_best.Sign_InnerOfr = bestOfrInnerOrder.Sign;
                        findData_best.Sign_InnerOfr1 = bestOfrInnerOrder.Sign1;
                        findData_best.Sign_InnerOfr2 = bestOfrInnerOrder.Sign2;
                        findData_best.Sign_InnerOfr3 = bestOfrInnerOrder.Sign3;
                        findData_best.Sign_InnerOfr4 = bestOfrInnerOrder.Sign4;
                        findData_best.Client_InnerOfr = bestOfrInnerOrder.Client;
                        findData_best.TraderID_InnerOfr = bestOfrInnerOrder.TraderID;
                        findData_best.TraderName_InnerOfr = bestOfrInnerOrder.TraderName;
                        findData_best.InstitutionID_InnerOfr = bestOfrInnerOrder.InstitutionID;
                        findData_best.InstitutionName_InnerOfr = bestOfrInnerOrder.InstitutionName;
                        findData_best.DQType_InnerOfr = bestOfrInnerOrder.DQType;
                        findData_best.HurryFlag_InnerOfr = bestOfrInnerOrder.HurryFlag;
                        findData_best.SuggestFlag_InnerOfr = bestOfrInnerOrder.SuggestFlag;
                        findData_best.FireFlag_InnerOfr = bestOfrInnerOrder.FireFlag;
                        findData_best.FloorPrice_InnerOfr = bestOfrInnerOrder.FloorPrice;
                        findData_best.OCOFlag_InnerOfr = bestOfrInnerOrder.OCOFlag;
                        findData_best.PublicRemarks_InnerOfr = bestOfrInnerOrder.PublicRemarks;
                        findData_best.PersonalRemarks_InnerOfr = findData_best.IsMe_InnerOfr ? bestOfrInnerOrder.PersonalRemarks : string.Empty;
                        findData_best.FromQQ_Ofr = bestOfrInnerOrder.FromQQ;
                        findData_best.Channel_InnerOfr = bestOfrInnerOrder.Channel;
                        findData_best.Info2_InnerOfr = bestOfrInnerOrder.Info2;
                        findData_best.Info1_InnerOfr = bestOfrInnerOrder.Info1;
                        findData_best.PriceType_InnerOfr = bestOfrInnerOrder.PriceType;

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

                        if (findData_best.PriceType_InnerOfr == "1")
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.NetPrice_InnerOfr;
                        }
                        else if (findData_best.PriceType_InnerOfr == "2")
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.FullPrice_InnerOfr;
                        }
                        else
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.Price_InnerOfr;
                        }
                    }

                    //findData_best.IsNeedFlash = isNew;
                    if (Common.Utils.CommonUtils.IsBDBAppName())
                    {
                        UpdateLastExecutions_BDB(findData_best);
                    }
                    else
                    {
                        UpdateLastExecutions(findData_best);
                    }

                    lock (_dataLock_best)
                    {
                        InnerOrders_Best.Insert(0, findData_best);

                        _Cache_InnerOrders_Dic_Best[key] = findData_best;

                        if (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null)
                        {
                            RowCount_Best++;

                            if (bidCount > 0)
                            {
                                BidRowCount_Best++;
                            }
                            if (ofrCount > 0)
                            {
                                OfrRowCount_Best++;
                            }
                        }
                    }

                    //try
                    //{
                    //    if (!_symbolCountDic.ContainsKey(er.securityid))
                    //    {
                    //        _symbolCountDic[er.securityid] = null;
                    //        LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{er.securityid}.");
                    //        PriceHandler.Instance.RegisterCTPrice(er.securityid, UpdateCtPrice);
                    //    }

                    //    CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(findData_best.SecurityID);
                    //    if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                    //    {
                    //        findData_best.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                    //        findData_best.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                    //        findData_best.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    LogHandler.Error(e);
                    //}

                    try
                    {
                        if (!_bestPriceDic.ContainsKey(er.securityid))
                        {
                            if ((er.orderSource ?? "1") == "1")
                            {
                                _bestPriceDic[er.securityid] = new decimal?[2];
                                LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{er.securityid}.");
                                PriceHandler.Instance.RegisterCTPrice(er.securityid, UpdateCtPrice);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }
                }

                //if (((er.side == "1" && findData_best.OrderID_InnerBid == er.orderid) 
                //      || (er.side == "2" && findData_best.OrderID_InnerOfr == er.orderid)) && isNew)
                {
                    _flashDic_best[key] = findData_best;
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
            catch (Exception e)
            {
                LogHelper.Error(e);
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

                if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                {
                    if (riskFilterList != null && riskFilterList.Count > 0)
                    {
                        bool isDisplay = false;

                        if (riskFilterList.Contains("无风险") && string.IsNullOrEmpty(data.RiskInfo))
                        {
                            isDisplay = true;
                        }
                        else
                        {
                            if (data.RiskList != null)
                            {
                                foreach (string subRisk in data.RiskList)
                                {
                                    if (riskFilterList.Any(e => !string.IsNullOrEmpty(subRisk) && subRisk.Contains(e)))
                                    {
                                        isDisplay = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (isDisplay == false)
                        {
                            return false;
                        }
                    }
                }

                lock (_GroupBondsLock)
                {
                    if (this.SelectedGroupBondsItems != null && this.SelectedGroupBondsItems.Count > 0
                    && (data.SecurityID == null || !this.SelectedGroupBondsItems.Contains(data.SecurityID)))
                    {
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(this.SearchText) == false
                    && (data.SecurityID == null || !data.SecurityID.Contains(this.SearchText))
                    && (data.Symbol == null || !data.Symbol.Contains(this.SearchText))
                    && (data.RoleName == null || !data.RoleName.Contains(this.SearchText))
                    && (data.PartySubName == null || !data.PartySubName.Contains(this.SearchText))
                    && (data.IssuerName == null || !data.IssuerName.Contains(this.SearchText))
                    && (data.WarranterName == null || !data.WarranterName.Contains(this.SearchText))
                    && (data.TraderName == null || !data.TraderName.Contains(this.SearchText))
                    && (data.InstitutionName == null || !data.InstitutionName.Contains(this.SearchText)))
                    return false;

                if (this._focusTagDic != null && this._focusTagDic.ContainsKey(Consts.SIDE))
                {
                    List<string> sidefilter = this._focusTagDic[Consts.SIDE];
                    if (sidefilter.Count > 0)
                    {
                        string otherside = data.Side == "1" ? "2" : "1";
                        if ((sidefilter.Contains("0001") && data.Side == "1")
                        || (sidefilter.Contains("0010") && data.Side == "2")
                        || (sidefilter.Contains("0011") && _Cache_InnerOrders_Dic[data.SecurityID].Values.Any(i => i.Side == otherside)))
                        {
                            // ignore
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (IsMe == true && data.IsMe == false)
                {
                    return false;
                }

                if (IsFuzzy == true && data.SecurityID != "99999")
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

                if (this.IsCoupon)
                {
                    if (!string.IsNullOrEmpty(this.StartCoupon) && (data.CouponRateCurrent == null || data.CouponRateCurrent < this.StartCoupon.ConvertToDecimal()))
                    {
                        return false;
                    }
                    if (!string.IsNullOrEmpty(this.EndCoupon) && (data.CouponRateCurrent == null || data.CouponRateCurrent > this.EndCoupon.ConvertToDecimal()))
                    {
                        return false;
                    }
                }

                {
                    if (!string.IsNullOrEmpty(this.StartPrice) && !string.IsNullOrEmpty(this.EndPrice))
                    {
                        decimal startPrice = this.StartPrice.ConvertToDecimal();
                        decimal endPrice = this.EndPrice.ConvertToDecimal();
                        if (!(data.Price >= startPrice && data.Price <= endPrice))
                        {
                            return false;
                        }
                    }
                    else if (!string.IsNullOrEmpty(this.StartPrice) && string.IsNullOrEmpty(this.EndPrice))
                    {
                        decimal startPrice = this.StartPrice.ConvertToDecimal();
                        if (!(data.Price >= startPrice))
                        {
                            return false;
                        }
                    }
                    else if (string.IsNullOrEmpty(this.StartPrice) && !string.IsNullOrEmpty(this.EndPrice))
                    {
                        decimal endPrice = this.EndPrice.ConvertToDecimal();
                        if (!(data.Price <= endPrice))
                        {
                            return false;
                        }
                    }
                }

                {
                    decimal? chinaBondValue = data.ChinaBondValuation;
                    if (data.ChinaBondExercise.HasValue)
                    {
                        chinaBondValue = data.ChinaBondExercise;
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
                }

                // bid-中债
                if (data.Side == "1" && data.Price.HasValue && data.ChinaBondValuation.HasValue)
                {
                    decimal value = (data.Price.Value - data.ChinaBondValuation.Value) * 100;
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

                // 中债-ofr
                if (data.Side == "2" && data.Price.HasValue && data.ChinaBondValuation.HasValue)
                {
                    decimal value = (data.ChinaBondValuation.Value - data.Price.Value) * 100;
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
                else if (result && data.Sign3 != 0 && "BNC".Equals(data.SecurityType) && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic3) {
                        string key = pair.Key;
                        string value = pair.Value;
                        if (key == Utils.Constants.RANKING_TYPE)
                        {
                            result = result
                               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
                               || (_focusTagDic[key].Contains("xx") && !rankingtype_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
                        }
                        else if (key == Utils.Constants.RATING_CURRENT)
                        {
                            result = result
                               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
                               || (_focusTagDic[key].Contains("xx") && !ratingCurrent_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
                        }
                        else if (key == Utils.Constants.CDC_IMPLITED_RATING)
                        {
                            result = result
                               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
                               || (_focusTagDic[key].Contains("xx") && !cdcImplitedRating_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
                        }
                        else if (key == Utils.Constants.ISSUER_RATING_CURRENT_CBR
                            || key == Utils.Constants.CSI_IMPLIED_RATING)
                        {
                            result = result
                               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
                               || (_focusTagDic[key].Contains("01000") && value == "00000" && !"BNC".Equals(data.SecurityType)); // 00000也算其他
                        }
                    }
                }
                
                if (data.Sign4 != 0 && this._focusTagDic != null)
                {
                    foreach (KeyValuePair<string, string> pair in data.SignDic4)
                    {
                        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

                        if (!result)
                            return result;
                    }
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

            //this.ViewSource_Best.View?.Refresh();
            this.FilterBestData();
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
            //this.ViewSource_Best.View?.Refresh();
            this.FilterBestData();
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
            //this.ViewSource_Best.View?.Refresh();
            this.FilterBestData();
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
            //this.ViewSource_Best.View?.Refresh();
            this.FilterBestData();
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
            //this.ViewSource_Best.View?.Refresh();
            this.FilterBestData();
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
                    && (data.IssuerName == null || !data.IssuerName.Contains(this.Best_SearchText))
                    && (data.WarranterName == null || !data.WarranterName.Contains(this.Best_SearchText))
                    && (data.TraderName_InnerBid == null || !data.TraderName_InnerBid.Contains(this.Best_SearchText))
                    && (data.TraderName_InnerOfr == null || !data.TraderName_InnerOfr.Contains(this.Best_SearchText))
                    && (data.InstitutionName_InnerBid == null || !data.InstitutionName_InnerBid.Contains(this.Best_SearchText))
                    && (data.InstitutionName_InnerOfr == null || !data.InstitutionName_InnerOfr.Contains(this.Best_SearchText)))
                    return false;

                //if (IsMe_Best == true && data.IsMe_InnerBid == false && data.IsMe_InnerOfr == false)
                //    return false;
                if (IsMe_Best)
                {
                    if (!_Cache_InnerOrders_Dic.ContainsKey(data.SecurityID)
                        || !_Cache_InnerOrders_Dic[data.SecurityID].Values.Any(item => item.IsMe))
                    {
                        return false;
                    }
                }

                if (IsMe_Best == true && _focusTagDic_Best != null && _focusTagDic_Best.ContainsKey(Utils.Constants.SIDE)
                    && _focusTagDic_Best[Utils.Constants.SIDE] != null && _focusTagDic_Best[Utils.Constants.SIDE].Count > 0
                    && !((_focusTagDic_Best[Utils.Constants.SIDE].Contains("0001") && data.IsMe_InnerBid == true)
                        || (_focusTagDic_Best[Utils.Constants.SIDE].Contains("0010") && data.IsMe_InnerOfr == true)))
                {
                    return false;
                }

                if (IsFuzzy_Best == true && data.SecurityID != "99999")
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

                if (this.IsCoupon_Best)
                {
                    if (!string.IsNullOrEmpty(this.StartCoupon_Best) && (data.CouponRateCurrent == null || data.CouponRateCurrent < this.StartCoupon_Best.ConvertToDecimal()))
                    {
                        return false;
                    }
                    if (!string.IsNullOrEmpty(this.EndCoupon_Best) && (data.CouponRateCurrent == null || data.CouponRateCurrent > this.EndCoupon_Best.ConvertToDecimal()))
                    {
                        return false;
                    }
                }

                {
                    decimal? chinaBondValue = data.ChinaBondValuation;
                    if (data.ChinaBondExercise.HasValue)
                    {
                        chinaBondValue = data.ChinaBondExercise;
                    }

                    if (!string.IsNullOrEmpty(this.StartChinaBondValue_Best) && !string.IsNullOrEmpty(this.EndChinaBondValue_Best))
                    {
                        decimal startChinaBondValue = this.StartChinaBondValue_Best.ConvertToDecimal();
                        decimal endChinaBondValue = this.EndChinaBondValue_Best.ConvertToDecimal();
                        if (!(chinaBondValue >= startChinaBondValue && chinaBondValue <= endChinaBondValue))
                        {
                            return false;
                        }
                    }
                    else if (!string.IsNullOrEmpty(this.StartChinaBondValue_Best) && string.IsNullOrEmpty(this.EndChinaBondValue_Best))
                    {
                        decimal startChinaBondValue = this.StartChinaBondValue_Best.ConvertToDecimal();
                        if (!(chinaBondValue >= startChinaBondValue))
                        {
                            return false;
                        }
                    }
                    else if (string.IsNullOrEmpty(this.StartChinaBondValue_Best) && !string.IsNullOrEmpty(this.EndChinaBondValue_Best))
                    {
                        decimal endChinaBondValue = this.EndChinaBondValue_Best.ConvertToDecimal();
                        if (!(chinaBondValue <= endChinaBondValue))
                        {
                            return false;
                        }
                    }
                }

                bool result_InnerBid = true;
                bool result_InnerOfr = true;
                // bid-中债
                decimal? value = null;
                if (data.Price_InnerBid.HasValue && data.ChinaBondValuation.HasValue)
                {
                    value = (data.Price_InnerBid - data.ChinaBondValuation) * 100;
                }
                result_InnerBid = result_InnerBid
                   && ((value.HasValue == false && string.IsNullOrEmpty(this.StartBidBp_Best) && string.IsNullOrEmpty(this.EndBidBp_Best))
                   || (value.HasValue
                   && (string.IsNullOrEmpty(this.StartBidBp_Best) || (!string.IsNullOrEmpty(this.StartBidBp_Best) && value >= this.StartBidBp_Best.ConvertToDecimal()))
                   && (string.IsNullOrEmpty(this.EndBidBp_Best) || (!string.IsNullOrEmpty(this.EndBidBp_Best) && value <= this.EndBidBp_Best.ConvertToDecimal()))));

                // 中债-ofr
                value = null;
                if (data.Price_InnerOfr.HasValue && data.ChinaBondValuation.HasValue)
                {
                    value = (data.ChinaBondValuation - data.Price_InnerOfr) * 100;
                }
                result_InnerOfr = result_InnerOfr
                    && ((value.HasValue == false && string.IsNullOrEmpty(this.StartOfrBp_Best) && string.IsNullOrEmpty(this.EndOfrBp_Best))
                    || (value.HasValue
                    && (string.IsNullOrEmpty(this.StartOfrBp_Best) || (!string.IsNullOrEmpty(this.StartOfrBp_Best) && value >= this.StartOfrBp_Best.ConvertToDecimal()))
                    && (string.IsNullOrEmpty(this.EndOfrBp_Best) || (!string.IsNullOrEmpty(this.EndOfrBp_Best) && value <= this.EndOfrBp_Best.ConvertToDecimal()))));

                if (!(result_InnerBid && result_InnerOfr))
                    return false;

                result_InnerBid = data.Count_InnerBid > 0;
                result_InnerOfr = data.Count_InnerOfr > 0;

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

                return result_InnerBid || result_InnerOfr;
            };
        }

        string[] rankingtype_arr = new string[] { "00001", "00010", "00011", "00100", "00101", "00110" };
        string[] ratingCurrent_arr = new string[] { "00001", "00010", "00011", "00100", "00101", "00110" };
        string[] cdcImplitedRating_arr = new string[] { "00001", "00010", "00011", "00100", "00101", "00110", "01000" };

        private void CheckSign(InnerOrderData data, string key, string value, ref bool result, Dictionary<string, List<string>> focusTagDic, string type = null)
        {
            if (key == Consts.SIDE)
            {
                result = true;
            }
            else if (key == Utils.Constants.ISSUER)
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
                        ChildTag ct = CommonUtils.GetIssuer(ic);
                        if (ct != null)
                        {
                            binaryList.Add(ct.childTag);
                        }

                    }
                }

                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || binaryList.Any(item => arr != null && arr.Contains(item)));
            }
            else if (key == Utils.Constants.GUARANTOR)
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
                        ChildTag ct = CommonUtils.GetGuarantor(ic);
                        if (ct != null)
                        {
                            binaryList.Add(ct.childTag);
                        }
                    }
                }

                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || binaryList.Any(item => arr != null && arr.Contains(item)));
            }
            else if (key == Utils.Constants.DQTYPE)
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
            else if (key == Utils.Constants.CUM_TYPE)
            {
                string cumType = "00";

                if (type == "1")
                {
                    if (data.SignDic_InnerBid1 != null && data.SignDic_InnerBid1.ContainsKey(Utils.Constants.CUM_TYPE))
                        cumType = data.SignDic_InnerBid1[Utils.Constants.CUM_TYPE];
                }
                else if (type == "2")
                {
                    if (data.SignDic_InnerOfr1 != null && data.SignDic_InnerOfr1.ContainsKey(Utils.Constants.CUM_TYPE))
                        cumType = data.SignDic_InnerOfr1[Utils.Constants.CUM_TYPE];
                }
                else
                {
                    if (data.SignDic1 != null && data.SignDic1.ContainsKey(Utils.Constants.CUM_TYPE))
                        cumType = data.SignDic1[Utils.Constants.CUM_TYPE];
                }

                string ifExe = "00";
                if (type == "1")
                {
                    if (data.SignDic_InnerBid3 != null && data.SignDic_InnerBid3.ContainsKey(Utils.Constants.IF_EXERCISE))
                        ifExe = data.SignDic_InnerBid3[Utils.Constants.IF_EXERCISE];
                }
                else if (type == "2")
                {
                    if (data.SignDic_InnerOfr3 != null && data.SignDic_InnerOfr3.ContainsKey(Utils.Constants.IF_EXERCISE))
                        ifExe = data.SignDic_InnerOfr3[Utils.Constants.IF_EXERCISE];
                }
                else
                {
                    if (data.SignDic3 != null && data.SignDic3.ContainsKey(Utils.Constants.IF_EXERCISE))
                        ifExe = data.SignDic3[Utils.Constants.IF_EXERCISE];
                }

                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(cumType) || (focusTagDic[key].Contains("11") && ifExe == "10"));
            }
            else if (key == Utils.Constants.ORGTYPE)
            {
                result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value) || (focusTagDic[key].Contains("0000000000000001") && value == "0000000000000000"));
            }
            else if (key == Utils.Constants.BOND_SUB_TYPE)
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
                    && (!focusTagDic.ContainsKey(Utils.Constants.BONDTYPE) || focusTagDic[Utils.Constants.BONDTYPE].Count == 0 || !focusTagDic.ContainsKey(key) || focusTagDic[Utils.Constants.BONDTYPE].Count == oldSubBondTypeList.Count);
            }
            else if (key == Utils.Constants.RANKING_TYPE)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("xx") && !rankingtype_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            }
            else if (key == Utils.Constants.RATING_CURRENT)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("xx") && !ratingCurrent_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            }
            else if (key == Utils.Constants.CDC_IMPLITED_RATING)
            {
                result = result
                   && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
                   || (focusTagDic[key].Contains("xx") && !cdcImplitedRating_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            }
            else if (key == Utils.Constants.ISSUER_RATING_CURRENT_CBR
                || key == Utils.Constants.CSI_IMPLIED_RATING)
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

        private string GenBondCategory(Dictionary<string, string> singDic, Dictionary<string, string> singDic1)
        {
            string client = string.Empty;
            string localGovBondTypeDefault = string.Empty;
            string finBondType = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Utils.Constants.BOND_SUB_TYPE))
                {
                    client = CommonUtils.GetTagNameByCode(singDic1[Utils.Constants.BOND_SUB_TYPE], Utils.Constants.BOND_SUB_TYPE);
                }

                if ("金融债" == client)
                {
                    if (singDic.ContainsKey(Utils.Constants.FINBONDTYPE))
                    {
                        finBondType = CommonUtils.GetTagNameByCode(singDic[Utils.Constants.FINBONDTYPE], Utils.Constants.FINBONDTYPE);
                    }

                    if (!string.IsNullOrEmpty(finBondType))
                    {
                        client = $"{client}({finBondType})";
                    }
                }


                if (singDic1.ContainsKey(Utils.Constants.LOCAL_GOV_BOND_TYPE))
                {
                    localGovBondTypeDefault = CommonUtils.GetTagNameByCode(singDic1[Utils.Constants.LOCAL_GOV_BOND_TYPE], Utils.Constants.LOCAL_GOV_BOND_TYPE).Replace("债", "");
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

            //if (rating_current.StartsWith("其他"))
            //{
            //    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            //    if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Rating_Current))
            //    {
            //        rating_current = bondsInfoPo.Rating_Current;
            //    }
            //    else
            //    {
            //        rating_current = "--";
            //    }
            //}

            string ranking_type = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Utils.Constants.RANKING_TYPE))
                {
                    ranking_type = CommonUtils.GetTagNameByCode(singDic1[Utils.Constants.RANKING_TYPE], Utils.Constants.RANKING_TYPE);
                }
            }
            ranking_type = string.IsNullOrEmpty(ranking_type) ? "--" : ranking_type;

            //if (ranking_type.StartsWith("其他"))
            //{
            //    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            //    if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Issuer_Rating_Current))
            //    {
            //        ranking_type = bondsInfoPo.Issuer_Rating_Current;
            //    }
            //    else
            //    {
            //        ranking_type = "--";
            //    }
            //}

            return $"{ranking_type}/{rating_current}";
        }

        private string GetCDCImplitedRating(Dictionary<string, string> signDic3)
        {
            string cdc_implited_ratings = string.Empty;
            if (signDic3 != null)
            {
                if (signDic3.ContainsKey(Utils.Constants.CDC_IMPLITED_RATING))
                {
                    cdc_implited_ratings = CommonUtils.GetTagNameByCode(signDic3[Utils.Constants.CDC_IMPLITED_RATING], Utils.Constants.CDC_IMPLITED_RATING);
                }
            }
            cdc_implited_ratings = string.IsNullOrEmpty(cdc_implited_ratings) ? "--" : cdc_implited_ratings;

            return cdc_implited_ratings;
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

        private string GenIfRepay(Dictionary<string, string> signDic3)
        {
            string ifRepay = string.Empty;
            if (signDic3 != null)
            {
                if (signDic3.ContainsKey(Utils.Constants.IF_REPAY))
                {
                    ifRepay = CommonUtils.GetTagNameByCode(signDic3[Utils.Constants.IF_REPAY], Utils.Constants.IF_REPAY);
                }
            }

            return ifRepay;
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
                    IfGuarantee = signDic1[Utils.Constants.IF_GUARANTEE];
                }
            }

            return IfGuarantee == "0001";
        }

        private bool IsCumType(Dictionary<string, string> signDic1)
        {
            bool isCumType = false;
            if (signDic1 != null)
            {
                if (signDic1.ContainsKey(Utils.Constants.CUM_TYPE))
                {
                    if (signDic1[Utils.Constants.CUM_TYPE] == "01"
                        || signDic1[Utils.Constants.CUM_TYPE] == "11")
                    {
                        isCumType = true;
                    }
                }
            }
            return isCumType;
        }

        private string GetCopyInfo(string qty, MDEntries er)
        {
            string nonfirmStar = string.Empty;
            if (er.FirmFlag == null || er.FirmFlag == "0")
            {
                nonfirmStar = "*";
            }
            StringBuilder copyInfo = new StringBuilder();
            string settlTypeDisplay = ComboSourceUtil.Instance.FindEnum(er.SettlType, "settletype");
            if (er.SettlType != "1" && er.SettlType != "2" && er.SettlType != "2,3")
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

        internal List<InnerOrderData> GetInnerOrderDatas(Func<InnerOrderData, bool> predicate, IComparer<InnerOrderData> comparer = null)
        {
            IEnumerable<InnerOrderData> dataList = null;
            if (predicate != null)
            {
                lock (_dataLock)
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

        internal InnerOrderData SelectedRowBySecurtiyID_Single_Copy(string groupid, string orderid, string securityID)
        {
            try
            {
                if (_Cache_InnerOrders_Dic.ContainsKey(securityID) && _Cache_InnerOrders_Dic[securityID] != null && _Cache_InnerOrders_Dic[securityID].ContainsKey(orderid))
                {
                    InnerOrderData record = _Cache_InnerOrders_Dic[securityID][orderid];
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
                if (_Cache_InnerOrders_Dic_Best.ContainsKey(securityid))
                {
                    InnerOrderData record = _Cache_InnerOrders_Dic_Best[securityid];

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
                                //ViewSource_Best.View?.Refresh();
                                this.FilterBestData();
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

        internal InnerOrderData SelectedRowBySecurtiyID_Best(string securityid, string orderID)
        {
            try
            {
                string key = securityid;
                if (securityid == "99999")
                {
                    key = $"{securityid}_{orderID}";
                }

                if (_Cache_InnerOrders_Dic_Best.ContainsKey(key))
                {
                    InnerOrderData record = _Cache_InnerOrders_Dic_Best[key];
                    this.SelectedItem_MyInnerOrders_Best = record;
                    this.SelectedItem_Tag_Best = null;
                    return record;
                }
            }
            catch (System.InvalidOperationException ex)
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
                    //if (this.ViewSource.View != null)
                    //{
                    //    (this.ViewSource.View as CustomCollectionView<InnerOrderData>).ClearOldSelectedItem();
                    //}

                    //if (this.ViewSource_Best.View != null)
                    //{
                    //    (this.ViewSource_Best.View as CustomCollectionView<InnerOrderData>).ClearOldSelectedItem();
                    //}

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
                        //ViewSource_Best.View?.Refresh();
                        this.FilterBestData();
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



        private Visibility _isBestOperationGrid = Visibility.Collapsed;

        public Visibility IsBestOperationGrid
        {
            get { return _isBestOperationGrid; }
            set {
                _isBestOperationGrid = value;
                this.OnPropertyChanged();
            }
        }


        private Visibility _isSingleOperationGrid = Visibility.Collapsed;

        public Visibility IsSingleOperationGrid
        {
            get { return _isSingleOperationGrid; }
            set
            {
                _isSingleOperationGrid = value;
                this.OnPropertyChanged();
            }
        }

        private string GenClient_Broker(Dictionary<string, string> dic, EmsOrderReport er, bool isMe)
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


        #region 过滤最优临时数据

        /// <summary>
        /// key: securityid
        /// value: 数组，[0]：买数列 [1]：卖数列 
        /// </summary>
        internal ConcurrentDictionary<string, SortedDictionary<InnerOrderData, object>[]> _Temp_Cache_InnerOrders_Dic = new ConcurrentDictionary<string, SortedDictionary<InnerOrderData, object>[]>();

        private volatile bool isFilterBestData = false;

        /// <summary>
        /// 最优数据过滤临时数据
        /// </summary>
        internal void FilterBestData()
        {
            isFilterBestData = true;
        } 

        private void ProcessFilterBestData()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                _Temp_Cache_InnerOrders_Dic.Clear();
                foreach (ConcurrentDictionary<string, InnerOrderData> dic in _Cache_InnerOrders_Dic.Values)
                {
                    foreach (InnerOrderData data in dic.Values)
                    {
                        Sub_FilterBestData(data);
                    }
                }

                foreach (InnerOrderData er in _Cache_InnerOrders_Dic_Best.Values)
                {
                    UpdateInnerOrder_Best(er.SecurityID, er.Symbol, er.OrderID, er.OrderSource);
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


                foreach (KeyValuePair<string, SortedDictionary<InnerOrderData, object>[]> pair in _Temp_Cache_InnerOrders_Dic)
                {

                    if (!_Cache_InnerOrders_Dic_Best.ContainsKey(pair.Key))
                    {
                        if (pair.Value.Length == 2)
                        {
                            if (pair.Value[0].Count > 0)
                            {
                                InnerOrderData data = pair.Value[0].First().Key;
                                string symbol = data.Symbol;
                                string orderid = data.OrderID;
                                string orderSource = data.OrderSource;
                                UpdateInnerOrder_Best(data.SecurityID, symbol, orderid, orderSource);

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
                            else if (pair.Value[1].Count > 0)
                            {
                                InnerOrderData data = pair.Value[1].First().Key;
                                string symbol = data.Symbol;
                                string orderid = data.OrderID;
                                string orderSource = data.OrderSource;
                                UpdateInnerOrder_Best(data.SecurityID, symbol, orderid, orderSource);

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
                        }
                    }
                }

                //// 计算条数
                //lock (_dataLock_best)
                //{
                //    CustomCollectionView<InnerOrderData> view = this.ViewSource_Best.View as CustomCollectionView<InnerOrderData>;
                //    if (view == null)
                //    {
                //        RowCount_Best = 0;
                //        BidRowCount_Best = 0;
                //        OfrRowCount_Best = 0;
                //    }
                //    else
                //    {
                //        RowCount_Best = view.Count;
                //        IEnumerable<InnerOrderData> tempDatas = view.Cast<InnerOrderData>();
                //        BidRowCount_Best = tempDatas.Count(p => p.Count_InnerBid > 0);
                //        OfrRowCount_Best = tempDatas.Count(p => p.Count_InnerOfr > 0);
                //    }
                //}
                SetDefaultSelectedItem_Best();
            }));
        }

        /// <summary>
        /// 重新刷新
        /// </summary>
        /// <param name="securityid"></param>
        /// <param name="symbol"></param>
        /// <param name="updatetime"></param>
        /// <param name="deleteflag">空：不删; 0:双边删除； 1：bid删除；2：ofr删除</param>
        internal void UpdateInnerOrder_Best(string securityid, string symbol, string orderid, string orderSource)
        {
            try
            {
                ThreadPoolHelper.InvokeThreadPool((o) => {
                    if (_Cache_InnerOrders_Dic.ContainsKey(securityid))
                    {
                        //try
                        //{
                        //    if (!_symbolCountDic.ContainsKey(securityid))
                        //    {
                        //        _symbolCountDic[securityid] = null;
                        //        LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{securityid}.");
                        //        PriceHandler.Instance.RegisterCTPrice(securityid, UpdateCtPrice);
                        //    }

                        //    CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(findData_best.SecurityID);
                        //    if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                        //    {
                        //        findData_best.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                        //        findData_best.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                        //        findData_best.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                        //    }
                        //}
                        //catch (Exception e)
                        //{
                        //    LogHandler.Error(e);
                        //}

                        try
                        {
                            if (!_bestPriceDic.ContainsKey(securityid))
                            {
                                if ((orderSource ?? "1") == "1")
                                {
                                    _bestPriceDic[securityid] = new decimal?[2];
                                    LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{securityid}.");
                                    PriceHandler.Instance.RegisterCTPrice(securityid, UpdateCtPrice);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            LogHandler.Error(e);
                        }
                    }
                    else
                    {
                        // 不注销price订阅
                        //if (_bestPriceDic.ContainsKey(securityid) && !InnerOrders.Any(item => item.SecurityID == securityid))
                        //{
                        //    PriceHandler.Instance.UnregisterCTPrice(securityid, UpdateCtPrice);
                        //}
                    }
                });

                string updateTime = _lastUpdateTimeDic[securityid];

                InnerOrderData findData_best = null;
                IEnumerable<InnerOrderData> bid_InnerOrdersList = null;
                IEnumerable<InnerOrderData> ofr_InnerOrdersList = null;
                int bidCount = 0;
                int ofrCount = 0;
                InnerOrderData myFirstBidInnerData = null;
                InnerOrderData myFirstOfrInnerData = null;

                string hurryFlag = string.Empty;
                string SuggestFlag = string.Empty;
                string fireflag = string.Empty;

                if (securityid == "99999")
                {
                    string primaryKey = $"{securityid}#{orderid}";
                    if (_Temp_Cache_InnerOrders_Dic.ContainsKey(primaryKey) && _Temp_Cache_InnerOrders_Dic[primaryKey] != null)
                    {
                        //updateTime = _Cache_InnerOrders_Dic[securityid][orderid].UpdateTime;

                        bid_InnerOrdersList = _Temp_Cache_InnerOrders_Dic[primaryKey][0].Keys;
                        ofr_InnerOrdersList = _Temp_Cache_InnerOrders_Dic[primaryKey][1].Keys;
                    }
                }
                else
                {
                    IEnumerable<InnerOrderData> findDatas_securityid = null;
                    if (_Temp_Cache_InnerOrders_Dic.ContainsKey(securityid) && _Temp_Cache_InnerOrders_Dic[securityid] != null)
                    {
                        //findDatas_securityid = _Cache_InnerOrders_Dic[securityid].Values.OrderByDescending(item => item.UpdateTime);

                        bid_InnerOrdersList = _Temp_Cache_InnerOrders_Dic[securityid][0].Keys;
                        ofr_InnerOrdersList = _Temp_Cache_InnerOrders_Dic[securityid][1].Keys;

                        hurryFlag = (bid_InnerOrdersList.Any(item => item.HurryFlag == "1") || ofr_InnerOrdersList.Any(item => item.HurryFlag == "1")) ? "1" : string.Empty;
                        SuggestFlag = (bid_InnerOrdersList.Any(item => item.SuggestFlag == "1") || ofr_InnerOrdersList.Any(item => item.SuggestFlag == "1")) ? "1" : string.Empty;
                        fireflag = (bid_InnerOrdersList.Any(item => item.FireFlag == "1") || ofr_InnerOrdersList.Any(item => item.FireFlag == "1") )? "1" : string.Empty;
                    }

                    //if (findDatas_securityid != null && findDatas_securityid.Count() > 0)
                    //{
                    //    updateTime = findDatas_securityid.First().UpdateTime;
                    //}
                }

                if (bid_InnerOrdersList != null)
                {
                    bidCount = bid_InnerOrdersList.Count();

                    IEnumerable<InnerOrderData> myBidList = bid_InnerOrdersList.Where(item => item.IsMe);
                    if (myBidList != null && myBidList.Count() > 0)
                    {
                        myFirstBidInnerData = myBidList.First();
                    }
                }

                if (ofr_InnerOrdersList != null)
                {
                    ofrCount = ofr_InnerOrdersList.Count();

                    IEnumerable<InnerOrderData> myOfrList = ofr_InnerOrdersList.Where(item => item.IsMe);
                    if (myOfrList != null && myOfrList.Count() > 0)
                    {
                        myFirstOfrInnerData = myOfrList.First();
                    }
                }

                this.InnerDepthPopupViewModel.OnInnerDepthCallback(securityid, bid_InnerOrdersList, ofr_InnerOrdersList);

                // side == 1, Bid
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
                // side == 2, Ofr
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

                string key = string.Empty;
                if (securityid == "99999")
                {
                    key = $"{securityid}_{orderid}";
                }
                else
                {
                    key = $"{securityid}";
                }

                if (_Cache_InnerOrders_Dic_Best.ContainsKey(key))
                {
                    findData_best = _Cache_InnerOrders_Dic_Best[key];
                }

                if (findData_best != null)
                {
                    //if (!(er.exectype == "N" || er.exectype == "0" || er.exectype == "1" || er.exectype == "S" || er.exectype == "6"))
                    // 删除全部
                    if (bidCount == 0 && ofrCount == 0)
                    {
                        lock (_dataLock_best)
                        {
                            this.InnerOrders_Best.Remove(findData_best);

                            InnerOrderData removeData;
                            _Cache_InnerOrders_Dic_Best.TryRemove(key, out removeData);

                            //if (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null)
                            {
                                RowCount_Best--;
                                if (removeData.Count_InnerBid > 0)
                                {
                                    BidRowCount_Best--;
                                }
                                if (removeData.Count_InnerOfr > 0)
                                {
                                    OfrRowCount_Best--;
                                }
                            }
                        }
                        return;
                    }
                    else
                    {
                        // 删除Bid单边
                        if (bidCount == 0)
                        {
                            findData_best.PartySubName_InnerBid = string.Empty;
                            findData_best.PartySubId_InnerBid = string.Empty;
                            findData_best.UserName_InnerBid = string.Empty;
                            findData_best.Qty_InnerBid = null;
                            findData_best.CopyInfo_InnerBid = null;
                            lock (_dataLock_best)
                            {
                                if (findData_best.Count_InnerBid > 0)
                                //    && (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null || this.ViewSource_Best.View.Contains(findData_best)))
                                {
                                    BidRowCount_Best--;
                                }
                            }
                            findData_best.Count_InnerBid = 0;
                            findData_best.Price_InnerBid = null;
                            findData_best.CCDCValuation_BP_InnerBid = null;
                            findData_best.CsiValuation_BP_InnerBid = null;
                            findData_best.FirmFlag_InnerBid = string.Empty;
                            findData_best.SettleType_InnerBid = string.Empty;
                            findData_best.RoleId_InnerBid = string.Empty;
                            findData_best.RoleName_InnerBid = string.Empty;
                            findData_best.WholeFlag_InnerBid = string.Empty;
                            findData_best.IsMe_InnerBid = false;
                            findData_best.NetPrice_InnerBid = null;
                            findData_best.FullPrice_InnerBid = null;
                            findData_best.IntentionId_InnerBid = string.Empty;
                            findData_best.StrategyOrderId_innerBid = string.Empty;
                            findData_best.IsBest_InnerBid = string.Empty;
                            findData_best.PartySubName_Tooltip_InnerBid = string.Empty;
                            findData_best.Qty_Tooltip_InnerBid = string.Empty;
                            findData_best.Sign_InnerBid = 0;
                            findData_best.Client_InnerBid = string.Empty;
                            findData_best.TraderID_InnerBid = string.Empty;
                            findData_best.TraderName_InnerBid = string.Empty;
                            findData_best.InstitutionID_InnerBid = string.Empty;
                            findData_best.InstitutionName_InnerBid = string.Empty;
                            findData_best.DQType_InnerBid = string.Empty;
                            findData_best.OrderID = string.Empty;
                            findData_best.HurryFlag_InnerBid = string.Empty;
                            findData_best.SuggestFlag_InnerBid = string.Empty;
                            findData_best.FireFlag_InnerBid = string.Empty;
                            findData_best.FloorPrice_InnerBid = string.Empty;
                            findData_best.OCOFlag_InnerBid = string.Empty;
                            findData_best.PublicRemarks_InnerBid = string.Empty;
                            findData_best.PersonalRemarks_InnerBid = string.Empty;
                            findData_best.FromQQ_Bid = string.Empty;
                            findData_best.Channel_InnerBid = string.Empty;
                            findData_best.Info2_InnerBid = string.Empty;
                            findData_best.Info1_InnerBid = string.Empty;
                            findData_best.PriceType_InnerBid = string.Empty;
                            findData_best.RiskInfo = string.Empty;
                            findData_best.IsRisk = false;
                            findData_best.Price_Display_InnerBid = null;
                        }

                        // 删除Ofr单边
                        if (ofrCount == 0)
                        {
                            findData_best.PartySubName_InnerOfr = string.Empty;
                            findData_best.PartySubId_InnerOfr = string.Empty;
                            findData_best.UserName_InnerOfr = string.Empty;
                            findData_best.Qty_InnerOfr = null;
                            findData_best.CopyInfo_InnerOfr = null;
                            
                            lock (_dataLock_best)
                            {
                                if (findData_best.Count_InnerOfr > 0)
                                //   && (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null || this.ViewSource_Best.View.Contains(findData_best)))
                                {
                                    OfrRowCount_Best--;
                                }
                            }
                            findData_best.Count_InnerOfr = 0;

                            findData_best.Price_InnerOfr = null;
                            findData_best.CCDCValuation_BP_InnerOfr = null;
                            findData_best.CsiValuation_BP_InnerOfr = null;
                            findData_best.FirmFlag_InnerOfr = string.Empty;
                            findData_best.SettleType_InnerOfr = string.Empty;
                            findData_best.RoleId_InnerOfr = string.Empty;
                            findData_best.RoleName_InnerOfr = string.Empty;
                            findData_best.WholeFlag_InnerOfr = string.Empty;
                            findData_best.IsMe_InnerOfr = false;
                            findData_best.NetPrice_InnerOfr = null;
                            findData_best.FullPrice_InnerOfr = null;
                            findData_best.IntentionId_InnerOfr = string.Empty;
                            findData_best.StrategyOrderId_InnerOfr = string.Empty;
                            findData_best.IsBest_InnerOfr = string.Empty;
                            findData_best.PartySubName_Tooltip_InnerOfr = string.Empty;
                            findData_best.Qty_Tooltip_InnerOfr = string.Empty;
                            findData_best.Sign_InnerOfr = 0;
                            findData_best.Client_InnerOfr = string.Empty;
                            findData_best.TraderID_InnerOfr = string.Empty;
                            findData_best.TraderName_InnerOfr = string.Empty;
                            findData_best.InstitutionID_InnerOfr = string.Empty;
                            findData_best.InstitutionName_InnerOfr = string.Empty;
                            findData_best.DQType_InnerOfr = string.Empty;
                            findData_best.OrderID = string.Empty;
                            findData_best.HurryFlag_InnerOfr = string.Empty;
                            findData_best.SuggestFlag_InnerOfr = string.Empty;
                            findData_best.FireFlag_InnerOfr = string.Empty;
                            findData_best.FloorPrice_InnerOfr = string.Empty;
                            findData_best.OCOFlag_InnerOfr = string.Empty;
                            findData_best.PublicRemarks_InnerOfr = string.Empty;
                            findData_best.PersonalRemarks_InnerOfr = string.Empty;
                            findData_best.FromQQ_Ofr = string.Empty;
                            findData_best.Channel_InnerOfr = string.Empty;
                            findData_best.Info2_InnerOfr = string.Empty;
                            findData_best.Info1_InnerOfr = string.Empty;
                            findData_best.PriceType_InnerOfr = string.Empty;
                            findData_best.Price_Display_InnerOfr = null;
                        }
                    }

                    findData_best.UpdateTime = updateTime;


                    if (VerticalOffset_Best > 0)
                    {
                        UpdateDataCount_Best++;
                    }
                    else
                    {
                        UpdateDataCount_Best = 0;
                    }
                    findData_best.SecurityID = securityid;
                    findData_best.Symbol = symbol;
                    findData_best.UpdateTime = updateTime;
                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityid);
                    if (bondsInfoPo != null)
                    {
                        //Symbol = bondsInfoPo.Short_Name;
                        //Security = bondsInfoPo.Bond_ID;
                        //SymbolName = bondsInfoPo.Short_Name;
                        //findData_best.RemainingTime = bondsInfoPo.RemaingingTime;
                        findData_best.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        findData_best.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        findData_best.BondSubType = bondsInfoPo.Bond_Subtype;
                        //findData_best.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                        //findData_best.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //findData_best.Issuer = bondsInfoPo.issuer;
                        //findData_best.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        findData_best.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    }
                    findData_best.HurryFlag = hurryFlag;
                    findData_best.SuggestFlag = SuggestFlag;
                    findData_best.FireFlag = fireflag;
                   
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

                        findData_best.RemainingTime = bestBidInnerOrder.RemainingTime;
                        findData_best.RemaingingTime_convert = bestBidInnerOrder.RemaingingTime_convert;
                        findData_best.TermDays = bestBidInnerOrder.TermDays;
                        findData_best.CouponRateCurrent = bestBidInnerOrder.CouponRateCurrent;
                        findData_best.IssuerCode = bestBidInnerOrder.IssuerCode;
                        findData_best.IssuerName = bestBidInnerOrder.IssuerName;
                        findData_best.WarranterCode = bestBidInnerOrder.WarranterCode;
                        findData_best.WarranterName = bestBidInnerOrder.WarranterName;
                        //findData_best.RemainingTime_New = bestBidInnerOrder.RemainingTime_New;
                        findData_best.ChinaBondValuation = bestBidInnerOrder.ChinaBondValuation;
                        findData_best.ChinaBondValuationFull = bestBidInnerOrder.ChinaBondValuationFull;
                        findData_best.ChinaBondExercise = bestBidInnerOrder.ChinaBondExercise;
                        findData_best.ChinaBondDue = bestBidInnerOrder.ChinaBondDue;
                        findData_best.CCDCValuation_BP = bestBidInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP = bestBidInnerOrder.CsiValuation_BP;
                        findData_best.CsiBondValuation = bestBidInnerOrder.CsiBondValuation;
                        findData_best.CsiBondValuationFull = bestBidInnerOrder.CsiBondValuationFull;
                        findData_best.CsiBondValuationExercise = bestBidInnerOrder.CsiBondValuationExercise;
                        findData_best.CsiBondValuationDue = bestBidInnerOrder.CsiBondValuationDue;
                        findData_best.ModifiedDuration = bestBidInnerOrder.ModifiedDuration;
                        findData_best.DayOffStr = bestBidInnerOrder.DayOffStr;
                        findData_best.Rating = bestBidInnerOrder.Rating;
                        findData_best.Cdc_Implited_Rating = bestBidInnerOrder.Cdc_Implited_Rating;
                        findData_best.BondCategory = bestBidInnerOrder.BondCategory;
                        findData_best.Level = bestBidInnerOrder.Level;
                        findData_best.RegionType = bestBidInnerOrder.RegionType;
                        findData_best.EnterpriseType = bestBidInnerOrder.EnterpriseType;
                        findData_best.If_Guarantee = bestBidInnerOrder.If_Guarantee;
                        findData_best.If_Repay = bestBidInnerOrder.If_Repay;
                        findData_best.IssueType = bestBidInnerOrder.IssueType;
                        findData_best.IssueAmount = bestBidInnerOrder.IssueAmount;
                        findData_best.Outstanding_Amount = bestBidInnerOrder.Outstanding_Amount;
                        findData_best.PartySubName_InnerBid = partySubNameList.Count > 1 ? "多" : bestBidInnerOrder.PartySubName;
                        findData_best.PartySubId_InnerBid = bestBidInnerOrder.PartySubId;
                        findData_best.UserName_InnerBid = bestBidInnerOrder.UserName;
                        findData_best.PartySubName_Tooltip_InnerBid = partySubNameTooltip.ToString();
                        findData_best.Qty_InnerBid = qty == 0 ? null : qty;
                        findData_best.CopyInfo_InnerBid = copyInfo.ToString();
                        findData_best.Qty_Tooltip_InnerBid = qtyTooltip.ToString();

                        //int count = bid_InnerOrdersList.Count(item => item.Price == bestBidInnerOrder.Price);
                        lock (_dataLock_best)
                        {
                            if (findData_best.Count_InnerBid == 0)
                            //    && (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null || this.ViewSource_Best.View.Contains(findData_best)))
                            {
                                BidRowCount_Best++;
                            }
                        }
                        findData_best.Count_InnerBid = bidCount;
                        findData_best.Price_InnerBid = bestBidInnerOrder.Price;
                        findData_best.CCDCValuation_BP_InnerBid = bestBidInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP_InnerBid = bestBidInnerOrder.CsiValuation_BP;
                        findData_best.FirmFlag_InnerBid = bestBidInnerOrder.FirmFlag;
                        findData_best.SettleType_InnerBid = bestBidInnerOrder.SettleType;
                        findData_best.RoleId_InnerBid = bestBidInnerOrder.RoleId;
                        findData_best.RoleName_InnerBid = bestBidInnerOrder.RoleName;
                        findData_best.WholeFlag_InnerBid = bestBidInnerOrder.WholeFlag;
                        findData_best.IsMe_InnerBid = bestBidInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                        findData_best.IntentionId_InnerBid = bestBidInnerOrder.IntentionID;
                        findData_best.NetPrice_InnerBid = bestBidInnerOrder.NetPrice;
                        findData_best.FullPrice_InnerBid = bestBidInnerOrder.FullPrice;
                        findData_best.OrderID_InnerBid = bestBidInnerOrder.OrderID;
                        findData_best.StrategyOrderId_innerBid = bestBidInnerOrder.StrategyOrderId;
                        findData_best.Sign_InnerBid = bestBidInnerOrder.Sign;
                        findData_best.Sign_InnerBid1 = bestBidInnerOrder.Sign1;
                        findData_best.Sign_InnerBid2 = bestBidInnerOrder.Sign2;
                        findData_best.Sign_InnerBid3 = bestBidInnerOrder.Sign3;
                        findData_best.Sign_InnerBid4 = bestBidInnerOrder.Sign4;
                        findData_best.Client_InnerBid = bestBidInnerOrder.Client;
                        findData_best.TraderID_InnerBid = bestBidInnerOrder.TraderID;
                        findData_best.TraderName_InnerBid = bestBidInnerOrder.TraderName;
                        findData_best.InstitutionID_InnerBid = bestBidInnerOrder.InstitutionID;
                        findData_best.InstitutionName_InnerBid = bestBidInnerOrder.InstitutionName;
                        findData_best.DQType_InnerBid = bestBidInnerOrder.DQType;
                        findData_best.HurryFlag_InnerBid = bestBidInnerOrder.HurryFlag;
                        findData_best.SuggestFlag_InnerBid = bestBidInnerOrder.SuggestFlag;
                        findData_best.FireFlag_InnerBid = bestBidInnerOrder.FireFlag;
                        findData_best.FloorPrice_InnerBid = bestBidInnerOrder.FloorPrice;
                        findData_best.OCOFlag_InnerBid = bestBidInnerOrder.OCOFlag;
                        findData_best.PublicRemarks_InnerBid = bestBidInnerOrder.PublicRemarks;
                        findData_best.PersonalRemarks_InnerBid = findData_best.IsMe_InnerBid ? bestBidInnerOrder.PersonalRemarks : string.Empty;
                        findData_best.FromQQ_Bid = bestBidInnerOrder.FromQQ;
                        findData_best.Channel_InnerBid = bestBidInnerOrder.Channel;
                        findData_best.Info2_InnerBid = bestBidInnerOrder.Info2;
                        findData_best.Info1_InnerBid = bestBidInnerOrder.Info1;
                        findData_best.PriceType_InnerBid = bestBidInnerOrder.PriceType;

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

                        if (findData_best.PriceType_InnerBid == "1")
                        {
                            findData_best.Price_Display_InnerBid = findData_best.NetPrice_InnerBid;
                        }
                        else if (findData_best.PriceType_InnerBid == "2")
                        {
                            findData_best.Price_Display_InnerBid = findData_best.FullPrice_InnerBid;
                        }
                        else
                        {
                            findData_best.Price_Display_InnerBid = findData_best.Price_InnerBid;
                        }

                        if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                        {
                            findData_best.RiskInfo = bestBidInnerOrder.RiskInfo;
                            findData_best.IsRisk = string.IsNullOrEmpty(bestBidInnerOrder.RiskInfo) ? false : true;
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

                        findData_best.RemainingTime = bestOfrInnerOrder.RemainingTime;
                        findData_best.RemaingingTime_convert = bestOfrInnerOrder.RemaingingTime_convert;
                        findData_best.PartySubName_InnerOfr = partySubNameList.Count > 1 ? "多" : bestOfrInnerOrder.PartySubName;
                        findData_best.PartySubId_InnerOfr = bestOfrInnerOrder.PartySubId;
                        findData_best.UserName_InnerOfr = bestOfrInnerOrder.UserName;
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
                        findData_best.ChinaBondValuationFull = bestOfrInnerOrder.ChinaBondValuationFull;
                        findData_best.ChinaBondExercise = bestOfrInnerOrder.ChinaBondExercise;
                        findData_best.ChinaBondDue = bestOfrInnerOrder.ChinaBondDue;
                        findData_best.CCDCValuation_BP = bestOfrInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP = bestOfrInnerOrder.CsiValuation_BP;
                        findData_best.CsiBondValuation = bestOfrInnerOrder.CsiBondValuation;
                        findData_best.CsiBondValuationFull = bestOfrInnerOrder.CsiBondValuationFull;
                        findData_best.CsiBondValuationExercise = bestOfrInnerOrder.CsiBondValuationExercise;
                        findData_best.CsiBondValuationDue = bestOfrInnerOrder.CsiBondValuationDue;
                        findData_best.ModifiedDuration = bestOfrInnerOrder.ModifiedDuration;
                        findData_best.DayOffStr = bestOfrInnerOrder.DayOffStr;
                        findData_best.Rating = bestOfrInnerOrder.Rating;
                        findData_best.Cdc_Implited_Rating = bestOfrInnerOrder.Cdc_Implited_Rating;
                        findData_best.BondCategory = bestOfrInnerOrder.BondCategory;
                        findData_best.Level = bestOfrInnerOrder.Level;
                        findData_best.RegionType = bestOfrInnerOrder.RegionType;
                        findData_best.EnterpriseType = bestOfrInnerOrder.EnterpriseType;
                        findData_best.If_Guarantee = bestOfrInnerOrder.If_Guarantee;
                        findData_best.If_Repay = bestOfrInnerOrder.If_Repay;
                        findData_best.IssueType = bestOfrInnerOrder.IssueType;
                        findData_best.IssueAmount = bestOfrInnerOrder.IssueAmount;
                        findData_best.Outstanding_Amount = bestOfrInnerOrder.Outstanding_Amount;
                        findData_best.Price_InnerOfr = bestOfrInnerOrder.Price;
                        findData_best.CCDCValuation_BP_InnerOfr = bestOfrInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP_InnerOfr = bestOfrInnerOrder.CsiValuation_BP;
                        //int count = ofr_InnerOrdersList.Count(item => item.Price == bestOfrInnerOrder.Price);
                        lock (_dataLock_best)
                        {
                            if (findData_best.Count_InnerOfr == 0)
                            //    && (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null || this.ViewSource_Best.View.Contains(findData_best)))
                            {
                                OfrRowCount_Best++;
                            }
                        }
                        findData_best.Count_InnerOfr = ofrCount;
                        findData_best.FirmFlag_InnerOfr = bestOfrInnerOrder.FirmFlag;
                        findData_best.SettleType_InnerOfr = bestOfrInnerOrder.SettleType;
                        findData_best.RoleId_InnerOfr = bestOfrInnerOrder.RoleId;
                        findData_best.RoleName_InnerOfr = bestOfrInnerOrder.RoleName;
                        findData_best.WholeFlag_InnerOfr = bestOfrInnerOrder.WholeFlag;
                        findData_best.IsMe_InnerOfr = bestOfrInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                        findData_best.IntentionId_InnerOfr = bestOfrInnerOrder.IntentionID;
                        findData_best.NetPrice_InnerOfr = bestOfrInnerOrder.NetPrice;
                        findData_best.FullPrice_InnerOfr = bestOfrInnerOrder.FullPrice;
                        findData_best.OrderID_InnerOfr = bestOfrInnerOrder.OrderID;
                        findData_best.StrategyOrderId_InnerOfr = bestOfrInnerOrder.StrategyOrderId;
                        findData_best.Sign_InnerOfr = bestOfrInnerOrder.Sign;
                        findData_best.Sign_InnerOfr1 = bestOfrInnerOrder.Sign1;
                        findData_best.Sign_InnerOfr2 = bestOfrInnerOrder.Sign2;
                        findData_best.Sign_InnerOfr3 = bestOfrInnerOrder.Sign3;
                        findData_best.Sign_InnerOfr4 = bestOfrInnerOrder.Sign4;
                        findData_best.Client_InnerOfr = bestOfrInnerOrder.Client;
                        findData_best.TraderID_InnerOfr = bestOfrInnerOrder.TraderID;
                        findData_best.TraderName_InnerOfr = bestOfrInnerOrder.TraderName;
                        findData_best.InstitutionID_InnerOfr = bestOfrInnerOrder.InstitutionID;
                        findData_best.InstitutionName_InnerOfr = bestOfrInnerOrder.InstitutionName;
                        findData_best.DQType_InnerOfr = bestOfrInnerOrder.DQType;
                        findData_best.HurryFlag_InnerOfr = bestOfrInnerOrder.HurryFlag;
                        findData_best.SuggestFlag_InnerOfr = bestOfrInnerOrder.SuggestFlag;
                        findData_best.FireFlag_InnerOfr = bestOfrInnerOrder.FireFlag;
                        findData_best.FloorPrice_InnerOfr = bestOfrInnerOrder.FloorPrice;
                        findData_best.OCOFlag_InnerOfr = bestOfrInnerOrder.OCOFlag;
                        findData_best.PublicRemarks_InnerOfr = bestOfrInnerOrder.PublicRemarks;
                        findData_best.PersonalRemarks_InnerOfr = findData_best.IsMe_InnerOfr ? bestOfrInnerOrder.PersonalRemarks : string.Empty;
                        findData_best.FromQQ_Ofr = bestOfrInnerOrder.FromQQ;
                        findData_best.Channel_InnerOfr = bestOfrInnerOrder.Channel;
                        findData_best.Info2_InnerOfr = bestOfrInnerOrder.Info2;
                        findData_best.Info1_InnerOfr = bestOfrInnerOrder.Info1;
                        findData_best.PriceType_InnerOfr = bestOfrInnerOrder.PriceType;

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

                        if (findData_best.PriceType_InnerOfr == "1")
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.NetPrice_InnerOfr;
                        }
                        else if (findData_best.PriceType_InnerOfr == "2")
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.FullPrice_InnerOfr;
                        }
                        else
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.Price_InnerOfr;
                        }
                    }
                
                    if (bidCount > 0)
                    {
                        findData_best.Tax_Free = bid_InnerOrdersList.First().Tax_Free;
                    }
                    else if (ofrCount > 0)
                    {
                        findData_best.Tax_Free = ofr_InnerOrdersList.First().Tax_Free;
                    }
                    else
                    {
                        findData_best.Tax_Free = string.Empty;
                    }
                }
                else
                {
                    //if (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C")
                    if (bidCount == 0 && ofrCount == 0)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(securityid))
                    {
                        return;
                    }

                    if (VerticalOffset_Best > 0)
                    {
                        UpdateDataCount_Best++;
                    }
                    else
                    {
                        UpdateDataCount_Best = 0;
                    }
                    findData_best = new InnerOrderData();

                    findData_best.SecurityID = securityid;
                    findData_best.Symbol = symbol;
                    findData_best.UpdateTime = updateTime;

                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityid);
                    if (bondsInfoPo != null)
                    {
                        //Symbol = bondsInfoPo.Short_Name;
                        //Security = bondsInfoPo.Bond_ID;
                        //SymbolName = bondsInfoPo.Short_Name;
                        //findData_best.RemainingTime = bondsInfoPo.RemaingingTime;
                        findData_best.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        findData_best.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        findData_best.BondSubType = bondsInfoPo.Bond_Subtype;
                        //findData_best.Coupon_Rate_Current = CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current);
                        //findData_best.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //findData_best.Issuer = bondsInfoPo.issuer;
                        //findData_best.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        findData_best.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    }
                    findData_best.HurryFlag = hurryFlag;
                    findData_best.SuggestFlag = SuggestFlag;
                    findData_best.FireFlag = fireflag;
                    

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

                        findData_best.RemainingTime = bestBidInnerOrder.RemainingTime;
                        findData_best.RemaingingTime_convert = bestBidInnerOrder.RemaingingTime_convert;
                        findData_best.TermDays = bestBidInnerOrder.TermDays;
                        findData_best.CouponRateCurrent = bestBidInnerOrder.CouponRateCurrent;
                        findData_best.IssuerCode = bestBidInnerOrder.IssuerCode;
                        findData_best.IssuerName = bestBidInnerOrder.IssuerName;
                        findData_best.WarranterCode = bestBidInnerOrder.WarranterCode;
                        findData_best.WarranterName = bestBidInnerOrder.WarranterName;
                        //findData_best.RemainingTime_New = bestBidInnerOrder.RemainingTime_New;
                        findData_best.ChinaBondValuation = bestBidInnerOrder.ChinaBondValuation;
                        findData_best.ChinaBondValuationFull = bestBidInnerOrder.ChinaBondValuationFull;
                        findData_best.ChinaBondExercise = bestBidInnerOrder.ChinaBondExercise;
                        findData_best.ChinaBondDue = bestBidInnerOrder.ChinaBondDue;
                        findData_best.CCDCValuation_BP = bestBidInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP = bestBidInnerOrder.CsiValuation_BP;
                        findData_best.CsiBondValuation = bestBidInnerOrder.CsiBondValuation;
                        findData_best.CsiBondValuationFull = bestBidInnerOrder.CsiBondValuationFull;
                        findData_best.CsiBondValuationExercise = bestBidInnerOrder.CsiBondValuationExercise;
                        findData_best.CsiBondValuationDue = bestBidInnerOrder.CsiBondValuationDue;
                        findData_best.ModifiedDuration = bestBidInnerOrder.ModifiedDuration;
                        findData_best.DayOffStr = bestBidInnerOrder.DayOffStr;
                        findData_best.Rating = bestBidInnerOrder.Rating;
                        findData_best.Cdc_Implited_Rating = bestBidInnerOrder.Cdc_Implited_Rating;
                        findData_best.BondCategory = bestBidInnerOrder.BondCategory;
                        findData_best.Level = bestBidInnerOrder.Level;
                        findData_best.RegionType = bestBidInnerOrder.RegionType;
                        findData_best.EnterpriseType = bestBidInnerOrder.EnterpriseType;
                        findData_best.If_Guarantee = bestBidInnerOrder.If_Guarantee;
                        findData_best.If_Repay = bestBidInnerOrder.If_Repay;
                        findData_best.IssueType = bestBidInnerOrder.IssueType;
                        findData_best.IssueAmount = bestBidInnerOrder.IssueAmount;
                        findData_best.Outstanding_Amount = bestBidInnerOrder.Outstanding_Amount;
                        findData_best.PartySubName_InnerBid = partySubNameList.Count > 1 ? "多" : bestBidInnerOrder.PartySubName;
                        findData_best.PartySubId_InnerBid = bestBidInnerOrder.PartySubId;
                        findData_best.UserName_InnerBid = bestBidInnerOrder.UserName;
                        findData_best.PartySubName_Tooltip_InnerBid = partySubNameTooltip.ToString();
                        findData_best.Qty_InnerBid = qty == 0 ? null : qty;
                        findData_best.CopyInfo_InnerBid = copyInfo.ToString();
                        findData_best.Qty_Tooltip_InnerBid = qtyTooltip.ToString();
                        //int count = bid_InnerOrdersList.Count(item => item.Price == bestBidInnerOrder.Price);
                        findData_best.Count_InnerBid = bidCount;
                        findData_best.Price_InnerBid = bestBidInnerOrder.Price;
                        findData_best.CCDCValuation_BP_InnerBid = bestBidInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP_InnerBid = bestBidInnerOrder.CsiValuation_BP;
                        findData_best.FirmFlag_InnerBid = bestBidInnerOrder.FirmFlag;
                        findData_best.SettleType_InnerBid = bestBidInnerOrder.SettleType;
                        findData_best.RoleId_InnerBid = bestBidInnerOrder.RoleId;
                        findData_best.RoleName_InnerBid = bestBidInnerOrder.RoleName;
                        findData_best.WholeFlag_InnerBid = bestBidInnerOrder.WholeFlag;
                        findData_best.IsMe_InnerBid = bestBidInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                        findData_best.IntentionId_InnerBid = bestBidInnerOrder.IntentionID;
                        findData_best.NetPrice_InnerBid = bestBidInnerOrder.NetPrice;
                        findData_best.FullPrice_InnerBid = bestBidInnerOrder.FullPrice;
                        findData_best.OrderID_InnerBid = bestBidInnerOrder.OrderID;
                        findData_best.StrategyOrderId_innerBid = bestBidInnerOrder.StrategyOrderId;
                        findData_best.Sign_InnerBid = bestBidInnerOrder.Sign;
                        findData_best.Sign_InnerBid1 = bestBidInnerOrder.Sign1;
                        findData_best.Sign_InnerBid2 = bestBidInnerOrder.Sign2;
                        findData_best.Sign_InnerBid3 = bestBidInnerOrder.Sign3;
                        findData_best.Sign_InnerBid4 = bestBidInnerOrder.Sign4;
                        findData_best.Client_InnerBid = bestBidInnerOrder.Client;
                        findData_best.TraderID_InnerBid = bestBidInnerOrder.TraderID;
                        findData_best.TraderName_InnerBid = bestBidInnerOrder.TraderName;
                        findData_best.InstitutionID_InnerBid = bestBidInnerOrder.InstitutionID;
                        findData_best.InstitutionName_InnerBid = bestBidInnerOrder.InstitutionName;
                        findData_best.DQType_InnerBid = bestBidInnerOrder.DQType;
                        findData_best.HurryFlag_InnerBid = bestBidInnerOrder.HurryFlag;
                        findData_best.SuggestFlag_InnerBid = bestBidInnerOrder.SuggestFlag;
                        findData_best.FireFlag_InnerBid = bestBidInnerOrder.FireFlag;
                        findData_best.FloorPrice_InnerBid = bestBidInnerOrder.FloorPrice;
                        findData_best.OCOFlag_InnerBid = bestBidInnerOrder.OCOFlag;
                        findData_best.PublicRemarks_InnerBid = bestBidInnerOrder.PublicRemarks;
                        findData_best.PersonalRemarks_InnerBid = findData_best.IsMe_InnerBid ? bestBidInnerOrder.PersonalRemarks : string.Empty;
                        findData_best.FromQQ_Bid = bestBidInnerOrder.FromQQ;
                        findData_best.Channel_InnerBid = bestBidInnerOrder.Channel;
                        findData_best.Info2_InnerBid = bestBidInnerOrder.Info2;
                        findData_best.Info1_InnerBid = bestBidInnerOrder.Info1;
                        findData_best.PriceType_InnerBid = bestBidInnerOrder.PriceType;

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

                        if (findData_best.PriceType_InnerBid == "1")
                        {
                            findData_best.Price_Display_InnerBid = findData_best.NetPrice_InnerBid;
                        }
                        else if (findData_best.PriceType_InnerBid == "2")
                        {
                            findData_best.Price_Display_InnerBid = findData_best.FullPrice_InnerBid;
                        }
                        else
                        {
                            findData_best.Price_Display_InnerBid = findData_best.Price_InnerBid;
                        }

                        if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                        {
                            findData_best.RiskInfo = bestBidInnerOrder.RiskInfo;
                            findData_best.IsRisk = string.IsNullOrEmpty(bestBidInnerOrder.RiskInfo) ? false : true;
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

                        findData_best.RemainingTime = bestOfrInnerOrder.RemainingTime;
                        findData_best.RemaingingTime_convert = bestOfrInnerOrder.RemaingingTime_convert;
                        findData_best.PartySubName_InnerOfr = partySubNameList.Count > 1 ? "多" : bestOfrInnerOrder.PartySubName;
                        findData_best.PartySubId_InnerOfr = bestOfrInnerOrder.PartySubId;
                        findData_best.UserName_InnerOfr = bestOfrInnerOrder.UserName;
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
                        findData_best.ChinaBondValuationFull = bestOfrInnerOrder.ChinaBondValuationFull;
                        findData_best.ChinaBondExercise = bestOfrInnerOrder.ChinaBondExercise;
                        findData_best.ChinaBondDue = bestOfrInnerOrder.ChinaBondDue;
                        findData_best.CCDCValuation_BP = bestOfrInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP = bestOfrInnerOrder.CsiValuation_BP;
                        findData_best.CsiBondValuation = bestOfrInnerOrder.CsiBondValuation;
                        findData_best.CsiBondValuationFull = bestOfrInnerOrder.CsiBondValuationFull;
                        findData_best.CsiBondValuationExercise = bestOfrInnerOrder.CsiBondValuationExercise;
                        findData_best.CsiBondValuationDue = bestOfrInnerOrder.CsiBondValuationDue;
                        findData_best.ModifiedDuration = bestOfrInnerOrder.ModifiedDuration;
                        findData_best.DayOffStr = bestOfrInnerOrder.DayOffStr;
                        findData_best.Rating = bestOfrInnerOrder.Rating;
                        findData_best.Cdc_Implited_Rating = bestOfrInnerOrder.Cdc_Implited_Rating;
                        findData_best.BondCategory = bestOfrInnerOrder.BondCategory;
                        findData_best.Level = bestOfrInnerOrder.Level;
                        findData_best.RegionType = bestOfrInnerOrder.RegionType;
                        findData_best.EnterpriseType = bestOfrInnerOrder.EnterpriseType;
                        findData_best.If_Guarantee = bestOfrInnerOrder.If_Guarantee;
                        findData_best.If_Repay = bestOfrInnerOrder.If_Repay;
                        findData_best.IssueType = bestOfrInnerOrder.IssueType;
                        findData_best.IssueAmount = bestOfrInnerOrder.IssueAmount;
                        findData_best.Outstanding_Amount = bestOfrInnerOrder.Outstanding_Amount;
                        findData_best.Price_InnerOfr = bestOfrInnerOrder.Price;
                        findData_best.CCDCValuation_BP_InnerOfr = bestOfrInnerOrder.CCDCValuation_BP;
                        findData_best.CsiValuation_BP_InnerOfr = bestOfrInnerOrder.CsiValuation_BP;
                        //int count = ofr_InnerOrdersList.Count(item => item.Price == bestOfrInnerOrder.Price);
                        findData_best.Count_InnerOfr = ofrCount;
                        findData_best.FirmFlag_InnerOfr = bestOfrInnerOrder.FirmFlag;
                        findData_best.SettleType_InnerOfr = bestOfrInnerOrder.SettleType;
                        findData_best.RoleId_InnerOfr = bestOfrInnerOrder.RoleId;
                        findData_best.RoleName_InnerOfr = bestOfrInnerOrder.RoleName;
                        findData_best.WholeFlag_InnerOfr = bestOfrInnerOrder.WholeFlag;
                        findData_best.IsMe_InnerOfr = bestOfrInnerOrder.UserName == LocalEnv.UserInfo.UserName;
                        findData_best.IntentionId_InnerOfr = bestOfrInnerOrder.IntentionID;
                        findData_best.NetPrice_InnerOfr = bestOfrInnerOrder.NetPrice;
                        findData_best.FullPrice_InnerOfr = bestOfrInnerOrder.FullPrice;
                        findData_best.OrderID_InnerOfr = bestOfrInnerOrder.OrderID;
                        findData_best.StrategyOrderId_InnerOfr = bestOfrInnerOrder.StrategyOrderId;
                        findData_best.Sign_InnerOfr = bestOfrInnerOrder.Sign;
                        findData_best.Sign_InnerOfr1 = bestOfrInnerOrder.Sign1;
                        findData_best.Sign_InnerOfr2 = bestOfrInnerOrder.Sign2;
                        findData_best.Sign_InnerOfr3 = bestOfrInnerOrder.Sign3;
                        findData_best.Sign_InnerOfr4 = bestOfrInnerOrder.Sign4;
                        findData_best.Client_InnerOfr = bestOfrInnerOrder.Client;
                        findData_best.TraderID_InnerOfr = bestOfrInnerOrder.TraderID;
                        findData_best.TraderName_InnerOfr = bestOfrInnerOrder.TraderName;
                        findData_best.InstitutionID_InnerOfr = bestOfrInnerOrder.InstitutionID;
                        findData_best.InstitutionName_InnerOfr = bestOfrInnerOrder.InstitutionName;
                        findData_best.DQType_InnerOfr = bestOfrInnerOrder.DQType;
                        findData_best.HurryFlag_InnerOfr = bestOfrInnerOrder.HurryFlag;
                        findData_best.SuggestFlag_InnerOfr = bestOfrInnerOrder.SuggestFlag;
                        findData_best.FireFlag_InnerOfr = bestOfrInnerOrder.FireFlag;
                        findData_best.FloorPrice_InnerOfr = bestOfrInnerOrder.FloorPrice;
                        findData_best.OCOFlag_InnerOfr = bestOfrInnerOrder.OCOFlag;
                        findData_best.PublicRemarks_InnerOfr = bestOfrInnerOrder.PublicRemarks;
                        findData_best.PersonalRemarks_InnerOfr = findData_best.IsMe_InnerOfr ? bestOfrInnerOrder.PersonalRemarks : string.Empty;
                        findData_best.FromQQ_Ofr = bestOfrInnerOrder.FromQQ;
                        findData_best.Channel_InnerOfr = bestOfrInnerOrder.Channel;
                        findData_best.Info2_InnerOfr = bestOfrInnerOrder.Info2;
                        findData_best.Info1_InnerOfr = bestOfrInnerOrder.Info1;
                        findData_best.PriceType_InnerOfr = bestOfrInnerOrder.PriceType;

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

                        if (findData_best.PriceType_InnerOfr == "1")
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.NetPrice_InnerOfr;
                        }
                        else if (findData_best.PriceType_InnerOfr == "2")
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.FullPrice_InnerOfr;
                        }
                        else
                        {
                            findData_best.Price_Display_InnerOfr = findData_best.Price_InnerOfr;
                        }
                    }

                    if (bidCount > 0)
                    {
                        findData_best.Tax_Free = bid_InnerOrdersList.First().Tax_Free;
                    }
                    else if (ofrCount > 0)
                    {
                        findData_best.Tax_Free = ofr_InnerOrdersList.First().Tax_Free;
                    }
                    else
                    {
                        findData_best.Tax_Free = string.Empty;
                    }

                    //findData_best.IsNeedFlash = isNew;
                    if (Common.Utils.CommonUtils.IsBDBAppName())
                    {
                        UpdateLastExecutions_BDB(findData_best);
                    }
                    else
                    {
                        UpdateLastExecutions(findData_best);
                    }

                    lock (_dataLock_best)
                    {
                        InnerOrders_Best.Insert(0, findData_best);

                        _Cache_InnerOrders_Dic_Best[key] = findData_best;

                        //if (Application.Current.CheckAccess() == false || this.ViewSource_Best.View == null)
                        {
                            RowCount_Best++;

                            if (bidCount > 0)
                            {
                                BidRowCount_Best++;
                            }
                            if (ofrCount > 0)
                            {
                                OfrRowCount_Best++;
                            }
                        }
                    }

                    
                }

                //if (((side == "1" && findData_best.OrderID_InnerBid == orderid) 
                //      || (side == "2" && findData_best.OrderID_InnerOfr == orderid)) && isNew)
                {
                    _flashDic_best[key] = findData_best;
                }

                if (_bestPriceDic.ContainsKey(securityid))
                {
                    findData_best.Best_Bid = _bestPriceDic[securityid][0];
                    if (findData_best.Price_InnerBid.HasValue && (findData_best.Best_Bid == null || findData_best.Best_Bid >= findData_best.Price_InnerBid) && findData_best.Count_InnerBid > 0)
                    {
                        findData_best.IsMarketBest_InnerBid = "1"; // 市场最优，大拇指
                    }
                    else
                    {
                        findData_best.IsMarketBest_InnerBid = "0";
                    }

                    findData_best.Best_Ofr = _bestPriceDic[securityid][1];
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
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        internal void Sub_FilterBestData(InnerOrderData data, bool isDel = false)
        {

            string primaryKey = data.SecurityID;
            if (data.SecurityID == "99999")
            {
                primaryKey = $"{data.SecurityID}#{data.OrderID}";
            }

            if (isDel)
            {
                if (_Temp_Cache_InnerOrders_Dic.ContainsKey(primaryKey))
                {
                    if (_Temp_Cache_InnerOrders_Dic[primaryKey].Length ==2)
                    {
                        if (data.Side == "1") // bid
                        {
                            _Temp_Cache_InnerOrders_Dic[primaryKey][0].Remove(data);
                        }
                        else if (data.Side == "2") //
                        {
                            _Temp_Cache_InnerOrders_Dic[primaryKey][1].Remove(data);
                        }

                        if (_Temp_Cache_InnerOrders_Dic[primaryKey][0].Count == 0
                            && _Temp_Cache_InnerOrders_Dic[primaryKey][1].Count == 0)
                        {
                            SortedDictionary<InnerOrderData, object>[] removeDic = null;
                            _Temp_Cache_InnerOrders_Dic.TryRemove(primaryKey, out removeDic);
                        }
                    }
                }
            }
            else
            {
                bool result = SetFilterDatas_Best_New(data);

                if (result)
                {
                    if (!_Temp_Cache_InnerOrders_Dic.ContainsKey(primaryKey))
                    {
                        _Temp_Cache_InnerOrders_Dic[primaryKey] = new SortedDictionary<InnerOrderData, object>[2];
                        _Temp_Cache_InnerOrders_Dic[primaryKey][0] = new SortedDictionary<InnerOrderData, object>(bidPriceComparer);
                        _Temp_Cache_InnerOrders_Dic[primaryKey][1] = new SortedDictionary<InnerOrderData, object>(ofrPriceComparer);
                    }

                    if (data.Side == "1") // bid
                    {
                        _Temp_Cache_InnerOrders_Dic[primaryKey][0][data] = null;
                    }
                    else if (data.Side == "2") //
                    {
                        _Temp_Cache_InnerOrders_Dic[primaryKey][1][data] = null;
                    }
                }
            }
        }

        /// <summary>
        /// 根据逐笔数据过滤
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool SetFilterDatas_Best_New(InnerOrderData data)
        {
            lock (_GroupBondsLock_Best)
            {
                if (this.SelectedGroupBondsItems_Best != null && this.SelectedGroupBondsItems_Best.Count > 0
                && (data.SecurityID == null || !this.SelectedGroupBondsItems_Best.Contains(data.SecurityID)))
                {
                    return false;
                }
            }

            if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
            {
                if (riskFilterList_best != null && riskFilterList_best.Count > 0)
                {
                    bool isDisplay = false;

                    if (riskFilterList_best.Contains("无风险") && string.IsNullOrEmpty(data.RiskInfo))
                    {
                        isDisplay = true;
                    }
                    else
                    {
                        if (data.RiskList != null)
                        {
                            foreach (string subRisk in data.RiskList)
                            {
                                if (riskFilterList_best.Any(e => !string.IsNullOrEmpty(subRisk) && subRisk.Contains(e)))
                                {
                                    isDisplay = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (isDisplay == false)
                    {
                        return false;
                    }
                }
            }

            if ((this.Best_DisplayMode == "BNC" && data.SecurityType != "BNC" && data.BondSubType != "PSB")
                || (string.IsNullOrEmpty(this.Best_DisplayMode) == false && this.Best_DisplayMode != "BNC" && data.SecurityType == "BNC" && data.BondSubType != "PSB")
                || (this.Best_DisplayMode == "BNC" && data.BondSubType == "PSB"))
                return false;

            if (string.IsNullOrEmpty(this.Best_SearchText) == false
                && (data.SecurityID == null || !data.SecurityID.Contains(this.Best_SearchText))
                && (data.Symbol == null || !data.Symbol.Contains(this.Best_SearchText))
                && (data.RoleName == null || !data.RoleName.Contains(this.Best_SearchText))
                && (data.PartySubName == null || !data.PartySubName.Contains(this.Best_SearchText))
                && (data.IssuerName == null || !data.IssuerName.Contains(this.Best_SearchText))
                && (data.WarranterName == null || !data.WarranterName.Contains(this.Best_SearchText))
                && (data.TraderName == null || !data.TraderName.Contains(this.Best_SearchText))
                && (data.InstitutionName == null || !data.InstitutionName.Contains(this.Best_SearchText)))
                return false;

            if (this._focusTagDic_Best != null && this._focusTagDic_Best.ContainsKey(Consts.SIDE))
            {
                List<string> sidefilter = this._focusTagDic_Best[Consts.SIDE];
                if (sidefilter.Count > 0)
                {
                    string otherside = data.Side == "1" ? "2" : "1";
                    if ((sidefilter.Contains("0001") && data.Side == "1")
                    || (sidefilter.Contains("0010") && data.Side == "2")
                    || (sidefilter.Contains("0011") && _Cache_InnerOrders_Dic[data.SecurityID].Values.Any(i => i.Side == otherside)))
                    {
                        // ignore
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (IsMe_Best == true && data.IsMe == false)
            {
                return false;
            }

            if (IsFuzzy_Best == true && data.SecurityID != "99999")
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

            if (this.IsCoupon_Best)
            {
                if (!string.IsNullOrEmpty(this.StartCoupon_Best) && (data.CouponRateCurrent == null || data.CouponRateCurrent < this.StartCoupon_Best.ConvertToDecimal()))
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(this.EndCoupon_Best) && (data.CouponRateCurrent == null || data.CouponRateCurrent > this.EndCoupon_Best.ConvertToDecimal()))
                {
                    return false;
                }
            }

            {
                if (!string.IsNullOrEmpty(this.StartPrice_Best) && !string.IsNullOrEmpty(this.EndPrice_Best))
                {
                    decimal startPrice = this.StartPrice_Best.ConvertToDecimal();
                    decimal endPrice = this.EndPrice_Best.ConvertToDecimal();
                    if (!(data.Price >= startPrice && data.Price <= endPrice))
                    {
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(this.StartPrice_Best) && string.IsNullOrEmpty(this.EndPrice_Best))
                {
                    decimal startPrice = this.StartPrice_Best.ConvertToDecimal();
                    if (!(data.Price >= startPrice))
                    {
                        return false;
                    }
                }
                else if (string.IsNullOrEmpty(this.StartPrice_Best) && !string.IsNullOrEmpty(this.EndPrice_Best))
                {
                    decimal endPrice = this.EndPrice_Best.ConvertToDecimal();
                    if (!(data.Price <= endPrice))
                    {
                        return false;
                    }
                }
            }

            {
                decimal? chinaBondValue = data.ChinaBondValuation;
                if (data.ChinaBondExercise.HasValue)
                {
                    chinaBondValue = data.ChinaBondExercise;
                }

                if (!string.IsNullOrEmpty(this.StartChinaBondValue_Best) && !string.IsNullOrEmpty(this.EndChinaBondValue_Best))
                {
                    decimal startChinaBondValue = this.StartChinaBondValue_Best.ConvertToDecimal();
                    decimal endChinaBondValue = this.EndChinaBondValue_Best.ConvertToDecimal();
                    if (!(chinaBondValue >= startChinaBondValue && chinaBondValue <= endChinaBondValue))
                    {
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(this.StartChinaBondValue_Best) && string.IsNullOrEmpty(this.EndChinaBondValue_Best))
                {
                    decimal startChinaBondValue = this.StartChinaBondValue_Best.ConvertToDecimal();
                    if (!(chinaBondValue >= startChinaBondValue))
                    {
                        return false;
                    }
                }
                else if (string.IsNullOrEmpty(this.StartChinaBondValue_Best) && !string.IsNullOrEmpty(this.EndChinaBondValue_Best))
                {
                    decimal endChinaBondValue = this.EndChinaBondValue_Best.ConvertToDecimal();
                    if (!(chinaBondValue <= endChinaBondValue))
                    {
                        return false;
                    }
                }
            }

            // bid-中债
            if (data.Side == "1" && data.Price.HasValue && data.ChinaBondValuation.HasValue)
            {
                decimal value = (data.Price.Value - data.ChinaBondValuation.Value) * 100;
                if (!string.IsNullOrEmpty(this.StartBidBp_Best) && !string.IsNullOrEmpty(this.EndBidBp_Best))
                {

                    decimal startBp = this.StartBidBp_Best.ConvertToDecimal();
                    decimal endBp = this.EndBidBp_Best.ConvertToDecimal();
                    if (!(value >= startBp && value <= endBp))
                    {
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(this.StartBidBp_Best) && string.IsNullOrEmpty(this.EndBidBp_Best))
                {
                    decimal startPrice = this.StartBidBp_Best.ConvertToDecimal();
                    if (!(value >= startPrice))
                    {
                        return false;
                    }
                }
                else if (string.IsNullOrEmpty(this.StartBidBp_Best) && !string.IsNullOrEmpty(this.EndBidBp_Best))
                {
                    decimal endPrice = this.EndBidBp_Best.ConvertToDecimal();
                    if (!(value <= endPrice))
                    {
                        return false;
                    }
                }
            }

            // 中债-ofr
            if (data.Side == "2" && data.Price.HasValue && data.ChinaBondValuation.HasValue)
            {
                decimal value = (data.ChinaBondValuation.Value - data.Price.Value) * 100;
                if (!string.IsNullOrEmpty(this.StartOfrBp_Best) && !string.IsNullOrEmpty(this.EndOfrBp_Best))
                {
                    decimal startPrice = this.StartOfrBp_Best.ConvertToDecimal();
                    decimal endPrice = this.EndOfrBp_Best.ConvertToDecimal();
                    if (!(value >= startPrice && value <= endPrice))
                    {
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(this.StartOfrBp_Best) && string.IsNullOrEmpty(this.EndOfrBp_Best))
                {
                    decimal startPrice = this.StartOfrBp_Best.ConvertToDecimal();
                    if (!(value >= startPrice))
                    {
                        return false;
                    }
                }
                else if (string.IsNullOrEmpty(this.StartOfrBp_Best) && !string.IsNullOrEmpty(this.EndOfrBp_Best))
                {
                    decimal endPrice = this.EndOfrBp_Best.ConvertToDecimal();
                    if (!(value <= endPrice))
                    {
                        return false;
                    }
                }
            }

            bool result = true;

            if (data.Sign != 0 && this._focusTagDic_Best != null)
            {
                foreach (KeyValuePair<string, string> pair in data.SignDic)
                {
                    CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic_Best);

                    if (!result)
                        return result;
                }
            }

            if (data.Sign1 != 0 && this._focusTagDic_Best != null)
            {
                foreach (KeyValuePair<string, string> pair in data.SignDic1)
                {
                    CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic_Best);

                    if (!result)
                        return result;
                }
            }

            if (data.Sign2 != 0 && this._focusTagDic_Best != null)
            {
                foreach (KeyValuePair<string, string> pair in data.SignDic2)
                {
                    CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic_Best);

                    if (!result)
                        return result;
                }
            }

            if (data.Sign3 != 0 && this._focusTagDic_Best != null)
            {
                foreach (KeyValuePair<string, string> pair in data.SignDic3)
                {
                    CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic_Best);

                    if (!result)
                        return result;
                }
            }
            else if (result && data.Sign3 != 0 && "BNC".Equals(data.SecurityType) && this._focusTagDic_Best != null)
            {
                foreach (KeyValuePair<string, string> pair in data.SignDic3)
                {
                    string key = pair.Key;
                    string value = pair.Value;
                    if (key == Utils.Constants.RANKING_TYPE)
                    {
                        result = result
                           && (!_focusTagDic_Best.ContainsKey(key) || _focusTagDic_Best[key].Count == 0) || _focusTagDic_Best[key].Contains(value)
                           || (_focusTagDic_Best[key].Contains("xx") && !rankingtype_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
                    }
                    else if (key == Utils.Constants.RATING_CURRENT)
                    {
                        result = result
                           && (!_focusTagDic_Best.ContainsKey(key) || _focusTagDic_Best[key].Count == 0) || _focusTagDic_Best[key].Contains(value)
                           || (_focusTagDic_Best[key].Contains("xx") && !ratingCurrent_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
                    }
                    else if (key == Utils.Constants.CDC_IMPLITED_RATING)
                    {
                        result = result
                           && (!_focusTagDic_Best.ContainsKey(key) || _focusTagDic_Best[key].Count == 0) || _focusTagDic_Best[key].Contains(value)
                           || (_focusTagDic_Best[key].Contains("xx") && !cdcImplitedRating_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
                    }
                    else if (key == Utils.Constants.ISSUER_RATING_CURRENT_CBR
                        || key == Utils.Constants.CSI_IMPLIED_RATING)
                    {
                        result = result
                           && (!_focusTagDic_Best.ContainsKey(key) || _focusTagDic_Best[key].Count == 0) || _focusTagDic_Best[key].Contains(value)
                           || (_focusTagDic_Best[key].Contains("01000") && value == "00000" && !"BNC".Equals(data.SecurityType)); // 00000也算其他
                    }
                }
            }

            if (data.Sign4 != 0 && this._focusTagDic_Best != null)
            {
                foreach (KeyValuePair<string, string> pair in data.SignDic4)
                {
                    CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic_Best);

                    if (!result)
                        return result;
                }
            }

            return result;

        }
        #endregion

        #region 券池（逐笔）
        private List<string> SelectedGroupBondsItems = new List<string>();
        private object _GroupBondsLock = new object();
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
                this.ViewSource.View?.Refresh();
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
                    this.ViewSource.View?.Refresh();
                    SetGroupBondDes();
                }

            }
        }

        #endregion

        #region 券池（最优）

        private List<string> SelectedGroupBondsItems_Best = new List<string>();
        private object _GroupBondsLock_Best = new object();
        private void SetGroupBondDes_Best()
        {
            StringBuilder sb = new StringBuilder();
            //if (!string.IsNullOrEmpty(this.FilterText))
            //{
            //    sb.Append(this.FilterText);
            //}
            if (this.SelectedGroupBondsItems_Best != null && this.SelectedGroupBondsItems_Best.Count > 0)
            {
                foreach (string securityId in SelectedGroupBondsItems_Best)
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
            SetDesSource(this.DesSource_Best, "券池", text);
        }

        /// <summary>
        /// 券池处理
        /// </summary>
        /// <param name="inf1"></param>
        /// <param name="focusGroupId"></param>
        internal void ProcessGroupBonds_Best(string inf1, string focusGroupId)
        {
            if (inf1 == "GROUP")
            {
                // 券池处理
                string[] groupidArr = focusGroupId.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                List<string> bonds = BondGroupHandler.Instance.GetSecuritys(new List<string>(groupidArr));
                lock (_GroupBondsLock_Best)
                {
                    SelectedGroupBondsItems_Best.Clear();
                    if (bonds != null)
                    {
                        SelectedGroupBondsItems_Best.AddRange(bonds);
                    }
                }
                FilterBestData();
                SetGroupBondDes_Best();
            }
            else
            {
                if (SelectedGroupBondsItems_Best == null || SelectedGroupBondsItems_Best.Count > 0)
                {
                    lock (_GroupBondsLock_Best)
                    {
                        SelectedGroupBondsItems_Best.Clear();
                    }
                    FilterBestData();
                    SetGroupBondDes_Best();
                }

            }
        }

        #endregion

        #region 风控过滤

        List<string> riskFilterList_best = new List<string>();
        internal void ProcessRiskFilterData_Best(string[] riskArr)
        {
            riskFilterList_best.Clear();

            if (riskArr != null)
            {
                riskFilterList_best.AddRange(riskArr);
            }

            //this.ViewSource_Best.View?.Refresh();
            this.FilterBestData();
        }

        List<string> riskFilterList = new List<string>();
        internal void ProcessRiskFilterData(string[] riskArr)
        {
            riskFilterList.Clear();

            if (riskArr != null)
            {
                riskFilterList.AddRange(riskArr);
            }

            this.ViewSource.View?.Refresh();
        }
        #endregion
    }
}
