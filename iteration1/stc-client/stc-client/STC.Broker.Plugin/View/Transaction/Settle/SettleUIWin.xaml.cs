
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Transaction.Settle
{
    /// <summary>
    /// SettleUIWin.xaml 的交互逻辑
    /// </summary>
    public partial class SettleUIWin : Window
    {
        public SettleUIWin()
        {
            InitializeComponent();
            //this.Icon = LocalEnv.ServerInfo.AppImage;
            settleOfr.OnSettleChange += SettleOfr_OnSettleChange;
            settleBid.OnSettleChange += SettleOfr_OnSettleChange;
            
        }

        private void SettleOfr_OnSettleChange(bool idBid, string settle)
        {
            SettlementPo bidSettle = settleBid.GetSettle();
            SettlementPo ofrSettle = settleOfr.GetSettle();

            if (!string.IsNullOrEmpty(bidSettle.Settle) && !string.IsNullOrEmpty(ofrSettle.Settle) && !bidSettle.Settle.Equals(ofrSettle.Settle))
            {
                cbExchange.IsChecked = new bool?(false);
                cbExchange.IsEnabled = false;
            }
            else
                cbExchange.IsEnabled = true;
        }

        public void SetValue(TransactionOrdermModel orderDetailPo)
        {
            string bidSettle = "+1";
            string ofrSettle = "+1";
            string remarks = orderDetailPo.Remarks;
            orderDetailPo.Remarks = string.Empty;
            string temSettle = TransOrderHelper.Instance.IsDealHistDay(orderDetailPo) ? TransOrderHelper.Instance.GetHistoryDisplaySettle(orderDetailPo) : TransOrderHelper.Instance.GetDisplaySettleForMain(orderDetailPo);
            orderDetailPo.Remarks = remarks;
            string[] arrSettle = temSettle.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (arrSettle != null)
            {
                if (arrSettle.Length > 1)
                {
                    ofrSettle = arrSettle[0];
                    bidSettle = arrSettle[1];
                }
                else if (arrSettle.Length == 1)
                    bidSettle = ofrSettle = arrSettle[0];
            }

            SettlementPo ofrSettlePo = new SettlementPo()
            {
                IsBid = false,
                Settle = ofrSettle,
                SettlementDate = ChangeDateStr(orderDetailPo.Settlementdate2),
                TradeDate = ChangeDateStr(orderDetailPo.Dealdate2)
            };

            SettlementPo bidSettlePo = new SettlementPo()
            {
                IsBid = true,
                Settle = bidSettle,
                SettlementDate = ChangeDateStr(orderDetailPo.Settlementdate),
                TradeDate = ChangeDateStr(orderDetailPo.Dealdate)
            };
            bool isHistory = TransOrderHelper.Instance.IsDealHistDay(orderDetailPo) ? true : false;
            settleOfr.Init(ofrSettlePo, isHistory);
            settleBid.Init(bidSettlePo, isHistory);

            if (!string.IsNullOrEmpty(remarks))
            {
                cbExchange.IsEnabled = true;
                cbExchange.IsChecked = new bool?(true);
            }
            else
            {
                if (!bidSettle.Equals(ofrSettle))
                {
                    cbExchange.IsEnabled = false;
                }
                else
                {
                    cbExchange.IsEnabled = true;
                    cbExchange.IsChecked = new bool?(false);
                }
            }
        }

        private string ChangeDateStr(string date)
        {
            if (!string.IsNullOrEmpty(date))
            {
                if (date.Length > 7)
                    date = date.Insert(4, "-").Insert(7, "-");
            }
            return date;
        }

        public event Action<object, SettlementPo> OnSettyleChange;
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SettlementPo result = new SettlementPo();

            SettlementPo bidSettle = settleBid.GetSettle();
            SettlementPo ofrSettle = settleOfr.GetSettle();

            result.Settle = ofrSettle.Settle;     
            result.TradeDate = ofrSettle.TradeDate;
            result.SettlementDate = ofrSettle.SettlementDate;

            result.BidSettle = bidSettle.Settle;
            result.BidTradeDate = bidSettle.BidTradeDate;
            result.BidSettlementDate = bidSettle.BidSettlementDate;

            if (cbExchange.IsChecked != null && cbExchange.IsChecked.HasValue && cbExchange.IsChecked.Value)
                result.Remarks = cbExchange.Content.ToString();
            else
                result.Remarks = string.Empty;
            
            OnSettyleChange?.Invoke(this, result);

            this.Close();
        }

        private void SettleUIWin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnOK_Click(null, null);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
