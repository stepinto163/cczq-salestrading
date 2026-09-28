using STC.Broker.Plugin.Model.Transaction.CrossBridge;
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
    /// BatchCrossBridgeWin.xaml 的交互逻辑
    /// </summary>
    public partial class BatchCrossBridgeWin : Window
    {
        private BatchCrossBridgeWinViewModel viewModel;
        private CrossBridgeConfWin crossBridgeConfWin;
        

        public BatchCrossBridgeWin()
        {
            InitializeComponent();
            viewModel = new BatchCrossBridgeWinViewModel(this, LoadingUI);
            this.DataContext = viewModel;
        }

        /// <summary>
        /// 切换桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lsBridge_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.ChangeBirdgeData(lsBridge.SelectedIndex);
            bool isCheck;
            if (viewModel.checkAllCache.TryGetValue(lsBridge.SelectedIndex, out isCheck))
            {
                CheckAll.IsChecked = isCheck;
            }
        }

        /// <summary>
        /// 切换到指定索引的桥
        /// </summary>
        /// <param name="index"></param>
        public void SetBridgeIndex(int index)
        {
            lsBridge.SelectedIndex = index;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 点击运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.RunBatchCrossBridge();
            CheckAll.IsChecked = false;
        }

        /// <summary>
        /// 确认过桥订单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.ConfirmBrigeOrder(lsBridge.SelectedIndex);
            CheckAll.IsChecked = false;
        }

        /// <summary>
        /// 移动窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// 选择所有订单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetAllBridgeOrderIsSelected(lsBridge.SelectedIndex, CheckAll.IsChecked.Value);
            BridgeOrderData.SelectedIndex = -1;
        }

        /// <summary>
        /// 单个订单选择或取消选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BridgeOrderData_Selected(object sender, MouseButtonEventArgs e)
        {
            //if (BridgeOrderData.SelectedItems.Count > 0)
            //{
            //    foreach (BridgeOrderInfoViewModel row in BridgeOrderData.SelectedItems)
            //    {
            //        row.IsSelectedFlag = !row.IsSelectedFlag;
            //    }
            //}
        }

        /// <summary>
        /// 一键过桥配置界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Config_Click(object sender, RoutedEventArgs e)
        {
            if (crossBridgeConfWin == null)
            {
                crossBridgeConfWin = new CrossBridgeConfWin();
                crossBridgeConfWin.Closed += (a, b) => { crossBridgeConfWin = null; };
                crossBridgeConfWin.Show();
            }
            else
            {
                crossBridgeConfWin.Activate();
            }
        }

        private void cbSort_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckAll.IsChecked = false;
            viewModel.checkAllCache[lsBridge.SelectedIndex] = false;
        }

        private void cbSort_Checked(object sender, RoutedEventArgs e)
        {           
            if (viewModel.NeedCheckAll())
            {
                CheckAll.IsChecked = true;
                viewModel.checkAllCache[lsBridge.SelectedIndex] = true;
            }
        }

        private void BridgeOrderData_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) != System.Windows.Forms.Keys.Shift)
            {
                BridgeOrderData.SelectedItems.Clear();
            }
        }

        private void cbSort_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                CheckBox cb = sender as CheckBox;
                if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift)
                {
                    if (BridgeOrderData.SelectedItems.Count > 0)
                    {
                        foreach (BridgeOrderInfoViewModel row in BridgeOrderData.SelectedItems)
                        {
                            row.IsSelectedFlag = cb.IsChecked.Value;
                        }
                    }
                }
            }

        }
    }
}
