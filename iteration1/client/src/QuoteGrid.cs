using System.Diagnostics;
using System.Windows.Forms;

namespace Ficc.SalesTrading {
    internal sealed class QuoteGrid : DataGridView {
        internal QuoteGrid() {
            // Compose cells in memory before presenting the grid to the window.
            DoubleBuffered = true;
        }

        internal double LastPaintMilliseconds { get; private set; }

        protected override void WndProc(ref Message message) {
            const int paintMessage = 0x000F;
            if (message.Msg != paintMessage) {
                base.WndProc(ref message);
                return;
            }
            var stopwatch = Stopwatch.StartNew();
            base.WndProc(ref message);
            LastPaintMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
