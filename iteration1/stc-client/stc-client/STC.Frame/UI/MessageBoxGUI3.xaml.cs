using STC.Common.Utils;
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

namespace STC.Frame.UI
{
    /// <summary>
    /// SysWarnGUI.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxGUI3 : Window
    {
        /// <summary>
        /// 关闭
        /// </summary>
        public event Action MClosed;
        /// <summary>
        /// 添加
        /// </summary>
        public event Action Add;

        /// <summary>
        /// 预订
        /// </summary>
        public event Action Reserve;
        public MessageBoxGUI3()
        {
            InitializeComponent();
            //if (LocalEnv.ConfInfo.IsShowReserveButton)
            //{
            //    reserver.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    reserver.Visibility = Visibility.Collapsed;
            //}
        }

        public bool IsOk = false;

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void setWarnMsg(string warnMsg)
        {
            tbMsg.Text = warnMsg;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            MClosed?.Invoke();
        }



        private void reserver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Reserve?.Invoke();

        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Add?.Invoke();

        }
    }
}
