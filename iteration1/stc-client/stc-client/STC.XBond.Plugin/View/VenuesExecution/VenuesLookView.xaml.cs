using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
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
using System.Windows.Shapes;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// VenuesLookView.xaml 的交互逻辑
    /// </summary>
    public partial class VenuesLookView : Window
    {
        private VenuesLookViewModel _venuesLookViewModel = null;

        public VenuesLookView()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        public VenuesLookView(ExecutionReport order)
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;
            _venuesLookViewModel = new VenuesLookViewModel();
            this.DataContext = _venuesLookViewModel;

            _venuesLookViewModel.Init(this, order);

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
