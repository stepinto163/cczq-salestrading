using BDB.Price.Plugin.ViewModel.Data;
using BDB.Price.Plugin.ViewModel.Trade;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.View.Trade;
using STC.Common.Utils;
using STC.Common.View.Component;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;

namespace BDB.Price.Plugin.View.Trade
{
    /// <summary>
    /// RecognizeIntentionQuoteWin.xaml 的交互逻辑
    /// </summary>
    public partial class RecognizeIntentionQuoteWin : WindowBase
    {
        RecognizeIntentionQuoteViewModel _recognizeIntentionQuoteViewModel = new RecognizeIntentionQuoteViewModel();
        public RecognizeIntentionQuoteWin()
        {
            InitializeComponent();

            DataObject.AddPastingHandler(contentTxt, OnPaste);

            this.DataContext = _recognizeIntentionQuoteViewModel;
            this.contentTxt.Dispatcher.BeginInvoke((Action)(() => {
                string tmp = Clipboard.GetText().Trim();
                this.contentTxt.Text = tmp;

                if (Clipboard.GetDataObject().GetDataPresent(typeof(Bitmap)))
                {
                    Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(typeof(Bitmap));
                    ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    img.Source = source;
                    img.Tag = ImageHelper.ImageToBase64(bitmap);
                }
            }));
            this.Loaded += RecognizeIntentionQuoteWin_Loaded;
        }

        public RecognizeIntentionQuoteWin(object obj)
        {
            InitializeComponent();

            DataObject.AddPastingHandler(contentTxt, OnPaste);

            this.DataContext = _recognizeIntentionQuoteViewModel;

            this.contentTxt.Dispatcher.BeginInvoke((Action)(() => {
                string tmp = Clipboard.GetText().Trim();
                this.contentTxt.Text = tmp;

                if (Clipboard.GetDataObject().GetDataPresent(typeof(Bitmap)))
                {
                    Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(typeof(Bitmap));
                    ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    img.Source = source;
                    img.Tag = ImageHelper.ImageToBase64(bitmap);
                }
            }));
            this.Loaded += RecognizeIntentionQuoteWin_Loaded;
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            //var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            //if (!isText) return;

            //var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            this.svTxt.Dispatcher.BeginInvoke((Action)(() => {
                this.contentTxt.SelectionStart = 0;
                this.svTxt.ScrollToHorizontalOffset(0);
                this.svTxt.ScrollToHome();
            }));
           
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

                        ImageTI.IsSelected = true;
                    }
                }

            }));
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
            this.contentTxt.Focus();
        }


        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _recognizeIntentionQuoteViewModel.SelectedItem_Recognize = (IntentionQuoteItem)((Button)sender).DataContext;
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            //string errMsg = string.Empty;
            if (_recognizeIntentionQuoteViewModel != null)
            {
                //    errMsg = _recognizeIntentionQuoteViewModel.GetErrorTxt();

                //    if (string.IsNullOrEmpty(errMsg))
                //    {
                //        _recognizeIntentionQuoteViewModel.UpdateOrder(this);
                //    }
                //    else
                //    {
                //        CommonUtils.Prompt2Second("确认", $"确认失败：{errMsg}", this);
                //    }
                _recognizeIntentionQuoteViewModel.OnOkClick();
            }

            this.Close();
        }
        public void SetGroupId(string groupId)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.GroupId = groupId;
            }
        }

        private void DataGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            refreshDataGridRowNumbers(sender);
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is IntentionQuoteItem)
            {
                (e.Row.Item as IntentionQuoteItem).No = e.Row.GetIndex() + 1;
            }
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

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                this.Loaded -= RecognizeIntentionQuoteWin_Loaded;
                SaveWindowPos();
            }
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.ClearList();
            }
        }

        #region 窗口和分割线处理
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecognizeIntentionQuoteWin_Loaded(object sender, RoutedEventArgs e)
        {
            SetExecutionsHeight();
            ReStoreWindow();
            this.SizeChanged += RecognizeIntentionQuoteWin_SizeChanged;
            this.contentTxt.Focus();
        }

        /// <summary>
        /// 窗口保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecognizeIntentionQuoteWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowPos();
        }

        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        private readonly string WinIniKey = $"WorkBench_{ LocalEnv.UserInfo.UserName}_RecognizeIntentionQuote";
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
                bool isContain = false;
                foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                {
                    if (sc.WorkingArea.Contains(new System.Drawing.Point((int)d, (int)dY)))
                    {
                        isContain = true;
                        break;
                    }
                }

                if (isContain)
                {
                    this.Top = dY;
                    this.Left = d;
                }
            }
        }


        /// <summary>
        /// 分割线完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            if (this.DataContext != null && this.DataContext is RecognizeIntentionQuoteViewModel)
            {
                RecognizeIntentionQuoteViewModel viewModel = (RecognizeIntentionQuoteViewModel)this.DataContext;
                IniFileHelper.WriteValue(WinIniKey, "RecognizeIntentionQuoteLeftWidth", v.ToString());
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

        #endregion

        private void TabControl_DragEnter(object sender, DragEventArgs e)
        {
            if (this.ImageTI.IsSelected)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.Copy;
                }
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
                    //img.Tag = ImageHelper.ImageToBase64(bitmap);
                    img.Tag = ImageHelper.DocumentToBase64Str(path);
                }
                else
                {
                    string[] s = (string[])e.Data.GetData(DataFormats.FileDrop);
                    //img.ImageLocation = s[0];
                    //_PicPath1 = s[0];
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
                    }
                }
                else if (e.Key == Key.Delete)
                {
                    img.Source = null;
                    img.Tag = null;
                }
            }
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is RecognizeIntentionQuoteViewModel)
            {
                RecognizeIntentionQuoteViewModel viewModel = (RecognizeIntentionQuoteViewModel)this.DataContext;
                viewModel.ClickChkBox();
            }
        }

        private async void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            // 添加组
            AddGroupWindow addGroupWindow = new AddGroupWindow();
            addGroupWindow.Owner = Application.Current.MainWindow;
            addGroupWindow.ShowDialog();
            if (addGroupWindow.Result)
            {
                if (addGroupWindow.Result == true)
                {
                    HomePageDataHandler.Instance.AddGroup(addGroupWindow.GroupName);
                }
            }

            await Task.Run(() => { System.Threading.Thread.Sleep(200); });

            // 刷新下拉框
            if (this.DataContext != null && this.DataContext is RecognizeIntentionQuoteViewModel)
            {
                RecognizeIntentionQuoteViewModel viewModel = (RecognizeIntentionQuoteViewModel)this.DataContext;
                viewModel.LoadGroupList(addGroupWindow.GroupName);
            }
        }
    }
}
