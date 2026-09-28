using com.gateway.log;
using Newtonsoft.Json;
using BDB.Trade.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Model.TBook;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Api;
using STC.Common.Handler.MarketExec;

using STC.Common.ViewModel.Data;
using STC.Common.Data;
using BDB.Common.Model.OHLC;
using System.Collections.Concurrent;
using System.Threading;
using com.gateway.connector.proto;
using System.Collections;

namespace BDB.Trade.Plugin.Handler.DataProcesser
{
    public class LastExecutionProcesser
    {
        // 订阅Ems订单
        private readonly Object m_OnLastExecutionEventLock = new Object();
        private Action<ExecutionReport> _OnLastExecutions;
        public event Action<ExecutionReport> OnLastExecutions
        {
            add
            {
                lock (m_OnLastExecutionEventLock)
                {
                    _OnLastExecutions += value;
                }
            }
            remove
            {
                lock (m_OnLastExecutionEventLock)
                {
                    _OnLastExecutions -= value;
                }
            }
        }

        /// <summary>
        /// 缓存最新的成交(不包含MANUAL,QUANT,IDB)
        /// key：SecurrityId
        /// value: ExecutionReport
        /// </summary>
        private Dictionary<string, MarketExecutionsItem> _LastExecutionDic = new Dictionary<string, MarketExecutionsItem>();

        private static LastExecutionProcesser proc = new LastExecutionProcesser();

        public static LastExecutionProcesser Processer
        {
            get
            {
                return proc;
            }
        }

        private volatile bool isInit = false;

        private LastExecutionProcesser()
        {
        }

        ~LastExecutionProcesser()
        {
            Dispose();
        }

        private TimeComparer m_TimeComparer = new TimeComparer();

        private object lockObj = new object();

        private Dictionary<string, ObservableCollection<MarketExecutionsItem>> _lastMarketExes = new Dictionary<string, ObservableCollection<MarketExecutionsItem>>();


        public ObservableCollection<MarketExecutionsItem> GetDatasBySecurityID(string securityID)
        {
            lock (this.lockObj)
            {
                if (!string.IsNullOrEmpty(securityID))
                {
                    if (this._lastMarketExes.ContainsKey(securityID) == false && securityID != "xxxxxx")
                    {
                        this._lastMarketExes[securityID] = new ObservableCollection<MarketExecutionsItem>();
                        //BindingOperations.EnableCollectionSynchronization(this._lastMarketExes[securityID], lockObj);
                    }

                    return this._lastMarketExes[securityID];
                }
                return null;
            }
        }

        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private ConcurrentQueue<ExecutionReport> queue = new ConcurrentQueue<ExecutionReport>();
        private SemaphoreSlim semaphore = new SemaphoreSlim(0, int.MaxValue); // 计数信号量
        ExecutionReport proto;
        private bool isStop = false;
        private List<Bond> _CacheBondList = new List<Bond>();
        
        public void Init()
        {
            if (isInit == false)
            {
                BaseHandler.Instance.Init(Instance_OnClear);

                BindingOperations.EnableCollectionSynchronization(_lastMarketExes, lockObj);

                isStop = false;
                //更新深度行情
                Task.Factory.StartNew(() => dealMsg(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

                #region 收MarketExec消息
                ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                #endregion

                STC.Common.DataProcesser.LastExecutionProcesser.Processer.OnGetLastExecutionsItem += GetLastExecutionsItem;

                isInit = true;
            }
        }

        private void dealMsg(CancellationToken cancellationToken)
        {
            while (!isStop)
            {
                try
                {
                    // 使用非阻塞轮询，减少线程切换开销
                    semaphore.Wait(cancellationToken); // 等待新数据到来

                    if (queue.TryDequeue(out proto))
                    {
                        UpdateLastExecutions(proto);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }

        }


        public void Instance_OnMarketExecutions(ExecutionReport obj)
        {
            this.queue.Enqueue(obj);
            semaphore.Release(); // 释放信号量，通知处理线程有新数据到来
        }


        public void Clear()
        {
            //ExecutionsHandler.Instance.OnOwnExecutions -= Instance_OnOwnExecutions;
            ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;
            //TBookHandler.Instance.UnSubscribeBond(Instance_OnBondList);
            lock (this.lockObj)
            {
                if (_lastMarketExes != null)
                {
                    foreach (KeyValuePair<string, ObservableCollection<MarketExecutionsItem>> pair in _lastMarketExes)
                    {
                        foreach (MarketExecutionsItem item in pair.Value)
                        {
                            item.Dispose();
                        }
                        pair.Value.Clear();
                    }
                    // 断开重连后，无需清空_lastMarketExes，只需清空ObservableCollection<MarketExecutionsItem>
                    //_lastMarketExes.Clear();
                }

                if (_LastExecutionDic != null)
                    _LastExecutionDic.Clear();
            }
            
            while (queue.TryDequeue(out proto))
            {
                
            }

            isInit = false;
        }

        public void Dispose()
        {
            this.Clear();
            lock (this.lockObj)
            {
                if (_lastMarketExes != null)
                {
                    _lastMarketExes.Clear();
                    _lastMarketExes = null;
                }
            }
            while (queue.TryDequeue(out proto))
            {
                this.isStop = true;
            }

            isInit = false;
        }

        public void ResetClear()
        {
            LogHelper.Info("LastExecutionProcesser ResetClear.");
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Clear();
                this.Init();
            }));
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info("LastExecutionProcesser Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        this.Clear();
                        this.Init();
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("LastExecutionProcesser Clear exception:" + e.Message);
                    }
                }));
            }
        }

        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOwnExecutions(ExecutionReport obj)
        {
            this.queue.Enqueue(obj);
            semaphore.Release(); // 释放信号量，通知处理线程有新数据到来
        }

        /// <summary>
        /// 收取Bond更新数据
        /// </summary>
        /// <param name="bondList"></param>
        private void Instance_OnBondList(List<Bond> bondList)
        {
            lock (this.lockObj)
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


                    if (this._lastMarketExes.ContainsKey(bond.securityid))
                    {
                        foreach (MarketExecutionsItem item in this._lastMarketExes[bond.securityid])
                        {
                            item.ShowStar = _CacheBondList.Any(p => p.securityid == item.SecurityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        }
                    }

                }
            }
        }

        public void CacheLastExes(MarketExecutionsItem currentExe, string ordStatus, string securtityID, string key)
        {
            lock (lockObj)
            {
                if (!string.IsNullOrEmpty(securtityID))
                {
                    if (_LastExecutionDic.ContainsKey(securtityID))
                    {
                        MarketExecutionsItem oldExe = _LastExecutionDic[securtityID];
                        if (oldExe.Key == key && ordStatus == "4")
                        {
                            _LastExecutionDic.Remove(securtityID);
                            ObservableCollection<MarketExecutionsItem> list = GetDatasBySecurityID(securtityID);
                            if (list.Count > 0)
                            {
                                foreach (MarketExecutionsItem item in list)
                                {
                                    if (item.SourceTrader != "MANUAL"
                                        && item.SourceTrader != "QUANT"
                                        && item.SourceTrader != "IDB")
                                    {
                                        _LastExecutionDic[securtityID] = item;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (currentExe != null && currentExe.Time.CompareTo(oldExe.Time) >= 0
                                && currentExe.SourceTrader != "MANUAL"
                                && currentExe.SourceTrader != "QUANT"
                                && currentExe.SourceTrader != "IDB")
                            {
                                _LastExecutionDic[securtityID] = currentExe;
                            }
                        }
                    }
                    else
                    {
                        if (currentExe != null
                            && currentExe.SourceTrader != "MANUAL"
                            && currentExe.SourceTrader != "QUANT"
                            && currentExe.SourceTrader != "IDB")
                        {
                            _LastExecutionDic[securtityID] = currentExe;
                        }
                    }
                }
            }
        }

        public MarketExecutionsItem GetLastExecutionsItem(string securityID)
        {
            lock (lockObj)
            {
                if (!string.IsNullOrEmpty(securityID) && _LastExecutionDic.ContainsKey(securityID))
                {
                    return _LastExecutionDic[securityID];
                }
                else
                {
                    return null;
                }
            }
        }



        /// <summary>
        /// 更新数据， 好像没有删除数据？？还有很多字段不知道什么意思??key也不确定
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateLastExecutions(ExecutionReport obj)
        {
            Sub_UpdateLastExecutions(obj);
            lock (m_OnLastExecutionEventLock)
            {
                ObservableCollection<MarketExecutionsItem> exeItemList = GetDatasBySecurityID(obj.SecurityID);
                if (exeItemList != null && exeItemList.Count > 0)
                {
                    MarketExecutionsItem lastExeItem = exeItemList[0];
                    ExecutionReport lastOrder = ExecutionsHandler.Instance.GetOrderById(lastExeItem.Key);

                    _OnLastExecutions?.Invoke(lastOrder);
                    STC.Common.DataProcesser.LastExecutionProcesser.Processer.TriggerLastExectuions_BDB(lastOrder);
                }
                else
                {
                    _OnLastExecutions?.Invoke(obj);
                    STC.Common.DataProcesser.LastExecutionProcesser.Processer.TriggerLastExectuions_BDB(obj);
                }
            }
        }

        private void Sub_UpdateLastExecutions(ExecutionReport obj)
        {
            lock (lockObj)
            {
                MarketExecutionsItem marketExe = null;
                try
                {
                    string key = obj.OrderID;
                    if (obj.VenueTypeGW.ToUpper().Equals(ApiConsts.CDH.ToUpper()) || obj.VenueTypeGW.ToUpper().Equals(ApiConsts.SDN.ToUpper()))
                    {
                        key = obj.ExecID;
                    }

                    if (GetDatasBySecurityID(obj.SecurityID).Any<MarketExecutionsItem>(orderItem => (orderItem.Key == key)))
                    {
                        var result = GetDatasBySecurityID(obj.SecurityID).Where(orderItem => orderItem.Key == key);

                        if (obj.OrdStatus == "4" && result != null && result.Count() > 0)
                        {
                            GetDatasBySecurityID(obj.SecurityID).Remove(result.First());
                        }


                        foreach (MarketExecutionsItem item in result)
                        {
                            if (item.Time.CompareTo(obj.TransactTime) >= 0)
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
                        //添加
                        // 如果状态是4， 删除这笔单子。
                        if (obj.OrdStatus == "4")
                        {
                            return;
                        }

                        if (GetDatasBySecurityID(obj.SecurityID).Count == MarketExecCache.Instance.Limit)
                        {
                            MarketExecutionsItem oldItem = GetDatasBySecurityID(obj.SecurityID).Last();
                            if (oldItem.Time.CompareTo(obj.TransactTime) < 0)
                            {
                                GetDatasBySecurityID(obj.SecurityID).Remove(oldItem);
                            }
                            else
                            {
                                return;
                            }
                        }

                        marketExe = new MarketExecutionsItem();
                        marketExe.Key = key;
                        bool isFuture = CommonUtils.IsFutureBond(obj.SecurityID);
                        if (isFuture)
                        {
                            Dictionary<string, string> infoDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.Info1);

                            if (infoDic.ContainsKey("natureKey"))
                            {
                                marketExe.Side = infoDic["natureKey"];
                            }
                            else
                            {
                                marketExe.Side = string.Empty;
                            }
                        }
                        else
                        {
                            marketExe.Side = ConvertSide(obj.Side); // 方向

                        }
                        marketExe.SecurityID = obj.SecurityID; // 代码
                        marketExe.Symbol = obj.Symbol;// 名称
                        marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.LastPx);// 用于显示的成交价
                        marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.LastPx); // 成交价
                        marketExe.SourceTrader = obj.Venues; // 来源
                        marketExe.Time = obj.TransactTime;// 时间
                        marketExe.SettlType = obj.SettlType; // 清算速度
                        marketExe.Info1 = obj.Info1;
                        //星星状态
                        //marketExe.ShowStar = _CacheBondList.Any(p => p.securityid == marketExe.SecurityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        //marketExe.ShowStar = HomePageDataHandler.IsFavorite(marketExe.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                        // GetDatasBySecurityID(obj.SecurityID).Add(marketExe);
                        ObservableCollection<MarketExecutionsItem> list = GetDatasBySecurityID(obj.SecurityID);

                        MarketExecutionsItem lastExeItem = null;
                        if (list != null && list.Count > 0)
                            lastExeItem = list[0];

                        AddOrUpdateDataToList_Sort(false, list, marketExe, 0, list.Count - 1);

                        if (list == null || list.Count <= 1)
                        {
                            marketExe.IsFirst = true;
                            marketExe.DownUp = CommonConstants.NO_UP_DOWN;
                            marketExe.DownUpBrush = "0";
                        }
                        else
                        {
                            LastPriceParam lastPriceParam = CalLastExe(marketExe, lastExeItem);
                            //CalLastExe(obj.SecurityID, ConvertSide(obj.Side), null);
                        }

                        CacheLastExes(marketExe, obj.OrdStatus, obj.SecurityID, obj.ExecID);
                        return;
                    }
                    //更新
                    if (marketExe != null)
                    {
                        // 如果状态是4， 删除这笔单子。
                        if (obj.OrdStatus == "4")
                        {
                            GetDatasBySecurityID(obj.SecurityID).Remove(marketExe);
                            return;
                        }

                        marketExe.Key = key;

                        bool isFuture = CommonUtils.IsFutureBond(obj.SecurityID);
                        if (isFuture)
                        {
                            Dictionary<string, string> infoDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.Info1);

                            if (infoDic.ContainsKey("natureKey"))
                            {
                                marketExe.Side = infoDic["natureKey"];
                            }
                            else
                            {
                                marketExe.Side = string.Empty;
                            }
                        }
                        else
                        {
                            marketExe.Side = ConvertSide(obj.Side); // 方向

                        }

                        marketExe.SecurityID = obj.SecurityID; // 代码
                                                               // 用户显示
                        marketExe.Symbol = obj.Symbol;// 名称
                        marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.LastPx);// 用于显示的成交价
                        marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.LastPx); // 成交价
                        marketExe.SourceTrader = obj.Venues; // 来源
                        marketExe.SettlType = obj.SettlType; // 清算速度
                        marketExe.ExecuteAmount = CommonUtils.ConvertToDecimal(obj.LastQty);
                        marketExe.Info1 = obj.Info1;
                        if (marketExe.Time != obj.TransactTime)
                        {
                            marketExe.Time = obj.TransactTime;// 时间
                            ObservableCollection<MarketExecutionsItem> list = GetDatasBySecurityID(obj.SecurityID);
                            AddOrUpdateDataToList_Sort(true, list, marketExe, 0, list.Count - 1);
                        }

                        //星星状态
                        //marketExe.ShowStar = _CacheBondList.Any(p => p.securityid == marketExe.SecurityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        //marketExe.ShowStar = HomePageDataHandler.Instance.IsFavorite(marketExe.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                        CacheLastExes(marketExe, obj.OrdStatus, obj.SecurityID, obj.ExecID);
                    }
                }
                catch (Exception e)
                {

                    LogHelper.Error("UpdateLastExecutions", e);
                }
                //finally
                //{
                //    CacheLastExes(marketExe, obj.OrdStatus, obj.SecurityID, obj.ExecID);
                //}
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
                    if (item.Time != null && item.Time.CompareTo(halfItem.Time) >= 0)
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
                    if (item.Time != null && item.Time.CompareTo(halfItem.Time) >= 0)
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
                if (item.Time != null && item.Time.CompareTo(halfItem.Time) >= 0)
                {
                    AddOrUpdateDataToList_Sort(isUpdateData, list, item, left, half);
                }
                else
                {
                    AddOrUpdateDataToList_Sort(isUpdateData, list, item, half + 1, right);
                }
            }
        }

        private bool CheckSettle(string settle, List<string> filterSettleList)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(settle) && filterSettleList != null && filterSettleList.Count > 0)
            {
                result = filterSettleList.Contains(settle);

                //处理远期
                if (!result && filterSettleList.Contains(CommonConstants.FWD) && settle.Contains("+"))
                {
                    result = true;
                }
            }
            else
                result = true;

            return result;
        }

        public LastPriceParam CalLastExe(ExecutionReport newExeOrder)
        {
            LastPriceParam lastPriceParam = new LastPriceParam();

            lock (lockObj)
            {
                ObservableCollection<MarketExecutionsItem> exeItemList = GetDatasBySecurityID(newExeOrder.SecurityID);
                if (exeItemList != null && exeItemList.Count > 0)
                {
                    MarketExecutionsItem lastExeItem = exeItemList[0];
                    if (!string.IsNullOrEmpty(lastExeItem.Info1))
                    {

                        decimal dClose = 0;
                        string valuation = null;
                        //MarketCount market = JsonConvert.DeserializeObject<MarketCount>(lastExeItem.Info1);
                        bool isFuture = CommonUtils.IsFutureBond(newExeOrder.SecurityID);
                        if (isFuture)
                        {
                            MarketDataSnapshotFullRefresh futureData = null;
                            List<MarketDataSnapshotFullRefresh> marketDataList = PriceHandler.Instance.GetAllMarketData(newExeOrder.SecurityID);
                            if (marketDataList.Count > 0)
                            {
                                futureData = marketDataList[0];

                                dClose = CommonUtils.ConvertToDecimal(futureData.PreSettlePrice);
                            }
                        }
                        else
                        {
                            if (TradingPlugin.BP_WITH_CCDC == "1")
                            {
                                // 中债估值
                                valuation = PriceHandler.Instance.GetValuation(newExeOrder.SecurityID, string.Empty);
                                if (CommonUtils.HasCDCAuth(newExeOrder.SecurityID))
                                {
                                    if (string.IsNullOrEmpty(valuation))
                                        valuation = CommonConstants.NO_UP_DOWN;
                                    else
                                        valuation = CommonUtils.FormatNum(valuation);
                                }
                                else
                                {
                                    valuation = CommonConstants.NO_PERMISSION;
                                }

                                //dClose = 0;
                                //MarketDataSnapshotFullRefresh futureData = null;
                                //List<MarketDataSnapshotFullRefresh> marketDataList = PriceHandler.Instance.GetAllMarketData(newExeOrder.SecurityID);
                                //if (marketDataList.Count > 0)
                                //{
                                //    futureData = marketDataList[0];
                                    
                                //    dClose = CommonUtils.ConvertToDecimal(futureData.CCDCValuation);
                                //}
                            }
                            else
                            {
                                OhlcMsg ohlcMsg = OhlcHandler.Instance.GetLastOhlc(newExeOrder.SecurityID);
                                dClose = 0;//ohlcMsg != null ? ohlcMsg.close : 0;
                                if (ohlcMsg != null)
                                {
                                    decimal.TryParse(ohlcMsg.inf2, out dClose);
                                }
                            }
                        }
                        decimal dNew = 0;

                        if (TradingPlugin.BP_WITH_CCDC == "1" && !isFuture)
                        {
                            if (valuation == CommonConstants.NO_UP_DOWN || valuation == CommonConstants.NO_PERMISSION)
                            {
                                lastPriceParam.Bp = valuation;
                                lastPriceParam.UpDown = CommonConstants.SIDE_TRD_THREE;
                            }
                            else
                            {
                                decimal ccdc = CommonUtils.ConvertToDecimal(valuation);
                                if (decimal.TryParse(newExeOrder.LastPx, out dNew) && ccdc != 0 && dNew != 0)
                                {
                                    decimal bp = dNew - ccdc;
                                    lastPriceParam.Bp = bp == 0 ? string.Empty : isFuture ? bp.ToString("N3") : (bp * 100).ToString("N2");
                                    lastPriceParam.UpDown = bp > 0 ? CommonConstants.SIDE_BID_ONE : bp < 0 ? CommonConstants.SIDE_OFR_TWO : CommonConstants.SIDE_TRD_THREE;
                                }
                            }
                        }
                        else
                        {
                            if (decimal.TryParse(newExeOrder.LastPx, out dNew) && dClose != 0 && dNew != 0)
                            {
                                decimal bp = dNew - dClose;
                                lastPriceParam.Bp = bp == 0 ? string.Empty : isFuture ? bp.ToString("N3") : (bp * 100).ToString("N2");
                                lastPriceParam.UpDown = bp > 0 ? CommonConstants.SIDE_BID_ONE : bp < 0 ? CommonConstants.SIDE_OFR_TWO : CommonConstants.SIDE_TRD_THREE;
                            }
                        }

                        lastPriceParam.Source = newExeOrder.Venues;
                    }
                }
            }

            return lastPriceParam;
        }

        public LastPriceParam CalLastExe(MarketExecutionsItem first, MarketExecutionsItem second)
        {
            LastPriceParam lastPriceParam = new LastPriceParam();
            if (first != null && second != null)
            {
                decimal bp = first.ExecutePricePercent - second.ExecutePricePercent;
                lastPriceParam.Bp = bp == 0 ? string.Empty : (bp * 100).ToString();

                lastPriceParam.UpDown = bp > 0 ? CommonConstants.SIDE_BID_ONE : bp < 0 ? CommonConstants.SIDE_OFR_TWO : CommonConstants.SIDE_TRD_THREE;

                first.DownUp = bp > 0 ? CommonConstants.UP : bp < 0 ? CommonConstants.DOWN : CommonConstants.NO_UP_DOWN;

                first.DownUpSort = bp > 0 ? 1 : bp < 0 ? 2 : 0;

                first.DownUpBrush = lastPriceParam.UpDown == CommonConstants.SIDE_BID_ONE ? "1" : lastPriceParam.UpDown == CommonConstants.SIDE_OFR_TWO ? "2" : "0";

                lastPriceParam.LastPrice = first.ExecutePricePercent == 0 ? string.Empty : first.ExecutePricePercent.ToString("N4");

                lastPriceParam.Qty = first.ExecuteAmount.ToString();
                lastPriceParam.Source = first.SourceTrader;
            }

            return lastPriceParam;
        }

        public LastPriceParam CalLastExe(string security, string side, List<string> filterSettleList)
        {
            LastPriceParam lastPriceParam = new LastPriceParam();

            lock (lockObj)
            {

                ObservableCollection<MarketExecutionsItem> lastExeItem = GetDatasBySecurityID(security);

                MarketExecutionsItem first = null;
                MarketExecutionsItem second = null;

                //按方向/清算速度取最新两笔计算
                if (CommonConstants.SIDE_BID_ONE == side)
                {
                    foreach (MarketExecutionsItem item in lastExeItem)
                    {
                        if ((item.Side == "Y" || item.Side == "1") && CheckSettle(item.SettlType, filterSettleList))
                        {
                            if (first == null)
                                first = item;
                            else if (second == null)
                                second = item;
                            else
                                break;
                        }
                    }
                }
                else if (CommonConstants.SIDE_OFR_TWO == side)
                {
                    foreach (MarketExecutionsItem item in lastExeItem)
                    {
                        if ((item.Side == "X" || item.Side == "2") && CheckSettle(item.SettlType, filterSettleList))
                        {
                            if (first == null)
                                first = item;
                            else if (second == null)
                                second = item;
                            else
                                break;
                        }
                    }
                }

                if (first != null && second != null)
                {
                    decimal bp = first.ExecutePricePercent - second.ExecutePricePercent;
                    lastPriceParam.Bp = bp == 0 ? string.Empty : (bp * 100).ToString();
                    if (CommonConstants.SIDE_BID_ONE == side)
                    {
                        lastPriceParam.Side = CommonConstants.TKN;
                        lastPriceParam.UpDown = bp > 0 ? CommonConstants.SIDE_BID_ONE : bp < 0 ? CommonConstants.SIDE_OFR_TWO : CommonConstants.SIDE_TRD_THREE;
                    }
                    else if (CommonConstants.SIDE_OFR_TWO == side)
                    {
                        lastPriceParam.Side = CommonConstants.GVN;
                        lastPriceParam.UpDown = bp > 0 ? CommonConstants.SIDE_OFR_TWO : bp < 0 ? CommonConstants.SIDE_BID_ONE : CommonConstants.SIDE_TRD_THREE;
                    }
                    lastPriceParam.LastPrice = first.ExecutePricePercent == 0 ? string.Empty : first.ExecutePricePercent.ToString("N4");
                    lastPriceParam.Side = side;
                    lastPriceParam.Qty = first.ExecuteAmount.ToString();
                    lastPriceParam.Source = first.SourceTrader;
                }
                else
                {
                    if (first != null)
                    {
                        lastPriceParam.Bp = string.Empty;
                        lastPriceParam.Side = side;
                        lastPriceParam.UpDown = string.Empty;
                        lastPriceParam.LastPrice = first.ExecutePricePercent == 0 ? string.Empty : first.ExecutePricePercent.ToString("N4");
                        lastPriceParam.Qty = first.ExecuteAmount.ToString();
                        lastPriceParam.Source = first.SourceTrader;
                    }
                }
            }

            return lastPriceParam;
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
            else if (side == "Y")
                return "1";
            else if (side == "X")
                return "2";

            return side;
        }


        private void GenerateTestData()
        {
            // -------- 创建测试数据 -------------

            //-------------------------------------
        }
    }

    public class TimeComparer : IComparer<MarketExecutionsItem>
    {
        public int Compare(MarketExecutionsItem x, MarketExecutionsItem y)
        {
            return y.Time.CompareTo(x.Time);
        }
    }
}
