using STC.Common.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// IRSRecognizeIntentionQuoteWin.xaml 的交互逻辑
    /// </summary>
    public partial class IRSRecognizeIntentionQuoteWin : WindowBase
    {
        IRSRecognizeIntentionQuoteViewModel _recognizeIntentionQuoteViewModel = new IRSRecognizeIntentionQuoteViewModel();
        public IRSRecognizeIntentionQuoteWin()
        {
            InitializeComponent();

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

            _recognizeIntentionQuoteViewModel.myWindow = this;
            this.DataContext = _recognizeIntentionQuoteViewModel;

            //Window1 window;
            //if (Application.Current.MainWindow is Window1)
            //{
            //    window = (Window1)Application.Current.MainWindow;
            //    window.OnCopyMessage_QuoteRecognize += Window_OnCopyMessage_QuoteRecognize;
            //}

            this.Loaded += RecognizeIntentionQuoteWin_Loaded;

        }

        public IRSRecognizeIntentionQuoteWin(object obj)
        {
            InitializeComponent();


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

            _recognizeIntentionQuoteViewModel.myWindow = this;
            this.DataContext = _recognizeIntentionQuoteViewModel;

            //Window1 window;
            //if (Application.Current.MainWindow is Window1)
            //{
            //    window = (Window1)Application.Current.MainWindow;
            //    window.OnCopyMessage_QuoteRecognize += Window_OnCopyMessage_QuoteRecognize;
            //}

            this.contentTxt.Dispatcher.BeginInvoke((Action)(() => {
                string tmp = Clipboard.GetText().Trim();
                this.contentTxt.Text = tmp;
            }));

            this.Loaded += RecognizeIntentionQuoteWin_Loaded;
        }

        private void PriceNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void SelectedInstitutionCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.FilterInstitution(args);
            }
        }

        private void SelectedUserIDCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
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
                _recognizeIntentionQuoteViewModel.SaveRow(e.Row.Item as IntentionQuoteItem, e.Column.Header.ToString());
            }
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            //string errMsg = string.Empty;
            //if (_recognizeIntentionQuoteViewModel != null)
            //{
            //    errMsg = _recognizeIntentionQuoteViewModel.GetErrorTxt();

            //    if (string.IsNullOrEmpty(errMsg))
            //    {
            //        _recognizeIntentionQuoteViewModel.UpdateOrder(this);
            //    }
            //    else
            //    {
            //        CommonUtils.Prompt2Second("确认", $"确认失败：{errMsg}", this);
            //    }
            //}
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

        private void QuantityNU_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.SaveRow(_recognizeIntentionQuoteViewModel.SelectedItem_Recognize, "报量");
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
                _recognizeIntentionQuoteViewModel.TraderName_Edit = string.Empty;
            }
        }

        private void InsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(InsNameCb_edit.Text))
            {
                InsNameCb_edit.ExpandOptions();
            }
        }

        private void TrdNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(TrdNameCb_edit.Text))
            {
                TrdNameCb_edit.ExpandOptions();
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

        private void SelectedClientType_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
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
        private readonly string WinIniKey = $"WorkBench_{ LocalEnv.UserInfo.UserName}_IRSRecognizeIntentionQuote";
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
            if (this.DataContext != null && this.DataContext is IRSRecognizeIntentionQuoteViewModel)
            {
                IRSRecognizeIntentionQuoteViewModel viewModel = (IRSRecognizeIntentionQuoteViewModel)this.DataContext;
                IniFileHelper.WriteValue(WinIniKey, "IRSRecognizeIntentionQuoteLeftWidth", v.ToString());
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
                    string height = IniFileHelper.ReadValue(WinIniKey, "IRSRecognizeIntentionQuoteLeftWidth");
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

        private void BrokerCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(BrokerCb_edit.Text))
            {
                BrokerCb_edit.ExpandOptions();
            }
        }

        private void BrokerCb_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.FilterBrokerType(args);
            }
        }

        private void WindowBase_Activated(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                contentTxt.Focus();
            });
        }
    }
}
