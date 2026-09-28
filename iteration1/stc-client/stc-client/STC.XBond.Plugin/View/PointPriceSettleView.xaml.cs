using STC.Common.Data;
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
    /// PointPriceSettleView.xaml 的交互逻辑
    /// </summary>
    public partial class PointPriceSettleView : Window
    {
        private PointPriceSettleViewModel _pointPriceSettleViewModel = null;

        public PointPriceSettleView()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        public PointPriceSettleView(string priceInfo, string settle, PriceMatchParam priceMatchParam)
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
            _pointPriceSettleViewModel = new PointPriceSettleViewModel();
            this.DataContext = _pointPriceSettleViewModel;
            _pointPriceSettleViewModel.Init(this, priceInfo, settle, priceMatchParam);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        
    }
}
