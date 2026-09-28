//using STC.Common.Utils;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace STC.Frame.UI
{
    /// <summary>
    /// SysWarnGUI.xaml 的交互逻辑
    /// </summary>
    public partial class SysWarnGUI : Window
    {
        public event Action MClosed;
        public SysWarnGUI()
        {
            InitializeComponent();

            this.Closed += SysWarnGUI_Closed;
            this.KeyDown += SysWarnGUI_KeyDown;
            //this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        private void SysWarnGUI_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Button_Click(null, null);
        }

        private void SysWarnGUI_Closed(object sender, EventArgs e)
        {
            this.Closed -= SysWarnGUI_Closed;
            this.KeyDown -= SysWarnGUI_KeyDown;
            Button_Click(null, null);
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
            //this.Hide();
            this.Close();
            IsOk = true;
            MClosed?.Invoke();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
