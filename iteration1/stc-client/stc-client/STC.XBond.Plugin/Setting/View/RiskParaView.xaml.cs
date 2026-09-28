using STC.XBond.Plugin.Setting.ViewModel;
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

namespace STC.XBond.Plugin.Setting.View
{
    /// <summary>
    /// RiskParaView.xaml 的交互逻辑
    /// </summary>
    public partial class RiskParaView : UserControl
    {
        //private RiskParaViewModel _riskParaViewModel = new RiskParaViewModel();
        public RiskParaView()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            //this.DataContext = _riskParaViewModel;
            //this._riskParaViewModel.init();
        }

        private void riskTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl s = sender as TabControl;
            if (s != null)
            {
                foreach (TabItem t in s.Items)
                {
                    t.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                }
                TabItem d = s.SelectedItem as TabItem;
                if (d.IsSelected)
                    d.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
