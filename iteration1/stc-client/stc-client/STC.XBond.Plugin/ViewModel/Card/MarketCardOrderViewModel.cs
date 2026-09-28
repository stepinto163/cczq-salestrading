using com.gateway.log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using STC.Common.Model;
using STC.XBond.Plugin.Utils;
using STC.Common.Model.EMS;
using STC.Common.Utils;
using STC.Common.Model.Fix.Message;
using Stc.Common.Model.EMS;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.XBond.Plugin.ViewModel.Data;
using STC.Frame.Utils;
using STC.Common.ViewModel.Data;

namespace STC.XBond.Plugin.ViewModel.Card
{
    public class MarketCardOrderViewModel : BaseViewModel
    {
        private CollectionViewSource filteredMarketExecutions = new CollectionViewSource();
        public CollectionViewSource FilteredMarketExecutions
        {
            get
            {
                if (filteredMarketExecutions == null)
                    filteredMarketExecutions = new CollectionViewSource();

                return filteredMarketExecutions;

            }
            set { filteredMarketExecutions = value; }
        }

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
                this.OnPropertyChanged();

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
                _closeYield = value;
                this.OnPropertyChanged();

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
                this.OnPropertyChanged();

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
                this.OnPropertyChanged();

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
                this.OnPropertyChanged();

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
                //if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged();

                    if (this.FilteredMarketExecutions.View != null)
                        this.FilteredMarketExecutions.View.Refresh();
                }
            }
        }

        ~MarketCardOrderViewModel()
        {
            Dispose();
        }

        new public void Dispose()
        {
            ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (filteredMarketExecutions != null)
                {
                    this.filteredMarketExecutions.Filter -= FilteredMarketExecutions_Filter;
                    this.filteredMarketExecutions = null;
                }
            }));

            if (_ProcessMarketExecutioinsThreadQueue == null)
            {
                _ProcessMarketExecutioinsThreadQueue.Clear();
                _ProcessMarketExecutioinsThreadQueue = null;
            }

            base.Dispose();
        }

        private AsyncThreadQueue<STC.Common.Model.Fix.Message.ExecutionReport> _ProcessMarketExecutioinsThreadQueue;
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
                        this.OpenYld = this.LowYield = this.OrderCount = this.HighYield = this.CloseYield = CommonConstants.NO_PRICE;
                    }));
                }
            }
        }

        public void Init()
        {
            BaseHandler.Instance.Init(Instance_OnClear);

            try
            {
                ////BondChangeHandler.Instance.OnSelectBondChange += Instance_OnSelectBondChange;
                this.filteredMarketExecutions.IsLiveFilteringRequested = true;
                this.filteredMarketExecutions.IsLiveSortingRequested = true;
                ////this.filteredMarketExecutions.Source = MarketExecutionProcesser.Processer.MyMarketExes;
                this.filteredMarketExecutions.Filter += FilteredMarketExecutions_Filter;

                if (_ProcessMarketExecutioinsThreadQueue == null)
                {
                    _ProcessMarketExecutioinsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateMarketExecutions);
                }

                #region 收MarketExec消息
                ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                #endregion

                // 时间倒序
                SortDescription timeDesc = new SortDescription("Time", ListSortDirection.Descending);
                SortDescription downUpSort = new SortDescription("DownUpSort", ListSortDirection.Ascending);
                this.filteredMarketExecutions.SortDescriptions.Add(timeDesc);
                this.filteredMarketExecutions.SortDescriptions.Add(downUpSort);

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        ////public void Instance_OnSelectBondChange(BondUpdateType actionFrom, string groupId, string id, string security, string settle, bool needRefresh)
        ////{

        ////    if (actionFrom == BondUpdateType.ChangeFocusRow)
        ////    {
        ////        _lastUpdateTime = string.Empty;

        ////        this.OpenYld = this.LowYield = this.OrderCount = this.HighYield = this.CloseYield = CommonConstants.NO_PRICE;

        ////        //this.Init();

        ////        SearchText = security;
        ////    }
        ////}

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
        /// 更新数据， 好像没有删除数据？？还有很多字段不知道什么意思??key也不确定
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMarketExecutions(ExecutionReport obj)
        {
            lock (lockObj)
            {
                if (obj != null && !string.IsNullOrEmpty(obj.Info1))
                {
                    MarketCount market = JsonConvert.DeserializeObject<MarketCount>(obj.Info1);

                    if (!string.IsNullOrEmpty(SearchText) && obj.SecurityID.Equals(SearchText))
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (!string.IsNullOrEmpty(market.CloseYield))
                                this.CloseYield = market.CloseYield;
                            else
                                this.CloseYield = CommonConstants.NO_PRICE;

                            if (!string.IsNullOrEmpty(market.HighYield))
                                this.HighYield = market.HighYield;
                            else
                                this.HighYield = CommonConstants.NO_PRICE;

                            if (market.TotalNumTrades != 0)
                                this.OrderCount = market.TotalNumTrades.ToString();
                            else
                                this.OrderCount = CommonConstants.NO_PRICE;

                            if (!string.IsNullOrEmpty(market.LowYield))
                                this.LowYield = market.LowYield;
                            else
                                this.LowYield = CommonConstants.NO_PRICE;

                            if (!string.IsNullOrEmpty(market.OpenYld))
                                this.OpenYld = market.OpenYld;
                            else
                                this.OpenYld = CommonConstants.NO_PRICE;
                        }));
                    }
                }
            }
        }

        private void RefreshHOLC(string info1)
        {
            if (!string.IsNullOrEmpty(info1))
            {
                MarketCount market = JsonConvert.DeserializeObject<MarketCount>(info1);

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (!string.IsNullOrEmpty(market.CloseYield))
                        this.CloseYield = market.CloseYield;

                    if (!string.IsNullOrEmpty(market.HighYield))
                        this.HighYield = market.HighYield;

                    if (market.TotalNumTrades != 0)
                        this.OrderCount = market.TotalNumTrades.ToString();

                    if (!string.IsNullOrEmpty(market.LowYield))
                        this.LowYield = market.LowYield;

                    if (!string.IsNullOrEmpty(market.OpenYld))
                        this.OpenYld = market.OpenYld;
                }));

            }
        }

        private string _lastUpdateTime = string.Empty;

        private void FilteredMarketExecutions_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = this.MarketExecutionFilter(e.Item);

            if (e.Accepted)
            {
                var marketExecutionItem = e.Item as MarketExecutionsItem;
                //Console.WriteLine("-----------------info1:" + marketExecutionItem.Info1);
                if ((string.IsNullOrEmpty(_lastUpdateTime) || _lastUpdateTime.CompareTo(marketExecutionItem.Time) <= 0))
                {
                    RefreshHOLC(marketExecutionItem.Info1);
                    
                    _lastUpdateTime = marketExecutionItem.Time;
                }
            }
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
    }
}
