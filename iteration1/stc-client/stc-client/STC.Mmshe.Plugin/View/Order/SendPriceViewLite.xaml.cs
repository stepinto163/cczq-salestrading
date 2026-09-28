using STC.Frame.Utils;
using STC.Mm.Plugin.CustomeUI;
using STC.Mm.Plugin.ViewModel;
using STC.Mm.Plugin.ViewModel.Order;
using System;
using System.Collections.Generic;
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

namespace STC.Mm.Plugin.View.Order
{
    /// <summary>
    /// SendPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class SendPriceViewLite : Window
    {
        private SendPriceViewModelLite viewModel = null;

        public SendPriceViewLite()
        {
            InitializeComponent();

            datePicker.BlackoutDates.AddDatesInPast();
        }

        public SendPriceViewLite(PriceMatchParam priceMatchParam)
        {
            InitializeComponent();

            datePicker.BlackoutDates.AddDatesInPast();

            this.Icon = LocalEnv.ServerInfo.AppImage;


            viewModel = new SendPriceViewModelLite();
            viewModel.Init(this, priceMatchParam);

            this.DataContext = viewModel;

            //if (priceMatchParam.VenuesSelectF.Equals("1"))
            //{
            //    viewModel.CheckNotOut = true;
            //}

            this.Activated += SendPriceViewLite_Activated;
        }

        private void SendPriceViewLite_Activated(object sender, EventArgs e)
        {
            SetPriceFocus();
        }

        public void SetPriceFocus()
        {
            // 选择最后位数
            int pos = PriceTxt.Text.IndexOf('.');
            if (pos > 0)
            {
                int sellength = PriceTxt.Text.Length - pos + 1;
                PriceTxt.Focus();
                PriceTxt.SelectionStart = pos + 1;
                PriceTxt.SelectionLength = sellength;
            }
            else
            {
                PriceTxt.Focus();
            }
        }

        public SendPriceViewLite(string security, bool isBid)
        {
            InitializeComponent();

            datePicker.BlackoutDates.AddDatesInPast();

            this.Icon = LocalEnv.ServerInfo.AppImage;

            viewModel = new SendPriceViewModelLite();
            if (security.ToLower() == "tradebook")
            {
                security = "";
                Search_TextBox.HintText = "请输入债券代码";
            }

            //请输入债券代码
            viewModel.Init(this, security, isBid);
            this.DataContext = viewModel;

            this.Activated += SendPriceViewLite_Activated;
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
            else if ((tb.Name == "PriceTxt") || (tb.Name == "netPrice"))
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



            int max = 1000;
            int min = -1;

            if (tag == "1")//量
            {
                max = 1000;
                min = 1;
            }
            else if (tag == "2")//价格
            {
                max = 9999;
                min = -100;
            }
            else if (tag == "3")//过期时间
            {
                max = 120;
                min = 1;
            }
            else if (tag == "4")//随机渠道
            {
                max = viewModel.RandomMax;
                min = 1;
            }

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

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

            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                viewModel.SendPriceEnable = false;
            }
            else
            {
                viewModel.SendPriceEnable = true;
            }
            //else
            //{
            //    int intAmount = 0;
            //    if (int.TryParse(AmountTxt.Text, out intAmount))
            //    {
            //        if (intAmount % 1000 != 0)
            //        {
            //            viewModel.SendPriceEnable = false;
            //            return;
            //        }
            //    }
            //}

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

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cbSettle.IsDropDownOpen = !cbSettle.IsDropDownOpen;
        }

        private void AmountTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            // 增加 ↑↓来改变价格
            if (tBox.Name.ToString() == "PriceTxt")
            {
                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    viewModel.AddPriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    viewModel.ReducePriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }
            else if (tBox.Name.ToString() == "AmountTxt") 
            {

                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    viewModel.AddAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    viewModel.ReduceAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }

            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.KeyboardDevice.IsKeyDown(Key.Enter)))
            {
                AmountTxt.Focus();
                // 选择最后位数
                int pos = AmountTxt.Text.Length;
                if (pos > 0)
                {
                    int sellength = AmountTxt.Text.Length;
                    AmountTxt.SelectionStart = 0;
                    AmountTxt.SelectionLength = sellength;
                }
                e.Handled = true;
                return;
            }
            // 判读是否是功能键
            // tag 改变用途
            string tag = string.Empty;


            // 功能键有效性检查
            string text = tBox.Text;

            if (e.Key == Key.Delete  || e.Key == Key.Back)
            {
                decimal d = 0;
                if (text.Length > 0)
                {
                    if (!decimal.TryParse(text, out d))
                    {
                        return;
                    }
                }


                // tag 改变用途
                if (tBox.Name == "AmountTxt")
                {
                    tag = "1";
                }
                else if ((tBox.Name == "PriceTxt") || (tBox.Name == "netPrice"))
                {
                    tag = "2";
                }
                else
                {
                    tag = tBox.Tag as string;
                }
                int max = 1000;
                int min = 0;

                if (tag == "1")//量
                {
                    max = 1000;
                    min = 0;
                }
                else if (tag == "2")//价格
                {
                    max = 9999;
                    min = -100;
                }
                else if (tag == "3")//过期时间
                {
                    max = 120;
                    min = 1;
                }
                else if (tag == "4")//随机渠道
                {
                    max = viewModel.RandomMax;
                    min = 1;
                }

                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    viewModel.SendPriceEnable = false;
                }
                else
                {
                    viewModel.SendPriceEnable = true;
                }
            }
        }


        private void AmountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbAmount = sender as TextBox;
            // tag 改变用途
            string tag = "1";
            int intAmount = 0;
            int.TryParse(tbAmount.Text, out intAmount);

            int max = 1000;
            int min = 0;

            if (tag == "1")//量
            {
                max = 1000;
                min = 0;
            }

            if (intAmount > max || intAmount < min)
            {
                viewModel.SendPriceEnable = false;
            }
            else
            {
                viewModel.SendPriceEnable = true;
            }

            viewModel.SendPriceItem.Amount = intAmount == 0 ? tbAmount.Text : intAmount.ToString();
            viewModel.SetAmountTxtSelectIndex();
            //if (intAmount % 1000 != 0)
            //{
            //    viewModel.SendPriceEnable = false;
            //}

            ////TextBox tbAmount = sender as TextBox;
            ////string tag = tbAmount.Tag as string;
            ////int intAmount = 0;
            ////int.TryParse(tbAmount.Text, out intAmount);
            viewModel.ResetQtyWarn(tag, intAmount.ToString());

        }

        private void LbDirect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel != null)
                viewModel.ResetQtyWarn();
        }

        private void bdSetleBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SettlTypeCombox.IsDropDownOpen = !SettlTypeCombox.IsDropDownOpen;
            if (viewModel != null)
            {
                viewModel.SettleBoxIndex = 0;
                viewModel.AddOneOpenedTag = "1";
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                //判断用户的按键是否为Enter
                if (Keyboard.IsKeyDown(Key.Enter))
                {
                    if (viewModel != null && viewModel.OkCommand.CanExecute(null))
                        viewModel.OkCommand.Execute(null);

                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SendPriceViewModelLite viewModel = (SendPriceViewModelLite)this.DataContext;
            viewModel.Dispose();
        }

        private void dgPrice_Unloaded(object sender, RoutedEventArgs e)
        {
            Card.PriceAndOrderView po = (Card.PriceAndOrderView)dgPrice;
            po.CloseViewModel();
            GC.Collect();
        }

        private void Search_TextBox_AfterSearch(Common.Model.Base.BondsInfoPo obj)
        {
            SendPriceViewModelLite viewModel = (SendPriceViewModelLite)this.DataContext;
            if (string.IsNullOrEmpty(Search_TextBox.SearchText))
            {
                Search_TextBox.SearchText = string.Empty;
                return;
            }

            string newSecurityId = Search_TextBox.SearchText;
            viewModel.SearchText = newSecurityId;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPriceViewModelLite))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("LostFocusBorderBrush");
                LineSplit.Stroke = (Brush)viewModel.myWindow.FindResource("LostFocusBorderBrush");
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPriceViewModelLite))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("GetFocusBorderBrush");
                if (btn_Expand.Content.ToString() == ">>")
                {
                    LineSplit.Stroke = (Brush)viewModel.myWindow.FindResource("GetFocusBorderBrush");
                }
                else
                { 
                    LineSplit.Stroke = (Brush)viewModel.myWindow.FindResource("LostFocusBorderBrush");
                }
            }
        }
    }
}
