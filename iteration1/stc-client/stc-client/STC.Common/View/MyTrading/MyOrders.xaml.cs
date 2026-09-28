using Newtonsoft.Json;
using STC.Common.DataProcesser;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.ViewModel.MyTrading;
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

namespace STC.Common.View.MyTrading
{
    /// <summary>
    /// MyOrders.xaml 的交互逻辑
    /// </summary>
    public partial class MyOrders : UserControl
    {
        public string TradeTabSelectedItem
        {
            get { return (string)GetValue(TradeTabSelectedItemProperty); }
            set { SetValue(TradeTabSelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TradeTabSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TradeTabSelectedItemProperty =
            DependencyProperty.Register("TradeTabSelectedItem", typeof(string), typeof(MyOrders), new PropertyMetadata(string.Empty));

        public string IsMyOrdersShowRedDot
        {
            get { return (string)GetValue(IsMyOrdersShowRedDotProperty); }
            set { SetValue(IsMyOrdersShowRedDotProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMyOrdersShowRedDot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMyOrdersShowRedDotProperty =
            DependencyProperty.Register("IsMyOrdersShowRedDot", typeof(string), typeof(MyOrders), new PropertyMetadata(string.Empty));


        public string TotalCountOrderHeader
        {
            get { return (string)GetValue(TotalCountOrderHeaderProperty); }
            set { SetValue(TotalCountOrderHeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalCountOrderHeader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalCountOrderHeaderProperty =
            DependencyProperty.Register("TotalCountOrderHeader", typeof(string), typeof(MyOrders), new PropertyMetadata("我的订单"));




        private MyOrdersViewModel _myOrderViewModel = new MyOrdersViewModel();
        DispatcherTimer firmToNoTimer;
        ContextMenu contextMenu;

        private void SetContextMenu()
        {
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("CopyContextMenu");
            }
        }


        #region CrossPopup
        private static DispatcherTimer Timer4ControlDetailPopup;
        private static DataGridCell placementTargetObject = null;
        private static Popup CrossPopup;

        private void ControlPopup(object state, EventArgs e)
        {
            try
            {
                if (placementTargetObject != null)
                {
                    bool isChange = false;
                    if (MyOrders.CrossPopup.PlacementTarget != placementTargetObject)
                    {
                        //MyOrders.CrossPopup.IsOpen = false;
                        isChange = true;
                        MyOrders.CrossPopup.PlacementTarget = placementTargetObject;
                        MyOrders.CrossPopup.Placement = PlacementMode.Bottom;
                        MyOrders.CrossPopup.HorizontalOffset = 0;
                        //bool ifLeft = SystemParameters.MenuDropAlignment;
                        //if (ifLeft)
                        //{
                        //    QuotationList.DetailPopup.HorizontalOffset = 140;
                        //}
                        //else
                        //{
                        //    QuotationList.DetailPopup.HorizontalOffset = 20;
                        //}
                    }

                    //if (MyOrders.CrossPopup.IsOpen == false)
                    if (isChange == true)
                    {
                        if (placementTargetObject is DataGridCell)
                        {
                            DataGridCell c = placementTargetObject as DataGridCell;
                            DataRowView currentItem = c.DataContext as DataRowView;

                            if (currentItem != null && currentItem.Row.RowState != DataRowState.Detached && MyOrders.CrossPopup != null && MyOrders.CrossPopup.Child != null && MyOrders.CrossPopup.Child is CrossQtyPopup)
                            {
                                CrossQtyPopup crossQtyPopup = MyOrders.CrossPopup.Child as CrossQtyPopup;
                                if (crossQtyPopup != null && crossQtyPopup.DataContext is CrossQtyPopupViewModel)
                                {
                                    CrossQtyPopupViewModel viewModel = crossQtyPopup.DataContext as CrossQtyPopupViewModel;
                                    viewModel.Side = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SIDE_ORDER_FIELD]);
                                    viewModel.SecurtityID = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
                                    viewModel.Symbol = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SYMBOL_ORDER_FIELD]);
                                    viewModel.STCCode = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.STCCODE_ORDER_FIELD]);
                                    viewModel.Price = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.PRICE_ORDER_FIELD]);
                                    viewModel.TotalQty = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(currentItem[MyOrderProcesser.QTY_ORDER_FIELD]));
                                    viewModel.ExeQty = CommonUtils.ConvertToLong(CommonUtils.ConvertToString(currentItem[MyOrderProcesser.EXECQTY_ORDER_FIELD]));
                                    viewModel.SettleType = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
                                    viewModel.GenCrossDv();
                                }
                            }
                        }


                        // 显示浮动框
                        MyOrders.CrossPopup.IsOpen = true;
                    }
                }

                DataGridCell arrowObj = MyOrders.CrossPopup.PlacementTarget as DataGridCell;
                if (!MyOrders.CrossPopup.IsMouseOver)
                {
                    if (arrowObj != null)
                    {
                        if (!arrowObj.IsMouseOver)
                        {
                            MyOrders.CrossPopup.IsOpen = false;
                            //MyOrders.placementTargetObject = null;
                            //MyOrders.CrossPopup.PlacementTarget = null;
                            MyOrders.Timer4ControlDetailPopup.Stop();
                        }
                    }
                    else
                    {
                        MyOrders.CrossPopup.IsOpen = false;
                        //MyOrders.placementTargetObject = null;
                        //MyOrders.CrossPopup.PlacementTarget = null;
                        MyOrders.Timer4ControlDetailPopup.Stop();
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

        private void ContentPresenter_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridCell c = ((sender as Border).TemplatedParent as DataGridCell);

            placementTargetObject = c;

            if (MyOrders.Timer4ControlDetailPopup.IsEnabled == false)
            {
                MyOrders.Timer4ControlDetailPopup.Start();
            }
        }
        #endregion


        public MyOrders()
        {
            InitializeComponent();

            this.DataContext = _myOrderViewModel;
            MyOrderProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions1;

            myOrderFilter.OnCancelClick += MyOrderFilter_OnCancelClick;

            if (MyOrders.CrossPopup == null)
            {
                MyOrders.CrossPopup = new Popup();
                MyOrders.CrossPopup.Child = new CrossQtyPopup();
                MyOrders.CrossPopup.StaysOpen = true;
                MyOrders.CrossPopup.Closed += CrossPopup_Closed;
                MyOrders.CrossPopup.Opened += CrossPopup_Opened;
            }
            if (MyOrders.Timer4ControlDetailPopup == null)
            {
                MyOrders.Timer4ControlDetailPopup = new DispatcherTimer();
                MyOrders.Timer4ControlDetailPopup.Interval = new TimeSpan(500);
                MyOrders.Timer4ControlDetailPopup.Tick += ControlPopup;
            }

            _myOrderViewModel.myWindow = this;

            // 报价倒计时
            if (firmToNoTimer == null)
            {
                firmToNoTimer = new DispatcherTimer();
                firmToNoTimer.Interval = new TimeSpan(1000);
                firmToNoTimer.Tick += FirmToNoTimer_Tick;
            }
        }

        private void Processer_OnLastExecutions1(object obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                int cnt = int.Parse(obj.ToString());
                if (cnt == 0)
                {
                    IsMyOrdersShowRedDot = "";
                    TotalCountOrderHeader = "我的订单";
                }
                else
                {
                    try
                    {
                        if (TradeTabSelectedItem == "MyOrders_TabItem")
                        {
                            IsMyOrdersShowRedDot = "";
                        }
                        else
                        {
                            IsMyOrdersShowRedDot = "+0";
                        }
                    }
                    catch
                    {
                        IsMyOrdersShowRedDot = "+0";
                    }

                    TotalCountOrderHeader = $"我的订单({cnt})";
                }
            });
        }

        private void MyOrderFilter_OnCancelClick(object arg1, RoutedEventArgs arg2)
        {
            Button button = arg1 as Button;
            _myOrderViewModel.DeleteAllOrders(button.Content.ToString());
        }

        private void CrossPopup_Closed(object sender, EventArgs e)
        {
            if (placementTargetObject != null)
            {
                DataGridCell c = placementTargetObject as DataGridCell;
                DataRowView currentItem = c.DataContext as DataRowView;
                if (currentItem != null)
                {
                    currentItem.PropertyChanged -= CurrentItem_PropertyChanged;
                }
                MyOrders.placementTargetObject = null;
                MyOrders.CrossPopup.PlacementTarget = null;
            }
        }

        private void CrossPopup_Opened(object sender, EventArgs e)
        {
            if (placementTargetObject != null)
            {
                DataGridCell c = placementTargetObject as DataGridCell;
                DataRowView currentItem = c.DataContext as DataRowView;
                currentItem.PropertyChanged += CurrentItem_PropertyChanged;
            }
        }

        private void CurrentItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == MyOrderProcesser.QTY_ORDER_FIELD
                 || e.PropertyName == MyOrderProcesser.EXECQTY_ORDER_FIELD)
                {
                    DataRowView currentItem = sender as DataRowView;

                    Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                        if (MyOrders.CrossPopup != null && MyOrders.CrossPopup.Child != null && MyOrders.CrossPopup.Child is CrossQtyPopup
                                && currentItem.Row.RowState != DataRowState.Detached)
                        {
                            CrossQtyPopup crossQtyPopup = MyOrders.CrossPopup.Child as CrossQtyPopup;
                            if (crossQtyPopup != null && crossQtyPopup.DataContext is CrossQtyPopupViewModel)
                            {
                                CrossQtyPopupViewModel viewModel = crossQtyPopup.DataContext as CrossQtyPopupViewModel;

                                if (e.PropertyName == MyOrderProcesser.QTY_ORDER_FIELD)
                                {
                                    viewModel.TotalQty = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(currentItem[MyOrderProcesser.QTY_ORDER_FIELD]));
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


        private void MyOrders_dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.MyOrders_Intention_dataGrid.CurrentItem != null)
            {
                _myOrderViewModel.SetCurrentSelected(this.MyOrders_Intention_dataGrid.SelectedItems);
            }
        }

        private void CheckBox_All_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (_myOrderViewModel != null)
            {
                _myOrderViewModel.CheckBoxAllValueChange();
            }
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyOrders_Intention_dataGrid.SelectedItem = ((Button)sender).DataContext;
        }

        private void MyOrders_dataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMyOrderOperationGridMargin();
        }

        private void SetMyOrderOperationGridMargin()
        {
            //PART_HorizontalScrollBar
            ScrollBar hScrollBar = CommonUtils.GetVisualChild<ScrollBar>(MyOrders_Intention_dataGrid, dp => dp.Name == "PART_HorizontalScrollBar" && dp.Tag != null);

            if (hScrollBar != null)
            {
                if (hScrollBar.Visibility == Visibility.Visible)
                {
                    this.MyOrdersOperationGrid.Margin = new Thickness(0, 0, 0, 10);
                }
                else
                {
                    this.MyOrdersOperationGrid.Margin = new Thickness(0);
                }
            }
        }


        #region 时钟倒计时显示
        DataRowView drv;
        private void clock_img_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((Button)sender).DataContext is DataRowView)
            {
                drv = ((Button)sender).DataContext as DataRowView;


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
                if (drv != null && drv.Row.RowState != DataRowState.Detached)
                {
                    if (drv != null)
                    {
                        string transactTime_str = CommonUtils.ConvertToString(drv[MyOrderProcesser.VALIDUNTILTIME_FIELD].ToString());
                        if (!string.IsNullOrEmpty(transactTime_str))
                        {
                            /// 到期时间
                            DateTime transactTime = CommonUtils.ConvertToDateTime(transactTime_str);

                            string srcinfo = drv[MyOrderProcesser.INFO1_FIELD].ToString();
                            string info1 = string.Empty;
                            if (JsonSplit.IsJson(srcinfo))
                            {
                                OrderTimeModel otm = JsonConvert.DeserializeObject<OrderTimeModel>(srcinfo);
                                info1 = otm.ValidTimeType;

                                if (info1.StartsWith("0|"))
                                {
                                    lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                                    {
                                        drv[MyOrderProcesser.REMAINING_TIME_FIELD] = "";
                                    }
                                }
                                else if (info1.StartsWith("2|"))
                                {
                                    lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                                        drv[MyOrderProcesser.REMAINING_TIME_FIELD] = $"{transactTime.ToString("HH:mm:ss")} 后失效";
                                }
                                else if (info1.StartsWith("1|"))
                                {
                                    if (transactTime > DateTimeEx.ServerTimeNow)
                                    {
                                        TimeSpan diffTime = DateTimeEx.ServerTimeNow - transactTime;
                                        long firmToNonTime = Math.Abs(CommonUtils.ConvertToLong(diffTime.TotalSeconds.ToString()));

                                        lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                                        {
                                            if (diffTime.TotalSeconds > firmToNonTime * 60)
                                            {
                                                drv[MyOrderProcesser.REMAINING_TIME_FIELD] = "00:00:00";
                                            }
                                            else
                                            {
                                                drv[MyOrderProcesser.REMAINING_TIME_FIELD] = DateTime.Parse("00:00:00").AddSeconds(firmToNonTime).ToString("HH:mm:ss") + " 后失效";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        drv[MyOrderProcesser.REMAINING_TIME_FIELD] = "00:00:00";
                                    }
                                }
                                else if (info1.StartsWith("3|"))
                                {
                                    lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                                        drv[MyOrderProcesser.REMAINING_TIME_FIELD] = $"报价一直有效，直到日终撤单";

                                }
                            }
                            else
                            {
                                lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                                    drv[MyOrderProcesser.REMAINING_TIME_FIELD] = $"{transactTime.ToString("HH:mm:ss")} 后失效";
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


        private void MyOrders_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.MyOrders_Intention_dataGrid.SelectedItems.Count > 0)
            {
                SetContextMenu();
                contextMenu.IsOpen = true;
            }
        }

        private void MyOrdersOperationGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetMyOrderOperationGridMargin();
        }

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void copyRow_Click(object sender, RoutedEventArgs e)
        {
            //我的订单
            string value = _myOrderViewModel.CopyRowText(this.MyOrders_Intention_dataGrid.SelectedItems, "Orders");
            ClipboardHelper.CopyToClipboard(value);
            MyOrders_Intention_dataGrid.UnselectAll();

        }

        private void copySTCCode_Click(object sender, RoutedEventArgs e)
        {
            //我的订单
            string value = _myOrderViewModel.CopySTCCodeText(this.MyOrders_Intention_dataGrid.SelectedItems, "Orders");
            ClipboardHelper.CopyToClipboard(value);
            MyOrders_Intention_dataGrid.UnselectAll();
        }


        private void myOrderFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_myOrderViewModel != null)
            {
                _myOrderViewModel.ProcessMyOrdersFilterCondition(this.myOrderFilter.FilterDic);
            }
        }
    }
}
