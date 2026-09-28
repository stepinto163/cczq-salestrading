using STC.XBond.Plugin.ViewModel.Risk;
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

namespace STC.XBond.Plugin.View.Risk
{
    /// <summary>
    /// RiskIndexView.xaml 的交互逻辑
    /// </summary>
    public partial class RiskIndexView : UserControl
    {
        private RiskIndexViewModel _riskIndexViewModel = null;

        public RiskIndexView()
        {
            InitializeComponent();

            _riskIndexViewModel = new RiskIndexViewModel();
            
            _riskIndexViewModel.Init(this);
            this.DataContext = _riskIndexViewModel;

            this.Unloaded += RiskIndexView_Unloaded;
        }

        private void RiskIndexView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= RiskIndexView_Unloaded;

            if (_riskIndexViewModel != null)
                _riskIndexViewModel.Clear();
        }
        
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cbUser.IsDropDownOpen = !cbUser.IsDropDownOpen;
        }

        private void cbUserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _riskIndexViewModel.ChangeSubscribe();
        }
    }
}
