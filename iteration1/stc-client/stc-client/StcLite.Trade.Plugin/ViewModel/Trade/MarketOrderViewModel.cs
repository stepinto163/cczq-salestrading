using com.gateway.log;
using Newtonsoft.Json;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using BDB.Price.Plugin.Model;
using BDB.Price.Plugin.Chart.Deal;
using BDB.Price.Plugin.Chart;
using BDB.Trade.Plugin.View.Trade;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.Handler.MarketExec;
using STC.Common.Model.Fix.Message;


using STC.Common.Model.TBook;
using STC.Common.Model.MDHist;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Data;
using STC.Common.ViewModel.Data;
using ExecutionsHandler = BDB.Trade.Plugin.Handler.ExecutionsHandler;
using BDB.Common.Model.OHLC;
using BDB.Common.Handler.MDHist;
using BDB.Trade.Plugin.View.Trade.Card;
using System.Collections.Concurrent;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class MarketOrderViewModel : BaseViewModel
    {
        private FrameworkElement ownUI = null;
        private string key = string.Empty;
        private bool isInit = true;
        private object _lockObj = new object();
        private ScrollViewer currentScrollViewer = null;
        private bool hadRefresh = false;

        private CardShowType cardShowType = CardShowType.InCardPage;

        public CardShowType CardShowType
        {
            get { return cardShowType; }
            set
            {
                cardShowType = value;
            }
        }

        public void SetInitStatus(bool isInit)
        {
            this.isInit = isInit;
        }

        private bool isFuture = false;
        public bool IsFuture
        {
            get { return isFuture; }
            set
            {
                isFuture = value;

                MarketOrderModel.HeadVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                MarketOrderModel.ChatFloatButtonVisibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private MarketOrderModel marketOrderModel = new MarketOrderModel();
        public MarketOrderModel MarketOrderModel
        {
            get { return marketOrderModel; }
            set
            {
                marketOrderModel = value;

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



        private string _searchText = string.Empty;

        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                lock (_lockObj)
                {
                    string oldSecurity = _searchText;

                    //if (oldSecurity == value)
                    //{
                    //    return;
                    //}

                    _searchText = value;
                    MarketOrderModel.SecurityID = value;
                    if (ExecChart != null)
                    {
                        this.ExecChart.SecurityID = value;
                    }

                    this.OnPropertyChanged();

                    MarketOrderModel.MarketExecutions.Clear();
                    MarketOrderModel.MarketExecutionsDic.Clear();

                    if (!string.IsNullOrEmpty(oldSecurity))
                    {
                        if (oldSecurity != value)
                        {
                            OhlcHandler.Instance.UnSubscribeOhlc(oldSecurity, onOHLC);
                            PriceHandler.Instance.UnregisterCTPrice(oldSecurity, Instance_OnMDPrice);
                            OhlcHandler.Instance.UnSubscribeKline1M(oldSecurity, Sub_Instance_OnKLine);
                        }
                    }
                    else
                    {
                        #region 订阅OHLC
                        bool isFuture = CommonUtils.IsFutureBond(SearchText);
                        if (isFuture)
                        {
                            PriceHandler.Instance.RegisterCTPrice(this.SearchText, Instance_OnMDPrice);
                            OhlcHandler.Instance.SubscribeKline1M(this.SearchText, Sub_Instance_OnKLine);
                        }
                        else
                        {
                            OhlcHandler.Instance.SubscribeOhlc(this.SearchText, onOHLC);
                        }
                        #endregion
                    }

                    if (!string.IsNullOrEmpty(value))
                    {
                        if (oldSecurity != value)
                        {
                            LoadCacheData();

                            bool isFuture = CommonUtils.IsFutureBond(value);
                            if (isFuture)
                            {
                                LoadKlineData();
                            }
                        }

                        decimal high = MarketOrderModel.HighYield != CommonConstants.NO_PRICE ? MarketOrderModel.HighYield.ConvertToDecimal() : 10M;
                        decimal low = MarketOrderModel.LowYield != CommonConstants.NO_PRICE ? MarketOrderModel.LowYield.ConvertToDecimal() : 0M;
                        this.ExecChart?.SetDataMinMax(high, low);
                    }
                    else
                    {
                        this.ExecChart?.ClearData();
                    }
                    //if (this.FilteredMarketExecutions.View != null)
                    //    this.FilteredMarketExecutions.View.Refresh();

                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    if (!string.IsNullOrEmpty(value))
                    {
                        MarketOrderModel.OhlcVisibility = Visibility.Visible;

                        //BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(value);
                        bool isFuture = CommonUtils.IsFutureBond(value);
                        if (isFuture)//(bondsInfoPo != null && bondsInfoPo.MarketIndicator == "5")
                        {
                            MarketOrderModel.HeadVisibility = Visibility.Visible;
                            MarketOrderModel.ChatFloatButtonVisibility = Visibility.Collapsed;
                        }
                        else
                        {
                            if (value.StartsWith("T") && !value.Contains("."))//国债期货
                            {
                                MarketOrderModel.HeadVisibility = Visibility.Visible;
                                MarketOrderModel.ChatFloatButtonVisibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MarketOrderModel.HeadVisibility = Visibility.Collapsed;
                                MarketOrderModel.ChatFloatButtonVisibility = Visibility.Visible;
                            }
                        }
                    }
                    else
                    {
                        MarketOrderModel.HeadVisibility = Visibility.Collapsed;
                        MarketOrderModel.OhlcVisibility = Visibility.Collapsed;
                    }
                    //}), System.Windows.Threading.DispatcherPriority.Normal);
                }

            }
        }

        

        private double rowHeight = 24;
        public double RowHeight
        {
            get { return rowHeight; }
            set
            {
                rowHeight = value;

                this.OnPropertyChanged();
            }
        }
        

        /// <summary>
        /// 1:列表模式
        /// 2:卡片模式
        /// </summary>
        private TabShowModel _showModel = TabShowModel.Table;

        public TabShowModel ShowModel { get { return _showModel; } set { _showModel = value; } }


        #region 图形部分

        private ExecChart _execChart;

        public ExecChart ExecChart
        {
            get
            {
                if (_execChart == null)
                {
                    if (ownUI != null && ownUI is MarketOrderView)
                    {
                        MarketOrderView mov = ownUI as MarketOrderView;
                        _execChart = mov.execChartView;//ownUI.FindName("execChartView") as ExecChart;
                    }
                }
                return _execChart;
            }
            set
            {
                _execChart = value;
            }
        }
        #endregion


        public MarketOrderViewModel()
        {
            

        }

        ~MarketOrderViewModel()
        {
            Dispose();
        }

        new public void Dispose()
        {
            ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;

            if (_showModel == TabShowModel.Table)
                BondChangeHandler.Instance.OnSelectBondChange -= Instance_OnSelectBondChange;


            while (queue.TryDequeue(out proto))
            {
                this.isStop = true;
            }

            base.Dispose();
        }


        private object lockObj = new object();

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("MarketOrderViewModel Clear, security:{0}", this.SearchText));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                lock (this.lockObj)
                {
                    _lastUpdateTime = string.Empty;

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MarketOrderModel.OpenYld = CommonConstants.NO_PRICE;
                        MarketOrderModel.LowYield = CommonConstants.NO_PRICE;
                        MarketOrderModel.OrderCount = CommonConstants.NO_PRICE;
                        MarketOrderModel.HighYield = CommonConstants.NO_PRICE;
                        MarketOrderModel.CloseYield = CommonConstants.NO_PRICE;

                        MarketOrderModel.MarketExecutions.Clear();
                        MarketOrderModel.MarketExecutionsDic.Clear();

                        this.ExecChart?.InitDateAxis();

                        this.ExecChart?.ClearData();

                        //Thread.Sleep(2000);
                        if (this.ExecChart != null)
                            this.ExecChart.SecurityID = this.SearchText;

                        LoadCacheData();

                        bool isFuture = CommonUtils.IsFutureBond(SearchText);
                        if (isFuture)
                        {
                            LoadKlineData();
                        }
                    }));
                }
            }

            while (queue.TryDequeue(out proto))
            {

            }
        }


        public void LoadCacheData()
        {
            //Task.Factory.StartNew(() =>
            //{
            //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //    {
                    MarketOrderModel.MarketExecutions.Clear();
                    MarketOrderModel.MarketExecutionsDic.Clear();

                    bool isLoad = MarketExecCache.Instance.GetLoadStatus(SearchText);

                    if (isLoad)
                    {
                        List<ExecutionReport> execList = MarketExecCache.Instance.GetMarketExecByCondition(SearchText, string.Empty);
                        LogHelper.Warn("LoadCacheData SecurityId:" + SearchText + "," + isLoad + "," + (execList != null ? execList.Count : 0));
                        if (execList != null && execList.Count > 0)
                        {

                            foreach (ExecutionReport order in execList)
                            {
                                this.queue.Enqueue(order);
                                semaphore.Release(); // 释放信号量，通知处理线程有新数据到来
                            }

                        }
                        else
                        {
                            RequestMarketExecutions();
                        }
                    }
                    else
                    {
                        RequestMarketExecutions();
                    }
            //    }));
            //});
        }

        public void LoadKlineData()
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    bool isLoad = Kline1MCache.Instance.GetLoadStatus(SearchText);
                    if (isLoad)
                    {
                        SortedList<string, OhlcMsg> klineList = Kline1MCache.Instance.GetKLine_1M(SearchText);
                        //LogHelper.Warn("LoadKlineData SecurityId:" + SearchText + "," + isLoad + "," + (klineList != null ? klineList.Count : 0));
                        //Instance_OnMarketExeList(execList);
                        if (klineList != null && klineList.Count > 0)
                        {
                            foreach (OhlcMsg order in klineList.Values)
                            {
                                this.Sub_Instance_OnKLine(order);
                            }
                        }
                        else
                        {
                            //RequestKline();
                        }
                    }
                    else
                    {
                        RequestKline();
                    }
                }));
            });
        }



        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private ConcurrentQueue<ExecutionReport> queue = new ConcurrentQueue<ExecutionReport>();
        private SemaphoreSlim semaphore = new SemaphoreSlim(0, int.MaxValue); // 计数信号量
        ExecutionReport proto;
        private bool isStop = false;

        public void Init(FrameworkElement ownUI)
        {
            BaseHandler.Instance.Init(Instance_OnClear);

            if (this.ownUI == null)
            {
                this.ownUI = ownUI;
                key = PageCardHandler.GetWindowHashKey(ownUI);
            }

            try
            {
                if (_showModel == TabShowModel.Table)
                    BondChangeHandler.Instance.OnSelectBondChange += Instance_OnSelectBondChange;

                MarketOrderModel.Init();

                isStop = false;
                //更新深度行情
                Task.Factory.StartNew(() => dealMsg(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

                #region 收MarketExec消息
                ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                #endregion

                //LoadCacheData();

                #region 订阅OHLC
                bool isFuture = CommonUtils.IsFutureBond(SearchText);
                if (isFuture)
                {
                    PriceHandler.Instance.RegisterCTPrice(this.SearchText, Instance_OnMDPrice);
                    OhlcHandler.Instance.SubscribeKline1M(this.SearchText, Sub_Instance_OnKLine);
                }
                else
                {
                    OhlcHandler.Instance.SubscribeOhlc(this.SearchText, onOHLC);
                }
                #endregion

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void Instance_OnMDPrice(MarketDataSnapshotFullRefresh data)
        {
            //CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            lock (this.lockObj)
            {
                if (data != null && !string.IsNullOrEmpty(data.SecurityID) && data.SecurityID.ToLower() == SearchText.ToLower())
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (!string.IsNullOrEmpty(data.SecurityID) && data.SecurityID.ToLower() == SearchText.ToLower())
                        {
                            bool isFuture = CommonUtils.IsFutureBond(data.SecurityID);
                            if (isFuture)
                            {
                                double number = 0;
                                if (double.TryParse(data.PreClosePrice, out number))
                                    MarketOrderModel.CloseYield = string.Format("{0:0.000}", number);
                                if (double.TryParse(data.HighPrice, out number))
                                    MarketOrderModel.HighYield = string.Format("{0:0.000}", number);
                                //number = double.Parse(data.Volume);
                                MarketOrderModel.OrderCount = data.Volume;// string.Format("{0:0.000}", number);
                                if (double.TryParse(data.LowPrice, out number))
                                    MarketOrderModel.LowYield = string.Format("{0:0.000}", number);
                                if (double.TryParse(data.OpenPrice, out number))
                                    MarketOrderModel.OpenYld = string.Format("{0:0.000}", number);

                                decimal high = MarketOrderModel.HighYield != CommonConstants.NO_PRICE ? MarketOrderModel.HighYield.ConvertToDecimal() : 10M;
                                decimal low = MarketOrderModel.LowYield != CommonConstants.NO_PRICE ? MarketOrderModel.LowYield.ConvertToDecimal() : 0M;
                                this.ExecChart?.SetDataMinMax(high, low);
                            }
                        }
                    }));
                }
                else if (string.IsNullOrEmpty(data.SecurityID))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MarketOrderModel.OpenYld = CommonConstants.NO_PRICE;
                        MarketOrderModel.LowYield = CommonConstants.NO_PRICE;
                        MarketOrderModel.OrderCount = CommonConstants.NO_PRICE;
                        MarketOrderModel.HighYield = CommonConstants.NO_PRICE;
                        MarketOrderModel.CloseYield = CommonConstants.NO_PRICE;
                    }));
                }
            }
        }

        public void onOHLC(OhlcMsg ohlcMsg)
        {
            //CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            lock (this.lockObj)
            {
                if (!string.IsNullOrEmpty(ohlcMsg.securityid) && ohlcMsg.securityid.ToLower() == SearchText.ToLower())
                {
                    bool isFuture = CommonUtils.IsFutureBond(ohlcMsg.securityid);
                    if (!isFuture)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (!string.IsNullOrEmpty(ohlcMsg.securityid) && ohlcMsg.securityid.ToLower() == SearchText.ToLower())
                            {
                                isFuture = CommonUtils.IsFutureBond(ohlcMsg.securityid);
                                if (!isFuture)
                                {
                                    string closePrice = string.IsNullOrEmpty(ohlcMsg.inf2) ? CommonConstants.NO_PRICE : ohlcMsg.inf2;

                                    if (CommonUtils.IsNumeric(closePrice))
                                    {
                                        MarketOrderModel.CloseYield = CommonUtils.RemoveZero(closePrice);
                                    }
                                    else
                                    {
                                        MarketOrderModel.CloseYield = closePrice;
                                    }

                                    if (ohlcMsg.high != 0)
                                        MarketOrderModel.HighYield = ohlcMsg.high.ToString();

                                    MarketOrderModel.OrderCount = string.IsNullOrEmpty(ohlcMsg.inf1) ? CommonConstants.NO_PRICE : ohlcMsg.inf1;

                                    if (ohlcMsg.low != 0)
                                        MarketOrderModel.LowYield = ohlcMsg.low.ToString();

                                    if (ohlcMsg.open != 0)
                                        MarketOrderModel.OpenYld = ohlcMsg.open.ToString();

                                    this.ExecChart?.SetDataMinMax(ohlcMsg.high, ohlcMsg.low);
                                }
                            }
                        }));
                    }
                }
                else if (string.IsNullOrEmpty(SearchText))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MarketOrderModel.OpenYld = CommonConstants.NO_PRICE;
                        MarketOrderModel.LowYield = CommonConstants.NO_PRICE;
                        MarketOrderModel.OrderCount = CommonConstants.NO_PRICE;
                        MarketOrderModel.HighYield = CommonConstants.NO_PRICE;
                        MarketOrderModel.CloseYield = CommonConstants.NO_PRICE;
                    }));
                }
            }
        }

        private void ScrollViewToTop()
        {
            if (ownUI != null)
            {
                var scrollViewer = this.GetDescendantByType(ownUI.FindName("dgMarket") as Visual, typeof(ScrollViewer)) as ScrollViewer;
                scrollViewer?.ScrollToTop();
            }
        }

        public void ClearData(bool isShowLabel)
        {
            //Application.Current.Dispatcher.Invoke((Action)(() =>
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
                MarketOrderModel.OpenYld = CommonConstants.NO_PRICE;
                MarketOrderModel.LowYield = CommonConstants.NO_PRICE;
                MarketOrderModel.OrderCount = CommonConstants.NO_PRICE;
                MarketOrderModel.HighYield = CommonConstants.NO_PRICE;
                MarketOrderModel.CloseYield = CommonConstants.NO_PRICE;

                ScrollViewToTop();

                if (!isShowLabel)
                {
                    MarketOrderModel.OhlcVisibility = Visibility.Collapsed;

                    this.SearchText = string.Empty;

                    MarketOrderModel.HeadVisibility = Visibility.Collapsed;
                    MarketOrderModel.ChatFloatButtonVisibility = Visibility.Visible;


                }
                else
                {
                    MarketOrderModel.OhlcVisibility = Visibility.Visible;

                    MarketOrderModel.HeadVisibility = Visibility.Visible;

                    MarketOrderModel.ChatFloatButtonVisibility = Visibility.Collapsed;
                }


            while (queue.TryDequeue(out proto))
            {

            }
            //}));
        }


        public void Instance_OnSelectBondChange(BondUpdateType actionFrom, string groupId, string id, string security, string settle, bool needRefresh)
        {
            if (_showModel == TabShowModel.Table)
            {
                if (actionFrom == BondUpdateType.ChangeFocusRow)
                {
                    _lastUpdateTime = string.Empty;

                    MarketOrderModel.OpenYld = CommonConstants.NO_PRICE;
                    MarketOrderModel.LowYield = CommonConstants.NO_PRICE;
                    MarketOrderModel.OrderCount = CommonConstants.NO_PRICE;
                    MarketOrderModel.HighYield = CommonConstants.NO_PRICE;
                    MarketOrderModel.CloseYield = CommonConstants.NO_PRICE;
                    //this.Init();

                    SearchText = security;

                    #region 订阅OHLC
                    bool isFuture = CommonUtils.IsFutureBond(SearchText);
                    if (isFuture)
                    {
                        PriceHandler.Instance.RegisterCTPrice(this.SearchText, Instance_OnMDPrice);
                        OhlcHandler.Instance.SubscribeKline1M(this.SearchText, Sub_Instance_OnKLine);
                    }
                    else
                    {
                        OhlcHandler.Instance.SubscribeOhlc(this.SearchText, onOHLC);
                    }
                    #endregion
                }
            }
            else
            {
                BondChangeHandler.Instance.OnSelectBondChange -= Instance_OnSelectBondChange;
            }
        }

        private void UpdateFutureExec(ExecutionReport executionReport, MarketExecutionsItem marketExe)
        {
            if (executionReport.Venues.Equals("CTP"))//国债期货
            {
                Dictionary<string, string> infoDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(executionReport.Info1);

                if (infoDic.ContainsKey("natureKey"))
                {
                    marketExe.Side = infoDic["natureKey"];
                }
                else
                {
                    marketExe.Side = string.Empty;
                }

                marketExe.SourceTrader = string.Empty;
                //增仓
                if (infoDic.ContainsKey("openInterest"))
                {
                    marketExe.OpenInterest = infoDic["openInterest"];
                }
                else
                {
                    marketExe.OpenInterest = string.Empty;
                }

                double number = double.Parse(marketExe.ExecutePricePercent_Display);

                marketExe.ExecutePricePercent_Display = string.Format("{0:0.000}", number);

                //现手
                marketExe.HandNow = executionReport.LastQty;

                marketExe.IsValid = "Y";

                List<MarketDataSnapshotFullRefresh> marketDataList = PriceHandler.Instance.GetAllMarketData(executionReport.SecurityID);
                if (marketDataList != null && marketDataList.Count > 0)
                {
                    MarketDataSnapshotFullRefresh marketData = marketDataList[0];
                    string preSettlePrice = marketData.PreSettlePrice;
                    decimal tem = 0;
                    if (decimal.TryParse(preSettlePrice, out tem))
                    {
                        if (marketExe.ExecutePricePercent > tem)
                        {
                            marketExe.PriceBrushFlag = "1";
                        }
                        else if (marketExe.ExecutePricePercent < tem)
                        {
                            marketExe.PriceBrushFlag = "2";
                        }
                        else if (marketExe.ExecutePricePercent > tem)
                        {
                            marketExe.PriceBrushFlag = "3";
                        }
                    }
                }
            }
        }

        private void Instance_OnMarketExecutions(ExecutionReport obj)
        {
            //Sub_Instance_OnMarketExecutions(obj);
            this.queue.Enqueue(obj);
            semaphore.Release(); // 释放信号量，通知处理线程有新数据到来
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
                        Sub_Instance_OnMarketExecutions(proto);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }

        }

        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Sub_Instance_OnMarketExecutions(ExecutionReport obj)
        {
            //CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            lock (_lockObj)
            {
                if (!string.IsNullOrEmpty(SearchText) && obj.SecurityID.ToLower() == SearchText.ToLower())
                {

                    bool isFuture = CommonUtils.IsFutureBond(obj.SecurityID);
                    if (!isFuture)
                    {
                        // 图形部分
                        decimal high = MarketOrderModel.HighYield != CommonConstants.NO_PRICE ? MarketOrderModel.HighYield.ConvertToDecimal() : 10M;
                        decimal low = MarketOrderModel.LowYield != CommonConstants.NO_PRICE ? MarketOrderModel.LowYield.ConvertToDecimal() : 0M;
                        this.ExecChart?.Update(obj);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (!string.IsNullOrEmpty(SearchText) && obj.SecurityID.ToLower() == SearchText.ToLower())
                        {
                            if (MarketOrderModel.MarketExecutionsDic.ContainsKey(obj.ExecID))
                            {
                                MarketExecutionsItem selectItem = MarketOrderModel.MarketExecutionsDic[obj.ExecID];
                                if (obj.OrdStatus == "4")
                                {
                                    MarketOrderModel.MarketExecutions.Remove(selectItem);
                                    //LogHelper.Warn("Remove invalid order, ordStatus:" + obj.OrdStatus + ", execId:" + obj.ExecID);
                                    MarketOrderModel.MarketExecutionsDic.TryRemove(obj.ExecID, out selectItem);
                                }
                                else
                                {
                                    // 如果是较新的， 才更新数据
                                    if (obj.OrderID.CompareTo(selectItem.OrderID) > 0) 
                                    {
                                        selectItem.Side = obj.Side; // 方向
                                        selectItem.SecurityID = obj.SecurityID; // 代码
                                        selectItem.Symbol = obj.Symbol;// 名称
                                        selectItem.ExecutePricePercent_Display = CommonUtils.FormatNum(obj.LastPx); //用于显示成交价
                                        selectItem.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.LastPx); // 成交价
                                        selectItem.SourceTrader = obj.Venues; // 来源
                                        selectItem.Time = obj.TransactTime;// 时间
                                        selectItem.Info1 = obj.Info1;
                                        selectItem.OrderID = obj.OrderID;

                                        if (string.IsNullOrEmpty(obj.Vs))
                                        {
                                            selectItem.IsValid = "Y";
                                        }
                                        else
                                        {
                                            selectItem.IsValid = obj.Vs;
                                        }

                                        if (obj.OrdStatus == "4")
                                        {
                                            selectItem.IsNothingDown = "1";
                                        }
                                        else
                                        {
                                            selectItem.IsNothingDown = string.Empty;
                                        }

                                        if (isFuture)
                                        {
                                            selectItem.RowHeight = 32;
                                            selectItem.ShowFontSize = 12;
                                        }
                                        else
                                        {
                                            selectItem.RowHeight = 24;
                                            selectItem.ShowFontSize = 8;
                                        }

                                        UpdateFutureExec(obj, selectItem);
                                    }
                                }
                            }
                            else
                            {
                                if (obj.OrdStatus == "4")
                                {
                                    LogHelper.Warn("Filter invalid order, execId:" + obj.ExecID + ", orderStatus:" + obj.OrdStatus);
                                    return;
                                }

                                MarketExecutionsItem marketExe = new MarketExecutionsItem();
                                marketExe.Side = obj.Side; // 方向
                                marketExe.SecurityID = obj.SecurityID; // 代码
                                marketExe.Symbol = obj.Symbol;// 名称
                                marketExe.ExecutePricePercent_Display = CommonUtils.FormatNum(obj.LastPx); //用于显示成交价
                                marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.LastPx); // 成交价
                                marketExe.SourceTrader = obj.Venues; // 来源
                                marketExe.Time = obj.TransactTime;// 时间
                                marketExe.Info1 = obj.Info1;
                                marketExe.Key = obj.ExecID;
                                marketExe.OrderID = obj.OrderID;

                                if (obj.OrdStatus == "4")
                                {
                                    marketExe.IsNothingDown = "1";
                                }
                                else
                                {
                                    marketExe.IsNothingDown = string.Empty;
                                }

                                if (string.IsNullOrEmpty(obj.Vs))
                                {
                                    marketExe.IsValid = "Y";
                                }
                                else
                                {
                                    marketExe.IsValid = obj.Vs;
                                }

                                if (isFuture)
                                {
                                    marketExe.RowHeight = 32;
                                    marketExe.ShowFontSize = 12;

                                    UpdateFutureExec(obj, marketExe);
                                }
                                else
                                {
                                    marketExe.RowHeight = 24;
                                    marketExe.ShowFontSize = 8;

                                    marketExe.DownUp = CommonConstants.NO_UP_DOWN;
                                    marketExe.DownUpBrush = "0";

                                    IEnumerable<object> viewList = MarketOrderModel.FilteredMarketExecutions.View.Cast<object>();
                                    var firstItem = viewList.LastOrDefault();
                                    if (firstItem != null)
                                    {
                                        MarketExecutionsItem calItem = firstItem as MarketExecutionsItem;
                                        if (calItem.Time.CompareTo(marketExe.Time) >= 0)
                                        {
                                            LastPriceParam lastPriceParam = LastExecutionProcesser.Processer.CalLastExe(calItem, marketExe);
                                        }
                                        else
                                        {
                                            firstItem = viewList.FirstOrDefault();
                                            LastExecutionProcesser.Processer.CalLastExe(marketExe, firstItem as MarketExecutionsItem);
                                        }
                                    }
                                    else
                                    {
                                        marketExe.IsFirst = true;
                                        marketExe.DownUp = CommonConstants.NO_UP_DOWN;
                                        marketExe.DownUpBrush = "0";
                                    }
                                }



                                MarketOrderModel.MarketExecutions.Add(marketExe);
                                MarketOrderModel.MarketExecutionsDic.TryAdd(obj.ExecID, marketExe);

                                if (MarketOrderModel.MarketExecutions.Count > MarketExecCache.Instance.Limit && isFuture)
                                {
                                    IOrderedEnumerable<MarketExecutionsItem> sortList = MarketOrderModel.MarketExecutions.OrderBy(o => o.Time).ThenBy(o => o.OrderID);
                                    MarketExecutionsItem firstItem = sortList.First();
                                    MarketOrderModel.MarketExecutions.Remove(firstItem);
                                    MarketOrderModel.MarketExecutionsDic.TryRemove(obj.ExecID, out firstItem);
                                }
                            }

                            if (marketOrderModel.TipVisibility == Visibility.Visible)
                            {
                                hadRefresh = true;

                                OnExecOrder();
                            }

                        }
                    }));
                }
                else if (string.IsNullOrEmpty(SearchText))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MarketOrderModel.MarketExecutions.Clear();
                        MarketOrderModel.MarketExecutionsDic.Clear();
                    }));
                }
            }

        }



        private void Sub_Instance_OnKLine(OhlcMsg obj)
        {
            //CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            lock (_lockObj)
            {
                if (!string.IsNullOrEmpty(SearchText) && obj.securityid.ToLower() == SearchText.ToLower())
                {
                    bool isFuture = CommonUtils.IsFutureBond(obj.securityid);
                    if (isFuture)
                    {
                        // 图形部分
                        decimal high = MarketOrderModel.HighYield != CommonConstants.NO_PRICE ? MarketOrderModel.HighYield.ConvertToDecimal() : 10M;
                        decimal low = MarketOrderModel.LowYield != CommonConstants.NO_PRICE ? MarketOrderModel.LowYield.ConvertToDecimal() : 0M;
                        //this.ExecChart?.BindData(obj, high, low);
                        //this.ExecChart?.SetDataMinMax(high, low);
                        this.ExecChart?.Update(obj);
                    }
                }
            }
        }

        private string _lastUpdateTime = string.Empty;

        private void FilteredMarketExecutions_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = this.MarketExecutionFilter(e.Item);

        }

        private bool MarketExecutionFilter(object item)
        {
            if (string.IsNullOrEmpty(this.SearchText) == true)
            {
                return false;
            }

            var marketExecutionItem = item as MarketExecutionsItem;
            return marketExecutionItem != null && marketExecutionItem.SecurityID != null &&
                marketExecutionItem.SecurityID.ToLower().Equals(this.SearchText.ToString().ToLower());
        }



        #region 各个card自己加载自己的数据

        private bool isQueryKline = false;

        public void RequestKline()
        {
            if (isQueryKline || string.IsNullOrEmpty(SearchText))
            {
                return;
            }

            isQueryKline = true;

            //LogHelper.Warn("RequestKline Load SecurityId:" + SearchText);

            ThreadPoolHelper.InvokeThreadPool((o) => {
                try
                {
                    // -----K线数据-----
                    string securityid = o.ToString();
                    bool isFuture = CommonUtils.IsFutureBond(securityid);
                    string yyyyMMdd = "yyyy-MM-dd";
                    string today = DateTimeEx.ServerTime.ToString(yyyyMMdd);

                    if (isFuture)
                    {
                        KLineSearchParam kLineSearchParam = new KLineSearchParam();
                        kLineSearchParam.SecurityID = securityid;
                        kLineSearchParam.Text = "1M";
                        kLineSearchParam.Type = "e";
                        kLineSearchParam.Field = "tradeDate,venue,securityID,text,close,closeTime,createTime,volume";
                        kLineSearchParam.QueryType = "1";
                        kLineSearchParam.Start = today + " 00:00:00";
                        kLineSearchParam.End = today + " 23:59:59";
                        TKLineReply result = MDHistHandler.Instance.SearchKLine(kLineSearchParam);
                        if (result != null && result.IsSuccess())
                        {
                            if (!string.IsNullOrEmpty(result.Data))
                            {
                                Kline1MCache.Instance.AddOrUpdateStatus(securityid);

                                PublishLastOrder_Kline1M(result);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestKline", e);
                }
                finally
                {
                    isQueryKline = false;
                }
            }, SearchText);
        }

        private bool isSend = false;

        public void RequestMarketExecutions()
        {
            if (isSend || string.IsNullOrEmpty(SearchText))
            {
                return;
            }

            isSend = true;

            LogHelper.Warn("RequestMarketExecutions Load SecurityId:" + SearchText);
            //LogHelper.Warn("RequestMarketExecutions Load SecurityId1:" + SearchText + "线程池是否饱和:" + ThreadPoolHelper.IsThreadPoolSaturated());

            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    LogHelper.Info("RequestMarketExecutions Load SecurityId2:" + SearchText);
                    string securityid = o.ToString();
                    bool isFuture = CommonUtils.IsFutureBond(securityid);
                    string yyyyMMdd = "yyyy-MM-dd";
                    //string commonFormat = "yyyy-MM-dd HH:mm:ss";
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString(yyyyMMdd);

                    //DateTime openDate = DateTime.ParseExact(DateTime.Now.ToString(yyyyMMdd) + " 09:30:00", commonFormat, CultureInfo.InvariantCulture);

                    //if (isFuture && DateTime.Now.CompareTo(openDate) < 0)
                    //{
                    //    today = DateTimeEx.ServerTime.AddDays(-1).ToString(yyyyMMdd);

                    //    request.pageSize = 5;
                    //}
                    //else
                    {
                        if (isFuture)
                        {
                            request.pageSize = MarketExecCache.Instance.Limit;
                        }
                        else
                        {
                            request.pageSize = 1000;
                        }
                    }

                    request.dealTimeBegin = today;
                    request.dealTimeEnd = today;
                    request.venuetypegw = isFuture ? STC.Common.Api.ApiConsts.SDN : STC.Common.Api.ApiConsts.CDH;
                    //request.venuetypegw = "CDH";
                    request.securityid = securityid;

                    List<string> permissionBroker = PermisionHandler.Instance.GetBondPermission();

                    foreach (string broker in permissionBroker)
                    {
                        request.venues += string.Format("{0},", broker);
                    }

                    if (!string.IsNullOrEmpty(request.venues))
                    {
                        request.venues += "CTP,";
                        request.venues = request.venues.Substring(0, request.venues.Length - 1);
                    }
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    request.orderStatus = "1,2";
                    int index = 0;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取成交
                        //LogHelper.Warn(string.Format("MarketOrderViewModel QueryExecOrderForEMS request:{0}", JsonConvert.SerializeObject(request)));
                        var reply = MarketExecHandler.Instance.QueryExecOrderForEMS(request);
                        //LogHelper.Warn(string.Format("MarketOrderViewModel QueryExecOrderForEMS replay:{0}", reply == null ? string.Empty : reply.totalSize.ToString()));
                        if (reply == null || reply.totalSize == 0 || reply.datas == null)
                        {
                            flag = false;
                        }
                        else if ((reply != null && reply.datas != null) || isFuture)
                        {
                            // 取较新成交数据，刷新下最新成交
                            if (index == 0)
                            {
                                HashSet<string> sendExecIdHs = new HashSet<string>();
                                foreach (ExecutionReport er in reply.datas)
                                {

                                    if (sendExecIdHs.Count < 5)
                                    {
                                        ExecutionsHandler.Instance.Instance_OnExecOrder(er);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    if (er.OrdStatus == "2")
                                    {
                                        sendExecIdHs.Add(er.ExecID);
                                    }
                                }
                            }

                            if (reply.datas.Count < request.pageSize || isFuture)
                            {
                                flag = false;

                                MarketExecCache.Instance.AddOrUpdateStatus(securityid);

                                PublishLastOrder(reply);
                            }
                            else
                            {
                                index++;

                                Thread.Sleep(100);
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestExecutions", e);
                }
                finally
                {
                    //MarketExecHandler.Instance.SetLoadDataStatus(true);

                    isSend = false;
                }
            }, SearchText);
        }

        #endregion

        public void PublishLastOrder(TBookOrder bookOrder)
        {
            if (bookOrder != null && bookOrder.datas != null)
            {
                LogHelper.Warn("PublishLastOrder Load SecurityId:" + SearchText + "," + CommonHandler.Instance.IsLoaded(key));
                if ((CommonHandler.Instance.IsLoaded(key) || CardShowType == CardShowType.AloneWindow) && bookOrder.datas.Count > 0)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        LoadCacheData();
                    }));
                }

                HashSet<string> sendExecIdHs = new HashSet<string>();
                foreach (ExecutionReport er in bookOrder.datas)
                {
                    
                    if (sendExecIdHs.Count < 5)
                    {
                        ExecutionsHandler.Instance.Instance_OnExecOrder(er);
                    }
                    else
                    {
                        break;
                    }

                    if (er.OrdStatus == "2")
                    {
                        sendExecIdHs.Add(er.ExecID);
                    }
                }
            }
        }

        public void PublishLastOrder_Kline1M(TKLineReply klineReply)
        {
            if (klineReply != null && !string.IsNullOrEmpty(klineReply.Data))
            {
                //LogHelper.Warn("PublishLastOrder_Kline1M Load SecurityId:" + SearchText + "," + CommonHandler.Instance.IsLoaded());
                if ((CommonHandler.Instance.IsLoaded(key) || CardShowType == CardShowType.AloneWindow) && klineReply.Data.Length > 0)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        LoadKlineData();
                    }));
                }

                //HashSet<string> sendExecIdHs = new HashSet<string>();
                //foreach (ExecutionReport er in bookOrder.datas)
                //{

                //    if (sendExecIdHs.Count < 5)
                //    {
                //        ExecutionsHandler.Instance.Instance_OnExecOrder(er);
                //    }
                //    else
                //    {
                //        break;
                //    }
                //    if (er.OrdStatus == "2")
                //    {
                //        sendExecIdHs.Add(er.ExecID);
                //    }

                //}
            }
        }


        #region Tip刷新条

        public void HideTip()
        {
            if (marketOrderModel.TipVisibility == Visibility.Visible)
            {
                marketOrderModel.TipVisibility = Visibility.Collapsed;

                lock (lockObj)
                {
                    hadRefresh = false;
                }
            }
        }

        public void RefreshTip()
        {
            if (ownUI != null)
            {
                lock (lockObj)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (marketOrderModel.TipVisibility != Visibility.Visible)
                        {
                            marketOrderModel.TipVisibility = Visibility.Visible;
                        }

                        if (hadRefresh)
                        {
                            marketOrderModel.TipTxt = CommonConstants.RefreshTxt;
                            marketOrderModel.TipTxtBrush = ownUI.FindResource("Yellow-2") as Brush;
                            marketOrderModel.RefreshBg = ownUI.FindResource("Brown-2-50%") as Brush;
                            marketOrderModel.HoverTipTxtBrush = ownUI.FindResource("Yellow-3") as Brush;
                            marketOrderModel.HoverRefreshBg = ownUI.FindResource("Brown-1-50%") as Brush;
                        }
                        else
                        {
                            marketOrderModel.TipTxt = CommonConstants.ToTop;
                            marketOrderModel.TipTxtBrush = ownUI.FindResource("Yellow-2") as Brush;
                            marketOrderModel.RefreshBg = ownUI.FindResource("Green-6-50%") as Brush;
                            marketOrderModel.HoverTipTxtBrush = ownUI.FindResource("Green-5") as Brush;
                            marketOrderModel.HoverRefreshBg = ownUI.FindResource("Green-4-50%") as Brush;
                        }
                    }));
                }
            }
        }


        private void OnExecOrder()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (currentScrollViewer == null)
                {
                    currentScrollViewer = GetDescendantByType(ownUI.FindName("dgMarket") as Visual, typeof(ScrollViewer)) as ScrollViewer;
                }
                
                if (currentScrollViewer != null && currentScrollViewer.VerticalOffset != 0)
                {
                    RefreshTip();
                }
            }));
        }

        public Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null)
                return null;
            if (element.GetType() == type)
                return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
                (element as FrameworkElement).ApplyTemplate();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }


        #endregion

    }

    public class MarketItemComparer : IComparer<MarketExecutionsItem>
    {
        public int Compare(MarketExecutionsItem x, MarketExecutionsItem y)
        {
            if (x == null || y == null)
                return 0;
            else
                return x.Time.CompareTo(y.Time);
        }
    }
}
