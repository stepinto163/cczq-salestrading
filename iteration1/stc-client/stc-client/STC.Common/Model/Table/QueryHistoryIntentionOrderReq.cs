using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryHistoryIntentionOrderReq
    {
        /// <summary>
        /// 查询日期,示例:"2022-12-20"
        /// </summary>
        public string date;

        /// <summary>
        /// 状态  "0"(待处理) 1"(已处理)"2”(已成交) "3"(已忽略) "4"("己 REF)
        /// </summary>
        public string status;

        /// <summary>
        /// 用户名
        /// </summary>
        public string userName;

        /// <summary>
        /// 0001销交，0010做市
        /// </summary>
        public string roletype;

        /// <summary>
        /// 开始日期,示例:"2022-12-20"
        /// </summary>
        public string startdate;

        /// <summary>
        /// 结束日期,示例:"2022-12-20"
        /// </summary>
        public string enddate;

        /// <summary>
        /// 默认0
        /// </summary>
        public int pageNum;

        /// <summary>
        /// 默认5000
        /// </summary>
        public int pageSize;

        /// <summary>
        /// 4,2
        /// </summary>
        public string marketindicator;

        /// <summary>
        /// 分组id
        /// </summary>
        public string biztypeid;

        /// <summary>
        /// 客户端使用
        /// </summary>
        public string queryGuid;

        /// <summary>
        /// 一。买方机构
        ///   个需：query 传quoteType传BUYER_INTENTION
        ///   私域：query 传quoteType传DEMAND
        /// 二。 卖方机构
        ///   个需：query 传quoteType传INTENTION
        ///   私域：query 传quoteType传DEMAND
        /// </summary>
        public string quoteType;
    }
}
