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
    /// MessageBoxCloseApp.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxCloseApp : Window
    {
        public MessageBoxCloseApp()
        {
            InitializeComponent();
            this.uiTxtinfo.Text = string.Format("进程 {0} 正在运行,请确定是否要关闭!", ConstFile.AppExe);
            this.KeyDown += MessageBoxCloseApp_KeyDown;
        }

        private void MessageBoxCloseApp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Y)//Y键  
            {
                btnOk_Click(null, null);
            }
            else if (e.Key == Key.N)
            {
                btnCancel_Click(null, null);
            }
        }

        public ResultBox resultBox;
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            resultBox = ResultBox.Ok;
            cloce();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            resultBox = ResultBox.Cancel;
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
