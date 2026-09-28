using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BDB.Price.Plugin.Chart.Deal
{
    public class DealCanvasRenderContext : CanvasRenderContext
    {
        /// <summary>
        /// The brush cache.
        /// </summary>
        private readonly Dictionary<OxyColor, Brush> linearGradientBrushCache = new Dictionary<OxyColor, Brush>();

        public DealCanvasRenderContext(Canvas canvas) : base(canvas)
        {

        }

        LinearGradientBrush lbrush = new LinearGradientBrush(
                Color.FromArgb(255, 255, 165, 0),
                //Color.FromArgb(20, 255, 235, 200),
                Color.FromArgb(255, 0, 165, 0),
                new Point(0, 0),
                new Point(0, 1));

        /////<inheritdoc/>
        //public override void DrawRectangles(IList<OxyRect> rectangles, OxyColor fill, OxyColor stroke, double thickness, EdgeRenderingMode edgeRenderingMode)
        //{
        //    if (rectangles.Count == 0)
        //    {
        //        return;
        //    }

        //    var path = this.CreateAndAdd<Path>();
        //    this.SetStroke(path, stroke, thickness, edgeRenderingMode);
        //    if (!fill.IsUndefined())
        //    {
        //        path.Fill = lbrush;// GetCachedLinearGradientBrush(fill);
        //    }

        //    var streamGeometry = new StreamGeometry { FillRule = FillRule.Nonzero };
        //    using (var context = streamGeometry.Open())
        //    {
        //        foreach (var rect in rectangles)
        //        {
        //            var r = this.GetActualRect(rect, thickness, edgeRenderingMode);
        //            context.BeginFigure(r.TopLeft, !fill.IsUndefined(), true);
        //            context.PolyLineTo(new[] { r.TopRight, r.BottomRight, r.BottomLeft }, !stroke.IsUndefined(), false);
        //        }
        //    }

        //    streamGeometry.Freeze();
        //    path.Data = streamGeometry;
        //}


        public override void DrawPolygons(
            IList<IList<ScreenPoint>> polygons,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            EdgeRenderingMode edgeRenderingMode,
            double[] dashArray,
            LineJoin lineJoin)
        {
            if (polygons.Count == 0)
            {
                return;
            }

            var path = this.CreateAndAdd<Path>();
            this.SetStroke(path, stroke, thickness, edgeRenderingMode, lineJoin, dashArray, 0);
            if (!fill.IsUndefined())
            {
                path.Fill = GetCachedLinearGradientBrush(fill); GetCachedBrush(fill);
            }
            

            var streamGeometry = new StreamGeometry { FillRule = FillRule.Nonzero };
            using (StreamGeometryContext sgc = streamGeometry.Open())
            {
                foreach (var polygon in polygons)
                {
                    var points = this.GetActualPoints(polygon, path.StrokeThickness, edgeRenderingMode);
                    IList<Point> otherPoints;
                    var firstPoint = GetFirstAndRest(points, out otherPoints);

                    sgc.BeginFigure(firstPoint, !fill.IsUndefined(), true);
                    if (otherPoints != null)
                    {
                        foreach (var point in otherPoints)
                        {
                            sgc.LineTo(point, !stroke.IsUndefined(), false);
                        }
                    }
                }
            }

            streamGeometry.Freeze();
            path.Data = streamGeometry;
        }

        private static T GetFirstAndRest<T>(IEnumerable<T> items, out IList<T> rest)
        {
            using (var e = items.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    rest = new T[0];
                    return default(T);
                }

                var ret = e.Current;

                rest = new List<T>();
                while (e.MoveNext())
                {
                    rest.Add(e.Current);
                }

                return ret;
            }
        }

        /// <summary>
        /// Gets the cached Linear Gradient brush.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The brush.</returns>
        protected Brush GetCachedLinearGradientBrush(OxyColor color)
        {
            if (color.A == 0)
            {
                return null;
            }

            Brush brush;
            if (!this.linearGradientBrushCache.TryGetValue(color, out brush))
            {
                brush = new LinearGradientBrush(
                    Color.FromArgb(100, color.R, color.G, color.B),
                    Color.FromArgb(10, color.R, color.G, color.B),
                    new Point(0, 0),
                    new Point(0, 1));
                brush.Freeze();
                this.linearGradientBrushCache.Add(color, brush);
            }

            return brush;
        }
    }
}
