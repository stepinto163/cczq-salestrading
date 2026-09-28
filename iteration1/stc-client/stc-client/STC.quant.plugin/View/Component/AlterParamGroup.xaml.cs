using STC.Common.View.Component;
using STC.quant.plugin.ViewModel;
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

namespace STC.quant.plugin.View.Component
{
    /// <summary>
    /// AlterParamGroup.xaml 的交互逻辑
    /// </summary>
    public partial class AlterParamGroup : WindowBase
    {
        public AlterParamViewModel viewModel = AlterParamViewModel.Instance;
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// 组id
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 策略的作者
        /// </summary>
        public string UserName { get; set; }
        

        public AlterParamGroup()
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.Loaded += AlterParamGroup_Loaded;
        }

        private void AlterParamGroup_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.initCheckGroup(GroupId,GroupName,UserName);
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.Close();
            e.Handled = true;
            string[] str = viewModel.AlterGroupBondClick();
            GroupId = str[0];
            GroupName = str[1];
        }
    }
}
