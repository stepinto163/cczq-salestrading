using netapi.Api;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Base;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Data;
using STC.WorkTable.Plugin.View.WorkTable;
using STC.WorkTable.Plugin.ViewModel.IntentionPanel;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace STC.WorkTable.Plugin.View.IntentionPanel
{
    /// <summary>
    /// IntentionPanel.xaml 的交互逻辑
    /// </summary>
    public partial class IntentionPanel : UserControl
    {
        IntentionPanelViewModel intentionPanelViewModel = new IntentionPanelViewModel();

        public IntentionPanel()
        {
            InitializeComponent();
            this.VenuesLv.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(VenuesLv_MouseLeftButtonDown), true);
            this.DataContext = intentionPanelViewModel;
        }

        public void UpdateIntentionID(string intentionID)
        {
            if (intentionPanelViewModel != null)
            {
                intentionPanelViewModel.IntentionID = intentionID;
                this.quotationPart.Init(intentionPanelViewModel.Data.SecurityId);

                #region 设置渠道权限
                List<Common.Model.TBook.Venue> venues = SystemParamHandler.Instance.GetListVenueByCondition(intentionPanelViewModel.Data.SecurityType, string.Empty);
                foreach (ListBoxItem item in VenuesLv.Items)
                {
                    if(item.Content.ToString() == "内部")
                    {
                        continue;
                    }
                    item.IsEnabled = false;
                }
                if (venues != null && venues.Count > 0)
                {
                    foreach (Common.Model.TBook.Venue uv in venues)
                    {
                        foreach (ListBoxItem item in VenuesLv.Items)
                        {
                            if ((uv.venue == CommonConstants.CNEX_EN && item.Content.ToString() == CommonConstants.CNEX)
                                || (uv.venue == CommonConstants.TPSC_EN && item.Content.ToString() == CommonConstants.TPSC)
                                || (uv.venue == CommonConstants.CTTJ_EN && item.Content.ToString() == CommonConstants.CTTJ)
                                || (uv.venue == CommonConstants.PTCN_EN && item.Content.ToString() == CommonConstants.PTCN)
                                || (uv.venue == CommonConstants.CCTB_EN && item.Content.ToString() == CommonConstants.CCTB)
                                || (uv.venue == "ODM" && item.Content.ToString() == "XBOND"))
                            {
                                item.IsEnabled = true;
                                break;
                            }
                        }
                    }
                }
                #endregion
            }
        }

        private void btnFixFloatButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void seBond_AfterSearch(Common.Model.Base.BondsInfoPo obj)
        {

        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.intentionPanelViewModel != null && this.intentionPanelViewModel.SelectIntentionOrder != null)
            {
                List<string> venuesList = new List<string>();
                if (string.IsNullOrEmpty(this.intentionPanelViewModel.SelectIntentionOrder.SelfSelectVenues) == false)
                {
                    venuesList = this.intentionPanelViewModel.SelectIntentionOrder.SelfSelectVenues.Split(CommonConstants.COMMA_CHAR).ToList();
                    for(int i = 0; i < venuesList.Count; i++)
                    {
                        venuesList[i] = venuesList[i].Substring(4);
                    }
                }
                string currentVenues = this.intentionPanelViewModel.SelectIntentionOrder.SelfSelectVenues;
                foreach (ListBoxItem item in VenuesLv.Items)
                {
                    if ((venuesList.Contains(CommonConstants.CNEX_EN) && item.Content.ToString() == CommonConstants.CNEX)
                                || (venuesList.Contains(CommonConstants.TPSC_EN) && item.Content.ToString() == CommonConstants.TPSC)
                                || (venuesList.Contains(CommonConstants.CTTJ_EN) && item.Content.ToString() == CommonConstants.CTTJ)
                                || (venuesList.Contains(CommonConstants.PTCN_EN) && item.Content.ToString() == CommonConstants.PTCN)
                                || (venuesList.Contains(CommonConstants.CCTB_EN) && item.Content.ToString() == CommonConstants.CCTB)
                                || (venuesList.Contains("ODM") && item.Content.ToString() == "XBOND")
                                || (venuesList.Count == 0 && item.Content.ToString() == CommonConstants.INNER))
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        bool isVenuesLv_Click = false;
        private void VenuesLv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            
        }

        private void VenuesLv_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isVenuesLv_Click = true;
        }

        private void VenuesLv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isVenuesLv_Click == false)
            {
                return;
            }
            isVenuesLv_Click = false;

            #region 检查内部和指定不能并存
            ListBoxItem innerItem = (ListBoxItem)this.VenuesLv.Items[6];
            if (e.AddedItems.Count > 0)
            {
                ListBoxItem item = (ListBoxItem)e.AddedItems[0];
                if (item.Content.ToString() == "内部")
                {
                    // 检查指定渠道不可选
                    for (int i = 0; i <= 5; i++)
                    {
                        ListBoxItem crossItem = (ListBoxItem)this.VenuesLv.Items[i];
                        if (crossItem.IsSelected)
                            crossItem.IsSelected = false;
                    }
                }
                else
                {
                    if (innerItem.IsSelected)
                        innerItem.IsSelected = false;
                }
            }

            #endregion

            
            if (innerItem.IsSelected)
            {
                if (intentionPanelViewModel != null)
                {
                    intentionPanelViewModel.EditVenues("仅内部");
                }
            }
            else
            {
                string selfSelectVenues = GetSelectItem();

                if (intentionPanelViewModel != null)
                {
                    intentionPanelViewModel.AssignVenues(selfSelectVenues);
                }
            }
        }

        public string GetSelectItem()
        {
            string result = string.Empty;

            foreach (ListBoxItem item in this.VenuesLv.SelectedItems)
            {
                if (item != null)
                {
                    if (item.Content.ToString() == CommonConstants.CNEX)
                    {
                        result += $"SOR-{CommonConstants.CNEX_EN},";
                    }
                    else if (item.Content.ToString() == CommonConstants.TPSC)
                    {
                        result += $"SOR-{CommonConstants.TPSC_EN},";
                    }
                    else if (item.Content.ToString() == CommonConstants.CTTJ)
                    {
                        result += $"SOR-{CommonConstants.CTTJ_EN},";
                    }
                    else if (item.Content.ToString() == CommonConstants.PTCN)
                    {
                        result += $"SOR-{CommonConstants.PTCN_EN},";
                    }
                    else if (item.Content.ToString() == CommonConstants.CCTB)
                    {
                        result += $"SOR-{CommonConstants.CCTB_EN},";
                    }
                    else if (item.Content.ToString() == "XBOND")
                    {
                        result += $"ODM-ODM,";
                    }
                }
            }

            return string.IsNullOrEmpty(result) ? result : result.Substring(0, result.Length - 1);
        }

        private static SolidColorBrush WaveProgressBackground_100 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A2420"));
        private static SolidColorBrush WaveBorderBrush_100 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#136C5E"));
        private static SolidColorBrush WavePorgressBarColor_100 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A2420"));
        private static SolidColorBrush TextColor_100 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFD6"));
        private static SolidColorBrush TextColor_0 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
        private static SolidColorBrush defaultWaveProgressBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
        private static SolidColorBrush defaultWaveBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
        private static SolidColorBrush defaultWavePorgressBarColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282828"));
        private static SolidColorBrush defaultTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        private void progress_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (progress.Value == 0)
            {
                progress.WaveProgressBackground = defaultWaveProgressBackground;
                progress.WaveBorderBrush = defaultWaveBorderBrush;
                progress.WavePorgressBarColor = defaultWavePorgressBarColor;
                progress.TextColor = TextColor_0;
            }
            else if (progress.Value == 100)
            {
                progress.WaveProgressBackground = WaveProgressBackground_100;
                progress.WaveBorderBrush = WaveBorderBrush_100;
                progress.WavePorgressBarColor = WavePorgressBarColor_100;
                progress.TextColor = TextColor_100;
            }
            else
            {
                progress.WaveProgressBackground = defaultWaveProgressBackground;
                progress.WaveBorderBrush = defaultWaveBorderBrush;
                progress.WavePorgressBarColor = defaultWavePorgressBarColor;
                progress.TextColor = defaultTextColor;
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.intentionPanelViewModel.Data != null)
            {
                UpdateIntentionWindow confirmWin = new UpdateIntentionWindow(this.intentionPanelViewModel.Data);
                confirmWin.ShowDialog();
            }
        }


        #region 撤单菜单
        private void MiCancel_Click(object sender, RoutedEventArgs e)
        {
            DeleteOrder();
        }

        private void DeleteOrder()
        {
            if (intentionPanelViewModel.SelectIntentionOrder != null)
            {
                // todo 保价逻辑， 数据在drv中
                DealDelete(intentionPanelViewModel.SelectIntentionOrder.OrderId, true);
            }
            else
            {
                LogHandler.Info("IntentionPanel DeleteOrder 没有取到数据");
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

        private void orderMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

            if (intentionPanelViewModel.SelectIntentionOrder != null)
            {
                bool isFirm = intentionPanelViewModel.SelectIntentionOrder.FirmFlag == "1";

                foreach (MenuItem menuItem in orderMenu.Items)
                {
                    if (menuItem.Name.Equals("miEdit") || menuItem.Name.Equals("miCancel"))
                        menuItem.Visibility = isFirm ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                LogHandler.Info("IntentionPanel orderMenu_ContextMenuOpening 没有取到数据");
            }

        }

        #region 获取数据
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
