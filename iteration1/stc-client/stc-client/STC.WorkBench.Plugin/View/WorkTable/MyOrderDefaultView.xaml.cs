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
using STC.WorkBench.Plugin.ViewModel.Data;
using STC.WorkBench.Plugin.ViewModel.WorkTable;
using STC.Common.Utils;
using STC.Common.View.Component;
using static STC.WorkBench.Plugin.View.Component.AutoCompleteComboBox;
using STC.WorkBench.Plugin.View.Component;
using AutoCompleteComboBox = STC.WorkBench.Plugin.View.Component.AutoCompleteComboBox;
using STC.Common.Data;
using System.Windows.Threading;
using STC.Common.View.Order.InnerPanel;
using STC.Common.ViewModel.Data;
using STC.Common.Model.Table;
using STC.Common.Handler.Table;
using STC.Common.View.FQ;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using STC.Common.Model.Admin;
using STC.Common.Handler.Admin;
using System.ComponentModel;

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// MyOrderDefaultView.xaml 的交互逻辑
    /// </summary>
    public partial class MyOrderDefaultView : UserControl
    {
        public event Action<IntentionQuoteItem> SelectedRowChanged;

        private MyOrderDefaultViewModel _myOrderDefaultViewModel;

        /// <summary>
        /// 1:做市 2:销交
        /// </summary>
        private string _innerType = "1";

        public IntentionControlInterface IntentionPart;
        private Border maskBorder = new Border();

        public MyOrderDefaultView()
        {
            InitializeComponent();

            if (CommonUtils.IsBDBAppName())
            {
                ToQbMenu_Reference.Visibility = Visibility.Visible;
            }

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                SearchIntentionBtn.Visibility = Visibility.Visible;

                IntentionPart = new IntentionControl_Buy();
                _myOrderDefaultViewModel = new IntentionControl_Buy_ViewModel();
                if (CommonUtils.IsInvestor())
                {
                    (_myOrderDefaultViewModel as IntentionControl_Buy_ViewModel).ProcessFilterIsMe(false);
                }

                Search_TextBox.ToolTip = "可模糊查询债券代码、债券简称、交易员、投资经理、发行人、担保人";

                List<DataGridColumn> removeList = new List<DataGridColumn>();
                foreach(DataGridColumn column in this.ReferenceDataGrid.Columns)
                {
                    if (column.Header.ToString() == "定价")
                    {
                        column.Header = "报价";
                    }

                    if (column.Header.ToString() == "来源方报价"
                        || column.Header.ToString() == "点差")
                    {
                        removeList.Add(column);
                    }
                }
                foreach(DataGridColumn column in removeList)
                {
                    this.ReferenceDataGrid.Columns.Remove(column);
                }

                Dictionary<string, object> dic = new Dictionary<string, object>();
                List<StcTbookUsers> traders = CommonUtils.GetTraders();
                if (LocalEnv.ConfInfo.IsI2T && CommonUtils.IsInvestor())
                {
                    List<string> traderList = AdminHandler.Instance.GetTraderList(LocalEnv.UserInfo.UserId);
                    if (traderList != null)
                    {
                        var list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                        foreach (StcTbookUsers stcUserInfo in list)
                        {
                            dic[stcUserInfo.Username] = stcUserInfo.Name;
                        }
                    }
                }
                else
                {
                    if (traders != null)
                    {
                        foreach (StcTbookUsers stcUserInfo in traders)
                        {
                            dic[stcUserInfo.Username] = stcUserInfo.Name;
                        }
                    }
                }
                
                this.TradersCB.ItemsSource = dic;
                this.TradersCB.FilterDicChanged += TradersCB_FilterDicChanged;

                if (CommonUtils.IsInvestor())
                {
                    TradersTB.Visibility = Visibility.Visible;
                    TradersCB.Visibility = Visibility.Visible;
                }
                else
                {
                    isMeCheck.Visibility = Visibility.Visible;
                    TradersTB.Visibility = Visibility.Visible;
                    TradersCB.Visibility = Visibility.Visible;
                    this.TradersCB.SelectedValues = LocalEnv.UserInfo.UserName;

                    dic = new Dictionary<string, object>();
                    List<StcTbookUsers> investors = CommonUtils.GetInvestors();
                    if (investors != null)
                    {
                        dic[" "] = "空";
                        foreach (StcTbookUsers stcUserInfo in investors)
                        {
                            dic[stcUserInfo.Username] = stcUserInfo.Name;
                        }
                    }
                    this.InvestorsCB.ItemsSource = dic;
                    this.InvestorsCB.FilterDicChanged += InvestorsCB_FilterDicChanged;

                    InvestorsTB.Visibility = Visibility.Visible;
                    InvestorsCB.Visibility = Visibility.Visible;
                }

                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    this.ReferenceDataGrid.Columns.Remove(this.status_demand_column);
                    this.ReferenceDataGrid.Columns.Remove(this.ops_demand_column);
                }
                publishBtn_Reference.Visibility = Visibility.Collapsed;
                hiddenBtn_Reference.Visibility = Visibility.Collapsed;
                AddIntention_Reference.Visibility = Visibility.Collapsed;
                refBtn_Reference.Visibility = Visibility.Collapsed;
                recoverBtn_Reference.Visibility = Visibility.Collapsed;
            }
            else
            {
                IntentionPart = new IntentionControl_Sell();
                _myOrderDefaultViewModel = new IntentionControl_Sell_ViewModel();
            }

            if (_myOrderDefaultViewModel.IsInquiry == "0")
            {
                this.DemandTab.IsSelected = true;
                demand_title_grid.Margin = new Thickness(110, 4, 0, 4);
            }
            IntentionPart.ClickWorkingDataGridAction = ClickWorkingDataGrid;
            //IntentionPart.OpenInquiryAction = OpenInquiry;
            //IntentionPart.CloseInquiryAction = CloseInquiry;
            IntentionPart.ChangeSelectedRowAction = ChangeSelectedRow;

            person_part.Children.Add(IntentionPart as UIElement);
            maskBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));
            maskBorder.Opacity = 0.5;
            maskBorder.IsHitTestVisible = false;
            maskBorder.Visibility = Visibility.Collapsed;
            person_part.Children.Add(maskBorder);

            //if (!_verClose)
            //{
            //    BtnVer_Click(null, null);
            //}

            MenuItem sortUpdateTimeMi_reference = new MenuItem();
            sortUpdateTimeMi_reference.Name = "sortUpdateTimeColumnMi";
            sortUpdateTimeMi_reference.Header = "按创建时间排序";
            sortUpdateTimeMi_reference.Style = (Style)this.FindResource("DefaultMenuItem");
            sortUpdateTimeMi_reference.Click += sortUpdateTimeColumnMi_Reference_Click;
            this.ReferenceDataGrid.GetHeaderContextMenu().Items.Add(sortUpdateTimeMi_reference);

            this.Loaded += MyOrderDefaultView_Loaded;

            if (IntentionOldHandler.Instance.isSALER())
            {
                _innerType = "2";
            }
            
            this.DataContext = _myOrderDefaultViewModel;
            this.ReferenceDataGrid.SelectionChanged += ReferenceDataGrid_SelectionChanged;
            this.ReferenceDataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.ReferenceDataGrid_MouseLeftButtonDown), true);


            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                CommonIntentionStatus_Tb.Visibility = Visibility.Collapsed;
                CommonIntentionStatusCB.Visibility = Visibility.Collapsed;
                IntentionStatus_tb.Visibility = Visibility.Visible;
                IntentionStatusCB.Visibility = Visibility.Visible;
                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    MatchStatus_tb.Visibility = Visibility.Visible;
                    MatchStatusCB.Visibility = Visibility.Visible;
                    ObservableCollection<OptionItem> filterItems = new ObservableCollection<OptionItem>();
                    filterItems.Add(new OptionItem("Bid", "1"));
                    filterItems.Add(new OptionItem("Ofr", "2"));
                    side.ItemsSource = filterItems;
                    side.Visibility = Visibility.Visible;

                    addBtn.Visibility = Visibility.Collapsed;
                }
                Dictionary<string, object> dic = new Dictionary<string, object>();
                //dic["待发布"] = "待发布";
                //dic["已发布"] = "已发布";
                dic["待处理"] = "待处理";
                dic["处理中"] = "处理中";
                dic["撤销中"] = "撤销中";
                dic["已撤销"] = "已撤销";
                dic["已完成"] = "已完成";
                IntentionStatusCB.ItemsSource = dic;
                //IntentionStatusCB.SelectedValues = "待处理,处理中,撤销中";
                IntentionStatusCB.FilterDicChanged += Cbb_FilterDicChanged;

                Dictionary<string, object> matchStatusDic = new Dictionary<string, object>();
                matchStatusDic["5"] = "未匹配";
                matchStatusDic["6"] = "部分匹配";
                matchStatusDic["7"] = "等量匹配";
                matchStatusDic["8"] = "超量匹配";
                MatchStatusCB.ItemsSource = matchStatusDic;
                MatchStatusCB.FilterDicChanged += MatchStatusCB_FilterDicChanged;
            }
            else
            {
                CommonIntentionStatus_Tb.Visibility = Visibility.Visible;
                CommonIntentionStatusCB.Visibility = Visibility.Visible;
                IntentionStatus_tb.Visibility = Visibility.Collapsed;
                IntentionStatusCB.Visibility = Visibility.Collapsed;
                MatchStatus_tb.Visibility = Visibility.Collapsed;
                MatchStatusCB.Visibility = Visibility.Collapsed;
                side.Visibility = Visibility.Collapsed;

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic["待发布"] = "待发布";
                dic["已发布"] = "已发布";
                dic["已撤销"] = "已撤销";
                dic["已完成"] = "已完成";
                CommonIntentionStatusCB.ItemsSource = dic;
                CommonIntentionStatusCB.SelectedValues = "待发布,已发布";
                CommonIntentionStatusCB.FilterDicChanged += Cbb_FilterDicChanged;
            }
            

            //ObservableCollection<OptionItem> listSide = new ObservableCollection<OptionItem>();
            //listSide.Add(new OptionItem("个需", "INTENTION"));
            //listSide.Add(new OptionItem("参考", "DEMAND"));
            //QuoteType.ItemsSource = listSide;
            //QuoteType.TagsListChanged += QuoteType_TagsListChanged;

            if (LocalEnv.UserInfo.RoleType != RoleType.BUYER_ROLE || LocalEnv.ConfInfo.IsPAYL == false)
            {
                this.inquiryDg.Columns.Remove(this.BuyTraderColumn);
                this.inquiryDg.Columns.Remove(this.RemaingingTimeColumn);
                this.inquiryDg.Columns.Remove(this.SymbolColumn);
            }
            else
            {
                InquiryTitle_SP.Visibility = Visibility.Collapsed;
            }
        }

        bool isTradersCBOps = false;
        private void TradersCB_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (isMeCheckOps)
            {
                isMeCheckOps = false;
                return;
            }

            if (_myOrderDefaultViewModel is IntentionControl_Buy_ViewModel)
            {
                MultiSelectComboBox ccb = sender as MultiSelectComboBox;

                string[] orgArr = null;
                if (ccb != null)
                {
                    string[] userNames = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);

                    if (userNames != null && userNames.Length > 0)
                    {
                        if (userNames.Any(x => x != LocalEnv.UserInfo.UserName))
                        {
                            if (isMeCheck.IsChecked == true)
                            {
                                isTradersCBOps = true;
                                isMeCheck.IsChecked = false;
                                
                            }
                        }
                        else
                        {
                            if (isMeCheck.IsChecked == false)
                            {
                                isTradersCBOps = true;
                                isMeCheck.IsChecked = true;
                            }
                        }
                    }
                    else
                    {
                        if (isMeCheck.IsChecked == true)
                        {
                            isTradersCBOps = true;
                            isMeCheck.IsChecked = false;
                        }
                    }

                    if (!string.IsNullOrEmpty(ccb.SelectedValues))
                    {
                        orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                    }
                }

                (_myOrderDefaultViewModel as IntentionControl_Buy_ViewModel).ProcessFilterMyTraders(orgArr);
            }
        }

        private void InvestorsCB_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel is IntentionControl_Buy_ViewModel)
            {
                MultiSelectComboBox ccb = sender as MultiSelectComboBox;

                string[] orgArr = null;
                if (ccb != null)
                {
                    if (!string.IsNullOrEmpty(ccb.SelectedValues))
                    {
                        orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR);
                    }
                }

                (_myOrderDefaultViewModel as IntentionControl_Buy_ViewModel).ProcessFilterMyInvestors(orgArr);
            }
        }


        private void sortUpdateTimeColumnMi_Reference_Click(object sender, RoutedEventArgs e)
        {
            this.ReferenceDataGrid.Items.SortDescriptions.Clear();
            this.ReferenceDataGrid.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("CreateTime", System.ComponentModel.ListSortDirection.Descending));
        }

        private void ReferenceDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Reference != null)
            {
                SelectedRowChanged?.BeginInvoke(_myOrderDefaultViewModel.SelectedItem_Reference, null, null);
            }
        }

        private void MyOrderDefaultView_Loaded(object sender, RoutedEventArgs e)
        {
                        
        }

        //private void QuoteType_TagsListChanged(object sender, RoutedEventArgs e)
        //{
        //    FilterOptions fo = sender as FilterOptions;

        //    _myOrderDefaultViewModel.ProcessQuoteTypeData(fo.TagsList);
        //}

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

        //private void CheckBox_All_Click_Ignore(object sender, RoutedEventArgs e)
        //{
        //    if (sender != null && sender is CheckBox)
        //    {
        //        CheckBox chk = sender as CheckBox;
        //        bool flag = chk.IsChecked == true ? true : false;
        //        if (_myOrderDefaultViewModel != null)
        //        {
        //            _myOrderDefaultViewModel.CheckBoxAllValueChange_Ignore(flag);
        //        }
        //    }
        //}

        //private void Selected_Chk_Click_Ignore(object sender, RoutedEventArgs e)
        //{
        //    if (_myOrderDefaultViewModel != null)
        //    {
        //        _myOrderDefaultViewModel.CheckBoxSingleClick_Ignore();
        //    }
        //}

        

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

        //private void SearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (_myOrderDefaultViewModel != null)
        //    {
        //        this.WorkingDataGrid?.CommitEdit();
        //        this.WorkingDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        //        this.WorkingDataGrid?.CancelEdit();
        //        //this.IgnoredDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        //        _myOrderDefaultViewModel.SelectFilterType = ((ComboBoxItem)this.SearchComboBox.SelectedItem).Tag.ToString();
        //        _myOrderDefaultViewModel.RefreshView();
        //    }
        //}

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppUtils.IsInnerInput = true;
            if (isFromEnter)
            {
                AppUtils.IsInnerInput = false;
            }
            if (_myOrderDefaultViewModel != null)
            {
                //this.WorkingDataGrid?.CommitEdit();
                //this.WorkingDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                //this.WorkingDataGrid?.CancelEdit();
                //this.IgnoredDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                //_myOrderDefaultViewModel.FilterText = this.SearchTextBox.Text;
                //_myOrderDefaultViewModel.RefreshView();
            }
        }

        

        //private void IgnoredDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Ignore != null)
        //    {
        //        SelectedRowChanged?.Invoke(_myOrderDefaultViewModel.SelectedItem_Ignore);
        //    }
        //}

        

        private void ClickWorkingDataGrid()
        {

            if(this.FilterPopup.Visibility == Visibility.Visible)
            {
                this.FilterPopup.Visibility = Visibility.Collapsed;
                IntentionPart.GetCustomDefaultDataGrid().Opacity = 1;
                maskBorder.Visibility = Visibility.Collapsed;
                //btn_Fileter.IsEnabled = true;
                this.btn_Fileter.Content = "更多筛选";
            }
            //if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
            //{
            //    SelectedRowChanged?.BeginInvoke(_myOrderDefaultViewModel.SelectedItem_Working, null, null);
            //}
        }

        //private void IgnoredDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Ignore != null)
        //    {
        //        SelectedRowChanged?.Invoke(_myOrderDefaultViewModel.SelectedItem_Ignore);
        //    }
        //}


        int i_demend = 0;
        bool isDouble_demand = false;
        DataGridRow _lastDGR_demand = null;
        private void ReferenceDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataGridRow dgr = null;

                var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
                DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
                while (cell != null && !(cell is System.Windows.Controls.DataGridCell))
                    cell = VisualTreeHelper.GetParent(cell);

                System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;

                string headerName = string.Empty;
                if (targetCell != null)
                {
                    headerName = targetCell.Column.Header.ToString();
                    DataGrid dg = sender as DataGrid;
                    dg.CurrentColumn = targetCell.Column;

                    dgr = ReferenceDataGrid.ItemContainerGenerator.ContainerFromItem(targetCell.DataContext) as DataGridRow;
                }

                i_demend += 1;
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
                timer.Tick += (s, e1) => { timer.IsEnabled = false; i_demend = 0; ReferenceDataGrid_UIElement_Click(sender, e); isDouble_demand = false; };
                timer.IsEnabled = true;
                if (i_demend > 0 && i_demend % 2 == 0)
                {
                    timer.IsEnabled = false;
                    i_demend = 0;
                    isDouble_demand = true;

                    //双击时执行的代码
                    //MessageBox.Show("DoubleClick!");
                    if (dgr != null && _lastDGR_demand == dgr)
                    {
                        if (headerName == "来源方机构" || headerName == "来源方"
                        || headerName == "IM号" || headerName == "群消息")
                        {
                            // 双击
                            _myOrderDefaultViewModel.Sub_DoStartQQCommand(false, _myOrderDefaultViewModel.SelectedItem_Reference);
                        }
                        //else
                        //{
                        //    if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null
                        //    && _myOrderDefaultViewModel.SelectedItem_Working.Status != "2"
                        //    && _myOrderDefaultViewModel.SelectedItem_Working.Status != "3"
                        //    && _myOrderDefaultViewModel.SelectedItem_Working.Status != "4")
                        //    {
                        //        _myOrderDefaultViewModel.EditOrder();
                        //    }
                        //}
                        else if (headerName == "代码" || headerName == "简称")
                        {
                            if (CommonUtils.IsBDBAppName() && _myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Reference != null)
                            {
                                CommonUtils.OpenSmallBondWin(_myOrderDefaultViewModel.SelectedItem_Reference.SecurityId);
                            }
                        }
                    }
                }
                else
                {
                    _lastDGR_demand = dgr;
                }
            }
            catch(Exception ex)
            {
                LogHandler.Error(ex);
            }
        }




        private void ReferenceDataGrid_UIElement_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble_demand)
                return;
        }

        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() == "来源方机构")
            {
                if (e.Column is DataGridTemplateColumn)
                {
                    IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                    FrameworkElement element = e.Column.GetCellContent(item);
                    DataGridTemplateColumn templeColumn = e.Column as DataGridTemplateColumn;
                    Object obj = templeColumn.CellEditingTemplate.FindName("InsNameCb_edit", element);

                    if (obj != null && obj is AutoCompleteComboBox)
                    {
                        AutoCompleteComboBox acc = obj as AutoCompleteComboBox;
                        acc.ExpandOptions();
                    }
                }
            }
            else if (e.Column.Header.ToString() == "来源方")
            {
                if (e.Column is DataGridTemplateColumn)
                {
                    IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                    FrameworkElement element = e.Column.GetCellContent(item);
                    DataGridTemplateColumn templeColumn = e.Column as DataGridTemplateColumn;
                    Object obj = templeColumn.CellEditingTemplate.FindName("TrdNameCb_edit", element);

                    if (obj != null && obj is AutoCompleteComboBox)
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
                IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;

                if ((e.Column.Header.ToString() == "来源方机构" || e.Column.Header.ToString() == "来源方")
                     && (_innerType != "2" || item.Status == "3"))
                {
                    e.Cancel = true;
                    return;
                }

                if (item.Status != "0" &&
                    e.Column.Header.ToString() != "来源方机构" && e.Column.Header.ToString() != "来源方"
                    && e.Column.Header.ToString() != "定价" && e.Column.Header.ToString() != "报量(万)"
                    && e.Column.Header.ToString() != "来源方报价" && e.Column.Header.ToString() != "点差"
                    && e.Column.Header.ToString() != "公告" && e.Column.Header.ToString() != "个人笔记")
                {
                    e.Cancel = true;
                    return;
                }

                if (item.Status == "2" && !string.IsNullOrEmpty(item.InstitutionName) && !string.IsNullOrEmpty(item.TraderName))
                {
                    e.Cancel = true;
                    return;
                }
                if (item.Status == "4")
                {
                    e.Cancel = true;
                    return;
                }
                _myOrderDefaultViewModel.EditRow(e.Row.Item as IntentionQuoteItem);
            }
        }

        private void WorkingDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && e.Row.Item is IntentionQuoteItem)
            {
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


        //private const string SaveColumnsInfo_IntentionTable = "IntentionColumns";
        //public void SaveColumnsInfo()
        //{
        //    // 保存待处理
        //    IniFileHelper.WriteValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "working", GetColumnsInfo(this.WorkingDataGrid));
        //    // 保存已忽略
        //    //IniFileHelper.WriteValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "ignored", GetColumnsInfo(this.IgnoredDataGrid));
        //}

        //private string GetColumnsInfo(DataGrid dg)
        //{
        //    StringBuilder saveInfo = new StringBuilder();
        //    var sortedColumns = dg.Columns.OrderBy(column => column.DisplayIndex);
        //    foreach (DataGridColumn column in sortedColumns)
        //    {
        //        if (saveInfo.Length > 0)
        //        {
        //            saveInfo.Append(";");
        //        }
        //        saveInfo.Append($"{column.Header}:{column.ActualWidth}");
        //    }
        //    return saveInfo.ToString();
        //}

        //public void LoadColumnsInfo()
        //{
        //    // 加载待处理
        //    string workingSaveInfo = IniFileHelper.ReadValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "working");
        //    SetColumnsInfo(workingSaveInfo, this.WorkingDataGrid);

        //    // 加载已忽略
        //    //string ignoredSaveInfo = IniFileHelper.ReadValue($"{SaveColumnsInfo_IntentionTable}-{LocalEnv.UserInfo.UserName}", "ignored");
        //    //SetColumnsInfo(ignoredSaveInfo, this.IgnoredDataGrid);
        //}

        //private void SetColumnsInfo(string saveInfo, DataGrid dg)
        //{
        //    if (!string.IsNullOrEmpty(saveInfo))
        //    {
        //        string[] columnArr = saveInfo.Split(CommonConstants.SEMICOLON_CHAR);
        //        int displayIndex = 0;
        //        if (columnArr != null && columnArr.Length > 0)
        //        {
        //            foreach (string item in columnArr)
        //            {
        //                string[] headerAndWidth = item.Split(CommonConstants.COLON_CHAR);
        //                if (headerAndWidth != null && headerAndWidth.Length == 2)
        //                {
        //                    string header = headerAndWidth[0];
        //                    string width = headerAndWidth[1];
        //                    DataGridColumn column = dg.Columns.First(c => c.Header.ToString() == header);
        //                    if (column != null)
        //                    {
        //                        column.DisplayIndex = displayIndex;
        //                        column.Width = CommonUtils.ConvertToDouble(width);
        //                        displayIndex++;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

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
                        bool flag = dg.BeginEdit();
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
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
            {
                _myOrderDefaultViewModel.SaveRow(_myOrderDefaultViewModel.SelectedItem_Working, "报量(万)");
            }
        }

        //private void InsNameCb_edit_AfterSearch(object obj)
        //{
        //    this.WorkingDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        //}

        //private void TrdNameCb_edit_AfterSearch(object obj)
        //{
        //    this.WorkingDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        //}


        #region 全局搜索
        private bool isFromEnter = false;
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AppUtils.IsInnerInput = false;
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            isFromEnter = false;
            AppUtils.IsInnerInput = true;
            if (e.Key == Key.Enter)
            {
                isFromEnter = true;
            }
        }
        #endregion



        private void BrokerCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AutoCompleteComboBox BrokerCb = sender as AutoCompleteComboBox;
            if (string.IsNullOrEmpty(BrokerCb.Text))
            {
                BrokerCb.ExpandOptions();
            }
        }

        private void CustomerPriceNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void BpNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void Tags_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.SaveRow(_myOrderDefaultViewModel.SelectedItem_Working, "标签");
            }
        }

        int i = 0;
        bool isDouble = false;
        private void im_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            i += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i = 0; im_Click(sender, e); isDouble = false; };
            timer.IsEnabled = true;
            if (i % 2 == 0)
            {
                timer.IsEnabled = false;
                i = 0;
                isDouble = true;

                // 双击
                _myOrderDefaultViewModel.StartQQCommand.Execute(null);
            }
        }

        private void im_Click(object sender, MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetAddTagGroup()))
            {
                AddSubscriptionGroupWindow addGroupWin = new AddSubscriptionGroupWindow(IntentionOldHandler.Instance.GroupList, this._myOrderDefaultViewModel.FocusTags, 
                    this._myOrderDefaultViewModel.SelectedItem_Tag?.GroupID, this._myOrderDefaultViewModel.StartTermDY, 
                    this._myOrderDefaultViewModel.EndTermDY, this._myOrderDefaultViewModel.IsAppTerm,
                    this._myOrderDefaultViewModel.StartCoupon, this._myOrderDefaultViewModel.EndCoupon, this._myOrderDefaultViewModel.IsCoupon,
                    this._myOrderDefaultViewModel.StartPrice, this._myOrderDefaultViewModel.EndPrice,
                    this._myOrderDefaultViewModel.StartChinaBondValue, this._myOrderDefaultViewModel.EndChinaBondValue,
                    this._myOrderDefaultViewModel.StartBidBp, this._myOrderDefaultViewModel.EndBidBp,
                    this._myOrderDefaultViewModel.StartOfrBp, this._myOrderDefaultViewModel.EndOfrBp);
                addGroupWin.AddAction = OnAddAction;
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetAddTagGroup(), addGroupWin);
            }
        }

        public void OnAddAction(FilterGroupItem filterGroupItem)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                _myOrderDefaultViewModel.SelectedItem_Tag = filterGroupItem;
                //_innerBoardViewModel.FocusTags = _innerBoardViewModel.SelectedItem_Tag?.FocusTags;
                //_innerBoardViewModel.FocusTags_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.FocusTags;
            }));
        }


        private void EditGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetEditTagGroup()))
            {
                EditGroupWindow editGroupWin = new EditGroupWindow(IntentionOldHandler.Instance.GroupList, _myOrderDefaultViewModel.SelectedItem_Tag?.GroupID);
                SecurityFilterPanel securityFilterPanel = editGroupWin.FindName("filterPanel") as SecurityFilterPanel;
                securityFilterPanel.Visibility4NoReply = Visibility.Collapsed;
                editGroupWin.EditAction = OnEditAction;
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetEditTagGroup(), editGroupWin);
            }
        }

        private string _TRADEBOOK = "Tradebook";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string securityID = _TRADEBOOK;
                    if (_myOrderDefaultViewModel != null
                        && _myOrderDefaultViewModel.SelectedItem_Working != null
                        && !string.IsNullOrEmpty(_myOrderDefaultViewModel.SelectedItem_Working.SecurityId))
                    {
                        securityID = _myOrderDefaultViewModel.SelectedItem_Working.SecurityId;
                    }


                    PriceMatchParam priceMatchParam = new PriceMatchParam();
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        priceMatchParam.Symbol = _TRADEBOOK;
                    }
                    else
                    {
                        priceMatchParam.Symbol = securityID;
                    }
                    
                    priceMatchParam.Side = "2";
                    priceMatchParam.IsIntention = true;
                    priceMatchParam.SettlType = "3";
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                    {
                        if (CommonUtils.IsInvestor())
                        {
                            priceMatchParam.managerId = LocalEnv.UserInfo.UserName;
                            priceMatchParam.managerName = LocalEnv.UserInfo.FullName;
                        }
                        else
                        {
                            priceMatchParam.buyerTraderId = LocalEnv.UserInfo.UserName;
                            priceMatchParam.buyerTraderName = LocalEnv.UserInfo.FullName;
                        }
                    }
                    SendRequireView sendRequireView = new SendRequireView(priceMatchParam);
                    //SendRequireNewView sendRequireView = new SendRequireNewView(securityID, false);
                    WindowManager.Instance.OpenOnlyOneWindow($"SendRequireView-{securityID}", sendRequireView);
                    //sendRequireView.Show();
                });
            });

            WriteDBLogHelper.Instance.GenRecord("personal", "001", "新增需求");
        }

        private void ShowRecognizeWin_Click(object sender, RoutedEventArgs e)
        {
            DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(null);

            WriteDBLogHelper.Instance.GenRecord("personal", "002", "批量识别");
        }

        //private void singleToggleBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this.singleToggleBtn.Style == FindResource("CollapsedButton_24"))
        //    {
        //        this.best_single_filter_part.Visibility = Visibility.Collapsed;
        //        this.singleToggleBtn.Style = (Style)FindResource("ExpandButton_24");
        //    }
        //    else if (this.singleToggleBtn.Style == FindResource("ExpandButton_24"))
        //    {
        //        this.best_single_filter_part.Visibility = Visibility.Visible;
        //        this.singleToggleBtn.Style = (Style)FindResource("CollapsedButton_24");
        //    }
        //}

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item.IsSelected)
            {
                item.IsSelected = false;
                this._myOrderDefaultViewModel.SelectedItem_Tag = null;
                e.Handled = true;
            }
        }

        private void TagGroupMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                MenuItem mi = sender as MenuItem;
                if (mi.DataContext is FilterGroupItem)
                {
                    FilterGroupItem fgi = mi.DataContext as FilterGroupItem;
                    _myOrderDefaultViewModel.UpdateGroupOne(fgi, !fgi.IfSubscribe);
                }
            }
        }

        private void EditGroupMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                MenuItem mi = sender as MenuItem;
                if (mi.DataContext is FilterGroupItem)
                {
                    FilterGroupItem fgi = mi.DataContext as FilterGroupItem;
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetEditTagGroup()))
                    {
                        EditGroupWindow editGroupWin = new EditGroupWindow(IntentionOldHandler.Instance.GroupList, fgi?.GroupID);
                        SecurityFilterPanel securityFilterPanel = editGroupWin.FindName("filterPanel") as SecurityFilterPanel;
                        securityFilterPanel.Visibility4NoReply = Visibility.Collapsed;
                        editGroupWin.EditAction = OnEditAction;
                        WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetEditTagGroup(), editGroupWin);
                    }
                }
            }
        }

        public void OnEditAction()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                _myOrderDefaultViewModel.FocusTags = _myOrderDefaultViewModel.SelectedItem_Tag?.FocusTags;

                _myOrderDefaultViewModel.IsPrice = _myOrderDefaultViewModel.SelectedItem_Tag != null ? _myOrderDefaultViewModel.SelectedItem_Tag.IsPrice : false;
                _myOrderDefaultViewModel.StartPrice = _myOrderDefaultViewModel.SelectedItem_Tag?.StartPrice;
                _myOrderDefaultViewModel.EndPrice = _myOrderDefaultViewModel.SelectedItem_Tag?.EndPrice;

                _myOrderDefaultViewModel.StartTermDY = _myOrderDefaultViewModel.SelectedItem_Tag?.StartTermDY;
                _myOrderDefaultViewModel.EndTermDY = _myOrderDefaultViewModel.SelectedItem_Tag?.EndTermDY;

                _myOrderDefaultViewModel.IsCoupon = _myOrderDefaultViewModel.SelectedItem_Tag != null ? _myOrderDefaultViewModel.SelectedItem_Tag.IsCoupon : false;
                _myOrderDefaultViewModel.StartCoupon = _myOrderDefaultViewModel.SelectedItem_Tag?.StartCoupon;
                _myOrderDefaultViewModel.EndCoupon = _myOrderDefaultViewModel.SelectedItem_Tag?.EndCoupon;


                _myOrderDefaultViewModel.StartChinaBondValue = _myOrderDefaultViewModel.SelectedItem_Tag?.StartChinaBond;
                _myOrderDefaultViewModel.EndChinaBondValue = _myOrderDefaultViewModel.SelectedItem_Tag?.EndChinaBond;

                _myOrderDefaultViewModel.StartBidBp = _myOrderDefaultViewModel.SelectedItem_Tag?.StartBidBp;
                _myOrderDefaultViewModel.EndBidBp = _myOrderDefaultViewModel.SelectedItem_Tag?.EndBidBp;

                _myOrderDefaultViewModel.StartOfrBp = _myOrderDefaultViewModel.SelectedItem_Tag?.StartOfrBp;
                _myOrderDefaultViewModel.EndOfrBp = _myOrderDefaultViewModel.SelectedItem_Tag?.EndOfrBp;

                _myOrderDefaultViewModel.ProcessGroupBonds(_myOrderDefaultViewModel.SelectedItem_Tag?.Inf1, _myOrderDefaultViewModel.SelectedItem_Tag?.FocusGroupId);
            }));
        }

        private void DelGroupMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                MenuItem mi = sender as MenuItem;
                if (mi.DataContext is FilterGroupItem)
                {
                    FilterGroupItem fgi = mi.DataContext as FilterGroupItem;
                    if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetEditTagGroup()))
                    {
                        DelOne(fgi?.GroupID);
                    }
                }
            }
        }

        internal void DelOne(string groupid)
        {
            bool result = CustomMessageBox.Question("是否删除组？", "删除组");

            if (result == false)
            {
                return;
            }

            if (!string.IsNullOrEmpty(groupid))
            {
                QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
                req.action = "3";
                req.id = groupid;
                QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);

                if (reply != null && reply.IsSuccess())
                {
                    try
                    {
                        FilterGroupItem removeItem = IntentionOldHandler.Instance.GroupList.First(item => item.GroupID == groupid);
                        IntentionOldHandler.Instance.GroupList.Remove(removeItem);
                    }
                    catch (System.InvalidOperationException)
                    {
                        // ignore
                    }
                }
            }
        }

        private void ProfileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                foreach (FilterGroupItem item in e.AddedItems)
                {
                    WriteDBLogHelper.Instance.GenRecord("personal", "021", item.GroupName);
                }
            }
        }

        private void ProfileListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                e.Handled = true;
        }

        private void DataGridRow_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridRow dgr = sender as DataGridRow;
            dgr.Cursor = Cursors.Hand;
        }

        private void DataGridRow_MouseLeave(object sender, MouseEventArgs e)
        {
            DataGridRow dgr = sender as DataGridRow;
            dgr.Cursor = null;
        }

       
        private void Single_Click(object sender, RoutedEventArgs e)
        {
            if (SingleRd_ALL.IsChecked == true)
            {
                this.customFilterPanel.SetDisplayMode("0");
                _myOrderDefaultViewModel.DisplayMode = "";
            }
            else if (SingleRd_R.IsChecked == true)
            {
                this.customFilterPanel.SetDisplayMode("1");
                _myOrderDefaultViewModel.DisplayMode = "BNC";
            }
            else if (SingleRd_C.IsChecked == true)
            {
                this.customFilterPanel.SetDisplayMode("2");
                _myOrderDefaultViewModel.DisplayMode = "BCO";
            }
            else if (SingleRd_L.IsChecked == true)
            {
                this.customFilterPanel.SetDisplayMode("3");
                _myOrderDefaultViewModel.DisplayMode = "LOCAL";
            }
            else if (SingleRd_N.IsChecked == true)
            {
                this.customFilterPanel.SetDisplayMode("4");
                _myOrderDefaultViewModel.DisplayMode = "NCD";
            }

            this.customFilterPanel.ShowAll();
        }


        

        private void SelectedClientType_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.FilterClientType(args);
            }
        }

        private void Cbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
           
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;

            string[] orgArr = null;
            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                _myOrderDefaultViewModel.ProcessFilterData_Buyer(orgArr);
            }
            else
            {
                _myOrderDefaultViewModel.ProcessFilterData(orgArr);
            }
        }

        private void MatchStatusCB_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;

            string[] orgArr = null;
            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            _myOrderDefaultViewModel.ProcessMatchStatusFilterData(orgArr);
        }

        //private void CopyMenu_Broker_Click(object sender, RoutedEventArgs e)
        //{
        //    if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
        //        && CommonUtils.CopyDic["COPYMODE"].Count > 0
        //        && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
        //    {
        //        this.InnerBoard_Broker_dataGrid.CopyData_Best(false, "2");
        //    }
        //    else
        //    {
        //        this.InnerBoard_Broker_dataGrid.CopyData_Best(false, "1");
        //    }
        //    FQSendWindow fqSendWindow = new FQSendWindow(true);
        //    fqSendWindow.Owner = Application.Current.MainWindow;
        //    fqSendWindow.Show();
        //}

        private void InnerBroker_DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        //private void opensecurity_btn_Click(object sender, RoutedEventArgs e)
        //{
        //    // 选中Item
        //    InnerOrderData innerOrderData = (sender as Button).DataContext as InnerOrderData;
        //    var item = this.InnerBoard_Broker_dataGrid.ItemContainerGenerator.ContainerFromItem(innerOrderData);
        //    (item as DataGridRow).IsSelected = true;
        //    Common.ViewModel.TradeBookHandler.TBookViewHandler.Instance.OpenQuotesMiniWindows(innerOrderData.SecurityID);
        //}


        private void InnerBoard_Broker_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void InnerBoard_Best_dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void InnerBoard_Broker_dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //double person_part_height;
        //private void personToggleBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this.personToggleBtn.Style == FindResource("CollapsedButton_24"))
        //    {
        //        person_part_height = person_part.ActualHeight;
        //        p_part.Height = new GridLength(0);
        //        this.person_part.Visibility = Visibility.Collapsed;
        //        this.personToggleBtn.Style = (Style)FindResource("ExpandButton_24");
        //    }
        //    else if (this.personToggleBtn.Style == FindResource("ExpandButton_24"))
        //    {
        //        if (r_part.Height.Value != 0)
        //        {
        //            double b = r_part.ActualHeight - person_part_height / 2 < 0 ? r_part.ActualHeight : r_part.ActualHeight - person_part_height / 2;
        //            r_part.Height = new GridLength(b, GridUnitType.Star);
        //        }

        //        p_part.Height = new GridLength(person_part_height, GridUnitType.Star);
        //        this.person_part.Visibility = Visibility.Visible;
        //        this.personToggleBtn.Style = (Style)FindResource("CollapsedButton_24");
        //    }
        //}
        //double reference_part_height;
        //private void referenceToggleBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this.referenceToggleBtn.Style == FindResource("CollapsedButton_24"))
        //    {
        //        reference_part_height = ReferenceDataGrid.ActualHeight;
        //        r_part.Height = new GridLength(0, GridUnitType.Star);
        //        this.ReferenceDataGrid.Visibility = Visibility.Collapsed;
        //        this.referenceToggleBtn.Style = (Style)FindResource("ExpandButton_24");
        //    }
        //    else if (this.referenceToggleBtn.Style == FindResource("ExpandButton_24"))
        //    {
        //        if (p_part.Height.Value != 0)
        //        {
        //            double a = p_part.ActualHeight - reference_part_height / 2 < 0 ? p_part.ActualHeight : p_part.ActualHeight - reference_part_height / 2;
        //            p_part.Height = new GridLength(a, GridUnitType.Star);
        //        }

        //        r_part.Height = new GridLength(reference_part_height, GridUnitType.Star);
        //        this.ReferenceDataGrid.Visibility = Visibility.Visible;
        //        this.referenceToggleBtn.Style = (Style)FindResource("CollapsedButton_24");
        //    }
        //}
        private void r_GridSplitter_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.ReferenceDataGrid.Visibility == Visibility.Collapsed)
            {
                e.Handled = true;
            }
        }

        private void CopyMenu_Reference_Click(object sender, RoutedEventArgs e)
        {
            if (ReferenceDataGrid.SelectedItems.Count >= 0)
            {
                if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                   && CommonUtils.CopyDic["COPYMODE"].Count > 0
                   && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
                {
                    this.ReferenceDataGrid.CopyData_Single(false, "2");
                }
                else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                  && CommonUtils.CopyDic["COPYMODE"].Count > 0
                  && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
                {
                    this.ReferenceDataGrid.CopyData_Single(false, "3");
                }
                else
                {
                    this.ReferenceDataGrid.CopyData_Single(false, "1");
                }
                FQSendWindow fqSendWindow = new FQSendWindow(true);
                fqSendWindow.Owner = Application.Current.MainWindow;
                fqSendWindow.Show();
            }
        }

        private void dgMenu_Reference_Opened(object sender, RoutedEventArgs e)
        {
            StartQQ_Reference.IsEnabled = false;
            StartQQ2_Reference.IsEnabled = false;
            if (ReferenceDataGrid.SelectedItems.Count == 1)
            {
                if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Reference != null)
                {
                    StartQQ_Reference.IsEnabled = true;
                    StartQQ2_Reference.IsEnabled = true;
                }
            }


            //publishBtn_Reference.Visibility = Visibility.Collapsed;
            //hiddenBtn_Reference.Visibility = Visibility.Collapsed;
            //refBtn_Reference.Visibility = Visibility.Collapsed;
            //recoverBtn_Reference.Visibility = Visibility.Collapsed;
            //AddIntention_Reference.Visibility = Visibility.Collapsed;

            //if (ReferenceDataGrid.SelectedItems.Count == 1)
            //{
            //    if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Reference != null)
            //    {
            //        IntentionQuoteItem item = _myOrderDefaultViewModel.SelectedItem_Reference;
            //        switch (item.Status)
            //        {
            //            case "0":
            //                publishBtn_Reference.Visibility = Visibility.Visible;
            //                refBtn_Reference.Visibility = Visibility.Visible;

            //                if (item.QuoteType == "DEMAND")
            //                {
            //                    AddIntention_Reference.Visibility = Visibility.Visible;
            //                }
            //                break;
            //            case "1":
            //                hiddenBtn_Reference.Visibility = Visibility.Visible;
            //                refBtn_Reference.Visibility = Visibility.Visible;
            //                break;
            //            case "2":
            //                break;
            //            case "3":
            //            case "4":
            //                recoverBtn_Reference.Visibility = Visibility.Visible;
            //                break;
            //        }
            //    }
            //}
        }

        //发布
        private void publishBtn_Reference_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_DoAllConfirmCommand(ReferenceDataGrid.SelectedItems);
            }
        }

        //隐藏
        private void hiddenBtn_Reference_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_DoAllNewCommand(ReferenceDataGrid.SelectedItems);
            }
        }

        //REF
        private void refBtn_Reference_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_DoAllIgnoreCommand(ReferenceDataGrid.SelectedItems, "DEMAND");
            }
        }

        //恢复（私域池）
        private void recoverBtn_Reference_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_DoAllRecoverCommand(ReferenceDataGrid.SelectedItems, "DEMAND");
            }
        }

        //发个需
        private void AddIntention_Reference_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_DoAllToReferenceCommand(ReferenceDataGrid.SelectedItems);
            }
        }
        private void StartQQ_Reference_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Reference != null)
            {
                _myOrderDefaultViewModel.Sub_DoStartQQCommand(false, _myOrderDefaultViewModel.SelectedItem_Reference);
            }
        }

        private void StartQQ2_Reference_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Reference != null)
            {
                _myOrderDefaultViewModel.Sub_DoStartQQCommand(true, _myOrderDefaultViewModel.SelectedItem_Reference);
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
             //if (FilterPopup.IsOpen == false)
            //{
            //    FilterPopup.IsOpen = true;
            //}
            //else if (FilterPopup.IsOpen == true)
            //{
            //    FilterPopup.IsOpen = false;
            //}

            if (btn_Fileter.Content.ToString() == "更多筛选")
            {
                if (person_part.ActualHeight > 230)
                {
                    (FilterPopup.Parent as Grid).Children.Remove(FilterPopup);
                    person_part.Children.Add(FilterPopup);
                    FilterPopup.VerticalAlignment = VerticalAlignment.Bottom;
                }
                else
                {
                    (FilterPopup.Parent as Grid).Children.Remove(FilterPopup);
                    demand_part.Children.Add(FilterPopup);
                    FilterPopup.VerticalAlignment = VerticalAlignment.Top;
                }


                FilterPopup.Visibility = Visibility.Visible;

                IntentionPart.GetCustomDefaultDataGrid().Opacity = 0.6;
                maskBorder.Visibility = Visibility.Visible;
                //btn_Fileter.IsEnabled = false;
                this.btn_Fileter.Content = "关闭筛选";



                //Storyboard storyboard = (Storyboard)this.Resources["MoveStoryboard"];
                //storyboard.Begin();

                WriteDBLogHelper.Instance.GenRecord("personal", "019", "更多筛选");
            }
            else
            {
                FilterPopup.Visibility = Visibility.Collapsed;

                IntentionPart.GetCustomDefaultDataGrid().Opacity = 1;
                maskBorder.Visibility = Visibility.Collapsed;
                //btn_Fileter.IsEnabled = true;
                this.btn_Fileter.Content = "更多筛选";

                
            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.StartTermDY = string.Empty;
                _myOrderDefaultViewModel.EndTermDY = string.Empty;
                _myOrderDefaultViewModel.IsAppTerm = false;

                _myOrderDefaultViewModel.StartCoupon = string.Empty;
                _myOrderDefaultViewModel.EndCoupon = string.Empty;
                _myOrderDefaultViewModel.IsCoupon = false;

                _myOrderDefaultViewModel.FocusTags = string.Empty;

                _myOrderDefaultViewModel.StartPrice = string.Empty;
                _myOrderDefaultViewModel.EndPrice = string.Empty;
                _myOrderDefaultViewModel.IsPrice = false;

                _myOrderDefaultViewModel.StartChinaBondValue = string.Empty;
                _myOrderDefaultViewModel.EndChinaBondValue = string.Empty;

                _myOrderDefaultViewModel.StartBidBp = string.Empty;
                _myOrderDefaultViewModel.EndBidBp = string.Empty;

                _myOrderDefaultViewModel.StartOfrBp = string.Empty;
                _myOrderDefaultViewModel.EndOfrBp = string.Empty;

                _myOrderDefaultViewModel.SelectedItem_Tag = null;
                //_myOrderDefaultViewModel.DesSource.Clear();

                _myOrderDefaultViewModel.TimeLimitSelectedIndex = 0;

                _myOrderDefaultViewModel.SelectedItem_Working = null;

                //if (_myOrderDefaultViewModel is IntentionControl_Buy_ViewModel)
                //{
                //    (_myOrderDefaultViewModel as IntentionControl_Buy_ViewModel)._isBatchOps = false;
                //    (_myOrderDefaultViewModel as IntentionControl_Buy_ViewModel)._cacheSelectedItems?.Clear();
                //}
               
            }
            WriteDBLogHelper.Instance.GenRecord("personal", "020", "show all");
        }

        private void FilterPopup_Opened(object sender, EventArgs e)
        {
            IntentionPart.GetCustomDefaultDataGrid().Opacity = 0.6;
            maskBorder.Visibility = Visibility.Visible;
            //btn_Fileter.IsEnabled = false;
            this.btn_Fileter.Content = "关闭筛选";

        }

        private void FilterPopup_Closed(object sender, EventArgs e)
        {
            IntentionPart.GetCustomDefaultDataGrid().Opacity = 1;
            maskBorder.Visibility = Visibility.Collapsed;
            //btn_Fileter.IsEnabled = true;
            this.btn_Fileter.Content = "更多筛选";
        }


        //private void FilterPopup_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    Application.Current.MainWindow.Activate();
        //}

        //private void GridSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        //{
        //    FilterPopup.UpdatePopupPosition();
        //}

        private void CopyMenu_SecurityID_Reference_Click(object sender, RoutedEventArgs e)
        {
            this.ReferenceDataGrid.CopyData_SecurityID();
        }

        private void CopyMenu_Symbol_Reference_Click(object sender, RoutedEventArgs e)
        {
            this.ReferenceDataGrid.CopyData_Symbol();
        }

        private void person_part_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (FilterPopup.Visibility == Visibility.Visible)
            {
                if (person_part.ActualHeight > 230)
                {
                    (FilterPopup.Parent as Grid).Children.Remove(FilterPopup);
                    person_part.Children.Add(FilterPopup);
                    FilterPopup.VerticalAlignment = VerticalAlignment.Bottom;
                }
                else
                {
                    (FilterPopup.Parent as Grid).Children.Remove(FilterPopup);
                    demand_part.Children.Add(FilterPopup);
                    FilterPopup.VerticalAlignment = VerticalAlignment.Top;
                }
            }
        }

        //private bool _verClose = false;
        //private void BtnVer_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.IsInquiry == "1")
        //    {
        //        if (!_verClose)
        //    {
        //        person_part.ColumnDefinitions[2].Width = new GridLength(0);
        //        _verClose = true;

        //        btnVer.Content = "<";

        //    }
        //    else
        //    {
        //        person_part.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        //        person_part.ColumnDefinitions[2].Width = new GridLength(300, GridUnitType.Pixel);
        //        _verClose = false;

        //        btnVer.Content = ">";
        //    }
        //    }
            
        //}

        //private void OpenInquiry()
        //{
        //    if (_verClose)
        //    {
        //        BtnVer_Click(null, null);
        //    }
        //}

        //private void CloseInquiry()
        //{
        //    if (!_verClose)
        //    {
        //        BtnVer_Click(null, null);
        //    }
        //}

        private void ChangeSelectedRow(IntentionQuoteItem item)
        {
            SelectedRowChanged?.BeginInvoke(item, null, null);
        }

        private void FilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                List<string> statuslist = new List<string>();
                foreach (ListBoxItem item in FilterListBox.SelectedItems)
                {
                    if (item.Content.ToString() == "有效")
                    {
                        statuslist.Add("1");
                    }
                    else if (item.Content.ToString() == "已REF")
                    {
                        statuslist.Add("4");
                    }

                }
                _myOrderDefaultViewModel.FilterInquiryStatus(statuslist);
            }
        }

        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_OnMarketCommand(_myOrderDefaultViewModel.SelectedItem_Working);
            }
        }

        private void refAll_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.OnRefAllInquiry();
            }
        }

        private void customFilterPanel_ShowAllAction(object sender, RoutedEventArgs e)
        {
            WriteDBLogHelper.Instance.GenRecord("personal", "020", "show all");
        }

        private void PriceNU_Edit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
                {
                    string groupKey = $"{_myOrderDefaultViewModel.SelectedItem_Working.QuoteType}_{_myOrderDefaultViewModel.SelectedItem_Working.Trader_Buyer}_{_myOrderDefaultViewModel.SelectedItem_Working.SecurityId}_{_myOrderDefaultViewModel.SelectedItem_Working.Side}";
                    _myOrderDefaultViewModel.BatchUpdateInquiry(groupKey);
                }
            }
        }

        private void QuantityNU_edit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.QuantityNU_edit.NumericValue < this.QuantityNU_edit.MinValue)
                {
                    this.QuantityNU_edit.NumericValue = this.QuantityNU_edit.MinValue;
                }
                else if (this.QuantityNU_edit.NumericValue > this.QuantityNU_edit.MaxValue)
                {
                    this.QuantityNU_edit.NumericValue = this.QuantityNU_edit.MaxValue;
                }
                if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
                {
                    string groupKey = $"{_myOrderDefaultViewModel.SelectedItem_Working.QuoteType}_{_myOrderDefaultViewModel.SelectedItem_Working.Trader_Buyer}_{_myOrderDefaultViewModel.SelectedItem_Working.SecurityId}_{_myOrderDefaultViewModel.SelectedItem_Working.Side}";
                    _myOrderDefaultViewModel.BatchUpdateInquiry(groupKey);
                }
            }
        }

        private void BtnHor_Click(object sender, RoutedEventArgs e)
        {
            //未收缩
            if (btnHor.Content.ToString() == "∨")
            {
                p_part.Height = new GridLength(2, GridUnitType.Star);
                r_part.Height = new GridLength(0);
                btnHor.Content = "∧";
            }
            else
            {
                p_part.Height = new GridLength(2, GridUnitType.Star);
                r_part.Height = new GridLength(1, GridUnitType.Star);
                btnHor.Content = "∨";
            }
        }

        private bool isMeCheckOps = false;
        private void isMeCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (isTradersCBOps)
            {
                isTradersCBOps = false;
                return;
            }
            isMeCheckOps = true;
            if (TradersCB != null)
            {
                this.TradersCB.SelectedValues = LocalEnv.UserInfo.UserName;
            }
            
            if (_myOrderDefaultViewModel is IntentionControl_Buy_ViewModel)
            {
                (_myOrderDefaultViewModel as IntentionControl_Buy_ViewModel).ProcessFilterIsMe(true);
            }
        }

        private void isMeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isTradersCBOps)
            {
                isTradersCBOps = false;
                return;
            }
            isMeCheckOps = true;
            if (TradersCB != null)
            {
                this.TradersCB.SelectedValues = string.Empty;
            }
            if (_myOrderDefaultViewModel is IntentionControl_Buy_ViewModel)
            {
                (_myOrderDefaultViewModel as IntentionControl_Buy_ViewModel).ProcessFilterIsMe(false);
            }
        }

        private void dateStartPicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if (e.Key == Key.Back || e.Key == Key.Delete)
                {
                    dateStartPicker.SelectedDate = null;
                }
            }
        }

        private void dateEndPicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if (e.Key == Key.Back || e.Key == Key.Delete)
                {
                    dateEndPicker.SelectedDate = null;
                }
            }
        }

        private void SearchIntentionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this._myOrderDefaultViewModel != null)
            {
                if ((this._myOrderDefaultViewModel.DealTimeBegin == null && this._myOrderDefaultViewModel.DealTimeEnd != null)
                    || (this._myOrderDefaultViewModel.DealTimeBegin != null && this._myOrderDefaultViewModel.DealTimeEnd == null)
                    || (this._myOrderDefaultViewModel.DealTimeBegin != null && this._myOrderDefaultViewModel.DealTimeEnd != null 
                        && ((this._myOrderDefaultViewModel.DealTimeEnd - this._myOrderDefaultViewModel.DealTimeBegin).Value.TotalDays > 30 || (this._myOrderDefaultViewModel.DealTimeEnd - this._myOrderDefaultViewModel.DealTimeBegin).Value.TotalDays < 0)))
                {
                    CustomMessageBox.Info("界面不支持30天以上筛选,\r\n请修改日期区间", "查询");
                    return;
                }
                this._myOrderDefaultViewModel.OnSearchIntentionDataCommand();
            }
        }

        private void TagsListChanged(object sender, RoutedEventArgs e)
        {
            FilterOptions fo = sender as FilterOptions;
            if (fo != null)
            {
                if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel is IntentionControl_Buy_ViewModel)
                {
                    (_myOrderDefaultViewModel as IntentionControl_Buy_ViewModel).ProcessSideFilter(fo.TagsList);
                }
            }
        }

        private void price_save_btn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
            {
                string groupKey = $"{_myOrderDefaultViewModel.SelectedItem_Working.QuoteType}_{_myOrderDefaultViewModel.SelectedItem_Working.Trader_Buyer}_{_myOrderDefaultViewModel.SelectedItem_Working.SecurityId}_{_myOrderDefaultViewModel.SelectedItem_Working.Side}";
                _myOrderDefaultViewModel.BatchUpdateInquiry(groupKey);
                tempInquiryPrice = this.PriceNU_Edit.NumericValue;
                tempPriceType = _myOrderDefaultViewModel.PriceType_Edit;
            }
            _myOrderDefaultViewModel.IsEditPrice = false;
        }

        private void qty_save_btn_Click(object sender, RoutedEventArgs e)
        {
            if (this.QuantityNU_edit.NumericValue < this.QuantityNU_edit.MinValue)
            {
                this.QuantityNU_edit.NumericValue = this.QuantityNU_edit.MinValue;
            }
            else if (this.QuantityNU_edit.NumericValue > this.QuantityNU_edit.MaxValue)
            {
                this.QuantityNU_edit.NumericValue = this.QuantityNU_edit.MaxValue;
            }
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
            {
                string groupKey = $"{_myOrderDefaultViewModel.SelectedItem_Working.QuoteType}_{_myOrderDefaultViewModel.SelectedItem_Working.Trader_Buyer}_{_myOrderDefaultViewModel.SelectedItem_Working.SecurityId}_{_myOrderDefaultViewModel.SelectedItem_Working.Side}";
                _myOrderDefaultViewModel.BatchUpdateInquiry(groupKey);
                tempInquiryQty = this.QuantityNU_edit.NumericValue;
            }
            qty_save_btn.Visibility = Visibility.Hidden;
        }

        decimal? tempInquiryPrice;
        string tempPriceType = "3";
        private void PriceNU_Edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Keyboard.FocusedElement != this.price_save_btn
                && Keyboard.FocusedElement != this.cbPriceType_edit)
            {
                bool flag = false;
                if (Keyboard.FocusedElement is ComboBoxItem)
                {
                    ComboBoxItem cbItem = Keyboard.FocusedElement as ComboBoxItem;
                    KeyValuePair<string, string> subItem = (KeyValuePair<string, string>)cbItem.DataContext;
                    flag = this.cbPriceType_edit.Items.Contains(subItem);
                }

                if (flag == false)
                {
                    if (price_save_btn != null)
                    {
                        _myOrderDefaultViewModel.IsEditPrice = false;
                    }
                    this.PriceNU_Edit.NumericValue = tempInquiryPrice;
                    _myOrderDefaultViewModel.PriceType_Edit = tempPriceType;
                }
            }
        }

        private void PriceNU_Edit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (price_save_btn != null)
            {
                _myOrderDefaultViewModel.IsEditPrice = true;
            }
            tempInquiryPrice = this.PriceNU_Edit.NumericValue;
        }

        decimal? tempInquiryQty;
        private void QuantityNU_edit_LostFocus_1(object sender, RoutedEventArgs e)
        {
            if (Keyboard.FocusedElement != this.qty_save_btn)
            {
                if (price_save_btn != null)
                {
                    qty_save_btn.Visibility = Visibility.Hidden;
                }
                this.QuantityNU_edit.NumericValue = tempInquiryQty;
            }
        }

        private void QuantityNU_edit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (qty_save_btn != null)
            {
                qty_save_btn.Visibility = Visibility.Visible;
            }
            tempInquiryQty = this.QuantityNU_edit.NumericValue;
        }

        private void ToQbMenu_Reference_Click(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.IsBDBAppName() && _myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Reference != null)
            {
                CommonUtils.OpenSmallBondWin(_myOrderDefaultViewModel.SelectedItem_Reference.SecurityId);
            }
        }

        private void inquiryDg_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
            {
                DataGrid dataGrid = sender as DataGrid;
                var column = e.Column;
                string sortPropertyName = e.Column.SortMemberPath;

                if (!string.IsNullOrEmpty(sortPropertyName))
                {
                    // stop the default sort
                    e.Handled = true;

                    ListSortDirection newDirection;
                    if (column.SortDirection == ListSortDirection.Ascending)
                    {
                        newDirection = ListSortDirection.Descending;
                    }
                    else
                    {
                        newDirection = ListSortDirection.Ascending;
                    }
                    column.SortDirection = newDirection;

                    //dataGrid.Items.SortDescriptions.Clear();
                    //dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, newDirection));
                    dataGrid.Items.SortDescriptions.Clear();
                    dataGrid.Items.SortDescriptions.Add(new SortDescription("TempSort", newDirection));
                    dataGrid.Items.SortDescriptions.Add(new SortDescription("SortKey", ListSortDirection.Ascending));
                    dataGrid.Items.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));
                    dataGrid.Items.SortDescriptions.Add(new SortDescription("Sn", ListSortDirection.Ascending));
                    dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, newDirection));

                    if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel is IntentionControl_Buy_ViewModel)
                    {
                        (_myOrderDefaultViewModel as IntentionControl_Buy_ViewModel).ResetCustomSort_Inquiry(newDirection, column.SortMemberPath);
                    }
                }
            }
        }

        private void inquiryDg_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item is IntentionMarketItem)
            {
                if ((e.Row.Item as IntentionMarketItem).Type == "G")
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void SettlTypeCombox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ComboBox cb = sender as System.Windows.Controls.ComboBox;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if ((cb.DataContext as IntentionMarketItem).AddOneOpenedTag == "1")
                {
                    if (cb.DataContext != null && cb.DataContext is IntentionMarketItem)
                    {
                        (cb.DataContext as IntentionMarketItem).AddOneOpenedTag = "0";
                    }
                    e.Handled = true;
                }
            }
        }

        private void cbPriceType_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Keyboard.FocusedElement != this.price_save_btn
                && Keyboard.FocusedElement != this.cbPriceType_edit)
            {
                bool flag = false;
                if (Keyboard.FocusedElement is ComboBoxItem)
                {
                    ComboBoxItem cbItem = Keyboard.FocusedElement as ComboBoxItem;
                    KeyValuePair<string, string> subItem = (KeyValuePair<string, string>)cbItem.DataContext;
                    flag = this.cbPriceType_edit.Items.Contains(subItem);
                }
                
                if (flag == false)
                {
                    if (price_save_btn != null)
                    {
                        _myOrderDefaultViewModel.IsEditPrice = false;
                    }
                    this.PriceNU_Edit.NumericValue = tempInquiryPrice;
                    _myOrderDefaultViewModel.PriceType_Edit = tempPriceType;
                }
            }
        }
    }
}
