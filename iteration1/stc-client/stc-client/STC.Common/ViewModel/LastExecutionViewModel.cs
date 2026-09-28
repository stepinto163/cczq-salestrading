using com.gateway.log;
using Newtonsoft.Json;
using Stc.Common.Model.EMS;
using STC.Common.Handler.Base;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.OHLC;
using STC.Common.Handler.TBook;
using STC.Common.Model.Base;
using STC.Common.Model.OHLC;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Reflection;
using STC.Common.Model.Chart;
using STC.Common.ViewModel.Data;
using STC.Common.DataProcesser;
using STC.Common.Data;
using static STC.Common.Utils.HomePageDataHandler;
using STC.Common.Handler.Admin;
using STC.Common.Model.Fix.Message;

namespace STC.Common.ViewModel
{
    /// <summary>
    /// 最近成交 LastExecutionView的数据内容
    /// </summary>
    public class LastExecutionViewModel : ViewModelBase
    {
        private CollectionViewSource filteredMarketExecutions = new CollectionViewSource();
        public CollectionViewSource FilteredMarketExecutions
        {
            get { return filteredMarketExecutions; }
            set 
            { 
                filteredMarketExecutions = value;
                this.OnPropertyChanged(nameof(FilteredMarketExecutions));
            }
        }

        //private CollectionViewSource _marketCardExecutions = new CollectionViewSource();
        //public CollectionViewSource MarketCardExecutions
        //{
        //    get { return _marketCardExecutions; }
        //    set { _marketCardExecutions = value; }
        //}

        private string _orderCount = CommonConstants.NO_PRICE;

        public string OrderCount
        {
            get
            {
                return _orderCount;
            }

            set
            {
                _orderCount = value;
                this.OnPropertyChanged(nameof(OrderCount));
            }
        }

        private string _HorizontalScrollBarVisibility = "Auto";
        public string HorizontalScrollBarVisibility
        {
            get
            {
                return _HorizontalScrollBarVisibility;
            }

            set
            {
                _HorizontalScrollBarVisibility = value;
                this.OnPropertyChanged(nameof(HorizontalScrollBarVisibility));

            }
        }

        private int klineButtonWidth = 0;

        public int KlineButtonWidth
        {
            get { return klineButtonWidth; }
            set { klineButtonWidth = value; this.OnPropertyChanged(); }
        }


        private string _closeYield = CommonConstants.NO_PRICE;

        public string CloseYield
        {
            get
            {
                return _closeYield;
            }

            set
            {
                if (value != _closeYield)
                {
                    _closeYield = value;
                    this.OnPropertyChanged(nameof(CloseYield));
                }

            }
        }
        private string _closeNetPx = CommonConstants.NO_PRICE;

        public string CloseNetPx
        {
            get
            {
                return _closeNetPx;
            }

            set
            {
                if (value != _closeNetPx)
                {
                    _closeNetPx = value;
                    this.OnPropertyChanged(nameof(CloseNetPx));
                }

            }
        }

        private string _highYield = CommonConstants.NO_PRICE;

        public string HighYield
        {
            get
            {
                return _highYield;
            }

            set
            {
                _highYield = value;
                this.OnPropertyChanged(nameof(HighYield));

            }
        }

        private string _highNetPx = CommonConstants.NO_PRICE;

        public string HighNetPx
        {
            get
            {
                return _highNetPx;
            }

            set
            {
                if (value != _highNetPx)
                {
                    _highNetPx = value;
                    this.OnPropertyChanged(nameof(HighNetPx));
                }

            }
        }
        private string _lowYield = CommonConstants.NO_PRICE;

        public string LowYield
        {
            get
            {
                return _lowYield;
            }

            set
            {
                _lowYield = value;
                this.OnPropertyChanged(nameof(LowYield));

            }
        }

        private string _lowNetPx = CommonConstants.NO_PRICE;

        public string LowNetPx
        {
            get
            {
                return _lowNetPx;
            }

            set
            {
                if (value != _lowNetPx)
                {
                    _lowNetPx = value;
                    this.OnPropertyChanged(nameof(LowNetPx));
                }

            }
        }
        private string _openYld = CommonConstants.NO_PRICE;

        public string OpenYld
        {
            get
            {
                return _openYld;
            }

            set
            {
                _openYld = value;
                this.OnPropertyChanged(nameof(OpenYld));

            }
        }

        private string _openNetPx = CommonConstants.NO_PRICE;

        public string OpenNetPx
        {
            get
            {
                return _openNetPx;
            }

            set
            {
                if (value != _openNetPx)
                {
                    _openNetPx = value;
                    this.OnPropertyChanged(nameof(OpenNetPx));
                }

            }
        }

        private string _searchText = string.Empty;

        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                _searchText = value;
                this.OnPropertyChanged(nameof(SearchText));
            }
        }

        private string ohlcHeight = "0";
        public string OhlcHeight
        {
            get
            {
                return ohlcHeight;
            }

            set
            {
                ohlcHeight = value;
                this.OnPropertyChanged(nameof(OhlcHeight));
            }
        }


        /// <summary>
        /// 是否是收藏债券， 0：不是 ； 1：是
        /// </summary>
        private string _isFavorite = "0";
        public string IsFavorite
        {
            get
            {
                return _isFavorite;
            }

            set
            {
                if (_isFavorite == value)
                {
                    return;
                }
                _isFavorite = value;
                this.OnPropertyChanged(nameof(IsFavorite));
            }
        }

        /// <summary>
        /// UI上用户默认选择的列表类型
        /// </summary>
        private int _ListTypeIndex = 0;
        public int ListTypeIndex
        {
            get { return _ListTypeIndex; }
            set
            {
                _ListTypeIndex = value;
                this.OnPropertyChanged(nameof(ListTypeIndex));
            }
        }

        /// <summary>
        /// tbook行情是否是%,%=收益率（true）;否则是净价（false）
        /// </summary>
        private string _IsYieldType;

        public string IsYieldType
        {
            get
            {
                return _IsYieldType;
            }

            set
            {
                _IsYieldType = value;
                this.OnPropertyChanged(nameof(IsYieldType));

            }
        }


        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //待处理市场成交
        public ConcurrentQueue<ExecutionReport> ERQueue = new ConcurrentQueue<ExecutionReport>();
        
        private ManualResetEventSlim ErResetEvent = new ManualResetEventSlim(false);

        private object lockobj = new object();

        public LastExecutionViewModel()
        {
            this.Init();
        }

        private void Init()
        {
            try
            {
                bool isPermission = AdminHandler.Instance.CheckPermisionById("4000", "4014");
                if (isPermission)
                {
                    FileInfo[] files = new DirectoryInfo(System.IO.Path.Combine(System.Environment.CurrentDirectory, "plugin")).GetFiles("*.dll");
                    foreach (FileInfo info2 in files)
                    {
                        if (info2.Name.Equals("STC.Chrome.Plugin.dll"))
                        {
                            KlineButtonWidth = isPermission ? 24 : 0;
                            break;
                        }
                    }
                }
                

                //使用使用OHLC接口
                OhlcHeight = LocalEnv.ConfInfo.IsUseOHLCSvr == "0" ? "0": "24";
                //this.filteredMarketExecutions.IsLiveFilteringRequested = true;
                //this.filteredMarketExecutions.IsLiveSortingRequested = true;
                ////this.filteredMarketExecutions.Source = LastExecutionProcesser.Processer.LastMarketExes;

                //// 时间倒序
                //SortDescription timeDesc = new SortDescription("TimeStamp", ListSortDirection.Ascending);
                //this.filteredMarketExecutions.SortDescriptions.Add(timeDesc);
                //this.filteredMarketExecutions.IsLiveFilteringRequested = true;
                //this.filteredMarketExecutions.IsLiveSortingRequested = true;

                //// CDH 成交
                //this._marketCardExecutions.IsLiveFilteringRequested = true;
                //this._marketCardExecutions.IsLiveSortingRequested = true;
                //this._marketCardExecutions.SortDescriptions.Add(timeDesc);

                //this.filteredMarketExecutions.Filter += FilteredMarketExecutions_Filter;
                ResetHOLC();

                //if (_ProcessMarketExecutioinsThreadQueue == null)
                //{
                //    _ProcessMarketExecutioinsThreadQueue = new AsyncThreadQueue<Common.Model.Fix.Message.ExecutionReport>(UpdateMarketExecutions);
                //}

                BaseHandler.Instance.Init(Instance_OnClear);

                RegSvr();

                //订阅最后成交处理完成事件，当获取历史记录完成后更新界面的最后成交
                BaseHandler.Instance.SubscribeProctMarketExecutionsStatus(OnLoadtMarketExecutionsStatusAsync);

                //更新显示用的市场成交
                Task.Factory.StartNew(() => DoProcERQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        #region 手绘15分钟成交图形插件处理功能

        /// <summary>
        /// 绘图LinePage的数据源
        /// </summary>
        private ObservableCollection<ChartModel> chartLineData = new ObservableCollection<ChartModel>();
        public ObservableCollection<ChartModel> ChartLineData
        {
            get
            {
                return chartLineData;
            }

            set
            {
                chartLineData = value;
                this.OnPropertyChanged(nameof(ChartLineData));
            }
        }


        public object LinePageInstance = null;
        public Type ChartType = null;
        public Type ChartModel = null;
        public Assembly ChartAsm = null;
        /// <summary>
        /// 初始化画图数据方法
        /// </summary>
        public MethodInfo ChartChangeDataSource = null;

        /// <summary>
        /// 更新单个数据
        /// </summary>
        public MethodInfo ChartUpdateChart = null;

        /// <summary>
        /// 清空数据
        /// </summary>
        public MethodInfo ChartClearData = null;
        
        //动态加载外部程序集
        public object LoadAssembly()
        {
            IList<FileInfo> list = new List<FileInfo>();
            
            FileInfo[] files = new DirectoryInfo(Path.Combine(System.Environment.CurrentDirectory, "plugin")).GetFiles("*.dll");
            foreach (FileInfo info2 in files)
            {
                if (info2.Name.Equals("STC.Chart.Plugin.dll"))
                {
                    ChartAsm = Assembly.LoadFile(info2.FullName);
                    LinePageInstance = ChartAsm.CreateInstance("STC.Chart.Plugin.Chart.LinePage");
                    ChartType = ChartAsm.GetType("STC.Chart.Plugin.Chart.LinePage");
                    ChartModel = ChartAsm.GetType("STC.Chart.Plugin.Chart.ChartModel");
                    ChartChangeDataSource = ChartType.GetMethod("ChangeDataSource");
                    ChartUpdateChart = ChartType.GetMethod("UpdateChart");
                    ChartClearData = ChartType.GetMethod("ClearData");
                    //ChartInit = ChartType.GetMethod("Init");
                    break;
                }
            }

            return LinePageInstance;
        }
        #endregion

        ~LastExecutionViewModel()
        {
            Dispose();
        }

        new public void Dispose()
        {
            ClearData();
            UnRegSvr();
            //cancellationTokenSource.Cancel();
            //this._marketCardExecutions = null;
            base.Dispose();
        }

        private ObservableCollection<MarketExecutionsItem> ds = new ObservableCollection<MarketExecutionsItem>();
        public ObservableCollection<MarketExecutionsItem> DS
        {
            get
            {
                return ds;
            }

            set
            {
                ds = value;
                this.OnPropertyChanged(nameof(DS));
            }
        }


        public async Task SetItemSource(string securityId)
        {
            if (this.DS != null)
            {
                this.DS.Clear();
            }

            this.SearchText = securityId;
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.ClearData();
                }));
            });

            OhlcHandler.Instance.SubscribeOhlc(this.SearchText, Instance_OnOhlcMsg);

            GetDSData(securityId, FilteredMarketExecutions);

            await GetIsFavorite(securityId);
        }


        private bool isLoadMarketExecutionsStatusOk = false;

        public void OnLoadtMarketExecutionsStatusAsync(bool obj)
        {
            if (obj)
            {
                isLoadMarketExecutionsStatusOk = true;
                GetDSData(this.SearchText, FilteredMarketExecutions);
            }
        }
        
        
        //获取历史数据
        public async Task GetDSData(string securityId, CollectionViewSource fm)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    //DS = new ObservableCollection<MarketExecutionsItem>(LastExecutionProcesser.Processer.GetDatasBySecurityID(securityId).OrderByDescending(o => o.Time).ToList());
                    //UpdateDS();

                    DS = new ObservableCollection<MarketExecutionsItem>();
                    SortDescription timeDesc = new SortDescription("Time", ListSortDirection.Descending);
                    //SortDescription timeDesc2 = new SortDescription("TimeStamp", ListSortDirection.Descending);
                    fm.SortDescriptions.Add(timeDesc);
                    //fm.SortDescriptions.Add(timeDesc2);
                    fm.IsLiveFilteringRequested = true;
                    fm.IsLiveSortingRequested = true;
                    fm.Source = DS;

                    //// 绘制15分钟ChartLine图
                    //UpdateChartLineData();

                    //////从RocksDb中读取市场成交数据
                    ////System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    ////stopwatch.Start();
                    if (!string.IsNullOrEmpty(SearchText) && SearchText != "xxxxxx" && SearchText != "Tradebook")
                    {
                        //// 模拟需要测量的程序运行时间
                        List<ExecutionReport> tmpER = STC.Common.Data.ExecutionsHandler.Instance.GetLocalExecutionsBySecurityID(this.SearchText, true , 3000)?.Where(o => !("0".Equals(o.TimeInForce) && "INNER".Equals(o.VenueTypeGW))).OrderBy(o => o.UpdateTime).ToList();
                        //////////// 停止Stopwatch
                        //////////stopwatch.Stop();
                        //CommonUtils.WriteConvertObjectToJsonToText(tmpER, "1.txt");
                        //////////// 输出所消耗的时间，以毫秒为单位
                        //////////LogHelper.Info($"程序执行时间: {stopwatch.ElapsedMilliseconds} 毫秒");

                        foreach (ExecutionReport obj in tmpER)
                        {
                            ERQueue.Enqueue(obj);
                            ErResetEvent.Set();
                        }
                    }

                }));
            });


        }

        private async Task GetIsFavorite(string securityId)
        {
            await Task.Run(() => 
            {
                IsFavorite = HomePageDataHandler.IsFavorite(this.SearchText);
            });
        }


        private async void RegSvr()
        {
            //订阅
            if (!IsRegSvr)
            {
                ExecutionsHandler.Instance.OnMarketExecutionsOnlyNew += Instance_OnMarketExecutions;
                ExecutionsHandler.Instance.OnOwnExecutionsOnlyNew += Instance_OnOwnExecutions;
                HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;
            }
        }

        private void UnRegSvr()
        {
            //取消订阅
            if (IsRegSvr)
            {
                ExecutionsHandler.Instance.OnMarketExecutionsOnlyNew -= Instance_OnMarketExecutions;
                ExecutionsHandler.Instance.OnOwnExecutionsOnlyNew -= Instance_OnOwnExecutions;
                HomePageDataHandler.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;
                IsRegSvr = false;
            }
        }

        bool IsRegSvr = false;

        #region 更新DP
        private void UpdateDS()
        {
            lock (updlock)
            {
                if (DS.Count > 0)
                {
                    for (int i = DS.Count - 1; i >= 0; i--)
                    {
                        MarketExecutionsItem marketExe = DS.ElementAt(i);
                        if (i == DS.Count - 1)
                        {
                            marketExe.IsFirst = true;
                            marketExe.DownUp = CommonConstants.NO_UP_DOWN;
                            marketExe.DownUpBrush = "0";
                            marketExe.Bp = CommonConstants.NO_UP_DOWN;
                            marketExe.NetPriceSpread = CommonConstants.NO_UP_DOWN;
                        }
                        else
                        {
                            UpdateBp(ref marketExe, _LastExecutionDic[marketExe.SecurityID]);
                        }

                        decimal prevvolume = GetCurrentVenuesPrevVolume(marketExe);
                        decimal currentvolume = GetSimpleVenuesTotalVolume(marketExe);
                        // 更新为当前成交量
                        decimal totalvolume = UpdateLastKineDic(marketExe);
                        marketExe.Volume = currentvolume - prevvolume;
                        marketExe.Volume = marketExe.Volume < 0 ? 0 : marketExe.Volume;
                        marketExe.ShowStar = IsFavorite == "1" ? Visibility.Visible : Visibility.Collapsed;
                        //marketExe.Info1 = currentvolume.ToString();
                        lock (lockobj)
                        {
                            _LastExecutionDic.AddOrUpdate(marketExe.SecurityID, marketExe, (o, e) => marketExe);
                        }
                    }
                   
                }
            }
        }


        /// <summary>
        /// 获取更新前总的成交量
        /// </summary>
        /// <returns></returns>
        private decimal GetPrevVenuesTotalVolume()
        {
            decimal sumvol = 0;
            sumvol = LastKlineDic.Sum(x => x.Value.total_num_trades);
            return sumvol;
        }

        /// <summary>
        /// 返回当前经纪商的交易总量
        /// </summary>
        /// <param name="marketExe"></param>
        /// <returns></returns>
        private decimal GetCurrentVenuesPrevVolume(MarketExecutionsItem marketExe)
        {
            decimal sumvol = 0;
            if (LastKlineDic.ContainsKey(marketExe.SourceTrader))
            {
                sumvol = LastKlineDic[marketExe.SourceTrader].total_num_trades;
            }

            return sumvol;
        }

        /// <summary>
        /// 更新经纪商成交量字典, 返回当前经纪商成交量
        /// </summary>
        private decimal UpdateLastKineDic(MarketExecutionsItem marketExe)
        {
            MarketKLine market = GetMarketKLineModel(marketExe);
            if (!string.IsNullOrEmpty(market.TimeStamp))
            {
                LastKlineDic.AddOrUpdate(marketExe.SourceTrader, market, (o, e) => market);
            }

            return market.total_num_trades;
        }

        /// <summary>
        /// 获取单个经纪商的总成交量
        /// </summary>
        /// <param name="marketExe"></param>
        /// <returns></returns>
        private decimal GetSimpleVenuesTotalVolume(MarketExecutionsItem marketExe)
        {
            decimal sumvol = 0;
            // 更新经纪商 Volumn和K线值
            MarketKLine market = GetMarketKLineModel(marketExe);
            if (!string.IsNullOrEmpty(market.TimeStamp))
            {
                sumvol = market.total_num_trades;
            }

            return sumvol;
        }

        /// <summary>
        /// 处理k线数据 info1 转为 MarketKLine
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal MarketKLine GetMarketKLineModel(MarketExecutionsItem obj)
        {
            MarketKLine market = new MarketKLine();
            string tmpstr = obj.Info1;
            if (JsonSplit.IsJson(tmpstr))
            {
                market = JsonConvert.DeserializeObject<MarketKLine>(tmpstr);
                market.TimeStamp = obj.TimeStamp;
            }

            return market;
        }

        /// <summary>
        /// 最后经纪商k线数据
        /// </summary>
        private ConcurrentDictionary<string, MarketKLine> LastKlineDic = new ConcurrentDictionary<string, MarketKLine>();

        private object updlock = new object();
        /// <summary>
        /// 缓存最新的成交(不包含MANUAL,QUANT,IDB)
        /// key：SecurrityId
        /// value: ExecutionReport
        /// </summary>
        private ConcurrentDictionary<string, MarketExecutionsItem> _LastExecutionDic = new ConcurrentDictionary<string, MarketExecutionsItem>();

        private void UpdateBp(ref MarketExecutionsItem marketExe, MarketExecutionsItem second)
        {
            if (second != null)
            {
                decimal bp = marketExe.ExecutePricePercent - second.ExecutePricePercent;
                string Bp = bp == 0 ? string.Empty : (bp * 100).ToString();

                marketExe.DownUp = bp > 0 ? CommonConstants.UP : bp < 0 ? CommonConstants.DOWN : CommonConstants.NO_UP_DOWN;
                marketExe.DownUpSort = bp > 0 ? 1 : bp < 0 ? 2 : 0;
                marketExe.DownUpBrush = bp > 0 ? "1" : bp < 0 ? "2" : "0";

                if (marketExe.DownUpSort == 1)
                {
                    marketExe.Bp = "+" + CommonUtils.FormatNum(Bp);
                }
                else if (marketExe.DownUpSort == 2)
                {
                    marketExe.Bp = CommonUtils.FormatNum(Bp);
                }
                else
                {
                    marketExe.Bp = CommonConstants.NO_UP_DOWN;
                }

                decimal netPxUnit = marketExe.NetPrice - second.NetPrice;
                string NetPxUnit = netPxUnit == 0 ? string.Empty : netPxUnit.ToString();

                marketExe.DownUp = netPxUnit > 0 ? CommonConstants.UP : netPxUnit < 0 ? CommonConstants.DOWN : CommonConstants.NO_UP_DOWN;
                marketExe.DownUpSort = netPxUnit > 0 ? 1 : netPxUnit < 0 ? 2 : 0;
                marketExe.DownUpBrush = netPxUnit > 0 ? "1" : netPxUnit < 0 ? "2" : "0";

                if (marketExe.SecurityID.EndsWith(".SH") || marketExe.SecurityID.EndsWith(".SZ") || "5".Equals(marketExe.MarketIndicator ?? "4"))
                {
                    if (marketExe.DownUpSort == 1)
                    {
                        marketExe.NetPriceSpread = "+" + CommonUtils.FormatNum3(NetPxUnit);
                    }
                    else if (marketExe.DownUpSort == 2)
                    {
                        marketExe.NetPriceSpread = CommonUtils.FormatNum3(NetPxUnit);
                    }
                    else
                    {
                        marketExe.NetPriceSpread = CommonConstants.NO_UP_DOWN;
                    }
                }
                else
                {
                    if (marketExe.DownUpSort == 1)
                    {
                        marketExe.NetPriceSpread = "+" + CommonUtils.FormatNum(NetPxUnit);
                    }
                    else if (marketExe.DownUpSort == 2)
                    {
                        marketExe.NetPriceSpread = CommonUtils.FormatNum(NetPxUnit);
                    }
                    else
                    {
                        marketExe.NetPriceSpread = CommonConstants.NO_UP_DOWN;
                    }
                }
            }
        }

        #endregion

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("MarketOrderViewModel Clear, security:{0}", this.SearchText));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                ResetHOLC();
                ClearData();
            }
        }

        public void ClearData(bool isClearSymbol = false)
        {
            try
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    lock (updlock)
                    {
                        if (isClearSymbol)
                            this.SearchText = string.Empty;

                        
                        ExecutionReport collect;
                        while (ERQueue.TryDequeue(out collect))
                        {
                        }
                        //if (_ProcessMarketExecutioinsThreadQueue != null)
                        //    _ProcessMarketExecutioinsThreadQueue.Clear();

                        if (this.DS != null)
                        {
                            this.DS.Clear();
                        }
                        _LastExecutionDic.Clear();
                        LastKlineDic.Clear();
                        ChartLineData.Clear();
                        OpenYld = LowYield = OrderCount = HighYield = CloseYield = CommonConstants.NO_PRICE;
                        OpenNetPx = LowNetPx = HighNetPx = CloseNetPx = CommonConstants.NO_PRICE;
                        ResetHOLC();

                        //ChartClearData?.Invoke(LinePageInstance, new object[] { });

                    }
                }));

            }
            catch { }
        }

        //private AsyncThreadQueue<Common.Model.Fix.Message.ExecutionReport> _ProcessMarketExecutioinsThreadQueue;
        private object lockObj = new object();
        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMarketExecutions(Model.Fix.Message.ExecutionReport obj)
        {
            if (obj.SecurityID == this.SearchText)
            {
                if ("0".Equals(obj.TimeInForce) && "INNER".Equals(obj.VenueTypeGW))
                {
                    return;
                }

                ERQueue.Enqueue(obj);
                ErResetEvent.Set();
                //lock (updlock)
                //{
                //    if (_ProcessMarketExecutioinsThreadQueue != null)
                //    {
                //        this._ProcessMarketExecutioinsThreadQueue.Enqueue(obj);
                //    }
                //}
            }
        }

        private void Instance_OnOhlcMsg(object obj)
        {
            if (obj != null)
            {
                if ((obj as OhlcMsg).securityid == this.SearchText)
                {
                    RefreshHOLC((obj as OhlcMsg));
                }
            }
        }

        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOwnExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj.SecurityID == this.SearchText)
            {
                if ("0".Equals(obj.TimeInForce) && "INNER".Equals(obj.VenueTypeGW))
                {
                    return;
                }

                ERQueue.Enqueue(obj);
                ErResetEvent.Set();
                //lock (updlock)
                //{
                //    if (_ProcessMarketExecutioinsThreadQueue != null)
                //    {
                //        this._ProcessMarketExecutioinsThreadQueue.Enqueue(obj);
                //    }
                //}
            }
        }

        #region 手绘15分钟成交图形处理

        public async Task UpdateChartLineData()
        {
            await Task.Run(() =>
            {
                if (LinePageInstance != null)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        lock (updlock)
                        {
                            ChartLineData = new ObservableCollection<ChartModel>();
                            if (DS.Count > 0)
                            {
                                for (int i = DS.Count - 1; i >= 0; i--)
                                {
                                    ChartModel tmpData = GetLineChartModel(DS.ElementAt(i));
                                    if (tmpData != null)
                                    {
                                        if (tmpData.Time != null)
                                        {
                                            ChartLineData.Add(tmpData);
                                        }
                                    }
                                }
                                // string s = JsonConvert.SerializeObject(ChartLineData);
                                ChartChangeDataSource?.Invoke(LinePageInstance, new object[] { ChartLineData, this.SearchText });
                            }
                        }
                    }));
                }
            });
        }

        /// <summary>
        /// 清空绘图
        /// </summary>
        public void ResetChartLine()
        {
            lock (lockObj)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    ChartClearData?.Invoke(LinePageInstance, new object[] { });
                }));
            }
        }

        /// <summary>
        /// 更新 LineChart 新的数据(因为手工绘制图有BUG，所以此方法不用)
        /// </summary>
        /// <param name="obj"></param>
        private async void UpdateLineChart(MarketExecutionsItem obj)
        {
            if (LinePageInstance != null)
            {
                UpdateLineChartTask(obj);
            }
        }

        private async Task UpdateLineChartTask(MarketExecutionsItem obj)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    ChartModel tmpData = GetLineChartModel(obj);
                    if (tmpData.Time != null)
                    {
                        ChartUpdateChart?.Invoke(LinePageInstance, new object[] { tmpData});
                    }
                }));
            });
        }

        /// <summary>
        /// 处理 LineChart 数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private ChartModel GetLineChartModel(MarketExecutionsItem obj)
        {
            ChartModel tmpData = new ChartModel();
            //string tmpstr = obj.Info1;
            //if (JsonSplit.IsJson(tmpstr))
            //{
            //    MarketKLine market = JsonConvert.DeserializeObject<MarketKLine>(tmpstr);
            //    if (market.total_num_trades > 0)
            //    {
            //        tmpData.Volumn = int.Parse(Math.Ceiling(obj.Volume).ToString());
            //    }
            //    else
            //    {
            //        tmpData.Volumn = 0;
            //    }
            //    tmpData.Time = DateTime.Parse(obj.Time).ToString("HH:mm:ss");
            //    tmpData.Price = obj.ExecutePricePercent;
            //}

            tmpData.Volumn = 1;
            if (string.IsNullOrEmpty(obj.Time))
            {
                return null;
            }
            else
            {
                tmpData.Time = obj.Time.Split(' ')[1];
                tmpData.Price = obj.ExecutePricePercent;

                return tmpData;

            }
        }

        #endregion


        /// <summary>
        /// 更新市场成交，仅仅用来显示，不包含内部成交
        /// </summary>
        private void DoProcERQueue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 等待数据可用
                    ErResetEvent.Wait();

                    ExecutionReport data;

                    while (ERQueue.TryDequeue(out data))
                    {
                        try
                        {
                            UpdateMarketExecutions(data);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex.Message);
                        }
                    }

                    // 重置事件
                    ErResetEvent.Reset();
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


        #region 更新成交趋势
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (string.IsNullOrEmpty(SearchText)  && !string.IsNullOrEmpty(SearchText))
                return;

                lock (updlock)
                {
                    MarketExecutionsItem marketExe = null;

                    //// 只保留CDH
                    //if (!obj.VenueTypeGW.ToUpper().Equals(ApiConsts.CDH.ToUpper()))
                    //{
                    //    return;
                    //}
                    string key = obj.OrderID;
                    if (obj.VenueTypeGW.ToUpper().Equals(Common.Api.ApiConsts.CDH.ToUpper()))
                    {
                        key = obj.ExecID;
                    }

                    marketExe = DS.Where(orderItem => orderItem.Key == key)?.OrderBy(p => p.TimeStamp)?.LastOrDefault();

                    bool isNeedUpdate = false;
                    if (marketExe != null)
                    {
                        DateTime orgtransactTime = DateTimeEx.ConvertStringToDateTime(marketExe.Time);
                        DateTime newtransactTime = DateTimeEx.ConvertStringToDateTime(obj.TransactTime);

                        DateTime orgupdateTime = DateTimeEx.ConvertStringToDateTime(marketExe.TimeStamp);
                        DateTime newupdateTime = DateTimeEx.ConvertStringToDateTime(obj.UpdateTime);

                        if (newtransactTime >= orgtransactTime && newupdateTime >= orgupdateTime)
                        {
                            //更新
                            isNeedUpdate = true;
                        }
                        else
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }
                    }
                    else
                    {
                        // 如果状态是4， 删除这笔单子。
                        if (obj.OrdStatus == "4")
                        {
                            return;
                        }

                        marketExe = new MarketExecutionsItem();
                        marketExe.Key = key;
                        marketExe.Side = ConvertSide(obj.Side); // 方向
                        marketExe.SecurityID = obj.SecurityID; // 代码
                        marketExe.MarketIndicator = obj.MarketIndicator;

                        marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
                        marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.Yield); // 成交价
                        marketExe.SourceTrader = obj.Venues; // 来源
                        marketExe.Time = obj.TransactTime;// 时间
                        marketExe.SettlType = obj.SettlType ?? ""; // 清算速度
                        if (marketExe.SettlType.Contains('.'))
                        {
                            marketExe.SettlType_yuan = obj.SettlType;
                        }
                        else
                        {
                            marketExe.SettlType_yuan = "";
                        }
                        marketExe.Info1 = obj.Info1;
                        marketExe.TimeStamp = obj.UpdateTime;
                        marketExe.IsValid = obj.Vs == null || obj.Vs == "Y" || obj.Vs == "" ? "Y" : "N";
                        marketExe.IsOwnInst = ("INNER".Equals(obj.VenueTypeGW) || "CDH".Equals(obj.VenueTypeGW) || "CMDS".Equals(obj.VenueTypeGW)
                        || "SDN".Equals(obj.VenueTypeGW) || "SSE".Equals(obj.VenueTypeGW)
                        || "SSF".Equals(obj.VenueTypeGW) || "SZE".Equals(obj.VenueTypeGW)) ? "N" : "Y";
                        if ("Y".Equals(marketExe.IsOwnInst))
                        {
                            marketExe.OwnInstTip = "本机构";
                        }
                        marketExe.VenueTypeGW = obj.VenueTypeGW;

                        if (obj.SecurityID.EndsWith(".SH") || obj.SecurityID.EndsWith(".SZ") || "5".Equals(obj.MarketIndicator ?? "4"))
                            marketExe.NetPrice_Display = CommonUtils.FormatNum3(obj.NetPrice);
                        else
                            marketExe.NetPrice_Display = CommonUtils.FormatPrice(obj.NetPrice);

                        marketExe.NetPrice = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        marketExe.LastQty = string.IsNullOrEmpty(obj.LastQty) ? "--" : ("0".Equals(obj.LastQty) ? "--" : obj.LastQty);
                        //星星状态
                        //marketExe.ShowStar = _CacheBondList.Any(p => p.securityid == marketExe.SecurityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        // to do ...
                        //marketExe.ShowStar = HomePageDataHandler.IsFavorite(marketExe.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                        UpdateOrderDp(obj.SecurityID, ref marketExe, obj);

                        //decimal prevvolume = GetCurrentVenuesPrevVolume(marketExe);
                        //decimal currentvolume = GetSimpleVenuesTotalVolume(marketExe);
                        //// 更新为当前成交量
                        //marketExe.Volume = currentvolume - prevvolume;
                        //marketExe.Volume = marketExe.Volume < 0 ? 0 : marketExe.Volume;
                        decimal totalvolume = UpdateLastKineDic(marketExe);
                        marketExe.Volume = 1;

                        DS.Insert(0, marketExe);

                        // 更新为当前成交量
                        _LastExecutionDic.AddOrUpdate(marketExe.SecurityID, marketExe, (o, e) => marketExe);
                        //ExecutionsHandler.Instance.PublishLastNewMarketExecutions(marketExe);
                        // 更新HOLC
                        //RefreshHOLC();

                        // 绘制15分钟ChartLine图
                        UpdateLineChart(marketExe);
                        return;
                    }

                    if (isNeedUpdate)
                    {
                        // 如果状态是4， 删除这笔单子。
                        if (obj.OrdStatus == "4")
                        {
                            DS.Remove(marketExe);
                            //更新最后的成交价
                            if (DS.Count > 0)
                            {
                                MarketExecutionsItem oldItem = DS.First();
                                _LastExecutionDic.AddOrUpdate(oldItem.SecurityID, oldItem, (o, e) => oldItem);
                                //ExecutionsHandler.Instance.PublishLastNewMarketExecutions(oldItem);
                            }

                            // 更新LineChart
                            // UpdateChartLineData();

                            return;
                        }

                        marketExe.Key = key;
                        marketExe.Side = ConvertSide(obj.Side); // 方向
                        marketExe.SecurityID = obj.SecurityID; // 代码
                        marketExe.VenueTypeGW = obj.VenueTypeGW;
                        marketExe.MarketIndicator = obj.MarketIndicator;

                        marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
                        marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.Yield); // 成交价
                        marketExe.SourceTrader = obj.Venues; // 来源
                        marketExe.SettlType = obj.SettlType ?? ""; // 清算速度
                        if (marketExe.SettlType.Contains('.'))
                        {
                            marketExe.SettlType_yuan = obj.SettlType;
                        }
                        else
                        {
                            marketExe.SettlType_yuan = "";
                        }
                        marketExe.ExecuteAmount = CommonUtils.ConvertToDecimal(obj.LastQty);
                        marketExe.Info1 = obj.Info1;
                        marketExe.TimeStamp = obj.UpdateTime;
                        marketExe.IsValid = obj.Vs == null || obj.Vs == "Y" || obj.Vs == "" ? "Y" : "N";
                        marketExe.IsOwnInst = ("INNER".Equals(obj.VenueTypeGW) || "CDH".Equals(obj.VenueTypeGW) || "CMDS".Equals(obj.VenueTypeGW)
                        || "SDN".Equals(obj.VenueTypeGW) || "SSE".Equals(obj.VenueTypeGW)
                        || "SSF".Equals(obj.VenueTypeGW) || "SZE".Equals(obj.VenueTypeGW)) ? "N" : "Y";
                        if ("Y".Equals(marketExe.IsOwnInst))
                        {
                            marketExe.OwnInstTip = "本机构";
                        }

                        if (obj.SecurityID.EndsWith(".SH") || obj.SecurityID.EndsWith(".SZ") || "5".Equals(obj.MarketIndicator ?? "4"))
                            marketExe.NetPrice_Display = CommonUtils.FormatNum3(obj.NetPrice);
                        else
                            marketExe.NetPrice_Display = CommonUtils.FormatPrice(obj.NetPrice);

                        marketExe.NetPrice = CommonUtils.ConvertToDecimal(obj.NetPrice);
                        marketExe.LastQty = string.IsNullOrEmpty(obj.LastQty) ? "--" : ("0".Equals(obj.LastQty) ? "--" : obj.LastQty);
                        //LastPriceParam lastPriceParam = CalLastExe(marketExe, _LastExecutionDic[obj.SecurityID]);
                        //星星状态
                        //marketExe.ShowStar = _CacheBondList.Any(p => p.securityid == marketExe.SecurityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        // to do ...
                        //marketExe.ShowStar = HomePageDataHandler.IsFavorite(marketExe.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                        if (marketExe.Time != obj.TransactTime)
                        {
                            marketExe.Time = obj.TransactTime;// 时间

                            //decimal prevvolume = GetCurrentVenuesPrevVolume(marketExe);
                            //decimal currentvolume = GetSimpleVenuesTotalVolume(marketExe);
                            //// 更新为当前成交量
                            //decimal totalvolume = UpdateLastKineDic(marketExe);
                            //marketExe.Volume = currentvolume - prevvolume;
                            //marketExe.Volume = marketExe.Volume < 0 ? 0 : marketExe.Volume;
                            decimal totalvolume = UpdateLastKineDic(marketExe);
                            marketExe.Volume = 1;

                            DS.Insert(0, marketExe);

                            // 更新为当前成交量
                            _LastExecutionDic.AddOrUpdate(marketExe.SecurityID, marketExe, (o, e) => marketExe);
                            //ExecutionsHandler.Instance.PublishLastNewMarketExecutions(marketExe);
                            // 更新HOLC
                            //RefreshHOLC();

                            //绘制15分钟ChartLine图
                            //UpdateLineChart(marketExe);
                        }
                    }
                }
            }));
        }

        private void UpdateOrderDp(string securityID, ref MarketExecutionsItem marketExe, Common.Model.Fix.Message.ExecutionReport newExe)
        {
            if (!_LastExecutionDic.ContainsKey(securityID))
            {
                marketExe.IsFirst = true;
                marketExe.DownUp = CommonConstants.NO_UP_DOWN;
                marketExe.DownUpBrush = "0";
                marketExe.Bp = CommonConstants.NO_UP_DOWN;
                marketExe.NetPriceSpread = CommonConstants.NO_UP_DOWN;
            }
            else
            {
                string old = _LastExecutionDic[securityID].TimeStamp;
                if (old.CompareTo(newExe.UpdateTime) <= 0)
                {
                    UpdateBp(ref marketExe, _LastExecutionDic[securityID]);
                }
                else
                {
                    marketExe.DownUp = CommonConstants.NO_UP_DOWN;
                    marketExe.DownUpBrush = "0";
                    marketExe.Bp = CommonConstants.NO_UP_DOWN;
                    marketExe.NetPriceSpread = CommonConstants.NO_UP_DOWN;
                }
            }
        }

        /// <summary>
        /// 按照时间倒序添加数据
        /// </summary>
        /// <param name="addOrUpdate"></param>
        /// <param name="item"></param>
        private void AddOrUpdateDataToList_Sort(bool isUpdateData, ObservableCollection<MarketExecutionsItem> list, MarketExecutionsItem item, int left, int right)
        {
            string securityID = item.SecurityID;
            int half = left + (right - left) / 2;

            if (list.Count == 0)
            {
                list.Insert(0, item);
            }
            else if (left == right)
            {
                MarketExecutionsItem halfItem = list.ElementAt(half);
                if (isUpdateData == true)
                {
                    int oldIndex = list.IndexOf(item);
                    if (item.TimeStamp != null && item.TimeStamp.CompareTo(halfItem.TimeStamp) >= 0)
                    {
                        list.Move(oldIndex, left);
                    }
                    else
                    {
                        list.Move(oldIndex, left + 1);
                    }
                }
                else
                {
                    if (item.TimeStamp != null && item.TimeStamp.CompareTo(halfItem.TimeStamp) >= 0)
                    {
                        list.Insert(left, item);
                    }
                    else
                    {
                        list.Insert(left + 1, item);
                    }
                }

            }
            else
            {
                MarketExecutionsItem halfItem = list.ElementAt(half);
                if (item.TimeStamp != null && item.TimeStamp.CompareTo(halfItem.TimeStamp) >= 0)
                {
                    AddOrUpdateDataToList_Sort(isUpdateData, list, item, left, half);
                }
                else
                {
                    AddOrUpdateDataToList_Sort(isUpdateData, list, item, half + 1, right);
                }
            }
        }


        public string ConvertSide(string side)
        {
            if (side == "2")
            {
                return "Y";
            }
            else if (side == "1")
            {
                return "X";
            }

            return side;
        }
        #endregion

        #region 更新 HOLC
        public void ResetHOLC()
        {
            lock (lockObj)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    OpenYld = LowYield = OrderCount = HighYield = CloseYield = CommonConstants.NO_PRICE;
                    OpenNetPx = LowNetPx = HighNetPx = CloseNetPx = CommonConstants.NO_PRICE;
                }));
            }
        }

        private void RefreshHOLC(OhlcMsg ohlcMsg)
        {
            //if (LocalEnv.ConfInfo.IsUseOHLCSvr == "1")
            //{
                lock (lockObj)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (!string.IsNullOrEmpty(ohlcMsg.inf2))
                            CloseYield = CommonUtils.FormatNum(ohlcMsg.inf2);

                        HighYield = CommonUtils.FormatNum(ohlcMsg.high.ToString());

                        LowYield = CommonUtils.FormatNum(ohlcMsg.low.ToString());

                        OpenYld = CommonUtils.FormatNum(ohlcMsg.open.ToString());

                        if (!string.IsNullOrEmpty(ohlcMsg.inf3))
                            CloseNetPx = CommonUtils.FormatNum(ohlcMsg.inf3);
                        HighNetPx = CommonUtils.FormatNum(ohlcMsg.highNetPrice.ToString());

                        LowNetPx = CommonUtils.FormatNum(ohlcMsg.lowNetPrice.ToString());

                        OpenNetPx = CommonUtils.FormatNum(ohlcMsg.openNetPrice.ToString());

                        if (!string.IsNullOrEmpty(ohlcMsg.inf1))
                            OrderCount = PriceCalHelper.Instance.RemoveZero(ohlcMsg.inf1);
                    }));
                }
            //}
        }

        //private void RefreshHOLC()
        //{
        //    if (LocalEnv.ConfInfo.IsUseOHLCSvr == "2")
        //    {
        //        lock (lockObj)
        //        {
        //            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //            {
        //                if (_LastExecutionDic.ContainsKey(this.SearchText))
        //                {
        //                    MarketExecutionsItem mi = _LastExecutionDic[this.SearchText];
        //                    if (mi == null)
        //                        return;

        //                    if (JsonSplit.IsJson(mi.Info1))
        //                    {
        //                        MarketKLine market = JsonConvert.DeserializeObject<MarketKLine>(mi.Info1);

        //                        if (!string.IsNullOrEmpty(market.prev_close_yield))
        //                            CloseYield = CommonUtils.FormatNum(market.prev_close_yield);

        //                        if (!string.IsNullOrEmpty(market.high_yield))
        //                            HighYield = CommonUtils.FormatNum(market.high_yield);

        //                        OrderCount = PriceCalHelper.Instance.RemoveZero(GetPrevVenuesTotalVolume().ToString());

        //                        if (!string.IsNullOrEmpty(market.low_yield))
        //                            LowYield = CommonUtils.FormatNum(market.low_yield);

        //                        if (!string.IsNullOrEmpty(market.open_yld))
        //                            OpenYld = CommonUtils.FormatNum(market.open_yld);
        //                    }
        //                }
        //            }));
        //        }
        //    }
        //}

        #endregion

        #region 处理Favorites
        private void HomePageDataHandler_OnFavoritesChanged(HomePageDataHandler.FavoriteTypeEnum type, string groupID, string securityID)
        {
            lock (updlock)
            {
                if (string.IsNullOrEmpty(SearchText) && !string.IsNullOrEmpty(SearchText))
                    return;

                if ((type == FavoriteTypeEnum.AddBond || type == FavoriteTypeEnum.DelBond) || (type == FavoriteTypeEnum.DelGroup || type == FavoriteTypeEnum.BatchImport))
                {
                    Visibility showStar = HomePageDataHandler.IsFavorite(securityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                    if (DS.Count > 0)
                    {
                        for (int i = DS.Count - 1; i >= 0; i--)
                        {
                            MarketExecutionsItem marketExe = DS.ElementAt(i);
                            marketExe.ShowStar = showStar;
                            _LastExecutionDic.AddOrUpdate(marketExe.SecurityID, marketExe, (o, e) => marketExe);
                            //if (i == 0)
                            //{
                            //    ExecutionsHandler.Instance.PublishLastNewMarketExecutions(marketExe);
                            //}
                        }
                    }
                }
            }
        }
        #endregion
        
    }
}