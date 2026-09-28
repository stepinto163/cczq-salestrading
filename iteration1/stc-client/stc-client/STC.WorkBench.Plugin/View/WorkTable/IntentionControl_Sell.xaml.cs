using STC.Common.Data;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.FQ;
using STC.WorkBench.Plugin.ViewModel.Data;
using STC.WorkBench.Plugin.ViewModel.WorkTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// IntentionControl_Sell.xaml 的交互逻辑
    /// </summary>
    public partial class IntentionControl_Sell : UserControl, IntentionControlInterface
    {
        public Action ClickWorkingDataGridAction { get; set; }

        public Action OpenInquiryAction { get; set; }

        public Action CloseInquiryAction { get; set; }

        public Action<IntentionQuoteItem> ChangeSelectedRowAction { get; set; }

        public IntentionControl_Sell()
        {
            InitializeComponent();

            if (CommonUtils.IsBDBAppName())
            {
                ToQbMenu.Visibility = Visibility.Visible;
            }

            if (!LocalEnv.ConfInfo.IsInquiry)
            {
                DataGridColumn dataGridColumn = null;
                foreach(DataGridColumn column in this.WorkingDataGrid.Columns)
                {
                    if (column.Header.ToString() == "询价渠道")
                    {
                        dataGridColumn = column;
                    }
                }
                if (dataGridColumn != null)
                {
                    this.WorkingDataGrid.Columns.Remove(dataGridColumn);
                }
            }


            MenuItem sortUpdateTimeMi = new MenuItem();
            sortUpdateTimeMi.Name = "sortUpdateTimeColumnMi";
            sortUpdateTimeMi.Header = "按创建时间排序";
            sortUpdateTimeMi.Style = (Style)this.FindResource("DefaultMenuItem");
            sortUpdateTimeMi.Click += sortUpdateTimeColumnMi_Click;
            this.WorkingDataGrid.GetHeaderContextMenu().Items.Add(sortUpdateTimeMi);


            if (!LocalEnv.ConfInfo.IsQQ)
            {
                StartQQ.Visibility = Visibility.Collapsed;
                StartQQ2.Visibility = Visibility.Collapsed;
            }

            if (!IntentionOldHandler.Instance.IsOrderOther)
            {
                this.Trader.IsReadOnly = true;
            }

            this.WorkingDataGrid.SelectionChanged += WorkingDataGrid_SelectionChanged;
            this.WorkingDataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.WorkingDataGrid_MouseLeftButtonDown), true);

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

        private void sortUpdateTimeColumnMi_Click(object sender, RoutedEventArgs e)
        {
            this.WorkingDataGrid.Items.SortDescriptions.Clear();
            this.WorkingDataGrid.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("CreateTime", System.ComponentModel.ListSortDirection.Descending));
        }

        private void WorkingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                if (this.WorkingDataGrid.SelectedItems.Count > 0)
                {
                    _myOrderDefaultViewModel.IsBestOperationGrid = Visibility.Visible;
                }
                else
                {
                    _myOrderDefaultViewModel.IsBestOperationGrid = Visibility.Collapsed;
                }
            }

            if ((sender as DataGrid).SelectedItems.Count > 0)
            {
                // 清空更多筛选
                _myOrderDefaultViewModel.StartTermDY = string.Empty;
                _myOrderDefaultViewModel.EndTermDY = string.Empty;
                _myOrderDefaultViewModel.IsAppTerm = false;

                _myOrderDefaultViewModel.StartCoupon = string.Empty;
                _myOrderDefaultViewModel.EndCoupon = string.Empty;
                _myOrderDefaultViewModel.IsCoupon = false;

                _myOrderDefaultViewModel.SelectedItem_Tag = null;
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

                OpenInquiryAction?.Invoke();
            }
            else
            {
                CloseInquiryAction?.Invoke();
            }


            _myOrderDefaultViewModel.SelectedItems_Working = (sender as DataGrid).SelectedItems;
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
            {
                ChangeSelectedRowAction?.Invoke(_myOrderDefaultViewModel.SelectedItem_Working);
            }
        }


        int i_dg = 0;
        bool isDouble_dg = false;
        DataGridRow _lastDGR_dg = null;
        private void WorkingDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string headerName = string.Empty;
            DataGridRow dgr = null;
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            if (hit != null)
            {
                DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
                while (cell != null && !(cell is System.Windows.Controls.DataGridCell))
                    cell = VisualTreeHelper.GetParent(cell);

                System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;


                if (targetCell != null)
                {
                    headerName = targetCell.Column.Header.ToString();
                    dgr = WorkingDataGrid.ItemContainerGenerator.ContainerFromItem(targetCell.DataContext) as DataGridRow;
                    //DataGrid dg = sender as DataGrid;
                    //dg.CurrentColumn = targetCell.Column;
                }
            }


            i_dg += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i_dg = 0; UIElement_Click(sender, e); isDouble_dg = false; };
            timer.IsEnabled = true;
            if (i_dg > 0 && i_dg % 2 == 0)
            {
                timer.IsEnabled = false;
                i_dg = 0;
                isDouble_dg = true;

                //双击时执行的代码
                //MessageBox.Show("DoubleClick!");
                if (dgr != null && _lastDGR_dg == dgr)
                {
                    if (headerName == "来源方机构" || headerName == "来源方"
                    || headerName == "IM号" || headerName == "群消息")
                    {
                        // 双击
                        _myOrderDefaultViewModel.StartQQCommand.Execute(null);
                    }
                    else if (headerName == "个人笔记")
                    {
                        if (_myOrderDefaultViewModel.SelectedItem_Working != null)
                        {
                            ModifyPersonalRemarksView x = new ModifyPersonalRemarksView();
                            x.memo.Text = _myOrderDefaultViewModel.SelectedItem_Working.PersonalRemarks;
                            if (x.ShowDialog() == true)
                            {
                                List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                                modifyList.Add(_myOrderDefaultViewModel.SelectedItem_Working);
                                _myOrderDefaultViewModel.EditPersonalMarks(modifyList, null, x.memo.Text);
                            }
                        }
                    }
                    else if (headerName == "定价" || headerName == "报量(万)")
                    {

                    }
                    else if (!string.IsNullOrEmpty(headerName))
                    {
                        if (CommonUtils.IsBDBAppName())
                        {
                            if (headerName == "代码" || headerName == "简称")
                            {
                                if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
                                {
                                    CommonUtils.OpenSmallBondWin(_myOrderDefaultViewModel.SelectedItem_Working.SecurityId);
                                }
                            }
                        }

                        if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null
                        && _myOrderDefaultViewModel.SelectedItem_Working.Status != "2"
                        && _myOrderDefaultViewModel.SelectedItem_Working.Status != "3"
                        && _myOrderDefaultViewModel.SelectedItem_Working.Status != "4"
                        && (CommonUtils.IsBDBAppName() == false || (headerName != "代码" && headerName != "简称")))
                        {
                            _myOrderDefaultViewModel.EditOrder();
                        }
                    }
                }
            }
            else
            {
                _lastDGR_dg = dgr;
            }
        }

        private void UIElement_Click(object sender, MouseButtonEventArgs e)
        {
            if (isDouble_dg)
                return;

            ClickWorkingDataGridAction?.Invoke();

            //if (this.FilterPopup.Visibility == Visibility.Visible)
            //{
            //    this.FilterPopup.Visibility = Visibility.Collapsed;
            //    WorkingDataGrid.Opacity = 1;
            //    maskBorder.Visibility = Visibility.Collapsed;
            //    //btn_Fileter.IsEnabled = true;
            //    this.btn_Fileter.Content = "更多筛选";
            //}
            ////if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
            ////{
            ////    SelectedRowChanged?.BeginInvoke(_myOrderDefaultViewModel.SelectedItem_Working, null, null);
            ////}
        }

        private void dgMenu_Post_Opened(object sender, RoutedEventArgs e)
        {
            _myOrderDefaultViewModel.PublishBtnH = 0;
            _myOrderDefaultViewModel.RefBtnH = 0;
            _myOrderDefaultViewModel.HiddenBtnH = 0;
            _myOrderDefaultViewModel.RecoverBtnH = 0;
            _myOrderDefaultViewModel.AddBrokerBtnH = 0;
            _myOrderDefaultViewModel.CancelBrokerBtnH = 0;
            StartQQ.IsEnabled = false;
            StartQQ2.IsEnabled = false;
            if (WorkingDataGrid.SelectedItems.Count == 1)
            {
                if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
                {
                    StartQQ.IsEnabled = true;
                    StartQQ2.IsEnabled = true;

                    IntentionQuoteItem item = _myOrderDefaultViewModel.SelectedItem_Working;
                    if (item.FQInfo != null)
                    {
                        item.IM = item.FQInfo.fromQQ;
                        if (LocalEnv.ConfInfo.IsQQ)
                        {
                            if (string.IsNullOrEmpty(item.IM))
                            {
                                _myOrderDefaultViewModel.IsEnable_StartQQBtn = false;
                            }
                            else
                            {
                                _myOrderDefaultViewModel.IsEnable_StartQQBtn = true;
                            }
                        }
                        //info2里面如果有ADD_WHITE_LIST=1,+号就显示
                        //只有私聊显示 
                        _myOrderDefaultViewModel.setAddVisible(item);
                        switch (item.Status)
                        {
                            case "0":
                                _myOrderDefaultViewModel.PublishBtnH = 30;
                                _myOrderDefaultViewModel.RefBtnH = 30;
                                break;
                            case "1":
                                _myOrderDefaultViewModel.HiddenBtnH = 30;
                                _myOrderDefaultViewModel.RefBtnH = 30;
                                break;
                            case "2":
                                break;
                            case "3":
                            case "4":
                                _myOrderDefaultViewModel.RecoverBtnH = 30;
                                break;
                        }
                        if (string.IsNullOrEmpty(item.FQInfo.toGroupOpenID) && !string.IsNullOrEmpty(item.IM))
                        {
                            if ("1".Equals(item.AddVisible))
                                _myOrderDefaultViewModel.AddBrokerBtnH = 30;
                            else
                                _myOrderDefaultViewModel.CancelBrokerBtnH = 30;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 操作，取消询价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelMarketBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
            {
                _myOrderDefaultViewModel.OnRefAllInquiry();
            }
            else
            {
                Common.View.Component.CustomMessageBox.Info($"请先选择一条个需", "撤销询价");
            }


            WriteDBLogHelper.Instance.GenRecord("personal", "031", "取消询价");
        }

        /// <summary>
        /// 操作，ref
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefBtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                modifyList.Add((sender as Button).DataContext as IntentionQuoteItem);
                _myOrderDefaultViewModel.Sub_DoAllIgnoreCommand(modifyList);
            }
            WriteDBLogHelper.Instance.GenRecord("personal", "009", "撤销");
        }

        private void FinishBtn_Single_Click(object sender, RoutedEventArgs e)
        {
            WriteDBLogHelper.Instance.GenRecord("personal", "010", "完成");

            if (_myOrderDefaultViewModel != null)
            {
                ModifyPersonalRemarksView x = new ModifyPersonalRemarksView();
                if (x.ShowDialog() == true)
                {
                    List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                    modifyList.Add((sender as Button).DataContext as IntentionQuoteItem);
                    _myOrderDefaultViewModel.EditPersonalMarks(modifyList, null, x.memo.Text, true);
                }
            }
        }

        private void BestCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.WorkingDataGrid.SelectedItems == null || this.WorkingDataGrid.SelectedItems.Count == 0)
            {
                return;
            }

            if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
               && CommonUtils.CopyDic["COPYMODE"].Count > 0
               && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
            {
                this.WorkingDataGrid.CopyData_Single(false, "2");
            }
            else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
            {
                this.WorkingDataGrid.CopyData_Single(false, "3");
            }
            else
            {
                this.WorkingDataGrid.CopyData_Single(false, "1");
            }
            FQSendWindow fqSendWindow = new FQSendWindow(true);
            fqSendWindow.Owner = Application.Current.MainWindow;
            fqSendWindow.Show();

            WriteDBLogHelper.Instance.GenRecord("personal", "003", "批量-复制");
        }

        private void CopyMenu_Click(object sender, RoutedEventArgs e)
        {
            this.WorkingDataGrid.CopyData_Single();
        }

        private void CopyMenu_SecurityID_Click(object sender, RoutedEventArgs e)
        {
            this.WorkingDataGrid.CopyData_SecurityID();
        }

        private void CopyMenu_Symbol_Click(object sender, RoutedEventArgs e)
        {
            this.WorkingDataGrid.CopyData_Symbol();
        }

        private void CopyMenu_Price_Click(object sender, RoutedEventArgs e)
        {
            this.WorkingDataGrid.CopyData_Single_Price();
        }

        private void SearchMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel.SelectedItem_Working != null)
            {
                _myOrderDefaultViewModel.FilterText = _myOrderDefaultViewModel.SelectedItem_Working.SecurityId;
            }
        }

        /// <summary>
        /// 个需右键，发内部
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void publishBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_DoAllConfirmCommand(WorkingDataGrid.SelectedItems);
            }
        }

        /// <summary>
        /// 个需右键，取消发内部
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hiddenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_DoAllNewCommand(WorkingDataGrid.SelectedItems);
            }
        }

        /// <summary>
        /// 个需右键，ref
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_DoAllIgnoreCommand(WorkingDataGrid.SelectedItems);
            }
        }

        // 个需右键，恢复
        private void recoverBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.Sub_DoAllRecoverCommand(WorkingDataGrid.SelectedItems);
            }
        }

        /// <summary>
        /// 个需右键， 完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FinishBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {

                List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                if (_myOrderDefaultViewModel.SelectedItems_Working != null && _myOrderDefaultViewModel.SelectedItems_Working.Count > 0)
                {
                    foreach (IntentionQuoteItem item in _myOrderDefaultViewModel.SelectedItems_Working)
                    {
                        if (item.Status == "0" || item.Status == "1")
                        {
                            modifyList.Add(item);
                        }
                    }
                }

                IntentionQuoteItem currentItem = null;
                ObservableCollection<IntentionMarketItem> datas = null;
                if (LocalEnv.ConfInfo.IsInquiry && _myOrderDefaultViewModel.SelectedItems_Working != null && _myOrderDefaultViewModel.SelectedItems_Working.Count == 1)
                {
                    if (_myOrderDefaultViewModel.SelectedItem_Working != null)
                    {
                        if (_myOrderDefaultViewModel._Cache_InquiryQuote_Dic.ContainsKey(_myOrderDefaultViewModel.SelectedItem_Working.IntentionId)
                           && _myOrderDefaultViewModel._Cache_InquiryQuote_Dic[_myOrderDefaultViewModel.SelectedItem_Working.IntentionId].Count > 0)
                        {
                            datas = new ObservableCollection<IntentionMarketItem>(_myOrderDefaultViewModel._Cache_InquiryQuote_Dic[_myOrderDefaultViewModel.SelectedItem_Working.IntentionId].Values.Where<IntentionMarketItem>(a => a.Status == "1" && a.SendType != 3 && a.OptionType != "sendIntentQQGroups").OrderByDescending(a => a.UpdateTime));
                            if (datas != null && datas.Count > 0)
                            {
                                currentItem = _myOrderDefaultViewModel.SelectedItem_Working;
                            }
                        }
                    }
                }

                if (modifyList.Count > 0)
                {
                    ModifyPersonalRemarksView x = new ModifyPersonalRemarksView(currentItem, datas);
                    if (_myOrderDefaultViewModel.SelectedItems_Working != null && _myOrderDefaultViewModel.SelectedItems_Working.Count == 1)
                    {
                        x.memo.Text = _myOrderDefaultViewModel.SelectedItem_Working.PersonalRemarks;
                    }
                    if (x.ShowDialog() == true)
                    {
                        _myOrderDefaultViewModel.EditPersonalMarks(modifyList, x.InquiryNotRefList, x.memo.Text, true);
                    }
                }
            }
        }


        private MyOrderDefaultViewModel _myOrderDefaultViewModel;
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _myOrderDefaultViewModel = e.NewValue as MyOrderDefaultViewModel;
        }


        //批量，

        private void PriceBtn_Click(object sender, RoutedEventArgs e)
        {
            Button priceBtn = sender as Button;
            if (_myOrderDefaultViewModel != null && priceBtn != null && priceBtn.CommandParameter != null)
            {
                _myOrderDefaultViewModel.EditPrice(priceBtn.CommandParameter.ToString());
            }
        }

        private void QtyBtn_Click(object sender, RoutedEventArgs e)
        {
            Button qtyBtn = sender as Button;
            if (_myOrderDefaultViewModel != null && qtyBtn != null && qtyBtn.CommandParameter != null)
            {
                _myOrderDefaultViewModel.EditQty(qtyBtn.CommandParameter.ToString());
            }
        }

        private void InsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Component.AutoCompleteComboBox instNameCb = sender as Component.AutoCompleteComboBox;
            if (string.IsNullOrEmpty(instNameCb.Text))
            {
                instNameCb.ExpandOptions();
            }
        }

        private void TrdNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Component.AutoCompleteComboBox trdNameCb = sender as Component.AutoCompleteComboBox;
            if (string.IsNullOrEmpty(trdNameCb.Text))
            {
                trdNameCb.ExpandOptions();
            }
        }

        private void SelectedInstitutionCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.FilterInstitution(args);
            }
        }


        private void SelectedUserIDCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_myOrderDefaultViewModel != null)
            {
                if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                {
                    _myOrderDefaultViewModel.FilterTraderLocal(args);
                }
                else
                {
                    _myOrderDefaultViewModel.FilterTrader(args);
                }
            }
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null)
            {
                _myOrderDefaultViewModel.EditClientInfo();

                _myOrderDefaultViewModel.HurryFlag = string.Empty;
                _myOrderDefaultViewModel.SuggestFlag = string.Empty;
                _myOrderDefaultViewModel.FireFlag = string.Empty;
                _myOrderDefaultViewModel.FloorPrice = string.Empty;
                _myOrderDefaultViewModel.OCOFlag = string.Empty;

                // 清空客户机构
                _myOrderDefaultViewModel.SelectedInstintuion_Edit = null;
                _myOrderDefaultViewModel.InstitutionName_Edit = null;
                // 清空客户名
                _myOrderDefaultViewModel.SelectedTrader_Edit = null;
                _myOrderDefaultViewModel.TraderName_Edit = null;
                //// 清空vip
                //vip_chk.IsChecked = false;
                // 清空客户类型
                _myOrderDefaultViewModel.SelectClientTypeItem = null;
                _myOrderDefaultViewModel.ClientTypeEnum = null;
            }

            WriteDBLogHelper.Instance.GenRecord("personal", "026", "来源方");
        }


        private void EditTags(object sender, RoutedEventArgs e)
        {
            _myOrderDefaultViewModel.EditTags();

            _myOrderDefaultViewModel.HurryFlag = string.Empty;
            _myOrderDefaultViewModel.SuggestFlag = string.Empty;
            _myOrderDefaultViewModel.FireFlag = string.Empty;
            _myOrderDefaultViewModel.FloorPrice = string.Empty;
            _myOrderDefaultViewModel.OCOFlag = string.Empty;

        }

        public Component.CustomDefaultDataGrid GetCustomDefaultDataGrid()
        {
            return WorkingDataGrid;
        }

        /// <summary>
        /// 操作，询价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MarketBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working != null)
            {
                Button btn = sender as Button;
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    _myOrderDefaultViewModel.Sub_OnMarketCommand(currentItem);
                }
            }
            else
            {
                Common.View.Component.CustomMessageBox.Info($"请先选择一条个需", "询价");
            }

            WriteDBLogHelper.Instance.GenRecord("personal", "030", "询价");
        }

        private void ToQbMenu_Click(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.IsBDBAppName() && _myOrderDefaultViewModel != null && _myOrderDefaultViewModel.SelectedItem_Working.SecurityId != null)
            {
                CommonUtils.OpenSmallBondWin(_myOrderDefaultViewModel.SelectedItem_Working.SecurityId);
            }
        }

        private void WorkingDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (_myOrderDefaultViewModel != null && e.Row.Item is IntentionQuoteItem)
            {
                IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                if (e.Column.Header.ToString() == "定价")
                {
                    string securityID = item.SecurityId;
                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                    if (bondsInfoPo != null)
                    {
                        // 含权
                        if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                        ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                        (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                        {

                            if (_myOrderDefaultViewModel.PriceTypeTableDic.Count == 2)
                            {
                                _myOrderDefaultViewModel.PriceTypeTableDic.Add(new KeyValuePair<string, string>("7", "行权"));
                            }
                        }
                        else
                        {
                            if (_myOrderDefaultViewModel.PriceTypeTableDic.Count == 3)
                            {
                                _myOrderDefaultViewModel.PriceTypeTableDic.RemoveAt(2);
                            }
                        }
                    }
                }
                _myOrderDefaultViewModel.EditRow(item);
            }
        }

        private void WorkingDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item != null && e.Row.Item is IntentionQuoteItem)
            {
                if (e.Column.Header != null)
                {
                    IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                    if (e.Column.Header.ToString() == "定价" && (item.Price_Edit != item.Price || item.PriceType_Edit != item.PriceType))
                    {
                        _myOrderDefaultViewModel.EditPrice_Custom(item.Price_Edit, item.PriceType_Edit);
                    }
                    else if (e.Column.Header.ToString() == "报量(万)" && item.Qty_Edit != item.Qty)
                    {
                        _myOrderDefaultViewModel.EditQty_Custom(item.Qty_Edit);
                    }
                }
            }
        }

        private void PriceNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void price_save_btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = sender as Button;
                if (btn != null && btn.DataContext != null && btn.DataContext is IntentionQuoteItem)
                {
                    IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;

                    this.WorkingDataGrid.CommitEdit();
                    //_myOrderDefaultViewModel.EditPrice_Custom(currentItem.Price_Edit, currentItem.PriceType_Edit, CancelEdit);
                }
            }
        }

        private void QuantityNU_edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void qty_save_btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = sender as Button;
                if (btn != null && btn.DataContext != null && btn.DataContext is IntentionQuoteItem)
                {
                    IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;

                    this.WorkingDataGrid.CommitEdit();
                    //_myOrderDefaultViewModel.EditQty_Custom(currentItem.Qty_Edit, CancelEdit);
                }
            }
        }
        private void CancelEdit()
        {
            this.Dispatcher.BeginInvoke((Action)(() => {
                this.WorkingDataGrid.CancelEdit();
            }));
        }
    }
}
