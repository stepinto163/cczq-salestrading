using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Data;
using STC.InnerBoard.Plugin.ViewModel;
using STC.InnerBoard.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace STC.InnerBoard.Plugin.View
{
    /// <summary>
    /// InnerHistoryRequire.xaml 的交互逻辑
    /// </summary>
    public partial class InnerHistoryRequire : UserControl
    {
        private InnerHistoryRequireViewModel _innerBoardViewModel;

        private string MODULEID_BONDSUBTYPE = "5005";
        private string RATE = "500502";
        private string CREDIT = "500503";

        public InnerHistoryRequire()
        {
            InitializeComponent();


            //HistoryFilterPanel.SetDisplayMode("3");
            _innerBoardViewModel = new InnerHistoryRequireViewModel();
            this.DataContext = _innerBoardViewModel;

            if (AdminHandler.Instance.CheckPermission(MODULEID_BONDSUBTYPE, RATE))
            {
                this.single.Visibility = Visibility.Collapsed;

                _innerBoardViewModel.DisplayMode = "BNC";
            }
            else if (AdminHandler.Instance.CheckPermission(MODULEID_BONDSUBTYPE, CREDIT))
            {
                this.single.Visibility = Visibility.Collapsed;

                _innerBoardViewModel.DisplayMode = "BCO";
            }

            (DataContext as InnerHistoryRequireViewModel).EMSPageControl = this.EMSPageControl;
            (DataContext as InnerHistoryRequireViewModel).Init(this);
        }

        private void PART_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 撤销类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbcancelordertype_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            string key = ccb.Tag?.ToString();
            List<string> _tmpfocusTagDic = new List<string>();

            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    string[] orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);

                    if (orgArr != null && orgArr.Length > 0)
                    {
                        foreach (string org in orgArr)
                        {
                            _tmpfocusTagDic.Add(org);
                        }
                    }
                    else
                    {
                        _tmpfocusTagDic.Add(string.Empty);
                    }
                }
            }

            if (DataContext is InnerHistoryRequireViewModel)
            {
                (DataContext as InnerHistoryRequireViewModel).CancelOrderType = _tmpfocusTagDic;
            }
        }

        #region 保存或设置
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetAddTagGroup()))
            //{
            //    AddGroupWindow addGroupWin = new AddGroupWindow(_innerBoardViewModel.GroupList, this._innerBoardViewModel.FocusTags, _innerBoardViewModel.SelectedItem_Tag?.GroupID);
            //    addGroupWin.AddAction = OnAddAction;
            //    WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetAddTagGroup(), addGroupWin);
            //}
        }

        private void EditGroup_Click(object sender, RoutedEventArgs e)
        {
            //if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetEditTagGroup()))
            //{
            //    EditGroupWindow editGroupWin = new EditGroupWindow(_innerBoardViewModel.GroupList, _innerBoardViewModel.SelectedItem_Tag?.GroupID);
            //    editGroupWin.EditAction = OnEditAction;
            //    WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetEditTagGroup(), editGroupWin);
            //}
        }

        public void OnAddAction(FilterGroupItem filterGroupItem)
        {
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    _innerBoardViewModel.SelectedItem_Tag = _innerBoardViewModel.SelectedItem_Tag_Best = filterGroupItem;
            //}));
        }

        public void OnEditAction()
        {
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    _innerBoardViewModel.FocusTags = _innerBoardViewModel.SelectedItem_Tag?.FocusTags;
            //    _innerBoardViewModel.FocusTags_Best = _innerBoardViewModel.SelectedItem_Tag_Best?.FocusTags;
            //}));
        }

        #endregion

        Thread threadInit;
        DateTime currentTime;
        private void dgDetail_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                e.Row.Header = e.Row.GetIndex() + 1;
            });

            if (threadInit == null)
            {
                threadInit = new Thread(() =>
                {
                    currentTime = DateTime.Now;
                    while ((DateTime.Now - currentTime).TotalMilliseconds <= 500)
                    {
                        currentTime = DateTime.Now;
                        Thread.Sleep(100);
                        break;
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (_innerBoardViewModel != null)
                        {
                            _innerBoardViewModel.SelectOrder = e.Row.Item as InnerOrderData;
                        }
                        SelectedRowChanged(e.Row.Item as InnerOrderData);
                    }));
                });
                threadInit.IsBackground = true;
                threadInit.Start();
            }
        }

        /// <summary>
        /// 多选处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MMListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (InnerOrderData removedItem in e.RemovedItems)
                {
                    removedItem.IsSelected = false;
                    removedItem.IsChecked = false;
                }

                foreach (InnerOrderData addedItem in e.AddedItems)
                {
                    if (addedItem.IsMe)
                    {
                        addedItem.IsSelected = true;
                        addedItem.IsChecked = true;
                        if (!addedItem.NetPrice.HasValue)
                            (DataContext as InnerHistoryRequireViewModel).AddQueue(new NetPriceValuaModel() { CalType = "4", Yield = addedItem.Price, IntentionID = addedItem.IntentionID, SecurityID = addedItem.SecurityID, SettlType = addedItem.SettleType });
                    }
                }

                if (_innerBoardViewModel != null && _innerBoardViewModel.SelectOrder != null)
                {
                    SelectedRowChanged(_innerBoardViewModel.SelectOrder);
                }
                else
                {
                    SelectedRowChanged(null);
                }
            });
        }

        /// <summary>
        /// 双击某行，可以进行单笔恢复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is InnerHistoryRequireViewModel)
            {
                string orderid = (DataContext as InnerHistoryRequireViewModel).SelectOrder.IntentionID;
                if ((DataContext as InnerHistoryRequireViewModel).SelectOrder.IsMe)
                    (DataContext as InnerHistoryRequireViewModel).OnResumeCommand(orderid);
            }
        }

        private void EMSPageControl_PageChanged(object sender, PageChangedEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    (this.DataContext as InnerHistoryRequireViewModel).LoadMaskVisible = Visibility.Visible;
                    (this.DataContext as InnerHistoryRequireViewModel).queryGuid = Guid.NewGuid().ToString();
                    (this.DataContext as InnerHistoryRequireViewModel).EMSIndex = e.Page - 1;
                    (this.DataContext as InnerHistoryRequireViewModel).InnerOrders.Clear();
                    (this.DataContext as InnerHistoryRequireViewModel).QueryHistoryIntentionOrder();
                }));
            }
        }

        /// <summary>
        /// 鼠标右键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMenu_Opened(object sender, RoutedEventArgs e)
        {
            //List<InnerOrderData> tmpsel = new List<InnerOrderData>((this.DataContext as InnerHistoryRequireViewModel).InnerOrders.Where(p => p.IsSelected && p.IsMe));
            //if (tmpsel.Count == 0)
            //{
            //    ResumeMenu.Visibility = Visibility.Collapsed;
            //    BatchResumeMenu.Visibility = Visibility.Collapsed;
            //    dgMenu.IsOpen = false;
            //    e.Handled = true;
            //}

            //if (tmpsel.Count == 1)
            //{
            //    if ((DataContext as InnerHistoryRequireViewModel).SelectOrder.IsMe)
            //        ResumeMenu.Visibility = Visibility.Visible;
            //    else
            //        ResumeMenu.Visibility = Visibility.Collapsed;
            //    BatchResumeMenu.Visibility = Visibility.Collapsed;
            //}
            //else if (tmpsel.Count > 1)
            //{
            //    ResumeMenu.Visibility = Visibility.Collapsed;
            //    BatchResumeMenu.Visibility = Visibility.Visible;
            //}
        }

        private void ResumeMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((this.DataContext as InnerHistoryRequireViewModel).SelectOrder !=null)
                (this.DataContext as InnerHistoryRequireViewModel).OnResumeCommand((this.DataContext as InnerHistoryRequireViewModel).SelectOrder.IntentionID);
        }

        private void Single_Click(object sender, RoutedEventArgs e)
        {
            if (SingleRd_ALL.IsChecked == true)
            {
                this.HistoryFilterPanel.SetDisplayMode("0");
                _innerBoardViewModel.DisplayMode = "";
            }
            else if (SingleRd_R.IsChecked == true)
            {
                this.HistoryFilterPanel.SetDisplayMode("1");
                _innerBoardViewModel.DisplayMode = "BNC";
            }
            else if (SingleRd_C.IsChecked == true)
            {
                this.HistoryFilterPanel.SetDisplayMode("2");
                _innerBoardViewModel.DisplayMode = "BCO";
            }
            this.HistoryFilterPanel.ShowAll();
        }

        private void singleToggleBtn_Best_Click(object sender, RoutedEventArgs e)
        {
            //HistoryFilterPanel
            if (this.singleToggleBt0n.Style == FindResource("CollapsedButton_24"))
            {
                this.HistoryFilterPanel.Visibility = Visibility.Collapsed;
                this.singleToggleBt0n.Style = (Style)FindResource("ExpandButton_24");
            }
            else if (this.singleToggleBt0n.Style == FindResource("ExpandButton_24"))
            {
                this.HistoryFilterPanel.Visibility = Visibility.Visible;
                this.singleToggleBt0n.Style = (Style)FindResource("CollapsedButton_24");
            }
        }

        /// <summary>
        /// 打开模糊需求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFuzzyRequire_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist("InnerFuzzyRequireView"))
            {
                InnerFuzzyRequireView win = new InnerFuzzyRequireView();
                WindowManager.Instance.OpenOnlyOneWindow("InnerFuzzyRequireView", win);
            }
        }

        private void CheckBox_All_Unchecked(object sender, RoutedEventArgs e)
        {
            HistoryRequireDataGrid.UnselectAll();
        }

        private void ProfileListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                e.Handled = true;
        }

        private void ProfileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                foreach (FilterGroupItem item in e.AddedItems)
                {
                    WriteDBLogHelper.Instance.GenRecord("inner", "006", item.GroupName);
                }
            }
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item.IsSelected)
            {
                item.IsSelected = false;
                _innerBoardViewModel.SelectedItem_Tag = null;
                e.Handled = true;
            }
        }

        private Popup tagPopup = new Popup();

        private void BestOfr_Arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = sender as Grid;
            tagPopup.Child = this.FindResource("Float_BestOfr") as UserControl;
            tagPopup.DataContext = (sender as Grid).DataContext;
            tagPopup.Placement = PlacementMode.Left;
            tagPopup.PlacementTarget = g;
            tagPopup.IsOpen = true;
        }

        private void BestOfr_Arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            tagPopup.IsOpen = false;
        }

        private void BestBid_Arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = sender as Grid;
            tagPopup.Child = this.FindResource("Float_BestBid") as UserControl;
            tagPopup.DataContext = (sender as Grid).DataContext;
            tagPopup.Placement = PlacementMode.Right;
            tagPopup.PlacementTarget = g;
            tagPopup.IsOpen = true;
        }

        private void BestBid_Arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            tagPopup.IsOpen = false;
        }

        private void SelectedRowChanged(ViewModel.Data.InnerOrderData obj)
        {
            InnerBoardView innerBoardView = CommonUtils.GetParentObject<InnerBoardView>(this);
            if (innerBoardView != null)
            {
                innerBoardView.MyOrderDefaultView_SelectedRowChanged(obj);
            }
        }

        private void positionloadMask_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (positionloadMask.Visibility == Visibility.Visible)
            {
                if (threadInit != null && !threadInit.IsAlive)
                {
                    threadInit = null;
                }
            }
        }

        private void HistoryRequireDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //重新查询下记录
            _innerBoardViewModel.BeginQuery(false);
            _innerBoardViewModel.QueryHistoryIntentionOrder();
        }

        private void HistoryFilterPanel_ShowAllAction(object sender, RoutedEventArgs e)
        {
            WriteDBLogHelper.Instance.GenRecord("inner", "005", "show all");
        }

        private void InnerHistory_DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();
            if (columnHeader == "代码"
               || columnHeader == "简称")
            {
                if (_innerBoardViewModel != null && _innerBoardViewModel.SelectOrder != null)
                {
                    if (CommonUtils.IsBDBAppName())
                    {
                        CommonUtils.OpenSmallBondWin(_innerBoardViewModel.SelectOrder.SecurityID);
                    }
                }
            }
        }

        private void ToQbMenu_Click(object sender, RoutedEventArgs e)
        {
            if (CommonUtils.IsBDBAppName() && _innerBoardViewModel != null && _innerBoardViewModel.SelectOrder != null)
            {
                CommonUtils.OpenSmallBondWin(_innerBoardViewModel.SelectOrder.SecurityID);
            }
        }

        private void CopyMenu_Click(object sender, RoutedEventArgs e)
        {
            this.HistoryRequireDataGrid.CopyData_Single();
        }

        private void CopyMenu_SecurityID_Click(object sender, RoutedEventArgs e)
        {
            this.HistoryRequireDataGrid.CopyData_SecurityID();
        }

        private void CopyMenu_Symbol_Click(object sender, RoutedEventArgs e)
        {
            this.HistoryRequireDataGrid.CopyData_Symbol();
        }

        private void CopyMenu_Price_Click(object sender, RoutedEventArgs e)
        {
            this.HistoryRequireDataGrid.CopyData_Single_Price();
        }
    }
}
