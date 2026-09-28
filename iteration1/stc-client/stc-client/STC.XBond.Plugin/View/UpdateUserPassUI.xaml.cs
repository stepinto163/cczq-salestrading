using STC.XBond.Plugin.ViewModel;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// UpdateUserPassUI.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateUserPassUI : Window
    {
        private UpdateUserPassViewModel _viewModel = new UpdateUserPassViewModel();
        public UpdateUserPassUI()
        {
            InitializeComponent();
            this.Icon = Common.Utils.LocalEnv.ServerInfo.AppImage;
            this.DataContext = _viewModel;
            _viewModel.init(this);
            this.PreviewKeyDown += new KeyEventHandler(CloseOnEscape);
        }

        private void CloseOnEscape(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
        private void tbWeight_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            bool status = _viewModel.btOK(tbPassword.Password,newPassword.Password,confirmPassword.Password);
            if (status)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(2000);

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() =>
                        {
                            this.Close();
                        }));
                });
            }
            //else
            //{
            //    this.Activate();
            //    //this.ShowDialog();
            //}
            
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();


                }
                catch { }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
