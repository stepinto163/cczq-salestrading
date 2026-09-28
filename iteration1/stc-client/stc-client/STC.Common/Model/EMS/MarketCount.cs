using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stc.Common.Model.EMS
{
    public class MarketCount
    {
        public string CloseTime { get; set; }
        public string CloseYield { get; set; }
        public string HighTime { get; set; }

        public string HighYield { get; set; }

        public string LowTime { get; set; }
        public string LowYield { get; set; }
        public string OpenTime { get; set; }

        public string OpenYld { get; set; }
        public decimal TotalNumTrades { get; set; }
    }
}
