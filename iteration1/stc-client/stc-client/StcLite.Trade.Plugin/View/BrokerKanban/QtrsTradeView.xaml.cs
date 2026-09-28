using BDB.Trade.Plugin.ViewModel.BrokerKanban;
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

namespace BDB.Trade.Plugin.View.BrokerKanban
{
    /// <summary>
    /// 已确认
    /// </summary>
    public partial class QtrsTradeView : UserControl
    {
        QtrsTradeViewModel mainViewModel;
        ContextMenu contextMenu;
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

        public QtrsTradeView()
        {
            InitializeComponent();

        }
        public QtrsTradeView(bool IsShowMaxied = true)
        {
            InitializeComponent();
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                btnFullScreen.Visibility = IsShowMaxied ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            mainViewModel = new QtrsTradeViewModel();
            mainViewModel.HistoryView = this;
            this.DataContext = mainViewModel;
            MyOrders.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, Copy_Executed, Copy_CanExecute));
        }

        public void ReLoadViewModel()
        {
            this.UserControl_Initialized(null, null);
        }

        public void load()
        {
            (this.DataContext as QtrsTradeViewModel).InitView();
        }


        private void dealFilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            (this.DataContext as QtrsTradeViewModel).ProcessFilterCondition(this.qtFilterPanel.FilterDic);
        }

        public void ShowEMSMask(bool flag)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                positionMainMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        public void Dispose()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                (this.DataContext as QtrsTradeViewModel).UnRegistEvent();
                 this.DataContext = null;
            }));
        }

        private void emsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        /// <summary>
        /// 设置菜单
        /// </summary>
        private void SetContextMenu()
        {
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("CopyContextMenu");
            }
        }

        /// <summary>
        /// 鼠标右键菜单打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        /// <summary>
        /// 鼠标右键复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyRow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string value = (this.DataContext as QtrsTradeViewModel).CopyRowText(this.MyOrders.SelectedItems);
                Clipboard.SetDataObject(value, true);
                MyOrders.UnselectAll();
            }));
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.MyOrders.SelectedItems.Count > 0)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    string value = (this.DataContext as QtrsTradeViewModel).CopyRowText(this.MyOrders.SelectedItems);
                    Clipboard.SetDataObject(value, true);
                    MyOrders.UnselectAll();
                    e.Handled = true;
                }));
            }
            else
            {
                e.Handled = false;
            }
        }

        private void MyOrders_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.MyOrders.SelectedItems.Count > 0)
            {
                SetContextMenu();
                contextMenu.IsOpen = true;
            }
        }

    }
}
