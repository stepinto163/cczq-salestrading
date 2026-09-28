using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Component;
using STC.Broker.Plugin.View.Transaction.Settle;
using STC.Broker.Plugin.ViewModel.Transaction;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Component;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;
using CommonUtils = STC.Common.Utils.CommonUtils;

namespace STC.Broker.Plugin.View.Transaction
{
    /// <summary>
    /// TransactionPanel.xaml 的交互逻辑
    /// </summary>
    public partial class TransactionPanel : UserControl
    {
        private TransactionPanelViewModel vm;


        public TransactionPanel()
        {
            Thread.CurrentThread.CurrentCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            InitializeComponent();
            // AppUtils.IsInnerInput = true;
            vm = new TransactionPanelViewModel(this);
            this.DataContext = vm;

            (DataContext as TransactionPanelViewModel).Init(this);

            //tb_remarks.KeyDown += Tb_remarks_KeyDown;
            //tb_remarks.GotFocus += Tb_remarks_GotFocus;
        }


        private void dgDetail_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                e.Row.Header = e.Row.GetIndex() + 1;
                e.Row.MouseEnter += Row_MouseEnter;
            });
        }

        private void Row_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                //DataGridRow row = sender as DataGridRow;
                //if (row != null)
                //{
                //    (this.DataContext as TransactionPanelViewModel).EditOrderSettle(row.DataContext as TransactionOrdermModel);
                //}
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        private void TransactionDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (vm.RowCount == 0)
            {
                e.Handled = true;
            }
            else
            {
                if (e.Key == Key.PageUp)
                {
                    // 处理 PageUp 按钮逻辑
                    // 例如，移动 DataGrid 到上一页
                    // 可以使用 DataGrid 的滚动条或者改变显示的数据
                    Int32 dd = GetVisibleRowCount();

                    if (TransactionDataGrid.SelectedIndex - dd > 0)
                        TransactionDataGrid.SelectedIndex = TransactionDataGrid.SelectedIndex - dd;
                    else
                        TransactionDataGrid.SelectedIndex = 0;

                    if (TransactionDataGrid.SelectedItem != null)
                        TransactionDataGrid.ScrollIntoView(TransactionDataGrid.SelectedItem);

                    e.Handled = true; // 防止事件继续传播
                }
                else if (e.Key == Key.PageDown)
                {
                    // 处理 PageDown 按钮逻辑
                    // 例如，移动 DataGrid 到下一页
                    // 可以使用 DataGrid 的滚动条或者改变显示的数据
                    Int32 dd = GetVisibleRowCount();

                    if (TransactionDataGrid.SelectedIndex + dd < TransactionDataGrid.Items.Count - 1)
                        TransactionDataGrid.SelectedIndex += dd;
                    else
                        TransactionDataGrid.SelectedIndex = TransactionDataGrid.Items.Count - 1;

                    if (TransactionDataGrid.SelectedItem != null)
                        TransactionDataGrid.ScrollIntoView(TransactionDataGrid.SelectedItem);

                    e.Handled = true; // 防止事件继续传播
                }
                else if (e.Key == Key.Home)
                {
                    // 处理 Home 按钮逻辑
                    TransactionDataGrid.SelectedIndex = 0;
                    TransactionDataGrid.ScrollIntoView(TransactionDataGrid.SelectedItem);
                    e.Handled = true; // 防止事件继续传播
                }
                else if (e.Key == Key.End)
                {
                    // 处理 End 按钮逻辑
                    TransactionDataGrid.SelectedIndex = TransactionDataGrid.Items.Count - 1;
                    TransactionDataGrid.ScrollIntoView(TransactionDataGrid.SelectedItem);
                    e.Handled = true; // 防止事件继续传播
                }
            }
        }

        #region 获取DataTable状态

        // 获取可视区域中的第一行索引
        public int GetFirstVisibleRowIndex(ScrollViewer scrollViewer)
        {
            // 计算第一行的索引
            double verticalOffset = scrollViewer.VerticalOffset;
            double rowHeight = GetRowHeight();
            int firstVisibleRowIndex = Convert.ToInt32(verticalOffset / rowHeight);
            return firstVisibleRowIndex;
        }


        // 获取可视行中第一行的高度
        private double GetRowHeight()
        {
            if ((this.DataContext as TransactionPanelViewModel).RowCount > 0)
            {
                DataGridRow row = (DataGridRow)TransactionDataGrid.ItemContainerGenerator.ContainerFromIndex(0);
                if (row != null)
                {
                    return row.ActualHeight;
                }
            }

            return vm.RowDefaultHeight;
        }

        // 获取 DataGrid 中可显示的行数
        public int GetVisibleRowCount()
        {
            // 获取 DataGrid 的可视化元素
            var scrollViewer = FindVisualChild<ScrollViewer>(TransactionDataGrid);

            if (scrollViewer != null)
            {
                // 计算可见行数
                double rowsPerPage = scrollViewer.ViewportHeight / GetRowHeight();
                return Convert.ToInt32(rowsPerPage);
            }

            return 0;
        }

        // 辅助方法，用于在 Visual 树中查找指定类型的子元素
        public T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null)
                    {
                        return childItem;
                    }
                }
            }
            return null;
        }
        #endregion

        private void tbOrderId_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {

                    if (e.ClickCount == 2)
                    {
                        e.Handled = true;
                        TextBlock textBlock = sender as TextBlock;
                        (this.DataContext as TransactionPanelViewModel).EditBondInfo(textBlock);

                    }
                }));
            }

        }

        private void SearchTextBox_AfterSearchBond(Common.Model.Base.SimpleBondsInfoPo obj, object arg2)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                TransactionOrdermModel transaction = arg2 as TransactionOrdermModel;
                (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(transaction);
                (this.DataContext as TransactionPanelViewModel).UpdateBondInfo(obj);
            }));
        }

        private void st_bondEdit_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                (this.DataContext as TransactionPanelViewModel).UpdateBondInfo(obj);
            }));
        }

        private void tbOrderId_MouseEnter(object sender, MouseEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Border border = sender as Border;
                if (border != null)
                {
                    TransactionOrdermModel transactionPanelViewModel = border.DataContext as TransactionOrdermModel;
                    if (transactionPanelViewModel != null)
                    {
                        transactionPanelViewModel.IsMouseover = true;
                        if (transactionPanelViewModel.OrderStatus == "G")
                        {
                            if ((this.DataContext as TransactionPanelViewModel).CheckOrderStatus(transactionPanelViewModel))
                            {
                                border.BorderThickness = new Thickness(1);
                                Cursor = Cursors.Hand;
                                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#238F7E"));
                            }
                            else
                            {
                                border.BorderThickness = new Thickness(0);
                            }
                        }
                        else
                        {
                            border.BorderThickness = new Thickness(0);
                        }
                    }
                    //border.BorderThickness = new Thickness(1);
                    //Cursor = Cursors.Hand;
                    //border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#238F7E"));
                }
            }));

        }

        private void tbOrderId_MouseLeave(object sender, MouseEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //Cursor = null;
                Border border = sender as Border;
                if (border != null)
                {
                    TransactionOrdermModel transactionPanelViewModel = border.DataContext as TransactionOrdermModel;
                    if (transactionPanelViewModel != null)
                    {
                        transactionPanelViewModel.IsMouseover = false;
                        if (transactionPanelViewModel.OrderStatus == "G")
                        {
                            if ((this.DataContext as TransactionPanelViewModel).CheckOrderStatus(transactionPanelViewModel))
                            {
                                if (border.Name == "bd_Fandian1" || border.Name == "bd_Fandian")
                                {
                                    if (transactionPanelViewModel.FandianFlag == "1")
                                    {
                                        border.BorderThickness = new Thickness(1);
                                        border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));
                                    }
                                    else
                                    {
                                        border.BorderThickness = new Thickness(0);
                                    }

                                }
                                else if (border.Name == "bd_settle_2" || border.Name == "bd_settle")
                                {
                                    border.BorderThickness = new Thickness(0);
                                }
                                else
                                {
                                    border.BorderThickness = new Thickness(1);
                                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));
                                }
                            }
                            else
                            {
                                border.BorderThickness = new Thickness(0);
                            }

                        }
                        else
                        {
                            border.BorderThickness = new Thickness(0);

                        }
                    }
                }
            }));

        }

        private void tbPrice_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    AppUtils.IsInnerInput = true;
                    if (e.ClickCount == 2)
                    {
                        e.Handled = true;
                        TextBlock textBlock = sender as TextBlock;

                        (this.DataContext as TransactionPanelViewModel).EditOrderInfo(textBlock, ECommandType.修改价格);
                    }
                }));
            }
        }

        private void tbPriceChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    TextBox tb = sender as TextBox;
                    Regex regex = new Regex("^[0-9.-]+$");
                    e.Handled = !regex.IsMatch(e.Text);

                    if (e.Handled)
                        return;

                    int max = 1000;
                    int min = -1000;
                    string text = tb.Text;

                    if ((text.Contains(".") && e.Text.Equals("."))
                        || (text.Contains("-") && e.Text.Equals("-")))
                    {
                        e.Handled = true;
                        return;
                    }

                    string selectTxt = tb.SelectedText;

                    if (selectTxt.Length == text.Length)
                        return;

                    decimal d = 0;
                    if (decimal.TryParse(text, out d) && (d > max || d < min))
                    {
                        e.Handled = true;
                        tb.Text = text.Remove(tb.Text.Length - 1, 1);
                        tb.SelectionStart = tb.Text.Length;
                        return;
                    }

                    string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 1)
                    {
                        if (decimal.TryParse(arr[1], out d) && arr[1].Length > 8)
                        {
                            e.Handled = true;
                            tb.Text = text.Remove(tb.Text.Length - 1, 1);
                            tb.SelectionStart = tb.Text.Length;
                            return;
                        }
                    }
                    int inputIndex = tb.SelectionStart;
                    text = text.Insert(inputIndex, e.Text);
                }));
            }

        }

        private void tbPriceChange_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.Key == Key.Enter)
                    {
                        e.Handled = true;
                        TextBox tbPriceChange = sender as TextBox;
                        TransactionOrdermModel transaction = tbPriceChange.DataContext as TransactionOrdermModel;
                        (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(transaction);
                        (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo(tbPriceChange.Text, ECommandType.修改价格);

                        var win = Window.GetWindow(this);
                        if (win != null)
                            win.Focus();
                    }
                }));
            }
        }

        private void tbAmount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.ClickCount == 2)
                    {
                        e.Handled = true;
                        TextBlock textBlock = sender as TextBlock;

                        (this.DataContext as TransactionPanelViewModel).EditOrderInfo(textBlock, ECommandType.修改数量);
                    }
                }));
            }
        }

        private void tbAmountChange_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.Key == Key.Enter)
                    {
                        TextBox tbPriceChange = sender as TextBox;
                        TransactionOrdermModel transaction = tbPriceChange.DataContext as TransactionOrdermModel;
                        (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(transaction);
                        (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo(tbPriceChange.Text, ECommandType.修改数量);
                        var win = Window.GetWindow(this);
                        if (win != null)
                            win.Focus();
                    }
                }));
            }
        }

        private void tbAmountChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    AppUtils.IsInnerInput = true;
                    TextBox tb = sender as TextBox;
                    Regex regex = new Regex("^[0-9.]+$");
                    e.Handled = !regex.IsMatch(e.Text);

                    if (e.Handled)
                        return;
                    int max = 2000;
                    int min = 0;
                    string text = tb.Text;

                    if (text.Contains(".") && e.Text.Equals("."))
                    {
                        e.Handled = true;
                        return;
                    }
                    decimal d = 0;
                    if (decimal.TryParse(text, out d) && (d > max || d < min) && string.IsNullOrEmpty(tb.SelectedText))
                    {
                        e.Handled = true;
                        tb.Text = text.Remove(tb.Text.Length - 1, 1);
                        tb.SelectionStart = tb.Text.Length;
                        return;
                    }
                    string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 1)
                    {
                        if (decimal.TryParse(arr[1], out d) && arr[1].Length > 8 && string.IsNullOrEmpty(tb.SelectedText))
                        {
                            e.Handled = true;
                            tb.Text = text.Remove(tb.Text.Length - 1, 1);
                            tb.SelectionStart = tb.Text.Length;
                            return;
                        }
                    }
                    int inputIndex = tb.SelectionStart;
                    text = text.Insert(inputIndex, e.Text);

                }));
            }
        }

        private void tbBrokerA_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                AppUtils.IsInnerInput = true;
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.ClickCount == 2)
                    {
                        e.Handled = true;
                        TextBlock textBlock = sender as TextBlock;

                        (this.DataContext as TransactionPanelViewModel).EditOrderInfo(textBlock, ECommandType.修改BrokerA);
                    }
                }));
            }
        }

        private void seBrokerAChange_AfterSearch(Common.Model.CRM.BrokerInfoPo obj, object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (obj == null)
                {
                    TransactionOrdermModel orderDetailPo = o == null ? null : o as TransactionOrdermModel;
                    (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(orderDetailPo);
                    (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo("", ECommandType.修改BrokerA);
                }
                else
                {
                    TransactionOrdermModel orderDetailPo = o == null ? null : o as TransactionOrdermModel;
                    (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(orderDetailPo);
                    (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo(obj.Idb_key, ECommandType.修改BrokerA);
                }

            }));
        }

        private void seBrokerAChangeB_AfterSearch(Common.Model.CRM.BrokerInfoPo obj, object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (obj == null)
                {
                    TransactionOrdermModel orderDetailPo = o == null ? null : o as TransactionOrdermModel;
                    (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(orderDetailPo);
                    (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo("", ECommandType.修改BrokerB);
                }
                else
                {
                    TransactionOrdermModel orderDetailPo = o == null ? null : o as TransactionOrdermModel;
                    (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo(obj.Idb_key, ECommandType.修改BrokerB);
                }
            }));
        }



        private void tbBrokerB_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                AppUtils.IsInnerInput = true;
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.ClickCount == 2)
                    {
                        e.Handled = true;
                        TextBlock textBlock = sender as TextBlock;

                        (this.DataContext as TransactionPanelViewModel).EditOrderInfo(textBlock, ECommandType.修改BrokerB);
                    }
                }));
            }
        }

        private Popup pop = null;
        private void btn_TraderA_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                AppUtils.IsInnerInput = true;
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pop = (Popup)FindName("tbpop_Seq");
                    (this.DataContext as TransactionPanelViewModel).EditOrderTrader(sender, pop, e);
                }));
            }

        }

        private void seTraderAChange_AfterSearchData(object arg1, object arg2)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                _isDoubleClickGrid = true;
                TransactionOrdermModel orderDetailPo = arg1 as TransactionOrdermModel;
                orderDetailPo.TraderAEditVisible = Visibility.Collapsed;
                orderDetailPo.TraderAShowVisible = Visibility.Visible;
                orderDetailPo.TraderAShowNoEditVisible = Visibility.Visible;

                DataGridDataPo dv = arg2 as DataGridDataPo;
                if (dv != null)
                {
                    if (!(orderDetailPo.TraderID + "" == dv.IdbKey))
                    {
                        (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo(dv.IdbKey, ECommandType.修改TraderA);
                    }
                }
                var win = Window.GetWindow(this);
                if (win != null)
                    win.Focus();
            }));

        }

        TransactionOrdermModel _operation = null;
        private bool _isDoubleClickGrid = true;
        private void ClickCommand(object sender, MouseButtonEventArgs e)
        {
            pop = (Popup)FindName("tbpop_Seq");
            tb_seqTxt.Text = "";
            Button button = sender as Button;
            tb_seqTxt.Tag = button.Tag;
            _operation = button.DataContext as TransactionOrdermModel;
            pop.PlacementTarget = button;
            pop.StaysOpen = false;
            dg_orderList.DataContext = null;
            pop.IsOpen = true;
            tb_seqTxt.Focus();
            e.Handled = true;
        }

        private void tb_seqTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                dg_orderList_MouseUp(null, null);
            }
        }

        private void tb_seqTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                AppUtils.IsInnerInput = true;
                TextBox textBox = sender as TextBox;
                if (this.DataContext != null)
                {
                    (this.DataContext as TransactionPanelViewModel).SearchOrderBySeq(textBox);
                }
            }
            catch
            {

            }
        }

        private void tb_seqTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dg_orderList.SelectedIndex > 0)
                    dg_orderList.SelectedIndex--;
                dg_orderList.ScrollIntoView(dg_orderList.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dg_orderList.SelectedIndex < dg_orderList.Items.Count - 1)
                    dg_orderList.SelectedIndex++;
                dg_orderList.ScrollIntoView(dg_orderList.SelectedItem);
            }
        }

        private void dg_orderList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = this.tb_seqTxt;
            (this.DataContext as TransactionPanelViewModel).ExchangeTrader(textBox);
        }

        private void txt_Fandian_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.Key == Key.Enter)
                    {
                        e.Handled = true;
                        TextBox tbPriceChange = sender as TextBox;
                        TransactionOrdermModel transaction = tbPriceChange.DataContext as TransactionOrdermModel;
                        (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(transaction);
                        (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo(tbPriceChange.Text, ECommandType.修改返点);

                        var win = Window.GetWindow(this);
                        if (win != null)
                            win.Focus();
                    }
                }));
            }
        }

        private void tb_Fandian_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    AppUtils.IsInnerInput = true;
                    if (e.ClickCount == 2)
                    {
                        e.Handled = true;
                        TextBlock textBlock = sender as TextBlock;

                        (this.DataContext as TransactionPanelViewModel).EditOrderInfo(textBlock, ECommandType.修改返点);
                    }
                }));
            }
        }

        private void tb_settle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    AppUtils.IsInnerInput = true;
                    e.Handled = true;
                    TextBlock textBlock = sender as TextBlock;
                    TransactionOrdermModel orderDetailPo = textBlock.DataContext as TransactionOrdermModel;
                    if (orderDetailPo == null)
                        return;
                    if (!(this.DataContext as TransactionPanelViewModel).Check(orderDetailPo))
                        return;
                    (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(orderDetailPo);
                    if (e.ClickCount == 2)
                    {
                        _isDoubleClickGrid = false;
                        SettleUIWin settleWin = new SettleUIWin();
                        settleWin.SetValue(orderDetailPo);
                        settleWin.OnSettyleChange += SettleWin_OnSettyleChange;

                        settleWin.Owner = Application.Current.MainWindow;

                        settleWin.ShowDialog();
                    }
                }));
            }
        }

        private void SettleWin_OnSettyleChange(object arg1, SettlementPo arg2)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                (this.DataContext as TransactionPanelViewModel).EditSettyle(arg1, arg2);

            }));
        }

        private void seTraderBChange_AfterSearch(object arg1, object arg2)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                _isDoubleClickGrid = true;
                TransactionOrdermModel orderDetailPo = arg1 as TransactionOrdermModel;
                orderDetailPo.TraderBEditVisible = Visibility.Collapsed;
                orderDetailPo.TraderBShowVisible = Visibility.Visible;

                DataGridDataPo dv = arg2 as DataGridDataPo;
                if (dv != null)
                {
                    if (!(orderDetailPo.TraderIDB + "" == dv.IdbKey))
                    {
                        (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo(dv.IdbKey, ECommandType.修改TraderB);
                    }
                }
                var win = Window.GetWindow(this);
                if (win != null)
                    win.Focus();
            }));
        }

        private void btn_Confirmed_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Refuse_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tb_sendBid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TextBox tb = sender as TextBox;
                if (tb != null)
                {
                    tb.SelectAll();
                    tb.Focus();
                }
                e.Handled = true;
            }
        }

        private bool IsFromEnter = false;
        private void tb_sendBid_KeyDown(object sender, KeyEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                _isDoubleClickGrid = true;



                //orderDetailPo.TraderAEditVisible = Visibility.Collapsed;
                //orderDetailPo.TraderAShowVisible = Visibility.Visible;

                if (e.Key == Key.Enter)
                {
                    _isDoubleClickGrid = true;
                    TextBox tb = sender as TextBox;
                    TransactionOrdermModel orderDetailPo = tb.DataContext as TransactionOrdermModel;
                    if (orderDetailPo == null)
                        return;
                    (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(orderDetailPo);
                    IsFromEnter = true;
                    //if (!recordDetailDomain.Check(orderDetailPo, OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId)))
                    //{
                    //    Application.Current.MainWindow.Focus();
                    //    return;
                    //}
                    bool needRequest = true;

                    if (tb.Text.Equals(orderDetailPo.BidSendto + ""))
                        needRequest = false;

                    if (needRequest)
                    {
                        (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo(tb.Text.Trim(), ECommandType.修改Bid发给);
                    }


                    IsFromEnter = false;

                    Application.Current.MainWindow.Focus();
                }
            }));

        }

        private void tb_sendOfr_KeyDown(object sender, KeyEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                _isDoubleClickGrid = true;
                if (e.Key == Key.Enter)
                {
                    _isDoubleClickGrid = true;
                    TextBox tb = sender as TextBox;
                    TransactionOrdermModel orderDetailPo = tb.DataContext as TransactionOrdermModel;
                    if (orderDetailPo == null)
                        return;
                    (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(orderDetailPo);
                    IsFromEnter = true;
                    bool needRequest = true;

                    if (tb.Text.Equals(orderDetailPo.OfrSendto + ""))
                        needRequest = false;

                    if (needRequest)
                    {
                        (this.DataContext as TransactionPanelViewModel).UpdateOrderInfo(tb.Text.Trim(), ECommandType.修改Ofr发给);
                    }
                    IsFromEnter = false;
                    Application.Current.MainWindow.Focus();
                }
            }));
        }

        private void tb_sendOfr_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TextBox tb = sender as TextBox;
                if (tb != null)
                {
                    tb.SelectAll();
                    tb.Focus();
                }
                e.Handled = true;
            }
        }

        private void RejectItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnClone_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Reminder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            TransactionOrdermModel orderDetailPo = (this.DataContext as TransactionPanelViewModel).GetSelectedOrder();
            if (orderDetailPo == null)
            {
                LogHelper.Info("UserControl_ContextMenuOpening");
                //_orderDetailPo = null;
                return;
            }
            else
            {
                LogHelper.Info("UserControl_ContextMenuOpening:" + orderDetailPo.SecurityID + "-seq:" + orderDetailPo.Seq);
            }

            (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(orderDetailPo);
            if ((SystemPowerHandler.Instance.IsCanEditOrder(orderDetailPo.BrokerIdA) || SystemPowerHandler.Instance.IsCanEditOrder(orderDetailPo.BrokerIdB))
                && ("G".Equals(orderDetailPo.OrderStatus) ||
                "H".Equals(orderDetailPo.OrderStatus) || "E".Equals(orderDetailPo.OrderStatus) ||
                "F".Equals(orderDetailPo.OrderStatus) || "I".Equals(orderDetailPo.OrderStatus) ||
                "0".Equals(orderDetailPo.OrderStatus) || "1".Equals(orderDetailPo.OrderStatus)
                || "2".Equals(orderDetailPo.OrderStatus)))
                btnDelete.Visibility = Visibility.Visible;
            else
                btnDelete.Visibility = Visibility.Collapsed;
            ///FAK订单不可以删除 20201027 V2.7
            ///线上成交订单不可以删除 20210302 V2.10.0
            if (orderDetailPo.OnlineA == "2" && orderDetailPo.OnlineB == "2")
                btnDelete.Visibility = Visibility.Collapsed;
            ///状态为G且具有订单修改权限的人才可以克隆订单
            if ((orderDetailPo.BrokerIdA == LocalEnv.UserInfo.UserId || orderDetailPo.BrokerIdB == LocalEnv.UserInfo.UserId) && orderDetailPo.OrderStatus + "" == "G")
                btnClone.Visibility = Visibility.Visible;
            else
                btnClone.Visibility = Visibility.Collapsed;

            if (orderDetailPo.OrderStatus.Equals("G"))
            {
                //btnRightReject.Visibility = Visibility.Collapsed;
                btnCopy.Visibility = Visibility.Visible;
                btnSend.Visibility = Visibility.Visible;
                btn_Reminder.Visibility = Visibility.Visible;
            }
            else if (!orderDetailPo.OrderStatus.Equals("E")
                || !LocalEnv.UserInfo.IsMyself(orderDetailPo.BrokerIdA))
            {
                //btnRightReject.Visibility = Visibility.Collapsed;
                btnCopy.Visibility = Visibility.Collapsed;
                btnSend.Visibility = Visibility.Collapsed;
                btn_Reminder.Visibility = Visibility.Collapsed;
            }
            else
            {
                //btnRightReject.Visibility = Visibility.Visible;
                btnCopy.Visibility = Visibility.Collapsed;
                btnSend.Visibility = Visibility.Collapsed;
                btn_Reminder.Visibility = Visibility.Collapsed;
            }

            if (btnSend.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrWhiteSpace(orderDetailPo.BrokerIdB) && string.IsNullOrWhiteSpace(orderDetailPo.BrokerIdA))
                {
                    btnSend.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if ((!string.IsNullOrWhiteSpace(orderDetailPo.BrokerIdB) && LocalEnv.UserInfo.UserId == orderDetailPo.BrokerIdB)
                        || !string.IsNullOrWhiteSpace(orderDetailPo.BrokerIdA) && LocalEnv.UserInfo.UserId == orderDetailPo.BrokerIdA)
                    {
                        btnSend.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (SystemPowerHandler.Instance.IsCanEditOrder(orderDetailPo.BrokerIdB)
                            || SystemPowerHandler.Instance.IsCanEditOrder(orderDetailPo.BrokerIdA))
                        {
                            btnSend.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            btnSend.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }


            //催单按钮是否显示
            if (btn_Reminder.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrWhiteSpace(orderDetailPo.BrokerIdB) || string.IsNullOrWhiteSpace(orderDetailPo.BrokerIdA))
                {
                    btn_Reminder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if ((!string.IsNullOrWhiteSpace(orderDetailPo.BrokerIdA) && LocalEnv.UserInfo.UserId == orderDetailPo.BrokerIdB)
                        || !string.IsNullOrWhiteSpace(orderDetailPo.BrokerIdB) && LocalEnv.UserInfo.UserId == orderDetailPo.BrokerIdA)
                    {
                        btn_Reminder.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btn_Reminder.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void WatermarkTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AppUtils.IsInnerInput = true;
                decimal d = 0;
                if (decimal.TryParse(wt_seq.Text.Trim(), out d))
                {
                    (this.DataContext as TransactionPanelViewModel).SetSeqPosition(wt_seq.Text.Trim());
                }
                else
                {
                    (this.DataContext as TransactionPanelViewModel).SetSeqPosition("0");
                }
                wt_seq.Focus();


            }
        }

        private void WatermarkTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        private void TransactionDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if (dg == null)
            {
                return;
            }

            (this.DataContext as TransactionPanelViewModel).DoubleClickRow(dg.SelectedItem);
        }

        double _width = 600;
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UserControl transaction = sender as UserControl;
                if (transaction == null)
                {
                    return;
                }
                if (Math.Abs(_width -transaction.ActualWidth) > 1.5)
                {
                    (this.DataContext as TransactionPanelViewModel).ExcelChangeSize();
                    _width = transaction.ActualWidth;
                }
            }));
        }

        private void tb_remarks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AppUtils.IsInnerInput = true;

                (this.DataContext as TransactionPanelViewModel).tb_remarks_KeyDown(sender, e);

            }
        }

        private void tb_remarks_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            textBox.Select(textBox.Text.Length, 0);
        }

        private void TransactionDataGrid_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void st_bondEdit_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj, object arg2)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                TransactionOrdermModel transaction = arg2 as TransactionOrdermModel;
                (this.DataContext as TransactionPanelViewModel).SetEditOrderModel(transaction);
                (this.DataContext as TransactionPanelViewModel).UpdateBondInfo(obj);
            }));
        }


        private void TransactionDataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            TransactionDataGrid.UpdateLayout();
        }


        #region 债券搜索框


        #endregion


        private void sp_StatusImage_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                StackPanel stackPanel = sender as StackPanel;
                TransactionOrdermModel transaction = stackPanel.DataContext as TransactionOrdermModel;
                (this.DataContext as TransactionPanelViewModel).SetImageTag(stackPanel, transaction);
            }));
        }
    }
}
