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
    public partial class MessageBoxGUI2 : Window
    {
        public event Action MClosed;
        public event Action MOK;
        public MessageBoxGUI2()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
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
            IsOk = false;
            MClosed?.Invoke();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            IsOk = true;
            MOK?.Invoke();
        }
    }
}
