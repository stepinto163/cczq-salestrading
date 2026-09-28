using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    /// <summary>
    /// BridgeInfoUI.xaml 的交互逻辑
    /// 单个桥信息面板，可以往桥里加订单
    /// </summary>
    public partial class BridgeInfoUI : UserControl
    {
        public UserBridge CurrentBridge;
        public BridgeInfoUI()
        {
            InitializeComponent();
        }

        public void Init(UserBridge bridge)
        {
            CurrentBridge = bridge;
            if (bridge != null)
            {
                //桥
                tbInst.Text = bridge.bankInstitutionname;
                tbContact.Text = bridge.bankTradername;
                tbContactType.Text = bridge.contactType;
                //计费
                tbCost.Text = bridge.feeInstitutionname;
                //发给，备注，渠道
                tbSendTo.Text = bridge.sendto;
                tbRemarks.Text = bridge.remark;
                if (!string.IsNullOrEmpty(bridge.channel))
                {
                    //tbWay.Text = bridge.channel.Equals("1") ? "对话" : bridge.channel.Equals("2") ? "请求" : string.Empty;
                    tbWay.Text = EChannel.对话.EqualsToValue(bridge.channel) ? EChannel.对话.ToString()
                        : EChannel.请求.EqualsToValue(bridge.channel) ? EChannel.请求.ToString() : string.Empty;
                }

                //if (LocalEnv.ConfInfo.IsShowReserveButton)
                //{
                //    xt_gd.ColumnDefinitions[8].Width = new GridLength(100);
                //    btnDrawUp.Visibility = Visibility.Visible;
                //}
                //else {
                    xt_gd.ColumnDefinitions[8].Width = new GridLength(50);
                    btnDrawUp.Visibility = Visibility.Collapsed;
                //}
            }
        }

        #region 
        /// <summary>
        /// 预订桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDrawUp_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbOrderId.Text))
                OnReserveBridge?.Invoke(tbOrderId.Text);

            tbOrderId.Text = string.Empty;

        }
        public event Action<string> OnReserveBridge;
        public event Action<string> OnAddBridge;
        private void btnAddBridge_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbOrderId.Text))
                OnAddBridge?.Invoke(tbOrderId.Text);

            tbOrderId.Text = string.Empty;
        }

        private void tbOrderId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void tbOrderId_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void tbOrderId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAddBridge_Click(null, null);
            }
        }

        #endregion
    }
}
