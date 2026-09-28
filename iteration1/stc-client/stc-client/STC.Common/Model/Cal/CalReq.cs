using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cal
{
    /// <summary>
    /// 计算净价、收益率请求参数
    /// </summary>
    public class CalReq
    {
        public string SecurityID { get; set; }
        /// <summary>
        /// Accrued("1"),CleanPrice("2"),FullPrice("3"),Yield("4"),Spread("5"),SpreadAndPredictRate("6"),yieldToExecution("7");
        /// </summary>
        public string CalType { get; set; }
        public string SettlementDate { get; set; }
        public string Value { get; set; }
        public string SettlType { get; set; }
        public string Tradedate { get; set; }
        /// <summary>
        /// 1:bond,2:cfets
        /// </summary>
        public string CalPath { get; set; }
        public int Scale = 4;
    }
}
