using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Order;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using com.gateway.log;
using STC.Common.Handler.QQ;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;
using ClipboardHelper = BDB.Trade.Plugin.Utils.ClipboardHelper;

namespace BDB.Trade.Plugin.View.Trade.Card
{
    /// <summary>
    /// BrokerPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class BrokerPriceView : UserControl
    {
        public BrokerPriceView()
        {
            InitializeComponent();

            //this.DataContextChanged += BrokerPriceView_DataContextChanged;
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.BidPartToolTip.DataContext = this.DataContext;
            this.BidPartToolTip1.DataContext = this.DataContext;
            this.OfrPartToolTip.DataContext = this.DataContext;
        }

        private void BrokerPriceView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is BrokerPriceViewModel)
            {
                (e.OldValue as BrokerPriceViewModel).PropertyChanged -= BrokerPriceView_PropertyChanged;
            }

            if (e.NewValue != null && e.NewValue is BrokerPriceViewModel)
            {
                (e.NewValue as BrokerPriceViewModel).PropertyChanged -= BrokerPriceView_PropertyChanged;
                (e.NewValue as BrokerPriceViewModel).PropertyChanged += BrokerPriceView_PropertyChanged;
            }
        }

        private string _LastBid;
        private string _LastOfr;

        private void BrokerPriceView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            BrokerPriceViewModel vm = sender as BrokerPriceViewModel;
            if (e.PropertyName == "Bid")
            {
                if (_LastBid != null && CommonUtils.IsNumeric(vm.Bid) && _LastBid != vm.Bid)
                {
                    AnimationHelper.Instance.StartAnimation(this, "bdBid", string.Empty, true);
                    _LastBid = vm.Bid;
                }
                
            }
            else if (e.PropertyName == "Ofr")
            {
                if (_LastOfr != null && CommonUtils.IsNumeric(vm.Ofr) && _LastOfr != vm.Ofr)
                {
                    AnimationHelper.Instance.StartAnimation(this, "bdOfr", string.Empty, true);
                    _LastOfr = vm.Ofr;
                }
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {

        }


        private void BdBid_MouseEnter(object sender, MouseEventArgs e)
        {
            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
            if (brokerPriceViewModel != null && brokerPriceViewModel.IsBidPermission)
            {
                brokerPriceViewModel.BidBgBrush = (Brush)this.FindResource("Bid.Selected.BorderBrush-50%");
                bdBid.BorderThickness = new Thickness(1);
                bdBid.BorderBrush = (Brush)this.FindResource("Bid.Selected.BorderBrush2");
            }
        }

        private void BdOfr_MouseEnter(object sender, MouseEventArgs e)
        {
            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
            if (brokerPriceViewModel != null && brokerPriceViewModel.IsOfrPermission)
            {
                brokerPriceViewModel.OfrBgBrush = (Brush)this.FindResource("Ofr.Selected.BorderBrush-50%");
                bdOfr.BorderThickness = new Thickness(1);
                bdOfr.BorderBrush = (Brush)this.FindResource("Ofr.Selected.BorderBrush");
            }
        }

        private void BdBid_MouseLeave(object sender, MouseEventArgs e)
        {
            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
            if (brokerPriceViewModel != null && brokerPriceViewModel.IsBidPermission)
            {
                brokerPriceViewModel.BidBgBrush = (Brush)this.FindResource("Bid.Selected.BorderBrush-30%");
                bdBid.BorderThickness = new Thickness(0);
            }
        }

        private void BdOfr_MouseLeave(object sender, MouseEventArgs e)
        {
            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
            if (brokerPriceViewModel != null && brokerPriceViewModel.IsOfrPermission)
            {
                brokerPriceViewModel.OfrBgBrush = (Brush)this.FindResource("Ofr.Selected.BorderBrush-30%");
                bdOfr.BorderThickness = new Thickness(0);
            }
        }

        int i_bid = 0;
        bool isDouble_bid = false;
        bool isCtrlBid = false;
        private void BdBid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isCtrlBid = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            i_bid += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, System.Windows.Forms.SystemInformation.DoubleClickTime);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i_bid = 0; UIElement_Bid_Click(sender, e); isDouble_bid = false; isCtrlBid = false; };
            timer.IsEnabled = true;
            if (i_bid % 2 == 0)
            {
                timer.IsEnabled = false;
                i_bid = 0;
                isDouble_bid = true;

                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                //if (e.ClickCount == 2 && )
                if (brokerPriceViewModel.IsBidPermission)
                {
                    DoubleClick(true);
                }


                bool isPermission = PermisionHandler.Instance.CheckBrokerPermissionByCN(brokerPriceViewModel.BrokerCN);
                if (isPermission)
                {
                    brokerPriceViewModel.CopyPointOrderData("1");
                }
            }
        }

        private void UIElement_Bid_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble_bid)
                return;

            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                if (isCtrlBid)
                {
                    brokerPriceViewModel.CopyOrderData("1", true);
                }
                else
                {
                    brokerPriceViewModel.CopyOrderData();
                }
            }
        }

        int i_ofr = 0;
        bool isDouble_ofr = false;
        bool isCtrlOfr = false;
        private void BdOfr_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isCtrlOfr = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            i_ofr += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, System.Windows.Forms.SystemInformation.DoubleClickTime);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i_ofr = 0; UIElement_Ofr_Click(sender, e); isDouble_ofr = false; isCtrlOfr = false; };
            timer.IsEnabled = true;
            if (i_ofr % 2 == 0)
            {
                timer.IsEnabled = false;
                i_ofr = 0;
                isDouble_ofr = true;

                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                //if (e.ClickCount == 2 && brokerPriceViewModel.IsOfrPermission)
                if (brokerPriceViewModel.IsOfrPermission)
                {
                    DoubleClick(false);
                }
                bool isPermission = PermisionHandler.Instance.CheckBrokerPermissionByCN(brokerPriceViewModel.BrokerCN);
                if (isPermission)
                {
                    brokerPriceViewModel.CopyPointOrderData("2");
                }
            }
        }

        private void UIElement_Ofr_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble_ofr)
                return;

            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                if (isCtrlOfr)
                {
                    brokerPriceViewModel.CopyOrderData("2", true);
                }
                else
                {
                    brokerPriceViewModel.CopyOrderData();
                }
            }
        }


        

        public async void CopyPointOrderData2(string side)
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();

            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;

            string s = string.Empty;
            string qty = string.Empty;
            string px = string.Empty;
            string settle = string.Empty;

            string securityId = brokerPriceViewModel.Security;
            string symbolName = string.Empty;//brokerPriceViewModel.Security;

            if (side == "1")//GVN
            {
                s = "GVN";

                qty = brokerPriceViewModel.BidAmount == "--" ? string.Empty : brokerPriceViewModel.BidAmount;
                px = brokerPriceViewModel.Bid == "--" ? string.Empty : brokerPriceViewModel.Bid;
                settle = brokerPriceViewModel.BidSettleTip;

            }
            else if (side == "2")//TKN
            {
                s = "TKN";

                qty = brokerPriceViewModel.OfrAmount == "--" ? string.Empty : brokerPriceViewModel.OfrAmount;
                px = brokerPriceViewModel.Ofr == "--" ? string.Empty : brokerPriceViewModel.Ofr;
                settle = brokerPriceViewModel.OfrSettleTip;
            }

            //html.Append($"<p><span style=\"color: {color}\">{s}</span>&#9;{securityId}&#9;{symbolName}&#9;{px}&#9;{qty}&#9;{settle}</p>");

            //html.Append($"<p>{s}&#9;{securityId}&#9;{symbolName}&#9;{px}&#9;{settle}&#9;{qty}</p>");

            //plainTxt.Append($"{s} {securityId} {symbolName} {px} {settle} {qty}");


            string htmlSettle = string.Empty;
            string txtSettle = string.Empty;
            if (settle.Length > 0)
            {
                htmlSettle = $"&#9;{settle}";
                txtSettle = $" {settle}";
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
                htmlQty = $"&#9;{qty}";
                txtQty = $" {qty}";
            }

            html.Append($"<p>{s}&#9;{securityId}{htmlPx}{htmlSettle}{htmlQty}</p>");

            plainTxt.Append($"{s} {securityId}{txtPx}{txtSettle}{txtQty}");

            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            if (plainTxt.Length > 0)
            {
                string qq = ResulfApiHelper.GetQQByBrokerCN(brokerPriceViewModel.BrokerCN);

                if (string.IsNullOrEmpty(qq))
                    CommonUtils.Prompt2Second("复制", "复制成功");
                else
                {
                    //await ResulfApiHelper.SendMessageToQQ(plainTxt.ToString());
                    await ResulfApiHelper.ManualSendMessageToQQRequest(qq, plainTxt.ToString());
                }
            }
        }

        private void DoubleClick(bool isBid)
        {
            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
            string source = PermisionHandler.Instance.GetBrokerByIndex(brokerPriceViewModel.BrokerIndex+1);
            //source = ComboSourceUtil.Instance.FindEnum(source, "tovenues");
            string security = brokerPriceViewModel.Security;
            string price = string.Empty;
            string qty = string.Empty;
            string settle = string.Empty;
            string orderinfotip = string.Empty;
            if (isBid)
            {
                price = brokerPriceViewModel.Bid;
                qty = brokerPriceViewModel.BidAmount;

                if (!string.IsNullOrEmpty(brokerPriceViewModel.BidSettleType))
                    settle = brokerPriceViewModel.BidSettleType;
                else
                    settle = brokerPriceViewModel.SettleType;

                orderinfotip = brokerPriceViewModel.BidCornerMarketText;
            }
            else
            {
                price = brokerPriceViewModel.Ofr;
                qty = brokerPriceViewModel.OfrAmount;

                if (!string.IsNullOrEmpty(brokerPriceViewModel.OfrSettleType))
                    settle = brokerPriceViewModel.OfrSettleType;
                else
                    settle = brokerPriceViewModel.SettleType;

                orderinfotip = brokerPriceViewModel.OfrCornerMarketText;
            }

            if (!string.IsNullOrEmpty(price))
            {
                decimal tmpprice = 0;
                if (!decimal.TryParse(price, out tmpprice))
                {
                    return;
                }

                //SendPointPriceViewLite sendPointPriceViewLite = new SendPointPriceViewLite(security, isBid, price, qty, settle, string.Empty, source, string.Empty, true, orderinfotip);
                //sendPointPriceViewLite.Show();
            }
        }

        private void Bid_CopySecurityID_mi_Click(object sender, RoutedEventArgs e)
        {
            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
            string securityId = brokerPriceViewModel.Security;
            ClipboardHelper.CopyToClipboard(securityId, securityId);
        }

        private void Bid_CopySymbolName_mi_Click(object sender, RoutedEventArgs e)
        {
            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
            string symbolName = string.Empty;//brokerPriceViewModel.SymbolName;
            if (this.Parent != null && this.Parent is FrameworkElement)
            {
                FrameworkElement fe = this.Parent as FrameworkElement;
                if (fe != null && fe.DataContext != null && fe.DataContext is PriceAndOrderViewModel)
                {
                    PriceAndOrderViewModel priceAndOrderViewModel = fe.DataContext as PriceAndOrderViewModel;
                    symbolName = priceAndOrderViewModel.PriceModel.SymbolName;
                }
            }
            ClipboardHelper.CopyToClipboard(symbolName, symbolName);
        }

        private void Bid_CopySingleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyOrderData();
            }
        }

        private void Bid_CopyDoubleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyPointOrderData("1");
            }
        }

        private void Bid_CopyCtrlSingleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyOrderData("1", true);
            }
        }

        private void Ofr_CopySecurityID_mi_Click(object sender, RoutedEventArgs e)
        {
            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
            string securityId = brokerPriceViewModel.Security;
            ClipboardHelper.CopyToClipboard(securityId, securityId);
        }

        private void Ofr_CopySymbolName_mi_Click(object sender, RoutedEventArgs e)
        {
            BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
            string symbolName = string.Empty;//brokerPriceViewModel.SymbolName;
            if (this.Parent != null && this.Parent is FrameworkElement)
            {
                FrameworkElement fe = this.Parent as FrameworkElement;
                if (fe != null && fe.DataContext != null && fe.DataContext is PriceAndOrderViewModel)
                {
                    PriceAndOrderViewModel priceAndOrderViewModel = fe.DataContext as PriceAndOrderViewModel;
                    symbolName = priceAndOrderViewModel.PriceModel.SymbolName;
                }
            }
            ClipboardHelper.CopyToClipboard(symbolName, symbolName);
        }

        private void Ofr_CopySingleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyOrderData();
            }
        }

        private void Ofr_CopyDoubleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyPointOrderData("2");
            }
        }

        private void Ofr_CopyCtrlSingleClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyOrderData("2", true);
            }
        }

        public event Action<string> OnClickBroker;
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            OnClickBroker?.Invoke(textBlock.Text);
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            if (textBlock != null)
            {
                textBlock.Cursor = Cursors.Hand; // 更改为指示的光标
            }
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            if (textBlock != null)
            {
                textBlock.Cursor = Cursors.Arrow; // 恢复默认箭头光标
            }
        }

        private void Bid_PointOrderClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyPointOrderData("1");
            }
        }

        private void Bid_JoinOrderClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyPointOrderData("1", "1");
            }
        }

        private void Bid_ConsiderationClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyPointOrderData("1", "2");
            }
        }

        private void Ofr_PointOrderClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyPointOrderData("2");
            }
        }

        private void Ofr_JoinOrderClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyPointOrderData("2", "1");
            }
        }

        private void Ofr_ConsiderationClick_mi_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BrokerPriceViewModel)
            {
                BrokerPriceViewModel brokerPriceViewModel = this.DataContext as BrokerPriceViewModel;
                brokerPriceViewModel.CopyPointOrderData("2", "2");
            }
        }

        private void ToolTip_Opened(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is ToolTip)
            {
                ToolTip cornerToolTip = sender as ToolTip;
                var temp = cornerToolTip.Content;
                cornerToolTip.Content = null;
                cornerToolTip.Content = temp;
            }
        }
    }
}
