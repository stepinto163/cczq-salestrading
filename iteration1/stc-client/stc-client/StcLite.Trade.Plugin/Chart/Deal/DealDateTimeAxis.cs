using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BDB.Price.Plugin.Chart.Deal
{
    public class DealDateTimeAxis : DateTimeAxis
    {
        private bool _firstLabelIsEmpty;

        public bool FirstLabelIsEmpty
        {
            get { return _firstLabelIsEmpty; }
            set { _firstLabelIsEmpty = value; }
        }

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
            majorLabelValues = majorTickValues.ToList();

            minorTickValues = AxisUtilities.FilterRedundantMinorTicks(majorTickValues, minorTickValues);

            if (FirstLabelIsEmpty)
            {
                if (majorLabelValues.Count > 0)
                {
                    majorLabelValues[0] = 0;
                }
            }
        }
    }
}
