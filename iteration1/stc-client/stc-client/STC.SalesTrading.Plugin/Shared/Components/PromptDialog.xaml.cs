using System;
using System.Windows;
using System.Windows.Input;
using STC.SalesTrading.Plugin.Shared.Services;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 深色名称输入对话框（原型 showNewGroupModal 的 WPF 复刻）。
    /// 遮罩铺满宿主窗口（<see cref="OverlayBackdrop"/>），点遮罩 / 取消 / Esc 返回 null，确定或 Enter 返回输入文本。
    /// </summary>
    public partial class PromptDialog : Window
    {
        private PromptDialog()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        /// <summary>模态弹出；owner 为空时退化为居中显示。返回 trim 后的文本，取消返回 null。</summary>
        public static string Show(Window owner, string title, string confirmText = "确定")
        {
            var dlg = new PromptDialog
            {
                Title = title,
                TitleText = { Text = title },
            };
            dlg.ConfirmButton.Content = confirmText;
            if (owner != null)
            {
                dlg.Owner = owner;
            }

            OverlayBackdrop.CoverOwner(dlg);
            dlg.ShowDialog();
            return dlg._confirmed ? (dlg.InputBox.Text ?? string.Empty).Trim() : null;
        }

        private bool _confirmed;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            OverlayBackdrop.CoverOwner(this);
            InputBox.Focus();
            InputBox.CaretIndex = InputBox.Text.Length;
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
            Confirm();
        }

        private void Confirm()
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
                Confirm();
                e.Handled = true;
            }
        }
    }
}
