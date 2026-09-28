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
    /// OpenFormGUI.xaml 的交互逻辑
    /// </summary>
    public partial class OpenFormGUI : Window
    {
        public OpenFormGUI()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public UIElement CustomContent { get { return bContent.Child; } set { bContent.Child = value; } }
        public string CustomTitle { get { return tbTitle.Text; } set { tbTitle.Text = value; } }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.Owner != null)
            {
                this.Owner.Focus();
            }
        }
    }
}
