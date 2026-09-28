using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace STC.Common.View.UI
{
    /// <summary>
    /// SysWarnGUI.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxGUI : Window
    {
        public event Action MClosed;
        public event Action MOK;
        public event Action<object> OnOk;
        public object Data;
        public MessageBoxGUI()
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

        public void setTitle(string warnMsg)
        {
            this.Title = warnMsg;

        }

        public void setWarnMsg(string warnMsg, bool autoClose)
        {
            tbMsg.Text = warnMsg;

            if (autoClose)
            {
                btnCloseB.Visibility = btnOK.Visibility = Visibility.Collapsed;
                ThreadPool.QueueUserWorkItem(o => {
                    Thread.Sleep(2000);

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() => {
                            this.Close();
                        }));
                });
            }
        }

        public void HideCancelButton()
        {
            btnCloseB.Visibility = Visibility.Hidden;
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
            OnOk?.Invoke(Data);
        }

        private void sysWarnGUI_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.dpMain.ActualHeight > 160)
            {
                this.Height = this.dpMain.ActualHeight + 60;
            }
        }
    }
}
