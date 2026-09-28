using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.quant.plugin.ViewModel;
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

namespace STC.quant.plugin.View.HomePage
{
    /// <summary>
    /// BatachAddBondTab1.xaml 的交互逻辑
    /// </summary>
    public partial class BatachAddBondTab1 : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        public AlterParamViewModel viewModel = AlterParamViewModel.Instance;
        public BatachAddBondTab1()
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.Loaded += BatachAddBondTab1_Loaded;
            viewModel.batachAddBondTab1 = this;
        }

        private void BatachAddBondTab1_Loaded(object sender, RoutedEventArgs e)
        {
           // viewModel.initGroup(false);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.Close();
            e.Handled = true;
        }

        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.CancelBondClick();
                viewModel.clearBond();
            }
        }

       
    

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
