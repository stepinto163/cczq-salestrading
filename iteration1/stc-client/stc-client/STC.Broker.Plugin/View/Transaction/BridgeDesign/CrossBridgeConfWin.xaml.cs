
using STC.Broker.Plugin.View.Component;
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
    /// CrossBridgeConfWin.xaml 的交互逻辑
    /// </summary>
    public partial class CrossBridgeConfWin : Window
    {
        private BridgeConfigViewModel viewModel;


        public CrossBridgeConfWin()
        {
            InitializeComponent();
            viewModel = new BridgeConfigViewModel(this.LoadingUI);
            viewModel.BridgeConfigInfos.CollectionChanged += BridgeConfigInfos_CollectionChanged;
            DataContext = viewModel;
        }

        private void BridgeConfigInfos_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            viewModel.ResetPriority();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            viewModel.BridgeConfigInfos.CollectionChanged -= BridgeConfigInfos_CollectionChanged;
            Close();
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
        /// 添加一行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddOneRow();

        }

        /// <summary>
        /// 确认按钮， 更新桥配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateData();
            BlackListViewModel.InsCache.Clear();
            this.Close();
        }

        /// <summary>
        /// 取消按钮。 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            BlackListViewModel.InsCache.Clear();
            this.Close();
        }

        /// <summary>
        /// 添加黑名单按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BlackListButton_Click(object sender, RoutedEventArgs e)
        {
            if (BridgeDataGrid.SelectedItem is BridgeConfigInfoViewModel)
            {
                BridgeConfigInfoViewModel model = (BridgeConfigInfoViewModel)BridgeDataGrid.SelectedItem;
                BlackListWin blackListWin = new BlackListWin(model);
                blackListWin.Show();
            }
        }


        private void BridgeDataGrid_Drop(object sender, DragEventArgs e)
        {

        }

        /// <summary>
        /// DataGrid 右键删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgMenu_Delete(object sender, RoutedEventArgs e)
        {
            if (BridgeDataGrid.SelectedItem is BridgeConfigInfoViewModel)
            {
                BridgeConfigInfoViewModel model = (BridgeConfigInfoViewModel)BridgeDataGrid.SelectedItem;
                viewModel.RemoveOneRow(model);
            }
        }


        private void BridgeDataGrid_PreviewDrop(object sender, DragEventArgs e)
        {
            //if (sender is DataGrid dataGrid && dataGrid.SelectedItem is BridgeConfigInfoViewModel model)
            //{

            //}
        }

        /// <summary>
        /// 机构交易员textchange事件
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void PopTextBox_TextChanged(object arg1, string arg2)
        {
            var model = BridgeDataGrid.SelectedItem as BridgeConfigInfoViewModel ;
            var data = viewModel.GetPopDataList(model, arg2);
            PopTextBox.PopDataList.Clear();
            data.ForEach(x => PopTextBox.PopDataList.Add(x));
        }
    }
}
