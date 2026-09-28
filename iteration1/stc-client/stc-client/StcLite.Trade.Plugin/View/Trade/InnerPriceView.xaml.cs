using BDB.Trade.Plugin.ViewModel.Trade;
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

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// InnerPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class InnerPriceView : UserControl
    {
        private InnerPriceViewModel _innerPriceViewModel = null;

        public InnerPriceView()
        {
            InitializeComponent();
            
            _innerPriceViewModel = new InnerPriceViewModel();

            _innerPriceViewModel.Init(this);
            this.DataContext = _innerPriceViewModel;
        }
    }
}
