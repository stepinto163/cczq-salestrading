using STC.Common.Data;
using STC.XBond.Plugin.ViewModel.Setting;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace STC.XBond.Plugin.View.Setting
{
    /// <summary>
    /// UpdateUserPwdView.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateUserPwdView : Window
    {
        private UpdateUserPwdViewModel _updateUserPwdViewModel = new UpdateUserPwdViewModel();
        private bool expire = false;

        public UpdateUserPwdView()
        {
            InitializeComponent();
            this.Icon = Common.Utils.LocalEnv.ServerInfo.AppImage;

            this.DataContext = _updateUserPwdViewModel;
            _updateUserPwdViewModel.Init(this);
        }

        public void SetDefaultUpdateDay(string updateDay,bool IspowerPwd,bool expire)
        {
            _updateUserPwdViewModel.UpdatePwdDay = updateDay;
            _updateUserPwdViewModel.IspowerPwd = IspowerPwd;
            this.expire = expire;
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (expire)
            {
                DesignHandler.Instance.SystemExit(true);
            }
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                bool status = _updateUserPwdViewModel.Update(pbOld.Password, pbNew.Password, pbNewCopy.Password);
                if (status)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() =>
                        {
                            this.Close();
                        }));
                    if (expire)
                    {
                        DesignHandler.Instance.SystemExit(true);
                    }
                }
            });
        }

        private void tbWeight_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
