using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.ViewModel.Trade;
using STC.Common.View.Component;
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

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// BatchAddBondsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BatchAddBondsWindow : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        private BatchAddBondsWindowViewModel _batchAddBondsWindowViewModel = new BatchAddBondsWindowViewModel();

        public BatchAddBondsWindow(string groupID)
        {
            InitializeComponent();
            _batchAddBondsWindowViewModel.GroupID = groupID;
            _batchAddBondsWindowViewModel.GroupName = HomePageDataHandler.Instance.GetGroupNameByGroupID(groupID);
            this.DataContext = _batchAddBondsWindowViewModel;
        }

        public object GetData()
        {
            return _batchAddBondsWindowViewModel.ImportDv;
        }
        
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string importStr = this.importTxt.Text;
            _batchAddBondsWindowViewModel.BindBonds(importStr);

            Result = true;
            e.Handled = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK2_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            //// todo 验证
            //this.Close();
            // e.Handled = true;
            this.step2.Visibility = Visibility.Collapsed;
            this.btnOK2.Visibility = Visibility.Collapsed;
            this.step3.Visibility = Visibility.Visible;
            this.btnOK3.Visibility = Visibility.Visible;
            
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            _batchAddBondsWindowViewModel.CalCheckedCount();
        }

        private void btnOK3_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;
           
            // todo 验证
            this.Close();
            e.Handled = true;
        }

        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if (_batchAddBondsWindowViewModel != null)
            {
                _batchAddBondsWindowViewModel.CancelOpt();
            }
        }
    }
}
