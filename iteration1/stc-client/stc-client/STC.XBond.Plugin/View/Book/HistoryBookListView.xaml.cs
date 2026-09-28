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

namespace STC.XBond.Plugin.View.Book
{
    /// <summary>
    /// HistoryBookListView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryBookListView : UserControl
    {
        private HistoryBookListViewModel _HistoryBookListViewModel = new HistoryBookListViewModel();

        public HistoryBookListView()
        {
            InitializeComponent();
            this.DataContext = _HistoryBookListViewModel;
        }

        private void bookStateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_HistoryBookListViewModel != null)
            {
                _HistoryBookListViewModel.ExecType = (this.bookStateComboBox.SelectedItem as ComboBoxItem).Tag.ToString();
            }
        }

        private void BookTerminalFilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_HistoryBookListViewModel != null)
            {
                _HistoryBookListViewModel.ProcessTerminalFilterConditon(this.bookTerminalFilterPanel.FilterDic);
            }
        }

        private void pageControl_PageChanged(object sender, CustomeUI.PageChangedEventArgs e)
        {
            if (e != null && _HistoryBookListViewModel != null)
            {
                _HistoryBookListViewModel.PageNum = e.Page - 1;
            }
        }

        private void Books_dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_HistoryBookListViewModel != null)
            {
                _HistoryBookListViewModel.selectBlockBySTCExecID();
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (_HistoryBookListViewModel != null)
            {
                _HistoryBookListViewModel.selectBlockBySTCExecID();
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (_HistoryBookListViewModel != null)
            {
                _HistoryBookListViewModel.Collapsed();
            }
        }

        private void subBookTypeFilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_HistoryBookListViewModel != null)
            {
                _HistoryBookListViewModel.ProcessSubDealTypeFilterCondition(this.subBookTypeFilterPanel.FilterDic);
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (_HistoryBookListViewModel != null)
            {
                _HistoryBookListViewModel.Init();
            }
        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainBooks_dataGrid.SelectedItem = ((Button)sender).DataContext;
        }
    }
}
