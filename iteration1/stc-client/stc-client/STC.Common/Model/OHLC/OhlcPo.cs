using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.OHLC
{
    public class OhlcPo
    {
        public decimal Close { set; get; }
        public string FmtTime { set; get; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Open { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }

        public string TradeDate { get; set; }

        public string SecurityID { get; set; }
    }
}
