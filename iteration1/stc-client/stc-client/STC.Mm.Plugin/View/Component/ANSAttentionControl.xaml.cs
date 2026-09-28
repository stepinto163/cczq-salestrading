using STC.Mm.Plugin.ViewModel;
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

namespace STC.Mm.Plugin.View.Component
{
    /// <summary>
    /// ANSAttentionControl.xaml 的交互逻辑
    /// </summary>
    public partial class ANSAttentionControl : UserControl
    {
        public ANSAttentionControl()
        {
            InitializeComponent();

            this.newBond_dg.SelectedIndex = -1;
            this.otherBond_dg.SelectedIndex = -1;
            this.bondType_dg.SelectedIndex = -1;
        }

        private void AnsCotrol_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext == null || this.DataContext is MainViewModel) { return; }

            ANSSummaryProp currentItem = this.DataContext as ANSSummaryProp;
            if (currentItem.OtherBondList.View == null || currentItem.securityTypeLt == null)
            {
                return;
            }

            int otherBondCount = currentItem.OtherBondList.View.Cast<object>().Count();

            if (otherBondCount <= 6 && currentItem.securityTypeLt.Count <=6)
            {
                AnsCotrol.Width = 795;
            }
            else if (otherBondCount > 6 && currentItem.securityTypeLt.Count > 6)
            {
                AnsCotrol.Width = 822;
            }
            else
            {
                AnsCotrol.Width = 810;
            }
        }
    }
}
