using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction.DesignModel;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.ViewModel.Setting;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Common.View.Component;
using STC.Frame.UI;
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

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// InstSettingWin.xaml 的交互逻辑
    /// </summary>
    public partial class InstSettingWin : WindowBase
    {
        private InstSettingWinViewModel viewModel;
        private bool textChangeFlag = false;

        private bool isMouseDownFlag = false;

        public InstSettingWin()
        {
            InitializeComponent();
            viewModel = new InstSettingWinViewModel(this);
            DataContext = viewModel;
            GroupList.SelectedIndex = 0;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void tbInst_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;

                dgInstResult_MouseLeftButtonUp(null, null);
            }
        }

        private void tbInst_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgInstResult.SelectedIndex > 0)
                    dgInstResult.SelectedIndex--;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgInstResult.SelectedIndex < dgInstResult.Items.Count - 1)
                    dgInstResult.SelectedIndex++;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
        }

        private void tbInst_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbInst.Text) && textChangeFlag)
            {
                string param = tbInst.Text.Replace("'", "");

                dgInstResult.DataContext = CommonUtils.GetInstitutionsInfoPo(param, 20);
                dgInstResult.SelectedIndex = 0;
                tbInstPop.IsOpen = true;
            }
            else
            {
                tbInstPop.IsOpen = false;
            }
        }


        private void tbInst_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbInst.Text))
            {
                tbInstPop.IsOpen = false;
            }
        }

        private void tbInstPop_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbInst.Text))
            {
                tbInstPop.IsOpen = false;
            }

        }

        private void dgInstResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }


        /// <summary>
        /// 数据定位
        /// </summary>
        /// <param name="dataPo"></param>
        private void GoToOrderBySeq(CrmInst dataPo)
        {
            if (viewModel != null
                && viewModel.IssueGroupViewModels.Count > 0
                && viewModel.IssueInstViewModels.Count > 0)
            {
                //定位分组
                int indexGroup = GroupList.Items.IndexOf(viewModel.IssueGroupViewModels.Where(_ => _.GroupId == viewModel.CurrentGroupId).FirstOrDefault());
                if (indexGroup >= 0)
                {
                    GroupList.SelectedIndex = indexGroup;
                    GroupList.ScrollIntoView(viewModel.IssueGroupViewModels.Where(_ => _.GroupId == viewModel.CurrentGroupId).FirstOrDefault());
                }
                //定位单子
                int index = InstDataGrid.Items.IndexOf(viewModel.IssueInstViewModels.Where(_ => _.InstId == dataPo.idbkey).FirstOrDefault());
                if (index >= 0)
                {
                    InstDataGrid.SelectedIndex = index;
                    InstDataGrid.ScrollIntoView(viewModel.IssueInstViewModels.Where(_ => _.InstId == dataPo.idbkey).FirstOrDefault());
                }
                else
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "该机构尚未添加分组！";
                    warn.ShowDialog();
                }
            }
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            AddGroupWin addGroupWin = new AddGroupWin(viewModel);
            addGroupWin.ShowDialog();

            //viewModel.AddGroup();
        }

        private void lsBridge_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void ListBoxItem_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {

        }


        private void tbInstPInput(object sender, TextCompositionEventArgs e)
        {
            textChangeFlag = true;
        }

        private void tbInstPInputUpdate(object sender, TextCompositionEventArgs e)
        {
            textChangeFlag = false;
        }

        private async void btnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button button = sender as Button;
                if (!string.IsNullOrEmpty(button.Tag?.ToString()))
                {
                    await viewModel.DeleteGroup(button.Tag.ToString());
                    SetControlStatus();
                }
            }
        }

        private void BridgeOrderData_Selected(object sender, MouseButtonEventArgs e)
        {

        }

        private void BridgeOrderData_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }


        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Utils.wpf.DragDrop.CanDrop = true;
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Utils.wpf.DragDrop.CanDrop = false;
        }

        private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!isMouseDownFlag)
                {
                    return;
                }
                if (GroupList.SelectedItem is IssueGroupViewModel)
                {
                    IssueGroupViewModel group = GroupList.SelectedItem as IssueGroupViewModel;
                    AddInstWin_DoRefreshInst(group.GroupId);
                }
                viewModel.IsAllSelect = false;
                //viewModel.IsAllRequestSelect = false;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                isMouseDownFlag = false;
            }
        }

        /// <summary>
        /// 添加机构
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {

            if (GroupList.SelectedItem is IssueGroupViewModel)
            {
                IssueGroupViewModel group = GroupList.SelectedItem as IssueGroupViewModel;
                AddInstWin addInstWin = new AddInstWin(group.GroupId);
                addInstWin.DoRefreshInst += AddInstWin_DoRefreshInst;
                addInstWin.ShowDialog();
            }
        }

        private async void AddInstWin_DoRefreshInst(string groupId, List<OperateIssueInstReply> result = null)
        {
            
            if (result != null)
            {
                foreach (OperateIssueInstReply reply in result)
                {
                    if (reply.IsSuccess())
                    {
                        viewModel.AddInst(groupId, reply.datas);
                    }
                }
            }
            else
            {
                await viewModel.GetData(QueryType.InstSearch, groupId, null);
            }

            viewModel.SetInstData(groupId);
            SetControlStatus();
        }

        private async void AddInstWin_DoRefreshInstOfGrop(string instId, CrmInst dv)
        {
            await viewModel.GetData(QueryType.InstOfGroup, null, instId);
            if (!string.IsNullOrWhiteSpace(viewModel.CurrentGroupId))
            {
                viewModel.SetInstData(viewModel.CurrentGroupId);
                GoToOrderBySeq(dv);
            }
            else
            {
                SysWarnGUI warn = new SysWarnGUI();
                Window win = Window.GetWindow(this);
                if (win != null)
                    warn.Owner = win;
                warn.tbTitle.Text = "提示";
                warn.tbMsg.Text = "该机构尚未添加分组！";
                warn.ShowDialog();
            }

        }

        /// <summary>
        /// 修改机构
        /// </summary>
        /// <param name="newGroupName"></param>
        private async void TextBoxBlockNoLimite_TextChanged(string newGroupName)
        {
            if (GroupList.SelectedItem is IssueGroupViewModel)
            {
                IssueGroupViewModel group = GroupList.SelectedItem as IssueGroupViewModel;
                await viewModel.UpdateGroupInfo(group.GroupId, newGroupName);
            }
        }

        /// <summary>
        /// 删除机构
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteInst(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingUI.Visibility = Visibility.Visible;
                if (GroupList.SelectedItem is IssueGroupViewModel)
                {
                    IssueGroupViewModel group = GroupList.SelectedItem as IssueGroupViewModel;
                    await viewModel.DeleteInsts(group.GroupId);
                    SetControlStatus();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
            finally
            {
                LoadingUI.Visibility = Visibility.Collapsed;
            }

        }

        /// <summary>
        /// 打开交易员管理界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManagerInstTrader(object sender, MouseButtonEventArgs e)
        {
            if (InstDataGrid.SelectedItem is IssueInstViewModel)
            {
                IssueInstViewModel instVm = InstDataGrid.SelectedItem as IssueInstViewModel;
                TraderMangerWin traderMangerWin = new TraderMangerWin(instVm.Id, instVm.InstId);
                traderMangerWin.ShowDialog();
            }
        }

        /// <summary>
        /// 更新默认请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateGroupDefaultRequst(object sender, RoutedEventArgs e)
        {
            if (GroupList.SelectedItem is IssueGroupViewModel)
            {
                IssueGroupViewModel group = GroupList.SelectedItem as IssueGroupViewModel;
                await viewModel.UpdateGroupInfo(group.GroupId, group.GroupName);
            }
        }

        /// <summary>
        /// 选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel != null && viewModel.IssueInstViewModels != null
                && viewModel.IssueInstViewModels.Count > 0)
            {
                bool isAllSelect = true;
                foreach (IssueInstViewModel item in viewModel.IssueInstViewModels)
                {
                    if (!item.IsSelect)
                    {
                        viewModel.IsAllSelect = false;
                        isAllSelect = false;
                        break;
                    }
                }
                viewModel.IsAllSelect = isAllSelect;
            }
            SetControlStatus();
        }

        /// <summary>
        /// 选中或取消选中所有机构
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_all_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                CheckBox checkBox = sender as CheckBox;
                viewModel.CheckAllInst(checkBox.IsChecked.Value);
                SetControlStatus();
            }
        }

        /// <summary>
        /// 设置控件状态
        /// </summary>
        private void SetControlStatus()
        {
            if (viewModel != null && viewModel.IssueInstViewModels != null
               && viewModel.IssueInstViewModels.Count > 0)
            {
                List<IssueInstViewModel> issueInstViews = new List<IssueInstViewModel>();
                foreach (IssueInstViewModel item in viewModel.IssueInstViewModels)
                {
                    if (item.IsSelect)
                    {
                        issueInstViews.Add(item);
                    }
                }

                if (issueInstViews.Count > 0)
                {
                    this.delete.IsEnabled = true;
                }
                else
                {
                    this.delete.IsEnabled = false;
                }
            }
            else
            {
                this.delete.IsEnabled = false;
            }
        }

        private void GroupList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDownFlag = true;
        }

        private void dgInstResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dv = dgInstResult.SelectedItem as CrmInst;
            if (dv != null && !string.IsNullOrWhiteSpace(this.tbInst.Text))
            {
                tbInst.Text = dv.shortnamecn;
                tbInstPop.IsOpen = false;

                // 选中分组
                // 选中机构 查询机构所在分组
                AddInstWin_DoRefreshInstOfGrop(dv.idbkey, dv);
            }
        }
        /// <summary>
        /// 全选中取消默认请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void cb_allRes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingUI.Visibility = Visibility.Visible;
                CheckBox checkBox = sender as CheckBox;
                if (checkBox == null)
                {
                    return;
                }
                if (viewModel != null && viewModel.IssueInstViewModels != null
                && viewModel.IssueInstViewModels.Count > 0)
                {
                    List<long> insts = new List<long>();
                    foreach (IssueInstViewModel item in viewModel.IssueInstViewModels)
                    {
                        item.IsDefualtRequestSelect = checkBox.IsChecked.Value;
                        if (!insts.Contains(item.Id))
                        {
                            insts.Add(item.Id);
                        }
                    }
                    if (GroupList.SelectedItem is IssueGroupViewModel)
                    {
                        IssueGroupViewModel group = GroupList.SelectedItem as IssueGroupViewModel;
                        await viewModel.UpdateInsts(group.GroupId, insts, checkBox.IsChecked.Value);
                    }
                }
                SetControlStatus();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
            finally
            {
                LoadingUI.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 单条选中取消默认请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void cb_dres_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingUI.Visibility = Visibility.Visible;
                CheckBox checkBox = sender as CheckBox;
                if (checkBox == null)
                {
                    return;
                }
                IssueInstViewModel issueInstViewModel = checkBox.DataContext as IssueInstViewModel;
                if (GroupList.SelectedItem is IssueGroupViewModel)
                {
                    IssueGroupViewModel group = GroupList.SelectedItem as IssueGroupViewModel;
                    List<long> insts = new List<long>();
                    if (issueInstViewModel != null)
                    {
                        if (!insts.Contains(issueInstViewModel.Id))
                        {
                            insts.Add(issueInstViewModel.Id);
                        }
                        await viewModel.UpdateInsts(group.GroupId, insts, issueInstViewModel.IsDefualtRequestSelect);
                    }
                }


                if (viewModel != null && viewModel.IssueInstViewModels != null
               && viewModel.IssueInstViewModels.Count > 0)
                {
                    bool isAllSelect = true;
                    foreach (IssueInstViewModel item in viewModel.IssueInstViewModels)
                    {
                        if (!item.IsDefualtRequestSelect)
                        {
                            viewModel.IsAllRequestSelect = false;
                            isAllSelect = false;
                            break;
                        }
                    }
                    viewModel.IsAllRequestSelect = isAllSelect;
                }

                SetControlStatus();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
            finally
            {
                LoadingUI.Visibility = Visibility.Collapsed;
            }
        }
    }
}

