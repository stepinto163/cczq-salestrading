using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.WorkTable.Plugin.ViewModel.WorkTable;
using STC.WorkTable.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.WorkTable.Plugin.View.WorkTable
{
    /// <summary>
    /// HistoryIntentionList.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryIntentionList : UserControl
    {
        private string today = string.Empty;
        private string week = string.Empty;
        private string month = string.Empty;
        private string quarter = string.Empty;

        private bool isClick = true;
        public bool IsClick
        {
            get
            {
                return isClick;
            }

            set
            {
                isClick = value;
            }
        }

        public IList SelectedItems_HistoryFilter 
        { 
            get 
            { 
                return this.HistoryFilter.SelectedItems;
            } 
        }

        public int SelectedIndex_HistoryFilter
        {
            get 
            {
                return this.HistoryFilter.SelectedIndex;
            }
            set
            {
                this.HistoryFilter.SelectedIndex = value;
            }
        }

        public HistoryIntentionList()
        {
            InitializeComponent();

            this.InterestRateDebtListBox.SelectionChanged += this.InterestRateDebtListBox_SelectionChanged;
            this.CreditBondListBox.SelectionChanged += this.CreditBondListBox_SelectionChanged;
            this.PolicyFinancialBondListBox.SelectionChanged += PolicyFinancialBondListBox_SelectionChanged;
            this.RemaingingTimeFilterListBox.SelectionChanged += this.RemaingingTimeFilterListBox_SelectionChanged;
            this.Rating_CurrentFilterListBox.SelectionChanged += this.Rating_CurrentFilterListBox_SelectionChanged;
            this.SettlTypeFilterListBox.SelectionChanged += SettlTypeFilterListBox_SelectionChanged;

            today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            week = DateTimeEx.ServerTime.AddDays(-7).ToString("yyyy-MM-dd");
            month = DateTimeEx.ServerTime.AddDays(-30).ToString("yyyy-MM-dd");
            quarter = DateTimeEx.ServerTime.AddDays(-90).ToString("yyyy-MM-dd");

            this.dpFrom.SelectedDate = DateTimeEx.ServerTime;
            this.dpTo.SelectedDate = DateTimeEx.ServerTime;

            string startDate = this.dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
            string endDate = this.dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd");
            this.SetTimeSpanType(startDate, endDate);

            try
            {
                BaseHandler.Instance.Init(Instance_OnClear);
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {

                        today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                        week = DateTimeEx.ServerTime.AddDays(-7).ToString("yyyy-MM-dd");
                        month = DateTimeEx.ServerTime.AddDays(-30).ToString("yyyy-MM-dd");
                        quarter = DateTimeEx.ServerTime.AddDays(-90).ToString("yyyy-MM-dd");

                        this.SetTimeSpanType(this.dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd"), this.dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
                    }));
                }
            }
        }

        private MainViewModel GetViewModel()
        {
            if (this.DataContext != null && this.DataContext is MainViewModel)
            {
                MainViewModel mainViewModel = this.DataContext as MainViewModel;
                return mainViewModel;
            }
            return null;
        }

        private void TimeSpanType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;

                ListBoxItem selected = listBox.SelectedItem as ListBoxItem;
                if (selected != null)
                {
                    SetDpFromToDate(selected.Content.ToString());
                }
            }
        }
        
        private void dpFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue)
            {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value)
                {
                    dpTo.SelectedDate = dpFrom.SelectedDate.Value;
                }
            }
            IList list = new ArrayList();
            if (dpFrom.SelectedDate.HasValue)
                list.Add(dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            if (dpTo.SelectedDate.HasValue)
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(DateTime.Now.Date.ToString("yyyy-MM-dd"));
            SetTimeSpanType(list[0].ToString(), list[1].ToString());
            IsClick = true;
        }

        private void dpTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue)
            {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value)
                {
                    dpFrom.SelectedDate = dpTo.SelectedDate.Value;
                }
            }
            IList list = new ArrayList();
            if (dpFrom.SelectedDate.HasValue)
                list.Add(dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            if (dpTo.SelectedDate.HasValue)
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(DateTime.Now.Date.ToString("yyyy-MM-dd"));
            SetTimeSpanType(list[0].ToString(), list[1].ToString());
            IsClick = true;
        }

        private void HistoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;

                int isInnerCount = 0;// 如果是true的个数是0或者2时， allinner按钮被选中 
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (("同客户".Equals(lbi.Content)
                    || "同券反向".Equals(lbi.Content))
                    && lbi.IsSelected == true)
                    {
                        isInnerCount++;
                    }
                }

                ListBoxItem tmpInner = (ListBoxItem)listBox.Items[0];// 这个是allinner按钮
                if (isInnerCount == 2)
                {
                    tmpInner.IsSelected = true;
                }
                else if (isInnerCount > 0 && isInnerCount < 3)
                {
                    tmpInner.IsSelected = false;
                }
            }
            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
                mainViewModel.HistoryFilterSelectedItems = this.HistoryFilter.SelectedItems.Cast<ListBoxItem>().ToList();
        }

        private void SetDpFromToDate(string text)
        {
            if (string.IsNullOrEmpty(today) || string.IsNullOrEmpty(week) || string.IsNullOrEmpty(month) || string.IsNullOrEmpty(quarter))
            {
                return;
            }

            switch (text)
            {
                case "今日":
                    this.dpFrom.SelectedDate = Convert.ToDateTime(today);
                    this.dpTo.SelectedDate = Convert.ToDateTime(today);
                    break;

                case "近1周":
                    this.dpFrom.SelectedDate = Convert.ToDateTime(week);
                    this.dpTo.SelectedDate = Convert.ToDateTime(today);
                    break;

                case "近1月":
                    this.dpFrom.SelectedDate = Convert.ToDateTime(month);
                    this.dpTo.SelectedDate = Convert.ToDateTime(today);
                    break;
            }
        }

        public void SetTimeSpanType(string begin, string end)
        {
            ListBoxItem itemToday = this.TimeSpanType.Items[0] as ListBoxItem;
            ListBoxItem itemWeek = this.TimeSpanType.Items[1] as ListBoxItem;
            ListBoxItem itemMonth = this.TimeSpanType.Items[2] as ListBoxItem;

            itemToday.IsSelected = false;
            itemWeek.IsSelected = false;
            itemMonth.IsSelected = false;

            if (end == today)
            {
                if (begin == end && end == today)
                {
                    itemToday.IsSelected = true;
                }
                else
                {

                    if (begin == week)
                    {
                        itemWeek.IsSelected = true;
                    }
                    else if (begin == month)
                    {
                        itemMonth.IsSelected = true;
                    }
                }
            }
        }


        private void FilterPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            FilterPopup.IsOpen = false;
        }

        private void CloseFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.FilterPopup.IsOpen = false;
        }

        private bool isNonmanualCheckShowAll = false;
        //private bool isCheckShowAll = false;
        private void ShowAll_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }

            //isCheckShowAll = true;
            if (this.InterestRateDebtListBox != null)
            {
                this.InterestRateDebtListBox.SelectedIndex = -1;
            }

            if (this.CreditBondListBox != null)
            {
                this.CreditBondListBox.SelectedIndex = -1;
            }

            //isCheckShowAll = false;
        }

        private void ShowAll_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }
        }
        private void ShowAll_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.InterestRateDebtListBox.SelectedIndex = -1;
            this.CreditBondListBox.SelectedIndex = -1;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = true;
        }

        private const string ALL = "全部";

        private void ItemsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (e.AddedItems.Count > 0)
                {
                    foreach (object obj in e.AddedItems)
                    {
                        FilterListBoxItemData item = obj as FilterListBoxItemData;
                        if (item != null)
                        {
                            if (ALL.Equals(item.itemText))
                            {
                                foreach (FilterListBoxItemData lbi in listBox.Items)
                                {
                                    if (lbi.itemIsSelected && !ALL.Equals(lbi.itemText))
                                        lbi.itemIsSelected = false;
                                }
                            }
                            else
                            {
                                FilterListBoxItemData tmp = (FilterListBoxItemData)listBox.Items[0];
                                if (tmp.itemIsSelected)
                                    tmp.itemIsSelected = false;

                            }
                        }
                    }
                }

                if (e.RemovedItems.Count > 0)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        if (listBox.SelectedItems.Count == 0)
                            listBox.SelectedItem = item;
                    }
                }
            }
        }
        private void AdjustShowAll_ToggleButtonCheck()
        {
            bool isChecked = true;

            if (InterestRateDebtListBox.SelectedItems.Count > 0
                 || this.CreditBondListBox.SelectedItems.Count > 0)
            {
                isChecked = false;
            }

            isNonmanualCheckShowAll = true;
            this.ShowAll_ToggleButton.IsChecked = isChecked;
            isNonmanualCheckShowAll = false;
        }

        private void InterestRateDebtListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (isCheckShowAll == true)
            //    return;

            AdjustShowAll_ToggleButtonCheck();

            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterBondType_History(InterestRateDebtListBox.SelectedItems);
            }
        }

        private void CreditBondListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (isCheckShowAll == true)
            //    return;

            AdjustShowAll_ToggleButtonCheck();

            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterCreditBondType_history();
            }
        }

        private void PolicyFinancialBondListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);

            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterPolicyFinancialBond_History();
            }
        }

        private void RemaingingTimeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);

            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterRemainingTime_History();
            }
        }

        private void Rating_CurrentFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);

            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterRating_History();
            }
        }

        private void SettlTypeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);

            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterSettlType_History();
            }
        }
    }
}
