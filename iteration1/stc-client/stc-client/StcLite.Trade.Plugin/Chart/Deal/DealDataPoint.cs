using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Price.Plugin.Chart.Deal
{
    public class DealDataPoint : IDataPointProvider
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Side { get; set; }
        public string Id { get; set; }

        public DealDataPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(X, Y);
        }
    }
}