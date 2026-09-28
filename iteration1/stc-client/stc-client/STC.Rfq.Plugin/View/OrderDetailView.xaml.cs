using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.Rfq.Plugin.View.Component;
using STC.Rfq.Plugin.ViewModel;

namespace STC.Rfq.Plugin.View
{
    /// <summary>
    /// OrderDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class OrderDetailView : Window
    {
        public OrderDetailView()
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;

                return;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = tb.Tag.ToString();
            decimal max = 0;
            decimal min = 0;
            Regex regex = null;
            int digit = 2;

            
            if (tag == "bidBaseQty" || tag == "ofrBaseQty")
            {
                // 回复量 输入范围：[0.01, 9999.90]
                regex = new Regex("^[0-9.-]+$");
                max = 9999.90m;
                min = 0.01m;
            }

            if (tag == "maxQtyBid" || tag == "maxQtyOfr")
            {
                // 预设回量上限 输入范围：[0.01, 9999.90]
                regex = new Regex("^[0-9.-]+$");
                max = 9999.90m;
                min = 0.01m;
            }
            else if (tag == "bidBaseReSendValue" || tag == "ofrBaseReSendValue")
            {
                // 重发阈值 输入范围：[0.01, 99.99] 
                regex = new Regex("^[0-9.-]+$");
                max = 99.99m;
                min = 0.01m;
            }
            else if (tag == "manualBid" || tag == "manualOfr" )
            {
                // 手动报价 默认无值 输入范围：(-100, 9999.0000]  
                regex = new Regex("^[0-9.-]+$");
                max = 9999m;
                min = -99.9999m;
                digit = 4;
            }
            else if (tag == "defBid" || tag == "defOfr" || tag == "algoMidPrice")
            {
                // 防守点 默认为0 输入范围：[0.00, 1000.00] 小数2位。 //算法报价-中间价值
                regex = new Regex("^[0-9.]+$");
                max = 1000.00m;
                min = 0;
                //min = 0.01m; 需要判断是否大于0
            }
            else if (tag == "defWallBid" || tag == "defWallOfr")
            {
                // 触墙拉宽 输入范围：[0, 1000.00] 
                regex = new Regex("^[0-9.]+$");
                max = 1000.00m;
                min = 0;
            }
            else if (tag == "algoDeviation")
            {
                //算法偏离 默认值为0 单位bp，允许输入范围：[-1000.00，1000.00]
                regex = new Regex("^[0-9.]+$");
                max = 1000.00m;
                min = -1000m;
            }
            else if (tag == "algoSpread")
            {
                //算法点差 无默认值，必须填写。单位bp，允许输入范围：(0，1000.00]。注意，这里点差必须大于0
                regex = new Regex("^[0-9.]+$");
                max = 1000.00m;
                min = 0.01m;
            }

            if (regex == null) { return; }
                    
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;
            if (selectTxt.Length == text.Length)
            {
                if (decimal.TryParse(e.Text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);


            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > digit)
                {
                    e.Handled = true;
                    return;
                }
            }

        }

        private void algoMidType_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ////SetAlgoEnable();
        }


        private void bond_txt_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            if (obj != null)
            {
                OrderDetailViewModel vm = this.DataContext as OrderDetailViewModel;
                //if (bond_txt.SearchText == vm.OrderParamReq.securityid)
                //{
                //    bond_txt.SearchText = string.Empty;

                //    List<Inline> txtRun = new List<Inline>();
                    
                //    Run run = new Run("基准券不能与当前券相同,请重新输入!");

                //    txtRun.Add(run);

                //    CustomMessageBox.ShowDialog(XBond.Plugin.View.Component.CustomMessageBox.EnumNotifyType.Warning, txtRun, null, this);

                //    bond_txt.Focus();

                // }
                ////vm.Data.algoBaseSecurityid = obj.Bond_ID;
                vm.UpdateSecurityCdc();

                this.bond_txt.ToolTip = obj.Bond_ID + " " + obj.Short_Name;

                bond_txt.BorderBrush = vm.DefaultBorderBrush;
            }
        }



        private void orderDetailView_Initialized(object sender, EventArgs e)
        {
            ////if (this.DataContext != null)
            ////{
            ////    SetAlgoEnable();
            ////    SetAlgoBaseEnable();
            ////    SetPosLmtEnable();
            ////    SetAPIModel();
            ////}
            ////if ("1".Equals(LocalEnv.ConfInfo.openQDM2Enable))
            ////{
            ////    openODM2_cb.Visibility = Visibility.Visible;
            ////}
            ////else
            ////{
            ////    openODM2_cb.Visibility = Visibility.Hidden;
            ////}
        }


        private void PART_Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag == null)
                return;
            if (btn.Tag.ToString() == "datePickerbid")
            {
                datePickerbid.BlackoutDates.AddDatesInPast();
            }
            else if (btn.Tag.ToString() == "datePickerofr")
            {
                datePickerofr.BlackoutDates.AddDatesInPast();
            }

            bool ifLeft = SystemParameters.MenuDropAlignment;
            if (ifLeft)
            {
                Grid g = btn.Parent as Grid;
                if (g != null)
                {
                    Popup popup = g.FindName("PART_Popup") as Popup;
                    popup.HorizontalOffset = 103;
                }
            }
        }

        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag.ToString() == "datePickerbid")
            {
                datePickerbid.Text = null;
            }
            else if (btn.Tag.ToString() == "datePickerofr")
            {
                datePickerofr.Text = null;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// 使用基准券
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void algoBaseEnable_chk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void bond_txt_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void bond_txt_ValueChanged(object sender, RoutedEventArgs e)
        {
            ////OrderDetailViewModel vm = this.DataContext as OrderDetailViewModel;
            ////vm.ResetWindowBorderBursh("bond_txt");
            ////bond_txt.ToolTip = string.Empty;
            ////Console.WriteLine("=== bond_txt.IsPopUpShow = {0}", bond_txt.IsPopUpShow);

        }

        private void scroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

        }
    }
}
