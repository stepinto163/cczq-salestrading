using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.ViewModel;
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
using System.Windows.Shapes;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// ConvertFirmOrNonFirmView.xaml 的交互逻辑
    /// </summary>
    public partial class ConvertFirmOrNonFirmView : STC.Common.View.Component.WindowBase
    {
        Brush BNC_ColorBrush;
        Brush BCO_ColorBrush;
        ConvertFirmOrNonFirmViewModel viewModel;

        public ConvertFirmOrNonFirmView()
        {
            InitializeComponent();
            viewModel = new ConvertFirmOrNonFirmViewModel(this);
            this.DataContext = viewModel;

            var converter = new System.Windows.Media.BrushConverter();

            BNC_ColorBrush = (Brush)converter.ConvertFromString("#E63034");
            BCO_ColorBrush = (Brush)converter.ConvertFromString("#2E58DB");
        }

        /// <summary>
        /// 界面初始化设置
        /// </summary>
        /// <param name="toNonfirmFlag">0: tofirm; 1: toNonFirm</param>
        /// <param name="newOrderSingleDic"></param>
        public void Init(string toNonfirmFlag, Dictionary<string, NewOrderSingle> newOrderSingleDic)
        {
            // 转Nonfirm
            if (toNonfirmFlag == "1")
            {
                this.Title = "批量转Nonfirm";
                viewModel.Header = "批量转Nonfirm";
                this.viewModel.FirmChecked = true;
                this.firmFlagChk.Content = "nonfirm";

                viewModel.IsToFirm = Visibility;
            }
            // 转Firm
            else
            {
                this.Title = "批量转Firm";
                viewModel.Header = "批量转Firm";
                this.viewModel.FirmChecked = false;
                this.firmFlagChk.Content = "firm";

                viewModel.IsToFirm = Visibility.Visible;
            }

            if (newOrderSingleDic != null && newOrderSingleDic.Count > 0)
            {

                bool hasBNC = false; // 有利率债
                bool hasBCO = false; // 有信用债

                foreach (NewOrderSingle item in newOrderSingleDic.Values)
                {
                    string securityID = item.SecurityID;

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
                    bncChannelCheckUI.Init("BNC", BNC_ColorBrush, toNonfirmFlag);
                    
                    Interest_Part.Children.Add(bncChannelCheckUI);
                    Grid.SetColumn(bncChannelCheckUI, 2);
                }
                if (hasBCO)
                {
                    viewModel.IsShowCreditDebt = Visibility.Visible;
                    ChannelCheckUI bcoChannelCheckUI = new ChannelCheckUI();
                    bcoChannelCheckUI.CheckValueChanged += BcoChannelCheckUI_CheckValueChanged;
                    bcoChannelCheckUI.Init("BCO", BCO_ColorBrush, toNonfirmFlag);
                    
                    Credit_Part.Children.Add(bcoChannelCheckUI);
                    Grid.SetColumn(bcoChannelCheckUI, 2);
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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = regex = new Regex("^[0-9]+$");

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 120;
            int min = 1;

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;

            if (selectTxt.Length == text.Length)
                return;

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);

            decimal d = 0;
            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if(this.viewModel != null)
            {
                this.viewModel.CancelOpt();
                this.viewModel.Dispose();
            }
        }

        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.viewModel != null && this.viewModel.FinishOpt() == false)
            {
                string msg = $"确认要取消{this.Title}吗?";
                bool flag = Common.View.Component.CustomMessageBox.Question(msg, this.Title);
                if (!flag)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
