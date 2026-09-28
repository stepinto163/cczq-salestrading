using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
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
//using System.Windows.Threading;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class XBondView : UserControl, IContentInterface
    {
        public XBondView()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            if (this.bondList != null && this.bondList.DataContext is BondListViewModel)
            {
                ((BondListViewModel)this.bondList.DataContext).Clear();
            }
            if (this.myTrading != null && this.myTrading.DataContext is MyTraderViewModel)
            {
                ((MyTraderViewModel)this.myTrading.DataContext).Dispose();
            }
            // marketExecution
            //if (this.myTrading != null && this.myTrading.Content.marketExecution)
        }
    }
}
