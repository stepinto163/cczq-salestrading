using com.gateway.log;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Handler.Price;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Common;
using netapi.Api;
using STC.Common.Api;
using STC.Common.ViewModel.Data;
using System.Windows;
using System.Windows.Threading;

namespace BDB.Trade.Plugin.Handler
{
    public class PriceHandler
    {
        private static PriceHandler server = new PriceHandler();
        public static PriceHandler Instance { get { return server; } }

        /////// <summary>
        /////// 更新价格的线程
        /////// </summary>
        ////private Thread _updatePriceThread;

        private AsyncThreadQueue<string[]> _subscribeQueue;

        private PriceHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);

            ////// 定时检查 ctprice是否需要更新。
            ////if (_updatePriceThread == null)
            ////{
            ////    _updatePriceThread = new Thread(UpdatePrice);
            ////    _updatePriceThread.IsBackground = true;
            ////    _updatePriceThread.Start();
            ////}

            // 订阅线程
            if (_subscribeQueue == null)
            {
                _subscribeQueue = new AsyncThreadQueue<string[]>(ProcessSub);
            }

            // 非堵塞订单处理线程
            // 启动深度行情
            if (_updateCTPMarketDataThread == null)
            {
                _updateCTPMarketDataThread = new Thread(UpdateCTPMarketDataProcessor);
                _updateCTPMarketDataThread.Priority = ThreadPriority.AboveNormal;
                _updateCTPMarketDataThread.Start();
            }
        }

        ~PriceHandler()
        {
            Dispose();

            ////if (_updatePriceThread != null)
            ////{
            ////    _updatePriceThread.Abort();
            ////}

            if (_subscribeQueue != null)
            {
                _subscribeQueue.Dispose();
            }
        }

        private void ProcessSub(string[] param)
        {
            string ric = string.Empty;
            string type = string.Empty;
            try
            {
                if (param.Length == 2)
                {
                    ric = param[0];
                    type = param[1];

                    if (type == "SubscribeCT")
                    {
                        MDHandler.Instance.SubscribeCT(ric, Instance_OnCTPrice);
                    }
                    else if (type == "UnsubscribeCT")
                    {
                        MDHandler.Instance.UnsubscribeCT(ric, Instance_OnCTPrice);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error($"{type}：{ric}", e);
            }

        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info("PriceHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                lock (_baseCTPriceDataViewDic)
                {
                    _baseCTPriceDataViewDic.Clear();
                }

                ////foreach (var ctPair in _UpdateCTPricesDic)
                ////{
                ////    string primaryKey = ctPair.Key;
                ////    lock (string.Intern(primaryKey))
                ////    {
                ////        _UpdateCTPricesDic.AddOrUpdate(primaryKey, false, (key, value) => false);

                ////    }
                ////}

                MDHandler.Instance.ClearCache();

            }
        }

        ///// <summary>
        ///// 深度行情是否要更新情况
        ///// </summary>
        //private ConcurrentDictionary<string, bool> _UpdateCTPricesDic = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 最优行情是否要更新情况
        /// </summary>
        private ConcurrentDictionary<string, bool> _UpdateH0PricesDic = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 深度行情回调函数
        /// </summary>
        private ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>> _UpdateCTCallbackDic = new ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>>();

        ///// <summary>
        ///// 最优行情回调函数
        ///// </summary>
        //private ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>> _UpdateH0CallbackDic = new ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>>();

        //存储CT初级处理数据（把MarketDataSnapshotFullRefresh转换成DataView数据结构）
        private ConcurrentDictionary<string, DataView> _baseCTPriceDataViewDic = new ConcurrentDictionary<string, DataView>();


        private object _mdLock = new object();
        private object _hoLock = new object();

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


        public List<MarketDataSnapshotFullRefresh> GetAllMarketData(string broker, string securityID)
        {
            List<MarketDataSnapshotFullRefresh> result = new List<MarketDataSnapshotFullRefresh>();

            List<string> ric = TopicUtil.GetMdRic(securityID, LocalEnv.UserInfo.BonDPermissions);

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


        /// <summary>
        /// 注册CT价格
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="CTPriceCallBack"></param>
        public void RegisterCTPrice(string securityID, Action<MarketDataSnapshotFullRefresh> CTPriceCallBack)
        {
            if (string.IsNullOrEmpty(securityID))
                return;

            List<string> ric = TopicUtil.GetMdRic(securityID, LocalEnv.UserInfo.BonDPermissions);

            foreach (string str in ric)
            {
                string primaryKey = str;
                if (CTPriceCallBack != null)
                {
                    if (_UpdateCTCallbackDic.ContainsKey(primaryKey) == false || _UpdateCTCallbackDic[primaryKey] == null)
                    {
                        Action<MarketDataSnapshotFullRefresh> newAction = null;
                        _UpdateCTCallbackDic.AddOrUpdate(primaryKey, newAction, (key, value) => newAction);
                        _UpdateCTCallbackDic[primaryKey] += CTPriceCallBack;
                        LogHelper.Info(string.Format("RegisterCTPrice:{0}", primaryKey));
                    }
                    else
                    {
                        int index = Array.IndexOf(_UpdateCTCallbackDic[primaryKey].GetInvocationList(), CTPriceCallBack);
                        if (index < 0)
                        {
                            _UpdateCTCallbackDic[primaryKey] += CTPriceCallBack;
                        }
                    }
                }


                lock (string.Intern(primaryKey))
                {
                    MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                    if (_UpdateCTCallbackDic.ContainsKey(primaryKey) && ctPrice != null)
                    {
                        if (!ctPrice.ContainsKey("isCache"))
                        {
                            ctPrice.Add("isCache", true);

                        }

                        CTPriceCallBack?.Invoke(ctPrice);
                    }
                }

                //_UpdateCTPricesDic.AddOrUpdate(primaryKey, true, (key, value) => true);
                _subscribeQueue.Enqueue(new string[] { securityID, "SubscribeCT" });

            }

        }

        
        /// <summary>
        /// 做市走势报价（CT）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnCTPrice(MarketDataSnapshotFullRefresh obj)
        {
            ////string primaryKey = TopicUtil.GetMdRic(obj.SecurityID, LocalEnv.UserInfo.BonDPermissions)[0];
            ////lock (string.Intern(primaryKey))
            ////{
            ////    if (_UpdateCTCallbackDic.ContainsKey(primaryKey))
            ////    {
            ////        _UpdateCTPricesDic.AddOrUpdate(primaryKey, true, (key, value) => true);

            ////    }
            ////}

            // 将数据放入队列
            dataCTPQueue.Enqueue(obj);
            // 设置事件，通知消费者有新数据
            dataCTPAvailable.Set();
        }

        #region 刷新_baseCTPriceDataViewDic, 计算最优价
        private void RefreshBaseCTPriceDataView(string key, MarketDataSnapshotFullRefresh obj)
        {
            lock (_baseCTPriceDataViewDic)
            {
                if (_baseCTPriceDataViewDic.ContainsKey(key) == false)
                {
                    _baseCTPriceDataViewDic[key] = CTPriceControler.CreateBaseDataView();
                }

                CTPriceControler.UpdateDataToBaseDV(_baseCTPriceDataViewDic[key], obj);
            }
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
                priceType = CommonUtils.ConvertToString(priceBaseData[0][CTPriceControler.PRICETYPE_FIELD]);
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

        #endregion

        /// <summary>
        /// 注销CTPrice
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="CTPriceCallBack"></param>
        public void UnregisterCTPrice(string securityID, Action<MarketDataSnapshotFullRefresh> CTPriceCallBack)
        {
            lock (_mdLock)
            {
                List<string> ric = TopicUtil.GetMdRic(securityID, LocalEnv.UserInfo.BonDPermissions);

                foreach (string str in ric)
                {
                    string primaryKey = str;//TopicUtil.getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, securityID, string.Empty, string.Empty, string.Empty);
                    lock (string.Intern(primaryKey))
                    {
                        if (_UpdateCTCallbackDic.ContainsKey(primaryKey))
                        {
                            _UpdateCTCallbackDic[primaryKey] -= CTPriceCallBack;

                            Action<MarketDataSnapshotFullRefresh> tem = _UpdateCTCallbackDic[primaryKey];

                            // 如果没人订阅就UnSubscribe
                            if (tem == null || tem.GetInvocationList().Length <= 0)//[primaryKey] == null)
                            {
                                _UpdateCTCallbackDic.TryRemove(primaryKey, out tem);

                                LogHelper.Info("MDHandler.Instance.UnsubscribeCT:" + securityID);
                                
                                _subscribeQueue.Enqueue(new string[] { securityID, "UnsubscribeCT" });
                            }
                        }
                    }
                }
            }
        }


        #region 行情处理线程  Author:chenhu Date:20240723 Func:解决堵塞等问题
        /// <summary>
        /// 释放数据
        /// </summary>
        public void Dispose()
        {
            isdispose = true;
        }


        private object lockObj = new object();
        //深度行情是否要更新情况
        private Thread _updateCTPMarketDataThread;

        //深度行情队列
        private ConcurrentQueue<MarketDataSnapshotFullRefresh> dataCTPQueue = new ConcurrentQueue<MarketDataSnapshotFullRefresh>();

        private ManualResetEventSlim dataCTPAvailable = new ManualResetEventSlim(false);

        private bool isdispose = false;
        private void UpdateCTPMarketDataProcessor()
        {
            while (!isdispose)
            {
                // 等待数据可用
                dataCTPAvailable.Wait();

                MarketDataSnapshotFullRefresh data;

                while (dataCTPQueue.TryDequeue(out data))
                {
                    if (data == null) // 生产结束的标记
                        return;

                    // 深度行情队列
                    ProcessCTPMarketData(data);
                }

                // 重置事件
                dataCTPAvailable.Reset();
            }
        }


        /// <summary>
        /// 深度行情队列
        /// </summary>
        /// <param name="data"></param>
        private void ProcessCTPMarketData(MarketDataSnapshotFullRefresh obj)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string key = TopicUtil.GetMdRic(obj.SecurityID, LocalEnv.UserInfo.BonDPermissions)[0];
                    string primaryKey = key.Contains("=CT") ? key.Replace("=CT", "=ct").Replace("stc.md.CT", "stc.md.ct") : key;

                    lock (string.Intern(primaryKey))
                    {
                        MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);

                        if (ctPrice != null)
                        {
                            RefreshBaseCTPriceDataView(primaryKey, ctPrice);

                            if (_UpdateCTCallbackDic.ContainsKey(primaryKey))
                            {
                                _UpdateCTCallbackDic[primaryKey]?.Invoke(ctPrice);
                            }
                        }
                    }

                }, DispatcherPriority.Send);
            });
        }

        #endregion
    }
}
