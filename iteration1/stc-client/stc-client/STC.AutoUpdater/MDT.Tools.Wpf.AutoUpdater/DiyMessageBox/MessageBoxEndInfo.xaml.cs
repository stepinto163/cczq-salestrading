using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MDT.Tools.Wpf.AutoUpdater.DiyMessageBox
{
    /// <summary>
    /// MessageBoxEndInfo.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxEndInfo : Window
    {
        public MessageBoxEndInfo()
        {
            InitializeComponent();
            this.KeyDown += MessageBoxEndInfo_KeyDown;
        }

        private void MessageBoxEndInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)//Esc键  
            {
                this.Close();
            }
        }

        public ResultBox resultBox = ResultBox.Ok;
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            cloce();
        }

        private void cloce()
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.cloce();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
