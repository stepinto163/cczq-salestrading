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

namespace STC.PT.Plugin.View
{
    /// <summary>
    /// PostTradeGUI.xaml 的交互逻辑
    /// </summary>
    public partial class PostTradeGUI : UserControl
    {
        PTInnerView innerView;
        public PostTradeGUI()
        {
            InitializeComponent();
            innerView = new PTInnerView();
            PTFrame.Content = innerView;
        }

        private void Overview_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InternalTransaction_Click(object sender, RoutedEventArgs e)
        {
            //PTFrame.Content = innerView;
        }

        private void ExternalTransaction_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OrderInquiry_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
