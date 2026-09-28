using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;


namespace STC.Common.View.StrongNotify
{
    public class PanleDragDropAdorner : Adorner
    {
        public PanleDragDropAdorner(UIElement parent)
            : base(parent)
        {
            IsHitTestVisible = false; // Seems Adorner is hit test visible?
            mDraggedElement = parent as FrameworkElement;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            try
            {
                if (mDraggedElement != null)
                {
                    Win32.POINT screenPos = new Win32.POINT();
                    if (Win32.GetCursorPos(ref screenPos))
                    {
                        StrongNotifyMainWindow w = StrongNotifyHandler.Instance.GetStrongNotifyMainWindow();

                        // 获取所有屏幕的 Bounds
                        //var screenBounds = Screen.AllScreens.Select(screen => screen.Bounds);

                        // 找到最小的 x 坐标，最小的 y 坐标，最大的 x 坐标和最大的 y 坐标
                        //int minX = screenBounds.Min(b => b.Left);
                        //int minY = screenBounds.Min(b => b.Top);
                        //int maxX = screenBounds.Max(b => b.Right);
                        //int maxY = screenBounds.Max(b => b.Bottom);

                        // 创建一个覆盖所有屏幕的矩形
                        //Rect rect = new Rect(minX, minY, maxX - minX, maxY - minY);

                        Rect rect = new Rect(0, 0, w.ActualWidth, w.ActualHeight);

                        drawingContext.PushOpacity(0.01);
                        Brush highlight = mDraggedElement.TryFindResource(SystemColors.HighlightBrushKey) as Brush;
                        if (highlight != null)
                            drawingContext.DrawRectangle(highlight, new Pen(Brushes.Transparent, 0), rect);
                        drawingContext.DrawRectangle(new VisualBrush(mDraggedElement),
                            new Pen(Brushes.Transparent, 2), rect);
                    }
                }
            }
            catch { }
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
