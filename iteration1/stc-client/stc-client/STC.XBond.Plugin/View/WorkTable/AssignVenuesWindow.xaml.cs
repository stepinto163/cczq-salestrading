using STC.XBond.Plugin.ViewModel.WorkTable;
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

namespace STC.XBond.Plugin.View.WorkTable
{
    /// <summary>
    /// AssignVenuesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AssignVenuesWindow : Window
    {
        public string BNCVenues = string.Empty;
        public string OBCVenues = string.Empty;

        AssignVenuesWindowViewModel m_assignVenuesWindowViewModel = new AssignVenuesWindowViewModel();
        public AssignVenuesWindow(bool isfirm)
        {
            
            InitializeComponent();
            if (!isfirm)
            {
                InterestRateDebt.isfirm = false;
            }
            this.DataContext = m_assignVenuesWindowViewModel;
            
           
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
