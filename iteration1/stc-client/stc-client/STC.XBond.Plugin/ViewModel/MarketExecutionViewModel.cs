using com.gateway.log;
using STC.Common.Handler.Base;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.TBook;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 市场成交MarketExecutionView的数据内容
    /// </summary>
    public class MarketExecutionViewModel : ViewModelBase
    {
        private CollectionViewSource filteredMarketExecutions = new CollectionViewSource();
        public CollectionViewSource FilteredMarketExecutions
        {
            get { return filteredMarketExecutions; }
            set { filteredMarketExecutions = value; }
        }

        private bool isFilterStar = false;

        public ICommand StarFilterCommand { get { return new RelayCommand(OnStarClick); } }

        private void OnStarClick()
        {
            isFilterStar = !isFilterStar;
            this.FilteredMarketExecutions.View.Refresh();
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
                    this.OnPropertyChanged();
                    GetRocksDbDataBySecurityId(_searchText);
                }
            }
        }

        public static List<KeyValuePair<string, string>> SideItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Side");
            }
        }

        public MarketExecutionViewModel()
        {
            try
            {
                this.filteredMarketExecutions.IsLiveFilteringRequested = true;
                this.filteredMarketExecutions.IsLiveSortingRequested = true;
                this.filteredMarketExecutions.Source = MarketExecutionProcesser.Processer.MyMarketExes;
                this.filteredMarketExecutions.Filter += FilteredMarketExecutions_Filter;

                // 时间倒序
                SortDescription timeDesc = new SortDescription("Time", ListSortDirection.Descending);
                this.filteredMarketExecutions.SortDescriptions.Add(timeDesc);
                
            }
            catch(Exception e)
            {
                LogHelper.Error(e);
            }
        }

        #region 从RocksDb读取数据

        public ObservableCollection<MarketExecutionsItem> tmpsource = new ObservableCollection<MarketExecutionsItem>();
        public void GetRocksDbDataBySecurityId(string key)
        {
            SortDescription timeDesc = new SortDescription("Time", ListSortDirection.Descending);
            MarketExecutionProcesser.Processer.SearchText = key;

            if (string.IsNullOrEmpty(key))
            {
                this.filteredMarketExecutions.IsLiveFilteringRequested = true;
                this.filteredMarketExecutions.IsLiveSortingRequested = true;
                this.filteredMarketExecutions.Source = MarketExecutionProcesser.Processer.MyMarketExes;
                // 时间倒序
                this.filteredMarketExecutions.SortDescriptions.Add(timeDesc);
                return;
            }

            this.filteredMarketExecutions.IsLiveFilteringRequested = true;
            this.filteredMarketExecutions.IsLiveSortingRequested = true;
            this.filteredMarketExecutions.Source = MarketExecutionProcesser.Processer.TmpMarketExes;
            this.filteredMarketExecutions.SortDescriptions.Add(timeDesc);
        }

        public void GetRocksDbDataByItem(object mei)
        {
            //如果不是搜索，则动态获取前面的记录
            if (string.IsNullOrEmpty(SearchText))
            {
                MarketExecutionsItem marketExe = mei as MarketExecutionsItem;
                string key = $"{marketExe.Time}";
                MarketExecutionProcesser.Processer.GetRocksDbDataByItemKey(key);
            }
        }

        #endregion


        ~MarketExecutionViewModel()
        {
            Dispose();
        }

        new public void Dispose()
        {
            this.filteredMarketExecutions = null;
            base.Dispose();
        }

        private void FilteredMarketExecutions_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = this.MarketExecutionFilter(e.Item);
        }

        private bool MarketExecutionFilter(object item)
        {
            var marketExecutionItem = item as MarketExecutionsItem;

            if (string.IsNullOrEmpty(this.SearchText) == true)
            {
                return filterStar(marketExecutionItem);
            }

            
            return marketExecutionItem != null && marketExecutionItem.SecurityID != null &&
                marketExecutionItem.SecurityID.ToLower().Contains(this.SearchText.ToString().ToLower())
                && filterStar(marketExecutionItem);
        }

        private bool filterStar(MarketExecutionsItem item)
        {
            return !isFilterStar? true: isFilterStar && item.ShowStar == Visibility.Visible;
        }
    }
}