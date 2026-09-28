using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
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
    /// VenuesExecutionEntryView.xaml 的交互逻辑
    /// </summary>
    public partial class VenuesExecutionEntryView : Window
    {
        private VenuesExecutionEntryViewModel _venuesExecutionEntryViewModel = null;

        public VenuesExecutionEntryView()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        public VenuesExecutionEntryView(ExecutionReport order, bool isCancel, bool isFromCancel)
        {
            InitializeComponent();
            
            this.Icon = LocalEnv.ServerInfo.AppImage;

            _venuesExecutionEntryViewModel = new VenuesExecutionEntryViewModel();

            this.DataContext = _venuesExecutionEntryViewModel;
            _venuesExecutionEntryViewModel.Init(this, order, isCancel, isFromCancel);
            
        }
        
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            string tag = tb.Tag as string;

            Regex regex = new Regex("^[0-9.-]+$");

            if (!string.IsNullOrEmpty(tag) && tag == "2")//量
                regex = new Regex("^[0-9]+$");

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string maxAmount = tb.Tag as string;

            int max = 1000;

            int tem = 0;
            if (Int32.TryParse(maxAmount, out tem))
                max = tem;

            int min = -1000;
            string text = tb.Text;

            if (!string.IsNullOrEmpty(tag) && tag == "1")//价格
            {
                max = 10000;
                min = -100;
            }

            else if (!string.IsNullOrEmpty(tag) && tag == "2")//量
            {
                min = 0;
                max = 1000000;
            }

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

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
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
    }
}
