using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Price.Plugin.Chart.Deal
{
    internal class DealHorizontalAndVerticalAxisRenderer : HorizontalAndVerticalAxisRenderer
    {
        public DealHorizontalAndVerticalAxisRenderer(IRenderContext rc, PlotModel plot) : base(rc, plot)
        {
        }

        protected override void RenderMajorItems(Axis axis, double axisPosition, double titlePosition, bool drawAxisLine)
        {
            double eps = axis.ActualMinorStep * 1e-3;

            double clipMinimum = axis.ClipMinimum;
            double clipMaximum = axis.ClipMaximum;

            double plotAreaLeft = this.Plot.PlotArea.Left;
            double plotAreaRight = this.Plot.PlotArea.Right;
            double plotAreaTop = this.Plot.PlotArea.Top;
            double plotAreaBottom = this.Plot.PlotArea.Bottom;
            bool isHorizontal = axis.IsHorizontal();
            bool cropGridlines = axis.CropGridlines;

            double a0;
            double a1;
            var majorSegments = new List<ScreenPoint>();
            var majorTickSegments = new List<ScreenPoint>();
            this.GetTickPositions(axis, axis.TickStyle, axis.MajorTickSize, axis.Position, out a0, out a1);

            var perpendicularAxis = axis.IsHorizontal() ? this.Plot.DefaultYAxis : this.Plot.DefaultXAxis;
            var dontRenderZero = axis.PositionAtZeroCrossing && perpendicularAxis.PositionAtZeroCrossing;

            List<Axis> perpAxes = null;
            if (cropGridlines)
            {
                if (isHorizontal)
                {
                    perpAxes = this.Plot.Axes.Where(x => x.IsXyAxis() && x.IsVertical()).ToList();
                }
                else
                {
                    perpAxes = this.Plot.Axes.Where(x => x.IsXyAxis() && x.IsHorizontal()).ToList();
                }
            }

            foreach (double value in this.MajorTickValues)
            {
                if (dontRenderZero && Math.Abs(value) < eps)
                {
                    continue;
                }

                double transformedValue = axis.Transform(value);
                if (isHorizontal)
                {
                    SnapTo(plotAreaLeft, ref transformedValue);
                    SnapTo(plotAreaRight, ref transformedValue);
                }
                else
                {
                    SnapTo(plotAreaTop, ref transformedValue);
                    SnapTo(plotAreaBottom, ref transformedValue);
                }

                if (this.MajorPen != null)
                {
                    this.AddSegments(
                        majorSegments,
                        perpAxes,
                        isHorizontal,
                        cropGridlines,
                        transformedValue,
                        plotAreaLeft,
                        plotAreaRight,
                        plotAreaTop,
                        plotAreaBottom);
                }

                if (axis.TickStyle != TickStyle.None && axis.MajorTickSize > 0)
                {
                    if (isHorizontal)
                    {
                        majorTickSegments.Add(new ScreenPoint(transformedValue, axisPosition + a0));
                        majorTickSegments.Add(new ScreenPoint(transformedValue, axisPosition + a1));
                    }
                    else
                    {
                        majorTickSegments.Add(new ScreenPoint(axisPosition + a0, transformedValue));
                        majorTickSegments.Add(new ScreenPoint(axisPosition + a1, transformedValue));
                    }
                }
            }

            // Render the axis labels (numbers or category names)
            foreach (double value in this.MajorLabelValues)
            {
                if (value < clipMinimum - eps || value > clipMaximum + eps)
                {
                    continue;
                }

                if (dontRenderZero && Math.Abs(value) < eps)
                {
                    continue;
                }

                double transformedValue = axis.Transform(value);
                if (isHorizontal)
                {
                    SnapTo(plotAreaLeft, ref transformedValue);
                    SnapTo(plotAreaRight, ref transformedValue);
                }
                else
                {
                    SnapTo(plotAreaTop, ref transformedValue);
                    SnapTo(plotAreaBottom, ref transformedValue);
                }

                var pt = new ScreenPoint();
                var ha = HorizontalAlignment.Right;
                var va = VerticalAlignment.Middle;
                switch (axis.Position)
                {
                    case AxisPosition.Left:
                        //pt = new ScreenPoint(axisPosition + a1 - axis.AxisTickToLabelDistance, transformedValue);
                        pt = new ScreenPoint(axisPosition - axis.AxisTickToLabelDistance, transformedValue - 4);
                        this.GetRotatedAlignments(axis.Angle, -90, out ha, out va);

                        break;
                    case AxisPosition.Right:
                        pt = new ScreenPoint(axisPosition + a1 + axis.AxisTickToLabelDistance, transformedValue);
                        this.GetRotatedAlignments(axis.Angle, 90, out ha, out va);

                        break;
                    case AxisPosition.Top:
                        pt = new ScreenPoint(transformedValue, axisPosition + a1 - axis.AxisTickToLabelDistance);
                        this.GetRotatedAlignments(axis.Angle, 0, out ha, out va);

                        break;
                    case AxisPosition.Bottom:
                        pt = new ScreenPoint(transformedValue, axisPosition + a1 + axis.AxisTickToLabelDistance);
                        this.GetRotatedAlignments(axis.Angle, -180, out ha, out va);

                        break;
                }

                string text = axis.FormatValue(value);
                if (axis is CustomLinearAxis)
                {
                    CustomLinearAxis cLinearAxis = axis as CustomLinearAxis;
                    this.RenderContext.DrawMathText(
                    pt,
                    text,
                    cLinearAxis.GetActualTextColor(),
                    cLinearAxis.GetActualFont(),
                    cLinearAxis.GetActualFontSize(),
                    cLinearAxis.GetActualFontWeight(),
                    cLinearAxis.Angle,
                    ha,
                    va);
                }
            }

            // Draw extra grid lines
            if (axis.ExtraGridlines != null && this.ExtraPen != null)
            {
                var extraSegments = new List<ScreenPoint>();

                foreach (double value in axis.ExtraGridlines)
                {
                    if (!this.IsWithin(value, clipMinimum, clipMaximum))
                    {
                        continue;
                    }

                    double transformedValue = axis.Transform(value);
                    this.AddSegments(
                        extraSegments,
                        perpAxes,
                        isHorizontal,
                        cropGridlines,
                        transformedValue,
                        plotAreaLeft,
                        plotAreaRight,
                        plotAreaTop,
                        plotAreaBottom);
                }

                this.RenderContext.DrawLineSegments(extraSegments, this.ExtraPen, axis.EdgeRenderingMode.GetActual(EdgeRenderingMode.PreferSharpness));
            }

            if (drawAxisLine)
            {
                // Draw the axis line (across the tick marks)
                if (isHorizontal)
                {
                    this.RenderContext.DrawLine(
                        axis.Transform(clipMinimum),
                        axisPosition,
                        axis.Transform(clipMaximum),
                        axisPosition,
                        this.AxislinePen,
                        axis.EdgeRenderingMode.GetActual(EdgeRenderingMode.PreferSharpness));
                }
                else
                {
                    this.RenderContext.DrawLine(
                        axisPosition,
                        axis.Transform(clipMinimum),
                        axisPosition,
                        axis.Transform(clipMaximum),
                        this.AxislinePen,
                        axis.EdgeRenderingMode.GetActual(EdgeRenderingMode.PreferSharpness));
                }
            }

            if (this.MajorPen != null)
            {
                this.RenderContext.DrawLineSegments(majorSegments, this.MajorPen, axis.EdgeRenderingMode.GetActual(EdgeRenderingMode.PreferSharpness));
            }

            if (this.MajorTickPen != null)
            {
                this.RenderContext.DrawLineSegments(majorTickSegments, this.MajorTickPen, axis.EdgeRenderingMode.GetActual(EdgeRenderingMode.PreferSharpness));
            }
        }

        /// <summary>
        /// Adds segments to <paramref name="segments"/> array. 
        /// If <paramref name="cropGridlines"/> is true, then lines will be cropped with <paramref name="perpAxes"/> lists axes.
        /// </summary>
        /// <param name="segments">The target segments.</param>
        /// <param name="perpAxes">Perpendicular axes list.</param>
        /// <param name="isHorizontal">True, if current axis is horizontal.</param>
        /// <param name="cropGridlines">True, if gridlines should be cropped.</param>
        /// <param name="transformedValue">Starting point position.</param>
        /// <param name="plotAreaLeft">Plot area left position.</param>
        /// <param name="plotAreaRight">Plot area right position.</param>
        /// <param name="plotAreaTop">Plot area top position.</param>
        /// <param name="plotAreaBottom">Plot area bottom position.</param>
        private void AddSegments(
            List<ScreenPoint> segments,
            List<Axis> perpAxes,
            bool isHorizontal,
            bool cropGridlines,
            double transformedValue,
            double plotAreaLeft,
            double plotAreaRight,
            double plotAreaTop,
            double plotAreaBottom)
        {
            if (isHorizontal)
            {
                if (!cropGridlines)
                {
                    segments.Add(new ScreenPoint(transformedValue, plotAreaTop));
                    segments.Add(new ScreenPoint(transformedValue, plotAreaBottom));
                }
                else
                {
                    foreach (var perpAxis in perpAxes)
                    {
                        segments.Add(new ScreenPoint(transformedValue, perpAxis.Transform(perpAxis.ClipMinimum)));
                        segments.Add(new ScreenPoint(transformedValue, perpAxis.Transform(perpAxis.ClipMaximum)));
                    }
                }
            }
            else
            {
                if (!cropGridlines)
                {
                    segments.Add(new ScreenPoint(plotAreaLeft, transformedValue));
                    segments.Add(new ScreenPoint(plotAreaRight, transformedValue));
                }
                else
                {
                    foreach (var perpAxis in perpAxes)
                    {
                        segments.Add(new ScreenPoint(perpAxis.Transform(perpAxis.ClipMinimum), transformedValue));
                        segments.Add(new ScreenPoint(perpAxis.Transform(perpAxis.ClipMaximum), transformedValue));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the alignments given the specified rotation angle.
        /// </summary>
        /// <param name="boxAngle">The angle of a box to rotate (usually it is label angle).</param>
        /// <param name="axisAngle">
        /// The axis angle, the original angle belongs to. The Top axis should have 0, next angles are computed clockwise. 
        /// The angle should be in [-180, 180). (T, R, B, L) is (0, 90, -180, -90). 
        /// </param>
        /// <param name="ha">Horizontal alignment.</param>
        /// <param name="va">Vertical alignment.</param>
        /// <remarks>
        /// This method is supposed to compute the alignment of the labels that are put near axis. 
        /// Because such labels can have different angles, and the axis can have different angles as well,
        /// computing the alignment is not straightforward.
        /// </remarks>
        private void GetRotatedAlignments(
            double boxAngle,
            double axisAngle,
            out HorizontalAlignment ha,
            out VerticalAlignment va)
        {
            const double AngleTolerance = 10.0;

            Debug.Assert(new[] { 0.0, 90.0, -180.0, -90.0 }.Contains(axisAngle), "The axis angles should be one of 0, 90, -180, -90");

            // The axis angle if it would have been turned on 180 and leave it in [-180, 180)
            double flippedAxisAngle = ((axisAngle + 360.0) % 360.0) - 180.0;

            // When the box (assuming the axis and box have the same angle) box starts to turn clockwise near the axis
            // It leans on the right until it gets to 180 rotation, when it is started to lean on the left.
            // In real computation we need to compute this in relation with axisAngle
            // So if axisAngle <= boxAngle < (axisAngle + 180), we align Right, else - left.
            // The check looks inverted because flippedAxisAngle has the opposite sign.
            ha = boxAngle >= Math.Min(axisAngle, flippedAxisAngle) && boxAngle < Math.Max(axisAngle, flippedAxisAngle) ? HorizontalAlignment.Left : HorizontalAlignment.Right;

            // If axisAngle was < 0, we need to shift the previous computation on 180.
            if (axisAngle < 0)
            {
                ha = (HorizontalAlignment)((int)ha * -1);
            }

            va = VerticalAlignment.Middle;

            // If the angle almost the same as axisAngle (or axisAngle + 180) - set horizontal alignment to Center
            if (Math.Abs(boxAngle - flippedAxisAngle) < AngleTolerance || Math.Abs(boxAngle - axisAngle) < AngleTolerance)
            {
                ha = HorizontalAlignment.Center;
            }

            // And vertical alignment according to whether it is near to axisAngle or flippedAxisAngle
            if (Math.Abs(boxAngle - axisAngle) < AngleTolerance)
            {
                va = VerticalAlignment.Bottom;
            }

            if (Math.Abs(boxAngle - flippedAxisAngle) < AngleTolerance)
            {
                va = VerticalAlignment.Top;
            }
        }
    }
}
