namespace STC.Common.Model.RFQ
{
    /// <summary>
    /// 通过该接口查询Taker发出的询价请求，传入查询条件为按条件查询，不传条件默认查询当天所有
    /// </summary>
    public class QueryRfqOrderReq
    {
        /// <summary>
        /// STC用户名称
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// Taker发出的请求ID
        /// </summary>
        public string quoteReqId { get; set; }

        /// <summary>
        /// 债券状态
        /// </summary>
        public string quoteStatus { get; set; }
        /// <summary>
        /// 债券简称
        /// </summary>
        public string symbol { get; set; }
        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityid { get; set; }

        /// <summary>
        /// 交易渠道
        /// </summary>
        public string venues { get; set; }
        /// <summary>
        /// 交易子渠道
        /// </summary>
        public string venuetypegw { get; set; }
        /// <summary>
        /// 历史查询开始时间,yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string queryTimeStart { get; set; }

        /// <summary>
        /// 历史查询结束时间yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string queryTimeEnd { get; set; }



    }
}
