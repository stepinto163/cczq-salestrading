using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace STC.Frame.UI
{
    /// <summary>
    /// SecurityGroupManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class SecurityGroupManagerView : UserControl
    {
        SecurityGroupManagerViewModel viewModel = new SecurityGroupManagerViewModel();
        public SecurityGroupManagerView()
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.Loaded += SecurityGroupManagerView_Loaded;
            ShowAll_ToggleButton.Checked += ShowAll_ToggleButton_Checked;
            viewModel.load();
            viewModel.securityGroupManagerView = this;
        }

        public void setDefaultParam(string defaultTag) 
        {
            viewModel.defaultTag = defaultTag;
        }
        private void ShowAll_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            LstModules.SelectedIndex = 0;
            viewModel.RefreshCondition();
            
        }
        private void SecurityGroupManagerView_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.initGroup();
        }


        private void WindowBase_Closed(object sender, EventArgs e)
        {
            viewModel.clearGroup();
        }

        private void LstModules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel != null && viewModel.SelectedGroupItem != null)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    this.detailView.initBondList(viewModel.SelectedGroupItem);
                });
            }
        }
    }
}
