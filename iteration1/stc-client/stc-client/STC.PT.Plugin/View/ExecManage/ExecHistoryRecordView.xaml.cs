using STC.Common.View.Component;
using STC.PT.Plugin.ViewModel.ExecManage;
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

namespace STC.PT.Plugin.View.ExecManage
{
    /// <summary>
    /// ExecHistoryRecordView.xaml 的交互逻辑
    /// </summary>
    public partial class ExecHistoryRecordView : WindowBase
    {
        ExecHistoryRecordViewModel _viewModel = new ExecHistoryRecordViewModel();
        public ExecHistoryRecordView()
        {
            InitializeComponent();
            this.DataContext = _viewModel;
        }

        public ExecHistoryRecordView(string execid)
        {
            InitializeComponent();
            this.DataContext = _viewModel;
            _viewModel.Init(execid);
        }
        private void Post_dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void historyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void historyDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void historyDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
