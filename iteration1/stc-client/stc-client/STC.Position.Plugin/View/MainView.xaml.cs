using STC.Common.ColumnSetting;
using STC.Common.Utils;
using STC.Frame.CustomUI.TabPage;
using STC.Position.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : UserControl
    {
        MainViewModel mainViewModel;
        ScrollViewer sv_Detail, sv_Total;
        public MainView()
        {
            InitializeComponent();
            this.DataContext = mainViewModel;

            //this.InterestRateDebtListBox.SelectionChanged += this.InterestRateDebtListBox_SelectionChanged;
            //this.CreditBondListBox.SelectionChanged += this.CreditBondListBox_SelectionChanged;
            this.FutureTypeListBox.SelectionChanged += this.FutureTypeListBox_SelectionChanged;
            this.MarketIndicatorFilterListBox.SelectionChanged += this.MarketIndicatorListBox_SelectionChanged;
            //InitColumnsInfo();
            //this.Search_TextBox.ValueChanged += new RoutedEventHandler(SearchText_ValueChanged);
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;
        }

        private void Selected_Chk_Checked(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).RefreshNodeKeyWord();
        }

        private void dgDetail_Sorting(object sender, DataGridSortingEventArgs e)
        {
            (this.DataContext as MainViewModel).Sort(e.Column.SortMemberPath, e.Column.SortDirection.ToString());
            //e.Handled = true;
        }
        
        
        public void Load()
        {
            (this.DataContext as MainViewModel).InitView();
        }

        public void TryDispose()
        {
            (Rev as RiskExposureView).TryDispose();
            (this.DataContext as MainViewModel).TryDispose();
        }

        private void Search_TextBox_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            (this.DataContext as MainViewModel).RefreshNodeKeyWord();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            this.FilterPopup.IsOpen = true;
        }

        private void FilterPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            this.FilterPopup.IsOpen = false;
        }

        private void CloseFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.FilterPopup.IsOpen = false;
        }

        private void AdjustShowAll_ToggleButtonCheck()
        {
            bool isChecked = true;

            //if (InterestRateDebtListBox.SelectedItems.Count > 0
            //     || this.CreditBondListBox.SelectedItems.Count > 0)
            //{
            //    isChecked = false;
            //}

            //this.ShowAll_ToggleButton.IsChecked = isChecked;
            if(this.mainViewModel!=null)
            {
                this.mainViewModel.IsShowAllBondType=isChecked;
            }
        }

        private void InterestRateDebtListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdjustShowAll_ToggleButtonCheck();

            if (this.mainViewModel != null)
            {
                this.mainViewModel.Mv.View.Refresh();

                //更新持仓盈亏
                this.mainViewModel.ReCalcPosition();
            }
        }

        private void CreditBondListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdjustShowAll_ToggleButtonCheck();

            if (this.mainViewModel != null)
            {
                this.mainViewModel.Mv.View.Refresh();

                //更新持仓盈亏
                this.mainViewModel.ReCalcPosition();
            }
        }

        private void FutureTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.mainViewModel != null)
            {
                this.mainViewModel.Mv.View.Refresh();

                //更新持仓盈亏
                this.mainViewModel.ReCalcPosition();
            }            
        }

        private void MarketIndicatorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.mainViewModel != null)
            {
                this.mainViewModel.Mv.View.Refresh();

                //更新持仓盈亏
                this.mainViewModel.ReCalcPosition();
            }
        }

        private void settingColumn_Click(object sender, RoutedEventArgs e)
        {
            //if (WindowManager.Instance.IsExist("ColumnSetting") == false)
            //{
            //    ColumnSettingView csv = new ColumnSettingView(this.dgPositionDetail);
            //    //Window parentWindow = CommonUtils.GetParentObject<Window>(this);
            //    WindowManager.Instance.OpenWindow("ColumnSetting", csv);
            //}
        }

        private void dgDetail_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            DataGridColumnHeader dgch = CommonUtils.GetParentObject<DataGridColumnHeader>(obj);
            if (dgch != null)
            {
                dgch.ContextMenu = (ContextMenu)this.FindResource("settingColumnContextMenu");
            }
        }

        public void SearchText_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (this.mainViewModel != null)
            {
                this.mainViewModel.Mv.View.Refresh();


                //更新持仓盈亏
                this.mainViewModel.ReCalcPosition();
            }
        }

        private void dgStatistics_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            DataGridColumnHeader dgch = CommonUtils.GetParentObject<DataGridColumnHeader>(obj);
            if (dgch != null)
            {
                dgch.ContextMenu = (ContextMenu)this.FindResource("settingColumnContextMenu");
            }
        }

        /// <summary>
        /// 双击打开Tbook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgPositionDetail_DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();
            if (columnHeader == "代码" || columnHeader == "简称")
            {
                if (mainViewModel != null)
                {
                    mainViewModel.OpenTradeBook(mainViewModel.SelectedItem_dgPositionDetail);
                }
            }
        }

        private void TreeView_NodeList_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            mainViewModel.SelectedItem = (TreeView_NodeList.SelectedItem ?? "").ToString();
        }

        private void dgNewPositionDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var target = e.OriginalSource as DependencyObject;

            // 遍历可能包含双击事件的所有可能的父级元素
            while (target != null && !(target is DataGridCell))
            {
                target = VisualTreeHelper.GetParent(target);
            }
            if (target is DataGridCell)
            {
                DataGridCell currentDgCell = target as DataGridCell;
                string columnHeader = currentDgCell.Column.Header.ToString();
                if (columnHeader != "代码" && columnHeader != "简称")
                {
                    if (mainViewModel != null)
                    {
                        mainViewModel.OpenHistoryPosition(mainViewModel.SelectedItem_dgPositionDetail);
                    }
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.dgNewPositionTotal.ColumnDisplayIndexChanged -= DgNewPositionTotal_ColumnDisplayIndexChanged;
            this.dgNewPositionTotal.ColumnDisplayIndexChanged += DgNewPositionTotal_ColumnDisplayIndexChanged;

            int index = 0;
            foreach (var item in this.dgNewPositionTotal.Columns)
            {
                var cl = this.dgNewPositionDetail.Columns[index];
                Binding widthBd = new Binding();
                widthBd.Source = item;
                widthBd.Mode = BindingMode.OneWay;
                widthBd.Path = new PropertyPath(DataGridColumn.ActualWidthProperty);
                BindingOperations.SetBinding(cl, DataGridTextColumn.WidthProperty, widthBd);
                index++;
            }
        }

        private void DgNewPositionTotal_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            int index = 0;
            foreach (var item in this.dgNewPositionTotal.Columns)
            {
                var cl = this.dgNewPositionDetail.Columns[index];
                cl.DisplayIndex = item.DisplayIndex;
                index++;
            }
        }
        // 通用方法查找视觉子元素  
        private static ScrollViewer FindVisualChild<T>(DependencyObject dep)
        {
            while ((dep != null) && !(dep is ScrollViewer))
            {
                int count = VisualTreeHelper.GetChildrenCount(dep);
                if (count == 0)
                    break;
                if (count >= 9)
                    dep = VisualTreeHelper.GetChild(dep, 9);
                else
                    dep = VisualTreeHelper.GetChild(dep, 0);
            }
            ScrollViewer tb = dep as ScrollViewer;
            return tb;
        }
        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid)
            {
                DataGrid dataGrid = sender as DataGrid;
                var scrollViewer = FindVisualChild<ScrollViewer>(dataGrid);
                if (dataGrid.Name == "dgNewPositionTotal")
                    sv_Total = scrollViewer;
                else if (dataGrid.Name == "dgNewPositionDetail")
                {
                    sv_Detail = scrollViewer;
                }
            }
        }

        private void dgNewPositionDetail_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sv_Total != null)
            {
                sv_Total.ScrollToHorizontalOffset(sv_Detail.HorizontalOffset);
            }
            
        }
    }
}
