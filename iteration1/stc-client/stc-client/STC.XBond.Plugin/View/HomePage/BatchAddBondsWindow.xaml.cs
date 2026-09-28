using STC.Common.Utils;
using STC.Common.View.Component;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel.HomePage;
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

namespace STC.XBond.Plugin.View.HomePage
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
            _batchAddBondsWindowViewModel.GroupName = HomePageDataHandler.GetGroupNameByGroupID(groupID);
            this.DataContext = _batchAddBondsWindowViewModel;
        }

        
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string importStr = this.importTxt.Text;
            _batchAddBondsWindowViewModel.BindBonds(importStr);
            this.step1.Visibility = Visibility.Collapsed;
            this.step2.Visibility = Visibility.Visible;
            this.btnCancel.Visibility = Visibility.Collapsed;
            this.btnOK.Visibility = Visibility.Collapsed;
            this.btnBack.Visibility = Visibility.Visible;
            this.btnOK2.Visibility = Visibility.Visible;
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
            this.btnBack.Visibility = Visibility.Collapsed;
            this.btnOK2.Visibility = Visibility.Collapsed;
            this.step3.Visibility = Visibility.Visible;
            this.btnOK3.Visibility = Visibility.Visible;
            this.btnCancel.Visibility = Visibility.Hidden;
            // 开始批量导入
            _batchAddBondsWindowViewModel.BatchImportBonds();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.step1.Visibility = Visibility.Visible;
            this.step2.Visibility = Visibility.Collapsed;
            this.btnCancel.Visibility = Visibility.Visible;
            this.btnOK.Visibility = Visibility.Visible;
            this.btnBack.Visibility = Visibility.Collapsed;
            this.btnOK2.Visibility = Visibility.Collapsed;
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
            //if (this._batchAddBondsWindowViewModel != null && this._batchAddBondsWindowViewModel.FinishOpt() == false)
            //{
            //    bool flag = CustomMessageBox.Question("确认要取消批量导入吗?", "取消批量导入",this);
            //    if (!flag)
            //    {
            //        e.Cancel = true;
            //        return;
            //    }
            //}
            this.Owner = null;
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if (_batchAddBondsWindowViewModel != null)
            {
                _batchAddBondsWindowViewModel.CancelOpt();
            }
        }

        private void importTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.importTxt.Text))
            {
                this.btnOK.IsEnabled = false;
            }
            else
            {
                this.btnOK.IsEnabled = true;
            }
        }
    }
}
