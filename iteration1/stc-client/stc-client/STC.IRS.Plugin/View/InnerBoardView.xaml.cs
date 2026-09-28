using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Order;
using STC.IRS.Plugin.View.Component;
using STC.Common.View.Order.InnerPanel;
using STC.IRS.Plugin.ViewModel;
using STC.IRS.Plugin.ViewModel.Data;
using STC.InnerBoard.ViewModel.Data;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using STC.Common.View.Component;
using STC.Common.DataProcesser;
using System.Data;
using STC.Common.Model.TBook;
using System.Timers;
using STC.Common.Model.Table;
using STC.Common.Handler.Table;
using STC.Common.Handler.SystemParam;
using System.ComponentModel;
using STC.Common.Handler.Admin;
using STC.Common.ViewModel.Data;

namespace STC.IRS.Plugin.View
{
    /// <summary>
    /// InnerBoardView.xaml 的交互逻辑
    /// </summary>
    public partial class InnerBoardView : UserControl
    {
        private string _TRADEBOOK = "Tradebook";
        private InnerBoardViewModel _innerBoardViewModel;

        private string _oldSecuityID = "Tradebook";

        public InnerBoardView()
        {
            InitializeComponent();
            this.InnerBoard_dataGrid_IRS.FixedDisplayColumns = "";
            this.InnerBoard_Best_dataGrid_IRS.FixedDisplayColumns = "";

            //if (Timer4ControlDetailPopup == null)
            //{
            //    this.Timer4ControlDetailPopup = new DispatcherTimer();
            //    this.Timer4ControlDetailPopup.Interval = new TimeSpan(500);
            //    this.Timer4ControlDetailPopup.Tick += ControlPopup;
            //}
            if (Timer4Refresh == null)
            {
                Timer4Refresh = new DispatcherTimer();
                Timer4Refresh.Interval = new TimeSpan(0, 0, 0, 0, 300);
                Timer4Refresh.Tick += Timer4Refresh_Tick;
                Timer4Refresh.Start();
            }


            dgPrice.RemoveDbDoubleClickEvent();
            dgPrice.Init(_oldSecuityID);
            this.InnerBoard_dataGrid_IRS.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.InnerBoard_dataGrid_MouseLeftButtonDown), true);
            this.InnerBoard_Best_dataGrid_IRS.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.InnerBoard_Beat_dataGrid_MouseLeftButtonDown), true);

            MenuItem sortTermMi = new MenuItem();
            sortTermMi.Name = "sortTermColumnMi";
            sortTermMi.Header = "按期限排序";
            sortTermMi.Style = (Style)this.FindResource("DefaultMenuItem");
            sortTermMi.Click += sortTermColumnMi_Click;
            this.InnerBoard_dataGrid_IRS.GetHeaderContextMenu().Items.Add(sortTermMi);

            MenuItem sortUpdateTimeMi = new MenuItem();
            sortUpdateTimeMi.Name = "sortUpdateTimeColumnMi";
            sortUpdateTimeMi.Header = "按最后更新时间排序";
            sortUpdateTimeMi.Style = (Style)this.FindResource("DefaultMenuItem");
            sortUpdateTimeMi.Click += sortUpdateTimeColumnMi_Click;
            this.InnerBoard_dataGrid_IRS.GetHeaderContextMenu().Items.Add(sortUpdateTimeMi);


            MenuItem sortTermColumnMi_Best = new MenuItem();
            sortTermColumnMi_Best.Name = "sortUpdateTimeColumnMi_Best";
            sortTermColumnMi_Best.Header = "按期限排序";
            sortTermColumnMi_Best.Style = (Style)this.FindResource("DefaultMenuItem");
            sortTermColumnMi_Best.Click += sortTermColumnMi_Best_Click;
            this.InnerBoard_Best_dataGrid_IRS.GetHeaderContextMenu().Items.Add(sortTermColumnMi_Best);

            MenuItem sortUpdateTimeColumnMi_Best = new MenuItem();
            sortUpdateTimeColumnMi_Best.Name = "sortUpdateTimeColumnMi_Best";
            sortUpdateTimeColumnMi_Best.Header = "按最后更新时间排序";
            sortUpdateTimeColumnMi_Best.Style = (Style)this.FindResource("DefaultMenuItem");
            sortUpdateTimeColumnMi_Best.Click += sortUpdateTimeColumnMi_Best_Click;
            this.InnerBoard_Best_dataGrid_IRS.GetHeaderContextMenu().Items.Add(sortUpdateTimeColumnMi_Best);
        }

        private void sortTermColumnMi_Best_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_Best_dataGrid_IRS.Items.SortDescriptions.Clear();
            this.InnerBoard_Best_dataGrid_IRS.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Term_Convert", System.ComponentModel.ListSortDirection.Ascending));
        }

        private void sortTermColumnMi_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_dataGrid_IRS.Items.SortDescriptions.Clear();
            this.InnerBoard_dataGrid_IRS.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Term_Convert", System.ComponentModel.ListSortDirection.Ascending));
        }

        private void sortUpdateTimeColumnMi_Best_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_Best_dataGrid_IRS.Items.SortDescriptions.Clear();
            this.InnerBoard_Best_dataGrid_IRS.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("UpdateTime", System.ComponentModel.ListSortDirection.Descending));
        }

        private void sortUpdateTimeColumnMi_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_dataGrid_IRS.Items.SortDescriptions.Clear();
            this.InnerBoard_dataGrid_IRS.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("UpdateTime", System.ComponentModel.ListSortDirection.Descending));
        }

        private void InnerBoard_dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders != null)
            {
                _innerBoardViewModel.OpenTradeBook(_innerBoardViewModel.SelectedItem_MyInnerOrders);
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);
            if (!(win is Common.View.UI.BorderWin))
            {
                this._innerBoardViewModel.Dispose();
            }
        }

        private void Bid_OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            string securityID = _TRADEBOOK;
            if (_innerBoardViewModel != null
                && _innerBoardViewModel.SelectedItem_MyInnerOrders != null
                && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID))
            {
                securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID;
            }

            OpenOrderWin(securityID, ComWindowKeyHelper.BID);
        }

        /// <summary>
        /// 打开报价界面
        /// </summary>
        /// <param name="security">债券</param>
        /// <param name="side">方向</param>
        private void OpenOrderWin(string securityID, string side)
        {
            //国债期货报价
            if (CommonUtils.IsTreasuryFuture(securityID))
            {
                if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceFutureWinKey(side, securityID)))
                {
                    SendPriceFutureViewLite sendPriceView = new SendPriceFutureViewLite(securityID, side == ComWindowKeyHelper.OFR ? false : true);
                    WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceFutureWinKey(side, securityID), sendPriceView);
                }
                return;
            }

            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityID)))
            {
                //SendPriceViewLite sendPriceView = new SendPriceViewLite(securityID, true);
                SendPriceView sendPriceView = new SendPriceView(securityID, side == ComWindowKeyHelper.OFR ? false : true);

                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceWinKey(side, securityID), sendPriceView);
            }
        }

        private void Ofr_OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            string securityID = _TRADEBOOK;
            if (_innerBoardViewModel != null
                && _innerBoardViewModel.SelectedItem_MyInnerOrders != null
                && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID))
            {
                securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID;
            }

            OpenOrderWin(securityID, ComWindowKeyHelper.OFR);
        }

        private void Bid_OrderBtn_Best_Click(object sender, RoutedEventArgs e)
        {
            string securityID = _TRADEBOOK;
            if (_innerBoardViewModel != null
                && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null
                && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID))
            {
                securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
            }

            OpenOrderWin(securityID, ComWindowKeyHelper.BID);
        }

        private void Ofr_OrderBtn_Best_Click(object sender, RoutedEventArgs e)
        {
            string securityID = _TRADEBOOK;
            if (_innerBoardViewModel != null
                && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null
                && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID))
            {
                securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
            }

            OpenOrderWin(securityID, ComWindowKeyHelper.OFR);
        }

        private void InnerBoard_dataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 1)
                return;

            if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders != null)
            {
                MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders);
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (this.InnerBoard_dataGrid_IRS.SelectedItems.Count > 0)
                {
                    this.InnerBoard_dataGrid_IRS.CopyData_Single();
                }
            }
        }

        private void MyOrderDefaultView_SelectedRowChanged(ViewModel.Data.InnerOrderData obj)
        {
            string securityObj = string.Empty;
            if (obj != null)
            {
                securityObj = obj.SecurityID;
                if (!string.IsNullOrEmpty(securityObj) && _oldSecuityID != securityObj)
                {
                    dgPrice.Init(securityObj);
                    _oldSecuityID = securityObj;
                }
            }
            else
            {
                dgPrice.Init(securityObj);
                _oldSecuityID = securityObj;
            }
            dgPrice.Init(securityObj, null, false);
            _oldSecuityID = securityObj;
        }

        private void InnerBoard_Best_dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
            {
                _innerBoardViewModel.OpenTradeBook(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best);
            }
        }

        private bool isDataGridRow(ItemsControl itemsControl, Point point)
        {
            UIElement element = itemsControl.InputHitTest(point) as UIElement;
            while (element != null)
            {
                if (element == itemsControl)
                    return false;
                if (element is DataGridRow)
                    return true;
                element = (UIElement)System.Windows.Media.VisualTreeHelper.GetParent(element);
            }
            return false;
        }

        int i = 0;
        bool isDouble = false;
        private void InnerBoard_Beat_dataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            i += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i = 0; UIElement_Click(sender, e); isDouble = false; };
            timer.IsEnabled = true;
            if (i % 2 == 0)
            {
                timer.IsEnabled = false;
                i = 0;
                isDouble = true;

                //双击时执行的代码
                //MessageBox.Show("DoubleClick!");

                this.InnerDepthPopup.IsOpen = false;
                if (isBidColumn())
                {
                    InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;
                    if (innerOrderData.Count_InnerBid > 0)
                    {
                        List<InnerOrderData> bidList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "1", _innerBoardViewModel.ofrPriceComparer);
                        if (bidList.Count > 0)
                        {
                            bool isDrp = !bidList[0].Price.HasValue || !bidList[0].Qty.HasValue;
                            Order(bidList[0], isDrp);
                        }
                    }
                    else
                    {
                        string securityID = _TRADEBOOK;
                        if (_innerBoardViewModel != null
                            && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null
                            && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID))
                        {
                            securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
                        }

                        SendIRSView sendRequireView = new SendIRSView(securityID, true);

                        sendRequireView.Show();
                    }
                }
                if (isOfrColumn())
                {
                    InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;

                    if (innerOrderData.Count_InnerOfr > 0)
                    {
                        List<InnerOrderData> ofrList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "2", _innerBoardViewModel.bidPriceComparer);
                        if (ofrList.Count > 0)
                        {
                            bool isDrp = !ofrList[0].Price.HasValue || !ofrList[0].Qty.HasValue;
                            Order(ofrList[0], isDrp);
                        }
                    }
                    else
                    {
                        string securityID = _TRADEBOOK;
                        if (_innerBoardViewModel != null
                            && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null
                            && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID))
                        {
                            securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
                        }

                        SendIRSView sendRequireView = new SendIRSView(securityID, false);

                        sendRequireView.Show();
                    }
                }
            }
        }

        private void UIElement_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble)
                return;

            if (isBestDataGridSelected)
            {
                DataGrid dg = sender as DataGrid;
                InnerOrderData innerOrderData = null;
                if (e.OriginalSource is FrameworkElement)
                {
                    innerOrderData = (e.OriginalSource as FrameworkElement).DataContext as InnerOrderData;
                }
                else if (e.OriginalSource is FrameworkContentElement)
                {
                    innerOrderData = (e.OriginalSource as FrameworkContentElement).DataContext as InnerOrderData;
                }

                if (innerOrderData != null)
                {
                    DataGridRow targetRow = dg.ItemContainerGenerator.ContainerFromItem(innerOrderData) as DataGridRow;

                    if (targetRow != null)
                    {
                        targetRow.IsSelected = false;
                    }
                }
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 0)
                {
                    this.InnerBoard_Best_dataGrid_IRS.CopyData_Best();
                }
            }

            if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
            {
                MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best);
            

                bool flag = isDataGridRow(this.InnerBoard_Best_dataGrid_IRS, e.GetPosition(this.InnerBoard_Best_dataGrid_IRS));
                if (flag && this.InnerBoard_Best_dataGrid_IRS.CurrentColumn != null
                    && this.InnerBoard_Best_dataGrid_IRS.CurrentColumn.GetCellContent(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best) != null
                    && (this.InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "Bid"
                    || this.InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "Ofr"))
                {
                    DataGridCell c = this.InnerBoard_Best_dataGrid_IRS.CurrentColumn.GetCellContent(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best).Parent as DataGridCell;
                    placementTargetObject = c;

                    this.InnerDepthPopup.IsOpen = false;
                    this.InnerDepthPopup.PlacementTarget = placementTargetObject;
                    this.InnerDepthPopup.Placement = PlacementMode.Bottom;
                    this.InnerDepthPopup.HorizontalOffset = 0;

                    InnerOrderData currentItem = c.DataContext as InnerOrderData;

                    if (currentItem != null && this.InnerDepthPopup != null && this.InnerDepthPopup.Child != null && this.InnerDepthPopup.Child is InnerDepthPopupView)
                    {
                        InnerDepthPopupView InnerDepthPopupView = this.InnerDepthPopup.Child as InnerDepthPopupView;
                        if (InnerDepthPopupView != null && InnerDepthPopupView.DataContext is InnerDepthPopupViewModel)
                        {
                            InnerDepthPopupViewModel viewModel = InnerDepthPopupView.DataContext as InnerDepthPopupViewModel;
                            viewModel.SecurityID = currentItem.SecurityID;
                            viewModel.Symbol = currentItem.Symbol;
                            viewModel.FilterData();
                        }
                    }

                    this.InnerDepthPopup.IsOpen = true;
                }
                    //else
                    //{
                    //    InnerBoard_Best_dataGrid_LostFocus(null, null);
                    //}
            }
            else
            {
                MyOrderDefaultView_SelectedRowChanged(null);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool flag = false;
            if (e.Source is TabControl)
            {
                TabControl tc = e.Source as TabControl;
                if (tc.Name == "InnerBoard_IRS_CT")
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                return;
            }
            
            if (this.singleTab.IsSelected)
            {
                this.countBar.Visibility = Visibility.Visible;
                this.countBar_best.Visibility = Visibility.Collapsed;
                this.countBar_H.Visibility = Visibility.Collapsed;
                MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders);
            }
            else if (this.bestTab.IsSelected)
            {
                this.countBar.Visibility = Visibility.Collapsed;
                this.countBar_best.Visibility = Visibility.Visible;
                this.countBar_H.Visibility = Visibility.Collapsed;
                MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best);
            }
            //else
            //{
            //    this.countBar.Visibility = Visibility.Collapsed;
            //    this.countBar_best.Visibility = Visibility.Collapsed;
            //    this.countBar_H.Visibility = Visibility.Visible;
            //    (ihr.DataContext as InnerHistoryRequireViewModel).LoadMaskVisible = Visibility.Collapsed;
            //    MyOrderDefaultView_SelectedRowChanged((ihr.DataContext as InnerHistoryRequireViewModel).SelectOrder);
            //}
        }

        private void singleToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.singleToggleBtn.Style == FindResource("CollapsedButton_24"))
            {
                this.single_filter_part.Visibility = Visibility.Collapsed;
                this.singleToggleBtn.Style = (Style)FindResource("ExpandButton_24"); 
            }
            else if (this.singleToggleBtn.Style == FindResource("ExpandButton_24"))
            {
                this.single_filter_part.Visibility = Visibility.Visible;
                this.singleToggleBtn.Style = (Style)FindResource("CollapsedButton_24");
            }
        }

        private void singleToggleBtn_Best_Click(object sender, RoutedEventArgs e)
        {
            if (this.singleToggleBtn_Best.Style == FindResource("CollapsedButton_24"))
            {
                this.best_single_filter_part.Visibility = Visibility.Collapsed;
                this.singleToggleBtn_Best.Style = (Style)FindResource("ExpandButton_24");
            }
            else if (this.singleToggleBtn_Best.Style == FindResource("ExpandButton_24"))
            {
                this.best_single_filter_part.Visibility = Visibility.Visible;
                this.singleToggleBtn_Best.Style = (Style)FindResource("CollapsedButton_24");
            }
        }

        private void opensecurity_btn_Click(object sender, RoutedEventArgs e)
        {
            // 选中Item
            InnerOrderData innerOrderData = (sender as Button).DataContext as InnerOrderData;
            var item = this.InnerBoard_dataGrid_IRS.ItemContainerGenerator.ContainerFromItem(innerOrderData);
            (item as DataGridRow).IsSelected = true;
            Common.ViewModel.TradeBookHandler.TBookViewHandler.Instance.OpenQuotesMiniWindows(innerOrderData.SecurityID);
        }

        private void best_opensecurity_btn_Click(object sender, RoutedEventArgs e)
        {
            // 选中Item
            InnerOrderData innerOrderData = (sender as Button).DataContext as InnerOrderData;
            var item = this.InnerBoard_Best_dataGrid_IRS.ItemContainerGenerator.ContainerFromItem(innerOrderData);
            (item as DataGridRow).IsSelected = true;
            Common.ViewModel.TradeBookHandler.TBookViewHandler.Instance.OpenQuotesMiniWindows(innerOrderData.SecurityID);
        }

        private void ShowRecognizeWin_Click(object sender, RoutedEventArgs e)
        {
            DesignHandler.Instance.ShowIRSRecognizeIntentionQuoteWin(null);
        }


        private void SearchMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            SearchSecurityID_Best();
        }

        private void SearchSecurityID_Best()
        {
            if (_innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
            {
                _innerBoardViewModel.Best_SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
                this.Search_TextBox_Best.SetPop(false);
            }
            else
            {
                LogHandler.Info("IntentionPanel DeleteOrder 没有取到数据");
            }
        }

        private void CopyMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_Best_dataGrid_IRS.CopyData_Best();
        }

        private void CopyMenu_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_dataGrid_IRS.CopyData_Single();
        }

        private void Single_DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();

            if (columnHeader == "报价"
                || columnHeader == "报量(百万)"
                || columnHeader == "意向类型"
                || columnHeader == "清算速度")
            {
                bool isDrp = !this._innerBoardViewModel.SelectedItem_MyInnerOrders.Price.HasValue || !this._innerBoardViewModel.SelectedItem_MyInnerOrders.Qty.HasValue;
                Order(this._innerBoardViewModel.SelectedItem_MyInnerOrders, isDrp);
            }
            else if (columnHeader == "代码"
                || columnHeader == "简称")
            {
                if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders != null)
                {
                    _innerBoardViewModel.OpenTradeBook(_innerBoardViewModel.SelectedItem_MyInnerOrders);
                }
            }
        }

        private void InnerBest_DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();
            if (columnHeader == "代码"
               || columnHeader == "简称")
            {
                if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
                {
                    _innerBoardViewModel.OpenTradeBook(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best);
                }
            }
        }

        private void InnerBid_DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();

            if (isBidColumn())
            {
                InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;
                if (innerOrderData.Count_InnerBid > 0)
                {
                    List<InnerOrderData> bidList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "1", _innerBoardViewModel.ofrPriceComparer);
                    if (bidList.Count > 0)
                    {
                        bool isDrp = !bidList[0].Price.HasValue || !bidList[0].Qty.HasValue;
                        Order(bidList[0], isDrp);
                    }
                }
            }
        }

        private void InnerOfr_DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();

            if (isOfrColumn())
            {
                InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;

                if (innerOrderData.Count_InnerOfr > 0)
                {
                    List<InnerOrderData> ofrList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "2", _innerBoardViewModel.bidPriceComparer);
                    if (ofrList.Count > 0)
                    {
                        bool isDrp = !ofrList[0].Price.HasValue || !ofrList[0].Qty.HasValue;
                        Order(ofrList[0], isDrp);
                    }
                }
            }
        }

        #region InnerDepthPopup

        private void InnerDepthPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            //this.InnerDepthPopup.IsOpen = false;
            //CloseDepthPop();
        }


        private void InnerDepthPopup_Closed(object sender, EventArgs e)
        {
            this.placementTargetObject = null;
            this.InnerDepthPopup.PlacementTarget = null;
        }

        private DispatcherTimer Timer4ControlDetailPopup;
        private DataGridCell placementTargetObject = null;

        private Popup InnerDepthPopup;

        private void ControlPopup(object state, EventArgs e)
        {
            try
            {
                if (placementTargetObject != null)
                {
                    bool isChange = false;
                    if (this.InnerDepthPopup.PlacementTarget != placementTargetObject)
                    {
                        isChange = true;
                        this.InnerDepthPopup.IsOpen = false;
                        this.InnerDepthPopup.PlacementTarget = placementTargetObject;
                        this.InnerDepthPopup.Placement = PlacementMode.Bottom;
                        this.InnerDepthPopup.HorizontalOffset = 0;
                    }

                    if (isChange == true)
                    {
                        if (placementTargetObject is DataGridCell)
                        {
                            DataGridCell c = placementTargetObject as DataGridCell;
                            InnerOrderData currentItem = c.DataContext as InnerOrderData;

                            if (currentItem != null && this.InnerDepthPopup != null && this.InnerDepthPopup.Child != null && this.InnerDepthPopup.Child is InnerDepthPopupView)
                            {
                                InnerDepthPopupView InnerDepthPopupView = this.InnerDepthPopup.Child as InnerDepthPopupView;
                                if (InnerDepthPopupView != null && InnerDepthPopupView.DataContext is InnerDepthPopupViewModel)
                                {
                                    InnerDepthPopupViewModel viewModel = InnerDepthPopupView.DataContext as InnerDepthPopupViewModel;
                                    viewModel.SecurityID = currentItem.SecurityID;
                                    viewModel.Symbol = currentItem.Symbol;
                                    viewModel.FilterData();
                                }
                            }

                            if ((placementTargetObject.Column.Header.ToString() == "Bid" && currentItem.Count_InnerBid > 0)
                            || (placementTargetObject.Column.Header.ToString() == "Ofr" && currentItem.Count_InnerOfr > 0))
                            {
                                // 显示浮动框
                                this.InnerDepthPopup.IsOpen = true;
                            }
                        }
                    }
                }

                DataGridCell arrowObj = this.InnerDepthPopup.PlacementTarget as DataGridCell;
                if (!this.InnerDepthPopup.IsMouseOver)
                {
                    if (arrowObj != null)
                    {
                        if (!arrowObj.IsMouseOver)
                        {
                            this.InnerDepthPopup.IsOpen = false;
                            this.Timer4ControlDetailPopup.Stop();
                            CloseDepthPop();
                        }
                    }
                    else
                    {
                        this.InnerDepthPopup.IsOpen = false;
                        this.Timer4ControlDetailPopup.Stop();
                        CloseDepthPop();
                    }
                }
            }
            catch (System.Data.RowNotInTableException ex)
            {
                // ignore
            }
            catch (Exception otherEx)
            {
                LogHandler.Error(otherEx);
            }
        }

        private void CloseDepthPop()
        {
            InnerDepthPopupView InnerDepthPopupView = this.InnerDepthPopup.Child as InnerDepthPopupView;
            if (InnerDepthPopupView != null && InnerDepthPopupView.DataContext is InnerDepthPopupViewModel)
            {
                InnerDepthPopupViewModel viewModel = InnerDepthPopupView.DataContext as InnerDepthPopupViewModel;
                viewModel.SecurityID = null;
                viewModel.Symbol = null;
                viewModel.UnfilterData();
            }
        }

        private void InnerBid_DataGridCell_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridCell c = sender as DataGridCell;
            //placementTargetObject = this.market_best.GetCellContent(c.DataContext).Parent as DataGridCell;
            placementTargetObject = c;

            if (this.Timer4ControlDetailPopup.IsEnabled == false)
            {
                this.Timer4ControlDetailPopup.Start();
            }
        }

        private void InnerOfr_DataGridCell_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridCell c = sender as DataGridCell;
            //placementTargetObject = this.market_best.GetCellContent(c.DataContext).Parent as DataGridCell;
            placementTargetObject = c;

            if (this.Timer4ControlDetailPopup.IsEnabled == false)
            {
                this.Timer4ControlDetailPopup.Start();
            }
        }

        #endregion

        #region 刷新闪烁
        private DispatcherTimer Timer4Refresh;

        private void Timer4Refresh_Tick(object sender, EventArgs e)
        {
            if (!_innerBoardViewModel.isInit)
                return;

            if (bestTab.IsSelected)
            {
                lock (_innerBoardViewModel._dataLock_best)
                {
                    IEnumerable<InnerOrderData> flashDatas = _innerBoardViewModel.FindInnerOrderDatas_Best();
                    foreach (InnerOrderData item in flashDatas)
                    {

                        if (item.bestbidFlashCount > 0)
                        {
                            if (item.bestbidFlashCount == 4)
                                item.Best_Bid_Opacity = 0.5;
                            else if (item.bestbidFlashCount == 3)
                                item.Best_Bid_Opacity = 0;
                            else if (item.bestbidFlashCount == 2)
                                item.Best_Bid_Opacity = 0.5;
                            else
                                item.Best_Bid_Opacity = 1;
                            item.bestbidFlashCount--;
                        }
                        if (item.bestofrFlashCount > 0)
                        {
                            if (item.bestofrFlashCount == 4)
                                item.Best_Ofr_Opacity = 0.5;
                            else if (item.bestofrFlashCount == 3)
                                item.Best_Ofr_Opacity = 0;
                            else if (item.bestofrFlashCount == 2)
                                item.Best_Ofr_Opacity = 0.5;
                            else
                                item.Best_Ofr_Opacity = 1;
                            item.bestofrFlashCount--;
                        }
                    }
                }

                lock (_innerBoardViewModel._dataLock)
                {
                    IEnumerable<InnerOrderData> flashDatas = _innerBoardViewModel.FindInnerOrderDatas();
                    foreach (InnerOrderData item in flashDatas)
                    {
                        item.bestbidFlashCount = 0;
                        item.bestofrFlashCount = 0;
                        item.FlashCount = 0;
                    }
                }
            }
            else
            {
                lock (_innerBoardViewModel._dataLock)
                {
                    IEnumerable<InnerOrderData> flashDatas = _innerBoardViewModel.FindInnerOrderDatas();

                    foreach (InnerOrderData item in flashDatas)
                    {
                        if (item.FlashCount > 0)
                            item.FlashCount--;
                        if (item.bestbidFlashCount > 0)
                        {
                            if (item.bestbidFlashCount == 4)
                                item.Best_Bid_Opacity = 0.5;
                            else if (item.bestbidFlashCount == 3)
                                item.Best_Bid_Opacity = 0;
                            else if (item.bestbidFlashCount == 2)
                                item.Best_Bid_Opacity = 0.5;
                            else
                                item.Best_Bid_Opacity = 1;
                            item.bestbidFlashCount--;
                        }
                        if (item.bestofrFlashCount > 0)
                        {
                            if (item.bestofrFlashCount == 4)
                                item.Best_Ofr_Opacity = 0.5;
                            else if (item.bestofrFlashCount == 3)
                                item.Best_Ofr_Opacity = 0;
                            else if (item.bestofrFlashCount == 2)
                                item.Best_Ofr_Opacity = 0.5;
                            else
                                item.Best_Ofr_Opacity = 1;
                            item.bestofrFlashCount--;
                        }
                    }

                }

                lock (_innerBoardViewModel._dataLock_best)
                {
                    IEnumerable<InnerOrderData> flashDatas = _innerBoardViewModel.FindInnerOrderDatas_Best();
                    foreach (InnerOrderData item in flashDatas)
                    {
                        item.bestbidFlashCount = 0;
                        item.bestofrFlashCount = 0;
                        item.FlashCount = 0;
                    }
                }
            }
        }
        #endregion

        private void EditGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetEditTagGroup()))
            {
                EditGroupWindow editGroupWin = new EditGroupWindow(IntentionOldHandler.Instance.GroupList, _innerBoardViewModel.SelectedItem_Tag?.GroupID);
                editGroupWin.EditAction = OnEditAction;
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetEditTagGroup(), editGroupWin);
            }
        }
        private void EditGroup_Best_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetEditTagGroup()))
            {
                EditGroupWindow editGroupWin = new EditGroupWindow(IntentionOldHandler.Instance.GroupList, _innerBoardViewModel.SelectedItem_Tag_Best?.GroupID);
                editGroupWin.EditAction = OnEditAction;
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetEditTagGroup(), editGroupWin);
            }
        }

        public void OnAddAction(FilterGroupItem filterGroupItem)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                _innerBoardViewModel.SelectedItem_Tag = _innerBoardViewModel.SelectedItem_Tag_Best = filterGroupItem;
                //_innerBoardViewModel.FocusTags = _innerBoardViewModel.SelectedItem_Tag?.FocusTags;
                //_innerBoardViewModel.FocusTags_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.FocusTags;
            }));
        }

        public void OnEditAction()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                _innerBoardViewModel.FocusTags = _innerBoardViewModel.SelectedItem_Tag?.FocusTags;
                _innerBoardViewModel.FocusTags_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.FocusTags;

                _innerBoardViewModel.IsPrice = _innerBoardViewModel.SelectedItem_Tag != null ? _innerBoardViewModel.SelectedItem_Tag.IsPrice : false;
                _innerBoardViewModel.StartPrice = _innerBoardViewModel.SelectedItem_Tag?.StartPrice;
                _innerBoardViewModel.EndPrice = _innerBoardViewModel.SelectedItem_Tag?.EndPrice;
                _innerBoardViewModel.StartTermDY = _innerBoardViewModel.SelectedItem_Tag?.StartTermDY;
                _innerBoardViewModel.EndTermDY = _innerBoardViewModel.SelectedItem_Tag?.EndTermDY;

                _innerBoardViewModel.IsPrice_Best = _innerBoardViewModel.SelectedItem_Tag_Best != null ? _innerBoardViewModel.SelectedItem_Tag_Best.IsPrice : false;
                _innerBoardViewModel.StartPrice_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.StartPrice;
                _innerBoardViewModel.EndPrice_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.EndPrice;
                _innerBoardViewModel.StartTermDY_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.StartTermDY;
                _innerBoardViewModel.EndTermDY_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.EndTermDY;
            }));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetAddTagGroup()))
            {
                AddGroupWindow addGroupWin = new AddGroupWindow(IntentionOldHandler.Instance.GroupList, this._innerBoardViewModel.FocusTags, _innerBoardViewModel.SelectedItem_Tag?.GroupID, this._innerBoardViewModel.StartTermDY, this._innerBoardViewModel.EndTermDY, this._innerBoardViewModel.IsAppTerm,
                    this._innerBoardViewModel.StartPrice, this._innerBoardViewModel.EndPrice, this._innerBoardViewModel.IsPrice);
                addGroupWin.AddAction = OnAddAction;
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetAddTagGroup(), addGroupWin);
            }
        }

        private void Save_Best_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetAddTagGroup()))
            {
                AddGroupWindow addGroupWin = new AddGroupWindow(IntentionOldHandler.Instance.GroupList, this._innerBoardViewModel.FocusTags_Best, _innerBoardViewModel.SelectedItem_Tag_Best?.GroupID, this._innerBoardViewModel.StartTermDY_Best, this._innerBoardViewModel.EndTermDY_Best, this._innerBoardViewModel.IsAppTerm_Best,
                    this._innerBoardViewModel.StartPrice_Best, this._innerBoardViewModel.EndPrice_Best, this._innerBoardViewModel.IsPrice_Best);
                addGroupWin.AddAction = OnAddAction;
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetAddTagGroup(), addGroupWin);
            }
        }

        private void btnRequire_Click(object sender, RoutedEventArgs e)
        {
            string securityID = _TRADEBOOK;
            if (_innerBoardViewModel != null
                && _innerBoardViewModel.SelectedItem_MyInnerOrders != null
                && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID))
            {
                securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID;
            }

            SendIRSView sendRequireView = new SendIRSView(securityID, true);

            sendRequireView.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string securityID = _TRADEBOOK;
            if (_innerBoardViewModel != null
                && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null
                && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID))
            {
                securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
            }

            SendIRSView sendRequireView = new SendIRSView(securityID, true);
            sendRequireView.Show();
        }

        public void EditOrder(InnerOrderData orderData, bool isBid, string securityId, string intentionid)
        {
            string side = isBid ? ComWindowKeyHelper.BID : ComWindowKeyHelper.OFR;
            
            if (orderData != null)
            {
                PriceMatchParam priceMatchParam = new PriceMatchParam();
                priceMatchParam.CustomFirmAmount = (orderData.Qty * 100).ToString(); // OrderQty 订单量
                priceMatchParam.OrderId = orderData.StrategyOrderId;// 订单号
                priceMatchParam.Side = orderData.Side; // 方向
                priceMatchParam.Symbol = orderData.SecurityID; // 代码
                priceMatchParam.SymbolName = orderData.Symbol; // 名称
                priceMatchParam.WholeFlag = orderData.WholeFlag; // 整量标志
                //priceMatchParam.NetPrice = orderData.NetPrice; // 净价
                priceMatchParam.Price = orderData.Price.ToString(); // 收益率
                priceMatchParam.SettlType = orderData.SettleType; // 清算速度
                priceMatchParam.NonfirmTime = string.Empty; // nonfirmTime;
                priceMatchParam.OperationType = "1";

                DataRow myOrderDr2 = MyOrderProcesser.Processer.GetOrderById(orderData.StrategyOrderId);
                if (myOrderDr2 != null)
                {
                    priceMatchParam.Info1 = CommonUtils.ConvertToString(myOrderDr2[MyOrderProcesser.INFO1_FIELD]);
                    priceMatchParam.Info2 = CommonUtils.ConvertToString(myOrderDr2[MyOrderProcesser.INFO2_FIELD]);
                    priceMatchParam.PriceCondition = CommonUtils.ConvertToString(myOrderDr2[MyOrderProcesser.PriceCondition_Field]);
                    priceMatchParam.InstitutionID = CommonUtils.ConvertToString(myOrderDr2[MyOrderProcesser.INSTITUTIONID_FIELD]);
                    priceMatchParam.InstitutionName = CommonUtils.ConvertToString(myOrderDr2[MyOrderProcesser.INSTITUTIONNAME_FIELD]);
                    priceMatchParam.TraderID = CommonUtils.ConvertToString(myOrderDr2[MyOrderProcesser.TRADERID_FIELD]);
                    priceMatchParam.TraderName = CommonUtils.ConvertToString(myOrderDr2[MyOrderProcesser.TRADERNAME_FIELD]);
                    priceMatchParam.QuoteType = CommonUtils.ConvertToString(myOrderDr2[MyOrderProcesser.QuoteType_Field]);
                }
                priceMatchParam.IntentionID = intentionid;

                if (orderData.FirmFlag == "1")
                {
                    priceMatchParam.IsFirm = true;// firm
                }
                else
                {
                    priceMatchParam.IsFirm = false;// nonfirm
                }

                priceMatchParam.SendOrderType = "OCO";
                //priceMatchParam.InstitutionID = orderData.InstitutionID; //客盘机构id
                //priceMatchParam.InstitutionName = orderData.InstitutionName;//客盘机构名字
                //priceMatchParam.TraderID = orderData.TraderID; // 客盘交易员id
                //priceMatchParam.TraderName = orderData.TraderName;//客盘交易员名字
                //priceMatchParam.Terminal = orderData.Terminal;
                //priceMatchParam.Info3 = orderData.Info3;//用于IDB单子

                if (string.IsNullOrEmpty(intentionid))
                {
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                    {
                        if (CommonUtils.IsIRS(priceMatchParam.Symbol))
                        {
                            SendIRSView sendrisview = new SendIRSView(priceMatchParam);
                            WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId), sendrisview);
                        }
                        else
                        {
                            SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
                            // bid;
                            if (orderData.Side == "1")
                            {
                                sendPriceView.SetButtonEnable("OfrBtn", false);
                            }
                            // ofr
                            else if (orderData.Side == "2")
                            {
                                sendPriceView.SetButtonEnable("BidBtn", false);
                            }

                            if (orderData.FirmFlag == "1")
                            {
                                sendPriceView.SetButtonEnable("Search_TextBox", false);
                            }
                            else
                            {
                                sendPriceView.SetButtonEnable("Search_TextBox", false);
                            }

                            WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId), sendPriceView);
                        }
                    }
                }
                else
                {
                    
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                    {
                        DataRow myOrderDr = MyOrderProcesser.Processer.GetOrderById(orderData.StrategyOrderId);
                        if (myOrderDr != null)
                        {
                            priceMatchParam.Info1 = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.INFO1_FIELD]);
                            priceMatchParam.Info2 = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.INFO2_FIELD]);
                            priceMatchParam.PriceCondition = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.PriceCondition_Field]);
                            priceMatchParam.InstitutionID = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.INSTITUTIONID_FIELD]);
                            priceMatchParam.InstitutionName = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.INSTITUTIONNAME_FIELD]);
                            priceMatchParam.TraderID = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.TRADERID_FIELD]);
                            priceMatchParam.TraderName = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.TRADERNAME_FIELD]);
                            priceMatchParam.QuoteType = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.QuoteType_Field]);
                        }
                        priceMatchParam.IntentionID = intentionid;

                        //如果是模糊需求
                        if (orderData.SecurityID == "99999")
                        {
                            Common.Model.Fix.Message.NewOrderSingle oldOrder = new Common.Model.Fix.Message.NewOrderSingle();
                            oldOrder.IntentionID = intentionid;
                            oldOrder.StrategyOrderId = orderData.StrategyOrderId;
                            oldOrder.AlgoName = "CROSS";
                            priceMatchParam.OldOrder = oldOrder;
                            Common.Model.OrderTimeModel otm = Newtonsoft.Json.JsonConvert.DeserializeObject<Common.Model.OrderTimeModel>(priceMatchParam.Info1);
                            if (!string.IsNullOrEmpty(otm.QuoteType))
                            {
                                if (otm.QuoteType == "virtual")
                                {
                                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "EditFuzzyOrder", RequestID = $"EditFuzzyOrder-{priceMatchParam.OrderId}", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(priceMatchParam) });
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (CommonUtils.IsIRS(priceMatchParam.Symbol))
                            {
                                SendIRSView sendrisview = new SendIRSView(priceMatchParam);
                                WindowManager.Instance.OpenOnlyOneWindow($"EditIRSOrder-{priceMatchParam.IntentionID}", sendrisview);
                            }
                            else
                            {
                                SendRequireView sendRequireView = new SendRequireView(priceMatchParam);
                                WindowManager.Instance.OpenOnlyOneWindow($"EditOrder-{priceMatchParam.IntentionID}", sendRequireView);
                            }
                        }
                    }
                }
            }
        }



        private void JoinMenu_Click(object sender, RoutedEventArgs e)
        {
            //if (_innerBoardViewModel.SelectedItem_MyInnerOrders != null)
            //{
            //    Common.ViewModel.TradeBookHandler.QuickOrderHandler.Instance.QuickPrice(null, true, _innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID);
            //}

            if (_innerBoardViewModel.SelectedItem_MyInnerOrders != null)
            {
                bool isBid = _innerBoardViewModel.SelectedItem_MyInnerOrders.Side == "1";

                string securityID = "TradeBook";
                if (!string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID))
                {
                    securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID;
                }

                SendIRSView sendRequireView = new SendIRSView(securityID, isBid, true);
                if (!WindowManager.Instance.IsExist($"IRS{securityID}"))
                {
                    WindowManager.Instance.OpenOnlyOneWindow($"IRS{securityID}", sendRequireView);
                }

            }

        }

        internal async void DealDelete(List<InnerOrderData> refList, string questionMsg = "是否撤销选中报价？")
        {
            //LogHandler.Info($"DealDelete {orderId}");
            bool result = CustomMessageBox.Question(questionMsg, "REF");

            if (result == true)
            {
                StringBuilder errMsg = new StringBuilder();
                foreach (InnerOrderData innerOrder in refList)
                {
                    if (innerOrder.IsMe)
                    {
                        string msg = await CancelOne(innerOrder.StrategyOrderId);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            errMsg.AppendLine(msg);
                        }
                    }
                }

                if (errMsg.Length > 0)
                {
                    CommonUtils.Prompt2Second("REF", errMsg.ToString());
                }
                else
                {
                    CommonUtils.Prompt2Second("REF", "REF成功");
                }
            }
        }

        private async void CancelPrice(string orderid)
        {
            bool result = CustomMessageBox.Question("是否撤销本人在本券上的所有报价？", "REF");

            if (result == true)
            {
                string msg = await CancelOne(orderid);

                if (!string.IsNullOrEmpty(msg))
                {
                    CommonUtils.Prompt2Second("REF", msg);
                }
            }
        }

        private async Task<string> CancelOne(string orderid)
        {
            OrderCancelRequest cancelPo = new OrderCancelRequest();
            DataRow dr = MyOrderProcesser.Processer.GetOrderById(orderid);
            if (dr != null)
            {
                cancelPo.AlgoName = CommonUtils.ConvertToString(dr[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
            }

            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
            cancelPo.OrderID = orderid;
            TBookCancelReplyPo reply = await CancelOrder(cancelPo);
            if (reply == null || !reply.IsSuccess())
            {
                return string.IsNullOrEmpty(reply.Msg) ? $"[{orderid}]撤销失败" : reply.Msg;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<TBookCancelReplyPo> CancelOrder(STC.Common.Model.TBook.OrderCancelRequest cancelPo)
        {
            TBookCancelReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = OrdersHandler.Instance.CancelOrder(cancelPo);

            });
            return reply;
        }

        private void Order(InnerOrderData innerOrderData, bool isDrp)
        {
            

            if (innerOrderData.IsMe == false)
            {
                if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetPriceMatchWinKey(innerOrderData.OrderID)))
                {
                    if (!LocalEnv.ConfInfo.EnableIOC)
                    {
                        return;
                    }
                    SendPointPriceView sendPointPriceView = new SendPointPriceView(innerOrderData.SecurityID, innerOrderData.Side == "1" ? true : false, innerOrderData.Price.ToString(), (innerOrderData.Qty * 100).ToString(),
                    innerOrderData.SettleType, innerOrderData.NetPrice.ToString(), "MANUAL",
                    innerOrderData.WholeFlag, innerOrderData.FirmFlag == "1", innerOrderData.StrategyOrderId,
                    null, null, null, null,
                    null, string.Empty, "INNER", null, innerOrderData.IntentionID, isDrp, $"{innerOrderData.RoleName}-{innerOrderData.PartySubName}", null , null, null, null, new Common.ViewModel.Order.DRPParamInfoModel() { OrderId = innerOrderData.OrderID });


                    WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetExecBackWinKey(innerOrderData.OrderID), sendPointPriceView);
                }
            }
            else
            {
                EditOrder(innerOrderData, innerOrderData.Side == "1" ? true : false, innerOrderData.SecurityID, innerOrderData.IntentionID);
            }
        }

        private void OrderBest_Bid(bool isDrp)
        {
            InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;
            if (innerOrderData.Count_InnerBid > 0)
            {
                List<InnerOrderData> bidList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "1", _innerBoardViewModel.ofrPriceComparer);
                if (bidList.Count > 0)
                {
                    Order(bidList[0], isDrp);
                }
            }
        }

        private void OrderBest_Ofr(bool isDrp)
        {
            InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;

            if (innerOrderData.Count_InnerOfr > 0)
            {
                List<InnerOrderData> ofrList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "2", _innerBoardViewModel.bidPriceComparer);
                if (ofrList.Count > 0)
                {
                    Order(ofrList[0], isDrp);
                }
            }
        }

        private void PointPriceMenu_Click(object sender, RoutedEventArgs e)
        {
            Order(this._innerBoardViewModel.SelectedItem_MyInnerOrders, false);
        }

        private void ReplyPriceMenu_Click(object sender, RoutedEventArgs e)
        {
            Order(this._innerBoardViewModel.SelectedItem_MyInnerOrders, true);
        }

        private void REFMenu_Click(object sender, RoutedEventArgs e)
        {
            List<InnerOrderData> refList = new List<InnerOrderData>();
            if (this.InnerBoard_dataGrid_IRS.SelectedItems.Count > 0)
            {
                foreach(InnerOrderData item in this.InnerBoard_dataGrid_IRS.SelectedItems)
                {
                    if (item.IsMe)
                    {
                        refList.Add(item);
                    }
                }
            }
            DealDelete(refList);
        }

        private void EditMenu_Click(object sender, RoutedEventArgs e)
        {
            Order(this._innerBoardViewModel.SelectedItem_MyInnerOrders, false);
        }

        private void JoinMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            if (_innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
            {
                bool isBid = !isOfrColumn();

                string securityID = "TradeBook";
                if (!string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID))
                {
                    securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
                }

                SendIRSView sendRequireView = new SendIRSView(securityID, isBid, true);
                if (!WindowManager.Instance.IsExist($"IRS_{securityID}"))
                {
                    WindowManager.Instance.OpenOnlyOneWindow($"IRS_{securityID}", sendRequireView);
                }

            }
        }

        private void PointPriceMenu_Best_Click(object sender, RoutedEventArgs e)
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

        private void ReplyPriceMenu_Best_Click(object sender, RoutedEventArgs e)
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

        private void REFMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            if (isOfrColumn())
            {
                List<InnerOrderData> refList = new List<InnerOrderData>();
                if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 0)
                {
                    foreach (InnerOrderData item in this.InnerBoard_Best_dataGrid_IRS.SelectedItems)
                    {
                        string securityid = item.SecurityID;
                        decimal? price = item.Price_InnerOfr;
                        IEnumerable<InnerOrderData> arr = this._innerBoardViewModel.GetInnerOrderDatas(obj => obj.SecurityID == securityid && obj.Side == "2" && obj.Price == price && obj.IsMe);
                        refList.AddRange(arr);
                    }
                }
                if (refList.Count > 0)
                {
                    DealDelete(refList);
                }
            }
            else
            {
                List<InnerOrderData> refList = new List<InnerOrderData>();
                if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 0)
                {
                    foreach (InnerOrderData item in this.InnerBoard_Best_dataGrid_IRS.SelectedItems)
                    {
                        string securityid = item.SecurityID;
                        decimal? price = item.Price_InnerBid;
                        IEnumerable<InnerOrderData> arr = this._innerBoardViewModel.GetInnerOrderDatas(obj => obj.SecurityID == securityid && obj.Side == "1" && obj.Price == price && obj.IsMe);
                        refList.AddRange(arr);
                    }
                }
                if (refList.Count > 0)
                {
                    DealDelete(refList);
                }
            }
        }

        private void EditMenu_Best_Click(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// 0:没有数据
        /// 1:全部是我的
        /// 2:全部不是我的
        /// 3:有的是我的， 有的不是我的
        /// </summary>
        /// <returns></returns>
        private string IsMe_SelectedItem()
        {
            if (this.InnerBoard_dataGrid_IRS.SelectedItems.Count > 0)
            {
                int isMeCount = 0;
                int isNotMeCount = 0;
                foreach (InnerOrderData item in this.InnerBoard_dataGrid_IRS.SelectedItems)
                {
                    if (item.IsMe)
                    {
                        isMeCount++;
                    }
                    else
                    {
                        isNotMeCount++;
                    }
                }

                if (isMeCount > 0 && isNotMeCount > 0)
                {
                    return "3";
                }
                else if (isMeCount > 0)
                {
                    return "1";
                }
                else if (isNotMeCount > 0)
                {
                    return "2";
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                return "0";
            }
        }

        private void dgMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (this.InnerBoard_dataGrid_IRS.SelectedItems.Count > 1)
            {
                ConsiderationMenu.Visibility = Visibility.Collapsed;
                SearchMenu.Visibility = Visibility.Collapsed;
                PointPriceMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (this.InnerBoard_dataGrid_IRS.SelectedItem != null && this.InnerBoard_dataGrid_IRS.SelectedItem is InnerOrderData)
                {
                    InnerOrderData innerOrderData = this.InnerBoard_dataGrid_IRS.SelectedItem as InnerOrderData;
                    if (innerOrderData.SecurityID == "99999")
                    {
                        ConsiderationMenu.Visibility = Visibility.Collapsed;
                        PointPriceMenu.Visibility = Visibility.Collapsed;
                        JoinMenu.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu.Visibility = Visibility.Collapsed;
                    }
                    else if (innerOrderData.Price.HasValue && innerOrderData.Qty.HasValue)
                    {
                        ConsiderationMenu.Visibility = Visibility.Visible;
                        if(innerOrderData.IsMe == false)
                        {
                            PointPriceMenu.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            PointPriceMenu.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        if (innerOrderData.FirmFlag == "1")
                        {
                            ConsiderationMenu.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ConsiderationMenu.Visibility = Visibility.Visible;
                        }
                        PointPriceMenu.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    ConsiderationMenu.Visibility = Visibility.Collapsed;
                    PointPriceMenu.Visibility = Visibility.Collapsed;
                }
                
                SearchMenu.Visibility = Visibility.Visible;
            }
            string status = IsMe_SelectedItem();
            if (status == "1")
            {
                JoinMenu.Visibility = Visibility.Collapsed;
                //PointPriceMenu.Visibility = Visibility.Collapsed;
                ReplyPriceMenu.Visibility = Visibility.Collapsed;
                REFMenu.Visibility = Visibility.Visible;
                if (this.InnerBoard_dataGrid_IRS.SelectedItems.Count > 1)
                {
                    EditMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    EditMenu.Visibility = Visibility.Visible;
                }
            }
            else if (status == "2")
            {
                
                if (this.InnerBoard_dataGrid_IRS.SelectedItems.Count > 1)
                {
                    JoinMenu.Visibility = Visibility.Collapsed;
                    //PointPriceMenu.Visibility = Visibility.Collapsed;
                    ReplyPriceMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (this._innerBoardViewModel.SelectedItem_MyInnerOrders != null 
                        && this._innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID == "99999")
                    {
                        JoinMenu.Visibility = Visibility.Collapsed;
                        //PointPriceMenu.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        JoinMenu.Visibility = Visibility.Visible;
                        //PointPriceMenu.Visibility = Visibility.Visible;
                        ReplyPriceMenu.Visibility = Visibility.Visible;
                    }
                }
                REFMenu.Visibility = Visibility.Collapsed;
                EditMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                JoinMenu.Visibility = Visibility.Collapsed;
                //PointPriceMenu.Visibility = Visibility.Collapsed;
                ReplyPriceMenu.Visibility = Visibility.Collapsed;
                REFMenu.Visibility = Visibility.Collapsed;
                EditMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void InnerBoard_Best_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            //DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            DependencyObject cell = e.OriginalSource as DependencyObject;
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            if (targetCell != null)
            {
                this.InnerBoard_Best_dataGrid_IRS.CurrentColumn = targetCell.Column;

                if(this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count == 0)
                {
                    DataGridRow targetRow = this.InnerBoard_Best_dataGrid_IRS.ItemContainerGenerator.ContainerFromItem(targetCell.DataContext) as DataGridRow;

                    if (targetRow != null)
                    {
                        targetRow.IsSelected = true;
                    }
                }
            }
        }

        private void InnerBoard_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            //DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            DependencyObject cell = e.OriginalSource as DependencyObject;
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            if (targetCell != null)
            {
                this.InnerBoard_dataGrid_IRS.CurrentColumn = targetCell.Column;

                if (this.InnerBoard_dataGrid_IRS.SelectedItems.Count == 0)
                {
                    DataGridRow targetRow = this.InnerBoard_dataGrid_IRS.ItemContainerGenerator.ContainerFromItem(targetCell.DataContext) as DataGridRow;

                    if (targetRow != null)
                    {
                        targetRow.IsSelected = true;
                    }
                }
            }
        }

        /// <summary>
        /// 0:没有数据
        /// 1:全部是我的
        /// 2:全部不是我的
        /// 3:有的是我的， 有的不是我的
        /// </summary>
        /// <returns></returns>
        private string IsMe_SelectedItem_Best(string side)
        {
            if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 0)
            {
                int isMeCount = 0;
                int isNotMeCount = 0;
                foreach (InnerOrderData item in this.InnerBoard_Best_dataGrid_IRS.SelectedItems)
                {
                    if (side == "1")// bid
                    {
                        if (item.Count_InnerBid > 0)
                        {
                            if (item.IsMe_InnerBid)
                            {
                                isMeCount++;
                            }
                            else
                            {
                                isNotMeCount++;
                            }
                        }
                    }
                    else//ofr
                    {
                        if (item.Count_InnerOfr > 0)
                        {
                            if (item.IsMe_InnerOfr)
                            {
                                isMeCount++;
                            }
                            else
                            {
                                isNotMeCount++;
                            }
                        }
                    }
                }

                if (isMeCount > 0 && isNotMeCount > 0)
                {
                    return "3";
                }
                else if (isMeCount > 0)
                {
                    return "1";
                }
                else if (isNotMeCount > 0)
                {
                    return "2";
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                return "0";
            }
        }

        private void dgMenu_Best_Opened(object sender, RoutedEventArgs e)
        {
            if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 1)
            {
                ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                SearchMenu_Best.Visibility = Visibility.Collapsed;
                PointPriceMenu_Best.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (this.InnerBoard_Best_dataGrid_IRS.SelectedItem != null && this.InnerBoard_Best_dataGrid_IRS.SelectedItem is InnerOrderData)
                {
                    InnerOrderData data = this.InnerBoard_Best_dataGrid_IRS.SelectedItem as InnerOrderData;
                    if (data.SecurityID == "99999")
                    {
                        ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                        PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        JoinMenu_Best.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                    }
                    else if (isBidColumn())
                    {
                        if (data.Price_InnerBid.HasValue && data.Qty_InnerBid.HasValue)
                        {
                            ConsiderationMenu_Best.Visibility = Visibility.Visible;
                            if (data.IsMe_InnerBid == false)
                            {
                                if (LocalEnv.ConfInfo.EnableIOC)
                                {
                                    PointPriceMenu_Best.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                                }
                            }
                            else
                            {
                                PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            if (data.FirmFlag_InnerBid == "1")
                            {
                                ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                ConsiderationMenu_Best.Visibility = Visibility.Visible;
                            }
                            PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        }
                    }
                    else if (isOfrColumn())
                    {
                        if (data.Price_InnerOfr.HasValue && data.Qty_InnerOfr.HasValue)
                        {
                            ConsiderationMenu_Best.Visibility = Visibility.Visible;
                            if (data.IsMe_InnerOfr == false)
                            {
                                if (LocalEnv.ConfInfo.EnableIOC)
                                {
                                    PointPriceMenu_Best.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                                }
                            }
                            else
                            {
                                PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            if (data.FirmFlag_InnerOfr == "1")
                            {
                                ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                ConsiderationMenu_Best.Visibility = Visibility.Visible;
                            }
                            PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                        PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                    PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                }
                SearchMenu_Best.Visibility = Visibility.Visible;
            }

            if (this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null
                && (isBidColumn() || isOfrColumn()))
            {
                if (isBidColumn())
                {
                    string status = IsMe_SelectedItem_Best("1");

                    if (status == "1")
                    {
                        JoinMenu_Best.Visibility = Visibility.Collapsed;
                        //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        REFMenu_Best.Visibility = Visibility.Visible;
                        if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 1)
                        {
                            EditMenu_Best.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            EditMenu_Best.Visibility = Visibility.Visible;
                        }
                    }
                    else if (status == "2")
                    {
                        
                        if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 1)
                        {
                            JoinMenu_Best.Visibility = Visibility.Collapsed;
                            //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                            ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            JoinMenu_Best.Visibility = Visibility.Visible;
                            //PointPriceMenu_Best.Visibility = Visibility.Visible;
                            if (LocalEnv.ConfInfo.EnableIOC)
                            {
                                ReplyPriceMenu_Best.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                            }
                            if (this.InnerBoard_Best_dataGrid_IRS.SelectedItem != null && this.InnerBoard_Best_dataGrid_IRS.SelectedItem is InnerOrderData)
                            {
                                InnerOrderData data = this.InnerBoard_Best_dataGrid_IRS.SelectedItem as InnerOrderData;

                                if (data.SecurityID == "99999")
                                {
                                    JoinMenu_Best.Visibility = Visibility.Collapsed;
                                    //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                                    ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                        REFMenu_Best.Visibility = Visibility.Collapsed;
                        EditMenu_Best.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        JoinMenu_Best.Visibility = Visibility.Collapsed;
                        //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        REFMenu_Best.Visibility = Visibility.Collapsed;
                        EditMenu_Best.Visibility = Visibility.Collapsed;
                    }
                }

                if (isOfrColumn())
                {
                    string status = IsMe_SelectedItem_Best("2");

                    if (status == "1")
                    {
                        JoinMenu_Best.Visibility = Visibility.Collapsed;
                        //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        REFMenu_Best.Visibility = Visibility.Visible;
                        if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 1)
                        {
                            EditMenu_Best.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            EditMenu_Best.Visibility = Visibility.Visible;
                        }
                    }
                    else if (status == "2")
                    {
                        if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 1)
                        {
                            JoinMenu_Best.Visibility = Visibility.Collapsed;
                            //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                            ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            JoinMenu_Best.Visibility = Visibility.Visible;
                            //PointPriceMenu_Best.Visibility = Visibility.Visible;
                            if (LocalEnv.ConfInfo.EnableIOC)
                            {
                                ReplyPriceMenu_Best.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                            }
                            if (this.InnerBoard_Best_dataGrid_IRS.SelectedItem != null && this.InnerBoard_Best_dataGrid_IRS.SelectedItem is InnerOrderData)
                            {
                                InnerOrderData data = this.InnerBoard_Best_dataGrid_IRS.SelectedItem as InnerOrderData;

                                if (data.SecurityID == "99999")
                                {
                                    JoinMenu_Best.Visibility = Visibility.Collapsed;
                                    //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                                    ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                        REFMenu_Best.Visibility = Visibility.Collapsed;
                        EditMenu_Best.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        JoinMenu_Best.Visibility = Visibility.Collapsed;
                        //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        REFMenu_Best.Visibility = Visibility.Collapsed;
                        EditMenu_Best.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                JoinMenu_Best.Visibility = Visibility.Collapsed;
                //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                REFMenu_Best.Visibility = Visibility.Collapsed;
                EditMenu_Best.Visibility = Visibility.Collapsed;
            }
        }

        private void TagGroupMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                MenuItem mi = sender as MenuItem;
                if (mi.DataContext is FilterGroupItem)
                {
                    FilterGroupItem fgi = mi.DataContext as FilterGroupItem;
                    _innerBoardViewModel.UpdateGroupOne(fgi, !fgi.IfSubscribe);
                }
            }
        }

        private void InnerBoard_Best_dataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            this.InnerDepthPopup.IsOpen = false;
            CloseDepthPop();
        }

        private void InnerBoard_Best_dataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.InnerDepthPopup.IsOpen = false;
            CloseDepthPop();
        }

        private void ProfileListBox_Best_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                e.Handled = true;
        }

        private void ProfileListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                e.Handled = true;
        }

        private void ConsiderationMenu_Click(object sender, RoutedEventArgs e)
        {
            InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders;
            ConsiderationOrder(innerOrderData, innerOrderData.Side == "1" ? false : true, innerOrderData);
        }

        private void ConsiderationMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            if (isOfrColumn())
            {
                InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;

                List<InnerOrderData> ofrList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "2", _innerBoardViewModel.bidPriceComparer);
                if (ofrList.Count > 0)
                {
                    ConsiderationOrder(ofrList[0], true, innerOrderData);
                }
                else
                {
                    ConsiderationOrder(null, true, innerOrderData);
                }
            }
            else
            {
                InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;

                List<InnerOrderData> bidList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "1", _innerBoardViewModel.ofrPriceComparer);
                if (bidList.Count > 0)
                {
                    ConsiderationOrder(bidList[0], false, innerOrderData);
                }
                else
                {
                    ConsiderationOrder(null, false, innerOrderData);
                }
            }
        }

        public void ConsiderationOrder(InnerOrderData orderData, bool isBid, InnerOrderData orderData_best)
        {
            string securityId = string.Empty;
            string symbol = string.Empty;
            string wholeFlag = string.Empty;
            string price_innerOfr = string.Empty;
            string price_innerBid = string.Empty;
            string settleType = "2";
            string firmFlag = string.Empty;
            string qtyQty = string.Empty;

            if (orderData != null)
            {
                wholeFlag = orderData.WholeFlag;
                firmFlag = orderData.FirmFlag;
                settleType = orderData.SettleType;
                qtyQty = orderData.Qty?.ToString();
            }

            if (orderData_best != null)
            {
                securityId = orderData_best.SecurityID;
                symbol = orderData_best.Symbol;
                price_innerBid = orderData_best.Price_InnerBid.ToString();
                price_innerOfr = orderData_best.Price_InnerOfr.ToString();
            }

            string side = isBid ? ComWindowKeyHelper.BID : ComWindowKeyHelper.OFR;

            PriceMatchParam priceMatchParam = new PriceMatchParam();

            if (string.IsNullOrEmpty(qtyQty))
            {
                //priceMatchParam.CustomFirmAmount = qtyQty;//orderData.Qty.ToString(); // OrderQty 订单量
                                                          // 使用默认报量 
                priceMatchParam.CustomFirmAmount = "3000";
                // 缺省报量
                if (SystemParamHandler.Instance.GetUserSetting() != null)
                {
                    UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
                    if (!string.IsNullOrEmpty(Us.userQty))
                    {
                        priceMatchParam.CustomFirmAmount = Us.userQty;
                    }
                }
            }
            else
            {
                priceMatchParam.CustomFirmAmount = (qtyQty.ConvertToLong() * 100).ToString();
            }
            
            priceMatchParam.Side = isBid ? "1" : "2"; // 方向
            priceMatchParam.Symbol = securityId; // 代码
            priceMatchParam.SymbolName = symbol; // 名称
            priceMatchParam.WholeFlag = wholeFlag; // 整量标志
                                                             //priceMatchParam.NetPrice = orderData.NetPrice; // 净价
                                                             //priceMatchParam.Price = orderData.Price.ToString(); // 收益率
            Common.Model.Fix.Message.MarketDataSnapshotFullRefresh ctPrice = PriceHandler.Instance.GetCacheCTPrice(securityId);
            if (isBid)
            {
                IEnumerable<Common.Model.Fix.Message.GNoMDEntries> bidBest = ctPrice.NoMDEntries?.Where(item => item.MDEntryType == "0" && item.VenueTypeGW != "INNER");
                if (bidBest.Count() > 0)
                {
                    priceMatchParam.Price = bidBest.First().MDEntryPx;
                }
                else
                {
                    if (!string.IsNullOrEmpty(price_innerBid))
                    {
                        priceMatchParam.Price = price_innerBid;
                    }
                    else if (!string.IsNullOrEmpty(price_innerOfr))
                    {
                        priceMatchParam.Price = price_innerOfr;
                    }
                    else
                    {
                        priceMatchParam.Price = string.Empty;
                    }
                }

            }
            else
            {
                IEnumerable<Common.Model.Fix.Message.GNoMDEntries> ofrBest = ctPrice.NoMDEntries?.Where(item => item.MDEntryType == "1" && item.VenueTypeGW != "INNER");
                if (ofrBest.Count() > 0)
                {
                    priceMatchParam.Price = ofrBest.First().MDEntryPx;
                }
                else
                {
                    if (!string.IsNullOrEmpty(price_innerOfr))
                    {
                        priceMatchParam.Price = price_innerOfr;
                    }
                    else if (!string.IsNullOrEmpty(price_innerBid))
                    {
                        priceMatchParam.Price = price_innerBid;
                    }
                    else
                    {
                        priceMatchParam.Price = string.Empty;
                    }
                }
            }

            priceMatchParam.SettlType = settleType; // 清算速度
            priceMatchParam.NonfirmTime = string.Empty; // nonfirmTime;
            priceMatchParam.OperationType = "1";
            if (firmFlag == "1")
            {
                priceMatchParam.IsFirm = true;// firm
            }
            else
            {
                priceMatchParam.IsFirm = false;// nonfirm
            }

            priceMatchParam.SendOrderType = "OCO";
            priceMatchParam.UpdataPrice = false;
            //priceMatchParam.InstitutionID = orderData.InstitutionID; //客盘机构id
            //priceMatchParam.InstitutionName = orderData.InstitutionName;//客盘机构名字
            //priceMatchParam.TraderID = orderData.TraderID; // 客盘交易员id
            //priceMatchParam.TraderName = orderData.TraderName;//客盘交易员名字
            //priceMatchParam.Terminal = orderData.Terminal;
            //priceMatchParam.Info3 = orderData.Info3;//用于IDB单子

            if (orderData == null || string.IsNullOrEmpty(orderData.IntentionID))
            {
                if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                {

                    //如果是模糊需求
                    if (securityId == "99999")
                    {
                        Common.Model.OrderTimeModel otm = Newtonsoft.Json.JsonConvert.DeserializeObject<Common.Model.OrderTimeModel>(priceMatchParam.Info1);
                        if (!string.IsNullOrEmpty(otm.QuoteType))
                        {
                            if (otm.QuoteType == "virtual")
                            {
                                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "EditFuzzyOrder", RequestID = $"EditFuzzyOrder-{priceMatchParam.OrderId}", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(priceMatchParam) });
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (CommonUtils.IsIRS(priceMatchParam.Symbol))
                        {
                            SendIRSView sendrisview = new SendIRSView(priceMatchParam);
                            WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId), sendrisview);
                        }
                        else
                        {
                            SendPriceView sendPriceView = new SendPriceView(priceMatchParam);

                            sendPriceView.SetButtonEnable("OfrBtn", false);
                            sendPriceView.SetButtonEnable("BidBtn", false);
                            sendPriceView.SetButtonEnable("Search_TextBox", false);
                            WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId), sendPriceView);
                        }
                    }
                }
            }
            else
            {

                if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                {
                    DataRow myOrderDr = MyOrderProcesser.Processer.GetOrderById(orderData.StrategyOrderId);
                    if (myOrderDr != null)
                    {
                        //priceMatchParam.Info1 = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.INFO1_FIELD]);
                        priceMatchParam.Info2 = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.INFO2_FIELD]);
                        priceMatchParam.PriceCondition = CommonUtils.ConvertToString(myOrderDr[MyOrderProcesser.PriceCondition_Field]);
                    }
                    priceMatchParam.IntentionID = orderData.IntentionID;
                    //如果是模糊需求
                    if (securityId == "99999")
                    {
                        Common.Model.OrderTimeModel otm = Newtonsoft.Json.JsonConvert.DeserializeObject<Common.Model.OrderTimeModel>(priceMatchParam.Info1);
                        if (!string.IsNullOrEmpty(otm.QuoteType))
                        {
                            if (otm.QuoteType == "virtual")
                            {
                                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "EditFuzzyOrder", RequestID = $"EditFuzzyOrder-{priceMatchParam.OrderId}", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(priceMatchParam) });
                                return;
                            }
                        }
                    }

                    SendIRSView sendrisview = new SendIRSView(priceMatchParam);
                    WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId), sendrisview);
                }
            }
        }

        private void InnerBoardView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                _innerBoardViewModel = this.DataContext as InnerBoardViewModel;
            }
            

            if (this.InnerDepthPopup == null)
            {
                this.InnerDepthPopup = new Popup();
                InnerDepthPopup.MouseLeave += InnerDepthPopup_MouseLeave;
                InnerDepthPopupView innerDepthPopupView = new InnerDepthPopupView();
                innerDepthPopupView.DataContext = _innerBoardViewModel.InnerDepthPopupViewModel;
                this.InnerDepthPopup.Child = innerDepthPopupView;
                this.InnerDepthPopup.StaysOpen = false;
                this.InnerDepthPopup.Closed += InnerDepthPopup_Closed;
            }
        }

        private void EditGroupMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                MenuItem mi = sender as MenuItem;
                if (mi.DataContext is FilterGroupItem)
                {
                    FilterGroupItem fgi = mi.DataContext as FilterGroupItem;
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetEditTagGroup()))
                    {
                        EditGroupWindow editGroupWin = new EditGroupWindow(IntentionOldHandler.Instance.GroupList, fgi?.GroupID);
                        editGroupWin.EditAction = OnEditAction;
                        WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetEditTagGroup(), editGroupWin);
                    }
                }
            }
        }

        private void DelGroupMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                MenuItem mi = sender as MenuItem;
                if (mi.DataContext is FilterGroupItem)
                {
                    FilterGroupItem fgi = mi.DataContext as FilterGroupItem;
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetEditTagGroup()))
                    {
                        DelOne(fgi?.GroupID);
                    }
                }
            }
        }

        internal void DelOne(string groupid)
        {
            bool result = CustomMessageBox.Question("是否删除组？", "删除组");

            if (result == false)
            {
                return;
            }

            if (!string.IsNullOrEmpty(groupid))
            {
                QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
                req.action = "3";
                req.id = groupid;
                QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);

                if (reply != null && reply.IsSuccess())
                {
                    try
                    {
                        FilterGroupItem removeItem = IntentionOldHandler.Instance.GroupList.First(item => item.GroupID == groupid);
                        IntentionOldHandler.Instance.GroupList.Remove(removeItem);
                    }
                    catch (System.InvalidOperationException)
                    {
                        // ignore
                    }
                }
            }
        }

        private void Sort_Best_Click(object sender, RoutedEventArgs e)
        {

        }

        private bool isOfrColumn()
        {
            return InnerBoard_Best_dataGrid_IRS.CurrentColumn != null &&
                (InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "Ofr"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "Vol.Ofr(百万)"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "No.Ofr"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "TraderO"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "ClientO"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "Ofr回复");
        }

        private bool isBidColumn()
        {
            return InnerBoard_Best_dataGrid_IRS.CurrentColumn != null &&
                (InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "Bid"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "Vol.Bid(百万)"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "No.Bid"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "TraderB"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "ClientB"
                || InnerBoard_Best_dataGrid_IRS.CurrentColumn.Header.ToString() == "Bid回复");
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item.IsSelected)
            {
                item.IsSelected = false;
                e.Handled = true;

                _innerBoardViewModel.FocusTags = InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS;

                _innerBoardViewModel.StartPrice = string.Empty;
                _innerBoardViewModel.EndPrice = string.Empty;
                _innerBoardViewModel.IsPrice = false;
            }
        }

        private void ListBoxItem_Best_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item.IsSelected)
            {
                item.IsSelected = false;
                e.Handled = true;

                _innerBoardViewModel.FocusTags_Best = InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS;

                _innerBoardViewModel.StartPrice_Best = string.Empty;
                _innerBoardViewModel.EndPrice_Best = string.Empty;
                _innerBoardViewModel.IsPrice_Best = false;
            }
        }

        bool isBestDataGridSelected = false;
        private void InnerBoard_Best_dataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            InnerOrderData innerOrderData = null;
            if (e.OriginalSource is FrameworkElement)
            {
                innerOrderData = (e.OriginalSource as FrameworkElement).DataContext as InnerOrderData;
            }
            else if (e.OriginalSource is FrameworkContentElement)
            {
                innerOrderData = (e.OriginalSource as FrameworkContentElement).DataContext as InnerOrderData;
            }
            
            if (innerOrderData != null)
            {
                DataGridRow targetRow = dg.ItemContainerGenerator.ContainerFromItem(innerOrderData) as DataGridRow;

                if (targetRow != null)
                { 
                   isBestDataGridSelected = targetRow.IsSelected;
                }
            }
            
        }

        bool isDataGridSelected = false;
        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                DependencyObject cell = e.OriginalSource as DependencyObject;
                while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
                System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
                 ;

                if (targetCell != null
                    && (targetCell.Column.Header.ToString() == "Bid"
                    || targetCell.Column.Header.ToString() == "Ofr"
                    || targetCell.Column.Header.ToString() == "代码"
                    || targetCell.Column.Header.ToString() == "简称"
                    || targetCell.Column.Header.ToString() == "报价"
                    || targetCell.Column.Header.ToString() == "报量(百万)"
                    || targetCell.Column.Header.ToString() == "意向类型"
                    || targetCell.Column.Header.ToString() == "清算速度"))
                    return;

                DataGrid dg = sender as DataGrid;
                InnerOrderData innerOrderData = null;
                if (e.OriginalSource is FrameworkElement)
                {
                    innerOrderData = (e.OriginalSource as FrameworkElement).DataContext as InnerOrderData;
                }
                else if (e.OriginalSource is FrameworkContentElement)
                {
                    innerOrderData = (e.OriginalSource as FrameworkContentElement).DataContext as InnerOrderData;
                }

                if (innerOrderData != null)
                {
                    DataGridRow targetRow = dg.ItemContainerGenerator.ContainerFromItem(innerOrderData) as DataGridRow;

                    if (targetRow != null && targetRow.IsSelected)
                    {
                        targetRow.IsSelected = false;
                        e.Handled = true;
                    }
                }
            }
            catch
            {

            }
        }

        private void refall_click(object sender, RoutedEventArgs e)
        {
            List<InnerOrderData> refList = _innerBoardViewModel.GetInnerOrderDatas(item => item.IsMe == true);
            
            DealDelete(refList, "是否撤销本人全部报价？");
        }

        private void ref_best_Click(object sender, RoutedEventArgs e)
        {
            List<InnerOrderData> refList = new List<InnerOrderData>();
            if (this.InnerBoard_Best_dataGrid_IRS.SelectedItems.Count > 0)
            {
                foreach (InnerOrderData item in this.InnerBoard_Best_dataGrid_IRS.SelectedItems)
                {
                    string securityid = item.SecurityID;
                    IEnumerable<InnerOrderData> arr = this._innerBoardViewModel.GetInnerOrderDatas(obj => obj.SecurityID == securityid && obj.IsMe);
                    refList.AddRange(arr);
                }
            }
            if (refList.Count > 0)
            {
                DealDelete(refList);
            }
        }

        private void InnerBoard_dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (InnerOrderData data in e.AddedItems)
            {
                data.PropertyChanged += Data_PropertyChanged;
            }

            foreach (InnerOrderData data in e.RemovedItems)
            {
                data.PropertyChanged -= Data_PropertyChanged;
            }
            string isMe = IsMe_SelectedItem();
            if (isMe == "1" || isMe == "3")
            {
                this.refBtn.IsEnabled = true;
            }
            else
            {
                this.refBtn.IsEnabled = false;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                this.InnerBoard_dataGrid_IRS.CopyData_Single();
            }
        }

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsMe")
            {
                string isMe = IsMe_SelectedItem();
                if (isMe == "1" || isMe == "3")
                {
                    this.refBtn.IsEnabled = true;
                }
                else
                {
                    this.refBtn.IsEnabled = false;
                }
            }
        }

        private void InnerBoard_Best_dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(InnerOrderData data in e.AddedItems)
            {
                data.PropertyChanged += Data_Best_PropertyChanged;
            }

            foreach(InnerOrderData data in e.RemovedItems)
            {
                data.PropertyChanged -= Data_Best_PropertyChanged;
            }

            string isMe_bid = IsMe_SelectedItem_Best("1");
            string isMe_ofr = IsMe_SelectedItem_Best("2");
            if (isMe_bid == "1" || isMe_bid == "3" || isMe_ofr == "1" || isMe_ofr == "3")
            {
                this.refBestBtn.IsEnabled = true;
            }
            else
            {
                this.refBestBtn.IsEnabled = false;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                InnerBoard_Best_dataGrid_IRS.CopyData_Best();
            }
        }

        private void Data_Best_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsMe_InnerBid"
                || e.PropertyName == "IsMe_InnerOfr")
            {
                string isMe_bid = IsMe_SelectedItem_Best("1");
                string isMe_ofr = IsMe_SelectedItem_Best("2");
                if (isMe_bid == "1" || isMe_bid == "3" || isMe_ofr == "1" || isMe_ofr == "3")
                {
                    this.refBestBtn.IsEnabled = true;
                }
                else
                {
                    this.refBestBtn.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// 打开模糊需求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFuzzyRequire_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist("InnerFuzzyRequireView"))
            {
                InnerFuzzyRequireView win = new InnerFuzzyRequireView();
                WindowManager.Instance.OpenOnlyOneWindow("InnerFuzzyRequireView", win);
            }
        }

        private void ProfileListBox_Best_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
        }

        private void ProfileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
        }

        #region 记忆窗口拆分布局
        private void gridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            SaveLastExecutionsHeight();
        }

        public void SaveLastExecutionsHeight()
        {
            bool isLite = AdminHandler.Instance.CheckPermission("5007", "500701");
            if (!isLite)
            {
                if (this.DataContext != null)
                {
                    IniFileHelper.WriteValue($"IRSInnerBoardView_{ LocalEnv.UserInfo.UserName}", "InnerBoardViewWidth", Row0.Width.ToString());
                    IniFileHelper.WriteValue($"IRSInnerBoardView_{ LocalEnv.UserInfo.UserName}", "PriceAndOrderViewWidth", Row2.Width.ToString());
                    if (Row2.Width.Value > 0)
                    {
                        btnVer.Content = ">";
                    }
                    IniFileHelper.WriteValue($"IRSInnerBoardView_{ LocalEnv.UserInfo.UserName}", "ExpandButtonCollapsed", btnVer.Content.ToString() == "<" ? "1" : "0");
                }
            }
        }

        private void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            //未收缩
            if (btnVer.Content.ToString() == ">")
            {
                Row0.Width = new GridLength(1, GridUnitType.Star);
                Row2.Width = new GridLength(0);
                btnVer.Content = "<";
            }
            else
            {
                Row0.Width = new GridLength(1, GridUnitType.Star);
                Row2.Width = new GridLength(300, GridUnitType.Pixel);
                btnVer.Content = ">";
            }

            SaveLastExecutionsHeight();
            IniFileHelper.RefreshIni();
        }

        private async void SetExecutionsWidth()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                bool isLite = AdminHandler.Instance.CheckPermission("5007", "500701");
                    if (isLite)
                    {
                        Row1.Width = new GridLength(0, GridUnitType.Star);
                        Row2.Width = new GridLength(0, GridUnitType.Star);
                    }
                    else
                    {
                        string expandbettoncollapsed = IniFileHelper.ReadValue($"IRSInnerBoardView_{ LocalEnv.UserInfo.UserName}", "ExpandButtonCollapsed");
                        if (!string.IsNullOrEmpty(expandbettoncollapsed))
                        {
                            btnVer.Content = expandbettoncollapsed == "1" ? "<" : ">";
                            if (expandbettoncollapsed == "1")
                            {
                                Row0.Width = new GridLength(1, GridUnitType.Star);
                                Row2.Width = new GridLength(0);
                            }
                            else
                            {
                                string height = IniFileHelper.ReadValue($"IRSInnerBoardView_{ LocalEnv.UserInfo.UserName}", "InnerBoardViewWidth");
                                double d = 0;
                                string width = IniFileHelper.ReadValue($"IRSInnerBoardView_{ LocalEnv.UserInfo.UserName}", "PriceAndOrderViewWidth");
                                if (!string.IsNullOrEmpty(height) && !string.IsNullOrEmpty(width))
                                {
                                    if (height.Contains("*"))
                                    {
                                        height = height.Replace("*", "");
                                        if (string.IsNullOrEmpty(height))
                                            Row0.Width = new GridLength(1, GridUnitType.Star);
                                        else
                                            Row0.Width = new GridLength(d, GridUnitType.Star);
                                    }
                                    else
                                    {
                                        if (double.TryParse(height, out d))
                                            Row0.Width = new GridLength(d, GridUnitType.Pixel);
                                    }

                                    if (width.Contains("*"))
                                    {
                                        width = width.Replace("*", "");
                                        if (string.IsNullOrEmpty(width))
                                            Row2.Width = new GridLength(1, GridUnitType.Star);
                                        else
                                            Row2.Width = new GridLength(d, GridUnitType.Star);
                                    }
                                    else
                                    {
                                        if (double.TryParse(width, out d))
                                            Row2.Width = new GridLength(d, GridUnitType.Pixel);
                                    }
                                }
                                else
                                {
                                    Row0.Width = new GridLength(1, GridUnitType.Star);
                                    Row2.Width = new GridLength(300, GridUnitType.Pixel);
                                }
                            }
                        }

                        Pov.Visibility = Visibility.Visible;
                    }
                }));
            });
        }

        private void innerBoard_Loaded(object sender, RoutedEventArgs e)
        {
            SetExecutionsWidth();
        }
        #endregion
    }
}
