using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.Common.View.Order.InnerPanel;
using STC.IRS.Plugin.ViewModel;
using STC.IRS.Plugin.ViewModel.Data;
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

namespace STC.IRS.Plugin.View
{
    /// <summary>
    /// InnerDepthPopup.xaml 的交互逻辑
    /// </summary>
    public partial class InnerDepthPopupView : UserControl
    {
        public InnerDepthPopupView()
        {
            InitializeComponent();
        }

        private void CloseDepthPopup()
        {
            if (this.Parent != null && this.Parent is Popup)
            {
                Popup popup = this.Parent as Popup;
                popup.IsOpen = false;
            }
        }

        private void JOIN_Click(object sender, RoutedEventArgs e)
        {
            CloseDepthPopup();
            InnerDepthPopupViewModel viewModel = this.DataContext as InnerDepthPopupViewModel;

            if (viewModel != null)
            {
                string securityID = "TradeBook";
                if (!string.IsNullOrEmpty(securityID))
                {
                    securityID = viewModel.SecurityID;
                }

                SendIRSView sendRequireView = new SendIRSView(securityID, true);
                if (!WindowManager.Instance.IsExist($"IRS_{securityID}")) 
                {
                    WindowManager.Instance.OpenOnlyOneWindow($"IRS_{securityID}", sendRequireView);
                }
                //Common.ViewModel.TradeBookHandler.QuickOrderHandler.Instance.QuickPrice(null, true, viewModel.SecurityID);
            }
            
        }

        private void REF_Click(object sender, RoutedEventArgs e)
        {
            InnerDepthPopupViewModel viewModel = this.DataContext as InnerDepthPopupViewModel;

            if (viewModel != null)
            {
                viewModel.DealDelete();
            }
        }

        private void depthDg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CloseDepthPopup();
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            if (targetCell != null)
            {
                if (targetCell.Column.Header.ToString() == "Trader(B)"
                    || targetCell.Column.Header.ToString() == "Vol.Bid"
                    || targetCell.Column.Header.ToString() == "Bid")
                {
                    InnerDepthPopupViewModel viewModel = this.DataContext as InnerDepthPopupViewModel;
                    if (viewModel != null)
                    {
                        InnerOrderData innerOrderData = viewModel.SelectedDepthData;
                        OrderBest_Bid(!innerOrderData.Price_InnerBid.HasValue || !innerOrderData.Qty_InnerBid.HasValue);
                    }
                }
                else if (targetCell.Column.Header.ToString() == "Trader(O)"
                    || targetCell.Column.Header.ToString() == "Vol.Ofr"
                    || targetCell.Column.Header.ToString() == "Ofr")
                {
                    InnerDepthPopupViewModel viewModel = this.DataContext as InnerDepthPopupViewModel;
                    if (viewModel != null)
                    {
                        InnerOrderData innerOrderData = viewModel.SelectedDepthData;
                        OrderBest_Ofr(!innerOrderData.Price_InnerOfr.HasValue || !innerOrderData.Qty_InnerOfr.HasValue);
                    }
                }
            }
        }


        private void OrderBest_Bid(bool isDrp)
        {
            InnerDepthPopupViewModel viewModel = this.DataContext as InnerDepthPopupViewModel;
            if (viewModel != null)
            {
                InnerOrderData innerOrderData = viewModel.SelectedDepthData;
                if (innerOrderData.Count_InnerBid > 0)
                {
                    if (innerOrderData.IsMe_InnerBid == false)
                    {
                        if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetPointWinKey(innerOrderData.StrategyOrderId_innerBid)))
                        {
                            SendPointPriceView sendPointPriceView = new SendPointPriceView(viewModel.SecurityID, true, innerOrderData.Price_InnerBid.ToString(), (innerOrderData.Qty_InnerBid * 100).ToString(),
                            innerOrderData.SettleType_InnerBid, innerOrderData.NetPrice_InnerBid.ToString(), "MANUAL",
                            innerOrderData.WholeFlag_InnerBid, innerOrderData.FirmFlag_InnerBid == "1", innerOrderData.StrategyOrderId_innerBid,
                            null, null, null, null,
                            null, string.Empty, "INNER", null, innerOrderData.IntentionId_InnerBid, isDrp, $"{innerOrderData.RoleName_InnerBid}-{innerOrderData.PartySubName_InnerBid}", null, null, null, null, new Common.ViewModel.Order.DRPParamInfoModel() { OrderId = innerOrderData.OrderID });


                            WindowManager.Instance.OpenWindow(ComWindowKeyHelper.GetPointWinKey(innerOrderData.StrategyOrderId_innerBid), sendPointPriceView);
                        }
                    }
                    else
                    {
                        EditOrder_BestBid(innerOrderData, true, viewModel.SecurityID);
                    }
                }
            }
            
        }

        private void OrderBest_Ofr(bool isDrp)
        {
            InnerDepthPopupViewModel viewModel = this.DataContext as InnerDepthPopupViewModel;
            if (viewModel != null)
            {
                InnerOrderData innerOrderData = viewModel.SelectedDepthData;

                if (innerOrderData.Count_InnerOfr > 0)
                {
                    if (innerOrderData.IsMe_InnerOfr == false)
                    {
                        if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetPointWinKey(innerOrderData.StrategyOrderId_InnerOfr)))
                        {
                            SendPointPriceView sendPointPriceView = new SendPointPriceView(viewModel.SecurityID, false, innerOrderData.Price_InnerOfr.ToString(), (innerOrderData.Qty_InnerOfr * 100).ToString(),
                            innerOrderData.SettleType_InnerOfr, innerOrderData.NetPrice_InnerOfr.ToString(), "MANUAL",
                            innerOrderData.WholeFlag_InnerOfr, innerOrderData.FirmFlag_InnerOfr == "1", innerOrderData.StrategyOrderId_InnerOfr,
                            null, null, null, null,
                            null, string.Empty, "INNER", null, innerOrderData.IntentionId_InnerOfr, isDrp, $"{innerOrderData.RoleName_InnerOfr}-{innerOrderData.PartySubName_InnerOfr}", null, null, null, null, new Common.ViewModel.Order.DRPParamInfoModel() { OrderId = innerOrderData.OrderID });


                            WindowManager.Instance.OpenWindow(ComWindowKeyHelper.GetPointWinKey(innerOrderData.StrategyOrderId_InnerOfr), sendPointPriceView);
                        }
                    }
                    else
                    {
                        EditOrder_BestOfr(innerOrderData, false, viewModel.SecurityID);
                    }
                }
            }
        }

        public void EditOrder_BestBid(InnerOrderData orderData, bool isBid, string securityId)
        {
            string side = isBid ? ComWindowKeyHelper.BID : ComWindowKeyHelper.OFR;


            if (orderData != null)
            {
                PriceMatchParam priceMatchParam = new PriceMatchParam();
                priceMatchParam.CustomFirmAmount = (orderData.Qty_InnerBid * 100).ToString(); // OrderQty 订单量
                priceMatchParam.OrderId = orderData.StrategyOrderId_innerBid;// 订单号
                priceMatchParam.Side = "1"; // 方向
                priceMatchParam.Symbol = securityId; // 代码
                priceMatchParam.SymbolName = orderData.Symbol; // 名称
                priceMatchParam.WholeFlag = orderData.WholeFlag_InnerBid; // 整量标志
                //priceMatchParam.NetPrice = orderData.NetPrice; // 净价
                priceMatchParam.Price = orderData.Price_InnerBid.ToString(); // 收益率
                priceMatchParam.SettlType = orderData.SettleType_InnerBid; // 清算速度
                priceMatchParam.NonfirmTime = string.Empty; // nonfirmTime;
                priceMatchParam.OperationType = "1";
                if (orderData.FirmFlag_InnerBid == "1")
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
                if (string.IsNullOrEmpty(orderData.IntentionId_InnerBid))
                {
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                    {
                        SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
                        // bid;
                        sendPriceView.SetButtonEnable("OfrBtn", false);


                        if (orderData.FirmFlag_InnerBid == "1")
                        {
                            sendPriceView.SetButtonEnable("Search_TextBox", false);
                        }
                        else
                        {
                            sendPriceView.SetButtonEnable("Search_TextBox", false);
                        }

                        if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                        {
                            WindowManager.Instance.OpenWindow(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId), sendPriceView);
                        }
                    }
                }
                else
                {
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                    {
                        DataRow myOrderDr = MyOrderProcesser.Processer.GetOrderById(orderData.StrategyOrderId_innerBid);
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
                        priceMatchParam.IntentionID = orderData.IntentionId_InnerBid;

                        //如果是模糊需求
                        if (orderData.SecurityID == "99999")
                        {
                            Common.Model.Fix.Message.NewOrderSingle oldOrder = new Common.Model.Fix.Message.NewOrderSingle();
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

        public void EditOrder_BestOfr(InnerOrderData orderData, bool isBid, string securityId)
        {
            string side = isBid ? ComWindowKeyHelper.BID : ComWindowKeyHelper.OFR;

            if (orderData != null)
            {
                PriceMatchParam priceMatchParam = new PriceMatchParam();
                priceMatchParam.CustomFirmAmount = (orderData.Qty_InnerOfr * 100).ToString(); // OrderQty 订单量
                priceMatchParam.OrderId = orderData.StrategyOrderId_InnerOfr;// 订单号
                priceMatchParam.Side = "2"; // 方向
                priceMatchParam.Symbol = securityId; // 代码
                priceMatchParam.SymbolName = orderData.Symbol; // 名称
                priceMatchParam.WholeFlag = orderData.WholeFlag_InnerOfr; // 整量标志
                //priceMatchParam.NetPrice = orderData.NetPrice; // 净价
                priceMatchParam.Price = orderData.Price_InnerOfr.ToString(); // 收益率
                priceMatchParam.SettlType = orderData.SettleType_InnerOfr; // 清算速度
                priceMatchParam.NonfirmTime = string.Empty; // nonfirmTime;
                priceMatchParam.OperationType = "1";
                if (orderData.FirmFlag_InnerOfr == "1")
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
                if (string.IsNullOrEmpty(orderData.IntentionId_InnerOfr))
                {
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                    {
                        SendPriceView sendPriceView = new SendPriceView(priceMatchParam);

                        sendPriceView.SetButtonEnable("BidBtn", false);


                        if (orderData.FirmFlag_InnerOfr == "1")
                        {
                            sendPriceView.SetButtonEnable("Search_TextBox", false);
                        }
                        else
                        {
                            sendPriceView.SetButtonEnable("Search_TextBox", false);
                        }

                        if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                        {
                            WindowManager.Instance.OpenWindow(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId), sendPriceView);
                        }
                    }
                }
                else
                {
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(side, securityId)))
                    {
                        DataRow myOrderDr = MyOrderProcesser.Processer.GetOrderById(orderData.StrategyOrderId_InnerOfr);
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
                        priceMatchParam.IntentionID = orderData.IntentionId_InnerOfr;
                        //如果是模糊需求
                        if (orderData.SecurityID == "99999")
                        {
                            Common.Model.Fix.Message.NewOrderSingle oldOrder = new Common.Model.Fix.Message.NewOrderSingle();
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

        /// <summary>
        /// 0:没有数据
        /// 1:全部是我的
        /// 2:全部不是我的
        /// 3:有的是我的， 有的不是我的
        /// </summary>
        /// <returns></returns>
        private string IsMe_SelectedItem_Best(string side)
        {
            if (this.depthDg_irs.SelectedItems.Count > 0)
            {
                int isMeCount = 0;
                int isNotMeCount = 0;
                foreach (InnerOrderData item in this.depthDg_irs.SelectedItems)
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
            if (this.depthDg_irs.SelectedItems.Count > 1)
            {
                ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                PointPriceMenu_Best.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (this.depthDg_irs.SelectedItem is InnerOrderData)
                {
                    InnerOrderData data = this.depthDg_irs.SelectedItem as InnerOrderData;

                    if (data.SecurityID == "99999")
                    {
                        ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                        PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        JoinMenu_Best.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                    }
                    else if (depthDg_irs.CurrentColumn.Header.ToString() == "Trader(B)"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Vol.Bid"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Bid")
                    {

                        if (data.Price_InnerBid.HasValue && data.Qty_InnerBid.HasValue)
                        {
                            ConsiderationMenu_Best.Visibility = Visibility.Visible;
                            if (data.IsMe_InnerBid == false)
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
                            if(data.FirmFlag_InnerBid == "1")
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
                        if (data.Price_InnerOfr.HasValue && data.Qty_InnerOfr.HasValue)
                        {
                            ConsiderationMenu_Best.Visibility = Visibility.Visible;
                            if (data.IsMe_InnerOfr == false)
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
                }
                else
                {
                    ConsiderationMenu_Best.Visibility = Visibility.Collapsed;
                    PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                }
            }
            if (this.depthDg_irs.SelectedItems.Count > 0)
            {
                if (depthDg_irs.CurrentColumn.Header.ToString() == "Trader(B)"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Vol.Bid"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Bid")
                {
                    string status = IsMe_SelectedItem_Best("1");

                    if (status == "1")
                    {
                        JoinMenu_Best.Visibility = Visibility.Collapsed;
                        //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        REFMenu_Best.Visibility = Visibility.Visible;
                        if (this.depthDg_irs.SelectedItems.Count > 1)
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

                        if (this.depthDg_irs.SelectedItems.Count > 1)
                        {
                            JoinMenu_Best.Visibility = Visibility.Collapsed;
                            //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                            ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            JoinMenu_Best.Visibility = Visibility.Visible;
                            //PointPriceMenu_Best.Visibility = Visibility.Visible;
                            ReplyPriceMenu_Best.Visibility = Visibility.Visible;

                            if (this.depthDg_irs.SelectedItem != null && this.depthDg_irs.SelectedItem is InnerOrderData)
                            {
                                InnerOrderData data = this.depthDg_irs.SelectedItem as InnerOrderData;

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

                if (depthDg_irs.CurrentColumn.Header.ToString() == "Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Vol.Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Trader(O)")
                {
                    string status = IsMe_SelectedItem_Best("2");

                    if (status == "1")
                    {
                        JoinMenu_Best.Visibility = Visibility.Collapsed;
                        //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                        ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        REFMenu_Best.Visibility = Visibility.Visible;
                        if (this.depthDg_irs.SelectedItems.Count > 1)
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
                        if (this.depthDg_irs.SelectedItems.Count > 1)
                        {
                            JoinMenu_Best.Visibility = Visibility.Collapsed;
                            //PointPriceMenu_Best.Visibility = Visibility.Collapsed;
                            ReplyPriceMenu_Best.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            JoinMenu_Best.Visibility = Visibility.Visible;
                            //PointPriceMenu_Best.Visibility = Visibility.Visible;
                            ReplyPriceMenu_Best.Visibility = Visibility.Visible;

                            if (this.depthDg_irs.SelectedItem != null && this.depthDg_irs.SelectedItem is InnerOrderData)
                            {
                                InnerOrderData data = this.depthDg_irs.SelectedItem as InnerOrderData;

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

        private void JoinMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            CloseDepthPopup();
            InnerDepthPopupViewModel viewModel = this.DataContext as InnerDepthPopupViewModel;
            if (viewModel != null)
            {
                bool isBid = depthDg_irs.CurrentColumn.Header.ToString() != "Ofr" && depthDg_irs.CurrentColumn.Header.ToString() != "Vol.Ofr" && depthDg_irs.CurrentColumn.Header.ToString() != "Trader(O)";


                string securityID = "TradeBook";
                if (!string.IsNullOrEmpty(securityID))
                {
                    securityID = viewModel.SecurityID;
                }

                SendIRSView sendRequireView = new SendIRSView(securityID, isBid, true);
                if (!WindowManager.Instance.IsExist($"IRS_{securityID}"))
                {
                    WindowManager.Instance.OpenOnlyOneWindow($"IRS_{securityID}", sendRequireView);
                }

                //Common.ViewModel.TradeBookHandler.QuickOrderHandler.Instance.QuickPrice(null, isBid, viewModel.SecurityID);
            }
        }

        private void PointPriceMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            CloseDepthPopup();
            if (depthDg_irs.CurrentColumn.Header.ToString() == "Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Vol.Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Trader(O)")
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
            CloseDepthPopup();
            if (depthDg_irs.CurrentColumn.Header.ToString() == "Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Vol.Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Trader(O)")
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
            InnerDepthPopupViewModel viewModel = this.DataContext as InnerDepthPopupViewModel;
            if (viewModel != null)
            {
                if (depthDg_irs.CurrentColumn.Header.ToString() == "Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Vol.Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Trader(O)")
                {
                    List<string> refList = new List<string>();
                    if (this.depthDg_irs.SelectedItems.Count > 0)
                    {
                        foreach (InnerOrderData item in this.depthDg_irs.SelectedItems)
                        {
                            string securityid = item.SecurityID;
                            decimal? price = item.Price_InnerOfr;
                            if (item.IsMe_InnerOfr)
                            {
                                refList.Add(item.StrategyOrderId_InnerOfr);
                            }
                            
                        }
                    }
                    if (refList.Count > 0)
                    {
                        DealDelete(refList);
                    }
                }
                else
                {
                    List<string> refList = new List<string>();
                    if (this.depthDg_irs.SelectedItems.Count > 0)
                    {
                        foreach (InnerOrderData item in this.depthDg_irs.SelectedItems)
                        {
                            string securityid = item.SecurityID;
                            decimal? price = item.Price_InnerBid;
                            if (item.IsMe_InnerBid)
                            {
                                refList.Add(item.StrategyOrderId_innerBid);
                            }
                        }
                    }
                    if (refList.Count > 0)
                    {
                        DealDelete(refList);
                    }
                }
            }
        }

        internal async void DealDelete(List<string> refList)
        {
            //LogHandler.Info($"DealDelete {orderId}");
            bool result = CustomMessageBox.Question("是否撤销选中报价？", "REF");

            if (result == true)
            {
                StringBuilder errMsg = new StringBuilder();
                foreach (string quoteId in refList)
                {
                    string msg = await CancelOne(quoteId);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        errMsg.AppendLine(msg);
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

        private void EditMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            CloseDepthPopup();
            if (depthDg_irs.CurrentColumn.Header.ToString() == "Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Vol.Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Trader(O)")
            {
                OrderBest_Ofr(false);
            }
            else
            {
                OrderBest_Bid(false);
            }
        }

        private void ConsiderationMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            CloseDepthPopup();
            InnerDepthPopupViewModel viewModel = this.DataContext as InnerDepthPopupViewModel;
            if (viewModel != null)
            {
                if (depthDg_irs.CurrentColumn.Header.ToString() == "Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Vol.Ofr"
                    || depthDg_irs.CurrentColumn.Header.ToString() == "Trader(O)")
                {
                    InnerOrderData innerOrderData = viewModel.SelectedDepthData;
                    ConsiderationOrder(innerOrderData, true, innerOrderData);
                }
                else
                {
                    InnerOrderData innerOrderData = viewModel.SelectedDepthData;
                    ConsiderationOrder(innerOrderData, false, innerOrderData);
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
                    if (orderData.SecurityID == "99999")
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


        private void CopyMenu_Best_Click(object sender, RoutedEventArgs e)
        {
            this.depthDg_irs.CopyData_Best();
        }

        private void SearchMenu_Best_Click(object sender, RoutedEventArgs e)
        {

        }

        private void depthDg_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
            this.depthDg_irs.CurrentColumn = targetCell.Column;
        }
    }
}
