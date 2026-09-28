using STC.Position.Plugin.Model;
using STC.Position.Plugin.ViewModel;
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

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// PositionCheckUI.xaml 的交互逻辑
    /// </summary>
    public partial class PositionCheckUI : UserControl
    {
        PositionCheckViewModel mainViewModel = null;
        public PositionCheckUI()
        {
            InitializeComponent();
            mainViewModel = new PositionCheckViewModel(this);
            this.DataContext = mainViewModel;
            mainViewModel.dtDataGrid = Trade_dataGrid;
        }

        private void positionRB_Click(object sender, RoutedEventArgs e)
        {
            dgPosition.Visibility = Visibility.Visible;
            dgDeal.Visibility = Visibility.Collapsed;
        }

        private void dealsRB_Click(object sender, RoutedEventArgs e)
        {
            dgPosition.Visibility = Visibility.Collapsed;
            dgDeal.Visibility = Visibility.Visible; 
        }

        private void positionStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox && mainViewModel != null)
            {
                ListBox listBox = sender as ListBox;
                ListBoxItem select = (ListBoxItem)listBox.SelectedValue;
                string content = select.Content + "";
                mainViewModel.selectContent = content;
                mainViewModel.seletChange();
                if (dgPosition != null && dgDeal != null)
                {
                    if ("持仓核对".Equals(content))
                    {
                        dgPosition.Visibility = Visibility.Visible;
                        dgDeal.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgPosition.Visibility = Visibility.Collapsed;
                        dgDeal.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public void SetPositionDetailSecurityId(string securityid)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //UC_Hpod.SecurityId = securityid;
            }));
        }
        public void SetPositionDetailDealTimeEnd(string dealTimeEnd)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //UC_Hpod.DealTimeEnd = dealTimeEnd;
            }));
        }

        public void SetPositionDetailQueryData()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //UC_Hpod.QueryData.Execute(string.Empty);
            }));
        }


        public void SetPositionDetailClearData()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //UC_Hpod.ClearData.Execute(string.Empty);
            }));
        }


        public void SetPositionDetailBookId(string bookid)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
               // UC_Hpod.BookId = bookid;
            }));
        }
        public void SetPositionDetailDealTimeStart(string dealtimebegin)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //UC_Hpod.DealTimeBegin = dealtimebegin;
            }));
        }
        public void ShowEMSMask(bool flag)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                spTitle.IsEnabled = !flag;
                loadDeal.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        private void OwnPosition_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void htPosition_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void checkBT_Click(object sender, RoutedEventArgs e)
        {

        }

        private void checkCBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void copyId_Click(object sender, RoutedEventArgs e)
        {
            this.Trade_dataGrid.copyId();
        }

        private void batchEnter_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.EnterClick();
        }
    }
}
