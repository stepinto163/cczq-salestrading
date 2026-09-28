using com.gateway.log;
using Newtonsoft.Json;
using Stc.Common.Model.EMS;
using STC.Common.Api;
using STC.Common.Handler.Base;
using STC.Common.Handler.OHLC;
using STC.Common.Handler.TBook;
using STC.Common.Model.Chart;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.OHLC;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.XBond.Plugin.ViewModel;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static STC.XBond.Plugin.Utils.HomePageDataHandler;

namespace STC.XBond.Plugin.Utils.DataProcesser
{
    /// <summary>
    /// 最新成交数据处理器(包含我的成交，cdh）
    /// </summary>
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

        /// <summary>
        /// 构造函数
        /// </summary>
        private LastExecutionProcesser()
        {
            BaseHandler.Instance.Init(Instance_OnClear);          
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~LastExecutionProcesser()
        {
            Dispose();
        }

        /// <summary>
        /// 时间排序器实例
        /// </summary>
        private TimeComparer m_TimeComparer = new TimeComparer();

        /// <summary>
        /// 资源锁
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// 最新成交数据源
        /// </summary>
        private Dictionary<string, ObservableCollection<MarketExecutionsItem>> _lastMarketExes = new Dictionary<string, ObservableCollection<MarketExecutionsItem>>();
        //public Dictionary<string, ObservableCollection<MarketExecutionsItem>> LastMarketExes
        //{
        //    get
        //    {
        //        lock (lockObj)
        //        {
        //            return this._lastMarketExes;
        //        }
        //    }
        //}


        /// <summary>
        /// 获得securityId的市场成交数据
        /// </summary>
        /// <param name="securityID">债券代码</param>
        /// <returns>市场成交数据</returns>
        public ObservableCollection<MarketExecutionsItem> GetDatasBySecurityID(string securityID)
        {
            lock (this.lockObj)
            {
                //if (string.IsNullOrEmpty(securityID) || (this._lastMarketExes.ContainsKey(securityID) == false && securityID != "xxxxxx"))
                //{
                //    this._lastMarketExes[securityID] = new ObservableCollection<MarketExecutionsItem>();
                //    //BindingOperations.EnableCollectionSynchronization(this._lastMarketExes[securityID], lockObj);
                //}
                if (!string.IsNullOrEmpty(securityID))
                {
                    if (this._lastMarketExes.ContainsKey(securityID))
                        return this._lastMarketExes[securityID];
                    else
                    {
                        this._lastMarketExes[securityID] = new ObservableCollection<MarketExecutionsItem>();
                        return this._lastMarketExes[securityID];
                    }
                }
                else
                { 
                    return new ObservableCollection<MarketExecutionsItem>();
                }
            }
        }

        /// <summary>
        /// 处理市场成交数据的线程队列
        /// </summary>
        private ThreadQueue<Common.Model.Fix.Message.ExecutionReport> _ProcessLastExecutioinsThreadQueue;

        /// <summary>
        /// 收藏的债券列表数据
        /// </summary>
        private List<Bond> _CacheBondList = new List<Bond>();
       
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (isInit == false)
            {
                BindingOperations.EnableCollectionSynchronization(_lastMarketExes, lockObj);

                if (_ProcessLastExecutioinsThreadQueue == null)
                {
                    _ProcessLastExecutioinsThreadQueue = new ThreadQueue<Common.Model.Fix.Message.ExecutionReport>(UpdateLastExecutions);
                }

                //#region  创建测试数据
                //this.GenerateTestData();
                //return;
                //#endregion

                #region 收MarketExec消息
                ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                ExecutionsHandler.Instance.OnOwnExecutions += Instance_OnOwnExecutions;
                #endregion

                #region 接收债券列表  
                //todo首页
                //TBookHandler.Instance.Init(null, Instance_OnBondList);
                #endregion

                HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;

                isInit = true;
            }
        }

        /// <summary>
        /// 处理收藏逻辑
        /// </summary>
        /// <param name="type"></param>
        /// <param name="groupID1"></param>
        /// <param name="securityID"></param>
        private void HomePageDataHandler_OnFavoritesChanged(FavoriteTypeEnum type, string groupID1, string securityID)
        {
            lock (this.lockObj)
            {
                if (type == FavoriteTypeEnum.AddBond
                    || type == FavoriteTypeEnum.DelBond)
                {
                     Visibility showStar = HomePageDataHandler.IsFavorite(securityID) == "1" ? Visibility.Visible : Visibility.Hidden;
                    if (this._lastMarketExes.ContainsKey(securityID))
                    {
                        foreach (MarketExecutionsItem item in this._lastMarketExes[securityID])
                        {
                            item.ShowStar = showStar;
                        }
                    }
                }
                else if (type == FavoriteTypeEnum.DelGroup
                    || type == FavoriteTypeEnum.BatchImport)
                {
                    foreach(string key in _lastMarketExes.Keys)
                    {
                        Visibility showStar = HomePageDataHandler.IsFavorite(key) == "1" ? Visibility.Visible : Visibility.Hidden;
                        foreach (MarketExecutionsItem item in this._lastMarketExes[key])
                        {
                            item.ShowStar = showStar;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 市场成交数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (_ProcessLastExecutioinsThreadQueue != null)
            {
                this._ProcessLastExecutioinsThreadQueue.Enqueue(obj);
            }
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            ExecutionsHandler.Instance.OnOwnExecutions -= Instance_OnOwnExecutions;
            ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;
            //TBookHandler.Instance.UnSubscribeBond(Instance_OnBondList);
            HomePageDataHandler.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;
            lock (this.lockObj)
            {
                if (_lastMarketExes != null)
                {
                    foreach (KeyValuePair<string, ObservableCollection<MarketExecutionsItem>> pair in _lastMarketExes)
                    {
                        foreach(MarketExecutionsItem item in pair.Value)
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
            if (_ProcessLastExecutioinsThreadQueue != null)
            {
                _ProcessLastExecutioinsThreadQueue.Clear();
            }

            isInit = false;
        }

        /// <summary>
        /// 释放数据
        /// </summary>
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
            if (_ProcessLastExecutioinsThreadQueue != null)
            {
                _ProcessLastExecutioinsThreadQueue.Clear();
                _ProcessLastExecutioinsThreadQueue.Dispose();
                _ProcessLastExecutioinsThreadQueue = null;
            }

            isInit = false;
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void ResetClear()
        {
            LogHandler.Info("LastExecutionProcesser ResetClear.");
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Clear();
                this.Init();
            }));
        }

        /// <summary>
        /// 清零回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("LastExecutionProcesser Clear");
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
        private void Instance_OnOwnExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            //UpdateMarketExecutions(obj);
            if (_ProcessLastExecutioinsThreadQueue != null)
            {
                this._ProcessLastExecutioinsThreadQueue.Enqueue(obj);
            }
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

        /// <summary>
        /// 缓存最新成交数据
        /// </summary>
        /// <param name="currentExe"></param>
        /// <param name="ordStatus"></param>
        /// <param name="securtityID"></param>
        /// <param name="key"></param>
        public void CacheLastExes(MarketExecutionsItem currentExe, string ordStatus, string securtityID, string key)
        {
            lock(lockObj)
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
                            if (currentExe != null && currentExe.Time.CompareTo(oldExe.Time) > 0
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

        /// <summary>
        /// 通过securityid获得最新成交
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public MarketExecutionsItem GetLastExecutionsItem(string securityID)
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

        /// <summary>
        /// 清空数据
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public void ClearLastExecutionsItem(string securityID)
        {
            if (!string.IsNullOrEmpty(securityID) && _LastExecutionDic.ContainsKey(securityID))
            {
                _LastExecutionDic.Remove(securityID);
            }
        }

        /// <summary>
        /// 更新数据， 好像没有删除数据？？还有很多字段不知道什么意思??key也不确定
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateLastExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            if (obj == null || string.IsNullOrEmpty(obj.SecurityID))
            {
                LogHelper.Info("UpdateLastExecutions obj == null || string.IsNullOrEmpty(obj.SecurityID)");
                return;
            }

            Sub_UpdateLastExecutions(obj);
            lock (m_OnLastExecutionEventLock)
            {
                _OnLastExecutions?.Invoke(obj);
            }
            //}));
        }

        /// <summary>
        /// 处理最新成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Sub_UpdateLastExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.SecurityID))
            {
                LogHelper.Info("Sub_UpdateLastExecutions obj == null || string.IsNullOrEmpty(obj.SecurityID)");
                return;
            }
            MarketExecutionsItem marketExe = null;
            try
            {
                lock (lockObj)
                {
                    //// 只保留CDH
                    //if (!obj.VenueTypeGW.ToUpper().Equals(ApiConsts.CDH.ToUpper()))
                    //{
                    //    return;
                    //}
                    string key = obj.OrderID;
                    if (obj.VenueTypeGW.ToUpper().Equals(ApiConsts.CDH.ToUpper()))
                    {
                        key = obj.ExecID;
                    }

                    if (GetDatasBySecurityID(obj.SecurityID).Any<MarketExecutionsItem>(orderItem => (orderItem.Key == key)))
                    {
                        var result = GetDatasBySecurityID(obj.SecurityID).Where(orderItem => orderItem.Key == key);
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

                        //if (GetDatasBySecurityID(obj.SecurityID).Count == 200)
                        //{
                        //    MarketExecutionsItem oldItem = GetDatasBySecurityID(obj.SecurityID).Last();
                        //    if (oldItem.Time.CompareTo(obj.TransactTime) < 0)
                        //    {
                        //        GetDatasBySecurityID(obj.SecurityID).Remove(oldItem);
                        //    }
                        //    else
                        //    {
                        //        return;
                        //    }
                        //}

                        marketExe = new MarketExecutionsItem();
                        marketExe.Key = key;
                        marketExe.ExecID = obj.ExecID;
                        marketExe.Side = ConvertSide(obj.Side); // 方向
                        marketExe.SecurityID = obj.SecurityID; // 代码

                        marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
                        marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.Yield); // 成交价
                        marketExe.SourceTrader = obj.Venues; // 来源
                        marketExe.Time = obj.TransactTime;// 时间
                        marketExe.SettlType = obj.SettlType; // 清算速度
                        marketExe.Info1 = obj.Info1;
                        marketExe.TimeStamp = obj.UpdateTime;
                        marketExe.Info2 = obj.Info2;
                        marketExe.IsValid = obj.Vs;
                        marketExe.IsValid = marketExe.IsValid == null || marketExe.IsValid == "Y" || marketExe.IsValid == "" ? "Y" : "N";
                        marketExe.VenueTypeGW = obj.VenueTypeGW;

                        //星星状态
                        //marketExe.ShowStar = _CacheBondList.Any(p => p.securityid == marketExe.SecurityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        marketExe.ShowStar = HomePageDataHandler.IsFavorite(marketExe.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                        if (_LastExecutionDic != null && string.IsNullOrEmpty(obj.SecurityID) == false && !_LastExecutionDic.ContainsKey(obj.SecurityID))
                        {
                            marketExe.IsFirst = true;
                            marketExe.DownUp = Constants.NO_UP_DOWN;
                            marketExe.DownUpBrush = "0";
                            marketExe.Bp = Constants.NO_UP_DOWN;
                        }
                        else
                        {
                            //string old = _LastExecutionDic[obj.SecurityID].TimeStamp;
                            //if (old.CompareTo(obj.UpdateTime) < 0)
                            //{
                            //    UpdateBp(ref marketExe, _LastExecutionDic[obj.SecurityID]);
                            //}
                            //else
                            //{
                            //    marketExe.DownUp = Constants.NO_UP_DOWN;
                            //    marketExe.DownUpBrush = "0";
                            //    marketExe.Bp = Constants.NO_UP_DOWN;
                            //}
                        }

                        // GetDatasBySecurityID(obj.SecurityID).Add(marketExe);
                        ObservableCollection<MarketExecutionsItem> list = GetDatasBySecurityID(obj.SecurityID);
                        AddOrUpdateDataToList_Sort(false, list, marketExe, 0, list.Count - 1);

                        UpdateExecutionDic(obj.SecurityID, marketExe);
                        return;
                    }

                    if (marketExe != null)
                    {
                        // 如果状态是4， 删除这笔单子。
                        if (obj.OrdStatus == "4")
                        {
                            GetDatasBySecurityID(obj.SecurityID).Remove(marketExe);
                            return;
                        }

                        marketExe.Key = key;
                        marketExe.ExecID = obj.ExecID;
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
                        marketExe.Info2 = obj.Info2;
                        marketExe.IsValid = obj.Vs;
                        marketExe.IsValid = marketExe.IsValid == null || marketExe.IsValid == "Y" || marketExe.IsValid == "" ? "Y" : "N";

                        // UpdateOrderDp(obj.SecurityID, ref marketExe, obj);
                        //LastPriceParam lastPriceParam = CalLastExe(marketExe, _LastExecutionDic[obj.SecurityID]);

                        //星星状态
                        //marketExe.ShowStar = _CacheBondList.Any(p => p.securityid == marketExe.SecurityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        marketExe.ShowStar = HomePageDataHandler.IsFavorite(marketExe.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;

                        if (marketExe.Time != obj.TransactTime)
                        {
                            marketExe.Time = obj.TransactTime;// 时间
                            ObservableCollection<MarketExecutionsItem> list = GetDatasBySecurityID(obj.SecurityID);
                            AddOrUpdateDataToList_Sort(true, list, marketExe, 0, list.Count - 1);
                        }

                        UpdateExecutionDic(obj.SecurityID, marketExe);

                    }
                }
            }
            catch (Exception e)
            {

                LogHelper.Error("UpdateLastExecutions", e);
            }
            finally
            {

                //CacheLastExes(marketExe, obj.OrdStatus, obj.SecurityID, obj.ExecID);
            }
        }

        private void UpdateOrderDp(string securityID,ref MarketExecutionsItem marketExe, Common.Model.Fix.Message.ExecutionReport newExe)
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
                if (old.CompareTo(newExe.UpdateTime) < 0)
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

        private void UpdateExecutionDic(string securityId, MarketExecutionsItem marketExe)
        {
            if (_LastExecutionDic.ContainsKey(securityId))
            {
                _LastExecutionDic[securityId] = marketExe;
            }
            else
            {
                _LastExecutionDic.Add(securityId, marketExe);
            }
        }

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

        /// <summary>
        /// 按照时间倒序添加数据
        /// </summary>
        /// <param name="addOrUpdate"></param>
        /// <param name="item"></param>
        private void AddOrUpdateDataToList_Sort(bool isUpdateData, ObservableCollection<MarketExecutionsItem> list,MarketExecutionsItem item, int left, int right)
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

        private bool CheckSettle(string settle, List<string> filterSettleList)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(settle) && filterSettleList != null && filterSettleList.Count > 0)
            {
                result = filterSettleList.Contains(settle);

                //处理远期
                if (!result && filterSettleList.Contains(Constants.FWD) && settle.Contains("+"))
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
                if ((LocalEnv.ConfInfo.IsUseOHLCSvr == "2") || (LocalEnv.ConfInfo.IsUseOHLCSvr == "0"))
                {
                    if (exeItemList != null && exeItemList.Count > 0)
                    {
                        MarketExecutionsItem lastExeItem = exeItemList[0];
                        if (!string.IsNullOrEmpty(lastExeItem.Info1))
                        {
                            if (JsonSplit.IsJson(lastExeItem.Info1))
                            {
                                MarketKLine market = JsonConvert.DeserializeObject<MarketKLine>(lastExeItem.Info1);

                                decimal dNew = 0;
                                decimal dClose = 0;
                                if (decimal.TryParse(newExeOrder.LastPx, out dNew) && decimal.TryParse(market.prev_close_yield, out dClose) && dClose != 0 && dNew != 0)
                                {
                                    decimal bp = dNew - dClose;
                                    lastPriceParam.Bp = bp == 0 ? string.Empty : (bp * 100).ToString();
                                    lastPriceParam.UpDown = bp > 0 ? CommonConstants.SIDE_BID_ONE : bp < 0 ? CommonConstants.SIDE_OFR_TWO : CommonConstants.SIDE_TRD_THREE;

                                }

                                lastPriceParam.Source = newExeOrder.Venues;
                            }
                        }
                    }
                }
                else if (LocalEnv.ConfInfo.IsUseOHLCSvr == "1")
                {
                    if (exeItemList != null && exeItemList.Count > 0)
                    {
                        OhlcMsg lastExeItem = OhlcHandler.Instance.GetLastOhlc(newExeOrder.SecurityID);
                        if (lastExeItem != null)
                        {
                            decimal dNew = 0;
                            decimal dClose = 0;
                            if (decimal.TryParse(newExeOrder.LastPx, out dNew) && decimal.TryParse(lastExeItem.inf2, out dClose) && dClose != 0 && dNew != 0)
                            {
                                decimal bp = dNew - dClose;
                                lastPriceParam.Bp = bp == 0 ? string.Empty : (bp * 100).ToString();
                                lastPriceParam.UpDown = bp > 0 ? CommonConstants.SIDE_BID_ONE : bp < 0 ? CommonConstants.SIDE_OFR_TWO : CommonConstants.SIDE_TRD_THREE;
                            }
                            else
                            {
                                lastPriceParam.Bp = CommonConstants.NO_PRICE;
                            }

                            lastPriceParam.Source = newExeOrder.Venues;
                        }
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
                string Bp = bp == 0 ? string.Empty : (bp * 100).ToString();
                first.Bp = Constants.NO_UP_DOWN;
                lastPriceParam.UpDown = bp > 0 ? CommonConstants.SIDE_BID_ONE : bp < 0 ? CommonConstants.SIDE_OFR_TWO : CommonConstants.SIDE_TRD_THREE;

                first.DownUp = bp > 0 ? Constants.UP : bp < 0 ? Constants.DOWN : Constants.NO_UP_DOWN;

                first.DownUpSort = bp > 0 ? 1 : bp < 0 ? 2 : 0;

                first.DownUpBrush = lastPriceParam.UpDown == CommonConstants.SIDE_BID_ONE ? "1" : lastPriceParam.UpDown == CommonConstants.SIDE_OFR_TWO ? "2" : "0";

                if (first.DownUpSort == 0)
                {
                    first.Bp = Constants.NO_UP_DOWN;
                }
                else
                {
                    if (first.DownUpSort == 1)
                    {
                        first.Bp = "+" + Utils.CommonUtils.FormatNum(Bp);
                    }
                    else
                    {
                        first.Bp = Utils.CommonUtils.FormatNum(Bp);
                    }
                }

                lastPriceParam.Bp = first.Bp;

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
                        lastPriceParam.Side = Constants.TKN;
                        lastPriceParam.UpDown = bp > 0 ? CommonConstants.SIDE_BID_ONE : bp < 0 ? CommonConstants.SIDE_OFR_TWO : CommonConstants.SIDE_TRD_THREE;
                    }
                    else if (CommonConstants.SIDE_OFR_TWO == side)
                    {
                        lastPriceParam.Side = Constants.GVN;
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

            return side;
        }

        private void GenerateTestData()
        {
            // -------- 创建测试数据 -------------

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                // 债券列表
                Bond bond1 = new Bond();
                bond1.closeby = "kong2";
                bond1.createtime = DateTime.Now.ToString("hh:mm:ss");
                bond1.enable = "0";
                bond1.ip = "172.16.97.119";
                bond1.remark = "手动关闭";
                bond1.securityid = "140215.IB";
                bond1.settltype = "2";
                bond1.symbol = "140215.IB";
                bond1.terminal = "";
                bond1.updatetime = DateTime.Now.ToString("hh:mm:ss");
                bond1.username = "";
                bond1.venues = "ODM";
                bond1.venuetypegw = "ODM";
                List<Bond> list = new List<Bond>();
                list.Add(bond1);
                Instance_OnBondList(list);

                int i = 0;
                Common.Model.Fix.Message.ExecutionReport er = new Common.Model.Fix.Message.ExecutionReport();
                er.ExecID = string.Format($"e{i}"); // 成交编号
                er.ExecType = "F"; // 执行状态 ; F = 正常 ; 5 = 修改; 4 = 撤销
                er.Info1 = DateTime.Now.ToString("HH:mm:ss");
                er.LastPx = "107.512";  //成交价
                er.LastQty = "0.00000000"; // 成交量
                er.OrdRejReason = "";
                er.OrdStatus = "2"; // 订单状态； 0 = 有效（正常）；2 = 成交；4 = 撤销；I = 冻结；21 = 过期
                er.OrderID = string.Format($"emse{i}"); // 订单编号
                er.PartyName = "";
                er.PartySubID = ""; // 本方交易员id
                er.SecurityID = "140215.IB"; // 代码
                er.Side = "Z";    // 这个里是x, y, z 是什么意思, 是否是分别代表GVN，TKN，Refer
                er.StandardHeader.MsgType = "8";
                er.VenueTypeGW = "CDH";
                er.Venues = "CTTJ";
                UpdateLastExecutions(er);

                i++;
                Common.Model.Fix.Message.ExecutionReport er1 = new Common.Model.Fix.Message.ExecutionReport();
                er1.ExecID = string.Format($"e{i}"); // 成交编号
                er1.ExecType = "F"; // 执行状态 ; F = 正常 ; 5 = 修改; 4 = 撤销
                er1.Info1 = DateTime.Now.ToString("HH:mm:ss");
                er1.LastPx = "100.9956";  //成交价
                er1.LastQty = "0.00000000"; // 成交量
                er1.OrdRejReason = "";
                er1.OrdStatus = "2"; // 订单状态； 0 = 有效（正常）；2 = 成交；4 = 撤销；I = 冻结；21 = 过期
                er1.OrderID = string.Format($"emse{i}"); // 订单编号
                er1.PartyName = "";
                er1.PartySubID = ""; // 本方交易员id
                er1.SecurityID = "190305.IB"; // 代码
                er1.Side = "Y";    // 这个里是x, y, z 是什么意思
                er1.StandardHeader.MsgType = "8";
                er1.VenueTypeGW = "CDH";
                er1.Venues = "PTCN";
                UpdateLastExecutions(er1);

                i++;
                Common.Model.Fix.Message.ExecutionReport er2 = new Common.Model.Fix.Message.ExecutionReport();
                er2.ExecID = string.Format($"e{i}"); // 成交编号
                er2.ExecType = "F"; // 执行状态 ; F = 正常 ; 5 = 修改; 4 = 撤销
                er2.Info1 = "09:00:00";
                er2.LastPx = "101.1401";  //成交价
                er2.LastQty = "0.00000000"; // 成交量
                er2.OrdRejReason = "";
                er2.OrdStatus = "2"; // 订单状态； 0 = 有效（正常）；2 = 成交；4 = 撤销；I = 冻结；21 = 过期
                er2.OrderID = string.Format($"emse{i}"); // 订单编号
                er2.PartyName = "";
                er2.PartySubID = ""; // 本方交易员id
                er2.SecurityID = "190215.IB"; // 代码
                er2.Side = "X";    // 这个里是x, y, z 是什么意思
                er2.StandardHeader.MsgType = "8";
                er2.VenueTypeGW = "CDH";
                er2.Venues = "CCTB";
                UpdateLastExecutions(er2);

            });
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
