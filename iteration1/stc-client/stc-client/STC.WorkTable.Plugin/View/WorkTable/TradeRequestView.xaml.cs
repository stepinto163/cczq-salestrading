using STC.WorkTable.Plugin.ViewModel.WorkTable;
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

namespace STC.WorkTable.Plugin.View.WorkTable
{
    /// <summary>
    /// TradeRequestView.xaml 的交互逻辑
    /// </summary>
    public partial class TradeRequestView : UserControl
    {
        private TradeRequestViewModel tradeRequestViewModel = new TradeRequestViewModel();

        public TradeRequestView()
        {
            InitializeComponent();
            this.DataContext = tradeRequestViewModel;
        }

        private void TradeInfo_tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            working_lv.SelectedItem = ((Button)sender).DataContext;
        }

        private void Btn_PreviewMouseLeftButtonDown2(object sender, MouseButtonEventArgs e)
        {
            ignore_lv.SelectedItem = ((Button)sender).DataContext;
        }
    }
}
