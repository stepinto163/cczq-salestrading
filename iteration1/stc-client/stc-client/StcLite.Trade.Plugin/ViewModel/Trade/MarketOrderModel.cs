using BDB.Trade.Plugin.ViewModel.Data;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class MarketOrderModel : BaseViewModel
    {
        private string _SecurityID;

        public string SecurityID
        {
            get { return _SecurityID; }
            set 
            { 
                _SecurityID = value;
                this.OnPropertyChanged();
            }
        }

        //为加快搜索速度
        private ConcurrentDictionary<string, MarketExecutionsItem> _marketExecutionsDic = new ConcurrentDictionary<string, MarketExecutionsItem>();
        public ConcurrentDictionary<string, MarketExecutionsItem> MarketExecutionsDic
        {
            get { return _marketExecutionsDic; }
        }

        private ObservableCollection<MarketExecutionsItem> marketExecutions = new ObservableCollection<MarketExecutionsItem>();

        public ObservableCollection<MarketExecutionsItem> MarketExecutions
        {
            get { return marketExecutions; }
            set
            {
                marketExecutions = value;

                //this.OnPropertyChanged();
            }
        }

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

        private Visibility chatFloatButtonVisibility;

        public Visibility ChatFloatButtonVisibility
        {
            get { return chatFloatButtonVisibility; }
            set { 
                chatFloatButtonVisibility = value;
                this.OnPropertyChanged();
            }
        }


        private Visibility headVisibility = Visibility.Collapsed;

        public Visibility HeadVisibility
        {
            get { return headVisibility; }
            set
            {
                headVisibility = value;

                if (headVisibility == Visibility.Visible)
                {
                    ExecTxt = "成交";
                    ExecTxtUnit = "手";
                }
                else
                {
                    ExecTxt = "成交";
                    ExecTxtUnit = "笔";
                }

                this.OnPropertyChanged();

            }
        }

        private string execTxt = "成交";

        public string ExecTxt
        {
            get
            {
                return execTxt;
            }

            set
            {
                execTxt = value;
                this.OnPropertyChanged();

            }
        }

        private string execTxtUnit = "笔";

        public string ExecTxtUnit
        {
            get
            {
                return execTxtUnit;
            }

            set
            {
                execTxtUnit = value;
                this.OnPropertyChanged();

            }
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


        private Visibility ohlcVisibility = Visibility.Visible;

        public Visibility OhlcVisibility
        {
            get { return ohlcVisibility; }
            set
            {
                ohlcVisibility = value;
                if (SelectedIndex == 1)
                {
                    ListVisibility = Visibility.Collapsed;
                    //if (OhlcVisibility == Visibility.Visible && )
                    ChartVisibility = OhlcVisibility;
                }
                else
                {
                    ListVisibility = OhlcVisibility;
                    ChartVisibility = Visibility.Collapsed;
                }
                this.OnPropertyChanged();

            }
        }

        private Visibility listVisibility = Visibility.Visible;

        public Visibility ListVisibility
        {
            get { return listVisibility; }
            set
            {
                listVisibility = value;
                this.OnPropertyChanged();

            }
        }

        private Visibility chartVisibility = Visibility.Collapsed;

        public Visibility ChartVisibility
        {
            get { return chartVisibility; }
            set
            {
                chartVisibility = value;
                this.OnPropertyChanged();

            }
        }

        private int _selecdtedIndex = 0;

        public int SelectedIndex {
            get 
            {
                return _selecdtedIndex;
            }
            set
            {
                _selecdtedIndex = value;

                if (value == 1)
                {
                    ListVisibility = Visibility.Collapsed;
                   // TipVisibility = Visibility.Collapsed;
                    ChartVisibility = OhlcVisibility;
                }
                else
                {
                    ListVisibility = OhlcVisibility;
                    //TipVisibility = ohlcVisibility;
                    ChartVisibility = Visibility.Collapsed;
                }
                this.OnPropertyChanged();
            }
        }


        #region tip刷新条提示

        private Visibility tipVisibility = Visibility.Collapsed;

        public Visibility TipVisibility
        {
            get { return tipVisibility; }
            set
            {
                tipVisibility = value;

                this.OnPropertyChanged();

            }
        }


        private string _tipTxt = CommonConstants.ToTop;

        public string TipTxt
        {
            get
            {
                return _tipTxt;
            }

            set
            {
                _tipTxt = value;
                this.OnPropertyChanged();

            }
        }

        private Brush _tipTxtBrush;
        public Brush TipTxtBrush
        {
            get { return _tipTxtBrush; }
            set { _tipTxtBrush = value; this.OnPropertyChanged(); }
        }

        private Brush _hoverTipTxtBrush;
        public Brush HoverTipTxtBrush
        {
            get { return _hoverTipTxtBrush; }
            set { _hoverTipTxtBrush = value; this.OnPropertyChanged(); }
        }

        private Brush _refreshBg;
        public Brush RefreshBg
        {
            get { return _refreshBg; }
            set { _refreshBg = value; this.OnPropertyChanged(); }
        }

        private Brush _hoverRefreshBg;
        public Brush HoverRefreshBg
        {
            get { return _hoverRefreshBg; }
            set { _hoverRefreshBg = value; this.OnPropertyChanged(); }
        }

        #endregion


        public void Init()
        {
            if (filteredMarketExecutions != null)
            {
                this.filteredMarketExecutions.IsLiveFilteringRequested = true;
                this.filteredMarketExecutions.IsLiveSortingRequested = true;
                this.filteredMarketExecutions.Source = MarketExecutions;//MarketExecutionProcesser.Processer.MyMarketExes;

                // 时间倒序
                SortDescription timeDesc = new SortDescription("Time", ListSortDirection.Descending);
                //SortDescription downUpSort = new SortDescription("DownUpSort", ListSortDirection.Ascending);
                this.filteredMarketExecutions.SortDescriptions.Add(timeDesc);
                //this.filteredMarketExecutions.SortDescriptions.Add(downUpSort);
                SortDescription idDesc = new SortDescription("OrderID", ListSortDirection.Descending);
                this.filteredMarketExecutions.SortDescriptions.Add(idDesc);
            }
        }
        
    }
}
