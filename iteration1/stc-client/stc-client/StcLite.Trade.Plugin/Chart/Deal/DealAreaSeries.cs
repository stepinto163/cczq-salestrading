using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Price.Plugin.Chart.Deal
{
    public class DealAreaSeries : AreaSeries
    {
        public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
        {
            if (CommonUtils.IsFutureBond(this.PlotModel.TitleToolTip))
            {
                var dpn_X = DateTimeAxis.ToDateTime(XAxis.InverseTransform(point.X)).ToString("HH:mm:ss");
                if (this.XAxisKey == "x1" && (dpn_X.CompareTo("09:00:00") < 0 || (dpn_X.CompareTo("11:30:00") > 0)))
                {
                    Console.WriteLine("null");
                    return null;
                }
                else if (this.XAxisKey == "x2" && (dpn_X.CompareTo("13:00:00") < 0 || (dpn_X.CompareTo("15:00:00") > 0)))
                {
                    Console.WriteLine("null");
                    return null;
                }
                else if (this.XAxisKey == "x3" && (dpn_X.CompareTo("15:00:00") < 0 || (dpn_X.CompareTo("15:15:00") > 0)))
                {
                    Console.WriteLine("null");
                    return null;
                }
            }
            

            return base.GetNearestPoint(point, interpolate);
        }
    }
}
