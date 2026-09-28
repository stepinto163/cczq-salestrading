using STC.Common.Utils;
using STC.InnerBoard.Plugin.ViewModel;
using STC.InnerBoard.Plugin.Utils;
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
using System.Windows.Threading;

namespace STC.InnerBoard.Plugin.View
{
    /// <summary>
    /// HistoryBrokerListView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryBrokerListView : UserControl
    {
       
        private HistoryBrokerListViewModel _viewModel = new HistoryBrokerListViewModel();
        public HistoryBrokerListView()
        {
            InitializeComponent();
            if (firmToNoTimer == null)
            {
                firmToNoTimer = new DispatcherTimer();
                firmToNoTimer.Interval = new TimeSpan(1000);
                firmToNoTimer.Tick += FirmToNoTimer_Tick;
            }
        }

        private void FirmToNoTimer_Tick(object sender, EventArgs e)
        {
        }
        private bool status = false;
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _viewModel;
            _viewModel.HistoryView = this;
            _viewModel.PageControl = this.pageControl;
            //_viewModel.PageControl.PageSize = 2;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.clear();
            pageControl.Page = 1;
            _viewModel.Init();

            //_viewModel.ProcessMsgsFilterCondition(this.historyFilter.RiskFilterDic);
        }

    

        private void Risk_dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void ContentPresenter_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void historyFilter_RiskFilterDicChanged(object sender, RoutedEventArgs e)
        {
            pageControl.Page = 1;
            _viewModel.ProcessMsgsFilterCondition(this.historyFilter.RiskFilterDic);
        }

        private void pageControl_PageChanged(object sender, Common.View.Component.PageChangedEventArgs e)
        {
            if (e != null)
            {
                _viewModel.Index = e.Page - 1;
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    _viewModel.clear();
                    ShowMask(true);
                    _viewModel.RequestData();
                });
            }
        }
        DispatcherTimer firmToNoTimer;
        DataRowView drv;
        private void clock_img_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((Image)sender).DataContext is DataRowView)
            {
                drv = ((Image)sender).DataContext as DataRowView;


                if (firmToNoTimer.IsEnabled == false)
                {
                    firmToNoTimer.Start();
                }
            }
        }

        private void clock_img_MouseLeave(object sender, MouseEventArgs e)
        {

        }
        public void ShowMask(bool flag)
        {
            loadMask.Dispatcher.Invoke(() =>
            {
                loadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AppUtils.IsInnerInput = false;
        }
    }
}
