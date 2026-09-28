using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// HistoryRiskView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryRiskView : UserControl
    {
        private bool isLoad = true;
        public bool IsLoad
        {
            get
            {
                return isLoad;
            }

            set
            {
                isLoad = value;
            }
        }
        private HistoryRiskViewModel _viewModel = new HistoryRiskViewModel();
        public HistoryRiskView()
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

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //_viewModel.clear();
            ////_viewModel.Init();

            //_viewModel.ProcessMsgsFilterCondition(this.historyFilter.RiskFilterDic);
        }

        public void load()
        {
            if (isLoad)
            {
                isLoad = false;

                _viewModel.clear();
                pageControl.Page = 1;

                _viewModel.ProcessMsgsFilterCondition(this.historyFilter.RiskFilterDic);
            }
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

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
            {
                FilterCondition();
                pageControl.Page = 1;
                _viewModel.ProcessMsgsFilterCondition(this.historyFilter.RiskFilterDic);
            }
        }
        private void FilterCondition()
        {
            string text = textBox.Text;
            if (((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("1"))
            {
                _viewModel.Request.orderid = text;
                _viewModel.Request.securityid = "";
            }
            if (((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("2"))
            {
                _viewModel.Request.orderid = "";
                _viewModel.Request.securityid = text;
            }
            if (((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("3"))
            {
                _viewModel.Request.orderid = "";
                _viewModel.Request.securityid = "";
            }
        }

        private bool isFromEnter = false;
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            isFromEnter = false;
            AppUtils.IsInnerInput = true;

            if (e.Key == Key.Enter)
            {
                isFromEnter = true;
                FilterCondition();
                pageControl.Page = 1;
                _viewModel.ProcessMsgsFilterCondition(this.historyFilter.RiskFilterDic);
            }
        }
        private bool Flag = false;
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppUtils.IsInnerInput = true;
            if (isFromEnter)
            {
                AppUtils.IsInnerInput = false;
            }

            if (!Flag)
                return;
            if (string.IsNullOrEmpty(textBox.Text.Trim()))
            {
                _viewModel.Request.orderid = "";
                _viewModel.Request.securityid = "";
            }
            else
            {
                FilterCondition();
            }
        }

        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Flag = true;
        }

        private void textBox_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            Flag = false;
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
