using com.gateway.log;
using STC.Broker.Plugin.ViewModel.Setting;
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
    /// AddInstWin.xaml 的交互逻辑
    /// </summary>
    public partial class AddInstWin : WindowBase
    {
        public event Action<string, List<OperateIssueInstReply>> DoRefreshInst;
        private AddInstWinViewModel viewModel;

        public AddInstWin(string groupId)
        {
            InitializeComponent();
            viewModel = new AddInstWinViewModel(groupId);
            DataContext = viewModel;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        private void tb_Organization_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgRecive.SelectedIndex > 0)
                {
                    dgRecive.SelectedIndex--;
                }
                else
                {
                    if (dgRecive.SelectedIndex == 0)
                    {
                        dgRecive.SelectedIndex = dgRecive.Items.Count - 1;
                    }
                }
                dgRecive.ScrollIntoView(dgRecive.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgRecive.SelectedIndex < dgRecive.Items.Count - 1)
                {
                    dgRecive.SelectedIndex++;
                }
                else
                {
                    if (dgRecive.SelectedIndex == dgRecive.Items.Count - 1)
                    {
                        dgRecive.SelectedIndex = 0;
                    }
                }
                dgRecive.ScrollIntoView(dgRecive.SelectedItem);
            }
        }



        private void tb_Organization_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbInstSearch.Text))
            {
                viewModel.SetInstSearchData(tbInstSearch.Text);
            }
            else
            {
                viewModel.InstInfoViewModels.Clear();
            }
            CheckAll.IsChecked = false;
        }

        /// <summary>
        /// 取消按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 确认按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _loading.Visibility = Visibility.Visible;
                List<OperateIssueInstReply> resultList = await viewModel.DoAddInst();

                bool hasError = resultList.Any(_ => !_.IsSuccess());

                if (hasError)
                {
                    DoRefreshInst?.Invoke(viewModel.GroupId, resultList);


                    StringBuilder errorMsg = new StringBuilder();
                    foreach(OperateIssueInstReply reply in resultList)
                    {
                        if (!reply.IsSuccess())
                        {
                            errorMsg.AppendLine(reply.Msg);
                        }
                    }
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.setWarnMsg(errorMsg.ToString()); ;
                    warn.ShowDialog();
                }
                else
                {
                    DoRefreshInst?.Invoke(viewModel.GroupId, resultList);
                    this.Close();
                }

                //if (string.IsNullOrEmpty(errorMsg))
                //{
                //    DoRefreshInst?.Invoke(viewModel.GroupId);
                //    this.Close();
                //}
                //else
                //{
                //    DoRefreshInst?.Invoke(viewModel.GroupId);
                //    SysWarnGUI warn = new SysWarnGUI();
                //    Window win = GetWindow(this);
                //    if (win != null)
                //        warn.Owner = win;
                //    warn.setWarnMsg(errorMsg);
                //    warn.ShowDialog();
                //    //if (response.Code == "8009") ???
                //    //{
                //    //    this.Close();
                //    //}
                //}

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
            finally
            {
                _loading.Visibility = Visibility.Collapsed;
            }
        }


        /// <summary>
        /// 关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 选中所有或者取消选中所有
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CheckAllInst(CheckAll.IsChecked.Value);
            if (viewModel.InstInfoViewModels.Count > 0)
            {
                var instIds = new List<string>();
                foreach (var item in viewModel.InstInfoViewModels)
                {
                    if (item.Selected)
                    {
                        instIds.Add(item.InstId);
                    }
                }
                if (instIds.Count > 0)
                {
                    btnCancel.IsEnabled = true;
                }
                else
                {
                    btnCancel.IsEnabled = false;
                }
            }
            else
            {
                btnCancel.IsEnabled = false;
            }
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            if (viewModel.InstInfoViewModels.Count > 0)
            {
                var instIds = new List<string>();
                foreach (var item in viewModel.InstInfoViewModels)
                {
                    if (item.Selected)
                    {
                        instIds.Add(item.InstId);
                    }
                }
                if (instIds.Count > 0)
                {
                    btnCancel.IsEnabled = true;
                }
                else
                {
                    btnCancel.IsEnabled = false;
                }
                if (viewModel.InstInfoViewModels.Count > 0 && viewModel.InstInfoViewModels.Count != instIds.Count)
                {
                    CheckAll.IsChecked = false;
                }
                else
                {
                    CheckAll.IsChecked = true;
                }

            }
            else
            {
                btnCancel.IsEnabled = false;
                CheckAll.IsChecked = false;
            }
        }
    }
}

