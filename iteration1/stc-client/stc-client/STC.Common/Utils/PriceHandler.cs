using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Handler.Base;
using STC.Common.Handler.Price;
using STC.Common.Model.Common;
using STC.Common.Model.Fix.Message;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static STC.Common.Model.Fix.Message.MarketDataSnapshotFullRefresh;

namespace STC.Common.Utils
{
    /// <summary>
    /// 行情数据处理类
    /// </summary>
    public class PriceHandler
    {
        /// <summary>
        /// PriceHandler实例
        /// </summary>
        private static PriceHandler server = new PriceHandler();
        public static PriceHandler Instance { get { return server; } }


        ///// <summary>
        ///// 深度行情是否要更新情况
        ///// </summary>
        //private ConcurrentDictionary<string, bool> _UpdateCTPricesDic = new ConcurrentDictionary<string, bool>();

        ///// <summary>
        ///// 最优行情是否要更新情况
        ///// </summary>
        //private ConcurrentDictionary<string, bool> _UpdateH0PricesDic = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 深度行情回调函数
        /// </summary>
        public ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>> _UpdateCTCallbackDic = new ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>>();

        /// <summary>
        /// 最优行情回调函数
        /// </summary>
        private ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>> _UpdateH0CallbackDic = new ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>>();

        //存储CT初级处理数据（把MarketDataSnapshotFullRefresh转换成DataView数据结构）
        private ConcurrentDictionary<string, DataView> _baseCTPriceDataViewDic = new ConcurrentDictionary<string, DataView>();


        /// <summary>
        /// 更新价格的线程
        /// </summary>
        //private Thread _updatePriceThread;

        private AsyncThreadQueue<string[]> _subscribeQueue;

        /// <summary>
        /// 构造函数
        /// </summary>
        private PriceHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
            //// 定时检查 ctprice是否需要更新。
            //if (_updatePriceThread == null)
            //{
            //    _updatePriceThread = new Thread(UpdatePrice);
            //    _updatePriceThread.IsBackground = true;
            //    _updatePriceThread.Start();
            //}

            // 订阅线程
            if(_subscribeQueue == null)
            {
                _subscribeQueue = new AsyncThreadQueue<string[]>(ProcessSub);
            }
            
            // 启动深度行情
            //if (_updateCTPMarketDataThread == null)
            //{
            //    _updateCTPMarketDataThread = new Thread(UpdateCTPMarketDataProcessor);
            //    _updateCTPMarketDataThread.Priority = ThreadPriority.AboveNormal;
            //    _updateCTPMarketDataThread.Start();
            //}
            // 启动最优行情
            if (_updateHOMarketDataThread == null)
            {
                _updateHOMarketDataThread = new Thread(UpdateHOMarketDataProcessor);
                _updateHOMarketDataThread.Priority = ThreadPriority.AboveNormal;
                _updateHOMarketDataThread.Start();
            }

            // 启动多Task处理    
            Init();
        }

        private void ProcessSub(string[] param)
        {
            string securtityid = string.Empty;
            string type = string.Empty;
            try
            {
                if (param.Length == 2)
                {
                    securtityid = param[0];
                    type = param[1];

                    if (type == "SubscribeCT")
                    {
                        MDHandler.Instance.SubscribeCT(securtityid, Instance_OnCTPrice);
                    }
                    else if (type == "UnsubscribeCT")
                    {
                        MDHandler.Instance.UnsubscribeCT(securtityid, Instance_OnCTPrice);
                    }
                }
            }
            catch(Exception e)
            {
                LogHelper.Error($"{type}：{securtityid}", e);
            }
            
        }


        #region 订阅退订

        /// <summary>
        /// 注册H0价格
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="H0PriceCallBack"></param>
        public void RegisterH0Price(string securityID, Action<MarketDataSnapshotFullRefresh> H0PriceCallBack)
        {
            string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0,"", securityID, string.Empty, string.Empty);
            lock (primaryKey)
            {
                //MDHandler.Instance.Init(securityID, null, Instance_OnMDPrice);
                //MDHandler.Instance.SubscribeBest(securityID, Instance_OnMDPrice);
                if (H0PriceCallBack != null)
                {
                    if (_UpdateH0CallbackDic.ContainsKey(primaryKey) == false || _UpdateH0CallbackDic[primaryKey] == null)
                    {
                        Action<MarketDataSnapshotFullRefresh> newAction = null;
                        _UpdateH0CallbackDic.AddOrUpdate(primaryKey, newAction, (key, value) => newAction);
                        _UpdateH0CallbackDic[primaryKey] += H0PriceCallBack;
                    }
                    else
                    {
                        int index = Array.IndexOf(_UpdateH0CallbackDic[primaryKey].GetInvocationList(), (Action<MarketDataSnapshotFullRefresh>)H0PriceCallBack);
                        if (index < 0)
                        {
                            _UpdateH0CallbackDic[primaryKey] += H0PriceCallBack;
                        }
                    }
                }

                //_UpdateH0PricesDic.AddOrUpdate(primaryKey, true, (key, value) => true);
                MarketDataSnapshotFullRefresh h0Price = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                if (h0Price != null)
                {
                    H0PriceCallBack?.Invoke(h0Price);
                }

                _subscribeQueue.Enqueue(new string[] { securityID, "SubscribeBest" });

            }
        }

        /// <summary>
        /// 注销H0价格
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="H0PriceCallBack"></param>
        public void UnregisterH0Price(string securityID, Action<MarketDataSnapshotFullRefresh> H0PriceCallBack)
        {
            string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0,"", securityID, string.Empty, string.Empty);
            lock (primaryKey)
            {
                if (_UpdateH0CallbackDic.ContainsKey(primaryKey))
                {
                    _UpdateH0CallbackDic[primaryKey] -= H0PriceCallBack;
                }

                //如果没人订阅就UnSubscribe
                if (_UpdateH0CallbackDic[primaryKey] == null)
                {
                    _subscribeQueue.Enqueue(new string[] { securityID, "UnsubscribeBest" });

                    //ThreadPool.QueueUserWorkItem(o =>
                    //{
                    //    try
                    //    {
                    //        MDHandler.Instance.UnsubscribeBest(securityID, Instance_OnMDPrice);
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        LogHelper.Error("UnregisterH0Price", e);
                    //    }
                    //});
                }
            }
        }

        /// <summary>
        /// 
        /// 注册CT价格
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="CTPriceCallBack"></param>
        public void RegisterCTPrice(string securityID, Action<MarketDataSnapshotFullRefresh> CTPriceCallBack)
        {
            if (string.IsNullOrEmpty(securityID))
                return;

            List<string> ric;
            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "bdb":
                case "idcclient":
                    ric = TopicUtil.GetMdRic(securityID, LocalEnv.UserInfo.BonDPermissions);
                    break;
                default:
                    string topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, securityID, string.Empty, string.Empty);
                    ric = new List<string>();
                    ric.Add(topic);
                    break;
            }
            foreach (string str in ric)
            {
                string primaryKey = str;
                lock (string.Intern(primaryKey))
                {
                    //MDHandler.Instance.Init(securityID, Instance_OnCTPrice, null);
                    //MDHandler.Instance.SubscribeCT(securityID, Instance_OnCTPrice);
                    if (CTPriceCallBack != null)
                    {
                        if (_UpdateCTCallbackDic.ContainsKey(primaryKey) == false || _UpdateCTCallbackDic[primaryKey] == null)
                        {
                            Action<MarketDataSnapshotFullRefresh> newAction = null;
                            _UpdateCTCallbackDic.AddOrUpdate(primaryKey, newAction, (key, value) => newAction);
                            _UpdateCTCallbackDic[primaryKey] += CTPriceCallBack;
                            LogHelper.Debug("PriceHandler _UpdateCTCallbackDic1:" + primaryKey);
                        }
                        else
                        {
                            int index = Array.IndexOf(_UpdateCTCallbackDic[primaryKey].GetInvocationList(), (Action<MarketDataSnapshotFullRefresh>)CTPriceCallBack);
                            if (index < 0)
                            {
                                _UpdateCTCallbackDic[primaryKey] += CTPriceCallBack;
                                LogHelper.Debug("PriceHandler _UpdateCTCallbackDic2:" + primaryKey);
                            }
                        }
                    }

                    //_UpdateCTPricesDic.AddOrUpdate(primaryKey, true, (key, value) => true);
                    MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);

                    if (_UpdateCTCallbackDic.ContainsKey(primaryKey) && ctPrice != null)
                    {
                        //BDB
                        switch (LocalEnv.ServerInfo.AppName.ToLower())
                        {
                            case "bdb":
                            case "idcclient":
                                if (!ctPrice.ContainsKey("isCache"))
                                {
                                    ctPrice.Add("isCache", true);
                                }
                                RefreshBaseCTPriceDataView(primaryKey, ctPrice);
                                break;
                        }

                        CTPriceCallBack?.Invoke(ctPrice);
                    }

                    _subscribeQueue.Enqueue(new string[] { securityID, "SubscribeCT" });
                }
            }
        }

        /// <summary>
        /// 注销CTPrice
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="CTPriceCallBack"></param>
        public void UnregisterCTPrice(string securityID, Action<MarketDataSnapshotFullRefresh> CTPriceCallBack)
        {
            if (!string.IsNullOrEmpty(securityID))
            {
                List<string> ric;
                switch (LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        ric = TopicUtil.GetMdRic(securityID, LocalEnv.UserInfo.BonDPermissions);
                        break;
                    default:
                        string topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, securityID, string.Empty, string.Empty);
                        ric = new List<string>();
                        ric.Add(topic);
                        break;
                }


                foreach (string str in ric)
                {
                    string primaryKey = str;//TopicUtil.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, securityID, string.Empty, string.Empty, string.Empty);
                    if (!string.IsNullOrEmpty(primaryKey))
                    {
                        lock (primaryKey)
                        {
                            if (_UpdateCTCallbackDic.ContainsKey(primaryKey))
                            {
                                _UpdateCTCallbackDic[primaryKey] -= CTPriceCallBack;

                                LogHelper.Debug("PriceHandler _UpdateCTCallbackDic:" + primaryKey);
                                
                                Action<MarketDataSnapshotFullRefresh> tem = _UpdateCTCallbackDic[primaryKey];

                                // 如果没人订阅就UnSubscribe
                                if (tem == null || tem.GetInvocationList().Length <= 0)//[primaryKey] == null)
                                {
                                    _UpdateCTCallbackDic.TryRemove(primaryKey, out tem);
                                    LogHelper.Info("MDHandler.Instance.UnsubscribeCT:" + securityID);
                                    //MDHandler.Instance.UnsubscribeCT(securityID, Instance_OnCTPrice);

                                    //ThreadPool.QueueUserWorkItem(o =>
                                    //{
                                    //    try
                                    //    {
                                    //        MDHandler.Instance.UnsubscribeCT(securityID, Instance_OnCTPrice);
                                    //    }
                                    //    catch (Exception e)
                                    //    {
                                    //        LogHelper.Error("UnregisterCTPrice", e);
                                    //    }
                                    //});

                                    _subscribeQueue.Enqueue(new string[] { securityID, "UnsubscribeCT" });
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion


        #region 最优行情数据处理
        private bool isdispose = false;
                    
                     
        //最优行情是否要更新情况
        private Thread _updateHOMarketDataThread;

        //最优行情队列
        private ConcurrentQueue<Common.Model.Fix.Message.MarketDataSnapshotFullRefresh> dataHOQueue = new ConcurrentQueue<Common.Model.Fix.Message.MarketDataSnapshotFullRefresh>();

        private ManualResetEventSlim dataHOAvailable = new ManualResetEventSlim(false);

        private void UpdateHOMarketDataProcessor()
        {
            while (!isdispose)
            {
                // 等待数据可用
                dataHOAvailable.Wait();

                Common.Model.Fix.Message.MarketDataSnapshotFullRefresh data;

                while (dataHOQueue.TryDequeue(out data))
                {
                    if (data == null) // 生产结束的标记
                        return;

                    //最优行情队列
                    ProcessHOMarketData(data);
                }

                // 重置事件
                dataHOAvailable.Reset();
            }
        }

        /// <summary>
        /// 做市走势报价（H0）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnBestPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            //string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, obj.SecurityID, string.Empty, string.Empty);
            //lock (string.Intern(primaryKey))
            //{
            //    _UpdateH0PricesDic.AddOrUpdate(primaryKey, true, (key, value) => true);
            //}
            // 将数据放入队列
            dataHOQueue.Enqueue(obj);
            // 设置事件，通知消费者有新数据
            dataHOAvailable.Set();
        }


        /// <summary>
        /// 最优行情队列
        /// </summary>
        /// <param name="data"></param>
        private void ProcessHOMarketData(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
                {

                    string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, "", obj.SecurityID, string.Empty, string.Empty);

                    if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                    {
                        LogHandler.Info($"TBook行情 需更新 {primaryKey}");
                    }

                    lock (string.Intern(primaryKey))
                    {
                        MarketDataSnapshotFullRefresh h0Price = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                        // 通知界面
                        if (_UpdateH0CallbackDic.ContainsKey(primaryKey) && h0Price != null)
                        {
                            _UpdateH0CallbackDic[primaryKey]?.Invoke(h0Price);
                        }
                    }

                }));
            });
        }

        #endregion

        #region  其它处理 获取 最优价格
        /// <summary>
        /// 测试MD行情数据
        /// </summary>
        /// <param name="obj"></param>
        public void TestOnMDPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            Instance_OnBestPrice(obj);
        }


        /// <summary>
        /// 根据债券代码， 返回最优行情报价
        /// </summary>
        /// <param name="securityID">债券代码</param>
        /// <returns>缓存的最优行情报价</returns>
        public MarketDataSnapshotFullRefresh GetCacheH0Price(string securityID)
        {
            string h0Topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0,"", securityID, string.Empty, string.Empty);
            return MDHandler.Instance.GetCachePriceByTopic(h0Topic);
        }

        /// <summary>
        /// 根据方向和清算速度筛选数据
        /// </summary>
        /// <param name="gNoMDEntriesList">原始price列表</param>
        /// <param name="side">方向</param>
        /// <param name="settlType">清算速度</param>
        /// <returns></returns>
        public List<GNoMDEntries> GetPriceList(List<GNoMDEntries> gNoMDEntriesList, string side, string settlType)
        {
            string[] arr = settlType.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            List<GNoMDEntries> list = new List<GNoMDEntries>();
            if (gNoMDEntriesList != null && (arr.Length == 0 || arr.Contains("T0")))
            {
                var sublist = gNoMDEntriesList.Where(item => item.MDEntryType == side && item.SettlType == "1");
                if (sublist != null)
                {
                    list.AddRange(sublist);
                }
            }
            if (gNoMDEntriesList != null && (arr.Length == 0 || arr.Contains("T1")))
            {
                var sublist = gNoMDEntriesList.Where(item => item.MDEntryType == side && (item.SettlType == "2" || item.SettlType == "3"));
                if (sublist != null)
                {
                    list.AddRange(sublist);
                }
            }
            if (gNoMDEntriesList != null && (arr.Length == 0 || arr.Contains("Yuan")))
            {
                var sublist = gNoMDEntriesList.Where(item => item.MDEntryType == side && !(item.SettlType == "1" || item.SettlType == "2" || item.SettlType == "3"));
                if (sublist != null)
                {
                    list.AddRange(sublist);
                }
            }

            return list;
        }

        /// <summary>
        /// 找到Bid最优价
        /// </summary>
        /// <param name="bidPriceList">Bid价格列表</param>
        /// <returns></returns>
        public decimal GetBestBidPrice(List<GNoMDEntries> bidPriceList)
        {
            decimal bestPrice = decimal.MaxValue;
            foreach (GNoMDEntries price in bidPriceList)
            {
                if (CommonUtils.ConvertToDecimal(price.MDEntryPx) < bestPrice)
                {
                    bestPrice = CommonUtils.ConvertToDecimal(price.MDEntryPx);
                }
            }
            return bestPrice;
        }

        /// <summary>
        /// 找到Ofr最优价
        /// </summary>
        /// <param name="ofrPriceList">Ofr价格列表</param>
        /// <returns></returns>
        public decimal GetBestOfrPrice(List<GNoMDEntries> ofrPriceList)
        {
            decimal bestPrice = decimal.MinValue;
            foreach (GNoMDEntries price in ofrPriceList)
            {
                if (CommonUtils.ConvertToDecimal(price.MDEntryPx) > bestPrice)
                {
                    bestPrice = CommonUtils.ConvertToDecimal(price.MDEntryPx);
                }
            }
            return bestPrice;
        }

        /// <summary>
        /// 根据最优价统计，价，量，渠道， 清算速度
        /// </summary>
        /// <param name="PriceList">价格列表</param>
        /// <param name="bestPrice">最优价格</param>
        /// <returns></returns>
        public BestPrice GenBestPrice(IEnumerable<GNoMDEntries> priceList, decimal bestPrice)
        {
            if (priceList == null || priceList.Count() == 0 || bestPrice == decimal.MinValue || bestPrice == decimal.MaxValue)
            {
                return null;
            }

            BestPrice result = new BestPrice();
            var list = priceList.Where(item => CommonUtils.ConvertToDecimal(item.MDEntryPx) == bestPrice);
            if (list != null && list.Count() > 0)
            {
                long bestQty = 0;
                string bestVenues = "-";
                int bestCount = 0;
                string firstBestVenues = string.Empty;
                int xbondBestCount = 0;
                StringBuilder bestSettlType = new StringBuilder();
                List<string> venueList = new List<string>();


                foreach (GNoMDEntries entry in list)
                {
                    bestCount++;
                    if (bestCount == 1)
                    {
                        firstBestVenues = entry.Venues;
                    }

                    bestQty += CommonUtils.ConvertToLong(entry.MDEntrySize);

                    if (entry.Venues.ToUpper() == firstBestVenues)
                    {
                        xbondBestCount++;
                    }

                    if (venueList.Contains(entry.Venues) == false)
                    {
                        venueList.Add(entry.Venues);
                    }

                    if (string.IsNullOrEmpty(entry.SettlType) == false && bestSettlType.ToString().Contains(entry.SettlType) == false)
                    {
                        bestSettlType.Append($"{entry.SettlType},");
                    }
                }
                if (bestCount == 1)
                {
                    bestVenues = firstBestVenues;
                }
                else if (bestCount > 1)
                {
                    if (xbondBestCount == bestCount)
                    {
                        bestVenues = firstBestVenues;
                    }
                    else
                    {
                        bestVenues = "多家";
                    }
                }
                if (bestVenues != "-")
                {
                    result.BestDisplayVenues = bestVenues;
                    result.BestVenues = venueList;
                    result.MDEntrySize = bestQty;
                    result.MDEntryPx = bestPrice;
                    result.SettlType = bestSettlType.ToString();
                }
            }
            return result;
        }

        public BilateralBestPrice CalBestPrice(string securityID, string customFilter, string broker)
        {
            BilateralBestPrice result = null;
            lock (_baseCTPriceDataViewDic)
            {
                List<string> ric = TopicUtil.GetMdRic(securityID, LocalEnv.UserInfo.BonDPermissions);

                List<BilateralBestPrice> bestPriceList = new List<BilateralBestPrice>();


                foreach (string key in ric)
                {
                    if (!string.IsNullOrEmpty(broker) && !key.Contains(broker))
                        continue;

                    string truekey = key.Contains("=CT") ? key.Replace("=CT", "=ct").Replace("stc.md.CT", "stc.md.ct") : key;
                    if (_baseCTPriceDataViewDic.ContainsKey(truekey))
                    {
                        this.UpdateBestPrice(_baseCTPriceDataViewDic[truekey], securityID, bestPriceList, customFilter);
                    }
                }

                result = PriceCalHelper.Instance.CalBestPrice(bestPriceList);
            }
            return result;
        }


        public string GetValuation(string securityID, string settle)
        {
            string result = string.Empty;
            List<MarketDataSnapshotFullRefresh> marketDatas = GetAllMarketData(string.Empty, securityID);

            if (marketDatas.Count > 0)
            {
                IOrderedEnumerable<MarketDataSnapshotFullRefresh> list = marketDatas.OrderByDescending(o => o.SendingTime);
                result = list.First().CCDCValuation;
            }

            return result;
        }

        public List<MarketDataSnapshotFullRefresh> GetAllMarketData(string securityID)
        {
            List<MarketDataSnapshotFullRefresh> result = new List<MarketDataSnapshotFullRefresh>();

            List<string> ric = TopicUtil.GetMdRic(securityID, LocalEnv.UserInfo.BonDPermissions);

            foreach (string topic in ric)
            {
                MarketDataSnapshotFullRefresh marketData = MDHandler.Instance.GetCachePriceByTopic(topic);
                if (marketData != null)
                {
                    result.Add(marketData);
                }
            }

            return result;
        }

        /// <summary>
        /// 根据筛选条件计算最优价格
        /// </summary>
        /// <param name="priceBaseData"></param>
        private void UpdateBestPrice(DataView priceBaseData, string securityID, List<BilateralBestPrice> bestPriceList, string customFilter)
        {
            if (priceBaseData == null)
                return;

            BilateralBestPrice bilateralBestPrice = new BilateralBestPrice();
            // bid最优价格
            var bestBid = CalcBestPrice(priceBaseData, "0", customFilter);
            // ofr 最优价格
            var bestOfr = CalcBestPrice(priceBaseData, "1", customFilter);

            bilateralBestPrice.BidBest = bestBid;
            bilateralBestPrice.OfrBest = bestOfr;
            //bilateralBestPrice.Valuation = obj.CCDCValuation;
            bestPriceList.Add(bilateralBestPrice);

        }

        private string CalBar(string bargainFlag, string newBarginFlag)
        {
            string result = bargainFlag;

            if (string.IsNullOrEmpty(bargainFlag))
                result = newBarginFlag;
            else if (bargainFlag != newBarginFlag && newBarginFlag == "2")
                result = "2";


            return result;
        }


        private BestPrice1 CalcBestPrice(DataView priceBaseData, string side, string customerFilter)
        {
            string bargain = string.Empty;
            StringBuilder sb = new StringBuilder();
            string bestVenue = string.Empty;
            string venues = string.Empty;
            string bestPx = string.Empty;
            decimal bestQty = 0;
            string bestT0 = string.Empty;
            string bestT1 = string.Empty;
            string bestYuan = string.Empty;
            string hasFirm = "0";
            string hasNonFirm = "0";
            string priceType = string.Empty;
            decimal T1Qty = 0;
            decimal hugeT1Qty = 0;

            string bidSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            string ofrSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            string filter = string.Empty;
            if (side == "0")
            {
                priceBaseData.Sort = bidSort;
                filter = $"{CTPriceControler.SIDE_FIELD} = '0'";
                // bid最优价格
                if (!string.IsNullOrEmpty(customerFilter))
                {
                    filter = $"{filter} and ({customerFilter})";
                }
                priceBaseData.RowFilter = filter;
            }
            else if (side == "1")
            {
                priceBaseData.Sort = ofrSort;
                filter = $"{CTPriceControler.SIDE_FIELD} = '1'";
                // bid最优价格
                if (!string.IsNullOrEmpty(customerFilter))
                {
                    filter = $"{filter} and ({customerFilter})";
                }
                priceBaseData.RowFilter = filter;
            }



            if (priceBaseData.Count > 0)
            {
                bestPx = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.ENTRY_PX_FIELD]);
                bestVenue = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.SOURCE_FIELD]);
                priceType = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.PriceType_Field]);
                decimal bestPx_decimal = CommonUtils.ConvertToDecimal(bestPx);
                if (CommonUtils.IsNumeric(bestPx))
                {
                    foreach (DataRowView drv in priceBaseData)
                    {
                        string currentVenues = CommonUtils.ConvertToString(drv[CTPriceControler.SOURCE_FIELD]);
                        string px = CommonUtils.ConvertToString(drv[CTPriceControler.ENTRY_PX_FIELD]);
                        if (CommonUtils.IsNumeric(px))
                        {
                            decimal px_decimal = CommonUtils.ConvertToDecimal(px);
                            decimal qty = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(drv[CTPriceControler.QTY_FIELD]));
                            string settlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_FIELD]));
                            string yuanSettlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD]));
                            string firmFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.FIRM_TYPE_FIELD]));
                            string text = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.TEXT_FIELD]));
                            string barginFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.BARGAIN_FLAG_FIELD]));
                           
                            if (bestPx_decimal == px_decimal)
                            {

                                if (sb.Length > 0)
                                {
                                    if (venues != currentVenues)
                                    {
                                        sb.Append("]");
                                    }
                                    sb.Append("+");
                                }

                                if (venues != currentVenues)
                                {
                                    sb.Append("[");
                                    T1Qty = 0;
                                }

                                T1Qty += qty;

                                if (hugeT1Qty < T1Qty)
                                {
                                    hugeT1Qty = T1Qty;
                                }

                                if (!string.IsNullOrEmpty(text))
                                    sb.Append(string.Format("{0}", text));//string.Format("{0}({1})", mdEntry.MDEntrySize, mdEntry.Text));
                                else
                                    sb.Append(qty);


                                bargain = CalBar(bargain, barginFlag);

                                bestQty += qty;
                                string[] settleArr = settlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string str in settleArr)
                                {
                                    if (str == "2")
                                    {
                                        bestT1 = "1";
                                    }

                                    if (str == "1")
                                    {
                                        bestT0 = "1";
                                    }
                                }
                                if (yuanSettlType == "4" || yuanSettlType.Length > 1)
                                {
                                    bestYuan = "1";
                                }
                                if (firmFlag == "1")
                                {
                                    hasFirm = "1";
                                }
                                if (firmFlag == "0")
                                {
                                    hasNonFirm = "1";
                                }
                                venues = currentVenues;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    foreach (DataRowView drv in priceBaseData)
                    {
                        string currentVenues = CommonUtils.ConvertToString(drv[CTPriceControler.SOURCE_FIELD]);
                        string px = CommonUtils.ConvertToString(drv[CTPriceControler.ENTRY_PX_FIELD]);
                        if (CommonUtils.IsNumeric(px) == false)
                        {
                            decimal qty = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(drv[CTPriceControler.QTY_FIELD]));
                            string settlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_FIELD]));
                            string yuanSettlType = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD]));
                            string firmFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.FIRM_TYPE_FIELD]));
                            string text = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.TEXT_FIELD]));
                            string barginFlag = CommonUtils.ConvertToString(CommonUtils.ConvertToString(drv[CTPriceControler.BARGAIN_FLAG_FIELD]));
                            
                            if (sb.Length > 0)
                            {
                                if (venues != currentVenues)
                                {
                                    sb.Append("]");
                                }
                                sb.Append("+");
                            }

                            if (!string.IsNullOrEmpty(text))
                                sb.Append(string.Format("{0}", text));//string.Format("{0}({1})", mdEntry.MDEntrySize, mdEntry.Text));
                            else
                                sb.Append(qty);

                            if (venues != currentVenues)
                            {
                                sb.Append("[");
                            }

                            bargain = CalBar(bargain, barginFlag);

                            bestQty += qty;

                            string[] settleArr = settlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string str in settleArr)
                            {
                                if (str == "2")
                                {
                                    bestT1 = "1";
                                }

                                if (str == "1")
                                {
                                    bestT0 = "1";
                                }
                            }


                            //if (settlType.Contains("1"))
                            //{
                            //    bestT0 = "1";
                            //}
                            if (yuanSettlType == "4" || yuanSettlType.Length > 1)
                            {
                                bestYuan = "1";
                            }
                            if (firmFlag == "1")
                            {
                                hasFirm = "1";
                            }
                            if (firmFlag == "0")
                            {
                                hasNonFirm = "1";
                            }

                            venues = currentVenues;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (priceType == PriceType.Intention && !CommonUtils.IsNumeric(bestPx))
                    {
                        bestPx = side == "0" ? "Bid" : "Ofr";
                    }
                }
            }

            if (sb.Length > 0)
            {
                sb.Append("]");
            }

            return new BestPrice1() { bestPx = bestPx, bestQty = bestQty, bestT0 = bestT0, bestT1 = bestT1, bestYuan = bestYuan, 
                isAllFirm = (hasFirm == "1" && hasNonFirm == "0") ? "1" : "0", venue = bestVenue, priceType = priceType,
                text = sb.ToString(), bargain = bargain, T1Qty = hugeT1Qty };
        }

        public List<MarketDataSnapshotFullRefresh> GetAllMarketData(string broker, string securityID)
        {
            List<MarketDataSnapshotFullRefresh> result = new List<MarketDataSnapshotFullRefresh>();
            List<string> ric;
            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "bdb":
                case "idcclient":
                    ric = TopicUtil.GetMdRic(securityID, LocalEnv.UserInfo.BonDPermissions);
                    break;
                default:
                    string topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, securityID, string.Empty, string.Empty);
                    ric = new List<string>();
                    ric.Add(topic);
                    break;
            }

            foreach (string topic in ric)
            {
                MarketDataSnapshotFullRefresh marketData = MDHandler.Instance.GetCachePriceByTopic(topic);
                if (marketData != null)
                {
                    if (!string.IsNullOrEmpty(broker) && marketData.Venues.Equals(broker))
                    {
                        result.Add(marketData);
                    }
                    else
                        result.Add(marketData);
                }
            }
            return result;
        }

        #endregion

        #region  安全处理MD线程
        /// <summary>
        /// 根据债券代码， 返回深度行情报价
        /// </summary>
        /// <param name="securityID">债券代码</param>
        /// <returns>缓存的深度行情报价</returns>
        public MarketDataSnapshotFullRefresh GetCacheCTPrice(string securityID)
        {
            string ctTopic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, securityID, string.Empty, string.Empty);
            return MDHandler.Instance.GetCachePriceByTopic(ctTopic);
        }


        private void RefreshBaseCTPriceDataView(string key, MarketDataSnapshotFullRefresh obj)
        {
            lock (_baseCTPriceDataViewDic)
            {
                if (_baseCTPriceDataViewDic.ContainsKey(key) == false)
                {
                    _baseCTPriceDataViewDic[key] = CTPriceControler.CreateBaseDataView();
                }

                if (CommonUtils.IsBDBAppName())
                {
                    CTPriceControler.UpdateDataToBaseDV_BDB(_baseCTPriceDataViewDic[key], obj);
                }
                else
                {
                    CTPriceControler.UpdateDataToBaseDV(_baseCTPriceDataViewDic[key], obj);
                }
            }
        }

        #endregion


        #region 调度管理器
        //Ver 1.1 Author:chenhu Date:20250414 Func:最优化处理深度行情
        //Ver 1.0 Author:chenhu Date:20240723 Func:解决堵塞等问题
        private  ConcurrentDictionary<string, MarketDataSnapshotFullRefresh> DictCTPrice = new ConcurrentDictionary<string, MarketDataSnapshotFullRefresh>(); // 存储债券最新行情
        private  int _initialTaskCount = 18;   // 初始 Worker 任务数量
        private  int _taskIncrement = 5;       // 任务不足时增加的 Worker 数量
        private  ConcurrentDictionary<string, object> _bondLocks = new ConcurrentDictionary<string, object>(); // 债券锁
        private  ConcurrentDictionary<string, MarketDataSnapshotFullRefresh> _bondMarketData = new ConcurrentDictionary<string, MarketDataSnapshotFullRefresh>(); // 债券最新行情数据
        private  ConcurrentQueue<string> _taskQueue = new ConcurrentQueue<string>(); // 任务队列
        private  ConcurrentDictionary<string, bool> _pendingBondQueue = new ConcurrentDictionary<string, bool>(); // 正在排队的债券
        private  List<Task> _workers = new List<Task>(); // Worker 线程池
        private  object _lock = new object(); // 线程安全锁
        private  SemaphoreSlim _taskAvailable = new SemaphoreSlim(0, int.MaxValue); // 信号量控制任务处理

        private bool ispending;
        private object islock;
        /// <summary>
        /// 启动新的 Worker 线程
        /// </summary>
        private void StartNewWorker()
        {
            Task worker = Task.Factory.StartNew(WorkerProcess, TaskCreationOptions.LongRunning);
            _workers.Add(worker);
            _initialTaskCount++;
            LogHelper.Info($"[TaskManager] 新增 Task 线程，总数: {_initialTaskCount}");
        }

        public void Init(int initialWorkers = 18)
        {
            try
            {
                _initialTaskCount = initialWorkers;
                for (int i = 0; i < initialWorkers; i++)
                {
                    Task worker = Task.Factory.StartNew(WorkerProcess, TaskCreationOptions.LongRunning);
                    _workers.Add(worker);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }


        /// <summary>
        /// 深度行情（CT）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnCTPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            try
            {
                if (obj.SecurityID == "TradeBook")
                {
                    return;
                }

                lock (_lock)
                {
                    // 更新长期行情存储，不会被清空
                    DictCTPrice.AddOrUpdate(obj.SecurityID, obj, (key, value) => obj);
                    // 只存储最新的行情数据，覆盖旧数据
                    _bondMarketData.AddOrUpdate(obj.SecurityID, obj, (key, value) => obj);

                    // 如果已经排队或正在处理，就不重复入队
                    if (_pendingBondQueue.ContainsKey(obj.SecurityID) || _bondLocks.ContainsKey(obj.SecurityID))
                    {
                        //LogHelper.Info($"[忽略] {obj.SecurityID} 已在处理或队列中，只更新数据: {obj.SecurityID}");
                        return;
                    }

                    // 加入唯一队列标记
                    _pendingBondQueue.AddOrUpdate(obj.SecurityID, true, (key, value) => true);

                    // 新增任务
                    _taskQueue.Enqueue(obj.SecurityID);
                    _taskAvailable.Release(); // 释放信号，通知 Worker 立即执行
                    //LogHelper.Info($"[入队] {obj.SecurityID} - {obj.SendingTime}");
                    // 处理完后，移除排队标记

                    // 如果任务数量超过了当前的 Task 数量，则创建新任务
                    //List<string> bondIds = new List<string>(DictCTPrice.Keys);
                    if (_taskQueue.Count > _initialTaskCount)
                    {
                        int additionalWorkers = Math.Min(_taskQueue.Count - _initialTaskCount, 5); // 每次最多新增 5 个任务线程
                        for (int i = 0; i < additionalWorkers; i++)
                        {
                            StartNewWorker();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }


        /// <summary>
        ///  任务线程循环处理数据
        /// </summary>
        /// <summary>
        /// 更高效、轻量的债券行情处理框架
        /// 每个债券一个 Task 处理，减少阻塞，提高吞吐量
        /// 适用于高频债券行情处理，吞吐量大幅提升
        /// 不会遗漏任何一笔债券行情，即使在处理过程中有新的数据到达
        /// 高并发，支持大量债券同时处理
        /// </summary>
        private void WorkerProcess()
        {
            while (!isdispose)
            {
                _taskAvailable.Wait(); // 等待任务信号（无阻塞）
                string bondId = string.Empty;
                MarketDataSnapshotFullRefresh latestData;
                // 处理任务
                try
                {
                    while (_taskQueue.TryDequeue(out bondId))
                    {
                        // **抢单机制：如果债券已被抢占，则跳过**
                        if (!_bondLocks.TryAdd(bondId, new object()))
                        {
                            //LogHelper.Info($"[抢单失败] 债券 {bondId} 正在处理中，任务跳过");
                            _pendingBondQueue.TryRemove(bondId, out ispending);
                            continue;
                        }

                        // 从 pending 队列中移除排队标记
                        _pendingBondQueue.TryRemove(bondId, out ispending);

                        // 读取最新行情数据
                        while (_bondMarketData.TryRemove(bondId, out latestData))
                        {
                            //LogHelper.Info($"[处理] Worker {Task.CurrentId} 处理债券 {bondId} 最新数据: {latestData.SendingTime}");
                            HandleLatestMarketData(latestData);
                            //LogHelper.Info($"[完成] Worker {Task.CurrentId} 完成债券 {bondId} 数据: {latestData.SendingTime}");
                        }

                        // 任务完成，释放锁
                        _bondLocks.TryRemove(bondId, out islock);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"{Task.CurrentId} ProcessBondData 处理 {bondId} 行情异常：", ex);
                }
            }
        }

        // 处理最新行情数据
        private void HandleLatestMarketData(MarketDataSnapshotFullRefresh ctPrice)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string primaryKey = string.Empty;
                //BDB
                switch (LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        string key = TopicUtil.GetMdRic(ctPrice.SecurityID, LocalEnv.UserInfo.BonDPermissions)[0];
                        primaryKey = key.Contains("=CT") ? key.Replace("=CT", "=ct").Replace("stc.md.CT", "stc.md.ct") : key;
                        break;
                    default:
                        primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, ctPrice.SecurityID, string.Empty, string.Empty);
                        break;
                }

                if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                {
                    LogHandler.Info($"TBook行情 Callback  {primaryKey}");

                    foreach (GNoMDEntries noMDEntry in ctPrice.NoMDEntries)
                        LogHelper.Info(String.Format("Security:{0}, time:{1}, price:{2}", ctPrice.SecurityID, noMDEntry.MDEntryTime, noMDEntry.MDEntryPx));
                }

                if (ctPrice != null)
                {
                    //BDB
                    switch (LocalEnv.ServerInfo.AppName.ToLower())
                    {
                        case "bdb":
                        case "idcclient":
                            RefreshBaseCTPriceDataView(primaryKey, ctPrice);
                            break;

                    }

                    if (_UpdateCTCallbackDic.ContainsKey(primaryKey))
                    {
                        _UpdateCTCallbackDic[primaryKey]?.Invoke(ctPrice);
                    }
                }
            }, DispatcherPriority.Send);

        }


      
        #endregion

        #region 析构函数 事件处理
        /// <summary>
        /// 析构函数
        /// </summary>
        ~PriceHandler()
        {
            Dispose();
            if (_subscribeQueue != null)
            {
                _subscribeQueue.Dispose();
            }
        }

        /// <summary>
        /// 释放数据
        /// </summary>
        public void Dispose()
        {
            isdispose = true;
        }

        /// <summary>
        /// 清零回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("PriceHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                // todo

                switch (LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        lock (_baseCTPriceDataViewDic)
                        {
                            _baseCTPriceDataViewDic.Clear();
                        }

                        MDHandler.Instance.ClearCache();

                        break;
                }
            }
        }

        /// <summary>
        /// 测试CT行情数据
        /// </summary>
        /// <param name="obj"></param>
        public void TestOnCTPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            Instance_OnCTPrice(obj);
        }
        #endregion
    }
}
