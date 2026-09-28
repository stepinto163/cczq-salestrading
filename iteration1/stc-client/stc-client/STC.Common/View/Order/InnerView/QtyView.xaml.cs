using STC.Common.Utils;
using STC.Common.ViewModel.Order.InnerView;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Common.View.Order.InnerView
{
    /// <summary>
    /// QtyView.xaml 的交互逻辑
    /// </summary>
    public partial class QtyView : UserControl
    {
        public event KeyEventHandler KeyDown_AmountTxt;

        private QtyViewModel qtyViewModel = new QtyViewModel();

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(QtyView));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        public QtyView()
        {
            InitializeComponent();

            this.DataContext = qtyViewModel;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = new Regex("^[0-9.-]+$");

            decimal max = 9999.99M;
            decimal min = -9999.99M;

            if (qtyViewModel.QtyTxt == QtyViewModel.TXT_IRS)//量
            {
                regex = new Regex("^[0-9]+$");

                max = 100000M;
                min = 1M;
            }
            else if (qtyViewModel.QtyTxt == QtyViewModel.TXT_W)
            {
                regex = new Regex("^[0-9]+$");

                max = 100000000M;
                min = 1M;
            }
            else
            {
                regex = new Regex("^[0-9.-]+$");

                
                max = 1000M;
                min = 0.1M;

                if (LocalEnv.ConfInfo.QtyDecimalDigit == 2)
                {
                    min = 0.01M;
                }
                else if (LocalEnv.ConfInfo.QtyDecimalDigit == 3)
                {
                    min = 0.001M;
                }
                else if (LocalEnv.ConfInfo.QtyDecimalDigit == 4)
                {
                    min = 0.0001M;
                }
            }
                

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".") && (qtyViewModel.QtyTxt == QtyViewModel.TXT_IRS || qtyViewModel.QtyTxt == QtyViewModel.TXT_W))
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
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > LocalEnv.ConfInfo.QtyDecimalDigit && qtyViewModel.QtyTxt != QtyViewModel.TXT_IRS && qtyViewModel.QtyTxt != QtyViewModel.TXT_W)
                {
                    e.Handled = true;
                    return;
                }
            }


            e.Handled = false;
        }

        private void AmountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.qtyViewModel != null)
            {
                int intAmount = 0;
                if (int.TryParse(AmountTxt.Text, out intAmount))
                {
                    qtyViewModel.SetAmountTxtSelectIndex(AmountTxt.Text);
                    string qty = qtyViewModel.GetAmount().ToString();

                    RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
                }
            }
        }

        private void AmountTxt_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDown_AmountTxt?.Invoke(sender, e);
            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                e.Handled = true;
            }
        }

        private void AmountTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if(!string.IsNullOrEmpty(this.AmountTxt.Text)
                    && CommonUtils.ConvertToDecimal(this.AmountTxt.Text) == 0)
                {
                    this.AmountTxt.Text = string.Empty;
                }
            }
        }
    }
}
