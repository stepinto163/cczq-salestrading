using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.UI;
using STC.WorkTable.Plugin.ViewModel.Data;
using STC.WorkTable.Plugin.ViewModel.WorkTable;
using STC.WorkTable.ViewModel.Data;
using System;
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
    /// IntentionList.xaml 的交互逻辑
    /// </summary>
    public partial class IntentionList : UserControl
    {

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(IntentionList));

        //public event RoutedEventHandler SelectionChanged
        //{
        //    add { AddHandler(SelectionChangedEvent, value); }
        //    remove { RemoveHandler(SelectionChangedEvent, value); }
        //}

        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }


        public IntentionList()
        {
            InitializeComponent();

            this.InterestRateDebtListBox.SelectionChanged += this.InterestRateDebtListBox_SelectionChanged;
            this.CreditBondListBox.SelectionChanged += this.CreditBondListBox_SelectionChanged;
            this.PolicyFinancialBondListBox.SelectionChanged += PolicyFinancialBondListBox_SelectionChanged;
            this.RemaingingTimeFilterListBox.SelectionChanged += this.RemaingingTimeFilterListBox_SelectionChanged;
            this.Rating_CurrentFilterListBox.SelectionChanged += this.Rating_CurrentFilterListBox_SelectionChanged;
            this.SettlTypeFilterListBox.SelectionChanged += SettlTypeFilterListBox_SelectionChanged;
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

        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null && mainViewModel.SelectedItem_Worked != null)
            {
                RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems));
            }
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                MainViewModel mainViewModel = GetViewModel();
                if (mainViewModel != null)
                {
                    mainViewModel.CheckBoxAllValueChange(flag);
                }
            }
        }
        
        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
                mainViewModel.CheckBoxSingleClick();
        }

        private void CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                if (sender is CheckBox)
                {
                    mainViewModel.SelectedItem_Worked = (TradeRequestData)((CheckBox)sender).DataContext;
                }
                e.Handled = true;
                if (sender != null && sender is CheckBox)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb != null && cb.DataContext != null && cb.DataContext is TradeRequestData)
                    {
                        TradeRequestData currentDRV = cb.DataContext as TradeRequestData;
                        if (currentDRV.Status == "4") // 已暂停
                        {
                            mainViewModel.DoStartOne();
                        }
                        else if (currentDRV.Status == "2") // 报价中
                        {
                            mainViewModel.DoStopOne();
                        }
                    }
                }
            }
        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NewIntentionList_dg.SelectedItem = ((Button)sender).DataContext;
        }

        private void detailBtn_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                OpenQuotesMiniWindows(mainViewModel.SelectedItem_Worked.SecurityId);
            }
        }

        private void OpenQuotesMiniWindows(string securityID)
        {
            string winkey = $"QuotesMiniWindows-{securityID}";
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (WindowManager.Instance.IsExist(winkey) == false)
                {
                    NoBorderWin myTradingPopupWin = new NoBorderWin(securityID, true);
                    myTradingPopupWin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A0F0E"));
                    myTradingPopupWin.Title = securityID;
                    PriceAndOrderView tradeBookView = new PriceAndOrderView(myTradingPopupWin);
                    myTradingPopupWin.SetMainControl(tradeBookView);
                    myTradingPopupWin.ResizeMode = ResizeMode.CanResize;
                    myTradingPopupWin.WindowStyle = WindowStyle.None;
                    myTradingPopupWin.WindowState = WindowState.Normal;
                    myTradingPopupWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    myTradingPopupWin.Owner = null;
                    myTradingPopupWin.Width = 290;
                    myTradingPopupWin.Height = 485;
                    myTradingPopupWin.Topmost = true;
                    tradeBookView.Init(securityID, myTradingPopupWin.myWindow);
                    WindowManager.Instance.OpenWindow(winkey, myTradingPopupWin);
                }
            }));
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
                mainViewModel.FilterBondType(InterestRateDebtListBox.SelectedItems);
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
                mainViewModel.FilterCreditBondType();
            }
        }
        
        private void PolicyFinancialBondListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterPolicyFinancialBond();
            }
        }

        private void RemaingingTimeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterRemainingTime();
            }
        }

        private void Rating_CurrentFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterRating();
            }
        }

        private void SettlTypeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            MainViewModel mainViewModel = GetViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.FilterSettlType();
            }
        }
    }
}
