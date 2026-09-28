using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    /// <summary>
    /// 估值
    /// </summary>
    public class ValuationInfo
    {
        /// <summary>
        /// 券码
        /// </summary>
        public string BondCode;
        /// <summary>
        /// 债券ID
        /// </summary>
        public string BondID;
        /// <summary>
        /// 债券所属市场
        /// </summary>
        public string Bondkeylistedmarket;
        /// <summary>
        /// 市场列表'CIB：yinhangjian SSE：shanghai SZE：shenzhen
        /// </summary>
        public string ListedMarket;
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Id;
        /// <summary>
        /// 凸性值
        /// </summary>
        public decimal ValConvexity;
        /// <summary>
        /// 修改间隔
        /// </summary>
        public decimal ValModifiedDuration;
        /// <summary>
        /// 修改时间
        /// </summary>
        public string ModifyTime;
        /// <summary>
        /// 状态位:1-有效 2-无效
        /// </summary>
        public string Status;
        /// <summary>
        /// 估值日期
        /// </summary>
        public string Valuationdate;
        /// <summary>
        /// 净价
        /// </summary>
        public string Valuationnetprice;
        /// <summary>
        /// 估值利率
        /// </summary>
        public string valuation_yield { get; set; }
        /// <summary>
        /// 基准点
        /// </summary>
        public decimal ValBasisPointValue;

    }
}
