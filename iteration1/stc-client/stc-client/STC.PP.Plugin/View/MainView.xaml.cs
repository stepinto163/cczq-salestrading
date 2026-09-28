using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Table;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.UI;
using STC.PP.Plugin.Handler;
using STC.PP.Plugin.Model;
using STC.PP.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;


namespace STC.PP.Plugin.View
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : UserControl
    {
        MainViewModel mainViewModel;
    
        public MainView()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            mainViewModel.InitView();
            this.DataContext = mainViewModel;
            (DataContext as MainViewModel).EMSPageControl = this.EMSPageControl;
            (DataContext as MainViewModel).mainView = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //SetExecutionsWidth();
        }

        public void TryDispose()
        {
            (this.DataContext as MainViewModel).TryDispose();
        }

        #region 记忆窗口拆分布局

        private void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            SaveLastExecutionsHeight();
        }

      
        private void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            //未收缩
            if (btnVer.Content.ToString() == ">")
            {
                Col0.Width = new GridLength(1, GridUnitType.Star);
                Col2.Width = new GridLength(0);
                btnVer.Content = "<";
            }
            else
            {
                Col0.Width = new GridLength(1, GridUnitType.Star);
                Col2.Width = new GridLength(300, GridUnitType.Pixel);
                btnVer.Content = ">";
            }

            //SaveLastExecutionsHeight();
            //IniFileHelper.RefreshIni();
        }

        public void SaveLastExecutionsHeight()
        {
            //bool isLite = AdminHandler.Instance.CheckPermission("5007", "500701");
            bool isLite = false;
            if (!isLite)
            {
                IniFileHelper.WriteValue($"PPMainView_{LocalEnv.UserInfo.UserName}", "MainViewWidth", Col0.Width.ToString());
                IniFileHelper.WriteValue($"PPMainView_{LocalEnv.UserInfo.UserName}", "DetailViewWidth", Col2.Width.ToString());
                if (Col2.Width.Value > 0)
                {
                    btnVer.Content = ">";
                }

                IniFileHelper.WriteValue($"PPMainView_{LocalEnv.UserInfo.UserName}", "ExpandButtonCollapsed", btnVer.Content.ToString() == "<" ? "1" : "0");
            }
        }

        #region 分隔符恢复


        private async void SetExecutionsWidth()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    string expandbettoncollapsed = IniFileHelper.ReadValue($"PPMainView_{LocalEnv.UserInfo.UserName}", "ExpandButtonCollapsed");
                    if (!string.IsNullOrEmpty(expandbettoncollapsed))
                    {
                        btnVer.Content = expandbettoncollapsed == "1" ? "<" : ">";
                        if (expandbettoncollapsed == "1")
                        {
                            Col0.Width = new GridLength(1, GridUnitType.Star);
                            Col2.Width = new GridLength(0);
                        }
                        else
                        {
                            string height = IniFileHelper.ReadValue($"PPMainView_{LocalEnv.UserInfo.UserName}", "MainViewWidth");
                            double d = 0;
                            string width = IniFileHelper.ReadValue($"PPMainView_{LocalEnv.UserInfo.UserName}", "DetailViewWidth");

                            if (!string.IsNullOrEmpty(height) && !string.IsNullOrEmpty(width))
                            {
                                if (height.Contains("*"))
                                {
                                    height = height.Replace("*", "");
                                    if (string.IsNullOrEmpty(height))
                                        Col0.Width = new GridLength(1, GridUnitType.Star);
                                    else
                                        Col0.Width = new GridLength(d, GridUnitType.Star);
                                }
                                else
                                {
                                    if (double.TryParse(height, out d))
                                        Col0.Width = new GridLength(d, GridUnitType.Pixel);
                                }

                                if (width.Contains("*"))
                                {
                                    width = width.Replace("*", "");
                                    if (string.IsNullOrEmpty(width))
                                        Col2.Width = new GridLength(1, GridUnitType.Star);
                                    else
                                        Col2.Width = new GridLength(d, GridUnitType.Star);
                                }
                                else
                                {
                                    if (double.TryParse(width, out d))
                                        Col2.Width = new GridLength(d, GridUnitType.Pixel);
                                }
                            }
                            else
                            {
                                Col0.Width = new GridLength(1, GridUnitType.Star);
                                Col2.Width = new GridLength(300, GridUnitType.Pixel);
                            }
                        }
                    }

                }));
            });
        }
        #endregion


        #endregion

        #region UI事件处理

        /// <summary>
        /// 投资经理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FundAccountCB_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;

            if (ccb != null)
            {
                if (string.IsNullOrWhiteSpace(ccb.SelectedValues))
                    ccb.ClearSelection();
            }
        }

        /// <summary>
        /// 交易账户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TradeAccountCB_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;

            if (ccb != null)
            {
                if (string.IsNullOrWhiteSpace(ccb.SelectedValues))
                    ccb.ClearSelection();
            }
        }

        /// <summary>
        /// 清空投资经理
        /// </summary>
        public void ClearFundAccountCB()
        {
            if (FundAccountCB != null)
            {
                FundAccountCB.ClearSelection();
            }
        }

        /// <summary>
        /// 设置组合和交易账户
        /// </summary>
        /// <param name="value"></param>
        public void SetGroupAndTradeAccount(string value)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                cmbMultiTree.SetSelection(value);
            }));
            
        }

        /// <summary>
        /// 清除组合和交易账户    
        /// </summary>
        public void ClearGroupAndTradeAccount()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                cmbMultiTree.ClearSelection();
            }));
        }

        public void ResetMyFilterSelection()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                MyFilter.UnCheckAll();
            }));
        }

        /// <summary>
        /// 债券类型 BCO,BNC,ABS,NCD  转换关系：0-全 1-信用债 BCO 2-利率债 BNC 11-ABS 12-NCD   BizTypeConvert
        /// </summary>
        /// <param name="value"></param>
        public void SetSingleRd(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                switch (value)
                {
                    case "0":
                        SingleRd_ALL.IsChecked = true;
                        mainViewModel.SelectSymbolType = "0";
                        customFilterPanel.SetSimplePanelDisplayMode("0");
                        break;
                    case "1":
                        //信用
                        mainViewModel.SelectSymbolType = "1";
                        customFilterPanel.SetSimplePanelDisplayMode("2");
                        SingleRd_C.IsChecked = true;
                        break;
                    case "2":
                        //利率
                        SingleRd_R.IsChecked = true;
                        mainViewModel.SelectSymbolType = "2";
                        customFilterPanel.SetSimplePanelDisplayMode("1");
                        break;
                    default:
                        break;
                }


            }
            else
            {
                SingleRd_ALL.IsChecked = true;
            }
        }
        #endregion


        #region 主表格事件处理

        private void DgDetail_Sorting(object sender, DataGridSortingEventArgs e)
        {

        }

        //private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        //{

        //}

        private void CheckBox_ButtonDown(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                isClickingCheckBox = false;
                
                var cb = sender as CheckBox;

                var items = cb.DataContext as StcBondPositionData;

                int index = dgPP.Items.IndexOf(items);
                if (index >= 0)
                {
                    dgPP.SelectedIndex = index;
                }
            }
        }

        private bool isClickingCheckBox = false;

        private void MMListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isClickingCheckBox = false;

            var originalSource = e.OriginalSource as DependencyObject;
            while (originalSource != null)
            {
                if (originalSource is CheckBox)
                {
                    isClickingCheckBox = true;
                    break;
                }

                originalSource = VisualTreeHelper.GetParent(originalSource);
            }
        }

        private void MMListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isClickingCheckBox)
                return; // 不处理，避免干扰 CheckBox 行为

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (StcBondPositionData removedItem in e.RemovedItems)
                {
                    removedItem.IsChecked = false;
                }

                foreach (StcBondPositionData addedItem in e.AddedItems)
                {
                    addedItem.IsChecked = true;
                }

                List<StcBondPositionData> drlist = mainViewModel.DsBondPosition.Where(p=>p.IsChecked)?.ToList();
                if (drlist != null && drlist.Count > 0)
                {
                    mainViewModel.QueryPledgeposition(drlist[0]);
                }
                else
                {
                    mainViewModel.QueryPledgeposition(null);
                }
            });
        }

        #endregion

        #region 分页
        Thread threadInit;

        private void PositionloadMask_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (positionloadMask.Visibility == Visibility.Visible)
            {
                if (threadInit != null && !threadInit.IsAlive)
                {
                    threadInit = null;
                }
            }
        }

        private void EMSPageControl_PageChanged(object sender, Common.View.Component.PageChangedEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    (this.DataContext as MainViewModel).IsLoading = true;
                    (this.DataContext as MainViewModel).EMSIndex = e.Page - 1;
                    (this.DataContext as MainViewModel).NotifyFilter(false);
                }));
            }
        }



        #endregion

        #region 质押券右键菜单处理
        private void dgMenu_Reference_Opened(object sender, RoutedEventArgs e)
        {

        }

        private void CopyMenu_SecurityID_Reference_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                (this.DataContext as MainViewModel).OnCopyPledgeposition();
                dgDetailPosition.UnselectAll();
            }));
        }

        private void dgDetailPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (StcPledgePositionModelData removedItem in e.RemovedItems)
                {
                    removedItem.IsChecked = false;
                }

                foreach (StcPledgePositionModelData addedItem in e.AddedItems)
                {
                    addedItem.IsChecked = true;
                }
            });
        }

        #endregion

        #region 债券过滤面板操作
        //选择过滤栏“SHOW ALL”按钮事件
        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.Info($"Querying with: SHOW ALL Click!");
            (this.DataContext as MainViewModel).IsLoading = true;
            (this.DataContext as MainViewModel).ResetFilterPanel();
        }

        //选择债券过滤面板的“显示全部”按钮事件
        private void customFilterPanel_ShowAllAction(object sender, RoutedEventArgs e)
        {

        }

        
        private void savebtn_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetAddTagGroup()))
            {
                AddSubscriptionGroupWindow addGroupWin = new AddSubscriptionGroupWindow(
                    IntentionOldHandler.Instance.PPGroupList, this.mainViewModel.FocusTags,
                    this.mainViewModel.SelectedItem_Tag?.GroupID, this.mainViewModel.StartTermDY,
                    this.mainViewModel.EndTermDY, this.mainViewModel.IsAppTerm,
                    this.mainViewModel.StartCoupon, this.mainViewModel.EndCoupon, this.mainViewModel.IsCoupon,
                    this.mainViewModel.StartPrice, this.mainViewModel.EndPrice,
                    this.mainViewModel.StartChinaBondValue, this.mainViewModel.EndChinaBondValue,
                    this.mainViewModel.StartBidBp, this.mainViewModel.EndBidBp,
                    this.mainViewModel.StartOfrBp, this.mainViewModel.EndOfrBp, this.mainViewModel.TopFilterSelectItem);
                addGroupWin.AddAction = OnAddAction;
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GetAddTagGroup(), addGroupWin);
            }
        }

        public void OnAddAction(FilterGroupItem filterGroupItem)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                mainViewModel.SelectedItem_Tag = filterGroupItem;
            }));
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
                        EditGroupWindow editGroupWin = new EditGroupWindow(IntentionOldHandler.Instance.PPGroupList, fgi?.GroupID);
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
                mainViewModel.FocusTags = mainViewModel.SelectedItem_Tag?.FocusTags;
                mainViewModel.StartPrice = mainViewModel.SelectedItem_Tag?.StartPrice;
                mainViewModel.EndPrice = mainViewModel.SelectedItem_Tag?.EndPrice;
                mainViewModel.StartTermDY = mainViewModel.SelectedItem_Tag?.StartTermDY;
                mainViewModel.EndTermDY = mainViewModel.SelectedItem_Tag?.EndTermDY;
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
                        FilterGroupItem removeItem = IntentionOldHandler.Instance.PPGroupList.First(item => item.GroupID == groupid);
                        IntentionOldHandler.Instance.PPGroupList.Remove(removeItem);
                    }
                    catch (System.InvalidOperationException)
                    {
                        // ignore
                    }
                }
            }
        }

        #endregion

        #region 全，信用，利率筛选
        private void Single_Click(object sender, RoutedEventArgs e)
        {
            //债券类型 BCO,BNC,ABS,NCD  转换关系：0-全 1-信用债 BCO 2-利率债 BNC 11-ABS 12-NCD   BizTypeConvert
            if (SingleRd_ALL.IsChecked == true)
            {
                mainViewModel.SelectSymbolType = "0";
                this.customFilterPanel.SetSimplePanelDisplayMode("0");
                //_innerBoardViewModel.DisplayMode = "";
            }
            else if (SingleRd_R.IsChecked == true)
            {
                //利率债
                mainViewModel.SelectSymbolType = "2";
                this.customFilterPanel.SetSimplePanelDisplayMode("1");
                //_innerBoardViewModel.DisplayMode = "BNC";
            }
            else if (SingleRd_C.IsChecked == true)
            {
                //信用债
                mainViewModel.SelectSymbolType = "1";
                this.customFilterPanel.SetSimplePanelDisplayMode("2");
                //_innerBoardViewModel.DisplayMode = "BCO";
            }
        }
        #endregion

        #region 持仓表头设置
        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (dgPP != null)
            {
                try
                {
                    DataGridConfigManager.Instance.ApplyConfig(dgPP, "DgPP");
                }
                catch { }
                
            }
        }

        private void HeaderSettings_Click(object sender, RoutedEventArgs e)
        {
            PPColumnsSetView ui = new PPColumnsSetView();
            BorderWin copyUIBoardWin = new BorderWin();
            ui.SetOwnerWin(copyUIBoardWin);

            MsgAlertDataModel dgc = DataGridConfigManager.Instance.GetDefaultColumn(dgPP);
            ui.SetSetGridDatas(dgc);

            copyUIBoardWin.SetTitleAndKey("PPColumnsSetWin", "表头设置", "PPColumnsSetting");
            copyUIBoardWin.SetMainControl(ui);
            copyUIBoardWin.Width = copyUIBoardWin.MaxWidth = 608;
            copyUIBoardWin.Height = copyUIBoardWin.MinHeight = 360;
            copyUIBoardWin.ResizeMode = ResizeMode.NoResize;
            copyUIBoardWin.SetMinVisible(false);
            copyUIBoardWin.SetMaxVisible(false);
            copyUIBoardWin.RemoveWindowsMouseDoubleClickEvent();
            copyUIBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            copyUIBoardWin.ShowDialog();
            DataGridConfigManager.Instance.ApplyConfig(dgPP, "DgPP");
        }

        private void dgPP_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 检查鼠标是否在表头区域
            var element = e.OriginalSource as FrameworkElement;
            var header = FindVisualParent<DataGridColumnHeader>(element);

            if (header != null)
            {
                // 在表头区域，显示菜单
                ContextMenu menu = this.FindResource("HeaderContextMenu") as ContextMenu;
                menu.PlacementTarget = header;
                menu.IsOpen = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// 利用VisualTreeHelper寻找指定依赖对象的父级对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T)
                    return parent as T;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
        #endregion

        private void MyFilter_DeleteRequested(object sender, FilterGroupItem e)
        {
            //MessageBox.Show("删除项：" + e);
            FilterGroupItem fgi = e as FilterGroupItem;
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GetEditTagGroup()))
            {
                DelOne(fgi?.GroupID);
            }
        }

        /// <summary>
        /// 字段拖动排序后保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgPP_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            MsgAlertDataModel dgc = DataGridConfigManager.Instance.GetDefaultColumn(dgPP);
            DataGridConfigManager.Instance.SaveAllConfigs(dgc);
        }
    }
}
