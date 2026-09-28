using STC.Common.View.Component;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.ViewModel;
using System;
using System.Collections;
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

namespace STC.PT.Plugin.View
{
    /// <summary>
    /// BatchUpdateDetailsView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchUpdateDetailsView : WindowBase
    {
        public BatchManualEntry manualEntry;
        private BatchUpdateDetailsViewModel viewModel = new BatchUpdateDetailsViewModel();
        public BatchUpdateDetailsView()
        {
            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.RequestCloseAfterSubmit += (sender, e) => 
            { 
                this.Close();
                manualEntry.Close();
            };
        }
        public void setData(IList selectedItemsList, PTUpdatePo po,BatchManualEntry manualEntry) 
        {
            this.manualEntry = manualEntry;
            if (viewModel != null) 
            {
                viewModel.setData(selectedItemsList,po);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }

        private void dg_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //e.Handled = true;
        }
    }
}
