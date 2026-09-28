using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Threading;
using System.Collections;
using STC.Common.Utils;

namespace BDB.Trade.Plugin.View.Position
{
    /// <summary>
    /// PositionView.xaml 的交互逻辑
    /// </summary>
    public partial class PositionView : UserControl
    {
        PositionViewModel _myTraderViewModel = new PositionViewModel();
        DispatcherTimer firmToNoTimer;
        ContextMenu contextMenu;
        //Modi:cyc Date:2021-04-12 Func:增加倒计时
        DispatcherTimer descTimer = new DispatcherTimer();

        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(MyOrdersFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));
        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        private void SetContextMenu()
        {
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("CopyContextMenu");
            }
        }
        public PositionView()
        {
            InitializeComponent();


            if (firmToNoTimer == null)
            {
                firmToNoTimer = new DispatcherTimer();
                firmToNoTimer.Interval = new TimeSpan(1000);
                firmToNoTimer.Tick += FirmToNoTimer_Tick;
                // firmToNoTimer.Start();
            }

            // 倒计时
            //if (this.descTimer == null)
            //{
                //this.descTimer = new DispatcherTimer();
                //this.descTimer.Interval = TimeSpan.FromSeconds(1);
                //this.descTimer.Tick += DescTimer_Tick;
                //this.descTimer.Start();
            //}
        }

        private void CurrentItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == MyOrderProcesser.QTY_ORDER_FIELD
                 || e.PropertyName == MyOrderProcesser.EXECQTY_ORDER_FIELD)
                {
                    DataRowView currentItem = sender as DataRowView;

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (PositionView.CrossPopup != null && PositionView.CrossPopup.Child != null && PositionView.CrossPopup.Child is CrossQtyPopup
                                && currentItem.Row.RowState != DataRowState.Detached)
                        {
                            CrossQtyPopup crossQtyPopup = PositionView.CrossPopup.Child as CrossQtyPopup;
                            if (crossQtyPopup != null && crossQtyPopup.DataContext is CrossQtyPopupViewModel)
                            {
                                CrossQtyPopupViewModel viewModel = crossQtyPopup.DataContext as CrossQtyPopupViewModel;

                                if (e.PropertyName == MyOrderProcesser.QTY_ORDER_FIELD)
                                {
                                    viewModel.TotalQty = CommonUtils.ConvertToLong(CommonUtils.ConvertToString(currentItem[MyOrderProcesser.QTY_ORDER_FIELD]));
                                }
                                else if (e.PropertyName == MyOrderProcesser.EXECQTY_ORDER_FIELD)
                                {
                                    viewModel.ExeQty = CommonUtils.ConvertToLong(CommonUtils.ConvertToString(currentItem[MyOrderProcesser.EXECQTY_ORDER_FIELD]));
                                }
                            }
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _myTraderViewModel;
            _myTraderViewModel.IsHiddenMyOrdersOperationGrid = Visibility.Collapsed;
            SetMyOrderOperationGridMargin();
        }
        

        /// <summary>
        /// 已确认条件过滤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VenuesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (listBox.Items.IndexOf(lbi) != 0 && lbi.IsSelected == true)
                    {
                        FilterDic["Venues"] = listBox.SelectedItems;
                        break;
                    }
                }

                if (this.DataContext != null && this.DataContext is PositionViewModel)
                {
                    PositionViewModel myTraderViewModel = this.DataContext as PositionViewModel;
                    myTraderViewModel.ProcessMyExecutionsFilterConditon(FilterDic);
                }
            }
        }

        private void TradeInfo_tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.TradeInfo_tabControl.SelectedItem != null)
            {
                // 我的需求
                if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Tag.ToString() == "0")
                {
                    this._myTraderViewModel.SelectedPageIndex = 0;
                    this.myOrderFilter.Visibility = Visibility.Visible;
                    this.Search_TextBox.Visibility = Visibility.Visible;
                    this.myExeFilter.Visibility = Visibility.Collapsed;
                    this.myCancelOrderFilter.Visibility = Visibility.Collapsed; 
                }

                // 已确认
                if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Tag.ToString() == "1")
                {
                    this._myTraderViewModel.SelectedPageIndex = 1;
                    this.myOrderFilter.Visibility = Visibility.Collapsed;
                    this.Search_TextBox.Visibility = Visibility.Collapsed;
                    this.myExeFilter.Visibility = Visibility.Visible;
                    this.myCancelOrderFilter.Visibility = Visibility.Collapsed;
                }

                // 已撤
                if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Tag.ToString() == "2")
                {
                    this._myTraderViewModel.SelectedPageIndex = 2;
                    this.myExeFilter.Visibility = Visibility.Collapsed;
                    this.myOrderFilter.Visibility = Visibility.Collapsed;
                    this.Search_TextBox.Visibility = Visibility.Visible;
                    this.myCancelOrderFilter.Visibility = Visibility.Visible;
                }
            }
        }

        private void myExeFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is PositionViewModel)
            {
                PositionViewModel myTraderViewModel = this.DataContext as PositionViewModel;
                myTraderViewModel.ProcessMyExecutionsFilterConditon(this.myExeFilter.FilterDic);
            }
        }

        //private void datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    DataGrid datagrid = sender as DataGrid;
        //    Point aP = e.GetPosition(datagrid);
        //    IInputElement obj = datagrid.InputHitTest(aP);
        //    DependencyObject target = obj as DependencyObject;


        //    while (target != null)
        //    {
        //        if (target is DataGridRow)
        //        {
        //            break;
        //        }
        //        target = VisualTreeHelper.GetParent(target);
        //    }
        //    DataGridRow row = target as DataGridRow;
        //    DataRowView currentItem = row.Item as DataRowView;

        //    Window w = new Window();
        //    CrossQtyPopup popup = new CrossQtyPopup();
        //    CrossQtyPopupViewModel viewModel = new CrossQtyPopupViewModel();
        //    viewModel.SecurtityID = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
        //    viewModel.Symbol = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SYMBOL_ORDER_FIELD]);
        //    viewModel.STCCode = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.STCCODE_ORDER_FIELD]);
        //    viewModel.TotalQty = CommonUtils.ConvertToLong(CommonUtils.ConvertToString(currentItem[MyOrderProcesser.QTY_ORDER_FIELD]));
        //    viewModel.ExeQty = CommonUtils.ConvertToLong(CommonUtils.ConvertToString(currentItem[MyOrderProcesser.EXECQTY_ORDER_FIELD]));
        //    viewModel.GenCrossDv();
        //    popup.DataContext = viewModel;
        //    w.Content = popup;

        //    w.Show();
        //}


        #region CrossPopup
        private static DispatcherTimer Timer4ControlDetailPopup;
        private static DataGridCell placementTargetObject = null;
        private static Popup CrossPopup;

        private void ControlPopup(object state, EventArgs e)
        {
            try
            {
                //if (placementTargetObject != null)
                //{
                //    bool isChange = false;
                //    if (PositionView.CrossPopup.PlacementTarget != placementTargetObject)
                //    {
                //        //PositionView.CrossPopup.IsOpen = false;
                //        isChange = true;
                //        PositionView.CrossPopup.PlacementTarget = placementTargetObject;
                //        PositionView.CrossPopup.Placement = PlacementMode.Bottom;
                //        PositionView.CrossPopup.HorizontalOffset = 0;
                //    }
                //    if (isChange == true)
                //    {
                //        if (placementTargetObject is DataGridCell)
                //        {
                //            DataGridCell c = placementTargetObject as DataGridCell;
                //            DataRowView currentItem = c.DataContext as DataRowView;

                //            if (currentItem != null && currentItem.Row.RowState != DataRowState.Detached && PositionView.CrossPopup != null && PositionView.CrossPopup.Child != null && PositionView.CrossPopup.Child is CrossQtyPopup)
                //            {
                //                CrossQtyPopup crossQtyPopup = PositionView.CrossPopup.Child as CrossQtyPopup;
                //                if (crossQtyPopup != null && crossQtyPopup.DataContext is CrossQtyPopupViewModel)
                //                {
                //                    CrossQtyPopupViewModel viewModel = crossQtyPopup.DataContext as CrossQtyPopupViewModel;
                //                    viewModel.SecurtityID = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
                //                    viewModel.Symbol = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SYMBOL_ORDER_FIELD]);
                //                    viewModel.STCCode = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.STCCODE_ORDER_FIELD]);
                //                    viewModel.TotalQty = CommonUtils.ConvertToLong(CommonUtils.ConvertToString(currentItem[MyOrderProcesser.QTY_ORDER_FIELD]));
                //                    viewModel.ExeQty = CommonUtils.ConvertToLong(CommonUtils.ConvertToString(currentItem[MyOrderProcesser.EXECQTY_ORDER_FIELD]));
                //                    viewModel.SettleType = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
                //                    viewModel.Side = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SIDE_ORDER_FIELD]);
                //                    viewModel.GenCrossDv();
                //                }
                //            }
                //        }


                //        // 显示浮动框
                //        PositionView.CrossPopup.IsOpen = true;
                //    }
                //}

                //DataGridCell arrowObj = PositionView.CrossPopup.PlacementTarget as DataGridCell;
                //if (!PositionView.CrossPopup.IsMouseOver)
                //{
                //    if (arrowObj != null)
                //    {
                //        if (!arrowObj.IsMouseOver)
                //        {
                //            PositionView.CrossPopup.IsOpen = false;
                //            //PositionView.placementTargetObject = null;
                //            //PositionView.CrossPopup.PlacementTarget = null;
                //            PositionView.Timer4ControlDetailPopup.Stop();
                //        }
                //    }
                //    else
                //    {
                //        PositionView.CrossPopup.IsOpen = false;
                //        //PositionView.placementTargetObject = null;
                //        //PositionView.CrossPopup.PlacementTarget = null;
                //        PositionView.Timer4ControlDetailPopup.Stop();
                //    }
                //}
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

        private void ContentPresenter_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridCell c = ((sender as Border).TemplatedParent as DataGridCell);

            placementTargetObject = c;

            //if (PositionView.Timer4ControlDetailPopup.IsEnabled == false)
            //{
            //    PositionView.Timer4ControlDetailPopup.Start();
            //}
        }
        #endregion

        #region 时钟倒计时显示
        OrderModel drv;
        private void clock_img_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((Button)sender).DataContext is OrderModel)
            {
                drv = ((Button)sender).DataContext as OrderModel;

                if (firmToNoTimer.IsEnabled == false)
                {
                    firmToNoTimer.Start();
                }
            }
        }

        private void clock_img_MouseLeave(object sender, MouseEventArgs e)
        {
            if (firmToNoTimer.IsEnabled == true)
            {
                firmToNoTimer.Stop();
            }
        }

        private void FirmToNoTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (drv != null)
                {
                    string transactTime_str = CommonUtils.ConvertToString(drv.ValidUntilTime);
                    DateTime transactTime = CommonUtils.ConvertToDateTime(transactTime_str);
                    if (string.IsNullOrEmpty(drv.Info1))
                    {
                        drv.Info1 = "1|";
                    }
                         
                    string info1 = drv.Info1.ToString().Split('|')[0];
                    if (info1 == "0")
                    {
                        drv.RemainingTime = "";
                    }
                    else if ((info1 == "2") || string.IsNullOrEmpty(info1))
                    {
                        //如果是通过api下单，则为空
                        drv.RemainingTime = $"{transactTime.ToString("HH:mm:ss")} 后失效";
                    }
                    else if (info1 == "1")
                    {
                        TimeSpan diffTime = DateTimeEx.ServerTime - transactTime;
                        long firmToNonTime = CommonUtils.ConvertToLong(drv.NonfirmTime.ToString());
                        lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                        {
                            if (diffTime.TotalSeconds > firmToNonTime * 60)
                            {
                                drv.RemainingTime = "00：00：00"; ;
                            }
                            else
                            {
                                drv.RemainingTime = DateTime.Parse("00:00:00").AddSeconds(firmToNonTime * 60 - diffTime.TotalSeconds).ToString("HH:mm:ss") + " 后失效";
                            }
                        }
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
        #endregion

        #region 点价意向倒计时
        private void DescTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                MyOrderProcesser.Processer.UpdatMyOrdersTimeRemains();
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        private void MyOrders_dataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMyOrderOperationGridMargin();
        }


        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (this.TradeInfo_tabControl.SelectedItem != null)
            {
                if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Header.ToString() == "已确认")
                {
                    _myTraderViewModel.OpenHistoryPage(true);
                }
            }

            _myTraderViewModel.OpenHistoryPage(false);

        }

        private void btnFullScreen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MyOrdersOperationGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetMyOrderOperationGridMargin();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyOrders_dataGrid.SelectedItem = ((Button)sender).DataContext;
        }

        private void ResumeBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyCancels_dataGrid.SelectedItem = ((Button)sender).DataContext;
        }

        private void processBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyExecutions_dataGrid.SelectedItem = ((Button)sender).DataContext;
        }

        private void MyOrders_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.MyOrders_dataGrid.SelectedItems.Count > 0)
            {
                SetContextMenu();
                contextMenu.IsOpen = true;
            }
        }

        private void MyExecutions_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.MyExecutions_dataGrid.SelectedItems.Count > 0)
            {
                SetContextMenu();
                contextMenu.IsOpen = true;
            }
        }

        private void MyCancels_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.MyCancels_dataGrid.SelectedItems.Count > 0)
            {
                SetContextMenu();
                contextMenu.IsOpen = true;
            }
        }

        private void copyRow_Click(object sender, RoutedEventArgs e)
        {
            if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Header.ToString() == "我的需求")
            {
                string value = _myTraderViewModel.CopyRowText(this.MyOrders_dataGrid.SelectedItems, "Orders");
                Clipboard.SetDataObject(value, true);
                MyOrders_dataGrid.UnselectAll();
            }
            else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Header.ToString() == "已确认")
            {
                string value = _myTraderViewModel.CopyRowText(this.MyExecutions_dataGrid.SelectedItems, "Deals");
                Clipboard.SetDataObject(value, true);
                MyExecutions_dataGrid.UnselectAll();
            }
            else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Header.ToString() == "已撤")
            {
                string value = _myTraderViewModel.CopyRowText(this.MyCancels_dataGrid.SelectedItems, "Cancels");
                Clipboard.SetDataObject(value, true);
                MyCancels_dataGrid.UnselectAll();
            }
        }

        private void copySTCCode_Click(object sender, RoutedEventArgs e)
        {
            if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Header.ToString() == "我的需求")
            {
                string value = _myTraderViewModel.CopySTCCodeText(this.MyOrders_dataGrid.SelectedItems, "Orders");
                Clipboard.SetDataObject(value, true);
                MyOrders_dataGrid.UnselectAll();
            }
            else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Header.ToString() == "已确认")
            {
                string value = _myTraderViewModel.CopySTCCodeText(this.MyExecutions_dataGrid.SelectedItems, "Deals");
                Clipboard.SetDataObject(value, true);
                MyExecutions_dataGrid.UnselectAll();
            }
            else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Header.ToString() == "已撤")
            {
                string value = _myTraderViewModel.CopySTCCodeText(this.MyCancels_dataGrid.SelectedItems, "Cancels");
                Clipboard.SetDataObject(value, true);
                MyCancels_dataGrid.UnselectAll();
            }
        }

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
        }

        private void VenuesListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        // 全选
        private void CheckBox_All_Checked(object sender, RoutedEventArgs e)
        {
            if (_myTraderViewModel != null)
            {
                _myTraderViewModel.SetAllReqRecordSelect(true);
                _myTraderViewModel.CalcRecordCount();
                SetMyOrderOperationGridMargin();
            }
        }

        // 全不选
        private void CheckBox_All_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_myTraderViewModel != null)
            {
                _myTraderViewModel.SetAllReqRecordSelect(false);
                _myTraderViewModel.CalcRecordCount();
                SetMyOrderOperationGridMargin();
            }
        }

        /// <summary>
        /// 单个选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Selected_Chk_Checked(object sender, RoutedEventArgs e)
        {
            if (_myTraderViewModel != null)
            {
                _myTraderViewModel.CalcRecordCount();
                SetMyOrderOperationGridMargin();
            }
        }

        /// <summary>
        /// 设置下方悬浮框
        /// </summary>
        public void SetMyOrderOperationGridMargin()
        {
            //PART_HorizontalScrollBar
            ScrollBar hScrollBar = CommonUtils.GetVisualChild<ScrollBar>(MyOrders_dataGrid, dp => dp.Name == "PART_HorizontalScrollBar");

            if (hScrollBar != null)
            {
                if (hScrollBar.Visibility == Visibility.Visible)
                {
                    this.MyOrdersOperationGrid.Margin = new Thickness(0, 0, 0, 10);
                    MyOrdersOperationGrid.Margin = new Thickness(0, -10, 0, 0);
                }
                else
                {
                    this.MyOrdersOperationGrid.Margin = new Thickness(0);
                    MyOrdersOperationGrid.Margin = new Thickness(0, 0, 0, 0);
                }
            }
        }

        #region 已撤
        private void CheckBox_All_Checked_1(object sender, RoutedEventArgs e)
        {
            if (_myTraderViewModel != null)
            {
                _myTraderViewModel.SetAllCancelRecordSelect(true);
            }
        }

        private void CheckBox_All_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if (_myTraderViewModel != null)
            {
                _myTraderViewModel.SetAllCancelRecordSelect(false);
            }
        }
        #endregion
    }
}
