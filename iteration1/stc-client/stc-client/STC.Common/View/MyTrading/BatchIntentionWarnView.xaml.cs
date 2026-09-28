using STC.Common.Model;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.ViewModel.MyTrading;
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

namespace STC.Common.View.MyTrading
{
    /// <summary>
    /// BatchWarnView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchIntentionWarnView : Window
    {
        BatchIntentionWarnViewModel _batchIntentionWarnViewModel = new BatchIntentionWarnViewModel();
        public BatchIntentionWarnView(Dictionary<string, List<TPlaceOrderResult>> replyDic, Dictionary<string, IntentionQuoteItem> orderDic, bool isDoubleWarn = false)
        {
            InitializeComponent();
            this.Icon = Common.Utils.LocalEnv.ServerInfo.AppImage;
            this.KeyDown += ModifyPrice_KeyDown;

            this.DataContext = _batchIntentionWarnViewModel;
            _batchIntentionWarnViewModel.Gray20 = (SolidColorBrush)this.FindResource("ColorBrush.Gray20");
            _batchIntentionWarnViewModel.Gray18 = (SolidColorBrush)this.FindResource("ColorBrush.Gray18");
            _batchIntentionWarnViewModel.Gray13 = (SolidColorBrush)this.FindResource("ColorBrush.Gray13");
            _batchIntentionWarnViewModel.Red7 = (SolidColorBrush)this.FindResource("ColorBrush.Red7");
            _batchIntentionWarnViewModel.Red14 = (SolidColorBrush)this.FindResource("ColorBrush.Red14");
            _batchIntentionWarnViewModel.Red15 = (SolidColorBrush)this.FindResource("ColorBrush.Red15");
            _batchIntentionWarnViewModel.Init(replyDic, orderDic, isDoubleWarn);
        }

        private void ModifyPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)//Esc键  
            {
                _batchIntentionWarnViewModel.Close(this);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _batchIntentionWarnViewModel.Close(this);
        }


    }
}
