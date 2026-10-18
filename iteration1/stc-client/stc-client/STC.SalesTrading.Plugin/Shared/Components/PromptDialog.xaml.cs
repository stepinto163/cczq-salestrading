using System.Windows;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 深色名称输入对话框（原型 showNewGroupModal 的 WPF 复刻）：普通模态窗口（无遮罩浮层），
    /// 取消 / ✕ / Esc 返回 null，确定或 Enter 返回输入文本。
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

            dlg.ShowDialog();
            return dlg._confirmed ? (dlg.InputBox.Text ?? string.Empty).Trim() : null;
        }

        private bool _confirmed;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InputBox.Focus();
            InputBox.CaretIndex = InputBox.Text.Length;
        }

        // ============ 标题栏 / 关闭（普通弹窗，与 QuoteEntryDialog 同款） ============

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            try { DragMove(); }
            catch (System.Exception) { }
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
