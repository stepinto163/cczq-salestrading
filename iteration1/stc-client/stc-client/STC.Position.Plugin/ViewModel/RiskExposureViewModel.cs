using com.gateway.log;
using STC.Common.Command;
using STC.Common.Handler.Base;
using STC.Common.Handler.Hedge;
using STC.Common.Model.Hedge;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Position.Plugin.View;
using STC.Position.Plugin.ViewModel.Chart;
using System;
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
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Position.Plugin.ViewModel
{
    public class RiskExposureViewModel : ViewModelBase
    {
        #region 构造

        /// <summary>
        /// 锁
        /// </summary>
        
        private object lockObj = new object();

        private object updlock = new object();

        private bool IsSubscribe = false;

        private bool isInit = false;

        /// <summary>
        /// 已订阅KRD列表
        /// </summary>
        private Dictionary<string, Sensitive> SubscribeSecurityidDict = new Dictionary<string, Sensitive>();
        //绘图X坐标轴 字典
        private Dictionary<string, string> XLabelDict = new Dictionary<string, string>();


        #region 敞口krd数据列表

        /// <summary>
        /// 敞口krd数据列表
        /// </summary>
        private ObservableCollection<KrdPositionDataModel> fullKrdDataDetail = new ObservableCollection<KrdPositionDataModel>();
        public ObservableCollection<KrdPositionDataModel> FullKrdDataDetail
        {
            get
            {
                return fullKrdDataDetail;
            }
            set
            {
                fullKrdDataDetail = value;
                this.OnPropertyChanged(nameof(FullKrdDataDetail));
            }
        }

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


        private Visibility _realsimloadmaskvisible = Visibility.Collapsed;

        public Visibility RealSimLoadMaskVisible
        {
            get
            {
                return _realsimloadmaskvisible;
            }

            set
            {
                _realsimloadmaskvisible = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region 持仓模拟交易存储表

        private ObservableCollection<SimPositionExecordersDataModel> fullSimPositionExecorders = new ObservableCollection<SimPositionExecordersDataModel>();
        /// <summary>
        /// 持仓模拟交易存储表
        /// </summary>
        public ObservableCollection<SimPositionExecordersDataModel> FullSimPositionExecorders
        {
            get
            {
                return fullSimPositionExecorders;
            }
            set
            {
                fullSimPositionExecorders = value;
                this.OnPropertyChanged(nameof(FullSimPositionExecorders));
            }
        }

        private DataTable _GlobalView;
        public DataTable GlobalView
        {
            get
            {
                if (_GlobalView == null)
                {
                    _GlobalView = new DataTable();
                    _GlobalView.Columns.Add("ischecked", typeof(string));
                    _GlobalView.Columns.Add("key", typeof(string));
                    _GlobalView.Columns.Add("id", typeof(string));
                    _GlobalView.Columns.Add("bookId", typeof(string));
                    _GlobalView.Columns.Add("bookName", typeof(string));

                    _GlobalView.Columns.Add("securityid", typeof(string));
                    _GlobalView.Columns.Add("symbol", typeof(string));
                    _GlobalView.Columns.Add("RemaingingTime", typeof(string));

                    _GlobalView.Columns.Add("side", typeof(string));

                    _GlobalView.Columns.Add("qty", typeof(decimal));

                    _GlobalView.Columns.Add("closeBy", typeof(string));
                    _GlobalView.Columns.Add("FullName", typeof(string));

                    _GlobalView.Columns.Add("updateTime", typeof(string));

                    _GlobalView.Columns.Add("EditBtnVisible", typeof(string));
                    _GlobalView.Columns.Add("SaveBtnVisible", typeof(string));
                    _GlobalView.Columns.Add("IsCanAdded", typeof(string));
                    _GlobalView.Columns.Add("itemsource", typeof(ObservableCollection<TreeNodeModel>));

                    // 编辑部分
                    _GlobalView.Columns.Add("securityid_edit", typeof(string));
                    _GlobalView.Columns.Add("symbol_orignal", typeof(string));
                    _GlobalView.Columns.Add("RemaingingTime_orignal", typeof(string));
                    _GlobalView.Columns.Add("bookId_edit", typeof(string));
                    _GlobalView.Columns.Add("bookName_edit", typeof(string));
                    _GlobalView.Columns.Add("side_edit", typeof(string));
                    _GlobalView.Columns.Add("qty_edit", typeof(decimal));
                    
                    //_GlobalView.DefaultView.Sort = $"updateTime desc";
                    this.OnPropertyChanged(nameof(GlobalView));
                }
                return this._GlobalView;
            }
            set
            {
                _GlobalView = value;
                this.OnPropertyChanged(nameof(GlobalView));
            }
        }


        private DataView globalviewmv ;
        public DataView GlobalViewMv
        {
            get { return globalviewmv; }
            set
            {
                globalviewmv = value;
                this.OnPropertyChanged(nameof(GlobalViewMv));
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
                    if (!string.IsNullOrEmpty(value))
                        GlobalViewMv.RowFilter = $"securityid = '{_searchText}'";
                    else
                        GlobalViewMv.RowFilter = $"1=1";
                }
            }
        }

        private DataRow _selectSimOrder;
        /// <summary>
        /// 选择的模拟交易
        /// </summary>
        public DataRow SelectSimOrder
        {
            get
            {
                return _selectSimOrder;
            }
            set
            {
                if (_selectSimOrder != value)
                {
                    _selectSimOrder = value;
                    this.OnPropertyChanged(nameof(SelectSimOrder));
                }
            }
        }

        private bool _chkJoinSimTrading = false;
        /// <summary>
        /// 加入模拟交易
        /// </summary>
        public bool ChkJoinSimTrading
        {
            get
            {
                return _chkJoinSimTrading;
            }
            set
            {
                if (_chkJoinSimTrading != value)
                {
                    _chkJoinSimTrading = value;
                    this.OnPropertyChanged(nameof(ChkJoinSimTrading));

                    if (value)
                    {
                        OnChkJoinSimTradingCommand(value);
                    }
                    else
                    {
                        OnChkRemoveSimTradingCommand(value);
                    }
                }
            }
        }


        #region 多选树分组数据源

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


        private string _SelectedItem = null;
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


        #endregion

        #endregion

        bool _isEdit = false;
        public bool IsEdit
        {
            get
            {
                return _isEdit;
            }
            set
            {
                _isEdit = value;
                this.OnPropertyChanged(nameof(IsEdit));
            }
        }

        public bool IsAdd = false;


        private bool _IsAllChecked = false;
        /// <summary>
        /// 模拟交易全选
        /// </summary>
        public bool IsAllChecked
        {
            get { return _IsAllChecked; }
            set
            {
                if (_IsAllChecked != value)
                {
                    _IsAllChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #region Command

        /// <summary>
        /// 批量删除
        /// </summary>
        public ICommand ManualBatchDelSimTradingCommand { get { return new ComRelayCommand(OnManualBatchDelSimTradingCommand); } }

        /// <summary>
        /// 新增
        /// </summary>
        public ICommand ManualAddSimTradingCommand { get { return new DelegateCommand(new Action<object>(OnManualAddSimTradingCommandCommand)); } }

        public ICommand SaveRecordCommand { get { return new DelegateCommand(new Action<object>(OnSaveRecordCommandClick)); } }

        public ICommand CancelRecordCommand { get { return new DelegateCommand(new Action<object>(OnCancelRecordCommandClick)); } }

        public ICommand DelRecordCommand { get { return new DelegateCommand(new Action<object>(OnDelRecordCommandClick)); } }

        public ICommand EditRecordCommand { get { return new DelegateCommand(new Action<object>(OnEditRecordCommandClick)); } }

        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource;
        //待查询债券
        private static BlockingCollection<string> securityidQueue = new BlockingCollection<string>();
        //待处理债券
        private static BlockingCollection<string> krdsecurityidQueue = new BlockingCollection<string>();
        //实况，实况+模拟
        private List<string> sortkeylist = new List<string>() { "1", "2", "3" };

       
        private List<string> keyword = new List<string>();
        /// <summary>
        /// BookId List
        /// </summary>
        public List<string> Keyword
        {
            get
            {
                return keyword;
            }
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    this.OnPropertyChanged(nameof(Keyword));
                }
            }
        }


        /// <summary>
        /// 组合和组合ID对照表
        /// </summary>
        private Dictionary<string, string> BookDict = new Dictionary<string, string>();

        public RiskExposureView view = null;
        #endregion

        #endregion

        public RiskExposureViewModel()
        {
            InitView();
        }

        ~RiskExposureViewModel()
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                XLabelDict.Clear();
                FullKrdDataDetail.Clear();

                string s = string.Empty;
                while (!securityidQueue.IsCompleted && securityidQueue.Count > 0)
                {
                    securityidQueue.TryTake(out s);
                }
                while (!krdsecurityidQueue.IsCompleted && krdsecurityidQueue.Count > 0)
                {
                    krdsecurityidQueue.TryTake(out s);
                }
                cancellationTokenSource.Cancel();
            }));
        }

        public void TryDispose()
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                XLabelDict.Clear();
                FullKrdDataDetail.Clear();
                string s = string.Empty;
                while (!securityidQueue.IsCompleted && securityidQueue.Count > 0)
                {
                    securityidQueue.TryTake(out s);
                }
                while (!krdsecurityidQueue.IsCompleted && krdsecurityidQueue.Count > 0)
                {
                    krdsecurityidQueue.TryTake(out s);
                }

                cancellationTokenSource.Cancel();
            }));
        }

        #region 初始化
            /// <summary>
            /// 初始化视图
            /// </summary>
        public void InitView()
        {
            cancellationTokenSource = new CancellationTokenSource();
            //持仓敞口
            if (!LocalEnv.ConfInfo.IsShowRiskExposure)
            {
                return;
            }

            //初始化绘图X坐标轴
            InitXLabelDict();

            BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);

            this.Mv.Source = FullKrdDataDetail;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            this.Mv.Filter += Mv_Filter;
            this.Mv.View.Refresh();
            this.Mv.SortDescriptions.Add(new SortDescription("sortkey", ListSortDirection.Descending));
            this.Mv.SortDescriptions.Add(new SortDescription("securityId", ListSortDirection.Ascending));

            this.GlobalViewMv = new DataView(GlobalView);

            this.GlobalViewMv.ListChanged += GlobalViewMv_ListChanged;
           
            // sortkey   3实况，2实况+模拟，1差值, 0数据区 - 模拟数据
            if (!FullKrdDataDetail.Any(o => o.sortkey == "3"))
            {
                FullKrdDataDetail.Add(new KrdPositionDataModel() { sortkey = "3", key = "3##", bookid = "0", bookname = "", securityid = "", DV01 = "实况", TotfyDV01 = 0 });
            }
            if (!FullKrdDataDetail.Any(o => o.sortkey == "2"))
            {
                FullKrdDataDetail.Add(new KrdPositionDataModel() { sortkey = "2", key = "2##", bookid = "0", bookname = "", securityid = "", DV01 = "实况+模拟", TotfyDV01 = 0 });
            }
            if (!FullKrdDataDetail.Any(o => o.sortkey == "1"))
            {
                FullKrdDataDetail.Add(new KrdPositionDataModel() { sortkey = "1", key = "1##", bookid = "0", bookname = "", securityid = "", DV01 = "差值", TotfyDV01 = 0 });
            }

            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

            //开启查询 KRD
            Task.Factory.StartNew(() => DoSubscribeSecurityidDV01Queue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
            //处理订阅或获得的 KRD
            Task.Factory.StartNew(() => DoProcDV01Queue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

            //设置树数据源
            MultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();

        }

        private void GlobalViewMv_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded
                || e.ListChangedType == ListChangedType.ItemDeleted)
            {
                DataRow[] drlit2 = GlobalView.Select($"ischecked = '1'");
                if (drlit2.Length != 0 && drlit2.Length == GlobalView.Rows.Count)
                {
                    IsAllChecked = true;
                }
                else
                {
                    IsAllChecked = false;
                }
            }
        }

        /// <summary>
        /// 初始化绘图X坐标轴
        /// </summary>
        private void InitXLabelDict()
        {
            XLabelDict.Add("0.5", "6M");
            XLabelDict.Add("1", "1Y");
            XLabelDict.Add("2", "2Y");
            XLabelDict.Add("3", "3Y");
            XLabelDict.Add("5", "5Y");
            XLabelDict.Add("7", "7Y");
            XLabelDict.Add("10", "10Y");
            XLabelDict.Add("15", "15Y");
            XLabelDict.Add("20", "20Y");
            XLabelDict.Add("30", "30Y");
            XLabelDict.Add("40", "40Y");
            XLabelDict.Add("50", "50Y");
        }

        private void Mv_Filter(object sender, FilterEventArgs e)
        {
            KrdPositionDataModel pbd = e.Item as KrdPositionDataModel;
            if (sortkeylist.Contains(pbd.sortkey))
                e.Accepted = true;
            else
                e.Accepted = false;
        }


        private void GlobalViewMv_Filter(object sender, FilterEventArgs e)
        {
            DataRow row = e.Item as DataRow;
            if (SearchText != null && this.SearchText.Length > 0)
            {
                if (row["securityid"].ToString().Contains(SearchText))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = false;
                }
            }

            return;
        }
        /// <summary>
        /// 债券信息加载成功回调
        /// </summary>
        /// <param name="obj"></param>
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
                            GetBookUser();

                            //如果有持仓组合查询权限
                            if (BookDict.Count > 0)
                            {
                                string boollist = string.Join(",", BookDict.Keys.ToArray());
                                
                                GetRiskExposureData(boollist);
                            }

                        }));
                    });
                }
            }
        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "SetSelectedPositionList" && !string.IsNullOrEmpty(obj.Info1))
            {
                Task.Factory.StartNew(() =>
                {
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                    {
                        //更新债券列表
                        try
                        {
                            bool IsReDraw = false;
                            if (string.IsNullOrEmpty(this.SelectedItem))
                            {
                                IsReDraw = true;
                            }
                            else if (this.SelectedItem != obj.Info2 as string)
                            {
                                IsReDraw = true;
                            }

                            this.SelectedItem = obj.Info2 as string;

                            if (!IsReDraw)
                                return;

                            if (obj.Info1.ToString() != "[]")
                            {
                                RealSimLoadMaskVisible = Visibility.Visible;
                            }
                            

                            string item = "";
                            while (securityidQueue.TryTake(out item))
                            {

                            }

                            while (krdsecurityidQueue.TryTake(out item))
                            {

                            }

                            //清空实况数据
                            //更新实况 KRD 汇总数据
                            KrdPositionDataModel m2 = FullKrdDataDetail.FirstOrDefault(o => o.sortkey == "3");
                            if (m2 != null)
                            {
                                m2.F6M = 0;
                                m2.F1Y = 0;
                                m2.F2Y = 0;
                                m2.F3Y = 0;
                                m2.F5Y = 0;
                                m2.F7Y = 0;
                                m2.F10Y = 0;
                                m2.F15Y = 0;
                                m2.F20Y = 0;
                                m2.F30Y = 0;
                                m2.F40Y = 0;
                                m2.F50Y = 0;
                                m2.TotfyDV01 = 0;
                            }
                            //2实况 + 模拟
                            KrdPositionDataModel realsim = FullKrdDataDetail.FirstOrDefault(o => o.sortkey == "2");
                            if (realsim != null)
                            {
                                realsim.F6M = 0;
                                realsim.F1Y = 0;
                                realsim.F2Y = 0;
                                realsim.F3Y = 0;
                                realsim.F5Y = 0;
                                realsim.F7Y = 0;
                                realsim.F10Y = 0;
                                realsim.F15Y = 0;
                                realsim.F20Y = 0;
                                realsim.F30Y = 0;
                                realsim.F40Y = 0;
                                realsim.F50Y = 0;
                                realsim.TotfyDV01 = 0;
                            }

                            //删除实况 KRD 数据
                            List<KrdPositionDataModel> mlist = FullKrdDataDetail.ToList().FindAll(o => o.sortkey == "0");
                            if (mlist != null && mlist.Count > 0)
                            {
                                lock (updlock)
                                {
                                    foreach (KrdPositionDataModel dr in mlist)
                                    {
                                        FullKrdDataDetail.Remove(dr);
                                    }
                                }
                            }

                            //清空图形
                            SendMsgToChart(new ObservableCollection<KrdPositionDataModel>(FullKrdDataDetail.Where(p => sortkeylist.Contains(p.sortkey))));

                            InitKrdsecurityid(obj.Info1);

                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex.Message);
                        }
                    }));
                });
            }
        }

        #endregion

        #region 更新债券列表
        public void InitKrdsecurityid(string info)
        {
            Task.Factory.StartNew(() =>
            {
                if (JsonSplit.IsJson(info))
                {
                    lock (lockObj)
                    {
                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                        {
                            try
                            {
                                
                                List<PositionListDataModel> LstKrd = CommonUtils.DeserializeObject<List<PositionListDataModel>>(info);

                                foreach (PositionListDataModel pbm in LstKrd)
                                {
                                    //string key = $"{pbm.bookid}#{pbm.securityid}#{pbm.settlDate}";
                                    string key = pbm.key;
                                    KrdPositionDataModel newpdd = FullKrdDataDetail.FirstOrDefault(o => o.key == key);
                                    bool isfound = newpdd == null ? false : true;

                                    if (!isfound)
                                    {
                                        newpdd = new KrdPositionDataModel();
                                        newpdd.key = key;
                                        newpdd.sortkey = "0";
                                        newpdd.bookid = pbm.bookid;
                                        newpdd.bookname = pbm.bookname;
                                        newpdd.securityid = pbm.securityid;
                                        newpdd.settlDate = pbm.settlDate;
                                        newpdd.DV01 = "";

                                        newpdd.updatetime = pbm.updatetime;
                                        newpdd.quantity = pbm.netposition;
                                    }
                                    else
                                    {
                                        newpdd.updatetime = pbm.updatetime;
                                        newpdd.quantity = pbm.netposition;
                                    }

                                    if (!isfound)
                                        FullKrdDataDetail.Add(newpdd);

                                }

                                this.Mv.View.Refresh();

                                if (LstKrd.Count > 0)
                                {
                                    //更新DV01数据
                                    UpdateKV01(LstKrd);
                                }
                                else
                                {
                                    RealSimLoadMaskVisible = Visibility.Collapsed;
                                    //更新总KRD
                                    ReCalcPosition();
                                }

                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex.Message);
                            }
                        }));
                    }
                }
            });
        }

        /// <summary>
        /// 插入模拟交易数据
        /// </summary>
        /// <param name="info"></param>
        public void InitSimSecurityid(List<SimPositionExecordersDataModel> LstKrd)
        {
            Task.Factory.StartNew(() =>
            {
                    lock (lockObj)
                    {
                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                        {
                            try
                            {

                                //if (ChkJoinSimTrading)
                                //{
                                //    if (FullKrdDataDetail.Where(o => o.sortkey == "1").ToList().Count == 0)
                                //    {
                                //        FullKrdDataDetail.Add(new KrdPositionDataModel() { sortkey = "2", key = "2##", bookid = "0", bookname = "", securityid = "", DV01 = "实况+模拟", TotfyDV01 = 0 });
                                //        FullKrdDataDetail.Add(new KrdPositionDataModel() { sortkey = "1", key = "1##", bookid = "0", bookname = "", securityid = "", DV01 = "差值", TotfyDV01 = 0 });
                                //    }
                                //}

                                foreach (SimPositionExecordersDataModel pbm in LstKrd)
                                {
                                    //string key = $"{pbm.bookid}#{pbm.securityid}#{pbm.settlDate}";
                                    string key = pbm.key;
                                    KrdPositionDataModel newpdd = FullKrdDataDetail.FirstOrDefault(o => o.key == key);
                                    bool isfound = newpdd == null ? false : true;

                                    if (!isfound)
                                    {
                                        newpdd = new KrdPositionDataModel();
                                        newpdd.key = key;
                                        newpdd.sortkey = "-";
                                        newpdd.bookid = string.IsNullOrEmpty(pbm.bookId) ? "-" : pbm.bookId;
                                        newpdd.bookname = pbm.bookName ?? "";
                                        newpdd.securityid = pbm.securityid;
                                        newpdd.DV01 = "";

                                        //多仓
                                        if (pbm.side == "0")
                                            newpdd.quantity = pbm.qty;
                                        else
                                            newpdd.quantity = pbm.qty * -1;
                                    }
                                    else
                                    {
                                        newpdd.bookid = string.IsNullOrEmpty(pbm.bookId) ? "-" : pbm.bookId;
                                        newpdd.bookname = pbm.bookName ?? "";
                                        //多仓
                                        if (pbm.side == "0")
                                            newpdd.quantity = pbm.qty;
                                        else
                                            newpdd.quantity = pbm.qty * -1;

                                        newpdd.securityid = pbm.securityid;
                                    }

                                    if (!isfound)
                                        FullKrdDataDetail.Add(newpdd);

                                }

                                this.Mv.View.Refresh();

                                if (LstKrd.Count > 0)
                                {
                                    //更新模拟DV01数据
                                    UpdateSimKV01(LstKrd);
                                }
                                else
                                {
                                    RealSimLoadMaskVisible = Visibility.Collapsed;
                                }

                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex.Message);
                            }
                        }));
                    }

            });
        }

        /// <summary>
        /// 更新模拟交易DV01数据
        /// </summary>
        /// <param name="LstKrd"></param>
        private async void UpdateSimKV01(List<SimPositionExecordersDataModel> LstKrd)
        {
            await Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                {
                    try
                    {
                        foreach (SimPositionExecordersDataModel pbm in LstKrd)
                        {
                            if (SubscribeSecurityidDict.ContainsKey(pbm.securityid))
                            {
                                krdsecurityidQueue.Add(pbm.securityid);
                            }
                        }

                        //订阅KRD
                        List<string> securityidlist = LstKrd.Where(item => item.bookId != "-1").Select(item => item.securityid).Distinct().ToList();
                        List<string> NotSubscribeSecurityid = GetKeysNotInDictionary(securityidlist, SubscribeSecurityidDict);
                        foreach (string key in NotSubscribeSecurityid)
                        {
                            securityidQueue.Add(key);
                        }

                        LogHelper.Info($"New SubscribeSecurityidLastExecution Successed! {NotSubscribeSecurityid.Count}");
                        RealSimLoadMaskVisible = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }));
            });
        }

        /// <summary>
        /// 更新DV01数据
        /// </summary>
        /// <param name="LstKrd"></param>
        private async void UpdateKV01(List<PositionListDataModel> LstKrd)
        {
            await Task.Factory.StartNew(() => 
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                {
                    try
                    {
                        foreach (PositionListDataModel pbm in LstKrd)
                        {
                            if (SubscribeSecurityidDict.ContainsKey(pbm.securityid))
                            {
                                krdsecurityidQueue.Add(pbm.securityid);
                            }
                        }

                        //订阅KRD
                        List<string> securityidlist = LstKrd.Where(item => item.bookid != "0").Select(item => item.securityid).Distinct().ToList();
                        List<string> NotSubscribeSecurityid = GetKeysNotInDictionary(securityidlist, SubscribeSecurityidDict);
                        foreach (string key in NotSubscribeSecurityid)
                        {
                            securityidQueue.Add(key);
                        }

                        LogHelper.Info($"New SubscribeSecurityidLastExecution Successed! {NotSubscribeSecurityid.Count}");
                        RealSimLoadMaskVisible = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }));
            });
        }
        #endregion

        #region GetKRD
        /// <summary>
        /// 查询债券ID 的KRD０１值
        /// </summary>
        /// <param name="secid"></param>
        public void LoadData(string secid)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    if (!SubscribeSecurityidDict.ContainsKey(secid))
                    {
                        RiskExposureCalclateReq req = new RiskExposureCalclateReq() { username = LocalEnv.UserInfo.UserName, securityIds = secid };
                        RiskExposureCalclateResult reply = GetData(req);

                        if (reply != null && reply.IsSuccess())
                        {
                            if (reply.Datas != null && reply.Datas.Count > 0)
                            {
                                Sensitive ss = reply.Datas.Where(p => p.securityId == secid).FirstOrDefault();
                                if (ss != null)
                                {
                                    SubscribeSecurityidDict[secid] = ss;
                                    krdsecurityidQueue.Add(secid);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
            });
        }

        public RiskExposureCalclateResult GetData(RiskExposureCalclateReq req)
        {
            RiskExposureCalclateResult reply = null;
            try
            {
                reply = HedgeHandler.Instance.QueryRiskExposureCalclate(req);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            return reply;
        }

        #endregion

        #region  处理订阅 DV01 债券
        /// 处理订阅 DV01 债券
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void DoSubscribeSecurityidDV01Queue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string secid = securityidQueue.Take(cancellationToken);

                    bool isSubscribedDV01 = SubscribeSecurityidDict.ContainsKey(secid);

                    if (!isSubscribedDV01)
                    {
                        LoadData(secid);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception on cancellation
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        /// <summary>
        /// 更新KRD数据
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void DoProcDV01Queue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string secid = krdsecurityidQueue.Take(cancellationToken);

                    bool isSubscribedDV01 = SubscribeSecurityidDict.ContainsKey(secid);

                    Sensitive krdobj = null;
                    if (SubscribeSecurityidDict[secid] != null)
                        krdobj = SubscribeSecurityidDict[secid];

                    if (krdobj == null)
                    {
                        return;
                    }

                    //更新数据
                    List<KrdPositionDataModel> mlist = FullKrdDataDetail.ToList().FindAll(o => o.securityid == secid);
                    if (mlist != null && mlist.Count > 0)
                    {
                        lock (updlock)
                        {
                            foreach (KrdPositionDataModel m2 in mlist)
                            {
                                m2.F6M = krdobj.rateKrdMap.ContainsKey("0.5") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["0.5"]) * m2.quantity * 100 : 0;
                                m2.F1Y = krdobj.rateKrdMap.ContainsKey("1.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["1.0"]) * m2.quantity * 100 : 0;
                                m2.F2Y = krdobj.rateKrdMap.ContainsKey("2.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["2.0"]) * m2.quantity * 100 : 0;
                                m2.F3Y = krdobj.rateKrdMap.ContainsKey("3.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["3.0"]) * m2.quantity * 100 : 0;
                                m2.F5Y = krdobj.rateKrdMap.ContainsKey("5.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["5.0"]) * m2.quantity * 100 : 0;
                                m2.F7Y = krdobj.rateKrdMap.ContainsKey("7.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["7.0"]) * m2.quantity * 100 : 0;
                                m2.F10Y = krdobj.rateKrdMap.ContainsKey("10.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["10.0"]) * m2.quantity * 100 : 0;
                                m2.F15Y = krdobj.rateKrdMap.ContainsKey("15.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["15.0"]) * m2.quantity * 100 : 0;
                                m2.F20Y = krdobj.rateKrdMap.ContainsKey("20.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["20.0"]) * m2.quantity * 100 : 0;
                                m2.F30Y = krdobj.rateKrdMap.ContainsKey("30.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["30.0"]) * m2.quantity * 100 : 0;
                                m2.F40Y = krdobj.rateKrdMap.ContainsKey("40.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["40.0"]) * m2.quantity * 100 : 0;
                                m2.F50Y = krdobj.rateKrdMap.ContainsKey("50.0") ? CommonUtils.ConvertToDecimal(krdobj.rateKrdMap["50.0"]) * m2.quantity * 100 : 0;

                                m2.TotfyDV01 = m2.F6M + m2.F1Y + m2.F2Y + m2.F3Y + m2.F5Y + m2.F7Y + m2.F10Y + m2.F15Y + m2.F20Y + m2.F30Y + m2.F40Y + m2.F50Y;
                                //不再更新
                                m2.updatetime = DateTime.Parse(krdobj.dataTime).ToString("yyyy-MM-dd HH:mm:ss"); ;
                            }
                        }
                    }

                    if (FullKrdDataDetail.Count > 0 && krdsecurityidQueue.Count == 0)
                    {
                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                        {
                            //更新总KRD
                            ReCalcPosition();
                        }));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception on cancellation
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        /// <summary>
        /// 更新KRD病发送消息绘图
        /// </summary>
        public void ReCalcPosition()
        {
            try
            {
                //实况
                decimal total_F6M = 0m;
                decimal total_F1Y = 0m;
                decimal total_F2Y = 0m;
                decimal total_F3Y = 0m;
                decimal total_F5Y = 0m;
                decimal total_F7Y = 0m;
                decimal total_F10Y = 0m;
                decimal total_F15Y = 0m;
                decimal total_F20Y = 0m;
                decimal total_F30Y = 0m;
                decimal total_F40Y = 0m;
                decimal total_F50Y = 0m;
                decimal total_TotfyDV01 = 0m;

                //模拟
                decimal sim_F6M = 0m;
                decimal sim_F1Y = 0m;
                decimal sim_F2Y = 0m;
                decimal sim_F3Y = 0m;
                decimal sim_F5Y = 0m;
                decimal sim_F7Y = 0m;
                decimal sim_F10Y = 0m;
                decimal sim_F15Y = 0m;
                decimal sim_F20Y = 0m;
                decimal sim_F30Y = 0m;
                decimal sim_F40Y = 0m;
                decimal sim_F50Y = 0m;
                decimal sim_TotfyDV01 = 0m;

                //实况+模拟
                decimal realsim_F6M = 0m;
                decimal realsim_F1Y = 0m;
                decimal realsim_F2Y = 0m;
                decimal realsim_F3Y = 0m;
                decimal realsim_F5Y = 0m;
                decimal realsim_F7Y = 0m;
                decimal realsim_F10Y = 0m;
                decimal realsim_F15Y = 0m;
                decimal realsim_F20Y = 0m;
                decimal realsim_F30Y = 0m;
                decimal realsim_F40Y = 0m;
                decimal realsim_F50Y = 0m;
                decimal realsim_TotfyDV01 = 0m;


                //实况记录
                ObservableCollection<KrdPositionDataModel> Tmp2 = new ObservableCollection<KrdPositionDataModel>(FullKrdDataDetail.Where(p => p.sortkey == "0"));
                if (Tmp2.Count > 0)
                {
                    total_F6M = Tmp2.Sum(p => p.F6M);
                    total_F1Y = Tmp2.Sum(p => p.F1Y);
                    total_F2Y = Tmp2.Sum(p => p.F2Y);
                    total_F3Y = Tmp2.Sum(p => p.F3Y);
                    total_F5Y = Tmp2.Sum(p => p.F5Y);
                    total_F7Y = Tmp2.Sum(p => p.F7Y);
                    total_F10Y = Tmp2.Sum(p => p.F10Y);
                    total_F15Y = Tmp2.Sum(p => p.F15Y);
                    total_F20Y = Tmp2.Sum(p => p.F20Y);
                    total_F30Y = Tmp2.Sum(p => p.F30Y);
                    total_F40Y = Tmp2.Sum(p => p.F40Y);
                    total_F50Y = Tmp2.Sum(p => p.F50Y);
                    total_TotfyDV01 = Tmp2.Sum(p => p.TotfyDV01);
                }

                //更新实况 KRD数据
                KrdPositionDataModel m2 = FullKrdDataDetail.FirstOrDefault(o => o.sortkey == "3");
                if (m2 != null)
                {
                    m2.F6M = total_F6M;
                    m2.F1Y = total_F1Y;
                    m2.F2Y = total_F2Y;
                    m2.F3Y = total_F3Y;
                    m2.F5Y = total_F5Y;
                    m2.F7Y = total_F7Y;
                    m2.F10Y = total_F10Y;
                    m2.F15Y = total_F15Y;
                    m2.F20Y = total_F20Y;
                    m2.F30Y = total_F30Y;
                    m2.F40Y = total_F40Y;
                    m2.F50Y = total_F50Y;
                    m2.TotfyDV01 = total_TotfyDV01;
                }

                if (ChkJoinSimTrading)
                {
                    //模拟记录
                    ObservableCollection<KrdPositionDataModel> Tmp_ = new ObservableCollection<KrdPositionDataModel>(FullKrdDataDetail.Where(p => p.sortkey == "-"));
                    if (Tmp_.Count > 0)
                    {
                        sim_F6M = Tmp_.Sum(p => p.F6M);
                        sim_F1Y = Tmp_.Sum(p => p.F1Y);
                        sim_F2Y = Tmp_.Sum(p => p.F2Y);
                        sim_F3Y = Tmp_.Sum(p => p.F3Y);
                        sim_F5Y = Tmp_.Sum(p => p.F5Y);
                        sim_F7Y = Tmp_.Sum(p => p.F7Y);
                        sim_F10Y = Tmp_.Sum(p => p.F10Y);
                        sim_F15Y = Tmp_.Sum(p => p.F15Y);
                        sim_F20Y = Tmp_.Sum(p => p.F20Y);
                        sim_F30Y = Tmp_.Sum(p => p.F30Y);
                        sim_F40Y = Tmp_.Sum(p => p.F40Y);
                        sim_F50Y = Tmp_.Sum(p => p.F50Y);
                        sim_TotfyDV01 = Tmp_.Sum(p => p.TotfyDV01);
                    }


                    //更新模拟KRD数据 差值
                    KrdPositionDataModel m_ = FullKrdDataDetail.FirstOrDefault(o => o.sortkey == "1");
                    if (m_ != null)
                    {
                        m_.F6M = sim_F6M;
                        m_.F1Y = sim_F1Y;
                        m_.F2Y = sim_F2Y;
                        m_.F3Y = sim_F3Y;
                        m_.F5Y = sim_F5Y;
                        m_.F7Y = sim_F7Y;
                        m_.F10Y = sim_F10Y;
                        m_.F15Y = sim_F15Y;
                        m_.F20Y = sim_F20Y;
                        m_.F30Y = sim_F30Y;
                        m_.F40Y = sim_F40Y;
                        m_.F50Y = sim_F50Y;
                        m_.TotfyDV01 = sim_TotfyDV01;
                    }

                    realsim_F6M = total_F6M + sim_F6M;
                    realsim_F1Y = total_F1Y + sim_F1Y;
                    realsim_F2Y = total_F2Y + sim_F2Y;
                    realsim_F3Y = total_F3Y + sim_F3Y;
                    realsim_F5Y = total_F5Y + sim_F5Y;
                    realsim_F7Y = total_F7Y + sim_F7Y;
                    realsim_F10Y = total_F10Y + sim_F10Y;
                    realsim_F15Y = total_F15Y + sim_F15Y;
                    realsim_F20Y = total_F20Y + sim_F20Y;
                    realsim_F30Y = total_F30Y + sim_F30Y;
                    realsim_F40Y = total_F40Y + sim_F40Y;
                    realsim_F50Y = total_F50Y + sim_F50Y;
                    realsim_TotfyDV01 = total_TotfyDV01 + sim_TotfyDV01;


                    //更新模拟KRD数据 实况+模拟
                    KrdPositionDataModel m_t = FullKrdDataDetail.FirstOrDefault(o => o.sortkey == "2");
                    if (m_t != null)
                    {
                        m_t.F6M = realsim_F6M;
                        m_t.F1Y = realsim_F1Y;
                        m_t.F2Y = realsim_F2Y;
                        m_t.F3Y = realsim_F3Y;
                        m_t.F5Y = realsim_F5Y;
                        m_t.F7Y = realsim_F7Y;
                        m_t.F10Y = realsim_F10Y;
                        m_t.F15Y = realsim_F15Y;
                        m_t.F20Y = realsim_F20Y;
                        m_t.F30Y = realsim_F30Y;
                        m_t.F40Y = realsim_F40Y;
                        m_t.F50Y = realsim_F50Y;
                        m_t.TotfyDV01 = realsim_TotfyDV01;
                    }
                }

                //画 实况, 实况+模拟, 差值
                SendMsgToChart(new ObservableCollection<KrdPositionDataModel>(FullKrdDataDetail.Where(p => sortkeylist.Contains(p.sortkey))));

                //更新最后更新时间
                KrdPositionDataModel lastpse = FullKrdDataDetail.OrderByDescending(p => p.updatetime).FirstOrDefault();

                if (lastpse != null)
                {
                    STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "UpdateDV01LastUpdateTime", Info1 = lastpse.updatetime, Info2 = "" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }
        #endregion

        #region Fucntion

        private void SendMsgToChart(ObservableCollection<KrdPositionDataModel> obslst)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
                {
                    string json = CommonUtils.SerializeObject(obslst);
                    STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "DrawDV01", Info1 = json, Info2 = "" });
                }));
            });
        }

        /// <summary>
        /// 获取 key的差集
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public List<string> GetKeysNotInDictionary(List<string> keys, Dictionary<string, Sensitive> dictionary)
        {
            return keys.Where(key => !dictionary.ContainsKey(key)).ToList();
        }


        public void SubscribeRiskExposure()
        {
            //处理完查询后，再订阅
            if (!IsSubscribe)
            {

                IsSubscribe = true;
            }
        }
        #endregion

        #region Command

        #region 新增
        /// <summary>
        /// 新增
        /// </summary>
        private void OnManualAddSimTradingCommandCommand(object param)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                try
                {
                    if (IsEdit)
                        return;

                    IsEdit = true;
                    IsAdd = true;
                    DataRow row = this.GlobalView.NewRow();
                    row["key"] = Guid.NewGuid().ToString();
                    row["id"] = "0";
                    row["EditBtnVisible"] = "false";
                    row["SaveBtnVisible"] = "true";
                    row["IsCanAdded"] = "true";
                    row["side"] = "0";
                    row["side_edit"] = "0"; 
                    row["itemsource"] = MultiTreeComboxItemsSource;
                    row["closeBy"] = LocalEnv.UserInfo.UserName;
                    row["FullName"] = LocalEnv.UserInfo.FullName;
                    this.GlobalView.Rows.Add(row);
                    this.GlobalView.AcceptChanges();
                    SelectSimOrder = row;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }

        #endregion

        #region 更新债券 
        public void RefreshSecurityid(string securityid, string shortname,string remaingingtime)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                DataRow row = SelectSimOrder;
                
                row.BeginEdit();
                row["securityid_edit"] = securityid;
                row["symbol"] = shortname;
                row["RemaingingTime"] = remaingingtime;
                row.EndEdit();
                this.GlobalView.AcceptChanges();
                
            }));
        }

        public void RefreshBookid(string bookid, string bookname)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                DataRow row = SelectSimOrder;
                if (row == null)
                    return;

                row.BeginEdit();
                row["bookId_edit"] = bookid;
                row["bookName_edit"] = bookname;
                row.EndEdit();
                this.GlobalView.AcceptChanges();
            }));
        }

        #endregion

        #region 保存
        /// <summary>
        /// 保存模拟数据
        /// </summary>
        /// <param name="param"></param>
        private void OnSaveRecordCommandClick(object param)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                try
                {
                    DataRow[] foundRows = GlobalView.Select($"id = '{param as string}'");
                    if (foundRows.Length > 0)
                    {
                        SelectSimOrder = foundRows[0];
                    }

                    DataRow row = SelectSimOrder;

                    if (string.IsNullOrEmpty(row["securityid_edit"].ToString()))
                    {
                        showMessage("请填入债券代码! ");
                        return;
                    }

                    if (string.IsNullOrEmpty(row["bookId_edit"].ToString()))
                    {
                        showMessage("请填入组合! ");
                        return;
                    }

                    decimal qty = CommonUtils.ConvertToDecimal(row["qty_edit"].ToString());
                    if (qty <= 0)
                    {
                        showMessage("面额必须是大于0的正整数! ");
                        return;
                    }

                    if (qty > 100000)
                    {
                        showMessage("面额输入范围（0，100,000] ! ");
                        return;
                    }

                    if (qty % 500 != 0)
                    {
                        showMessage("面额必须是500的整数倍! ");
                        return;
                    }

                    //string newkey = $"{row["bookid"].ToString()}#{row["securityid"].ToString()}#{row["side"].ToString()}";

                    //row["key"] = newkey;

                    //DataRow[] dupikey = GlobalView.Select($"key = '{newkey}'");
                    //if (dupikey.Length > 1)
                    //{
                    //    showMessage("已存在同方向的模拟债券！");
                    //    return;
                    //}

                    row["securityid"] = row["securityid_edit"];
                    row["bookId"] = row["bookId_edit"];
                    row["bookName"] = row["bookName_edit"];
                    row["side"] = row["side_edit"];
                    row["qty"] = row["qty_edit"];

                    row["EditBtnVisible"] = "true";
                    row["SaveBtnVisible"] = "false";
                    row["IsCanAdded"] = "false";
                    row["itemsource"] = new ObservableCollection<TreeNodeModel>();
                    
                    IsEdit = false;
                    IsAdd = false;
                    this.GlobalView.AcceptChanges();

                    //插入后台数据库
                    SavePositionSimExec(row);
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }
        #endregion

        #region 取消
        private void OnCancelRecordCommandClick(object param)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                DataRow row = SelectSimOrder;
                if (string.IsNullOrEmpty(row["updateTime"].ToString()))
                {
                    this.GlobalView.Rows.Remove(row);
                    this.GlobalView.AcceptChanges();
                }
                else
                {
                    row["symbol"] = row["symbol_orignal"];
                    row["RemaingingTime"] = row["RemaingingTime_orignal"];
                    row["EditBtnVisible"] = "true";
                    row["SaveBtnVisible"] = "false";
                    row["IsCanAdded"] = "false";
                    row["itemsource"] = new ObservableCollection<TreeNodeModel>();
                }

                IsEdit = false;
                IsAdd = false;
            }));
        }

        #endregion

        #region 删除或撤销
        private void OnDelRecordCommandClick(object param)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                try
                {
                    DataRow[] foundRows = GlobalView.Select($"id = '{param as string}'");
                    if (foundRows.Length > 0)
                    {
                        SelectSimOrder = foundRows[0];
                    }

                    lock (lockObj)
                    {
                        DataRow row = SelectSimOrder;
                        string id = row["id"].ToString();
                        if (id == "0")
                        {
                            row.BeginEdit();
                            row.Delete();
                            row.EndEdit();
                            this.GlobalView.AcceptChanges();
                        }
                        else
                        {
                            row.BeginEdit();
                            row.Delete();
                            row.EndEdit();
                            this.GlobalView.AcceptChanges();

                            DelPositionSimExec(id);
                        }

                        IsEdit = false;
                        IsAdd = false;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }));
        }

        
        #endregion

        #region 编辑
        private void OnEditRecordCommandClick(object param)
        {
            try
            {
                IsEdit = true;
                IsAdd = false;
                DataRow[] foundRows = GlobalView.Select($"id = '{param as string}'");
                if (foundRows.Length > 0)
                {
                    SelectSimOrder = foundRows[0];
                }

                DataRow row = this.SelectSimOrder;
                row.BeginEdit();
                row["securityid_edit"] = row["securityid"];
                row["symbol_orignal"] = row["symbol"];
                row["RemaingingTime_orignal"] = row["RemaingingTime"];
                row["bookId_edit"] = row["bookId"];
                row["bookName_edit"] = row["bookName"];
                row["side_edit"] = row["side"];
                row["qty_edit"] = row["qty"];

                row["EditBtnVisible"] = "false";
                row["SaveBtnVisible"] = "true";
                row["IsCanAdded"] = "true";
                row["itemsource"] = MultiTreeComboxItemsSource;
                row.EndEdit();
                this.GlobalView.AcceptChanges();
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        #endregion

        #region 批量删除
        /// <summary>
        /// 批量删除
        /// </summary>
        private void OnManualBatchDelSimTradingCommand()
        {
            try
            {
                DataRow[] foundRows = GlobalView.Select($"ischecked = '1'");

                if (foundRows.Length > 0)
                {
                    bool res = CustomMessageBox.Question($"是否批量删除 {foundRows.Length} 个已选模拟交易？", "信息");
                    if (res == true)
                    {
                        int mm = foundRows.Length;
                        string str = ConvertDataTableToJsonExcludingFields(GlobalView, "itemsource", "updateTime", "EditBtnVisible", "SaveBtnVisible", "IsCanAdded");
                        string ids = string.Empty;
                        List<SimPositionExecordersDataModel> simlist = new List<SimPositionExecordersDataModel>(new ObservableCollection<SimPositionExecordersDataModel>(CommonUtils.DeserializeObject<ObservableCollection<SimPositionExecordersDataModel>>(str)).Where(p=>p.ischecked == "1").ToList());
                        if (simlist.Count > 0)
                        {
                            ids = string.Join(",", simlist.Select(p => p.id).ToList().ToArray());
                        }

                        for (int i = 0; i < mm; i++)
                        {
                            DataRow row = foundRows[i];

                            row.BeginEdit();
                            row.Delete();
                            row.EndEdit();
                        }

                        this.GlobalView.AcceptChanges();

                        if (!string.IsNullOrEmpty(ids))
                            DelPositionSimExec(ids);
                    }
                }
                else
                {
                    CustomMessageBox.Warning("请选择批量删除的模拟交易! ", "信息");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }

        }

        #endregion

        #region Sub Function
        /// <summary>
        /// 全选
        /// </summary>
        internal void OnCheckAll()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                bool _checked = IsAllChecked;

                foreach (DataRow dr in GlobalView.Rows)
                {
                    dr["ischecked"] = _checked ? "1" : "0";
                }
            });
        }
        #endregion

        #region 模拟交易插入明细
        /// <summary>
        /// 加入模拟交易
        /// </summary>
        /// <param name="joinsimtradeing"></param>
        public void OnChkJoinSimTradingCommand(bool joinsimtradeing)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (GlobalView.Rows.Count == 0)
                {
                    ChkJoinSimTrading = false;
                    CustomMessageBox.Warning("请增加模拟交易订单! ", "信息");
                    return;
                }
                
                string str = ConvertDataTableToJsonExcludingFields(GlobalView, "itemsource", "ischecked", "updateTime", "EditBtnVisible", "SaveBtnVisible", "IsCanAdded");

                try
                {
                    FullSimPositionExecorders = new ObservableCollection<SimPositionExecordersDataModel>(CommonUtils.DeserializeObject<ObservableCollection<SimPositionExecordersDataModel>>(str));
                    List<SimPositionExecordersDataModel> simlist = new List<SimPositionExecordersDataModel>();
                    foreach(SimPositionExecordersDataModel item in FullSimPositionExecorders)
                    {
                        string key = item.key;
                        SimPositionExecordersDataModel newpdd = simlist.FirstOrDefault(o => o.key == key);
                        bool isfound = newpdd == null ? false : true;

                        if (isfound)
                        {
                            newpdd.qty += item.qty;
                        }
                        else
                        {
                            newpdd = item.Clone();
                            simlist.Add(newpdd);
                        }
                    }
                    //List<SimPositionExecordersDataModel> simlist = new List<SimPositionExecordersDataModel>(FullSimPositionExecorders.ToList());
                    InitSimSecurityid(simlist);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    CustomMessageBox.Warning($"插入失败! {ex.Message}", "信息");
                }
            });
        }

        /// <summary>
        /// 删除模拟交易
        /// </summary>
        /// <param name="joinsimtradeing"></param>
        public void OnChkRemoveSimTradingCommand(bool joinsimtradeing)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    ////删除汇总模拟数据
                    //KrdPositionDataModel removem2 = FullKrdDataDetail.FirstOrDefault(o => o.sortkey == "2");
                    //if (removem2 != null)
                    //{
                    //    FullKrdDataDetail.Remove(removem2);
                    //}
                    //KrdPositionDataModel removem1 = FullKrdDataDetail.FirstOrDefault(o => o.sortkey == "1");
                    //if (removem1 != null)
                    //{
                    //    FullKrdDataDetail.Remove(removem1);
                    //}

                    //删除模拟数据
                    List<KrdPositionDataModel> mlist = FullKrdDataDetail.ToList().FindAll(o => o.sortkey == "-");
                    if (mlist != null && mlist.Count > 0)
                    {
                        lock (updlock)
                        {
                            foreach (KrdPositionDataModel m2 in mlist)
                            {
                                FullKrdDataDetail.Remove(m2);
                            }

                            KrdPositionDataModel updm2 = FullKrdDataDetail.FirstOrDefault(o => o.sortkey == "2");
                            if (updm2 != null)
                            {
                                updm2.F6M = 0;
                                updm2.F1Y = 0;
                                updm2.F2Y = 0;
                                updm2.F3Y = 0;
                                updm2.F5Y = 0;
                                updm2.F7Y = 0;
                                updm2.F10Y = 0;
                                updm2.F15Y = 0;
                                updm2.F20Y = 0;
                                updm2.F30Y = 0;
                                updm2.F40Y = 0;
                                updm2.F50Y = 0;
                                updm2.TotfyDV01 = 0;
                            }

                            KrdPositionDataModel updm1 = FullKrdDataDetail.FirstOrDefault(o => o.sortkey == "1");
                            if (updm1 != null)
                            {
                                updm1.F6M = 0;
                                updm1.F1Y = 0;
                                updm1.F2Y = 0;
                                updm1.F3Y = 0;
                                updm1.F5Y = 0;
                                updm1.F7Y = 0;
                                updm1.F10Y = 0;
                                updm1.F15Y = 0;
                                updm1.F20Y = 0;
                                updm1.F30Y = 0;
                                updm1.F40Y = 0;
                                updm1.F50Y = 0;
                                updm1.TotfyDV01 = 0;
                            }
                        }
                    }

                    //更新绘图 todo...
                    //画 实况, 实况+模拟, 差值
                    SendMsgToChart(new ObservableCollection<KrdPositionDataModel>(FullKrdDataDetail.Where(p => sortkeylist.Contains(p.sortkey))));
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            });
        }

        /// <summary>
        /// DataTable转Json  string str = ConvertDataTableToJsonExcludingFields(GlobalView, "itemsource", "ischecked", "updateTime", "EditBtnVisible", "SaveBtnVisible", "IsCanAdded");
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="excludedFields"></param>
        /// <returns></returns>
        public string ConvertDataTableToJsonExcludingFields(DataTable dt, params string[] excludedFields)
        {
            var filteredRows = dt.Select("ischecked = '1'").AsEnumerable().Select(row =>
            {
                var selectedColumns = dt.Columns.Cast<DataColumn>()
                                    .Where(col => !excludedFields.Contains(col.ColumnName))
                                    .ToDictionary(col => col.ColumnName, col => row[col]);

                return selectedColumns;
            });

            
            return CommonUtils.SerializeObject(filteredRows);
        }

        #endregion

        #endregion

        #region 查询字典表
        public void GetBookUser()
        {
            QueryPositionBookReq req = new QueryPositionBookReq();
            req.username = LocalEnv.UserInfo.UserName;
            req.userid = LocalEnv.UserInfo.UserId;
            QueryPositionBookResult reply = Common.Handler.Position.PositionHandler.Instance.QueryPositionBook(req);

            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    foreach (PostionBookModel pbm in reply.Datas)
                    {
                        lock (updlock)
                        {
                            BookDict.Add(pbm.id, pbm.bookname);
                        }
                    }
                }
            }
        }
        #endregion

        #region 模拟交易处理
        
        #region 查询并插入模拟交易数据 到服务器
        public async void GetRiskExposureData(string bookidlist)
        {
            PositionSimExecReq req = new PositionSimExecReq();
            req.action = "0";
            req.bookIds = bookidlist;

            PositionSimExecResult reply = await GetPositionSimExecReq(req);

            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DataRow[] drlit2 = this.GlobalView.Select($"id='0'");
                        bool isHaveNew = drlit2.Length > 0 ? true : false;

                        foreach (PositionSimExecModel dr in reply.Datas)
                        {
                            // 如果有新增加,则不更新其它记录
                            if (isHaveNew)
                            {
                                if (drlit2[0]["bookId"].ToString() == dr.bookId.ToString() && drlit2[0]["securityId"].ToString() == dr.securityId.ToString() && drlit2[0]["side"].ToString() == dr.side.ToString())
                                {
                                    DataRow updrow = drlit2[0];
                                    updrow.BeginEdit();
                                    updrow["id"] = dr.id.ToString();
                                    updrow["updateTime"] = dr.updateTime.ToString();
                                    updrow["key"] = $"{dr.bookId.ToString()}#{dr.securityId.ToString()}#{dr.side.ToString()}";
                                    updrow["EditBtnVisible"] = "true";
                                    updrow["SaveBtnVisible"] = "false";
                                    updrow["IsCanAdded"] = "false";
                                    updrow.EndEdit();
                                    this.GlobalView.AcceptChanges();
                                }
                            }
                            else
                            {
                                DataRow[] drlst = this.GlobalView.Select($"id='{dr.id.ToString()}'");
                                //如果是修改
                                if (drlst.Length > 0)
                                {
                                    DataRow modirow = drlst[0];
                                    modirow.BeginEdit();

                                    modirow["closeBy"] = dr.closeBy.ToString();
                                    modirow["FullName"] = dr.FullName ?? "";
                                    modirow["securityid"] = dr.securityId.ToString();

                                    Common.Model.Base.BondsInfoPo modibondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(dr.securityId.ToString());
                                    if (modibondsInfo != null)
                                    {
                                        modirow["symbol"] = modibondsInfo.Short_Name;
                                        modirow["RemaingingTime"] = modibondsInfo.RemaingingTime;
                                    }

                                    modirow["bookId"] = dr.bookId.ToString();
                                    modirow["bookName"] = dr.bookName.ToString();
                                    modirow["updateTime"] = dr.updateTime.ToString();
                                    modirow["side"] = dr.side.ToString();
                                    modirow["qty"] = CommonUtils.ConvertToDecimal(dr.qty.ToString());
                                    modirow["EditBtnVisible"] = "true";
                                    modirow["SaveBtnVisible"] = "false";
                                    modirow["IsCanAdded"] = "false";
                                    modirow["key"] = $"{dr.bookId.ToString()}#{dr.securityId.ToString()}#{dr.side.ToString()}";
                                    modirow["itemsource"] = new ObservableCollection<TreeNodeModel>();

                                    modirow.EndEdit();
                                    this.GlobalView.AcceptChanges();
                                }
                                else
                                {
                                    //如果是查询

                                    DataRow row = this.GlobalView.NewRow();

                                    row["id"] = dr.id.ToString();
                                    row["closeBy"] = dr.closeBy.ToString();
                                    row["FullName"] = dr.FullName ?? "";
                                    row["securityid"] = dr.securityId.ToString();

                                    Common.Model.Base.BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(dr.securityId.ToString());
                                    if (bondsInfo != null)
                                    {
                                        row["symbol"] = bondsInfo.Short_Name;
                                        row["RemaingingTime"] = bondsInfo.RemaingingTime;
                                    }

                                    row["bookId"] = dr.bookId.ToString();
                                    row["bookName"] = dr.bookName.ToString();
                                    row["updateTime"] = dr.updateTime.ToString();
                                    row["side"] = dr.side.ToString();
                                    row["qty"] = CommonUtils.ConvertToDecimal(dr.qty.ToString());
                                    row["EditBtnVisible"] = "true";
                                    row["SaveBtnVisible"] = "false";
                                    row["IsCanAdded"] = "false";
                                    row["key"] = $"{dr.bookId.ToString()}#{dr.securityId.ToString()}#{dr.side.ToString()}";
                                    row["itemsource"] = new ObservableCollection<TreeNodeModel>();
                                    this.GlobalView.Rows.Add(row);
                                    this.GlobalView.AcceptChanges();
                                }
                            }
                        }

                        IsEdit = false;
                        IsAdd = false;

                        //更新最后更新时间
                        if (ChkJoinSimTrading)
                        {
                            PositionSimExecModel lastpse = reply.Datas.OrderByDescending(p => p.updateTime).FirstOrDefault();
                            if (lastpse != null)
                            {
                                STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "UpdateDV01LastUpdateTime", Info1 = lastpse.updateTime, Info2 = "" });
                            }
                        }
                    });
                }
            }

            //更新模拟交易实况+模拟绘图
            if (ChkJoinSimTrading)
            {
                OnChkRemoveSimTradingCommand(false);
                OnChkJoinSimTradingCommand(true);
            }
        }

        /// <summary>
        /// 查询模拟交易数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<PositionSimExecResult> GetPositionSimExecReq(PositionSimExecReq req)
        {
            PositionSimExecResult reply = null;
            await Task.Run(() =>
            {
                try
                {
                    reply = HedgeHandler.Instance.QueryPositionSimExec(req);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
            });

            return reply;
        }

        #endregion

        #region 新增/修改/删除 查询模拟交易数据 提交到服务器
        /// <summary>
        /// 新增/修改 查询模拟交易数据
        /// </summary>
        public async void SavePositionSimExec(DataRow row)
        {
            PositionSimExecReq req = new PositionSimExecReq();
            string id = row["id"].ToString();
            
            /// <summary>
            /// 操作类型 0：查询 1：新增 2：修改 3：删除
            /// </summary>
            req.action = id == "0" ? "1" : "2";
            if (req.action == "2")
                req.id = id;

            req.bookId = row["bookId"].ToString();
            req.bookName = row["bookName"].ToString();

            req.securityId = row["securityid"].ToString();
            req.symbol = row["symbol"].ToString();

            req.side = row["side"].ToString();
            req.qty = CommonUtils.ConvertToDecimal(row["qty"].ToString());
            req.closeBy = row["closeBy"].ToString();
            req.FullName = row["FullName"].ToString();

            Common.Model.Fix.Message.PResult reply = await CommmitPositionSimExec(req);

            if (reply != null && reply.IsSuccess())
            {
                //showSuccessMessage($"模拟交易数据保存成功!");
            }
            else
            {
                STC.Common.View.Component.CustomMessageBox.Error($"模拟交易数据保存失败!\r\n{reply.Msg}", "提示", null);
            }

            string boollist = string.Join(",", BookDict.Keys.ToArray());
            GetRiskExposureData(boollist);
        }

        /// <summary>
        /// 删除 拟交易数据
        /// </summary>
        /// <param name="id"></param>
        public async void DelPositionSimExec(string id)
        {
            PositionSimExecReq req = new PositionSimExecReq();

            /// <summary>
            /// 操作类型 0：查询 1：新增 2：修改 3：删除
            /// </summary>
            req.action = "3";
            req.ids = id;

            Common.Model.Fix.Message.PResult reply = await CommmitPositionSimExec(req);

            if (reply != null && reply.IsSuccess())
            {
                showSuccessMessage($"模拟交易数据删除成功!");
            }
            else
            {
                STC.Common.View.Component.CustomMessageBox.Error($"模拟交易数据删除失败!\r\n{reply.Msg}", "提示", null);
            }

            string boollist = string.Join(",", BookDict.Keys.ToArray());
            GetRiskExposureData(boollist);
        }


        public async Task<Common.Model.Fix.Message.PResult> CommmitPositionSimExec(PositionSimExecReq req)
        {
            Common.Model.Fix.Message.PResult reply = null;
            await Task.Run(() =>
            {
                try
                {
                    reply = HedgeHandler.Instance.UpdPositionSimExec(req);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
               
            });

            return reply;
        }
        #endregion

        #endregion

        #region 显示消息
        /// <summary>
        /// 提示Warn消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.Owner = null;
                me.ShowDialog();
            }));
        }

        /// <summary>
        /// 提示成功消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showSuccessMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                SuccessBoxView me = new SuccessBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.Owner = null;
                me.ShowDialog();
            }));
        }
        #endregion
    }
}
