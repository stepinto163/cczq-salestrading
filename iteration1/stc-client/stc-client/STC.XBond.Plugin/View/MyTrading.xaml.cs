using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.FinQ;
using STC.Common.Handler.PT;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.FinQ;
using STC.Common.Utils;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Frame.Common;
using STC.XBond.Plugin.Command;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// MyTrading.xaml 的交互逻辑
    /// </summary>
    public partial class MyTrading : UserControl
    {
        MyTraderViewModel _myTraderViewModel = new MyTraderViewModel();
        DispatcherTimer firmToNoTimer;
        //ContextMenu contextMenu;
        object rfqorderlistview;
        private void SetContextMenu()
        {
            //if (contextMenu == null)
            //{
            //    contextMenu = (ContextMenu)this.FindResource("CopyContextMenu");
            //}
        }
        public MyTrading()
        {
            InitializeComponent();

            if (!LocalEnv.ConfInfo.IsQQ)
            {
                StartQQBtn.Visibility = Visibility.Collapsed;
                StartQQ2Btn.Visibility = Visibility.Collapsed;
            }

            this.MyOrders_dataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.MyOrders_dataGrid_MouseLeftButtonDown), true);

            SetBDB();

            DesignHandler.Instance.OnIntentionLink += HighligthRow;

            myOrderFilter.OnCancelClick += MyOrderFilter_OnCancelClick;
            myOrderFilter.OnManualClick += MyOrderFilter_OnManualClick;

            if (MyTrading.CrossPopup == null)
            {
                MyTrading.CrossPopup = new Popup();
                MyTrading.CrossPopup.Child = new CrossQtyPopup();
                MyTrading.CrossPopup.StaysOpen = true;
                MyTrading.CrossPopup.Closed += CrossPopup_Closed;
                MyTrading.CrossPopup.Opened += CrossPopup_Opened;
            }
            if (MyTrading.Timer4ControlDetailPopup == null)
            {
                MyTrading.Timer4ControlDetailPopup = new DispatcherTimer();
                MyTrading.Timer4ControlDetailPopup.Interval = new TimeSpan(500);
                MyTrading.Timer4ControlDetailPopup.Tick += ControlPopup;
            }

            _myTraderViewModel.myWindow = this;

            // 报价倒计时
            if (firmToNoTimer == null)
            {
                firmToNoTimer = new DispatcherTimer();
                firmToNoTimer.Interval = new TimeSpan(1000);
                firmToNoTimer.Tick += FirmToNoTimer_Tick;
            }


            //RFQ权限判断
            //to do...
            if (CommonUtils.IsBDBAppName() == false)
            {
                bool isPermission = AdminHandler.Instance.CheckPermisionById("4000", "4016");
                if (isPermission)
                {
                    bool isExistPlugin = false;
                    FileInfo[] files = new DirectoryInfo(System.IO.Path.Combine(System.Environment.CurrentDirectory, "plugin")).GetFiles("*.dll");
                    foreach (FileInfo info2 in files)
                    {
                        if (info2.Name.Equals("STC.Rfq.Plugin.dll"))
                        {
                            isExistPlugin = true;
                            break;
                        }
                    }

                    MyRfq_TabItem.Visibility = isExistPlugin ? Visibility.Visible : Visibility.Collapsed;
                    double lm = isExistPlugin ? 4 * 110 + 10 : 3 * 100 + 10;
                    RightPanel.Margin = new Thickness(lm, 2, 0, 0);
                }
                else
                {
                    MyRfq_TabItem.Visibility = Visibility.Collapsed;
                    double lm2 = 3 * 110 + 10;
                    RightPanel.Margin = new Thickness(lm2, 2, 0, 0);
                }
            }
            else
            {
                double lm2 = 2 * 110 + 10;
                RightPanel.Margin = new Thickness(lm2, 2, 0, 0);
            }

            //线下成交按钮权限
            bool isManualPermission = AdminHandler.Instance.CheckPermisionById("5000", "5006");
            if (isManualPermission && !CommonUtils.IsBDBAppName())
            {
                myOrderFilter.VisibilityManualBtn = Visibility.Visible;
                ManualBtn.Visibility = Visibility.Visible;
            }
            else
            {
                myOrderFilter.VisibilityManualBtn = Visibility.Hidden;
                ManualBtn.Visibility = Visibility.Hidden;
                ManualBtn.Height = 0;
            }

            //动态隐藏 回价详情
            foreach (var dc in MyOrders_dataGrid.Columns)
            {
                if (dc is DataGridTemplateColumn)
                {
                    if ((dc as DataGridTemplateColumn).Header.ToString() == "回价详情")
                    {
                        dc.Visibility = LocalEnv.ConfInfo.IsShowDRP == "1" ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    }
                }
            }
        }

        int i = 0;
        bool isDouble = false;
        private void MyOrders_dataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            i += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i = 0; MyOrders_UIElement_Click(sender, e); isDouble = false; };
            timer.IsEnabled = true;
            if (i % 2 == 0)
            {
                timer.IsEnabled = false;
                i = 0;
                isDouble = true;

                // 双击
                if (MyOrders_dataGrid.CurrentColumn.Header.ToString() == "客户机构"
                    || MyOrders_dataGrid.CurrentColumn.Header.ToString() == "客户名")
                {
                    StartQQBtn_Click(null, null);
                }
            }
        }

        private void MyOrders_UIElement_Click(object sender, MouseButtonEventArgs e)
        {
            //ignore
        }

        private void SetBDB()
        {
            if (CommonUtils.IsBDBAppName())
            {
                HistoryButton_MyOrder.Visibility = Visibility.Collapsed;
                HistoryButton_MyCancel.Visibility = Visibility.Collapsed;

                MyExecutions_TabItem.Visibility = Visibility.Collapsed;
                MyCancels_TabItem.Header = "已撤需求";

                myOrderFilter.ContractVisibility = Visibility.Collapsed;
                myOrderFilter.SourceVisibility = Visibility.Collapsed;
                myOrderFilter.CancelAllBtnText = "一键撤销";

                myCancelOrderFilter.ContractVisibility = Visibility.Collapsed;
                myCancelOrderFilter.SourceVisibility = Visibility.Collapsed;

                List<DataGridColumn> removeColumns = new List<DataGridColumn>();
                foreach (DataGridColumn c in this.MyOrders_dataGrid.Columns)
                {
                    string headerName = c.Header.ToString();
                    if (headerName == "组合" || headerName == "报量(手)" || headerName == "执行类型"
                        || headerName == "订单类型" || headerName == "回价详情" || headerName == "已成交(万元)"
                        || headerName == "已成交(手)" || headerName == "开平标志" || headerName == "投机套保"
                        || headerName == "资金账号" || headerName == "本方交易员" || headerName == "报价方式"
                        || headerName == "撮合范围" || headerName == "策略名" || headerName == "渠道"
                        || headerName == "STC单号" || headerName == "投组账户" || headerName == "报价(权%)")
                    {
                        removeColumns.Add(c);
                    }
                }
                foreach (DataGridColumn removeColumn in removeColumns)
                {
                    this.MyOrders_dataGrid.Columns.Remove(removeColumn);
                }
                removeColumns.Clear();

                foreach (DataGridColumn c in this.MyCancels_dataGrid.Columns)
                {
                    string headerName = c.Header.ToString();
                    if (headerName == "组合" || headerName == "执行类型" || headerName == "报量(手)"
                        || headerName == "已成交(手)" || headerName == "开平标志" || headerName == "投机套保"
                        || headerName == "资金账号" || headerName == "本方交易员" || headerName == "报价方式"
                        || headerName == "撮合范围" || headerName == "策略名" || headerName == "渠道"
                        || headerName == "STC单号" || headerName == "投组账户" || headerName == "报价(权%)")
                    {
                        removeColumns.Add(c);
                    }
                }
                foreach (DataGridColumn removeColumn in removeColumns)
                {
                    this.MyCancels_dataGrid.Columns.Remove(removeColumn);
                }
                removeColumns.Clear();

                MyOrders_dataGrid.DefaultHideColumns = string.Empty;
                MyCancels_dataGrid.DefaultHideColumns = string.Empty;
                //MyOrders_dataGrid.DefaultHideColumns = "组合,报量(手),执行类型,订单类型,回价详情,已成交(万元),已成交(手),开平标志,投机套保,资金账号,本方交易员,报价方式,撮合范围,策略名,渠道,STC单号,投组账户,报价(权%)";
                //MyCancels_dataGrid.DefaultHideColumns = "组合,执行类型,报量(手),已成交(手),开平标志,投机套保,资金账号,本方交易员,报价方式,撮合范围,策略名,渠道,STC单号,投组账户,报价(权%)";
            }
        }

        private void MyOrderFilter_OnCancelClick(object arg1, RoutedEventArgs arg2)
        {
            Button button = arg1 as Button;
            _myTraderViewModel.DeleteAllOrders(button.Content.ToString());
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
                MyTrading.placementTargetObject = null;
                MyTrading.CrossPopup.PlacementTarget = null;
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

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (MyTrading.CrossPopup != null && MyTrading.CrossPopup.Child != null && MyTrading.CrossPopup.Child is CrossQtyPopup
                                && currentItem.Row.RowState != DataRowState.Detached)
                        {
                            CrossQtyPopup crossQtyPopup = MyTrading.CrossPopup.Child as CrossQtyPopup;
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

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _myTraderViewModel;
            //_myTraderViewModel.Init();
            _myTraderViewModel.IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
            _myTraderViewModel.IsHiddenMyOrdersOperationGridTF = Visibility.Hidden;
            SetMyOrderOperationGridMargin();
            //判断RFQ权限

        }

        private void TradeInfo_tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (this.TradeInfo_tabControl.SelectedItem != null)
            //{
            //    if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Header.ToString() == "我的订单")
            //    {
            //        Grid.SetColumnSpan(this.TradeInfo_tabControl, 1);
            //        Grid.SetColumnSpan(this.Search_TextBox, 1);
            //        Grid.SetColumnSpan(this.btnHistory, 1);
            //        Grid.SetColumnSpan(this.btnFullScreen, 1);
            //        this.gridSplitter.Visibility = Visibility.Visible;
            //        this.marketExecution.Visibility = Visibility.Visible;
            //    }
            //    else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Header.ToString() == "我的成交")
            //    {
            //        Grid.SetColumnSpan(this.TradeInfo_tabControl, 3);
            //        Grid.SetColumnSpan(this.Search_TextBox, 3);
            //        Grid.SetColumnSpan(this.btnHistory, 3);
            //        Grid.SetColumnSpan(this.btnFullScreen, 3);
            //        this.gridSplitter.Visibility = Visibility.Collapsed;
            //        this.marketExecution.Visibility = Visibility.Collapsed;
            //    }
            //}

            if (this.TradeInfo_tabControl.SelectedItem != null)
            {
                if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyOrders_TabItem")
                {
                    //我的订单
                    this.myOrderFilter.Visibility = Visibility.Visible;
                    this.myExeFilter.Visibility = Visibility.Hidden;
                    this.myCancelOrderFilter.Visibility = Visibility.Hidden;
                    //this.Search_TextBox.Visibility = Visibility.Visible;
                    this.btnFullScreen.Command = Utils.WindowKeyHelper.popupWindowCommand;
                    this.btnFullScreen.CommandParameter = PopupTypeEnum.MyOrders;
                    //this.btnHistory.Visibility = Visibility.Visible;
                    this.myOrdersTB.Visibility = Visibility.Visible;
                    this.myExesTB.Visibility = Visibility.Collapsed;
                    this.myCancelOrdersTB.Visibility = Visibility.Collapsed;
                    this.myRfqFilter.Visibility = Visibility.Collapsed;
                    _myTraderViewModel.IsMyOrdersShowRedDot = "";
                    _myTraderViewModel.TradeTabSelectedItem = "MyOrders_TabItem";

                }
                else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyExecutions_TabItem")
                {
                    //我的成交
                    this.myOrderFilter.Visibility = Visibility.Hidden;
                    this.myExeFilter.Visibility = Visibility.Visible;
                    this.myCancelOrderFilter.Visibility = Visibility.Hidden;
                    //this.Search_TextBox.Visibility = Visibility.Collapsed;
                    this.btnFullScreen.Command = Utils.WindowKeyHelper.popupWindowCommand;
                    this.btnFullScreen.CommandParameter = PopupTypeEnum.MyExecutions;
                    //this.btnHistory.Visibility = Visibility.Visible;
                    this.myOrdersTB.Visibility = Visibility.Collapsed;
                    this.myExesTB.Visibility = Visibility.Visible;
                    this.myCancelOrdersTB.Visibility = Visibility.Collapsed;
                    this.myRfqFilter.Visibility = Visibility.Collapsed;
                    _myTraderViewModel.IsMyExecutionsShowRedDot = "";
                    _myTraderViewModel.TradeTabSelectedItem = "MyExecutions_TabItem";
                }
                else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyCancels_TabItem")
                {
                    //已撤订单
                    this.myOrderFilter.Visibility = Visibility.Hidden;
                    this.myExeFilter.Visibility = Visibility.Hidden;
                    this.myCancelOrderFilter.Visibility = Visibility.Visible;
                    //this.Search_TextBox.Visibility = Visibility.Visible;
                    this.btnFullScreen.Command = Utils.WindowKeyHelper.popupWindowCommand;
                    this.btnFullScreen.CommandParameter = PopupTypeEnum.MyCancelOrders;
                    //this.btnHistory.Visibility = Visibility.Visible;
                    this.myOrdersTB.Visibility = Visibility.Collapsed;
                    this.myExesTB.Visibility = Visibility.Collapsed;
                    this.myCancelOrdersTB.Visibility = Visibility.Visible;
                    this.myRfqFilter.Visibility = Visibility.Collapsed;
                    _myTraderViewModel.TradeTabSelectedItem = "MyCancels_TabItem";
                }
                else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyRfq_TabItem")
                {
                    //RFQ
                    //to do...
                    this.myOrderFilter.Visibility = Visibility.Hidden;
                    this.myExeFilter.Visibility = Visibility.Hidden;
                    this.myCancelOrderFilter.Visibility = Visibility.Hidden;
                    //this.Search_TextBox.Visibility = Visibility.Hidden;
                    this.btnFullScreen.Command = Utils.WindowKeyHelper.popupWindowCommand;
                    this.btnFullScreen.CommandParameter = PopupTypeEnum.RfqOrderList;
                    //this.btnHistory.Visibility = Visibility.Collapsed;
                    this.myOrdersTB.Visibility = Visibility.Collapsed;
                    this.myExesTB.Visibility = Visibility.Collapsed;
                    this.myCancelOrdersTB.Visibility = Visibility.Collapsed;
                    this.myRfqFilter.Visibility = Visibility.Visible;
                    _myTraderViewModel.TradeTabSelectedItem = "MyRfq_TabItem";
                    _myTraderViewModel.IsRfqShowRedDot = "";
                    //RFQ列表
                    //先判断是否有该插件
                    IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin((int)PluginKeyEnum.RfqPlugin);
                    if (plugin != null)
                    {
                        object postionctl = AppUtils.Application.GetObject($"{(int)PluginKeyEnum.RfqPlugin}_RfqOrderList");
                        object rfqtopbar = AppUtils.Application.GetObject($"{(int)PluginKeyEnum.RfqPlugin}_RfqTopBar");
                        if (postionctl != null)
                        {
                            rfqorderlistview = postionctl;
                            MyRfq_TabItem.Content = rfqorderlistview;

                            myRfqFilter.Content = rfqtopbar;
                            //PropertyInfo isloadProperyInfo = postionctl.GetType().GetProperty("IsLoad");
                            //bool isload = (bool)isloadProperyInfo?.GetValue(postionctl);
                        }
                        else
                        {

                        }
                    }

                }
            }
        }

        private void CheckBox_All_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (_myTraderViewModel != null)
            {
                _myTraderViewModel.CheckBoxAllValueChange();
            }
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            //if (_myTraderViewModel != null)
            //{
            //    _myTraderViewModel.AdjustAllChkStatus();
            //}
        }

        private void myOrderFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is MyTraderViewModel)
            {
                MyTraderViewModel myTraderViewModel = this.DataContext as MyTraderViewModel;
                myTraderViewModel.ProcessMyOrdersFilterCondition(this.myOrderFilter.FilterDic);
            }
        }

        private void myExeFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is MyTraderViewModel)
            {
                MyTraderViewModel myTraderViewModel = this.DataContext as MyTraderViewModel;
                myTraderViewModel.ProcessMyExecutionsFilterConditon(this.myExeFilter.FilterDic);
            }
        }

        private void CancelOrders_CheckBox_All_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (_myTraderViewModel != null)
            {
                _myTraderViewModel.CheckBoxAllValueChange4CancelOrders();
            }
        }

        private void CancelOrders_Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            //if (_myTraderViewModel != null)
            //{
            //    _myTraderViewModel.AdjustAllChkStatus4CancelOrders();
            //}
        }

        private void myCancelOrderFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is MyTraderViewModel)
            {
                MyTraderViewModel myTraderViewModel = this.DataContext as MyTraderViewModel;
                myTraderViewModel.ProcessMyCancelOrdersFilterCondition(this.myOrderFilter.FilterDic);
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
                if (placementTargetObject != null)
                {
                    bool isChange = false;
                    if (MyTrading.CrossPopup.PlacementTarget != placementTargetObject)
                    {
                        //MyTrading.CrossPopup.IsOpen = false;
                        isChange = true;
                        MyTrading.CrossPopup.PlacementTarget = placementTargetObject;
                        MyTrading.CrossPopup.Placement = PlacementMode.Bottom;
                        MyTrading.CrossPopup.HorizontalOffset = 0;
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

                    //if (MyTrading.CrossPopup.IsOpen == false)
                    if (isChange == true)
                    {
                        if (placementTargetObject is DataGridCell)
                        {
                            DataGridCell c = placementTargetObject as DataGridCell;
                            DataRowView currentItem = c.DataContext as DataRowView;

                            if (currentItem != null && currentItem.Row.RowState != DataRowState.Detached && MyTrading.CrossPopup != null && MyTrading.CrossPopup.Child != null && MyTrading.CrossPopup.Child is CrossQtyPopup)
                            {
                                CrossQtyPopup crossQtyPopup = MyTrading.CrossPopup.Child as CrossQtyPopup;
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
                                    //4=债券，5=国债期货
                                    viewModel.MarketIndicator = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.MARKETINDICATOR_FIELD]);
                                    viewModel.HeaderAmount = Consts.MARKETINDICATOR_FUTURE.Equals(viewModel.MarketIndicator) ? "量(手)" : "量(万)";

                                    string algoName = CommonUtils.ConvertToString(currentItem[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                                    if (algoName.ToUpper().Equals("STP"))
                                    {
                                        viewModel.SendExecType = "DMA";
                                    }
                                    else
                                    {
                                        viewModel.SendExecType = "OCO";
                                    }

                                    viewModel.getEmsOrder();
                                }
                            }
                        }


                        // 显示浮动框
                        MyTrading.CrossPopup.IsOpen = true;
                    }
                }

                DataGridCell arrowObj = MyTrading.CrossPopup.PlacementTarget as DataGridCell;
                if (!MyTrading.CrossPopup.IsMouseOver)
                {
                    if (arrowObj != null)
                    {
                        if (!arrowObj.IsMouseOver)
                        {
                            MyTrading.CrossPopup.IsOpen = false;
                            //MyTrading.placementTargetObject = null;
                            //MyTrading.CrossPopup.PlacementTarget = null;
                            MyTrading.Timer4ControlDetailPopup.Stop();
                        }
                    }
                    else
                    {
                        MyTrading.CrossPopup.IsOpen = false;
                        //MyTrading.placementTargetObject = null;
                        //MyTrading.CrossPopup.PlacementTarget = null;
                        MyTrading.Timer4ControlDetailPopup.Stop();
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

            if (MyTrading.Timer4ControlDetailPopup.IsEnabled == false)
            {
                MyTrading.Timer4ControlDetailPopup.Start();
            }
        }
        #endregion

        #region 时钟倒计时显示
        DataRowView drv;
        private void clock_img_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((Button)sender).DataContext is DataRowView)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    drv = ((Button)sender).DataContext as DataRowView;

                    if (firmToNoTimer.IsEnabled == false)
                    {
                        firmToNoTimer.Start();
                    }
                });
            }

        }

        private void clock_img_MouseLeave(object sender, MouseEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (firmToNoTimer.IsEnabled == true)
                {
                    firmToNoTimer.Stop();
                }
            });
        }

        private void FirmToNoTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
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
                });
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

        private void MyOrders_dataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMyOrderOperationGridMargin();
        }

        private void SetMyOrderOperationGridMargin()
        {
            //PART_HorizontalScrollBar
            ScrollBar hScrollBar = CommonUtils.GetVisualChild<ScrollBar>(MyOrders_dataGrid, dp => dp.Name == "PART_HorizontalScrollBar" && dp.Tag != null);

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

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            bool isExec = false;
            if (this.TradeInfo_tabControl.SelectedItem != null)
            {
                if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyExecutions_TabItem")
                {
                    //我的成交
                    _myTraderViewModel.OpenHistoryPage(true);
                    isExec = true;
                }
            }
            if (!isExec)
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
                //contextMenu.IsOpen = true;
            }
        }

        private void MyExecutions_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.MyExecutions_dataGrid.SelectedItems.Count > 0)
            {
                SetContextMenu();
                //contextMenu.IsOpen = true;
            }
        }

        private void MyCancels_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.MyCancels_dataGrid.SelectedItems.Count > 0)
            {
                SetContextMenu();
                //contextMenu.IsOpen = true;
            }
        }

        private void copyRow_Click(object sender, RoutedEventArgs e)
        {
            if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyOrders_TabItem")
            {
                //我的订单
                string value = _myTraderViewModel.CopyRowText(this.MyOrders_dataGrid.SelectedItems, "Orders");
                ClipboardHelper.CopyToClipboard(value);
                MyOrders_dataGrid.UnselectAll();
            }
            else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyExecutions_TabItem")
            {
                //我的成交
                string value = _myTraderViewModel.CopyRowText(this.MyExecutions_dataGrid.SelectedItems, "Deals");
                ClipboardHelper.CopyToClipboard(value);
                MyExecutions_dataGrid.UnselectAll();
            }
            else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyCancels_TabItem")
            {
                //已撤订单
                string value = _myTraderViewModel.CopyRowText(this.MyCancels_dataGrid.SelectedItems, "Cancels");
                ClipboardHelper.CopyToClipboard(value);
                MyCancels_dataGrid.UnselectAll();
            }
        }

        private void copyRow_institution_Click(object sender, RoutedEventArgs e)
        {
            if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyExecutions_TabItem")
            {
                //我的成交
                string value = _myTraderViewModel.CopyRowText(this.MyExecutions_dataGrid.SelectedItems, "Deals_Institution");
                ClipboardHelper.CopyToClipboard(value);
                MyExecutions_dataGrid.UnselectAll();
            }
        }

        private void copySTCCode_Click(object sender, RoutedEventArgs e)
        {
            if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyOrders_TabItem")
            {
                //我的订单
                string value = _myTraderViewModel.CopySTCCodeText(this.MyOrders_dataGrid.SelectedItems, "Orders");
                ClipboardHelper.CopyToClipboard(value);
                MyOrders_dataGrid.UnselectAll();
            }
            else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyExecutions_TabItem")
            {
                //我的成交
                string value = _myTraderViewModel.CopySTCCodeText(this.MyExecutions_dataGrid.SelectedItems, "Deals");
                ClipboardHelper.CopyToClipboard(value);
                MyExecutions_dataGrid.UnselectAll();
            }
            else if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyCancels_TabItem")
            {
                //已撤订单
                string value = _myTraderViewModel.CopySTCCodeText(this.MyCancels_dataGrid.SelectedItems, "Cancels");
                ClipboardHelper.CopyToClipboard(value);
                MyCancels_dataGrid.UnselectAll();
            }
        }

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (LocalEnv.ConfInfo.IsQQ)
            {
                if (this._myTraderViewModel.SelectedItem_MyOrders != null)
                {
                    //string inf2 = this._myTraderViewModel.SelectedItem_MyOrders[MyOrderProcesser.INFO2_FIELD].ToString();
                    //if (!string.IsNullOrEmpty(inf2))
                    //{
                    //    TagData tg = JsonConvert.DeserializeObject<TagData>(inf2);
                    //    if (tg != null && !string.IsNullOrEmpty(tg.fromQQ))
                    //    {
                    //        StartQQBtn.IsEnabled = true;
                    //        StartQQ2Btn.IsEnabled = true;
                    //    }
                    //    else
                    //    {
                    //        StartQQBtn.IsEnabled = false;
                    //        StartQQ2Btn.IsEnabled = false;
                    //    }
                    //}
                }
            }
        }

        public void HighligthRow(string intentionId)
        {
            //我的订单
            if (this.MyOrders_TabItem.IsSelected == true)
            {
                DataRowView drv = this._myTraderViewModel.HighlightRow_MyOrder(intentionId);
                if (drv != null)
                {
                    this.MyOrders_dataGrid.ScrollIntoView(drv);
                }
            }
            //已撤订单
            else if (this.MyOrders_TabItem.IsSelected == true)
            {
                drv = this._myTraderViewModel.HighlightRow_MyCancelOrder(intentionId);
                if (drv != null)
                {
                    this.MyCancels_dataGrid.ScrollIntoView(drv);
                }
            }
            //我的成交
            else if (this.MyExecutions_TabItem.IsSelected == true)
            {
                DataRowView exedrv = this._myTraderViewModel.HighlightRow_MyExe(intentionId);
                if (exedrv != null)
                {
                    this.MyExecutions_dataGrid.ScrollIntoView(exedrv);
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._myTraderViewModel.UpdateExe();
        }


        private void EnterTransaction_Click(object sender, RoutedEventArgs e)
        {
            EnterTransaction();
        }

        private void MyOrderFilter_OnManualClick(object arg1, RoutedEventArgs arg2)
        {
            EnterTransaction();
        }

        public void EnterTransaction()
        {
            if ((this.TradeInfo_tabControl.SelectedItem as TabItem).Name.ToString() == "MyOrders_TabItem")
            {
                //我的订单
                DataRowView drv = (DataRowView)this.MyOrders_dataGrid.SelectedItem;
                PTHandler.Instance.OnSendPT(drv);
            }
        }

        private void StartQQBtn_Click(object sender, RoutedEventArgs e)
        {
            Sub_DoStartQQCommand(false);
        }

        private void StartQQ2Btn_Click(object sender, RoutedEventArgs e)
        {
            Sub_DoStartQQCommand(true);
        }

        private void Sub_DoStartQQCommand(bool is_send_direct)
        {
            if (!LocalEnv.ConfInfo.IsQQ)
            {
                return;
            }

            string receiver = string.Empty;
            string inf2 = this._myTraderViewModel.SelectedItem_MyOrders[MyOrderProcesser.INFO2_FIELD].ToString();
            if (!string.IsNullOrEmpty(inf2))
            {
                TagData tg = JsonConvert.DeserializeObject<TagData>(inf2);
                if (tg != null && !string.IsNullOrEmpty(tg.fromQQ))
                    receiver = tg.fromQQ;
            }



            string remainingTime = string.Empty;
            string symbol = this._myTraderViewModel.SelectedItem_MyOrders[MyOrderProcesser.SYMBOL_ORDER_FIELD].ToString();
            string securityid = this._myTraderViewModel.SelectedItem_MyOrders[MyOrderProcesser.SECURITYID_ORDER_FIELD].ToString();
            BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityid);
            if (bondsInfoPo != null && string.IsNullOrEmpty(remainingTime))
            {
                remainingTime = bondsInfoPo.RemaingingTime;
            }
            string side = this._myTraderViewModel.SelectedItem_MyOrders[MyOrderProcesser.SIDE_ORDER_FIELD].ToString() == "1" ? "Bid" : "Ofr";
            string price_temp = this._myTraderViewModel.SelectedItem_MyOrders[MyOrderProcesser.PRICE_ORDER_FIELD].ToString();
            string price = string.IsNullOrEmpty(price_temp) ? string.Empty : $" {price_temp}";
            string qty_temp = this._myTraderViewModel.SelectedItem_MyOrders[MyOrderProcesser.QTY_ORDER_FIELD].ToString();
            string qty = string.IsNullOrEmpty(qty_temp) ? string.Empty : $" {qty_temp}w";
            string settlType_temp = this._myTraderViewModel.SelectedItem_MyOrders[MyOrderProcesser.SETTLETYPE_ORDER_FIELD].ToString();
            string settlType = string.Empty;
            if (settlType_temp != "2"
                && settlType_temp != "3"
                 && settlType_temp != "2,3")
            {
                settlType = " " + ComboSourceUtil.Instance.FindEnum(settlType_temp, "settletype");
            }
            string message = $"{remainingTime} {symbol} {securityid} {side}{price}{qty}";

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
                ClipboardHelper.CopyToClipboard(message);
            }
            else
            {
                if (string.IsNullOrEmpty(LocalEnv.UserInfo.QQNumber))
                    CommonUtils.Prompt2Second("发起客户单聊QQ", "请联系管理员设置本用户对应QQ号码!");
                else if (string.IsNullOrEmpty(receiver))
                    CommonUtils.Prompt2Second("发起客户单聊QQ", "找不到客户QQ号");
            }
        }
    }
}
