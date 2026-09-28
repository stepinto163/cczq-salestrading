using STC.Common.Data;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame;
using STC.PT.Plugin.View.Component;
using STC.PT.Plugin.ViewModel;
using STC.PT.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoCompleteComboBox = STC.PT.Plugin.View.Component.AutoCompleteComboBox;

namespace STC.PT.Plugin.View.ExecManage
{
    /// <summary>
    /// BatchRecognizeView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchRecognizeView : STC.Common.View.Component.WindowBase
    {
        BatchRecognizeViewModel _recognizeIntentionQuoteViewModel = new BatchRecognizeViewModel();
        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        private readonly string WinIniKey = $"WorkBench_{ LocalEnv.UserInfo.UserName}_RecognizeDeal";
        public BatchRecognizeView()
        {
            InitializeComponent();

            this.DataContext = _recognizeIntentionQuoteViewModel;
            _recognizeIntentionQuoteViewModel.RequestClose += () => this.Close();
            Window1 window;
            if (Application.Current.MainWindow is Window1)
            {
                window = (Window1)Application.Current.MainWindow;
                window.OnCopyMessage_QuoteRecognize += Window_OnCopyMessage_QuoteRecognize;
            }

            this.Loaded += RecognizeDealWin_Loaded;
        }

        private void RecognizeDealWin_Loaded(object sender, RoutedEventArgs e)
        {
            SetExecutionsHeight();
            ReStoreWindow();
            this.SizeChanged += RecognizeDealWin_SizeChanged;
        }
        /// <summary>
        /// 保存窗口大小和位置
        /// </summary>
        private void SaveWindowPos()
        {
            IniFileHelper.WriteValue(WinIniKey, WinHeight, this.ActualHeight + "");
            IniFileHelper.WriteValue(WinIniKey, WinWidth, this.ActualWidth + "");
            IniFileHelper.WriteValue(WinIniKey, PositionX, this.Left + "");
            IniFileHelper.WriteValue(WinIniKey, PositionY, this.Top + "");
        }
        private void RecognizeDealWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowPos();
        }

        /// <summary>
        /// 恢复窗口
        /// </summary>
        private void ReStoreWindow()
        {
            string height = IniFileHelper.ReadValue(WinIniKey, WinHeight);
            string width = IniFileHelper.ReadValue(WinIniKey, WinWidth);

            double d = 0;
            if (double.TryParse(height, out d))
                this.Height = d;
            if (double.TryParse(width, out d))
                this.Width = d;

            string positionX = IniFileHelper.ReadValue(WinIniKey, PositionX);
            string positionY = IniFileHelper.ReadValue(WinIniKey, PositionY);
            double dY = 0;
            if (double.TryParse(positionX, out d) && double.TryParse(positionY, out dY))
            {
                this.Top = dY;
                this.Left = d;
            }
        }
        /// <summary>
        /// 恢复分割线
        /// </summary>
        private async void SetExecutionsHeight()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    string height = IniFileHelper.ReadValue(WinIniKey, "RecognizeIntentionQuoteLeftWidth");
                    double d = 0;
                    if (double.TryParse(height, out d))
                    {
                        if (d > 0)
                        {
                            LeftRow.Width = new GridLength(d);
                        }
                    }
                }));
            });
        }
        private void Window_OnCopyMessage_QuoteRecognize(object obj)
        {
            this.contentTxt.Dispatcher.BeginInvoke((Action)(() => {
                if (obj != null)
                {
                    this.contentTxt.Text = obj.ToString();
                }
                else
                {
                    if (Clipboard.GetDataObject().GetDataPresent(typeof(Bitmap)))
                    {
                        Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(typeof(Bitmap));
                        ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        img.Source = source;
                        img.Tag = ImageHelper.ImageToBase64(bitmap);
                        ImageWaterMark.Visibility = Visibility.Collapsed;

                        ImageTI.IsSelected = true;
                    }
                }

            }));
        }
        public BatchRecognizeView(object obj)
        {
            InitializeComponent();

            this.DataContext = _recognizeIntentionQuoteViewModel;

            Window1 window;
            if (Application.Current.MainWindow is Window1)
            {
                window = (Window1)Application.Current.MainWindow;
                window.OnCopyMessage_QuoteRecognize += Window_OnCopyMessage_QuoteRecognize;
            }

            this.contentTxt.Dispatcher.BeginInvoke((Action)(() =>
            {
                string tmp = Clipboard.GetText().Trim();
                if (!string.IsNullOrEmpty(tmp))
                {
                    this.contentTxt.Text = tmp;
                }
                else
                {
                    if (Clipboard.GetDataObject().GetDataPresent(typeof(Bitmap)))
                    {
                        Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(typeof(Bitmap));
                        ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        img.Source = source;
                        img.Tag = ImageHelper.ImageToBase64(bitmap);
                        ImageWaterMark.Visibility = Visibility.Collapsed;

                        ImageTI.IsSelected = true;
                    }
                }
            }));

            this.Loaded += RecognizeDealWin_Loaded;
        }

        private void TabControl_DragEnter(object sender, DragEventArgs e)
        {
            if (this.ImageTI.IsSelected)
            {
                e.Effects = DragDropEffects.None;
                e.Effects = DragDropEffects.Copy;
                //if (e.Data.GetDataPresent(DataFormats.FileDrop))
                //{
                //    e.Effects = DragDropEffects.Copy;
                //}
                //else
                //{
                //    e.Effects = DragDropEffects.Copy;
                //}
            }
        }

        private void TabControl_Drop(object sender, DragEventArgs e)
        {
            if (this.ImageTI.IsSelected)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                    Bitmap bitmap = new Bitmap(path);
                    ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    img.Source = source;
                    bitmap.Dispose();
                    img.Tag = ImageHelper.DocumentToBase64Str(path);
                    ImageWaterMark.Visibility = Visibility.Collapsed;
                }
                else
                {
                    string[] s = (string[])e.Data.GetData(DataFormats.FileDrop);
                }
            }
        }

        private void img_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.ImageTI.IsSelected)
            {
                if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V))
                {
                    if (Clipboard.GetDataObject().GetDataPresent(typeof(Bitmap)))
                    {
                        Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(typeof(Bitmap));
                        ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        img.Source = source;
                        img.Tag = ImageHelper.ImageToBase64(bitmap);
                        ImageWaterMark.Visibility = Visibility.Collapsed;
                    }
                }
                else if (e.Key == Key.Delete)
                {
                    img.Source = null;
                    img.Tag = null;
                    ImageWaterMark.Visibility = Visibility.Visible;
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (this.textTI.IsSelected)
            {
                this.contentTxt.Text = string.Empty;
            }
            else if (this.ImageTI.IsSelected)
            {
                this.img.Source = null;
                ImageWaterMark.Visibility = Visibility.Visible;
            }
        }

        private void btnRecognize_Click(object sender, RoutedEventArgs e)
        {
            if (textTI.IsSelected)
            {
                _recognizeIntentionQuoteViewModel.RecognizeIntentionQuote(this.contentTxt.Text);
            }
            else if (ImageTI.IsSelected)
            {
                string content = this.img.Tag?.ToString();
                _recognizeIntentionQuoteViewModel.RecognizeIntentionQuoteImage(content);
            }
        }

        private void gridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            double hei = LeftRow.ActualWidth;
            try
            {
                SaveLastExecutionsHeight(hei);
            }
            catch { }
        }
        /// <summary>
        /// 保存分割线
        /// </summary>
        /// <param name="v"></param>
        public void SaveLastExecutionsHeight(double v)
        {
            if (this.DataContext != null && this.DataContext is BatchRecognizeViewModel)
            {
                BatchRecognizeViewModel viewModel = (BatchRecognizeViewModel)this.DataContext;
                IniFileHelper.WriteValue(WinIniKey, "RecognizeDealLeftWidth", v.ToString());
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is IntentionQuoteItem)
            {
                (e.Row.Item as IntentionQuoteItem).No = e.Row.GetIndex() + 1;
            }
        }

        private void DataGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            refreshDataGridRowNumbers(sender);
        }
        // 刷新序号
        private static void refreshDataGridRowNumbers(object sender)
        {
            DataGrid grid = sender as DataGrid;
            if (grid == null)
                return;

            foreach (var item in grid.Items)
            {
                var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null && row.Item is IntentionQuoteItem)
                    (row.Item as IntentionQuoteItem).No = row.GetIndex() + 1;
            }
        }
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null && e.Row.Item is IntentionQuoteItem)
            {
                _recognizeIntentionQuoteViewModel.EditRow(e.Row.Item as IntentionQuoteItem);
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null && e.Row.Item is IntentionQuoteItem)
            {
                _recognizeIntentionQuoteViewModel.SaveRow(e.Row.Item as IntentionQuoteItem, e.Column.Header.ToString());
            }
        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _recognizeIntentionQuoteViewModel.SelectedItem_Recognize = (IntentionQuoteItem)((Button)sender).DataContext;
        }

        private void PriceNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            var nu = sender as NumericUpDown; // 替换为实际类型
            if (nu == null) return;
            nu.EnterEdit();
        }

        private void QuantityNU_edit_Loaded(object sender, RoutedEventArgs e)
        {
            var nu = sender as NumericUpDown; // 替换为实际类型
            if (nu == null) return;
            nu.EnterEdit();
        }

        private void QuantityNU_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                //_recognizeIntentionQuoteViewModel.SaveRow(_recognizeIntentionQuoteViewModel.SelectedItem_Recognize, BatchRecognizeViewModel.COLUMNNAME_QTY);
            }
        }

        private void CheckBox_All_Opposite_Click(object sender, RoutedEventArgs e)
        {
            _recognizeIntentionQuoteViewModel.ClickOppositeCheckAll(_recognizeIntentionQuoteViewModel.IsAllChecked_Opposite);
        }

        private void Selected_Chk_Opposite_Click(object sender, RoutedEventArgs e)
        {
            _recognizeIntentionQuoteViewModel.SetOppositeCheckAll();
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.ClearList();
            }
        }

        private void export_button_Click(object sender, RoutedEventArgs e)
        {
            string errMsg = string.Empty;
            if (_recognizeIntentionQuoteViewModel != null)
            {
                errMsg = _recognizeIntentionQuoteViewModel.GetErrorTxt();

                if (string.IsNullOrEmpty(errMsg))
                {
                    if (columnOrder.Count <= 0)
                    {
                        refreshColumnsOrder();
                    }
                    _recognizeIntentionQuoteViewModel.exportDeal(this,columnOrder);
                }
                else
                {
                    CommonUtils.Prompt2Second("确认", $"确认失败：{errMsg}", this);
                }
            }
        }

        private void CopyCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null)
            {
                foreach (IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
                {
                    sb.AppendLine(item.SecurityId);
                }
            }

            ClipboardHelper.CopyToClipboard(sb.ToString(), sb.ToString());
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.UnregisterEvent();
                this.Loaded -= RecognizeDealWin_Loaded;
                SaveWindowPos();
            }
        }

        private void InsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AutoCompleteComboBox box = sender as AutoCompleteComboBox;
            if (box != null)
            {
                if (string.IsNullOrEmpty(box.Text))
                {
                    box.ExpandOptions();
                }
            }
        }

        private void SelectedInstitutionCB_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            AutoCompleteComboBox box = sender as AutoCompleteComboBox;
            if (box != null)
            {
                if (_recognizeIntentionQuoteViewModel != null)
                {
                    _recognizeIntentionQuoteViewModel.FilterInstitution(args);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ("1".Equals(_recognizeIntentionQuoteViewModel.SelectedItem_Recognize.DealSide)) 
            {
                _recognizeIntentionQuoteViewModel.SelectedItem_Recognize.DealSide = "2";
            }
            else
            {
                _recognizeIntentionQuoteViewModel.SelectedItem_Recognize.DealSide = "1";
            }
        }

        private void copy_button_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                if (columnOrder.Count <= 0)
                {
                    refreshColumnsOrder();
                }
                _recognizeIntentionQuoteViewModel.CopyDeal(columnOrder);
            }
        }
        private List<DataGridColumn> columnOrder = new List<DataGridColumn>();
        private void refreshColumnsOrder() 
        {
            columnOrder.Clear();
            dg.Dispatcher.Invoke(() =>
            {
                var sortedColumns = dg.Columns.OrderBy(c => c.DisplayIndex);
                foreach (DataGridColumn column in dg.Columns)
                {
                    if ("操作".Equals(column.Header) || "序号".Equals(column.Header) || "我方方向".Equals(column.Header) || "发单方式".Equals(column.Header) || "对方机构".Equals(column.Header))
                    {
                        continue;
                    }
                    columnOrder.Add(column);
                }
            });
        }

        private void dg_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            // 获取列的最新显示顺序
            columnOrder.Clear();
            dataGrid.Dispatcher.Invoke(() =>
            {
                var sortedColumns = dataGrid.Columns.OrderBy(c => c.DisplayIndex);
                foreach (DataGridColumn column in sortedColumns)
                {
                    if ("操作".Equals(column.Header) || "序号".Equals(column.Header) || "我方方向".Equals(column.Header) || "成交方式".Equals(column.Header) || "对方机构".Equals(column.Header))
                    {
                        continue;
                    }
                    columnOrder.Add(column);
                }
            });
        }

        private void YieldtoexecutionNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            var nu = sender as NumericUpDown; // 替换为实际类型
            if (nu == null) return;
            nu.EnterEdit();
        }

        private void TxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            if ((tb.Name == "txPrice") || (tb.Name == "txExePrice") ||
                (tb.Name == "txFullPrice"))
            {
                tag = "2";
            }
            else if (tb.Name == "txPaymentRate")
            {
                tag = "5";
            }
            else if ((tb.Name == "txNetPrice"))
            {
                tag = "7";
            }
            else
            {
                tag = tb.Tag as string;
            }

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4" || tag == "5" || tag == "8")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2" || tag == "6" || tag == "7")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;



            decimal max = 1000;
            decimal min = -1;

            if (tag == "2" || tag == "7")//价格
            {
                max = 9999;
                min = -100;
            }
            else if (tag == "5")
            {
                max = 100;
                min = 1;
            }
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }
            decimal d = 0;
            string selectTxt = tb.SelectedText;

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

                if (tag == "1" || tag == "5" || tag == "8") //量
                {
                    if (text != d.ToString())
                    {
                        e.Handled = true;
                        return;
                    }

                }
            }

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
                if (_recognizeIntentionQuoteViewModel!= null && 
                    _recognizeIntentionQuoteViewModel.SelectedItem_Recognize != null && 
                    !string.IsNullOrEmpty(_recognizeIntentionQuoteViewModel.SelectedItem_Recognize.SecurityId) &&
                    _recognizeIntentionQuoteViewModel.SelectedItem_Recognize.SecurityId.EndsWith(".SH"))
                {
                    if ("7".Equals(tag) && decimal.TryParse(arr[1], out d) && arr[1].Length > 3)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }


            e.Handled = false;
        }

        private void TxNetPrice_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            // 增加 ↑↓来改变价格
            if (tBox.Name.ToString() == "txPrice" || tBox.Name.ToString() == "txNetPrice" ||
                tBox.Name.ToString() == "txFullPrice" )
            {
                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    e.Handled = true;
                    return;
                }
            }
            else if (tBox.Name.ToString() == "txQty")
            {

                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    e.Handled = true;
                    return;
                }
            }

            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.KeyboardDevice.IsKeyDown(Key.Enter)))
            {
                e.Handled = true;
                return;
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
                if (tBox.Name == "txQty")
                {
                    tag = "1";
                }
                else if ((tBox.Name == "txPrice") || (tBox.Name == "txNetPrice"))
                {
                    tag = "2";
                }
                else
                {
                    tag = tBox.Tag as string;
                }
                int max = 1000;
                int min = 0;

                if (tag == "1")//量
                {
                    max = 1000;
                    min = 0;
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
                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}
