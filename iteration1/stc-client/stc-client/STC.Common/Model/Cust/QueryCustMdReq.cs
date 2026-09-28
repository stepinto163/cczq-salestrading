using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cust
{
    public class QueryCustMdReq
    {
        /// <summary>
        /// 通过标签类型和标签值查询，key=标签类型 Value=标签值 list
        /// </summary>
        public ConcurrentDictionary<string, List<string>> reqTags;

        /// <summary>
        /// 通过属性值查询，可以逗号分割传多个，可以模糊匹配查询
        /// "SecurityID", "Symbol", "SecurityType", "MarketIndicator"
        /// </summary>
        public Dictionary<string, string> reqParams;

        /// <summary>
        /// extParam存的是区间查询 "TermDaysRange", "CouponRateCurrentRange", "PriceRange", "YieldRange", "ChinaBondValuationRange", "BondType", "SettlType"
        /// </summary>
        public Dictionary<string, string> extParams;

        public string couponRangeStr;

        /// <summary>
        /// 默认从 1开始
        /// </summary>
        public int pageNum;

        /// <summary>
        /// 默认 5000
        /// </summary>
        public int pageSize;

        /// <summary>
        /// 用户名
        /// </summary>
        public string userName;

        /// <summary>
        /// 额外字段
        /// </summary>
        public string info1;
    }
}
