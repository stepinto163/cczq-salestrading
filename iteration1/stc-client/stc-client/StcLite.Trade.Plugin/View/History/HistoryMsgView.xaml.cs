using BDB.Trade.Plugin.ViewModel;
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

namespace BDB.Trade.Plugin.View
{
    /// <summary>
    /// HistoryMsgView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryMsgView : UserControl
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
        private HistoryMsgViewModel _viewModel = new HistoryMsgViewModel();
        public HistoryMsgView()
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
            //_viewModel.clear();
            //_viewModel.Init();
        }
        public void load()
        {
            _viewModel.clear();
            pageControl.Page = 1;
            _viewModel.Init();
        }
        private void ContentPresenter_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        //private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
        //    {
        //        FilterCondition();
        //        pageControl.Page = 1;
        //        _viewModel.ProcessMsgsFilterCondition(this.historyMsgFilter.MsgFilterDic);
        //    }
        //}
        private void FilterCondition()
        {
            //string text = textBox.SearchText;
            //if (((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("1")|| 
            //    ((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("2")|| 
            //    ((ComboBoxItem)comboBox.SelectedItem).Tag.ToString().Equals("3"))
            //{
                //_viewModel.Request.contentP2 = text;
            //}
        }
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //FilterCondition();
                pageControl.Page = 1;
                //_viewModel.ProcessMsgsFilterCondition(this.historyMsgFilter.MsgFilterDic);
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

        public void historyMsgFilter_MsgFilterDicChanged(object sender, RoutedEventArgs e)
        {
            pageControl.Page = 1;
            _viewModel.ProcessMsgsFilterCondition(this.historyMsgFilter.MsgFilterDic);
        }

        private void Msg_dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void pageControl_PageChanged(object sender, CustomeUI.PageChangedEventArgs e)
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
        public void ShowMask(bool flag)
        {
            loadMask.Dispatcher.Invoke(() =>
            {
                loadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }
        
    }
}
