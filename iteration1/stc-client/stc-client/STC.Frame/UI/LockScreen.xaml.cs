using com.gateway.log;
using STC.Common.Data;
using STC.Common.Utils;
using STC.Frame.Common;
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

namespace STC.Frame.UI
{

    /// <summary>
    /// LockScreen.xaml 的交互逻辑
    /// </summary>
    public partial class LockScreen : Window
    {
        Hook hook;
        bool isVisible = true;
        public LockScreen(bool visible)
        {
            InitializeComponent();
            DesignHandler.Instance.OnScreenUnlock += Instance_OnScreenUnlock;
            this.isVisible = false;
            this.MainGrid.Visibility = Visibility.Hidden;
        }

        public LockScreen()
        {
            InitializeComponent();
            isVisible = true;
            tbUser.Text = LocalEnv.UserInfo.UserName;
            tbUser.IsEnabled = false;
            hook = new Hook();
            DesignHandler.Instance.OnScreenUnlock += Instance_OnScreenUnlock;
        }

        private void Instance_OnScreenUnlock(object obj)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                DesignHandler.Instance.OnScreenUnlock -= Instance_OnScreenUnlock;
                if (isVisible)
                {
                    hook.Hook_Clear();
                }
                //关闭窗体
                this.Close();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbUser.Text) || string.IsNullOrWhiteSpace(tbPwd.Password))
            {
                return;
            }

            if (tbUser.Text.Trim() != LocalEnv.UserInfo.UserName)
            {
                tbUser.Focus();
                LogHelper.Info("{0}", "用户名或密码错误");
                ErrMsg.Foreground = new SolidColorBrush(Colors.Red);
                ErrMsg.Text = "用户名或密码错误";
                return;
            }

            if (tbPwd.Password.Trim() != LocalEnv.UserInfo.Password)
            {
                tbPwd.Password = string.Empty;
                tbPwd.Focus();
                LogHelper.Info("{0}", "用户名或密码错误");
                ErrMsg.Foreground = new SolidColorBrush(Colors.Red);
                ErrMsg.Text = "用户名或密码错误";
                return;
            }
            else
            {
                // 密码正确，开始解锁
                LogHelper.Info("{0}", "解锁成功");
                ErrMsg.Text = "";
                DesignHandler.Instance.IsScreenUnlock(new object());
            }
        }

        private void TbUser_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(tbUser.Text))
                    tbPwd.Focus();
            }
        }

        private void TbPwd_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(tbPwd.Password))
                    Button_Click(null, null);
            }

            //if (tbPwd.Password.Length == 0)
            //{
            //    ErrMsg.Text = "* 请输入密码";
            //    ErrMsg.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            //}
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (isVisible)
            {
                hook.Hook_Start();
                tbPwd.Focus();
            }
        }
    }
}
