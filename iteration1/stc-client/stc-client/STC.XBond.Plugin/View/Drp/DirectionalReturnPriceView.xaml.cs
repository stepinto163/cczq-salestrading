using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.XBond.Plugin.CustomUI;
using STC.XBond.Plugin.ViewModel.Drp;
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


namespace STC.XBond.Plugin.View.Drp
{
    /// <summary>
    /// DirectionalReturnPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class DirectionalReturnPriceView : Window
    {
        private DirectionalReturnPriceViewModel _priceMatchViewModel = null;

        public DirectionalReturnPriceView()
        {
            InitializeComponent();

            this.Closed += PriceMatchView_Closed;
            this.Unloaded += PriceMatchView_Unloaded;
        }

        public DirectionalReturnPriceView(MsgPo msg)
        {
            InitializeComponent();

            this.Unloaded += PriceMatchView_Unloaded;
            this.Closed += PriceMatchView_Closed;

            this.Icon = LocalEnv.ServerInfo.AppImage;

            _priceMatchViewModel = new DirectionalReturnPriceViewModel();
            _priceMatchViewModel.Init(this, msg);
            this.DataContext = _priceMatchViewModel;
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

        

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9]+$");
            string maxAmount = string.Empty;
            string tag = tb.Tag as string;

            int min = 0;
            if (tag.Equals("1"))
            {
                maxAmount = _priceMatchViewModel == null ? string.Empty : _priceMatchViewModel.OutChannelNum;
            }
            else if (tag.Equals("2"))
            {
                maxAmount = "10000000";
                min = 1;
            }
            else if (tag.Equals("3"))
            {
                maxAmount = "10000000";
                min = 1;
            }
            else if (tag.Equals("4"))
            {
                regex = new Regex("^[0-9.-]+$");
                maxAmount = "9999";
                min = -100;
            }

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 20;

            int tem = 0;
            if (Int32.TryParse(maxAmount, out tem))
                max = tem;

            
            string text = tb.Text;
            if (tag.Equals("4"))
            {
                if ((text.Contains(".") && e.Text.Equals("."))
                    || (text.Contains("-") && e.Text.Equals("-")))
                {
                    e.Handled = true;
                    return;
                }
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

            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell))
                cell = VisualTreeHelper.GetParent(cell);

            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;


            if (targetCell != null && targetCell.Column.Header != null)
            {
                string head = targetCell.Column.Header.ToString();

                if (head == "点价量(万元)")
                    return;

            }

            DataGrid dg = sender as DataGrid;
            _priceMatchViewModel.OnDoubleClick(dg.SelectedItem);
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            if (_priceMatchViewModel != null)
            {
                _priceMatchViewModel.ComputeAmount();
            }
        }

        private void CheckBox_All_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (_priceMatchViewModel != null)
            {
                _priceMatchViewModel.CheckBoxAllValueChange();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (_priceMatchViewModel != null)
            {
                _priceMatchViewModel.ChangeAmount(tb.Text);
            }
        }

        /// <summary>
        /// to do ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextBox tb = sender as TextBox;
            //if (_priceMatchViewModel != null)
            //{
            //    _priceMatchViewModel.ChangeAmount(tb.Text);
            //}
        }

        private void CheckBox_All_Checked(object sender, RoutedEventArgs e)
        {
            if (_priceMatchViewModel != null)
            {
                _priceMatchViewModel.ComputeAmount();
            }
        }
    }
}
