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
    /// HistoryAgencyView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryAgencyView : UserControl
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
        private HistoryAgencyViewModel _viewModel = new HistoryAgencyViewModel();
        public HistoryAgencyView()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _viewModel;
            _viewModel.HistoryView = this;
            _viewModel.PageControl = this.pageControl;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //_viewModel.clear();
            //_viewModel.Init();
        }
        public void load()
        {
            _viewModel.clear();
            pageControl.Page = 1;
            _viewModel.Init();
        }
        private void historyMsgFilter_MsgFilterDicChanged(object sender, RoutedEventArgs e)
        {
            pageControl.Page = 1;
            _viewModel.ProcessMsgsFilterCondition(this.historyMsgFilter.AgencyFilterDic);
        }

        //private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
        //    {
        //        FilterCondition();
        //        pageControl.Page = 1;
        //        _viewModel.ProcessMsgsFilterCondition(this.historyMsgFilter.AgencyFilterDic);
        //    }
        //}
        private void FilterCondition()
        {
            //string text = textBox.Text;
            //if (((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("1") ||
            //    ((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("2") ||
            //    ((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("3"))
            //{
            //    _viewModel.Request.contentP2 = text;
            //}
        }
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //FilterCondition();
                pageControl.Page = 1;
                //_viewModel.ProcessMsgsFilterCondition(this.historyMsgFilter.AgencyFilterDic);
            }
        }
        private bool Flag = false;
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Flag)
                return;
            if (string.IsNullOrEmpty(textBox.SearchText.Trim()))
            {
                _viewModel.Request.contentP2 = "";
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

        private void Msg_dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
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
                    _viewModel.RequestMsg();
                });
            }
        }

        private void ContentPresenter_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        public void ShowMask(bool flag)
        {
            loadMask.Dispatcher.Invoke(() =>
            {
                loadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }
    }
}
