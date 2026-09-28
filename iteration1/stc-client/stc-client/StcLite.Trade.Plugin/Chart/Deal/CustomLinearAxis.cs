using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Price.Plugin.Chart.Deal
{
    public class CustomLinearAxis : LinearAxis
    {
        /// <summary>
        /// Gets the coordinates used to draw ticks and tick labels (numbers or category names).
        /// </summary>
        /// <param name="majorLabelValues">The major label values.</param>
        /// <param name="majorTickValues">The major tick values.</param>
        /// <param name="minorTickValues">The minor tick values.</param>
        public override void GetTickValues(
            out IList<double> majorLabelValues, out IList<double> majorTickValues, out IList<double> minorTickValues)
        {
            minorTickValues = OxyPlotUtil.CreateTickValues_Custom(this.ClipMinimum, this.ClipMaximum, this.ActualMinorStep);
            majorTickValues = OxyPlotUtil.CreateTickValues_Custom(this.ClipMinimum, this.ClipMaximum, this.ActualMajorStep);
            majorLabelValues = majorTickValues;

            minorTickValues = AxisUtilities.FilterRedundantMinorTicks(majorTickValues, minorTickValues);
        }

        public override void Render(IRenderContext rc, int pass)
        {
            if (this.Position == AxisPosition.None)
            {
                return;
            }

            var r = new DealHorizontalAndVerticalAxisRenderer(rc, this.PlotModel);
            r.Render(this, pass);
        }

        public OxyColor GetActualTextColor()
        {
            return this.ActualTextColor;
        }

        public string GetActualFont()
        {
            return this.ActualFont;
        }

        public double GetActualFontSize()
        {
            return this.ActualFontSize;
        }

        public double GetActualFontWeight()
        {
            return this.ActualFontWeight;
        }
    }
}
