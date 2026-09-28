using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Shell.View
{
    /// <summary>
    /// 系统设置弹窗：偏好设置（识别行为/弹窗提醒）+ QT/IDEAL 连接状态。
    /// 开发期 Mock：偏好项不改状态，保存仅 toast；连接状态为静态样例。
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>打开弹窗（模态）。失败以 toast 呈现。</summary>
        public static void Show(Window owner)
        {
            try
            {
                var w = new SettingsWindow();
                var host = owner ?? Application.Current?.MainWindow;
                if (host != null) w.Owner = host;
                w.ShowDialog();
            }
            catch (Exception ex)
            {
                ToastService.Show("打开系统设置失败：" + ex.Message, ToastKind.Error);
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            ToastService.Show("设置已保存（Mock，接入网关后生效）", ToastKind.Success);
            Close();
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