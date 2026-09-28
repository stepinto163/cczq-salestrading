using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Data;
using STC.IRS.Plugin.ViewModel;
using STC.IRS.Plugin.ViewModel.Data;
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

namespace STC.IRS.Plugin.View
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

        private void dgDetail_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                e.Row.Header = e.Row.GetIndex() + 1;
            });
        }

        /// <summary>
        /// 多选处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MMListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (InnerOrderData removedItem in e.RemovedItems)
            {
                removedItem.IsSelected = false;
            }

            foreach (InnerOrderData addedItem in e.AddedItems)
            {
                addedItem.IsSelected = true;
                if (!addedItem.NetPrice.HasValue)
                    (DataContext as InnerHistoryRequireViewModel).AddQueue(new NetPriceValuaModel() { CalType = "4", Yield = addedItem.Price, IntentionID = addedItem.IntentionID, SecurityID = addedItem.SecurityID, SettlType = addedItem.SettleType });
            }
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

        private void singleToggleBtn_Best_Click(object sender, RoutedEventArgs e)
        {
            //HistoryFilterPanel
            if (this.singleToggleBt0n.Style == FindResource("CollapsedButton_24"))
            {
                this.HistoryFilterPanel.Visibility = ExtendFilterPanel.Visibility = ExtendFilterPanel2.Visibility = Visibility.Collapsed;
                this.singleToggleBt0n.Style = (Style)FindResource("ExpandButton_24");
            }
            else if (this.singleToggleBt0n.Style == FindResource("ExpandButton_24"))
            {
                this.HistoryFilterPanel.Visibility = ExtendFilterPanel.Visibility = ExtendFilterPanel2.Visibility = Visibility.Visible;
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
    }
}
