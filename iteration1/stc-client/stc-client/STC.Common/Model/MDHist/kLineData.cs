using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MDHist
{
    public class kLineData
    {
        public string CloseTime { get; set; }
        public string FmtTime { get; set; }
        public string SecurityId { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public string Open { get; set; }
        public string High { get; set; }
        public string Close { get; set; }
        public string Low { get; set; }
        public string TradeDate { get; set; }
        public string NumTrades { get; set; }
    }
}
