using STC.Common.Utils;
using STC.WorkTable.Plugin.InnerOrders.ViewModel;
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

namespace STC.WorkTable.Plugin.InnerOrders.View
{
    /// <summary>
    /// InnerBoardView.xaml 的交互逻辑
    /// </summary>
    public partial class InnerBoardView : UserControl
    {
        private InnerBoardViewModel _innerBoardViewModel = new InnerBoardViewModel();

        public InnerBoardView()
        {
            InitializeComponent();
            this.DataContext = _innerBoardViewModel;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = true;
        }

        private void FilterPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            FilterPopup.IsOpen = false;
        }

        private void InnerBoardFilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.InnerBoardFilterPanel.FilterDic != null && _innerBoardViewModel != null)
            {
                // 过滤
                _innerBoardViewModel.FilterData(this.InnerBoardFilterPanel.FilterDic);
            }
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
            AdjustShowAll_ToggleButtonCheck();

            if (_innerBoardViewModel != null)
            {
                _innerBoardViewModel.FilterBondType(InterestRateDebtListBox.SelectedItems);
            }
        }

        private void CreditBondListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdjustShowAll_ToggleButtonCheck();

            if (_innerBoardViewModel != null)
            {
                _innerBoardViewModel.FilterCreditBondType();
            }
        }

        private void RemaingingTimeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (_innerBoardViewModel != null)
            {
                _innerBoardViewModel.FilterRemainingTime();
            }
        }

        private void Rating_CurrentFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (_innerBoardViewModel != null)
            {
                _innerBoardViewModel.FilterRating();
            }
        }

        private void InnerBoard_dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_innerBoardViewModel != null)
            {
                _innerBoardViewModel.OpenTradeBook();
            }
        }

        private void SearchMenu_Click(object sender, RoutedEventArgs e)
        {
            SearchSecurityID();
        }

        private void SearchSecurityID()
        {
            if (_innerBoardViewModel.SelectedItem_MyInnerOrders != null)
            {
                _innerBoardViewModel.SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID;
                this.Search_TextBox.SetPop(false);
            }
            else
            {
                LogHandler.Info("IntentionPanel DeleteOrder 没有取到数据");
            }
        }
    }
}
