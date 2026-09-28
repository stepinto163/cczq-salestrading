using BDB.Trade.Plugin.ViewModel.Trade;
using STC.Common.Model.Fix.Message;
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

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// BrokerPriceTableView.xaml 的交互逻辑
    /// </summary>
    public partial class BrokerPriceTableView : UserControl
    {
        private BrokerPriceTableViewModel _brokerPriceTableViewModel = null;

        public BrokerPriceTableView()
        {
            InitializeComponent();

            _brokerPriceTableViewModel = new BrokerPriceTableViewModel();
            this.DataContext = _brokerPriceTableViewModel;
        }

        public void ClearData(string broker)
        {
            _brokerPriceTableViewModel.SetCurrentBroker(broker);
            _brokerPriceTableViewModel.ClearUI();
            
        }

        public void RefreshData(string broker, List<GNoMDEntries> bidNoMDEntryList, List<GNoMDEntries> ofrNoMDEntryList)
        {
            _brokerPriceTableViewModel.SetCurrentBroker(broker);
            _brokerPriceTableViewModel.RefreshData(broker, bidNoMDEntryList, ofrNoMDEntryList);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
