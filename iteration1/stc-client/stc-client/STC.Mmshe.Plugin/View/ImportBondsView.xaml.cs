using STC.Common.Model.MM;
using STC.Common.Model.MMshe;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Mmshe.Plugin.Utils.Data;
using STC.Mmshe.Plugin.ViewModel;
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

namespace STC.Mmshe.Plugin.View
{
    /// <summary>
    /// ImportBondsView.xaml 的交互逻辑
    /// </summary>
    public partial class ImportBondsView : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        ImportBondsViewModel vm = new ImportBondsViewModel();

        public ImportBondsView(MMsheGroupInfo mmgroup)
        {
            InitializeComponent();
            vm.SelectGroup = mmgroup;
            this.DataContext = vm;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string importStr = this.importTxt.Text;
            vm.BindBonds(importStr);
            this.step1.Visibility = Visibility.Collapsed;
            this.step2.Visibility = Visibility.Visible;
            this.btnCancel.Visibility = Visibility.Collapsed;
            this.btnOK.Visibility = Visibility.Collapsed;
            this.btnOK.IsEnabled = false;
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
            vm.BatchImportBonds();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.step1.Visibility = Visibility.Visible;
            this.step2.Visibility = Visibility.Collapsed;
            this.btnCancel.Visibility = Visibility.Visible;
            this.btnOK.Visibility = Visibility.Visible;
            this.btnOK.IsEnabled = true;
            this.btnBack.Visibility = Visibility.Collapsed;
            this.btnOK2.Visibility = Visibility.Collapsed;
            vm.ClearBinds();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            vm.CalCheckedCount();
        }

        private void btnOK3_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;

            // todo 验证
            this.Close();
            e.Handled = true;
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if (vm != null)
            {
                vm.CancelOpt();
            }
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (vm != null)
            {
                if(vm.DoImportCommand())
                {
                    string importStr = this.importTxt.Text;
                    vm.BindBonds(importStr);
                    this.step1.Visibility = Visibility.Collapsed;
                    this.step2.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Visible;
                    this.btnOK.Visibility = Visibility.Collapsed;
                    this.btnOK.IsEnabled = false;
                    this.btnBack.Visibility = Visibility.Collapsed;
                    this.btnOK2.Visibility = Visibility.Visible;
                }
            }
        }

        private void IsHomeCb_Checked(object sender, RoutedEventArgs e)
        {
            vm.BindBonds(string.Empty);
            this.step1.Visibility = Visibility.Collapsed;
            this.step2.Visibility = Visibility.Visible;
            this.btnCancel.Visibility = Visibility.Visible;
            this.btnOK.Visibility = Visibility.Collapsed;
            this.btnOK.IsEnabled = false;
            this.btnBack.Visibility = Visibility.Collapsed;
            this.btnOK2.Visibility = Visibility.Visible;
        }

        private bool handleSelection = true;
        private void UnifyGroupCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (handleSelection)
            {
                GroupEntity addItem = e.AddedItems[0] as GroupEntity;
                GroupEntity removeItem = e.RemovedItems[0] as GroupEntity;
                if (MMsheGroupHandler.Instance.HasOpen(addItem.GroupId))
                {
                    handleSelection = false;
                    this.UnifyGroupCb.SelectedItem = removeItem;
                    CommonUtils.Prompt2Second("批量导入",$"[{addItem.GroupName}]分组内存在未关闭策略，请确认关闭后再批量导入",this);
                    return;
                }
            }
            handleSelection = true;
        }

        private void WindowBase_ContentRendered(object sender, EventArgs e)
        {
            this.UnifyGroupCb.SelectionChanged -= UnifyGroupCb_SelectionChanged;
            this.UnifyGroupCb.SelectionChanged += UnifyGroupCb_SelectionChanged;
        }

        private void IsHomeCb_Unchecked(object sender, RoutedEventArgs e)
        {
            this.step1.Visibility = Visibility.Visible;
            this.step2.Visibility = Visibility.Collapsed;
            this.btnCancel.Visibility = Visibility.Visible;
            this.btnOK.Visibility = Visibility.Visible;
            this.btnOK.IsEnabled = true;
            this.btnBack.Visibility = Visibility.Collapsed;
            this.btnOK2.Visibility = Visibility.Collapsed;
            vm.ClearBinds();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.ClearBinds();
            vm.BindBonds(string.Empty);
            /*this.step1.Visibility = Visibility.Collapsed;
            this.step2.Visibility = Visibility.Visible;
            this.btnCancel.Visibility = Visibility.Visible;
            this.btnOK.Visibility = Visibility.Collapsed;
            this.btnOK.IsEnabled = false;
            this.btnBack.Visibility = Visibility.Collapsed;
            this.btnOK2.Visibility = Visibility.Visible;*/
        }
    }
}
