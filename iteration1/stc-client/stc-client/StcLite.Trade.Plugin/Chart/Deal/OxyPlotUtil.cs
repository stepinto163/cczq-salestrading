using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Price.Plugin.Chart.Deal
{
    public static class OxyPlotUtil
    {

        /// <summary>
        /// Creates tick values at the specified interval.
        /// </summary>
        /// <param name="from">The start value.</param>
        /// <param name="to">The end value.</param>
        /// <param name="step">The interval.</param>
        /// <param name="maxTicks">The maximum number of ticks (optional). The default value is 1000.</param>
        /// <returns>A sequence of values.</returns>
        /// <exception cref="System.ArgumentException">Step cannot be zero or negative.;step</exception>
        public static IList<double> CreateTickValues_Custom(double from, double to, double step, int maxTicks = 1000)
        {
            if (step <= 0)
            {
                throw new ArgumentOutOfRangeException("Step cannot be zero or negative.", nameof(step));
            }

            if (to < from)
            {
                step = -step;
            }

            //var epsilon = step * 1e-3;
            //from -= epsilon;
            //to += epsilon;

            //var startValue = Math.Ceiling(from / step) * step;
            //if (startValue == -0d)
            //{
            //    startValue = 0;
            //}
            var startValue = from;

            var values = new List<double>();
            var sign = Math.Sign(step);
            var i = 0;

            var currentValue = startValue;
            while ((to - currentValue) * sign >= 0 && i < maxTicks)
            {
                values.Add(currentValue);
                currentValue = startValue + (++i * step);
            }

            return values;
        }
    }
}
