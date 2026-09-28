
using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction.DesignModel;
using STC.Broker.Plugin.ViewModel.Transaction;
using STC.Broker.Plugin.ViewModel.Transaction.BridgeDesign;
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
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    /// <summary>
    /// BlackListWin.xaml 的交互逻辑
    /// </summary>
    public partial class BlackListWin : Window
    {

        private BlackListViewModel viewModel;
        public BridgeConfigInfoViewModel BridgeConfigInfo { get; set; }

        public BlackListWin(BridgeConfigInfoViewModel bridgeConfigInfo)
        {
            InitializeComponent();
            viewModel = new BlackListViewModel(bridgeConfigInfo);
            DataContext = viewModel;
            BridgeConfigInfo = bridgeConfigInfo;
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

        private void tb_Organization_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                dgRecive_MouseUp(null, null);
            }
        }

        private void tb_Organization_TextChanged(object sender, TextChangedEventArgs e)
        {
            string organizationKey = TB_Organization.Text.Trim();
            organizationKey = organizationKey.Replace("'", "").Replace("’", "");
            if (!string.IsNullOrEmpty(organizationKey))
            {
                viewModel.SetPopDataList(organizationKey);

                if (viewModel.PopData.Count > 0)
                {
                    dgRecive.DataContext = viewModel.PopData;
                    pop_Organization.IsOpen = true;
                    dgRecive.SelectedIndex = 0;
                }
                else
                {
                    dgRecive.DataContext = null;
                    pop_Organization.PlacementTarget = TB_Organization;
                    pop_Organization.IsOpen = false;
                }
            }
            else
            {
                dgRecive.DataContext = null;
                pop_Organization.IsOpen = false;
            }
            lbRecive.SelectedIndex = -1;
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
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Confirm();
            this.Close();
        }

        /// <summary>
        /// 选择pop窗里面的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgRecive_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                InstitutionalInfo dataPo = dgRecive.SelectedItem as InstitutionalInfo;
                if (dataPo != null)
                {
                    pop_Organization.IsOpen = false;
                    viewModel.SearchInsShow = dataPo.Shortnamecn;
                    viewModel.SearchInsKey = dataPo.Idbkey;
                    //viewModel.AddOne();
                    TB_Organization.Text = dataPo.Shortnamecn;
                    TB_Organization.SelectionStart = TB_Organization.Text.Length;
                    GoToOrderBySeq(dataPo);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                pop_Organization.IsOpen = false;
            }
        }

        /// <summary>
        /// 查找已添加的机构
        /// </summary>
        /// <param name="dataPo"></param>
        private void GoToOrderBySeq(InstitutionalInfo dataPo)
        {
            var tuple = viewModel.GetAddedItemIndex(dataPo.Idbkey);
            if (tuple != null)
            {
                lbRecive.SelectedIndex = tuple.Item1;
                lbRecive.ScrollIntoView(tuple.Item2);
            }
        }


        /// <summary>
        /// 添加按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddOne();
            TB_Organization.Text = string.Empty;
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
    }
}
