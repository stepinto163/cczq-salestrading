using STC.Common.View.Component;
using STC.PT.Plugin.ViewModel;
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

namespace STC.PT.Plugin.View
{
    /// <summary>
    /// BatchManualSendView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchManualSendView : WindowBase
    {
        private BatchManualSendViewModel viewModel = null;
        public BatchManualSendView()
        {
            InitializeComponent();
            if(viewModel == null)
            {
                viewModel = new BatchManualSendViewModel();
            }
            this.DataContext = viewModel;
            this.Loaded += BatchManualSendView_Loaded;
            viewModel.RequestCloseAfterSubmit += (sender, e) =>
            {
                this.Close();
            };
        }
        public void setSales(bool isSales) 
        {
            viewModel.isSales = isSales;
        }
        private void BatchManualSendView_Loaded(object sender, RoutedEventArgs e)
        {
            if (viewModel != null) 
            {
                viewModel.loadData();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
