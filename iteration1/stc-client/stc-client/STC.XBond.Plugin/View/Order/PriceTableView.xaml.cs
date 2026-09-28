using STC.XBond.Plugin.ViewModel.Order;
using System;
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

namespace STC.XBond.Plugin.View.Order
{
    /// <summary>
    /// PriceTableView.xaml 的交互逻辑
    /// </summary>
    public partial class PriceTableView : UserControl
    {
        private PriceTableViewModel _priceTableViewModel = null;
        
        public PriceTableView()
        {
            InitializeComponent();

            _priceTableViewModel = new PriceTableViewModel();
            _priceTableViewModel.SetOwnDataGrid(dgPrice);
            this.DataContext = _priceTableViewModel;
        }

        private void CheckBox_All_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DataGrid_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Selected_Chk_Checked(object sender, RoutedEventArgs e)
        {
            lock (_priceTableViewModel.PriceTableLock)
            {
                CheckBox checkBox = sender as CheckBox;

                DataRowView dataRow = checkBox.DataContext as DataRowView;

                if (dataRow != null)
                {
                    string key = dataRow[PriceTableViewModel.KEY].ToString();
                    DataTable dataTable = _priceTableViewModel.PriceTableDV.ToTable();

                    int index = -1;
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        index++;
                        if (dr[PriceTableViewModel.KEY].ToString() == key)
                        {
                            break;
                        }
                    }

                    if (index >= 0)
                    {
                        DataGridRow row = (DataGridRow)dgPrice.ItemContainerGenerator.ContainerFromIndex(index);

                        if (row != null)
                        {
                            row.Background = this.FindResource("ColorBrush.Green3") as Brush;
                        }
                    }
                }
            }
        }
    }
}
