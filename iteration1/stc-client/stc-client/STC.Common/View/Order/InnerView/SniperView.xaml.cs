using STC.Common.ViewModel.Order.InnerView;
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

namespace STC.Common.View.Order.InnerView
{
    /// <summary>
    /// SniperView.xaml 的交互逻辑
    /// </summary>
    public partial class SniperView : UserControl
    {

        private SniperViewModel sniperViewModel = new SniperViewModel();

        public SniperView()
        {
            InitializeComponent();


            this.DataContext = sniperViewModel;
        }
    }
}
