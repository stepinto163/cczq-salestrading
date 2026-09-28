using STC.Common.Command;
using STC.Common.DataProcesser;
using STC.Common.Utils;
using STC.quant.plugin.View.Component;
using STC.quant.plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace STC.quant.plugin.View
{
    /// <summary>
    /// MyTrading.xaml 的交互逻辑
    /// </summary>
    public partial class MyTrading : UserControl
    {
       public MyTraderViewModel _myTraderViewModel = new MyTraderViewModel();

        public MyTrading()
        {
            InitializeComponent();
            _myTraderViewModel.myWindow = this; 
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _myTraderViewModel;
            //_myTraderViewModel.Init();
           // _myTraderViewModel.IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
            //SetMyOrderOperationGridMargin();
        }

  
    }
}
