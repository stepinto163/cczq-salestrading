using STC.Common.Data;
using STC.Common.View.Component;
using STC.WorkTable.Plugin.Utils.Data;
using STC.WorkTable.Plugin.ViewModel.Data;
using STC.WorkTable.Plugin.ViewModel.WorkTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace STC.WorkTable.Plugin.View.WorkTable
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
            this.DataContext = _recognizeIntentionQuoteViewModel;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.contentTxt.Text = string.Empty;
            //if (_recognizeIntentionQuoteViewModel != null)
            //{
            //    _recognizeIntentionQuoteViewModel.ClearList();
            //}
        }

        private void btnRecognize_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                //_recognizeIntentionQuoteViewModel.ClearList();
                _recognizeIntentionQuoteViewModel.RecognizeIntentionQuote(this.contentTxt.Text);
                //IntentionHandler.Instance.RecognizeIntentionQuote(this.contentTxt.Text);
                //this.Close();
            }
        }

        private void QuantityNU_edit_Loaded(object sender, RoutedEventArgs e)
        {

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
                _recognizeIntentionQuoteViewModel.FilterTrader(args);
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
                if (e.Column.Header.ToString() != "报量")
                    _recognizeIntentionQuoteViewModel.SaveRow(e.Row.Item as IntentionQuoteItem, e.Column.Header.ToString());
            }
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.UpdateOrder();
            }
            Close();
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
            }
        }
        private void QuantityNU_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_recognizeIntentionQuoteViewModel != null)
            {
                _recognizeIntentionQuoteViewModel.SaveRow(_recognizeIntentionQuoteViewModel.SelectedItem_Recognize, "报量");
            }
        }
    }
}
