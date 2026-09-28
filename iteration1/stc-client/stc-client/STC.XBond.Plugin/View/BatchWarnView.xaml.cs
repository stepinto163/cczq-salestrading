using STC.Common.Model.TBook;
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
    /// BatchWarnView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchWarnView : Window
    {
        BatchWarnViewModel _batchWarnViewModel = new BatchWarnViewModel();
        public BatchWarnView(Dictionary<string, List<TPlaceOrderResult>> replyDic, Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> orderDic, bool isDoubleWarn = false)
        {
            InitializeComponent();
            this.Icon = Common.Utils.LocalEnv.ServerInfo.AppImage;
            this.KeyDown += ModifyPrice_KeyDown;

            this.DataContext = _batchWarnViewModel;
            _batchWarnViewModel.Gray20 = (SolidColorBrush)this.FindResource("ColorBrush.Gray20");
            _batchWarnViewModel.Gray18 = (SolidColorBrush)this.FindResource("ColorBrush.Gray18");
            _batchWarnViewModel.Gray13 = (SolidColorBrush)this.FindResource("ColorBrush.Gray13");
            _batchWarnViewModel.Red7 = (SolidColorBrush)this.FindResource("ColorBrush.Red7");
            _batchWarnViewModel.Red14 = (SolidColorBrush)this.FindResource("ColorBrush.Red14");
            _batchWarnViewModel.Red15 = (SolidColorBrush)this.FindResource("ColorBrush.Red15");
            _batchWarnViewModel.Init(replyDic, orderDic, isDoubleWarn);
        }

        private void ModifyPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)//Esc键  
            {
                this.Close();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
