using STC.Common.Model.EMS;
using STC.Common.Model.Fix.Message;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BDB.Trade.Plugin.CustomeUI
{
    /// <summary>
    /// ValetDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class ValetDetailView : UserControl
    {
        private ValetDetailViewModel _valetDetailViewModle = null;

        public ValetDetailView()
        {
            InitializeComponent();

            this.Unloaded += ValetDetailView_Unloaded;
        }

        private void ValetDetailView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ValetDetailView_Unloaded;
            
            if (OnDownClick != null)
            {
                OnDownClick = null;
            }
        }

        public ValetDetailView(object comboxData, string price, string goodsCode, string tradeDate, string settleType, string amount, ExecutionReport order = null)
        {
            InitializeComponent();

            _valetDetailViewModle = new ValetDetailViewModel();
            this.DataContext = _valetDetailViewModle;
            _valetDetailViewModle.Init(this, comboxData, price, goodsCode, tradeDate, settleType, amount, order);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cbIDB.IsDropDownOpen = !cbIDB.IsDropDownOpen;
        }

        public UpdateOrderReq GetValetInfo()
        {
            return _valetDetailViewModle.GetValetInfo();
        }

        public bool GetPriceCheck()
        {
            return _valetDetailViewModle.PriceCheck;
        }

        public ComboxItemData GetComboxItemData()
        {
            return _valetDetailViewModle.ComboxSelectedItem;
        }

        public bool CheckInfo()
        {
            return _valetDetailViewModle.Check();
        }

        public event Action<string> OnDownClick;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnDownClick?.Invoke(this.Tag as string);
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

        private void tbYield_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            string tag = tb.Tag as string;

            Regex regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;



            int max = 9999;
            int min = -100;
            
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
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        

        private void tbAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            string tag = tb.Tag as string;

            Regex regex = new Regex("^[A-Za-z0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);

            if (text.Length > 10)
            {
                e.Handled = true;
                return;
            }
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
    }
}
