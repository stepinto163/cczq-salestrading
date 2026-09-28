using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Order;
using BDB.Trade.Plugin.ViewModel.Trade;
using BDB.Trade.Plugin.ViewModel.Adapter;
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
using BDB.Trade.Plugin.ViewModel.Data;
using com.gateway.log;
using BDB.Trade.Plugin.Component;
using System.Threading;
using BDB.Trade.Plugin.Handler.DataProcesser;
using System.Windows.Threading;
using STC.Common.Utils;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Base;
using STC.Frame.CustomUI.Component;
using STC.Common.Model.TBook;
using OrderCancelRequest = STC.Common.Model.TBook.OrderCancelRequest;
using ClipboardHelper = BDB.Trade.Plugin.Utils.ClipboardHelper;
using STC.Common.Data;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel;
using CTPriceControler = BDB.Trade.Plugin.ViewModel.Trade.CTPriceControler;
using OrdersHandler = BDB.Trade.Plugin.Handler.OrdersHandler;

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// PriceTableView.xaml 的交互逻辑
    /// </summary>
    public partial class PriceTableView : UserControl
    {
        private PriceTableViewModel _priceTableViewModel;

        public PriceTableView()
        {
            InitializeComponent();

            this.dgData.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.dgData_MouseLeftButtonDown), true);

            this.Loaded += PriceTableView_Loaded;

            this.Unloaded += PriceTableView_Unloaded;


            #region 拖拽

            //this.dgData.MouseMove += DgData_MouseMove;
            //this.dgData.PreviewMouseUp += DgData_PreviewMouseUp;
            //this.dgData.DragOver += DgData_DragOver;
            //this.dgData.DragLeave += DgData_DragLeave;
            //this.dgData.Drop += DgData_Drop;

            #endregion
        }

        #region 拖拽事件

        private void DgData_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !PermisionHandler._IsMoveMessage)
            {
                prevRowIndex = UITools.GetDataGridItemCurrentRowIndex(e.GetPosition, dgData);
                //如果选中区域在第一列，即点击选择框，则不进行
                if (e.GetPosition(this.dgData).X < 30) return;

                if (prevRowIndex < 0) return;
                //dgData.SelectedIndex = prevRowIndex;

                DataRowView selectedEmp = dgData.Items[prevRowIndex] as DataRowView;

                if (selectedEmp == null) return;

                DragDropEffects dragdropeffects = DragDropEffects.Move;
                this._priceTableViewModel.SelectedDataRow = selectedEmp;
                if (DragDrop.DoDragDrop(dgData, selectedEmp, dragdropeffects) != DragDropEffects.None)
                {
                    //dgData.SelectedItem = selectedEmp;
                }
            }
        }    

        private void DgData_DragLeave(object sender, DragEventArgs e)
        {
            this.popup1.IsOpen = false;
        }

        private void DgData_DragOver(object sender, DragEventArgs e)
        {
            int index = UITools.GetDataGridItemCurrentRowIndex(e.GetPosition, dgData);

            for (int i = 0; i < dgData.Items.Count; i++)
            {
                DataGridRow r = dgData.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                if (r != null)
                {
                    if (i != index)
                    {
                        r.BorderBrush = new SolidColorBrush(Colors.Transparent);
                        r.BorderThickness = new Thickness(0, 0, 0, 0);
                    }
                    else
                    {
                        r.BorderBrush = new SolidColorBrush(Color.FromRgb(32, 164, 230));
                        Thickness th = new Thickness(0, 0, 0, 0);

                        if (index > prevRowIndex) th = new Thickness(0, 0, 0, 1);
                        else if (index < prevRowIndex) th = new Thickness(0, 1, 0, 0);
                        r.BorderThickness = th;
                    }
                }
            }

            if (!popup1.IsOpen)
            {
                popup1.IsOpen = true;
            }
            Size popupSize = new Size(popup1.ActualWidth + 110, popup1.ActualHeight + 10);
            Point p = e.GetPosition(this);
            p.X += 10;
            p.Y += 10;
            popup1.PlacementRectangle = new Rect(p, popupSize);

        }

        int prevRowIndex = -1;

        private void DgData_Drop(object sender, DragEventArgs e)
        {

            this.popup1.IsOpen = false;
            int index = UITools.GetDataGridItemCurrentRowIndex(e.GetPosition, dgData);

            if (index < 0 || index == prevRowIndex) return;
            (dgData.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow).BorderThickness = new Thickness(0, 0, 0, 0);


            if (index >= dgData.Items.Count - 1)
            {
                index = dgData.Items.Count - 1;
            }

            if (_priceTableViewModel !=null)
                _priceTableViewModel.Drop(prevRowIndex, index, dgData);
            
            prevRowIndex = -1;

        }

        private void DgData_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        #endregion

        private void PriceTableView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= PriceTableView_Unloaded;

            this.dgData.MouseMove -= DgData_MouseMove;
            this.dgData.PreviewMouseUp -= DgData_PreviewMouseUp;
            this.dgData.DragOver -= DgData_DragOver;
            this.dgData.DragLeave -= DgData_DragLeave;
            this.dgData.Drop -= DgData_Drop;


            LogHelper.Info("PriceTableView_Unloaded");
            if (_priceTableViewModel != null)
            {
                _priceTableViewModel.Dispose();
            }
        }

        public PriceTableView(string groupId)
        {
            InitializeComponent();

            this.dgData.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.dgData_MouseLeftButtonDown), true);


            this.Loaded += PriceTableView_Loaded;
            _priceTableViewModel = new PriceTableViewModel();
            _priceTableViewModel.GroupID = groupId;

            DataContext = _priceTableViewModel;
        }

        public void Init(string groupId, bool needRefreshData)
        {
            if (_priceTableViewModel == null)
            {
                _priceTableViewModel = new PriceTableViewModel();
                DataContext = _priceTableViewModel;
            }
            if (string.IsNullOrEmpty(_priceTableViewModel.GroupID))
            {
                _priceTableViewModel.GroupID = groupId;

                _priceTableViewModel.Init(this, true);//needRefreshData);
            }
        }
        
        ContextMenu contextMenu;

        private void SetContextMenu()
        {
            return;
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("OrderContextMenu");

                bool result = PermisionHandler.Instance.GetIdcTraderPermission();
                if (!result)
                {
                    (contextMenu.Items[0] as MenuItem).Visibility = Visibility.Collapsed;
                    (contextMenu.Items[1] as MenuItem).Visibility = Visibility.Collapsed;
                    (contextMenu.Items[2] as MenuItem).Visibility = Visibility.Collapsed;
                }
            }
        }

        private void PriceTableView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PriceTableView_Loaded;

            SetContextMenu();

            //if (DataContext != null)
            //{
            //    PriceTableViewModel _priceTableViewModel = this.DataContext as PriceTableViewModel;
            //    if (_priceTableViewModel != null)
            //        _priceTableViewModel.Init(this, true);
            //}
        }

        private void LbFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is PriceTableViewModel)
            {
                PriceTableViewModel myTraderViewModel = this.DataContext as PriceTableViewModel;
                //myTraderViewModel.ProcessMyOrdersFilterCondition(this.lbFilter.FilterDic);
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            ComboBox cbb = tb.TemplatedParent as ComboBox;
            if (cbb != null)
                cbb.IsDropDownOpen = !cbb.IsDropDownOpen;
        }

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool result = PermisionHandler.Instance.GetIdcTraderPermission();
            if (!result)
            {
                (contextMenu.Items[0] as MenuItem).Visibility = Visibility.Collapsed;
                (contextMenu.Items[1] as MenuItem).Visibility = Visibility.Collapsed;
                (contextMenu.Items[2] as MenuItem).Visibility = Visibility.Collapsed;
            }
            else
            {
                (contextMenu.Items[0] as MenuItem).Visibility = Visibility.Visible;
                (contextMenu.Items[1] as MenuItem).Visibility = Visibility.Visible;
                (contextMenu.Items[2] as MenuItem).Visibility = Visibility.Visible;

                DataRowView drv = dgData.SelectedItem as DataRowView;
                if (drv != null)
                {
                    string ids = string.Empty;
                    if (_isClickBid)
                        ids = drv[PriceTableViewModel.MY_BID_ORDERID_FIELD] as string;
                    else
                        ids = drv[PriceTableViewModel.MY_OFR_ORDERID_FIELD] as string;

                    ContextMenu contextMenu = sender as ContextMenu;
                    if (string.IsNullOrEmpty(ids))
                    {
                        (contextMenu.Items[1] as MenuItem).IsEnabled = false;
                        (contextMenu.Items[2] as MenuItem).IsEnabled = false;
                    }
                    else
                    {
                        (contextMenu.Items[1] as MenuItem).IsEnabled = true;
                        (contextMenu.Items[2] as MenuItem).IsEnabled = true;
                    }
                }
            }

            
        }

        private void OrderMenuItme_Click(object sender, RoutedEventArgs e)
        {
            //if (contextMenu.Tag != null && contextMenu.Tag is DataGridRow)
            {
                DataGridRow dgRow = (DataGridRow)this.dgData.ItemContainerGenerator.ContainerFromItem( _priceTableViewModel.SelectedItem_Symbol);
                if (dgRow.Item != null && dgRow.Item is DataRowView)
                {
                    DataRowView drv = dgRow.Item as DataRowView;
                    // todo 保价逻辑， 数据在drv中

                    string securityID = CommonUtils.ConvertToString(drv[PriceTableViewModel.SECURITY_ID_FIELD]);
                    string side = _isClickBid ? "1" : "2"; //CommonUtils.ConvertToString(drv[CTPriceControler.SIDE_FIELD]);
                    string price = string.Empty;// CommonUtils.ConvertToString(drv[PriceTableViewModel.ENTRY_PX_FIELD]);
                    // 报量为个人偏好值
                    string qty = string.Empty;//SystemParamHandler.Instance.GetUserSetting().userQty;

                    PriceMatchParam priceMatchParam = new PriceMatchParam();
                    priceMatchParam.Side = side == "1" ? "BUY" : "SELL";
                    priceMatchParam.Price = price;
                    priceMatchParam.UpdataPrice = false;
                    priceMatchParam.CustomFirmAmount = qty;
                    priceMatchParam.IsFirm = true;
                    priceMatchParam.Symbol = securityID;
                    priceMatchParam.NonfirmTime = string.Empty;
                    priceMatchParam.OperationType = "1";
                    //设置默认结算方式
                    priceMatchParam.SettlType = CommonUtils.ConvertToString(drv[PriceTableViewModel.SETTLE_TYPE_FIELD]);
                    //priceMatchParam.NetPrice = GetNetPrice(drv);
                    SendPriceViewLite view = new SendPriceViewLite(priceMatchParam);

                    view.Show();
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string strategyCodeID = string.Empty;
            DataRowView drv = dgData.SelectedItem as DataRowView;
            if (drv != null)
            {
                string ids = string.Empty;
                if (_isClickBid)
                    ids = drv[PriceTableViewModel.MY_BID_ORDERID_FIELD] as string;
                else
                    ids = drv[PriceTableViewModel.MY_OFR_ORDERID_FIELD] as string;

                string[] arr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (arr != null && arr.Length > 0)
                {
                    strategyCodeID = arr[0];
                }

                if (WindowManager.Instance.IsExist($"EditOrder-{strategyCodeID}"))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strategyCodeID))
                    return;

                DataRow dataRow = MyOrderProcesser.Processer.GetOrderById(strategyCodeID);

                if (dataRow == null)
                    return;

                NewOrderSingle singleOrder = GetNewOrderSingle(dataRow);
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
                priceMatchParam.Info1 = singleOrder.Info1;//用于记忆报价有效时长
                priceMatchParam.Venues = singleOrder.Venues; // 渠道
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

                WindowManager.Instance.OpenOnlyOneWindow($"EditOrder-{strategyCodeID}", sendPriceView);
            }
        }


        private NewOrderSingle GetNewOrderSingle(DataRow drv)
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
            string info1 = CommonUtils.ConvertToString(drv[MyOrderProcesser.INFO1_FIELD]);
            NewOrderSingle sendPricePo = new NewOrderSingle();

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
            sendPricePo.OrderQty = (CommonUtils.ConvertToDecimal(qty) - CommonUtils.ConvertToDecimal(exeqty)).ToString();
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
            sendPricePo.Info1 = info1;

            return sendPricePo;
        }

        private void MiCancel_Click(object sender, RoutedEventArgs e)
        {
            bool result = CustomMessageBox.Question("是否撤销该笔报价？", "删除订单");
            if (result == true)
            {
                OrderCancelRequest cancelPo = new OrderCancelRequest();
                //cancelPo.AlgoName = CommonUtils.ConvertToString(this.SelectedItem_MyOrders[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                DataRowView drv = dgData.SelectedItem as DataRowView;
                if (drv != null)
                {
                    string ids = string.Empty;
                    if (_isClickBid)
                        ids = drv[PriceTableViewModel.MY_BID_ORDERID_FIELD] as string;
                    else
                        ids = drv[PriceTableViewModel.MY_OFR_ORDERID_FIELD] as string;

                    string[] arr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (arr != null && arr.Length > 0)
                    {
                        cancelPo.OrderID = arr[0];

                        ThreadPool.QueueUserWorkItem(o =>
                        {

                            try
                            {
                                TBookCancelReplyPo reply = OrdersHandler.Instance.CancelOrder(cancelPo);

                                if (reply != null)
                                {
                                    if (reply.Code == "0000")
                                    {
                                    }
                                    else
                                    {
                                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                        {
                                            CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
                                        }));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHandler.Error(ex);
                            }
                        });
                    }
                }
            }
        }

        public DataRowView GetFocusDataRow()
        {
            return dgData.SelectedItem as DataRowView;
        }

        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow)
            {
                SetContextMenu();

                DataGridRow dgr = sender as DataGridRow;

                DataGrid dg = CommonUtils.GetParentObject<DataGrid>(dgr) as DataGrid;
                dg.Focus();

                dgr.Focus();
                dgr.IsSelected = true;

                if (this.contextMenu != null)
                {
                    this.contextMenu.Tag = dgr;

                    contextMenu.IsOpen = true;
                }
                
            }
        }

        public void OnTabItemChange()
        {
            if (dgData.Items == null || dgData.Items.Count <= 0)
            {
                BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeFocusRow, string.Empty, string.Empty, string.Empty, string.Empty, false);
            }
            else
            {
                DataRowView drv = dgData.SelectedItem as DataRowView;

                if (drv != null)
                {
                    string security = drv[PriceTableViewModel.SECURITY_ID_FIELD] as string;
                    string settle = drv[PriceTableViewModel.SETTLE_TYPE_FIELD] as string;
                    string id = drv[PriceTableViewModel.KEY_FIELD] as string;

                    BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeFocusRow, _priceTableViewModel.GroupID, id, security, settle, false);
                }
            }
        }

        private int _currentSelectedIndex = -1;

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_currentSelectedIndex == dgData.SelectedIndex)
                    return;

                _currentSelectedIndex = dgData.SelectedIndex;

                DataRowView drv = dgData.SelectedItem as DataRowView;

                if (drv != null)
                {
                    string security = drv[PriceTableViewModel.SECURITY_ID_FIELD] as string;
                    string settle = drv[PriceTableViewModel.SETTLE_TYPE_FIELD] as string;
                    string id = drv[PriceTableViewModel.KEY_FIELD] as string;
                    BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeFocusRow, _priceTableViewModel.GroupID, id, security, settle, false);
                }
                else
                {
                    dgData.SelectedIndex = dgData.Items.Count > 0 ? 0 : -1;
                }
            }), DispatcherPriority.Send);
        }

        private void SendSettleChangeSignal()
        {
            if (_currentSelectedIndex == dgData.SelectedIndex)
            {
                DataRowView drv = dgData.SelectedItem as DataRowView;

                if (drv != null)
                {
                    string security = drv[PriceTableViewModel.SECURITY_ID_FIELD] as string;
                    string settle = drv[PriceTableViewModel.SETTLE_TYPE_FIELD] as string;
                    string id = drv[PriceTableViewModel.KEY_FIELD] as string;
                    BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeRowSettleSignal, _priceTableViewModel.GroupID, id, security, settle, false);
                }
            }
        }
        
        private void CbSettle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_priceTableViewModel.IsChangeMainSettle || !_priceTableViewModel.IsClickRowSettle)
                return;
            
            ComboBox comboBox = sender as ComboBox;
            DataRowView dataRowView = comboBox.DataContext as DataRowView;
            if (dataRowView != null && dataRowView.Row != null)
            {
                string changeRowId = dataRowView.Row[PriceViewModel.KEY_FIELD] as string;
                DataRowView drv = dgData.SelectedItem as DataRowView;
                if (drv != null)
                {
                    string focusRowId = drv[PriceViewModel.KEY_FIELD] as string;
                    if (changeRowId == focusRowId)
                    {
                        string selectSettle = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
                        PriceTableViewModel priceTableViewModel = this.DataContext as PriceTableViewModel;
                        priceTableViewModel.OnRowSettleChange(drv.Row, selectSettle);

                        //单行改变结算方式时广播通知
                        SendSettleChangeSignal();

                        FilterAllSettle(selectSettle, focusRowId);
                    }
                }
            }
        }

        private void FilterAllSettle(string selectSettle, string selectId)
        {
            if (_priceTableViewModel != null)
            {
                List<string> allSettle = _priceTableViewModel.GetAllSettle(selectId);
                if (!allSettle.Contains(selectSettle))
                {
                    allSettle.Add(selectSettle);
                }
                BondChangeHandler.Instance.OnMainSettleChange(OperationType.Main, _priceTableViewModel.GroupID, allSettle);

                if (allSettle.Count > 1)
                {
                    DataRowView drv = dgData.SelectedItem as DataRowView;
                    string security = drv[PriceTableViewModel.SECURITY_ID_FIELD] as string;
                    string id = drv[PriceTableViewModel.KEY_FIELD] as string;
                    BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeRowSettle, _priceTableViewModel.GroupID, id, security, selectSettle, false);
                }
            }
            
        }

        private bool _isClickBid = false;
        
        private void BtnAddBond_Click(object sender, RoutedEventArgs e)
        {
            AddBondWindow addBondWind = new AddBondWindow();
            if (_priceTableViewModel != null)
            {
                addBondWind.SetGroupId(_priceTableViewModel.GroupID);
            }

            addBondWind.Owner = Application.Current.MainWindow;
            addBondWind.ShowDialog();
        }

        

        private void ComboBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _priceTableViewModel.IsClickRowSettle = true;
        }

        private void DgData_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell))
                cell = VisualTreeHelper.GetParent(cell);

            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;


            if (targetCell != null)
            {
                string head = targetCell.Column.Header.ToString();

                this.dgData.CurrentColumn = targetCell.Column;

                if (head == "MyBid" || head == "最优Bid")
                    _isClickBid = true;
                else
                    _isClickBid = false;

            }
        }

        private void MiSort_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataGridColumn column in dgData.Columns)
            {
                column.SortDirection = null;
                
            }
            
            if (_priceTableViewModel != null)
                _priceTableViewModel.UserCustomerSort();
        }

        int i = 0;
        bool isDouble = false;

        private void dgData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            if (hit != null && (hit.VisualHit is DataGridCellsPanel || hit.VisualHit is Border))
            {
                var hitElement = hit.VisualHit;
                var dataGridRow = GetDataGridRowParent(hitElement);
                if (dataGridRow != null)
                {
                    dgData.SelectedIndex = _priceTableViewModel.GetRowIndex(dataGridRow);
                }
            }

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

                if (dgData.CurrentColumn != null && dgData.CurrentColumn.Header != null)
                {
                    // 最优行情
                    if (dgData.CurrentColumn.Header.ToString() == "最优Bid")
                    {
                        CopyDoubleClick("1");
                    }
                    else if (dgData.CurrentColumn.Header.ToString() == "最优Ofr")
                    {
                        CopyDoubleClick("2");
                    }
                }
            }
        }

        private void UIElement_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble)
                return;

            if (dgData.CurrentColumn != null && dgData.CurrentColumn.Header != null)
            {
                // 最优行情
                if (dgData.CurrentColumn.Header.ToString() == "最优Bid"
                    || dgData.CurrentColumn.Header.ToString() == "最优Ofr")
                {
                    CopySingleClick();
                }

                // 最新成交
                if (dgData.CurrentColumn.Header.ToString() == "最新成交"
                    || dgData.CurrentColumn.Header.ToString() == "方向趋势")
                {
                    CopyExe();
                }
            }
        }

        private void CopySingleClick(string side = null)
        {
            string html;
            string plainTxt;
            string html_double;
            string plainTxt_double;
            GetDoubleText(out html_double, out plainTxt_double);
            string securityId = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SECURITY_ID_FIELD].ToString();
            string settle = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SETTLE_TYPE_FIELD] as string;
            string filterStr = CTPriceControler.GetAloneSettleFilter(settle);
            BilateralBestPrice bestPrice = PriceHandler.Instance.CalBestPrice(securityId, filterStr, string.Empty);
            string bidPx = "- -";
            if (bestPrice.BidBest != null)
            {
                bidPx = bestPrice.BidBest.bestPx;
            }

            string ofrPx = "- -";
            if (bestPrice.OfrBest != null)
            {
                ofrPx = bestPrice.OfrBest.bestPx;
            }

            string info;
            if (side == "1")
            {
                string s = "GVN";
                info = $",    {s} {securityId} {bidPx}";

                html = $"<p>[{html_double}]{info}</p>";

                plainTxt = $"[{plainTxt_double}]{info}";
            }
            else if (side == "2")
            {
                string s = "TKN";
                info = $",    {s} {securityId} {ofrPx}";

                html = $"<p>[{html_double}]{info}</p>";

                plainTxt = $"[{plainTxt_double}]{info}";
            }
            else
            {
                html = $"<p>{html_double}</p>";

                plainTxt = $"{plainTxt_double}";
            }


            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            if (!string.IsNullOrEmpty(side))
            {
                if (plainTxt.Length > 0)
                    CommonUtils.Prompt2Second("复制", "复制成功");
            }
        }

        private void GetDoubleText(out string html, out string plainTxt)
        {
            string securityId = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SECURITY_ID_FIELD].ToString();
            string symbolName = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SYMBOL_FIELD].ToString();
            string remainingTime = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.REMAING_TIME_FIELD].ToString();

            string settle = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SETTLE_TYPE_FIELD] as string;
            string filterStr = CTPriceControler.GetAloneSettleFilter(settle);
            BilateralBestPrice bestPrice = PriceHandler.Instance.CalBestPrice(securityId, filterStr, string.Empty);



            string bidPx = "- -";
            string bidQtyAndSettle = "- -";
            if (bestPrice.BidBest != null)
            {
                //string bidQty = bestPrice.BidBest.bestQty.ToString();
                bidPx = bestPrice.BidBest.bestPx;
                bidQtyAndSettle = bestPrice.BidBest.text;
            }

            string ofrPx = "- -";
            string ofrQtyAndSettle = "- -";
            if (bestPrice.OfrBest != null)
            {
                //string ofrQty = bestPrice.OfrBest.bestQty.ToString();
                ofrPx = bestPrice.OfrBest.bestPx;
                ofrQtyAndSettle = bestPrice.OfrBest.text;
            }


            string cdcValue = CommonConstants.CDCValueTxt + _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.VALUATION_FIELD].ToString();

            //if (bidQty.EndsWith("K"))
            //{
            //    bidQty = bidQty.Replace("K", "000");
            //}

            //if (ofrQty.EndsWith("K"))
            //{
            //    ofrQty = ofrQty.Replace("K", "000");
            //}

            html = $"{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQtyAndSettle}/{ofrQtyAndSettle}&#9;{cdcValue}";
            plainTxt = $"{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQtyAndSettle}/{ofrQtyAndSettle}    {cdcValue}";
        }

        private void CopyDoubleClick(string side)
        {
            string html;
            string plainTxt;
            string html_double;
            string plainTxt_double;
            GetDoubleText(out html_double, out plainTxt_double);

            string securityId = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SECURITY_ID_FIELD].ToString();
            string symbolName = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SYMBOL_FIELD].ToString();

            string filterStr = CTPriceControler.GetAloneSettleFilter("+1");
            BilateralBestPrice bestPrice = PriceHandler.Instance.CalBestPrice(securityId, filterStr, string.Empty);

            string s = string.Empty;
            string qty = string.Empty;
            string px = string.Empty;
            if (side == "1")//GVN
            {
                s = "GVN";

                if (bestPrice.BidBest != null)
                {
                    qty = bestPrice.BidBest.T1Qty.ToString();


                    if (bestPrice.BidBest.bestPx != null && bestPrice.BidBest.bestPx != "- -" && bestPrice.BidBest.bestPx != "***")
                    {
                        px = bestPrice.BidBest.bestPx;
                    }
                }

            }
            else if (side == "2")//TKN
            {
                s = "TKN";

                if (bestPrice.OfrBest != null)
                {
                    qty = bestPrice.OfrBest.T1Qty.ToString();

                    if (bestPrice.OfrBest.bestPx != null && bestPrice.OfrBest.bestPx != "- -" && bestPrice.OfrBest.bestPx != "***")
                    {
                        px = bestPrice.OfrBest.bestPx;
                    }
                }
            }

            string htmlPx = string.Empty;
            string txtPx = string.Empty;
            if (px.Length > 0)
            {
                htmlPx = $"&#9;{px}";
                txtPx = $" {px}";
            }

            string htmlQty = string.Empty;
            string txtQty = string.Empty;
            if (qty.Length > 0)
            {
                htmlQty = $"&#9;{qty}w";
                txtQty = $" {qty}w";
            }

            html = $"<p>[{html_double}],<br>{s}&#9;{securityId}&#9;{symbolName}{htmlPx}{htmlQty}</p>";

            plainTxt = $"[{plainTxt_double}],\r\n{s} {securityId} {symbolName}{txtPx}{txtQty}";

            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            if (plainTxt.Length > 0)
                CommonUtils.Prompt2Second("复制", "复制成功");
        }


        private void CopyExe()
        {
            if (_priceTableViewModel.SelectedItem_Symbol != null)
            {
                string px = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.LAST_EXEC_PRICE_DISPLAY_FIELD].ToString();
                if (string.IsNullOrEmpty(px) == false)
                {
                    string securityID = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SECURITY_ID_FIELD].ToString();
                    string symbol = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SYMBOL_FIELD].ToString();
                    string source = ComboSourceUtil.Instance.FindEnum(_priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.LAST_EXEC_SOURCE_FIELD].ToString(), "venues_exe");
                    string side = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.LAST_EXEC_PRICE_DISPLAY_SIDE_FIELD].ToString();

                    string time = CommonUtils.ConvertToDateTime(_priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.LAST_EXEC_TIME_FIELD].ToString()).ToString("HH:mm:ss");
                    string remainingTime = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.REMAING_TIME_FIELD].ToString();


                    StringBuilder html = new StringBuilder();
                    StringBuilder plainTxt = new StringBuilder();

                    html.Append($"<p>{remainingTime}&#9;{securityID}&#9;{symbol}&#9;{px}&#9;{side}&#9;{source}&#9;{time}</p>");

                    plainTxt.Append($"{remainingTime} {securityID} {symbol} {px} {side} {source} {time}");

                    //复制
                    ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());
                }
                else
                {
                    //复制
                    ClipboardHelper.CopyToClipboard("暂无成交", "暂无成交");
                }
            }
        }

        private DataGridRow GetDataGridRowParent(DependencyObject obj)
        {
            while (obj != null)
            {
                if (obj is DataGridRow)
                    return obj as DataGridRow;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        private void CopySecurityIDMenu_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = dgData.SelectedItem as DataRowView;
            if (drv != null)
            {
                string securityId = drv[PriceTableViewModel.SECURITY_ID_FIELD] as string;
                ClipboardHelper.CopyToClipboard(securityId, securityId);
            }
        }

        private void CopySymbolNameMenu_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = dgData.SelectedItem as DataRowView;
            if (drv != null)
            {
                string symbol = drv[PriceTableViewModel.SYMBOL_FIELD] as string;
                ClipboardHelper.CopyToClipboard(symbol, symbol);
            }
        }

        private void CopySingleClickMenu_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.CurrentColumn.Header.ToString() == "最优Bid"
                || dgData.CurrentColumn.Header.ToString() == "最优Ofr")
            {
                CopySingleClick();
            }
        }

        private void CopyDoubleClick_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.CurrentColumn.Header.ToString() == "最优Bid")
            {
                CopyDoubleClick("1");
            }
            else if (dgData.CurrentColumn.Header.ToString() == "最优Ofr")
            {
                CopyDoubleClick("2");
            }
        }

        private void CopyCtrlSingleClickMenu_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.CurrentColumn.Header.ToString() == "最优Bid")
            {
                CopySingleClick("1");
            }
            else if (dgData.CurrentColumn.Header.ToString() == "最优Ofr")
            {
                CopySingleClick("2");
            }
        }

        private void dgMenu_Opened(object sender, RoutedEventArgs e)
        {
            bool result = PermisionHandler.Instance.GetIdcTraderPermission();
            if (!result)
            {
                (dgMenu.Items[0] as MenuItem).Visibility = Visibility.Collapsed;
                (dgMenu.Items[1] as MenuItem).Visibility = Visibility.Collapsed;
                (dgMenu.Items[2] as MenuItem).Visibility = Visibility.Collapsed;
            }
            else
            {
                (dgMenu.Items[0] as MenuItem).Visibility = Visibility.Visible;
                (dgMenu.Items[1] as MenuItem).Visibility = Visibility.Visible;
                (dgMenu.Items[2] as MenuItem).Visibility = Visibility.Visible;

                DataRowView drv = dgData.SelectedItem as DataRowView;
                if (drv != null)
                {
                    string ids = string.Empty;
                    if (_isClickBid)
                        ids = drv[PriceTableViewModel.MY_BID_ORDERID_FIELD] as string;
                    else
                        ids = drv[PriceTableViewModel.MY_OFR_ORDERID_FIELD] as string;

                    if (string.IsNullOrEmpty(ids))
                    {
                        (dgMenu.Items[1] as MenuItem).IsEnabled = false;
                        (dgMenu.Items[2] as MenuItem).IsEnabled = false;
                    }
                    else
                    {
                        (dgMenu.Items[1] as MenuItem).IsEnabled = true;
                        (dgMenu.Items[2] as MenuItem).IsEnabled = true;
                    }
                }
            }

            if (this.dgData.CurrentColumn != null
                && (this.dgData.CurrentColumn.Header.ToString() == "最优Ofr"
                || this.dgData.CurrentColumn.Header.ToString() == "最优Bid"))
            {
                (dgMenu.Items[6] as MenuItem).Visibility = Visibility.Visible;
                (dgMenu.Items[7] as MenuItem).Visibility = Visibility.Visible;
            }
            else
            {
                (dgMenu.Items[6] as MenuItem).Visibility = Visibility.Collapsed;
                (dgMenu.Items[7] as MenuItem).Visibility = Visibility.Collapsed;
            }
        }
        private void dgData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string securityid = _priceTableViewModel.SelectedItem_Symbol[PriceTableViewModel.SECURITY_ID_FIELD] + "";
            CommonUtils.OpenSmallBondWin(securityid);
        }
    }

    public enum TableSort
    {
        RemaingTimeAsc, RemaingTimeDesc, BondCodeAsc, BondCodeDesc, ExecNumAsc, ExecNumDesc
    }
}
