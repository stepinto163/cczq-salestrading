using STC.InnerBoard.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace STC.InnerBoard.Plugin.View
{
    /// <summary>
    /// BrokerList.xaml 的交互逻辑
    /// </summary>
    public partial class BrokerListView : UserControl
    {
        private ListManagementViewModel _viewModel = ListManagementViewModel.UserManager;
        public BrokerListView()
        {
            InitializeComponent();

            this.DataContext = _viewModel;
            _viewModel.View = this;
            this.Loaded += BrokerListView_Loaded;
        }

        private void BrokerListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.init();
        }

      

        /// <summary>
        /// 聚焦选中的行
        /// </summary>
        public void ScrollTable()
        {
            this.TPDataGrid.SelectedIndex = this.TPDataGrid.Items.Count - 1;
            this.TPDataGrid.ScrollIntoView(this.TPDataGrid.SelectedItem);
            this.TPDataGrid.UpdateLayout();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
        private void enableChk_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender != null && sender is CheckBox)
            {
                CheckBox cb = sender as CheckBox;
                if (cb != null && cb.DataContext != null && cb.DataContext is DataRowView)
                {
                    DataRowView currentDRV = cb.DataContext as DataRowView;
                    string IsSwitch = currentDRV["WhiteListEnable"] + "";
                    string updateType = currentDRV["UpdateType"] + "";
                    IsSwitch = IsSwitch.Equals("0") ? "1" : "0";
                    _viewModel.ChangeStatus(IsSwitch, currentDRV);
                }
            }
        }

    }
}
