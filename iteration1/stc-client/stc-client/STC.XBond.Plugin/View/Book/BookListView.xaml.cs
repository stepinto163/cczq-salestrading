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
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Utils;

namespace STC.XBond.Plugin.View.Book
{
    /// <summary>
    /// BookListView.xaml 的交互逻辑
    /// </summary>
    public partial class BookListView : UserControl
    {
        private BookListViewModel _BookListViewModel = new BookListViewModel();

        public BookListView()
        {
            InitializeComponent();
            BaseHandler.Instance.Init(Instance_OnClear);
            this.DataContext = _BookListViewModel;
        }
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("BookListView sort Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.MainBooks_dataGrid.Items.SortDescriptions.Clear();
                    foreach (DataGridColumn column in this.MainBooks_dataGrid.Columns)
                    {
                        column.SortDirection = null;
                    }
                }));
            }
        }
        private void Books_dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_BookListViewModel != null)
            {
                _BookListViewModel.selectBlockBySTCExecID();
            }

        }

        private void pageControl_PageChanged(object sender, CustomeUI.PageChangedEventArgs e)
        {
			if (e != null && _BookListViewModel != null)
            {
                _BookListViewModel.PageNum = e.Page - 1;
            }
        }

        private void bookStateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.bookStateComboBox.SelectedItem != null)
            {
                ComboBoxItem selectedComboBoxItem = this.bookStateComboBox.SelectedItem as ComboBoxItem;
                _BookListViewModel.ExecType = selectedComboBoxItem.Tag.ToString();
            }
        }

        private void BookTerminalFilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_BookListViewModel != null)
            {
                _BookListViewModel.ProcessTerminalFilterConditon(this.bookTerminalFilterPanel.FilterDic);
                _BookListViewModel.ProcessExeTypeFilterConditon(this.bookTerminalFilterPanel.FilterDic);
            }
        }

        private void subBookTypeFilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
			if (_BookListViewModel != null)
	    	{
            	_BookListViewModel.ProcessSubDealTypeFilterCondition(this.subBookTypeFilterPanel.FilterDic);
			}
        }
		
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (_BookListViewModel != null)
            {
                _BookListViewModel.selectBlockBySTCExecID();
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (_BookListViewModel != null)
            {
                _BookListViewModel.Collapsed();
            }
        }
		
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (_BookListViewModel != null)
            {
                _BookListViewModel.Init();
            }
        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainBooks_dataGrid.SelectedItem = ((Button)sender).DataContext;
        }
    }
}
