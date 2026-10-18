using System.Windows;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 深色二次确认对话框（原型 _fbmConfirm 的 WPF 复刻）：普通模态窗口（无遮罩浮层），
    /// 取消 / ✕ / Esc 返回 false，确认或 Enter 返回 true。
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        private ConfirmDialog()
        {
            InitializeComponent();
        }

        /// <summary>模态弹出并返回用户选择；owner 为空时退化为居中显示。</summary>
        public static bool Show(Window owner, string message)
        {
            var dlg = new ConfirmDialog { MessageText = { Text = message } };
            if (owner != null)
            {
                dlg.Owner = owner;
            }

            dlg.ShowDialog();
            return dlg._confirmed;
        }

        private bool _confirmed;

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
