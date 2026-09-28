using STC.XBond.Plugin.ViewModel;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// RiskView.xaml 的交互逻辑
    /// </summary>
    public partial class RiskView : UserControl
    {
        private RiskViewModel _riskViewModle = null;

        public RiskView()
        {
            InitializeComponent();

            //Unloaded += RiskView_Unloaded;
            
            _riskViewModle = new RiskViewModel();
            this.DataContext = _riskViewModle;

            _riskViewModle.Init(this);
        }

        private void RiskView_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (_riskViewModle != null)
            //    _riskViewModle.UnLoad();
        }
    }
}
