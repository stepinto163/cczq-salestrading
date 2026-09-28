using STC.Common.Utils;
using STC.Rfq.Plugin.ViewModel;
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
using STC.Common.View.Component;
using STC.Rfq.Plugin.Model;
using System.Text.RegularExpressions;

namespace STC.Rfq.Plugin.View
{
    /// <summary>
    /// RfqReplayRefuseView.xaml 的交互逻辑
    /// </summary>
    public partial class RfqNewReqView : Window
    {
        RfqReplayRefuseViewModel viewModel;

        public RfqNewReqView(RfqOrderDetailData para)
        {
            InitializeComponent();
            viewModel = new RfqReplayRefuseViewModel(this, para);

            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "stc":
                    TitleIcon.Source = new BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/stc_windows_icon_64.ico"));
                    break;
                case "dmart":
                    TitleIcon.Source = new BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/dmart_windows_ico_32.ico"));
                    break;
            }
            this.DataContext = viewModel;
            this.Activated += Window_Activated;
        }

        public void SetAction(string action)
        {
            //CancelOrder 撤单
            //ReplyOrder 回价
            //ChangeOrder 改价
            //NewOrder  新需求
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                switch (action)
                {
                    case "CancelOrder":
                        btnOk.IsEnabled = false;
                        break;
                    case "NewOrder":
                    case "ReplyOrder":
                    case "ChangeOrder":
                        btnCancel.Visibility = Visibility.Collapsed;
                        break;
                }

                if ((this.viewModel != null) && (viewModel is RfqReplayRefuseViewModel))
                {
                    viewModel.Action = action;
                }
            }));
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is RfqReplayRefuseViewModel))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("GetFocusBorderBrush");
            }

            SetPriceFocus();
        }

        public void SetPriceFocus()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
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
            }));
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is RfqReplayRefuseViewModel))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("LostFocusBorderBrush");
                //LineSplit.Stroke = (Brush)viewModel.myWindow.FindResource("LostFocusBorderBrush");
            }
        }


        /// <summary>
        /// 禁止粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidMinute_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            TextBox tb = sender as TextBox;

            if (e.Key == Key.Enter)
            {
                if (tb.Name.ToString() == "ValidMinute")
                {
                    ValidSecond.Focus();
                }
                else if (tb.Name.ToString() == "ValidSecond")
                {
                    btnOk.Focus();
                }
            }

        }


        /// <summary>
        /// 检查输入范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidMinute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = null;
            regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Handled)
                return;

            string tag = tb.Tag as string;
            int max = 500;
            int min = (tag == "0") ? 0 : 1;

            if (tag == "0")// [0-60)
            {
                max = 59;
            }
            else if (tag == "1") // [0-500]
            {
                min = 0;
                max = 500;
            }
            else if (tag == "2") // (0-24]
            {
                max = 24;
            }
            else if (tag == "3") // [0-60)
            {
                min = 0;
                max = 59;
            }
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".")) || (text.Contains("-") && e.Text.Equals("-")))
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
        }

        private void bdTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        #region Price
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
            regex = new Regex("^[0-9.-]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;


            decimal max = 1000;
            decimal min = -1;

            if (tag == "1")//量
            {
                max = 9999.90m;
                min = 0.01m;
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

            //if (decimal.TryParse(text, out d) && (d > max || d < min))
            //{
            //    e.Handled = true;
            //    return;
            //}

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (tag == "2")
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                else if (tag == "1")
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 2)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }


            e.Handled = false;
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

            // 判读是否是功能键
            // tag 改变用途
            string tag = string.Empty;


            // 功能键有效性检查
            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
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

                decimal max = 1000;
                decimal min = 0;

                if (tag == "1")//量
                {
                    max = 9999.90m;
                    min = 0.1m;
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

                //if (decimal.TryParse(text, out d) && (d > max || d < min))
                //{
                //    viewModel.SendPriceEnable = false;
                //}
                //else
                //{
                //    viewModel.SendPriceEnable = true;
                //}
            }

            if (e.Key == Key.Enter)
            {
                if (tBox.Name.ToString() == "PriceTxt")
                {
                    netPrice.Focus();
                }
                else if (tBox.Name.ToString() == "netPrice")
                {
                    AmountTxt.Focus();
                }
                else if (tBox.Name.ToString() == "AmountTxt")
                {
                    ValidMinute.Focus();
                }
            }
        }
        #endregion
    }
}
