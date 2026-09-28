using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.ViewModel.Drp;
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


namespace STC.Common.View.Drp
{
    /// <summary>
    /// DirectionalReturnPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class DirectionalReturnPriceView : Window
    {
        private DirectionalReturnPriceViewModel ddrvm = null;

        public DirectionalReturnPriceView()
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);
            this.Closed += PriceMatchView_Closed;
            this.Unloaded += PriceMatchView_Unloaded;
        }

        public DirectionalReturnPriceView(MsgPo msg)
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);
            this.Unloaded += PriceMatchView_Unloaded;
            this.Closed += PriceMatchView_Closed;

            this.Icon = LocalEnv.ServerInfo.AppImage;

            ddrvm = new DirectionalReturnPriceViewModel();
            ddrvm.Init(this, msg);
            this.DataContext = ddrvm;
        }

        private void PriceMatchView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Closed -= PriceMatchView_Closed;
            this.Unloaded -= PriceMatchView_Unloaded;

            if (ddrvm != null)
                ddrvm.Clear();
        }

        private void PriceMatchView_Closed(object sender, EventArgs e)
        {
            ddrvm.Clear();
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
            }
            else if (tag.Equals("2"))
            {
                maxAmount = "10000000";
                min = 1000;
            }
            else if (tag.Equals("3"))
            {
                maxAmount = "10000000";
                min = 1000;
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
            //if (selectTxt.Length == text.Length)
            //{
            //    if (decimal.TryParse(e.Text, out d) && (d > max || d < min))
            //    {
            //        e.Handled = true;
            //        return;
            //    }
            //    return;
            //}


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
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }

                if (tag == "2") //量
                {
                    if (text != d.ToString())
                    {
                        e.Handled = true;
                        return;
                    }

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
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
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

            ////var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            ////DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            ////while (cell != null && !(cell is System.Windows.Controls.DataGridCell))
            ////    cell = VisualTreeHelper.GetParent(cell);

            ////System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;


            ////if (targetCell != null && targetCell.Column.Header != null)
            ////{
            ////    string head = targetCell.Column.Header.ToString();

            ////    if (head == "点价量(万元)")
            ////        return;

            ////}

            ////DataGrid dg = sender as DataGrid;
            ////_priceMatchViewModel.OnDoubleClick(dg.SelectedItem);
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            if (ddrvm != null)
            {
                ddrvm.ComputeAmount();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ddrvm != null)
            {
                ddrvm.ComputeAmount();
            }
        }

        /// <summary>
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

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (this.DataContext != null && this.DataContext is DirectionalReturnPriceViewModel)
            {
                (this.DataContext as DirectionalReturnPriceViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x28, 0x28, 0x28));
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (this.DataContext != null && this.DataContext is DirectionalReturnPriceViewModel)
            {
                (this.DataContext as DirectionalReturnPriceViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
            }
        }

        /// <summary>
        /// 复制到剪贴板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPrice_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (sender is TextBlock)
                {
                    string id = (sender as TextBlock).Tag.ToString();
                    (this.DataContext as DirectionalReturnPriceViewModel).SetClipboard(id);
                }
            });
        }

        #region 报价追踪
        /// <summary>
        /// 打开报价追踪
        /// </summary>
        //// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Expand_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowWarnPopup();
        }

        /// <summary>
        /// 关闭报价追踪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Expand_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!btn_Expand.IsMouseOver && !this.AnsAttention.IsMouseOver)
            { 
                CollapsedWarnPopup();
            }
        }


        private void ShowWarnPopup()
        {
            MM_Duty_pop.Visibility = Visibility.Visible;
            MM_Duty_pop.IsOpen = true;
            AnsAttention.QueryLog((this.DataContext as DirectionalReturnPriceViewModel).OrderId.Replace("DQLimit_", ""), "1");
        }


        private void CollapsedWarnPopup()   
        {
            MM_Duty_pop.IsOpen = false;
            MM_Duty_pop.Visibility = Visibility.Collapsed;
        }

        private void Ans_status_tb_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapsedWarnPopup();
        }
        #endregion
    }
}
