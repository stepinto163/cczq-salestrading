using Newtonsoft.Json;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.IRS.Plugin.View;
using STC.IRS.Plugin.ViewModel.Data;
using STC.IRS.Plugin.View.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using STC.Common.Utils;
using STC.Common.Data;
using STC.Common.Model.Table;
using com.gateway.log;
using STC.Common.Handler.Table;
using STC.Common.Model.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;
using STC.Common.Command;
using STC.Common.Model.Fix.Message;
using System.Threading;
using System.Collections.Concurrent;
using STC.Common.Model;
using STC.Common.Model.TBook;
using STC.Common.View.Order.InnerPanel;
using STC.Common.Handler.SystemParam;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Common.Model.Cal;
using STC.Common.Handler.Cal;
using STC.Common.Handler.Admin;
using System.ComponentModel;
using STC.Common.Model.CRM;
using STC.Common.Handler.Base;
using STC.Common.ViewModel.Data;

namespace STC.IRS.Plugin.ViewModel
{
    public class InnerHistoryRequireViewModel : ViewModelBase
    {
        internal bool isInit = false;
        private InnerHistoryRequire myWindow;
        public InnerBoardViewModel innerBoardViewModel;
        private int PageMaxCount = 2000;

        #region 构造
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
                            FocusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);
                        }
                        else
                        {
                            FocusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS);
                        }
                    }
                    catch (Exception)
                    {
                        FocusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS);
                    }

                    if (this.SelectedItem_Tag != null && this.SelectedItem_Tag.FocusTags != value)
                    {
                        this.SelectedItem_Tag = null;
                    }

                    this.OnPropertyChanged();
                    this.Mv.View?.Refresh();
                }
            }
        }

        private Dictionary<string, List<string>> _focusTagDic;
        /// <summary>
        /// 筛选字典
        /// </summary>
        public Dictionary<string, List<string>> FocusTagDic
        {
            get
            {
                return _focusTagDic;
            }
            set
            {
                _focusTagDic = value;
                this.OnPropertyChanged();

                //this.ViewSource.View?.Refresh();
            }
        }

        private List<string> _cancelOrderType;
        /// <summary>
        /// 撤销类型过滤
        /// </summary>
        public List<string> CancelOrderType
        {
            get
            {
                return _cancelOrderType;
            }
            set
            {
                _cancelOrderType = value;
                this.OnPropertyChanged();

                this.Mv.View?.Refresh();
            }

        }

        private string _SearchText;
        /// <summary>
        /// ShowAll 时触发
        /// </summary>
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
                // to do ... 清空过滤条件

                this.Mv.View?.Refresh();
            }
        }



        private string _best_SearchText;
        /// <summary>
        /// 模糊查询 债券代码，客户机构， 客户名
        /// </summary>
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
                this.Mv.View?.Refresh();
            }
        }


        private bool _isMe;
        // 选择我
        public bool IsMe
        {
            get { return _isMe; }
            set
            {
                bool oldvalue = _isMe;
                _isMe = value;
                this.OnPropertyChanged();
                this.Mv.View?.Refresh();
            }
        }

        /// <summary>
        /// 1:做市 2:销交
        /// </summary>
        private string _innerType = "1";

        ////**************** 逐笔 **********************
        internal object _dataLock = new object();

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

        private ObservableCollection<InnerOrderData> _innerOrders = new ObservableCollection<InnerOrderData>();

        public ObservableCollection<InnerOrderData> InnerOrders
        {
            get
            {
                if (_innerOrders == null)
                    _innerOrders = new ObservableCollection<InnerOrderData>();

                return _innerOrders;
            }

            set
            {
                _innerOrders = value;
                this.OnPropertyChanged(nameof(InnerOrders));
            }
        }

        private bool dealTimeChecked = true;
        /// <summary>
        /// 是否按时间查询
        /// </summary>
        public bool DealTimeChecked
        {
            get { return dealTimeChecked; }
            set
            {
                bool oldvalue = dealTimeChecked;
                dealTimeChecked = value;
                this.OnPropertyChanged();

                if (oldvalue != value)
                {
                    if (!isInit)
                    {
                        BeginQuery();
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this.QueryHistoryIntentionOrder();
                        }));
                    }
                }
            }
        }

        private DateTime? dealTimeBegin;
        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? DealTimeBegin
        {
            get { return dealTimeBegin; }
            set
            {
                dealTimeBegin = value;
                this.OnPropertyChanged();
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
                dealTimeEnd = value;
                this.OnPropertyChanged();
            }
        }

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

        private InnerOrderData selectorder;
        public InnerOrderData SelectOrder
        {
            get
            {
                return selectorder;
            }
            set
            {
                selectorder = value;
                this.OnPropertyChanged();
            }
        }


        private int totalCount = 0;
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

        private string emsPageVisible = "false";
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

        private Visibility loadmaskvisible = Visibility.Collapsed;
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
        public Visibility LoadMaskVisible
        {
            get
            {
                return loadmaskvisible;
            }

            set
            {
                loadmaskvisible = value;
                this.OnPropertyChanged(nameof(LoadMaskVisible));
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

        private object _queueLock = new object();
        /// <summary>
        /// 处理净价的线程队列
        /// </summary>
        private AsyncThreadQueue<NetPriceValuaModel> _ProcessNetPriceThreadQueue;

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
                this.Mv.View?.Refresh();
            }
        }

        private string queryPermission = "500401";
        /// <summary>
        /// 作废区查询权限
        /// </summary>
        /// <remarks>
        /// 500401 '无'                
        /// 500402 '查看本人'
        /// 500403 '查看做市'
        /// 500404 '查看销交'
        /// 500405 '查看所有'
        /// </remarks>
        public string QueryPermission
        {
            get
            {
                return queryPermission;
            }
            set
            {
                queryPermission = value;
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


        #region 分组标签
        private CollectionViewSource _viewSource_tag = new CollectionViewSource();
        public CollectionViewSource ViewSource_Tag
        {
            get { return _viewSource_tag; }
            set { _viewSource_tag = value; }
        }

        private FilterGroupItem _selectedItem_tag_best;
        /// <summary>
        /// 分组标签
        /// </summary>
        public FilterGroupItem SelectedItem_Tag_Best
        {
            get { return _selectedItem_tag_best; }
            set
            {
                if (_selectedItem_tag_best != value)
                {
                    _selectedItem_tag_best = value;

                    if (_selectedItem_tag_best != null)
                    {
                        this.FocusTags_Best = _selectedItem_tag_best.FocusTags;
                    }
                    else
                    {
                        //this.FocusTags_Best = InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS;
                    }

                    this.OnPropertyChanged();
                }
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
                    ////this.ViewSource_Best.View?.Refresh();
                }
            }
        }

        private Dictionary<string, List<string>> _focusTagDic_Best;

        bool type1_flag;
        bool type2_flag;
        bool type3_flag;

        public const string MODULEID_CLIENT = "5003";
        public const string TYPE1 = "500301";
        public const string TYPE2 = "500302";
        public const string TYPE3 = "500303";

        private bool _IsAppTerm;

        public bool IsAppTerm
        {
            get { return _IsAppTerm; }
            set
            {
                _IsAppTerm = value;
                this.OnPropertyChanged();
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
                this.Mv.View?.Refresh();
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


        private bool _isCoupon;

        public bool IsCoupon
        {
            get { return _isCoupon; }
            set
            {
                _isCoupon = value;
                this.OnPropertyChanged();
                this.Mv.View?.Refresh();
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


        private bool _isPrice;

        public bool IsPrice
        {
            get { return _isPrice; }
            set
            {
                _isPrice = value;
                this.OnPropertyChanged();
                this.Mv.View?.Refresh();
            }
        }

        private Dictionary<string, string> dictcrminst;
        /// <summary>
        /// 客户类型
        /// </summary>
        public Dictionary<string,string> DictCrmInst
        {
            get { return dictcrminst; }
            set
            {
                dictcrminst = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region Command
        public ICommand RefreshCommand { get; set; }
        public ICommand ResumeCommand { get; set; }

        public ICommand BatchResumeCommand { get; set; }
        public ICommand SearchUseDateCommand { get; set; }
        
        public ICommand NewRequireCommand { get; set; }

        public ICommand OpenRecognizeWinCommand { get; set; }
        #endregion

        
        #endregion

        public InnerHistoryRequireViewModel()
        {
            this.RefreshCommand = new ComRelayCommand(OnRefreshCommand);
            this.ResumeCommand = new DelegateCommand(new Action<object>(OnResumeCommand));
            this.SearchUseDateCommand = new ComRelayCommand(OnSearchUseDateCommand);
            this.BatchResumeCommand = new ComRelayCommand(OnBatchResumeCommand);
            this.NewRequireCommand = new ComRelayCommand(OnNewRequireCommand);
            this.OpenRecognizeWinCommand = new ComRelayCommand(OnOpenRecognizeWinCommand);

            if (_ProcessNetPriceThreadQueue == null)
            {
                _ProcessNetPriceThreadQueue = new AsyncThreadQueue<NetPriceValuaModel>(UpdateNetPriceData);
            }

            if (IntentionIRSHandler.Instance.isSALER())
            {
                _innerType = "2";
            }

            type1_flag = AdminHandler.Instance.CheckPermission(MODULEID_CLIENT, TYPE1);
            type2_flag = AdminHandler.Instance.CheckPermission(MODULEID_CLIENT, TYPE2);
            type3_flag = AdminHandler.Instance.CheckPermission(MODULEID_CLIENT, TYPE3);

            //作废区查询权限
            if (AdminHandler.Instance.CheckPermission("5004", "500402"))
            {
                QueryPermission = "500402";
            }
            else if (AdminHandler.Instance.CheckPermission("5004", "500403"))
            {
                QueryPermission = "500403";
            }
            else if (AdminHandler.Instance.CheckPermission("5004", "500404"))
            {
                QueryPermission = "500404";
            }
            else if (AdminHandler.Instance.CheckPermission("5004", "500405"))
            {
                QueryPermission = "500405";
            }
            else
            {
                QueryPermission = "500401";
            }
        }



        public void Init(InnerHistoryRequire ownWindow)
        {
            myWindow = ownWindow;
            // 初始化撤销类型
            InitCancelOrderType();
            IsShowFuzzy = LocalEnv.ConfInfo.IsShowInnerFuzzyRequire == "1" ? Visibility.Visible : Visibility.Collapsed;
            //分组标签
            ViewSource_Tag.Source = IntentionOldHandler.Instance.GroupList;

            //客户类型
            DictCrmInst = GetCrmInstDict();

            this.Mv.Source = InnerOrders;
            this.mv.View.CollectionChanged += View_CollectionChanged;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;

            this.Mv.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));

            SetMvFilter();

            InitHistoryRequireOrder();
        }


        
        #region 加载数据
        private void InitHistoryRequireOrder()
        {
            BeginQuery(true);
            QueryHistoryIntentionOrder();
        }

        private void BeginQuery(bool isfirst = false)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                EMSIndex = 0;
                EMSPageControl.Page = 1;
                if (isfirst)
                {
                    isInit = true;
                }
                else
                {
                    InnerOrders.Clear();
                    this.Mv.View.Refresh();
                }

                LoadMaskVisible = Visibility.Visible;
            }));
        }

        public void QueryHistoryIntentionOrder()
        {
            if (DealTimeChecked)
            {
                string dealDateBegin = this.DealTimeBegin.HasValue ? $"{this.DealTimeBegin.Value.ToString("yyyy-MM-dd")}" : "";
                string dealDateEnd = this.DealTimeEnd.HasValue ? $"{this.DealTimeEnd.Value.ToString("yyyy-MM-dd")}" : "";
                RequestIntentionOrder(dealDateBegin, dealDateEnd);
            }
            else
            {
                RequestIntentionOrder("", "");
            }
        }


        /// <summary>
        /// 按日期查询数据，默认当天，当dealTimeChecked = false时查询所有
        /// </summary>
        /// <param name="dealTimeBegin"></param>
        /// <param name="dealTimeEnd"></param>
        /// <param name="dealTimeChecked"></param>
        private async void RequestIntentionOrder(string dealTimeBegin = "", string dealTimeEnd = "")
        {
            QueryHistoryIntentionOrderReq request = new QueryHistoryIntentionOrderReq();
            if ((!string.IsNullOrEmpty(dealTimeBegin)) && (!string.IsNullOrEmpty(dealTimeEnd)))
            {
                request.startdate = dealTimeBegin;
                request.enddate = dealTimeEnd;
            }

            switch (QueryPermission)
            {
                case "500402":
                    //'500402', '查看本人'
                    request.userName = LocalEnv.UserInfo.UserName;
                    break;
                case "500403":
                    request.roletype = "0010";
                    request.userName = LocalEnv.UserInfo.UserName;
                    //'500403', '查看做市'
                    break;
                case "500404":
                    request.roletype = "0001";
                    request.userName = LocalEnv.UserInfo.UserName;
                    //'500404', '查看销交'
                    break;
                case "500405":
                    //'500405', '查看所有'
                    //'500401', '无'
                    break;
            }

            request.pageNum = EMSIndex;
            request.pageSize = PageMaxCount;
            //状态,多个逗号分隔。       "0"(待处理) 1"(已处理)"2”(已成交) "3"(已忽略) "4"("己 REF)
            request.status = "2,4";
            

            QueryHistoryIntentionOrderReplyPo reply = await GetHistoryPositionDetailData(request);

            isInit = false;

            if (reply != null)
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    LogHelper.Info($"queryIntentionQuote Result: Return {reply.Datas.Count} Records; TotalRecord {reply.totalSize} Records.");

                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        TotalCount = reply.totalSize;
                        EMSPageControl.RecordCount = TotalCount;
                        EMSPageControl.PageSize = PageMaxCount;
                        if (TotalCount > PageMaxCount)
                        {
                            EMSPageVisible = "true";
                        }
                        else
                        {
                            EMSPageVisible = "false";
                        }
                    }));

                    UpdateQueryData(reply);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadMaskVisible = Visibility.Collapsed;
                    });
                }
            }
        }

        public async Task<QueryHistoryIntentionOrderReplyPo> GetHistoryPositionDetailData(QueryHistoryIntentionOrderReq req)
        {
            QueryHistoryIntentionOrderReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = TableHandler.Instance.QueryEmsOrderHist(req);
            });

            return reply;
        }

        private void UpdateQueryData(QueryHistoryIntentionOrderReplyPo reply)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (HistoryIntentionOrderReport obj in reply.Datas)
                {
                        UpdateInnerOrder(obj);
                }

                LoadMaskVisible = Visibility.Collapsed;

                this.Mv.View.Refresh();
            }));
        }

        /// <summary>
        /// 更新界面
        /// </summary>
        /// <param name="er"></param>
        private void UpdateInnerOrder(HistoryIntentionOrderReport er)
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            InnerOrderData rf = this.InnerOrders.FirstOrDefault(o => o.IntentionID == er.intentionId);
            if (rf != null)
            {
                return;
            }

            //过滤掉撤销类型为空的记录
            if (string.IsNullOrEmpty(er.reftype))
            {
                return;
            }

            InnerOrderData findData = new InnerOrderData();
            findData.OrderID = er.orderId;
            findData.SecurityID = er.securityId;
            findData.IsSpreadOrBasis = CommonUtils.IsSpreadOrBasic(er.securityId);
            findData.Symbol = er.symbol;

            if (er.securityId != "99999")
            {
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityId);
                if (bondsInfoPo != null)
                {
                    findData.Symbol = bondsInfoPo.Short_Name;
                    findData.RemainingTime = bondsInfoPo.RemaingingTime;
                    findData.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                    findData.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    findData.BondSubType = bondsInfoPo.Bond_Subtype;
                    findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    findData.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }
            }

            findData.Side = er.side;

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
                if (er.side == "1")
                {
                    findData.Price_InnerBid = er.price;
                }
                else if (er.side == "2")
                {
                    findData.Price_InnerOfr = er.price;
                }
            }
            findData.NetPrice = er.cleanPrice;
            findData.Qty = er.qty?.ConvertToLong();
            findData.Qty = findData.Qty / 100;
            //findData.CopyInfo = GetCopyInfo(findData.Qty.ToString(), er);
            findData.PartySubId = er.userId;
            findData.PartySubName = er.closeBy;
            findData.UpdateTime = er.updateTime;
            findData.CreateTime = er.createTime;
            findData.WholeFlag = er.wholeflag;
            findData.IsMe = er.closeBy == LocalEnv.UserInfo.UserName;
            findData.IntentionID = er.intentionId;
            findData.FlashCount = 0;

            findData.Info1 = er.inf1;
            findData.Text = er.text;
            findData.Sign = er.sign;
            findData.Sign1 = er.sign1;
            findData.Sign2 = er.sign2;
            findData.Sign3 = er.sign3;
            findData.InstitutionID = er.institutionId;
            findData.InstitutionName = er.institutionName;
            findData.TraderID = er.traderId;
            findData.TraderName = er.traderName;
            findData.SettleType = er.settlType;

            findData.Status = er.status;
            findData.vipCode = er.vipCode;
            findData.orgType = er.orgType;
            findData.ifSubBond = er.ifSubBond;
            findData.validUntilTime = er.validuntitime;
            findData.RefType = er.reftype;

            findData.Client = GenClient(findData.SignDic, er);

            findData.Rating = GenRating(findData.SignDic1, findData.SignDic3, findData.SecurityID); ;
            findData.BondCategory = GenBondCategory(findData.SignDic1);
            findData.DQType = GenDQType(findData.SignDic, findData.IsMe);

            try
            {
                if (CdccHandler.Instance.CcdcDic.ContainsKey(findData.SecurityID))
                {
                    if (!string.IsNullOrEmpty(CdccHandler.Instance.CcdcDic[findData.SecurityID].CCDCValuation))
                    {
                        findData.CCDCValuation = CdccHandler.Instance.CcdcDic[findData.SecurityID].CCDCValuation.ConvertToDecimal();
                        findData.CCDCValuation_Date = CdccHandler.Instance.CcdcDic[findData.SecurityID].CCDCValuaDate;
                        findData.ShowLastWorkDay = CdccHandler.Instance.CcdcDic[findData.SecurityID].ShowLastWorkDay;

                        if (findData.CCDCValuation.HasValue && findData.Price.HasValue)
                        {
                            if (findData.Side == "1")
                                findData.CCDCValuation_BP = (findData.Price - findData.CCDCValuation) * 100;
                            else if (findData.Side == "2")
                                findData.CCDCValuation_BP = (findData.CCDCValuation - findData.Price) * 100;
                        }
                    }
                }

                if (er.securityId != "99999")
                {
                    CdccHandler.Instance.SubscribeCCDC(findData.SecurityID, "#", OnCCDCEvent);
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }


            lock (_dataLock)
            {
                InnerOrders.Insert(0, findData);
            }
            //});
            //await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
        }
        
        private string GenClient(Dictionary<string, string> dic, HistoryIntentionOrderReport er)
        {
            string client = string.Empty;
            if (type1_flag)
            {
                if (dic != null)
                {
                    if (dic.ContainsKey(Utils.Constants.ORGTYPE))
                    {
                        if (!string.IsNullOrEmpty(er.orgType))
                        {
                            if (DictCrmInst.ContainsKey(er.orgType))
                                client = DictCrmInst[er.orgType];
                                // client = Utils.CommonUtils.GetTagNameByCode(dic[Utils.Constants.ORGTYPE], Utils.Constants.ORGTYPE);
                        }
                    }

                    if (!string.IsNullOrEmpty(er.vipCode))
                    {
                        if (er.vipCode == "0001")
                        {
                            client = client + "(vip)";
                        }
                    }
                }
            }
            else if (type2_flag)
            {
                client = er.institutionName;
            }
            else if (type3_flag)
            {
                if (!string.IsNullOrEmpty(er.institutionName)
                    || !string.IsNullOrEmpty(er.traderName))
                {
                    client = $"{er.institutionName}-{er.traderName}";
                }
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

        /// <summary>
        /// 获取客户类型列表
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetCrmInstDict()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Dictionary<string, STC.Common.Model.Table.QuoteTag> dic = BaseHandler.Instance.GetQuoteTagDic();

            if (dic != null && dic.ContainsKey("ORG_TYPE") && dic["ORG_TYPE"] != null && dic["ORG_TYPE"].childTags != null && dic["ORG_TYPE"].childTags.Count > 0)
            {
                foreach (STC.Common.Model.Table.ChildTag ct in dic["ORG_TYPE"].childTags)
                {
                    result.Add(ct.childTagValue, ct.childTagName.Trim());
                }
            }

            return result;
        }

        #endregion

        #region 设置过滤条件
        private void SetMvFilter()
        {
            this.Mv.View.Filter = x =>
            {
                InnerOrderData data = x as InnerOrderData;

                if (!string.IsNullOrEmpty(this.DisplayMode) && data.SecurityType != this.DisplayMode)
                    return false;

                //撤销类型
                if (CancelOrderType.Count > 0)
                {
                    if (!CancelOrderType.Contains(data.RefType))
                        return false;
                }

                if (string.IsNullOrEmpty(this.SearchText) == false
                    && (data.SecurityID == null || !data.SecurityID.Contains(this.SearchText))
                    && (data.Symbol == null || !data.Symbol.Contains(this.SearchText))
                    && (data.TraderName == null || !data.TraderName.Contains(this.SearchText))
                    && (data.InstitutionName == null || !data.InstitutionName.Contains(this.SearchText)))
                    return false;

                if (string.IsNullOrEmpty(this.Best_SearchText) == false
                    && (data.SecurityID == null || !data.SecurityID.Contains(this.Best_SearchText))
                    && (data.Symbol == null || !data.Symbol.Contains(this.Best_SearchText))
                    && (data.TraderName == null || !data.TraderName.Contains(this.Best_SearchText))
                    && (data.InstitutionName == null || !data.InstitutionName.Contains(this.Best_SearchText)))
                    return false;

                if (IsMe == true && data.IsMe == false)
                {
                    return false;
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
                return result;
            };
        }

        private void CheckSign(InnerOrderData data, string key, string value, ref bool result, Dictionary<string, List<string>> focusTagDic, string type = null)
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

        #region 计算净价 更新中债期货

        /// <summary>
        /// 更新中债期货
        /// </summary>
        /// <param name="obj"></param>
        private void OnCCDCEvent(CCDCValuaModel obj)
        {
            string secid = obj.CCDCValuationShow;
            if (!string.IsNullOrEmpty(obj.CCDCValuation))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    List<InnerOrderData> lstdr = InnerOrders.ToList().FindAll(o => o.SecurityID == secid);
                    foreach (InnerOrderData dr in lstdr)
                    {
                        if (dr.CCDCValuation == null)
                        {
                            InnerOrderData m2 = InnerOrders.FirstOrDefault(o => o.OrderID == dr.OrderID);

                            m2.CCDCValuation = obj.CCDCValuation.ConvertToDecimal();
                            m2.CCDCValuation_Date = obj.CCDCValuaDate;
                            m2.ShowLastWorkDay = obj.ShowLastWorkDay;


                            if (m2.CCDCValuation.HasValue && m2.Price.HasValue)
                            {
                                if (m2.Side == "1")
                                    m2.CCDCValuation_BP = (m2.Price - m2.CCDCValuation) * 100;
                                else if (m2.Side == "2")
                                    m2.CCDCValuation_BP = (m2.CCDCValuation - m2.Price) * 100;
                            }
                        }
                    }
                }));
            }
        }

        private void OnCheckAll()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (InnerOrderData addedItem in InnerOrders)
                {
                    if (addedItem.IsMe)
                        addedItem.IsChecked = IsAllChecked;

                    if (addedItem.IsChecked)
                    {
                        if (!addedItem.NetPrice.HasValue)
                            AddQueue(new NetPriceValuaModel() { CalType = "4", Yield = addedItem.Price, IntentionID = addedItem.IntentionID, SecurityID = addedItem.SecurityID, SettlType = addedItem.SettleType });
                    }
                }
            });
        }


        /// <summary>
        /// 添加到队列
        /// </summary>
        /// <param name="obj"></param>
        public void AddQueue(NetPriceValuaModel obj)
        {
            lock (_queueLock)
            {
                _ProcessNetPriceThreadQueue.Enqueue(obj);
            }

        }

        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        /// <param name="security"></param>
        private void UpdateNetPriceData(NetPriceValuaModel rs)
        {
            decimal d = 0;
            if (!rs.Yield.HasValue)
                return;

            if (!decimal.TryParse(rs.Yield.Value.ToString(), out d))
                return;

            decimal val = rs.Yield.Value / 100;

            Task.Run(() =>
            {
                try
                {
                    CalReq requst = new CalReq();
                    requst.SecurityID = rs.SecurityID;
                    requst.CalType = rs.CalType;
                    requst.SettlType = rs.SettlType;
                    requst.Value = val.ToString("F4");

                    if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                        requst.CalPath = LocalEnv.ConfInfo.CalType;

                    CalResult result = CalHandler.Instance.CalBond(requst);
                    if (requst != null)
                    {
                        if (result != null && result.Data != null && result.IsSuccess())
                        {
                            result.ReqData = string.Format("{0}-{1}-{2}", requst.CalType, requst.SettlType, requst.Value);
                            if (rs.CalType.Equals("4"))
                            {
                                string netPrice = result.Data.CleanPrice.ToString("F4");
                                InnerOrderData iod = InnerOrders.FirstOrDefault( k => k.IntentionID == rs.IntentionID);
                                if (iod != null)
                                {
                                    iod.NetPrice = result.Data.CleanPrice;
                                }
                            }
                            else if (rs.CalType.Equals("2"))//净价
                            {
                                string customPrice = (result.Data.Yield * 100).ToString("F4");
                            }
                        }
                    }
                    else
                    {
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
                finally
                {

                }
            });
        }
        #endregion

        #region Command
        /// <summary>
        /// 刷新
        /// </summary>
        private void OnRefreshCommand()
        {
            BeginQuery();

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.QueryHistoryIntentionOrder();
            }));
        }

        /// <summary>
        /// 查询
        /// </summary>
        private void OnSearchUseDateCommand()
        {
            BeginQuery();

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.QueryHistoryIntentionOrder();
            }));
        }

        /// <summary>
        /// 新增需求
        /// </summary>
        private void OnNewRequireCommand()
        {
            string securityID = "Tradebook";
            if (this.SelectOrder != null)
            {
                securityID = this.SelectOrder.SecurityID;
            }

            SendRequireView sendRequireView = new SendRequireView(securityID, true);
            sendRequireView.Show();
        }

        /// <summary>
        /// 批量识别
        /// </summary>
        private void OnOpenRecognizeWinCommand()
        {
            DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(null);
        }

        /// <summary>
        /// 单个恢复
        /// </summary>
        /// <param name="obj"></param>
        public void OnResumeCommand(object inparam)
        {
            if (inparam == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string errorMsg = string.Empty;
                InnerOrderData rs= InnerOrders.FirstOrDefault(o => o.IntentionID == (string)inparam);
                if (rs != null)
                {
                    PriceMatchParam priceMatchParam = new PriceMatchParam();
                    priceMatchParam.Side = rs.Side;
                    priceMatchParam.Symbol = rs.SecurityID;
                    priceMatchParam.SymbolName = rs.Symbol;
                    priceMatchParam.WholeFlag = rs.WholeFlag;

                    if (rs.Qty.HasValue)
                        priceMatchParam.CustomFirmAmount = rs.Qty.ToString() ?? "";

                    if (rs.Price.HasValue)
                        priceMatchParam.Price = rs.Price.Value.ToString("F4");

                    priceMatchParam.SettlType = rs.SettleType;
                    priceMatchParam.NonfirmTime = string.Empty; // nonfirmTime;
                    priceMatchParam.OperationType = "1";
                    priceMatchParam.IsFirm = false;
                    priceMatchParam.SendOrderType = "OCO";
                    priceMatchParam.IntentionID = rs.IntentionID;
                    priceMatchParam.Text = rs.Text;
                    if (!WindowManager.Instance.IsExist($"EditOrder-{priceMatchParam.IntentionID}"))
                    {

                        priceMatchParam.Info1 = rs.Info1;
                        priceMatchParam.Info2 = rs.Info1;
                        priceMatchParam.PriceCondition = rs.Info1;
                        priceMatchParam.InstitutionID = rs.InstitutionID;
                        priceMatchParam.InstitutionName = rs.InstitutionName;
                        priceMatchParam.TraderID = rs.TraderID;
                        priceMatchParam.TraderName = rs.TraderName;

                        //如果是模糊需求
                        if (rs.SecurityID == "99999")
                        {
                            if (!string.IsNullOrEmpty(rs.Info1))
                            {
                                if (JsonSplit.IsJson(rs.Info1))
                                {
                                    OrderTimeModel otm = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderTimeModel>(rs.Info1);
                                    if (!string.IsNullOrEmpty(otm.QuoteType))
                                    {
                                        if (otm.QuoteType == "virtual")
                                        {
                                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "EditFuzzyOrder", RequestID = $"EditFuzzyOrder-{priceMatchParam.OrderId}", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(priceMatchParam) });
                                            return;
                                        }
                                    }
                                }
                            }
                        
                        }

                        SendRequireView sendRequireView = new SendRequireView(priceMatchParam);
                        WindowManager.Instance.OpenOnlyOneWindow($"EditOrder-{priceMatchParam.IntentionID}", sendRequireView);
                    }
                }
            }));
        }

        /// <summary>
        /// 批量恢复
        /// </summary>
        public async void OnBatchResumeCommand()
        {
            List<InnerOrderData> tmp = new List<InnerOrderData>(InnerOrders.Where(p => p.IsChecked));
            if (tmp.Count > 0)
            {
                bool res = CustomMessageBox.Question($"是否批量恢复 {tmp.Count} 个已选订单？", "信息");
                if (res == true)
                {
                    StringBuilder error = null;
                    error = await SubBatchResumeOrders(tmp);
                    if (error != null && error.Length > 0)
                    {
                        CustomMessageBox.Error($"批量恢复订单失败, 原因:\r\n{error.ToString()}");
                    }
                }
            }
            else
            {
                List<InnerOrderData> tmpsel = new List<InnerOrderData>(InnerOrders.Where(p => p.IsSelected && p.IsMe));
                if (tmpsel.Count > 1)
                {
                    bool res = CustomMessageBox.Question($"是否批量恢复设为高亮的本人的 {tmpsel.Count} 个订单？", "信息");
                    if (res == true)
                    {
                        StringBuilder error = null;
                        error = await SubBatchResumeOrders(tmpsel);
                        if (error != null && error.Length > 0)
                        {
                            CustomMessageBox.Error($"批量恢复订单失败, 原因:\r\n{error.ToString()}");
                        }
                    }
                }
                else
                {
                    CustomMessageBox.Warning("请选择需要恢复的订单", "信息");
                }
            }
        }

        /// <summary>
        /// 批量恢复
        /// </summary>
        /// <param name="tmp"></param>
        /// <returns></returns>
        private async Task<StringBuilder> SubBatchResumeOrders(List<InnerOrderData> tmp)
        {
            StringBuilder error = new StringBuilder();

            await Task.Run(() =>
            {
                List<NewOrderSingle> resumeOrdersList = new List<NewOrderSingle>();
                foreach (InnerOrderData rf in tmp)
                {
                    NewOrderSingle rs = GetNewOrderSingle(rf);
                    resumeOrdersList.Add(rs);
                }

                if (resumeOrdersList != null)
                {
                    int count = resumeOrdersList.Count;
                    Task[] lstTask = new Task[count];

                    ConcurrentDictionary<string, RecognizeIntentionQuoteReplyPo> dic = new ConcurrentDictionary<string, RecognizeIntentionQuoteReplyPo>();
                    for (int i = 0; i < count; i++)
                    {

                        NewOrderSingle cancelPo = resumeOrdersList[i];
                        lstTask[i] = new Task(() =>
                        {
                            cancelPo.HandlInst = "2";
                            RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(cancelPo);
                            if (result != null && !string.IsNullOrEmpty(result.Msg))
                            {
                                dic[cancelPo.ClOrdID] = result;
                            }
                            else
                            {
                                RecognizeIntentionQuoteReplyPo res = new RecognizeIntentionQuoteReplyPo();
                                res.Code = "-1";
                                res.Msg = "服务器接收数据失败！";
                                dic[cancelPo.ClOrdID] = res;
                            }
                        });

                        lstTask[i].Start();
                    }

                    Task.WaitAll(lstTask);

                    foreach (KeyValuePair<string, RecognizeIntentionQuoteReplyPo> pair in dic)
                    {
                        string orderID = pair.Key;
                        RecognizeIntentionQuoteReplyPo reply = pair.Value;
                        if (reply != null)
                        {
                            if (reply.IsSuccess() == false)
                            {
                                error.AppendLine($"订单号 {orderID}:{reply.Msg}");
                            }
                        }
                    }

                }
            });

            return error;
        }

        #endregion

        #region Funcitons
        /// <summary>
        /// 生成订单报文
        /// </summary>
        /// <returns></returns>
        private NewOrderSingle GetNewOrderSingle(InnerOrderData rs)
        {
            NewOrderSingle sendPricePo = new NewOrderSingle();

            sendPricePo.AlgoName = "CROSS";
            sendPricePo.VenueTypeGW = "INNER";
            sendPricePo.Venues = "MANUAL";

            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            sendPricePo.Info1 = rs.Info1;

            if (JsonSplit.IsJson(rs.Info1))
            {
                OrderTimeModel otm = JsonConvert.DeserializeObject<OrderTimeModel>(rs.Info1);
                PriceCondition pc = new PriceCondition();
                pc.MorethanMarketBPCheck = string.IsNullOrEmpty(otm.MorethanMarketBPCheck) ? null : otm.MorethanMarketBPCheck;
                pc.MorethanMarketBP = string.IsNullOrEmpty(otm.MorethanMarketBP) ? null : otm.MorethanMarketBP;
                sendPricePo.PriceCondition = JsonConvert.SerializeObject(pc);

                sendPricePo.IfSubBond = otm.IF_SUB_BOND ?? null;
                sendPricePo.OrgType = otm.ORG_TYPE ?? null;
                sendPricePo.VipCode = otm.VIP ?? null;
                //计算最新的ValidUntilTime
                string validTime = GetValidUntilTime(otm.ValidTimeType);
                if (!string.IsNullOrEmpty(validTime))
                {
                    sendPricePo.ValidUntilTime = validTime;
                }
            }

            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(rs.ifSubBond))
                dic.Add("IF_SUB_BOND", rs.ifSubBond);
            if (!string.IsNullOrEmpty(rs.vipCode))
                dic.Add("VIP", rs.vipCode);
            if (!string.IsNullOrEmpty(rs.orgType))
                dic.Add("ORG_TYPE", rs.orgType);

            sendPricePo.Info2 = JsonConvert.SerializeObject(dic);

            BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(rs.SecurityID);
            if (bondsInfoPo != null)
            {
                sendPricePo.MarketIndicator = bondsInfoPo.MarketIndicator;
            }
            else
                sendPricePo.MarketIndicator = "4";

            sendPricePo.OrdType = "2";

            if (rs.Qty.HasValue)
                sendPricePo.OrderQty = rs.Qty.ToString() ?? "";

            if (rs.Price.HasValue)
                sendPricePo.Price = rs.Price.Value.ToString("F4");

            if (sendPricePo.MarketIndicator == "4" && rs.NetPrice.HasValue)
                sendPricePo.NetPrice = rs.NetPrice.Value.ToString("F4");
            else
                sendPricePo.NetPrice = "100";

            sendPricePo.SecurityID = rs.SecurityID;
            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            sendPricePo.SettlType = rs.SettleType;
            sendPricePo.Side = rs.Side;

            sendPricePo.Terminal = string.Format("STClient_{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = "0";
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;
            sendPricePo.Name = LocalEnv.UserInfo.UserName;
            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = "0";

            sendPricePo.CrossVenues = "";
            sendPricePo.VenuesSelectF = "";

            sendPricePo.WholeFlag = rs.WholeFlag;
            sendPricePo.HandlInst = "1";

            sendPricePo.InstitutionID = rs.InstitutionID;
            sendPricePo.InstitutionName = rs.InstitutionName;
            sendPricePo.TraderID = rs.TraderID;
            sendPricePo.TraderName = rs.TraderName;

            //sendPricePo.Text = rs.Text;
            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();

            return sendPricePo;
        }

        /// <summary>
        /// 计算过期时间
        /// </summary>
        /// <param name="ValidTimeType"></param>
        /// <returns></returns>
        private string GetValidUntilTime(string ValidTimeType)
        {
            string IocOutTime = SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut;
            string ValidUntilTime = string.Empty;
            string ValidTimeHour = string.Empty;
            string ValidTimeMinute = string.Empty;
            string ValidTimeSecond = string.Empty;

            bool DefaultValidTimeQuotationCheck = false;
            bool NoLimitValidTimeQuotationCheck = false;
            //报价选择过期类型  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无"3|"
            if (ValidTimeType.StartsWith("0|"))
            {
                DefaultValidTimeQuotationCheck = true;
                // 默认时间
                int hour = DateTimeEx.ServerTimeNow.Hour;
                int min = DateTimeEx.ServerTimeNow.Minute;
                if (hour > 0 && hour < 6)
                {
                    ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (hour >= 6 && hour < 12)
                {
                    ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            else if (ValidTimeType.StartsWith("1|"))
            {
                // 时长
                // ValidMinute  ValidSecond
                string s = ValidTimeType.Split('|')[1];
                if (s.Contains(":"))
                {
                    ValidTimeMinute = s.Split(':')[0];
                    ValidTimeSecond = s.Split(':')[1];
                }

                int totalsecond = 0;
                if (!string.IsNullOrEmpty(ValidTimeMinute))
                {
                    totalsecond = int.Parse(ValidTimeMinute) * 60;
                }

                if (!string.IsNullOrEmpty(ValidTimeSecond))
                {
                    totalsecond += int.Parse(ValidTimeSecond);
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

            
            else if (ValidTimeType.StartsWith("2|"))
            {
                // 时点
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                string s = ValidTimeType.Split('|')[1];
                if (s.Contains(":"))
                {
                    ValidTimeHour = s.Split(':')[0];
                    ValidTimeMinute = s.Split(':')[1];
                    ValidTimeSecond = s.Split(':')[2];
                }

                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(ValidTimeHour))
                {
                    hour = int.Parse(ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(ValidTimeMinute))
                {
                    min = int.Parse(ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(ValidTimeSecond))
                {
                    sec = int.Parse(ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                ValidUntilTime = tmpdt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else if (ValidTimeType.StartsWith("3|"))
            {
                NoLimitValidTimeQuotationCheck = true;
                // 无
                // 不填写
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
                    if (!NoLimitValidTimeQuotationCheck)
                    {
                        ValidUntilTime = InnerPlaceOrderAdapter.Instance.GetMinDate(selfUntilTime, validUntiTimeSetting, DefaultValidTimeQuotationCheck);
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
                    if (!NoLimitValidTimeQuotationCheck)
                    {
                        // 默认
                        if (DefaultValidTimeQuotationCheck)
                        {
                            ValidUntilTime = InnerPlaceOrderAdapter.Instance.GetMinDate(selfUntilTime, "", true);
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

        #region 初始化参数设置
        /// <summary>
        /// 撤销类型过滤
        /// </summary>
        private void InitCancelOrderType()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic["1"] = "手动撤销";
                dic["2"] = "成交撤销";
                dic["3"] = "系统撤销";
                myWindow.cmbcancelordertype.ItemsSource = dic;

                this.DealTimeBegin = this.DealTimeEnd = DateTime.Now;
            });

        }
        #endregion

        #region Dispose
        ~InnerHistoryRequireViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            base.Dispose();
            isInit = false;
        }
        #endregion

        #region 逐笔
        private int _rowCount;
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

        private int _bidRowCount;
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

        private int _ofrRowCount;
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
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                RowCount = Mv.View.Cast<InnerOrderData>().Count();
                BidRowCount = Mv.View.Cast<InnerOrderData>().Count(p => p.Side == "1");
                OfrRowCount = Mv.View.Cast<InnerOrderData>().Count(p => p.Side == "2");
            }));
        }
        #endregion
    }

    public class NetPriceValuaModel
    {
        /// <summary>
        /// 需求ID
        /// </summary>
        public string IntentionID { get; set; }

        /// <summary>
        /// 债券ID
        /// </summary>
        public string SecurityID { get; set; }

        /// <summary>
        /// 清算速度
        /// </summary>
        public string SettlType { get; set; }

        /// <summary>
        /// 计算类型  2 NetPrice->Yield  4 Yield->NetPrice
        /// </summary>
        public string CalType { get; set; }

        /// <summary>
        /// 收益率
        /// </summary>
        public decimal ?Yield { get; set; }

        /// <summary>
        /// 净价
        /// </summary>
        public decimal CleanPrice { get; set; }

    }
}
