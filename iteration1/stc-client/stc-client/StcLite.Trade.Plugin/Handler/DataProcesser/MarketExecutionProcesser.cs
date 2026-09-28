using com.gateway.log;
using Newtonsoft.Json;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using STC.Common.Handler.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.Model.Base;
using Stc.Common.Model.EMS;
using STC.Common.ViewModel.Data;
using CTPriceControler = BDB.Trade.Plugin.ViewModel.Trade.CTPriceControler;

namespace BDB.Trade.Plugin.Handler.DataProcesser
{
    public class MarketExecutionProcesser
    {
        private static MarketExecutionProcesser proc = new MarketExecutionProcesser();

        public static MarketExecutionProcesser Processer
        {
            get
            {
                return proc;
            }
        }

        private volatile bool isInit = false;

        private MarketExecutionProcesser()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        ~MarketExecutionProcesser()
        {
            Dispose();
        }

        private object lockObj = new object();

        private ObservableCollection<MarketExecutionsItem> _myMarketExes = new ObservableCollection<MarketExecutionsItem>();
        public ObservableCollection<MarketExecutionsItem> MyMarketExes
        {
            get
            {
                lock (this.lockObj)
                {
                    //BindingOperations.EnableCollectionSynchronization(_myMarketExes, lockObj);
                    return this._myMarketExes;
                }
            }
        }

        private const string SECURITYID_FIELD = "SecurityID";
        private const string TRANSACTIONTIME_FIELD = "TransactionTime";
        private const string EXEID_FIELD = "ExeID";
        private const string PRICE_FIELD = "Price";
       
        private DataView _PrimaryKeyDV;
        private DataView PrimaryKeyDV
        {
            get
            {
                if (_PrimaryKeyDV == null)
                {
                    _PrimaryKeyDV = new DataTable().DefaultView;
                    _PrimaryKeyDV.Table.Columns.Add($"{SECURITYID_FIELD}");
                    _PrimaryKeyDV.Table.Columns.Add($"{TRANSACTIONTIME_FIELD}");
                    _PrimaryKeyDV.Table.Columns.Add($"{EXEID_FIELD}");
                    _PrimaryKeyDV.Table.Columns.Add($"{PRICE_FIELD}");

                    _PrimaryKeyDV.Table.PrimaryKey = new DataColumn[] { _PrimaryKeyDV.Table.Columns[$"{ EXEID_FIELD}"] };
                    _PrimaryKeyDV.Sort = $"{TRANSACTIONTIME_FIELD} DESC";
                    //_PrimaryKeyDV.Sort = $"{EXEID_FIELD},{TRANSACTIONTIME_FIELD}";
                }
                return _PrimaryKeyDV;
            }
        }

        private ConcurrentDictionary<string, int> countDic = new ConcurrentDictionary<string, int>();

        private AsyncThreadQueue<ExecutionReport> _ProcessMarketExecutioinsThreadQueue;

        private List<Bond> _CacheBondList = new List<Bond>();

        public void Init()
        {
            if (isInit == false)
            {

                HomePageDataHandler.Instance.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;

                BindingOperations.EnableCollectionSynchronization(_myMarketExes, lockObj);

                if (_ProcessMarketExecutioinsThreadQueue == null)
                {
                    _ProcessMarketExecutioinsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateMarketExecutions);
                }

                //#region  创建测试数据
                //this.GenerateTestData();
                //return;
                //#endregion


                #region 收MarketExec消息
                ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                #endregion

                #region 接收债券列表
                // todo首页
                //TBookHandler.Instance.Init(null, Instance_OnBondList);
                #endregion

                isInit = true;
            }
        }

        private void HomePageDataHandler_OnFavoritesChanged(FavoriteTypeEnum type, string groupID, string securityID, string bondId)
        {
            //lock (this.lockObj)
            //{
            //    if (type == FavoriteTypeEnum.AddBond
            //        || type == FavoriteTypeEnum.DelBond)
            //    {
            //        Visibility showStar = HomePageDataHandler.Instance.IsFavorite(securityID) == "1" ? Visibility.Visible : Visibility.Hidden;

            //        foreach (MarketExecutionsItem item in _myMarketExes.Where(o => { return o.SecurityID == securityID; }))
            //        {
            //            item.ShowStar = showStar;
            //        }
            //    }
            //    else if (type == FavoriteTypeEnum.DelGroup
            //        || type == FavoriteTypeEnum.BatchImport)
            //    {
            //        Dictionary<string, Visibility> showStarDic = new Dictionary<string, Visibility>();

            //        foreach (MarketExecutionsItem item in _myMarketExes)
            //        {
            //            if (showStarDic.ContainsKey(item.SecurityID) == false)
            //            {
            //                Visibility showStar = HomePageDataHandler.Instance.IsFavorite(item.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;
            //                showStarDic[item.SecurityID] = showStar;

            //            }
            //            item.ShowStar = showStarDic[item.SecurityID];
            //        }
            //    }
            //}
        }

        public void Clear()
        {
            HomePageDataHandler.Instance.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;

            ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;
            //TBookHandler.Instance.UnSubscribeBond(Instance_OnBondList);


            lock (this.lockObj)
            {
                if (countDic != null)
                {
                    countDic.Clear();
                }

                if (_PrimaryKeyDV != null)
                {
                    _PrimaryKeyDV.Table.Rows.Clear();
                }

                if (_myMarketExes != null)
                {
                    foreach (MarketExecutionsItem item in _myMarketExes)
                    {
                        //item.Dispose();
                    }
                    _myMarketExes.Clear();
                }
            }
            if (_ProcessMarketExecutioinsThreadQueue != null)
            {
                _ProcessMarketExecutioinsThreadQueue.Clear();
            }

            isInit = false;
        }

        public void Dispose()
        {
            this.Clear();

            lock (this.lockObj)
            {
                if (countDic != null)
                {
                    countDic.Clear();
                    countDic = null;
                }

                if (_PrimaryKeyDV != null)
                {
                    _PrimaryKeyDV.Table.Clear();
                    _PrimaryKeyDV.Table.Dispose();
                    _PrimaryKeyDV.Dispose();
                    _PrimaryKeyDV = null;
                }

                if (_myMarketExes != null)
                {
                    foreach (MarketExecutionsItem item in _myMarketExes)
                    {
                        item.Dispose();
                    }
                    _myMarketExes.Clear();
                    _myMarketExes = null;
                }
            }
            if (_ProcessMarketExecutioinsThreadQueue != null)
            {
                _ProcessMarketExecutioinsThreadQueue.Clear();
                _ProcessMarketExecutioinsThreadQueue.Dispose();
                _ProcessMarketExecutioinsThreadQueue = null;
            }

            isInit = false;
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("MarketExecutionProcesser Clear");
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
                        LogHelper.Error("MarketExecutionProcesser Clear exception:" + e.Message);
                    }
                }));
            }
        }

        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMarketExecutions(ExecutionReport obj)
        {
            //UpdateMarketExecutions(obj);
            if (_ProcessMarketExecutioinsThreadQueue != null)
            {
                this._ProcessMarketExecutioinsThreadQueue.Enqueue(obj);
            }
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
        }

      
        /// <summary>
        /// 更新数据， 好像没有删除数据？？还有很多字段不知道什么意思??key也不确定
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMarketExecutions(ExecutionReport obj)
        {
            lock (lockObj)
            {
                DataRow findDr = this.PrimaryKeyDV.Table.Rows.Find(obj.ExecID);
                if (findDr == null)
                {
                    // 如果状态是4， 删除这笔单子。
                    if (obj.OrdStatus == "4")
                    {
                        return;
                    }

                    // 计数
                    if (this.countDic.ContainsKey(obj.SecurityID) == false)
                    {
                        this.countDic[obj.SecurityID] = 0;
                    }

                    //if (this.countDic[obj.SecurityID] == 200)
                    //{
                    //    // 删除老数据
                    //    this.PrimaryKeyDV.RowFilter = $"{SECURITYID_FIELD} = '{obj.SecurityID}'";

                    //    string oldExecID = CommonUtils.ConvertToString(this.PrimaryKeyDV[199][EXEID_FIELD]); //this.PrimaryKeyDV.Table.Rows[199][EXEID_FIELD]);

                    //    string oldTime = CommonUtils.ConvertToString(this.PrimaryKeyDV[199][TRANSACTIONTIME_FIELD]);
                    //    if (oldTime.CompareTo(obj.TransactTime) <= 0)
                    //    {

                    //        this.PrimaryKeyDV.Table.Rows.Remove(this.PrimaryKeyDV[199].Row);

                    //        DeleteRecord(oldExecID);
                    //    }
                    //    else
                    //        return;
                    //}
                    //else
                    {
                        this.countDic[obj.SecurityID]++;
                    }

                    // 存储基础数据
                    DataRow newDr = this.PrimaryKeyDV.Table.NewRow();
                    newDr[EXEID_FIELD] = obj.ExecID;
                    newDr[TRANSACTIONTIME_FIELD] = obj.TransactTime;
                    newDr[SECURITYID_FIELD] = obj.SecurityID;
                    newDr[PRICE_FIELD] = obj.LastPx;

                    MarketExecutionsItem lastExeItem = null;

                    string filter = $"{SECURITYID_FIELD} = '{obj.SecurityID}'";
                    DataTable dt = CTPriceControler.getDataTable(PrimaryKeyDV, filter);
                    
                    if (dt.Rows.Count > 0)
                    {
                        string exeId = dt.Rows[0][EXEID_FIELD] as string;
                        List<MarketExecutionsItem> filterResult = _myMarketExes.Where(o => { return o.Key == exeId; }).ToList();
                        if (filterResult != null && filterResult.Count > 0)
                            lastExeItem = filterResult[0];
                    }

                    this.PrimaryKeyDV.Table.Rows.Add(newDr);
                    newDr.AcceptChanges();

                    // 添加新数据
                    AddRecord(obj, lastExeItem);
                }
                else
                {
                    string transactionTime = CommonUtils.ConvertToString(findDr[TRANSACTIONTIME_FIELD]);
                    if (transactionTime.CompareTo(obj.TransactTime) > 0)
                    {
                        // 如果时间早， 就不更新了。
                        return;
                    }

                    // 如果状态是4， 删除这笔单子。
                    if (obj.OrdStatus == "4")
                    {
                        this.countDic[obj.SecurityID]--;

                        this.PrimaryKeyDV.Table.Rows.Remove(findDr);

                        // 删除
                        DeleteRecord(obj.ExecID);

                        return;
                    }

                    // 更新数据
                    UpdateRecord(obj);
                }
            }
        }

        private bool CheckSettle(string settle, List<string> filterSettleList)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(settle))
            {
                if (filterSettleList != null && filterSettleList.Count > 0)
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
            }
            else
                result = true;

            return result;
        }

        public int GetCountByCondition(string security, List<string> filterSettleList)
        {
            int result = 0;
            lock (lockObj)
            {
                List<MarketExecutionsItem> filterResult = _myMarketExes.Where(orderItem => orderItem.SecurityID == security && CheckSettle(orderItem.SettlType, filterSettleList)).ToList();

                if (filterResult.Count > 0)
                {
                    MarketExecutionsItem item = filterResult[0];
                    MarketCount market = JsonConvert.DeserializeObject<MarketCount>(item.Info1);
                    result = (int)market.TotalNumTrades;
                }
            }
            return result;
            
        }

        private void DeleteRecord(string execID)
        {
            MarketExecutionsItem marketExe = null;
            var result = _myMarketExes.Where(orderItem => orderItem.Key == execID);
            if (result != null)
            {
                foreach (MarketExecutionsItem item in result)
                {
                    marketExe = item;
                    break;
                }
            }
            if (marketExe != null)
            {
                _myMarketExes.Remove(marketExe);
            }
        }

        private void UpdateRecord(ExecutionReport obj)
        {
            MarketExecutionsItem marketExe = null;
            var result = _myMarketExes.Where(orderItem => orderItem.Key == obj.ExecID);
            if (result != null)
            {
                foreach (MarketExecutionsItem item in result)
                {
                    marketExe = item;
                    break;
                }
            }
            if (marketExe != null)
            {
                marketExe.Key = obj.ExecID;
                marketExe.Side = obj.Side; // 方向
                marketExe.SecurityID = obj.SecurityID; // 代码
                marketExe.Symbol = obj.Symbol;// 名称
                marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.LastPx); //用于显示成交价
                marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.LastPx); // 成交价
                marketExe.SourceTrader = obj.Venues; // 来源
                marketExe.Time = obj.TransactTime;// 时间
                marketExe.Info1 = obj.Info1;

                marketExe.IsValid = obj.Vs;
                
            }
        }

        private void AddRecord(ExecutionReport obj, MarketExecutionsItem lastExeItem)
        {
            MarketExecutionsItem marketExe = new MarketExecutionsItem();
            marketExe.Key = obj.ExecID;
            marketExe.Side = obj.Side; // 方向
            marketExe.SecurityID = obj.SecurityID; // 代码
            marketExe.Symbol = obj.Symbol;// 名称
            marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.LastPx); //用于显示成交价
            marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.LastPx); // 成交价
            marketExe.SourceTrader = obj.Venues; // 来源
            marketExe.Time = obj.TransactTime;// 时间
            marketExe.Info1 = obj.Info1;
            marketExe.ExecuteAmount = CommonUtils.ConvertToDecimal(obj.LastQty);
            marketExe.IsValid = obj.Vs;

            //if (obj.SecurityID == "200210.IB")
            {
                //是否是第一笔成交
                if (_myMarketExes.Any(o => (o.SecurityID == obj.SecurityID)))
                {
                    LastPriceParam lastPriceParam = LastExecutionProcesser.Processer.CalLastExe(marketExe, lastExeItem);
                    //LastExecutionProcesser.Processer.CalLastExe(obj.SecurityID, LastExecutionProcesser.Processer.ConvertSide(obj.Side), null);
                }
                else
                {
                    marketExe.DownUp = CommonConstants.NO_UP_DOWN;
                    marketExe.DownUpBrush = "0";
                    marketExe.DownUpSort = 0;
                    marketExe.IsFirst = true;
                }

                _myMarketExes.Add(marketExe);
                
            }
        }
        
    }
}
