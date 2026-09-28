using com.gateway.log;
using Microsoft.Win32;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.PP;
using STC.Common.Model.Admin;
using STC.Common.Model.PP;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.UI;
using STC.Frame.Utils;
using STC.PP.Plugin.Handler;
using STC.PP.Plugin.Model;
using STC.PP.Plugin.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static DotNetty.Codecs.Http.HttpContentEncoder;

namespace STC.PP.Plugin.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region 字段
        private int PageMaxCount = 200;
        public int PageShowMaxCount
        {
            get
            {
                return PageMaxCount;
            }
            set
            {
                PageMaxCount = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region 属性
        private bool isAllChecked;
        public bool IsAllChecked
        {
            get
            {
                return isAllChecked;
            }
            set
            {
                isAllChecked = value;
                this.OnPropertyChanged();
                this.OnCheckAll();
            }
        }

        private int totalCount = 0;
        /// <summary>
        /// 总记录数
        /// </summary>
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


        private int _rowCount;
        /// <summary>
        /// 当前筛选记录数
        /// </summary>
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

        private string emsPageVisible = "true";
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
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


        private MainView _mainView;
        public MainView mainView
        {
            get
            {
                return _mainView;
            }

            set
            {
                _mainView = value;
            }
        }

        private Common.View.Component.PageControl _emsPageControl;
        public Common.View.Component.PageControl EMSPageControl
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
        #endregion

        #region 集合

        #region 搜索过滤数据源

        private ObservableCollection<TreeNodeModel> _MultiTreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        /// <summary>
        /// 组合
        /// </summary>
        public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource
        {
            get { return _MultiTreeComboxItemsSource; }
            set
            {
                if (_MultiTreeComboxItemsSource != value)
                {
                    _MultiTreeComboxItemsSource = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Dictionary<string, object> _dictFundAccount =  new Dictionary<string, object>();

        /// <summary>
        /// 投资经理 
        /// </summary>
        public Dictionary<string, object> DictFundAccount
        {
            get { return _dictFundAccount; }
            set
            {
                _dictFundAccount = value;
                this.OnPropertyChanged();
            }
        }

        private string _SelectetFundAccountItem = null;
        /// <summary>
        /// 选中的投资经理
        /// </summary>
        public string SelectetFundAccountItem
        {
            get { return _SelectetFundAccountItem; }
            set
            {
                if (_SelectetFundAccountItem != value)
                {
                    _SelectetFundAccountItem = value;
                    this.OnPropertyChanged();
                    NotifyFilter();
                }
            }
        }


        private string _SelectedTradeAccountCode = null;
        /// <summary>
        /// 选中的交易账户代码
        /// </summary>
        public string SelectedTradeAccountCode
        {
            get { return _SelectedTradeAccountCode; }
            set
            {
                if (_SelectedTradeAccountCode != value)
                {
                    _SelectedTradeAccountCode = value;
                    this.OnPropertyChanged();
                    NotifyFilter();
                }
            }
        }


        private string _SelectedTradeAccountName = null;
        /// <summary>
        /// 选中的交易账户名称
        /// </summary>
        public string SelectedTradeAccountName
        {
            get { return _SelectedTradeAccountName; }
            set
            {
                if (_SelectedTradeAccountName != value)
                {
                    _SelectedTradeAccountName = value;
                    this.OnPropertyChanged();
                }
            }

        }

        private string _SelectedItem = null;
        /// <summary>
        /// 选中 id，以,隔开
        /// </summary>
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (_SelectedItem != value)
                {
                    _SelectedItem = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _SelectedProductId = null;
        /// <summary>
        /// 选中恒生编号，以,隔开
        /// </summary>
        public string SelectedProductId
        {
            get { return _SelectedProductId; }
            set
            {
                if (_SelectedProductId != value)
                {
                    _SelectedProductId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private List<string> _SelectedGroupItem = new List<string>();
        /// <summary>
        /// 选中的组合代码
        /// </summary>
        public List<string> SelectedGroupItem
        {
            get { return _SelectedGroupItem; }
            set
            {
                if (_SelectedGroupItem != value)
                {
                    _SelectedGroupItem = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private List<string> _SelectedGroupText = new List<string>();
        /// <summary>
        /// 选中的组合
        /// </summary>
        public List<string> SelectedGroupText
        {
            get { return _SelectedGroupText; }
            set
            {
                if (_SelectedGroupText != value)
                {
                    _SelectedGroupText = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public Dictionary<string, object> _selectedGroupDict = new Dictionary<string, object>();
        /// <summary>
        /// 选中的组合字典
        /// </summary>
        public Dictionary<string, object> SelectedGroupDict
        {
            get { return _selectedGroupDict; }
            set
            {
                if (_selectedGroupDict != value)
                {
                    _selectedGroupDict = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _FilterText;
        /// <summary>
        /// 搜索框文本
        /// </summary>
        public string FilterText
        {
            get { return _FilterText; }
            set
            {
                if (_FilterText != value)
                {
                    _FilterText = value;
                    this.OnPropertyChanged();
                    NotifyFilter();
                }
            }
        }

        /// <summary>
        /// 选择的组合信息
        /// </summary>
        public string TopFilterSelectItem
        {
            get 
            {
                //StcPaTopSelectData stcpt = new StcPaTopSelectData(){ BookItem = SelectedTradeAccountCode, BookItemText = string.Join(",", SelectedGroupDict.Keys.ToList()), FundAccountItem = SelectetFundAccountItem, SymbolType = SelectSymbolType };
                StcPaTopSelectData stcpt = new StcPaTopSelectData() { BookItem = SelectedTradeAccountCode, BookItemText = SelectedTradeAccountName, FundAccountItem = SelectetFundAccountItem, SymbolType = SelectSymbolType };
                stcpt.StartTermDYItem = StartTermDY;
                stcpt.EndTermDYItem = EndTermDY;
                stcpt.IsAppTermItem = IsAppTerm;
                stcpt.StartCouponItem = StartCoupon;
                stcpt.EndCouponItem = EndCoupon;
                stcpt.IsCouponItem = IsCoupon;
                stcpt.StartPriceItem = StartPrice;
                stcpt.EndPriceItem = EndPrice;
                stcpt.StartChinaBondItem = StartChinaBondValue;
                stcpt.EndChinaBondItem= EndChinaBondValue;
                stcpt.StartExCloseItem = StartExCloseValue;
                stcpt.EndExCloseItem = EndExCloseValue;
                stcpt.StartBidBpItem = StartBidBp;
                stcpt.EndBidBpItem = EndBidBp;
                stcpt.StartOfrBpItem = StartOfrBp;
                stcpt.EndOfrBpItem = EndOfrBp;
                return CommonUtils.SerializeObject(stcpt);
            }
        }


        #endregion

        #region  订阅组
        private ObservableCollection<FilterGroupItem> _ppgroupList = new ObservableCollection<FilterGroupItem>();
        public ObservableCollection<FilterGroupItem> PPGroupList
        {
            get { return _ppgroupList; }
            set { _ppgroupList = value; }
        }

        private FilterGroupItem _selectedItem_tag;
        /// <summary>
        /// 订阅组选中项
        /// </summary>
        public FilterGroupItem SelectedItem_Tag
        {
            get { return _selectedItem_tag; }
            set
            {
                _selectedItem_tag = value;

                if (value != null)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        this.FocusTags = _selectedItem_tag.FocusTags;

                        if (!string.IsNullOrWhiteSpace(_selectedItem_tag.Remark))
                        {
                            //todo...
                            if (JsonSplit.IsJson(_selectedItem_tag.Remark))
                            {
                                StcPaTopSelectData pd = CommonUtils.DeserializeObject<StcPaTopSelectData>(_selectedItem_tag.Remark);
                                mainView.SetGroupAndTradeAccount(pd.BookItem);

                                if (!string.IsNullOrEmpty(pd.StartExCloseItem) && !string.IsNullOrEmpty(pd.EndExCloseItem))
                                {
                                    StartExCloseValue = pd.StartExCloseItem;
                                    EndExCloseValue = pd.EndExCloseItem;
                                }

                                mainView.SetSingleRd(pd.SymbolType ?? "0");

                                if (string.IsNullOrEmpty(pd.FundAccountItem))
                                {
                                    mainView.ClearFundAccountCB();
                                }

                                SelectetFundAccountItem = pd.FundAccountItem;

                                StartTermDY = pd.StartTermDYItem;
                                EndTermDY = pd.EndTermDYItem;
                                if (!string.IsNullOrEmpty(pd.StartTermDYItem) || !string.IsNullOrEmpty(pd.EndTermDYItem))
                                {
                                    IsAppTerm = pd.IsAppTermItem = true;
                                }
                                else
                                {
                                    IsAppTerm = pd.IsAppTermItem = false;
                                }

                                StartCoupon = pd.StartCouponItem;
                                EndCoupon = pd.EndCouponItem;
                                IsCoupon = pd.IsCouponItem;
                                StartPrice = pd.StartPriceItem;
                                EndPrice = pd.EndPriceItem;
                                StartChinaBondValue = pd.StartChinaBondItem;
                                EndChinaBondValue = pd.EndChinaBondItem;
                                StartExCloseValue = pd.StartExCloseItem;
                                EndExCloseValue = pd.EndExCloseItem;
                                StartBidBp = pd.StartBidBpItem;
                                EndBidBp = pd.EndBidBpItem;
                                StartOfrBp = pd.StartOfrBpItem;
                                EndOfrBp = pd.EndOfrBpItem;

                                FilterText = string.Empty;

                                //获取当前查询请求参数,用于判断是否需要重新查询，是否改变了请求参数
                                QueryBondPositionReq req = GetQueryBondPositionReq();
                                req.pageNum = 0;
                                req.pageSize = 0;
                                string selecttreq = CommonUtils.SerializeObject(req);
                                FavoritesTagReq = selecttreq;
                            }
                            else
                            {
                                FavoritesTagReq = null;
                                mainView.ResetMyFilterSelection();
                                mainView.ClearGroupAndTradeAccount();
                                mainView.ClearFundAccountCB();
                                StartExCloseValue = string.Empty;
                                EndExCloseValue = string.Empty;
                                mainView.SetSingleRd("0");
                                FilterText = string.Empty;
                                ClearCustomFilterPanel();
                            }
                        }
                        else
                        {
                            FavoritesTagReq = null;
                            mainView.ResetMyFilterSelection();
                            mainView.ClearGroupAndTradeAccount();
                            mainView.ClearFundAccountCB();
                            StartExCloseValue = string.Empty;
                            EndExCloseValue = string.Empty;
                            mainView.SetSingleRd("0");
                            FilterText = string.Empty;
                            ClearCustomFilterPanel();
                        }
                    }));
                }
                else
                {
                    FavoritesTagReq = null;
                    mainView.ResetMyFilterSelection();
                    LogHelper.Info($"Querying with: FilterPanel UnChecked!");
                    ResetFilterPanel();
                }

                this.OnPropertyChanged();
            }
        }

        public void ClearCustomFilterPanel()
        {
            StartTermDY = string.Empty;
            EndTermDY = string.Empty;
            IsAppTerm = false;
            StartCoupon = string.Empty;
            EndCoupon = string.Empty;
            IsCoupon = false;
            StartPrice = string.Empty;
            EndPrice = string.Empty;
            StartChinaBondValue = string.Empty;
            EndChinaBondValue = string.Empty;
            StartExCloseValue = string.Empty;
            EndExCloseValue = string.Empty;
            StartBidBp = string.Empty;
            EndBidBp = string.Empty;
            StartOfrBp = string.Empty;
            EndOfrBp = string.Empty;
        }


        public void ResetFilterPanel()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                mainView.customFilterPanel.ShowAll();
                FilterText = string.Empty;
                mainView.ClearGroupAndTradeAccount();
                mainView.ClearFundAccountCB();
                mainView.SetSingleRd("0");
                FilterText = string.Empty;
                DesSource.Clear();
                ClearCustomFilterPanel();
                EMSIndex = 0;
                NotifyFilter();
            }));
        }

        #endregion

        #region 持仓和质押券持仓数据源

        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }


        private ObservableCollection<StcBondPositionData> _dsBondPosition = new ObservableCollection<StcBondPositionData>();

        /// <summary>
        /// 债券持仓
        /// </summary>
        public ObservableCollection<StcBondPositionData> DsBondPosition
        {
            get
            {
                if (_dsBondPosition == null)
                    _dsBondPosition = new ObservableCollection<StcBondPositionData>();

                return _dsBondPosition;
            }

            set
            {
                _dsBondPosition = value;
                this.OnPropertyChanged(nameof(DsBondPosition));
            }
        }



        private ObservableCollection<StcBondPositionData> _dsfullbondposition = new ObservableCollection<StcBondPositionData>();

        /// <summary>
        /// 完整的数据持仓数据
        /// </summary>
        public ObservableCollection<StcBondPositionData> DsFullBondPosition
        {
            get
            {
                if (_dsfullbondposition == null)
                    _dsfullbondposition = new ObservableCollection<StcBondPositionData>();

                return _dsfullbondposition;
            }

            set
            {
                _dsfullbondposition = value;
                this.OnPropertyChanged(nameof(DsFullBondPosition));
            }
        }

        private StcBondPositionData _SelectOrder;
        public StcBondPositionData SelectOrder
        {
            get { return _SelectOrder; }
            set
            {
                _SelectOrder = value;
                OnPropertyChanged("SelectOrder");
            }
        }


        private CollectionViewSource dtmv = new CollectionViewSource();
        public CollectionViewSource DtMv
        {
            get { return dtmv; }
            set
            {
                dtmv = value;
                this.OnPropertyChanged(nameof(DtMv));
            }
        }


        private ObservableCollection<StcPledgePositionModelData> _dsPledgePosition = new ObservableCollection<StcPledgePositionModelData>();

        /// <summary>
        /// 质押券持仓
        /// </summary>
        public ObservableCollection<StcPledgePositionModelData> DsPledgePosition
        {
            get
            {
                if (_dsPledgePosition == null)
                    _dsPledgePosition = new ObservableCollection<StcPledgePositionModelData>();

                return _dsPledgePosition;
            }

            set
            {
                _dsPledgePosition = value;
                this.OnPropertyChanged(nameof(DsPledgePosition));
            }
        }


        #endregion

        #region 债券过滤面板操作
        private bool _IsAppTerm;

        /// <summary>
        /// 是否是自定义期限，已经选择了自定义期限后，其他的期限选择会被清空，该定义已经废弃，为了兼容，故保留
        /// </summary>
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
                
                NotifyFilter();
            }
        }

        private bool _IsShowFilterPanel = true;
        /// <summary>
        /// 是否显示过滤面板
        /// </summary>
        public bool IsShowFilterPanel
        {
            get { return _IsShowFilterPanel; }
            set
            {
                _IsShowFilterPanel = value;
                IniFileHelper.WriteValue($"PPMainView_{LocalEnv.UserInfo.UserName}", "IsShowFilterPanel", IsShowFilterPanel ? "1" : "0");
                //IniFileHelper.RefreshIni();
                this.OnPropertyChanged();
            }
        }
        

        private string _startTermDY = string.Empty;
        /// <summary>
        /// 期限 开始
        /// </summary>
        public string StartTermDY
        {
            get { return _startTermDY; }
            set
            {
                if (_startTermDY != value)
                {
                    _startTermDY = value;
                    this.OnPropertyChanged();

                    //会导致重复添加自定义期限
                    //if (_focusTagDic == null || (_focusTagDic.ContainsKey(Consts.TERM) && _focusTagDic[Consts.TERM].Count == 0))
                    //{
                    string text = string.Empty;
                    if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                    {
                        text = $"剩余期限:{StartTermDY}-{EndTermDY}";
                    }

                    SetDesSource(this.DesSource, "CUSTTERM", text);
                    //}

                    NotifyFilter();
                }
            }
        }

        private string _endTermDY = string.Empty;
        /// <summary>
        /// 期限 结束
        /// </summary>
        public string EndTermDY
        {
            get { return _endTermDY; }
            set
            {
                if (_endTermDY != value)
                {
                    _endTermDY = value;
                    this.OnPropertyChanged();

                    //会导致重复添加自定义期限
                    //if (_focusTagDic == null || (_focusTagDic.ContainsKey(Consts.TERM) && _focusTagDic[Consts.TERM].Count == 0))
                    //{
                    string text = string.Empty;
                    if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                    {
                        text = $"剩余期限:{StartTermDY}-{EndTermDY}";
                    }

                    SetDesSource(this.DesSource, "CUSTTERM", text);
                    //}


                    NotifyFilter();
                }

            }
        }


        private string _focusTags;
        /// <summary>
        /// 选择的关注标签
        /// </summary>
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
                            _focusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);
                        }
                        else
                        {
                            _focusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(string.Empty);
                        }
                    }
                    catch (Exception)
                    {
                        _focusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(string.Empty);
                    }

                    this.OnPropertyChanged();
                    
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
                            if (filterValue != "xx")
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

                        //会导致重复添加自定义期限
                        //if (pair.Key == Consts.TERM && text.Length == 0)
                        //{
                        //    if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
                        //    {
                        //        text.Append($"剩余期限:{StartTermDY}-{EndTermDY}");
                        //    }
                        //}

                        SetDesSource(this.DesSource, pair.Key, text.ToString());
                    }
                }
                else
                {
                    foreach (string key in Common.Handler.Base.BaseHandler.Instance.GetQuoteTagDic().Keys)
                    {
                        SetDesSource(this.DesSource, key, string.Empty);
                    }
                }
            }
        }

        private Dictionary<string, List<string>> _focusTagDic = new Dictionary<string, List<string>>();

        private bool _isMe;
        /// <summary>
        /// IsMe 不用
        /// </summary>
        public bool IsMe
        {
            get { return _isMe; }
            set
            {
                _isMe = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = value ? "我" : string.Empty;
                SetDesSource(this.DesSource, "我", text);
            }
        }

        private string _startCoupon;
        //票面 开始
        public string StartCoupon
        {
            get { return _startCoupon; }
            set
            {
                _startCoupon = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartCoupon) || !string.IsNullOrEmpty(EndCoupon))
                {
                    text = $"票面:{StartCoupon}-{EndCoupon}";
                }
                SetDesSource(this.DesSource, "Coupon", text);
            }
        }

        private string _endCoupon;
        //票面 结束
        public string EndCoupon
        {
            get { return _endCoupon; }
            set
            {
                _endCoupon = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartCoupon) || !string.IsNullOrEmpty(EndCoupon))
                {
                    text = $"票面:{StartCoupon}-{EndCoupon}";
                }
                SetDesSource(this.DesSource, "Coupon", text);

            }
        }

        private bool _isCoupon;
        //不用选择票面
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
                
                NotifyFilter();
            }
        }

        private string _startPrice;
        //报价（收益率） 开始
        public string StartPrice
        {
            get { return _startPrice; }
            set
            {
                _startPrice = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartPrice) || !string.IsNullOrEmpty(EndPrice))
                {
                    text = $"报价:{StartPrice}-{EndPrice}";
                }
                SetDesSource(this.DesSource, "Price", text);
            }
        }

        private string _endPrice;
        //报价（收益率） 结束
        public string EndPrice
        {
            get { return _endPrice; }
            set
            {
                _endPrice = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartPrice) || !string.IsNullOrEmpty(EndPrice))
                {
                    text = $"报价:{StartPrice}-{EndPrice}";
                }
                SetDesSource(this.DesSource, "Price", text);
            }
        }

        private string _startChinaBondValue;
        /// <summary>
        /// 中债估值 开始
        /// </summary>
        public string StartChinaBondValue
        {
            get { return _startChinaBondValue; }
            set
            {
                _startChinaBondValue = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartChinaBondValue) || !string.IsNullOrEmpty(EndChinaBondValue))
                {
                    text = $"中债估值:{StartChinaBondValue}-{EndChinaBondValue}";
                }
                SetDesSource(this.DesSource, "ChinaBondValue", text);
            }
        }

        private string _endChinaBondValue;
        /// <summary>
        /// 中债估值 结束
        /// </summary>
        public string EndChinaBondValue
        {
            get { return _endChinaBondValue; }
            set
            {
                _endChinaBondValue = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartChinaBondValue) || !string.IsNullOrEmpty(EndChinaBondValue))
                {
                    text = $"中债估值:{StartChinaBondValue}-{EndChinaBondValue}";
                }
                SetDesSource(this.DesSource, "ChinaBondValue", text);
            }
        }


        private string _startExCloseValue;
        /// <summary>
        /// 交易所收盘价-估值开始
        /// </summary>
        public string StartExCloseValue
        {
            get { return _startExCloseValue; }
            set
            {
                _startExCloseValue = value;
                this.OnPropertyChanged();

                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartExCloseValue) || !string.IsNullOrEmpty(EndExCloseValue))
                {
                    text = $"交易所收盘价-估值:{StartExCloseValue}-{EndExCloseValue}";
                }
                SetDesSource(this.DesSource, "ExCloseValue", text);
            }
        }

        private string _endExCloseValue;
        /// <summary>
        /// 交易所收盘价-估值 结束
        /// </summary>
        public string EndExCloseValue
        {
            get { return _endExCloseValue; }
            set
            {
                _endExCloseValue = value;
                this.OnPropertyChanged();

                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartExCloseValue) || !string.IsNullOrEmpty(EndExCloseValue))
                {
                    text = $"交易所收盘价-估值:{StartExCloseValue}-{EndExCloseValue}";
                }
                SetDesSource(this.DesSource, "ExCloseValue", text);
            }
        }


        private string _StartBidBp;
        /// <summary>
        /// bid-中债 开始
        /// </summary>
        public string StartBidBp
        {
            get { return _StartBidBp; }
            set
            {
                _StartBidBp = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartBidBp) || !string.IsNullOrEmpty(EndBidBp))
                {
                    text = $"bid-中债:{StartBidBp}-{EndBidBp}";
                }
                SetDesSource(this.DesSource, "BidBp", text);
            }
        }

        private string _selectsymboltype = "0";
        /// <summary>
        /// 债券类型 BCO,BNC,ABS,NCD  转换关系：1-信用债 BCO 2-利率债 BNC 11-ABS 12-NCD   BizTypeConvert
        /// </summary>
        public string SelectSymbolType
        {
            get { return _selectsymboltype; }
            set
            {
                _selectsymboltype = value;
                this.OnPropertyChanged();
                SymbolType = value == "0" ? "" : value;

                NotifyFilter();

                string text = "全";
                if (value == "0")
                    text = "";
                else if (value == "1")
                    text = "信用";
                else if (value == "2")
                    text = "利率";
                
                SetDesSource(this.DesSource, "SymbolType", text);
            }
        }

        private string _symboltype = "";
        public string SymbolType
        {
            get { return _symboltype; }
            set
            {
                _symboltype = value;
                this.OnPropertyChanged();
            }
        }

        private string _EndBidBp;
        /// <summary>
        /// bid-中债 结束
        /// </summary>
        public string EndBidBp
        {
            get { return _EndBidBp; }
            set
            {
                _EndBidBp = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartBidBp) || !string.IsNullOrEmpty(EndBidBp))
                {
                    text = $"bid-中债:{StartBidBp}-{EndBidBp}";
                }
                SetDesSource(this.DesSource, "BidBp", text);

            }
        }

        private string _StartOfrBp;
        /// <summary>
        /// 中债-ofr 开始
        /// </summary>
        public string StartOfrBp
        {
            get { return _StartOfrBp; }
            set
            {
                _StartOfrBp = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartOfrBp) || !string.IsNullOrEmpty(EndOfrBp))
                {
                    text = $"中债-ofr:{StartOfrBp}-{EndOfrBp}";
                }
                SetDesSource(this.DesSource, "OfrBp", text);
            }
        }

        private string _EndOfrBp;
        /// <summary>
        /// 中债-ofr 结束
        /// </summary>
        public string EndOfrBp
        {
            get { return _EndOfrBp; }
            set
            {
                _EndOfrBp = value;
                this.OnPropertyChanged();
                
                NotifyFilter();

                string text = string.Empty;
                if (!string.IsNullOrEmpty(StartOfrBp) || !string.IsNullOrEmpty(EndOfrBp))
                {
                    text = $"中债-ofr:{StartOfrBp}-{EndOfrBp}";
                }
                SetDesSource(this.DesSource, "OfrBp", text);
            }
        }

        
        private ObservableCollection<DescriptionItem> _desSource = new ObservableCollection<DescriptionItem>();
        /// <summary>
        /// 选择的过滤条件描述
        /// </summary>
        public ObservableCollection<DescriptionItem> DesSource
        {
            get { return _desSource; }
            set { _desSource = value; this.OnPropertyChanged(); }
        }
        #endregion

        /// <summary>
        /// 是否投资经理
        /// </summary>
        private bool _IsFundAccount = false;
        public bool IsFundAccount
        {
            get { return _IsFundAccount; }
            set
            {
                if (_IsFundAccount != value)
                {
                    _IsFundAccount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Command
        /// <summary>
        /// Command集合
        /// </summary>
        public ICommand RefreshCommand { get { return new ComRelayCommand(OnRefreshCommand); } }
        public ICommand ExportCommand { get { return new ComRelayCommand(OnExportCommand); } }
        public ICommand CopyCodeCommand { get { return new ComRelayCommand(OnCopyCodeCommand, CanExecuteStockCommand); } }
        public ICommand CopyPositionCommand { get { return new ComRelayCommand(OnCopyPositionCommand, CanExecuteStockCommand); } }
        public ICommand SetCopyPositionFieldCommand { get { return new ComRelayCommand(OnSetCopyPositionFieldCommand); } }
        public ICommand SalelAllCommand { get { return new ComRelayCommand(OnSalelAllCommand, CanExecuteStockCommand); } }
        public ICommand PartiallySoldCommand { get { return new ComRelayCommand(OnPartiallySoldCommand, CanExecuteStockCommand); } }
        public ICommand SaveSearchGroupCommand { get { return new ComRelayCommand(OnSaveSearchGroupCommand); } }
        public ICommand CopyCodeAndNameCommand { get { return new ComRelayCommand(OnCopyCodeAndNameCommand, CanExecuteStockCommand); } }

        #endregion

        #region 构造函数与初始化
        public MainViewModel()
        {

        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        public void InitView()
        {
            PPGroupList = IntentionOldHandler.Instance.PPGroupList;
            //获取模拟字典数据  GetTAllBookCollection
            //GenerateSimlatedDict();
            //405701 仅自己   405702 全部
            IsFundAccount = Common.Handler.Admin.AdminHandler.Instance.CheckPermission("4057", "405701");
            LogHelper.Info("IsFundAccount:" + IsFundAccount);
            MultiTreeComboxItemsSource = !IsFundAccount ? STC.Common.Handler.PP.PPHandler.Instance.GetTradeTBookGroup() : STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();

            if (IsFundAccount && MultiTreeComboxItemsSource.Count == 0)
            {
                LogHelper.Error("警告：该绩效经理没有关联任何交易账户!");
            }

            //读取配置文件，是否显示过滤面板
            string isshowfilterpanel = IniFileHelper.ReadValue($"PPMainView_{LocalEnv.UserInfo.UserName}", "IsShowFilterPanel");
            IsShowFilterPanel = string.IsNullOrEmpty(isshowfilterpanel) || isshowfilterpanel == "1";

            //初始化交易账户
            //InitTradeAccount();
            //初始化投资经理
            InitFundAccount(IsFundAccount);

            this.Mv.Source = DsBondPosition;
            this.Mv.View.CollectionChanged += View_CollectionChanged;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            //this.Mv.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));

            this.DtMv.Source = DsPledgePosition;
            this.DtMv.IsLiveFilteringRequested = true;
            this.DtMv.IsLiveSortingRequested = true;

            //SetMvFilter();

            NotifyFilter();
        }

        
        private void View_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                List<StcBondPositionData> list = this.Mv.View.Cast<StcBondPositionData>().ToList();
                RowCount = list.Count();
            }));
        }

       
        ///// <summary>
        ///// 初始化交易账户
        ///// </summary>
        //public async void InitTradeAccount()
        //{
        //    await Task.Run(async () =>
        //    {
        //       await PAPositionHandler.Instance.QueryTradingAccount();    
        //    });

        //    DictTradeAccount = PAPositionHandler.Instance.DictTradeAccount;
        //}

        /// <summary>
        /// 初始化投资经理
        /// </summary>
        public async void InitFundAccount(bool isfundaccount)
        {
            await Task.Run(async () =>
            {
				DictFundAccount = await PPHandler.Instance.QueryFundAccount(isfundaccount);
                //await PAPositionHandler.Instance.QueryFundAccount(isfundaccount);
            });

            //DictFundAccount = PAPositionHandler.Instance.DictFundAccount;
        }

        #endregion

        #region 设置过滤条件
        private void SetMvFilter()
        {
            this.Mv.View.Filter = x =>
            {
                StcBondPositionData item = x as StcBondPositionData;

                //组合代码
                if (SelectedGroupItem.Count > 0)
                {
                    if (!SelectedGroupItem.Contains(item.PortfolioNo))
                        return false;
                }

                //投资经理
                if (!string.IsNullOrWhiteSpace(SelectetFundAccountItem))
                {
                    if (string.IsNullOrWhiteSpace(item.FundAccount))
                        return false;
                    
                    if (!SelectetFundAccountItem.Split(',').Intersect(item.FundAccount?.Split(',')).Any())
                        return false;
                }

                //交易账户
                if (!string.IsNullOrWhiteSpace(SelectedTradeAccountName))
                {
                    if (!SelectedTradeAccountName.Contains(item.TradeName))
                        return false;
                }

                //模糊查询
                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    if (string.IsNullOrEmpty(this.FilterText) == false
                        && (item.SecurityId == null || !item.SecurityId.Contains(this.FilterText))
                        && (item.SymbolName == null || !item.SymbolName.Contains(this.FilterText))
                        && (item.FundAccount == null || !item.FundAccount.Contains(this.FilterText))
                        && (item.TradeName == null || !item.TradeName.Contains(this.FilterText))
                        && (item.Issuer == null || !item.Issuer.Contains(this.FilterText))
                        && (item.TradeAccount == null || !item.TradeAccount.Contains(this.FilterText)))
                        return false;
                }

                // 自动选中过滤后的第一项
                if (SelectOrder == null)
                {
                    SelectOrder = item;
                }

                return true;
            };
        }

        #endregion

        #region 查询债券持仓

        /// <summary>
        /// 设置UI
        /// </summary>
        /// <param name="isfirst"></param>
        public void InitFirstQuery(bool isfirst = false)
        {
            IsLoading = true;
            IsAllChecked = false;
            EMSIndex = 0;
            EMSPageControl.Page = 1;
            DsBondPosition.Clear();
            DsPledgePosition.Clear();
            if (this.Mv.View != null)
                this.Mv.View.Refresh();

            if (this.DtMv.View != null)
                this.DtMv.View.Refresh();
        }

       
        private void UpdateQueryBondPositionData(List<StcBondPositionModel> reply)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (StcBondPositionModel obj in reply)
                {
                    UpdateBondPositioRecord(obj, DsBondPosition);
                }

                this.Mv.View.Refresh();
            }));
        }

        /// <summary>
        /// 更新界面
        /// </summary>
        /// <param name="er"></param>
        /// <param name="tardata">DsBondPosition</param>
        private void UpdateBondPositioRecord(StcBondPositionModel er, ObservableCollection<StcBondPositionData> targetDb)
        {
            string key = er.securityId + er.portfolioNo + er.tradeAccount;
            StcBondPositionData data = targetDb.FirstOrDefault(x => x.SecurityId == er.securityId && x.PortfolioNo == er.portfolioNo && x.TradeAccount == er.tradeAccount);
            if (data != null)
            {
                data.PortfolioNo = er.portfolioNo;
                data.PortfolioName = er.portfolioName;
                data.TradeAccount = er.tradeAccount;
                data.TradeName = er.tradeName;
                data.FundAccount = er.fundAccount;
                data.Issuer = er.issuer;
                data.SecurityId = er.securityId;
                data.SymbolName = er.symbolName;
                data.AvailablePosition = er.availablePosition;
                data.CsiBondValuation = er.csiBondValuation;
                data.ExClose = er.exClose ?? "";
                data.ExClose2 = er.exClose2 ?? "";
                data.Netprice = er.netprice;
                data.ValuationYield = er.valuationYield;
                data.ValuationYield2 = er.valuationYield2;
                data.StcAvailablePosition = er.stcAvailablePosition;
                data.StcInBuyVolume = er.stcInBuyVolume;
                data.StcInSalesVolume = er.stcInSalesVolume;
                data.Quotalimit = er.quotalimit;
                data.OutStandingAmount = er.outstandingAmount;
                data.RemaingingTime = er.remaingingTime;
                data.Investmentratio = ConvertInvestmentRatio(er.investmentratio);
                data.Bonddrating = er.bonddrating;
                data.Rate = er.rate;
                data.TotalQty = er.totalQty;
                data.StcAvailableLimit = er.stcAvailableLimit;
                data.RateEnddate = er.rateenddate ?? "";
                data.HsProductId = er.hsProductId ?? "";
                data.RatingExpirydate = er.ratingExpirydate ?? "";
                data.SingleBondAmount = er.singleBondAmount;
				data.IssueType = er.issueType ?? "";                
				data.Guarantor = er.guarantor ?? "";
                data.GuarantorRating = er.guarantorRating ?? "";
            }
            else
            {
                StcBondPositionData newdata = new StcBondPositionData();
                newdata.Key = key;
                newdata.PortfolioNo = er.portfolioNo;
                newdata.PortfolioName = er.portfolioName;
                newdata.TradeAccount = er.tradeAccount;
                newdata.TradeName = er.tradeName;
                newdata.FundAccount = er.fundAccount;
                newdata.Issuer = er.issuer;
                newdata.SecurityId = er.securityId;
                newdata.SymbolName = er.symbolName;
                newdata.AvailablePosition = er.availablePosition;
                newdata.CsiBondValuation = er.csiBondValuation;
                newdata.ExClose = er.exClose ?? "";
                newdata.ExClose2 = er.exClose2 ?? "";
                newdata.ValuationYield = er.valuationYield;
                newdata.ValuationYield2 = er.valuationYield2;
                newdata.Netprice = er.netprice;
                newdata.StcAvailablePosition = er.stcAvailablePosition;
                newdata.StcInBuyVolume = er.stcInBuyVolume;
                newdata.StcInSalesVolume = er.stcInSalesVolume;
                newdata.Quotalimit = er.quotalimit;
                newdata.OutStandingAmount = er.outstandingAmount;
                newdata.RemaingingTime = er.remaingingTime;
                newdata.Investmentratio = ConvertInvestmentRatio(er.investmentratio);
                newdata.Bonddrating = er.bonddrating;
                newdata.Rate = er.rate;
                newdata.TotalQty = er.totalQty;
                newdata.StcAvailableLimit = er.stcAvailableLimit;
                newdata.RateEnddate = er.rateenddate ?? "";
                newdata.HsProductId = er.hsProductId ?? "";
                newdata.RatingExpirydate = er.ratingExpirydate ?? "";
                newdata.SingleBondAmount = er.singleBondAmount;
				newdata.IssueType = er.issueType ?? "";
                newdata.Guarantor = er.guarantor ?? "";
                newdata.GuarantorRating = er.guarantorRating ?? "";
                targetDb.Add(newdata);
            }
        }

        public  string ConvertInvestmentRatio(string investmentRatio)
        {
            decimal value = 0M;
            if (decimal.TryParse(investmentRatio, out value))
            {
                return value.ToString("0.00"); // 保留2位小数
            }
            return string.Empty; // 转换失败返回空
        }
        #endregion

        #region 查询质押券持仓

        /// <summary>
        /// 查询债券持仓
        /// </summary>
        public void QueryPledgeposition(StcBondPositionData da)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                DsPledgePosition.Clear();
                DtMv.View.Refresh();
                if (da != null)
                {
                    QueryPledgePositionReq request = new QueryPledgePositionReq();
                    request.portfolioNo = da.PortfolioNo; // 组合代码
                    request.tradeAccount = da.TradeAccount;// 交易账户代码
                    request.securityId = da.SecurityId;// 债券代码
                    await RequestQueryPledgeposition(request, da);
                }
            }));
        }

        /// <summary>
        /// 查询质押券持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task RequestQueryPledgeposition(QueryPledgePositionReq req, StcBondPositionData da)
        {
            QueryPledgePositionResult reply = await PAPositionHandler.Instance.GetQueryPledgeposition(req);
            if (reply != null)
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    LogHelper.Info($"RequestQueryPledgeposition Result: Return {reply.Datas.Count} Records; TotalRecord {reply.totalSize} Records.");
                    UpdatePledgepositionData(reply.Datas, da);
                }
                else
                {
                    if (reply.Datas == null)
                    {
                        ////如果没有数据，则生成模拟数据
                        //List<StcPledgePositionModel> simdata = GenerateSimulatedPledgePositionData();
                        //UpdatePledgepositionData(simdata);
                    }
                }

            }
            
        }

        /// <summary>
        /// 更新质押券持仓
        /// </summary>
        /// <param name="er"></param>
        private void UpdatePledgepositionData(List<StcPledgePositionModel> reply, StcBondPositionData da)
        {
            foreach (StcPledgePositionModel obj in reply)
            {
                StcPledgePositionModelData newdata = new StcPledgePositionModelData();
                //补全数据
                newdata.SecurityId = da.SecurityId;
                newdata.SymbolName = da.SymbolName;
                newdata.PortfolioName = da.PortfolioName;
                //更新数据
                newdata.PortfolioNo = obj.portfolioNo ?? da.PortfolioNo;
                newdata.TradeAccount = obj.tradeAccount ?? da.TradeAccount;
                newdata.TradeName = obj.tradeName ?? da.TradeName;
                newdata.TradeType = obj.tradeType;
                newdata.Side = obj.side;
                newdata.Qty = obj.qty;
                newdata.MaturityDate = obj.maturityDate;
                newdata.CounterParty = obj.counterparty;
                this.DsPledgePosition.Add(newdata);
            }
                
            this.DtMv.View.Refresh();
        }

        #endregion

        #region 债券过滤面板操作 处理 
        /// <summary>
        /// 数据防抖动定时器
        /// </summary>
        private CancellationTokenSource _cts;
        private Task _debounceTask;

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; this.OnPropertyChanged(); }
        }

        public void NotifyFilter(bool isresetpage = true)
        {
            _cts?.Cancel(); // 取消上一次的任务
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (isresetpage)
                EMSIndex = 0;

            _debounceTask = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, token); // 防抖时间
                    await QueryDataAsync(token);
                }
                catch (TaskCanceledException) { /* 正常取消 */ }
            }, token);
        }

        /// <summary>
        /// 上一次请求字符串
        /// </summary>
        private string prevReq = null;

        /// <summary>
        /// 用户选择的标签对象
        /// </summary>
        private string _FavoritesTagReq;
        public string FavoritesTagReq
        {
            get 
            { 
                return _FavoritesTagReq; 
            }
            set 
            { 
                _FavoritesTagReq = value; 
                this.OnPropertyChanged(); 
            }
        }


        private async Task QueryDataAsync(CancellationToken token)
        {
            try
            {
                QueryBondPositionReq req = GetQueryBondPositionReq();
                req.pageNum = EMSIndex;
                req.pageSize = PageMaxCount;
                // 序列化请求参数用于对比
                string currentreq = CommonUtils.SerializeObject(req);
                //判断用户是否选择了自选标签
                if (FavoritesTagReq != null)
                {
                    QueryBondPositionReq tagreq = CommonUtils.DeserializeObject<QueryBondPositionReq>(FavoritesTagReq);
                    tagreq.pageNum = EMSIndex;
                    tagreq.pageSize = PageMaxCount;
                    string tagreqjson = CommonUtils.SerializeObject(tagreq);
                    if (currentreq != tagreqjson)
                    {
                        mainView.ResetMyFilterSelection();
                    }
                }

                if (!string.IsNullOrWhiteSpace(prevReq))
                {
                    if (currentreq == prevReq)
                    {
                        IsLoading = false;
                        LogHelper.Info($"本次请求与上一次请求相同！无需刷新!{currentreq}");
                        return;
                    }
                    else
                    {
                        LogHelper.Info($"GetQueryBondPosition RequestString Changed：{currentreq}");
                    }
                }
                else
                {
                    LogHelper.Info($"GetQueryBondPosition First RequestString：{currentreq}");
                }

                //请求刷新
                prevReq = currentreq;

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    IsLoading = true;
                    IsAllChecked = false;
                    DsBondPosition.Clear();
                    DsPledgePosition.Clear();
                }));


                // 执行耗时的查询逻辑
                await SimulateQueryAsync(token, req);

                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                // 用户快速修改导致的取消，忽略
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Error during NotifyFilter: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// 获取当前查询请求参数,用于判断是否需要重新查询，是否改变了请求参数
        /// </summary>
        /// <returns></returns>
        public QueryBondPositionReq GetQueryBondPositionReq()
        {
            QueryBondPositionReq req = new QueryBondPositionReq();
            //req.pageNum = EMSIndex;
            //req.pageSize = PageMaxCount;
            req.userName = LocalEnv.UserInfo.UserName; // 后台暂不支持
            req.tradeAccount = string.IsNullOrWhiteSpace(SelectedTradeAccountCode) ? null : SelectedTradeAccountCode;
            req.tradeManager = string.IsNullOrWhiteSpace(SelectetFundAccountItem) ? null : SelectetFundAccountItem;
            req.securityId = FilterText;
            if (IsFundAccount)
            {
                List<string> leafIds = new List<string>();
                if (string.IsNullOrWhiteSpace(req.tradeAccount))
                {
                    leafIds = GetAllLeafNodeIdsNonRecursive(MultiTreeComboxItemsSource).OrderBy(x => x).ToList();
                }
                else
                {
                    leafIds = req.tradeAccount.Split(',').ToList();
                }

                if (leafIds.Count == 0)
                {
                    LogHelper.Info("警告：该投资经理没有关联任何交易账户!");
                    req.tradeAccount = "-1";
                }
                else
                {
                    // 拆分
                    PPTradeAccountGroups grouped = SplitLeafIdsIntoGroups(leafIds, 1000);
                    req.tradeAccount = grouped.tradeAccount;
                    req.tradeAccount1 = grouped.tradeAccount1;
                    req.tradeAccount2 = grouped.tradeAccount2;
                    req.tradeAccount3 = grouped.tradeAccount3;
                    req.tradeAccount4 = grouped.tradeAccount4;
                    req.tradeAccount5 = grouped.tradeAccount5;
                    req.tradeAccount6 = grouped.tradeAccount6;
                    req.tradeAccount7 = grouped.tradeAccount7;
                    req.tradeAccount8 = grouped.tradeAccount8;
                    req.tradeAccount9 = grouped.tradeAccount9;
                    req.tradeAccount10 = grouped.tradeAccount10;
                }
            }


            if (!string.IsNullOrEmpty(StartExCloseValue) || !string.IsNullOrEmpty(EndExCloseValue))
                req.exClose2 = $"{StartExCloseValue ?? ""},{EndExCloseValue ?? ""}";

            if (!string.IsNullOrEmpty(this.SymbolType))
            {
                req.symbolType = this.SymbolType;
            }

            if (!string.IsNullOrEmpty(StartChinaBondValue) || !string.IsNullOrEmpty(EndChinaBondValue))
                req.chinaBondValue = $"{StartChinaBondValue ?? ""},{EndChinaBondValue ?? ""}";

            // 获取过滤标签
            req.reqTags = PPHandler.Instance.GetReqTags(DesSource, _focusTagDic);
            // 获取扩展参数
            req.extParams = PPHandler.Instance.GetReqExtParams(DesSource, _focusTagDic, StartTermDY, EndTermDY, StartBidBp, EndBidBp, StartOfrBp, EndOfrBp, StartCoupon, EndCoupon, StartPrice, EndPrice, StartChinaBondValue, EndChinaBondValue);

            return req;
        }



        public List<string> GetAllLeafNodeIdsNonRecursive(ObservableCollection<TreeNodeModel> roots)
        {
            var result = new List<string>();
            if (roots == null || roots.Count == 0)
                return result;

            var stack = new Stack<TreeNodeModel>();

            // 根节点入栈
            foreach (var node in roots)
                stack.Push(node);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current.Nodes == null || current.Nodes.Count == 0)
                {
                    // 叶子节点
                    if (!string.IsNullOrEmpty(current.Id))
                        result.Add(current.Id);
                }
                else
                {
                    // 子节点入栈
                    foreach (var child in current.Nodes)
                        stack.Push(child);
                }
            }

            return result;
        }

        public PPTradeAccountGroups SplitLeafIdsIntoGroups(List<string> leafIds, int maxPerGroup = 1000)
        {
            var result = new PPTradeAccountGroups();
            if (leafIds == null || leafIds.Count == 0)
                return result;

            // 拆分成子集合
            var groups = leafIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / maxPerGroup)
                .Select(g => string.Join(",", g.Select(x => x.id)))
                .Take(11) // 最多 11 个
                .ToList();

            var t = typeof(PPTradeAccountGroups);
            // 分配到 TradeAccount / TradeAccount0-9
            for (int i = 0; i < groups.Count; i++)
            {
                string value = groups[i];
                if (i == 0)
                    result.tradeAccount = value;
                else
                {

                    string propName = "tradeAccount" + (i.ToString()); // tradeAccount1..TradeAccount10
                    var prop = t.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(result, groups[i]);
                    }
                    else
                    {
                        // 说明属性不存在或不可写，可在此做降级处理（记录日志或放到字典）
                    }

                    //var prop = typeof(PPTradeAccountGroups).GetProperty($"tradeAccount{i}");
                    //if (prop != null)
                    //    prop.SetValue(result, value);
                }
            }

            return result;
        }
        /// <summary>
        /// 查询债券持仓
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SimulateQueryAsync(CancellationToken token, QueryBondPositionReq req)
        {
            //LogHelper.Info($"RequestTag with: {CommonUtils.SerializeObject(req)}");
            QueryBondPositionResult reply = await PAPositionHandler.Instance.GetQueryBondPosition(req);

            if (reply != null)
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    LogHelper.Info($"RequestQueryBondPosition Result: Return {reply.Datas.Count} Records; TotalRecord {reply.totalSize} Records.");

                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        TotalCount = reply.totalSize;
                        EMSPageControl.RecordCount = TotalCount;
                        EMSPageControl.PageSize = PageMaxCount;
                        QueryBondPositionReq orireq = CommonUtils.DeserializeObject<QueryBondPositionReq>(prevReq);
                        if (orireq.pageNum + 1 != EMSPageControl.Page)
                        {
                            EMSPageControl.Page = 1;
                        }
                    }));

                    UpdateQueryBondPositionData(reply.Datas);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        TotalCount = 0;
                        EMSPageControl.RecordCount = 0;
                        EMSPageControl.PageSize = PageMaxCount;
                        EMSPageControl.Page = 1;
                    }));
                    ////if (reply.Datas == null || reply.Datas?.Count == 0)
                    ////{
                    ////    //如果没有数据，则生成模拟数据
                    ////    List<StcBondPositionModel> simdata = GenerateSimulatedBondPositionData();
                    ////    Application.Current.Dispatcher.Invoke((Action)(() =>
                    ////    {
                    ////        TotalCount = simdata.Count;
                    ////        EMSPageControl.RecordCount = TotalCount;
                    ////        EMSPageControl.PageSize = PageMaxCount;
                    ////    }));

                    ////    UpdateQueryBondPositionData(simdata);
                    ////}
                }

            }
        }

        private void SetDesSource(ObservableCollection<DescriptionItem> datas, string key, string value)
        {
            if (key == Utils.Constants.BONDTYPE)
                return;

            var existingItem = datas.FirstOrDefault(item => item.Key == key);

            if (string.IsNullOrEmpty(value))
            {
                if (existingItem != null)
                {
                    datas.Remove(existingItem);
                }
            }
            else
            {
                if (existingItem != null)
                {
                    int index = datas.IndexOf(existingItem);
                    datas[index] = new DescriptionItem(value, key); // 替换为新对象
                }
                else
                {
                    datas.Add(new DescriptionItem(value, key));
                }
            }
        }

        #endregion

        #region Command 事件处理

        /// <summary>
        /// 刷新持仓
        /// </summary>
        private void OnRefreshCommand()
        {
            RefreshBtnEnable = false;
            
            prevReq = string.Empty;

            NotifyFilter();

            RefreshBtnEnable = true;
        }

        /// <summary>
        /// 复制代码
        /// </summary>
        private void OnCopyCodeCommand()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                List<StcBondPositionData> tmp = new List<StcBondPositionData>(DsBondPosition.Where(p => p.IsChecked));
                if (tmp.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (StcBondPositionData item in tmp)
                    {
                        sb.AppendLine($"{item.SecurityId}");
                    }

                    CopyToClipboard(sb.ToString().TrimEnd('\r', '\n'));
                }
                else
                {
                    CommonUtils.Prompt2Second("提示", "请选择需要复制代码的记录");
                }
            });
        }

        /// <summary>
        /// 复制简称
        /// </summary>
        private void OnCopyCodeAndNameCommand()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                List<StcBondPositionData> tmp = new List<StcBondPositionData>(DsBondPosition.Where(p => p.IsChecked));
                if (tmp.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (StcBondPositionData item in tmp)
                    {
                        sb.AppendLine($"{item.SymbolName}");
                    }

                    CopyToClipboard(sb.ToString().TrimEnd('\r', '\n'));
                }
                else
                {
                    CommonUtils.Prompt2Second("提示", "请选择需要复制代码的记录");
                }
            });
        }


        /// <summary>
        /// 复制持仓信息
        /// </summary>
        private void OnCopyPositionCommand()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                //首先检测持仓复制参数
                List<MsgAlertModel> PaCopyParams = PAPositionHandler.Instance.PAPostionCopyParams;
                if (PaCopyParams == null || PaCopyParams?.Count == 0)
                {
                    ShowWarnMessage("复制失败! \r\n未配置 持仓管理全局复制参数!");
                    LogHelper.Error("未配置 持仓管理全局复制参数！请联系管理员!");
                    return;
                }

                List<StcBondPositionData> selectedData = new List<StcBondPositionData>(DsBondPosition.Where(p => p.IsChecked)).ToList();
                if (selectedData.Count > 0)
                {
                    List<MsgAlertModel> fieldOrder = PAPositionHandler.Instance.PAPostionCopyParams.Where(o => o.ischecked).ToList();
                    
                    string str = ExportCheckedItemsToString(selectedData, fieldOrder);

                    CopyToClipboard(str.TrimEnd('\r', '\n'));
                }
                else
                {
                    CommonUtils.Prompt2Second("提示", "请选择需要复制持仓的记录");
                }
            });
        }

        /// <summary>
        /// 复制质押券信息
        /// </summary>
        public void OnCopyPledgeposition()
        {
            List<StcPledgePositionModelData> selectedData = new List<StcPledgePositionModelData>(DsPledgePosition.Where(p => p.IsChecked)).ToList();
            if (selectedData.Count == 0)
            {
                CommonUtils.Prompt2Second("提示", "没有选择质押券数据！");
                return;
            }

            List<MsgAlertModel> fieldOrder = new List<MsgAlertModel>();
            fieldOrder.Add(new MsgAlertModel() { code = "TradeType", name = "类型", ischecked = true});
            fieldOrder.Add(new MsgAlertModel() { code = "SecurityId", name = "债券代码", ischecked = true });
            fieldOrder.Add(new MsgAlertModel() { code = "SymbolName", name = "债券名称", ischecked = true });
            fieldOrder.Add(new MsgAlertModel() { code = "Side", name = "方向", ischecked = true });
            fieldOrder.Add(new MsgAlertModel() { code = "Qty", name = "质押量(万)", ischecked = true });
            fieldOrder.Add(new MsgAlertModel() { code = "MaturityDate", name = "质押到期日", ischecked = true });
            fieldOrder.Add(new MsgAlertModel() { code = "CounterParty", name = "质押对手方", ischecked = true });

            string str = ExportPledgePositionItemsToString(selectedData, fieldOrder, false);

            CopyToClipboard(str.TrimEnd('\r', '\n'));
        }

        /// <summary>
        /// 导出选中持仓信息为字符串，当需要导出表头时，使用逗号分隔符
        /// </summary>
        /// <param name="selectedData"></param>
        /// <param name="fieldOrder"></param>
        /// <param name="isquotationmarks">字符串和日期是否包含引号 true</param>
        /// <param name="deci">分隔</param>
        /// <param name="isExportHeader">是否导出字段名</param>
        /// <returns></returns>
        public string ExportPledgePositionItemsToString(List<StcPledgePositionModelData> selectedData, List<MsgAlertModel> fieldOrder,bool isquotationmarks = true, string deci = "\t", bool isExportHeader = false)
        {
            StringBuilder sb = new StringBuilder();
            var fields = fieldOrder.Select(f => f.code).ToList();
            if (isExportHeader && fields.Count == 0)
            {
                return null;
            }

            if (isExportHeader && fields.Count > 0)
            {
                // 如果需要导出表头，则写入表头
                deci = ",";
                var chsfields = fieldOrder.Select(f => f.name).ToList();
                // 写入表头（字段名）
                var headerLine = string.Join(",", chsfields.Select(f => $"\"{f}\""));
                sb.AppendLine(headerLine);
            }


            //指定保留0位小数的字段 
            var decimal0Fields = new List<string> { "Qty"}; // 指定保留0位小数的字段 

            foreach (StcPledgePositionModelData item in selectedData)
            {
                List<string> values = new List<string>();

                var proptradetype = typeof(StcPledgePositionModelData).GetProperty("TradeType", BindingFlags.Public | BindingFlags.Instance);
                string tradetype = proptradetype?.GetValue(item)?.ToString() ?? "1"; // 默认质押

                foreach (var field in fields)
                {
                    var prop = typeof(StcPledgePositionModelData).GetProperty(field, BindingFlags.Public | BindingFlags.Instance);

                    if (prop != null)
                    {
                        var value = prop.GetValue(item);
                        if (value == null)
                        {
                            values.Add("");
                        }
                        else
                        {
                            Type type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                            if (type == typeof(string))
                            {
                                string str = string.Empty;
                                switch (field)  
                                {
                                    case "TradeType":
                                        //1:质押, 2:STC在途意向,3:STC在途成交 PATradeType
                                        str = ComboSourceUtil.Instance.FindEnum(value.ToString(), "patradetype");
                                        break;
                                    case "Side":
                                        //"1", "正回购 "2", "STC在途意向Bid" "3", "STC在途意向Ofr" "4", "STC在途成交Bid" "5", "STC在途成交Ofr"
                                        str = ComboSourceUtil.Instance.FindEnum(value.ToString(), "patradeside");
                                        break;
                                    case "MaturityDate":
                                        str = value.ToString().Replace("\"", "\"\""); // 转义引号
                                        if (tradetype == "1") // 质押
                                        {
                                            str = $"{str} 到期";
                                        }
                                        else if (tradetype == "2") // STC在途意向
                                        {
                                            str = $"{str} 交割";
                                        }
                                        else if (tradetype == "3") // STC在途成交
                                        {
                                            str = $"{str} 交割";
                                        }

                                        break;
                                    case "CounterParty":
                                        // 质押对手方
                                        str = value.ToString().Replace("\"", "\"\""); // 转义引号
                                        if (tradetype == "1") // 质押
                                        {
                                            str = $"对手方:{str}";
                                        }
                                        else if (tradetype == "2") // STC在途意向
                                        {
                                            var propTradeName = typeof(StcPledgePositionModelData).GetProperty("TradeName", BindingFlags.Public | BindingFlags.Instance);
                                            string tradename = propTradeName?.GetValue(item)?.ToString() ?? str; // 默认对手方
                                            str = $"交易员:{tradename}";
                                        }
                                        else if (tradetype == "3") // STC在途成交
                                        {
                                            str = $"对手方:{str}";
                                        }
                                        break;
                                    default:
                                        str = value.ToString().Replace("\"", "\"\""); // 转义引号
                                        break;
                                }

                                if (isquotationmarks)
                                    values.Add($"\"{str}\"");
                                else
                                    values.Add($"{str}");
                            }
                            else if (type == typeof(decimal))
                            {
                                decimal decValue = (decimal)value;

                                if (decimal0Fields.Contains(field))
                                {
                                    if (field == "Qty")
                                       values.Add($"{decValue.ToString("F0")}万");
                                    else
                                       values.Add(decValue.ToString("F0"));
                                }
                                else
                                {
                                    values.Add(decValue.ToString("F4"));
                                }
                            }
                            else if (type == typeof(bool))
                            {
                                values.Add((bool)value ? "true" : "false");
                            }
                            else if (type == typeof(DateTime))
                            {
                                string formattedDate = ((DateTime)value).ToString("yyyy-MM-dd");
                                if (isquotationmarks)
                                    values.Add($"\"{formattedDate}\"");
                                else
                                    values.Add($"{formattedDate}");
                            }
                            else
                            {
                                values.Add(value.ToString());
                            }
                        }
                    }
                    else
                    {
                        values.Add(""); // 字段不存在，留空
                    }
                }

                sb.AppendLine(string.Join(deci, values));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 导出
        /// </summary>
        private void OnExportCommand()
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                try
                {
                    saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                    saveFileDialog.FileName = $"持仓管理_{DateTimeEx.ServerTime.ToString("yyyyMMddHHmmss")}";
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.RestoreDirectory = true;
                    if ((bool)(saveFileDialog.ShowDialog()))
                    {
                        string fileName = saveFileDialog.FileName;
                        if (string.IsNullOrEmpty(fileName))
                        {
                            CommonUtils.Prompt2Second("提示", "请选择导出文件路径！");
                            return;
                        }

                        IsLoading = true;

                        DsFullBondPosition.Clear();

                        await GetFullData();


                        List<StcBondPositionData> selectedData = DsFullBondPosition.ToList();
                        if (selectedData.Count == 0)
                        {
                            IsLoading = false;
                            CommonUtils.Prompt2Second("提示", "没有可导出的数据！");
                            return;
                        }


                        //导出所有的字段
                        List<MsgAlertModel> fieldOrder = PAPositionHandler.Instance.PAPostionCopyParams.ToList();
                        string str = ExportCheckedItemsToString(selectedData, fieldOrder, true, ",", true);
                        if (str != null)
                        {
                            File.WriteAllText(fileName, str.TrimEnd('\r', '\n'), Encoding.UTF8);
                        }

                        IsLoading = false;
                        Common.View.Component.CustomMessageBox.Info($"《{saveFileDialog.FileName}》已保存");
                    }
                }
                catch (Exception ex)
                {
                    IsLoading = false;
                    LogHelper.Error("导出失败", ex);
                    CommonUtils.Prompt2Second("提示", "导出失败，请检查是否有权限访问文件夹！");
                }
            }));
        }


        #region 重新获取全部的持仓数据
        public async Task GetFullData()
        {
            // 序列化请求参数用于对比
            QueryBondPositionReq orireq = CommonUtils.DeserializeObject<QueryBondPositionReq>(prevReq);
            orireq.pageNum = 0;
            orireq.pageSize = int.MaxValue;

            QueryBondPositionResult reply = await PAPositionHandler.Instance.GetQueryBondPosition(orireq);

            if (reply != null)
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    LogHelper.Info($"RequestQueryFullBondPosition Result: Return {reply.Datas.Count} Records; TotalRecord {reply.totalSize} Records.");

                    foreach (StcBondPositionModel obj in reply.Datas)
                    {
                        UpdateBondPositioRecord(obj, DsFullBondPosition);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 导出选中持仓信息为字符串，当需要导出表头时，使用逗号分隔符
        /// </summary>
        /// <param name="selectedData"></param>
        /// <param name="fieldOrder"></param>
        /// <param name="isquotationmarks">字符串和日期是否包含引号 true</param>
        /// <param name="deci">分割符 默认Tab</param>
        /// <param name="isExportHeader">是否导出字段名</param>
        /// /// <param name="salStcAvailableAmount">指定替换字段值</param>
        /// <returns></returns>
        public string ExportCheckedItemsToString(List<StcBondPositionData> selectedData, List<MsgAlertModel> fieldOrder, bool isquotationmarks = false, string deci = "\t", bool isExportHeader = false, decimal salStcAvailableAmount = 0M)
        { 
            StringBuilder sb = new StringBuilder();
            var fields = fieldOrder.Select(f => f.code).ToList();
            if (isExportHeader && fields.Count == 0)
            {
                CommonUtils.Prompt2Second("提示", "没有可导出的字段，请先设置持仓管理复制参数！");
                return null;
            }

            if (isExportHeader && fields.Count > 0)
            {
                // 如果需要导出表头，则写入表头
                deci = ",";
                var chsfields = fieldOrder.Select(f => f.name).ToList();
                // 写入表头（字段名）
                var headerLine = string.Join(",", chsfields.Select(f => $"\"{f}\""));
                sb.AppendLine(headerLine);
            }


            //指定保留0位小数的字段 
            var decimal0Fields = new List<string> { "AvailablePosition", "StcInSalesVolume", "StcAvailablePosition", "StcInBuyVolume", "Quotalimit" , "OutStandingAmount", "TotalQty", "StcAvailableLimit" }; // 指定保留0位小数的字段 

            foreach (StcBondPositionData item in selectedData)
            {
                List<string> values = new List<string>();

                foreach (var field in fields)
                {
                    var prop = typeof(StcBondPositionData).GetProperty(field, BindingFlags.Public | BindingFlags.Instance);

                    if (prop != null)
                    {
                        var value = prop.GetValue(item);
                        if (value == null)
                        {
                            values.Add("");
                        }
                        else
                        {
                            Type type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                            if (type == typeof(string))
                            {
                                string str = value.ToString().Replace("\"", "\"\""); // 转义引号
                                if (isquotationmarks)
                                    values.Add($"\"{str}\"");
                                else
                                    values.Add($"{str}");
                            }
                            else if (type == typeof(decimal))
                            {
                                decimal decValue = (decimal)value;

                                if (decimal0Fields.Contains(field))
                                {
                                    if (field == "StcAvailablePosition")
                                    {
                                        if (salStcAvailableAmount > 0)
                                        {
                                            //替换可用持仓量，用于卖出
                                            values.Add(salStcAvailableAmount.ToString("F0"));
                                        }
                                        else
                                        {
                                            values.Add(decValue.ToString("F0"));
                                        }
                                    }
                                    else
                                        values.Add(decValue.ToString("F0"));
                                }
                                else
                                {
                                    values.Add(decValue.ToString("F4"));
                                }
                            }
                            else if (type == typeof(bool))
                            {
                                values.Add((bool)value ? "true" : "false");
                            }
                            else if (type == typeof(DateTime))
                            {
                                string formattedDate = ((DateTime)value).ToString("yyyy-MM-dd");
                                if (isquotationmarks)
                                    values.Add($"\"{formattedDate}\"");
                                else
                                    values.Add($"{formattedDate}");
                            }
                            else
                            {
                                values.Add(value.ToString());
                            }
                        }
                    }
                    else
                    {
                        values.Add(""); // 字段不存在，留空
                    }
                }

                sb.AppendLine(string.Join(deci, values));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 全部卖出
        /// </summary>
        private void OnSalelAllCommand()
        {
            List<StcBondPositionData> selectedData = new List<StcBondPositionData>(DsBondPosition.Where(p => p.IsChecked)).ToList();
            if (selectedData.Count == 0)
            {
                CommonUtils.Prompt2Second("提示", "没有选择持仓数据！");
                return;
            }

            decimal minAver = selectedData.Min(x => x.StcAvailablePosition);
            if (minAver <= 0)
            {
                PartiallySoldPositionView Mppv = new PartiallySoldPositionView();
                Mppv.SetShowWarnTxt(selectedData.Count > 1);
                Mppv.SetMinStcAvailablePositionTxt(minAver);
                Mppv.ShowDialog();
                if (Mppv.DialogResult == true)
                {
                    //获取输入的数量
                    decimal inputAmount = CommonUtils.ConvertToDecimal(Mppv.AmountTxt.Text);
                    Mppv?.Close();
                    if (inputAmount <= 0)
                    {
                        CommonUtils.Prompt2Second("提示", "输入的数量必须大于0！");
                        return;
                    }

                    List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> saleInfo = GetAllSaleQuoteInfo(selectedData, inputAmount.ToString());
                    STC.Common.Data.DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(false, saleInfo);
                }

            }
            else
            {
                List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> saleInfo = GetAllSaleQuoteInfo(selectedData, null);
                STC.Common.Data.DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(false, saleInfo);
            }

            //string str = ExportCheckedItemsToString(selectedData, fieldOrder);
            //if (str != null)
            //{
            //    string _value = str.TrimEnd('\r', '\n');
            //    //弹出批量识别窗口
            //    System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            //    {
            //        if (!string.IsNullOrEmpty(_value))
            //        {
            //            try
            //            {
            //                Clipboard.SetText(_value);
            //            }
            //            catch { }
            //        }

            //        STC.Common.Data.DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(false, textList);
            //    }));
            //}
        }


        /// <summary>
        /// 部分卖出
        /// </summary>
        private void OnPartiallySoldCommand()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                List<StcBondPositionData> selectedData = new List<StcBondPositionData>(DsBondPosition.Where(p => p.IsChecked)).ToList();
                if (selectedData.Count == 0)
                {
                    CommonUtils.Prompt2Second("提示", "没有选择持仓数据！");
                    return;
                }

                decimal minAver = selectedData.Min(x => x.StcAvailablePosition);
                //if (minAver <= 0)
                //{
                //    CustomMessageBox.NewError("选择的持仓记录中，STC可用持仓量最小值小于等于0 ！","提示",null , "我知道了");
                //    return;
                //}
                
                PartiallySoldPositionView Mppv = new PartiallySoldPositionView();
                Mppv.SetMinStcAvailablePositionTxt(minAver);
                Mppv.SetAmountText(minAver.ToString("F0"));
                Mppv.SetShowWarnTxt(selectedData.Count > 1);
                Mppv.ShowDialog();
                if (Mppv.DialogResult== true)
                {
                    //获取输入的数量
                    decimal inputAmount = CommonUtils.ConvertToDecimal(Mppv.AmountTxt.Text);
                    Mppv?.Close();
                    if (inputAmount <= 0)
                    {
                        CommonUtils.Prompt2Second("提示", "输入的数量必须大于0！");
                        return;
                    }

                    ////导出字段
                    //List<MsgAlertModel> fieldOrder = new List<MsgAlertModel>();
                    //fieldOrder.Add(new MsgAlertModel() { id = "1_1", pid = "1", code = "SecurityId", name = "债券代码", ischecked = true, visible = true });
                    //fieldOrder.Add(new MsgAlertModel() { id = "1_2", pid = "1", code = "SymbolName", name = "债券简称", ischecked = true, visible = true });
                    //fieldOrder.Add(new MsgAlertModel() { id = "1_3", pid = "1", code = "PortfolioNo", name = "组合代码", ischecked = true, visible = true });
                    //fieldOrder.Add(new MsgAlertModel() { id = "1_4", pid = "1", code = "PortfolioName", name = "组合名称", ischecked = true, visible = true });
                    //fieldOrder.Add(new MsgAlertModel() { id = "1_5", pid = "1", code = "TradeAccount", name = "交易账户", ischecked = true, visible = true });
                    //fieldOrder.Add(new MsgAlertModel() { id = "1_6", pid = "1", code = "TradeName", name = "交易账户名称", ischecked = true, visible = true });
                    //fieldOrder.Add(new MsgAlertModel() { id = "1_7", pid = "1", code = "FundAccount", name = "投资经理", ischecked = false, visible = true });
                    //fieldOrder.Add(new MsgAlertModel() { id = "1_8", pid = "1", code = "StcAvailablePosition", name = "Stc可用持仓量", ischecked = false, visible = true });
                    //fieldOrder.Add(new MsgAlertModel() { id = "1_9", pid = "1", code = "ValuationYield", name = "中债估值", ischecked = false, visible = true });
                    List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> saleInfo = GetAllSaleQuoteInfo(selectedData, inputAmount.ToString());
                    STC.Common.Data.DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(false, saleInfo);


                    ////批量卖出
                    //string str = ExportCheckedItemsToString(selectedData, fieldOrder,false,"\t",false, inputAmount);
                    //if (str != null)
                    //{
                    //    string _value = str.TrimEnd('\r', '\n');
                    //    //弹出批量识别窗口
                    //    if (!string.IsNullOrEmpty(_value))
                    //    {
                    //        try
                    //        {
                    //            Clipboard.SetText(_value);
                    //        }
                    //        catch { }
                    //    }

                    //    STC.Common.Data.DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(false, textList);
                    //}
                }
            }));
        }

        /// <summary>
        /// 获取待卖数据
        /// </summary>
        /// <param name="selectedData"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> GetAllSaleQuoteInfo(List<StcBondPositionData> selectedData, string inputAmount = null)
        {
            List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> textList = new List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>();
            foreach (StcBondPositionData data in selectedData)
            {
                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj = new Common.Model.Table.WorkBrench.IntentionQuoteInfo();

                string key = data.SecurityId + data.PortfolioNo + data.TradeAccount;
                obj.clordId = obj.clordid = key;
                obj.securityId = data.SecurityId;
                obj.symbol = data.SymbolName;
                obj.timeToMaturity = data.RemaingingTime;
                obj.side = "2";
                obj.bookId = data.PortfolioNo;
                obj.bookName = data.PortfolioName;
                obj.accountID = data.TradeAccount;
                obj.accountName = data.TradeName;
                //obj.price = data.ValuationYield.ConvertToDecimal();
                obj.qty = string.IsNullOrEmpty(inputAmount) ? data.StcAvailablePosition : CommonUtils.ConvertToDecimal(inputAmount);
                obj.settlType = "1";
                obj.userName = LocalEnv.UserInfo.UserName;
                obj.closeBy = LocalEnv.UserInfo.UserName;
                obj.issuerName = data.Issuer;
                obj.cleanPrice = data.Netprice;
                obj.chinaBondValuation = data.ValuationYield2.ConvertToDecimal();
                obj.chinaBondValuationFull = data.ValuationYield2;
                obj.csiBondValuation = data.CsiBondValuation.ConvertToDecimal();
                obj.csiBondValuationFull = data.CsiBondValuation;
                obj.hsProductId = data.HsProductId;
                textList.Add(obj);
            }

            return textList;
        }

        /// <summary>
        /// 打开个人设置持仓管理
        /// </summary>
        private void OnSetCopyPositionFieldCommand()
        {
            if (!WindowManager.Instance.IsExist(nameof(SettingView)))
            {
                SettingView settingView = new SettingView();
                DesignHandler.Instance.showDesignUI(settingView);
                settingView.SetCopyPositionSettingSelect();
                WindowManager.Instance.OpenOnlyOneWindow(nameof(SettingView), settingView);
            }
            else
            {
                SettingView settingView = WindowManager.Instance.GetExistWin(nameof(SettingView)) as SettingView;
                if (settingView != null)
                    settingView.SetCopyPositionSettingSelect();
            }
        }

        /// <summary>
        /// todo 保存组 
        /// </summary>
        private void OnSaveSearchGroupCommand()
        { 
            
        }

        #endregion

        #region Funciton

        /// <summary>
        /// 全选
        /// </summary>
        public void OnCheckAll()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                foreach (StcBondPositionData addedItem in this.Mv.View)
                {
                    addedItem.IsChecked = IsAllChecked;
                }
            }));
        }

        /// <summary>
        /// 复制到剪贴板
        /// </summary>
        /// <param name="s"></param>
        public void CopyToClipboard(string copyTxt)
        {
            ClipboardHelper.CopyToClipboard(copyTxt);
            CommonUtils.Prompt2Second("提示", "已复制");
        }


        /// <summary>
        /// 显示警告信息
        /// </summary>
        /// <param name="s"></param>
        public void ShowWarnMessage(string s)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg(s, true);
                msgBox.ShowDialog();
            }));
        }
        /// <summary>
        /// 添加到队列
        /// </summary>
        /// <param name="obj"></param>
        //public void AddQueue(NetPriceValuaModel obj)
        //{
        //    lock (_queueLock)
        //    {
        //        _ProcessNetPriceThreadQueue.Enqueue(obj);
        //    }
        //}


        // 判断命令是否可执行
        private bool CanExecuteStockCommand()
        {
            return DsBondPosition.Count(o => o.IsChecked) > 0;
        }

        #endregion

        #region Dispose 事件处理
        ~MainViewModel()
        {

        }

        public void TryDispose()
        {
            //PositionHandler.Instance.UnSubscribePositionDetail(OnPostionDetailCallBack);

            //string s = string.Empty;
            //while (!securityidQueue.IsCompleted && securityidQueue.Count > 0)
            //{
            //    securityidQueue.TryTake(out s);
            //}

            //List<PositionDetailModel> tmplist = new List<PositionDetailModel>();
            //while (!logQueue.IsCompleted && logQueue.Count > 0)
            //{
            //    logQueue.TryTake(out tmplist);
            //}

            //cancellationTokenSource.Cancel();

            //ClearTotal();
            //FullPositionDetail.Clear();
            //this.Dispose();
            base.Dispose();
        }
        #endregion

        #region 模拟数据
        /// <summary>
        /// 模拟数据
        /// </summary>
        /// <returns></returns>
        public List<StcBondPositionModel> GenerateSimulatedBondPositionData()
        {
            List<StcBondPositionModel> res = new List<StcBondPositionModel>();
            StcBondPositionModel dat = new StcBondPositionModel
            {
                csiBondValuation = "3.12",
                exClose = "3.11",
                exClose2 = "3.15",
                fundAccount = "李经理",
                issuer = "上海浦东发展银行",
                portfolioName = "主投资组合",
                portfolioNo = "PORT001",
                availablePosition = 100000.0M,
                securityId = "180210.IB",
                stcAvailablePosition = 1000,
                stcInBuyVolume = -2000,
                stcInSalesVolume = 1000,
                symbolName = "18国债10",
                tradeAccount = "ACCT001",
                tradeName = "主交易账户",
                valuationYield = "3.12",
                valuationYield2 = "3.12",
            };

            res.Add(dat);

            StcBondPositionModel dat2 = new StcBondPositionModel
            {
                csiBondValuation = "3.12",
                exClose = "3.11",
                exClose2 = "3.15",
                fundAccount = "李经理",
                issuer = "中华人民共和国财政部",
                portfolioName = "固定收益组合",
                portfolioNo = "PORT002",
                availablePosition = 50000.0M,
                securityId = "019657.SH",
                stcAvailablePosition = 1000,
                stcInBuyVolume = 2000,
                stcInSalesVolume = 1000,
                symbolName = "21国债09",
                tradeAccount = "ACCT002",
                tradeName = "债券交易账户",
                valuationYield = "3.12",
                valuationYield2 = "3.12",
            };

            res.Add(dat2);

            StcBondPositionModel dat3 = new StcBondPositionModel
            {
                csiBondValuation = "3.12",
                exClose = "3.11",
                exClose2 = "3.15",
                fundAccount = "王经理",
                issuer = "中华人民共和国财政部",
                portfolioName = "其它组合",
                portfolioNo = "PORT003",
                availablePosition = 50000.0M,
                securityId = "019657.SH",
                stcAvailablePosition = -1000,
                stcInBuyVolume = 2000,
                stcInSalesVolume = 1000,
                symbolName = "21国债09",
                tradeAccount = "ACCT002",
                tradeName = "债券交易账户",
                valuationYield = "3.12",
                valuationYield2 = "3.12",
            };

            res.Add(dat3);
            return res;
        }

        /// <summary>
        /// 获取模拟字典数据
        /// </summary>
        public void GenerateSimlatedDict()
        {
            DictFundAccount.Add("测试模拟绩效经理1", "测试模拟绩效经理1");
            DictFundAccount.Add("测试模拟绩效经理2", "测试模拟绩效经理2");
        }

        #endregion
    }
}
