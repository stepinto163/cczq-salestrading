using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.XBond.Plugin.ViewModel.HomePage;
using STC.XBond.Plugin.View.Component;
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
using System.Data;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel.TradeBookHandler;
using STC.Frame.Common;
using STC.Common.View;
using STC.Common.Utils;
using STC.Common.ColumnSetting;
using System.Collections.ObjectModel;
using STC.Common.Handler.Admin;
using STC.Common.ViewModel;
using STC.Common.Model.Base;
using STC.Common.View.Order;

namespace STC.XBond.Plugin.View.HomePage
{
    /// <summary>
    /// FavoriteBondsListBox.xaml 的交互逻辑
    /// </summary>
    public partial class FavoriteBondsListBox : UserControl//, ISettingColumnsBase
    {
        private string _TRADEBOOK = "Tradebook";
        private FavoriteBondsListBoxViewModel _FavoriteBondsListBoxViewModel = new FavoriteBondsListBoxViewModel();

        public FavoriteBondsListBox()
        {
            InitializeComponent();
            _FavoriteBondsListBoxViewModel.MainView = this;
            this.DataContext = _FavoriteBondsListBoxViewModel;
            this.SizeChanged += FavoriteBondsListBox_SizeChanged;
            this.InterestRateDebtListBox.SelectionChanged += this.InterestRateDebtListBox_SelectionChanged;
            this.CreditBondListBox.SelectionChanged += this.CreditBondListBox_SelectionChanged;
            this.RemaingingTimeFilterListBox.SelectionChanged += this.RemaingingTimeFilterListBox_SelectionChanged;
            this.Rating_CurrentFilterListBox.SelectionChanged += this.Rating_CurrentFilterListBox_SelectionChanged;
            // this.SettlTypeFilterListBox.SelectionChanged += this.SettlTypeFilterListBox_SelectionChanged;

            //KeyDown += FavoriteBondsListBox_KeyDown;

            //DesignHandler.Instance.OnKeyDown += Instance_OnKeyDown;

            //this.Unloaded += FavoriteBondsListBox_Unloaded;
        }
        

        private void FavoriteBondsListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            //DesignHandler.Instance.OnKeyDown -= Instance_OnKeyDown;

            this.Unloaded -= FavoriteBondsListBox_Unloaded;

            this.SizeChanged -= FavoriteBondsListBox_SizeChanged;
            this.InterestRateDebtListBox.SelectionChanged -= this.InterestRateDebtListBox_SelectionChanged;
            this.CreditBondListBox.SelectionChanged -= this.CreditBondListBox_SelectionChanged;
            this.RemaingingTimeFilterListBox.SelectionChanged -= this.RemaingingTimeFilterListBox_SelectionChanged;
            this.Rating_CurrentFilterListBox.SelectionChanged -= this.Rating_CurrentFilterListBox_SelectionChanged;
        }

        private void Instance_OnKeyDown()
        {
            object obj = AppUtils.Application.Panel.SelectedItem;
            if (obj != null)
            {
                TabItem tabItem = obj as TabItem;
                if (tabItem != null && tabItem.Tag.ToString() == "80")
                    FavoriteBondsListBox_KeyDown(null, null);
            }

        }

        public void OnKeyDown()
        {
            this.FavoriteBondsListBox_KeyDown(null, null);
        }

        private void FavoriteBondsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                DataRowView dataRowView = FavoriteBondslistBox.SelectedItem as DataRowView;
                string securityID = _TRADEBOOK;
                //Ofr
                if (Keyboard.IsKeyDown(Key.O))
                {
                    if (dataRowView != null)
                    {
                        securityID = dataRowView["securityID"] as string;
                        QuickOrderHandler.Instance.QuickPrice(null, false, securityID);
                    }
                    else
                    {
                        QuickOrderHandler.Instance.QuickPrice(null, false, securityID);
                    }
                }
                else if (Keyboard.IsKeyDown(Key.B))
                {
                    //Bid
                    if (dataRowView != null)
                    {
                        securityID = dataRowView["securityID"] as string;
                        QuickOrderHandler.Instance.QuickPrice(null, true, securityID);
                    }
                    else
                    {
                        QuickOrderHandler.Instance.QuickPrice(null, true, securityID);
                    }
                }
            }
        }

        public void ClearView()
        {
            _FavoriteBondsListBoxViewModel.Clear();
        }

        public void Init(HomeGroupInfo gi)
        {
            _FavoriteBondsListBoxViewModel.groupID = gi.id;
            if (HomePageDataHandler.CatchHomePageDisplayModeDic.ContainsKey(gi.id))
            {
                string value = HomePageDataHandler.CatchHomePageDisplayModeDic[gi.id];
                if (value == "1")
                {
                    cardMode_btn.IsChecked = true;
                }
            }
           
            _FavoriteBondsListBoxViewModel.Init(this);
        }

        public void Init2(string id)
        {
            _FavoriteBondsListBoxViewModel.groupID = id;
            _FavoriteBondsListBoxViewModel.Init(this);
        }

        private void FavoriteBondsListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Console.WriteLine("QuoteUI_SizeChanged actual: {0},{1}", (int)FavoriteBondslistBox.ActualWidth, (int)FavoriteBondslistBox.ActualHeight);
            //Console.WriteLine("QuoteUI_SizeChanged this.actual: {0},{1}", (int)this.ActualWidth, (int)this.ActualHeight);
        }

        private void listMode_btn_Click(object sender, RoutedEventArgs e)
        {
            ShowSortComboBox(false);
            // 转换成list模式的样式
            if (this.FavoriteBondslistBox != null)
            {
                FavoriteBondslistBox.Visibility = Visibility.Collapsed;
                FavoriteBondsDg.Visibility = Visibility.Visible;

                #region old code
                //ScrollViewer.SetHorizontalScrollBarVisibility(this.FavoriteBondslistBox, ScrollBarVisibility.Auto);
                ////ItemsPanelTemplate list_ItemsPanelTemplate = (ItemsPanelTemplate)this.FindResource("List_ItemsPanelTemplate");

                //Style list_ListViewItemStyle = (Style)this.FindResource("List_ListViewItemStyle");
                //GridView list_View = (GridView)this.FindResource("List_View");
                //this.FavoriteBondslistBox.View = list_View;
                ////this.FavoriteBondslistBox.ItemsPanel = list_ItemsPanelTemplate;
                //this.FavoriteBondslistBox.ItemContainerStyle = list_ListViewItemStyle;
                //this.FavoriteBondslistBox.ItemTemplate = null;
                #endregion
                HomePageDataHandler.CatchHomePageDisplayModeDic[this._FavoriteBondsListBoxViewModel.groupID] = "0";
                HomePageDataHandler.WriteHomePageDisplayMode();
            
                InitColumnsInfo(this.GetColumnsInfoStr());
            }
        }

        private void cardMode_btn_Click(object sender, RoutedEventArgs e)
        {
            ShowSortComboBox(true);
            // 转换成Cards模式的样式
            if (this.FavoriteBondslistBox != null)
            {
                FavoriteBondslistBox.Visibility = Visibility.Visible;
                FavoriteBondsDg.Visibility = Visibility.Collapsed;

                ScrollViewer.SetHorizontalScrollBarVisibility(this.FavoriteBondslistBox, ScrollBarVisibility.Disabled);

                //ItemsPanelTemplate card_ItemsPanelTemplate = (ItemsPanelTemplate)this.FindResource("Card_ItemsPanelTemplate");
                Style card_ListViewItemStyle = (Style)this.FindResource("Card_ListViewItemStyle");
                DataTemplate card_DataTemplate = (DataTemplate)this.FindResource("Card_DataTemplate");
                //this.FavoriteBondslistBox.ItemsPanel = card_ItemsPanelTemplate;
                this.FavoriteBondslistBox.ItemContainerStyle = card_ListViewItemStyle;
                this.FavoriteBondslistBox.ItemTemplate = card_DataTemplate;
                this.FavoriteBondslistBox.View = this.FindResource("cardView") as PlainView;
                HomePageDataHandler.CatchHomePageDisplayModeDic[this._FavoriteBondsListBoxViewModel.groupID] = "1";
                HomePageDataHandler.WriteHomePageDisplayMode();
            }
        }

        //static Thickness SearchBtn_Thick1 = new Thickness(0, 0, 98, 0);
        //static Thickness Search_TextBox_Thick1 = new Thickness(0, 0, 126, 0);
        //static Thickness SearchBtn_Thick2 = new Thickness(0, 0, 286, 0);
        //static Thickness Search_TextBox_Thick2 = new Thickness(0, 0, 314, 0);

        private void ShowSortComboBox(bool flag)
        {
            if (flag == false)
            {
                //this.SearchBtn.Margin = SearchBtn_Thick1;
                //this.Search_TextBox.Margin = Search_TextBox_Thick1;
                //this.sep.Visibility = Visibility.Collapsed;
                //this.sort_CBC.Visibility = Visibility.Collapsed;
                //this.sort_textblock.Visibility = Visibility.Collapsed;
                sortGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                //this.SearchBtn.Margin = SearchBtn_Thick2;
                //this.Search_TextBox.Margin = Search_TextBox_Thick2;
                //this.sep.Visibility = Visibility.Visible;
                //this.sort_CBC.Visibility = Visibility.Visible;
                //this.sort_textblock.Visibility = Visibility.Visible;
                sortGrid.Visibility = Visibility.Visible;
            }

        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            this.FilterPopup.IsOpen = true;
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
                        ListBoxItem item = obj as ListBoxItem;
                        if (item != null)
                        {
                            if (ALL.Equals(item.Content))
                            {
                                foreach (ListBoxItem lbi in listBox.Items)
                                {
                                    if (lbi.IsSelected && !ALL.Equals(lbi.Content))
                                        lbi.IsSelected = false;
                                }
                            }
                            else
                            {
                                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                if (tmp.IsSelected)
                                    tmp.IsSelected = false;

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

        private void InterestRateDebtListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (isCheckShowAll == true)
            //    return;

            AdjustShowAll_ToggleButtonCheck();

            if (this._FavoriteBondsListBoxViewModel != null)
            {
                this._FavoriteBondsListBoxViewModel.FilterBondType(InterestRateDebtListBox.SelectedItems);
            }
        }

        private void CreditBondListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (isCheckShowAll == true)
            //    return;

            AdjustShowAll_ToggleButtonCheck();

            if (this._FavoriteBondsListBoxViewModel != null)
            {
                this._FavoriteBondsListBoxViewModel.FilterCreditBondType(CreditBondListBox.SelectedItems);
            }
        }

        private void RemaingingTimeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._FavoriteBondsListBoxViewModel != null)
            {
                this._FavoriteBondsListBoxViewModel.FilterRemainingTime(RemaingingTimeFilterListBox.SelectedItems);
            }
        }

        private void Rating_CurrentFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._FavoriteBondsListBoxViewModel != null)
            {
                this._FavoriteBondsListBoxViewModel.FilterRating(Rating_CurrentFilterListBox.SelectedItems);
            }
        }

        private void SettlTypeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._FavoriteBondsListBoxViewModel != null)
            {
               // this._FavoriteBondsListBoxViewModel.FilterSettlType(SettlTypeFilterListBox.SelectedItems);
            }
        }

        private void CloseFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.FilterPopup.IsOpen = false;
        }

        private void Rating_CurrentFilterListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var item10 = this.Rating_CurrentFilterListBox.Items[0];
            if (item10 != null && item10 is ListBoxItem)
            {
                ListBoxItem lbi = item10 as ListBoxItem;
                var reg = lbi.Template.FindName("Sep", lbi);
                if (reg != null && reg is Rectangle)
                {
                    ((Rectangle)reg).Visibility = Visibility.Hidden;
                }
            }
        }

        public void Clear()
        {
            if (this._FavoriteBondsListBoxViewModel != null)
            {
                //this._FavoriteBondsListBoxViewModel.Clear();
                this._FavoriteBondsListBoxViewModel.Dispose();
                this.DataContext = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddBondWindow addBondWind = new AddBondWindow();
            addBondWind.Owner = Application.Current.MainWindow;
            addBondWind.ShowDialog();
            bool result = addBondWind.Result;

            if (result == true)
            {
                HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                request.securityid = addBondWind.securityID;
                request.symbol = addBondWind.symbol;
                request.action = "1"; // 添加
                request.remark = $"{addBondWind.securityID}  {addBondWind.symbol}  {addBondWind.remainingTime}";
                request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                request.type = CommonConstants.HOME_Type;
                
                _FavoriteBondsListBoxViewModel.AddAndUpdateBond(request);
            }
        }

        private void del_btn_Click(object sender, RoutedEventArgs e)
        {
            // 选中Item
            var item = this.FavoriteBondslistBox.ItemContainerGenerator.ContainerFromItem((sender as Button).DataContext as DataRowView);
            if (item == null)
            {
                Common.View.Component.CustomMessageBox.Info("请先选择债券！", "");
                return;
            }
            (item as ListBoxItem).IsSelected = true;

            string text = "是否确定删除该债券？";
            string title = "删除债券";
            if (Common.View.Component.CustomMessageBox.Question(text, title) == true)
            {
                _FavoriteBondsListBoxViewModel.DelBond();
            }
        }

        /// <summary>
        /// 行情小窗
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void opensecurity_btn_Click(object sender, RoutedEventArgs e)
        {
            // 选中Item
            var item = (sender as Button).DataContext as DataRowView;
            _FavoriteBondsListBoxViewModel.OpenSecurity();
        }

        private void BatchImportBtn_Click(object sender, RoutedEventArgs e)
        {
            BatchAddBondsWindow batchAddBondsWind = new BatchAddBondsWindow(_FavoriteBondsListBoxViewModel.groupID);
            batchAddBondsWind.Owner = Application.Current.MainWindow;
            batchAddBondsWind.Show();
        }

        GridViewColumnHeader lstGVCH = null;
        MyListSortDirection lstListDirection = MyListSortDirection.None;

        /// <summary>
        /// ‘债券基本信息’和 ‘剩余期限’字段支持排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader)
            {
                //获得点击的列
                GridViewColumnHeader gridViewColumnHeader = (e.OriginalSource as GridViewColumnHeader);
                string columnHeader = gridViewColumnHeader.Content.ToString();
                string columnName = _FavoriteBondsListBoxViewModel.ConvertColumnNameFromHeader(columnHeader);
                if (columnName == null)
                {
                    return;
                }

                //GridViewColumn clickedColumn = gridViewColumnHeader.Column;
                if (lstGVCH != gridViewColumnHeader)
                {
                    if (lstGVCH != null)
                    {
                        SortDirectionManager.SetSortDirection(lstGVCH, MyListSortDirection.None);
                    }
                    
                    lstGVCH = gridViewColumnHeader;
                }

                MyListSortDirection listSortDirection = SortDirectionManager.GetSortDirection(gridViewColumnHeader);

                if (listSortDirection == MyListSortDirection.None
                    || listSortDirection == MyListSortDirection.Descending)
                {
                    lstListDirection = MyListSortDirection.Ascending;
                }
                else if (listSortDirection == MyListSortDirection.Ascending)
                {
                    lstListDirection = MyListSortDirection.Descending;
                }
                SortDirectionManager.SetSortDirection(gridViewColumnHeader, lstListDirection);

                _FavoriteBondsListBoxViewModel.SortDv(columnName, lstListDirection);

                this.sort_CBC.SelectedIndex = 0;
            }
        }

        private void sort_CBC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            string selectContent = (e.AddedItems[0] as ComboBoxItem).Content.ToString();
            if (selectContent == "债券代码(升序)")
            {
                string columnName = _FavoriteBondsListBoxViewModel.ConvertColumnNameFromHeader("债券代码");
                _FavoriteBondsListBoxViewModel.SortDv(columnName, MyListSortDirection.Ascending);
                
                if (this.FavoriteBondslistBox.View is GridView)
                {
                    GridViewColumnHeader securityHeader = CommonUtils.GetVisualChild<GridViewColumnHeader>(this.FavoriteBondslistBox, dp => dp.Content != null && dp.Content.ToString() == "债券基本信息");
                    SortDirectionManager.SetSortDirection(securityHeader, MyListSortDirection.Ascending);
                }
            }
            else if (selectContent == "债券代码(降序)")
            {
                string columnName = _FavoriteBondsListBoxViewModel.ConvertColumnNameFromHeader("债券代码");
                _FavoriteBondsListBoxViewModel.SortDv(columnName, MyListSortDirection.Descending);
                if (this.FavoriteBondslistBox.View is GridView)
                {
                    GridViewColumnHeader securityHeader = CommonUtils.GetVisualChild<GridViewColumnHeader>(this.FavoriteBondslistBox, dp => dp.Content != null && dp.Content.ToString() == "债券基本信息");
                    SortDirectionManager.SetSortDirection(securityHeader, MyListSortDirection.Descending);
                }
            }
            else if(selectContent == "剩余期限(升序)")
            {
                string columnName = _FavoriteBondsListBoxViewModel.ConvertColumnNameFromHeader("剩余期限");
                _FavoriteBondsListBoxViewModel.SortDv(columnName, MyListSortDirection.Ascending);
                if (this.FavoriteBondslistBox.View is GridView)
                {
                    GridViewColumnHeader securityHeader = CommonUtils.GetVisualChild<GridViewColumnHeader>(this.FavoriteBondslistBox, dp => dp.Content != null && dp.Content.ToString() == "剩余期限");
                    SortDirectionManager.SetSortDirection(securityHeader, MyListSortDirection.Ascending);
                }
            }
            else if (selectContent == "剩余期限(降序)")
            {
                string columnName = _FavoriteBondsListBoxViewModel.ConvertColumnNameFromHeader("剩余期限");
                _FavoriteBondsListBoxViewModel.SortDv(columnName, MyListSortDirection.Descending);
                if (this.FavoriteBondslistBox.View is GridView)
                {
                    GridViewColumnHeader securityHeader = CommonUtils.GetVisualChild<GridViewColumnHeader>(this.FavoriteBondslistBox, dp => dp.Content != null && dp.Content.ToString() == "剩余期限");
                    SortDirectionManager.SetSortDirection(securityHeader, MyListSortDirection.Descending);
                }
            }
        }

        private void FavoriteBondslistBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //// 选中Item
            //var item = this.FavoriteBondslistBox.ItemContainerGenerator.ContainerFromItem((sender as Label).DataContext as DataRowView);
            //(item as ListBoxItem).IsSelected = true;

            _FavoriteBondsListBoxViewModel.OpenTradeBook();
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

        private void ShowAll_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.InterestRateDebtListBox.SelectedIndex = -1;
            this.CreditBondListBox.SelectedIndex = -1;
        }

        private void FilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.FilterPanel.FilterDic != null && _FavoriteBondsListBoxViewModel != null)
            {
                // 过滤
                _FavoriteBondsListBoxViewModel.FilterData(this.FilterPanel.FilterDic);
            }
        }

        public event Action<object> OnSectionChange;
        private void FavoriteBondslistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnSectionChange?.Invoke(FavoriteBondslistBox.SelectedItem);
        }

        public void OnUpdateSelectedItem(object dr)
        {
            OnSectionChange?.Invoke(dr);
        }

        private void BtnAddBond_Click(object sender, RoutedEventArgs e)
        {
            Button_Click(null, null);
        }

        private void Bid_OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRowView = null;
            if (FavoriteBondslistBox.Visibility == Visibility.Visible)
            {
                dataRowView = FavoriteBondslistBox.SelectedItem as DataRowView;
            }
            else
            {
                dataRowView = FavoriteBondsDg.SelectedItem as DataRowView;
            }
            if (dataRowView != null)
            {
                string securityID = dataRowView["securityID"] as string;
                BondsInfoPo bond = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                string marketIndicator = bond != null ? bond.MarketIndicator : Consts.MARKETINDICATOR_BOND;
                if (string.IsNullOrEmpty(securityID))
                    securityID = _TRADEBOOK;

                if (!string.IsNullOrEmpty(securityID))
                {
                    //国债期货报价
                    if (CommonUtils.IsTreasuryFuture(securityID))
                    {
                        if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceFutureWinKey(WindowKeyHelper.BID, securityID)))
                        {
                            SendPriceFutureViewLite sendPriceView = new SendPriceFutureViewLite(securityID, true);
                            WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceFutureWinKey(WindowKeyHelper.BID, securityID), sendPriceView);
                        }
                        return;
                    }
                    //商品期货报价
                    if (CommonUtils.GetFutureType(securityID, marketIndicator) == 2)
                    {
                        if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceCommodityWinKey(WindowKeyHelper.BID, securityID)))
                        {
                            SendPriceCommodityViewLite sendPriceView = new SendPriceCommodityViewLite(securityID, true);
                            WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceCommodityWinKey(WindowKeyHelper.BID, securityID), sendPriceView);
                        }
                        return;
                    }
                    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, securityID)))
                    {
                        Task.Factory.StartNew(() =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                SendPriceView sendPriceView = new SendPriceView(securityID, true);
                                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, securityID), sendPriceView);
                            });
                        });
                    }
                }
            }
        }

        private void Ofr_OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRowView = null;
            if (FavoriteBondslistBox.Visibility == Visibility.Visible)
            {
                dataRowView = FavoriteBondslistBox.SelectedItem as DataRowView;
            }
            else
            {
                dataRowView = FavoriteBondsDg.SelectedItem as DataRowView;
            }
            if (dataRowView != null)
            {
                string securityID = dataRowView["securityID"] as string;
                BondsInfoPo bond = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                string marketIndicator = bond != null ? bond.MarketIndicator : Consts.MARKETINDICATOR_BOND;
                if (string.IsNullOrEmpty(securityID))
                    securityID = _TRADEBOOK;

                if (!string.IsNullOrEmpty(securityID))
                {
                    if (CommonUtils.IsTreasuryFuture(securityID))
                    {
                        if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceFutureWinKey(WindowKeyHelper.OFR, securityID)))
                        {
                            SendPriceFutureViewLite sendPriceView = new SendPriceFutureViewLite(securityID, false);
                            WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceFutureWinKey(WindowKeyHelper.OFR, securityID), sendPriceView);
                        }
                        return;
                    }
                    //商品期货报价
                    if (CommonUtils.GetFutureType(securityID, marketIndicator) == 2)
                    {
                        if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceCommodityWinKey(WindowKeyHelper.OFR, securityID)))
                        {
                            SendPriceCommodityViewLite sendPriceView = new SendPriceCommodityViewLite(securityID, false);
                            WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceCommodityWinKey(WindowKeyHelper.OFR, securityID), sendPriceView);
                        }
                        return;
                    }
                    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.OFR, securityID)))
                    {
                        Task.Factory.StartNew(() =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                SendPriceView sendPriceView = new SendPriceView(securityID, !true);

                                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, securityID), sendPriceView);
                            });
                        });
                        //sendPriceView.Owner = Application.Current.MainWindow;
                        //sendPriceView.Show();
                    }
                }
            }
        }


        private void btnKline_Click(object sender, RoutedEventArgs e)
        {
            // 选中Item
            var item = this.FavoriteBondslistBox.ItemContainerGenerator.ContainerFromItem((sender as Button).DataContext as DataRowView);
            ListBoxItem selectItem = item as ListBoxItem;
            if (selectItem != null)
            {
                selectItem.IsSelected = true;

                DataRowView dataRowView = selectItem.DataContext as DataRowView;
                if (dataRowView != null)
                {
                    string securityID = dataRowView["securityID"] as string;


                    string url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/KLine/KLineView.html?timestamp={new Random(10000).NextDouble()}";
                    //string url = $"/STC.Chrome.Plugin;component/Html/KLine/KLineView.html?timestamp={new Random(10000).NextDouble()}";
                    Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = true, Url = url, IsShowAddressBar = true, IsShowStatusBar = false };
                    cw.Key = "JsC#KLineDemo-" + securityID;
                    cw.Title = "K线分析";
                    cw.UseChromePluginObject = true;
                    cw.SecurityId = securityID;
                    cw.CallBackObjectInstance.Add("cefJS", "KLineViewModel");
                    //下面采用异步方式，前端Js也是异步调用的，因而在key后面加上| 告诉插件要异步注册到chrome中
                    //cw.CallBackObjectInstance.Add("csharpTest|", "JavascriptCallCharpTest");
                    AppUtils.Application.PluginManager.Application.BroadCast("OpenKLine", cw);
                }
            }
        }

        private void FavoriteBondslistBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            GridViewColumnHeader dgch = CommonUtils.GetParentObject<GridViewColumnHeader>(obj);
            if (dgch != null)
            {
                dgch.ContextMenu = (ContextMenu)this.FindResource("settingColumnContextMenu");
            }
        }

        private void settingColumn_Click(object sender, RoutedEventArgs e)
        {
            //if (WindowManager.Instance.IsExist("ColumnSetting") == false)
            //{
            //    ColumnSettingView csv = new ColumnSettingView(this);
            //    //Window parentWindow = CommonUtils.GetParentObject<Window>(this);
            //    WindowManager.Instance.OpenWindow("ColumnSetting", csv);
            //}
        }

        #region 表头设置
        private string _defaultColumnsInfoStr_NoRight = "+:45;债券代码:80;债券简称:80;剩余期限:120;类型:52;最优Bid (%)    :170;    最优Ofr (%):170;最新成交价:200;中债估值:140";
        private string _defaultColumnsInfoStr = "+:45;债券代码:80;债券简称:80;剩余期限:120;类型:52;MyBid:104;最优Bid (%)    :170;    最优Ofr (%):170;MyOfr:104;最新成交价:200;中债估值:140";
        List<string> fixedDisplyColumnList = new List<string>() { "+", "债券代码", "债券简称" };
        public ObservableCollection<ColumnsInfo> GetColumnsInfos()
        {
            ObservableCollection<ColumnsInfo> columnsInfoList = new ObservableCollection<ColumnsInfo>();
            if (this.FavoriteBondslistBox.View is GridView)
            {
                GridView gv = this.FavoriteBondslistBox.View as GridView;
                if (gv == null)
                    return columnsInfoList;
                foreach (GridViewColumn column in gv.Columns)
                {
                    string columnName = "+";
                    if (column.Header != null)
                    {
                        columnName = column.Header.ToString();
                    }

                    if (_FavoriteBondsListBoxViewModel.NoRightWidth == 0
                        && (columnName == "MyBid" || columnName == "MyOfr"))
                    {
                        continue;
                    }

                    ColumnsInfo ci = new ColumnsInfo();
                    ci.IsVisible = column.Width > 0 ? true : false;

                    if (fixedDisplyColumnList.Contains(columnName))
                    {
                        ci.IsEnable = false;
                    }
                    ci.ColumnName = columnName;
                    
                    if (column.ActualWidth > 0)
                    {
                        ci.ColumnWidth = column.ActualWidth;
                    }
                    else
                    {
                        switch(columnName)
                        {
                            case "剩余期限":
                                ci.ColumnWidth = 120;
                                break;
                            case "类型":
                                ci.ColumnWidth = 52;
                                break;
                            case "MyBid":
                                ci.ColumnWidth = 104;
                                break;
                            case "最优Bid (%)    ":
                                ci.ColumnWidth = 170;
                                break;
                            case "    最优Ofr (%)":
                                ci.ColumnWidth = 170;
                                break;
                            case "MyOfr":
                                ci.ColumnWidth = 104;
                                break;
                            case "最新成交价":
                                ci.ColumnWidth = 200;
                                break;
                            case "中债估值":
                                ci.ColumnWidth = 140;
                                break;
                        }
                    }
                       
                   // ci.DisplayIndex = column.DisplayIndex;
                    columnsInfoList.Add(ci);
                }
               
            }
            return columnsInfoList;
        }

        public void SaveColumnInfo(ObservableCollection<ColumnsInfo> _MyColumnsInfo)
        {
            if (_MyColumnsInfo == null && _MyColumnsInfo.Count == 0)
                return;

            string key = $"{this.GetName()}-{LocalEnv.UserInfo.UserName}";
           // await Task.Run(() => {
                #region 保存表头
                StringBuilder columnsInfoSB = new StringBuilder();
                foreach (ColumnsInfo cloumnsInfo in _MyColumnsInfo)
                {
                    if (columnsInfoSB.Length > 0)
                        columnsInfoSB.Append(";");
                    if (cloumnsInfo.IsVisible)
                    {
                        columnsInfoSB.Append($"{cloumnsInfo.ColumnName}:{cloumnsInfo.ColumnWidth}");
                    }
                    else
                    {
                        columnsInfoSB.Append($"({cloumnsInfo.ColumnName}):{cloumnsInfo.ColumnWidth}");
                    }
                }
                IniFileHelper.WriteValue(key, ColumnSettingView.IniKey, columnsInfoSB.ToString());
                #endregion
           // });
        }

        public void InitColumnsInfo(string columnsInfoStr = null)
        {
            if (!string.IsNullOrEmpty(columnsInfoStr))
            {
                GridView gv = this.FavoriteBondslistBox.View as GridView;
                if (gv == null)
                    return;
                foreach (GridViewColumn column in gv.Columns)
                {
                    string headerName = column.Header == null ? "+" : column.Header.ToString();
                    if (!fixedDisplyColumnList.Contains(headerName))
                    {
                        column.Width = 0;
                    }
                }

                string[] columnPairs = columnsInfoStr.Split(CommonConstants.SEMICOLON_CHAR);
                int index = 0;
                foreach (string columnPair in columnPairs)
                {
                    string[] propertyValues = columnPair.Split(CommonConstants.COLON_CHAR);
                    if (propertyValues.Length == 2)
                    {
                        string columnName = propertyValues[0];
                        if (propertyValues[0].StartsWith("(") && propertyValues[0].EndsWith(")"))
                        {
                            columnName = columnName.TrimStart('(').TrimEnd(')');
                        }

                        if (!gv.Columns.Any(o => { string headerName = o.Header == null ? string.Empty : o.Header.ToString(); return headerName == "+"; }))
                            columnName = columnName == "+" ? string.Empty : columnName;

                        try
                        {
                            GridViewColumn col = gv.Columns.First(o => { string headerName = o.Header == null ? string.Empty : o.Header.ToString(); return headerName == columnName; });
                            //if (!fixedDisplyColumnList.Contains(columnName) && !(propertyValues[0].StartsWith("(") && propertyValues[0].EndsWith(")")))
                            //{
                            //    col.Visibility = Visibility.Visible;
                            //}
                            double width = col.ActualWidth;
                            double.TryParse(propertyValues[1], out width);
                            
                            if (!(propertyValues[0].StartsWith("(") && propertyValues[0].EndsWith(")")))
                            {
                                col.Width = width;
                            }
                            //col.DisplayIndex = index;
                            index++;
                        }
                        catch (System.InvalidOperationException e)
                        {
                            // ignore
                        }
                    }
                }
            }
        }

        public string GetName()
        {
            return this.FavoriteBondslistBox.Name;
        }

        public void DeleteColumnInfo()
        {
            //await Task.Run(() =>
            //{
                string key = $"{this.GetName()}-{LocalEnv.UserInfo.UserName}";
                IniFileHelper.WriteValue(key, null, null);

                string columnsInfoStr = GetColumnsInfoStr();
                InitColumnsInfo(columnsInfoStr);
            //});
        }


        private string GetColumnsInfoStr()
        {
            string columnsInfoStr = IniFileHelper.ReadValue($"{this.GetName()}-{LocalEnv.UserInfo.UserName}", ColumnSettingView.IniKey);
            if (string.IsNullOrEmpty(columnsInfoStr))
            {
                columnsInfoStr = this.GetDefaultColumnsInfoStr();
            }
            return columnsInfoStr;
        }

        public string GetDefaultColumnsInfoStr()
        {
            string columnsInfoStr = IniFileHelper.ReadValue($"{this.GetName()}-{LocalEnv.UserInfo.UserName}", ColumnSettingView.IniKey);
            if (string.IsNullOrEmpty(columnsInfoStr))
            {
                if (_FavoriteBondsListBoxViewModel.NoRightWidth > 0)
                {
                    columnsInfoStr = _defaultColumnsInfoStr;
                }
                else
                {
                    columnsInfoStr = _defaultColumnsInfoStr_NoRight;
                }
            }
            return columnsInfoStr;
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //SettingSyncHandler.Instance.AddSyncDg(this);
            //InitColumnsInfo(this.GetColumnsInfoStr());
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //SettingSyncHandler.Instance.RemoveSyncDg(this);
        }

        private void SendQM_Click(object sender, RoutedEventArgs e)
        {
            TradeRequestView f = new TradeRequestView();
            f.Owner = Application.Current.MainWindow;
            f.Show();
        }

        private void CopySecurityIDMenu_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = FavoriteBondsDg.SelectedItem as DataRowView;
            if (drv != null)
            {
                string securityId = drv[FavoriteBondsListBoxViewModel.SECURITY_ID_FIELD].ConvertToString();
                ClipboardHelper.CopyToClipboard(securityId, securityId);
            }
        }

        private void CopySymbolNameMenu_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = FavoriteBondsDg.SelectedItem as DataRowView;
            if (drv != null)
            {
                string securityId = drv[FavoriteBondsListBoxViewModel.SYMBOL_FIELD].ConvertToString();
                ClipboardHelper.CopyToClipboard(securityId, securityId);
            }
        }

        private void CopySingleClickMenu_Click(object sender, RoutedEventArgs e)
        {
            CopySingleClick();
        }

        private void CopyDoubleClick_Click(object sender, RoutedEventArgs e)
        {
            if (FavoriteBondsDg.CurrentColumn.Header.ToString() == "    最优Ofr (%)")
            {
                CopyDoubleClick("2");
            }
            else if (FavoriteBondsDg.CurrentColumn.Header.ToString() == "最优Bid (%)    ")
            {
                CopyDoubleClick("1");
            }
        }

        private void CopyCtrlSingleClickMenu_Click(object sender, RoutedEventArgs e)
        {
            if (FavoriteBondsDg.CurrentColumn.Header.ToString() == "    最优Ofr (%)")
            {
                CopySingleClick("2");
            }
            else if (FavoriteBondsDg.CurrentColumn.Header.ToString() == "最优Bid (%)    ")
            {
                CopySingleClick("1");
            }
        }

        private void CopySingleClick(string side = null)
        {
            string html;
            string plainTxt;
            string html_double;
            string plainTxt_double;
            GetDoubleText(out html_double, out plainTxt_double);
            string securityId = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.SECURITY_ID_FIELD].ToString();
            string bidPx = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.BID_BBO_FIELD].ToString();
            string ofrPx = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.OFR_BBO_FIELD].ToString();

            string info;
            if (side == "1")
            {
                string s = "GVN";
                info = $",    {s} {securityId} {bidPx}";

                html = $"<p>[{html_double}]{info}</p>";

                plainTxt = $"[{plainTxt_double}]{info}";
            }
            else if (side == "2")
            {
                string s = "TKN";
                info = $",    {s} {securityId} {ofrPx}";

                html = $"<p>[{html_double}]{info}</p>";

                plainTxt = $"[{plainTxt_double}]{info}";
            }
            else
            {
                html = $"<p>{html_double}</p>";

                plainTxt = $"{plainTxt_double}";
            }


            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            if (!string.IsNullOrEmpty(side))
            {
                if (plainTxt.Length > 0)
                    CommonUtils.Prompt2Second("复制", "复制成功");
            }
        }

        private void GetDoubleText(out string html, out string plainTxt)
        {
            string securityId = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.SECURITY_ID_FIELD].ToString();
            string symbolName = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.SYMBOL_FIELD].ToString();
            string remainingTime = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.REMAINGING_TIME_FIELD].ToString();


            string bidPx = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.BID_BBO_FIELD].ToString();
            string bidQtyAndSettle = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.BID_TXT_FIELD].ToString();

            string ofrPx = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.OFR_BBO_FIELD].ToString();
            string ofrQtyAndSettle = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.OFR_TXT_FIELD].ToString();

            string cdcValue = CommonConstants.CDCValueTxt + _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.VALUATION_FIELD].ToString();

            //if (bidQty.EndsWith("K"))
            //{
            //    bidQty = bidQty.Replace("K", "000");
            //}

            //if (ofrQty.EndsWith("K"))
            //{
            //    ofrQty = ofrQty.Replace("K", "000");
            //}

            html = $"{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQtyAndSettle}/{ofrQtyAndSettle}&#9;{cdcValue}";
            plainTxt = $"{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQtyAndSettle}/{ofrQtyAndSettle}    {cdcValue}";
        }

        private void CopyDoubleClick(string side)
        {
            string html;
            string plainTxt;
            string html_double;
            string plainTxt_double;
            GetDoubleText(out html_double, out plainTxt_double);

            string securityId = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.SECURITY_ID_FIELD].ToString();
            string symbolName = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.SYMBOL_FIELD].ToString();

            string s = string.Empty;
            string qty = string.Empty;
            string px = string.Empty;
            if (side == "1")//GVN
            {
                s = "GVN";
                qty = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.BID_MAX_T1_QTY_FIELD].ToString();
                px = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.BID_BBO_FIELD].ToString();
            }
            else if (side == "2")//TKN
            {
                s = "TKN";
                qty = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.OFR_MAX_T1_QTY_FIELD].ToString();
                px = _FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.OFR_BBO_FIELD].ToString();
            }

            string htmlPx = string.Empty;
            string txtPx = string.Empty;
            if (px.Length > 0)
            {
                htmlPx = $"&#9;{px}";
                txtPx = $" {px}";
            }

            string htmlQty = string.Empty;
            string txtQty = string.Empty;
            if (qty.Length > 0)
            {
                htmlQty = $"&#9;{qty}w";
                txtQty = $" {qty}w";
            }

            html = $"<p>[{html_double}],<br>{s}&#9;{securityId}&#9;{symbolName}{htmlPx}{htmlQty}</p>";

            plainTxt = $"[{plainTxt_double}],\r\n{s} {securityId} {symbolName}{txtPx}{txtQty}";

            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            if (plainTxt.Length > 0)
                CommonUtils.Prompt2Second("复制", "复制成功");
        }

        private void FavoriteBondsDg_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject cell = e.OriginalSource as DependencyObject;
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            if (targetCell != null)
            {
                this.FavoriteBondsDg.CurrentColumn = targetCell.Column;

                if (this.FavoriteBondsDg.SelectedItems.Count == 0)
                {
                    DataGridRow targetRow = this.FavoriteBondsDg.ItemContainerGenerator.ContainerFromItem(targetCell.DataContext) as DataGridRow;

                    if (targetRow != null)
                    {
                        targetRow.IsSelected = true;
                    }
                }
            }
        }

        private void dgMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (this.FavoriteBondsDg.SelectedItems.Count > 1)
            {
                JoinMenu.Visibility = Visibility.Collapsed;
                ReplyPriceMenu.Visibility = Visibility.Collapsed;
                PointPriceMenu.Visibility = Visibility.Collapsed;
                ConsiderationMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (isOfrColumn())
                {
                    string myOfrQty = this._FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.MY_OFR_QTY_FIELD].ConvertToString();
                    string myOfr = this._FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.MY_OFR_QTY_FIELD].ConvertToString();
                    string bestOfr = this._FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.BEST_OFR_PRICE_FIELD].ConvertToString();
                    string bestOfrQty = this._FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.BEST_OFR_QTY_FIELD].ConvertToString();
                    if (!string.IsNullOrEmpty(myOfr))
                    {
                        if (myOfr == bestOfr && myOfrQty == bestOfrQty)
                        {
                            ReplyPriceMenu.Visibility = Visibility.Collapsed;
                            PointPriceMenu.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        ConsiderationMenu.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    string myBidQty = this._FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.MY_BID_QTY_FIELD].ConvertToString();
                    string myBid = this._FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.MY_BID_QTY_FIELD].ConvertToString();
                    string bestBid = this._FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.BEST_BID_PRICE_FIELD].ConvertToString();
                    string bestBidQty = this._FavoriteBondsListBoxViewModel.SelectedItem[FavoriteBondsListBoxViewModel.BEST_BID_QTY_FIELD].ConvertToString();
                    if (!string.IsNullOrEmpty(myBid))
                    {
                        if (myBid == bestBid && myBidQty == bestBidQty)
                        {
                            ReplyPriceMenu.Visibility = Visibility.Collapsed;
                            PointPriceMenu.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        ConsiderationMenu.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (this.FavoriteBondsDg.CurrentColumn != null
                && (this.FavoriteBondsDg.CurrentColumn.Header.ToString() == "    最优Ofr (%)"
                || this.FavoriteBondsDg.CurrentColumn.Header.ToString() == "最优Bid (%)    "))
            {
                CopyDoubleClickMenu.Visibility = Visibility.Visible;
                CopyCtrlSingleClickMenu.Visibility = Visibility.Visible;
            }
            else
            {
                CopyDoubleClickMenu.Visibility = Visibility.Collapsed;
                CopyCtrlSingleClickMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void JoinMenu_Click(object sender, RoutedEventArgs e)
        {
            if (isOfrColumn())
            {
                Ofr_OrderBtn_Click(null, null);
            }
            else
            {
                Bid_OrderBtn_Click(null, null);
            }
        }

        private void PointPriceMenu_Click(object sender, RoutedEventArgs e)//内部别人的能点
        {
            if (isOfrColumn())
            {
                OrderBest_Ofr(false);
            }
            else
            {
                OrderBest_Bid(false);
            }
        }

        private void ReplyPriceMenu_Click(object sender, RoutedEventArgs e)// 内部别人的才回, 
        {
            if (isOfrColumn())
            {
                OrderBest_Ofr(true);
            }
            else
            {
                OrderBest_Bid(true);
            }
        }

        private void ConsiderationMenu_Click(object sender, RoutedEventArgs e)//只要有值
        {

        }

        private bool isOfrColumn()
        {
            return FavoriteBondsDg.CurrentColumn != null
               && (FavoriteBondsDg.CurrentColumn.Header.ToString() == "MyOfr"
               || FavoriteBondsDg.CurrentColumn.Header.ToString() == "    最优Ofr (%)");
        }

        private bool isBidColumn()
        {
            return this.FavoriteBondsDg.CurrentColumn != null
                 && (FavoriteBondsDg.CurrentColumn.Header.ToString() == "MyBid"
                 || FavoriteBondsDg.CurrentColumn.Header.ToString() == "最优Bid (%)    ");
        }

        private void OrderBest_Bid(bool isDrp)
        {
            //InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;
            //if (innerOrderData.Count_InnerBid > 0)
            //{
            //    List<InnerOrderData> bidList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "1", _innerBoardViewModel.bidPriceComparer);
            //    if (bidList.Count > 0)
            //    {
            //        Order(bidList[0], isDrp);
            //    }
            //}
        }

        private void OrderBest_Ofr(bool isDrp)
        {
            //InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;

            //if (innerOrderData.Count_InnerOfr > 0)
            //{
            //    List<InnerOrderData> ofrList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "2", _innerBoardViewModel.ofrPriceComparer);
            //    if (ofrList.Count > 0)
            //    {
            //        Order(ofrList[0], isDrp);
            //    }
            //}
        }

        private void Order(DataView innerOrderData, bool isDrp)
        {

                //if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetPriceMatchWinKey(innerOrderData.OrderID)))
                //{
                //    SendPointPriceView sendPointPriceView = new SendPointPriceView(innerOrderData.SecurityID, innerOrderData.Side == "1" ? true : false, innerOrderData.Price.ToString(), innerOrderData.Qty.ToString(),
                //    innerOrderData.SettleType, innerOrderData.NetPrice.ToString(), "MANUAL",
                //    innerOrderData.WholeFlag, innerOrderData.FirmFlag == "1", innerOrderData.QuoteId,
                //    null, null, null, null,
                //    null, string.Empty, "INNER", null, innerOrderData.IntentionID, isDrp, $"{innerOrderData.RoleName}-{innerOrderData.PartySubName}", null, null, null, null, new Common.ViewModel.Order.DRPParamInfoModel() { OrderId = innerOrderData.OrderID });


                //    WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetExecBackWinKey(innerOrderData.OrderID), sendPointPriceView);
                //}
           
        }
    }
}
