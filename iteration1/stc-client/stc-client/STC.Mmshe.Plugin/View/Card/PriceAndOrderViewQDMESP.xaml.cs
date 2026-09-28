using STC.Common.DataProcesser;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.ViewModel.Data;
using STC.Frame.UI;
using STC.Frame.Utils;
using STC.Mm.Plugin.CustomeUI;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.Utils.DataProcesser;
using STC.Mm.Plugin.View.Card;
using STC.Mm.Plugin.View.Order;
using STC.Mm.Plugin.ViewModel;
using STC.Mm.Plugin.ViewModel.TradeBookHandler;
using STC.XBond.Plugin.Utils;
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

namespace STC.Mm.Plugin.View.Card
{
    /// <summary>
    /// PriceAndOrderView.xaml 的交互逻辑
    /// </summary>
    public partial class PriceAndOrderViewQDMESP : UserControl
    {
        NoBorderWin myTradingPopupWin;

        public static readonly DependencyProperty VenuesFilterStringPropertyProperty = DependencyProperty.Register("VenuesFilterString", typeof(string), typeof(PriceAndOrderViewQDMESP),
        new PropertyMetadata("1", onFilterString));

        //public static readonly DependencyProperty WidthPropertyProperty = DependencyProperty.Register("Width", typeof(string), typeof(PriceAndOrderView),
        //new PropertyMetadata("", onWidth));

        public string VenuesFilterString
        {
            get { return (string)GetValue(VenuesFilterStringPropertyProperty); }
            set 
            { 
                SetValue(VenuesFilterStringPropertyProperty, value); 
            }
        }

        private static void onTitleBarVisibility(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            PriceAndOrderViewQDMESP source = (PriceAndOrderViewQDMESP)sender;
            source.TitleBar.Visibility = (Visibility)args.NewValue;
        }

        private static void onFilterString(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;

            string filterstr = (string)args.NewValue;
            if (filterstr == "1")
            {
                //Dictionary<string, IList> filterDic
                Dictionary<string, IList> FilterDic = new Dictionary<string, IList>();
                ListBox lb = new ListBox();
                ListBoxItem tmp = new ListBoxItem();
                tmp.Content = "QDMESP"; //QDMESP   //ODM X-Bound
                tmp.IsSelected = true;
                lb.Items.Add(tmp);

                FilterDic.Add("Venues", lb.SelectedItems);

                // 过滤
                PriceAndOrderViewQDMESP source = (PriceAndOrderViewQDMESP)sender;
                if (source.DataContext != null && source.DataContext is QuotationListViewModel)
                {
                    QuotationListViewModel viewModel = (QuotationListViewModel)(source.DataContext);
                    // 过滤
                    source.FilterPanel_FilterDicChanged(source, null);
                }
            }
            else
            {
                PriceAndOrderViewQDMESP source = (PriceAndOrderViewQDMESP)sender;
                if (source.DataContext != null && source.DataContext is QuotationListViewModel)
                {
                    QuotationListViewModel viewModel = (QuotationListViewModel)(source.DataContext);
                    // 过滤
                    source.FilterPanel_FilterDicChanged(source, null);
                }
            }


        }

        public PriceAndOrderViewQDMESP()
        {
            InitializeComponent();
        }

        public PriceAndOrderViewQDMESP(NoBorderWin mw)
        {
            InitializeComponent();
            this.myTradingPopupWin = mw;
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                // this.LastExecutions.SecurtityID = string.IsNullOrEmpty(viewModel.SecurityID) ? "xxxxxx" : viewModel.SecurityID;
                this.Loaded += PriceAndOrderView_Loaded;
            }
        }

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
        }


        ContextMenu contextMenu;

        //public bool GetIdcTraderPermission()
        //{
            //if (!string.IsNullOrEmpty(LocalEnv.UserInfo.IdcTradePermissions) && LocalEnv.UserInfo.IdcTradePermissions == "1")
            //    return true;
            //else
            //    return false;
        //}

        private void SetContextMenu()
        {
            // to do ... 接口不支持
            //bool result = GetIdcTraderPermission();
            //if (contextMenu == null && result)
            //{
                contextMenu = (ContextMenu)this.FindResource("OrderContextMenu");

            //}
        }

        private bool _isBid = false;
        private void BdBid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetContextMenu();

            _isBid = true;
            if (contextMenu != null)
            {
                contextMenu.IsOpen = true;
            }
        }

        private void BdOfr_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetContextMenu();

            _isBid = false;
            if (contextMenu != null)
            {
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

        private void OrderMenuItme_Click(object sender, RoutedEventArgs e)
        {
            QuotationListViewModel viewModel = this.DataContext as QuotationListViewModel;
            if (_isBid)
            {
                if (viewModel != null)
                {
                    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, viewModel.SecurityID)))
                    {
                        SendPriceViewLite sendPriceView = new SendPriceViewLite(viewModel.SecurityID, true);

                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.BID, viewModel.SecurityID), sendPriceView);
                        //sendPriceView.Owner = Application.Current.MainWindow;
                        //sendPriceView.Show();
                    }
                }
            }
            else
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.OFR, viewModel.SecurityID)))
                {
                    SendPriceViewLite sendPriceView = new SendPriceViewLite(viewModel.SecurityID, false);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetSendPriceWinKey(WindowKeyHelper.OFR, viewModel.SecurityID), sendPriceView);
                    //sendPriceView.Owner = Application.Current.MainWindow;
                    //sendPriceView.Show();
                }
            }
         }

        public void FilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;

                Dictionary<string, IList> FilterDic = UnionDict(GetFilterDic(), this.FilterPanel.FilterDic);
                     
                viewModel.FilterData(FilterDic);
            }
        }

        private Dictionary<string, IList> UnionDict(Dictionary<string, IList> dic1, Dictionary<string, IList> dic2)
        {
            foreach (var ke in dic1)
            {
                if (!dic2.ContainsKey(ke.Key))
                    dic2.Add(ke.Key, ke.Value); 
            }
            return dic2;
        }

        #region Bid 下单
        private void BidDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

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

            SendBidOrder(viewModel, dg);

        }

        public void SendBidOrder(QuotationListViewModel viewModel, DataRowView dg)
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

                        SendPointPriceViewLite sendPointPriceView = new SendPointPriceViewLite(viewModel.SecurityID, true, GetClickPrice(dg), GetClickAmount(dg), settleTypeSB, GetNetPrice(dg), GetSource(dg),
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

            SendOfrOrder(viewModel, dg);
        }

        public void SendOfrOrder(QuotationListViewModel viewModel, DataRowView dg)
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

                        SendPointPriceViewLite sendPointPriceView = new SendPointPriceViewLite(viewModel.SecurityID, false, GetClickPrice(dg), GetClickAmount(dg), settleTypeSB, GetNetPrice(dg), GetSource(dg),
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


        // <summary>
        /// 只读行情，禁止点价
        /// </summary>
        public void RemoveDbDoubleClickEvent()
        {
            btnFixFloatButton.Visibility = Visibility.Hidden;
            btnShutDown.Visibility = Visibility.Hidden;
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
            return;
            ////if (isBid == "1")
            ////{
            ////    this.BestBidSenderOrder();
            ////}
            ////else
            ////{
            ////    this.BestOfrSenderOrder();
            ////}
        }
        #endregion


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
                    //if (!IsReinit)
                    //{
                    //    if (viewModel != null && viewModel.SecurityID == securityID)
                    //    {
                    //        this.FilterPanel_FilterDicChanged(null, null);
                    //        return;
                    //    }
                    //}

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

        private void SetContextSecurityID(QuotationListViewModel Qm, string SID, bool isSetDataContext = false)
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

            if (isSetDataContext)
            {
                this.DataContext = Qm;
                this.FilterPanel_FilterDicChanged(null, null);
            }
        }

        public Dictionary<string, IList> GetFilterDic()
        { 
            Dictionary<string, IList> FilterDic = new Dictionary<string, IList>();
            ListBox lb = new ListBox();
            ListBoxItem tmp = new ListBoxItem();
            tmp.Content = "QDMESP"; //QDMESP   //ODM X-Bound
            tmp.IsSelected = true;
            lb.Items.Add(tmp);

            FilterDic.Add("Venues", lb.SelectedItems);
            return FilterDic;
        }

        /// <summary>
        /// 设置过滤条件 +0,+1,T+0,T+1, ""或不传参数 则清空清算速度
        /// </summary>
        /// <param name="setttle"></param>
        public void SetFilterDicSetttle(string setttle = "")
        {
            ListBox lb = new ListBox();
            ListBoxItem tmp = new ListBoxItem();
            string key = "Settle"; //+0 或 +1
            if (!string.IsNullOrEmpty(setttle))
            {
                tmp.Content = setttle.Substring(0, 1) == "T" ? setttle.Replace("T", "") : setttle;
                tmp.IsSelected = true;
                lb.Items.Add(tmp);

                if (!this.FilterPanel.FilterDic.ContainsKey(key))
                {
                    this.FilterPanel.FilterDic.Add(key, lb.SelectedItems);
                }
                else
                {
                    this.FilterPanel.FilterDic[key] = lb.SelectedItems;
                }
            }
            else
            {
                if (this.FilterPanel.FilterDic.ContainsKey(key))
                {
                    this.FilterPanel.FilterDic.Remove(key);
                }
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
                    pmvm.ClearData();
                }
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
           //  GC.Collect();
        }
    }
}