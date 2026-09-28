using netapi.Api;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Order;
using STC.Common.View.Order.InnerPanel;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Order;
using STC.XBond.Plugin.Command;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.View.Book;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.View.Order;
using STC.XBond.Plugin.ViewModel;
using STC.XBond.Plugin.ViewModel.Data;
using STC.XBond.Plugin.ViewModel.TradeBookHandler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.IO;
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
using OrderCancelRequest = STC.Common.Model.TBook.OrderCancelRequest;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// QuotationList.xaml 的交互逻辑
    /// </summary>
    public partial class QuotationList : UserControl
    {
        //定义依赖项属性
        public static DependencyProperty HeaderAmountProperty = DependencyProperty.Register("AmountType", typeof(string), typeof(QuotationList), new PropertyMetadata("0"));

        //声明属性        
        /// <summary>
        /// 默认收益率
        /// </summary>
        public string AmountType
        {
            get { return (string)GetValue(HeaderAmountProperty); }
            set 
            {
                SetValue(HeaderAmountProperty, value);
                string pricetype = (string)value;
                QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                if (pricetype == "0")
                {
                    viewModel.PriceTypeIndex = 0;
                    Switch_Yield_Btn_Click(null, null);
                }
                else
                {
                    viewModel.PriceTypeIndex = 1;
                    Switch_NetPrice_Btn_Click(null, null);
                }
                PriceTypeSelected?.Invoke(this, new PriceTypeSelectedEventArgs(viewModel.PriceTypeIndex));
            }
        }
        public event EventHandler<PriceTypeSelectedEventArgs> PriceTypeSelected;
        public QuotationList()
        {
            InitializeComponent();
            if (QuotationList.DetailPopup == null)
            {
                QuotationList.DetailPopup = new DetailPopup();
                QuotationList.DetailPopup.StaysOpen = true;
            }
            if (QuotationList.Timer4ControlDetailPopup == null)
            {
                QuotationList.Timer4ControlDetailPopup = new DispatcherTimer();
                QuotationList.Timer4ControlDetailPopup.Interval = new TimeSpan(100);
                QuotationList.Timer4ControlDetailPopup.Tick += ControlPopup;
            }

            //KeyDown += QuotationList_KeyDown;
            PreviewKeyDown += QuotationList_PreviewKeyDown;

            this.Unloaded += QuotationList_Unloaded;

            DesignHandler.Instance.OnKeyDown += Instance_OnKeyDown;
        }

        private void Instance_OnKeyDown(KeyEventArgs keyEventArgs)
        {
            object obj = AppUtils.Application.Panel.SelectedItem;
            if (obj != null)
            {
                TabItem tabItem = obj as TabItem;
                if (tabItem != null && tabItem.Tag.ToString() == "22")
                    QuotationList_KeyDown(null, null);
            }
            
        }

        private void QuotationList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void QuotationList_Unloaded(object sender, RoutedEventArgs e)
        {
            DesignHandler.Instance.OnKeyDown -= Instance_OnKeyDown;

            this.Unloaded -= QuotationList_Unloaded;
        }

        private void QuotationList_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                //Ofr
                if (Keyboard.IsKeyDown(Key.O))
                {
                    if (viewModel.ShowOrderBtn == Visibility.Visible)
                    {
                        object selectObj = OfrDataGrid.SelectedItem;

                        if (selectObj == null)
                            selectObj = this.OfrDataGrid_Frozen.SelectedItem;

                        if (selectObj != null)
                        {
                            DataRowView dataGridRow = selectObj as DataRowView;

                            QuickOrderHandler.Instance.QuickPrice(dataGridRow, false, viewModel.SecurityID);
                        }
                        else
                        {
                            QuickOrderHandler.Instance.QuickPrice(null, false, viewModel.SecurityID);
                        }
                    }
                }
                else if (Keyboard.IsKeyDown(Key.B))
                {
                    //Bid
                    if (viewModel.ShowOrderBtn == Visibility.Visible)
                    {
                        object selectObj = BidDataGrid.SelectedItem;

                        if (selectObj == null)
                            selectObj = BidDataGrid_Frozen.SelectedItem;

                        if (selectObj != null)
                        {
                            DataRowView dataGridRow = selectObj as DataRowView;

                            QuickOrderHandler.Instance.QuickPrice(dataGridRow, true, viewModel.SecurityID);
                        }
                        else
                        {
                            QuickOrderHandler.Instance.QuickPrice(null, true, viewModel.SecurityID);
                        }
                    }
                }
            }
            //e.Handled = true;
        }

        #region DetailPopup
        private static DispatcherTimer Timer4ControlDetailPopup;
        private static Grid placementTargetObject = null;
        private static Popup DetailPopup;

        private void ControlPopup(object state, EventArgs e)
        {
            if (placementTargetObject != null)
            {
                if (QuotationList.DetailPopup.PlacementTarget != placementTargetObject)
                {
                    QuotationList.DetailPopup.IsOpen = false;
                    QuotationList.DetailPopup.PlacementTarget = placementTargetObject;
                    QuotationList.DetailPopup.Placement = PlacementMode.Bottom;
                    QuotationList.DetailPopup.HorizontalOffset = 0;
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

                if (QuotationList.DetailPopup.IsOpen == false)
                {
                    // TODO: 绑定数据
                    if (QuotationList.DetailPopup.DataContext != null
                        && QuotationList.DetailPopup.DataContext is DetailPopupViewModel
                        && placementTargetObject != null
                        && placementTargetObject.DataContext is DataRowView)
                    {
                        DataRowView drv = (DataRowView)placementTargetObject.DataContext;
                        // 债券代码
                        string securityID = drv[CTPriceControler.SECURITY_ID_FIELD] == null ? string.Empty : drv[CTPriceControler.SECURITY_ID_FIELD].ToString();
                        // 买卖方向
                        string side_Str = drv[CTPriceControler.SIDE_FIELD] == null ? string.Empty : drv[CTPriceControler.SIDE_FIELD].ToString();
                        string side = side_Str == "1" ? "Ofr" : "Bid";
                        // 清算速度
                        StringBuilder allSettleTypeSB = new StringBuilder();
                        List<string> settleTypeList = new List<string>();
                        GenAllSettlType(drv, ref allSettleTypeSB, ref settleTypeList);
                        // 收益率
                        decimal px_decimal = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                        string px = px_decimal.ToString("N4");
                        // 数量Text
                        string text = CommonUtils.ConvertToString(drv[CTPriceControler.TEXT_FIELD]);
                        // 渠道
                        string source = CommonUtils.ConvertToString(drv[CTPriceControler.SOURCE_FIELD]);
                        string venuesTypeGW = CommonUtils.ConvertToString(drv[CTPriceControler.VENUE_TYPE_GW_FIELD]);


                        DetailPopupViewModel vm = (DetailPopupViewModel)QuotationList.DetailPopup.DataContext;
                        vm.SecurtityID = securityID;
                        vm.Side = side_Str;
                        vm.Px = px_decimal;
                        vm.Source = source;
                        vm.Title = $"{securityID}  {side}  {allSettleTypeSB}  {px}";
                        vm.TotalQty = text;
                        vm.AllSettleType = allSettleTypeSB.ToString();

                        StringBuilder settlTypeRowfilter = new StringBuilder();
                        foreach (string item in settleTypeList)
                        {
                            if (settlTypeRowfilter.Length > 0)
                            {
                                settlTypeRowfilter.Append(",");
                            }
                            settlTypeRowfilter.Append($"'{item}'");

                        }
                        vm.SettleType = settlTypeRowfilter.ToString();
                        vm.VenuesTypeGW = venuesTypeGW;

                        vm.FilterData();
                    }

                    // 显示浮动框
                    QuotationList.DetailPopup.IsOpen = true;
                }
            }

            Grid arrowObj = QuotationList.DetailPopup.PlacementTarget as Grid;
            if (!QuotationList.DetailPopup.IsMouseOver)
            {
                if (arrowObj != null)
                {
                    if (!arrowObj.IsMouseOver)
                    {
                        QuotationList.DetailPopup.IsOpen = false;
                        QuotationList.placementTargetObject = null;
                        QuotationList.DetailPopup.PlacementTarget = null;
                        QuotationList.Timer4ControlDetailPopup.Stop();
                    }
                }
                else
                {
                    QuotationList.DetailPopup.IsOpen = false;
                    QuotationList.placementTargetObject = null;
                    QuotationList.DetailPopup.PlacementTarget = null;
                    QuotationList.Timer4ControlDetailPopup.Stop();
                }
            }
        }

        private void Sub_GenSettleType(string settleType, ref StringBuilder allSettleTypeSB, ref List<string> settleTypeList)
        {
            string[] settlTypeSplit = null;
            //if (settleType.Equals(Constants.T2_T3))
            //    settlTypeSplit = new string[] { settleType };
            //else
            settlTypeSplit = settleType.Split(',');
            foreach (string settlTypeItem in settlTypeSplit)
            {
                string item = settlTypeItem.Trim();
                if (string.IsNullOrEmpty(item) == false)
                {
                    // 原始值拼凑过滤条件
                    if (settleTypeList.Contains(item) == false)
                    {
                        settleTypeList.Add($"{item}");

                        // 枚举值拼凑显示内容
                        string settlType = ComboSourceUtil.Instance.FindEnum(item, "settletype");
                        if (settlType == "远期")
                        {
                            settlType = item;
                        }
                        //if (settlType == Constants.T2_T3)
                        //    settlType = Constants.T2_3_CN;

                        if (allSettleTypeSB.Length > 0)
                        {
                            allSettleTypeSB.Append(",");
                        }
                        allSettleTypeSB.Append(settlType);
                    }
                }
            }
        }

        private void GenAllSettlType(DataRowView drv, ref StringBuilder allSettleTypeSB, ref List<string> settleTypeList)
        {
            string settleType = CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_FIELD]);
            Sub_GenSettleType(settleType, ref allSettleTypeSB, ref settleTypeList);

            string settleType_yuan = CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD]);
            Sub_GenSettleType(settleType_yuan, ref allSettleTypeSB, ref settleTypeList);
        }

        private void Arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            //this.detailPopup.PlacementTarget = (UIElement)sender;
            //this.detailPopup.IsOpen = true;
            placementTargetObject = (Grid)sender;
            if (placementTargetObject.DataContext != null && placementTargetObject.DataContext is DataRowView)
            {
                DataRowView drv = placementTargetObject.DataContext as DataRowView;
                string flagType = CommonUtils.ConvertToString(drv[CTPriceControler.FLAG_TYPE_FIELD]);
                if (string.IsNullOrEmpty(flagType) == false)
                {
                    QuotationList.DetailPopup.PlacementTarget = placementTargetObject;

                    if (QuotationList.Timer4ControlDetailPopup.IsEnabled == false)
                    {
                        QuotationList.Timer4ControlDetailPopup.Start();
                    }
                }
            }
        }

        private void Arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            //this.detailPopup.PlacementTarget = (UIElement)sender;
            //this.detailPopup.IsOpen = false;
        }


        #endregion


        private void UserControl_Initialized(object sender, EventArgs e)
        {
        }


        private void lbPirceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                if (viewModel.PriceTypeIndex == 0)
                {
                    Switch_Yield_Btn_Click(sender, e);
                }
                else if (viewModel.PriceTypeIndex == 1)
                {
                    Switch_NetPrice_Btn_Click(sender, e);
                }
                else
                {
                    Switch_EntryPx_Btn_Click(sender, e);
                }
                PriceTypeSelected?.Invoke(this, new PriceTypeSelectedEventArgs(viewModel.PriceTypeIndex));
            }
        }

        public void Switch_Yield_Btn_Click(object sender, RoutedEventArgs e)
        {
            bool isCheckExe = false;
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                viewModel.IsShowNetPrice = false;
                if (viewModel.CheckExeRadio) 
                {
                    isCheckExe = true;
                }
            }

            this.bidNetPriceColumn_Frozen.Visibility = Visibility.Collapsed;
            this.OfrNetPriceColumn_Frozen.Visibility = Visibility.Collapsed;
            this.bidEntryPxColumn_Frozen.Visibility = Visibility.Collapsed;
            this.OfrEntryPxColumn_Frozen.Visibility = Visibility.Collapsed;
            if (isCheckExe)
            {
                this.bidYieldToExecutionColumn_Frozen.Visibility = Visibility.Visible;
                this.OfrYieldToExecutionColumn_Frozen.Visibility = Visibility.Visible;
                this.bidYieldColumn_Frozen.Visibility = Visibility.Collapsed;
                this.OfrYieldColumn_Frozen.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.bidYieldToExecutionColumn_Frozen.Visibility = Visibility.Collapsed;
                this.OfrYieldToExecutionColumn_Frozen.Visibility = Visibility.Collapsed;
                this.bidYieldColumn_Frozen.Visibility = Visibility.Visible;
                this.OfrYieldColumn_Frozen.Visibility = Visibility.Visible;
            }

            this.bidNetPriceColumn.Visibility = Visibility.Collapsed;
            this.OfrNetPriceColumn.Visibility = Visibility.Collapsed;
            this.bidEntryPxColumn.Visibility = Visibility.Collapsed;
            this.OfrEntryPxColumn.Visibility = Visibility.Collapsed;
            if (isCheckExe)
            {
                this.bidYieldToExecutionColumn.Visibility = Visibility.Visible;
                this.OfrYieldToExecutionColumn.Visibility = Visibility.Visible;
                this.bid_InComplete_YieldToExecutionColumn.Visibility = Visibility.Visible;
                this.Ofr_Incomplete_YieldToExecutionColumn.Visibility = Visibility.Visible;
                this.bidYieldColumn.Visibility = Visibility.Collapsed;
                this.OfrYieldColumn.Visibility = Visibility.Collapsed;
                this.bid_InComplete_YieldColumn.Visibility = Visibility.Collapsed;
                this.Ofr_Incomplete_YieldColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.bidYieldToExecutionColumn.Visibility = Visibility.Collapsed;
                this.OfrYieldToExecutionColumn.Visibility = Visibility.Collapsed;
                this.bid_InComplete_YieldToExecutionColumn.Visibility = Visibility.Collapsed;
                this.Ofr_Incomplete_YieldToExecutionColumn.Visibility = Visibility.Collapsed;
                this.bidYieldColumn.Visibility = Visibility.Visible;
                this.OfrYieldColumn.Visibility = Visibility.Visible;
                this.bid_InComplete_YieldColumn.Visibility = Visibility.Visible;
                this.Ofr_Incomplete_YieldColumn.Visibility = Visibility.Visible;
            }

            this.bid_InComplete_NetPriceColumn.Visibility = Visibility.Collapsed;
            this.Ofr_Incomplete_NetPriceColumn.Visibility = Visibility.Collapsed;
        }

        private void Switch_NetPrice_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                viewModel.IsShowNetPrice = true;
            }
            this.bidNetPriceColumn_Frozen.Visibility = Visibility.Visible;
            this.OfrNetPriceColumn_Frozen.Visibility = Visibility.Visible;
            this.bidEntryPxColumn_Frozen.Visibility = Visibility.Collapsed;
            this.OfrEntryPxColumn_Frozen.Visibility = Visibility.Collapsed;
            this.bidYieldColumn_Frozen.Visibility = Visibility.Collapsed;
            this.OfrYieldColumn_Frozen.Visibility = Visibility.Collapsed;

            this.bidNetPriceColumn.Visibility = Visibility.Visible;
            this.OfrNetPriceColumn.Visibility = Visibility.Visible;
            this.bidEntryPxColumn.Visibility = Visibility.Collapsed;
            this.OfrEntryPxColumn.Visibility = Visibility.Collapsed;
            this.bidYieldColumn.Visibility = Visibility.Collapsed;
            this.OfrYieldColumn.Visibility = Visibility.Collapsed;
            this.bid_InComplete_YieldColumn.Visibility = Visibility.Collapsed;
            this.Ofr_Incomplete_YieldColumn.Visibility = Visibility.Collapsed;

            this.bid_InComplete_NetPriceColumn.Visibility = Visibility.Visible;
            this.Ofr_Incomplete_NetPriceColumn.Visibility = Visibility.Visible;
            /*this.bid_InComplete_EntryPxColumn.Visibility = Visibility.Collapsed;
            this.Ofr_Incomplete_EntryPxColumn.Visibility = Visibility.Collapsed;*/
            this.bidYieldToExecutionColumn_Frozen.Visibility = Visibility.Collapsed;
            this.OfrYieldToExecutionColumn_Frozen.Visibility = Visibility.Collapsed;
            this.bidYieldToExecutionColumn.Visibility = Visibility.Collapsed;
            this.OfrYieldToExecutionColumn.Visibility = Visibility.Collapsed;
            this.bid_InComplete_YieldToExecutionColumn.Visibility = Visibility.Collapsed;
            this.Ofr_Incomplete_YieldToExecutionColumn.Visibility = Visibility.Collapsed;
        }

        private void Switch_EntryPx_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                viewModel.IsShowNetPrice = true;
            }
            this.bidNetPriceColumn_Frozen.Visibility = Visibility.Collapsed;
            this.OfrNetPriceColumn_Frozen.Visibility = Visibility.Collapsed;
            this.bidEntryPxColumn_Frozen.Visibility = Visibility.Visible;
            this.OfrEntryPxColumn_Frozen.Visibility = Visibility.Visible;
            this.bidYieldColumn_Frozen.Visibility = Visibility.Collapsed;
            this.OfrYieldColumn_Frozen.Visibility = Visibility.Collapsed;

            this.bidNetPriceColumn.Visibility = Visibility.Collapsed;
            this.OfrNetPriceColumn.Visibility = Visibility.Collapsed;
            this.bidEntryPxColumn.Visibility = Visibility.Visible;
            this.OfrEntryPxColumn.Visibility = Visibility.Visible;
            this.bidYieldColumn.Visibility = Visibility.Collapsed;
            this.OfrYieldColumn.Visibility = Visibility.Collapsed;
            this.bid_InComplete_YieldColumn.Visibility = Visibility.Collapsed;
            this.Ofr_Incomplete_YieldColumn.Visibility = Visibility.Collapsed;

            this.bid_InComplete_NetPriceColumn.Visibility = Visibility.Collapsed;
            this.Ofr_Incomplete_NetPriceColumn.Visibility = Visibility.Collapsed;
           /* this.bid_InComplete_EntryPxColumn.Visibility = Visibility.Visible;
            this.Ofr_Incomplete_EntryPxColumn.Visibility = Visibility.Visible;*/

            this.bidYieldToExecutionColumn_Frozen.Visibility = Visibility.Collapsed;
            this.OfrYieldToExecutionColumn_Frozen.Visibility = Visibility.Collapsed;
            this.bidYieldToExecutionColumn.Visibility = Visibility.Collapsed;
            this.OfrYieldToExecutionColumn.Visibility = Visibility.Collapsed;
            this.bid_InComplete_YieldToExecutionColumn.Visibility = Visibility.Collapsed;
            this.Ofr_Incomplete_YieldToExecutionColumn.Visibility = Visibility.Collapsed;
        }
        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
                if (viewModel != null)
                {
                    //国债期货报价
                    if (CommonUtils.GetFutureType(viewModel.SecurityID, viewModel.MarketIndicator) == 1)
                    {
                        if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceFutureWinKey(WindowKeyHelper.BID, viewModel.SecurityID)))
                        {
                            SendPriceFutureViewLite sendPriceView = new SendPriceFutureViewLite(viewModel.SecurityID, true);
                            WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceFutureWinKey(WindowKeyHelper.BID, viewModel.SecurityID), sendPriceView);
                        }
                        return;
                    }
                    //商品期货报价
                    if (CommonUtils.GetFutureType(viewModel.SecurityID, viewModel.MarketIndicator) == 2)
                    {
                        if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceCommodityWinKey(WindowKeyHelper.BID, viewModel.SecurityID)))
                        {
                            SendPriceCommodityViewLite sendPriceView = new SendPriceCommodityViewLite(viewModel.SecurityID, true);
                            WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceCommodityWinKey(WindowKeyHelper.BID, viewModel.SecurityID), sendPriceView);
                        }
                        return;
                    }

                    if (viewModel.MarketIndicator == "2")
                    {
                        //IRS报价
                        SendIRSView sendPriceView = new SendIRSView(viewModel.SecurityID, true, false, false);
                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, viewModel.SecurityID), sendPriceView);
                    }
                    else if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, viewModel.SecurityID)))
                    {
                        //债券报价
                        //SendPriceViewLite sendPriceView = new SendPriceViewLite(viewModel.SecurityID, true);

                        SendPriceView sendPriceView = new SendPriceView(viewModel.SecurityID, true);

                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, viewModel.SecurityID), sendPriceView);
                        //sendPriceView.Owner = Application.Current.MainWindow;
                        //sendPriceView.Show();
                    }
                }
            }
        }
        
        private ExecutionReport GetExecutionReport(DataRowView dr)
        {
            ExecutionReport order = new ExecutionReport();
            
            order.Yield = CommonUtils.ConvertToString(dr[CTPriceControler.ENTRY_PX_FIELD]);
            order.LastQty = CommonUtils.ConvertToString(dr[CTPriceControler.QTY_FIELD]);
            order.SettlType = GetSettle(dr);
            order.InstitutionName = CommonUtils.ConvertToString(dr[CTPriceControler.INSTITUTION_NAME_FIELD]);
            order.PartySubName = CommonUtils.ConvertToString(dr[CTPriceControler.PARTY_SUB_NAME_FIELD]);

            if (!string.IsNullOrEmpty(order.PartySubName) && order.PartySubName.Contains("-"))
            {
                string[] arr = order.PartySubName.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                    order.PartySubName = arr[1];
            }

            return order;
        }

        private void QuickUnderline(string side, DataRowView drv)
        {
            List<Inline> txtRun = new List<Inline>();
            Run run = new Run(Constants.SHOW_QUICK_EXEC + "\n\n" + Constants.SHOW_QUICK_EXEC_WARN);
            //Run runO = new Run(Constants.SHOW_QUICK_EXEC_WARN);

            txtRun.Add(run);
            //txtRun.Add(runO);

            bool result = Common.View.Component.CustomMessageBox.ShowDialogAndResult(Common.View.Component.CustomMessageBox.EnumNotifyType.Question, txtRun);

            if (result)
            {
                DealDelete(CommonUtils.GetOrderId(CommonUtils.ConvertToString(drv[CTPriceControler.KEY_FIELD])), false);
                BookHandler.Instance.RequestAccountInfoQuery(); 
                BookEditView bookEditView = new BookEditView(null, true, true);

                string veneues = GetSource(drv);
                string partySubName = drv[CTPriceControler.PARTY_SUB_NAME_FIELD] == null ? string.Empty : drv[CTPriceControler.PARTY_SUB_NAME_FIELD].ToString();
                string traderName = drv[CTPriceControler.TRADERNAME_FIELD] == null ? string.Empty : drv[CTPriceControler.TRADERNAME_FIELD].ToString();

                string panelType = Constants.IDB_UNDERLINE_PART;
                

                ExecutionReport order = GetExecutionReport(drv);
                order.Side = side;
                order.Venues = veneues;
                order.TraderName = traderName;

                bool isSale = BookHandler.Instance.IsSale(order.PartySubName);

                panelType = isSale ? Constants.IDB_UNDERLINE_PART : Constants.SELF_PART;

                order.SalesAgentFlag = panelType;

                BookEditViewModel model = new BookEditViewModel(null, true, order);

                model.Side = side;
                if (side.Equals("1"))
                {
                    model.Side = "2";
                    model.SellIsCheck = true;
                }
                else
                {
                    model.Side = "1";
                    model.BuyIsCheck = true;
                }

                QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                model.Symbol = viewModel.Symbol;
                model.SecurityID = CommonUtils.ConvertToString(drv[CTPriceControler.SECURITY_ID_FIELD]);

                model.Price = GetClickPrice(drv);
                model.Qty = GetClickAmount(drv);
                model.SettlType = GetSettle(drv);
                bookEditView.SetDataContext(model);
                bookEditView.Owner = Application.Current.MainWindow;
                bookEditView.Show();

            }
        }

        //private bool CheckVenuePermission(string venue, string security)
        //{
        //    //if (!string.IsNullOrEmpty(venue) && venue.ToUpper() == "MANUAL")
        //    //    return true;

        //    string goodsType = CommonUtils.SetGoodsShortname(security);

        //    List<Venue> venues = SystemParamHandler.Instance.GetListVenueByFilter(goodsType, string.Empty);

        //    bool result = false;
        //    if (venues != null)
        //    {
        //        foreach (Venue ve in venues)
        //        {
        //            if (ve.venue.ToUpper() == venue.ToUpper())
        //            {
        //                result = true;

        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        private void BidDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dg = sender as DataGridRow;

            QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
            //如果渠道执行仅有内部权限
            bool isInner = AdminHandler.Instance.CheckPermission("1002", "100201");
            if (isInner && !IsInnerPermission(dg.Item as DataRowView)) 
                return;


            //如果渠道执行有内部权限+XBOND
            bool isInnerXbond = AdminHandler.Instance.CheckPermission("1002", "100202");
            if (isInnerXbond && !IsInnerXbondPermission(dg.Item as DataRowView))
                return;

            if (!IsPermission(dg.Item as DataRowView))
            {
                return;
            }

            if (!PermissionHandler.Instance.CheckVenuePermission(GetSource(dg.Item as DataRowView), viewModel.SecurityID))
                return;

            if (viewModel.ShowOrderBtn != Visibility.Visible)
            {
                return;
            }

            if (this.OpenBatchWin(sender, e))
                return;
            //是否允许发起点价单，允许为true,不允许为false
            if (!LocalEnv.ConfInfo.EnableIOC)
                return;
            //双击国债期货行情
            if (CommonUtils.GetFutureType(viewModel.SecurityID,viewModel.MarketIndicator)==1)
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg.Item as DataRowView))))
                {
                    SendPriceFutureClickViewLite sendPriceFutureClickView = new SendPriceFutureClickViewLite(viewModel.SecurityID, false, GetClickPrice3(dg.Item as DataRowView), GetClickAmount(dg.Item as DataRowView));
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg.Item as DataRowView)), sendPriceFutureClickView);
                    return;
                }
            }

            //双击商品期货行情
            if (CommonUtils.GetFutureType(viewModel.SecurityID, viewModel.MarketIndicator) == 2)
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg.Item as DataRowView))))
                {
                    SendPriceCommodityClickViewLite sendPriceFutureClickView = new SendPriceCommodityClickViewLite(viewModel.SecurityID, false, GetClickPrice3(dg.Item as DataRowView), GetClickAmount(dg.Item as DataRowView));
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg.Item as DataRowView)), sendPriceFutureClickView);
                    return;
                }
            }

            //if (IsMe(dg.Item as DataRowView))
            //{
            //    QuickUnderline("1", dg.Item as DataRowView);

            //    return;
            //}


            //if (IsFirmFlag(dg.Item as DataRowView))
            {
                //if (CheckParam(dg.Item as DataRowView))
                //    return;

                StringBuilder allSettleTypeSB = new StringBuilder();
                List<string> settleTypeList = new List<string>();
                GenAllSettlType(dg.Item as DataRowView, ref allSettleTypeSB, ref settleTypeList);

                //if (CheckMultipleSettle(allSettleTypeSB.ToString()))
                //{
                //    string priceInfo = string.Format("{0} {1} {2} {3}", viewModel.SecurityID, "Bid", allSettleTypeSB.ToString(), GetClickPrice(dg.Item as DataRowView));

                //    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg.Item as DataRowView))))
                //    {
                //        PointPriceSettleView pointPriceSettleView = new PointPriceSettleView(priceInfo, allSettleTypeSB.ToString(), GetPriceMatchParam(dg.Item as DataRowView, "1"));
                //        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg.Item as DataRowView)), pointPriceSettleView);
                //    }
                //}
                //else
                {
                    bool isQuick = QuickOrderHandler.Instance.QuickOrder(dg.Item as DataRowView, true, null, false);

                    if (!isQuick && !WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg.Item as DataRowView))))
                    {
                        bool isdefaultdrp = false;
                        // 如果无价无量，并且不开启定向询价，则返回
                        if (LocalEnv.ConfInfo.IsShowDRP == "0")
                        {
                            if (string.IsNullOrEmpty(GetClickPrice(dg.Item as DataRowView)) | string.IsNullOrEmpty(GetClickAmount(dg.Item as DataRowView)))
                            {
                                return;
                            }
                        }

                        string clickvalue = GetClickPrice(dg.Item as DataRowView);
                        string clickAmount = GetClickAmount(dg.Item as DataRowView);
                        clickAmount = clickAmount == "0" ? "" : clickAmount; 
                        isdefaultdrp = (string.IsNullOrEmpty(clickvalue) || string.IsNullOrEmpty(clickAmount));

                        DRPParamInfoModel dRPParam = new DRPParamInfoModel() {
                            OrderId = (dg.Item as DataRowView)[CTPriceControler.KEY_FIELD].ToString(),
                            PartyId = (dg.Item as DataRowView)[CTPriceControler.PARTY_ID_FIELD].ToString(),
                            PartyName = (dg.Item as DataRowView)[CTPriceControler.PARTY_NAME_FIELD].ToString()
                        };

                        SendPointPriceView sendPointPriceView = new SendPointPriceView(viewModel.SecurityID,
                        true, GetClickPrice(dg.Item as DataRowView), GetClickAmount(dg.Item as DataRowView),
                        string.Join(":", settleTypeList.ToArray()), GetNetPrice(dg.Item as DataRowView), GetSource(dg.Item as DataRowView),
                        GetWholeFlag(dg.Item as DataRowView), IsFirmFlag(dg.Item as DataRowView), GetPriceKey(dg.Item as DataRowView), GetTransactTime(dg.Item as DataRowView),
                        GetDeliveryType(dg.Item as DataRowView), GetClearingMethod(dg.Item as DataRowView), GetQuoteType(dg.Item as DataRowView), GetOrderTip(dg.Item as DataRowView), string.Empty, GetVenueTypeGw(dg.Item as DataRowView)
                        ,null,null, isdefaultdrp, GetPartySubName(dg.Item as DataRowView),null,null,null,null, dRPParam);

                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg.Item as DataRowView)), sendPointPriceView);
                    }
                }
            }
        }

        private PriceMatchParam GetPriceMatchParam(DataRowView drv, string side)
        {
            QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);

            PriceMatchParam priceMatchParam = new PriceMatchParam();

            priceMatchParam.Symbol = viewModel.SecurityID;
            priceMatchParam.Side = side;
            priceMatchParam.Price = GetClickPrice(drv);
            priceMatchParam.CustomFirmAmount = GetClickAmount(drv);
            priceMatchParam.NetPrice = GetNetPrice(drv);
            priceMatchParam.Terminal = GetSource(drv);
            priceMatchParam.WholeFlag = GetWholeFlag(drv);
            priceMatchParam.IsFirm = IsFirmFlag(drv);
            priceMatchParam.Source = GetSource(drv);
            priceMatchParam.Id = GetPriceKey(drv);
            priceMatchParam.TrsactTime = GetTransactTime(drv);
            priceMatchParam.Text = GetOrderTip(drv);
            priceMatchParam.DeliveryType = GetDeliveryType(drv);
            priceMatchParam.ClearingMethod = GetClearingMethod(drv);
            priceMatchParam.SettlType = GetAllSettle(drv[CTPriceControler.SETTL_TYPE_FIELD].ToString(), drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString());
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                priceMatchParam.OperationType = "3";
            else
                priceMatchParam.QuoteType = GetQuoteType(drv);

            return priceMatchParam;
        }

        private bool CheckMultipleSettle(string allSettleTypeSB)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(allSettleTypeSB))
            {

                string[] arr = allSettleTypeSB.Split(new char[] { ',' });

                result = arr != null && arr.Length > 1;
            }
            return result;
        }

        private string GetNetPrice(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                // 净价
                result = drv[CTPriceControler.NET_PRICE_FIELD].ToString();
            }
            return result;
        }

        private string GetVenue(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                // 渠道
                result = drv[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
            }
            return result;
        }

        private string GetSettle(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                // 结算方式
                result = drv[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                if (string.IsNullOrEmpty(result))
                    result = drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
            }
            return result;
        }

        private bool IsFirmFlag(DataRowView drv)
        {
            if (drv != null)
            {
                // firm标记
                string firmFlag = drv[CTPriceControler.FIRM_TYPE_FIELD].ToString();

                return firmFlag.Equals("1");
            }
            return false;
        }

        private string GetClickPrice(DataRowView drv)
        {
            string px = string.Empty;
            if (drv != null)
            {
                decimal px_decimal = drv[CTPriceControler.PriceType_Field].ToString() == "3" ? CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString()) : CommonUtils.ConvertToDecimal(drv[CTPriceControler.Yield_Field].ToString()); ;
                if (!string.IsNullOrEmpty(drv[CTPriceControler.Yield_Field].ToString()) && drv[CTPriceControler.Yield_Field].ToString()!="--")
                    px = px_decimal.ToString("F4");
                else
                    px = "";

                if (string.IsNullOrEmpty(px))
                {
                    px = GetPrice(drv) == "--" ? "" : GetPrice(drv);
                }
            }
            return px;
        }

        //获取点击价格- 国债期货保留3位小数
        private string GetClickPrice3(DataRowView drv)
        {
            string px = string.Empty;
            if (drv != null)
            {
                // 收益率
                decimal px_decimal = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                px = px_decimal.ToString("F3");
            }
            return px;
        }

        private string GetSource(DataRowView drv)
        {
            string source = string.Empty;
            if (drv != null)
            {
                source = drv[CTPriceControler.SOURCE_FIELD].ToString();
            }
            return source;
        }

        private string GetSide(DataRowView drv)
        {
            string source = string.Empty;
            if (drv != null)
            {
                source = drv[CTPriceControler.SIDE_FIELD].ToString();
            }
            return source;
        }

        private string GetPrice(DataRowView drv)
        {
            string source = string.Empty;
            if (drv != null)
            {
                source = drv[CTPriceControler.ENTRY_PX_FIELD].ToString();
            }
            return source;
        }

        private string GetClickAmount(DataRowView drv)
        {
            string px = string.Empty;
            if (drv != null)
            {
                // 交易量
                decimal px_decimal = CommonUtils.ConvertToDecimal(drv[CTPriceControler.QTY_FIELD].ToString());
                if (!string.IsNullOrEmpty(drv[CTPriceControler.QTY_FIELD].ToString()))
                {
                    px = px_decimal.ToString();
                }
                else
                {
                    px = "";
                }
                    
            }
            return px;
        }

        private string GetWholeFlag(DataRowView drv)
        {
            // 整量标志
            string firmFlag = CommonUtils.ConvertToString(drv[CTPriceControler.WHOLE_FLAG_FIELD]);
            return firmFlag;
        }

        private void Ofr_OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);

            //国债期货报价
            if (CommonUtils.GetFutureType(viewModel.SecurityID, viewModel.MarketIndicator) == 1)
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceFutureWinKey(WindowKeyHelper.OFR, viewModel.SecurityID)))
                {
                    SendPriceFutureViewLite sendPriceView = new SendPriceFutureViewLite(viewModel.SecurityID, false);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceFutureWinKey(WindowKeyHelper.OFR, viewModel.SecurityID), sendPriceView);
                }
                return;
            }

            //商品期货报价
            if (CommonUtils.GetFutureType(viewModel.SecurityID, viewModel.MarketIndicator) == 2)
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceCommodityWinKey(WindowKeyHelper.OFR, viewModel.SecurityID)))
                {
                    SendPriceCommodityViewLite sendPriceView = new SendPriceCommodityViewLite(viewModel.SecurityID, false);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceCommodityWinKey(WindowKeyHelper.OFR, viewModel.SecurityID), sendPriceView);
                }
                return;
            }

            if (viewModel.MarketIndicator == "2")
            {
                //IRS报价
                SendIRSView sendPriceView = new SendIRSView(viewModel.SecurityID, false, false, false);
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, viewModel.SecurityID), sendPriceView);
            }
            else if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.OFR, viewModel.SecurityID)))
            {
                //SendPriceViewLite sendPriceView = new SendPriceViewLite(viewModel.SecurityID, false);
                SendPriceView sendPriceView = new SendPriceView(viewModel.SecurityID, false);
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.OFR, viewModel.SecurityID), sendPriceView);
                //sendPriceView.Owner = Application.Current.MainWindow;
                //sendPriceView.Show();
            }
        }

        private bool IsMe(DataRowView drv)
        {
            bool result = false;
            if (drv != null)
            {
                result = (bool)(drv[CTPriceControler.ISME_FIELD]);
            }
            return result;
        }

        private bool IsPermission(DataRowView drv)
        {
            bool result = false;
            if (drv != null)
            {
                if (string.IsNullOrEmpty(drv[CTPriceControler.IS_PERMISSION_FIELD].ToString()))
                    result = true;
                else
                    result = (bool)(drv[CTPriceControler.IS_PERMISSION_FIELD]);
            }

            return result;
        }

        private string GetPriceKey(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                result = CommonUtils.GetOrderId(drv[CTPriceControler.KEY_FIELD].ToString());
            }
            return result;
        }

        private string GetTransactTime(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                result = drv[CTPriceControler.TRANSACT_TIME_FILED].ToString();
            }
            return result;
        }

        private string GetDeliveryType(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                result = drv[CTPriceControler.DELIVERY_TYPE_FILED].ToString();
            }
            return result;
        }

        private string GetClearingMethod(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                result = drv[CTPriceControler.CLEARING_METHOD_FILED].ToString();
            }
            return result;
        }


        private string GetQuoteType(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                result = drv[CTPriceControler.QUOTE_TYPE_FILED].ToString();
            }
            return result;
        }

        private string GetOrderTip(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                result = drv[CTPriceControler.TEXT_FIELD].ToString();
            }
            return result;
        }


        private string GetVenueTypeGw(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                result = drv[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
            }
            return result;
        }

        private string GetPartySubName(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                result = drv[CTPriceControler.CUSTOMER_SOURCE_FIELD].ToString();
            }
            return result;
        }
        /// <summary>
        /// 获取全部的结算方式，用 : 分开
        /// </summary>
        /// <param name="drv"></param>
        /// <returns></returns>
        private string GetAllSettle(string settletype, string settletypeyuan)
        {
            string result = string.Empty;

            // 结算方式
            if (!string.IsNullOrEmpty(settletype))
            {
                List<string> list = settletype.Split(',').ToList();
                if (!string.IsNullOrEmpty(settletypeyuan))
                {
                    list.Add(settletypeyuan);
                }

                result = string.Join(":", list.ToArray());
            }
            else
            {
                result = settletypeyuan;
            }

            return result;
        }

        private bool OpenBatchWin(object sender, MouseButtonEventArgs e)
        {
            ////暂时不开放
            //return false;

            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell))
                cell = VisualTreeHelper.GetParent(cell);

            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;


            if (targetCell != null && targetCell.Column != null && targetCell.Column.Header != null)
            {
                string head = targetCell.Column.Header.ToString();

                if (head == "累计")
                {
                    QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                    DataGridRow dg = sender as DataGridRow;
                    DataRowView dataRowView = dg.Item as DataRowView;
                    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetBatchPointWinKey(GetPriceKey(dataRowView))))
                    {
                        BatchPointPriceView batchPointPriceView = new BatchPointPriceView();
                        batchPointPriceView.Init(GetSide(dataRowView), viewModel.SecurityID, GetPrice(dataRowView));

                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetBatchPointWinKey(GetPriceKey(dataRowView)), batchPointPriceView);

                        return true;
                    }
                }
            }

            return false;
        }

        private void OfrDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dg = sender as DataGridRow;

            QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
            //如果渠道执行仅有内部权限
            bool isInner = AdminHandler.Instance.CheckPermission("1002", "100201");
            if (isInner && !IsInnerPermission(dg.Item as DataRowView))
                return;
            //如果渠道执行有内部权限+XBOND
            bool isInnerXbond = AdminHandler.Instance.CheckPermission("1002", "100202");
            if (isInnerXbond && !IsInnerXbondPermission(dg.Item as DataRowView)) 
                return;

            if (!IsPermission(dg.Item as DataRowView))
            {
                return;
            }

            if (!PermissionHandler.Instance.CheckVenuePermission(GetSource(dg.Item as DataRowView), viewModel.SecurityID))
                return;

            if (viewModel.ShowOrderBtn != Visibility.Visible)
                return;

            if (this.OpenBatchWin(sender, e))
                return;

            //是否允许发起点价单，允许为true,不允许为false
            if (!LocalEnv.ConfInfo.EnableIOC)
                return;

            //双击国债期货行情
            if (CommonUtils.GetFutureType(viewModel.SecurityID, viewModel.MarketIndicator) == 1)
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg.Item as DataRowView))))
                {
                    SendPriceFutureClickViewLite sendPriceFutureClickView = new SendPriceFutureClickViewLite(viewModel.SecurityID, true, GetClickPrice3(dg.Item as DataRowView), GetClickAmount(dg.Item as DataRowView));
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg.Item as DataRowView)), sendPriceFutureClickView);
                    return;
                }
            }

            //双击商品期货行情
            if (CommonUtils.GetFutureType(viewModel.SecurityID, viewModel.MarketIndicator) == 2)
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg.Item as DataRowView))))
                {
                    SendPriceCommodityClickViewLite sendPriceCommodityClickView = new SendPriceCommodityClickViewLite(viewModel.SecurityID, true, GetClickPrice3(dg.Item as DataRowView), GetClickAmount(dg.Item as DataRowView));
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg.Item as DataRowView)), sendPriceCommodityClickView);
                    return;
                }
            }

            //if (IsMe(dg.Item as DataRowView))
            //{
            //    QuickUnderline("2", dg.Item as DataRowView);

            //    return;
            //}



            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {

                //if (IsFirmFlag(dg.Item as DataRowView))
                {

                    //if (CheckParam(dg.Item as DataRowView))
                    //    return;

                    StringBuilder allSettleTypeSB = new StringBuilder();
                    List<string> settleTypeList = new List<string>();
                    GenAllSettlType(dg.Item as DataRowView, ref allSettleTypeSB, ref settleTypeList);
                    //if (CheckMultipleSettle(allSettleTypeSB.ToString()))
                    //{
                    //    string priceInfo = string.Format("{0} {1} {2} {3}", viewModel.SecurityID, "Ofr", allSettleTypeSB.ToString(), GetClickPrice(dg.Item as DataRowView));

                    //    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg.Item as DataRowView))))
                    //    {
                    //        PointPriceSettleView pointPriceSettleView = new PointPriceSettleView(priceInfo, allSettleTypeSB.ToString(), GetPriceMatchParam(dg.Item as DataRowView, "2"));
                    //        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg.Item as DataRowView)), pointPriceSettleView);
                    //        //priceMatchView.Show();
                    //    }




                    //    //PointPriceSettleView pointPriceSettleView = new PointPriceSettleView(priceInfo, allSettleTypeSB.ToString(), GetPriceMatchParam(dg.Item as DataRowView, "2"));
                    //    //pointPriceSettleView.Show();
                    //}
                    //else
                    {
                        bool isQuick = QuickOrderHandler.Instance.QuickOrder(dg.Item as DataRowView, false, null, false);


                        if (!isQuick && !WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg.Item as DataRowView))))
                        {
                            bool isdefaultdrp = false;
                            // 如果无价无量，并且不开启定向询价，则返回
                            if (LocalEnv.ConfInfo.IsShowDRP == "0")
                            {
                                if (string.IsNullOrEmpty(GetClickPrice(dg.Item as DataRowView)) | string.IsNullOrEmpty(GetClickAmount(dg.Item as DataRowView)))
                                {
                                    return;
                                }
                            }

                            string clickvalue = GetClickPrice(dg.Item as DataRowView);
                            string clickAmount = GetClickAmount(dg.Item as DataRowView);
                            clickAmount = clickAmount == "0" ? "" : clickAmount;
                            isdefaultdrp = (string.IsNullOrEmpty(clickvalue) || string.IsNullOrEmpty(clickAmount));

                            DRPParamInfoModel dRPParam = new DRPParamInfoModel()
                            {
                                OrderId = (dg.Item as DataRowView)[CTPriceControler.KEY_FIELD].ToString(),
                                PartyId = (dg.Item as DataRowView)[CTPriceControler.PARTY_ID_FIELD].ToString(),
                                PartyName = (dg.Item as DataRowView)[CTPriceControler.PARTY_NAME_FIELD].ToString()
                            };

                            SendPointPriceView sendPointPriceView = new SendPointPriceView(viewModel.SecurityID,
                            false, GetClickPrice(dg.Item as DataRowView), GetClickAmount(dg.Item as DataRowView),
                            string.Join(":", settleTypeList.ToArray()), GetNetPrice(dg.Item as DataRowView), GetSource(dg.Item as DataRowView),
                            GetWholeFlag(dg.Item as DataRowView), IsFirmFlag(dg.Item as DataRowView), GetPriceKey(dg.Item as DataRowView), GetTransactTime(dg.Item as DataRowView),
                            GetDeliveryType(dg.Item as DataRowView), GetClearingMethod(dg.Item as DataRowView), GetQuoteType(dg.Item as DataRowView), GetOrderTip(dg.Item as DataRowView), string.Empty, GetVenueTypeGw(dg.Item as DataRowView)
                            ,null,null, isdefaultdrp, GetPartySubName(dg.Item as DataRowView), null, null, null, null, dRPParam);

                            WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg.Item as DataRowView)), sendPointPriceView);
                            //priceMatchView.Show();
                        }



                        //SendPointPriceView sendPointPriceView = new SendPointPriceView(viewModel.SecurityID,
                        //    false, GetClickPrice(dg.Item as DataRowView), GetClickAmount(dg.Item as DataRowView),
                        //    GetSettle(dg.Item as DataRowView), GetNetPrice(dg.Item as DataRowView), GetSource(dg.Item as DataRowView),
                        //    GetWholeFlag(dg.Item as DataRowView), IsFirmFlag(dg.Item as DataRowView));
                        //sendPointPriceView.Owner = Application.Current.MainWindow;
                        //sendPointPriceView.Show();
                    }
                }
            }
            else
            {
                return;
            }
        }

        private bool CheckParam(DataRowView drv)
        {
            bool result = true;
            result = (bool)drv[CTPriceControler.ISME_FIELD];

            if (result)
            {
                Common.View.Component.CustomMessageBox.Info("不能点击本机构报价");
            }

            return result;
        }



        private void FilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.FilterPanel.FilterDic != null)
            {
                if (this.DataContext != null && this.DataContext is QuotationListViewModel)
                {
                    QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;

                    // 控制净价和收益率切换开关
                    //bool isShowSwitch = true;
                    //if (this.FilterPanel.FilterDic.ContainsKey("Venues"))
                    //{
                    //    if (this.FilterPanel.FilterDic["Venues"].Count == 1)
                    //    {
                    //        string content = ((ListBoxItem)this.FilterPanel.FilterDic["Venues"][0]).Content.ToString();
                    //        if (content == "X-Bond" || content == "双边")
                    //        {
                    //            isShowSwitch = true;
                    //        }
                    //    }
                    //}
                    //Switch_Yield_Btn_Click(null, null);

                    // 过滤
                    viewModel.FilterData(this.FilterPanel.FilterDic);
                }
            }
        }

        private void BidDataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            this.BidDataGrid_Frozen.SelectedIndex = -1;
            this.BidIncompleteDataGrid.SelectedIndex = -1;
        }

        private void BidDataGrid_Frozen_GotFocus(object sender, RoutedEventArgs e)
        {
            this.BidDataGrid.SelectedIndex = -1;
            this.BidIncompleteDataGrid.SelectedIndex = -1;
        }

        private void BidIncompleteDataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            this.BidDataGrid.SelectedIndex = -1;
            this.BidDataGrid_Frozen.SelectedIndex = -1;
        }

        private void OfrDataGrid_Frozen_GotFocus(object sender, RoutedEventArgs e)
        {
            this.OfrDataGrid.SelectedIndex = -1;
            this.OfrIncompleteDataGrid.SelectedIndex = -1;
        }

        private void OfrDataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            this.OfrDataGrid_Frozen.SelectedIndex = -1;
            this.OfrIncompleteDataGrid.SelectedIndex = -1;
        }

        private void OfrIncompleteDataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            this.OfrDataGrid.SelectedIndex = -1;
            this.OfrDataGrid_Frozen.SelectedIndex = -1;
        }

        #region 报价右键菜单
        ContextMenu contextMenu;
        QuotationCloneRowData cloneRowData;

        private void SetContextMenu()
        {
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("OrderContextMenu");
            }
        }

        private void OrderMenuItme_Click(object sender, RoutedEventArgs e)
        {
            if (this.contextMenu == null)
            {
                LogHandler.Info("QuotationList OrderMenuItme_Click 找不到右建菜单");
                return;
            }
            if (contextMenu.Tag != null && contextMenu.Tag is QuotationCloneRowData)
            {
                QuotationCloneRowData dgRow = contextMenu.Tag as QuotationCloneRowData;

                // todo 保价逻辑， 数据在drv中
                string securityID = dgRow.securityID;
                string side = dgRow.side;

                string price = dgRow.entryPx;
                // 报量为个人偏好值
                string qty = SystemParamHandler.Instance.GetUserSetting().userQty;

                PriceMatchParam priceMatchParam = new PriceMatchParam();
                priceMatchParam.Side = side == "1" ? "SELL" : "BUY";
                priceMatchParam.Price = price;
                priceMatchParam.UpdataPrice = false;
                priceMatchParam.CustomFirmAmount = qty;
                priceMatchParam.Symbol = securityID;
                priceMatchParam.NonfirmTime = string.Empty;
                priceMatchParam.OperationType = "1";
                //设置默认结算方式
                priceMatchParam.SettlType = dgRow.settle;
                priceMatchParam.NetPrice = dgRow.netPrice;
                //SendPriceViewLite view = new SendPriceViewLite(priceMatchParam);

                string marketindicator = (dgRow.Item as DataRowView)[CTPriceControler.MARKETINDICATOR_FIELD].ToString();
                if (marketindicator == "2")
                {
                    //IRS报价
                    SendIRSView sendPriceView = new SendIRSView(priceMatchParam);
                    sendPriceView.Show();
                }
                else
                {
                    SendPriceView view = new SendPriceView(priceMatchParam);
                    view.Show();
                }
            }
            else
            {
                LogHandler.Info("QuotationList OrderMenuItme_Click 没有取到数据");
            }
        }

        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                if(viewModel != null) 
                {
                    if(Consts.MARKETINDICATOR_FUTURE.Equals(viewModel.MarketIndicator))
                    {
                        return;
                    }
                }
                if (sender is DataGridRow)
                {
                    SetContextMenu();
                    DataGridRow dgr = sender as DataGridRow;

                    DataGrid dg = CommonUtils.GetParentObject<DataGrid>(dgr) as DataGrid;
                    dg.Focus();

                    dgr.Focus();
                    dgr.IsSelected = true;

                    LogHandler.Info("QuotationList DataGrid_MouseRightButtonUp 拷贝数据");
                    if (this.contextMenu == null)
                    {
                        LogHandler.Info("QuotationList DataGrid_MouseRightButtonUp 找不到右建菜单");
                        return;
                    }
                    DataGridRow dgRow = dgr as DataGridRow;
                    if (dgRow != null)
                    {
                        if (dgRow.Item != null && dgRow.Item is DataRowView)
                        {
                            // clone 一条数据
                            DataRowView drv = dgRow.Item as DataRowView;
                            cloneRowData = GetCloneData(drv);
                            this.contextMenu.Tag = cloneRowData;
                        }
                        else
                        {
                            LogHandler.Info("QuotationList DataGrid_MouseRightButtonUp 没有取到当前数据");
                        }
                    }
                    else
                    {
                        LogHandler.Info("QuotationList DataGrid_MouseRightButtonUp 没有取到行数据");
                    }

                    contextMenu.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }

        private QuotationCloneRowData GetCloneData(DataRowView drv)
        {
            QuotationCloneRowData cloneData = new QuotationCloneRowData();

            cloneData.securityID = CommonUtils.ConvertToString(drv[CTPriceControler.SECURITY_ID_FIELD]);
            cloneData.side = CommonUtils.ConvertToString(drv[CTPriceControler.SIDE_FIELD]);

            cloneData.entryPx = GetClickPrice(drv);
            if (string.IsNullOrEmpty(cloneData.entryPx))
            {
                cloneData.entryPx = GetPrice(drv) == "--" ? "" : GetPrice(drv);
            }

            cloneData.settle = GetSettle(drv);//"2";
            cloneData.netPrice = GetNetPrice(drv);
            cloneData.key = CommonUtils.GetOrderId(CommonUtils.ConvertToString(drv[CTPriceControler.KEY_FIELD]));
            cloneData.IsMe = (bool)drv[CTPriceControler.ISME_FIELD];
            cloneData.FirmFlag = drv[CTPriceControler.FIRM_TYPE_FIELD].ToString();
            cloneData.Item = drv;
            return cloneData;
        }

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(DataRow drv)
        {
            string strategyCodeID = CommonUtils.ConvertToString(drv[MyOrderProcesser.STCCODE_ORDER_FIELD]);
            string securityID = CommonUtils.ConvertToString(drv[MyOrderProcesser.SECURITYID_ORDER_FIELD]);
            string qty = CommonUtils.ConvertToString(drv[MyOrderProcesser.QTY_ORDER_FIELD]);
            string exeqty = CommonUtils.ConvertToString(drv[MyOrderProcesser.EXECQTY_ORDER_FIELD]);
            string price = CommonUtils.ConvertToString(drv[MyOrderProcesser.PRICE_ORDER_FIELD]);
            string netPrice = CommonUtils.ConvertToString(drv[MyOrderProcesser.NETPRICE_ORDER_FIELD]);
            string settlType = CommonUtils.ConvertToString(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]);
            string side = CommonUtils.ConvertToString(drv[MyOrderProcesser.SIDE_ORDER_FIELD]);
            string wholeFlag = CommonUtils.ConvertToString(drv[MyOrderProcesser.WHOLE_FLAG_FIELD]);
            string firmFlag = CommonUtils.ConvertToString(drv[MyOrderProcesser.FIRMFLAG_ORDER_FIELD]);
            string crossVenues = CommonUtils.ConvertToString(drv[MyOrderProcesser.CROSS_VENUES_FIELD]);
            string firmToNonTime = CommonUtils.ConvertToString(drv[MyOrderProcesser.NONFIRM_TIME_FIELD]);
            string randomNum = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENUES_RDM_NUM_FIELD]);
            string venuesSelectF = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENUES_SELECT_F_FIELD]);
            string selfSelectVenues = CommonUtils.ConvertToString(drv[MyOrderProcesser.SELF_SELECT_VENUES_FIELD]);


            string venues = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENEUES_ORDER_FIELD]);
            string venueTypeGw = CommonUtils.ConvertToString(drv[MyOrderProcesser.VENUETYPEGW_FIELD]);
            string algoName = CommonUtils.ConvertToString(drv[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
            string quantID = CommonUtils.ConvertToString(drv[MyOrderProcesser.QUANTID_FIELD]);
            string timeInForce = CommonUtils.ConvertToString(drv[MyOrderProcesser.TIMEINFORCE_FIELD]);
            string ordStatus = CommonUtils.ConvertToString(drv[MyOrderProcesser.ORDERSTATUS_ORDER_FIELD]);
            string bizType = CommonUtils.ConvertToString(drv[MyOrderProcesser.BIZTYPE_FIELD]);
            string bizTypeID = CommonUtils.ConvertToString(drv[MyOrderProcesser.BIZTYPEID_FIELD]);
            string quoteID = CommonUtils.ConvertToString(drv[MyOrderProcesser.QUOTEID_FIELD]);
            string roleID = CommonUtils.ConvertToString(drv[MyOrderProcesser.ROLEID_FIELD]);
            string roleName = CommonUtils.ConvertToString(drv[MyOrderProcesser.ROLENAME_FIELD]);
            string symbol = CommonUtils.ConvertToString(drv[MyOrderProcesser.SYMBOL_ORDER_FIELD]);
            string userId = CommonUtils.ConvertToString(drv[MyOrderProcesser.USERID_FIELD]);
            string institutionID = CommonUtils.ConvertToString(drv[MyOrderProcesser.INSTITUTIONID_FIELD]);
            string institutionName = CommonUtils.ConvertToString(drv[MyOrderProcesser.INSTITUTIONNAME_FIELD]);
            string traderID = CommonUtils.ConvertToString(drv[MyOrderProcesser.TRADERID_FIELD]);
            string traderName = CommonUtils.ConvertToString(drv[MyOrderProcesser.TRADERNAME_FIELD]);
            string showTrader = CommonUtils.ConvertToString(drv[MyOrderProcesser.SHOW_TRADER_FIELD]);
            string ternimal = CommonUtils.ConvertToString(drv[MyOrderProcesser.TERMINAL_FIELD]);
            string info1 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO1_FIELD]);
            string info2 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO2_FIELD]);
            string info3 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO3_FIELD]);
            string intentionID = CommonUtils.ConvertToString(drv[MyOrderProcesser.INTENTIONID_FIELD]);
            string accountid = CommonUtils.ConvertToString(drv[MyOrderProcesser.AccountID_Field]);
            string accountName = CommonUtils.ConvertToString(drv[MyOrderProcesser.AccountName_Field]);

            string priceCondition = CommonUtils.ConvertToString(drv[MyOrderProcesser.PriceCondition_Field]);

            Common.Model.Fix.Message.NewOrderSingle sendPricePo = new Common.Model.Fix.Message.NewOrderSingle();

            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            sendPricePo.SecurityType = bondsInfoPo.SECURITY_TYPE;

            sendPricePo.BizType = bizType;
            sendPricePo.BizTypeID = bizTypeID;
            sendPricePo.QuoteID = quoteID;
            sendPricePo.RoleID = roleID;
            sendPricePo.RoleName = roleName;
            sendPricePo.Symbol = symbol;
            sendPricePo.UserID = userId;
            sendPricePo.OrdStatus = ordStatus;
            sendPricePo.StrategyOrderId = strategyCodeID;

            sendPricePo.AlgoName = algoName;
            sendPricePo.VenueTypeGW = venueTypeGw;
            sendPricePo.Venues = venues;
            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            sendPricePo.MarketIndicator = "4";
            sendPricePo.OrdType = "2";
            sendPricePo.OrderQty = qty;//(CommonUtils.ConvertToDecimal(qty) - CommonUtils.ConvertToDecimal(exeqty)).ToString();
            sendPricePo.Price = price;
            sendPricePo.NetPrice = netPrice;
            sendPricePo.SecurityID = securityID;
            sendPricePo.PriceType = "3"; //收益率
            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            sendPricePo.SettlType = settlType;

            sendPricePo.Side = side;

            sendPricePo.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = timeInForce;
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;

            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = firmFlag;

            sendPricePo.CrossVenues = crossVenues;

            sendPricePo.WholeFlag = wholeFlag;

            sendPricePo.QuantID = quantID;

            sendPricePo.FirmToNonTime = firmToNonTime;

            sendPricePo.VenuesRdmNum = randomNum;

            sendPricePo.VenuesSelectF = venuesSelectF;

            sendPricePo.SelfSelectVenues = selfSelectVenues;

            sendPricePo.InstitutionID = institutionID; //客盘机构id
            sendPricePo.InstitutionName = institutionName;//客盘机构名字
            sendPricePo.TraderID = traderID; // 客盘交易员id
            sendPricePo.TraderName = traderName;//客盘交易员名字
            sendPricePo.ShowTrader = showTrader;// 是否显示经济商

            //sendPricePo.ValidUntilTime = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");
            sendPricePo.Terminal = ternimal;
            sendPricePo.Info1 = info1;
            sendPricePo.Info3 = info3;
            sendPricePo.Info2 = info2;
            sendPricePo.Info4 = intentionID;
            sendPricePo.AccountID = accountid;
            sendPricePo.AccountName = accountName;

            sendPricePo.PriceCondition = priceCondition;

            sendPricePo.IntentionID = intentionID;

            return sendPricePo;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.contextMenu == null)
            {
                LogHandler.Info("QuotationList MenuItem_Click 找不到右建菜单");
                return;
            }
            if (contextMenu.Tag != null && contextMenu.Tag is QuotationCloneRowData)
            {
                QuotationCloneRowData dgRow = contextMenu.Tag as QuotationCloneRowData;

                // todo 保价逻辑， 数据在drv中

                string orderId = dgRow.key;
                DataRow drv = MyOrderProcesser.Processer.GetOrderById(orderId);
                if (drv == null)
                {
                    LogHandler.Info("QuotationList MenuItem_Click 找不到订单");
                    return;
                }    

                Common.Model.Fix.Message.NewOrderSingle singleOrder = GetNewOrderSingle(drv);
                //修改量
                singleOrder.OrderQty = GetClickAmount(dgRow.Item);

                PriceMatchParam priceMatchParam = new PriceMatchParam();

                priceMatchParam.CustomFirmAmount = singleOrder.OrderQty; // OrderQty 订单量
                priceMatchParam.OrderId = singleOrder.StrategyOrderId;// 订单号
                priceMatchParam.RandomNum = singleOrder.VenuesRdmNum; // 渠道类型默认，填写随机几家渠道
                priceMatchParam.Side = singleOrder.Side; // 方向
                priceMatchParam.Symbol = singleOrder.SecurityID; // 代码
                priceMatchParam.SymbolName = singleOrder.Symbol; // 名称
                priceMatchParam.WholeFlag = singleOrder.WholeFlag; // 整量标志
                priceMatchParam.NetPrice = singleOrder.NetPrice; // 净价
                priceMatchParam.Price = singleOrder.Price; // 收益率
                priceMatchParam.SettlType = singleOrder.SettlType; // 清算速度
                priceMatchParam.NonfirmTime = singleOrder.FirmToNonTime; // nonfirmTime;
                priceMatchParam.CrossVenues = singleOrder.CrossVenues;// 撮合渠道
                priceMatchParam.VenuesSelectF = singleOrder.VenuesSelectF;//渠道类型  不填写1,默认2，指定3 
                priceMatchParam.SelfSelectVenues = singleOrder.SelfSelectVenues;//指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC

                priceMatchParam.InstitutionID = singleOrder.InstitutionID; //客盘机构id
                priceMatchParam.InstitutionName = singleOrder.InstitutionName;//客盘机构名字
                priceMatchParam.TraderID = singleOrder.TraderID; // 客盘交易员id
                priceMatchParam.TraderName = singleOrder.TraderName;//客盘交易员名字
                priceMatchParam.Terminal = singleOrder.Terminal;
                priceMatchParam.Info1 = singleOrder.Info1;
                priceMatchParam.Info3 = singleOrder.Info3;//用于IDB单子
                priceMatchParam.Info2 = singleOrder.Info2;
                priceMatchParam.Info4 = singleOrder.Info4;//用于意向单Intentionid
                priceMatchParam.AccountID = singleOrder.AccountID;


                priceMatchParam.SendOrderType = singleOrder.AlgoName.Equals("CROSS") ? "OCO" : "直通";

                priceMatchParam.PriceCondition = singleOrder.PriceCondition;

                if (singleOrder.FirmFlag == "1")
                {
                    priceMatchParam.IsFirm = true;// firm
                }
                else
                {
                    priceMatchParam.IsFirm = false;// nonfirm
                }

                priceMatchParam.OldOrder = singleOrder;



                if (singleOrder.VenueTypeGW == "INNER" && !string.IsNullOrEmpty(singleOrder.IntentionID))
                {
                    priceMatchParam.IntentionID = singleOrder.IntentionID;
                    SendRequireView sendRequireView = new SendRequireView(priceMatchParam);
                    WindowManager.Instance.OpenOnlyOneWindow($"EditOrder-{priceMatchParam.IntentionID}", sendRequireView);
                }
                else
                {
                    string marketindicator = drv[CTPriceControler.MARKETINDICATOR_FIELD].ToString();
                    if (marketindicator == "2")
                    {
                        //IRS报价
                        SendIRSView sendPriceView = new SendIRSView(priceMatchParam);
                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetPointWinKey(singleOrder.StrategyOrderId), sendPriceView);
                    }
                    else
                    {
                        SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
                        // bid;
                        if (singleOrder.Side == "1")
                        {
                            sendPriceView.SetButtonEnable("OfrBtn", false);
                        }
                        // ofr
                        else if (singleOrder.Side == "2")
                        {
                            sendPriceView.SetButtonEnable("BidBtn", false);
                        }

                        sendPriceView.SetButtonEnable("Search_TextBox", false);


                        if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPointWinKey(singleOrder.StrategyOrderId)))
                        {
                            WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetPointWinKey(singleOrder.StrategyOrderId), sendPriceView);
                        }
                    }
                }
            }
            else
            {
                LogHandler.Info("QuotationList MenuItem_Click 没有取到数据");
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void MiCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.contextMenu == null)
            {
                LogHandler.Info("QuotationList MiCancel_Click 找不到右建菜单");
                return;
            }
            DeleteOrder();
        }

        private void DeleteOrder()
        {
            if (contextMenu.Tag != null && contextMenu.Tag is QuotationCloneRowData)
            {
                QuotationCloneRowData dgRow = contextMenu.Tag as QuotationCloneRowData;

                // todo 保价逻辑， 数据在drv中
                DealDelete(dgRow.key, true);
            }
            else
            {
                LogHandler.Info("QuotationList DeleteOrder 没有取到数据");
            }
        }

        private async void DealDelete(string orderId, bool isShowWarn)
        {
            LogHandler.Info($"DealDelete {orderId}");
            bool result = true;

            if (isShowWarn)
                result = Common.View.Component.CustomMessageBox.Question("是否撤销该笔报价？", "删除订单");

            if (result == true)
            {
                if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                {
                    LogHandler.Info("点击撤单按钮");
                }
                OrderCancelRequest cancelPo = new OrderCancelRequest();

                DataRow dr = MyOrderProcesser.Processer.GetOrderById(orderId);
                if (dr != null)
                {
                    cancelPo.AlgoName = CommonUtils.ConvertToString(dr[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                }

                cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                cancelPo.OrderID = orderId;
                TBookCancelReplyPo reply = await CancelOrder(cancelPo);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        //this.SelectedItem_MyOrders[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                    }
                    else
                    {
                        Common.View.Component.CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
                    }
                }
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

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (this.contextMenu == null)
            {
                LogHandler.Info("QuotationList UserControl_ContextMenuOpening 找不到右建菜单");
                return;
            }

            if (contextMenu.Tag != null && contextMenu.Tag is QuotationCloneRowData)
            {
                QuotationCloneRowData dgRow = contextMenu.Tag as QuotationCloneRowData;

                // todo 保价逻辑， 数据在drv中

                bool isMe = dgRow.IsMe;

                if (this.DataContext != null && this.DataContext is QuotationListViewModel)
                {
                    QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
                    if (viewModel.ShowOrderBtn != Visibility.Visible)
                    {
                        foreach (MenuItem menuItem in contextMenu.Items)
                        {
                            menuItem.Visibility = Visibility.Collapsed;
                        }

                        return;
                    }
                }
                
                foreach (MenuItem menuItem in contextMenu.Items)
                {
                    if (menuItem.Name.Equals("miEdit") || menuItem.Name.Equals("miCancel"))
                    {
                        menuItem.Visibility = isMe ? Visibility.Visible : Visibility.Collapsed;
                    }

                    if (menuItem.Name.Equals("miSendDrp"))
                    {
                        if (isMe)
                        {
                            menuItem.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            //判断是否是Nonfirm
                            if (contextMenu.Tag != null && contextMenu.Tag is QuotationCloneRowData)
                            {
                                if (LocalEnv.ConfInfo.IsShowDRP == "0")
                                {
                                    menuItem.Visibility = Visibility.Collapsed;
                                    continue;
                                }

                                QuotationCloneRowData dr = contextMenu.Tag as QuotationCloneRowData;
                                if (!IsPermission(dr.Item as DataRowView))
                                {
                                    menuItem.Visibility = Visibility.Collapsed;
                                    continue;
                                }

                                //if ((dr.Item as DataRowView)["MarketIndicator"].ToString().ToUpper() == "2")
                                //{
                                //    menuItem.Visibility = Visibility.Collapsed;
                                //    continue;
                                //}

                                if ((dr.Item as DataRowView)["VenueTypeGW"].ToString().ToUpper() != "INNER")
                                {
                                    menuItem.Visibility = Visibility.Collapsed;
                                    continue;
                                }

                                //检查渠道权限和内部权限
                                if (!Common.ViewModel.Drp.Utils.DRPTools.CheckInnerPermission(dr.securityID))
                                {
                                    menuItem.Visibility = Visibility.Collapsed;
                                    continue;
                                }
                                if (LocalEnv.ConfInfo.EnableIOC)
                                {
                                    menuItem.Visibility = dr.FirmFlag != "1" ? Visibility.Visible : Visibility.Collapsed;
                                }
                                else 
                                {
                                    menuItem.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                LogHandler.Info("QuotationList UserControl_ContextMenuOpening 没有取到数据");
            }
        }

        #endregion

        private void PopupBtn1_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
                if (viewModel != null)
                {
                    TBookViewHandler.Instance.OpenQuotesMiniWindows(viewModel.SecurityID);
                }
            }
        }

        /// <summary>
        /// 根据类型设置标题
        /// </summary>
        /// <param name="txt"></param>
        public void SetQtyHeaderText(string _marketIndicator)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                BidHeaderText.Text = _marketIndicator == "5" ? "量(手)" : "量(万)";
                OfrHeaderText.Text = BidHeaderText.Text;

                BidPriceHeaderTextFrozen.Text = _marketIndicator == "5" ? "元" : "价";
                OfrPriceHeaderTextFrozen.Text = BidPriceHeaderTextFrozen.Text;
            }));
        }

        /// <summary>
        /// 定向报价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miSendDrp_Click(object sender, RoutedEventArgs e)
        {
            if (this.contextMenu == null)
            {
                LogHandler.Info("QuotationList OrderMenuItme_Click 找不到右建菜单");
                return;
            }

            if (contextMenu.Tag != null && contextMenu.Tag is QuotationCloneRowData)
            {
                QuotationCloneRowData dg = contextMenu.Tag as QuotationCloneRowData;

                string id = $"EditDrpOrder-{dg.key}";

                if (!WindowManager.Instance.IsExist(id))
                {
                    // 如果无价无量，并且不开启定向询价，则返回
                    if (LocalEnv.ConfInfo.IsShowDRP == "0")
                    {
                        if (string.IsNullOrEmpty(GetClickPrice(dg.Item as DataRowView)) | string.IsNullOrEmpty(GetClickAmount(dg.Item as DataRowView)))
                        {
                            return;
                        }
                    }

                    // 回价反向 0 bid 1 ofr
                    bool side = (dg.side == "0") ? true : false;

                    string clickAmount = GetClickAmount(dg.Item as DataRowView);
                    clickAmount = clickAmount == "0" ? "" : clickAmount;

                    DRPParamInfoModel dRPParam = new DRPParamInfoModel() { OrderId  = (dg.Item as DataRowView)[CTPriceControler.KEY_FIELD].ToString() };

                    SendPointPriceView sendPointPriceView = new SendPointPriceView(dg.securityID,
                    side, GetClickPrice(dg.Item as DataRowView), clickAmount,
                    dg.settle, GetNetPrice(dg.Item as DataRowView), GetSource(dg.Item as DataRowView),
                    GetWholeFlag(dg.Item as DataRowView), IsFirmFlag(dg.Item as DataRowView), GetPriceKey(dg.Item as DataRowView), GetTransactTime(dg.Item as DataRowView),
                    GetDeliveryType(dg.Item as DataRowView), GetClearingMethod(dg.Item as DataRowView), GetQuoteType(dg.Item as DataRowView), GetOrderTip(dg.Item as DataRowView), string.Empty, GetVenueTypeGw(dg.Item as DataRowView)
                    , null,null,true, GetPartySubName(dg.Item as DataRowView), null, null, null, null, dRPParam);

                    WindowManager.Instance.OpenOnlyOneWindow(id, sendPointPriceView);
                }
            }
        }
        private bool IsInnerPermission(DataRowView drv)
        {
            bool result = false;
            if (drv != null)
            {
                if (string.IsNullOrEmpty(drv[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString()))
                    result = false;
                else
                    result = "INNER".Equals(drv[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString());
            }

            return result;
        }
        private bool IsInnerXbondPermission(DataRowView drv)
        {
            bool result = false;
            if (drv != null)
            {
                if (string.IsNullOrEmpty(drv[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString()))
                    result = false;
                else
                    result = "INNER".Equals(drv[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString()) || "ODM".Equals(drv[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString());
            }

            return result;
        }
    }
}
// 自定义事件参数  
public class PriceTypeSelectedEventArgs : EventArgs
{
    public int PriceTypeIndex { get; }

    public PriceTypeSelectedEventArgs(int priceTypeIndex)
    {
        PriceTypeIndex = priceTypeIndex;
    }
}
