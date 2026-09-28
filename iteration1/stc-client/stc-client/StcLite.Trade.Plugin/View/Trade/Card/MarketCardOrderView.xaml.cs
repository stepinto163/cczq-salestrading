using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
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

namespace BDB.Trade.Plugin.View.Trade.Card
{
    /// <summary>
    /// MarketCardOrderView.xaml 的交互逻辑
    /// </summary>
    public partial class MarketCardOrderView : UserControl
    {
        private MarketCardOrderViewModel _marketCardOrderViewModel = null;

        public MarketCardOrderView()
        {
            InitializeComponent();

            this.Unloaded += MarketCardOrderView_Unloaded;

            _marketCardOrderViewModel = new MarketCardOrderViewModel();
            this.DataContext = _marketCardOrderViewModel;
        }

        private void MarketCardOrderView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= MarketCardOrderView_Unloaded;

            if (_marketCardOrderViewModel != null && _marketCardOrderViewModel.ShowModel == TabShowModel.Card)
                _marketCardOrderViewModel.Dispose();
        }

        public void Init(TabShowModel showModel, string security)
        {
            
            _marketCardOrderViewModel.ShowModel = showModel;
            _marketCardOrderViewModel.SearchText = security;

            _marketCardOrderViewModel.Init();

        }

        /// <summary>
        /// 鼠标滚轮移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_marketCardOrderViewModel != null)
            {
                if (e.Delta > 0)
                {
                    //正在向上滚动滑轮
                    _marketCardOrderViewModel.MouseWheel(1);
                }
                else
                {
                    // 正在向下滚动滑轮
                    _marketCardOrderViewModel.MouseWheel(0);
                }

                e.Handled = true;
            }
        }
    }
}
