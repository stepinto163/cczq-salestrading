using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
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

namespace STC.Broker.Plugin.View.Transaction.UnderLineExec
{
    /// <summary>
    /// UnderLineExecUI.xaml 的交互逻辑
    /// 线下成交单子窗口
    /// </summary>
    public partial class UnderLineExecUI : UserControl
    {
        public OrderDetailPo orderDetailPo { get; set; }
        public int Index = 0;
        public UnderLineExecUI()
        {
            InitializeComponent();
        }

        public void RefreshPrice(string price)
        {
            this.Dispatcher.Invoke(() =>
            {
                tbPrice.Text = price;
            });
        }

        public void RefreshUI()
        {
            if (orderDetailPo != null)
            {
                tbBondCode.Text = OrderHelper.ShowCodeOrName(orderDetailPo);//orderDetailPo.goodsCode;

                tbPrice.Text = orderDetailPo.PriceFormat;

                tbAmount.Text = orderDetailPo.qty + "";

                tbSettle.Text = OrderHelper.GetDisplaySettle(orderDetailPo);//orderDetailPo.SettleDisplay;

                if (!string.IsNullOrWhiteSpace(orderDetailPo.InstitutionName))
                {
                    //DataTable dt = CRMHandler.Instance.SearchContact(orderDetailPo.bankAgentNameA);
                    //string inst = string.Empty;
                    //if (dt.Rows.Count > 0)
                    //{
                    //    DataRow dr = dt.Rows[0];
                    //    inst = dr["instname"] + "";
                    //}
                    tbTraderAInfo.Text = orderDetailPo.TraderAInfo;//orderDetailPo.bankCodeA;//string.Format("{0}({1})", inst, orderDetailPo.bankAgentNameA);
                }
                else
                    tbTraderAInfo.Text = string.Empty;

                tbBrokerAName.Text = orderDetailPo.PartySubName;
                if (LocalEnv.UserInfo.IsMyself(orderDetailPo.PartySubID))
                    tbBrokerA.Style = (Style)FindResource("UnDoBrokerLableStyle");
                else
                    tbBrokerA.Style = (Style)FindResource("OToDoBrokerLableStyle");


                tbSide.Text = orderDetailPo.IsGVN ? "出给" : "买入";
                if (orderDetailPo.IsGVN)
                    tbSide.Foreground = (Brush)FindResource("scbBidPrice");
                else
                    tbSide.Foreground = (Brush)FindResource("scbOfferPrice");

                if (!string.IsNullOrWhiteSpace(orderDetailPo.CounterPartyInstitutionName) || !string.IsNullOrWhiteSpace(orderDetailPo.CounterPartyTraderName))
                    tbTraderBInfo.Text = orderDetailPo.TraderBInfo;//orderDetailPo.bankCodeB;//string.Format("{0}({1})", orderDetailPo.bankCodeB, orderDetailPo.bankAgentNameB);
                else
                    tbTraderBInfo.Text = string.Empty;

                tbBrokerBName.Text = orderDetailPo.CounterPartySubName;
                if (!LocalEnv.UserInfo.IsMyself(orderDetailPo.CounterPartySubID))
                    tbBrokerB.Style = (Style)FindResource("UnDoBrokerLableStyle");
                else
                    tbBrokerB.Style = (Style)FindResource("OToDoBrokerLableStyle");
            }
        }
    }
}
