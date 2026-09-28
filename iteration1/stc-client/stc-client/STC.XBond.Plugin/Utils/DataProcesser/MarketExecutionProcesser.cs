using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.Position;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using static STC.Common.Utils.HomePageDataHandler;

namespace STC.XBond.Plugin.Utils.DataProcesser
{
    /// <summary>
    /// 市场成交
    /// </summary>
    public class MarketExecutionProcesser
    {
        /// <summary>
        /// MarketExecutionProcesser实例
        /// </summary>
        private static MarketExecutionProcesser proc = new MarketExecutionProcesser();

        public static MarketExecutionProcesser Processer
        {
            get
            {
                return proc;
            }
        }

        private volatile bool isInit = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        private MarketExecutionProcesser()
        {
            ExecutionsHandler.Instance.OnClear += Instance_OnClear;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~MarketExecutionProcesser()
        {
            Dispose();
        }

        private object lockObj = new object();
        /// <summary>
        /// 市场成交
        /// </summary>
        private ObservableCollection<MarketExecutionsItem> _myMarketExes = new ObservableCollection<MarketExecutionsItem>();
        public ObservableCollection<MarketExecutionsItem> MyMarketExes
        {
            get
            {
                lock (this.lockObj)
                {
                    BindingOperations.EnableCollectionSynchronization(_myMarketExes, lockObj);
                    return this._myMarketExes;
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
                if (_searchText != value)
                {
                    _searchText = value;

                    if (string.IsNullOrEmpty(_searchText))
                    {
                        lock (this.lockObj)
                        {
                            TmpMarketExes.Clear();
                        }
                    }
                    else
                    {
                        //List<MarketExecutionsItem> temp = STC.Common.Data.STCRocksDbHandler.Instance.GetAllByKey(STC.Common.Data.STCRocksDbHandler.DbMarketExecution, _searchText).OrderBy(p => p.Time).ToList();
                        //foreach (var item in temp)
                        //{
                        //    TmpMarketExes.Add(item);
                        //}

                        List<ExecutionReport> tmpER = STC.Common.Data.ExecutionsHandler.Instance.GetLocalExecutionsBySecurityID(_searchText, false);
                        //CommonUtils.WriteConvertObjectToJsonToText(temp, "2.txt");
                        foreach (var item in tmpER)
                        {
                            QueryERQueue.Enqueue(item);
                            QueryErResetEvent.Set();
                        }
                    }
                }
            }
        }

        private ObservableCollection<MarketExecutionsItem> _tmpMarketExes = new ObservableCollection<MarketExecutionsItem>();
        /// <summary>
        /// 市场成交搜索用数据源
        /// </summary>
        public ObservableCollection<MarketExecutionsItem> TmpMarketExes
        {
            get
            {
                lock (this.lockObj)
                {
                    BindingOperations.EnableCollectionSynchronization(_tmpMarketExes, lockObj);
                    return this._tmpMarketExes;
                }
            }
        }

        private const string SECURITYID_FIELD = "SecurityID";
        private const string TRANSACTIONTIME_FIELD = "TransactionTime";
        private const string EXEID_FIELD = "ExeID";

        private List<Bond> _CacheBondList = new List<Bond>();

        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //更新最新成交字典，含内部成交
        private ConcurrentQueue<ExecutionReport> MeQueue = new ConcurrentQueue<ExecutionReport>();
        //最后更新时间
        private DateTime LastUpdateTime;

        private ManualResetEventSlim _autoResetEvent = new ManualResetEventSlim(false);
        
        //市场成交用
        private ManualResetEventSlim ErResetEvent = new ManualResetEventSlim(false);

        //更新显示用的市场成交 
        //private ThreadQueue<Common.Model.Fix.Message.ExecutionReport> ProcessErThreadQueue;
        private ConcurrentQueue<ExecutionReport> ERQueue = new ConcurrentQueue<ExecutionReport>();


        //查询指定债券的市场成交
        private ConcurrentQueue<ExecutionReport> QueryERQueue = new ConcurrentQueue<ExecutionReport>();
        //查询指定债券的市场成交用
        private ManualResetEventSlim QueryErResetEvent = new ManualResetEventSlim(false);

        public Dictionary<string, string> DictMarketVenue = new Dictionary<string, string>();
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (isInit == false && LocalEnv.ServerInfo.AppName.ToLower() != "bdb")
            {

                HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;

                BindingOperations.EnableCollectionSynchronization(_myMarketExes, lockObj);

                //更新最新成交字典，含内部成交
                Task.Factory.StartNew(() => DoProcMeQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

                //更新显示用的市场成交
                Task.Factory.StartNew(() => DoProcERQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

                //查询指定债券的市场成交用
                Task.Factory.StartNew(() => DoProcQueryERQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

                //if (ProcessErThreadQueue == null)
                //{
                //    ProcessErThreadQueue = new ThreadQueue<Common.Model.Fix.Message.ExecutionReport>(UpdateMarketExecutions);
                //}


                //读取历史行情数据
                GetNewData();


                #region 收MarketExec消息
                ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                #endregion

                isInit = true;

                DictMarketVenue["CMDS"] = "CMDS";
                DictMarketVenue["CDH"] = "CDH";
                DictMarketVenue["SDN"] = "SDN";
                DictMarketVenue["SZE"] = "SZE";
                DictMarketVenue["SSE"] = "SSE";
                DictMarketVenue["SSF"] = "SSF";
                DictMarketVenue["SZF"] = "SZF";
            }
        }


        public async void GetNewData()
        {
            List<ExecutionReport> lst = await RequestMarketExecutionsSortExecid(null, null);
            if (lst != null)
            {
                foreach (ExecutionReport obj in lst)
                {
                    if (obj != null)
                    {
                        //更新市场成交
                        ERQueue.Enqueue(obj);
                        ErResetEvent.Set();
                        //更新最后成交
                        MeQueue.Enqueue(obj);
                        _autoResetEvent.Set();
                    }
                    //ProcessErThreadQueue.Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// 处理收藏功能
        /// </summary>
        /// <param name="type"></param>
        /// <param name="groupID"></param>
        /// <param name="securityID"></param>
        private void HomePageDataHandler_OnFavoritesChanged(HomePageDataHandler.FavoriteTypeEnum type, string groupID, string securityID)
        {
            lock (this.lockObj)
            {
                if (type == FavoriteTypeEnum.AddBond
                    || type == FavoriteTypeEnum.DelBond)
                {
                    Visibility showStar = HomePageDataHandler.IsFavorite(securityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                    foreach (MarketExecutionsItem item in _myMarketExes.Where(o => { return o.SecurityID == securityID; }))
                    {
                        item.ShowStar = showStar;
                    }
                }
                else if (type == FavoriteTypeEnum.DelGroup
                    || type == FavoriteTypeEnum.BatchImport)
                {
                    Dictionary<string, Visibility> showStarDic = new Dictionary<string, Visibility>();

                    foreach (MarketExecutionsItem item in _myMarketExes)
                    {
                        if (showStarDic.ContainsKey(item.SecurityID) == false)
                        {
                            Visibility showStar = HomePageDataHandler.IsFavorite(item.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;
                            showStarDic[item.SecurityID] = showStar;

                        }
                        item.ShowStar = showStarDic[item.SecurityID];
                    }
                }
            }
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            HomePageDataHandler.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;

            ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;
            //TBookHandler.Instance.UnSubscribeBond(Instance_OnBondList);
            
            ExecutionReport collect;
            while (MeQueue.TryDequeue(out collect))
            {
            }

            while (ERQueue.TryDequeue(out collect))
            {
            }

            while (QueryERQueue.TryDequeue(out collect))
            {
            }

            lock (this.lockObj)
            {
                if (_myMarketExes != null)
                {
                    _myMarketExes.Clear();
                }
            }

            //if (ProcessErThreadQueue != null)
            //{
            //    ProcessErThreadQueue.Clear();
            //}

            isInit = false;
        }

        /// <summary>
        /// 释放数据
        /// </summary>
        public void Dispose()
        {
            this.Clear();

            ExecutionReport collect;

            cancellationTokenSource.Cancel();

            while (MeQueue.TryDequeue(out collect))
            { 
            }

            while (ERQueue.TryDequeue(out collect))
            {
            }

            while (QueryERQueue.TryDequeue(out collect))
            {
            }

            lock (this.lockObj)
            {
                //if (countDic != null)
                //{
                //    countDic.Clear();
                //    countDic = null;
                //}

                //if (_PrimaryKeyDV != null)
                //{
                //    _PrimaryKeyDV.Table.Clear();
                //    _PrimaryKeyDV.Table.Dispose();
                //    _PrimaryKeyDV.Dispose();
                //    _PrimaryKeyDV = null;
                //}

                if (_myMarketExes != null)
                {
                    _myMarketExes.Clear();
                    _myMarketExes = null;
                }
            }

            //if (ProcessErThreadQueue != null)
            //{
            //    ProcessErThreadQueue.Clear();
            //    ProcessErThreadQueue.Dispose();
            //    ProcessErThreadQueue = null;
            //}


            isInit = false;
        }

        /// <summary>
        /// 清零数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("MarketExecutionProcesser Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    this.Init();
                }));
            }
        }

        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj != null)
            {
                //更新市场成交
                ERQueue.Enqueue(obj);
                ErResetEvent.Set();
                //更新最后成交
                MeQueue.Enqueue(obj);
                _autoResetEvent.Set();
            }
            
            //ProcessErThreadQueue.Enqueue(obj);
        }

        /// <summary>
        /// 收取Bond更新数据
        /// </summary>
        /// <param name="bondList"></param>
        private void Instance_OnBondList(List<Bond> bondList)
        {
            lock (_CacheBondList)
            {
                foreach (Bond bond in bondList)
                {
                    _CacheBondList.RemoveAll(orderItem => (orderItem.securityid == bond.securityid
                                                           && orderItem.settltype == bond.settltype
                                                           && orderItem.venues == bond.venues
                                                           && orderItem.venuetypegw == bond.venuetypegw));
                    if (bond.action != "0")
                    {
                        _CacheBondList.Add(bond);
                    }
                }
            }

            lock (this.lockObj)
            {
                foreach (MarketExecutionsItem item in this._myMarketExes)
                {
                    UpdateStarStatus(item);
                }
            }
        }

        /// <summary>
        /// 更新收藏标记
        /// </summary>
        /// <param name="item"></param>
        private void UpdateStarStatus(MarketExecutionsItem item)
        {
            string securityID = item.SecurityID;
            lock (_CacheBondList)
            {
                //item.ShowStar = _CacheBondList.Any(p => p.securityid == securityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                item.ShowStar = HomePageDataHandler.IsFavorite(securityID) == "1" ? Visibility.Visible : Visibility.Hidden;
            }
        }


        /// <summary>
        /// 更新最新成交字典，含内部成交
        /// </summary>
        private void DoProcMeQueue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 等待数据可用
                    _autoResetEvent.Wait();

                    ExecutionReport data;

                    while (MeQueue.TryDequeue(out data))
                    {
                        try
                        {
                            if (data.OrdStatus == "4")
                                //更新最新成交字典，获取上一个有效的成交作为最后成交
                                Common.DataProcesser.LastExecutionProcesser.Processer.DeleteLastERAndGetPrevER(data);
                            else
                                //更新最新成交字典
                                Common.DataProcesser.LastExecutionProcesser.Processer.UpdateLastExecutionProcesserLastExecution(data);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex.Message);
                        }
                    }

                    // 重置事件
                    _autoResetEvent.Reset();
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

        private object updlock = new object();
        
        /// <summary>
        /// 更新市场成交
        /// </summary>
        /// <param name="obj">原始报文</param>
        private void UpdateMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            try
            {
                if (!DictMarketVenue.ContainsKey(obj.VenueTypeGW))
                    return;

                // 如果状态是4， 删除这笔单子。
                if (obj.OrdStatus == "4")
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        // 删除
                        DeleteRecord(obj);
                    }));

                    return;
                }

                //显示数据

                MarketExecutionsItem result = null;
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    result = _myMarketExes.Where(orderItem => orderItem.Key == obj.ExecID)?.FirstOrDefault();

                    if (result == null)
                    {
                        // 添加新数据
                        AddRecord(obj);
                    }
                    else
                    {
                        string transactionTime = CommonUtils.ConvertToString(result.TimeStamp);
                        DateTime orgdatetime = DateTimeEx.ConvertStringToDateTime(transactionTime);
                        DateTime curUpdateTime = DateTimeEx.ConvertStringToDateTime(obj.UpdateTime);
                        if (DateTime.Compare(orgdatetime, curUpdateTime) > 0)
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }
                        // 更新数据
                        UpdateRecord(obj);

                    }
                }));
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }

        /// <summary>
        /// 删除execID的记录
        /// </summary>
        /// <param name="execID"></param>
        private void DeleteRecord(Common.Model.Fix.Message.ExecutionReport obj)
        {
            string execID = obj.ExecID;
            MarketExecutionsItem marketExe = _myMarketExes.Where(orderItem => orderItem.Key == execID)?.FirstOrDefault();
            if (marketExe != null)
            {
                _myMarketExes.Remove(marketExe);
            }


            if (!string.IsNullOrEmpty(this.SearchText))
            {
                MarketExecutionsItem result2 = _tmpMarketExes.Where(orderItem => orderItem.Key == execID)?.FirstOrDefault();
                if (result2 != null)
                {
                    _tmpMarketExes.Remove(result2);
                }

            }

            //string src = STC.Common.Data.STCRocksDbHandler.Instance.Get(STC.Common.Data.STCRocksDbHandler.DbMarketExecution, $"{obj.SecurityID}#{obj.ExecID}");
            //if (!string.IsNullOrEmpty(src))
            //    STCRocksDbHandler.Instance.Delete(STC.Common.Data.STCRocksDbHandler.DbMarketExecution, $"{obj.SecurityID}#{obj.ExecID}");

        }

        /// <summary>
        /// 更新记录内容
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateRecord(Common.Model.Fix.Message.ExecutionReport obj)
        {
            MarketExecutionsItem marketExe = null;
            //显示数据
            marketExe = _myMarketExes.Where(orderItem => orderItem.Key == obj.ExecID)?.FirstOrDefault();
  
            if (marketExe != null)
            {
                ConvertExecutionReportToMarketExecutionsItem(obj, ref marketExe);
                //更新最新成交字典
                //Common.DataProcesser.LastExecutionProcesser.Processer.UpdateLastExecutionProcesserLastExecution(obj, marketExe);
            }

            if (!string.IsNullOrEmpty(this.SearchText))
            {
                if (obj.SecurityID == this.SearchText)
                {
                    MarketExecutionsItem result2 = _tmpMarketExes.Where(orderItem => orderItem.Key == obj.ExecID)?.FirstOrDefault();
                    if (result2 != null)
                    {
                        ConvertExecutionReportToMarketExecutionsItem(obj, ref result2);
                        //更新最新成交字典
                        //Common.DataProcesser.LastExecutionProcesser.Processer.UpdateLastExecutionProcesserLastExecution(obj, result2);
                    }
                }
            }
        }

        private string _lastupdate = string.Empty;
        public bool _isScrollToEnd = false;

        /// <summary>
        /// 添加新记录
        /// </summary>
        /// <param name="obj"></param>
        private void AddRecord(Common.Model.Fix.Message.ExecutionReport obj)
        {
            MarketExecutionsItem marketExe = new MarketExecutionsItem();
            ConvertExecutionReportToMarketExecutionsItem(obj, ref marketExe);
            marketExe.Key = marketExe.ExecID;

            if (!string.IsNullOrEmpty(this.SearchText))
            {
                if (marketExe.SecurityID == this.SearchText)
                {
                    _tmpMarketExes.Add(marketExe);
                }
            }

            _lastupdate = _lastupdate ?? marketExe.TimeStamp;

            string transactionTime = CommonUtils.ConvertToString(_lastupdate);

            if (_myMarketExes.Count < 100)
            {
                _myMarketExes.Add(marketExe);
                if (transactionTime.CompareTo(marketExe.TimeStamp) < 0)
                {
                    _lastupdate = marketExe.TimeStamp;
                }
            }
            else
            {
                if (transactionTime.CompareTo(marketExe.TimeStamp) < 0)
                {
                    // 如果时间新
                    _myMarketExes.Add(marketExe);
                    _lastupdate = marketExe.TimeStamp;
                }
            }

            if (!_isScrollToEnd)
            {
                if (_myMarketExes.Count > 100)
                {
                    //MarketExecutionsItem ls = _myMarketExes.OrderByDescending(o => o.Time)?.LastOrDefault();

                    //MarketExecutionsItem lastrecc = _myMarketExes.Where(o=>o.ExecID == ls.ExecID)?.FirstOrDefault();
                    MarketExecutionsItem lastrecc = _myMarketExes?.FirstOrDefault();
                    if (lastrecc != null)
                    {
                        _myMarketExes.Remove(lastrecc);
                    }
                }
            }
        }

        /// <summary>
        /// 请求指定时间之前的市场成交
        /// </summary>
        /// <param name="key"></param>
        public async void GetRocksDbDataByItemKey(string key)
        {
            try
            {
                List<ExecutionReport> lst = await RequestMarketExecutionsSortExecid(key);
                _isScrollToEnd = true;
                ////string s = CommonUtils.SerializeObject(lst);
                ////string fname = "data/" + DateTime.Now.ToString("yyyyMMddHHmmssfff")+".txt";
                ////fname = Path.Combine(LocalEnv.ConfInfo.DataPath ,fname);
                ////FileHelper.WriteFullText(fname, s );
                if (lst != null)
                {
                    foreach (ExecutionReport obj in lst)
                    {
                        lock (lockObj)
                        {
                            MarketExecutionsItem result = _myMarketExes.Where(orderItem => orderItem.Key == obj.ExecID)?.FirstOrDefault();
                            if (result == null)
                            {
                                MarketExecutionsItem mi = new MarketExecutionsItem();
                                ConvertExecutionReportToMarketExecutionsItem(obj, ref mi);
                                mi.Key = mi.ExecID;
                                _myMarketExes.Add(mi);
                            }
                            else
                            {
                                ConvertExecutionReportToMarketExecutionsItem(obj, ref result);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }

        private void ConvertExecutionReportToMarketExecutionsItem(ExecutionReport obj, ref MarketExecutionsItem marketExe)
        {
            try
            {
                marketExe.ExecID = obj.ExecID;
                marketExe.Key = obj.ExecID;
                marketExe.OrderID = obj.OrderID;
                marketExe.Side = obj.Side; // 方向
                marketExe.SecurityID = obj.SecurityID; // 代码
                marketExe.MarketIndicator = obj.MarketIndicator;
                if (obj.SecurityID.EndsWith(".SH") || obj.SecurityID.EndsWith(".SZ"))
                {
                    marketExe.ExecutePricePercent_Display = CommonUtils.FormatPriceN3(obj.NetPrice); //用于显示成交价
                    marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.NetPrice); // 成交价
                }
                else
                {
                    marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.Yield); //用于显示成交价
                    marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.Yield); // 成交价
                    marketExe.SettlType = obj.SettlType;
                }

                marketExe.SourceTrader = obj.Venues; // 来源
                marketExe.Time = obj.TransactTime;// 时间
                marketExe.SettlType = obj.SettlType; // 清算速度
                if ((obj.SettlType ?? "").Contains('.'))
                {
                    marketExe.SettlType_yuan = obj.SettlType;
                }
                else
                {
                    marketExe.SettlType_yuan = "";
                }

                marketExe.IsValid = obj.Vs == null || obj.Vs == "Y" || obj.Vs == "" ? "Y" : "N";
                marketExe.IsOwnInst = ("INNER".Equals(obj.VenueTypeGW) || "CDH".Equals(obj.VenueTypeGW) || "CMDS".Equals(obj.VenueTypeGW)
                || "SDN".Equals(obj.VenueTypeGW) || "SSE".Equals(obj.VenueTypeGW)
                || "SSF".Equals(obj.VenueTypeGW) || "SZE".Equals(obj.VenueTypeGW)) ? "N" : "Y";
                if ("Y".Equals(marketExe.IsOwnInst))
                {
                    marketExe.OwnInstTip = "本机构";
                }

                marketExe.Info1 = obj.Info1;
                marketExe.TimeStamp = obj.UpdateTime;
                marketExe.Info2 = obj.Info2;
                marketExe.Info10 = obj.Info10;
                marketExe.VenueTypeGW = obj.VenueTypeGW;

                if (obj.SecurityID.EndsWith(".SH") || obj.SecurityID.EndsWith(".SZ") || "5".Equals(obj.MarketIndicator ?? "4"))
                    marketExe.NetPrice_Display = CommonUtils.FormatNum3(obj.NetPrice);
                else
                    marketExe.NetPrice_Display = CommonUtils.FormatPrice(obj.NetPrice);

                marketExe.NetPrice = CommonUtils.ConvertToDecimal(obj.NetPrice);
                marketExe.LastQty = string.IsNullOrEmpty(obj.LastQty) ? "--" : ("0".Equals(obj.LastQty) ? "--" : obj.LastQty);
                marketExe.DownUp = CommonConstants.NO_UP_DOWN;
                marketExe.Bp = CommonConstants.NO_UP_DOWN;

                UpdateStarStatus(marketExe);//星星状态
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }

        /// <summary>
        /// 请求指定时间之前的市场成交
        /// </summary>
        public async Task<List<ExecutionReport>> RequestMarketExecutionsSortExecid(string key, string sortRule = "2")
        {
            List<ExecutionReport> res = new List<ExecutionReport>();
            await Task.Run(() =>
            {
                try
                {
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.dealTimeBegin = today;
                    request.dealTimeEnd = today;
                    //request.securityid = "180210.IB";
                    //request.venuetypegw = "CDH,CMDS,SDN,INNER";
                    request.venues = "CCTB,CNEX,PTCN,CTTJ,TPSC,UEDA,ODM,QDMESP,MANUAL,SSE,SSF,SZE,SZF";
                    //request.venues = "SSEMD,SSFMD,SZEMD";
                    //request.venuetypegw = "CMDS,INNER";
                    //request.venues = "QDMESP,MANUAL";
                    //request.venuetypegw = "CDH";
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    request.orderStatus = "1,2";
                    request.transactTime = key;
                    request.sortRule = sortRule;
                    int index = 0;
                    request.pageSize = 200;
                    request.pageNum = index;
 
                    TBookOrder reply = ApiManager.Instance.GetDefaultApi().Request<TBookOrder>(ApiConsts.EMSSVR, ApiConsts.Topic_stc_ems_execOrderQuery, request); 
                        ///获取成交
                    if (reply != null && reply.datas != null)
                    {
                        foreach (Common.Model.Fix.Message.ExecutionReport obj in reply.datas)
                        {
                            if ("0".Equals(obj.TimeInForce) && "INNER".Equals(obj.VenueTypeGW))
                            {
                                continue;
                            }

                            res.Add(obj);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestExecutions", e);
                }
            });


            return res.OrderBy(o => o.TransactTime).ToList();
        }


        #region 查询指定的债券
        /// <summary>
        /// 查询指定债券的市场成交用
        /// </summary>
        private void DoProcQueryERQueue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 等待数据可用
                    QueryErResetEvent.Wait();

                    ExecutionReport data;

                    while (QueryERQueue.TryDequeue(out data))
                    {
                        try
                        {
                            UpdateQueryMarketExecutions(data);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex.Message);
                        }
                    }

                    // 重置事件
                    QueryErResetEvent.Reset();
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
        /// 更新查询指定的债券市场成交
        /// </summary>
        /// <param name="obj">原始报文</param>
        private void UpdateQueryMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.SearchText))
                {
                    if (obj.SecurityID != this.SearchText)
                    {
                        return;
                    }
                }

                if (!DictMarketVenue.ContainsKey(obj.VenueTypeGW))
                    return;

                // 如果状态是4， 删除这笔单子。
                if (obj.OrdStatus == "4")
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        // 删除
                        string execID = obj.ExecID;

                        MarketExecutionsItem result2 = _tmpMarketExes.Where(orderItem => orderItem.Key == execID)?.FirstOrDefault();
                        if (result2 != null)
                        {
                            _tmpMarketExes.Remove(result2);
                        }
                    }));

                    return;
                }

                //显示数据

                MarketExecutionsItem result = null;
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    result = _tmpMarketExes.Where(orderItem => orderItem.Key == obj.ExecID)?.FirstOrDefault();
                    MarketExecutionsItem marketExe = new MarketExecutionsItem();
                    ConvertExecutionReportToMarketExecutionsItem(obj, ref marketExe);
                    marketExe.Key = marketExe.ExecID;

                    if (result == null)
                    {
                        // 添加新数据
                        _tmpMarketExes.Add(marketExe);
                    }
                    else
                    {
                        string transactionTime = CommonUtils.ConvertToString(result.TimeStamp);
                        DateTime orgdatetime = DateTimeEx.ConvertStringToDateTime(transactionTime);
                        DateTime curUpdateTime = DateTimeEx.ConvertStringToDateTime(obj.UpdateTime);
                        if (DateTime.Compare(orgdatetime, curUpdateTime) > 0)
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }

                        ConvertExecutionReportToMarketExecutionsItem(obj, ref result);
                    }
                }));
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }
    }
    #endregion
}
