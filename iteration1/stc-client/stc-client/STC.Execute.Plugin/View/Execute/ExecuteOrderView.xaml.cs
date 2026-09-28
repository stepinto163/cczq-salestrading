using STC.Common.Handler.Base;
using STC.Common.Utils;
using STC.Execute.Plugin.Handler;
using STC.Execute.Plugin.Handler.Process;
using STC.Execute.Plugin.ViewModel.Execute;
using System;
using System.Collections;
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

namespace STC.Execute.Plugin.View.Execute
{
    /// <summary>
    /// ExecuteOrderView.xaml 的交互逻辑
    /// </summary>
    public partial class ExecuteOrderView : UserControl
    {
        private ExecuteOrderViewModel _executeOrderViewModel = null;
        ContextMenu contextMenu;
        public ExecuteOrderView()
        {
            InitializeComponent();

            _executeOrderViewModel = new ExecuteOrderViewModel();
            _executeOrderViewModel.Init(this);
            BaseHandler.Instance.Init(_executeOrderViewModel.Instance_OnClear);
            this.DataContext = _executeOrderViewModel;
            this.Loaded += ExecuteOrderView_Loaded;

            this.Unloaded += ExecuteOrderView_Unloaded;
            Executions_dataGrid.SelectedIndex = 0;
        }


        private void SetContextMenu()
        {
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("CopyContextMenu");
            }
        }


        private void ExecuteOrderView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ExecuteOrderView_Unloaded;
            //if (_executeOrderViewModel != null)
                //_executeOrderViewModel.Clear();

        }

        private void ExecuteOrderView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ExecuteOrderView_Loaded;
            ExecuterOrderHandler.Instance.SearchExeOrder();
        }

        private void Executions_dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView dataRowView = Executions_dataGrid.SelectedItem as DataRowView;
                if (dataRowView == null)
                    return;
                DataRow row = dataRowView.Row as DataRow;
                detailView.OnOrder(row);
                historyView.OnOrder(row);
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }

      

        private void BrokerFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_executeOrderViewModel != null)
                _executeOrderViewModel.FilterData(getFilter());
        }

        private void ExecStatusFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_executeOrderViewModel != null)
                _executeOrderViewModel.FilterData(getFilter());
        }

        private void securityTypeFilters_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_executeOrderViewModel != null)
                _executeOrderViewModel.FilterData(getFilter());
        }

        private Dictionary<string, IList> getFilter()
        {
            Dictionary<string, IList> result = new Dictionary<string, IList>();
            Dictionary<string, IList> brokerDic = brokerFilter.FilterDic;
            Dictionary<string, IList> statusDic = statusFilter.FilterDic;
            Dictionary<string, IList> securityTypeDic = securityTypeFilters.FilterDic;

            if (brokerDic != null)
            {
                foreach (KeyValuePair<string, IList> kv in brokerDic)
                {
                    result.Add(kv.Key, kv.Value);
                }
            }

            if (statusDic != null)
            {
                foreach (KeyValuePair<string, IList> kv in statusDic)
                {
                    result.Add(kv.Key, kv.Value);
                }
            }

            if (securityTypeDic != null)
            {
                foreach (KeyValuePair<string, IList> kv in securityTypeDic)
                {
                    result.Add(kv.Key, kv.Value);
                }
            }

            if (!string.IsNullOrEmpty(btnSearch.Text))
            {
                if (comboBox.SelectedIndex == 0)
                {
                    List<string> list = new List<string>();
                    list.Add(btnSearch.Text);
                    result.Add("Code", list);
                }
                else if (comboBox.SelectedIndex == 1)
                {
                    List<string> list = new List<string>();
                    list.Add(btnSearch.Text);
                    result.Add("UserName", list);
                }
                else if (comboBox.SelectedIndex == 2)
                {
                    List<string> list = new List<string>();
                    list.Add(btnSearch.Text);
                    result.Add("CounterParty", list);
                } 
            }


            return result;
        }

        private void Search_TextBox_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (_executeOrderViewModel != null)
                _executeOrderViewModel.FilterData(getFilter());
        }

        private void orderTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_executeOrderViewModel != null)
                    _executeOrderViewModel.FilterData(getFilter());
            }
            else if (e.Key == Key.Escape)
            {
                btnSearch.Text = string.Empty;
            }
        }

        private bool orderFlag = false;
        private void orderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!orderFlag)
                return;
        }

        private void orderTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            orderFlag = true;
        }

        private void orderTextBox_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            orderFlag = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnSearch.Text = string.Empty;
        }

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            
        }

        private void Executions_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.Executions_dataGrid.SelectedItems.Count > 0)
            {
                SetContextMenu();
                contextMenu.IsOpen = true;
            }
        }

        private void copyRow_Click(object sender, RoutedEventArgs e)
        {
            if (_executeOrderViewModel != null)
            {
                string value = _executeOrderViewModel.copyRowData(Executions_dataGrid.SelectedItems);
                ClipboardHelper.CopyToClipboard(value, true);
            }
        }

      

        private bool _horClose = false;


        //private void BtnHor_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!_horClose)
        //    {
        //        gridMain.RowDefinitions[1].Height = new GridLength(0);
        //        _horClose = true;

        //        btnHor.Content = "∧";
        //    }
        //    else
        //    {
        //        gridMain.RowDefinitions[2].Height = new GridLength(60, GridUnitType.Star);
        //        //gridMain.RowDefinitions[4].Height = new GridLength(40, GridUnitType.Star);
        //        _horClose = false;

        //        btnHor.Content = "∨";
        //    }
        //}
    }
}
