using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Position;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Hedge;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.Frame.Common;
using STC.Position.Plugin.Model;
using STC.Position.Plugin.View;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
namespace STC.Position.Plugin.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region 构造
        /// <summary>
        /// 持仓管理模块权限
        /// </summary>
        private readonly string MODULEID = "4012";

        /// <summary>
        /// 该用户具有执行手工结转权限
        /// </summary>
        private readonly string MODULEACTIONID1 = "401201";

        /// <summary>
        /// 该用户具有可以查看持仓对账结果，交易流水对账结果
        /// </summary>
        private readonly string MODULEACTIONID2 = "401202";
       
        /// <summary>
        /// 组合和组合ID对照表
        /// </summary>
        private Dictionary<string, string> BookDict = new Dictionary<string, string>();

        /// <summary>
        /// 有权限的组合
        /// </summary>
        private Dictionary<string, string> SelfBookDict = new Dictionary<string, string>();

        /// <summary>
        /// 债券列表
        /// </summary>
        private Dictionary<string, ExecutionReport> SubscribeSecurityidDict = new Dictionary<string, ExecutionReport>();

        /// <summary>
        /// 字典锁
        /// </summary>
        private object LockDict = new object();

        private bool IsSubscribe = false;

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
                    //FilterOrderData();

                    this.Mv.View.Refresh();

                    //更新持仓盈亏
                    ReCalcPosition();
                }
            }
        }

        #region 持仓组合构造
        /// <summary>
        /// 类型
        /// </summary>
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
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (PostionBookData r in LstBook)
                        {
                            r.IsChecked = value;
                        }

                        RefreshNodeKeyWord();
                    }));

                    this.OnPropertyChanged(nameof(IsAllChecked));
                }
            }
        }


        /// <summary>
        /// 持仓组合
        /// </summary>
        private ObservableCollection<PostionBookData> lstBook = new ObservableCollection<PostionBookData>();
        public ObservableCollection<PostionBookData> LstBook
        {
            get
            {
                return lstBook;
            }

            set
            {
                lstBook = value;
                this.OnPropertyChanged(nameof(LstBook));
            }
        }

        private ObservableCollection<FilterListBoxItemData> _futureTypeList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "利率期货", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "TS", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "TF", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "T", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> FutureTypeList { get { return _futureTypeList; } }

        private ObservableCollection<FilterListBoxItemData> _marketIndicatorList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "债券", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "期货", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> MarketIndicatorList { get { return _marketIndicatorList; } }

        private ObservableCollection<FilterListBoxItemData> _interestRateDebtList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "利率债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "国债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "政策性金融债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "地方政府债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "央票", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> InterestRateDebtList { get { return _interestRateDebtList; } }

        private ObservableCollection<FilterListBoxItemData> _creditBondList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "信用债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "政府支持机构债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "公司债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "金融债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "NCD", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "ABS", itemIsSelected = false, Itemmargin="50 0 0 0"},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> CreditBondList { get { return _creditBondList; } }


        #endregion

        #region 持仓明细构造
        private object lockObj = new object();
        /// <summary>
        /// 持仓明细构造
        /// </summary>
        private ObservableCollection<PositionDetailData> fullPositionDetail = new ObservableCollection<PositionDetailData>();
        public ObservableCollection<PositionDetailData> FullPositionDetail
        {
            get
            {
                return fullPositionDetail;
            }
            set
            {
                fullPositionDetail = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<PositionDetailData> fullPositionTotal = new ObservableCollection<PositionDetailData>();

        public ObservableCollection<PositionDetailData> FullPositionTotal
        {
            get
            {
                return fullPositionTotal;
            }
            set
            {
                fullPositionTotal = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

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


        private List<string> keyword = new List<string>();
        public List<string> Keyword
        {
            get
            {
                return keyword;
            }
            set
            {
                keyword = value;
                this.OnPropertyChanged(nameof(Keyword));
            }
        }


        private PositionDetailData _selecteditem_dgpositiondetail;

        public PositionDetailData SelectedItem_dgPositionDetail
        {
            get
            {
                return _selecteditem_dgpositiondetail;
            }
            set
            {
                _selecteditem_dgpositiondetail = value;
                this.OnPropertyChanged();
            }
        }

        private string _PositionDetailTag = "1";
        /// <summary>
        /// 持仓明细 Tag
        /// </summary>
        public string PositionDetailTag
        {
            get { return _PositionDetailTag; }
            set
            {
                if (_PositionDetailTag != value)
                {
                    _PositionDetailTag = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 是否显示跨日结转按钮
        /// </summary>
        private Visibility _HistoryProcessingVis = Visibility.Collapsed;

        public Visibility HistoryProcessingVis
        {
            get
            {
                return _HistoryProcessingVis;
            }
            set
            {

                _HistoryProcessingVis = value;
                this.OnPropertyChanged();

            }
        }
        /// <summary>
        /// 是否显示对账按钮
        /// </summary>
        private Visibility _PositionCheckVis = Visibility.Collapsed;

        public Visibility PositionCheckVis
        {
            get
            {
                return _PositionCheckVis;
            }
            set
            {

                _PositionCheckVis = value;
                this.OnPropertyChanged();

            }
        }

        #region 风险敞口
        private string _RiskExposureTag = "4";
        /// <summary>
        /// 风险敞口 Tag
        /// </summary>
        public string RiskExposureTag
        {
            get { return _RiskExposureTag; }
            set
            {
                if (_RiskExposureTag != value)
                {
                    _RiskExposureTag = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _RiskExposureTagVisible = Visibility.Collapsed;
        /// <summary>
        /// 风险敞口 是否可见
        /// </summary>
        public Visibility RiskExposureTagVisible
        {
            get { return _RiskExposureTagVisible; }
            set
            {
                if (_RiskExposureTagVisible != value)
                {
                    _RiskExposureTagVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int _RiskExposureTabItemHeight = 0;
        /// <summary>
        /// 风险敞口 TabItem 高度
        /// </summary>
        public int RiskExposureTabItemHeight
        {
            get { return _RiskExposureTabItemHeight; }
            set
            {
                if (_RiskExposureTabItemHeight != value)
                {
                    _RiskExposureTabItemHeight = value;
                    this.OnPropertyChanged();
                }
            }
        }


        public ICommand PositionDetailCommand { get { return new ComRelayCommand(OnPositionDetailCommand); } }

        public ICommand RiskExposureCommand { get { return new ComRelayCommand(OnRiskExposureCommand); } }

        #endregion

        /// <summary>
        /// 成交手工录入
        /// </summary>
        public ICommand ManualInputCommand { get { return new ComRelayCommand(OnManualInputClick); } }

        public ICommand TestCommand { get { return new ComRelayCommand(OnTestCommandClick); } }

        public ICommand ManualProcessingPositionCommand { get { return new ComRelayCommand(OnManualProcessingPositionCommand); } }

        public ICommand QueryProcessingRecordHistCommand { get { return new ComRelayCommand(OnQueryProcessingRecordHistCommand); } }

        private bool isInit = false;

        #region Page

        private int _SelectedPageIndex = 0;
        public int SelectedPageIndex
        {
            get { return _SelectedPageIndex; }
            set
            {
                if (_SelectedPageIndex != value)
                {
                    _SelectedPageIndex = value;

                    TreeVisibility = value == 0 && SubSelectedPageIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility treevisibility = Visibility.Visible;
        public Visibility TreeVisibility
        {
            get { return treevisibility; }
            set
            {
                if (treevisibility != value)
                {
                    treevisibility = value;
                    this.OnPropertyChanged();
                }
            }
        }



        private int _SubSelectedPageIndex = 0;
        public int SubSelectedPageIndex
        {
            get { return _SubSelectedPageIndex; }
            set
            {
                if (_SubSelectedPageIndex != value)
                {
                    _SubSelectedPageIndex = value;

                    //风险敞口
                    if (SubSelectedPageIndex == 1)
                    {
                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                        {
                            SendMsgToChart();
                        }));
                    }

                    this.OnPropertyChanged();
                }
            }
        }
        #endregion

        #endregion

        #region 树状分组数据源

        private ObservableCollection<TreeNodeModel> _MultiTreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
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


        private string _SelectedItem = "";
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (_SelectedItem != value)
                {
                    _SelectedItem = value;
                    SingleLoadMaskVisible = Visibility.Visible;
                    RefreshNodeKeyWord();
                    this.OnPropertyChanged();
                }
            }
        }

        private object updlock = new object();

        private Visibility loadmaskvisible_Single = Visibility.Visible;

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

        private string _TotalCount4MyOrders = "";
        public string TotalCount4MyOrders
        {
            get { return _TotalCount4MyOrders; }
            set
            {
                if (_TotalCount4MyOrders != value)
                {
                    _TotalCount4MyOrders = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        public MainViewModel()
        {
            
        }

        ~MainViewModel()
        {

        }

        public void TryDispose()
        {
            IsSubscribe = false;
            isInit = false;
            PositionHandler.Instance.UnSubscribePositionDetail(OnPostionDetailCallBack);

            string s = string.Empty;
            while (!securityidQueue.IsCompleted && securityidQueue.Count > 0)
            {
                securityidQueue.TryTake(out s);
            }

            List<PositionDetailModel> tmplist = new List<PositionDetailModel>();
            while (!logQueue.IsCompleted && logQueue.Count > 0)
            {
                logQueue.TryTake(out tmplist);
            }

            cancellationTokenSource.Cancel();

            ClearTotal();
            FullPositionDetail.Clear();
            this.Dispose();
            base.Dispose();
        }

        private void ClearTotal()
        {
            PositionDetailData m2 = FullPositionTotal.FirstOrDefault(o => o.bookid == "0");
            if (m2 != null)
            {
                //ondayprofitandloss 当日盈亏,closingprofitandloss 平仓盈亏,positionprofitandloss 持仓盈亏, positionmarket 持仓市值
                m2.totalprofitandloss = decimal.Zero;
                m2.closingprofitandloss = decimal.Zero;
                m2.positionprofitandloss = decimal.Zero;
                m2.positionmarket = decimal.Zero;
                m2.dv01 = decimal.Zero;
            }
        }

        public void UnRegistEvent()
        {
            IsSubscribe = false;
            isInit = false;
            PositionHandler.Instance.UnSubscribePositionDetail(OnPostionDetailCallBack);
        }


        private List<TTbookBook> DictTTbookBook = new List<TTbookBook>();

        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static BlockingCollection<List<PositionDetailModel>> logQueue = new BlockingCollection<List<PositionDetailModel>>();

        private static CancellationTokenSource cancellationTokenSource;

        private static BlockingCollection<string> securityidQueue = new BlockingCollection<string>();

        /// <summary>
        /// 初始化视图
        /// </summary>
        public void InitView()
        {
            SetPermission();
            cancellationTokenSource = new CancellationTokenSource();
            //查询所有组合名称
            GetBookUser();
            DictTTbookBook = STC.Common.Handler.Admin.AdminHandler.Instance.GetTTbookBook();

            //string s = CommonUtils.SerializeObject(DicTTbook);
            //FileHelper.WriteFullText("D:\\DicTTbook.txt", s);

            this.Mv.Source = FullPositionDetail;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            this.Mv.Filter += Mv_Filter;
            this.Mv.View.Refresh();
            this.Mv.SortDescriptions.Add(new SortDescription("AlwaysTop", ListSortDirection.Descending));
            this.Mv.SortDescriptions.Add(new SortDescription("bookid", ListSortDirection.Ascending));
            this.Mv.SortDescriptions.Add(new SortDescription("securityid", ListSortDirection.Ascending));
            this.Mv.SortDescriptions.Add(new SortDescription("settlDate", ListSortDirection.Ascending));
            this.Mv.SortDescriptions.Add(new SortDescription("updatetime", ListSortDirection.Descending));

            //添加汇总
            if (!FullPositionTotal.Any(o => o.bookid == "0"))
            {
                FullPositionTotal.Add(new PositionDetailData() { bookid = "0", key = "0#0", bookname = "", securityid = "汇总", AlwaysTop = 1 });
            }

            //持仓敞口
            if (LocalEnv.ConfInfo.IsShowRiskExposure)
            {
                RiskExposureTagVisible = Visibility.Visible;
            }

            BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);

            //设置树数据源
            MultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();

            //开启订阅最优价线程
            Task.Factory.StartNew(() => DoSubscribeSecurityidLastExecutionQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

            //启动订阅处理
            Task.Factory.StartNew(() => ProcessLogQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

            //订阅跨日结转结果
            PositionHandler.Instance.SubscribeProcessingRecordStatus(OnSubscribeLoadtMarketExecutionsStatusCallBack);

            //启动最优价更新持仓
            Task.Factory.StartNew(() => ProcessLogQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
                if (!isInit)
                {
                    isInit = true;
                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                        {
                            // 查询持仓组合
                            GetBookGroup();
                        }));
                    });
                }
            }
        }



        #region 订阅债券最新价

        public async Task<ExecutionReport> GetLastExecutionsItem(string secid)
        {
            ExecutionReport reply = null;
            await Task.Run(() =>
            {
                ExecutionReport lstlastOwnOrder = LastExecutionProcesser.Processer.GetExecutionReportDic(secid);

                if (lstlastOwnOrder != null)
                {
                    reply = lstlastOwnOrder;
                    SubscribeSecurityidDict[secid] = lstlastOwnOrder;
                }
            });

            return reply;
        }

        ////public ExecutionReport GetLastExecutionsItem(string secid)
        ////{
        ////    List<ExecutionReport> lstlastOwnOrder = ExecutionsHandler.Instance.GetLastExecutionBySecurityID(secid);
        ////    ExecutionReport lastOwnOrder = null;
        ////    if (lstlastOwnOrder != null & lstlastOwnOrder.Count>0)
        ////    {
        ////         SubscribeSecurityidDict[secid] = lstlastOwnOrder[0];
        ////    }

        ////    return lastOwnOrder;
        ////}

        private void Instance_OnMarketExecutions(ExecutionReport obj)
        {
            if (SubscribeSecurityidDict.ContainsKey(obj.SecurityID))
            {
                SubscribeSecurityidDict[obj.SecurityID] = obj;
                OnLastExec(obj);
                
            }
        }

        private void Instance_OnOwnExecutions(ExecutionReport obj)
        {
            if (SubscribeSecurityidDict.ContainsKey(obj.SecurityID))
            {
                SubscribeSecurityidDict[obj.SecurityID] = obj;
                OnLastExec(obj);
            }
        }

        /// <summary>
        /// 接收最新成交回调
        /// </summary>
        /// <param name="obj"></param>
        public void OnLastExec(ExecutionReport obj)
        {
            //更新最新成交
            if (string.IsNullOrEmpty(obj.NetPrice))
                return;

            securityidQueue.Add(obj.SecurityID);
        }

        /// <summary>
        /// 更新持仓盈亏
        /// </summary>
        public void ReCalcPosition()
        {
            //平仓盈亏
            decimal total_closingprofitandloss = 0m;
            //当日盈亏
            decimal total_ondayprofitandloss = 0m;
            //持仓盈亏
            decimal total_positionprofitandloss = 0m;
            //positionmarket
            decimal total_positionmarket = 0m;
            // dv01
            decimal total_dv01 = 0m;

            if (FullPositionDetail.Count > 0)
            {
                foreach (PositionDetailData p in this.Mv.View)
                {
                    if (this.Keyword.Count == 0 || (this.keyword.Count > 1 && this.keyword.Contains(p.bookid)))
                    {
                        total_ondayprofitandloss += p.totalprofitandloss;
                        total_closingprofitandloss += p.closingprofitandloss;
                        total_positionprofitandloss += p.positionprofitandloss;
                        total_positionmarket += p.positionmarket;
                        total_dv01 += p.dv01;
                    }
                }

                //更新数据
                PositionDetailData m2 = FullPositionTotal.FirstOrDefault(o => o.bookid == "0");
                if (m2 != null)
                {
                    //ondayprofitandloss 当日盈亏,closingprofitandloss 平仓盈亏,positionprofitandloss 持仓盈亏, positionmarket 持仓市值
                    m2.totalprofitandloss = CommonUtils.ConvertToDecimal(total_ondayprofitandloss.ToString());
                    m2.closingprofitandloss = CommonUtils.ConvertToDecimal(total_closingprofitandloss.ToString());
                    m2.positionprofitandloss = CommonUtils.ConvertToDecimal(total_positionprofitandloss.ToString());
                    m2.positionmarket = CommonUtils.ConvertToDecimal(total_positionmarket.ToString());
                    m2.dv01 = CommonUtils.ConvertToDecimal(total_dv01.ToString());
                }
            }

            string recc  = (Mv.View.Cast<PositionDetailData>().Count() - 1).ToString();
            TotalCount4MyOrders = recc == "-1" ? "--" : recc;
        }

        private void SendMsgToChart()
        {
            if (SubSelectedPageIndex == 0)
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ObservableCollection<PositionDetailData> obslst = new ObservableCollection<PositionDetailData>(Mv.View.Cast<PositionDetailData>().Where(o=>o.bookid != "0").ToList());
                    List<PositionListDataModel> LstKrd = CommonUtils.DeserializeObject<List<PositionListDataModel>>(CommonUtils.SerializeObject(obslst));
                    string json = CommonUtils.SerializeObject(LstKrd);
                    STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "SetSelectedPositionList", Info1 = json, Info2 = SelectedItem });
                }, System.Windows.Threading.DispatcherPriority.DataBind);
            });
        }

        #region to do diff
        private Dictionary<string, PositionListDataModel> DictLastPositionList = new Dictionary<string, PositionListDataModel>();
        private bool CheckDictioarnyDiff(Dictionary<string, PositionListDataModel> dict1, Dictionary<string, PositionListDataModel> dict2)
        {
            bool result = false;

            var differences = new List<string>();

            // 查找 dict1 中的键在 dict2 中不存在，或值不同的键值对
            var diffInDict1 = dict1.Where(kvp => !dict2.ContainsKey(kvp.Key) || !EqualityComparer<PositionListDataModel>.Default.Equals(kvp.Value, dict2[kvp.Key]))
                                   .Select(kvp => dict2.ContainsKey(kvp.Key)
                                       ? $"Key '{kvp.Key}' has different values: dict1='{kvp.Value}', dict2='{dict2[kvp.Key]}'."
                                       : $"Key '{kvp.Key}' is missing in dict2.");

            // 查找 dict2 中的键在 dict1 中不存在的键值对
            var diffInDict2 = dict2.Where(kvp => !dict1.ContainsKey(kvp.Key))
                                   .Select(kvp => $"Key '{kvp.Key}' is missing in dict1.");

            // 合并所有不同点
            differences.AddRange(diffInDict1);
            differences.AddRange(diffInDict2);

            return result;
        }
        #endregion

        #region 筛选数据
        /// <summary>
        /// 返回所有被选择的节点
        /// </summary>
        public void RefreshNodeKeyWord()
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                this.Keyword.Clear();
                this.Keyword.Add("0");

                if (!string.IsNullOrEmpty(SelectedItem))
                    this.Keyword.AddRange(SelectedItem.Split(',').ToList());
                else
                    this.Keyword.AddRange(LstBook.Where(o=>o.id != 0).Select(o => o.id.ToString()).ToList());

                this.Mv.View.Refresh();

                SingleLoadMaskVisible = Visibility.Collapsed;

                if (SingleLoadMaskVisible == Visibility.Collapsed)
                    SendMsgToChart();

                //更新持仓盈亏
                ReCalcPosition();

            }));
        }

        /// <summary>
        /// 排序算法，需要手工设置界面符号，因为要禁用系统自动排序功能
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sortDirection"></param>
        public void Sort(string key, string sortDirection)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                this.Mv.SortDescriptions.Clear();
                this.Mv.SortDescriptions.Add(new SortDescription("AlwaysTop", ListSortDirection.Descending));
                if (sortDirection == "Ascending")
                {
                    this.Mv.SortDescriptions.Add(new SortDescription(key, ListSortDirection.Descending));
                }
                else
                {
                    this.Mv.SortDescriptions.Add(new SortDescription(key, ListSortDirection.Ascending));
                }
            }));
        }

        private bool FilterByMarketIndicator(ObservableCollection<FilterListBoxItemData> filterListBoxItems, PositionDetailData pbd)
        {
            bool result = true;
            foreach (FilterListBoxItemData lbi in MarketIndicatorList)
            {
                if (lbi.itemIsSelected == false)
                {
                    continue;
                }
                if (lbi.itemText == CommonConstants.ALL)
                {
                    result = true;
                    break;
                }
                if (lbi.itemText == "债券")
                {
                    if (Consts.MARKETINDICATOR_BOND.Equals(pbd.marketindicator))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                }
                else if (lbi.itemText == "期货")
                {
                    if (Consts.MARKETINDICATOR_FUTURE.Equals(pbd.marketindicator))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                }
            }
            return result;
        }

        private bool FilterByFutureType(ObservableCollection<FilterListBoxItemData> filterListBoxItems, PositionDetailData pbd)
        {
            bool result = true;
            foreach (FilterListBoxItemData lbi in FutureTypeList)
            {
                if (lbi.itemIsSelected == false)
                {
                    continue;
                }
                if (lbi.itemText == CommonConstants.ALL)
                {
                    result = true;
                    break;
                }
                if (lbi.itemText == "利率期货")
                {
                    if (Consts.MARKETINDICATOR_BOND.Equals(pbd.marketindicator))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                }
                else if (lbi.itemText == "TS")
                {
                    Regex regex = new Regex("^TS[0-9]{4}");
                    result = regex.IsMatch(pbd.securityid);
                    break;
                }
                else if (lbi.itemText == "TF")
                {
                    Regex regex = new Regex("^TF[0-9]{4}");
                    result = regex.IsMatch(pbd.securityid);
                    break;
                }
                else if (lbi.itemText == "T")
                {
                    Regex regex = new Regex("^T[0-9]{4}");
                    result = regex.IsMatch(pbd.securityid);
                    break;
                }
            }
            return result;
        }

        private void Mv_Filter(object sender, FilterEventArgs e)
        {
            if (Keyword.Count <= 1)
            {
                e.Accepted = true;
                PositionDetailData pbd = e.Item as PositionDetailData;
                //不显示汇总
                //if (pbd.bookid == "0")
                //{
                //    e.Accepted = false;
                //    return;
                //}

                //搜索框关键字搜索
                if (SearchText != null && this.SearchText.Length > 0)
                {
                    if (pbd.securityid.Contains(SearchText) || pbd.symbol.Contains(SearchText))
                    {
                        e.Accepted = true;
                    }
                    else
                    {
                        e.Accepted = false;
                    }
                }
                bool filter1 = FilterByMarketIndicator(MarketIndicatorList, pbd);
                bool filter2 = FilterByFutureType(MarketIndicatorList, pbd);
                bool filter3 = true;//FilterByBondType(InterestRateDebtList, pbd);
                e.Accepted = e.Accepted && filter1 && filter2 && filter3;
                return;
            }
            else
            {
                PositionDetailData pbd = e.Item as PositionDetailData;
                if (Keyword.Contains(pbd.bookid))
                {
                    e.Accepted = true;
                    //不显示汇总
                    //if (pbd.bookid == "0")
                    //{
                    //    e.Accepted = false;
                    //    return;
                    //}

                    //搜索框关键字搜索
                    if (SearchText != null && this.SearchText.Length > 0)
                    {
                        if (pbd.securityid.Contains(SearchText) || (pbd.symbol??"").Contains(SearchText))
                        {
                            e.Accepted = true;
                        }
                        else
                        {
                            e.Accepted = false;
                        }
                    }
                    bool filter1 = FilterByMarketIndicator(MarketIndicatorList, pbd);
                    bool filter2 = FilterByFutureType(MarketIndicatorList, pbd);
                    bool filter3 = true;//FilterByBondType(InterestRateDebtList, pbd);
                    e.Accepted = e.Accepted && filter1 && filter2 && filter3;
                }
                else
                {
                    e.Accepted = false;
                }
            }

        }

        #endregion

        #region 查询字典表
        public void GetBookUser()
        {
            QueryPositionBookResult reply = PositionHandler.Instance.QueryBookUser();

            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    foreach (PostionBookModel pbm in reply.Datas)
                    {
                        lock (LockDict)
                        {
                            BookDict.Add(pbm.id, pbm.bookname);
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 成交手工录入
        /// </summary>
        private void OnManualInputClick()
        {
            if (SelfBookDict.Count == 0)
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                {
                    DeleteBoxView me = new DeleteBoxView();

                    me.Title = "提示";
                    me.setWarnMsg("你还没有配置拥有权限的组合！");
                    me.Owner = null;
                    me.ShowDialog();
                }));

                return;

            }
            else
            {
                ManualInput mi = new ManualInput();
                if (SelectedItem_dgPositionDetail != null) 
                {
                    mi.setDefaultSecurityID(SelectedItem_dgPositionDetail.securityid);
                }
                WindowManager.Instance.OpenWindow("持仓管理_持仓调整", mi);
            }
        }


        private void OnTestCommandClick()
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                List<PositionDetailModel> lsttest = new List<PositionDetailModel>();
                int i = 0;
                foreach (var kk in FullPositionDetail)
                {
                    if (kk.bookid != "0")
                    {
                        PositionDetailData mmm = kk;

                        PositionDetailModel test = JsonConvert.DeserializeObject<PositionDetailModel>(JsonConvert.SerializeObject(mmm));
                        //test.updatetime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");

                        test.netposition = test.netposition + 2;
                        test.longposition = test.longposition + 1;
                        test.shortposition = test.shortposition + 1;

                        lsttest.Add(test);
                        i++;
                        if (i > 2)
                            break;
                    }
                }
                //PositionDetailData m2 = FullPositionDetail.FirstOrDefault(o => o.securityid == "220201.IB");
                // 模拟数据更新
                OnPostionDetailCallBack(lsttest);

                // 模拟刷新列表
                RefreshNodeKeyWord();
            }));
        }

        /// <summary>
        /// 手工跑批计算
        /// </summary>
        private void OnManualProcessingPositionCommand()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                ManualProcessingPositionView Mppv = new ManualProcessingPositionView();
                Mppv.Show();
            }));
        }


        /// <summary>
        /// 手工跑批计算历史
        /// </summary>
        private void OnQueryProcessingRecordHistCommand()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "HistoryView_Focus_Tag", Info1 = "OpenProcessingRecordHist" });
            }));
        }

        #region 持仓明细 风险敞口 
        /// <summary>
        /// 持仓明细
        /// </summary>
        private void OnPositionDetailCommand()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                SubSelectedPageIndex = 0;
                PositionDetailTag = "1";
                RiskExposureTag = "4";
                TreeVisibility = Visibility.Visible;
            }));
        }

        /// <summary>
        /// 风险敞口
        /// </summary>
        private void OnRiskExposureCommand()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                SubSelectedPageIndex = 1;
                PositionDetailTag = "4";
                RiskExposureTag = "1";
                TreeVisibility = Visibility.Collapsed;
            }));
        }

        #endregion

        #endregion

        #region 查询持仓明细

        public async void GetHistoryBookDetail()
        {
            if (LstBook.Count > 0)
            {

                QueryPositionReq req = new QueryPositionReq();
                req.bookid = LstBook.Count > 0 ? string.Join(",", LstBook.Select(o => o.id).ToList()) : string.Empty;

                QueryPositionResult reply = await GetCurrentPositionDetailData(req);

                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        if (reply.Datas != null && reply.Datas.Count > 0)
                        {
                            //CommonUtils.WriteConvertObjectToJsonToText(reply.Datas, "1.txt");
                            foreach (PositionDetailModel pbm in reply.Datas)
                            {
                                string key = $"{pbm.bookid}#{pbm.securityid}#{pbm.settlDate}";

                                lock (updlock)
                                {
                                    PositionDetailData newpdd = FullPositionDetail.FirstOrDefault(o => o.key == key);
                                    bool isfound = newpdd == null ? false : true;

                                    if (!isfound)
                                    {
                                        newpdd = new PositionDetailData();
                                    }

                                    newpdd.bookid = pbm.bookid;
                                    newpdd.bookname = BookDict.ContainsKey(pbm.bookid) ? BookDict[pbm.bookid] : "";
                                    newpdd.key = $"{pbm.bookid}#{pbm.securityid}#{pbm.settlDate}";
                                    newpdd.key = key;
                                    newpdd.securityid = pbm.securityid;
                                    newpdd.symbol = pbm.symbol;
                                    newpdd.symboltype = pbm.symboltype;
                                    newpdd.bondcategory = CommonUtils.GetBondCategoryNameBySecurityId(pbm.securityid);
                                    newpdd.marketindicator = pbm.marketindicator;
                                    newpdd.updatetime = pbm.updatetime;
                                    newpdd.createtime = pbm.createtime;
                                    newpdd.settlType = pbm.settlType;
                                    newpdd.settlDate = pbm.settlDate;
                                    newpdd.traderdate = pbm.traderdate;
                                    //总净持仓
                                    newpdd.netposition = CommonUtils.ConvertToDecimal(pbm.netposition.ToString("N0"));
                                    newpdd.netavgprice = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(pbm.netavgprice.ToString()));
                                    newpdd.netavgyield = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(pbm.netavgyield.ToString()));
                                    //日初头寸
                                    newpdd.initialPosition = CommonUtils.ConvertToDecimal(pbm.initialPosition.ToString("N0"));
                                    //总持仓量(多)
                                    newpdd.longposition = CommonUtils.ConvertToDecimal(pbm.longposition.ToString("N0"));
                                    newpdd.longavgprice = CommonUtils.ConvertToDecimal(pbm.longavgprice.ToString());
                                    newpdd.longpriceTotal = CommonUtils.ConvertToDecimal(pbm.longpriceTotal.ToString());
                                    newpdd.shortposition = CommonUtils.ConvertToDecimal(pbm.shortposition.ToString("N0"));
                                    newpdd.shortavgprice = CommonUtils.ConvertToDecimal(pbm.shortavgprice.ToString());
                                    newpdd.shortpricetotal = CommonUtils.ConvertToDecimal(pbm.shortpricetotal.ToString());

                                    newpdd.lastnetprice = CommonUtils.ConvertToDecimal(pbm.netprice.ToString());
                                    newpdd.lastprice = CommonUtils.ConvertToDecimal(pbm.yield.ToString());
                                    //今持仓量（多）
                                    newpdd.todaylongposition = CommonUtils.ConvertToDecimal(pbm.todaylongposition.ToString("N0"));
                                    //今持仓量（空）
                                    newpdd.todayshortposition = CommonUtils.ConvertToDecimal(pbm.todayshortposition.ToString("N0"));
                                    //今日净持仓
                                    newpdd.dailyNetPosition = CommonUtils.ConvertToDecimal(pbm.dailyNetPosition.ToString("N0"));
                                    // dv01
                                    newpdd.dv01 = CommonUtils.ConvertToDecimal(pbm.dv01.ToString("N2"));

                                    //昨持仓量(多)
                                    newpdd.ytdlongposition = newpdd.longposition - newpdd.todaylongposition;
                                    //昨持仓量(空)
                                    newpdd.ytdshortposition = newpdd.shortposition - newpdd.todayshortposition;


                                    //totalprofitandloss 当日盈亏(万元)
                                    newpdd.totalprofitandloss = CommonUtils.ConvertToDecimal((pbm.totalprofitandloss * 10000).ToString());
                                    //positionprofitandloss 持仓盈亏  当日盈亏 - 平仓盈亏
                                    newpdd.positionprofitandloss = CommonUtils.ConvertToDecimal((pbm.positionprofitandloss * 10000).ToString());
                                    //closingprofitandloss 平仓盈亏 
                                    newpdd.closingprofitandloss = CommonUtils.ConvertToDecimal((pbm.closingprofitandloss * 10000).ToString());
                                    //positionmarket 持仓市值
                                    newpdd.positionmarket = CommonUtils.ConvertToDecimal((newpdd.netposition / 100 * newpdd.lastnetprice * 10000).ToString());

                                    newpdd.delflag = string.IsNullOrEmpty(pbm.delflag) ? "0" : pbm.delflag;

                                    if (newpdd.delflag != "1")
                                    {
                                        if (!isfound)
                                            FullPositionDetail.Add(newpdd);
                                    }
                                    else
                                    {
                                        if (isfound)
                                            FullPositionDetail.Remove(newpdd);
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        LogHelper.Error($"GetHistoryBookDetail Error:{reply.Msg}");
                    }
                }

                RefreshNodeKeyWord();

                //订阅最新价
                SubscribeSecurityidLastExecutionThread();

                //关闭等待状态
                SingleLoadMaskVisible = Visibility.Collapsed;

                SubscribePostion();
            }
        }

        /// <summary>
        /// 查询合约组合持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<QueryPositionResult> GetCurrentPositionDetailData(QueryPositionReq req)
        {
            QueryPositionResult reply = null;
            await Task.Run(() =>
            {
                reply = PositionHandler.Instance.QueryPositionDetail(req);
            });

            return reply;
        }

        /// <summary>
        /// 批量订阅最优价
        /// </summary>
        public void SubscribeSecurityidLastExecutionThread()
        {
            List<string> securityidlist = FullPositionDetail.Where(o=>o.bookid != "0").Select(item => item.securityid).Distinct().ToList();
            List<string> NotSubscribeSecurityid = GetKeysNotInDictionary(securityidlist, SubscribeSecurityidDict);
            lock (LockDict)
            {
                foreach (string key in NotSubscribeSecurityid)
                {
                    securityidQueue.Add(key);
                }

                LogHelper.Info($"SubscribeSecurityidLastExecution Successed! {NotSubscribeSecurityid.Count}");
            }
        }

        /// <summary>
        /// 根据债券信息获取最优价
        /// </summary>
        /// <param name="cancellationToken"></param>
        private async void DoSubscribeSecurityidLastExecutionQueue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string secid = securityidQueue.Take(cancellationToken);

                    if (!SubscribeSecurityidDict.ContainsKey(secid))
                    {
                        SubscribeSecurityidDict.Add(secid, null);
                    }

                    ExecutionReport obj = null;
                    if (SubscribeSecurityidDict[secid] == null)
                        obj = await GetLastExecutionsItem(secid);
                    else
                        obj = SubscribeSecurityidDict[secid];


                    if (obj != null)
                    {
                        if (string.IsNullOrEmpty(obj.NetPrice))
                            return;

                        //更新数据
                        List<PositionDetailData> mlist = FullPositionDetail.ToList().FindAll(o => o.securityid == obj.SecurityID);
                        if (mlist != null && mlist.Count > 0)
                        {
                            lock (updlock)
                            {
                                decimal newnetprice = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(obj.NetPrice));

                                if (newnetprice != mlist[0].lastnetprice)
                                {
                                    foreach (PositionDetailData m2 in mlist)
                                    {
                                        m2.lastnetprice = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(obj.NetPrice));
                                        m2.lastprice = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(obj.Yield));

                                        //positionmarket 持仓市值
                                        //平仓盈亏  closingprofitandloss
                                        //当日盈亏  totalprofitandloss = ondayprofitandloss
                                        //持仓盈亏 positionprofitandloss

                                        //当日盈亏
                                        decimal avp = m2.longposition / 100 * (m2.lastnetprice - m2.longavgprice) + m2.shortposition / 100 * (m2.shortavgprice - m2.lastnetprice);
                                        avp = avp * 10000;
                                        m2.totalprofitandloss = CommonUtils.ConvertToDecimal(avp.ToString());

                                        ////closingprofitandloss 平仓盈亏 
                                        //m2.closingprofitandloss = CommonUtils.ConvertToDecimal(m2.closingprofitandloss.ToString("N4"));

                                        //positionprofitandloss 持仓盈亏  当日盈亏 - 平仓盈亏
                                        m2.positionprofitandloss = m2.totalprofitandloss - m2.closingprofitandloss;

                                        //positionmarket 持仓市值
                                        m2.positionmarket = CommonUtils.ConvertToDecimal((m2.netposition / 100 * m2.lastnetprice * 10000).ToString());

                                        //不再更新
                                        ////m2.updatetime = DateTime.Parse(obj.UpdateTime).ToString("yyyy-MM-dd HH:mm:ss"); ;
                                    }
                                }
                            }
                        }
                    }

                    if (FullPositionDetail.Count > 0 && securityidQueue.Count == 0)
                    {
                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                        {
                            //更新持仓盈亏
                            ReCalcPosition();
                        }));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception on cancellation
            }
        }
        
        public List<string> GetKeysNotInDictionary(List<string> keys, Dictionary<string, ExecutionReport> dictionary)
        {
            return keys.Where(key => !dictionary.ContainsKey(key)).ToList();
        }

        public void SubscribePostion()
        {
            //处理完查询后，再订阅
            if (!IsSubscribe)
            {
                List<string> bookidlist = LstBook.Select(item => item.id.ToString()).Distinct().ToList();

                if (bookidlist.Count > 0)
                {
                    PositionHandler.Instance.SubscribePositionDetail(bookidlist, OnPostionDetailCallBack);
                }

                IsSubscribe = true;

                BaseHandler.Instance.SubscribeProctMarketExecutionsStatus(OnLoadtMarketExecutionsStatus);

                // 收MarketExec消息
                //ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;

                #region 收Exe订单消息
                ExecutionsHandler.Instance.OnOwnExecutions += Instance_OnOwnExecutions;
                #endregion
            }

        }

        /// <summary>
        /// 订阅回调
        /// </summary>
        /// <param name="riskIndicator"></param>
        private void OnPostionDetailCallBack(List<PositionDetailModel> riskIndicator)
        {
            logQueue.Add(riskIndicator);
            //LogHelper.Info($"收到持仓订阅回调信息 {riskIndicator.Count} 条");
        }

        /// <summary>
        /// 处理持仓回调
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void ProcessLogQueue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    List<PositionDetailModel> riskIndicator = logQueue.Take(cancellationToken);

                    List<PositionDetailModel> selfData = new List<PositionDetailModel>(riskIndicator.Where(o => SelfBookDict.ContainsKey(o.bookid)));

                    if (selfData.Count > 0)
                    {
                        Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        {
                            try
                            {
                                foreach (PositionDetailModel pbm in selfData)
                                {
                                    //加入筛选，如果不是自己的则舍弃  security+tradedate+settldate
                                    string key = $"{pbm.bookid}#{pbm.securityid}#{pbm.settlDate}";

                                    PositionDetailData newpdd = FullPositionDetail.FirstOrDefault(o => o.key == key);
                                    bool isfound = newpdd == null ? false : true;

                                    if (!isfound)
                                    {
                                        newpdd = new PositionDetailData();
                                    }

                                    newpdd.bookid = pbm.bookid;
                                    newpdd.bookname = BookDict.ContainsKey(pbm.bookid) ? BookDict[pbm.bookid] : "";
                                    newpdd.key = key;

                                    newpdd.securityid = pbm.securityid;
                                    newpdd.symbol = pbm.symbol;
                                    newpdd.symboltype = pbm.symboltype;
                                    newpdd.bondcategory = CommonUtils.GetBondCategoryNameBySecurityId(pbm.securityid);
                                    newpdd.marketindicator = pbm.marketindicator;
                                    newpdd.updatetime = pbm.updatetime;
                                    newpdd.createtime = pbm.createtime;
                                    newpdd.settlType = pbm.settlType;
                                    newpdd.settlDate = pbm.settlDate;
                                    newpdd.traderdate = pbm.traderdate;
                                    //总净持仓
                                    newpdd.netposition = CommonUtils.ConvertToDecimal(pbm.netposition.ToString("N0"));
                                    newpdd.netavgprice = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(pbm.netavgprice.ToString()));
                                    newpdd.netavgyield = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(pbm.netavgyield.ToString()));
                                    //日初头寸
                                    newpdd.initialPosition = CommonUtils.ConvertToDecimal(pbm.initialPosition.ToString("N0"));
                                    //总持仓量(多)
                                    newpdd.longposition = CommonUtils.ConvertToDecimal(pbm.longposition.ToString("N0"));
                                    newpdd.longavgprice = CommonUtils.ConvertToDecimal(pbm.longavgprice.ToString());
                                    newpdd.longpriceTotal = CommonUtils.ConvertToDecimal(pbm.longpriceTotal.ToString());
                                    newpdd.shortposition = CommonUtils.ConvertToDecimal(pbm.shortposition.ToString("N0"));
                                    newpdd.shortavgprice = CommonUtils.ConvertToDecimal(pbm.shortavgprice.ToString());
                                    newpdd.shortpricetotal = CommonUtils.ConvertToDecimal(pbm.shortpricetotal.ToString());

                                    newpdd.lastnetprice = CommonUtils.ConvertToDecimal(pbm.netprice.ToString());
                                    newpdd.lastprice = CommonUtils.ConvertToDecimal(pbm.yield.ToString());
                                    //今持仓量（多）
                                    newpdd.todaylongposition = CommonUtils.ConvertToDecimal(pbm.todaylongposition.ToString("N0"));
                                    //今持仓量（空）
                                    newpdd.todayshortposition = CommonUtils.ConvertToDecimal(pbm.todayshortposition.ToString("N0"));
                                    //今日净持仓
                                    newpdd.dailyNetPosition = CommonUtils.ConvertToDecimal(pbm.dailyNetPosition.ToString("N0"));
                                    // dv01
                                    newpdd.dv01 = CommonUtils.ConvertToDecimal(pbm.dv01.ToString("N2"));

                                    //昨持仓量(多)
                                    newpdd.ytdlongposition = newpdd.longposition - newpdd.todaylongposition;
                                    //昨持仓量(空)
                                    newpdd.ytdshortposition = newpdd.shortposition - newpdd.todayshortposition;
                                    //////positionmarket 持仓市值
                                    ////if (m2.lastnetprice == 0)
                                    ////{
                                    ////    m2.positionmarket = CommonUtils.ConvertToDecimal((pbm.netposition / 100 * pbm.netavgprice).ToString("N4"));
                                    ////    //ondayprofitandloss 当日盈亏 多方向持仓 /100 * （市场最新成交净价 - 多方向持仓均价） + 空方向持仓 /100 * （空方向持仓均价 - 市场最新成交净价）
                                    ////    m2.ondayprofitandloss = 0;
                                    ////}
                                    ////else
                                    ////{
                                    ////    m2.positionmarket = CommonUtils.ConvertToDecimal((pbm.netposition / 100 * m2.lastnetprice).ToString("N4"));
                                    ////    decimal avp = pbm.longposition / 100 * (m2.lastnetprice - pbm.longavgprice) + pbm.shortposition / 100 * (pbm.shortavgprice - m2.lastnetprice);
                                    ////    m2.ondayprofitandloss = CommonUtils.ConvertToDecimal(avp.ToString("N4"));
                                    ////}

                                    //////当日盈亏(万元)
                                    ////m2.totalprofitandloss = CommonUtils.ConvertToDecimal((pbm.totalprofitandloss * 10000).ToString("N4"));
                                    //////positionprofitandloss 持仓盈亏  当日盈亏 - 平仓盈亏
                                    ////m2.positionprofitandloss = CommonUtils.ConvertToDecimal((pbm.positionprofitandloss * 10000).ToString("N4"));
                                    //////closingprofitandloss 平仓盈亏 
                                    ////m2.closingprofitandloss = CommonUtils.ConvertToDecimal((pbm.closingprofitandloss * 10000).ToString("N4"));

                                    //totalprofitandloss 当日盈亏(万元)
                                    decimal avp = newpdd.longposition / 100 * (newpdd.lastnetprice - newpdd.longavgprice) + newpdd.shortposition / 100 * (newpdd.shortavgprice - newpdd.lastnetprice);
                                    avp = avp * 10000;
                                    newpdd.totalprofitandloss = CommonUtils.ConvertToDecimal(avp.ToString());
                                    //closingprofitandloss 平仓盈亏 
                                    newpdd.closingprofitandloss = CommonUtils.ConvertToDecimal((pbm.closingprofitandloss * 10000).ToString());
                                    //positionprofitandloss 持仓盈亏  当日盈亏 - 平仓盈亏
                                    newpdd.positionprofitandloss = pbm.totalprofitandloss - pbm.closingprofitandloss;
                                    //positionmarket 持仓市值
                                    newpdd.positionmarket = CommonUtils.ConvertToDecimal((pbm.netposition / 100 * newpdd.lastnetprice * 10000).ToString());

                                    //totalprofitandloss 当日盈亏(万元)
                                    //newpdd.totalprofitandloss = CommonUtils.ConvertToDecimal((pbm.totalprofitandloss * 10000).ToString("N4"));
                                    //positionprofitandloss 持仓盈亏  当日盈亏 - 平仓盈亏
                                    //newpdd.positionprofitandloss = CommonUtils.ConvertToDecimal((pbm.positionprofitandloss * 10000).ToString("N4"));
                                    ////closingprofitandloss 平仓盈亏 
                                    //m2.closingprofitandloss = CommonUtils.ConvertToDecimal(m2.closingprofitandloss.ToString("N4"));

                                    //positionmarket 持仓市值
                                    //m2.positionmarket = CommonUtils.ConvertToDecimal((pbm.netposition / 100 * pbm.netprice * 10000).ToString("N4"));
                                    //m2.positionmarket = CommonUtils.ConvertToDecimal((pbm.positionmarketvalue * 10000).ToString("N4"));

                                    newpdd.delflag = string.IsNullOrEmpty(pbm.delflag) ? "0" : pbm.delflag;

                                    if (newpdd.delflag != "1")
                                    {
                                        if (!isfound)
                                            FullPositionDetail.Add(newpdd);
                                    }
                                    else
                                    {
                                        if (isfound)
                                            FullPositionDetail.Remove(newpdd);
                                    }
                                }

                                //更新持仓盈亏
                                ReCalcPosition();
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex.Message);
                            }

                            //订阅最新的债券
                            List<string> securityidlist = selfData.Where(item => item.bookid != "0").Select(item => item.securityid).Distinct().ToList();
                            List<string> NotSubscribeSecurityid = GetKeysNotInDictionary(securityidlist, SubscribeSecurityidDict);
                            lock (LockDict)
                            {
                                foreach (string key in NotSubscribeSecurityid)
                                {
                                    securityidQueue.Add(key);
                                }

                                LogHelper.Info($"New SubscribeSecurityidLastExecution Successed! {NotSubscribeSecurityid.Count}");
                            }
                        }));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception on cancellation
            }
        }

        #endregion

        #region 查询用户持仓组合并订阅持仓明细
        public async void GetBookGroup()
        {
            QueryPositionBookReq req = new QueryPositionBookReq();
            req.username = LocalEnv.UserInfo.UserName;
            req.userid = LocalEnv.UserInfo.UserId;
            QueryPositionBookResult reply = await GetData(req);

            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (PostionBookModel pbm in reply.Datas)
                        {
                            lstBook.Add(new PostionBookData() { IsChecked = false, id = int.Parse(pbm.id), bookname = pbm.bookname, closeby = pbm.closeby, createtime = pbm.createtime, remark = pbm.remark, userid = pbm.userid, username = pbm.username, updatetime = pbm.updatetime });
                            if (!SelfBookDict.ContainsKey(pbm.id))
                            {
                                SelfBookDict.Add(pbm.id, pbm.bookname);
                            }
                        }
                    });
                }

                if (SelfBookDict.Count == 0)
                {
                    //关闭等待状态
                    SingleLoadMaskVisible = Visibility.Collapsed;
                }
                else
                {
                    // 查询并订阅用户持仓组合
                    GetHistoryBookDetail();
                }
            }
       
        }

        public async Task<QueryPositionBookResult> GetData(QueryPositionBookReq req)
        {
            QueryPositionBookResult reply = null;
            await Task.Run(() =>
            {
                reply = PositionHandler.Instance.QueryPositionBook(req);
            });

            return reply;
        }
        #endregion

        /// <summary>
        /// 持仓管理-筛选-债券类型 全部
        /// </summary>
        private bool _isShowAllBondType = true;
        public bool IsShowAllBondType
        {
            get
            {
                return _isShowAllBondType;
            }
            set
            {
                if (_isShowAllBondType != value)
                {
                    _isShowAllBondType = value;
                    this.OnPropertyChanged(nameof(IsShowAllBondType));
                }
            }
        }


        #region 打开tradebook界面
        /// <summary>
        /// 打开tradebook界面
        /// </summary>
        public void OpenTradeBook(PositionDetailData item)
        {
            if (item != null)
            {
                // 使用插件形式打开深度行情页面
                string securityID = CommonUtils.ConvertToString(item.securityid);
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
        #endregion

        #region 市场成交处理完成

        private void OnLoadtMarketExecutionsStatus(bool obj)
        {
            if (obj)
            {
                Thread thread = new Thread(() =>
                {
                    UpdateLastExecutions();
                });

                thread.Priority = ThreadPriority.Highest; // 设置线程的优先级为最高
                thread.Start();
            }
        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateLastExecutions()
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, (Action)(() =>
            {
                //更新组
                try
                {

                    List<string> keys = SubscribeSecurityidDict.Keys.ToList();
                    for (int i = 0;i<keys.Count;i++)
                    {
                        LastExecutionProcesser.Processer.AddGetLastExecutionQueue(keys[i]);
                    }
                }
                catch  (Exception ex)
                {
                    LogHelper.Error($"UpdateLastExecutions Error:{ex.Message}");
                }
            }));
        }

        #endregion

        #region 跑批结果回调
        /// <summary>
        /// 跑批结果回调
        /// </summary>
        /// <param name="callback"></param>
        private void OnSubscribeLoadtMarketExecutionsStatusCallBack(QueryProcessingRecordResult callback)
        {
            List<QueryProcessingRecord> curdaterecord = callback.Datas;
            if (curdaterecord != null)
            {
                // StatusMsg 0 - 未执行 1 - 执行中 2 - 执行成功 3 - 执行失败
                string statuscode = curdaterecord.Count > 0 ? curdaterecord[0].status : "";
                if (statuscode == "1")
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        ClearTotal();
                        FullPositionDetail.Clear();
                        //更新持仓盈亏
                        ReCalcPosition();
                    }));
                }
            }
        }
        #endregion

        public void OpenHistoryPosition(PositionDetailData item)
        {
            if (item != null)
            {
                string endDate = CommonUtils.ConvertToDateTime(item.updatetime).ToString("yyyy-MM-dd");
                string startDate = DateTime.Now.ToString("yyyy-MM-dd");
                string bookid = item.bookid;
                string securityid = item.securityid;
                if (!WindowManager.Instance.IsExist($"HistoryPositionOrderDetail_{securityid+ bookid}"))
                {
                    string bookname = item.bookname;
                    string symbol = item.symbol;
                    HistoryPositionOrderDetail history = new HistoryPositionOrderDetail();
                    HistoryDetailWin win = new HistoryDetailWin();
                    win.Title = $"{securityid} {symbol} @组合 {bookname}";
                    history.SecurityId = securityid;
                    history.BookId = bookid;
                    history.loadWindows();
                    history.QueryData.Execute(string.Empty);
                    win.SetMainControl(history);
                    win.Width = 1000;
                    win.Height = 460;
                    WindowManager.Instance.OpenWindow($"HistoryPositionOrderDetail_{securityid + bookid}", win);
                }
                else 
                {
                    HistoryDetailWin myTradingPopupWin = WindowManager.Instance.GetExistWin($"HistoryPositionOrderDetail_{securityid+ bookid}") as HistoryDetailWin;
                    System.Windows.Controls.Frame frame = (System.Windows.Controls.Frame)myTradingPopupWin.Content;
                    if (frame != null)
                    {
                        HistoryPositionOrderDetail history = (HistoryPositionOrderDetail)frame.Content;
                        if (history != null)
                        {
                            history.QueryData.Execute(string.Empty);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 权限设置
        /// </summary>
        private void SetPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, MODULEACTIONID1);
            if (isAllow)
            {
                HistoryProcessingVis = Visibility.Visible;
            }
            else
            {
                HistoryProcessingVis = Visibility.Collapsed;
            }
            bool isAllow2 = AdminHandler.Instance.CheckPermission(MODULEID, MODULEACTIONID2);
            if (isAllow2)
            {
                PositionCheckVis = Visibility.Visible;
            }
            else
            {
                PositionCheckVis = Visibility.Collapsed;
            }
        }

    }
}