using BDB.Trade.Plugin.ViewModel.Chat;
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

namespace BDB.Trade.Plugin.View.Chat
{
    /// <summary>
    /// ChatView.xaml 的交互逻辑
    /// </summary>
    public partial class ChatView : UserControl
    {
        public ChatViewModel _chatViewModel = null;
        public ChatView()
        {
            InitializeComponent();
            _chatViewModel = new ChatViewModel(this);
            this.DataContext = _chatViewModel;
        }


        ~ChatView()
        {
            if (_chatViewModel != null)
            {
                _chatViewModel.Dispose();
            }
        }

        private void lbDirect_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_chatViewModel != null)
            {
                _chatViewModel.FilterDic = this.lbDirect.FilterDic;
            }
        }
    }
}
