using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace  STC.Broker.Plugin.Utils.wpf
{
    public class DropTargetInsertionAdorner : DropTargetAdorner
    {
        private static Pen m_Pen;

        private static PathGeometry m_Triangle;

        public DropTargetInsertionAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            ItemsControl itemsControl = base.DropInfo.VisualTarget as ItemsControl;
            if (itemsControl == null)
            {
                return;
            }

            ItemsControl itemsControl2 = (base.DropInfo.VisualTargetItem == null) ? itemsControl : ItemsControl.ItemsControlFromItemContainer(base.DropInfo.VisualTargetItem);
            int index = Math.Min(base.DropInfo.InsertIndex, itemsControl2.Items.Count - 1);
            UIElement uIElement = (UIElement)itemsControl2.ItemContainerGenerator.ContainerFromIndex(index);
            if (uIElement == null)
            {
                return;
            }

            Rect rect = new Rect(uIElement.TranslatePoint(default(Point), base.AdornedElement), uIElement.RenderSize);
            double num = 0.0;
            Point point;
            Point point2;
            if (base.DropInfo.VisualTargetOrientation == Orientation.Vertical)
            {
                if (base.DropInfo.InsertIndex == itemsControl2.Items.Count)
                {
                    rect.Y += uIElement.RenderSize.Height;
                }

                point = new Point(rect.X, rect.Y);
                point2 = new Point(rect.Right, rect.Y);
            }
            else
            {
                if (base.DropInfo.InsertIndex == itemsControl2.Items.Count)
                {
                    rect.X += uIElement.RenderSize.Width;
                }

                point = new Point(rect.X, rect.Y);
                point2 = new Point(rect.X, rect.Bottom);
                num = 90.0;
            }

            drawingContext.DrawLine(m_Pen, point, point2);
            DrawTriangle(drawingContext, point, num);
            DrawTriangle(drawingContext, point2, 180.0 + num);
        }

        private void DrawTriangle(DrawingContext drawingContext, Point origin, double rotation)
        {
            drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
            drawingContext.PushTransform(new RotateTransform(rotation));
            drawingContext.DrawGeometry(m_Pen.Brush, null, m_Triangle);
            drawingContext.Pop();
            drawingContext.Pop();
        }

        static DropTargetInsertionAdorner()
        {
            m_Pen = new Pen(Brushes.Gray, 2.0);
            m_Pen.Freeze();
            LineSegment lineSegment = new LineSegment(new Point(0.0, -3.0), isStroked: false);
            lineSegment.Freeze();
            LineSegment lineSegment2 = new LineSegment(new Point(0.0, 3.0), isStroked: false);
            lineSegment2.Freeze();
            PathFigure pathFigure = new PathFigure
            {
                StartPoint = new Point(3.0, 0.0)
            };
            pathFigure.Segments.Add(lineSegment);
            pathFigure.Segments.Add(lineSegment2);
            pathFigure.Freeze();
            m_Triangle = new PathGeometry();
            m_Triangle.Figures.Add(pathFigure);
            m_Triangle.Freeze();
        }
    }
}
