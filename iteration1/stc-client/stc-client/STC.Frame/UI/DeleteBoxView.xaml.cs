using STC.Common.Utils;
using STC.Frame.Utils;
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
    /// DeleteBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class DeleteBoxView : Window
    {
        public DeleteBoxView()
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;
        }

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
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void sysWarnGUI_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
