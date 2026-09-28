using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace STC.Broker.Plugin.Utils
{
    public class DragDropAdorner : Adorner
    {
        public DragDropAdorner(UIElement parent)
            : base(parent)
        {
            IsHitTestVisible = false;
            mDraggedElement = parent as FrameworkElement;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (mDraggedElement != null)
            {
                Win32.POINT screenPos = new Win32.POINT();
                if (Win32.GetCursorPos(ref screenPos))
                {
                    Point pos = PointFromScreen(new Point(screenPos.X, screenPos.Y));
                    Rect rect = new Rect(pos.X, pos.Y, mDraggedElement.ActualWidth, mDraggedElement.ActualHeight);
                    drawingContext.PushOpacity(0.5);
                    Brush highlight = mDraggedElement.TryFindResource(SystemColors.HighlightBrushKey) as Brush;
                    if (highlight != null)
                        drawingContext.DrawRectangle(highlight, new Pen(Brushes.Red, 0), rect);
                    drawingContext.DrawRectangle(new VisualBrush(mDraggedElement),
                        new Pen(Brushes.Transparent, 0), rect);
                    drawingContext.Pop();
                }
            }
        }

        FrameworkElement mDraggedElement = null;
    }

    public static class Win32
    {
        public struct POINT { public Int32 X; public Int32 Y; }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref POINT point);
    }
}
