using System.Windows;
using System.Windows.Media;

namespace  STC.Broker.Plugin.Utils.wpf
{
    public class DropTargetHighlightAdorner : DropTargetAdorner
    {
        public DropTargetHighlightAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (base.DropInfo.VisualTargetItem != null)
            {
                Rect rectangle = new Rect(base.DropInfo.VisualTargetItem.TranslatePoint(default(Point), base.AdornedElement), VisualTreeHelper.GetDescendantBounds(base.DropInfo.VisualTargetItem).Size);
                drawingContext.DrawRoundedRectangle(null, new Pen(Brushes.Gray, 2.0), rectangle, 2.0, 2.0);
            }
        }
    }
}
