using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Handler.Base;
using STC.Common.Handler.Price;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.Mm.Plugin.ViewModel.Data;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static STC.Common.Model.Fix.Message.MarketDataSnapshotFullRefresh;

namespace STC.Mm.Plugin.Utils
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

        
        /// <summary>
        /// 深度行情是否要更新情况
        /// </summary>
        private ConcurrentDictionary<string, bool> _UpdateCTPricesDic = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 最优行情是否要更新情况
        /// </summary>
        private ConcurrentDictionary<string, bool> _UpdateH0PricesDic = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 深度行情回调函数
        /// </summary>
        private ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>> _UpdateCTCallbackDic = new ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>>();

        /// <summary>
        /// 最优行情回调函数
        /// </summary>
        private ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>> _UpdateH0CallbackDic = new ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>>();

        /// <summary>
        /// 更新价格的线程
        /// </summary>
        private Thread _updatePriceThread;

        /// <summary>
        /// 构造函数
        /// </summary>
        private PriceHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
            // 定时检查 ctprice是否需要更新。
            if (_updatePriceThread == null)
            {
                _updatePriceThread = new Thread(UpdatePrice);
                _updatePriceThread.IsBackground = true;
                _updatePriceThread.Start();
            }
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
            }
        }

        /// <summary>
        /// 注册H0价格
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="H0PriceCallBack"></param>
        public void RegisterH0Price(string securityID, Action<MarketDataSnapshotFullRefresh> H0PriceCallBack)
        {
            string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, securityID, string.Empty, string.Empty);
            lock (primaryKey)
            {
                //MDHandler.Instance.Init(securityID, null, Instance_OnMDPrice);
                MDHandler.Instance.SubscribeBest(securityID, Instance_OnMDPrice);
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

                _UpdateH0PricesDic.AddOrUpdate(primaryKey, true, (key, value) => true);

            }
        }

        /// <summary>
        /// 注销H0价格
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="H0PriceCallBack"></param>
        public void UnregisterH0Price(string securityID, Action<MarketDataSnapshotFullRefresh> H0PriceCallBack)
        {
            string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, securityID, string.Empty, string.Empty);
            lock (primaryKey)
            {
                if (_UpdateH0CallbackDic.ContainsKey(primaryKey))
                {
                    _UpdateH0CallbackDic[primaryKey] -= H0PriceCallBack;
                }

                //如果没人订阅就UnSubscribe
                if (_UpdateH0CallbackDic[primaryKey] == null)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            MDHandler.Instance.UnsubscribeBest(securityID, Instance_OnMDPrice);
                        }
                        catch (Exception e)
                        {
                            LogHelper.Error("UnregisterH0Price", e);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 注册CT价格
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="CTPriceCallBack"></param>
        public void RegisterCTPrice(string securityID, Action<MarketDataSnapshotFullRefresh> CTPriceCallBack)
        {
            try
            {
                string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, securityID, string.Empty, string.Empty);
                lock (primaryKey)
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
                        }
                        else
                        {
                            int index = Array.IndexOf(_UpdateCTCallbackDic[primaryKey].GetInvocationList(), (Action<MarketDataSnapshotFullRefresh>)CTPriceCallBack);
                            if (index < 0)
                            {
                                _UpdateCTCallbackDic[primaryKey] += CTPriceCallBack;
                            }
                        }
                    }

                    _UpdateCTPricesDic.AddOrUpdate(primaryKey, true, (key, value) => true);

                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            MDHandler.Instance.SubscribeCT(securityID, Instance_OnCTPrice);
                        }
                        catch (Exception e)
                        {
                            LogHelper.Error("RegisterCTPrice", e);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 注销CTPrice
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="CTPriceCallBack"></param>
        public void UnregisterCTPrice(string securityID, Action<MarketDataSnapshotFullRefresh> CTPriceCallBack)
        {
            string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, securityID, string.Empty, string.Empty);
            lock (primaryKey)
            {
                if (_UpdateCTCallbackDic.ContainsKey(primaryKey))
                {
                    _UpdateCTCallbackDic[primaryKey] -= CTPriceCallBack;
                    LogHelper.Info("PriceHandler _UpdateCTCallbackDic:" + primaryKey);
                    // 如果没人订阅就UnSubscribe
                    if (_UpdateCTCallbackDic.Count <= 0)//[primaryKey] == null)
                    {
                        LogHelper.Info("MDHandler.Instance.UnsubscribeCT:" + securityID);
                        //MDHandler.Instance.UnsubscribeCT(securityID, Instance_OnCTPrice);

                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                MDHandler.Instance.UnsubscribeCT(securityID, Instance_OnCTPrice);
                            }
                            catch (Exception e)
                            {
                                LogHelper.Error("UnregisterCTPrice", e);
                            }
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 更新行情
        /// </summary>
        /// <param name="obj"></param>
        private void UpdatePrice()
        {
            while (true)
            {
                // 更新最优报价
                foreach(var h0Pair in _UpdateH0PricesDic)
                {
                    string primaryKey = h0Pair.Key;
                    if (h0Pair.Value == true)
                    {
                        lock (string.Intern(primaryKey))
                        {
                            MarketDataSnapshotFullRefresh h0Price = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                            // 通知界面
                            if (_UpdateH0CallbackDic.ContainsKey(primaryKey) && h0Price != null)
                            {
                                _UpdateH0CallbackDic[primaryKey]?.Invoke(h0Price);
                            }

                            _UpdateH0PricesDic.AddOrUpdate(primaryKey, false, (key, value) => false);
                        }
                    }
                    //lock (string.Intern(primaryKey))
                    //{
                    //    _UpdateH0PricesDic.AddOrUpdate(primaryKey, false, (key, value) => false);
                    //}
                }


                // 更新深度行情
                foreach (var ctPair in _UpdateCTPricesDic)
                {
                    string primaryKey = ctPair.Key;
                    if (ctPair.Value == true)
                    {
                        if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                        {
                            LogHandler.Info($"TBook行情 有更新 {primaryKey}");
                        }

                        lock (string.Intern(primaryKey))
                        {
                            MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                            if (_UpdateCTCallbackDic.ContainsKey(primaryKey) && ctPrice != null)
                            {
                                if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                                {
                                    LogHandler.Info($"TBook行情 Callback  {primaryKey}");

                                    foreach (GNoMDEntries noMDEntry in ctPrice.NoMDEntries)
                                        LogHelper.Info(String.Format("Security:{0}, time:{1}, price:{2}", ctPrice.SecurityID, noMDEntry.MDEntryTime, noMDEntry.MDEntryPx));
                                }

                                _UpdateCTCallbackDic[primaryKey]?.Invoke(ctPrice);
                                _UpdateCTPricesDic.AddOrUpdate(primaryKey, false, (key, value) => false);

                            }
                        }
                    }
                    //lock (string.Intern(primaryKey))
                    //{
                    //    _UpdateCTPricesDic.AddOrUpdate(primaryKey, false, (key, value) => false);
                    //}
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// 做市走势报价（H0）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMDPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, obj.SecurityID, string.Empty, string.Empty);
            lock (string.Intern(primaryKey))
            {
                _UpdateH0PricesDic.AddOrUpdate(primaryKey, true, (key, value) => true);
            }
        }

        /// <summary>
        /// 做市走势报价（CT）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnCTPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, obj.SecurityID, string.Empty, string.Empty);
            //if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
            //{
            //    LogHandler.Info($"TBook行情 {primaryKey}");
            //}
            lock (string.Intern(primaryKey))
            {
                _UpdateCTPricesDic.AddOrUpdate(primaryKey, true, (key,value)=>true);
            }
        }

        /// <summary>
        /// 测试MD行情数据
        /// </summary>
        /// <param name="obj"></param>
        public void TestOnMDPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            Instance_OnMDPrice(obj);
        }

        /// <summary>
        /// 测试CT行情数据
        /// </summary>
        /// <param name="obj"></param>
        public void TestOnCTPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            Instance_OnCTPrice(obj);
        }

        /// <summary>
        /// 根据债券代码， 返回深度行情报价
        /// </summary>
        /// <param name="securityID">债券代码</param>
        /// <returns>缓存的深度行情报价</returns>
        public MarketDataSnapshotFullRefresh GetCacheCTPrice(string securityID)
        {
            string ctTopic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, securityID, string.Empty, string.Empty);
            return MDHandler.Instance.GetCachePriceByTopic(ctTopic);
        }

        /// <summary>
        /// 根据债券代码， 返回最优行情报价
        /// </summary>
        /// <param name="securityID">债券代码</param>
        /// <returns>缓存的最优行情报价</returns>
        public MarketDataSnapshotFullRefresh GetCacheH0Price(string securityID)
        {
            string h0Topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, securityID, string.Empty, string.Empty);
            return MDHandler.Instance.GetCachePriceByTopic(h0Topic);
        }

        /// <summary>
        /// 根据方向和清算速度筛选数据
        /// </summary>
        /// <param name="gNoMDEntriesList">原始price列表</param>
        /// <param name="side">方向</param>
        /// <param name="settlType">清算速度</param>
        /// <returns></returns>
        public List<MarketDataSnapshotFullRefresh.GNoMDEntries> GetPriceList(List<MarketDataSnapshotFullRefresh.GNoMDEntries> gNoMDEntriesList, string side, string settlType)
        {
            string[] arr = settlType.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            List<MarketDataSnapshotFullRefresh.GNoMDEntries> list = new List<MarketDataSnapshotFullRefresh.GNoMDEntries>();
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
        public decimal GetBestBidPrice(List<MarketDataSnapshotFullRefresh.GNoMDEntries> bidPriceList)
        {
            decimal bestPrice = decimal.MaxValue;
            foreach (MarketDataSnapshotFullRefresh.GNoMDEntries price in bidPriceList)
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
        public decimal GetBestOfrPrice(List<MarketDataSnapshotFullRefresh.GNoMDEntries> ofrPriceList)
        {
            decimal bestPrice = decimal.MinValue;
            foreach (MarketDataSnapshotFullRefresh.GNoMDEntries price in ofrPriceList)
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
        public BestPrice GenBestPrice(IEnumerable<MarketDataSnapshotFullRefresh.GNoMDEntries> priceList, decimal bestPrice)
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


                foreach (MarketDataSnapshotFullRefresh.GNoMDEntries entry in list)
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
    }
}
