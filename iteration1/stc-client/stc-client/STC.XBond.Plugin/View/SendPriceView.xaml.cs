using STC.Frame.Utils;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.CustomUI;
using STC.XBond.Plugin.ViewModel;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// SendPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class SendPriceView : Window
    {
        private SendPriceViewModel viewModel = null;

        public SendPriceView()
        {
            InitializeComponent();

            datePicker.BlackoutDates.AddDatesInPast();
            
        }

        public SendPriceView(PriceMatchParam priceMatchParam)
        {
            InitializeComponent();

            datePicker.BlackoutDates.AddDatesInPast();

            this.Icon = LocalEnv.ServerInfo.AppImage;
            

            viewModel = new SendPriceViewModel();
            viewModel.Init(this, priceMatchParam);
            this.DataContext = viewModel;
        }

        public SendPriceView(string security, bool isBid)
        {
            InitializeComponent();

            datePicker.BlackoutDates.AddDatesInPast();
            
            this.Icon = LocalEnv.ServerInfo.AppImage;

            viewModel = new SendPriceViewModel();
            viewModel.Init(this, security, isBid);
            this.DataContext = viewModel;
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

            string tag = tb.Tag as string;

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2")
                regex = new Regex("^[0-9.-]+$");
           

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            
            
            int max = 1000;
            int min = -1000;

            if (tag == "1")//量
            {
                max = 1000000;
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
            if (selectTxt.Length == text.Length)
            {
                if (decimal.TryParse(e.Text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            if(selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart+selectTxt.Length, (text.Length- tb.SelectionStart - selectTxt.Length));
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
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) 
                && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;

                return;
            }
        }

        private void AmountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbAmount = sender as TextBox;
            string tag = tbAmount.Tag as string;
            int intAmount = 0;
            int.TryParse(tbAmount.Text, out intAmount);
            viewModel.ResetQtyWarn(tag, intAmount.ToString());
        }

        private void LbDirect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel != null)
                viewModel.ResetQtyWarn();
        }
    }
}
