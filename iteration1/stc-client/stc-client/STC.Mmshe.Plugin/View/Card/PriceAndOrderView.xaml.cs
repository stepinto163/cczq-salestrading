using netapi.Api;
using STC.Common.DataProcesser;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.ViewModel.Data;
using STC.Frame.CustomUI.Component;
using STC.Frame.UI;
using STC.Frame.Utils;
using STC.Mm.Plugin.CustomeUI;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.Utils.DataProcesser;
using STC.Mm.Plugin.View.Component;
using STC.Mm.Plugin.View.Order;
using STC.Mm.Plugin.ViewModel;
using STC.Mm.Plugin.ViewModel.Order;
using STC.Mm.Plugin.ViewModel.TradeBookHandler;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OrderCancelRequest = STC.Common.Model.TBook.OrderCancelRequest;

namespace STC.Mm.Plugin.View.Card
{
    /// <summary>
    /// PriceAndOrderView.xaml 的交互逻辑
    /// </summary>
    public partial class PriceAndOrderView : UserControl
    {
        NoBorderWin myTradingPopupWin;

        public static readonly DependencyProperty TitleBarVisibilityPropertyProperty = DependencyProperty.Register("TitleBarVisible", typeof(Visibility), typeof(PriceAndOrderView),
        new PropertyMetadata(Visibility.Visible, onTitleBarVisibility));


        public static readonly DependencyProperty RowHeightPropertyProperty = DependencyProperty.Register("RowHeight", typeof(string), typeof(PriceAndOrderView),
        new PropertyMetadata("", onRowHeight));

        private bool isRemoveMouseLeftButtonDown = false;

        // 标题栏是否可见
        public Visibility TitleBarVisible
        {
            get { return (Visibility)GetValue(TitleBarVisibilityPropertyProperty); }
            set { SetValue(TitleBarVisibilityPropertyProperty, value); }
        }

        // 标题栏是否可见
        public string RowHeight
        {
            get { return (string)GetValue(RowHeightPropertyProperty); }
            set { SetValue(RowHeightPropertyProperty, value); }
        }

        private static void onRowHeight(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.NewValue.ToString()))
                return;
            
            PriceAndOrderView source = (PriceAndOrderView)sender;
            int h = 181;
            if (int.TryParse(args.NewValue.ToString(), out h))
            {
                source.Row3.MaxHeight = source.Row3.MinHeight = h;
            }
        }

        private static void onTitleBarVisibility(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            PriceAndOrderView source = (PriceAndOrderView)sender;
            source.TitleBar.Visibility = (Visibility)args.NewValue;
            source.Row3.MaxHeight = source.Row3.MinHeight = 170;
            //source.LastExecutions.Visibility = (Visibility)args.NewValue;
            //source.Row3.MinHeight = 500;
            //source.Row3.MaxHeight = 1000;
            source.seBond.MouseLeftButtonDown -= source.TextBlock_MouseLeftButtonDown;
        }

        private static void onFilterString(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;

        }

        public PriceAndOrderView()
        {
            InitializeComponent();
        }

        public PriceAndOrderView(NoBorderWin mw)
        {
            InitializeComponent();
            this.myTradingPopupWin = mw;
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                this.LastExecutions.SecurtityID = string.IsNullOrEmpty(viewModel.SecurityID) ? "xxxxxx" : viewModel.SecurityID;
                this.Loaded += PriceAndOrderView_Loaded;
                viewModel.OnYunEvent += ViewModel_OnYunEvent;
            }
        }

        private void ViewModel_OnYunEvent(ExecutionReport obj)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                if (obj.SecurityID == viewModel.SecurityID)
                {
                    FilterPanel.SetEllipseVisibility(Visibility.Visible);
                }
            }
        }

        ////private void PriceAndOrderView_Unloaded(object sender, RoutedEventArgs e)
        ////{
        ////    this.Unloaded -= PriceAndOrderView_Unloaded;

        ////    PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
        ////    if (priceAndOrderViewModel != null)
        ////    {
        ////        //priceAndOrderViewModel.Dispose();
        ////    }

        ////}

        private void PriceAndOrderView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PriceAndOrderView_Loaded;
            QuotationListViewModel priceAndOrderViewModel = this.DataContext as QuotationListViewModel;

            if (priceAndOrderViewModel != null  && !string.IsNullOrEmpty(priceAndOrderViewModel.SecurityID))
            {
                ////        //priceAndOrderViewModel.Init(this, priceAndOrderViewModel.PriceModel.Security, priceAndOrderViewModel.Id, priceAndOrderViewModel.GroupId);

                ////        string cacheSettle = SettleCache.Instance.GetSettleById(priceAndOrderViewModel.GroupId, priceAndOrderViewModel.Id);
                ////        if (!string.IsNullOrEmpty(cacheSettle))
                ////        {
                ////            List<string> list = new List<string>();
                ////            list.Add(cacheSettle);
                ////            this.SetSettle(OperationType.RemerberToCard, ConditionHelper.SettleListToDic(list));
                ////        }

                ////        MarketDataSnapshotFullRefresh refreshSignal = new MarketDataSnapshotFullRefresh();
                ////        refreshSignal.SecurityID = priceAndOrderViewModel.PriceModel.Security;

                ////        priceAndOrderViewModel.OnPrice(refreshSignal);


                MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(priceAndOrderViewModel.SecurityID);
                if (lastExeItem != null)
                {
                    ExecutionReport temOrder = new ExecutionReport();
                    temOrder.SecurityID = priceAndOrderViewModel.SecurityID;
                    temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : lastExeItem.Side;
                    temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
                    temOrder.Venues = lastExeItem.SourceTrader;
                    temOrder.TransactTime = lastExeItem.Time;
                    priceAndOrderViewModel.OnLastExec(temOrder);
                }


                ////        marketView.Init(ViewModel.Data.TabShowModel.Card, priceAndOrderViewModel.PriceModel.Security);
            }

            btnFixFloatButton.IsChecked = myTradingPopupWin != null && myTradingPopupWin.Topmost ? true : false;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                seBond.SearchText = priceAndOrderViewModel.SecurityID;
                bool ispwd = priceAndOrderViewModel.CheckPwd(priceAndOrderViewModel.SecurityID);
                if (ispwd)
                {
                    FilterPanel.SetEllipseVisibility(Visibility.Visible);
                }
            }));
        }


        ContextMenu contextMenu;
        QuotationCloneRowData cloneRowData;
        //public bool GetIdcTraderPermission()
        //{
        //if (!string.IsNullOrEmpty(LocalEnv.UserInfo.IdcTradePermissions) && LocalEnv.UserInfo.IdcTradePermissions == "1")
        //    return true;
        //else
        //    return false;
        //}

        private void SetContextMenu()
        {
            //bool result = GetIdcTraderPermission();
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("OrderContextMenu");
            }
        }

        private bool _isBid = false;
        private void BdBid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetContextMenu();

            _isBid = true;
            if (contextMenu != null)
            {
                //如不允许直接报价，则取消注释
                //QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
                //if ((viewModel.PriceModel.BestBid == "--") || string.IsNullOrEmpty(viewModel.PriceModel.BestBid))
                //{
                //    return;
                //}

                contextMenu.Tag = "Best";
                contextMenu.IsOpen = true;
            }
        }

        private void BdOfr_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetContextMenu();

            _isBid = false;
            if (contextMenu != null)
            {
                //如不允许直接报价，则取消注释
                //QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
                //if ((viewModel.PriceModel.BestOfr == "--") || string.IsNullOrEmpty(viewModel.PriceModel.BestOfr))
                //{
                //    return;
                //}

                contextMenu.Tag = "Best";
                contextMenu.IsOpen = true;
            }
        }

        private void SeSearch_SearchChanged(object sender, object e)
        {
            ////SeBondPo seBondPo = e as SeBondPo;

            ////PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            ////if (priceAndOrderViewModel != null && seBondPo != null && !seBondPo.BondCode.Equals(priceAndOrderViewModel.PriceModel.Security))
            ////{
            ////    HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
            ////    request.securityid = seBondPo.BondCode;
            ////    request.symbol = seBondPo.ShortName;
            ////    request.action = "1"; // 添加
            ////    request.remark = $"{request.securityid}  {request.symbol}  {seBondPo.LeftTenor}";
            ////    request.terminal = string.Format("STCLite_v{0}", LocalEnv.ServerInfo.Version);
            ////    request.remainingTime = seBondPo.LeftTenor;

            ////    request.groupid = priceAndOrderViewModel.GroupId;

            ////    HomePageDataHandler.Instance.AddBond(request);
            ////}

            ////priceAndOrderViewModel.PriceModel.AddBondVisibility = Visibility.Visible;
            ////priceAndOrderViewModel.PriceModel.BondGridVisibility = Visibility.Collapsed;
            ////priceAndOrderViewModel.PriceModel.SearchVisibility = Visibility.Collapsed;

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ////PriceAndOrderViewModel priceAndOrderViewModel = this.DataContext as PriceAndOrderViewModel;
            ////priceAndOrderViewModel.OnAddClick();

            ////seSearch.SetFocus();
        }

        #region 报价
        /// <summary>
        /// 报价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OrderMenuItme_Click(object sender, RoutedEventArgs e)
        {
            if (this.contextMenu == null)
            {
                LogHandler.Info("QuotationList OrderMenuItme_Click 找不到右建菜单");
                return;
            }
            if (contextMenu.Tag != null)
            {
                if (contextMenu.Tag.GetType() == typeof(string))
                {
                    QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
                    if (_isBid)
                    {
                        PriceMatchParam priceMatchParam = new PriceMatchParam();
                        priceMatchParam.Side = !_isBid  ? "SELL" : "BUY";
                        priceMatchParam.Price = viewModel.PriceModel.BestBid == "--" ? "" : viewModel.PriceModel.BestBid;
                        priceMatchParam.UpdataPrice = false;
                        // priceMatchParam.CustomFirmAmount = viewModel.PriceModel.BestBidAmountText;
                        priceMatchParam.IsFirm = true;
                        priceMatchParam.Symbol = viewModel.SecurityID;
                        priceMatchParam.NonfirmTime = string.Empty;
                        priceMatchParam.OperationType = "1";
                        //设置默认结算方式
                        priceMatchParam.SettlType = "2,3";
                        //SendPriceViewLite view = new SendPriceViewLite(priceMatchParam);
                        //view.Show();

                        if (viewModel != null)
                        {
                            if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, viewModel.SecurityID)))
                            {
                                SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);

                                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, viewModel.SecurityID), sendPriceView);
                                //sendPriceView.Owner = Application.Current.MainWindow;
                                //sendPriceView.Show();
                            }
                        }
                    }
                    else
                    {
                        PriceMatchParam priceMatchParam = new PriceMatchParam();
                        priceMatchParam.Side = !_isBid ? "SELL" : "BUY";
                        priceMatchParam.Price = viewModel.PriceModel.BestOfr == "--" ? "" : viewModel.PriceModel.BestOfr;
                        priceMatchParam.UpdataPrice = false;
                        // priceMatchParam.CustomFirmAmount = viewModel.PriceModel.BestOfrAmountText;
                        priceMatchParam.IsFirm = true;
                        priceMatchParam.Symbol = viewModel.SecurityID;
                        priceMatchParam.NonfirmTime = string.Empty;
                        priceMatchParam.OperationType = "1";
                        //设置默认结算方式
                        priceMatchParam.SettlType = "2,3";

                        if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.OFR, viewModel.SecurityID)))
                        {
                            SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);
                            WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.OFR, viewModel.SecurityID), sendPriceView);
                            //sendPriceView.Owner = Application.Current.MainWindow;
                            //sendPriceView.Show();
                        }
                    }
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
                    priceMatchParam.IsFirm = true;
                    priceMatchParam.Symbol = securityID;
                    priceMatchParam.NonfirmTime = string.Empty;
                    priceMatchParam.OperationType = "1";
                    //设置默认结算方式
                    priceMatchParam.SettlType = dgRow.settle;
                    priceMatchParam.NetPrice = dgRow.netPrice;
                    SendPriceViewLite view = new SendPriceViewLite(priceMatchParam);

                    view.Show();
                }
                else
                {
                    LogHandler.Info("QuotationList OrderMenuItme_Click 没有取到数据");
                }

            }
         }
        #endregion

        #region 过滤
        public void FilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.FilterPanel.FilterDic != null)
            {
                if (this.DataContext != null && this.DataContext is QuotationListViewModel)
                {
                    QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                    // 过滤
                    viewModel.FilterData(this.FilterPanel.FilterDic);
                }
            }
        }

        #endregion

        #region Bid 下单
        private void BidDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ////if (TitleBarVisible == Visibility.Collapsed)
            ////{
            ////    return;
            ////}

            DataGridRow dg = sender as DataGridRow;

            //if (IsMe(dg))
            //{
            //    QuickUnderline("1", dg);

            //    return;
            //}


            QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
            if (!PermissionHandler.Instance.CheckVenuePermission(GetSource(dg.Item as DataRowView), viewModel.SecurityID))
                return;

            SendBidOrder(viewModel, dg.Item as DataRowView);

        }

        /// <summary>
        /// 最优价 Bid 下单
        /// </summary>
        public void BestBidSenderOrder()
        {

            QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);

            DataRowView dg = viewModel.PriceModel.BestBidDataRowView;
            if (!PermissionHandler.Instance.CheckVenuePermission(GetSource(dg), viewModel.SecurityID))
                return;

            SendBidOrder(viewModel, dg, Convert.ToDouble(viewModel.PriceModel.BestBidAmount.Replace("K","")));

        }

        public void SendBidOrder(QuotationListViewModel viewModel, DataRowView dg ,double clickAmount = 0)
        {
            //if (IsFirmFlag(dg))
            {
                //if (CheckParam(dg))
                //    return;

                //StringBuilder allSettleTypeSB = new StringBuilder();
                //StringBuilder settleTypeSB = new StringBuilder();
                // GenAllSettlType(dg, ref allSettleTypeSB, ref settleTypeSB);

                string settleTypeSB = GetAllSettle(dg[CTPriceControler.SETTL_TYPE_FIELD].ToString(), dg[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString());
                string allSettleTypeSB = GetAllChsSettle(settleTypeSB);
                //if (CheckMultipleSettle(allSettleTypeSB.ToString()))
                //{
                //    string priceInfo = string.Format("{0} {1} {2} {3}", viewModel.SecurityID, "Bid", allSettleTypeSB.ToString(), GetClickPrice(dg));

                //    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg))))
                //    {
                //        PointPriceSettleView pointPriceSettleView = new PointPriceSettleView(priceInfo, allSettleTypeSB.ToString(), GetPriceMatchParam(dg, "1"));
                //        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg)), pointPriceSettleView);
                //        //priceMatchView.Show();
                //    }


                //    //PointPriceSettleView pointPriceSettleView = new PointPriceSettleView(priceInfo, allSettleTypeSB.ToString(), GetPriceMatchParam(dg, "1"));
                //    //pointPriceSettleView.Show();
                //}
                //else
                {
                    bool isQuick = QuickOrderHandler.Instance.QuickOrder(dg, true, null, false);


                    if (!isQuick && !WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg))))
                    {
                        // to do ...
                        ////SendPointPriceView sendPointPriceView = new SendPointPriceView(viewModel.SecurityID,
                        ////true, GetClickPrice(dg), GetClickAmount(dg),
                        ////GetSettle(dg), GetNetPrice(dg), GetSource(dg),
                        ////GetWholeFlag(dg), IsFirmFlag(dg), GetPriceKey(dg), GetTransactTime(dg),
                        ////GetDeliveryType(dg), GetClearingMethod(dg), GetQuoteType(dg));

                        string sendclickAmount = clickAmount == 0 ? GetClickAmount(dg) : (clickAmount*1000).ToString();

                        SendPointPriceViewLite sendPointPriceView = new SendPointPriceViewLite(viewModel.SecurityID, true, GetClickPrice(dg), sendclickAmount, settleTypeSB, GetNetPrice(dg), GetSource(dg),
                        GetWholeFlag(dg), IsFirmFlag(dg), GetPriceKey(dg), GetTransactTime(dg),
                        GetDeliveryType(dg), GetClearingMethod(dg), GetQuoteType(dg),
                        GetOrderTip(dg), string.Empty, GetVenueTypeGw(dg));


                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg)), sendPointPriceView);
                        //priceMatchView.Show();
                    }
                }
            }
        }

        #endregion

        #region  Ofr 下单
        private void OfrDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ////if (TitleBarVisible == Visibility.Collapsed)
            ////{
            ////    return;
            ////}

            DataGridRow dg = sender as DataGridRow;

            //if (IsMe(dg))
            //{
            //    QuickUnderline("2", dg);

            //    return;
            //}

            QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);
            if (!PermissionHandler.Instance.CheckVenuePermission(GetSource(dg.Item as DataRowView), viewModel.SecurityID))
                return;

            SendOfrOrder(viewModel, dg.Item as DataRowView);

        }

        /// <summary>
        /// 最优价 Ofr 下单
        /// </summary>
        public void BestOfrSenderOrder()
        {
            
            QuotationListViewModel viewModel = ((QuotationListViewModel)this.DataContext);

            DataRowView dg = viewModel.PriceModel.BestOfrDataRowView;
            if (!PermissionHandler.Instance.CheckVenuePermission(GetSource(dg), viewModel.SecurityID))
                return;

            SendOfrOrder(viewModel, dg, Convert.ToDouble(viewModel.PriceModel.BestOfrAmount.Replace("K", "")));
        }

        public void SendOfrOrder(QuotationListViewModel viewModel, DataRowView dg, double clickAmount = 0)
        {
            //if (IsFirmFlag(dg))
            {

                //if (CheckParam(dg))
                //    return;

                string settleTypeSB = GetAllSettle(dg[CTPriceControler.SETTL_TYPE_FIELD].ToString(), dg[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString());
                string allSettleTypeSB = GetAllChsSettle(settleTypeSB);

                //if (CheckMultipleSettle(allSettleTypeSB.ToString()))
                //{
                //    string priceInfo = string.Format("{0} {1} {2} {3}", viewModel.SecurityID, "Ofr", allSettleTypeSB.ToString(), GetClickPrice(dg));

                //    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg))))
                //    {
                //        PointPriceSettleView pointPriceSettleView = new PointPriceSettleView(priceInfo, allSettleTypeSB.ToString(), GetPriceMatchParam(dg, "2"));
                //        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg)), pointPriceSettleView);
                //        //priceMatchView.Show();
                //    }
                //}
                //else
                {
                    bool isQuick = QuickOrderHandler.Instance.QuickOrder(dg, false, null, false);

                    if (!isQuick && !WindowManager.Instance.IsExist(WindowKeyHelper.GetPriceMatchWinKey(GetPriceKey(dg))))
                    {
                        // to do ...
                        ////SendPointPriceView sendPointPriceView = new SendPointPriceView(viewModel.SecurityID,
                        ////false, GetClickPrice(dg), GetClickAmount(dg),
                        ////GetSettle(dg), GetNetPrice(dg), GetSource(dg),
                        ////GetWholeFlag(dg), IsFirmFlag(dg), GetPriceKey(dg), GetTransactTime(dg),
                        ////GetDeliveryType(dg), GetClearingMethod(dg), GetQuoteType(dg));
                        string sendclickAmount = clickAmount == 0 ? GetClickAmount(dg) : (clickAmount * 1000).ToString();

                        SendPointPriceViewLite sendPointPriceView = new SendPointPriceViewLite(viewModel.SecurityID, false, GetClickPrice(dg), sendclickAmount, settleTypeSB, GetNetPrice(dg), GetSource(dg),
                            GetWholeFlag(dg), IsFirmFlag(dg), GetPriceKey(dg), GetTransactTime(dg),
                            GetDeliveryType(dg), GetClearingMethod(dg), GetQuoteType(dg),
                            GetOrderTip(dg), string.Empty, GetVenueTypeGw(dg));

                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(GetPriceKey(dg)), sendPointPriceView);
                        //priceMatchView.Show();
                    }
                }
            }
        }


        #endregion

        #region function 
        //private bool CheckVenuePermission(string venue, string security)
        //{
        //    if (!string.IsNullOrEmpty(venue) && venue.ToUpper() == "MANUAL")
        //        return true;

        //    string goodsType = CommonUtils.SetGoodsShortname(security);

        //    List<Venue> venues = SystemParamHandler.Instance.GetListVenueByCondition(goodsType, string.Empty);

        //    bool result = false;
        //    foreach (Venue ve in venues)
        //    {
        //        if (ve.venue.ToUpper() == venue.ToUpper())
        //        {
        //            result = true;

        //            break;
        //        }
        //    }

        //    return result;
        //}

        private string GetSource(DataRowView drv)
        {
            string source = string.Empty;
            if (drv != null)
            {
                source = drv[CTPriceControler.SOURCE_FIELD].ToString();
            }
            return source;
        }

        private void GenAllSettlType(DataRowView drv, ref StringBuilder allSettleTypeSB, ref StringBuilder settleTypeSB)
        {
            string settleType = CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_FIELD]);
            Sub_GenSettleType(settleType, ref allSettleTypeSB, ref settleTypeSB);

            string settleType_yuan = CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD]);
            Sub_GenSettleType(settleType_yuan, ref allSettleTypeSB, ref settleTypeSB);
        }

        private void Sub_GenSettleType(string settleType, ref StringBuilder allSettleTypeSB, ref StringBuilder settleTypeSB)
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
                    if (settleTypeSB.Length > 0)
                    {
                        settleTypeSB.Append(",");
                    }
                    settleTypeSB.Append($"'{item}'");

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

        private string GetClickPrice(DataRowView drv)
        {
            string px = string.Empty;
            if (drv != null)
            {
                // 收益率
                decimal px_decimal = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                px = px_decimal.ToString("F4");
            }
            return px;
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

        private string GetPriceKeyForBest(string key)
        {
            string result = string.Empty;
            if (key != null)
            {
                result = CommonUtils.GetOrderId(key);
            }
            return result;
        }

        private string GetClickAmount(DataRowView drv)
        {
            string px = string.Empty;
            if (drv != null)
            {
                // 交易量
                decimal px_decimal = CommonUtils.ConvertToDecimal(drv[CTPriceControler.QTY_FIELD].ToString());
                px = px_decimal.ToString();
            }
            return px;
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

        private string GetWholeFlag(DataRowView drv)
        {
            // 整量标志
            string firmFlag = CommonUtils.ConvertToString(drv[CTPriceControler.WHOLE_FLAG_FIELD]);
            return firmFlag;
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

        private string GetTransactTime(DataRowView drv)
        {
            string result = string.Empty;
            if (drv != null)
            {
                result = drv[CTPriceControler.TRANSACT_TIME_FILED].ToString();
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
        /// <summary>
        /// 获取全部的结算方式，用 : 分开
        /// </summary>
        /// <param name="drv"></param>
        /// <returns></returns>
        private string GetAllSettle(string settletype,string settletypeyuan)
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

        /// <summary>
        /// 获取全部的中文结算方式，用 , 分开
        /// </summary>
        /// <param name="settle"></param>
        /// <returns></returns>
        private string GetAllChsSettle(string settle)
        {
            List<string> list = new List<string>();
            string[] settlTypeSplit = settle.Split(':');
            foreach (string settlTypeItem in settlTypeSplit)
            {
                string item = settlTypeItem.Trim();
                if (string.IsNullOrEmpty(item) == false)
                {
                    // 枚举值拼凑显示内容
                    string settlType = ComboSourceUtil.Instance.FindEnum(item, "settletype_myorder");
                    if (!string.IsNullOrEmpty(settlType))
                    {
                        list.Add(settlType);
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
            }
            return string.Join(",", list.ToArray());
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
        #endregion

        #region 关闭 Close 和 固定 button, 隐藏标题 ,固定窗口
        // <summary>
        /// 关闭 Close 和 固定 button
        /// </summary>
        public void RemoveDbDoubleClickEvent()
        {
            btnFixFloatButton.Visibility = Visibility.Hidden;
            btnShutDown.Visibility = Visibility.Hidden;

            if (!isRemoveMouseLeftButtonDown)
            {
                TextBlockSecurity.MouseLeftButtonDown -= TextBlock_MouseLeftButtonDown;
                isRemoveMouseLeftButtonDown = true;
            }
        }

        /// <summary>
        /// 隐藏标题
        /// </summary>
        public void HideTitle()
        {
            TitleBar.Visibility = Visibility.Collapsed;
        }


        private void btnFixFloatButton_Checked(object sender, RoutedEventArgs e)
        {
            if (btnFixFloatButton.IsChecked.Value)
            {
                if (myTradingPopupWin != null)
                {
                    myTradingPopupWin.Topmost = true;
                }
            }
            else
            {
                if (myTradingPopupWin != null)
                {
                    myTradingPopupWin.Topmost = false;
                }
            }
        }
        #endregion

        #region Bid Hover
        private void bdBid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                if (viewModel.PriceModel.BidIsPermission == "true")
                {
                    bdBid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5F340E"));
                    bdBid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D00"));
                    BidBorder.BorderBrush = bdBid.BorderBrush;
                }
                else
                {
                    bdBid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                }
            }
        }



        private void bdBid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                if (viewModel.PriceModel.BidIsPermission == "true")
                {
                    bdBid.Background = viewModel.PriceModel.UpSideBidBgBrush;
                    bdBid.BorderBrush = viewModel.PriceModel.UpSideBidBorderBrush;
                    BidBorder.BorderBrush = bdBid.BorderBrush;
                    viewModel.PriceModel.ReSetBgColor();
                }
                else
                {
                    bdBid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                }
            }
        }
        #endregion

        #region Ofr Hover
        private void bdOfr_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                if (viewModel.PriceModel.OfrIsPermission == "true")
                {
                    bdOfr.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#063B5B"));
                    bdOfr.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007FFF"));
                    OfrBorder.BorderBrush = bdOfr.BorderBrush;
                }
                else
                {
                    bdOfr.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                }
            }
        }

        private void bdOfr_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                if (viewModel.PriceModel.OfrIsPermission == "true")
                {
                    bdOfr.Background = viewModel.PriceModel.UpSideOfrBgBrush;
                    bdOfr.BorderBrush = viewModel.PriceModel.UpSideOfrBorderBrush;
                    OfrBorder.BorderBrush = bdOfr.BorderBrush;
                    viewModel.PriceModel.ReSetBgColor();
                }
                else
                {
                    bdOfr.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                }
            }
        }
        #endregion

        #region 双击判断
        private void bdBid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            var element = (FrameworkElement)sender;
            if (e.ClickCount == 1)
            {
                var timer = new System.Timers.Timer(500);
                timer.AutoReset = false;
                timer.Elapsed += new ElapsedEventHandler((o, ex) => element.Dispatcher.Invoke(new Action(() =>
                {
                    var timer2 = (System.Timers.Timer)element.Tag;
                    timer2.Stop();
                    timer2.Dispose();
                    UIElement_Click("1");
                    e.Handled = true;
                })));
                timer.Start();
                element.Tag = timer;
            }
            if (e.ClickCount > 1)
            {
                var timer = element.Tag as System.Timers.Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    UIElement_DoubleClick("1");
                    e.Handled = true;
                }
            }
            
        }

        private void bdOfr_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            var element = (FrameworkElement)sender;
            if (e.ClickCount == 1)
            {
                var timer = new System.Timers.Timer(500);
                timer.AutoReset = false;
                timer.Elapsed += new ElapsedEventHandler((o, ex) => element.Dispatcher.Invoke(new Action(() =>
                {
                    var timer2 = (System.Timers.Timer)element.Tag;
                    timer2.Stop();
                    timer2.Dispose();
                    UIElement_Click("0");
                    e.Handled = true;
                })));
                timer.Start();
                element.Tag = timer;
            }
            if (e.ClickCount > 1)
            {
                var timer = element.Tag as System.Timers.Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    e.Handled = true;
                    UIElement_DoubleClick("0");
                    e.Handled = true;
                }
            }

            e.Handled = true;
        }

        private void UIElement_Click(string isBid)
        {
            // MessageBox.Show("单击");
        }

        private void UIElement_DoubleClick(string isBid)
        {
            ////if (isBid == "1")
            ////{
            ////    this.BestBidSenderOrder();
            ////}
            ////else
            ////{
            ////    this.BestOfrSenderOrder();
            ////}
            // MessageBox.Show("双击");
        }
        #endregion

        #region 初始化等处理主逻辑
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="securityID"></param>
        public void Init(string securityID, Window win = null, bool IsReinit = false)
        {
            if (!string.IsNullOrEmpty(securityID))
            {
                if (this.DataContext != null)
                {
                    QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
                    // 清空小红点
                    FilterPanel.SetEllipseVisibility(Visibility.Collapsed);
                    if (!IsReinit)
                    {
                        if (viewModel != null && viewModel.SecurityID == securityID)
                        {
                            return;
                        }
                    }
                    if (viewModel != null)
                    {
                        if (win != null)
                        {
                            viewModel.myWindow = win;
                        }

                        SetContextSecurityID(viewModel, securityID, IsReinit);
                    }
                    else
                    {
                        QuotationListViewModel viewModel2 = new QuotationListViewModel();
                        if (win != null)
                        {
                            viewModel2.myWindow = win;
                        }
                        SetContextSecurityID(viewModel2, securityID, IsReinit);
                    }
                }
                else
                {
                    QuotationListViewModel viewModel = new QuotationListViewModel();
                    if (win != null)
                    {
                        viewModel.myWindow = win;
                    }
                    SetContextSecurityID(viewModel, securityID, IsReinit);
                }
            }
            else
            {
                this.ClearData();
            }
        }

        private void SetContextSecurityID(QuotationListViewModel Qm, string SID, bool isSetDataContext = true)
        {
            MarketCardOrderView pmv = LastExecutions as MarketCardOrderView;
            Qm.SecurityID = SID;
            if (isSetDataContext)
            {
                pmv.SetSecurtityID(SID, true);
            }
            else
            {
                pmv.SetSecurtityID(SID, false);
            }

            if (SID != null)
            {
                //Qm.SecurityID = SID;
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(SID);
                if (bondsInfoPo != null)
                {
                    Qm.Symbol = bondsInfoPo.Short_Name;
                }
            }

            Qm.GetLastExecutionsItem(SID);

            //if (isSetDataContext)
            {
                this.DataContext = Qm;
            }
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        /// <param name="isClearSymbol">是否清空债券信息</param>
        public void ClearData(bool isClearSymbol = true)
        {
            if (this.DataContext != null)
            {
                QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
                if (viewModel != null)
                {
                    viewModel.ClearData(isClearSymbol);
                    MarketCardOrderView pmv = LastExecutions as MarketCardOrderView;
                    LastExecutionViewModel pmvm = pmv.DataContext as LastExecutionViewModel;
                    // 清空小红点
                    FilterPanel.SetEllipseVisibility(Visibility.Collapsed);
                    pmvm.ClearData();
                }
            }
        }


        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            seBond.Visibility = Visibility.Visible;
            TitlePanel.Visibility = Visibility.Collapsed;
            if (this.DataContext != null)
            {
                QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
                seBond.SearchText = viewModel.SecurityID;
                seBond.SelectAll();
            }
        }

        private void seBond_AfterSearch(BondsInfoPo obj)
        {
            seBond.Visibility = Visibility.Collapsed;
            TitlePanel.Visibility = Visibility.Visible;
            
            if (this.DataContext != null)
            {
                QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
                if (viewModel != null && viewModel.SecurityID == seBond.SearchText)
                    return;
                if (string.IsNullOrEmpty(seBond.SearchText))
                {
                    seBond.SearchText = string.Empty;
                    return;
                }
                // viewModel.ClearData(true);
                string oldSecurityId = viewModel.SecurityID;
                string newSecurityId = seBond.SearchText;
                if (newSecurityId == oldSecurityId)
                    return;

                if (WindowManager.Instance.IsExist($"QuotesMiniWindows-{newSecurityId}"))
                {
                    seBond.SearchText = string.Empty;
                    return;
                }

                QuotationListViewModel Qm2 = new QuotationListViewModel();
                if (newSecurityId != null)
                {
                    Qm2.SecurityID = newSecurityId;
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(newSecurityId);
                    if (bondsInfoPo != null)
                    {
                        Qm2.Symbol = bondsInfoPo.Short_Name;
                    }
                }

                if (string.IsNullOrEmpty(newSecurityId))
                {
                    newSecurityId = oldSecurityId;
                    seBond.SearchText = newSecurityId;
                }

                this.Loaded -= PriceAndOrderView_Loaded;
                Qm2.myWindow = viewModel.myWindow;
                Qm2.myWindow.Title = newSecurityId;
                
                this.myTradingPopupWin.myWindow = viewModel.myWindow;
                this.myTradingPopupWin.SecurityID = newSecurityId;
                viewModel.Dispose();
                Qm2.GetLastExecutionsItem(newSecurityId);
                this.DataContext = Qm2;
                LastExecutions.SecurtityID = newSecurityId;
                WindowManager.Instance.ChangWindowsDict(oldSecurityId, newSecurityId, this.myTradingPopupWin);
            }
        }

        private void BidDDataGridRow_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridRow dg = sender as DataGridRow;
            DataRowView dv = dg.Item as DataRowView;
            string isp = dv["IsPermission"].ToString();
            if (isp == "False")
            {
                dg.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                dg.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
            }
            else
            {
                dg.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#583209"));
                dg.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D00"));
            }
        }

        private void BidDDataGridRow_MouseLeave(object sender, MouseEventArgs e)
        {
            DataGridRow dg = sender as DataGridRow;
            dg.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
            dg.BorderBrush = dg.Background;
            //dg.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#191919"));
        }

        private void OfrDDataGridRow_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridRow dg = sender as DataGridRow;
            DataRowView dv = dg.Item as DataRowView;
            string isp = dv["IsPermission"].ToString();
            if (isp == "False")
            {
                dg.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                dg.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
            }
            else
            {
                dg.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#063B5B"));
                dg.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009AFF"));
            }
        }

        private void OfrDDataGridRow_MouseLeave(object sender, MouseEventArgs e)
        {
            DataGridRow dg = sender as DataGridRow;
            dg.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
            dg.BorderBrush = dg.Background;
            // dg.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#191919"));
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (TitleBarVisible == Visibility.Visible)
            {
                if (this.DataContext != null && this.DataContext is QuotationListViewModel)
                {
                    QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                    viewModel.Dispose();
                    GC.Collect();
                }
            }
        }

        public void CloseViewModel()
        {
            if (this.DataContext is SendPriceViewModelLite)
            {
                SendPriceViewModelLite viewModel = (SendPriceViewModelLite)this.DataContext;
                viewModel.Dispose();
                GC.Collect();
            }
            else if (this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                viewModel.Dispose();
                GC.Collect();
            }
        }

        #endregion

        #region 编辑菜单

        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is DataGridRow)
                {
                    SetContextMenu();
                    DataGridRow dgr = sender as DataGridRow;
                    // dgr.IsSelected = true;
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

                Common.Model.Fix.Message.NewOrderSingle singleOrder = GetNewOrderSingle(MyOrderProcesser.Processer.GetOrderById(orderId));
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
                priceMatchParam.Info3 = singleOrder.Info3;//用于IDB单子

                if (singleOrder.FirmFlag == "1")
                {
                    priceMatchParam.IsFirm = true;// firm
                }
                else
                {
                    priceMatchParam.IsFirm = false;// nonfirm
                }

                priceMatchParam.OldOrder = singleOrder;

                SendPriceViewLite sendPriceView = new SendPriceViewLite(priceMatchParam);
                // bid;
                if (singleOrder.Side == "1")
                {
                    sendPriceView.OfrBtn.IsEnabled = false;
                }
                // ofr
                else if (singleOrder.Side == "2")
                {
                    sendPriceView.BidBtn.IsEnabled = false;
                }

                if (singleOrder.FirmFlag == "1")
                {
                    //sendPriceView.AmountTxt.IsEnabled = false;
                    //sendPriceView.btnOne.IsEnabled = sendPriceView.btnThree.IsEnabled = sendPriceView.btnFive.IsEnabled = false;
                }

                sendPriceView.Search_TextBox.IsEnabled = false;
                sendPriceView.btnXBond.IsEnabled = false;

                //sendPriceView.datePicker.IsEnabled = false;
                //sendPriceView.cbSettle.IsEnabled = false;
                //sendPriceView.lbOne.IsEnabled = sendPriceView.lbOther.IsEnabled = false;
                //sendPriceView.lbSpotOne.IsEnabled = sendPriceView.lbSpotOther.IsEnabled = false;

                //sendPriceView.lbAddOneNotLimit.IsEnabled = sendPriceView.lbAddOneToday.IsEnabled = sendPriceView.lbAddOneTomorrow.IsEnabled = false;


                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetPointWinKey(singleOrder.StrategyOrderId)))
                {
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetPointWinKey(singleOrder.StrategyOrderId), sendPriceView);
                }
            }
            else
            {
                LogHandler.Info("QuotationList MenuItem_Click 没有取到数据");
            }

        }
        #endregion

        #region 撤单菜单
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
                result = CustomMessageBox.Question("是否撤销该笔报价？", "删除订单");

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
                        CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
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

            if (contextMenu.Tag != null && contextMenu.Tag.GetType() == typeof(string))
            {
                foreach (MenuItem menuItem in contextMenu.Items)
                {
                    if (menuItem.Name.Equals("miEdit") || menuItem.Name.Equals("miCancel"))
                        menuItem.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (contextMenu.Tag != null && contextMenu.Tag is QuotationCloneRowData)
                {
                    QuotationCloneRowData dgRow = contextMenu.Tag as QuotationCloneRowData;

                    // todo 保价逻辑， 数据在drv中

                    bool isMe = dgRow.IsMe;

                    foreach (MenuItem menuItem in contextMenu.Items)
                    {
                        if (menuItem.Name.Equals("miEdit") || menuItem.Name.Equals("miCancel"))
                            menuItem.Visibility = isMe ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else
                {
                    LogHandler.Info("QuotationList UserControl_ContextMenuOpening 没有取到数据");
                }
            }
        }

        #region 获取数据
        private QuotationCloneRowData GetCloneData(DataRowView drv)
        {
            QuotationCloneRowData cloneData = new QuotationCloneRowData();

            cloneData.securityID = CommonUtils.ConvertToString(drv[CTPriceControler.SECURITY_ID_FIELD]); ;
            cloneData.side = CommonUtils.ConvertToString(drv[CTPriceControler.SIDE_FIELD]);
            cloneData.entryPx = CommonUtils.ConvertToString(drv[CTPriceControler.ENTRY_PX_FIELD]);
            cloneData.settle = GetSettle(drv);//"2";
            cloneData.netPrice = GetNetPrice(drv);
            cloneData.key = CommonUtils.GetOrderId(CommonUtils.ConvertToString(drv[CTPriceControler.KEY_FIELD]));
            cloneData.IsMe = (bool)drv[CTPriceControler.ISME_FIELD];
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
            string info3 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO3_FIELD]);

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
            sendPricePo.Info3 = info3;

            return sendPricePo;
        }
        #endregion

        #endregion
    }
}