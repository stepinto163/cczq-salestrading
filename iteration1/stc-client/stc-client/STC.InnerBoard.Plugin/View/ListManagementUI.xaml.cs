using STC.Common.View.Component;
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

namespace STC.InnerBoard.Plugin.View
{
    /// <summary>
    /// ListManagementUI.xaml 的交互逻辑
    /// </summary>
    public partial class ListManagementUI : WindowBase
    {
        BrokerListView brokerListView;
        HistoryBrokerListView historyListView;
        public ListManagementUI()
        {
            InitializeComponent();
            brokerListView = new BrokerListView();
            historyListView = new HistoryBrokerListView();
            brokerFrame.Content = brokerListView;
        }

        private void BrokerList_Click(object sender, RoutedEventArgs e)
        {
            brokerFrame.Content = brokerListView;
        }

        private void HistoryList_Click(object sender, RoutedEventArgs e)
        {
            brokerFrame.Content = historyListView;
        }
    }
}
