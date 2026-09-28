using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Table.WorkBrench;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.FQ;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.WorkBench.Plugin.ViewModel.Data;
using STC.WorkBench.Plugin.ViewModel.WorkTable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// IntentionControl_Buy.xaml 的交互逻辑
    /// </summary>
    public partial class IntentionControl_Buy : UserControl, IntentionControlInterface
    {
        public Action ClickWorkingDataGridAction { get; set; }

        public Action OpenInquiryAction { get; set; }

        public Action CloseInquiryAction { get; set; }

        public Action<IntentionQuoteItem> ChangeSelectedRowAction { get; set; }


        IntentionControl_Buy_ViewModel vm_buy;
        public IntentionControl_Buy()
        {
            InitializeComponent();

            Application.Current.MainWindow.MouseDown += MainWindow_MouseDown; ;

            //if (CommonUtils.IsInvestor())
            //{
            //    trader_filter.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    investor_filter.Visibility = Visibility.Visible;
            //    Dictionary<string, object> dic = new Dictionary<string, object>();
            //    List<StcTbookUsers> traders = CommonUtils.GetTraders();
            //    if (traders != null)
            //    {
            //        foreach (StcTbookUsers stcUserInfo in traders)
            //        {
            //            dic[stcUserInfo.Username] = stcUserInfo.Name;
            //        }
            //    }
            //    this.TradersCB.ItemsSource = dic;
            //    this.TradersCB.FilterDicChanged += TradersCB_FilterDicChanged;
            //    my_filter.Visibility = Visibility.Visible;
            //}

            if (CommonUtils.IsInvestor())
            {
                //AllProcessBtn.Visibility = Visibility.Collapsed;
                //AllFinishBtn.Visibility = Visibility.Collapsed;
                AllInquiryBtn.Visibility = Visibility.Collapsed;
            }

            this.WorkingDataGrid.Loaded += WorkingDataGrid_Loaded;

            //MenuItem sortUpdateTimeMi = new MenuItem();
            //sortUpdateTimeMi.Name = "sortUpdateTimeColumnMi";
            //sortUpdateTimeMi.Header = "按创建时间排序";
            //sortUpdateTimeMi.Style = (Style)this.FindResource("DefaultMenuItem");
            //sortUpdateTimeMi.Click += sortUpdateTimeColumnMi_Click;
            //this.WorkingDataGrid.GetHeaderContextMenu().Items.Add(sortUpdateTimeMi);

            if (!LocalEnv.ConfInfo.IsQQ)
            {
                StartQQ.Visibility = Visibility.Collapsed;
                //StartQQ2.Visibility = Visibility.Collapsed;
            }

            this.WorkingDataGrid.SelectionChanged += WorkingDataGrid_SelectionChanged;
            this.WorkingDataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.WorkingDataGrid_MouseLeftButtonDown), true);

        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 获取鼠标当前位置
            Point pntMouse = Mouse.GetPosition(Application.Current.MainWindow);
            // 获取子控件的Rect
            Point pntSub = this.WorkingDataGrid.TranslatePoint(new Point(), Application.Current.MainWindow);
            Rect rectSub = new Rect(pntSub, this.WorkingDataGrid.RenderSize);
            // 判断鼠标是否在子控件内
            bool isContain = rectSub.Contains(pntMouse);
            if (!isContain)
            {
                this.WorkingDataGrid.CancelEdit();
            }
        }

        private void TradersCB_FilterDicChanged(object sender, RoutedEventArgs e)
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

            vm_buy.ProcessFilterMyTraders(orgArr);
        }

        private void WorkingDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.IsInvestor())
            {
                foreach (DataGridColumn column in WorkingDataGrid.Columns)
                {
                    if (column.Header.ToString() == "询价状态")
                    {
                        column.Visibility = Visibility.Collapsed;
                    }
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

        int lastSelectedItemsCount = 0;
        private void WorkingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
            //{
            //    if (lastSelectedItemsCount > 1 && this.WorkingDataGrid.SelectedItems.Count == 1 && vm_buy._isBatchOps == false && isUserOps == false)
            //    {
            //        // 判断是由于Refresh导致的选中数量变化
            //        //Console.WriteLine($"-------------------------------------------:{this.WorkingDataGrid.SelectedItems.Count}");
            //        vm_buy._isBatchOps = true;
            //    }

            //    lastSelectedItemsCount = this.WorkingDataGrid.SelectedItems.Count;
            //    //Console.WriteLine($"44444444444444444444444444:{this.WorkingDataGrid.SelectedItems.Count}");

            //    if (vm_buy._isBatchOps)
            //    {
            //        isUserOps = false;
            //        DispatcherTimer timer = new DispatcherTimer();
            //        timer.Interval = new TimeSpan(0, 0, 0, 0, 500);// 大于表格刷新频率
            //        timer.Tick += (s, e1) =>
            //        {
            //            //停止timer
            //            timer.IsEnabled = false;
            //            //重置选中
            //            if (this.vm_buy._cacheSelectedItems != null && this.vm_buy._cacheSelectedItems.Count > 0)
            //            {
            //                //Console.WriteLine($"11111111111111111111111:{this.vm_buy._cacheSelectedItems.Count}");
            //                this.WorkingDataGrid.SelectedItems.Clear();
            //                foreach (var item in this.vm_buy._cacheSelectedItems)
            //                {
            //                    this.WorkingDataGrid.SelectedItems.Add(item);
            //                }
            //                this.WorkingDataGrid.ScrollIntoView(this.vm_buy._cacheSelectedItems[this.vm_buy._cacheSelectedItems.Count - 1]);
            //            }
            //            //vm_buy._cacheSelectedItems = null;
            //            vm_buy._isBatchOps = false;
            //        };
            //        timer.IsEnabled = true;
            //        return;
            //    }
            //}

            if (vm_buy != null)
            {
                if (this.WorkingDataGrid.SelectedItems.Count > 0)
                {
                    vm_buy.IsBestOperationGrid = Visibility.Visible;
                }
                else
                {
                    vm_buy.IsBestOperationGrid = Visibility.Collapsed;
                }
            }

            if ((sender as DataGrid).SelectedItems.Count > 0)
            {
                // 清空更多筛选
                vm_buy.StartTermDY = string.Empty;
                vm_buy.EndTermDY = string.Empty;
                vm_buy.IsAppTerm = false;

                vm_buy.StartCoupon = string.Empty;
                vm_buy.EndCoupon = string.Empty;
                vm_buy.IsCoupon = false;

                vm_buy.SelectedItem_Tag = null;
                vm_buy.FocusTags = string.Empty;

                vm_buy.StartPrice = string.Empty;
                vm_buy.EndPrice = string.Empty;
                vm_buy.IsPrice = false;

                vm_buy.StartChinaBondValue = string.Empty;
                vm_buy.EndChinaBondValue = string.Empty;

                vm_buy.StartBidBp = string.Empty;
                vm_buy.EndBidBp = string.Empty;

                vm_buy.StartOfrBp = string.Empty;
                vm_buy.EndOfrBp = string.Empty;

                OpenInquiryAction?.Invoke();
            }
            else
            {
                CloseInquiryAction?.Invoke();
            }


            vm_buy.SelectedItems_Working = (sender as DataGrid).SelectedItems;
            if (vm_buy != null && vm_buy.SelectedItem_Working != null)
            {
                ChangeSelectedRowAction?.Invoke(vm_buy.SelectedItem_Working);
            }

            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                    this.WorkingDataGrid.CopyData_Single(false);
                }));
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
                        vm_buy.StartQQCommand.Execute(null);
                    }
                    else if (headerName == "个人笔记")
                    {
                        if (vm_buy.SelectedItem_Working != null)
                        {
                            CacheSelectedItems_PA();

                            ModifyPersonalRemarksView x = new ModifyPersonalRemarksView();
                            x.memo.Text = vm_buy.SelectedItem_Working.PersonalRemarks;
                            if (x.ShowDialog() == true)
                            {
                                List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                                modifyList.Add(vm_buy.SelectedItem_Working);
                                vm_buy.EditPersonalMarks(modifyList, null, x.memo.Text);
                            }
                        }
                    }
                    else if (headerName == "报价" || headerName == "报量(万)")
                    {

                    }
                    else if (!string.IsNullOrEmpty(headerName))
                    {
                        if (vm_buy != null && vm_buy.SelectedItem_Working != null && vm_buy.SelectedItem_Working.Type != "INTENTION_GROUP"
                        && (vm_buy.SelectedItem_Working.Status == "80" || vm_buy.SelectedItem_Working.Status == "81"))
                        {
                            vm_buy.EditOrder();
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
            ////if (vm_buy != null && vm_buy.SelectedItem_Working != null)
            ////{
            ////    SelectedRowChanged?.BeginInvoke(vm_buy.SelectedItem_Working, null, null);
            ////}
        }

        private void dgMenu_Post_Opened(object sender, RoutedEventArgs e)
        {
            StartQQ.IsEnabled = false;
            //StartQQ2.IsEnabled = false;
            if (WorkingDataGrid.SelectedItems.Count == 1)
            {
                if (vm_buy != null && vm_buy.SelectedItem_Working != null)
                {
                    StartQQ.IsEnabled = true;
                    //StartQQ2.IsEnabled = true;
                }
            }

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    if (CommonUtils.IsInvestor())
                    {
                        List<string> list = new List<string>();
                        foreach (IntentionQuoteItem item in this.WorkingDataGrid.SelectedItems)
                        {
                            if (!list.Contains(item.Status))
                            {
                                list.Add(item.Status);
                            }
                        }

                        if (list.Count == 1 && list[0] == "81")
                        {
                            refBtn.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            refBtn.Visibility = Visibility.Collapsed;
                        }
                        
                        Inquiry_Menu.Visibility = Visibility.Collapsed;
                        processBtn.Visibility = Visibility.Collapsed;
                        finishBtn.Visibility = Visibility.Collapsed;
                        MatchExe_Menu.Visibility = Visibility.Collapsed;
                        Agree_Menu.Visibility = Visibility.Collapsed;
                        Disagree_Menu.Visibility = Visibility.Collapsed;
                        //addBrokerBtn.Visibility = Visibility.Collapsed;
                        //cancelBrokerBtn.Visibility = Visibility.Collapsed;
                        StartQQ.Visibility = Visibility.Collapsed;
                        //StartQQ2.Visibility = Visibility.Collapsed;
                        CloneMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        refBtn.Visibility = Visibility.Visible;
                        Inquiry_Menu.Visibility = Visibility.Visible;
                        processBtn.Visibility = Visibility.Visible;
                        finishBtn.Visibility = Visibility.Visible;
                        MatchExe_Menu.Visibility = Visibility.Visible;
                        Agree_Menu.Visibility = Visibility.Visible;
                        Disagree_Menu.Visibility = Visibility.Visible;
                        //addBrokerBtn.Visibility = Visibility.Visible;
                        //cancelBrokerBtn.Visibility = Visibility.Visible;
                        StartQQ.Visibility = Visibility.Collapsed;
                        //StartQQ2.Visibility = Visibility.Collapsed;
                        
                        if (WorkingDataGrid.SelectedItems.Count == 1)
                        {
                            if (vm_buy != null && vm_buy.SelectedItem_Working != null)
                            {
                                IntentionQuoteItem item = vm_buy.SelectedItem_Working;
                                if (item.FQInfo != null)
                                {
                                    if (LocalEnv.ConfInfo.IsQQ)
                                    {
                                        if (!string.IsNullOrEmpty(item.IM))
                                        {
                                            StartQQ.Visibility = Visibility.Visible;
                                            //StartQQ2.Visibility = Visibility.Visible;
                                        }
                                    }
                                }
                            }
                        }

                        //CopyMenu_Price.Visibility = Visibility.Visible;

                        if (this.WorkingDataGrid.SelectedItems.Count == 1)
                        {
                            
                            if (vm_buy.SelectedItem_Working.Type == "INTENTION_GROUP")
                            {
                                //汇总行
                                //CopyMenu_Price.Visibility = Visibility.Collapsed;
                                Inquiry_Menu.Visibility = Visibility.Visible;
                                processBtn.Visibility = Visibility.Collapsed;
                                refBtn.Visibility = Visibility.Collapsed;
                                finishBtn.Visibility = Visibility.Collapsed;
                                MatchExe_Menu.Visibility = Visibility.Collapsed;
                                Agree_Menu.Visibility = Visibility.Collapsed;
                                Disagree_Menu.Visibility = Visibility.Collapsed;
                                //addBrokerBtn.IsEnabled = false;
                                //cancelBrokerBtn.IsEnabled = false;
                                //StartQQ.IsEnabled = false;
                                //StartQQ2.IsEnabled = false;
                                CloneMenu.Visibility = Visibility.Collapsed;
                                return;
                            }
                            else
                            {
                                if (vm_buy.SelectedItem_Working.Status == "81"
                                    || vm_buy.SelectedItem_Working.Status == "84")
                                {
                                    MatchExe_Menu.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    MatchExe_Menu.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                        else
                        {
                            MatchExe_Menu.Visibility = Visibility.Collapsed;
                        }

                        CloneMenu.Visibility = Visibility.Collapsed;
                        List<string> list = new List<string>();
                        foreach (IntentionQuoteItem item in this.WorkingDataGrid.SelectedItems)
                        {
                            if (!list.Contains(item.Status))
                            {
                                list.Add(item.Status);
                            }
                            if (item.Type != "INTENTION_GROUP")
                            {
                                CloneMenu.Visibility = Visibility.Visible;
                            }
                        }

                        if (list.Count > 1)
                        {
                            Inquiry_Menu.Visibility = Visibility.Visible;// todo 批量询价
                            processBtn.Visibility = Visibility.Visible;
                            refBtn.Visibility = Visibility.Visible;
                            finishBtn.Visibility = Visibility.Visible;
                            Agree_Menu.Visibility = Visibility.Collapsed;
                            Disagree_Menu.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            string status = list.Count > 0 ? list[0] : string.Empty;

                            if (status == "80")//待处理
                            {
                                if (vm_buy.SelectedItems_Working.Count == 1)
                                {
                                    Inquiry_Menu.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    Inquiry_Menu.Visibility = Visibility.Visible;// todo 批量询价
                                }

                                processBtn.Visibility = Visibility.Visible;
                                refBtn.Visibility = Visibility.Visible;
                                finishBtn.Visibility = Visibility.Collapsed;
                                Agree_Menu.Visibility = Visibility.Collapsed;
                                Disagree_Menu.Visibility = Visibility.Collapsed;
                            }
                            else if (status == "81")//处理中
                            {
                                if (vm_buy.SelectedItems_Working.Count == 1)
                                {
                                    if (vm_buy.SelectedItem_Working.MatchStatus == "5")//未匹配
                                    {
                                        Inquiry_Menu.Visibility = Visibility.Visible;
                                        processBtn.Visibility = Visibility.Collapsed;
                                        refBtn.Visibility = Visibility.Visible;
                                        finishBtn.Visibility = Visibility.Visible;
                                        Agree_Menu.Visibility = Visibility.Collapsed;
                                        Disagree_Menu.Visibility = Visibility.Collapsed;
                                    }
                                    else
                                    {
                                        Inquiry_Menu.Visibility = Visibility.Visible;
                                        processBtn.Visibility = Visibility.Collapsed;
                                        refBtn.Visibility = Visibility.Collapsed;
                                        finishBtn.Visibility = Visibility.Visible;
                                        Agree_Menu.Visibility = Visibility.Collapsed;
                                        Disagree_Menu.Visibility = Visibility.Collapsed;
                                    }
                                }
                                else
                                {
                                    Inquiry_Menu.Visibility = Visibility.Visible;// todo 批量询价
                                    processBtn.Visibility = Visibility.Collapsed;
                                    refBtn.Visibility = Visibility.Visible;
                                    finishBtn.Visibility = Visibility.Visible;
                                    Agree_Menu.Visibility = Visibility.Collapsed;
                                    Disagree_Menu.Visibility = Visibility.Collapsed;

                                }
                            }
                            else if (status == "82")//撤销中
                            {
                                Inquiry_Menu.Visibility = Visibility.Collapsed;
                                processBtn.Visibility = Visibility.Collapsed;
                                refBtn.Visibility = Visibility.Collapsed;
                                finishBtn.Visibility = Visibility.Collapsed;
                                Agree_Menu.Visibility = Visibility.Visible;
                                Disagree_Menu.Visibility = Visibility.Visible;
                            }
                            else if (status == "83")//已撤销
                            {
                                Inquiry_Menu.Visibility = Visibility.Collapsed;
                                processBtn.Visibility = Visibility.Collapsed;
                                refBtn.Visibility = Visibility.Collapsed;
                                finishBtn.Visibility = Visibility.Collapsed;
                                Agree_Menu.Visibility = Visibility.Collapsed;
                                Disagree_Menu.Visibility = Visibility.Collapsed;
                            }
                            else if (status == "84")// 已完成
                            {
                                Inquiry_Menu.Visibility = Visibility.Collapsed;
                                processBtn.Visibility = Visibility.Collapsed;
                                refBtn.Visibility = Visibility.Collapsed;
                                finishBtn.Visibility = Visibility.Collapsed;
                                Agree_Menu.Visibility = Visibility.Collapsed;
                                Disagree_Menu.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                Inquiry_Menu.Visibility = Visibility.Collapsed;
                                processBtn.Visibility = Visibility.Collapsed;
                                refBtn.Visibility = Visibility.Collapsed;
                                finishBtn.Visibility = Visibility.Collapsed;
                                Agree_Menu.Visibility = Visibility.Collapsed;
                                Disagree_Menu.Visibility = Visibility.Collapsed;
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// 操作，询价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MarketBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null && vm_buy.SelectedItem_Working != null)
            {
                Button btn = sender as Button;
                IntentionQuoteItem currentItem = btn.DataContext as IntentionQuoteItem;
                if (currentItem != null)
                {
                    vm_buy.Sub_OnMarketCommand(currentItem);
                }
            }
            else
            {
                CustomMessageBox.Info($"请先选择一条个需", "询价");
            }

            WriteDBLogHelper.Instance.GenRecord("personal", "030", "询价");
        }

        /// <summary>
        /// 操作，取消询价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelMarketBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null && vm_buy.SelectedItem_Working != null)
            {
                vm_buy.OnRefAllInquiry();
            }
            else
            {
                CustomMessageBox.Info($"请先选择一条个需", "撤销询价");
            }


            WriteDBLogHelper.Instance.GenRecord("personal", "031", "取消询价");
        }
        private void FinishBtn_Single_Click(object sender, RoutedEventArgs e)
        {
            WriteDBLogHelper.Instance.GenRecord("personal", "010", "完成");

            if (vm_buy != null)
            {
                ModifyPersonalRemarksView x = new ModifyPersonalRemarksView();
                if (x.ShowDialog() == true)
                {
                    List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                    modifyList.Add((sender as Button).DataContext as IntentionQuoteItem);
                    vm_buy.EditPersonalMarks(modifyList, null, x.memo.Text, true);
                }
            }
        }

        private void BestCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.WorkingDataGrid.SelectedItems == null || this.WorkingDataGrid.SelectedItems.Count == 0)
            {
                return;
            }

            this.WorkingDataGrid.CopyData_Single(false);

            FQSendWindow fqSendWindow = new FQSendWindow(true);
            fqSendWindow.Owner = Application.Current.MainWindow;
            fqSendWindow.Show();

            WriteDBLogHelper.Instance.GenRecord("personal", "003", "批量-复制");
        }

        private void CopyMenu_Click(object sender, RoutedEventArgs e)
        {
            this.WorkingDataGrid.CopyData_Single(false);
        }

        private void CopyMenu_SecurityID_Click(object sender, RoutedEventArgs e)
        {
            this.WorkingDataGrid.CopyData_SecurityID(false);
        }

        private void CopyMenu_Symbol_Click(object sender, RoutedEventArgs e)
        {
            this.WorkingDataGrid.CopyData_Symbol(false);
        }

        private void CopyMenu_Price_Click(object sender, RoutedEventArgs e)
        {
            this.WorkingDataGrid.CopyData_Single_Price(false);
        }

        private void SearchMenu_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy.SelectedItem_Working != null)
            {
                vm_buy.FilterText = vm_buy.SelectedItem_Working.SecurityId;
            }
        }


        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is IntentionControl_Buy_ViewModel)
            {
                vm_buy = e.NewValue as IntentionControl_Buy_ViewModel;

                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    if (CommonUtils.IsInvestor())
                    {
                        //StcTbookUsers stcTbookUsers = CommonUtils.GetInvestors().FirstOrDefault(a => a.Username == LocalEnv.UserInfo.UserName);
                        //if (stcTbookUsers != null)
                        //{
                        //    vm_buy.SelectInvestorItem = stcTbookUsers;
                        //}
                        //cbInvestor.IsEnabled = false;
                        //vm_buy.SelectInvestorText = LocalEnv.UserInfo.FullName;
                    }
                    else
                    {
                        StcTbookUsers stcTbookUsers = CommonUtils.GetTraders().FirstOrDefault(a => a.Username == LocalEnv.UserInfo.UserName);
                        if (stcTbookUsers != null)
                        {
                            vm_buy.SelectTraderItem = stcTbookUsers;
                        }
                        //cbTrader.IsEnabled = false;
                        vm_buy.SelectTraderText = LocalEnv.UserInfo.FullName;
                        cbInvestor.Visibility = Visibility.Visible;
                        UpdateBtn.Visibility = Visibility.Visible;
                        sp1.Visibility = Visibility.Visible;
                    }
                }
            }
        }


        //批量，

        private void PriceBtn_Click(object sender, RoutedEventArgs e)
        {
            Button priceBtn = sender as Button;
            if (vm_buy != null && priceBtn != null && priceBtn.CommandParameter != null)
            {
                CacheSelectedItems_PA();

                vm_buy.EditPrice(priceBtn.CommandParameter.ToString());
            }
        }

        private void QtyBtn_Click(object sender, RoutedEventArgs e)
        {
            Button qtyBtn = sender as Button;
            if (vm_buy != null && qtyBtn != null && qtyBtn.CommandParameter != null)
            {
                CacheSelectedItems_PA();

                vm_buy.EditQty(qtyBtn.CommandParameter.ToString());
            }
        }

        private void InsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AutoCompleteComboBox instNameCb = sender as AutoCompleteComboBox;
            if (string.IsNullOrEmpty(instNameCb.Text))
            {
                instNameCb.ExpandOptions();
            }
        }

        private void TrdNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AutoCompleteComboBox trdNameCb = sender as AutoCompleteComboBox;
            if (string.IsNullOrEmpty(trdNameCb.Text))
            {
                trdNameCb.ExpandOptions();
            }
        }

        private void cbInvestor_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            List<StcTbookUsers> investors = CommonUtils.GetInvestors();

            List<string> userList = new List<string>();
            foreach(IntentionQuoteItem item in vm_buy.SelectedItems_Working)
            {
                if (!string.IsNullOrEmpty(item.Trader_Buyer) && !userList.Contains(item.Trader_Buyer))
                {
                    userList.Add(item.Trader_Buyer);
                }
            }

            StcTbookUsers trader = null;
            if (userList != null && userList.Count == 1 && !string.IsNullOrEmpty(userList[0]))
            {
                trader = CommonUtils.GetUsersInfoByUserName(userList[0]);
            }

            if (trader != null && !string.IsNullOrEmpty(trader.Userid) && LocalEnv.ConfInfo.IsI2T)
            {
                List<string> managerList = AdminHandler.Instance.GetManagerList(trader.Userid);
                if (managerList != null)
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

        private void cbTrader_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            List<StcTbookUsers> traders = CommonUtils.GetTraders();

            if (vm_buy.SelectInvestorItem != null && LocalEnv.ConfInfo.IsI2T)
            {
                List<string> traderList = AdminHandler.Instance.GetTraderList(vm_buy.SelectInvestorItem.Userid);
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

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                CacheSelectedItems_PA();

                vm_buy.EditCounterPartySubName();

                if (CommonUtils.IsInvestor())
                {
                    StcTbookUsers stcTbookUsers = CommonUtils.GetInvestors().FirstOrDefault(a => a.Username == LocalEnv.UserInfo.UserName);
                    if (stcTbookUsers != null)
                    {
                        vm_buy.SelectInvestorItem = stcTbookUsers;
                    }
                    //cbInvestor.IsEnabled = false;
                    vm_buy.SelectInvestorText = LocalEnv.UserInfo.FullName;

                    // 清空交易员
                    vm_buy.SelectTraderItem = null;
                    vm_buy.SelectTraderText = null;
                }
                else
                {
                    StcTbookUsers stcTbookUsers = CommonUtils.GetTraders().FirstOrDefault(a => a.Username == LocalEnv.UserInfo.UserName);
                    if (stcTbookUsers != null)
                    {
                        vm_buy.SelectTraderItem = stcTbookUsers;
                    }
                    //cbTrader.IsEnabled = false;
                    vm_buy.SelectTraderText = LocalEnv.UserInfo.FullName;

                    // 清空投资经理
                    vm_buy.SelectInvestorItem = null;
                    vm_buy.SelectInvestorText = null;
                }
            }
        }

        public Component.CustomDefaultDataGrid GetCustomDefaultDataGrid()
        {
            return WorkingDataGrid;
        }

        private void MatchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null && vm_buy.SelectedItem_Working != null)
            {
                //IInputElement element = this.WorkingDataGrid.InputHitTest(Mouse.GetPosition(this.WorkingDataGrid));
                //IntentionQuoteItem currentItem = null;
                //if (element != null && element is DependencyObject)
                //{
                //    DependencyObject obj = element as DependencyObject;
                //    while (obj != null && !(obj is System.Windows.Controls.Button))
                //        obj = VisualTreeHelper.GetParent(obj);

                //    if (obj is Button)
                //    {
                //        currentItem = (obj as Button).DataContext as IntentionQuoteItem;
                //    }
                //}

                MatchExeView matchExecView = new MatchExeView();
                if (matchExecView.DataContext != null && matchExecView.DataContext is MatchExeViewModel)
                {
                    MatchExeViewModel vm = matchExecView.DataContext as MatchExeViewModel;
                    //vm.CurrentItem = (sender as Button).DataContext as IntentionQuoteItem;
                    vm.CurrentItem = vm_buy.SelectedItem_Working;
                    vm.FinishIntentionItemAction = FinishSingle;
                }
                matchExecView.Show();
            }
        }

        /// <summary>
        /// 操作, 处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcessBtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                CacheSelectedItems_PA();

                List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                modifyList.Add((sender as Button).DataContext as IntentionQuoteItem);
                vm_buy.Sub_DoAllProcessCommand(modifyList);
            }
        }

        /// <summary>
        /// 批量，右键： 处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                CacheSelectedItems_PA();

                vm_buy.Sub_DoAllProcessCommand(WorkingDataGrid.SelectedItems);
            }
        }

        /// <summary>
        /// 操作，ref
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefBtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                CacheSelectedItems_PA();

                List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                modifyList.Add((sender as Button).DataContext as IntentionQuoteItem);
                vm_buy.Sub_DoAllIgnoreCommand(modifyList);
            }
            WriteDBLogHelper.Instance.GenRecord("personal", "009", "撤销");
        }

        /// <summary>
        /// 批量，右键 ： ref
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllRefBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                CacheSelectedItems_PA();

                vm_buy.Sub_DoAllIgnoreCommand(WorkingDataGrid.SelectedItems);
            }
        }

        /// <summary>
        /// 操作，完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FinishBtn_Click_1(object sender, RoutedEventArgs e)
        {
            CacheSelectedItems_PA();

            WriteDBLogHelper.Instance.GenRecord("personal", "010", "完成");

            FinishSingle((sender as Button).DataContext as IntentionQuoteItem);
        }

        private void FinishSingle(IntentionQuoteItem item)
        {
            if (vm_buy != null)
            {
                IntentionQuoteItem currentItem = null;
                ObservableCollection<IntentionMarketItem> datas = null;
                if (LocalEnv.ConfInfo.IsInquiry && vm_buy.SelectedItems_Working != null && vm_buy.SelectedItems_Working.Count == 1)
                {
                    if (vm_buy.SelectedItem_Working != null && vm_buy.SelectedItem_Working.Type != "INTENTION_GROUP" && !vm_buy.HasGroupRow(vm_buy.SelectedItem_Working.GroupKey))
                    {
                        string key = vm_buy.SelectedItem_Working.IntentionId;
                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            key = vm_buy.SelectedItem_Working.GroupKey;
                        }

                        if (!string.IsNullOrEmpty(key)
                           && vm_buy._Cache_InquiryQuote_Dic.ContainsKey(key)
                           && vm_buy._Cache_InquiryQuote_Dic[key].Count > 0)
                        {
                            datas = new ObservableCollection<IntentionMarketItem>(vm_buy._Cache_InquiryQuote_Dic[key].Values.Where<IntentionMarketItem>(a => a.Status == "1" && a.SendType != 3 && a.OptionType != "sendIntentQQGroups").OrderByDescending(a => a.UpdateTime));
                            if (datas != null && datas.Count > 0)
                            {
                                currentItem = vm_buy.SelectedItem_Working;
                            }
                        }
                    }
                }

                ModifyPersonalRemarksView x = new ModifyPersonalRemarksView(currentItem, datas);
                if (vm_buy.SelectedItems_Working != null && vm_buy.SelectedItems_Working.Count == 1)
                {
                    x.memo.Text = vm_buy.SelectedItem_Working.PersonalRemarks;
                }
                if (x.ShowDialog() == true)
                {
                    List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                    modifyList.Add(item);
                    vm_buy.EditPersonalMarks(modifyList, x.InquiryNotRefList, x.memo.Text, true);
                }
            }
        }

        /// <summary>
        /// 批量，右键 ： 完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllFinishBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                CacheSelectedItems_PA();

                List<IntentionQuoteItem> modifyList = new List<IntentionQuoteItem>();
                if (vm_buy.SelectedItems_Working != null && vm_buy.SelectedItems_Working.Count > 0)
                {
                    foreach (IntentionQuoteItem item in vm_buy.SelectedItems_Working)
                    {
                        if (item.Status == "81" && item.Type != "INTENTION_GROUP")
                        {
                            modifyList.Add(item);
                        }
                    }
                }

                IntentionQuoteItem currentItem = null;
                ObservableCollection<IntentionMarketItem> datas = null;
                if (LocalEnv.ConfInfo.IsInquiry && vm_buy.SelectedItems_Working != null && vm_buy.SelectedItems_Working.Count == 1)
                {
                    if (vm_buy.SelectedItem_Working != null && vm_buy.SelectedItem_Working.Type != "INTENTION_GROUP" && !vm_buy.HasGroupRow(vm_buy.SelectedItem_Working.GroupKey))
                    {
                        string key = vm_buy.SelectedItem_Working.IntentionId;
                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            key = vm_buy.SelectedItem_Working.GroupKey;
                        }

                        if (!string.IsNullOrEmpty(key)
                           && vm_buy._Cache_InquiryQuote_Dic.ContainsKey(key)
                           && vm_buy._Cache_InquiryQuote_Dic[key].Count > 0)
                        {
                            datas = new ObservableCollection<IntentionMarketItem>(vm_buy._Cache_InquiryQuote_Dic[key].Values.Where<IntentionMarketItem>(a => a.Status == "1" && a.SendType != 3 && a.OptionType != "sendIntentQQGroups").OrderByDescending(a => a.UpdateTime));
                            if (datas != null && datas.Count > 0)
                            {
                                currentItem = vm_buy.SelectedItem_Working;
                            }
                        }
                    }
                }

                if (modifyList.Count > 0)
                {
                    ModifyPersonalRemarksView x = new ModifyPersonalRemarksView(currentItem, datas);
                    if (vm_buy.SelectedItems_Working != null && vm_buy.SelectedItems_Working.Count == 1)
                    {
                        x.memo.Text = vm_buy.SelectedItem_Working.PersonalRemarks;
                    }
                    if (x.ShowDialog() == true)
                    {
                        vm_buy.EditPersonalMarks(modifyList, x.InquiryNotRefList, x.memo.Text, true);
                    }
                }
            }
        }

        private void QueryLogsBtn_Click(object sender, RoutedEventArgs e)
        {
            IntentionQuoteItem currentItem = (sender as Button).DataContext as IntentionQuoteItem;

            if (!WindowManager.Instance.IsExist($"IntentionLogsView_{currentItem.IntentionId}"))
            {
                IntentionLogsView view = new IntentionLogsView(currentItem);
                WindowManager.Instance.OpenOnlyOneWindow($"IntentionLogsView_{currentItem.IntentionId}", view);
            }
        }

        private void CheckBtn_Click(object sender, RoutedEventArgs e)
        {
            IntentionQuoteItem currentItem = (sender as Button).DataContext as IntentionQuoteItem;
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                if (!string.IsNullOrEmpty(currentItem.IntentionId))
                {
                    STC.Common.Model.Table.WorkBrench.IntentionQuoteReplyPo reply = TableOldHandler.Instance.GetBuyerIntentionLogs(currentItem.IntentionId);
                    if (reply != null && reply.IsSuccess())
                    {
                        if (reply.Datas.Count > 0)
                        {
                            List<IntentionQuoteInfo> snapshot;
                            lock (reply.Datas)
                            {
                                snapshot = reply.Datas.OrderByDescending(entry => entry.updateTime).ToList(); // 线程安全拷贝
                            }

                            IntentionQuoteInfo data2 = snapshot[0];
                            // 更新
                            string side2 = ComboSourceUtil.Instance.FindEnum(data2.side, "side");
                            string price2 = data2.price.HasValue ? $" {data2.price?.ToString("0.00##")}%" : string.Empty;
                            string qty2 = data2.qty.HasValue ? $" {data2.qty?.ToString("#")}" : string.Empty;
                            string settle2 = ComboSourceUtil.Instance.FindEnum(data2.settlType, "settletype_myorder");
                            string commandNote2 = string.Empty;
                            if (JsonSplit.IsJson(data2.info5))
                            {
                                NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(data2.info5);
                                if (!string.IsNullOrEmpty(info5tag.COMMAND_NOTES))
                                {
                                    commandNote2 = $" {info5tag.COMMAND_NOTES}";
                                }
                            }
                            string status2 = ComboSourceUtil.Instance.FindEnum(data2.status, "intentionstatus_buyer");
                            string matchStatus_code2 = IntentionOldHandler.Instance.ExtractByKey(data2.subStatus, "MATCH_STATUS");
                            string matchStatus2 = ComboSourceUtil.Instance.FindEnum(matchStatus_code2, "matchstatus");
                            string execStatus_code2 = IntentionOldHandler.Instance.ExtractByKey(data2.subStatus, "EXEC_STATUS");
                            string execStatus2 = ComboSourceUtil.Instance.FindEnum(execStatus_code2, "execstatus");
                            string str2 = $"{data2.securityId} {data2.symbol} {side2}{price2}{qty2} {settle2}{commandNote2} {status2} {matchStatus2} {execStatus2}";

                            string str1 = string.Empty;
                            bool isFirstRow = true;
                            foreach (IntentionQuoteInfo data1 in snapshot)
                            {
                                if (isFirstRow)
                                {
                                    isFirstRow = false;
                                    continue;
                                }
                                // 原始
                                string side1 = ComboSourceUtil.Instance.FindEnum(data1.side, "side");
                                string price1 = data1.price.HasValue ? $" {data1.price?.ToString("0.00##")}%" : string.Empty;
                                string qty1 = data1.qty.HasValue ? $" {data1.qty?.ToString("#")}" : string.Empty;
                                string settle1 = ComboSourceUtil.Instance.FindEnum(data1.settlType, "settletype_myorder");
                                string commandNote1 = string.Empty;
                                if (JsonSplit.IsJson(data1.info5))
                                {
                                    NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(data1.info5);
                                    if (!string.IsNullOrEmpty(info5tag.COMMAND_NOTES))
                                    {
                                        commandNote1 = $" {info5tag.COMMAND_NOTES}";
                                    }
                                }
                                string status1 = ComboSourceUtil.Instance.FindEnum(data1.status, "intentionstatus_buyer");
                                string matchStatus_code1 = IntentionOldHandler.Instance.ExtractByKey(data1.subStatus, "MATCH_STATUS");
                                string matchStatus1 = ComboSourceUtil.Instance.FindEnum(matchStatus_code1, "matchstatus");
                                string execStatus_code1 = IntentionOldHandler.Instance.ExtractByKey(data1.subStatus, "EXEC_STATUS");
                                string execStatus1 = ComboSourceUtil.Instance.FindEnum(execStatus_code1, "execstatus");

                                if (side1 != side2 || price1 != price2 || qty1 != qty2 || settle1 != settle2 || commandNote1 != commandNote2
                                || status1 != status2 || matchStatus1 != matchStatus2 || execStatus1 != execStatus2)
                                {
                                    str1 = $"{data1.securityId} {data1.symbol} {side1}{price1}{qty1} {settle1}{commandNote1} {status1} {matchStatus1} {execStatus1}";
                                    break;
                                }
                            }

                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                //string str1 = "220220.IB 22国开20 ofr 2.7688% 10000 1000 T+1";
                                //string str2 = "1 220220.IB 22国开20 ofr 2.7555% 10000 T+1 急售!!!!!! 10000";
                                TextDiffView textDiffView = new TextDiffView(str1, str2, currentItem);
                                textDiffView.Show();
                            }));
                        }
                    }
                }
            });

            
        }

        //private void InvestorListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.RightButton == MouseButtonState.Pressed)
        //        e.Handled = true;
        //}

        //private void InvestorListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    ListBoxItem item = sender as ListBoxItem;
        //    if (item.IsSelected)
        //    {
        //        var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
        //        if (hit != null && (hit.VisualHit is Image || hit.VisualHit is Button))
        //        {
        //            // ignore
        //        }
        //        else
        //        {
        //            item.IsSelected = false;
        //            e.Handled = true;
        //        }
        //    }
        //}

        //private void AddInvestorBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (vm_buy != null)
        //    {
        //        AddInvestorOptionsView view = new AddInvestorOptionsView();
        //        if (view.ShowDialog() == true)
        //        {
        //            vm_buy.AddInvestorFilter(view.SelectedItem);
        //        }
        //    }
        //}

        //private void InvestorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (vm_buy != null)
        //    {
        //        vm_buy.ProcessInvestorFilterData(this.InvestorListBox.SelectedItems);
        //    }
        //}

        //private void AddTraderBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (vm_buy != null)
        //    {
        //        AddInvestorOptionsView view = new AddInvestorOptionsView();
        //        if (view.ShowDialog() == true)
        //        {
        //            vm_buy.AddTraderFilter(view.SelectedItem);
        //        }
        //    }
        //}

        //private void TraderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (vm_buy != null)
        //    {
        //        vm_buy.ProcessTraderFilterData(this.TraderListBox.SelectedItems);
        //    }
        //}

        //private void TraderListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    ListBoxItem item = sender as ListBoxItem;
        //    if (item.IsSelected)
        //    {
        //        var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
        //        if (hit != null && (hit.VisualHit is Image || hit.VisualHit is Button))
        //        {
        //            // ignore
        //        }
        //        else
        //        {
        //            item.IsSelected = false;
        //            e.Handled = true;
        //        }
        //    }
        //}

        private void DelMenu_Click(object sender, RoutedEventArgs e)
        {
        //    if (vm_buy != null)
        //    {
        //        if (CommonUtils.IsInvestor())
        //        {
        //            vm_buy.DelTraderFilter(((sender as Button).DataContext as InvestorFilter).UserName);
        //        }
        //        else
        //        {
        //            vm_buy.DelInvesotrFilter(((sender as Button).DataContext as InvestorFilter).UserName);
        //        }
        //    }
        }

        //private void ShowAll_I_Click(object sender, RoutedEventArgs e)
        //{
        //    InvestorListBox.SelectedItem = null;
        //}

        //private void ShowAll_T_Click(object sender, RoutedEventArgs e)
        //{
        //    TraderListBox.SelectedItem = null;
        //}

        private void WorkingDataGrid_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e)
        {
            if (e.Row.Item is IntentionQuoteItem)
            {
                IntentionQuoteItem row = e.Row.Item as IntentionQuoteItem;
                e.Row.DetailsVisibility = row.DetailIntentionQuoteInfo.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void DataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void WorkingDetailDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            (sender as DataGrid).RaiseEvent(eventArg);
        }

        /// <summary>
        /// 操作， 同意
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgreeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                List<IntentionQuoteItem> list = new List<IntentionQuoteItem>();
                list.Add((sender as Button).DataContext as IntentionQuoteItem);
                 
                vm_buy.DoConfirmRefAll("82", list);
            }
        }

        /// <summary>
        /// 右键， 同意
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllAgreeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                CacheSelectedItems_PA();

                vm_buy.DoConfirmRefAll("82", this.WorkingDataGrid.SelectedItems);
            }
        }

        /// <summary>
        /// 操作 ， 不同意
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisAgreeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                List<IntentionQuoteItem> list = new List<IntentionQuoteItem>();
                list.Add((sender as Button).DataContext as IntentionQuoteItem);

                vm_buy.DoConfirmRefAll("87", list);
            }
        }


        /// <summary>
        /// 右键， 不同意
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllDisAgreeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm_buy != null)
            {
                CacheSelectedItems_PA();

                vm_buy.DoConfirmRefAll("87", this.WorkingDataGrid.SelectedItems);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkingDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
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
                    dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, newDirection));

                    this.vm_buy.ResetCustomSort(newDirection, column.SortMemberPath);
                }
            }
        }

        private void isMeCheck_Checked(object sender, RoutedEventArgs e)
        {
            vm_buy.ProcessFilterIsMe(true);
        }

        private void isMeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            vm_buy.ProcessFilterIsMe(false);
        }

        private void RiskCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            vm_buy.BatchCheckRisk(true);
        }

        private void bp_Edit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (vm_buy != null && vm_buy.SelectedItems_Working != null && vm_buy.SelectedItems_Working.Count > 0)
                {
                    CacheSelectedItems_PA();

                    vm_buy.EditPrice_Bp(this.bp_Edit.NumericValue);
                }
                else
                {
                    CustomMessageBox.Info("请先选择要修改的个需", "点差");
                }
            }
        }

        private void qty_edit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (vm_buy != null && vm_buy.SelectedItems_Working != null && vm_buy.SelectedItems_Working.Count > 0)
                {
                    CacheSelectedItems_PA();

                    vm_buy.EditQty_Custom(this.qty_edit.NumericValue);
                }
                else
                {
                    CustomMessageBox.Info("请先选择要修改的个需", "点差");
                }
            }
        }

        private void Inquiry_Menu_Click(object sender, RoutedEventArgs e)
        {
            BatchInquiry();
        }

        private void WorkingDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (vm_buy != null && e.Row.Item is IntentionQuoteItem)
            {
                IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                if (e.Column.Header.ToString() == "报价")
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

                            if (vm_buy.PriceTypeTableDic.Count == 2)
                            {
                                vm_buy.PriceTypeTableDic.Add(new KeyValuePair<string, string>("7", "行权"));
                            }
                        }
                        else
                        {
                            if (vm_buy.PriceTypeTableDic.Count == 3)
                            {
                                vm_buy.PriceTypeTableDic.RemoveAt(2);
                            }
                        }
                    }
                }
                vm_buy.EditRow(item);
            }
        }

        private void WorkingDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item != null && e.Row.Item is IntentionQuoteItem)
            {
                if (e.Column.Header != null)
                {
                    IntentionQuoteItem item = e.Row.Item as IntentionQuoteItem;
                    if (e.Column.Header.ToString() == "报价" && (item.Price_Edit != item.Price || item.PriceType_Edit != item.PriceType))
                    {
                        vm_buy.EditPrice_Custom(item.Price_Edit, item.PriceType_Edit);
                    }
                    else if (e.Column.Header.ToString() == "报量(万)" && item.Qty_Edit != item.Qty)
                    {
                        vm_buy.EditQty_Custom(item.Qty_Edit);
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

        private void QuantityNU_edit_Loaded(object sender, RoutedEventArgs e)
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

                    //vm_buy.EditPrice_Custom(currentItem.Price_Edit, currentItem.PriceType_Edit, CancelEdit);
                    this.WorkingDataGrid.CommitEdit();
                }
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

                    //vm_buy.EditQty_Custom(currentItem.Qty_Edit, CancelEdit);
                    this.WorkingDataGrid.CommitEdit();
                }
            }
        }

        private void CancelEdit()
        {
            this.Dispatcher.BeginInvoke((Action)(()=>{
                this.WorkingDataGrid.CancelEdit();
            }));
        }

        private void CacheSelectedItems_PA()
        {
            //if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL && vm_buy._isBatchOps == false)
            //{
            //    vm_buy._isBatchOps = true;
            //    vm_buy._cacheSelectedItems = this.WorkingDataGrid.SelectedItems.Cast<IntentionQuoteItem>().ToList();
            //    //Console.WriteLine($"22222222222222222222:{this.vm_buy._cacheSelectedItems.Count}");
            //}
        }

        bool isUserOps = false;
        private void WorkingDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //isUserOps = true;
            //vm_buy._isBatchOps = false;
            //vm_buy._cacheSelectedItems?.Clear();
        }

        private void WorkingDataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //isUserOps = true;
            //vm_buy._isBatchOps = false;
            //vm_buy._cacheSelectedItems?.Clear();
        }

        private void AllInquiryBtn_Click(object sender, RoutedEventArgs e)
        {
            BatchInquiry();
        }

        private void BatchInquiry()
        {
            if (vm_buy != null && vm_buy.SelectedItem_Working != null)
            {
                bool canInquiry = false;
                foreach(IntentionQuoteItem item in vm_buy.SelectedItems_Working)
                {
                    if (item.Status == "80" || item.Status == "81" || item.Status == "82")
                    {
                        canInquiry = true;
                        break;
                    }
                }
                if (canInquiry)
                {
                    PublishQuoteView_PA publishQuoteView_PA = new PublishQuoteView_PA();
                    PublishQuoteViewModel_PA vm_pa = new PublishQuoteViewModel_PA();
                    vm_pa.Init(this.WorkingDataGrid.SelectedItems, publishQuoteView_PA);
                    publishQuoteView_PA.DataContext = vm_pa;
                    publishQuoteView_PA.Show();
                }
            }
            else
            {
                CustomMessageBox.Info($"请先选择一条个需", "询价");
            }

            WriteDBLogHelper.Instance.GenRecord("personal", "030", "询价");
        }

        private void CloneMenu_Click(object sender, RoutedEventArgs e)
        {
            List<IntentionQuoteItem> list = new List<IntentionQuoteItem>();
            if (this.WorkingDataGrid.SelectedItems != null)
            {
                foreach (IntentionQuoteItem item in this.WorkingDataGrid.SelectedItems)
                {
                    if (item.Type != "INTENTION_GROUP")
                    {
                        list.Add(item);
                    }
                }
            }

            if (list.Count == 1)
            {
                if (vm_buy != null && vm_buy.SelectedItem_Working != null && vm_buy.SelectedItem_Working.Type != "INTENTION_GROUP")
                {
                    vm_buy.EditOrder(false);
                }
            }
            else
            {
                Clipboard.Clear();
                List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> saleInfo = GetAllSaleQuoteInfo(list);
                STC.Common.Data.DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(false, saleInfo);
            }
        }

        /// <summary>
        /// 获取待卖数据
        /// </summary>
        /// <param name="selectedData"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> GetAllSaleQuoteInfo(List<IntentionQuoteItem> list)
        {
            List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> textList = new List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>();
            foreach (IntentionQuoteItem data in list)
            {
                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj = new Common.Model.Table.WorkBrench.IntentionQuoteInfo();

                string key = $"{data.Trader_Buyer}_{data.Investor}_{data.SecurityId}_{data.Side}_{data.AccountID}";
                obj.clordId = obj.clordid = key;
                obj.securityId = data.SecurityId;
                obj.symbol = data.Symbol;
                obj.timeToMaturity = data.RemainingTime;
                obj.side = data.Side;
                obj.bookId = data.BookId;
                obj.bookName = data.BookName;
                obj.accountID = data.AccountID;
                obj.accountName = data.AccountName;
                obj.traderId = data.Trader_Buyer;
                obj.traderName = data.TraderName_Buyer;
                obj.price = data.Price;
                obj.priceType = data.PriceType;
                obj.qty = data.Qty;
                obj.settlType = data.SettlType;
                obj.userName = LocalEnv.UserInfo.UserName;
                obj.closeBy = LocalEnv.UserInfo.UserName;
                obj.issuerName = data.IssuerCode;
                obj.cleanPrice = data.CleanPrice;
                obj.chinaBondValuation = data.ChinaBondValuation.ConvertToDecimal();
                obj.chinaBondValuationFull = data.ChinaBondValuationFull;
                obj.csiBondValuation = data.CsiBondValuation.ConvertToDecimal();
                obj.csiBondValuationFull = data.CsiBondValuationFull;
                obj.hsProductId = data.HsProductId;
                obj.partyManager = data.Investor;
                obj.partyManagerName = data.InvestorName;
                obj.relatedUsers = data.RelatedUsers;
                textList.Add(obj);
            }

            return textList;
        }

    }
}
