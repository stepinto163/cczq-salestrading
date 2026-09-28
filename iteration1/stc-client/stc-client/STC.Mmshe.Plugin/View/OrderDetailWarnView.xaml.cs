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

namespace STC.Mmshe.Plugin.View
{
    /// <summary>
    /// OrderDetailWarnView.xaml 的交互逻辑
    /// </summary>
    public partial class OrderDetailWarnView : Window
    {
        public OrderDetailWarnView(Dictionary<string,string> errorList)
        {
            InitializeComponent();
            this.Icon = Common.Utils.LocalEnv.ServerInfo.AppImage;

            listview.ItemsSource = errorList;
            listview.Height = 30 * errorList.Count;
            this.Height = 100 + listview.Height;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void close_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
