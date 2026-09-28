using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Order;
using STC.InnerBoard.Plugin.View.Component;
using STC.Common.View.Order.InnerPanel;
using STC.InnerBoard.Plugin.ViewModel;
using STC.InnerBoard.Plugin.ViewModel.Data;
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
using STC.WorkBench.Plugin.InnerOrders;
using STC.Common.ColumnSetting;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using STC.Common.View.FQ;
using System.Threading;
using STC.Common.Model.FinQ;
using STC.Common.Handler.FinQ;
using STC.Common.ViewModel.Data;
using STC.Common.Model.Admin;

namespace STC.InnerBoard.Plugin.View
{
    /// <summary>
    /// InnerBoardView.xaml 的交互逻辑
    /// </summary>
    public partial class InnerBoardView : UserControl
    {
        private string _TRADEBOOK = "Tradebook";
        private InnerBoardViewModel _innerBoardViewModel;

        private string _oldSecuityID = "Tradebook";

        private string MODULEID_BONDSUBTYPE = "5005";
        private string RATE = "500502";
        private string CREDIT = "500503";

        public Common.Interface.IPriceAndOrderViewInterface dgPrice;

        public InnerBoardView()
        {
            InitializeComponent();

            if (!LocalEnv.ConfInfo.IsRiskCheck_Sell)
            {
                this.InnerBoard_Best_dataGrid.Columns.Remove(RiskInfoColumn_best);
                this.InnerBoard_dataGrid.Columns.Remove(RiskInfoColumn);

                this.RiskCB_Best_Tb.Visibility = Visibility.Collapsed;
                this.RiskCB_Best.Visibility = Visibility.Collapsed;
                this.RiskCB_Tb.Visibility = Visibility.Collapsed;
                this.RiskCB.Visibility = Visibility.Collapsed;
            }
            else
            {
                Dictionary<string, object> dic_best = new Dictionary<string, object>();
                dic_best["无风险"] = "无风险";
                dic_best["黑白名单"] = "黑白名单";
                dic_best["集中度"] = "集中度";
                dic_best["授信"] = "授信";
                dic_best["内部评级"] = "内部评级";
                dic_best["主体限额"] = "主体限额";
                RiskCB_Best.ItemsSource = dic_best;
                RiskCB_Best.FilterDicChanged += RiskCB_Best_FilterDicChanged;


                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic["无风险"] = "无风险";
                dic["黑白名单"] = "黑白名单";
                dic["集中度"] = "集中度";
                dic["授信"] = "授信";
                dic["内部评级"] = "内部评级";
                dic["主体限额"] = "主体限额";
                RiskCB.ItemsSource = dic;
                RiskCB.FilterDicChanged += RiskCB_FilterDicChanged;
            }

            if (!LocalEnv.ConfInfo.IsQQ)
            {
                StartQQMenu_Best.Visibility = Visibility.Collapsed;
                StartQQ2Menu_Best.Visibility = Visibility.Collapsed;
                StartQQMenu_Trader.Visibility = Visibility.Collapsed;
                StartQQ2Menu_Trader.Visibility = Visibility.Collapsed;
            }

            if (LocalEnv.ConfInfo.IsFQ && AdminHandler.Instance.CheckPermisionById(CommonConstants.MODELID_5000, CommonConstants.CODE_5002))
            {
                this.BestSendBtn.Visibility = Visibility.Visible;
                this.SingleSendBtn.Visibility = Visibility.Visible;

                this.BestCopyBtn.Content = "仅复制";
                this.SingleCopyBtn.Content = "仅复制";
            }

            if (CommonUtils.IsBDBAppName())
            {
                dgPrice = DesignHandler.Instance.CreatePriceAndOrderView_BDB();
                ToQbMenu.Visibility = Visibility.Visible;
                ToQbMenu_Best.Visibility = Visibility.Visible;
            }
            else
            {
                dgPrice = new Common.View.Card.PriceAndOrderView();
            }


            dgPrice.InitTableModel(_oldSecuityID, "STC");
            Pov.Child = dgPrice as UIElement;

            //if (Timer4ControlDetailPopup == null)
            //{
            //    this.Timer4ControlDetailPopup = new DispatcherTimer();
            //    this.Timer4ControlDetailPopup.Interval = new TimeSpan(500);
            //    this.Timer4ControlDetailPopup.Tick += ControlPopup;
            //}
            //if (Timer4Refresh == null)
            //{
            //    Timer4Refresh = new DispatcherTimer();
            //    Timer4Refresh.Interval = new TimeSpan(0, 0, 0, 0, 300);
            //    Timer4Refresh.Tick += Timer4Refresh_Tick;
            //    Timer4Refresh.Start();
            //}

            // todo
            //dgPrice.RemoveDbDoubleClickEvent();
            //dgPrice.Init(_oldSecuityID);
            this.InnerBoard_dataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.InnerBoard_dataGrid_MouseLeftButtonDown), true);
            this.InnerBoard_Best_dataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.InnerBoard_Beat_dataGrid_MouseLeftButtonDown), true);

            MenuItem sortUpdateTimeMi = new MenuItem();
            sortUpdateTimeMi.Name = "sortUpdateTimeColumnMi";
            sortUpdateTimeMi.Header = "按最后更新时间排序";
            sortUpdateTimeMi.Style = (Style)this.FindResource("DefaultMenuItem");
            sortUpdateTimeMi.Click += sortUpdateTimeColumnMi_Click;
            this.InnerBoard_dataGrid.GetHeaderContextMenu().Items.Add(sortUpdateTimeMi);

            MenuItem defaultTableColumnMi = new MenuItem();
            defaultTableColumnMi.Name = "defaultTableColumnMi";
            defaultTableColumnMi.Header = "恢复默认";
            defaultTableColumnMi.Style = (Style)this.FindResource("DefaultMenuItem");
            defaultTableColumnMi.Click += DefaultTableColumnMi_Click; ;
            this.InnerBoard_dataGrid.GetHeaderContextMenu().Items.Add(defaultTableColumnMi);

            MenuItem sortUpdateTimeColumnMi_Best = new MenuItem();
            sortUpdateTimeColumnMi_Best.Name = "sortUpdateTimeColumnMi_Best";
            sortUpdateTimeColumnMi_Best.Header = "按最后更新时间排序";
            sortUpdateTimeColumnMi_Best.Style = (Style)this.FindResource("DefaultMenuItem");
            sortUpdateTimeColumnMi_Best.Click += sortUpdateTimeColumnMi_Best_Click;
            this.InnerBoard_Best_dataGrid.GetHeaderContextMenu().Items.Add(sortUpdateTimeColumnMi_Best);

            MenuItem defaultTableColumnMi_Best = new MenuItem();
            defaultTableColumnMi_Best.Name = "defaultTableColumnMi_Best";
            defaultTableColumnMi_Best.Header = "恢复默认";
            defaultTableColumnMi_Best.Style = (Style)this.FindResource("DefaultMenuItem");
            defaultTableColumnMi_Best.Click += DefaultTableColumnMi_Best_Click; ;
            this.InnerBoard_Best_dataGrid.GetHeaderContextMenu().Items.Add(defaultTableColumnMi_Best);


        }

        private void RiskCB_Best_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;

            string[] orgArr = null;
            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            _innerBoardViewModel.ProcessRiskFilterData_Best(orgArr);
        }

        private void RiskCB_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;

            string[] orgArr = null;
            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            _innerBoardViewModel.ProcessRiskFilterData(orgArr);
        }

        private void DefaultTableColumnMi_Best_Click(object sender, RoutedEventArgs e)
        {
            if (InnerBoard_Best_dataGrid != null)
            {
                // 删除记录
                InnerBoard_Best_dataGrid.DeleteColumnInfo();

                SettingSyncHandler.Instance.LoadDefaultColumns(InnerBoard_Best_dataGrid.GetName());
            }
        }

        private void DefaultTableColumnMi_Click(object sender, RoutedEventArgs e)
        {
            if (InnerBoard_dataGrid != null)
            {
                // 删除记录
                InnerBoard_dataGrid.DeleteColumnInfo();

                SettingSyncHandler.Instance.LoadDefaultColumns(InnerBoard_dataGrid.GetName());
            }
        }

        private void sortUpdateTimeColumnMi_Best_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_Best_dataGrid.Items.SortDescriptions.Clear();
            this.InnerBoard_Best_dataGrid.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("UpdateTime", System.ComponentModel.ListSortDirection.Descending));
        }

        private void sortUpdateTimeColumnMi_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_dataGrid.Items.SortDescriptions.Clear();
            this.InnerBoard_dataGrid.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("UpdateTime", System.ComponentModel.ListSortDirection.Descending));
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
            Search_Single("SecurityID");
        }

        private void Search_Single(string columnName)
        {
            if (_innerBoardViewModel.SelectedItem_MyInnerOrders != null)
            {
                if (columnName == "SecurityID")
                {
                    _innerBoardViewModel.SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID;
                }
                else if (columnName == "PartySubName")
                {
                    _innerBoardViewModel.SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders.PartySubName;
                }
                else if (columnName == "IssuerName")
                {
                    _innerBoardViewModel.SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders.IssuerName;
                }
                else if (columnName == "WarranterName")
                {
                    _innerBoardViewModel.SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders.WarranterName;
                }

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

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (this.InnerBoard_dataGrid.SelectedItems.Count > 0)
                {
                    this.InnerBoard_dataGrid.CopyData_Single();
                }
            }
        }

        internal void MyOrderDefaultView_SelectedRowChanged(ViewModel.Data.InnerOrderData obj)
        {
            string securityObj = string.Empty;
            if (CommonUtils.IsBDBAppName())
            {
                if (obj != null)
                {
                    securityObj = obj.SecurityID;
                }
                string settle = "全";
                string id = securityObj;
                dgPrice.SelectionChange("STC", id, securityObj, settle, false);
                _oldSecuityID = securityObj;
            }
            else
            {
                bool isinit = false;
                if (obj != null)
                {
                    securityObj = obj.SecurityID;
                    if (!string.IsNullOrEmpty(securityObj) && _oldSecuityID != securityObj)
                    {
                        isinit = true;
                        dgPrice.InitTableModel(securityObj, "STC");
                        _oldSecuityID = securityObj;
                    }
                }
                else
                {
                    isinit = true;
                    dgPrice.InitTableModel(securityObj, "STC");
                    _oldSecuityID = securityObj;
                }

                if (!isinit)
                {
                    dgPrice.InitTableModel(securityObj, "STC");
                    _oldSecuityID = securityObj;
                }
            }
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
        DataGridRow _lastRow = null;
        private void InnerBoard_Beat_dataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;
            try
            {
                var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
                if (hit != null)
                {
                    DependencyObject hitInfo = VisualTreeHelper.GetParent(hit.VisualHit);
                    while (hitInfo != null && !(hitInfo is System.Windows.Controls.DataGridRow))
                        hitInfo = VisualTreeHelper.GetParent(hitInfo);

                    dgr = hitInfo as DataGridRow;
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }

            i += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i = 0; UIElement_Click(sender, e); isDouble = false; };
            timer.IsEnabled = true;
            if (i > 0 && i % 2 == 0)
            {
                timer.IsEnabled = false;
                i = 0;
                isDouble = true;

                //双击时执行的代码
                //MessageBox.Show("DoubleClick!");

                try
                {
                    if (_lastRow != dgr)
                        return;

                    this.InnerDepthPopup.IsOpen = false;
                    if (isBidColumn())
                    {
                        InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;
                        if (innerOrderData.Count_InnerBid > 0)
                        {
                            List<InnerOrderData> bidList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "1", _innerBoardViewModel.bidPriceComparer);
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

                            SendRequireView sendRequireView = new SendRequireView(securityID, true);

                            sendRequireView.Show();
                        }
                    }
                    if (isOfrColumn())
                    {
                        InnerOrderData innerOrderData = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best;

                        if (innerOrderData.Count_InnerOfr > 0)
                        {
                            List<InnerOrderData> ofrList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "2", _innerBoardViewModel.ofrPriceComparer);
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

                            SendRequireView sendRequireView = new SendRequireView(securityID, false);

                            sendRequireView.Show();
                        }
                    }
                    if (InnerBoard_Best_dataGrid.CurrentColumn != null)
                    {
                        if (InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "TraderB")
                        {
                            //Search_Best("PartySubName_InnerBid");
                            StartQQMenu_Trader_Click(null, null);
                        }
                        else if (InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "TraderO")
                        {
                            //Search_Best("PartySubName_InnerOfr");
                            StartQQMenu_Trader_Click(null, null);
                        }
                        else if (InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "ClientB")
                        {
                            StartQQMenu_Best_Click(null, null);
                        }
                        else if (InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "ClientO")
                        {
                            StartQQMenu_Best_Click(null, null);
                        }
                        else if (InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "发行人")
                        {
                            Search_Best("IssuerName");
                        }
                        else if (InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "担保人")
                        {
                            Search_Best("WarranterName");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.Error(ex);
                }
            }
            else
            {
                _lastRow = dgr;
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
                if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 0)
                {
                    this.InnerBoard_Best_dataGrid.CopyData_Best();
                }
            }

            bool InnerDepthPopup_IsOpen = false;
            if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
            {
                bool flag = isDataGridRow(this.InnerBoard_Best_dataGrid, e.GetPosition(this.InnerBoard_Best_dataGrid));
                if (flag && this.InnerBoard_Best_dataGrid.CurrentColumn != null && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID != "99999"
                    && (this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Bid"
                    || this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Ofr"
                    || this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "No.Bid"
                    || this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "No.Ofr"
                    || this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Vol.Bid"
                    || this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Vol.Ofr"
                    || this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "TraderB"
                    || this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "TraderO"
                    )
                    && isNoReply_best == false)
                {
                    DataGridCell c = this.InnerBoard_Best_dataGrid.CurrentColumn.GetCellContent(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best).Parent as DataGridCell;
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

                    InnerDepthPopup_IsOpen = true;
                    
                }
                //else
                //{
                //    InnerBoard_Best_dataGrid_LostFocus(null, null);
                //}
            }
            this.InnerDepthPopup.IsOpen = InnerDepthPopup_IsOpen;
            isNoReply_best = false;
        }

        private string previousTabItem;
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool flag = false;
            if (e.Source is TabControl)
            {
                TabControl tc = e.Source as TabControl;
                if (tc.Name == "InnerBoard_Bond_TC")
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                return;
            }

            // 获取当前选项卡
            TabControl tabControl = sender as TabControl;
            string currentTabItem = string.Empty;

            if (tabControl.SelectedItem != null)
                currentTabItem = (tabControl.SelectedItem as TabItem).Name;

            if (this.singleTab.IsSelected)
            {
                this.countBar.Visibility = Visibility.Visible;
                this.countBar_best.Visibility = Visibility.Collapsed;
                this.countBar_H.Visibility = Visibility.Collapsed;
                if (_innerBoardViewModel.SelectedItem_MyInnerOrders == null)
                {
                    if (this.InnerBoard_dataGrid.Items != null && this.InnerBoard_dataGrid.Items.Count > 0)
                    {
                        _innerBoardViewModel.SelectedItem_MyInnerOrders = (InnerOrderData)this.InnerBoard_dataGrid.Items[0];
                    }
                }
                MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders);
                
            }
            else if (this.bestTab.IsSelected)
            {
                this.countBar.Visibility = Visibility.Collapsed;
                this.countBar_best.Visibility = Visibility.Visible;
                this.countBar_H.Visibility = Visibility.Collapsed;
                if (_innerBoardViewModel.SelectedItem_MyInnerOrders_Best == null)
                {
                    if (this.InnerBoard_Best_dataGrid.Items != null && this.InnerBoard_Best_dataGrid.Items.Count > 0)
                    {
                        _innerBoardViewModel.SelectedItem_MyInnerOrders_Best = (InnerOrderData)this.InnerBoard_Best_dataGrid.Items[0];
                    }
                }
                MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best);
            }
            else if (this.historyreqTab.IsSelected)
            {
                bool isNeedSearch = false;
                if (previousTabItem == null)
                {
                    isNeedSearch = true;
                }

                if (previousTabItem != null)
                {
                    if (previousTabItem != currentTabItem)
                    {
                        isNeedSearch = true;
                    }
                }

                if (isNeedSearch)
                {
                    this.countBar.Visibility = Visibility.Collapsed;
                    this.countBar_best.Visibility = Visibility.Collapsed;
                    this.countBar_H.Visibility = Visibility.Visible;
                }
            }


            // 更新前一个选项卡
            previousTabItem = currentTabItem;
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
            var item = this.InnerBoard_dataGrid.ItemContainerGenerator.ContainerFromItem(innerOrderData);
            (item as DataGridRow).IsSelected = true;
            Common.ViewModel.TradeBookHandler.TBookViewHandler.Instance.OpenQuotesMiniWindows(innerOrderData.SecurityID);
        }

        private void best_opensecurity_btn_Click(object sender, RoutedEventArgs e)
        {
            // 选中Item
            InnerOrderData innerOrderData = (sender as Button).DataContext as InnerOrderData;
            var item = this.InnerBoard_Best_dataGrid.ItemContainerGenerator.ContainerFromItem(innerOrderData);
            (item as DataGridRow).IsSelected = true;
            Common.ViewModel.TradeBookHandler.TBookViewHandler.Instance.OpenQuotesMiniWindows(innerOrderData.SecurityID);
        }

        private void ShowRecognizeWin_Click(object sender, RoutedEventArgs e)
        {
            DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(null);

            WriteDBLogHelper.Instance.GenRecord("inner", "002", "批量识别");
        }


        private void SearchMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            Search_Best("SecurityID");
        }

        private void Search_Best(string columnName)
        {
            if (_innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
            {
                if (columnName == "SecurityID")
                {
                    _innerBoardViewModel.Best_SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
                }
                else if (columnName == "PartySubName_InnerBid")
                {
                    _innerBoardViewModel.Best_SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.PartySubName_InnerBid;
                }
                else if (columnName == "PartySubName_InnerOfr")
                {
                    _innerBoardViewModel.Best_SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.PartySubName_InnerOfr;
                }
                else if (columnName == "IssuerName")
                {
                    _innerBoardViewModel.Best_SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.IssuerName;
                }
                else if (columnName == "WarranterName")
                {
                    _innerBoardViewModel.Best_SearchText = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.WarranterName;
                }
                this.Search_TextBox_Best.SetPop(false);
            }
            else
            {
                LogHandler.Info("IntentionPanel DeleteOrder 没有取到数据");
            }
        }

        private void CopyMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_Best_dataGrid.CopyData_Best();

            WriteDBLogHelper.Instance.GenRecord("inner", "003", "批量-复制");
        }

        private void CopyMenu_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_dataGrid.CopyData_Single();

            WriteDBLogHelper.Instance.GenRecord("inner", "003", "批量-复制");
        }

        private void Single_DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();

            if (columnHeader == "报价(%)"
                || columnHeader == "报量(万元)"
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

                    if (CommonUtils.IsBDBAppName())
                    {
                        CommonUtils.OpenSmallBondWin(_innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID);
                    }
                }
            }
            else if (columnHeader == "本方交易员")
            {
                Search_Single("PartySubName");
            }
            else if (columnHeader == "发行人")
            {
                Search_Single("IssuerName");
            }
            else if (columnHeader == "担保人")
            {
                Search_Single("WarranterName");
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

                    if (CommonUtils.IsBDBAppName())
                    {
                        CommonUtils.OpenSmallBondWin(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID);
                    }
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
                    List<InnerOrderData> bidList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "1", _innerBoardViewModel.bidPriceComparer);
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
                    List<InnerOrderData> ofrList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "2", _innerBoardViewModel.ofrPriceComparer);
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
                if (_innerBoardViewModel._flashDic_best.Count > 0)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (InnerOrderData ib in _innerBoardViewModel._flashDic_best.Values)
                        {
                            DataGridRow targetRow = this.InnerBoard_Best_dataGrid.ItemContainerGenerator.ContainerFromItem(ib) as DataGridRow;

                            if (targetRow != null)
                            {
                                RefreshRow(targetRow);
                            }
                            InnerOrderData removeData;

                            string key = ib.SecurityID;
                            if (ib.SecurityID == "99999")
                            {
                                key = $"{ib.SecurityID}_{ib.OrderID}";
                            }

                            _innerBoardViewModel._flashDic_best.TryRemove(ib.SecurityID, out removeData);
                        }
                    }));
                }

                //lock (_innerBoardViewModel._dataLock_best)
                //{
                //    IEnumerable<InnerOrderData> flashDatas = _innerBoardViewModel.FindInnerOrderDatas_Best();
                //    foreach (InnerOrderData item in flashDatas)
                //    {

                //        if (item.bestbidFlashCount > 0)
                //        {
                //            if (item.bestbidFlashCount == 4)
                //                item.Best_Bid_Opacity = 0.5;
                //            else if (item.bestbidFlashCount == 3)
                //                item.Best_Bid_Opacity = 0;
                //            else if (item.bestbidFlashCount == 2)
                //                item.Best_Bid_Opacity = 0.5;
                //            else
                //                item.Best_Bid_Opacity = 1;
                //            item.bestbidFlashCount--;
                //        }
                //        if (item.bestofrFlashCount > 0)
                //        {
                //            if (item.bestofrFlashCount == 4)
                //                item.Best_Ofr_Opacity = 0.5;
                //            else if (item.bestofrFlashCount == 3)
                //                item.Best_Ofr_Opacity = 0;
                //            else if (item.bestofrFlashCount == 2)
                //                item.Best_Ofr_Opacity = 0.5;
                //            else
                //                item.Best_Ofr_Opacity = 1;
                //            item.bestofrFlashCount--;
                //        }
                //    }
                //}

                //lock (_innerBoardViewModel._dataLock)
                //{
                //    IEnumerable<InnerOrderData> flashDatas = _innerBoardViewModel.FindInnerOrderDatas();
                //    foreach (InnerOrderData item in flashDatas)
                //    {
                //        item.bestbidFlashCount = 0;
                //        item.bestofrFlashCount = 0;
                //        //item.FlashCount = 0;
                //    }
                //}
            }
            else
            {
                if (_innerBoardViewModel._flashDic.Count > 0)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (InnerOrderData ib in _innerBoardViewModel._flashDic.Values)
                        {
                            DataGridRow targetRow = this.InnerBoard_dataGrid.ItemContainerGenerator.ContainerFromItem(ib) as DataGridRow;

                            if (targetRow != null)
                            {
                                RefreshRow(targetRow);
                            }
                            else
                            {

                            }

                            InnerOrderData removeData;
                            _innerBoardViewModel._flashDic.TryRemove(ib.OrderID, out removeData);
                        }
                    }));
                }

                //lock (_innerBoardViewModel._dataLock)
                //{
                //    IEnumerable<InnerOrderData> flashDatas = _innerBoardViewModel.FindInnerOrderDatas();

                //    foreach (InnerOrderData item in flashDatas)
                //    {
                //        //if (item.FlashCount > 0)
                //        //    item.FlashCount--;
                //        if (item.bestbidFlashCount > 0)
                //        {
                //            if (item.bestbidFlashCount == 4)
                //                item.Best_Bid_Opacity = 0.5;
                //            else if (item.bestbidFlashCount == 3)
                //                item.Best_Bid_Opacity = 0;
                //            else if (item.bestbidFlashCount == 2)
                //                item.Best_Bid_Opacity = 0.5;
                //            else
                //                item.Best_Bid_Opacity = 1;
                //            item.bestbidFlashCount--;
                //        }
                //        if (item.bestofrFlashCount > 0)
                //        {
                //            if (item.bestofrFlashCount == 4)
                //                item.Best_Ofr_Opacity = 0.5;
                //            else if (item.bestofrFlashCount == 3)
                //                item.Best_Ofr_Opacity = 0;
                //            else if (item.bestofrFlashCount == 2)
                //                item.Best_Ofr_Opacity = 0.5;
                //            else
                //                item.Best_Ofr_Opacity = 1;
                //            item.bestofrFlashCount--;
                //        }
                //    }

                //}

                //lock (_innerBoardViewModel._dataLock_best)
                //{
                //    IEnumerable<InnerOrderData> flashDatas = _innerBoardViewModel.FindInnerOrderDatas_Best();
                //    foreach (InnerOrderData item in flashDatas)
                //    {
                //        item.bestbidFlashCount = 0;
                //        item.bestofrFlashCount = 0;
                //        //item.FlashCount = 0;
                //    }
                //}
            }

            //foreach (ConcurrentDictionary<string, InnerOrderData> pair in _innerBoardViewModel._Cache_InnerOrders_Dic.Values)
            //{
            //    foreach (InnerOrderData item in pair.Values)
            //    {
            //        if (item.bestbidFlashCount > 0)
            //        {
            //            if (item.bestbidFlashCount == 4)
            //                item.Best_Bid_Opacity = 0.5;
            //            else if (item.bestbidFlashCount == 3)
            //                item.Best_Bid_Opacity = 0;
            //            else if (item.bestbidFlashCount == 2)
            //                item.Best_Bid_Opacity = 0.5;
            //            else
            //                item.Best_Bid_Opacity = 1;
            //            item.bestbidFlashCount--;
            //        }
            //        if (item.bestofrFlashCount > 0)
            //        {
            //            if (item.bestofrFlashCount == 4)
            //                item.Best_Ofr_Opacity = 0.5;
            //            else if (item.bestofrFlashCount == 3)
            //                item.Best_Ofr_Opacity = 0;
            //            else if (item.bestofrFlashCount == 2)
            //                item.Best_Ofr_Opacity = 0.5;
            //            else
            //                item.Best_Ofr_Opacity = 1;
            //            item.bestofrFlashCount--;
            //        }
            //    }
            //}

            //foreach (InnerOrderData item in _innerBoardViewModel._Cache_InnerOrders_Dic_Best.Values)
            //{
            //    if (item.bestbidFlashCount > 0)
            //    {
            //        if (item.bestbidFlashCount == 4)
            //            item.Best_Bid_Opacity = 0.5;
            //        else if (item.bestbidFlashCount == 3)
            //            item.Best_Bid_Opacity = 0;
            //        else if (item.bestbidFlashCount == 2)
            //            item.Best_Bid_Opacity = 0.5;
            //        else
            //            item.Best_Bid_Opacity = 1;
            //        item.bestbidFlashCount--;
            //    }
            //    if (item.bestofrFlashCount > 0)
            //    {
            //        if (item.bestofrFlashCount == 4)
            //            item.Best_Ofr_Opacity = 0.5;
            //        else if (item.bestofrFlashCount == 3)
            //            item.Best_Ofr_Opacity = 0;
            //        else if (item.bestofrFlashCount == 2)
            //            item.Best_Ofr_Opacity = 0.5;
            //        else
            //            item.Best_Ofr_Opacity = 1;
            //        item.bestofrFlashCount--;
            //    }
            //}

            //foreach (ConcurrentDictionary<string, InnerOrderData> pair in _innerBoardViewModel._Cache_InnerOrders_Dic.Values)
            //{
            //    foreach (InnerOrderData item in pair.Values)
            //    {
            //        if (item.bestbidFlashCount > 0)
            //        {
            //            if (item.bestbidFlashCount == 4)
            //                item.Best_Bid_Opacity = 0.5;
            //            else if (item.bestbidFlashCount == 3)
            //                item.Best_Bid_Opacity = 0;
            //            else if (item.bestbidFlashCount == 2)
            //                item.Best_Bid_Opacity = 0.5;
            //            else
            //                item.Best_Bid_Opacity = 1;
            //            item.bestbidFlashCount--;
            //        }
            //        if (item.bestofrFlashCount > 0)
            //        {
            //            if (item.bestofrFlashCount == 4)
            //                item.Best_Ofr_Opacity = 0.5;
            //            else if (item.bestofrFlashCount == 3)
            //                item.Best_Ofr_Opacity = 0;
            //            else if (item.bestofrFlashCount == 2)
            //                item.Best_Ofr_Opacity = 0.5;
            //            else
            //                item.Best_Ofr_Opacity = 1;
            //            item.bestofrFlashCount--;
            //        }
            //    }
            //}

            if (this._innerBoardViewModel.VerticalOffset > 0)
            {
                if (this._innerBoardViewModel.UpdateDataCount > 0)
                {
                    this.returnTopBtn.Visibility = Visibility.Collapsed;
                    this.returnTopBtn_refresh.Visibility = Visibility.Visible;
                }
                else
                {
                    this.returnTopBtn.Visibility = Visibility.Visible;
                    this.returnTopBtn_refresh.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.returnTopBtn.Visibility = Visibility.Collapsed;
                this.returnTopBtn_refresh.Visibility = Visibility.Collapsed;

            }

            if (this._innerBoardViewModel.VerticalOffset_Best > 0)
            {
                if (this._innerBoardViewModel.UpdateDataCount_Best > 0)
                {
                    this.returnTopBtn_best.Visibility = Visibility.Collapsed;
                    this.returnTopBtn_refresh_best.Visibility = Visibility.Visible;
                }
                else
                {
                    this.returnTopBtn_best.Visibility = Visibility.Visible;
                    this.returnTopBtn_refresh_best.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.returnTopBtn_best.Visibility = Visibility.Collapsed;
                this.returnTopBtn_refresh_best.Visibility = Visibility.Collapsed;
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

                _innerBoardViewModel.IsCoupon = _innerBoardViewModel.SelectedItem_Tag != null ? _innerBoardViewModel.SelectedItem_Tag.IsCoupon : false;
                _innerBoardViewModel.StartCoupon = _innerBoardViewModel.SelectedItem_Tag?.StartCoupon;
                _innerBoardViewModel.EndCoupon = _innerBoardViewModel.SelectedItem_Tag?.EndCoupon;
                _innerBoardViewModel.StartTermDY = _innerBoardViewModel.SelectedItem_Tag?.StartTermDY;
                _innerBoardViewModel.EndTermDY = _innerBoardViewModel.SelectedItem_Tag?.EndTermDY;
                _innerBoardViewModel.StartPrice = _innerBoardViewModel.SelectedItem_Tag?.StartPrice;
                _innerBoardViewModel.EndPrice = _innerBoardViewModel.SelectedItem_Tag?.EndPrice;
                _innerBoardViewModel.StartChinaBondValue = _innerBoardViewModel.SelectedItem_Tag?.StartChinaBond;
                _innerBoardViewModel.EndChinaBondValue = _innerBoardViewModel.SelectedItem_Tag?.EndChinaBond;
                _innerBoardViewModel.StartBidBp = _innerBoardViewModel.SelectedItem_Tag?.StartBidBp;
                _innerBoardViewModel.EndBidBp = _innerBoardViewModel.SelectedItem_Tag?.EndBidBp;
                _innerBoardViewModel.StartOfrBp = _innerBoardViewModel.SelectedItem_Tag?.StartOfrBp;
                _innerBoardViewModel.EndOfrBp = _innerBoardViewModel.SelectedItem_Tag?.EndOfrBp;

                _innerBoardViewModel.ProcessGroupBonds(_innerBoardViewModel.SelectedItem_Tag?.Inf1, _innerBoardViewModel.SelectedItem_Tag?.FocusGroupId);

                _innerBoardViewModel.IsCoupon_Best = _innerBoardViewModel.SelectedItem_Tag_Best != null ? _innerBoardViewModel.SelectedItem_Tag_Best.IsCoupon : false;
                _innerBoardViewModel.StartCoupon_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.StartCoupon;
                _innerBoardViewModel.EndCoupon_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.EndCoupon;
                _innerBoardViewModel.StartTermDY_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.StartTermDY;
                _innerBoardViewModel.EndTermDY_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.EndTermDY;
                _innerBoardViewModel.StartPrice_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.StartPrice;
                _innerBoardViewModel.EndPrice_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.EndPrice;
                _innerBoardViewModel.StartChinaBondValue_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.StartChinaBond;
                _innerBoardViewModel.EndChinaBondValue_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.EndChinaBond;
                _innerBoardViewModel.StartBidBp_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.StartBidBp;
                _innerBoardViewModel.EndBidBp_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.EndBidBp;
                _innerBoardViewModel.StartOfrBp_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.StartOfrBp;
                _innerBoardViewModel.EndOfrBp_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.EndOfrBp;

                _innerBoardViewModel.ProcessGroupBonds_Best(_innerBoardViewModel.SelectedItem_Tag_Best?.Inf1, _innerBoardViewModel.SelectedItem_Tag_Best?.FocusGroupId);

            }));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetAddTagGroup()))
            {
                AddSubscriptionGroupWindow addGroupWin = new AddSubscriptionGroupWindow(IntentionOldHandler.Instance.GroupList, this._innerBoardViewModel.FocusTags, _innerBoardViewModel.SelectedItem_Tag?.GroupID, this._innerBoardViewModel.StartTermDY, this._innerBoardViewModel.EndTermDY, this._innerBoardViewModel.IsAppTerm,
                    this._innerBoardViewModel.StartCoupon, this._innerBoardViewModel.EndCoupon, this._innerBoardViewModel.IsCoupon,
                    this._innerBoardViewModel.StartPrice, this._innerBoardViewModel.EndPrice,
                    this._innerBoardViewModel.StartChinaBondValue, this._innerBoardViewModel.EndChinaBondValue,
                    this._innerBoardViewModel.StartBidBp, this._innerBoardViewModel.EndBidBp,
                    this._innerBoardViewModel.StartOfrBp, this._innerBoardViewModel.EndOfrBp);
                addGroupWin.AddAction = OnAddAction;
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetAddTagGroup(), addGroupWin);
            }
        }

        private void Save_Best_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetAddTagGroup()))
            {
                AddSubscriptionGroupWindow addGroupWin = new AddSubscriptionGroupWindow(IntentionOldHandler.Instance.GroupList, this._innerBoardViewModel.FocusTags_Best, _innerBoardViewModel.SelectedItem_Tag_Best?.GroupID, this._innerBoardViewModel.StartTermDY_Best, this._innerBoardViewModel.EndTermDY_Best, this._innerBoardViewModel.IsAppTerm_Best,
                    this._innerBoardViewModel.StartCoupon_Best, this._innerBoardViewModel.EndCoupon_Best, this._innerBoardViewModel.IsCoupon_Best,
                    this._innerBoardViewModel.StartPrice_Best, this._innerBoardViewModel.EndPrice_Best,
                    this._innerBoardViewModel.StartChinaBondValue_Best, this._innerBoardViewModel.EndChinaBondValue_Best,
                    this._innerBoardViewModel.StartBidBp_Best, this._innerBoardViewModel.EndBidBp_Best,
                    this._innerBoardViewModel.StartOfrBp_Best, this._innerBoardViewModel.EndOfrBp_Best);
                addGroupWin.AddAction = OnAddAction;
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetAddTagGroup(), addGroupWin);
            }
        }

        private void btnRequire_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string securityID = _TRADEBOOK;
                    if (_innerBoardViewModel != null
                        && _innerBoardViewModel.SelectedItem_MyInnerOrders != null
                        && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID))
                    {
                        securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID;
                    }

                    SendRequireView sendRequireView = new SendRequireView(securityID, false);
                    //SendRequireNewView sendRequireView = new SendRequireNewView(securityID, false);
                    WindowManager.Instance.OpenOnlyOneWindow($"SendRequireView-{securityID}", sendRequireView);
                    //sendRequireView.Show();
                });
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string securityID = _TRADEBOOK;
                    if (_innerBoardViewModel != null
                        && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null
                        && !string.IsNullOrEmpty(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID))
                    {
                        securityID = _innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
                    }

                    SendRequireView sendRequireView = new SendRequireView(securityID, false);
                    //SendRequireNewView sendRequireView = new SendRequireNewView(securityID, false);
                    WindowManager.Instance.OpenOnlyOneWindow($"SendRequireView-{securityID}", sendRequireView);
                    //sendRequireView.Show();
                });

                WriteDBLogHelper.Instance.GenRecord("inner", "001", "新增需求");
            });
        }

        public void EditOrder(InnerOrderData orderData, bool isBid, string securityId, string intentionid)
        {
            string side = isBid ? ComWindowKeyHelper.BID : ComWindowKeyHelper.OFR;

            if (orderData != null)
            {
                PriceMatchParam priceMatchParam = new PriceMatchParam();
                priceMatchParam.CustomFirmAmount = orderData.Qty.ToString(); // OrderQty 订单量
                priceMatchParam.OrderId = orderData.StrategyOrderId;// 订单号
                priceMatchParam.Side = orderData.Side; // 方向
                priceMatchParam.Symbol = orderData.SecurityID; // 代码
                priceMatchParam.SymbolName = orderData.Symbol; // 名称
                priceMatchParam.WholeFlag = orderData.WholeFlag; // 整量标志
                priceMatchParam.NetPrice = orderData.NetPrice?.ToString(); // 净价
                priceMatchParam.Price = orderData.Price.ToString(); // 收益率
                priceMatchParam.PriceType = orderData.PriceType;
                priceMatchParam.SettlType = orderData.SettleType; // 清算速度
                priceMatchParam.NonfirmTime = string.Empty; // nonfirmTime;
                priceMatchParam.OperationType = "1";
                if (orderData.FirmFlag == "1")
                {
                    priceMatchParam.IsFirm = true;// firm
                }
                else
                {
                    priceMatchParam.IsFirm = false;// nonfirm
                }
                priceMatchParam.channel = orderData.Channel;
                priceMatchParam.Info2 = orderData.Info2;
                priceMatchParam.Info1 = orderData.Info1;
                priceMatchParam.InstitutionID = orderData.InstitutionID;
                priceMatchParam.InstitutionName = orderData.InstitutionName;
                priceMatchParam.TraderID = orderData.TraderID;
                priceMatchParam.TraderName = orderData.TraderName;

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
                        if (!LocalEnv.ConfInfo.EnableIOC)
                        {
                            return;
                        }
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
                            Task.Factory.StartNew(() =>
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    SendRequireView sendRequireView = new SendRequireView(priceMatchParam);
                                    WindowManager.Instance.OpenOnlyOneWindow($"EditOrder-{priceMatchParam.IntentionID}", sendRequireView);
                                });
                            });
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

                Task.Factory.StartNew(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SendRequireView sendRequireView = new SendRequireView(securityID, isBid);
                        if (!WindowManager.Instance.IsExist($"Intention_{securityID}"))
                        {
                            WindowManager.Instance.OpenOnlyOneWindow($"Intention_{securityID}", sendRequireView);
                        }
                    });
                });
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
                    SendPointPriceView sendPointPriceView = new SendPointPriceView(innerOrderData.SecurityID, innerOrderData.Side == "1" ? true : false, innerOrderData.Price.ToString(), innerOrderData.Qty.ToString(),
                    innerOrderData.SettleType, innerOrderData.NetPrice.ToString(), "MANUAL",
                    innerOrderData.WholeFlag, innerOrderData.FirmFlag == "1", innerOrderData.StrategyOrderId,
                    null, null, null, null,
                    null, string.Empty, "INNER", null, innerOrderData.IntentionID, isDrp, $"{innerOrderData.RoleName}-{innerOrderData.PartySubName}", null, null, null, null, new Common.ViewModel.Order.DRPParamInfoModel() { OrderId = innerOrderData.OrderID, PriceType = innerOrderData.PriceType, SettlType = innerOrderData.SettleType});


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
                List<InnerOrderData> bidList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "1", _innerBoardViewModel.bidPriceComparer);
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
                List<InnerOrderData> ofrList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "2", _innerBoardViewModel.ofrPriceComparer);
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
            if (this.InnerBoard_dataGrid.SelectedItems.Count > 0)
            {
                foreach (InnerOrderData item in this.InnerBoard_dataGrid.SelectedItems)
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

                Task.Factory.StartNew(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SendRequireView sendRequireView = new SendRequireView(securityID, isBid);
                        if (!WindowManager.Instance.IsExist($"Intention_{securityID}"))
                        {
                            WindowManager.Instance.OpenOnlyOneWindow($"Intention_{securityID}", sendRequireView);
                        }
                    });
                });
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
                if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 0)
                {
                    foreach (InnerOrderData item in this.InnerBoard_Best_dataGrid.SelectedItems)
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
                if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 0)
                {
                    foreach (InnerOrderData item in this.InnerBoard_Best_dataGrid.SelectedItems)
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
            if (this.InnerBoard_dataGrid.SelectedItems.Count > 0)
            {
                int isMeCount = 0;
                int isNotMeCount = 0;
                foreach (InnerOrderData item in this.InnerBoard_dataGrid.SelectedItems)
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
            if (this.InnerBoard_dataGrid.SelectedItems.Count > 1)
            {
                ConsiderationMenu.Visibility = Visibility.Collapsed;
                SearchMenu.Visibility = Visibility.Collapsed;
                PointPriceMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (this.InnerBoard_dataGrid.SelectedItem != null && this.InnerBoard_dataGrid.SelectedItem is InnerOrderData)
                {
                    InnerOrderData innerOrderData = this.InnerBoard_dataGrid.SelectedItem as InnerOrderData;

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
                        if (innerOrderData.IsMe == false)
                        {
                            if (LocalEnv.ConfInfo.EnableIOC)
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
                if (this.InnerBoard_dataGrid.SelectedItems.Count > 1)
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

                if (this.InnerBoard_dataGrid.SelectedItems.Count > 1)
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
                        if (LocalEnv.ConfInfo.EnableIOC)
                        {
                            ReplyPriceMenu.Visibility = Visibility.Visible;
                        }
                        else 
                        {
                            ReplyPriceMenu.Visibility = Visibility.Collapsed;
                        }
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
                this.InnerBoard_Best_dataGrid.CurrentColumn = targetCell.Column;

                if (this.InnerBoard_Best_dataGrid.SelectedItems.Count == 0)
                {
                    DataGridRow targetRow = this.InnerBoard_Best_dataGrid.ItemContainerGenerator.ContainerFromItem(targetCell.DataContext) as DataGridRow;

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
                this.InnerBoard_dataGrid.CurrentColumn = targetCell.Column;

                if (this.InnerBoard_dataGrid.SelectedItems.Count == 0)
                {
                    DataGridRow targetRow = this.InnerBoard_dataGrid.ItemContainerGenerator.ContainerFromItem(targetCell.DataContext) as DataGridRow;

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
            if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 0)
            {
                int isMeCount = 0;
                int isNotMeCount = 0;
                foreach (InnerOrderData item in this.InnerBoard_Best_dataGrid.SelectedItems)
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
            if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 1)
            {
                ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                SearchMenu_Best.Visibility = Visibility.Collapsed;
                PointPriceMenu_Best.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (this.InnerBoard_Best_dataGrid.SelectedItem != null && this.InnerBoard_Best_dataGrid.SelectedItem is InnerOrderData)
                {
                    InnerOrderData data = this.InnerBoard_Best_dataGrid.SelectedItem as InnerOrderData;

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
                        if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 1)
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

                        if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 1)
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
                            if (this.InnerBoard_Best_dataGrid.SelectedItem != null && this.InnerBoard_Best_dataGrid.SelectedItem is InnerOrderData)
                            {
                                InnerOrderData data = this.InnerBoard_Best_dataGrid.SelectedItem as InnerOrderData;

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
                        if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 1)
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
                        if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 1)
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

                            if (this.InnerBoard_Best_dataGrid.SelectedItem != null && this.InnerBoard_Best_dataGrid.SelectedItem is InnerOrderData)
                            {
                                InnerOrderData data = this.InnerBoard_Best_dataGrid.SelectedItem as InnerOrderData;

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

            if (LocalEnv.ConfInfo.IsQQ)
            {
                //StartQQMenu_Best.IsEnabled = false;
                StartQQMenu_Best.Visibility = Visibility.Collapsed;
                //StartQQ2Menu_Best.IsEnabled = false;
                StartQQ2Menu_Best.Visibility = Visibility.Collapsed;
                //StartQQMenu_Trader.IsEnabled = false;
                StartQQMenu_Trader.Visibility = Visibility.Collapsed;
                //StartQQ2Menu_Trader.IsEnabled = false;
                StartQQ2Menu_Trader.Visibility = Visibility.Collapsed;
                if (this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
                {
                    if (this._innerBoardViewModel.Type3_Flag)
                    {
                        if (this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "ClientB")
                        {
                            StartQQMenu_Best.Visibility = Visibility.Visible;
                            StartQQ2Menu_Best.Visibility = Visibility.Visible;
                            //if (!string.IsNullOrEmpty(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.FromQQ_Bid))
                            //{
                            //    StartQQMenu_Best.IsEnabled = true;
                            //    StartQQ2Menu_Best.IsEnabled = true;
                            //}
                        }
                        if (this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "ClientO")
                        {
                            StartQQMenu_Best.Visibility = Visibility.Visible;
                            StartQQ2Menu_Best.Visibility = Visibility.Visible;
                            //if (!string.IsNullOrEmpty(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.FromQQ_Ofr))
                            //{
                            //    StartQQMenu_Best.IsEnabled = true;
                            //    StartQQ2Menu_Best.IsEnabled = true;
                            //}
                        }
                    }

                    if (this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "TraderB")
                    {
                        StartQQMenu_Trader.Visibility = Visibility.Visible;
                        StartQQ2Menu_Trader.Visibility = Visibility.Visible;
                        //if (!this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.IsMe_InnerBid
                        //    && !string.IsNullOrEmpty(CommonUtils.GetQQNumber(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.UserName_InnerBid)))
                        //{
                        //    StartQQMenu_Trader.IsEnabled = true;
                        //    StartQQ2Menu_Trader.IsEnabled = true;
                        //}
                    }

                    if (this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "TraderO")
                    {
                        StartQQMenu_Trader.Visibility = Visibility.Visible;
                        StartQQ2Menu_Trader.Visibility = Visibility.Visible;
                        //if (!this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.IsMe_InnerOfr
                        //     && !string.IsNullOrEmpty(CommonUtils.GetQQNumber(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.UserName_InnerOfr)))
                        //{
                        //    StartQQMenu_Trader.IsEnabled = true;
                        //    StartQQ2Menu_Trader.IsEnabled = true;
                        //}
                    }
                }
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
            //this.InnerDepthPopup.IsOpen = false;
            //CloseDepthPop();
        }


        private void InnerBoard_dataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //this.InnerDepthPopup.IsOpen = false;
            //CloseDepthPop();
            ScrollViewer scrollview = CommonUtils.GetVisualChild<ScrollViewer>((DataGrid)sender, v => v.Name == "DG_ScrollViewer");
            Visibility verticalVisibility = Visibility.Collapsed;
            if (scrollview != null)
            {
                verticalVisibility = scrollview.ComputedVerticalScrollBarVisibility;
            }

            if (verticalVisibility == Visibility.Visible)
            {
                if (e.VerticalOffset == 0 && e.ViewportHeightChange == 0 && e.ViewportWidthChange == 0)
                {
                    this._innerBoardViewModel.UpdateDataCount = 0;
                }

                if (e.ViewportHeightChange == 0 && e.ViewportWidthChange == 0)
                {
                    this._innerBoardViewModel.VerticalOffset = e.VerticalOffset;// + Math.Abs(e.ViewportHeightChange);
                }
            }
            else
            {
                this._innerBoardViewModel.UpdateDataCount = 0;
                this._innerBoardViewModel.VerticalOffset = e.VerticalOffset;

            }

        }

        private void InnerBoard_Best_dataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //this.InnerDepthPopup.IsOpen = false;
            //CloseDepthPop();

            ScrollViewer scrollview = CommonUtils.GetVisualChild<ScrollViewer>((DataGrid)sender, v => v.Name == "DG_ScrollViewer");
            Visibility verticalVisibility = Visibility.Collapsed;
            if (scrollview != null)
            {
                verticalVisibility = scrollview.ComputedVerticalScrollBarVisibility;
            }

            if (verticalVisibility == Visibility.Visible)
            {
                if (e.VerticalOffset == 0 && e.ViewportHeightChange == 0 && e.ViewportWidthChange == 0)
                {
                    this._innerBoardViewModel.UpdateDataCount_Best = 0;
                }
                if (e.ViewportHeightChange == 0 && e.ViewportWidthChange == 0)
                {
                    this._innerBoardViewModel.VerticalOffset_Best = e.VerticalOffset; // + Math.Abs(e.ViewportHeightChange);
                }
            }
            else
            {
                this._innerBoardViewModel.UpdateDataCount_Best = 0;
                this._innerBoardViewModel.VerticalOffset_Best = e.VerticalOffset;
            }
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

                List<InnerOrderData> ofrList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "2", _innerBoardViewModel.ofrPriceComparer);
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

                List<InnerOrderData> bidList = _innerBoardViewModel.GetInnerOrderDatas(item => item.SecurityID == innerOrderData.SecurityID && item.Side == "1", _innerBoardViewModel.bidPriceComparer);
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
            string priceType_innerOfr = string.Empty;
            string price_innerBid = string.Empty;
            string priceType_innerBid = string.Empty;
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
                priceType_innerBid = orderData_best.PriceType_InnerBid;
                price_innerOfr = orderData_best.Price_InnerOfr.ToString();
                priceType_innerOfr = orderData_best.PriceType_InnerOfr;
            }

            string side = isBid ? ComWindowKeyHelper.BID : ComWindowKeyHelper.OFR;

            PriceMatchParam priceMatchParam = new PriceMatchParam();

            if (string.IsNullOrEmpty(qtyQty))
            {
                priceMatchParam.CustomFirmAmount = qtyQty;//orderData.Qty.ToString(); // OrderQty 订单量
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
                priceMatchParam.CustomFirmAmount = qtyQty;
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
                    priceMatchParam.Price = bidBest.First().Yield;
                }
                else
                {
                    if (!string.IsNullOrEmpty(price_innerBid))
                    {
                        priceMatchParam.Price = price_innerBid;
                        priceMatchParam.PriceType = priceType_innerBid;
                    }
                    else if (!string.IsNullOrEmpty(price_innerOfr))
                    {
                        priceMatchParam.Price = price_innerOfr;
                        priceMatchParam.PriceType = priceType_innerOfr;
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
                    priceMatchParam.Price = ofrBest.First().Yield;
                }
                else
                {
                    if (!string.IsNullOrEmpty(price_innerOfr))
                    {
                        priceMatchParam.Price = price_innerOfr;
                        priceMatchParam.PriceType = priceType_innerOfr;
                    }
                    else if (!string.IsNullOrEmpty(price_innerBid))
                    {
                        priceMatchParam.Price = price_innerBid;
                        priceMatchParam.PriceType = priceType_innerBid;
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
                    SendRequireView sendPriceView = new SendRequireView(priceMatchParam);

                    WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId), sendPriceView);
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
                    //priceMatchParam.IntentionID = orderData.IntentionID;
                    Task.Factory.StartNew(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SendRequireView sendRequireView = new SendRequireView(priceMatchParam);
                            WindowManager.Instance.OpenOnlyOneWindow($"EditOrder-{orderData.IntentionID}", sendRequireView);
                        });
                    });
                }
            }
        }

        private void InnerBoardView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                _innerBoardViewModel = this.DataContext as InnerBoardViewModel;
                if (AdminHandler.Instance.CheckPermission(MODULEID_BONDSUBTYPE, RATE))
                {
                    this.single.Visibility = Visibility.Collapsed;
                    this.best.Visibility = Visibility.Collapsed;
                    this.ihr_Inner.single.Visibility = Visibility.Collapsed;

                    _innerBoardViewModel.DisplayMode = "BNC";

                    _innerBoardViewModel.Best_DisplayMode = "BNC";
                }
                else if (AdminHandler.Instance.CheckPermission(MODULEID_BONDSUBTYPE, CREDIT))
                {
                    this.single.Visibility = Visibility.Collapsed;
                    this.best.Visibility = Visibility.Collapsed;
                    this.ihr_Inner.single.Visibility = Visibility.Collapsed;

                    _innerBoardViewModel.DisplayMode = "BCO";

                    _innerBoardViewModel.Best_DisplayMode = "BCO";
                }

                _innerBoardViewModel._InnerBoardView = this;
                dgPrice.SelectionChange("STC", "Tradebook", "Tradebook", "全", false);
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
                        // Msg 为 空 表示直接通知网页端订阅组更新
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "UpdateQuoteTagGroup", Msg = string.Empty });
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
            return InnerBoard_Best_dataGrid.CurrentColumn != null
                && (InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Ofr"
                || InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Vol.Ofr"
                || InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "No.Ofr"
                || InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Ofr回复");
        }

        private bool isBidColumn()
        {
            return InnerBoard_Best_dataGrid.CurrentColumn != null
                && (InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Bid"
                || InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Vol.Bid"
                || InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "No.Bid"
                || InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "Bid回复");
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item.IsSelected)
            {
                item.IsSelected = false;
                _innerBoardViewModel.SelectedItem_Tag = null;
                e.Handled = true;
            }
        }

        private void ListBoxItem_Best_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item.IsSelected)
            {
                item.IsSelected = false;
                _innerBoardViewModel.SelectedItem_Tag_Best = null;
                e.Handled = true;
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
                DependencyObject btn = e.OriginalSource as DependencyObject; ;
                while (btn != null && !(btn is Button)) btn = VisualTreeHelper.GetParent(btn);

                if (btn != null)
                    return;


                DependencyObject cell = e.OriginalSource as DependencyObject;
                while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
                System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;


                if (targetCell != null
                    && (targetCell.Column.Header.ToString() == "Bid"
                    || targetCell.Column.Header.ToString() == "Ofr"
                    || targetCell.Column.Header.ToString() == "Vol.Bid"
                     || targetCell.Column.Header.ToString() == "No.Bid"
                     || targetCell.Column.Header.ToString() == "TraderB"
                     || targetCell.Column.Header.ToString() == "ClientB"
                     || targetCell.Column.Header.ToString() == "Vol.Ofr"
                     || targetCell.Column.Header.ToString() == "No.Ofr"
                     || targetCell.Column.Header.ToString() == "TraderO"
                     || targetCell.Column.Header.ToString() == "ClientO"
                     || targetCell.Column.Header.ToString() == "发行人"
                     || targetCell.Column.Header.ToString() == "担保人"
                    || targetCell.Column.Header.ToString() == "代码"
                    || targetCell.Column.Header.ToString() == "简称"
                    || targetCell.Column.Header.ToString() == "报价(%)"
                    || targetCell.Column.Header.ToString() == "报量(万元)"
                    || targetCell.Column.Header.ToString() == "意向类型"
                    || targetCell.Column.Header.ToString() == "清算速度"
                    || targetCell.Column.Header.ToString() == "本方交易员"))
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
                        this.InnerDepthPopup.IsOpen = false;
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
            if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 0)
            {
                foreach (InnerOrderData item in this.InnerBoard_Best_dataGrid.SelectedItems)
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
            if (this.InnerBoard_dataGrid.SelectedItems.Count > 0)
            {
                _innerBoardViewModel.IsSingleOperationGrid = Visibility.Visible;
            }
            else
            {
                _innerBoardViewModel.IsSingleOperationGrid = Visibility.Collapsed;
            }

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

            if (this.singleTab.IsSelected)
            {
                if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders != null)
                {
                    MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders);
                }
                else
                {
                    MyOrderDefaultView_SelectedRowChanged(null);
                }
            }

            if (this.InnerBoard_dataGrid.SelectedItems.Count > 0 && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                this.InnerBoard_dataGrid.CopyData_Single();
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
            if (this.InnerBoard_Best_dataGrid.SelectedItems.Count > 0)
            {
                _innerBoardViewModel.IsBestOperationGrid = Visibility.Visible;
            }
            else
            {
                _innerBoardViewModel.IsBestOperationGrid = Visibility.Collapsed;
            }

            foreach (InnerOrderData data in e.AddedItems)
            {
                data.PropertyChanged += Data_Best_PropertyChanged;
            }

            foreach (InnerOrderData data in e.RemovedItems)
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

            if (this.bestTab.IsSelected)
            {
                if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
                {
                    MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best);
                }
                else
                {
                    MyOrderDefaultView_SelectedRowChanged(null);
                }
            }

            if (InnerBoard_Best_dataGrid.SelectedItems.Count > 0 && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                InnerBoard_Best_dataGrid.CopyData_Best();
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

        private void Single_Click(object sender, RoutedEventArgs e)
        {
            if (SingleRd_ALL.IsChecked == true)
            {
                this.InnerBoardFilterPanel_New.SetDisplayMode("0");
                _innerBoardViewModel.DisplayMode = "";
            }
            else if (SingleRd_R.IsChecked == true)
            {
                this.InnerBoardFilterPanel_New.SetDisplayMode("1");
                _innerBoardViewModel.DisplayMode = "BNC";
            }
            else if (SingleRd_C.IsChecked == true)
            {
                this.InnerBoardFilterPanel_New.SetDisplayMode("2");
                _innerBoardViewModel.DisplayMode = "BCO";
            }
            this.InnerBoardFilterPanel_New.ShowAll();
        }

        private void Best_Click(object sender, RoutedEventArgs e)
        {
            if (SingleRd_ALL_Best.IsChecked == true)
            {
                this.InnerBoardFilterPanel_New_Best.SetDisplayMode("0");
                _innerBoardViewModel.Best_DisplayMode = "";
            }
            else if (SingleRd_R_Best.IsChecked == true)
            {
                this.InnerBoardFilterPanel_New_Best.SetDisplayMode("1");
                _innerBoardViewModel.Best_DisplayMode = "BNC";
            }
            else if (SingleRd_C_Best.IsChecked == true)
            {
                this.InnerBoardFilterPanel_New_Best.SetDisplayMode("2");
                _innerBoardViewModel.Best_DisplayMode = "BCO";
            }
            this.InnerBoardFilterPanel_New_Best.ShowAll();
        }

        /// <summary>
        /// 打开模糊需求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFuzzyRequire_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!WindowManager.Instance.IsExist("InnerFuzzyRequireView"))
                    {
                        InnerFuzzyRequireView win = new InnerFuzzyRequireView();
                        WindowManager.Instance.OpenOnlyOneWindow("InnerFuzzyRequireView", win);
                    }
                });
            });

        }

        private void InnerBoardFilterPanel_New_CouponAction(object sender, RoutedEventArgs e)
        {
            this._innerBoardViewModel.SelectedItem_Tag = null;
            this._innerBoardViewModel.ViewSource.View?.Refresh();
        }

        private void InnerBoardFilterPanel_New_Best_CouponAction(object sender, RoutedEventArgs e)
        {
            this._innerBoardViewModel.SelectedItem_Tag_Best = null;
            //this._innerBoardViewModel.ViewSource_Best.View?.Refresh();
            this._innerBoardViewModel.FilterBestData();
        }

        private void ProfileListBox_Best_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            if (!CommonUtils.isOnlyCredit && !CommonUtils.isOnlyRate)
            {
                SingleRd_ALL_Best.IsChecked = true;
                this.InnerBoardFilterPanel_New_Best.SetDisplayMode("0");
                _innerBoardViewModel.Best_DisplayMode = "";
            }

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                foreach (FilterGroupItem item in e.AddedItems)
                {
                    WriteDBLogHelper.Instance.GenRecord("inner", "006", item.GroupName);
                }
            }
        }

        private void ProfileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            if (!CommonUtils.isOnlyCredit && !CommonUtils.isOnlyRate)
            {
                SingleRd_ALL.IsChecked = true;
                this.InnerBoardFilterPanel_New.SetDisplayMode("0");
                _innerBoardViewModel.DisplayMode = "";
            }

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                foreach(FilterGroupItem item in e.AddedItems)
                {
                    WriteDBLogHelper.Instance.GenRecord("inner", "006", item.GroupName);
                }
            }
        }


        bool isNoReply_best = false;
        private void reply_Best_Click(object sender, RoutedEventArgs e)
        {
            isNoReply_best = true;
            ConsiderationMenu_Best_Click(null, null);
        }

        private void returnTopBtn_Click(object sender, RoutedEventArgs e)
        {
            InnerOrderData firstData = null;
            foreach (InnerOrderData data in this._innerBoardViewModel.ViewSource.View)
            {
                firstData = data;
                break;
            }
            if (firstData != null)
            {
                this._innerBoardViewModel.SelectedItem_MyInnerOrders = firstData;
                this.InnerBoard_dataGrid.ScrollIntoView(firstData);
            }
        }

        private void returnTopBtn_best_Click(object sender, RoutedEventArgs e)
        {
            InnerOrderData firstData = null;
            foreach (InnerOrderData data in this._innerBoardViewModel.ViewSource_Best.View)
            {
                firstData = data;
                break;
            }
            if (firstData != null)
            {
                this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best = firstData;
                this.InnerBoard_Best_dataGrid.ScrollIntoView(firstData);
            }
        }

        private void copyBestBtn_Click(object sender, RoutedEventArgs e)
        {
            //List<InnerOrderData> list = new List<InnerOrderData>();
            //Button btn = sender as Button;
            //list.Add(btn.DataContext as InnerOrderData);
            //this.InnerBoard_Best_dataGrid.SelectedItem = btn.DataContext;
            //this.InnerBoard_Best_dataGrid.Sub_CopyData_Best(list);
            Button btn = sender as Button;
            if (!this.InnerBoard_Best_dataGrid.SelectedItems.Contains(btn.DataContext))
            {
                this.InnerBoard_Best_dataGrid.SelectedItem = btn.DataContext;
            }
            this.InnerBoard_Best_dataGrid.CopyData_Best();
        }

        private void copyBtn_Click(object sender, RoutedEventArgs e)
        {
            //List<InnerOrderData> list = new List<InnerOrderData>();
            //Button btn = sender as Button;
            //list.Add(btn.DataContext as InnerOrderData);
            //this.InnerBoard_dataGrid.SelectedItem = btn.DataContext;
            //this.InnerBoard_dataGrid.Sub_CopyData_Single(list);
            Button btn = sender as Button;
            if (!this.InnerBoard_dataGrid.SelectedItems.Contains(btn.DataContext))
            {
                this.InnerBoard_dataGrid.SelectedItem = btn.DataContext;
            }
            this.InnerBoard_dataGrid.CopyData_Single();
        }

        #region 记忆窗口拆分布局
        private void gridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            SaveLastExecutionsHeight();
        }

        public void SaveLastExecutionsHeight()
        {
            //bool isLite = AdminHandler.Instance.CheckPermission("5007", "500701");
            bool isLite = false;
            if (!isLite)
            {
                IniFileHelper.WriteValue($"InnerBoardView_{ LocalEnv.UserInfo.UserName}", "InnerBoardViewWidth", Row0.Width.ToString());
                IniFileHelper.WriteValue($"InnerBoardView_{ LocalEnv.UserInfo.UserName}", "PriceAndOrderViewWidth", Row2.Width.ToString());
                if (Row2.Width.Value > 0)
                {
                    btnVer.Content = ">";
                }
                IniFileHelper.WriteValue($"InnerBoardView_{ LocalEnv.UserInfo.UserName}", "ExpandButtonCollapsed", btnVer.Content.ToString() == "<" ? "1" : "0");
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
                    //bool isLite = AdminHandler.Instance.CheckPermission("5007", "500701");
                    bool isLite = false;
                    if (isLite)
                    {
                        Row1.Width = new GridLength(0, GridUnitType.Star);
                        Row2.Width = new GridLength(0, GridUnitType.Star);
                    }
                    else
                    {
                        string expandbettoncollapsed = IniFileHelper.ReadValue($"InnerBoardView_{ LocalEnv.UserInfo.UserName}", "ExpandButtonCollapsed");
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
                                string height = IniFileHelper.ReadValue($"InnerBoardView_{ LocalEnv.UserInfo.UserName}", "InnerBoardViewWidth");
                                double d = 0;
                                string width = IniFileHelper.ReadValue($"InnerBoardView_{ LocalEnv.UserInfo.UserName}", "PriceAndOrderViewWidth");

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

        private Color refeshColor = (Color)ColorConverter.ConvertFromString("#744C21");

        private void RefreshRow(DataGridRow dgr)
        {
            //DataGridRow dgr = (sender as DataGridRow);
            //dgr.Loaded -= Row_Loaded;
            InnerOrderData data = dgr.Item as InnerOrderData;
            InnerOrderData removeData;

            // if (data.IsNeedFlash)
            {
                Style oldStyle = dgr.Style;
                Style newStyle = new Style();
                foreach (SetterBase setterBase in oldStyle.Setters)
                {
                    newStyle.Setters.Add(setterBase);
                }
                dgr.Style = newStyle;

                //创建动画使用 故事板（Storyboard）
                Storyboard sb = new Storyboard();

                ColorAnimation ca = new ColorAnimation();
                ca.From = refeshColor;
                ca.To = (dgr.FindResource("DefaultBackgroup.ColorBrush") as SolidColorBrush).Color;
                ca.Duration = TimeSpan.FromSeconds(6);
                ca.FillBehavior = FillBehavior.Stop;
                ca.Completed += (obj, args) => {

                    dgr.Style = oldStyle;
                    //data.IsNeedFlash = false;
                };
                Storyboard.SetTarget(ca, dgr);
                DependencyProperty[] propertyChain = new DependencyProperty[]
                {
                                Border.BackgroundProperty,
                                SolidColorBrush.ColorProperty
                };
                Storyboard.SetTargetProperty(ca, new PropertyPath("(0).(1)", propertyChain));
                sb.Children.Add(ca);
                sb.Begin();
            }
        }

        private void MyOrdersOperationGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetBestOperationGridMargin();
        }

        private void SetBestOperationGridMargin()
        {
            //PART_HorizontalScrollBar
            ScrollBar hScrollBar = CommonUtils.GetVisualChild<ScrollBar>(InnerBoard_Best_dataGrid, dp => dp.Name == "PART_HorizontalScrollBar" && dp.Tag != null);

            if (hScrollBar != null)
            {
                if (hScrollBar.Visibility == Visibility.Visible)
                {
                    this.BestOperationGrid.Margin = new Thickness(0, 0, 0, 10);
                }
                else
                {
                    this.BestOperationGrid.Margin = new Thickness(0);
                }
            }
        }

        private void SingleOperationGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetSingleOperationGridMargin();
        }

        private void SetSingleOperationGridMargin()
        {
            //PART_HorizontalScrollBar
            ScrollBar hScrollBar = CommonUtils.GetVisualChild<ScrollBar>(InnerBoard_dataGrid, dp => dp.Name == "PART_HorizontalScrollBar" && dp.Tag != null);

            if (hScrollBar != null)
            {
                if (hScrollBar.Visibility == Visibility.Visible)
                {
                    this.singleOperationGrid.Margin = new Thickness(0, 0, 0, 10);
                }
                else
                {
                    this.singleOperationGrid.Margin = new Thickness(0);
                }
            }
        }
        private void SingleSendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
            {
                this.InnerBoard_dataGrid.CopyData_Single(false, "2");
            }
            else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
            {
                this.InnerBoard_dataGrid.CopyData_Single(false, "3");
            }
            else
            {
                this.InnerBoard_dataGrid.CopyData_Single(false, "1");
            }
            FQSendWindow fqSendWindow = new FQSendWindow();
            fqSendWindow.Owner = Application.Current.MainWindow;
            fqSendWindow.Show();
        }

        private void SingleCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
               && CommonUtils.CopyDic["COPYMODE"].Count > 0
               && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
            {
                this.InnerBoard_dataGrid.CopyData_Single(false, "2");
            }
            else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
               && CommonUtils.CopyDic["COPYMODE"].Count > 0
               && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
            {
                this.InnerBoard_dataGrid.CopyData_Single(false, "3");
            }
            else
            {
                this.InnerBoard_dataGrid.CopyData_Single(false, "1");
            }
            FQSendWindow fqSendWindow = new FQSendWindow(true);
            fqSendWindow.Owner = Application.Current.MainWindow;
            fqSendWindow.Show();

            WriteDBLogHelper.Instance.GenRecord("inner", "003", "批量-复制");
        }

        private void BestSendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                && CommonUtils.CopyDic["COPYMODE"].Count > 0
                && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
            {
                this.InnerBoard_Best_dataGrid.CopyData_Best(false, "2");
            }
            else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                && CommonUtils.CopyDic["COPYMODE"].Count > 0
                && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
            {
                this.InnerBoard_Best_dataGrid.CopyData_Best(false, "3");
            }
            else
            {
                this.InnerBoard_Best_dataGrid.CopyData_Best(false, "1");
            }
            
            FQSendWindow fqSendWindow = new FQSendWindow();
            fqSendWindow.Owner = Application.Current.MainWindow;
            fqSendWindow.Show();
        }

        private void BestCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                && CommonUtils.CopyDic["COPYMODE"].Count > 0
                && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
            {
                this.InnerBoard_Best_dataGrid.CopyData_Best(false, "2");
            }
            else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                && CommonUtils.CopyDic["COPYMODE"].Count > 0
                && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
            {
                this.InnerBoard_Best_dataGrid.CopyData_Best(false, "3");
            }
            else
            {
                this.InnerBoard_Best_dataGrid.CopyData_Best(false, "1");
            }
            FQSendWindow fqSendWindow = new FQSendWindow(true);
            fqSendWindow.Owner = Application.Current.MainWindow;
            fqSendWindow.Show();

            WriteDBLogHelper.Instance.GenRecord("inner", "003", "批量-复制");
        }

        private Popup tagPopup = new Popup();

        private void single_arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = sender as Grid;
            tagPopup.Child = this.FindResource("Float_Single") as UserControl;
            tagPopup.DataContext = (sender as Grid).DataContext;
            tagPopup.Placement = PlacementMode.Right;
            tagPopup.PlacementTarget = g;
            tagPopup.IsOpen = true;
        }

        private void single_arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            tagPopup.IsOpen = false;
        }

        private void BestBid_Arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = sender as Grid;
            tagPopup.Child = this.FindResource("Float_BestBid") as UserControl;
            tagPopup.DataContext = (sender as Grid).DataContext;
            tagPopup.Placement = PlacementMode.Right;
            tagPopup.PlacementTarget = g;
            tagPopup.IsOpen = true;
        }

        private void BestBid_Arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            tagPopup.IsOpen = false;
        }

        private void BestOfr_Arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = sender as Grid;
            tagPopup.Child = this.FindResource("Float_BestOfr") as UserControl;
            tagPopup.DataContext = (sender as Grid).DataContext;
            tagPopup.Placement = PlacementMode.Left;
            tagPopup.PlacementTarget = g;
            tagPopup.IsOpen = true;
        }

        private void BestOfr_Arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            tagPopup.IsOpen = false;
        }

        Thread threadInit;
        DateTime currentTime;
        private void InnerBoard_Best_dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (threadInit == null)
            {
                threadInit = new Thread(() => 
                {
                    currentTime = DateTime.Now;
                    while ((DateTime.Now - currentTime).TotalMilliseconds <= 500)
                    {
                        currentTime = DateTime.Now;
                        Thread.Sleep(100);
                        break;
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (this.bestTab.IsSelected)
                        {
                            if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best == null)
                            {
                                if (this.InnerBoard_Best_dataGrid.Items != null && this.InnerBoard_Best_dataGrid.Items.Count > 0)
                                {
                                    _innerBoardViewModel.SelectedItem_MyInnerOrders_Best = (InnerOrderData)this.InnerBoard_Best_dataGrid.Items[0];
                                }
                            }
                            MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best);
                        }
                        else if (this.singleTab.IsSelected)
                        {
                            if (_innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders == null)
                            {
                                if (this.InnerBoard_dataGrid.Items != null && this.InnerBoard_dataGrid.Items.Count > 0)
                                {
                                    _innerBoardViewModel.SelectedItem_MyInnerOrders = (InnerOrderData)this.InnerBoard_dataGrid.Items[0];
                                }
                            }
                            MyOrderDefaultView_SelectedRowChanged(_innerBoardViewModel.SelectedItem_MyInnerOrders);
                        }
                    }));
                });
                threadInit.IsBackground = true;
                threadInit.Start();
            }
        }

        private void StartQQMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            Sub_DoStartQQ(false);
        }

        private void Sub_DoStartQQ(bool is_send_direct)
        {
            if (!LocalEnv.ConfInfo.IsQQ)
            {
                return;
            }
            string receiver = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.FromQQ_Bid;
            string remainingTime = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.RemainingTime;
            string symbol = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Symbol;
            string securityid = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
            string side = "Bid";
            string price = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Price_InnerBid == null ? string.Empty : $" {this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Price_InnerBid.Value.ToString("#.####")}";
            string qty = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Qty_InnerBid == null ? string.Empty : $" {this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Qty_InnerBid.Value.ToString("#")}w";
            string settlType = string.Empty;
            if (!string.IsNullOrEmpty(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid)
                && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid != "2"
                && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid != "3"
                && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid != "2,3")
            {
                settlType = " " + ComboSourceUtil.Instance.FindEnum(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid, "settletype");
            }
            string message = $"{remainingTime} {symbol} {securityid} {side}{price}{qty}{settlType}";
            if (this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "ClientO")
            {
                receiver = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.FromQQ_Ofr;
                remainingTime = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.RemainingTime;
                symbol = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Symbol;
                securityid = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
                side = "Ofr";
                price = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Price_InnerOfr == null ? string.Empty : $" {this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Price_InnerOfr.Value.ToString("#.####")}";
                qty = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Qty_InnerOfr == null ? string.Empty : $" {this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Qty_InnerOfr.Value.ToString("#")}w";
                if (!string.IsNullOrEmpty(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr)
                    && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr != "2"
                    && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr != "3"
                    && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr != "2,3")
                {
                    settlType = " " + ComboSourceUtil.Instance.FindEnum(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr, "settletype");
                }
                message = $"{remainingTime} {symbol} {securityid} {side}{price}{qty}{settlType}";
            }
            if (!string.IsNullOrEmpty(receiver) && !string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
            {
                ThreadPoolHelper.InvokeThreadPool(o =>
                {
                    SendDesktopFinQReq request = new SendDesktopFinQReq();
                    request.Username = LocalEnv.UserInfo.UserName;
                    request.sender = LocalEnv.UserInfo.QQNumber;
                    request.receiver = receiver;
                    request.send_direct = is_send_direct;
                    request.clear_edit = true;
                    request.channel = "0";
                    request.message = is_send_direct ? $"您的报价【{message}】，还能再好吗？" : message;
                    Common.Model.Fix.Message.PResult result = FinQHandler.Instance.SendDesktopFinQ(request);

                    if (result != null && !result.IsSuccess())
                    {
                        string errorMsg = "失败";
                        if (!string.IsNullOrEmpty(result.Msg))
                            errorMsg = result.Msg;
                        CommonUtils.Prompt2Second("发起客户单聊QQ", errorMsg);
                    }
                });
                ClipboardHelper.CopyToClipboard(message, true);
            }
            else
            {
                if (string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
                    CommonUtils.Prompt2Second("发起客户单聊QQ", "请联系管理员设置本用户对应QQ号码!");
                else if (string.IsNullOrEmpty(receiver))
                    CommonUtils.Prompt2Second("发起客户单聊QQ", "找不到客户QQ号");
            }
        }

        private void StartQQMenu_Trader_Click(object sender, RoutedEventArgs e)
        {
            DoStartQQ_Trader(false);
        }

        private void DoStartQQ_Trader(bool is_send_direct)
        {
            if (!LocalEnv.ConfInfo.IsQQ)
            {
                return;
            }
            string receiver = string.Empty;
            string message = string.Empty;
            bool isMe = false;
            if (this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "TraderB")
            {
                if (this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.IsMe_InnerBid)
                {
                    isMe = true;
                }
                else
                {
                    receiver = CommonUtils.GetQQNumber(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.UserName_InnerBid);
                    string remainingTime = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.RemainingTime;
                    string symbol = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Symbol;
                    string securityid = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
                    string side = "Bid";
                    string price = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Price_InnerBid == null ? string.Empty : $" {this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Price_InnerBid.Value.ToString("#.####")}";
                    string qty = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Qty_InnerBid == null ? string.Empty : $" {this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Qty_InnerBid.Value.ToString("#")}w";
                    string settlType = string.Empty;
                    if (!string.IsNullOrEmpty(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid)
                        && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid != "2"
                        && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid != "3"
                        && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid != "2,3")
                    {
                        settlType = " " + ComboSourceUtil.Instance.FindEnum(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerBid, "settletype");
                    }
                    message = $"{remainingTime} {symbol} {securityid} {side}{price}{qty}{settlType}";
                }
            }
            else if (this.InnerBoard_Best_dataGrid.CurrentColumn.Header.ToString() == "TraderO")
            {
                if (this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.IsMe_InnerOfr)
                {
                    isMe = true;
                }
                else
                {
                    receiver = CommonUtils.GetQQNumber(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.UserName_InnerOfr);
                    string remainingTime = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.RemainingTime;
                    string symbol = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Symbol;
                    string securityid = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID;
                    string side = "Ofr";
                    string price = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Price_InnerOfr == null ? string.Empty : $" {this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Price_InnerOfr.Value.ToString("#.####")}";
                    string qty = this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Qty_InnerOfr == null ? string.Empty : $" {this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.Qty_InnerOfr.Value.ToString("#")}w";
                    string settlType = string.Empty;
                    if (!string.IsNullOrEmpty(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr)
                        && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr != "2"
                        && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr != "3"
                        && this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr != "2,3")
                    {
                        settlType = " " + ComboSourceUtil.Instance.FindEnum(this._innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SettleType_InnerOfr, "settletype");
                    }
                    message = $"{remainingTime} {symbol} {securityid} {side}{price}{qty}{settlType}";
                }
            }

            if (isMe)
            {
                return;
            }

            if (!string.IsNullOrEmpty(receiver) && !string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
            {
                ThreadPoolHelper.InvokeThreadPool(o =>
                {
                    SendDesktopFinQReq request = new SendDesktopFinQReq();
                    request.Username = LocalEnv.UserInfo.UserName;
                    request.sender = LocalEnv.UserInfo.QQNumber;
                    request.receiver = receiver;
                    request.send_direct = is_send_direct;
                    request.clear_edit = true;
                    request.channel = "0";
                    request.message = is_send_direct ? $"您的报价【{message}】，还能再好吗？" : message;
                    Common.Model.Fix.Message.PResult result = FinQHandler.Instance.SendDesktopFinQ(request);

                    if (result != null && !result.IsSuccess())
                    {
                        string errorMsg = "失败";
                        if (!string.IsNullOrEmpty(result.Msg))
                            errorMsg = result.Msg;
                        CommonUtils.Prompt2Second("发起客户单聊QQ", errorMsg);
                    }
                });
                ClipboardHelper.CopyToClipboard(message, true);
            }
            else
            {
                if (string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
                    CommonUtils.Prompt2Second("发起客户单聊QQ", "请联系管理员设置本用户对应QQ号码!");
                else if (string.IsNullOrEmpty(receiver))
                    CommonUtils.Prompt2Second("发起客户单聊QQ", "没有找到对应交易员的QQ号码！");
            }
        }

        private void StartQQ2Menu_Best_Click(object sender, RoutedEventArgs e)
        {
            Sub_DoStartQQ(true);
        }

        private void StartQQ2Menu_Trader_Click(object sender, RoutedEventArgs e)
        {
            DoStartQQ_Trader(true);
        }

        private void CopyMenu_SecurityID_Best_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_Best_dataGrid.CopyData_SecurityID();
        }

        private void CopyMenu_Symbol_Best_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_Best_dataGrid.CopyData_Symbol();
        }

        private void CopyMenu_Price_Best_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_Best_dataGrid.CopyData_Best_Price();
        }

        private void CopyMenu_SecurityID_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_dataGrid.CopyData_SecurityID();
        }

        private void CopyMenu_Symbol_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_dataGrid.CopyData_Symbol();
        }

        private void CopyMenu_Price_Click(object sender, RoutedEventArgs e)
        {
            this.InnerBoard_dataGrid.CopyData_Single_Price();
        }

        private void InnerBoardFilterPanel_New_Best_ShowAllAction(object sender, RoutedEventArgs e)
        {
            WriteDBLogHelper.Instance.GenRecord("inner", "005", "show all");
        }

        private void InnerBoardFilterPanel_New_ShowAllAction(object sender, RoutedEventArgs e)
        {
            WriteDBLogHelper.Instance.GenRecord("inner", "005", "show all");
        }

        private void ToQbMenu_Click(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.IsBDBAppName() && _innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders != null)
            {
                CommonUtils.OpenSmallBondWin(_innerBoardViewModel.SelectedItem_MyInnerOrders.SecurityID);
            }
        }

        private void ToQbMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.IsBDBAppName() && _innerBoardViewModel != null && _innerBoardViewModel.SelectedItem_MyInnerOrders_Best != null)
            {
                CommonUtils.OpenSmallBondWin(_innerBoardViewModel.SelectedItem_MyInnerOrders_Best.SecurityID);
            }
        }
    }
}
