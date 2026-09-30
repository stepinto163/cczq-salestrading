using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.View
{
    /// <summary>精确报价页：可拖拽输入区 + 识别 + 19 列预览表。</summary>
    public partial class PreciseQuoteView : UserControl
    {
        public PreciseQuoteView()
        {
            InitializeComponent();
        }

        /// <summary>输入区底部手柄纵向拖拽（原型 preciseResizeHandle，100–500px 由 VM 钳制）。</summary>
        private void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var vm = DataContext as PreciseQuoteViewModel;
            if (vm == null) return;
            vm.InputHeight += e.VerticalChange;
        }

        /// <summary>Enter 换行；Ctrl+Enter 识别。</summary>
        private void InputBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var vm = DataContext as PreciseQuoteViewModel;
            if (vm == null) return;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                // Enter 和 Shift+Enter 保持 TextBox 默认换行行为。
                return;
            }

            e.Handled = true;
            if (vm.IdentifyCommand.CanExecute(null))
            {
                vm.IdentifyCommand.Execute(null);
            }
        }
    }
}
