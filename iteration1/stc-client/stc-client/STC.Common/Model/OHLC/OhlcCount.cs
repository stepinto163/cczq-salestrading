using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.OHLC
{
    public class OhlcMsg
    {
        public string Topic { get; set; }
        public string Data { get; set; }
        public string SendTime { get; set; }
        //topic:stc.ohlc.ems.e.210408.IB.1D, body:{"close":3.18,"closetime":"2021-10-26 14:06:45.317","fmttime":"20211026",
        //"high":3.18,"hightime":"2021-10-26 14:06:45.317","inf1":"1","inf2":"3.19500000","low":3.18,"lowtime":"2021-10-26 14:06:45.317",
        //"open":3.18,"opentime":"2021-10-26 14:06:45.317","securityid":"210408.IB","text":"1D","tradedate":"","type":"e","venue":"ems"} 
        public string venue { get; set; }
        public string securityid { get; set; }

        /// <summary>
        /// 收
        /// </summary>
        public string close { get; set; }
        public string closeNetPrice { get; set; }
        public string closetime { get; set; }
        
        /// <summary>
        /// 高
        /// </summary>
        public decimal high { get; set; }
        public decimal highNetPrice { get; set; }
        public string hightime { get; set; }
        /// <summary>
        /// 量
        /// </summary>
        public string inf1 { get; set; }
        /// <summary>
        /// 昨收 3.19500000
        /// </summary>
        public string inf2 { get; set; }
        /// <summary>
        /// 昨收 净价
        /// </summary>
        public string inf3 { get; set; }

        /// <summary>
        /// 低
        /// </summary>
        public decimal low { get; set; }
        public decimal lowNetPrice { get; set; }
        public string lowtime { get; set; }

        /// <summary>
        /// 开
        /// </summary>
        public decimal open { get; set; }
        public decimal openNetPrice { get; set; }
        public string opentime { get; set; }

        /// <summary>
        /// 行情类型 1D 日线 1M 1分钟 5M 5分钟 ...
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 交易日期 20211026
        /// </summary>
        public string fmttime { get; set; }

        public string tradedate { get; set; }
        public decimal numTrades { get; set; }
    }
}
