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
using STC.Frame.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.ViewModel.Data;
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

namespace STC.XBond.Plugin.ViewModel
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

        public LastExecutionViewModel()
        {
            this.Init();
        }

        private void Init()
        {
            try
            {
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

                if (_ProcessMarketExecutioinsThreadQueue == null)
                {
                    _ProcessMarketExecutioinsThreadQueue = new AsyncThreadQueue<Common.Model.Fix.Message.ExecutionReport>(UpdateMarketExecutions);
                }

                BaseHandler.Instance.Init(Instance_OnClear);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }


        #region 手绘15分钟成交图形插件处理功能
        /// <summary>
        /// 列表按钮
        /// </summary>
        private bool listFloatButtonChecked = true;
        public bool ListFloatButtonChecked
        {
            get
            {
                return listFloatButtonChecked;
            }

            set
            {
                if (listFloatButtonChecked != value)
                {

                    listFloatButtonChecked = value;
                    ChatFloatButtonChecked = !value;
                    this.OnPropertyChanged(nameof(ListFloatButtonChecked));
                }
            }
        }

        /// <summary>
        /// 图表按钮
        /// </summary>
        private bool chatFloatButtonChecked = false;
        public bool ChatFloatButtonChecked
        {
            get
            {
                return chatFloatButtonChecked;
            }

            set
            {
                if (chatFloatButtonChecked != value)
                {
                    chatFloatButtonChecked = value;
                    ListFloatButtonChecked = !value;
                    this.OnPropertyChanged(nameof(ChatFloatButtonChecked));
                }
            }
        }

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


        public void SetItemSource(string securityId)
        {
            this.SearchText = securityId;

            lock (updlock)
            {
                if (IsRegSvr)
                    return;

                IsRegSvr = true;
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!string.IsNullOrEmpty(securityId) && securityId != "xxxxxx")
                {
                    lock (updlock)
                    {
                        this.ClearData();
                        //获取历史数据(Max 200,由于订阅是全部推送，因而此处取消)
                        //DS = new ObservableCollection<MarketExecutionsItem>();
                        DS = new ObservableCollection<MarketExecutionsItem>(LastExecutionProcesser.Processer.GetDatasBySecurityID(securityId).OrderByDescending(o => o.TimeStamp).ToList());
                        UpdateDS();

                        SortDescription timeDesc = new SortDescription("TimeStamp", ListSortDirection.Descending);
                        this.filteredMarketExecutions.SortDescriptions.Add(timeDesc);
                        this.filteredMarketExecutions.IsLiveFilteringRequested = true;
                        this.filteredMarketExecutions.IsLiveSortingRequested = true;
                        this.filteredMarketExecutions.Source = DS;
                        // 使用成交 info1
                        if (LocalEnv.ConfInfo.IsUseOHLCSvr == "2")
                        {
                            RefreshHOLC();
                        }
                        else if (LocalEnv.ConfInfo.IsUseOHLCSvr == "1")
                        {
                            // 使用订阅服务获取
                            OhlcHandler.Instance.SubscribeOhlc(securityId, Instance_OnOhlcMsg);
                        }

                        RegSvr();

                        // 绘制15分钟ChartLine图
                        UpdateChartLineData();
                    }
                }
            }));
        }

        private void RegSvr()
        {
            //订阅
            if (!IsRegSvr)
            {
                ExecutionsHandler.Instance.OnMarketExecutionsOnlyNew += Instance_OnMarketExecutions;
                ExecutionsHandler.Instance.OnOwnExecutionsOnlyNew += Instance_OnOwnExecutions;
            }
        }

        private void UnRegSvr()
        {
            //取消订阅
            if (IsRegSvr)
            {
                ExecutionsHandler.Instance.OnMarketExecutionsOnlyNew -= Instance_OnMarketExecutions;
                ExecutionsHandler.Instance.OnOwnExecutionsOnlyNew -= Instance_OnOwnExecutions;
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
                            marketExe.DownUp = Constants.NO_UP_DOWN;
                            marketExe.DownUpBrush = "0";
                            marketExe.Bp = Constants.NO_UP_DOWN;
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
                        //marketExe.Info1 = currentvolume.ToString();
                        _LastExecutionDic.AddOrUpdate(marketExe.SecurityID, marketExe, (o, e) => marketExe);
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

                marketExe.DownUp = bp > 0 ? Constants.UP : bp < 0 ? Constants.DOWN : Constants.NO_UP_DOWN;
                marketExe.DownUpSort = bp > 0 ? 1 : bp < 0 ? 2 : 0;
                marketExe.DownUpBrush = bp > 0 ? "1" : bp < 0 ? "2" : "0";

                if (marketExe.DownUpSort == 1)
                {
                    marketExe.Bp = "+" + Utils.CommonUtils.FormatNum(Bp);
                }
                else if (marketExe.DownUpSort == 2)
                {
                    marketExe.Bp = Utils.CommonUtils.FormatNum(Bp);
                }
                else
                {
                    marketExe.Bp = Constants.NO_UP_DOWN;
                }
            }
        }

        #endregion

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("MarketOrderViewModel Clear, security:{0}", this.SearchText));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                ClearData();
            }
        }

        public void ClearData()
        {
            if (IsRegSvr)
            {
                UnRegSvr();
            }

            try
            {
                lock (updlock)
                {
                    if (_ProcessMarketExecutioinsThreadQueue != null)
                        _ProcessMarketExecutioinsThreadQueue.Clear();
                    if  (this.DS !=null)
                        this.DS.Clear();
                      _LastExecutionDic.Clear();
                    LastKlineDic.Clear();
                    ChartLineData.Clear();
                }

                lock (this.lockObj)
                {
                    ResetHOLC();
                    //to do 重置绘图
                    ResetChartLine();
                }
            }
            catch { }
        }

        private AsyncThreadQueue<Common.Model.Fix.Message.ExecutionReport> _ProcessMarketExecutioinsThreadQueue;
        private object lockObj = new object();
        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj.SecurityID == this.SearchText)
            {
                lock (updlock)
                {
                    //UpdateMarketExecutions(obj);
                    if (_ProcessMarketExecutioinsThreadQueue != null)
                    {
                        this._ProcessMarketExecutioinsThreadQueue.Enqueue(obj);
                    }
                }
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
                lock (updlock)
                {
                    if (_ProcessMarketExecutioinsThreadQueue != null)
                    {
                        this._ProcessMarketExecutioinsThreadQueue.Enqueue(obj);
                    }
                }
            }
        }

        #region 手绘15分钟成交图形处理

        public void UpdateChartLineData()
        {
            if (LinePageInstance != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                     lock (updlock)
                     {
                        ChartLineData = new ObservableCollection<ChartModel>();
                        if (DS.Count > 0)
                        {
                            for (int i = DS.Count - 1; i >= 0; i--)
                            {
                                ChartModel tmpData = GetLineChartModel(DS.ElementAt(i));
                                if (tmpData.Time != null)
                                {
                                    ChartLineData.Add(tmpData);
                                }
                            }
                            string s = JsonConvert.SerializeObject(ChartLineData);
                            ChartChangeDataSource?.Invoke(LinePageInstance, new object[] { ChartLineData, this.SearchText });
                        }
                     }
                 }));
            }
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
        private void UpdateLineChart(MarketExecutionsItem obj)
        {
            if (LinePageInstance != null)
            {
                ChartModel tmpData = GetLineChartModel(obj);
                if (tmpData.Time != null)
                {
                    ChartUpdateChart?.Invoke(LinePageInstance, new object[] { tmpData });
                }
            }
        }

        /// <summary>
        /// 处理 LineChart 数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private ChartModel GetLineChartModel(MarketExecutionsItem obj)
        {
            ChartModel tmpData = new ChartModel();
            string tmpstr = obj.Info1;
            if (JsonSplit.IsJson(tmpstr))
            {
                MarketKLine market = JsonConvert.DeserializeObject<MarketKLine>(tmpstr);
                if (market.total_num_trades > 0)
                {
                    tmpData.Volumn = int.Parse(Math.Ceiling(obj.Volume).ToString());
                }
                else
                {
                    tmpData.Volumn = 0;
                }

                tmpData.Time = DateTime.Parse(obj.Time).ToString("HH:mm:ss");
                tmpData.Price = obj.ExecutePricePercent;
            }

            return tmpData;
        }

        #endregion

        #region 更新成交趋势
        /// <summary>
        /// 更新数据， 好像没有删除数据？？还有很多字段不知道什么意思??key也不确定
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

                    if (DS.Any<MarketExecutionsItem>(orderItem => (orderItem.Key == key)))
                    {
                        var result = DS.Where(orderItem => orderItem.Key == key);
                        foreach (MarketExecutionsItem item in result)
                        {
                            if (item.Time.CompareTo(obj.TransactTime) > 0)
                            {
                                // 如果时间早， 就不更新了。
                                return;
                            }
                            else
                            {
                                marketExe = item;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // 如果状态是4， 删除这笔单子。
                        if (obj.OrdStatus == "4")
                        {
                            return;
                        }

                        //if (DS.Count == 200)
                        //{
                        //    MarketExecutionsItem oldItem = DS.Last();
                        //    if (oldItem.Time.CompareTo(obj.TransactTime) < 0)
                        //    {
                        //        DS.Remove(oldItem);
                        //    }
                        //    else
                        //    {
                        //        return;
                        //    }
                        //}

                        marketExe = new MarketExecutionsItem();
                        marketExe.Key = key;
                        marketExe.Side = ConvertSide(obj.Side); // 方向
                        marketExe.SecurityID = obj.SecurityID; // 代码

                        marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
                        marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.Yield); // 成交价
                        marketExe.SourceTrader = obj.Venues; // 来源
                        marketExe.Time = obj.TransactTime;// 时间
                        marketExe.SettlType = obj.SettlType; // 清算速度
                        marketExe.Info1 = obj.Info1;
                        marketExe.TimeStamp = obj.UpdateTime;
                        marketExe.IsValid = obj.Vs;
                        marketExe.IsValid = marketExe.IsValid == null || marketExe.IsValid == "Y" || marketExe.IsValid == "" ? "Y" : "N";
                        marketExe.VenueTypeGW = obj.VenueTypeGW;
                        //星星状态
                        //marketExe.ShowStar = _CacheBondList.Any(p => p.securityid == marketExe.SecurityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        marketExe.ShowStar = HomePageDataHandler.IsFavorite(marketExe.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                        UpdateOrderDp(obj.SecurityID, ref marketExe, obj);

                        decimal prevvolume = GetCurrentVenuesPrevVolume(marketExe);
                        decimal currentvolume = GetSimpleVenuesTotalVolume(marketExe);
                        // 更新为当前成交量
                        decimal totalvolume = UpdateLastKineDic(marketExe);
                        marketExe.Volume = currentvolume - prevvolume;
                        marketExe.Volume = marketExe.Volume < 0 ? 0 : marketExe.Volume;

                        //marketExe.Info2 = currentvolume.ToString();

                        DS.Insert(0,marketExe);

                        // 更新为当前成交量
                        _LastExecutionDic.AddOrUpdate(marketExe.SecurityID, marketExe, (o, e) => marketExe);

                        // 更新HOLC
                        RefreshHOLC();

                        // 绘制15分钟ChartLine图
                        UpdateLineChart(marketExe);
                        return;
                    }

                    if (marketExe != null)
                    {
                        // 如果状态是4， 删除这笔单子。
                        if (obj.OrdStatus == "4")
                        {
                            DS.Remove(marketExe);

                            // 更新LineChart
                            // UpdateChartLineData();

                            return;
                        }

                        marketExe.Key = key;
                        marketExe.Side = ConvertSide(obj.Side); // 方向
                        marketExe.SecurityID = obj.SecurityID; // 代码
                        marketExe.VenueTypeGW = obj.VenueTypeGW;

                        marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
                        marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.Yield); // 成交价
                        marketExe.SourceTrader = obj.Venues; // 来源
                        marketExe.SettlType = obj.SettlType; // 清算速度
                        marketExe.ExecuteAmount = CommonUtils.ConvertToDecimal(obj.LastQty);
                        marketExe.Info1 = obj.Info1;
                        marketExe.TimeStamp = obj.UpdateTime;
                        marketExe.IsValid = obj.Vs;
                        marketExe.IsValid = marketExe.IsValid == null || marketExe.IsValid == "Y" || marketExe.IsValid == "" ? "Y" : "N";
                        //LastPriceParam lastPriceParam = CalLastExe(marketExe, _LastExecutionDic[obj.SecurityID]);
                        //星星状态
                        //marketExe.ShowStar = _CacheBondList.Any(p => p.securityid == marketExe.SecurityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        marketExe.ShowStar = HomePageDataHandler.IsFavorite(marketExe.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                        if (marketExe.Time != obj.TransactTime)
                        {
                            marketExe.Time = obj.TransactTime;// 时间

                            decimal prevvolume = GetCurrentVenuesPrevVolume(marketExe);
                            decimal currentvolume = GetSimpleVenuesTotalVolume(marketExe);
                            // 更新为当前成交量
                            decimal totalvolume = UpdateLastKineDic(marketExe);
                            marketExe.Volume = currentvolume - prevvolume;
                            marketExe.Volume = marketExe.Volume < 0 ? 0 : marketExe.Volume;
                            //marketExe.Info2 = currentvolume.ToString();
                            DS.Insert(0, marketExe);

                            // 更新为当前成交量
                            _LastExecutionDic.AddOrUpdate(marketExe.SecurityID, marketExe, (o, e) => marketExe);

                            // 更新HOLC
                            RefreshHOLC();

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
                marketExe.DownUp = Constants.NO_UP_DOWN;
                marketExe.DownUpBrush = "0";
                marketExe.Bp = Constants.NO_UP_DOWN;
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
                    marketExe.DownUp = Constants.NO_UP_DOWN;
                    marketExe.DownUpBrush = "0";
                    marketExe.Bp = Constants.NO_UP_DOWN;
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
            if (side == "1")
            {
                return "Y";
            }
            else if (side == "2")
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
                }));
            }
        }

        private void RefreshHOLC(OhlcMsg info1)
        {
            if (LocalEnv.ConfInfo.IsUseOHLCSvr == "1")
            {
                lock (lockObj)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (!string.IsNullOrEmpty(info1.inf2))
                            CloseYield = CommonUtils.FormatNum(info1.inf2);

                        if (!string.IsNullOrEmpty(info1.high))
                            HighYield = CommonUtils.FormatNum(info1.high);

                        if (!string.IsNullOrEmpty(info1.inf1))
                            OrderCount = PriceCalHelper.Instance.RemoveZero(info1.inf1);

                        if (!string.IsNullOrEmpty(info1.low))
                            LowYield = CommonUtils.FormatNum(info1.low);

                        if (!string.IsNullOrEmpty(info1.open))
                            OpenYld = CommonUtils.FormatNum(info1.open);
                    }));
                }
            }
        }

        private void RefreshHOLC()
        {
            if (LocalEnv.ConfInfo.IsUseOHLCSvr == "2")
            {
                lock (lockObj)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (_LastExecutionDic.ContainsKey(this.SearchText))
                        {
                            MarketExecutionsItem mi = _LastExecutionDic[this.SearchText];
                            if (mi == null)
                                return;

                            if (JsonSplit.IsJson(mi.Info1))
                            {
                                MarketKLine market = JsonConvert.DeserializeObject<MarketKLine>(mi.Info1);

                                if (!string.IsNullOrEmpty(market.prev_close_yield))
                                    CloseYield = CommonUtils.FormatNum(market.prev_close_yield);

                                if (!string.IsNullOrEmpty(market.high_yield))
                                    HighYield = CommonUtils.FormatNum(market.high_yield);

                                OrderCount = PriceCalHelper.Instance.RemoveZero(GetPrevVenuesTotalVolume().ToString());

                                if (!string.IsNullOrEmpty(market.low_yield))
                                    LowYield = CommonUtils.FormatNum(market.low_yield);

                                if (!string.IsNullOrEmpty(market.open_yld))
                                    OpenYld = CommonUtils.FormatNum(market.open_yld);
                            }
                        }
                    }));
                }
            }
        }

        #endregion

    }
}