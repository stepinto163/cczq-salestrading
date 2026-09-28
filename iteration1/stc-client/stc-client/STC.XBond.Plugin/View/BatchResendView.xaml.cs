using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.ViewModel;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// BatchResendView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchResendView : WindowBase
    {
		Brush BNC_ColorBrush;
        Brush BCO_ColorBrush;
        BatchResendViewModel viewModel;
		
        public BatchResendView()
        {
            InitializeComponent();
            viewModel = new BatchResendViewModel(this);
            this.DataContext = viewModel;

            var converter = new System.Windows.Media.BrushConverter();

            BNC_ColorBrush = (Brush)converter.ConvertFromString("#E63034");
            BCO_ColorBrush = (Brush)converter.ConvertFromString("#2E58DB");
        }

        /// <summary>
        /// 界面初始化设置
        /// </summary>
        /// <param name="toNonfirmFlag">0: tofirm; 1: toNonFirm</param>
        /// <param name="newSingleOrderDic"></param>
        public void Init(Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic)
        {

            this.Title = "批量恢复报价";

            if (newOrderSingleDic != null && newOrderSingleDic.Count > 0)
            {

                bool hasBNC = false; // 有利率债
                bool hasBCO = false; // 有信用债

                foreach (NewOrderSingle item in newOrderSingleDic.Values)
                {
                    if (CommonUtils.IsRate(item.SecurityType))
                    {
                        hasBNC = true;
                    }
                    else
                    {
                        hasBCO = true;
                    }
                }
                // 选择订单数量
                viewModel.SelectedCount = newOrderSingleDic.Count;

                // 设置显示利率债， 信用债
                if (hasBNC)
                {
                    viewModel.IsShowInterestRateDebt = Visibility.Visible;
                    ChannelCheckUI bncChannelCheckUI = new ChannelCheckUI();
                    bncChannelCheckUI.CheckValueChanged += bncChannelCheckUI_CheckValueChanged;
                    bncChannelCheckUI.Init("BNC", BNC_ColorBrush, string.Empty);
                    
                    Interest_Part.Children.Add(bncChannelCheckUI);
                    Grid.SetColumn(bncChannelCheckUI, 1);
                }
                if (hasBCO)
                {
                    viewModel.IsShowCreditDebt = Visibility.Visible;
                    ChannelCheckUI bcoChannelCheckUI = new ChannelCheckUI();
                    bcoChannelCheckUI.CheckValueChanged += BcoChannelCheckUI_CheckValueChanged;
                    bcoChannelCheckUI.Init("BCO", BCO_ColorBrush, string.Empty);

                    Credit_Part.Children.Add(bcoChannelCheckUI);
                    Grid.SetColumn(bcoChannelCheckUI, 1);
                }

                this.viewModel.newOrderSingleDic = newOrderSingleDic;
            }
        }

        /// <summary>
        /// 信用债设置修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BcoChannelCheckUI_CheckValueChanged(object sender, RoutedEventArgs e)
        {
            ChannelCheckUI channelCheckUI = sender as ChannelCheckUI;
            ChannelCheckParam checkValue = channelCheckUI.CheckValue;
            viewModel.SetBCOSetting(checkValue);
        }

        /// <summary>
        /// 利率债设置修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bncChannelCheckUI_CheckValueChanged(object sender, RoutedEventArgs e)
        {
            ChannelCheckUI channelCheckUI = sender as ChannelCheckUI;
            ChannelCheckParam checkValue = channelCheckUI.CheckValue;
            viewModel.SetBNCSetting(checkValue);
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            this.viewModel.CancelOpt();
        }

        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.viewModel.FinishOpt() == false)
            {
                bool flag = Common.View.Component.CustomMessageBox.Question("确认要取消恢复报价吗?", "取消恢复报价");
                if (!flag)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
