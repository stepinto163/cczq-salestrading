using netapi.Api;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Base;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Data;
using STC.WorkTable.Plugin.ViewModel.IntentionPanel;
using System;
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

namespace STC.WorkTable.Plugin.View.IntentionPanel
{
    /// <summary>
    /// QuotationPart.xaml 的交互逻辑
    /// </summary>
    public partial class QuotationPart : UserControl
    {
        QuotationPartViewModel viewModel = new QuotationPartViewModel();
        public QuotationPart()
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

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

        private void SetContextSecurityID(QuotationListViewModel Qm, string SID, bool isSetDataContext = false)
        {
            Qm.SecurityID = SID;
            
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
                    // 清空小红点
                    FilterPanel.SetEllipseVisibility(Visibility.Collapsed);
                }
            }
        }
        #endregion

        private bool _isBid = false;
        ContextMenu contextMenu;
        QuotationCloneRowData cloneRowData;
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
        private void SetContextMenu()
        {
            //bool result = GetIdcTraderPermission();
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("OrderContextMenu");
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
                        priceMatchParam.Side = !_isBid ? "SELL" : "BUY";
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
                            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(ComWindowKeyHelper.BID, viewModel.SecurityID)))
                            {
                                SendPriceView sendPriceView = new SendPriceView(priceMatchParam);

                                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceWinKey(ComWindowKeyHelper.BID, viewModel.SecurityID), sendPriceView);
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

                        if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetSendPriceWinKey(ComWindowKeyHelper.OFR, viewModel.SecurityID)))
                        {
                            SendPriceView sendPriceView = new SendPriceView(priceMatchParam);
                            WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetSendPriceWinKey(ComWindowKeyHelper.OFR, viewModel.SecurityID), sendPriceView);
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
                    SendPriceView view = new SendPriceView(priceMatchParam);

                    view.Show();
                }
                else
                {
                    LogHandler.Info("QuotationList OrderMenuItme_Click 没有取到数据");
                }

            }
        }
        #endregion

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
    }
}
