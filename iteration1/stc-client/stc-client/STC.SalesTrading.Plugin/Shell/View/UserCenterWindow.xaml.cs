using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Shell.View
{
    /// <summary>
    /// 个人中心弹窗：资料展示（头像/姓名/角色/部门/邮箱/手机）+ 修改密码表单（同窗内切换）。
    /// 开发期 Mock：资料来自注入的 UserProfile；密码修改校验通过后仅 toast，不落库。
    /// </summary>
    public partial class UserCenterWindow : Window
    {
        public UserCenterWindow(UserProfile profile)
        {
            InitializeComponent();
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            DataContext = profile;
        }

        /// <summary>打开弹窗（模态）。失败以 toast 呈现。</summary>
        public static void Show(Window owner, UserProfile profile)
        {
            try
            {
                var w = new UserCenterWindow(profile);
                var host = owner ?? Application.Current?.MainWindow;
                if (host != null) w.Owner = host;
                w.ShowDialog();
            }
            catch (Exception ex)
            {
                ToastService.Show("打开个人中心失败：" + ex.Message, ToastKind.Error);
            }
        }

        private void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "修改密码";
            ProfilePanel.Visibility = Visibility.Collapsed;
            PasswordPanel.Visibility = Visibility.Visible;
            ResetPasswordForm();
        }

        private void OnCancelPasswordClick(object sender, RoutedEventArgs e)
        {
            SwitchToProfile();
        }

        private void OnConfirmPasswordClick(object sender, RoutedEventArgs e)
        {
            var newPassword = NewPassword.Password ?? string.Empty;
            var confirm = ConfirmPassword.Password ?? string.Empty;

            if (newPassword.Length < 8)
            {
                ErrorText.Text = "新密码至少需要8位";
                return;
            }
            if (!string.Equals(newPassword, confirm, StringComparison.Ordinal))
            {
                ErrorText.Text = "两次输入的新密码不一致";
                return;
            }

            // Mock 提交（无后端），成功后回到资料视图
            ToastService.Show("密码修改成功（Mock，接入网关后生效）", ToastKind.Success);
            SwitchToProfile();
        }

        private void ResetPasswordForm()
        {
            OldPassword.Password = string.Empty;
            NewPassword.Password = string.Empty;
            ConfirmPassword.Password = string.Empty;
            ErrorText.Text = string.Empty;
        }

        private void SwitchToProfile()
        {
            TitleText.Text = "个人中心";
            PasswordPanel.Visibility = Visibility.Collapsed;
            ProfilePanel.Visibility = Visibility.Visible;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); }
                catch (Exception) { }
            }
        }
    }
}