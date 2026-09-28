using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using STC.WorkTable.Plugin.ViewModel.Data;
using STC.WorkTable.Plugin.ViewModel.WorkTable;
using STC.Common.Utils;
using STC.Common.View.Component;
using static STC.WorkTable.Plugin.View.Component.AutoCompleteComboBox;
using STC.WorkTable.Plugin.View.Component;

namespace STC.WorkTable.Plugin.View.WorkTable
{
    /// <summary>
    /// MyOrderDefaultView.xaml 的交互逻辑
    /// </summary>
    public partial class MyOrderDefaultView : UserControl
    {
        public event Action<IntentionQuoteItem> SelectedRowChanged;

        private MyOrderDefaultViewModel _myOrderDefaultViewModel = new MyOrderDefaultViewModel();

        public MyOrderDefaultView()
        {
            InitializeComponent();
            try
            {
                this.DataContext = _myOrderDefaultViewModel;
                this.WorkingDataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.WorkingDataGrid_MouseLeftButtonDown), true);
                this.WorkedDataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.WorkedDataGrid_MouseLeftButtonDown), true);
                this.IgnoredDataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.IgnoredDataGrid_MouseLeftButtonDown), true);
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }



        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.SetMyOrderOperationGridMargin();
        }

        private void SetMyOrderOperationGridMargin()
        {
            //part_horizontalscrollbar
            //scrollbar hscrollbar = commonutils.getvisualchild<scrollbar>(myorders_datagrid, dp => dp.name == "part_horizontalscrollbar");

            //if (hscrollbar != null)
            //{
            //    if (hscrollbar.visibility == visibility.visible)
            //    {
            //        this.myordersoperationgrid.margin = new thickness(0, 0, 0, 10);
            //    }
            //    else
            //    {
            //        this.myordersoperationgrid.margin = new thickness(0);
            //    }
            //}
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                if (_myOrderDefaultViewModel != null)
                {
                    _myOrderDefaultViewModel.CheckBoxAllValueChange(flag);
                }
            }
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.CheckBoxSingleClick();
            }
        }

        private void CheckBox_All_Click_Ignore(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                if (_myOrderDefaultViewModel != null)
                {
                    _myOrderDefaultViewModel.CheckBoxAllValueChange_Ignore(flag);
                }
            }
        }

        private void Selected_Chk_Click_Ignore(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.CheckBoxSingleClick_Ignore();
            }
        }

        private void SelectedInstitutionCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.FilterInstitution(args);
            }
        }


        private void SelectedUserIDCB_PatternChanged(object sender, AutoCompleteArgs args)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.FilterTrader(args);
            }
        }

        private void SelectedBrokerCB_PatternChanged(object sender, AutoCompleteArgs args)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.FilterBroker(args);
            }
        }

        public void DoAllConfirmCommand(string commandText)
        {
            switch (commandText)
            {
                case "F1_Text":
                    if (_myOrderDefaultViewModel != null)
                    {
                        _myOrderDefaultViewModel.DoAllConfirmCommand();
                    }
                    break;
                default:
                    return;
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

        private void SearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                this.WorkingDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                this.WorkedDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                this.IgnoredDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                _myOrderDefaultViewModel.SelectFilterType = ((ComboBoxItem)this.SearchComboBox.SelectedItem).Tag.ToString();
                _myOrderDefaultViewModel.FilterData();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                this.WorkingDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                this.WorkedDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                this.IgnoredDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                _myOrderDefaultViewModel.FilterText = this.SearchTextBox.Text;
                _myOrderDefaultViewModel.FilterData();
            }
        }

        private void WorkingDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
            {
                SelectedRowChanged?.Invoke(_myOrderDefaultViewModel.SelectedItem_Working);
            }
        }
        
        private void WorkedDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Worked != null)
            {
                SelectedRowChanged?.Invoke(_myOrderDefaultViewModel.SelectedItem_Worked);
            }
        }
        
        private void IgnoredDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Ignore != null)
            {
                SelectedRowChanged?.Invoke(_myOrderDefaultViewModel.SelectedItem_Ignore);
            }
        }
        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() == "客户机构")
            {
                if (e.Column is DataGridTemplateColumn)
                {
                    IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                    FrameworkElement element = e.Column.GetCellContent(item);
                    DataGridTemplateColumn templeColumn = e.Column as DataGridTemplateColumn;
                    Object obj = templeColumn.CellEditingTemplate.FindName("InsNameCb_edit", element);
                    
                    if (item.SelectedTrader_Edit != null && obj != null && obj is AutoCompleteComboBox)
                    {
                        AutoCompleteComboBox acc = obj as AutoCompleteComboBox;
                        acc.ExpandOptions();
                    }
                }
            }
            else if (e.Column.Header.ToString() == "客户名")
            {
                if (e.Column is DataGridTemplateColumn)
                {
                    IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                    FrameworkElement element = e.Column.GetCellContent(item);
                    DataGridTemplateColumn templeColumn = e.Column as DataGridTemplateColumn;
                    Object obj = templeColumn.CellEditingTemplate.FindName("TrdNameCb_edit", element);

                    if (item.SelectedInstintuion_Edit != null && obj != null && obj is AutoCompleteComboBox)
                    {
                        AutoCompleteComboBox acc = obj as AutoCompleteComboBox;
                        acc.ExpandOptions();
                    }
                }
            }
        }

        private void WorkingDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && e.Row.Item is IntentionQuoteItem)
            {
                _myOrderDefaultViewModel.EditRow(e.Row.Item as IntentionQuoteItem);
            }
        }

        private void WorkingDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && e.Row.Item is IntentionQuoteItem)
            {
                if (e.Column.Header.ToString() != "报量")
                    _myOrderDefaultViewModel.SaveRow(e.Row.Item as IntentionQuoteItem, e.Column.Header.ToString());
            }
        }

        private void WorkingDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                DataGrid dataGrid = CommonUtils.GetParentObject<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        DataGridRow row = CommonUtils.GetParentObject<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }

        private void WorkedDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && e.Row.Item is IntentionQuoteItem)
            {
                IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                if (item.Status == "2" && !string.IsNullOrEmpty(item.InstitutionName) && !string.IsNullOrEmpty(item.TraderName))
                {
                    e.Cancel = true;
                    return;
                }
                _myOrderDefaultViewModel.EditRow(e.Row.Item as IntentionQuoteItem);
            }
        }

        private void WorkedDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && e.Row.Item is IntentionQuoteItem)
            {
                _myOrderDefaultViewModel.SaveRow(e.Row.Item as IntentionQuoteItem, e.Column.Header.ToString());
            }
        }


        private const string SaveColumnsInfo_IntentionTable = "IntentionColumns";
        public void SaveColumnsInfo()
        {
            // 保存待处理
            IniFileHelper.WriteValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "working", GetColumnsInfo(this.WorkingDataGrid));
            // 保存已处理
            IniFileHelper.WriteValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "worked", GetColumnsInfo(this.WorkedDataGrid));
            // 保存已忽略
            IniFileHelper.WriteValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "ignored", GetColumnsInfo(this.IgnoredDataGrid));
        }

        private string GetColumnsInfo(DataGrid dg)
        {
            StringBuilder saveInfo = new StringBuilder();
            var sortedColumns = dg.Columns.OrderBy(column => column.DisplayIndex);
            foreach (DataGridColumn column in sortedColumns)
            {
                if (saveInfo.Length > 0)
                {
                    saveInfo.Append(";");
                }
                saveInfo.Append($"{column.Header}:{column.ActualWidth}");
            }
            return saveInfo.ToString();
        }

        public void LoadColumnsInfo()
        {
            // 加载待处理
            string workingSaveInfo = IniFileHelper.ReadValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "working");
            SetColumnsInfo(workingSaveInfo, this.WorkingDataGrid);
            // 加载已处理
            string workedSaveInfo = IniFileHelper.ReadValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "worked");
            SetColumnsInfo(workedSaveInfo, this.WorkedDataGrid);
            // 加载已忽略
            string ignoredSaveInfo = IniFileHelper.ReadValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "ignored");
            SetColumnsInfo(ignoredSaveInfo, this.IgnoredDataGrid);
        }

        private void SetColumnsInfo(string saveInfo, DataGrid dg)
        {
            if (!string.IsNullOrEmpty(saveInfo))
            {
                string[] columnArr = saveInfo.Split(CommonConstants.SEMICOLON_CHAR);
                int displayIndex = 0;
                if (columnArr != null && columnArr.Length > 0)
                {
                    foreach (string item in columnArr)
                    {
                        string[] headerAndWidth = item.Split(CommonConstants.COLON_CHAR);
                        if (headerAndWidth != null && headerAndWidth.Length == 2)
                        {
                            string header = headerAndWidth[0];
                            string width = headerAndWidth[1];
                            DataGridColumn column = dg.Columns.First(c => c.Header.ToString() == header);
                            if (column != null)
                            {
                                column.DisplayIndex = displayIndex;
                                column.Width = CommonUtils.ConvertToDouble(width);
                                displayIndex++;
                            }
                        }
                    }
                }
            }
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!(sender is DataGrid))
                    return;
                DataGrid dg = sender as DataGrid;
                if (e.KeyboardDevice.Modifiers == ModifierKeys.None && e.Key == Key.Tab)
                {
                    e.Handled = true;
                    Int32 row = dg.Items.IndexOf(dg.CurrentItem);
                    Int32 Col = dg.Columns.IndexOf(dg.CurrentColumn);

                    DataGridCell cell = GetCell(dg, row, Col);
                    if (cell.IsEditing == false)
                    {
                        cell.IsSelected = true;
                        cell.Focus();
                        dg.BeginEdit();
                        return;
                    }
                    do
                    {

                        if (Col + 1 > dg.Columns.Count)
                        {
                            if (row + 1 < dg.Items.Count)
                            {
                                row = row + 1;
                                Col = 0;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            Col = Col + 1;
                        }
                        cell = GetCell(dg, row, Col);
                    }
                    while (cell == null || cell.Column.IsReadOnly || cell.Column.Header.ToString() == "选择");
                    

                    if (cell != null && !cell.Column.IsReadOnly)
                    {
                        cell.IsSelected = true;
                        cell.Focus();
                        dg.BeginEdit();
                    }
                }
                else if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift && e.Key == Key.Tab)
                {
                    e.Handled = true;
                    Int32 row = dg.Items.IndexOf(dg.CurrentItem);
                    Int32 Col = dg.Columns.IndexOf(dg.CurrentColumn);

                    DataGridCell cell = GetCell(dg, row, Col);
                    if (cell.IsEditing == false)
                    {
                        cell.IsSelected = true;
                        cell.Focus();
                        dg.BeginEdit();
                        return;
                    }
                    do
                    {

                        if (Col - 1 < 0)
                        {
                            if (row - 1 >= 0)
                            {
                                row = row - 1;
                                Col = dg.Columns.Count - 1;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            Col = Col - 1;
                        }
                        cell = GetCell(dg, row, Col);
                    }
                    while (cell == null || cell.Column.IsReadOnly || cell.Column.Header.ToString() == "选择");


                    if (cell != null && !cell.Column.IsReadOnly)
                    {
                        cell.IsSelected = true;
                        cell.Focus();
                        dg.BeginEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }

        /// <summary>
        /// 根据行、列索引取的对应单元格对象
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public DataGridCell GetCell(DataGrid grid, int row, int column)
        {
            DataGridRow rowContainer = GetRow(grid, row);
            if (rowContainer != null)
            {
                rowContainer.IsSelected = true;
                DataGridCellsPresenter presenter = CommonUtils.FindVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null)
                {
                    grid.ScrollIntoView(rowContainer, grid.Columns[column]);
                    presenter = CommonUtils.FindVisualChild<DataGridCellsPresenter>(rowContainer);
                }
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }


        /// <summary>
        /// 根据行索引取的行对象
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DataGridRow GetRow(DataGrid grid, int index)
        {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        private void PriceNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void QuantityNU_edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void QuantityNU_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.SaveRow(_myOrderDefaultViewModel.SelectedItem_Working, "报量");
            }
        }
    }
}
