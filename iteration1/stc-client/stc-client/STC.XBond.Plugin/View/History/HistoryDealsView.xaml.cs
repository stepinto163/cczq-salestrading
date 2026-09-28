using STC.Common.Utils;
using STC.XBond.Plugin.ViewModel;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// HistoryDealsView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryDealsView : UserControl
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
        private HistoryDealsViewModel _viewModel = new HistoryDealsViewModel();
        public HistoryDealsView()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _viewModel;
            _viewModel.HistoryView = this;
            _viewModel.PageControl = this.pageControl;
            //_viewModel.Init();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //_viewModel.Init();
        }

        public void load()
        {
            _viewModel.clear();
            pageControl.Page = 1;
            _viewModel.Init();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
            {
                FilterCondition();
                pageControl.Page = 1;
                _viewModel.ProcessDealsFilterCondition(this.historyDealsFilter.DealsFilterDic);
            }
        }

        private void FilterCondition()
        {
            string text = textBox.Text;
            if (((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("1"))
            {
                _viewModel.Request.execid = text;
                _viewModel.Request.strategyorderid = "";
                _viewModel.Request.securityid = "";
                _viewModel.Request.counterpartyname = "";
            }
            if (((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("2"))
            {
                _viewModel.Request.execid = "";
                _viewModel.Request.strategyorderid = text;
                _viewModel.Request.securityid = "";
                _viewModel.Request.counterpartyname = "";
            }
            if (((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("3"))
            {
                _viewModel.Request.execid = "";
                _viewModel.Request.strategyorderid = "";
                _viewModel.Request.securityid = text;
                _viewModel.Request.counterpartyname = "";
            }
            if (((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("4"))
            {
                _viewModel.Request.execid = "";
                _viewModel.Request.strategyorderid = "";
                _viewModel.Request.securityid = "";
                _viewModel.Request.counterpartyname = text;
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
                _viewModel.ProcessDealsFilterCondition(this.historyDealsFilter.DealsFilterDic);
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
                _viewModel.Request.execid = "";
                _viewModel.Request.strategyorderid = "";
                _viewModel.Request.securityid = "";
                _viewModel.Request.counterpartyname = "";
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

        private void ContentPresenter_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void Executions_dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void pageControl_PageChanged(object sender, Common.View.Component.PageChangedEventArgs e)
        {
            if (e != null)
            {
                _viewModel.clear();
                _viewModel.Index = e.Page - 1;
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    ShowMask(true);
                    _viewModel.RequestMarketExecutions();
                });
            }
        }

        private void historyDealsFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            pageControl.Page = 1;
            _viewModel.ProcessDealsFilterCondition(this.historyDealsFilter.DealsFilterDic);
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
