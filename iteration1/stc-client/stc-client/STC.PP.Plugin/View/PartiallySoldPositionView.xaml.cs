using com.gateway.log;
using STC.Common.Utils;
using STC.Common.ViewModel.Order.InnerView;
using STC.PP.Plugin.ViewModel;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.PP.Plugin.View
{
    /// <summary>
    /// ManualInput.xaml 的交互逻辑
    /// </summary>
    public partial class PartiallySoldPositionView : Window
    {
        private PartiallySoldPositionViewModel viewModel = null;
        public decimal max = 100000000M;
        //STC可用持仓量
        public decimal MinStcAvailablePosition = 0;
        public PartiallySoldPositionView()
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);
            viewModel = new PartiallySoldPositionViewModel(this);
            this.DataContext = viewModel;
            AmountTxt.TextChanged += AmountTxt_TextChanged;
            this.Activated += PartiallySoldPositionView_Activated;
        }

        public void SetAmountText(string text)
        {
            if (AmountTxt != null)
            {
                try
                {
                    //max = CommonUtils.ConvertToDecimal(text) > 9999999 ? 9999999 : CommonUtils.ConvertToDecimal(text);
                    viewModel.Amount = text;
                    AmountTxt.Text = text;
                    AmountTxt.Focus();
                    AmountTxt.SelectAll();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("SetAmountText error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 设置最小STC可用持仓量
        /// </summary>
        /// <param name="_stcAvailablePosition"></param>
        public void SetMinStcAvailablePositionTxt(decimal _stcAvailablePosition)
        {
            MinStcAvailablePosition = CommonUtils.ConvertToDecimal(_stcAvailablePosition.ToString("N0"));
            MinStcAvailablePositionTxt.Text = FormatWithComma(_stcAvailablePosition.ToString("N0"));   
        }

        /// <summary>
        /// 是否选择的多条记录
        /// </summary>
        /// <param name="isSelectMultiRecord"></param>
        public void SetShowWarnTxt(bool isSelectMultiRecord = true)
        {
            ShowWarnTxt.Text = isSelectMultiRecord ? "选中的持仓记录中最小<STC可用持仓量>: " : "选中的持仓记录中<STC可用持仓量>: ";
        }

       
        private void PartiallySoldPositionView_Activated(object sender, EventArgs e)
        {
            // 选择最后位数
            int pos = AmountTxt.Text.IndexOf('.');
            if (pos > 0)
            {
                int sellength = AmountTxt.Text.Length - pos + 1;
                AmountTxt.Focus();
                AmountTxt.SelectionStart = pos + 1;
                AmountTxt.SelectionLength = sellength;
            }
            else
            {
                AmountTxt.Focus();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.Enter))
            {
                //判断用户的按键是否为Enter
                if (Keyboard.IsKeyDown(Key.Enter))
                {
                    if (viewModel != null && viewModel.OkCommand.CanExecute(null))
                        viewModel.OkCommand.Execute(null);
                }
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (this.viewModel != null)
            {
                viewModel.onWindowActivated(1);
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (this.viewModel != null)
            {
                viewModel.onWindowActivated(2);
            }
        }

        public void ShowEMSMask(bool flag)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                emsloadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            }));
        }


        public void SetTopmost(bool flag)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Topmost = flag;
            }));
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = new Regex("^[0-9.-]+$");
         
            decimal min = 1M;

            regex = new Regex("^[0-9]+$");

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".")) || (text.Contains("-") && e.Text.Equals("-")))
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
            else
            {
                e.Handled = true;
                return;
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min) && d != 0)
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 0)
                {
                    e.Handled = true;
                    return;
                }
            }

            e.Handled = false;
        }

        private void AmountTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        private void AmountTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        private void AmountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            string currentText = AmountTxt.Text;
            if (!string.IsNullOrWhiteSpace(currentText))
            {
                decimal inputAmount = CommonUtils.ConvertToDecimal(currentText);
                if (inputAmount > MinStcAvailablePosition)
                {
                    RedAlertTxt.Text = $"输入量大于 {FormatWithComma(MinStcAvailablePosition.ToString("N0"))} 万";
                    return;
                }
            }

            RedAlertTxt.Text = "";
        }

        private string FormatWithComma(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 使用正则表达式,从右往左每3位添加逗号
            return System.Text.RegularExpressions.Regex.Replace(
                input,
                @"(\d)(?=(\d{3})+(?!\d))",
                "$1,"
            );
        }
    }
}
