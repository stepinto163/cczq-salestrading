using BDB.Trade.Plugin.ViewModel.Order;
using STC.Common.Utils;
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
using System.Windows.Shapes;

namespace BDB.Trade.Plugin.View.Order
{ 
    /// <summary>
    /// SendPointPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class SendPointPriceViewLite : Window
    {
        private SendPointPriceModelLite viewModel = null;

        public SendPointPriceViewLite()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始调用参数
        /// </summary>
        /// <param name="security">债券代码 例如 180210.IB</param>
        /// <param name="isBid">方向 1 = Bid 2 = Ofr</param>
        /// <param name="clickPrice">收益率（%）</param>
        /// <param name="clickAmount">数量（万）</param>
        /// <param name="settle"></param>
        /// <param name="netPrice"></param>
        /// <param name="source">渠道代码或渠道名称或渠道数字代码</param>
        /// <param name="wholeflag"></param>
        /// <param name="isFrim"></param>
        /// <param name="orderinfotip">订单盈利信息  例如 3.2250% 2000*+4000(明+0)+1000(+0)+3000(远)</param>
        /// <param name="clOrdID">下单ID</param>
        public SendPointPriceViewLite(string security, bool isBid, string clickPrice, string clickAmount, string settle, string netPrice, string source, string wholeflag, bool isFrim, string orderinfotip = "",string clOrdID="")
        {
            decimal tmpprice = 0;
            if (!decimal.TryParse(clickPrice, out tmpprice))
            {
                this.Close();
                return;
            }
            clickPrice = tmpprice.ToString("F4");
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
            viewModel = new SendPointPriceModelLite();
            this.DataContext = viewModel;


            Style s = (Style)this.Resources["SendPriceFilterListBoxItemStyle"];
            Thickness margin = new Thickness(0, 0, 2, 2);
            //LstSettleBox.Items.Add(new ListBoxItem() { Content = "不限", Style = s, Width = 60, Height = 24, Margin = margin });
            LstSettleBox.Items.Add(new ListBoxItem() { Content = "+1", Style = s, Width = 60, Height = 24, Margin = margin });
            LstSettleBox.Items.Add(new ListBoxItem() { Content = "+0", Style = s, Width = 60, Height = 24, Margin = margin });

            viewModel.DictSettleBox.Add(0, "2,3");
            //viewModel.DictSettleBox.Add(1, "2");  //+1 的默认值，可能为2或3
            viewModel.DictSettleBox.Add(1, "1");
            // 判断是否是多远期
            // settle = "2:3:5.28+1";
            List<string> lstsettle = new List<string>();
            string[] tmpsettle = settle.Split(':');
            string tmp = string.Empty;
            for (int i = 0; i < tmpsettle.Length; i++)
            {
                tmp = tmpsettle[i];
                // +0 
                if (tmp == "1")
                {
                    if (!lstsettle.Contains("1"))
                    {
                        lstsettle.Add("1");
                    }
                }
                // +1
                if (tmp == "2" || tmp == "3" || tmp == "2,3" || tmp == "3,2")
                {
                    if (!lstsettle.Contains("2,3"))
                    {
                        lstsettle.Add("2,3");
                    }
                }

                // 远
                if (tmp.Contains("."))
                {
                    if (!lstsettle.Contains(tmp))
                    {
                        lstsettle.Add(tmp);
                    }
                }
            }

            // 如果混合 就是 +1
            if (lstsettle.Count > 1)
            {
                // 判断settle的值 +1 的情况
                if (settle.Contains("."))
                {
                    List<string> lstSettle = settle.Split(':').ToList();
                    for (int i = 0; i < lstSettle.Count; i++)
                    {
                        if (lstSettle[i].Contains("."))
                        {
                            LstSettleBox.Items.Add(new ListBoxItem() { Content = lstSettle[i], Style = s, Width = 60, Height = 24, Margin = margin });
                            viewModel.DictSettleBox.Add(viewModel.DictSettleBox.Count, lstSettle[i]);
                        }
                    }
                }
                // 混合结算速度，默认设为+1
                settle = "2,3";
            }
            else if (lstsettle.Count == 1)
            {
                if (lstsettle[0] == "1")
                {
                    settle = "1";
                }
                else if (lstsettle[0] == "2,3")
                {
                    settle = "2,3";
                }
                else
                {
                    // 判断settle的值 +1 的情况
                    if (settle.Contains("."))
                    {
                        List<string> lstSettle = settle.Split(':').ToList();
                        for (int i = 0; i < lstSettle.Count; i++)
                        {
                            LstSettleBox.Items.Add(new ListBoxItem() { Content = lstSettle[i], Style = s, Width = 60, Height = 24, Margin = margin });
                            viewModel.DictSettleBox.Add(2 + i, lstSettle[i]);
                        }
                    }
                }
            }

            if (LstSettleBox.Items.Count > 6)
            {
                this.Height = 342;
                viewModel.MaxPriceAreaHeight = 272;
            }
            else
            {
                this.Height = 318;
                viewModel.MaxPriceAreaHeight = 248;
            }


            viewModel.Init(this, security, isBid, clickPrice, clickAmount, lstsettle, netPrice, source, wholeflag, isFrim, orderinfotip, clOrdID);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void PART_Button_Click(object sender, RoutedEventArgs e)
        {
            bool ifLeft = SystemParameters.MenuDropAlignment;
            if (ifLeft)
            {
                Button btn = (Button)sender;
                Grid g = btn.Parent as Grid;
                if (g != null)
                {
                    Popup popup = g.FindName("PART_Popup") as Popup;
                    popup.HorizontalOffset = 103;
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            // tag 改变用途
            if (tb.Name == "AmountTxt")
            {
                tag = "1";
            }
            else if (tb.Name == "PriceTxt")
            {
                tag = "2";
            }
            else
            {
                tag = tb.Tag as string;
            }

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 1000000;
            int min = 1000;


            string text = tb.Text;


            string selectTxt = tb.SelectedText;
            decimal d = 0;

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // this.Topmost = false;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.Topmost = true;
        }

        private void NumericUpDown_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                viewModel.SetTimeOutSelectIndex();
            }
        }

        private void AmountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.viewModel != null)
            {
                int intAmount = 0;
                if (int.TryParse(AmountTxt.Text, out intAmount))
                {
                    if (intAmount % 1000 != 0)
                    {
                        viewModel.SendPriceEnable = false;
                    }
                    else
                    {
                        viewModel.SendPriceEnable = intAmount >= 1000 && intAmount <= 1000000;
                    }
                }
                else
                {
                    viewModel.SendPriceEnable = false;
                }


                if (viewModel.SendPriceEnable)
                {
                    viewModel.SendPriceEnable = viewModel.PriceValid;
                }

                viewModel.SetAmountTxtSelectIndex();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //判断用户的按键是否为Enter
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                if (viewModel != null && viewModel.OkCommand.CanExecute(null))
                    viewModel.OkCommand.Execute(null);
            }
        }
    }
}
