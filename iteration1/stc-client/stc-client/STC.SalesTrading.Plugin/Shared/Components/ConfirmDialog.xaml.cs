using System;
using System.Windows;
using System.Windows.Input;
using STC.SalesTrading.Plugin.Shared.Services;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 深色二次确认对话框（原型 _fbmConfirm 的 WPF 复刻）。
    /// 遮罩铺满宿主窗口（<see cref="OverlayBackdrop"/>），点遮罩 / 取消 / Esc 均返回 false，确认或 Enter 返回 true。
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        private ConfirmDialog()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        /// <summary>模态弹出并返回用户选择；owner 为空时退化为居中显示。</summary>
        public static bool Show(Window owner, string message)
        {
            var dlg = new ConfirmDialog { MessageText = { Text = message } };
            if (owner != null)
            {
                dlg.Owner = owner;
            }

            // ShowDialog 前先对齐宿主，避免窗口先出现在默认位置再跳动
            OverlayBackdrop.CoverOwner(dlg);
            dlg.ShowDialog();
            return dlg._confirmed;
        }

        private bool _confirmed;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            OverlayBackdrop.CoverOwner(this);
        }

        private void OnBackdropMouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            _confirmed = true;
            Close();
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                _confirmed = true;
                Close();
                e.Handled = true;
            }
        }
    }
}
