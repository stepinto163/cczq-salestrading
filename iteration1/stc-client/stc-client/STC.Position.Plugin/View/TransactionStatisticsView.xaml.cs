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
    /// TransactionStatisticsView.xaml 的交互逻辑
    /// </summary>
    public partial class TransactionStatisticsView : UserControl
    {
        TransactionStatisticsViewModel mainViewModel;
        public TransactionStatisticsView()
        {
            InitializeComponent();
            mainViewModel = new TransactionStatisticsViewModel();
            this.DataContext = mainViewModel;
        }

        private void TreeView_NodeList_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            mainViewModel.SelectedItemNew = (TreeView_NodeList.SelectedItem ?? "").ToString();
        }

        //成交统计按字段排序
        private void dgDetailStatistic_Sorting(object sender, DataGridSortingEventArgs e)
        {
            (this.DataContext as TransactionStatisticsViewModel).SortStatistic(e.Column.SortMemberPath, e.Column.SortDirection.ToString());
        }
    }
}
