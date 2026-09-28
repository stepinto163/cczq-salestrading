using BDB.Trade.Plugin.CustomUI;
using STC.Common.Utils;
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

namespace BDB.Trade.Plugin.CustomeUI
{
    /// <summary>
    /// PriceMatchView.xaml 的交互逻辑
    /// </summary>
    public partial class PriceMatchView : Window
    {
        private PriceMatchViewModel _priceMatchViewModel = null;

        public PriceMatchView()
        {
            InitializeComponent();

            this.Closed += PriceMatchView_Closed;
            this.Unloaded += PriceMatchView_Unloaded;
        }

        private void PriceMatchView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Closed -= PriceMatchView_Closed;
            this.Unloaded -= PriceMatchView_Unloaded;

            if (_priceMatchViewModel != null)
                _priceMatchViewModel.Clear();
        }

        private void PriceMatchView_Closed(object sender, EventArgs e)
        {
            _priceMatchViewModel.Clear();
            this.Close();
        }

        public PriceMatchView(MsgPo msg)
        {
            InitializeComponent();

            this.Unloaded += PriceMatchView_Unloaded;
            this.Closed += PriceMatchView_Closed;

            this.Icon = LocalEnv.ServerInfo.AppImage;

            _priceMatchViewModel = new PriceMatchViewModel();
            this.DataContext = _priceMatchViewModel;
            _priceMatchViewModel.Init(this, msg);
        }
        

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string maxAmount = string.Empty;
            string tag = tb.Tag as string;

            int min = 0;
            if (tag.Equals("1"))
                maxAmount = _priceMatchViewModel == null ? string.Empty : _priceMatchViewModel.OutChannelNum;
            else if (tag.Equals("2"))
            {
                maxAmount = "10000000";
                min = 1;
            }
            int max = 20;

            int tem = 0;
            if (Int32.TryParse(maxAmount, out tem))
                max = tem;

            
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
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 8)
                {
                    e.Handled = true;
                    return;
                }
            }
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

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            _priceMatchViewModel.OnDoubleClick(dg.SelectedItem);
        }
    }
}
