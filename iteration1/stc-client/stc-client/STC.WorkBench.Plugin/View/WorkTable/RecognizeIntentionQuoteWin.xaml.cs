using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame;
using STC.WorkBench.Plugin.Utils.Data;
using STC.WorkBench.Plugin.ViewModel.Data;
using STC.WorkBench.Plugin.ViewModel.WorkTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using static STC.Common.View.Component.AutoCompleteComboBox;

namespace STC.WorkBench.Plugin.View.WorkTable
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

            if (!LocalEnv.ConfInfo.IsRiskCheck_Sell)
            {
                this.dg.Columns.Remove(RiskNoteColumn);
                this.dg.Columns.Add(RiskNoteColumn);
            }

            DataObject.AddPastingHandler(contentTxt, OnPaste);

            ObservableCollection<OptionItem> items = new ObservableCollection<OptionItem>();
            items.Add(new OptionItem("银行间", "银行间"));
            items.Add(new OptionItem("上交", "上交"));
            items.Add(new OptionItem("深交", "深交"));
            exchangeFilter.ItemsSource = items;

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                DataGridTextColumn commandNoteCol = new DataGridTextColumn();
                commandNoteCol.Header = "指令备注";
                commandNoteCol.Width = 80;
                commandNoteCol.MinWidth = 20;
                commandNoteCol.MaxWidth = 500;
                Binding data_note = new Binding();
                data_note.Path = new PropertyPath("CommandNote");
                commandNoteCol.Binding = data_note;

                this.dg.Columns.Insert(1, commandNoteCol);

                commandNote.Visibility = Visibility.Visible;


                cbClientType.Visibility = Visibility.Collapsed;
                clientTB.Visibility = Visibility.Collapsed;
                InsNameCb_edit.Visibility = Visibility.Collapsed;
                TrdNameCb_edit.Visibility = Visibility.Collapsed;
                nf_button.Visibility = Visibility.Collapsed;

                this.BrokerTextBox.Visibility = Visibility.Collapsed;
                this.BrokerCb_edit.Visibility = Visibility.Collapsed;

                if (CommonUtils.IsInvestor())
                {
                    StcTbookUsers stcTbookUsers = CommonUtils.GetInvestors().FirstOrDefault(a => a.Username == LocalEnv.UserInfo.UserName);
                    if (stcTbookUsers != null)
                    {
                        _recognizeIntentionQuoteViewModel.SelectedInvestor_Edit = stcTbookUsers;
                    }
                    Investor_edit.IsEnabled = false;
                    _recognizeIntentionQuoteViewModel.Investor_Edit = LocalEnv.UserInfo.FullName;
                }
                else
                {
                    StcTbookUsers stcTbookUsers = CommonUtils.GetTraders().FirstOrDefault(a => a.Username == LocalEnv.UserInfo.UserName);
                    if (stcTbookUsers != null)
                    {
                        _recognizeIntentionQuoteViewModel.SelectedTrader_Buy_Edit = stcTbookUsers;
                    }
                    //TraderCb_edit.IsEnabled = false;
                    _recognizeIntentionQuoteViewModel.Trader_Buy_Edit = LocalEnv.UserInfo.FullName;
                }
            }

            if (LocalEnv.ConfInfo.HasSendDemandBtn_ParseView == "0")
            {
                this.demand_button.Visibility = Visibility.Collapsed;
            }
            if (LocalEnv.ConfInfo.HasCopyBtn_ParseView == "0")
            {
                this.copy_button.Visibility = Visibility.Collapsed;
            }

            if (!IntentionOldHandler.Instance.IsOrderOther)
            {
                this.BrokerCb_edit.IsEnabled = false;
            }

            if (!IntentionOldHandler.Instance.isSALER())
            {
                clientTB.Visibility = Visibility.Collapsed;
                InsNameCb_edit.Visibility = Visibility.Collapsed;
                TrdNameCb_edit.Visibility = Visibility.Collapsed;
                tagGrid.Visibility = Visibility.Collapsed;
            }

            this.DataContext = _recognizeIntentionQuoteViewModel;

            Window1 window;
            if (Application.Current.MainWindow is Window1)
            {
                window = (Window1)Application.Current.MainWindow;
                window.OnCopyMessage_QuoteRecognize += Window_OnCopyMessage_QuoteRecognize;
            }

            this.Loaded += RecognizeIntentionQuoteWin_Loaded;

        }

        public RecognizeIntentionQuoteWin(object obj)
        {
            InitializeComponent();

            if (!LocalEnv.ConfInfo.IsRiskCheck_Sell)
            {
                this.dg.Columns.Remove(RiskNoteColumn);
                this.dg.Columns.Add(RiskNoteColumn);
            }

            DataObject.AddPastingHandler(contentTxt, OnPaste);

            ObservableCollection<OptionItem> items = new ObservableCollection<OptionItem>();
            items.Add(new OptionItem("银行间", "银行间"));
            items.Add(new OptionItem("上交", "上交"));
            items.Add(new OptionItem("深交", "深交"));
            exchangeFilter.ItemsSource = items;

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                InvestorTB.Visibility = Visibility.Visible;
                TraderTB.Visibility = Visibility.Visible;
                Investor_edit.Visibility = Visibility.Visible;
                TraderCb_edit.Visibility = Visibility.Visible;

                DataGridTextColumn commandNoteCol = new DataGridTextColumn();
                commandNoteCol.Header = "指令备注";
                commandNoteCol.Width = 80;
                commandNoteCol.MinWidth = 20;
                commandNoteCol.MaxWidth = 500;
                Binding data_note = new Binding();
                data_note.Path = new PropertyPath("CommandNote");
                commandNoteCol.Binding = data_note;

                this.dg.Columns.Insert(1, commandNoteCol);

                commandNote.Visibility = Visibility.Visible;


                cbClientType.Visibility = Visibility.Collapsed;
                clientTB.Visibility = Visibility.Collapsed;
                InsNameCb_edit.Visibility = Visibility.Collapsed;
                TrdNameCb_edit.Visibility = Visibility.Collapsed;
                nf_button.Visibility = Visibility.Collapsed;
                vipChk.Visibility = Visibility.Collapsed;

                this.BrokerTextBox.Visibility = Visibility.Collapsed;
                this.BrokerCb_edit.Visibility = Visibility.Collapsed;

                if (CommonUtils.IsInvestor())
                {
                    StcTbookUsers stcTbookUsers = CommonUtils.GetInvestors().FirstOrDefault(a => a.Username == LocalEnv.UserInfo.UserName);
                    if (stcTbookUsers != null)
                    {
                        _recognizeIntentionQuoteViewModel.SelectedInvestor_Edit = stcTbookUsers;
                    }
                    Investor_edit.IsEnabled = false;
                    _recognizeIntentionQuoteViewModel.Investor_Edit = LocalEnv.UserInfo.FullName;
                }
                else
                {
                    StcTbookUsers stcTbookUsers = CommonUtils.GetTraders().FirstOrDefault(a => a.Username == LocalEnv.UserInfo.UserName);
                    if (stcTbookUsers != null)
                    {
                        _recognizeIntentionQuoteViewModel.SelectedTrader_Buy_Edit = stcTbookUsers;
                    }
                    //TraderCb_edit.IsEnabled = false;
                    _recognizeIntentionQuoteViewModel.Trader_Buy_Edit = LocalEnv.UserInfo.FullName;
                }
            }

            //自动识别
            if (obj != null)
            {
                if (obj.GetType() == typeof(bool))
                {
                    _recognizeIntentionQuoteViewModel.IsAutoPrase = true;
                }
            }

            if (LocalEnv.ConfInfo.HasSendDemandBtn_ParseView == "0")
            {
                this.demand_button.Visibility = Visibility.Collapsed;
            }
            if (LocalEnv.ConfInfo.HasCopyBtn_ParseView == "0")
            {
                this.copy_button.Visibility = Visibility.Collapsed;
            }

            if (!IntentionOldHandler.Instance.IsOrderOther)
            {
                this.BrokerCb_edit.IsEnabled = false;
            }

            if (!IntentionOldHandler.Instance.isSALER())
            {
                clientTB.Visibility = Visibility.Collapsed;
                InsNameCb_edit.Visibility = Visibility.Collapsed;
                TrdNameCb_edit.Visibility = Visibility.Collapsed;
                tagGrid.Visibility = Visibility.Collapsed;
            }

            this.DataContext = _recognizeIntentionQuoteViewModel;

            Window1 window;
            if (Application.Current.MainWindow is Window1)
            {
                window = (Window1)Application.Current.MainWindow;
                window.OnCopyMessage_QuoteRecognize += Window_OnCopyMessage_QuoteRecognize;
            }

            this.contentTxt.Dispatcher.Invoke((Action)(() => {
                string tmp = getClipboardText();
                if (!string.IsNullOrEmpty(tmp))
                {
                    this.contentTxt.Text = tmp;
                }
                else
                {
                    if (_recognizeIntentionQuoteViewModel.ParserImageVisibility == Visibility.Visible)
                    {
                        if (Clipboard.ContainsFileDropList())
                        {
                            string[] filePaths = (string[])Clipboard.GetData(DataFormats.FileDrop);
                            if (filePaths != null && filePaths.Length > 0)
                            {
                                string filePath = filePaths[0];
                                string extension = System.IO.Path.GetExtension(filePath).ToLower();
                                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" ||
                                        extension == ".gif" || extension == ".bmp" || extension == ".tiff")
                                {
                                    img.Tag = ImageHelper.DocumentToBase64Str(filePath);
                                    img.Source = LoadImage(filePath);
                                    btnRecognize_Click(null, null);
                                    ImageWaterMark.Visibility = Visibility.Collapsed;
                                    ImageTI.IsSelected = true;
                                }
                            }
                        }
                        else if (Clipboard.GetDataObject().GetDataPresent(typeof(Bitmap)))
                        {
                            Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(typeof(Bitmap));
                            ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            img.Source = source;
                            img.Tag = ImageHelper.ImageToBase64(bitmap);
                            btnRecognize_Click(null, null);
                            ImageWaterMark.Visibility = Visibility.Collapsed;

                            ImageTI.IsSelected = true;
                        }
                    }
                }
            }));

            this.Loaded += RecognizeIntentionQuoteWin_Loaded;

        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

            _recognizeIntentionQuoteViewModel.RecognizeIntentionQuote(text);
        }

        //剪贴板被占用（每次只能一个程序读取），则会报OpenClipboard HRESULT:0x800401D0 (CLIPBRD_E_CANT_OPEN))错误
        private string getClipboardText()
        {
            for (int i = 0; i < 200; i++)
            {
                try
                {
                    return System.Windows.Clipboard.GetText();
                }
                catch
                {
                    System.Threading.Thread.Sleep(10);//这句加不加都没关系
                }
            }
            return string.Empty;
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
                    if (ImageTI.Visibility == Visibility.Visible)
                    {
                        if (Clipboard.ContainsFileDropList())
                        {
                            string[] filePaths = (string[])Clipboard.GetData(DataFormats.FileDrop);
                            if (filePaths != null && filePaths.Length > 0)
                            {
                                string filePath = filePaths[0];
                                string extension = System.IO.Path.GetExtension(filePath).ToLower();
                                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" ||
                                        extension == ".gif" || extension == ".bmp" || extension == ".tiff")
                                {
                                    img.Tag = ImageHelper.DocumentToBase64Str(filePath);
                                    img.Source = LoadImage(filePath);
                                    btnRecognize_Click(null, null);
                                    ImageWaterMark.Visibility = Visibility.Collapsed;
                                    ImageTI.IsSelected = true;
                                }
                            }
                        }
                        else if (Clipboard.GetDataObject().GetDataPresent(typeof(Bitmap)))
                        {
                            Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(typeof(Bitmap));
                            ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            img.Source = source;
                            img.Tag = ImageHelper.ImageToBase64(bitmap);
                            btnRecognize_Click(null, null);
                            ImageWaterMark.Visibility = Visibility.Collapsed;

                            ImageTI.IsSelected = true;
                        }
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
                this.img.Tag = null;
                ImageWaterMark.Visibility = Visibility.Visible;
            }
            //if (_recognizeIntentionQuoteViewModel != null)
            //{
            //    _recognizeIntentionQuoteViewModel.ClearList();
            //}
        }

        bool isClickRecognizeBtn = false;
        private void btnRecognize_Click(object sender, RoutedEventArgs e)
        {
            if (textTI.IsSelected)
            {
                if (!string.IsNullOrEmpty(this.contentTxt.Text))
                {
                    _recognizeIntentionQuoteViewModel.RecognizeIntentionQuote(this.contentTxt.Text);
                }
            }
            else if (ImageTI.IsSelected)
            {
                string content = this.img.Tag?.ToString();
                if (!string.IsNullOrEmpty(content))
                {
                    _recognizeIntentionQuoteViewModel.RecognizeIntentionQuoteImage(content);
                }
            }
            //IntentionOldHandler.Instance.RecognizeIntentionQuote(this.contentTxt.Text);
            //this.Close();
            isClickRecognizeBtn = true;
        }

        private void QuantityNU_edit_Loaded(object sender, RoutedEventArgs e)
        {
            var nu = sender as NumericUpDown; // 替换为实际类型
            if (nu == null) return;
            nu.EnterEdit();
        }

        private void PriceNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            var nu = sender as NumericUpDown; // 替换为实际类型
            if (nu == null) return;
            nu.EnterEdit();
        }

        private void SelectedInstitutionCB_PatternChanged(object sender, AutoCompleteArgs args)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.FilterInstitution(args);
            }
        }

        private void SelectedUserIDCB_PatternChanged(object sender, AutoCompleteArgs args)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                {
                    _recognizeIntentionQuoteViewModel.FilterTraderLocal(args);
                }
                else
                {
                    _recognizeIntentionQuoteViewModel.FilterTrader(args);
                }
            }
        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _recognizeIntentionQuoteViewModel.SelectedItem_Recognize = (IntentionQuoteItem)((Button)sender).DataContext;
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
                IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                string columnName = e.Column.Header.ToString();
                if (columnName == RecognizeIntentionQuoteViewModel.COLUMNNAME_INSTITUTION 
                    || columnName == RecognizeIntentionQuoteViewModel.COLUMNNAME_TRADER
                    || columnName == RecognizeIntentionQuoteViewModel.COLUMNNAME_SIDE
                    || columnName == RecognizeIntentionQuoteViewModel.COLUMNNAME_PRICE
                    || columnName == RecognizeIntentionQuoteViewModel.COLUMNNAME_QTY
                    || columnName == RecognizeIntentionQuoteViewModel.COLUMNNAME_SETTLETYPE)
                {
                    _recognizeIntentionQuoteViewModel.SaveRow(item, e.Column.Header.ToString());
                }

                if (e.Column.Header != null && e.Column.Header.ToString() == RecognizeIntentionQuoteViewModel.COLUMNNAME_SIDE)
                {
                    decimal? temp = item.Price;
                    item.Price = null;
                    item.Price = temp;
                }
            }
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            if (LocalEnv.ConfInfo.RequiredIns == "1" && InsNameCb_edit.Visibility == Visibility.Visible && _recognizeIntentionQuoteViewModel.SelectedInstintuion_Edit == null)
            {
                CommonUtils.Prompt2Second("提示", $"请填写来源方机构", this);
                return;
            }

            string errMsg = string.Empty;
            if (_recognizeIntentionQuoteViewModel != null)
            {
                errMsg = _recognizeIntentionQuoteViewModel.GetErrorTxt();

                if (string.IsNullOrEmpty(errMsg))
                {
                    if (LocalEnv.UserInfo.RoleType ==  RoleType.BUYER_ROLE)
                    {
                        _recognizeIntentionQuoteViewModel.UpdateOrder(this, "BUYER_INTENTION");
                    }
                    else
                    {
                        _recognizeIntentionQuoteViewModel.UpdateOrder(this);
                    }
                }
                else
                {
                    CommonUtils.Prompt2Second("发个需", $"发个需失败：{errMsg}", this);
                }
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
                _recognizeIntentionQuoteViewModel.UnregisterEvent();
                this.Loaded -= RecognizeIntentionQuoteWin_Loaded;
                SaveWindowPos();
            }
        }

        private void QuantityNU_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.SelectedItem_Recognize != null)
            {
                _recognizeIntentionQuoteViewModel.SaveRow(_recognizeIntentionQuoteViewModel.SelectedItem_Recognize, RecognizeIntentionQuoteViewModel.COLUMNNAME_QTY);
            }
        }

        private void PriceNU_Edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.SelectedItem_Recognize != null)
            {
                _recognizeIntentionQuoteViewModel.SaveRow(_recognizeIntentionQuoteViewModel.SelectedItem_Recognize, RecognizeIntentionQuoteViewModel.COLUMNNAME_PRICE);
            }
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.ClearList();
            }
        }

        private void InsNameCb_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel.SelectedInstintuion_Edit == null
                && !string.IsNullOrEmpty(_recognizeIntentionQuoteViewModel.InstitutionName_Edit))
            {
                //_recognizeIntentionQuoteViewModel.InstitutionName_Edit = string.Empty;
            }
        }

        private void TrdNameCb_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel.SelectedTrader_Edit == null 
                && !string.IsNullOrEmpty(_recognizeIntentionQuoteViewModel.TraderName_Edit))
            {
                //_recognizeIntentionQuoteViewModel.TraderName_Edit = string.Empty;
            }
        }

        private void AutoComplateCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AutoCompleteComboBox cb = sender as AutoCompleteComboBox;
            if (cb != null)
            {
                if (string.IsNullOrEmpty(cb.Text))
                {
                    cb.ExpandOptions();
                }
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

        private void CheckBox_All_Same_Click(object sender, RoutedEventArgs e)
        {
             _recognizeIntentionQuoteViewModel.ClickSameCheckAll(_recognizeIntentionQuoteViewModel.IsAllChecked_Same);
        }

        private void Selected_Chk_Same_Click(object sender, RoutedEventArgs e)
        {
            _recognizeIntentionQuoteViewModel.SetSameCheckAll();
        }

        private void ValidMinute_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        private void ValidMinute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = null;
            regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Handled)
                return;

            string tag = tb.Tag as string;
            int max = 500;
            int min = (tag == "0") ? 0 : 1;

            if (tag == "0")// [0-60)
            {
                max = 59;
            }
            else if (tag == "1") // [0-500]
            {
                min = 0;
                max = 500;
            }
            else if (tag == "2") // (0-24]
            {
                max = 24;
            }
            else if (tag == "3") // [0-60)
            {
                min = 0;
                max = 59;
            }
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".")) || (text.Contains("-") && e.Text.Equals("-")))
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
        }

        private void bdTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cbSettle2.IsDropDownOpen = !cbSettle2.IsDropDownOpen;
        }

        private void firm_button_Click(object sender, RoutedEventArgs e)
        {
            string errMsg = _recognizeIntentionQuoteViewModel.GetErrorTxt();

            if (string.IsNullOrEmpty(errMsg))
            {
                _recognizeIntentionQuoteViewModel.DoAllConfirmCommand(this);
            }
            else
            {
                CommonUtils.Prompt2Second("确认", $"firm发布失败：{errMsg}", this);
            }
        }

        private void nf_button_Click(object sender, RoutedEventArgs e)
        {
            if (LocalEnv.ConfInfo.RequiredIns == "1" && InsNameCb_edit.Visibility == Visibility.Visible && _recognizeIntentionQuoteViewModel.SelectedInstintuion_Edit == null)
            {
                CommonUtils.Prompt2Second("提示", $"请填写来源方机构", this);
                return;
            }

            string errMsg = _recognizeIntentionQuoteViewModel.GetErrorTxt();

            if (string.IsNullOrEmpty(errMsg))
            {
                _recognizeIntentionQuoteViewModel.DoAllQuickConfirmCommand(this);
            }
            else
            {
                CommonUtils.Prompt2Second("发内部看板", $"发内部看板失败：{errMsg}", this);
            }
        }

        private void SelectedClientType_PatternChanged(object sender, AutoCompleteArgs args)
        {
            if(_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.FilterClientType(args);
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

            //if (_recognizeIntentionQuoteViewModel.IsAutoPrase)
            //{
                this.contentTxt.Dispatcher.Invoke((Action)(() =>
                {
                    btnRecognize_Click(sender, e);
                }));
            //}
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
                //bool isContain = false;
                //foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                //{
                //    if (sc.WorkingArea.Contains(new System.Drawing.Point((int)d, (int)dY)))
                //    {
                //        isContain = true;
                //        break;
                //    }
                //}

                //if (isContain)
                //{
                    this.Top = dY;
                    this.Left = d;
                //}
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

        private void BrokerCb_edit_PatternChanged(object sender, AutoCompleteArgs args)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.FilterBrokerType(args);
            }
        }

        private void CopyCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null)
            {
                foreach(IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
                {
                    sb.AppendLine(item.SecurityId);
                }
            }

            ClipboardHelper.CopyToClipboard(sb.ToString(), sb.ToString());

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
                    img.Tag = ImageHelper.ImageToBase64(bitmap);
                    img.Tag = ImageHelper.DocumentToBase64Str(path);
                    btnRecognize_Click(null, null);
                    ImageWaterMark.Visibility = Visibility.Collapsed;
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
                    if (Clipboard.ContainsFileDropList())
                    {
                        string[] filePaths = (string[])Clipboard.GetData(DataFormats.FileDrop);
                        if (filePaths != null && filePaths.Length > 0)
                        {
                            string filePath = filePaths[0];
                            string extension = System.IO.Path.GetExtension(filePath).ToLower();
                            if(extension == ".jpg" || extension == ".jpeg" || extension == ".png" ||
                                    extension == ".gif" || extension == ".bmp" || extension == ".tiff")
                            {
                                img.Tag = ImageHelper.DocumentToBase64Str(filePath);
                                img.Source = LoadImage(filePath);
                                btnRecognize_Click(null, null);
                            }
                        }
                    }
                    else if (Clipboard.GetDataObject().GetDataPresent(typeof(Bitmap)))
                    {
                        Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(typeof(Bitmap));
                        ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        img.Source = source;
                        img.Tag = ImageHelper.ImageToBase64(bitmap);
                        btnRecognize_Click(null, null);
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

        private BitmapImage LoadImage(string imagePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit(); // 开始初始化BitmapImage对象
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute); // 设置图片路径
                bitmap.EndInit(); // 结束初始化，加载图片到BitmapImage对象中
                return bitmap;
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
            return null;
        }

        private void contentTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                btnRecognize.Focus();
            }
        }

        private void nf_button_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isClickRecognizeBtn && nf_button.IsEnabled)
            {
                nf_button.Focus();
                isClickRecognizeBtn = false;
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

        private void ClearPriceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null)
            {
                foreach (IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
                {
                    if (item.IsChecked)
                    {
                        item.Price = null;
                    }
                }
            }
        }

        private void ClearQtyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null)
            {
                foreach (IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
                {
                    if (item.IsChecked)
                    {
                        item.Qty = null;
                        item.CopyInfo = GetCopyInfo(item);
                    }
                }
            }
        }

        private void batchQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetCommonQty();
            }
        }

        private void batchQty_LostFocus(object sender, RoutedEventArgs e)
        {
            SetCommonQty();
        }

        private void SetCommonQty()
        {
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null 
                && _recognizeIntentionQuoteViewModel.CommonQty.HasValue)
            {
                foreach (IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
                {
                    if (item.IsChecked)
                    {
                        item.Qty = _recognizeIntentionQuoteViewModel.CommonQty.Value;
                        item.CopyInfo = GetCopyInfo(item);
                    }
                }
            }
        }

        //private void bp_Bid_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        SetBidPrice();
        //    }
        //}

        //private void bp_Bid_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    SetBidPrice();
        //}

        //private void SetBidPrice()
        //{
        //    if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null
        //        && _recognizeIntentionQuoteViewModel.BidBp.HasValue)
        //    {
        //        foreach (IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
        //        {
        //            if (item.CCDCValuation.HasValue)
        //            {
        //                if (item.IsChecked && item.Side == "1")
        //                {
        //                    if (bidOp.SelectedValue.ToString() == "+")
        //                    {
        //                        item.Price = item.CCDCValuation.Value + _recognizeIntentionQuoteViewModel.BidBp.Value / 100;
        //                    }
        //                    else
        //                    {
        //                        item.Price = item.CCDCValuation.Value - _recognizeIntentionQuoteViewModel.BidBp.Value / 100;
        //                    }
        //                }
        //            }
                    
        //        }
        //    }
        //}

        //private void bp_Ofr_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        SetOfrPrice();
        //    }
        //}

        //private void bp_Ofr_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    SetOfrPrice();
        //}

        //private void SetOfrPrice()
        //{
        //    if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null
        //        && _recognizeIntentionQuoteViewModel.OfrBp.HasValue)
        //    {
        //        foreach (IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
        //        {
        //            if (item.CCDCValuation.HasValue)
        //            {
        //                if (item.IsChecked && item.Side == "2")
        //                {
        //                    if (ofrOp.SelectedValue.ToString() == "+")
        //                    {
        //                        item.Price = item.CCDCValuation.Value + _recognizeIntentionQuoteViewModel.OfrBp.Value / 100;
        //                    }
        //                    else
        //                    {
        //                        item.Price = item.CCDCValuation.Value - _recognizeIntentionQuoteViewModel.OfrBp.Value / 100;
        //                    }

        //                }
        //            }
        //        }
        //    }
        //}

        //private void bidOp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    SetBidPrice();
        //}

        //private void ofrOp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    SetOfrPrice();
        //}

        private void demand_button_Click(object sender, RoutedEventArgs e)
        {
            if (LocalEnv.ConfInfo.RequiredIns == "1" && InsNameCb_edit.Visibility == Visibility.Visible && _recognizeIntentionQuoteViewModel.SelectedInstintuion_Edit == null)
            {
                CommonUtils.Prompt2Second("提示", $"请填写来源方机构", this);
                return;
            }

            string errMsg = string.Empty;
            if (_recognizeIntentionQuoteViewModel != null)
            {
                errMsg = _recognizeIntentionQuoteViewModel.GetErrorTxt();

                if (string.IsNullOrEmpty(errMsg))
                {
                    _recognizeIntentionQuoteViewModel.UpdateOrder(this, "DEMAND");
                }
                else
                {
                    CommonUtils.Prompt2Second("发私域池", $"发私域池失败：{errMsg}", this);
                }
            }
        }

        private void PriceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null)
            {
                Button btn = sender as Button;
                string commandParameter = btn.CommandParameter.ToString();

                foreach (IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
                {
                    if (item.IsChecked)
                    {
                        decimal? newPrice = null;
                        if (commandParameter == "中债估值")
                        {
                            decimal? price = null;
                            if (item.ChinaBondExercise != null && item.ChinaBondExercise.HasValue)
                            {
                                price = item.ChinaBondExercise;
                            }
                            else
                            {
                                price = item.ChinaBondDue;
                            }
                             
                            if (price.HasValue && _recognizeIntentionQuoteViewModel.BP.HasValue)
                            {
                                price = price + _recognizeIntentionQuoteViewModel.BP / 100;
                            }
                            newPrice = price;
                        }
                        else if (commandParameter == "中证估值")
                        {
                            decimal? price = null;
                            if (item.CsiBondValuationExercise != null && item.CsiBondValuationExercise.HasValue)
                            {
                                price = item.CsiBondValuationExercise;
                            }
                            else
                            {
                                price = item.CsiBondValuationDue;
                            }
                            if (price.HasValue && _recognizeIntentionQuoteViewModel.BP.HasValue)
                            {
                                price = price + _recognizeIntentionQuoteViewModel.BP / 100;
                            }
                            newPrice = price;
                        }
                        else if (commandParameter == "市场最优")
                        {
                            decimal? price = null;
                            if (item.Side == "1")
                            {
                                price = item.Best_Bid;
                                item.PriceType = item.Best_Bid_PriceType;
                            }
                            else if (item.Side == "2")
                            {
                                price = item.Best_Ofr;
                                item.PriceType = item.Best_Ofr_PriceType;
                            }
                            if (price.HasValue && _recognizeIntentionQuoteViewModel.BP.HasValue)
                            {
                                price = price + _recognizeIntentionQuoteViewModel.BP / 100;
                            }
                            newPrice = price;
                        }
                        else
                        {
                            if (item.Price.HasValue)
                            {
                                newPrice = modifyPrice(item.Price.Value, commandParameter);
                            }
                        }

                        item.Price = newPrice;
                    }
                }
            }
        }

        private decimal modifyPrice(decimal price, string parameter)
        {
            if (parameter == "+5")
            {
                price += 0.05m;
            }
            else if (parameter == "+1")
            {
                price += 0.01m;
            }
            else if (parameter == "+0.5")
            {
                price += 0.005m;
            }
            else if (parameter == "+0.25")
            {
                price += 0.0025m;
            }
            else if (parameter == "-0.25")
            {
                price -= 0.0025m;
            }
            else if (parameter == "-0.5")
            {
                price -= 0.005m;
            }
            else if (parameter == "-1")
            {
                price -= 0.01m;
            }
            if (parameter == "-5")
            {
                price -= 0.05m;
            }
            return price;
        }

        private void QtyBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null)
            {
                foreach (IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
                {
                    if (item.IsChecked)
                    {
                        decimal originalQty = item.Qty.HasValue ? item.Qty.Value : decimal.Zero;
                        decimal newQty = modifyQty(originalQty, btn.CommandParameter.ToString());
                        item.Qty = newQty;
                        item.CopyInfo = GetCopyInfo(item);
                    }
                }
            }
        }

        private string GetCopyInfo(IntentionQuoteItem item)
        {
            string nonfirmStar = string.Empty;
            if (item.FirmFlag == null || item.FirmFlag == "0")
            {
                nonfirmStar = "*";
            }
            StringBuilder copyInfo = new StringBuilder();
            string settlTypeDisplay = ComboSourceUtil.Instance.FindEnum(item.SettlType, "settletype");
            if (item.SettlType != "1" && item.SettlType != "2" && item.SettlType != "2,3")
            {
                if (copyInfo.Length > 0)
                {
                    copyInfo.Append(",");
                }
                copyInfo.Append(settlTypeDisplay);
            }
            if (item.WholeFlag == "1")
            {
                if (copyInfo.Length > 0)
                {
                    copyInfo.Append(",");
                }
                copyInfo.Append("整量");
            }

            string copyText = !item.Qty.HasValue ? "--" : item.Qty.ToString();
            if (nonfirmStar.Length > 0)
            {
                copyText = $"{copyText}{nonfirmStar}";
            }
            if (copyInfo.Length > 0)
            {
                copyText = $"{copyText}({copyInfo})";
            }
            return copyText;
        }

        private decimal modifyQty(decimal qty, string parameter)
        {
            if (parameter == "+5000")
            {
                qty += 5000;
            }
            else if (parameter == "+1000")
            {
                qty += 1000;
            }
            else if (parameter == "-5000")
            {
                qty -= 5000;
            }
            else if (parameter == "-1000")
            {
                qty -= 1000;
            }
            else if (parameter == "5K")
            {
                qty = 5000;
            }
            else if (parameter == "1E")
            {
                qty = 10000;
            }

            if (qty < 0)
            {
                qty = 0;
            }
            else if (qty > 1000000)
            {
                qty = 1000000;
            }

            return qty;
        }

        private void copy_button_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IntentionQuoteItem> selectedItems = _recognizeIntentionQuoteViewModel.ViewSource.View.Cast<IntentionQuoteItem>().Where(x => x.IsChecked == true);

            if (selectedItems != null && selectedItems.Count() > 0)
            {
                if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                   && CommonUtils.CopyDic["COPYMODE"].Count > 0
                   && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
                {
                    this.dg.Sub_CopyData_Single(selectedItems.ToList(), false, "2");
                }
                else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                       && CommonUtils.CopyDic["COPYMODE"].Count > 0
                       && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
                {
                    this.dg.Sub_CopyData_Single(selectedItems.ToList(), false, "3");
                }
                else
                {
                    this.dg.Sub_CopyData_Single(selectedItems.ToList(), false, "1");
                }
            }
        }

        private void TraderCb_edit_PatternChanged(object sender, AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            List<StcTbookUsers> traders = CommonUtils.GetTraders();

            if (_recognizeIntentionQuoteViewModel.SelectedInvestor_Edit != null && LocalEnv.ConfInfo.IsI2T)
            {
                List<string> traderList = AdminHandler.Instance.GetTraderList(_recognizeIntentionQuoteViewModel.SelectedInvestor_Edit.Userid);
                if (traderList != null)
                {
                    list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                }
            }
            else
            {
                if (traders != null)
                {
                    if (!string.IsNullOrEmpty(args.Pattern))
                    {
                        list = traders.Where(d => (d.Username != null && d.Username.Contains(args.Pattern))
                        || (d.Name != null && d.Name.Contains(args.Pattern))).ToList();//多条件筛选，新功能增加
                    }
                    else
                    {
                        list = traders.ToList();
                    }
                }
            }

            args.DataSource = list;
        }

        private void Investor_edit_PatternChanged(object sender, AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            List<StcTbookUsers> investors = CommonUtils.GetInvestors();

            if (_recognizeIntentionQuoteViewModel.SelectedTrader_Buy_Edit != null && LocalEnv.ConfInfo.IsI2T)
            {
                List<string> managerList = AdminHandler.Instance.GetManagerList(_recognizeIntentionQuoteViewModel.SelectedTrader_Buy_Edit.Userid);
                if (managerList != null )
                {
                    list = investors.Where(d => managerList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                }
                
            }
            else
            {
                if (investors != null)
                {
                    if (!string.IsNullOrEmpty(args.Pattern))
                    {
                        list = investors.Where(d => (d.Username != null && d.Username.Contains(args.Pattern))
                        || (d.Name != null && d.Name.Contains(args.Pattern))).ToList();//多条件筛选，新功能增加
                    }
                    else
                    {
                        list = investors.ToList();
                    }
                }
            }

            args.DataSource = list;
        }

        private void cbPriceType_edit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null && _recognizeIntentionQuoteViewModel.IntentionQuoteInfo != null)
            {
                foreach (IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
                {
                    if (item.IsChecked)
                    {
                        if (string.IsNullOrEmpty(_recognizeIntentionQuoteViewModel.PriceType))
                        {
                            // ignore
                        }
                        else if (_recognizeIntentionQuoteViewModel.PriceType == "7") // 行权收益率
                        {
                            BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(item.SecurityId);
                            if (bondsInfoPo != null)
                            {
                                // 含权
                                if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                                {
                                    item.PriceType = "7";
                                }
                                else
                                {
                                    //item.PriceType = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            item.PriceType = _recognizeIntentionQuoteViewModel.PriceType;
                        }
                    }
                }
            }
        }

        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((sender as DataGrid).Name == "dg")
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is IntentionQuoteItem)
                {
                    foreach (IntentionQuoteItem item in e.AddedItems)
                    {
                        item.IsChecked = true;
                    }
                }
            }
            
        }

        private void exchangeChanged(object sender, RoutedEventArgs e)
        {
            FilterOptions fo = sender as FilterOptions;
            _recognizeIntentionQuoteViewModel.FilterExchage(fo.TagsList);
        }

        private void PriceNU_Edit_ValueChanged(object sender, RoutedEventArgs e)
        {
            decimal? price = ((sender as NumericUpDown).DataContext as IntentionQuoteItem).Price;
            decimal? thisPrice = (sender as NumericUpDown).NumericValue;
            if (thisPrice != price)
            {
                ((sender as NumericUpDown).DataContext as IntentionQuoteItem).Price = thisPrice;
            }
        }

        private void SwitchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                foreach(IntentionQuoteItem item in _recognizeIntentionQuoteViewModel.IntentionQuoteInfo)
                {
                    if (item.Side == "1")
                    {
                        item.Side = "2";
                    }
                    else if (item.Side == "2")
                    {
                        item.Side = "1";
                    }
                }

                if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                {
                    _recognizeIntentionQuoteViewModel.BatchCheckRisk();
                }
            }
        }

        private void Sub_SwitchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is Button)
            {
                Button btn = sender as Button;
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem.Side == "1")
                {
                    currentItem.Side = "2";
                }
                else if (currentItem.Side == "2")
                {
                    currentItem.Side = "1";
                }

                if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                {
                    _recognizeIntentionQuoteViewModel.BatchCheckRisk();
                }
            }
        }

        //private void TagCB_FilterDicChanged(object sender, RoutedEventArgs e)
        //{
        //    MultiSelectComboBox mscb = sender as MultiSelectComboBox;
        //    string[] arr = mscb.SelectedValues.Split(CommonConstants.COMMA_CHAR);

        //    foreach (string s in arr)
        //    {
        //        if (s == "hq")
        //        {
        //            _recognizeIntentionQuoteViewModel.ExchangeSignFlag = true;
        //        }
        //        if (s == "ap")
        //        {
        //            _recognizeIntentionQuoteViewModel.Handlinst = "1";
        //        }    
        //        if (s == "fd")//????
        //        {
        //            _recognizeIntentionQuoteViewModel.FanDianFlag = "1";
        //        }

        //    }
        //}
    }
}
